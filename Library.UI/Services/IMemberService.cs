using Library.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Library.UI.Services
{
    public interface IMemberService
    {
        Task<List<MemberDto>> GetMembersAsync();
        Task<bool> AddMemberAsync(MemberDto member);
    }
}