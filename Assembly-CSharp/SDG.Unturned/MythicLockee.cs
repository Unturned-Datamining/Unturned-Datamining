using UnityEngine;

namespace SDG.Unturned;

public class MythicLockee : MonoBehaviour
{
    public MythicLocker locker;

    public bool IsMythicalEffectEnabled
    {
        get
        {
            return locker.IsMythicalEffectEnabled;
        }
        set
        {
            locker.IsMythicalEffectEnabled = value;
        }
    }
}
