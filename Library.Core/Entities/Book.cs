using System;
using System.Collections.Generic;

namespace Library.Core.Entities
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;

        // Diğer projelerden erişim için public set
        public bool IsAvailable { get; set; } = true;

        public ICollection<BorrowRecord> BorrowRecords { get; set; } = new List<BorrowRecord>();

        public void MarkAsBorrowed()
        {
            if (!IsAvailable)
                throw new InvalidOperationException("Bu kitap zaten ödünç verilmiş durumda.");

            IsAvailable = false;
        }

        public void MarkAsReturned()
        {
            IsAvailable = true;
        }
    }
}