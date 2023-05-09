using UnityEditor;
using UnityEngine;
using UnityEditor.UI;

[CustomEditor(typeof(MatchingContentSizeFitter))]
public class MatchingContentSizeFitterEditor : ContentSizeFitterEditor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		serializedObject.Update();
		EditorGUILayout.ObjectField(serializedObject.FindProperty("m_matchingTransform"), typeof(RectTransform));
		serializedObject.ApplyModifiedProperties();
	}
}