using System;

namespace SDG.Unturned;

public enum ESpawnMode
{
    [Obsolete]
    ADD_RESOURCE,
    [Obsolete]
    REMOVE_RESOURCE,
    ADD_ITEM,
    REMOVE_ITEM,
    ADD_PLAYER,
    REMOVE_PLAYER,
    ADD_ZOMBIE,
    REMOVE_ZOMBIE,
    ADD_VEHICLE,
    REMOVE_VEHICLE,
    ADD_ANIMAL,
    REMOVE_ANIMAL
}
