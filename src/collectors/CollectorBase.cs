using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeCollector.Lib;
using YoutubeCollector.Models;

namespace YoutubeCollector.collectors {
    public abstract class CollectorBase : ICollector {
        protected async Task<List<Video>> GetVideoFromId(string videoId, string apiKey, CancellationToken ct) {
            using (var api = new YoutubeApi(apiKey)) {
                var ytVid = await api.GetVideoDetails(videoId, ct);
                return ytVid.Items.Select(v => v.MapToDbEntity()).ToList();
            }
        }

        public abstract Task ExecuteAsync(CancellationToken stoppingToken);
    }
}
