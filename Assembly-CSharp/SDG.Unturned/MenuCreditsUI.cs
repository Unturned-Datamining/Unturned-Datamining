using System;
using UnityEngine;

namespace SDG.Unturned;

public class MenuCreditsUI
{
    private static SleekFullscreenBox container;

    public static bool active;

    private static SleekButtonIcon returnButton;

    private static ISleekBox creditsBox;

    private static ISleekScrollView scrollBox;

    private static Local localization;

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
            container.AnimateOutOfView(0f, -1f);
        }
    }

    private static void onClickedReturnButton(ISleekElement button)
    {
        close();
        MenuPauseUI.open();
    }

    public MenuCreditsUI()
    {
        localization = Localization.read("/Menu/MenuCredits.dat");
        container = new SleekFullscreenBox();
        container.positionOffset_X = 10;
        container.positionOffset_Y = 10;
        container.positionScale_Y = -1f;
        container.sizeOffset_X = -20;
        container.sizeOffset_Y = -20;
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
        MenuUI.container.AddChild(container);
        active = false;
        returnButton = new SleekButtonIcon(MenuPauseUI.icons.load<Texture2D>("Exit"));
        returnButton.positionOffset_X = -250;
        returnButton.positionOffset_Y = 100;
        returnButton.positionScale_X = 0.5f;
        returnButton.sizeOffset_X = 500;
        returnButton.sizeOffset_Y = 50;
        returnButton.text = MenuPauseUI.localization.format("Return_Button");
        returnButton.tooltip = MenuPauseUI.localization.format("Return_Button_Tooltip");
        returnButton.onClickedButton += onClickedReturnButton;
        returnButton.fontSize = ESleekFontSize.Medium;
        returnButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(returnButton);
        creditsBox = Glazier.Get().CreateBox();
        creditsBox.positionOffset_X = -250;
        creditsBox.positionOffset_Y = 160;
        creditsBox.positionScale_X = 0.5f;
        creditsBox.sizeOffset_X = 500;
        creditsBox.sizeOffset_Y = -260;
        creditsBox.sizeScale_Y = 1f;
        creditsBox.fontSize = ESleekFontSize.Medium;
        container.AddChild(creditsBox);
        scrollBox = Glazier.Get().CreateScrollView();
        scrollBox.positionOffset_X = 5;
        scrollBox.positionOffset_Y = 5;
        scrollBox.sizeOffset_X = -10;
        scrollBox.sizeOffset_Y = -10;
        scrollBox.sizeScale_X = 1f;
        scrollBox.sizeScale_Y = 1f;
        scrollBox.scaleContentToWidth = true;
        creditsBox.AddChild(scrollBox);
        int verticalOffset = 0;
        AddHeader(localization.format("Header_Unturned"), ref verticalOffset);
        AddRow("Nelson Sexton", "adding bugs and breaking the game", ref verticalOffset);
        AddRow("Tyler \"MoltonMontro\" Pope", "community+web+server admin", ref verticalOffset);
        AddRow("Sven Mawby", "RocketMod", ref verticalOffset);
        AddRow("Riley Labrecque", "Steamworks .NET", ref verticalOffset);
        AddRow("Stephen McKamey", "A* Pathfinding Project", ref verticalOffset);
        AddRow("James Newton-King", "Json .NET", ref verticalOffset);
        AddRow("Still North Media", "The Firearm Sound Library", ref verticalOffset);
        AddRow("Peter Wayne", "GameMaster Audio Pro Sound Collection", ref verticalOffset);
        AddRow("John '00' Fleming", "Title Music", ref verticalOffset);
        AddRow("staswalle", "Loading Screen Music", ref verticalOffset);
        AddHeader(localization.format("Header_CommunityTeam"), ref verticalOffset);
        string[] obj = new string[15]
        {
            "Deathismad", "James", "Retuuyo", "Fran-war", "SongPhoenix", "Lu", "Morkva", "Reaver", "Shadow", "Yarrrr",
            "DeusExMachina", "Pablo824", "Genestic12", "Armaros", "Great Hero J"
        };
        Array.Sort(obj);
        AddRowColumns(obj, ref verticalOffset);
        AddHeader(localization.format("Header_MapCreators"), ref verticalOffset);
        string[] obj2 = new string[35]
        {
            "Nicolas \"Putin3D\" Arisi", "Mia \"Myria\" Brookman", "Ben \"Paladin\" Hoefer", "Nathan \"Wolf_Maniac\" Zwerka", "Nolan \"Azz\" Ross", "Husky", "Emily Barry", "Justin \"Gamez2much\" Morton", "Terran \"Spyjack\" Orion", "Alex \"Rain\" Storanov",
            "Amanda \"Mooki2much\" Hubler", "Joshua \"Storm_Epidemic\" Rist", "Th3o", "Diesel_Sisel", "Misterl212", "Mitch \"Sketches\" Wheaton", "AnimaticFreak", "NSTM", "Maciej \"Renaxon\" Maziarz", "Daniel \"danaby2\" Segboer",
            "Dug", "Thom \"Spebby\" Mott", "Steven \"MeloCa\" Nadeau", "Ethan \"Vilespring\" Lossner", "SluggedCascade", "Sam \"paper_walls84\" Clerke", "clue", "Vilaskis \"BATTLEKOT\" Shaleshev", "Andrii \"TheCubicNoobik\" Vitiv", "Oleksandr \"BlackLion\" Shcherba",
            "Dmitriy \"Potatoes\" Usenko", "Liya \"Ms.Evrika\" Bognat", "Denis \"FlodotelitoKifo\" Souza", "Joao \"L2\" Vitor", "Josh \"Leprechan12\" Hogan"
        };
        Array.Sort(obj2);
        AddRowColumns(obj2, ref verticalOffset);
        scrollBox.contentSizeOffset = new Vector2(0f, verticalOffset);
    }

    private static void AddHeader(string key, ref int verticalOffset)
    {
        ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
        sleekLabel.positionOffset_Y = verticalOffset;
        sleekLabel.sizeOffset_Y = 50;
        sleekLabel.sizeScale_X = 1f;
        sleekLabel.fontAlignment = TextAnchor.MiddleCenter;
        sleekLabel.fontSize = ESleekFontSize.Large;
        sleekLabel.text = localization.format(key);
        scrollBox.AddChild(sleekLabel);
        verticalOffset += sleekLabel.sizeOffset_Y;
    }

    private static void AddRow(string contributor, string contribution, ref int verticalOffset)
    {
        ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
        sleekLabel.positionOffset_Y = verticalOffset;
        sleekLabel.sizeOffset_Y = 30;
        sleekLabel.sizeScale_X = 1f;
        sleekLabel.fontAlignment = TextAnchor.MiddleLeft;
        sleekLabel.fontSize = ESleekFontSize.Medium;
        sleekLabel.text = contributor;
        scrollBox.AddChild(sleekLabel);
        ISleekLabel sleekLabel2 = Glazier.Get().CreateLabel();
        sleekLabel2.positionOffset_Y = verticalOffset;
        sleekLabel2.sizeOffset_Y = 30;
        sleekLabel2.sizeScale_X = 1f;
        sleekLabel2.fontAlignment = TextAnchor.MiddleRight;
        sleekLabel2.fontSize = ESleekFontSize.Medium;
        sleekLabel2.text = contribution;
        scrollBox.AddChild(sleekLabel2);
        verticalOffset += 30;
    }

    private static void AddRowColumns(string[] contributors, ref int verticalOffset)
    {
        int num = 0;
        foreach (string text in contributors)
        {
            ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
            sleekLabel.positionOffset_Y = verticalOffset;
            sleekLabel.positionScale_X = (float)num * 0.5f;
            sleekLabel.sizeOffset_Y = 30;
            sleekLabel.sizeScale_X = 0.5f;
            sleekLabel.fontAlignment = TextAnchor.MiddleCenter;
            sleekLabel.fontSize = ESleekFontSize.Medium;
            sleekLabel.text = text;
            scrollBox.AddChild(sleekLabel);
            num++;
            if (num >= 2)
            {
                num = 0;
                verticalOffset += 30;
            }
        }
        if (num > 0)
        {
            verticalOffset += 30;
        }
    }
}
