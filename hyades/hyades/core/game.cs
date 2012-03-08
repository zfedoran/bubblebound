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
using hyades.entity;
using hyades.level;
using hyades.graphics;
using hyades.graphics.particle;
using Microsoft.Xna.Framework.Audio;

namespace hyades
{
    public class Hyades
    {
        public InputDevice input;
        public Entity player;
        public Character character;
        public Level level;
        public Camera camera;
        public float total_time = 0;
        public ParticleManager particlemanager;

        public SoundEffectInstance background_music;

        private static Random rand = new Random();

        public Hyades(InputDevice input)
        {
            this.input = input;
            instance = this;

            background_music = Resources.background_music.CreateInstance();
            background_music.IsLooped = true;
            background_music.Play();
           

            player = new Player(input);
            player.position.Z = 2.0f;

            camera = new FirstPersonCamera(player);
            camera.width = Application.WIDTH;
            camera.height = Application.HEIGHT;
            camera.near = 0.1f;
            camera.far = 500;
            camera.fov = 1.1f;

            level = new Level();
            level.Load();

            Skybox skybox = new Skybox(player);
            level.Add(skybox);

            StaticModelEntity birthstone = new StaticModelEntity(Resources.rock_model);
            birthstone.position = Vector3.Zero;
            birthstone.rotation.X = 1.11592329f;
            birthstone.rotation.Y = 0.188416481f;
            birthstone.rotation.Z = 1.31485772f;
            birthstone.size = new Vector3(0.0006f);
            AmbientEntityBubblesLogic birthstone_logic = new AmbientEntityBubblesLogic(birthstone, 80, 10);
            level.Add(birthstone);


            for (int i = 0; i < 10; i++)
            {
                Bubble bubble = new Bubble(0.03f + (float)rand.NextDouble()*0.02f);
                bubble.SetPosition(new Vector2(-50000, 0));
                bubble.SetVelocity(new Vector2(0, -1.0f));
                level.Add(bubble);
            }

            Enemy enemy;
            for (int i = 0; i < 2; i++)
            {
                enemy = new Fish();
               // enemy.size *= 0.1f;
                enemy.size *= (((float)rand.NextDouble())) * 0.06f + 0.05f;
               // enemy.size.Z *= 0.2f;
                //enemy.position.Z = (((float)rand.NextDouble()) -0.5f)*2;
                enemy.SetPosition(new Vector2(-50000, 0));
                level.Add(enemy);
            }

            for (int i = 0; i < 1; i++)
            {
                enemy = new Octopus();
                enemy.size *= 0.1f;
                enemy.SetPosition(new Vector2(-50000, -10));
                level.Add(enemy);
            }

            character = new Character(player, input);
            character.SetPosition(new Vector2(0, 0));
            level.Add(character);
            Enemy.player = character;

            particlemanager = ParticleManager.GetInstance();

            AmbientParticleLogic ambient_logic = new AmbientParticleLogic(character, 10);
            
            for (int i = 0; i < 400; i++)
            {
                ambient_logic.Emit();
            }

            AmbientCircleParticleLogic ambient_circle_logic = new AmbientCircleParticleLogic(character, 20);

            for (int i = 0; i < 10; i++)
            {
                ambient_circle_logic.Emit();
            }

            WormParticleLogic worm_logic = new WormParticleLogic(character, 10);

            for (int i = 0; i < 20; i++)
            {
                worm_logic.Emit();
            }

            Update(0);
        }

        public void Update(double elapsed)
        {
            total_time += (float)elapsed;

            camera.Update(elapsed);
            player.Update(elapsed);
            level.Update(elapsed);

            particlemanager.Update(elapsed);

            //player.position.X = fish.position.X;
            //player.position.Y = fish.position.Y;
            //player.position.Z = 0;
        }

        public void Draw(GraphicsDevice device)
        {
            PostProcessor postprocessor = PostProcessor.GetInstance(device);

            device.SetRenderTargets(postprocessor.color_rt, postprocessor.depth_rt);
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil, Application.CLEAR_COLOR, 1.0f, 0);
            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;

            level.Draw3dEntities(device, camera);

            postprocessor.DepthOfField(postprocessor.color_rt, postprocessor.result_rt, postprocessor.depth_rt, camera, DepthOfFieldType.DiscBlur, player.position.Z, 5, total_time);

            device.SetRenderTarget(postprocessor.color_rt);
            device.Clear(ClearOptions.Target, Application.CLEAR_COLOR, 1, 0);
            device.BlendState = BlendState.AlphaBlend;

            level.Draw2dEntities(device, camera);
            device.DepthStencilState = DepthStencilState.DepthRead;
            particlemanager.Draw(device, camera);
            device.DepthStencilState = DepthStencilState.Default;
            level.DrawBubbleReflections(device, camera);

            device.SetRenderTargets(postprocessor.result_rt);

            SpriteRenderer spriterenderer = SpriteRenderer.GetInstance(device);
            spriterenderer.Begin(null);
            spriterenderer.Add(new TextureRegion(postprocessor.color_rt, 0, 0, postprocessor.color_rt.Width, postprocessor.color_rt.Height), Color.White, 0, 0, postprocessor.color_rt.Width, postprocessor.color_rt.Height, 0);
            spriterenderer.End();

        }

        public void SetDebugView(bool visible)
        {
            if (visible)
                level.is_debug_visible = true;
            else
                level.is_debug_visible = false;
        }

        private static Hyades instance;

        public static Hyades GetInstance()
        { return instance; }
    }
}
