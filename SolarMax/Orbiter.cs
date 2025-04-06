using System;
using System.Collections.Generic;

namespace SolarMax;

internal sealed class Orbiter : CelestialBody
{
    private const int MIN_INTENSITY_FOR_RANDOM_COLOR = 80;
    private const int MAX_INTENSITY_FOR_RANDOM_COLOR = 255;

    public double NPRightAscension { get; private set; }
    public double NPDeclination { get; private set; }
    public string Symbol { get; private set; }
    public double MG { get; private set; }

    private static readonly Dictionary<string, string> planetSymbols;

    public Vector Jerk { get; set; }
    public Vector Jerk2 { get; set; }

    public Vector Pos2 { get; set; }
    public Vector Pos3 { get; set; }
    public Vector Pos4 { get; set; }
    public Vector Vel2 { get; set; }
    public Vector Vel3 { get; set; }
    public Vector Vel4 { get; set; }
    public Vector Acc2 { get; set; }
    public Vector Acc3 { get; set; }
    public Vector Acc4 { get; set; }

    private readonly double startingAngle;

    private bool isDead = false;
    public override bool IsDead => this.isDead;

    public Orbiter(string Name, double MGInM3PerS2, double MassInGrams, double RadiusInMeters, double AngularVelocity, double NPRightAscension, double NPDeclination, double StartingAngle, double Oblateness)
    {
        this.Name = Name;
        this.DisplayName = Name;
        this.IsNamed = Name.Length > 0;

        if (MGInM3PerS2 < MathEx.EPSILON)
        {
            this.Mass = MassInGrams;
            this.MG = this.Mass * Physics.G;
        }
        else
        {
            this.MG = MGInM3PerS2;
            this.Mass = this.MG / Physics.G;
        }
        
        this.Radius = RadiusInMeters;
        this.AngularVelocity = AngularVelocity;
        this.NPRightAscension = NPRightAscension;
        this.NPDeclination = NPDeclination;
        this.BodyType = CelestialBodyType.Dynamic;
        this.RadiusEnhancement = (float)(Math.Log(this.Radius) / 12);

        switch (this.Name)
        {
            case "Earth":
                CelestialBody.Earth = this;
                break;
            case "Sun":
                CelestialBody.Sun = this;
                break;
            case "Moon":
                CelestialBody.Moon = this;
                break;
            case "Saturn":
                this.NumRings = 4;
                break;
            case "Uranus":
                this.NumRings = 1;
                break;
        }
        
        setupSymbolAndDisplayName();

        if (NPRightAscension != 0.0)
        {
            this.Axis = UnitWithDeclAndAsc(this.NPDeclination, this.NPRightAscension);
            
            this.Axis = ConvertEquatorialToEcliptic(this.Axis);

            double distFromPtOfAriesToQ = MathEx.HALF_PI + this.NPRightAscension;
            
            this.Angle = this.startingAngle = (Util.FIRST_PT_OF_ARIES_DEG.ToRadiansFromDegrees() + distFromPtOfAriesToQ + StartingAngle);
            
        }
        else
        {
            this.Axis = new Vector(0, 0, 1);
            this.Angle = this.startingAngle = StartingAngle;
        }

        this.Acceleration = new Vector();

        this.Color = Colors.GetColor(this.Name, MIN_INTENSITY_FOR_RANDOM_COLOR, MAX_INTENSITY_FOR_RANDOM_COLOR);

        if (this.Mass > 1E+9)
        {
            var s = Shape.GetSphere(32, 16, 8, 1.0, this.NumRings, Oblateness, true);
            this.ShapeSmall = adjustShape(s);
            s = Shape.GetSphere(48, 24, 12, 1.0, this.NumRings, Oblateness, true);
            this.ShapeMedium = adjustShape(s);
            s = Shape.GetSphere(64, 32, 16, 1.0, this.NumRings, Oblateness, true);
            this.ShapeBig = adjustShape(s);
        }
        else // artificial satellite
        {
            var s = Shape.GetCube(1);
            adjustShape(s);
            this.ShapeSmall = this.ShapeMedium = this.ShapeBig = s.ToResettable();
        }
        this.HasShape = true;
        this.HasDynamicShape = true;

        this.CaptionZoomThreshold = 0;

        this.SortKey = this.Name;
    }
    static Orbiter()
    {
        planetSymbols = new Dictionary<string, string>
        {
            { "Earth", "\u2295" },
            { "Sun", "\u2299" },
            { "Moon", "\u263E" },
            { "Mercury", "\u263f" },
            { "Venus", "\u2640" },
            { "Mars", "\u2642" },
            { "Jupiter", "\u2643" },
            { "Saturn", "\u2644" },
            { "Uranus", "\u2645" },
            { "Neptune", "\u2646" },
            { "Pluto", "\u2647" }
        };
    }
    public void Kill()
    {
        this.isDead = true;
        this.Velocity = new Vector(1E+4, 1E+3, 1E+2);
        if (this.Position.IsZero)
            this.Position = new Vector(1E+8, 1E+8, 0);
    }
    public void Reify()
    {
        this.isDead = false;
    }
    private void setupSymbolAndDisplayName()
    {
        this.Symbol = string.Empty;

        bool hasSymbol = planetSymbols.TryGetValue(this.Name, out string symbol);

        if (hasSymbol && (Preferences.PlanetLabelType == PlanetLabelType.Both || Preferences.PlanetLabelType == PlanetLabelType.SymbolOnly))
        {
            this.Symbol = symbol;
            this.FullName = /*this.Symbol + */ this.Name;

            switch (Preferences.PlanetLabelType)
            {
                case PlanetLabelType.SymbolOnly:
                    this.DisplayName = this.Symbol;
                    break;
                case PlanetLabelType.Both:
                    this.DisplayName = this.Symbol + this.Name;
                    break;
            }
        }
        else
        {
            this.Symbol = string.Empty;
            this.DisplayName = this.Name;
            this.FullName = this.Name;
        }
    }
    public override IEnumerable<string> SearchNames
    {
        get
        {
            yield return this.Name;
            if (this.FullName != this.Name)
                yield return FullName;
        }
    }
    private static Vector ConvertEquatorialToEcliptic(Vector Input)
        => Input.GetRotationAboutXAxis(-Util.ECLIPTIC_OBLIQUITY_DEGREES.ToRadiansFromDegrees())
                .GetRotationAboutZAxis(-Util.ECLIPTIC_EQUINOX_DEGREES.ToRadiansFromDegrees());
    private static Vector UnitWithDeclAndAsc(double Declination, double Ascension)
        => Vector.UnitX
                 .GetRotationAboutYAxis(-Declination)
                 .GetRotationAboutZAxis(Ascension);
    private ResettableShape adjustShape(Shape S)
    {
        var cross = Vector.UnitZ ^ this.Axis;
        if (cross.Magnitude > MathEx.EPSILON)
            S.RotateAbout(cross, MathEx.HALF_PI - this.Axis.Inclination);
        
        S.Inflate(this.Radius);

        return S.ToResettable();
    }
    public void Rotate(double Seconds)
        => this.Angle = (this.Angle + this.AngularVelocity * Seconds).NormalizeAngleZeroToTwoPi();
    public void RotateTo(double Seconds)
        => this.Angle = this.startingAngle + (this.AngularVelocity * Seconds).NormalizeAngleZeroToTwoPi();
    public Vector Momentum => this.Velocity * this.Mass;
    public double KineticEnergy => 0.5 * this.Mass * this.Velocity.MagnitudeSquared;
    public override string ToString() => this.Name;
}
