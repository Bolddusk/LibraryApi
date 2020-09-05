using AutoMapper;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/authors/{authorId:Guid}/courses")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository;
        private readonly IMapper _mapper;

        public CoursesController(ICourseLibraryRepository courseLibraryRepository
            , IMapper mapper)
        {
            _courseLibraryRepository = courseLibraryRepository ??
                throw new ArgumentNullException(nameof(courseLibraryRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public ActionResult<IEnumerable<CourseDto>> GetCourses(Guid authorId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (!_courseLibraryRepository.AuthorExists(authorId))
                return NotFound();

            var coursesFromRepo = _courseLibraryRepository.GetCourses(authorId);
            if (coursesFromRepo == null)
                return NoContent();
            return Ok(_mapper.Map<IEnumerable<CourseDto>>(coursesFromRepo));

        }
        [HttpGet("{courseId}")]
        public IActionResult GetCourse(Guid authorId,Guid courseId)
        {
            if (!_courseLibraryRepository.AuthorExists(authorId))
                return NotFound();

            var courseFromRepo = _courseLibraryRepository.GetCourse(authorId, courseId);
            if (courseFromRepo == null)
                return NotFound();

            return Ok(_mapper.Map<CourseDto>(courseFromRepo));
        }
    }
}
