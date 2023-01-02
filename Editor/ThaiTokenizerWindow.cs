using ThaiStringTokenizer;
using UnityEditor;
using UnityEngine;

namespace PhEngine.UI.ThaiText.Editor
{
    public class ThaiTokenizerWindow : EditorWindow
    {
        const string WINDOW_NAME = "Thai Message Tokenizer";
        const string EXAMPLE_MESSAGE = "ศิลาจารึกเป็นวรรณกรรมชนิดลายลักษณ์อักษรอย่างหนึ่งอาศัยการบันทึกบนเนื้อศิลาทั้งชนิดเป็นแผ่นและเป็นแท่งโดยใช้โลหะแหลมขูดเนื้อศิลาให้เป็นตัวอักษรเรียกว่าจารหรือการจารึกศิลาจารึกมีคุณค่าในเชิงบันทึกทางประวัติศาสตร์ผู้จารึกหรือผู้สั่งให้มีศิลาจารึกมักจะเป็นผู้มีอำนาจมิใช่บุคคลทั่วไปเนื้อหาที่จารึกมีความหลากหลายตามความประสงค์ของผู้จารึกเช่นบันทึกเหตุการณ์บันทึกเรื่องราวในศาสนาบันทึกตำรับตำราการแพทย์และวรรณคดีเป็นต้น";
            
        static string originalMessage;
        static string tokenizedMessage;
        
        static Vector2 originalMessageScroll;
        static Vector2 tokenizedMessageScroll;
        static TokenizerConfig config;
        
        [MenuItem("PhEngine/Thai/" + WINDOW_NAME)]
        static void Init()
        {
            ThaiTokenizerWindow window = (ThaiTokenizerWindow)GetWindow(typeof(ThaiTokenizerWindow));
            window.titleContent = new GUIContent(WINDOW_NAME);
            window.Show();
            SetupTexts();
        }
        
        static void SetupTexts()
        {
            tokenizedMessage = string.Empty;
            originalMessage = EXAMPLE_MESSAGE;
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Original Message : ");
            originalMessageScroll = EditorGUILayout.BeginScrollView(originalMessageScroll);
            originalMessage = EditorGUILayout.TextArea(originalMessage, GUILayout.Height(position.height - 30));
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            config = EditorGUILayout.ObjectField("Tokenizer Config", config, typeof(TokenizerConfig), false) as TokenizerConfig;
            var isConfigValid = config && config.IsValid();
            EditorGUI.BeginDisabledGroup(!isConfigValid);
            if (GUILayout.Button("Generate", GUILayout.Width(100)))
                tokenizedMessage = ThaiTokenizer.GetTokenizedString(originalMessage, config);

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Tokenized Message : ");
            tokenizedMessageScroll = EditorGUILayout.BeginScrollView(tokenizedMessageScroll);
            EditorGUILayout.TextArea(tokenizedMessage, GUILayout.Height(position.height - 30));
            EditorGUILayout.EndScrollView();
        }
    }
}