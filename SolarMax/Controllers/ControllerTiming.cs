using SolarMax.Dialogs;
using System;

namespace SolarMax.Controllers;

internal sealed partial class Controller
{
    private bool UserPaused
    {
        get => physics.Paused;
        set
        {
            physics.Paused = value;
            if (value)
                messageWidget.SetMessage("Paused");
            else
                messageWidget.SetMessage($"Running: {physics.TimeFactor:0}x");
        }
    }
    private void SpeedUp(AdjustmentAmount AdjAmount)
    {
        if (physics.StartupDone)
        {
            if (physics.SpeedUp(AdjAmount))
                messageWidget.SetMessage($"Speed Up Time: {physics.TimeFactor:0}x");
            else
                messageWidget.SetMessage("Maximum Speed");
        }
    }
    private void SlowDown(AdjustmentAmount AdjAmount)
    {
        if (physics.StartupDone)
        {
            if (physics.SlowDown(AdjAmount))
                messageWidget.SetMessage($"Slow Down Time: {physics.TimeFactor:0}x");
            else
                messageWidget.SetMessage("Real Time");
        }
    }
    private void ReverseTime()
    {
        if (physics.StartupDone)
        {
            physics.ReverseTime = !physics.ReverseTime;
            messageWidget.SetMessage("Time Running " + (physics.ReverseTime ? "Backwards" : "Forwards"));
        }
    }
    private void GoToToday()
    {
        if (physics.StartupDone)
        {
            messageWidget.SetMessage("Go to Today");
            if (InDateTimeAdjustMode)
                (currentDialog as DateTimeDialog).DateTime = DateTime.UtcNow;
            else
                physics.GoToToday();
        }
    }
}
