
using Microsoft.Xna.Framework;
using MonoGameToolkit;

namespace Game1
{
    public class Chunk
    {
        public AnimatedSprite Sprite;
        public ChunkData Data;
        
        public Chunk(AnimatedSprite sprite, ChunkData data)
        {
            this.Sprite = sprite;
            this.Data = data;
        }

        public void Destroy()
        {
            if (Sprite != null)
            {
                if (Sprite.Texture != null)
                    Sprite.Texture.Dispose();
                Sprite.Destroy();
            }
            Data.Objects.Clear();
            Data.Tiles = null;
        }

        public bool Equals(Chunk b)
        {
            return (
                this.Data.ChunkPosition == b.Data.ChunkPosition &&
                this.Sprite == b.Sprite);
        }

        public override bool Equals(object obj)
        {
            return Equals((Chunk)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(Chunk a, Chunk b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if ((object)a == null || (object)b == null)
                return false;
            return a.Equals(b);
        }

        public static bool operator !=(Chunk a, Chunk b)
        {
            return !(a == b);
        }
    }
}
