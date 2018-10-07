using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YoutubeCollector.Models {
    public class Video {
        public string Id { get; set; }
        public DateTime? PublishedAt { get; set; }
        public string ChannelId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Thumbnail { get; set; }
        public string MaxResImage { get; set; }
        public string ChannelTitle { get; set; }
        public string[] Tags { get; set; }
        public string Duration { get; set; }
        public bool CaptionsAvailable { get; set; }
        public string Rating { get; set; }
        public bool Embeddable { get; set; }
        public string[] TopicCategories { get; set; }
        public bool HasComments { get; set; }

        public IList<Statistics> Statistics { get; set; }
        public IList<Comment> Comments { get; set; }
    }
}
