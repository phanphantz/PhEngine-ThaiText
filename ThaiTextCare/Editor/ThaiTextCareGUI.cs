using UnityEditor;
using UnityEngine;

namespace PhEngine.ThaiTextCare.Editor
{
    public static class ThaiTextCareGUI
    {
        public static void DrawHorizontalLine()
        {
            var padding = 3f;
            var thickness = 0.75f;
            var r = EditorGUILayout.GetControlRect(GUILayout.Height(padding+thickness));
            r.height = thickness;
            r.y+=padding/2;
            r.x-=2;
            EditorGUI.DrawRect(r, Color.grey);
        }
    }
}