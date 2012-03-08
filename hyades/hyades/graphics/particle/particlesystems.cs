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
using hyades.entity;

namespace hyades.graphics.particle
{
    public class AmbientParticleLogic : IParticleLogic
    {
        private static Random rand = new Random();

        private Character entity;
        private float life;
        private float[] radius;
        private float[] scale;

        public AmbientParticleLogic(Character entity, float life)
        {
            this.entity = entity;
            this.life = life;

            this.radius = new float[] { 15, 18, 20, 25 };
            this.scale = new float[] { 0.5f, 0.8f, 1f, 1.4f };
        }

        public void Birth(Particle particle)
        {
            particle.position.X = (float)(rand.NextDouble() - 0.5f) * radius[entity.curr_size];
            particle.position.Y = (float)(rand.NextDouble() - 0.5f) * radius[entity.curr_size];
            particle.position.Z = (float)(rand.NextDouble() - 0.5f) * radius[entity.curr_size] * 0.5f;
            particle.position.X += entity.position.X;
            particle.position.Y += entity.position.Y;
            particle.position.Z += entity.position.Z;

            particle.region = Resources.particle_texture_region;
            particle.scale = new Vector2(particle.region.width * (1f / particle.region.texture.Width), particle.region.height * (1f / particle.region.texture.Height));
            particle.scale *= (float)(rand.NextDouble()) + scale[entity.curr_size];
        }

        public void Frame(Particle particle, double elapsed)
        {
            float alpha = particle.age / particle.life;
            alpha = (float)Math.Sin(alpha*MathHelper.Pi);
            particle.color.A = (byte)(byte.MaxValue * alpha);

            particle.velocity.X = MathHelper.Lerp(particle.velocity.X, (float)(rand.NextDouble() - 0.5f), 0.001f);
            particle.velocity.Y = MathHelper.Lerp(particle.velocity.Y, (float)(rand.NextDouble() - 0.5f), 0.001f);
            particle.velocity.Z = MathHelper.Lerp(particle.velocity.Z, (float)(rand.NextDouble() - 0.5f), 0.001f);

            float distance;
            Vector3.Distance(ref particle.position, ref entity.position, out distance);
            if (distance < 1)
            {
                float amount = 0.0002f;
                particle.velocity.X = MathHelper.Lerp(particle.velocity.X, entity.velocity.X, amount);
                particle.velocity.Y = MathHelper.Lerp(particle.velocity.Y, entity.velocity.Y, amount);
            }
        }

        public void Death(Particle particle)
        {
            Emit();
        }

        public void Emit()
        {
            ParticleManager particlemanager = ParticleManager.GetInstance();
            particlemanager.Emit(this, (float)rand.NextDouble() * life * 0.5f + life * 0.5f);
        }
    }


    public class AmbientCircleParticleLogic : IParticleLogic
    {
        private static Random rand = new Random();

        private Character entity;
        private float life;
        private float[] radius;
        private float[] scale;

        public AmbientCircleParticleLogic(Character entity, float life)
        {
            this.entity = entity;
            this.life = life;

            this.radius = new float[] { 15, 18, 20, 25 };
            this.scale = new float[] { 10.5f, 12.5f, 15.5f, 20.5f };
        }

        public void Birth(Particle particle)
        {
            particle.position.X = (float)(rand.NextDouble() - 0.5f) * radius[entity.curr_size];
            particle.position.Y = (float)(rand.NextDouble() - 0.5f) * radius[entity.curr_size];
            particle.position.Z = (float)(rand.NextDouble() - 0.5f) * radius[entity.curr_size] * 0.5f;
            particle.position.X += entity.position.X;
            particle.position.Y += entity.position.Y;
            particle.position.Z += entity.position.Z;

            particle.region = Resources.particle_texture_region;
            particle.scale = new Vector2(particle.region.width * (1f / particle.region.texture.Width), particle.region.height * (1f / particle.region.texture.Height));
            particle.scale *= (float)(rand.NextDouble()) * scale[entity.curr_size];
        }

        public void Frame(Particle particle, double elapsed)
        {
            float alpha = particle.age / particle.life;
            alpha = (float)Math.Sin(alpha * MathHelper.Pi) * 0.15f;
            particle.color.A = (byte)(byte.MaxValue * alpha);

            particle.velocity.X = MathHelper.Lerp(particle.velocity.X, (float)(rand.NextDouble() - 0.5f), 0.001f);
            particle.velocity.Y = MathHelper.Lerp(particle.velocity.Y, (float)(rand.NextDouble() - 0.5f), 0.001f);
            particle.velocity.Z = MathHelper.Lerp(particle.velocity.Z, (float)(rand.NextDouble() - 0.5f), 0.001f);
        }

        public void Death(Particle particle)
        {
            Emit();
        }

        public void Emit()
        {
            ParticleManager particlemanager = ParticleManager.GetInstance();
            particlemanager.Emit(this, (float)rand.NextDouble() * life);
        }
    }


    public class SimpleParticleLogic : IParticleLogic
    {
        private static Random rand = new Random();

        public void Birth(Particle particle)
        {
            particle.region = Resources.particle_texture_region;
            particle.scale = new Vector2(particle.region.width * (1f / particle.region.texture.Width), particle.region.height * (1f / particle.region.texture.Height));
        }

        public void Frame(Particle particle, double elapsed)
        {
            float alpha = 1 - particle.age / particle.life;
            particle.color.A = (byte)(byte.MaxValue * alpha);
        }

        public void Death(Particle particle)
        {}

    }

    public class WormParticleLogic : IParticleLogic
    {
        private static Random rand = new Random();

        private SimpleParticleLogic tail_logic;
        private Character entity;
        private float life;
        private float time;

        private float[] radius;
        private float[] scale;

        public WormParticleLogic(Character entity, float life)
        {
            this.entity = entity;
            this.life = life;
            this.tail_logic = new SimpleParticleLogic();

            this.radius = new float[] { 15, 18, 20, 25 };
            this.scale = new float[] { 1.5f, 1.8f, 2.1f, 2.4f };
        }

        public void Birth(Particle particle)
        {
            particle.position.X = (float)(rand.NextDouble() - 0.5f) * radius[entity.curr_size];
            particle.position.Y = (float)(rand.NextDouble() - 0.5f) * radius[entity.curr_size];
            particle.position.Z = (float)(rand.NextDouble() - 0.5f) * radius[entity.curr_size] * 0.5f;
            particle.position.X += entity.position.X;
            particle.position.Y += entity.position.Y;
            particle.position.Z += entity.position.Z;

            particle.region = Resources.particle_texture_region;
            particle.scale = new Vector2(particle.region.width * (1f / particle.region.texture.Width), particle.region.height * (1f / particle.region.texture.Height));
            particle.scale *= (float)(rand.NextDouble()) + scale[entity.curr_size];
        }

        public void Frame(Particle particle, double elapsed)
        {
            time += (float)elapsed;
            if (time > 0.1f)
            {
                Tail(particle);
                time = 0;
            }

            float alpha = particle.age / particle.life;
            alpha = (float)Math.Sin(alpha * MathHelper.Pi);
            particle.color.A = (byte)(byte.MaxValue * alpha);

            particle.velocity.X = MathHelper.Lerp(particle.velocity.X, (float)(rand.NextDouble() - 0.5f), 0.003f);
            particle.velocity.Y = MathHelper.Lerp(particle.velocity.Y, (float)(rand.NextDouble() - 0.5f), 0.003f);
            particle.velocity.Z = MathHelper.Lerp(particle.velocity.Z, (float)(rand.NextDouble() - 0.5f), 0.003f);
        }

        public void Death(Particle particle)
        {
            Emit();
        }

        public void Emit()
        {
            ParticleManager particlemanager = ParticleManager.GetInstance();
            particlemanager.Emit(this, (float)rand.NextDouble() * life);
        }

        public void Tail(Particle parent)
        {
            ParticleManager particlemanager = ParticleManager.GetInstance();
            Particle child = particlemanager.Emit(tail_logic, (float)rand.NextDouble());

            if (child == null)
                return;

            child.scale = parent.scale;
            child.position = parent.position;
        }
    }


    public class BubbleTailLogic : IParticleLogic
    {
        private static Random rand = new Random();
        private Character entity;
        private int count;

        public float life;
        private float[] radius;
        private float[] scale;
        
        
        public BubbleTailLogic(Character entity, float life)
        {
            this.entity = entity;
            this.life = life;
            this.radius = new float[] { 0.01f, 0.1f, 1f, 1.5f };
            this.scale = new float[] { 0.05f, 0.07f, 0.09f, 0.11f };
        }

        public void Birth(Particle particle)
        {
            particle.position.X = (float)(rand.NextDouble() - 0.5f) * radius[entity.curr_size];
            particle.position.Y = (float)(rand.NextDouble() - 0.5f) * radius[entity.curr_size];
            particle.position.Z = -0.1f;
            particle.position.X += entity.position.X;
            particle.position.Y += entity.position.Y;
            particle.position.Z += entity.position.Z;

            particle.region = Resources.bubble_texture_region;
            particle.scale = new Vector2(particle.region.width * (1f / particle.region.texture.Width), particle.region.height * (1f / particle.region.texture.Height));
            particle.scale *= (float)(rand.NextDouble()) * scale[entity.curr_size];
            particle.color.A = 0;
            count++;
        }

        public void Frame(Particle particle, double elapsed)
        {
            float fade = 1 - particle.age / particle.life;
            fade = fade * 0.4f;
            particle.color.A = (byte)(byte.MaxValue * fade);

            particle.velocity.X = MathHelper.Lerp(particle.velocity.X, (float)(rand.NextDouble() - 0.5f), 0.002f);
            particle.velocity.Y = MathHelper.Lerp(particle.velocity.Y, (float)(rand.NextDouble() - 0.5f), 0.002f);
            particle.velocity.Z = MathHelper.Lerp(particle.velocity.Z, (float)(rand.NextDouble() - 0.5f), 0.002f);
        }

        public void Death(Particle particle)
        {
            count--;
        }

        public void Emit()
        {
            if (count > 50)
                return;

            ParticleManager particlemanager = ParticleManager.GetInstance();
            particlemanager.Emit(this, (float)rand.NextDouble() * life);
        }

    }


    public class HintBubblesLogic : IParticleLogic
    {
        private static Random rand = new Random();
        public Entity entity;

        public float life;
        private float radius;
        private float scale;
        private int count;

        public HintBubblesLogic(float life)
        {
            this.life = life;
            this.radius = 0.01f;
            this.scale = 0.04f;
        }

        public void Birth(Particle particle)
        {
            particle.position.X = (float)(rand.NextDouble() - 0.5f) * radius;
            particle.position.Y = (float)(rand.NextDouble() - 0.5f) * radius;
            particle.position.Z = (float)(rand.NextDouble() - 0.5f) * radius;
            particle.position.X += entity.position.X;
            particle.position.Y += entity.position.Y;
            particle.position.Z += entity.position.Z;

            particle.region = Resources.bubble_texture_region;
            particle.scale = new Vector2(particle.region.width * (1f / particle.region.texture.Width), particle.region.height * (1f / particle.region.texture.Height));
            particle.scale *= (float)(rand.NextDouble()) * scale;
            particle.color.A = 0;

            count++;
        }

        public void Frame(Particle particle, double elapsed)
        {
            float fade = 1 - particle.age / particle.life;
            fade = fade * 0.2f;
            particle.color.A = (byte)(byte.MaxValue * fade);

            particle.velocity.X = MathHelper.Lerp(particle.velocity.X, (float)(rand.NextDouble() - 0.5f), 0.0005f);
            particle.velocity.Y = MathHelper.Lerp(particle.velocity.Y, (float)(rand.NextDouble() - 0.5f), 0.0005f);
            particle.velocity.Z = MathHelper.Lerp(particle.velocity.Z, (float)(rand.NextDouble() - 0.5f), 0.0005f);
        }

        public void Death(Particle particle)
        {
            count--;
        }

        public void Emit()
        {
            if (count > 100)
                return;

            ParticleManager particlemanager = ParticleManager.GetInstance();
            particlemanager.Emit(this, (float)rand.NextDouble() * life);
        }
    }


    public class AmbientBubblesLogic : IParticleLogic
    {
        private static Random rand = new Random();
        private Character entity;

        public float life;
        private float[] radius;
        private float[] scale;

        public AmbientBubblesLogic(Character entity, int num, float life)
        {
            this.entity = entity;
            this.life = life;
            this.radius = new float[] { 0.3f, 0.6f, 1f, 1.5f };
            this.scale = new float[] { 0.05f, 0.07f, 0.09f, 0.11f };

            for (int i = 0; i < num; i++)
                Emit();
        }

        public void Birth(Particle particle)
        {
            particle.position.X = (float)(rand.NextDouble() - 0.5f) * radius[entity.curr_size];
            particle.position.Y = (float)(rand.NextDouble() - 0.5f) * radius[entity.curr_size];
            particle.position.Z = (float)(rand.NextDouble() - 0.5f) * radius[entity.curr_size];
            particle.position.X += entity.position.X;
            particle.position.Y += entity.position.Y;
            particle.position.Z += entity.position.Z;

            particle.region = Resources.bubble_texture_region;
            particle.scale = new Vector2(particle.region.width * (1f / particle.region.texture.Width), particle.region.height * (1f / particle.region.texture.Height));
            particle.scale *= (float)(rand.NextDouble()) * scale[entity.curr_size];
            particle.color.A = 0;
        }

        public void Frame(Particle particle, double elapsed)
        {
            float fade = 1 - particle.age / particle.life;
            fade = fade * 0.3f;
            particle.color.A = (byte)(byte.MaxValue * fade);

            particle.velocity.X = MathHelper.Lerp(particle.velocity.X, (float)(rand.NextDouble() - 0.5f), 0.0005f);
            particle.velocity.Y = MathHelper.Lerp(particle.velocity.Y, (float)(rand.NextDouble() - 0.5f), 0.0005f);
            particle.velocity.Z = MathHelper.Lerp(particle.velocity.Z, (float)(rand.NextDouble() - 0.5f), 0.0005f);
        }

        public void Death(Particle particle)
        {
            Emit();
        }

        public void Emit()
        {
            ParticleManager particlemanager = ParticleManager.GetInstance();
            particlemanager.Emit(this, (float)rand.NextDouble() * life);
        }

    }

    public class AmbientEntityBubblesLogic : IParticleLogic
    {
        private static Random rand = new Random();
        private Entity entity;

        public float life;
        private float[] radius;
        private float[] scale;

        public AmbientEntityBubblesLogic(Entity entity, int num, float life)
        {
            this.entity = entity;
            this.life = life;
            this.radius = new float[] { 0.3f, 0.6f, 1f, 1.5f };
            this.scale = new float[] { 0.05f, 0.07f, 0.09f, 0.11f };

            for (int i = 0; i < num; i++)
                Emit();
        }

        public void Birth(Particle particle)
        {
            particle.position.X = (float)(rand.NextDouble() - 0.5f) * radius[0];
            particle.position.Y = (float)(rand.NextDouble() - 0.5f) * radius[0];
            particle.position.Z = (float)(rand.NextDouble() - 0.5f) * radius[0];
            particle.position.X += entity.position.X;
            particle.position.Y += entity.position.Y;
            particle.position.Z += entity.position.Z;

            particle.region = Resources.bubble_texture_region;
            particle.scale = new Vector2(particle.region.width * (1f / particle.region.texture.Width), particle.region.height * (1f / particle.region.texture.Height));
            particle.scale *= (float)(rand.NextDouble()) * scale[0];
            particle.color.A = 0;
        }

        public void Frame(Particle particle, double elapsed)
        {
            float fade = 1 - particle.age / particle.life;
            fade = fade * 0.3f;
            particle.color.A = (byte)(byte.MaxValue * fade);

            particle.velocity.X = MathHelper.Lerp(particle.velocity.X, (float)(rand.NextDouble() - 0.5f), 0.0005f);
            particle.velocity.Y = MathHelper.Lerp(particle.velocity.Y, (float)(rand.NextDouble() - 0.5f), 0.0005f);
            particle.velocity.Z = MathHelper.Lerp(particle.velocity.Z, (float)(rand.NextDouble() - 0.5f), 0.0005f);
        }

        public void Death(Particle particle)
        {
            Emit();
        }

        public void Emit()
        {
            ParticleManager particlemanager = ParticleManager.GetInstance();
            particlemanager.Emit(this, (float)rand.NextDouble() * life);
        }

    }

    public class BoostLogic : IParticleLogic
    {
        private static Random rand = new Random();
        private Character entity;
        private int count;

        public float life;
        private float[] radius;
        private float[] scale;


        public BoostLogic(Character entity, float life)
        {
            this.entity = entity;
            this.life = life;
            this.radius = new float[] { 0.3f, 0.6f, 1f, 1.5f };
            this.scale = new float[] { 0.05f, 0.07f, 0.09f, 0.11f };
        }

        public void Birth(Particle particle)
        {
            particle.position.X = (float)(rand.NextDouble() - 0.5f) * radius[entity.curr_size];
            particle.position.Y = (float)(rand.NextDouble() - 0.5f) * radius[entity.curr_size];
            particle.position.Z = (float)(rand.NextDouble() - 0.5f) * radius[entity.curr_size];
            particle.position.X += entity.position.X;
            particle.position.Y += entity.position.Y;
            particle.position.Z += entity.position.Z;

            particle.region = Resources.bubble_texture_region;
            particle.scale = new Vector2(particle.region.width * (1f / particle.region.texture.Width), particle.region.height * (1f / particle.region.texture.Height));
            particle.scale *= (float)(rand.NextDouble()) * scale[entity.curr_size];
            particle.color.A = 0;
            count++;
        }

        public void Frame(Particle particle, double elapsed)
        {
            float fade = 1 - particle.age / particle.life;
            fade = fade * 0.5f;
            particle.color.A = (byte)(byte.MaxValue * fade);

            particle.velocity.X = MathHelper.Lerp(particle.velocity.X, (float)(rand.NextDouble() - 0.5f), 0.006f);
            particle.velocity.Y = MathHelper.Lerp(particle.velocity.Y, (float)(rand.NextDouble() - 0.5f), 0.006f);
            particle.velocity.Z = MathHelper.Lerp(particle.velocity.Z, (float)(rand.NextDouble() - 0.5f), 0.006f);
        }

        public void Death(Particle particle)
        {
            count--;
        }

        public void Emit()
        {
            if (count > 80)
                return;

            ParticleManager particlemanager = ParticleManager.GetInstance();
            particlemanager.Emit(this, (float)rand.NextDouble() * life);
        }

        public void Burst()
        {
            for (int i = 0; i < 40; i++)
            {
                Emit();
            }

        }

    }

    public class PopLogic : IParticleLogic
    {
        private static Random rand = new Random();
        private Character entity;
        private int count;

        public float life;
        private float[] radius;
        private float[] scale;


        public PopLogic(Character entity, float life)
        {
            this.entity = entity;
            this.life = life;
            this.radius = new float[] { 0.3f, 0.6f, 1f, 1.5f };
            this.scale = new float[] { 0.11f, 0.15f, 0.18f, 0.20f };
        }

        public void Birth(Particle particle)
        {
            particle.position.X = (float)(rand.NextDouble() - 0.5f) * radius[entity.curr_size];
            particle.position.Y = (float)(rand.NextDouble() - 0.5f) * radius[entity.curr_size];
            particle.position.Z = (float)(rand.NextDouble() - 0.5f) * radius[entity.curr_size];
            particle.position.X += entity.position.X;
            particle.position.Y += entity.position.Y;
            particle.position.Z += entity.position.Z;

            particle.region = Resources.bubble_texture_region;
            particle.scale = new Vector2(particle.region.width * (1f / particle.region.texture.Width), particle.region.height * (1f / particle.region.texture.Height));
            particle.scale *= (float)(rand.NextDouble()) * scale[entity.curr_size];
            particle.color.A = 0;
            count++;
        }

        public void Frame(Particle particle, double elapsed)
        {
            float fade = 1 - particle.age / particle.life;
            fade = fade * 0.5f;
            particle.color.A = (byte)(byte.MaxValue * fade);

            particle.velocity.X = MathHelper.Lerp(particle.velocity.X, (float)(rand.NextDouble() - 0.5f), 0.001f);
            particle.velocity.Y = MathHelper.Lerp(particle.velocity.Y, (float)(rand.NextDouble() - 0.5f), 0.001f);
            particle.velocity.Z = MathHelper.Lerp(particle.velocity.Z, (float)(rand.NextDouble() - 0.5f), 0.001f);
        }

        public void Death(Particle particle)
        {
            count--;
        }

        public void Emit()
        {
            if (count > 80)
                return;

            ParticleManager particlemanager = ParticleManager.GetInstance();
            particlemanager.Emit(this, (float)rand.NextDouble() * life);
        }

        public void Burst()
        {
            for (int i = 0; i < 40; i++)
            {
                Emit();
            }

        }

    }


    public class FinalPopLogic : IParticleLogic
    {
        private static Random rand = new Random();
        private Character entity;
        private int count;

        public float life;
        private float[] radius;
        private float[] scale;


        public FinalPopLogic(Character entity, float life)
        {
            this.entity = entity;
            this.life = life;
            this.radius = new float[] { 0.3f, 0.6f, 1f, 1.5f };
            this.scale = new float[] { 0.11f, 0.15f, 0.18f, 0.30f };
        }

        public void Birth(Particle particle)
        {
            particle.position.X = (float)(rand.NextDouble() - 0.5f);
            particle.position.Y = (float)(rand.NextDouble() - 0.5f);
            particle.position.Z = (float)(rand.NextDouble() - 0.5f);
            particle.position.Normalize();
            particle.position *= (float)(rand.NextDouble()) * radius[entity.curr_size];
            particle.position.X += entity.position.X;
            particle.position.Y += entity.position.Y;
            particle.position.Z += entity.position.Z;


            particle.region = Resources.bubble_texture_region;
            particle.scale = new Vector2(particle.region.width * (1f / particle.region.texture.Width), particle.region.height * (1f / particle.region.texture.Height));
            particle.scale *= (float)(rand.NextDouble()) * scale[entity.curr_size];
            particle.color.A = 0;
            count++;
        }

        public void Frame(Particle particle, double elapsed)
        {
            float fade = 1 - particle.age / particle.life;
            particle.color.A = (byte)(byte.MaxValue * fade*0.5f);

            particle.velocity.X = MathHelper.Lerp(particle.velocity.X, (float)(rand.NextDouble() - 0.5f), 0.002f);
            particle.velocity.Y = MathHelper.Lerp(particle.velocity.Y, (float)(rand.NextDouble() - 0.5f), 0.002f);
            particle.velocity.Z = MathHelper.Lerp(particle.velocity.Z, (float)(rand.NextDouble() - 0.5f), 0.002f);

            particle.scale *= 1.001f;
        }

        public void Death(Particle particle)
        {
            count--;
        }

        public void Emit()
        {
            if (count > 1000)
                return;

            ParticleManager particlemanager = ParticleManager.GetInstance();
            particlemanager.Emit(this, (float)rand.NextDouble() * life);
        }

        public void Burst()
        {
            for (int i = 0; i < 500; i++)
            {
                Emit();
            }

        }

    }

    public class HurtLogic : IParticleLogic
    {
        private static Random rand = new Random();
        private Character entity;
        private int count;

        public float life;
        private float[] radius;
        private float[] scale;


        public HurtLogic(Character entity, float life)
        {
            this.entity = entity;
            this.life = life;
            this.radius = new float[] { 0.3f, 0.6f, 1f, 1.5f };
            this.scale = new float[] { 0.11f, 0.15f, 0.18f, 0.20f };
        }

        public void Birth(Particle particle)
        {
            particle.position.X = (float)(rand.NextDouble() - 0.5f) * radius[entity.curr_size];
            particle.position.Y = (float)(rand.NextDouble() - 0.5f) * radius[entity.curr_size];
            particle.position.Z = (float)(rand.NextDouble() - 0.5f) * radius[entity.curr_size];
            particle.position.X += entity.position.X;
            particle.position.Y += entity.position.Y;
            particle.position.Z += entity.position.Z;

            particle.region = Resources.bubble_texture_region;
            particle.scale = new Vector2(particle.region.width * (1f / particle.region.texture.Width), particle.region.height * (1f / particle.region.texture.Height));
            particle.scale *= (float)(rand.NextDouble()) * scale[entity.curr_size];
            particle.color.A = 0;
            count++;
        }

        public void Frame(Particle particle, double elapsed)
        {
            float fade = 1 - particle.age / particle.life;
            fade = fade * 0.5f;
            particle.color.A = (byte)(byte.MaxValue * fade);

            particle.velocity.X = MathHelper.Lerp(particle.velocity.X, (float)(rand.NextDouble() - 0.5f), 0.006f);
            particle.velocity.Y = MathHelper.Lerp(particle.velocity.Y, (float)(rand.NextDouble() - 0.5f), 0.006f);
            particle.velocity.Z = MathHelper.Lerp(particle.velocity.Z, (float)(rand.NextDouble() - 0.5f), 0.006f);
        }

        public void Death(Particle particle)
        {
            count--;
        }

        public void Emit()
        {
            if (count > 80)
                return;

            ParticleManager particlemanager = ParticleManager.GetInstance();
            particlemanager.Emit(this, (float)rand.NextDouble() * life);
        }

        public void Burst()
        {
            for (int i = 0; i < 40; i++)
            {
                Emit();
            }

        }

    }
}
