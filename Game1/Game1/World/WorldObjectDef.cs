
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;
using MonoGameToolkit;
using System;

namespace Game1
{
    public enum WorldObjectFade
    {
        NoFading,
        FadingIn,
        FadingOut
    }

    public class WorldObjectDef
    {
        public readonly Guid Guid;
        public Vector2 WorldPosition;
        public Vector2 Scale;
        public float Rotation;
        public string Texture;
        public Rectangle SourceRect;
        public Point ChunkPosition;
        public Point TileOffset;
        public DrawOrders DrawOrder;
        public WorldObjectCollisionData WocData;
        public byte Tag;

        private WorldObjectFade _fade;
        public WorldObjectFade Fade
        {
            get { return _fade; }
            set { _fade = value; }
        }

        private Entities.WorldObject _sprite;
        public Entities.WorldObject Sprite
        {
            get { return _sprite; }
            set { _sprite = value; }
        }
        
        public WorldObjectDef()
        {
            this.Guid = Guid.NewGuid();
        }

        public WorldObjectDef(byte tag, string texture, Rectangle sourceRect, Vector2 worldPosition, Point chunkPosition, Point tileOffset, DrawOrders drawOrder, Vector2 scale, float rotation, WorldObjectCollisionData wocData)
        {
            this.Guid = Guid.NewGuid();
            this.WocData = wocData;
            this.Texture = texture;
            this.SourceRect = sourceRect;
            this.ChunkPosition = chunkPosition;
            this.TileOffset = tileOffset;
            this.DrawOrder = drawOrder;
            this.WorldPosition = worldPosition;
            this.Tag = tag;
            _sprite = null;
            Rotation = rotation;
            Scale = scale;
        }
        
        public WorldObjectDef(string texture, Rectangle sourceRect, Vector2 worldPosition, Point chunkPosition, Point tileOffset, DrawOrders drawOrder, WorldObjectCollisionData wocData)
            : this((byte)Tags.None, texture, sourceRect, worldPosition, chunkPosition, tileOffset, drawOrder, Vector2.One, 0.0f, wocData)
        { }


    }
}
