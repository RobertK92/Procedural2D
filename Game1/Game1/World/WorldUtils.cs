using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1
{
    public static class WorldUtils
    {
        public static TileArrayPosition GetTileAbove(Region region, TileId[,] tiles, int chunkX, int chunkY, int tileX, int tileY)
        {
            TileArrayPosition tap = new TileArrayPosition();
            TileId result = TileId.Empty;
            if (tileY > 0)
            {
                result = tiles[tileX, tileY - 1];
                tap.TileX = tileX;
                tap.TileY = tileY - 1;
                tap.Chunk = region.GetChunk(chunkX, chunkY);
            }
            else
            {
                if (chunkY > region.offsetY)
                {
                    Chunk above = region.GetChunk(chunkX, chunkY - 1);
                    result = above.Data.Tiles[tileX, (region.World.Config.ChunkHeight - 1)];
                    tap.Chunk = above;
                }
                else
                {
                    tap.Chunk = region.GetChunk(tileX, region.offsetY);
                }

                tap.TileX = tileX;
                tap.TileY = region.World.Config.ChunkHeight - 1;
            }

            tap.Id = result;
            return tap;
        }

        public static TileArrayPosition GetTileLeft(Region region, TileId[,] tiles, int chunkX, int chunkY, int tileX, int tileY)
        {
            TileArrayPosition tap = new TileArrayPosition();
            TileId result = TileId.Empty;
            if (tileX > 0)
            {
                result = tiles[tileX - 1, tileY];
                tap.Chunk = region.GetChunk(chunkX, chunkY);
                tap.TileX = tileX - 1;
                tap.TileY = tileY;
            }
            else
            {
                if (chunkX > 0)
                {
                    Chunk left = region.GetChunk(chunkX - 1, chunkY);
                    result = left.Data.Tiles[(region.World.Config.ChunkWidth - 1), tileY];
                }
                else
                {
                    tap.Chunk = region.GetChunk(0, chunkY);
                }

                tap.TileX = region.World.Config.ChunkWidth - 1;
                tap.TileY = tileY;
            }
            
            tap.Id = result;
            return tap;
        }
    }
}
