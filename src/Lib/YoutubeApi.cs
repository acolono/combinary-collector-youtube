using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace YoutubeCollector.Lib {
    public class YoutubeApi : IDisposable
    {
        private readonly YouTubeService _youtubeService;

        public YoutubeApi(string apiKey, string applicationName = null)
        {
            _youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = apiKey,
                ApplicationName = applicationName ?? "combinary-youtube-collector", 
            });
        }

        public YoutubeApi NewCopy()
        {
            lock (_youtubeService)
            {
                return new YoutubeApi(_youtubeService.ApiKey, _youtubeService.ApplicationName);
            }
        }

        public async Task<VideoListResponse> GetVideoDetails(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await GetVideo(id, "id,statistics,contentDetails,topicDetails,status,player,snippet", cancellationToken);
        }

        public async Task<VideoListResponse> GetVideo(string id, string part, CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = _youtubeService.Videos.List(part);
            request.Id = id;
            return await request.ExecuteAllAsync(cancellationToken);
        }

        public async Task<SearchListResponse> GetAllVideosFromChannel(string channelId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = _youtubeService.Search.List("id");
            request.ChannelId = channelId;
            request.Type = "video";
            return await request.ExecuteAllAsync(cancellationToken);
        }

        public async Task<SearchListResponse> GetAllVideosFromQuery(string query, CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = _youtubeService.Search.List("id");
            request.Q = query;
            request.Type = "video";
            return await request.ExecuteAllAsync(cancellationToken);
        }


        public async Task<PlaylistItemListResponse> GetAllVideosFromPlaylist(string playlistId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = _youtubeService.PlaylistItems.List("id");
            request.PlaylistId = playlistId;
            return await request.ExecuteAllAsync(cancellationToken);
        }

        public async Task<CommentThreadListResponse> GetAllCommentsFromVideo(string videoId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = _youtubeService.CommentThreads.List("snippet");
            request.VideoId = videoId;
            return await request.ExecuteAllAsync(cancellationToken);
        }

        public async Task<CommentListResponse> GetAllAnswersFromComment(string parentId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = _youtubeService.Comments.List("snippet");
            request.ParentId = parentId;
            return await request.ExecuteAllAsync(cancellationToken);
        }

        public void Dispose()
        {
            _youtubeService.Dispose();
        }
    }
}