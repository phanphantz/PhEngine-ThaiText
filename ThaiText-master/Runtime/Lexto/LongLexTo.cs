// LongLexTo: Tokenizing Thai texts using Longest Matching Approach
//   Note: Types: 0=unknown  1=known  2=ambiguous  3=English/digits  4=special characters
//
// Public methods:
//   1) public LongLexTo(File dictFile);	    //Constructor with a dictionary file
//   2) public void addDict(File dictFile);     //Add dictionary (e.g., unknown-word file)
//   3) public void wordInstance(String text);  //Word tokenization
//   4) public void lineInstance(String text);  //Line-break tokenization
//   4) public Vector getIndexList();
//   5) Iterator's public methods: hasNext, first, next
//
// Author: Choochart Haruechaiyasak
// Last update: 28 March 2006

using System;
using System.Collections;
using System.IO;

namespace Lexto
{
    class LongLexTo
    {
        //Private variables
        private Trie dict;               //For storing words from dictionary
        private LongParseTree ptree;     //Parsing tree (for Thai words)

        //Returned variables
        private ArrayList indexList;  //List of word index positions
        private ArrayList lineList;   //List of line index positions
        private ArrayList typeList;   //List of word types (for word only)
        private IEnumerator iter; //Iterator for indexList OR lineList (depends on the call)

        /*******************************************************************/
        /*********************** Return index list *************************/
        /*******************************************************************/
        public ArrayList GetIndexList()
        {
            return indexList;
        }

        /*******************************************************************/
        /*********************** Return type list *************************/
        /*******************************************************************/
        public ArrayList GetTypeList()
        {
            return typeList;
        }

        /*******************************************************************/
        /******************** Iterator for index list **********************/
        /*******************************************************************/
        //Return iterator's hasNext for index list
        public Boolean HasNext()
        {
            if (!iter.MoveNext())
            {
                return false;
            }
            return true;
        }

        //Return iterator's first index
        public int First()
        {
            return 0;
        }

        //Return iterator's next index
        public int Next()
        {

            return (short)iter.Current;
        }

        /*******************************************************************/
        /********************** Constructor (default) **********************/
        /*******************************************************************/

        public LongLexTo()
        {
            dict = new Trie();
            indexList = new ArrayList();
            lineList = new ArrayList();
            typeList = new ArrayList();
            ptree = new LongParseTree(dict, indexList, typeList);
        }

        public LongLexTo(String input)
        {
            dict = new Trie();
            indexList = new ArrayList();
            lineList = new ArrayList();
            typeList = new ArrayList();
            ptree = new LongParseTree(dict, indexList, typeList);
        }

        public LongLexTo(StreamReader input)
        {
            dict = new Trie();
            if (input!=null)
            {
                AddDict(input);
            }
            indexList = new ArrayList();
            lineList = new ArrayList();
            typeList = new ArrayList();
            ptree = new LongParseTree(dict, indexList, typeList);
        }



        //...

        /*******************************************************************/
        /**************************** addDict ******************************/
        /*******************************************************************/
        public void AddDict(String line)
        {
            line = line.Trim();
            if (line.Length > 0)
            {
                dict.add(line);
            }
        }

        public void AddDict(StreamReader dictFile) //File dictFile
        {
            //Read words from dictionary
            String line = ""; //, word, word2;
            using (StreamReader sr = dictFile)
            {
                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Trim();
                    // ignore if start with #
                    if (line.Length > 0 && !line.StartsWith("#"))
                    {
                        dict.add(line);
                    }
                }
            }
        } //addDict

        /****************************************************************/
        /************************** wordInstance ************************/
        /****************************************************************/

        public void WordInstance(String text)
        {
            //System.out.println("I'm In wordInStance");
            indexList.Clear();
            typeList.Clear();
            int pos;
            char ch;
            pos = 0;
            while (pos < text.Length)
            {
                //Check for special characters and English words/numbers
                ch = text[pos];
                //English
                if (((ch >= 'A') && (ch <= 'Z')) || ((ch >= 'a') && (ch <= 'z')))
                {
                    while ((pos < text.Length) && (((ch >= 'A') && (ch <= 'Z')) || ((ch >= 'a') && (ch <= 'z'))))
                    {
                        ch = text[pos++];
                    }
                    if (pos < text.Length)
                    {
                        pos--;
                    }
                    indexList.Add((short)pos);
                    typeList.Add((short)3);
                }
                //Digits
                else if (((ch >= '0' && ch <= '9')) || ((ch >= '๐') && (ch <= '๙')))
                {
                    while ((pos < text.Length) && (((ch >= '0') && (ch <= '9')) || ((ch >= '๐') && (ch <= '๙')) || (ch == ',') || (ch == '.')))
                    {
                        ch = text[pos++];
                    }
                    if (pos < text.Length)
                    {
                        pos--;
                    }
                    indexList.Add((short)pos);
                    typeList.Add((short)3);
                }
                //Special characters
                else if ((ch <= '~') || (ch == 'ๆ') || (ch == 'ฯ') || (ch == '“') || (ch == '”') || (ch == ','))
                {
                    pos++;
                    indexList.Add((short)pos);
                    typeList.Add((short)4);
                }
                //Thai word (known/unknown/ambiguous)
                else
                {
                    pos = ptree.parseWordInstance(pos, text);
                }
            }//While all text length
            iter = (IEnumerator)indexList.GetEnumerator();
        } //wordInstance
        
    }//class LongLexTo
}