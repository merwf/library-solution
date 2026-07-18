using System.ComponentModel.DataAnnotations;

namespace Library.Core
{
    public class BookDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kitap adı boş bırakılamaz.")]
        [StringLength(250, ErrorMessage = "Kitap adı en fazla 250 karakter olabilir.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yazar adı boş bırakılamaz.")]
        [StringLength(150, ErrorMessage = "Yazar adı en fazla 150 karakter olabilir.")]
        public string Author { get; set; } = string.Empty;

        [Required(ErrorMessage = "ISBN alanı boş bırakılamaz.")]
        [StringLength(50, ErrorMessage = "ISBN en fazla 50 karakter olabilir.")]
        public string ISBN { get; set; } = string.Empty;

        public bool IsAvailable { get; set; } = true;
    }
}