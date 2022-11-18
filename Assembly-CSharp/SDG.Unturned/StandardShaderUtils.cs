using UnityEngine;

namespace SDG.Unturned;

public static class StandardShaderUtils
{
    public static bool isNameStandard(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }
        if (name.StartsWith("Standard"))
        {
            if (name.Length != 8 && !name.EndsWith(" (Decalable)"))
            {
                return name.EndsWith(" (Specular setup)");
            }
            return true;
        }
        return false;
    }

    public static bool isMaterialUsingStandardShader(Material material)
    {
        if (material != null && material.shader != null)
        {
            return isNameStandard(material.shader.name);
        }
        return false;
    }

    public static bool isModeFade(Material material)
    {
        return material.IsKeywordEnabled("_ALPHABLEND_ON");
    }

    public static bool isModeTransparent(Material material)
    {
        return material.IsKeywordEnabled("_ALPHAPREMULTIPLY_ON");
    }

    public static void setModeToOpaque(Material material)
    {
        material.SetFloat("_Mode", 0f);
        material.SetOverrideTag("RenderType", "");
        material.SetInt("_SrcBlend", 1);
        material.SetInt("_DstBlend", 0);
        material.SetInt("_ZWrite", 1);
        material.DisableKeyword("_ALPHATEST_ON");
        material.DisableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = -1;
    }

    public static void setModeToCutout(Material material)
    {
        material.SetFloat("_Mode", 1f);
        material.SetOverrideTag("RenderType", "TransparentCutout");
        material.SetInt("_SrcBlend", 1);
        material.SetInt("_DstBlend", 0);
        material.SetInt("_ZWrite", 1);
        material.EnableKeyword("_ALPHATEST_ON");
        material.DisableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 2450;
    }

    public static void setModeToFade(Material material)
    {
        material.SetFloat("_Mode", 2f);
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_SrcBlend", 5);
        material.SetInt("_DstBlend", 10);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;
    }

    public static void setModeToTransparent(Material material)
    {
        material.SetFloat("_Mode", 3f);
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_SrcBlend", 1);
        material.SetInt("_DstBlend", 10);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.DisableKeyword("_ALPHABLEND_ON");
        material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;
    }

    public static void fixupEmission(Material material)
    {
        if (material.GetColor("_EmissionColor").maxColorComponent < 0.01f)
        {
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
        }
        else
        {
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
        bool flag = (material.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack) == 0;
        if (flag)
        {
            flag = material.GetTexture("_EmissionMap") != null;
        }
        if (flag)
        {
            material.EnableKeyword("_EMISSION");
        }
        else
        {
            material.DisableKeyword("_EMISSION");
        }
    }

    public static bool maybeFixupMaterial(Material material)
    {
        if (!isMaterialUsingStandardShader(material))
        {
            return false;
        }
        bool flag;
        if (isModeFade(material))
        {
            setModeToFade(material);
            flag = true;
        }
        else if (isModeTransparent(material))
        {
            setModeToTransparent(material);
            flag = true;
        }
        else
        {
            flag = false;
        }
        if (flag)
        {
            fixupEmission(material);
        }
        return true;
    }
}
