using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax
{
    internal enum CelestialBodyType { Dynamic, Star, Constellation }

    internal abstract class CelestialBody
    {
        public static Orbiter Earth { get; protected set; }
        public static Orbiter Sun { get; protected set; }
        public static Orbiter Moon { get; protected set; }
        
        public string Name { get; protected set; }
        public string FullName { get; protected set; }
        public string Description { get; protected set; }
        public string DisplayName { get; protected set; }
        public string SortKey { get; protected set; }

        public QPen CaptionPen { get; protected set; }
        public QPen Pen { get; protected set; }
        public QPen FrontPen { get; protected set; }
        public QPen BackPen { get; protected set; }
        public bool HasDynamicShape { get; protected set; }
        public bool HasShape { get; protected set; }
        public ResettableShape ShapeSmall { get; protected set; }
        public ResettableShape ShapeMedium { get; protected set; }
        public ResettableShape ShapeBig { get; protected set; }
        public Vector Position { get; set; }
        public Vector Velocity { get; set; }
        public Vector Acceleration { get; set; }
        public Vector Axis { get; protected set; }
        public double Mass { get; protected set; }
        public double Radius { get; protected set; }
        public double AngularVelocity { get; protected set; }
        public double Angle { get; protected set; }
        public CelestialBodyType BodyType { get; protected set; }
        public float RadiusEnhancement { get; protected set; }
        public bool IsNamed { get; protected set; }
        public double CaptionZoomThreshold { get; set; }
        
        public int NumRings { get; protected set; }
        public Vector PositionSnapshot { get; private set; }
        public double AngleSnapshot { get; private set; }
        
        public QPoint RenderPoint { get; set; }
        public double DistanceFromCamera { get; set; }

        private QColor color = QColor.Black;

        public bool IsEarth { get { return this.Equals(CelestialBody.Earth); } }
        public bool IsSun { get { return this.Equals(CelestialBody.Sun); } }
        public bool IsMoon { get { return this.Equals(CelestialBody.Moon); } }

        public abstract IEnumerable<string> SearchNames { get; }

        public void Snapshot()
        {
            this.PositionSnapshot = this.Position;
            this.AngleSnapshot = this.Angle;
        }
        public void SetEphemeris(Vector Location, Vector Velocity)
        {
            this.Position = Location * 1000;
            this.Velocity = Velocity * 1000;
            this.Snapshot();
        }
        public virtual bool IsDead { get { return false; } }
        public string SerializeEphermis()
        {
            var loc = (this.Position - CelestialBody.Sun.Position) / 1000;
            var vel = (this.Velocity - CelestialBody.Sun.Velocity) / 1000;

            return this.Name + "," + loc.Serialize() + "," + vel.Serialize();
        }
        protected virtual QColor Color
        {
            set
            {
                this.color = value;
                this.Pen = new QPen(value);
                this.CaptionPen = new QPen(value.Darken());
                this.FrontPen = new QPen(value.Brighten());
                this.BackPen = this.CaptionPen;
            }
            get { return color; }
        }
        public double AngleSubtendedFromDistance(double Distance)
        {
            return 2.0 * Math.Abs(Math.Atan2(this.Radius, Distance + this.Radius));
        }
    }
}
