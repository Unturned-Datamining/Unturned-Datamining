using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public static class ShaderConsolidator
{
    private static readonly Dictionary<string, string> SHADER_REDIRECTS = new Dictionary<string, string>
    {
        { "Particles/Additive", "Legacy Shaders/Particles/Additive" },
        { "Particles/Additive (Soft)", "Legacy Shaders/Particles/Additive (Soft)" },
        { "Particles/Alpha Blended", "Legacy Shaders/Particles/Alpha Blended" },
        { "Particles/Anim Alpha Blended", "Legacy Shaders/Particles/Anim Alpha Blended" },
        { "Particles/Alpha Blended Premultiply", "Legacy Shaders/Particles/Alpha Blended Premultiply" }
    };

    public static Shader findConsolidatedShader(Shader originalShader)
    {
        if (originalShader == null)
        {
            return null;
        }
        string name = originalShader.name;
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }
        throw new Exception($"Dedicated server trying to consolidate '{name}' shader");
    }

    public static string redirectShaderName(string shaderName)
    {
        if (SHADER_REDIRECTS.TryGetValue(shaderName, out var value))
        {
            return redirectShaderName(value);
        }
        return shaderName;
    }
}
