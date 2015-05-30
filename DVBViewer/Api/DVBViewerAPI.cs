using System.Threading.Tasks;
using System.Xml;
using System.Net;

namespace DVBViewer.Api
{
    public static class DVBViewerAPI
    {
        private static WebClient webClient = new WebClient();
        private static XmlDocument xmlDoc = new XmlDocument();

        public static string host { get; set; }

        public static string port { get; set; }

        public static async Task<XmlElement> getChannels()
        {
            string s = await webClient.DownloadStringTaskAsync("http://" + host + ":" + port + "/api/getchannelsxml.html?upnp=1&logo=1");

            s.Replace("&", "&amp;");

            xmlDoc.LoadXml(s);

            XmlElement Channels = xmlDoc.DocumentElement;

            return Channels;
        }

        public static async Task<XmlElement> getEPGData(string EPGID)
        {
            string s = await webClient.DownloadStringTaskAsync("http://" + host + ":" + port + "/api/epg.html?lvl=2&channel=" + EPGID);

            s.Remove(0, 41);

            xmlDoc.LoadXml(s);

            XmlElement EPGData = xmlDoc.DocumentElement;

            return EPGData;
        }

        public static async Task<string> getVersion()
        {
            string s = await webClient.DownloadStringTaskAsync("http://" + host + ":" + port + "/api/version.html");

            xmlDoc.LoadXml(s);

            XmlElement versionInfo = xmlDoc.DocumentElement;

            string dvbVersion = versionInfo.SelectSingleNode("//version").InnerText;

            return dvbVersion;
        }
    }
}
