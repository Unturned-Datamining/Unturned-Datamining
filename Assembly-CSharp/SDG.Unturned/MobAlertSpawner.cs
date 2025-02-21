using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Allows Unity events to startle nearby animals and zombies.
/// </summary>
[AddComponentMenu("Unturned/Mob Alert Spawner")]
public class MobAlertSpawner : MonoBehaviour
{
    public float AlertRadius;

    /// <summary>
    /// Transform to spawn the alert at.
    /// If unset, this game object's transform will be used instead.
    /// </summary>
    public Transform AlertOriginTransformOverride;

    /// <summary>
    /// If true, find the nearest player within ScanForPlayersRadius, and use the player's position as the alert
    /// origin.
    /// </summary>
    public bool UseScanForPlayers;

    public float ScanForPlayersRadius;

    /// <summary>
    /// If UseScanForPlayers is enabled and this is true, an alert is broadcast even if no nearby player was found.
    /// Otherwise, alert is ignored if no nearby player was found.
    /// </summary>
    public bool ScanForPlayersUseTransformAsFallback;

    public void Alert()
    {
        Vector3 vector = ((!(AlertOriginTransformOverride != null)) ? base.transform.position : AlertOriginTransformOverride.position);
        if (UseScanForPlayers)
        {
            Player nearestPlayerInRadius = PlayerTool.GetNearestPlayerInRadius(vector, ScanForPlayersRadius * ScanForPlayersRadius);
            if (nearestPlayerInRadius != null)
            {
                vector = nearestPlayerInRadius.transform.position;
            }
            else if (!ScanForPlayersUseTransformAsFallback)
            {
                return;
            }
        }
        AlertTool.alert(vector, AlertRadius);
    }
}
