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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using hyades.graphics;

namespace hyades.screen.menu.controls
{
    public class Slider : Screen
    {
        public int index;
        private float x, y;
        public float value, new_value;

        private const float offset = 400;
        private const float bar_width = 150;
        private const float bar_height = 5;
        private const float slider_width = 5;
        private const float slider_height = 15;

        private Action<float> action;

        public Slider(Menu parent, int index, float value, Action<float> action)
            : base(parent, parent.screenmanager)
        {
            this.index = index;
            this.action = action;
            SetValue(value);
        }

        public void SetValue(float value)
        {
            this.value = value;
            this.new_value = value;
            this.action(value);
        }

        public override void Update(double elapsed)
        {
            Menu parent = ((Menu)this.parent);
            this.x = parent.GetOptionPositionX(index);
            this.y = parent.GetOptionCenterPositionY(index);
        }

        public override void OnActive(double elapsed, InputDevice input)
        {
            float input_x, input_y;
            input_x = input.LeftThumbStick.X;
            input_y = input.LeftThumbStick.Y;
            if (Math.Abs(input_x) + Math.Abs(input_y) < Math.Abs(input.RightThumbStick.X) + Math.Abs(input.RightThumbStick.Y))
            {
                input_x = input.RightThumbStick.X;
                input_y = input.RightThumbStick.Y;
            }

            new_value += (float)(input_y * elapsed);
            new_value += (float)(input_x * elapsed);
            new_value = MathHelper.Clamp(new_value, 0, 1);

            if (input.IsPressed(Buttons.B)) // cancel
            {
                screenmanager.SetActive(parent);
                new_value = value;
                action(value);
            }

            if (input.IsPressed(Buttons.A)) // apply
            {
                screenmanager.SetActive(parent);
                value = new_value;
                action(value);
            }

            action(new_value);
        }

        public override void Draw(GraphicsDevice device)
        {
            Menu parent = ((Menu)this.parent);

            float fade_value = parent.fade_value;
            float selected = parent.curr_option == index ? parent.selected_intensity : Menu.default_intensity;

            Color bar_color, slider_color;
            if (IsActive())
            {
                bar_color = new Color(Menu.default_intensity - 0.1f + 0.5f, Menu.default_intensity - 0.1f, Menu.default_intensity - 0.1f, fade_value);
                slider_color = new Color(Menu.default_intensity + 0.5f, Menu.default_intensity, Menu.default_intensity, fade_value * 0.8f);
            }
            else
            {
                bar_color = new Color(selected - 0.1f, selected - 0.1f, selected - 0.1f, fade_value);
                slider_color = new Color(selected, selected, selected, fade_value * 0.8f);
            }

            SpriteRenderer spriterenderer = SpriteRenderer.GetInstance(device);

            spriterenderer.Begin(null);
            spriterenderer.Add(Resources.white_texture_region, bar_color, x + offset, y - bar_height, bar_width, bar_height, 0);
            spriterenderer.Add(Resources.white_texture_region, slider_color, x + offset + bar_width * new_value - 0.5f * slider_width, y - slider_height * 0.75f, slider_width, slider_height, 0);
            spriterenderer.End();
        }
    }
}
