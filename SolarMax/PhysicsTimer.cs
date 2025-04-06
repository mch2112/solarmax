using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax
{
    internal sealed partial class Physics
    {
        private const double TIME_SLICE_MIN = 0.1;
        private const double TIME_SLICE_IDEAL = 45;
        private const double TIME_SLICE_MAX = 180;
        private const double NEGATIVE_TIME_SLICE_IDEAL = -TIME_SLICE_IDEAL;
        private const double NEGATIVE_TIME_SLICE_MAX = -TIME_SLICE_MAX;

        private Clock clock;
        private TimeMode timeMode;
        private double totalElapsedTime = 0; 
        private double timeFactor = 1.0;
        private bool reverseTime = false;
        
        public bool Paused
        {
            get
            {
                return this.timeMode == TimeMode.Paused;
            }
            set
            {
                if (value)
                {
                    this.TimeMode = TimeMode.Paused;
                    this.TargetDate = this.Date;
                }
                else
                {
                    if (this.Paused && this.TimeFactor == 1 && this.Date.SecondsApart(this.TargetDate) < 60 && this.Date.SecondsApart(DateTime.UtcNow) < 60)
                        this.TimeMode = SolarMax.TimeMode.RealTime;
                    else
                        this.TimeMode = TimeMode.Normal;
                }
            }
        }
        public bool SpeedUp(AdjustmentAmount AdjAmount)
        {
            return changeSpeed(TimeFactor * getSpeedFactor(AdjAmount));
        }
        public bool SlowDown(AdjustmentAmount AdjAmount)
        {
            return changeSpeed(TimeFactor / getSpeedFactor(AdjAmount));
        }
        
        public bool ReverseTime
        {
            get { return reverseTime; }
            set
            {
                if (reverseTime != value)
                {
                    syncTargetDate();
                    reverseTime = value;
                    this.TimeMode = TimeMode.Normal;
                }
            }
        }
        public DateTime TargetDate
        {
            get
            {
                return BaselineDate.AddSeconds(clock.Seconds * TimeFactor * (ReverseTime ? -1.0 : 1.0));
            }
            private set
            {
                BaselineDate = value;
                clock.Reset();
                double timeDiff;

                if ((this.ephemeris != null) && (timeDiff = this.Date.SecondsApart(value)) > (10.0 * MathEx.SECONDS_PER_MINUTE))
                {
                    Ephemeris prospectiveEphemeris = this.availableEphemeres.GetClosest(value);
                    if (!prospectiveEphemeris.Equals(this.ephemeris))
                    {
                        var currentEphemerisDiff = this.ephemeris.Date.SecondsApart(value);
                        var prospectiveEphemerisDiff = prospectiveEphemeris.Date.SecondsApart(value);

                        if (prospectiveEphemerisDiff < currentEphemerisDiff * 0.8)
                        {
                            this.ephemeris = prospectiveEphemeris;
                            invokeEphemeris(SetTimeMode: false, Wait: true, EstablishGravitationalInfluences: false);
                        }
                    }
                    else if (timeDiff > 2.0 * MathEx.SECONDS_PER_HOUR)
                    {
                        wait();
                        establishGravitationalInfluences();
                        NoWait();
                    }
                }
            }
        }
        public double TimeFactor
        {
            get { return timeFactor; }
            set
            {
                timeFactor = value.Clamp(1, (1024 * 1024 * 4));
            }
        }

        public void GoToToday()
        {
            this.TimeMode = TimeMode.TargetToRealTime;
            this.reverseTime = false;
            this.TargetDate = DateTime.UtcNow;
        }
        public void TargetAndPauseTime(DateTime TargetDate)
        {
            this.TargetDate = TargetDate;
            this.TimeMode = TimeMode.TargetToPaused;
        }

        private TimeMode TimeMode
        {
            get { return timeMode; }
            set
            {
                if (this.timeMode != value)
                {
                    this.timeMode = value;
                    switch (value)
                    {
                        case SolarMax.TimeMode.RealTime:
                            this.TimeFactor = 1;
                            this.clock.Paused = false;
                            break;
                        case SolarMax.TimeMode.TargetToNormal:
                        case SolarMax.TimeMode.TargetToPaused:
                        case SolarMax.TimeMode.TargetToRealTime:
                        case SolarMax.TimeMode.Paused:
                            this.clock.Paused = true;
                            break;
                        default:
                            this.clock.Paused = false;
                            break;
                    }
                }
            }
        }

        private double getSpeedFactor(AdjustmentAmount AdjAmount)
        {
            switch (AdjAmount)
            {
                case AdjustmentAmount.All:
                    return double.MaxValue;
                case AdjustmentAmount.Large:
                    return 32.0;
                case AdjustmentAmount.Normal:
                    return 2.0;
                case AdjustmentAmount.Fine:
                default:
                    return 1.25;
            }
        }
        private void syncTargetDate()
        {
            TargetDate = ExternalDate;
        }
        private bool changeSpeed(double ToAmount)
        {
            switch (this.TimeMode)
            {
                case SolarMax.TimeMode.Normal:
                case SolarMax.TimeMode.RealTime:
                    var tf = TimeFactor;
                    syncTargetDate();
                    TimeFactor = ToAmount;
                    this.TimeMode = SolarMax.TimeMode.Normal;
                    return TimeFactor != tf;
                default:
                    this.TimeFactor = 1.0;
                    this.TimeMode = SolarMax.TimeMode.Normal;
                    return true;
            }
        }
    }
}
