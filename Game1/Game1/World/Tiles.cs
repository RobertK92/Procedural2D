using Microsoft.Xna.Framework;
using MonoGameToolkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Game1
{
    /* Note: if you're using multiple tile layers, the order of the tile in this enum determines the draw order. */
    [Flags]
    public enum TileId : ulong 
    {
        Empty                       = 1 << 0,

        Grass1                      = 1 << 1,
        Grass2                      = 1 << 2,
        Grass1Darker                = 1 << 3,
        JungleGrass1                = 1 << 4,
        OrangeSand1                 = 1 << 5,
        Sand1                       = 1 << 6,
        Dirt1                       = 1 << 7,
        Lava1                       = 1 << 8,
        LavaRock1                   = 1 << 9,
        Snow1                       = 1 << 10,
        Ice1                        = 1 << 11,
        Flowers1                    = 1 << 12,
        Flowers2                    = 1 << 13,
        WeedSmall1                  = 1 << 14,
        River1                      = 1 << 15,
        Grass1Top                   = 1 << 16,
        Grass1Bottom                = 1 << 17,
        
        Grass1Right                 = 1 << 18,
        Grass1Left                  = 1 << 19,

        Grass1BottomRightEnclave    = 1 << 20,
        Grass1BottomLeftEnclave     = 1 << 21,
        Grass1TopLeftEnclave        = 1 << 22,
        Grass1TopRightEnclave       = 1 << 23,

        Grass1BottomRightCorner     = 1 << 24,
        Grass1BottomLeftCorner      = 1 << 25,
        Grass1TopRightCorner        = 1 << 26,
        Grass1TopLeftCorner         = 1 << 27,

    }

    public static class TilesLoader
    {
        public static Dictionary<TileId, Tile> LoadTiles()
        {
            TexturePackerAtlas tiles1 = TexturePacker.LoadAtlas(Strings.Content.Textures.Tiles1XML);
            Dictionary<TileId, Tile> tiles = new Dictionary<TileId, Tile>();

            tiles.Add(TileId.Empty, new Tile(Strings.Content.DefaultTexture32PNG, new Rectangle(0, 0, 32, 32)));

            BitArray bits = new BitArray(5);
            bits.Set(0, false);

            foreach(TileId value in Enum.GetValues(typeof(TileId)))
            {
                if (value == TileId.Empty)
                    continue;
                tiles.Add(value, new Tile(tiles1, value.ToString()));
            }

            return tiles;
        }
    }
}
