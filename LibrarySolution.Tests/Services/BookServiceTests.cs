using System.Collections.Generic;
using System.Threading.Tasks;
using Library.Business.Concrete;
using Library.Core.DTOs;
using Library.Core.Entities;
using Library.Data.Repositories.Interfaces;
using Moq;
using Xunit;

namespace LibrarySolution.Tests.Services
{
    public class BookServiceTests
    {
        private readonly Mock<IBookRepository> _bookRepoMock;
        private readonly BookService _sut; // System Under Test

        public BookServiceTests()
        {
            _bookRepoMock = new Mock<IBookRepository>();
            _sut = new BookService(_bookRepoMock.Object);
        }

        [Fact]
        public async Task GetBooksAsync_DefaultPagination_ReturnsMappedPagedResult()
        {
            // Arrange
            var books = new List<Book>
            {
                new Book { Id = 1, Title = "Clean Code", Author = "Robert C. Martin", ISBN = "123", IsAvailable = true },
                new Book { Id = 2, Title = "Refactoring", Author = "Martin Fowler", ISBN = "456", IsAvailable = false }
            };

            _bookRepoMock.Setup(r => r.GetAllAsync(null, 1, 10))
                         .ReturnsAsync((books, 2));

            // Act
            var result = await _sut.GetBooksAsync(null, 1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.Items.Count);
            Assert.Equal("Clean Code", result.Items[0].Title);
            _bookRepoMock.Verify(r => r.GetAllAsync(null, 1, 10), Times.Once);
        }

        [Fact]
        public async Task GetBooksAsync_WithSearchQuery_ReturnsFilteredResults()
        {
            // Arrange
            var books = new List<Book>
            {
                new Book { Id = 1, Title = "Clean Code", Author = "Robert C. Martin", ISBN = "123", IsAvailable = true }
            };

            _bookRepoMock.Setup(r => r.GetAllAsync("clean", 1, 10))
                         .ReturnsAsync((books, 1));

            // Act
            var result = await _sut.GetBooksAsync("clean", 1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.TotalCount);
            Assert.Single(result.Items);
            _bookRepoMock.Verify(r => r.GetAllAsync("clean", 1, 10), Times.Once);
        }

        [Fact]
        public async Task GetBookByIdAsync_WhenBookExists_ReturnsMappedDto()
        {
            // Arrange
            var book = new Book { Id = 1, Title = "Sherlock Holmes", Author = "Doyle", ISBN = "789", IsAvailable = true };
            _bookRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(book);

            // Act
            var result = await _sut.GetBookByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result!.Id);
            Assert.Equal("Sherlock Holmes", result.Title);
        }

        [Fact]
        public async Task GetBookByIdAsync_WhenBookDoesNotExist_ReturnsNull()
        {
            // Arrange
            _bookRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Book?)null);

            // Act
            var result = await _sut.GetBookByIdAsync(99);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateBookAsync_ValidDto_AddsToRepositoryAndReturnsDto()
        {
            // Arrange
            var dto = new BookDto { Title = "New Book", Author = "New Author", ISBN = "000" };

            _bookRepoMock.Setup(r => r.AddAsync(It.IsAny<Book>()))
                         .Callback<Book>(b => b.Id = 10)
                         .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.CreateBookAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.Id);
            _bookRepoMock.Verify(r => r.AddAsync(It.Is<Book>(b => b.Title == "New Book" && b.IsAvailable)), Times.Once);
        }

        [Fact]
        public async Task UpdateBookAsync_IdMismatch_ReturnsFalseWithoutCallingRepo()
        {
            // Arrange
            var dto = new BookDto { Id = 2, Title = "Test" };

            // Act
            var result = await _sut.UpdateBookAsync(1, dto);

            // Assert
            Assert.False(result);
            _bookRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Book>()), Times.Never);
        }

        [Fact]
        public async Task UpdateBookAsync_ValidRequest_CallsRepositoryAndReturnsTrue()
        {
            // Arrange
            var dto = new BookDto { Id = 1, Title = "Updated Title", Author = "Author", ISBN = "111", IsAvailable = true };
            _bookRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Book>())).ReturnsAsync(true);

            // Act
            var result = await _sut.UpdateBookAsync(1, dto);

            // Assert
            Assert.True(result);
            _bookRepoMock.Verify(r => r.UpdateAsync(It.Is<Book>(b => b.Id == 1 && b.Title == "Updated Title")), Times.Once);
        }

        [Fact]
        public async Task DeleteBookAsync_CallsRepositoryAndReturnsResult()
        {
            // Arrange
            _bookRepoMock.Setup(r => r.DeleteAsync(5)).ReturnsAsync(true);

            // Act
            var result = await _sut.DeleteBookAsync(5);

            // Assert
            Assert.True(result);
            _bookRepoMock.Verify(r => r.DeleteAsync(5), Times.Once);
        }
    }
}