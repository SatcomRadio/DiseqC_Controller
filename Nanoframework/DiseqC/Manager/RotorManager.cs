using nanoFramework.Hardware.Esp32;
using DiseqC.Manager.Led;
using System;
using System.Device;
using System.Device.Gpio;
using System.Device.Pwm;
using System.IO.Ports;
using System.Threading;

namespace DiseqC.Manager
{
    internal class RotorManager
    {
        public float CurrentAngle;

        private const int Frequency = 22000;
        private const int MaxAngle = 80;
        private const int DataPin = 0;
        private const int Resolution = 8;

        //private readonly int _ticks1000Us = (int)TimeSpan.FromTicks(1000 * TimeSpan.TicksPerMillisecond / 1000).Ticks;
        //private readonly int _ticks500Us = (int)TimeSpan.FromTicks(500 * TimeSpan.TicksPerMillisecond / 1000).Ticks;

        private readonly int _ticks1000Us = int.MaxValue - 1;
        private readonly int _ticks500Us = int.MaxValue - 1;

        private readonly PwmChannel _pwm;
        private readonly MotorLedManager _motorLed;
        private Thread _ledThread;

        public RotorManager(MotorLedManager motorLed)
        {
            _motorLed = motorLed;
            Configuration.SetPinFunction(DataPin, DeviceFunction.PWM1);
            _pwm = PwmChannel.CreateFromPin(DataPin, Frequency);
        }

        public void GotoAngle(float angle, int expectedTravelTimeSec)
        {
            _pwm.Start();
            Thread.Sleep(1);
            _pwm.Stop();
            Thread.Sleep(50);

            CurrentAngle = angle;

            angle = angle switch
            {
                > MaxAngle => MaxAngle,
                < MaxAngle * -1 => MaxAngle * -1,
                _ => angle
            };

            SetMotorLed(expectedTravelTimeSec);

            var n1 = angle < 0 ? (byte)0xE0 : (byte)0xD0;

            var a16 = (int)(16.0f * Math.Abs(angle) + 0.5f);
            var n2 = (byte)((a16 & 0xF00) >> 8);
            var d2 = (byte)(a16 & 0xFF);
            var d1 = (byte)(n1 | n2);

            // send the command sequence to the positioner
            WriteByteWithParity(0xE0);
            WriteByteWithParity(0x31);
            WriteByteWithParity(0x6E);
            WriteByteWithParity(d1);
            WriteByteWithParity(d2);
        }

        private void SetMotorLed(int travelTimeSec)
        {
            if (_ledThread != null && _ledThread.IsAlive)
            {
                _ledThread.Abort();
                _ledThread.Join();
            }
            
            _ledThread = new Thread(() =>
            {
                _motorLed.SetState(PinValue.High);
                Thread.Sleep(TimeSpan.FromSeconds(travelTimeSec));
                _motorLed.SetState(PinValue.Low);
            });
            _ledThread.Start();
        }

        private void Write0()
        {
            _pwm.Start();
            for (var i = 0; i < 130; i++)
            {
                //Do nothing
            }

            _pwm.Stop();
            for (var i = 0; i < 60; i++)
            {
                //Do nothing
            }
        }

        private void Write1()
        {
            _pwm.Start();
            for (var i = 0; i < 60; i++)
            {
                //Do nothing
            }

            _pwm.Stop();
            for (var i = 0; i < 130; i++)
            {
                //Do nothing
            }
        }

        private void WriteByteWithParity(byte x)
        {
            WriteByte(x);
            WriteParity(x);
        }

        private void WriteParity(byte x)
        {
            if (ParityHelper.ParityEvenBit(x) == ParityHelper.Parity.EVEN)
                Write0();
            else
                Write1();
        }

        private void WriteByte(byte x)
        {
            for (var j = 7; j >= 0; j--)
            {
                if ((x & (1 << j)) != 0)
                    Write1();
                else
                    Write0();
            }
        }
    }
}
