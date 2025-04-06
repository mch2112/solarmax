using System;

namespace SolarMax;
public enum ProjectionMode { Cylindrical, Stereographic, Orthographic }

internal class Projector
{
    private const double TILT_INCREMENT = 5.0 * MathEx.RAD_PER_DEG; // five degrees
    private const double PAN_INCREMENT = 150.0;
    
    private delegate void projectDelegate(ref Vector V, ref QPoint P);

    private const double FRONT_CLIP_PLANE = 1E+5;

    public Vector Position { get; private set; }
    public bool PositionNearlyLocked { get; private set; }
    public double Zoom { get; private set; }
    public ViewMode ViewMode { get; private set; }
    public CelestialBody BodyWithCamera { get; private set; }
    public CelestialBody BodyBeingViewed { get; private set; }

    private QSize screenSize = QSize.Empty;
    private QSize halfScreenSize;
    private QPoint minPoint;
    private QPoint maxPoint;
    
    private projectDelegate project;

    private readonly DampenerQuaternion pan; 
    private Quaternion projectionQ;
    private double panAzimuth = 0;
    private double panInclination = 0;
    private double panRotate = 0;

    public Projector(ProjectionMode ProjectionMode)
    {
        this.ProjectionMode = ProjectionMode;
        pan = new DampenerQuaternion(Quaternion.Identity, 0.1)
        {
            AutoLockThreshold = 0.01,
            NearLockThreshold = 0.02,
            AutoLock = true
        };
    }
    
    public ProjectionMode ProjectionMode { get; set; }
    public QSize ScreenSize
    {
        get { return this.screenSize; }
        set 
        {
            if (this.screenSize != value)
                this.screenSize = value;
        }
    }
    public Vector PanView { get; private set; }
    public Vector PanUp { get; private set; }
    public void SetupForProjection(Camera Camera)
    {
        Position = Camera.Position;
        PositionNearlyLocked = Camera.PositionNearlyLocked;
        ViewMode = Camera.ViewMode;
        var camView = Camera.View;
        var camUp = Camera.Up;
        BodyWithCamera = Camera.BodyWithCamera;
        BodyBeingViewed = Camera.BodyBeingViewed;

        halfScreenSize = new QSize(ScreenSize.Width / 2, ScreenSize.Height / 2);

        minPoint = new QPoint(0, 0);
        maxPoint = new QPoint(screenSize.Width, screenSize.Height);

        switch (ProjectionMode)
        {
            case ProjectionMode.Cylindrical:
                project = cylindricalProject;
                Zoom = (float)Camera.Zoom;
                break;
            case ProjectionMode.Stereographic:
                project = stereographicProject;
                Zoom = (float)Camera.Zoom * 0.8f;
                break;
            case ProjectionMode.Orthographic:
                project = orthographicProject;
                Zoom = (float)Camera.Zoom * 0.9f;
                break;
        }
        
        // Get Camera Rotation
        var camAz = Quaternion.GetRotationQuaternion(Vector.UnitZ, -camView.Azimuth);
        var camInc = Quaternion.GetRotationQuaternion(Vector.UnitY, camView.Inclination);

        var trans = camInc * camAz;

        Vector upTrans;

        if (camView.AngleDiffAbs(camUp) > MathEx.EPSILON)
            upTrans = trans.RotateVectorFast(camUp);
        else if (camView.AngleDiffAbs(Vector.UnitX) > MathEx.EPSILON)
            upTrans = trans.RotateVectorFast(Vector.UnitZ);
        else
            upTrans = trans.RotateVectorFast(Vector.UnitX);

        var camRot = Quaternion.GetRotationQuaternion(Vector.UnitX, Math.Atan2(upTrans.Y, upTrans.Z));

        var transWRotate = camRot * trans;
        
        // Get Panning Rotation
        var az = Quaternion.GetRotationQuaternion(Vector.UnitZ, panAzimuth);
        panAzimuth = 0;
        
        var inc = Quaternion.GetRotationQuaternion(Vector.UnitY, panInclination);
        panInclination = 0;
        
        var rot = Quaternion.GetRotationQuaternion(Vector.UnitX, panRotate);
        panRotate = 0;

        pan.SetTarget(az * inc * rot * pan.Target, false);
        pan.Track();

        // Combine
        projectionQ = pan.Actual * transWRotate;

        PanView = projectionQ.Conjugate.RotateVectorFast(Vector.UnitX);
        PanUp = projectionQ.Conjugate.RotateVectorFast(Vector.UnitZ);
    }
    private void cylindricalProject(ref Vector VectorIn3DSpace, ref QPoint P)
    {
        P.Overwrite(halfScreenSize.Width - VectorIn3DSpace.Azimuth * Zoom, halfScreenSize.Height - VectorIn3DSpace.Inclination * Zoom);
    }
    private void stereographicProject(ref Vector VectorIn3DSpace, ref QPoint P)
    {
        P.Overwrite(halfScreenSize.Width - Zoom * VectorIn3DSpace.Y / VectorIn3DSpace.X, halfScreenSize.Height - Zoom * VectorIn3DSpace.Z / VectorIn3DSpace.X);
    }
    private void orthographicProject(ref Vector VectorIn3DSpace, ref QPoint P)
    {
        double inc = VectorIn3DSpace.Inclination;
        P.Overwrite(halfScreenSize.Width - Zoom * Math.Cos(inc) * Math.Sin(VectorIn3DSpace.Azimuth), halfScreenSize.Height - Zoom * Math.Sin(inc));
    }
    public bool Project2DLine(Vector PointIn3DSpace1, Vector PointIn3DSpace2, out QPoint ProjectedPointIn2D1, out QPoint ProjectedPointIn2D2)
    {
        ProjectedPointIn2D1 = QPoint.Empty;
        ProjectedPointIn2D2 = QPoint.Empty;

        Vector v1 = orientToCamera(ref PointIn3DSpace1);
        Vector v2 = orientToCamera(ref PointIn3DSpace2);

        if (v1.X < FRONT_CLIP_PLANE)
        {
            if (v2.X < FRONT_CLIP_PLANE)
                return false;

            clipToFrontPlane(ref v1, ref v2);
        }
        else if (v2.X < FRONT_CLIP_PLANE)
        {
            if (v1.X < FRONT_CLIP_PLANE)
                return false;

            clipToFrontPlane(ref v2, ref v1);
        }

        project(ref v1, ref ProjectedPointIn2D1);
        project(ref v2, ref ProjectedPointIn2D2);

        return lineClip(ref ProjectedPointIn2D1, ref ProjectedPointIn2D2);
    }

    public void PanInclination(double Amount) => panInclination += Amount * PAN_INCREMENT;
    public void PanAzimuth(double Amount) => panAzimuth -= Amount * PAN_INCREMENT;
    public void PanRotate(double Amount) => panRotate += Amount * TILT_INCREMENT;
    public void ResetPanning() => pan.SetTarget(Quaternion.Identity, false);
    public void Settle() => pan.LockAndTrack();
    private Vector orientToCamera(ref Vector Target) => projectionQ.RotateVectorFast(Target - this.Position);

    private static void clipToFrontPlane(ref Vector ClipPoint, ref Vector OtherPoint)
    {
        double clipAmt = (FRONT_CLIP_PLANE - ClipPoint.X) / (OtherPoint.X - ClipPoint.X);
        ClipPoint.Y += (OtherPoint.Y - ClipPoint.Y) * clipAmt;
        ClipPoint.Z += (OtherPoint.Z - ClipPoint.Z) * clipAmt;
        ClipPoint.X = FRONT_CLIP_PLANE;
    }

    public bool Project2DPoint(Vector PointIn3DSpace, bool FailIfOffscreen, out QPoint ProjectedPointIn2D)
    {
        ProjectedPointIn2D = QPoint.Empty;

        Vector v = orientToCamera(ref PointIn3DSpace);

        if (v.X < FRONT_CLIP_PLANE)
            return false;

        project(ref v, ref ProjectedPointIn2D);

        if (!FailIfOffscreen || 
            (ProjectedPointIn2D.X >= minPoint.X &&
             ProjectedPointIn2D.X <= maxPoint.X &&
             ProjectedPointIn2D.Y >= minPoint.Y &&
             ProjectedPointIn2D.Y < maxPoint.Y))
        {
            return true;
        }

        return false;
    }
    private enum ClipCode : int { Inside = 0, Left = 1, Right = 2, Bottom = 4, Top = 8 }
    private ClipCode GetClipCode(QPoint P)
    {
        ClipCode code;

        code = ClipCode.Inside;

        if (P.X < minPoint.X)
            code |= ClipCode.Left;
        else if (P.X > maxPoint.X)
            code |= ClipCode.Right;

        if (P.Y < minPoint.Y)
            code |= ClipCode.Bottom;
        else if (P.Y > maxPoint.Y)
            code |= ClipCode.Top;

        return code;
    }
    // Cohen–Sutherland clipping algorithm
    private bool lineClip(ref QPoint P1, ref QPoint P2)
    {
        ClipCode cc1 = GetClipCode(P1);
        ClipCode cc2 = GetClipCode(P2);

        while (true)
        {
            if ((cc1 | cc2) == ClipCode.Inside)
            {
                return true;
            }
            else if ((cc1 & cc2) != ClipCode.Inside)
            {
                return false;
            }
            else
            {
                QPoint p;

                // failed both tests, so calculate the line segment to clip
                // from an outside point to an intersection with clip edge
                
                // At least one endpoint is outside the clip rectangle; pick it.
                ClipCode cc = (cc1 != ClipCode.Inside) ? cc1 : cc2;

                // Now find the intersection point;
                // use formulas y = y0 + slope * (x - x0), x = x0 + (1 / slope) * (y - y0)
                if ((cc & ClipCode.Top) != ClipCode.Inside)
                {
                    p = new QPoint(P1.X + (P2.X - P1.X) * (maxPoint.Y - P1.Y) / (P2.Y - P1.Y),
                                   maxPoint.Y);
                }
                else if ((cc & ClipCode.Bottom) != ClipCode.Inside)
                {
                    p = new QPoint(P1.X + (P2.X - P1.X) * (minPoint.Y - P1.Y) / (P2.Y - P1.Y),
                                   minPoint.Y);
                }
                else if ((cc & ClipCode.Right) != ClipCode.Inside)
                {
                    p = new QPoint(maxPoint.X,
                                   P1.Y + (P2.Y - P1.Y) * (maxPoint.X - P1.X) / (P2.X - P1.X));
                }
                else // point must be to left
                {
                    p = new QPoint(minPoint.X,
                                   P1.Y + (P2.Y - P1.Y) * (minPoint.X - P1.X) / (P2.X - P1.X));
                }

                if (cc.Equals(cc1))
                {
                    P1.Overwrite(p);
                    cc1 = GetClipCode(P1);
                }
                else
                {
                    P2.Overwrite(p);
                    cc2 = GetClipCode(P2);
                }
            }
        }
    }
}