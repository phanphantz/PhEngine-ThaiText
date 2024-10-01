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
        bool isWordFound;
        char firstCharacter;
        
        public override int HandleCharacter(List<string> resultWords, char[] characters, int index)
        {
            firstCharacter = characters[index];
            resultWord.Clear();
            resultWord.Append(firstCharacter);
            moreCharacters.Clear();
            moreCharacters.Append(firstCharacter);
            isWordFound = false;
            
            if (HandlePreviousWord(resultWords))
            {
                Append(resultWords, resultWord.ToString());
                return index;
            }
            
            if (Dictionary.TryGetValue(firstCharacter, out var dicWords))
            {
                count = characters.Length;
                for (int j = index + 1; j < count; j++)
                {
                    moreCharacters.Append(characters[j]);
                    if (dicWords.Contains(moreCharacters.ToString()))
                    {
                        isWordFound = true;
                        index = j;
                        resultWord.Clear();
                        resultWord.Append(moreCharacters);
                    }
                    if (MatchingMode == MatchingMode.Shortest && isWordFound) 
                        break;
                }
            }
            HandleResultWords(resultWords, resultWord.ToString(), isWordFound);
            return index;
        }

        private bool HandlePreviousWord(List<string> resultWords)
        {
            var lastResultIndex = resultWords.Count - 1;
            if (lastResultIndex < 0) 
                return false;

            var previousWord = resultWords[lastResultIndex];
            var wordLength = previousWord.Length;
            var lastCharacter = Convert.ToChar(previousWord[wordLength - 1]);
            return IsRequiredSpelling(lastCharacter);
        }

        private void HandleResultWords(List<string> resultWords, string resultWord, bool isWordFound)
        {
            if (isWordFound)
            {
                resultWords.Add(resultWord);
            }
            else
            {
                Append(resultWords, resultWord);
            }
        }

        static void Append(List<string> resultWords, string resultWord)
        {
            var lastResultIndex = resultWords.Count - 1;
            var previousWord = resultWords.Count == 0 ? resultWord : resultWords[lastResultIndex];
            if (string.IsNullOrWhiteSpace(previousWord) || resultWords.Count == 0)
            {
                resultWords.Add(resultWord);
            }
            else
            {
                resultWords[lastResultIndex] += resultWord;
            }
        }

        private bool IsRequiredSpelling(char character) => ThaiUnicodeCharacter.PostpendVowelsRequiredSpelling.Contains(character);

        public override bool IsMatch(char character) => ThaiUnicodeCharacter.Characters.Contains(character);
    }
}