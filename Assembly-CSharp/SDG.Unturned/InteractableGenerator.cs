using System;
using System.Collections.Generic;
using SDG.NetTransport;
using UnityEngine;

namespace SDG.Unturned;

public class InteractableGenerator : Interactable, IManualOnDestroy
{
    private ushort _capacity;

    private float _wirerange;

    private float _sqrWirerange;

    private float burn;

    private bool _isPowered;

    private ushort _fuel;

    private Transform engine;

    private float lastBurn;

    private bool isWiring;

    private byte[] metadata;

    internal static readonly ClientInstanceMethod<ushort> SendFuel = ClientInstanceMethod<ushort>.Get(typeof(InteractableGenerator), "ReceiveFuel");

    internal static readonly ClientInstanceMethod<bool> SendPowered = ClientInstanceMethod<bool>.Get(typeof(InteractableGenerator), "ReceivePowered");

    private static readonly ServerInstanceMethod<bool> SendToggleRequest = ServerInstanceMethod<bool>.Get(typeof(InteractableGenerator), "ReceiveToggleRequest");

    /// <summary>
    /// Unsorted list of world space generators turned-on and fueled.
    /// </summary>
    private static List<InteractableGenerator> worldCandidates = new List<InteractableGenerator>(40);

    private bool isWorldCandidate;

    public ushort capacity => _capacity;

    public float wirerange => _wirerange;

    public float sqrWirerange => _sqrWirerange;

    public bool isRefillable => fuel < capacity;

    public bool isSiphonable => fuel > 0;

    public bool isPowered => _isPowered;

    public ushort fuel => _fuel;

    public void askBurn(ushort amount)
    {
        if (amount != 0)
        {
            if (amount >= fuel)
            {
                _fuel = 0;
            }
            else
            {
                _fuel -= amount;
            }
            if (Provider.isServer)
            {
                updateState();
            }
        }
    }

    public void askFill(ushort amount)
    {
        if (amount != 0)
        {
            if (amount >= capacity - fuel)
            {
                _fuel = capacity;
            }
            else
            {
                _fuel += amount;
            }
            if (Provider.isServer)
            {
                updateState();
            }
        }
    }

    public void tellFuel(ushort newFuel)
    {
        _fuel = newFuel;
        updateWire();
    }

    public void updatePowered(bool newPowered)
    {
        _isPowered = newPowered;
        updateWire();
    }

    public override void updateState(Asset asset, byte[] state)
    {
        _capacity = ((ItemGeneratorAsset)asset).capacity;
        _wirerange = ((ItemGeneratorAsset)asset).wirerange;
        _sqrWirerange = wirerange * wirerange;
        burn = ((ItemGeneratorAsset)asset).burn;
        _isPowered = state[0] == 1;
        _fuel = BitConverter.ToUInt16(state, 1);
        engine = base.transform.Find("Engine");
        if (Provider.isServer)
        {
            metadata = state;
        }
        updateWire();
    }

    public override void use()
    {
        ClientToggle();
    }

    public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
    {
        if (isPowered)
        {
            message = EPlayerMessage.GENERATOR_OFF;
        }
        else
        {
            message = EPlayerMessage.GENERATOR_ON;
        }
        text = "";
        color = Color.white;
        return true;
    }

    private void updateState()
    {
        if (metadata != null)
        {
            BitConverter.GetBytes(fuel).CopyTo(metadata, 1);
        }
    }

    /// <summary>
    /// Catch exceptions to prevent a broken powerable from breaking all the other powerable items in the area.
    /// </summary>
    private void updatePowerableIsWired(InteractablePower powerable, bool isWired)
    {
        try
        {
            powerable.updateWired(isWired);
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, "Generator caught exception during updateWired for {0}:", powerable.GetSceneHierarchyPath());
        }
    }

    private void updateWire()
    {
        if (engine != null)
        {
            engine.gameObject.SetActive(isPowered && fuel > 0);
        }
        bool flag = isPowered && fuel > 0 && !base.IsChildOfVehicle;
        if (isWorldCandidate != flag)
        {
            isWorldCandidate = flag;
            if (isWorldCandidate)
            {
                worldCandidates.Add(this);
            }
            else
            {
                worldCandidates.RemoveFast(this);
            }
        }
        if (Level.info != null && Level.info.configData != null && Level.info.configData.Has_Global_Electricity)
        {
            return;
        }
        ushort plant = ushort.MaxValue;
        if (base.IsChildOfVehicle)
        {
            BarricadeManager.tryGetPlant(base.transform.parent, out var _, out var _, out plant, out var _);
        }
        List<InteractablePower> list = PowerTool.checkPower(base.transform.position, wirerange, plant);
        for (int i = 0; i < list.Count; i++)
        {
            InteractablePower interactablePower = list[i];
            if (interactablePower.isWired)
            {
                if (isPowered && fuel != 0)
                {
                    continue;
                }
                bool flag2;
                if (plant == ushort.MaxValue)
                {
                    flag2 = IsWorldPositionPowered(interactablePower.transform.position);
                }
                else
                {
                    flag2 = false;
                    List<InteractableGenerator> list2 = PowerTool.checkGenerators(interactablePower.transform.position, PowerTool.MAX_POWER_RANGE, plant);
                    for (int j = 0; j < list2.Count; j++)
                    {
                        if (list2[j] != this && list2[j].isPowered && list2[j].fuel > 0 && (list2[j].transform.position - interactablePower.transform.position).sqrMagnitude < list2[j].sqrWirerange)
                        {
                            flag2 = true;
                            break;
                        }
                    }
                }
                if (!flag2)
                {
                    updatePowerableIsWired(interactablePower, isWired: false);
                }
            }
            else if (isPowered && fuel > 0)
            {
                updatePowerableIsWired(interactablePower, isWired: true);
            }
        }
    }

    public void ManualOnDestroy()
    {
        updatePowered(newPowered: false);
    }

    private void OnEnable()
    {
        lastBurn = Time.realtimeSinceStartup;
    }

    private void Update()
    {
        if (!(Time.realtimeSinceStartup - lastBurn > burn))
        {
            return;
        }
        lastBurn = Time.realtimeSinceStartup;
        if (isPowered)
        {
            if (fuel > 0)
            {
                isWiring = true;
                askBurn(1);
            }
            else if (isWiring)
            {
                isWiring = false;
                updateWire();
            }
        }
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
    public void ReceiveFuel(ushort newFuel)
    {
        tellFuel(newFuel);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
    public void ReceivePowered(bool newPowered)
    {
        updatePowered(newPowered);
    }

    public void ClientToggle()
    {
        SendToggleRequest.Invoke(GetNetId(), ENetReliability.Unreliable, !isPowered);
    }

    [SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 2)]
    public void ReceiveToggleRequest(in ServerInvocationContext context, bool desiredPowered)
    {
        if (isPowered != desiredPowered && BarricadeManager.tryGetRegion(base.transform, out var x, out var y, out var plant, out var region))
        {
            Player player = context.GetPlayer();
            if (!(player == null) && !player.life.isDead && !((base.transform.position - player.transform.position).sqrMagnitude > 400f))
            {
                BarricadeManager.ServerSetGeneratorPoweredInternal(this, x, y, plant, region, !isPowered);
                EffectManager.TriggerFiremodeEffect(base.transform.position);
            }
        }
    }

    internal static bool IsWorldPositionPowered(Vector3 position)
    {
        foreach (InteractableGenerator worldCandidate in worldCandidates)
        {
            if ((worldCandidate.transform.position - position).sqrMagnitude < worldCandidate.sqrWirerange)
            {
                return true;
            }
        }
        return false;
    }

    private void OnDestroy()
    {
        if (isWorldCandidate)
        {
            isWorldCandidate = false;
            worldCandidates.RemoveFast(this);
        }
    }
}
