using System;
using SDG.Framework.IO.FormattedFiles;

namespace SDG.Unturned;

public struct AssetReference<T> : IAssetReference, IFormattedFileReadable, IFormattedFileWritable, IEquatable<AssetReference<T>> where T : Asset
{
    public static AssetReference<T> invalid = new AssetReference<T>(Guid.Empty);

    private Guid _guid;

    public Guid GUID
    {
        get
        {
            return _guid;
        }
        set
        {
            _guid = value;
        }
    }

    public bool isValid => GUID != Guid.Empty;

    public bool isNull => GUID == Guid.Empty;

    public bool isReferenceTo(Asset asset)
    {
        if (asset != null)
        {
            return GUID == asset.GUID;
        }
        return false;
    }

    public T Find()
    {
        return Assets.find(this);
    }

    [Obsolete("Renamed to Find because Get might imply that asset is cached")]
    public T get()
    {
        return Assets.find(this);
    }

    public void read(IFormattedFileReader reader)
    {
        IFormattedFileReader formattedFileReader = reader.readObject();
        if (formattedFileReader == null)
        {
            GUID = reader.readValue<Guid>();
        }
        else
        {
            GUID = formattedFileReader.readValue<Guid>("GUID");
        }
    }

    public void write(IFormattedFileWriter writer)
    {
        writer.beginObject();
        writer.writeValue("GUID", GUID);
        writer.endObject();
    }

    public static bool TryParse(string input, out AssetReference<T> result)
    {
        if (Guid.TryParse(input, out var result2))
        {
            result = new AssetReference<T>(result2);
            return true;
        }
        result = invalid;
        return false;
    }

    public static bool operator ==(AssetReference<T> a, AssetReference<T> b)
    {
        return a.GUID == b.GUID;
    }

    public static bool operator !=(AssetReference<T> a, AssetReference<T> b)
    {
        return !(a == b);
    }

    public override int GetHashCode()
    {
        return GUID.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return false;
        }
        AssetReference<T> assetReference = (AssetReference<T>)obj;
        return GUID.Equals(assetReference.GUID);
    }

    public override string ToString()
    {
        return GUID.ToString("N");
    }

    public bool Equals(AssetReference<T> other)
    {
        return GUID.Equals(other.GUID);
    }

    public AssetReference(Guid GUID)
    {
        this.GUID = GUID;
    }

    public AssetReference(string GUID)
    {
        Guid.TryParse(GUID, out _guid);
    }

    public AssetReference(IAssetReference assetReference)
    {
        GUID = assetReference.GUID;
    }
}
