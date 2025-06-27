using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Visualizes reverb zone in-game.
/// </summary>
internal class ReverbGizmoComponent : MonoBehaviour
{
    public AudioReverbZone zone;

    protected void Update()
    {
        if (zone == null || !base.gameObject.activeInHierarchy)
        {
            Object.Destroy(this);
            return;
        }
        Player localPlayer = Player.LocalPlayer;
        if (localPlayer == null || !localPlayer.channel.owner.isAdmin)
        {
            Object.Destroy(this);
            return;
        }
        RuntimeGizmos runtimeGizmos = RuntimeGizmos.Get();
        Color color = new Color(1f, 0.5f, 0f);
        Matrix4x4 localToWorldMatrix = zone.transform.localToWorldMatrix;
        float minDistance = zone.minDistance;
        float maxDistance = zone.maxDistance;
        runtimeGizmos.Sphere(localToWorldMatrix, minDistance, color);
        runtimeGizmos.Sphere(localToWorldMatrix, maxDistance, color);
        runtimeGizmos.Line(localToWorldMatrix.MultiplyPoint3x4(new Vector3(minDistance, 0f, 0f)), localToWorldMatrix.MultiplyPoint3x4(new Vector3(maxDistance, 0f, 0f)), color);
        runtimeGizmos.Line(localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f - minDistance, 0f, 0f)), localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f - maxDistance, 0f, 0f)), color);
        runtimeGizmos.Line(localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f, minDistance, 0f)), localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f, maxDistance, 0f)), color);
        runtimeGizmos.Line(localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f, 0f - minDistance, 0f)), localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f, 0f - maxDistance, 0f)), color);
        runtimeGizmos.Line(localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f, 0f, minDistance)), localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f, 0f, maxDistance)), color);
        runtimeGizmos.Line(localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f, 0f, 0f - minDistance)), localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f, 0f, 0f - maxDistance)), color);
    }

    protected void OnEnable()
    {
        zone = GetComponent<AudioReverbZone>();
    }
}
