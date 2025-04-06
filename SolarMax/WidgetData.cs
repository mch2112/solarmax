using SolarMax.Controllers;

namespace SolarMax;

internal class WidgetData(Controller Controller, IRenderer Renderer, Physics Physics, Camera Camera, QPen TextPen) : Widget
{
    private readonly Controller controller = Controller;
    private readonly IRenderer renderer = Renderer;
    private readonly Physics physics = Physics;
    private readonly Camera camera = Camera;
    private readonly QPen textPen = TextPen;

    private QPoint line1 = QPoint.Empty;
    private QPoint line2 = new(0, 30);
    private QPoint line3 = new(0, 60);
    private QPoint line4 = new(0, 90);
    private QPoint line5 = new(0, 110);
    private QPoint line6 = new(0, 130);

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
                        positionText = $"Flying to the center of {posBody.Name} ({RangeForUI((camera.Position - camera.DesiredPosition).Magnitude)})";
                    break;
                case ViewMode.Surface:
                    if (camera.PositionNearlyLocked)
                        positionText = $"On the surface of {posBody.Name} {camera.SurfaceLatitudeActual.ToLatitudeString()} {camera.SurfaceLongitudeActual.ToLongitudeString()}";
                    else
                        positionText = $"Flying to the surface of {posBody.Name} ({RangeForUI((camera.Position - camera.DesiredPosition).Magnitude)})";
                    break;
                case ViewMode.TopDown:
                    if (camera.PositionNearlyLocked)
                        positionText = "Above the ecliptic";
                    else
                        positionText =  string.Format("Flying to position above the ecliptic ({0})", RangeForUI((camera.Position - camera.DesiredPosition).Magnitude));
                    break;
                case ViewMode.Follow:
                    if (camera.PositionNearlyLocked)
                        positionText = "Following";
                    else
                        positionText = $"Flying To {camera.BodyBeingViewed.Name} ({RangeForUI((camera.Position - camera.DesiredPosition).Magnitude)})";
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
                    case ViewMode.Surface:
                        {
                            camera.GetLocalCoordinates(camera.BodyBeingViewed.PositionSnapshot, out double az, out double inc, out double dist);
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
                                                                  RangeForUI(dist));
                                belowHorizonWarning = adjInc.IsNegative();
                            }
                            else
                            {
                                coordinatesText = string.Format("Az {0} Alt {1} Dist {2}",
                                                                  az.ToAzimuthString(),
                                                                  inc.ToInclinationString(),
                                                                  RangeForUI(dist));
                                belowHorizonWarning = inc.IsNegative();
                            }
                        }
                        break;
                    default:
                        {
                            var v = camera.BodyBeingViewed.PositionSnapshot - camera.Position;
                            coordinatesText = $"Bearing {HeadingToUI(v.Azimuth):000.0}° Inc {InclinationToUI(v.Inclination):00.0}° Dist {RangeForUI(v.Magnitude)}";
                        }
                        break;
                }
                viewText = controller.ViewMode switch
                {
                    ViewMode.Follow => "Following " + camera.BodyBeingViewed.FullName,
                    _ => $"Viewing {camera.BodyBeingViewed.FullName}{(belowHorizonWarning ? " (Below Horizon)" : "")}",
                };
                Util.EquatorialCoordsFromLocation(camera.BodyBeingViewed.PositionSnapshot - camera.BodyWithCamera.PositionSnapshot,
                                                  out double ra,
                                                  out double decl);
                coordinatesText2 = $"RA {ra.ToHourAngleString()}  DE {decl.ToInclinationString()}";
            }
            else
            {
                viewText = "Seeking " + camera.BodyBeingViewed.FullName + "...";
                coordinatesText = $"Bearing {HeadingToUI(camera.View.Azimuth):000.0}° Inc {InclinationToUI(camera.View.Inclination):00.0°} Zoom {camera.Zoom / controller.MinimumPixelsPerRadian:#}x";
            }
        }
        renderer.DrawString(viewText, line3, textPen, renderer.ExtraLargeFont);
        renderer.DrawString(viewBody.Description, line4, textPen, renderer.LargeFont);
        renderer.DrawString(coordinatesText, line5, textPen, renderer.LargeFont);
        renderer.DrawString(coordinatesText2, line6, textPen, renderer.LargeFont);
    }
}
