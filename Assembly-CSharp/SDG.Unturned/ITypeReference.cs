using System;

namespace SDG.Unturned;

public interface ITypeReference
{
    string assemblyQualifiedName { get; set; }

    Type type { get; }

    bool isValid { get; }
}
