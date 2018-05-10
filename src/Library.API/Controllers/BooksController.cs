using AutoMapper;
using Library.API.Entities;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Controllers
{
    [Route("api/authors/{authorId}/books")]
    public class BooksController : Controller
    {
        private ILibraryRepository _libraryRepository;

        public BooksController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        [HttpGet()]
        public IActionResult GetBooksForAuthor(Guid authorId)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var booksForAuthorFromRepo = _libraryRepository.GetBooksForAuthor(authorId);

            var booksForAuthor = Mapper.Map<IEnumerable<BookDto>>(booksForAuthorFromRepo);

            return Ok(booksForAuthor);
        }

        [HttpGet("{id}", Name = "GetBookForAuthor")]
        public IActionResult GetBookForAuthor(Guid authorId, Guid id)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookForAuthorFromRepo == null)
            {
                return NotFound();
            }

            var bookForAuthor = Mapper.Map<BookDto>(bookForAuthorFromRepo);
            return Ok(bookForAuthor);
       }

        [HttpPost]
        public IActionResult CreateBookForAuthor(Guid authorId,[FromBody]BookCreationDto book)
        {
            if (book == null)
                return BadRequest();
            if (!_libraryRepository.AuthorExists(authorId))
                return NotFound();
            var bookEntity = Mapper.Map<Book>(book);

            _libraryRepository.AddBookForAuthor(authorId, bookEntity);
            if(!_libraryRepository.Save())
            {
                throw new Exception($"unable to save book for {authorId}. Please try again");
            }

            var bookToReturn = Mapper.Map<BookDto>(bookEntity);

            return CreatedAtRoute("GetBookForAuthor", new { authorId = bookToReturn.AuthorId, id = bookToReturn.Id }, bookToReturn);

        }

        [HttpDelete("{id}")]
        public IActionResult DeleteBookForAuthor(Guid authorId, Guid id)
        {
            if(!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

           var bookEntity= _libraryRepository.GetBookForAuthor(authorId,id);

            if(bookEntity==null)
            {
                return NotFound();
            }

            _libraryRepository.DeleteBook(bookEntity);

            if(!_libraryRepository.Save())
            {
                throw new Exception($"Unable to delete book for author {authorId}");
            }

            return NoContent();
        }

        [HttpPut("{id}")]
        public IActionResult UpdateBookForAuthor(Guid authorId, Guid id, [FromBody] BookForUpdateDto updateBookDto)
        {
            if(updateBookDto==null)
            {
                return BadRequest();
            }

            if(!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);

            if(bookFromRepo==null)
            {
                var bookToAdd = Mapper.Map<Book>(updateBookDto);
                bookToAdd.Id = id;

                _libraryRepository.AddBookForAuthor(authorId, bookToAdd);

                if(!_libraryRepository.Save())
                {
                    throw new Exception($"Upserting book for author {authorId} failed on save");
                }

                var bookDtoToReturn = Mapper.Map<BookDto>(bookToAdd);

                return CreatedAtRoute("GetBookForAuthor", new { authorId = authorId, id = bookToAdd.Id }, bookDtoToReturn);

            }

            Mapper.Map(updateBookDto, bookFromRepo);

            _libraryRepository.UpdateBookForAuthor(bookFromRepo);

            if(!_libraryRepository.Save())
            {
                throw new Exception("unable to update book");
            }

            return NoContent();
        }
    }
}
