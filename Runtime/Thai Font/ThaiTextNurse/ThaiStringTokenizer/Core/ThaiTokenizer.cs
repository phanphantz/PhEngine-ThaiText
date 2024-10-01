using System.Collections.Generic;
using System.Text;
using ThaiStringTokenizer.Models;

namespace ThaiStringTokenizer
{
    public class ThaiTokenizer : TokenizerBase
    {
        public string DictionaryContent { get;}
        public string Separator { get; }
        protected override string GetDictionaryContent() => DictionaryContent;

        StringBuilder result = new StringBuilder();
        
        public ThaiTokenizer(string dictionaryContent, TokenizerOptions options)
        {
            DictionaryContent = dictionaryContent;
            MatchingMode = options.matchingMode;
            PreferDecodableWord = options.preferDecodableWord;
            Separator = string.IsNullOrEmpty(options.customSeparator) ? "​" : options.customSeparator;
            InitialDictionary(options.customWords);
        }
        
        public string GetTokenizedString(string input)
        {
            var splitResultList = Split(input);
            result.Clear();
            foreach (var word in splitResultList)
            {
                result.Append(word);
                result.Append(Separator);
            }
            return result.ToString();
        }

        public List<StringBuilder> Split(string input)
        {
            var resultWords = new List<StringBuilder>();
            var handlers = GetCharacterHandlers();

            var inputWordChars = input.ToCharArray();
            var charsLength = inputWordChars.Length;

            for (int i = 0; i < charsLength; i++)
            {
                var character = inputWordChars[i];

                foreach (var handler in handlers)
                {
                    if (!handler.IsMatch(character)) 
                        continue;

                    handler.Dictionary = Dictionary;
                    handler.MatchingMode = MatchingMode;
                    i = handler.HandleCharacter(resultWords, inputWordChars, i);
                    break;
                }
            }

            return AnalyzeWords(resultWords);
        }

        private List<StringBuilder> AnalyzeWords(List<StringBuilder> resultWords)
        {
            if (!PreferDecodableWord) { return resultWords; }

            var finalResults = new List<StringBuilder>();
            var resultLength = resultWords.Count;

            for (int i = 0; i < resultLength; i++)
            {
                var word = resultWords[i];
                var isWordFound = Words.Contains(word.ToString());
                var isSpace = string.IsNullOrWhiteSpace(word.ToString());

                if (isWordFound || isSpace)
                {
                    finalResults.Add(new StringBuilder(word.ToString()));
                    continue;
                }

                var lastFinalResultIndex = finalResults.Count - 1;
                if (lastFinalResultIndex < 0)
                {
                    finalResults.Add(new StringBuilder(word.ToString()));
                    continue;
                }

                var previousWord = finalResults[lastFinalResultIndex];
                var mergeWord = $"{previousWord}{word}";
                var isMergeWordFound = Words.Contains(mergeWord);

                if (isMergeWordFound)
                {
                    finalResults[lastFinalResultIndex].Append(word);
                    continue;
                }

                finalResults.Add(word);
            }

            return finalResults;
        }
    }
}