using System.Device.Gpio;
using System.Threading;

namespace DiseqC.Manager.Led
{
    internal abstract class LedManager
    {
        private readonly GpioPin _led;
        private readonly bool _reverseState;
        private bool _blink;
        private int _intervalMs;
        private Thread _blinkThread;

        protected LedManager(GpioPin pin, bool reverseState = false)
        {
            if (pin.GetPinMode() != PinMode.Output) return;
            _led = pin;
            _reverseState = reverseState;
            WriteLedValue(PinValue.Low);
        }

        public void Blink(int intervalMs = 500)
        {
            if (_blink && _intervalMs == intervalMs) return;

            _intervalMs = intervalMs;
            _blink = true;

            _blinkThread = new Thread(BlinkLoop);
            _blinkThread.Start();
        }

        public void SetState(PinValue value)
        {
            _blink = false;

            if (_blinkThread != null && _blinkThread.IsAlive)
            {
                _blinkThread.Join();
                _blinkThread = null;
            }

            WriteLedValue(value);
        }

        private void WriteLedValue(PinValue value)
        {
            if (_reverseState)
            {
                _led.Write(value == PinValue.High ? PinValue.Low : PinValue.High);
            }
            else
            {
                _led.Write(value);
            }
        }

        private void BlinkLoop()
        {
            while (_blink)
            {
                _led.Write(PinValue.Low);
                Thread.Sleep(_intervalMs);
                _led.Write(PinValue.High);
                Thread.Sleep(_intervalMs);
            }
        }
    }
}
