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
using hyades.graphics;
using hyades.utils;

namespace hyades.screen
{
    public abstract class Screen
    {
        public ScreenManager screenmanager;
        public bool ready;
        public Screen parent;
        public float transition_time;

        public Screen(Screen parent, ScreenManager manager)
        {
            this.parent = parent;
            this.screenmanager = manager;
            this.ready = true;
            this.transition_time = 0.5f;
        }

        public bool IsActive()
        { return screenmanager.IsActive(this); }

        public bool IsReady()
        { return ready; }

        public virtual void FadeIn()
        { DelayedSetReady(); ready = false; }
        public virtual void FadeOut()
        { DelayedSetReady(); ready = false; }

        public virtual void Update(double elapsed) { }
        public abstract void OnActive(double elapsed, InputDevice input);
        public abstract void Draw(GraphicsDevice device);

        public void TransitionTo(Screen screen)
        { screenmanager.Transition(this, screen); }

        private void DelayedSetReady()
        { TimerCollection.GetInstance().Create(transition_time, false, SetReady); }

        private void SetReady(Timer t)
        { ready = true; }

    }
}
