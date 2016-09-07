using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax
{
    internal sealed partial class Controller
    {
        private bool InFindMode
        {
            get { return currentDialog is DialogFind; }
            set
            {
                if (InFindMode != value)
                {
                    if (value)
                    {
                        if (currentDialog != null)
                            currentDialog.Close();


                        currentDialog = new DialogFind(renderer, physics, this.ScreenSize, foundItemCallback, closeDialog, camera.BodyBeingViewed, defaultPen, borderPen, QPen.Black);
                    }
                    else
                    {
                        closeDialog();
                    }
                }
            }
        }
        private bool InHelpMode
        {
            get { return currentDialog is DialogHelp; }
            set
            {
                if (InHelpMode != value)
                {
                    if (value)
                    {
                        if (currentDialog != null)
                            currentDialog.Close();
                        currentDialog = new DialogHelp(renderer, this.ScreenSize, closeDialog, defaultPen, borderPen, QPen.Black);
                    }
                    else
                    {
                        closeDialog();
                    }
                }
            }
        }
        private bool InDateTimeAdjustMode
        {
            get { return currentDialog is DialogDateTime; }
            set
            {
                if (InDateTimeAdjustMode != value)
                {
                    if (value)
                    {
                        if (currentDialog != null)
                            currentDialog.Close();

                        physics.Paused = true;
                        currentDialog = new DialogDateTime(renderer, closeDialog, dateChangeCallback, physics.TargetDate, DisplayTimeUTC, this.screenSize, defaultPen, borderPen, QPen.Black);
                    }
                    else
                    {
                        closeDialog();
                    }
                }
            }
        }
        private bool InLatLongAdjustaMode
        {
            get { return currentDialog is DialogLatLong; }
            set
            {
                if (InLatLongAdjustaMode != value)
                {
                    if (value)
                    {
                        if (ViewMode != SolarMax.ViewMode.Surface)
                        {
                            messageWidget.SetMessage("Lat / Long Adjustments in Surface Mode Only [2]");
                        }
                        else
                        {
                            if (currentDialog != null)
                                currentDialog.Close();
                            currentDialog = new DialogLatLong(renderer, closeDialog, latLongChangeCallback, camera.SurfaceLatitudeTarget, camera.SurfaceLongitudeTarget, this.screenSize, defaultPen, borderPen, QPen.Black);
                        }
                    }
                    else
                    {
                        closeDialog();
                    }
                }
            }
        }
        private void foundItemCallback(CelestialBody Body, bool MoveTo)
        {
            if (Body != null)
            {
                if (Body is Constellation && ConstellationMode == SolarMax.ConstellationMode.None)
                    ConstellationMode = SolarMax.ConstellationMode.LinesAndCaptions;
                else if (Body is Star)
                {
                    if (MinStarBrightness < (Body as Star).Magnitude)
                        MinStarBrightness = (float)((Body as Star).Magnitude + 1.0).Floor();
                }
                if (MoveTo)
                    setHome(Body);
                else
                    setViewTarget(Body);
            }
        }
        private void closeDialog()
        {
            if (currentDialog.Message.Length > 0)
                messageWidget.SetMessage(currentDialog.Message);

            if (currentDialog is DialogDateTime)
            {
                DateTime dt = (currentDialog as DialogDateTime).DateTime;

                if (dt.SecondsApart(DateTime.UtcNow) < 60)
                {
                    physics.Paused = false;
                    physics.TimeFactor = 1;
                }
                else
                {
                    physics.TargetAndPauseTime(dt);
                }
            }

            currentDialog = null;
        }
        private void latLongChangeCallback(double Latitude, double Longitude)
        {
            camera.SurfaceLatitudeTarget = Latitude;
            camera.SurfaceLongitudeTarget = Longitude;
        }
        private void dateChangeCallback(DateTime DateTime)
        {
            physics.TargetAndPauseTime(DateTime);
        }
    }
}
