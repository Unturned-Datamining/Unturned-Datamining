using System;
using System.IO;
using System.Runtime.InteropServices;
using BattlEye;
using SDG.NetPak;
using Steamworks;

namespace SDG.Unturned;

internal static class ClientMessageHandler_Accepted
{
    internal static event System.Action OnGameplayConfigReceived;

    internal static void ReadMessage(NetPakReader reader)
    {
        Provider.isWaitingForAuthenticationResponse = false;
        reader.ReadUInt32(out var value);
        reader.ReadUInt16(out var value2);
        UnturnedLog.info("Accepted by server");
        if (Provider.currentServerInfo != null && Provider.currentServerInfo.IsBattlEyeSecure)
        {
            string text = ReadWrite.PATH + "/BattlEye/BEClient_x64.so";
            if (!File.Exists(text))
            {
                text = ReadWrite.PATH + "/BattlEye/BEClient.so";
            }
            if (!File.Exists(text))
            {
                Provider._connectionFailureInfo = ESteamConnectionFailureInfo.KICKED;
                Provider._connectionFailureReason = "Missing BattlEye client library! (" + text + ")";
                UnturnedLog.error(Provider.connectionFailureReason);
                Provider.RequestDisconnect("BattlEye missing");
                return;
            }
            UnturnedLog.info("Loading BattlEye client library from: " + text);
            try
            {
                Provider.battlEyeClientHandle = BEClient.dlopen(text, 2);
                if (!(Provider.battlEyeClientHandle != IntPtr.Zero))
                {
                    Provider._connectionFailureInfo = ESteamConnectionFailureInfo.KICKED;
                    Provider._connectionFailureReason = "Failed to load BattlEye client library!";
                    UnturnedLog.error(Provider.connectionFailureReason);
                    Provider.RequestDisconnect("BattlEye load error");
                    return;
                }
                if (!(Marshal.GetDelegateForFunctionPointer(BEClient.dlsym(Provider.battlEyeClientHandle, "Init"), typeof(BEClient.BEClientInitFn)) is BEClient.BEClientInitFn bEClientInitFn))
                {
                    BEClient.dlclose(Provider.battlEyeClientHandle);
                    Provider.battlEyeClientHandle = IntPtr.Zero;
                    Provider._connectionFailureInfo = ESteamConnectionFailureInfo.KICKED;
                    Provider._connectionFailureReason = "Failed to get BattlEye client init delegate!";
                    UnturnedLog.error(Provider.connectionFailureReason);
                    Provider.RequestDisconnect("BattlEye get init error");
                    return;
                }
                uint ulAddress = ((value & 0xFF) << 24) | ((value & 0xFF00) << 8) | ((value & 0xFF0000) >> 8) | ((value & 0xFF000000u) >> 24);
                ushort usPort = (ushort)((uint)((value2 & 0xFF) << 8) | ((uint)(value2 & 0xFF00) >> 8));
                Provider.battlEyeClientInitData = new BEClient.BECL_GAME_DATA();
                Provider.battlEyeClientInitData.pstrGameVersion = Provider.APP_NAME + " " + Provider.APP_VERSION;
                Provider.battlEyeClientInitData.ulAddress = ulAddress;
                Provider.battlEyeClientInitData.usPort = usPort;
                Provider.battlEyeClientInitData.pfnPrintMessage = Provider.battlEyeClientPrintMessage;
                Provider.battlEyeClientInitData.pfnRequestRestart = Provider.battlEyeClientRequestRestart;
                Provider.battlEyeClientInitData.pfnSendPacket = Provider.battlEyeClientSendPacket;
                Provider.battlEyeClientRunData = new BEClient.BECL_BE_DATA();
                if (!bEClientInitFn(2, Provider.battlEyeClientInitData, Provider.battlEyeClientRunData))
                {
                    BEClient.dlclose(Provider.battlEyeClientHandle);
                    Provider.battlEyeClientHandle = IntPtr.Zero;
                    Provider._connectionFailureInfo = ESteamConnectionFailureInfo.KICKED;
                    Provider._connectionFailureReason = "Failed to call BattlEye client init!";
                    UnturnedLog.error(Provider.connectionFailureReason);
                    Provider.RequestDisconnect("BattlEye init error");
                    return;
                }
            }
            catch (Exception e)
            {
                Provider._connectionFailureInfo = ESteamConnectionFailureInfo.KICKED;
                Provider._connectionFailureReason = "Unhandled exception when loading BattlEye client library!";
                UnturnedLog.error(Provider.connectionFailureReason);
                UnturnedLog.exception(e);
                Provider.RequestDisconnect("BattlEye load exception");
                return;
            }
        }
        Provider._modeConfigData = new ModeConfigData(Provider.mode);
        reader.ReadUInt8(out var value3);
        Provider._modeConfigData.Gameplay.Repair_Level_Max = value3;
        reader.ReadBit(out Provider._modeConfigData.Gameplay.Hitmarkers);
        reader.ReadBit(out Provider._modeConfigData.Gameplay.Crosshair);
        reader.ReadBit(out Provider._modeConfigData.Gameplay.Ballistics);
        reader.ReadBit(out Provider._modeConfigData.Gameplay.Chart);
        reader.ReadBit(out Provider._modeConfigData.Gameplay.Satellite);
        reader.ReadBit(out Provider._modeConfigData.Gameplay.Compass);
        reader.ReadBit(out Provider._modeConfigData.Gameplay.Group_Map);
        reader.ReadBit(out Provider._modeConfigData.Gameplay.Group_HUD);
        reader.ReadBit(out Provider._modeConfigData.Gameplay.Group_Player_List);
        reader.ReadBit(out Provider._modeConfigData.Gameplay.Allow_Static_Groups);
        reader.ReadBit(out Provider._modeConfigData.Gameplay.Allow_Dynamic_Groups);
        reader.ReadBit(out Provider._modeConfigData.Gameplay.Allow_Shoulder_Camera);
        reader.ReadBit(out Provider._modeConfigData.Gameplay.Can_Suicide);
        reader.ReadBit(out Provider._modeConfigData.Gameplay.Friendly_Fire);
        reader.ReadBit(out Provider._modeConfigData.Gameplay.Bypass_Buildable_Mobility);
        reader.ReadUInt16(out var value4);
        Provider._modeConfigData.Gameplay.Timer_Exit = MathfEx.Min(value4, 60u);
        reader.ReadUInt16(out var value5);
        Provider._modeConfigData.Gameplay.Timer_Respawn = value5;
        reader.ReadUInt16(out var value6);
        Provider._modeConfigData.Gameplay.Timer_Home = value6;
        reader.ReadUInt16(out var value7);
        Provider._modeConfigData.Gameplay.Max_Group_Members = value7;
        reader.ReadBit(out Provider._modeConfigData.Barricades.Allow_Item_Placement_On_Vehicle);
        reader.ReadBit(out Provider._modeConfigData.Barricades.Allow_Trap_Placement_On_Vehicle);
        reader.ReadFloat(out Provider._modeConfigData.Barricades.Max_Item_Distance_From_Hull);
        reader.ReadFloat(out Provider._modeConfigData.Barricades.Max_Trap_Distance_From_Hull);
        reader.ReadFloat(out Provider._modeConfigData.Gameplay.AirStrafing_Acceleration_Multiplier);
        reader.ReadFloat(out Provider._modeConfigData.Gameplay.AirStrafing_Deceleration_Multiplier);
        reader.ReadFloat(out Provider._modeConfigData.Gameplay.ThirdPerson_RecoilMultiplier);
        reader.ReadFloat(out Provider._modeConfigData.Gameplay.ThirdPerson_SpreadMultiplier);
        if (OptionsSettings.streamer)
        {
            SteamFriends.SetRichPresence("connect", "");
        }
        else
        {
            SteamUser.AdvertiseGame(Provider.server, 0u, 0);
            SteamFriends.SetRichPresence("connect", "+connect " + value + ":" + value2);
        }
        Lobbies.leaveLobby();
        SteamMatchmaking.AddFavoriteGame(Provider.APP_ID, value, (ushort)(value2 + 1), value2, Provider.STEAM_FAVORITE_FLAG_HISTORY, SteamUtils.GetServerRealTime());
        Provider.updateRichPresence();
        Provider.onClientConnected?.Invoke();
        ClientMessageHandler_Accepted.OnGameplayConfigReceived?.Invoke();
    }
}
