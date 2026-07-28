using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Library.Core.Entities;

namespace Library.Data.Repositories.Interfaces
{
    public interface IBookRepository
    {
        Task<(List<Book> Items, int TotalCount)> GetAllAsync(int page, int pageSize);
        Task<Book?> GetByIdAsync(int id);
        Task AddAsync(Book book);
        Task<bool> UpdateAsync(Book book);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
