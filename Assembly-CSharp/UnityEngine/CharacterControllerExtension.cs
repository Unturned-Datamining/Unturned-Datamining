using System.Collections.Generic;
using SDG.Framework.Utilities;

namespace UnityEngine;

public static class CharacterControllerExtension
{
    private struct PendingEnableRigidbody
    {
        public CharacterController component;

        public int frameNumber;

        public PendingEnableRigidbody(CharacterController component)
        {
            this.component = component;
            frameNumber = Time.frameCount + 1;
        }
    }

    private const int CHECKED_MOVE_BUFFER_SIZE = 8;

    private static Collider[] initialOverlaps;

    private static RaycastHit[] results;

    private static List<PendingEnableRigidbody> pendingChanges;

    private static bool wasHitInitialOverlap(RaycastHit hit, int initialOverlapCount)
    {
        for (int i = 0; i < initialOverlapCount; i++)
        {
            if (hit.collider == initialOverlaps[i])
            {
                return true;
            }
        }
        return false;
    }

    private static bool wereAllHitsInitialOverlaps(int hitCount, int initialOverlapCount)
    {
        for (int i = 0; i < hitCount; i++)
        {
            if (!wasHitInitialOverlap(results[i], initialOverlapCount))
            {
                return false;
            }
        }
        return true;
    }

    public static void CheckedMove(this CharacterController component, Vector3 motion)
    {
        Vector3 position = component.transform.position;
        component.Move(motion);
        Vector3 vector = component.transform.position - position;
        float sqrMagnitude = vector.sqrMagnitude;
        if (sqrMagnitude < 1E-05f)
        {
            return;
        }
        float num = Mathf.Sqrt(sqrMagnitude);
        Vector3 direction = vector / num;
        float num2 = component.height / 3f;
        float num3 = component.radius / 2f;
        Vector3 vector2 = new Vector3(0f, num2 - num3, 0f);
        Vector3 vector3 = position + component.center;
        Vector3 vector4 = vector3 - vector2;
        Vector3 vector5 = vector3 + vector2;
        int layerMask = 406437888;
        int num4 = Physics.OverlapCapsuleNonAlloc(vector4, vector5, num3, initialOverlaps, layerMask, QueryTriggerInteraction.Ignore);
        if (num4 < initialOverlaps.Length)
        {
            int num5 = Physics.CapsuleCastNonAlloc(vector4, vector5, num3, direction, results, num, layerMask, QueryTriggerInteraction.Ignore);
            if (num5 < results.Length && num5 > 0 && (num4 <= 0 || !wereAllHitsInitialOverlaps(num5, num4)))
            {
                component.transform.position = position;
            }
        }
    }

    private static void removePendingChange(CharacterController component)
    {
        for (int num = pendingChanges.Count - 1; num >= 0; num--)
        {
            if (pendingChanges[num].component == component)
            {
                pendingChanges.RemoveAtFast(num);
                break;
            }
        }
    }

    public static void DisableDetectCollisions(this CharacterController component)
    {
        component.detectCollisions = false;
        removePendingChange(component);
    }

    public static void EnableDetectCollisionsNextFrame(this CharacterController component)
    {
        removePendingChange(component);
        pendingChanges.Add(new PendingEnableRigidbody(component));
    }

    public static void SetDetectCollisionsDeferred(this CharacterController component, bool detectCollisions)
    {
        if (detectCollisions)
        {
            component.EnableDetectCollisionsNextFrame();
        }
        else
        {
            component.DisableDetectCollisions();
        }
    }

    public static void DisableDetectCollisionsUntilNextFrame(this CharacterController component)
    {
        component.DisableDetectCollisions();
        component.EnableDetectCollisionsNextFrame();
    }

    private static void OnUpdate()
    {
        int frameCount = Time.frameCount;
        for (int num = pendingChanges.Count - 1; num >= 0; num--)
        {
            if (frameCount >= pendingChanges[num].frameNumber)
            {
                CharacterController component = pendingChanges[num].component;
                if (component != null)
                {
                    component.detectCollisions = true;
                }
                pendingChanges.RemoveAtFast(num);
            }
        }
    }

    static CharacterControllerExtension()
    {
        initialOverlaps = new Collider[8];
        results = new RaycastHit[8];
        pendingChanges = new List<PendingEnableRigidbody>();
        TimeUtility.updated += OnUpdate;
    }
}
