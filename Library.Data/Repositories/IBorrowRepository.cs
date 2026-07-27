using Library.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Data.Repositories
{
    public interface IBorrowRepository
    {
        Task<BorrowRecord?> GetByIdWithBookAsync(int id);
        Task<List<BorrowRecordDto>> GetActiveBorrowsAsync();
        Task AddAsync(BorrowRecord record);
        Task SaveChangesAsync();
    }
}
