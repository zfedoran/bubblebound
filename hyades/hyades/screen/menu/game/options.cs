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
using hyades.screen.menu.controls;

namespace hyades.screen.menu
{
    
    public class OptionsMenu : Menu
    {
        private Slider volume_slider;
        private Slider brightness_slider;
        private Switch debug_switch;

        private const float default_volume = 0.5f;
        private const float default_brightness = 0.5f;
        private const bool  default_debug = false;

        public OptionsMenu(Screen parent, ScreenManager manager)
            : base(parent, manager)
        {
            Hyades game = Hyades.GetInstance();

            volume_slider = new Slider(this, 1, default_volume, Application.GetInstance().SetVolume);
            brightness_slider = new Slider(this, 2, default_brightness, Application.GetInstance().SetBrightness);
            debug_switch = new Switch(this, 3, default_debug, game.SetDebugView);

            options = new ScreenOption[]
            {
                new ScreenOption("Reset to Default", "Reset the volume and brightness to default values", null),
                new ScreenOption("Volume", "Change the game music and sound effect volume", null),
                new ScreenOption("Brightness", "Change the game brightness", null),
                new ScreenOption("Debug", "Show debug view", null),
                new ScreenOption("Back", "Go back to the previous menu", parent)
            };

            title = "Options";
        }

        public override void OnActive(double elapsed, InputDevice input)
        {
            if (input.IsPressed(Buttons.B))
                TransitionTo(parent);

            if (input.IsPressed(Buttons.A) && curr_option == volume_slider.index)
                screenmanager.SetActive(volume_slider);

            if (input.IsPressed(Buttons.A) && curr_option == brightness_slider.index)
                screenmanager.SetActive(brightness_slider);

            if (input.IsPressed(Buttons.A) && curr_option == debug_switch.index)
                screenmanager.SetActive(debug_switch);

            if (input.IsPressed(Buttons.A) && curr_option == 0)
            {
                volume_slider.SetValue(default_volume);
                brightness_slider.SetValue(default_brightness);
                debug_switch.SetValue(default_debug);
            }

            base.OnActive(elapsed, input);
        }

        public override void FadeIn()
        {
            screenmanager.FadeIn(volume_slider);
            screenmanager.FadeIn(brightness_slider);
            screenmanager.FadeIn(debug_switch);
            screenmanager.SetActive(this);

            base.FadeIn();
        }

        public override void FadeOut()
        {
            screenmanager.FadeOut(volume_slider);
            screenmanager.FadeOut(brightness_slider);
            screenmanager.FadeOut(debug_switch);

            base.FadeOut();
        }
    }
}
