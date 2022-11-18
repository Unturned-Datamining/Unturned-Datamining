using UnityEngine;

namespace SDG.Unturned;

public class InteractableClock : InteractablePower
{
    private Transform handHourTransform;

    private Transform handMinuteTransform;

    public override void updateState(Asset asset, byte[] state)
    {
        handHourTransform = base.transform.Find("Hand_Hour");
        handMinuteTransform = base.transform.Find("Hand_Minute");
        base.updateState(asset, state);
        RefreshIsConnectedToPowerWithoutNotify();
    }

    public override bool checkUseable()
    {
        return base.isWired;
    }

    public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
    {
        text = "";
        color = Color.white;
        if (!base.isWired)
        {
            message = EPlayerMessage.POWER;
            return true;
        }
        message = EPlayerMessage.NONE;
        return false;
    }

    private void Update()
    {
        if (base.isWired && !(handHourTransform == null) && !(handMinuteTransform == null))
        {
            float num = ((!(LightingManager.day < LevelLighting.bias)) ? ((LightingManager.day - LevelLighting.bias) / (1f - LevelLighting.bias)) : (LightingManager.day / LevelLighting.bias));
            float num2 = num - 0.5f;
            float num3 = num * 12f;
            handHourTransform.localRotation = Quaternion.Euler(0f, num2 * -360f, 0f);
            handMinuteTransform.localRotation = Quaternion.Euler(0f, num3 * -360f, 0f);
        }
    }
}
