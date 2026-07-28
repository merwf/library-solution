using Library.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Data.Repositories
{
    public interface IMemberRepository
    {
        Task<(List<Member> Items, int TotalCount)> GetAllAsync(int page, int pageSize);
        Task<Member?> GetByIdAsync(int id);
        Task AddAsync(Member member);
        Task<bool> UpdateAsync(Member member);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
