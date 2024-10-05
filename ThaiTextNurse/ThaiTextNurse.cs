using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

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
        [SerializeField] bool isTokenize;
        
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
        
        [SerializeField, HideInInspector] TMP_Text tmpText;
        [SerializeField, HideInInspector] string lastKnownText;
        
        public string OutputString => outputString;
        [SerializeField, HideInInspector] string outputString;
        
        public int LastWordCount => lastWordCount;
        [SerializeField, HideInInspector] int lastWordCount;
        
        static PhunTokenizer tokenizer;
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

            var settings = ThaiTextNurseSettings.PrepareInstance();
            if (tokenizer == null && !TryRebuildDictionary(settings))
                return outputString;
            if (tokenizer == null)
                return outputString;
            
            var finalSeparator = GetFinalSeparator(settings);
            if (string.IsNullOrEmpty(finalSeparator))
                return outputString;
            
            var words = tokenizer.Tokenize(text, tmpText.richText);
            lastWordCount = words.Count;
            outputString = string.Join(finalSeparator, words);
            return outputString;
        }
        
        string GetFinalSeparator(ThaiTextNurseSettings settings)
        {
            var finalSeparator = separator;
            if (tmpText.enableWordWrapping)
                finalSeparator += settings ? settings.WordBreakCharacter : "​";
            return finalSeparator;
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

        public static string GetDictionaryPath(ThaiTextNurseSettings settings)
        {
            return settings? settings.DictionaryResourcePath : "dictionary";
        }

        static void RebuildTokenizer(TextAsset textAsset)
        {
            tokenizer = new PhunTokenizer(WordsFromDictionary(textAsset));
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
    }

    public enum ThaiGlyphCorrection
    {
        None, YoorYingAndToorTaan, FullC90
    }
}