using Library.Core.Common;
using Library.Core.DTOs;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Library.UI.Services
{
    public class MemberService : IMemberService
    {
        private readonly HttpClient _http;

        public MemberService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<MemberDto>> GetMembersAsync()
        {
            var result = await _http.GetFromJsonAsync<PagedResult<MemberDto>>("api/members");
            return result?.Items ?? new List<MemberDto>();
        }

        public async Task<MemberDto?> GetMemberByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<MemberDto>($"api/members/{id}");
        }

        public async Task<bool> AddMemberAsync(MemberDto member)
        {
            var response = await _http.PostAsJsonAsync("api/members", member);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateMemberAsync(int id, MemberDto member)
        {
            var response = await _http.PutAsJsonAsync($"api/members/{id}", member);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteMemberAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/members/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}