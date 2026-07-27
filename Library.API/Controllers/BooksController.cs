using Library.Core;
using Library.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        public BooksController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: api/books?page=1&pageSize=10 -> Sayfalanmış kitap listesini getirir
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<BookDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBooks([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var totalCount = await _context.Books.CountAsync();

            var items = await _context.Books
                .OrderBy(b => b.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BookDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                    ISBN = b.ISBN,
                    IsAvailable = b.IsAvailable
                })
                .ToListAsync();

            var result = new PagedResult<BookDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return Ok(result);
        }

        // GET: api/books/{id} -> ID'ye göre kitap detayı getir
        [HttpGet("{id}")]
        public async Task<ActionResult<BookDto>> GetBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return Problem(
                    detail: $"Id={id} olan kitap sistemde mevcut değil.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Kaynak Bulunamadı");
            }

            var bookDto = new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                ISBN = book.ISBN,
                IsAvailable = book.IsAvailable
            };

            return Ok(bookDto);
        }

        // POST: api/books -> Yeni kitap ekle
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
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            bookDto.Id = book.Id;
            return CreatedAtAction(nameof(GetBook), new { id = book.Id }, bookDto);
        }

        // PUT: api/books/{id} -> Kitap güncelle
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

            var book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                return Problem(
                    detail: $"Güncellenmek istenen Id={id} olan kitap bulunamadı.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Kaynak Bulunamadı");
            }

            book.Title = bookDto.Title;
            book.Author = bookDto.Author;
            book.ISBN = bookDto.ISBN;
            book.IsAvailable = bookDto.IsAvailable;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Books.Any(e => e.Id == id))
                {
                    return Problem(
                        detail: "Güncelleme sırasında çakışma oluştu. Kitap silinmiş olabilir.",
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Kaynak Bulunamadı");
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/books/{id} -> Kitap sil
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return Problem(
                    detail: $"Silinmek istenen Id={id} olan kitap bulunamadı.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Kaynak Bulunamadı");
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}