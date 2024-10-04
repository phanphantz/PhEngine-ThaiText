using System.Collections.Generic;
using UnityEngine;

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

        // Add word to the Trie
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

        // Tokenize the input text by finding the longest match in the dictionary
        public List<string> Tokenize(string input)
        {
            var tokens = new List<string>();
            int i = 0;
            while (i < input.Length)
            {
                string longestMatch = null;
                bool wasThaiCharacter = false;
                TrieNode currentNode = m_Root;
                int matchLength = 0;
                var length = input.Length;
                for (int j = i; j < length; j++)
                {
                    char c = input[j];
                    if (IsOpenBracket(c))
                        continue;
                    
                    //Cut after close bracket
                    if (IsCloseBracket(c))
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
                            Debug.Log("while");
                        }
                        longestMatch = input.Substring(i, j - i);
                        break;
                    }
                    
                    //Try check for Rich Text Tags
                    if (c == '<')
                    {
                        var k = j;
                        j++;
                        while (j < length)
                        {
                            c = input[j];
                            if (c == '>')
                            {
                                j++;
                                break;
                            }
                            if (c == '<' || j >= length)
                            {
                                j = k+1;
                                break;
                            }
                            j++;
                            Debug.Log("while");
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
                            longestMatch = input.Substring(i, matchLength);
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
            return ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')) ||
                   (c >= '๐' && c <= '๙') || char.IsDigit(c) ||
                   (c == '~' || c == 'ๆ' || c == 'ฯ' || c == '“' || c == '”' || c == ',')
                   || (char.IsWhiteSpace(c));
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