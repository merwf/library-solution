using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Library.Business.Concrete;
using Library.Core;
using Library.Core.DTOs;
using Library.Core.Entities;
using Library.Data.Repositories.Interfaces;
using Moq;
using Xunit;

namespace LibrarySolution.Tests.Services
{
    public class MemberServiceTests
    {
        private readonly Mock<IMemberRepository> _memberRepoMock;
        private readonly MemberService _sut;

        public MemberServiceTests()
        {
            _memberRepoMock = new Mock<IMemberRepository>();
            _sut = new MemberService(_memberRepoMock.Object);
        }

        [Fact]
        public async Task GetMembersAsync_ReturnsPagedMembers()
        {
            // Arrange
            var members = new List<Member>
            {
                new Member { Id = 1, FullName = "Merve Gazioğlu", Email = "merve@kocaeli.edu.tr", MembershipDate = DateTime.Now }
            };

            _memberRepoMock.Setup(r => r.GetAllAsync(1, 10)).ReturnsAsync((members, 1));

            // Act
            var result = await _sut.GetMembersAsync(1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Items);
            Assert.Equal("Merve Gazioğlu", result.Items[0].FullName);
        }

        [Fact]
        public async Task GetMemberByIdAsync_WhenMemberExists_ReturnsDto()
        {
            // Arrange
            var member = new Member { Id = 1, FullName = "Melike Yılmaz", Email = "melike@kocaeli.edu.tr" };
            _memberRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(member);

            // Act
            var result = await _sut.GetMemberByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Melike Yılmaz", result!.FullName);
        }

        [Fact]
        public async Task CreateMemberAsync_ValidMember_AssignsIdAndReturnsDto()
        {
            // Arrange
            var dto = new MemberDto { FullName = "Yeni Üye", Email = "yeni@kocaeli.edu.tr" };

            _memberRepoMock.Setup(r => r.AddAsync(It.IsAny<Member>()))
                           .Callback<Member>(m => m.Id = 5)
                           .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.CreateMemberAsync(dto);

            // Assert
            Assert.Equal(5, result.Id);
            _memberRepoMock.Verify(r => r.AddAsync(It.Is<Member>(m => m.FullName == "Yeni Üye")), Times.Once);
        }

        [Fact]
        public async Task DeleteMemberAsync_MemberNotFound_ReturnsFalse()
        {
            // Arrange
            _memberRepoMock.Setup(r => r.DeleteAsync(99)).ReturnsAsync(false);

            // Act
            var result = await _sut.DeleteMemberAsync(99);

            // Assert
            Assert.False(result);
        }
    }
}