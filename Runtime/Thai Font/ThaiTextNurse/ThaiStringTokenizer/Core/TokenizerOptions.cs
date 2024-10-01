using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace ThaiStringTokenizer
{
    [Serializable]
    public class TokenizerOptions 
    {
        public List<string> customWords;
        public MatchingMode matchingMode = MatchingMode.Longest;
        public bool preferDecodableWord;
        public string customSeparator;
    }
}