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
using hyades.graphics.prefab;
using hyades.screen;


namespace hyades.entity
{
    public class Player : Entity
    {
        private InputDevice input;
        private BasicEffect material;

        private Matrix world;

        public Player(InputDevice input)
        {
            this.input = input;
            this.size = Vector3.One;
        }

        public override void Update(double elapsed)
        {
            if (removed)
                return;

            float x, y;
            x = input.LeftThumbStick.X;
            y = input.LeftThumbStick.Y;
            if (Math.Abs(x) + Math.Abs(y) < Math.Abs(input.RightThumbStick.X) + Math.Abs(input.RightThumbStick.Y))
            {
                x = input.RightThumbStick.X;
                y = input.RightThumbStick.Y;
            }

            velocity.X += (float)(x * elapsed * 10);
            velocity.Y += (float)(y * elapsed * 10);

            velocity.X *= 0.8f;
            velocity.Y *= 0.8f;

            position.X += velocity.X;
            position.Y += velocity.Y;

            Entity.CreateWorldTransformMatrix(ref position, ref rotation, ref size, out world);
        }

        public override void Draw(GraphicsDevice device, Camera camera)
        {

        }
    }

    public class UnrestrictedPlayer : Entity
    {
        private InputDevice input;
        private BasicEffect material;

        private Matrix world;
        private Matrix rotation_matrix;
        private Vector3 rotation_velocity;

        public UnrestrictedPlayer(InputDevice input)
        {
            this.input = input;
            this.size = Vector3.One;
        }

        public override void Update(double elapsed)
        {
            rotation_velocity.X += (float)(input.RightThumbStick.Y * elapsed);
            rotation_velocity.Y += (float)(-input.RightThumbStick.X * elapsed);
            rotation_velocity.X *= 0.8f;
            rotation_velocity.Y *= 0.8f;

            rotation.X = MathHelper.Clamp(rotation.X + rotation_velocity.X, -MathHelper.PiOver2 * 0.95f, MathHelper.PiOver2 * 0.95f);
            rotation.Y += rotation_velocity.Y;
            rotation_matrix = Matrix.CreateRotationX(rotation.X) * Matrix.CreateRotationY(rotation.Y);

            velocity.X += (float)((rotation_matrix.Forward.X * input.LeftThumbStick.Y + rotation_matrix.Right.X * input.LeftThumbStick.X) * elapsed);
            velocity.Y += (float)((rotation_matrix.Forward.Y * input.LeftThumbStick.Y + rotation_matrix.Right.Y * input.LeftThumbStick.X) * elapsed);
            velocity.Z += (float)((rotation_matrix.Forward.Z * input.LeftThumbStick.Y + rotation_matrix.Right.Z * input.LeftThumbStick.X) * elapsed);


            velocity.X *= 0.8f;
            velocity.Y *= 0.8f;
            velocity.Z *= 0.8f;

            position.X += velocity.X;
            position.Y += velocity.Y;
            position.Z += velocity.Z;

            Entity.CreateWorldTransformMatrix(ref position, ref rotation, ref size, out world);
        }

        public override void Draw(GraphicsDevice device, Camera camera)
        {

        }
    }

}
