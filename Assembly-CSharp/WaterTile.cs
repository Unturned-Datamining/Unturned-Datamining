using UnityEngine;

[ExecuteInEditMode]
public class WaterTile : MonoBehaviour
{
    public PlanarReflection reflection;

    public WaterBase waterBase;

    public void Start()
    {
        AcquireComponents();
    }

    private void AcquireComponents()
    {
        if (!reflection)
        {
            if ((bool)base.transform.parent)
            {
                reflection = base.transform.parent.GetComponent<PlanarReflection>();
            }
            else
            {
                reflection = base.transform.GetComponent<PlanarReflection>();
            }
        }
    }

    public void OnWillRenderObject()
    {
        Camera current = Camera.current;
        if ((bool)reflection)
        {
            reflection.WaterTileBeingRendered(base.transform, current);
        }
        if ((bool)waterBase)
        {
            waterBase.WaterTileBeingRendered(base.transform, current);
        }
    }
}
