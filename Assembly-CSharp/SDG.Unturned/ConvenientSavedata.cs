using System;

namespace SDG.Unturned;

public static class ConvenientSavedata
{
    private static ConvenientSavedataImplementation instance = null;

    private static readonly string RELATIVE_PATH = "/Cloud/ConvenientSavedata.json";

    public static IConvenientSavedata get()
    {
        if (instance == null)
        {
            load();
        }
        return instance;
    }

    private static void load()
    {
        if (ReadWrite.fileExists(RELATIVE_PATH, useCloud: false, usePath: true))
        {
            try
            {
                instance = ReadWrite.deserializeJSON<ConvenientSavedataImplementation>(RELATIVE_PATH, useCloud: false, usePath: true);
            }
            catch (Exception e)
            {
                UnturnedLog.exception(e, "Unable to parse {0}! consider validating with a JSON linter", RELATIVE_PATH);
                instance = null;
            }
            if (instance == null)
            {
                instance = new ConvenientSavedataImplementation();
            }
        }
        else
        {
            instance = new ConvenientSavedataImplementation();
        }
    }

    public static void save()
    {
        if (instance == null)
        {
            UnturnedLog.info("Skipped saving convenient data");
            return;
        }
        instance.isDirty = false;
        try
        {
            ReadWrite.serializeJSON(RELATIVE_PATH, useCloud: false, usePath: true, instance);
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, "Caught exception serializing convenient data:");
        }
        UnturnedLog.info("Saved convenient data");
    }

    public static void SaveIfDirty()
    {
        if (instance != null && instance.isDirty)
        {
            instance.isDirty = false;
            ReadWrite.serializeJSON(RELATIVE_PATH, useCloud: false, usePath: true, instance);
            UnturnedLog.info("Saved convenient data (dirty)");
        }
    }
}
