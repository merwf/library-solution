using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Library.Core.Entities;
using Library.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Library.Data.Repositories.Implementations
{
    public class BookRepository : IBookRepository
    {
        private readonly LibraryDbContext _context;

        public BookRepository(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<(List<Book> Items, int TotalCount)> GetAllAsync(string? search, int page, int pageSize)
        {
            var query = _context.Books.AsQueryable();

            // 1. Boş değilse Title veya Author üzerinde arama yap
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.Trim();
                query = query.Where(b => 
                    EF.Functions.Like(b.Title, $"%{searchTerm}%") || 
                    EF.Functions.Like(b.Author, $"%{searchTerm}%"));
            }

            // 2. Filtrelenmiş toplam kayıt sayısı (Arama sonucu yoksa 0 döner)
            var totalCount = await query.CountAsync();

            // 3. Pagination uygula
            var items = await query
                .OrderBy(b => b.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Book?> GetByIdAsync(int id)
        {
            return await _context.Books.FindAsync(id);
        }

        public async Task AddAsync(Book book)
        {
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(Book book)
        {
            var existing = await _context.Books.FindAsync(book.Id);
            if (existing == null) return false;

            existing.Title = book.Title;
            existing.Author = book.Author;
            existing.ISBN = book.ISBN;

            // Doğrudan 'IsAvailable =' yazmak yerine domain metodunu çağırıyoruz:
            if (book.IsAvailable)
                existing.MarkAsReturned();
            else
                existing.MarkAsBorrowed();

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return false;

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Books.AnyAsync(b => b.Id == id);
        }
    }
}
