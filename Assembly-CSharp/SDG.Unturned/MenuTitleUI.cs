using UnityEngine;

namespace SDG.Unturned;

public class MenuTitleUI
{
    private static readonly byte STAT_COUNT = 18;

    private static Local localization;

    private static SleekFullscreenBox container;

    public static bool active;

    private static ISleekBox titleBox;

    private static ISleekLabel titleLabel;

    private static ISleekLabel authorLabel;

    private static ISleekButton statButton;

    private static EPlayerStat stat;

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
            container.AnimateOutOfView(0f, 1f);
        }
    }

    private static void onClickedStatButton(ISleekElement button)
    {
        byte b;
        do
        {
            b = (byte)Random.Range(1, STAT_COUNT + 1);
        }
        while (b == (byte)stat);
        stat = (EPlayerStat)b;
        if (stat == EPlayerStat.KILLS_ZOMBIES_NORMAL)
        {
            Provider.provider.statisticsService.userStatisticsService.getStatistic("Kills_Zombies_Normal", out int data);
            Provider.provider.statisticsService.globalStatisticsService.getStatistic("Kills_Zombies_Normal", out long data2);
            statButton.Text = localization.format("Stat_Kills_Zombies_Normal", data.ToString("n0"), data2.ToString("n0"));
        }
        else if (stat == EPlayerStat.KILLS_PLAYERS)
        {
            Provider.provider.statisticsService.userStatisticsService.getStatistic("Kills_Players", out int data3);
            Provider.provider.statisticsService.globalStatisticsService.getStatistic("Kills_Players", out long data4);
            statButton.Text = localization.format("Stat_Kills_Players", data3.ToString("n0"), data4.ToString("n0"));
        }
        else if (stat == EPlayerStat.FOUND_ITEMS)
        {
            Provider.provider.statisticsService.userStatisticsService.getStatistic("Found_Items", out int data5);
            Provider.provider.statisticsService.globalStatisticsService.getStatistic("Found_Items", out long data6);
            statButton.Text = localization.format("Stat_Found_Items", data5.ToString("n0"), data6.ToString("n0"));
        }
        else if (stat == EPlayerStat.FOUND_RESOURCES)
        {
            Provider.provider.statisticsService.userStatisticsService.getStatistic("Found_Resources", out int data7);
            Provider.provider.statisticsService.globalStatisticsService.getStatistic("Found_Resources", out long data8);
            statButton.Text = localization.format("Stat_Found_Resources", data7.ToString("n0"), data8.ToString("n0"));
        }
        else if (stat == EPlayerStat.FOUND_EXPERIENCE)
        {
            Provider.provider.statisticsService.userStatisticsService.getStatistic("Found_Experience", out int data9);
            Provider.provider.statisticsService.globalStatisticsService.getStatistic("Found_Experience", out long data10);
            statButton.Text = localization.format("Stat_Found_Experience", data9.ToString("n0"), data10.ToString("n0"));
        }
        else if (stat == EPlayerStat.KILLS_ZOMBIES_MEGA)
        {
            Provider.provider.statisticsService.userStatisticsService.getStatistic("Kills_Zombies_Mega", out int data11);
            Provider.provider.statisticsService.globalStatisticsService.getStatistic("Kills_Zombies_Mega", out long data12);
            statButton.Text = localization.format("Stat_Kills_Zombies_Mega", data11.ToString("n0"), data12.ToString("n0"));
        }
        else if (stat == EPlayerStat.DEATHS_PLAYERS)
        {
            Provider.provider.statisticsService.userStatisticsService.getStatistic("Deaths_Players", out int data13);
            Provider.provider.statisticsService.globalStatisticsService.getStatistic("Deaths_Players", out long data14);
            statButton.Text = localization.format("Stat_Deaths_Players", data13.ToString("n0"), data14.ToString("n0"));
        }
        else if (stat == EPlayerStat.KILLS_ANIMALS)
        {
            Provider.provider.statisticsService.userStatisticsService.getStatistic("Kills_Animals", out int data15);
            Provider.provider.statisticsService.globalStatisticsService.getStatistic("Kills_Animals", out long data16);
            statButton.Text = localization.format("Stat_Kills_Animals", data15.ToString("n0"), data16.ToString("n0"));
        }
        else if (stat == EPlayerStat.FOUND_CRAFTS)
        {
            Provider.provider.statisticsService.userStatisticsService.getStatistic("Found_Crafts", out int data17);
            Provider.provider.statisticsService.globalStatisticsService.getStatistic("Found_Crafts", out long data18);
            statButton.Text = localization.format("Stat_Found_Crafts", data17.ToString("n0"), data18.ToString("n0"));
        }
        else if (stat == EPlayerStat.FOUND_FISHES)
        {
            Provider.provider.statisticsService.userStatisticsService.getStatistic("Found_Fishes", out int data19);
            Provider.provider.statisticsService.globalStatisticsService.getStatistic("Found_Fishes", out long data20);
            statButton.Text = localization.format("Stat_Found_Fishes", data19.ToString("n0"), data20.ToString("n0"));
        }
        else if (stat == EPlayerStat.FOUND_PLANTS)
        {
            Provider.provider.statisticsService.userStatisticsService.getStatistic("Found_Plants", out int data21);
            Provider.provider.statisticsService.globalStatisticsService.getStatistic("Found_Plants", out long data22);
            statButton.Text = localization.format("Stat_Found_Plants", data21.ToString("n0"), data22.ToString("n0"));
        }
        else if (stat == EPlayerStat.ACCURACY)
        {
            Provider.provider.statisticsService.userStatisticsService.getStatistic("Accuracy_Shot", out int data23);
            Provider.provider.statisticsService.userStatisticsService.getStatistic("Accuracy_Hit", out int data24);
            Provider.provider.statisticsService.globalStatisticsService.getStatistic("Accuracy_Shot", out long data25);
            Provider.provider.statisticsService.globalStatisticsService.getStatistic("Accuracy_Hit", out long data26);
            float num = ((data23 != 0 && data24 != 0) ? ((float)data24 / (float)data23) : 0f);
            double num2 = ((data25 != 0L && data26 != 0L) ? ((double)data26 / (double)data25) : 0.0);
            statButton.Text = localization.format("Stat_Accuracy", data23.ToString("n0"), (float)(int)(num * 10000f) / 100f, data25.ToString("n0"), (double)(long)(num2 * 10000.0) / 100.0);
        }
        else if (stat == EPlayerStat.HEADSHOTS)
        {
            Provider.provider.statisticsService.userStatisticsService.getStatistic("Headshots", out int data27);
            Provider.provider.statisticsService.globalStatisticsService.getStatistic("Headshots", out long data28);
            statButton.Text = localization.format("Stat_Headshots", data27.ToString("n0"), data28.ToString("n0"));
        }
        else if (stat == EPlayerStat.TRAVEL_FOOT)
        {
            Provider.provider.statisticsService.userStatisticsService.getStatistic("Travel_Foot", out int data29);
            Provider.provider.statisticsService.globalStatisticsService.getStatistic("Travel_Foot", out long data30);
            if (OptionsSettings.metric)
            {
                statButton.Text = localization.format("Stat_Travel_Foot", data29.ToString("n0") + " m", data30.ToString("n0") + " m");
            }
            else
            {
                statButton.Text = localization.format("Stat_Travel_Foot", MeasurementTool.MtoYd(data29).ToString("n0") + " yd", MeasurementTool.MtoYd(data30).ToString("n0") + " yd");
            }
        }
        else if (stat == EPlayerStat.TRAVEL_VEHICLE)
        {
            Provider.provider.statisticsService.userStatisticsService.getStatistic("Travel_Vehicle", out int data31);
            Provider.provider.statisticsService.globalStatisticsService.getStatistic("Travel_Vehicle", out long data32);
            if (OptionsSettings.metric)
            {
                statButton.Text = localization.format("Stat_Travel_Vehicle", data31.ToString("n0") + " m", data32.ToString("n0") + " m");
            }
            else
            {
                statButton.Text = localization.format("Stat_Travel_Vehicle", MeasurementTool.MtoYd(data31).ToString("n0") + " yd", MeasurementTool.MtoYd(data32).ToString("n0") + " yd");
            }
        }
        else if (stat == EPlayerStat.ARENA_WINS)
        {
            Provider.provider.statisticsService.userStatisticsService.getStatistic("Arena_Wins", out int data33);
            Provider.provider.statisticsService.globalStatisticsService.getStatistic("Arena_Wins", out long data34);
            statButton.Text = localization.format("Stat_Arena_Wins", data33.ToString("n0"), data34.ToString("n0"));
        }
        else if (stat == EPlayerStat.FOUND_BUILDABLES)
        {
            Provider.provider.statisticsService.userStatisticsService.getStatistic("Found_Buildables", out int data35);
            Provider.provider.statisticsService.globalStatisticsService.getStatistic("Found_Buildables", out long data36);
            statButton.Text = localization.format("Stat_Found_Buildables", data35.ToString("n0"), data36.ToString("n0"));
        }
        else if (stat == EPlayerStat.FOUND_THROWABLES)
        {
            Provider.provider.statisticsService.userStatisticsService.getStatistic("Found_Throwables", out int data37);
            Provider.provider.statisticsService.globalStatisticsService.getStatistic("Found_Throwables", out long data38);
            statButton.Text = localization.format("Stat_Found_Throwables", data37.ToString("n0"), data38.ToString("n0"));
        }
    }

    public MenuTitleUI()
    {
        localization = Localization.read("/Menu/MenuTitle.dat");
        container = new SleekFullscreenBox();
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        MenuUI.container.AddChild(container);
        active = true;
        titleBox = Glazier.Get().CreateBox();
        titleBox.SizeOffset_Y = 100f;
        titleBox.SizeScale_X = 1f;
        container.AddChild(titleBox);
        titleLabel = Glazier.Get().CreateLabel();
        titleLabel.SizeScale_X = 1f;
        titleLabel.SizeOffset_Y = 70f;
        titleLabel.FontSize = ESleekFontSize.Title;
        titleLabel.Text = Provider.APP_NAME;
        titleBox.AddChild(titleLabel);
        authorLabel = Glazier.Get().CreateLabel();
        authorLabel.PositionOffset_Y = 60f;
        authorLabel.SizeScale_X = 1f;
        authorLabel.SizeOffset_Y = 30f;
        authorLabel.Text = localization.format("Author_Label", Provider.APP_VERSION, Provider.APP_AUTHOR);
        titleBox.AddChild(authorLabel);
        statButton = Glazier.Get().CreateButton();
        statButton.PositionOffset_Y = 110f;
        statButton.SizeOffset_Y = 50f;
        statButton.SizeScale_X = 1f;
        statButton.OnClicked += onClickedStatButton;
        container.AddChild(statButton);
        stat = EPlayerStat.NONE;
        onClickedStatButton(statButton);
    }
}
