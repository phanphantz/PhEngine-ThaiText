using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

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

        public static void AddWordsToDictionary(string pendingWords)
        {
            var settings = ThaiTextNurseSettings.PrepareInstance();
            if (!ThaiTextNurse.TryLoadDictionaryAsset(settings, out var textAsset))
                return;

            var assetPath = AssetDatabase.GetAssetPath(textAsset);
            var inputWords = GetWords(pendingWords);
            var successCount = 0;
            var dictionaryWordList = new List<string>(ThaiTextNurse.WordsFromDictionary(textAsset));
            foreach (var word in inputWords)
            {
                if (!dictionaryWordList.Contains(word))
                {
                    dictionaryWordList.Add(word);
                    successCount++;
                }
            }
            var content = string.Join(System.Environment.NewLine, dictionaryWordList);
            SaveDictionaryAndRebuild(assetPath, content);
            SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Added New " + successCount + " Words"), 1f);
        }

        static void SaveDictionaryAndRebuild(string assetPath, string content)
        {
            File.WriteAllText(assetPath, content); // Save as a plain text file
            AssetDatabase.Refresh();
            ThaiTextNurse.RebuildDictionary();
        }

        static string[] GetWords(string input)
        {
            return input.Split(' ').Select(w => w.Trim()).ToArray();
        }

        public static void RemoveWordsFromDictionary(string pendingWords)
        {
            var settings = ThaiTextNurseSettings.PrepareInstance();
            if (!ThaiTextNurse.TryLoadDictionaryAsset(settings, out var textAsset))
                return;

            var assetPath = AssetDatabase.GetAssetPath(textAsset);
            var inputWords = GetWords(pendingWords);
            var successCount = 0;
            var dictionaryWordList = new List<string>(ThaiTextNurse.WordsFromDictionary(textAsset));
            foreach (var word in inputWords)
            {
                if (dictionaryWordList.Remove(word))
                    successCount++;
            }
            var content = string.Join(System.Environment.NewLine, dictionaryWordList);
            SaveDictionaryAndRebuild(assetPath, content);
            SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Removed " + successCount + " Words"), 1f);
        }
    }
}