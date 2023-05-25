using UnityEngine;

namespace SDG.Unturned;

public class MythicLocker : MonoBehaviour
{
    public MythicLockee system;

    private bool _isMythicalEffectEnabled = true;

    public bool IsMythicalEffectEnabled
    {
        get
        {
            return _isMythicalEffectEnabled;
        }
        set
        {
            _isMythicalEffectEnabled = value;
            if (base.gameObject.activeInHierarchy)
            {
                system.gameObject.SetActive(IsMythicalEffectEnabled);
            }
        }
    }

    private void Update()
    {
        if (!(system == null))
        {
            system.transform.position = base.transform.position;
            system.transform.rotation = base.transform.rotation;
        }
    }

    private void LateUpdate()
    {
        if (!(system == null))
        {
            system.transform.position = base.transform.position;
            system.transform.rotation = base.transform.rotation;
        }
    }

    private void OnEnable()
    {
        if (!(system == null))
        {
            system.gameObject.SetActive(IsMythicalEffectEnabled);
        }
    }

    private void OnDisable()
    {
        if (!(system == null))
        {
            system.gameObject.SetActive(value: false);
        }
    }

    private void Start()
    {
        if (!(system == null))
        {
            system.transform.parent = null;
        }
    }

    private void OnDestroy()
    {
        if (!(system == null))
        {
            Object.Destroy(system.gameObject);
        }
    }
}
