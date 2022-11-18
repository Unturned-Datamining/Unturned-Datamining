using System.Collections.Generic;

namespace SDG.Framework.Foliage;

public interface IFoliageStorage
{
    void Initialize();

    void Shutdown();

    void TileBecameRelevantToViewer(FoliageTile tile);

    void TileNoLongerRelevantToViewer(FoliageTile tile);

    void Update();

    void EditorLoadAllTiles(IEnumerable<FoliageTile> tiles);

    void EditorSaveAllTiles(IEnumerable<FoliageTile> tiles);
}
