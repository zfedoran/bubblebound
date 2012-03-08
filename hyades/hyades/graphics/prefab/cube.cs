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

namespace hyades.graphics.prefab
{
    public class PrefabCube : PrefabMesh
    {
        public static readonly Vector3[] normals;

        static PrefabCube()
        {
            normals = new Vector3[] 
            {
                new Vector3(1, 0, 0),  //right
                new Vector3(-1, 0, 0), //left
                new Vector3(0, 1, 0),  //top
                new Vector3(0, -1, 0), //bottom
                new Vector3(0, 0, 1),  //back
                new Vector3(0, 0, -1), //front
            };
        }
        
        private int currentindex;
        private int currentvertex;

        public PrefabCube(Color color)
            : this(color, Matrix.Identity)
        { }

        public PrefabCube(Color color, Matrix transform)
        {
            this.name = "Cube";
            this.currentindex = 0;
            this.currentvertex = 0;

            this.vertices = new PrefabVertex[4 * 6];

#if HIDEF
            this.indices = new int[6 * 6];
#else
            this.indices = new short[6 * 6];
#endif

            Vector3 side1, side2, normal;
            side1 = new Vector3();
            currentindex = 0;
            currentvertex = 0;
            for (int i = 0; i < 6; i++)
            {
                normal = normals[i];
                side1.X = normal.Y;
                side1.Y = normal.Z;
                side1.Z = normal.X;
                Vector3.Cross(ref normal, ref side1, out side2);

#if HIDEF
                indices[currentindex++] = currentvertex + 0;
                indices[currentindex++] = currentvertex + 1;
                indices[currentindex++] = currentvertex + 2;

                indices[currentindex++] = currentvertex + 0;
                indices[currentindex++] = currentvertex + 2;
                indices[currentindex++] = currentvertex + 3;
#else
                indices[currentindex++] = (short)(currentvertex + 0);
                indices[currentindex++] = (short)(currentvertex + 1);
                indices[currentindex++] = (short)(currentvertex + 2);

                indices[currentindex++] = (short)(currentvertex + 0);
                indices[currentindex++] = (short)(currentvertex + 2);
                indices[currentindex++] = (short)(currentvertex + 3);
#endif

                vertices[currentvertex].position = (normal - side1 - side2) * 0.5f;
                vertices[currentvertex].normal = normal;
                vertices[currentvertex].color = color;
                currentvertex++;
                vertices[currentvertex].position = (normal - side1 + side2) * 0.5f;
                vertices[currentvertex].normal = normal;
                vertices[currentvertex].color = color;
                currentvertex++;
                vertices[currentvertex].position = (normal + side1 + side2) * 0.5f;
                vertices[currentvertex].normal = normal;
                vertices[currentvertex].color = color;
                currentvertex++;
                vertices[currentvertex].position = (normal + side1 - side2) * 0.5f;
                vertices[currentvertex].normal = normal;
                vertices[currentvertex].color = color;
                currentvertex++;
            }

            for (int i = 0; i < vertices.Length; i++)
                Vector3.Transform(ref vertices[i].position, ref transform, out vertices[i].position);
        }
    }
}
