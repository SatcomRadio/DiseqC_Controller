using nanoFramework.WebServer;
using DiseqC.Manager;
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
            WebServer.OutPutStream(e.Context.Response, "New settings saved. Rebooting device to put into normal mode");

            WiFiConnectionManager.SaveConfiguration(ssid, password);

            _apMgr.DisableAccessPoint();
        }
    }
}
