using System.Collections.Generic;

namespace PhEngine.ThaiTMP
{
    public class PhunTokenizer
    {
        class TrieNode
        {
            public Dictionary<char, TrieNode> Children = new Dictionary<char, TrieNode>();
            public bool IsEndOfWord;
        }

        TrieNode m_Root;

        public PhunTokenizer(IEnumerable<string> dictionary)
        {
            m_Root = new TrieNode();
            foreach (var word in dictionary)
            {
                AddWord(word);
            }
        }
        
        void AddWord(string word)
        {
            var currentNode = m_Root;
            foreach (var letter in word)
            {
                if (!currentNode.Children.ContainsKey(letter))
                {
                    currentNode.Children[letter] = new TrieNode();
                }
                currentNode = currentNode.Children[letter];
            }
            currentNode.IsEndOfWord = true;
        }

        /// <summary>
        ///  Tokenize the input text by finding the longest match in the dictionary
        /// </summary>
        /// <param name="input">original text</param>
        /// <param name="isSupportRichTextTags"></param>
        /// <returns></returns>
        public List<string> Tokenize(string input, bool isSupportRichTextTags)
        {
            var tokens = new List<string>();
            int i = 0;
            while (i < input.Length)
            {
                string longestMatch = null;
                bool wasOpenBracket = false;
                bool wasThaiCharacter = false;
                TrieNode currentNode = m_Root;
                int matchLength = 0;
                var length = input.Length;
                for (int j = i; j < length; j++)
                {
                    char c = input[j];
                    if (IsOpenBracket(c))
                    {
                        wasOpenBracket = true;
                        continue;
                    }
                    
                    if (IsCloseBracket(c) || c == 'ๆ' || c == 'ฯ')
                    {
                        longestMatch = input.Substring(i, j - i+1);
                        break;
                    }
                    
                    if (IsShouldNotTokenize(c))
                    {
                        //If last character was Thai, end it first
                        if (wasThaiCharacter)
                        {
                            longestMatch = input.Substring(i, j - i);
                            break;
                        }
                        j++;
                        while (j < length)
                        {
                            c = input[j];
                            if (IsShouldNotTokenize(c))
                            {
                                //Keep going with non-tokenized characters
                                j++;
                            }
                            else
                            {
                                //We've found something recognizable again
                                break;
                            }
                        }
                        longestMatch = input.Substring(i, j - i);
                        break;
                    }
                    
                    //Try check for Rich Text Tags
                    if (isSupportRichTextTags && c == '<')
                    {
                        var k = j;
                        j++;
                        while (j < length)
                        {
                            //Keep going until the tag is closed or invalid
                            c = input[j];
                            if (c == '>')
                            {
                                j++;
                                break;
                            }
                            if (c == '<' || j >= length)
                            {
                                //Invalid tag, revert to character after open bracket
                                j = k+1;
                                break;
                            }
                            j++;
                        }
                        longestMatch = input.Substring(i, j - i);
                        break;
                    }

                    if (currentNode.Children.TryGetValue(c, out var child))
                    {
                        wasThaiCharacter = true;
                        currentNode = child;
                        matchLength++;
                        if (currentNode.IsEndOfWord)
                        {
                            if (wasOpenBracket)
                            {
                                matchLength++;
                                wasOpenBracket = false;
                            }
                            longestMatch = input.Substring(i, matchLength);
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                //Increment main index
                if (longestMatch != null)
                {
                    tokens.Add(longestMatch);
                    i += longestMatch.Length;
                }
                else
                {
                    // If no match, add single character
                    tokens.Add(input[i].ToString()); 
                    i++;
                }
            }

            return tokens;
        }

        static bool IsShouldNotTokenize(char c)
        {
            // Avoid IsDigit() and IsWhiteSpace() to gain more performance
            return ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')) ||
                   (c >= '๐' && c <= '๙') || (c >= '0' && c <= '9')||
                   (c == '~' || c == 'ๆ' || c == 'ฯ' || c == '“' || c == '”' || c == ',' || c =='.')
                   || c == ' ' || c == '\n' || c == '\r' || c == '\t';
        }

        static bool IsOpenBracket(char c)
        {
            return c == '(' || c == '{' || c == '[';
        }
        
        static bool IsCloseBracket(char c)
        {
            return c == ')' || c == '}' || c == ']';
        }
    }
}