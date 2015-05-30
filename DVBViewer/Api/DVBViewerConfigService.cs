using DVBViewer.EPGProvider;
using MediaBrowser.Controller.Net;
using ServiceStack;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DVBViewer.Configuration;
using DVBViewer.TunerHost;

namespace DVBViewer.Api
{
    [Route("/DVBViewer/Tuner/ConfigurationFields", "GET")]
    public class GetTunerConfigurationFields : IReturn<ConfigurationFieldsDefaults>
    {
    }

    public class DVBViewerConfigService : IRestfulService
    {
        public object Get(GetTunerConfigurationFields request)
        {
            return new ConfigurationFieldsDefaults { DefaultsBuilders = TunerHostStatics.BuildDefaultForTunerHostsBuilders() };
        }
    }

    public class ConfigurationFieldsDefaults
    {
        public List<FieldBuilder> DefaultsBuilders { get; set; }
    }
}