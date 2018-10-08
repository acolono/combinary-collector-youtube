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
    public class CommentCollector : ICollector {
        private readonly Repository _repository;
        private readonly SettingsProvider _settingsProvider;
        private readonly ILogger<CommentCollector> _logger;
        private CancellationToken _ct;

        public async Task ExecuteAsync(CancellationToken stoppingToken) {
            _ct = stoppingToken;
            await CollectComments();
        }

        public CommentCollector(Repository repository, SettingsProvider settingsProvider, ILogger<CommentCollector> logger) {
            _repository = repository;
            _settingsProvider = settingsProvider;
            _logger = logger;
        }

        private async Task CollectComments() {
            var keys = _settingsProvider.ApiKeys;
            var par = _settingsProvider.Parallelism;
            var videoIdsWithComments = _repository.GetAllVideoIdsWithComments();
            var videoIdParts = videoIdsWithComments.Partition(par);
            var commentsCounter = new SyncCounter();
            var updatesCounter = new SyncCounter();
            var videoCountDown = new SyncCounter(videoIdsWithComments.Count);
            var tasks = videoIdParts.Select(videoIds => GetCommentsFromVideoIds(videoIds, keys.Next(), commentsCounter, updatesCounter, videoCountDown)).ToList();

            while (!_ct.IsCancellationRequested) {
                var runningTasks = tasks.Count(t => !t.IsCompleted);
                _logger.LogTrace($"comments received: {commentsCounter.Read()}, videos left: {videoCountDown.Read()}, db updates: {updatesCounter.Read()}, running tasks: {runningTasks}");
                if (runningTasks == 0) break;
                await tasks.WaitOneOrTimeout(4000);
            }

            foreach (var task in tasks) {
                await task;
            }

            _logger.LogDebug($"comments db updates: {updatesCounter.Read()}");
        }

        private async Task GetCommentsFromVideoIds(IEnumerable<string> videoIds, string apiKey, SyncCounter commentsCnt, SyncCounter updatesCnt, SyncCounter vidCntDown) {
            using (var api = new YoutubeApi(apiKey)) {
                foreach (var videoId in videoIds) {
                    var ytComments = await api.GetAllCommentsFromVideo(videoId, _ct);
                    vidCntDown.Decrement();
                    var dbComments = ytComments.Items.Select(i => i.Snippet.TopLevelComment.MapToDbEntity(CommentType.Comment)).ToList();
                    commentsCnt.Add(dbComments.Count);
                    var updates = await _repository.SaveOrUpdate(dbComments);
                    updatesCnt.Add(updates);
                }    
            }
        }
    }
}
