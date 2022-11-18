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

    public static void Clear()
    {
        timer = 0f;
        validatedGuids.Clear();
        serverKnownMissingGuids.Clear();
        pendingValidation.Clear();
    }

    public static void ServerAddKnownMissingAsset(Guid guid, string context)
    {
        if (guid != Guid.Empty && serverKnownMissingGuids.Add(guid))
        {
            UnturnedLog.info("Context \"" + context + "\" known missing asset " + guid.ToString("N") + ", server will not kick clients for this");
        }
    }

    public static void QueueRequest(Guid guid, Asset asset, string context)
    {
        if (!(guid == Guid.Empty) && validatedGuids.Add(guid))
        {
            if (asset == null)
            {
                UnturnedLog.warn("Context \"" + context + "\" missing asset " + guid.ToString("N"));
            }
            pendingValidation.Add(new KeyValuePair<Guid, Asset>(guid, asset));
        }
    }

    public static void QueueRequest(Asset asset)
    {
        if (asset != null)
        {
            QueueRequest(asset.GUID, asset, null);
        }
    }

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
                writer.WriteGuid(keyValuePair2.Key);
                if ((num3 & (uint)(1 << num4)) != 0)
                {
                    Asset value2 = keyValuePair2.Value;
                    if (value2.originMasterBundle != null && value2.originMasterBundle.doesHashFileExist && value2.originMasterBundle.hash != null && value2.originMasterBundle.hash.Length == 20)
                    {
                        writer.WriteBytes(Hash.combine(value2.hash, value2.originMasterBundle.hash));
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
