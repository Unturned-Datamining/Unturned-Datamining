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
        sleekLabel.positionOffset_X = parentButton.positionOffset_X;
        sleekLabel.positionScale_X = parentButton.positionScale_X;
        sleekLabel.sizeOffset_X = -parentButton.sizeOffset_X;
        sleekLabel.sizeOffset_Y = parentButton.sizeOffset_Y;
        sleekLabel.sizeScale_X = parentButton.sizeScale_X;
        sleekLabel.text = localization.format("Crafting_Disabled");
        sleekLabel.textColor = ESleekTint.BAD;
        sleekLabel.fontSize = ESleekFontSize.Large;
        sleekLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
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
        container.positionScale_Y = -1f;
        container.positionOffset_X = 10;
        container.positionOffset_Y = 10;
        container.sizeOffset_X = -20;
        container.sizeOffset_Y = -20;
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
        PlayerUI.container.AddChild(container);
        active = false;
        inventoryButton = new SleekButtonIcon(bundle.load<Texture2D>("Inventory"));
        inventoryButton.sizeOffset_X = -5;
        inventoryButton.sizeOffset_Y = 50;
        inventoryButton.sizeScale_X = 0.25f;
        inventoryButton.text = local.format("Inventory", ControlsSettings.inventory);
        inventoryButton.tooltip = local.format("Inventory_Tooltip");
        inventoryButton.onClickedButton += onClickedInventoryButton;
        inventoryButton.fontSize = ESleekFontSize.Medium;
        inventoryButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(inventoryButton);
        craftingButton = new SleekButtonIcon(bundle.load<Texture2D>("Crafting"));
        craftingButton.positionOffset_X = 5;
        craftingButton.positionScale_X = 0.25f;
        craftingButton.sizeOffset_X = -10;
        craftingButton.sizeOffset_Y = 50;
        craftingButton.sizeScale_X = 0.25f;
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
        skillsButton.positionOffset_X = 5;
        skillsButton.positionScale_X = 0.5f;
        skillsButton.sizeOffset_X = -10;
        skillsButton.sizeOffset_Y = 50;
        skillsButton.sizeScale_X = 0.25f;
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
        informationButton.positionOffset_X = 5;
        informationButton.positionScale_X = 0.75f;
        informationButton.sizeOffset_X = -5;
        informationButton.sizeOffset_Y = 50;
        informationButton.sizeScale_X = 0.25f;
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
            inventoryButton.sizeScale_X = 0.5f;
            craftingButton.isVisible = false;
            skillsButton.isVisible = false;
            informationButton.positionScale_X = 0.5f;
            informationButton.sizeScale_X = 0.5f;
        }
        bundle.unload();
        new PlayerDashboardInventoryUI();
        new PlayerDashboardCraftingUI();
        new PlayerDashboardSkillsUI();
        infoUI = new PlayerDashboardInformationUI();
    }
}
