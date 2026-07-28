using Library.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Library.UI.HttpServices.Interfaces
{
    public interface IBorrowService
    {
        Task<List<BorrowRecordDto>> GetActiveBorrowsAsync();
        Task<bool> BorrowBookAsync(object borrowRequest);
        Task<string> ReturnBookAsync(int id);
    }
}