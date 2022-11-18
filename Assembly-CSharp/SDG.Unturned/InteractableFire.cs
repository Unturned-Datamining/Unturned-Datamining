using SDG.NetTransport;
using UnityEngine;

namespace SDG.Unturned;

public class InteractableFire : Interactable
{
    private bool _isLit;

    private Material material;

    private Transform fire;

    internal static readonly ClientInstanceMethod<bool> SendLit = ClientInstanceMethod<bool>.Get(typeof(InteractableFire), "ReceiveLit");

    private static readonly ServerInstanceMethod<bool> SendToggleRequest = ServerInstanceMethod<bool>.Get(typeof(InteractableFire), "ReceiveToggleRequest");

    public bool isLit => _isLit;

    private void updateFire()
    {
        if (material != null)
        {
            material.SetColor("_EmissionColor", isLit ? new Color(2f, 2f, 2f) : Color.black);
        }
        if (fire != null)
        {
            fire.gameObject.SetActive(isLit);
        }
    }

    public void updateLit(bool newLit)
    {
        _isLit = newLit;
        updateFire();
    }

    public override void updateState(Asset asset, byte[] state)
    {
        _isLit = state[0] == 1;
        if (material == null)
        {
            material = HighlighterTool.getMaterialInstance(base.transform);
        }
        if (fire == null)
        {
            fire = base.transform.Find("Fire");
            LightLODTool.applyLightLOD(fire);
        }
        updateFire();
    }

    public override void use()
    {
        ClientToggle();
    }

    public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
    {
        if (isLit)
        {
            message = EPlayerMessage.FIRE_OFF;
        }
        else
        {
            message = EPlayerMessage.FIRE_ON;
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
    public void ReceiveLit(bool newLit)
    {
        updateLit(newLit);
    }

    public void ClientToggle()
    {
        SendToggleRequest.Invoke(GetNetId(), ENetReliability.Unreliable, !isLit);
    }

    [SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 2)]
    public void ReceiveToggleRequest(in ServerInvocationContext context, bool desiredLit)
    {
        if (isLit != desiredLit && BarricadeManager.tryGetRegion(base.transform, out var x, out var y, out var plant, out var region))
        {
            Player player = context.GetPlayer();
            if (!(player == null) && !player.life.isDead && !((base.transform.position - player.transform.position).sqrMagnitude > 400f))
            {
                BarricadeManager.ServerSetFireLitInternal(this, x, y, plant, region, !isLit);
            }
        }
    }
}
