using System;

namespace SDG.Unturned;

public class Action
{
    internal CachingBcAssetRef blueprintOwnerRef;

    private EActionType _type;

    private ActionBlueprint[] _blueprints;

    private string _text;

    private string _tooltip;

    private string _key;

    public EActionType type => _type;

    public ActionBlueprint[] blueprints => _blueprints;

    public string text => _text;

    public string tooltip => _tooltip;

    public string key => _key;

    public bool IsAnyBlueprintLink
    {
        get
        {
            if (_blueprints == null)
            {
                return false;
            }
            ActionBlueprint[] array = _blueprints;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].isLink)
                {
                    return true;
                }
            }
            return false;
        }
    }

    [Obsolete("Please use FindBlueprintOwnerAsset for GUID support")]
    public ushort source => blueprintOwnerRef.LegacyId;

    public Asset FindBlueprintOwnerAsset()
    {
        return blueprintOwnerRef.Get();
    }

    public Action(ushort newSource, EActionType newType, ActionBlueprint[] newBlueprints, string newText, string newTooltip, string newKey)
    {
        _type = newType;
        _blueprints = newBlueprints;
        _text = newText;
        _tooltip = newTooltip;
        _key = newKey;
    }

    public override string ToString()
    {
        return $"(Type: {_type} Blueprints: {_blueprints?.Length} Text: {_text} Tooltip: {_tooltip} Key: {_key})";
    }
}
