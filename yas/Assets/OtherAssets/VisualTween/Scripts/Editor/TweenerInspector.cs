using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(Tweener))]
public class TweenerInspector : Editor {
	public override void OnInspectorGUI ()
	{
		serializedObject.Update ();
		SerializedProperty sequenceArray = serializedObject.FindProperty ("sequences");
		EditorGUIUtility.labelWidth = 60;
		for (int i=0; i< sequenceArray.arraySize; i++) {
			SerializedProperty sequenceProperty=sequenceArray.GetArrayElementAtIndex(i);
			GUILayout.BeginHorizontal("box");

			EditorGUILayout.PropertyField(sequenceProperty.FindPropertyRelative("name"));
			if(GUILayout.Button(EditorGUIUtility.FindTexture("toolbar minus"),"label",GUILayout.Width(20))){
				sequenceArray.DeleteArrayElementAtIndex(i);
			}
			GUILayout.EndHorizontal();
		}
		serializedObject.ApplyModifiedProperties ();
	}
}
