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

namespace hyades.graphics
{
    public class PhysicsRenderer
    {
        private GraphicsDevice device;
        private BasicEffect material;

        private static PhysicsRenderer instance;
        public static PhysicsRenderer GetInstance(GraphicsDevice device)
        {
            if (instance == null)
                instance = new PhysicsRenderer(device);
            return instance;
        }

        public PhysicsRenderer(GraphicsDevice device)
        {
            this.device = device;

            material = new BasicEffect(device);
            material.VertexColorEnabled = true;
        }

        public void Draw(Physics physics, Camera camera)
        {
            Draw(physics, camera, false, true, true, false, true, false);
        }

        public void Draw(Physics physics, Camera camera, bool shapes, bool outlines, bool normals, bool points, bool chains, bool tags)
        {
            material.Projection = camera.projection;
            material.View = camera.view;
            material.World = Matrix.Identity;
            material.CurrentTechnique.Passes[0].Apply();

            PrimitiveBatch instance = PrimitiveBatch.GetInstance(device);
            instance.Begin(Primitive.Line);

            for (int i = 0; i < physics.body_list.Count; i++)
            {
                Body body = physics.body_list[i];

                if (shapes)
                {
                    instance.SetColor(Color.Yellow);
                    for (int p = 1; p < body.curr_shape.count; p++)
                    {
                        instance.AddVertex(body.curr_shape.points[p - 1]);
                        instance.AddVertex(body.curr_shape.points[p - 0]);
                    }

                    instance.AddVertex(body.curr_shape.points[body.curr_shape.count - 1]);
                    instance.AddVertex(body.curr_shape.points[0]);
                }

                if (outlines)
                {
                    instance.SetColor(Color.White);
                    for (int p = 1; p < body.pointmass_list.Length; p++)
                    {
                        instance.AddVertex(body.pointmass_list[p - 1].position);
                        instance.AddVertex(body.pointmass_list[p - 0].position);
                    }

                    instance.AddVertex(body.pointmass_list[body.pointmass_list.Length - 1].position);
                    instance.AddVertex(body.pointmass_list[0].position);
                }

                
                if (normals)
                {
                    instance.SetColor(Color.Purple);
                    for (int p = 0; p < body.pointmass_list.Length; p++)
                    {
                        Vector2 pt = body.pointmass_list[p].position;

                        int prevPt = (p > 0) ? p - 1 : body.pointmass_list.Length - 1;
                        int nextPt = (p < body.pointmass_list.Length - 1) ? p + 1 : 0;

                        Vector2 prev = body.pointmass_list[prevPt].position;
                        Vector2 next = body.pointmass_list[nextPt].position;

                        Vector2 fromPrev = new Vector2();
                        fromPrev.X = pt.X - prev.X;
                        fromPrev.Y = pt.Y - prev.Y;

                        Vector2 toNext = new Vector2();
                        toNext.X = next.X - pt.X;
                        toNext.Y = next.Y - pt.Y;

                        Vector2 ptNorm = new Vector2();
                        ptNorm.X = fromPrev.X + toNext.X;
                        ptNorm.Y = fromPrev.Y + toNext.Y;
                        VectorHelper.Perpendicular(ref ptNorm);

                        ptNorm = Vector2.Normalize(ptNorm)*(camera.position.Z * 0.03f);

                        instance.AddVertex(pt);
                        instance.AddVertex(pt + ptNorm);
                    }
                }


                /*
                if (body is SpringBody)
                {
                    SpringBody sbody = body as SpringBody;

                    instance.SetColor(Color.Green);
                    for (int s = 0; s < sbody.spring_list.Count; s++)
                    {

                        instance.AddVertex(sbody.spring_list[s].pointmass_a.position);
                        instance.AddVertex(sbody.spring_list[s].pointmass_b.position);
                    }
                }*/
            }

            if (chains)
            {
                for (int i = 0; i < physics.chain_list.Count; i++)
                {
                    Chain chain = physics.chain_list[i];

                    instance.SetColor(Color.Green);
                    for (int s = 0; s < chain.spring_list.Count; s++)
                    {

                        instance.AddVertex(chain.spring_list[s].pointmass_a.position);
                        instance.AddVertex(chain.spring_list[s].pointmass_b.position);
                    }
                }
            }

            instance.End();


            if (points)
            {
                instance.Begin(Primitive.Quad);
                instance.SetColor(Color.Red);

                float size = 0.015f;
                for (int i = 0; i < physics.body_list.Count; i++)
                {
                    Body body = physics.body_list[i];

                    for (int p = 0; p < body.pointmass_list.Length; p++)
                    {
                        PointMass pm = body.pointmass_list[p];

                        instance.AddVertex(pm.position.X + size, pm.position.Y - size, 0);
                        instance.AddVertex(pm.position.X + size, pm.position.Y + size, 0);
                        instance.AddVertex(pm.position.X - size, pm.position.Y + size, 0);
                        instance.AddVertex(pm.position.X - size, pm.position.Y - size, 0);

                    }
                }

                for (int i = 0; i < physics.chain_list.Count; i++)
                {
                    Chain chain = physics.chain_list[i];

                    for (int p = 0; p < chain.pointmass_list.Count; p++)
                    {
                        PointMass pm = chain.pointmass_list[p];

                        instance.AddVertex(pm.position.X + size, pm.position.Y - size, 0);
                        instance.AddVertex(pm.position.X + size, pm.position.Y + size, 0);
                        instance.AddVertex(pm.position.X - size, pm.position.Y + size, 0);
                        instance.AddVertex(pm.position.X - size, pm.position.Y - size, 0);

                    }
                }

                instance.End();
            }

            if (tags)
            {
                SpriteRenderer spriterenderer = SpriteRenderer.GetInstance(device);
                spriterenderer.Begin(null);

                for (int i = 0; i < physics.body_list.Count; i++)
                {
                    Body body = physics.body_list[i];
                    Vector3 proj = camera.Project(new Vector3(body.position, 0));
                    spriterenderer.AddString(Resources.arial10px_font, string.Format("[id:{0} body:{1}",i, body.ToStringSimple()), proj.X, proj.Y);
                }

                spriterenderer.End();
            }



        }

        public void Draw(Body body, Camera camera)
        { Draw(body, camera, false, true, true, false, true, false); }

        public void Draw(Body body, Camera camera, bool shapes, bool outlines, bool normals, bool points, bool chains, bool tags)
        {
            material.Projection = camera.projection;
            material.View = camera.view;
            material.World = Matrix.Identity;
            material.CurrentTechnique.Passes[0].Apply();

            PrimitiveBatch instance = PrimitiveBatch.GetInstance(device);
            instance.Begin(Primitive.Line);

            if (shapes)
            {
                instance.SetColor(Color.Purple);
                for (int p = 1; p < body.curr_shape.count; p++)
                {
                    instance.AddVertex(body.curr_shape.points[p - 1]);
                    instance.AddVertex(body.curr_shape.points[p - 0]);
                }

                instance.AddVertex(body.curr_shape.points[body.curr_shape.count - 1]);
                instance.AddVertex(body.curr_shape.points[0]);
            }

            if (outlines)
            {
                instance.SetColor(Color.White);
                for (int p = 1; p < body.pointmass_list.Length; p++)
                {
                    instance.AddVertex(body.pointmass_list[p - 1].position);
                    instance.AddVertex(body.pointmass_list[p - 0].position);
                }

                instance.AddVertex(body.pointmass_list[body.pointmass_list.Length - 1].position);
                instance.AddVertex(body.pointmass_list[0].position);
            }

            if (normals)
            {
                instance.SetColor(Color.Purple);
                for (int p = 0; p < body.pointmass_list.Length; p++)
                {
                    Vector2 pt = body.pointmass_list[p].position;

                    int prevPt = (p > 0) ? p - 1 : body.pointmass_list.Length - 1;
                    int nextPt = (p < body.pointmass_list.Length - 1) ? p + 1 : 0;

                    Vector2 prev = body.pointmass_list[prevPt].position;
                    Vector2 next = body.pointmass_list[nextPt].position;

                    Vector2 fromPrev = new Vector2();
                    fromPrev.X = pt.X - prev.X;
                    fromPrev.Y = pt.Y - prev.Y;

                    Vector2 toNext = new Vector2();
                    toNext.X = next.X - pt.X;
                    toNext.Y = next.Y - pt.Y;

                    Vector2 ptNorm = new Vector2();
                    ptNorm.X = fromPrev.X + toNext.X;
                    ptNorm.Y = fromPrev.Y + toNext.Y;
                    VectorHelper.Perpendicular(ref ptNorm);

                    ptNorm = Vector2.Normalize(ptNorm) * (camera.position.Z * 0.03f); ;

                    instance.AddVertex(pt);
                    instance.AddVertex(pt + ptNorm);
                }
            }
            instance.End();


            if (points)
            {
                instance.Begin(Primitive.Quad);
                instance.SetColor(Color.Red);

                float size = 0.015f;

                for (int p = 0; p < body.pointmass_list.Length; p++)
                {
                    PointMass pm = body.pointmass_list[p];

                    instance.AddVertex(pm.position.X + size, pm.position.Y - size, 0);
                    instance.AddVertex(pm.position.X + size, pm.position.Y + size, 0);
                    instance.AddVertex(pm.position.X - size, pm.position.Y + size, 0);
                    instance.AddVertex(pm.position.X - size, pm.position.Y - size, 0);

                }

                instance.End();
            }

            if (tags)
            {
                SpriteRenderer spriterenderer = SpriteRenderer.GetInstance(device);
                spriterenderer.Begin(null);

                Vector3 proj = camera.Project(new Vector3(body.position, 0));
                spriterenderer.AddString(Resources.arial10px_font, body.ToStringSimple(), proj.X, proj.Y);


                spriterenderer.End();
            }
        }
    }
}
