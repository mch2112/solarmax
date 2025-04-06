using System;
using System.Collections.Generic;
using System.Threading;
using SolarMax.Integrators;

namespace SolarMax;

internal enum TimeMode { Paused, TargetToNormal, TargetToPaused, TargetToRealTime, Normal, RealTime }

internal sealed partial class Physics
{
    private enum WaitState { None, Requested, Confirmed }

    public delegate void StartupDoneDelegate();

    private WaitState waitState = WaitState.None;

    public const string CONSTANT_DATA_FILENAME = "constant_data.txt";
    public const string EPHEMERIS_SNAPSHOT_FILENAME = "ephemeris_snapshot.txt";
    public const string EPHEMERIS_FILENAME = "ephemeris.txt";
    public const double G = 6.67259E-14; // m^3 * s^-2 * g^-1
    
    private const double GRAVITATIONAL_INFLUENCE_THRESHOLD = 5E-9;

    public const string DATE_FORMAT_1 = "yyyy/MM/dd HH:mm:ss.fffffff";
    public const string DATE_FORMAT_2 = "yyyy/MM/dd HH:mm:ss";
    
    private const int MAX_SNAPSHOT_GENERATIONS = 30;
    private readonly DateTime EPOCH_START = new(/*EPOCH START: DO NOT CHANGE*/ 2011, 01, 01, 0, 0, 0);

    private double dt;
    
    public long Frames { get; private set; }

    public DateTime EphemerisDate { get { return this.ephemeris.Date; } }
    
    private DateTime Date { get; set; }
    public DateTime ExternalDate { get; private set; }
    private DateTime StartDate { get; set; }
    private DateTime BaselineDate { get; set; }

    public Tuple<Orbiter, Orbiter, double, int>[] GravitationalInfluences { get; private set; }

    public int SleepBetweenCycles { get; set; }

    public bool StartupDone { get; private set; }

    private bool cancel = false;
    
    private StartupDoneDelegate startupDoneDelegate;

    private Ephemeris ephemeris = null;
    private EphemerisCatalog availableEphemeres;
    private IIntegrator integrator;

    public Physics(IIntegrator Integrator, bool AllowLoadSnapshot, StartupDoneDelegate StartupDone)
    {
        this.clock = new Clock();

        this.integrator = Integrator;
        startupDoneDelegate = StartupDone;
        AllBodies = new LinkedList<CelestialBody>();
        
        BodyDictionary = new Dictionary<string, CelestialBody>(DEFAULT_STARTING_NUM_ALL_BODIES);
        StarDictionary = new Dictionary<int, Star>(DEFAULT_STARTING_NUM_STARS);
        
        SearchDatabase = null;
        Date = new DateTime(2011, 1, 1);
        SleepBetweenCycles = 0;

        createOrbiters();

        loadEphemeres(!TEST && AllowLoadSnapshot);

        if (TEST)
            this.TargetDate = TEST_DATE;
        else
            GoToToday();

        this.ephemeris = this.availableEphemeres.GetClosest(this.TargetDate);

        invokeEphemeris(SetTimeMode: true, Wait: false, EstablishGravitationalInfluences: true);

        loadStars();
        
        setupConstellations();

        setupCaptioning();
    }
    static Physics()
    {
        for (int i = 0; i < integrationFactors.Length; i++)
        {
            integrationFactors[i] = ((i) == 0xFF) ? 0x100 :
                                    ((i & 0x7F) == 0x7F) ? 0x80 :
                                    ((i & 0x3F) == 0x3F) ? 0x40 :
                                    ((i & 0x1F) == 0x1F) ? 0x20 :
                                    ((i & 0x0F) == 0x0F) ? 0x10 :
                                    ((i & 0x07) == 0x07) ? 0x08 :
                                    ((i & 0x03) == 0x03) ? 0x04 :
                                    ((i & 0x01) == 0x01) ? 0x02 :
                                    0x01;
        }
    }

    public void Go(ThreadPriority Priority)
    {
        Thread t = new(new ThreadStart(this.moveObjects))
        {
            Priority = Priority,
            Name = "Integrators"
        };
        t.Start();
    }
    public void Cancel()
    {
        this.cancel = true;
    }

    private void moveObjects()
    {
        clock.Reset();

        while (!cancel)
        {
            if (this.waitState == WaitState.Requested)
                this.waitState = WaitState.Confirmed;

            while ((this.Paused || this.waitState == WaitState.Confirmed) && !cancel)
            {
                System.Threading.Thread.Sleep(0);
            }

            if (timeMode == TimeMode.RealTime && clock.Seconds > 10)
                TargetDate = DateTime.UtcNow;

            double diff = (TargetDate - Date).TotalSeconds;

            if (diff > TIME_SLICE_MAX)
            {
                if (StartupDone)
                    dt = TIME_SLICE_MAX;
                else
                    dt = TIME_SLICE_IDEAL;
            }
            else if (diff < NEGATIVE_TIME_SLICE_MAX)
            {
                if (StartupDone)
                    dt = NEGATIVE_TIME_SLICE_MAX;
                else
                    dt = NEGATIVE_TIME_SLICE_IDEAL;
            }
            else if (diff > TIME_SLICE_MIN || diff < -TIME_SLICE_MIN)
            {
                dt = diff;
            }
            else
            {
                switch (this.timeMode)
                {
                    case TimeMode.TargetToNormal:
                        this.TimeMode = TimeMode.Normal;
                        break;
                    case TimeMode.TargetToPaused:
                        this.TimeMode = SolarMax.TimeMode.Paused;
                        break;
                    case TimeMode.TargetToRealTime:
                        this.TimeMode = TimeMode.RealTime;
                        break;
                }

                if (!StartupDone)
                {
                    dt = diff;
                    doMovement();
                    StartupDone = true;
                    Ephemeris e = new(this.AllOrbiters, this.Date, this.ephemeris.Date, this.ephemeris.VersionCount + 1);

                    if (TEST)
                    {
                        IO.WriteFile("test_report_" + testEphemeris.Date.ToString(Ephemeris.DATE_FORMAT_FOR_FILE_NAME) + ".txt", DirectoryLocation.Data, testReport(e, testEphemeris));
                    }
                    else if (!this.availableEphemeres.HasItemWithinSeconds(e.Date, MathEx.SECONDS_PER_DAY))
                    {
                        e.Save();
                        availableEphemeres.Add(e);
                        availableEphemeres.Init();
                    }
                    
                    startupDoneDelegate();
                    continue;
                }
                else
                {
                    System.Threading.Thread.Sleep(10);
                    ExternalDate = this.Date.AddSeconds(diff);
                    continue;
                }
            }

            doMovement();

            System.Threading.Thread.Sleep(SleepBetweenCycles);
        }
    }
    private void doMovement()
    {
        integrator.MoveOrbiters(dt);
        rotate();
        totalElapsedTime += dt;
        Date = StartDate.AddSeconds(totalElapsedTime); // less error than Date.AddSeconds(dt)?
        ExternalDate = Date;
        Frames++;
    }
    
    private void establishGravitationalInfluences()
    {
        List<Tuple<Orbiter, Orbiter, double>> list = new(Ephemeris.STARTING_NUM_ORBITERS * Ephemeris.STARTING_NUM_ORBITERS);
        
        foreach (var o in AllOrbiters)
            foreach (var oo in AllOrbiters)
                if (!o.Equals(oo))
                {
                    list.Add(new Tuple<Orbiter, Orbiter, double>(o,                                                 // body being influenced
                                                                 oo,                                                // influencing body
                                                                 oo.MG / o.Position.DistanceToSquared(oo.Position)) // acceleration scalar
                             );
                }

        list.Sort((b, a) => a.Item3.CompareTo(b.Item3));
        GravitationalInfluences = new Tuple<Orbiter, Orbiter, double, int>[list.Count];

        integrationFactorCount = new int[256];
        for (int i = 0; i < list.Count; i++)
        {
            int factor = ((int)(Math.Pow(2.0, -(int)(Math.Log(list[i].Item3)/6.0)))).Clamp(1, 256); // always an exponent of two

            GravitationalInfluences[i] = new Tuple<Orbiter, Orbiter, double, int>(list[i].Item1,
                                                                                  list[i].Item2,
                                                                                  list[i].Item3,
                                                                                  factor);

            ++integrationFactorCount[factor - 1];
        }

        integrationFactorCount[1] += integrationFactorCount[0];
        integrationFactorCount[3] += integrationFactorCount[1];
        integrationFactorCount[7] += integrationFactorCount[3];
        integrationFactorCount[15] += integrationFactorCount[7];
        integrationFactorCount[31] += integrationFactorCount[15];
        integrationFactorCount[63] += integrationFactorCount[31];
        integrationFactorCount[127] += integrationFactorCount[63];
        integrationFactorCount[255] += integrationFactorCount[127];
    }
    private static int[] integrationFactors = new int[256];
    private static int[] integrationFactorCount;
    private int stepCount = 0;
    
    public void UpdateAccelerationVariable()
    {
        unchecked
        {
            foreach (var o in AllOrbiters)
                o.Acceleration = new Vector();

            int Factor = integrationFactors[++stepCount % 256];
            int len = integrationFactorCount[Factor - 1];

            for (int i = 0; i < len; i++)
            {
                var gi = GravitationalInfluences[i];
                gi.Item1.Acceleration += gi.Item1.Position.DifferenceDirection(gi.Item2.Position) * (gi.Item4 * gi.Item2.MG / gi.Item1.Position.DistanceToSquared(gi.Item2.Position));
            }
        }
    }
    public void UpdateAcceleration()
    {
        foreach (var o in AllOrbiters)
            o.Acceleration = new Vector();

        foreach (var gi in GravitationalInfluences)
            gi.Item1.Acceleration += gi.Item1.Position.DifferenceDirection(gi.Item2.Position) * (gi.Item2.MG / gi.Item1.Position.DistanceToSquared(gi.Item2.Position));
    }
    
    private void rotate()
    {
        foreach (var t in this.AllOrbiters)
            t.Rotate(dt);
    }
}