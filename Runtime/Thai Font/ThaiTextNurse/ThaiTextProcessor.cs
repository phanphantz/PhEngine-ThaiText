using System.Text;
using Lexto;
using ThaiStringTokenizer;
using UnityEngine;

namespace PhEngine.ThaiTMP
{
    public static class ThaiTextProcessor
    {
        static string dictionaryName = "dictionary";
        static TextAsset  unsafeDictionary;
        static ThaiTokenizer tokenizer;
        static TokenizerOptions options = new TokenizerOptions();
        
        public static bool TryGetDictionary(out string dictionaryContent)
        {
            dictionaryContent = null;
            if (unsafeDictionary == null)
            {
                unsafeDictionary = Resources.Load<TextAsset>(dictionaryName);
                if (unsafeDictionary == null)
                    return false;
            }

            dictionaryContent = unsafeDictionary.text;
            return true;
        }

        public static void UnloadDictionary()
        {
            Resources.UnloadAsset(unsafeDictionary);
        }

        public static string Process(string thaiText, bool isCorrectGlyphs, bool isTokenize)
        {
            if (isCorrectGlyphs)
                thaiText = CorrectDescenderGlyphs(thaiText);

            if (!isTokenize)
                return thaiText;

            if (!TryGetDictionary(out var dictionaryContent))
                return thaiText;
            
            //return LexTo.Instance.InsertLineBreaks(thaiText, '/');
            tokenizer ??= new ThaiTokenizer(dictionaryContent, options);
            return tokenizer.GetTokenizedString(thaiText);
        }

        static string CorrectDescenderGlyphs(string thaiText)
        {
            var length = thaiText.Length;
            var sb = new StringBuilder(length);
            for (var i = 0; i < length; i++)
            {
                var c = thaiText[i];
                // [YO YING] [lower] -> [YO YING w/o lower] [lower]
                if (c == '\x0E0D' && i < length - 1 && IsLower(thaiText[i + 1]))
                    c = '\xF70F';
                // [THO THAN] [lower] -> [THO THAN w/o lower] [lower]
                else if (c == '\x0E10' && i < length - 1 && IsLower(thaiText[i + 1]))
                    c = '\xF700';
                
                sb.Append(c);
            }

            thaiText = sb.ToString();
            return thaiText;
        }

        private static bool IsLower(char c)
        {
            // SARA U, SARA UU, PHINTHU
            return c >= '\x0E38' && c <= '\x0E3A';
        }
    }
}