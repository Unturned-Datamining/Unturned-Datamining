namespace System;

public static class TypeExtension
{
    public static object getDefaultValue(this Type type)
    {
        if (type.IsValueType)
        {
            return Activator.CreateInstance(type);
        }
        return null;
    }
}
