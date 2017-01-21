using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Linq;
using Ionic.Zlib;
using System;
using Microsoft.Xna.Framework;

namespace MonoGameToolkit
{
    public static class Tiled
    {
        public static TiledMap CreateMap(string tmxFile)
        {
            TiledMap map = null;
            XmlSerializer reader = new XmlSerializer(typeof(TiledMap));
            using (FileStream stream = File.OpenRead(string.Format("Content/{0}.tmx", tmxFile)))
            {
                map = (TiledMap)reader.Deserialize(stream);
            }
            return map;
        }

        public static void GenerateTileLayers(TiledMap map)
        {
            for (int i = map.Layers.Count - 1; i >= 0; i--)
            {
                TiledTileLayer layer = map.Layers[i];
                if (layer.Data == null)
                    continue;
                
                long[,] tiles = GetTileDataFromBase64Zlib(layer.Data, map);
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    for (int x = 0; x < tiles.GetLength(0); x++)
                    {
                        long tile = tiles[x, y];
                        if (tile == 0)
                            continue;

                        TiledTileSet set = GetTileSet(tile, map);
                        int realIndex = (int)(tile - set.FirstGID);
                        int srcY = (realIndex / set.Columns);
                        int srcX = (realIndex - (srcY * set.Columns));

                        string source = Path.ChangeExtension(set.Image.Source, null).Replace("../", "");
                        Sprite sprite = new Sprite(source);
                        sprite.SourceRect = new Rectangle(srcX * set.TileWidth, srcY * set.TileHeight, set.TileWidth, set.TileHeight);
                        sprite.Position = new Vector2(x * set.TileWidth, y * set.TileHeight);
                        sprite.Visible = layer.Visible;
                        sprite.Opacity = layer.Opacity;
                    }
                }
            }
        }

        private static TiledTileSet GetTileSet(long tile, TiledMap map)
        {
            foreach (TiledTileSet ts in map.TileSets)
            {
                if (tile >= ts.FirstGID && tile < (ts.FirstGID + ts.TileCount))
                    return ts;
            }
            throw new Exception(string.Format("[Tiled] No TileSet found for tile index '{0}'", tile.ToString()));
        }


        private static long[,] GetTileDataFromBase64Zlib(TiledLayerData data, TiledMap map)
        {
            if (data.Encoding == "base64" && data.Compression == "zlib")
            {
                byte[] zlibBuffer = Convert.FromBase64String(data.Data);
                MemoryStream comp = new MemoryStream(zlibBuffer);
                Stream zlibStream = new ZlibStream(comp, CompressionMode.Decompress);

                long[,] tiles = new long[map.Width, map.Height];

                using (var bn = new BinaryReader(zlibStream))
                {
                    for (int j = 0; j < map.Height; j++)
                    {
                        for (int i = 0; i < map.Width; i++)
                        {
                            tiles[i, j] = bn.ReadInt32();
                        }
                    }
                }
                return tiles;
            }
            return null;
        }
    }
        
    [XmlRoot("map")]
    public class TiledMap
    {
        [XmlAttribute("width")]
        public int Width { get; set; }
        [XmlAttribute("height")]
        public int Height { get; set; }
        [XmlAttribute("nextobjectid")]
        public long NextObjectID { get; set; }
        [XmlAttribute("orientation")]
        public string Orientation { get; set; }
        [XmlAttribute("renderorder")]
        public string RenderOrder { get; set; }
        [XmlAttribute("tileheight")]
        public int TileHeight { get; set; }
        [XmlAttribute("tilewidth")]
        public int TileWidth { get; set; }
        [XmlAttribute("version")]
        public string Version { get; set; }

        [XmlElement("tileset")]
        public List<TiledTileSet> TileSets { get; set; }
        [XmlElement("layer")]
        public List<TiledTileLayer> Layers { get; set; }
        [XmlElement("objectgroup")]
        public List<TiledObjectGroup> ObjectGroups { get; set; }

        public override string ToString()
        {
            return string.Format(
                "TiledMap \n  version = {0} \n  orientation = {1}\n  renderorder = {2}\n  width = {3}\n  " +
                "height = {4}\n  tilewidth = {5}\n  tileheight = {6}\n  nextobjectid = {7}\n  tilesetcount = {8}\n  layercount = {9}",
                Version, Orientation, RenderOrder, Width, Height, TileWidth, TileHeight, NextObjectID, TileSets.Count, Layers.Count);
        }
    }

    public class TiledObjectGroup
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("color")]
        public string Color { get; set; }
        [XmlAttribute("x")]
        public int X { get; set; }
        [XmlAttribute("y")]
        public int Y { get; set; }
        [XmlAttribute("width")]
        public int Width { get; set; }
        [XmlAttribute("height")]
        public int Height { get; set; }
        [XmlAttribute("opacity")]
        public float Opacity { get; set; } = 1.0f;
        [XmlAttribute("visible")]
        public bool Visible { get; set; } = true;
        [XmlAttribute("offsetx")]
        public float OffsetX { get; set; }
        [XmlAttribute("offsety")]
        public float OffsetY { get; set; }
        [XmlAttribute("draworder")]
        public string DrawOrderMode { get; set; } = "topdown";

        [XmlElement("object")]
        public List<TiledObject> Objects { get; set; }
    }

    public class TiledObject
    {
        [XmlAttribute("id")]
        public int ID { get; set; }
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("type")]
        public string Type { get; set; }
        [XmlAttribute("x")]
        public float X { get; set; }
        [XmlAttribute("y")]
        public float Y { get; set; }
        [XmlAttribute("width")]
        public float Width { get; set; }
        [XmlAttribute("height")]
        public float Height { get; set; }
        [XmlAttribute("rotation")]
        public float Rotation { get; set; }
        [XmlAttribute("gid")]
        public long GID { get; set; }
        [XmlAttribute("visible")]
        public bool Visible { get; set; } = true;

        [XmlElement("ellipse")]
        public TiledEllipse Ellipse { get; set; }
        [XmlElement("polygon")]
        public TiledPolygon Polygon { get; set; }
        [XmlElement("polyline")]
        public TiledPolyline Polyline { get; set; }
    }

    public class TiledLayerData
    {
        [XmlAttribute("encoding")]
        public string Encoding { get; set; }
        [XmlAttribute("compression")]
        public string Compression { get; set; }

        [XmlText]
        public string Data { get; set; } /* the CSV or base64 string */
    }

    public class TiledImageLayer
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("offsetx")]
        public float OffsetX { get; set; }
        [XmlAttribute("offsety")]
        public float OffsetY { get; set; }
        [XmlAttribute("opacity")]
        public float Opacity { get; set; } = 1.0f;
        [XmlAttribute("visible")]
        public bool Visible { get; set; } = true;

        [XmlElement("image")]
        public TiledImage Image { get; set; }
    }

    public class TiledImage
    {
        [XmlAttribute("source")]
        public string Source { get; set; }
        [XmlAttribute("width")]
        public int Width { get; set; }
        [XmlAttribute("height")]
        public int Height { get; set; }

    }

    public class TiledTileSet
    {
        [XmlAttribute("firstgid")]
        public long FirstGID { get; set; }
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("margin")]
        public int Margin { get; set; }
        [XmlAttribute("spacing")]
        public int Spacing { get; set; }
        [XmlAttribute("tilewidth")]
        public int TileWidth { get; set; }
        [XmlAttribute("tileheight")]
        public int TileHeight { get; set; }
        [XmlAttribute("tilecount")]
        public int TileCount { get; set; }
        [XmlAttribute("columns")]
        public int Columns { get; set; }

        [XmlElement("tile")]
        public List<TiledTile> Tiles { get; set; }

        [XmlElement("image")]
        public TiledImage Image { get; set; }
    }

    public class TiledTileLayer
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("width")]
        public int Width { get; set; }
        [XmlAttribute("height")]
        public int Height { get; set; }
        [XmlAttribute("opacity")]
        public float Opacity { get; set; } = 1.0f;
        [XmlAttribute("visible")]
        public bool Visible { get; set; } = true;
        [XmlAttribute("offsetx")]
        public float OffsetX { get; set; }
        [XmlAttribute("offsety")]
        public float OffsetY { get; set; }

        [XmlElement("data")]
        public TiledLayerData Data { get; set; }

    }

    public class TiledTile
    {
        [XmlAttribute("id")]
        public int ID { get; set; }
        [XmlAttribute("image")]
        public string Image { get; set; }
        [XmlAttribute("terrain")]
        public int[] Terrain { get; set; }
    }
        
    public class TiledPolygon
    {
        [XmlAttribute("points")]
        public string Points { get; set; }
    }

    public class TiledPolyline
    {
        [XmlAttribute("points")]
        public string Points { get; set; }
    }

    public class TiledEllipse
    {
    }
}

