﻿/*
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

namespace hyades.graphics
{
    public struct PrimitiveBatchVertex : IVertexType
    {
        public Vector3 position;
        public Vector2 texture;
        public Color color;

        private static readonly VertexDeclaration vertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(20, VertexElementFormat.Color, VertexElementUsage.Color, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return vertexDeclaration; }
        }

        public override string ToString()
        {
            return position.ToString();
        }
    }

    public struct LineBatchVertex : IVertexType
    {
        public Vector3 position;
        public Color color;

        private static readonly VertexDeclaration vertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return vertexDeclaration; }
        }

        public override string ToString()
        {
            return position.ToString();
        }
    }
}
