namespace SDG.Framework.Water;

public interface IWaterVolumeInteractionHandler
{
    void waterBeginCollision(WaterVolume volume);

    void waterEndCollision(WaterVolume volume);
}
