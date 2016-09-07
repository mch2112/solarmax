using System;
using System.Collections.Generic;
namespace SolarMax
{
    interface IRenderer
    {
#if WPF
        System.Windows.Media.Imaging.WriteableBitmap DrawingTarget { get; set; }
        List<Tuple<string, QPoint, QPen, QFont, bool>> PendingText { get; }
#else
        System.Drawing.Graphics DrawingTarget { get; set; }
#endif
        void DrawBodies(CaptionMode CaptionMode, double Zoom);

        void DrawArc(QRectangle Rectangle, double StartAngle, double SweepAngle, QPen Pen);
        void DrawArc(QRectangle Rectangle, float StartAngle, float SweepAngle, QPen Pen);
        void DrawString(string Text, QPoint Point, QPen Pen, QFont Font);
        void DrawString(string Text, QRectangle Rectangle, QPen Pen, QFont Font);
        void DrawStringCentered(string Text, QPoint Location, QPen Pen, QFont Font);

        void DrawLine(QPen Pen, QPoint P1, QPoint P2);
        void DrawLine(QPen Pen, float X1, float Y1, float X2, float Y2);

        void DrawRectangle(QPoint Location, QSize Size, QPen Pen);
        void DrawRectangle(QRectangle Rectangle, QPen Pen);

        void FillRectangle(QRectangle Rectangle, QPen FillPen, QPen BorderPen);
        void FillRectangle(QPoint Location, QSize Size, QPen FillPen, QPen BorderPen);
        void FillRectangle(QPoint Location, QSize Size, QPen Pen);

        void DrawEllipse(QPoint Center, QSize Radius, QPen Pen);
        void DrawCircle(QPoint Center, float Radius, QPen Pen);

        void RenderConstellation(Constellation Constellation, bool WithCaption);
        void RenderShape(Shape Shape, QPen Pen);


        QFont Font { get; set; }
        QFont SmallFont { get; set; }
        QFont SmallItalicFont { get; set; }
        QFont LargeFont { get; set; }
        QFont ExtraLargeFont { get; set; }
        QFont LargeItalicFont { get; set; }
        
        QSize MeasureText(string Text, QFont Font);
        
        QSize ScreenSize { get; set; }
        
        void SetupRenderPass();
        
        void StageBodyForRender(Orbiter Orbiter);
        void StageBodyForRender(Star Star);
        bool WireFrameBodyRender { get; set; }
        bool HighlightSunlitAreas { get; set; }

        List<CelestialBody> RenderLocations { get; }
        int NumRenderLocations { get; }
    }
}
