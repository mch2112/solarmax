using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax
{
    internal abstract class Widget
    {
        protected float headingToUI(double Input)
        {
            return (float)((360.0 - Input.ToDegreesFromRadians()) % 360.0);
        }
        protected float inclinationToUI(double Input)
        {
            return (float)((Input.ToDegreesFromRadians()) % 360.0);
        }
        protected string rangeForUI(double Input)
        {
            return (Input > 0.5 * Util.METERS_PER_LIGHT_YEAR) ? string.Format("{0:0.0} LY", (Input / Util.METERS_PER_LIGHT_YEAR)) :
                   (Input > 1.5 * Util.METERS_PER_AU) ? string.Format("{0:0.0} AU", (Input / Util.METERS_PER_AU)) :
                   string.Format("{0:0,000} km", (Input / 1000));
        }
        public abstract void Render();
    }
}
