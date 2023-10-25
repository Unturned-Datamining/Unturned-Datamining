using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Optional Unturned extensions to the LOD Group component.
/// </summary>
[AddComponentMenu("Unturned/LOD Group Additional Data")]
[RequireComponent(typeof(LODGroup))]
public class LODGroupAdditionalData : MonoBehaviour
{
    /// <summary>
    /// Could be extended, e.g. to clamp cull size separately from the per-LOD sizes.
    /// </summary>
    public enum ELODBiasOverride
    {
        None,
        /// <summary>
        /// Unturned will adjust per-LOD sizes to counteract LOD bias.
        /// Elver has carefully tuned LOD sizes for the interior of the mall, so LOD bias affecting them is undesirable.
        /// Note that due to a Unity bug only LOD0 can be greater than 100%.
        /// </summary>
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
