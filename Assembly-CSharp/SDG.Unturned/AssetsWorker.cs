using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace SDG.Unturned;

/// <summary>
/// Responsible for loading asset definitions on a separate thread.
/// </summary>
internal class AssetsWorker
{
    public enum EResultType
    {
        MasterBundle,
        Asset,
        Exception
    }

    public abstract class ResultItem
    {
        public abstract EResultType ResultType { get; }
    }

    public sealed class MasterBundle : ResultItem
    {
        public MasterBundleConfig config;

        public byte[] assetBundleData;

        public byte[] hash;

        public override EResultType ResultType => EResultType.MasterBundle;
    }

    public sealed class AssetDefinition : ResultItem
    {
        public string path;

        public byte[] hash;

        public IDatDictionary assetData;

        public IDatDictionary translationData;

        public IDatDictionary fallbackTranslationData;

        /// <summary>
        /// Parser error messages, if any.
        /// </summary>
        public List<string> assetErrors;

        /// <summary>
        /// Warning: on worker thread this only acts as handle. Do not access.
        /// </summary>
        public AssetOrigin origin;

        public override EResultType ResultType => EResultType.Asset;
    }

    public sealed class ExceptionDetails : ResultItem
    {
        public string message;

        public Exception exception;

        public override EResultType ResultType => EResultType.Exception;
    }

    private class WorkerThreadState
    {
        public struct ReaderWorkItem
        {
            public enum EItemType
            {
                MasterBundle,
                Asset
            }

            public string filePath;

            public EItemType itemType;
        }

        public AssetsWorker owner;

        public DatParser datParser;

        public string rootPath;

        public Queue<string> searchPaths;

        public ConcurrentQueue<ReaderWorkItem> readerWorkItems;

        public int isFinishedSearching;

        /// <summary>
        /// Warning: on worker thread this only acts as handle. Do not access.
        /// </summary>
        public AssetOrigin origin;

        public IDatDictionary ReadFileWithoutHash(string path)
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
                    readerWorkItems.Enqueue(new ReaderWorkItem
                    {
                        filePath = text,
                        itemType = ReaderWorkItem.EItemType.Asset
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
                    readerWorkItems.Enqueue(new ReaderWorkItem
                    {
                        filePath = text,
                        itemType = ReaderWorkItem.EItemType.Asset
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
                    readerWorkItems.Enqueue(new ReaderWorkItem
                    {
                        filePath = text,
                        itemType = ReaderWorkItem.EItemType.Asset
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
                    readerWorkItems.Enqueue(new ReaderWorkItem
                    {
                        filePath = item,
                        itemType = ReaderWorkItem.EItemType.Asset
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
            IDatDictionary assetData = datParser.Parse(inputReader);
            List<string> assetErrors = (datParser.HasError ? new List<string>(datParser.ErrorMessages) : null);
            byte[] hash = sHA1Stream.Hash;
            IDatDictionary translationData = null;
            IDatDictionary fallbackTranslationData = null;
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
            owner.resultItems.Enqueue(new AssetDefinition
            {
                path = filePath,
                assetData = assetData,
                assetErrors = assetErrors,
                hash = hash,
                translationData = translationData,
                fallbackTranslationData = fallbackTranslationData,
                origin = origin
            });
        }

        public void AddException(Exception exception, string message)
        {
            owner.resultItems.Enqueue(new ExceptionDetails
            {
                message = message,
                exception = exception
            });
        }
    }

    private int shouldWorkerThreadsContinue;

    private ConcurrentQueue<ResultItem> resultItems;

    internal int totalMasterBundlesFound;

    internal int totalMasterBundlesRead;

    internal int totalAssetDefinitionsFound;

    internal int totalAssetDefinitionsRead;

    internal int totalSearchLocationRequests;

    internal int totalSearchLocationsFinishedSearching;

    internal int totalSearchLocationsFinishedReading;

    private bool isWorking;

    private string language;

    private bool languageIsEnglish;

    /// <summary>
    /// Used on main thread to determine when all queued tasks have finished.
    /// </summary>
    public bool IsWorking => isWorking;

    public void Initialize()
    {
        language = Provider.language;
        languageIsEnglish = Provider.languageIsEnglish;
        shouldWorkerThreadsContinue = 1;
        resultItems = new ConcurrentQueue<ResultItem>();
    }

    public void Shutdown()
    {
        Interlocked.Exchange(ref shouldWorkerThreadsContinue, 0);
    }

    public void RequestSearch(string path, AssetOrigin origin)
    {
        totalSearchLocationRequests++;
        WorkerThreadState workerThreadState = new WorkerThreadState
        {
            owner = this,
            rootPath = path,
            searchPaths = new Queue<string>(),
            readerWorkItems = new ConcurrentQueue<WorkerThreadState.ReaderWorkItem>(),
            datParser = new DatParser(),
            origin = origin
        };
        workerThreadState.datParser.EnableMetadata = Assets.shouldParseMetadata;
        ThreadPool.QueueUserWorkItem(SearcherThreadMain, workerThreadState);
        ThreadPool.QueueUserWorkItem(ReaderThreadMain, workerThreadState);
        isWorking = true;
    }

    public bool TryDequeueResult(out ResultItem result)
    {
        return resultItems.TryDequeue(out result);
    }

    public void Update()
    {
        isWorking = totalSearchLocationRequests > totalSearchLocationsFinishedReading || resultItems.Count > 0;
    }

    /// <summary>
    /// Loop searching directories recursively for asset bundle and asset definition files.
    /// </summary>
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
                    workerThreadState.readerWorkItems.Enqueue(new WorkerThreadState.ReaderWorkItem
                    {
                        filePath = text2,
                        itemType = WorkerThreadState.ReaderWorkItem.EItemType.MasterBundle
                    });
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

    private async void ReaderThreadMain(object untypedState)
    {
        WorkerThreadState state = (WorkerThreadState)untypedState;
        while (shouldWorkerThreadsContinue > 0)
        {
            bool flag = state.isFinishedSearching > 0;
            if (state.readerWorkItems.TryDequeue(out var workItem))
            {
                if (workItem.itemType == WorkerThreadState.ReaderWorkItem.EItemType.MasterBundle)
                {
                    try
                    {
                        IDatDictionary data = state.ReadFileWithoutHash(workItem.filePath);
                        string directoryName = Path.GetDirectoryName(workItem.filePath);
                        MasterBundleConfig config = new MasterBundleConfig(directoryName, data, state.origin);
                        byte[] assetBundleData;
                        byte[] hash;
                        using (FileStream fileStream = new FileStream(config.getAssetBundlePath(), FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            using SHA1Stream hashStream = new SHA1Stream(fileStream);
                            using MemoryStream memoryStream = new MemoryStream();
                            await hashStream.CopyToAsync(memoryStream);
                            assetBundleData = memoryStream.ToArray();
                            hash = hashStream.Hash;
                        }
                        Interlocked.Increment(ref totalMasterBundlesRead);
                        state.owner.resultItems.Enqueue(new MasterBundle
                        {
                            config = config,
                            assetBundleData = assetBundleData,
                            hash = hash
                        });
                    }
                    catch (Exception exception)
                    {
                        state.AddException(exception, "Caught exception reading master bundle config at: \"" + workItem.filePath + "\"");
                    }
                }
                else if (workItem.itemType == WorkerThreadState.ReaderWorkItem.EItemType.Asset)
                {
                    try
                    {
                        state.AddFoundAsset(workItem.filePath, checkForTranslations: true);
                    }
                    catch (Exception exception2)
                    {
                        state.AddException(exception2, "Caught exception reading asset definition at: \"" + workItem.filePath + "\"");
                    }
                }
            }
            else
            {
                if (flag)
                {
                    break;
                }
                workItem = default(WorkerThreadState.ReaderWorkItem);
            }
        }
        Interlocked.Increment(ref totalSearchLocationsFinishedReading);
    }
}
