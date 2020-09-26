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
using System.Text.Json;
using System.Threading.Tasks;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/authors")]
    public class AuthorsController : ControllerBase
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository;
        private readonly IMapper _mapper;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly IPropertyCheckerService _propertyCheckerService;

        public AuthorsController(ICourseLibraryRepository courseLibraryRepository
            ,IMapper mapper, IPropertyMappingService propertyMappingService
            ,IPropertyCheckerService propertyCheckerService)
        {
            _courseLibraryRepository = courseLibraryRepository ??
                throw new ArgumentNullException(nameof(courseLibraryRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
            _propertyMappingService = propertyMappingService ??
                throw new ArgumentNullException(nameof(propertyMappingService));
            _propertyCheckerService = propertyCheckerService;
        }
        [HttpGet(Name = "GetAuthors")]
        [HttpHead]
        //public ActionResult<IEnumerable<AuthorDto>> GetAuthors(        
        public IActionResult GetAuthors(
            [FromQuery]AuthorResourceParameters authorResourceParameters)
        {
            if(!_propertyMappingService.ValidMappingExistsFor<AuthorDto,Entities.Author>
                (authorResourceParameters.OrderBy))
            {
                return BadRequest();
            }

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

            var previousPageLink = AuthorsFromRepo.HasPrevious ?
                CreateAuthorsResourceUri(authorResourceParameters,
                ResourceUriType.PreviousPage) : null;
            var nextPageLink = AuthorsFromRepo.HasNext ?
                CreateAuthorsResourceUri(authorResourceParameters,
                ResourceUriType.NextPage) : null;

            var paginationMetaData = new
            { 
                totalCount = AuthorsFromRepo.TotalCount,
                pageSize = AuthorsFromRepo.PageSize,
                totalPages = AuthorsFromRepo.TotalPages,
                currentPage = AuthorsFromRepo.CurrentPage,
                previousPageLink = previousPageLink,
                nextPageLink = nextPageLink
            };

            Response.Headers.Add("X-Pagination",
                JsonSerializer.Serialize(paginationMetaData));

            return Ok(_mapper.Map<IEnumerable<AuthorDto>>(AuthorsFromRepo)
                .ShapeData(authorResourceParameters.Fields));
        }
        [HttpGet("{id:guid}",Name ="GetAuthor")]
        public IActionResult GetAuthor(Guid id,string fields)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (!_propertyCheckerService.TypeHasProperties<AuthorDto>(fields)){
                return BadRequest();
            }
            var author = _courseLibraryRepository.GetAuthor(id);
            if (author == null)
                return NotFound();

            return Ok(_mapper.Map<AuthorDto>(author).ShapeData(fields));
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

        [HttpDelete("{authorId}")]
        public ActionResult DeleteAuthor(Guid authorId)
        {
            var authorFromRepo = _courseLibraryRepository.GetAuthor(authorId);
            if (authorFromRepo == null)
                return NotFound();

            _courseLibraryRepository.DeleteAuthor(authorFromRepo);
            _courseLibraryRepository.Save();

            return NoContent();
        }

        [HttpOptions]
        public IActionResult GetAuthorsOption()
        {
            Response.Headers.Add("Allow", "GET,OPTIONS,POST");
            return Ok();
        }

        private string CreateAuthorsResourceUri(
            AuthorResourceParameters authorResourceParameters,
            ResourceUriType type
            )
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return Url.Link("GetAuthors",
                        new
                        {
                            fields = authorResourceParameters.Fields,
                            orderBy = authorResourceParameters.OrderBy,
                            pageNumber = authorResourceParameters.PageNumber - 1,
                            pageSize = authorResourceParameters.PageSize,
                            mainCategory = authorResourceParameters.mainCategory,
                            searchQuery = authorResourceParameters.searchQuery
                        });
                case ResourceUriType.NextPage:
                    return Url.Link("GetAuthors",
                        new {
                            fields = authorResourceParameters.Fields,
                            orderBy = authorResourceParameters.OrderBy,
                            pageNumber = authorResourceParameters.PageNumber + 1,
                            pageSize = authorResourceParameters.PageSize,
                            mainCategory = authorResourceParameters.mainCategory,
                            searchQuery = authorResourceParameters.searchQuery
                    });
                default:
                    return Url.Link("GetAuthors",new {
                        fields = authorResourceParameters.Fields,
                        orderBy = authorResourceParameters.OrderBy,
                        pageNumber = authorResourceParameters.PageNumber,
                        pageSize = authorResourceParameters.PageSize,
                        mainCategory = authorResourceParameters.mainCategory,
                        searchQuery = authorResourceParameters.searchQuery
                    });
            }    
        }
    }
}
