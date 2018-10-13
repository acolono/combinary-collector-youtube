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
    public class VideoCollector : ICollector {
        private readonly SettingsProvider _settingsProvider;
        private readonly Repository _repository;
        private readonly ILogger<VideoCollector> _logger;
        private CancellationToken _ct;

        public VideoCollector(SettingsProvider settingsProvider, Repository repository, ILogger<VideoCollector> logger) {
            _settingsProvider = settingsProvider;
            _repository = repository;
            _logger = logger;
        }
        public async Task ExecuteAsync(CancellationToken ct) {
            _ct = ct;
            await CollectVideoDetails();
        }

        private async Task CollectVideoDetails() {
            var keys = _settingsProvider.ApiKeys;
            var channelIds = _settingsProvider.ChannelIds ?? new List<string>();
            var listIds = _settingsProvider.ListIds ?? new List<string>();

            var fromChannelsTask = GetVideoIdsFromChannels(channelIds, keys.Next());
            var fromListTask = GetVideoIdsFromList(listIds, keys.Next());
            var fromDbTask = _repository.GetAllVideoIdsAsync();

            var videoIds = new HashSet<string>();
            videoIds.AddRange(await fromChannelsTask);
            videoIds.AddRange(await fromListTask);
            videoIds.AddRange(await fromDbTask);

            var videoIdsPartitions = videoIds.Partition(_settingsProvider.Parallelism);
            var videoCountDown = new SyncCounter(videoIds.Count);
            var videoTasks = videoIdsPartitions.Select(part => GetVideoFromId(part, keys.Next(), videoCountDown, _ct)).ToList();

            while (!_ct.IsCancellationRequested) {
                var runningTasks = videoTasks.Count(t => !t.IsCompleted);
                _logger.LogTrace($"videos left: {videoCountDown.Read()}, running tasks: {runningTasks}");

                if(runningTasks<=0) break;
                await videoTasks.WaitOneOrTimeout(4000, ct: _ct);
            }

            var videos = new List<Video>();
            foreach (var videoTask in videoTasks) {
                videos.AddRange(await videoTask);
            }

            var updates = await _repository.SaveOrUpdate(videos);
            _logger.LogDebug($"video db updates: {updates}");
        }


        private async Task<List<string>> GetVideoIdsFromList(IEnumerable<string> listIds, string apiKey) {
            var videoIds = new List<string>();
            using (var api = new YoutubeApi(apiKey)) {
                foreach (var listId in listIds) {
                    var ytVids = await api.GetAllVideosFromPlaylist(listId, _ct);
                    _logger.LogDebug($"found {ytVids.PageInfo.TotalResults} videos for list {listId}");
                    videoIds.AddRange(ytVids.Items.Select(v=>v.ContentDetails.VideoId).Where(i => !string.IsNullOrWhiteSpace(i)));
                }
            }
            return videoIds;
        }

        private async Task<List<string>> GetVideoIdsFromChannels(IEnumerable<string> channelIds, string apiKey) {
            var videoIds = new List<string>();
            using (var api = new YoutubeApi(apiKey)) {
                foreach (var channelId in channelIds) {
                    var ytVids = await api.GetAllVideosFromChannel(channelId, _ct);
                    _logger.LogDebug($"found {ytVids.PageInfo.TotalResults} Videos for channel {channelId}");
                    videoIds.AddRange(ytVids.Items.Select(v => v.Id.VideoId).Where(i => !string.IsNullOrWhiteSpace(i)));
                }
                return videoIds;
            }
        }

        private async Task<List<Video>> GetVideoFromId(IEnumerable<string> videoIds, string apiKey, SyncCounter videoCountDown, CancellationToken ct) {
            var videos = new List<Video>();
            using (var api = new YoutubeApi(apiKey)) {
                foreach (var videoId in videoIds) {
                    var ytVid = await api.GetVideoDetails(videoId, ct);
                    var dbVid = ytVid.Items.Select(v => v.MapToDbEntity());
                    videoCountDown.Decrement();
                    videos.AddRange(dbVid);
                }
            }
            return videos;
        }
    }
}
