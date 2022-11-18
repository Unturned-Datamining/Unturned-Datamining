using System;
using UnityEngine;

namespace SDG.Unturned;

public class InteractableRainBarrel : Interactable
{
    private bool _isFull;

    internal static readonly ClientInstanceMethod<bool> SendFull = ClientInstanceMethod<bool>.Get(typeof(InteractableRainBarrel), "ReceiveFull");

    public bool isFull => _isFull;

    public void updateFull(bool newFull)
    {
        _isFull = newFull;
    }

    public override void updateState(Asset asset, byte[] state)
    {
        _isFull = state[0] == 1;
    }

    public override bool checkUseable()
    {
        return isFull;
    }

    public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
    {
        message = EPlayerMessage.VOLUME_WATER;
        text = "";
        color = Color.white;
        return true;
    }

    private void onRainUpdated(ELightingRain rain)
    {
        if (rain == ELightingRain.POST_DRIZZLE && !Physics.Raycast(base.transform.position + Vector3.up, Vector3.up, 32f, RayMasks.BLOCK_WIND))
        {
            _isFull = true;
            if (Provider.isServer)
            {
                BarricadeManager.updateRainBarrel(base.transform, isFull, shouldSend: false);
            }
        }
    }

    private void OnEnable()
    {
        LightingManager.onRainUpdated = (RainUpdated)Delegate.Combine(LightingManager.onRainUpdated, new RainUpdated(onRainUpdated));
    }

    private void OnDisable()
    {
        LightingManager.onRainUpdated = (RainUpdated)Delegate.Remove(LightingManager.onRainUpdated, new RainUpdated(onRainUpdated));
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
    public void ReceiveFull(bool newFull)
    {
        updateFull(newFull);
    }
}
