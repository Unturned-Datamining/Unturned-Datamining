using UnityEngine;

namespace SDG.Unturned;

public class EngineCurvesComponent : MonoBehaviour
{
    [Tooltip("Maps normalized engine RPM to torque multiplier.\nIdle RPM is zero and max RPM is one on the X axis.")]
    public AnimationCurve engineRpmToTorqueCurve;
}
