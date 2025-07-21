using System.Collections.Generic;

namespace SDG.Unturned;

internal class SleekItemActionButton : SleekWrapper
{
    private Action action;

    private ISleekButton button;

    private Blueprint[] relatedBlueprints;

    private static List<Blueprint> tempBlueprints = new List<Blueprint>();

    public SleekItemActionButton(Action action)
    {
        this.action = action;
        Local localization = PlayerDashboardInventoryUI.localization;
        button = Glazier.Get().CreateButton();
        button.SizeScale_X = 1f;
        button.SizeScale_Y = 1f;
        string text;
        if (!string.IsNullOrEmpty(action.key))
        {
            button.Text = localization.format(action.key + "_Button");
            text = localization.format(action.key + "_Button_Tooltip");
        }
        else
        {
            button.Text = action.text;
            text = action.tooltip;
        }
        if (action.type == EActionType.BLUEPRINT && !string.IsNullOrEmpty(text))
        {
            text += "\n\n";
            if (action.IsAnyBlueprintLink)
            {
                text += localization.format("ActionBlueprint_SkipCraftingTooltip", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.SkipActionCraftingMenu));
                text += "\n";
            }
            text += localization.format("ActionBlueprint_CraftAllTooltip", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.other));
        }
        button.TooltipText = text;
        button.OnClicked += OnClickedButton;
        AddChild(button);
        if (!(action.FindBlueprintOwnerAsset() is IBlueprintOwner blueprintOwner))
        {
            UnturnedLog.warn($"Unable to find item action blueprint owner {action}");
            button.IsClickable = false;
            return;
        }
        tempBlueprints.Clear();
        ActionBlueprint[] blueprints = action.blueprints;
        foreach (ActionBlueprint actionBlueprint in blueprints)
        {
            Blueprint blueprint = actionBlueprint.FindBlueprint(blueprintOwner);
            if (blueprint == null)
            {
                UnturnedLog.warn($"Unable to find item action's blueprint {actionBlueprint}");
            }
            else
            {
                tempBlueprints.Add(blueprint);
            }
        }
        if (tempBlueprints.Count > 0)
        {
            relatedBlueprints = tempBlueprints.ToArray();
            PlayerDashboardCraftingUI.filteredBlueprintsOverride = relatedBlueprints;
            bool num = PlayerDashboardCraftingUI.UpdateFilteredBlueprintsAndGetAreAllCraftable();
            PlayerDashboardCraftingUI.filteredBlueprintsOverride = null;
            if (!num)
            {
                button.BackgroundColor = new SleekColor(ESleekTint.BACKGROUND, 0.5f);
            }
        }
        else
        {
            button.IsClickable = false;
            UnturnedLog.warn($"Item action has no blueprints {action}");
        }
    }

    private void OnClickedButton(ISleekElement element)
    {
        if (relatedBlueprints == null)
        {
            return;
        }
        bool isAnyBlueprintLink = action.IsAnyBlueprintLink;
        isAnyBlueprintLink &= !InputEx.GetKey(ControlsSettings.SkipActionCraftingMenu);
        PlayerDashboardCraftingUI.filteredBlueprintsOverride = relatedBlueprints;
        if (!isAnyBlueprintLink)
        {
            isAnyBlueprintLink = Player.LocalPlayer.equipment.isBusy || !PlayerDashboardCraftingUI.UpdateFilteredBlueprintsAndGetAreAllCraftable();
        }
        if (isAnyBlueprintLink)
        {
            PlayerDashboardInventoryUI.close();
            PlayerDashboardCraftingUI.open();
            return;
        }
        bool key = InputEx.GetKey(ControlsSettings.other);
        Blueprint[] array = relatedBlueprints;
        foreach (Blueprint blueprint in array)
        {
            Player.LocalPlayer.crafting.SendRequestToCraft(blueprint, key);
        }
        PlayerDashboardCraftingUI.filteredBlueprintsOverride = null;
        PlayerDashboardInventoryUI.closeSelection();
    }
}
