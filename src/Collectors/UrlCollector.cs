using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using YoutubeCollector.Db;
using YoutubeCollector.Lib;

namespace YoutubeCollector.Collectors {
    public class UrlCollector : ICollector {
        private readonly ILogger<UrlCollector> _logger;
        private readonly Repository _repository;
        private readonly SettingsProvider _settingsProvider;
        private readonly SyncHashSet<string> _urlHashSet = new SyncHashSet<string>();
        private CancellationToken _ct;

        public UrlCollector(ILogger<UrlCollector> logger, Repository repository, SettingsProvider settingsProvider) {
            _logger = logger;
            _repository = repository;
            _settingsProvider = settingsProvider;
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken) {
            if (string.IsNullOrWhiteSpace(_settingsProvider.UrlBucketBaseUrl)) {
                _logger.LogWarning("No base url for url-bucket provided");
                return;
            };
            _ct = stoppingToken;
            var tasks = new List<Task> {
                _repository.GetAllAuthorProfileImageUrlsAsync(StoreAsset),
                _repository.GetAllVideoThumbnailsAsync(StoreAsset),
                _repository.GetAllVideoMaxResImageUrlsAsync(StoreAsset),
            };

            while (!_ct.IsCancellationRequested) {
                var runningTasks = tasks.Count(t => !t.IsCompleted);
                _logger.LogInformation($"urls stored: {_urlHashSet.Count()}");
                if(runningTasks <= 0) break;
                await tasks.WaitOneOrTimeout(4000, _ct);
            }

            await Task.WhenAll(tasks);

        }

        private static bool IsUrl(string url) {
            if (string.IsNullOrWhiteSpace(url)) return false;
            try {
                var _ = new Uri(url);
                return true;
            }
            catch {
                return false;
            }
        }

        private async Task StoreAsset(string url) {
            if (IsUrl(url) && _urlHashSet.Add(url)) {
                _logger.LogTrace($"storing: {url}");
                // TODO: call to url bucket
                await Task.Delay(200, _ct);
            }
        }
    }
}
