using Library.Business.Interfaces;
using Library.Core.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BorrowController : ControllerBase
    {
        private readonly IBorrowService _borrowService;

        public BorrowController(IBorrowService borrowService)
        {
            _borrowService = borrowService;
        }

        [HttpPost]
        public async Task<IActionResult> BorrowBook([FromBody] BorrowRequestDto request)
        {
            var result = await _borrowService.BorrowBookAsync(request);

            if (!result.IsSuccess)
            {
                return Problem(
                    detail: result.ErrorMessage,
                    statusCode: result.StatusCode,
                    title: result.StatusCode == 404 ? "Kaynak Bulunamadı" : "Çakışma Durumu");
            }

            return Created($"api/borrow/{result.RecordId}", new
            {
                Message = "Kitap başarıyla ödünç verildi.",
                RecordId = result.RecordId,
                TeslimTarihi = result.DueDate
            });
        }

        [HttpPost("{id}/return")]
        public async Task<IActionResult> ReturnBook(int id)
        {
            var result = await _borrowService.ReturnBookAsync(id);

            if (!result.IsSuccess)
            {
                return Problem(
                    detail: result.ErrorMessage,
                    statusCode: result.StatusCode,
                    title: result.StatusCode == 404 ? "Kaynak Bulunamadı" : "Geçersiz İşlem");
            }

            return Ok(new
            {
                Message = "Kitap başarıyla iade alındı.",
                IadeTarihi = result.ReturnDate,
                CezaDurumu = result.FormattedPenaltyResult
            });
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<BorrowRecordDto>>> GetActiveBorrows()
        {
            var activeBorrows = await _borrowService.GetActiveBorrowsAsync();
            return Ok(activeBorrows);
        }
    }
}