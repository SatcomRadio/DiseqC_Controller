using System.Device.Gpio;

namespace DiseqC.Manager.Led
{
    internal class StatusLedManager : LedManager
    {
        public const int PinNumber = 3;

        public StatusLedManager(GpioController gpioController) : base(gpioController.OpenPin(PinNumber, PinMode.Output))
        {
            
        }
    }
}
