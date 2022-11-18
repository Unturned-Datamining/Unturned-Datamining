using UnityEngine;

public class DestroyMaterialOnDestroy : MonoBehaviour
{
    public Material instantiatedMaterial;

    private void OnDestroy()
    {
        Object.Destroy(instantiatedMaterial);
    }
}
