using System;
using UnityEngine;
using UnityEngine.UI;

namespace SDG.Unturned;

public class WebImage : MonoBehaviour
{
    public Image targetImage;

    public string url;

    public bool shouldCache = true;

    /// <summary>
    /// If set, we are responsible for destroying texture.
    /// </summary>
    protected Texture2D texture;

    protected Sprite sprite;

    public void setAddressAndRefresh(string newURL, bool newShouldCache, bool forceRefresh)
    {
        if (forceRefresh)
        {
            Provider.destroyCachedIcon(newURL);
        }
        else if (url != null && shouldCache && newShouldCache && url.Equals(newURL, StringComparison.InvariantCultureIgnoreCase))
        {
            return;
        }
        url = newURL;
        shouldCache = newShouldCache;
        Refresh();
    }

    private void onImageReady(Texture2D texture, bool responsibleForDestroy)
    {
        cleanupResources();
        if (responsibleForDestroy)
        {
            this.texture = texture;
        }
        targetImage.enabled = true;
        if (texture != null)
        {
            sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            sprite.name = texture.name + "Sprite";
            sprite.hideFlags = HideFlags.HideAndDontSave;
            targetImage.sprite = sprite;
            AspectRatioFitter component = GetComponent<AspectRatioFitter>();
            if (component != null)
            {
                component.aspectRatio = (float)texture.width / (float)texture.height;
            }
        }
    }

    public void Refresh()
    {
        if (!(targetImage == null))
        {
            targetImage.enabled = false;
            if (!string.IsNullOrEmpty(url))
            {
                Provider.refreshIcon(new Provider.IconQueryParams(url, onImageReady, shouldCache));
            }
        }
    }

    protected void cleanupResources()
    {
        if (texture != null)
        {
            UnityEngine.Object.Destroy(texture);
            texture = null;
        }
        if (sprite != null)
        {
            UnityEngine.Object.Destroy(sprite);
            sprite = null;
        }
    }

    protected virtual void Start()
    {
        Refresh();
    }

    protected void OnDestroy()
    {
        cleanupResources();
    }
}
