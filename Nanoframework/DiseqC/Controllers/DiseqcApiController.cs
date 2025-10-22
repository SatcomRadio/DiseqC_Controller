using nanoFramework.WebServer;
using System.Diagnostics;
using DiseqC.Manager;

namespace DiseqC.Controllers
{
    internal class DiseqcApiController
    {
        private readonly RotorManager _rotorMgr;
        private readonly RotorManager _rmtRotorMgr;
        public DiseqcApiController(RotorManager rotorMgr, RotorManager rmtRotorMgr)
        {
            _rotorMgr = rotorMgr;
            _rmtRotorMgr = rmtRotorMgr;
        }

        [Route("angle")]
        [Method("POST")]
        public void SetAzimuth(WebServerEventArgs e)
        {
            var parameters = ControllerHelper.ParseParamsFromStream(e.Context.Request.InputStream);
            if (float.TryParse(parameters["angle"].ToString(), out var angle))
            {
                Debug.WriteLine($"Requested angle: {angle}");
                //_rotorMgr.GotoAngle(angle, 5);
                _rmtRotorMgr.GotoAngle(angle, 5);
            }
            else
            {
                e.Context.Response.ContentType = "text/plain";
                WebServer.OutPutStream(e.Context.Response, "Invalid angle");
            }

            WebServer.OutPutStream(e.Context.Response, Resources.GetString(Resources.StringResources.webpage));
        }
    }
}
