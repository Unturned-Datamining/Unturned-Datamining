namespace SDG.Unturned;

/// <summary>
/// Button in the list of levels for server browser filters.
/// </summary>
public class SleekFilterLevel : SleekLevel
{
    protected ISleekToggle toggle;

    public bool IsIncludedInFilter
    {
        get
        {
            return toggle.Value;
        }
        set
        {
            toggle.Value = value;
        }
    }

    public SleekFilterLevel(LevelInfo level)
        : base(level)
    {
        toggle = Glazier.Get().CreateToggle();
        toggle.PositionOffset_X = 20f;
        toggle.PositionOffset_Y = 30f;
        toggle.OnValueChanged += OnToggleValueChanged;
        AddChild(toggle);
    }

    protected void OnToggleValueChanged(ISleekToggle toggle, bool value)
    {
        onClickedLevel?.Invoke(this, 0);
    }
}
