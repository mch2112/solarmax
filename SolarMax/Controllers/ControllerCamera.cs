using System;
using System.Collections.Generic;

namespace SolarMax.Controllers;

internal enum AdjustmentAmount { Fine, Normal, Large, All }

internal sealed partial class Controller
{
    private Camera camera;

    private void SwapViewAndHome()
    {
        if (this.ViewMode == ViewMode.Surface)
            this.ViewMode = ViewMode.Ecliptic;

        camera.ZoomTarget = MinimumPixelsPerRadian;
        camera.Swap();
        projector.ResetPanning();
        messageWidget.SetMessage("Switched View: From " + camera.BodyWithCamera.Name + " to " + camera.BodyBeingViewed.Name);
    }
    private double GetZoomAdjustmentAmount(AdjustmentAmount AdjAmount)
    {
        return AdjAmount switch
        {
            AdjustmentAmount.All or AdjustmentAmount.Large => 5,
            AdjustmentAmount.Normal => 1.15,
            _ => 1.04,
        };
    }
    private double GetPanAdjustmentAmount(AdjustmentAmount AdjAmount)
    {
        return AdjAmount switch
        {
            AdjustmentAmount.All or AdjustmentAmount.Large => 5,
            AdjustmentAmount.Normal => 1,
            _ => 0.25,
        };
    }
    private void PanLeft(AdjustmentAmount AdjAmount)
    {
        messageWidget.SetMessage("Pan Left");
        projector.PanAzimuth(GetPanAdjustmentAmount(AdjAmount) / camera.Zoom);
    }
    private void PanRight(AdjustmentAmount AdjAmount)
    {
        messageWidget.SetMessage("Pan Right");
        projector.PanAzimuth(-GetPanAdjustmentAmount(AdjAmount) / camera.Zoom);
    }
    private void PanHorizontal(double Offset)
    {
        projector.PanAzimuth(Offset * PAN_ADJUST / camera.Zoom);
    }
    private void PanVertical(double Offset)
    {
        projector.PanInclination(Offset * PAN_ADJUST / camera.Zoom);
    }
    private void PanUp(AdjustmentAmount AdjAmount)
    {
        messageWidget.SetMessage("Pan Up");
        projector.PanInclination(GetPanAdjustmentAmount(AdjAmount) / camera.Zoom);
    }
    private void PanDown(AdjustmentAmount AdjAmount)
    {
        messageWidget.SetMessage("Pan Down");
        projector.PanInclination(-GetPanAdjustmentAmount(AdjAmount) / camera.Zoom);
    }
    private void TiltLeft(AdjustmentAmount AdjAmount)
    {
        messageWidget.SetMessage("Tilt Left");
        projector.PanRotate(GetPanAdjustmentAmount(AdjAmount));
    }
    private void TiltRight(AdjustmentAmount AdjAmount)
    {
        messageWidget.SetMessage("Tilt Right");
        projector.PanRotate(-GetPanAdjustmentAmount(AdjAmount));
    }
    private void Tilt(double Offset)
    {
        projector.PanRotate(Offset * PAN_ADJUST);
    }
    public void ResetPanningAdjustments()
    {
        projector.ResetPanning();
        messageWidget.SetMessage("Reset Panning Adjustments");
    }
    private double FullZoomAmount(double Distance, CelestialBody Body)
        => Math.Max(MinimumPixelsPerRadian, 0.004 * Math.Log(Body.Radius) * (double)this.ScreenSize.Width / Body.AngleSubtendedFromDistance(Distance));
    private double ValidateZoom(double PixelsPerRadian) 
        => Math.Max(MinimumPixelsPerRadian, Math.Min(1E+14, PixelsPerRadian));
    public double MinimumPixelsPerRadian 
        => Math.Max(ScreenSize.Width, ScreenSize.Height) / Math.PI;
    private void Zoom(double offset)
    {
        camera.ZoomTarget = ValidateZoom(camera.ZoomTarget * (1.0 + offset * PAN_ADJUST));
        ShowZoomMessage();
    }
    private void ZoomIn(AdjustmentAmount AdjAmount)
    {
        if (AdjAmount == AdjustmentAmount.All)
        {
            projector.ResetPanning();
            camera.ZoomTarget = FullZoomAmount((camera.DesiredPosition - camera.BodyBeingViewed.Position).Magnitude, camera.BodyBeingViewed);
        }
        else
        {
            camera.ZoomTarget = ValidateZoom(camera.ZoomTarget * GetZoomAdjustmentAmount(AdjAmount));
        }
        ShowZoomMessage();
    }
    public void ZoomOut(AdjustmentAmount AdjAmount)
    {
        if (AdjAmount == AdjustmentAmount.All)
        {
            projector.ResetPanning();
            camera.ZoomTarget = this.MinimumPixelsPerRadian;
        }
        else
        {
            camera.ZoomTarget = ValidateZoom(camera.ZoomTarget / GetZoomAdjustmentAmount(AdjAmount));
        }
        ShowZoomMessage();
    }
    private void ShowZoomMessage()
    {
        double z = camera.ZoomTarget / this.MinimumPixelsPerRadian;

        if (z > 20)
            messageWidget.SetMessage(string.Format("Zoom {0:0,0}x", z));
        else
            messageWidget.SetMessage(string.Format("Zoom {0:0.0}x", z));
    }
    private void ZoomForBody()
    {
        camera.ZoomTarget = ValidateZoom(Math.Min(camera.Zoom, 400.0 / camera.BodyBeingViewed.AngleSubtendedFromDistance((camera.BodyBeingViewed.Position - camera.DesiredPosition).Magnitude)));
    }
    private AdjustmentAmount GetAdjustmentCode(QCommand Command)
    {
        if (Command.Control && Command.Shift)
            return AdjustmentAmount.All;
        else if (Command.Control)
            return AdjustmentAmount.Fine;
        else if (Command.Shift)
            return AdjustmentAmount.Large;
        else
            return AdjustmentAmount.Normal;
    }
    private void FindTarget()
    {
        if (camera.BodyWithCamera == CelestialBody.Moon)
            camera.SetPosition(CelestialBody.Earth);
        SetViewTarget(CelestialBody.Moon);
    }
    private void FindHome()
    {
        if (camera.BodyBeingViewed.IsEarth)
            SetViewTarget(CelestialBody.Moon);

        SetHome(CelestialBody.Earth);
    }
    private void SetViewTarget(CelestialBody CB)
    {
        if (camera.BodyBeingViewed != CB)
        {
            camera.SetTarget(CB);
            ZoomForBody();
        }
        projector.ResetPanning();
        messageWidget.SetMessage("Now Viewing " + camera.BodyBeingViewed.Name);
    }
    private void SetViewTarget(bool Forward)
    {
        CelestialBody body = camera.BodyBeingViewed;
        do
        {
            body = GetNextBody(body, Forward);
        }
        while (!IsValidViewTarget(body));

        SetViewTarget(body);
    }

    private bool CanViewStars => MinStarBrightness > MIN_STAR_BRIGHTNESS_THRESHOLD;

    private void SetHome(bool Forward)
    {
        if (this.ViewMode == ViewMode.TopDown || this.ViewMode == ViewMode.Follow)
        {
            SetViewTarget(Forward);
        }
        else
        {
            CelestialBody body = camera.BodyWithCamera;
            do
            {
                body = GetNextBody(body, Forward);
            }
            while (!IsValidHome(body));

            SetHome(body);
        }
    }
    private void SetHome(CelestialBody CB)
    {
        if (camera.BodyWithCamera != CB)
        {
            if (this.ViewMode == ViewMode.Surface)
                this.ViewMode = ViewMode.Ecliptic;

            camera.SetPosition(CB);
            camera.ZoomTarget = MinimumPixelsPerRadian;
        }
        projector.ResetPanning();
        messageWidget.SetMessage("Flying to " + camera.BodyWithCamera.Name);
    }
    private CelestialBody GetNextBody(CelestialBody Current, bool Forward)
    {
        LinkedListNode<CelestialBody> node;

        if (lastNode != null && lastNode.Value.Equals(Current))
            node = lastNode;
        else
            node = physics.AllBodies.Find(Current);

        node = Forward ?
               node.Next ?? physics.AllBodies.First :
               node.Previous ?? physics.AllBodies.Last;

        return (lastNode = node).Value;
    }
    private bool IsValidViewTarget(CelestialBody CB) => CanRender(CB);
    private bool IsValidHome(CelestialBody CB) => (CB is not Constellation) && CanRender(CB);
}
