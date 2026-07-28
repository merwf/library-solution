using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Core.DTOs
{
    public class PenaltyResultDto
    {
        public decimal Amount { get; set; }  // Örn: 5.25
        public string Currency { get; set; } = string.Empty; // Örn: "TRY"
        public bool IsError { get; set; } = false;
        public string? ErrorMessage { get; set; }

        // Ekrana veya loglara basmak gerekirse
        public string FormattedResult => IsError ? $"Error: {ErrorMessage}" : $"{Amount:F2} {Currency}";
    }
}