using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax
{
    internal sealed partial class Controller
    {
        private bool UserPaused
        {
            get { return physics.Paused; }
            set
            {
                physics.Paused = value;
                if (value)
                    messageWidget.SetMessage("Paused");
                else
                    messageWidget.SetMessage(string.Format("Unpaused: {0:0}x", physics.TimeFactor));
            }
        }
        private void speedUp(AdjustmentAmount AdjAmount)
        {
            if (physics.StartupDone)
            {
                if (physics.SpeedUp(AdjAmount))
                    messageWidget.SetMessage(String.Format("Speed Up Time: {0:0}x", physics.TimeFactor));
                else
                    messageWidget.SetMessage("Maximum Speed");
            }
        }
        private void slowDown(AdjustmentAmount AdjAmount)
        {
            if (physics.StartupDone)
            {
                if (physics.SlowDown(AdjAmount))
                    messageWidget.SetMessage(String.Format("Slow Down Time: {0:0}x", physics.TimeFactor));
                else
                    messageWidget.SetMessage("Real Time");
            }
        }
        private void reverseTime()
        {
            if (physics.StartupDone)
            {
                physics.ReverseTime = !physics.ReverseTime;
                messageWidget.SetMessage("Time Running " + (physics.ReverseTime ? "Backwards" : "Forwards"));
            }
        }
        private void goToToday()
        {
            if (physics.StartupDone)
            {
                messageWidget.SetMessage("Go to Today");
                if (InDateTimeAdjustMode)
                    (currentDialog as DialogDateTime).DateTime = DateTime.UtcNow;
                else
                    physics.GoToToday();
            }
        }
       
    }
}
