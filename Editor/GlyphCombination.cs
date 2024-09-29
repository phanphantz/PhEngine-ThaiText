using System;
using PhEngine.UI.ThaiText.Editor;

[Serializable]
public class GlypthOffset
{
    public string DisplayName => group == ThaiGlyphGroup.Custom ? customName : ThaiLanguageInfo.GetThaiGlyphGroupName(group);
    public string customName = "untitled";
    public string glyphs = "";
    public ThaiGlyphGroup group = ThaiGlyphGroup.Custom;
    public float xPlacement;
    public float yPlacement;

    public void AssignGroup(ThaiGlyphGroup value)
    {
        group = value;
        glyphs = string.Join("",ThaiLanguageInfo.GetGlyphsOf(group));
    }
}

[Serializable]
public class GlyphCombination
{
    public GlypthOffset first = new GlypthOffset();
    public GlypthOffset second = new GlypthOffset();
    public string DisplayName => first.DisplayName + " + " + second.DisplayName;
}