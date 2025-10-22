using DiseqC.Manager;
using nanoFramework.Runtime.Native;
using nanoFramework.WebServer;
using System.Diagnostics;

namespace DiseqC.Controllers
{
    internal class WifiSetupController
    {
        private readonly AccessPointManager _apMgr;
        public WifiSetupController(AccessPointManager apMgr) => _apMgr = apMgr;

        [Route("wifi")]
        [Method("POST")]
        public void SetWifiParameters(WebServerEventArgs e)
        {
            var hashPars = ControllerHelper.ParseParamsFromStream(e.Context.Request.InputStream);
            var ssid = (string)hashPars["ssid"];
            var password = (string)hashPars["password"];

            Debug.WriteLine($"Wireless parameters SSID:{ssid} PASSWORD:{password}");

            e.Context.Response.ContentType = "text/plain";
            WebServer.OutPutStream(e.Context.Response, "New settings saved. Rebooting device");

            WiFiConnectionManager.SaveConfiguration(ssid, password);

            if (_apMgr.IsApModeEnabled())
                _apMgr.DisableAccessPoint(true);
            else
                Power.RebootDevice();
        }
    }
}
