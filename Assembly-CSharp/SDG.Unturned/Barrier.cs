using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class Barrier : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!Provider.isServer)
        {
            return;
        }
        if (other.transform.CompareTag("Player"))
        {
            Player player = DamageTool.getPlayer(other.transform);
            if (player != null)
            {
                player.life.askDamage(101, Vector3.up * 10f, EDeathCause.SUICIDE, ELimb.SKULL, CSteamID.Nil, out var _);
            }
        }
        else
        {
            if (!other.CompareTag("Agent"))
            {
                return;
            }
            Zombie zombie = DamageTool.getZombie(other.transform);
            if (zombie != null)
            {
                DamageZombieParameters parameters = DamageZombieParameters.makeInstakill(zombie);
                parameters.instigator = this;
                DamageTool.damageZombie(parameters, out var _, out var _);
                return;
            }
            Animal animal = DamageTool.getAnimal(other.transform);
            if (animal != null)
            {
                DamageAnimalParameters parameters2 = DamageAnimalParameters.makeInstakill(animal);
                parameters2.instigator = this;
                DamageTool.damageAnimal(parameters2, out var _, out var _);
            }
        }
    }
}
