using Library.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Library.UI.Services
{
    public interface IBorrowService
    {
        Task<List<BorrowRecordDto>> GetActiveBorrowsAsync();
        Task<bool> BorrowBookAsync(object borrowRequest);
        Task<string> ReturnBookAsync(int id);
    }
}