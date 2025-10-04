using Microsoft.Extensions.DependencyInjection;
using nanoFramework.Networking;
using nanoFramework.WebServer;
using DiseqC.Controllers;
using DiseqC.Manager;
using DiseqC.Manager.Led;
using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.Net.NetworkInformation;
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

            Debug.WriteLine("Starting WiFiManager...");
            var wifiConnected = connectionMgr.TryConnect(100000);
            if (wifiConnected)
            {
                Debug.WriteLine("WiFi connected successfully.");
                var ni = NetworkInterface.GetAllNetworkInterfaces()[0];
                Debug.WriteLine($"IP address: {ni.IPv4Address}");
            }
            else
            {
                Debug.WriteLine("WiFi connection failed.");
                if (WifiNetworkHelper.HelperException != null)
                {
                    Debug.WriteLine($"Error: {WifiNetworkHelper.HelperException.Message}");
                }

                var apMgr = (AccessPointManager)services.GetRequiredService(typeof(AccessPointManager));
                apMgr.SetupAccessPoint();
            }

            using var webServer = new DiseqcWebServer(80, HttpProtocol.Http, new Type[] { typeof(DiseqcApiController), typeof(WifiSetupController), typeof(WebsiteController) }, services);
            webServer.Start();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}