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
using hyades.graphics;

namespace hyades.editor
{
    public class Cursor
    {
        private BasicEffect material;
        private Vector2[] points;
        private Matrix view, proj;
        public Color color;

        public Cursor(float scale)
        {
            List<Vector2> point_list = new List<Vector2>();
            for (int i = 0; i < 360; i += 20)
                point_list.Add(new Vector2((float)Math.Cos(MathHelper.ToRadians((float)-i)) * scale,
                                           (float)Math.Sin(MathHelper.ToRadians((float)-i)) * scale));

            points = point_list.ToArray();

        }

        public void Warm(GraphicsDevice device)
        {
            material = new BasicEffect(device);
            material.VertexColorEnabled = true;

            view = Matrix.Identity;
            proj = Matrix.CreateTranslation(-0.5f, -0.5f, 0) * Matrix.CreateOrthographic(device.Viewport.Width, device.Viewport.Height, 0, 1);
        }

        public void Draw(GraphicsDevice device)
        {
            if (material == null)
                Warm(device);

            material.Projection = proj;
            material.View = view;
            material.World = Matrix.Identity;
            material.CurrentTechnique.Passes[0].Apply();

            PrimitiveBatch instance = PrimitiveBatch.GetInstance(device);
            instance.Begin(Primitive.Line);

            instance.SetColor(color);
            for (int p = 1; p < points.Length; p++)
            {
                instance.AddVertex(points[p - 1]);
                instance.AddVertex(points[p - 0]);
            }

            instance.AddVertex(points[points.Length - 1]);
            instance.AddVertex(points[0]);

            instance.End();
        }
    }
}
