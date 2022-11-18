using System.IO;

namespace SDG.Provider.Services.Multiplayer;

public delegate void NetworkingReadingCallback(MemoryStream bufferStream, BinaryReader bufferReader);
