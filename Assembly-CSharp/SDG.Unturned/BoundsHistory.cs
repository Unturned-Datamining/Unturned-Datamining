using UnityEngine;

namespace SDG.Unturned;

internal class BoundsHistory
{
    private Bounds[] buffer;

    private int writeIndex;

    private int writeCount;

    private float expansion;

    /// <summary>
    /// Half second history at 50 tickrate.
    /// </summary>
    private const int CAPACITY = 25;

    public float Expansion
    {
        get
        {
            return expansion;
        }
        set
        {
            expansion = value;
        }
    }

    public BoundsHistory()
    {
        buffer = new Bounds[25];
        writeIndex = 0;
        writeCount = 0;
    }

    public void Clear()
    {
        writeIndex = 0;
        writeCount = 0;
    }

    public void AddBounds(Bounds bounds)
    {
        buffer[writeIndex] = bounds;
        writeIndex = (writeIndex + 1) % 25;
        writeCount = Mathf.Min(writeCount + 1, 25);
    }

    public void AddCharacterControllerBounds(CharacterController characterController)
    {
        AddBounds(CalculateCharacterControllerBounds(characterController));
    }

    /// <summary>
    /// Tests whether current or recent history contains point.
    /// </summary>
    public bool ContainsPoint(CharacterController characterController, Vector3 point)
    {
        Bounds bounds = CalculateCharacterControllerBounds(characterController);
        if (bounds.Contains(point))
        {
            return true;
        }
        if (writeCount < 1)
        {
            return false;
        }
        Bounds bounds2 = bounds;
        int num = writeIndex;
        int num2 = 0;
        do
        {
            Bounds bounds3 = buffer[num];
            bounds2.Encapsulate(bounds3);
            if (bounds2.Contains(point))
            {
                return true;
            }
            bounds2 = bounds3;
            num--;
            if (num < 0)
            {
                num = 24;
            }
            num2++;
        }
        while (num2 < writeCount);
        return false;
    }

    public Bounds CalculateCharacterControllerBounds(CharacterController characterController)
    {
        characterController.transform.GetPositionAndRotation(out var position, out var rotation);
        return CalculateCharacterControllerBounds(position, rotation, characterController.center, characterController.radius, characterController.height);
    }

    public Bounds CalculateCharacterControllerBounds(Vector3 position, Quaternion rotation, Vector3 center, float radius, float height)
    {
        float num = Mathf.Max(0f, height * 0.5f - radius);
        Vector3 vector = position + rotation * (center + new Vector3(0f, 0f - num, 0f));
        Vector3 vector2 = position + rotation * (center + new Vector3(0f, num, 0f));
        float num2 = (expansion + radius) * 2f;
        Vector3 center2 = (vector + vector2) * 0.5f;
        Vector3 size = (vector2 - vector).GetAbs() + new Vector3(num2, num2, num2);
        return new Bounds(center2, size);
    }

    internal void DrawGizmos()
    {
        if (writeCount < 1)
        {
            return;
        }
        RuntimeGizmos runtimeGizmos = RuntimeGizmos.Get();
        Bounds bounds = default(Bounds);
        int num = writeIndex;
        int num2 = 0;
        do
        {
            Bounds bounds2 = buffer[num];
            runtimeGizmos.Box(bounds2.center, bounds2.size, Color.red);
            if (num2 > 0)
            {
                bounds.Encapsulate(bounds2);
                runtimeGizmos.Box(bounds.center, bounds.size, Color.yellow);
            }
            bounds = bounds2;
            num--;
            if (num < 0)
            {
                num = 24;
            }
            num2++;
        }
        while (num2 < writeCount);
    }
}
