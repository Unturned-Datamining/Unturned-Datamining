using UnityEngine;

namespace SDG.Unturned;

public class EditorSpawnsUI
{
    private static SleekFullscreenBox container;

    public static bool active;

    private static SleekButtonIcon animalsButton;

    private static SleekButtonIcon itemsButton;

    private static SleekButtonIcon zombiesButton;

    private static SleekButtonIcon vehiclesButton;

    public static void open()
    {
        if (!active)
        {
            active = true;
            container.AnimateIntoView();
        }
    }

    public static void close()
    {
        if (active)
        {
            active = false;
            EditorSpawnsItemsUI.close();
            EditorSpawnsAnimalsUI.close();
            EditorSpawnsZombiesUI.close();
            EditorSpawnsVehiclesUI.close();
            container.AnimateOutOfView(1f, 0f);
        }
    }

    private static void onClickedAnimalsButton(ISleekElement button)
    {
        EditorSpawnsItemsUI.close();
        EditorSpawnsZombiesUI.close();
        EditorSpawnsVehiclesUI.close();
        EditorSpawnsAnimalsUI.open();
    }

    private static void onClickItemsButton(ISleekElement button)
    {
        EditorSpawnsAnimalsUI.close();
        EditorSpawnsZombiesUI.close();
        EditorSpawnsVehiclesUI.close();
        EditorSpawnsItemsUI.open();
    }

    private static void onClickedZombiesButton(ISleekElement button)
    {
        EditorSpawnsAnimalsUI.close();
        EditorSpawnsItemsUI.close();
        EditorSpawnsVehiclesUI.close();
        EditorSpawnsZombiesUI.open();
    }

    private static void onClickedVehiclesButton(ISleekElement button)
    {
        EditorSpawnsAnimalsUI.close();
        EditorSpawnsItemsUI.close();
        EditorSpawnsZombiesUI.close();
        EditorSpawnsVehiclesUI.open();
    }

    public EditorSpawnsUI()
    {
        Local local = Localization.read("/Editor/EditorSpawns.dat");
        Bundle bundle = Bundles.getBundle("/Bundles/Textures/Edit/Icons/EditorSpawns/EditorSpawns.unity3d");
        container = new SleekFullscreenBox();
        container.positionOffset_X = 10;
        container.positionOffset_Y = 10;
        container.positionScale_X = 1f;
        container.sizeOffset_X = -20;
        container.sizeOffset_Y = -20;
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
        EditorUI.window.AddChild(container);
        active = false;
        animalsButton = new SleekButtonIcon(bundle.load<Texture2D>("Animals"));
        animalsButton.positionOffset_Y = 40;
        animalsButton.sizeOffset_X = -5;
        animalsButton.sizeOffset_Y = 30;
        animalsButton.sizeScale_X = 0.25f;
        animalsButton.text = local.format("AnimalsButtonText");
        animalsButton.tooltip = local.format("AnimalsButtonTooltip");
        animalsButton.onClickedButton += onClickedAnimalsButton;
        container.AddChild(animalsButton);
        itemsButton = new SleekButtonIcon(bundle.load<Texture2D>("Items"));
        itemsButton.positionOffset_X = 5;
        itemsButton.positionOffset_Y = 40;
        itemsButton.positionScale_X = 0.25f;
        itemsButton.sizeOffset_X = -10;
        itemsButton.sizeOffset_Y = 30;
        itemsButton.sizeScale_X = 0.25f;
        itemsButton.text = local.format("ItemsButtonText");
        itemsButton.tooltip = local.format("ItemsButtonTooltip");
        itemsButton.onClickedButton += onClickItemsButton;
        container.AddChild(itemsButton);
        zombiesButton = new SleekButtonIcon(bundle.load<Texture2D>("Zombies"));
        zombiesButton.positionOffset_X = 5;
        zombiesButton.positionOffset_Y = 40;
        zombiesButton.positionScale_X = 0.5f;
        zombiesButton.sizeOffset_X = -10;
        zombiesButton.sizeOffset_Y = 30;
        zombiesButton.sizeScale_X = 0.25f;
        zombiesButton.text = local.format("ZombiesButtonText");
        zombiesButton.tooltip = local.format("ZombiesButtonTooltip");
        zombiesButton.onClickedButton += onClickedZombiesButton;
        container.AddChild(zombiesButton);
        vehiclesButton = new SleekButtonIcon(bundle.load<Texture2D>("Vehicles"));
        vehiclesButton.positionOffset_X = 5;
        vehiclesButton.positionOffset_Y = 40;
        vehiclesButton.positionScale_X = 0.75f;
        vehiclesButton.sizeOffset_X = -5;
        vehiclesButton.sizeOffset_Y = 30;
        vehiclesButton.sizeScale_X = 0.25f;
        vehiclesButton.text = local.format("VehiclesButtonText");
        vehiclesButton.tooltip = local.format("VehiclesButtonTooltip");
        vehiclesButton.onClickedButton += onClickedVehiclesButton;
        container.AddChild(vehiclesButton);
        bundle.unload();
        new EditorSpawnsAnimalsUI();
        new EditorSpawnsItemsUI();
        new EditorSpawnsZombiesUI();
        new EditorSpawnsVehiclesUI();
    }
}
