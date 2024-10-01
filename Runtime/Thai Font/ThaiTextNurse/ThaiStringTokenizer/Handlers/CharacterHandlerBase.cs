using System.Collections.Generic;
using System.Text;

namespace ThaiStringTokenizer.Handlers
{
    public abstract class CharacterHandlerBase
    {
        public virtual Dictionary<char, HashSet<string>> Dictionary { get; set; } = new Dictionary<char, HashSet<string>>();

        public virtual MatchingMode MatchingMode { get; set; }
        
        public virtual int HandleCharacter(List<StringBuilder> resultWords, char[] characters, int index)
        {
            var resultWord = characters[index].ToString();

            for (int j = index + 1; j < characters.Length; j++)
            {
                if (IsMatch(characters[j]))
                {
                    resultWord += characters[j];
                    index = j;
                }
                else
                {
                    break;
                }
            }

            resultWords.Add(new StringBuilder(resultWord));
            return index;
        }

        public virtual bool IsMatch(char character) => true;
    }
}