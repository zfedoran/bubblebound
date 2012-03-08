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
using hyades.graphics.prefab;
using hyades.screen;

namespace hyades.entity
{
    public class Box : Entity
    {
        private BasicEffect material;
        public PrefabCube mesh;
        public Matrix world;

        public Box(Color color)
        {
            mesh = new PrefabCube(color);
        }

        public override void Update(double elapsed)
        {
            Entity.CreateWorldTransformMatrix(ref position, ref rotation, ref size, out world);
        }

        public override void Draw(GraphicsDevice device, Camera camera)
        {
            if (material == null)
                Warm(device);

            material.Projection = camera.projection;
            material.View = camera.view;
            material.World = world;
            material.CurrentTechnique.Passes[0].Apply();

            mesh.Draw(device);
        }

        public void Warm(GraphicsDevice device)
        {
            material = new BasicEffect(device);
            material.EnableDefaultLighting();
            material.VertexColorEnabled = true;
            material.PreferPerPixelLighting = true;
        }
    }
}
