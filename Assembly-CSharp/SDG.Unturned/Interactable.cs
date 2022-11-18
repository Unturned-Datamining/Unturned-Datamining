using UnityEngine;

namespace SDG.Unturned;

public class Interactable : MonoBehaviour
{
    private NetId _netId;

    public bool isPlant
    {
        get
        {
            if (base.transform.parent != null)
            {
                return base.transform.parent.CompareTag("Vehicle");
            }
            return false;
        }
    }

    public virtual void updateState(Asset asset, byte[] state)
    {
    }

    public virtual bool checkInteractable()
    {
        return true;
    }

    public virtual bool checkUseable()
    {
        return true;
    }

    public virtual bool checkHighlight(out Color color)
    {
        color = Color.white;
        return false;
    }

    public virtual bool checkHint(out EPlayerMessage message, out string text, out Color color)
    {
        message = EPlayerMessage.NONE;
        text = "";
        color = Color.white;
        return false;
    }

    public virtual void use()
    {
    }

    public NetId GetNetId()
    {
        return _netId;
    }

    internal void AssignNetId(NetId netId)
    {
        _netId = netId;
        NetIdRegistry.Assign(netId, this);
    }

    internal void ReleaseNetId()
    {
        NetIdRegistry.Release(_netId);
        _netId.Clear();
    }
}
