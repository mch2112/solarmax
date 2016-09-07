using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax
{
    internal enum AdjustmentAmount { Fine, Normal, Large, All }

    internal sealed partial class Controller
    {
        private Camera camera;

        private void swapViewAndHome()
        {
            if (this.ViewMode == SolarMax.ViewMode.Surface)
                this.ViewMode = SolarMax.ViewMode.Ecliptic;

            camera.ZoomTarget = MinimumPixelsPerRadian;
            camera.Swap();
            projector.ResetPanning();
            messageWidget.SetMessage("Switched View: From " + camera.BodyWithCamera.Name + " to " + camera.BodyBeingViewed.Name);
        }
        private double getZoomAdjustmentAmount(AdjustmentAmount AdjAmount)
        {
            switch (AdjAmount)
            {
                case AdjustmentAmount.All:
                case AdjustmentAmount.Large:
                    return 5;
                case AdjustmentAmount.Normal:
                    return 1.15;
                case AdjustmentAmount.Fine:
                default:
                    return 1.04;
            }
        }
        private double getPanAdjustmentAmount(AdjustmentAmount AdjAmount)
        {
            switch (AdjAmount)
            {
                case AdjustmentAmount.All:
                case AdjustmentAmount.Large:
                    return 5;
                case AdjustmentAmount.Normal:
                    return 1;
                case AdjustmentAmount.Fine:
                default:
                    return 0.25;
            }
        }
        private void panLeft(AdjustmentAmount AdjAmount)
        {
            messageWidget.SetMessage("Pan Left");
            projector.PanAzimuth(getPanAdjustmentAmount(AdjAmount) / camera.Zoom);
        }
        private void panRight(AdjustmentAmount AdjAmount)
        {
            messageWidget.SetMessage("Pan Right");
            projector.PanAzimuth(-getPanAdjustmentAmount(AdjAmount) / camera.Zoom);
        }
        private void panHorizontal(double Offset)
        {
            projector.PanAzimuth(Offset * PAN_ADJUST / camera.Zoom);
        }
        private void panVertical(double Offset)
        {
            projector.PanInclination(Offset * PAN_ADJUST / camera.Zoom);
        }
        private void panUp(AdjustmentAmount AdjAmount)
        {
            messageWidget.SetMessage("Pan Up");
            projector.PanInclination(getPanAdjustmentAmount(AdjAmount) / camera.Zoom);
        }
        private void panDown(AdjustmentAmount AdjAmount)
        {
            messageWidget.SetMessage("Pan Down");
            projector.PanInclination(-getPanAdjustmentAmount(AdjAmount) / camera.Zoom);
        }
        private void tiltLeft(AdjustmentAmount AdjAmount)
        {
            messageWidget.SetMessage("Tilt Left");
            projector.PanRotate(getPanAdjustmentAmount(AdjAmount));
        }
        private void tiltRight(AdjustmentAmount AdjAmount)
        {
            messageWidget.SetMessage("Tilt Right");
            projector.PanRotate(-getPanAdjustmentAmount(AdjAmount));
        }
        private void tilt(double Offset)
        {
            projector.PanRotate(Offset * PAN_ADJUST);
        }
        public void ResetPanningAdjustments()
        {
            projector.ResetPanning();
            messageWidget.SetMessage("Reset Panning Adjustments");
        }
        private double fullZoomAmount(double Distance, CelestialBody Body)
        {
            return Math.Max(MinimumPixelsPerRadian, 0.004 * Math.Log(Body.Radius) * (double)this.ScreenSize.Width / Body.AngleSubtendedFromDistance(Distance));
        }
        private double validateZoom(double PixelsPerRadian)
        {
            return Math.Max(MinimumPixelsPerRadian, Math.Min(1E+14, PixelsPerRadian));
        }
        public double MinimumPixelsPerRadian
        {
            get { return Math.Max(ScreenSize.Width, ScreenSize.Height) / Math.PI; } // 180 degrees
        }
        private void zoom(double offset)
        {
            camera.ZoomTarget = validateZoom(camera.ZoomTarget * (1.0 + offset * PAN_ADJUST));
            showZoomMessage();
        }
        private void zoomIn(AdjustmentAmount AdjAmount)
        {
            if (AdjAmount == AdjustmentAmount.All)
            {
                projector.ResetPanning();
                camera.ZoomTarget = fullZoomAmount((camera.DesiredPosition - camera.BodyBeingViewed.Position).Magnitude, camera.BodyBeingViewed);
            }
            else
            {
                camera.ZoomTarget = validateZoom(camera.ZoomTarget * getZoomAdjustmentAmount(AdjAmount));
            }
            showZoomMessage();
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
                camera.ZoomTarget = validateZoom(camera.ZoomTarget / getZoomAdjustmentAmount(AdjAmount));
            }
            showZoomMessage();
        }
        private void showZoomMessage()
        {
            double z = camera.ZoomTarget / this.MinimumPixelsPerRadian;

            if (z > 20)
                messageWidget.SetMessage(string.Format("Zoom {0:0,0}x", z));
            else
                messageWidget.SetMessage(string.Format("Zoom {0:0.0}x", z));
        }
        private void zoomForBody()
        {
            camera.ZoomTarget = validateZoom(Math.Min(camera.Zoom, 400.0 / camera.BodyBeingViewed.AngleSubtendedFromDistance((camera.BodyBeingViewed.Position - camera.DesiredPosition).Magnitude)));
        }
        private AdjustmentAmount getAdjustmentCode(QCommand Command)
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
        private void findTarget()
        {
            if (camera.BodyWithCamera == CelestialBody.Moon)
                camera.SetPosition(CelestialBody.Earth);
            setViewTarget(CelestialBody.Moon);
        }
        private void findHome()
        {
            if (camera.BodyBeingViewed.IsEarth)
                setViewTarget(CelestialBody.Moon);

            setHome(CelestialBody.Earth);
        }
        private void setViewTarget(CelestialBody CB)
        {
            if (camera.BodyBeingViewed != CB)
            {
                camera.SetTarget(CB);
                zoomForBody();
            }
            projector.ResetPanning();
            messageWidget.SetMessage("Now Viewing " + camera.BodyBeingViewed.Name);
        }
        private void setViewTarget(bool Forward)
        {
            CelestialBody body = camera.BodyBeingViewed;
            do
            {
                body = getNextBody(body, Forward);
            }
            while (!isValidViewTarget(body));

            setViewTarget(body);
        }

        private bool CanViewStars { get { return MinStarBrightness > MIN_STAR_BRIGHTNESS_THRESHOLD; } }

        private void setHome(bool Forward)
        {
            if (this.ViewMode == SolarMax.ViewMode.TopDown || this.ViewMode == SolarMax.ViewMode.Follow)
            {
                setViewTarget(Forward);
            }
            else
            {
                bool cantViewStars = !CanViewStars;
                CelestialBody body = camera.BodyWithCamera;
                do
                {
                    body = getNextBody(body, Forward);
                }
                while (!isValidHome(body));

                setHome(body);
            }
        }
        private void setHome(CelestialBody CB)
        {
            if (camera.BodyWithCamera != CB)
            {
                if (this.ViewMode == SolarMax.ViewMode.Surface)
                    this.ViewMode = SolarMax.ViewMode.Ecliptic;

                camera.SetPosition(CB);
                camera.ZoomTarget = MinimumPixelsPerRadian;
            }
            projector.ResetPanning();
            messageWidget.SetMessage("Flying to " + camera.BodyWithCamera.Name);
        }
        private CelestialBody getNextBody(CelestialBody Current, bool Forward)
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
        private bool isValidViewTarget(CelestialBody CB)
        {
            return canRender(CB);
        }
        private bool isValidHome(CelestialBody CB)
        {
            return (!(CB is Constellation)) && canRender(CB);
        }
    }
}
