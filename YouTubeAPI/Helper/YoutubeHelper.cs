using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using YouTubeAPI.Model;

namespace YouTubeAPI.Helper
{
    public class YoutubeHelper
    {
        public static void TestProgram()
        {
            Console.WriteLine(GetTranscriptions("ugRc5jx80yg"));
            Console.ReadLine();
        }

        public static String GetVideoIdFromURL(String URL)
        {
            int indexOfFirstId = URL.IndexOf("=") + 1;
            String videoId = URL.Substring(indexOfFirstId);
            return videoId;
        }

        public static Video GetVideoFromId(string videoId)  // method that returns a Video object, the purpose of this method is to return a Video object that the API can use to create an entry in the database.
        {
            String APIKey = "AIzaSyDgzKgPCj9S2-1QxCtTN7fkYUx-kgdkOzc";
            String YouTubeAPIURL = "https://www.googleapis.com/youtube/v3/videos?id=" + videoId + "&key=" + APIKey + "&part=snippet,contentDetails";

            // Use an http client to grab the JSON string from the web.
            String videoInfoJSON = new WebClient().DownloadString(YouTubeAPIURL);

            dynamic jsonObj = JsonConvert.DeserializeObject<dynamic>(videoInfoJSON);

            // Extract information from the dynamic object.
            String title = jsonObj["items"][0]["snippet"]["title"];
            String thumbnailURL = jsonObj["items"][0]["snippet"]["thumbnails"]["medium"]["url"];
            String durationString = jsonObj["items"][0]["contentDetails"]["duration"];
            String videoUrl = "https://www.youtube.com/watch?v=" + videoId;

            // duration is given in this format: PT4M17S, we need to use a simple parser to get the duration in seconds.
            TimeSpan videoDuration = XmlConvert.ToTimeSpan(durationString);
            int duration = (int)videoDuration.TotalSeconds;

            Video video = new Video
            {
                VideoTitle = title,
                WebUrl = videoUrl,
                VideoLength = duration,
                IsFavourite = false,
                ThumbnailUrl = thumbnailURL
            };
            return video;
        }

        private static String GetTranscriptionLink(String videoId)
        {
            String YouTubeVideoURL = "https://www.youtube.com/watch?v=" + videoId;
            // Use a WebClient to download the source code.
            String HTMLSource = new WebClient().DownloadString(YouTubeVideoURL);

            String pattern = "timedtext.+?lang=";  //Get me the last string that starts with timedtext and end with lang=, capturing any characters in between the two strings.
            Match match = Regex.Match(HTMLSource, pattern);

            if (match.ToString() != "")
            {
                String subtitleLink = "https://www.youtube.com/api/" + match + "en";
                subtitleLink = CleanLink(subtitleLink);
                return subtitleLink;
            }
            else
            {
                return null;
            }


        }

        public static String CleanLink(String subtitleLink)
        {
            subtitleLink = subtitleLink.Replace("\\\\u0026", "&");
            subtitleLink = subtitleLink.Replace("\\", "");
            return subtitleLink;
        }

        public static List<Transcription> GetTranscriptions(String videoId)
        {
            String subtitleLink = GetTranscriptionLink(videoId);

            // Use XmlDocument to load the subtitle XML.
            XmlDocument doc = new XmlDocument();
            doc.Load(subtitleLink);
            XmlNode root = doc.ChildNodes[1];

            // Go through each tag and look for start time and phrase.
            List<Transcription> transcriptions = new List<Transcription>();
            if (root.HasChildNodes)
            {
                for (int i = 0; i < root.ChildNodes.Count; i++)
                {
                    // Decode HTTP characters to text
                    // e.g. &#39; -> '
                    String phrase = root.ChildNodes[i].InnerText;
                    phrase = HttpUtility.HtmlDecode(phrase);

                    Transcription transcription = new Transcription
                    {
                        StartTime = (int)Convert.ToDouble(root.ChildNodes[i].Attributes["start"].Value),
                        Phrase = phrase
                    };

                    transcriptions.Add(transcription);
                }
            }
            return transcriptions;
        }

    }
}
