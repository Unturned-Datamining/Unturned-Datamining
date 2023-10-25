using System;
using SDG.Framework.IO.FormattedFiles;
using SDG.Framework.IO.FormattedFiles.KeyValueTables;

namespace SDG.Unturned;

public struct TypeReference<T> : ITypeReference, IFormattedFileReadable, IFormattedFileWritable, IDatParseable, IEquatable<TypeReference<T>>
{
    public static TypeReference<T> invalid = new TypeReference<T>((string)null);

    public string assemblyQualifiedName { get; set; }

    public Type type
    {
        get
        {
            if (!string.IsNullOrEmpty(assemblyQualifiedName) && assemblyQualifiedName.IndexOfAny(DatValue.INVALID_TYPE_CHARS) < 0)
            {
                return Type.GetType(assemblyQualifiedName);
            }
            return null;
        }
    }

    /// <summary>
    /// Whether the type has been asigned. Note that this doesn't mean an asset with <see cref="P:SDG.Unturned.TypeReference`1.assemblyQualifiedName" /> exists.
    /// </summary>
    public bool isValid => !string.IsNullOrEmpty(assemblyQualifiedName);

    /// <summary>
    /// True if resovling this type reference would get that type.
    /// </summary>
    public bool isReferenceTo(Type type)
    {
        if (type != null)
        {
            return assemblyQualifiedName == type.FullName;
        }
        return false;
    }

    public bool TryParse(IDatNode node)
    {
        if (node is DatValue datValue)
        {
            assemblyQualifiedName = datValue.value;
            return true;
        }
        if (node is DatDictionary datDictionary)
        {
            assemblyQualifiedName = datDictionary.GetString("Type");
            return true;
        }
        return false;
    }

    public void read(IFormattedFileReader reader)
    {
        reader = reader.readObject();
        if (reader != null)
        {
            assemblyQualifiedName = reader.readValue("Type");
            assemblyQualifiedName = KeyValueTableTypeRedirectorRegistry.chase(assemblyQualifiedName);
        }
    }

    public void write(IFormattedFileWriter writer)
    {
        writer.beginObject();
        writer.writeValue("Type", assemblyQualifiedName);
        writer.endObject();
    }

    public static bool operator ==(TypeReference<T> a, TypeReference<T> b)
    {
        return a.assemblyQualifiedName == b.assemblyQualifiedName;
    }

    public static bool operator !=(TypeReference<T> a, TypeReference<T> b)
    {
        return !(a == b);
    }

    public override int GetHashCode()
    {
        return assemblyQualifiedName.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return false;
        }
        TypeReference<T> typeReference = (TypeReference<T>)obj;
        return assemblyQualifiedName == typeReference.assemblyQualifiedName;
    }

    public override string ToString()
    {
        return assemblyQualifiedName;
    }

    public bool Equals(TypeReference<T> other)
    {
        return assemblyQualifiedName == other.assemblyQualifiedName;
    }

    public TypeReference(string assemblyQualifiedName)
    {
        this.assemblyQualifiedName = assemblyQualifiedName;
    }

    public TypeReference(ITypeReference typeReference)
    {
        assemblyQualifiedName = typeReference.assemblyQualifiedName;
    }
}
