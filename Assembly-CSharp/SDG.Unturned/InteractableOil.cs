using System;
using UnityEngine;

namespace SDG.Unturned;

public class InteractableOil : InteractablePower
{
    private ushort _fuel;

    private byte[] metadata;

    private Transform engine;

    private Animation root;

    private float lastDrilled;

    internal static readonly ClientInstanceMethod<ushort> SendFuel = ClientInstanceMethod<ushort>.Get(typeof(InteractableOil), "ReceiveFuel");

    public ushort fuel => _fuel;

    public ushort capacity { get; protected set; }

    public bool isRefillable => fuel < capacity;

    public bool isSiphonable => fuel > 0;

    public void tellFuel(ushort newFuel)
    {
        _fuel = newFuel;
    }

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

    private void UpdateVisual()
    {
        if (engine != null)
        {
            engine.gameObject.SetActive(base.isWired);
        }
        if (!(root != null))
        {
            return;
        }
        if (base.isWired)
        {
            root.Play();
            {
                foreach (AnimationState item in root)
                {
                    item.normalizedTime = UnityEngine.Random.value;
                }
                return;
            }
        }
        root.Stop();
    }

    protected override void updateWired()
    {
        UpdateVisual();
    }

    public override void updateState(Asset asset, byte[] state)
    {
        base.updateState(asset, state);
        capacity = ((ItemOilPumpAsset)asset).fuelCapacity;
        _fuel = BitConverter.ToUInt16(state, 0);
        if (Provider.isServer)
        {
            metadata = state;
        }
        RefreshIsConnectedToPowerWithoutNotify();
        UpdateVisual();
    }

    public override bool checkUseable()
    {
        return fuel > 0;
    }

    public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
    {
        message = EPlayerMessage.VOLUME_FUEL;
        text = "";
        color = Color.white;
        return true;
    }

    private void updateState()
    {
        if (metadata != null)
        {
            BitConverter.GetBytes(fuel).CopyTo(metadata, 0);
        }
    }

    private void Update()
    {
        if (!base.isWired)
        {
            lastDrilled = Time.realtimeSinceStartup;
        }
        else if (Time.realtimeSinceStartup - lastDrilled > 2f)
        {
            lastDrilled = Time.realtimeSinceStartup;
            if (fuel < capacity)
            {
                askFill(1);
            }
        }
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
    public void ReceiveFuel(ushort newFuel)
    {
        tellFuel(newFuel);
    }
}
