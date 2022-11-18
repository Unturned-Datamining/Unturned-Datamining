using System.Collections.Generic;

namespace SDG.Unturned;

public static class DecalSystem
{
    private static bool _isVisible;

    private static HashSet<Decal> _decalsDiffuse;

    public static bool IsVisible
    {
        get
        {
            return _isVisible;
        }
        set
        {
            if (_isVisible == value)
            {
                return;
            }
            _isVisible = value;
            ConvenientSavedata.get().write("Visibility_Decals", value);
            if (!Level.isEditor)
            {
                return;
            }
            foreach (Decal item in decalsDiffuse)
            {
                item.UpdateEditorVisibility();
            }
        }
    }

    public static HashSet<Decal> decalsDiffuse => _decalsDiffuse;

    public static void add(Decal decal)
    {
        if (!(decal == null) && !(decal.material == null))
        {
            remove(decal);
            if (decal.type == EDecalType.DIFFUSE)
            {
                decalsDiffuse.Add(decal);
            }
        }
    }

    public static void remove(Decal decal)
    {
        if (!(decal == null) && decal.type == EDecalType.DIFFUSE)
        {
            decalsDiffuse.Remove(decal);
        }
    }

    static DecalSystem()
    {
        _decalsDiffuse = new HashSet<Decal>();
        if (ConvenientSavedata.get().read("Visibility_Decals", out bool value))
        {
            _isVisible = value;
        }
        else
        {
            _isVisible = true;
        }
    }
}
