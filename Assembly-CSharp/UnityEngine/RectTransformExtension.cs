namespace UnityEngine;

public static class RectTransformExtension
{
    public static void reset(this RectTransform transform)
    {
        transform.anchorMin = Vector2.zero;
        transform.anchorMax = Vector2.one;
        transform.offsetMin = Vector2.zero;
        transform.offsetMax = Vector2.zero;
        transform.localScale = Vector3.one;
        transform.ForceUpdateRectTransforms();
    }

    public static Rect GetAbsoluteRect(this RectTransform transform)
    {
        Vector2 vector = Vector2.Scale(transform.rect.size, transform.lossyScale);
        Rect result = new Rect(transform.position.x, (float)Screen.height - transform.position.y, vector.x, vector.y);
        result.x -= transform.pivot.x * vector.x;
        result.y -= (1f - transform.pivot.y) * vector.y;
        return result;
    }
}
