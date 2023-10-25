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

    /// <summary>
    /// Has the contained been animated into visibility on-screen?
    /// Used to disable animating out if disabled.
    /// </summary>
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
            causeBox.Text = localization.format("Bleeding");
        }
        else if (PlayerLife.deathCause == EDeathCause.BONES)
        {
            causeBox.Text = localization.format("Bones");
        }
        else if (PlayerLife.deathCause == EDeathCause.FREEZING)
        {
            causeBox.Text = localization.format("Freezing");
        }
        else if (PlayerLife.deathCause == EDeathCause.BURNING)
        {
            causeBox.Text = localization.format("Burning");
        }
        else if (PlayerLife.deathCause == EDeathCause.FOOD)
        {
            causeBox.Text = localization.format("Food");
        }
        else if (PlayerLife.deathCause == EDeathCause.WATER)
        {
            causeBox.Text = localization.format("Water");
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
                causeBox.Text = localization.format("Gun", arg, text, text2);
            }
            else if (PlayerLife.deathCause == EDeathCause.MELEE)
            {
                causeBox.Text = localization.format("Melee", arg, text, text2);
            }
            else if (PlayerLife.deathCause == EDeathCause.PUNCH)
            {
                causeBox.Text = localization.format("Punch", arg, text, text2);
            }
            else if (PlayerLife.deathCause == EDeathCause.ROADKILL)
            {
                causeBox.Text = localization.format("Roadkill", text, text2);
            }
            else if (PlayerLife.deathCause == EDeathCause.GRENADE)
            {
                causeBox.Text = localization.format("Grenade", text, text2);
            }
            else if (PlayerLife.deathCause == EDeathCause.MISSILE)
            {
                causeBox.Text = localization.format("Missile", text, text2);
            }
            else if (PlayerLife.deathCause == EDeathCause.CHARGE)
            {
                causeBox.Text = localization.format("Charge", text, text2);
            }
            else if (PlayerLife.deathCause == EDeathCause.SPLASH)
            {
                causeBox.Text = localization.format("Splash", text, text2);
            }
        }
        else if (PlayerLife.deathCause == EDeathCause.ZOMBIE)
        {
            causeBox.Text = localization.format("Zombie");
        }
        else if (PlayerLife.deathCause == EDeathCause.ANIMAL)
        {
            causeBox.Text = localization.format("Animal");
        }
        else if (PlayerLife.deathCause == EDeathCause.SUICIDE)
        {
            causeBox.Text = localization.format("Suicide");
        }
        else if (PlayerLife.deathCause == EDeathCause.KILL)
        {
            causeBox.Text = localization.format("Kill");
        }
        else if (PlayerLife.deathCause == EDeathCause.INFECTION)
        {
            causeBox.Text = localization.format("Infection");
        }
        else if (PlayerLife.deathCause == EDeathCause.BREATH)
        {
            causeBox.Text = localization.format("Breath");
        }
        else if (PlayerLife.deathCause == EDeathCause.ZOMBIE)
        {
            causeBox.Text = localization.format("Zombie");
        }
        else if (PlayerLife.deathCause == EDeathCause.VEHICLE)
        {
            causeBox.Text = localization.format("Vehicle");
        }
        else if (PlayerLife.deathCause == EDeathCause.SHRED)
        {
            causeBox.Text = localization.format("Shred");
        }
        else if (PlayerLife.deathCause == EDeathCause.LANDMINE)
        {
            causeBox.Text = localization.format("Landmine");
        }
        else if (PlayerLife.deathCause == EDeathCause.ARENA)
        {
            causeBox.Text = localization.format("Arena");
        }
        else if (PlayerLife.deathCause == EDeathCause.SENTRY)
        {
            causeBox.Text = localization.format("Sentry");
        }
        else if (PlayerLife.deathCause == EDeathCause.ACID)
        {
            causeBox.Text = localization.format("Acid");
        }
        else if (PlayerLife.deathCause == EDeathCause.BOULDER)
        {
            causeBox.Text = localization.format("Boulder");
        }
        else if (PlayerLife.deathCause == EDeathCause.BURNER)
        {
            causeBox.Text = localization.format("Burner");
        }
        else if (PlayerLife.deathCause == EDeathCause.SPIT)
        {
            causeBox.Text = localization.format("Spit");
        }
        else if (PlayerLife.deathCause == EDeathCause.SPARK)
        {
            causeBox.Text = localization.format("Spark");
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
        container.PositionScale_Y = 1f;
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        PlayerUI.container.AddChild(container);
        active = false;
        containerOnScreen = false;
        causeBox = Glazier.Get().CreateBox();
        causeBox.PositionOffset_Y = -25f;
        causeBox.PositionScale_Y = 0.8f;
        causeBox.SizeOffset_Y = 50f;
        causeBox.SizeScale_X = 1f;
        container.AddChild(causeBox);
        homeButton = new SleekButtonIcon(bundle.load<Texture2D>("Home"));
        homeButton.PositionOffset_X = -205f;
        homeButton.PositionOffset_Y = 35f;
        homeButton.PositionScale_X = 0.5f;
        homeButton.PositionScale_Y = 0.8f;
        homeButton.SizeOffset_X = 200f;
        homeButton.SizeOffset_Y = 30f;
        homeButton.text = localization.format("Home_Button");
        homeButton.tooltip = localization.format("Home_Button_Tooltip");
        homeButton.iconColor = ESleekTint.FOREGROUND;
        homeButton.onClickedButton += onClickedHomeButton;
        container.AddChild(homeButton);
        respawnButton = new SleekButtonIcon(bundle.load<Texture2D>("Respawn"));
        respawnButton.PositionOffset_X = 5f;
        respawnButton.PositionOffset_Y = 35f;
        respawnButton.PositionScale_X = 0.5f;
        respawnButton.PositionScale_Y = 0.8f;
        respawnButton.SizeOffset_X = 200f;
        respawnButton.SizeOffset_Y = 30f;
        respawnButton.text = localization.format("Respawn_Button");
        respawnButton.tooltip = localization.format("Respawn_Button_Tooltip");
        respawnButton.iconColor = ESleekTint.FOREGROUND;
        respawnButton.onClickedButton += onClickedRespawnButton;
        container.AddChild(respawnButton);
        bundle.unload();
    }
}
