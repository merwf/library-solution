// Library.API/Controllers/BooksController.cs
using Library.Core;
using Library.Data.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;

        public BooksController(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<BookDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBooks([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var (books, totalCount) = await _bookRepository.GetAllAsync(page, pageSize);

            var result = new PagedResult<BookDto>
            {
                Items = books.Select(b => new BookDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                    ISBN = b.ISBN,
                    IsAvailable = b.IsAvailable
                }).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookDto>> GetBook(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null)
            {
                return Problem(
                    detail: $"Id={id} olan kitap sistemde mevcut değil.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Kaynak Bulunamadı");
            }

            return Ok(new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                ISBN = book.ISBN,
                IsAvailable = book.IsAvailable
            });
        }

        [HttpPost]
        public async Task<ActionResult<BookDto>> PostBook(BookDto bookDto)
        {
            var book = new Book
            {
                Title = bookDto.Title,
                Author = bookDto.Author,
                ISBN = bookDto.ISBN,
                IsAvailable = true
            };

            await _bookRepository.AddAsync(book);

            bookDto.Id = book.Id;
            return CreatedAtAction(nameof(GetBook), new { id = book.Id }, bookDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutBook(int id, BookDto bookDto)
        {
            if (id != bookDto.Id)
            {
                return Problem(
                    detail: "URL'deki ID ile gönderilen gövdedeki (body) ID uyuşmuyor.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Geçersiz İstek");
            }

            var book = new Book
            {
                Id = bookDto.Id,
                Title = bookDto.Title,
                Author = bookDto.Author,
                ISBN = bookDto.ISBN,
                IsAvailable = bookDto.IsAvailable
            };

            var updated = await _bookRepository.UpdateAsync(book);
            if (!updated)
            {
                return Problem(
                    detail: $"Güncellenmek istenen Id={id} olan kitap bulunamadı.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Kaynak Bulunamadı");
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var deleted = await _bookRepository.DeleteAsync(id);
            if (!deleted)
            {
                return Problem(
                    detail: $"Silinmek istenen Id={id} olan kitap bulunamadı.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Kaynak Bulunamadı");
            }

            return NoContent();
        }
    }
}