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
using hyades.graphics.particle;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using hyades.graphics;
using Microsoft.Xna.Framework.Input;
using hyades.utils;

namespace hyades.entity
{
    public class Character : Bubble
    {
        public Entity player;
        public InputDevice input;
        private BubbleTailLogic tail_logic;
        private BoostLogic boost_logic;
        private HurtLogic hurt_logic;
        private AmbientBubblesLogic ambient_logic;
        private PopLogic pop_logic;
        private FinalPopLogic final_pop;

        private float boost;
        private float[] camera_distances;
        public int[] num_bubbles;
        public int num_collected;
        public float hurt;

        private static Random rand = new Random();

        public Character(Entity player, InputDevice input)
            : base(0.2f)
        {
            this.player = player;
            this.input = input;
            this.max_speed = 2;
            this.alpha = 1;
            this.camera_distances = new float[] { 3, 3, 4, 5 };
            this.num_bubbles = new int[] { 5, 8, 10, 0 };
            this.num_collected = 0;
        
            tail_logic = new BubbleTailLogic(this, 3);
            ambient_logic = new AmbientBubblesLogic(this, 50, 10);
            boost_logic = new BoostLogic(this, 5);
            hurt_logic = new HurtLogic(this, 5);
            pop_logic = new PopLogic(this, 5);
            final_pop = new FinalPopLogic(this, 45);
        }

        public override void Update(double elapsed)
        {
            if (popped)
                return;

            hurt -= (float)elapsed;
            if (hurt < 0)
                hurt = 0;

            float size = (curr_size + 1);

            if (num_collected >= num_bubbles[curr_size] && curr_size != 3)
            {
                IncreaseSize();
                ParticleManager particlemanager = ParticleManager.GetInstance();
                particlemanager.Explode(position, 3);
                num_collected = 0;
            }

            if (curr_size == 3 && !player.removed)
            {
                Birth();
            }

            //set position [physics to entity]
            position.X = MathHelper.Lerp(position.X, body.position.X, 0.12f);
            position.Y = MathHelper.Lerp(position.Y, body.position.Y, 0.12f);

            //set camera position
            player.position.X = position.X;
            player.position.Y = position.Y;
            player.position.Z = MathHelper.Lerp(player.position.Z, camera_distances[curr_size], 0.01f);

            //if (input.IsPressed(Buttons.B))
            //    player.position.Z = 125;

            // set velocity [camera to bubble]
            velocity.X = player.velocity.X;
            velocity.Y = player.velocity.Y;

            //apply force [entity to physics]
            body.velocity.X += (float)(velocity.X * elapsed) * 3;
            body.velocity.Y += (float)(velocity.Y * elapsed) * 3;


            boost -= ((float)elapsed);
            boost = MathHelper.Clamp(boost, 0, 1);

            if (boost == 0 && input.IsPressed(Buttons.A))
            {
                if (num_collected > 0)
                {
                    num_collected--;

                    boost_logic.Burst();
                    ParticleManager.GetInstance().Explode(position, 10);
                    boost_logic.Burst();

                    boost = 1.5f;
                }
                else if (curr_size > 0)
                {
                    DecreaseSize();
                    num_collected = num_bubbles[curr_size] - 1;

                    boost_logic.Burst();
                    ParticleManager.GetInstance().Explode(position, 10);
                    boost_logic.Burst();

                    boost = 1.5f;
                }
            }

            //limit speed to max_speed
            velocity.X = body.velocity.X ;
            velocity.Y = body.velocity.Y;
            velocity.Z = 0;

            float speed = body.velocity.Length();
            speed = MathHelper.Clamp(speed + boost, -(max_speed + boost), (max_speed + boost));
            if (speed != 0)
                velocity.Normalize();

            velocity.X = speed * velocity.X;
            velocity.Y = speed * velocity.Y;

            body.velocity.X = velocity.X;
            body.velocity.Y = velocity.Y;


            if (speed > 1f)
                tail_logic.Emit();

        }

        public override void Draw(GraphicsDevice device, Camera camera)
        {
            if (popped)
                return;

            base.Draw(device, camera);
        }

        public void Birth()
        {
            hurt = 10;
            player.removed = true;
            num_collected = 0;

            TimerCollection.GetInstance().Create(5, false, (t) => {
                player.removed = false;
                player.velocity = Vector3.Zero;
                velocity = Vector3.Zero;
                final_pop.Burst();
                SetSize(0); 
                body.Update(0);
                Resources.droplet_sound.Play(0.025f, ((float)rand.NextDouble() - 0.5f), 0);
            });
        }

        public void Hurt()
        {
            if (curr_size > 0 || num_collected > 0)
            {
                hurt = 5;
                num_collected = 0;

                hurt_logic.Burst();
                ParticleManager.GetInstance().Explode(position, 10);
                hurt_logic.Burst();

                DecreaseSize();
                Resources.droplet_sound.Play(0.025f, ((float)rand.NextDouble() - 0.5f), 0);
            }
            else
            {
                pop_logic.Burst();
                player.velocity = Vector3.Zero;
                player.removed = true;
                popped = true;
                hurt = 5;
                Resources.droplet_sound.Play(0.025f, ((float)rand.NextDouble() - 0.5f), 0);

                TimerCollection.GetInstance().Create(5, false, (t) => { hurt = 5; popped = false; player.removed = false; SetPosition(new Vector2(0, 0)); body.Update(0); });
            }
        }
    }
}
