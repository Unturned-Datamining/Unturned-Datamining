using System.Collections;
using UnityEngine;

namespace SDG.Unturned;

public class VolumeTeleporter : MonoBehaviour
{
    public string achievement;

    public Transform target;

    public ushort teleportEffect;

    public Transform effectHook;

    private Player playerTeleported;

    private IEnumerator teleport()
    {
        yield return new WaitForSeconds(3f);
        if (target != null && playerTeleported != null && playerTeleported.life.IsAlive)
        {
            playerTeleported.teleportToLocation(target.position, target.rotation.eulerAngles.y);
            if (playerTeleported.equipment.HasValidUseable)
            {
                playerTeleported.equipment.dequip();
            }
            playerTeleported.equipment.canEquip = true;
        }
        playerTeleported = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!Dedicator.IsDedicatedServer && !string.IsNullOrEmpty(achievement) && other.transform.CompareTag("Player") && other.transform == Player.player.transform && Provider.provider.achievementsService.getAchievement(achievement, out var has) && !has)
        {
            Provider.provider.achievementsService.setAchievement(achievement);
        }
        if (Provider.isServer && other.transform.CompareTag("Player") && playerTeleported == null)
        {
            EffectManager.sendEffect(teleportEffect, 16f, effectHook.position);
            playerTeleported = DamageTool.getPlayer(other.transform);
            StartCoroutine("teleport");
        }
    }
}
