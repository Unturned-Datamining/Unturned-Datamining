using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace SDG.Unturned;

internal class AssetsWorker
{
    public struct MasterBundle
    {
        public MasterBundleConfig config;

        public byte[] assetBundleData;

        public byte[] hash;
    }

    public struct AssetDefinition
    {
        public string path;

        public byte[] hash;

        public DatDictionary assetData;

        public DatDictionary translationData;

        public DatDictionary fallbackTranslationData;

        public string assetError;

        public AssetOrigin origin;
    }

    private class WorkerThreadState
    {
        public struct AssetDefFilePath
        {
            public string filePath;

            public bool checkForTranslations;
        }

        public AssetsWorker owner;

        public DatParser datParser;

        public string rootPath;

        public Queue<string> searchPaths;

        public ConcurrentQueue<string> masterBundleFilePaths;

        public ConcurrentQueue<AssetDefFilePath> assetDefinitionFilePaths;

        public int isFinishedSearching;

        public AssetOrigin origin;

        public DatDictionary ReadFileWithoutHash(string path)
        {
            using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            using StreamReader inputReader = new StreamReader(stream);
            return datParser.Parse(inputReader);
        }

        public void FindAssets(string dirPath, AssetOrigin origin)
        {
            string fileName = Path.GetFileName(dirPath);
            string text = Path.Combine(dirPath, fileName + ".asset");
            try
            {
                if (File.Exists(text))
                {
                    Interlocked.Increment(ref owner.totalAssetDefinitionsFound);
                    assetDefinitionFilePaths.Enqueue(new AssetDefFilePath
                    {
                        filePath = text,
                        checkForTranslations = true
                    });
                    return;
                }
            }
            catch (Exception exception)
            {
                AddException(exception, "Caught exception checking for assets at: \"" + text + "\"");
                return;
            }
            text = Path.Combine(dirPath, fileName + ".dat");
            try
            {
                if (File.Exists(text))
                {
                    Interlocked.Increment(ref owner.totalAssetDefinitionsFound);
                    assetDefinitionFilePaths.Enqueue(new AssetDefFilePath
                    {
                        filePath = text,
                        checkForTranslations = true
                    });
                    return;
                }
            }
            catch (Exception exception2)
            {
                AddException(exception2, "Caught exception checking for assets at: \"" + text + "\"");
                return;
            }
            text = Path.Combine(dirPath, "Asset.dat");
            try
            {
                if (File.Exists(text))
                {
                    Interlocked.Increment(ref owner.totalAssetDefinitionsFound);
                    assetDefinitionFilePaths.Enqueue(new AssetDefFilePath
                    {
                        filePath = text,
                        checkForTranslations = true
                    });
                    return;
                }
            }
            catch (Exception exception3)
            {
                AddException(exception3, "Caught exception checking for assets at: \"" + text + "\"");
                return;
            }
            try
            {
                foreach (string item in Directory.EnumerateFiles(dirPath, "*.asset"))
                {
                    Interlocked.Increment(ref owner.totalAssetDefinitionsFound);
                    assetDefinitionFilePaths.Enqueue(new AssetDefFilePath
                    {
                        filePath = item,
                        checkForTranslations = true
                    });
                }
            }
            catch (Exception exception4)
            {
                AddException(exception4, "Caught exception checking for assets in: \"" + dirPath + "\"");
            }
        }

        public void AddFoundAsset(string filePath, bool checkForTranslations)
        {
            string directoryName = Path.GetDirectoryName(filePath);
            using FileStream underlyingStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using SHA1Stream sHA1Stream = new SHA1Stream(underlyingStream);
            using StreamReader inputReader = new StreamReader(sHA1Stream);
            DatDictionary assetData = datParser.Parse(inputReader);
            string errorMessage = datParser.ErrorMessage;
            byte[] hash = sHA1Stream.Hash;
            DatDictionary translationData = null;
            DatDictionary fallbackTranslationData = null;
            if (checkForTranslations)
            {
                string path = Path.Combine(directoryName, owner.language + ".dat");
                string path2 = Path.Combine(directoryName, "English.dat");
                if (File.Exists(path))
                {
                    translationData = ReadFileWithoutHash(path);
                    if (!owner.languageIsEnglish && File.Exists(path2))
                    {
                        fallbackTranslationData = ReadFileWithoutHash(path2);
                    }
                }
                else if (File.Exists(path2))
                {
                    translationData = ReadFileWithoutHash(path2);
                }
            }
            Interlocked.Increment(ref owner.totalAssetDefinitionsRead);
            owner.foundAssetDefinitions.Enqueue(new AssetDefinition
            {
                path = filePath,
                assetData = assetData,
                assetError = errorMessage,
                hash = hash,
                translationData = translationData,
                fallbackTranslationData = fallbackTranslationData,
                origin = origin
            });
        }

        public void AddException(Exception exception, string message)
        {
            owner.exceptions.Enqueue(new ExceptionDetails
            {
                message = message,
                exception = exception
            });
        }
    }

    private struct ExceptionDetails
    {
        public string message;

        public Exception exception;
    }

    private int shouldWorkerThreadsContinue;

    private ConcurrentQueue<MasterBundle> foundMasterBundles;

    private ConcurrentQueue<AssetDefinition> foundAssetDefinitions;

    internal int totalMasterBundlesFound;

    internal int totalMasterBundlesRead;

    internal int totalAssetDefinitionsFound;

    internal int totalAssetDefinitionsRead;

    internal int totalSearchLocationRequests;

    internal int totalSearchLocationsFinishedSearching;

    internal int totalSearchLocationsFinishedReading;

    private ConcurrentQueue<ExceptionDetails> exceptions;

    private bool isWorking;

    private string language;

    private bool languageIsEnglish;

    public bool IsWorking => isWorking;

    public void Initialize()
    {
        language = Provider.language;
        languageIsEnglish = Provider.languageIsEnglish;
        shouldWorkerThreadsContinue = 1;
        foundMasterBundles = new ConcurrentQueue<MasterBundle>();
        foundAssetDefinitions = new ConcurrentQueue<AssetDefinition>();
        exceptions = new ConcurrentQueue<ExceptionDetails>();
    }

    public void Shutdown()
    {
        Interlocked.Exchange(ref shouldWorkerThreadsContinue, 0);
    }

    public void RequestSearch(string path, AssetOrigin origin)
    {
        totalSearchLocationRequests++;
        WorkerThreadState state = new WorkerThreadState
        {
            owner = this,
            rootPath = path,
            searchPaths = new Queue<string>(),
            masterBundleFilePaths = new ConcurrentQueue<string>(),
            assetDefinitionFilePaths = new ConcurrentQueue<WorkerThreadState.AssetDefFilePath>(),
            datParser = new DatParser(),
            origin = origin
        };
        ThreadPool.QueueUserWorkItem(SearcherThreadMain, state);
        ThreadPool.QueueUserWorkItem(ReaderThreadMain, state);
        isWorking = true;
    }

    public bool TryDequeueMasterBundle(out MasterBundle result)
    {
        return foundMasterBundles.TryDequeue(out result);
    }

    public bool TryDequeueAssetDefinition(out AssetDefinition result)
    {
        return foundAssetDefinitions.TryDequeue(out result);
    }

    public void Update()
    {
        isWorking = totalSearchLocationRequests > totalSearchLocationsFinishedReading || foundMasterBundles.Count > 0 || foundAssetDefinitions.Count > 0;
        ExceptionDetails result;
        while (exceptions.TryDequeue(out result))
        {
            UnturnedLog.exception(result.exception, result.message);
        }
    }

    private void SearcherThreadMain(object untypedState)
    {
        WorkerThreadState workerThreadState = (WorkerThreadState)untypedState;
        workerThreadState.searchPaths.Enqueue(workerThreadState.rootPath);
        while (shouldWorkerThreadsContinue > 0 && workerThreadState.searchPaths.Count > 0)
        {
            string text = workerThreadState.searchPaths.Dequeue();
            string text2 = Path.Combine(text, "MasterBundle.dat");
            try
            {
                if (File.Exists(text2))
                {
                    Interlocked.Increment(ref totalMasterBundlesFound);
                    workerThreadState.masterBundleFilePaths.Enqueue(text2);
                }
            }
            catch (Exception exception)
            {
                workerThreadState.AddException(exception, "Caught exception reading master bundle config at: \"" + text2 + "\"");
            }
            workerThreadState.FindAssets(text, workerThreadState.origin);
            try
            {
                foreach (string item in Directory.EnumerateDirectories(text))
                {
                    workerThreadState.searchPaths.Enqueue(item);
                }
            }
            catch (Exception exception2)
            {
                workerThreadState.AddException(exception2, "Caught exception finding asset subdirectories in: \"" + text + "\"");
            }
        }
        Interlocked.Exchange(ref workerThreadState.isFinishedSearching, 1);
        Interlocked.Increment(ref totalSearchLocationsFinishedSearching);
    }

    private void ReaderThreadMain(object untypedState)
    {
        WorkerThreadState workerThreadState = (WorkerThreadState)untypedState;
        while (shouldWorkerThreadsContinue > 0)
        {
            WorkerThreadState.AssetDefFilePath result2;
            if (workerThreadState.masterBundleFilePaths.TryDequeue(out var result))
            {
                try
                {
                    DatDictionary data = workerThreadState.ReadFileWithoutHash(result);
                    MasterBundleConfig masterBundleConfig = new MasterBundleConfig(Path.GetDirectoryName(result), data, workerThreadState.origin);
                    byte[] assetBundleData;
                    byte[] hash;
                    using (FileStream underlyingStream = new FileStream(masterBundleConfig.getAssetBundlePath(), FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using SHA1Stream sHA1Stream = new SHA1Stream(underlyingStream);
                        using MemoryStream memoryStream = new MemoryStream();
                        sHA1Stream.CopyTo(memoryStream);
                        assetBundleData = memoryStream.ToArray();
                        hash = sHA1Stream.Hash;
                    }
                    Interlocked.Increment(ref totalMasterBundlesRead);
                    workerThreadState.owner.foundMasterBundles.Enqueue(new MasterBundle
                    {
                        config = masterBundleConfig,
                        assetBundleData = assetBundleData,
                        hash = hash
                    });
                }
                catch (Exception exception)
                {
                    workerThreadState.AddException(exception, "Caught exception reading master bundle config at: \"" + result + "\"");
                }
            }
            else if (workerThreadState.assetDefinitionFilePaths.TryDequeue(out result2))
            {
                try
                {
                    workerThreadState.AddFoundAsset(result2.filePath, result2.checkForTranslations);
                }
                catch (Exception exception2)
                {
                    workerThreadState.AddException(exception2, "Caught exception reading asset definition at: \"" + result2.filePath + "\"");
                }
            }
            else if (workerThreadState.isFinishedSearching > 0)
            {
                break;
            }
        }
        Interlocked.Increment(ref totalSearchLocationsFinishedReading);
    }
}
