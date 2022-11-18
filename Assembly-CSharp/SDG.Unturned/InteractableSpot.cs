using SDG.NetTransport;
using UnityEngine;

namespace SDG.Unturned;

public class InteractableSpot : InteractablePower
{
    private bool _isPowered;

    private Material material;

    private Transform spot;

    internal static readonly ClientInstanceMethod<bool> SendPowered = ClientInstanceMethod<bool>.Get(typeof(InteractableSpot), "ReceivePowered");

    private static readonly ServerInstanceMethod<bool> SendToggleRequest = ServerInstanceMethod<bool>.Get(typeof(InteractableSpot), "ReceiveToggleRequest");

    public bool isPowered => _isPowered;

    private void updateLights()
    {
        bool flag = base.isWired && isPowered;
        if (material != null)
        {
            material.SetColor("_EmissionColor", flag ? new Color(2f, 2f, 2f) : Color.black);
        }
        if (spot != null)
        {
            spot.gameObject.SetActive(flag);
        }
    }

    protected override void updateWired()
    {
        updateLights();
    }

    public void updatePowered(bool newPowered)
    {
        if (_isPowered != newPowered)
        {
            _isPowered = newPowered;
            updateLights();
        }
    }

    public override void updateState(Asset asset, byte[] state)
    {
        base.updateState(asset, state);
        _isPowered = state[0] == 1;
        RefreshIsConnectedToPowerWithoutNotify();
        updateLights();
    }

    public override void use()
    {
        ClientToggle();
    }

    public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
    {
        if (isPowered)
        {
            message = EPlayerMessage.SPOT_OFF;
        }
        else
        {
            message = EPlayerMessage.SPOT_ON;
        }
        text = "";
        color = Color.white;
        return true;
    }

    private void OnDestroy()
    {
        if (material != null)
        {
            Object.DestroyImmediate(material);
            material = null;
        }
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
                BarricadeManager.ServerSetSpotPoweredInternal(this, x, y, plant, region, !isPowered);
                EffectManager.TriggerFiremodeEffect(base.transform.position);
            }
        }
    }
}
