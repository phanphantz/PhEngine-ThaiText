using TMPro;
using UnityEngine;

namespace PhEngine.UI.ThaiText
{
    public class ThaiTextMeshPro : TextMeshPro
    {
        public bool isAdjustThaiText = true;
        const string DEFAULT_TEXT = "Sample text";

        public override void SetVerticesDirty()
        {
            if (IsShouldAIsAdjustThaiText())
                m_text = ThaiFontAdjuster.Adjust(m_text);
            
            base.SetVerticesDirty();
        }

        bool IsShouldAIsAdjustThaiText()
        {
            return !string.IsNullOrEmpty(m_text) && isAdjustThaiText;
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("GameObject/3D Object/Thai Text - TextMeshPro")]
#endif
        public static ThaiTextMeshPro Create()
        {
            var objName = "ThaiText (TMP)";
            var newGameObj = new GameObject
            {
                name = objName
            };
            var newText = newGameObj.AddComponent<ThaiTextMeshPro>();
            newText.text = DEFAULT_TEXT;
#if UNITY_EDITOR
            UnityEditor.Undo.RegisterCreatedObjectUndo(newGameObj, $"Create {objName}");
#endif
            return newText;
        }
    }
}