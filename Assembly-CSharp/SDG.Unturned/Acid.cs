using System;
using UnityEngine;

namespace SDG.Unturned;

public class Acid : MonoBehaviour
{
    private bool isExploded;

    private Vector3 lastPos;

    public Guid effectGuid;

    /// <summary>
    /// Kept because lots of modders have been using this script in Unity,
    /// so removing legacy effect id would break their content.
    /// </summary>
    public ushort effectID;

    private void OnTriggerEnter(Collider other)
    {
        if (isExploded || other.isTrigger || other.transform.CompareTag("Agent"))
        {
            return;
        }
        isExploded = true;
        if (Provider.isServer)
        {
            EffectAsset effectAsset = Assets.FindEffectAssetByGuidOrLegacyId(effectGuid, effectID);
            if (effectAsset != null)
            {
                TriggerEffectParameters parameters = new TriggerEffectParameters(effectAsset);
                parameters.position = lastPos;
                parameters.relevantDistance = EffectManager.LARGE;
                EffectManager.triggerEffect(parameters);
            }
        }
        UnityEngine.Object.Destroy(base.transform.parent.gameObject);
    }

    private void FixedUpdate()
    {
        lastPos = base.transform.position;
    }

    private void Awake()
    {
        lastPos = base.transform.position;
    }
}
