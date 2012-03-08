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
    public class Switch : Screen
    {
        public int index;
        private float x, y;
        public bool value, new_value;

        private const float offset = 400;

        private Font font;
        private Action<bool> action;

        public Switch(Menu parent, int index, bool value, Action<bool> action)
            : base(parent, parent.screenmanager)
        {
            this.index = index;
            this.font = parent.font_option;
            this.action = action;
            SetValue(value);
        }

        public void SetValue(bool value)
        {
            this.value = value;
            this.new_value = value;
            this.action(value);
        }

        public override void Update(double elapsed)
        {
            Menu parent = ((Menu)this.parent);
            this.x = parent.GetOptionPositionX(index);
            this.y = parent.GetOptionPositionY(index);
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

            if (input_y > Menu.epsilon)
                new_value = true;
            if (input_y < -Menu.epsilon)
                new_value = false;
            if (input_x > Menu.epsilon)
                new_value = true;
            if (input_x < -Menu.epsilon)
                new_value = false;

            if (input.IsPressed(Buttons.B)) // cancel
            {
                screenmanager.SetActive(parent);
                new_value = value;
            }

            if (input.IsPressed(Buttons.A)) // apply
            {
                screenmanager.SetActive(parent);

                if (new_value != value)
                {
                    value = new_value;
                    action(value);
                }
            }
        }

        public override void Draw(GraphicsDevice device)
        {
            Menu parent = ((Menu)this.parent);

            float fade_value = parent.fade_value;
            float selected = parent.curr_option == index ? parent.selected_intensity : Menu.default_intensity;

            Color color;

            if (IsActive())
                color = new Color(Menu.default_intensity + 0.5f, Menu.default_intensity, Menu.default_intensity, fade_value);
            else
                color = new Color(selected, selected, selected, fade_value);

            SpriteRenderer spriterenderer = SpriteRenderer.GetInstance(device);

            spriterenderer.Begin(null);
            spriterenderer.AddString(font, new_value ? "ON" : "OFF", x + offset, y, 0, Menu.option_scale, Menu.option_kerring, 0, 0, 0, color);
            spriterenderer.End();
        }
    }
}
