using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace SDG.Unturned;

public sealed class SrScopeRenderer : PostProcessEffectRenderer<SrScope>
{
    private Shader gaussianBlurShader;

    private int scopeBlurTexId;

    private int standardDeviationId;

    private int halfKernelSizeId;

    public override void Init()
    {
        base.Init();
        gaussianBlurShader = Shader.Find("Hidden/Custom/GaussianBlur");
        scopeBlurTexId = Shader.PropertyToID("_ScopeBlurTex");
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
        context.GetScreenSpaceTemporaryRT(context.command, scopeBlurTexId);
        PropertySheet propertySheet = context.propertySheets.Get(gaussianBlurShader);
        propertySheet.properties.SetFloat(standardDeviationId, (float)base.settings.standardDeviation * (float)base.settings.standardDeviation);
        propertySheet.properties.SetInt(halfKernelSizeId, Mathf.CeilToInt((float)base.settings.standardDeviation * 3f));
        context.command.BlitFullscreenTriangle(context.source, scopeBlurTexId, propertySheet, 0);
        context.command.BlitFullscreenTriangle(scopeBlurTexId, context.destination, propertySheet, 1);
        context.command.ReleaseTemporaryRT(scopeBlurTexId);
    }
}
