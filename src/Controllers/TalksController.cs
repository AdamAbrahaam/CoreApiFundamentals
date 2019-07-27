using System;
using System.Threading.Tasks;
using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace CoreCodeCamp.Controllers
{
    // Association controller => is associated another part of URI (ex. camps/{moniker}/talks )
    [ApiController]
    [Route("api/camps/{moniker}/talks")]
    public class TalksController : ControllerBase
    {
        private readonly ICampRepository _repository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        public TalksController(ICampRepository repository, IMapper mapper, LinkGenerator generator)
        {
            _repository = repository;
            _mapper = mapper;
            _linkGenerator = generator;
        }

        [HttpGet]
        public async Task<ActionResult<TalkModel[]>> Get(string moniker) // moniker from URI automatically binds to param
        {
            try
            {
                var talks = await _repository.GetTalksByMonikerAsync(moniker, true);

                return _mapper.Map<TalkModel[]>(talks);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to get Talks!");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TalkModel>> Get(string moniker, int id)
        {
            try
            {
                var talk = await _repository.GetTalkByMonikerAsync(moniker, id, true);
                if (talk == null) return NotFound("Talk Not Found!");

                return _mapper.Map<TalkModel>(talk);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to get Talks!");
            }
        }

        [HttpPost]
        public async Task<ActionResult<TalkModel>> Post(string moniker, TalkModel model)
        {
            try
            {
                // Example post in Postman
                // No need to assign TalkId, its automatic
                // SpeakerId is enough, rest is assigned automatically from ID
                //
                // {
                //  "title": "Building APIs",
                //  "abstract": "Thinking of good sample data examples is tiring.",
                //  "level": 2300,
                //  "speaker": {
                //      "speakerId": 1
                //      }
                //  }

                var camp = await _repository.GetCampAsync(moniker);
                if (camp == null) return NotFound("Camp does not exists!");

                var talk = _mapper.Map<Talk>(model);
                talk.Camp = camp;   // Assign talk a camp - makes foreign key relationship automatically

                if (model.Speaker == null) return BadRequest("Speaker ID is required!"); // Not Not Found(404), bad model
                var speaker = await _repository.GetSpeakerAsync(model.Speaker.SpeakerId);
                if (speaker == null) return NotFound("Speaker not found!");
                talk.Speaker = speaker; // automatic foreign key relationship

                _repository.Add(talk);

                if (await _repository.SaveChangesAsync())
                {
                    var url = _linkGenerator.GetPathByAction(HttpContext,
                        "Get",
                        values: new {moniker, id = talk.TalkId});

                    return Created(url, _mapper.Map<TalkModel>(talk));
                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to get Talks!");
            }

            return BadRequest();
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<TalkModel>> Put(string moniker, int id, TalkModel model)
        {
            try
            {
                // PUT needs TalkId when updating data, not liek POST !!!!!

                var talk = await _repository.GetTalkByMonikerAsync(moniker, id, true);
                if (talk == null) return NotFound("Talk not found!");

                _mapper.Map(model, talk); // This will map anything in the model to anything in the talk, including speaker and camp, need to adjust in profile

                if (model.Speaker != null)
                {
                    var speaker = await _repository.GetSpeakerAsync(model.Speaker.SpeakerId);
                    if (speaker != null)
                    {
                        talk.Speaker = speaker;
                    }
                }

                if (await _repository.SaveChangesAsync())
                {
                    return _mapper.Map<TalkModel>(talk);
                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to get Talks!");
            }

            return BadRequest();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(string moniker, int id)
        {
            try
            {
                var talk = await _repository.GetTalkByMonikerAsync(moniker, id);
                if (talk == null) return NotFound("Talk not found!");

                _repository.Delete(talk);

                if (await _repository.SaveChangesAsync())
                {
                    return Ok();
                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to get Talks!");
            }

            return BadRequest();
        }
    }
}