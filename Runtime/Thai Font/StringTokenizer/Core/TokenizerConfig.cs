using UnityEngine;

namespace ThaiStringTokenizer
{
    [CreateAssetMenu(menuName = "Config/Localization/TokenizerConfig", fileName = "TokenizerConfig", order = 0)]
    public class TokenizerConfig : ScriptableObject
    {
        public TextAsset dictionaryTextAsset;
        public TokenizerOptions options;

        public bool IsValid()
        {
            return dictionaryTextAsset != null
                   && (options.separatorOption == WordSeparatorOption.ZeroWidthSpace
                       || options.separatorOption == WordSeparatorOption.Custom &&
                       !string.IsNullOrEmpty(options.customSeparatorString));
        }
    }
}