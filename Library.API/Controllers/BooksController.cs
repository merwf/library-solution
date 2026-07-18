using Library.Core;
using Library.Data;
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

        // Program.cs'de tanımladığımız veritabanı bağlantısını buraya çağırıyoruz (Dependency Injection)
        public BooksController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: api/books -> Tüm kitapları listele
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetBooks()
        {
            return await _context.Books
                .Select(b => new BookDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                    ISBN = b.ISBN,
                    IsAvailable = b.IsAvailable
                }).ToListAsync();
        }

        // GET: api/books/{id} -> ID'ye göre kitap detayı getir
        [HttpGet("{id}")]
        public async Task<ActionResult<BookDto>> GetBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound("Kitap bulunamadı.");

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
            if (id != bookDto.Id) return BadRequest("ID uyuşmazlığı.");

            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound("Güncellenmek istenen kitap bulunamadı.");

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
                if (!_context.Books.Any(e => e.Id == id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/books/{id} -> Kitap sil
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound("Silinmek istenen kitap bulunamadı.");

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}