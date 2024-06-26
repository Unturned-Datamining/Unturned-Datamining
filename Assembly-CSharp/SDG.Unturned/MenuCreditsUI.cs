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
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.PositionScale_Y = -1f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        MenuUI.container.AddChild(container);
        active = false;
        returnButton = new SleekButtonIcon(MenuPauseUI.icons.load<Texture2D>("Exit"));
        returnButton.PositionOffset_X = -250f;
        returnButton.PositionOffset_Y = 100f;
        returnButton.PositionScale_X = 0.5f;
        returnButton.SizeOffset_X = 500f;
        returnButton.SizeOffset_Y = 50f;
        returnButton.text = MenuPauseUI.localization.format("Return_Button");
        returnButton.tooltip = MenuPauseUI.localization.format("Return_Button_Tooltip");
        returnButton.onClickedButton += onClickedReturnButton;
        returnButton.fontSize = ESleekFontSize.Medium;
        returnButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(returnButton);
        creditsBox = Glazier.Get().CreateBox();
        creditsBox.PositionOffset_X = -250f;
        creditsBox.PositionOffset_Y = 160f;
        creditsBox.PositionScale_X = 0.5f;
        creditsBox.SizeOffset_X = 500f;
        creditsBox.SizeOffset_Y = -260f;
        creditsBox.SizeScale_Y = 1f;
        creditsBox.FontSize = ESleekFontSize.Medium;
        container.AddChild(creditsBox);
        scrollBox = Glazier.Get().CreateScrollView();
        scrollBox.PositionOffset_X = 5f;
        scrollBox.PositionOffset_Y = 5f;
        scrollBox.SizeOffset_X = -10f;
        scrollBox.SizeOffset_Y = -10f;
        scrollBox.SizeScale_X = 1f;
        scrollBox.SizeScale_Y = 1f;
        scrollBox.ScaleContentToWidth = true;
        creditsBox.AddChild(scrollBox);
        float verticalOffset = 0f;
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
        string[] obj = new string[16]
        {
            "Deathismad", "James", "Retuuyo", "Fran-war", "SongPhoenix", "Lu", "Morkva", "Reaver", "Shadow", "Yarrrr",
            "DeusExMachina", "Pablo824", "Genestic12", "Armaros", "Great Hero J", "SomeCatIDK"
        };
        Array.Sort(obj);
        AddRowColumns(obj, ref verticalOffset);
        AddHeader(localization.format("Header_MapCreators"), ref verticalOffset);
        string[] obj2 = new string[36]
        {
            "Nicolas \"Putin3D\" Arisi", "Mia \"Myria\" Brookman", "Ben \"Paladin\" Hoefer", "Nathan \"Wolf_Maniac\" Zwerka", "Nolan \"Azz\" Ross", "Husky", "Emily Barry", "Justin \"Gamez2much\" Morton", "Terran \"Spyjack\" Orion", "Alex \"Rain\" Storanov",
            "Amanda \"Mooki2much\" Hubler", "Joshua \"Storm_Epidemic\" Rist", "Th3o", "Diesel_Sisel", "Misterl212", "Mitch \"Sketches\" Wheaton", "AnimaticFreak", "NSTM", "Maciej \"Renaxon\" Maziarz", "Daniel \"danaby2\" Segboer",
            "Dug", "Thom \"Spebby\" Mott", "Steven \"MeloCa\" Nadeau", "Ethan \"Vilespring\" Lossner", "SluggedCascade", "Sam \"paper_walls84\" Clerke", "clue", "Vilaskis \"BATTLEKOT\" Shaleshev", "Andrii \"TheCubicNoobik\" Vitiv", "Oleksandr \"BlackLion\" Shcherba",
            "Dmitriy \"Potatoes\" Usenko", "Liya \"Ms.Evrika\" Bognat", "Denis \"FlodotelitoKifo\" Souza", "Joao \"L2\" Vitor", "Josh \"Leprechan12\" Hogan", "Toothy Deerryte"
        };
        Array.Sort(obj2);
        AddRowColumns(obj2, ref verticalOffset);
        scrollBox.ContentSizeOffset = new Vector2(0f, verticalOffset);
    }

    private static void AddHeader(string key, ref float verticalOffset)
    {
        ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
        sleekLabel.PositionOffset_Y = verticalOffset;
        sleekLabel.SizeOffset_Y = 50f;
        sleekLabel.SizeScale_X = 1f;
        sleekLabel.TextAlignment = TextAnchor.MiddleCenter;
        sleekLabel.FontSize = ESleekFontSize.Large;
        sleekLabel.Text = localization.format(key);
        scrollBox.AddChild(sleekLabel);
        verticalOffset += sleekLabel.SizeOffset_Y;
    }

    private static void AddRow(string contributor, string contribution, ref float verticalOffset)
    {
        ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
        sleekLabel.PositionOffset_Y = verticalOffset;
        sleekLabel.SizeOffset_Y = 30f;
        sleekLabel.SizeScale_X = 1f;
        sleekLabel.TextAlignment = TextAnchor.MiddleLeft;
        sleekLabel.FontSize = ESleekFontSize.Medium;
        sleekLabel.Text = contributor;
        scrollBox.AddChild(sleekLabel);
        ISleekLabel sleekLabel2 = Glazier.Get().CreateLabel();
        sleekLabel2.PositionOffset_Y = verticalOffset;
        sleekLabel2.SizeOffset_Y = 30f;
        sleekLabel2.SizeScale_X = 1f;
        sleekLabel2.TextAlignment = TextAnchor.MiddleRight;
        sleekLabel2.FontSize = ESleekFontSize.Medium;
        sleekLabel2.Text = contribution;
        scrollBox.AddChild(sleekLabel2);
        verticalOffset += 30f;
    }

    private static void AddRowColumns(string[] contributors, ref float verticalOffset)
    {
        int num = 0;
        foreach (string text in contributors)
        {
            ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
            sleekLabel.PositionOffset_Y = verticalOffset;
            sleekLabel.PositionScale_X = (float)num * 0.5f;
            sleekLabel.SizeOffset_Y = 30f;
            sleekLabel.SizeScale_X = 0.5f;
            sleekLabel.TextAlignment = TextAnchor.MiddleCenter;
            sleekLabel.FontSize = ESleekFontSize.Medium;
            sleekLabel.Text = text;
            scrollBox.AddChild(sleekLabel);
            num++;
            if (num >= 2)
            {
                num = 0;
                verticalOffset += 30f;
            }
        }
        if (num > 0)
        {
            verticalOffset += 30f;
        }
    }
}
