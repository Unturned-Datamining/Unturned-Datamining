using System;
using SDG.Framework.IO.FormattedFiles.KeyValueTables;
using SDG.Framework.IO.FormattedFiles.KeyValueTables.TypeReaders.CoreTypes;
using SDG.Framework.IO.FormattedFiles.KeyValueTables.TypeReaders.SystemTypes;
using SDG.Framework.IO.FormattedFiles.KeyValueTables.TypeReaders.UnityTypes;
using SDG.Framework.IO.FormattedFiles.KeyValueTables.TypeWriters.CoreTypes;
using SDG.Framework.IO.FormattedFiles.KeyValueTables.TypeWriters.SystemTypes;
using SDG.Framework.IO.FormattedFiles.KeyValueTables.TypeWriters.UnityTypes;
using SDG.Framework.Modules;
using UnityEngine;

namespace SDG.Framework.IO;

public class IONexus : IModuleNexus
{
    public void initialize()
    {
        KeyValueTableTypeReaderRegistry.add<bool>(new KeyValueTableBoolReader());
        KeyValueTableTypeReaderRegistry.add<byte>(new KeyValueTableByteReader());
        KeyValueTableTypeReaderRegistry.add<float>(new KeyValueTableFloatReader());
        KeyValueTableTypeReaderRegistry.add<int>(new KeyValueTableIntReader());
        KeyValueTableTypeReaderRegistry.add<sbyte>(new KeyValueTableSByteReader());
        KeyValueTableTypeReaderRegistry.add<long>(new KeyValueTableLongReader());
        KeyValueTableTypeReaderRegistry.add<short>(new KeyValueTableIntReader());
        KeyValueTableTypeReaderRegistry.add<string>(new KeyValueTableStringReader());
        KeyValueTableTypeReaderRegistry.add<uint>(new KeyValueTableUIntReader());
        KeyValueTableTypeReaderRegistry.add<ulong>(new KeyValueTableULongReader());
        KeyValueTableTypeReaderRegistry.add<ushort>(new KeyValueTableUShortReader());
        KeyValueTableTypeReaderRegistry.add<Guid>(new KeyValueTableGUIDReader());
        KeyValueTableTypeReaderRegistry.add<Type>(new KeyValueTableTypeReader());
        KeyValueTableTypeReaderRegistry.add<Color>(new KeyValueTableColorReader());
        KeyValueTableTypeReaderRegistry.add<Color32>(new KeyValueTableColor32Reader());
        KeyValueTableTypeReaderRegistry.add<Quaternion>(new KeyValueTableQuaternionReader());
        KeyValueTableTypeReaderRegistry.add<Vector2>(new KeyValueTableVector2Reader());
        KeyValueTableTypeReaderRegistry.add<Vector3>(new KeyValueTableVector3Reader());
        KeyValueTableTypeReaderRegistry.add<Vector4>(new KeyValueTableVector4Reader());
        KeyValueTableTypeWriterRegistry.add<bool>(new KeyValueTableBoolWriter());
        KeyValueTableTypeWriterRegistry.add<float>(new KeyValueTableFloatWriter());
        KeyValueTableTypeWriterRegistry.add<double>(new KeyValueTableDoubleWriter());
        KeyValueTableTypeWriterRegistry.add<Guid>(new KeyValueTableGUIDWriter());
        KeyValueTableTypeWriterRegistry.add<Type>(new KeyValueTableTypeWriter());
        KeyValueTableTypeWriterRegistry.add<Color>(new KeyValueTableColorWriter());
        KeyValueTableTypeWriterRegistry.add<Color32>(new KeyValueTableColor32Writer());
        KeyValueTableTypeWriterRegistry.add<Quaternion>(new KeyValueTableQuaternionWriter());
        KeyValueTableTypeWriterRegistry.add<Vector2>(new KeyValueTableVector2Writer());
        KeyValueTableTypeWriterRegistry.add<Vector3>(new KeyValueTableVector3Writer());
        KeyValueTableTypeWriterRegistry.add<Vector4>(new KeyValueTableVector4Writer());
    }

    public void shutdown()
    {
        KeyValueTableTypeReaderRegistry.remove<bool>();
        KeyValueTableTypeReaderRegistry.remove<byte>();
        KeyValueTableTypeReaderRegistry.remove<float>();
        KeyValueTableTypeReaderRegistry.remove<int>();
        KeyValueTableTypeReaderRegistry.remove<long>();
        KeyValueTableTypeReaderRegistry.remove<sbyte>();
        KeyValueTableTypeReaderRegistry.remove<short>();
        KeyValueTableTypeReaderRegistry.remove<string>();
        KeyValueTableTypeReaderRegistry.remove<uint>();
        KeyValueTableTypeReaderRegistry.remove<ulong>();
        KeyValueTableTypeReaderRegistry.remove<ushort>();
        KeyValueTableTypeReaderRegistry.remove<Guid>();
        KeyValueTableTypeReaderRegistry.remove<Type>();
        KeyValueTableTypeReaderRegistry.remove<Color>();
        KeyValueTableTypeReaderRegistry.remove<Color32>();
        KeyValueTableTypeReaderRegistry.remove<Quaternion>();
        KeyValueTableTypeReaderRegistry.remove<Vector2>();
        KeyValueTableTypeReaderRegistry.remove<Vector3>();
        KeyValueTableTypeReaderRegistry.remove<Vector4>();
        KeyValueTableTypeWriterRegistry.remove<bool>();
        KeyValueTableTypeWriterRegistry.remove<Guid>();
        KeyValueTableTypeWriterRegistry.remove<Type>();
        KeyValueTableTypeWriterRegistry.remove<Color>();
        KeyValueTableTypeWriterRegistry.remove<Color32>();
        KeyValueTableTypeWriterRegistry.remove<Quaternion>();
        KeyValueTableTypeWriterRegistry.remove<Vector2>();
        KeyValueTableTypeWriterRegistry.remove<Vector3>();
        KeyValueTableTypeWriterRegistry.remove<Vector4>();
    }
}
