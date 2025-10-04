using nanoFramework.Hardware.Esp32;
using nanoFramework.Hardware.Esp32.Rmt;
using DiseqC.Manager.Led;
using System;
using System.Device;
using System.Device.Gpio;
using System.Device.Pwm;
using System.IO.Ports;
using System.Threading;

namespace DiseqC.Manager
{
    internal class RmtRotorManager
    {
        public float CurrentAngle;

        private const int Frequency = 22000;
        private const int MaxAngle = 80;
        private const int DataPin = 0;

        private readonly MotorLedManager _motorLed;
        private Thread _ledThread;
        private readonly TransmitterChannel _txChannel;

        public RmtRotorManager(MotorLedManager motorLed)
        {
            _motorLed = motorLed;
            Configuration.SetPinFunction(DataPin, DeviceFunction.PWM1);


            var txChannelSettings = new TransmitChannelSettings(pinNumber: DataPin)
            {
                ClockDivider = 80,
                EnableCarrierWave = true,
                IdleLevel = false,
                CarrierWaveFrequency = 22000
            };

            _txChannel = new TransmitterChannel(txChannelSettings);
        }
        
        public void GotoAngle(float angle, int expectedTravelTimeSec)
        {
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

            _txChannel.ClearCommands();
            WriteByteWithParity(0xE0);
            WriteByteWithParity(0x31);
            WriteByteWithParity(0x6E);
            WriteByteWithParity(d1);
            WriteByteWithParity(d2);
            _txChannel.Send(true);
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
            _txChannel.AddCommand(new RmtCommand(1000, true, 500, false));
        }

        private void Write1()
        {
            _txChannel.AddCommand(new RmtCommand(500, true, 1000, false));
        }

        private void WriteByteWithParity(byte x)
        {
            WriteByte(x);
            WriteParity(x);
        }

        private void WriteParity(byte x)
        {
            if (ParityHelper.ParityEvenBit(x) == ParityHelper.Parity.EVEN)
                Write1();
            else
                Write0();
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
