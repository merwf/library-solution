using Library.Business.Interfaces;
using Library.Core.Common;
using Library.Core.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembersController : ControllerBase
    {
        private readonly IMemberService _memberService;

        public MembersController(IMemberService memberService)
        {
            _memberService = memberService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<MemberDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMembers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _memberService.GetMembersAsync(page, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MemberDto>> GetMember(int id)
        {
            var member = await _memberService.GetMemberByIdAsync(id);
            if (member == null)
            {
                return Problem(
                    detail: $"Id={id} olan üye sistemde mevcut değil.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Kaynak Bulunamadı");
            }

            return Ok(member);
        }

        [HttpPost]
        public async Task<ActionResult<MemberDto>> PostMember(MemberDto memberDto)
        {
            var createdMember = await _memberService.CreateMemberAsync(memberDto);
            return CreatedAtAction(nameof(GetMember), new { id = createdMember.Id }, createdMember);
        }

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

            var updated = await _memberService.UpdateMemberAsync(id, memberDto);
            if (!updated)
            {
                return Problem(
                    detail: $"Güncellenmek istenen Id={id} olan üye bulunamadı.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Kaynak Bulunamadı");
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMember(int id)
        {
            var deleted = await _memberService.DeleteMemberAsync(id);
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