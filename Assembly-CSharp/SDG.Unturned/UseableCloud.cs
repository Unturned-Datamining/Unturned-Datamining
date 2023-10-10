namespace SDG.Unturned;

public class UseableCloud : Useable
{
    public override void equip()
    {
        base.player.animator.play("Equip", smooth: true);
    }

    public override void dequip()
    {
        base.player.movement.itemGravityMultiplier = 1f;
    }

    public override void tick()
    {
        if (base.player.equipment.IsEquipAnimationFinished)
        {
            base.player.movement.itemGravityMultiplier = ((ItemCloudAsset)base.player.equipment.asset).gravity;
        }
    }
}
