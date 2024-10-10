using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Manages lifetime and attachment of a mythical effect. Added by <see cref="M:SDG.Unturned.ItemTool.ApplyMythicalEffect(UnityEngine.Transform,System.UInt16,SDG.Unturned.EEffectType)" />.
/// Was called `MythicLocker` with a paired `MythicLockee` prior to 2024-06-11.
/// </summary>
public class MythicalEffectController : MonoBehaviour
{
    public GameObject systemPrefab;

    public Transform systemTransform;

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
            MaybeInstantiateOrDestroySystem();
        }
    }

    private void Update()
    {
        if (systemTransform != null)
        {
            base.transform.GetPositionAndRotation(out var position, out var rotation);
            systemTransform.SetPositionAndRotation(position, rotation);
        }
    }

    private void LateUpdate()
    {
        if (systemTransform != null)
        {
            base.transform.GetPositionAndRotation(out var position, out var rotation);
            systemTransform.SetPositionAndRotation(position, rotation);
        }
    }

    private void OnEnable()
    {
        MaybeInstantiateOrDestroySystem();
    }

    private void OnDisable()
    {
        if (systemTransform != null)
        {
            Object.Destroy(systemTransform.gameObject);
            systemTransform = null;
        }
    }

    private void OnDestroy()
    {
        if (systemTransform != null)
        {
            Object.Destroy(systemTransform.gameObject);
            systemTransform = null;
        }
    }

    private void Start()
    {
        MaybeInstantiateOrDestroySystem();
    }

    private void MaybeInstantiateOrDestroySystem()
    {
        if (_isMythicalEffectEnabled && base.gameObject.activeInHierarchy)
        {
            if (systemTransform == null && systemPrefab != null)
            {
                base.transform.GetPositionAndRotation(out var position, out var rotation);
                systemTransform = Object.Instantiate(systemPrefab, position, rotation).transform;
                systemTransform.name = "System";
                systemTransform.localScale = base.transform.localScale;
            }
        }
        else if (systemTransform != null)
        {
            Object.Destroy(systemTransform.gameObject);
            systemTransform = null;
        }
    }
}
