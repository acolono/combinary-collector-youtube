using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using YoutubeCollector.Db;
using YoutubeCollector.Lib;
using YoutubeCollector.Models;

namespace YoutubeCollector.collectors {
    public class AnswerCollector : ICollector {

        private readonly Repository _repository;
        private readonly SettingsProvider _settingsProvider;
        private readonly ILogger<AnswerCollector> _logger;
        private CancellationToken _ct;

        public async Task ExecuteAsync(CancellationToken stoppingToken) {
            _ct = stoppingToken;
            await CollectAnswers();
        }

        public AnswerCollector(Repository repository, SettingsProvider settingsProvider, ILogger<AnswerCollector> logger) {
            _repository = repository;
            _settingsProvider = settingsProvider;
            _logger = logger;
        }

        private async Task CollectAnswers() {
            var keys = _settingsProvider.ApiKeys;
            var par = _settingsProvider.Parallelism;
            var allParents = await _repository.GetCommentsWithAnswersAsync();
            var parentIdParts = allParents.Partition(par);
            var answersCounter = new SyncCounter();
            var updatesCounter = new SyncCounter();
            var parentsCountDown = new SyncCounter(allParents.Count);
            var tasks = parentIdParts.Select(parents => GetAnswersFromComments(parents, keys.Next(), answersCounter, updatesCounter, parentsCountDown)).ToList();
            
            while (!_ct.IsCancellationRequested) {
                var runningTasks = tasks.Count(t => !t.IsCompleted);
                _logger.LogTrace($"answers received: {answersCounter.Read()}, parents left: {parentsCountDown.Read()}, db updates: {updatesCounter.Read()}, running tasks: {runningTasks}");
                if(runningTasks<=0) break;
                await tasks.WaitOneOrTimeout(4000, _ct);
            }
            
            foreach (var task in tasks) {
                await task;
            }
            _logger.LogDebug($"comments db updates: {updatesCounter.Read()}");
        }

        private async Task GetAnswersFromComments(IEnumerable<CommentBase> parents, string apiKey, SyncCounter answersCounter, SyncCounter updatesCounter, SyncCounter parentsCountDown) {
            using (var api = new YoutubeApi(apiKey)) {
                foreach (var parent in parents) {
                    var ytComments = await api.GetAllAnswersFromComment(parent.Id, _ct).TryHarder(_logger, ct: _ct);
                    parentsCountDown.Decrement();
                    var dbAnswers = ytComments.Items.Select(i => i.MapToDbEntity(CommentType.Answer, parent.VideoId)).ToList();
                    answersCounter.Add(dbAnswers.Count);
                    var updates = await _repository.SaveOrUpdate(dbAnswers);
                    updatesCounter.Add(updates);
                }    
            }
        }
    }
}
