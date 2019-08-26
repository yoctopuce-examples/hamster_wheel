using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Yoctopuce_Hamster_Wheel
{
    class HamsterController
    {
        private string _url;
        private readonly string _displayHwId;
        private readonly string _nextButtonHwId;
        private readonly string _prevButtonHwId;
        private readonly string _pwmHwId;
        private List<HamsterRunArchiver> _archivers = new List<HamsterRunArchiver>();

        private HamsterScreen _hamsterScreen;
        private HamsterWheel _hamsterWheel;
        private HamsterButton _hamsterNextButton;
        private HamsterButton _hamsterPrevButton;

        enum DisplayMode
        {
            SPEED,
            MAX_SPEED,
            AVG_SPEED,
            DISTANCE,
            TIME,
            ALL
        }

        private DisplayMode _mode = DisplayMode.AVG_SPEED;
        private double _currentSpeed = 0;
        private HamsterRun _currenRun;
        private uint _inactivityS;
        private uint _diameterMM;
        private HamsterRun _lastRun;


        public HamsterController(string url, string pwmHwId, string displayHwId, string nextButtonHwId, string prevButtonHwId, uint diameterMM, uint inactivityS, string csvfile)
        {
            _url = url;
            _nextButtonHwId = nextButtonHwId;
            _prevButtonHwId = prevButtonHwId;
            _pwmHwId = pwmHwId;
            _displayHwId = displayHwId;
            _currenRun = new HamsterRun();
            _diameterMM = diameterMM;
            _inactivityS = inactivityS;
            _lastRun = new HamsterRun();
            if (csvfile != "") {
                _archivers.Add(new CSVRunArchiver(csvfile));
            }
        }


        public int RunForever()
        {
            string errmsg = "";
            int res = YAPI.RegisterHub(_url, ref errmsg);
            if (res != YAPI.SUCCESS) {
                Console.Error.WriteLine("Unable to register " + _url + " hub:" + errmsg);
                return 1;
            }

            foreach (HamsterRunArchiver archiver in _archivers) {
                archiver.Init();
            }

            try {
                _hamsterScreen = new HamsterScreen(_displayHwId);
                _hamsterNextButton = new HamsterButton(_nextButtonHwId, nextButtonPressed);
                _hamsterPrevButton = new HamsterButton(_prevButtonHwId, prevButtonPressed);
                _hamsterWheel = new HamsterWheel(_pwmHwId, _diameterMM, _inactivityS, updateLiveValue, endOfExercice);
                _hamsterWheel.runForever();
            } catch (Exception ex) {
                Console.Error.WriteLine("Fatal error:" + ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
                return 1;
            }

            return 0;
        }

        private void nextButtonPressed(HamsterButton oject)
        {
            Debug.WriteLine(_mode.ToString() + "++");
            if (_mode < DisplayMode.ALL) {
                _mode++;
            } else {
                _mode = DisplayMode.SPEED;
            }

            UpdateDisplay();
        }

        private void prevButtonPressed(HamsterButton oject)
        {
            Debug.WriteLine(_mode.ToString() + "--");
            if (_mode > DisplayMode.SPEED) {
                _mode--;
            } else {
                _mode = DisplayMode.ALL;
            }

            UpdateDisplay();
        }

        private void endOfExercice(DateTime start, double duration, double avgSpeed, double maxSpeed, double distance)
        {
            Console.Out.WriteLine("Exercise summary:[{0}] avgSpeed={1}km/h maxSpeed={2}km/h distance={3}m duration={4}s", start.ToString("u"), avgSpeed, maxSpeed, distance, duration);
            _lastRun = new HamsterRun(start, duration, avgSpeed, maxSpeed, distance);
            foreach (HamsterRunArchiver archiver in _archivers) {
                archiver.Add(_lastRun);
            }
        }

        private void updateLiveValue(double currentspeed, double currentdistance, double currentduration, double maxspeed, double avgspeed)
        {
            _currentSpeed = currentspeed;
            if (_hamsterWheel.IsRunning()) {
                Debug.WriteLine("Live: speed={0:0.00} avg={1:0.00} max={2:0.00} distance={3:0.00} duration={4:0.00}", currentspeed, avgspeed, maxspeed, currentdistance, currentduration);
                _currenRun.Distance = currentdistance;
                _currenRun.AVGSpeed = avgspeed;
                _currenRun.MaxSpeed = maxspeed;
                _currenRun.Duration = currentduration;
            } else {
                Debug.WriteLine("Live: speed={0:0.00} avg={1:0.00} max={2:0.00} distance={3:0.00} duration={4:0.00}", currentspeed, avgspeed, maxspeed, currentdistance, currentduration);
                _currenRun.Distance = _lastRun.Distance;
                _currenRun.AVGSpeed = _lastRun.AVGSpeed;
                _currenRun.MaxSpeed = _lastRun.MaxSpeed;
                _currenRun.Duration = _lastRun.Duration;
            }

            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            switch (_mode) {
                case DisplayMode.SPEED:
                    _hamsterScreen.DisplaySpeed("Speed", _currentSpeed);
                    break;
                case DisplayMode.MAX_SPEED:
                    _hamsterScreen.DisplaySpeed("Max Speed", _currenRun.MaxSpeed);
                    break;
                case DisplayMode.AVG_SPEED:
                    _hamsterScreen.DisplaySpeed("Avg Speed", _currenRun.AVGSpeed);
                    break;
                case DisplayMode.DISTANCE:
                    _hamsterScreen.DisplayDistance("Distance", _currenRun.Distance);
                    break;
                case DisplayMode.TIME:
                    _hamsterScreen.DisplayDuration("Duration", _currenRun.Duration);
                    break;
                case DisplayMode.ALL:
                default:
                    _hamsterScreen.DisplayFull(_currentSpeed, _currenRun.Distance, _currenRun.Duration, _currenRun.MaxSpeed, _currenRun.AVGSpeed, "km/h", "s", "km");
                    break;
            }
        }
    }
}