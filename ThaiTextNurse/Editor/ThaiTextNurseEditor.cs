using UnityEditor;
using UnityEngine;

namespace PhEngine.ThaiTMP.Editor
{
    [CustomEditor(typeof(ThaiTextNurse), true), CanEditMultipleObjects]
    public class ThaiTextNurseEditor : UnityEditor.Editor
    {
        ThaiTextNurse nurse;
        GUIStyle miniTextStyle;

        void OnEnable()
        {
            nurse = target as ThaiTextNurse;
        }

        public override void OnInspectorGUI()
        {
            miniTextStyle = new GUIStyle(EditorStyles.miniLabel);
            miniTextStyle.alignment = TextAnchor.MiddleRight;
            
            PropertyField("correction");
            EditorGUILayout.BeginHorizontal();
            PropertyField("isTokenize");
            if (serializedObject.FindProperty("isTokenize").boolValue)
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(nurse.LastWordCount + " Words", miniTextStyle);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel++;
            PropertyField("separator");
            EditorGUI.indentLevel--;

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
                foreach (var t in targets)
                    (t as ThaiTextNurse)?.NotifyChange();
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Copy Output to Clipboard", GUILayout.ExpandWidth(false)))
            {
                GUIUtility.systemCopyBuffer = nurse.OutputString;
                SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Copied Output to Clipboard"), 1f);
            }
            EditorGUILayout.EndHorizontal();
        }

        void PropertyField(string fieldName)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(fieldName));
        }
    }
}