using Microsoft.Xna.Framework;
using MonoGameToolkit;
using System.IO;

namespace Game1
{
    public struct Tile
    {
        public readonly string Texture;
        public readonly Rectangle SourceRect;

        public Tile(string texture, Rectangle sourceRect)
        {
            this.Texture = texture;
            this.SourceRect = sourceRect;
        }

        public Tile(TexturePackerAtlas atlas, string spriteName)
        {
            Texture = Path.ChangeExtension(atlas.ImagePath, null);
            SourceRect = atlas[spriteName].Rect;
        }
    }
}
