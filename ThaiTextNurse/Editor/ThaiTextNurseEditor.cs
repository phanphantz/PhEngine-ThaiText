using UnityEditor;
using UnityEngine;

namespace PhEngine.ThaiTMP.Editor
{
    [CustomEditor(typeof(ThaiTextNurse), true), CanEditMultipleObjects]
    public class ThaiTextNurseEditor : UnityEditor.Editor
    {
        ThaiTextNurse nurse;
        GUIStyle miniTextStyle;
        string pendingWords;
        
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
                EditorGUILayout.LabelField(nurse.LastWordCount.ToString("N0") + " Words", miniTextStyle);
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

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Dictionary" , EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Force Rebuild", EditorStyles.linkLabel,GUILayout.ExpandWidth(false)))
            {
               ThaiTextNurse.RebuildDictionary();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            pendingWords = EditorGUILayout.TextField("Words :",pendingWords);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Use whitespace to separate between words", EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Remove", GUILayout.ExpandWidth(false)))
            {
                if (!string.IsNullOrEmpty(pendingWords))
                    ThaiTextCareEditorCore.RemoveWordsFromDictionary(pendingWords.Trim());
            }
            if (GUILayout.Button("Add", GUILayout.ExpandWidth(false)))
            {
                if (!string.IsNullOrEmpty(pendingWords))
                    ThaiTextCareEditorCore.AddWordsToDictionary(pendingWords.Trim());
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
        }

        void PropertyField(string fieldName)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(fieldName));
        }
    }
}