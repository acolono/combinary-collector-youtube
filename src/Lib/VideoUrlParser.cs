using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.WebUtilities;

namespace YoutubeCollector.Lib {
    public class VideoUrlParser {

        public string GetVideoId(string urlOrId) {
            return InternalGetId(urlOrId) ?? throw new Exception("Unable to extract VideoId");
        }

        public bool TryGetVideoId(string urlOrId, out string videoId) {
            videoId = InternalGetId(urlOrId);
            return videoId != null;
        }

        private string InternalGetId(string urlOrId) {
            // something similar in js : https://github.com/itteco/iframely/blob/master/plugins/domains/youtube.com/youtube.video.js

            if (urlOrId is null) return null;

            // Jj0ZkflWXQk
            var idRegex = new Regex(@"^[a-zA-Z0-9_-]+$");
            if (idRegex.IsMatch(urlOrId)) return urlOrId;

            // https://youtu.be/Jj0ZkflWXQk
            var shortlinkRegex = new Regex(@"youtu\.be/(?<videoId>[a-zA-Z0-9_-]+)");
            if (shortlinkRegex.IsMatch(urlOrId, out var match)) {
                return match.Groups["videoId"].Value;
            }

            // https://www.youtube.com/watch?v=Jj0ZkflWXQk
            var watchlinkRegex = new Regex(@"youtube\.com/watch\?v=(?<videoId>[a-zA-Z0-9_-]+)");
            if (watchlinkRegex.IsMatch(urlOrId, out match)) {
                return match.Groups["videoId"].Value;
            }

            // https://www.youtube.com/embed/RkEXGgdqMz8
            var embedlinkRegex = new Regex(@"youtube\.com/embed/(?<videoId>[a-zA-Z0-9_-]+)");
            if (embedlinkRegex.IsMatch(urlOrId, out match)) {
                return match.Groups["videoId"].Value;
            }

            // https://www.youtube.com/watch?index=2&v=c0U4AUTbDC8&t=0s&list=LL2m1XOEtkVR_KGKrZDao20Q

            string query;
            try {
                var url = new Uri(urlOrId);
                query = url.GetComponents(UriComponents.Query, UriFormat.UriEscaped);
            }
            catch (Exception) {
                return null;
            }

            var parameters = QueryHelpers.ParseQuery(query);
            if (parameters.TryGetValue("v", out var v)) {
                if (idRegex.IsMatch(v)) return v;
            }

            return null;
        }
    }

    public static class RegexEx {

        public static bool IsMatch(this Regex regex, string input, out Match match) {
            match = regex.Match(input);
            return match.Success;
        }
    }
}
