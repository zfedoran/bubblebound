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
using hyades.utils;

namespace hyades.screen
{
    public class ScreenManager
    {
        private static ScreenManager instance;

        public static ScreenManager GetInstance()
        { return instance; }

        private List<Screen> screen_list;
        private Screen active;
        private InputDevice input;

        public ScreenManager(InputDevice input)
        {
            this.screen_list = new List<Screen>();
            this.input = input;
            instance = this;
        }

        public bool IsActive(Screen screen)
        { return screen == active; }

        public void SetActive(Screen screen)
        {
            if (screen == null || !screen.IsReady())
                return;

            input.Update();

            if (!screen_list.Contains(screen))
                screen_list.Add(screen);

            active = screen;
        }

        public void Transition(Screen from, Screen to)
        {
            if (from == null || !from.IsReady())
                return;
            if (to == null || !to.IsReady())
                return;

            FadeOut(from);
            FadeIn(to);
        }

        public void FadeIn(Screen screen)
        {
            if (screen == null || !screen.IsReady())
                return;

            SetActive(screen);
            screen.FadeIn();
        }

        public void FadeOut(Screen screen)
        {
            if (screen == null || !screen.IsReady())
                return;

            screen.FadeOut();
            DelayedRemoved(screen);
        }

        private void DelayedRemoved(Screen screen)
        { Timer t = TimerCollection.GetInstance().Create(screen.transition_time, false, Remove); t.tag = screen; }

        private void Remove(Timer t)
        { screen_list.Remove((Screen)t.tag); }

        public void Update(double elapsed)
        {
            /*
            for (int i = screen_list.Count - 1; i >= 0; i--)
            {
                Screen screen = screen_list[i];
                if (screen.IsRemoved())
                    screen_list.Remove(screen);
            }
            */
            for (int i = 0; i < screen_list.Count; i++)
                screen_list[i].Update(elapsed);

            active.OnActive(elapsed, input);
        }

        public void Draw(GraphicsDevice device)
        {
            for (int i = 0; i < screen_list.Count; i++)
                screen_list[i].Draw(device);
            
        }
    }
}
