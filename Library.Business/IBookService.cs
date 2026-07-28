using Library.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Business
{
    public interface IBookService
    {
        Task<PagedResult<BookDto>> GetBooksAsync(int page, int pageSize);
        Task<BookDto?> GetBookByIdAsync(int id);
        Task<BookDto> CreateBookAsync(BookDto bookDto);
        Task<bool> UpdateBookAsync(int id, BookDto bookDto);
        Task<bool> DeleteBookAsync(int id);
    }
}
