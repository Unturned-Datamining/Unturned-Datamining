using System;
using System.Collections.Generic;
using System.Reflection;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class SteamChannel : MonoBehaviour
{
    /// <summary>
    /// If changing header size remember to update PlayerManager and allocPlayerChannelId.
    /// </summary>
    public const int CHANNEL_ID_HEADER_SIZE = 1;

    public const int RPC_HEADER_SIZE = 2;

    [Obsolete]
    public const int VOICE_HEADER_SIZE = 3;

    /// <summary>
    /// How far to shift compressed voice data.
    /// </summary>
    [Obsolete]
    public const int VOICE_DATA_OFFSET = 6;

    public int id;

    public SteamPlayer owner;

    /// <summary>
    /// Don't use this. Originally added so that Rocketmod didn't have to inject into the game's assembly.
    /// </summary>
    [Obsolete("Will be deprecated soon. Please discuss on the issue tracker and we will find an alternative.")]
    public static TriggerReceive onTriggerReceive;

    private static bool warnedAboutTriggerReceive;

    /// <summary>
    /// Don't use this. Originally added so that Rocketmod didn't have to inject into the game's assembly.
    /// </summary>
    [Obsolete("Will be deprecated soon. Please discuss on the issue tracker and we will find an alternative.")]
    public static TriggerSend onTriggerSend;

    private static bool warnedAboutTriggerSend;

    /// <summary>
    /// Does array of RPCs need to be rebuilt?
    /// </summary>
    private bool callArrayDirty = true;

    private static List<SteamChannelMethod> workingCalls = new List<SteamChannelMethod>();

    private static List<Component> workingComponents = new List<Component>();

    [Obsolete("Renamed to IsLocalPlayer")]
    public bool isOwner;

    public SteamChannelMethod[] calls { get; protected set; }

    /// <summary>
    /// If true, this object is owned by a locally-controlled player.
    /// For example, some code is not run for "remote" players.
    /// Always true in singleplayer. Always false on dedicated server.
    /// </summary>
    public bool IsLocalPlayer
    {
        get
        {
            return isOwner;
        }
        internal set
        {
            isOwner = value;
        }
    }

    [Obsolete]
    public bool longBinaryData
    {
        get
        {
            return SteamPacker.longBinaryData;
        }
        set
        {
            SteamPacker.longBinaryData = value;
        }
    }

    /// <summary>
    /// Use on server when invoking client methods on the owning player.
    /// </summary>
    public ITransportConnection GetOwnerTransportConnection()
    {
        return owner?.transportConnection;
    }

    [Obsolete]
    public bool checkServer(CSteamID steamID)
    {
        return steamID == Provider.server;
    }

    [Obsolete]
    public bool checkOwner(CSteamID steamID)
    {
        if (owner == null)
        {
            return false;
        }
        return steamID == owner.playerID.steamID;
    }

    /// <summary>
    /// Replacement for ESteamCall.NOT_OWNER.
    /// </summary>
    public PooledTransportConnectionList GatherRemoteClientConnectionsExcludingOwner()
    {
        PooledTransportConnectionList pooledTransportConnectionList = TransportConnectionListPool.Get();
        foreach (SteamPlayer client in Provider.clients)
        {
            if (!client.IsLocalServerHost && client != owner)
            {
                pooledTransportConnectionList.Add(client.transportConnection);
            }
        }
        return pooledTransportConnectionList;
    }

    [Obsolete("Replaced by GatherRemoteClientConnectionsExcludingOwner")]
    public IEnumerable<ITransportConnection> EnumerateClients_RemoteNotOwner()
    {
        return GatherRemoteClientConnectionsExcludingOwner();
    }

    public PooledTransportConnectionList GatherRemoteClientConnectionsWithinSphereExcludingOwner(Vector3 position, float radius)
    {
        PooledTransportConnectionList pooledTransportConnectionList = TransportConnectionListPool.Get();
        float num = radius * radius;
        foreach (SteamPlayer client in Provider.clients)
        {
            if (!client.IsLocalServerHost && client != owner && client.player != null && (client.player.transform.position - position).sqrMagnitude < num)
            {
                pooledTransportConnectionList.Add(client.transportConnection);
            }
        }
        return pooledTransportConnectionList;
    }

    [Obsolete("Replaced by GatherRemoteClientConnectionsWithinSphereExcludingOwner")]
    public IEnumerable<ITransportConnection> EnumerateClients_RemoteNotOwnerWithinSphere(Vector3 position, float radius)
    {
        return GatherRemoteClientConnectionsWithinSphereExcludingOwner(position, radius);
    }

    public PooledTransportConnectionList GatherOwnerAndClientConnectionsWithinSphere(Vector3 position, float radius)
    {
        PooledTransportConnectionList pooledTransportConnectionList = TransportConnectionListPool.Get();
        float num = radius * radius;
        foreach (SteamPlayer client in Provider.clients)
        {
            if (client == owner || (client.player != null && (client.player.transform.position - position).sqrMagnitude < num))
            {
                pooledTransportConnectionList.Add(client.transportConnection);
            }
        }
        return pooledTransportConnectionList;
    }

    [Obsolete("Replaced by GatherOwnerAndClientConnectionsWithinSphere")]
    public IEnumerable<ITransportConnection> EnumerateClients_WithinSphereOrOwner(Vector3 position, float radius)
    {
        return GatherOwnerAndClientConnectionsWithinSphere(position, radius);
    }

    /// <returns>True if the call succeeded, or false if the sender should be refused.</returns>
    [Obsolete]
    public bool receive(CSteamID steamID, byte[] packet, int offset, int size)
    {
        if (onTriggerReceive != null)
        {
            if (!warnedAboutTriggerReceive)
            {
                warnedAboutTriggerReceive = true;
                CommandWindow.LogError("Plugin(s) using unsafe onTriggerReceive which will be deprecated soon.");
            }
            try
            {
                byte[] array = packet;
                if (Provider.useConstNetEvents)
                {
                    array = new byte[offset + size];
                    Array.Copy(packet, array, array.Length);
                }
                onTriggerReceive(this, steamID, array, offset, size);
                if (Provider.useConstNetEvents && Provider.hasNetBufferChanged(packet, array, offset, size))
                {
                    CommandWindow.LogError("Plugin(s) modified buffer during onTriggerReceive!");
                }
            }
            catch (Exception e)
            {
                UnturnedLog.warn("Plugin raised an exception from SteamChannel.onTriggerReceive:");
                UnturnedLog.exception(e);
            }
        }
        if (size < 3)
        {
            return true;
        }
        int num = packet[offset + 1];
        buildCallArrayIfDirty();
        if (num < 0 || num >= calls.Length)
        {
            return true;
        }
        _ = packet[offset];
        bool flag;
        switch (calls[num].attribute.validation)
        {
        case ESteamCallValidation.NONE:
            flag = true;
            break;
        case ESteamCallValidation.ONLY_FROM_SERVER:
            flag = steamID == Provider.server;
            break;
        case ESteamCallValidation.SERVERSIDE:
            flag = Provider.isServer;
            break;
        case ESteamCallValidation.ONLY_FROM_OWNER:
            flag = owner != null && steamID == owner.playerID.steamID;
            break;
        default:
            flag = false;
            UnturnedLog.warn("Unhandled RPC validation type on method: " + calls[num].method.Name);
            break;
        }
        if (!flag)
        {
            return true;
        }
        if (calls[num].attribute.rateLimitIndex >= 0)
        {
            string text = calls[num].method.Name;
            SteamPlayer steamPlayer = PlayerTool.getSteamPlayer(steamID);
            if (steamPlayer == null)
            {
                UnturnedLog.info("RPC " + text + " on channel " + id + " called without player sender, so we're ignoring it");
                return true;
            }
            float realtimeSinceStartup = Time.realtimeSinceStartup;
            float num2 = steamPlayer.rpcAllowedTimes[calls[num].attribute.rateLimitIndex];
            if (realtimeSinceStartup < num2)
            {
                return true;
            }
            steamPlayer.rpcAllowedTimes[calls[num].attribute.rateLimitIndex] = realtimeSinceStartup + calls[num].attribute.ratelimitSeconds;
        }
        try
        {
            if (calls[num].types.Length != 0)
            {
                object[] objectsForLegacyRPC = SteamPacker.getObjectsForLegacyRPC(offset, 3, size, packet, calls[num].types, calls[num].typesReadOffset);
                switch (calls[num].contextType)
                {
                case SteamChannelMethod.EContextType.Client:
                    objectsForLegacyRPC[calls[num].contextParameterIndex] = default(ClientInvocationContext);
                    break;
                case SteamChannelMethod.EContextType.Server:
                    objectsForLegacyRPC[calls[num].contextParameterIndex] = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
                    break;
                }
                if (calls[num].method.IsStatic)
                {
                    calls[num].method.Invoke(null, objectsForLegacyRPC);
                }
                else
                {
                    calls[num].method.Invoke(calls[num].component, objectsForLegacyRPC);
                }
            }
            else
            {
                calls[num].method.Invoke(calls[num].component, null);
            }
        }
        catch (Exception e2)
        {
            UnturnedLog.info("Exception raised when RPC invoked {0}:", calls[num].method.Name);
            UnturnedLog.exception(e2);
        }
        return true;
    }

    [Obsolete]
    public object read(Type type)
    {
        return SteamPacker.read(type);
    }

    [Obsolete]
    public object[] read(Type type_0, Type type_1, Type type_2)
    {
        return SteamPacker.read(type_0, type_1, type_2);
    }

    [Obsolete]
    public object[] read(Type type_0, Type type_1, Type type_2, Type type_3)
    {
        return SteamPacker.read(type_0, type_1, type_2, type_3);
    }

    [Obsolete]
    public object[] read(Type type_0, Type type_1, Type type_2, Type type_3, Type type_4, Type type_5)
    {
        return SteamPacker.read(type_0, type_1, type_2, type_3, type_4, type_5);
    }

    [Obsolete]
    public object[] read(Type type_0, Type type_1, Type type_2, Type type_3, Type type_4, Type type_5, Type type_6)
    {
        return SteamPacker.read(type_0, type_1, type_2, type_3, type_4, type_5, type_6);
    }

    [Obsolete]
    public object[] read(params Type[] types)
    {
        return SteamPacker.read(types);
    }

    [Obsolete]
    public void write(object objects)
    {
    }

    [Obsolete]
    public void write(object object_0, object object_1, object object_2)
    {
    }

    [Obsolete]
    public void write(object object_0, object object_1, object object_2, object object_3)
    {
    }

    [Obsolete]
    public void write(object object_0, object object_1, object object_2, object object_3, object object_4, object object_5)
    {
    }

    [Obsolete]
    public void write(object object_0, object object_1, object object_2, object object_3, object object_4, object object_5, object object_6)
    {
    }

    [Obsolete]
    public void write(params object[] objects)
    {
    }

    [Obsolete]
    public void openWrite()
    {
    }

    [Obsolete]
    public void closeWrite(string name, CSteamID steamID, ESteamPacket type)
    {
    }

    [Obsolete]
    public void closeWrite(string name, ESteamCall mode, byte bound, ESteamPacket type)
    {
    }

    [Obsolete]
    public void closeWrite(string name, ESteamCall mode, byte x, byte y, byte area, ESteamPacket type)
    {
    }

    [Obsolete]
    public void closeWrite(string name, ESteamCall mode, ESteamPacket type)
    {
    }

    [Obsolete]
    public void send(string name, CSteamID steamID, ESteamPacket type, params object[] arguments)
    {
        int call = getCall(name);
        if (call != -1)
        {
            getPacket(type, call, out var size, out var packet, arguments);
            if (IsLocalPlayer && steamID == Provider.client)
            {
                receive(Provider.client, packet, 0, size);
            }
            else if (Provider.isServer && steamID == Provider.server)
            {
                receive(Provider.server, packet, 0, size);
            }
            else
            {
                Provider.send(steamID, type, packet, size, 0);
            }
        }
    }

    [Obsolete]
    public void sendAside(string name, CSteamID steamID, ESteamPacket type, params object[] arguments)
    {
    }

    [Obsolete]
    public void send(ESteamCall mode, byte bound, ESteamPacket type, int size, byte[] packet)
    {
        switch (mode)
        {
        case ESteamCall.SERVER:
            if (Provider.isServer)
            {
                receive(Provider.server, packet, 0, size);
                break;
            }
            throw new NotSupportedException();
        case ESteamCall.ALL:
        {
            if (!Provider.isServer)
            {
                throw new NotSupportedException();
            }
            for (int j = 0; j < Provider.clients.Count; j++)
            {
                if (Provider.clients[j].playerID.steamID != Provider.client && Provider.clients[j].player != null && Provider.clients[j].player.movement.bound == bound)
                {
                    Provider.sendToClient(Provider.clients[j].transportConnection, type, packet, size);
                }
            }
            if (Provider.isServer)
            {
                receive(Provider.server, packet, 0, size);
            }
            else
            {
                receive(Provider.client, packet, 0, size);
            }
            break;
        }
        case ESteamCall.OTHERS:
        {
            if (!Provider.isServer)
            {
                throw new NotSupportedException();
            }
            for (int k = 0; k < Provider.clients.Count; k++)
            {
                if (Provider.clients[k].playerID.steamID != Provider.client && Provider.clients[k].player != null && Provider.clients[k].player.movement.bound == bound)
                {
                    Provider.sendToClient(Provider.clients[k].transportConnection, type, packet, size);
                }
            }
            break;
        }
        case ESteamCall.OWNER:
            if (IsLocalPlayer)
            {
                receive(owner.playerID.steamID, packet, 0, size);
            }
            else
            {
                Provider.sendToClient(owner.transportConnection, type, packet, size);
            }
            break;
        case ESteamCall.NOT_OWNER:
        {
            if (!Provider.isServer)
            {
                throw new NotSupportedException();
            }
            for (int l = 0; l < Provider.clients.Count; l++)
            {
                if (Provider.clients[l].playerID.steamID != owner.playerID.steamID && Provider.clients[l].player != null && Provider.clients[l].player.movement.bound == bound)
                {
                    Provider.sendToClient(Provider.clients[l].transportConnection, type, packet, size);
                }
            }
            break;
        }
        case ESteamCall.CLIENTS:
        {
            for (int i = 0; i < Provider.clients.Count; i++)
            {
                if (Provider.clients[i].playerID.steamID != Provider.client && Provider.clients[i].player != null && Provider.clients[i].player.movement.bound == bound)
                {
                    Provider.sendToClient(Provider.clients[i].transportConnection, type, packet, size);
                }
            }
            if (Provider.isClient)
            {
                receive(Provider.client, packet, 0, size);
            }
            break;
        }
        }
    }

    [Obsolete]
    public void send(string name, ESteamCall mode, byte bound, ESteamPacket type, params object[] arguments)
    {
        int call = getCall(name);
        if (call != -1)
        {
            getPacket(type, call, out var size, out var packet, arguments);
            send(mode, bound, type, size, packet);
        }
    }

    [Obsolete]
    public void send(ESteamCall mode, byte x, byte y, byte area, ESteamPacket type, int size, byte[] packet)
    {
        switch (mode)
        {
        case ESteamCall.SERVER:
            if (Provider.isServer)
            {
                receive(Provider.server, packet, 0, size);
                break;
            }
            throw new NotSupportedException();
        case ESteamCall.ALL:
        {
            if (!Provider.isServer)
            {
                throw new NotSupportedException();
            }
            for (int j = 0; j < Provider.clients.Count; j++)
            {
                if (Provider.clients[j].playerID.steamID != Provider.client && Provider.clients[j].player != null && Regions.checkArea(x, y, Provider.clients[j].player.movement.region_x, Provider.clients[j].player.movement.region_y, area))
                {
                    Provider.sendToClient(Provider.clients[j].transportConnection, type, packet, size);
                }
            }
            if (Provider.isServer)
            {
                receive(Provider.server, packet, 0, size);
            }
            else
            {
                receive(Provider.client, packet, 0, size);
            }
            break;
        }
        case ESteamCall.OTHERS:
        {
            if (!Provider.isServer)
            {
                throw new NotSupportedException();
            }
            for (int k = 0; k < Provider.clients.Count; k++)
            {
                if (Provider.clients[k].playerID.steamID != Provider.client && Provider.clients[k].player != null && Regions.checkArea(x, y, Provider.clients[k].player.movement.region_x, Provider.clients[k].player.movement.region_y, area))
                {
                    Provider.sendToClient(Provider.clients[k].transportConnection, type, packet, size);
                }
            }
            break;
        }
        case ESteamCall.OWNER:
            if (IsLocalPlayer)
            {
                receive(owner.playerID.steamID, packet, 0, size);
            }
            else
            {
                Provider.sendToClient(owner.transportConnection, type, packet, size);
            }
            break;
        case ESteamCall.NOT_OWNER:
        {
            if (!Provider.isServer)
            {
                throw new NotSupportedException();
            }
            for (int l = 0; l < Provider.clients.Count; l++)
            {
                if (Provider.clients[l].playerID.steamID != owner.playerID.steamID && Provider.clients[l].player != null && Regions.checkArea(x, y, Provider.clients[l].player.movement.region_x, Provider.clients[l].player.movement.region_y, area))
                {
                    Provider.sendToClient(Provider.clients[l].transportConnection, type, packet, size);
                }
            }
            break;
        }
        case ESteamCall.CLIENTS:
        {
            for (int i = 0; i < Provider.clients.Count; i++)
            {
                if (Provider.clients[i].playerID.steamID != Provider.client && Provider.clients[i].player != null && Regions.checkArea(x, y, Provider.clients[i].player.movement.region_x, Provider.clients[i].player.movement.region_y, area))
                {
                    Provider.sendToClient(Provider.clients[i].transportConnection, type, packet, size);
                }
            }
            if (Provider.isClient)
            {
                receive(Provider.client, packet, 0, size);
            }
            break;
        }
        }
    }

    [Obsolete]
    public void send(string name, ESteamCall mode, byte x, byte y, byte area, ESteamPacket type, params object[] arguments)
    {
        int call = getCall(name);
        if (call != -1)
        {
            getPacket(type, call, out var size, out var packet, arguments);
            send(mode, x, y, area, type, size, packet);
        }
    }

    [Obsolete]
    public void send(ESteamCall mode, ESteamPacket type, int size, byte[] packet)
    {
        switch (mode)
        {
        case ESteamCall.SERVER:
            if (Provider.isServer)
            {
                receive(Provider.server, packet, 0, size);
                break;
            }
            throw new NotSupportedException();
        case ESteamCall.ALL:
        {
            if (!Provider.isServer)
            {
                throw new NotSupportedException();
            }
            for (int j = 0; j < Provider.clients.Count; j++)
            {
                if (Provider.clients[j].playerID.steamID != Provider.client)
                {
                    Provider.sendToClient(Provider.clients[j].transportConnection, type, packet, size);
                }
            }
            if (Provider.isServer)
            {
                receive(Provider.server, packet, 0, size);
            }
            else
            {
                receive(Provider.client, packet, 0, size);
            }
            break;
        }
        case ESteamCall.OTHERS:
        {
            if (!Provider.isServer)
            {
                throw new NotSupportedException();
            }
            for (int k = 0; k < Provider.clients.Count; k++)
            {
                if (Provider.clients[k].playerID.steamID != Provider.client)
                {
                    Provider.sendToClient(Provider.clients[k].transportConnection, type, packet, size);
                }
            }
            break;
        }
        case ESteamCall.OWNER:
            if (IsLocalPlayer)
            {
                receive(owner.playerID.steamID, packet, 0, size);
            }
            else
            {
                Provider.sendToClient(owner.transportConnection, type, packet, size);
            }
            break;
        case ESteamCall.NOT_OWNER:
        {
            if (!Provider.isServer)
            {
                throw new NotSupportedException();
            }
            for (int l = 0; l < Provider.clients.Count; l++)
            {
                if (Provider.clients[l].playerID.steamID != owner.playerID.steamID)
                {
                    Provider.sendToClient(Provider.clients[l].transportConnection, type, packet, size);
                }
            }
            break;
        }
        case ESteamCall.CLIENTS:
        {
            for (int i = 0; i < Provider.clients.Count; i++)
            {
                if (Provider.clients[i].playerID.steamID != Provider.client)
                {
                    Provider.sendToClient(Provider.clients[i].transportConnection, type, packet, size);
                }
            }
            if (Provider.isClient)
            {
                receive(Provider.client, packet, 0, size);
            }
            break;
        }
        }
    }

    [Obsolete]
    public void send(string name, ESteamCall mode, ESteamPacket type, params object[] arguments)
    {
        if (onTriggerSend != null)
        {
            if (!warnedAboutTriggerSend)
            {
                warnedAboutTriggerSend = true;
                CommandWindow.LogError("Plugin(s) using unsafe onTriggerSend which will be deprecated soon.");
            }
            try
            {
                onTriggerSend(owner, name, mode, type, arguments);
            }
            catch (Exception e)
            {
                UnturnedLog.warn("Plugin raised an exception from SteamChannel.onTriggerSend:");
                UnturnedLog.exception(e);
            }
        }
        int call = getCall(name);
        if (call != -1)
        {
            getPacket(type, call, out var size, out var packet, arguments);
            send(mode, type, size, packet);
        }
    }

    [Obsolete]
    public void send(ESteamCall mode, Vector3 point, float radius, ESteamPacket type, int size, byte[] packet)
    {
        radius *= radius;
        switch (mode)
        {
        case ESteamCall.SERVER:
            if (Provider.isServer)
            {
                receive(Provider.server, packet, 0, size);
                break;
            }
            throw new NotSupportedException();
        case ESteamCall.ALL:
        {
            if (!Provider.isServer)
            {
                throw new NotSupportedException();
            }
            for (int j = 0; j < Provider.clients.Count; j++)
            {
                if (Provider.clients[j].playerID.steamID != Provider.client && Provider.clients[j].player != null && (Provider.clients[j].player.transform.position - point).sqrMagnitude < radius)
                {
                    Provider.sendToClient(Provider.clients[j].transportConnection, type, packet, size);
                }
            }
            if (Provider.isServer)
            {
                receive(Provider.server, packet, 0, size);
            }
            else
            {
                receive(Provider.client, packet, 0, size);
            }
            break;
        }
        case ESteamCall.OTHERS:
        {
            if (!Provider.isServer)
            {
                throw new NotSupportedException();
            }
            for (int k = 0; k < Provider.clients.Count; k++)
            {
                if (Provider.clients[k].playerID.steamID != Provider.client && Provider.clients[k].player != null && (Provider.clients[k].player.transform.position - point).sqrMagnitude < radius)
                {
                    Provider.sendToClient(Provider.clients[k].transportConnection, type, packet, size);
                }
            }
            break;
        }
        case ESteamCall.OWNER:
            if (IsLocalPlayer)
            {
                receive(owner.playerID.steamID, packet, 0, size);
            }
            else
            {
                Provider.sendToClient(owner.transportConnection, type, packet, size);
            }
            break;
        case ESteamCall.NOT_OWNER:
        {
            if (!Provider.isServer)
            {
                throw new NotSupportedException();
            }
            for (int l = 0; l < Provider.clients.Count; l++)
            {
                if (Provider.clients[l].playerID.steamID != owner.playerID.steamID && Provider.clients[l].player != null && (Provider.clients[l].player.transform.position - point).sqrMagnitude < radius)
                {
                    Provider.sendToClient(Provider.clients[l].transportConnection, type, packet, size);
                }
            }
            break;
        }
        case ESteamCall.CLIENTS:
        {
            for (int i = 0; i < Provider.clients.Count; i++)
            {
                if (Provider.clients[i].playerID.steamID != Provider.client && Provider.clients[i].player != null && (Provider.clients[i].player.transform.position - point).sqrMagnitude < radius)
                {
                    Provider.sendToClient(Provider.clients[i].transportConnection, type, packet, size);
                }
            }
            if (Provider.isClient)
            {
                receive(Provider.client, packet, 0, size);
            }
            break;
        }
        }
    }

    [Obsolete]
    public void send(string name, ESteamCall mode, Vector3 point, float radius, ESteamPacket type, params object[] arguments)
    {
        int call = getCall(name);
        if (call != -1)
        {
            getPacket(type, call, out var size, out var packet, arguments);
            send(mode, point, radius, type, size, packet);
        }
    }

    /// <summary>
    /// Calls array needs rebuilding the next time it is used.
    /// Should be invoked when adding/removing components with RPCs.
    /// </summary>
    public void markDirty()
    {
        callArrayDirty = true;
    }

    /// <summary>
    /// Find methods with SteamCall attribute, and gather them into an array.
    /// </summary>
    private void buildCallArray()
    {
        workingCalls.Clear();
        workingComponents.Clear();
        GetComponents(workingComponents);
        foreach (Component workingComponent in workingComponents)
        {
            if ((workingComponent.hideFlags & HideFlags.NotEditable) == HideFlags.NotEditable)
            {
                continue;
            }
            MemberInfo[] methods = workingComponent.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
            methods = methods;
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo methodInfo = (MethodInfo)methods[i];
                SteamCall customAttribute = methodInfo.GetCustomAttribute<SteamCall>();
                if (customAttribute == null)
                {
                    continue;
                }
                string legacyName = customAttribute.legacyName;
                if (string.IsNullOrEmpty(legacyName))
                {
                    legacyName = methodInfo.Name;
                }
                ParameterInfo[] parameters = methodInfo.GetParameters();
                Type[] array = new Type[parameters.Length];
                for (int j = 0; j < parameters.Length; j++)
                {
                    array[j] = parameters[j].ParameterType;
                }
                int num = 0;
                SteamChannelMethod.EContextType contextType = SteamChannelMethod.EContextType.None;
                int contextParameterIndex = -1;
                if (num < array.Length)
                {
                    if (array[num].GetElementType() == typeof(ClientInvocationContext))
                    {
                        contextParameterIndex = num;
                        num++;
                        contextType = SteamChannelMethod.EContextType.Client;
                    }
                    else if (array[num].GetElementType() == typeof(ServerInvocationContext))
                    {
                        contextParameterIndex = num;
                        num++;
                        contextType = SteamChannelMethod.EContextType.Server;
                    }
                }
                if (customAttribute.ratelimitHz > 0)
                {
                    customAttribute.ratelimitSeconds = 1f / (float)customAttribute.ratelimitHz;
                    ServerMethodInfo serverMethodInfo = NetReflection.GetServerMethodInfo(methodInfo.DeclaringType, methodInfo.Name);
                    if (serverMethodInfo != null)
                    {
                        customAttribute.rateLimitIndex = serverMethodInfo.rateLimitIndex;
                    }
                }
                workingCalls.Add(new SteamChannelMethod(workingComponent, methodInfo, legacyName, array, num, contextType, contextParameterIndex, customAttribute));
            }
        }
        calls = workingCalls.ToArray();
        if (calls.Length > 235)
        {
            CommandWindow.LogError(base.name + " approaching 255 methods!");
        }
    }

    private void buildCallArrayIfDirty()
    {
        if (callArrayDirty)
        {
            callArrayDirty = false;
            buildCallArray();
        }
    }

    public void setup()
    {
        Provider.openChannel(this);
    }

    private void encodeChannelId(byte[] packet)
    {
        packet[2] = (byte)((uint)id & 0xFFu);
    }

    [Obsolete]
    public void getPacket(ESteamPacket type, int index, out int size, out byte[] packet)
    {
        packet = SteamPacker.closeWrite(out size);
        packet[0] = (byte)type;
        packet[1] = (byte)index;
        encodeChannelId(packet);
    }

    /// <summary>
    /// Encode byte array of voice data to send.
    /// </summary>
    [Obsolete]
    public void encodeVoicePacket(byte callIndex, out int size, out byte[] packet, byte[] bytes, ushort length, bool usingWalkieTalkie)
    {
        size = 0;
        packet = null;
    }

    /// <summary>
    /// Decode voice parameters from byte array.
    /// </summary>
    [Obsolete]
    public void decodeVoicePacket(byte[] packet, out uint compressedSize, out bool usingWalkieTalkie)
    {
        compressedSize = 0u;
        usingWalkieTalkie = false;
    }

    [Obsolete]
    public void sendVoicePacket(SteamPlayer player, byte[] packet, int packetSize)
    {
    }

    [Obsolete]
    public void getPacket(ESteamPacket type, int index, out int size, out byte[] packet, params object[] arguments)
    {
        packet = SteamPacker.getBytes(3, out size, arguments);
        packet[0] = (byte)type;
        packet[1] = (byte)index;
        encodeChannelId(packet);
    }

    [Obsolete]
    public int getCall(string name)
    {
        buildCallArrayIfDirty();
        for (int i = 0; i < calls.Length; i++)
        {
            if (calls[i].legacyMethodName == name)
            {
                return i;
            }
        }
        CommandWindow.LogError("Failed to find a method named: " + name);
        return -1;
    }

    private void OnDestroy()
    {
        if (id != 0)
        {
            Provider.closeChannel(this);
        }
    }
}
