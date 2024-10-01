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
            
            isDirty = true;
            tmpText.havePropertiesChanged = true;
        }
        
        public string PreprocessText(string text)
        {
            if (lastKnownText == text && !isDirty)
                return lastProcessedText;
            
            Debug.Log("Start Process");

            lastKnownText = text;
            lastProcessedText = ThaiTextProcessor.Process(text, isCorrectGlyphs, isTokenize);
            isDirty = false;
            return lastProcessedText;
        }
    }
}