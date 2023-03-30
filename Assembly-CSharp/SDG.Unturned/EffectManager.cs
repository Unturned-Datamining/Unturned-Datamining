using System;
using System.Collections.Generic;
using SDG.NetTransport;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SDG.Unturned;

public class EffectManager : SteamCaller
{
    public delegate void EffectButtonClickedHandler(Player player, string buttonName);

    public delegate void EffectTextCommittedHandler(Player player, string buttonName, string text);

    private struct UIEffectInstance
    {
        public EffectAsset asset;

        public GameObject gameObject;

        public UIEffectInstance(EffectAsset asset, GameObject gameObject)
        {
            this.asset = asset;
            this.gameObject = gameObject;
        }
    }

    public static readonly float SMALL = 64f;

    public static readonly float MEDIUM = 128f;

    public static readonly float LARGE = 256f;

    public static readonly float INSANE = 512f;

    private static List<Text> formattingComponents = new List<Text>();

    private static List<Button> buttonComponents = new List<Button>();

    private static List<InputField> inputFieldComponents = new List<InputField>();

    private static List<TextMeshProUGUI> tmpTexts = new List<TextMeshProUGUI>();

    private static List<TMP_InputField> tmpInputFields = new List<TMP_InputField>();

    private static EffectManager manager;

    private static GameObjectPoolDictionary pool;

    private static Dictionary<short, GameObject> indexedUIEffects;

    private static readonly ClientStaticMethod<ushort> SendEffectClearById = ClientStaticMethod<ushort>.Get(ReceiveEffectClearById);

    private static readonly ClientStaticMethod<Guid> SendEffectClearByGuid = ClientStaticMethod<Guid>.Get(ReceiveEffectClearByGuid);

    private static readonly ClientStaticMethod SendEffectClearAll = ClientStaticMethod.Get(ReceiveEffectClearAll);

    private static AssetReference<EffectAsset> FiremodeRef = new AssetReference<EffectAsset>("bc41e0feaebe4e788a3612811b8722d3");

    private static readonly ClientStaticMethod<Guid, Vector3, Vector3, Vector3> SendEffectPointNormal_NonUniformScale = ClientStaticMethod<Guid, Vector3, Vector3, Vector3>.Get(ReceiveEffectPointNormal_NonUniformScale);

    private static readonly ClientStaticMethod<Guid, Vector3, Vector3, float> SendEffectPointNormal_UniformScale = ClientStaticMethod<Guid, Vector3, Vector3, float>.Get(ReceiveEffectPointNormal_UniformScale);

    private static readonly ClientStaticMethod<Guid, Vector3, Vector3> SendEffectPointNormal = ClientStaticMethod<Guid, Vector3, Vector3>.Get(ReceiveEffectPointNormal);

    private static readonly ClientStaticMethod<Guid, Vector3, Vector3> SendEffectPoint_NonUniformScale = ClientStaticMethod<Guid, Vector3, Vector3>.Get(ReceiveEffectPoint_NonUniformScale);

    private static readonly ClientStaticMethod<Guid, Vector3, float> SendEffectPoint_UniformScale = ClientStaticMethod<Guid, Vector3, float>.Get(ReceiveEffectPoint_UniformScale);

    private static readonly ClientStaticMethod<Guid, Vector3> SendEffectPoint = ClientStaticMethod<Guid, Vector3>.Get(ReceiveEffectPoint);

    private static readonly ClientStaticMethod<ushort, short> SendUIEffect = ClientStaticMethod<ushort, short>.Get(ReceiveUIEffect);

    private static readonly ClientStaticMethod<ushort, short, string> SendUIEffect1Arg = ClientStaticMethod<ushort, short, string>.Get(ReceiveUIEffect1Arg);

    private static readonly ClientStaticMethod<ushort, short, string, string> SendUIEffect2Args = ClientStaticMethod<ushort, short, string, string>.Get(ReceiveUIEffect2Args);

    private static readonly ClientStaticMethod<ushort, short, string, string, string> SendUIEffect3Args = ClientStaticMethod<ushort, short, string, string, string>.Get(ReceiveUIEffect3Args);

    private static readonly ClientStaticMethod<ushort, short, string, string, string, string> SendUIEffect4Args = ClientStaticMethod<ushort, short, string, string, string, string>.Get(ReceiveUIEffect4Args);

    private static readonly ClientStaticMethod<short, string, bool> SendUIEffectVisibility = ClientStaticMethod<short, string, bool>.Get(ReceiveUIEffectVisibility);

    private static readonly ClientStaticMethod<short, string, string> SendUIEffectText = ClientStaticMethod<short, string, string>.Get(ReceiveUIEffectText);

    private static readonly ClientStaticMethod<short, string, string, bool, bool> SendUIEffectImageURL = ClientStaticMethod<short, string, string, bool, bool>.Get(ReceiveUIEffectImageURL);

    public static EffectButtonClickedHandler onEffectButtonClicked;

    private static readonly ServerStaticMethod<string> SendEffectClicked = ServerStaticMethod<string>.Get(ReceiveEffectClicked);

    public static EffectTextCommittedHandler onEffectTextCommitted;

    private static readonly ServerStaticMethod<string, string> SendEffectTextCommitted = ServerStaticMethod<string, string>.Get(ReceiveEffectTextCommitted);

    private List<GameObject> debrisGameObjects = new List<GameObject>();

    private List<UIEffectInstance> uiEffectInstances = new List<UIEffectInstance>();

    private static Dictionary<Transform, List<GameObject>> attachedEffects;

    private static Stack<List<GameObject>> attachedEffectsListPool;

    public static EffectManager instance => manager;

    [Obsolete("Renamed to InstantiateFromPool to fix name conflict with Object.Instantiate")]
    public static GameObject Instantiate(GameObject element)
    {
        return InstantiateFromPool(element);
    }

    public static GameObject InstantiateFromPool(GameObject element)
    {
        PoolReference poolReference = pool.Instantiate(element);
        poolReference.excludeFromDestroyAll = true;
        GameObject obj = poolReference.gameObject;
        ParticleSystem component = obj.GetComponent<ParticleSystem>();
        if (component != null)
        {
            component.Stop(withChildren: true);
            component.Clear(withChildren: true);
        }
        obj.tag = "Debris";
        obj.layer = 12;
        return obj;
    }

    [Obsolete("Renamed to DestroyIntoPool to fix name conflict with Object.Destroy")]
    public static void Destroy(GameObject element)
    {
        DestroyIntoPool(element);
    }

    public static void DestroyIntoPool(GameObject element)
    {
        if (!(element == null))
        {
            pool.Destroy(element);
        }
    }

    [Obsolete("Renamed to DestroyIntoPool to fix name conflict with Object.Destroy")]
    public static void Destroy(GameObject element, float t)
    {
        DestroyIntoPool(element, t);
    }

    public static void DestroyIntoPool(GameObject element, float t)
    {
        if (!(element == null))
        {
            pool.Destroy(element, t);
        }
    }

    [Obsolete]
    public void tellEffectClearByID(CSteamID steamID, ushort id)
    {
        ReceiveEffectClearById(id);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellEffectClearByID")]
    public static void ReceiveEffectClearById(ushort id)
    {
        if (Assets.find(EAssetType.EFFECT, id) is EffectAsset asset)
        {
            ClearEffect(asset);
        }
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public static void ReceiveEffectClearByGuid(Guid assetGuid)
    {
        EffectAsset effectAsset = Assets.find<EffectAsset>(assetGuid);
        if (effectAsset != null)
        {
            ClearEffect(effectAsset);
        }
    }

    private static void ClearEffect(EffectAsset asset)
    {
        if (asset.effect != null)
        {
            pool.DestroyAllMatchingPrefab(asset.effect);
        }
        if (asset.splatter > 0)
        {
            GameObject[] splatters = asset.splatters;
            foreach (GameObject prefab in splatters)
            {
                pool.DestroyAllMatchingPrefab(prefab);
            }
        }
        for (int num = manager.uiEffectInstances.Count - 1; num >= 0; num--)
        {
            UIEffectInstance uIEffectInstance = manager.uiEffectInstances[num];
            if (uIEffectInstance.asset == asset)
            {
                if (uIEffectInstance.gameObject != null)
                {
                    UnityEngine.Object.Destroy(uIEffectInstance.gameObject);
                }
                manager.uiEffectInstances.RemoveAtFast(num);
            }
        }
    }

    [Obsolete]
    public static void askEffectClearByID(ushort id, CSteamID steamID)
    {
        ITransportConnection transportConnection = Provider.findTransportConnection(steamID);
        if (transportConnection != null)
        {
            askEffectClearByID(id, transportConnection);
        }
    }

    public static void askEffectClearByID(ushort id, ITransportConnection transportConnection)
    {
        SendEffectClearById.Invoke(ENetReliability.Reliable, transportConnection, id);
    }

    public static void ClearEffectByID_AllPlayers(ushort id)
    {
        SendEffectClearById.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), id);
    }

    public static void ClearEffectByGuid(Guid assetGuid, ITransportConnection transportConnection)
    {
        SendEffectClearByGuid.Invoke(ENetReliability.Reliable, transportConnection, assetGuid);
    }

    public static void ClearEffectByGuid_AllPlayers(Guid assetGuid)
    {
        SendEffectClearByGuid.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), assetGuid);
    }

    [Obsolete]
    public void tellEffectClearAll(CSteamID steamID)
    {
        ReceiveEffectClearAll();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellEffectClearAll")]
    public static void ReceiveEffectClearAll()
    {
        pool.DestroyAll();
        manager.destroyAllDebris();
        manager.destroyAllUI();
    }

    public static void askEffectClearAll()
    {
        if (Provider.isServer)
        {
            SendEffectClearAll.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections());
        }
    }

    internal static void TriggerFiremodeEffect(Vector3 position)
    {
        EffectAsset effectAsset = FiremodeRef.Find();
        if (effectAsset != null)
        {
            TriggerEffectParameters parameters = new TriggerEffectParameters(effectAsset);
            parameters.position = position;
            parameters.relevantDistance = SMALL;
            triggerEffect(parameters);
        }
    }

    [Obsolete("Please use TriggerEffectParameters with guid instead")]
    public static void sendEffect(ushort id, byte x, byte y, byte area, Vector3 point, Vector3 normal)
    {
        if (Assets.find(EAssetType.EFFECT, id) is EffectAsset asset)
        {
            TriggerEffectParameters parameters = new TriggerEffectParameters(asset);
            parameters.reliable = false;
            parameters.SetRelevantTransportConnections(Regions.GatherClientConnections(x, y, area));
            parameters.position = point;
            parameters.direction = normal;
            triggerEffect(parameters);
        }
    }

    [Obsolete("Please use TriggerEffectParameters with guid instead")]
    public static void sendEffect(ushort id, byte x, byte y, byte area, Vector3 point)
    {
        if (Assets.find(EAssetType.EFFECT, id) is EffectAsset asset)
        {
            TriggerEffectParameters parameters = new TriggerEffectParameters(asset);
            parameters.reliable = false;
            parameters.SetRelevantTransportConnections(Regions.GatherClientConnections(x, y, area));
            parameters.position = point;
            triggerEffect(parameters);
        }
    }

    [Obsolete("Please use TriggerEffectParameters with guid instead")]
    public static void sendEffectReliable(ushort id, byte x, byte y, byte area, Vector3 point, Vector3 normal)
    {
        if (Assets.find(EAssetType.EFFECT, id) is EffectAsset asset)
        {
            TriggerEffectParameters parameters = new TriggerEffectParameters(asset);
            parameters.reliable = true;
            parameters.SetRelevantTransportConnections(Regions.GatherClientConnections(x, y, area));
            parameters.position = point;
            parameters.direction = normal;
            triggerEffect(parameters);
        }
    }

    [Obsolete("Please use TriggerEffectParameters with guid instead")]
    public static void sendEffectReliable(ushort id, byte x, byte y, byte area, Vector3 point)
    {
        if (Assets.find(EAssetType.EFFECT, id) is EffectAsset asset)
        {
            TriggerEffectParameters parameters = new TriggerEffectParameters(asset);
            parameters.reliable = true;
            parameters.SetRelevantTransportConnections(Regions.GatherClientConnections(x, y, area));
            parameters.position = point;
            triggerEffect(parameters);
        }
    }

    [Obsolete("Please use TriggerEffectParameters with guid instead")]
    public static void sendEffect(ushort id, float radius, Vector3 point, Vector3 normal)
    {
        if (Assets.find(EAssetType.EFFECT, id) is EffectAsset asset)
        {
            TriggerEffectParameters parameters = new TriggerEffectParameters(asset);
            parameters.reliable = false;
            parameters.relevantDistance = radius;
            parameters.position = point;
            parameters.direction = normal;
            triggerEffect(parameters);
        }
    }

    [Obsolete("Please use TriggerEffectParameters with guid instead")]
    public static void sendEffect(ushort id, float radius, Vector3 point)
    {
        if (Assets.find(EAssetType.EFFECT, id) is EffectAsset asset)
        {
            TriggerEffectParameters parameters = new TriggerEffectParameters(asset);
            parameters.reliable = false;
            parameters.relevantDistance = radius;
            parameters.position = point;
            triggerEffect(parameters);
        }
    }

    [Obsolete("Please use TriggerEffectParameters with guid instead")]
    public static void sendEffectReliable(ushort id, float radius, Vector3 point, Vector3 normal)
    {
        if (Assets.find(EAssetType.EFFECT, id) is EffectAsset asset)
        {
            TriggerEffectParameters parameters = new TriggerEffectParameters(asset);
            parameters.reliable = true;
            parameters.relevantDistance = radius;
            parameters.position = point;
            parameters.direction = normal;
            triggerEffect(parameters);
        }
    }

    [Obsolete("Please use TriggerEffectParameters with guid instead")]
    public static void sendEffectReliable(ushort id, float radius, Vector3 point)
    {
        if (Assets.find(EAssetType.EFFECT, id) is EffectAsset asset)
        {
            TriggerEffectParameters parameters = new TriggerEffectParameters(asset);
            parameters.reliable = true;
            parameters.relevantDistance = radius;
            parameters.position = point;
            triggerEffect(parameters);
        }
    }

    [Obsolete("Please use TriggerEffectParameters with guid instead")]
    public static void sendEffect(ushort id, CSteamID steamID, Vector3 point, Vector3 normal)
    {
        ITransportConnection transportConnection = Provider.findTransportConnection(steamID);
        if (transportConnection != null)
        {
            sendEffect(id, transportConnection, point, normal);
        }
    }

    [Obsolete("Please use TriggerEffectParameters with guid instead")]
    public static void sendEffect(ushort id, ITransportConnection transportConnection, Vector3 point, Vector3 normal)
    {
        if (Assets.find(EAssetType.EFFECT, id) is EffectAsset asset)
        {
            TriggerEffectParameters parameters = new TriggerEffectParameters(asset);
            parameters.reliable = false;
            parameters.SetRelevantPlayer(transportConnection);
            parameters.position = point;
            parameters.direction = normal;
            triggerEffect(parameters);
        }
    }

    [Obsolete("Please use TriggerEffectParameters with guid instead")]
    public static void sendEffect(ushort id, CSteamID steamID, Vector3 point)
    {
        ITransportConnection transportConnection = Provider.findTransportConnection(steamID);
        if (transportConnection != null)
        {
            sendEffect(id, transportConnection, point);
        }
    }

    [Obsolete("Please use TriggerEffectParameters with guid instead")]
    public static void sendEffect(ushort id, ITransportConnection transportConnection, Vector3 point)
    {
        if (Assets.find(EAssetType.EFFECT, id) is EffectAsset asset)
        {
            TriggerEffectParameters parameters = new TriggerEffectParameters(asset);
            parameters.reliable = false;
            parameters.SetRelevantPlayer(transportConnection);
            parameters.position = point;
            triggerEffect(parameters);
        }
    }

    [Obsolete("Please use TriggerEffectParameters with guid instead")]
    public static void sendEffectReliable(ushort id, CSteamID steamID, Vector3 point, Vector3 normal)
    {
        ITransportConnection transportConnection = Provider.findTransportConnection(steamID);
        if (transportConnection != null)
        {
            sendEffectReliable(id, transportConnection, point, normal);
        }
    }

    [Obsolete("Please use TriggerEffectParameters with guid instead")]
    public static void sendEffectReliable(ushort id, ITransportConnection transportConnection, Vector3 point, Vector3 normal)
    {
        if (Assets.find(EAssetType.EFFECT, id) is EffectAsset asset)
        {
            TriggerEffectParameters parameters = new TriggerEffectParameters(asset);
            parameters.reliable = true;
            parameters.SetRelevantPlayer(transportConnection);
            parameters.position = point;
            parameters.direction = normal;
            triggerEffect(parameters);
        }
    }

    [Obsolete("Please use TriggerEffectParameters with guid instead")]
    public static void sendEffectReliable(ushort id, CSteamID steamID, Vector3 point)
    {
        ITransportConnection transportConnection = Provider.findTransportConnection(steamID);
        if (transportConnection != null)
        {
            sendEffectReliable(id, transportConnection, point);
        }
    }

    [Obsolete("Please use TriggerEffectParameters with guid instead")]
    public static void sendEffectReliable(ushort id, ITransportConnection transportConnection, Vector3 point)
    {
        if (Assets.find(EAssetType.EFFECT, id) is EffectAsset asset)
        {
            TriggerEffectParameters parameters = new TriggerEffectParameters(asset);
            parameters.reliable = true;
            parameters.SetRelevantPlayer(transportConnection);
            parameters.position = point;
            triggerEffect(parameters);
        }
    }

    [Obsolete("Please use TriggerEffectParameters with guid instead")]
    public static void sendEffectReliable(ushort id, CSteamID steamID, Vector3 point, Vector3 normal, float uniformScale)
    {
        ITransportConnection transportConnection = Provider.findTransportConnection(steamID);
        if (transportConnection != null)
        {
            sendEffectReliable(id, transportConnection, point, normal, uniformScale);
        }
    }

    [Obsolete("Please use TriggerEffectParameters with guid instead")]
    public static void sendEffectReliable(ushort id, ITransportConnection transportConnection, Vector3 point, Vector3 normal, float uniformScale)
    {
        if (Assets.find(EAssetType.EFFECT, id) is EffectAsset asset)
        {
            TriggerEffectParameters parameters = new TriggerEffectParameters(asset);
            parameters.reliable = true;
            parameters.SetRelevantPlayer(transportConnection);
            parameters.position = point;
            parameters.direction = normal;
            parameters.SetUniformScale(uniformScale);
            triggerEffect(parameters);
        }
    }

    [Obsolete("Please use TriggerEffectParameters with guid instead")]
    public static void sendEffectReliable_NonUniformScale(ushort id, CSteamID steamID, Vector3 point, Vector3 normal, Vector3 scale)
    {
        ITransportConnection transportConnection = Provider.findTransportConnection(steamID);
        if (transportConnection != null)
        {
            sendEffectReliable_NonUniformScale(id, transportConnection, point, normal, scale);
        }
    }

    [Obsolete("Please use TriggerEffectParameters with guid instead")]
    public static void sendEffectReliable_NonUniformScale(ushort id, ITransportConnection transportConnection, Vector3 point, Vector3 normal, Vector3 scale)
    {
        if (Assets.find(EAssetType.EFFECT, id) is EffectAsset asset)
        {
            TriggerEffectParameters parameters = new TriggerEffectParameters(asset);
            parameters.reliable = true;
            parameters.SetRelevantPlayer(transportConnection);
            parameters.position = point;
            parameters.direction = normal;
            parameters.scale = scale;
            triggerEffect(parameters);
        }
    }

    [Obsolete("Please use TriggerEffectParameters with guid instead")]
    public static void sendEffectReliable(ushort id, CSteamID steamID, Vector3 point, float uniformScale)
    {
        ITransportConnection transportConnection = Provider.findTransportConnection(steamID);
        if (transportConnection != null)
        {
            sendEffectReliable(id, transportConnection, point, uniformScale);
        }
    }

    [Obsolete("Please use TriggerEffectParameters with guid instead")]
    public static void sendEffectReliable(ushort id, ITransportConnection transportConnection, Vector3 point, float uniformScale)
    {
        if (Assets.find(EAssetType.EFFECT, id) is EffectAsset asset)
        {
            TriggerEffectParameters parameters = new TriggerEffectParameters(asset);
            parameters.reliable = true;
            parameters.SetRelevantPlayer(transportConnection);
            parameters.position = point;
            parameters.SetUniformScale(uniformScale);
            triggerEffect(parameters);
        }
    }

    [Obsolete("Please use TriggerEffectParameters with guid instead")]
    public static void sendEffectReliable_NonUniformScale(ushort id, CSteamID steamID, Vector3 point, Vector3 scale)
    {
        ITransportConnection transportConnection = Provider.findTransportConnection(steamID);
        if (transportConnection != null)
        {
            sendEffectReliable_NonUniformScale(id, transportConnection, point, scale);
        }
    }

    [Obsolete("Please use TriggerEffectParameters with guid instead")]
    public static void sendEffectReliable_NonUniformScale(ushort id, ITransportConnection transportConnection, Vector3 point, Vector3 scale)
    {
        if (Assets.find(EAssetType.EFFECT, id) is EffectAsset asset)
        {
            TriggerEffectParameters parameters = new TriggerEffectParameters(asset);
            parameters.reliable = true;
            parameters.SetRelevantPlayer(transportConnection);
            parameters.position = point;
            parameters.scale = scale;
            triggerEffect(parameters);
        }
    }

    public static void sendUIEffect(ushort id, short key, bool reliable)
    {
        ENetReliability reliability = ((!reliable) ? ENetReliability.Unreliable : ENetReliability.Reliable);
        SendUIEffect.Invoke(reliability, Provider.GatherClientConnections(), id, key);
    }

    public static void sendUIEffect(ushort id, short key, bool reliable, string arg0)
    {
        ENetReliability reliability = ((!reliable) ? ENetReliability.Unreliable : ENetReliability.Reliable);
        SendUIEffect1Arg.Invoke(reliability, Provider.GatherClientConnections(), id, key, arg0);
    }

    public static void sendUIEffect(ushort id, short key, bool reliable, string arg0, string arg1)
    {
        ENetReliability reliability = ((!reliable) ? ENetReliability.Unreliable : ENetReliability.Reliable);
        SendUIEffect2Args.Invoke(reliability, Provider.GatherClientConnections(), id, key, arg0, arg1);
    }

    public static void sendUIEffect(ushort id, short key, bool reliable, string arg0, string arg1, string arg2)
    {
        ENetReliability reliability = ((!reliable) ? ENetReliability.Unreliable : ENetReliability.Reliable);
        SendUIEffect3Args.Invoke(reliability, Provider.GatherClientConnections(), id, key, arg0, arg1, arg2);
    }

    public static void sendUIEffect(ushort id, short key, bool reliable, string arg0, string arg1, string arg2, string arg3)
    {
        ENetReliability reliability = ((!reliable) ? ENetReliability.Unreliable : ENetReliability.Reliable);
        SendUIEffect4Args.Invoke(reliability, Provider.GatherClientConnections(), id, key, arg0, arg1, arg2, arg3);
    }

    [Obsolete]
    public static void sendUIEffect(ushort id, short key, CSteamID steamID, bool reliable)
    {
        ITransportConnection transportConnection = Provider.findTransportConnection(steamID);
        if (transportConnection != null)
        {
            sendUIEffect(id, key, transportConnection, reliable);
        }
    }

    public static void sendUIEffect(ushort id, short key, ITransportConnection transportConnection, bool reliable)
    {
        ENetReliability reliability = ((!reliable) ? ENetReliability.Unreliable : ENetReliability.Reliable);
        SendUIEffect.Invoke(reliability, transportConnection, id, key);
    }

    [Obsolete]
    public static void sendUIEffect(ushort id, short key, CSteamID steamID, bool reliable, string arg0)
    {
        ITransportConnection transportConnection = Provider.findTransportConnection(steamID);
        if (transportConnection != null)
        {
            sendUIEffect(id, key, transportConnection, reliable, arg0);
        }
    }

    public static void sendUIEffect(ushort id, short key, ITransportConnection transportConnection, bool reliable, string arg0)
    {
        ENetReliability reliability = ((!reliable) ? ENetReliability.Unreliable : ENetReliability.Reliable);
        SendUIEffect1Arg.Invoke(reliability, transportConnection, id, key, arg0);
    }

    [Obsolete]
    public static void sendUIEffect(ushort id, short key, CSteamID steamID, bool reliable, string arg0, string arg1)
    {
        ITransportConnection transportConnection = Provider.findTransportConnection(steamID);
        if (transportConnection != null)
        {
            sendUIEffect(id, key, transportConnection, reliable, arg0, arg1);
        }
    }

    public static void sendUIEffect(ushort id, short key, ITransportConnection transportConnection, bool reliable, string arg0, string arg1)
    {
        ENetReliability reliability = ((!reliable) ? ENetReliability.Unreliable : ENetReliability.Reliable);
        SendUIEffect2Args.Invoke(reliability, transportConnection, id, key, arg0, arg1);
    }

    [Obsolete]
    public static void sendUIEffect(ushort id, short key, CSteamID steamID, bool reliable, string arg0, string arg1, string arg2)
    {
        ITransportConnection transportConnection = Provider.findTransportConnection(steamID);
        if (transportConnection != null)
        {
            sendUIEffect(id, key, transportConnection, reliable, arg0, arg1, arg2);
        }
    }

    public static void sendUIEffect(ushort id, short key, ITransportConnection transportConnection, bool reliable, string arg0, string arg1, string arg2)
    {
        ENetReliability reliability = ((!reliable) ? ENetReliability.Unreliable : ENetReliability.Reliable);
        SendUIEffect3Args.Invoke(reliability, transportConnection, id, key, arg0, arg1, arg2);
    }

    [Obsolete]
    public static void sendUIEffect(ushort id, short key, CSteamID steamID, bool reliable, string arg0, string arg1, string arg2, string arg3)
    {
        ITransportConnection transportConnection = Provider.findTransportConnection(steamID);
        if (transportConnection != null)
        {
            sendUIEffect(id, key, transportConnection, reliable, arg0, arg1, arg2, arg3);
        }
    }

    public static void sendUIEffect(ushort id, short key, ITransportConnection transportConnection, bool reliable, string arg0, string arg1, string arg2, string arg3)
    {
        ENetReliability reliability = ((!reliable) ? ENetReliability.Unreliable : ENetReliability.Reliable);
        SendUIEffect4Args.Invoke(reliability, transportConnection, id, key, arg0, arg1, arg2, arg3);
    }

    [Obsolete]
    public static void sendUIEffectVisibility(short key, CSteamID steamID, bool reliable, string childName, bool visible)
    {
        ITransportConnection transportConnection = Provider.findTransportConnection(steamID);
        if (transportConnection != null)
        {
            sendUIEffectVisibility(key, transportConnection, reliable, childName, visible);
        }
    }

    public static void sendUIEffectVisibility(short key, ITransportConnection transportConnection, bool reliable, string childName, bool visible)
    {
        ENetReliability reliability = ((!reliable) ? ENetReliability.Unreliable : ENetReliability.Reliable);
        SendUIEffectVisibility.Invoke(reliability, transportConnection, key, childName, visible);
    }

    [Obsolete]
    public static void sendUIEffectText(short key, CSteamID steamID, bool reliable, string childName, string text)
    {
        ITransportConnection transportConnection = Provider.findTransportConnection(steamID);
        if (transportConnection != null)
        {
            sendUIEffectText(key, transportConnection, reliable, childName, text);
        }
    }

    public static void sendUIEffectText(short key, ITransportConnection transportConnection, bool reliable, string childName, string text)
    {
        ENetReliability reliability = ((!reliable) ? ENetReliability.Unreliable : ENetReliability.Reliable);
        SendUIEffectText.Invoke(reliability, transportConnection, key, childName, text);
    }

    public static void sendUIEffectImageURL(short key, CSteamID steamID, bool reliable, string childName, string url)
    {
        ITransportConnection transportConnection = Provider.findTransportConnection(steamID);
        if (transportConnection != null)
        {
            sendUIEffectImageURL(key, transportConnection, reliable, childName, url);
        }
    }

    public static void sendUIEffectImageURL(short key, CSteamID steamID, bool reliable, string childName, string url, bool shouldCache = true, bool forceRefresh = false)
    {
        ITransportConnection transportConnection = Provider.findTransportConnection(steamID);
        if (transportConnection != null)
        {
            sendUIEffectImageURL(key, transportConnection, reliable, childName, url, shouldCache, forceRefresh);
        }
    }

    public static void sendUIEffectImageURL(short key, ITransportConnection transportConnection, bool reliable, string childName, string url, bool shouldCache = true, bool forceRefresh = false)
    {
        ENetReliability reliability = ((!reliable) ? ENetReliability.Unreliable : ENetReliability.Reliable);
        SendUIEffectImageURL.Invoke(reliability, transportConnection, key, childName, url, shouldCache, forceRefresh);
    }

    [Obsolete]
    public void tellEffectPointNormal_NonUniformScale(CSteamID steamID, ushort id, Vector3 point, Vector3 normal, Vector3 scale)
    {
        effect(id, point, normal, scale);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public static void ReceiveEffectPointNormal_NonUniformScale(Guid assetGuid, Vector3 point, Vector3 normal, Vector3 scale)
    {
        effect(assetGuid, point, normal, scale);
    }

    [Obsolete]
    public void tellEffectPointNormal_UniformScale(CSteamID steamID, ushort id, Vector3 point, Vector3 normal, float uniformScale)
    {
        effect(id, point, normal, new Vector3(uniformScale, uniformScale, uniformScale));
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public static void ReceiveEffectPointNormal_UniformScale(Guid assetGuid, Vector3 point, Vector3 normal, float uniformScale)
    {
        effect(assetGuid, point, normal, new Vector3(uniformScale, uniformScale, uniformScale));
    }

    [Obsolete]
    public void tellEffectPointNormal(CSteamID steamID, ushort id, Vector3 point, Vector3 normal)
    {
        effect(id, point, normal);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public static void ReceiveEffectPointNormal(Guid assetGuid, Vector3 point, Vector3 normal)
    {
        effect(assetGuid, point, normal);
    }

    [Obsolete]
    public void tellEffectPoint_NonUniformScale(CSteamID steamID, ushort id, Vector3 point, Vector3 scale)
    {
        effect(id, point, Vector3.up, scale);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellEffectPoint_NonUniformScale")]
    public static void ReceiveEffectPoint_NonUniformScale(Guid assetGuid, Vector3 point, Vector3 scale)
    {
        effect(assetGuid, point, Vector3.up, scale);
    }

    [Obsolete]
    public void tellEffectPoint_UniformScale(CSteamID steamID, ushort id, Vector3 point, float uniformScale)
    {
        effect(id, point, Vector3.up, new Vector3(uniformScale, uniformScale, uniformScale));
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellEffectPoint_UniformScale")]
    public static void ReceiveEffectPoint_UniformScale(Guid assetGuid, Vector3 point, float uniformScale)
    {
        effect(assetGuid, point, Vector3.up, new Vector3(uniformScale, uniformScale, uniformScale));
    }

    [Obsolete]
    public void tellEffectPoint(CSteamID steamID, ushort id, Vector3 point)
    {
        effect(id, point, Vector3.up);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellEffectPoint")]
    public static void ReceiveEffectPoint(Guid assetGuid, Vector3 point)
    {
        effect(assetGuid, point, Vector3.up);
    }

    [Obsolete]
    public void tellUIEffect(CSteamID steamID, ushort id, short key)
    {
        ReceiveUIEffect(id, key);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellUIEffect")]
    public static void ReceiveUIEffect(ushort id, short key)
    {
        createUIEffect(id, key);
    }

    [Obsolete]
    public void tellUIEffect1Arg(CSteamID steamID, ushort id, short key, string arg0)
    {
        ReceiveUIEffect1Arg(id, key, arg0);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellUIEffect1Arg")]
    public static void ReceiveUIEffect1Arg(ushort id, short key, string arg0)
    {
        createAndFormatUIEffect(id, key, arg0);
    }

    [Obsolete]
    public void tellUIEffect2Args(CSteamID steamID, ushort id, short key, string arg0, string arg1)
    {
        ReceiveUIEffect2Args(id, key, arg0, arg1);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellUIEffect2Args")]
    public static void ReceiveUIEffect2Args(ushort id, short key, string arg0, string arg1)
    {
        createAndFormatUIEffect(id, key, arg0, arg1);
    }

    [Obsolete]
    public void tellUIEffect3Args(CSteamID steamID, ushort id, short key, string arg0, string arg1, string arg2)
    {
        ReceiveUIEffect3Args(id, key, arg0, arg1, arg2);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellUIEffect3Args")]
    public static void ReceiveUIEffect3Args(ushort id, short key, string arg0, string arg1, string arg2)
    {
        createAndFormatUIEffect(id, key, arg0, arg1, arg2);
    }

    [Obsolete]
    public void tellUIEffect4Args(CSteamID steamID, ushort id, short key, string arg0, string arg1, string arg2, string arg3)
    {
        ReceiveUIEffect4Args(id, key, arg0, arg1, arg2, arg3);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellUIEffect4Args")]
    public static void ReceiveUIEffect4Args(ushort id, short key, string arg0, string arg1, string arg2, string arg3)
    {
        createAndFormatUIEffect(id, key, arg0, arg1, arg2, arg3);
    }

    [Obsolete]
    public void tellUIEffectVisibility(CSteamID steamID, short key, string childName, bool visible)
    {
        ReceiveUIEffectVisibility(key, childName, visible);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellUIEffectVisibility")]
    public static void ReceiveUIEffectVisibility(short key, string childName, bool visible)
    {
        if (!indexedUIEffects.TryGetValue(key, out var value))
        {
            UnturnedLog.info("tellUIEffectVisibility: key {0} not found (childName {1})", key, childName);
            return;
        }
        if (value == null)
        {
            UnturnedLog.info("tellUIEffectVisibility: key {0} was destroyed (childName {1})", key, childName);
            return;
        }
        Transform transform = value.transform.FindChildRecursive(childName);
        if (transform == null)
        {
            UnturnedLog.info("tellUIEffectVisibility: childName '{0}' not found (key {1})", childName, key);
        }
        else
        {
            transform.gameObject.SetActive(visible);
        }
    }

    [Obsolete]
    public void tellUIEffectText(CSteamID steamID, short key, string childName, string text)
    {
        ReceiveUIEffectText(key, childName, text);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellUIEffectText")]
    public static void ReceiveUIEffectText(short key, string childName, string text)
    {
        if (!indexedUIEffects.TryGetValue(key, out var value))
        {
            UnturnedLog.info("tellUIEffectText: key {0} not found (childName {1} text {2})", key, childName, text);
            return;
        }
        if (value == null)
        {
            UnturnedLog.info("tellUIEffectText: key {0} was destroyed (childName {1} text {2})", key, childName, text);
            return;
        }
        Transform transform = value.transform.FindChildRecursive(childName);
        if (transform == null)
        {
            UnturnedLog.info("tellUIEffectText: childName '{0}' not found (key {1} text {2})", childName, key, text);
            return;
        }
        Text component = transform.GetComponent<Text>();
        if (component != null)
        {
            ControlsSettings.formatPluginHotkeysIntoText(ref text);
            component.text = text;
            return;
        }
        TextMeshProUGUI component2 = transform.GetComponent<TextMeshProUGUI>();
        if (component2 != null)
        {
            ControlsSettings.formatPluginHotkeysIntoText(ref text);
            component2.text = text;
            return;
        }
        InputField component3 = transform.GetComponent<InputField>();
        if (component3 != null)
        {
            component3.SetTextWithoutNotify(text);
            return;
        }
        TMP_InputField component4 = transform.GetComponent<TMP_InputField>();
        if (component4 != null)
        {
            component4.SetTextWithoutNotify(text);
            return;
        }
        UnturnedLog.info("tellUIEffectText: '{0}' does not have a text or input field component (key {1} text {2})", childName, key, text);
    }

    [Obsolete]
    public void tellUIEffectImageURL(CSteamID steamID, short key, string childName, string url, bool shouldCache, bool forceRefresh)
    {
        ReceiveUIEffectImageURL(key, childName, url, shouldCache, forceRefresh);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellUIEffectImageURL")]
    public static void ReceiveUIEffectImageURL(short key, string childName, string url, bool shouldCache, bool forceRefresh)
    {
        if (!indexedUIEffects.TryGetValue(key, out var value))
        {
            UnturnedLog.info("tellUIEffectImageURL: key {0} not found (childName {1} url {2})", key, childName, url);
            return;
        }
        if (value == null)
        {
            UnturnedLog.info("tellUIEffectImageURL: key {0} was destroyed (childName {1} url {2})", key, childName, url);
            return;
        }
        Transform transform = value.transform.FindChildRecursive(childName);
        if (transform == null)
        {
            UnturnedLog.info("tellUIEffectImageURL: childName '{0}' not found (key {1} text {2})", childName, key, url);
            return;
        }
        Image component = transform.GetComponent<Image>();
        if (component == null)
        {
            UnturnedLog.info("tellUIEffectImageURL: '{0}' does not have an image component (key {1} url {2})", childName, key, url);
        }
        else
        {
            WebImage orAddComponent = transform.GetOrAddComponent<WebImage>();
            orAddComponent.targetImage = component;
            orAddComponent.setAddressAndRefresh(url, shouldCache, forceRefresh);
        }
    }

    [Obsolete]
    public void tellEffectClicked(CSteamID steamID, string buttonName)
    {
        ServerInvocationContext context = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
        ReceiveEffectClicked(in context, buttonName);
    }

    [SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 20, legacyName = "tellEffectClicked")]
    public static void ReceiveEffectClicked(in ServerInvocationContext context, string buttonName)
    {
        Player player = context.GetPlayer();
        if (!(player == null))
        {
            onEffectButtonClicked?.Invoke(player, buttonName);
        }
    }

    public static void sendEffectClicked(string buttonName)
    {
        SendEffectClicked.Invoke(ENetReliability.Reliable, buttonName);
    }

    [Obsolete]
    public void tellEffectTextCommitted(CSteamID steamID, string inputFieldName, string text)
    {
        ServerInvocationContext context = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
        ReceiveEffectTextCommitted(in context, inputFieldName, text);
    }

    [SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 20, legacyName = "tellEffectTextCommitted")]
    public static void ReceiveEffectTextCommitted(in ServerInvocationContext context, string inputFieldName, string text)
    {
        Player player = context.GetPlayer();
        if (!(player == null))
        {
            onEffectTextCommitted?.Invoke(player, inputFieldName, text);
        }
    }

    public static void sendEffectTextCommitted(string inputFieldName, string text)
    {
        SendEffectTextCommitted.Invoke(ENetReliability.Reliable, inputFieldName, text);
    }

    public static Transform createAndFormatUIEffect(ushort id, short key, params object[] args)
    {
        Transform transform = createUIEffect(id, key);
        if (transform != null)
        {
            formatTextIntoUIEffect(transform, args);
        }
        return transform;
    }

    private static void destroyUIEffect(short key)
    {
        if (indexedUIEffects.TryGetValue(key, out var value))
        {
            if (value != null)
            {
                UnityEngine.Object.Destroy(value);
            }
            indexedUIEffects.Remove(key);
        }
    }

    public static Transform createUIEffect(ushort id, short key)
    {
        destroyUIEffect(key);
        if (!(Assets.find(EAssetType.EFFECT, id) is EffectAsset effectAsset) || effectAsset.effect == null)
        {
            return null;
        }
        GameObject gameObject = UnityEngine.Object.Instantiate(effectAsset.effect);
        Transform transform = gameObject.transform;
        transform.name = id.ToString();
        if (key == -1)
        {
            if (effectAsset.lifetime > float.Epsilon)
            {
                UnityEngine.Object.Destroy(transform.gameObject, effectAsset.lifetime + UnityEngine.Random.Range(0f - effectAsset.lifetimeSpread, effectAsset.lifetimeSpread));
            }
        }
        else
        {
            indexedUIEffects.Add(key, transform.gameObject);
        }
        instance.uiEffectInstances.Add(new UIEffectInstance(effectAsset, gameObject));
        hookButtonsInUIEffect(transform);
        hookInputFieldsInUIEffect(transform);
        gatherFormattingForUIEffect(transform);
        formatPluginHotkeysIntoUIEffect(transform);
        return transform;
    }

    public static void gatherFormattingForUIEffect(Transform effect)
    {
        formattingComponents.Clear();
        tmpTexts.Clear();
        effect.GetComponentsInChildren(includeInactive: true, formattingComponents);
        if (formattingComponents.Count >= 1)
        {
            return;
        }
        effect.GetComponentsInChildren(includeInactive: true, tmpTexts);
        foreach (TextMeshProUGUI tmpText in tmpTexts)
        {
            TextMeshProUtils.FixupFont(tmpText);
        }
    }

    public static void formatTextIntoUIEffect(Transform effect, params object[] args)
    {
        if (formattingComponents.Count > 0)
        {
            foreach (Text formattingComponent in formattingComponents)
            {
                formattingComponent.text = string.Format(formattingComponent.text, args);
            }
            return;
        }
        foreach (TextMeshProUGUI tmpText in tmpTexts)
        {
            tmpText.text = string.Format(tmpText.text, args);
        }
    }

    public static void formatPluginHotkeysIntoUIEffect(Transform effect)
    {
        if (formattingComponents.Count > 0)
        {
            foreach (Text formattingComponent in formattingComponents)
            {
                string text = formattingComponent.text;
                ControlsSettings.formatPluginHotkeysIntoText(ref text);
                formattingComponent.text = text;
            }
            return;
        }
        foreach (TextMeshProUGUI tmpText in tmpTexts)
        {
            string text2 = tmpText.text;
            ControlsSettings.formatPluginHotkeysIntoText(ref text2);
            tmpText.text = text2;
        }
    }

    public static void hookButtonsInUIEffect(Transform effect)
    {
        buttonComponents.Clear();
        effect.GetComponentsInChildren(includeInactive: true, buttonComponents);
        foreach (Button buttonComponent in buttonComponents)
        {
            buttonComponent.gameObject.AddComponent<PluginButtonListener>().targetButton = buttonComponent;
        }
    }

    public static void hookInputFieldsInUIEffect(Transform effect)
    {
        inputFieldComponents.Clear();
        tmpInputFields.Clear();
        effect.GetComponentsInChildren(includeInactive: true, inputFieldComponents);
        if (inputFieldComponents.Count > 0)
        {
            foreach (InputField inputFieldComponent in inputFieldComponents)
            {
                inputFieldComponent.gameObject.AddComponent<PluginInputFieldListener>().targetInputField = inputFieldComponent;
            }
            return;
        }
        effect.GetComponentsInChildren(includeInactive: true, tmpInputFields);
        foreach (TMP_InputField tmpInputField in tmpInputFields)
        {
            tmpInputField.gameObject.AddComponent<TMP_PluginInputFieldListener>().targetInputField = tmpInputField;
            TextMeshProUtils.FixupFont(tmpInputField);
        }
    }

    public static Transform effect(ushort id, Vector3 point, Vector3 normal)
    {
        return effect(id, point, normal, Vector3.one);
    }

    public static Transform effect(Guid assetGuid, Vector3 point, Vector3 normal)
    {
        return effect(assetGuid, point, normal, Vector3.one);
    }

    public static Transform effect(EffectAsset asset, Vector3 point, Vector3 normal)
    {
        return effect(asset, point, normal, Vector3.one);
    }

    public static Transform effect(ushort id, Vector3 point, Vector3 normal, Vector3 scaleMultiplier)
    {
        if (Assets.find(EAssetType.EFFECT, id) is EffectAsset asset)
        {
            return internalSpawnEffect(asset, point, normal, scaleMultiplier, wasInstigatedByPlayer: false, null);
        }
        return null;
    }

    public static Transform effect(Guid assetGuid, Vector3 point, Vector3 normal, Vector3 scaleMultiplier)
    {
        if (Assets.find(assetGuid) is EffectAsset asset)
        {
            return internalSpawnEffect(asset, point, normal, scaleMultiplier, wasInstigatedByPlayer: false, null);
        }
        return null;
    }

    public static Transform effect(EffectAsset asset, Vector3 point, Vector3 normal, Vector3 scaleMultiplier)
    {
        if (asset != null)
        {
            return internalSpawnEffect(asset, point, normal, scaleMultiplier, wasInstigatedByPlayer: false, null);
        }
        return null;
    }

    public static Transform effect(AssetReference<EffectAsset> assetRef, Vector3 position)
    {
        EffectAsset effectAsset = assetRef.Find();
        if (effectAsset != null)
        {
            return internalSpawnEffect(effectAsset, position, Vector3.up, Vector3.one, wasInstigatedByPlayer: false, null);
        }
        return null;
    }

    internal static Transform internalSpawnEffect(EffectAsset asset, Vector3 point, Vector3 normal, Vector3 scaleMultiplier, bool wasInstigatedByPlayer, Transform parent)
    {
        if (parent != null && !parent.gameObject.activeInHierarchy)
        {
            return null;
        }
        if (asset.splatterTemperature != EPlayerTemperature.NONE && (Provider.isPvP || !wasInstigatedByPlayer))
        {
            Transform obj = new GameObject().transform;
            obj.name = "Temperature";
            RegisterDebris(obj.gameObject);
            obj.position = point + Vector3.down * -2f;
            obj.localScale = Vector3.one * 6f;
            obj.gameObject.SetActive(value: false);
            obj.gameObject.AddComponent<TemperatureTrigger>().temperature = asset.splatterTemperature;
            obj.gameObject.SetActive(value: true);
            UnityEngine.Object.Destroy(obj.gameObject, asset.splatterLifetime - asset.splatterLifetimeSpread);
        }
        if (Dedicator.IsDedicatedServer)
        {
            if (!asset.spawnOnDedicatedServer)
            {
                return null;
            }
        }
        else if (GraphicsSettings.effectQuality == EGraphicQuality.OFF && !asset.splatterLiquid)
        {
            return null;
        }
        if (!Provider.isServer)
        {
            ClientAssetIntegrity.QueueRequest(asset);
        }
        Quaternion rotation = Quaternion.LookRotation(normal);
        if (asset.randomizeRotation)
        {
            rotation *= Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0, 360));
        }
        if (pool == null)
        {
            return null;
        }
        Transform transform = pool.Instantiate(asset.effect, point, rotation).transform;
        transform.localScale = scaleMultiplier;
        transform.parent = parent;
        if (parent != null)
        {
            RegisterAttachment(transform.gameObject);
        }
        if (asset.splatter > 0 && (!asset.gore || OptionsSettings.gore))
        {
            for (int i = 0; i < asset.splatter * ((asset.splatterLiquid || !(Player.player != null) || Player.player.skills.boost != EPlayerBoost.SPLATTERIFIC) ? 1 : 8); i++)
            {
                RaycastHit hitInfo;
                if (asset.splatterLiquid)
                {
                    float f = UnityEngine.Random.Range(0f, (float)Math.PI * 2f);
                    float num = UnityEngine.Random.Range(1f, 6f);
                    Ray ray = new Ray(point + new Vector3(Mathf.Cos(f) * num, 0f, Mathf.Sin(f) * num), Vector3.down);
                    int layerMask = 471433216;
                    Physics.Raycast(ray, out hitInfo, 8f, layerMask);
                }
                else
                {
                    Ray ray2 = new Ray(point, -2f * normal + new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)));
                    int layerMask2 = 471433216;
                    Physics.Raycast(ray2, out hitInfo, 8f, layerMask2);
                }
                if (hitInfo.transform != null)
                {
                    Transform transform2 = pool.Instantiate(asset.splatters[UnityEngine.Random.Range(0, asset.splatters.Length)], hitInfo.point + hitInfo.normal * UnityEngine.Random.Range(0.04f, 0.06f), Quaternion.LookRotation(hitInfo.normal) * Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0, 360))).transform;
                    transform2.name = "Splatter";
                    float num2 = UnityEngine.Random.Range(1f, 2f);
                    transform2.localScale = new Vector3(num2, num2, num2);
                    transform2.parent = hitInfo.transform;
                    RegisterAttachment(transform2.gameObject);
                    RegisterDebris(transform2.gameObject);
                    transform2.gameObject.SetActive(value: true);
                    if (asset.splatterLifetime > float.Epsilon)
                    {
                        pool.Destroy(transform2.gameObject, asset.splatterLifetime + UnityEngine.Random.Range(0f - asset.splatterLifetimeSpread, asset.splatterLifetimeSpread));
                    }
                    else
                    {
                        pool.Destroy(transform2.gameObject, GraphicsSettings.effect);
                    }
                }
            }
        }
        if (asset.gore)
        {
            ParticleSystem.EmissionModule emission = transform.GetComponent<ParticleSystem>().emission;
            emission.enabled = OptionsSettings.gore;
        }
        if (!asset.isStatic && transform.GetComponent<AudioSource>() != null)
        {
            transform.GetComponent<AudioSource>().pitch = UnityEngine.Random.Range(0.9f, 1.1f);
        }
        if (asset.lifetime > float.Epsilon)
        {
            pool.Destroy(transform.gameObject, asset.lifetime + UnityEngine.Random.Range(0f - asset.lifetimeSpread, asset.lifetimeSpread));
        }
        else
        {
            float num3 = 0f;
            if (transform.GetComponent<MeshRenderer>() == null)
            {
                ParticleSystem component = transform.GetComponent<ParticleSystem>();
                if (component != null)
                {
                    num3 = ((!component.main.loop) ? (component.main.duration + component.main.startLifetime.constantMax) : component.main.startLifetime.constantMax);
                }
                AudioSource component2 = transform.GetComponent<AudioSource>();
                if (component2 != null && component2.clip != null && component2.clip.length > num3)
                {
                    num3 = component2.clip.length;
                }
            }
            if (num3 < float.Epsilon)
            {
                num3 = GraphicsSettings.effect;
            }
            pool.Destroy(transform.gameObject, num3);
        }
        if (GraphicsSettings.blast && GraphicsSettings.renderMode == ERenderMode.DEFERRED)
        {
            EffectAsset effectAsset = asset.FindBlastmarkEffectAsset();
            if (effectAsset != null)
            {
                effect(effectAsset, point, new Vector3(UnityEngine.Random.Range(-0.1f, 0.1f), 1f, UnityEngine.Random.Range(-0.1f, 0.1f)));
            }
        }
        if (asset.cameraShakeRadius > 0.001f && asset.cameraShakeMagnitudeDegrees > 0.1f && Player.player != null)
        {
            Player.player.look.FlinchFromExplosion(point, asset.cameraShakeRadius, asset.cameraShakeMagnitudeDegrees);
        }
        return transform;
    }

    public static void triggerEffect(TriggerEffectParameters parameters)
    {
        if (parameters.asset == null)
        {
            return;
        }
        bool flag = parameters.asset.splatterTemperature != EPlayerTemperature.NONE || parameters.asset.spawnOnDedicatedServer;
        if (!parameters.shouldReplicate)
        {
            if (!Dedicator.IsDedicatedServer || flag)
            {
                internalSpawnEffect(parameters.asset, parameters.position, parameters.direction, parameters.scale, parameters.wasInstigatedByPlayer, null);
            }
            return;
        }
        ENetReliability reliability = ((!parameters.reliable) ? ENetReliability.Unreliable : ENetReliability.Reliable);
        bool flag2 = MathfEx.IsNearlyEqual(parameters.direction, Vector3.up);
        ITransportConnection transportConnection = parameters.relevantTransportConnection;
        if (parameters.relevantPlayerID != CSteamID.Nil)
        {
            transportConnection = Provider.findTransportConnection(parameters.relevantPlayerID);
        }
        if (transportConnection == null)
        {
            if (Dedicator.IsDedicatedServer && flag)
            {
                internalSpawnEffect(parameters.asset, parameters.position, parameters.direction, parameters.scale, parameters.wasInstigatedByPlayer, null);
            }
            PooledTransportConnectionList pooledTransportConnectionList = parameters.relevantTransportConnections;
            if (pooledTransportConnectionList == null)
            {
                float relevantDistance = parameters.relevantDistance;
                if (parameters.asset.relevantDistance > 0f)
                {
                    relevantDistance = parameters.asset.relevantDistance;
                }
                pooledTransportConnectionList = Provider.GatherClientConnectionsWithinSphere(parameters.position, relevantDistance);
            }
            if (MathfEx.IsNearlyEqual(parameters.scale, Vector3.one))
            {
                if (flag2)
                {
                    SendEffectPoint.Invoke(reliability, pooledTransportConnectionList, parameters.asset.GUID, parameters.position);
                }
                else
                {
                    SendEffectPointNormal.Invoke(reliability, pooledTransportConnectionList, parameters.asset.GUID, parameters.position, parameters.direction);
                }
            }
            else if (parameters.scale.AreComponentsNearlyEqual())
            {
                float x = parameters.scale.x;
                if (flag2)
                {
                    SendEffectPoint_UniformScale.Invoke(reliability, pooledTransportConnectionList, parameters.asset.GUID, parameters.position, x);
                }
                else
                {
                    SendEffectPointNormal_UniformScale.Invoke(reliability, pooledTransportConnectionList, parameters.asset.GUID, parameters.position, parameters.direction, x);
                }
            }
            else if (flag2)
            {
                SendEffectPoint_NonUniformScale.Invoke(reliability, pooledTransportConnectionList, parameters.asset.GUID, parameters.position, parameters.scale);
            }
            else
            {
                SendEffectPointNormal_NonUniformScale.Invoke(reliability, pooledTransportConnectionList, parameters.asset.GUID, parameters.position, parameters.direction, parameters.scale);
            }
        }
        else if (MathfEx.IsNearlyEqual(parameters.scale, Vector3.one))
        {
            if (flag2)
            {
                SendEffectPoint.Invoke(reliability, transportConnection, parameters.asset.GUID, parameters.position);
            }
            else
            {
                SendEffectPointNormal.Invoke(reliability, transportConnection, parameters.asset.GUID, parameters.position, parameters.direction);
            }
        }
        else if (parameters.scale.AreComponentsNearlyEqual())
        {
            float x2 = parameters.scale.x;
            if (flag2)
            {
                SendEffectPoint_UniformScale.Invoke(reliability, transportConnection, parameters.asset.GUID, parameters.position, x2);
            }
            else
            {
                SendEffectPointNormal_UniformScale.Invoke(reliability, transportConnection, parameters.asset.GUID, parameters.position, parameters.direction, x2);
            }
        }
        else if (flag2)
        {
            SendEffectPoint_NonUniformScale.Invoke(reliability, transportConnection, parameters.asset.GUID, parameters.position, parameters.scale);
        }
        else
        {
            SendEffectPointNormal_NonUniformScale.Invoke(reliability, transportConnection, parameters.asset.GUID, parameters.position, parameters.direction, parameters.scale);
        }
    }

    public static void RegisterDebris(GameObject item)
    {
        if (instance != null)
        {
            instance.debrisGameObjects.Add(item);
        }
    }

    private void destroyAllDebris()
    {
        foreach (GameObject debrisGameObject in debrisGameObjects)
        {
            if (debrisGameObject != null)
            {
                pool.Destroy(debrisGameObject);
            }
        }
        debrisGameObjects.Clear();
    }

    private void destroyAllUI()
    {
        foreach (UIEffectInstance uiEffectInstance in uiEffectInstances)
        {
            if (uiEffectInstance.gameObject != null)
            {
                UnityEngine.Object.Destroy(uiEffectInstance.gameObject);
            }
        }
        uiEffectInstances.Clear();
        indexedUIEffects.Clear();
    }

    private void onLevelLoaded(int level)
    {
        pool = new GameObjectPoolDictionary();
        indexedUIEffects = new Dictionary<short, GameObject>();
        attachedEffects = new Dictionary<Transform, List<GameObject>>();
        attachedEffectsListPool = new Stack<List<GameObject>>();
        debrisGameObjects.Clear();
        uiEffectInstances.Clear();
        if (Dedicator.IsDedicatedServer)
        {
            return;
        }
        Asset[] array = Assets.find(EAssetType.EFFECT);
        for (int i = 0; i < array.Length; i++)
        {
            if (!(array[i] is EffectAsset effectAsset) || effectAsset.effect == null || effectAsset.preload == 0)
            {
                continue;
            }
            pool.Instantiate(effectAsset.effect, effectAsset.id.ToString(), effectAsset.preload);
            if (effectAsset.splatter <= 0 || effectAsset.splatterPreload <= 0)
            {
                continue;
            }
            for (int j = 0; j < effectAsset.splatters.Length; j++)
            {
                if (!(effectAsset.splatters[j] == null))
                {
                    pool.Instantiate(effectAsset.splatters[j], "Splatter", effectAsset.splatterPreload);
                }
            }
        }
    }

    private void Start()
    {
        manager = this;
        CommandLogMemoryUsage.OnExecuted = (Action<List<string>>)Delegate.Combine(CommandLogMemoryUsage.OnExecuted, new Action<List<string>>(OnLogMemoryUsage));
        Level.onPrePreLevelLoaded = (PrePreLevelLoaded)Delegate.Combine(Level.onPrePreLevelLoaded, new PrePreLevelLoaded(onLevelLoaded));
    }

    private void OnLogMemoryUsage(List<string> results)
    {
        results.Add($"Effect pool assets: {pool.pools.Count}");
        int num = 0;
        int num2 = 0;
        foreach (KeyValuePair<GameObject, GameObjectPool> pool in pool.pools)
        {
            num += pool.Value.pool.Count;
            num2 += pool.Value.active.Count;
        }
        results.Add($"Inactive pooled effects: {num}");
        results.Add($"Active pooled effects: {num2}");
        results.Add($"Effect debris: {debrisGameObjects?.Count}");
        results.Add($"Attached effect parents: {attachedEffects.Count}");
        int num3 = 0;
        foreach (KeyValuePair<Transform, List<GameObject>> attachedEffect in attachedEffects)
        {
            num3 += attachedEffect.Value?.Count ?? 0;
        }
        results.Add($"Attached effect children: {num3}");
        results.Add($"Attached effect pool size: {attachedEffectsListPool.Count}");
    }

    internal static void ClearAttachments(Transform root)
    {
        if (!attachedEffects.TryGetValue(root, out var value))
        {
            return;
        }
        attachedEffects.Remove(root);
        attachedEffectsListPool.Push(value);
        foreach (GameObject item in value)
        {
            if (item != null && item.transform.root == root)
            {
                pool.Destroy(item);
            }
        }
    }

    internal static void UnregisterAttachment(GameObject effect)
    {
        Transform root = effect.transform.root;
        if (attachedEffects.TryGetValue(root, out var value))
        {
            value.RemoveFast(effect);
            if (value.Count < 1)
            {
                attachedEffects.Remove(root);
                attachedEffectsListPool.Push(value);
            }
        }
    }

    private static void RegisterAttachment(GameObject effect)
    {
        Transform root = effect.transform.root;
        if (!attachedEffects.TryGetValue(root, out var value))
        {
            if (attachedEffectsListPool.Count > 0)
            {
                value = attachedEffectsListPool.Pop();
                value.Clear();
            }
            else
            {
                value = new List<GameObject>(4);
            }
            attachedEffects.Add(root, value);
        }
        value.Add(effect);
    }
}
