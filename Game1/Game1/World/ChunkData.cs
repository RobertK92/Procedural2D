using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Game1
{
    

    public struct ChunkData
    {
        private static ChunkData _empty = new ChunkData(-1, -1, null, null);
        public static ChunkData Empty
        {
            get
            {
                return _empty;
            }
        }

        public readonly Region Region;
        public TileId[,] Tiles;
        public List<WorldObjectDef> Objects;
        public Point ChunkPosition;
        
        public ChunkData(int chunkX, int chunkY, TileId[,] tiles, Region region)
        {
            this.ChunkPosition = new Point(chunkX, chunkY);
            this.Tiles = tiles;
            this.Region = region;
            this.Objects = new List<WorldObjectDef>();
        }

    }
}
