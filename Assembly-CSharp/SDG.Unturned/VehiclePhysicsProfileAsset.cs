using System;
using System.Diagnostics;
using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Overrides vehicle physics values in bulk without building asset bundles.
/// </summary>
public class VehiclePhysicsProfileAsset : Asset
{
    public struct Friction
    {
        public float extremumSlip;

        public float extremumValue;

        public float asymptoteSlip;

        public float asymptoteValue;

        public float stiffness;

        public void applyTo(ref WheelFrictionCurve frictionCurve)
        {
            frictionCurve.extremumSlip = extremumSlip;
            frictionCurve.extremumValue = extremumValue;
            frictionCurve.asymptoteSlip = frictionCurve.asymptoteSlip;
            frictionCurve.asymptoteValue = frictionCurve.asymptoteValue;
        }
    }

    public enum EDriveModel
    {
        Front,
        Rear,
        All
    }

    public static AssetReference<VehiclePhysicsProfileAsset> defaultProfile_Boat = new AssetReference<VehiclePhysicsProfileAsset>(new Guid("47258d0dcad14cb8be26e24c1ef3449e"));

    public static AssetReference<VehiclePhysicsProfileAsset> defaultProfile_Car = new AssetReference<VehiclePhysicsProfileAsset>(new Guid("6b91a94f01b6472eaca31d9420ec2367"));

    public static AssetReference<VehiclePhysicsProfileAsset> defaultProfile_Helicopter = new AssetReference<VehiclePhysicsProfileAsset>(new Guid("bb9f9f0204c4462ca7d976b87d1336d4"));

    public static AssetReference<VehiclePhysicsProfileAsset> defaultProfile_Plane = new AssetReference<VehiclePhysicsProfileAsset>(new Guid("93a47d6d40454335b4784e803628ac54"));

    public float? rootMassOverride { get; protected set; }

    public float? rootMassMultiplier { get; protected set; }

    public float? rootDragMultiplier { get; protected set; }

    public float? rootAngularDragMultiplier { get; protected set; }

    public float? carjackForceMultiplier { get; protected set; }

    public float? wheelMassOverride { get; protected set; }

    public float? wheelMassMultiplier { get; protected set; }

    public float? wheelDampingRate { get; protected set; }

    public float? wheelStiffnessTractionMultiplier { get; protected set; }

    public float? wheelSuspensionForce { get; protected set; }

    public float? wheelSuspensionDamper { get; protected set; }

    public Friction? forwardFriction { get; protected set; }

    public Friction? sidewaysFriction { get; protected set; }

    public float? motorTorqueMultiplier { get; protected set; }

    public float? motorTorqueClampMultiplier { get; protected set; }

    public float? brakeTorqueMultiplier { get; protected set; }

    public float? brakeTorqueTractionMultiplier { get; protected set; }

    public EDriveModel? wheelDriveModel { get; protected set; }

    public EDriveModel? wheelBrakeModel { get; protected set; }

    protected Friction? readFriction(DatDictionary data, string key)
    {
        if (data.ContainsKey(key))
        {
            DatDictionary dictionary = data.GetDictionary(key);
            Friction value = default(Friction);
            value.extremumSlip = dictionary.ParseFloat("Extremum_Slip");
            value.extremumValue = dictionary.ParseFloat("Extremum_Value");
            value.asymptoteSlip = dictionary.ParseFloat("Asymptote_Slip");
            value.asymptoteValue = dictionary.ParseFloat("Asymptote_Value");
            value.stiffness = dictionary.ParseFloat("Stiffness");
            return value;
        }
        return null;
    }

    public void applyTo(InteractableVehicle vehicle)
    {
        Rigidbody component = vehicle.GetComponent<Rigidbody>();
        if (component != null)
        {
            if (rootMassOverride.HasValue)
            {
                component.mass = rootMassOverride.Value;
            }
            else if (rootMassMultiplier.HasValue)
            {
                component.mass *= rootMassMultiplier.Value;
            }
            if (rootDragMultiplier.HasValue)
            {
                component.drag *= rootDragMultiplier.Value;
            }
            if (rootAngularDragMultiplier.HasValue)
            {
                component.angularDrag *= rootAngularDragMultiplier.Value;
            }
        }
        bool flag = wheelMassOverride.HasValue && !vehicle.asset.wheelColliderMassOverride.HasValue;
        bool flag2 = wheelSuspensionForce.HasValue || wheelSuspensionDamper.HasValue;
        Wheel[] tires = vehicle.tires;
        foreach (Wheel wheel in tires)
        {
            if (wheel.wheel == null)
            {
                continue;
            }
            if (wheelStiffnessTractionMultiplier.HasValue)
            {
                wheel.stiffnessTractionMultiplier = wheelStiffnessTractionMultiplier.Value;
            }
            if (wheelDampingRate.HasValue)
            {
                wheel.wheel.wheelDampingRate = wheelDampingRate.Value;
            }
            if (flag2)
            {
                JointSpring suspensionSpring = wheel.wheel.suspensionSpring;
                if (wheelSuspensionForce.HasValue)
                {
                    suspensionSpring.spring = wheelSuspensionForce.Value;
                }
                if (wheelSuspensionDamper.HasValue)
                {
                    suspensionSpring.damper = wheelSuspensionDamper.Value;
                }
                wheel.wheel.suspensionSpring = suspensionSpring;
            }
            if (sidewaysFriction.HasValue)
            {
                wheel.stiffnessSideways = sidewaysFriction.Value.stiffness;
                sidewaysFriction.Value.applyTo(ref wheel.sidewaysFriction);
            }
            if (forwardFriction.HasValue)
            {
                wheel.stiffnessForward = forwardFriction.Value.stiffness;
                forwardFriction.Value.applyTo(ref wheel.forwardFriction);
            }
            if (flag)
            {
                wheel.wheel.mass = wheelMassOverride.Value;
            }
            else if (wheelMassMultiplier.HasValue)
            {
                wheel.wheel.mass *= wheelMassMultiplier.Value;
            }
            if (motorTorqueMultiplier.HasValue)
            {
                wheel.motorTorqueMultiplier = motorTorqueMultiplier.Value;
            }
            if (motorTorqueClampMultiplier.HasValue)
            {
                wheel.motorTorqueClampMultiplier = motorTorqueClampMultiplier.Value;
            }
            if (brakeTorqueMultiplier.HasValue)
            {
                wheel.brakeTorqueMultiplier = brakeTorqueMultiplier.Value;
            }
            if (brakeTorqueTractionMultiplier.HasValue)
            {
                wheel.brakeTorqueTractionMultiplier = brakeTorqueTractionMultiplier.Value;
            }
            if (wheelDriveModel.HasValue && wheel.index >= 0)
            {
                switch (wheelDriveModel.Value)
                {
                case EDriveModel.Front:
                    wheel.isPowered = wheel.index < 2;
                    break;
                default:
                    wheel.isPowered = wheel.index >= 2;
                    break;
                case EDriveModel.All:
                    wheel.isPowered = true;
                    break;
                }
            }
            if (wheelBrakeModel.HasValue && wheel.index >= 0)
            {
                switch (wheelBrakeModel.Value)
                {
                case EDriveModel.Front:
                    wheel.hasBrakes = wheel.index < 2;
                    break;
                case EDriveModel.Rear:
                    wheel.hasBrakes = wheel.index >= 2;
                    break;
                default:
                    wheel.hasBrakes = true;
                    break;
                }
            }
        }
    }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        if (data.ContainsKey("Root_Mass"))
        {
            rootMassOverride = data.ParseFloat("Root_Mass");
        }
        if (data.ContainsKey("Root_Mass_Multiplier"))
        {
            rootMassMultiplier = data.ParseFloat("Root_Mass_Multiplier");
        }
        if (data.ContainsKey("Root_Drag_Multiplier"))
        {
            rootDragMultiplier = data.ParseFloat("Root_Drag_Multiplier");
        }
        if (data.ContainsKey("Root_Angular_Drag_Multiplier"))
        {
            rootAngularDragMultiplier = data.ParseFloat("Root_Angular_Drag_Multiplier");
        }
        if (data.ContainsKey("Carjack_Force_Multiplier"))
        {
            carjackForceMultiplier = data.ParseFloat("Carjack_Force_Multiplier");
        }
        if (data.ContainsKey("Wheel_Mass"))
        {
            wheelMassOverride = data.ParseFloat("Wheel_Mass");
        }
        if (data.ContainsKey("Wheel_Mass_Multiplier"))
        {
            wheelMassMultiplier = data.ParseFloat("Wheel_Mass_Multiplier");
        }
        if (data.ContainsKey("Wheel_Damping_Rate"))
        {
            wheelDampingRate = data.ParseFloat("Wheel_Damping_Rate");
        }
        if (data.ContainsKey("Wheel_Stiffness_Traction_Multiplier"))
        {
            wheelStiffnessTractionMultiplier = data.ParseFloat("Wheel_Stiffness_Traction_Multiplier");
        }
        if (data.ContainsKey("Wheel_Suspension_Force"))
        {
            wheelSuspensionForce = data.ParseFloat("Wheel_Suspension_Force");
        }
        if (data.ContainsKey("Wheel_Suspension_Damper"))
        {
            wheelSuspensionDamper = data.ParseFloat("Wheel_Suspension_Damper");
        }
        sidewaysFriction = readFriction(data, "Wheel_Friction_Sideways");
        forwardFriction = readFriction(data, "Wheel_Friction_Forward");
        if (data.ContainsKey("Motor_Torque_Multiplier"))
        {
            motorTorqueMultiplier = data.ParseFloat("Motor_Torque_Multiplier");
        }
        if (data.ContainsKey("Motor_Torque_Clamp_Multiplier"))
        {
            motorTorqueClampMultiplier = data.ParseFloat("Motor_Torque_Clamp_Multiplier");
        }
        if (data.ContainsKey("Brake_Torque_Multiplier"))
        {
            brakeTorqueMultiplier = data.ParseFloat("Brake_Torque_Multiplier");
        }
        if (data.ContainsKey("Brake_Torque_Traction_Multiplier"))
        {
            brakeTorqueTractionMultiplier = data.ParseFloat("Brake_Torque_Traction_Multiplier");
        }
        if (data.ContainsKey("Wheel_Drive_Model"))
        {
            wheelDriveModel = data.ParseEnum("Wheel_Drive_Model", EDriveModel.Front);
        }
        if (data.ContainsKey("Wheel_Brake_Model"))
        {
            wheelBrakeModel = data.ParseEnum("Wheel_Brake_Model", EDriveModel.Front);
        }
    }

    [Conditional("LOG_VEHICLE_PHYSICS_PROFILE")]
    private void log(InteractableVehicle vehicle, string format, params object[] args)
    {
        UnturnedLog.info(vehicle.asset.name + ": " + format, args);
    }
}
