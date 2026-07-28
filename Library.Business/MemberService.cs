using Library.Core;
using Library.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Business
{
    public class MemberService : IMemberService
    {
        private readonly IMemberRepository _memberRepository;

        public MemberService(IMemberRepository memberRepository)
        {
            _memberRepository = memberRepository;
        }

        public async Task<PagedResult<MemberDto>> GetMembersAsync(int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var (members, totalCount) = await _memberRepository.GetAllAsync(page, pageSize);

            return new PagedResult<MemberDto>
            {
                Items = members.Select(m => new MemberDto
                {
                    Id = m.Id,
                    FullName = m.FullName,
                    Email = m.Email,
                    MembershipDate = m.MembershipDate
                }).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<MemberDto?> GetMemberByIdAsync(int id)
        {
            var member = await _memberRepository.GetByIdAsync(id);
            if (member == null) return null;

            return new MemberDto
            {
                Id = member.Id,
                FullName = member.FullName,
                Email = member.Email,
                MembershipDate = member.MembershipDate
            };
        }

        public async Task<MemberDto> CreateMemberAsync(MemberDto memberDto)
        {
            var member = new Member
            {
                FullName = memberDto.FullName,
                Email = memberDto.Email,
                MembershipDate = DateTime.Now
            };

            await _memberRepository.AddAsync(member);

            memberDto.Id = member.Id;
            memberDto.MembershipDate = member.MembershipDate;
            return memberDto;
        }

        public async Task<bool> UpdateMemberAsync(int id, MemberDto memberDto)
        {
            if (id != memberDto.Id) return false;

            var member = new Member
            {
                Id = memberDto.Id,
                FullName = memberDto.FullName,
                Email = memberDto.Email
            };

            return await _memberRepository.UpdateAsync(member);
        }

        public async Task<bool> DeleteMemberAsync(int id)
        {
            return await _memberRepository.DeleteAsync(id);
        }
    }
}
