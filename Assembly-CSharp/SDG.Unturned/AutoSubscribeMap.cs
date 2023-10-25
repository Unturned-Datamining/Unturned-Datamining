using System;

namespace SDG.Unturned;

public class AutoSubscribeMap
{
    /// <summary>
    /// Published steam id to subscribe to.
    /// </summary>
    public ulong Workshop_File_Id;

    /// <summary>
    /// If logging in after this point, subscribe.
    /// </summary>
    public DateTime Start;

    /// <summary>
    /// If logging in before this point, subscribe. 
    /// </summary>
    public DateTime End;
}
