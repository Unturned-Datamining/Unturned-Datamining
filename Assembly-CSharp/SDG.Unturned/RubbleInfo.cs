using UnityEngine;

namespace SDG.Unturned;

public class RubbleInfo
{
    public float lastDead;

    public ushort health;

    public Transform section;

    public GameObject aliveGameObject;

    public GameObject deadGameObject;

    public RubbleRagdollInfo[] ragdolls;

    public Transform effectTransform;

    public bool isDead => health == 0;

    public void askDamage(ushort amount)
    {
        if (amount != 0 && !isDead)
        {
            if (amount >= health)
            {
                health = 0;
            }
            else
            {
                health -= amount;
            }
        }
    }
}
