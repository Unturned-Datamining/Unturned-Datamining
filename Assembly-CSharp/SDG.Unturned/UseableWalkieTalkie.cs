namespace SDG.Unturned;

public class UseableWalkieTalkie : Useable
{
    public override void equip()
    {
        base.player.animator.play("Equip", smooth: true);
        base.player.voice.hasUseableWalkieTalkie = true;
    }

    public override void dequip()
    {
        base.player.voice.hasUseableWalkieTalkie = false;
    }
}
