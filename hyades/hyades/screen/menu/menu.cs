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
using hyades.graphics;
using hyades.utils;

namespace hyades.screen.menu
{
    public struct ScreenOption
    {
        public string name;
        public string desc;
        public float value;
        public Screen screen;

        public ScreenOption(string name, string desc, Screen screen)
        { this.name = name; this.desc = desc; this.screen = screen; this.value = 0; }
    }

    public abstract class Menu : Screen
    {
        private float x = 250;
        private float y = 200;

        public string title;
        public ScreenOption[] options;
        public int curr_option;

        public float total_time;
        public float fade_value;
        public float selected_intensity;
        private float time_1, time_2;
        private float prev_direction;

        public const float default_intensity = 0.3f;
        public const float epsilon = 0.3f;
        public const float switch_delay = 0.1f;
        public const float scroll_delay = 0.5f;

        public const float title_scale = 1f;
        public const float option_scale = 1f;
        public const float desc_scale = 1f;
        public const float title_kerring = 8;
        public const float option_kerring = 6;
        public const float desc_kerring = 4;

        public Font font_title, font_option, font_description;

        public Menu(Screen parent, ScreenManager manager)
            : base(parent, manager)
        {
            transition_time = 0.3f;

            font_title = Resources.title_font;
            font_option = Resources.option_font;
            font_description = Resources.description_font;
        }

        public override void FadeIn()
        { InterpolatorCollection.GetInstance().Create(0, 1, transition_time, (i) => { fade_value = i.value; }, null); base.FadeIn(); }

        public override void FadeOut()
        { InterpolatorCollection.GetInstance().Create(1, 0, transition_time, (i) => { fade_value = i.value; }, null); base.FadeOut(); }

        public override void Update(double elapsed)
        {
            total_time += (float)elapsed;

            for (int i = 0; i < options.Length; i++)
            {
                options[i].value -= (float)elapsed * 2;
                if (options[i].value < default_intensity)
                    options[i].value = default_intensity;
            }

            selected_intensity = 0.8f + ((float)Math.Sin(total_time * 4)) * 0.1f;
            options[curr_option].value = selected_intensity;
        }

        public override void OnActive(double elapsed, InputDevice input)
        {
            float curr_input = Math.Abs(input.LeftThumbStick.Y) > Math.Abs(input.RightThumbStick.Y) ? input.LeftThumbStick.Y : input.RightThumbStick.Y;
            if (Math.Abs(curr_input) > epsilon)
                time_1 += (float)elapsed;
            else
                time_1 = 0;

            if (curr_input > epsilon && prev_direction > epsilon
             || curr_input < -epsilon && prev_direction < -epsilon)
                time_2 += (float)elapsed;
            else
                time_2 = 0;

            if ((time_1 > switch_delay && time_2 > scroll_delay) || (Math.Abs(prev_direction) < epsilon && Math.Abs(curr_input) > epsilon))
            {
                if (curr_input > epsilon)
                    curr_option--;
                if (curr_input < -epsilon)
                    curr_option++;

                if (curr_option < 0)
                    curr_option = options.Length - 1;

                curr_option %= options.Length;
                time_1 = 0;
            }

            if (input.IsPressed(Buttons.A))
            {
                TransitionTo(options[curr_option].screen);
            }

            prev_direction = curr_input;
        }

        public override void Draw(GraphicsDevice device)
        {
            PostProcessor postprocessor = PostProcessor.GetInstance(device);
            postprocessor.Blur(postprocessor.result_rt, postprocessor.result_rt, 0.5f + 8 * fade_value);


            SpriteRenderer spriterenderer = SpriteRenderer.GetInstance(device);
            spriterenderer.Begin(null);

            spriterenderer.AddString(font_title, title.ToUpper(), GetX(), GetY(), 0, title_scale, title_kerring, 0, 0, 0, new Color(1f, 1f, 1f, fade_value));

            for (int i = 0; i < options.Length; i++)
                spriterenderer.AddString(font_option, options[i].name.ToUpper(), GetOptionPositionX(i), GetOptionPositionY(i), 0, option_scale /*+ (options[i].value - default_intensity) * option_scale * 0.1f*/, option_kerring, 0, 0, 0, new Color(options[i].value, options[i].value, options[i].value, fade_value));

            spriterenderer.AddString(font_description, options[curr_option].desc.ToUpper(), GetX(), GetY() + (font_title.height * title_scale * 3) + options.Length * (font_option.height + font_option.height * 0.5f) * option_scale, 0, desc_scale, desc_kerring, 0, 0, 0, new Color(1f, 1f, 1f, fade_value / 2));

            //spriterenderer.AddString(Resources.arial10px_font, string.Format("transition:{0:0.000} switch:{1:0.000} scroll:{2:0.000}", fade_value, time_1, time_2), x, y-12);
            spriterenderer.End();
        }

        private float GetX()
        { return x * fade_value; }

        private float GetY()
        { return y; }

        public float GetOptionPositionX(int index)
        { return GetX(); }

        public float GetOptionPositionY(int index)
        { return GetY() + (font_title.height * title_scale * 2) + index * (font_option.height + font_option.height * 0.3f) * option_scale; }

        public float GetOptionCenterPositionY(int index)
        { return GetOptionPositionY(index) + font_option.height * option_scale * 0.5f; }
    }
}
