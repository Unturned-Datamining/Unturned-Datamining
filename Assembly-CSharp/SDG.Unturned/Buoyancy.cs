using System.Collections.Generic;
using SDG.Framework.Water;
using UnityEngine;

namespace SDG.Unturned;

public class Buoyancy : MonoBehaviour
{
    private static readonly float DAMPER = 0.1f;

    private static readonly float WATER_DENSITY = 1000f;

    public float density = 500f;

    public int slicesPerAxis = 2;

    private float voxelHalfHeight;

    private Vector3 localArchimedesForce;

    private List<Vector3> voxels;

    private Rigidbody rootRigidbody;

    private Collider volumeCollider;

    public float overrideSurfaceElevation = -1f;

    private void FixedUpdate()
    {
        for (int i = 0; i < voxels.Count; i++)
        {
            Vector3 vector = base.transform.TransformPoint(voxels[i]);
            bool isUnderwater;
            float surfaceElevation;
            if (overrideSurfaceElevation < 0f)
            {
                WaterUtility.getUnderwaterInfo(vector, out isUnderwater, out surfaceElevation);
            }
            else
            {
                isUnderwater = vector.y < overrideSurfaceElevation;
                surfaceElevation = overrideSurfaceElevation;
            }
            if (isUnderwater && vector.y - voxelHalfHeight < surfaceElevation)
            {
                Vector3 force = -rootRigidbody.GetPointVelocity(vector) * DAMPER * rootRigidbody.mass + Mathf.Sqrt(Mathf.Clamp01((surfaceElevation - vector.y) / (2f * voxelHalfHeight) + 0.5f)) * localArchimedesForce;
                rootRigidbody.AddForceAtPosition(force, vector);
            }
        }
    }

    private void Start()
    {
        rootRigidbody = base.gameObject.GetComponentInParent<Rigidbody>();
        volumeCollider = GetComponent<Collider>();
        BoxCollider boxCollider = volumeCollider as BoxCollider;
        if (!boxCollider)
        {
            UnturnedLog.warn("Unknown volume collider for buoyancy simulation: {0}", volumeCollider);
            return;
        }
        Vector3 size = boxCollider.size;
        Vector3 vector = size / -2f;
        Vector3 vector2 = size / slicesPerAxis;
        voxelHalfHeight = Mathf.Min(vector2.x, Mathf.Min(vector2.y, vector2.z)) / 2f;
        voxels = new List<Vector3>(slicesPerAxis * slicesPerAxis * slicesPerAxis);
        for (int i = 0; i < slicesPerAxis; i++)
        {
            for (int j = 0; j < slicesPerAxis; j++)
            {
                for (int k = 0; k < slicesPerAxis; k++)
                {
                    float x = vector.x + vector2.x * (0.5f + (float)i);
                    float y = vector.y + vector2.y * (0.5f + (float)j);
                    float z = vector.z + vector2.z * (0.5f + (float)k);
                    Vector3 item = new Vector3(x, y, z);
                    voxels.Add(item);
                }
            }
        }
        if (voxels.Count == 0)
        {
            voxels.Add(boxCollider.center);
        }
        float num = rootRigidbody.mass / density;
        float y2 = WATER_DENSITY * Mathf.Abs(Physics.gravity.y) * num;
        localArchimedesForce = new Vector3(0f, y2, 0f) / voxels.Count;
    }
}
