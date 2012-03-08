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

namespace hyades.graphics.particle
{

    public class ParticleManager
    {
        private static ParticleManager instance;
        public static ParticleManager GetInstance()
        {
            if (instance == null)
                instance = new ParticleManager();

            return instance;
        }

        private const int NUM_PARTICLES = 10000;
        public Particle[] particle_array;
        public Queue<Particle> particle_pool;

        public ParticleManager()
        {
            particle_array = new Particle[NUM_PARTICLES];
            particle_pool = new Queue<Particle>(NUM_PARTICLES);

            for (int i = 0; i < particle_array.Length; i++)
            {
                Particle particle = new Particle();
                particle_array[i] = particle;
                particle_pool.Enqueue(particle);
                particle.in_pool = true;
                particle.is_alive = false;
            }
        }

        public void Update(double elapsed)
        {
            for (int i = 0; i < particle_array.Length; i++)
            {
                Particle particle = particle_array[i];

                if (particle.is_alive)
                    particle.Update(elapsed);
                else if (!particle.in_pool)
                {
                    particle_pool.Enqueue(particle);
                    particle.in_pool = true;
                }
            }
        }

        public void Draw(GraphicsDevice device, Camera camera)
        {
            ParticleRenderer particlerenderer = ParticleRenderer.GetInstance(device);
            particlerenderer.Begin(camera);

            for (int i = 0; i < particle_array.Length; i++)
            {
                Particle particle = particle_array[i];

                if (particle.is_alive)
                {
                    particle.UpdateWorldMatrix(camera);
                    particlerenderer.Add(particle);
                }
            }

            particlerenderer.End();
        }

        public Particle Emit(IParticleLogic logic, float life)
        {
            if (particle_pool.Count == 0)
                return null;

            Particle particle = particle_pool.Dequeue();
            particle.in_pool = false;
            particle.Create(logic, life);

            return particle;
        }

        public void Explode(Vector3 position, float radius)
        {
            Interact(position, radius, true);
        }

        public void Implode(Vector3 position, float radius)
        {
            Interact(position, radius, false);
        }

        private void Interact(Vector3 position, float radius, bool explode)
        {
            for (int i = 0; i < particle_array.Length; i++)
            {
                Particle particle = particle_array[i];

                if (particle.is_alive)
                {
                    Vector3 direction;
                    if(explode)
                        direction = particle.position - position;
                    else
                        direction = position - particle.position;

                    float distance = direction.Length();

                    if (distance < radius)
                    {
                        direction.Normalize();
                        particle.velocity += direction * 0.1f * (1 - distance / radius);
                    }
                }
            }
        }
    }


}
