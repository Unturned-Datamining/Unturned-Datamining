using System;
using SDG.Framework.IO.FormattedFiles;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace SDG.Framework.Devkit;

public class KillVolume : LevelVolume<KillVolume, KillVolumeManager>
{
    private class Menu : SleekWrapper
    {
        private KillVolume volume;

        public Menu(KillVolume volume)
        {
            this.volume = volume;
            base.sizeOffset_X = 400;
            base.sizeOffset_Y = 190;
            ISleekToggle sleekToggle = Glazier.Get().CreateToggle();
            sleekToggle.sizeOffset_X = 40;
            sleekToggle.sizeOffset_Y = 40;
            sleekToggle.state = volume.killPlayers;
            sleekToggle.addLabel("Kill Players", ESleekSide.RIGHT);
            sleekToggle.onToggled += OnKillPlayersToggled;
            AddChild(sleekToggle);
            ISleekToggle sleekToggle2 = Glazier.Get().CreateToggle();
            sleekToggle2.positionOffset_Y = 40;
            sleekToggle2.sizeOffset_X = 40;
            sleekToggle2.sizeOffset_Y = 40;
            sleekToggle2.state = volume.killZombies;
            sleekToggle2.addLabel("Kill Zombies", ESleekSide.RIGHT);
            sleekToggle2.onToggled += OnKillZombiesToggled;
            AddChild(sleekToggle2);
            ISleekToggle sleekToggle3 = Glazier.Get().CreateToggle();
            sleekToggle3.positionOffset_Y = 80;
            sleekToggle3.sizeOffset_X = 40;
            sleekToggle3.sizeOffset_Y = 40;
            sleekToggle3.state = volume.killAnimals;
            sleekToggle3.addLabel("Kill Animals", ESleekSide.RIGHT);
            sleekToggle3.onToggled += OnKillAnimalsToggled;
            AddChild(sleekToggle3);
            ISleekToggle sleekToggle4 = Glazier.Get().CreateToggle();
            sleekToggle4.positionOffset_Y = 120;
            sleekToggle4.sizeOffset_X = 40;
            sleekToggle4.sizeOffset_Y = 40;
            sleekToggle4.state = volume.killVehicles;
            sleekToggle4.addLabel("Kill Vehicles", ESleekSide.RIGHT);
            sleekToggle4.onToggled += OnKillVehiclesToggled;
            AddChild(sleekToggle4);
            SleekButtonStateEnum<EDeathCause> sleekButtonStateEnum = new SleekButtonStateEnum<EDeathCause>();
            sleekButtonStateEnum.positionOffset_Y = 160;
            sleekButtonStateEnum.sizeOffset_X = 200;
            sleekButtonStateEnum.sizeOffset_Y = 30;
            sleekButtonStateEnum.SetEnum(volume.deathCause);
            sleekButtonStateEnum.addLabel("Death Cause", ESleekSide.RIGHT);
            sleekButtonStateEnum.OnSwappedEnum = (Action<SleekButtonStateEnum<EDeathCause>, EDeathCause>)Delegate.Combine(sleekButtonStateEnum.OnSwappedEnum, new Action<SleekButtonStateEnum<EDeathCause>, EDeathCause>(OnSwappedDeathCause));
            AddChild(sleekButtonStateEnum);
        }

        private void OnKillPlayersToggled(ISleekToggle toggle, bool state)
        {
            volume.killPlayers = state;
        }

        private void OnKillZombiesToggled(ISleekToggle toggle, bool state)
        {
            volume.killZombies = state;
        }

        private void OnKillAnimalsToggled(ISleekToggle toggle, bool state)
        {
            volume.killAnimals = state;
        }

        private void OnKillVehiclesToggled(ISleekToggle toggle, bool state)
        {
            volume.killVehicles = state;
        }

        private void OnSwappedDeathCause(SleekButtonStateEnum<EDeathCause> button, EDeathCause deathCause)
        {
            volume.deathCause = deathCause;
        }
    }

    public bool killPlayers = true;

    public bool killZombies = true;

    public bool killAnimals = true;

    public bool killVehicles;

    public EDeathCause deathCause = EDeathCause.BURNING;

    public override ISleekElement CreateMenu()
    {
        ISleekElement sleekElement = new Menu(this);
        AppendBaseMenu(sleekElement);
        return sleekElement;
    }

    protected override void readHierarchyItem(IFormattedFileReader reader)
    {
        base.readHierarchyItem(reader);
        killPlayers = reader.readValue<bool>("Kill_Players");
        killZombies = reader.readValue<bool>("Kill_Zombies");
        killAnimals = reader.readValue<bool>("Kill_Animals");
        killVehicles = reader.readValue<bool>("Kill_Vehicles");
        deathCause = reader.readValue<EDeathCause>("Death_Cause");
    }

    protected override void writeHierarchyItem(IFormattedFileWriter writer)
    {
        base.writeHierarchyItem(writer);
        writer.writeValue("Kill_Players", killPlayers);
        writer.writeValue("Kill_Zombies", killZombies);
        writer.writeValue("Kill_Animals", killAnimals);
        writer.writeValue("Kill_Vehicles", killVehicles);
        writer.writeValue("Death_Cause", deathCause);
    }

    protected override void Awake()
    {
        forceShouldAddCollider = true;
        base.Awake();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger || !SDG.Unturned.Provider.isServer)
        {
            return;
        }
        if (other.CompareTag("Player"))
        {
            if (killPlayers)
            {
                Player player = DamageTool.getPlayer(other.transform);
                if (player != null)
                {
                    DamageTool.damage(player, deathCause, ELimb.SPINE, CSteamID.Nil, Vector3.up, 101f, 1f, out var _, applyGlobalArmorMultiplier: false);
                }
            }
        }
        else if (other.CompareTag("Agent"))
        {
            if (!killZombies && !killAnimals)
            {
                return;
            }
            Zombie zombie = DamageTool.getZombie(other.transform);
            if (zombie != null)
            {
                if (killZombies)
                {
                    DamageZombieParameters parameters = DamageZombieParameters.makeInstakill(zombie);
                    parameters.instigator = this;
                    DamageTool.damageZombie(parameters, out var _, out var _);
                }
            }
            else if (killAnimals)
            {
                Animal animal = DamageTool.getAnimal(other.transform);
                if (animal != null)
                {
                    DamageAnimalParameters parameters2 = DamageAnimalParameters.makeInstakill(animal);
                    parameters2.instigator = this;
                    DamageTool.damageAnimal(parameters2, out var _, out var _);
                }
            }
        }
        else
        {
            if (!other.CompareTag("Vehicle"))
            {
                return;
            }
            InteractableVehicle vehicle = DamageTool.getVehicle(other.transform);
            if (!(vehicle != null) || vehicle.isDead)
            {
                return;
            }
            if (killPlayers)
            {
                for (int num = vehicle.passengers.Length - 1; num >= 0; num--)
                {
                    Passenger passenger = vehicle.passengers[num];
                    if (passenger != null && passenger.player != null)
                    {
                        Player player2 = passenger.player.player;
                        if (!(player2 == null))
                        {
                            DamageTool.damage(player2, deathCause, ELimb.SPINE, CSteamID.Nil, Vector3.up, 101f, 1f, out var _, applyGlobalArmorMultiplier: false);
                        }
                    }
                }
            }
            if (killVehicles && !vehicle.isDead)
            {
                DamageTool.damage(vehicle, damageTires: false, Vector3.zero, isRepairing: false, 65000f, 1f, canRepair: false, out var _, default(CSteamID), EDamageOrigin.Kill_Volume);
            }
        }
    }
}
