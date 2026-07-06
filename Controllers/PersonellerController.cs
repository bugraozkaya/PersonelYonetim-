using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PersonelYonetim.Data;
using PersonelYonetim.Models;

namespace PersonelYonetim.Controllers
{
    public class PersonellerController : Controller
    {
        private readonly UygulamaDbContext _context;

        // Dependency injection: Adim 4'te kaydettigimiz DbContext
        // buraya otomatik olarak teslim edilir. new'lemiyoruz!
        public PersonellerController(UygulamaDbContext context)
        {
            _context = context;
        }

        // GET: /Personeller?arama=ali&departman=IT&sadeceAktif=true
        // Parametreler URL'den (query string) otomatik baglanir; hepsi opsiyonel
        public async Task<IActionResult> Index(string? arama, string? departman, bool sadeceAktif = false)
        {
            // IQueryable: sorgu burada CALISMAZ, sadece tarif edilir.
            // Kosullari ekledikce SQL'e WHERE olarak eklenir,
            // veritabanina ancak ToListAsync deyince gidilir.
            var sorgu = _context.Personeller.AsQueryable();

            if (!string.IsNullOrWhiteSpace(arama))
            {
                // Ad veya e-posta icinde gecsin (SQL'de LIKE '%arama%' olur)
                sorgu = sorgu.Where(p => p.AdSoyad.Contains(arama) || p.Eposta.Contains(arama));
            }

            if (!string.IsNullOrWhiteSpace(departman))
            {
                sorgu = sorgu.Where(p => p.Departman == departman);
            }

            if (sadeceAktif)
            {
                sorgu = sorgu.Where(p => p.AktifMi);
            }

            var personeller = await sorgu.OrderBy(p => p.AdSoyad).ToListAsync();

            // Filtre formunu tekrar doldurabilmek icin secimleri view'a tasi
            ViewData["Arama"] = arama;
            ViewData["SecilenDepartman"] = departman;
            ViewData["SadeceAktif"] = sadeceAktif;

            // Acilir liste icin veritabanindaki benzersiz departmanlar
            var departmanlar = await _context.Personeller
                .Select(p => p.Departman)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();
            ViewData["Departmanlar"] = new SelectList(departmanlar, departman);

            return View(personeller);
        }

        // GET: /Personeller/Details/5 -> tek kayit detayi (READ)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var personel = await _context.Personeller
                .FirstOrDefaultAsync(p => p.Id == id);
            if (personel == null) return NotFound();

            return View(personel);
        }

        // GET: /Personeller/Create -> bos formu goster
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Personeller/Create -> formdan geleni kaydet (CREATE)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("AdSoyad,Departman,Pozisyon,Eposta,Maas,IseGirisTarihi,AktifMi")] Personel personel)
        {
            if (ModelState.IsValid)   // Data Annotations kurallari burada denetlenir
            {
                _context.Add(personel);
                await _context.SaveChangesAsync();
                TempData["Mesaj"] = $"\"{personel.AdSoyad}\" adlı personel başarıyla eklendi.";
                return RedirectToAction(nameof(Index));
            }
            return View(personel);    // hata varsa formu mesajlarla geri goster
        }

        // GET: /Personeller/Edit/5 -> formu mevcut veriyle doldur
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var personel = await _context.Personeller.FindAsync(id);
            if (personel == null) return NotFound();

            return View(personel);
        }

        // POST: /Personeller/Edit/5 -> guncelle (UPDATE)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,AdSoyad,Departman,Pozisyon,Eposta,Maas,IseGirisTarihi,AktifMi")] Personel personel)
        {
            if (id != personel.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(personel);
                await _context.SaveChangesAsync();
                TempData["Mesaj"] = $"\"{personel.AdSoyad}\" adlı personel başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            return View(personel);
        }

        // GET: /Personeller/Delete/5 -> once ONAY sayfasi goster
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var personel = await _context.Personeller
                .FirstOrDefaultAsync(p => p.Id == id);
            if (personel == null) return NotFound();

            return View(personel);
        }

        // POST: /Personeller/Delete/5 -> onaydan sonra sil (DELETE)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var personel = await _context.Personeller.FindAsync(id);
            if (personel != null)
            {
                _context.Personeller.Remove(personel);
                await _context.SaveChangesAsync();
                TempData["Mesaj"] = $"\"{personel.AdSoyad}\" adlı personel silindi.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}