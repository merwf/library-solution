using System.Collections.Generic;

namespace LibrarySolution.Domain
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public bool IsAvailable { get; set; } = true;

        // İlişki için Navigation Property
        public ICollection<BorrowRecord> BorrowRecords { get; set; } = new List<BorrowRecord>();
    }
}