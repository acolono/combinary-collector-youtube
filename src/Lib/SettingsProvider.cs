using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ApiKeys = YoutubeCollector.Lib.RotatableReadOnlyCollection<string>;

namespace YoutubeCollector.Lib {
    public class SettingsProvider {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SettingsProvider> _logger;

        public SettingsProvider(IConfiguration configuration, ILogger<SettingsProvider> logger) {
            _configuration = configuration;
            _logger = logger;
        }

        public string PgConnectionString => Get("PG_CONNECTION_STRING", "Host=localhost;Database=postgres;Username=postgres");
        public string PgHost => Get("PG_HOST");
        public bool LogSql => ParseBool(Get("LOG_SQL")) ?? false;
        public bool CollectVideos => ParseBool(Get("COLLECT_VIDEOS")) ?? true;
        public bool CollectComments => ParseBool(Get("COLLECT_COMMENTS")) ?? true;
        public bool CollectAnswers => ParseBool(Get("COLLECT_ANSWERS")) ?? true;
        public IList<string> ChannelIds => GetIds("CHANNEL_ID", "CHANNEL_IDS");
        public IList<string> ListIds => GetIds("LIST_ID","LIST_IDS");
        public ApiKeys ApiKeys => GetIds("API_KEY", "API_KEYS").ToRotatableReadOnlyCollection();
        public int Parallelism => GetParallelism();
        public int IdleMinutes => ParseInt(Get("IDLE_MINUTES")) ?? 60;

        private int GetParallelism() {
            if (Debugger.IsAttached) return 1;
            return ParseInt(Get("PARALLELISM")) ?? Environment.ProcessorCount;
        }

        private IList<string> GetIds(string singular, string plural) {
            var list = new List<string>();
            var ids = Get(singular) ?? Get(plural);
            if (ids != null) {
                var foundIds = ids.Split(',', ';').Select(i => i.Trim()).Where(i => !string.IsNullOrWhiteSpace(i));
                list.AddRange(foundIds);
            }
            return list;

        }

        private int? ParseInt(string value) {
            if (value is null) return null;
            if (int.TryParse(value, out var v)) {
                return v;
            }
            return null;
        }

        private bool? ParseBool(string value) {
            if (value is null) return null;
            if (bool.TryParse(value, out var v)) {
                return v;
            }
            return null;
        }

        private string Get(string key, string defaultValue = null) {
#if DEBUG_DOCKER
            var envVar = GetEnvVar(key);
            var config = GetConfig(key);
            var docker = GetDockerSecret(key);
            _logger.LogCritical($"Get('{key}') = '{envVar ?? "<null>"}' ?? '{config ?? "<null>"}' ?? '{docker ?? "<null>"}' ?? '{defaultValue ?? "<null>"}'");
            return envVar ?? config ?? docker ?? defaultValue;
#else
            return GetEnvVar(key) ?? GetConfig(key) ?? GetDockerSecret(key) ?? defaultValue;
#endif
        }

        private string GetConfig(string key) {
            return _configuration?[key];
        }

        private string GetEnvVar(string key) => Environment.GetEnvironmentVariable(key); 

        private string GetDockerSecret(string key) {
            var baseDir = "/run/secrets";
            if (!Directory.Exists(baseDir)) return null;
            var file = Path.Combine(baseDir, key);
            if (!File.Exists(file)) return null;
            return File.ReadAllText(file, Encoding.UTF8);
        }
    }
}
