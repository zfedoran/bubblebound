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
using hyades.screen;

namespace hyades.graphics
{
	public struct TextureRegion
	{
		public Texture2D texture;
		public int u, v, width, height;

		public TextureRegion(Texture2D texture, int u, int v, int width, int height)
		{ this.texture = texture; this.u = u; this.v = v; this.width = width; this.height = height; }

		public override string ToString()
		{ return string.Format("{{u:[{0}] v:[{1}] width:[{2}] height:[{3}]}}", u, v, width, height); }
	}

	public struct SpriteInfo
	{
		public TextureRegion region;
		public Color color;
		public float x, y, z, width, height;
		public float rotation_x, rotation_y, rotation_z;
		public int index; 

		// 2d sprite
		public SpriteInfo(TextureRegion region, Color color, float x, float y, float width, float height, float rotation, int index)
		{ this.region = region; this.color = color; this.x = x; this.y = y; this.z = 0; this.width = width; this.height = height; this.index = index; this.rotation_x = 0; this.rotation_y = 0; this.rotation_z = rotation; }

		// 3d sprite
		public SpriteInfo(TextureRegion region, Color color, float x, float y, float z, float width, float height, float rotation_x, float rotation_y, float rotation_z, int index)
		{ this.region = region; this.color = color; this.x = x; this.y = y; this.z = z; this.width = width; this.height = height; this.index = index; this.rotation_x = rotation_x; this.rotation_y = rotation_y; this.rotation_z = rotation_z; }

		public override string ToString()
		{ return string.Format("{{x:[{0}] y:[{1}] z:[{2}] width:[{3}] height:[{4}] rotation:[{5},{6},{7}] index:[{8}] color:[{9}] region:[{10}]}}", x, y, z, width, height, rotation_x, rotation_y, rotation_z, index, color, region); }
	}

	public class SpriteRenderer
	{
		private Effect material;
		private Matrix view, proj;
		private Camera camera;
		private GraphicsDevice device;
		private PrimitiveBatch primitivebatch;
		private Dictionary<Texture2D, List<SpriteInfo>> sprites;
		private bool has_begun, is_ortho;

		public SpriteRenderer(GraphicsDevice device)
		{ this.sprites = new Dictionary<Texture2D, List<SpriteInfo>>(); this.device = device; }

		private void Warm(GraphicsDevice device)
		{
			primitivebatch = new PrimitiveBatch(device);
			view = Matrix.Identity;
			proj = Matrix.CreateTranslation(-0.5f, -0.5f, 0) * Matrix.CreateOrthographicOffCenter(0, device.Viewport.Width, device.Viewport.Height, 0, 0, 1);
			material = Resources.spriterenderer_material;
		}

		public void Add(TextureRegion region, Color color, float x, float y, float width, float height, float rotation)
		{
			SpriteInfo sprite = new SpriteInfo(region, color, x, y, width, height, rotation, count);
			
			if (!sprites.ContainsKey(region.texture))
				sprites[region.texture] = new List<SpriteInfo>();
			sprites[region.texture].Add(sprite);

			count++;
		}

		public void Add(TextureRegion region, Color color, float x, float y, float z, float width, float height, float rotation_x, float rotation_y, float rotation_z)
		{
			SpriteInfo sprite = new SpriteInfo(region, color, x, y, z, width, height, rotation_x, rotation_y, rotation_z, count);

			if (!sprites.ContainsKey(region.texture))
				sprites[region.texture] = new List<SpriteInfo>();
			sprites[region.texture].Add(sprite);

			count++;
		}

		public void AddString(Font font, string text, float x, float y)
		{ AddString(font, text, x, y, 0, 1, 0, 0, 0, 0, Color.White); }

		public void AddString(Font font, string text, float x, float y, Color color)
		{ AddString(font, text, x, y, 0, 1, 0, 0, 0, 0, color); }

		public void AddString(Font font, string text, float x, float y, float z)
		{ AddString(font, text, x, y, z, 1, 0, 0, 0, 0, Color.White); }

		public void AddString(Font font, string text, float x, float y, float z, Color color)
		{ AddString(font, text, x, y, z, 1, 0, 0, 0, 0, color); }

		public void AddString(Font font, string text, float x, float y, float z, float scale, Color color)
		{ AddString(font, text, x, y, 0, scale, 0, 0, 0, 0, color); }

		public void AddString(Font font, string text, float x, float y, float z, float scale, float kerning, float rotation_x, float rotation_y, float rotation_z, Color color)
		{
			float a, b, c, d, e, f;

			float half_width = font.GetStringLength(text, scale, kerning) / 2;
			float half_height = font.regions[0].height / 2.0f * scale;

			Matrix matrix = Matrix.Identity;
			if (rotation_z != 0 || rotation_x != 0 || rotation_y != 0)
			{
				a = (float)Math.Cos(rotation_x);
				b = (float)Math.Sin(rotation_x);

				c = (float)Math.Cos(rotation_y);
				d = (float)Math.Sin(rotation_y);

				e = (float)Math.Cos(rotation_z);
				f = (float)Math.Sin(rotation_z);

				matrix.M11 = (c * e);
				matrix.M12 = (c * f);
				matrix.M13 = -d;
				matrix.M21 = (e * b * d - a * f);
				matrix.M22 = ((e * a) + (f * b * d));
				matrix.M23 = (b * c);
				matrix.M31 = (e * a * d + b * f);
				matrix.M33 = (a * c);
				matrix.M32 = -(b * e - f * a * d);
			}

			matrix.M41 = x + half_width;
			matrix.M42 = y + half_height;
			matrix.M43 = z;

			TextureRegion region; float offset = 0;
			for (int i = 0; i < text.Length; i++)
			{
				a = offset - half_width;
				b = -half_height;
				c = z;

				d = (((a * matrix.M11) + (b * matrix.M21)) + (c * matrix.M31)) + matrix.M41;
				e = (((a * matrix.M12) + (b * matrix.M22)) + (c * matrix.M32)) + matrix.M42;
				f = (((a * matrix.M13) + (b * matrix.M23)) + (c * matrix.M33)) + matrix.M43;

				region = font.regions[font.char_set.IndexOf(text[i])];
				Add(region, color, d, e, f, region.width * scale, region.height * scale, rotation_x, rotation_y, rotation_z);
				offset += region.width * scale + kerning;
			}
		}

		public void Begin(Camera camera)
		{
			if (has_begun)
				throw new InvalidOperationException("End must be called before Begin can be called again.");

			this.camera = camera;
			this.has_begun = true;
			this.is_ortho = camera == null;
		}

		public void End()
		{
			if (!has_begun)
				throw new InvalidOperationException("Begin must be called before End can be called.");

			if (primitivebatch == null || material == null)
				Warm(device);

			material.Parameters["TextureEnabled"].SetValue(true);
			material.Parameters["World"].SetValue(Matrix.Identity);

			if (is_ortho)
			{
				material.Parameters["View"].SetValue(view);
				material.Parameters["Projection"].SetValue(proj);
			}
			else
			{
				material.Parameters["View"].SetValue(camera.view);
				material.Parameters["Projection"].SetValue(camera.projection);
			}

			Matrix matrix;
			float a, b, c, d, e, f;

			foreach (Texture2D texture in sprites.Keys)
			{
				List<SpriteInfo> list = sprites[texture];

				if(list.Count <= 0)
					continue;

				material.Parameters["Texture"].SetValue(texture);
				material.CurrentTechnique.Passes[0].Apply();

				primitivebatch.Begin(Primitive.Quad);
				float texture_pixel_width = 1.0f / texture.Width;
				float texture_pixel_height = 1.0f / texture.Height;

				for (int i = 0; i < list.Count; i++)
				{
					SpriteInfo sprite = list[i];
					TextureRegion region = sprite.region;

					float index = 0;
					if (is_ortho)
					{
						index = 1 - (((float)(sprite.index)) / count);
						index *= -0.09f;
					}

					matrix = Matrix.Identity;
					if (sprite.rotation_z != 0 || sprite.rotation_x != 0 || sprite.rotation_y != 0)
					{
						a = (float)Math.Cos(sprite.rotation_x);
						b = (float)Math.Sin(sprite.rotation_x);

						c = (float)Math.Cos(sprite.rotation_y);
						d = (float)Math.Sin(sprite.rotation_y);

						e = (float)Math.Cos(sprite.rotation_z);
						f = (float)Math.Sin(sprite.rotation_z);

						matrix.M11 = (c * e);
						matrix.M12 = (c * f);
						matrix.M13 = -d;
						matrix.M21 = (e * b * d - a * f);
						matrix.M22 = ((e * a) + (f * b * d));
						matrix.M23 = (b * c);
						matrix.M31 = (e * a * d + b * f);
						matrix.M33 = (a * c);
						matrix.M32 = -(b * e - f * a * d);
					}

					matrix.M41 = sprite.x;
					matrix.M42 = sprite.y;
					matrix.M43 = sprite.z + index;

					primitivebatch.SetTransform(ref matrix);
					primitivebatch.SetColor(sprite.color);
					primitivebatch.SetTextureCoords(region.u * texture_pixel_width, region.v * texture_pixel_height);

					if (is_ortho) //in ortho the view is upside down
					{
						primitivebatch.AddVertex(0,                0,                 0, 0,                                      0);
						primitivebatch.AddVertex(0,                0 + sprite.height, 0, 0,                                      0 + region.height * texture_pixel_height);
						primitivebatch.AddVertex(0 + sprite.width, 0 + sprite.height, 0, 0 + region.width * texture_pixel_width, 0 + region.height * texture_pixel_height);
						primitivebatch.AddVertex(0 + sprite.width, 0,                 0, 0 + region.width * texture_pixel_width, 0);
					}
					else
					{
						primitivebatch.AddVertex(0,                0 + sprite.height, 0, 0,                                      0);
						primitivebatch.AddVertex(0,                0,                 0, 0,                                      0 + region.height * texture_pixel_height);
						primitivebatch.AddVertex(0 + sprite.width, 0,                 0, 0 + region.width * texture_pixel_width, 0 + region.height * texture_pixel_height);
						primitivebatch.AddVertex(0 + sprite.width, 0 + sprite.height, 0, 0 + region.width * texture_pixel_width, 0);
					}
				}
				
				primitivebatch.End();

				list.Clear();
			}

			has_begun = false;
		}


		//Static instance stuff
		private static int count;
		private static SpriteRenderer instance;

		public static SpriteRenderer GetInstance(GraphicsDevice device)
		{
			if (instance == null)
				instance = new SpriteRenderer(device);
			return instance; 
		}

		public static void Clear()
		{ count = 0; }
	}
}