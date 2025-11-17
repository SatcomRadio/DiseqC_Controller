using Iot.Device.Button;
using nanoFramework.Hardware.Esp32;
using System;
using System.Device.Adc;
using System.Diagnostics;
using System.Threading;

namespace DiseqC.Manager
{
    internal class ButtonManager
    {
        private const int Pin = 10;
        private const int PotentiometerPin = 2;

        private readonly RotorManager _rotorMgr;
        private readonly GpioButton _button;
        private readonly AdcChannel _pot;
        private readonly long _timeBetweenPresses = TimeSpan.FromMilliseconds(1500).Ticks;

        private long _lastPress = DateTime.MinValue.Ticks;
        
        public ButtonManager(RotorManager rotorMgr)
        {
            var adc = new AdcController();
            _pot = adc.OpenChannel(PotentiometerPin);

            _rotorMgr = rotorMgr;
            _button = new GpioButton(buttonPin: Pin);
            _button.IsDoublePressEnabled = false;
            _button.IsHoldingEnabled = false;
        }

        public void DebugPot()
        {
            var t = new Thread(() =>
            {
                while (true)
                {
                    var percent = _pot.ReadRatio();
                    var angle = Map(percent, 0, 1, -180, 180);
                    var roundAngle = (int)(Math.Round(angle / 20.0) * 20);

                    Debug.WriteLine($"{_pot.ReadValue()} - {percent} - {angle} - {roundAngle}");
                    Thread.Sleep(50);
                }
            });
            t.Start();
        }

        public void MonitorButtonPresses()
        {
            _button.Press += (sender, e) => { ButtonPress(); };
        }

        private void ButtonPress()
        {
            if (DateTime.UtcNow.Ticks - _lastPress >= _timeBetweenPresses)
                GotoPotentiometerAngle();

            _lastPress = DateTime.UtcNow.Ticks;
        }

        private void GotoPotentiometerAngle()
        {
            var percent = _pot.ReadRatio();
            var angle = Map(percent, 0, 1, -180, 180);
            var roundAngle = (int)(Math.Round(angle / 10.0) * 10);

            _rotorMgr.GotoAngle(roundAngle, 120);
        }

        private static double Map(double x, double inMin, double inMax, double outMin, double outMax)
        {
            return (x - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
        }
    }
}