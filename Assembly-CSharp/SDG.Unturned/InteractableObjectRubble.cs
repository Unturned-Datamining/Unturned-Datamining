using UnityEngine;

namespace SDG.Unturned;

public class InteractableObjectRubble : MonoBehaviour
{
    private RubbleInfo[] rubbleInfos;

    private GameObject aliveGameObject;

    private GameObject deadGameObject;

    private Transform finaleTransform;

    private Transform dropTransform;

    public ObjectAsset asset { get; protected set; }

    public byte getSectionCount()
    {
        return (byte)rubbleInfos.Length;
    }

    public Transform getSection(byte section)
    {
        return rubbleInfos[section].section;
    }

    public RubbleInfo getSectionInfo(byte section)
    {
        return rubbleInfos[section];
    }

    public bool isAllAlive()
    {
        for (byte b = 0; b < rubbleInfos.Length; b = (byte)(b + 1))
        {
            if (rubbleInfos[b].isDead)
            {
                return false;
            }
        }
        return true;
    }

    public bool isAllDead()
    {
        for (byte b = 0; b < rubbleInfos.Length; b = (byte)(b + 1))
        {
            if (!rubbleInfos[b].isDead)
            {
                return false;
            }
        }
        return true;
    }

    public bool isSectionDead(byte section)
    {
        return rubbleInfos[section].isDead;
    }

    public void askDamage(byte section, ushort amount)
    {
        if (section == byte.MaxValue)
        {
            for (section = 0; section < rubbleInfos.Length; section = (byte)(section + 1))
            {
                rubbleInfos[section].askDamage(amount);
            }
        }
        else
        {
            rubbleInfos[section].askDamage(amount);
        }
    }

    public byte checkCanReset(float multiplier)
    {
        for (byte b = 0; b < rubbleInfos.Length; b = (byte)(b + 1))
        {
            if (rubbleInfos[b].isDead && asset.rubbleReset > 1f && Time.realtimeSinceStartup - rubbleInfos[b].lastDead > asset.rubbleReset * multiplier)
            {
                return b;
            }
        }
        return byte.MaxValue;
    }

    public byte getSection(Transform section)
    {
        for (byte b = 0; b < rubbleInfos.Length; b = (byte)(b + 1))
        {
            RubbleInfo rubbleInfo = rubbleInfos[b];
            if (section == rubbleInfo.section || section.parent == rubbleInfo.section || section.parent.parent == rubbleInfo.section)
            {
                return b;
            }
        }
        return byte.MaxValue;
    }

    public void updateRubble(byte section, bool isAlive, bool playEffect, Vector3 ragdoll)
    {
        if (rubbleInfos == null || section >= rubbleInfos.Length)
        {
            return;
        }
        RubbleInfo rubbleInfo = rubbleInfos[section];
        if (isAlive)
        {
            rubbleInfo.health = asset.rubbleHealth;
        }
        else
        {
            rubbleInfo.lastDead = Time.realtimeSinceStartup;
            rubbleInfo.health = 0;
        }
        bool flag = isAllDead();
        if (rubbleInfo.aliveGameObject != null)
        {
            rubbleInfo.aliveGameObject.SetActive(!rubbleInfo.isDead);
        }
        if (rubbleInfo.deadGameObject != null)
        {
            rubbleInfo.deadGameObject.SetActive(rubbleInfo.isDead && (!flag || asset.IsRubbleFinaleEffectRefNull()));
        }
        if (aliveGameObject != null)
        {
            aliveGameObject.SetActive(!flag);
        }
        if (deadGameObject != null)
        {
            deadGameObject.SetActive(flag);
        }
        if (!(Provider.isServer && dropTransform != null && asset.rubbleRewardID != 0 && playEffect && flag) || (asset.holidayRestriction != 0 && !Provider.modeConfigData.Objects.Allow_Holiday_Drops) || !(Random.value <= asset.rubbleRewardProbability))
        {
            return;
        }
        int value = Random.Range(asset.rubbleRewardsMin, asset.rubbleRewardsMax + 1);
        value = Mathf.Clamp(value, 0, 100);
        for (int i = 0; i < value; i++)
        {
            ushort num = SpawnTableTool.resolve(asset.rubbleRewardID);
            if (num != 0)
            {
                ItemManager.dropItem(new Item(num, EItemOrigin.NATURE), dropTransform.position, playEffect: false, isDropped: true, wideSpread: false);
            }
        }
    }

    public void updateState(Asset asset, byte[] state)
    {
        this.asset = asset as ObjectAsset;
        Transform transform = base.transform.Find("Sections");
        if (transform != null)
        {
            rubbleInfos = new RubbleInfo[transform.childCount];
            for (int i = 0; i < rubbleInfos.Length; i++)
            {
                Transform section = transform.Find("Section_" + i);
                RubbleInfo rubbleInfo = new RubbleInfo();
                rubbleInfo.section = section;
                rubbleInfos[i] = rubbleInfo;
            }
            Transform transform2 = base.transform.Find("Alive");
            if (transform2 != null)
            {
                aliveGameObject = transform2.gameObject;
            }
            Transform transform3 = base.transform.Find("Dead");
            if (transform3 != null)
            {
                deadGameObject = transform3.gameObject;
            }
            finaleTransform = base.transform.Find("Finale");
        }
        else
        {
            rubbleInfos = new RubbleInfo[1];
            RubbleInfo rubbleInfo2 = new RubbleInfo();
            rubbleInfo2.section = base.transform;
            rubbleInfos[0] = rubbleInfo2;
        }
        dropTransform = base.transform.Find("Drop");
        for (byte b = 0; b < rubbleInfos.Length; b = (byte)(b + 1))
        {
            RubbleInfo rubbleInfo3 = rubbleInfos[b];
            Transform section2 = rubbleInfo3.section;
            Transform transform4 = section2.Find("Alive");
            if (transform4 != null)
            {
                rubbleInfo3.aliveGameObject = transform4.gameObject;
            }
            Transform transform5 = section2.Find("Dead");
            if (transform5 != null)
            {
                rubbleInfo3.deadGameObject = transform5.gameObject;
            }
            Transform transform6 = section2.Find("Ragdolls");
            if (transform6 != null)
            {
                rubbleInfo3.ragdolls = new RubbleRagdollInfo[transform6.childCount];
                for (int j = 0; j < rubbleInfo3.ragdolls.Length; j++)
                {
                    Transform transform7 = transform6.Find("Ragdoll_" + j);
                    Transform transform8 = transform7.Find("Ragdoll");
                    if (transform8 != null)
                    {
                        rubbleInfo3.ragdolls[j] = new RubbleRagdollInfo();
                        rubbleInfo3.ragdolls[j].ragdollGameObject = transform8.gameObject;
                        rubbleInfo3.ragdolls[j].forceTransform = transform7.Find("Force");
                    }
                }
            }
            else
            {
                Transform transform9 = section2.Find("Ragdoll");
                if (transform9 != null)
                {
                    rubbleInfo3.ragdolls = new RubbleRagdollInfo[1];
                    rubbleInfo3.ragdolls[0] = new RubbleRagdollInfo();
                    rubbleInfo3.ragdolls[0].ragdollGameObject = transform9.gameObject;
                    rubbleInfo3.ragdolls[0].forceTransform = section2.Find("Force");
                }
            }
            rubbleInfo3.effectTransform = section2.Find("Effect");
        }
        for (byte b2 = 0; b2 < rubbleInfos.Length; b2 = (byte)(b2 + 1))
        {
            bool isAlive = (state[state.Length - 1] & Types.SHIFTS[b2]) == Types.SHIFTS[b2];
            updateRubble(b2, isAlive, playEffect: false, Vector3.zero);
        }
    }
}
