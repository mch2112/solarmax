using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SolarMax
{
    internal static class IO
    {
        private const string DATA_DIRECTORY_NAME = "celestial_data";

        public static void DeleteFile(string FileName, DirectoryLocation DirectoryLocation)
        {
            try
            {
                System.IO.File.Delete(getRootedFileName(FileName, DirectoryLocation));
            }
            catch { }
        }
        public static IEnumerable<FileInfo> EnumerateFiles(DirectoryLocation DirectoryLocation, string Pattern)
        {
            string path = Path.Combine(IO.GetExecutablePath(), DATA_DIRECTORY_NAME);

            var di = new DirectoryInfo(path);

            return di.EnumerateFiles(Pattern, SearchOption.TopDirectoryOnly);
        }
        public static void WriteFile(string FileName, DirectoryLocation DirectoryLocation, string Contents)
        {
            FileName = getRootedFileName(FileName, DirectoryLocation);

            TextWriter writer;

            writer = new StreamWriter(FileName);

            try
            {
                writer.Write(Contents);
            }
            catch
            {
            }
            finally
            {
                writer.Close();
            }
        }
        
        public static string[,] ReadFile(string FileName, DirectoryLocation DirectoryLocation, char Delimiter = ',')
        {
            return readFile(getRootedFileName(FileName, DirectoryLocation), Delimiter);
        }
        private static string[,] readFile(string FileName, char Delimiter = ',')
        {
            int size = 0;
            List<List<string>> strings = new List<List<string>>();
            TextReader reader = null;

            if (!File.Exists(FileName))
                return new string[0, 0];

            try
            {
                reader = new StreamReader(FileName);
            }
            catch (FileNotFoundException)
            {
                return new string[0, 0];
            }
            try
            {
                string s;

                while ((s = reader.ReadLine()) != null)
                {
                    if (s.Contains("//"))
                        s = s.Substring(0, s.IndexOf("//"));

                    if (s.Contains(";"))
                        s = s.Substring(0, s.IndexOf(";"));
                    
                    s = s.Trim();

                    if (s.Length > 0)
                    {
                        string[] ss = s.Split(Delimiter);

                        size = Math.Max(size, ss.Length);
                        List<string> ls = new List<string>();
                        ls.AddRange(ss);
                        strings.Add(ls);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.ToString());
                return null;
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
            string[,] ret = new string[strings.Count, size];
            for (int i = 0; i < strings.Count; i++)
            {
                for (int j = 0; j < strings[i].Count; j++)
                {
                    ret[i, j] = strings[i][j].Trim();
                }
            }
            return ret;
        }
        private static string getRootedFileName(string FileName, DirectoryLocation DirectoryLocation)
        {
            if (!Path.IsPathRooted(FileName))
            {
                switch (DirectoryLocation)
                {
                    case SolarMax.DirectoryLocation.Root:
                        FileName = Path.Combine(IO.GetExecutablePath(), FileName);
                        break;
                    case SolarMax.DirectoryLocation.Data:
                        FileName = Path.Combine(Path.Combine(IO.GetExecutablePath(), DATA_DIRECTORY_NAME), FileName);
                        break;
                }
            }
            return FileName;
        }
        public static string GetExecutablePath()
        {
            return Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).LocalPath);
        }

    }
}
