using System.Text;

namespace PhEngine.ThaiTMP
{
    public static class ThaiFontAdjuster
    {
        public static bool IsThaiString(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return false;
            }
            var length = s.Length;
            for (var i = 0; i < length; i++)
            {
                var c = s[i];
                if (c >= '\x0E00' && c <= '\x0E7F')
                    return true;
            }
            return false;
        }
        
        public static string Adjust(string s)
        {
            // http://www.bakoma-tex.com/doc/fonts/enc/c90/c90.pdf

            //This is the trimmed down version of ThaiFontAdjuster
            var length = s.Length;
            var sb = new StringBuilder(length);
            for (var i = 0; i < length; i++)
            {
                var c = s[i];
                // [YO YING] [lower] -> [YO YING w/o lower] [lower]
                if (c == '\x0E0D' && i < length - 1 && IsLower(s[i + 1]))
                {
                    c = '\xF70F';
                }
                // [THO THAN] [lower] -> [THO THAN w/o lower] [lower]
                else if (c == '\x0E10' && i < length - 1 && IsLower(s[i + 1]))
                {
                    c = '\xF700';
                }
                sb.Append(c);
            }
            return sb.ToString();
        }

        static bool IsLower(char c)
        {
            // SARA U, SARA UU, PHINTHU
            return c >= '\x0E38' && c <= '\x0E3A';
        }
    }
}