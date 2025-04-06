using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax
{
    internal abstract class Dampener<T>
    {
        public delegate void LockDelegate();

        protected T target;
        public T Actual { get; protected set; }
        private readonly double trackFactorBasis;
        private double trackFactor;
        private double trackFrameIncrease;
        
        private bool nearlyLocked;
        private bool locked;

        public LockDelegate OnLockDelegate = null;
        public LockDelegate OnUnlockDelegate = null;
        public LockDelegate OnBecomeNearlyLocked = null;

        public Dampener(T InitialValue, double TrackFactor)
        {
            this.Locked = true;
            this.NearLocked = true;
            this.trackFactorBasis = TrackFactor;
            this.AutoLockThreshold = 0;
            this.AutoLock = false;
            this.TrackFrameIncrease = 1;
            SetTarget(InitialValue, true);
        }
        public virtual void SetTarget(T Value)
        {
            target = Value;
            
            if (Locked)
                Actual = target;
        }
        public void SetTarget(T Value, bool Lock)
        {
            if (Lock)
                this.Lock();
            else
                this.Unlock();

            SetTarget(Value);
        }
        public virtual T Target
        {
            get { return target; }
        }
        public double TrackFrameIncrease
        {
            get { return trackFrameIncrease; }
            set
            {
#if DEBUG
                if (value < 1.0 || value > 1.5)
                    throw new Exception("Invalid Track Frame Increase Amount");
#endif
                trackFrameIncrease = value;
            }
        }
        public virtual double AutoLockThreshold { protected get; set; }
        public virtual double NearLockThreshold { protected get; set; }
        
        public bool AutoLock { get; set; }

        public virtual void Track()
        {
            if (Locked)
            {
                Actual = Target;
            }
            else
            {
                Track(trackFactor);
                trackFactor *= TrackFrameIncrease;
                
                if (trackFactor < 1.0)
                {
                    if (AutoLock)
                    {
                        var newDiff = differenceAbsoluteValue();

                        if (newDiff < AutoLockThreshold)
                            this.Lock();
                        else if (newDiff < NearLockThreshold)
                            this.NearLocked = true;
                    }
                }
                else
                {
                    this.Lock();
                }
            }
        }
        protected abstract void Track(double TrackFactor);
        
        public bool Locked
        {
            get { return locked; }
            set
            {
                if (locked != value)
                {
                    locked = value;
                    
                    if (locked && OnLockDelegate != null)
                        OnLockDelegate();
                    else if (!locked && OnUnlockDelegate != null)
                        OnUnlockDelegate();
                }
            }
        }
        public bool NearLocked
        {
            get { return nearlyLocked; }
            set
            {
                if (nearlyLocked != value)
                {
                    nearlyLocked = value;
                    if (nearlyLocked && OnBecomeNearlyLocked != null)
                        OnBecomeNearlyLocked();
                }
            }
        }
        
        public virtual void LockAndTrack()
        {
            this.Lock();
            this.Track();
        }
        public virtual void Lock()
        {
            bool wasLocked = this.Locked;
            this.Locked = true;
            this.NearLocked = true;
            if (!wasLocked && OnLockDelegate != null)
                OnLockDelegate();
        }
        public void Unlock()
        {
            bool wasLocked = this.Locked;

            this.Locked = false;
            this.NearLocked = false;
            this.trackFactor = this.trackFactorBasis;

            if (wasLocked && OnUnlockDelegate != null)
                OnUnlockDelegate();
        }
        protected abstract double differenceAbsoluteValue();

        public override string ToString()
        {
            return String.Format("Actual: {0} -> Target {1}", Actual.ToString(), Target.ToString());
        }
    }
}
