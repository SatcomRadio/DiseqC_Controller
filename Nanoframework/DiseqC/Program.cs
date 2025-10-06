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
                .AddSingleton(typeof(AccessPointManager))
                .AddSingleton(typeof(WiFiConnectionManager))
                .AddSingleton(typeof(RotorManager))
                .AddSingleton(typeof(RmtRotorManager))
                .AddSingleton(typeof(DiseqcApiController))
                .AddSingleton(typeof(WifiSetupController))
                .AddSingleton(typeof(WebsiteController))
                .BuildServiceProvider();
        }

        public static void Test()
        {

            var services = ConfigureServices();
            var mgr = (RmtRotorManager)services.GetRequiredService(typeof(RmtRotorManager));

            while (true)
            {

                mgr.GotoAngle(10, 5);
                Thread.Sleep(5000);
                mgr.GotoAngle(0, 5);
                Thread.Sleep(5000);
            }

        }

        public static void Main()
        {
            //Test();
            var services = ConfigureServices();

            var connectionMgr = (WiFiConnectionManager)services.GetRequiredService(typeof(WiFiConnectionManager));
            connectionMgr.ConnectOrStartAccessPoint();

            using var webServer = new DiseqcWebServer(80, HttpProtocol.Http, new Type[] { typeof(DiseqcApiController), typeof(WifiSetupController), typeof(WebsiteController) }, services);
            webServer.Start();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}