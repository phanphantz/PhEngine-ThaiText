using System.Collections.Generic;
using System.Text;

namespace ThaiStringTokenizer.Handlers
{
    public class UnknownCharacterHandler : CharacterHandlerBase, ICharacterHandler
    {
        public override int HandleCharacter(List<StringBuilder> resultWords, char[] characters, int index)
        {
            resultWords.Add(new StringBuilder(characters[index].ToString()));

            return index;
        }
    }
}