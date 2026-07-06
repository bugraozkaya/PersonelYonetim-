using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonelYonetim.Data;
using PersonelYonetim.Models;

namespace PersonelYonetim.Controllers
{
    // Departmanlarin yonetimi: listeleme, ekleme, silme.
    // Tek sayfada hallolacak kadar basit oldugu icin ayri Create/Delete view'lari yok.
    public class DepartmanlarController : Controller
    {
        private readonly UygulamaDbContext _context;

        public DepartmanlarController(UygulamaDbContext context)
        {
            _context = context;
        }

        // GET: /Departmanlar
        public async Task<IActionResult> Index()
        {
            // Include ile her departmanin personelleri de gelir -> sayilarini gosterecegiz
            var departmanlar = await _context.Departmanlar
                .Include(d => d.Personeller)
                .OrderBy(d => d.Ad)
                .ToListAsync();
            return View(departmanlar);
        }

        // POST: /Departmanlar/Ekle  (Index'teki kucuk formdan gelir)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(string ad)
        {
            ad = (ad ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(ad))
                TempData["Hata"] = "Departman adı boş olamaz.";
            else if (ad.Length > 50)
                TempData["Hata"] = "Departman adı en fazla 50 karakter olabilir.";
            else if (await _context.Departmanlar.AnyAsync(d => d.Ad == ad))
                TempData["Hata"] = $"\"{ad}\" departmanı zaten kayıtlı.";
            else
            {
                _context.Departmanlar.Add(new Departman { Ad = ad });
                await _context.SaveChangesAsync();
                TempData["Mesaj"] = $"\"{ad}\" departmanı eklendi.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Departmanlar/Sil/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sil(int id)
        {
            var departman = await _context.Departmanlar
                .Include(d => d.Personeller)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (departman == null) return NotFound();

            // Referans butunlugu: icinde personel olan departman silinemez,
            // yoksa o personellerin DepartmanId'si bosluga isaret ederdi
            if (departman.Personeller.Any())
            {
                TempData["Hata"] = $"\"{departman.Ad}\" silinemez: bu departmanda " +
                                   $"{departman.Personeller.Count} personel var. Önce onları taşıyın.";
            }
            else
            {
                _context.Departmanlar.Remove(departman);
                await _context.SaveChangesAsync();
                TempData["Mesaj"] = $"\"{departman.Ad}\" departmanı silindi.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
