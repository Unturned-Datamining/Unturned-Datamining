using UnityEngine;

namespace SDG.Unturned;

public class Wheel
{
    private InteractableVehicle _vehicle;

    private WheelCollider _wheel;

    public Transform model;

    public Quaternion rest;

    public WheelFrictionCurve forwardFriction;

    public WheelFrictionCurve sidewaysFriction;

    private bool _isSteered;

    public bool isPowered;

    public bool hasBrakes;

    private bool _isGrounded;

    protected bool _isAlive;

    public bool isAnimationSteered;

    public float stiffnessTractionMultiplier = 0.25f;

    public float stiffnessSideways = 1f;

    public float stiffnessForward = 2f;

    public float motorTorqueMultiplier = 1f;

    public float motorTorqueClampMultiplier = 0.5f;

    public float brakeTorqueMultiplier = 1f;

    public float brakeTorqueTractionMultiplier = 0.5f;

    private float direction;

    private float steer;

    private float speed;

    protected bool _isPhysical;

    private static readonly AssetReference<EffectAsset> Rubber_0_Ref = new AssetReference<EffectAsset>("a87c5007b22542dcbf3599ee3faceadd");

    public InteractableVehicle vehicle => _vehicle;

    public int index { get; protected set; }

    public WheelCollider wheel => _wheel;

    public bool isSteered => _isSteered;

    public bool isGrounded => _isGrounded;

    public bool isAlive
    {
        get
        {
            return _isAlive;
        }
        set
        {
            if (isAlive != value)
            {
                _isAlive = value;
                if (model != null)
                {
                    model.gameObject.SetActive(isAlive);
                }
                updateColliderEnabled();
                triggerAliveChanged();
            }
        }
    }

    public bool isPhysical
    {
        get
        {
            return _isPhysical;
        }
        set
        {
            _isPhysical = value;
            updateColliderEnabled();
        }
    }

    public event WheelAliveChangedHandler aliveChanged;

    protected virtual void triggerAliveChanged()
    {
        if (this.aliveChanged != null)
        {
            this.aliveChanged(this);
        }
    }

    public void askRepair()
    {
        if (!isAlive)
        {
            isAlive = true;
            vehicle.sendTireAliveMaskUpdate();
        }
    }

    public void askDamage()
    {
        if (isAlive)
        {
            isAlive = false;
            vehicle.sendTireAliveMaskUpdate();
            EffectAsset effectAsset = Rubber_0_Ref.Find();
            if (effectAsset != null)
            {
                TriggerEffectParameters parameters = new TriggerEffectParameters(effectAsset);
                parameters.relevantDistance = EffectManager.SMALL;
                parameters.position = wheel.transform.position;
                parameters.direction = wheel.transform.up;
                EffectManager.triggerEffect(parameters);
            }
        }
    }

    protected virtual void updateColliderEnabled()
    {
        if (wheel != null)
        {
            wheel.gameObject.SetActive(isPhysical && isAlive);
        }
    }

    public void reset()
    {
        direction = 0f;
        steer = 0f;
        speed = 0f;
        if (wheel != null)
        {
            wheel.steerAngle = 0f;
            wheel.motorTorque = 0f;
            wheel.brakeTorque = vehicle.asset.brake * 0.25f;
            sidewaysFriction.stiffness = 0.25f;
            wheel.sidewaysFriction = sidewaysFriction;
            forwardFriction.stiffness = 0.25f;
            wheel.forwardFriction = forwardFriction;
        }
    }

    public void simulate(float input_x, float input_y, bool inputBrake, float delta)
    {
        if (wheel == null)
        {
            return;
        }
        if (isSteered)
        {
            direction = input_x;
            steer = Mathf.Lerp(steer, Mathf.Lerp(vehicle.asset.steerMax, vehicle.asset.steerMin, vehicle.factor), 2f * delta);
        }
        float num = Mathf.Abs(input_y);
        if (isPowered)
        {
            if (input_y > 0f)
            {
                if (vehicle.asset.engine == EEngine.PLANE)
                {
                    if (vehicle.speed < 0f)
                    {
                        speed = Mathf.Lerp(speed, vehicle.asset.speedMax * num / 2f, delta / 4f);
                    }
                    else
                    {
                        speed = Mathf.Lerp(speed, vehicle.asset.speedMax * num / 2f, delta / 8f);
                    }
                }
                else if (vehicle.speed < 0f)
                {
                    speed = Mathf.Lerp(speed, vehicle.asset.speedMax * num, 2f * delta);
                }
                else
                {
                    speed = Mathf.Lerp(speed, vehicle.asset.speedMax * num, delta);
                }
            }
            else if (input_y < 0f)
            {
                if (vehicle.speed > 0f)
                {
                    speed = Mathf.Lerp(speed, vehicle.asset.speedMin * num, 2f * delta);
                }
                else
                {
                    speed = Mathf.Lerp(speed, vehicle.asset.speedMin * num, delta);
                }
            }
            else
            {
                speed = Mathf.Lerp(speed, 0f, delta);
            }
        }
        if (hasBrakes && inputBrake)
        {
            float num2 = Mathf.Lerp(1f, brakeTorqueTractionMultiplier, vehicle.slip);
            num2 *= brakeTorqueMultiplier;
            wheel.brakeTorque = vehicle.asset.brake * num2;
        }
        else
        {
            wheel.brakeTorque = 0f;
        }
        _isGrounded = Physics.Raycast(new Ray(wheel.transform.position, -wheel.transform.up), out var _, wheel.suspensionDistance + wheel.radius, RayMasks.BLOCK_COLLISION);
    }

    public void update(float delta)
    {
        if (wheel == null)
        {
            return;
        }
        wheel.steerAngle = Mathf.Lerp(wheel.steerAngle, direction * steer, 4f * delta);
        if (vehicle.asset.hasSleds)
        {
            sidewaysFriction.stiffness = Mathf.Lerp(wheel.sidewaysFriction.stiffness, 0.25f, 4f * delta);
            forwardFriction.stiffness = Mathf.Lerp(wheel.forwardFriction.stiffness, 0.25f, 4f * delta);
        }
        else
        {
            float num = Mathf.Lerp(1f, stiffnessTractionMultiplier, vehicle.slip);
            sidewaysFriction.stiffness = Mathf.Lerp(wheel.sidewaysFriction.stiffness, stiffnessSideways * num, 4f * delta);
            forwardFriction.stiffness = Mathf.Lerp(wheel.forwardFriction.stiffness, stiffnessForward * num, 4f * delta);
        }
        wheel.sidewaysFriction = sidewaysFriction;
        wheel.forwardFriction = forwardFriction;
        if (speed > 0f)
        {
            if (vehicle.speed < 0f)
            {
                wheel.motorTorque = Mathf.Lerp(wheel.motorTorque, speed * motorTorqueMultiplier, 4f * delta);
            }
            else if (vehicle.speed < vehicle.asset.speedMax)
            {
                wheel.motorTorque = Mathf.Lerp(wheel.motorTorque, speed * motorTorqueMultiplier, 2f * delta);
            }
            else
            {
                wheel.motorTorque = Mathf.Lerp(wheel.motorTorque, speed * motorTorqueMultiplier * motorTorqueClampMultiplier, 2f * delta);
            }
        }
        else if (vehicle.speed > 0f)
        {
            wheel.motorTorque = Mathf.Lerp(wheel.motorTorque, speed * motorTorqueMultiplier, 4f * delta);
        }
        else if (vehicle.speed > vehicle.asset.speedMin)
        {
            wheel.motorTorque = Mathf.Lerp(wheel.motorTorque, speed * motorTorqueMultiplier, 2f * delta);
        }
        else
        {
            wheel.motorTorque = Mathf.Lerp(wheel.motorTorque, speed * motorTorqueMultiplier * motorTorqueClampMultiplier, 2f * delta);
        }
    }

    public void checkForTraps()
    {
        if (!(wheel == null) && isAlive && Provider.isServer && vehicle.asset != null && vehicle.asset.canTiresBeDamaged)
        {
            Physics.Raycast(new Ray(wheel.transform.position, -wheel.transform.up), out var hitInfo, wheel.suspensionDistance + wheel.radius, 134217728);
            if (hitInfo.transform != null && hitInfo.transform.CompareTag("Barricade") && hitInfo.transform.GetComponent<InteractableTrapDamageTires>() != null)
            {
                askDamage();
            }
        }
    }

    public Wheel(InteractableVehicle newVehicle, int newIndex, WheelCollider newWheel, bool newSteered, bool newPowered)
    {
        _vehicle = newVehicle;
        index = newIndex;
        _wheel = newWheel;
        if (wheel != null)
        {
            sidewaysFriction = wheel.sidewaysFriction;
            forwardFriction = wheel.forwardFriction;
            wheel.forceAppPointDistance = 0f;
        }
        _isSteered = newSteered;
        isPowered = newPowered;
        hasBrakes = true;
        isAlive = true;
    }
}
