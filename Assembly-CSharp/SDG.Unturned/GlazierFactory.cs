using System;

namespace SDG.Unturned;

public static class GlazierFactory
{
    private static CommandLineString clImpl = new CommandLineString("-Glazier");

    public static void Create()
    {
        throw new NotSupportedException("Glazier should not be used by dedicated server");
    }
}
