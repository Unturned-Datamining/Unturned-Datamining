using System;
using UnityEngine;

namespace SDG.Unturned;

public class Carepackage : MonoBehaviour
{
    /// <summary>
    /// Item ID of barricade to spawn after landing.
    /// </summary>
    [Obsolete]
    public ushort barricadeID = 1374;

    /// <summary>
    /// Barricade to spawn after landing.
    /// </summary>
    public ItemBarricadeAsset barricadeAsset;

    public ushort id;

    public string landedEffectGuid = "2c17fbd0f0ce49aeb3bc4637b68809a2";

    private bool isExploded;

    /// <summary>
    /// Kill any players inside the spawned interactable box.
    /// Uses hardcoded size of 4 x 4 x 4.
    /// </summary>
    private void squishPlayersUnderBox(Transform barricade)
    {
        foreach (SteamPlayer client in Provider.clients)
        {
            if (client != null && !(client.player == null) && !(client.player.life == null))
            {
                Vector3 vector = barricade.InverseTransformPoint(client.model.position);
                if (Mathf.Abs(vector.x) < 2f && Mathf.Abs(vector.y) < 2f && Mathf.Abs(vector.z) < 2f)
                {
                    DamagePlayerParameters parameters = new DamagePlayerParameters(client.player);
                    parameters.damage = 101f;
                    parameters.applyGlobalArmorMultiplier = false;
                    DamageTool.damagePlayer(parameters, out var _);
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isExploded || collision.collider.isTrigger)
        {
            return;
        }
        isExploded = true;
        if (Provider.isServer)
        {
            Vector3 position = base.transform.position;
            ItemBarricadeAsset itemBarricadeAsset = barricadeAsset;
            if (itemBarricadeAsset == null)
            {
                itemBarricadeAsset = Assets.find(EAssetType.ITEM, barricadeID) as ItemBarricadeAsset;
            }
            Transform transform = BarricadeManager.dropBarricade(new Barricade(itemBarricadeAsset), null, base.transform.position, 0f, 0f, 0f, 0uL, 0uL);
            if (transform != null)
            {
                squishPlayersUnderBox(transform);
                InteractableStorage component = transform.GetComponent<InteractableStorage>();
                component.despawnWhenDestroyed = true;
                if (component != null && component.items != null)
                {
                    int num = 0;
                    while (num < 8)
                    {
                        ushort num2 = SpawnTableTool.ResolveLegacyId(id, EAssetType.ITEM, OnGetSpawnTableErrorContext);
                        if (num2 == 0)
                        {
                            break;
                        }
                        if (!component.items.tryAddItem(new Item(num2, EItemOrigin.ADMIN), isStateUpdatable: false))
                        {
                            num++;
                        }
                    }
                    component.items.onStateUpdated();
                }
                transform.gameObject.AddComponent<CarepackageDestroy>();
                Transform transform2 = transform.Find("Flare");
                if (transform2 != null)
                {
                    position = transform2.position;
                }
            }
            EffectAsset effectAsset = Assets.find(new AssetReference<EffectAsset>(landedEffectGuid));
            if (effectAsset != null)
            {
                TriggerEffectParameters parameters = new TriggerEffectParameters(effectAsset);
                parameters.position = position;
                parameters.reliable = true;
                parameters.relevantDistance = EffectManager.INSANE;
                EffectManager.triggerEffect(parameters);
            }
        }
        UnityEngine.Object.Destroy(base.gameObject);
    }

    private string OnGetSpawnTableErrorContext()
    {
        return "airdrop care package";
    }
}
