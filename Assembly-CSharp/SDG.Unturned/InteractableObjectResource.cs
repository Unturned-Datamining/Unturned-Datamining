using System;
using UnityEngine;

namespace SDG.Unturned;

public class InteractableObjectResource : InteractableObject
{
    private ushort _amount;

    private ushort _capacity;

    private bool isListeningForRain;

    private float lastUsed = -9999f;

    public ushort amount => _amount;

    public ushort capacity => _capacity;

    public bool isRefillable => amount < capacity;

    public bool isSiphonable => amount > 0;

    public bool checkCanReset(float multiplier)
    {
        if (amount == capacity)
        {
            return false;
        }
        if (base.objectAsset.interactabilityReset < 1f)
        {
            return false;
        }
        if (base.objectAsset.interactability == EObjectInteractability.WATER)
        {
            return Time.realtimeSinceStartup - lastUsed > base.objectAsset.interactabilityReset * multiplier;
        }
        if (base.objectAsset.interactability == EObjectInteractability.FUEL)
        {
            return Time.realtimeSinceStartup - lastUsed > base.objectAsset.interactabilityReset * multiplier;
        }
        return false;
    }

    public void updateAmount(ushort newAmount)
    {
        _amount = newAmount;
        lastUsed = Time.realtimeSinceStartup;
    }

    public override void updateState(Asset asset, byte[] state)
    {
        base.updateState(asset, state);
        _amount = BitConverter.ToUInt16(state, 0);
        _capacity = ((ObjectAsset)asset).interactabilityResource;
        lastUsed = Time.realtimeSinceStartup;
        if (base.objectAsset.interactability == EObjectInteractability.WATER && !isListeningForRain)
        {
            isListeningForRain = true;
            LightingManager.onRainUpdated = (RainUpdated)Delegate.Combine(LightingManager.onRainUpdated, new RainUpdated(onRainUpdated));
        }
    }

    public override bool checkUseable()
    {
        return amount > 0;
    }

    public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
    {
        if (base.objectAsset.interactability == EObjectInteractability.WATER)
        {
            message = EPlayerMessage.VOLUME_WATER;
            text = amount + "/" + capacity;
        }
        else
        {
            message = EPlayerMessage.VOLUME_FUEL;
            text = "";
        }
        color = Color.white;
        return true;
    }

    private void onRainUpdated(ELightingRain rain)
    {
        if (rain == ELightingRain.POST_DRIZZLE)
        {
            _amount = capacity;
            if (Provider.isServer)
            {
                ObjectManager.updateObjectResource(base.transform, amount, shouldSend: false);
            }
        }
    }

    private void OnDestroy()
    {
        if (base.objectAsset.interactability == EObjectInteractability.WATER && isListeningForRain)
        {
            isListeningForRain = false;
            LightingManager.onRainUpdated = (RainUpdated)Delegate.Remove(LightingManager.onRainUpdated, new RainUpdated(onRainUpdated));
        }
    }
}
