using Library.Core;
using Library.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<ActionResult<IEnumerable<Member>>> GetMembers()
        {
            return await _context.Members.ToListAsync();
        }

        // POST: api/members -> Yeni üye ekle
        [HttpPost]
        public async Task<ActionResult<Member>> PostMember(Member member)
        {
            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            // Üye eklendikten sonra listeleme endpoint'ine yönlendiriyoruz
            return CreatedAtAction(nameof(GetMembers), new { id = member.Id }, member);
        }
    }
}