using Microsoft.Xna.Framework.Graphics;
using MonoGameToolkit;
using System;

namespace Game1.Entities
{
    public class WorldObject : AnimatedSprite
    {
        public readonly Guid Guid;
        public readonly World World;

        public WorldObject(string texture, Guid guid, World world)
            : base(texture)
        {
            this.Guid = guid;
            this.World = world;
        }

        public WorldObject(Texture2D texture, Guid guid, World world)
            : base(texture)
        {
            this.Guid = guid;
            this.World = world;
        }

        /// <summary>
        /// Destroys the object and makes sure it won't spawn again.
        /// </summary>
        public void DestroyFromWorld()
        {
            World.DestroyWorldObject(this);
            Destroy();
        }
    }
}
