using System.IO;

namespace SDG.Provider.Services.Multiplayer;

public delegate void NetworkingWritingCallback(MemoryStream bufferStream, BinaryWriter bufferWriter);
