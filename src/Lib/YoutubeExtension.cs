using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace YoutubeCollector.Lib {
    public static class YoutubeExtension {
         public static async Task<CommentListResponse> ExecuteAllAsync(this CommentsResource.ListRequest request, CancellationToken ct = default(CancellationToken))
        {
            request.MaxResults = request.MaxResults ?? 100;
            var response = await request.ExecuteAsync(ct);
            if (!response.Items.Any()) return response;
            var collection = response.Items.ToList();
            while (!ct.IsCancellationRequested)
            {
                if (string.IsNullOrWhiteSpace(response.NextPageToken)) break;
                request.PageToken = response.NextPageToken;
                response = await request.ExecuteAsync(ct);
                if (response.Items.Any()) collection.AddRange(response.Items);
            }

            response.Items = collection;
            return response;
        }

        public static async Task<CommentThreadListResponse> ExecuteAllAsync(this CommentThreadsResource.ListRequest request, CancellationToken ct = default(CancellationToken))
        {
            request.MaxResults = request.MaxResults ?? 100;
            var response = await request.ExecuteAsync(ct);
            if (!response.Items.Any()) return response;
            var collection = response.Items.ToList();
            while (!ct.IsCancellationRequested)
            {
                if(string.IsNullOrWhiteSpace(response.NextPageToken)) break;
                request.PageToken = response.NextPageToken;
                response = await request.ExecuteAsync(ct);
                if(response.Items.Any()) collection.AddRange(response.Items);
            }

            response.Items = collection;
            return response;
        }

        public static async Task<SearchListResponse> ExecuteAllAsync(this SearchResource.ListRequest request, CancellationToken ct = default(CancellationToken))
        {
            request.MaxResults = request.MaxResults ?? 50;
            var response = await request.ExecuteAsync(ct);
            if (!response.Items.Any()) return response;
            var collection = response.Items.ToList();
            while (!ct.IsCancellationRequested)
            {
                if (string.IsNullOrWhiteSpace(response.NextPageToken)) break;
                request.PageToken = response.NextPageToken;
                response = await request.ExecuteAsync(ct);
                if (response.Items.Any()) collection.AddRange(response.Items);
            }

            response.Items = collection;
            return response;
        }

        public static async Task<VideoListResponse> ExecuteAllAsync(this VideosResource.ListRequest request, CancellationToken ct = default(CancellationToken))
        {
            request.MaxResults = request.MaxResults ?? 50;
            var response = await request.ExecuteAsync(ct);
            if (!response.Items.Any()) return response;
            var collection = response.Items.ToList();
            while (!ct.IsCancellationRequested)
            {
                if (string.IsNullOrWhiteSpace(response.NextPageToken)) break;
                request.PageToken = response.NextPageToken;
                response = await request.ExecuteAsync(ct);
                if (response.Items.Any()) collection.AddRange(response.Items);
            }

            response.Items = collection;
            return response;
        }

        public static async Task<PlaylistItemListResponse> ExecuteAllAsync(this PlaylistItemsResource.ListRequest request, CancellationToken ct = default(CancellationToken))
        {
            request.MaxResults = request.MaxResults ?? 50;
            var response = await request.ExecuteAsync(ct);
            if (!response.Items.Any()) return response;
            var collection = response.Items.ToList();
            while (!ct.IsCancellationRequested)
            {
                if (string.IsNullOrWhiteSpace(response.NextPageToken)) break;
                request.PageToken = response.NextPageToken;
                response = await request.ExecuteAsync(ct);
                if (response.Items.Any()) collection.AddRange(response.Items);
            }

            response.Items = collection;
            return response;
        }
    }
}
