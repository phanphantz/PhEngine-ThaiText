namespace PhEngine.ThaiTMP
{
    public class TokenizeResult
    {
        public string Result { get; }
        public int WordCount { get; }

        public TokenizeResult(string result, int wordCount)
        {
            Result = result;
            WordCount = wordCount;
        }
    }
}