using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax
{
    internal class HorizonsDownload
    {
        private static List<Tuple<string, string, DateTime, DateTime>> orbiterCodes;

        private static string START_MARKER = "$$SOE";
        private static string END_MARKER = "$$EOE";

        static HorizonsDownload()
        {
            orbiterCodes = new List<Tuple<string, string, DateTime, DateTime>>();

            addOrbiter("Sun", "10");
            addOrbiter("Mercury", "199");
            addOrbiter("Venus", "299");
            addOrbiter("Earth", "399");
            addOrbiter("Mars", "499");
            addOrbiter("Jupiter", "599");
            addOrbiter("Saturn", "699");
            addOrbiter("Uranus", "799");
            addOrbiter("Neptune", "899");
            addOrbiter("Pluto", "999");

            addOrbiter("Moon", "301");
            
            addOrbiter("Phobos", "401");
            addOrbiter("Deimos", "402");

            addOrbiter("Io", "501");
            addOrbiter("Europa", "502");
            addOrbiter("Ganymede", "503");
            addOrbiter("Callisto", "504");

            addOrbiter("Mimas", "601");
            addOrbiter("Enceladus", "602");
            addOrbiter("Tethys", "603");
            addOrbiter("Dione", "604");
            addOrbiter("Rhea", "605");
            addOrbiter("Titan", "606");
            addOrbiter("Hyperion", "607");
            addOrbiter("Iapetus", "608");

            addOrbiter("Ariel", "701");
            addOrbiter("Umbriel", "702");
            addOrbiter("Titania", "703");
            addOrbiter("Oberon", "704");
            addOrbiter("Miranda", "705");

            addOrbiter("Triton", "801");
            addOrbiter("Nereid", "802");
            addOrbiter("Larissa", "807");
            addOrbiter("Proteus", "808");

            addOrbiter("Charon", "901");

            addOrbiter("Ceres", "1;");
            addOrbiter("Haumea", "136108;");
            addOrbiter("Makemake", "136472;");
            addOrbiter("Eris", "136199;");
            addOrbiter("Vesta", "4;");
            addOrbiter("Sedna", "90377;");
            addOrbiter("Quaoar", "50000;");
            addOrbiter("Orcus", "90482;");
            addOrbiter("Ixion", "28978;"); 
            addOrbiter("Pallas", "2;");
            addOrbiter("2007 OR10", "225088;");
            addOrbiter("2002 TC302", "84522;");
            
            addOrbiter("Apophis", "99942;");
            addOrbiter("Comet Halley", "900033;");

            addOrbiter("New Horizons", "-98", new DateTime(2006, 1, 19), new DateTime(2015, 9, 25));
        }

        public static bool GetData(DateTime Date)
        {
            string startDate = Date.ToString("yyyy-MMM-dd HH:mm:ss.ffff");
            string endDate = Date.AddDays(1).ToString("yyyy-MMM-dd HH:mm:ss.ffff");

            StringBuilder sbEphemeris = new StringBuilder();

            Telnet tc = new Telnet("ssd.jpl.nasa.gov", 6775);

            Queue<string> commandQueue = new Queue<string>();

            commandQueue.Enqueue("tty 80 240");

            bool ready = false;
            int triesLeft;

            System.Threading.Thread.Sleep(500);

            bool isFirst = true;
            const int TRY_COUNT = 200;
            Buffer inputBuffer = new Buffer();

            foreach (var o in orbiterCodes.Where(oc => oc.Item3 < Date && oc.Item4 > Date))
            {
                if (tc.IsConnected)
                {
                    triesLeft = TRY_COUNT;
                    bool gotData = false;
                    bool readingData = false; 
                    queueCommands(startDate, endDate, commandQueue, ref isFirst, o.Item2);

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

                            ready |= nrtl.EndsWith("horizons>") || nrtl.EndsWith(":");
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
                                sbEphemeris.AppendLine(o.Item1 + "," + line);

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

            StringBuilder eph = new StringBuilder();

            eph.AppendLine("// Improperly altering this file can cause SolarMax to malfunction");
            eph.AppendLine();
            eph.AppendLine("// Original Ephemeris: Version is Zero");
            eph.AppendLine("0");
            eph.AppendLine();
            eph.AppendLine("// Baseline Date");
            eph.AppendLine(Date.ToString(Physics.DATE_FORMAT_1));
            eph.AppendLine();
            eph.AppendLine("// Name, X, Y, Z, VX, VY, VZ");

            foreach (string s in sbEphemeris.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None))
                eph.AppendLine(stripColsTwoAndThree(s));

            IO.WriteFile("ephemeris_" + Date.ToString(Ephemeris.DATE_FORMAT_FOR_FILE_NAME) + ".txt", DirectoryLocation.Data, eph.ToString());

            return true;
        }
        private static void addOrbiter(string Name, string Code)
        {
            addOrbiter(Name, Code, DateTime.MinValue, DateTime.MaxValue);
        }
        private static void addOrbiter(string Name, string Code, DateTime MinDate, DateTime MaxDate)
        {
            orbiterCodes.Add(new Tuple<string, string, DateTime, DateTime>(Name, Code, MinDate, MaxDate));
        }
        private static void queueCommands(string StartDate, string EndDate, Queue<string> CommandQueue, ref bool IsFirst, string Target)
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
        private static string stripColsTwoAndThree(string Input)
        {
            string[] s = Input.Split(',');
            StringBuilder sb = new StringBuilder();

            int len = s.Length;
            
            // fix annoying trailing comma
            if (s[s.Length - 1].Trim().Length == 0)
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
            public string Log
            {
                get { return log.ToString(); }
            }

            public string PendingText { get; private set; }
            private readonly char[] endLines = new char[] { '\n', '\r' };

            private Queue<string> Lines;
            private StringBuilder log;
            public Buffer()
            {
                Lines = new Queue<string>();
                PendingText = string.Empty;
                log = new StringBuilder();
            }
            public bool HasLine { get { return Lines.Count > 0; } }
            public bool HasPendingText { get { return PendingText.Length > 0; } }
            public string Dequeue()
            {
                return Lines.Dequeue();
            }
            public void Read(Telnet TC)
            {
                string read = TC.Read();
                log.Append(read);
                PendingText += read;

                System.Diagnostics.Debug.Write(read);

                int index;

                while ((index = PendingText.IndexOfAny(endLines)) >= 0)
                {
                    string line = PendingText.Substring(0, index);
                    PendingText = PendingText.Substring(index + 1);

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
}
