using System;
using MonoGameToolkit;
using Game1.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel;
using FarseerPhysics.Dynamics;

namespace Game1.Scenes
{

    public class SurvivalScene : Scene
    {
        

        private Camera2D _camera;
        private Player _player;
        private World _world;
        private float _prevWheelValue;

        
        
        public override void Load()
        {
            _camera = new Camera2D();
            _camera.Zoom = 1.0f;
            ActiveCamera = _camera;
            
            int seed = 1;
            int tileSize = 32;
            int worldSizeInTiles = 2000;
            
            _player = new Player(Strings.Content.Textures.BaseHumanPNG);
            _player.Position = new Vector2((worldSizeInTiles * 0.5f) * tileSize, (worldSizeInTiles * 0.5f) * tileSize);

            _player.Scale = Vector2.One;
            _player.EnablePhysicsRectangle(BodyType.Dynamic, _player.Bounds);
            _player.PhysicsBody.FixedRotation = true;
            _player.PhysicsBody.LinearDamping = 30.0f;

            _world = new World(seed, worldSizeInTiles, worldSizeInTiles, tileSize, tileSize, _player);
        }


        public override void Update(GameTime gameTime)
        {
            if(Keyboard.GetState().IsKeyPressedOnce(Keys.Tab))
            {
                MGTK.Instance.DebugDrawEnabled = !MGTK.Instance.DebugDrawEnabled;
            }
            if(Keyboard.GetState().IsKeyPressedOnce(Keys.OemTilde))
            {
                MGTK.Instance.DebugPhysicsViewEnabled = !MGTK.Instance.DebugPhysicsViewEnabled;
            }
            
            if(Keyboard.GetState().IsKeyDown(Keys.LeftControl) && Keyboard.GetState().IsKeyPressedOnce(Keys.R))
            {
                _world.Reload();
            }

            if (_camera != null)
            {
                _camera.Position = _player.Position - new Vector2(_camera.Size.X / 2, _camera.Size.Y / 2);

                if (Keyboard.GetState().IsKeyDown(Keys.F))
                    _camera.Rotation += (float)gameTime.ElapsedGameTime.TotalSeconds * 10.0f;

                float wheelValue = Mouse.GetState().ScrollWheelValue;
                if (wheelValue != _prevWheelValue)
                {
                    if (wheelValue > _prevWheelValue)
                        _camera.Zoom *= 1.2f;
                    else
                        _camera.Zoom /= 1.2f;
                }
                _prevWheelValue = wheelValue;
            }
        }
        
    }
}
