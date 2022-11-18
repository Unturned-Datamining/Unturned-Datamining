using UnityEngine;

namespace SDG.Unturned;

public delegate void DamageToolZombieDamagedHandler(Zombie zombie, ref Vector3 direction, ref float damage, ref float times, ref bool canDamage);
