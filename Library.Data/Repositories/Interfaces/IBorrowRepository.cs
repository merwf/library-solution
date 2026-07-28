using Library.Core.DTOs;
using Library.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Data.Repositories.Interfaces
{
    public interface IBorrowRepository
    {
        Task<BorrowRecord?> GetByIdWithBookAsync(int id);
        Task<List<BorrowRecordDto>> GetActiveBorrowsAsync();
        Task AddAsync(BorrowRecord record);
        Task SaveChangesAsync();
    }
}
