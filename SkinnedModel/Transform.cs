using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SkinnedModel
{
    public struct Transform
    {
        public Vector3 translation;
        public Vector3 scale;
        public Quaternion rotation;

        public Transform(ref Matrix matrix)
        {
            matrix.Decompose(out scale, out rotation, out translation);

            if (scale.X > 0.9999f && scale.X <= 1.0001f)
                scale.X = 1;
            if (scale.Y > 0.9999f && scale.Y <= 1.0001f)
                scale.Y = 1;
            if (scale.Z > 0.9999f && scale.Z <= 1.0001f)
                scale.Z = 1;

#if DEBUG
            this.Validate();
#endif
        }

        public Transform(Matrix matrix)
        {
            matrix.Decompose(out scale, out rotation, out translation);

            if (scale.X > 0.9999f && scale.X <= 1.0001f)
                scale.X = 1;
            if (scale.Y > 0.9999f && scale.Y <= 1.0001f)
                scale.Y = 1;
            if (scale.Z > 0.9999f && scale.Z <= 1.0001f)
                scale.Z = 1;

#if DEBUG
            this.Validate();
#endif
        }

        public void Validate()
        {
            if (float.IsNaN(rotation.X) ||
                float.IsNaN(rotation.Y) ||
                float.IsNaN(rotation.Z) ||
                float.IsNaN(rotation.W) ||

                float.IsNaN(translation.X) ||
                float.IsNaN(translation.Y) ||
                float.IsNaN(translation.Z) ||

                float.IsNaN(scale.X) ||
                float.IsNaN(scale.Y) ||
                float.IsNaN(scale.Z) ||

                float.IsInfinity(rotation.X) ||
                float.IsInfinity(rotation.Y) ||
                float.IsInfinity(rotation.Z) ||
                float.IsInfinity(rotation.W) ||

                float.IsInfinity(translation.X) ||
                float.IsInfinity(translation.Y) ||
                float.IsInfinity(translation.Z) ||

                float.IsInfinity(scale.X) ||
                float.IsInfinity(scale.Y) ||
                float.IsInfinity(scale.Z))
            {
                throw new ArgumentException();
            }
        }

        public void GetMatrix(out Matrix mat)
        {
            Matrix.CreateFromQuaternion(ref rotation, out mat);

            mat.M41 = translation.X;
            mat.M42 = translation.Y;
            mat.M43 = translation.Z;

            mat.M11 *= scale.X;
            mat.M12 *= scale.X;
            mat.M13 *= scale.X;

            mat.M21 *= scale.Y;
            mat.M22 *= scale.Y;
            mat.M23 *= scale.Y;

            mat.M31 *= scale.Z;
            mat.M32 *= scale.Z;
            mat.M33 *= scale.Z;
        }

        public static void Lerp(ref Transform from, ref Transform to, float amount, out Transform result)
        {
            Quaternion.Lerp(ref from.rotation, ref to.rotation, amount, out result.rotation);
            Vector3.Lerp(ref from.translation, ref to.translation, amount, out result.translation);
            Vector3.Lerp(ref from.scale, ref to.scale, amount, out result.scale);
        }

        public Transform Lerp(ref Transform to, float amount)
        {
            Transform result;
            Lerp(ref this, ref to, amount, out result);
            return result;
        }

        public void LerpToIdentity(float weighting)
        {
            this.translation.X *= weighting;
            this.translation.Y *= weighting;
            this.translation.Z *= weighting;
            this.scale.X *= weighting;
            this.scale.Y *= weighting;
            this.scale.Z *= weighting;

            if (rotation.W >= 0)
            {
                rotation.X = (weighting * rotation.X);
                rotation.Y = (weighting * rotation.Y);
                rotation.Z = (weighting * rotation.Z);
                rotation.W = (weighting * rotation.W) + (1 - weighting);
            }
            else
            {
                rotation.X = (weighting * rotation.X);
                rotation.Y = (weighting * rotation.Y);
                rotation.Z = (weighting * rotation.Z);
                rotation.W = (weighting * rotation.W) - (1 - weighting);
            }
            rotation.Normalize();
        }

        public static Transform operator *(Transform a, Transform b)
        {
            Transform t;
            Multiply(ref a, ref b, out t);
            return t;
        }

        public static void Multiply(ref Transform transform1, ref Transform transform2, out Transform result)
        {
            Quaternion q;
            Vector3 t, s;
            s = new Vector3();
            s.X = transform2.scale.X * transform1.scale.X;
            s.Y = transform2.scale.Y * transform1.scale.Y;
            s.Z = transform2.scale.Z * transform1.scale.Z;

            if (transform2.rotation.W == 1 &&
                (transform2.rotation.X == 0 && transform2.rotation.Y == 0 && transform2.rotation.Z == 0))
            {
                q.X = transform1.rotation.X;
                q.Y = transform1.rotation.Y;
                q.Z = transform1.rotation.Z;
                q.W = transform1.rotation.W;
                t.X = transform1.translation.X;
                t.Y = transform1.translation.Y;
                t.Z = transform1.translation.Z;
            }
            else
            {
                float num12 = transform2.rotation.X + transform2.rotation.X;
                float num2 = transform2.rotation.Y + transform2.rotation.Y;
                float num = transform2.rotation.Z + transform2.rotation.Z;
                float num11 = transform2.rotation.W * num12;
                float num10 = transform2.rotation.W * num2;
                float num9 = transform2.rotation.W * num;
                float num8 = transform2.rotation.X * num12;
                float num7 = transform2.rotation.X * num2;
                float num6 = transform2.rotation.X * num;
                float num5 = transform2.rotation.Y * num2;
                float num4 = transform2.rotation.Y * num;
                float num3 = transform2.rotation.Z * num;
                t.X = ((transform1.translation.X * ((1f - num5) - num3)) + (transform1.translation.Y * (num7 - num9))) + (transform1.translation.Z * (num6 + num10));
                t.Y = ((transform1.translation.X * (num7 + num9)) + (transform1.translation.Y * ((1f - num8) - num3))) + (transform1.translation.Z * (num4 - num11));
                t.Z = ((transform1.translation.X * (num6 - num10)) + (transform1.translation.Y * (num4 + num11))) + (transform1.translation.Z * ((1f - num8) - num5));

                num12 = (transform2.rotation.Y * transform1.rotation.Z) - (transform2.rotation.Z * transform1.rotation.Y);
                num11 = (transform2.rotation.Z * transform1.rotation.X) - (transform2.rotation.X * transform1.rotation.Z);
                num10 = (transform2.rotation.X * transform1.rotation.Y) - (transform2.rotation.Y * transform1.rotation.X);
                num9 = ((transform2.rotation.X * transform1.rotation.X) + (transform2.rotation.Y * transform1.rotation.Y)) + (transform2.rotation.Z * transform1.rotation.Z);
                q.X = ((transform2.rotation.X * transform1.rotation.W) + (transform1.rotation.X * transform2.rotation.W)) + num12;
                q.Y = ((transform2.rotation.Y * transform1.rotation.W) + (transform1.rotation.Y * transform2.rotation.W)) + num11;
                q.Z = ((transform2.rotation.Z * transform1.rotation.W) + (transform1.rotation.Z * transform2.rotation.W)) + num10;
                q.W = (transform2.rotation.W * transform1.rotation.W) - num9;
            }

            t.X = t.X * transform2.scale.X + transform2.translation.X;
            t.Y = t.Y * transform2.scale.Y + transform2.translation.Y;
            t.Z = t.Z * transform2.scale.Z + transform2.translation.Z;

            result = new Transform();

            result.rotation.X = q.X;
            result.rotation.Y = q.Y;
            result.rotation.Z = q.Z;
            result.rotation.W = q.W;

            result.translation.X = t.X;
            result.translation.Y = t.Y;
            result.translation.Z = t.Z;
            result.scale = s;
        }
    }
}
