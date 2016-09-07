using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax
{
    internal sealed partial class Controller
    {
        private IRenderer renderer;

        private QPen defaultPen;
        private QPen borderPen;
        private QPen equatorialCompassPen;
        private QPen eclipticCompassPen;
        private QPen equatorialGridPen;
        private QPen eclipticGridPen;
        private QPen localGridPen;
        private QPen localCompassPen;
        private QPen instrumentLinePen;
        private QPen instrumentOutlinePen;
        private QPen constellationBoundaryPen;
        private QPen instrumentDataPen;

        public void Render(System.Drawing.Graphics DrawingTarget)
        {
            if (camera != null)
            {
                this.drawingTarget = DrawingTarget;
                renderer.DrawingTarget = DrawingTarget;

                try
                {
                    // Frame counter
                    //renderer.DrawString(physics.Frames.ToString(), new QPoint(500, 0), QPen.White, renderer.Font);

                    // Ephemeris Date
                    //renderer.DrawStringCentered(physics.EphemerisDate.ToString(), new QPoint(this.ScreenSize.Width / 2, 10), QPen.White, renderer.Font);

                    if (HighQualityRender)
                        DrawingTarget.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                    foreach (var o in physics.AllOrbiters)
                        o.Snapshot();

                    camera.Track(true);

                    projector.SetupForProjection(camera);
                    renderer.SetupRenderPass();

                    switch (screenSaverMode)
                    {
                        case ScreenSaverMode.ScreenSaverPreview:
                            stagePlanetsForRender();
                            renderer.DrawBodies(this.CaptionMode, camera.Zoom);
                            break;
                        default:
                            if (physics.StartupDone)
                            {
                                if (ShowConstellationBoundaries)
                                    renderer.RenderShape(Constellation.ConstellationBoundaries, constellationBoundaryPen);

                                renderGrids();

                                if (this.showInstruments)
                                    renderInstruments();

                                switch (ConstellationMode)
                                {
                                    case SolarMax.ConstellationMode.Lines:
                                        renderConstellations(false);
                                        break;
                                    case SolarMax.ConstellationMode.LinesAndCaptions:
                                        renderConstellations(true);
                                        break;
                                }
                            }

                            stagePlanetsForRender();
                            if (this.physicsStartupDone && CanViewStars)
                            {
                                if (this.MinStarBrightness >= MAX_STAR_BRIGHTNESS_THRESHOLD)
                                    stageStarsForRender(float.MaxValue);
                                else
                                    stageStarsForRender(this.MinStarBrightness);
                            }
                            renderer.DrawBodies(this.CaptionMode, camera.Zoom);

                            if (this.ShowData)
                                dataWidget.Render();

                            messageWidget.Render();

                            if (currentDialog != null)
                                currentDialog.Render(ScreenSize);

                            if (ShowCredits)
                            {
                                if (creditsSize == QSize.Empty)
                                    creditsSize = renderer.MeasureText(credits, renderer.Font);

                                renderer.DrawString(credits, new QPoint(screenSize.Width - creditsSize.Width, screenSize.Height - creditsSize.Height), borderPen, renderer.Font);
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    messageWidget.SetMessage(ex.Message);
                }
            }
        }
        private void renderGrids()
        {
            if (this.ShowEquatorialGrid)
            {
                renderShape(this.equatorialGrid, equatorialGridPen, equatorialCompassPen);
            }
            if (this.ShowEclipticGrid)
            {
                renderShape(this.eclipticGrid, eclipticGridPen, eclipticCompassPen);
            }
            if (ShowLocalGrid && this.ViewMode == SolarMax.ViewMode.Surface && camera.PositionLocked)
            {
                var grid = camera.BodyWithCamera.IsEarth ? localGridWithRefraction : localGrid;
                grid.Reset();

                Quaternion tilt = Quaternion.GetRotationQuaternion(Vector.UnitZ ^ camera.BodyWithCamera.Axis,
                                                                   camera.BodyWithCamera.Axis.Tilt);
                Quaternion rot = Quaternion.GetRotationQuaternion(camera.BodyWithCamera.Axis,
                                                                   camera.BodyWithCamera.AngleSnapshot + camera.SurfaceLongitudeActual);
                Quaternion inc = Quaternion.GetRotationQuaternion(camera.BodyWithCamera.Axis ^ camera.SurfaceVector,
                                                                   MathEx.HALF_PI - camera.SurfaceLatitudeActual);

                Quaternion q = inc * rot * tilt;

                grid.Rotate(q);
                grid.Move(camera.Position);

                renderShape(grid, localGridPen, localCompassPen);
            }
        }
        private void stagePlanetsForRender()
        {
            foreach (Orbiter o in physics.AllOrbiters)
                renderer.StageBodyForRender(o);
        }
        private void stageStarsForRender(float MinMagnitude)
        {
#if DEBUG
            float mag = float.MinValue;
#endif
            if (physics.StartupDone)
                foreach (var star in physics.Stars)
                {
#if DEBUG
                    if (mag > star.Magnitude)
                        throw new Exception("Stars not ordered by decreasing brightness");
                    mag = star.Magnitude;
#endif
                    if (star.Magnitude > MinMagnitude)
                        break;

                    renderer.StageBodyForRender(star);
                }
        }
        private void renderShape(Shape Grid, QPen Pen, QPen SpecialPen)
        {
            QPoint p1, p2;
            foreach (var l in Grid.Lines)
                if (projector.Project2DLine(l.P1, l.P2, out p1, out p2))
                    drawingTarget.DrawLine(l.LineType == LineType.Special ? SpecialPen : Pen, p1, p2);
        }
        private void renderConstellations(bool WithCaption)
        {
            foreach (var c in physics.Constellations)
            {
                if (c.Shape.Lines.Count() == 0)
#if DEBUG
                    throw new Exception();
#else
                    return;
#endif
                renderer.RenderConstellation(c, WithCaption);
            }
        }
        private void renderInstruments()
        {
            instrumentDataSource.Update();

            azimuthInstrument.Render();
            inclinometerInstrument.Render();
            attitudeInstrument.Render();
            fieldOfViewInstrument.Render();
        }
    }
}
