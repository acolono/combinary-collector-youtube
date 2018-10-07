using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ApiKeys = YoutubeCollector.Lib.RotatableReadOnlyCollection<string>;

namespace YoutubeCollector.Lib {
    public class SettingsProvider {
        private readonly IConfiguration _configuration;

        public SettingsProvider(IConfiguration configuration) {
            _configuration = configuration;
        }

        public string PgConnectionString => Get("PG_CONNECTION_STRING", "Host=localhost;Database=postgres;Username=postgres");
        public string PgHost => Get("PG_HOST");
        public bool LogSql => ParseBool(Get("LOG_SQL")) ?? false;
        public bool CollectVideos => ParseBool(Get("COLLECT_VIDEOS")) ?? true;
        public bool CollectComments => ParseBool(Get("COLLECT_COMMENTS")) ?? true;
        public bool CollectAnswers => ParseBool(Get("COLLECT_ANSWERS")) ?? true;
        public IList<string> ChannelIds => GetChannelIds();
        public ApiKeys ApiKeys => GetApiKeys();
        public int Parallelism => GetParallelism();
        public int IdleMinutes => ParseInt(Get("IDLE_MINUTES")) ?? 60;

        private int GetParallelism() {
            if (Debugger.IsAttached) return 1;
            return ParseInt(Get("PARALLELISM")) ?? Environment.ProcessorCount;
        }

        private ApiKeys GetApiKeys() {
            var r = new List<string>();
            var apiKeys = Get("API_KEYS") ?? Get("API_KEYS");
            if (apiKeys != null) {
                r.AddRange(apiKeys.Split(',',';'));
            }
            return r.ToRotatableReadOnlyCollection();
        }

        private IList<string> GetChannelIds() {
            var r = new List<string>();
            var cids = Get("CHANNEL_ID") ?? Get("CHANNEL_IDS");
            if (cids != null) {
                r.AddRange(cids.Split(',', ';'));
            }
            return r;

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
            return GetEnvVar(key) ?? GetConfig(key) ?? GetDockerSecret(key) ?? defaultValue;
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
