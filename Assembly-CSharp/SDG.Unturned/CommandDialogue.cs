using Steamworks;

namespace SDG.Unturned;

public class CommandDialogue : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (!Provider.isServer || !Provider.hasCheats)
        {
            return;
        }
        SteamPlayer steamPlayer = PlayerTool.getSteamPlayer(executorID);
        if (steamPlayer != null && !(steamPlayer.player == null) && CachingAssetRef.TryParse(parameter, out var result))
        {
            DialogueAsset dialogueAsset = result.Get<DialogueAsset>();
            if (dialogueAsset != null)
            {
                steamPlayer.player.quests.ApproveTalkWithNpcRequest(steamPlayer.player, dialogueAsset);
            }
        }
    }

    public CommandDialogue(Local newLocalization)
    {
        localization = newLocalization;
        _command = "Dialogue";
        _info = string.Empty;
        _help = string.Empty;
    }
}
