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

namespace hyades.entity
{
    public abstract class Entity
    {
        public BoundingBox aabb;
        public Vector3 size, position, velocity, rotation;
        public bool visible, removed;
        public readonly int id;
        public string name;
        private static int count;

        public virtual void Update(double elapsed) { }
        public virtual void Draw(GraphicsDevice device, Camera camera) { }

        public Entity()
        {
            visible = true;
            removed = false;
            id = count++;
            name = id.ToString();
        }

        public void Remove()
        {
            removed = true;
            count--;
        }

        public Matrix GetTransform()
        {
            Matrix transform;
            CreateWorldTransformMatrix(ref position, ref rotation, ref size, out transform);

            return transform;
        }

        public static void CreateWorldTransformMatrix(ref Vector3 position, ref Vector3 rotation, ref Vector3 size, out Matrix matrix)
        {
            float a = (float)Math.Cos(rotation.X);
            float b = (float)Math.Sin(rotation.X);

            float c = (float)Math.Cos(rotation.Y);
            float d = (float)Math.Sin(rotation.Y);

            float e = (float)Math.Cos(rotation.Z);
            float f = (float)Math.Sin(rotation.Z);

            matrix = Matrix.Identity;
            matrix.M11 = (c * e) * size.X;
            matrix.M12 = (c * f) * size.X;
            matrix.M13 = -d * size.X;
            matrix.M21 = (e * b * d - a * f) * size.Y;
            matrix.M22 = ((e * a) + (f * b * d)) * size.Y;
            matrix.M23 = (b * c) * size.Y;
            matrix.M31 = (e * a * d + b * f) * size.Z;
            matrix.M33 = (a * c) * size.Z;
            matrix.M32 = -(b * e - f * a * d) * size.Z;
            matrix.M41 = position.X;
            matrix.M42 = position.Y;
            matrix.M43 = position.Z;
        }

        public static void CopyFrom(Entity from, Entity to)
        {
            to.position = from.position;
            to.rotation = from.rotation;
            to.velocity = from.velocity;
            to.size = from.size;
            to.aabb = from.aabb;
            to.name = from.name;
            to.visible = from.visible;
        }

        public static Entity Clone(Entity entity)
        {
            Entity clone = null;

            if (entity is StaticModelEntity)
            {
                StaticModelEntity modelentity = entity as StaticModelEntity;
                clone = new StaticModelEntity(modelentity.GetModel());
            }

            if (entity is WallModelEntity)
            {
                WallModelEntity wallentity = entity as WallModelEntity;
                clone = new WallModelEntity(wallentity.GetModel());
            }

            if (entity is AnimatedModelEntity)
            {
                AnimatedModelEntity modelentity = entity as AnimatedModelEntity;
                clone = new AnimatedModelEntity(modelentity.GetModel());
            }

            if (entity is Billboard)
            {
                Billboard billboard = entity as Billboard;
                clone = new Billboard(billboard.region);
            }

            if (clone != null)
                CopyFrom(entity, clone);

            return clone;
        }

    }
}
