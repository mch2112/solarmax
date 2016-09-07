using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SolarMax
{
    public static class BitmapEx
    {
        public static int[] clearArray;

        private static int Width { get; set; }
        private static int Height { get; set; }

        public static void SetSize(int Width, int Height)
        {
            BitmapEx.Width = Width;
            BitmapEx.Height = Height;

            const int val = 255 << 24;

            clearArray = new int[BitmapEx.Width * BitmapEx.Height];
            for (int i = 0; i < clearArray.Length; i++)
            {
                clearArray[i] = val;
            }
        }
        public static void Clear(this WriteableBitmap bmp)
        {
            bmp.WritePixels(new System.Windows.Int32Rect(0, 0, BitmapEx.Width, BitmapEx.Height), clearArray, BitmapEx.Width * 4, 0);
        }
        public static void SetPixel(this WriteableBitmap bmp, QPen Pen, float x, float y)
        {
            bmp.SetPixel(Pen, (int)x, (int)y);
        }
        public static void SetPixel(this WriteableBitmap bmp, QPen Pen, int x, int y)
        {
            unsafe
            {
                IntPtr buff = bmp.BackBuffer;
                int* pixels = (int*)buff.ToPointer();
                pixels[BitmapEx.Width * y + x] = Pen.BGRA;
            }
        }
        public static void DrawLine(this WriteableBitmap bmp, QPen Pen, QPoint P1, QPoint P2)
        {
            bmp.DrawLine(Pen, (int)P1.X, (int)P1.Y, (int)P2.X, (int)P2.Y);
        }
        public static void DrawLine(this WriteableBitmap bmp, QPen Pen, float x1, float y1, float x2, float y2)
        {
            bmp.DrawLine(Pen, (int)x1, (int)y1, (int)x2, (int)y2);
        }
        public static void DrawLine(this WriteableBitmap bmp, QPen Pen, int x1, int y1, int x2, int y2)
        {
            // Distance start and end point
            int dx = x2 - x1;
            int dy = y2 - y1;

            int len = BitmapEx.Width * BitmapEx.Height;

            const int PRECISION_SHIFT = 8;
            const int PRECISION_VALUE = 1 << PRECISION_SHIFT;

            unsafe
            {
                IntPtr buff = bmp.BackBuffer;
                int* pixels = (int*)buff.ToPointer();

                // Determine slope (absoulte value)
                int lenX, lenY;
                int incy1;
                if (dy >= 0)
                {
                    incy1 = PRECISION_VALUE;
                    lenY = dy;
                }
                else
                {
                    incy1 = -PRECISION_VALUE;
                    lenY = -dy;
                }

                int incx1;
                if (dx >= 0)
                {
                    incx1 = 1;
                    lenX = dx;
                }
                else
                {
                    incx1 = -1;
                    lenX = -dx;
                }

                if (lenX > lenY)
                { // x increases by +/- 1
                    // Init steps and start
                    int incy = (dy << PRECISION_SHIFT) / lenX;
                    int y = y1 << PRECISION_SHIFT;

                    for (int i = 0; i < lenX; i++)
                    {
                        // Check boundaries
                        y1 = y >> PRECISION_SHIFT;
                        if (x1 >= 0 && x1 < BitmapEx.Width && y1 >= 0 && y1 < BitmapEx.Height)
                        {
                            pixels[y1 * BitmapEx.Width + x1] = Pen.BGRA;
                        }
                        x1 += incx1;
                        y += incy;
                    }
                }
                else
                {
                    if (lenY == 0) // Prevent divison by zero
                        return;

                    // Init steps and start
                    // since y increases by +/-1, we can safely add (*h) before the for() loop, since there is no fractional value for y
                    int incx = (dx << PRECISION_SHIFT) / lenY;
                    int x = x1 << PRECISION_SHIFT;
                    int y = y1 << PRECISION_SHIFT;
                    int index = (x1 + y1 * BitmapEx.Width) << PRECISION_SHIFT;

                    var inc = incy1 * BitmapEx.Width + incx;
                    for (int i = 0; i < lenY; i++)
                    {
                        x1 = x >> PRECISION_SHIFT;
                        y1 = y >> PRECISION_SHIFT;
                        if (x1 >= 0 && x1 < BitmapEx.Width && y1 >= 0 && y1 < BitmapEx.Height)
                        {
                            pixels[index >> PRECISION_SHIFT] = Pen.BGRA;
                        }
                        x += incx;
                        y += incy1;
                        index += inc;
                    }
                }
            }
        }
        public static void FillCircle(this WriteableBitmap bmp, QPen Pen, float X, float Y, float Radius)
        {
            bmp.FillCircle(Pen, (int)X, (int)Y, (int)Radius);
        }
        public static void FillCircle(this WriteableBitmap bmp, QPen Pen, int X, int Y, int Radius)
        {
            unsafe
            {
                IntPtr buff = bmp.BackBuffer;
                int* pixels = (int*)buff.ToPointer();

                // Init vars
                int uh, lh, uy, ly, lx, rx;
                int x = Radius;
                int y = 0;
                int sq2 = (Radius * Radius) << 1;
                int xChg = Radius * Radius * (1 - (Radius << 1));
                int yChg = Radius * Radius;
                int err = 0;
                int xStopping = sq2 * Radius;
                int yStopping = 0;

                // Draw first set of points counter clockwise where tangent line slope > -1.
                while (xStopping >= yStopping)
                {
                    // Draw 4 quadrant points at once
                    uy = Y + y;                  // Upper half
                    ly = Y - y;                  // Lower half
                    if (uy < 0)
                        uy = 0;          // Clip
                    if (uy >= BitmapEx.Height)
                        uy = BitmapEx.Height - 1;
                    if (ly < 0)
                        ly = 0;
                    if (ly >= BitmapEx.Height)
                        ly = BitmapEx.Height - 1;
                    uh = uy * BitmapEx.Width;                  // Upper half
                    lh = ly * BitmapEx.Width;                  // Lower half

                    rx = X + x;
                    lx = X - x;
                    if (rx < 0)
                        rx = 0;          // Clip
                    if (rx >= BitmapEx.Width)
                        rx = BitmapEx.Width - 1;      // ...
                    if (lx < 0)
                        lx = 0;
                    if (lx >= BitmapEx.Width)
                        lx = BitmapEx.Width - 1;

                    // Draw line
                    for (int i = lx; i <= rx; i++)
                    {
                        pixels[i + uh] = Pen.BGRA;      // Quadrant II to I (Actually two octants)
                        pixels[i + lh] = Pen.BGRA;      // Quadrant III to IV
                    }

                    y++;
                    yStopping += sq2;
                    err += yChg;
                    yChg += sq2;
                    if ((xChg + (err << 1)) > 0)
                    {
                        x--;
                        xStopping -= sq2;
                        err += xChg;
                        xChg += sq2;
                    }
                }

                // ReInit vars
                x = 0;
                y = Radius;
                uy = Y + y;                  // Upper half
                ly = Y - y;                  // Lower half
                if (uy < 0) uy = 0;          // Clip
                if (uy >= BitmapEx.Height)
                    uy = BitmapEx.Height - 1;
                if (ly < 0) ly = 0;
                if (ly >= BitmapEx.Height)
                    ly = BitmapEx.Height - 1;
                uh = uy * BitmapEx.Width;                  // Upper half
                lh = ly * BitmapEx.Width;                  // Lower half
                xChg = Radius * Radius;
                yChg = Radius * Radius * (1 - (Radius << 1));
                err = 0;
                xStopping = 0;
                yStopping = sq2 * Radius;

                // Draw second set of points clockwise where tangent line slope < -1.
                while (xStopping <= yStopping)
                {
                    // Draw 4 quadrant points at once
                    rx = X + x;
                    lx = X - x;
                    if (rx < 0) rx = 0;          // Clip
                    if (rx >= BitmapEx.Width)
                        rx = BitmapEx.Width - 1;
                    if (lx < 0)
                        lx = 0;
                    if (lx >= BitmapEx.Width) lx = BitmapEx.Width - 1;

                    // Draw line
                    for (int i = lx; i <= rx; i++)
                    {
                        pixels[i + uh] = Pen.BGRA;      // Quadrant II to I (Actually two octants)
                        pixels[i + lh] = Pen.BGRA;      // Quadrant III to IV
                    }

                    x++;
                    xStopping += sq2;
                    err += xChg;
                    xChg += sq2;
                    if ((yChg + (err << 1)) > 0)
                    {
                        y--;
                        uy = Y + y;                  // Upper half
                        ly = Y - y;                  // Lower half
                        if (uy < 0)
                            uy = 0;          // Clip
                        if (uy >= BitmapEx.Height)
                            uy = BitmapEx.Height - 1;      // ...
                        if (ly < 0)
                            ly = 0;
                        if (ly >= BitmapEx.Height)
                            ly = BitmapEx.Height - 1;
                        uh = uy * BitmapEx.Width;                  // Upper half
                        lh = ly * BitmapEx.Width;                  // Lower half
                        yStopping -= sq2;
                        err += yChg;
                        yChg += sq2;
                    }
                }
            }
        }
        public static void DrawRectangle(this WriteableBitmap bmp, QPen Pen, QPoint Point, QSize Size)
        {
            bmp.DrawRectangle(Pen, (int)Point.X, (int)Point.Y, (int)(Point.X + Size.Width), (int)(Point.Y + Size.Height));
        }
        public static void DrawRectangle(this WriteableBitmap bmp, QPen Pen, int x1, int y1, int x2, int y2)
        {
            unsafe
            {
                IntPtr buff = bmp.BackBuffer;
                int* pixels = (int*)buff.ToPointer();

                // Check boundaries
                if (x1 < 0) { x1 = 0; }
                if (y1 < 0) { y1 = 0; }
                if (x2 < 0) { x2 = 0; }
                if (y2 < 0) { y2 = 0; }
                if (x1 >= Width) { x1 = Width - 1; }
                if (y1 >= Height) { y1 = Height - 1; }
                if (x2 >= Width) { x2 = Width - 1; }
                if (y2 >= Height) { y2 = Height - 1; }

                int startY = y1 * Width;
                int endY = y2 * Width;

                int offset2 = endY + x1;
                int endOffset = startY + x2;
                int startYPlusX1 = startY + x1;

                // top and bottom horizontal scanlines
                for (int x = startYPlusX1; x <= endOffset; x++)
                {
                    pixels[x] = Pen.BGRA; // top horizontal line
                    pixels[offset2] = Pen.BGRA; // bottom horizontal line
                    offset2++;
                }

                // offset2 == endY + x2

                // vertical scanlines
                endOffset = startYPlusX1 + Width;
                offset2 -= Width;

                for (int y = startY + x2 + Width; y < offset2; y += Width)
                {
                    pixels[y] = Pen.BGRA; // right vertical line
                    pixels[endOffset] = Pen.BGRA; // left vertical line

                    endOffset += Width;
                }
            }
        }
        public static void FillRectangle(this WriteableBitmap bmp, QPen Pen, QPoint Point, QSize Size)
        {
            bmp.FillRectangle(Pen, (int)Point.X, (int)Point.Y, (int)(Point.X + Size.Width), (int)(Point.Y + Size.Height));
        }
        public static void FillRectangle(this WriteableBitmap bmp, QPen Pen, int x1, int y1, int x2, int y2)
        {
            unsafe
            {
                IntPtr buff = bmp.BackBuffer;
                int* pixels = (int*)buff.ToPointer();

                // Check boundaries
                if (x1 < 0) { x1 = 0; }
                if (y1 < 0) { y1 = 0; }
                if (x2 < 0) { x2 = 0; }
                if (y2 < 0) { y2 = 0; }
                if (x1 >= Width) { x1 = Width - 1; }
                if (y1 >= Height) { y1 = Height - 1; }
                if (x2 >= Width) { x2 = Width - 1; }
                if (y2 >= Height) { y2 = Height - 1; }

                for (int y = y1; y <= y2; y++)
                {
                    var yy = y * Width;
                    for (int x = x1; x <= x2; x++)
                        pixels[yy + x] = Pen.BGRA;
                }
            }
        }
    }
}