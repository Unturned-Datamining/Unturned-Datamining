using System;
using UnityEngine;

namespace SDG.Framework.Foliage;

internal struct FoliageInstancingBatchConfig : IEquatable<FoliageInstancingBatchConfig>
{
    public Mesh mesh;

    public Material material;

    public bool castShadows;

    public int hashCode;

    public override int GetHashCode()
    {
        return hashCode;
    }

    public bool Equals(FoliageInstancingBatchConfig other)
    {
        if (mesh == other.mesh && material == other.material)
        {
            return castShadows == other.castShadows;
        }
        return false;
    }

    public override string ToString()
    {
        return $"(Mesh: {mesh} Material: {material} Shadows: {castShadows})";
    }

    public FoliageInstancingBatchConfig(Mesh mesh, Material material, bool castShadows)
    {
        this.mesh = mesh;
        this.material = material;
        this.castShadows = castShadows;
        hashCode = HashCode.Combine(mesh, material, castShadows);
    }
}
