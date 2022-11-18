namespace SDG.Framework.IO.Streams;

public interface INetworkStreamable
{
    void readFromStream(NetworkStream networkStream);

    void writeToStream(NetworkStream networkStream);
}
