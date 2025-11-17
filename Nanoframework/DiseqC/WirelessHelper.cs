using System.Net.NetworkInformation;

namespace DiseqC
{
    internal class WirelessHelper
    {
        public static NetworkInterface GetInterface(NetworkInterfaceType type)
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var ni in interfaces)
            {
                if (ni.NetworkInterfaceType == type)
                {
                    return ni;
                }
            }
            return null;
        }
    }
}
