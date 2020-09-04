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

        public AuthorsController(ICourseLibraryRepository courseLibraryRepository)
        {
            _courseLibraryRepository = courseLibraryRepository ??
                throw new ArgumentNullException(nameof(ICourseLibraryRepository));
        }
        [HttpGet]
        public IActionResult GetAuthors()
        {
            var Authors = _courseLibraryRepository.GetAuthors();
            return Ok(Authors);
        }
        [HttpGet("{id:guid}")]
        public IActionResult GetAuthor(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var author = _courseLibraryRepository.GetAuthor(id);
            if (author == null)
                return NotFound();

            return Ok(author);
        }
    }
}
