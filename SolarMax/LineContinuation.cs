using System;

namespace SolarMax;

internal sealed class LineContinuation : LineBase
{
    public LineBase Parent { get; private set; }
    public override Vector P1
    {
        get { return Parent.P2; }
        protected set
        {
#if DEBUG
            throw new NotImplementedException();
#endif
        }
    }
    public LineContinuation(LineBase Parent, Vector P2)
    {
        this.Parent = Parent;
        this.P2 = P2;
        this.LineType = LineType.Normal;
    }
    public LineContinuation(LineBase Parent, Vector P2, LineType LineType)
    {
        this.Parent = Parent;
        this.P2 = P2;
        this.LineType = LineType;
    }
    public LineContinuation Copy(LineBase Parent) => new(Parent, this.P2, this.LineType);
    public override LineBase GetRotationAboutXAxis(double Angle)
        => new LineContinuation(this.Parent,
                                this.P2.GetRotationAboutXAxis(Angle),
                                this.LineType);
    public override LineBase GetRotationAboutYAxis(double Angle) 
        => new LineContinuation(this.Parent,
                                this.P2.GetRotationAboutYAxis(Angle),
                                this.LineType);
    public override LineBase GetRotationAboutZAxis(double Angle) 
        => new LineContinuation(this.Parent,
                                this.P2.GetRotationAboutZAxis(Angle),
                                this.LineType);
    public override LineBase GetOffsetLine(Vector Offset)
        => new LineContinuation(this.Parent,
                                this.P2 + Offset,
                                this.LineType);
}
