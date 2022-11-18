namespace SDG.Framework.Devkit;

public interface IDirtyable
{
    bool isDirty { get; set; }

    void save();
}
