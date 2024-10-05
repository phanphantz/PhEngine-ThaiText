namespace PhEngine.ThaiTMP
{
    public class TokenizeRequest
    {
        public TokenizeRequest(string input, string separator, bool isBreakWords, bool isSupportRichText)
        {
            Input = input;
            Separator = separator;
            IsBreakWords = isBreakWords;
            IsSupportRichText = isSupportRichText;
        }

        public string Input { get; }
        public string Separator { get; }
        public bool IsBreakWords { get; }
        public bool IsSupportRichText { get; }
    }
}