using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameToolkit;
using FarseerPhysics.Dynamics;

namespace Game1.Entities
{
    [Flags]
    public enum WalkMode
    {
        None = 0,
        Up = 1,
        Down = 2,
        Left = 4,
        Right = 8,
        StrafeUpLeft = 16,
        StrafeUpRight = 32,
        StrafeDownLeft = 64,
        StrafeDownRight = 128
    }

    public class Player : AnimatedSprite
    {
        [LogStaticValue]
        public static Vector2 PlayerPosition;
        
        private const WalkMode StrafeMask = WalkMode.StrafeDownLeft | WalkMode.StrafeDownRight | WalkMode.StrafeUpLeft | WalkMode.StrafeUpRight;
        private const float StrafeRotation = 0.0f;
        
        private float _moveSpeed;
        public float MoveSpeed
        {
            get { return _moveSpeed; }
            set { _moveSpeed = value; }
        }

        private WalkMode _walkMode;
        public WalkMode WalkMode { get { return _walkMode; } }

        public bool IsStrafing { get { return ((_walkMode & StrafeMask) != 0); } }

        private float cos4OverPi;
        private float sin4OverPi;


        public Player(string texture)
            : base(texture)
        {
            Name = "Player";
            Tag = (byte)Tags.Player;
            cos4OverPi = (float)Math.Cos(Math.PI / 4);
            sin4OverPi = (float)Math.Sin(Math.PI / 4);
           
            _walkMode = WalkMode.None;
            AnimationSpeed = 0.05f;
            DrawOrder = (int)DrawOrders.Player;

            TexturePackerAtlas atlas = TexturePacker.LoadAtlas(Strings.Content.Textures.BaseHumanXML);
            
            Animations.Add("walk down", new KeyFrame[]
            {
                new KeyFrame(atlas["BaseHuman_WalkDown_0"]),
                new KeyFrame(atlas["BaseHuman_WalkDown_1"]),
                new KeyFrame(atlas["BaseHuman_WalkDown_2"]),
                new KeyFrame(atlas["BaseHuman_WalkDown_3"]),
                new KeyFrame(atlas["BaseHuman_WalkDown_4"]),
                new KeyFrame(atlas["BaseHuman_WalkDown_5"]),
                new KeyFrame(atlas["BaseHuman_WalkDown_6"]),
                new KeyFrame(atlas["BaseHuman_WalkDown_7"]),
            });

            Animations.Add("walk up", new KeyFrame[]
            {
                new KeyFrame(atlas["BaseHuman_WalkUp_0"]),
                new KeyFrame(atlas["BaseHuman_WalkUp_1"]),
                new KeyFrame(atlas["BaseHuman_WalkUp_2"]),
                new KeyFrame(atlas["BaseHuman_WalkUp_3"]),
                new KeyFrame(atlas["BaseHuman_WalkUp_4"]),
                new KeyFrame(atlas["BaseHuman_WalkUp_5"]),
                new KeyFrame(atlas["BaseHuman_WalkUp_6"]),
                new KeyFrame(atlas["BaseHuman_WalkUp_7"]),
            });

            Animations.Add("walk side", new KeyFrame[]
            {
                new KeyFrame(atlas["BaseHuman_WalkSide_0"]),
                new KeyFrame(atlas["BaseHuman_WalkSide_1"]),
                new KeyFrame(atlas["BaseHuman_WalkSide_2"]),
                new KeyFrame(atlas["BaseHuman_WalkSide_3"]),
                new KeyFrame(atlas["BaseHuman_WalkSide_4"]),
                new KeyFrame(atlas["BaseHuman_WalkSide_5"]),
                new KeyFrame(atlas["BaseHuman_WalkSide_6"]),
                new KeyFrame(atlas["BaseHuman_WalkSide_7"]),
            });

            Animations.Add("idle up", new KeyFrame[]
            {
                new KeyFrame(atlas["BaseHuman_IdleUp_0"])
            });

            Animations.Add("idle down", new KeyFrame[]
            {
                new KeyFrame(atlas["BaseHuman_IdleDown_0"])
            });

            Animations.Add("idle side", new KeyFrame[]
            {
                new KeyFrame(atlas["BaseHuman_IdleSide_0"])
            });

            
            PlayAnimation("idle down");

            
        }
        
        protected override void Update(GameTime gameTime)
        {
            if (World.IsLoading)
                return;

            float dt = Time.DeltaTime;
            
            PlayerPosition = Position;
            KeyboardState keyboard = Keyboard.GetState();

            if (keyboard.IsKeyDown(Keys.LeftShift))
                MoveSpeed = 10000.0f;
            else
                MoveSpeed = 3000.0f;

            _walkMode = WalkMode.None;
            if (keyboard.IsKeyDown(Keys.W))
            {
                _walkMode = WalkMode.Up;
                if (keyboard.IsKeyDown(Keys.A))
                    _walkMode = WalkMode.StrafeUpLeft;
                if (keyboard.IsKeyDown(Keys.D))
                    _walkMode = WalkMode.StrafeUpRight;
            }

            else if (keyboard.IsKeyDown(Keys.S))
            {
                _walkMode = WalkMode.Down;
                if (keyboard.IsKeyDown(Keys.A))
                    _walkMode = WalkMode.StrafeDownLeft;
                if (keyboard.IsKeyDown(Keys.D))
                    _walkMode = WalkMode.StrafeDownRight;
            }

            if (!IsStrafing)
            {
                if (keyboard.IsKeyDown(Keys.A))
                    _walkMode = WalkMode.Left;
                else if (keyboard.IsKeyDown(Keys.D))
                    _walkMode = WalkMode.Right;
            }
            
            Vector2 strafeDir = Vector2.Zero;
            Vector2 velocity = Vector2.Zero;
            switch (WalkMode)
            {
                case WalkMode.None:
                    Rotation = 0.0f;
                    if (CurrentAnimation == "walk down")
                        PlayAnimation("idle down");
                    else if (CurrentAnimation == "walk up")
                        PlayAnimation("idle up");
                    else if (CurrentAnimation == "walk side")
                        PlayAnimation("idle side");
                    break;
                case WalkMode.Up:
                    PlayAnimation("walk up");
                    Rotation = 0.0f;
                    velocity = -Vector2.UnitY * MoveSpeed * dt;
                    break;
                case WalkMode.Down:
                    PlayAnimation("walk down");
                    Rotation = 0.0f;
                    velocity = Vector2.UnitY * MoveSpeed * dt;
                    break;
                case WalkMode.Left:
                    PlayAnimation("walk side");
                    Rotation = 0.0f;
                    velocity = -Vector2.UnitX * MoveSpeed * dt;
                    SpriteEffects = SpriteEffects.None;
                    break;
                case WalkMode.Right:
                    PlayAnimation("walk side");
                    Rotation = 0.0f;
                    velocity = Vector2.UnitX * MoveSpeed * dt;
                    SpriteEffects = SpriteEffects.FlipHorizontally;
                    break;
                case WalkMode.StrafeUpLeft:
                    PlayAnimation("walk up");
                    Rotation = -StrafeRotation;
                    strafeDir = -new Vector2(cos4OverPi, sin4OverPi);
                    strafeDir.Normalize();
                    velocity = strafeDir * MoveSpeed * dt;
                    break;
                case WalkMode.StrafeUpRight:
                    PlayAnimation("walk up");
                    Rotation = StrafeRotation;
                    strafeDir = -new Vector2(-cos4OverPi, sin4OverPi);
                    strafeDir.Normalize();
                    velocity = strafeDir * MoveSpeed * dt;
                    break;
                case WalkMode.StrafeDownLeft:
                    PlayAnimation("walk down");
                    Rotation = StrafeRotation;
                    strafeDir = -new Vector2(-cos4OverPi, sin4OverPi);
                    strafeDir.Normalize();
                    velocity = -strafeDir * MoveSpeed * dt;
                    break;
                case WalkMode.StrafeDownRight:
                    PlayAnimation("walk down");
                    Rotation = -StrafeRotation;
                    strafeDir = -new Vector2(cos4OverPi, sin4OverPi);
                    strafeDir.Normalize();
                    velocity = -strafeDir * MoveSpeed * dt;
                    break;
                default:
                    break;
            }

            if(PhysicsEnabled)
                PhysicsBody.ApplyForce(velocity);
            

            base.Update(gameTime);
        }

        protected override void DebugDraw(DebugDrawer drawer)
        {
            drawer.DrawTransformation(this);
            
        }

    }
}
