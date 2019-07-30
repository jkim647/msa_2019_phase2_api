using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YouTubeAPI.Model;

namespace YouTubeAPI.DAL
{
    public class VideoRepository : IVideoRepository, IDisposable
    {
        private youtubeContext context;

        public VideoRepository(youtubeContext context)
        {
            this.context = context;
            
        }

        public IEnumerable<Video> GetVideos()
        {
            return context.Video.ToList();
        }

        public Video GetVideoByID(int id)
        {
            return context.Video.Find(id);
        }

        public void InsertVideo(Video video)
        {
            context.Video.Add(video);
        }

        public void DeleteVideo(int videoId)
        {
            Video video = context.Video.Find(videoId);
            context.Video.Remove(video);
        }

        public void UpdateVideo(Video video)
        {
            context.Entry(video).State = EntityState.Modified;
        }

        public void Save()
        {
            context.SaveChanges();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
