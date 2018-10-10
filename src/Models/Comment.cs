using System;
using System.Collections;
using System.Collections.Generic;

namespace YoutubeCollector.Models {

    public class CommentBase {
        public string Id { get; set; }
        public string VideoId { get; set; }
    }

    public class Comment : CommentBase {
        public string AuthorDisplayName { get; set; }
        public string AuthorProfileImageUrl { get; set; }
        public string AuthorChannelUrl { get; set; }
        public string OriginalText { get; set; }
        public string ParentId { get; set; }
        public long? LikeCount { get; set; }
        public string ModerationStatus { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool HasAnswers { get; set; }
        public CommentType CommentType { get; set; }
        public Video Video { get; set; }    
        public IList<Comment> Children { get; set; }
        public Comment Parent { get; set; }
    }

    public enum CommentType {
        /// <summary>
        /// Top level - comment on Video
        /// </summary>
        Comment,

        /// <summary>
        /// Second level - comment on comment (aka answer or reply)
        /// </summary>
        Answer,
    }
}