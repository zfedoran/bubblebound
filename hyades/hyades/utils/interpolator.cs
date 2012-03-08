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
    public partial class Interpolator
    {
        private bool valid;
        public float progress;
        public float start;
        public float end;
        public float value;
        public object tag;
        private float range;
        private float speed;
        private Action<Interpolator> step;
        private Action<Interpolator> completed;

        /// <summary>
        /// Gets whether or not the timer is active.
        /// </summary>
        public bool IsActive { get { return valid; } }
        
        /// <summary>
        /// Gets or sets some extra data to the timer.
        /// </summary>
        public object Tag { get { return tag; } set { tag = value; } }

        /// <summary>
        /// Internal constructor used by InterpolatorCollection
        /// </summary>
        internal Interpolator() { }

        /// <summary>
        /// Creates a new Interpolator.
        /// </summary>
        /// <param name="startValue">The starting value.</param>
        /// <param name="endValue">The ending value.</param>
        /// <param name="step">An optional delegate to invoke each update.</param>
        /// <param name="completed">An optional delegate to invoke upon completion.</param>
        public Interpolator(float startValue, float endValue, Action<Interpolator> step, Action<Interpolator> completed)
            : this(startValue, endValue, 1f, step, completed)
        {
        }

        /// <summary>
        /// Creates a new Interpolator.
        /// </summary>
        /// <param name="startValue">The starting value.</param>
        /// <param name="endValue">The ending value.</param>
        /// <param name="interpolationLength">The amount of time, in seconds, for the interpolation to occur.</param>
        /// <param name="step">An optional delegate to invoke each update.</param>
        /// <param name="completed">An optional delegate to invoke upon completion.</param>
        public Interpolator(float startValue, float endValue, float interpolationLength, Action<Interpolator> step, Action<Interpolator> completed)
        {
            Reset(startValue, endValue, interpolationLength, step, completed);
        }

        /// <summary>
        /// Stops the Interpolator.
        /// </summary>
        public void Stop()
        {
            valid = false;
            step = null;
            completed = null;
        }

        /// <summary>
        /// Forces the interpolator to set itself to its final position and fire off its delegates before invalidating itself.
        /// </summary>
        public void ForceFinish()
        {
            if (valid)
            {
                valid = false;
                progress = 1;
                float scaledProgress = progress;
                value = start + range * scaledProgress;

                if (step != null)
                    step(this);

                if (completed != null)
                    completed(this);
            }
        }

        internal void Reset(float s, float e, float l, Action<Interpolator> stepFunc, Action<Interpolator> completedFunc)
        {
            if (l <= 0f)
                throw new ArgumentException("length must be greater than zero");


            valid = true;
            progress = 0f;

            start = s;
            end = e;
            range = e - s;
            speed = 1f / l;

            step = stepFunc;
            completed = completedFunc;
        }

        /// <summary>
        /// Updates the Interpolator.
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(double elapsed)
        {
            if (!valid)
                return;

            // update the progress, clamping at 1f
            progress = (float)Math.Min(progress + speed * elapsed, 1f);

            // get the scaled progress and use that to generate the value
            float scaledProgress = progress;
            value = start + range * scaledProgress;

            // invoke the step callback
            if (step != null)
                step(this);

            // if the progress is 1...
            if (progress == 1f)
            {
                // the interpolator is done
                valid = false;

                // invoke the completed callback
                if (completed != null)
                    completed(this);

                tag = null;
                step = null;
                completed = null;
            }
        }
    }

    /// <summary>
    /// A managed collection of interpolators.
    /// </summary>
    public sealed class InterpolatorCollection
    {
        private static InterpolatorCollection interpolatorcollection;

        static InterpolatorCollection()
        { interpolatorcollection = new InterpolatorCollection(); }

        public static InterpolatorCollection GetInstance()
        { return interpolatorcollection; }

        private readonly Pool<Interpolator> interpolators = new Pool<Interpolator>(10, i => i.IsActive);

        /// <summary>
        /// Creates a new Interpolator.
        /// </summary>
        /// <param name="start">The starting value.</param>
        /// <param name="end">The ending value.</param>
        /// <param name="step">An optional callback to invoke when the Interpolator is updated.</param>
        /// <param name="completed">An optional callback to invoke when the Interpolator completes.</param>
        /// <returns>The Interpolator instance.</returns>
        public Interpolator Create(
            float start,
            float end,
            Action<Interpolator> step,
            Action<Interpolator> completed)
        {
            return Create(start, end, 1f, step, completed);
        }

        /// <summary>
        /// Creates a new Interpolator.
        /// </summary>
        /// <param name="start">The starting value.</param>
        /// <param name="end">The ending value.</param>
        /// <param name="length">The length of time, in seconds, to perform the interpolation.</param>
        /// <param name="step">An optional callback to invoke when the Interpolator is updated.</param>
        /// <param name="completed">An optional callback to invoke when the Interpolator completes.</param>
        /// <returns>The Interpolator instance.</returns>
        public Interpolator Create(
            float start,
            float end,
            float length,
            Action<Interpolator> step,
            Action<Interpolator> completed)
        {
            lock (interpolators)
            {
                Interpolator i = interpolators.New();
                i.Reset(start, end, length, step, completed);

                return i;
            }
        }


        /// <summary>
        /// Updates all active Interpolators in the collection.
        /// </summary>
        /// <param name="gameTime">The current game timestamp.</param>
        public void Update(double elapsed)
        {
            lock (interpolators)
            {
                for (int i = 0; i < interpolators.ValidCount; i++)
                    interpolators[i].Update(elapsed);
                interpolators.CleanUp();
            }
        }
    }
}
