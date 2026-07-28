using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Library.API.Controllers;
using Library.Business;
using Library.Core;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace LibrarySolution.Tests
{
    // BorrowController artık IBorrowService'e delege eden ince (thin) bir katman,
    // dolayısıyla bu testlerin amacı iş mantığını tekrar test etmek değil;
    // controller'ın servisten dönen sonuca göre DOĞRU HTTP response'unu
    // (Created / Ok / Problem + doğru status code) üretip üretmediğini doğrulamak.
    public class BorrowControllerTests
    {
        private readonly Mock<IBorrowService> _borrowServiceMock;
        private readonly BorrowController _sut;

        public BorrowControllerTests()
        {
            _borrowServiceMock = new Mock<IBorrowService>();
            _sut = new BorrowController(_borrowServiceMock.Object);
        }

        [Fact]
        public async Task BorrowBook_Success_ReturnsCreatedWithRecordId()
        {
            var request = new BorrowRequestDto { BookId = 1, MemberId = 2, CountryCode = "tr-TR" };
            var serviceResult = new BorrowResultDto
            {
                IsSuccess = true,
                RecordId = 10,
                DueDate = DateTime.Now.AddDays(10)
            };
            _borrowServiceMock.Setup(s => s.BorrowBookAsync(request)).ReturnsAsync(serviceResult);

            var actionResult = await _sut.BorrowBook(request);

            var createdResult = Assert.IsType<CreatedResult>(actionResult);
            Assert.Equal($"api/borrow/{serviceResult.RecordId}", createdResult.Location);
        }

        [Theory]
        [InlineData(404, "Kaynak Bulunamadı")]
        [InlineData(409, "Çakışma Durumu")]
        public async Task BorrowBook_Failure_ReturnsProblemWithCorrectStatusCode(int statusCode, string expectedTitle)
        {
            var request = new BorrowRequestDto { BookId = 1, MemberId = 2, CountryCode = "tr-TR" };
            var serviceResult = new BorrowResultDto
            {
                IsSuccess = false,
                StatusCode = statusCode,
                ErrorMessage = "Test hata mesajı"
            };
            _borrowServiceMock.Setup(s => s.BorrowBookAsync(request)).ReturnsAsync(serviceResult);

            var actionResult = await _sut.BorrowBook(request);

            var objectResult = Assert.IsType<ObjectResult>(actionResult);
            Assert.Equal(statusCode, objectResult.StatusCode);

            var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
            Assert.Equal(expectedTitle, problemDetails.Title);
            Assert.Equal("Test hata mesajı", problemDetails.Detail);
        }

        [Fact]
        public async Task ReturnBook_Success_ReturnsOkWithPenaltyInfo()
        {
            var serviceResult = new ReturnResultDto
            {
                IsSuccess = true,
                ReturnDate = DateTime.Now,
                FormattedPenaltyResult = "5.25 TRY"
            };
            _borrowServiceMock.Setup(s => s.ReturnBookAsync(7)).ReturnsAsync(serviceResult);

            var actionResult = await _sut.ReturnBook(7);

            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task ReturnBook_Failure_ReturnsProblem()
        {
            var serviceResult = new ReturnResultDto
            {
                IsSuccess = false,
                StatusCode = 400,
                ErrorMessage = "Bu ödünç kaydına ait kitap daha önce zaten iade edilmiş."
            };
            _borrowServiceMock.Setup(s => s.ReturnBookAsync(7)).ReturnsAsync(serviceResult);

            var actionResult = await _sut.ReturnBook(7);

            var objectResult = Assert.IsType<ObjectResult>(actionResult);
            Assert.Equal(400, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetActiveBorrows_ReturnsOkWithList()
        {
            var records = new List<BorrowRecordDto>
            {
                new BorrowRecordDto { Id = 1, BookTitle = "Death Note - Vol 1" }
            };
            _borrowServiceMock.Setup(s => s.GetActiveBorrowsAsync()).ReturnsAsync(records);

            var actionResult = await _sut.GetActiveBorrows();

            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedRecords = Assert.IsAssignableFrom<IEnumerable<BorrowRecordDto>>(okResult.Value);
            Assert.Single(returnedRecords);
        }
    }
}