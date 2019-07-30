using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YouTubeAPI.Helper;
using YouTubeAPI.Model;
using YouTubeAPI.DAL;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;

namespace YouTubeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VideosController : ControllerBase
    {
        public class URLDTO
        {
           public String URL { get; set; }
        }
        private readonly youtubeContext _context;
        private IVideoRepository videoRepository;
        private readonly IMapper _mapper;

        public VideosController(youtubeContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            this.videoRepository = new VideoRepository(new youtubeContext());
        }

        // GET: api/Videos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Video>>> GetVideo()
        {
            return await _context.Video.ToListAsync();
        }

        // GET: api/Videos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Video>> GetVideo(int id)
        {
            var video = await _context.Video.FindAsync(id);

            if (video == null)
            {
                return NotFound();
            }

            return video;
        }

        // PUT: api/Videos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVideo(int id, Video video)
        {
            if (id != video.VideoId)
            {
                return BadRequest();
            }

            _context.Entry(video).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VideoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Videos
        [HttpPost]
        public async Task<ActionResult<Video>> PostVideo(URLDTO URL)
        {
           
                String videoId = YoutubeHelper.GetVideoIdFromURL(URL.URL);
                Video newVideo = YoutubeHelper.GetVideoFromId(videoId);
           

            _context.Video.Add(newVideo);
            await _context.SaveChangesAsync();

            TranscriptionsController transcriptionsController = new TranscriptionsController(new youtubeContext());
            List<Transcription> transcriptions = YoutubeHelper.GetTranscriptions(videoId);

            //Go through each transaction and insert it into the database by calling PostTransactions()

            Task addCaptions = Task.Run(async () =>
            {
                for (int i = 0; i < transcriptions.Count; i++)
                {
                    Transcription transcription = transcriptions.ElementAt(i);
                    transcription.VideoId = newVideo.VideoId;

                    await transcriptionsController.PostTranscription(transcription);
                    Console.WriteLine("inserting transcription" + i);

                }

            });

            return CreatedAtAction("GetVideo", new { id = newVideo.VideoId }, newVideo);
        }

        // DELETE: api/Videos/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Video>> DeleteVideo(int id)
        {
            var video = await _context.Video.FindAsync(id);
            if (video == null)
            {
                return NotFound();
            }

            _context.Video.Remove(video);
            await _context.SaveChangesAsync();

            return video;
        }

        // GET api/Videos/SearchByTranscriptions/HelloWorld
        [HttpGet("SearchByTranscriptions/{searchString}")]
        public async Task<ActionResult<IEnumerable<Video>>> Search(string searchString)
        {
            var videos = await _context.Video.Include(video => video.Transcription).Select(video => new Video
            {
                VideoId = video.VideoId,
                VideoTitle = video.VideoTitle,
                VideoLength = video.VideoLength,
                WebUrl = video.WebUrl,
                ThumbnailUrl = video.ThumbnailUrl,
                IsFavourite = video.IsFavourite,
                Transcription = video.Transcription.Where(tran => tran.Phrase.Contains(searchString)).ToList()
            }).ToListAsync();

            // Removes all videos with empty transcription
            videos.RemoveAll(video => video.Transcription.Count == 0);
            return Ok(videos);
            
        }

        [HttpGet("SearchByVideoName/{searchString}")]
        public async Task<List<Video>> SearchVideo(string searchString)
        {
            var videoTitles = from m in _context.Video
                              select m; //get all the videos
            if (!String.IsNullOrEmpty(searchString))
            {
                videoTitles = videoTitles.Where(s => s.VideoTitle.Contains(searchString));
            }
            var returned = await videoTitles.ToListAsync();

            return returned;
        }

        //PUT with PATCH to handle isFavourite
        [HttpPatch("update/{id}")]
        public VideoDTO Patch(int id, [FromBody]JsonPatchDocument<VideoDTO> videoPatch)
        {
            //get original video object from the database
            Video originVideo = videoRepository.GetVideoByID(id);
            //use automapper to map that to DTO object
            VideoDTO videoDTO = _mapper.Map<VideoDTO>(originVideo);
            //apply the patch to that DTO
            videoPatch.ApplyTo(videoDTO);
            //use automapper to map the DTO back ontop of the database object
            _mapper.Map(videoDTO, originVideo);
            //update video in the database
            _context.Update(originVideo);
            _context.SaveChanges();
            return videoDTO;
        }

        private bool VideoExists(int id)
        {
            return _context.Video.Any(e => e.VideoId == id);
        }
    }
}
