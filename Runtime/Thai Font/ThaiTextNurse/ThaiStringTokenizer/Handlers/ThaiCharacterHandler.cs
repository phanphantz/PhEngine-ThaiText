using System;
using System.Collections.Generic;
using System.Text;
using ThaiStringTokenizer.Characters;

namespace ThaiStringTokenizer.Handlers
{
    public class ThaiCharacterHandler : CharacterHandlerBase, ICharacterHandler
    {
        private StringBuilder resultWord = new StringBuilder();
        private StringBuilder moreCharacters = new StringBuilder();
        int count;
        int wordCount;
        bool isWordFound;
        char firstCharacter;
        
        public override int HandleCharacter(List<StringBuilder> resultWords, char[] characters, int index)
        {
            firstCharacter = characters[index];
            resultWord.Clear();
            resultWord.Append(firstCharacter);
            moreCharacters.Clear();
            moreCharacters.Append(firstCharacter);
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
                    var currentCharacter = characters[j];
                    moreCharacters.Append(currentCharacter);
                    if (dicWords.Contains(moreCharacters.ToString()))
                    {
                        isWordFound = true;
                        index = j;
                        resultWord.Clear();
                        wordCount++;
                        resultWord.Append(moreCharacters);
                    }
                    if ((MatchingMode == MatchingMode.Shortest && isWordFound) || wordCount >= 3) 
                        break;
                }
            }
            HandleResultWords(resultWords);
            return index;
        }

        private bool HandlePreviousWord(List<StringBuilder> resultWords)
        {
            var lastResultIndex = resultWords.Count - 1;
            if (lastResultIndex < 0) 
                return false;

            var previousWord = resultWords[lastResultIndex];
            var wordLength = previousWord.Length;
            var lastCharacter = Convert.ToChar(previousWord[wordLength - 1]);
            return IsRequiredSpelling(lastCharacter);
        }

        private void HandleResultWords(List<StringBuilder> resultWords)
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

        private bool IsRequiredSpelling(char character) => ThaiUnicodeCharacter.PostpendVowelsRequiredSpelling.Contains(character);

        public override bool IsMatch(char character) => ThaiUnicodeCharacter.Characters.Contains(character);
    }
}