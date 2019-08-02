using YouTubeAPI.Controllers;
using YouTubeAPI.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace UnitTestScribrAPI
{
    [TestClass]
    public class TranscriptionsControllerUnitTests
    {
        public static readonly DbContextOptions<youtubeContext> options
            = new DbContextOptionsBuilder<youtubeContext>()
            .UseInMemoryDatabase(databaseName: "testDatabase")
            .Options;

        public static readonly IList<Transcription> transcriptions = new List<Transcription>
        {
            new Transcription()
                {
                    Phrase = "That's like calling"
                },
            new Transcription()
                {
                Phrase = "your peanut butter sandwich"
                }
        };

        [TestInitialize]
        public void SetupDb()
        {
            using (var context = new youtubeContext(options))
            {
                // populate the db
                context.Transcription.Add(transcriptions[0]);
                context.Transcription.Add(transcriptions[1]);
                context.SaveChanges();
            }
        }

        [TestCleanup]
        public void ClearDb()
        {
            using (var context = new youtubeContext(options))
            {
                // clear the db
                context.Transcription.RemoveRange(context.Transcription);
                context.SaveChanges();
            };
        }

        [TestMethod]
        public async Task TestGetSuccessfully()
        {
            using (var context = new youtubeContext(options))
            {
                TranscriptionsController transcriptionsController = new TranscriptionsController(context);
                ActionResult<IEnumerable<Transcription>> result = await transcriptionsController.GetTranscription();

                Assert.IsNotNull(result);
                // i should really check to make sure the exact transcriptions are in there, but that requires an equality comparer,
                // which requires a whole nested class, thanks to C#'s lack of anonymous classes that implement interfaces
            }
        }

        [TestMethod]
        public async Task TestPutTranscriptionNoContentStatusCode()
        {
            using (var context = new youtubeContext(options))
            {
                string newPhrase = "this is now a different phrase";
                Transcription transcription1 = context.Transcription.Where(x => x.Phrase == transcriptions[0].Phrase).Single();
                transcription1.Phrase = newPhrase;

                TranscriptionsController transcriptionsController = new TranscriptionsController(context);
                IActionResult result = await transcriptionsController.PutTranscription(transcription1.TranscriptionId, transcription1) as IActionResult;

                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(NoContentResult));
            }
        }


    }
}
