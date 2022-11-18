using UnityEngine;

namespace SDG.Unturned;

[AddComponentMenu("Unturned/LOD Group Additional Data")]
[RequireComponent(typeof(LODGroup))]
public class LODGroupAdditionalData : MonoBehaviour
{
    public enum ELODBiasOverride
    {
        None,
        IgnoreLODBias
    }

    public ELODBiasOverride LODBiasOverride = ELODBiasOverride.IgnoreLODBias;

    private void Start()
    {
        LODGroupManager.Get().Register(this);
    }

    private void OnDestroy()
    {
        LODGroupManager.Get().Unregister(this);
    }
}
