using Microsoft.Xna.Framework;
using MonoGameToolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1
{
    public class TreeDef
    {
        protected WorldObjectDef _top;
        public  WorldObjectDef Top
        {
            get { return _top; }
            protected set { _top = value; }
        }

        protected WorldObjectDef _base;
        public WorldObjectDef Base
        {
            get { return _base; }
            protected set { _base = value; }
        }

        private Vector2 _worldPosition;
        public Vector2 WorldPosition
        {
            get { return _worldPosition; }
        }

        public TreeDef(World world, Point chunkPosition, Point tileOffset, string baseSprite, string topSprite)
        {
            _worldPosition = world.GetWorldPosition(chunkPosition.X, chunkPosition.Y) + new Vector2(tileOffset.X * world.TileWidth, tileOffset.Y * world.TileHeight);
            
            TexturePackerAtlas environment1Atlas = TexturePacker.LoadAtlas(Strings.Content.Textures.Environment1XML);
            TexturePackerSprite normalTreeBase = environment1Atlas[baseSprite];
            TexturePackerSprite normalTreeTop = environment1Atlas[topSprite];

            Vector2 treeScale = Vector2.One * 2;
            
            float faderSizeDecreaseX = world.Player.Bounds.Width;
            float faderSizeDecreaseY = world.Player.Bounds.Height;
            
            WorldObjectCollisionData wocDataTop = new WorldObjectCollisionData(
                    FarseerPhysics.Dynamics.BodyType.Static,
                    WorldObjectCollisionShape.Rectangle,
                    new Vector2((normalTreeTop.Rect.Width - faderSizeDecreaseX) * treeScale.X, (normalTreeTop.Rect.Height - faderSizeDecreaseY) * treeScale.Y),
                    true, Vector2.Zero, 1.0f, PhysicsLayers.WorldObjectSensors, PhysicsLayers.Player);

            Top = new WorldObjectDef((byte)Tags.OpaqueSensor, environment1Atlas.ImagePath,
                                    normalTreeTop.Rect,
                                    WorldPosition,
                                    chunkPosition,
                                    tileOffset,
                                    DrawOrders.TreeTops,
                                    treeScale,
                                    normalTreeTop.Rotated ? -90.0f : 0.0f,
                                    wocDataTop);

            float colliderScaleX = 0.55f;
            float colliderScaleY = 0.45f;

            WorldObjectCollisionData wocData = new WorldObjectCollisionData(
                    FarseerPhysics.Dynamics.BodyType.Static,
                    WorldObjectCollisionShape.Rectangle,
                    new Vector2(normalTreeBase.Rect.Width * treeScale.X * colliderScaleX, normalTreeBase.Rect.Height * treeScale.Y * colliderScaleY),
                    false,
                    new Vector2(0.0f, -25.0f));

            Base = new WorldObjectDef((byte)Tags.None, environment1Atlas.ImagePath,
                                    normalTreeBase.Rect,
                                    WorldPosition + (new Vector2(0.0f, 50.0f) * treeScale.Y),
                                    chunkPosition,
                                    tileOffset,
                                    DrawOrders.TreeBases,
                                    treeScale,
                                    normalTreeBase.Rotated ? -90.0f : 0.0f,
                                    wocData);
        }
    }
}
