using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Net;
using MediaBrowser.Model.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DVBViewer.EPGProvider;
using DVBViewer.TunerHost;
using DVBViewer.Configuration;
using DVBViewer.GeneralHelpers;
using MediaBrowser.Model.LiveTv;

namespace DVBViewer
{
    public class LiveTvService : ILiveTvService
    {
        private List<ITunerHost> _tunerServer;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly DVBViewerEPG _dvbEpg;
        private readonly IHttpClient _httpClient;
        private readonly IXmlSerializer _xmlSerializer;
        private Dictionary<string, MediaSourceInfo> streams;
        private readonly IApplicationPaths _appPaths;
        private readonly ILogger _logger;

        public LiveTvService(IHttpClient httpClient, IJsonSerializer jsonSerializer, ILogManager logManager, IXmlSerializer xmlSerializer, IApplicationPaths appPaths)
        {
            _httpClient = httpClient;
            _jsonSerializer = jsonSerializer;
            streams = new Dictionary<string, MediaSourceInfo>();
            _xmlSerializer = xmlSerializer;
            _appPaths = appPaths;
            _logger = logManager.GetLogger(Name);
            _logger.Info("Directory is: " + DataPath);
            _dvbEpg = new DVBViewerEPG();

            RefreshConfigData(false, CancellationToken.None);
            Plugin.Instance.ConfigurationUpdated += (sender, args) => RefreshConfigData(true, CancellationToken.None);
        }

        public async void RefreshConfigData(bool isConfigChange, CancellationToken cancellationToken)
        {
            var config = Plugin.Instance.Configuration;
            if (config.TunerHostsConfiguration != null)
            {
                _tunerServer = TunerHostFactory.CreateTunerHosts(config.TunerHostsConfiguration, _logger, _jsonSerializer, _httpClient);
                for (var i = 0; i < _tunerServer.Count(); i++)
                {
                    await _tunerServer[i].GetDeviceInfo(cancellationToken);
                    config.TunerHostsConfiguration[i].ServerId = _tunerServer[i].HostId;
                }
            }
            Plugin.Instance.SaveConfiguration();
        }

        public string DataPath
        {
            get { return Plugin.Instance.DataFolderPath; }
        }

        public async Task<IEnumerable<ChannelInfo>> GetChannelsAsync(CancellationToken cancellationToken)
        {
            List<ChannelInfo> channels = new List<ChannelInfo>();
            await EnsureConnectionAsync(cancellationToken).ConfigureAwait(false);
            foreach (var host in _tunerServer)
            {
                channels.AddRange(await host.GetChannels(cancellationToken));
            }
            channels = channels.GroupBy(x => x.Id).Select(x => x.First()).ToList();
            return channels;
        }

        public Task<MediaSourceInfo> GetChannelStream(string channelId, string streamId, CancellationToken cancellationToken)
        {
            MediaSourceInfo mediaSourceInfo = null;

            foreach (var host in _tunerServer)
            {
                try
                {
                    mediaSourceInfo = host.GetChannelStreamInfo(channelId);
                    break;
                }
                catch (ApplicationException e)
                {
                    _logger.Info(e.Message);
                }
            }
            if ((mediaSourceInfo == null)) { throw new ApplicationException("No tuners Avaliable"); }
            mediaSourceInfo.Id = Guid.NewGuid().ToString("N");
            streams.Add(mediaSourceInfo.Id, mediaSourceInfo);
            return Task.FromResult(mediaSourceInfo);
        }

        private async Task EnsureConnectionAsync(CancellationToken cancellationToken)
        {
            foreach (var host in _tunerServer)
            {
                try
                {
                    if (string.IsNullOrEmpty(host.getWebUrl()))
                    {
                        throw new ApplicationException("Tuner hostname/ip missing.");
                    }
                    await host.GetDeviceInfo(cancellationToken);
                    host.Enabled = true;
                }
                catch (HttpException)
                {
                    host.Enabled = false;
                }
                catch (ApplicationException)
                {
                    host.Enabled = false;
                }
            }

        }

        public Task CloseLiveStream(string id, CancellationToken cancellationToken)
        {
            streams.Remove(id);
            return Task.FromResult(0);
        }

        public async Task<IEnumerable<ProgramInfo>> GetProgramsAsync(string channelId, DateTime startDateUtc, DateTime endDateUtc, CancellationToken cancellationToken)
        {
            var epgData = await _dvbEpg.getTvGuideForChannel(channelId, startDateUtc, endDateUtc, cancellationToken);

            var programInfos = epgData as IList<ProgramInfo> ?? epgData.ToList();

            if (!programInfos.Any())
            {
                epgData = GetEpgDataForChannel(channelId);
            }
            else
            {
                SaveEpgDataForChannel(channelId, programInfos);
            }
            return epgData;
        }

        public async Task<LiveTvServiceStatusInfo> GetStatusInfoAsync(CancellationToken cancellationToken)
        {
            await EnsureConnectionAsync(cancellationToken).ConfigureAwait(false);
            bool upgradeAvailable;
            string serverVersion;
            upgradeAvailable = false;
            serverVersion = Plugin.Instance.Version.ToString();
            List<LiveTvTunerInfo> tvTunerInfos = new List<LiveTvTunerInfo>();
            foreach (var host in _tunerServer)
            {
                tvTunerInfos.AddRange(await host.GetTunersInfo(cancellationToken));
            }
            return new LiveTvServiceStatusInfo
            {
                HasUpdateAvailable = upgradeAvailable,
                Version = serverVersion,
                Tuners = tvTunerInfos
            };
        }

        public string HomePageUrl
        {
            get { return "http://emby.media"; }
        }

        public string Name
        {
            get { return "DVBViewer"; }
        }

        public Task ResetTuner(string id, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }

        public void GetFileCopy<T>(ref T obj, string filename)
        {
            var path = DataPath + @"\" + filename;
            if (File.Exists(path))
            {
                obj = (T)_xmlSerializer.DeserializeFromFile(typeof(T), path);
            }
            else
            {
                obj = (T)Activator.CreateInstance<T>();
            }
        }

        private void SaveEpgDataForChannel(string channelId, IEnumerable<ProgramInfo> epgData)
        {
            CreateFileCopy(epgData, DataPath + @"\EPG\" + channelId + ".xml");
        }

        private List<ProgramInfo> GetEpgDataForChannel(string channelId)
        {
            List<ProgramInfo> channelEpg = new List<ProgramInfo>();
            GetFileCopy<List<ProgramInfo>>(ref channelEpg, @"EPG\" + channelId + ".xml");
            return channelEpg;
        }

        public void CreateFileCopy(object obj, string filePath)
        {
            GeneralHelpers.Helpers.CreateFileCopy(obj, filePath, _xmlSerializer);
        }

        private List<ProgramInfo> GetEpgDataForAllChannels()
        {
            List<ProgramInfo> channelEpg = new List<ProgramInfo>();
            DirectoryInfo dir = new DirectoryInfo(DataPath + @"\EPG\");
            List<FileInfo> Files = dir.GetFiles("*.xml").ToList();
            List<string> channels = Files.Select(f => f.Name).ToList();
            foreach (var channel in channels)
            {
                channelEpg.AddRange(GetEpgDataForChannel(channel));
            }
            return channelEpg;
        }

        private ProgramInfo GetProgramInfoFromCache(string channelId, string programId)
        {
            var epgData = GetEpgDataForChannel(channelId);
            if (epgData.Any())
            {
                return epgData.FirstOrDefault(p => p.Id == programId);
            }
            return null;
        }

        public Task<IEnumerable<TimerInfo>> GetTimersAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<RecordingInfo>> GetRecordingsAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<SeriesTimerInfo>> GetSeriesTimersAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task RecordLiveStream(string id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task CancelSeriesTimerAsync(string timerId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task CancelTimerAsync(string timerId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task CreateTimerAsync(TimerInfo info, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task CreateSeriesTimerAsync(SeriesTimerInfo info, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<ImageStream> GetChannelImageAsync(string channelId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<List<MediaSourceInfo>> GetChannelStreamMediaSources(string channelId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task DeleteRecordingAsync(string recordingId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<SeriesTimerInfo> GetNewTimerDefaultsAsync(CancellationToken cancellationToken, ProgramInfo program = null)
        {
            throw new NotImplementedException();
        }

        public Task<ImageStream> GetProgramImageAsync(string programId, string channelId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<ImageStream> GetRecordingImageAsync(string recordingId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<MediaSourceInfo> GetRecordingStream(string recordingId, string streamId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<List<MediaSourceInfo>> GetRecordingStreamMediaSources(string recordingId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task UpdateSeriesTimerAsync(SeriesTimerInfo info, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task UpdateTimerAsync(TimerInfo info, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public event EventHandler DataSourceChanged;
        public event EventHandler<RecordingStatusChangedEventArgs> RecordingStatusChanged;
    }
}