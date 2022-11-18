using SDG.NetTransport;
using UnityEngine;

namespace SDG.Unturned;

public class InteractableLadder : Interactable
{
    public override bool checkUseable()
    {
        return true;
    }

    public override void use()
    {
        if (CanClimb(Player.player))
        {
            Vector3 normalized = (PlayerInteract.hit.point - Player.player.look.aim.position).normalized;
            PlayerStance.SendClimbRequest.Invoke(Player.player.stance.GetNetId(), ENetReliability.Reliable, normalized);
        }
    }

    public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
    {
        text = "";
        color = Color.white;
        if (CanClimb(Player.player))
        {
            message = EPlayerMessage.CLIMB;
            return true;
        }
        message = EPlayerMessage.NONE;
        return false;
    }

    private bool CanClimb(Player player)
    {
        if (player == null || player.stance.stance == EPlayerStance.CLIMB)
        {
            return false;
        }
        if (!player.stance.canCurrentStanceTransitionToClimbing)
        {
            return false;
        }
        if (!player.stance.isAllowedToStartClimbing)
        {
            return false;
        }
        Vector3 normalized = (PlayerInteract.hit.point - player.look.aim.position).normalized;
        Physics.SphereCast(new Ray(player.look.aim.position, normalized), PlayerStance.RADIUS, out var hitInfo, 4f, RayMasks.LADDER_INTERACT);
        if (hitInfo.collider == null || !hitInfo.collider.CompareTag("Ladder"))
        {
            return false;
        }
        Physics.Raycast(new Ray(player.look.aim.position, normalized), out var hitInfo2, 4f, RayMasks.LADDER_INTERACT);
        if (hitInfo2.collider == null || !hitInfo2.collider.CompareTag("Ladder"))
        {
            return false;
        }
        if (Mathf.Abs(Vector3.Dot(hitInfo2.normal, hitInfo2.collider.transform.up)) <= 0.9f)
        {
            return false;
        }
        if (Mathf.Abs(Vector3.Dot(Vector3.up, hitInfo2.collider.transform.up)) > 0.1f)
        {
            return false;
        }
        Vector3 vector = new Vector3(hitInfo2.collider.transform.position.x, hitInfo2.point.y - 0.5f - 0.5f - 0.1f, hitInfo2.collider.transform.position.z) + hitInfo2.normal * 0.65f;
        float num = PlayerMovement.HEIGHT_STAND + 0.1f + 0.5f;
        Vector3 end = vector + new Vector3(0f, num * 0.5f, 0f);
        if (Physics.Linecast(hitInfo2.point, end, out var _, RayMasks.BLOCK_STANCE, QueryTriggerInteraction.Collide))
        {
            return false;
        }
        return PlayerStance.hasHeightClearanceAtPosition(vector, num);
    }
}
