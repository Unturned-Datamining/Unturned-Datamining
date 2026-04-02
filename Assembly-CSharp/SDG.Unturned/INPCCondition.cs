using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SDG.Unturned;

public class INPCCondition
{
    protected string text;

    protected bool shouldReset;

    /// <summary>
    /// If set, only show this condition in the UI when conditions with these indices are met.
    /// For example don't show "arrest the criminal (name)" until "investigate crime" is completed.
    /// </summary>
    internal List<int> uiRequirementIndices;

    public virtual bool isConditionMet(Player player)
    {
        return false;
    }

    public virtual void ApplyCondition(Player player)
    {
    }

    public virtual string formatCondition(Player player)
    {
        if (!string.IsNullOrEmpty(text))
        {
            return text;
        }
        return null;
    }

    public virtual ISleekElement createUI(Player player, Texture2D icon)
    {
        string value = formatCondition(player);
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }
        ISleekBox sleekBox = Glazier.Get().CreateBox();
        sleekBox.SizeOffset_Y = 30f;
        sleekBox.SizeScale_X = 1f;
        if (icon != null)
        {
            ISleekImage sleekImage = Glazier.Get().CreateImage(icon);
            sleekImage.PositionOffset_X = 5f;
            sleekImage.PositionOffset_Y = 5f;
            sleekImage.SizeOffset_X = 20f;
            sleekImage.SizeOffset_Y = 20f;
            sleekBox.AddChild(sleekImage);
        }
        ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
        if (icon != null)
        {
            sleekLabel.PositionOffset_X = 30f;
            sleekLabel.SizeOffset_X = -35f;
        }
        else
        {
            sleekLabel.PositionOffset_X = 5f;
            sleekLabel.SizeOffset_X = -10f;
        }
        sleekLabel.SizeScale_X = 1f;
        sleekLabel.SizeScale_Y = 1f;
        sleekLabel.TextAlignment = TextAnchor.MiddleLeft;
        sleekLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
        sleekLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        sleekLabel.AllowRichText = true;
        sleekLabel.Text = value;
        sleekBox.AddChild(sleekLabel);
        return sleekBox;
    }

    /// <summary>
    /// Is this condition influenced by a given quest flag?
    /// Used by level objects to determine if local player's flag change may affect visibility.
    /// </summary>
    public virtual bool isAssociatedWithFlag(ushort flagID)
    {
        return false;
    }

    /// <summary>
    /// Replacement for isAssociatedWithFlag to fix quest conditions and somewhat improve perf.
    /// </summary>
    internal virtual void GatherAssociatedFlags(HashSet<ushort> associatedFlags)
    {
    }

    /// <summary>
    /// Intended to replace filling data from constructor.
    /// </summary>
    internal virtual void PopulateV2(in PopulateConditionParameters p)
    {
        string @string = p.data.GetString("TextId");
        if (!string.IsNullOrEmpty(@string))
        {
            string text = p.localization.FormatOrEmpty(@string);
            if (!string.IsNullOrEmpty(text))
            {
                text = ItemTool.filterRarityRichText(text);
                this.text = text;
            }
            else
            {
                p.ReportError("no text for condition text ID \"" + @string + "\"");
            }
        }
        shouldReset = p.data.ParseBool("Reset");
        if (p.data.TryGetString("UI_Requirements", out var value))
        {
            ParseUIRequirements(in p, value);
        }
    }

    /// <summary>
    /// Intended to replace filling data from constructor. Legacy is for backwards compatibility with Condition_#_Key
    /// format, whereas V2 uses the list and dictionary features.
    /// </summary>
    internal virtual void PopulateLegacy(in PopulateConditionParameters p)
    {
        string desc = p.localization.FormatOrEmpty(p.legacyPrefix);
        desc = ItemTool.filterRarityRichText(desc);
        text = desc;
        shouldReset = p.data.ContainsKey(p.legacyPrefix + "_Reset");
        if (p.data.TryGetString(p.legacyPrefix + "_UI_Requirements", out var value))
        {
            ParseUIRequirements(in p, value);
        }
    }

    private void ParseUIRequirements(in PopulateConditionParameters p, string uiRequirements)
    {
        string[] array = uiRequirements.Split(',', StringSplitOptions.RemoveEmptyEntries);
        if (array == null || array.Length < 1)
        {
            p.ReportError("UI_Requirements are empty");
            return;
        }
        List<int> list = new List<int>(array.Length);
        string[] array2 = array;
        foreach (string text in array2)
        {
            if (!int.TryParse(text, out var result))
            {
                p.ReportError("unable to parse UI Requirement index from \"" + text + "\"");
            }
            else if (result < 0 || result >= p.conditionsLength)
            {
                p.ReportError($"UI Requirement index {result} out of bounds");
            }
            else if (result == p.conditionIndex)
            {
                p.ReportError("UI Requirement depends on itself");
            }
            else
            {
                list.Add(result);
            }
        }
        if (list.Count > 0)
        {
            uiRequirementIndices = list;
        }
    }

    public bool AreUIRequirementsMet(List<bool> areConditionsMet)
    {
        if (uiRequirementIndices == null || uiRequirementIndices.Count < 1)
        {
            return true;
        }
        foreach (int uiRequirementIndex in uiRequirementIndices)
        {
            if (uiRequirementIndex >= 0 && uiRequirementIndex < areConditionsMet.Count && !areConditionsMet[uiRequirementIndex])
            {
                return false;
            }
        }
        return true;
    }

    public virtual string GetTypeFriendlyName()
    {
        string text = GetType().Name;
        if (text.StartsWith("NPC", StringComparison.Ordinal))
        {
            text = text.Substring("NPC".Length);
        }
        if (text.EndsWith("Condition", StringComparison.Ordinal))
        {
            text = text.Substring(0, text.Length - "Condition".Length);
        }
        StringBuilder stringBuilder = new StringBuilder(32);
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (i > 0 && char.IsUpper(c) && !char.IsUpper(text[i - 1]))
            {
                stringBuilder.Append(' ');
            }
            stringBuilder.Append(c);
        }
        return stringBuilder.ToString();
    }

    public virtual void DebugDumpToStringBuilder(Player player, StringBuilder sb)
    {
        sb.Append(GetTypeFriendlyName());
    }

    public INPCCondition()
    {
    }

    [Obsolete]
    public INPCCondition(string newText, bool newShouldReset)
    {
        text = newText;
        shouldReset = newShouldReset;
    }

    [Obsolete("Removed shouldSend parameter because ApplyCondition is only called on the server now")]
    public virtual void applyCondition(Player player, bool shouldSend)
    {
        ApplyCondition(player);
    }
}
