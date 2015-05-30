using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Plugins;

namespace DVBViewer.Configuration
{
    class DVBViewerConfigurationPage : IPluginConfigurationPage
    {
        public ConfigurationPageType ConfigurationPageType
        {
            get { return ConfigurationPageType.PluginConfiguration; }
        }

        public System.IO.Stream GetHtmlStream()
        {
            return GetType().Assembly.GetManifestResourceStream("DVBViewer.Configuration.configPage.html");
        }

        public string Name
        {
            get { return "DVBViewer"; }
        }

        public IPlugin Plugin
        {
            get { return DVBViewer.Plugin.Instance; }
        }
    }
}