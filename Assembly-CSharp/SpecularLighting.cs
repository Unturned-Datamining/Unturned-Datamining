using UnityEngine;

[RequireComponent(typeof(WaterBase))]
[ExecuteInEditMode]
public class SpecularLighting : MonoBehaviour
{
    public Transform specularLight;

    private WaterBase waterBase;

    public void Start()
    {
        waterBase = (WaterBase)base.gameObject.GetComponent(typeof(WaterBase));
    }

    public void Update()
    {
        if (!waterBase)
        {
            waterBase = (WaterBase)base.gameObject.GetComponent(typeof(WaterBase));
        }
        if ((bool)specularLight && (bool)waterBase.sharedMaterial)
        {
            waterBase.sharedMaterial.SetVector("_WorldLightDir", specularLight.transform.forward);
        }
    }
}
