using Library.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Business
{
    public interface IMemberService
    {
        Task<PagedResult<MemberDto>> GetMembersAsync(int page, int pageSize);
        Task<MemberDto?> GetMemberByIdAsync(int id);
        Task<MemberDto> CreateMemberAsync(MemberDto memberDto);
        Task<bool> UpdateMemberAsync(int id, MemberDto memberDto);
        Task<bool> DeleteMemberAsync(int id);
    }
}
