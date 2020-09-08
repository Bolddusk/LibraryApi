using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.ResourceParameters;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/authors")]
    public class AuthorsController : ControllerBase
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository;
        private readonly IMapper _mapper;

        public AuthorsController(ICourseLibraryRepository courseLibraryRepository
            ,IMapper mapper)
        {
            _courseLibraryRepository = courseLibraryRepository ??
                throw new ArgumentNullException(nameof(courseLibraryRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
        }
        [HttpGet]
        [HttpHead]
        public ActionResult<IEnumerable<AuthorDto>> GetAuthors(
            [FromQuery]AuthorResourceParameters authorResourceParameters)
        {
            var AuthorsFromRepo = _courseLibraryRepository.GetAuthors(authorResourceParameters);
            //var Authors = new List<AuthorDto>();
            //foreach (var author in AuthorsFromRepo)
            //{
            //    Authors.Add(new AuthorDto()
            //    {
            //        Id = author.Id,
            //        Name = $"{author.FirstName} {author.LastName}",
            //        Age = author.DateOfBirth.GetCurrentAge()
            //    });
            //}
            return Ok(_mapper.Map<IEnumerable<AuthorDto>>(AuthorsFromRepo));
        }
        [HttpGet("{id:guid}",Name ="GetAuthor")]
        public IActionResult GetAuthor(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var author = _courseLibraryRepository.GetAuthor(id);
            if (author == null)
                return NotFound();

            return Ok(_mapper.Map<AuthorDto>(author));
        }

        [HttpPost]
        public ActionResult<AuthorDto> CreateAuthor(AuthorForCreationDto author)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var authorEntity = _mapper.Map<Entities.Author>(author);
            _courseLibraryRepository.AddAuthor(authorEntity);
            _courseLibraryRepository.Save();

            var authorToReturn = _mapper.Map<AuthorDto>(authorEntity);
            return CreatedAtRoute("GetAuthor",new { id=authorToReturn.Id },
                authorToReturn);

        }

        [HttpOptions]
        public IActionResult GetAuthorsOption()
        {
            Response.Headers.Add("Allow", "GET,OPTIONS,POST");
            return Ok();
        }
    }
}
