using System;
using System.Collections.Generic;

namespace LpmsB
{
    public struct Vector3d : IEquatable<Vector3d>, IEnumerable<double>
    {
        // dimensions count
        public const int Dimension = 3;

        public static Vector3d Zero
        {
            get { return new Vector3d(); }
        }

        public static Vector3d UnitX
        {
            get { return new Vector3d(1, 0, 0); }
        }

        public static Vector3d UnitY
        {
            get { return new Vector3d(0, 1, 0); }
        }

        public static Vector3d UnitZ
        {
            get { return new Vector3d(0, 0, 1); }
        }


        public double X;

        public double Y;

        public double Z;

        
        public Vector3d(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3d(double v)
        {
            X = Y = Z = v;
        }

        public double this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return X;
                    case 1: return Y;
                    case 2: return Z;
                    default: throw new ArgumentOutOfRangeException("index");
                }
            }
            set
            {
                switch (index)
                {
                    case 0: X = value; break;
                    case 1: Y = value; break;
                    case 2: Z = value; break;
                    default: throw new ArgumentOutOfRangeException("index");
                }
            }
        }

        public IEnumerator<double> GetEnumerator()
        {
            yield return X;
            yield return Y;
            yield return Z;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            yield return X;
            yield return Y;
            yield return Z;
        }

        public static Vector3d operator - (Vector3d v)
        {
            return new Vector3d(-v.X, -v.Y, -v.Z);
        }

        public static Vector3d operator - (Vector3d left, Vector3d right)
        {
            return new Vector3d(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }

        public static Vector3d operator + (Vector3d left, Vector3d right)
        {
            return new Vector3d(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }

        public static Vector3d operator * (Vector3d left, double right)
        {
            return new Vector3d(left.X * right, left.Y * right, left.Z * right);
        }

        public static Vector3d operator * (double left, Vector3d right)
        {
            return new Vector3d(left * right.X, left * right.Y, left * right.Z);
        }

        public static Vector3d operator / (Vector3d left, double right)
        {
            return new Vector3d(left.X / right, left.Y / right, left.Z / right);
        }

        public static double operator | (Vector3d left, Vector3d right)
        {
            return left.X * right.X + left.Y * right.Y + left.Z * right.Z;
        }

        public static Vector3d operator * (Vector3d left, Vector3d right)
        {
            return new Vector3d(
                left.Y * right.Z - left.Z * right.Y,
                left.Z * right.X - left.X * right.Z,
                left.X * right.Y - left.Y * right.X);
        }

        public static bool operator == (Vector3d left, Vector3d right)
        {
            return left.X == right.X && left.Y == right.Y && left.Z == right.Z;
        }

        public static bool operator ==(Vector3d left, double right)
        {
            return left.X == right && left.Y == right && left.Z == right;
        }

        public static bool operator != (Vector3d left, Vector3d right)
        {
            return left.X != right.X || left.Y != right.Y || left.Z != right.Z;
        }

        public static bool operator !=(Vector3d left, double right)
        {
            return left.X != right || left.Y != right || left.Z != right;
        }

        public static bool operator <(Vector3d left, Vector3d right)
        {
            return left.X < right.X && left.Y < right.Y && left.Z < right.Z;
        }

        public static bool operator <(Vector3d left, double right)
        {
            return left.X < right && left.Y < right && left.Z < right;
        }

        public static bool operator <=(Vector3d left, Vector3d right)
        {
            return left.X <= right.X && left.Y <= right.Y && left.Z <= right.Z;
        }

        public static bool operator <=(Vector3d left, double right)
        {
            return left.X <= right && left.Y <= right && left.Z <= right;
        }

        public static bool operator >(Vector3d left, Vector3d right)
        {
            return left.X > right.X && left.Y > right.Y && left.Z > right.Z;
        }

        public static bool operator >(Vector3d left, double right)
        {
            return left.X > right && left.Y > right && left.Z > right;
        }

        public static bool operator >=(Vector3d left, Vector3d right)
        {
            return left.X >= right.X && left.Y >= right.Y && left.Z >= right.Z;
        }

        public static bool operator >=(Vector3d left, double right)
        {
            return left.X >= right && left.Y >= right && left.Z >= right;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null) || !(obj is Vector3d))
                return false;

            var v = (Vector3d)obj;

            return v == this;
        }

        public bool Equals(Vector3d v)
        {
            return v == this;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("[{0} {1} {2}]", X, Y, Z);
        }


        public double Length
        {
            get { return Math.Sqrt(X * X + Y * Y + Z * Z); }
        }

        public Vector3d Normalize()
        {
            var length = Length;
            return length <= double.Epsilon ? Vector3d.Zero : (this / length);
        }
    }
}
