using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Library.API.Entities;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Produces("application/json")]
    [Route("api/AuthorCollections")]
    public class AuthorCollectionsController : Controller
    {
        private readonly ILibraryRepository _libraryRepository;

        public AuthorCollectionsController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        public IActionResult CreateAuthorCollections([FromBody] IEnumerable<AuthorCreationDto> authors)
        {
            if(authors ==null)
            {
                return BadRequest();
            }

            var authorEntities = AutoMapper.Mapper.Map<IEnumerable<Author>>(authors);

            foreach (var author in authorEntities)
            {
                _libraryRepository.AddAuthor(author);
            }

            if(!_libraryRepository.Save())
            {
                throw new Exception("Creating author collection has failed");
            }

            return Ok();
        }
    }
}