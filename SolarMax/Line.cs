using System;
using System.Collections.Generic;
using System.Text;

namespace SolarMax
{
    internal sealed class Line : LineBase
    {
        public override Vector P1 { get; protected set; }
        
        private Line()
        {

        }
        public Line(Vector P1, Vector P2)
        {
            this.P1 = P1;
            this.P2 = P2;
            this.LineType = LineType.Normal;
        }
        public Line(Vector P1, Vector P2, LineType LineType)
        {
            this.P1 = P1;
            this.P2 = P2;
            this.LineType = LineType;
        }
        public override void Overwrite(LineBase Line)
        {
            base.Overwrite(Line);
            P1 = Line.P1;
        }
        public override void Rotate(Quaternion Q)
        {
            base.Rotate(Q);
            P1 = Q.RotateVectorFast(P1);
        }
        public override void Scale(double XScale, double YScale, double ZScale)
        {
            base.Scale(XScale, YScale, ZScale);
            P1 = new Vector(P1.X * XScale, P1.Y * YScale, P1.Z * ZScale);
        }
        public override void Adjust(AdjustDelegate D)
        {
            base.Adjust(D);
            P1 = D(P1);
        }
        public override void Move(Vector Offset)
        {
            base.Move(Offset);
            P1 += Offset;
        }
        public override void RotateAboutXAxis(double Angle)
        {
            base.RotateAboutXAxis(Angle);
            P1 = P1.GetRotationAboutXAxis(Angle);
        }
        public override void RotateAboutYAxis(double Angle)
        {
            base.RotateAboutYAxis(Angle);
            P1 = P1.GetRotationAboutYAxis(Angle);
        }
        public override void RotateAboutZAxis(double Angle)
        {
            base.RotateAboutZAxis(Angle);
            P1 = P1.GetRotationAboutZAxis(Angle);
        }
        public override void RotateAbout(Vector Axis, double Angle)
        {
            base.RotateAbout(Axis, Angle);
            P1 = P1.GetRotationAbout(Axis, Angle);
        }
        public override LineBase GetRotationAboutXAxis(double Angle)
        {
            return new Line(this.P1.GetRotationAboutXAxis(Angle),
                            this.P2.GetRotationAboutXAxis(Angle),
                            this.LineType);
        }
        public override LineBase GetRotationAboutYAxis(double Angle)
        {
            return new Line(this.P1.GetRotationAboutYAxis(Angle),
                            this.P2.GetRotationAboutYAxis(Angle),
                            this.LineType);
        }
        public override LineBase GetRotationAboutZAxis(double Angle)
        {
            return new Line(this.P1.GetRotationAboutZAxis(Angle),
                            this.P2.GetRotationAboutZAxis(Angle),
                            this.LineType);
        }
        public override LineBase GetOffsetLine(Vector Offset)
        {
            return new Line(this.P1 + Offset,
                            this.P2 + Offset,
                            this.LineType);
        }
        public override void Inflate(double Factor)
        {
            base.Inflate(Factor);
            P1 *= Factor;
        }
        public static List<Line> GetLineList(params Vector[] Vectors)
        {
            List<Line> ll = new List<Line>();
            for (int i = 0; i < Vectors.Length - 1; i++)
            {
                ll.Add(new Line(Vectors[i], Vectors[i + 1]));
            }
            return ll;
        }
    }
}
