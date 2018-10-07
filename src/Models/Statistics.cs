using System;

namespace YoutubeCollector.Models {
    public class Statistics
    {
        public long Id { get; set; }
        public DateTime Timestamp { get; set; }

        public ulong? CommentCount { get; set; }
        public ulong? DislikeCount { get; set; }
        public ulong? LikeCount { get; set; }
        public ulong? ViewCount { get; set; }

        public string VideoId { get; set; }
        public Video Video { get; set; }
    }
}