using System;
using System.Collections.Generic;

namespace SolarMax;

internal sealed class ResettableShape : Shape
{
    private readonly List<LineBase> shadowLines;

    public ResettableShape(Shape s)
    {
        s = s.Normalized;

        Dictionary<LineBase, LineBase> copies = [];

        foreach (var l in s.Lines)
        {
            LineBase copy;
            if (l is LineContinuation)
                copy = (l as LineContinuation).Copy(copies[(l as LineContinuation).Parent]);
            else
                copy = l.Copy();
            
            copies.Add(l, copy);
            lines.Add(copy);
        }

        shadowLines = [];
        foreach (var l in lines)
            shadowLines.Add(l.Copy());
    }
    
    public void Reset()
    {
        var target = this.Lines.GetEnumerator();

        foreach (var l in this.shadowLines)
        {
            target.MoveNext();
            target.Current.Overwrite(l);
        }
    }
    public override Shape Normalized
    {
        get { throw new NotImplementedException(); }
    }
}
