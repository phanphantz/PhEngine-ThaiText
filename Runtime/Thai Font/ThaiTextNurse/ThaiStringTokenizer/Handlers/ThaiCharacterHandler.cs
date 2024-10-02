using System;
using System.Collections.Generic;
using System.Text;
using ThaiStringTokenizer.Characters;

namespace ThaiStringTokenizer.Handlers
{
    public class ThaiCharacterHandler : CharacterHandlerBase, ICharacterHandler
    {
        private StringBuilder resultWord = new StringBuilder();
        private string moreCharacters;
        int count;
        int wordCount;
        bool isWordFound;
        char firstCharacter;

        public override int HandleCharacter(List<StringBuilder> resultWords, string characters, int index)
        {
            var startIndex = index;
            firstCharacter = characters[index];
            resultWord.Clear();
            resultWord.Append(firstCharacter);
            isWordFound = false;
            wordCount = 0;

            if (HandlePreviousWord(resultWords))
            {
                Append(resultWords);
                return index;
            }

            if (Dictionary.TryGetValue(firstCharacter, out var dicWords))
            {
                count = characters.Length;
                for (int j = index + 1; j < count; j++)
                {
                    var currentChar = characters[j];
                    // prepend vowels cannot be alone after first position, skip to the next char
                    if (ThaiUnicodeCharacter.PrependVowels.Contains(currentChar))
                        continue;
                    if (IsRequiredSpelling(currentChar))
                        continue;

                    moreCharacters = characters.Substring(startIndex, (j - startIndex) + 1);
                    if (dicWords.Contains(moreCharacters))
                    {
                        index = Found(j);
                    }

                    if ((MatchingMode == MatchingMode.Shortest && isWordFound) || wordCount >= 3)
                        break;
                }
            }

            HandleResultWords(resultWords);
            return index;
        }

        int Found(int j)
        {
            isWordFound = true;
            resultWord.Clear();
            resultWord.Append(moreCharacters);
            wordCount++;
            return j;
        }

        bool HandlePreviousWord(List<StringBuilder> resultWords)
        {
            var lastResultIndex = resultWords.Count - 1;
            if (lastResultIndex < 0)
                return false;

            var previousWord = resultWords[lastResultIndex];
            var wordLength = previousWord.Length;
            var lastCharacter = Convert.ToChar(previousWord[wordLength - 1]);
            return IsRequiredSpelling(lastCharacter);
        }

        void HandleResultWords(List<StringBuilder> resultWords)
        {
            if (isWordFound)
            {
                resultWords.Add(new StringBuilder().Append(resultWord));
            }
            else
            {
                Append(resultWords);
            }
        }

        void Append(List<StringBuilder> resultWords)
        {
            var lastResultIndex = resultWords.Count - 1;
            var previousWord = resultWords.Count == 0 ? resultWord : resultWords[lastResultIndex];
            if (previousWord.Length == 0 || resultWords.Count == 0)
            {
                resultWords.Add(new StringBuilder().Append(resultWord));
            }
            else
            {
                resultWords[lastResultIndex].Append(resultWord);
            }
        }

        private bool IsRequiredSpelling(char character) =>
            ThaiUnicodeCharacter.PostpendVowelsRequiredSpelling.Contains(character);

        public override bool IsMatch(char character) => ThaiUnicodeCharacter.Characters.Contains(character);
    }
}