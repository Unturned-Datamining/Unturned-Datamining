namespace SDG.Unturned;

public class SleekCustomServerListPresetButton : SleekWrapper
{
    private ISleekButton internalButton;

    private ServerListFilters preset;

    public SleekCustomServerListPresetButton(ServerListFilters preset)
    {
        this.preset = preset;
        internalButton = Glazier.Get().CreateButton();
        internalButton.SizeScale_X = 1f;
        internalButton.SizeScale_Y = 1f;
        internalButton.Text = preset.presetName;
        internalButton.OnClicked += OnClicked;
        AddChild(internalButton);
    }

    private void OnClicked(ISleekElement button)
    {
        FilterSettings.activeFilters.CopyFrom(preset);
        FilterSettings.InvokeActiveFiltersReplaced();
    }
}
