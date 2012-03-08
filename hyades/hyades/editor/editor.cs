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
using Microsoft.Xna.Framework.Input;
using hyades.entity;
using hyades.level;
using hyades.graphics;
using SkinnedModel;

namespace hyades.editor
{
    public class Editor
    {
        private static Random rand = new Random();
        public Cursor cursor;
        public Level level;

        public int curr_asset_index;
        public int curr_depth_index;

        public float[] depth_values;
        public Entity curr_entity;
        public List<object> asset_list;

        public InputDevice input;
        public Entity player;
        public Camera camera;
        public bool use_free_camera, use_walls;

        public Editor(InputDevice input)
        {
            this.input = input;

            player = new Player(input);
            player.position.Y = 0.0f;
            player.position.Z = 15.0f;

            camera = new FirstPersonCamera(player);
            camera.width = Application.WIDTH;
            camera.height = Application.HEIGHT;
            camera.near = 0.1f;
            camera.far = 500;
            camera.fov = 1.1f;

            depth_values = new float[]{ 0, -150, -400};
            curr_depth_index = 0;

            cursor = new Cursor(6);
            cursor.color = Color.Gray;

            level = new Level();

            asset_list = new List<object>();
            asset_list.Add(Resources.box_model);
            asset_list.Add(Resources.rock_model);
            asset_list.Add(Resources.seaurchin_model);
            asset_list.Add(Resources.ray_texture);
            asset_list.Add(Resources.starfish_model);

            instance = this;
        }

        public void Update(double elapsed)
        {
            camera.Update(elapsed);
            player.Update(elapsed);
            //level.Update(elapsed);

            // get camera ray
            Vector3 direction = camera.target - camera.position;
            direction.Normalize();
            Ray camera_ray = new Ray(camera.position, direction);

            bool unrestricted = player is UnrestrictedPlayer;
            
            // move camera in and out
            if (input.IsDown(Buttons.DPadDown))
                player.position.Z += (float)elapsed * 50;
            if (input.IsDown(Buttons.DPadUp))
                player.position.Z -= (float)elapsed * 50;

            // switch layers
            if (input.IsPressed(Buttons.RightShoulder))
                curr_depth_index++;
            curr_depth_index %= depth_values.Length;

            // get next asset
            if (input.IsPressed(Buttons.LeftShoulder))
            {
                object asset = asset_list[curr_asset_index];
                curr_entity = CreateFromAsset(asset);

                curr_asset_index++;
            }
            curr_asset_index %= asset_list.Count;

            if (curr_entity != null)
            {
                // randomize rotation
                if (input.IsPressed(Buttons.Y))
                {
                    curr_entity.rotation.X = (float)rand.NextDouble() * MathHelper.TwoPi;
                    curr_entity.rotation.Y = (float)rand.NextDouble() * MathHelper.TwoPi;
                    curr_entity.rotation.Z = (float)rand.NextDouble() * MathHelper.TwoPi;
                }

                // rotate
                if (!unrestricted)
                {
                    curr_entity.rotation.X += (float)(input.RightThumbStick.Y * elapsed);
                    curr_entity.rotation.Z += (float)(input.RightThumbStick.X * elapsed);
                }

                // scale
                curr_entity.size *= 1 + (float)(input.RightTrigger * elapsed);
                curr_entity.size *= 1 - (float)(input.LeftTrigger * elapsed);

                // position
                Vector3 position = player.position;
                Plane p = new Plane(0, 0, -1, depth_values[curr_depth_index]);
                float? distance = camera_ray.Intersects(p);
                if (distance != null)
                    position = camera.position + camera_ray.Direction * (float)distance;

                curr_entity.position.X = position.X;
                curr_entity.position.Y = position.Y;
                curr_entity.position.Z = depth_values[curr_depth_index];

                if(curr_entity is AnimatedModelEntity)
                    curr_entity.Update(elapsed);

                // place entity
                if (input.IsPressed(Buttons.A))
                {
                    Entity clone;

                    if (use_walls && curr_entity is StaticModelEntity)
                    {
                        clone = new WallModelEntity((curr_entity as IHasModel).GetModel());
                        Entity.CopyFrom(curr_entity, clone);
                    }
                    else
                    {
                        clone = Entity.Clone(curr_entity);
                    }

                    level.Add(clone);
                }
            }
            else
            {
                if (input.IsPressed(Buttons.A))
                {
                    curr_entity = level.GetEntityAt(camera_ray, depth_values[curr_depth_index]);

                    if (curr_entity != null)
                        level.Remove(curr_entity);
                }
            }

            if (input.IsPressed(Buttons.B))
            {
                curr_entity = null;
            }

            if (input.IsPressed(Buttons.X))
            {
                use_walls = !use_walls;
            }
        }



        public void Draw(GraphicsDevice device)
        {
            level.Draw3dEntities(device, camera);
            level.Draw2dEntities(device, camera);

            if (curr_entity != null)
            {
                if (curr_entity is WallModelEntity)
                {
                    (curr_entity as WallModelEntity).color = Color.Red;
                }
                if (curr_entity is StaticModelEntity)
                {
                    if(use_walls)
                        (curr_entity as StaticModelEntity).color = Color.Red;
                    else
                        (curr_entity as StaticModelEntity).color = Color.White;
                }

                curr_entity.Draw(device, camera);
            }
            else
                cursor.Draw(device);


            SpriteRenderer spriterenderer = SpriteRenderer.GetInstance(device);
            spriterenderer.Begin(null);

            spriterenderer.AddString(Resources.arial10px_font, string.Format("Level Name:[{0} | {1}.xml] Entities:{2}", level.name, level.id, level.Count), 20, 20, Color.Yellow);
            spriterenderer.AddString(Resources.arial10px_font, string.Format("Building Walls:{0}", use_walls), 20, 40, Color.Red);

            for (int i = 0; i < depth_values.Length; i++)
                spriterenderer.AddString(Resources.arial10px_font, (curr_depth_index==i?">> ":"    ") + depth_values[i].ToString(), 20, 60+12*i);

            spriterenderer.End();
        }

        public Entity CreateFromAsset(object asset)
        {
            Entity entity = null;

            if (asset is Model)
            {
                Model model = asset as Model;

                if (model.Tag is SkinningData)
                    entity = new AnimatedModelEntity(model);
                else
                    entity = new StaticModelEntity(model);
            }
            else if (asset is Texture2D)
            {
                Texture2D texture = asset as Texture2D;
                entity = new Billboard(texture);
            }

            return entity;
        }

        public void Save()
        {
            level.Save();
        }

        public void Load()
        {
            level.Load();
        }

        public void SetDebugView(bool visible)
        {
            if (visible)
                level.is_debug_visible = true;
            else
                level.is_debug_visible = false;
        }

        public void SetCamera(bool unrestricted)
        {
            if (unrestricted)
                SetUnrestrictedCamera();
            else
                SetRestrictedCamera();
        }

        private void SetUnrestrictedCamera()
        {
            float x, y, z;
            x = player.position.X;
            y = player.position.Y;
            z = player.position.Z;

            player = new UnrestrictedPlayer(input);
            player.position.X = x;
            player.position.Y = y;
            player.position.Z = z;
            ((FirstPersonCamera)camera).entity = player;
        }

        private void SetRestrictedCamera()
        {
            float x, y;
            x = player.position.X;
            y = player.position.Y;

            player = new Player(input);
            player.position.X = x;
            player.position.Y = y;
            player.position.Z = 15.0f;
            ((FirstPersonCamera)camera).entity = player;
        }


        private static Editor instance;

        public static Editor GetInstance()
        { return instance; }
    }
}
