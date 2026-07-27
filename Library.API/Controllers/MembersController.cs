using Library.Core;
using Library.Data.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembersController : ControllerBase
    {
        private readonly IMemberRepository _memberRepository;

        public MembersController(IMemberRepository memberRepository)
        {
            _memberRepository = memberRepository;
        }

        // GET: api/members -> Tüm üyeleri listele
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetMembers()
        {
            var (members, _) = await _memberRepository.GetAllAsync(1, 1000);

            var result = members.Select(m => new MemberDto
            {
                Id = m.Id,
                FullName = m.FullName,
                Email = m.Email,
                MembershipDate = m.MembershipDate
            }).ToList();

            return Ok(result);
        }

        // GET: api/members/5 -> Tekil üye getir
        [HttpGet("{id}")]
        public async Task<ActionResult<MemberDto>> GetMember(int id)
        {
            var member = await _memberRepository.GetByIdAsync(id);
            if (member == null)
            {
                return Problem(
                    detail: $"Id={id} olan üye sistemde mevcut değil.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Kaynak Bulunamadı");
            }

            return Ok(new MemberDto
            {
                Id = member.Id,
                FullName = member.FullName,
                Email = member.Email,
                MembershipDate = member.MembershipDate
            });
        }

        // POST: api/members -> Yeni üye ekle
        [HttpPost]
        public async Task<ActionResult<MemberDto>> PostMember(MemberDto memberDto)
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

            // id alan GetMember endpoint'ine yönlendirme yapıyoruz
            return CreatedAtAction(nameof(GetMember), new { id = member.Id }, memberDto);
        }

        // PUT: api/members/5 -> Üye güncelle
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMember(int id, MemberDto memberDto)
        {
            if (id != memberDto.Id)
            {
                return Problem(
                    detail: "URL'deki ID ile gönderilen gövdedeki (body) ID uyuşmuyor.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Geçersiz İstek");
            }

            var member = new Member
            {
                Id = memberDto.Id,
                FullName = memberDto.FullName,
                Email = memberDto.Email
            };

            var updated = await _memberRepository.UpdateAsync(member);
            if (!updated)
            {
                return Problem(
                    detail: $"Güncellenmek istenen Id={id} olan üye bulunamadı.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Kaynak Bulunamadı");
            }

            return NoContent();
        }

        // DELETE: api/members/5 -> Üye sil
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMember(int id)
        {
            var deleted = await _memberRepository.DeleteAsync(id);
            if (!deleted)
            {
                return Problem(
                    detail: $"Silinmek istenen Id={id} olan üye bulunamadı.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Kaynak Bulunamadı");
            }

            return NoContent();
        }
    }
}