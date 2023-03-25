using System;

namespace SDG.HostBans;

[Flags]
public enum EHostBanFlags : uint
{
    None = 0u,
    HiddenFromAllServerLists = 1u,
    MonetizationWarning = 2u,
    Blocked = 4u,
    WorkshopWarning = 8u,
    Apology = 0x10u,
    HiddenFromInternetServerList = 0x20u,
    QueryPingWarning = 0x40u,
    IncorrectMonetizationTagWarning = 0x80u,
    HostingProvider = 0x100u,
    RecommendHostCheckWarningsList = 0x8Au
}
