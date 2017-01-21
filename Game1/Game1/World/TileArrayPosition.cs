
namespace Game1
{
    public struct TileArrayPosition
    {
        public TileId Id { get; set; }
        public int TileX { get; set; }
        public int TileY { get; set; }
        public Chunk Chunk { get; set; }
    }
}
