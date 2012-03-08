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
    public class WallModelEntity : Entity, IHasModel
    {
        private Model model;
        public Shape shape;
        public Body body;
        public Vector2 offset;
        public Color color;

        public WallModelEntity(Model model)
        { this.model = model; this.size = Vector3.One; this.color = Color.White; }

        public override void Update(double elapsed)
        {
            body.position.X = position.X + offset.X;
            body.position.Y = position.Y + offset.Y;
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

        public void BuildWall(Plane p)
        {
            ShapeBuilder shapebuilder = new ShapeBuilder(model);

            Matrix transform = GetTransform();
            Vector2[] points = shapebuilder.GetShape(ref transform, p);

            shape = new Shape(points, false);
            offset = shape.GetCenter() - new Vector2(position.X, position.Y);
            shape.CenterAtZero();

            body = new Body(shape, float.MaxValue);
            body.position.X = position.X + offset.X;
            body.position.Y = position.Y + offset.Y;
            body.is_static = true;
            body.Update(0);
        }
    }
}
