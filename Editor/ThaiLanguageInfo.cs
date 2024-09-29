using System;
using System.Linq;

namespace PhEngine.UI.ThaiText.Editor
{
    public static class ThaiLanguageInfo
    {
        public static char[] behindDashGlyphs => allFollowingVowels
                .Concat(lowerVowels)
                .Concat(allFollowingVowels)
                .Concat(allUpperGlyphs)
                .ToArray();

        public static readonly char[] lowerVowels = new[] { 'ุ', 'ู'};
        public static readonly char[] allFollowingVowels = new[] {'ะ', 'ำ', 'า', 'ๅ'};
        public static readonly char[] leadingVowels = new[] {'เ', 'แ', 'โ', 'ไ', 'ใ'};
        public static readonly char[] upperVowels = new[] { 'ิ', 'ี', 'ึ', 'ื', '็', 'ั'};
        
        public static readonly char[] allUpperGlyphs = new[] { 'ิ', 'ี', 'ึ', 'ื', '็', 'ั', '์', '่', '้', '๊', '๋'};
        public static readonly char[] toneMarks = new[] {'่', '้', '๊', '๋'};
        public static readonly char thanThaKhaat = '์';
        public static readonly char saraAum = 'ำ';
        
        public static readonly char[] allConsonants = new char[]
        {
            'ก', 'ข', 'ฃ', 'ค', 'ฅ', 'ฆ', 'ง', 'จ', 'ฉ', 'ช',
            'ซ', 'ฌ', 'ญ', 'ฎ', 'ฏ', 'ฐ', 'ฑ', 'ฒ', 'ณ', 'ด',
            'ต', 'ถ', 'ท', 'ธ', 'น', 'บ', 'ป', 'ผ', 'ฝ', 'พ',
            'ฟ', 'ภ', 'ม', 'ย', 'ร', 'ล', 'ว', 'ศ', 'ษ', 'ส',
            'ห', 'ฬ', 'อ', 'ฮ'
        };

        public static readonly char[] descenderConsonants = new char[]
        {
            'ฎ', 'ฏ'
        };

        public static readonly char[] ascenderConsonants = new char[]
        {
            'ป', 'ฝ', 'ฟ', 'ฬ'
        };

        public static string GetDisplayedString(string characters)
        {
            characters = characters.Trim();
            if (string.IsNullOrEmpty(characters))
                return "";
            
            if (characters.Length > 1)
                return characters;

            if (behindDashGlyphs.Contains(characters[0]))
                return "-" + characters[0];

            if (leadingVowels.Contains(characters[0]))
                return characters[0] + "-";

            return characters;
        }

        public static string GetThaiGlyphGroupName(ThaiGlyphGroup group)
        {
            switch (group)
            {
                case ThaiGlyphGroup.LowerVowels:
                    return "สระล่าง";
                case ThaiGlyphGroup.AllUpperGlyphs:
                    return "อักขระด้านบนทั้งหมด";
                case ThaiGlyphGroup.AllFollowingVowels:
                    return "สระหลัง";
                case ThaiGlyphGroup.LeadingVowels:
                    return "สระหน้า";
                case ThaiGlyphGroup.UpperVowels:
                    return "สระบน";
                case ThaiGlyphGroup.ToneMarks:
                    return "วรรณยุกต์";
                case ThaiGlyphGroup.ThanThaKhaat:
                    return "ทัณฑฆาต";
                case ThaiGlyphGroup.SaraAum:
                    return "สระอำ";
                case ThaiGlyphGroup.AllConsonants:
                    return "ก - ฮ";
                case ThaiGlyphGroup.DescenderConsonants:
                    return "พยัญชนะหางล่าง";
                case ThaiGlyphGroup.AscenderConsonants:
                    return "พยัญชนะหางบน";
                case ThaiGlyphGroup.Custom:
                    return "กำหนดเอง";
                default:
                    throw new ArgumentOutOfRangeException(nameof(group), group, null);
            }
        }
        
        public static char[] GetGlyphsOf(ThaiGlyphGroup group)
        {
            switch (group)
            {
                case ThaiGlyphGroup.AllConsonants:
                    return allConsonants;
                case ThaiGlyphGroup.AscenderConsonants:
                    return ascenderConsonants;
                case ThaiGlyphGroup.DescenderConsonants:
                    return descenderConsonants;
                case ThaiGlyphGroup.AllUpperGlyphs:
                    return allUpperGlyphs;
                case ThaiGlyphGroup.UpperVowels:
                    return upperVowels;
                case ThaiGlyphGroup.ToneMarks:
                    return toneMarks;
                case ThaiGlyphGroup.ThanThaKhaat:
                    return new char[]{thanThaKhaat};
                case ThaiGlyphGroup.LeadingVowels:
                    return leadingVowels;
                case ThaiGlyphGroup.AllFollowingVowels:
                    return allFollowingVowels;
                case ThaiGlyphGroup.SaraAum:
                    return new char[]{saraAum};
                case ThaiGlyphGroup.LowerVowels:
                    return lowerVowels;
                case ThaiGlyphGroup.Custom:
                    return new char[] { };
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(group), group, null);
            }
        }
    }

    public enum ThaiGlyphGroup
    {
        AllConsonants, 
        AscenderConsonants,
        DescenderConsonants, 
        AllUpperGlyphs, 
        UpperVowels, 
        ToneMarks, 
        ThanThaKhaat, 
        LeadingVowels, 
        AllFollowingVowels, 
        SaraAum, 
        LowerVowels, 
        Custom
    }
}