using System;
using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Thanks to Glenn Fiedler for this RK4 implementation article:
/// https://gafferongames.com/post/integration_basics/
/// </summary>
[Serializable]
public struct Rk4SpringQ
{
    private struct Rk4DerivativeQ
    {
        public Vector3 velocity;

        public Vector3 acceleration;
    }

    public Quaternion currentRotation;

    public Quaternion targetRotation;

    /// <summary>
    /// Higher values return to the target position faster.
    /// </summary>
    public float stiffness;

    /// <summary>
    /// Higher values reduce bounciness and settle at the target position faster.
    /// e.g. a value of zero will bounce back and forth for a long time (indefinitely?)
    /// </summary>
    public float damping;

    private Vector3 currentVelocity;

    public Rk4SpringQ(float stiffness, float damping)
    {
        currentRotation = Quaternion.identity;
        targetRotation = Quaternion.identity;
        this.stiffness = stiffness;
        this.damping = damping;
        currentVelocity = default(Vector3);
    }

    public void Update(float deltaTime)
    {
        while (deltaTime > 0.05f)
        {
            PrivateUpdate(0.05f);
            deltaTime -= 0.05f;
        }
        if (deltaTime > 0f)
        {
            PrivateUpdate(deltaTime);
        }
    }

    private void PrivateUpdate(float deltaTime)
    {
        Rk4DerivativeQ initialDerivative = Evaluate(0f, default(Rk4DerivativeQ));
        Rk4DerivativeQ initialDerivative2 = Evaluate(deltaTime * 0.5f, initialDerivative);
        Rk4DerivativeQ initialDerivative3 = Evaluate(deltaTime * 0.5f, initialDerivative2);
        Rk4DerivativeQ rk4DerivativeQ = Evaluate(deltaTime, initialDerivative3);
        Vector3 vector = 1f / 6f * (initialDerivative.velocity + 2f * (initialDerivative2.velocity + initialDerivative3.velocity) + rk4DerivativeQ.velocity);
        Vector3 vector2 = 1f / 6f * (initialDerivative.acceleration + 2f * (initialDerivative2.acceleration + initialDerivative3.acceleration) + rk4DerivativeQ.acceleration);
        currentRotation *= Quaternion.Euler(vector * deltaTime);
        currentVelocity += vector2 * deltaTime;
    }

    private Rk4DerivativeQ Evaluate(float deltaTime, Rk4DerivativeQ initialDerivative)
    {
        Quaternion rotation = currentRotation * Quaternion.Euler(initialDerivative.velocity * deltaTime);
        Vector3 vector = currentVelocity + initialDerivative.acceleration * deltaTime;
        (Quaternion.Inverse(rotation) * targetRotation).ToAngleAxis(out var angle, out var axis);
        Rk4DerivativeQ result = default(Rk4DerivativeQ);
        result.velocity = vector;
        result.acceleration = stiffness * axis * angle - damping * vector;
        return result;
    }
}
