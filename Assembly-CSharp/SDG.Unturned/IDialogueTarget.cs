using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Implemented by components the player can talk with using DialogeAssets. (e.g., InteractableObjectNPC)
/// </summary>
public interface IDialogueTarget
{
    /// <summary>
    /// Used to test whether player is within range.
    /// Ideally, this should be removed in the future in favor of the server resetting speaker when out of range.
    /// </summary>
    Vector3 GetDialogueTargetWorldPosition();

    /// <summary>
    /// Get a net ID that can be used with GetDialogueTargetFromNetId to resolve IDialogueTarget in multiplayer.
    /// </summary>
    NetId GetDialogueTargetNetId();

    /// <summary>
    /// Called on server to test whether object conditions are met.
    /// </summary>
    bool ShouldServerApproveDialogueRequest(Player withPlayer);

    /// <summary>
    /// Called on server to find the start of conversation dialogue asset.
    /// </summary>
    DialogueAsset FindStartingDialogueAsset();

    /// <summary>
    /// Used in error messages.
    /// </summary>
    string GetDialogueTargetDebugName();

    /// <summary>
    /// Called on client to format in UI.
    /// </summary>
    string GetDialogueTargetNameShownToPlayer(Player player);

    void SetFaceOverride(byte? faceOverride);

    void SetIsTalkingWithLocalPlayer(bool isTalkingWithLocalPlayer);
}
