using System.Device.Gpio;

namespace DiseqC.Manager.Led
{
    internal class MotorLedManager : LedManager
    {
        public const int PinNumber = 9;
        public MotorLedManager(GpioController gpioController) : base(gpioController.OpenPin(PinNumber, PinMode.Output))
        {
            
        }
    }
}
