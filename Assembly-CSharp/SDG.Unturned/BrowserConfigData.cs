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
        Monetization = EServerMonetizationTag.Unspecified;
        Links = null;
    }
}
