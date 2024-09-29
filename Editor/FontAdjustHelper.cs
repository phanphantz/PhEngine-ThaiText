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
        public TMP_Text testerTMPText;
        public Vector2 testerTMPSize = new Vector2(50f, 25f);
        public string testMessage = @"ปิ่น อื้อ จี๊ด อื้ม ปรื๋อ ผื่น ลิ้น 
            ติ๋ม ปริ่ม หั่น ปั้น ตั๊ก ป้า ม๊า 
            ฝ่า ป่า ฟ้า ผ่า ผ้า จ๋า สิทธิ์ 
            ย่ำ ถ้ำ ฎุ ฎูำ";

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
                fontAsset.fontFeatureTable.glyphPairAdjustmentRecords.RemoveAll(r =>
                    r.firstAdjustmentRecord.glyphIndex == pairRecord.firstAdjustmentRecord.glyphIndex &&
                    r.secondAdjustmentRecord.glyphIndex == pairRecord.secondAdjustmentRecord.glyphIndex);

            cachedPairList.Clear();
        }

        void InjectModifiedPairs()
        {
            var enabledCombinations = glyphCombinationList.Where(c => c.isEnabled).ToArray();
            foreach (var glyphCouple in enabledCombinations)
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
            ApplyToTesterText();
        }

        public void ApplyToTesterText()
        {
            if (testerTMPText)
                testerTMPText.text = testMessage;
        }

        private TMP_GlyphPairAdjustmentRecord GetPairAdjustmentRecord(uint firstGlyphIndex, uint secondGlyphIndex)
        {
            var adjustmentRecords = fontAsset.fontFeatureTable.glyphPairAdjustmentRecords;
            foreach (var record in adjustmentRecords)
            {
                if (record.firstAdjustmentRecord.glyphIndex == firstGlyphIndex &&
                    record.secondAdjustmentRecord.glyphIndex == secondGlyphIndex)
                    return record;
            }

            var firstAdjustment = new TMP_GlyphAdjustmentRecord(firstGlyphIndex, new TMP_GlyphValueRecord());
            var secondAdjustment = new TMP_GlyphAdjustmentRecord(secondGlyphIndex, new TMP_GlyphValueRecord());
            var newPairRecord = new TMP_GlyphPairAdjustmentRecord(firstAdjustment, secondAdjustment);
            fontAsset.fontFeatureTable.glyphPairAdjustmentRecords.Add(newPairRecord);
            return newPairRecord;
        }

        public void CleanRebuild()
        {
            fontAsset.fontFeatureTable.glyphPairAdjustmentRecords.Clear();
            EditorUtility.SetDirty(fontAsset);
            ApplyModifications();
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

        static TMP_GlyphValueRecord ApplyPlacement(GlyphOffset offset, TMP_GlyphAdjustmentRecord secondRecord)
        {
            var record = secondRecord.glyphValueRecord;
            record.xPlacement = offset.xPlacement;
            record.yPlacement = offset.yPlacement;
            return record;
        }

        public static bool TryGetGlyphIndex(TMP_FontAsset fontAsset, char character, out uint glyphIndex,
            out int characterIndex)
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

        public void SetTesterTMPText(TMP_Text tmpText)
        {
            testerTMPText = tmpText;
            testerTMPText.font = fontAsset;
            ApplyToTesterText();
        }

        public void SetTestText(string value)
        {
            testMessage = value;
            ApplyToTesterText();
        }
    }
}