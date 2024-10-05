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
        [SerializeField, HideInInspector] string displayedString;
        static PhunTokenizer tokenizer;

        bool isDirty;

        void Awake()
        {
            if (tmpText == null)
                tmpText = GetComponent<TMP_Text>();
            isDirty = true;
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
                return displayedString;
            
            Debug.Log("Process");
            lastKnownText = text;
            isDirty = false;
            return RebuildDisplayString(text);
        }

        string RebuildDisplayString(string text)
        {
            displayedString = text;
            if (isCorrectGlyphs)
                displayedString = ThaiFontAdjuster.Adjust(text);

            if (!isTokenize)
                return displayedString;

            var finalSeparator = separator;
            if (tmpText.enableWordWrapping)
                finalSeparator += "​";

            if (string.IsNullOrEmpty(finalSeparator))
                return displayedString;

            if (tokenizer == null)
            {
                var textAsset = Resources.Load<TextAsset>("dictionary");
                if (textAsset == null)
                    return displayedString;
                
                tokenizer = new PhunTokenizer(textAsset.text.Split(System.Environment.NewLine));
                Resources.UnloadAsset(textAsset);
            }
            displayedString = string.Join(finalSeparator,tokenizer.Tokenize(text));
            return displayedString;
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