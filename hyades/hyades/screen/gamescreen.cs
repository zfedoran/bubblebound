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
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using hyades.entity;
using hyades.level;
using hyades.screen.menu;
using hyades.graphics;
using hyades.graphics.particle;
using hyades.utils;

namespace hyades.screen
{
    public class GameScreen : Screen
    {
        public Hyades game;
        //public Title title_screen;
        public Menu menu_screen;

        public float time;
        public GameScreen(Hyades game, ScreenManager manager)
            : base(null, manager)
        {
            this.game = game;

            //title_screen = new Title(this, manager);
            menu_screen = new MainMenu(this, manager);


        }

        public override void OnActive(double elapsed, InputDevice input)
        {
            game.Update(elapsed);

            /*
            if (input.IsPressed(Buttons.LeftShoulder))
            {
                game.character.IncreaseSize();
                ParticleManager particlemanager = ParticleManager.GetInstance();
                particlemanager.Explode(game.character.position, 3);
            }

            if (input.IsPressed(Buttons.RightShoulder))
            {
                game.character.DecreaseSize();
                ParticleManager particlemanager = ParticleManager.GetInstance();
                particlemanager.Explode(game.character.position, 3);
            }
            */

            if (input.IsPressed(Buttons.Start) || input.IsPressed(Buttons.Back))
                screenmanager.FadeIn(menu_screen);



            time += (float)elapsed;
        }

        public override void Draw(GraphicsDevice device)
        {
            game.Draw(device);


            int curr_size = game.character.curr_size;
            int num_bubbles = game.character.num_bubbles[curr_size];
            int num_collected = game.character.num_collected;

            int size = 20;
            int space = 10;

            int display_width = num_bubbles * size + num_bubbles * space + (num_bubbles / 3)*space;
            float center = device.PresentationParameters.BackBufferWidth / 2;

            float on = 0.8f + (float)Math.Sin(time*MathHelper.Pi)*0.4f;
            float off = 0.1f;

            device.DepthStencilState = DepthStencilState.None;
            SpriteRenderer spriterenderer = SpriteRenderer.GetInstance(device);
            spriterenderer.Begin(null);

            for (int i = 0; i < num_bubbles; i++)
                spriterenderer.Add(Resources.particle_texture_region, new Color(1, 0, 1, off), center - (display_width / 2) + i * space + i * size, 20, size, size, 0);
            
            for (int i = 0; i < num_collected; i++)
                spriterenderer.Add(Resources.particle_texture_region, new Color(1, 0, 1, on), center - (display_width / 2) + i * space + i * size, 20, size, size, 0);


            TextureRegion logo = Resources.logo_texture_region;
            float x, y, scale;
            scale = 0.3f;
            x = device.PresentationParameters.BackBufferWidth - logo.width * scale - 20;
            y = device.PresentationParameters.BackBufferHeight - logo.height * scale - 20;

            spriterenderer.Add(logo, new Color(1f, 1f, 1f, 0.1f), x, y, logo.width * scale, logo.height * scale, 0);


            spriterenderer.End();
            device.DepthStencilState = DepthStencilState.Default;
        }
    }
}
