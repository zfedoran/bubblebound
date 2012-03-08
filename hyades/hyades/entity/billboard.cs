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
using hyades.graphics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace hyades.entity
{
    public class Billboard : Entity
    {
        public TextureRegion region;
        public Color color;
        public float age;
        public float alpha;
        public bool fade;

        private static Random rand = new Random();

        public Billboard(Texture2D texture)
            : this(new TextureRegion(texture, 0, 0, texture.Width, texture.Height))
        {}

        public Billboard(TextureRegion region)
        { 
            this.region = region;
            float max = MathHelper.Max(region.width, region.height);
            this.size = new Vector3(region.width / max, region.height / max, 0); 
            this.color = Color.White;

            this.age = 0;
            this.fade = false;
        }

        public override void Update(double elapsed)
        {
            if (fade)
            {
                age += (float)(elapsed);

                float alpha = (float)(Math.Sin(age * 0.9f) + Math.Cos(age * 0.1f) + Math.Sin(age * 0.05f)) * 0.2f;
                alpha = MathHelper.Clamp(alpha, 0, 1);

                color.A = (byte)(alpha * byte.MaxValue);
            }
        }

        public override void Draw(GraphicsDevice device, Camera camera)
        {
            SpriteRenderer spriterenderer = SpriteRenderer.GetInstance(device);

            spriterenderer.Begin(camera);
            spriterenderer.Add(region, color, position.X, position.Y, position.Z, size.X, size.Y, rotation.X, rotation.Y, rotation.Z);
            spriterenderer.End();

        }

    }
}
