using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YoutubeCollector.Lib;
using YoutubeCollector.Models;

namespace YoutubeCollector.Db {
    public class Repository {
        private readonly ILogger<Repository> _logger;
        private readonly SettingsProvider _settingsProvider;

        public Repository(ILogger<Repository> logger, SettingsProvider settingsProvider) {
            _logger = logger;
            _settingsProvider = settingsProvider;
        }

        private StorageContext GetContext() => new StorageContext(settingsProvider: _settingsProvider);

        public async Task<IList<string>> GetAllVideoIdsAsync() {
            using (var db = GetContext()) {
                return await db.Videos.Select(o => o.Id).Distinct().ToListAsync();
            }
        }

        public IList<CommentBase> GetCommentIdsByType(CommentType commentType) {
            using (var db = GetContext()) {
                return db.Comments.Where(c => c.CommentType == commentType).Select(c => new CommentBase {Id = c.Id, VideoId = c.VideoId}).ToList();
            }
        }

        public HashSet<string> GetAllVideoIdsWithComments() {
            using (var db = GetContext()) {
                return db.Videos.Where(o=>o.HasComments).Select(o => o.Id).ToHashSet();
            }
        }

        public async Task<int> SaveOrUpdate(IEnumerable<Comment> comments) {
            using (var db = GetContext()) {
                foreach (var comment in comments) {
                    comment.Video = null;
                    var existing = await db.Comments.FindAsync(comment.Id);
                    if (existing is null) {
                        await db.Comments.AddAsync(comment);
                    }
                    else {
                        db.Entry(existing).CurrentValues.SetValues(comment);
                    }
                }
                return await db.SaveChangesAsync();
            }
        }

        public async Task<int> SaveOrUpdate(Video video) => await SaveOrUpdate(new[] {video});
        public async Task<int> SaveOrUpdate(IEnumerable<Video> videos) {
            using (var db = GetContext()) {
                foreach (var video in videos) {
                    var statistics = video.Statistics;
                    video.Statistics = null;
                    var existing = await db.Videos.FindAsync(video.Id);
                    if (existing is null) {
                        await db.AddAsync(video);
                    }
                    else {
                        db.Entry(existing).CurrentValues.SetValues(video);
                    }
                    if(statistics != null && statistics.Any()) {
                        await db.Statistics.AddRangeAsync(statistics);
                    }
                }
                return await db.SaveChangesAsync();
            }
        }

        public async Task Save(Statistics statistics) => await Save(new[] {statistics});
        public async Task Save(IEnumerable<Statistics> statistics) {
            using (var db = GetContext()) {
                await db.Statistics.AddRangeAsync(statistics);
            }
        }

        public async Task Migrate(bool logSql=false) {
            using (var db = new StorageContext(logSql)) {
                // take a minute to get the database up and migrated
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
                Exception lastException = null;
                while (!cts.Token.IsCancellationRequested) {
                    try {
                        await db.Database.MigrateAsync(cts.Token);
                        lastException = null;
                        break;
                    }
                    catch (Exception e) {
                        lastException = e;
                        _logger.LogWarning($"Migration Problem: {e.Message}");
                    }

                    await Task.Delay(1000, cts.Token);
                }

                if (lastException != null || cts.Token.IsCancellationRequested) {
                    throw new Exception("Migration Failed", lastException);
                }
            }
        }
    }
}
