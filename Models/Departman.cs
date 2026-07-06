using System.ComponentModel.DataAnnotations;

namespace PersonelYonetim.Models
{
    // Departmanlar artik serbest metin degil, ayri bir tablo.
    // Boylece "Bilgi İşlem" / "bilgi islem" gibi tutarsizliklar onlenir.
    public class Departman
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Departman adı zorunludur.")]
        [StringLength(50, ErrorMessage = "Departman adı en fazla 50 karakter olabilir.")]
        [Display(Name = "Departman Adı")]
        public string Ad { get; set; } = string.Empty;

        // Navigation property: bu departmandaki personeller (1-e-cok iliskinin "cok" tarafi)
        public List<Personel> Personeller { get; set; } = new();
    }
}
