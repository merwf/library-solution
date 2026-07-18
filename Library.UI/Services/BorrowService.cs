using Library.Core;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Library.UI.Services
{
    public class BorrowService : IBorrowService
    {
        private readonly HttpClient _http;

        public BorrowService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<BorrowRecordDto>> GetActiveBorrowsAsync()
        {
            try
            {
                return await _http.GetFromJsonAsync<List<BorrowRecordDto>>("api/borrow/active") ?? new List<BorrowRecordDto>();
            }
            catch
            {
                return new List<BorrowRecordDto>();
            }
        }

        public async Task<bool> BorrowBookAsync(object borrowRequest)
        {
            var response = await _http.PostAsJsonAsync("api/borrow", borrowRequest);
            return response.IsSuccessStatusCode;
        }

        public async Task<string> ReturnBookAsync(int id)
        {
            var response = await _http.PostAsync($"api/borrow/{id}/return", null);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ReturnResponseDto>();
                return result?.CezaDurumu ?? "İşlem başarılı";
            }
            return "Hata: İade işlemi başarısız.";
        }
    }

    public class ReturnResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public string CezaDurumu { get; set; } = string.Empty;
    }
}