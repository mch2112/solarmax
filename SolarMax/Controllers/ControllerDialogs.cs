using SolarMax.Dialogs;
using System;

namespace SolarMax.Controllers;

internal sealed partial class Controller
{
    private bool InFindMode
    {
        get => currentDialog is FindDialog;
        set
        {
            if (InFindMode != value)
            {
                if (value)
                {
                    currentDialog?.Close();
                    currentDialog = new FindDialog(renderer,
                                                   physics,
                                                   this.ScreenSize,
                                                   FoundItemCallback,
                                                   CloseDialog,
                                                   defaultPen,
                                                   borderPen,
                                                   QPen.Black);
                }
                else
                {
                    CloseDialog();
                }
            }
        }
    }
    private bool InHelpMode
    {
        get => currentDialog is HelpDialog;
        set
        {
            if (InHelpMode != value)
            {
                if (value)
                {
                    currentDialog?.Close();
                    currentDialog = new HelpDialog(renderer,
                                                   this.ScreenSize,
                                                   CloseDialog,
                                                   defaultPen,
                                                   borderPen,
                                                   QPen.Black);
                }
                else
                {
                    CloseDialog();
                }
            }
        }
    }
    private bool InDateTimeAdjustMode
    {
        get => currentDialog is DateTimeDialog;
        set
        {
            if (InDateTimeAdjustMode != value)
            {
                if (value)
                {
                    currentDialog?.Close();

                    physics.Paused = true;
                    currentDialog = new DateTimeDialog(renderer,
                                                       CloseDialog,
                                                       DateChangeCallback,
                                                       physics.TargetDate,
                                                       DisplayTimeUTC,
                                                       this.screenSize,
                                                       defaultPen,
                                                       borderPen,
                                                       QPen.Black);
                }
                else
                {
                    CloseDialog();
                }
            }
        }
    }
    private bool InLatLongAdjustMode
    {
        get => currentDialog is LatLongDialog;
        set
        {
            if (InLatLongAdjustMode != value)
            {
                if (value)
                {
                    if (ViewMode != ViewMode.Surface)
                    {
                        messageWidget.SetMessage("Lat / Long Adjustments in Surface Mode Only [2]");
                    }
                    else
                    {
                        currentDialog?.Close();
                        currentDialog = new LatLongDialog(renderer,
                                                          CloseDialog,
                                                          LatLongChangeCallback,
                                                          camera.SurfaceLatitudeTarget,
                                                          camera.SurfaceLongitudeTarget,
                                                          this.screenSize,
                                                          defaultPen,
                                                          borderPen,
                                                          QPen.Black);
                    }
                }
                else
                {
                    CloseDialog();
                }
            }
        }
    }
    private void FoundItemCallback(CelestialBody Body, bool MoveTo)
    {
        if (Body is not null)
        {
            if (Body is Constellation && ConstellationMode == ConstellationMode.None)
            {
                ConstellationMode = ConstellationMode.LinesAndCaptions;
            }
            else if (Body is Star)
            {
                if (MinStarBrightness < (Body as Star).Magnitude)
                    MinStarBrightness = (float)((Body as Star).Magnitude + 1.0).Floor();
            }
            if (MoveTo)
                SetHome(Body);
            else
                SetViewTarget(Body);
        }
    }
    private void CloseDialog()
    {
        if (currentDialog.Message.Length > 0)
            messageWidget.SetMessage(currentDialog.Message);

        if (currentDialog is DateTimeDialog cd)
        {
            DateTime dt = cd.DateTime;

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
    private void LatLongChangeCallback(double Latitude, double Longitude)
    {
        camera.SurfaceLatitudeTarget = Latitude;
        camera.SurfaceLongitudeTarget = Longitude;
    }
    private void DateChangeCallback(DateTime DateTime)
    {
        physics.TargetAndPauseTime(DateTime);
    }
}
