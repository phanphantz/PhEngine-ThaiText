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
        bool isPostprendFound;
        char firstCharacter;

        public override int HandleCharacter(List<StringBuilder> resultWords, string characters, int index)
        {
            firstCharacter = characters[index];
            moreCharacters.Clear();
            resultWord.Clear();
            resultWord.Append(firstCharacter);
            moreCharacters.Append(firstCharacter);
            isPostprendFound = false;
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
                    moreCharacters.Append(currentChar);
                    if (IsRequiredSpelling(currentChar))
                        continue;
                    // prepend vowels cannot be alone after first position, skip to the next char
                    if (IsPrepend(currentChar))
                        continue;
                    
                    var currentText = moreCharacters.ToString();
                    if (dicWords.Contains(currentText))
                    {
                        index = Found(j);
                        isPostprendFound = false;
                    }
                    if ((MatchingMode == MatchingMode.Shortest && isWordFound))
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
            var lastCharacter = previousWord[wordLength - 1];
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

        private bool IsRequiredSpelling(char character) => character is '\u0E31' or '\u0E37';
        private bool IsPrepend(char character)
        {
            return character == '\u0E40' || // เ
                   character == '\u0E41' || // แ
                   character == '\u0E42' || // โ
                   character == '\u0E43' || // ใ
                   character == '\u0E44';   // ไ
        }
        
        private bool IsPostpend(char character)
        {
            return character == '\u0E30' || // ะ
                   character == '\u0E32' || // า
                   character == '\u0E33' || // ำ
                   character == '\u0E34' || // ิ
                   character == '\u0E35' || // ี
                   character == '\u0E36' || // ึ
                   character == '\u0E38' || // ุ
                   character == '\u0E39' || // ู
                   character == '\u0E4C' || // ์
                   character == '\u0E47';   // ็
        }
        
        public override bool IsMatch(char character) => ThaiUnicodeCharacter.Characters.Contains(character);
    }
}