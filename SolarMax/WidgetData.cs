using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax
{
    internal class WidgetData : Widget
    {
        private Controller controller;
        private IRenderer renderer;
        private Physics physics;
        private Camera camera;
        private QPen textPen;

        private QPoint line1 = QPoint.Empty;
        private QPoint line2 = new QPoint(0, 30);
        private QPoint line3 = new QPoint(0, 60);
        private QPoint line4 = new QPoint(0, 90);
        private QPoint line5 = new QPoint(0, 110);
        private QPoint line6 = new QPoint(0, 130);

        public WidgetData(Controller Controller, IRenderer Renderer, Physics Physics, Camera Camera, QPen TextPen)
        {
            this.controller = Controller;
            this.renderer = Renderer;
            this.physics = Physics;
            this.camera = Camera;
            this.textPen = TextPen;
        }
        public override void Render()
        {
            

            if (controller.DisplayTimeUTC)
            {
                renderer.DrawString(Util.GetDateString(physics.ExternalDate, true),
                                    line1,
                                    textPen,
                                    renderer.LargeFont);
            }
            else
            {
                renderer.DrawString(Util.GetDateString(physics.ExternalDate, false),
                                    line1,
                                    textPen,
                                    renderer.LargeFont);
            }

            var viewBody = (camera.PositionLocked || camera.ViewMode == ViewMode.TopDown) ? camera.BodyBeingViewed : camera.BodyWithCamera;
            var posBody = camera.BodyWithCamera;

            string positionText = string.Empty;

            if (camera.BodyWithCamera != null)
            {
                switch (camera.ViewMode)
                {
                    case ViewMode.Ecliptic:
                        if (camera.PositionNearlyLocked)
                            positionText = "Centered on " + posBody.Name;
                        else
                            positionText = string.Format("Flying to the center of {0} ({1})", posBody.Name, rangeForUI((camera.Position - camera.DesiredPosition).Magnitude));
                        break;
                    case ViewMode.Surface:
                        if (camera.PositionNearlyLocked)
                            positionText = string.Format("On the surface of {0} {1} {2}", posBody.Name, camera.SurfaceLatitudeActual.ToLatitudeString(), camera.SurfaceLongitudeActual.ToLongitudeString());
                        else
                            positionText = string.Format("Flying to the surface of {0} ({1})", posBody.Name, rangeForUI((camera.Position - camera.DesiredPosition).Magnitude));
                        break;
                    case SolarMax.ViewMode.TopDown:
                        if (camera.PositionNearlyLocked)
                            positionText = "Above the ecliptic";
                        else
                            positionText =  string.Format("Flying to position above the ecliptic ({0})", rangeForUI((camera.Position - camera.DesiredPosition).Magnitude));
                        break;
                    case SolarMax.ViewMode.Follow:
                        if (camera.PositionNearlyLocked)
                            positionText = "Following";
                        else
                            positionText = string.Format("Flying To {0} ({1})", camera.BodyBeingViewed.Name, rangeForUI((camera.Position - camera.DesiredPosition).Magnitude));
                        break;
                }
            }
            
            renderer.DrawString(positionText, line2, textPen, renderer.LargeFont);

            string viewText = string.Empty;
            string coordinatesText = string.Empty;
            string coordinatesText2 = string.Empty;
            if (camera.PositionNearlyLocked)
            {
                if (camera.ViewNearlyLocked)
                {
                    bool belowHorizonWarning = false;
                    switch (controller.ViewMode)
                    {
                        case SolarMax.ViewMode.Surface:
                            {
                                double az, inc, dist;
                                camera.GetLocalCoordinates(camera.BodyBeingViewed.PositionSnapshot, out az, out inc, out dist);
                                double adjInc;
                                if (camera.BodyWithCamera.IsEarth &&
                                    inc > -MathEx.TWO_DEGREES_IN_RADIANS &&
                                    inc < (MathEx.RAD_PER_DEG * 15.0) &&
                                    ((adjInc = inc.Refract()) > -MathEx.ONE_DEGREE_IN_RADIANS))
                                {
                                    coordinatesText = string.Format("Az {0} Alt {1} (App Alt {2}) Dist {3}",
                                                                      az.ToAzimuthString(),
                                                                      inc.ToInclinationString(),
                                                                      adjInc.ToInclinationString(),
                                                                      rangeForUI(dist));
                                    belowHorizonWarning = adjInc.IsNegative();
                                }
                                else
                                {
                                    coordinatesText = string.Format("Az {0} Alt {1} Dist {2}",
                                                                      az.ToAzimuthString(),
                                                                      inc.ToInclinationString(),
                                                                      rangeForUI(dist));
                                    belowHorizonWarning = inc.IsNegative();
                                }
                            }
                            break;
                        default:
                            {
                                var v = camera.BodyBeingViewed.PositionSnapshot - camera.Position;
                                coordinatesText = string.Format("Bearing {0:000.0}° Inc {1:00.0}° Dist {2}", headingToUI(v.Azimuth), inclinationToUI(v.Inclination), rangeForUI(v.Magnitude));
                            }
                            break;
                    }
                    switch (controller.ViewMode)
                    {
                        case SolarMax.ViewMode.Follow:
                            viewText = "Following " + camera.BodyBeingViewed.FullName;
                            break;
                        default:
                            viewText = "Viewing " + camera.BodyBeingViewed.FullName + (belowHorizonWarning ? " (Below Horizon)" : "");
                            break;
                    }

                    double ra;
                    double decl;
                    Util.EquatorialCoordsFromLocation(camera.BodyBeingViewed.PositionSnapshot - camera.BodyWithCamera.PositionSnapshot, out ra, out decl);
                    coordinatesText2 = string.Format("RA {0}  DE {1}",
                                                     ra.ToHourAngleString(),
                                                     decl.ToInclinationString());
                }
                else
                {
                    viewText = "Seeking " + camera.BodyBeingViewed.FullName + "...";
                    coordinatesText = string.Format("Bearing {0:000.0}° Inc {1:00.0°} Zoom {2:#}x",
                                                    headingToUI(camera.View.Azimuth),
                                                    inclinationToUI(camera.View.Inclination),
                                                    camera.Zoom / controller.MinimumPixelsPerRadian);
                }
            }
            renderer.DrawString(viewText, line3, textPen, renderer.ExtraLargeFont);
            renderer.DrawString(viewBody.Description, line4, textPen, renderer.LargeFont);
            renderer.DrawString(coordinatesText, line5, textPen, renderer.LargeFont);
            renderer.DrawString(coordinatesText2, line6, textPen, renderer.LargeFont);
        }
    }
}
