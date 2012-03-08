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

namespace hyades.utils
{
    /// <summary>
    /// An object that invokes an action after an amount of time has elapsed and
    /// optionally continues repeating until told to stop.
    /// </summary>
    public partial class Timer 
    {
        private bool valid;
        public float time;
        public float tickLength;
        public object tag;
        private bool repeats;
        private Action<Timer> tick;

        /// <summary>
        /// Gets whether or not the timer is active.
        /// </summary>
        public bool IsActive { get { return valid; } }

        /// <summary>
        /// Gets or sets some extra data to the timer.
        /// </summary>
        public object Tag { get { return tag; } set { tag = value; } }

        /// <summary>
        /// Gets whether or not this timer repeats.
        /// </summary>
        public bool Repeats { get { return repeats; } }

        internal Timer() { }

        /// <summary>
        /// Creates a new Timer.
        /// </summary>
        /// <param name="length">The length of time between ticks.</param>
        /// <param name="repeats">Whether or not the timer repeats.</param>
        /// <param name="tick">The delegate to invoke when the timer ticks.</param>
        public Timer(float length, bool repeats, Action<Timer> tick)
        {
            if (length <= 0f)
                throw new ArgumentException("length must be greater than 0");
            if (tick == null)
                throw new ArgumentNullException("tick");

            Reset(length, repeats, tick);
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
        public void Stop()
        {
            valid = false;
            tick = null;
            Tag = null;
        }

        /// <summary>
        /// Forces the timer to fire its tick event, invalidating the timer unless it is set to repeat.
        /// </summary>
        public void ForceTick()
        {
            if (!valid)
                return;

            tick(this);
            time = 0f;

            valid = Repeats;

            if (!valid)
            {
                tick = null;
                Tag = null;
            }
        }

        internal void Reset(float l, bool r, Action<Timer> t)
        {
            valid = true;
            time = 0f;
            tickLength = l;
            repeats = r;
            tick = t;
        }

        /// <summary>
        /// Updates the timer.
        /// </summary>
        public void Update(double elapsed)
        {
            // if a timer is stopped manually, it may not
            // be valid at this point so we skip i
            if (!valid)
                return;

            // update the timer's time
            time += (float)elapsed;

            // if the timer passed its tick length...
            if (time >= tickLength)
            {
                // perform the action
                tick(this);

                // subtract the tick length in case we need to repeat
                time -= tickLength;

                // if the timer doesn't repeat, it is no longer valid
                valid = repeats;

                if (!valid)
                {
                    tick = null;
                    Tag = null;
                }
            }
        }
    }


    /// <summary>
    /// A managed collection of timers.
    /// </summary>
    public sealed class TimerCollection
    {
        private static TimerCollection timercollection;

        static TimerCollection()
        { timercollection = new TimerCollection(); }

        public static TimerCollection GetInstance()
        { return timercollection; }

        private readonly Pool<Timer> timers = new Pool<Timer>(10, t => t.IsActive);

        /// <summary>
        /// Creates a new Timer.
        /// </summary>
        /// <param name="tickLength">The amount of time between the timer's ticks.</param>
        /// <param name="repeats">Whether or not the timer repeats.</param>
        /// <param name="tick">An action to perform when the timer ticks.</param>
        /// <returns>The new Timer object or null if the timer pool is full.</returns>
        public Timer Create(float tickLength, bool repeats, Action<Timer> tick)
        {
            if (tickLength <= 0f)
                throw new ArgumentException("tickLength must be greater than zero.");
            if (tick == null)
                throw new ArgumentNullException("tick");

            lock (timers)
            {
                // get a new timer from the pool
                Timer t = timers.New();
                t.Reset(tickLength, repeats, tick);

                return t;
            }
        }

        /// <summary>
        /// Updates all timers in the collection.
        /// </summary>
        /// <param name="gameTime">The current game timestamp.</param>
        public void Update(double elapsed)
        {
            lock (timers)
            {
                for (int i = 0; i < timers.ValidCount; i++)
                    timers[i].Update(elapsed);
                timers.CleanUp();
            }
        }
    }
}
