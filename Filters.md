# Host bans

## IPv4 filters
| Address | BanFlags |
| ------- | -------- |

## Name filters
| Regex                                                                                 | BanFlags                          |
| ------------------------------------------------------------------------------------- | --------------------------------- |
| (?i)(pandahut&#124;panda hut)(?!.* )                                                  | HiddenFromAllServerLists          |
| (?i)(Auschwitz&#124;Nazi&#124;Nazista&#124;Anti-Negro&#124;Anti-Jew&#124;Anti-Judeus) | HiddenFromAllServerLists, Blocked |
| (?i)(nigger)                                                                          | HiddenFromAllServerLists, Blocked |
| (?i)(no\s*-*\s*lag)                                                                   | HiddenFromInternetServerList      |

## Description filters
| Regex | BanFlags |
| ----- | -------- |

## Thumbnail filters
| Regex                                 | IconPreview                                                                 | BanFlags                     |
| ------------------------------------- | --------------------------------------------------------------------------- | ---------------------------- |
| (https://r.resimlink.com/GXZ0gP2.jpg) | ![https://r.resimlink.com/GXZ0gP2.jpg](https://r.resimlink.com/GXZ0gP2.jpg) | HiddenFromInternetServerList |

## SteamId filters
| SteamId           | BanFlags                     |
| ----------------- | ---------------------------- |
| 85568392927283679 | HiddenFromInternetServerList |
