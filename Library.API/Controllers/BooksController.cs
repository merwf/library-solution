using Library.Business.Interfaces;
using Library.Core.Common;
using Library.Core.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<BookDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBooks([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _bookService.GetBooksAsync(page, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookDto>> GetBook(int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null)
            {
                return Problem(
                    detail: $"Id={id} olan kitap sistemde mevcut değil.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Kaynak Bulunamadı");
            }

            return Ok(book);
        }

        [HttpPost]
        public async Task<ActionResult<BookDto>> PostBook(BookDto bookDto)
        {
            var createdBook = await _bookService.CreateBookAsync(bookDto);
            return CreatedAtAction(nameof(GetBook), new { id = createdBook.Id }, createdBook);
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

            var updated = await _bookService.UpdateBookAsync(id, bookDto);
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
            var deleted = await _bookService.DeleteBookAsync(id);
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