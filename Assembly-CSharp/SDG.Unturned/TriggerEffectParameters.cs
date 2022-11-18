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

    public Vector3 direction;

    public Vector3 scale;

    public bool shouldReplicate;

    public bool reliable;

    public bool wasInstigatedByPlayer;

    public float relevantDistance;

    [Obsolete("Please use SetRelevantPlayer instead! This field will be removed.")]
    public CSteamID relevantPlayerID;

    internal ITransportConnection relevantTransportConnection;

    internal IEnumerable<ITransportConnection> relevantTransportConnections;

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

    public void SetRelevantTransportConnections(IEnumerable<ITransportConnection> transportConnections)
    {
        relevantTransportConnections = transportConnections;
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
