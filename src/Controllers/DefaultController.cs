using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using YoutubeCollector.Db;
using YoutubeCollector.Lib;

namespace YoutubeCollector.Controllers {
    [Route("/")]
    [ApiController]
    public class DefaultController : Controller {
        private readonly SettingsProvider _settingsProvider;
        private readonly Repository _repository;

        //TODO: swagger
        //[HttpGet("/"), ApiExplorerSettings(IgnoreApi = true)]
        //public IActionResult Index() => Redirect("~/swagger");

        public DefaultController(SettingsProvider settingsProvider, Repository repository) {
            _settingsProvider = settingsProvider;
            _repository = repository;
        }

        [HttpGet("/api/get-video-details/{videoIdOrUrl}"), Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<Models.Video>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetVideoDetails([Required] string videoIdOrUrl, CancellationToken ct = default(CancellationToken)) {
            try {
                using (var api = new YoutubeApi(_settingsProvider.ApiKeys.Next())) {
                    var vidId = new VideoUrlParser().GetVideoId(videoIdOrUrl);
                    var vid = await api.GetVideoDetails(vidId, ct);
                    var vids = vid.Items.Select(v => v.MapToDbEntity()).ToList();
                    if (!vids.Any()) {
                        return NotFound(new ProblemDetails {
                            Status = StatusCodes.Status404NotFound,
                            Title = "Not Found",
                        });
                    }
                    await _repository.SaveOrUpdate(vids);
                    return Ok(vids);
                }
            }
            catch (Exception e) {
                return BadRequest(new ProblemDetails {
                    Status = StatusCodes.Status400BadRequest,
                    Title = e.Message,
                    Type = e.GetType().FullName,
                });
            }
        }
    }
}
