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
 

    public class Bubble : Entity
    {
        public PressureBody body;
        public Shape shape;

        private List<Vector2> vertices;
        private List<Vector2> texture_coords;
        private Effect material;

        public static float[] k;
        public static float[] pressure;
        public float scale;
        public int curr_size;
        public float alpha;
        public float max_speed;
        public bool popped;

        static Bubble()
        {
            k = new float[] { 1300, 400, 150, 50 };
            pressure = new float[] { 1, 15, 20, 25 };
        }

        public Bubble(float scale)
        {
            this.scale = scale;
            this.curr_size = k.Length - 1;
            this.alpha = 1f;
            this.max_speed = 1.5f;

            List<Vector2> circle = new List<Vector2>();
            for (int i = 0; i < 360; i += 40)
                circle.Add(new Vector2((float)Math.Cos(MathHelper.ToRadians((float)-i)) * (scale + 0.005f), (float)Math.Sin(MathHelper.ToRadians((float)-i)) * (scale + 0.005f)));

            shape = new Shape(circle.ToArray(), true);
            body = new PressureBody(shape, 1, pressure[curr_size], k[curr_size], 20.0f, k[curr_size], 20.0f);

            vertices = circle;
            vertices.Add(Vector2.Zero);

            texture_coords = new List<Vector2>();
            for (int i = 0; i < circle.Count-1; i++)
            {
                Vector2 point = circle[i];
                if (point.X + point.Y != 0)
                {
                    point.Normalize();
                    point *= 1.025f;
                    point.X = (point.X + 1) * 0.5f;
                    point.Y = (point.Y + 1) * 0.5f;
                }
                
                texture_coords.Add(point);
            }
            texture_coords.Add(new Vector2(0.5f, 0.5f));

            SetSize(0);

            material = Resources.spriterenderer_material;
        }

        
        public override void Update(double elapsed)
        {
            if (popped)
                return;

            //set position [physics to entity]
            position.X = body.position.X;
            position.Y = body.position.Y;
            
            //apply force [entity to physics]
            //body.velocity.X += (float)(velocity.X * elapsed) * 3;
            //body.velocity.Y += (float)(velocity.Y * elapsed) * 3;

            //limit speed to max_speed
            velocity.X = body.velocity.X;
            velocity.Y = body.velocity.Y;
            velocity.Z = 0;

            float speed = body.velocity.Length();
            speed = MathHelper.Clamp(speed, -max_speed, max_speed);
            if (speed != 0)
                velocity.Normalize();

            velocity.X = speed * velocity.X;
            velocity.Y = speed * velocity.Y;

            body.velocity.X = velocity.X;
            body.velocity.Y = velocity.Y;
        }

        public override void Draw(GraphicsDevice device, Camera camera)
        {
            if (popped)
                return;

            material.Parameters["TextureEnabled"].SetValue(true);
            material.Parameters["World"].SetValue(Matrix.Identity);
            material.Parameters["View"].SetValue(camera.view);
            material.Parameters["Projection"].SetValue(camera.projection);
            material.Parameters["Texture"].SetValue(Resources.bubble_texture);
            material.CurrentTechnique.Passes[0].Apply();

            PrimitiveBatch primitivebatch = PrimitiveBatch.GetInstance(device);

            primitivebatch.Begin(Primitive.Triangle);
            primitivebatch.SetColor(new Color(1, 1, 1, alpha));

            for (int i = 0; i < vertices.Count - 1; i++)
                vertices[i] = body.pointmass_list[i].position;
            vertices[vertices.Count - 1] = body.position;

            int center = vertices.Count-1;
            for (short i = 0; i < center; i++)
            {
                primitivebatch.AddVertex(vertices[center].X, vertices[center].Y, 0, texture_coords[center].X, texture_coords[center].Y);
                primitivebatch.AddVertex(vertices[i].X, vertices[i].Y, 0, texture_coords[i].X, texture_coords[i].Y);
                primitivebatch.AddVertex(vertices[i+1].X, vertices[i+1].Y, 0, texture_coords[i+1].X, texture_coords[i+1].Y);
            }
            primitivebatch.AddVertex(vertices[center].X, vertices[center].Y, 0, texture_coords[center].X, texture_coords[center].Y);
            primitivebatch.AddVertex(vertices[center - 1].X, vertices[center - 1].Y, 0, texture_coords[center - 1].X, texture_coords[center - 1].Y);
            primitivebatch.AddVertex(vertices[0].X, vertices[0].Y, 0, texture_coords[0].X, texture_coords[0].Y);
            primitivebatch.End();

            //PhysicsRenderer.GetInstance(device).Draw(body, camera);
        }

        public void DrawReflection(GraphicsDevice device, Camera camera)
        {
            if (popped)
                return;

            float size = (body.aabb.max - body.aabb.min).Length() * 0.7f;
            SpriteRenderer spriterenderer = SpriteRenderer.GetInstance(device);
            spriterenderer.Begin(camera);
            spriterenderer.Add(Resources.bubble_reflection_texture_region, new Color(1f, 1f, 1f, 0.8f), body.aabb.min.X + size * 0.09f, body.aabb.min.Y, 0.001f, size, size, 0, 0, 0);
            spriterenderer.End();
        }

        public void SetPosition(Vector2 position)
        {
            body.position = position;
        }

        public void SetVelocity(Vector2 velocity)
        {
            body.velocity = velocity;
        }

        public void SetSize(int size)
        {
            curr_size = size;
            if (curr_size >= k.Length)
                curr_size = k.Length - 1;
            if (curr_size < 0)
                curr_size = 0;

            body.pressure = pressure[curr_size];
            body.shape_k = k[curr_size];
            body.edge_k = k[curr_size];
        }

        public void IncreaseSize()
        {
            curr_size++;
            if (curr_size >= k.Length)
                curr_size = k.Length - 1;

            body.pressure = pressure[curr_size];
            body.shape_k = k[curr_size];
            body.edge_k = k[curr_size];
        }

        public void DecreaseSize()
        {
            curr_size--;
            if (curr_size < 0)
                curr_size = 0;

            body.pressure = pressure[curr_size];
            body.shape_k = k[curr_size];
            body.edge_k = k[curr_size];
        }
        
    }
}
