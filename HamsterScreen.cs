using System;
using System.Collections.Generic;
using System.Text;

namespace Yoctopuce_Hamster_Wheel
{
    class HamsterScreen
    {
        private YDisplay _display;
        private int _w;
        private int _h;
        private YDisplayLayer _fgLayer;
        private YDisplayLayer _bgLayer;


        public HamsterScreen(string hwid)
        {
            if (hwid == "") {
                _display = YDisplay.FirstDisplay();
                if (_display == null) {
                    throw new Exception("No Yocto-Display connected");
                }
            } else {
                _display = YDisplay.FindDisplay(hwid);
                if (!_display.isOnline()) {
                    throw new Exception("No Yocto-Display named " + hwid + "found");
                }
            }

            _display.resetAll();
            _w = _display.get_displayWidth();
            _h = _display.get_displayHeight();
            _fgLayer = _display.get_displayLayer(2);
            _bgLayer = _display.get_displayLayer(1);
        }


        public void DisplayError(string msg)
        {
            _bgLayer.reset();
            _bgLayer.selectGrayPen(255);
            _bgLayer.drawBar(0, 0, _w, _h);
            _bgLayer.selectColorPen(0);
            _bgLayer.drawText(_w / 2, _h / 3, YDisplayLayer.ALIGN.CENTER, "ERROR !");
            _bgLayer.drawText(_w / 2, _h * 2 / 3, YDisplayLayer.ALIGN.CENTER, msg);
        }


        public void RePaint()
        {
            _display.swapLayerContent(1, 2);
        }

        public bool isOnline()
        {
            return _display.isOnline();
        }

        public void DisplayFull(double currentspeed, double currentdistance, double currentduration, double maxspeed, double avgspeed, string speedUnit, string timeUnit, string distanceUnit)
        {
            if (_display.isOnline()) {
                _bgLayer.clear();
                _bgLayer.selectGrayPen(0);
                _bgLayer.drawBar(0, 0, _w, _h);
                _bgLayer.selectGrayPen(255);
                _bgLayer.selectFont("Small.yfm");
                _bgLayer.drawText(1, 1 * _h / 5, YDisplayLayer.ALIGN.CENTER_LEFT, String.Format("speed={0:0.0}{1}", currentspeed, speedUnit));
                _bgLayer.drawText(1, 2 * _h / 5, YDisplayLayer.ALIGN.CENTER_LEFT, String.Format("(max={0:0.0}{2} avg={1:0.0}{2})", maxspeed, avgspeed, speedUnit));
                _bgLayer.drawText(1, 3 * _h / 5, YDisplayLayer.ALIGN.CENTER_LEFT, String.Format("distance = {0:0.0}{1}", currentdistance, distanceUnit));
                _bgLayer.drawText(1, 4 * _h / 5, YDisplayLayer.ALIGN.CENTER_LEFT, String.Format("Time = {0:0.0}{1}", currentduration, timeUnit));
                RePaint();
            }
        }

        public void DisplaySpeed(string label1, string label2, double speed)
        {
            DisplaySingleValue(label1, label2, String.Format("{0:0.0} km/h", speed));
        }

        private void DisplaySingleValue(string label, string labe2, string value)
        {
            if (_display.isOnline()) {
                _bgLayer.clear();
                _bgLayer.selectGrayPen(0);
                _bgLayer.drawBar(0, 0, _w, _h);
                _bgLayer.selectGrayPen(255);
                _bgLayer.selectFont("Small.yfm");
                _bgLayer.drawText(2, 2, YDisplayLayer.ALIGN.TOP_LEFT, label);
                if (labe2 != "") {
                    _bgLayer.drawText(_w - 2, 2, YDisplayLayer.ALIGN.TOP_LEFT, labe2);
                }

                _bgLayer.selectFont("Medium.yfm");
                _bgLayer.drawText(_w / 2, _h / 2, YDisplayLayer.ALIGN.CENTER, value);
                RePaint();
            }
        }

        public void DisplayDistance(string label, string dur, double currentdistance)
        {
            DisplaySingleValue(label, dur, String.Format("{0:0.0} m", currentdistance));
        }

        public void DisplayDuration(string label, string dur, double currentdurationS)
        {
            TimeSpan time = TimeSpan.FromSeconds(currentdurationS);
            string str = time.ToString(@"hh\:mm\:ss");
            DisplaySingleValue(label,dur, str);
        }
    }


    class HamsterButton
    {
        private bool isUp = true;
        private YAnButton _button;
        private PressedDelegate _pressedDe;

        public delegate void PressedDelegate(HamsterButton oject);


        public HamsterButton(string hwid, PressedDelegate pressedDe)
        {
            _button = YAnButton.FindAnButton(hwid);
            if (!_button.isOnline()) {
                throw new Exception("No anButton named " + hwid);
            }

            _pressedDe = pressedDe;
            _button.registerValueCallback(AnButtonListener);
        }

        private void AnButtonListener(YAnButton func, string value)
        {
            int intVal = Convert.ToInt32(value);
            if (intVal < 200) {
                if (isUp) {
                    isUp = false;
                }
            } else if (intVal > 800) {
                if (!isUp) {
                    _pressedDe(this);
                    isUp = true;
                }
            }
        }
    }
}