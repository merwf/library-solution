using Library.Core;
using Library.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembersController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        // Veritabanı bağlantısını içeri alıyoruz
        public MembersController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: api/members -> Tüm üyeleri listele
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetMembers()
        {
            return await _context.Members
                .Select(m => new MemberDto
                {
                    Id = m.Id,
                    FullName = m.FullName,
                    Email = m.Email,
                    MembershipDate = m.MembershipDate
                }).ToListAsync();
        }

        // POST: api/members -> Yeni üye ekle
        [HttpPost]
        public async Task<ActionResult<MemberDto>> PostMember(MemberDto memberDto)
        {
            var member = new Member
            {
                FullName = memberDto.FullName,
                Email = memberDto.Email,
                MembershipDate = System.DateTime.Now
            };

            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            memberDto.Id = member.Id;
            memberDto.MembershipDate = member.MembershipDate;

            // Üye eklendikten sonra listeleme endpoint'ine yönlendiriyoruz
            return CreatedAtAction(nameof(GetMembers), new { id = member.Id }, memberDto);
        }
    }
}