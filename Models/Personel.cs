using System.ComponentModel.DataAnnotations;

namespace PersonelYonetim.Models
{
    // Veritabanindaki Personeller tablosunu temsil eden entity sinifi
    public class Personel
    {
        // "Id" ismi EF Core tarafindan otomatik olarak
        // primary key + otomatik artan kabul edilir
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad Soyad alanı zorunludur.")]
        [StringLength(100, ErrorMessage = "Ad Soyad en fazla 100 karakter olabilir.")]
        [Display(Name = "Ad Soyad")]
        public string AdSoyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Departman alanı zorunludur.")]
        [Display(Name = "Departman")]
        public string Departman { get; set; } = string.Empty;

        [Required(ErrorMessage = "Pozisyon alanı zorunludur.")]
        [Display(Name = "Pozisyon")]
        public string Pozisyon { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        public string Eposta { get; set; } = string.Empty;

        [Required(ErrorMessage = "Maaş alanı zorunludur.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Maaş 0'dan büyük olmalıdır.")]
        [Display(Name = "Maaş (₺)")]
        public decimal Maas { get; set; }

        [Required(ErrorMessage = "İşe giriş tarihi zorunludur.")]
        [DataType(DataType.Date)]   // formda saat olmadan tarih secici uretir
        [Display(Name = "İşe Giriş Tarihi")]
        public DateTime IseGirisTarihi { get; set; }

        [Display(Name = "Aktif mi?")]
        public bool AktifMi { get; set; } = true;   // varsayilan: aktif
    }
}