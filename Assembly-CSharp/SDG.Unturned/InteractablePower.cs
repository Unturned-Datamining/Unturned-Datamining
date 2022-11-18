using System.Collections.Generic;

namespace SDG.Unturned;

public class InteractablePower : Interactable
{
    protected bool _isWired;

    public bool isWired => _isWired;

    protected virtual void updateWired()
    {
    }

    public void updateWired(bool newWired)
    {
        if (newWired != isWired)
        {
            _isWired = newWired;
            updateWired();
        }
    }

    private bool CalculateIsConnectedToPower()
    {
        if (Level.isEditor)
        {
            return true;
        }
        if (Level.info != null && Level.info.configData != null && Level.info.configData.Has_Global_Electricity)
        {
            return true;
        }
        if (base.isPlant)
        {
            ushort plant = ushort.MaxValue;
            BarricadeManager.tryGetPlant(base.transform.parent, out var _, out var _, out plant, out var _);
            List<InteractableGenerator> list = PowerTool.checkGenerators(base.transform.position, PowerTool.MAX_POWER_RANGE, plant);
            for (int i = 0; i < list.Count; i++)
            {
                InteractableGenerator interactableGenerator = list[i];
                if (interactableGenerator.isPowered && interactableGenerator.fuel > 0 && (interactableGenerator.transform.position - base.transform.position).sqrMagnitude < interactableGenerator.sqrWirerange)
                {
                    return true;
                }
            }
            return false;
        }
        return InteractableGenerator.IsWorldPositionPowered(base.transform.position);
    }

    internal void RefreshIsConnectedToPower()
    {
        updateWired(CalculateIsConnectedToPower());
    }

    internal void RefreshIsConnectedToPowerWithoutNotify()
    {
        _isWired = CalculateIsConnectedToPower();
    }
}
