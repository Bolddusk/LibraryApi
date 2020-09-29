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
            if (!_propertyCheckerService.TypeHasProperties<AuthorDto>
                (authorResourceParameters.Fields))
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

            //var previousPageLink = AuthorsFromRepo.HasPrevious ?
            //    CreateAuthorsResourceUri(authorResourceParameters,
            //    ResourceUriType.PreviousPage) : null;
            //var nextPageLink = AuthorsFromRepo.HasNext ?
            //    CreateAuthorsResourceUri(authorResourceParameters,
            //    ResourceUriType.NextPage) : null;

            var paginationMetaData = new
            { 
                totalCount = AuthorsFromRepo.TotalCount,
                pageSize = AuthorsFromRepo.PageSize,
                totalPages = AuthorsFromRepo.TotalPages,
                currentPage = AuthorsFromRepo.CurrentPage,
                //previousPageLink = previousPageLink,
                //nextPageLink = nextPageLink
            };

            Response.Headers.Add("X-Pagination",
                JsonSerializer.Serialize(paginationMetaData));

            var links = CreateLinksForAuthors(authorResourceParameters,
                AuthorsFromRepo.HasNext,AuthorsFromRepo.HasPrevious);
            var shapedAuthors = _mapper.Map<IEnumerable<AuthorDto>>(AuthorsFromRepo)
                .ShapeData(authorResourceParameters.Fields);

            var shapedAuthorsWithLinks = shapedAuthors.Select(author => {
                var authorAsDictionary = author as IDictionary<string, object>;
                var authorLinks = CreateLinksForAuthor((Guid)authorAsDictionary["Id"],null);
                authorAsDictionary.Add("links", authorLinks);
                return authorAsDictionary;
            });

            var linkedCollectionResource = new
            {
                value = shapedAuthorsWithLinks,
                links
            };
            return Ok(linkedCollectionResource);
            return Ok(_mapper.Map<IEnumerable<AuthorDto>>(AuthorsFromRepo)
                .ShapeData(authorResourceParameters.Fields));
        }
        [HttpGet("{authorId:guid}",Name ="GetAuthor")]
        public IActionResult GetAuthor(Guid authorId,string fields)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_propertyCheckerService.TypeHasProperties<AuthorDto>(fields)){
                return BadRequest();
            }
            var authorFromRepo = _courseLibraryRepository.GetAuthor(authorId);
            if (authorFromRepo == null)
                return NotFound();

            var links = CreateLinksForAuthor(authorId, fields);
            var linkedResourceToReturn =
                _mapper.Map<AuthorDto>(authorFromRepo).ShapeData(fields)
                as IDictionary<string, object>;

            linkedResourceToReturn.Add("links", links);
            return Ok(linkedResourceToReturn);

            //return Ok(_mapper.Map<AuthorDto>(authorFromRepo).ShapeData(fields));
        }

        [HttpPost(Name ="CreateAuthor")]
        public ActionResult<AuthorDto> CreateAuthor(AuthorForCreationDto author)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var authorEntity = _mapper.Map<Entities.Author>(author);
            _courseLibraryRepository.AddAuthor(authorEntity);
            _courseLibraryRepository.Save();

            var authorToReturn = _mapper.Map<AuthorDto>(authorEntity);
            var links = CreateLinksForAuthor(authorToReturn.Id, null);

            var linkedResourceToReturn = authorToReturn.ShapeData(null)
                as IDictionary<string, object>;
            linkedResourceToReturn.Add("links", links);
            return CreatedAtRoute("GetAuthor",new { id=authorToReturn.Id },
                authorToReturn);

        }

        [HttpDelete("{authorId}",Name ="DeleteAuthor")]
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
                case ResourceUriType.Current:
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

        private IEnumerable<LinkDto> CreateLinksForAuthor(Guid authorId,string fields)
        {
            var links = new List<LinkDto>();
            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(new LinkDto(
                    Url.Link("GetAuthor",new { authorId }),
                    "self",
                    "Get")
                    );
            }
            else
            {
                links.Add(new LinkDto(
                    Url.Link("GetAuthor",new { authorId,fields}),
                    "self",
                    "Get"
                    ));
            }

            links.Add(new LinkDto(Url.Link("DeleteAuthor",new { authorId}),
                "delete_author",
                "DELETE"));

            links.Add(new LinkDto(Url.Link("CreateCourseForAuthor",new { authorId}),
                "create_course_for_author",
                "POST"));

            links.Add(new LinkDto(Url.Link("GetCoursesForAuthor",new {authorId }),
                "courses",
                "GET"));
            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForAuthors(
            AuthorResourceParameters authorResourceParameters,
            bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            //self 
            links.Add(new LinkDto(CreateAuthorsResourceUri(authorResourceParameters, ResourceUriType.Current),
                "self", "GET"));
            if (hasPrevious)
               links.Add(new LinkDto(CreateAuthorsResourceUri(authorResourceParameters,
                    ResourceUriType.PreviousPage), "previousPage", "GET"));
            if (hasNext)
                links.Add(new LinkDto(CreateAuthorsResourceUri(authorResourceParameters,
                    ResourceUriType.NextPage),"nextPage","GET"));

            return links;
        }
    }
}
