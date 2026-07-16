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
        private readonly PenaltyFeeCalculator _calculator;

        // Hem veritabanını hem de senin yazdığın Ceza Hesaplayıcıyı içeri alıyoruz
        public BorrowController(LibraryDbContext context)
        {
            _context = context;
            _calculator = new PenaltyFeeCalculator();
        }

        // POST: api/borrow -> Kitap ödünç al (BookId, MemberId, CountryCode)
        [HttpPost]
        public async Task<IActionResult> BorrowBook([FromBody] BorrowRequestDto request)
        {
            var book = await _context.Books.FindAsync(request.BookId);
            if (book == null) return NotFound("Kitap bulunamadı.");
            if (!book.IsAvailable) return BadRequest("Bu kitap şu an başkasında, ödünç verilemez.");

            var member = await _context.Members.FindAsync(request.MemberId);
            if (member == null) return NotFound("Üye bulunamadı.");

            // Yeni bir ödünç kaydı oluşturuyoruz
            var record = new BorrowRecord
            {
                BookId = request.BookId,
                MemberId = request.MemberId,
                CountryCode = request.CountryCode,
                BorrowDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(10), // Kitaplar varsayılan 10 günlüğüne verilsin
                ReturnDate = null,
                ComputedPenaltyFee = 0,
                IsPenaltyPaid = false
            };

            // Kitabın durumunu "Ödünçte" (false) yapıyoruz
            book.IsAvailable = false;

            _context.BorrowRecords.Add(record);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Kitap başarıyla ödünç verildi.", RecordId = record.Id, TeslimTarihi = record.DueDate });
        }

        // POST: api/borrow/{id}/return -> Kitap iade et (iade tarihine göre ceza hesapla)
        [HttpPost("{id}/return")]
        public async Task<IActionResult> ReturnBook(int id)
        {
            var record = await _context.BorrowRecords
                .Include(br => br.Book)
                .FirstOrDefaultAsync(br => br.Id == id);

            if (record == null) return NotFound("Ödünç kaydı bulunamadı.");
            if (record.ReturnDate != null) return BadRequest("Bu kitap zaten iade edilmiş.");

            DateTime returnDate = DateTime.Now; // Şu anki iade zamanı
            record.ReturnDate = returnDate;

            // Tarihleri PenaltyFeeCalculator'ın istediği string formatına (dd.MM.yyyy) getiriyoruz
            string dueDateStr = record.DueDate.ToString("dd.MM.yyyy");
            string returnDateStr = returnDate.ToString("dd.MM.yyyy");

            // --- SENİN YAZDIĞIN CEZA HESAPLAYICIYI ÇAĞIRIYORUZ ---
            string penaltyResult = _calculator.Calculate(record.CountryCode, dueDateStr, returnDateStr);

            // Eğer sonuç "0.00 TRY" veya bir "Error" değilse, cezayı entity üzerindeki alana kaydedelim
            if (!penaltyResult.StartsWith("Error") && !penaltyResult.Contains("0.00"))
            {
                // Örn: "5,25 TRY" ifadesindeki sayı kısmını ayıklayıp decimal'e çevirme simülasyonu
                var parts = penaltyResult.Split(' ');
                if (parts.Length > 0 && decimal.TryParse(parts[0], out decimal fee))
                {
                    record.ComputedPenaltyFee = fee;
                }
            }

            // Kitap iade edildiği için durumunu tekrar "Erişilebilir" (true) yapıyoruz
            if (record.Book != null)
            {
                record.Book.IsAvailable = true;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Kitap başarıyla iade alındı.",
                IadeTarihi = record.ReturnDate,
                CezaDurumu = penaltyResult
            });
        }

        // GET: api/borrow/active -> Aktif ödünç kayıtlarını listele (ReturnDate'i boş olanlar)
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<BorrowRecord>>> GetActiveBorrows()
        {
            var activeBorrows = await _context.BorrowRecords
                .Where(br => br.ReturnDate == null)
                .Include(br => br.Book)
                .Include(br => br.Member)
                .ToListAsync();

            return Ok(activeBorrows);
        }
    }

    // İstek gövdesini (JSON body) düzgün karşılayabilmek için yardımcı DTO sınıfı
    public class BorrowRequestDto
    {
        public int BookId { get; set; }
        public int MemberId { get; set; }
        public string CountryCode { get; set; } = string.Empty; // tr-TR veya ar-AE
    }
}