using Library.Core;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Library.UI.Services
{
    public class BookService : IBookService
    {
        private readonly HttpClient _http;

        public BookService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<BookDto>> GetBooksAsync()
        {
            try
            {
                return await _http.GetFromJsonAsync<List<BookDto>>("api/books") ?? new List<BookDto>();
            }
            catch
            {
                return new List<BookDto>();
            }
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