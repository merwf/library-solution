using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Core.DTOs
{
    // İstek gövdesini (JSON body) düzgün karşılayabilmek için yardımcı DTO sınıfı
    public class BorrowRequestDto
    {
        public int BookId { get; set; }
        public int MemberId { get; set; }
        public string CountryCode { get; set; } = string.Empty;
    }
}
