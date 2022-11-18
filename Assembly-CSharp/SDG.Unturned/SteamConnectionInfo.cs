namespace SDG.Unturned;

public class SteamConnectionInfo
{
    public uint _ip;

    public ushort _port;

    public string _password;

    public uint ip => _ip;

    public ushort port => _port;

    public string password => _password;

    public SteamConnectionInfo(uint newIP, ushort newPort, string newPassword)
    {
        _ip = newIP;
        _port = newPort;
        _password = newPassword;
    }

    public SteamConnectionInfo(string newIP, ushort newPort, string newPassword)
    {
        _ip = Parser.getUInt32FromIP(newIP);
        _port = newPort;
        _password = newPassword;
    }

    public SteamConnectionInfo(string newIPPort, string newPassword)
    {
        string[] componentsFromSerial = Parser.getComponentsFromSerial(newIPPort, ':');
        _ip = Parser.getUInt32FromIP(componentsFromSerial[0]);
        _port = ushort.Parse(componentsFromSerial[1]);
        _password = newPassword;
    }
}
