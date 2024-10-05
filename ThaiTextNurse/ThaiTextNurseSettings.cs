#if UNITY_EDITOR
using UnityEditor;
#endif

using System.IO;
using UnityEngine;

namespace PhEngine.ThaiTMP
{
    [CreateAssetMenu(menuName = "ThaiTMP/ThaiTextNurseSettings", fileName = "ThaiTextNurseSettings")]
    public class ThaiTextNurseSettings : ScriptableObject
    {
        public string WordBreakCharacter => wordBreakType == WordBreakType.ZeroWidthSpace
            ? "​"
            : customCharacter;
        
        [Header("General")]
        [SerializeField] string dictionaryResourcePath = "dictionary";
        public string DictionaryResourcePath => dictionaryResourcePath;
        
        [SerializeField] WordBreakType wordBreakType;
        [SerializeField] string customCharacter;
        
        [Header("Editor-Only")]
        [SerializeField] bool loadDictionaryOnStart = true;
        public bool IsLoadDictionaryOnEditorStartUp => loadDictionaryOnStart;
        
        public static ThaiTextNurseSettings PrepareInstance()
        {
            if (unsafeInstance == null)
            {
                unsafeInstance = Resources.Load<ScriptableObject>("ThaiTextNurseSettings") as ThaiTextNurseSettings;
                if (unsafeInstance == null)
                {
#if UNITY_EDITOR
                    unsafeInstance = CreateInstance<ThaiTextNurseSettings>();
                    var path = "Assets/Plugins/ThaiFontCare/Resources/ThaiTextNurseSettings.asset";
                    var directory = Path.GetDirectoryName(path);
                    if (!Directory.Exists(directory) && directory != null)
                            Directory.CreateDirectory(directory);

                    AssetDatabase.CreateAsset(unsafeInstance, path);
                    AssetDatabase.SaveAssets();
                    Debug.Log("Created a default ThaiTextNurseSettings at : " + path);
#else
                    Debug.LogError("ThaiTextNurseSettings.asset is missing from the Resources folder! the default settings will be used on ThaiTextNurse components");
#endif
                }
            }
            return unsafeInstance;
        }

        static ThaiTextNurseSettings unsafeInstance;
    }

    public enum WordBreakType
    {
        ZeroWidthSpace, CustomCharacter
    }
}