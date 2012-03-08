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

namespace hyades.graphics
{
    public struct ModelInfo
    {
        public Model model;
        public Matrix transform;

        public ModelInfo(Model model, ref Matrix transform)
        { this.model = model; this.transform = transform; }

        public override string ToString()
        { return string.Format("{{model:[{0}] transform:[{1}]}}", model, transform); }
    }

    public class ModelRenderer 
    {
        private Dictionary<Model, List<ModelInfo>> models;
        private bool has_begun;
        private GraphicsDevice device;
        private Camera camera;
        private Color color;

        private bool use_instancing = true;
        private Matrix[] instanceTransforms;
        private DynamicVertexBuffer instanceVertexBuffer;

        // vertex type which encodes 4x4 matrices as a set of four Vector4 values.
        private static VertexDeclaration instanceVertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0),
            new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 1),
            new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 2),
            new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 3)
        );

        public ModelRenderer(GraphicsDevice device)
        { this.models = new Dictionary<Model, List<ModelInfo>>(); this.device = device; }

        public void Add(Model model, float x, float y, float z, float scale, float rotation_x, float rotation_y, float rotation_z)
        {
            float a, b, c, d, e, f;

            Matrix matrix = Matrix.Identity;

            a = (float)Math.Cos(rotation_x);
            b = (float)Math.Sin(rotation_x);

            c = (float)Math.Cos(rotation_y);
            d = (float)Math.Sin(rotation_y);

            e = (float)Math.Cos(rotation_z);
            f = (float)Math.Sin(rotation_z);

            matrix.M11 = (c * e) * scale;
            matrix.M12 = (c * f) * scale;
            matrix.M13 = -d * scale;
            matrix.M21 = (e * b * d - a * f) * scale;
            matrix.M22 = ((e * a) + (f * b * d)) * scale;
            matrix.M23 = (b * c) * scale;
            matrix.M31 = (e * a * d + b * f) * scale;
            matrix.M33 = (a * c) * scale;
            matrix.M32 = -(b * e - f * a * d) * scale;
            matrix.M41 = x;
            matrix.M42 = y;
            matrix.M43 = z;

            Add(model, ref matrix);
        }

        public void Add(Model model, ref Matrix transform)
        {
            ModelInfo modelinfo = new ModelInfo(model, ref transform);

            if (!models.ContainsKey(model))
                models[model] = new List<ModelInfo>();
            models[model].Add(modelinfo);
        }

        public void Begin(Camera camera)
        { Begin(camera, Color.White); }

        public void Begin(Camera camera, Color color)
        {
            if (has_begun)
                throw new InvalidOperationException("End must be called before Begin can be called again.");

            if (camera == null)
                throw new InvalidOperationException("Camera cannot be null!");

            this.camera = camera;
            this.has_begun = true;
            this.color = color;
        }

        public void End()
        {
            if (!has_begun)
                throw new InvalidOperationException("Begin must be called before End can be called.");

            DrawModels();

            this.has_begun = false;
        }


        private void DrawModels()
        {
            foreach (Model model in models.Keys)
            {
                List<ModelInfo> list = models[model];

                if (list.Count <= 0)
                    continue;

                Matrix[] instancedModelBones = new Matrix[model.Bones.Count];
                model.CopyAbsoluteBoneTransformsTo(instancedModelBones);

                // Gather instance transform matrices into a single array.
                Array.Resize(ref instanceTransforms, list.Count);

                for (int i = 0; i < list.Count; i++)
                {
                    instanceTransforms[i] = list[i].transform;
                }

                // Draw all the instances, using the currently selected rendering technique.
                if (use_instancing)
                    DrawModelHardwareInstancing(model, instancedModelBones, instanceTransforms);
                else
                    DrawModelNoInstancing(model, instancedModelBones, instanceTransforms);

                list.Clear();
            }
        }


        private void SetEffectParameters(Effect effect)
        {
            effect.Parameters["FarClip"].SetValue(camera.far);
            effect.Parameters["View"].SetValue(camera.view);
            effect.Parameters["Projection"].SetValue(camera.projection);
            effect.Parameters["Color"].SetValue(color.ToVector3());
            effect.Parameters["FogEnabled"].SetValue(1);
            effect.Parameters["FogStart"].SetValue(camera.near);
            effect.Parameters["FogEnd"].SetValue(camera.far);
            effect.Parameters["DesaturateEnabled"].SetValue(1);
        }

        /// <summary>
        /// Efficiently draws several copies of a piece of geometry using hardware instancing.
        /// </summary>
        private void DrawModelHardwareInstancing(Model model, Matrix[] modelBones, Matrix[] instances)
        {
            if (instances.Length == 0)
                return;

            // If we have more instances than room in our vertex buffer, grow it to the neccessary size.
            if ((instanceVertexBuffer == null) || (instances.Length > instanceVertexBuffer.VertexCount))
            {
                if (instanceVertexBuffer != null)
                    instanceVertexBuffer.Dispose();

                instanceVertexBuffer = new DynamicVertexBuffer(device, instanceVertexDeclaration, instances.Length, BufferUsage.WriteOnly);
            }

            // Transfer the latest instance transform matrices into the instanceVertexBuffer.
            instanceVertexBuffer.SetData(instances, 0, instances.Length, SetDataOptions.Discard);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    // Tell the GPU to read from both the model vertex buffer plus our instanceVertexBuffer.
                    device.SetVertexBuffers
                    (
                        new VertexBufferBinding(meshPart.VertexBuffer, meshPart.VertexOffset, 0),
                        new VertexBufferBinding(instanceVertexBuffer, 0, 1)
                    );

                    device.Indices = meshPart.IndexBuffer;

                    // Set up the instance rendering effect.
                    Effect effect = meshPart.Effect;

                    effect.CurrentTechnique = effect.Techniques["HardwareInstancing"];
                    effect.Parameters["World"].SetValue(modelBones[mesh.ParentBone.Index]);
                    SetEffectParameters(effect);

                    // Draw all the instance copies in a single call.
                    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        device.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0,
                                                       meshPart.NumVertices, meshPart.StartIndex,
                                                       meshPart.PrimitiveCount, instances.Length);
                    }
                }
            }
        }


        /// <summary>
        /// Draws several copies of a piece of geometry without using any
        /// special GPU instancing techniques at all. This just does a
        /// regular loop and issues several draw calls one after another.
        /// </summary>
        private void DrawModelNoInstancing(Model model, Matrix[] modelBones, Matrix[] instances)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    device.SetVertexBuffer(meshPart.VertexBuffer, meshPart.VertexOffset);
                    device.Indices = meshPart.IndexBuffer;

                    
                    // Set up the rendering effect.
                    Effect effect = meshPart.Effect;

                    effect.CurrentTechnique = effect.Techniques["NoInstancing"];
                    SetEffectParameters(effect);

                    EffectParameter transformParameter = effect.Parameters["World"];

                    // Draw a single instance copy each time around this loop.
                    for (int i = 0; i < instances.Length; i++)
                    {
                        transformParameter.SetValue(modelBones[mesh.ParentBone.Index] * instances[i]);

                        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                        {
                            pass.Apply();

                            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
                                                         meshPart.NumVertices, meshPart.StartIndex,
                                                         meshPart.PrimitiveCount);
                        }
                    }
                }
            }
        }

        private static ModelRenderer instance;

        public static ModelRenderer GetInstance(GraphicsDevice device)
        {
            if (instance == null)
                instance = new ModelRenderer(device);
            return instance; 
        }

    }
}
