using UnityEngine;

public class ItemLook : MonoBehaviour
{
    public Camera inspectCamera;

    public float _yaw;

    public float yaw;

    public GameObject target;

    private void Update()
    {
        if (target == null)
        {
            return;
        }
        Bounds bounds = default(Bounds);
        bool flag = false;
        Collider[] componentsInChildren = target.GetComponentsInChildren<Collider>();
        for (int i = 0; i < componentsInChildren.Length; i++)
        {
            Bounds bounds2 = componentsInChildren[i].bounds;
            if (flag)
            {
                bounds.Encapsulate(bounds2);
                continue;
            }
            bounds = bounds2;
            flag = true;
        }
        Vector3 center = bounds.center;
        float num = bounds.extents.magnitude * 2.25f;
        _yaw = Mathf.Lerp(_yaw, yaw, 4f * Time.deltaTime);
        inspectCamera.transform.rotation = Quaternion.Euler(20f, _yaw, 0f);
        inspectCamera.transform.position = center - inspectCamera.transform.forward * num;
    }
}
