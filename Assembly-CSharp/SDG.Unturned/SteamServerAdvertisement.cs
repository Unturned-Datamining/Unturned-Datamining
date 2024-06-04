using SDG.HostBans;
using Steamworks;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned;

/// <summary>
/// Information about a game server retrieved through Steam's "A2S" query system.
/// Available when joining using the Steam server list API (in-game server browser)
/// or querying the Server's A2S port directly (connect by IP menu), but not when
/// joining by Steam ID.
/// </summary>
public class SteamServerAdvertisement
{
    public enum EPluginFramework
    {
        None,
        Rocket,
        OpenMod,
        Unknown
    }

    public enum EInfoSource
    {
        /// <summary>
        /// Join server by IP.
        /// </summary>
        DirectConnect,
        InternetServerList,
        FavoriteServerList,
        FriendServerList,
        HistoryServerList,
        LanServerList
    }

    private CSteamID _steamID;

    private uint _ip;

    public ushort queryPort;

    public ushort connectionPort;

    private string _name;

    private string _map;

    private bool _isPvP;

    private bool _hasCheats;

    private bool _isWorkshop;

    private EGameMode _mode;

    private ECameraMode _cameraMode;

    private int _ping;

    internal int sortingPing;

    private int _players;

    private int _maxPlayers;

    private bool _isPassworded;

    private bool _isPro;

    internal EInfoSource infoSource;

    internal EHostBanFlags hostBanFlags;

    public CSteamID steamID => _steamID;

    public uint ip => _ip;

    public string name => _name;

    public string map => _map;

    public bool isPvP => _isPvP;

    public bool hasCheats => _hasCheats;

    public bool isWorkshop => _isWorkshop;

    public EGameMode mode => _mode;

    public ECameraMode cameraMode => _cameraMode;

    public EServerMonetizationTag monetization { get; private set; }

    public int ping => _ping;

    public int players => _players;

    public int maxPlayers => _maxPlayers;

    public bool isPassworded => _isPassworded;

    public bool IsVACSecure { get; private set; }

    public bool IsBattlEyeSecure { get; private set; }

    public bool isPro => _isPro;

    /// <summary>
    /// ID of network transport implementation to use.
    /// </summary>
    public string networkTransport { get; protected set; }

    /// <summary>
    /// Known plugin systems.
    /// </summary>
    public EPluginFramework pluginFramework { get; protected set; }

    public string thumbnailURL { get; protected set; }

    public string descText { get; protected set; }

    /// <summary>
    /// Probably just checks whether IP is link-local, but may as well use Steam's utility function.
    /// </summary>
    public bool IsAddressUsingSteamFakeIP()
    {
        return SteamNetworkingUtils.IsFakeIPv4(ip);
    }

    /// <summary>
    /// Parses value between two keys <stuff>thing</stuff> would parse thing
    /// </summary>
    protected string parseTagValue(string tags, string startKey, string endKey)
    {
        int num = tags.IndexOf(startKey);
        if (num == -1)
        {
            return null;
        }
        num += startKey.Length;
        int num2 = tags.IndexOf(endKey, num);
        if (num2 == -1)
        {
            return null;
        }
        if (num2 == num)
        {
            return null;
        }
        return tags.Substring(num, num2 - num);
    }

    protected bool hasTagKey(string tags, string key, int thumbnailIndex)
    {
        int num = tags.IndexOf(key);
        if (num == -1)
        {
            return false;
        }
        if (thumbnailIndex == -1)
        {
            return true;
        }
        return num < thumbnailIndex;
    }

    internal void SetServerListHostBanFlags(EHostBanFlags hostBanFlags)
    {
        this.hostBanFlags = hostBanFlags;
        if (hostBanFlags.HasFlag(EHostBanFlags.QueryPingWarning))
        {
            sortingPing += LiveConfig.Get().queryPingWarningOffsetMs;
        }
    }

    public SteamServerAdvertisement(gameserveritem_t data, EInfoSource infoSource)
    {
        _steamID = data.m_steamID;
        _ip = data.m_NetAdr.GetIP();
        queryPort = data.m_NetAdr.GetQueryPort();
        connectionPort = (ushort)(queryPort + 1);
        _name = data.GetServerName();
        ProfanityFilter.ApplyFilter(OptionsSettings.filter, ref _name);
        _map = data.GetMap();
        string gameTags = data.GetGameTags();
        if (gameTags.Length > 0)
        {
            int thumbnailIndex = gameTags.IndexOf("<tn>");
            _isPvP = hasTagKey(gameTags, "PVP", thumbnailIndex);
            _hasCheats = hasTagKey(gameTags, "CHy", thumbnailIndex);
            _isWorkshop = hasTagKey(gameTags, "WSy", thumbnailIndex);
            if (hasTagKey(gameTags, Provider.getModeTagAbbreviation(EGameMode.EASY), thumbnailIndex))
            {
                _mode = EGameMode.EASY;
            }
            else if (hasTagKey(gameTags, Provider.getModeTagAbbreviation(EGameMode.HARD), thumbnailIndex))
            {
                _mode = EGameMode.HARD;
            }
            else
            {
                _mode = EGameMode.NORMAL;
            }
            if (hasTagKey(gameTags, Provider.getCameraModeTagAbbreviation(ECameraMode.FIRST), thumbnailIndex))
            {
                _cameraMode = ECameraMode.FIRST;
            }
            else if (hasTagKey(gameTags, Provider.getCameraModeTagAbbreviation(ECameraMode.THIRD), thumbnailIndex))
            {
                _cameraMode = ECameraMode.THIRD;
            }
            else if (hasTagKey(gameTags, Provider.getCameraModeTagAbbreviation(ECameraMode.BOTH), thumbnailIndex))
            {
                _cameraMode = ECameraMode.BOTH;
            }
            else
            {
                _cameraMode = ECameraMode.VEHICLE;
            }
            if (hasTagKey(gameTags, Provider.GetMonetizationTagAbbreviation(EServerMonetizationTag.None), thumbnailIndex))
            {
                monetization = EServerMonetizationTag.None;
            }
            else if (hasTagKey(gameTags, Provider.GetMonetizationTagAbbreviation(EServerMonetizationTag.NonGameplay), thumbnailIndex))
            {
                monetization = EServerMonetizationTag.NonGameplay;
            }
            else if (hasTagKey(gameTags, Provider.GetMonetizationTagAbbreviation(EServerMonetizationTag.Monetized), thumbnailIndex))
            {
                monetization = EServerMonetizationTag.Monetized;
            }
            else
            {
                monetization = EServerMonetizationTag.Unspecified;
            }
            _isPro = hasTagKey(gameTags, "GLD", thumbnailIndex);
            IsBattlEyeSecure = hasTagKey(gameTags, "BEy", thumbnailIndex);
            networkTransport = parseTagValue(gameTags, "<net>", "</net>");
            if (string.IsNullOrEmpty(networkTransport))
            {
                UnturnedLog.warn("Unable to parse net transport tag for server \"{0}\" from \"{1}\"", name, gameTags);
            }
            string text = parseTagValue(gameTags, "<pf>", "</pf>");
            if (string.IsNullOrEmpty(text))
            {
                if (data.m_nBotPlayers == 1)
                {
                    pluginFramework = EPluginFramework.Rocket;
                }
                else
                {
                    pluginFramework = EPluginFramework.None;
                }
            }
            else if (text.Equals("rm"))
            {
                pluginFramework = EPluginFramework.Rocket;
            }
            else if (text.Equals("om"))
            {
                pluginFramework = EPluginFramework.OpenMod;
            }
            else
            {
                pluginFramework = EPluginFramework.Unknown;
            }
            thumbnailURL = parseTagValue(gameTags, "<tn>", "</tn>");
            string message = data.GetGameDescription();
            if (!RichTextUtil.IsTextValidForServerListShortDescription(message))
            {
                message = null;
            }
            else
            {
                ProfanityFilter.ApplyFilter(OptionsSettings.filter, ref message);
            }
            if (message.ContainsNewLine() || message.ContainsChar('\t'))
            {
                message = null;
                UnturnedLog.warn("Control characters not allowed in server \"" + name + "\" description");
            }
            descText = message;
        }
        else
        {
            _isPvP = true;
            _hasCheats = false;
            _mode = EGameMode.NORMAL;
            _cameraMode = ECameraMode.FIRST;
            monetization = EServerMonetizationTag.Unspecified;
            _isPro = true;
            IsBattlEyeSecure = false;
            networkTransport = null;
            pluginFramework = EPluginFramework.None;
            thumbnailURL = null;
            descText = null;
        }
        _ping = data.m_nPing;
        sortingPing = _ping;
        _maxPlayers = data.m_nMaxPlayers;
        if (data.m_nPlayers < 0 || data.m_nBotPlayers < 0 || data.m_nPlayers > 255 || data.m_nBotPlayers > 255)
        {
            _players = 0;
        }
        else
        {
            _players = Mathf.Max(0, data.m_nPlayers - data.m_nBotPlayers);
        }
        _isPassworded = data.m_bPassword;
        IsVACSecure = data.m_bSecure;
        this.infoSource = infoSource;
    }

    public SteamServerAdvertisement(string newName, EGameMode newMode, bool newVACSecure, bool newBattlEyeEnabled, bool newPro)
    {
        _name = newName;
        ProfanityFilter.ApplyFilter(OptionsSettings.filter, ref _name);
        _mode = newMode;
        IsVACSecure = newVACSecure;
        IsBattlEyeSecure = newBattlEyeEnabled;
        _isPro = newPro;
    }

    public SteamServerAdvertisement(CSteamID steamId)
    {
        _steamID = steamId;
    }

    public override string ToString()
    {
        return "Name: " + name + " Map: " + map + " PvP: " + isPvP + " Mode: " + mode.ToString() + " Ping: " + ping + " Players: " + players + "/" + maxPlayers + " Passworded: " + isPassworded;
    }
}