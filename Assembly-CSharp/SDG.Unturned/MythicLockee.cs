using UnityEngine;

namespace SDG.Unturned;

public class MythicLockee : MonoBehaviour
{
    public MythicLocker locker;

    public bool isMythic
    {
        get
        {
            return locker.isMythic;
        }
        set
        {
            locker.isMythic = value;
        }
    }
}
