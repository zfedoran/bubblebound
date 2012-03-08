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
using hyades.editor;

namespace hyades.screen.menu
{
    
    public class EditorOptionsMenu : Menu
    {
        private Switch camera_switch;
        private Switch debug_switch;
        private const bool default_free_camera = false;
        private const bool default_debug_view = true;

        public EditorOptionsMenu(Screen parent, ScreenManager manager)
            : base(parent, manager)
        {
            Editor editor = Editor.GetInstance();

            camera_switch = new Switch(this, 1, default_free_camera, editor.SetCamera);
            debug_switch = new Switch(this, 2, default_debug_view, editor.SetDebugView);

            options = new ScreenOption[]
            {
                new ScreenOption("Reset to Default", "Reset the volume and brightness to default values", null),
                new ScreenOption("Free Camera", "Change the camera type", null),
                new ScreenOption("Debug View", "Toggle debug view", null),
                new ScreenOption("Back", "Go back to the previous menu", parent)
            };

            title = "Options";
        }

        public override void OnActive(double elapsed, InputDevice input)
        {
            if (input.IsPressed(Buttons.B))
                TransitionTo(parent);

            if (input.IsPressed(Buttons.A) && curr_option == 0)
            {
                camera_switch.SetValue(default_free_camera);
                debug_switch.SetValue(default_debug_view);
            }

            if (input.IsPressed(Buttons.A) && curr_option == camera_switch.index)
                screenmanager.SetActive(camera_switch);

            if (input.IsPressed(Buttons.A) && curr_option == debug_switch.index)
                screenmanager.SetActive(debug_switch);


            base.OnActive(elapsed, input);
        }

        public override void FadeIn()
        {
            screenmanager.FadeIn(camera_switch);
            screenmanager.FadeIn(debug_switch);
            screenmanager.SetActive(this);

            base.FadeIn();
        }

        public override void FadeOut()
        {
            screenmanager.FadeOut(camera_switch);
            screenmanager.FadeOut(debug_switch);

            base.FadeOut();
        }
    }
}
