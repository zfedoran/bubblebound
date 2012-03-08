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
using hyades.graphics;
using Microsoft.Xna.Framework.Input;
using hyades.utils;

namespace hyades.screen
{

    public class Title : Screen
    {
        private TextureRegion logo;
        private float alpha;
        private float background;

        public Title(Screen parent, ScreenManager manager)
            : base(parent, manager)
        {
            logo = Resources.logo_texture_region;
            transition_time = 3;
            background = 1;
        }

        public override void FadeIn()
        { InterpolatorCollection.GetInstance().Create(0, 1, transition_time, (i) => { alpha = i.value; }, null); base.FadeIn(); }

        public override void FadeOut()
        {
            InterpolatorCollection.GetInstance().Create(1, 0, transition_time, (i) => { alpha = i.value; }, null); base.FadeOut();
            InterpolatorCollection.GetInstance().Create(1, 0, transition_time, (i) => { background = i.value; }, null); base.FadeOut(); 
        }

        public override void Update(double elapsed)
        {
        }

        public override void OnActive(double elapsed, InputDevice input)
        {
            if (input.IsPressed(Buttons.A) || input.IsPressed(Buttons.B) || input.IsPressed(Buttons.Start))
            {
                TransitionTo(parent);
            }
        }

        public override void Draw(GraphicsDevice device)
        {
            PostProcessor postprocessor = PostProcessor.GetInstance(device);
            postprocessor.Blur(postprocessor.result_rt, postprocessor.result_rt, 0.5f + 8 * background);

            float x, y;
            x = (device.PresentationParameters.BackBufferWidth - logo.width) / 2f;
            y = (device.PresentationParameters.BackBufferHeight - logo.height) / 2f;

            SpriteRenderer spriterenderer = SpriteRenderer.GetInstance(device);

            spriterenderer.Begin(null);
            spriterenderer.Add(logo, new Color(1f, 1f, 1f, alpha), x, y, logo.width, logo.height, 0);
            spriterenderer.End();
        }
    }

    public class Instructions : Screen
    {
        private TextureRegion logo;
        private float alpha;
        private float background;

        public Instructions(Screen parent, ScreenManager manager)
            : base(parent, manager)
        {
            logo = Resources.instructions_texture_region;
            transition_time = 3;
            background = 1;
        }

        public override void FadeIn()
        { InterpolatorCollection.GetInstance().Create(0, 1, transition_time, (i) => { alpha = i.value; }, null); base.FadeIn(); }

        public override void FadeOut()
        {
            InterpolatorCollection.GetInstance().Create(1, 0, transition_time, (i) => { alpha = i.value; }, null); base.FadeOut();
            InterpolatorCollection.GetInstance().Create(1, 0, transition_time, (i) => { background = i.value; }, null); base.FadeOut();
        }

        public override void Update(double elapsed)
        {
        }

        public override void OnActive(double elapsed, InputDevice input)
        {
            if (input.IsPressed(Buttons.A) || input.IsPressed(Buttons.B) || input.IsPressed(Buttons.Start))
            {
                TransitionTo(parent);
            }
        }

        public override void Draw(GraphicsDevice device)
        {
            PostProcessor postprocessor = PostProcessor.GetInstance(device);
            postprocessor.Blur(postprocessor.result_rt, postprocessor.result_rt, 0.5f + 8 * background);

            float x, y;
            x = (device.PresentationParameters.BackBufferWidth - logo.width) / 2f;
            y = (device.PresentationParameters.BackBufferHeight - logo.height) / 2f;

            SpriteRenderer spriterenderer = SpriteRenderer.GetInstance(device);

            spriterenderer.Begin(null);
            spriterenderer.Add(logo, new Color(1f, 1f, 1f, alpha), x, y, logo.width, logo.height, 0);
            spriterenderer.End();
        }
    }

}
