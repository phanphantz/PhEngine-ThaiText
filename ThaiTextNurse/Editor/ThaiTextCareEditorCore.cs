using UnityEditor;

namespace PhEngine.ThaiTMP.Editor
{
    [InitializeOnLoad]
    public static class ThaiTextCareEditorCore 
    {
        static ThaiTextCareEditorCore()
        {
            var settings = ThaiTextNurseSettings.PrepareInstance();
            if (settings && settings.IsLoadDictionaryOnEditorStartUp)
                ThaiTextNurse.RebuildDictionary();
        }
    }
}