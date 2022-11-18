using UnityEngine;

namespace SDG.Unturned;

public class NetworkSnapshotBuffer
{
    private int readIndex;

    private int readCount;

    private int writeIndex;

    private int writeCount;

    private ISnapshotInfo lastInfo;

    private float readLast;

    private float readDuration;

    private float readDelay;

    public NetworkSnapshot[] snapshots { get; private set; }

    public ISnapshotInfo getCurrentSnapshot()
    {
        int num = writeCount - readCount;
        if (num <= 0)
        {
            readLast = Time.realtimeSinceStartup;
            return lastInfo;
        }
        if (num > 4)
        {
            if (writeIndex == 0)
            {
                readIndex = snapshots.Length - 1;
            }
            else
            {
                readIndex = writeIndex - 1;
            }
            readCount = writeCount - 1;
            lastInfo = snapshots[readIndex].info;
            readLast = Time.realtimeSinceStartup;
            return lastInfo;
        }
        if (Time.realtimeSinceStartup - readLast > readDuration && num > 1)
        {
            lastInfo = snapshots[readIndex].info;
            readLast += readDuration;
            incrementReadIndex();
        }
        if (Time.realtimeSinceStartup - snapshots[readIndex].timestamp < readDelay)
        {
            readLast = Time.realtimeSinceStartup;
            return lastInfo;
        }
        float delta = Mathf.Clamp01((Time.realtimeSinceStartup - readLast) / readDuration);
        return lastInfo.lerp(snapshots[readIndex].info, delta);
    }

    public void updateLastSnapshot(ISnapshotInfo info)
    {
        readIndex = 0;
        readCount = 0;
        writeIndex = 0;
        writeCount = 0;
        lastInfo = info;
        readLast = Time.realtimeSinceStartup;
    }

    public void addNewSnapshot(ISnapshotInfo info)
    {
        snapshots[writeIndex].info = info;
        snapshots[writeIndex].timestamp = Time.realtimeSinceStartup;
        incrementWriteIndex();
    }

    private void incrementReadIndex()
    {
        readIndex++;
        if (readIndex == snapshots.Length)
        {
            readIndex = 0;
        }
        readCount++;
    }

    private void incrementWriteIndex()
    {
        writeIndex++;
        if (writeIndex == snapshots.Length)
        {
            writeIndex = 0;
        }
        writeCount++;
    }

    public NetworkSnapshotBuffer(float newDuration, float newDelay)
    {
        snapshots = new NetworkSnapshot[8];
        readIndex = 0;
        readCount = 0;
        writeIndex = 0;
        writeCount = 0;
        readDuration = newDuration;
        readDelay = newDelay;
    }
}
