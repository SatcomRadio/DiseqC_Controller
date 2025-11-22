using Iot.Device.Button;
using nanoFramework.Hardware.Esp32;
using System;
using System.Device.Adc;
using System.Device.Gpio;
using System.Diagnostics;
using System.Threading;
using DiseqC.Manager.Led;

namespace DiseqC.Manager
{
    internal class ButtonManager
    {
        private const int Pin = 10;
        private const int PotentiometerPin = 2;
        private const int MaxAngle = 80;
        private const int MinAngle = -80;
        private const int PotSteps = 20;

        private readonly RotorManager _rotorMgr;
        private readonly GpioButton _button;
        private readonly AdcChannel _pot;
        private readonly MotorLedManager _motorLed;
        private readonly long _timeBetweenPresses = TimeSpan.FromMilliseconds(1500).Ticks;

        private long _lastPress = DateTime.MinValue.Ticks;

        public ButtonManager(RotorManager rotorMgr, MotorLedManager motorLed)
        {
            var adc = new AdcController();
            _pot = adc.OpenChannel(PotentiometerPin);

            _rotorMgr = rotorMgr;
            _motorLed = motorLed;
            _button = new GpioButton(Pin, TimeSpan.FromTicks(15000000L), TimeSpan.FromMilliseconds(1000L), null, true, PinMode.InputPullUp, TimeSpan.Zero);
            _button.IsDoublePressEnabled = false;
            _button.IsHoldingEnabled = false;
        }

        public void DebugPot()
        {
            var t = new Thread(() =>
            {
                while (true)
                {
                    GetPotentiometerAngle(true);
                    Thread.Sleep(50);
                }
            });
            t.Start();
        }

        public void MonitorButtonPresses()
        {
            _button.Press += (sender, e) => { ButtonPress(); };
        }

        public int GetPotentiometerAngle(bool debug = false)
        {
            var percent = _pot.ReadRatio();

            var angle = Map(percent, 0, 1, MinAngle, MaxAngle);

            angle = Math.Min(MaxAngle, angle);
            angle = Math.Max(MinAngle, angle);

            var roundedAngle = (int)(Math.Round(angle / 20.0) * 20);

            if (debug)
                Debug.WriteLine($"{_pot.ReadValue()} - {percent} - {angle} - {roundedAngle}");

            return roundedAngle;
        }

        private void ButtonPress()
        {
            if (DateTime.UtcNow.Ticks - _lastPress >= _timeBetweenPresses)
            {
                _rotorMgr.GotoAngle(GetPotentiometerAngle(), 120);
            }

            _lastPress = DateTime.UtcNow.Ticks;
        }

        private static double Map(double x, double inMin, double inMax, double outMin, double outMax)
        {
            return (x - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
        }
    }
}