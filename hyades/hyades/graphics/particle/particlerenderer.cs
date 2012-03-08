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
using Microsoft.Xna.Framework.Graphics;
using hyades.screen;

namespace hyades.graphics.particle
{
    public struct ParticleInfo
    {
        public TextureRegion region;
        public Color color;
        public Matrix transform;

        public ParticleInfo(TextureRegion region, Color color, ref Matrix transform)
        { this.region = region; this.color = color; this.transform = transform; }
    }

    public class ParticleRenderer
    {
        private static ParticleRenderer instance;

        public static ParticleRenderer GetInstance(GraphicsDevice device)
        {
            if (instance == null)
                instance = new ParticleRenderer(device);
            return instance;
        }

        private Effect material;
        private Camera camera;
        private GraphicsDevice device;
        private PrimitiveBatch primitivebatch;
        private Dictionary<Texture2D, List<ParticleInfo>> particles;
        private bool has_begun, is_ortho;

        public ParticleRenderer(GraphicsDevice device)
        { this.particles = new Dictionary<Texture2D, List<ParticleInfo>>(); this.device = device; }

        private void Warm(GraphicsDevice device)
        {
            primitivebatch = new PrimitiveBatch(device);
            material = Resources.spriterenderer_material;
        }

        public void Add(Particle particle)
        {
            TextureRegion region = particle.region;
            ParticleInfo particleinfo = new ParticleInfo(region, particle.color, ref particle.world);

            if (!particles.ContainsKey(region.texture))
                particles[region.texture] = new List<ParticleInfo>();
            particles[region.texture].Add(particleinfo);
        }

        public void Begin(Camera camera)
        {
            if (has_begun)
                throw new InvalidOperationException("End must be called before Begin can be called again.");

            this.camera = camera;
            this.has_begun = true;
        }

        public void End()
        {
            if (!has_begun)
                throw new InvalidOperationException("Begin must be called before End can be called.");

            if (primitivebatch == null || material == null)
                Warm(device);

            material.Parameters["TextureEnabled"].SetValue(true);
            material.Parameters["World"].SetValue(Matrix.Identity);
            material.Parameters["View"].SetValue(camera.view);
            material.Parameters["Projection"].SetValue(camera.projection);

            foreach (Texture2D texture in particles.Keys)
            {
                List<ParticleInfo> list = particles[texture];

                if(list.Count <= 0)
                    continue;

                material.Parameters["Texture"].SetValue(texture);
                material.CurrentTechnique.Passes[0].Apply();

                primitivebatch.Begin(Primitive.Quad);
                float texture_pixel_width = 1.0f / texture.Width;
                float texture_pixel_height = 1.0f / texture.Height;

                for (int i = 0; i < list.Count; i++)
                {
                    ParticleInfo particle = list[i];
                    TextureRegion region = particle.region;

                    primitivebatch.SetTransform(ref particle.transform);
                    primitivebatch.SetColor(particle.color);
                    primitivebatch.SetTextureCoords(region.u * texture_pixel_width, region.v * texture_pixel_height);

                    primitivebatch.AddVertex(-0.5f, 0.5f, 0, 0, 0);
                    primitivebatch.AddVertex(-0.5f, -0.5f, 0, 0, region.height * texture_pixel_height);
                    primitivebatch.AddVertex(0.5f, -0.5f, 0, region.width * texture_pixel_width, region.height * texture_pixel_height);
                    primitivebatch.AddVertex(0.5f, 0.5f, 0, region.width * texture_pixel_width, 0);
                    
                }
                
                primitivebatch.End();

                list.Clear();
            }

            has_begun = false;
        }

    }
}