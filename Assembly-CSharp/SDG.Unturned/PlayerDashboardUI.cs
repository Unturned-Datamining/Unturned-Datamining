using UnityEngine;

namespace SDG.Unturned;

public class PlayerDashboardUI
{
    public static SleekFullscreenBox container;

    public static bool active;

    private static SleekButtonIcon inventoryButton;

    private static SleekButtonIcon craftingButton;

    private static SleekButtonIcon skillsButton;

    private static SleekButtonIcon informationButton;

    private PlayerDashboardInformationUI infoUI;

    public static void open()
    {
        if (!active)
        {
            active = true;
            if (PlayerDashboardInventoryUI.active)
            {
                PlayerDashboardInventoryUI.active = false;
                PlayerDashboardInventoryUI.open();
            }
            else if (PlayerDashboardCraftingUI.active)
            {
                PlayerDashboardCraftingUI.active = false;
                PlayerDashboardCraftingUI.open();
            }
            else if (PlayerDashboardSkillsUI.active)
            {
                PlayerDashboardSkillsUI.active = false;
                PlayerDashboardSkillsUI.open();
            }
            else if (PlayerDashboardInformationUI.active)
            {
                PlayerDashboardInformationUI.active = false;
                PlayerDashboardInformationUI.open();
            }
            container.AnimateIntoView();
        }
    }

    public static void close()
    {
        if (active)
        {
            active = false;
            if (PlayerDashboardInventoryUI.active)
            {
                PlayerDashboardInventoryUI.close();
                PlayerDashboardInventoryUI.active = true;
            }
            else if (PlayerDashboardCraftingUI.active)
            {
                PlayerDashboardCraftingUI.close();
                PlayerDashboardCraftingUI.active = true;
            }
            else if (PlayerDashboardSkillsUI.active)
            {
                PlayerDashboardSkillsUI.close();
                PlayerDashboardSkillsUI.active = true;
            }
            else if (PlayerDashboardInformationUI.active)
            {
                PlayerDashboardInformationUI.close();
                PlayerDashboardInformationUI.active = true;
            }
            container.AnimateOutOfView(0f, -1f);
        }
    }

    private static void onClickedInventoryButton(ISleekElement button)
    {
        PlayerDashboardCraftingUI.close();
        PlayerDashboardSkillsUI.close();
        PlayerDashboardInformationUI.close();
        if (PlayerDashboardInventoryUI.active)
        {
            close();
            PlayerLifeUI.open();
        }
        else
        {
            PlayerDashboardInventoryUI.open();
        }
    }

    private static void onClickedCraftingButton(ISleekElement button)
    {
        PlayerDashboardInventoryUI.close();
        PlayerDashboardSkillsUI.close();
        PlayerDashboardInformationUI.close();
        if (PlayerDashboardCraftingUI.active)
        {
            close();
            PlayerLifeUI.open();
        }
        else
        {
            PlayerDashboardCraftingUI.open();
        }
    }

    private static void onClickedSkillsButton(ISleekElement button)
    {
        PlayerDashboardInventoryUI.close();
        PlayerDashboardCraftingUI.close();
        PlayerDashboardInformationUI.close();
        if (PlayerDashboardSkillsUI.active)
        {
            close();
            PlayerLifeUI.open();
        }
        else
        {
            PlayerDashboardSkillsUI.open();
        }
    }

    private static void onClickedInformationButton(ISleekElement button)
    {
        PlayerDashboardInventoryUI.close();
        PlayerDashboardCraftingUI.close();
        PlayerDashboardSkillsUI.close();
        if (PlayerDashboardInformationUI.active)
        {
            close();
            PlayerLifeUI.open();
        }
        else
        {
            PlayerDashboardInformationUI.open();
        }
    }

    private void createDisabledLabel(SleekButtonIcon parentButton, Local localization)
    {
        parentButton.isClickable = false;
        ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
        sleekLabel.PositionOffset_X = parentButton.PositionOffset_X;
        sleekLabel.PositionScale_X = parentButton.PositionScale_X;
        sleekLabel.SizeOffset_X = 0f - parentButton.SizeOffset_X;
        sleekLabel.SizeOffset_Y = parentButton.SizeOffset_Y;
        sleekLabel.SizeScale_X = parentButton.SizeScale_X;
        sleekLabel.Text = localization.format("Crafting_Disabled");
        sleekLabel.TextColor = ESleekTint.BAD;
        sleekLabel.FontSize = ESleekFontSize.Large;
        sleekLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        container.AddChild(sleekLabel);
    }

    public void OnDestroy()
    {
        infoUI.OnDestroy();
    }

    public PlayerDashboardUI()
    {
        Local local = Localization.read("/Player/PlayerDashboard.dat");
        Bundle bundle = Bundles.getBundle("/Bundles/Textures/Player/Icons/PlayerDashboard/PlayerDashboard.unity3d");
        container = new SleekFullscreenBox();
        container.PositionScale_Y = -1f;
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        PlayerUI.container.AddChild(container);
        active = false;
        inventoryButton = new SleekButtonIcon(bundle.load<Texture2D>("Inventory"));
        inventoryButton.SizeOffset_X = -5f;
        inventoryButton.SizeOffset_Y = 50f;
        inventoryButton.SizeScale_X = 0.25f;
        inventoryButton.text = local.format("Inventory", ControlsSettings.inventory);
        inventoryButton.tooltip = local.format("Inventory_Tooltip");
        inventoryButton.onClickedButton += onClickedInventoryButton;
        inventoryButton.fontSize = ESleekFontSize.Medium;
        inventoryButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(inventoryButton);
        craftingButton = new SleekButtonIcon(bundle.load<Texture2D>("Crafting"));
        craftingButton.PositionOffset_X = 5f;
        craftingButton.PositionScale_X = 0.25f;
        craftingButton.SizeOffset_X = -10f;
        craftingButton.SizeOffset_Y = 50f;
        craftingButton.SizeScale_X = 0.25f;
        craftingButton.text = local.format("Crafting", ControlsSettings.crafting);
        craftingButton.tooltip = local.format("Crafting_Tooltip");
        craftingButton.iconColor = ESleekTint.FOREGROUND;
        craftingButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(craftingButton);
        if (Level.info != null && !Level.info.configData.Allow_Crafting)
        {
            createDisabledLabel(craftingButton, local);
        }
        else
        {
            craftingButton.onClickedButton += onClickedCraftingButton;
        }
        skillsButton = new SleekButtonIcon(bundle.load<Texture2D>("Skills"));
        skillsButton.PositionOffset_X = 5f;
        skillsButton.PositionScale_X = 0.5f;
        skillsButton.SizeOffset_X = -10f;
        skillsButton.SizeOffset_Y = 50f;
        skillsButton.SizeScale_X = 0.25f;
        skillsButton.text = local.format("Skills", ControlsSettings.skills);
        skillsButton.tooltip = local.format("Skills_Tooltip");
        skillsButton.iconColor = ESleekTint.FOREGROUND;
        skillsButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(skillsButton);
        if (Level.info != null && !Level.info.configData.Allow_Skills)
        {
            createDisabledLabel(skillsButton, local);
        }
        else
        {
            skillsButton.onClickedButton += onClickedSkillsButton;
        }
        informationButton = new SleekButtonIcon(bundle.load<Texture2D>("Information"));
        informationButton.PositionOffset_X = 5f;
        informationButton.PositionScale_X = 0.75f;
        informationButton.SizeOffset_X = -5f;
        informationButton.SizeOffset_Y = 50f;
        informationButton.SizeScale_X = 0.25f;
        informationButton.text = local.format("Information", ControlsSettings.map);
        informationButton.tooltip = local.format("Information_Tooltip");
        informationButton.iconColor = ESleekTint.FOREGROUND;
        informationButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(informationButton);
        if (Level.info != null && !Level.info.configData.Allow_Information)
        {
            createDisabledLabel(informationButton, local);
        }
        else
        {
            informationButton.onClickedButton += onClickedInformationButton;
        }
        if (Level.info != null && Level.info.type == ELevelType.HORDE)
        {
            inventoryButton.SizeScale_X = 0.5f;
            craftingButton.IsVisible = false;
            skillsButton.IsVisible = false;
            informationButton.PositionScale_X = 0.5f;
            informationButton.SizeScale_X = 0.5f;
        }
        bundle.unload();
        new PlayerDashboardInventoryUI();
        new PlayerDashboardCraftingUI();
        new PlayerDashboardSkillsUI();
        infoUI = new PlayerDashboardInformationUI();
    }
}
