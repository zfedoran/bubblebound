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

namespace hyades.graphics
{
    public class Font
    {
        //public const string char_set = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 _.,:;!?'\"`~()[]{}|/\\*+-<=>^#%$@&";
        public readonly string char_set = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";

        public Texture2D texture;
        public TextureRegion[] regions;
        public float height;

        public float GetStringLength(string text, float scale, float kerning)
        {
            TextureRegion region; float offset = 0;
            for (int i = 0; i < text.Length; i++)
            {
                region = regions[char_set.IndexOf(text[i])];
                offset += region.width * scale + kerning;
            }

            return offset;
        }
    }

    public class FontProcessor
    {
        private static readonly Color clip_color = new Color(255, 0, 255, 255);

        public static Font Build(Texture2D texture)
        {
            Texture2D clone = new Texture2D(texture.GraphicsDevice, texture.Width, texture.Height);

            Color[] data = new Color[texture.Width * texture.Height];
            texture.GetData<Color>(data);

            Font font = new Font();
            font.regions = CreateRegionsFromTexture(clone, data);
            font.texture = clone;
            font.height = font.regions[0].height;

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == clip_color)
                    data[i] = Color.Transparent;
            }

            clone.SetData<Color>(data);

            return font;
        }

        private static int GetFirstSpriteHeight(Texture2D texture, Color[] data)
        {
            Color curr_color, prev_color; bool begin, end, sprite_found;
            int height, texture_width, texture_height;

            texture_width = texture.Width;
            texture_height = texture.Height;
            height = 0;
            sprite_found = false;
            prev_color = clip_color;

            for (int i = 0; i < texture_width; i++)
            {
                for (int j = 0; j < texture_height; j++)
                {
                    curr_color = data[i + j * texture.Width];

                    begin = (curr_color != clip_color && prev_color == clip_color);
                    end = (curr_color == clip_color && prev_color != clip_color);

                    if (begin)
                        sprite_found = true;

                    if (end)
                        return height;

                    if (sprite_found)
                        height++;

                    prev_color = curr_color;
                }
            }

            return height;
        }

        private static TextureRegion[] CreateRegionsFromTexture(Texture2D texture, Color[] data)
        {
            List<TextureRegion> regions_list = new List<TextureRegion>();
            TextureRegion region;
            Color curr_color, prev_color;
            bool begin, end, curr_clear, prev_clear;
            int height, offset_y, offset_x;
            int texture_width, texture_height;
            int curr_region;
            int sprites_per_row = 0;

            texture_width = texture.Width;
            texture_height = texture.Height;

            height = GetFirstSpriteHeight(texture, data);

            offset_x = offset_y = 0; prev_clear = true; prev_color = clip_color;
            for (int j = 0; j < texture_height; j++)
            {
                curr_clear = true;
                for (int i = 0; i < texture_width; i++)
                {
                    curr_color = data[i + j * texture_width];

                    begin = (curr_color != clip_color && prev_color == clip_color);
                    end = (curr_color == clip_color && prev_color != clip_color);
                    curr_clear = (curr_clear && curr_color == clip_color);

                    if (begin && prev_clear)
                    {
                        if (offset_y == 0 && prev_clear)
                            sprites_per_row++;

                        region = new TextureRegion();
                        region.texture = texture;
                        region.height = height;
                        region.u = i;
                        if (prev_clear)
                            region.v = j;

                        regions_list.Add(region);
                    }
                    if (end && prev_clear)
                    {
                        curr_region = offset_x + offset_y * sprites_per_row;
                        region = regions_list[curr_region];
                        region.width = i - region.u;
                        regions_list[curr_region] = region;

                        offset_x++;
                    }

                    prev_color = curr_color;

                }

                offset_x = 0;
                if ((!prev_clear) && curr_clear)
                    offset_y++;

                prev_clear = curr_clear;
            }

            return regions_list.ToArray();
        }
    }
}
