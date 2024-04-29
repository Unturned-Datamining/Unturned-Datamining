# Host bans

## IPv4 filters
| Address                    | BanFlags                                           |
| -------------------------- | -------------------------------------------------- |
| 195.18.27.196:25444-25445  | HiddenFromAllServerLists                           |
| 89.208.226.202:28069-28070 | HiddenFromAllServerLists, Blocked, WorkshopWarning |
| 89.208.226.202:28120-28121 | HiddenFromAllServerLists, Blocked, WorkshopWarning |
| 89.208.226.202:28129-28130 | HiddenFromAllServerLists, Blocked, WorkshopWarning |
| 89.208.226.202:28180-28181 | HiddenFromAllServerLists, Blocked, WorkshopWarning |
| 89.208.226.202:28097-28098 | HiddenFromAllServerLists, Blocked, WorkshopWarning |
| 212.22.93.57:50000-50001   | HiddenFromAllServerLists, Blocked, WorkshopWarning |
| 185.17.0.85                | HiddenFromAllServerLists, Blocked, WorkshopWarning |
| 185.17.0.77                | HiddenFromAllServerLists, Blocked, WorkshopWarning |
| 185.17.0.28                | HiddenFromAllServerLists, Blocked, WorkshopWarning |
| 185.17.0.14                | HiddenFromAllServerLists, Blocked, WorkshopWarning |
| 185.17.0.29                | HiddenFromAllServerLists, Blocked, WorkshopWarning |
| 185.17.0.2                 | HiddenFromAllServerLists, Blocked, WorkshopWarning |
| 185.17.0.86                | HiddenFromAllServerLists, Blocked, WorkshopWarning |
| 185.17.0.88                | HiddenFromAllServerLists, Blocked, WorkshopWarning |
| 194.93.2.43                | HiddenFromAllServerLists, Blocked, WorkshopWarning |
| 37.230.228.210             | HiddenFromAllServerLists, Blocked, WorkshopWarning |
| 194.147.90.244:23443-23444 | HiddenFromAllServerLists, Blocked, WorkshopWarning |
| 109.248.4.107              | HiddenFromAllServerLists, Blocked, WorkshopWarning |
| 92.38.222.24               | HiddenFromAllServerLists, Blocked, WorkshopWarning |
| 5.42.211.19                | HiddenFromAllServerLists, Blocked, WorkshopWarning |

## Name filters
| Regex                                                                                 | BanFlags                                                      |
| ------------------------------------------------------------------------------------- | ------------------------------------------------------------- |
| (?i)(pandahut&#124;panda hut)(?!.* )                                                  | HiddenFromAllServerLists                                      |
| ^(-SOD-)                                                                              | HiddenFromAllServerLists                                      |
| (?i)(Auschwitz&#124;Nazi&#124;Nazista&#124;Anti-Negro&#124;Anti-Jew&#124;Anti-Judeus) | HiddenFromAllServerLists, Blocked                             |
| (?i)(nigger)                                                                          | HiddenFromAllServerLists, Blocked                             |
| (?i)(UnityPlay)                                                                       | HiddenFromAllServerLists, Blocked                             |
| (?i)(UNIT)Y*У*(PL)                                                                    | HiddenFromAllServerLists, Blocked                             |
| (?i)(ЮНИТИПЛЕЙ)                                                                       | HiddenFromAllServerLists, Blocked                             |
| (?i)(FUNNYPLAY)                                                                       | HiddenFromAllServerLists, Blocked                             |
| (?i)(YAKINDA)&#124;(discord.gg/gporp)&#124;(GPO LIFE)                                 | HiddenFromInternetServerList                                  |
| (?i)(Cronix)                                                                          | HiddenFromAllServerLists, Blocked, WorkshopWarning            |
| (?i)(Fenix Server)                                                                    | HiddenFromAllServerLists, Blocked, WorkshopWarning            |
| (?i)(Fenix Project)                                                                   | HiddenFromAllServerLists, Blocked, WorkshopWarning            |
| (?i)(NortxNetwork)                                                                    | HiddenFromAllServerLists, Blocked, WorkshopWarning            |
| (?i)(no\s*-*\s*lag)                                                                   | HiddenFromInternetServerList                                  |
| (?i)(DarkStar)                                                                        | Blocked, WorkshopWarning, HiddenFromInternetServerList        |
| (?i)(Eagles Roleplay)&#124;(EaglesRP)                                                 | MonetizationWarning, HiddenFromInternetServerList             |
| (?i)(Sunnyvale)                                                                       | MonetizationWarning, HiddenFromInternetServerList             |
| (?i)(horizon)                                                                         | QueryPingWarning                                              |
| (?i)(BlackBear)                                                                       | QueryPingWarning                                              |
| (?i)(Asgard Roleplay)                                                                 | MonetizationWarning, HiddenFromInternetServerList             |
| (?i)(Galaxy Project)                                                                  | MonetizationWarning, HiddenFromInternetServerList             |
| (?i)(A9 ARENA)                                                                        | HiddenFromInternetServerList, IncorrectMonetizationTagWarning |
| (?i)(\[EU\] Eclipse)                                                                  | QueryPingWarning                                              |
| (?i)(Hunters Z)                                                                       | MonetizationWarning                                           |

## Description filters
| Regex                                       | BanFlags                                           |
| ------------------------------------------- | -------------------------------------------------- |
| &lt;color=#FFE818&gt;ROLEPLAY&lt;/color&gt; | HiddenFromAllServerLists, Blocked, WorkshopWarning |
| (?i)(&lt;style)&#124;(&lt;align)            | HiddenFromInternetServerList                       |
| (?i)(&lt;scale=)                            | HiddenFromInternetServerList                       |
| (?i)(&lt;pos=)                              | HiddenFromInternetServerList                       |

## Thumbnail filters
| Regex                                                    | IconPreview                                                                                                       | BanFlags                                           |
| -------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------- | -------------------------------------------------- |
| (https://i.ibb.co/Z25Y8D6/image-3.png)                   | ![https://i.ibb.co/Z25Y8D6/image-3.png](https://i.ibb.co/Z25Y8D6/image-3.png)                                     | HiddenFromAllServerLists                           |
| (https://r.resimlink.com/GXZ0gP2.jpg)                    | ![https://r.resimlink.com/GXZ0gP2.jpg](https://r.resimlink.com/GXZ0gP2.jpg)                                       | HiddenFromInternetServerList                       |
| (?i)(https://i.ibb.co/5k2nSYh/R6f-Al-Ufeyw8.jpg)         | ![https://i.ibb.co/5k2nSYh/R6f-Al-Ufeyw8.jpg](https://i.ibb.co/5k2nSYh/R6f-Al-Ufeyw8.jpg)                         | HiddenFromAllServerLists, Blocked                  |
| (https://i.ibb.co/M28DTbq/Untitled-logo-1-free-file.jpg) | ![https://i.ibb.co/M28DTbq/Untitled-logo-1-free-file.jpg](https://i.ibb.co/M28DTbq/Untitled-logo-1-free-file.jpg) | HiddenFromAllServerLists, Blocked                  |
| (?i)(https://icdn.su/f/kJN/VxF/Ku3.jpg)                  | ![https://icdn.su/f/kJN/VxF/Ku3.jpg](https://icdn.su/f/kJN/VxF/Ku3.jpg)                                           | HiddenFromAllServerLists, Blocked, WorkshopWarning |
| (Ekran-Al-nt-s.png)                                      |                                                                                                                   | MonetizationWarning, HiddenFromInternetServerList  |
| (https://i.imgur.com/c8yoKsk.png)                        | ![https://i.imgur.com/c8yoKsk.png](https://i.imgur.com/c8yoKsk.png)                                               | MonetizationWarning                                |
| (https://gspics.org/images/2018/06/10/AkbKx.png)         | ![https://gspics.org/images/2018/06/10/AkbKx.png](https://gspics.org/images/2018/06/10/AkbKx.png)                 | MonetizationWarning                                |

## SteamId filters
| SteamId           | BanFlags                                               |
| ----------------- | ------------------------------------------------------ |
| 85568392927283679 | HiddenFromInternetServerList                           |
| 85568392925162145 | HiddenFromAllServerLists, Blocked, WorkshopWarning     |
| 85568392925812850 | HiddenFromAllServerLists, Blocked, WorkshopWarning     |
| 85568392929406588 | HiddenFromAllServerLists, Blocked, WorkshopWarning     |
| 85568392929420767 | HiddenFromAllServerLists, Blocked, WorkshopWarning     |
| 85568392928968627 | Blocked, WorkshopWarning, HiddenFromInternetServerList |
| 85568392924735843 | MonetizationWarning, HiddenFromInternetServerList      |
| 85568392929250709 | MonetizationWarning, HiddenFromInternetServerList      |
| 85568392928833031 | MonetizationWarning, HiddenFromInternetServerList      |
| 85568392925961914 | QueryPingWarning                                       |
| 85568392928794437 | WorkshopWarning                                        |
| 85568392929557585 | MonetizationWarning, HiddenFromInternetServerList      |
| 85568392929711611 | MonetizationWarning, HiddenFromInternetServerList      |
| 85568392930733591 | Blocked                                                |
| 85568392931324392 | MonetizationWarning                                    |
| 85568392924910977 | MonetizationWarning                                    |
