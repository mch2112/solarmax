using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax;

internal class HorizonsDownload
{
    private static readonly List<Tuple<string, string, DateTime, DateTime>> orbiterCodes;

    private const string START_MARKER = "$$SOE";
    private const string END_MARKER = "$$EOE";

    static HorizonsDownload()
    {
        orbiterCodes = [];

        AddOrbiter("Sun", "10");
        AddOrbiter("Mercury", "199");
        AddOrbiter("Venus", "299");
        AddOrbiter("Earth", "399");
        AddOrbiter("Mars", "499");
        AddOrbiter("Jupiter", "599");
        AddOrbiter("Saturn", "699");
        AddOrbiter("Uranus", "799");
        AddOrbiter("Neptune", "899");
        AddOrbiter("Pluto", "999");

        AddOrbiter("Moon", "301");
        
        AddOrbiter("Phobos", "401");
        AddOrbiter("Deimos", "402");

        AddOrbiter("Io", "501");
        AddOrbiter("Europa", "502");
        AddOrbiter("Ganymede", "503");
        AddOrbiter("Callisto", "504");

        AddOrbiter("Mimas", "601");
        AddOrbiter("Enceladus", "602");
        AddOrbiter("Tethys", "603");
        AddOrbiter("Dione", "604");
        AddOrbiter("Rhea", "605");
        AddOrbiter("Titan", "606");
        AddOrbiter("Hyperion", "607");
        AddOrbiter("Iapetus", "608");

        AddOrbiter("Ariel", "701");
        AddOrbiter("Umbriel", "702");
        AddOrbiter("Titania", "703");
        AddOrbiter("Oberon", "704");
        AddOrbiter("Miranda", "705");

        AddOrbiter("Triton", "801");
        AddOrbiter("Nereid", "802");
        AddOrbiter("Larissa", "807");
        AddOrbiter("Proteus", "808");

        AddOrbiter("Charon", "901");

        AddOrbiter("Ceres", "1;");
        AddOrbiter("Haumea", "136108;");
        AddOrbiter("Makemake", "136472;");
        AddOrbiter("Eris", "136199;");
        AddOrbiter("Vesta", "4;");
        AddOrbiter("Sedna", "90377;");
        AddOrbiter("Quaoar", "50000;");
        AddOrbiter("Orcus", "90482;");
        AddOrbiter("Ixion", "28978;"); 
        AddOrbiter("Pallas", "2;");
        AddOrbiter("2007 OR10", "225088;");
        AddOrbiter("2002 TC302", "84522;");
        
        AddOrbiter("Apophis", "99942;");
        AddOrbiter("Comet Halley", "900033;");

        AddOrbiter("New Horizons", "-98", new DateTime(2006, 1, 19), new DateTime(2015, 9, 25));
    }

    public static bool GetData(DateTime Date)
    {
        string startDate = Date.ToString("yyyy-MMM-dd HH:mm:ss.ffff");
        string endDate = Date.AddDays(1).ToString("yyyy-MMM-dd HH:mm:ss.ffff");

        StringBuilder sbEphemeris = new();

        Telnet tc = new("ssd.jpl.nasa.gov", 6775);

        Queue<string> commandQueue = [];

        commandQueue.Enqueue("tty 80 240");

        bool ready = false;
        int triesLeft;

        System.Threading.Thread.Sleep(500);

        bool isFirst = true;
        const int TRY_COUNT = 200;
        Buffer inputBuffer = new();

        foreach (var o in orbiterCodes.Where(oc => oc.Item3 < Date && oc.Item4 > Date))
        {
            if (tc.IsConnected)
            {
                triesLeft = TRY_COUNT;
                bool gotData = false;
                bool readingData = false; 
                QueueCommands(startDate, endDate, commandQueue, ref isFirst, o.Item2);

                while (tc.IsConnected && (commandQueue.Count > 0 || !gotData))
                {
                    if (--triesLeft < 0)
                        throw new Exception("Horizons Time Out");

                    inputBuffer.Read(tc);

                    if (inputBuffer.HasPendingText)
                    {
                        var nrtl = inputBuffer.PendingText.Trim().ToLower();

                        if (nrtl.EndsWith("return to continue -->"))
                            tc.WriteLine();
                        else if (nrtl.Contains("scroll & page: space, <cr>"))
                            tc.Write(" ");

                        ready |= nrtl.EndsWith("horizons>") || nrtl.EndsWith(':');
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(3000 / TRY_COUNT);
                    }

                    while (inputBuffer.HasLine)
                    {
                        triesLeft = TRY_COUNT;

                        string line = inputBuffer.Dequeue();

                        if (line.StartsWith(END_MARKER))
                        {
                            readingData = false;
                            gotData = true;
                        }
                        if (readingData)
                            sbEphemeris.AppendLine($"{o.Item1},{line}");

                        if (line.StartsWith(START_MARKER))
                            readingData = true;
                    }
                    if (ready && commandQueue.Count > 0)
                    {
                        string cmd = commandQueue.Dequeue();

                        tc.WriteLine(cmd);

                        inputBuffer.WriteToLog("{{Command Sent: " + cmd + "}}");
                        triesLeft = TRY_COUNT;
                        ready = false;
                    }
                }
            }
        }

        tc.WriteLine("quit");

        IO.WriteFile("all_horizons_output.txt", DirectoryLocation.Data, inputBuffer.Log);

        if (sbEphemeris.Length == 0)
            return false;

        StringBuilder eph = new();

        eph.AppendLine("// Improperly altering this file can cause SolarMax to malfunction");
        eph.AppendLine();
        eph.AppendLine("// Original Ephemeris: Version is Zero");
        eph.AppendLine("0");
        eph.AppendLine();
        eph.AppendLine("// Baseline Date");
        eph.AppendLine(Date.ToString(Physics.DATE_FORMAT_1));
        eph.AppendLine();
        eph.AppendLine("// Name, X, Y, Z, VX, VY, VZ");

        foreach (string s in sbEphemeris.ToString().Split([Environment.NewLine], StringSplitOptions.None))
            eph.AppendLine(StripColsTwoAndThree(s));

        IO.WriteFile("ephemeris_" + Date.ToString(Ephemeris.DATE_FORMAT_FOR_FILE_NAME) + ".txt", DirectoryLocation.Data, eph.ToString());

        return true;
    }
    private static void AddOrbiter(string Name, string Code)
    {
        AddOrbiter(Name, Code, DateTime.MinValue, DateTime.MaxValue);
    }
    private static void AddOrbiter(string Name, string Code, DateTime MinDate, DateTime MaxDate)
    {
        orbiterCodes.Add(new Tuple<string, string, DateTime, DateTime>(Name, Code, MinDate, MaxDate));
    }
    private static void QueueCommands(string StartDate, string EndDate, Queue<string> CommandQueue, ref bool IsFirst, string Target)
    {
        CommandQueue.Enqueue(Target);
        CommandQueue.Enqueue("E"); // Ephemeris
        CommandQueue.Enqueue("V"); // Vectors

        if (IsFirst)
            CommandQueue.Enqueue("@sun"); // relative to sun center
        else
            CommandQueue.Enqueue("Y");    // accept previous

        CommandQueue.Enqueue("eclip");    // Relative to Ecliptic
        CommandQueue.Enqueue(StartDate);
        CommandQueue.Enqueue(EndDate);
        CommandQueue.Enqueue("1y");       // 1 year periodicity (so we get only one)
        if (IsFirst)
        {
            CommandQueue.Enqueue("n");    // specify table output
            CommandQueue.Enqueue("J2000");
            CommandQueue.Enqueue("1");    // no corrections
            CommandQueue.Enqueue("1");    // km/s
            CommandQueue.Enqueue("YES");  // csv
            CommandQueue.Enqueue("NO");   // no labels
            CommandQueue.Enqueue("2");    // output type 2, position + velocity XYZ
            IsFirst = false;
        }
        else
        {
            CommandQueue.Enqueue("y");    // accept current table output
        }
        CommandQueue.Enqueue("N");        // new case
    }
    private static string StripColsTwoAndThree(string Input)
    {
        string[] s = Input.Split(',');
        StringBuilder sb = new();

        int len = s.Length;
        
        // fix annoying trailing comma
        if (s[^1].Trim().Length == 0)
            len--;

        for (int i = 0; i < len; i++)
        {
            if (i != 1 && i != 2)
            {
                sb.Append(s[i]);
                if (i < s.Length - 1)
                    sb.Append(',');
            }
        }
        return sb.ToString();
    }
    private class Buffer
    {
        public string Log => log.ToString();

        public string PendingText { get; private set; }
        private readonly char[] endLines = ['\n', '\r'];

        private readonly Queue<string> Lines;
        private readonly StringBuilder log;
        public Buffer()
        {
            Lines = new Queue<string>();
            PendingText = string.Empty;
            log = new StringBuilder();
        }
        public bool HasLine => Lines.Count > 0;
        public bool HasPendingText => PendingText.Length > 0;
        public string Dequeue() => Lines.Dequeue();
        public void Read(Telnet TC)
        {
            string read = TC.Read();
            log.Append(read);
            PendingText += read;

            System.Diagnostics.Debug.Write(read);

            int index;

            while ((index = PendingText.IndexOfAny(endLines)) >= 0)
            {
                string line = PendingText[..index];
                PendingText = PendingText[(index + 1)..];

                if (line.Length > 0)
                    Lines.Enqueue(line);
            }
        }
        public void WriteToLog(string Log)
        {
            this.log.AppendLine();
            this.log.AppendLine(Log);
        }
    }
}
