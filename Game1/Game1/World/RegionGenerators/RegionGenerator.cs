using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1
{
    public abstract class RegionGenerator
    {
        private World _world;
        protected World World { get { return _world; } }

        private Region _region;
        protected Region Region { get { return _region; } }

        private int _lastTx;
        public int LastTx
        {
            get { return _lastTx; }
            set { _lastTx = value; }
        }

        private int _lastTy;
        public int LastTy
        {
            get { return _lastTy; }
            set { _lastTy = value; }
        }

        public RegionGenerator(World world, Region region)
        {
            _world = world;
            _region = region;
        }

        public abstract void GenerateTileData(ref TileId[,] tiles, ref List<WorldObjectDef> objects, int x, int y, int tx, int ty);
        
        protected void GenerateTile(ref TileId[,] tiles, int x, int y, int tx, int ty, TileId id, TileCorners corners = null)
        {
            tiles[tx, ty] = id;
            if (corners == null)
                return;

            TileId current = tiles[tx, ty];
            TileArrayPosition tileAbove = WorldUtils.GetTileAbove(Region, tiles, x, y, tx, ty);
            TileArrayPosition tileLeft = WorldUtils.GetTileLeft(Region, tiles, x, y, tx, ty);

            if (!id.HasFlag(corners.Middle))
            {
                // top
                if (tileAbove.Id.HasFlag(corners.Middle))
                {
                    tiles[tx, ty] = id | corners.Top;
                }

                // right
                if (tileLeft.Id.HasFlag(corners.Middle))
                {
                    if (tileAbove.Id.HasFlag(corners.Middle))
                    {
                        tiles[tx, ty] = id | corners.TopLeftEnclave;
                    }
                    else
                    {
                        tiles[tx, ty] = id | corners.Left;
                    }
                }
            }
            else
            {
                // bottom
                if (ty > 0)
                {   
                    if (!tileAbove.Id.HasFlag(corners.Middle))
                    {
                        bool enclaved = false;
                        if (tileAbove.Id.HasFlag(corners.Left))
                        {
                            tiles[tx, ty - 1] |= corners.BottomLeftEnclave;
                            enclaved = true;
                        }
                        if(tileAbove.Id.HasFlag(corners.Right))
                        {
                            tiles[tx, ty - 1] |= corners.BottomRightEnclave;
                            enclaved = true;
                        }
                        
                        if(!enclaved)
                        {
                            tiles[tx, ty - 1] |= corners.Bottom;
                        }
                    }
                }
                else
                {
                    if (y > Region.offsetY)
                    {
                        Chunk above = Region.GetChunk(x, y - 1);
                        TileId topTileId = above.Data.Tiles[tx, (World.Config.ChunkHeight - 1)];
                        if (!topTileId.HasFlag(corners.Middle))
                        {
                            bool enclaved = false;

                            if(topTileId.HasFlag(corners.Left))
                            {
                                above.Data.Tiles[tx, (World.Config.ChunkHeight - 1)] |= corners.BottomLeftEnclave;
                                enclaved = true;
                            }
                            if(topTileId.HasFlag(corners.Right))
                            {
                                above.Data.Tiles[tx, (World.Config.ChunkHeight - 1)] |= corners.BottomRightEnclave;
                                enclaved = true;
                            }

                            if (!enclaved)
                            {
                                above.Data.Tiles[tx, (World.Config.ChunkHeight - 1)] |= corners.Bottom;
                            }
                        }
                    }
                }

                // left
                if (tx > 0)
                {
                    if (!tiles[tx - 1, ty].HasFlag(corners.Middle))
                    {
                        bool enclaved = false;
                        if (tileLeft.Id.HasFlag(corners.Top))
                        {
                            if (tileLeft.Id.HasFlag(corners.Left))
                            {
                                tiles[tx - 1, ty] |= corners.TopLeftEnclave;
                            }
                            else
                            {
                                tiles[tx - 1, ty] |= corners.TopRightEnclave;
                            }
                            enclaved = true;
                        }
                        
                        if (!enclaved)
                        {
                            tiles[tx - 1, ty] |= corners.Right;
                        }
                    }
                }
                else
                {
                    if (x > 0)
                    {
                        Chunk left = Region.GetChunk(x - 1, y);
                        TileId leftTileId = left.Data.Tiles[(World.Config.ChunkWidth - 1), ty];
                        if (!leftTileId.HasFlag(corners.Middle))
                        {
                            bool enclaved = false;
                            if (leftTileId.HasFlag(corners.Top))
                            {
                                if (leftTileId.HasFlag(corners.Left))
                                {
                                    left.Data.Tiles[(World.Config.ChunkWidth - 1), ty] |= corners.TopLeftEnclave;
                                }
                                else
                                {
                                    left.Data.Tiles[(World.Config.ChunkWidth - 1), ty] |= corners.TopRightEnclave;
                                }
                                enclaved = true;
                            }

                            if (!enclaved)
                            {
                                left.Data.Tiles[(World.Config.ChunkWidth - 1), ty] |= corners.Right;
                            }
                        }
                    }
                }
            }

            /* Corners */
            if ((tileLeft.Id.HasFlag(corners.Top) || tileLeft.Id.HasFlag(corners.TopLeftEnclave)) && 
                (tileAbove.Id.HasFlag(corners.Left) || tileAbove.Id.HasFlag(corners.TopLeftEnclave)))
            {
                tiles[tx, ty] = id | corners.BottomRightCorner;
            }

            if (ty > 0)
            {
                TileId tile = tiles[tx, ty - 1];
                bool untouched = (!tile.HasFlag(corners.Middle) && 
                                !tile.HasFlag(corners.Left) && 
                                !tile.HasFlag(corners.TopLeftEnclave) && 
                                !tile.HasFlag(corners.TopRightCorner) && 
                                !tile.HasFlag(corners.TopLeftCorner));

                if (untouched)
                {
                    if ((tiles[tx, ty].HasFlag(corners.Left) || tiles[tx, ty].HasFlag(corners.BottomLeftEnclave)))
                    {
                        tiles[tx, ty - 1] |= corners.TopRightCorner;
                    }
                }
            }
            else
            {
                if (y > Region.offsetY)
                {
                    Chunk above = Region.GetChunk(x, y - 1);
                    if ((tiles[tx, ty].HasFlag(corners.Left) || tiles[tx, ty].HasFlag(corners.BottomLeftEnclave)))
                    {
                        TileId tile = above.Data.Tiles[tx, (World.Config.ChunkHeight - 1)];
                        bool untouched = (!tile.HasFlag(corners.Middle) &&  
                                        !tile.HasFlag(corners.Left) &&
                                        !tile.HasFlag(corners.TopLeftEnclave) && 
                                        !tile.HasFlag(corners.TopRightCorner) && 
                                        !tile.HasFlag(corners.TopLeftCorner));

                        if (untouched)
                        {
                            above.Data.Tiles[tx, (World.Config.ChunkHeight - 1)] |= corners.TopRightCorner;
                        }
                    }
                }
            }

            if (tx > 0)
            {
                if (tiles[tx, ty].HasFlag(corners.Top) || tiles[tx, ty].HasFlag(corners.TopRightEnclave))
                {
                    TileId tile = tiles[tx - 1, ty];
                    bool untouched = (!tile.HasFlag(corners.Middle) && 
                                        !tile.HasFlag(corners.Top) &&
                                        !tile.HasFlag(corners.TopLeftEnclave) &&
                                        !tile.HasFlag(corners.Right) && 
                                        !tile.HasFlag(corners.BottomRightEnclave) && 
                                        !tile.HasFlag(corners.BottomLeftCorner));

                    if (untouched)
                    {
                        tiles[tx - 1, ty] |= corners.BottomLeftCorner;
                    }
                }
            }
            else
            {
                if (x > 0)
                {
                    Chunk left = Region.GetChunk(x - 1, y);
                    if (tiles[tx, ty].HasFlag(corners.Top) || tiles[tx, ty].HasFlag(corners.TopRightEnclave))
                    {
                        TileId tile = left.Data.Tiles[(World.Config.ChunkWidth - 1), ty];
                        bool untouched = (!tile.HasFlag(corners.Middle) &&
                                        !tile.HasFlag(corners.Top) &&
                                        !tile.HasFlag(corners.TopLeftEnclave) &&
                                        !tile.HasFlag(corners.Right) &&
                                        !tile.HasFlag(corners.BottomRightEnclave) &&
                                        !tile.HasFlag(corners.BottomLeftCorner));
                        
                        if (untouched)
                        {
                            left.Data.Tiles[(World.Config.ChunkWidth - 1), ty] |= corners.BottomLeftCorner;
                        }
                    }
                }
            }
            
            if (tx > 0 && ty > 0)
            {
                TileId tile = tiles[tx - 1, ty - 1];
                bool untouched = (!tile.HasFlag(corners.Middle) &&  
                                    !tile.HasFlag(corners.TopRightEnclave) &&
                                    !tile.HasFlag(corners.TopLeftCorner));

                if (untouched)
                {
                    if (tiles[tx - 1, ty].HasFlag(corners.Right) || tiles[tx - 1, ty].HasFlag(corners.BottomRightEnclave))
                    {
                        if (tiles[tx, ty - 1].HasFlag(corners.Bottom) || tiles[tx, ty - 1].HasFlag(corners.BottomRightEnclave))
                        {
                            tiles[tx - 1, ty - 1] |= corners.TopLeftCorner;
                        }
                    }
                }
            }
            else 
            {
                if (tx == 0 && ty == 0)
                {
                    if (x > 0 && y > Region.offsetY)
                    {
                        Chunk aboveLeft = Region.GetChunk(x - 1, y - 1);
                        Chunk left = Region.GetChunk(x - 1, y);
                        Chunk above = Region.GetChunk(x, y - 1);

                        TileId tile = aboveLeft.Data.Tiles[(World.Config.ChunkWidth - 1), (World.Config.ChunkHeight - 1)];
                        bool untouched = (!tile.HasFlag(corners.Middle) && 
                                        !tile.HasFlag(corners.Top) && 
                                        !tile.HasFlag(corners.TopRightEnclave) &&
                                        !tile.HasFlag(corners.TopLeftCorner));

                        if (untouched)
                        {
                            TileId leftId = left.Data.Tiles[(World.Config.ChunkWidth - 1), ty];
                            if (leftId.HasFlag(corners.Right) || leftId.HasFlag(corners.BottomRightEnclave))
                            {
                                TileId topId = above.Data.Tiles[tx, (World.Config.ChunkHeight - 1)];
                                if (topId.HasFlag(corners.Bottom) || topId.HasFlag(corners.BottomRightEnclave))
                                {
                                    aboveLeft.Data.Tiles[(World.Config.ChunkWidth - 1), (World.Config.ChunkHeight - 1)] |= corners.TopLeftCorner;
                                }
                            }
                        }
                    }
                }
                else if(tx > 0 && ty == 0)
                {
                    if (y > Region.offsetY)
                    {
                        Chunk above = Region.GetChunk(x, y - 1);
                        TileId tile = above.Data.Tiles[tx, (World.Config.ChunkHeight - 1)];
                        bool untouched = (!tile.HasFlag(corners.Middle) && 
                                        !tile.HasFlag(corners.Top) &&
                                        !tile.HasFlag(corners.TopRightEnclave) &&
                                        !tile.HasFlag(corners.TopLeftCorner));

                        if (untouched)
                        {
                            if (tiles[tx - 1, ty].HasFlag(corners.Right) || tiles[tx - 1, ty].HasFlag(corners.BottomRightEnclave))
                            {
                                TileId topId = above.Data.Tiles[tx, (World.Config.ChunkHeight - 1)];
                                if (topId.HasFlag(corners.Bottom) || topId.HasFlag(corners.BottomRightEnclave))
                                {
                                    above.Data.Tiles[tx - 1, (World.Config.ChunkHeight - 1)] |= corners.TopLeftCorner;
                                }
                            }
                        }
                    }
                }
                else if(tx == 0 && ty > 0)
                {
                    if(x > 0)
                    {
                        Chunk left = Region.GetChunk(x - 1, y);

                        TileId tile = left.Data.Tiles[(World.Config.ChunkWidth - 1), ty];
                        bool untouched = (!tile.HasFlag(corners.Middle) && 
                                        !tile.HasFlag(corners.Top) &&
                                        !tile.HasFlag(corners.TopRightEnclave) &&
                                        !tile.HasFlag(corners.TopLeftCorner));
                        
                        if (untouched)
                        {
                            TileId leftId = left.Data.Tiles[(World.Config.ChunkWidth - 1), ty];
                            if (leftId.HasFlag(corners.Right) || leftId.HasFlag(corners.BottomRightEnclave))
                            {
                                if(tiles[tx, ty - 1].HasFlag(corners.Bottom) || tiles[tx, ty - 1].HasFlag(corners.BottomRightEnclave))
                                {
                                    left.Data.Tiles[(World.Config.ChunkWidth - 1), ty - 1] |= corners.TopLeftCorner;
                                }
                            }
                        }
                    }
                }
            }

        }

        private bool IsInChunk(TileLocation current, Point chunkPosition)
        {
            return (chunkPosition == current.ChunkPosition);
        }

        private bool IsInChunkAbove(Point currentChunk, Point subjectChunk)
        {
            return (currentChunk.X == subjectChunk.X && currentChunk.Y == (subjectChunk.Y - 1));
        }

        private bool IsInChunkBelow(Point currentChunk, Point subjectChunk)
        {
            return (currentChunk.X == subjectChunk.X && currentChunk.Y == (subjectChunk.Y + 1));
        }

        protected bool Below(TileLocation current, TileLocation subject)
        {
            bool inChunk = IsInChunk(current, subject.ChunkPosition);
            if (inChunk)
            {
                return (current.TileOffset.X == subject.TileOffset.X && current.TileOffset.Y == (subject.TileOffset.Y + 1));
            }

            return (IsInChunkBelow(current.ChunkPosition, subject.ChunkPosition) &&
                (current.TileOffset.X == subject.TileOffset.X && current.TileOffset.Y == 0));
        }

        protected bool Above(TileLocation current, TileLocation subject)
        {
            bool inChunk = IsInChunk(current, subject.ChunkPosition);
            if (inChunk)
            {
                return (current.TileOffset.X == subject.TileOffset.X && current.TileOffset.Y == (subject.TileOffset.Y - 1));
            }

            return (IsInChunkAbove(current.ChunkPosition, subject.ChunkPosition) &&
                (current.TileOffset.X == subject.TileOffset.X && current.TileOffset.Y == (World.Config.ChunkHeight - 1)));
        }

        protected bool AlignedY(TileLocation current, TileLocation subject)
        {
            return current.ChunkPosition.X == subject.ChunkPosition.X && current.TileOffset.X == subject.TileOffset.X;
        }

        protected bool AlignedX(TileLocation current, TileLocation subject)
        {
            return current.ChunkPosition.Y == subject.ChunkPosition.Y && current.TileOffset.Y == subject.TileOffset.Y;
        }
    }
}
