using System;
using System.Collections.Generic;
using SDG.NetPak;
using SDG.NetTransport;
using UnityEngine;

namespace SDG.Unturned;

internal static class ClientAssetIntegrity
{
    private static float timer;

    private static HashSet<Guid> validatedGuids = new HashSet<Guid>();

    internal static HashSet<Guid> serverKnownMissingGuids = new HashSet<Guid>();

    private static List<KeyValuePair<Guid, Asset>> pendingValidation = new List<KeyValuePair<Guid, Asset>>();

    /// <summary>
    /// Reset prior to joining a new server.
    /// </summary>
    public static void Clear()
    {
        timer = 0f;
        validatedGuids.Clear();
        serverKnownMissingGuids.Clear();
        pendingValidation.Clear();
    }

    /// <summary>
    /// By default if the client submits an asset guid which the server cannot find an asset for the client will
    /// be kicked. This is necessary to prevent cheaters from spamming huge numbers of random guids. In certain cases
    /// like a terrain material missing the server knows the client will be missing it as well, and can register
    /// it here to prevent the client from being kicked unnecessarily.
    /// </summary>
    public static void ServerAddKnownMissingAsset(Guid guid, string context)
    {
        if (guid != Guid.Empty && serverKnownMissingGuids.Add(guid))
        {
            UnturnedLog.info($"Context \"{context}\" known missing asset {guid:N}, server will not kick clients for this");
        }
    }

    /// <summary>
    /// Send asset hash (or lack thereof) to server.
    ///
    /// IMPORTANT: should only be called in cases where the server has verified the asset exists by loading it,
    /// otherwise only if the asset exists on the client. This is because the server kicks if the asset does not
    /// exist in order to prevent hacked clients from spamming requests. Context parameter is intended to help
    /// narrow down cases where this rule is being broken.
    /// </summary>
    public static void QueueRequest(Guid guid, Asset asset, string context)
    {
        if (!(guid == Guid.Empty) && validatedGuids.Add(guid))
        {
            if (asset == null)
            {
                UnturnedLog.warn($"Context \"{context}\" missing asset {guid:N}");
            }
            pendingValidation.Add(new KeyValuePair<Guid, Asset>(guid, asset));
        }
    }

    /// <summary>
    /// Send asset hash to server.
    /// Used in cases where server does not verify asset exists. (see other method's comment)
    /// </summary>
    public static void QueueRequest(Asset asset)
    {
        if (asset != null)
        {
            QueueRequest(asset.GUID, asset, null);
        }
    }

    /// <summary>
    /// Called each Update on the client.
    /// </summary>
    public static void SendRequests()
    {
        if (pendingValidation.Count < 1)
        {
            return;
        }
        timer += Time.unscaledDeltaTime;
        if (!(timer > 0.1f))
        {
            return;
        }
        timer = 0f;
        NetMessages.SendMessageToServer(EServerMessage.ValidateAssets, ENetReliability.Reliable, delegate(NetPakWriter writer)
        {
            int count = pendingValidation.Count;
            int b = (int)(ServerMessageHandler_ValidateAssets.MAX_ASSETS.value + 1);
            int num = Mathf.Min(count, b);
            int num2 = count - num;
            uint value = (uint)(num - 1);
            writer.WriteBits(value, ServerMessageHandler_ValidateAssets.MAX_ASSETS.bitCount);
            uint num3 = 0u;
            int num4 = 0;
            int num5 = count - 1;
            while (num5 >= num2)
            {
                KeyValuePair<Guid, Asset> keyValuePair = pendingValidation[num5];
                if (keyValuePair.Value != null && keyValuePair.Value.hash != null && keyValuePair.Value.hash.Length == 20)
                {
                    num3 |= (uint)(1 << num4);
                }
                num5--;
                num4++;
            }
            writer.WriteBits(num3, num);
            num4 = 0;
            int num6 = count - 1;
            while (num6 >= num2)
            {
                KeyValuePair<Guid, Asset> keyValuePair2 = pendingValidation[num6];
                Guid key = keyValuePair2.Key;
                writer.WriteGuid(key);
                if ((num3 & (uint)(1 << num4)) != 0)
                {
                    Asset value2 = keyValuePair2.Value;
                    if (value2.originMasterBundle != null && value2.originMasterBundle.doesHashFileExist && value2.originMasterBundle.hash != null && value2.originMasterBundle.hash.Length == 20)
                    {
                        byte[] bytes = Hash.combine(value2.hash, value2.originMasterBundle.hash);
                        writer.WriteBytes(bytes);
                    }
                    else
                    {
                        writer.WriteBytes(value2.hash);
                    }
                }
                num6--;
                num4++;
            }
            pendingValidation.RemoveRange(num2, num);
        });
    }
}
