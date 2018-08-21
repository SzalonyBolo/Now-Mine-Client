using System;
using System.Collections.Generic;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;

namespace NowMineClient.Network
{
    class YoutubeProvider
    {
        #region Data
        private const string SEARCH = "http://gdata.youtube.com/feeds/api/videos?q={0}&alt=rss&&max-results=20&v=1";
        YouTubeService youtubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            ApiKey = "AIzaSyC5zI6qk0KkTtePHp5yh23fcPgSLnio2V4",
            ApplicationName = "Play Mine!"
        });
        #endregion

        public List<YoutubeInfo> LoadVideosKey(string keyWord)
        {
            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = keyWord; // Replace with your search term.
            searchListRequest.MaxResults = 20;

            // Call the search.list method to retrieve results matching the specified query term.

            Google.Apis.YouTube.v3.Data.SearchListResponse searchListResponse = null;
            try
            {
                searchListResponse = searchListRequest.Execute();
            }
            catch (Exception e)
            {

            }

            List<string> videos = new List<string>();
            //List<string> channels = new List<string>();
            //List<string> playlists = new List<string>();

            List<YoutubeInfo> resultInfo = new List<YoutubeInfo>();

            foreach (var ytInfo in searchListResponse.Items)
            {
                YoutubeInfo result = new YoutubeInfo();
                switch (ytInfo.Id.Kind)
                {
                    case "youtube#video":
                        videos.Add(String.Format("{0} ({1})", ytInfo.Snippet.Title, ytInfo.Id.VideoId));
                        result.title = ytInfo.Snippet.Title;
                        result.channelName = ytInfo.Snippet.ChannelTitle;
                        result.id = ytInfo.Id.VideoId;
                        result.userId = User.DeviceUser.Id;
                        //result.LinkUrl = "http://www.youtube.com/embed/" + ytInfo.Id.VideoId;
                        //if (ytInfo.Snippet.Thumbnails.High != null)
                        //    result.thumbnail = ytInfo.Snippet.Thumbnails.High;
                        //else if (ytInfo.Snippet.Thumbnails.Medium != null)
                        //    result.thumbnail = ytInfo.Snippet.Thumbnails.Medium;
                        //else if (ytInfo.Snippet.Thumbnails.Standard != null)
                        //    result.thumbnail = ytInfo.Snippet.Thumbnails.Standard;
                        //else if (ytInfo.Snippet.Thumbnails.Default__ != null)
                        result.thumbnail = ytInfo.Snippet.Thumbnails.Default__;
                        resultInfo.Add(result);
                        break;

                    case "youtube#channel":
                        //channels.Add(String.Format("{0} ({1})", MusicPiece.Snippet.Title, MusicPiece.Id.ChannelId));
                        //result.ChannelName = MusicPiece.Snippet.Title;
                        break;

                    case "youtube#playlist":
                        //playlists.Add(String.Format("{0} ({1})", MusicPiece.Snippet.Title, MusicPiece.Id.PlaylistId));
                        //result.Title = MusicPiece.Snippet.Title;
                        break;
                }
                //resultInfo.Add(result);
            }
            return resultInfo;
        }

        //public async Task<List<YoutubeInfo>> GeKeywordSearch(string keyword)
        //{
        //    using (var client = new HttpClient())
        //    {
        //        var uri = getUrl(keyword);
        //        var response = await client.GetAsync(uri);
        //        if (response.IsSuccessStatusCode)
        //        {
        //            var content = await response.Content.ReadAsStringAsync();
        //            //var infoList= JsonConvert.DeserializeObject<List<YoutubeInfo>>(content);
        //            var xmlObject = JsonConvert.DeserializeObject(content);
        //            var infoList = new List<YoutubeInfo>();
        //            return infoList;
        //        }
        //    }
        //    return null;
        //}

        //private Uri getUrl(string keyword)
        //{
        //    string keywordEncoded = keyword.Replace(" ", "%20");
        //    return new Uri("https://www.googleapis.com/youtube/v3/search?part=snippet&q=\'" + keywordEncoded + "\'&type=video&key=" + API_KEY);
        //}
    }
}
