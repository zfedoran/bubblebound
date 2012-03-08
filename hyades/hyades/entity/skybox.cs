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
using hyades.physics;
using hyades.graphics;

namespace hyades.entity
{
    public class Skybox : Entity, IHasModel
    {
        public Entity player;
        public Model model;
        public Color color;

        public Skybox(Entity player)
        {
            this.player = player;
            this.model = Resources.skybox_model;
            this.size = new Vector3(4.5f);
            this.rotation.X = MathHelper.PiOver2;
            this.color = Color.White;
        }

        public override void Update(double elapsed)
        {
           position.X = MathHelper.Lerp(position.X, player.position.X, 0.01f);
           position.Y = MathHelper.Lerp(position.Y, player.position.Y, 0.01f);
        }

        public override void Draw(GraphicsDevice device, Camera camera)
        {
            ModelRenderer modelrenderer = ModelRenderer.GetInstance(device);
            modelrenderer.Begin(camera, color);

            Matrix transform;
            Entity.CreateWorldTransformMatrix(ref position, ref rotation, ref size, out transform);
            modelrenderer.Add(model, ref transform);
            modelrenderer.End();
        }

        public Model GetModel()
        { return model; }
    }
}
