using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeCollector.Lib;
using YoutubeCollector.Models;

namespace YoutubeCollector.collectors {
    public abstract class CollectorBase : ICollector {
        protected async Task<List<Video>> GetVideoFromId(IEnumerable<string> videoIds, string apiKey, SyncCounter videoCountDown, CancellationToken ct) {
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

        public abstract Task ExecuteAsync(CancellationToken stoppingToken);
    }
}
