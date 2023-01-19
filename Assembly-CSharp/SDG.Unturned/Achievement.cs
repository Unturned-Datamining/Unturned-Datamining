using UnityEngine;

namespace SDG.Unturned;

public class Achievement : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!Dedicator.IsDedicatedServer && other.transform.CompareTag("Player") && !(other.transform != Player.player.transform) && Provider.provider.achievementsService.getAchievement(base.transform.name, out var has) && !has)
        {
            Provider.provider.achievementsService.setAchievement(base.transform.name);
        }
    }
}
