using nanoFramework.Networking;
using DiseqC.Manager.Led;
using System;
using System.Device.Wifi;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;

namespace DiseqC.Manager
{
    internal class WiFiConnectionManager
    {
        private readonly StatusLedManager _statusLed;

        public WiFiConnectionManager(StatusLedManager statusLed)
        {
            _statusLed = statusLed;
        }

        public bool TryConnect(int timeoutMilliseconds)
        {
            _statusLed.Blink(50);
            using var cs = new CancellationTokenSource(timeoutMilliseconds);
            try
            {
                var configurations = Wireless80211Configuration.GetAllWireless80211Configurations();
                if (configurations.Length <= 0)
                {
                    Debug.WriteLine($"No stored wifi configurations");
                    return false;
                }

                var connectOk = Configure(configurations[0].Ssid, configurations[0].Password, cs.Token);
                if (connectOk) _statusLed.SetState(true);
                
                return connectOk;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error connecting to WiFi: {ex.Message}");
                return false;
            }
        }

        public static bool SaveConfiguration(string ssid, string password)
        {
            try
            {
                WifiNetworkHelper.Disconnect();
                var wifiConfig = new Wireless80211Configuration(0)
                {
                    Ssid = ssid,
                    Password = password,
                    Authentication = AuthenticationType.WPA2,
                    Encryption = EncryptionType.WPA2
                };
                wifiConfig.SaveConfiguration();
                Debug.WriteLine("WiFi configuration saved successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving WiFi configuration: {ex.Message}");
                return false;
            }
        }

        private static Wireless80211Configuration GetConfiguration()
        {
            var ni = WirelessHelper.GetInterface(NetworkInterfaceType.Wireless80211);
            return Wireless80211Configuration.GetAllWireless80211Configurations()[ni.SpecificConfigId];
        }

        private static bool Configure(string ssid, string password, CancellationToken token)
        {
            // Make sure we are disconnected before we start connecting otherwise
            // ConnectDhcp will just return success instead of reconnecting.
            var wa = WifiAdapter.FindAllAdapters()[0];
            wa.Disconnect();

            Console.WriteLine("ConnectDHCP");
            WifiNetworkHelper.Disconnect();

            // Reconfigure properly the normal wifi
            var wConf = GetConfiguration();
            wConf.Options = Wireless80211Configuration.ConfigurationOptions.AutoConnect | Wireless80211Configuration.ConfigurationOptions.Enable;
            wConf.Ssid = ssid;
            wConf.Password = password;
            wConf.SaveConfiguration();

            WifiNetworkHelper.Disconnect();

            var success = WifiNetworkHelper.ConnectDhcp(ssid, password, WifiReconnectionKind.Automatic, true, token: token);
            if (!success)
            {
                wa.Disconnect();
                // Bug in network helper, we've most likely try to connect before, let's make it manual
                var res = wa.Connect(ssid, WifiReconnectionKind.Automatic, password);
                success = res.ConnectionStatus == WifiConnectionStatus.Success;
                Console.WriteLine($"Connected: {res.ConnectionStatus}");
            }

            return success;
        }
    }
}