using Library.Core.Common;
using Library.Core.DTOs;
using Library.UI.HttpServices.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Library.UI.HttpServices.Services
{
    public class BookService : IBookService
    {
        private readonly HttpClient _http;

        public BookService(HttpClient http)
        {
            _http = http;
        }

        public async Task<PagedResult<BookDto>> GetBooksAsync(string? search = null, int page = 1, int pageSize = 10)
        {
            var url = $"api/books?page={page}&pageSize={pageSize}";

            if (!string.IsNullOrWhiteSpace(search))
            {
                url += $"&search={Uri.EscapeDataString(search)}";
            }

            // Not: Hata burada yutulmuyor. API'ye ulaşılamazsa exception sayfaya (Books.razor)
            // gider; sayfa bunu yakalayıp kullanıcıya anlamlı bir hata mesajı gösterir.
            // Aksi halde "liste boş" ile "API'ye ulaşılamadı" durumları ayırt edilemezdi.
            return await _http.GetFromJsonAsync<PagedResult<BookDto>>(url)
                ?? new PagedResult<BookDto>();
        }

        public async Task<bool> AddBookAsync(BookDto book)
        {
            var response = await _http.PostAsJsonAsync("api/books", book);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateBookAsync(int id, BookDto book)
        {
            var response = await _http.PutAsJsonAsync($"api/books/{id}", book);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteBookAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/books/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}