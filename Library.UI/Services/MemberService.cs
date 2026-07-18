using Library.Core;
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
            try
            {
                return await _http.GetFromJsonAsync<List<MemberDto>>("api/members") ?? new List<MemberDto>();
            }
            catch
            {
                return new List<MemberDto>();
            }
        }

        public async Task<bool> AddMemberAsync(MemberDto member)
        {
            var response = await _http.PostAsJsonAsync("api/members", member);
            return response.IsSuccessStatusCode;
        }
    }
}