using SDG.Framework.Devkit;

namespace SDG.Unturned;

public class LevelVisibility
{
    public static readonly byte SAVEDATA_VERSION = 2;

    public static readonly byte OBJECT_REGIONS = 4;

    private static bool _roadsVisible;

    private static bool _navigationVisible;

    private static bool _itemsVisible;

    private static bool _playersVisible;

    private static bool _zombiesVisible;

    private static bool _vehiclesVisible;

    private static bool _borderVisible;

    private static bool _animalsVisible;

    public static bool roadsVisible
    {
        get
        {
            return _roadsVisible;
        }
        set
        {
            _roadsVisible = value;
            LevelRoads.setEnabled(roadsVisible);
        }
    }

    public static bool navigationVisible
    {
        get
        {
            return _navigationVisible;
        }
        set
        {
            _navigationVisible = value;
            LevelNavigation.setEnabled(navigationVisible);
        }
    }

    public static bool nodesVisible
    {
        get
        {
            return SpawnpointSystemV2.Get().IsVisible;
        }
        set
        {
            SpawnpointSystemV2.Get().IsVisible = value;
        }
    }

    public static bool itemsVisible
    {
        get
        {
            return _itemsVisible;
        }
        set
        {
            _itemsVisible = value;
            LevelItems.setEnabled(itemsVisible);
        }
    }

    public static bool playersVisible
    {
        get
        {
            return _playersVisible;
        }
        set
        {
            _playersVisible = value;
            LevelPlayers.setEnabled(playersVisible);
        }
    }

    public static bool zombiesVisible
    {
        get
        {
            return _zombiesVisible;
        }
        set
        {
            _zombiesVisible = value;
            LevelZombies.setEnabled(zombiesVisible);
        }
    }

    public static bool vehiclesVisible
    {
        get
        {
            return _vehiclesVisible;
        }
        set
        {
            _vehiclesVisible = value;
            LevelVehicles.setEnabled(vehiclesVisible);
        }
    }

    public static bool borderVisible
    {
        get
        {
            return _borderVisible;
        }
        set
        {
            _borderVisible = value;
            Level.setEnabled(borderVisible);
        }
    }

    public static bool animalsVisible
    {
        get
        {
            return _animalsVisible;
        }
        set
        {
            _animalsVisible = value;
            LevelAnimals.setEnabled(animalsVisible);
        }
    }

    public static void load()
    {
        if (Level.isVR)
        {
            roadsVisible = false;
            _navigationVisible = false;
            _itemsVisible = false;
            playersVisible = false;
            _zombiesVisible = false;
            _vehiclesVisible = false;
            borderVisible = false;
            _animalsVisible = false;
        }
        else
        {
            if (!Level.isEditor)
            {
                return;
            }
            if (ReadWrite.fileExists(Level.info.path + "/Level/Visibility.dat", useCloud: false, usePath: false))
            {
                River river = new River(Level.info.path + "/Level/Visibility.dat", usePath: false);
                byte b = river.readByte();
                if (b > 0)
                {
                    roadsVisible = river.readBoolean();
                    navigationVisible = river.readBoolean();
                    river.readBoolean();
                    itemsVisible = river.readBoolean();
                    playersVisible = river.readBoolean();
                    zombiesVisible = river.readBoolean();
                    vehiclesVisible = river.readBoolean();
                    borderVisible = river.readBoolean();
                    if (b > 1)
                    {
                        animalsVisible = river.readBoolean();
                    }
                    else
                    {
                        _animalsVisible = true;
                    }
                    river.closeRiver();
                }
            }
            else
            {
                _roadsVisible = true;
                _navigationVisible = true;
                _itemsVisible = true;
                _playersVisible = true;
                _zombiesVisible = true;
                _vehiclesVisible = true;
                _borderVisible = true;
                _animalsVisible = true;
            }
        }
    }

    public static void save()
    {
        River river = new River(Level.info.path + "/Level/Visibility.dat", usePath: false);
        river.writeByte(SAVEDATA_VERSION);
        river.writeBoolean(roadsVisible);
        river.writeBoolean(navigationVisible);
        river.writeBoolean(nodesVisible);
        river.writeBoolean(itemsVisible);
        river.writeBoolean(playersVisible);
        river.writeBoolean(zombiesVisible);
        river.writeBoolean(vehiclesVisible);
        river.writeBoolean(borderVisible);
        river.writeBoolean(animalsVisible);
        river.closeRiver();
    }
}
