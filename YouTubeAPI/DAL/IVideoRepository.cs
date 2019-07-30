using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YouTubeAPI.Model;

namespace YouTubeAPI.DAL
{
    public interface IVideoRepository : IDisposable
    {
        IEnumerable<Video> GetVideos();
        Video GetVideoByID(int VideoId);
        void InsertVideo(Video video);
        void DeleteVideo(int VideoId);
        void UpdateVideo(Video video);
        void Save();
    }
}