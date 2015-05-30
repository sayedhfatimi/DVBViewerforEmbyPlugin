using MediaBrowser.Common.Net;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Model.LiveTv;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DVBViewer.Configuration;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.MediaInfo;
using System.Xml;
using DVBViewer.Api;
using System;
using DVBViewer.GeneralHelpers;

namespace DVBViewer.TunerHost.HostDefinitions
{
    public class DVBViewer : ITunerHost
    {
        List<ChannelInfo> ChannelList;

        public bool Enabled { get; set; }
        public string model { get; set; }
        public string deviceID { get; set; }
        public string firmware { get; set; }
        public List<LiveTvTunerInfo> tuners;

        public string getWebUrl()
        {
            return "http://" + DVBViewerAPI.host + ":" + DVBViewerAPI.port;
        }

        public DVBViewer(ILogger logger, IJsonSerializer jsonSerializer, IHttpClient httpClient)
        {
            tuners = new List<LiveTvTunerInfo>();
        }

        public DVBViewer()
        {

        }

        public string HostId
        {
            get
            {
                var hostId = model + "-" + deviceID;
                if (hostId == "-")
                {
                    hostId = "";
                }
                return hostId;
            }
            set { }
        }

        public async Task GetDeviceInfo(CancellationToken cancellationToken)
        {
            model = "";
            deviceID = "";
            firmware = "";
            if (String.IsNullOrWhiteSpace(model))
            {
                throw new ApplicationException("Failed to locate the tuner host.");
            }
        }

        public async Task<List<LiveTvTunerInfo>> GetTunersInfo(CancellationToken cancellationToken)
        {
            tuners.Add(new LiveTvTunerInfo() { Name = "DVBViewer", SourceType = "UPnP", ProgramName = "", Status = LiveTvTunerStatus.Available });
            return tuners;
        }

        public async Task<IEnumerable<ChannelInfo>> GetChannels(CancellationToken cancellationToken)
        {
            ChannelList = new List<ChannelInfo>();

            XmlElement root = await DVBViewerAPI.getChannels();

            XmlNodeList channelNodes = root.SelectNodes("//channels/root/group/channel");

            var items = new List<ChannelInfo>();

            for (int i = 0; i < channelNodes.Count; i++)
            {
                var item = new ChannelInfo
                {
                    Name = channelNodes[i].Attributes["name"].Value,
                    ImageUrl = getWebUrl() + "/" + channelNodes[i].InnerText,
                    Number = channelNodes[i].Attributes["nr"].Value.ToString(),
                    Id = channelNodes[i].Attributes["nr"].Value.ToString()
                };

                items.Add(item);
            }

            ChannelList = items.ToList();

            return ChannelList;
        }

        public MediaSourceInfo GetChannelStreamInfo(string ChannelNumber)
        {
            return new MediaSourceInfo
            {
                Path = "http://localhost:7522/upnp/channelstream/" + ChannelNumber + ".ts",
                Protocol = MediaProtocol.Http,
                MediaStreams = new List<MediaStream>
                {
                    new MediaStream
                    {
                        Type = MediaStreamType.Video,
                        Index = -1,
                        IsInterlaced = true
                    },
                    new MediaStream
                    {
                        Type = MediaStreamType.Audio,
                        Index = -1
                    }
                }
            };
        }

        public void RefreshConfiguration()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ConfigurationField> GetFieldBuilder()
        {
            List<ConfigurationField> userFields = new List<ConfigurationField>()
            {
                new ConfigurationField()
                {
                    Name = "Url",
                    Type = FieldType.Text,
                    defaultValue = "localhost",
                    Description = "Hostname or IP address of the DVBViewer Recording Service",
                    Label = "Hostname/IP"
                },
                new ConfigurationField()
                {
                    Name = "Port",
                    Type = FieldType.Text,
                    defaultValue = "8089",
                    Description = "Port of the DVBViewer Recording Service",
                    Label = "Port"
                }
            };
            return userFields;
        }
    }
}