using Library.Business;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PenaltyController : ControllerBase
    {
        private readonly IPenaltyFeeCalculator _penaltyFeeCalculator;

        // Dependency Injection ile IPenaltyFeeCalculator enjekte ediliyor
        public PenaltyController(IPenaltyFeeCalculator penaltyFeeCalculator)
        {
            _penaltyFeeCalculator = penaltyFeeCalculator;
        }

        /// <summary>
        /// Ülke kodu, başlangıç ve bitiş tarihlerine göre ceza tutarını hesaplar.
        /// </summary>
        /// <param name="countryCode">Kültür Kodu (Örn: tr-TR, ar-AE)</param>
        /// <param name="startDate">Geri getirilmesi gereken tarih / Ödünç alma tarihi</param>
        /// <param name="endDate">Teslim edilen tarih</param>
        [HttpGet("calculate")]
        public IActionResult Calculate([FromQuery] string countryCode, [FromQuery] string startDate, [FromQuery] string endDate)
        {
            if (string.IsNullOrWhiteSpace(countryCode) || string.IsNullOrWhiteSpace(startDate) || string.IsNullOrWhiteSpace(endDate))
            {
                return Problem(
                    detail: "Lütfen tüm parametreleri (countryCode, startDate, endDate) eksiksiz giriniz.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Eksik Parametre");
            }

            var result = _penaltyFeeCalculator.Calculate(countryCode, startDate, endDate);

            if (result.IsError)
            {
                return Problem(
                    detail: result.ErrorMessage,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Hesaplama Hatası");
            }

            return Ok(new { FeeResult = result });
        }
    }
}