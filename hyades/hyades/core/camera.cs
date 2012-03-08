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

namespace hyades
{
    public class Camera
    {
        public float fov, aspect, width, height, near, far;
        public Vector3 position, target, up;
        public Matrix view, projection, viewprojection;
        public BoundingFrustum boundingfrustum;

#if DEBUG
        public bool pauseBoundingFrustumUpdates;
#endif

        public Camera()
        {
            view = projection = viewprojection = Matrix.Identity;
            boundingfrustum = new BoundingFrustum(viewprojection);
        }

        public virtual void Update(double elapsed)
        {
            aspect = width / height;
            Matrix.CreateLookAt(ref position, ref target, ref up, out view);
            Matrix.CreatePerspectiveFieldOfView(fov, aspect, near, far, out projection);
            Matrix.Multiply(ref view, ref projection, out viewprojection);
#if DEBUG
            if (!pauseBoundingFrustumUpdates)
#endif
                boundingfrustum.Matrix = viewprojection;
        }

        public bool CullTest(ref BoundingBox aabb)
        {
            bool isVisible;
            boundingfrustum.Intersects(ref aabb, out isVisible);

            return !isVisible;
        }

        public Vector3 Project(Vector3 source)
        {
            Matrix matrix = Matrix.Multiply(view, projection);
            Vector3 vector = Vector3.Transform(source, matrix);
            float a = (((source.X * matrix.M14) + (source.Y * matrix.M24)) + (source.Z * matrix.M34)) + matrix.M44;

            if (!WithinEpsilon(a, 1f))
                vector = (Vector3)(vector / a);

            vector.X = (((vector.X + 1f) * 0.5f) * width);
            vector.Y = (((-vector.Y + 1f) * 0.5f) * height);
            vector.Z = (vector.Z * (near - far)) + near;
            return vector;
        }

        private bool WithinEpsilon(float a, float b)
        {
            float num = a - b;
            return ((-1.401298E-45f <= num) && (num <= float.Epsilon));
        }

        public static void CreateCameraMatrix(ref Vector3 rotation, ref Vector3 position, out Matrix matrix)
        {
            float a = (float)Math.Cos(rotation.X);
            float b = (float)Math.Sin(rotation.X);

            float c = (float)Math.Cos(rotation.Y);
            float d = (float)Math.Sin(rotation.Y);

            float e = (float)Math.Cos(rotation.Z);
            float f = (float)Math.Sin(rotation.Z);

            matrix = new Matrix();
            matrix.M11 = (c * e);
            matrix.M12 = (c * f);
            matrix.M13 = -d;
            matrix.M21 = (e * b * d - a * f);
            matrix.M22 = ((e * a) + (f * b * d));
            matrix.M23 = (b * c);
            matrix.M31 = (e * a * d + b * f);
            matrix.M32 = -(b * e - f * a * d);
            matrix.M33 = (a * c);
            matrix.M41 = position.X;
            matrix.M42 = position.Y;
            matrix.M43 = position.Z;
            matrix.M44 = 1;
        }

        public static void CreateCameraMatrix(ref Vector3 rotation, out Matrix matrix)
        {
            float a = (float)Math.Cos(rotation.X);
            float b = (float)Math.Sin(rotation.X);

            float c = (float)Math.Cos(rotation.Y);
            float d = (float)Math.Sin(rotation.Y);

            float e = (float)Math.Cos(rotation.Z);
            float f = (float)Math.Sin(rotation.Z);

            matrix = new Matrix();
            matrix.M11 = (c * e);
            matrix.M12 = (c * f);
            matrix.M13 = -d;
            matrix.M21 = (e * b * d - a * f);
            matrix.M22 = ((e * a) + (f * b * d));
            matrix.M23 = (b * c);
            matrix.M31 = (e * a * d + b * f);
            matrix.M32 = -(b * e - f * a * d);
            matrix.M33 = (a * c);
            matrix.M44 = 1;
        }
    }


    public class ThirdPersonCamera : Camera
    {
        public Entity entity;
        public Vector3 offset;
        public bool rotate;

        public ThirdPersonCamera(Entity e, Vector3 offset)
            : this(e, offset, true)
        { }

        public ThirdPersonCamera(Entity e, Vector3 offset, bool rotate)
        {
            this.entity = e;
            this.offset = offset;
            this.up = Vector3.Up;
            this.rotate = rotate;
        }

        public override void Update(double elapsed)
        {
            target = entity.position;

            float a, b;
            if (rotate)
            {
                // create rotation about y
                float cos = (float)Math.Cos(entity.rotation.Y);
                float sin = (float)Math.Sin(entity.rotation.Y);
                a = offset.X * cos + offset.Z * sin;
                b = offset.X * -sin + offset.Z * cos;
            }
            else
            {
                a = offset.X;
                b = offset.Z;
            }

            // add translation
            a += entity.position.X;
            b += entity.position.Z;

            // lerp
            this.position.X += ((a - this.position.X) * 0.1f);
            this.position.Z += ((b - this.position.Z) * 0.1f);
            this.position.Y = offset.Y;

            base.Update(elapsed);
        }
    }

    public class FirstPersonCamera : Camera
    {
        public Entity entity;
        public Matrix camera_matrix;

        public FirstPersonCamera(Entity e)
        { this.entity = e; }

        public override void Update(double elapsed)
        {
            CreateCameraMatrix(ref entity.rotation, ref entity.position, out camera_matrix);

            this.position = camera_matrix.Translation;
            this.target = camera_matrix.Translation + camera_matrix.Forward;
            this.up = camera_matrix.Up;

            base.Update(elapsed);
        }
    }
}
