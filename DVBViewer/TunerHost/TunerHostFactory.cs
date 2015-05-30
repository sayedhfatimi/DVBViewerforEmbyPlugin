using System;
using System.Collections.Generic;
using System.Linq;
using DVBViewer.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;


namespace DVBViewer.TunerHost
{
    public static class TunerHostFactory
    {
        public static ITunerHost CreateTunerHost(TunerUserConfiguration tunerUserConfiguration, ILogger logger, IJsonSerializer jsonSerializer, IHttpClient httpClient)
        {
            ITunerHost tunerHost;
            IEnumerable<Type> hostTypes = TunerHostStatics.GetAllTunerHostTypes();
            Type hostType = hostTypes.FirstOrDefault(t => String.Equals(t.Name, tunerUserConfiguration.ServerType,
                   StringComparison.OrdinalIgnoreCase));
            tunerHost = (ITunerHost)Activator.CreateInstance(hostType, logger, jsonSerializer, httpClient);
            foreach (var field in tunerUserConfiguration.UserFields)
            {
                tunerHost.GetType().GetProperty(field.Name).SetValue(tunerHost, field.Value, null);
            }
            return tunerHost;
        }

        public static ITunerHost CreateTunerHost(Type type)
        {
            ITunerHost tunerHost;
            tunerHost = (ITunerHost)Activator.CreateInstance(type);
            return tunerHost;
        }

        public static List<ITunerHost> CreateTunerHosts(List<TunerUserConfiguration> tunerUserConfigurations, ILogger logger, IJsonSerializer jsonSerializer, IHttpClient httpClient)
        {
            List<ITunerHost> tunerHosts = new List<ITunerHost>();
            foreach (TunerUserConfiguration config in tunerUserConfigurations)
            {
                tunerHosts.Add(CreateTunerHost(config, logger, jsonSerializer, httpClient));
            }
            return tunerHosts;
        }
    }
}