
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameToolkit;

namespace Game1
{
    public class Game1 : MGTK
	{
        public override string DefaultFont
        {
            get
            {
                return "DefaultFont";
            }
        }

        public override string DefaultTexture
        {
            get
            {
                return "DefaulTexture32";
            }
        }

        public Game1()
            : base()
        {
            IsFixedTimeStep = true;
            Graphics.SynchronizeWithVerticalRetrace = true;
            
            Physics.Gravity = Vector2.Zero;
            DebugDrawEnabled = false;
            LoggerEnabled = true;
        }
        
        protected override void LoadContent()
        {
            base.LoadContent();
            LoadScene<Scenes.SurvivalScene>();
        }
    }
}
