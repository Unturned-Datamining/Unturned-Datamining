namespace SDG.Unturned;

public class Action
{
    private ushort _source;

    private EActionType _type;

    private ActionBlueprint[] _blueprints;

    private string _text;

    private string _tooltip;

    private string _key;

    public ushort source => _source;

    public EActionType type => _type;

    public ActionBlueprint[] blueprints => _blueprints;

    public string text => _text;

    public string tooltip => _tooltip;

    public string key => _key;

    public Action(ushort newSource, EActionType newType, ActionBlueprint[] newBlueprints, string newText, string newTooltip, string newKey)
    {
        _source = newSource;
        _type = newType;
        _blueprints = newBlueprints;
        _text = newText;
        _tooltip = newTooltip;
        _key = newKey;
    }
}
