using Microsoft.Xna.Framework;
using MonoGameToolkit;
using System;
using System.Collections.Generic;

namespace Game1
{
    public class GrasslandsGenerator : RegionGenerator
    {
        public readonly TileCorners GrassCorners;

        private TexturePackerAtlas _environment1atlas;

        public GrasslandsGenerator(World world, Region region)
            : base(world, region)
        {
            GrassCorners = new TileCorners(
                TileId.Grass1, 
                TileId.Grass1Top, 
                TileId.Grass1Bottom, 
                TileId.Grass1Right, 
                TileId.Grass1Left,
                TileId.Grass1BottomRightEnclave,
                TileId.Grass1TopLeftEnclave,
                TileId.Grass1BottomLeftEnclave,
                TileId.Grass1TopRightEnclave,
                TileId.Grass1BottomRightCorner,
                TileId.Grass1BottomLeftCorner,
                TileId.Grass1TopRightCorner,
                TileId.Grass1TopLeftCorner);

            _environment1atlas = TexturePacker.LoadAtlas(Strings.Content.Textures.Environment1XML);
        }

        public override void GenerateTileData(ref TileId[,] tiles, ref List<WorldObjectDef> objects, int x, int y, int tx, int ty)
        {
            TileLocation current = new TileLocation(new Point(x, y), new Point(tx, ty));
            
            GenerateGround(x, y, tx, ty, ref tiles);
            GenerateSmallPlants(x, y, tx, ty, ref tiles);
            GenerateTrees(x, y, tx, ty, ref objects);
            GenerateMunshrooms(x, y, tx, ty, ref objects, tiles);
            
        }
        
        private void GenerateGround(int x, int y, int tx, int ty, ref TileId[,] tiles)
        {
            World.Perlin.OctaveCount = World.Config.Grasslands.GroundNoiseOctaves;
            World.Perlin.Frequency = World.Config.Grasslands.GroundNoiseFrequency;
            World.Perlin.Persistence = World.Config.Grasslands.GroundNoisePersistence;
            World.Perlin.Lacunarity = World.Config.Grasslands.GroundNoiseLacunarity;
            double nx = (double)tx / (World.Config.ChunkWidth);
            double ny = (double)ty / (World.Config.ChunkHeight);

            double noise = World.Perlin.GetValue(x + nx, y + ny, 0.0f);

            if (noise > World.Config.Grasslands.GrassFrequency)
            {
                GenerateTile(ref tiles, x, y, tx, ty, TileId.River1, GrassCorners);
            }
            else 
            {
                GenerateTile(ref tiles, x, y, tx, ty, TileId.Grass1, GrassCorners);
            }
        }

        private void GenerateSmallPlants(int x, int y, int tx, int ty, ref TileId[,] tiles)
        {
            if (World.FlipCoin(2.0f))
            {
                GenerateTile(ref tiles, x, y, tx, ty, TileId.Grass1 | TileId.Flowers1, GrassCorners);
            }
            else if (World.FlipCoin(1.0f) )
            {
                GenerateTile(ref tiles, x, y, tx, ty, TileId.Grass1 | TileId.Flowers2, GrassCorners);
            }
        }

        private void GenerateTrees(int x, int y, int tx, int ty, ref List<WorldObjectDef> objects)
        {
            World.Perlin.OctaveCount = 3;
            World.Perlin.Frequency = 0.6f;
            World.Perlin.Persistence = 0.7f;
            World.Perlin.Lacunarity = 2.0;
            double nx = (double)tx / (World.Config.ChunkWidth);
            double ny = (double)ty / (World.Config.ChunkHeight);
            
            double noise = World.Perlin.GetValue(x + nx, y + ny, World.Random.NextDouble());
            
            if (noise > 0.5)
            {
                int treeOffsetX = 3;
                int treeOffsetY = 3;
                int worldY = y + ty;

                if (tx > (LastTx + treeOffsetX) || (worldY > (LastTy + treeOffsetY)))
                {
                    TreeDef tree = null;
                    tree = new TreeDef(World, new Point(x, y), new Point(tx, ty), "TreeNormalBase4", "TreeNormalTop4");
                    
                    objects.Add(tree.Base);
                    objects.Add(tree.Top);
                    LastTx = tx;
                    LastTy = worldY;
                }
            }
        }

        private void GenerateMunshrooms(int x, int y, int tx, int ty, ref List<WorldObjectDef> objects, TileId[,] tiles)
        {
            World.Perlin.OctaveCount = 1;
            World.Perlin.Frequency = 2.0f;
            World.Perlin.Persistence = 1.0f;
            World.Perlin.Lacunarity = 2.0;
            double nx = (double)tx / (World.Config.ChunkWidth);
            double ny = (double)ty / (World.Config.ChunkHeight);

            double noise = World.Perlin.GetValue(x + nx, y + ny, World.Random.NextDouble());

            if (noise > 0.99)
            {
                if (tiles[tx, ty].HasFlag(TileId.Grass1))
                {
                    WorldObjectDef munshroom = new WorldObjectDef();
                    munshroom.Texture = _environment1atlas.ImagePath;
                    munshroom.Scale = Vector2.One;
                    munshroom.WorldPosition = World.GetWorldPosition(x, y) + new Vector2(tx * World.TileWidth, ty * World.TileHeight);
                    munshroom.DrawOrder = DrawOrders.Munshrooms;
                    munshroom.SourceRect = _environment1atlas["MunschroomSmall1"].Rect;
                    objects.Add(munshroom);
                }
            }
        }

    }
}
