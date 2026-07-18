using System;

namespace Library.Core
{
    public class BorrowRecordDto
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty; // Kitabın adını da dönerek kolaylık sağlıyoruz
        public int MemberId { get; set; }
        public string MemberName { get; set; } = string.Empty; // Üyenin adını da ekliyoruz
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string CountryCode { get; set; } = string.Empty;
        public decimal ComputedPenaltyFee { get; set; }
        public bool IsPenaltyPaid { get; set; }
    }
}