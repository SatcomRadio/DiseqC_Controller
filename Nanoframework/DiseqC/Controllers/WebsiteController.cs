using nanoFramework.WebServer;

namespace DiseqC.Controllers
{
    internal class WebsiteController
    {
        [Route("favicon.ico")]
        public void Favicon(WebServerEventArgs e)
        {
            WebServer.SendFileOverHTTP(e.Context.Response, "favicon.ico", Resources.GetBytes(Resources.BinaryResources.favicon), "image/ico");
        }

        [Route("script.js")]
        public void Script(WebServerEventArgs e)
        {
            e.Context.Response.ContentType = "text/javascript";
            WebServer.OutPutStream(e.Context.Response, Resources.GetString(Resources.StringResources.script));
        }

        //[Route("image.svg")]
        //public void Image(WebServerEventArgs e)
        //{
        //    WebServer.SendFileOverHTTP(e.Context.Response, "image.svg", Resources.GetBytes(Resources.BinaryResources.image), "image/svg+xml");
        //}

        [Route("default.html"), Route("index.html"), Route("/")]
        public void Default(WebServerEventArgs e)
        {
            e.Context.Response.ContentType = "text/html";
            WebServer.OutPutStream(e.Context.Response, Resources.GetString(Resources.StringResources.webpage));
        }
    }
}
