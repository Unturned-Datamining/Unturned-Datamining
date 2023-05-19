using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace SDG.Unturned;

public class ReadWrite
{
    public static readonly string PATH = UnturnedPaths.RootDirectory.FullName;

    private static CommandLineFlag disableSteamCloudRead = new CommandLineFlag(defaultValue: false, "-DisableSteamCloudRead");

    private static readonly XmlSerializerNamespaces XML_SERIALIZER_NAMESPACES = new XmlSerializerNamespaces(new XmlQualifiedName[1] { XmlQualifiedName.Empty });

    private static readonly XmlWriterSettings XML_WRITER_SETTINGS = new XmlWriterSettings
    {
        Indent = true,
        OmitXmlDeclaration = true,
        Encoding = new UTF8Encoding()
    };

    private static DatParser datParser = new DatParser();

    public static bool SupportsOpeningFileBrowser => false;

    public static bool appIn(byte[] h, byte p)
    {
        Block block = readBlock("/Extras/Sources/Animation/appout.log", useCloud: false, 0);
        byte[] hash_ = block.readByteArray();
        byte[] hash_2 = block.readByteArray();
        byte[] hash_3 = block.readByteArray();
        byte[] hash_4 = block.readByteArray();
        byte[] hash_5 = block.readByteArray();
        byte[] hash_6 = block.readByteArray();
        byte[] hash_7 = block.readByteArray();
        switch (p)
        {
        case 0:
            if (Hash.verifyHash(h, hash_))
            {
                return true;
            }
            break;
        case 1:
            if (Hash.verifyHash(h, hash_))
            {
                return true;
            }
            if (Hash.verifyHash(h, hash_2))
            {
                return true;
            }
            if (Hash.verifyHash(h, hash_3))
            {
                return true;
            }
            break;
        case 2:
            if (Hash.verifyHash(h, hash_4))
            {
                return true;
            }
            if (Hash.verifyHash(h, hash_5))
            {
                return true;
            }
            break;
        case 3:
            if (Hash.verifyHash(h, hash_6))
            {
                return true;
            }
            if (Hash.verifyHash(h, hash_7))
            {
                return true;
            }
            break;
        }
        return false;
    }

    public static byte[] readData()
    {
        FileStream fileStream = new FileStream(PATH + "/Unturned_Data/Managed/Assembly-CSharp.dll", FileMode.Open, FileAccess.Read, FileShare.Read);
        byte[] array = new byte[fileStream.Length];
        fileStream.Read(array, 0, array.Length);
        fileStream.Close();
        fileStream.Dispose();
        return Hash.SHA1(array);
    }

    public static T deserializeJSON<T>(string path, bool useCloud)
    {
        return deserializeJSON<T>(path, useCloud, usePath: true);
    }

    public static T deserializeJSON<T>(string path, bool useCloud, bool usePath)
    {
        T result = default(T);
        byte[] array = readBytes(path, useCloud, usePath);
        if (array == null)
        {
            return result;
        }
        string @string = Encoding.UTF8.GetString(array);
        if (@string == null)
        {
            return result;
        }
        return JsonConvert.DeserializeObject<T>(@string);
    }

    public static void populateJSON(string path, object target, bool usePath = true)
    {
        byte[] array = readBytes(path, useCloud: false, usePath);
        if (array != null)
        {
            string @string = Encoding.UTF8.GetString(array);
            if (@string != null)
            {
                JsonConvert.PopulateObject(@string, target);
            }
        }
    }

    public static byte[] cloudFileRead(string path)
    {
        if (!cloudFileExists(path))
        {
            return null;
        }
        Provider.provider.cloudService.getSize(path, out var size);
        byte[] array = new byte[size];
        if (!Provider.provider.cloudService.read(path, array))
        {
            UnturnedLog.error("Failed to read the correct file size.");
            return null;
        }
        return array;
    }

    public static void cloudFileWrite(string path, byte[] bytes, int size)
    {
        if (!Provider.provider.cloudService.write(path, bytes, size))
        {
            UnturnedLog.error("Failed to write file.");
        }
    }

    public static void cloudFileDelete(string path)
    {
        Provider.provider.cloudService.delete(path);
    }

    public static bool cloudFileExists(string path)
    {
        if ((bool)disableSteamCloudRead)
        {
            return false;
        }
        Provider.provider.cloudService.exists(path, out var exists);
        return exists;
    }

    public static void serializeJSON<T>(string path, bool useCloud, T instance)
    {
        serializeJSON(path, useCloud, usePath: true, instance);
    }

    public static void serializeJSON<T>(string path, bool useCloud, bool usePath, T instance)
    {
        string s = JsonConvert.SerializeObject(instance, Newtonsoft.Json.Formatting.Indented);
        byte[] bytes = Encoding.UTF8.GetBytes(s);
        writeBytes(path, useCloud, usePath, bytes, bytes.Length);
    }

    public static T deserializeXML<T>(string path, bool useCloud)
    {
        return deserializeXML<T>(path, useCloud, usePath: true);
    }

    public static T deserializeXML<T>(string path, bool useCloud, bool usePath)
    {
        T result = default(T);
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
        if (useCloud)
        {
            MemoryStream memoryStream = new MemoryStream(cloudFileRead(path));
            try
            {
                return (T)xmlSerializer.Deserialize(memoryStream);
            }
            finally
            {
                memoryStream.Close();
                memoryStream.Dispose();
            }
        }
        if (usePath)
        {
            path += PATH;
        }
        if (!Directory.Exists(Path.GetDirectoryName(path)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        }
        if (!File.Exists(path))
        {
            UnturnedLog.info("Failed to find file at: " + path);
            return result;
        }
        StreamReader streamReader = new StreamReader(path);
        try
        {
            return (T)xmlSerializer.Deserialize(streamReader);
        }
        finally
        {
            streamReader.Close();
            streamReader.Dispose();
        }
    }

    public static void serializeXML<T>(string path, bool useCloud, T instance)
    {
        serializeXML(path, useCloud, usePath: true, instance);
    }

    public static void serializeXML<T>(string path, bool useCloud, bool usePath, T instance)
    {
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
        if (useCloud)
        {
            MemoryStream memoryStream = new MemoryStream();
            XmlWriter xmlWriter = XmlWriter.Create(memoryStream, XML_WRITER_SETTINGS);
            try
            {
                xmlSerializer.Serialize(xmlWriter, instance, XML_SERIALIZER_NAMESPACES);
                cloudFileWrite(path, memoryStream.GetBuffer(), (int)memoryStream.Length);
                return;
            }
            finally
            {
                xmlWriter.Close();
                memoryStream.Close();
                memoryStream.Dispose();
            }
        }
        if (usePath)
        {
            path = PATH + path;
        }
        if (!Directory.Exists(Path.GetDirectoryName(path)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        }
        StreamWriter streamWriter = new StreamWriter(path);
        try
        {
            xmlSerializer.Serialize(streamWriter, instance, XML_SERIALIZER_NAMESPACES);
        }
        finally
        {
            streamWriter.Close();
            streamWriter.Dispose();
        }
    }

    public static byte[] readBytes(string path, bool useCloud)
    {
        return readBytes(path, useCloud, usePath: true);
    }

    public static byte[] readBytes(string path, bool useCloud, bool usePath)
    {
        if (useCloud)
        {
            return cloudFileRead(path);
        }
        if (usePath)
        {
            path = PATH + path;
        }
        if (!Directory.Exists(Path.GetDirectoryName(path)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        }
        if (!File.Exists(path))
        {
            UnturnedLog.info("Failed to find file at: " + path);
            return null;
        }
        FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        byte[] array = new byte[fileStream.Length];
        if (fileStream.Read(array, 0, array.Length) != array.Length)
        {
            UnturnedLog.error("Failed to read the correct file size.");
            return null;
        }
        fileStream.Close();
        fileStream.Dispose();
        return array;
    }

    private static string readString(string filePath, bool useCloud, bool prependPath)
    {
        if (useCloud)
        {
            byte[] array = readBytes(filePath, useCloud, prependPath);
            if (array == null)
            {
                return null;
            }
            return Encoding.UTF8.GetString(array);
        }
        if (prependPath)
        {
            filePath = PATH + filePath;
        }
        string directoryName = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }
        if (!File.Exists(filePath))
        {
            UnturnedLog.info("Failed to find file at: " + filePath);
            return null;
        }
        return File.ReadAllText(filePath);
    }

    public static Data readData(string path, bool useCloud)
    {
        return readData(path, useCloud, usePath: true);
    }

    public static Data readData(string path, bool useCloud, bool usePath)
    {
        string text = readString(path, useCloud, usePath);
        if (text == null)
        {
            return null;
        }
        if (text.Length == 0)
        {
            return new Data();
        }
        return new Data(text);
    }

    internal static DatDictionary ReadDataWithoutHash(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }
        using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        using StreamReader inputReader = new StreamReader(stream);
        return datParser.Parse(inputReader);
    }

    public static Block readBlock(string path, bool useCloud, byte prefix)
    {
        return readBlock(path, useCloud, usePath: true, prefix);
    }

    public static Block readBlock(string path, bool useCloud, bool usePath, byte prefix)
    {
        byte[] array = readBytes(path, useCloud, usePath);
        if (array == null)
        {
            return null;
        }
        return new Block(prefix, array);
    }

    public static void writeBytes(string path, bool useCloud, byte[] bytes)
    {
        writeBytes(path, useCloud, usePath: true, bytes, bytes.Length);
    }

    public static void writeBytes(string path, bool useCloud, byte[] bytes, int size)
    {
        writeBytes(path, useCloud, usePath: true, bytes, size);
    }

    public static void writeBytes(string path, bool useCloud, bool usePath, byte[] bytes)
    {
        writeBytes(path, useCloud, usePath, bytes, bytes.Length);
    }

    public static void writeBytes(string path, bool useCloud, bool usePath, byte[] bytes, int size)
    {
        if (useCloud)
        {
            cloudFileWrite(path, bytes, size);
            return;
        }
        if (usePath)
        {
            path = PATH + path;
        }
        if (!Directory.Exists(Path.GetDirectoryName(path)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        }
        FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate);
        fileStream.Write(bytes, 0, size);
        fileStream.SetLength(size);
        fileStream.Flush();
        fileStream.Close();
        fileStream.Dispose();
    }

    public static void writeData(string path, bool useCloud, Data data)
    {
        writeData(path, useCloud, usePath: true, data);
    }

    public static void writeData(string path, bool useCloud, bool usePath, Data data)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(data.getFile());
        writeBytes(path, useCloud, usePath, bytes);
    }

    public static void writeBlock(string path, bool useCloud, Block block)
    {
        writeBlock(path, useCloud, usePath: true, block);
    }

    public static void writeBlock(string path, bool useCloud, bool usePath, Block block)
    {
        int size;
        byte[] bytes = block.getBytes(out size);
        writeBytes(path, useCloud, usePath, bytes, size);
    }

    public static void deleteFile(string path, bool useCloud)
    {
        deleteFile(path, useCloud, usePath: true);
    }

    public static void deleteFile(string path, bool useCloud, bool usePath)
    {
        if (useCloud)
        {
            cloudFileDelete(path);
            return;
        }
        if (usePath)
        {
            path = PATH + path;
        }
        File.Delete(path);
    }

    public static void deleteFolder(string path)
    {
        deleteFolder(path, usePath: true);
    }

    public static void deleteFolder(string path, bool usePath)
    {
        if (usePath)
        {
            path = PATH + path;
        }
        Directory.Delete(path, recursive: true);
    }

    public static void moveFolder(string origin, string target)
    {
        moveFolder(origin, target, usePath: true);
    }

    public static void moveFolder(string origin, string target, bool usePath)
    {
        if (usePath)
        {
            origin = PATH + origin;
            target = PATH + target;
        }
        Directory.Move(origin, target);
    }

    public static void createFolder(string path)
    {
        createFolder(path, usePath: true);
    }

    public static void createFolder(string path, bool usePath)
    {
        if (usePath)
        {
            path = PATH + path;
        }
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    public static void createHidden(string path)
    {
        createHidden(path, usePath: true);
    }

    public static void createHidden(string path, bool usePath)
    {
        if (usePath)
        {
            path = PATH + path;
        }
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path).Attributes = FileAttributes.Directory | FileAttributes.Hidden;
        }
    }

    public static string folderName(string path)
    {
        return new DirectoryInfo(path).Name;
    }

    public static string folderPath(string path)
    {
        return Path.GetDirectoryName(path);
    }

    public static void renameFile(string path_0, string path_1)
    {
        path_0 = PATH + path_0;
        path_1 = PATH + path_1;
        File.Move(path_0, path_1);
    }

    public static string fileName(string path)
    {
        return Path.GetFileNameWithoutExtension(path);
    }

    public static bool fileExists(string path, bool useCloud)
    {
        return fileExists(path, useCloud, usePath: true);
    }

    public static bool fileExists(string path, bool useCloud, bool usePath)
    {
        if (useCloud)
        {
            return cloudFileExists(path);
        }
        if (usePath)
        {
            path = PATH + path;
        }
        return File.Exists(path);
    }

    public static string folderFound(string path)
    {
        return folderFound(path, usePath: true);
    }

    public static string folderFound(string path, bool usePath)
    {
        if (usePath)
        {
            path = PATH + path;
        }
        string[] directories = Directory.GetDirectories(path);
        if (directories.Length != 0)
        {
            return directories[0];
        }
        return null;
    }

    public static bool folderExists(string path)
    {
        return folderExists(path, usePath: true);
    }

    public static bool folderExists(string path, bool usePath)
    {
        if (usePath)
        {
            path = PATH + path;
        }
        return Directory.Exists(path);
    }

    public static bool hasDirectoryWritePermission(string path)
    {
        try
        {
            AuthorizationRuleCollection accessRules = Directory.GetAccessControl(path).GetAccessRules(includeExplicit: true, includeInherited: true, typeof(SecurityIdentifier));
            bool result = false;
            foreach (FileSystemAccessRule item in accessRules)
            {
                if ((item.FileSystemRights & FileSystemRights.Write) == FileSystemRights.Write)
                {
                    switch (item.AccessControlType)
                    {
                    case AccessControlType.Allow:
                        result = true;
                        break;
                    case AccessControlType.Deny:
                        return false;
                    }
                }
            }
            return result;
        }
        catch
        {
            return false;
        }
    }

    public static string[] getFolders(string path)
    {
        return getFolders(path, usePath: true);
    }

    public static string[] getFolders(string path, bool usePath)
    {
        if (usePath)
        {
            path = PATH + path;
        }
        return Directory.GetDirectories(path);
    }

    public static string[] getFiles(string path)
    {
        return getFiles(path, usePath: true);
    }

    public static string[] getFiles(string path, bool usePath)
    {
        if (usePath)
        {
            path = PATH + path;
        }
        return Directory.GetFiles(path);
    }

    public static void copyFile(string source, string destination)
    {
        source = PATH + source;
        destination = PATH + destination;
        if (!Directory.Exists(Path.GetDirectoryName(destination)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(destination));
        }
        File.Copy(source, destination);
    }

    public static Texture2D readTextureFromFile(string path, bool useBasePath, EReadTextureFromFileMode mode = EReadTextureFromFileMode.UI)
    {
        if (useBasePath)
        {
            path = PATH + path;
        }
        return readTextureFromFile(path);
    }

    public static Texture2D readTextureFromFile(string absolutePath, EReadTextureFromFileMode mode = EReadTextureFromFileMode.UI)
    {
        byte[] data = File.ReadAllBytes(absolutePath);
        bool mipChain = false;
        bool linear = false;
        Texture2D obj = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain, linear)
        {
            hideFlags = HideFlags.HideAndDontSave
        };
        bool markNonReadable = true;
        obj.LoadImage(data, markNonReadable);
        return obj;
    }

    public static void OpenFileBrowser(string folderPath)
    {
        UnturnedLog.info("Cannot open file browser to path (not supported): \"" + folderPath + "\"");
    }
}
