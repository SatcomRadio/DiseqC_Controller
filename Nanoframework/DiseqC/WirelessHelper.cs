using System.Collections;
using System.IO;
using System.Net.NetworkInformation;
using System.Text;
using System.Web;

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
