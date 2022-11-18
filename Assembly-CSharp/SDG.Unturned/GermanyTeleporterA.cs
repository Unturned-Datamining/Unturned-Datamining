using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class GermanyTeleporterA : MonoBehaviour
{
    protected static List<Player> nearbyPlayers = new List<Player>();

    public Transform target;

    public float sqrRadius;

    public string eventID;

    public ushort teleportEffect;

    private float lastTeleport;

    private bool isListening;

    protected virtual IEnumerator teleport()
    {
        yield return new WaitForSeconds(1f);
        if (!(target != null))
        {
            yield break;
        }
        nearbyPlayers.Clear();
        PlayerTool.getPlayersInRadius(base.transform.position, sqrRadius, nearbyPlayers);
        for (int i = 0; i < nearbyPlayers.Count; i++)
        {
            Player player = nearbyPlayers[i];
            if (!player.life.isDead)
            {
                if (player.quests.getQuestStatus(248) != ENPCQuestStatus.COMPLETED)
                {
                    player.quests.sendAddQuest(248);
                }
                player.teleportToLocationUnsafe(target.position, target.rotation.eulerAngles.y);
            }
        }
    }

    protected virtual void handleEventTriggered(Player player, string id)
    {
        if (!(id != eventID) && !(Time.realtimeSinceStartup - lastTeleport < 5f))
        {
            lastTeleport = Time.realtimeSinceStartup;
            EffectManager.sendEffect(teleportEffect, 16f, base.transform.position);
            StartCoroutine("teleport");
        }
    }

    protected virtual void OnEnable()
    {
        if (Provider.isServer && !isListening)
        {
            NPCEventManager.onEvent += handleEventTriggered;
            isListening = true;
        }
    }

    protected virtual void OnDisable()
    {
        if (isListening)
        {
            NPCEventManager.onEvent -= handleEventTriggered;
            isListening = false;
        }
    }
}
