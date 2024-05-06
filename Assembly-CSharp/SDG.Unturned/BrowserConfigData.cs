using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SDG.Unturned;

public class BrowserConfigData
{
    public struct Link
    {
        public string Message;

        public string Url;
    }

    public string Icon;

    public string Thumbnail;

    public string Desc_Hint;

    public string Desc_Full;

    public string Desc_Server_List;

    /// <summary>
    /// https://steamcommunity.com/dev/managegameservers
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
    /// </summary>
    public string BookmarkHost;

    [JsonConverter(typeof(StringEnumConverter))]
    public EServerMonetizationTag Monetization;

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
