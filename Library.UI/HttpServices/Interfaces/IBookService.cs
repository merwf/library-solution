using Library.Core.Common;
using Library.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Library.UI.HttpServices.Interfaces
{
    public interface IBookService
    {
        Task<PagedResult<BookDto>> GetBooksAsync(int page = 1, int pageSize = 10);
        Task<bool> AddBookAsync(BookDto book);
        Task<bool> UpdateBookAsync(int id, BookDto book);
        Task<bool> DeleteBookAsync(int id);
    }
}