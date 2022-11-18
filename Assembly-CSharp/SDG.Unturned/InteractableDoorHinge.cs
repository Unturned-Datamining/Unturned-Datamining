using UnityEngine;

namespace SDG.Unturned;

public class InteractableDoorHinge : Interactable
{
    public InteractableDoor door;

    public override bool checkUseable()
    {
        return door.checkToggle(Provider.client, Player.player.quests.groupID);
    }

    public override void use()
    {
        door.ClientToggle();
    }

    public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
    {
        if (checkUseable())
        {
            if (door.isOpen)
            {
                message = EPlayerMessage.DOOR_CLOSE;
            }
            else
            {
                message = EPlayerMessage.DOOR_OPEN;
            }
        }
        else
        {
            message = EPlayerMessage.LOCKED;
        }
        text = "";
        color = Color.white;
        return true;
    }
}
