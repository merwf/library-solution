using Library.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Library.UI.Services
{
    public interface IBookService
    {
        Task<List<BookDto>> GetBooksAsync();
        Task<bool> AddBookAsync(BookDto book);
        Task<bool> UpdateBookAsync(int id, BookDto book);
        Task<bool> DeleteBookAsync(int id);
    }
}