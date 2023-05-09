using UnityEditor;
using UnityEngine;
using TMPro;
using TMPro.EditorUtilities;

[CustomEditor(typeof(CustomDropdown))]
public class CustomDropdownEditor : DropdownEditor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI(); //Draw inspector UI of DropdownEditor

		serializedObject.Update();
		EditorGUILayout.ObjectField(serializedObject.FindProperty("m_normalSprite"), typeof(Sprite));
		EditorGUILayout.ObjectField(serializedObject.FindProperty("m_expandSprite"), typeof(Sprite));
		serializedObject.ApplyModifiedProperties();
	}
}
