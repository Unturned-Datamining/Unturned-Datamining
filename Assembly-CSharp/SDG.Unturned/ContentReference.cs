using System;
using SDG.Framework.IO.FormattedFiles;
using UnityEngine;

namespace SDG.Unturned;

public struct ContentReference<T> : IContentReference, IFormattedFileReadable, IFormattedFileWritable, IDatParseable, IEquatable<ContentReference<T>> where T : UnityEngine.Object
{
    public static ContentReference<T> invalid = new ContentReference<T>(null, null);

    public string name { get; set; }

    public string path { get; set; }

    public bool isValid
    {
        get
        {
            if (!string.IsNullOrEmpty(name))
            {
                return !string.IsNullOrEmpty(path);
            }
            return false;
        }
    }

    public bool TryParse(IDatNode node)
    {
        if (node is DatValue datValue)
        {
            if (string.IsNullOrEmpty(datValue.value))
            {
                return false;
            }
            if (datValue.value.Length < 2)
            {
                return false;
            }
            int num = datValue.value.IndexOf(':');
            if (num < 0)
            {
                if (Assets.currentMasterBundle != null)
                {
                    name = Assets.currentMasterBundle.assetBundleName;
                }
                path = datValue.value;
            }
            else
            {
                name = datValue.value.Substring(0, num);
                path = datValue.value.Substring(num + 1);
            }
            return true;
        }
        if (node is DatDictionary datDictionary)
        {
            name = datDictionary.GetString("Name");
            path = datDictionary.GetString("Path");
            return true;
        }
        return false;
    }

    public void read(IFormattedFileReader reader)
    {
        IFormattedFileReader formattedFileReader = reader.readObject();
        if (formattedFileReader == null)
        {
            if (Assets.currentMasterBundle != null)
            {
                name = Assets.currentMasterBundle.assetBundleName;
            }
            path = reader.readValue();
        }
        else
        {
            name = formattedFileReader.readValue("Name");
            path = formattedFileReader.readValue("Path");
        }
    }

    public void write(IFormattedFileWriter writer)
    {
        writer.beginObject();
        writer.writeValue("Name", name);
        writer.writeValue("Path", path);
        writer.endObject();
    }

    public static bool operator ==(ContentReference<T> a, ContentReference<T> b)
    {
        if (a.name == b.name)
        {
            return a.path == b.path;
        }
        return false;
    }

    public static bool operator !=(ContentReference<T> a, ContentReference<T> b)
    {
        return !(a == b);
    }

    public override int GetHashCode()
    {
        return name.GetHashCode() ^ path.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return false;
        }
        ContentReference<T> contentReference = (ContentReference<T>)obj;
        if (name == contentReference.name)
        {
            return path == contentReference.path;
        }
        return false;
    }

    public override string ToString()
    {
        return "#" + name + "::" + path;
    }

    public bool Equals(ContentReference<T> other)
    {
        if (name == other.name)
        {
            return path == other.path;
        }
        return false;
    }

    public ContentReference(string newName, string newPath)
    {
        name = newName;
        path = newPath;
    }

    public ContentReference(IContentReference contentReference)
    {
        name = contentReference.name;
        path = contentReference.path;
    }
}
