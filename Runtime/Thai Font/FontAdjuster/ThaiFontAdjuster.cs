using System.Text;
using TMPro;

namespace PhEngine.UI.ThaiText
{
    public static class ThaiFontAdjuster
    {
        // ========== EXTENDED CHARACTER TABLE ==========
        // F700:     uni0E10.descless    (base.descless)
        // F701~04:  uni0E34~37.left     (upper.left)
        // F705~09:  uni0E48~4C.lowleft  (top.lowleft)
        // F70A~0E:  uni0E48~4C.low      (top.low)
        // F70F:     uni0E0D.descless    (base.descless)
        // F710~12:  uni0E31,4D,47.left  (upper.left)
        // F713~17:  uni0E48~4C.left     (top.left)
        // F718~1A:  uni0E38~3A.low      (lower.low)
        // ==============================================

        public static string Adjust(string s, TMP_FontAsset fontAsset)
        {
            // http://www.bakoma-tex.com/doc/fonts/enc/c90/c90.pdf

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

        private static bool IsLower(char c)
        {
            // SARA U, SARA UU, PHINTHU
            return c >= '\x0E38' && c <= '\x0E3A';
        }
    }
}