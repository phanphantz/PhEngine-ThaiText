using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PhEngine.ThaiTMP
{
    [RequireComponent(typeof(TMP_Text)), ExecuteAlways]
    public class ThaiTextNurse : MonoBehaviour, ITextPreprocessor
    {
        public ThaiGlyphCorrection Correction
        {
            get => correction;
            set
            {
                correction = value;
                NotifyChange();
            }
        }
        [SerializeField] ThaiGlyphCorrection correction;
        
        public bool IsTokenize
        {
            get => isTokenize;
            set
            {
                isTokenize = value;
                NotifyChange();
            }
        }
        [SerializeField] bool isTokenize = true;
        
        public string Separator
        {
            get => separator;
            set
            {
                separator = value;
                NotifyChange();
            }
        }
        [SerializeField] string separator;
        
        [SerializeField] TMP_Text tmpText;
        [SerializeField, HideInInspector] string lastKnownText;
        
        public string OutputString => outputString;
        [SerializeField, HideInInspector] string outputString;
        
        public int LastWordCount => lastWordCount;
        [SerializeField, HideInInspector] int lastWordCount;
        
        public bool isVisualizeWordBreaks = true;
        public Color guiColor = new Color(0f, 0.5f, 0.8f);
        
        static BaengPhunTokenizer tokenizer;
        bool isRebuildRequired;

        void Awake()
        {
            if (tmpText == null)
                tmpText = GetComponent<TMP_Text>();
            isRebuildRequired = true;
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
        
        public void NotifyChange()
        {
            isRebuildRequired = true;
            tmpText.havePropertiesChanged = true;
        }
        
        public string PreprocessText(string text)
        {
            if (lastKnownText == text && !isRebuildRequired)
                return outputString;
            
            lastKnownText = text;
            isRebuildRequired = false;
            
            //Sanity Check
            //Debug.Log(gameObject.name + " : Rebuild Display String");
            return RebuildOutputString(text);
        }

        string RebuildOutputString(string text)
        {
            outputString = text;
            if (correction != ThaiGlyphCorrection.None)
                outputString = ThaiFontAdjuster.Adjust(text, correction);

            if (!isTokenize)
                return outputString;

            return Tokenize();
        }

        string Tokenize()
        {
            var request = new TokenizeRequest(outputString, separator, tmpText.enableWordWrapping, tmpText.richText);
            if (TryTokenize(request, out var result))
            {
                lastWordCount = result.WordCount;
                outputString = result.Result;
            }
            return outputString;
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!isVisualizeWordBreaks)
                return;
            
            if (string.IsNullOrEmpty(outputString))
                return;
            
            var settings = ThaiTextNurseSettings.PrepareInstance();
            var breakCharacter = GetWordBreakCharacter(settings);
            if (string.IsNullOrEmpty(breakCharacter))
                return;
            
            var breakIndices = new List<int>();
            for (int i = 0; i < outputString.Length; i++)
            {
                if (outputString[i] == breakCharacter[0])
                    breakIndices.Add(i);
            }
            var oldColor = Handles.color;
            var color = guiColor;
            color.a = 0.75f;
            Handles.color = color;
            var widthScale = transform.lossyScale.x;
            
            // 0.1f seems to be a magic number that makes the height scale looks correct for Worldspace texts.
            // Why? I don't know... Unity magic?
            var heightScale = transform.lossyScale.y * (tmpText is TextMeshProUGUI ? 1f : 0.1f); 
            foreach (int index in breakIndices)
            {
                var charInfo = GetCharacterInfo(index);
                if (charInfo.character == '<')
                {
                    var i = index;
                    i++;
                    while (i < tmpText.textInfo.characterInfo.Length)
                    {
                        var character = GetCharacterInfo(i).character;
                        if (character == '>')
                        {
                            break;
                        }
                        else if (character == '<')
                        {
                            
                        }
                    }
                }
                Vector3 pos = transform.TransformPoint(charInfo.bottomRight);
                float lineHeight = charInfo.pointSize * heightScale;
                Handles.DrawLine(pos, pos + Vector3.up * lineHeight, 3f * widthScale);
            }
            Handles.color = oldColor;
        }

        TMP_CharacterInfo GetCharacterInfo(int index)
        {
            return tmpText.textInfo.characterInfo[index];
        }
#endif
        
        #region Static Methods
        
        public static string SafeTokenize(string input)
        {
            return SafeTokenize(new TokenizeRequest(input));
        }

        public static string SafeTokenize(TokenizeRequest request)
        {
            return TryTokenize(request, out var result) ? result.Result : request.Input;
        }
        
        public static bool TryTokenize(string input, out TokenizeResult result)
        {
            return TryTokenize(new TokenizeRequest(input), out result);
        }
        
        public static bool TryTokenize(TokenizeRequest tokenizeRequest, out TokenizeResult result)
        {
            result = null;
            var settings = ThaiTextNurseSettings.PrepareInstance();
            if (tokenizer == null && !TryRebuildDictionary(settings))
                return false;
            
            if (tokenizer == null)
                return false;
            
            var wordBreakCharacter = GetWordBreakCharacter(settings);
            var finalSeparator = tokenizeRequest.Separator;
            if (tokenizeRequest.IsBreakWords)
                finalSeparator += wordBreakCharacter;
           
            if (string.IsNullOrEmpty(finalSeparator))
                return false;

            //Remove all existing word break characters
            var input = tokenizeRequest.Input;
            if (input.Contains(wordBreakCharacter))
                input = input.Replace(wordBreakCharacter, string.Empty);
            
            var words = tokenizer.Tokenize(input,  tokenizeRequest.IsSupportRichText);
            result = new TokenizeResult(string.Join(finalSeparator, words), words.Count);
            return true;
        }

        static string GetWordBreakCharacter(ThaiTextNurseSettings settings)
        {
            return settings ? settings.WordBreakCharacter : "​";
        }

        public static void RebuildDictionary(bool isUpdateNursesInScene = true)
        {
            if (!TryRebuildDictionary(ThaiTextNurseSettings.PrepareInstance()))
                throw new InvalidOperationException("Failed to setup Dictionary for ThaiTextNurse");

            if (isUpdateNursesInScene)
                UpdateAllNursesInScene();
        }

        public static void UpdateAllNursesInScene()
        {
            var existingNurses = FindObjectsOfType<ThaiTextNurse>();
            foreach (var nurse in existingNurses)
                nurse.NotifyChange();
        }
        
        public static void EnableAllTokenizerInScene()
        {
            var existingNurses = FindObjectsOfType<ThaiTextNurse>();
            foreach (var nurse in existingNurses)
                nurse.IsTokenize = true;
        }

        public static void DisableAllTokenizerInScene()
        {
            var existingNurses = FindObjectsOfType<ThaiTextNurse>();
            foreach (var nurse in existingNurses)
                nurse.IsTokenize = false;
        }

        public static IEnumerator RebuildDictionaryAsync(bool isUpdateNursesInScene = true, Action<float> onProgress = null, Action onFail = null)
        {
            var settings = ThaiTextNurseSettings.PrepareInstance();
            var path = GetDictionaryPath(settings);
            var request = Resources.LoadAsync<TextAsset>(path);
            while (!request.isDone)
            {
                onProgress?.Invoke(request.progress);
                yield return new WaitForEndOfFrame();
            }
            
            var textAsset = request.asset as TextAsset;
            if (textAsset == null)
            {
                Debug.LogError("Cannot find any dictionary under Resources Folder path : " + path);
                onFail?.Invoke();
                yield break;
            }
            RebuildTokenizer(textAsset);
            if (isUpdateNursesInScene)
                UpdateAllNursesInScene();
        }

        static bool TryRebuildDictionary(ThaiTextNurseSettings settings)
        {
            if (!TryLoadDictionaryAsset(settings, out var textAsset)) 
                return false;
            
            RebuildTokenizer(textAsset);
            return true;
        }

        public static bool TryLoadDictionaryAsset(ThaiTextNurseSettings settings, out TextAsset textAsset)
        {
            var path = GetDictionaryPath(settings);
            textAsset = Resources.Load<TextAsset>(path);
            if (textAsset == null)
            {
                Debug.LogError("Cannot find any dictionary under Resources Folder path : " + path);
                return false;
            }
            return true;
        }

        static string GetDictionaryPath(ThaiTextNurseSettings settings)
        {
            return settings? settings.DictionaryResourcePath : "dictionary";
        }

        static void RebuildTokenizer(TextAsset textAsset)
        {
            tokenizer = new BaengPhunTokenizer(WordsFromDictionary(textAsset));
            Resources.UnloadAsset(textAsset);
            Debug.Log("[ThaiTextNurse] Dictionary Rebuild Completed!");
        }

        public static string[] WordsFromDictionary(TextAsset textAsset)
        {
            var content = textAsset.text;
            
            // Don't trust the file, Normalize all new lines to '\n'
            content = content.Replace("\r\n", "\n").Replace("\r", "\n");
            
            //Ignore empty and trim all words
            return content
                    .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(w => w.Trim())
                    .ToArray();
        }
        
        #endregion
    }

    public enum ThaiGlyphCorrection
    {
        None, YoorYingAndToorTaan, FullC90
    }
}