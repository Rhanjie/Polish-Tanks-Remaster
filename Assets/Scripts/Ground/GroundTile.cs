using UnityEngine;
using UnityEngine.Tilemaps;

namespace PolishTanks.Ground
{
    /// <summary>
    /// Tile painted on the ground tilemap. It only says which <see cref="GroundLayer"/> the cell is; the
    /// <see cref="GroundSurface"/> draws the blended ground. The preview sprite is what the tile palette shows.
    /// </summary>
    [CreateAssetMenu(menuName = "Polish Tanks/Ground Tile", fileName = "GroundTile")]
    public class GroundTile : TileBase
    {
        [SerializeField, Min(0)] private int layer;
        [SerializeField] private Sprite preview;
        [SerializeField] private Color previewColor = Color.white;

        public int Layer => layer;

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            tileData.sprite = preview;
            tileData.color = previewColor;
            tileData.flags = TileFlags.LockColor;
            tileData.colliderType = Tile.ColliderType.None;
        }
    }
}
