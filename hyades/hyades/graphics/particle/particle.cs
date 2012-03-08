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
using Microsoft.Xna.Framework;

namespace hyades.graphics.particle
{
    public interface IParticleLogic
    {
        void Birth(Particle particle);
        void Frame(Particle particle, double elapsed);
        void Death(Particle particle);
    }

    public class Particle
    {
        public Matrix world;
        public Vector3 position, velocity, rotation;
        public Vector2 scale;
        public Color color;
        public TextureRegion region;
        public float age, life, velocity_scale;
        public bool is_alive, in_pool, is_velocity_particle;

        public IParticleLogic logic;

        public void Create(IParticleLogic logic, float life)
        {
            age = 0;
            position = Vector3.Zero;
            velocity = Vector3.Zero;
            rotation = Vector3.Zero;
            scale = Vector2.One;
            color = Color.White;
            velocity_scale = 0;
            is_velocity_particle = false;
            is_alive = true;

            this.life = life;
            this.logic = logic;

            logic.Birth(this);
        }

        public void UpdateWorldMatrix(Camera camera)
        {
            world = Matrix.Identity;

            // calculate rotation
            if (rotation.X != 0 && rotation.Y != 0 && rotation.Z != 0)
            {
                float a = (float)Math.Cos(rotation.X);
                float b = (float)Math.Sin(rotation.X);

                float c = (float)Math.Cos(rotation.Y);
                float d = (float)Math.Sin(rotation.Y);

                float e = (float)Math.Cos(rotation.Z);
                float f = (float)Math.Sin(rotation.Z);


                world.M11 = (c * e);
                world.M12 = (c * f);
                world.M13 = -d;
                world.M21 = (e * b * d - a * f);
                world.M22 = ((e * a) + (f * b * d));
                world.M23 = (b * c);
                world.M31 = (e * a * d + b * f);
                world.M33 = (a * c);
                world.M32 = -(b * e - f * a * d);
            }

            world.M11 *= scale.X;
            world.M12 *= scale.X;
            world.M13 *= scale.X;
            world.M21 *= scale.Y;
            world.M22 *= scale.Y;
            world.M23 *= scale.Y;
            world.M41 = position.X;
            world.M42 = position.Y;
            world.M43 = position.Z;


            // rotate and scale particle in the direction of velocity
            if (is_velocity_particle)
            {
                Vector3 forward, up, right;
                up = Vector3.Up;

                float speed = velocity.Length();
                Vector3.Normalize(ref velocity, out forward);
                Vector3.Cross(ref up, ref forward, out right);
                Vector3.Normalize(ref right, out right);
                Vector3.Cross(ref forward, ref right, out up);

                forward = -forward * speed * 1000 * velocity_scale;

                world.Up = up;
                world.Right = forward;
                world.Forward = right;


                /*
                world.M11 = (((world.M11 * right.X) + (world.M12 * up.X)) + (world.M13 * forward.X));
                world.M12 = (((world.M11 * right.Y) + (world.M12 * up.Y)) + (world.M13 * forward.Y));
                world.M13 = (((world.M11 * right.Z) + (world.M12 * up.Z)) + (world.M13 * forward.Z));
                world.M21 = (((world.M21 * right.X) + (world.M22 * up.X)) + (world.M23 * forward.X));
                world.M22 = (((world.M21 * right.Y) + (world.M22 * up.Y)) + (world.M23 * forward.Y));
                world.M23 = (((world.M21 * right.Z) + (world.M22 * up.Z)) + (world.M23 * forward.Z));
                world.M31 = (((world.M31 * right.X) + (world.M32 * up.X)) + (world.M33 * forward.X));
                world.M32 = (((world.M31 * right.Y) + (world.M32 * up.Y)) + (world.M33 * forward.Y));
                world.M33 = (((world.M31 * right.Z) + (world.M32 * up.Z)) + (world.M33 * forward.Z));
                world.M41 = (((world.M41 * right.X) + (world.M42 * up.X)) + (world.M43 * forward.X));
                world.M42 = (((world.M41 * right.Y) + (world.M42 * up.Y)) + (world.M43 * forward.Y));
                world.M43 = (((world.M41 * right.Z) + (world.M42 * up.Z)) + (world.M43 * forward.Z));
                */
            }
        }

        public void Update(double elapsed)
        {
            if (!is_alive)
                return;

            age += (float)elapsed;

            if (is_alive && age > life)
                Destroy();

            logic.Frame(this, elapsed);

            position.X += velocity.X;
            position.Y += velocity.Y;
            position.Z += velocity.Z;

            velocity.X *= 0.99f;
            velocity.Y *= 0.99f;
            velocity.Z *= 0.99f;

            velocity.X += 0.00005f;
        }

        public void Destroy()
        {
            logic.Death(this);
            is_alive = false;
        }
    }
}
