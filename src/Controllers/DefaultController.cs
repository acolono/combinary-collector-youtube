using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using YoutubeCollector.Db;
using YoutubeCollector.Lib;

namespace YoutubeCollector.Controllers {
    [Route("/")]
    [ApiController]
    public class DefaultController : ControllerBase {
        private readonly SettingsProvider _settingsProvider;
        private readonly Repository _repository;
        private readonly ILogger<DefaultController> _logger;
        private readonly IConfiguration _configuration;

        // GET api/values
        [HttpGet("/")]
        public ActionResult<string> Get() {

            var c = string.Join(Environment.NewLine, _configuration.AsEnumerable().Select(kp => $"{kp.Key}={kp.Value}"));
            c += Environment.NewLine + "PgConnectionString" + _settingsProvider.PgConnectionString;
            c += Environment.NewLine + "PgHost" + _settingsProvider.PgHost;
            _logger.LogCritical(c);

            var uh = new UrlHelper(ControllerContext);
            var link = uh.Action(nameof(GetVideoDetails), new {videoId="VIDEO_ID"});
            return $"try {link}";
        }

        public DefaultController(SettingsProvider settingsProvider, Repository repository, ILogger<DefaultController> logger, IConfiguration configuration) {
            _settingsProvider = settingsProvider;
            _repository = repository;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet("/api/get-video-details/{videoId}"), Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<Models.Video>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetVideoDetails([Required] string videoId, CancellationToken ct = default(CancellationToken)) {
            try {
                using (var api = new YoutubeApi(_settingsProvider.ApiKeys.Next())) {
                    var vid = await api.GetVideoDetails(videoId, ct);
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
