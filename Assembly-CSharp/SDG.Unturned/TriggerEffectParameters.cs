using System;
using System.Collections.Generic;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Payload for the EffectManager.triggerEffect method.
/// </summary>
public struct TriggerEffectParameters
{
    /// <summary>
    /// Required effect to spawn.
    /// </summary>
    public EffectAsset asset;

    /// <summary>
    /// World-space position to spawn at.
    /// </summary>
    public Vector3 position;

    /// <summary>
    /// Local-space scale. Defaults to one.
    /// </summary>
    public Vector3 scale;

    /// <summary>
    /// If running as server should this effect be replicated to clients?
    /// Defaults to true. Set to false for code that is called on client AND server.
    /// </summary>
    public bool shouldReplicate;

    /// <summary>
    /// Should the RPC be called in reliable mode? Unreliable effects might not be received.
    /// </summary>
    public bool reliable;

    /// <summary>
    /// Was a player directly responsible for triggering this effect?
    /// For example grenade explosions are instigated by players, whereas zombie acid explosions are not.
    /// Used to prevent mod damage on the effect prefab from hurting players on PvE servers.
    /// </summary>
    public bool wasInstigatedByPlayer;

    /// <summary>
    /// Players within this radius will be sent the effect unless the effect overrides it.
    /// Defaults to 128.
    /// </summary>
    public float relevantDistance;

    /// <summary>
    /// World-space rotation for the effect.
    /// </summary>
    private Quaternion rotation;

    /// <summary>
    /// If true, rotation was specified by setter methods.
    /// Required for backwards compatibility because `direction` field is public.
    /// </summary>
    private bool wasRotationSet;

    /// <summary>
    /// Only send the effect to the given player, if set.
    /// </summary>
    internal ITransportConnection relevantTransportConnection;

    /// <summary>
    /// Only send the effect to the given players, if set.
    /// Otherwise relevantDistance is used.
    /// </summary>
    internal PooledTransportConnectionList relevantTransportConnections;

    /// <summary>
    /// Only send the effect to the given player, if set.
    /// </summary>
    [Obsolete("Please use SetRelevantPlayer instead! This field will be removed.")]
    public CSteamID relevantPlayerID;

    /// <summary>
    /// World-space direction to orient the Z axis along. Defaults to up.
    /// </summary>
    [Obsolete("Please use GetDirection and SetDirection instead now that rotation quaternion is supported. This field will be removed.")]
    public Vector3 direction;

    /// <summary>
    /// Get world-space rotation for the effect.
    /// </summary>
    public Quaternion GetRotation()
    {
        if (!wasRotationSet)
        {
            return Quaternion.LookRotation(direction);
        }
        return rotation;
    }

    /// <summary>
    /// Set world-space rotation for the effect.
    /// </summary>
    public void SetRotation(Quaternion rotation)
    {
        this.rotation = rotation;
        wasRotationSet = true;
    }

    public Vector3 GetDirection()
    {
        if (!wasRotationSet)
        {
            return direction;
        }
        return rotation * Vector3.forward;
    }

    public void SetDirection(Vector3 forward)
    {
        direction = forward;
        rotation = Quaternion.LookRotation(forward);
        wasRotationSet = true;
    }

    public void SetDirection(Vector3 forward, Vector3 upwards)
    {
        direction = forward;
        rotation = Quaternion.LookRotation(forward, upwards);
        wasRotationSet = true;
    }

    public void SetUniformScale(float scale)
    {
        this.scale = new Vector3(scale, scale, scale);
    }

    public void SetRelevantPlayer(SteamPlayer player)
    {
        relevantTransportConnection = player?.transportConnection;
    }

    public void SetRelevantPlayer(ITransportConnection transportConnection)
    {
        relevantTransportConnection = transportConnection;
    }

    public void SetRelevantTransportConnections(PooledTransportConnectionList transportConnections)
    {
        relevantTransportConnections = transportConnections;
    }

    [Obsolete("Replaced by the List overload")]
    public void SetRelevantTransportConnections(IEnumerable<ITransportConnection> transportConnections)
    {
        relevantTransportConnections = TransportConnectionListPool.Get();
        foreach (ITransportConnection transportConnection in transportConnections)
        {
            relevantTransportConnections.Add(transportConnection);
        }
    }

    public TriggerEffectParameters(EffectAsset asset)
    {
        this.asset = asset;
        position = Vector3.zero;
        direction = Vector3.up;
        scale = Vector3.one;
        shouldReplicate = true;
        reliable = false;
        wasInstigatedByPlayer = false;
        relevantDistance = 128f;
        relevantPlayerID = CSteamID.Nil;
        rotation = Quaternion.identity;
        wasRotationSet = false;
        relevantTransportConnection = null;
        relevantTransportConnections = null;
    }

    [Obsolete("Please find asset by GUID")]
    public TriggerEffectParameters(ushort id)
        : this(Assets.find(EAssetType.EFFECT, id) as EffectAsset)
    {
    }

    public TriggerEffectParameters(Guid assetGuid)
        : this(Assets.find<EffectAsset>(assetGuid))
    {
    }

    public TriggerEffectParameters(AssetReference<EffectAsset> assetRef)
        : this(assetRef.Find())
    {
    }
}
