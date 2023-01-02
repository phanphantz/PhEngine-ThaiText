using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace PhEngine.UI.ThaiText.Editor
{
    public class ThaiFontTesterWindow : EditorWindow
    {
        const string WINDOW_NAME = "Thai Font Tester";

        static Font targetFont;
        static TMP_FontAsset targetTMPFont;
        
        const string THAI_LETTERS_STRING = "ก ข ฃ ค ฅ ฆ ง จ ฉ ช ซ ฌ ญ ฎ ฏ ฐ ฑ ฒ ณ ด ต ถ ท ธ น บ ป ผ ฝ พ ฟ ภ ม ย ร ฤ ล ฦ ว ศ ษ ส ห ฬ อ ฮ ฯ ะ ั า ำ   ิ   ี   ึ   ื   ุ   ู   ฺ   ฿   เ แ โ ใ ไ ๅ ๆ   ็   ่   ้   ๊   ๋   ์   ํ   ๎   ๏ ๐ ๑ ๒ ๓ ๔ ๕ ๖ ๗ ๘ ๙ ๚ ๛";
        const string THAI_WORDS_STRING = "กิ ก์ กิ์ ก์ำ ปิ ป์ ปุ์ ปิ์ ป์ำ ฏุ ฏู ฎุ ฎู ญู ญุ ฐุ ฐู ป่า ปิ ปี่ ก้ำ ฆ่า กลั้น ก็ เป็น ซึ่ง ว่า ป้า กั้น ปั้น น้า น้ำ ล้ำ รั้ง ปั๊ม ฟี ฟี่ ฟิ ฟิ่ บิ่ บี่ กิน สิ้น พลั่ว ค้ำ";

        static string testMessage;

        static Vector2 scroll;
        
        public enum FontType
        {
            TextMeshPro, Normal
        }

        FontType fontType;
        
        [MenuItem("PhEngine/Thai/" + WINDOW_NAME)]
        static void Init()
        {
            ThaiFontTesterWindow window = (ThaiFontTesterWindow)GetWindow(typeof(ThaiFontTesterWindow));
            window.titleContent = new GUIContent(WINDOW_NAME);
            window.Show();
            SetupTestMessage();
        }

        static void SetupTestMessage()
        {
            var stringBuilder = new StringBuilder(THAI_LETTERS_STRING);
            stringBuilder.Append("\n\n");
            stringBuilder.Append(THAI_WORDS_STRING);
            testMessage = stringBuilder.ToString();
        }

        void OnGUI()
        {
            DrawFontFields();
            EditorGUILayout.Space();
            DrawTestMessageField();
            DrawTestButton();
        }
        
        void DrawFontFields()
        {
            fontType = (FontType) EditorGUILayout.EnumPopup("Type", fontType);

            if (fontType == FontType.Normal)
                targetFont =
                    EditorGUILayout.ObjectField(new GUIContent("Target Font"), targetFont, typeof(Font), false) as Font;
            else
                targetTMPFont =
                    EditorGUILayout.ObjectField(new GUIContent("Target TMP Font"), targetTMPFont, typeof(TMP_FontAsset), false)
                        as TMP_FontAsset;
        }
        
        void DrawTestMessageField()
        {
            EditorGUILayout.LabelField("Test Message : ");
            scroll = EditorGUILayout.BeginScrollView(scroll);
            testMessage = EditorGUILayout.TextArea(testMessage, GUILayout.Height(position.height - 30));
            EditorGUILayout.EndScrollView();
        }

        void DrawTestButton()
        {
            EditorGUI.BeginDisabledGroup(!IsShouldEnableTestButton());

            if (GUILayout.Button("Create Test Scene"))
                CreateTestScene();

            EditorGUI.EndDisabledGroup();
        }

        bool IsShouldEnableTestButton()
        {
            return fontType == FontType.Normal && targetFont != null
                   || fontType == FontType.TextMeshPro && targetTMPFont != null;
        }
        
        void CreateTestScene()
        {
            if (fontType == FontType.Normal)
                CreateTestNormalTextScene(targetFont);
            else
                CreateTestTMPTextScene(targetTMPFont);
        }
        
        void CreateTestNormalTextScene(Font font)
        {
            CreateAndOpenScene(font.name);
            var newGameObj = new GameObject();
            var text = newGameObj.AddComponent<Text>();
            text.font = font;
            text.text = testMessage;
            text.resizeTextForBestFit = true;
            
            CanvasForTextCreator.Create(text.gameObject);
            TrySetupVerticalLayoutGroupInParent(text.gameObject);
        }

        void CreateTestTMPTextScene(TMP_FontAsset font)
        {
            CreateAndOpenScene(font.name);
            var tmpText = ThaiTextMeshProUGUI.CreateCanvasWithText();
            tmpText.font = font;
            tmpText.text = testMessage;
            tmpText.enableAutoSizing = true;
            
            TrySetupVerticalLayoutGroupInParent(tmpText.gameObject);
        }

        static void TrySetupVerticalLayoutGroupInParent(GameObject tmpText)
        {
            var parent = tmpText.transform.parent;
            if (!parent)
                return;

            var verticalLayoutGroup = parent.gameObject.AddComponent<VerticalLayoutGroup>();
            SetupVerticalLayoutGroup(verticalLayoutGroup);
        }

        static void SetupVerticalLayoutGroup(VerticalLayoutGroup verticalLayoutGroup)
        {
            var padding = verticalLayoutGroup.padding;
            
            var paddingOffset = 40;
            padding.left = paddingOffset;
            padding.right = paddingOffset;
            padding.top = paddingOffset;
            padding.bottom = paddingOffset;
            verticalLayoutGroup.padding = padding;
            
            verticalLayoutGroup.childForceExpandHeight = true;
            verticalLayoutGroup.childControlWidth = true;
            verticalLayoutGroup.childControlHeight = true;
            verticalLayoutGroup.childControlWidth = true;
        }

        static void CreateAndOpenScene(string fontName)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            scene.name = $"Test_{fontName}_Font";
        }
    }
}