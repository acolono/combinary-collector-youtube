using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using YoutubeCollector.Db;
using YoutubeCollector.Lib;
using YoutubeCollector.Models;

namespace YoutubeCollector.collectors {
    public class ChannelCollector : CollectorBase {
        private readonly SettingsProvider _settingsProvider;
        private readonly Repository _repository;
        private readonly ILogger<ChannelCollector> _logger;
        private CancellationToken _ct;

        public ChannelCollector(SettingsProvider settingsProvider, Repository repository, ILogger<ChannelCollector> logger) {
            _settingsProvider = settingsProvider;
            _repository = repository;
            _logger = logger;
        }
        public override async Task ExecuteAsync(CancellationToken ct) {
            _ct = ct;
            await CollectFromChannels();
        }

        private async Task CollectFromChannels() {
            var keys = _settingsProvider.ApiKeys;
            var channelIds = _settingsProvider.ChannelIds;
            var sem = new SemaphoreSlim(_settingsProvider.Parallelism);

            var videoIdTasks = channelIds.Select(id => GetVideoIdFromChannel(id, keys.Next(), sem)).ToList();
            var videoIds = new HashSet<string>();
            foreach (var videoIdTask in videoIdTasks) {
                videoIds.AddRange(await videoIdTask);
            }

            var oldVideoIds = _repository.GetAllVideoIds();
            videoIds.AddRange(oldVideoIds);

            var videoIdsPartitions = videoIds.Partition(_settingsProvider.Parallelism);
            var videoCountDown = new SyncCounter(videoIds.Count);
            var videoTasks = videoIdsPartitions.Select(part => GetVideoFromId(part, keys.Next(), videoCountDown, _ct)).ToList();

            while (!_ct.IsCancellationRequested) {
                var runningTasks = videoTasks.Count(t => !t.IsCompleted);
                _logger.LogTrace($"videos left: {videoCountDown.Read()}, running tasks: {runningTasks}");

                if(runningTasks<=0) break;
                await videoTasks.WaitOneOrTimeout(4000);
            }

            var videos = new List<Video>();
            foreach (var videoTask in videoTasks) {
                videos.AddRange(await videoTask);
            }

            var updates = await _repository.SaveOrUpdate(videos);
            _logger.LogDebug($"video db updates: {updates}");
        }

        private async Task<List<string>> GetVideoIdFromChannel(string channelId, string apiKey, SemaphoreSlim sem) {
            await sem.WaitAsync(_ct);
            try {
                using (var api = new YoutubeApi(apiKey)) {
                    var ytVids = await api.GetAllVideosFromChannel(channelId, _ct);
                    _logger.LogDebug($"found {ytVids.PageInfo.TotalResults} VideoIds for channel {channelId}");
                    return ytVids.Items.Select(v => v.Id.VideoId).Where(i => !string.IsNullOrWhiteSpace(i)).ToList();
                }
            }
            finally {
                sem.Release();
            }
        }
    }
}
