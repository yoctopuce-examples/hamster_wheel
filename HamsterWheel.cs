using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Text;
using System.Threading;

namespace Yoctopuce_Hamster_Wheel
{
    class HamsterWheel
    {
        public delegate void LiveValues(double CurrentSpeed, Double CurrentDistance, Double CurrentDuration, Double MaxSpeed, Double AvgSpeed);

        public delegate void StartOfExercice();

        public delegate void EndOfExercice(DateTime time, double duration, double avgSpeed, double maxSpeed, double distance);

        private string _hwid;
        private LiveValues _liveUpdateCb;
        private EndOfExercice _endOfExerciceCb;


        private bool _isRunning;
        private DateTime _startTime;
        private long _initialCount;
        private long _lastCount;
        private ulong _firstTickCount;
        private ulong _lastTickCount;
        private double _maxSpeedCMS;
        private double _lastSpeedCMS;
        private double _totalCount;
        private readonly double _perimeterKm;
        private readonly ulong _inactivityMS;

        public double getCurrentSpeedKmh()
        {
            return _lastSpeedCMS * _perimeterKm * 3600000;
        }


        public double getDistanceKm()
        {
            return _totalCount * _perimeterKm;
        }

        public double getDistanceM()
        {
            return _totalCount * _perimeterKm * 1000;
        }

        public double getDurationS()
        {
            return (_lastTickCount - _firstTickCount) / 1000.0;
        }

        public double getAVGSpeedKmh()
        {
            double durationS = getDurationS();
            if (durationS == 0) {
                return 0;
            }

            return getDistanceKm() * 3600 / durationS;
        }

        public double getMaxSpeedKmh()
        {
            return _maxSpeedCMS * _perimeterKm * 3600000;
        }


        public void ResetRun(long initialcounter)
        {
            _initialCount = initialcounter;
            _lastCount = initialcounter;
            _startTime = DateTime.Now;
            _firstTickCount = YAPI.GetTickCount();
            _lastTickCount = _firstTickCount;
            _lastSpeedCMS = 0;
            _totalCount = 0;
            _maxSpeedCMS = 0;
        }


        public void Stop()
        {
            _isRunning = false;
        }

        public bool IsRunning()
        {
            if (_totalCount == 0) {
                return false;
            }

            ulong now = YAPI.GetTickCount();
            ulong delta = now - _lastTickCount;
            return delta < _inactivityMS;
        }

        public DateTime getStartTime()
        {
            return _startTime;
        }


        public HamsterWheel(string hwid, double diameterMM, uint inactivityS, LiveValues liveUpdateCB, EndOfExercice endOfExerciceCb)
        {
            _hwid = hwid;
            double perimeterKm = (diameterMM * Math.PI) / 1000000;
            _liveUpdateCb = liveUpdateCB;
            _endOfExerciceCb = endOfExerciceCb;
            _perimeterKm = perimeterKm;
            _inactivityMS = inactivityS * 1000;
            _isRunning = false;
        }

        public void runForever()
        {
            YPwmInput pwmInput;
            if (_hwid != "") {
                pwmInput = YPwmInput.FindPwmInput(_hwid);
                if (!pwmInput.isOnline()) {
                    throw new Exception("No Yocto-PWM-Rx name " + _hwid + " found");
                }
            } else {
                pwmInput = YPwmInput.FirstPwmInput();
                if (pwmInput == null) {
                    throw new Exception("No Yocto-PWM-Rx connected");
                }
            }

            log("use PWM " + pwmInput.get_hardwareId());

            configurePWMInput(pwmInput);

            pwmInput.resetCounter();
            ResetRun(pwmInput.get_pulseCounter());
            pwmInput.registerValueCallback(count_callback);
            string errmsg = "";
            while (true) {
                YAPI.Sleep(1000, ref errmsg);
                _liveUpdateCb(getCurrentSpeedKmh(), getDistanceM(), getDurationS(), getMaxSpeedKmh(), getAVGSpeedKmh());
                if (getDistanceKm() > 0 && !IsRunning()) {
                    _endOfExerciceCb(getStartTime(), getDurationS(), getAVGSpeedKmh(), getMaxSpeedKmh(), getDistanceM());
                    ResetRun(pwmInput.get_pulseCounter());
                }
            }
        }

        private void count_callback(YPwmInput func, string value)
        {
            int count = Int32.Parse(value, CultureInfo.InvariantCulture);
            if (count == _initialCount) {
                return;
            }

            if (!_isRunning) {
                // we start the run at the first magnet pass
                ResetRun(count);
                _isRunning = true;
                return;
            }

            ulong now = YAPI.GetTickCount();
            ulong deltaTime = now - _lastTickCount;


            long deltaCount;
            if (_lastCount > count) {
                //Fixme: handle wrap
                deltaCount = count;
            } else {
                deltaCount = (count - _lastCount);
            }

            double speed = (double) deltaCount / deltaTime;
            Debug.WriteLine(String.Format("count ={0} delta={1} time={2} speed={3}", count, deltaCount, deltaTime, speed));

            //Update Max speed
            if (_maxSpeedCMS < speed) {
                _maxSpeedCMS = speed;
            }

            _totalCount += deltaCount;
            _lastSpeedCMS = speed;
            _lastTickCount = now;
            _lastCount = count;
        }

        private void configurePWMInput(YPwmInput pwmInput)
        {
            this.log("Configure PWM Input");

            // Set debounce value to 5 ms
            pwmInput.set_debouncePeriod(5);
            pwmInput.set_pwmReportMode(YPwmInput.PWMREPORTMODE_PWM_PULSECOUNT);
            pwmInput.set_logFrequency("1/m");
            YDataLogger dataLogger = pwmInput.get_dataLogger();
            dataLogger.set_autoStart(YDataLogger.AUTOSTART_ON);
            dataLogger.set_recording(YDataLogger.RECORDING_ON);
        }

        private void log(string line)
        {
            Console.Out.WriteLine(line);
        }
    }
}