using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Library.Core.DTOs;
using Library.Core.Entities;
using Library.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Library.Data.Repositories.Implementations
{
    public class BorrowRepository : IBorrowRepository
    {
        private readonly LibraryDbContext _context;

        public BorrowRepository(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<BorrowRecord?> GetByIdWithBookAsync(int id)
        {
            return await _context.BorrowRecords
                .Include(br => br.Book)
                .FirstOrDefaultAsync(br => br.Id == id);
        }

        public async Task<List<BorrowRecordDto>> GetActiveBorrowsAsync()
        {
            return await _context.BorrowRecords
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
        }

        public async Task AddAsync(BorrowRecord record)
        {
            _context.BorrowRecords.Add(record);
            await SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
