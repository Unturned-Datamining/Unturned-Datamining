using System;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Rendering;

public class GLRenderer : MonoBehaviour
{
    public static event GLRenderHandler render;

    public static event GLRenderHandler OnGameRender;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination);
        bool flag = false;
        bool flag2 = false;
        bool flag3 = false;
        if (Level.isEditor)
        {
            flag2 = GLRenderer.render != null;
            if (EditorUI.window == null || !EditorUI.window.isEnabled)
            {
                flag2 = false;
            }
            flag = flag || flag2;
        }
        else
        {
            flag3 = GLRenderer.OnGameRender != null;
            if (PlayerUI.window == null || !PlayerUI.window.isEnabled)
            {
                flag3 = false;
            }
            flag = flag || flag3;
        }
        bool hasQueuedElements = RuntimeGizmos.Get().HasQueuedElements;
        if (!(flag || hasQueuedElements))
        {
            return;
        }
        RenderTexture.active = destination;
        if (flag2)
        {
            GL.PushMatrix();
            try
            {
                GLRenderer.render();
            }
            catch (Exception e)
            {
                UnturnedLog.exception(e);
            }
            GL.PopMatrix();
        }
        if (flag3)
        {
            GL.PushMatrix();
            try
            {
                GLRenderer.OnGameRender();
            }
            catch (Exception e2)
            {
                UnturnedLog.exception(e2);
            }
            GL.PopMatrix();
        }
        if (hasQueuedElements)
        {
            GL.PushMatrix();
            try
            {
                RuntimeGizmos.Get().Render();
            }
            catch (Exception e3)
            {
                UnturnedLog.exception(e3);
            }
            GL.PopMatrix();
        }
        RenderTexture.active = null;
    }
}
