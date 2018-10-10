using System;
using System.Collections.Generic;
using System.Linq;
using Google.Apis.YouTube.v3.Data;

namespace YoutubeCollector.Lib {
    public static class Mapper {
        public static Models.Video MapToDbEntity(this Video v) {
            return new Models.Video {
                Id = v.Id,
                PublishedAt = v.Snippet?.PublishedAt,
                ChannelId = v.Snippet?.ChannelId,
                Title = v.Snippet?.Title.RemoveNullChars(),
                Description = v.Snippet?.Description.RemoveNullChars(),
                Thumbnail = v.Snippet?.Thumbnails?.Default__?.Url,
                MaxResImage = v.Snippet?.Thumbnails?.Maxres?.Url,
                ChannelTitle = v.Snippet?.ChannelTitle.RemoveNullChars(),
                Tags = v.Snippet?.Tags?.ToArray(),
                Duration = v.ContentDetails?.Duration,
                CaptionsAvailable = v.ContentDetails?.Caption?.ToBool() ?? false,
                Rating = v.ContentDetails?.ContentRating?.YtRating,
                Embeddable = v.Status?.Embeddable ?? false,
                Statistics = new List<Models.Statistics> {v.Statistics?.MapToDbEntity(v.Id)}.Where(s => s != null).ToList(),
                TopicCategories = v.TopicDetails?.TopicCategories?.ToArray(),
                HasComments = (v.Statistics?.CommentCount ?? 0) > 0,
            };
        }

        public static Models.Statistics MapToDbEntity(this VideoStatistics s, string videoId) {
            return new Models.Statistics {
                VideoId = videoId,
                CommentCount = s?.CommentCount,
                DislikeCount = s?.DislikeCount,
                LikeCount = s?.LikeCount,
                ViewCount = s?.ViewCount,
                Timestamp = DateTime.UtcNow,
            };
        }

        public static Models.Comment MapToDbEntity(this Comment c, Models.CommentType commentType, string videoId = null, bool hasAnswers = false) {
            var m = new Models.Comment {
                VideoId = c?.Snippet?.VideoId,
                Id = c?.Id,
                AuthorDisplayName = c?.Snippet?.AuthorDisplayName.RemoveNullChars(),
                AuthorChannelUrl = c?.Snippet?.AuthorChannelUrl,
                AuthorProfileImageUrl = c?.Snippet?.AuthorProfileImageUrl,
                LikeCount = c?.Snippet?.LikeCount,
                ModerationStatus = c?.Snippet?.ModerationStatus,
                OriginalText = c?.Snippet?.TextOriginal.RemoveNullChars(),
                ParentId = c?.Snippet?.ParentId,
                PublishedAt = c?.Snippet?.PublishedAt,
                UpdatedAt = c?.Snippet?.UpdatedAt,
                CommentType = commentType,
                HasAnswers = hasAnswers,
            };
            if (videoId != null) m.VideoId = videoId;

            return m;
        }

        public static Uri ToUri(this string str) => str is null ? null : new Uri(str);
        public static bool ToBool(this string str) => bool.Parse(str);

        public static string RemoveNullChars(this string str) => str?.Replace("\0", "");
    }
}
