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
using Microsoft.Xna.Framework.Content;
using hyades.graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;

namespace hyades
{
    public static class Resources
    {        
        public static Texture2D title_texture;
        public static Texture2D bubble_texture;
        public static Texture2D bubble_reflection_texture;
        public static Texture2D ray_texture;
        public static Texture2D logo_texture;
        public static Texture2D instructions_texture;

        public static Font arial10px_font;
        public static Font description_font;
        public static Font option_font;
        public static Font title_font;

        public static TextureRegion particle_texture_region;
        public static TextureRegion white_texture_region;
        public static TextureRegion logo_texture_region;
        public static TextureRegion instructions_texture_region;
        public static TextureRegion bubble_reflection_texture_region;
        public static TextureRegion bubble_texture_region;
        public static TextureRegion controls_region;
        public static TextureRegion hud_on, hud_off;

        public static Effect spriterenderer_material;
        public static Effect postprocessor_blur;
        public static Effect postprocessor_dof;
        public static Effect postprocessor_scale;
        public static Effect postprocessor_extract;
        public static Effect postprocessor_bloom;

        public static Model box_model;
        public static Model rock_model;
        public static Model skybox_model;
        public static Model seaurchin_model;
        public static Model fish_model;
        public static Model octopus_model;
        public static Model starfish_model;

        public static SoundEffect background_music;
        public static SoundEffect droplet_sound;

        private static Dictionary<string, object> name_to_object_mapping;
        private static Dictionary<object, string> object_to_name_mapping;

        public static void Load(ContentManager content)
        {
            background_music = content.Load<SoundEffect>("music/background");
            droplet_sound = content.Load<SoundEffect>("music/droplet");

            bubble_texture = content.Load<Texture2D>("textures/bubble_small");
            bubble_reflection_texture = content.Load<Texture2D>("textures/bubble_reflection_small");
            title_texture = content.Load<Texture2D>("textures/title");
            ray_texture = content.Load<Texture2D>("textures/bg-light-0001");
            logo_texture = content.Load<Texture2D>("textures/logo");
            instructions_texture = content.Load<Texture2D>("textures/instructions");

            instructions_texture_region = new TextureRegion(instructions_texture, 0, 0, 813, 360);
            logo_texture_region = new TextureRegion(logo_texture, 0, 0, 340, 270);
            white_texture_region = new TextureRegion(title_texture, 17 * 16 + 8, 8, 1, 1);
            particle_texture_region = new TextureRegion(title_texture, 18 * 16+1, 1, 14, 14);
            bubble_reflection_texture_region = new TextureRegion(bubble_reflection_texture, 0, 0, 128, 128);
            bubble_texture_region = new TextureRegion(bubble_texture, 0, 0, bubble_texture.Width, bubble_texture.Height);
            controls_region = new TextureRegion(title_texture, 0, 96, 512, 320);
            hud_on = new TextureRegion(title_texture, 96, 416, 96, 96);
            hud_off = new TextureRegion(title_texture, 0, 416, 96, 96);

            arial10px_font = FontProcessor.Build(content.Load<Texture2D>("fonts/arial10px"));
            description_font = FontProcessor.Build(content.Load<Texture2D>("fonts/aharoni15px_bold"));
            option_font = FontProcessor.Build(content.Load<Texture2D>("fonts/aharoni28px_bold"));
            title_font = FontProcessor.Build(content.Load<Texture2D>("fonts/aharoni48px_bold"));

            spriterenderer_material = content.Load<Effect>("shaders/spriterenderer/default");
            postprocessor_blur = content.Load<Effect>("shaders/postprocessor/dof_blur");
            postprocessor_dof = content.Load<Effect>("shaders/postprocessor/dof");
            postprocessor_scale = content.Load<Effect>("shaders/postprocessor/dof_scale");
            postprocessor_extract = content.Load<Effect>("shaders/postprocessor/bloom_extract");
            postprocessor_bloom = content.Load<Effect>("shaders/postprocessor/bloom_combine");

            box_model = content.Load<Model>("models/box");
            rock_model = content.Load<Model>("models/rock");
            skybox_model = content.Load<Model>("models/skybox");
            seaurchin_model = content.Load<Model>("models/urchin");
            fish_model = content.Load<Model>("models/nemo");
            octopus_model = content.Load<Model>("models/octopus");
            starfish_model = content.Load<Model>("models/starfish");

            name_to_object_mapping = new Dictionary<string, object>();
            object_to_name_mapping = new Dictionary<object, string>();

            Add("models/box", box_model);
            Add("models/rock", rock_model);
            Add("models/skybox", skybox_model);
            Add("models/urchin", seaurchin_model);
            Add("models/nemo", fish_model);
            Add("models/octopus", octopus_model);
            Add("models/starfish", starfish_model);
            Add("textures/bg-light-0001", ray_texture);
        }

        private static void Add(string name, object o)
        {
            object_to_name_mapping.Add(o, name);
            name_to_object_mapping.Add(name, o);
        }

        public static void Unload()
        { }

        public static object GetObject(string name)
        {
            return name_to_object_mapping[name];
        }

        public static string GetName(object o)
        {
            return object_to_name_mapping[o];
        }
    }
}
