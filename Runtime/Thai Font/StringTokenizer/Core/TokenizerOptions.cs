using System;
using System.Collections.Generic;

namespace ThaiStringTokenizer
{
    [Serializable]
    public class TokenizerOptions 
    {
        public List<string> customWords;
        public MatchingMode matchingMode = MatchingMode.Longest;
        public bool preferDecodableWord;
        public WordSeparatorOption separatorOption;
        public string customSeparatorString = "|";
    }
}