using Microsoft.EntityFrameworkCore;
using Library.Core;
using LibraryConfigUtilities;

namespace Library.Data
{
    public class LibraryDbContext : DbContext
    {
        public DbSet<Book> Books { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<BorrowRecord> BorrowRecords { get; set; }

        public LibraryDbContext() { }

        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured) {}
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Book - BorrowRecord (Bire-Çok İlişki)
            modelBuilder.Entity<BorrowRecord>()
                .HasOne(br => br.Book)
                .WithMany(b => b.BorrowRecords)
                .HasForeignKey(br => br.BookId);

            // Member - BorrowRecord (Bire-Çok İlişki)
            modelBuilder.Entity<BorrowRecord>()
                .HasOne(br => br.Member)
                .WithMany(m => m.BorrowRecords)
                .HasForeignKey(br => br.MemberId);

            // Tablo kısıtlamaları (Hafif validation kuralları)
            modelBuilder.Entity<Book>().Property(b => b.Title).IsRequired().HasMaxLength(250);
            modelBuilder.Entity<Book>().Property(b => b.ISBN).IsRequired().HasMaxLength(50);
            modelBuilder.Entity<Member>().Property(m => m.FullName).IsRequired().HasMaxLength(150);
            modelBuilder.Entity<BorrowRecord>().Property(br => br.CountryCode).IsRequired().HasMaxLength(10);

            // Ondalık sayı hassasiyet uyarısını çözen satır:
            modelBuilder.Entity<BorrowRecord>()
                .Property(br => br.ComputedPenaltyFee)
                .HasColumnType("decimal(18,2)");

            // --- SEED DATA (ÖRNEK VERİLER) ---
            // Örnek Kitaplar
            modelBuilder.Entity<Book>().HasData(
                new Book { Id = 1, Title = "Sherlock Holmes: A Study in Scarlet", Author = "Arthur Conan Doyle", ISBN = "9780141036236", IsAvailable = true },
                new Book { Id = 2, Title = "Death Note - Vol 1", Author = "Tsugumi Ohba", ISBN = "9781421501680", IsAvailable = true },
                new Book { Id = 3, Title = "11/22/63", Author = "Stephen King", ISBN = "9781451627282", IsAvailable = false }
            );

            // Örnek Üyeler
            modelBuilder.Entity<Member>().HasData(
                new Member { Id = 1, FullName = "Merve Gazioğlu", Email = "merve@kocaeli.edu.tr", MembershipDate = new System.DateTime(2025, 11, 1) },
                new Member { Id = 2, FullName = "Melike Yılmaz", Email = "melike@kocaeli.edu.tr", MembershipDate = new System.DateTime(2026, 1, 15) }
            );
        }
    }
}