using System;
using System.Collections.Generic;
using System.Text;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class CommandLogMemoryUsage : Command
{
    internal static Action<List<string>> OnExecuted;

    private static void GatherInfo(List<string> results)
    {
        OnExecuted?.Invoke(results);
        Type[] array = new Type[13]
        {
            typeof(GameObject),
            typeof(AudioSource),
            typeof(ParticleSystem),
            typeof(Collider),
            typeof(Rigidbody),
            typeof(Renderer),
            typeof(MeshRenderer),
            typeof(SkinnedMeshRenderer),
            typeof(Animation),
            typeof(Animator),
            typeof(Camera),
            typeof(Light),
            typeof(LODGroup)
        };
        foreach (Type type in array)
        {
            UnityEngine.Object[] array2 = UnityEngine.Object.FindObjectsOfType(type, includeInactive: true);
            results.Add($"{type.Name}(s) in scene: {array2.Length}");
        }
        array = new Type[6]
        {
            typeof(UnityEngine.Object),
            typeof(GameObject),
            typeof(Texture),
            typeof(AudioClip),
            typeof(AnimationClip),
            typeof(Mesh)
        };
        foreach (Type type2 in array)
        {
            UnityEngine.Object[] array3 = Resources.FindObjectsOfTypeAll(type2);
            results.Add($"{type2.Name}(s) in resources: {array3.Length}");
        }
    }

    internal static void ExecuteAndCopyToClipboard()
    {
        List<string> list = new List<string>();
        GatherInfo(list);
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{list.Count} memory usage result(s):");
        for (int i = 0; i < list.Count; i++)
        {
            stringBuilder.AppendLine($"[{i}] {list[i]}");
        }
        GUIUtility.systemCopyBuffer = stringBuilder.ToString();
    }

    protected override void execute(CSteamID executorID, string parameter)
    {
        List<string> list = new List<string>();
        GatherInfo(list);
        CommandWindow.Log($"{list.Count} memory usage result(s):");
        for (int i = 0; i < list.Count; i++)
        {
            CommandWindow.Log($"[{i}] {list[i]}");
        }
    }

    public CommandLogMemoryUsage(Local newLocalization)
    {
        localization = newLocalization;
        _command = "LogMemoryUsage";
        _info = string.Empty;
        _help = string.Empty;
    }
}
