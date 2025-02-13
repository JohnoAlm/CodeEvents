﻿using AutoMapper;
using CodeEvents.Api.Core.Entities;
using CodeEvents.Api.Data;
using CodeEvents.Api.Data.Repositories;
using CodeEvents.Common.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CodeEvents.Api.Controllers
{
    [Route("api/events/{name}/lectures")]
    [ApiController]
    public class LecturesController : ControllerBase
    {
        private readonly CodeEventsApiContext db;
        private readonly IMapper mapper;
        private readonly UnitOfWork uow;

        public LecturesController(CodeEventsApiContext context, IMapper mapper)
        {
            db = context;
            this.mapper = mapper;
            uow = new UnitOfWork(db);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LectureDto>>> GetLecture(string name)
        {
            if ((await uow.CodeEventRepository.GetAsync(name)) is null) 
                return NotFound(new { Error = new[] { $"CodeEvent with name: [{name}] not found" } }); 

            var lectures = await uow.LectureRepository.GetLecturesForEvent(name);
            return Ok(mapper.Map<IEnumerable<LectureDto>>(lectures));
        }

        [HttpGet("{id}")]
        //[Route("{id}")]
        public async Task<ActionResult<LectureDto>> GetLecture(string name, int id)
        {
            if ((await uow.CodeEventRepository.GetAsync(name)) is null) 
                return NotFound(new { Error = new[] { $"CodeEvent with name: [{name}] not found" } });

            var lecture = await uow.LectureRepository.GetLectureAsync(id);

            if (lecture is null) return BadRequest();

            return Ok(mapper.Map<LectureDto>(lecture));
        }

        [HttpPost]
        public async Task<ActionResult<LectureDto>> Create(string name, LectureCreateDto dto)
        {
           var codeEvent = await uow.CodeEventRepository.GetAsync(name);
            if(codeEvent is null)
                return NotFound(new { Error = new[] { $"CodeEvent with name: [{name}] not found" } });

            var lecture = mapper.Map<Lecture>(dto);
            lecture.CodeEvent = codeEvent;
            await uow.LectureRepository.AddAsync(lecture);

            await uow.CompleteAsync();
            var model = mapper.Map<LectureDto>(lecture);
            return CreatedAtAction(nameof(GetLecture), new { name = codeEvent.Name, id = model.Id }, model);

        }


    }
}
