using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace SDG.Unturned;

public sealed class SrScopeRenderer : PostProcessEffectRenderer<SrScope>
{
    private Shader gaussianBlurShader;

    private Shader vignetteShader;

    private int scopeBlurTexId;

    private int scopeAlphaId;

    private int standardDeviationId;

    private int halfKernelSizeId;

    public override void Init()
    {
        base.Init();
        gaussianBlurShader = Shader.Find("Hidden/Custom/GaussianBlur");
        vignetteShader = Shader.Find("Hidden/Custom/ScopeVignette");
        scopeBlurTexId = Shader.PropertyToID("_ScopeBlurTex");
        scopeAlphaId = Shader.PropertyToID("_ScopeAlpha");
        standardDeviationId = Shader.PropertyToID("_StdDeviationSquared");
        halfKernelSizeId = Shader.PropertyToID("_HalfKernelSize");
    }

    public override void Render(PostProcessRenderContext context)
    {
        RenderTexture renderTexture = (RenderTexture)base.settings.renderTarget.value;
        Vector2 vector = new Vector2(Screen.width, Screen.height);
        int num = ((!(vector.x < vector.y)) ? 1 : 0);
        int index = 1 - num;
        Vector2 scale = new Vector2(1f, 1f);
        scale[index] = vector[num] / vector[index];
        Vector2 offset = new Vector2(0f, 0f);
        offset[index] = (scale[index] - 1f) * -0.5f;
        context.command.Blit(context.source, renderTexture, scale, offset);
        bool flag = false;
        if (GraphicsSettings.WantsDarkScopePeripheral)
        {
            if ((float)base.settings.scopeAlpha > 0.001f)
            {
                PropertySheet propertySheet = context.propertySheets.Get(vignetteShader);
                propertySheet.properties.SetFloat(scopeAlphaId, base.settings.scopeAlpha);
                context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 0);
            }
            else
            {
                flag = true;
            }
        }
        else if ((float)base.settings.standardDeviation > 0.001f)
        {
            float num2 = vector[num] / 1080f;
            float num3 = (float)base.settings.standardDeviation * num2;
            float value = num3 * num3;
            context.GetScreenSpaceTemporaryRT(context.command, scopeBlurTexId);
            PropertySheet propertySheet2 = context.propertySheets.Get(gaussianBlurShader);
            propertySheet2.properties.SetFloat(standardDeviationId, value);
            propertySheet2.properties.SetInt(halfKernelSizeId, Mathf.CeilToInt(num3 * 3f));
            context.command.BlitFullscreenTriangle(context.source, scopeBlurTexId, propertySheet2, 0);
            context.command.BlitFullscreenTriangle(scopeBlurTexId, context.destination, propertySheet2, 1);
            context.command.ReleaseTemporaryRT(scopeBlurTexId);
        }
        else
        {
            flag = true;
        }
        if (flag)
        {
            context.command.Blit(context.source, context.destination);
        }
    }
}
