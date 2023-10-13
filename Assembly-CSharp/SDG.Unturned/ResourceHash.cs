using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Unturned.UnityEx;

namespace SDG.Unturned;

public class ResourceHash
{
    private class ResourceHashThreadState
    {
        public List<string> logMessages;

        public byte[] hash;

        public Stopwatch watch;

        public bool shouldLogVerbose;
    }

    public static byte[] localHash = new byte[20];

    private static bool wasInitialized;

    private static CommandLineFlag shouldSkipHashing = new CommandLineFlag(defaultValue: false, "-SkipResourcesHashing");

    private static CommandLineFlag shouldLogHash = new CommandLineFlag(defaultValue: false, "-LogResourcesHash");

    public static void Initialize()
    {
        if (wasInitialized)
        {
            return;
        }
        wasInitialized = true;
        if (Dedicator.IsDedicatedServer)
        {
            return;
        }
        if ((bool)shouldSkipHashing)
        {
            UnturnedLog.info("Skipping resources hashing");
            return;
        }
        ResourceHashThreadState resourceHashThreadState = new ResourceHashThreadState();
        resourceHashThreadState.shouldLogVerbose = shouldLogHash;
        resourceHashThreadState.logMessages = new List<string>();
        if (resourceHashThreadState.shouldLogVerbose)
        {
            UnturnedLog.info("Queueing resources hashing work item...");
        }
        resourceHashThreadState.watch = Stopwatch.StartNew();
        ThreadPool.QueueUserWorkItem(ThreadInitialize, resourceHashThreadState);
    }

    public static List<string> GatherFilePaths(string dataPath)
    {
        List<string> list = new List<string>();
        GatherFilePathsInDirectory(dataPath, list);
        GatherFilePathsInDirectory(Path.Combine(dataPath, "Resources"), list);
        return list;
    }

    private static async void ThreadInitialize(object voidState)
    {
        ResourceHashThreadState state = (ResourceHashThreadState)voidState;
        string fullName = UnityPaths.GameDataDirectory.FullName;
        try
        {
            if (Directory.Exists(fullName))
            {
                long previousElapsedMs = state.watch.ElapsedMilliseconds;
                List<string> list = GatherFilePaths(fullName);
                List<byte[]> hashes = new List<byte[]>();
                foreach (string path in list)
                {
                    using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    using SHA1Stream hashStream = new SHA1Stream(fileStream);
                    using MemoryStream memoryStream = new MemoryStream();
                    await hashStream.CopyToAsync(memoryStream);
                    byte[] hash = hashStream.Hash;
                    hashes.Add(hash);
                    if (state.shouldLogVerbose)
                    {
                        long elapsedMilliseconds = state.watch.ElapsedMilliseconds;
                        long num = elapsedMilliseconds - previousElapsedMs;
                        previousElapsedMs = elapsedMilliseconds;
                        state.logMessages.Add($"Including {path} in resources hash: {Hash.toString(hash)} ({num} ms)");
                    }
                }
                byte[] hash2 = Hash.combine(hashes);
                if (state.shouldLogVerbose)
                {
                    state.logMessages.Add("Combined resources hash: " + Hash.toString(hash2));
                }
                state.hash = hash2;
            }
            else
            {
                state.logMessages.Add("Resources data path does not exist (" + fullName + ")");
            }
        }
        catch (Exception ex)
        {
            state.logMessages.Add("Caught exception hashing resources: " + ex.Message);
        }
        GameThreadQueueUtil.QueueGameThreadWorkItem(OnHashReady, state);
    }

    private static void OnHashReady(object voidState)
    {
        ResourceHashThreadState resourceHashThreadState = (ResourceHashThreadState)voidState;
        resourceHashThreadState.watch.Stop();
        if ((bool)shouldLogHash)
        {
            UnturnedLog.info($"Hash resources: {resourceHashThreadState.watch.ElapsedMilliseconds} ms");
        }
        if (resourceHashThreadState.logMessages != null && resourceHashThreadState.logMessages.Count > 0)
        {
            foreach (string logMessage in resourceHashThreadState.logMessages)
            {
                UnturnedLog.info(logMessage);
            }
        }
        if (resourceHashThreadState.hash != null)
        {
            localHash = resourceHashThreadState.hash;
            UnturnedLog.info("Hashed resources");
        }
        else
        {
            UnturnedLog.error("Hashing resources failed");
        }
    }

    private static void GatherFilePathsInDirectory(string directoryPath, List<string> filePaths)
    {
        List<string> list = new List<string>();
        foreach (FileInfo item in new DirectoryInfo(directoryPath).EnumerateFiles())
        {
            string name = item.Name;
            if (name.Equals("globalgamemanagers", StringComparison.Ordinal) || name.Equals("unity default resources", StringComparison.Ordinal) || name.Equals("unity_builtin_extra", StringComparison.Ordinal) || name.StartsWith("level", StringComparison.Ordinal) || name.EndsWith(".assets", StringComparison.Ordinal) || name.EndsWith(".assets.resS", StringComparison.Ordinal))
            {
                list.Add(name);
            }
        }
        list.Sort(StringComparer.Ordinal);
        foreach (string item2 in list)
        {
            filePaths.Add(Path.Combine(directoryPath, item2));
        }
    }
}
