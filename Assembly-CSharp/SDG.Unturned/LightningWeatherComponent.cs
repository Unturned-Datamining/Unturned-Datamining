using System.Collections;
using System.Collections.Generic;
using SDG.NetTransport;
using UnityEngine;

namespace SDG.Unturned;

public class LightningWeatherComponent : MonoBehaviour
{
    public WeatherComponentBase weatherComponent;

    private static ClientInstanceMethod<Vector3> SendLightningStrike = ClientInstanceMethod<Vector3>.Get(typeof(LightningWeatherComponent), "ReceiveLightningStrike");

    private List<Vector3> playerPositions = new List<Vector3>();

    internal NetId netId;

    private float timer;

    private bool hasInitializedTimer;

    private static AssetReference<EffectAsset> LightningHitRef = new AssetReference<EffectAsset>("bed12ffc45694cd69217924d75e96fe9");

    public NetId GetNetId()
    {
        return netId;
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public void ReceiveLightningStrike(Vector3 hitPosition)
    {
    }

    private void Update()
    {
        if (!Provider.isServer || weatherComponent == null || !weatherComponent.isFullyTransitionedIn)
        {
            return;
        }
        if (hasInitializedTimer)
        {
            timer -= Time.deltaTime;
            if (timer > 0f)
            {
                return;
            }
            hasInitializedTimer = false;
            playerPositions.Clear();
            foreach (Player item in weatherComponent.EnumerateMaskedPlayers())
            {
                if (!item.life.isDead)
                {
                    playerPositions.Add(item.transform.position);
                }
            }
            if (!playerPositions.IsEmpty())
            {
                int index = Random.Range(0, playerPositions.Count - 1);
                Vector3 vector = MathfEx.RandomPositionInCircleY(playerPositions[index], weatherComponent.asset.lightningTargetRadius);
                RaycastHit hitInfo;
                Vector3 vector2 = (Physics.Raycast(new Vector3(vector.x, Level.HEIGHT, vector.z), Vector3.down, out hitInfo, Level.HEIGHT * 2f, 471449600) ? hitInfo.point : vector);
                SendLightningStrike.Invoke(GetNetId(), ENetReliability.Reliable, Provider.EnumerateClients_WithinSphere(vector2, 600f), vector2);
                StartCoroutine(DoExplosionDamage(vector2));
            }
        }
        else
        {
            timer = Random.Range(weatherComponent.asset.minLightningInterval, weatherComponent.asset.maxLightningInterval);
            hasInitializedTimer = true;
        }
    }

    private IEnumerator DoExplosionDamage(Vector3 hitPosition)
    {
        yield return new WaitForSeconds(1f);
        ExplosionParameters parameters = new ExplosionParameters(hitPosition, 10f, EDeathCause.BURNING);
        parameters.damageOrigin = EDamageOrigin.Lightning;
        parameters.playImpactEffect = false;
        parameters.playerDamage = 75f;
        parameters.zombieDamage = 200f;
        parameters.animalDamage = 200f;
        parameters.barricadeDamage = 100f;
        parameters.structureDamage = 100f;
        parameters.vehicleDamage = 200f;
        parameters.resourceDamage = 1000f;
        parameters.objectDamage = 1000f;
        parameters.launchSpeed = 50f;
        DamageTool.explode(parameters, out var _);
    }

    private void Start()
    {
    }

    private void OnDestroy()
    {
        if (!netId.IsNull())
        {
            NetIdRegistry.Release(netId);
            netId.Clear();
        }
    }
}
