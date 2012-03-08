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

namespace hyades.graphics.prefab
{
    public class PrefabMesh
    {
        public string name; 
        public PrefabVertex[] vertices;

#if HIDEF
        public int[] indices;
#else
        public short[] indices;
#endif

        public VertexBuffer vertexbuffer;
        public IndexBuffer indexbuffer;

        public void Warm(GraphicsDevice device)
        {
            vertexbuffer = new VertexBuffer(device, typeof(PrefabVertex), vertices.Length, BufferUsage.None);
            vertexbuffer.SetData<PrefabVertex>(vertices);

#if HIDEF
            indexbuffer = new IndexBuffer(device, typeof(int), indices.Length, BufferUsage.None);
            indexbuffer.SetData<int>(indices);
#else
            indexbuffer = new IndexBuffer(device, typeof(short), indices.Length, BufferUsage.None);
            indexbuffer.SetData<short>(indices);
#endif
        }

        public void Draw(GraphicsDevice device)
        {
            if (indices.Length == 0)
                return;

            if (indexbuffer == null)
                Warm(device);

            device.SetVertexBuffer(vertexbuffer);
            device.Indices = indexbuffer;
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertices.Length, 0, indices.Length / 3);
        }

        public static PrefabMesh operator +(PrefabMesh m1, PrefabMesh m2)
        {
            PrefabMesh new_mesh = new PrefabMesh();

            new_mesh.name = m1.name;
            new_mesh.vertices = new PrefabVertex[m1.vertices.Length + m2.vertices.Length];

#if HIDEF
            new_mesh.indices = new int[m1.indices.Length + m2.indices.Length];
#else
            new_mesh.indices = new short[m1.indices.Length + m2.indices.Length];
#endif

            for (int i = 0; i < m1.vertices.Length; i++)
                new_mesh.vertices[i] = m1.vertices[i];
            for (int i = 0; i < m1.indices.Length; i++)
                new_mesh.indices[i] = m1.indices[i];

            for (int i = 0; i < m2.vertices.Length; i++)
                new_mesh.vertices[i + m1.vertices.Length] = m2.vertices[i];
            for (int i = 0; i < m2.indices.Length; i++)
#if HIDEF
                new_mesh.indices[i + m1.indices.Length] = m2.indices[i] + m1.vertices.Length;
#else
                new_mesh.indices[i + m1.indices.Length] = (short)(m2.indices[i] + m1.vertices.Length);
#endif
            return new_mesh;
        }
    }
}
