using System.Collections.Generic;
using System.Text;

namespace ThaiStringTokenizer.Handlers
{
    public interface ICharacterHandler
    {
        Dictionary<char, HashSet<string>> Dictionary { get; set; }

        MatchingMode MatchingMode { get; set; }

        bool IsMatch(char character);

        int HandleCharacter(List<StringBuilder> resultWords, char[] characters, int index);
    }
}