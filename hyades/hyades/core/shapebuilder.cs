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

namespace hyades
{
    public class ShapeBuilder
    {
        private Model model;
        private List<Triangle> indices;
        private List<Vector3> vertices;

        private static Dictionary<Model, List<Triangle>> index_dictionary;
        private static Dictionary<Model, List<Vector3>> vertex_dictionary;

        static ShapeBuilder()
        {
            index_dictionary = new Dictionary<Model, List<Triangle>>();
            vertex_dictionary = new Dictionary<Model, List<Vector3>>();
        }

        public ShapeBuilder(Model model)
        {
            this.model = model;

            if (index_dictionary.ContainsKey(model))
            {
                this.vertices = vertex_dictionary[model];
                this.indices = index_dictionary[model];
            }
            else
            {
                this.indices = new List<Triangle>();
                this.vertices = new List<Vector3>();

                MeshReader.ExtractTrianglesFrom(model, vertices, indices, Matrix.Identity);

                index_dictionary.Add(model, indices);
                vertex_dictionary.Add(model, vertices);
            }
        }

        public Vector2[] GetShape(ref Matrix transform, Plane p)
        {
            List<Vector3> points;
            List<Line> lines;

            IntersectPlane(p, Transform(ref transform, vertices), indices, out points, out lines);

            return PolygonReduce(CreateShape(points, lines, 0.0001f), 0.3f).ToArray();
        }

        public static List<Vector3> Transform(ref Matrix transform, List<Vector3> vertices)
        {
            List<Vector3> world_vertices = new List<Vector3>(vertices.Count);

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 vertex = vertices[i];
                Vector3.Transform(ref vertex, ref transform, out vertex);
                world_vertices.Add(vertex);
            }

            return world_vertices;
        }

        public static void IntersectPlane(Plane p, List<Vector3> vertices, List<Triangle> indices, out List<Vector3> points, out List<Line> lines)
        {            
            lines = new List<Line>();
            points = new List<Vector3>();

            foreach (Triangle triangle in indices)
            {
                bool intersects = false;
                Vector3 a, b, q;

                a = vertices[triangle.a];
                b = vertices[triangle.b];
                if (IntersectSegmentPlane(a, b, p, out q))
                { points.Add(q); intersects = true; }

                a = vertices[triangle.b];
                b = vertices[triangle.c];
                if (IntersectSegmentPlane(a, b, p, out q))
                { points.Add(q); intersects = true; }

                a = vertices[triangle.c];
                b = vertices[triangle.a];
                if (IntersectSegmentPlane(a, b, p, out q))
                { points.Add(q); intersects = true; }

                if (intersects)
                {
                    Line line = new Line();
                    line.a = points.Count - 2;
                    line.b = points.Count - 1;
                    lines.Add(line);
                }
            }
        }

        private static List<Vector2> CreateShape(List<Vector3> vertices, List<Line> lines, float epsilon)
        {
            List<Vector2> output = new List<Vector2>();
            Vector3 vertex, initial;
            initial = vertices[0];
            vertex = initial;
            int curr_line = -1;
            while (true)
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    Vector3 a = vertices[lines[i].a];
                    Vector3 b = vertices[lines[i].b];

                    if (Same(vertex, a, epsilon))
                    {
                        if (curr_line == i)
                            continue;

                        curr_line = i;
                        vertex = vertices[lines[i].b];
                        output.Add(new Vector2(vertex.X, vertex.Y));
                        break;
                    }

                    if (Same(vertex, b, epsilon))
                    {
                        if (curr_line == i)
                            continue;

                        curr_line = i;
                        vertex = vertices[lines[i].a];
                        output.Add(new Vector2(vertex.X, vertex.Y));
                        break;
                    }
                }

                if (Same(vertex, initial, epsilon))
                    break;

                if (output.Count > vertices.Count)
                {
                    output.Clear();
                    return output;
                }
            }

            return output;
        }

        private static List<Vector2> PolygonReduce(List<Vector2> shape, float distance)
        {
            List<Vector2> output = new List<Vector2>();
            Vector2 key;
            key = shape[0];
            for (int i = 0; i < shape.Count; i++)
            {
                if (Vector2.Distance(shape[i], key) > distance)
                {
                    key = shape[i];
                    output.Add(key);
                }
            }

            return output;
        }

        private static bool Same(Vector3 a, Vector3 b, float epsilon)
        {
            return Math.Abs(a.X - b.X) < epsilon && Math.Abs(a.Y - b.Y) < epsilon && Math.Abs(a.Z - b.Z) < epsilon;
        }

        private static bool IntersectSegmentPlane(Vector3 a, Vector3 b, Plane p, out Vector3 q)
        {
            Vector3 ab = b - a;
            float d = (p.D - Vector3.Dot(p.Normal, a)) / Vector3.Dot(p.Normal, ab);

            if (d >= 0 && d <= 1)
            {
                q = a + d * ab;
                return true;
            }
            q = Vector3.Zero;

            return false;
        }
    }
}
