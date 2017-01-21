using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1
{
    public class TileDataGenerator
    {
        private World _world;
        
        private Dictionary<RegionId, RegionGenerator> _regionGenerators = new Dictionary<RegionId, RegionGenerator>();

        public TileDataGenerator(World world)
        {
            _world = world;

            _regionGenerators.Add(RegionId.IcyPeaks,    null);
            _regionGenerators.Add(RegionId.Snow,        null);
            _regionGenerators.Add(RegionId.Alaska,      null);
            _regionGenerators.Add(RegionId.Grasslands,  new GrasslandsGenerator(world, world.GetRegion(RegionId.Grasslands)));
            _regionGenerators.Add(RegionId.Jungle,      null);
            _regionGenerators.Add(RegionId.Desert,      null);
            _regionGenerators.Add(RegionId.Lavalands,   null);
        }

        public void GenerateTileDataForChunk(Region region, int x, int y)
        {
            TileId[,] tiles = new TileId[_world.Config.ChunkWidth, _world.Config.ChunkHeight];
            List<WorldObjectDef> objects = new List<WorldObjectDef>();

            /* Reset last tile indices */
            foreach (KeyValuePair<RegionId, RegionGenerator> generator in _regionGenerators)
            {
                if (generator.Value == null)
                    continue;
                generator.Value.LastTx = 0;
                generator.Value.LastTx = 0;
            }
            
            /* Generate region tile data and world objects */
            for (int ty = 0; ty < _world.Config.ChunkHeight; ty++)
            {
                for (int tx = 0; tx < _world.Config.ChunkWidth; tx++)
                {
                    RegionGenerator generator = _regionGenerators[region.Id];
                    if (generator != null)
                    {
                        generator.GenerateTileData(ref tiles, ref objects, x, y, tx, ty);
                    }
                    else
                    {
                        tiles[tx, ty] = TileId.Empty;
                    }
                }
            }

            /* Create chunks using generated data */
            ChunkData data = new ChunkData(x, y, tiles, region);
            Chunk chunk = new Chunk(null, data);
            chunk.Data.Objects = objects;
            region.chunks[x, y - region.offsetY] = chunk;
        }
        
    }
}
