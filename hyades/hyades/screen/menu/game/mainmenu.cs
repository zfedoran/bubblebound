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

namespace hyades.screen.menu
{
    public class MainMenu : Menu
    {
        public MainMenu(Screen parent, ScreenManager manager)
            : base(parent, manager)
        {
            options = new ScreenOption[]
            {
                new ScreenOption("Resume", "Resume playing the game", parent), 
                new ScreenOption("Controls", "View the game controls", new ControlsMenu(this, manager)),
                new ScreenOption("Options", "View and change the game options", new OptionsMenu(this, manager)),
                new ScreenOption("Exit Game", "Exit the game", null),
            };

            title = "Main Menu";
        }

        public override void OnActive(double elapsed, InputDevice input)
        {
            if (input.IsPressed(Buttons.Start) || input.IsPressed(Buttons.B))
                TransitionTo(parent);

            if (input.IsPressed(Buttons.A) && curr_option == 3)
                Application.GetInstance().Exit();

            base.OnActive(elapsed, input);
        }
    }
}
