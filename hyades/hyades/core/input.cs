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
using Microsoft.Xna.Framework.Input;

namespace hyades
{
    public abstract class InputDevice
    {
        public Vector2 LeftThumbStick;
        public Vector2 RightThumbStick;

        public float LeftTrigger;
        public float RightTrigger;

        public abstract void Update();
        public abstract bool IsPressed(Buttons button);
        public abstract bool IsDown(Buttons button);
        public abstract bool IsUp(Buttons button);
    }

    public class GamePadDevice : InputDevice
    {
        private GamePadState prev;
        private GamePadState curr;

        public PlayerIndex playerIndex;

        public GamePadDevice(PlayerIndex playerIndex)
        {
            this.playerIndex = playerIndex;
        }

        public override void Update()
        {
            prev = curr;
            curr = GamePad.GetState(playerIndex);
            LeftThumbStick = curr.ThumbSticks.Left;
            RightThumbStick = curr.ThumbSticks.Right;
            LeftTrigger = curr.Triggers.Left;
            RightTrigger = curr.Triggers.Right;
        }

        public override bool IsPressed(Buttons button)
        {
            return curr.IsButtonDown(button) && prev.IsButtonUp(button);
        }

        public override bool IsDown(Buttons button)
        {
            return curr.IsButtonDown(button);
        }

        public override bool IsUp(Buttons button)
        {
            return curr.IsButtonUp(button);
        }
    }

    public class KeyboardDevice : InputDevice
    {
        private KeyboardState prev_keyboard;
        private KeyboardState curr_keyboard;
        private Dictionary<Buttons, Keys> button_map;

        public KeyboardDevice()
        {
            button_map = new Dictionary<Buttons, Keys>();
            button_map[Buttons.A] = Keys.Enter;
            button_map[Buttons.B] = Keys.B;
            button_map[Buttons.X] = Keys.X;
            button_map[Buttons.Y] = Keys.Y;
            button_map[Buttons.DPadUp] = Keys.Up;
            button_map[Buttons.DPadDown] = Keys.Down;
            button_map[Buttons.DPadLeft] = Keys.Left;
            button_map[Buttons.DPadRight] = Keys.Right;
            button_map[Buttons.LeftTrigger] = Keys.LeftControl;
            button_map[Buttons.RightTrigger] = Keys.RightControl;
            button_map[Buttons.Start] = Keys.Escape;
        }

        public override void Update()
        {
            prev_keyboard = curr_keyboard;
            curr_keyboard = Keyboard.GetState();

            LeftThumbStick = new Vector2((curr_keyboard.IsKeyDown(Keys.A) ? -1 : 0) + (curr_keyboard.IsKeyDown(Keys.D) ? 1 : 0), (curr_keyboard.IsKeyDown(Keys.W) ? 1 : 0) + (curr_keyboard.IsKeyDown(Keys.S) ? -1 : 0));
            RightThumbStick = new Vector2((curr_keyboard.IsKeyDown(Keys.Left) ? -1 : 0) + (curr_keyboard.IsKeyDown(Keys.Right) ? 1 : 0), (curr_keyboard.IsKeyDown(Keys.Up) ? 1 : 0) + (curr_keyboard.IsKeyDown(Keys.Down) ? -1 : 0));
            LeftTrigger = curr_keyboard.IsKeyDown(Keys.Space) ? 1 : 0;
            RightTrigger = curr_keyboard.IsKeyDown(Keys.LeftControl) ? 1 : 0;
        }

        public override bool IsPressed(Buttons button)
        {
            if (button == Buttons.A)
                if (curr_keyboard.IsKeyDown(Keys.Space) && prev_keyboard.IsKeyUp(Keys.Space))
                    return true;

            Keys key = button_map[button];
            return curr_keyboard.IsKeyDown(key) && prev_keyboard.IsKeyUp(key);
        }

        public override bool IsDown(Buttons button)
        {
            Keys key = button_map[button];
            return curr_keyboard.IsKeyDown(key);
        }

        public override bool IsUp(Buttons button)
        {
            Keys key = button_map[button];
            return curr_keyboard.IsKeyUp(key);
        }
    }

    public class MultiInputDevice : InputDevice
    {
        private GamePadState prev;
        private GamePadState curr;
        private PlayerIndex playerIndex;
        private KeyboardState prev_keyboard;
        private KeyboardState curr_keyboard;
        private MouseState curr_mouse;
        private MouseState prev_mouse;
        private int x, y;
        private Dictionary<Buttons, Keys> button_map;

        public MultiInputDevice(PlayerIndex playerIndex)
        {
            this.playerIndex = playerIndex;
            x = Application.WIDTH/2;
            y = Application.HEIGHT/2;

            button_map = new Dictionary<Buttons, Keys>();
            button_map[Buttons.A] = Keys.Enter;
            button_map[Buttons.Back] = Keys.Enter;
            button_map[Buttons.B] = Keys.B;
            button_map[Buttons.X] = Keys.X;
            button_map[Buttons.Y] = Keys.Y;
            button_map[Buttons.DPadUp] = Keys.Up;
            button_map[Buttons.DPadDown] = Keys.Down;
            button_map[Buttons.DPadLeft] = Keys.Left;
            button_map[Buttons.DPadRight] = Keys.Right;
            button_map[Buttons.LeftTrigger] = Keys.LeftControl;
            button_map[Buttons.RightTrigger] = Keys.RightControl;
            button_map[Buttons.Start] = Keys.Escape;
        }

        public override void Update()
        {
            prev = curr;
            curr = GamePad.GetState(playerIndex);
            LeftThumbStick = curr.ThumbSticks.Left;
            RightThumbStick = curr.ThumbSticks.Right;
            LeftTrigger = curr.Triggers.Left;
            RightTrigger = curr.Triggers.Right;

            prev_keyboard = curr_keyboard;
            curr_keyboard = Keyboard.GetState();

            LeftThumbStick += new Vector2((curr_keyboard.IsKeyDown(Keys.A) ? -1 : 0) + (curr_keyboard.IsKeyDown(Keys.D) ? 1 : 0), (curr_keyboard.IsKeyDown(Keys.W) ? 1 : 0) + (curr_keyboard.IsKeyDown(Keys.S) ? -1 : 0));
            RightThumbStick += new Vector2((curr_keyboard.IsKeyDown(Keys.Left) ? -1 : 0) + (curr_keyboard.IsKeyDown(Keys.Right) ? 1 : 0), (curr_keyboard.IsKeyDown(Keys.Up) ? 1 : 0) + (curr_keyboard.IsKeyDown(Keys.Down) ? -1 : 0));            
            LeftTrigger += curr_keyboard.IsKeyDown(Keys.Space) ? 1 : 0;
            RightTrigger += curr_keyboard.IsKeyDown(Keys.LeftControl) ? 1 : 0;

            Vector2.Clamp(LeftThumbStick,-Vector2.One,Vector2.One);
            Vector2.Clamp(RightThumbStick,-Vector2.One,Vector2.One);
            MathHelper.Clamp(LeftTrigger,-1,1);
            MathHelper.Clamp(RightTrigger,-1,1);

            prev_mouse = curr_mouse;
            curr_mouse = Mouse.GetState();
            /*if (prev_mouse != curr_mouse)
            {
                float dx = MathHelper.Clamp(curr_mouse.X - prev_mouse.X,-1, 1);
                float dy = MathHelper.Clamp(curr_mouse.Y - prev_mouse.Y,-1, 1);
                Mouse.SetPosition(x, y);

                RightThumbStick = new Vector2(dx, dy);
            }*/
            
            
        }

        public override bool IsPressed(Buttons button)
        {
            if(curr.IsButtonDown(button) && prev.IsButtonUp(button))
                return true;

            if (button == Buttons.A)
                if (curr_mouse.LeftButton == ButtonState.Pressed && prev_mouse.LeftButton == ButtonState.Released)
                    return true;

            if (button == Buttons.A || button == Buttons.B)
                if (curr_keyboard.IsKeyDown(Keys.Space) && prev_keyboard.IsKeyUp(Keys.Space))
                    return true;

            Keys key = button_map[button];
            return curr_keyboard.IsKeyDown(key) && prev_keyboard.IsKeyUp(key);
        }

        public override bool IsDown(Buttons button)
        {
            Keys key = button_map[button];
            return curr_keyboard.IsKeyDown(key) || curr.IsButtonDown(button);
        }

        public override bool IsUp(Buttons button)
        {
            Keys key = button_map[button];
            return curr_keyboard.IsKeyUp(key) || curr.IsButtonUp(button);
        }
    }
}
