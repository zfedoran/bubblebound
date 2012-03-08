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
using Microsoft.Xna.Framework;
using hyades.graphics;

namespace hyades.screen.menu
{
    public class ControlsMenu : Menu
    {
        public ControlsMenu(Screen parent, ScreenManager manager)
            : base(parent, manager)
        {
            options = new ScreenOption[]
            {
                new ScreenOption("BACK", "Go back to the previous menu", parent),
            };

            title = "Controls";
        }

        public override void OnActive(double elapsed, InputDevice input)
        {
            if (input.IsPressed(Buttons.B))
            {
                TransitionTo(parent);
            }

            base.OnActive(elapsed, input);
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.GraphicsDevice device)
        {
            base.Draw(device);

            SpriteRenderer spriterenderer = SpriteRenderer.GetInstance(device);
            spriterenderer.Begin(null);
            spriterenderer.Add(Resources.controls_region, new Color(1f, 1f, 1f, 0.8f), GetOptionPositionX(0)+350, GetOptionPositionY(0)-100, Resources.controls_region.width, Resources.controls_region.height, 0);
            spriterenderer.End();
        }
    }
}
