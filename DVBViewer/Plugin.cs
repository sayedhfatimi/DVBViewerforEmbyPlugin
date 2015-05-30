using System;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Serialization;
using DVBViewer.Configuration;
using MediaBrowser.Model.Plugins;

namespace DVBViewer
{
    public class Plugin : BasePlugin<PluginConfiguration>
    {
        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer) : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
        }

        public override string Name
        {
            get { return "DVBViewer"; }
        }

        public override string Description
        {
            get
            {
                return "Watch Live TV using the DVBViewer Backend.";
            }
        }

        public static Plugin Instance { get; private set; }

        public event EventHandler ConfigurationUpdated;
        public delegate void EventHandler(Plugin plugin, EventArgs e);

        public override void UpdateConfiguration(BasePluginConfiguration configuration)
        {
            base.UpdateConfiguration(configuration);
            if (ConfigurationUpdated != null) { ConfigurationUpdated(this, EventArgs.Empty); }
        }
    }
}