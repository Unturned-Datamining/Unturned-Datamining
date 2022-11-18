using System;
using SDG.Framework.IO.FormattedFiles;
using SDG.Framework.IO.FormattedFiles.KeyValueTables;

namespace SDG.Unturned;

public struct TypeReference<T> : ITypeReference, IFormattedFileReadable, IFormattedFileWritable, IEquatable<TypeReference<T>>
{
    public static TypeReference<T> invalid = new TypeReference<T>((string)null);

    public string assemblyQualifiedName { get; set; }

    public Type type
    {
        get
        {
            if (!string.IsNullOrEmpty(assemblyQualifiedName))
            {
                return Type.GetType(assemblyQualifiedName);
            }
            return null;
        }
    }

    public bool isValid => !string.IsNullOrEmpty(assemblyQualifiedName);

    public bool isReferenceTo(Type type)
    {
        if (type != null)
        {
            return assemblyQualifiedName == type.FullName;
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
