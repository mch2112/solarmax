using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SolarMax
{
    internal static class GraphicsEx
    {
        public static void DrawLine(this Graphics G, QPen Pen, QPoint P1, QPoint P2)
        {
            G.DrawLine(Pen.Pen, P1.X, P1.Y, P2.X, P2.Y);
        }
        public static void DrawLine(this Graphics G, QPen Pen, float X1, float Y1, float X2, float Y2)
        {
            G.DrawLine(Pen.Pen, X1, Y1, X2, Y2);
        }
        public static void FillRectangle(this Graphics G, QPen Pen, QRectangle Rectangle)
        {
            G.FillRectangle(Pen.Brush, Rectangle.X, Rectangle.Y, Rectangle.Width, Rectangle.Height);
        }
        public static void FillRectangle(this Graphics G, QPen Pen, QPoint Location, QSize Size)
        {
            G.FillRectangle(Pen.Brush, Location.X, Location.Y, Size.Width, Size.Height);
        }
        public static void DrawRectangle(this Graphics G, QPen Pen, QRectangle Rectangle)
        {
            G.DrawRectangle(Pen.Pen, Rectangle.X, Rectangle.Y, Rectangle.Width, Rectangle.Height);
        }
        public static void DrawRectangle(this Graphics G, QPen Pen, QPoint Location, QSize Size)
        {
            G.DrawRectangle(Pen.Pen, Location.X, Location.Y, Size.Width, Size.Height);
        }
        public static void FillCircle(this Graphics G, QPen Pen, float X, float Y, float Radius)
        {
            G.FillEllipse(Pen.Brush, X - Radius, Y - Radius, Radius * 2, Radius * 2);
        }
        public static void DrawEllipse(this Graphics G, QPen Pen, QPoint Center, QSize Size)
        {
            G.DrawEllipse(Pen.Pen, Center.X - Size.Width / 2, Center.Y - Size.Height / 2, Size.Width, Size.Height);
        }
        public static void DrawCircle(this Graphics G, QPen Pen, QPoint Center, float Radius)
        {
            G.DrawEllipse(Pen.Pen, Center.X - Radius, Center.Y - Radius, Radius * 2, Radius * 2);
        }
        public static void SetPixel(this Graphics G, QPen Pen, float X, float Y)
        {
            G.DrawLine(Pen.Pen, X, Y, X + 1, Y);
        }
    }
}
