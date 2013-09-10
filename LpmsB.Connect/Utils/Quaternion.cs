using System;
using System.Collections.Generic;

namespace LpmsB.Utils
{
    public struct Quaternion : IEquatable<Quaternion>, IEnumerable<double>
    {
        // dimensions count
        public const int Dimension = 4;

        public static Quaternion Zero { get { return new Quaternion(); } }
        public static Quaternion Identity { get { return new Quaternion(1, Vector3d.Zero); } }

        public double W;

        public double X;

        public double Y;

        public double Z;

        public Quaternion(double w, double x, double y, double z)
        {
            W = w;
            X = x;
            Y = y;
            Z = z;
        }

        public Quaternion(double w, Vector3d v)
        {
            W = w;
            X = v.X;
            Y = v.Y;
            Z = v.Z;
        }

        public double Scalar
        {
            get { return W; }
            set { W = value; }
        }

        public Vector3d Vector
        {
            get { return new Vector3d(X, Y, Z); }
            set { X = value.X; Y = value.Y; Z = value.Z; }
        }

        public double this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return W;
                    case 1: return X;
                    case 2: return Y;
                    case 3: return Z;
                    default: throw new ArgumentOutOfRangeException("index");
                }
            }
            set
            {
                switch (index)
                {
                    case 0: W = value; break;
                    case 1: X = value; break;
                    case 2: Y = value; break;
                    case 3: Z = value; break;
                    default: throw new ArgumentOutOfRangeException("index");
                }
            }
        }

        public bool Equals(Quaternion other)
        {
            return other == this;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null) || !(obj is Quaternion))
                return false;
            return Equals((Quaternion)obj);
        }

        public override int GetHashCode()
        {
            return W.GetHashCode() ^ X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("[W:{0} X:{1} Y:{2} Z:{3}]", W, X, Y, Z);
        }

        public IEnumerator<double> GetEnumerator()
        {
            yield return W;
            yield return X;
            yield return Y;
            yield return Z;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            yield return W;
            yield return X;
            yield return Y;
            yield return Z;
        }

        public static bool operator ==(Quaternion left, Quaternion right)
        {
            return left.W == right.W
                && left.X == right.X
                && left.Y == right.Y
                && left.Z == right.Z;
        }

        public static bool operator !=(Quaternion left, Quaternion right)
        {
            return !(left == right);
        }

        public double SquaredLength
        {
            get { return W * W + X * X + Y * Y + Z * Z; }
        }

        public double Length
        {
            get { return Math.Sqrt(SquaredLength); }
        }

        public Quaternion Conjugate()
        {
            return new Quaternion(W, -X, -Y, -Z);
        }

        public Quaternion Normalize()
        {
            var len = Length;
            return len > double.Epsilon ? this / len : Zero;
        }

        public static Quaternion operator /(Quaternion q, double devider)
        {
            return new Quaternion(q.W / devider, q.X / devider, q.Y / devider, q.Z / devider);
        }

        public static Quaternion operator *(Quaternion q, double mul)
        {
            return new Quaternion(q.W * mul, q.X * mul, q.Y * mul, q.Z * mul);
        }

        public static Quaternion operator *(double mul, Quaternion q)
        {
            return new Quaternion(q.W * mul, q.X * mul, q.Y * mul, q.Z * mul);
        }

        public static Quaternion operator *(Quaternion q, Quaternion p)
        {
            return new Quaternion(
                q.W * p.W - q.X * p.X - q.Y * p.Y - q.Z * p.Z,
                q.W * p.X + q.X * p.W + q.Y * p.Z - q.Z * p.Y,
                q.W * p.Y - q.X * p.Z + q.Y * p.W + q.Z * p.X,
                q.W * p.Z + q.X * p.Y - q.Y * p.X + q.Z * p.W);
        }

        public Vector3d Rotate(Vector3d v)
        {
            return (this * new Quaternion(0, v) * Conjugate()).Vector;
        }

        public static Quaternion operator +(Quaternion left, Quaternion right)
        {
            return new Quaternion(
                left.W + right.W,
                left.X + right.X,
                left.Y + right.Y,
                left.Z + right.Z);
        }

        public static Quaternion operator -(Quaternion left, Quaternion right)
        {
            return new Quaternion(
                left.W - right.W,
                left.X - right.X,
                left.Y - right.Y,
                left.Z - right.Z);
        }

        public static Quaternion operator -(Quaternion q)
        {
            return new Quaternion(-q.W, -q.X, -q.Y, -q.Z);
        }

        public static Quaternion CreateRotation(Vector3d axis, double angle)
        {
            return new Quaternion(Math.Cos(0.5 * angle), Math.Sin(0.5 * angle) * axis);
        }
    }
}
