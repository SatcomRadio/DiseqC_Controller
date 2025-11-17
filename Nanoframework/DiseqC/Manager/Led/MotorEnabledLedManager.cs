using System.Device.Gpio;

namespace DiseqC.Manager.Led
{
    internal class MotorEnabledLedManager : LedManager
    {
        public const int PinNumber = 8;

        public MotorEnabledLedManager(GpioController gpioController) : base(gpioController.OpenPin(PinNumber, PinMode.Output), true)
        {
            
        }
    }
}
