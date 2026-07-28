using Library.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Library.UI.HttpServices.Interfaces
{
    public interface IMemberService
    {
        Task<List<MemberDto>> GetMembersAsync();
        Task<MemberDto?> GetMemberByIdAsync(int id);
        Task<bool> AddMemberAsync(MemberDto member);
        Task<bool> UpdateMemberAsync(int id, MemberDto member);
        Task<bool> DeleteMemberAsync(int id);
    }
}