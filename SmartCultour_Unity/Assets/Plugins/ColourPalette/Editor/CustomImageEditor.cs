using UnityEditor;
using UnityEngine;

namespace ColourPalette
{
    [CustomEditor(typeof(CustomImage))]
    public class CustomImageEditor : UnityEditor.UI.ImageEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();//Draw inspector UI of ImageEditor

            serializedObject.Update();
            EditorGUILayout.ObjectField(serializedObject.FindProperty("colourAsset"), typeof(ColourAsset));
            serializedObject.ApplyModifiedProperties();
        }
    }
}