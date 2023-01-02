using UnityEngine;

namespace PhEngine.UI.ThaiText
{
    [CreateAssetMenu(menuName = "PhEngine/UI/TextValidation/ThaiFontAdjustor", fileName = "UITextValidation_ThaiFontAdjustor", order = 0)]
    public class UITextValidation_ThaiFontAdjustor : UITextValidation
    {
        public override (bool isValid, string resultText) GetValidatedText(string textValue)
        {
            return (true, ThaiFontAdjuster.Adjust(textValue));
        }
    }
}