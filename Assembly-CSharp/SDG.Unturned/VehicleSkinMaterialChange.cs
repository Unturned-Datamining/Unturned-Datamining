using UnityEngine;

namespace SDG.Unturned;

internal struct VehicleSkinMaterialChange
{
    public Renderer renderer;

    public Material originalMaterial;

    /// <summary>
    /// If true, set sharedMaterial. If false, set material.
    /// </summary>
    public bool shared;
}
