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

namespace hyades.graphics
{
    public enum Primitive { Line, Triangle, Quad };

    public class PrimitiveBatch : IDisposable
    {
        protected const int DEFAULT_CAPACITY = 500;
        protected static PrimitiveBatch instance;
        
        protected Primitive primitive_type;
        protected GraphicsDevice device;
        protected int primitive_size, curr_vertex, curr_index, count, capacity;
        protected bool hasBegun, draw_enabled, has_translation, has_matrix, has_texture_coords;
        protected Color color; 
        protected Vector2 texture_coords; 
        protected Matrix matrix;
        protected Vector3 translation;
        protected PrimitiveBatchVertex[] vertices;
#if HIDEF
        protected int[] indices;
#else
        protected short[] indices;
#endif

        public PrimitiveBatch(GraphicsDevice device)
        { this.capacity = DEFAULT_CAPACITY; this.device = device; this.draw_enabled = true; }

        public static PrimitiveBatch GetInstance(GraphicsDevice device)
        {
            if (instance == null)
                instance = new PrimitiveBatch(device);
            return instance; 
        }

        public void Begin(Primitive primitive)
        {
            if (hasBegun)
                throw new InvalidOperationException("End must be called before Begin can be called again.");

            hasBegun = true;
            has_texture_coords = false;
            has_translation = false;
            has_matrix = false;
            color = Color.White;
            curr_vertex = 0;
            curr_index = 0;
            count = 0;

            if (primitive == Primitive.Line)
                primitive_size = 2;
            if (primitive == Primitive.Triangle)
                primitive_size = 3;
            if(primitive == Primitive.Quad)
                primitive_size = 4;

            primitive_type = primitive;

            if (vertices == null || indices == null)
                Warm();
        }

        public void AddVertex(Vector2 position)
        { AddVertex(position.X, position.Y, 0, 0, 0); }

        public void AddVertex(Vector3 position)
        { AddVertex(position.X, position.Y, position.Z, 0, 0); }

        public void AddVertex(float x, float y, float z)
        { AddVertex(x, y, z, 0, 0); }

        public void AddVertex(float x, float y, float z, float u, float v)
        {
            if (!hasBegun)
                throw new InvalidOperationException("Begin must be called before AddVertex can be called.");

            bool newPrimitive = ((curr_vertex % primitive_size) == 0);
            bool hasRoom = curr_vertex < vertices.Length;

            if (newPrimitive && !hasRoom)
            {
                if (!draw_enabled)
                    Grow((int)(capacity * 0.10f) + primitive_size * 10);
                else
                    Flush();
            }

            if (newPrimitive)
            {
                count++;

                if (primitive_type == Primitive.Quad)
                {
#if HIDEF
                indices[curr_index++] = curr_vertex + 0;
                indices[curr_index++] = curr_vertex + 2;
                indices[curr_index++] = curr_vertex + 1;

                indices[curr_index++] = curr_vertex + 0;
                indices[curr_index++] = curr_vertex + 3;
                indices[curr_index++] = curr_vertex + 2;
#else
                    indices[curr_index++] = (short)(curr_vertex + 0);
                    indices[curr_index++] = (short)(curr_vertex + 2);
                    indices[curr_index++] = (short)(curr_vertex + 1);

                    indices[curr_index++] = (short)(curr_vertex + 0);
                    indices[curr_index++] = (short)(curr_vertex + 3);
                    indices[curr_index++] = (short)(curr_vertex + 2);
#endif
                }
            }

            if (has_matrix)
            {
                float a = (((x * matrix.M11) + (y * matrix.M21)) + (z * matrix.M31)) + matrix.M41;
                float b = (((x * matrix.M12) + (y * matrix.M22)) + (z * matrix.M32)) + matrix.M42;
                float c = (((x * matrix.M13) + (y * matrix.M23)) + (z * matrix.M33)) + matrix.M43;
                x = a;
                y = b;
                z = c;
            }

            vertices[curr_vertex].position.X = x;
            vertices[curr_vertex].position.Y = y;
            vertices[curr_vertex].position.Z = z;
            vertices[curr_vertex].texture.X = u;
            vertices[curr_vertex].texture.Y = v;
            vertices[curr_vertex].color = color;

            if (has_translation)
            {
                vertices[curr_vertex].position.X += translation.X;
                vertices[curr_vertex].position.Y += translation.Y;
                vertices[curr_vertex].position.Z += translation.Z;
            }

            if (has_texture_coords)
            {
                vertices[curr_vertex].texture.X += texture_coords.X;
                vertices[curr_vertex].texture.Y += texture_coords.Y;
            }

            curr_vertex++;
        }

        public void End()
        {
            if (!hasBegun)
                throw new InvalidOperationException("Begin must be called before End can be called.");

            if ((curr_vertex % primitive_size) != 0)
                throw new InvalidOperationException("End called before closing the primitive; Add " + (primitive_size - (curr_vertex % primitive_size)) + " more vertices.");

            Flush();
            hasBegun = false;
        }

        private void Flush()
        {
            if (!hasBegun)
                throw new InvalidOperationException("Begin must be called before Flush can be called.");

            if (curr_vertex == 0)
                return;

            if (draw_enabled)
            {
                if (primitive_type == Primitive.Line)
                    device.DrawUserPrimitives<PrimitiveBatchVertex>(PrimitiveType.LineList, vertices, 0, curr_vertex / 2);
                if (primitive_type == Primitive.Triangle)
                    device.DrawUserPrimitives<PrimitiveBatchVertex>(PrimitiveType.TriangleList, vertices, 0, curr_vertex / 3);
                if (primitive_type == Primitive.Quad)
                    device.DrawUserIndexedPrimitives<PrimitiveBatchVertex>(PrimitiveType.TriangleList, vertices, 0, curr_vertex, indices, 0, curr_index / 3);

                curr_vertex = 0;
                curr_index = 0;
                count = 0;
            }
        }

        private void Warm()
        {
            vertices = new PrimitiveBatchVertex[capacity * primitive_size];

            if (primitive_type == Primitive.Quad)
            {
#if HIDEF
                indices = new int[capacity * 6];
#else
                indices = new short[capacity * 6];
#endif
            }

        }

        private void Grow(int amount)
        {
            capacity += amount;

            PrimitiveBatchVertex[] tmp_vertices = new PrimitiveBatchVertex[capacity * primitive_size];

            for (int i = 0; i < vertices.Length; i++)
                tmp_vertices[i] = vertices[i];
            vertices = tmp_vertices;

            if (primitive_type == Primitive.Quad)
            {
#if HIDEF
                int[] tmp_indices = new int[capacity * 6];
#else
                short[] tmp_indices = new short[capacity * 6];
#endif
                for (int i = 0; i < indices.Length; i++)
                    tmp_indices[i] = indices[i];
                indices = tmp_indices;
            }
        }

        public void Dispose()
        {
            this.indices = null;
            this.vertices = null;
        }

        public void ClearColor()
        { this.color = Color.White; }

        public void ClearTranslation()
        { this.has_translation = false; }

        public void ClearTextureCoords()
        { this.has_texture_coords = false; }

        public void ClearTransform()
        { this.has_matrix = false; }

        public void SetDrawEnabled(bool enabled)
        { this.draw_enabled = enabled; }

        public void SetColor(Color color)
        {
            if (!hasBegun)
                throw new InvalidOperationException("Begin must be called before SetColor can be called.");

            this.color = color;
        }

        public void SetColor(float r, float g, float b, float a)
        {
            if (!hasBegun)
                throw new InvalidOperationException("Begin must be called before SetColor can be called.");

            this.color = new Color(r, g, b, a);
        }

        public void SetTranslation(float x, float y, float z)
        {
            if (!hasBegun)
                throw new InvalidOperationException("Begin must be called before SetTranslation can be called.");

            this.translation = new Vector3(x, y, z);
            this.has_translation = true;
        }

        public void SetTextureCoords(float u, float v)
        {
            if (!hasBegun)
                throw new InvalidOperationException("Begin must be called before SetTranslation can be called.");

            this.texture_coords = new Vector2(u, v);
            this.has_texture_coords = true;
        }

        public void SetTransform(ref Matrix matrix)
        {
            if (!hasBegun)
                throw new InvalidOperationException("Begin must be called before SetTranslation can be called.");

            this.matrix = matrix;
            this.has_matrix = true;
            this.has_translation = false;
        }

        public PrimitiveBatchVertex[] GetVertices()
        { return vertices; }

#if HIDEF
        public int[] GetIndices()
#else
        public short[] GetIndices()
#endif
        { return indices; }
    }
}
