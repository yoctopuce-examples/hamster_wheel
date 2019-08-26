using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CsvHelper;

namespace Yoctopuce_Hamster_Wheel
{
    class CSVRunArchiver : HamsterRunArchiver
    {
        private string _filename;

        public CSVRunArchiver(string filename)
        {
            _filename = filename;
        }

        public void Init()
        {
            if (!File.Exists(_filename)) {
                string headerline = formatRow("Date", "Duration", "AVG Speed", "Max Speed", "Distance");
                File.WriteAllText(_filename, headerline);
            }
        }

        public void Add(HamsterRun newRun)
        {
            string timestr = newRun.Time.ToString("u");
            string line = formatRow(timestr, newRun.Duration.ToString(), newRun.AVGSpeed.ToString(), newRun.MaxSpeed.ToString(), newRun.Distance.ToString());
            File.AppendAllText(_filename, line);
        }

        private string formatRow(string start, string duration, string avg, string max, string distance)
        {
            return string.Format("{0},{1},{2},{3},{4}" + Environment.NewLine, start, duration, avg, max, distance);
        }
    }
}