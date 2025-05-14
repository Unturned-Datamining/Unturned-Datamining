using UnityEngine;

namespace SDG.Unturned;

public class TagAsset : Asset
{
    /// <summary>
    /// Tag display name without rich text formatting. To use, for example, in logging, sorting, or with rich color override.
    /// </summary>
    public string PlainTextName { get; protected set; }

    /// <summary>
    /// If true, tag requests name to be displayed in UI with NameColor label color override.
    /// </summary>
    public bool HasNameColor { get; protected set; }

    /// <summary>
    /// Color to use in UI when displaying name.
    /// </summary>
    public Color NameColor { get; protected set; }

    /// <summary>
    /// If HasNameColor is enabled, this is PlainTextName wrapped with NameColor rich text color tags.
    /// If unset, falls back to PlainTextName.
    ///
    /// Nelson 2025-05-02: initially, we allowed enabling any rich text tags in names. But, considering that we
    /// are also using PlainTextName for other color overrides (e.g. "bad" when missing), this will be disappointing
    /// if tags are displayed inconsistently.
    /// </summary>
    public string RichTextName { get; protected set; }

    public Texture2D Icon { get; protected set; }

    /// <summary>
    /// If true, icon should be tinted according to player's foreground color preference.
    /// </summary>
    public bool ShouldTintIcon { get; protected set; }

    /// <summary>
    /// Wrap PlainTextName with player's font color preference.
    /// </summary>
    public string PlainTextNameWithPreferredFontColor => GetPlainTextNameWithColor(OptionsSettings.fontColor);

    /// <summary>
    /// If HasNameColor, get RichTextName. Otherwise, get PlainTextNameWithPreferredFontColor.
    /// </summary>
    public string RichTextOrPreferredFontColor
    {
        get
        {
            if (!HasNameColor)
            {
                return PlainTextNameWithPreferredFontColor;
            }
            return RichTextName;
        }
    }

    /// <summary>
    /// Get sleek color for UI. If HasNameColor, get NameColor. Otherwise, preferred font color.
    /// </summary>
    public SleekColor NameColorOrPreferredFontColor
    {
        get
        {
            if (!HasNameColor)
            {
                return ESleekTint.FONT;
            }
            return NameColor;
        }
    }

    public override string FriendlyName => PlainTextName;

    /// <summary>
    /// Wrap PlainTextName with color rich text tag.
    /// </summary>
    public string GetPlainTextNameWithColor(Color32 color)
    {
        return RichTextUtil.wrapWithColor(PlainTextName, color);
    }

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        PlainTextName = p.localization.format("Name");
        if (p.data.TryParseColor32RGB("NameColor", out var value))
        {
            HasNameColor = true;
            NameColor = value;
            RichTextName = RichTextUtil.wrapWithColor(PlainTextName, value);
        }
        else
        {
            HasNameColor = false;
            RichTextName = PlainTextName;
        }
        if (p.data.ParseBool("HasIcon", defaultValue: true))
        {
            Icon = LoadRedirectableAsset<Texture2D>(p.bundle, "Icon", p.data, "IconPath");
            if (Icon == null)
            {
                ReportAssetError("missing Icon texture");
            }
            ShouldTintIcon = p.data.ParseBool("TintIcon", defaultValue: true);
        }
    }
}
