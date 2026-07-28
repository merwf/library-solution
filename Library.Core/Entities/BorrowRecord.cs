using System;

namespace Library.Core.Entities
{
    public class BorrowRecord
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public int MemberId { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }

        // Atama yapılabilmesi için set accessor'ı açıyoruz
        public DateTime? ReturnDate { get; set; }

        public string CountryCode { get; set; } = string.Empty;
        public decimal ComputedPenaltyFee { get; set; } = 0.00m;
        public bool IsPenaltyPaid { get; set; } = false;

        public Book? Book { get; set; }
        public Member? Member { get; set; }

        public void CompleteReturn(DateTime returnDate, decimal penaltyFee)
        {
            if (ReturnDate != null)
                throw new InvalidOperationException("Bu ödünç kaydı daha önce zaten iade edilmiş.");

            ReturnDate = returnDate;
            ComputedPenaltyFee = penaltyFee;
            Book?.MarkAsReturned();
        }

        public void MarkPenaltyAsPaid()
        {
            IsPenaltyPaid = true;
        }
    }
}