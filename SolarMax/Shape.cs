using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax
{
    internal class Shape
    {
        private const double GRID_SCALE = Util.METERS_PER_PARSEC * 200;
        private const double COMPASS_POINT_DISTANCE = 100;
        private const double COMPASS_POINT_SCALE = GRID_SCALE / COMPASS_POINT_DISTANCE;
        private const double COMPASS_LETTER_SPACING = 1.2;
        private const double COMPASS_LETTER_HEIGHT = 1.4;
        private static readonly double COMPASS_POINT_LETTER_WIDTH = Math.Atan2(2.0, COMPASS_POINT_DISTANCE);

        protected List<LineBase> lines;

        public Shape()
        {
            lines = new List<LineBase>();
        }
        public Shape(Shape Template) : this()
        {
            this.AddLines(Template.Copy().Lines);
        }
        public ResettableShape ToResettable()
        {
            return new ResettableShape(this);
        }
        public IEnumerable<LineBase> Lines
        {
            get { return lines; }
        }
        public virtual Shape Copy()
        {
            return copyTo(new Shape());
        }
        private Shape copyTo(Shape s)
        {
            foreach (var l in this.Lines)
                s.AddLine(l.Copy());

            return s;
        }
        
        public virtual void AddLine(LineBase Line)
        {
            this.lines.Add(Line);
        }
        public virtual void AddLines(IEnumerable<LineBase> Lines)
        {
            foreach (var l in Lines)
                this.lines.Add(l);
        }
        public void AddLineCopies(IEnumerable<LineBase> Lines)
        {
            foreach (var l in Lines)
                this.lines.Add(l.Copy());
        }
        public void AddLines(LineBase Line, IEnumerable<Vector> Continuations)
        {
            AddLine(Line);

            LineBase l = Line;
            foreach (var vv in Continuations)
            {
                l = new LineContinuation(l, vv);
                AddLine(l);
            }
        }
        public static Shape GetAggregate(List<Shape> Shapes)
        {
            Shape s = new Shape();

            foreach (var ss in Shapes)
            {
                foreach (var l in ss.Lines)
                    s.AddLine(l.Copy());
            }
            return s;
        }
        public void Rotate(Quaternion Q)
        {
            foreach (var l in this.Lines)
                l.Rotate(Q);
        }
        public void Move(Vector Offset)
        {
            foreach (var l in this.Lines)
                l.Move(Offset);
        }
        public void Scale(double XScale, double YScale, double ZScale)
        {
            foreach (var l in this.Lines)
                l.Scale(XScale, YScale, ZScale);
        }
        public void Inflate(double Factor)
        {
            foreach (var l in this.Lines)
                l.Inflate(Factor);
        }
        public void RotateAboutXAxis(double Angle)
        {
            if (Angle != 0.0)
            {
                foreach (var l in this.Lines)
                    l.RotateAboutXAxis(Angle);
            }
        }
        public void RotateAboutYAxis(double Angle)
        {
            if (Angle != 0.0)
            {
                foreach (var l in this.Lines)
                    l.RotateAboutYAxis(Angle);
            }
        }
        public void RotateAboutZAxis(double Angle)
        {
            if (Angle != 0.0)
            {
                foreach (var l in this.Lines)
                    l.RotateAboutZAxis(Angle);
            }
        }
        public Shape GetRotationAboutXAxis(double Angle)
        {
            Shape s = this.Copy();
            s.RotateAboutXAxis(Angle);
            return s;
        }
        public Shape GetRotationAboutYAxis(double Angle)
        {
            Shape s = this.Copy();
            s.RotateAboutYAxis(Angle);
            return s;
        }
        public Shape GetRotationAboutZAxis(double Angle)
        {
            Shape s = this.Copy();
            s.RotateAboutZAxis(Angle);
            return s;
        }
        public Shape GetRotationAbout(Vector Axis, double Angle)
        {
            Shape s = this.Copy();
            s.RotateAbout(Axis, Angle);
            return s;
        }
        public void RotateAbout(Vector Axis, double Angle)
        {
            if (Angle != 0.0)
            {
                foreach (var l in this.Lines)
                    l.RotateAbout(Axis, Angle);
            }
        }
        public void MarkAllLines(LineType LineType)
        {
            foreach (var l in this.Lines)
                l.LineType = LineType;
        }
        public static Shape GetCube(double SizeLength)
        {
            Shape rs = new Shape();

            LineBase l = new Line(new Vector(-SizeLength, -SizeLength, -SizeLength),
                                  new Vector(SizeLength,  -SizeLength, -SizeLength));

            rs.AddLines(l, new List<Vector>() { new Vector( SizeLength,  SizeLength, -SizeLength),
                                                new Vector(-SizeLength,  SizeLength, -SizeLength),
                                                new Vector(-SizeLength, -SizeLength, -SizeLength),
                                                new Vector(-SizeLength, -SizeLength,  SizeLength),
                                                new Vector( SizeLength, -SizeLength,  SizeLength),
                                                new Vector( SizeLength,  SizeLength,  SizeLength),
                                                new Vector(-SizeLength,  SizeLength,  SizeLength),
                                                new Vector(-SizeLength, -SizeLength,  SizeLength) } );

            rs.AddLine(new Line(new Vector( SizeLength,  SizeLength, -SizeLength),
                                new Vector( SizeLength,  SizeLength,  SizeLength)));
            rs.AddLine(new Line(new Vector( SizeLength, -SizeLength, -SizeLength),
                                new Vector( SizeLength, -SizeLength,  SizeLength)));
            rs.AddLine(new Line(new Vector(-SizeLength,  SizeLength, -SizeLength),
                                new Vector(-SizeLength,  SizeLength,  SizeLength)));

            return rs;
        }
        public static Shape GetSphere(int NumPointsPerCircle, int NumLongitudeLines, int NumLatitudeLines, double Radius, int NumRings = 0, double Oblateness = 0, bool HighlightPrimeMerdian = false, bool HighlightAntiPrimeMerdian = false, bool HighlightEquator = false, bool Hemisphere = false)
        {
            var sphere = new Shape();

            double increment = Math.PI / (double)NumLatitudeLines;
            double angle = increment;
            
            int steps = Hemisphere ? NumLatitudeLines / 2 : NumLatitudeLines - 1;
            int equatorIndex = Hemisphere ? steps - 1 : steps / 2;

            for (int i = 0; i < steps; i++)
            {
                var circle = GetCircle(NumPointsPerCircle, Radius);

                if (HighlightEquator && i == equatorIndex)
                    circle.MarkAllLines(LineType.Special);
                circle.Inflate(Math.Sin(angle));
                circle.Move(new Vector(0, 0, Math.Cos(angle) * Radius));
                sphere.AddLines(circle.Lines);
                angle += increment;
            }

            for (int i = 0; i < NumRings; i++)
            {
                var circle = GetCircle(NumPointsPerCircle, Radius * (2.0 - ((double)i) / (double)NumRings));
                circle.MarkAllLines(LineType.Ring);
                sphere.AddLines(circle.lines);
            }

            angle = 0.0;
            increment = MathEx.TWO_PI / (double)NumLongitudeLines;
            for (int i = 0; i < NumLongitudeLines; i++)
            {
                var halfCircle = Hemisphere ? GetQuarterCircle(NumPointsPerCircle / 2, Radius) : GetHalfCircle(NumPointsPerCircle / 2, Radius);
                halfCircle.RotateAboutYAxis(-MathEx.HALF_PI);
                halfCircle.RotateAboutZAxis(-MathEx.HALF_PI);
            
                if ((HighlightPrimeMerdian && i == 0) || (HighlightAntiPrimeMerdian && i == NumLongitudeLines / 2))
                    halfCircle.MarkAllLines(LineType.Special);

                if (angle != 0.0)
                    halfCircle.RotateAboutZAxis(angle);

                sphere.AddLines(halfCircle.Lines);
                angle += increment;
            }
            if (Oblateness != 0)
            {
                sphere.Scale(1, 1, 1 - Oblateness);
            }
            return sphere;
        }
        public static Shape GetQuarterCircle(int LineSteps, double Radius)
        {
            return getCircle(LineSteps, Radius, MathEx.HALF_PI);
        }
        public static Shape GetHalfCircle(int LineSteps, double Radius)
        {
            return getCircle(LineSteps, Radius, Math.PI);
        }
        public static Shape GetCircle(int LineSteps, double Radius)
        {
            return getCircle(LineSteps, Radius, MathEx.TWO_PI);
        }
        private static Shape getCircle(int LineSteps, double Radius, double Arc)
        {
            var circle = new Shape();
            double increment = Arc / (double)LineSteps;
            double angle = 0.0;
            for (int i = 0; i < LineSteps; i++)
            {
                circle.AddLine(new Line(new Vector(Math.Cos(angle) * Radius, Math.Sin(angle) * Radius, 0),
                                        new Vector(Math.Cos(angle + increment) * Radius, Math.Sin(angle + increment) * Radius, 0)));
                angle += increment;
            }
            return circle;
        }
        public virtual Shape Normalized
        {
            get
            {
                LineBase[] old = new LineBase[lines.Count];
                this.lines.CopyTo(old);

                List<LineBase> newLines = new List<LineBase>();

                LineBase l = null;
                for (int i = 0; i < old.Length; i++)
                {
                    if (l != null && l.P2 == old[i].P1)
                        l = new LineContinuation(l, old[i].P2, old[i].LineType);
                    else
                        l = new Line(old[i].P1, old[i].P2, old[i].LineType);
                    newLines.Add(l);
                }
                var s = new Shape();
                s.AddLines(newLines);
                return s;
            }
        }
        public static Shape GetLatitudePointsNumeric(bool IncludeNegativeInclinations)
        {
            Shape one, two, three, four, five, six, seven, eight, nine, zero;
            getNumberGlyphs(out zero, out one, out two, out three, out four, out five, out six, out seven, out eight, out nine);

            double rotateAngle = COMPASS_POINT_LETTER_WIDTH * COMPASS_LETTER_SPACING;
            List<Shape> points = new List<Shape>();

            var plus = new Shape();
            plus.AddLines(Line.GetLineList(new Vector(COMPASS_POINT_DISTANCE, 0, COMPASS_LETTER_HEIGHT),
                                           new Vector(COMPASS_POINT_DISTANCE, 0, -COMPASS_LETTER_HEIGHT)));
            plus.AddLines(Line.GetLineList(new Vector(COMPASS_POINT_DISTANCE, -1, 0),
                                           new Vector(COMPASS_POINT_DISTANCE, 1, 0)));

            var minus = new Shape();
            minus.AddLines(Line.GetLineList(new Vector(COMPASS_POINT_DISTANCE, -1, 0),
                                           new Vector(COMPASS_POINT_DISTANCE, 1, 0)));

            var p15 = new Shape();
            addGlyphs(p15, plus, one, five);
            p15.RotateAboutZAxis(Math.PI * 1.0 / 12.0 - rotateAngle);
            points.Add(p15);

            var p30 = new Shape();
            addGlyphs(p30, plus, three, zero);
            p30.RotateAboutZAxis(Math.PI * 2.0 / 12.0 - rotateAngle);
            points.Add(p30);

            var p45 = new Shape();
            addGlyphs(p45, plus, four, five);
            p45.RotateAboutZAxis(Math.PI * 3.0 / 12.0 - rotateAngle);
            points.Add(p45);

            var p60 = new Shape();
            addGlyphs(p60, plus, six, zero);
            p60.RotateAboutZAxis(Math.PI * 4.0 / 12.0 - rotateAngle);
            points.Add(p60);

            var p75 = new Shape();
            addGlyphs(p75, plus, seven, five);
            p75.RotateAboutZAxis(Math.PI * 5.0 / 12.0 - rotateAngle);
            points.Add(p75);

            var p90 = new Shape();
            addGlyphs(p90, plus, nine, zero);
            p90.RotateAboutZAxis(Math.PI * 6.0 / 12.0 - rotateAngle);
            points.Add(p90);

            if (IncludeNegativeInclinations)
            {
                var m15 = new Shape();
                addGlyphs(m15, minus, one, five);
                m15.RotateAboutZAxis(-Math.PI * 1.0 / 12.0 - rotateAngle);
                points.Add(m15);

                var m30 = new Shape();
                addGlyphs(m30, minus, three, zero);
                m30.RotateAboutZAxis(-Math.PI * 2.0 / 12.0 - rotateAngle);
                points.Add(m30);

                var m45 = new Shape();
                addGlyphs(m45, minus, four, five);
                m45.RotateAboutZAxis(-Math.PI * 3.0 / 12.0 - rotateAngle);
                points.Add(m45);

                var m60 = new Shape();
                addGlyphs(m60, minus, six, zero);
                m60.RotateAboutZAxis(-Math.PI * 4.0 / 12.0 - rotateAngle);
                points.Add(m60);

                var m75 = new Shape();
                addGlyphs(m75, minus, seven, five);
                m75.RotateAboutZAxis(-Math.PI * 5.0 / 12.0 - rotateAngle);
                points.Add(m75);

                var m90 = new Shape();
                addGlyphs(m90, minus, nine, zero);
                m90.RotateAboutZAxis(-Math.PI * 6.0 / 12.0 - rotateAngle);
                points.Add(m90);

                m75 = m75.Copy();
                m75.RotateAboutZAxis(-Math.PI * 2.0 / 12.0);
                points.Add(m75);

                m60 = m60.Copy();
                m60.RotateAboutZAxis(-Math.PI * 4.0 / 12.0);
                points.Add(m60);

                m45 = m45.Copy();
                m45.RotateAboutZAxis(-Math.PI * 6.0 / 12.0);
                points.Add(m45);

                m30 = m30.Copy();
                m30.RotateAboutZAxis(-Math.PI * 8.0 / 12.0);
                points.Add(m30);

                m15 = m15.Copy();
                m15.RotateAboutZAxis(-Math.PI * 10.0 / 12.0);
                points.Add(m15);

            }
            p75 = p75.Copy();
            p75.RotateAboutZAxis(Math.PI * 2.0 / 12.0);
            points.Add(p75);

            p60 = p60.Copy();
            p60.RotateAboutZAxis(Math.PI * 4.0 / 12.0);
            points.Add(p60);

            p45 = p45.Copy();
            p45.RotateAboutZAxis(Math.PI * 6.0 / 12.0);
            points.Add(p45);

            p30 = p30.Copy();
            p30.RotateAboutZAxis(Math.PI * 8.0 / 12.0);
            points.Add(p30);

            p15 = p15.Copy();
            p15.RotateAboutZAxis(Math.PI * 10.0 / 12.0);
            points.Add(p15);

            var s = Shape.GetAggregate(points);
            s.Move(new Vector(0, 0, COMPASS_LETTER_HEIGHT * 1.4));
            s.RotateAboutXAxis(MathEx.HALF_PI);
            s.MarkAllLines(LineType.Special);
            s.Inflate(COMPASS_POINT_SCALE);
            return s;
        }

        private static void getNumberGlyphs(out Shape zero, out Shape one, out Shape two, out Shape three, out Shape four, out Shape five, out Shape six, out Shape seven, out Shape eight, out Shape nine)
        {
            zero = new Shape();
            zero.AddLines(Line.GetLineList(new Vector(COMPASS_POINT_DISTANCE, 1, COMPASS_LETTER_HEIGHT),
                                           new Vector(COMPASS_POINT_DISTANCE, -1, COMPASS_LETTER_HEIGHT),
                                           new Vector(COMPASS_POINT_DISTANCE, -1, -COMPASS_LETTER_HEIGHT),
                                           new Vector(COMPASS_POINT_DISTANCE, 1, -COMPASS_LETTER_HEIGHT),
                                           new Vector(COMPASS_POINT_DISTANCE, 1, COMPASS_LETTER_HEIGHT)));

            nine = new Shape();
            nine.AddLines(Line.GetLineList(new Vector(COMPASS_POINT_DISTANCE, 1, COMPASS_LETTER_HEIGHT),
                                           new Vector(COMPASS_POINT_DISTANCE, -1, COMPASS_LETTER_HEIGHT),
                                           new Vector(COMPASS_POINT_DISTANCE, -1, 0),
                                           new Vector(COMPASS_POINT_DISTANCE, 1, 0),
                                           new Vector(COMPASS_POINT_DISTANCE, 1, COMPASS_LETTER_HEIGHT)));
            nine.AddLines(Line.GetLineList(new Vector(COMPASS_POINT_DISTANCE, -1, 0),
                                           new Vector(COMPASS_POINT_DISTANCE, -1, -COMPASS_LETTER_HEIGHT),
                                           new Vector(COMPASS_POINT_DISTANCE, 1, -COMPASS_LETTER_HEIGHT)));

            one = new Shape();
            one.AddLines(Line.GetLineList(new Vector(COMPASS_POINT_DISTANCE, 0, -COMPASS_LETTER_HEIGHT),
                                           new Vector(COMPASS_POINT_DISTANCE, 0, COMPASS_LETTER_HEIGHT),
                                           new Vector(COMPASS_POINT_DISTANCE, 0.4, COMPASS_LETTER_HEIGHT * 0.6)));

            eight = new Shape();
            eight.AddLines(zero.Copy().Lines);
            eight.AddLine(new Line(new Vector(COMPASS_POINT_DISTANCE, -1, 0), new Vector(COMPASS_POINT_DISTANCE, 1, 0)));

            two = new Shape();
            two.AddLines(Line.GetLineList(new Vector(COMPASS_POINT_DISTANCE, 1, COMPASS_LETTER_HEIGHT),
                                          new Vector(COMPASS_POINT_DISTANCE, -1, COMPASS_LETTER_HEIGHT),
                                          new Vector(COMPASS_POINT_DISTANCE, -1, 0),
                                          new Vector(COMPASS_POINT_DISTANCE, 1, 0),
                                          new Vector(COMPASS_POINT_DISTANCE, 1, -COMPASS_LETTER_HEIGHT),
                                          new Vector(COMPASS_POINT_DISTANCE, -1, -COMPASS_LETTER_HEIGHT)));

            three = new Shape();
            three.AddLines(Line.GetLineList(new Vector(COMPASS_POINT_DISTANCE, 1, COMPASS_LETTER_HEIGHT),
                                            new Vector(COMPASS_POINT_DISTANCE, -1, COMPASS_LETTER_HEIGHT),
                                            new Vector(COMPASS_POINT_DISTANCE, 0, 0),
                                            new Vector(COMPASS_POINT_DISTANCE, -1, 0),
                                            new Vector(COMPASS_POINT_DISTANCE, -1, -COMPASS_LETTER_HEIGHT),
                                            new Vector(COMPASS_POINT_DISTANCE, 1, -COMPASS_LETTER_HEIGHT)));


            seven = new Shape();
            seven.AddLines(Line.GetLineList(new Vector(COMPASS_POINT_DISTANCE, 1, COMPASS_LETTER_HEIGHT),
                                            new Vector(COMPASS_POINT_DISTANCE, -1, COMPASS_LETTER_HEIGHT),
                                            new Vector(COMPASS_POINT_DISTANCE, 0, -COMPASS_LETTER_HEIGHT)));

            five = new Shape();
            five.AddLines(Line.GetLineList(new Vector(COMPASS_POINT_DISTANCE, -1, COMPASS_LETTER_HEIGHT),
                                           new Vector(COMPASS_POINT_DISTANCE, 1, COMPASS_LETTER_HEIGHT),
                                           new Vector(COMPASS_POINT_DISTANCE, 1, 0),
                                           new Vector(COMPASS_POINT_DISTANCE, -1, 0),
                                           new Vector(COMPASS_POINT_DISTANCE, -1, -COMPASS_LETTER_HEIGHT),
                                           new Vector(COMPASS_POINT_DISTANCE, 1, -COMPASS_LETTER_HEIGHT)));

            four = new Shape();
            four.AddLines(Line.GetLineList(new Vector(COMPASS_POINT_DISTANCE, -1, COMPASS_LETTER_HEIGHT),
                                           new Vector(COMPASS_POINT_DISTANCE, -1, -COMPASS_LETTER_HEIGHT)));
            four.AddLines(Line.GetLineList(new Vector(COMPASS_POINT_DISTANCE, 0.7, COMPASS_LETTER_HEIGHT * 0.8),
                                           new Vector(COMPASS_POINT_DISTANCE, 1, 0),
                                           new Vector(COMPASS_POINT_DISTANCE, -1, 0)));

            six = nine.GetRotationAboutXAxis(Math.PI);
        }
        public static Shape GetEclipticGrid()
        {
            var s = Shape.GetAggregate(new List<Shape>() { Shape.GetSphere(36, 24, 12, GRID_SCALE, 0, 0, false, false, true), Shape.getCompassPointsNumeric(), Shape.GetLatitudePointsNumeric(true) });
            return s;
        }
        public static Shape GetEquatorialGrid()
        {
            var s = Shape.GetAggregate(new List<Shape>() { Shape.GetSphere(36, 24, 12, GRID_SCALE, 0, 0, false, false, true), Shape.getCompassPointsNumeric(), Shape.GetLatitudePointsNumeric(true) });
            s.RotateAbout(Vector.UnitZ ^ CelestialBody.Earth.Axis, CelestialBody.Earth.Axis.Tilt);
            return s;
        }
        
        public static Shape GetLocalGrid(bool WithRefraction)
        {
            var s = Shape.GetAggregate(new List<Shape>() { Shape.GetSphere(36, 32, 12, GRID_SCALE, 0, 0, false, false, true, true),
                                                           Shape.getCompassPointLetters(),
                                                           Shape.GetLatitudePointsNumeric(false) });

            if (WithRefraction)
                refract(s);

            return s;
        }
        private static void refract(Shape S)
        {
            foreach (var l in S.lines)
                refract(l);
        }
        private static void refract(LineBase l)
        {
            l.Adjust((Vector v) => v.GetRotationAbout(v ^ Vector.UnitZ, Util.GetInverseRefractionCorrection(v.Inclination)));
        }
        private static Shape getCompassPointsNumeric()
        {
            Shape one, two, three, four, five, six, seven, eight, nine, zero;
            getNumberGlyphs(out zero, out one, out two, out three, out four, out five, out six, out seven, out eight, out nine);
            Shape[] glyphs = new Shape[] { zero, one, two, three, four, five, six, seven, eight, nine };

            double rotateAngle = COMPASS_POINT_LETTER_WIDTH * COMPASS_LETTER_SPACING;
            List<Shape> points = new List<Shape>();

            for (int i = 0; i < 360; i += 15)
            {
                var pt = new Shape();
                var deg = string.Format("{0:000}", i);
                addGlyphs(pt, glyphs[deg[0] - '0'], glyphs[deg[1] - '0'], glyphs[deg[2] - '0']);
                pt.RotateAboutZAxis(-((double)i).ToRadiansFromDegrees() - rotateAngle);
                points.Add(pt);
            }

            var s = Shape.GetAggregate(points);
            s.Move(new Vector(0, 0, COMPASS_LETTER_HEIGHT * 1.4));
            s.MarkAllLines(LineType.Special);
            s.Inflate(COMPASS_POINT_SCALE);
            return s;
        }

        private static Shape getCompassPointLetters()
        {
            var points = new List<Shape>();
            double rotateAngle = COMPASS_POINT_LETTER_WIDTH * COMPASS_LETTER_SPACING / 2.0;

            Shape north = new Shape();
            north.AddLines(Line.GetLineList(new Vector(COMPASS_POINT_DISTANCE, 1, -COMPASS_LETTER_HEIGHT),
                                            new Vector(COMPASS_POINT_DISTANCE, 1, COMPASS_LETTER_HEIGHT),
                                            new Vector(COMPASS_POINT_DISTANCE, -1, -COMPASS_LETTER_HEIGHT),
                                            new Vector(COMPASS_POINT_DISTANCE, -1, COMPASS_LETTER_HEIGHT)));

            Shape south = new Shape();
            south.AddLines(Line.GetLineList(new Vector(COMPASS_POINT_DISTANCE, -1, COMPASS_LETTER_HEIGHT),
                                            new Vector(COMPASS_POINT_DISTANCE, 1, COMPASS_LETTER_HEIGHT),
                                            new Vector(COMPASS_POINT_DISTANCE, 1, 0),
                                            new Vector(COMPASS_POINT_DISTANCE, -1, 0),
                                            new Vector(COMPASS_POINT_DISTANCE, -1, -COMPASS_LETTER_HEIGHT),
                                            new Vector(COMPASS_POINT_DISTANCE, 1, -COMPASS_LETTER_HEIGHT)));

            Shape west = new Shape();
            west.AddLines(Line.GetLineList(new Vector(COMPASS_POINT_DISTANCE, 1, COMPASS_LETTER_HEIGHT),
                                           new Vector(COMPASS_POINT_DISTANCE, 0.5, -COMPASS_LETTER_HEIGHT),
                                           new Vector(COMPASS_POINT_DISTANCE, 0, COMPASS_LETTER_HEIGHT),
                                           new Vector(COMPASS_POINT_DISTANCE, -0.5, -COMPASS_LETTER_HEIGHT),
                                           new Vector(COMPASS_POINT_DISTANCE, -1, COMPASS_LETTER_HEIGHT)));

            Shape east = new Shape();
            east.AddLines(Line.GetLineList(new Vector(COMPASS_POINT_DISTANCE, -1, COMPASS_LETTER_HEIGHT),
                                           new Vector(COMPASS_POINT_DISTANCE, 1, COMPASS_LETTER_HEIGHT),
                                           new Vector(COMPASS_POINT_DISTANCE, 1, -COMPASS_LETTER_HEIGHT),
                                           new Vector(COMPASS_POINT_DISTANCE, -1, -COMPASS_LETTER_HEIGHT)));
            east.AddLine(new Line(new Vector(COMPASS_POINT_DISTANCE, 1, 0), new Vector(COMPASS_POINT_DISTANCE, -1, 0)));

            points.Add(north.Copy());
            points.Add(west.GetRotationAboutZAxis(MathEx.HALF_PI));
            points.Add(south.GetRotationAboutZAxis(Math.PI));
            points.Add(east.GetRotationAboutZAxis(3.0 * MathEx.HALF_PI));

            Shape nw = new Shape();
            addGlyphs(nw, north, west);
            nw.RotateAboutZAxis(MathEx.HALF_PI / 2.0 - rotateAngle);
            points.Add(nw);

            Shape ne = new Shape();
            addGlyphs(ne, north, east);
            ne.RotateAboutZAxis(-MathEx.HALF_PI / 2.0 - rotateAngle);
            points.Add(ne);

            Shape sw = new Shape();
            addGlyphs(sw, south, west);
            sw.RotateAboutZAxis(Math.PI * 0.75 - rotateAngle);
            points.Add(sw);

            Shape se = new Shape();
            addGlyphs(se, south, east);
            se.RotateAboutZAxis(-Math.PI * 0.75 - rotateAngle);
            points.Add(se);

            rotateAngle *= 2.0;

            Shape nnw = new Shape();
            addGlyphs(nnw, north, north, west);
            nnw.RotateAboutZAxis(Math.PI * 0.125 - rotateAngle);
            points.Add(nnw);

            Shape wnw = new Shape();
            addGlyphs(wnw, west, north, west);
            wnw.RotateAboutZAxis(Math.PI * 0.375 - rotateAngle);
            points.Add(wnw);

            Shape wsw = new Shape();
            addGlyphs(wsw, west, south, west);
            wsw.RotateAboutZAxis(Math.PI * 0.625 - rotateAngle);
            points.Add(wsw);

            Shape ssw = new Shape();
            addGlyphs(ssw, south, south, west);
            ssw.RotateAboutZAxis(Math.PI * 0.875 - rotateAngle);
            points.Add(ssw);

            Shape sse = new Shape();
            addGlyphs(sse, south, south, east);
            sse.RotateAboutZAxis(Math.PI * 1.125 - rotateAngle);
            points.Add(sse);

            Shape ese = new Shape();
            addGlyphs(ese, east, south, east);
            ese.RotateAboutZAxis(Math.PI * 1.375 - rotateAngle);
            points.Add(ese);

            Shape ene = new Shape();
            addGlyphs(ene, east, north, east);
            ene.RotateAboutZAxis(Math.PI * 1.625 - rotateAngle);
            points.Add(ene);

            Shape nne = new Shape();
            addGlyphs(nne, north, north, east);
            nne.RotateAboutZAxis(Math.PI * 1.875 - rotateAngle);
            points.Add(nne);

            var s = Shape.GetAggregate(points);
            s.Move(new Vector(0, 0, COMPASS_LETTER_HEIGHT * 1.4));
            s.RotateAboutZAxis(Math.PI);
            s.Inflate(COMPASS_POINT_SCALE);
            s.MarkAllLines(LineType.Special);

            return s;
        }

        private static void addGlyphs(Shape Target, params Shape[] Glyphs)
        {
            for (int i = 0; i < Glyphs.Length - 1; i++)
            {
                Target.AddLines(Glyphs[i].Copy().Lines);
                Target.RotateAboutZAxis(COMPASS_POINT_LETTER_WIDTH * COMPASS_LETTER_SPACING);
            }
            Target.AddLines(Glyphs[Glyphs.Length - 1].Copy().Lines);
        }
        public override string ToString()
        {
            return string.Format("Shape: {0} Lines {1} Normal {2} Continuations", this.lines.Count, this.lines.Where(l => l is Line).Count(), this.lines.Where(l => l is LineContinuation).Count());
        }
    }
}
