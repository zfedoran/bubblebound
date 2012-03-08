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
using hyades.entity;
using hyades.screen;
using hyades.graphics;
using hyades.physics;
using hyades.utils;
using hyades.graphics.particle;

namespace hyades.level
{
    public class Level
    {
        public string name = "First Level";
        public int id = 0;

        public List<Entity> entities;
        public Physics physics;
        public Plane plane;
        public Character player;
        public HintBubblesLogic hint_logic;

        private ModelRenderer modelrenderer;
        private BasicEffect material;
        private bool is_warm;
        public  bool is_debug_visible;

        private static Random rand = new Random();

        public Level()
        {
            this.entities = new List<Entity>();
            this.physics = new Physics();
            this.plane = new Plane(0, 0, -1, 0);

            this.hint_logic = new HintBubblesLogic(10);

            physics.on_penetration = penetration;
            physics.on_collision = collision;
        }

        private void collision(Body body_a, Body body_b, CollisionInfo info)
        {
            if (player.hurt != 0)
                return;

            // early out, cant be a bubble merge
            if (body_a.is_static || body_b.is_static)
                return;

            if (body_a == player.body)
                if (FindEnemy(body_b) != null)
                {
                    player.Hurt();
                    Resources.droplet_sound.Play(0.025f, ((float)rand.NextDouble() - 0.5f), 0);
                }

            if (body_b == player.body)
                if (FindEnemy(body_a) != null)
                {
                    player.Hurt();
                    Resources.droplet_sound.Play(0.025f, ((float)rand.NextDouble() - 0.5f), 0);
                }
        }



        private void penetration(Body body_a, Body body_b)
        {
            // early out, cant be a bubble merge
            if (body_a.is_static || body_b.is_static)
                return;

            if (body_a == player.body)
                if (TryMerge(FindBubble(body_b)))
                    player.num_collected++;

            if (body_b == player.body)
                if (TryMerge(FindBubble(body_a)))
                    player.num_collected++;

            

        }

        private Entity FindEnemy(Body body)
        {
            for (int i = 0; i < entities.Count; i++)
            {
                Entity entity = entities[i];

                if (entity is Enemy || entity is Fish || entity is Octopus)
                {
                    Enemy enemy = entity as Enemy;

                    if (enemy.body == body)
                        return enemy;
                }
            }

            return null;
        }

        private bool TryMerge(Entity entity)
        {
            if (!(entity is Bubble))
                return false;

            Bubble bubble = entity as Bubble;
            Body body = bubble.body;

            if (body.is_merging)
                return false;

            Resources.droplet_sound.Play(0.025f, ((float)rand.NextDouble()-0.5f), 0);

            body.is_merging = true;
            // body.is_static = true;
            InterpolatorCollection.GetInstance().Create(0, 1, 0.2f,
            (interpolator) =>
            {

                //body.position = Vector2.Lerp(body.position, bubble.body.position, interpolator.value);
            },
            (interpolator) =>
            {

                body.is_merging = false;
                body.position.X = -99999999;
                body.velocity *= 0.1f;
            }
            );

            return true;

        }

        private Entity FindBubble(Body body)
        {
            for (int i = 0; i < entities.Count; i++)
            {
                Entity entity = entities[i];

                if (entity is Bubble)
                {
                    Bubble bubble = entity as Bubble;

                    if (bubble.body == body)
                        return bubble;
                }
            }

            return null;
        }

        public void Update(double elapsed)
        {
            physics.MoveDistantBodies(player.body.position, 15, 20);

            Entity entity;
            for (int i = entities.Count-1; i >= 0; i--)
            {
                entity = entities[i];
                if (entity.removed)
                    Remove(entity);
                else
                {
                    entity.Update(elapsed);

                    if (entity is Enemy)
                        (entity as Enemy).UpdateTarget(physics, elapsed);
                }
            }

            physics.Update(elapsed);
        }

        public void Draw3dEntities(GraphicsDevice device, Camera camera)
        {
            if (!is_warm)
                Warm(device);

            Entity entity;

            modelrenderer.Begin(camera);

            for (int i = 0; i < entities.Count; i++)
            {
                entity = entities[i];

                if (entity is StaticModelEntity || entity is WallModelEntity || entity is Skybox)
                {
                    Model model = (entity as IHasModel).GetModel();
                    Matrix transform = entity.GetTransform();
                    modelrenderer.Add(model, ref transform);
                }
            }

            modelrenderer.End();

            //animated models appear to have messed up normals... dirty fix:
            device.RasterizerState = RasterizerState.CullNone;
            for (int i = 0; i < entities.Count; i++)
            {
                entity = entities[i];

                if (entity is AnimatedModelEntity || entity is Enemy)
                    entity.Draw(device, camera);
            }
            device.RasterizerState = RasterizerState.CullCounterClockwise;
            /*
#if DEBUG
            if (!is_debug_visible)
                return;

            modelrenderer.Begin(camera, Color.Red);

            for (int i = 0; i < entities.Count; i++)
            {
                entity = entities[i];

                if (entity is WallModelEntity)
                {
                    Model model = (entity as IHasModel).GetModel();
                    Matrix transform = entity.GetTransform();
                    modelrenderer.Add(model, ref transform);
                }
            }

            modelrenderer.End();
#endif
             */
        }

        public void DrawBubbleReflections(GraphicsDevice device, Camera camera)
        {
            if (!is_warm)
                Warm(device);

            Entity entity;
            for (int i = 0; i < entities.Count; i++)
            {
                entity = entities[i];

                if (entity is Bubble)
                    (entity as Bubble).DrawReflection(device, camera);
            }
        }

        public void Draw2dEntities(GraphicsDevice device, Camera camera)
        {
            if (!is_warm)
                Warm(device);

            Entity entity;
            for (int i = 0; i < entities.Count; i++)
            {
                entity = entities[i];

                if (entity is Bubble || entity is Billboard)
                    entity.Draw(device, camera);

                if (entity is Bubble || entity is Fish)
                {
                    float distance = (entity.position - player.position).Length();
                    if (distance < 10)
                    {
                        hint_logic.entity = entity;
                        hint_logic.Emit();
                    }
                }
            }


            if (!is_debug_visible)
                return;

            // no depth reading
            device.DepthStencilState = DepthStencilState.None;

            PhysicsRenderer physicsrenderer = PhysicsRenderer.GetInstance(device);
            physicsrenderer.Draw(physics, camera);

            // set depth reading to default
            device.DepthStencilState = DepthStencilState.Default;

        }

        private void Warm(GraphicsDevice device)
        {
            modelrenderer = new ModelRenderer(device);
            material = new BasicEffect(device);
            material.VertexColorEnabled = true;
            is_warm = true;
        }

        public void Add(Entity entity)
        {
            if (entities.Contains(entity))
                return;

            entities.Add(entity);

            if (entity is WallModelEntity)
            {
                WallModelEntity wallentity = entity as WallModelEntity;

                wallentity.BuildWall(plane);
                physics.Add(wallentity.body);
            }

            if (entity is Bubble)
            {
                Bubble bubble = entity as Bubble;
                physics.Add(bubble.body);
            }

            if (entity is Enemy)
            {
                Enemy enemy = entity as Enemy;
                physics.Add(enemy.body);
            }

            if (entity is Character)
                player = entity as Character;
        }

        public void Remove(Entity entity)
        {
            if (!entities.Contains(entity))
                return;

            entities.Remove(entity);

            if (entity is WallModelEntity)
            {
                WallModelEntity wallentity = entity as WallModelEntity;

                physics.Remove(wallentity.body);
            }

            if (entity is Bubble)
            {
                Bubble bubble = entity as Bubble;
                physics.Remove(bubble.body);
            }

            if (entity is Enemy)
            {
                Enemy enemy = entity as Enemy;
                physics.Remove(enemy.body);
            }
        }

        public int Count
        { get { return entities.Count; } }


        public Entity GetEntityAt(Ray ray, float depth)
        {
            Entity smallest = null;

            for (int i = 0; i < entities.Count; i++)
            {
                Entity entity = entities[i];

                // pick current layer only
                if (entity.position.Z != depth)
                    continue;

                if (!(entity is IHasModel))
                    continue;

                Model model = ((IHasModel)entity).GetModel();
                Matrix transform = entity.GetTransform();
                Matrix[] bones = new Matrix[model.Bones.Count];
                model.CopyAbsoluteBoneTransformsTo(bones);

                if (RayIntersectsModel(ray, model, transform, bones))
                {
                    if (smallest == null || entity.size.X < smallest.size.X && entity.size.Y < smallest.size.Y && entity.size.Z < smallest.size.Z)
                        smallest = entity;
                }
            }

            return smallest;
        }

        private static bool RayIntersectsModel(Ray ray, Model model, Matrix worldTransform, Matrix[] absoluteBoneTransforms)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                Matrix world = absoluteBoneTransforms[mesh.ParentBone.Index] * worldTransform;
                BoundingSphere sphere = TransformBoundingSphere(mesh.BoundingSphere, world);
                if (sphere.Intersects(ray) != null)
                    return true;
            }

            return false;
        }

        private static BoundingSphere TransformBoundingSphere(BoundingSphere sphere, Matrix transform)
        {
            BoundingSphere transformedSphere;
            Vector3 scale3 = new Vector3(sphere.Radius, sphere.Radius, sphere.Radius);
            scale3 = Vector3.TransformNormal(scale3, transform);
            transformedSphere.Radius = Math.Max(scale3.X, Math.Max(scale3.Y, scale3.Z));
            transformedSphere.Center = Vector3.Transform(sphere.Center, transform);

            return transformedSphere;
        }



        public void Save()
        {
            List<XmlEntity> xmlentities = new List<XmlEntity>();
            XmlLevel xmllevel = new XmlLevel(name, id, xmlentities);

            for (int i = 0; i < entities.Count; i++)
            {
                Entity entity = entities[i];
                
                XmlEntity xmlentity = new XmlEntity();
                xmlentity.position = entity.position;
                xmlentity.rotation = entity.rotation;
                xmlentity.size = entity.size;

                if (entity is StaticModelEntity)
                {
                    StaticModelEntity modelentity = entity as StaticModelEntity;

                    xmlentity.asset = Resources.GetName(modelentity.GetModel());
                    xmlentity.type = "static_model";
                }

                if (entity is AnimatedModelEntity)
                {
                    AnimatedModelEntity modelentity = entity as AnimatedModelEntity;

                    xmlentity.asset = Resources.GetName(modelentity.GetModel());
                    xmlentity.type = "animated_model";
                }

                if (entity is WallModelEntity)
                {
                    WallModelEntity wallentity = entity as WallModelEntity;

                    xmlentity.asset = Resources.GetName(wallentity.GetModel());
                    xmlentity.type = "wall_model";
                }

                if (entity is Billboard)
                {
                    Billboard billboard = entity as Billboard;

                    xmlentity.asset = Resources.GetName(billboard.region.texture);
                    xmlentity.type = "billboard";
                    xmlentity.color = billboard.color;
                }

                xmlentities.Add(xmlentity);
            }

            XmlHelper.Save(xmllevel);
        }

        public void Load()
        {
            entities.Clear();

            XmlLevel xmllevel = XmlHelper.Load(id);
            List<XmlEntity> xmlentities = xmllevel.entities;

            for (int i = 0; i < xmlentities.Count; i++)
            {
                XmlEntity xmlentity = xmlentities[i];
                Entity entity = null;

                if (xmlentity.type == "static_model")
                {
                    Model model = (Model)Resources.GetObject(xmlentity.asset);
                    entity = new StaticModelEntity(model);
                }

                if (xmlentity.type == "animated_model")
                {
                    Model model = (Model)Resources.GetObject(xmlentity.asset);
                    entity = new AnimatedModelEntity(model);
                }

                if (xmlentity.type == "wall_model")
                {
                    Model model = (Model)Resources.GetObject(xmlentity.asset);
                    entity = new WallModelEntity(model);
                }

                if (xmlentity.type == "billboard")
                {
                    Texture2D texture = (Texture2D)Resources.GetObject(xmlentity.asset);
                    entity = new Billboard(texture);
                    ((Billboard)entity).color = xmlentity.color;
                }

                if (entity == null)
                    continue;

                entity.position = xmlentity.position;
                entity.rotation = xmlentity.rotation;
                entity.size = xmlentity.size;

                Add(entity);
            }
        }
    }
}
