using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class Data
{
    private Dictionary<string, string> data;

    private byte[] _hash;

    public bool isCSV;

    public byte[] hash => _hash;

    public bool isEmpty => data.Count == 0;

    public List<string> errors { get; protected set; }

    public bool TryReadString(string key, out string value)
    {
        return data.TryGetValue(key, out value);
    }

    public string readString(string key, string defaultValue = null)
    {
        if (!data.TryGetValue(key, out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public T readEnum<T>(string key, T defaultValue = default(T)) where T : struct
    {
        if (data.TryGetValue(key, out var value))
        {
            try
            {
                return (T)Enum.Parse(typeof(T), value, ignoreCase: true);
            }
            catch
            {
                return defaultValue;
            }
        }
        return defaultValue;
    }

    public bool readBoolean(string key, bool defaultValue = false)
    {
        if (data.TryGetValue(key, out var value))
        {
            if (!value.Equals("y", StringComparison.InvariantCultureIgnoreCase) && !(value == "1"))
            {
                return value.Equals("true", StringComparison.InvariantCultureIgnoreCase);
            }
            return true;
        }
        return defaultValue;
    }

    public byte readByte(string key, byte defaultValue = 0)
    {
        if (!byte.TryParse(readString(key), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
        {
            return defaultValue;
        }
        return result;
    }

    public sbyte readSByte(string key, sbyte defaultValue = 0)
    {
        if (!sbyte.TryParse(readString(key), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
        {
            return defaultValue;
        }
        return result;
    }

    public byte[] readByteArray(string key)
    {
        string s = readString(key);
        return Encoding.UTF8.GetBytes(s);
    }

    public short readInt16(string key, short defaultValue = 0)
    {
        if (!short.TryParse(readString(key), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
        {
            return defaultValue;
        }
        return result;
    }

    public ushort readUInt16(string key, ushort defaultValue = 0)
    {
        if (!ushort.TryParse(readString(key), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
        {
            return defaultValue;
        }
        return result;
    }

    public int readInt32(string key, int defaultValue = 0)
    {
        if (!int.TryParse(readString(key), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
        {
            return defaultValue;
        }
        return result;
    }

    public uint readUInt32(string key, uint defaultValue = 0u)
    {
        if (!uint.TryParse(readString(key), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
        {
            return defaultValue;
        }
        return result;
    }

    public long readInt64(string key, long defaultValue = 0L)
    {
        if (!long.TryParse(readString(key), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
        {
            return defaultValue;
        }
        return result;
    }

    public ulong readUInt64(string key, ulong defaultValue = 0uL)
    {
        if (!ulong.TryParse(readString(key), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
        {
            return defaultValue;
        }
        return result;
    }

    public float readSingle(string key, float defaultValue = 0f)
    {
        if (!float.TryParse(readString(key), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
        {
            return defaultValue;
        }
        return result;
    }

    public Vector3 readVector3(string key)
    {
        return new Vector3(readSingle(key + "_X"), readSingle(key + "_Y"), readSingle(key + "_Z"));
    }

    public Quaternion readQuaternion(string key)
    {
        return Quaternion.Euler(readByte(key + "_X", 0) * 2, (int)readByte(key + "_Y", 0), (int)readByte(key + "_Z", 0));
    }

    public Color readColor(string key)
    {
        return readColor(key, Color.black);
    }

    public Color readColor(string key, Color defaultColor)
    {
        return new Color(readSingle(key + "_R", defaultColor.r), readSingle(key + "_G", defaultColor.g), readSingle(key + "_B", defaultColor.b));
    }

    public Color32 ReadColor32RGB(string key, Color32 defaultValue)
    {
        return new Color32(readByte(key + "_R", defaultValue.r), readByte(key + "_G", defaultValue.g), readByte(key + "_B", defaultValue.b), byte.MaxValue);
    }

    public CSteamID readSteamID(string key)
    {
        return new CSteamID(readUInt64(key, 0uL));
    }

    public Guid readGUID(string key)
    {
        string text = readString(key);
        if (string.IsNullOrEmpty(text) || (text.Length == 1 && text[0] == '0'))
        {
            return Guid.Empty;
        }
        return new Guid(text);
    }

    public void ReadGuidOrLegacyId(string key, out Guid guid, out ushort legacyId)
    {
        if (data.TryGetValue(key, out var value) && !string.IsNullOrEmpty(value) && (value.Length != 1 || value[0] != '0'))
        {
            if (ushort.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out legacyId))
            {
                guid = Guid.Empty;
                return;
            }
            if (Guid.TryParse(value, out guid))
            {
                legacyId = 0;
                return;
            }
        }
        guid = Guid.Empty;
        legacyId = 0;
    }

    public ushort ReadGuidOrLegacyId(string key, out Guid guid)
    {
        ReadGuidOrLegacyId(key, out guid, out var legacyId);
        return legacyId;
    }

    public AssetReference<T> readAssetReference<T>(string key) where T : Asset
    {
        if (has(key))
        {
            return new AssetReference<T>(readGUID(key));
        }
        return AssetReference<T>.invalid;
    }

    public AssetReference<T> readAssetReference<T>(string key, in AssetReference<T> defaultValue) where T : Asset
    {
        if (has(key))
        {
            return new AssetReference<T>(readGUID(key));
        }
        return defaultValue;
    }

    private void ParseMasterBundleReference(string key, string value, out string name, out string path)
    {
        int num = value.IndexOf(':');
        if (num < 0)
        {
            if (Assets.currentMasterBundle != null)
            {
                name = Assets.currentMasterBundle.assetBundleName;
            }
            else
            {
                name = string.Empty;
                AddError("MasterBundleRef \"" + key + "\" is not associated with a master bundle nor does it specify one");
            }
            path = value;
            return;
        }
        name = value.Substring(0, num);
        path = value.Substring(num + 1);
        if (string.IsNullOrEmpty(name))
        {
            AddError("MasterBundleRef \"" + key + "\" specified asset bundle name is empty");
        }
        if (string.IsNullOrEmpty(path))
        {
            AddError("MasterBundleRef \"" + key + "\" specified asset path is empty");
        }
    }

    public MasterBundleReference<T> readMasterBundleReference<T>(string key) where T : UnityEngine.Object
    {
        if (TryReadString(key, out var value))
        {
            ParseMasterBundleReference(key, value, out var name, out var path);
            return new MasterBundleReference<T>(name, path);
        }
        return MasterBundleReference<T>.invalid;
    }

    public AudioReference ReadAudioReference(string key)
    {
        if (TryReadString(key, out var value))
        {
            ParseMasterBundleReference(key, value, out var name, out var path);
            return new AudioReference(name, path);
        }
        return default(AudioReference);
    }

    public void writeString(string key, string value)
    {
        data.Add(key, value);
    }

    public void writeBoolean(string key, bool value)
    {
        data.Add(key, value ? "y" : "n");
    }

    public void writeByte(string key, byte value)
    {
        data.Add(key, value.ToString());
    }

    public void writeByteArray(string key, byte[] value)
    {
        data.Add(key, Encoding.UTF8.GetString(value));
    }

    public void writeInt16(string key, short value)
    {
        data.Add(key, value.ToString());
    }

    public void writeUInt16(string key, ushort value)
    {
        data.Add(key, value.ToString());
    }

    public void writeInt32(string key, int value)
    {
        data.Add(key, value.ToString());
    }

    public void writeUInt32(string key, uint value)
    {
        data.Add(key, value.ToString());
    }

    public void writeInt64(string key, long value)
    {
        data.Add(key, value.ToString());
    }

    public void writeUInt64(string key, ulong value)
    {
        data.Add(key, value.ToString());
    }

    public void writeSingle(string key, float value)
    {
        data.Add(key, (Mathf.Floor(value * 100f) / 100f).ToString());
    }

    public void writeVector3(string key, Vector3 value)
    {
        writeSingle(key + "_X", value.x);
        writeSingle(key + "_Y", value.y);
        writeSingle(key + "_Z", value.z);
    }

    public void writeQuaternion(string key, Quaternion value)
    {
        Vector3 eulerAngles = value.eulerAngles;
        writeByte(key + "_X", MeasurementTool.angleToByte(eulerAngles.x));
        writeByte(key + "_Y", MeasurementTool.angleToByte(eulerAngles.y));
        writeByte(key + "_Z", MeasurementTool.angleToByte(eulerAngles.z));
    }

    public void writeColor(string key, Color value)
    {
        writeSingle(key + "_R", value.r);
        writeSingle(key + "_G", value.g);
        writeSingle(key + "_B", value.b);
    }

    public void writeSteamID(string key, CSteamID value)
    {
        writeUInt64(key, value.m_SteamID);
    }

    public void writeGUID(string key, Guid value)
    {
        writeString(key, value.ToString("N"));
    }

    public string getFile()
    {
        string text = "";
        char c = (isCSV ? ',' : ' ');
        foreach (KeyValuePair<string, string> datum in data)
        {
            text = text + datum.Key + c + datum.Value + "\n";
        }
        return text;
    }

    public string[] getLines()
    {
        string[] array = new string[data.Count];
        char c = (isCSV ? ',' : ' ');
        int num = 0;
        foreach (KeyValuePair<string, string> datum in data)
        {
            array[num] = datum.Key + c + datum.Value;
            num++;
        }
        return array;
    }

    public KeyValuePair<string, string>[] getContents()
    {
        KeyValuePair<string, string>[] array = new KeyValuePair<string, string>[data.Count];
        int num = 0;
        foreach (KeyValuePair<string, string> datum in data)
        {
            KeyValuePair<string, string> keyValuePair = (array[num] = datum);
            num++;
        }
        return array;
    }

    public string[] getValuesWithKey(string key)
    {
        List<string> list = new List<string>();
        foreach (KeyValuePair<string, string> datum in data)
        {
            if (datum.Key == key)
            {
                list.Add(datum.Value);
            }
        }
        return list.ToArray();
    }

    public string[] getKeysWithValue(string value)
    {
        List<string> list = new List<string>();
        foreach (KeyValuePair<string, string> datum in data)
        {
            if (datum.Value == value)
            {
                list.Add(datum.Key);
            }
        }
        return list.ToArray();
    }

    public bool has(string key)
    {
        return data.ContainsKey(key);
    }

    public void reset()
    {
        data.Clear();
    }

    public void log()
    {
        foreach (KeyValuePair<string, string> datum in data)
        {
            UnturnedLog.info("{0} = {1}", datum.Key, datum.Value);
        }
    }

    internal Data(StreamReader streamReader, SHA1Stream hashStream)
    {
        data = new Dictionary<string, string>();
        _ = string.Empty;
        string line;
        while ((line = streamReader.ReadLine()) != null)
        {
            ParseLine(line);
        }
        _hash = hashStream?.Hash;
    }

    public Data(string content)
    {
        data = new Dictionary<string, string>();
        StringReader stringReader = null;
        try
        {
            stringReader = new StringReader(content);
            _ = string.Empty;
            string line;
            while ((line = stringReader.ReadLine()) != null)
            {
                ParseLine(line);
            }
            _hash = Hash.SHA1(content);
        }
        catch (Exception innerException)
        {
            do
            {
                AddError("Caught exception: \"" + innerException.Message + "\"\n" + innerException.StackTrace);
                innerException = innerException.InnerException;
            }
            while (innerException != null);
            data.Clear();
            _hash = null;
        }
        finally
        {
            stringReader?.Close();
        }
    }

    public Data()
    {
        data = new Dictionary<string, string>();
        _hash = null;
    }

    private void AddError(string message)
    {
        if (errors == null)
        {
            errors = new List<string>();
        }
        errors.Add(message);
    }

    private void ParseLine(string line)
    {
        if (line.Length >= 1 && (line.Length <= 1 || line[0] != '/' || line[1] != '/'))
        {
            int num = line.IndexOf(' ');
            string text;
            string text2;
            if (num != -1)
            {
                text = line.Substring(0, num);
                text2 = line.Substring(num + 1, line.Length - num - 1);
            }
            else
            {
                text = line;
                text2 = string.Empty;
            }
            if (data.TryGetValue(text, out var value))
            {
                AddError("Duplicate key: \"" + text + "\" Old value: " + value + " New value: " + text2);
                data[text] = text2;
            }
            else
            {
                data.Add(text, text2);
            }
        }
    }
}
