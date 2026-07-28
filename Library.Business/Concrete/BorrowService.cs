using Library.Business.Interfaces;
using Library.Core.DTOs;
using Library.Core.Entities;
using Library.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Library.Business.Concrete
{
    public class BorrowService : IBorrowService
    {
        private readonly IBorrowRepository _borrowRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly IPenaltyFeeCalculator _calculator;

        public BorrowService(
            IBorrowRepository borrowRepository,
            IBookRepository bookRepository,
            IMemberRepository memberRepository,
            IPenaltyFeeCalculator calculator)
        {
            _borrowRepository = borrowRepository;
            _bookRepository = bookRepository;
            _memberRepository = memberRepository;
            _calculator = calculator;
        }

        public async Task<BorrowResultDto> BorrowBookAsync(BorrowRequestDto request)
        {
            var book = await _bookRepository.GetByIdAsync(request.BookId);
            if (book == null)
            {
                return new BorrowResultDto
                {
                    IsSuccess = false,
                    StatusCode = 404,
                    ErrorMessage = $"Id={request.BookId} olan kitap bulunamadı."
                };
            }

            if (!book.IsAvailable)
            {
                return new BorrowResultDto
                {
                    IsSuccess = false,
                    StatusCode = 409,
                    ErrorMessage = "Bu kitap şu an bir başkasında olduğu için ödünç verilemez."
                };
            }

            var member = await _memberRepository.GetByIdAsync(request.MemberId);
            if (member == null)
            {
                return new BorrowResultDto
                {
                    IsSuccess = false,
                    StatusCode = 404,
                    ErrorMessage = $"Id={request.MemberId} olan üye bulunamadı."
                };
            }

            var record = new BorrowRecord
            {
                BookId = request.BookId,
                MemberId = request.MemberId,
                CountryCode = request.CountryCode,
                BorrowDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(10)
            };

            // RICH DOMAIN MODEL:
            // Doğrudan book.IsAvailable = false yazmak yerine kitabın kendi domain davranışını çağırıyoruz.
            book.MarkAsBorrowed();

            await _borrowRepository.AddAsync(record);

            return new BorrowResultDto
            {
                IsSuccess = true,
                RecordId = record.Id,
                DueDate = record.DueDate
            };
        }

        public async Task<ReturnResultDto> ReturnBookAsync(int recordId)
        {
            var record = await _borrowRepository.GetByIdWithBookAsync(recordId);

            if (record == null)
            {
                return new ReturnResultDto
                {
                    IsSuccess = false,
                    StatusCode = 404,
                    ErrorMessage = $"Id={recordId} olan ödünç kaydı bulunamadı."
                };
            }

            if (record.ReturnDate != null)
            {
                return new ReturnResultDto
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    ErrorMessage = "Bu ödünç kaydına ait kitap daha önce zaten iade edilmiş."
                };
            }

            DateTime returnDate = DateTime.Now;
            string dueDateStr = record.DueDate.ToString("dd.MM.yyyy");
            string returnDateStr = returnDate.ToString("dd.MM.yyyy");

            var penalty = _calculator.Calculate(record.CountryCode, dueDateStr, returnDateStr);

            if (penalty.IsError)
            {
                return new ReturnResultDto
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    ErrorMessage = penalty.ErrorMessage
                };
            }

            // RICH DOMAIN MODEL:
            // İade sürecini, ceza tutarını ve bağlı kitabın IsAvailable durumunu 
            // BorrowRecord nesnesi içindeki CompleteReturn metodu ile yönetiyoruz.
            record.CompleteReturn(returnDate, penalty.Amount);

            await _borrowRepository.SaveChangesAsync();

            return new ReturnResultDto
            {
                IsSuccess = true,
                ReturnDate = record.ReturnDate,
                FormattedPenaltyResult = penalty.FormattedResult
            };
        }

        public async Task<List<BorrowRecordDto>> GetActiveBorrowsAsync()
        {
            return await _borrowRepository.GetActiveBorrowsAsync();
        }
    }
}