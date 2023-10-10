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
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.PositionScale_X = 1f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        EditorUI.window.AddChild(container);
        active = false;
        animalsButton = new SleekButtonIcon(bundle.load<Texture2D>("Animals"));
        animalsButton.PositionOffset_Y = 40f;
        animalsButton.SizeOffset_X = -5f;
        animalsButton.SizeOffset_Y = 30f;
        animalsButton.SizeScale_X = 0.25f;
        animalsButton.text = local.format("AnimalsButtonText");
        animalsButton.tooltip = local.format("AnimalsButtonTooltip");
        animalsButton.onClickedButton += onClickedAnimalsButton;
        container.AddChild(animalsButton);
        itemsButton = new SleekButtonIcon(bundle.load<Texture2D>("Items"));
        itemsButton.PositionOffset_X = 5f;
        itemsButton.PositionOffset_Y = 40f;
        itemsButton.PositionScale_X = 0.25f;
        itemsButton.SizeOffset_X = -10f;
        itemsButton.SizeOffset_Y = 30f;
        itemsButton.SizeScale_X = 0.25f;
        itemsButton.text = local.format("ItemsButtonText");
        itemsButton.tooltip = local.format("ItemsButtonTooltip");
        itemsButton.onClickedButton += onClickItemsButton;
        container.AddChild(itemsButton);
        zombiesButton = new SleekButtonIcon(bundle.load<Texture2D>("Zombies"));
        zombiesButton.PositionOffset_X = 5f;
        zombiesButton.PositionOffset_Y = 40f;
        zombiesButton.PositionScale_X = 0.5f;
        zombiesButton.SizeOffset_X = -10f;
        zombiesButton.SizeOffset_Y = 30f;
        zombiesButton.SizeScale_X = 0.25f;
        zombiesButton.text = local.format("ZombiesButtonText");
        zombiesButton.tooltip = local.format("ZombiesButtonTooltip");
        zombiesButton.onClickedButton += onClickedZombiesButton;
        container.AddChild(zombiesButton);
        vehiclesButton = new SleekButtonIcon(bundle.load<Texture2D>("Vehicles"));
        vehiclesButton.PositionOffset_X = 5f;
        vehiclesButton.PositionOffset_Y = 40f;
        vehiclesButton.PositionScale_X = 0.75f;
        vehiclesButton.SizeOffset_X = -5f;
        vehiclesButton.SizeOffset_Y = 30f;
        vehiclesButton.SizeScale_X = 0.25f;
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
