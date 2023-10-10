using System;
using System.Collections.Generic;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public struct TriggerEffectParameters
{
    public EffectAsset asset;

    public Vector3 position;

    public Vector3 scale;

    public bool shouldReplicate;

    public bool reliable;

    public bool wasInstigatedByPlayer;

    public float relevantDistance;

    private Quaternion rotation;

    private bool wasRotationSet;

    internal ITransportConnection relevantTransportConnection;

    internal PooledTransportConnectionList relevantTransportConnections;

    [Obsolete("Please use SetRelevantPlayer instead! This field will be removed.")]
    public CSteamID relevantPlayerID;

    [Obsolete("Please use GetDirection and SetDirection instead now that rotation quaternion is supported. This field will be removed.")]
    public Vector3 direction;

    public Quaternion GetRotation()
    {
        if (!wasRotationSet)
        {
            return Quaternion.LookRotation(direction);
        }
        return rotation;
    }

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
