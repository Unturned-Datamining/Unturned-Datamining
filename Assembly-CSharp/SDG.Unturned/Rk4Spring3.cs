using System;
using UnityEngine;

namespace SDG.Unturned;

[Serializable]
public struct Rk4Spring3
{
    private struct Rk4Derivative3
    {
        public Vector3 velocity;

        public Vector3 acceleration;
    }

    public Vector3 currentPosition;

    public Vector3 targetPosition;

    public float stiffness;

    public float damping;

    private Vector3 currentVelocity;

    public Rk4Spring3(float stiffness, float damping)
    {
        currentPosition = default(Vector3);
        targetPosition = default(Vector3);
        this.stiffness = stiffness;
        this.damping = damping;
        currentVelocity = default(Vector3);
    }

    public void Update(float deltaTime)
    {
        Rk4Derivative3 initialDerivative = Evaluate(0f, default(Rk4Derivative3));
        Rk4Derivative3 initialDerivative2 = Evaluate(deltaTime * 0.5f, initialDerivative);
        Rk4Derivative3 initialDerivative3 = Evaluate(deltaTime * 0.5f, initialDerivative2);
        Rk4Derivative3 rk4Derivative = Evaluate(deltaTime, initialDerivative3);
        Vector3 vector = 1f / 6f * (initialDerivative.velocity + 2f * (initialDerivative2.velocity + initialDerivative3.velocity) + rk4Derivative.velocity);
        Vector3 vector2 = 1f / 6f * (initialDerivative.acceleration + 2f * (initialDerivative2.acceleration + initialDerivative3.acceleration) + rk4Derivative.acceleration);
        currentPosition += vector * deltaTime;
        currentVelocity += vector2 * deltaTime;
    }

    private Rk4Derivative3 Evaluate(float deltaTime, Rk4Derivative3 initialDerivative)
    {
        Vector3 vector = currentPosition + initialDerivative.velocity * deltaTime;
        Rk4Derivative3 result = default(Rk4Derivative3);
        Vector3 vector2 = (result.velocity = currentVelocity + initialDerivative.acceleration * deltaTime);
        result.acceleration = stiffness * (targetPosition - vector) - damping * vector2;
        return result;
    }
}
