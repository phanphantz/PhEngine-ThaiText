using Lexto;
using TMPro;
using UnityEngine;

namespace PhEngine.ThaiTMP
{
    [RequireComponent(typeof(TMP_Text)), ExecuteAlways]
    public class ThaiTextNurse : MonoBehaviour, ITextPreprocessor
    {
        [SerializeField, HideInInspector] TMP_Text tmpText;
        [SerializeField] bool isCorrectGlyphs = true;
        [SerializeField] bool isTokenize;
        [SerializeField] string separator;
        [SerializeField, HideInInspector] string lastKnownText;
        [SerializeField, HideInInspector] string lastProcessedText;
        static PhunTokenizer tokenizer;

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
            isDirty = false;
            if (isCorrectGlyphs)
                lastProcessedText = ThaiFontAdjuster.Adjust(text);

            if (!isTokenize)
                return lastProcessedText;

            var finalSeparator = separator;
            if (tmpText.enableWordWrapping)
                finalSeparator += "​";

            if (string.IsNullOrEmpty(finalSeparator))
                return lastProcessedText;

            if (tokenizer == null)
            {
                var textAsset = Resources.Load<TextAsset>("dictionary");
                if (textAsset == null)
                    return lastProcessedText;
                
                tokenizer = new PhunTokenizer(textAsset.text.Split(System.Environment.NewLine));
                Resources.UnloadAsset(textAsset);
            }
            lastProcessedText = string.Join(finalSeparator,tokenizer.Tokenize(text));
            return lastProcessedText;
        }

        [ContextMenu(nameof(RebuildDict))]
        public void RebuildDict()
        {
            Clear();
            NotifyChange();
        }

        [ContextMenu(nameof(Clear))]
        public void Clear()
        {
            tokenizer = null;
        }
    }
}