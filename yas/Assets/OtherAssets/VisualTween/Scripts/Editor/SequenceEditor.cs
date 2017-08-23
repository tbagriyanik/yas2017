﻿using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class SequenceEditor : EditorWindow {
	private Timeline timeline;
	private GameObject selectedGameObject;
	private Tweener tweener;
	private Sequence sequence{
		get{
			if(tweener != null && selectedSequenceIndex < tweener.sequences.Count){
				return tweener.sequences[selectedSequenceIndex];
			}
			return null;
		}
	}
	private int selectedNodeIndex;

	private SequenceNode selectedNode{
		get{
			if(sequence != null && selectedNodeIndex <sequence.nodes.Count){
				return sequence.nodes[selectedNodeIndex];
			}
			return null;
		}
	}


	private bool isPlaying;
	private bool isRecording;
	private float playStartTime;
	private bool resizeNodeStart;
	private bool resizeNodeEnd;
	private bool dragNode;
	private float timeClickOffset;
	private Vector2 settingsScroll;
	private GameObject backupGameObject;
	private int selectedEventIndex;
	private EventNode selectedEvent{
		get{
			if(sequence != null && selectedEventIndex <sequence.events.Count){
				return sequence.events[selectedEventIndex];
			}
			return null;
		}
	}
	[MenuItem("Window/Zerano Assets/Visual Tween", false)]
	public static void ShowWindow()
	{
		SequenceEditor window = EditorWindow.GetWindow<SequenceEditor>(false, "Tweener");
		window.wantsMouseMove = true;
		UnityEngine.Object.DontDestroyOnLoad(window);
	}
	
	private void OnEnable(){
		if (timeline == null) {
			timeline = new Timeline ();
		}
		timeline.onRecord = OnRecord;
		timeline.onPlay = OnPlay;
		timeline.onSettingsGUI = OnSettings;
		timeline.onTimelineGUI = DrawNodes;
		timeline.onTimelineClick = OnTimelineClick;
		timeline.onAddEvent = OnAddEvent;
		timeline.onEventGUI = OnEventGUI;
		selectedEventIndex = int.MaxValue;
		selectedNodeIndex = int.MaxValue;
		if(selectedGameObject == null)
		OnSelectionChange ();

		EditorApplication.playmodeStateChanged += OnPlayModeStateChange;
	}

	private void OnAddEvent(){
		AddTweener (Selection.activeGameObject);
		if (sequence == null) {
			AddSequence (tweener);
		}
		if (sequence.events == null) {
			sequence.events= new List<EventNode>();
		}
		GenericMenu menu = new GenericMenu ();
		//Component[] components=selectedGameObject.GetComponents<Component>();
		List<Type> types = new List<Type> ();
		//types.AddRange (components.Select (x => x.GetType ()));
		types.AddRange (GetSupportedTypes ());
		foreach (Type type in types) {
			List<MethodInfo> functions= GetValidFunctions(type,!(type.IsSubclassOf(typeof(Component)) || type.IsSubclassOf(typeof(MonoBehaviour))) || selectedGameObject.GetComponent(type)==null);
			foreach(MethodInfo mi in functions){
				if(mi != null){
					EventNode node = new EventNode ();
					node.time = timeline.CurrentTime;
					node.SerializedType=type;
					node.method=mi.Name;
					node.arguments=GetMethodArguments(mi);
					menu.AddItem(new GUIContent(type.Name+"/"+mi.Name),false,AddEvent,node);
				}
			}
		}
		menu.ShowAsContext ();
	}


	public List<MethodInfo> GetValidFunctions(Type type,bool staticOnly){
		List<MethodInfo> validMethods = new List<MethodInfo> ();
		List<MethodInfo> mMethods=type.GetMethods(BindingFlags.Public|BindingFlags.Static).ToList();
		if (!staticOnly) {
			mMethods.AddRange (type.GetMethods (BindingFlags.Public | BindingFlags.Instance));
		}
		for (int b = 0; b < mMethods.Count; ++b) {
			MethodInfo mi = mMethods [b];
			
			string name = mi.Name;
			if (name == "Invoke")continue;
			if (name == "InvokeRepeating")continue;
			if (name == "CancelInvoke")continue;
			if (name == "StopCoroutine")continue;
			if (name == "StopAllCoroutines")continue;
			if (name.StartsWith ("get_"))continue;
			if (mi.ReturnType != typeof(void))continue;
			if(mi.IsGenericMethod)continue;
			if(!IsArgumentValid(mi))continue;
			
			validMethods.Add(mi);
		}
		return validMethods;
	}

	private bool isArgumentValid(Type type){
		switch(type.ToString())
		{
		case "System.Int16":
		case "System.Int32":
		case "System.Int64":
		case "System.Single":
		case "System.Double":
		case "System.String":
		case "System.Boolean":
		case "UnityEngine.Vector2":
		case "UnityEngine.Vector3":
		case "UnityEngine.Vector4":
		case "UnityEngine.Color":
		case "UnityEngine.Rect":
		case "UnityEngine.AnimationCurve":
		case "UnityEngine.Space":
		case "UnityEngine.Quaternion":
		case "UnityEngine.SendMessageOptions":
			return true;
		default:
			if(type==typeof(UnityEngine.Object) || type.IsSubclassOf( typeof(UnityEngine.Object) ) )
			{
				return true;
			}
			return false;
		}
	}
	
	private bool IsArgumentValid(MethodInfo mi){
		ParameterInfo[] info = mi.GetParameters();
		foreach(ParameterInfo p in info){
			return isArgumentValid(p.ParameterType);
		}
		
		return true;
	}

	private List<MethodArgument> GetMethodArguments(MethodInfo mi){
		ParameterInfo[] pi = mi.GetParameters ();
		List<MethodArgument> args = new List<MethodArgument> ();
		foreach (ParameterInfo info in pi) {
			MethodArgument arg= new MethodArgument(info.Name,info.ParameterType.ToString());
			args.Add(arg);
		}
		return args;
	}

	public List<Type> GetSupportedTypes(){
		List<Type> types = new List<Type> ();
		/*types.Add(typeof(Application));
		types.Add (typeof(GameObject));
		types.Add (typeof(Debug));*/

		types.AddRange(AppDomain.CurrentDomain.GetAssemblies()
		               .SelectMany(t => t.GetTypes())
		               .Where(t => t.Namespace == "UnityEngine"));

		return types;
	}

	private void AddEvent(object data){
		EventNode node = data as EventNode;
		sequence.events.Add (node);
		EditorUtility.SetDirty (tweener);
	}

	private void OnEventGUI(Rect rect){
		if (sequence != null) {
			for(int i=0;i< sequence.events.Count;i++){
				Rect rect1=new Rect(timeline.SecondsToGUI(sequence.events[i].time)-timeline.scroll.x+rect.x-5f,rect.y,17,20);
				if(rect1.x+6f > rect.x){
					Color color=GUI.color;
					if(i==selectedEventIndex){
						Color mColor=Color.blue;
						GUI.color=mColor;
					}
					GUI.Label(rect1,"",(GUIStyle)"Grad Up Swatch");
					GUI.color=color;
				}
			}
		}
	}

	private void OnPlayModeStateChange(){
		timeline.Stop ();
	}

	private void OnDestroy(){
		UndoObject ();
	}

	private void OnSelectionChange(){
		timeline.Stop();
		selectedGameObject = Selection.activeGameObject;
		if (selectedGameObject != null) {
			tweener = selectedGameObject.GetComponent<Tweener> ();
		} else {
			tweener = null;
		}
		selectedNodeIndex = 0;
		Repaint ();
	}

	private bool playForward;
	private float time;
	private bool stop;

	private void Update(){
		if (!Application.isPlaying) {
			if (isPlaying && !stop) {
				if( (float)EditorApplication.timeSinceStartup >time){
					switch(wrap){
					case SequenceWrap.PingPong:
						playForward=!playForward;
						time=(float)EditorApplication.timeSinceStartup+GetSequenceEnd();
						if(playForward){
							timeline.CurrentTime = 0;
							playStartTime=(float)EditorApplication.timeSinceStartup;
						}
						break;
					case SequenceWrap.Once:
						sequence.Stop(false);
						playStartTime=(float)EditorApplication.timeSinceStartup;
						timeline.CurrentTime = 0;
						stop=true;
						break;
					case SequenceWrap.ClampForever:
						sequence.Stop(true);
						stop=true;
						break;
					case SequenceWrap.Loop:
						sequence.Stop(false);
						playStartTime=(float)EditorApplication.timeSinceStartup;
						timeline.CurrentTime = 0;
						stop=false;
						time=(float)EditorApplication.timeSinceStartup+GetSequenceEnd();
						break;
					}		
				}
				
				timeline.CurrentTime= (playForward?((float)EditorApplication.timeSinceStartup-playStartTime):time-(float)EditorApplication.timeSinceStartup);
				//->
				//timeline.CurrentTime = (float)EditorApplication.timeSinceStartup - playStartTime;
				EditorUpdate (timeline.CurrentTime);
				Repaint ();
			}
			if (isRecording) {
				EditorUpdate (timeline.CurrentTime);
			}
		} else {
			if(tweener != null && sequence != null && tweener.IsPlaying(sequence.name)){
				timeline.CurrentTime=sequence.passedTime;
				Repaint();
			}
		}
	}

	public float GetSequenceEnd(){
		if (sequence == null) {
			return Mathf.Infinity;			
		}
		float sequenceEnd = 0;
		foreach (SequenceNode node in sequence.nodes) {
			if(sequenceEnd< (node.startTime+node.duration)){
				sequenceEnd=node.startTime+node.duration;
			}
		}
		return sequenceEnd;
	}

	private void OnGUI(){
		bool enabled = GUI.enabled;
		GUI.enabled = selectedGameObject != null && !Application.isPlaying;
		timeline.DoTimeline (new Rect(0,0,this.position.width,this.position.height));
		GUI.enabled = enabled;
	}

	private void EditorUpdate(float time){
		if (sequence != null) {
			sequence.nodes = sequence.nodes.OrderBy (x => x.startTime).ToList ();
			foreach (SequenceNode node in sequence.nodes) {
				node.UpdateTween (time);
			}
			//Canvas.ForceUpdateCanvases();
			EditorUtility.SetDirty(tweener);
		}
	}

	private void OnTimelineClick(float time){
		if (sequence == null) {
			return;
		}
		foreach (SequenceNode node in sequence.nodes) {
			if(time < node.startTime  ){
				node.UpdateValue(0.0f);
			}

			if(time > node.startTime+node.duration){
				node.UpdateValue(1.0f);
			}
		}
	}

	private void DrawNodes(Rect position){
		if (sequence == null) {
			return;
		}
		foreach (SequenceNode node in sequence.nodes) {
			EditorGUIUtility.AddCursorRect (new Rect (timeline.SecondsToGUI (node.startTime) - 5, node.channel * 20 , 10, 20), MouseCursor.ResizeHorizontal);			
			EditorGUIUtility.AddCursorRect (new Rect (timeline.SecondsToGUI (node.startTime+node.duration)-5, node.channel * 20 , 10, 20), MouseCursor.ResizeHorizontal);
			EditorGUIUtility.AddCursorRect (new Rect(timeline.SecondsToGUI(node.startTime),node.channel*20,timeline.SecondsToGUI(node.duration),20), MouseCursor.Pan);
		}

		foreach(SequenceNode node in sequence.nodes){
			Rect boxRect=new Rect(timeline.SecondsToGUI(node.startTime),node.channel*20,timeline.SecondsToGUI(node.duration),20);
			GUI.Box (boxRect,"","TL LogicBar 0");
			
			GUIStyle style = new GUIStyle("Label");
			style.fontSize= (selectedNode==node?12:style.fontSize);
			style.fontStyle= (selectedNode==node?FontStyle.Bold:FontStyle.Normal);
			Color color=style.normal.textColor;
			color.a=(selectedNode==node?1.0f:0.7f);
			style.normal.textColor=color;
			Vector3 size=style.CalcSize(new GUIContent(node.target.GetType().Name+"."+node.property));
			Rect rect1=new Rect(boxRect.x+boxRect.width*0.5f-size.x*0.5f,boxRect.y+boxRect.height*0.5f-size.y*0.5f,size.x,size.y);
			GUI.Label(rect1,node.target.GetType().Name+"."+node.property,style);
		}
		DoEvents ();
	}


	private bool dragEvent;
	private void DoEvents(){
		Event ev = Event.current;
		switch (ev.rawType) {
		case EventType.MouseDown:
			for(int j=0;j< sequence.events.Count;j++){
				Rect rect1=new Rect(timeline.SecondsToGUI(sequence.events[j].time)-5f,-15,17,20);
				if(rect1.Contains(Event.current.mousePosition)){
					selectedEventIndex=j;
					selectedNodeIndex=int.MaxValue;
					if(ev.button==0){
						dragEvent=true;
					}
					if(ev.button==1){
						GenericMenu genericMenu = new GenericMenu ();
						genericMenu.AddItem (new GUIContent ("Remove"), false,delegate() {
							sequence.events.RemoveAt(selectedEventIndex);
						});
						genericMenu.ShowAsContext ();
					}
					ev.Use();
				}
			}

			for(int i=0;i<sequence.nodes.Count;i++) {
				SequenceNode node=sequence.nodes[i];

				if (new Rect (timeline.SecondsToGUI (node.startTime) - 5, node.channel * 20 , 10, 20).Contains (Event.current.mousePosition)) {
					selectedNodeIndex=i;
					selectedEventIndex=int.MaxValue;
					resizeNodeStart = true;
					EditorGUI.FocusTextInControl("");
					ev.Use();
				}
				
				if (new Rect (timeline.SecondsToGUI (node.startTime+node.duration)-5, node.channel * 20 , 10, 20).Contains (Event.current.mousePosition)) {
					selectedNodeIndex=i;
					selectedEventIndex=int.MaxValue;
					resizeNodeEnd = true;
					EditorGUI.FocusTextInControl("");
					ev.Use();
				}
				
				if (new Rect(timeline.SecondsToGUI(node.startTime),node.channel*20,timeline.SecondsToGUI(node.duration),20).Contains (Event.current.mousePosition)) {
					if (ev.button == 0) {
						timeClickOffset = node.startTime - timeline.GUIToSeconds (Event.current.mousePosition.x);
						dragNode = true;
						selectedNodeIndex=i;
						selectedEventIndex=int.MaxValue;
						EditorGUI.FocusTextInControl("");
					} 
					if(ev.button == 1){
						GenericMenu genericMenu = new GenericMenu ();
						genericMenu.AddItem (new GUIContent ("Remove"), false, this.RemoveTween, node);
						genericMenu.ShowAsContext ();
					}
					ev.Use();
				}
			}
			break;
		case EventType.MouseDrag:
			if(dragEvent){
				selectedEvent.time = timeline.GUIToSeconds (Event.current.mousePosition.x);
				selectedEvent.time=Mathf.Clamp(selectedEvent.time,0,float.MaxValue);
				ev.Use();
			}
			if(resizeNodeStart){
				selectedNode.startTime = timeline.GUIToSeconds (Event.current.mousePosition.x);
				selectedNode.startTime=Mathf.Clamp(selectedNode.startTime,0,float.MaxValue);
				if(selectedNode.startTime>0){
					selectedNode.duration -= timeline.GUIToSeconds (ev.delta.x );
					selectedNode.duration=Mathf.Clamp(selectedNode.duration,0.01f,float.MaxValue);
				}
				
				ev.Use();
			}
			
			if(resizeNodeEnd){
				selectedNode.duration = (timeline.GUIToSeconds (Event.current.mousePosition.x) - selectedNode.startTime);
				selectedNode.duration=Mathf.Clamp(selectedNode.duration,0.01f,float.MaxValue);
				ev.Use();
			}
			
			if(dragNode && !resizeNodeStart && !resizeNodeEnd){
				selectedNode.startTime = timeline.GUIToSeconds (Event.current.mousePosition.x) +timeClickOffset;
				selectedNode.startTime=Mathf.Clamp(selectedNode.startTime,0,float.MaxValue);
				if(Event.current.mousePosition.y > selectedNode.channel*20+25){
					selectedNode.channel+=1;
				}
				if(Event.current.mousePosition.y < selectedNode.channel*20-5){
					selectedNode.channel-=1;
				}
				selectedNode.channel=Mathf.Clamp(selectedNode.channel,0,int.MaxValue);
				ev.Use();
			}
			break;
		case EventType.MouseUp:
			dragNode=false;
			resizeNodeStart=false;
			resizeNodeEnd=false;
			dragEvent=false;
			break;
		}
	}

	private bool backup;
	private void OnRecord(bool isRecording){
		if (tweener == null && selectedGameObject != null && isRecording) {
			tweener=selectedGameObject.AddComponent<Tweener>();
			AddSequence(tweener);
			//sequence.nodes = new List<SequenceNode>();
			EditorUtility.SetDirty(tweener);
		}
		this.isRecording = isRecording;
		if (!isRecording) {
			OnTimelineClick (0.0f);
			UndoObject();
		} else {
			RecordObject();
		} 

	}

	private void OnPlay(bool isPlaying){
		playStartTime=(float)EditorApplication.timeSinceStartup;
		time=(float)EditorApplication.timeSinceStartup+GetSequenceEnd();
		timeline.CurrentTime = 0;

		this.isPlaying = isPlaying;
		if (!isPlaying) {
			OnTimelineClick (0.0f);
			UndoObject ();
		} else {
			stop=false;
			playForward = true;
			RecordObject();
		}
	}

	private void RecordObject(){
		if (backup) {
			return;
		}
		backupGameObject=(GameObject)Instantiate(selectedGameObject);
		backupGameObject.transform.SetParent (selectedGameObject.transform.parent, false);
		backupGameObject.name=selectedGameObject.name;
		backupGameObject.SetActive(false);
		backupGameObject.hideFlags=HideFlags.HideInHierarchy;
		backup = true;
	}

	private void UndoObject(){
		if (!backup) {
			return;
		}
		Tweener mTweener = backupGameObject.GetComponent<Tweener> ();
		mTweener.sequences = tweener.sequences;
		for (int i = 0; i<tweener.sequences.Count; i++) {
			for(int j=0;j<tweener.sequences[i].nodes.Count;j++){
				SequenceNode node=tweener.sequences[i].nodes[j];
				mTweener.sequences[i].nodes[j].target=mTweener.GetComponent(node.target.GetType());
			}
		}
		DestroyImmediate(selectedGameObject);
		selectedGameObject=backupGameObject;
		selectedGameObject.hideFlags=HideFlags.None;
		selectedGameObject.SetActive(true);
		Selection.activeGameObject=selectedGameObject;
		backup = false;
	}

	private int selectedSequenceIndex;
	private SequenceWrap wrap;

	private void OnSettings(float width){
		GUILayout.BeginHorizontal ();
		if (GUILayout.Button (sequence != null ? sequence.name : "[None Selected]", EditorStyles.toolbarDropDown, GUILayout.Width (width * 0.5f))) {
			GenericMenu toolsMenu = new GenericMenu ();
			if(tweener != null){
				for (int i=0;i<tweener.sequences.Count;i++) {
					int mIndex=i;
					toolsMenu.AddItem (new GUIContent (tweener.sequences[mIndex].name), false, delegate() {
						selectedSequenceIndex=mIndex;
					});
				}
			}
			toolsMenu.AddItem (new GUIContent ("[New Sequence]"), false, delegate() {
				AddTweener(Selection.activeGameObject);
				AddSequence(tweener);
			});
			GUIUtility.keyboardControl = 0;
			toolsMenu.DropDown (new Rect (3, 37, 0, 0));
			EditorGUIUtility.ExitGUI ();
		}
		
		if (sequence != null) {
			wrap = sequence.wrap;			
		}
		wrap = (SequenceWrap)EditorGUILayout.EnumPopup (wrap, EditorStyles.toolbarDropDown, GUILayout.Width (width * 0.5f));
		if (sequence != null) {
			sequence.wrap = wrap;			
		}
		GUILayout.EndHorizontal ();

		//-->
		GUILayout.BeginVertical ();
		settingsScroll = GUILayout.BeginScrollView (settingsScroll);
		if (tweener != null) {
			DoSequence ();
		}
		GUILayout.EndScrollView ();

		GUILayout.FlexibleSpace ();
		bool enabled = GUI.enabled;
		GUI.enabled = sequence != null;
		if (GUILayout.Button ("Add Tween")) {
			GenericMenu genericMenu = new GenericMenu ();
			Component[] components=selectedGameObject.GetComponents<Component>();
			for(int i=0;i< components.Length;i++){
				PropertyInfo[] properties=components[i].GetType().GetProperties();
				for(int j=0;j<properties.Length;j++){
					if(properties[j].CanWrite){
						if(IsSupportedType(properties[j].PropertyType)){
							genericMenu.AddItem(new GUIContent(components[i].GetType().Name+"/"+properties[j].Name),false,AddTween,new SequenceNode(components[i],properties[j].Name));
						}
					}
				}

				FieldInfo[] fields=components[i].GetType().GetFields();
				for(int j=0;j<fields.Length;j++){
					if(IsSupportedType(fields[j].FieldType)){
						genericMenu.AddItem(new GUIContent(components[i].GetType().Name+"/"+fields[j].Name),false,AddTween,new SequenceNode(components[i],fields[j].Name));
					}
				}
			}
			genericMenu.ShowAsContext ();
		}
		GUI.enabled = enabled;
		GUILayout.EndVertical ();
	}

	private void DoSequence(){
		EditorGUIUtility.labelWidth=63;
		SerializedObject serializedObject = new SerializedObject (tweener);
		serializedObject.Update ();
		SerializedProperty sequenceArray = serializedObject.FindProperty ("sequences");

		if(selectedSequenceIndex < sequenceArray.arraySize){
			SerializedProperty sequenceProperty = sequenceArray.GetArrayElementAtIndex (selectedSequenceIndex);
			SerializedProperty sequenceName=sequenceProperty.FindPropertyRelative("name");
			SerializedProperty playAutomatically=sequenceProperty.FindPropertyRelative("playAutomatically");
			EditorGUILayout.PropertyField(sequenceName);
			EditorGUILayout.PropertyField(playAutomatically);
			SerializedProperty nodesArray = sequenceProperty.FindPropertyRelative ("nodes");
			if(selectedNodeIndex < nodesArray.arraySize){
				SerializedProperty nodeProperty = nodesArray.GetArrayElementAtIndex (selectedNodeIndex);
				EditorGUILayout.PropertyField(nodeProperty.FindPropertyRelative("startTime"));
				EditorGUILayout.PropertyField(nodeProperty.FindPropertyRelative("duration"));
				SerializedProperty fromProperty=null;
				SerializedProperty toProperty=null;
				if(selectedNode.PropertyType == typeof(float)){
					fromProperty = nodeProperty.FindPropertyRelative("fromFloat");
					toProperty = nodeProperty.FindPropertyRelative("toFloat");
				}else if(selectedNode.PropertyType == typeof(Vector2)){
					fromProperty = nodeProperty.FindPropertyRelative("fromVector2");
					toProperty = nodeProperty.FindPropertyRelative("toVector2");
				}else if(selectedNode.PropertyType == typeof(Vector3)){
					fromProperty = nodeProperty.FindPropertyRelative("fromVector3");
					toProperty = nodeProperty.FindPropertyRelative("toVector3");
				}else if(selectedNode.PropertyType == typeof(Color)){
					fromProperty = nodeProperty.FindPropertyRelative("fromColor");
					toProperty = nodeProperty.FindPropertyRelative("toColor");
				}
				if(fromProperty != null && toProperty != null){
					EditorGUILayout.PropertyField(fromProperty, new GUIContent("From"));
					EditorGUILayout.PropertyField(toProperty, new GUIContent("To"));
				}
				EditorGUILayout.PropertyField(nodeProperty.FindPropertyRelative("ease"));
			}
			SerializedProperty eventsArray=sequenceProperty.FindPropertyRelative("events");
			if(selectedEventIndex < eventsArray.arraySize){
				SerializedProperty eventProperty = eventsArray.GetArrayElementAtIndex (selectedEventIndex);
				SerializedProperty methodProperty=eventProperty.FindPropertyRelative("method");
				//SerializedProperty typeProperty=eventProperty.FindPropertyRelative("type");
				SerializedProperty argumentsArray=eventProperty.FindPropertyRelative("arguments");

				EventNode eventNode=sequence.events[selectedEventIndex];

				if(GUILayout.Button(eventNode.SerializedType.Name+"."+methodProperty.stringValue,"DropDown")){
					GenericMenu menu = new GenericMenu ();
					Component[] components=selectedGameObject.GetComponents<Component>();
					List<Type> types = new List<Type> ();
					types.AddRange (components.Select (x => x.GetType ()));
					types.AddRange (GetSupportedTypes ());
					foreach (Type type in types) {
						List<MethodInfo> functions= GetValidFunctions(type,!(type.IsSubclassOf(typeof(Component)) || type.IsSubclassOf(typeof(MonoBehaviour))) || selectedGameObject.GetComponent(type)==null);
						foreach(MethodInfo mi in functions){
							if(mi != null){
								EventNode node = new EventNode ();
								node.time = timeline.CurrentTime;
								node.SerializedType=type;
								node.method=mi.Name;
								node.arguments=GetMethodArguments(mi);
								node.time=eventNode.time;
								menu.AddItem(new GUIContent(type.Name+"/"+mi.Name),false,delegate() {
									sequence.events[selectedEventIndex]=node;
									EditorUtility.SetDirty(tweener);
								});
							}
						}
					}
					menu.ShowAsContext ();
				}
				for(int i=0;i<argumentsArray.arraySize;i++){
					SerializedProperty argumentProperty=argumentsArray.GetArrayElementAtIndex(i);
					EditorGUILayout.PropertyField(argumentProperty.FindPropertyRelative(eventNode.arguments[i].GetValueName()),new GUIContent("Parameter"));
				}

			}
		}

		serializedObject.ApplyModifiedProperties ();
	}	

	private void AddTween(object node){
		SequenceNode mNode = node as SequenceNode;
		sequence.nodes.Add (mNode);
		mNode.SetDefaultValue ();
		EditorUtility.SetDirty (tweener);
	}

	private void RemoveTween(object node){
		SequenceNode mNode = node as SequenceNode;
		sequence.nodes.Remove (mNode);
		EditorUtility.SetDirty (tweener);
	}

	private void AddTweener(GameObject gameObject){
		if (gameObject.GetComponent<Tweener> () == null) {
			tweener=gameObject.AddComponent<Tweener>();
			tweener.sequences=new List<Sequence>();
			EditorUtility.SetDirty(gameObject);
		}
	}

	private void AddSequence(Tweener tweener){
		if (tweener != null) {
			if(tweener.sequences == null){
				tweener.sequences=new List<Sequence>();
			}
			Sequence sequence= new Sequence();
			sequence.nodes= new List<SequenceNode>();
			sequence.events= new List<EventNode>();
			int cnt = 0;
			while (tweener.sequences.Find(x=>x.name=="New Sequence "+cnt.ToString()) != null) {
				cnt++;	
			}
			sequence.name="New Sequence "+cnt.ToString();
			tweener.sequences.Add(sequence);
			selectedSequenceIndex=tweener.sequences.Count-1;
		}
	}

	private bool IsSupportedType(Type type){
		if (type == typeof(float) || type == typeof(Vector3) || type == typeof(Vector2) || type == typeof(Color)) {
			return true;
		}
		return false;
	}
}
