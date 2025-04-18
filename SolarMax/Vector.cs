using System;
using System.Diagnostics;

namespace SolarMax;
public struct Vector
{
    public static Vector Zero { get; private set; }
    public static Vector MinValue { get; private set; }
    public static Vector UnitX { get; private set; }
    public static Vector UnitY { get; private set; }
    public static Vector UnitZ { get; private set; }

    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }

    public Vector(double X, double Y, double Z) : this()
    {
#if DEBUG
        if (double.IsNaN(X) || double.IsNaN(Y) || double.IsNaN(Z) || double.IsInfinity(X) || double.IsInfinity(Y) || double.IsInfinity(Z))
            throw new Exception();
#endif
        this.X = X;
        this.Y = Y;
        this.Z = Z;
    }
    public Vector(Vector Template) : this()
    {
        this.X = Template.X;
        this.Y = Template.Y;
        this.Z = Template.Z;
    }
    public readonly bool IsZero
        => this.X == 0.0 && this.Y == 0.0 && this.Z == 0.0;
    static Vector()
    {
        Vector.Zero = new Vector();
        Vector.MinValue = new Vector(double.MinValue / 4, double.MinValue / 4, double.MinValue / 4);
        Vector.UnitX = new Vector(1, 0, 0);
        Vector.UnitY = new Vector(0, 1, 0);
        Vector.UnitZ = new Vector(0, 0, 1);
    }
    public readonly double DistanceTo(Vector V) => Math.Sqrt(this.DistanceToSquared(V));
    public readonly double DistanceToSquared(Vector V)
    {
        var x = this.X - V.X;
        var y = this.Y - V.Y;
        var z = this.Z - V.Z;

        return x * x + y * y + z * z;
    }
    public readonly Vector DifferenceDirection(Vector Ref) => (Ref - this).Unit;
    public readonly Vector Unit => this / this.Magnitude;
    public void Normalize()
    {
        double length = Math.Sqrt(X * X + Y * Y + Z * Z);
        this /= length;
    }
    public readonly Vector Inverse => new(-X, -Y, -Z);

    public static double operator * (Vector V1, Vector V2)
    {
        // Dot Product
        return V1.X * V2.X +
               V1.Y * V2.Y +
               V1.Z * V2.Z;
    }
    public static Vector operator ^ (Vector V1, Vector V2)
    {
        // Cross Product
        var cross = new Vector(V1.Y * V2.Z - V1.Z * V2.Y,
                               V1.Z * V2.X - V1.X * V2.Z,
                               V1.X * V2.Y - V1.Y * V2.X);

#if DEBUG
        if (cross.Magnitude < MathEx.EPSILON)
        //throw new Exception("Cross product of parallel vectors");
        {
            Debug.WriteLine("Cross product of parallel vectors");
        }
#endif

        return cross;
    }
    public static Vector operator * (Vector V, double Scalar)
    {
        return new Vector(V.X * Scalar,
                          V.Y * Scalar,
                          V.Z * Scalar);
    }
    public static Vector operator * (double Scalar, Vector V)
    {
        return new Vector(V.X * Scalar,
                          V.Y * Scalar,
                          V.Z * Scalar);
    }
    public static Vector operator / (Vector V, double Denominator)
    {
        return new Vector(V.X / Denominator,
                          V.Y / Denominator,
                          V.Z / Denominator);
    }
    public static Vector operator + (Vector V1, Vector V2)
    {
        return new Vector(V1.X + V2.X,
                          V1.Y + V2.Y,
                          V1.Z + V2.Z);
    }
    public static Vector operator -(Vector V1, Vector V2)
    {
        return new Vector(V1.X - V2.X,
                          V1.Y - V2.Y,
                          V1.Z - V2.Z);
    }
    public static Vector operator -(Vector V)
    {
        return new Vector(-V.X, -V.Y, -V.Z);
    }
    public readonly double MagnitudeSquared
    {
        get
        {
            return this.X * this.X +
                   this.Y * this.Y +
                   this.Z * this.Z;
        }
    }
    public double Magnitude
    {
        readonly get
        {
            var magSquared = this.X * this.X +
                             this.Y * this.Y +
                             this.Z * this.Z;

            if (magSquared < Double.MaxValue)
            {
                return Math.Sqrt(magSquared);
            }
            else
            {
                double max = Math.Max(Math.Abs(X), Math.Max(Math.Abs(Y), Math.Abs(Z)));

                double x = X / max;
                double y = Y / max;
                double z = Z / max;

                return Math.Sqrt(x * x + y * y + z * z) * max;
            }
        }
        set
        {
            double mag = this.Magnitude;
            X *= value / mag;
            Y *= value / mag;
            Z *= value / mag;
        }
    }
    public override readonly string ToString()
    {
        return String.Format("X: {0:e} Y: {1:e} Z: {2:e} Az: {3:000.00}� Inc: {4:00.00}� Tilt: {5:00.00}� Mag: {6:e}",
                             X,
                             Y,
                             Z,
                             this.Azimuth.NormalizeAngleZeroToTwoPi().ToDegreesFromRadians(),
                             this.Inclination.ToDegreesFromRadians(),
                             this.Tilt.ToDegreesFromRadians(),
                             this.Magnitude);
    }
    public readonly Vector GetRotationAboutZAxis(double Angle)
    {
        if (Angle == 0.0)
        {
            return this;
        }
        else
        {
            double x = this.X;
            double cos = Math.Cos(Angle);
            double sin = Math.Sin(Angle);

            return new Vector(x * cos - this.Y * sin,
                              x * sin + this.Y * cos,
                              this.Z);
        }
    }
    public readonly Vector GetRotationAboutYAxis(double Angle)
    {
        if (Angle == 0.0)
        {
            return this;
        }
        else
        {
            double x = this.X;

            double cos = Math.Cos(Angle);
            double sin = Math.Sin(Angle);

            return new Vector(x * cos + this.Z * sin,
                              this.Y,
                              this.Z * cos - x * sin);
        }
    }
    public readonly Vector GetRotationAboutXAxis(double Angle)
    {
        if (Angle == 0.0)
        {
            return this;
        }
        else
        {
            double y = this.Y;

            double cos = Math.Cos(Angle);
            double sin = Math.Sin(Angle);

            return new Vector(this.X,
                              y * cos - this.Z * sin,
                              y * sin + this.Z * cos);
        }
    }
    public readonly double Azimuth => Math.Atan2(this.Y, this.X);
    public readonly double Inclination 
        => Math.Atan2(this.Z,
                      Math.Sqrt(this.X * this.X +
                                this.Y * this.Y));
    public readonly double Tilt => MathEx.HALF_PI - this.Inclination;
    public readonly Vector GetRotationAbout(Vector Axis, double Angle)
    {
        if (Angle == 0.0)
        {
            return this;
        }
        else
        {
            //return Quaternion.RotateVector(this, Axis, Angle);

            // This is probably faster:

            double azimuth = Axis.Azimuth;
            double inc = Axis.Inclination;

            return this.GetRotationAboutZAxis(-azimuth)
                       .GetRotationAboutYAxis(inc)
                       .GetRotationAboutXAxis(Angle)
                       .GetRotationAboutYAxis(-inc)
                       .GetRotationAboutZAxis(azimuth);
        }
    }
    public readonly Vector GetTranslationRelativeToReferenceAsXAxisAlt(Vector Reference)
    {
        return this.GetRotationAboutZAxis(-Reference.Azimuth)
                   .GetRotationAboutYAxis(Reference.Inclination);
    }
    public readonly double AngleDiffAbs(Vector V)
    {
        var v = this.Unit;
        V.Normalize();

        double ratio = v * V;
        double result;

        if (ratio.IsNegative())
        {
            result = Math.PI - 2.0 * Math.Asin((-v - V).Magnitude / 2.0);
        }
        else
        {
            result = 2.0 * Math.Asin((v - V).Magnitude / 2.0);
        }

#if DEBUG
        if (result < 0)
            throw new Exception("AngleDiffAbs < 0");
#endif
        return result;
    }
    public readonly Vector Slerp(Vector Target, double FractionToRotate)
    {
#if DEBUG
        if (FractionToRotate > 1.0)
            throw new Exception();
#endif
        double heading = this.Azimuth;
        double inc = this.Inclination;

        // Rotate so this becomes the X axis
        Target = Target.GetRotationAboutZAxis(-heading)
                       .GetRotationAboutYAxis(inc);

        // Rotate to Z = 0 Plane
        double b2InclinationProjectedToXAxis = Math.Atan2(Target.Z, Target.Y);
        Target = Target.GetRotationAboutXAxis(-b2InclinationProjectedToXAxis);

        // Scale angle change to Factor
        double headingB = Target.Azimuth;
        headingB *= FractionToRotate;

        // Undo rotations
        return new Vector(Math.Cos(headingB) * this.Magnitude, Math.Sin(headingB) * this.Magnitude, 0)
                  .GetRotationAboutXAxis(b2InclinationProjectedToXAxis)
                  .GetRotationAboutYAxis(-inc)
                  .GetRotationAboutZAxis(heading);
    }
    public readonly bool IsSimilarTo(Vector CompareTo)
    {
        return this.Inclination.IsCloseAbsolute(CompareTo.Inclination) &&
               this.Azimuth.IsCloseAbsolute(CompareTo.Azimuth) &&
               this.Magnitude.IsCloseRelative(CompareTo.Magnitude);
    }
    public override readonly bool Equals(object Obj) => Obj is Vector v && this == v;
    public readonly bool Equals(Vector V) => this == V;
    public override readonly int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
    public static bool operator ==(Vector A, Vector B)
        => A.X == B.X && A.Y == B.Y && A.Z == B.Z;

    public static bool operator !=(Vector A, Vector B) => !(A == B);
    public readonly string Serialize()
    {
        return string.Format("{0:" + Util.DOUBLE_STRING_FORMAT + "},{1:" + Util.DOUBLE_STRING_FORMAT + "},{2:" + Util.DOUBLE_STRING_FORMAT + "}", this.X, this.Y, this.Z);
    }
}
