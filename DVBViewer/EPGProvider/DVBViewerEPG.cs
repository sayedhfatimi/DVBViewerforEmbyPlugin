using DVBViewer.GeneralHelpers;
using MediaBrowser.Controller.LiveTv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DVBViewer.Api;
using System.Xml;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.LiveTv;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using System.Globalization;
using System.IO;

namespace DVBViewer.EPGProvider
{
    public class DVBViewerEPG
    {
        private readonly ILogger _logger;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IHttpClient _httpClient;
        public static DVBViewerEPG Current;
        List<ProgramInfo> programsInfo;

        public DVBViewerEPG(ILogger logger, IJsonSerializer jsonSerializer, IHttpClient httpClient)
        {
            _logger = logger;
            _jsonSerializer = jsonSerializer;
            _httpClient = httpClient;
            Current = this;
        }

        private async Task<string> getepgID(string channelNumber)
        {
            try {
                XmlElement root = await DVBViewerAPI.getChannels();

                XmlNodeList channelNodes = root.SelectNodes("//channels/root/group/channel");

                string epgID;

                for (int i = 0; i < channelNodes.Count; i++)
                {
                    if (channelNodes[i].Attributes["nr"].Value.ToString() == channelNumber) {
                        return epgID = channelNodes[i].Attributes["EPGID"].Value.ToString();
                    }
                }
            }
            catch
            {
                _logger.Error("Can't Retrieve Channel List for EPG");
            }
            return null;
        }

        public async Task<IEnumerable<ProgramInfo>> getEPGList(string channelNumber)
        {
            try {
                XmlElement epgData = await DVBViewerAPI.getEPGData(await getepgID(channelNumber));

                if (epgData == null) { _logger.Error("No EPG Data"); }
                else
                {
                    XmlNodeList epgList = epgData.SelectNodes("//programme");

                    programsInfo = new List<ProgramInfo>();

                    var items = new List<ProgramInfo>();

                    for (int i = 0; i < epgList.Count; i++)
                    {
                        var item = new ProgramInfo
                        {
                            ChannelId = channelNumber,
                            Id = epgData.GetElementsByTagName("eventid").Item(i).Value.ToString(),
                            Name = epgData.GetElementsByTagName("title").Item(i).Value.ToString(),
                            Overview = epgData.GetElementsByTagName("event").Item(i).Value.ToString(),
                            StartDate = Helpers.convertToDateTime(epgList[i].Attributes["start"].Value.ToString()),
                            EndDate = Helpers.convertToDateTime(epgList[i].Attributes["stop"].Value.ToString())
                        };
                        items.Add(item);
                    }

                    programsInfo = items.ToList();

                    return programsInfo;
                }
            }
            catch(Exception ex)
            {
                _logger.Error("Can't Retrieve EPG List from Server", ex);
            }
            return null;
        }
    }
}
