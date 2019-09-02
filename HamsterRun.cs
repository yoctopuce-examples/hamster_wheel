using System;
using System.Collections.Generic;
using System.Text;

namespace Yoctopuce_Hamster_Wheel
{
    class HamsterRun
    {
        public double AVGSpeed { get; set; }
        public double MaxSpeed { get; set; }
        public double Distance { get; set; }
        public DateTime Time { get; set; }
        public double Duration { get; set; }

        public HamsterRun(DateTime time, double duration, double avgSpeed, double maxSpeed, double distance)
        {
            AVGSpeed = avgSpeed;
            MaxSpeed = maxSpeed;
            Distance = distance;
            Time = time;
            Duration = duration;
        }

        public HamsterRun()
        {
        }

        public void Add(HamsterRun newRun)
        {
            if (newRun.MaxSpeed > MaxSpeed) {
                MaxSpeed = newRun.MaxSpeed;
            }

            Distance += newRun.Distance;
            Duration += newRun.Duration;

            AVGSpeed = 3.6 * Distance / Duration;
        }

        public void Reset()
        {
            AVGSpeed = 0;
            MaxSpeed = 0;
            Distance = 0;
            Time = DateTime.Now;
            Duration = 0;
        }
    }
}