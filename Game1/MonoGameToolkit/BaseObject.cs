﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;

namespace MonoGameToolkit
{
    public class BaseObject
    {
        private const int LookupSize = 1024;
        private static bool precomputed;
        private static float[] sinLookup = new float[LookupSize];
        private static float[] cosLookup = new float[LookupSize];

        private bool _destroying;
        public bool Destroying { get { return _destroying; } }

        protected ContentManager Content { get { return MGTK.Instance.Content; } }
        protected GraphicsDevice GraphicsDevice { get { return MGTK.Instance.GraphicsDevice; } }
        protected Camera2D Camera { get { return LoadedScene.ActiveCamera; } }
        protected Scene LoadedScene { get { return MGTK.Instance.LoadedScene; } }
        protected Coroutines Coroutines { get { return MGTK.Instance.LoadedScene.Coroutines; } }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private Vector2 _position;
        public Vector2 Position
        {
            get { return _position; }
            set
            {
                if (_position != value)
                {
                    _position = value;
                    OnMoved.Invoke();
                    OnTransformed.Invoke();
                }
            }
        }

        private float _rotation;
        /// <summary>
        /// In degrees.
        /// </summary>
        public float Rotation
        {
            get { return _rotation; }
            set
            {
                if(_rotation != value)
                {
                    _rotation = value;
                    // Update right and up vectors.
                    float rot = MathHelper.ToRadians(value);
                    float upRot = rot - 1.57079633f;
                    int roti = (int)(-rot / (Math.PI * 2) * LookupSize) & (LookupSize - 1);
                    int upRoti = (int)(-upRot / (Math.PI * 2) * LookupSize) & (LookupSize - 1);
                    _up.X = cosLookup[upRoti];
                    _up.Y = -sinLookup[upRoti];
                    _right.X = cosLookup[roti];
                    _right.Y = -sinLookup[roti];
                    OnRotated.Invoke();
                    OnTransformed.Invoke();
                }
                
            }
        }

        private Vector2 _scale;
        public Vector2 Scale
        {
            get { return _scale; }
            set
            {
                if (_scale != value)
                {
                    _scale = value;
                    OnScaled.Invoke();
                    OnTransformed.Invoke();
                }
            }
        }

        private bool _enabled;
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled != value)
                {
                    if (PhysicsEnabled)
                    {
                        PhysicsBody.Enabled = value;
                    }
                }
                _enabled = value;
            }
        }

        private byte _tag;
        public byte Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        private Vector2 _right;
        public Vector2 Right { get { return _right; } }

        private Vector2 _up;
        public Vector2 Up { get { return _up; } }
        
        private Body _physicsBody;
        public Body PhysicsBody { get { return _physicsBody; } }

        public bool PhysicsEnabled { get { return (PhysicsBody != null); } }

        private Vector2 _physicsShapeOffset;
        public Vector2 PhysicsShapeOffset { get { return _physicsShapeOffset; } }

        public event Action OnMoved         = delegate { };
        public event Action OnRotated       = delegate { };
        public event Action OnScaled        = delegate { };
        public event Action OnTransformed   = delegate { };

        public BaseObject()
        {
            _name       = string.Empty;
            _enabled    = true;
            _scale      = Vector2.One;
            _position   = Vector2.Zero;
            _rotation   = 0.0f;
            _right      = Vector2.UnitX;
            _up         = -Vector2.UnitY;

            if (!precomputed)
            {
                for (int i = 0; i < LookupSize; i++)
                {
                    sinLookup[i] = (float)Math.Sin(i * Math.PI / LookupSize * 2.0f);
                    cosLookup[i] = (float)Math.Cos(i * Math.PI / LookupSize * 2.0f);
                }
                precomputed = true;
            }

            LoadedScene.Objects.Add(this); 
        }

        public void EnablePhysicsCircle(BodyType bodyType, float radius, Vector2 offset = default(Vector2), float density = 1.0f)
        {
            CircleShape circle = new CircleShape(radius * Physics.UPP, density);
            EnablePhysics(bodyType, circle, offset, density);
        }

        public void EnablePhysicsRectangle(BodyType bodyType, Rectangle rectangle, Vector2 offset = default(Vector2), float density =  1.0f)
        {
            Vector2 size = new Vector2(rectangle.Size.X * Physics.UPP, rectangle.Size.Y * Physics.UPP);
            Vertices verts = PolygonTools.CreateRectangle(size.X / 2, size.Y / 2);
            PolygonShape rectShape = new PolygonShape(verts, density);
            EnablePhysics(bodyType, rectShape, offset, density);
        }

        public void EnablePhysicsPolygonShape(BodyType bodyType, Vertices vertices, Vector2 offset = default(Vector2), float density = 1.0f)
        {
            PolygonShape polyShape = new PolygonShape(vertices, density);
            EnablePhysics(bodyType, polyShape, offset, density);
        }

        public void EnablePhysics(BodyType bodyType, Shape shape, Vector2 offset = default(Vector2), float density = 1.0f)
        {
            if (_physicsBody == null)
            {
                _physicsShapeOffset = offset;
                _physicsBody = new Body(LoadedScene.PhysicsWorld, (Position + offset) * Physics.UPP, MathHelper.ToRadians(Rotation), this);
                _physicsBody.BodyType = bodyType;
                _physicsBody.CreateFixture(shape, this);
                _physicsBody.Mass = 1.0f;
            }
        }

        public void DisablePhysics()
        {
            if(_physicsBody != null)
            {
                LoadedScene.PhysicsWorld.RemoveBody(_physicsBody);
            }
        }

        public override string ToString()
        {
            return !string.IsNullOrEmpty(Name) ? Name : GetType().Name;
        }

        internal void FixedUpdateInternal()
        {
            if (PhysicsEnabled)
            {
                float deg = MathHelper.ToDegrees(PhysicsBody.Rotation);
                Vector2 pos = (PhysicsBody.Position - (PhysicsShapeOffset * Physics.UPP)) / Physics.UPP;
                
                Position = pos;
                Rotation = deg;
            }  

            FixedUpdate();
        }

        internal void UpdateInternal(GameTime gameTime)
        {
            Update(gameTime);
        }

        internal void OnContactInternal(BaseObject obj)
        {
            OnContact(obj);
        }

        internal void OnSeperatedInternal(BaseObject obj)
        {
            OnSeperate(obj);
        }

        internal void OnPreSolveInternal(ContactInfo info)
        {
            OnPreSolve(info);
        }

        internal void OnPostSolveInternal(ContactInfo info)
        {
            OnPostSolve(info);
        }

        internal void OnDestroyInternal()
        {
            if (PhysicsEnabled)
                DisablePhysics();
            OnDestroy();
        }

        internal void DebugDrawInternal(DebugDrawer drawer) { DebugDraw(drawer); }
        
        protected virtual void DebugDraw(DebugDrawer drawer) { }
        protected virtual void Update(GameTime gameTime) { }
        protected virtual void FixedUpdate() { }
        protected virtual void OnDestroy() { }

        protected virtual void OnContact(BaseObject obj) { }
        protected virtual void OnSeperate(BaseObject obj) { }
        protected virtual void OnPreSolve(ContactInfo info) { }
        protected virtual void OnPostSolve(ContactInfo info) { }
    
        public void Destroy()
        {
            _destroying = true;
        }
    }
}
