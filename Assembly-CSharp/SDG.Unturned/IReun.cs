using UnityEngine;

namespace SDG.Unturned;

public interface IReun
{
    int step { get; }

    Transform redo();

    void undo();
}
