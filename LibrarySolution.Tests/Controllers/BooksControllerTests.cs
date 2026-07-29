using System.Collections.Generic;
using System.Threading.Tasks;
using Library.API.Controllers;
using Library.Business.Interfaces;
using Library.Core.Common;
using Library.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace LibrarySolution.Tests.Controllers
{
    public class BooksControllerTests
    {
        private readonly Mock<IBookService> _bookServiceMock;
        private readonly BooksController _sut;

        public BooksControllerTests()
        {
            _bookServiceMock = new Mock<IBookService>();
            _sut = new BooksController(_bookServiceMock.Object);
        }

        [Fact]
        public async Task GetBooks_ReturnsOkWithPagedResult()
        {
            // Arrange
            var pagedResult = new PagedResult<BookDto> { TotalCount = 1, Items = new List<BookDto> { new BookDto { Id = 1, Title = "Test" } } };
            _bookServiceMock.Setup(s => s.GetBooksAsync(null, 1, 10)).ReturnsAsync(pagedResult);

            // Act
            var actionResult = await _sut.GetBooks(null, 1, 10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.Equal(pagedResult, okResult.Value);
        }

        [Fact]
        public async Task GetBook_WhenNotFound_ReturnsProblemDetails()
        {
            // Arrange
            _bookServiceMock.Setup(s => s.GetBookByIdAsync(99)).ReturnsAsync((BookDto?)null);

            // Act
            var actionResult = await _sut.GetBook(99);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(404, objectResult.StatusCode);
            var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
            Assert.Equal("Kaynak Bulunamadı", problem.Title);
        }
    }
}