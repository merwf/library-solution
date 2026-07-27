using Library.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Business
{
    public interface IBorrowService
    {
        Task<BorrowResultDto> BorrowBookAsync(BorrowRequestDto request);
        Task<ReturnResultDto> ReturnBookAsync(int recordId);
        Task<List<BorrowRecordDto>> GetActiveBorrowsAsync();
    }

    public class BorrowResultDto
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public int RecordId { get; set; }
        public DateTime DueDate { get; set; }
    }

    public class ReturnResultDto
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string FormattedPenaltyResult { get; set; } = string.Empty;
    }
}
