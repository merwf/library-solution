using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Library.Business;
using Library.Core;
using Library.Data.Repositories;
using Moq;
using Xunit;

namespace LibrarySolution.Tests
{
    // BorrowService, projenin gerçek iş mantığının yaşadığı yer olduğu için
    // önceliği buraya veriyoruz. IBorrowRepository, IBookRepository,
    // IMemberRepository ve IPenaltyFeeCalculator hepsi Moq ile sahteleniyor,
    // yani bu testler HERHANGİ bir gerçek veritabanına dokunmuyor.
    public class BorrowServiceTests
    {
        private readonly Mock<IBorrowRepository> _borrowRepoMock;
        private readonly Mock<IBookRepository> _bookRepoMock;
        private readonly Mock<IMemberRepository> _memberRepoMock;
        private readonly Mock<IPenaltyFeeCalculator> _calculatorMock;
        private readonly BorrowService _sut; // sut = System Under Test

        public BorrowServiceTests()
        {
            _borrowRepoMock = new Mock<IBorrowRepository>();
            _bookRepoMock = new Mock<IBookRepository>();
            _memberRepoMock = new Mock<IMemberRepository>();
            _calculatorMock = new Mock<IPenaltyFeeCalculator>();

            _sut = new BorrowService(
                _borrowRepoMock.Object,
                _bookRepoMock.Object,
                _memberRepoMock.Object,
                _calculatorMock.Object);
        }

        // ---------------------------------------------------------
        // BorrowBookAsync senaryoları
        // ---------------------------------------------------------

        [Fact]
        public async Task BorrowBookAsync_BookNotFound_Returns404()
        {
            // Arrange: repository'den kitap dönmüyor (null)
            _bookRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                         .ReturnsAsync((Book?)null);

            var request = new BorrowRequestDto { BookId = 99, MemberId = 1, CountryCode = "tr-TR" };

            // Act
            var result = await _sut.BorrowBookAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
            Assert.Contains("99", result.ErrorMessage);

            // Kitap bulunamadıysa, üye sorgusuna hiç gidilmemeli.
            _memberRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task BorrowBookAsync_BookNotAvailable_Returns409()
        {
            // Arrange: kitap var ama müsait değil
            var book = new Book { Id = 3, Title = "11/22/63", IsAvailable = false };
            _bookRepoMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(book);

            var request = new BorrowRequestDto { BookId = 3, MemberId = 1, CountryCode = "tr-TR" };

            // Act
            var result = await _sut.BorrowBookAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(409, result.StatusCode);

            // Kayıt asla eklenmemeli
            _borrowRepoMock.Verify(r => r.AddAsync(It.IsAny<BorrowRecord>()), Times.Never);
        }

        [Fact]
        public async Task BorrowBookAsync_MemberNotFound_Returns404()
        {
            // Arrange: kitap uygun ama üye bulunamıyor
            var book = new Book { Id = 1, Title = "Sherlock Holmes", IsAvailable = true };
            _bookRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(book);
            _memberRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Member?)null);

            var request = new BorrowRequestDto { BookId = 1, MemberId = 42, CountryCode = "tr-TR" };

            // Act
            var result = await _sut.BorrowBookAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
            Assert.Contains("42", result.ErrorMessage);
        }

        [Fact]
        public async Task BorrowBookAsync_ValidRequest_MarksBookUnavailableAndAddsRecord()
        {
            // Arrange
            var book = new Book { Id = 1, Title = "Sherlock Holmes", IsAvailable = true };
            var member = new Member { Id = 2, FullName = "Merve Gazioğlu" };

            _bookRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(book);
            _memberRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(member);

            BorrowRecord? capturedRecord = null;
            _borrowRepoMock.Setup(r => r.AddAsync(It.IsAny<BorrowRecord>()))
                           .Callback<BorrowRecord>(rec => capturedRecord = rec)
                           .Returns(Task.CompletedTask);

            var request = new BorrowRequestDto { BookId = 1, MemberId = 2, CountryCode = "tr-TR" };

            // Act
            var beforeCall = DateTime.Now;
            var result = await _sut.BorrowBookAsync(request);

            // Assert
            Assert.True(result.IsSuccess);

            // book.IsAvailable false'a çekilmiş olmalı (aynı context'te tracked olduğu için)
            Assert.False(book.IsAvailable);

            // AddAsync tam olarak 1 kez, doğru içerikle çağrılmış olmalı
            _borrowRepoMock.Verify(r => r.AddAsync(It.IsAny<BorrowRecord>()), Times.Once);
            Assert.NotNull(capturedRecord);
            Assert.Equal(1, capturedRecord!.BookId);
            Assert.Equal(2, capturedRecord.MemberId);
            Assert.Equal("tr-TR", capturedRecord.CountryCode);
            Assert.Null(capturedRecord.ReturnDate);

            // DueDate, BorrowDate + 10 gün olmalı (saniye hassasiyetinde tolerans bırakıyoruz
            // çünkü servis DateTime.Now'ı doğrudan kullanıyor, enjekte edilebilir bir
            // clock/IDateTimeProvider olmadığı için testte küçük bir zaman farkı normaldir)
            var expectedDueDate = beforeCall.AddDays(10);
            Assert.True(Math.Abs((result.DueDate - expectedDueDate).TotalSeconds) < 5);
        }

        // ---------------------------------------------------------
        // ReturnBookAsync senaryoları
        // ---------------------------------------------------------

        [Fact]
        public async Task ReturnBookAsync_RecordNotFound_Returns404()
        {
            _borrowRepoMock.Setup(r => r.GetByIdWithBookAsync(It.IsAny<int>()))
                           .ReturnsAsync((BorrowRecord?)null);

            var result = await _sut.ReturnBookAsync(123);

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task ReturnBookAsync_AlreadyReturned_Returns400()
        {
            var record = new BorrowRecord
            {
                Id = 5,
                ReturnDate = DateTime.Now.AddDays(-1) // zaten iade edilmiş
            };
            _borrowRepoMock.Setup(r => r.GetByIdWithBookAsync(5)).ReturnsAsync(record);

            var result = await _sut.ReturnBookAsync(5);

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);

            // Ceza hesaplayıcıya hiç gidilmemeli
            _calculatorMock.Verify(c => c.Calculate(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ReturnBookAsync_CalculatorReturnsError_Returns400()
        {
            var book = new Book { Id = 1, IsAvailable = false };
            var record = new BorrowRecord
            {
                Id = 5,
                BookId = 1,
                CountryCode = "xx-XX",
                DueDate = DateTime.Now,
                ReturnDate = null,
                Book = book
            };
            _borrowRepoMock.Setup(r => r.GetByIdWithBookAsync(5)).ReturnsAsync(record);

            _calculatorMock
                .Setup(c => c.Calculate("xx-XX", It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new PenaltyResultDto { IsError = true, ErrorMessage = "Country configuration not found." });

            var result = await _sut.ReturnBookAsync(5);

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Country configuration not found.", result.ErrorMessage);

            // Hata durumunda kitap tekrar müsait yapılmamalı, kayıt da save edilmemeli
            Assert.False(book.IsAvailable);
            _borrowRepoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task ReturnBookAsync_ValidReturn_NoPenalty_MarksBookAvailable()
        {
            var book = new Book { Id = 1, IsAvailable = false };
            var record = new BorrowRecord
            {
                Id = 5,
                BookId = 1,
                CountryCode = "tr-TR",
                DueDate = DateTime.Now.AddDays(1), // henüz gecikme yok
                ReturnDate = null,
                Book = book
            };
            _borrowRepoMock.Setup(r => r.GetByIdWithBookAsync(5)).ReturnsAsync(record);

            _calculatorMock
                .Setup(c => c.Calculate("tr-TR", It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new PenaltyResultDto { Amount = 0, Currency = "TRY" });

            var result = await _sut.ReturnBookAsync(5);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.ReturnDate);
            Assert.Equal(0m, record.ComputedPenaltyFee);
            Assert.True(book.IsAvailable); // kitap tekrar ödünç alınabilir hale gelmeli

            _borrowRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ReturnBookAsync_ValidReturn_WithPenalty_SetsComputedPenaltyFee()
        {
            var book = new Book { Id = 1, IsAvailable = false };
            var record = new BorrowRecord
            {
                Id = 5,
                BookId = 1,
                CountryCode = "tr-TR",
                DueDate = DateTime.Now.AddDays(-3), // gecikmiş
                ReturnDate = null,
                Book = book
            };
            _borrowRepoMock.Setup(r => r.GetByIdWithBookAsync(5)).ReturnsAsync(record);

            _calculatorMock
                .Setup(c => c.Calculate("tr-TR", It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new PenaltyResultDto { Amount = 15.75m, Currency = "TRY" });

            var result = await _sut.ReturnBookAsync(5);

            Assert.True(result.IsSuccess);
            Assert.Equal(15.75m, record.ComputedPenaltyFee);
            Assert.Contains("TRY", result.FormattedPenaltyResult);
            Assert.Equal(15.75m, record.ComputedPenaltyFee);
            Assert.True(book.IsAvailable);
        }

        // ---------------------------------------------------------
        // GetActiveBorrowsAsync
        // ---------------------------------------------------------

        [Fact]
        public async Task GetActiveBorrowsAsync_ReturnsListFromRepository()
        {
            var expected = new List<BorrowRecordDto>
            {
                new BorrowRecordDto { Id = 1, BookTitle = "Death Note - Vol 1" },
                new BorrowRecordDto { Id = 2, BookTitle = "Sherlock Holmes: A Study in Scarlet" }
            };
            _borrowRepoMock.Setup(r => r.GetActiveBorrowsAsync()).ReturnsAsync(expected);

            var result = await _sut.GetActiveBorrowsAsync();

            Assert.Equal(2, result.Count);
            Assert.Same(expected, result);
            _borrowRepoMock.Verify(r => r.GetActiveBorrowsAsync(), Times.Once);
        }
    }
}