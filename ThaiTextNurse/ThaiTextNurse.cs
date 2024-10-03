using System;
using System.Collections.Generic;
using Lexto;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PhEngine.ThaiTMP
{
    [RequireComponent(typeof(TMP_Text)), ExecuteAlways]
    public class ThaiTextNurse : MonoBehaviour, ITextPreprocessor
    {
        private static readonly Char SEPARATOR = '\u200B';
        private static readonly Char NEWLINE = '\u000A';
        private static readonly Char SPACE = '\u0020';
        private static readonly Char SPECIAL_TAG = '\uFFF0';

        private static readonly Char APPEND_NEWLINE = '\n';

        [SerializeField, HideInInspector] TMP_Text tmpText;
        [SerializeField] bool isCorrectGlyphs = true;
        [SerializeField] bool isTokenize;
        [SerializeField] string separatorPrefix;
        [SerializeField, HideInInspector] string lastKnownText;
        [SerializeField, HideInInspector] string lastProcessedText;

        bool isDirty;

        void Awake()
        {
            if (tmpText == null)
                tmpText = GetComponent<TMP_Text>();
        }

        void OnEnable()
        {
            tmpText.textPreprocessor = this;
        }

        void OnDisable()
        {
            tmpText.textPreprocessor = null;
        }

        void OnDestroy()
        {
            tmpText.textPreprocessor = null;
        }

        void OnValidate()
        {
            if (!enabled)
                return;

            NotifyChange();
        }

        void NotifyChange()
        {
            isDirty = true;
            tmpText.havePropertiesChanged = true;
        }

        public string PreprocessText(string text)
        {
            if (lastKnownText == text && !isDirty)
                return lastProcessedText;
            
            Debug.Log("Start Process");
            lastKnownText = text;
            if (isCorrectGlyphs)
                lastProcessedText = ThaiFontAdjuster.Adjust(text);

            if (!isTokenize)
                return lastProcessedText;
            
            lastProcessedText = LexTo.Instance.InsertLineBreaks(text, separatorPrefix + "​");
            isDirty = false;
            return lastProcessedText;
        }

        string ThaiWrappingText(string value, float boxwidth, FontData fontData)
        {
            List<string> htmlTag;
            string inputText = ParserTag(value, out htmlTag);
            inputText = Lexto.LexTo.Instance.InsertLineBreaks(inputText, separatorPrefix + "​");
            char[] arr = inputText.ToCharArray();
            Font font = fontData.font;
            CharacterInfo characterInfo = new CharacterInfo();
            if (font != null) {
                font.RequestCharactersInTexture(inputText, fontData.fontSize, fontData.fontStyle);
            }
            string outputText = "";
            int lineLength = 0;
            string word = "";
            int wordLength = 0;
            int SEPARATOR_Count = 0;
            foreach (char c in arr)
            {
                if (c == SEPARATOR)
                {
                    outputText = AddWordToText(outputText, lineLength, word, wordLength, boxwidth, out lineLength);
                    word = "";
                    wordLength = 0;
                    SEPARATOR_Count++;
                    continue;
                }
                else if (c == NEWLINE)
                {
                    outputText = AddNewLineToText(outputText, lineLength, word, wordLength, boxwidth, out lineLength);
                    word = "";
                    wordLength = 0;
                    continue;
                }
                else if (font != null && font.GetCharacterInfo(c, out characterInfo, fontData.fontSize))
                {
                    if (c == SPACE)
                    {
                        outputText = AddSpaceToText(outputText, lineLength, word, wordLength, characterInfo.advance, boxwidth, out lineLength);
                        word = "";
                        wordLength = 0;
                    }
                    else if (c == SPECIAL_TAG)
                    {
                        outputText = AddWordToText(outputText, lineLength, word, wordLength, boxwidth, out lineLength);
                        word = "";
                        wordLength = 0;
                        outputText += htmlTag[0];
                        htmlTag.RemoveAt(0);
                    }
                    else
                    {
                        word += c;
                        wordLength += characterInfo.advance;
                    }
                }
            }
            outputText = AddWordToText(outputText, lineLength, word, wordLength, boxwidth, out lineLength); // Add remaining word
            return outputText;
        }

        private static string ParserTag(string value, out List<string> htmlTag)
        {
            TagString[] tagArr = TagStringParser.Parser(value);
            string parserValue = "";
            htmlTag = new List<string>();
            foreach (TagString tag in tagArr)
            {
                if (tag.IsTag)
                {
                    parserValue += SPECIAL_TAG;
                    htmlTag.Add(tag.GetTagString());
                }
                else
                {
                    parserValue += tag.GetTagString();
                }
            }
            return parserValue;
        }

        private static string AddSpaceToText(string inputText, int lineLength, string word, int wordLength, int spaceWidth, float boxwidth, out int totalLength)
        {
            string outputText;
            if (lineLength + wordLength + spaceWidth <= boxwidth)
            {
                outputText = inputText + word + SPACE;
                totalLength = lineLength + wordLength + spaceWidth;
            }
            else if (lineLength + wordLength <= boxwidth)
            {
                outputText = inputText + word + APPEND_NEWLINE;
                totalLength = 0;
            }
            else
            {
                outputText = inputText + APPEND_NEWLINE + word + SPACE;
                totalLength = wordLength + spaceWidth;
            }
            return outputText;
        }

        private static string AddWordToText(string inputText, int lineLength, string word, int wordLength, float boxwidth, out int totalLength)
        {
            string outputText;
            if (lineLength + wordLength <= boxwidth)
            {
                outputText = inputText + word;
                totalLength = lineLength + wordLength;
            }
            else
            {
                outputText = inputText + APPEND_NEWLINE + word;
                totalLength = wordLength;
            }
            return outputText;
        }

        private static string AddNewLineToText(string inputText, int lineLength, string word, int wordLength, float boxwidth, out int totalLength)
        {
            string outputText;
            if (lineLength + wordLength <= boxwidth)
            {
                outputText = inputText + word + APPEND_NEWLINE;
                totalLength = 0;
            }
            else
            {
                outputText = inputText + APPEND_NEWLINE + word;
                totalLength = wordLength;
            }
            return outputText;
        }

        [ContextMenu(nameof(RebuildDict))]
        public void RebuildDict()
        {
            LexTo.Instance.TryInitialize();
            NotifyChange();
        }
    }
    
    public enum TokenizationSeparator
    {
        ZWSP, SlashAndZWSP
    }
}