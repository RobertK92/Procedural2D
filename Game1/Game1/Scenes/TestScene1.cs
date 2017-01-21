using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Game1.Entities;

using MonoGameToolkit;
using Microsoft.Xna.Framework.Input;

namespace Game1.Scenes
{
    public class TestScene1 : Scene
    {
        private Camera2D _camera;
        private Player p2;
        private Player p1;
        
        public override void Load()
        {
            
            p1 = new Player(Strings.Content.Textures.BaseHumanPNG);
            p1.Color = new Color(255, 255, 255, 255);
            p1.DrawOrder = 1;

            //TiledMap map = Tiled.CreateMap(Strings.Content.Levels.TestLevel);
            //Tiled.GenerateTileLayers(map);

            int t = 0;
            /*p2 = new Player();

            p2.Position = new Vector2(600, 300);
            p2.Color = Color.Red;
            */
            //p2.Scale = new Vector2(3000, 2500);

            _camera = new Camera2D();
            _camera.Position = new Vector2(0, 0);

        }

        private float _prevWheelValue;
        public override void Update(GameTime gameTime)
        {
            _camera.Position = p1.Position;
            // Console.WriteLine(Globals.DrawCalls);
            float speed = 100.0f;
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            //p1.Rotation += dt * 100.0f;
            

            if(Keyboard.GetState().IsKeyDown(Keys.F))
                _camera.Rotation += dt;

            float wheelValue = Mouse.GetState().ScrollWheelValue;
            if (wheelValue != _prevWheelValue)
            {
                if (wheelValue > _prevWheelValue)
                    _camera.Zoom += 0.1f;
                else
                    _camera.Zoom -= 0.1f;
            }
            _prevWheelValue = wheelValue;
        }

        public override void DebugDraw(DebugDrawer drawer)
        {
            //drawer.DrawLine(Vector2.Zero, new Vector2(500, 500), Color.White);
        }

    }
}
