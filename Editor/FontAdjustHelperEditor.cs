using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace PhEngine.UI.ThaiText.Editor
{
    [CustomEditor(typeof(FontAdjustHelper))]
    public class FontAdjustHelperEditor : UnityEditor.Editor
    {
        FontAdjustHelper fontAdjustHelper;
        string targetCharacters = "";
        string pairCharacters = "";
        Color selectedColor = new Color(0f, 0.8f, 0.9f);
        GUIStyle hugeFontStyle;
        
        void OnEnable()
        {
            fontAdjustHelper = target as FontAdjustHelper;
        }

        public override void OnInspectorGUI()
        {
            hugeFontStyle = new GUIStyle(EditorStyles.label);
            hugeFontStyle.fontSize = 20;
            hugeFontStyle.normal.textColor = new Color(0.8f, 0.5f, 0);

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            var fontAsset = EditorGUILayout.ObjectField(new GUIContent("Font Asset"), fontAdjustHelper.fontAsset, typeof(TMP_FontAsset), false) as TMP_FontAsset;
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(fontAdjustHelper, "Change font asset");
                fontAdjustHelper.fontAsset = fontAsset;
                ApplyChanges();
            }

            if (fontAdjustHelper.fontAsset)
            {
                if (GUILayout.Button("Clear All Pairs", GUILayout.ExpandWidth(false)))
                {
                    bool result = EditorUtility.DisplayDialog($"Confirmation", $"Are you sure you want to clear all glyph adjustment pairs on the font asset : {fontAdjustHelper.fontAsset.name}?", "Yes", "No");
                    if (result)
                    {
                        Undo.RecordObject(fontAdjustHelper.fontAsset, " Clear all adjustment pairs");
                        fontAdjustHelper.ClearAllExistingPairs();
                        EditorUtility.SetDirty(fontAdjustHelper.fontAsset);
                    }
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            var i = 0;
            var list = fontAdjustHelper.glyphCombinationList;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Glyph  Combination List (" + list.Count + ")", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+", GUILayout.ExpandWidth(false)))
            {
                Undo.RecordObject(fontAdjustHelper, "Add new combination");
                list.Add(new GlyphCombination());
                ApplyChanges();
                return;
            }
            EditorGUILayout.EndHorizontal();
            
            
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
         
            foreach (var combo in list)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                var style = new GUIStyle(GUI.skin.horizontalSlider);
                style.normal.background = new Texture2D(1, 2);
                style.normal.background.SetPixel(0,0, selectedColor);
                style.normal.background.SetPixel(0,1, Color.clear);
                style.normal.background.Apply();

                EditorGUILayout.BeginHorizontal();
                
                EditorGUILayout.LabelField(combo.DisplayName, EditorStyles.boldLabel);
                EditorGUI.BeginChangeCheck();
                
                if (GUILayout.Button("x", EditorStyles.boldLabel, GUILayout.ExpandWidth(false)))
                {
                    Undo.RecordObject(fontAdjustHelper, "Remove combination");
                    list.Remove(combo);
                    return;
                }
                EditorGUILayout.EndHorizontal();

              
                DrawCharacterAdder("Leading Glyphs ("+ combo.first.glyphs.Length + ")", ref targetCharacters, ref combo.first);
                DrawCharacterAdder("Following Glyphs (" + combo.second.glyphs.Length + ")",ref pairCharacters, ref combo.second);
                i++;
                EditorGUILayout.EndVertical();  
                EditorGUILayout.Space();
            }
            

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        void DrawCharacterAdder(string fieldName, ref string newCharacters, ref GlypthOffset offset)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField(fieldName, EditorStyles.boldLabel);
            if (!string.IsNullOrEmpty(offset.glyphs.Trim()))
                DrawPlacementAdjustor(offset);
            
            EditorGUI.BeginChangeCheck();
            var group = (ThaiGlyphGroup) EditorGUILayout.EnumPopup("Glyph Group",offset.group);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(fontAdjustHelper, "Change Glyph Groups");
                offset.AssignGroup(group);
                ApplyChanges();
            }

            if (offset.group == ThaiGlyphGroup.Custom)
            {
                var newName = EditorGUILayout.TextField("Custom Name", offset.customName);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(fontAdjustHelper, "Change combination name");
                    offset.customName = newName;
                }
                var isPressEnter = (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return);
                newCharacters = EditorGUILayout.TextField("Add Glyph: " , newCharacters);
            
                var isInputEmpty = string.IsNullOrEmpty(newCharacters.Trim());
                if (!isInputEmpty)
                {
                    EditorGUILayout.BeginHorizontal();
                    var displayedGlyphs = newCharacters.Select(c => ThaiLanguageInfo.GetDisplayedString(c.ToString()));
                    EditorGUILayout.LabelField("Preview : ", EditorStyles.boldLabel, GUILayout.Width(95));
             
                    EditorGUILayout.LabelField(string.Join(", ", displayedGlyphs), hugeFontStyle);
                    if (GUILayout.Button("Add", GUILayout.ExpandWidth(false)) || isPressEnter)
                    {
                        Undo.RecordObject(fontAdjustHelper, "Modify Characters");
                        offset.glyphs = string.Join("",offset.glyphs.ToCharArray().Union(newCharacters.ToCharArray()));
                        newCharacters = "";
                        ApplyChanges();
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            DrawGlyph(ref offset.glyphs);
            EditorGUILayout.EndVertical();
        }

        void DrawPlacementAdjustor(GlypthOffset offset)
        {
            EditorGUI.BeginChangeCheck();
            var offsetVector = EditorGUILayout.Vector2Field("Offset", new Vector2(offset.xPlacement, offset.yPlacement));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(fontAdjustHelper, "Change Target Character");
                offset.xPlacement = offsetVector.x;
                offset.yPlacement = offsetVector.y;
                ApplyChanges();
            }
        }

        void ApplyChanges()
        {
            fontAdjustHelper.ApplyModifications();
        }

        void DrawGlyph(ref string characters)
        {
            if (string.IsNullOrEmpty(characters))
                return;

            var cellSize = 25f;
            var cellPerRow = Mathf.FloorToInt(EditorGUIUtility.currentViewWidth / (cellSize * 2.5f));
            EditorGUILayout.BeginVertical();
            for (int i = 0; i < characters.Length; i++)
            {
                if (i == 0)
                    EditorGUILayout.BeginHorizontal();
             
                var character = characters[i];
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox, GUILayout.Width(cellSize));
                if (!FontAdjustHelper.TryGetGlyphIndex(fontAdjustHelper.fontAsset, character, out _, out var index))
                {
                    EditorGUILayout.HelpBox("The Character '" + character + "' is not found on the font asset.", MessageType.Error);
                }
                else
                {
                    EditorGUILayout.LabelField(" "+ ThaiLanguageInfo.GetDisplayedString(character.ToString()), hugeFontStyle, GUILayout.Width(cellSize), GUILayout.Height(25f));
                }
                if (GUILayout.Button("x", EditorStyles.boldLabel ,GUILayout.ExpandWidth(false)))
                {
                    Undo.RecordObject(fontAdjustHelper, "Modify Characters");
                    characters = characters.Replace(character.ToString(), "");
                    ApplyChanges();
                }
                EditorGUILayout.EndHorizontal();

                if ((i + 1) % cellPerRow == 0)
                {
                    EditorGUILayout.EndHorizontal();
                    if (i != characters.Length - 1)
                        EditorGUILayout.BeginHorizontal();
                }
                else if (i == characters.Length - 1)
                {
                    EditorGUILayout.EndHorizontal();

                }
            }
            EditorGUILayout.EndVertical();
        }
    }
}