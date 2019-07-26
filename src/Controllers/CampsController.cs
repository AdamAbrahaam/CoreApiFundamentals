﻿using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace CoreCodeCamp.Controllers
{
    [Route("api/[controller]")] // [controller] gets the name from class name before Controller

    [ApiController]             // Model Binding, binds body to model, ex. POST in JSON without type is bound to param model
                                // Sends validation(Data Annotations in Models) error handling automatic, error in response body
    public class CampsController : ControllerBase
    {
        private readonly ICampRepository _repository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;  // Links(gets) the location of the generated(POST) object

        public CampsController(ICampRepository repository, IMapper mapper, LinkGenerator linkGenerator)
        {
            _repository = repository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }
        [HttpGet]
        public async Task<ActionResult<CampModel[]>> Get(bool includeTalks = false) // queries in params
        {
            try
            {
                var results = await _repository.GetAllCampsAsync(includeTalks);

                return _mapper.Map<CampModel[]>(results);
            }
            catch(Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure!");
            }
        }

        [HttpGet("{moniker}")]
        public async Task<ActionResult<CampModel>> Get(string moniker)
        {
            try
            {
                var results = await _repository.GetCampAsync(moniker);

                if (results == null) return NotFound();

                return _mapper.Map<CampModel>(results);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure!");
            }
        }

        public async Task<ActionResult<CampModel>> Post(CampModel model)
        {
            try
            {
                // Model Validation, unique moniker example     -   Required, etc. Data Annotations in Models
                var exists = _repository.GetCampAsync(model.Moniker);
                if (exists != null)
                {
                    return BadRequest("Moniker already exists!");
                }

                var location = _linkGenerator.GetPathByAction(
                    "Get",                // Our Get method from above
                    "Camps",    // Controller
                    new {moniker = model.Moniker}); // Get method param

                if (string.IsNullOrWhiteSpace(location))
                {
                    return BadRequest("Could not use current moniker!");
                }

                var camp = _mapper.Map<Camp>(model);
                _repository.Add(camp);

                if (await _repository.SaveChangesAsync())
                {
                    return Created(location, _mapper.Map<CampModel>(camp));   // Use created when POST, Created needs the URI of how to GET the created object and the Created result mapped back
                                                                              // Location URI can be hardcoded without linkGen, but we might change the URI in the future and it wont work
                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure!");
            }

            return BadRequest(); // Something wrong happend
        }
    }
}