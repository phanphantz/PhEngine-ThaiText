using System;
using System.Linq;
using PhEngine.UI.ThaiText.Editor;

[Serializable]
public class GlyphOffset
{
    public string DisplayName
    {
        get
        {
            if (group != ThaiGlyphGroup.Custom) 
                return ThaiLanguageInfo.GetThaiGlyphGroupName(group);
            
            var displayedChars = glyphs.Select(c => ThaiLanguageInfo.GetDisplayedString(c.ToString())).ToArray();
            if (displayedChars.Length == 1)
                return displayedChars.FirstOrDefault();
                
            return "[ " + string.Join(", ", displayedChars) + " ]";
        }
    }
    
    public string glyphs;
    public ThaiGlyphGroup group;
    public float xPlacement;
    public float yPlacement;

    public GlyphOffset()
    {
        glyphs = "";
        group = ThaiGlyphGroup.Custom;
    }
    
    public GlyphOffset(GlyphOffset offset)
    {
        glyphs = offset.glyphs;
        group = offset.group;
        xPlacement = offset.xPlacement;
        yPlacement = offset.yPlacement;
    }

    public void AssignGroup(ThaiGlyphGroup value)
    {
        group = value;
        var presetGlyphs = ThaiLanguageInfo.GetGlyphsOf(group);
        if (presetGlyphs.Length > 0)
            glyphs = string.Join("",presetGlyphs);
    }
}

[Serializable]
public class GlyphCombination
{
    public bool isEnabled = true;
    public GlyphOffset first;
    public GlyphOffset second;

    public GlyphCombination()
    {
        isEnabled = true;
        first = new GlyphOffset();
        second = new GlyphOffset();
    }

    public GlyphCombination(GlyphCombination combo)
    {
        first = new GlyphOffset(combo.first);
        second = new GlyphOffset(combo.second);
    }

    public string DisplayName => first.DisplayName + " + " + second.DisplayName;
    public int VariantCount
    {
        get
        {
            if (first == null || second == null)
                return 0;
            
            return first.glyphs.Length * second.glyphs.Length;
        }
    }
}