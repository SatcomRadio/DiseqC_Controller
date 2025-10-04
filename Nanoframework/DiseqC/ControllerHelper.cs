using System.Collections;
using System.IO;
using System.Text;
using System.Web;

namespace DiseqC
{
    public class ControllerHelper
    {
        public static Hashtable ParseParamsFromStream(Stream inputStream)
        {
            var buffer = new byte[inputStream.Length];
            inputStream.Read(buffer, 0, (int)inputStream.Length);

            return ParseParams(Encoding.UTF8.GetString(buffer, 0, buffer.Length));
        }

        public static Hashtable ParseParams(string rawParams)
        {
            var hash = new Hashtable();

            var parPairs = rawParams.Split('&');
            foreach (var pair in parPairs)
            {
                var nameValue = pair.Split('=');
                hash.Add(nameValue[0], HttpUtility.UrlDecode(nameValue[1]));
            }

            return hash;
        }
    }
}
