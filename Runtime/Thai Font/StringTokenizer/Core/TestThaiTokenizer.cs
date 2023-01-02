using TMPro;
using UnityEngine;

namespace ThaiStringTokenizer
{
    public class TestThaiTokenizer : MonoBehaviour
    {
        [SerializeField] string thaiMessage;
        [SerializeField] string separator = "​";
        [SerializeField] TMP_Text text;
        [SerializeField] TextAsset textAsset;

        void Start()
        {
            var tokenizer = new ThaiTokenizer(textAsset.text);
            text.text = tokenizer.GetTokenizedString(thaiMessage, separator);
        }
    }
}