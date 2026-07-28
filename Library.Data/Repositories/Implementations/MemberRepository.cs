using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Library.Core.Entities;
using Library.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Library.Data.Repositories.Implementations
{
    public class MemberRepository : IMemberRepository
    {
        private readonly LibraryDbContext _context;

        public MemberRepository(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<(List<Member> Items, int TotalCount)> GetAllAsync(int page, int pageSize)
        {
            var totalCount = await _context.Members.CountAsync();
            var items = await _context.Members
                .OrderBy(m => m.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (items, totalCount);
        }

        public async Task<Member?> GetByIdAsync(int id) => await _context.Members.FindAsync(id);

        public async Task AddAsync(Member member)
        {
            _context.Members.Add(member);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(Member member)
        {
            var existing = await _context.Members.FindAsync(member.Id);
            if (existing == null) return false;

            // Modelindeki gerçek alanlar: FullName ve Email[cite: 1]
            existing.FullName = member.FullName;
            existing.Email = member.Email;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null) return false;
            _context.Members.Remove(member);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id) => await _context.Members.AnyAsync(m => m.Id == id);
    }
}

