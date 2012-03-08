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
using System.Reflection;
using System.Diagnostics;

namespace hyades.utils
{
    /// <summary>
    /// A collection that maintains a set of class instances to allow for recycling
    /// instances and minimizing the effects of garbage collection.
    /// </summary>
    /// <typeparam name="T">The type of object to store in the Pool. Pools can only hold class types.</typeparam>
    public class Pool<T> where T : class
    {
        // the amount to enlarge the items array if New is called and there are no free items
        private const int resizeAmount = 20;

        // the actual items of the pool
        private T[] items;

        // used for checking if a given object is still valid
        private readonly Predicate<T> validate;

        // used for allocating instances of the object
        private readonly Allocate allocate;

        // a constructor the default allocate method can use to create instances
        private readonly ConstructorInfo constructor;

        /// <summary>
        /// Gets or sets a delegate used for initializing objects before returning them from the New method.
        /// </summary>
        public Action<T> Initialize { get; set; }

        /// <summary>
        /// Gets or sets a delegate that is run when an object is moved from being valid to invalid
        /// in the CleanUp method.
        /// </summary>
        public Action<T> Deinitialize { get; set; }

        /// <summary>
        /// Gets the number of valid objects in the pool.
        /// </summary>
        public int ValidCount { get { return items.Length - InvalidCount; } }

        /// <summary>
        /// Gets the number of invalid objects in the pool.
        /// </summary>
        public int InvalidCount { get; private set; }

        /// <summary>
        /// Returns a valid object at the given index. The index must fall in the range of [0, ValidCount].
        /// </summary>
        /// <param name="index">The index of the valid object to get</param>
        /// <returns>A valid object found at the index</returns>
        public T this[int index]
        {
            get
            {
                index += InvalidCount;

                if (index < InvalidCount || index >= items.Length)
                    throw new IndexOutOfRangeException("The index must be less than or equal to ValidCount");

                return items[index];
            }
        }

        /// <summary>
        /// Creates a new pool.
        /// </summary>
        /// <param name="validateFunc">A predicate used to determine if a given object is still valid.</param>
        public Pool(Predicate<T> validateFunc) : this(0, validateFunc) { }

        /// <summary>
        /// Creates a new pool with a specific starting size.
        /// </summary>
        /// <param name="initialSize">The initial size of the pool.</param>
        /// <param name="validateFunc">A predicate used to determine if a given object is still valid.</param>
        public Pool(int initialSize, Predicate<T> validateFunc) : this(initialSize, validateFunc, null) { }

        /// <summary>
        /// Creates a new pool with a specific starting size.
        /// </summary>
        /// <param name="initialSize">The initial size of the pool.</param>
        /// <param name="validateFunc">A predicate used to determine if a given object is still valid.</param>
        /// <param name="allocateFunc">A function used to allocate an instance for the pool.</param>
        public Pool(int initialSize, Predicate<T> validateFunc, Allocate allocateFunc)
        {
            // validate some parameters
            if (initialSize < 0)
                throw new ArgumentException("initialSize must be non-negative");
            if (validateFunc == null)
                throw new ArgumentNullException("validateFunc");

            if (initialSize == 0)
                initialSize = 10;

            items = new T[initialSize];
            validate = validateFunc;
            InvalidCount = items.Length;

            // default to using a parameterless constructor if no allocateFunc was given
            allocate = allocateFunc ?? ConstructorAllocate;

            // if we are using the ConstructorAllocate method, make sure we have a valid parameterless constructor
            if (allocate == ConstructorAllocate)
            {
                // we want to find any parameterless constructor, public or not
                constructor = typeof(T).GetConstructor(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new Type[] { },
                    null);

                if (constructor == null)
                    throw new InvalidOperationException(typeof(T) + " does not have a parameterless constructor.");
            }
        }

        /// <summary>
        /// Cleans up the pool by checking each valid object to ensure it is still actually valid.
        /// </summary>
        public void CleanUp()
        {
            for (int i = InvalidCount; i < items.Length; i++)
            {
                T obj = items[i];

                // if it's still valid, keep going
                if (validate(obj))
                    continue;

                // otherwise if we're not at the start of the invalid objects, we have to move
                // the object to the invalid object section of the array
                if (i != InvalidCount)
                {
                    items[i] = items[InvalidCount];
                    items[InvalidCount] = obj;
                }

                // clean the object if desired
                if (Deinitialize != null)
                    Deinitialize(obj);

                InvalidCount++;
            }
        }

        /// <summary>
        /// Returns a new object from the Pool.
        /// </summary>
        /// <returns>The next object in the pool if available, null if all instances are valid.</returns>
        public T New()
        {
            // if we're out of invalid instances, resize to fit some more
            if (InvalidCount == 0)
            {
#if DEBUG
                Trace.WriteLine("Resizing pool. Old size: " + items.Length + ". New size: " + (items.Length + resizeAmount));
#endif
                // create a new array with some more slots and copy over the existing items
                T[] newItems = new T[items.Length + resizeAmount];
                for (int i = items.Length - 1; i >= 0; i--)
                    newItems[i + resizeAmount] = items[i];
                items = newItems;

                // move the invalid count based on our resize amount
                InvalidCount += resizeAmount;
            }

            // decrement the counter
            InvalidCount--;

            // get the next item in the list
            T obj = items[InvalidCount];

            // if the item is null, we need to allocate a new instance
            if (obj == null)
            {
                obj = allocate();

                if (obj == null)
                    throw new InvalidOperationException("The pool's allocate method returned a null object reference.");

                items[InvalidCount] = obj;
            }

            // initialize the object if a delegate was provided
            if (Initialize != null)
                Initialize(obj);

            return obj;
        }

        // a default Allocate delegate for use when no custom allocate delegate is provided
        private T ConstructorAllocate()
        {
            return constructor.Invoke(null) as T;
        }

        /// <summary>
        /// A delegate that returns a new object instance for the Pool.
        /// </summary>
        /// <returns>A new object instance.</returns>
        public delegate T Allocate();
    }
}
