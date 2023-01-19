using System.Collections;
using System.Collections.Generic;
using SDG.NetTransport;
using UnityEngine;

namespace SDG.Unturned;

public class LightningWeatherComponent : MonoBehaviour
{
    public WeatherComponentBase weatherComponent;

    private static ClientInstanceMethod<Vector3> SendLightningStrike = ClientInstanceMethod<Vector3>.Get(typeof(LightningWeatherComponent), "ReceiveLightningStrike");

    private GameObject effectInstance;

    private LineRenderer lineRenderer;

    private AudioClip nearClip;

    private AudioClip mediumClip;

    private AudioClip farClip;

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
        StartCoroutine(PlayEffect(hitPosition));
        AudioClip audioClip = farClip;
        if (MainCamera.instance != null)
        {
            float num = MathfEx.HorizontalDistanceSquared(MainCamera.instance.transform.position, hitPosition);
            if (num < 10000f)
            {
                audioClip = nearClip;
            }
            else if (num < 90000f)
            {
                audioClip = mediumClip;
            }
        }
        if (audioClip != null)
        {
            OneShotAudioParameters oneShotAudioParameters = new OneShotAudioParameters(hitPosition, audioClip);
            oneShotAudioParameters.RandomizePitch(0.95f, 1.05f);
            oneShotAudioParameters.SetLinearRolloff(0f, 2048f);
            oneShotAudioParameters.Play();
        }
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

    private IEnumerator AsyncLoadEffects()
    {
        AssetBundleRequest request4 = Assets.coreMasterBundle.LoadAssetAsync<GameObject>("Effects/Weather/Lightning/LightningEffect.prefab");
        if (request4 != null)
        {
            yield return request4;
            GameObject gameObject = request4.asset as GameObject;
            if (gameObject != null)
            {
                effectInstance = Object.Instantiate(gameObject);
                effectInstance.SetActive(value: false);
                lineRenderer = effectInstance.GetComponent<LineRenderer>();
            }
        }
        request4 = Assets.coreMasterBundle.LoadAssetAsync<AudioClip>("Effects/Weather/Lightning/thunder_lightning_strike_rumble_01.wav");
        yield return request4;
        nearClip = request4.asset as AudioClip;
        request4 = Assets.coreMasterBundle.LoadAssetAsync<AudioClip>("Effects/Weather/Lightning/thunder_lightning_strike_rumble_02.wav");
        yield return request4;
        mediumClip = request4.asset as AudioClip;
        request4 = Assets.coreMasterBundle.LoadAssetAsync<AudioClip>("Effects/Weather/Lightning/thunder_lightning_strike_rumble_04.wav");
        yield return request4;
        farClip = request4.asset as AudioClip;
    }

    private IEnumerator PlayEffect(Vector3 hitPosition)
    {
        if (!(effectInstance == null))
        {
            yield return new WaitForSeconds(1f);
            Vector3 position = hitPosition;
            position.y = Level.HEIGHT;
            effectInstance.transform.position = position;
            float num = Level.HEIGHT - hitPosition.y;
            int num2 = Mathf.CeilToInt(num / 25f);
            Vector3[] array = new Vector3[num2 + 1];
            array[0] = hitPosition;
            for (int i = 1; i <= num2; i++)
            {
                float num3 = (float)i / (float)num2;
                Vector2 vector = MathfEx.RandomPositionInCircle(50f * num3);
                array[i] = hitPosition + new Vector3(vector.x, num3 * num, vector.y);
            }
            lineRenderer.positionCount = array.Length;
            lineRenderer.SetPositions(array);
            EffectAsset effectAsset = Assets.find(LightningHitRef);
            if (effectAsset != null)
            {
                EffectManager.effect(effectAsset, hitPosition, Vector3.up);
            }
            effectInstance.SetActive(value: true);
            yield return new WaitForSeconds(0.1f);
            effectInstance.SetActive(value: false);
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
        StartCoroutine(AsyncLoadEffects());
    }

    private void OnDestroy()
    {
        if (effectInstance != null)
        {
            Object.Destroy(effectInstance);
            effectInstance = null;
        }
        if (!netId.IsNull())
        {
            NetIdRegistry.Release(netId);
            netId.Clear();
        }
    }
}
