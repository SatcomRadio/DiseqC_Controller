using DiseqC.Controllers;
using DiseqC.Manager;
using DiseqC.Manager.Led;
using Microsoft.Extensions.DependencyInjection;
using nanoFramework.WebServer;
using System;
using System.Device.Gpio;
using System.Threading;

namespace DiseqC
{
    public class Program
    {
        private static ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton(typeof(GpioController))
                .AddSingleton(typeof(StatusLedManager))
                .AddSingleton(typeof(MotorLedManager))
                .AddSingleton(typeof(MotorEnabledLedManager))
                .AddSingleton(typeof(AccessPointManager))
                .AddSingleton(typeof(WiFiConnectionManager))
                .AddSingleton(typeof(SocketMotorManager))
                .AddSingleton(typeof(RotorManager))
                .AddSingleton(typeof(ButtonManager))
                .AddSingleton(typeof(MotorEnablerManager))
                .AddSingleton(typeof(DiseqcApiController))
                .AddSingleton(typeof(WifiSetupController))
                .AddSingleton(typeof(WebsiteController))
                .BuildServiceProvider();
        }

        public static void Test()
        {
            var services = ConfigureServices();
            var mgr = (RotorManager)services.GetRequiredService(typeof(RotorManager));
            var ledMot = (MotorLedManager)services.GetRequiredService(typeof(MotorLedManager));
            var ledStatus = (StatusLedManager)services.GetRequiredService(typeof(StatusLedManager));

            ledMot.SetState(PinValue.High);
            ledStatus.SetState(PinValue.High);

            while (true)
            {
                mgr.GotoAngle(10, 5);
                Thread.Sleep(7000);
                mgr.GotoAngle(0, 5);
                Thread.Sleep(7000);
            }

        }

        public static void Main()
        {
            //Test();
            var services = ConfigureServices();

            var btnMgr = (ButtonManager)services.GetRequiredService(typeof(ButtonManager));
            btnMgr.MonitorButtonPresses();

            var connectionMgr = (WiFiConnectionManager)services.GetRequiredService(typeof(WiFiConnectionManager));
            connectionMgr.ConnectOrStartAccessPoint(TimeSpan.FromSeconds(30));

            var socketMgr = (SocketMotorManager)services.GetRequiredService(typeof(SocketMotorManager));
            socketMgr.Start();

            using var webServer = new DiseqcWebServer(80, HttpProtocol.Http, new[] { typeof(DiseqcApiController), typeof(WifiSetupController), typeof(WebsiteController) }, services);
            webServer.Start();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}