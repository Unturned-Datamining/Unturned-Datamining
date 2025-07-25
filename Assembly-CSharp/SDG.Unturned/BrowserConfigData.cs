using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SDG.Unturned;

public class BrowserConfigData
{
    public struct Link : IDatParseable, IDatSerializable
    {
        public string Message;

        public string Url;

        /// <summary>
        /// Used to find overrides in json file.
        /// </summary>
        public override bool Equals(object other)
        {
            if (other == null || !(other is Link link))
            {
                return false;
            }
            if (string.Equals(Message, link.Message))
            {
                return string.Equals(Url, link.Url);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Message, Url);
        }

        public bool TryParse(IDatNode node)
        {
            if (node is IDatDictionary dictionary)
            {
                Message = dictionary.GetString("Message");
                Url = dictionary.GetString("URL");
                return true;
            }
            return false;
        }

        public void SerializeIntoDictionary(IEditableDatDictionary dictionary)
        {
            dictionary.AddValue("Message").SetString(Message);
            dictionary.AddValue("URL").SetString(Url);
        }
    }

    /// <summary>
    /// URL of a 64x64 image shown in the upper-left of the server lobby menu.
    /// </summary>
    public string Icon;

    /// <summary>
    /// URL of a 32x32 image shown in the server list.
    /// </summary>
    public string Thumbnail;

    /// <summary>
    /// Short description underneath the server name in the server lobby menu.
    /// </summary>
    public string Desc_Hint;

    /// <summary>
    /// Long description in the lower-right of the server lobby menu.
    /// </summary>
    public string Desc_Full;

    /// <summary>
    /// Short description underneath the server name in the server list.
    /// </summary>
    public string Desc_Server_List;

    /// <summary>
    /// Documentation: https://docs.smartlydressedgames.com/en/stable/servers/game-server-login-tokens.html
    /// To generate a new token visit: https://steamcommunity.com/dev/managegameservers
    /// </summary>
    public string Login_Token;

    /// <summary>
    /// IP address, DNS name, or a web address (to perform GET request) to advertise.
    ///
    /// Servers not using Fake IP can specify just a DNS entry. This way if server's IP changes clients can rejoin.
    /// For example, if you own the "example.com" domain you could add an A record "myunturnedserver" pointing at
    /// your game server IP and set that record here "myunturnedserver.example.com".
    ///
    /// Servers using Fake IP are assigned random ports at startup, but can implement a web API endpoint to return
    /// the IP and port. Clients perform a GET request if this string starts with http:// or https://. The returned
    /// text can be an IP address or DNS name with optional query port override. (e.g., "127.0.0.1:27015")
    ///
    /// Documentation: https://docs.smartlydressedgames.com/en/stable/servers/bookmark-host.html
    /// </summary>
    public string BookmarkHost;

    /// <summary>
    /// If true, the server lobby warns that in-game ping may be higher than shown.
    /// </summary>
    public bool Is_Using_Anycast_Proxy;

    /// <summary>
    /// How the server is monetized (if at all).
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public EServerMonetizationTag Monetization;

    /// <summary>
    /// Buttons shown in the server lobby menu. For example:
    /// ` Links
    /// ` [
    /// `     {
    /// `         Message Visit our website!
    /// `         URL https://smartlydressedgames.com/
    /// `     }
    /// ` ]
    /// </summary>
    public Link[] Links;

    public BrowserConfigData()
    {
        Icon = string.Empty;
        Thumbnail = string.Empty;
        Desc_Hint = string.Empty;
        Desc_Full = string.Empty;
        Desc_Server_List = string.Empty;
        Login_Token = string.Empty;
        BookmarkHost = string.Empty;
        Monetization = EServerMonetizationTag.Unspecified;
        Links = null;
    }
}
