using DVBViewer.GeneralHelpers;
using MediaBrowser.Controller.LiveTv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DVBViewer.Api;
using System.Xml;

namespace DVBViewer.EPGProvider
{
    class DVBViewerEPG : IEpgSupplier
    {
        private async Task<string> getepgID(string channelNumber)
        {
            XmlElement root = await DVBViewerAPI.getChannels();

            XmlNodeList channelNodes = root.SelectNodes("//channels/root/group/channel");

            string epgID;

            for (int i = 0; i < channelNodes.Count; i++)
            {
                if (channelNodes[i].Attributes["nr"].Value.ToString() == channelNumber) {
                    return epgID = channelNodes[i].Attributes["EPGID"].Value.ToString();
                }
            }

            return null;
        }

        public async Task<IEnumerable<ProgramInfo>> getTvGuideForChannel(string channelNumber, DateTime start,
            DateTime end, CancellationToken cancellationToken)
        {
            List<ProgramInfo> programsInfo = new List<ProgramInfo>();

            XmlElement epgData = await DVBViewerAPI.getEPGData(await getepgID(channelNumber));

            XmlNodeList epgList = epgData.SelectNodes("//programme");

            var items = new List<ProgramInfo>();
            for(int i = 0; i < epgList.Count; i++)
            {
                var info = new ProgramInfo
                {
                    ChannelId = channelNumber,
                    Id = epgData.GetElementsByTagName("eventid").Item(i).Value.ToString(),
                    Name = epgData.GetElementsByTagName("title").Item(i).Value.ToString(),
                    Overview = epgData.GetElementsByTagName("event").Item(i).Value.ToString(),
                    StartDate = Helpers.convertToDateTime(epgList[i].Attributes["start"].Value.ToString()),
                    EndDate = Helpers.convertToDateTime(epgList[i].Attributes["stop"].Value.ToString())
                };
                items.Add(info);
            }

            programsInfo = items.ToList();

            return programsInfo;
        }
    }
}
