using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class TileBaseFactory : TileBase
{
    protected const string CreateAssetMenuPath = ProjectConstants.ContentFolder + "TileBase Factories/";

    private TileBase cachedTileBase;

    public TileBase TileBase
    {
        get
        {
            if (cachedTileBase == null)
            {
                cachedTileBase = CreateTileBase();
            }
            
            return cachedTileBase;
        }
    }

    protected abstract TileBase CreateTileBase();
}