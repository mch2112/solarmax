using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SolarMax
{
    internal enum ViewMode { Ecliptic, TopDown, Surface, Follow }
    
    internal sealed class Camera
    {
        private const double UP_TRACK_FRACTION = 0.2;
        private const double VIEW_TRACK_FRACTION = 0.03;
        private const double AXIS_TRACK_FRACTION = 0.1;
        private const double POSITION_TRACK_FRACTION = 0.05;
        private const double ZOOM_TRACK_FRACTION = 0.15;
        private const double LAT_LONG_TRACK_FRACTION = 0.2;
        private const double PAN_TRACK_FRACTION = 0.1;
        private const double USER_TRACK_FRACTION = 0.1;
        private readonly Vector TopDownHome = new Vector(0, 0, 3.0E+012);

        private ViewMode viewMode = ViewMode.Ecliptic;
        private Quaternion surfaceQ;
        private DampenerDirection view;
        private DampenerDirection up;
        private DampenerScalar zoom;
        private DampenerVector position;
        private DampenerAngle surfaceLongitude;
        private DampenerAngle surfaceLatitude;
        private Vector surfaceVector = Vector.UnitX;
        private Vector horizonNorthPole = Vector.UnitZ;
        private Vector horizonWest = Vector.UnitY;
        
        public bool PositionLocked { get { return position.Locked; } }
        public Vector Position
        {
            get
            {
                return ViewMode == SolarMax.ViewMode.Surface && PositionNearlyLocked ?
                       getSurfaceLocation() :
                       position.Actual;
            }
        }
        public Vector DesiredPosition
        {
            get
            {
                switch (ViewMode)
                {
                    case SolarMax.ViewMode.Ecliptic:
                        return BodyWithCamera.PositionSnapshot;
                    case SolarMax.ViewMode.TopDown:
                        return TopDownHome;
                    case SolarMax.ViewMode.Surface:
                        return getSurfaceLocation();
                    case SolarMax.ViewMode.Follow:
                        if (BodyBeingViewed.Velocity.IsZero)
                            return BodyBeingViewed.PositionSnapshot - (BodyWithCamera.PositionSnapshot - BodyBeingViewed.PositionSnapshot).Unit * 1E+10;
                        else
                            return BodyBeingViewed.PositionSnapshot - BodyBeingViewed.Velocity.Unit * (BodyBeingViewed.Radius * 14.0);
                    default:
                        throw new Exception();
                }
            }
        }
        public Vector SurfaceVector
        {
            get { return surfaceVector; }
        }
        public double SurfaceLatitudeActual
        {
            get { return surfaceLatitude.Actual; }
        }
        public double SurfaceLongitudeActual
        {
            get { return surfaceLongitude.Actual; }
        }
        public double SurfaceLongitudeTarget
        {
            get { return surfaceLongitude.Target; }
            set
            {
                surfaceLongitude.SetTarget(value, false);
            }
        }
        public double SurfaceLatitudeTarget
        {
            get { return surfaceLatitude.Target; }
            set
            {
                var lat = value.NormalizeAngleNegativePiToPi();
                if (lat > MathEx.HALF_PI)
                {
                    lat = Math.PI - lat;
                    SurfaceLongitudeTarget += Math.PI;
                }
                if (lat < -MathEx.HALF_PI)
                {
                    lat = -MathEx.HALF_PI + lat;
                    SurfaceLongitudeTarget += Math.PI;
                }
                surfaceLatitude.SetTarget(lat, false);
            }
        }
        public double Zoom
        {
            get { return zoom.Actual; }
        }
        public double ZoomTarget
        {
            get { return zoom.Target; }
            set
            {
                zoom.SetTarget(value, false);
                view.AutoLockThreshold = 0.8 / zoom.Target;
                view.NearLockThreshold = 30.0 / zoom.Target;
            }
        }
       
        public CelestialBody BodyBeingViewed { get; private set; }
        public CelestialBody BodyWithCamera { get; private set; }
        public bool PositionNearlyLocked { get { return position.NearLocked; } }
        public bool ViewNearlyLocked { get { return view.NearLocked; } }
        public ViewMode ViewMode
        {
            get { return viewMode; }
            set
            {
                if (viewMode != value)
                {
                    viewMode = value;

                    SetPosition(BodyWithCamera);

                    switch (ViewMode)
                    {
                        case SolarMax.ViewMode.TopDown:
                            up.SetTarget(Vector.UnitX, false);
                            break;
                        case SolarMax.ViewMode.Surface:
                            ensureViewAndTargetDifferent();
                            up.Unlock();
                            break;
                        case SolarMax.ViewMode.Ecliptic:
                            ensureViewAndTargetDifferent();
                            up.SetTarget(Vector.UnitZ, false);
                            break;
                        case SolarMax.ViewMode.Follow:
                            up.SetTarget(Vector.UnitZ, false);
                            break;
                    }
                }
            }
        }
        public Vector Up
        {
            get { return this.up.Actual; }
        }
        public Vector View
        {
            get { return this.view.Actual; }
        }
        
        public Camera(ViewMode ViewMode, CelestialBody PositionBody, CelestialBody ViewBody, double SurfaceLatitude, double SurfaceLongitude)
        {
            position = new DampenerVector(new Vector(), POSITION_TRACK_FRACTION);
            position.TrackFrameIncrease = 1.02;
            position.AutoLock = true;

            up = new DampenerDirection(Vector.UnitZ, UP_TRACK_FRACTION);
            up.AutoLock = true;
            up.AutoLockThreshold = 0.01;

            view = new DampenerDirection(Vector.UnitX, VIEW_TRACK_FRACTION);
            view.AutoLock = true;
            view.TrackFrameIncrease = 1.02;

            zoom = new DampenerScalar(1000, ZOOM_TRACK_FRACTION);
            zoom.TrackFrameIncrease = 1.03;

            surfaceLatitude = new DampenerAngle(0, LAT_LONG_TRACK_FRACTION);
            surfaceLatitude.AutoLockThreshold = MathEx.ONE_DEGREE_IN_RADIANS / MathEx.SECONDS_PER_DEGREE;
            surfaceLongitude = new DampenerAngle(0, LAT_LONG_TRACK_FRACTION);
            surfaceLongitude.AutoLockThreshold = MathEx.ONE_DEGREE_IN_RADIANS / MathEx.SECONDS_PER_DEGREE;

            this.BodyBeingViewed = ViewBody;
            
            this.SurfaceLatitudeTarget = SurfaceLatitude;
            this.SurfaceLongitudeTarget = SurfaceLongitude;

            this.SetPosition(PositionBody);
            this.ViewMode = ViewMode;

            this.position.Lock();

            position.OnUnlockDelegate += positionLockChanged;
            position.OnLockDelegate += positionLockChanged;
            position.OnBecomeNearlyLocked += positionLockChanged;
        }
        
        public void GetLocalCoordinates(Vector V, out double Azimuth, out double Inclination, out double Distance)
        {
#if DEBUG
            if (this.ViewMode != SolarMax.ViewMode.Surface)
                throw new Exception();
#endif
            var view = (V - this.position.Actual);

            Azimuth = Math.Atan2(view * horizonWest, view * horizonNorthPole);
            Inclination = MathEx.HALF_PI - view.AngleDiffAbs(surfaceVector); 
            Distance = view.Magnitude;

#if DEBUG
            var a = view
              .GetRotationAbout(Vector.UnitZ ^ this.BodyWithCamera.Axis, -this.BodyWithCamera.Axis.Tilt)
              .GetRotationAboutZAxis(-SurfaceLongitudeActual - BodyWithCamera.AngleSnapshot)
              .GetRotationAboutYAxis(-MathEx.HALF_PI + SurfaceLatitudeActual)
              .GetRotationAboutZAxis(Math.PI);

            if (!a.Inclination.IsCloseAbsolute(Inclination) || (!a.Azimuth.IsCloseAbsolute(Azimuth)))
                throw new Exception();
#endif
        }
 
        public void Track(bool Incremental)
        {
            if (this.ViewMode == SolarMax.ViewMode.Surface && (!this.surfaceLatitude.Locked || !this.surfaceLongitude.Locked))
                this.surfaceQ = null;

            if (Incremental)
            {
                this.surfaceLongitude.Track();
                this.surfaceLatitude.Track();

                this.position.SetTarget(DesiredPosition);
                this.position.Track();

                if (this.ViewMode == SolarMax.ViewMode.Surface)
                    updateSurfaceVector();
                
                this.view.SetTarget(DesiredTarget - this.Position);
                this.view.Track();

                this.up.Track();

                this.zoom.Track();
            }
            else
            {
                this.view.LockAndTrack();
                this.up.LockAndTrack();
                this.position.LockAndTrack();
                this.zoom.LockAndTrack();
                this.surfaceLongitude.LockAndTrack();
                this.surfaceLatitude.LockAndTrack();
            }
        }
        public void Swap()
        {
            var t = BodyBeingViewed;
            BodyBeingViewed = BodyWithCamera;
            SetPosition(t);
        }
        public void SetTarget(CelestialBody Target)
        {
            this.BodyBeingViewed = Target;

            if (this.ViewMode == SolarMax.ViewMode.Follow)
                this.position.Unlock();

            this.view.SetTarget(DesiredPosition - this.position.Actual, false);
        }
        public void SetPosition(CelestialBody Body)
        {
            BodyWithCamera = Body;

            position.Unlock();
            position.AutoLockThreshold = Body.Radius;
            position.NearLockThreshold = Body.Radius * 2.0;
            
            view.Unlock();
        }


        private void ensureViewAndTargetDifferent()
        {
            if (this.BodyWithCamera.Equals(this.BodyBeingViewed))
                if (this.BodyBeingViewed.IsSun)
                    this.BodyBeingViewed = CelestialBody.Earth;
                else
                    this.BodyBeingViewed = CelestialBody.Moon;
        }
        private void positionLockChanged()
        {
            view.Unlock();
            surfaceQ = null;
        }
        private void updateSurfaceQ()
        {
            var lat = Quaternion.GetRotationQuaternion(Vector.UnitY, -SurfaceLatitudeActual);
            var lng = Quaternion.GetRotationQuaternion(Vector.UnitZ, SurfaceLongitudeActual);

            var cross = Vector.UnitZ ^ BodyWithCamera.Axis;

            if (cross.Magnitude > MathEx.EPSILON)
            {
                Quaternion tilt = Quaternion.GetRotationQuaternion(Vector.UnitZ ^ BodyWithCamera.Axis, BodyWithCamera.Axis.Tilt);
                surfaceQ = tilt * lng * lat;
            }
            else
            {
                surfaceQ = lng * lat;
            }
        }
        private Vector DesiredTarget
        {
            get
            {
                switch (ViewMode)
                {
                    case SolarMax.ViewMode.Ecliptic:
                        return position.NearLocked ? BodyBeingViewed.PositionSnapshot : BodyWithCamera.PositionSnapshot;
                    case SolarMax.ViewMode.Surface:
                        return position.NearLocked ? BodyBeingViewed.PositionSnapshot : getSurfaceLocation();
                    case SolarMax.ViewMode.TopDown:
                        return BodyBeingViewed.PositionSnapshot;
                    case SolarMax.ViewMode.Follow:
                        return (BodyBeingViewed.PositionSnapshot - this.Position) * 1E+50;
                    default:
                        throw new Exception();
                }
            }
        }
        private Vector getSurfaceLocation()
        {
            return surfaceVector + BodyWithCamera.PositionSnapshot;
        }
        
        private void updateSurfaceVector()
        {
            if (surfaceQ == null)
                updateSurfaceQ();

            this.surfaceVector = surfaceQ.RotateVectorFast(new Vector(this.BodyWithCamera.Radius, 0, 0))
                                         .GetRotationAbout(BodyWithCamera.Axis, BodyWithCamera.AngleSnapshot);

            this.horizonWest = (this.surfaceVector ^ BodyWithCamera.Axis).Unit;
            this.horizonNorthPole = (horizonWest ^ surfaceVector).Unit;
#if DEBUG
            var np = this.horizonWest.GetRotationAbout(this.surfaceVector, -MathEx.HALF_PI).Unit;
            if (!np.IsSimilarTo(horizonNorthPole))
                throw new Exception();

            var w = this.horizonNorthPole.GetRotationAbout(this.surfaceVector, MathEx.HALF_PI);
            if (!w.IsSimilarTo(horizonWest))
                throw new Exception();
#endif
            this.up.SetTarget(this.position.Locked ? this.surfaceVector.Unit : Vector.UnitZ);
        }
    }
}
