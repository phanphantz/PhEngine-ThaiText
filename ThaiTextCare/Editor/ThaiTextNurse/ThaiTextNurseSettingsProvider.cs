using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PhEngine.ThaiTextCare.Editor
{
    public class ThaiTextNurseSettingsProvider : SettingsProvider
    {
        const string SettingsPath = "Project/Thai Text Nurse Settings";
        ThaiTextNurseSettings settings;
        SerializedObject serializedObject;

        public ThaiTextNurseSettingsProvider(string path, SettingsScope scope = SettingsScope.Project) : base(path, scope)
        {
            settings = ThaiTextNurseSettings.PrepareInstance();
            serializedObject = new SerializedObject(settings);
        }

        public override void OnGUI(string searchContext)
        {
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("dictionaryResourcePath"));
            EditorGUI.indentLevel++;
            EditorGUILayout.HelpBox("The path must be under 'Resources' folder. Without file extension.", MessageType.Info);
            EditorGUI.indentLevel--;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("wordBreakType"));
            if (serializedObject.FindProperty("wordBreakType").boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("customCharacter"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("loadDictionaryOnStart"));
            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(settings);
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Apply Changes", GUILayout.ExpandWidth(false)))
            {
                ThaiTextNurse.RebuildDictionary();
            }
            EditorGUILayout.EndHorizontal();
        }
        
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);
            Undo.undoRedoPerformed += Repaint;
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();
            Undo.undoRedoPerformed -= Repaint;
        }

        [SettingsProvider]
        public static SettingsProvider CreateThaiTextNurseSettingsProvider()
        {
            return new ThaiTextNurseSettingsProvider(SettingsPath);
        }
    }
}