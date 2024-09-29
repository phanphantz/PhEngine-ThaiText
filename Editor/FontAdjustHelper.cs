using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace PhEngine.UI.ThaiText.Editor
{
    [CreateAssetMenu(menuName = "Create FontAdjustHelper", fileName = "FontAdjustHelper", order = 0)]
    public class FontAdjustHelper : ScriptableObject
    {
        public TMP_FontAsset fontAsset;
        public List<GlyphCombination> glyphCombinationList;

        [HideInInspector, SerializeField]
        List<TMP_GlyphPairAdjustmentRecord> cachedPairList = new List<TMP_GlyphPairAdjustmentRecord>();

        [ContextMenu(nameof(ApplyModifications))]
        public void ApplyModifications()
        {
            if (fontAsset == null)
                return;
            
            Undo.RecordObject(fontAsset, "Modify Pair Adjustments");
            DisposeCachedPairs();
            InjectModifiedPairs();
            ApplyChanges();
        }
        
        void DisposeCachedPairs()
        {
            foreach (var pairRecord in cachedPairList)
                fontAsset.fontFeatureTable.glyphPairAdjustmentRecords.Remove(pairRecord);

            cachedPairList.Clear();
        }
        
        void InjectModifiedPairs()
        {
            foreach (var glyphCouple in glyphCombinationList)
            {
                var targetCharacters = glyphCouple.first.glyphs.Trim();
                var pairCharacters = glyphCouple.second.glyphs.Trim();
                foreach (var targetCharacter in targetCharacters)
                {
                    if (!TryGetGlyphIndex(fontAsset, targetCharacter, out var targetGlyphIndex, out _))
                        continue;
                    
                    foreach (var pairCharacter in pairCharacters)
                    {
                        if (!TryGetGlyphIndex(fontAsset, pairCharacter, out var pairGlyphIndex, out _))
                            continue;
                        
                        var targetPair = GetPairAdjustmentRecord(targetGlyphIndex, pairGlyphIndex);
                        ModifyPairGlyph(targetPair, glyphCouple);
                    }
                }
            }
        }

        void ApplyChanges()
        {
            TMPro_EventManager.ON_FONT_PROPERTY_CHANGED(true, fontAsset);
            fontAsset.ReadFontAssetDefinition();
            EditorUtility.SetDirty(fontAsset);
        }

        private TMP_GlyphPairAdjustmentRecord GetPairAdjustmentRecord(uint firstGlyphIndex, uint secondGlyphIndex)
        {
            var adjustmentRecords = fontAsset.fontFeatureTable.glyphPairAdjustmentRecords;
            foreach (var record in adjustmentRecords)
            {
                if (record.firstAdjustmentRecord.glyphIndex == firstGlyphIndex && record.secondAdjustmentRecord.glyphIndex == secondGlyphIndex)
                    return record;
            }

            var firstAdjustment = new TMP_GlyphAdjustmentRecord(firstGlyphIndex, new TMP_GlyphValueRecord());
            var secondAdjustment = new TMP_GlyphAdjustmentRecord(secondGlyphIndex, new TMP_GlyphValueRecord());
            var newPairRecord = new TMP_GlyphPairAdjustmentRecord(firstAdjustment, secondAdjustment);
            fontAsset.fontFeatureTable.glyphPairAdjustmentRecords.Add(newPairRecord);
            return newPairRecord;
        }

        public void ClearAllExistingPairs()
        {
            fontAsset.fontFeatureTable.glyphPairAdjustmentRecords.Clear();
        }

        private void ModifyPairGlyph(TMP_GlyphPairAdjustmentRecord pairRecord, GlyphCombination glyphCombination)
        {
            var firstRecord = pairRecord.firstAdjustmentRecord;
            firstRecord.glyphValueRecord = ApplyPlacement(glyphCombination.first, firstRecord);
            
            var secondRecord = pairRecord.secondAdjustmentRecord;
            secondRecord.glyphValueRecord = ApplyPlacement(glyphCombination.second, secondRecord);
            
            var modifiedPair = new TMP_GlyphPairAdjustmentRecord(firstRecord, secondRecord);
            fontAsset.fontFeatureTable.glyphPairAdjustmentRecords.Remove(pairRecord);
            fontAsset.fontFeatureTable.glyphPairAdjustmentRecords.Add(modifiedPair);
            cachedPairList.Add(modifiedPair);
        }

        static TMP_GlyphValueRecord ApplyPlacement(GlypthOffset offset, TMP_GlyphAdjustmentRecord secondRecord)
        {
            var record = secondRecord.glyphValueRecord;
            record.xPlacement = offset.xPlacement;
            record.yPlacement = offset.yPlacement;
            return record;
        }

        public static bool TryGetGlyphIndex(TMP_FontAsset fontAsset, char character, out uint glyphIndex, out int characterIndex)
        {
            glyphIndex = 0;
            characterIndex = -1;
            TMP_Character characterData = fontAsset.characterTable.FirstOrDefault(c => c.unicode == character);
            if (characterData != null)
            {
                glyphIndex = characterData.glyphIndex;
                characterIndex = fontAsset.characterTable.IndexOf(characterData);
                return true;
            }
            return false;
        }
    }
}