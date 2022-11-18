using UnityEngine;

namespace SDG.Unturned;

public class PlayerDeathUI
{
    private static SleekFullscreenBox container;

    public static Local localization;

    public static bool active;

    private static ISleekBox causeBox;

    public static SleekButtonIcon homeButton;

    public static SleekButtonIcon respawnButton;

    private static bool containerOnScreen;

    public static void open(bool fromDeath)
    {
        if (!active)
        {
            active = true;
            synchronizeDeathCause();
            if (fromDeath && PlayerLife.deathCause != EDeathCause.SUICIDE && OptionsSettings.music && Provider.isServer)
            {
                MainCamera.instance.GetComponent<AudioSource>().Play();
            }
            if (Player.player.isPluginWidgetFlagActive(EPluginWidgetFlags.ShowDeathMenu) && !containerOnScreen)
            {
                containerOnScreen = true;
                container.AnimateIntoView();
            }
        }
    }

    public static void close()
    {
        if (active)
        {
            active = false;
            MainCamera.instance.GetComponent<AudioSource>().Stop();
            if (containerOnScreen)
            {
                containerOnScreen = false;
                container.AnimateOutOfView(0f, 1f);
            }
        }
    }

    private static void synchronizeDeathCause()
    {
        if (PlayerLife.deathCause == EDeathCause.BLEEDING)
        {
            causeBox.text = localization.format("Bleeding");
        }
        else if (PlayerLife.deathCause == EDeathCause.BONES)
        {
            causeBox.text = localization.format("Bones");
        }
        else if (PlayerLife.deathCause == EDeathCause.FREEZING)
        {
            causeBox.text = localization.format("Freezing");
        }
        else if (PlayerLife.deathCause == EDeathCause.BURNING)
        {
            causeBox.text = localization.format("Burning");
        }
        else if (PlayerLife.deathCause == EDeathCause.FOOD)
        {
            causeBox.text = localization.format("Food");
        }
        else if (PlayerLife.deathCause == EDeathCause.WATER)
        {
            causeBox.text = localization.format("Water");
        }
        else if (PlayerLife.deathCause == EDeathCause.GUN || PlayerLife.deathCause == EDeathCause.MELEE || PlayerLife.deathCause == EDeathCause.PUNCH || PlayerLife.deathCause == EDeathCause.ROADKILL || PlayerLife.deathCause == EDeathCause.GRENADE || PlayerLife.deathCause == EDeathCause.MISSILE || PlayerLife.deathCause == EDeathCause.CHARGE || PlayerLife.deathCause == EDeathCause.SPLASH)
        {
            SteamPlayer steamPlayer = PlayerTool.getSteamPlayer(PlayerLife.deathKiller);
            string text;
            string text2;
            if (steamPlayer != null)
            {
                text = steamPlayer.playerID.characterName;
                text2 = steamPlayer.playerID.playerName;
            }
            else
            {
                text = "?";
                text2 = "?";
            }
            string arg = "";
            if (PlayerLife.deathLimb == ELimb.LEFT_FOOT || PlayerLife.deathLimb == ELimb.LEFT_LEG || PlayerLife.deathLimb == ELimb.RIGHT_FOOT || PlayerLife.deathLimb == ELimb.RIGHT_LEG)
            {
                arg = localization.format("Leg");
            }
            else if (PlayerLife.deathLimb == ELimb.LEFT_HAND || PlayerLife.deathLimb == ELimb.LEFT_ARM || PlayerLife.deathLimb == ELimb.RIGHT_HAND || PlayerLife.deathLimb == ELimb.RIGHT_ARM)
            {
                arg = localization.format("Arm");
            }
            else if (PlayerLife.deathLimb == ELimb.SPINE)
            {
                arg = localization.format("Spine");
            }
            else if (PlayerLife.deathLimb == ELimb.SKULL)
            {
                arg = localization.format("Skull");
            }
            if (PlayerLife.deathCause == EDeathCause.GUN)
            {
                causeBox.text = localization.format("Gun", arg, text, text2);
            }
            else if (PlayerLife.deathCause == EDeathCause.MELEE)
            {
                causeBox.text = localization.format("Melee", arg, text, text2);
            }
            else if (PlayerLife.deathCause == EDeathCause.PUNCH)
            {
                causeBox.text = localization.format("Punch", arg, text, text2);
            }
            else if (PlayerLife.deathCause == EDeathCause.ROADKILL)
            {
                causeBox.text = localization.format("Roadkill", text, text2);
            }
            else if (PlayerLife.deathCause == EDeathCause.GRENADE)
            {
                causeBox.text = localization.format("Grenade", text, text2);
            }
            else if (PlayerLife.deathCause == EDeathCause.MISSILE)
            {
                causeBox.text = localization.format("Missile", text, text2);
            }
            else if (PlayerLife.deathCause == EDeathCause.CHARGE)
            {
                causeBox.text = localization.format("Charge", text, text2);
            }
            else if (PlayerLife.deathCause == EDeathCause.SPLASH)
            {
                causeBox.text = localization.format("Splash", text, text2);
            }
        }
        else if (PlayerLife.deathCause == EDeathCause.ZOMBIE)
        {
            causeBox.text = localization.format("Zombie");
        }
        else if (PlayerLife.deathCause == EDeathCause.ANIMAL)
        {
            causeBox.text = localization.format("Animal");
        }
        else if (PlayerLife.deathCause == EDeathCause.SUICIDE)
        {
            causeBox.text = localization.format("Suicide");
        }
        else if (PlayerLife.deathCause == EDeathCause.KILL)
        {
            causeBox.text = localization.format("Kill");
        }
        else if (PlayerLife.deathCause == EDeathCause.INFECTION)
        {
            causeBox.text = localization.format("Infection");
        }
        else if (PlayerLife.deathCause == EDeathCause.BREATH)
        {
            causeBox.text = localization.format("Breath");
        }
        else if (PlayerLife.deathCause == EDeathCause.ZOMBIE)
        {
            causeBox.text = localization.format("Zombie");
        }
        else if (PlayerLife.deathCause == EDeathCause.VEHICLE)
        {
            causeBox.text = localization.format("Vehicle");
        }
        else if (PlayerLife.deathCause == EDeathCause.SHRED)
        {
            causeBox.text = localization.format("Shred");
        }
        else if (PlayerLife.deathCause == EDeathCause.LANDMINE)
        {
            causeBox.text = localization.format("Landmine");
        }
        else if (PlayerLife.deathCause == EDeathCause.ARENA)
        {
            causeBox.text = localization.format("Arena");
        }
        else if (PlayerLife.deathCause == EDeathCause.SENTRY)
        {
            causeBox.text = localization.format("Sentry");
        }
        else if (PlayerLife.deathCause == EDeathCause.ACID)
        {
            causeBox.text = localization.format("Acid");
        }
        else if (PlayerLife.deathCause == EDeathCause.BOULDER)
        {
            causeBox.text = localization.format("Boulder");
        }
        else if (PlayerLife.deathCause == EDeathCause.BURNER)
        {
            causeBox.text = localization.format("Burner");
        }
        else if (PlayerLife.deathCause == EDeathCause.SPIT)
        {
            causeBox.text = localization.format("Spit");
        }
        else if (PlayerLife.deathCause == EDeathCause.SPARK)
        {
            causeBox.text = localization.format("Spark");
        }
    }

    private static void onClickedHomeButton(ISleekElement button)
    {
        if (!Provider.isServer && Provider.isPvP)
        {
            if (Time.realtimeSinceStartup - Player.player.life.lastDeath < (float)Provider.modeConfigData.Gameplay.Timer_Home)
            {
                return;
            }
        }
        else if (Time.realtimeSinceStartup - Player.player.life.lastRespawn < (float)Provider.modeConfigData.Gameplay.Timer_Respawn)
        {
            return;
        }
        Player.player.life.sendRespawn(atHome: true);
    }

    private static void onClickedRespawnButton(ISleekElement button)
    {
        if (!(Time.realtimeSinceStartup - Player.player.life.lastRespawn < (float)Provider.modeConfigData.Gameplay.Timer_Respawn))
        {
            Player.player.life.sendRespawn(atHome: false);
        }
    }

    public PlayerDeathUI()
    {
        localization = Localization.read("/Player/PlayerDeath.dat");
        Bundle bundle = Bundles.getBundle("/Bundles/Textures/Player/Icons/PlayerDeath/PlayerDeath.unity3d");
        container = new SleekFullscreenBox();
        container.positionScale_Y = 1f;
        container.positionOffset_X = 10;
        container.positionOffset_Y = 10;
        container.sizeOffset_X = -20;
        container.sizeOffset_Y = -20;
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
        PlayerUI.container.AddChild(container);
        active = false;
        containerOnScreen = false;
        causeBox = Glazier.Get().CreateBox();
        causeBox.positionOffset_Y = -25;
        causeBox.positionScale_Y = 0.8f;
        causeBox.sizeOffset_Y = 50;
        causeBox.sizeScale_X = 1f;
        container.AddChild(causeBox);
        homeButton = new SleekButtonIcon(bundle.load<Texture2D>("Home"));
        homeButton.positionOffset_X = -205;
        homeButton.positionOffset_Y = 35;
        homeButton.positionScale_X = 0.5f;
        homeButton.positionScale_Y = 0.8f;
        homeButton.sizeOffset_X = 200;
        homeButton.sizeOffset_Y = 30;
        homeButton.text = localization.format("Home_Button");
        homeButton.tooltip = localization.format("Home_Button_Tooltip");
        homeButton.iconColor = ESleekTint.FOREGROUND;
        homeButton.onClickedButton += onClickedHomeButton;
        container.AddChild(homeButton);
        respawnButton = new SleekButtonIcon(bundle.load<Texture2D>("Respawn"));
        respawnButton.positionOffset_X = 5;
        respawnButton.positionOffset_Y = 35;
        respawnButton.positionScale_X = 0.5f;
        respawnButton.positionScale_Y = 0.8f;
        respawnButton.sizeOffset_X = 200;
        respawnButton.sizeOffset_Y = 30;
        respawnButton.text = localization.format("Respawn_Button");
        respawnButton.tooltip = localization.format("Respawn_Button_Tooltip");
        respawnButton.iconColor = ESleekTint.FOREGROUND;
        respawnButton.onClickedButton += onClickedRespawnButton;
        container.AddChild(respawnButton);
        bundle.unload();
    }
}
