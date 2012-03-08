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
using Microsoft.Xna.Framework.Input;
using hyades.screen.menu.controls;
using hyades.editor;
using hyades.graphics;
using Microsoft.Xna.Framework.Graphics;

namespace hyades.screen.menu
{
    public class EditorMainMenu: Menu
    {
        public EditorMainMenu(Screen parent, ScreenManager manager)
            : base(parent, manager)
        {
            options = new ScreenOption[]
            {
                new ScreenOption("Resume", "Resume editing", parent),
                new ScreenOption("Options", "Load a level", new EditorOptionsMenu(this, manager)),
                new ScreenOption("Save", "Save the level", null),
                new ScreenOption("Load", "Load a level", null),
                new ScreenOption("Exit", "Exit the editor", null),
            };

            title = "Edit Menu";
        }

        public override void OnActive(double elapsed, InputDevice input)
        {
            Editor editor = Editor.GetInstance();

            if (input.IsPressed(Buttons.Start) || input.IsPressed(Buttons.B))
                TransitionTo(parent);

            if (input.IsPressed(Buttons.A) && curr_option == 2)
                editor.Save();

            if (input.IsPressed(Buttons.A) && curr_option == 3)
                editor.Load();

            if (input.IsPressed(Buttons.A) && curr_option == 4)
                Application.GetInstance().Exit();

            base.OnActive(elapsed, input);
        }
    }
}
