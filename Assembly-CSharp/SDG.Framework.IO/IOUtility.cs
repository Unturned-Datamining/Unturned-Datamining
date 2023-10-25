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

    /// <summary>
    /// Path to the folder which contains the Unity player executable.
    /// </summary>
    [Obsolete("Replaced by UnturnedPaths.RootDirectory")]
    public static string rootPath => UnturnedPaths.RootDirectory.FullName;
}
