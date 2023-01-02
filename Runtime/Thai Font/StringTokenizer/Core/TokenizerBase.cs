using System;
using System.Collections.Generic;
using System.Linq;
using ThaiStringTokenizer.Handlers;

namespace ThaiStringTokenizer
{
    public abstract class TokenizerBase
    {
        public Dictionary<char, List<string>> Dictionary { get; protected set; } = new Dictionary<char, List<string>>();

        public MatchingMode MatchingMode { get; protected set; }

        public List<string> Words { get; protected set; }

        public bool PreferDecodableWord { get; protected set; }

        public List<ICharacterHandler> GetCharacterHandlers()
        {
            return new List<ICharacterHandler>
            {
                new EnglishCharacterHandler(),
                new NumberCharacterHandler(),
                new ThaiCharacterHandler(),
                new UnknownCharacterHandler()
            };
        }

        private List<string> InitialWords(List<string> customWords = null)
        {
            var textWords = GetDictionaryContent();
            Words = textWords.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();

            if (customWords != null && customWords.Any()) { Words.InsertRange(0, customWords); }

            return Words;
        }

        protected abstract string GetDictionaryContent();

        public void InitialDictionary(List<string> customWords = null)
        {
            var words = InitialWords(customWords);

            foreach (var word in words)
            {
                var firstCharacter = word[0];

                if (!Dictionary.ContainsKey(firstCharacter))
                {
                    Dictionary.Add(firstCharacter, new List<string>());
                }

                Dictionary[firstCharacter].Add(word);
            }
        }
    }
}