namespace SolarMax;
internal enum LineType { Normal, Ring, Special }

internal abstract class LineBase
{
    public delegate Vector AdjustDelegate(Vector V);

    public LineType LineType { get; set; }
    public bool IsInFront { get; set; }

    public abstract Vector P1 { get; protected set; }
    public Vector P2 { get; protected set; }

    public LineBase Copy() => new Line(this.P1, this.P2, this.LineType);

    public virtual void Overwrite(LineBase Line)
    {
        P2 = Line.P2;
    }
    public virtual void Move(Vector Offset)
    {
        P2 += Offset;
    }
    public virtual void Scale(double XScale, double YScale, double ZScale)
    {
        P2 = new Vector(P2.X * XScale, P2.Y * YScale, P2.Z * ZScale);
    }
    public virtual void Adjust(AdjustDelegate D)
    {
        P2 = D(P2);
    }
    public virtual void Inflate(double Factor)
    {
        P2 *= Factor;
    }
    public virtual void Rotate(Quaternion Q)
    {
        P2 = Q.RotateVectorFast(P2);
    }
    public virtual void RotateAboutXAxis(double Angle)
    {
        P2 = P2.GetRotationAboutXAxis(Angle);
    }
    public virtual void RotateAboutYAxis(double Angle)
    {
        P2 = P2.GetRotationAboutYAxis(Angle);
    }
    public virtual void RotateAboutZAxis(double Angle)
    {
        P2 = P2.GetRotationAboutZAxis(Angle);
    }
    public virtual void RotateAbout(Vector Axis, double Angle)
    {
        P2 = P2.GetRotationAbout(Axis, Angle);
    }
    public abstract LineBase GetRotationAboutXAxis(double Angle);
    public abstract LineBase GetRotationAboutYAxis(double Angle);
    public abstract LineBase GetRotationAboutZAxis(double Angle);
    public abstract LineBase GetOffsetLine(Vector Offset);

    public override bool Equals(object Obj)
    {
        return Obj is LineBase lb && lb == this;
    }
    public bool Equals(LineBase V)
    {
        if (V == null)
            return false;
        return this == V;
    }
    public override int GetHashCode()
    {
        return P1.GetHashCode() ^ P2.GetHashCode();
    }
    public static bool operator ==(LineBase A, LineBase B)
    {
        if (object.ReferenceEquals(A, B))
            return true;

        if ((A is null) || (B is null))
            return false;

        // Return true if the fields match:
        return A.P1 == B.P1 && A.P2 == B.P2;
    }

    public static bool operator !=(LineBase A, LineBase B)
    {
        return !(A == B);
    }
    public override string ToString()
    {
        return $"[{P1}->{P2}]";
    }
}