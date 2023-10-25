using UnityEngine;

/// <summary>
/// Hacky workaround to fix item skin material leak. Unfortunately none of the original item skin code destroyed
/// instantiated materials, and did not keep a reference to the instantiated materials, so until that code gets a
/// rewrite this will take care of cleanup.
/// </summary>
public class DestroyMaterialOnDestroy : MonoBehaviour
{
    public Material instantiatedMaterial;

    private void OnDestroy()
    {
        Object.Destroy(instantiatedMaterial);
    }
}
