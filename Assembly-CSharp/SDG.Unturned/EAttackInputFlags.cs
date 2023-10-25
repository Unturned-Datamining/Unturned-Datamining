using System;

namespace SDG.Unturned;

/// <summary>
/// Start/Stop input is encoded as 2 bits, 1 bit for Start flag and 1 bit for Stop flag.
///
/// Prior to 2023-03-16 it was a single bit. The server would Start if true and the previous frame was false,
/// and vice versa call Stop if false and the previous frame was true. The problem with that approach was when
/// the client FPS is higher than the simulation FPS a series of repeated attack presses would be treated as a
/// continuous held attack input. Semi-auto guns were difficult to shoot at their max rate of fire. Sending both
/// allows the server to theoretically call Start every simulation frame as opposed to only half.
///
/// First approach was to OR Start if held, otherwise OR Stop. This doesn't work because for example when Aim is
/// pressed the Stop flag will already be enabled, so the gun Starts aiming, Stops aiming, Starts aiming, and then
/// stays aiming rather than just Start and stay aiming. Instead we only want Stop to be sent once.
/// </summary>
[Flags]
public enum EAttackInputFlags
{
    None = 0,
    /// <summary>
    /// Wants to "start" primary or secondary input. (e.g., Useable.startPrimary)
    /// </summary>
    Start = 1,
    /// <summary>
    /// Wants to "stop" primary or secondary input. (e.g., Useable.stopPrimary)
    /// </summary>
    Stop = 2
}
