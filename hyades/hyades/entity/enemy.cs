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
using hyades.physics;
using Microsoft.Xna.Framework;
using hyades.graphics;
using Microsoft.Xna.Framework.Graphics;

namespace hyades.entity
{
    public class Enemy : Entity
    {
        public static Character player;

        public PressureBody body;
        public Shape shape;
        public Vector2 target;
        public float max_speed;
        public float radius;

        public static Random rand = new Random();

        public Enemy()
        {
            float scale = 0.5f;
            Vector2[] points = new Vector2[]
            {
                new Vector2(0,0),
                new Vector2(0,scale*0.5f),
                new Vector2(scale*0.5f,scale*0.5f),
                new Vector2(scale*0.5f,0),
            };

            shape = new Shape(points, true);

            body = new PressureBody(shape, 1, 1, 1300, 20, 1000, 20);

            max_speed = 2;
            radius = 2f;

        }

        public override void Update(double elapsed)
        {
            LimitSpeed();

            position.X = body.position.X;
            position.Y = body.position.Y;
        }

        public override void Draw(GraphicsDevice device, Camera camera)
        {
            PhysicsRenderer physicsrenderer = PhysicsRenderer.GetInstance(device);
            physicsrenderer.Draw(body, camera);

            SpriteRenderer spriterenderer = SpriteRenderer.GetInstance(device);
            spriterenderer.Begin(null);
            Vector3 proj = camera.Project(new Vector3(target, 0));
            spriterenderer.AddString(Resources.arial10px_font, "x", proj.X, proj.Y);
            spriterenderer.End();
        }

        public void UpdateTarget(Physics physics, double elapsed)
        {
            Vector2 direction; float distance;

            distance = (position - player.position).Length();

            if (!player.popped && player.hurt == 0 && distance < 2)
            {
                target.X = player.position.X;
                target.Y = player.position.Y;
            }
            else
            {
                direction = body.position - target;
                distance = direction.Length();

                if (distance < radius || velocity.Length() < 0.1f)
                {
                    Vector2 point = new Vector2(position.X, position.Y);
                    point.X += (float)(rand.NextDouble() - 0.5f) * radius * 15;
                    point.Y += (float)(rand.NextDouble() - 0.5f) * radius * 15;

                    if (!physics.IsPointInsideAnyBody(point))
                    {
                        target = point;
                    }
                }
            }

            direction = target - body.position;
            direction.Normalize();

            body.velocity.X += (float)(direction.X * elapsed) * 3;
            body.velocity.Y += (float)(direction.Y * elapsed) * 3;
        }

        public void SetPosition(Vector2 position)
        {
            body.position = position;
        }

        public void SetVelocity(Vector2 velocity)
        {
            body.velocity = velocity;
        }

        private void LimitSpeed()
        {
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
    }
}
