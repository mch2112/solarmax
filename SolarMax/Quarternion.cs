using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax
{
    internal sealed class Quaternion
    {
        public double W { get; private set; }
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Z { get; private set; }

        public static Quaternion Identity { get; private set; }
        public Quaternion(double W, double X, double Y, double Z)
        {
            this.W = W;
            this.X = X;
            this.Y = Y;
            this.Z = Z;

#if DEBUG
            this.validate();
#endif
        }
        public Quaternion(Vector V)
        {
            this.W = 0;
            this.X = V.X;
            this.Y = V.Y;
            this.Z = V.Z;
#if DEBUG
            this.validate();
#endif
        }
        public Quaternion Copy()
        {
            return new Quaternion(this.W, this.X, this.Y, this.Z);
        }
        private void validate()
        {
            if (double.IsNaN(W) || double.IsNaN(X) || double.IsNaN(Y) || double.IsNaN(Z) || double.IsInfinity(W) || double.IsInfinity(X) || double.IsInfinity(Y) || double.IsInfinity(Z))
                throw new Exception("Invalid Quaternion");
            //if (Math.Abs(1.0 - this.Magnitude) > MathEx.EPSILON)
            //    throw new Exception("Non-Unit Quaternion");

        }
        static Quaternion()
        {
            Identity = new Quaternion(1, 0, 0, 0);
        }
        public double DotProduct(Quaternion Other)
        {
            return this.W * Other.W +
                   this.X * Other.X +
                   this.Y * Other.Y +
                   this.Z * Other.Z;
        }
        public double Magnitude
        {
            get
            {
                var magSquared = this.W * this.W +
                                 this.X * this.X +
                                 this.Y * this.Y +
                                 this.Z * this.Z;

                if (magSquared < Double.MaxValue)
                {
                    return Math.Sqrt(magSquared);
                }
                else
                {
                    double max = Math.Max(Math.Max(Math.Abs(W), Math.Abs(X)),
                                          Math.Max(Math.Abs(Y), Math.Abs(Z)));

                    double w = W / max;
                    double x = X / max;
                    double y = Y / max;
                    double z = Z / max;

                    return Math.Sqrt(w * w + x * x + y * y + z * z) * max;
                }
            }
        }
        public void GetRotations(out double XAxis, out double YAxis, out double ZAxis)
        {
            XAxis = Math.Atan2(2.0 * (W * X + Y * Z), 1.0 - 2.0 * (X * X + Y * Y));
            YAxis = Math.Asin(2.0 * (W * Y - Z * X));
            ZAxis = Math.Atan2(2.0 * (W * Z + X * Y), 1.0 - 2.0 * (Y * Y + Z * Z));
        }
        public Quaternion Scale(double Factor)
        {
            return new Quaternion(this.W * Factor,
                                  this.X * Factor,
                                  this.Y * Factor,
                                  this.Z * Factor);
        }
        public Quaternion Conjugate
        {
            get { return new Quaternion(this.W, -this.X, -this.Y, -this.Z); }
        }
        public Quaternion Negation
        {
            get { return new Quaternion(-W, -X, -Y, -Z); }
        }
        public static Quaternion operator +(Quaternion Q1, Quaternion Q2)
        {
            return new Quaternion(Q1.W + Q2.W, Q1.X + Q2.X, Q1.Y + Q2.Y, Q1.Z + Q2.Z);
        }
        public static Quaternion operator -(Quaternion Q1, Quaternion Q2)
        {
            return new Quaternion(Q1.W - Q2.W, Q1.X - Q2.X, Q1.Y - Q2.Y, Q1.Z - Q2.Z);
        }
        public static Quaternion operator *(Quaternion Q, double Multiplier)
        {
            return new Quaternion(Q.W * Multiplier, Q.X * Multiplier, Q.Y * Multiplier, Q.Z * Multiplier);
        }
        public static Quaternion operator *(double Multiplier, Quaternion Q)
        {
            return new Quaternion(Q.W * Multiplier, Q.X * Multiplier, Q.Y * Multiplier, Q.Z * Multiplier);
        }
        public static Quaternion operator /(Quaternion Q, double Divisor)
        {
            return new Quaternion(Q.W / Divisor, Q.X / Divisor, Q.Y / Divisor, Q.Z / Divisor);
        }
        public static Quaternion operator *(Quaternion Q1, Quaternion Q2)
        {
            return new Quaternion((Q1.W * Q2.W - Q1.X * Q2.X - Q1.Y * Q2.Y - Q1.Z * Q2.Z),
                                  (Q1.W * Q2.X + Q1.X * Q2.W + Q1.Y * Q2.Z - Q1.Z * Q2.Y),
                                  (Q1.W * Q2.Y - Q1.X * Q2.Z + Q1.Y * Q2.W + Q1.Z * Q2.X),
                                  (Q1.W * Q2.Z + Q1.X * Q2.Y - Q1.Y * Q2.X + Q1.Z * Q2.W));
        }
        public static Quaternion operator *(Quaternion Q1, Vector Q2)
        {
            return new Quaternion((            - Q1.X * Q2.X - Q1.Y * Q2.Y - Q1.Z * Q2.Z),
                                  (Q1.W * Q2.X               + Q1.Y * Q2.Z - Q1.Z * Q2.Y),
                                  (Q1.W * Q2.Y - Q1.X * Q2.Z               + Q1.Z * Q2.X),
                                  (Q1.W * Q2.Z + Q1.X * Q2.Y - Q1.Y * Q2.X              ));
        }
        //Quaternion multiplication without the W component
        public static Vector PointMultiply(Quaternion Q1, Quaternion Q2)
        {
            return new Vector((Q1.W * Q2.X + Q1.X * Q2.W + Q1.Y * Q2.Z - Q1.Z * Q2.Y),
                              (Q1.W * Q2.Y - Q1.X * Q2.Z + Q1.Y * Q2.W + Q1.Z * Q2.X),
                              (Q1.W * Q2.Z + Q1.X * Q2.Y - Q1.Y * Q2.X + Q1.Z * Q2.W));
        }
        public Vector RotateVector(Vector V)
        {
            // 21 + 21 = 42 operations
            return PointMultiply(this * V, this.Conjugate);
        }

        public Vector RotateVectorFast(Vector V)
        {
            // 30 operations
            // uses fewer calcs, see http://physicsforgames.blogspot.com/2010/03/quaternion-tricks.html

            var x = this.Y * V.Z - this.Z * V.Y;
            var y = this.Z * V.X - this.X * V.Z;
            var z = this.X * V.Y - this.Y * V.X;

            var vv = new Vector(V.X + 2.0 * (this.W * x + this.Y * z - this.Z * y),
                                V.Y + 2.0 * (this.W * y + this.Z * x - this.X * z),
                                V.Z + 2.0 * (this.W * z + this.X * y - this.Y * x));

//#if DEBUG
//            if (!RotateVectorOld(V).IsSimilarTo(vv))
//                throw new Exception();
//#endif
                return vv;
        }
        public static Quaternion GetRotationQuaternion(Vector Start, Vector End)
        {
            Vector halfway = Start + End;
            halfway.Normalize();

            return new Quaternion(Start * halfway,
                                  Start.Y * halfway.Z - Start.Z * halfway.Y,
                                  Start.Z * halfway.X - Start.X * halfway.Z,
                                  Start.X * halfway.Y - Start.Y * halfway.X);
        }
        public static Quaternion GetRotationQuaternion(Vector Axis, double Angle)
        {
            if (Axis.Magnitude < MathEx.EPSILON)
                return Quaternion.Identity;

            double sin = Math.Sin(Angle * 0.5);

            Axis = Axis.Unit;

            return new Quaternion(Math.Cos(Angle * 0.5),
                                  sin * Axis.X,
                                  sin * Axis.Y,
                                  sin * Axis.Z);
        }
        public static Vector RotateVector(Vector V, Vector Axis, double Angle)
        {
            // Probably not performant

            Quaternion rotQuat = GetRotationQuaternion(Axis, Angle);

            return PointMultiply(rotQuat * V, rotQuat.Conjugate);
        }
#if DEBUG
        public Quaternion __SlerpTo(Quaternion Target, double Factor)
        {
            double cosOmega;
            double scaleFrom, scaleTo;

            // TODO: Needed?
            var From = this.Copy();

            // Normalize inputs and stash their lengths 
            double lenFrom = From.Magnitude;
            double lenTo = Target.Magnitude;
            
            From = From.Scale(1 / lenFrom);
            Target = Target.Scale(1 / lenTo);

            // Calculate cos of omega. 
            cosOmega = From.DotProduct(Target);

            if (true) // Shortest Path
            {
                // If we are taking the shortest path we flip the signs to ensure that 
                // cosOmega will be positive.
                if (cosOmega < 0.0)
                {
                    cosOmega = -cosOmega;
                    Target = Target.Negation;
                }
            }
            else
            {
                // If we are not taking the UseShortestPath we clamp cosOmega to 
                // -1 to stay in the domain of Math.Acos below.
                if (cosOmega < -1.0)
                {
                    cosOmega = -1.0;
                }
            }

            // Clamp cosOmega to [-1,1] to stay in the domain of Math.Acos below.
            // The logic above has either flipped the sign of cosOmega to ensure it 
            // is positive or clamped to -1 aready.  We only need to worry about the
            // upper limit here. 
            if (cosOmega > 1.0)
            {
                cosOmega = 1.0;
            }

            System.Diagnostics.Debug.Assert(!(cosOmega < -1.0) && !(cosOmega > 1.0),
                "cosOmega should be clamped to [-1,1]");

            // The mainline algorithm doesn't work for extreme 
            // cosine values.  For large cosine we have a better 
            // fallback hence the asymmetric limits.
            const double MAX_COSINE = 1.0 - 1E-6;
            const double MIN_COSINE = 1e-10 - 1.0;

            // Calculate scaling coefficients.
            if (cosOmega > MAX_COSINE)
            {
                // Quaternions are too close - use linear interpolation. 
                scaleFrom = 1.0 - Factor;
                scaleTo = Factor;
            }
            else if (cosOmega < MIN_COSINE)
            {
                // Quaternions are nearly opposite, so we will pretend to
                // is exactly -from. 
                // First assign arbitrary perpendicular to "to".
                Target = new Quaternion(-From.Y, From.X, -From.W, From.Z);

                double theta = Factor * Math.PI;

                scaleFrom = Math.Cos(theta);
                scaleTo = Math.Sin(theta);
            }
            else
            {
                // Standard case - use SLERP interpolation. 
                double omega = Math.Acos(cosOmega);
                double sinOmega = Math.Sqrt(1.0 - cosOmega * cosOmega);
                scaleFrom = Math.Sin((1.0 - Factor) * omega) / sinOmega;
                scaleTo = Math.Sin(Factor * omega) / sinOmega;
            }

            double lengthOut = lenFrom * Math.Pow(lenTo / lenFrom, Factor);
            scaleFrom *= lengthOut;
            scaleTo *= lengthOut;

            return (scaleFrom * From) + (scaleTo * Target);
        }
#endif
        public Quaternion SlerpTo(Quaternion Target, double Factor)
        {
            double fActual, fTarget;
            double cos = this.DotProduct(Target);

            // For near-zero angle diff, use linear interpolation
            if ((1.0 - Math.Abs(cos)) > 1.0E-6)
            {
                double angle = Math.Acos(Math.Abs(cos));
                double sin = Math.Sin(angle);
                fActual = Math.Sin((1.0 - Factor) * angle) / sin;
                fTarget = Math.Sin(Factor * angle) / sin;
            }
            else
            {
                fActual = 1.0 - Factor;
                fTarget = Factor;
            }
            if (cos < 0.0)
                fTarget = -fTarget;

            return (this * fActual) + (Target * fTarget);
        }
        public override string ToString()
        {
            return string.Format("W: {0:0.00000} X: {1:0.00000} Y: {2:0.00000} Z: {3:0.00000}", this.W, this.X, this.Y, this.Z);
        }
    }
}