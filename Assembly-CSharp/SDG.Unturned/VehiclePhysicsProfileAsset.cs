using System;
using System.Diagnostics;
using SDG.Framework.IO.FormattedFiles;
using UnityEngine;

namespace SDG.Unturned;

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

    protected Friction? readFriction(IFormattedFileReader assetReader, string key)
    {
        if (assetReader.containsKey(key))
        {
            IFormattedFileReader formattedFileReader = assetReader.readObject(key);
            Friction value = default(Friction);
            value.extremumSlip = formattedFileReader.readValue<float>("Extremum_Slip");
            value.extremumValue = formattedFileReader.readValue<float>("Extremum_Value");
            value.asymptoteSlip = formattedFileReader.readValue<float>("Asymptote_Slip");
            value.asymptoteValue = formattedFileReader.readValue<float>("Asymptote_Value");
            value.stiffness = formattedFileReader.readValue<float>("Stiffness");
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

    protected override void readAsset(IFormattedFileReader reader)
    {
        base.readAsset(reader);
        if (reader.containsKey("Root_Mass"))
        {
            rootMassOverride = reader.readValue<float>("Root_Mass");
        }
        if (reader.containsKey("Root_Mass_Multiplier"))
        {
            rootMassMultiplier = reader.readValue<float>("Root_Mass_Multiplier");
        }
        if (reader.containsKey("Root_Drag_Multiplier"))
        {
            rootDragMultiplier = reader.readValue<float>("Root_Drag_Multiplier");
        }
        if (reader.containsKey("Root_Angular_Drag_Multiplier"))
        {
            rootAngularDragMultiplier = reader.readValue<float>("Root_Angular_Drag_Multiplier");
        }
        if (reader.containsKey("Carjack_Force_Multiplier"))
        {
            carjackForceMultiplier = reader.readValue<float>("Carjack_Force_Multiplier");
        }
        if (reader.containsKey("Wheel_Mass"))
        {
            wheelMassOverride = reader.readValue<float>("Wheel_Mass");
        }
        if (reader.containsKey("Wheel_Mass_Multiplier"))
        {
            wheelMassMultiplier = reader.readValue<float>("Wheel_Mass_Multiplier");
        }
        if (reader.containsKey("Wheel_Damping_Rate"))
        {
            wheelDampingRate = reader.readValue<float>("Wheel_Damping_Rate");
        }
        if (reader.containsKey("Wheel_Stiffness_Traction_Multiplier"))
        {
            wheelStiffnessTractionMultiplier = reader.readValue<float>("Wheel_Stiffness_Traction_Multiplier");
        }
        if (reader.containsKey("Wheel_Suspension_Force"))
        {
            wheelSuspensionForce = reader.readValue<float>("Wheel_Suspension_Force");
        }
        if (reader.containsKey("Wheel_Suspension_Damper"))
        {
            wheelSuspensionDamper = reader.readValue<float>("Wheel_Suspension_Damper");
        }
        sidewaysFriction = readFriction(reader, "Wheel_Friction_Sideways");
        forwardFriction = readFriction(reader, "Wheel_Friction_Forward");
        if (reader.containsKey("Motor_Torque_Multiplier"))
        {
            motorTorqueMultiplier = reader.readValue<float>("Motor_Torque_Multiplier");
        }
        if (reader.containsKey("Motor_Torque_Clamp_Multiplier"))
        {
            motorTorqueClampMultiplier = reader.readValue<float>("Motor_Torque_Clamp_Multiplier");
        }
        if (reader.containsKey("Brake_Torque_Multiplier"))
        {
            brakeTorqueMultiplier = reader.readValue<float>("Brake_Torque_Multiplier");
        }
        if (reader.containsKey("Brake_Torque_Traction_Multiplier"))
        {
            brakeTorqueTractionMultiplier = reader.readValue<float>("Brake_Torque_Traction_Multiplier");
        }
        if (reader.containsKey("Wheel_Drive_Model"))
        {
            wheelDriveModel = reader.readValue<EDriveModel>("Wheel_Drive_Model");
        }
        if (reader.containsKey("Wheel_Brake_Model"))
        {
            wheelBrakeModel = reader.readValue<EDriveModel>("Wheel_Brake_Model");
        }
    }

    public VehiclePhysicsProfileAsset(Bundle bundle, Local localization, byte[] hash)
        : base(bundle, localization, hash)
    {
    }

    [Conditional("LOG_VEHICLE_PHYSICS_PROFILE")]
    private void log(InteractableVehicle vehicle, string format, params object[] args)
    {
        UnturnedLog.info(vehicle.asset.name + ": " + format, args);
    }
}
