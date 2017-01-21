
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game1
{
    /* Enum order determines region height in the world */
    public enum RegionId
    {
        IcyPeaks,
        Snow,
        Alaska,
        Grasslands,
        Jungle,
        Desert,
        Lavalands
    }

    public class Region
    {
        private World _world;
        public World World { get { return _world; } }

        private RegionId _id;
        public RegionId Id { get { return _id; } }

        private float _height;
        /// <summary>
        /// Percentage of the world height.
        /// </summary>
        public float Height { get { return _height; } }

        
        public Chunk[,] chunks;

        public readonly int ChunksLengthX;
        public readonly int ChunksLengthY;
        public readonly int offsetY;

        public Region(World world, RegionId id, int widthInChunks, int heightInChunks, int offsetY, float height)
        {
            this._world = world;
            this.offsetY = offsetY;
            _id = id;
            _height = height;
            chunks = new Chunk[widthInChunks, heightInChunks];
            ChunksLengthX = widthInChunks;
            ChunksLengthY = heightInChunks;
        }

        public void Destroy()
        {
            for (int y = 0; y < ChunksLengthY; y++)
            {
                for (int x = 0; x < ChunksLengthX; x++)
                {
                    chunks[x, y].Destroy();
                }
            }
        }

        public Chunk GetChunk(int chunkX, int chunkY)
        {
            return GetChunk(new Point(chunkX, chunkY));
        }

        public Chunk GetChunk(Point chunkPosition)
        {
            return chunks[chunkPosition.X, chunkPosition.Y - offsetY];
        }
    }
}
