using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using TMP_Text = TMPro.TMP_Text;

namespace PhEngine.UI.ThaiText.Editor
{
    [CustomEditor(typeof(FontAdjustHelper))]
    public class FontAdjustHelperEditor : UnityEditor.Editor
    {
        FontAdjustHelper fontAdjustHelper;
        string targetCharacters = "";
        string pairCharacters = "";
        GUIStyle hugeFontStyle;
        int selectedIndex = -1;
        Color orangeColor = new Color(0.8f, 0.5f, 0);
        
        void OnEnable()
        {
            Undo.undoRedoPerformed -= UndoRedoPerformed;
            Undo.undoRedoPerformed += UndoRedoPerformed;
            fontAdjustHelper = target as FontAdjustHelper;
        }

        void OnDisable()
        {
            Undo.undoRedoPerformed -= UndoRedoPerformed;
        }

        void UndoRedoPerformed()
        {
            ApplyChanges();
        }

        public override void OnInspectorGUI()
        {
            hugeFontStyle = new GUIStyle(EditorStyles.label);
            hugeFontStyle.fontSize = 20;
            hugeFontStyle.normal.textColor = orangeColor;
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
                if (GUILayout.Button("Clean Rebuild", GUILayout.ExpandWidth(false)))
                {
                    bool result = EditorUtility.DisplayDialog($"Confirmation", $"Are you sure you want to Clean Rebuild all the glyph pair adjustments? Any pair adjustment you made directly to the font asset will be lost", "Yes", "No");
                    if (result)
                    {
                        Undo.RecordObject(fontAdjustHelper.fontAsset, "Perform Clean Rebuild");
                        fontAdjustHelper.CleanRebuild();
                        EditorGUILayout.EndHorizontal();
                        return;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            
            var combinationList = fontAdjustHelper.glyphCombinationList;
            EditorGUI.indentLevel++;
            var variantCount = combinationList.Sum(i => i.VariantCount);
            EditorGUILayout.LabelField("Combination Variant Count : " + variantCount);
            if (fontAsset)
                EditorGUILayout.LabelField("Actual Pair Adjustment Count : " + fontAsset.fontFeatureTable.glyphPairAdjustmentRecords.Count);
            EditorGUI.indentLevel--;

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            var testerTMPText = EditorGUILayout.ObjectField("Tester TMP Text", fontAdjustHelper.testerTMPText, typeof(TMP_Text), true) as TMP_Text;
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(fontAdjustHelper, "Change Tester TMP Text");
                fontAdjustHelper.SetTesterTMPText(testerTMPText);
            }

            if (testerTMPText == null)
            {
                if (GUILayout.Button("Create In Scene", GUILayout.ExpandWidth(false)))
                {
                    GameObject textObject = new GameObject("TMP_Text_FontTester");
                    var textMeshPro = textObject.AddComponent<TextMeshPro>();
                    textMeshPro.text = fontAdjustHelper.testMessage;
                    textMeshPro.font = fontAdjustHelper.fontAsset;
                    textMeshPro.rectTransform.sizeDelta = fontAdjustHelper.testerTMPSize;
                    textMeshPro.fontSize = 24;
                    textMeshPro.alignment = TextAlignmentOptions.Center;
                    textMeshPro.color = Color.white;
                    fontAdjustHelper.testerTMPText = textMeshPro;
                    Undo.RegisterCreatedObjectUndo(textObject, "Create TextMeshPro Object");
                    Selection.activeGameObject = textObject;
                    EditorGUIUtility.PingObject(textObject);
                    SceneView.lastActiveSceneView.FrameSelected();
                    Selection.activeGameObject = null;
                    Selection.SetActiveObjectWithContext(fontAdjustHelper, fontAdjustHelper);
                }
            }
            else
            {
                if (GUILayout.Button("Dispose", GUILayout.ExpandWidth(false)))
                {
                   DestroyImmediate(testerTMPText.gameObject);
                }
            }
           
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            var testText = EditorGUILayout.TextArea(fontAdjustHelper.testMessage);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(fontAdjustHelper, "Change Test Text");
                fontAdjustHelper.SetTestText(testText);
            }
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Glyph  Combination List (" + combinationList.Count + ")", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add", GUILayout.ExpandWidth(false)))
            {
                Undo.RecordObject(fontAdjustHelper, "Add new combination");
                combinationList.Add(new GlyphCombination());
                ApplyChanges();
                return;
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            DrawCombinations(combinationList);
        }

        void DrawCombinations(List<GlyphCombination> list)
        {
            var i = 0;
            foreach (var combo in list)
            {
                Rect elementStartRegion = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true));
                if (DrawCombinationHeader(list, i, combo)) 
                    return;
                
                if (selectedIndex == i)
                {
                    EditorGUI.BeginDisabledGroup(!combo.isEnabled);
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                    DrawCharacterAdder("Leading Glyphs ("+ combo.first.glyphs.Length + ")", ref targetCharacters, ref combo.first);
                    DrawCharacterAdder("Following Glyphs (" + combo.second.glyphs.Length + ")",ref pairCharacters, ref combo.second);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(combo.VariantCount + " Total Pairs", EditorStyles.miniLabel,GUILayout.ExpandWidth(false));
                    GUILayout.FlexibleSpace();
                    EditorGUI.EndDisabledGroup();
                    if (GUILayout.Button("Duplicate", EditorStyles.linkLabel, GUILayout.ExpandWidth(false)))
                    {
                        Undo.RecordObject(fontAdjustHelper, "Remove combination");
                        var targetIndex = list.IndexOf(combo) + 1;
                        list.Insert(targetIndex,new GlyphCombination(combo));
                        selectedIndex = targetIndex;
                        return;
                    }
                }
                EditorGUILayout.EndVertical();
                if (selectedIndex == i)
                {
                    EditorGUILayout.EndHorizontal();
                 
                    Rect elementEndRegion = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true));
                    Rect selectionArea = new Rect(elementStartRegion.x, elementStartRegion.y, elementEndRegion.width, elementEndRegion.y - elementStartRegion.y);
                    DrawBox(selectionArea, 1f, orangeColor);
                }
                i++;
            }
        }
        
        public static void DrawBox(Rect rect, float thickness, Color color)
        {
            EditorGUI.DrawRect(new Rect(rect.x - thickness, rect.y + thickness, rect.width + thickness * 2, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.x - thickness, rect.y + thickness, thickness, rect.height - thickness * 2), color);
            EditorGUI.DrawRect(new Rect(rect.x - thickness, rect.y + rect.height - thickness * 2, rect.width + thickness * 2, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.x + rect.width, rect.y + thickness, thickness, rect.height - thickness * 2), color);
        }


        bool DrawCombinationHeader(List<GlyphCombination> list, int i, GlyphCombination combo)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            var headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 15;
            
            EditorGUI.BeginChangeCheck();
            var isEnabled = EditorGUILayout.Toggle(combo.isEnabled, GUILayout.Width(15));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(fontAdjustHelper, "Toggle Combination");
                combo.isEnabled = isEnabled;
                ApplyChanges();
            }
            
            if (GUILayout.Button((i + 1) + ". " + combo.DisplayName, headerStyle))
            {
                if (selectedIndex == i)
                    selectedIndex = -1;
                else
                    selectedIndex = i;
            }
            
            if (i == selectedIndex)
            {
                if (GUILayout.Button("\u2193", GUILayout.ExpandWidth(false)))
                {
                    Undo.RecordObject(fontAdjustHelper, "Move combination down");
                    var targetIndex = Mathf.Min(list.IndexOf(combo) + 1, list.Count - 1);
                    list.Remove(combo);
                    list.Insert(targetIndex, combo);
                    selectedIndex = targetIndex;
                    ApplyChanges();
                    return true;
                }

                if (GUILayout.Button("\u2191", GUILayout.ExpandWidth(false)))
                {
                    Undo.RecordObject(fontAdjustHelper, "Move combination up");
                    var targetIndex = Mathf.Max(list.IndexOf(combo) - 1, 0);
                    list.Remove(combo);
                    list.Insert(targetIndex, combo);
                    selectedIndex = targetIndex;
                    ApplyChanges();
                    return true;
                }
            }

            if (GUILayout.Button("x", EditorStyles.boldLabel, GUILayout.ExpandWidth(false)))
            {
                Undo.RecordObject(fontAdjustHelper, "Remove combination");
                list.Remove(combo);
                ApplyChanges();
                return true;
            }
            EditorGUILayout.EndHorizontal();
            return false;
        }

        void DrawCharacterAdder(string fieldName, ref string newCharacters, ref GlyphOffset offset)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(fieldName, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            if (!string.IsNullOrEmpty(offset.glyphs.Trim()))
                DrawPlacementAdjustor(offset);

            EditorGUI.BeginChangeCheck();

            var values = (ThaiGlyphGroup[])Enum.GetValues(typeof(ThaiGlyphGroup));
            var popups = values.Select(e => $"{ThaiLanguageInfo.GetThaiGlyphGroupName(e)} ({e})").ToArray();
            var currentIndex = Array.IndexOf(popups,  $"{ThaiLanguageInfo.GetThaiGlyphGroupName(offset.group)} ({offset.group})");
            var newIndex = EditorGUILayout.Popup("Glyph Group", currentIndex, popups);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(fontAdjustHelper, "Change Glyph Groups");
                offset.AssignGroup((ThaiGlyphGroup)newIndex);
                ApplyChanges();
            }
            
            if (offset.group == ThaiGlyphGroup.Custom)
            {
                newCharacters = EditorGUILayout.TextField("Add Glyph: " , newCharacters);
                var isInputEmpty = string.IsNullOrEmpty(newCharacters.Trim());
                if (!isInputEmpty)
                {
                    EditorGUILayout.BeginHorizontal();
                    var displayedGlyphs = newCharacters.Select(c => ThaiLanguageInfo.GetDisplayedString(c.ToString()));
                    EditorGUILayout.LabelField("Preview : ", EditorStyles.boldLabel, GUILayout.Width(95));
             
                    EditorGUILayout.LabelField(string.Join(", ", displayedGlyphs), hugeFontStyle);
                    if (GUILayout.Button("Add", GUILayout.ExpandWidth(false)))
                    {
                        Undo.RecordObject(fontAdjustHelper, "Modify Characters");
                        offset.glyphs = string.Join("",offset.glyphs.ToCharArray().Union(newCharacters.ToCharArray()));
                        newCharacters = "";
                        ApplyChanges();
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUI.indentLevel--;

            DrawGlyph(offset.group == ThaiGlyphGroup.Custom, ref offset.glyphs);
            EditorGUILayout.EndVertical();
        }

        void DrawPlacementAdjustor(GlyphOffset offset)
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

        void DrawGlyph(bool isCustomMode, ref string characters)
        {
            if (string.IsNullOrEmpty(characters))
                return;

            var cellSize = 25f;
            var cellPerRow = Mathf.FloorToInt(EditorGUIUtility.currentViewWidth / (cellSize * 2.5f));
            EditorGUILayout.BeginVertical();
            for (int i = 0; i < characters.Length; i++)
            {
                if (i == 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("", GUILayout.Width(10));
                }
             
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

                if (isCustomMode)
                {
                    if (GUILayout.Button("x", EditorStyles.boldLabel ,GUILayout.ExpandWidth(false)))
                    {
                        Undo.RecordObject(fontAdjustHelper, "Modify Characters");
                        characters = characters.Replace(character.ToString(), "");
                        ApplyChanges();
                    }
                }
               
                EditorGUILayout.EndHorizontal();

                if ((i + 1) % cellPerRow == 0)
                {
                    EditorGUILayout.EndHorizontal();
                    if (i != characters.Length - 1)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("", GUILayout.Width(10));
                    }
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