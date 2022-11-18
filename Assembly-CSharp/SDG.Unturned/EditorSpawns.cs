using UnityEngine;

namespace SDG.Unturned;

public class EditorSpawns : MonoBehaviour
{
    public static readonly byte SAVEDATA_VERSION = 1;

    public static readonly byte MIN_REMOVE_SIZE = 2;

    public static readonly byte MAX_REMOVE_SIZE = 30;

    private static bool _isSpawning;

    public static byte selectedItem;

    public static byte selectedZombie;

    public static byte selectedVehicle;

    public static byte selectedAnimal;

    private static bool _selectedAlt;

    private static Transform _itemSpawn;

    private static Transform _playerSpawn;

    private static Transform _playerSpawnAlt;

    private static Transform _zombieSpawn;

    private static Transform _vehicleSpawn;

    private static Transform _animalSpawn;

    private static Transform _remove;

    private static float _rotation;

    private static byte _radius;

    private static ESpawnMode _spawnMode;

    public static bool isSpawning
    {
        get
        {
            return _isSpawning;
        }
        set
        {
            _isSpawning = value;
            if (!isSpawning)
            {
                itemSpawn.gameObject.SetActive(value: false);
                playerSpawn.gameObject.SetActive(value: false);
                playerSpawnAlt.gameObject.SetActive(value: false);
                zombieSpawn.gameObject.SetActive(value: false);
                vehicleSpawn.gameObject.SetActive(value: false);
                animalSpawn.gameObject.SetActive(value: false);
                remove.gameObject.SetActive(value: false);
            }
        }
    }

    public static bool selectedAlt
    {
        get
        {
            return _selectedAlt;
        }
        set
        {
            _selectedAlt = value;
            playerSpawn.gameObject.SetActive(spawnMode == ESpawnMode.ADD_PLAYER && isSpawning && !selectedAlt);
            playerSpawnAlt.gameObject.SetActive(spawnMode == ESpawnMode.ADD_PLAYER && isSpawning && selectedAlt);
        }
    }

    public static Transform itemSpawn => _itemSpawn;

    public static Transform playerSpawn => _playerSpawn;

    public static Transform playerSpawnAlt => _playerSpawnAlt;

    public static Transform zombieSpawn => _zombieSpawn;

    public static Transform vehicleSpawn => _vehicleSpawn;

    public static Transform animalSpawn => _animalSpawn;

    public static Transform remove => _remove;

    public static float rotation
    {
        get
        {
            return _rotation;
        }
        set
        {
            _rotation = value;
            if (playerSpawn != null)
            {
                playerSpawn.transform.rotation = Quaternion.Euler(0f, rotation, 0f);
            }
            if (playerSpawnAlt != null)
            {
                playerSpawnAlt.transform.rotation = Quaternion.Euler(0f, rotation, 0f);
            }
            if (vehicleSpawn != null)
            {
                vehicleSpawn.transform.rotation = Quaternion.Euler(0f, rotation, 0f);
            }
        }
    }

    public static byte radius
    {
        get
        {
            return _radius;
        }
        set
        {
            _radius = value;
            if (remove != null)
            {
                remove.localScale = new Vector3(radius * 2, radius * 2, radius * 2);
            }
        }
    }

    public static ESpawnMode spawnMode
    {
        get
        {
            return _spawnMode;
        }
        set
        {
            _spawnMode = value;
            itemSpawn.gameObject.SetActive(spawnMode == ESpawnMode.ADD_ITEM && isSpawning);
            playerSpawn.gameObject.SetActive(spawnMode == ESpawnMode.ADD_PLAYER && isSpawning && !selectedAlt);
            playerSpawnAlt.gameObject.SetActive(spawnMode == ESpawnMode.ADD_PLAYER && isSpawning && selectedAlt);
            zombieSpawn.gameObject.SetActive(spawnMode == ESpawnMode.ADD_ZOMBIE && isSpawning);
            vehicleSpawn.gameObject.SetActive(spawnMode == ESpawnMode.ADD_VEHICLE && isSpawning);
            animalSpawn.gameObject.SetActive(spawnMode == ESpawnMode.ADD_ANIMAL && isSpawning);
            remove.gameObject.SetActive((spawnMode == ESpawnMode.REMOVE_ITEM || spawnMode == ESpawnMode.REMOVE_PLAYER || spawnMode == ESpawnMode.REMOVE_ZOMBIE || spawnMode == ESpawnMode.REMOVE_VEHICLE || spawnMode == ESpawnMode.REMOVE_ANIMAL) && isSpawning);
        }
    }

    private void Update()
    {
        if (!isSpawning || EditorInteract.isFlying || !Glazier.Get().ShouldGameProcessInput)
        {
            return;
        }
        if (InputEx.GetKeyDown(ControlsSettings.tool_0))
        {
            if (spawnMode == ESpawnMode.REMOVE_ITEM)
            {
                spawnMode = ESpawnMode.ADD_ITEM;
            }
            else if (spawnMode == ESpawnMode.REMOVE_PLAYER)
            {
                spawnMode = ESpawnMode.ADD_PLAYER;
            }
            else if (spawnMode == ESpawnMode.REMOVE_ZOMBIE)
            {
                spawnMode = ESpawnMode.ADD_ZOMBIE;
            }
            else if (spawnMode == ESpawnMode.REMOVE_VEHICLE)
            {
                spawnMode = ESpawnMode.ADD_VEHICLE;
            }
            else if (spawnMode == ESpawnMode.REMOVE_ANIMAL)
            {
                spawnMode = ESpawnMode.ADD_ANIMAL;
            }
        }
        if (InputEx.GetKeyDown(ControlsSettings.tool_1))
        {
            if (spawnMode == ESpawnMode.ADD_ITEM)
            {
                spawnMode = ESpawnMode.REMOVE_ITEM;
            }
            else if (spawnMode == ESpawnMode.ADD_PLAYER)
            {
                spawnMode = ESpawnMode.REMOVE_PLAYER;
            }
            else if (spawnMode == ESpawnMode.ADD_ZOMBIE)
            {
                spawnMode = ESpawnMode.REMOVE_ZOMBIE;
            }
            else if (spawnMode == ESpawnMode.ADD_VEHICLE)
            {
                spawnMode = ESpawnMode.REMOVE_VEHICLE;
            }
            else if (spawnMode == ESpawnMode.ADD_ANIMAL)
            {
                spawnMode = ESpawnMode.REMOVE_ANIMAL;
            }
        }
        if (EditorInteract.worldHit.transform != null)
        {
            if (spawnMode == ESpawnMode.ADD_ITEM)
            {
                itemSpawn.position = EditorInteract.worldHit.point;
            }
            else if (spawnMode == ESpawnMode.ADD_PLAYER)
            {
                playerSpawn.position = EditorInteract.worldHit.point;
                playerSpawnAlt.position = EditorInteract.worldHit.point;
            }
            else if (spawnMode == ESpawnMode.ADD_ZOMBIE)
            {
                zombieSpawn.position = EditorInteract.worldHit.point + Vector3.up;
            }
            else if (spawnMode == ESpawnMode.ADD_VEHICLE)
            {
                vehicleSpawn.position = EditorInteract.worldHit.point;
            }
            else if (spawnMode == ESpawnMode.ADD_ANIMAL)
            {
                animalSpawn.position = EditorInteract.worldHit.point;
            }
            else if (spawnMode == ESpawnMode.REMOVE_ITEM || spawnMode == ESpawnMode.REMOVE_PLAYER || spawnMode == ESpawnMode.REMOVE_ZOMBIE || spawnMode == ESpawnMode.REMOVE_VEHICLE || spawnMode == ESpawnMode.REMOVE_ANIMAL)
            {
                remove.position = EditorInteract.worldHit.point;
            }
        }
        if (!InputEx.GetKeyDown(ControlsSettings.primary) || !(EditorInteract.worldHit.transform != null))
        {
            return;
        }
        Vector3 point = EditorInteract.worldHit.point;
        if (spawnMode == ESpawnMode.ADD_ITEM)
        {
            if (selectedItem < LevelItems.tables.Count)
            {
                LevelItems.addSpawn(point);
            }
        }
        else if (spawnMode == ESpawnMode.REMOVE_ITEM)
        {
            LevelItems.removeSpawn(point, (int)radius);
        }
        else if (spawnMode == ESpawnMode.ADD_PLAYER)
        {
            LevelPlayers.addSpawn(point, rotation, selectedAlt);
        }
        else if (spawnMode == ESpawnMode.REMOVE_PLAYER)
        {
            LevelPlayers.removeSpawn(point, (int)radius);
        }
        else if (spawnMode == ESpawnMode.ADD_ZOMBIE)
        {
            if (selectedZombie < LevelZombies.tables.Count)
            {
                LevelZombies.addSpawn(point);
            }
        }
        else if (spawnMode == ESpawnMode.REMOVE_ZOMBIE)
        {
            LevelZombies.removeSpawn(point, (int)radius);
        }
        else if (spawnMode == ESpawnMode.ADD_VEHICLE)
        {
            LevelVehicles.addSpawn(point, rotation);
        }
        else if (spawnMode == ESpawnMode.REMOVE_VEHICLE)
        {
            LevelVehicles.removeSpawn(point, (int)radius);
        }
        else if (spawnMode == ESpawnMode.ADD_ANIMAL)
        {
            LevelAnimals.addSpawn(point);
        }
        else if (spawnMode == ESpawnMode.REMOVE_ANIMAL)
        {
            LevelAnimals.removeSpawn(point, (int)radius);
        }
    }

    private void Start()
    {
        _isSpawning = false;
        _itemSpawn = ((GameObject)Object.Instantiate(Resources.Load("Edit/Item"))).transform;
        itemSpawn.name = "Item Spawn";
        itemSpawn.parent = Level.editing;
        itemSpawn.gameObject.SetActive(value: false);
        if (selectedItem < LevelItems.tables.Count)
        {
            itemSpawn.GetComponent<Renderer>().material.color = LevelItems.tables[selectedItem].color;
        }
        _playerSpawn = ((GameObject)Object.Instantiate(Resources.Load("Edit/Player"))).transform;
        playerSpawn.name = "Player Spawn";
        playerSpawn.parent = Level.editing;
        playerSpawn.gameObject.SetActive(value: false);
        _playerSpawnAlt = ((GameObject)Object.Instantiate(Resources.Load("Edit/Player_Alt"))).transform;
        playerSpawnAlt.name = "Player Spawn Alt";
        playerSpawnAlt.parent = Level.editing;
        playerSpawnAlt.gameObject.SetActive(value: false);
        _zombieSpawn = ((GameObject)Object.Instantiate(Resources.Load("Edit/Zombie"))).transform;
        zombieSpawn.name = "Zombie Spawn";
        zombieSpawn.parent = Level.editing;
        zombieSpawn.gameObject.SetActive(value: false);
        if (selectedZombie < LevelZombies.tables.Count)
        {
            zombieSpawn.GetComponent<Renderer>().material.color = LevelZombies.tables[selectedZombie].color;
        }
        _vehicleSpawn = ((GameObject)Object.Instantiate(Resources.Load("Edit/Vehicle"))).transform;
        vehicleSpawn.name = "Vehicle Spawn";
        vehicleSpawn.parent = Level.editing;
        vehicleSpawn.gameObject.SetActive(value: false);
        if (selectedVehicle < LevelVehicles.tables.Count)
        {
            vehicleSpawn.GetComponent<Renderer>().material.color = LevelVehicles.tables[selectedVehicle].color;
            vehicleSpawn.Find("Arrow").GetComponent<Renderer>().material.color = LevelVehicles.tables[selectedVehicle].color;
        }
        _animalSpawn = ((GameObject)Object.Instantiate(Resources.Load("Edit/Animal"))).transform;
        _animalSpawn.name = "Animal Spawn";
        _animalSpawn.parent = Level.editing;
        _animalSpawn.gameObject.SetActive(value: false);
        if (selectedAnimal < LevelAnimals.tables.Count)
        {
            animalSpawn.GetComponent<Renderer>().material.color = LevelAnimals.tables[selectedAnimal].color;
        }
        _remove = ((GameObject)Object.Instantiate(Resources.Load("Edit/Remove"))).transform;
        remove.name = "Remove";
        remove.parent = Level.editing;
        remove.gameObject.SetActive(value: false);
        spawnMode = ESpawnMode.ADD_ITEM;
        load();
    }

    public static void load()
    {
        if (ReadWrite.fileExists(Level.info.path + "/Editor/Spawns.dat", useCloud: false, usePath: false))
        {
            Block block = ReadWrite.readBlock(Level.info.path + "/Editor/Spawns.dat", useCloud: false, usePath: false, 1);
            rotation = block.readSingle();
            radius = block.readByte();
        }
        else
        {
            rotation = 0f;
            radius = MIN_REMOVE_SIZE;
        }
    }

    public static void save()
    {
        Block block = new Block();
        block.writeByte(SAVEDATA_VERSION);
        block.writeSingle(rotation);
        block.writeByte(radius);
        ReadWrite.writeBlock(Level.info.path + "/Editor/Spawns.dat", useCloud: false, usePath: false, block);
    }
}
