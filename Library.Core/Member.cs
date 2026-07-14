using System;
using System.Collections.Generic;

namespace Library.Core
{
    public class Member
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime MembershipDate { get; set; } = DateTime.Now;

        // İlişki için Navigation Property
        public ICollection<BorrowRecord> BorrowRecords { get; set; } = new List<BorrowRecord>();
    }
}