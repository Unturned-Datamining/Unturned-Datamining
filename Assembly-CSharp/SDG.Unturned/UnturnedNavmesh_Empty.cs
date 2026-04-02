using UnityEngine;

namespace SDG.Unturned;

public class UnturnedNavmesh_Empty : IUnturnedNavmeshInterface
{
    private Vector3 boundsCenter;

    private Vector3 boundsSize;

    private int tileXCount;

    private int tileZCount;

    private int[][] triangleArrays;

    private Vector3Int[][] vertexArrays;

    public bool ContainsAnyBakedData => false;

    public void Deserialize(River river)
    {
        boundsCenter = river.readSingleVector3();
        boundsSize = river.readSingleVector3();
        tileXCount = river.readByte();
        tileZCount = river.readByte();
        triangleArrays = new int[tileXCount * tileZCount][];
        vertexArrays = new Vector3Int[tileXCount * tileZCount][];
        int num = 0;
        for (int i = 0; i < tileZCount; i++)
        {
            for (int j = 0; j < tileXCount; j++)
            {
                int[] array = new int[river.readUInt16()];
                for (int k = 0; k < array.Length; k++)
                {
                    array[k] = river.readUInt16();
                }
                Vector3Int[] array2 = new Vector3Int[river.readUInt16()];
                for (int l = 0; l < array2.Length; l++)
                {
                    array2[l] = new Vector3Int(river.readInt32(), river.readInt32(), river.readInt32());
                }
                triangleArrays[num] = array;
                vertexArrays[num] = array2;
                num++;
            }
        }
    }

    public void Serialize(River river)
    {
        river.writeSingleVector3(boundsCenter);
        river.writeSingleVector3(boundsSize);
        river.writeByte((byte)tileXCount);
        river.writeByte((byte)tileZCount);
        int num = 0;
        for (int i = 0; i < tileZCount; i++)
        {
            for (int j = 0; j < tileXCount; j++)
            {
                int[] array = triangleArrays[num];
                Vector3Int[] array2 = vertexArrays[num];
                river.writeUInt16((ushort)array.Length);
                for (int k = 0; k < array.Length; k++)
                {
                    river.writeUInt16((ushort)array[k]);
                }
                river.writeUInt16((ushort)array2.Length);
                for (int l = 0; l < array2.Length; l++)
                {
                    Vector3Int vector3Int = array2[l];
                    river.writeInt32(vector3Int.x);
                    river.writeInt32(vector3Int.y);
                    river.writeInt32(vector3Int.z);
                }
                num++;
            }
        }
    }
}
