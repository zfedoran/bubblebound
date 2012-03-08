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
using hyades.editor;

namespace hyades.screen
{
    public class EditorScreen : Screen
    {
        public Editor editor;

        public Title title_screen;
        public Menu menu_screen;

        public EditorScreen(Editor editor, ScreenManager manager)
            : base(null, manager)
        {
            this.editor = editor;

            menu_screen = new EditorMainMenu(this, manager);
        }

        public override void OnActive(double elapsed, InputDevice input)
        {
            editor.Update(elapsed);

            if (input.IsPressed(Buttons.Start))
                screenmanager.FadeIn(menu_screen);
        }

        public override void Draw(GraphicsDevice device)
        {
            editor.Draw(device);
        }


    }
}
