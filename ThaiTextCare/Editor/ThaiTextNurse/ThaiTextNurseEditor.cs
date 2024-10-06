using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace PhEngine.ThaiTextCare.Editor
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
            DrawCopyToClipboardButton();

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Editor-Only", EditorStyles.boldLabel);
            ThaiTextCareGUI.DrawHorizontalLine();
            PropertyField("isVisualizeInEditor");
            PropertyField("guiColor");
            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
                foreach (var t in targets)
                    (t as ThaiTextNurse)?.NotifyChange();
            }

            EditorGUILayout.Space();
            DrawDictionarySection();
            EditorGUILayout.EndVertical();
        }

        void DrawCopyToClipboardButton()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Copy Output to Clipboard", GUILayout.ExpandWidth(false)))
            {
                GUIUtility.systemCopyBuffer = nurse.OutputString;
                SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Copied Output to Clipboard"), 1f);
            }

            EditorGUILayout.EndHorizontal();
        }

        void DrawDictionarySection()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Dictionary", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Force Rebuild", EditorStyles.linkLabel, GUILayout.ExpandWidth(false)))
            {
                ThaiTextNurse.RebuildDictionary();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            pendingWords = EditorGUILayout.TextField("Words :", pendingWords);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Use [Space] to separate words", EditorStyles.miniLabel);
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

        void OnSceneGUI()
        {
            if (!nurse.IsShouldDrawGizmos())
                return;
            
            var settings = ThaiTextNurseSettings.PrepareInstance();
            var breakCharacter = ThaiTextNurse.GetWordBreakCharacter(settings);
            if (string.IsNullOrEmpty(breakCharacter))
                return;

            var breakIndices = new List<int>();
            var wordLeftCount = nurse.LastWordCount;
            for (int i = 0; i < nurse.CharacterInfoLength; i++)
            {
                if (nurse.GetCharacterInfo(i).character == breakCharacter[0])
                {
                    breakIndices.Add(i);
                    
                    //This is needed because CharacterInfos are kind of unreliable on undo
                    wordLeftCount--;
                    if (wordLeftCount <= 1)
                        break;
                }
            }

            var oldColor = Handles.color;
            var color = nurse.guiColor;
            color.a = 0.75f;
            Handles.color = color;
            var widthScale = nurse.transform.lossyScale.x;

            // 0.1f seems to be a magic number that makes the height scale looks correct for Worldspace texts.
            // Why? I don't know... Unity magic?
            var heightScale = nurse.transform.lossyScale.y * (nurse.TextComponent is TextMeshProUGUI ? 1f : 0.1f);
            foreach (int index in breakIndices)
            {
                var charInfo = nurse.GetCharacterInfo(index);
                Vector3 pos = nurse.transform.TransformPoint(charInfo.bottomRight);
                float lineHeight = charInfo.pointSize * heightScale;
                Handles.DrawLine(pos, pos + Vector3.up * lineHeight, 3f * widthScale);
            }
            Handles.color = oldColor;
        }
    }
}