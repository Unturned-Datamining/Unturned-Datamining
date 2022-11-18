using System;
using SDG.Framework.IO.Deserialization;
using SDG.Framework.IO.Serialization;
using SDG.Unturned;

namespace SDG.Framework.IO;

public class IOUtility
{
    public static IDeserializer jsonDeserializer = new JSONDeserializer();

    public static ISerializer jsonSerializer = new JSONSerializer();

    public static IDeserializer xmlDeserializer = new XMLDeserializer();

    public static ISerializer xmlSerializer = new XMLSerializer();

    [Obsolete("Replaced by UnturnedPaths.RootDirectory")]
    public static string rootPath => UnturnedPaths.RootDirectory.FullName;
}
