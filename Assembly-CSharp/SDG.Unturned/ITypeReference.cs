using System;

namespace SDG.Unturned;

public interface ITypeReference
{
    /// <summary>
    /// GUID of the asset this is referring to.
    /// </summary>
    string assemblyQualifiedName { get; set; }

    Type type { get; }

    bool isValid { get; }
}
