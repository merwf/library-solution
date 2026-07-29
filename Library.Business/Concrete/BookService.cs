using Library.Business.Interfaces;
using Library.Core.Common;
using Library.Core.DTOs;
using Library.Core.Entities;
using Library.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Business.Concrete
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;

        public BookService(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public async Task<PagedResult<BookDto>> GetBooksAsync(string? search, int page, int pageSize)
        {
            var (items, totalCount) = await _bookRepository.GetAllAsync(search, page, pageSize);

            var dtos = items.Select(b => new BookDto
            {
                Id = b.Id,
                Title = b.Title,
                Author = b.Author,
                ISBN = b.ISBN,
                IsAvailable = b.IsAvailable
            }).ToList();

            return new PagedResult<BookDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<BookDto?> GetBookByIdAsync(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null) return null;

            return new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                ISBN = book.ISBN,
                IsAvailable = book.IsAvailable
            };
        }

        public async Task<BookDto> CreateBookAsync(BookDto bookDto)
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
            return bookDto;
        }

        public async Task<bool> UpdateBookAsync(int id, BookDto bookDto)
        {
            if (id != bookDto.Id) return false;

            var book = new Book
            {
                Id = bookDto.Id,
                Title = bookDto.Title,
                Author = bookDto.Author,
                ISBN = bookDto.ISBN
            };

            if (bookDto.IsAvailable)
                book.MarkAsReturned();
            else
                book.MarkAsBorrowed();

            return await _bookRepository.UpdateAsync(book);
        }

        public async Task<bool> DeleteBookAsync(int id)
        {
            return await _bookRepository.DeleteAsync(id);
        }
    }
}
