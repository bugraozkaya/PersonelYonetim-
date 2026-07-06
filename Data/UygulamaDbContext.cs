using Microsoft.EntityFrameworkCore;
using PersonelYonetim.Models;

namespace PersonelYonetim.Data
{
    // EF Core'un veritabaniyla konusmasini saglayan kopru sinifi
    public class UygulamaDbContext : DbContext
    {
        // Baglanti ayarlari disaridan (Program.cs'ten) verilir
        public UygulamaDbContext(DbContextOptions<UygulamaDbContext> options)
            : base(options)
        {
        }

        // Bu satir "Personeller adinda bir tablom var" demek.
        // Sorgular hep bunun uzerinden yapilir: _context.Personeller...
        public DbSet<Personel> Personeller { get; set; }
    }
}