using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class CommandLogMemoryUsage : Command
{
    internal static Action<List<string>> OnExecuted;

    protected override void execute(CSteamID executorID, string parameter)
    {
        List<string> list = new List<string>();
        OnExecuted?.Invoke(list);
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
            list.Add($"{type.Name}(s) in scene: {array2.Length}");
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
            list.Add($"{type2.Name}(s) in resources: {array3.Length}");
        }
        CommandWindow.Log($"{list.Count} memory usage result(s):");
        for (int j = 0; j < list.Count; j++)
        {
            CommandWindow.Log($"[{j}] {list[j]}");
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
