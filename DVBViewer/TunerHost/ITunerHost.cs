using MediaBrowser.Controller.LiveTv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DVBViewer.Configuration;
using DVBViewer.GeneralHelpers;
using MediaBrowser.Model.Dto;
using DVBViewer.Api;

namespace DVBViewer.TunerHost
{
    public interface ITunerHost
    {
        Task GetDeviceInfo(CancellationToken cancellationToken);
        string HostId { get; set; }
        bool Enabled { get; set; }
        Task<IEnumerable<ChannelInfo>> GetChannels(CancellationToken cancellationToken);
        Task<List<LiveTvTunerInfo>> GetTunersInfo(CancellationToken cancellationToken);
        IEnumerable<ConfigurationField> GetFieldBuilder();
        string getWebUrl();
        MediaSourceInfo GetChannelStreamInfo(string channelId);
    }


    public static class TunerHostStatics
    {
        public static IEnumerable<Type> GetAllTunerHostTypes()
        {
            return Helpers.GetTypesInNamespace(Assembly.GetExecutingAssembly(), "DVBViewer.TunerHost.HostDefinitions");
        }
        public static List<FieldBuilder> BuildDefaultForTunerHostsBuilders()
        {
            List<FieldBuilder> defaultTunerHostsConfigFields = new List<FieldBuilder>();
            foreach (Type tunerHostType in GetAllTunerHostTypes())
            {
                var tunerHost = TunerHostFactory.CreateTunerHost(tunerHostType);
                FieldBuilder fieldBuilder = new FieldBuilder()
                {
                    Type = tunerHostType.Name,
                    DefaultConfigurationFields = tunerHost.GetFieldBuilder().ToArray()
                };
                defaultTunerHostsConfigFields.Add(fieldBuilder);
            }
            return defaultTunerHostsConfigFields;
        }
    }

}