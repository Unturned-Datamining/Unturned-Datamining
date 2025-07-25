namespace SDG.Unturned;

public class ModeConfigData
{
    public ItemsConfigData Items;

    public VehiclesConfigData Vehicles;

    public ZombiesConfigData Zombies;

    public AnimalsConfigData Animals;

    public BarricadesConfigData Barricades;

    public StructuresConfigData Structures;

    public PlayersConfigData Players;

    public ObjectConfigData Objects;

    public EventsConfigData Events;

    public GameplayConfigData Gameplay;

    public ModeConfigData(EGameMode mode)
    {
        Items = new ItemsConfigData(mode);
        Vehicles = new VehiclesConfigData(mode);
        Zombies = new ZombiesConfigData(mode);
        Animals = new AnimalsConfigData(mode);
        Barricades = new BarricadesConfigData(mode);
        Structures = new StructuresConfigData(mode);
        Players = new PlayersConfigData(mode);
        Objects = new ObjectConfigData(mode);
        Events = new EventsConfigData(mode);
        Gameplay = new GameplayConfigData(mode);
    }

    public void InitSingleplayerDefaults()
    {
        Players.InitSingleplayerDefaults();
        Gameplay.InitSingleplayerDefaults();
    }

    public static ModeConfigData CreateDefault(EGameMode mode, bool singleplayer)
    {
        ModeConfigData modeConfigData = new ModeConfigData(mode);
        if (singleplayer)
        {
            modeConfigData.InitSingleplayerDefaults();
        }
        return modeConfigData;
    }
}
