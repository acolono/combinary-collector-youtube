using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using YoutubeCollector.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using Npgsql;
using YoutubeCollector.Lib;

namespace YoutubeCollector.Db {
    public class StorageContext : DbContext {
        private readonly ILogger<StorageContext> _logger;
        public bool? LogSql { get; private set; }
        private readonly SettingsProvider _settingsProvider = new SettingsProvider(null, null);

        public StorageContext() {}

        public StorageContext(bool? logSql = null, SettingsProvider settingsProvider = null, ILogger<StorageContext> logger = null) {
            _logger = logger;
            if (logSql != null) LogSql = logSql;
            if (settingsProvider != null) _settingsProvider = settingsProvider;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            base.OnConfiguring(optionsBuilder);
            if (!optionsBuilder.IsConfigured) {
                // devenv$ docker run -d --name pg -p 5432:5432 --restart always postgres:alpine

                var cfg = _settingsProvider;
                LogSql = LogSql ?? cfg.LogSql;

                var cb = new NpgsqlConnectionStringBuilder(cfg.PgConnectionString);
                var pgHost = cfg.PgHost;
                if (pgHost != null) {
                    var ips = Dns.GetHostAddresses(pgHost);
                    _logger?.LogTrace($"Resolving: {pgHost} -> {string.Join(", ", ips.Select(i => i.ToString()))}");
                    if (ips.Any()) pgHost = ips.First().ToString();
                    cb.Host = pgHost;
                }
                optionsBuilder.UseNpgsql(cb.ConnectionString);
                if (LogSql ?? cfg.LogSql) {
                    optionsBuilder.UseLoggerFactory(new ConsoleLoggerFactory());
                    optionsBuilder.EnableSensitiveDataLogging();
                    _logger?.LogTrace($"connectionString: {cb.ConnectionString}");
                }
            }
        }

        public DbSet<Video> Videos { get; set; }
        public DbSet<Statistics> Statistics { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder) {
            var video = builder.Entity<Video>();
            video.HasKey(k => k.Id);
            video.HasIndex(k => k.HasComments);

            var comment = builder.Entity<Comment>();
            comment.HasKey(k => k.Id);
            comment.HasIndex(k => k.CommentType);
            comment.HasIndex(k => k.HasAnswers);
            comment.HasOne(d => d.Parent)
                .WithMany(p => p.Children)
                .HasForeignKey(d => d.ParentId);
        }
    }

    public class ConsoleLogger : ILogger {
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) => Console.WriteLine($"{logLevel}:{formatter(state, exception)}");
        public bool IsEnabled(LogLevel logLevel) => true;
        public IDisposable BeginScope<TState>(TState state) => null;
    }

    public class ConsoleLoggerFactory : ILoggerFactory{
        public void Dispose() {}
        public ILogger CreateLogger(string categoryName) => new ConsoleLogger();
        public void AddProvider(ILoggerProvider provider) {}
    }
}
