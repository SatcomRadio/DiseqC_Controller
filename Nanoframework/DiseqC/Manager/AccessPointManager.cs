using Iot.Device.DhcpServer;
using nanoFramework.Runtime.Native;
using DiseqC.Manager.Led;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using AuthenticationType = System.Net.NetworkInformation.AuthenticationType;

namespace DiseqC.Manager
{
    internal class AccessPointManager
    {
        private readonly StatusLedManager _statusLed;

        public AccessPointManager(StatusLedManager statusLed)
        {
            _statusLed = statusLed;
        }

        public static string ApIp = "192.168.5.1";
        public static DhcpServer DhcpServer = new()
        {
            CaptivePortalUrl = $"http://{ApIp}"
        };

        public void DisableAccessPoint()
        {
            var apConf = GetConfiguration();
            apConf.Options = WirelessAPConfiguration.ConfigurationOptions.None;
            apConf.SaveConfiguration();
            Power.RebootDevice();
        }

        public bool SetupAccessPoint()
        {
            Debug.WriteLine("Setting up AP mode...");

            try
            {
                _statusLed.Blink();
                if (!IsApEnabled())
                {
                    var networkIfc = WirelessHelper.GetInterface(NetworkInterfaceType.WirelessAP);
                    networkIfc.EnableStaticIPv4(ApIp, "255.255.255.0", ApIp);

                    var apConfig = new WirelessAPConfiguration(0)
                    {
                        Options = WirelessAPConfiguration.ConfigurationOptions.Enable,
                        Ssid = $"DISEQc Controller {networkIfc.PhysicalAddress[networkIfc.PhysicalAddress.Length-1]:X2}",
                        Password = "",
                        Channel = 6,
                        MaxConnections = 4,
                        Authentication = AuthenticationType.Open,
                        Encryption = EncryptionType.None,
                        Radio = RadioType._802_11g,
                        
                    };
                    apConfig.SaveConfiguration();
                    Debug.WriteLine("AP configuration saved successfully. AP mode is now active. Rebooting");
                    Power.RebootDevice();
                    return true;
                }

                SetupDhcp();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving AP configuration: {ex.Message}");
            }

            return false;
        }

        private static void SetupDhcp()
        {
            var dhcpInitResult = DhcpServer.Start(IPAddress.Parse(ApIp), new IPAddress(new byte[] { 255, 255, 255, 0 }));
            if (!dhcpInitResult)
            {
                Console.WriteLine("Error initializing DHCP server.");
                Power.RebootDevice();
            }
        }

        private static bool IsApEnabled()
        {
            var ni = WirelessHelper.GetInterface(NetworkInterfaceType.WirelessAP);
            var wirelessConf = GetConfiguration();

            if (ni.IPv4Address == ApIp &&
                (wirelessConf.Options == WirelessAPConfiguration.ConfigurationOptions.Enable ||
                 wirelessConf.Options == WirelessAPConfiguration.ConfigurationOptions.AutoStart))
            {
                return true;
            }

            return false;
        }

        private static WirelessAPConfiguration GetConfiguration()
        {
            var ni = WirelessHelper.GetInterface(NetworkInterfaceType.WirelessAP);
            return WirelessAPConfiguration.GetAllWirelessAPConfigurations()[ni.SpecificConfigId];
        }
    }
}