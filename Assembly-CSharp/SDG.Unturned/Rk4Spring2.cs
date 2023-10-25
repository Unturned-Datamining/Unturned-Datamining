using System;
using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Thanks to Glenn Fiedler for this RK4 implementation article:
/// https://gafferongames.com/post/integration_basics/
/// </summary>
[Serializable]
public struct Rk4Spring2
{
    private struct Rk4Derivative2
    {
        public Vector2 velocity;

        public Vector2 acceleration;
    }

    public Vector2 currentPosition;

    public Vector2 targetPosition;

    /// <summary>
    /// Higher values return to the target position faster.
    /// </summary>
    public float stiffness;

    /// <summary>
    /// Higher values reduce bounciness and settle at the target position faster.
    /// e.g. a value of zero will bounce back and forth for a long time (indefinitely?)
    /// </summary>
    public float damping;

    private Vector2 currentVelocity;

    public Rk4Spring2(float stiffness, float damping)
    {
        currentPosition = default(Vector2);
        targetPosition = default(Vector2);
        this.stiffness = stiffness;
        this.damping = damping;
        currentVelocity = default(Vector2);
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
        Rk4Derivative2 initialDerivative = Evaluate(0f, default(Rk4Derivative2));
        Rk4Derivative2 initialDerivative2 = Evaluate(deltaTime * 0.5f, initialDerivative);
        Rk4Derivative2 initialDerivative3 = Evaluate(deltaTime * 0.5f, initialDerivative2);
        Rk4Derivative2 rk4Derivative = Evaluate(deltaTime, initialDerivative3);
        Vector2 vector = 1f / 6f * (initialDerivative.velocity + 2f * (initialDerivative2.velocity + initialDerivative3.velocity) + rk4Derivative.velocity);
        Vector2 vector2 = 1f / 6f * (initialDerivative.acceleration + 2f * (initialDerivative2.acceleration + initialDerivative3.acceleration) + rk4Derivative.acceleration);
        currentPosition += vector * deltaTime;
        currentVelocity += vector2 * deltaTime;
    }

    private Rk4Derivative2 Evaluate(float deltaTime, Rk4Derivative2 initialDerivative)
    {
        Vector2 vector = currentPosition + initialDerivative.velocity * deltaTime;
        Rk4Derivative2 result = default(Rk4Derivative2);
        Vector2 vector2 = (result.velocity = currentVelocity + initialDerivative.acceleration * deltaTime);
        result.acceleration = stiffness * (targetPosition - vector) - damping * vector2;
        return result;
    }
}
