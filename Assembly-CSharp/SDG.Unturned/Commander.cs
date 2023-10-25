using System;
using System.Collections.Generic;
using Steamworks;

namespace SDG.Unturned;

public class Commander
{
    public delegate void ServerUnityEventPermissionHandler(ServerTextChatMessenger messenger, string command, ref bool shouldAllow);

    public static List<Command> commands { get; set; }

    public static event ServerUnityEventPermissionHandler onCheckUnityEventPermissions;

    public static void register(Command command)
    {
        int num = commands.BinarySearch(command);
        if (num < 0)
        {
            num = ~num;
        }
        commands.Insert(num, command);
    }

    public static void deregister(Command command)
    {
        commands.Remove(command);
    }

    public static bool execute(CSteamID executorID, string command)
    {
        try
        {
            string method = command;
            string parameter = "";
            int num = command.IndexOf(' ');
            if (num != -1)
            {
                method = command.Substring(0, num);
                parameter = command.Substring(num + 1, command.Length - num - 1);
            }
            for (int i = 0; i < commands.Count; i++)
            {
                if (commands[i].check(executorID, method, parameter))
                {
                    return true;
                }
            }
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, "Caught exception while executing command string \"{0}\"", command);
        }
        return false;
    }

    /// <summary>
    /// Allows Unity events to execute commands from the server.
    /// Messenger context is logged to help track down abusive assets.
    /// </summary>
    public static void execute_UnityEvent(string command, ServerTextChatMessenger messenger)
    {
        if (messenger == null)
        {
            throw new ArgumentNullException("messenger");
        }
        if (!Dedicator.IsDedicatedServer || Provider.configData.UnityEvents.Allow_Server_Commands)
        {
            bool shouldAllow = true;
            Commander.onCheckUnityEventPermissions?.Invoke(messenger, command, ref shouldAllow);
            UnturnedLog.info("UnityEventCmd {0}: '{1}' Allow: {2}", messenger.gameObject.GetSceneHierarchyPath(), command, shouldAllow);
            if (shouldAllow)
            {
                execute(CSteamID.Nil, command);
            }
        }
    }

    public static void init()
    {
        commands = new List<Command>();
        register(new CommandModules(Localization.read("/Server/ServerCommandModules.dat")));
        register(new CommandReload(Localization.read("/Server/ServerCommandReload.dat")));
        register(new CommandHelp(Localization.read("/Server/ServerCommandHelp.dat")));
        register(new CommandName(Localization.read("/Server/ServerCommandName.dat")));
        register(new CommandPort(Localization.read("/Server/ServerCommandPort.dat")));
        register(new CommandPassword(Localization.read("/Server/ServerCommandPassword.dat")));
        register(new CommandMaxPlayers(Localization.read("/Server/ServerCommandMaxPlayers.dat")));
        register(new CommandQueue(Localization.read("/Server/ServerCommandQueue.dat")));
        register(new CommandMap(Localization.read("/Server/ServerCommandMap.dat")));
        register(new CommandPvE(Localization.read("/Server/ServerCommandPvE.dat")));
        register(new CommandWhitelisted(Localization.read("/Server/ServerCommandWhitelisted.dat")));
        register(new CommandCheats(Localization.read("/Server/ServerCommandCheats.dat")));
        register(new CommandHideAdmins(Localization.read("/Server/ServerCommandHideAdmins.dat")));
        register(new CommandEffectUI(Localization.read("/Server/ServerCommandEffectUI.dat")));
        register(new CommandSync(Localization.read("/Server/ServerCommandSync.dat")));
        register(new CommandFilter(Localization.read("/Server/ServerCommandFilter.dat")));
        register(new CommandVotify(Localization.read("/Server/ServerCommandVotify.dat")));
        register(new CommandMode(Localization.read("/Server/ServerCommandMode.dat")));
        register(new CommandGameMode(Localization.read("/Server/ServerCommandGameMode.dat")));
        register(new CommandGold(Localization.read("/Server/ServerCommandGold.dat")));
        register(new CommandCamera(Localization.read("/Server/ServerCommandCamera.dat")));
        register(new CommandCycle(Localization.read("/Server/ServerCommandCycle.dat")));
        register(new CommandTime(Localization.read("/Server/ServerCommandTime.dat")));
        register(new CommandDay(Localization.read("/Server/ServerCommandDay.dat")));
        register(new CommandNight(Localization.read("/Server/ServerCommandNight.dat")));
        register(new CommandWeather(Localization.read("/Server/ServerCommandWeather.dat")));
        register(new CommandAirdrop(Localization.read("/Server/ServerCommandAirdrop.dat")));
        register(new CommandKick(Localization.read("/Server/ServerCommandKick.dat")));
        register(new CommandSpy(Localization.read("/Server/ServerCommandSpy.dat")));
        register(new CommandBan(Localization.read("/Server/ServerCommandBan.dat")));
        register(new CommandUnban(Localization.read("/Server/ServerCommandUnban.dat")));
        register(new CommandBans(Localization.read("/Server/ServerCommandBans.dat")));
        register(new CommandAdmin(Localization.read("/Server/ServerCommandAdmin.dat")));
        register(new CommandUnadmin(Localization.read("/Server/ServerCommandUnadmin.dat")));
        register(new CommandAdmins(Localization.read("/Server/ServerCommandAdmins.dat")));
        register(new CommandOwner(Localization.read("/Server/ServerCommandOwner.dat")));
        register(new CommandPermit(Localization.read("/Server/ServerCommandPermit.dat")));
        register(new CommandUnpermit(Localization.read("/Server/ServerCommandUnpermit.dat")));
        register(new CommandPermits(Localization.read("/Server/ServerCommandPermits.dat")));
        register(new CommandPlayers(Localization.read("/Server/ServerCommandPlayers.dat")));
        register(new CommandSay(Localization.read("/Server/ServerCommandSay.dat")));
        register(new CommandWelcome(Localization.read("/Server/ServerCommandWelcome.dat")));
        register(new CommandSlay(Localization.read("/Server/ServerCommandSlay.dat")));
        register(new CommandKill(Localization.read("/Server/ServerCommandKill.dat")));
        register(new CommandGive(Localization.read("/Server/ServerCommandGive.dat")));
        register(new CommandUnlockNpcAchievement(Localization.read("/Server/ServerCommandGive.dat")));
        register(new CommandScheduledShutdownInfo(Localization.read("/Server/ServerCommandGive.dat")));
        register(new CommandSetNpcSpawnId(Localization.read("/Server/ServerCommandGive.dat")));
        register(new CommandNpcEvent(Localization.read("/Server/ServerCommandGive.dat")));
        register(new CommandLoadout(Localization.read("/Server/ServerCommandLoadout.dat")));
        register(new CommandExperience(Localization.read("/Server/ServerCommandExperience.dat")));
        register(new CommandReputation(Localization.read("/Server/ServerCommandReputation.dat")));
        register(new CommandFlag(Localization.read("/Server/ServerCommandFlag.dat")));
        register(new CommandQuest(Localization.read("/Server/ServerCommandQuest.dat")));
        register(new CommandVehicle(Localization.read("/Server/ServerCommandVehicle.dat")));
        register(new CommandAnimal(Localization.read("/Server/ServerCommandAnimal.dat")));
        register(new CommandTeleport(Localization.read("/Server/ServerCommandTeleport.dat")));
        register(new CommandTimeout(Localization.read("/Server/ServerCommandTimeout.dat")));
        register(new CommandChatrate(Localization.read("/Server/ServerCommandChatrate.dat")));
        register(new CommandLog(Localization.read("/Server/ServerCommandLog.dat")));
        register(new CommandLogMemoryUsage(Localization.read("/Server/ServerCommandGive.dat")));
        register(new CommandDebug(Localization.read("/Server/ServerCommandDebug.dat")));
        register(new CommandResetConfig(Localization.read("/Server/ServerCommandResetConfig.dat")));
        register(new CommandBind(Localization.read("/Server/ServerCommandBind.dat")));
        register(new CommandSave(Localization.read("/Server/ServerCommandSave.dat")));
        register(new CommandShutdown(Localization.read("/Server/ServerCommandShutdown.dat")));
        register(new CommandGSLT(Localization.read("/Server/ServerCommandGSLT.dat")));
    }
}
