using Library.Business;
using Library.Core;
using Library.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BorrowController : ControllerBase
    {
        private readonly LibraryDbContext _context;
        private readonly IPenaltyFeeCalculator _calculator;

        public BorrowController(LibraryDbContext context, IPenaltyFeeCalculator calculator)
        {
            _context = context;
            _calculator = calculator;
        }

        // POST: api/borrow -> Kitap ödünç al
        [HttpPost]
        public async Task<IActionResult> BorrowBook([FromBody] BorrowRequestDto request)
        {
            var book = await _context.Books.FindAsync(request.BookId);
            if (book == null)
            {
                return Problem(
                    detail: $"Id={request.BookId} olan kitap bulunamadı.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Kaynak Bulunamadı");
            }

            if (!book.IsAvailable)
            {
                return Problem(
                    detail: "Bu kitap şu an bir başkasında olduğu için ödünç verilemez.",
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Çakışma Durumu");
            }

            var member = await _context.Members.FindAsync(request.MemberId);
            if (member == null)
            {
                return Problem(
                    detail: $"Id={request.MemberId} olan üye bulunamadı.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Kaynak Bulunamadı");
            }

            var record = new BorrowRecord
            {
                BookId = request.BookId,
                MemberId = request.MemberId,
                CountryCode = request.CountryCode,
                BorrowDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(10),
                ReturnDate = null,
                ComputedPenaltyFee = 0,
                IsPenaltyPaid = false
            };

            book.IsAvailable = false;
            _context.BorrowRecords.Add(record);
            await _context.SaveChangesAsync();

            return Created($"api/borrow/{record.Id}", new
            {
                Message = "Kitap başarıyla ödünç verildi.",
                RecordId = record.Id,
                TeslimTarihi = record.DueDate
            });
        }

        // POST: api/borrow/{id}/return -> Kitap iade et
        [HttpPost("{id}/return")]
        public async Task<IActionResult> ReturnBook(int id)
        {
            var record = await _context.BorrowRecords
                .Include(br => br.Book)
                .FirstOrDefaultAsync(br => br.Id == id);

            if (record == null)
            {
                return Problem(
                    detail: $"Id={id} olan ödünç kaydı bulunamadı.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Kaynak Bulunamadı");
            }

            if (record.ReturnDate != null)
            {
                return Problem(
                    detail: "Bu ödünç kaydına ait kitap daha önce zaten iade edilmiş.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Geçersiz İşlem");
            }

            DateTime returnDate = DateTime.Now;
            record.ReturnDate = returnDate;

            string dueDateStr = record.DueDate.ToString("dd.MM.yyyy");
            string returnDateStr = returnDate.ToString("dd.MM.yyyy");

            var penalty = _calculator.Calculate(record.CountryCode, dueDateStr, returnDateStr);

            if (penalty.IsError)
            {
                return Problem(
                    detail: penalty.ErrorMessage,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Hesaplama Hatası");
            }

            if (penalty.Amount > 0)
            {
                record.ComputedPenaltyFee = penalty.Amount;
            }

            if (record.Book != null)
            {
                record.Book.IsAvailable = true;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Kitap başarıyla iade alındı.",
                IadeTarihi = record.ReturnDate,
                CezaDurumu = penalty.FormattedResult
            });
        }

        // GET: api/borrow/active -> Aktif ödünç kayıtlarını listele
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<BorrowRecordDto>>> GetActiveBorrows()
        {
            var activeBorrows = await _context.BorrowRecords
                .Where(br => br.ReturnDate == null)
                .Include(br => br.Book)
                .Include(br => br.Member)
                .Select(br => new BorrowRecordDto
                {
                    Id = br.Id,
                    BookId = br.BookId,
                    BookTitle = br.Book != null ? br.Book.Title : string.Empty,
                    MemberId = br.MemberId,
                    MemberName = br.Member != null ? br.Member.FullName : string.Empty,
                    BorrowDate = br.BorrowDate,
                    DueDate = br.DueDate,
                    ReturnDate = br.ReturnDate,
                    CountryCode = br.CountryCode,
                    ComputedPenaltyFee = br.ComputedPenaltyFee,
                    IsPenaltyPaid = br.IsPenaltyPaid
                })
                .ToListAsync();

            return Ok(activeBorrows);
        }
    }
}