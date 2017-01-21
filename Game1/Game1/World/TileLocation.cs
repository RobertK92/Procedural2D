using Microsoft.Xna.Framework;

namespace Game1
{
    public struct TileLocation
    {
        public readonly Point ChunkPosition;
        public readonly Point TileOffset;

        public TileLocation(Point chunkPosition, Point tileOffset)
        {
            this.ChunkPosition = chunkPosition;
            this.TileOffset = tileOffset;
        }

        public bool Equals(TileLocation b)
        {
            return (
                this.ChunkPosition == b.ChunkPosition &&
                this.TileOffset == b.TileOffset);
        }

        public override bool Equals(object obj)
        {
            return Equals((TileLocation)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(TileLocation a, TileLocation b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if ((object)a == null || (object)b == null)
                return false;
            return a.Equals(b);
        }

        public static bool operator !=(TileLocation a, TileLocation b)
        {
            return !(a == b);
        }
    }
}
