using System.Collections.Generic;
using UnityEngine;

public class ItemLook : MonoBehaviour
{
    public Camera inspectCamera;

    public float _yaw;

    public float yaw;

    public GameObject target;

    private List<Renderer> renderers = new List<Renderer>();

    private void Update()
    {
        if (target == null)
        {
            return;
        }
        renderers.Clear();
        target.GetComponentsInChildren(includeInactive: false, renderers);
        Bounds bounds = default(Bounds);
        bool flag = false;
        foreach (Renderer renderer in renderers)
        {
            if (renderer is MeshRenderer || renderer is SkinnedMeshRenderer)
            {
                if (flag)
                {
                    bounds.Encapsulate(renderer.bounds);
                    continue;
                }
                flag = true;
                bounds = renderer.bounds;
            }
        }
        if (flag)
        {
            Vector3 center = bounds.center;
            float num = bounds.extents.magnitude * 2.25f;
            _yaw = Mathf.Lerp(_yaw, yaw, 4f * Time.deltaTime);
            inspectCamera.transform.rotation = Quaternion.Euler(20f, 0f - _yaw, 0f);
            inspectCamera.transform.position = center - inspectCamera.transform.forward * num;
        }
    }
}
