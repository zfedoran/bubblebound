/*
 * Copyright (C) 2009-2012 - Zelimir Fedoran
 *
 * This file is part of Bubble Bound.
 *
 * Bubble Bound is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * Bubble Bound is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Bubble Bound.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using hyades.graphics;
using hyades.screen;
using hyades.editor;
using hyades.utils;

namespace hyades
{
    public class Application : Microsoft.Xna.Framework.Game
    {
        public const int WIDTH = 1280;//1920;
        public const int HEIGHT = 720;//1200;
        public const bool FULLSCREEN = false;//true;
        public static readonly Color CLEAR_COLOR = new Color(0,0,0,0);
        public float brightness = 0.5f;

        private GraphicsDeviceManager graphics;
        private GraphicsDevice device;
        private InputDevice input;
        private ContentManager content;
        private ScreenManager screenmanager;
        private GameScreen gamescreen;
        private Hyades game;
        private EditorScreen editorscreen;
        private Editor editor;

        public Application()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            SetupGraphicsDeviceManager(graphics);

        }

        protected void SetupGraphicsDeviceManager(GraphicsDeviceManager graphics)
        {
            if (graphics != null)
            { 
                graphics.IsFullScreen = FULLSCREEN;
                graphics.PreferredBackBufferWidth = WIDTH;
                graphics.PreferredBackBufferHeight = HEIGHT;
            }
        }

        protected override void Initialize()
        {
            device = GraphicsDevice;
            content = Content;
            

            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            double elapsed = gameTime.ElapsedGameTime.TotalSeconds;

            TimerCollection.GetInstance().Update(elapsed);
            InterpolatorCollection.GetInstance().Update(elapsed);

            input.Update();
            screenmanager.Update(elapsed);
            
        }

        protected override void Draw(GameTime gameTime)
        {
            SpriteRenderer.Clear();

            PostProcessor postprocessor = PostProcessor.GetInstance(device);
            device.SetRenderTargets(postprocessor.result_rt);

            device.Clear(CLEAR_COLOR);
            device.SamplerStates[0] = SamplerState.LinearWrap;
            //device.SamplerStates[0] = SamplerState.PointClamp;
            device.DepthStencilState = DepthStencilState.Default;
            device.BlendState = BlendState.AlphaBlend;

            screenmanager.Draw(device);

            postprocessor.Bloom(postprocessor.result_rt, null, 0.25f, 1, 2 * brightness, 0.5f, 0.5f);
        }

        protected override void LoadContent()
        {
            Resources.Load(content);

            input = new MultiInputDevice(PlayerIndex.One);
            //input = new GamePadDevice(PlayerIndex.One);

            
            game = new Hyades(input);
            screenmanager = new ScreenManager(input);
            gamescreen = new GameScreen(game, screenmanager);
            screenmanager.FadeIn(gamescreen);
            Title logo = new Title(gamescreen, screenmanager);
            screenmanager.FadeIn(logo);
/*

            editor = new Editor(input);
            screenmanager = new ScreenManager(input);
            editorscreen = new EditorScreen(editor, screenmanager);
            screenmanager.FadeIn(editorscreen);
*/
        }

        protected override void UnloadContent()
        {
            Resources.Unload();
            content.Unload();
        }

        public void SetVolume(float volume)
        {
            if (game.background_music != null)
                game.background_music.Volume = volume;
        }

        public void SetBrightness(float brightness)
        {
            this.brightness = brightness;
        }

        private static Application instance;

        static Application()
        { instance = new Application(); }

        public static Application GetInstance()
        { return instance; }
    }


#if WINDOWS || XBOX
    static class Program
    {
        static void Main(string[] args)
        {
            using (Application game = Application.GetInstance())
            {
                game.Run();
            }
        }
    }
#endif
}
