using System;

namespace LibrarySolution.Domain
{
    public class BorrowRecord
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public int MemberId { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string CountryCode { get; set; } = string.Empty;

        // İleride işimizi kolaylaştıracak genişletilmiş alanlar
        public decimal ComputedPenaltyFee { get; set; } = 0.00m;
        public bool IsPenaltyPaid { get; set; } = false;

        // Navigation Properties
        public Book? Book { get; set; }
        public Member? Member { get; set; }
    }
}