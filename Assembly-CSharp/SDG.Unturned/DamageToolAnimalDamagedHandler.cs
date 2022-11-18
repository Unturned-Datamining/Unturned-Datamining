using UnityEngine;

namespace SDG.Unturned;

public delegate void DamageToolAnimalDamagedHandler(Animal animal, ref Vector3 direction, ref float damage, ref float times, ref bool canDamage);
