using UnityEngine;
using UnityEngine.UI;

namespace PhEngine.UI.ThaiText
{
    public static class CanvasForTextCreator
    {
        public static void Create(GameObject newText, Transform parent = null)
        {
            var newCanvas = CreateCanvas(parent);
            newText.transform.SetParent(newCanvas.transform);
            newText.transform.localPosition = Vector3.zero;
#if UNITY_EDITOR
            UnityEditor.Undo.RegisterCreatedObjectUndo(newCanvas.gameObject, $"Create {newText.name}");
#endif
        }

        static Canvas CreateCanvas(Transform parent = null)
        {
            var newCanvasGameObj = new GameObject {name = "Canvas"};
            var newCanvas = newCanvasGameObj.AddComponent<Canvas>();
            newCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var newScaler = newCanvasGameObj.AddComponent<CanvasScaler>();
            var raycaster = newCanvasGameObj.AddComponent<GraphicRaycaster>();
            if (parent)
                newCanvasGameObj.transform.SetParent(parent);
            
            return newCanvas;
        }
    }
}