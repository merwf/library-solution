using System;
using System.ComponentModel.DataAnnotations;

namespace Library.Core
{
    public class MemberDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Üye adı soyadı boş bırakılamaz.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta alanı boş bırakılamaz.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; } = string.Empty;

        public System.DateTime MembershipDate { get; set; }
    }
}