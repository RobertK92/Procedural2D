using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using MonoGameToolkit;
using System.Collections.Generic;

namespace Game1
{
    public enum WorldObjectCollisionShape
    {
        Circle,
        Rectangle,
        Polygon
    }

    public class WorldObjectCollisionData
    {
        public readonly WorldObjectCollisionShape ShapeType;
        public readonly BodyType BodyType;
        public readonly Vector2 Size;
        public readonly Vector2 Offset;
        public readonly Shape Shape;
        public readonly bool IsSensor;
        public readonly PhysicsLayers Layer;
        public readonly PhysicsLayers CollidesWith;

        public WorldObjectCollisionData(BodyType bodyType, WorldObjectCollisionShape shapeType, Vector2 size, bool isSensor, Vector2 offset, float density, PhysicsLayers layer, PhysicsLayers collidesWith, Vertices vertices = null)
        {
            this.ShapeType = shapeType;
            this.BodyType = bodyType;
            this.Offset = offset;
            this.IsSensor = isSensor;
            this.Layer = layer;
            this.CollidesWith = collidesWith;
            this.Size = (size * Physics.UPP);
            Shape = null;

            switch (ShapeType)
            {
                case WorldObjectCollisionShape.Circle:
                    Shape = new CircleShape(Size.X, density);
                    break;
                case WorldObjectCollisionShape.Rectangle:
                    Vector2 halfSize = new Vector2(Size.X / 2, Size.Y / 2);
                    Vertices verts = PolygonTools.CreateRectangle(halfSize.X, halfSize.Y);
                    Shape = new PolygonShape(verts, density);
                    break;
                case WorldObjectCollisionShape.Polygon:
                    Shape = new PolygonShape(vertices, density);
                    break;
                default:
                    break;
            }
            
        }

        public WorldObjectCollisionData(BodyType bodyType, WorldObjectCollisionShape shapeType, Vector2 size, bool isSensor, Vector2 offset, float density, Vertices vertices = null)
            : this(bodyType, shapeType, size, isSensor, offset, density, PhysicsLayers.All, PhysicsLayers.All, vertices)
        {
            
        }

        public WorldObjectCollisionData(BodyType bodyType, WorldObjectCollisionShape shapeType, Vector2 size, bool isSensor, Vector2 offset, Vertices vertices = null)
            : this(bodyType, shapeType, size, isSensor, offset, 1.0f, PhysicsLayers.All, PhysicsLayers.All, vertices)
        {

        }

        public WorldObjectCollisionData(BodyType bodyType, WorldObjectCollisionShape shapeType, Vector2 size, bool isSensor, Vertices vertices = null)
            : this(bodyType, shapeType, size, isSensor, default(Vector2), 1.0f, PhysicsLayers.All, PhysicsLayers.All, vertices)
        {

        }

        public WorldObjectCollisionData(BodyType bodyType, WorldObjectCollisionShape shapeType, Vector2 size, Vertices vertices = null)
            : this(bodyType, shapeType, size, false, default(Vector2), 1.0f, PhysicsLayers.All, PhysicsLayers.All, vertices)
        {

        }
    }
}
