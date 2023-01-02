using TMPro;
using UnityEngine;

namespace PhEngine.UI.ThaiText
{
    public class ThaiTextMeshProUGUI : TextMeshProUGUI
    {
        public bool isAdjustThaiText = true;
        const string DEFAULT_TEXT = "New Text";
        
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
        [UnityEditor.MenuItem("GameObject/UI/Thai Text - TextMeshPro")]
#endif
        public static ThaiTextMeshProUGUI Create()
        {
#if UNITY_EDITOR
            return CreateTextInEditor();
#else
            return CreateText();
#endif
        }

        static ThaiTextMeshProUGUI CreateText()
        {
            var objName = "ThaiText (TMP)";
            var newGameObj = new GameObject
            {
                name = objName
            };
            var newText = newGameObj.AddComponent<ThaiTextMeshProUGUI>();
            newText.text = DEFAULT_TEXT;
            return newText;
        }

#if UNITY_EDITOR
        static ThaiTextMeshProUGUI CreateTextInEditor()
        {
            var selectedGameObject = UnityEditor.Selection.activeGameObject;
            if (selectedGameObject != null)
                return selectedGameObject.TryGetComponent<Canvas>(out var canvas) ? 
                    CreateTextUnderCanvas(canvas) : 
                    CreateCanvasWithText(selectedGameObject.transform);
            
            var firstOrDefaultCanvas = FindObjectOfType<Canvas>();
            return firstOrDefaultCanvas ? 
                CreateTextUnderCanvas(firstOrDefaultCanvas) : 
                CreateCanvasWithText();
        }

        static ThaiTextMeshProUGUI CreateTextUnderCanvas(Canvas canvasTransform)
        {
            var newGameObj = CreateText();
            newGameObj.transform.SetParent(canvasTransform.transform);
            UnityEditor.Undo.RegisterCreatedObjectUndo(newGameObj, $"Create {newGameObj.name}");
            return newGameObj;
        }

        public static ThaiTextMeshProUGUI CreateCanvasWithText(Transform parent = null)
        {
            var newText = CreateText();
            CanvasForTextCreator.Create(newText.gameObject, parent);
            return newText;
        }
#endif
    }
}