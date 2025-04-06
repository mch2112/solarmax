using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using SolarMax.Controllers;

namespace SolarMax;

internal sealed class Renderer : IRenderer
{
    public const float AUTO_CAPTION_THRESHOLD = 0.03f;
    public const string DEFAULT_FONT_NAME = "Calibri";
    public const string DEFAULT_SMALL_FONT_NAME = "Calibri";

    private const float DRAW_AXIS_THRESHOLD = 10f;
    private const double AXIS_SCALE = 1.2;

    public const int MAX_RENDER_LOCATIONS = 15000;

    private readonly Projector projector;

    private QSize screenSize = QSize.Empty;
    
    public QFont SmallFont { get; set; }
    public QFont SmallItalicFont { get; set; }
    public QFont Font { get; set; }
    public QFont LargeFont { get; set; }
    public QFont LargeItalicFont { get; set; }
    public QFont ExtraLargeFont { get; set; }

    private readonly List<Orbiter> orbitersToRender = [];
    private readonly List<CelestialBody> starsToRender = [];

    private readonly QPen primeMeridianPen = new(Colors.GetColor("prime_meridian"));
    private readonly QPen primeMeridianDarkPen = new(Colors.GetColor("prime_meridian_dark")); 
    private readonly QPen planetAxisPen = new(Colors.GetColor("planet_axis"));
    private readonly QPen sunlitArea = new(Colors.GetColor("sunlit_area"));

    public bool HighlightSunlitAreas { get; set; }
    public List<CelestialBody> RenderLocations { get; private set; }
    public int NumRenderLocations { get; private set; }

    public Renderer(ScreenSaverMode ScreenSaverMode, Projector Projector)
    {
        this.projector = Projector;
        switch (ScreenSaverMode)
        {
            case ScreenSaverMode.ScreenSaverPreview:
                this.SmallFont = new QFont(DEFAULT_SMALL_FONT_NAME, 4);
                this.SmallItalicFont = new QFont(DEFAULT_SMALL_FONT_NAME, 4, true);
                this.Font = new QFont(DEFAULT_SMALL_FONT_NAME, 5);
                this.LargeFont = new QFont(DEFAULT_FONT_NAME, 6);
                this.LargeItalicFont = new QFont(DEFAULT_FONT_NAME, 6.5f, true);
                this.ExtraLargeFont = new QFont(DEFAULT_FONT_NAME, 7);
                break;
            default:
                this.SmallFont = new QFont(DEFAULT_SMALL_FONT_NAME, Preferences.LabelFontSize);
                this.SmallItalicFont = new QFont(DEFAULT_SMALL_FONT_NAME, Preferences.LabelFontSize, true);
                this.Font = new QFont(DEFAULT_SMALL_FONT_NAME, 10);
                this.LargeFont = new QFont(DEFAULT_FONT_NAME, 12);
                this.LargeItalicFont = new QFont(DEFAULT_FONT_NAME, 13, true);
                this.ExtraLargeFont = new QFont(DEFAULT_FONT_NAME, 16);
                break;
        }

        RenderLocations = new List<CelestialBody>(MAX_RENDER_LOCATIONS);

        for (int i = 0; i < MAX_RENDER_LOCATIONS; i++)
            RenderLocations.Add(null);
    }
    public Graphics DrawingTarget { get; set; }
    public bool WireFrameBodyRender { get; set; }

    public void DrawLine(QPen Pen, QPoint P1, QPoint P2)
    {
        DrawingTarget.DrawLine(Pen.Pen, P1.X, P1.Y, P2.X, P2.Y);
    }
    public void DrawLine(QPen Pen, float X1, float Y1, float X2, float Y2)
    {
        DrawingTarget.DrawLine(Pen.Pen, X1, Y1, X2, Y2);
    }
    public void DrawEllipse(QPoint Center, QSize Size, QPen Pen)
    {
        DrawingTarget.DrawEllipse(Pen, Center, Size);
    }
    public void DrawCircle(QPoint Center, float Radius, QPen Pen)
    {
        DrawingTarget.DrawCircle(Pen, Center, Radius);
    }
    public void DrawArc(QRectangle Rectangle, float StartAngle, float SweepAngle, QPen Pen)
    {
        DrawingTarget.DrawArc(Pen.Pen, Rectangle.X, Rectangle.Y, Rectangle.Width, Rectangle.Height, StartAngle, SweepAngle);
    }
    public void DrawArc(QRectangle Rectangle, double StartAngle, double SweepAngle, QPen Pen)
    {
        this.DrawArc(Rectangle, (float)StartAngle, (float)SweepAngle, Pen);
    }
    public void DrawRectangle(QPoint Location, QSize Size, QPen Pen)
    {
        DrawingTarget.DrawRectangle(Pen, Location, Size);
    }
    public void DrawRectangle(QRectangle Rectangle, QPen Pen)
    {
        DrawingTarget.DrawRectangle(Pen, Rectangle.Location, Rectangle.Size);
    }
    public void FillRectangle(QRectangle Rectangle, QPen FillPen, QPen BorderPen)
    {
        DrawingTarget.FillRectangle(FillPen, Rectangle);
        DrawingTarget.DrawRectangle(BorderPen, Rectangle);
    }
    public void FillRectangle(QPoint Location, QSize Size, QPen Pen)
    {
        DrawingTarget.FillRectangle(Pen, Location, Size);
    }
    public void FillRectangle(QPoint Location, QSize Size, QPen FillPen, QPen BorderPen)
    {
        this.FillRectangle(Location, Size, FillPen);
        DrawingTarget.DrawRectangle(BorderPen, Location, Size);
    }
    public void RenderConstellation(Constellation Constellation, bool WithCaption)
    {
        this.RenderShape(Constellation.Shape, Constellation.FrontPen);
        if (projector.Project2DPoint(Constellation.PositionSnapshot, true, out QPoint p))
        {
            if (WithCaption)
                this.DrawStringCentered(Constellation.Name, p, Constellation.CaptionPen, this.SmallItalicFont);
            Constellation.RenderPoint = p;
            RenderLocations[NumRenderLocations++] = Constellation;
        }
    }
    public void RenderShape(Shape Shape, QPen Pen)
    {
        foreach (var l in Shape.Lines)
            if (projector.Project2DLine(l.P1, l.P2, out QPoint p1, out QPoint p2))
                DrawingTarget.DrawLine(Pen, p1, p2);
    }

    public void SetupRenderPass()
    {
        orbitersToRender.Clear();
        starsToRender.Clear();
        NumRenderLocations = 0;
    }
    public void StageBodyForRender(Star Star)
    {
        if (projector.Project2DPoint(Star.PositionSnapshot, true, out QPoint pf))
        {
            Star.RenderPoint = pf;
            Star.DistanceFromCamera = (Star.PositionSnapshot - projector.Position).Magnitude;
            starsToRender.Add(Star);
        }
    }
    public void StageBodyForRender(Orbiter Orbiter)
    {
        if (projector.Project2DPoint(Orbiter.PositionSnapshot, false, out QPoint pf))
        {
            Orbiter.RenderPoint = pf;
            Orbiter.DistanceFromCamera = (Orbiter.PositionSnapshot - projector.Position).Magnitude;
            orbitersToRender.Add(Orbiter);
        }
    }
    public void DrawBodies(CaptionMode CaptionMode, double Zoom)
    {
        foreach (var ttr in starsToRender)
            RenderBody(ttr, CaptionMode, Zoom);

        foreach (var ttr in orbitersToRender.OrderByDescending(ttr => ttr.DistanceFromCamera)) // draw further things first
            RenderBody(ttr, CaptionMode, Zoom);
    }
    private void RenderBody(CelestialBody Body, CaptionMode CaptionMode, double Zoom)
    {
        if (projector.PositionNearlyLocked && projector.BodyWithCamera.Equals(Body) && projector.ViewMode != ViewMode.TopDown)
            return;

        float radiusInPixels;

        double subtendedAngle = Body.AngleSubtendedFromDistance(Body.DistanceFromCamera) / 2.0;

        radiusInPixels = (float)Math.Abs(projector.Zoom * subtendedAngle);

        if (Body.RenderPoint.X < -radiusInPixels || Body.RenderPoint.Y < -radiusInPixels || Body.RenderPoint.X > this.ScreenSize.Width + radiusInPixels || Body.RenderPoint.Y > this.ScreenSize.Height + radiusInPixels)
            return;

#if DEBUG
        if (Body.RenderPoint.IsZero)
            throw new Exception();
#endif

        var caption = CaptionMode switch
        {
            CaptionMode.Auto => projector.BodyBeingViewed.Equals(Body) || radiusInPixels > AUTO_CAPTION_THRESHOLD || Zoom > Body.CaptionZoomThreshold,
            CaptionMode.DynamicOnly => projector.BodyBeingViewed.Equals(Body) || (Body.BodyType == CelestialBodyType.Dynamic && (radiusInPixels > AUTO_CAPTION_THRESHOLD || Zoom > Body.CaptionZoomThreshold)),
            _ => false,
        };
        if (caption)
            DrawString(Body.DisplayName, new QPoint(Body.RenderPoint.X + radiusInPixels, Body.RenderPoint.Y + radiusInPixels), Body == projector.BodyBeingViewed ? QPen.White : Body.CaptionPen, this.SmallFont);

        if (this.WireFrameBodyRender &&
            radiusInPixels > 7 &&
            Body.HasShape)
        {
            var shape = (radiusInPixels > 80)
                            ? Body.ShapeBig
                            : (radiusInPixels > 30)
                                    ? Body.ShapeMedium
                                    : Body.ShapeSmall;

            shape.Reset();
            if (Body.HasDynamicShape)
                shape.RotateAbout(Body.Axis, Body.AngleSnapshot);
            shape.Move(Body.PositionSnapshot);

            QPoint p1, p2;

            double distSqr = projector.Position.DistanceToSquared(Body.PositionSnapshot);

            bool highlightSun = HighlightSunlitAreas && Body is Orbiter;

            double distSqrSun = highlightSun ? CelestialBody.Sun.PositionSnapshot.DistanceToSquared(Body.PositionSnapshot) : double.MaxValue;

            // Draw back of object
            foreach (var l in shape.Lines)
            {
                l.IsInFront = l.P1.DistanceToSquared(projector.Position) < distSqr || l.P2.DistanceToSquared(projector.Position) < distSqr;
                if (!l.IsInFront)
                    if (projector.Project2DLine(l.P1, l.P2, out p1, out p2))
                        DrawingTarget.DrawLine(l.LineType == LineType.Special ? primeMeridianDarkPen : Body.BackPen, p1, p2);
            }
            // Draw axis
            if (radiusInPixels > DRAW_AXIS_THRESHOLD)
            {
                if (projector.Project2DLine(Body.PositionSnapshot + Body.Axis * Body.Radius * AXIS_SCALE, Body.PositionSnapshot - Body.Axis * Body.Radius * AXIS_SCALE, out p1, out p2))
                    DrawingTarget.DrawLine(planetAxisPen, p1, p2);
            }
            //Draw front of object
            foreach (var l in shape.Lines)
            {
                if (l.IsInFront)
                    if (projector.Project2DLine(l.P1, l.P2, out p1, out p2))
                    {
                        if (highlightSun && l.LineType != LineType.Special && (l.P1.DistanceToSquared(CelestialBody.Sun.PositionSnapshot) < distSqrSun || l.P2.DistanceToSquared(CelestialBody.Sun.PositionSnapshot) < distSqrSun))
                            DrawingTarget.DrawLine(sunlitArea, p1, p2);
                        else
                            DrawingTarget.DrawLine(l.LineType == LineType.Special ? primeMeridianPen : Body.FrontPen, p1, p2);
                    }
            }
        }
        else
        {
            radiusInPixels += Body.RadiusEnhancement;
            if (radiusInPixels > 1f)
            {
               DrawingTarget.FillCircle(Body.Pen, Body.RenderPoint.X, Body.RenderPoint.Y, radiusInPixels);
            }
            else
            {
                DrawingTarget.SetPixel(Body.Pen, Body.RenderPoint.X, Body.RenderPoint.Y);
            }
        }
        RenderLocations[NumRenderLocations++] = Body;
    }
    public void DrawString(string Text, QPoint Location, QPen Pen, QFont Font)
    {
        DrawingTarget.DrawString(Text, Font.Font, Pen.Brush, Location.X, Location.Y);
    }
    public void DrawString(string Text, QRectangle Rectangle, QPen Pen, QFont Font)
    {
        System.Windows.Forms.TextRenderer.DrawText(DrawingTarget, Text, Font.Font, Rectangle.ToRectangle(), Pen.Color, System.Windows.Forms.TextFormatFlags.WordBreak);
    }
    public void DrawStringCentered(string Text, QPoint Location, QPen Pen, QFont Font)
    {
        var size = MeasureText(Text, Font);
        DrawingTarget.DrawString(Text, Font.Font, Pen.Brush, Location.X - size.Width / 2, Location.Y - size.Height / 2);
    }
    public QSize MeasureText(string Text, QFont Font)
    {
        var size = DrawingTarget.MeasureString(Text, Font.Font);
        return new QSize(size.Width, size.Height);
    }
    public QSize ScreenSize
    {
        get => screenSize;
        set
        {
            screenSize = value;
            projector.ScreenSize = value;
        }
    }
}
