using System;
using System.IO;
using System.Runtime.InteropServices;
using BattlEye;
using SDG.NetPak;
using Steamworks;
using Unturned.SystemEx;

namespace SDG.Unturned;

internal static class ClientMessageHandler_Accepted
{
    /// <summary>
    /// Nelson 2025-06-19: using server-provided connection details is useful because
    /// it can find its public IP (e.g., joining by LAN and sharing WAN IP), and/or
    /// its fake IP (again when joining by LAN).
    /// </summary>
    internal static string RichPresenceConnectionTarget { get; private set; }

    internal static event System.Action OnGameplayConfigReceived;

    internal static void ReadMessage(NetPakReader reader)
    {
        Provider.isWaitingForAuthenticationResponse = false;
        reader.ReadUInt32(out var value);
        reader.ReadUInt16(out var value2);
        bool flag = SteamNetworkingUtils.IsFakeIPv4(value);
        UnturnedLog.info("Accepted by server");
        if (!Provider.IsThirdpartyAntiCheatActiveOnCurrentServer || InitThirdpartyAntiCheat(value, value2, flag))
        {
            bool valueOrDefault = (Provider._modeConfigData?.Gameplay?.Use_2D_Scope_Overlay).GetValueOrDefault();
            bool valueOrDefault2 = (Provider._modeConfigData?.Gameplay?.Disable_Foliage_Off).GetValueOrDefault();
            Provider._modeConfigData = new ModeConfigData(Provider.mode);
            reader.ReadUInt8(out var value3);
            Provider._modeConfigData.Gameplay.Repair_Level_Max = value3;
            reader.ReadFloat(out Provider._modeConfigData.Players.Skill_Cost_Multiplier);
            reader.ReadBit(out Provider._modeConfigData.Players.Skillset_Reduces_Skill_Cost);
            reader.ReadBit(out Provider._modeConfigData.Players.Prevent_Level_Skill_Overrides);
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
            reader.ReadBit(out Provider._modeConfigData.Gameplay.Bypass_Building_In_Safezones);
            reader.ReadBit(out Provider._modeConfigData.Gameplay.Bypass_No_Building_Zones);
            reader.ReadBit(out Provider._modeConfigData.Gameplay.Allow_Freeform_Buildables);
            reader.ReadBit(out Provider._modeConfigData.Gameplay.Allow_Freeform_Buildables_On_Vehicles);
            reader.ReadBit(out Provider._modeConfigData.Gameplay.Enable_Damage_Flinch);
            reader.ReadBit(out Provider._modeConfigData.Gameplay.Enable_Explosion_Camera_Shake);
            reader.ReadBit(out Provider._modeConfigData.Gameplay.Enable_Workstation_Requirements);
            reader.ReadBit(out Provider._modeConfigData.Gameplay.Disable_Motion_Sickness_Options);
            reader.ReadBit(out Provider._modeConfigData.Gameplay.Disable_Foliage_Off);
            reader.ReadBit(out Provider._modeConfigData.Gameplay.Use_2D_Scope_Overlay);
            reader.ReadBit(out Provider._modeConfigData.Gameplay.Enable_Fishing_Catch_Challenge);
            if (Provider._modeConfigData.Gameplay.Use_2D_Scope_Overlay != valueOrDefault && Player.LocalPlayer != null)
            {
                Player.LocalPlayer.look.updateScope(GraphicsSettings.scopeQuality);
            }
            if (Provider._modeConfigData.Gameplay.Disable_Foliage_Off != valueOrDefault2)
            {
                GraphicsSettings.ApplyFoliageQuality();
            }
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
            reader.ReadFloat(out Provider._modeConfigData.Gameplay.FirstPerson_RecoilMultiplier);
            reader.ReadFloat(out Provider._modeConfigData.Gameplay.FirstPerson_AimingRecoilMultiplier);
            reader.ReadFloat(out Provider._modeConfigData.Gameplay.FirstPerson_AimingZoomRecoilReduction);
            reader.ReadFloat(out Provider._modeConfigData.Gameplay.ThirdPerson_RecoilMultiplier);
            reader.ReadFloat(out Provider._modeConfigData.Gameplay.ThirdPerson_SpreadMultiplier);
            reader.ReadFloat(out Provider._modeConfigData.Gameplay.Viewmodel_AimingJumpLandMultiplier);
            reader.ReadFloat(out Provider._modeConfigData.Gameplay.Viewmodel_AimingMisalignmentMultiplier);
            reader.ReadFloat(out Provider._modeConfigData.Gameplay.Min_Fishing_Bite_Interval);
            reader.ReadFloat(out Provider._modeConfigData.Gameplay.Max_Fishing_Bite_Interval);
            reader.ReadFloat(out Provider._modeConfigData.Gameplay.Fishing_MaxStrength_Bite_Interval_Multiplier);
            RichPresenceConnectionTarget = Provider.server.ToString();
            if (flag)
            {
                IPv4Address pv4Address = new IPv4Address(value);
                RichPresenceConnectionTarget = $"{pv4Address}:{value2}";
                UnturnedLog.info("Rich presence advertisement using Fake IP address (" + RichPresenceConnectionTarget + ")");
            }
            else
            {
                UnturnedLog.info("Rich presence advertisement using server code (" + RichPresenceConnectionTarget + ")");
            }
            if (OptionsSettings.ShouldHideRichPresence)
            {
                SteamFriends.SetRichPresence("connect", "");
            }
            else
            {
                SteamUser.AdvertiseGame(Provider.server, 0u, 0);
                SteamFriends.SetRichPresence("connect", "+connect " + RichPresenceConnectionTarget);
            }
            Lobbies.leaveLobby();
            SteamMatchmaking.AddFavoriteGame(Provider.APP_ID, value, (ushort)(value2 + 1), value2, Provider.STEAM_FAVORITE_FLAG_HISTORY, SteamUtils.GetServerRealTime());
            Provider.updateRichPresence();
            Provider.onClientConnected?.Invoke();
            ClientMessageHandler_Accepted.OnGameplayConfigReceived?.Invoke();
        }
    }

    private static bool InitThirdpartyAntiCheat(uint ip, ushort port, bool isIpFake)
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
            return false;
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
                return false;
            }
            if (!(Marshal.GetDelegateForFunctionPointer(BEClient.dlsym(Provider.battlEyeClientHandle, "Init"), typeof(BEClient.BEClientInitFn)) is BEClient.BEClientInitFn bEClientInitFn))
            {
                BEClient.dlclose(Provider.battlEyeClientHandle);
                Provider.battlEyeClientHandle = IntPtr.Zero;
                Provider._connectionFailureInfo = ESteamConnectionFailureInfo.KICKED;
                Provider._connectionFailureReason = "Failed to get BattlEye client init delegate!";
                UnturnedLog.error(Provider.connectionFailureReason);
                Provider.RequestDisconnect("BattlEye get init error");
                return false;
            }
            uint ulAddress;
            ushort usPort;
            if (isIpFake)
            {
                ulAddress = 0u;
                usPort = 0;
            }
            else
            {
                ulAddress = ((ip & 0xFF) << 24) | ((ip & 0xFF00) << 8) | ((ip & 0xFF0000) >> 8) | ((ip & 0xFF000000u) >> 24);
                usPort = (ushort)((uint)((port & 0xFF) << 8) | ((uint)(port & 0xFF00) >> 8));
            }
            Provider.battlEyeClientInitData = new BEClient.BECL_GAME_DATA();
            if (Provider._modInfo != null)
            {
                Provider.battlEyeClientInitData.pstrGameVersion = Provider._modInfo.Name + " " + Provider._modInfo.FormatModVersion();
            }
            else
            {
                Provider.battlEyeClientInitData.pstrGameVersion = Provider.APP_NAME + " " + Provider.APP_VERSION;
            }
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
                return false;
            }
        }
        catch (Exception e)
        {
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.KICKED;
            Provider._connectionFailureReason = "Unhandled exception when loading BattlEye client library!";
            UnturnedLog.error(Provider.connectionFailureReason);
            UnturnedLog.exception(e);
            Provider.RequestDisconnect("BattlEye load exception");
            return false;
        }
        return true;
    }
}
