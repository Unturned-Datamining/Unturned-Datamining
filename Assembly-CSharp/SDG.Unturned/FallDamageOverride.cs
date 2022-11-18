using UnityEngine;

namespace SDG.Unturned;

[AddComponentMenu("Unturned/Fall Damage Override")]
public class FallDamageOverride : MonoBehaviour
{
    public enum EMode
    {
        None,
        PreventFallDamage
    }

    public EMode Mode = EMode.PreventFallDamage;
}
