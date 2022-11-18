using System;
using System.Collections.Generic;

namespace SDG.Unturned;

public class TypeRegistryDictionary
{
    private Type baseType;

    private Dictionary<string, Type> typesDictionary = new Dictionary<string, Type>();

    public Type getType(string key)
    {
        Type value = null;
        typesDictionary.TryGetValue(key, out value);
        return value;
    }

    public void addType(string key, Type value)
    {
        if (baseType.IsAssignableFrom(value))
        {
            typesDictionary.Add(key, value);
            return;
        }
        throw new ArgumentException(baseType?.ToString() + " is not assignable from " + value, "value");
    }

    public void removeType(string key)
    {
        typesDictionary.Remove(key);
    }

    public TypeRegistryDictionary(Type newBaseType)
    {
        baseType = newBaseType;
    }
}
