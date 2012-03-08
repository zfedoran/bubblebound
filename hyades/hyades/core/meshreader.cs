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

namespace hyades
{
    public struct Line
    {
        public int a, b;
    }

    public struct Triangle
    {
        public int a, b, c;
    }

    // MeshReader based on code posted on official XNA forum
    // http://forums.create.msdn.com/forums/p/47653/285915.aspx
    public class MeshReader
    {
        public static void ExtractTrianglesFrom(Model model, List<Vector3> vertices, List<Triangle> indices, Matrix world)
        {
            Matrix transform = Matrix.Identity;
            foreach (ModelMesh mesh in model.Meshes)
            {
                // If the model has bones the vertices have to be transformed by the bone position
                transform = Matrix.Multiply(GetAbsoluteTransform(mesh.ParentBone), world);
                ExtractModelMeshData(mesh, ref transform, vertices, indices);
            }
        }

        /// <summary>
        /// Transform by a bone position or Identity if no bone is supplied
        /// </summary>
        private static Matrix GetAbsoluteTransform(ModelBone bone)
        {
            if (bone == null)
            {
                return Matrix.Identity;
            }
            return bone.Transform * GetAbsoluteTransform(bone.Parent);
        }

        /// <summary>
        /// Get all the triangles from all mesh parts
        /// </summary>
        private static void ExtractModelMeshData(ModelMesh mesh, ref Matrix transform, List<Vector3> vertices, List<Triangle> indices)
        {
            foreach (ModelMeshPart meshPart in mesh.MeshParts)
            {
                ExtractModelMeshPartData(meshPart, ref transform, vertices, indices);
            }
        }

        /// <summary>
        /// Get all the triangles from each mesh part (Changed for XNA 4)
        /// </summary>
        private static void ExtractModelMeshPartData(ModelMeshPart meshPart, ref Matrix transform, List<Vector3> vertices, List<Triangle> indices)
        {
            // Before we add any more where are we starting from
            int offset = vertices.Count;

            // Vertices

            // Read the format of the vertex buffer
            VertexDeclaration declaration = meshPart.VertexBuffer.VertexDeclaration;
            VertexElement[] vertexElements = declaration.GetVertexElements();
            // Find the element that holds the position
            VertexElement vertexPosition = new VertexElement();
            foreach (VertexElement vert in vertexElements)
            {
                if (vert.VertexElementUsage == VertexElementUsage.Position &&
                    vert.VertexElementFormat == VertexElementFormat.Vector3)
                {
                    vertexPosition = vert;
                    // There should only be one
                    break;
                }
            }
            // Check the position element found is valid
            if (vertexPosition == null ||
                vertexPosition.VertexElementUsage != VertexElementUsage.Position ||
                vertexPosition.VertexElementFormat != VertexElementFormat.Vector3)
            {
                throw new Exception("Model uses unsupported vertex format!");
            }
            // This where we store the vertices until transformed
            Vector3[] allVertex = new Vector3[meshPart.NumVertices];
            // Read the vertices from the buffer in to the array
            meshPart.VertexBuffer.GetData<Vector3>(
                meshPart.VertexOffset * declaration.VertexStride + vertexPosition.Offset,
                allVertex,
                0,
                meshPart.NumVertices,
                declaration.VertexStride);
            // Transform them based on the relative bone location and the world if provided
            for (int i = 0; i != allVertex.Length; ++i)
            {
                Vector3.Transform(ref allVertex[i], ref transform, out allVertex[i]);
            }
            // Store the transformed vertices with those from all the other meshes in this model
            vertices.AddRange(allVertex);

            // Indices 

            // Find out which vertices make up which triangles
            if (meshPart.IndexBuffer.IndexElementSize != IndexElementSize.SixteenBits)
            {
                // This could probably be handled by using int in place of short but is unnecessary
                throw new Exception("Model uses 32-bit indices, which are not supported.");
            }
            // Each primitive is a triangle
            short[] indexElements = new short[meshPart.PrimitiveCount * 3];
            meshPart.IndexBuffer.GetData<short>(
                meshPart.StartIndex * 2,
                indexElements,
                0,
                meshPart.PrimitiveCount * 3);
            // Each TriangleVertexIndices holds the three indexes to each vertex that makes up a triangle
            Triangle[] tvi = new Triangle[meshPart.PrimitiveCount];
            for (int i = 0; i != tvi.Length; ++i)
            {
                // The offset is because we are storing them all in the one array and the 
                // vertices were added to the end of the array.
                tvi[i].a = indexElements[i * 3 + 0] + offset;
                tvi[i].b = indexElements[i * 3 + 1] + offset;
                tvi[i].c = indexElements[i * 3 + 2] + offset;
            }
            // Store our triangles
            indices.AddRange(tvi);
        }

    }
}
