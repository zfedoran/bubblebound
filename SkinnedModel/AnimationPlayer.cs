#region File Description
//-----------------------------------------------------------------------------
// AnimationPlayer.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
#endregion

namespace SkinnedModel
{
    /// <summary>
    /// The animation player is in charge of decoding bone position
    /// matrices from an animation clip.
    /// </summary>
    public class AnimationPlayer
    {

        // Information about the currently playing animation clip.
        AnimationClip currentClipValue;
        TimeSpan currentTimeValue;
        int currentKeyframe;


        // Current animation transform matrices.
        Matrix[] bone_matrices;
        Matrix[] world_matrices;
        Matrix[] skin_matrices;
        Matrix[] keyframe_matrices;

        Transform[] bone_transforms;
        float curr_keyframe_time;
        float prev_keyframe_time;

        // Backlink to the bind pose and skeleton hierarchy data.
        SkinningData skinningDataValue;


        /// <summary>
        /// Constructs a new animation player.
        /// </summary>
        public AnimationPlayer(SkinningData skinningData)
        {
            if (skinningData == null)
                throw new ArgumentNullException("skinningData");

            skinningDataValue = skinningData;

            bone_matrices = new Matrix[skinningData.BindPose.Count];
            world_matrices = new Matrix[skinningData.BindPose.Count];
            skin_matrices = new Matrix[skinningData.BindPose.Count];
            keyframe_matrices = new Matrix[skinningData.BindPose.Count];

            bone_transforms = new Transform[skinningData.BindPose.Count];

            skinningDataValue.BindPose.CopyTo(bone_matrices, 0);

            for (int i = 0; i < bone_matrices.Length; i++)
                bone_transforms[i] = new Transform(bone_matrices[i]);
        }


        /// <summary>
        /// Starts decoding the specified animation clip.
        /// </summary>
        public void StartClip(AnimationClip clip)
        {
            if (clip == null)
                throw new ArgumentNullException("clip");

            currentClipValue = clip;
            currentTimeValue = TimeSpan.Zero;
            currentKeyframe = 0;

            // Initialize bone transforms to the bind pose.
            skinningDataValue.BindPose.CopyTo(bone_matrices, 0);
        }


        /// <summary>
        /// Advances the current animation position.
        /// </summary>
        public void Update(TimeSpan time, bool relativeToCurrentTime,
                           Matrix rootTransform)
        {
            UpdateBoneTransforms(time, relativeToCurrentTime);
            UpdateWorldTransforms(rootTransform);
            UpdateSkinTransforms();
        }


        /// <summary>
        /// Helper used by the Update method to refresh the BoneTransforms data.
        /// </summary>
        public void UpdateBoneTransforms(TimeSpan time, bool relativeToCurrentTime)
        {
            if (currentClipValue == null)
                throw new InvalidOperationException(
                            "AnimationPlayer.Update was called before StartClip");

            // Update the animation position.
            if (relativeToCurrentTime)
            {
                time += currentTimeValue;

                // If we reached the end, loop back to the start.
                while (time >= currentClipValue.Duration)
                    time -= currentClipValue.Duration;
            }

            if ((time < TimeSpan.Zero))
                throw new ArgumentOutOfRangeException("time");

            // If the position moved backwards, reset the keyframe index.
            if (time < currentTimeValue)
            {
                currentKeyframe = 0;
                skinningDataValue.BindPose.CopyTo(bone_matrices, 0);
            }

            currentTimeValue = time;

            // Read keyframe matrices.
            IList<Keyframe> keyframes = currentClipValue.Keyframes;

            while (currentKeyframe < keyframes.Count)
            {
                Keyframe keyframe = keyframes[currentKeyframe];

                // Stop when we've read up to the current time position.
                if (keyframe.Time > currentTimeValue)
                    break;

               // float keyframe_time = (float)(keyframe.Time.TotalSeconds);
               // if(keyframe_time > curr_keyframe_time)
               // {
               //     prev_keyframe_time = curr_keyframe_time;
               //     curr_keyframe_time = keyframe_time;
               // }

                keyframe_matrices[keyframe.Bone] = keyframe.Transform;

                currentKeyframe++;
            }

            //float curr_time = (float)currentTimeValue.TotalSeconds;
            //float amount = 0;

            //if(curr_keyframe_time != prev_keyframe_time)
            //   amount = ((curr_time - curr_keyframe_time) / (curr_keyframe_time - prev_keyframe_time));

            float amount = 0.1f;
            for (int i = 0; i < bone_matrices.Length; i++)
            {
                Transform transform = new Transform(ref keyframe_matrices[i]);
                Transform.Lerp(ref bone_transforms[i], ref transform, amount, out bone_transforms[i]);
                bone_transforms[i].GetMatrix(out bone_matrices[i]);
            }
        }


        /// <summary>
        /// Helper used by the Update method to refresh the WorldTransforms data.
        /// </summary>
        public void UpdateWorldTransforms(Matrix rootTransform)
        {
            // Root bone.
            world_matrices[0] = bone_matrices[0] * rootTransform;

            // Child bones.
            for (int bone = 1; bone < world_matrices.Length; bone++)
            {
                int parentBone = skinningDataValue.SkeletonHierarchy[bone];

                world_matrices[bone] = bone_matrices[bone] *
                                             world_matrices[parentBone];
            }
        }


        /// <summary>
        /// Helper used by the Update method to refresh the SkinTransforms data.
        /// </summary>
        public void UpdateSkinTransforms()
        {
            for (int bone = 0; bone < skin_matrices.Length; bone++)
            {
                skin_matrices[bone] = skinningDataValue.InverseBindPose[bone] *
                                            world_matrices[bone];
            }
        }


        /// <summary>
        /// Gets the current bone transform matrices, relative to their parent bones.
        /// </summary>
        public Matrix[] GetBoneTransforms()
        {
            return bone_matrices;
        }


        /// <summary>
        /// Gets the current bone transform matrices, in absolute format.
        /// </summary>
        public Matrix[] GetWorldTransforms()
        {
            return world_matrices;
        }


        /// <summary>
        /// Gets the current bone transform matrices,
        /// relative to the skinning bind pose.
        /// </summary>
        public Matrix[] GetSkinTransforms()
        {
            return skin_matrices;
        }


        /// <summary>
        /// Gets the clip currently being decoded.
        /// </summary>
        public AnimationClip CurrentClip
        {
            get { return currentClipValue; }
        }


        /// <summary>
        /// Gets the current play position.
        /// </summary>
        public TimeSpan CurrentTime
        {
            get { return currentTimeValue; }
        }
    }
}
