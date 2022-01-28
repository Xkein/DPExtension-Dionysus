using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vector3D = PatcherYRpp.SingleVector3D;

namespace PatcherYRpp.Utilities
{
    public static class MathEx
    {
        private static Random _random = new Random(60);
        public static void SetRandomSeed(int seed)
        {
            _random = new Random(seed);
        }
        public static Random Random => _random;

        // ===============================================
        // Utilities for numeric
        #region Numeric
        public const double Epsilon = float.Epsilon;


        public static bool Approximately(double a, double b)
        {
            return Math.Abs(a - b) < Epsilon;
        }
        public static bool Approximately(float a, float b)
        {
            return Approximately(a, b);
        }
        public static bool IsNearlyZero(double val)
        {
            return Approximately(val, 0);
        }

        public static double Deg2Rad(double deg)
        {
            return ((Math.PI * 2) / 360) * deg;
        }
        public static double Rad2Deg(double rad)
        {
            return (360 / (Math.PI * 2)) * rad;
        }

        public static double Clamp(double x, double min, double max)
        {
            return x < min ? min : x < max ? x : max;
        }
        public static float Clamp(float x, float min, float max)
        {
            return x < min ? min : x < max ? x : max;
        }
        public static int Clamp(int x, int min, int max)
        {
            return x < min ? min : x < max ? x : max;
        }
        public static long Clamp(long x, long min, long max)
        {
            return x < min ? min : x < max ? x : max;
        }

        public static double Wrap(double x, double min, double max)
        {
            if (min == max)
            {
                return min;
            }

            var size = max - min;
            var endVal = x;

            while (endVal < min)
            {
                endVal += size;
            }
            while (endVal > max)
            {
                endVal -= size;
            }

            return endVal;
        }
        public static float Wrap(float x, float min, float max)
        {
            return (float)Wrap((double)x, min, max);
        }
        public static long Wrap(long x, long min, long max)
        {
            if (min == max)
            {
                return min;
            }

            var size = max - min;
            var endVal = x;

            while (endVal < min)
            {
                endVal += size;
            }
            while (endVal > max)
            {
                endVal -= size;
            }

            return endVal;
        }
        public static int Wrap(int x, int min, int max)
        {
            return (int)Wrap((long)x, min, max);
        }

        public static double Lerp(double a, double b, double alpha)
        {
            return a + alpha * (b - a);
        }
        public static float Lerp(float a, float b, double alpha)
        {
            return (float)Lerp((double)a, b, alpha);
        }
        public static long Lerp(long a, long b, double alpha)
        {
            return (long)(a + alpha * (b - a));
        }
        public static int Lerp(int a, int b, double alpha)
        {
            return (int)Lerp((long)a, b, alpha);
        }

        public static double Repeat(double t, double length)
        {
            if (length <= 0)
            {
                throw new ArgumentException("length should not <= 0");
            }

            var val = t % length;
            return val < 0 ? val + length : val;
        }
        public static double PingPong(double t, double length)
        {
            var val = Repeat(t, length * 2);
            return val > length ? 2 * length - val : val;
        }
        #endregion



        #region Miscellaneous
        public static Vector3D GetForwardVector(Pointer<TechnoClass> pTechno, bool getTurret = false)
        {
            FacingStruct facing = getTurret ? pTechno.Ref.TurretFacing : pTechno.Ref.Facing;

            return facing.current().ToVector3D();
        }


        #endregion


        // ===============================================
        // Utilities for Vectors
        #region Vectors
        public static Vector3D ZeroVector3D = new Vector3D(0, 0, 0);
        public static Vector3D GetNormalizedVector3D(Vector3D vector)
        {
            return vector == ZeroVector3D ? ZeroVector3D : vector * (1 / vector.Magnitude());
        }

        public static float CalculateRandomRange(float min = 0.0f, float max = 1.0f)
        {
            if (min == max)
            {
                return min;
            }

            float length = max - min;
            return min + (float)_random.NextDouble() * length;

        }

        public static Vector3D CalculateRandomUnitVector()
        {
            const float r = 1;
            const float PI2 = (float)(Math.PI * 2);

            float azimuth = (float)(_random.NextDouble() * PI2);
            float elevation = (float)(_random.NextDouble() * PI2);

            return new Vector3D(
                (float)(r * Math.Cos(elevation) * Math.Cos(azimuth)),
                (float)(r * Math.Cos(elevation) * Math.Sin(azimuth)),
                (float)(r * Math.Sin(elevation))
                );

        }
        public static Vector3D CalculateRandomPointInSphere(float innerRadius, float outerRadius)
        {
            return CalculateRandomUnitVector() * CalculateRandomRange(innerRadius, outerRadius);
        }

        public static Vector3D CalculateRandomPointInBox(Vector3D size)
        {
            return new Vector3D(
                CalculateRandomRange(0, size.X) - size.X / 2f,
                CalculateRandomRange(0, size.Y) - size.Y / 2f,
                CalculateRandomRange(0, size.Z) - size.Z / 2f
                );
        }

        #endregion




        // ===============================================
        // Utilities for Convertions
        #region Convertions
        public static Vector3D ToVector3D(this DirStruct dir)
        {
            double rad = -dir.radians();
            Vector3D vec = new Vector3D(Math.Cos(rad), Math.Sin(rad), 0);
            return vec;
        }

        public static Vector3D ToVector3D(this CoordStruct coord)
        {
            return new Vector3D(coord.X, coord.Y, coord.Z);
        }
        public static CoordStruct ToCoordStruct(this Vector3D vec)
        {
            return new CoordStruct((int)vec.X, (int)vec.Y, (int)vec.Z);
        }

        public static Vector3D ToVector3D(this BulletVelocity velocity)
        {
            return new Vector3D(velocity.X, velocity.Y, velocity.Z);
        }

        #endregion
    }
}
