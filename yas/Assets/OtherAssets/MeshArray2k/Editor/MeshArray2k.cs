////////////////////////////////////////////////////////////////////////////
//
// Author: Alireza Amiri
// Copyright (c) 2012 Kiavash2k.com
//
// Description: Mesh Array Tool for Unity to clone gameObjects Rail Easily
//
////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class MeshArray2k : EditorWindow {
	
	//Init Window and create a menuItem
	[MenuItem("Window/Kiavash2k/MeshArray2k")]
	public static void ShowWindow()
	{
		EditorWindow.GetWindow(typeof(MeshArray2k));
	}
	
	//Definitions
	public Transform Source = null;
	public string SourceName = "";
	public Transform NewPreview = null;
	public Vector3 Move = Vector3.zero;
	public Vector3 Rotation = Vector3.zero;
	public Vector3 LocalRotation = Vector3.zero;
	public int Count = 0;
	public int items = 0;
	public bool PreviewToggle = true;
	public string MeshArray = "MeshArray2kPreviewItem";
	public Vector2 scrollPosition; // Window Scroll Vector2D
	
	void OnGUI () {
		// make window Scrollable
		scrollPosition = GUILayout.BeginScrollView (scrollPosition);
		
		Source = Selection.activeTransform;
		
		//get source item and clone count
		if (Selection.activeTransform == null)
			SourceName = EditorGUILayout.TextField("Selected Object Name: ","Nothing to Clone");
		else
			SourceName = EditorGUILayout.TextField("Selected Object Name: ",Source.name);
		
		EditorGUILayout.Separator();
		Count = EditorGUILayout.IntField("Clone Count: ", Count);
		
		//set position
		EditorGUILayout.Separator();
		Move = EditorGUILayout.Vector3Field("Move: ", Move);
		LocalRotation = EditorGUILayout.Vector3Field("Local Rotation: ", LocalRotation);
		
		EditorGUILayout.Separator();
		if (GUILayout.Button("Generate Items"))
		{
			items = 0;
			if (Selection.activeTransform == null)
				EditorUtility.DisplayDialog("Error 110:", "Please select an object to clone.", "ok");
			else
				if (Count <= 0)
					EditorUtility.DisplayDialog("Error 121:", "Please raise the clone count before Generate.", "ok");
				else
					Generate();
			
		}		

		GUILayout.EndScrollView();

		if(PreviewToggle)
			ShowGizmos();
	
	}
	
	//Remove Preview Items
	void RemoveGizmos(bool show)
	{
		if (GameObject.Find(MeshArray) != null && !show)
		{
			GameObject Temp = GameObject.Find(MeshArray);
			DestroyImmediate(Temp);
		}	
		
		if (GameObject.Find(MeshArray) != null && show)
		{
			GameObject Temp = GameObject.Find(MeshArray);
			DestroyImmediate(Temp);
			ShowGizmos();
		}	
		
	}
	
	void ShowGizmos()
	{
		Vector3 MovePath = Move;
		Vector3 LocRotPath = LocalRotation;
		
		if (GameObject.Find(MeshArray) != null)
			RemoveGizmos(true);
		else
			if (Source != null)
			{
				for (int i = 0; i < Count; i++)
				{	
					
					NewPreview = (Transform) Instantiate(Source);
					NewPreview.position += MovePath;
					NewPreview.localEulerAngles += LocRotPath;
				
					MovePath = MovePath + Move;
					LocRotPath = LocRotPath + LocalRotation;
				
					NewPreview.name = MeshArray;
				}
				
			}
	}
	
	void OnDestroy()
	{
		if (GameObject.Find(MeshArray) != null)
		{
			bool options = EditorUtility.DisplayDialog("Saving Objects", "Do you want to save the generated objects?", "Yes", "No");
			
			switch(options)
			{
				case true:
					Generate();
					break;
				case false:
					while (GameObject.Find(MeshArray) != null)
					{
						GameObject Temp = GameObject.Find(MeshArray);
						DestroyImmediate(Temp);
					}
					break;
				default:
					break;
			}	
		}
		else 
		{
			return;
		}
	}
	
	void Generate () {
		if (GameObject.Find(MeshArray) != null)
		{
			items++;
			GameObject Temp = GameObject.Find(MeshArray);
			Temp.name = Source.name + "_" + items;
			Generate();
		}		
	}
}
