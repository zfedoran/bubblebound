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
using SkinnedModel;
using Microsoft.Xna.Framework;

namespace hyades.entity
{
    public class Fish : Enemy, IHasModel
    {
        private Model model;
        private SkinningData skinning_data;
        private AnimationPlayer animation_player;
        private TimeSpan time;
        private float speed;

        public Fish()
        {
            this.model = Resources.fish_model; 
            this.size = Vector3.One;
            this.speed = 2;
            this.max_speed = (float)rand.NextDouble()*2 + 0.5f;
            this.time = new TimeSpan();

            skinning_data = model.Tag as SkinningData;

            if (skinning_data == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag.");

            animation_player = new AnimationPlayer(skinning_data);

            StartDefaultClip();
            Update(0);
        }

        private void StartDefaultClip()
        {
            string clip = "";
            foreach (string item in skinning_data.AnimationClips.Keys)
            {
                clip = item;
                break;
            }

            AnimationClip animation_clip = skinning_data.AnimationClips[clip];
            animation_player.StartClip(animation_clip);
        }

        public void StartClip(string name)
        {
            AnimationClip animation_clip = skinning_data.AnimationClips[name];
            animation_player.StartClip(animation_clip);
        }

        public override void Update(double elapsed)
        {
            time = new TimeSpan((int)((elapsed * speed) / 1E-07)); // thanks to reflector

            Matrix world, transform;
            CreateWorldTransformMatrix(ref position, ref rotation, ref size, out world);

            Vector3 forward, up, right;
            up = Vector3.Up;
            if (velocity.X < 0)
                up = Vector3.Down;

            Vector3.Normalize(ref velocity, out forward);
            Vector3.Cross(ref up, ref forward, out right);
            Vector3.Normalize(ref right, out right);
            Vector3.Cross(ref forward, ref right, out up);

            forward = -forward;

            transform = Matrix.Identity;
            transform.Up = up;
            transform.Right = forward;
            transform.Forward = right;

            world = transform * world;

            animation_player.Update(time, true, world);

            base.Update(elapsed);
        }


        public override void Draw(GraphicsDevice device, Camera camera)
        {
            Matrix[] bones = animation_player.GetSkinTransforms();

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters["Bones"].SetValue(bones);
                    effect.Parameters["View"].SetValue(camera.view);
                    effect.Parameters["Projection"].SetValue(camera.projection);
                    effect.Parameters["FarClip"].SetValue(camera.far);
                    effect.Parameters["FogEnabled"].SetValue(1);
                    effect.Parameters["FogStart"].SetValue(camera.near);
                    effect.Parameters["FogEnd"].SetValue(camera.far);
                    effect.Parameters["DesaturateEnabled"].SetValue(1);
                }

                mesh.Draw();
            }

           // base.Draw(device, camera);
        }

        public Model GetModel()
        { return model; }
    }

}
