using System.Text;
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
        private readonly IWebHostEnvironment _env;   // wwwroot klasorunun yolunu verir

        // Sayfa basina gosterilecek kayit sayisi
        private const int SayfaBoyutu = 5;

        // Fotograf yukleme kurallari
        private static readonly string[] IzinliUzantilar = { ".jpg", ".jpeg", ".png" };
        private const long MaksimumFotoBoyutu = 2 * 1024 * 1024;   // 2 MB

        public PersonellerController(UygulamaDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ---------- ORTAK YARDIMCILAR ----------

        // Filtre + siralama mantigi tek yerde: hem Index hem CSV aktarimi kullanir.
        // Boylece ayni kodu iki kez yazmayiz (DRY prensibi).
        private IQueryable<Personel> FiltreliSorgu(string? arama, int? departmanId,
            bool sadeceAktif, string? sirala)
        {
            // Include: Personel cekilirken iliskili Departman kaydi da JOIN ile gelsin
            var sorgu = _context.Personeller
                .Include(p => p.Departman)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(arama))
                sorgu = sorgu.Where(p => p.AdSoyad.Contains(arama) || p.Eposta.Contains(arama));

            if (departmanId.HasValue && departmanId > 0)
                sorgu = sorgu.Where(p => p.DepartmanId == departmanId);

            if (sadeceAktif)
                sorgu = sorgu.Where(p => p.AktifMi);

            sorgu = sirala switch
            {
                "ad_desc"        => sorgu.OrderByDescending(p => p.AdSoyad),
                "departman"      => sorgu.OrderBy(p => p.Departman!.Ad),
                "departman_desc" => sorgu.OrderByDescending(p => p.Departman!.Ad),
                "maas"           => sorgu.OrderBy(p => p.Maas),
                "maas_desc"      => sorgu.OrderByDescending(p => p.Maas),
                "tarih"          => sorgu.OrderBy(p => p.IseGirisTarihi),
                "tarih_desc"     => sorgu.OrderByDescending(p => p.IseGirisTarihi),
                _                => sorgu.OrderBy(p => p.AdSoyad)
            };

            return sorgu;
        }

        // Create/Edit formlarindaki departman acilir listesini doldurur
        private async Task DepartmanListesiniYukleAsync(int? secilenId = null)
        {
            var departmanlar = await _context.Departmanlar.OrderBy(d => d.Ad).ToListAsync();
            ViewData["DepartmanListesi"] = new SelectList(departmanlar, "Id", "Ad", secilenId);
        }

        // Yuklenen fotografi dogrular ve wwwroot/fotograflar altina kaydeder.
        // Gecersizse ModelState'e hata ekler ve null doner.
        private async Task<string?> FotografKaydetAsync(IFormFile? fotograf)
        {
            if (fotograf == null || fotograf.Length == 0) return null;   // fotograf opsiyonel

            var uzanti = Path.GetExtension(fotograf.FileName).ToLowerInvariant();
            if (!IzinliUzantilar.Contains(uzanti))
            {
                ModelState.AddModelError("FotografYolu", "Sadece .jpg, .jpeg veya .png dosyası yükleyebilirsiniz.");
                return null;
            }
            if (fotograf.Length > MaksimumFotoBoyutu)
            {
                ModelState.AddModelError("FotografYolu", "Fotoğraf en fazla 2 MB olabilir.");
                return null;
            }

            var klasor = Path.Combine(_env.WebRootPath, "fotograflar");
            Directory.CreateDirectory(klasor);   // klasor yoksa olustur

            // Guid ile benzersiz isim: ayni adli iki dosya birbirini ezmesin,
            // kullanicinin dosya adindaki tehlikeli karakterler diske yansimasin
            var dosyaAdi = $"{Guid.NewGuid()}{uzanti}";
            using var akis = System.IO.File.Create(Path.Combine(klasor, dosyaAdi));
            await fotograf.CopyToAsync(akis);

            return "/fotograflar/" + dosyaAdi;   // tarayicinin kullanacagi goreli yol
        }

        // Kayit silinince/degisince eski fotograf dosyasini diskten temizle
        private void FotografSil(string? yol)
        {
            if (string.IsNullOrEmpty(yol)) return;
            var tamYol = Path.Combine(_env.WebRootPath, yol.TrimStart('/'));
            if (System.IO.File.Exists(tamYol))
                System.IO.File.Delete(tamYol);
        }

        // ---------- ACTION'LAR ----------

        // GET: /Personeller?arama=ali&departmanId=2&sirala=maas_desc&sayfa=2
        public async Task<IActionResult> Index(string? arama, int? departmanId,
            bool sadeceAktif = false, string? sirala = null, int sayfa = 1)
        {
            var sorgu = FiltreliSorgu(arama, departmanId, sadeceAktif, sirala);

            // --- SAYFALAMA ---
            var toplamKayit = await sorgu.CountAsync();
            var toplamSayfa = Math.Max(1, (int)Math.Ceiling(toplamKayit / (double)SayfaBoyutu));
            sayfa = Math.Clamp(sayfa, 1, toplamSayfa);

            var personeller = await sorgu
                .Skip((sayfa - 1) * SayfaBoyutu)
                .Take(SayfaBoyutu)
                .ToListAsync();

            // Filtre formu ve linkler icin mevcut secimler
            ViewData["Arama"] = arama;
            ViewData["SecilenDepartmanId"] = departmanId;
            ViewData["SadeceAktif"] = sadeceAktif;

            // Filtre acilir listesi artik Departmanlar tablosundan geliyor
            var departmanlar = await _context.Departmanlar.OrderBy(d => d.Ad).ToListAsync();
            ViewData["DepartmanFiltre"] = new SelectList(departmanlar, "Id", "Ad", departmanId);

            // Siralama toggle degerleri
            ViewData["MevcutSirala"] = sirala;
            ViewData["AdSirala"] = string.IsNullOrEmpty(sirala) ? "ad_desc" : "";
            ViewData["DepartmanSirala"] = sirala == "departman" ? "departman_desc" : "departman";
            ViewData["MaasSirala"] = sirala == "maas" ? "maas_desc" : "maas";
            ViewData["TarihSirala"] = sirala == "tarih" ? "tarih_desc" : "tarih";

            // Sayfalama bilgileri
            ViewData["Sayfa"] = sayfa;
            ViewData["ToplamSayfa"] = toplamSayfa;
            ViewData["ToplamKayit"] = toplamKayit;

            return View(personeller);
        }

        // GET: /Personeller/DisariAktar?arama=...&departmanId=...
        // Mevcut filtrelerle eslesen TUM kayitlari (sayfalamasiz) CSV olarak indirir
        public async Task<IActionResult> DisariAktar(string? arama, int? departmanId,
            bool sadeceAktif = false, string? sirala = null)
        {
            var kayitlar = await FiltreliSorgu(arama, departmanId, sadeceAktif, sirala).ToListAsync();
            var tr = new System.Globalization.CultureInfo("tr-TR");

            // Ayirac olarak TAB, kodlama olarak UTF-16 kullaniyoruz.
            // Neden? UTF-8 BOM'u bazi Excel surumleri yok sayip Turkce
            // karakterleri bozuyor. UTF-16 imzasi ise her Excel'de taninir
            // ve tab'li sutunlar otomatik ayrilir - en garantili kombinasyon.
            var sb = new StringBuilder();
            sb.AppendLine("Ad Soyad\tDepartman\tPozisyon\tE-posta\tMaaş\tİşe Giriş Tarihi\tDurum");
            foreach (var p in kayitlar)
            {
                sb.AppendLine(string.Join('\t',
                    Csv(p.AdSoyad),
                    Csv(p.Departman!.Ad),
                    Csv(p.Pozisyon),
                    Csv(p.Eposta),
                    // Binlik ayracsiz yaz (120000,00): "120.000,00" bazi Excel
                    // ayarlarinda metin saniliyordu
                    p.Maas.ToString("0.00", tr),
                    p.IseGirisTarihi.ToString("dd.MM.yyyy"),
                    p.AktifMi ? "Aktif" : "Pasif"));
            }

            // Encoding.Unicode = UTF-16 LE; GetPreamble() dosya basina BOM imzasini koyar
            var icerik = Encoding.Unicode.GetPreamble()
                .Concat(Encoding.Unicode.GetBytes(sb.ToString()))
                .ToArray();

            // File(): tarayiciya "bunu sayfa olarak acma, dosya olarak indir" der
            return File(icerik, "text/csv", $"personeller_{DateTime.Now:yyyyMMdd_HHmm}.csv");
        }

        // Metin alanlarini CSV standardina gore tirnak icine al:
        // icinde ';' veya tirnak olsa bile hucre bolunmez
        private static string Csv(string deger) => "\"" + deger.Replace("\"", "\"\"") + "\"";

        // GET: /Personeller/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var personel = await _context.Personeller
                .Include(p => p.Departman)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (personel == null) return NotFound();

            return View(personel);
        }

        // GET: /Personeller/Create
        public async Task<IActionResult> Create()
        {
            await DepartmanListesiniYukleAsync();
            return View();
        }

        // POST: /Personeller/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("AdSoyad,DepartmanId,Pozisyon,Eposta,Maas,IseGirisTarihi,AktifMi")] Personel personel,
            IFormFile? fotograf)   // formdaki <input type="file" name="fotograf"> buraya baglanir
        {
            if (ModelState.IsValid)
            {
                // Once alanlar gecerli olsun, sonra fotografi diske yaz
                personel.FotografYolu = await FotografKaydetAsync(fotograf);

                if (ModelState.IsValid)   // fotograf dogrulamasi hata eklemis olabilir
                {
                    _context.Add(personel);
                    await _context.SaveChangesAsync();
                    TempData["Mesaj"] = $"\"{personel.AdSoyad}\" adlı personel başarıyla eklendi.";
                    return RedirectToAction(nameof(Index));
                }
            }

            await DepartmanListesiniYukleAsync(personel.DepartmanId);
            return View(personel);
        }

        // GET: /Personeller/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var personel = await _context.Personeller.FindAsync(id);
            if (personel == null) return NotFound();

            await DepartmanListesiniYukleAsync(personel.DepartmanId);
            return View(personel);
        }

        // POST: /Personeller/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,AdSoyad,DepartmanId,Pozisyon,Eposta,Maas,IseGirisTarihi,AktifMi,FotografYolu")] Personel personel,
            IFormFile? fotograf)
        {
            if (id != personel.Id) return NotFound();

            if (ModelState.IsValid)
            {
                // Yeni fotograf yuklendiyse eskisini diskten sil, yenisini kaydet.
                // Yuklenmediyse gizli alandan gelen mevcut FotografYolu korunur.
                if (fotograf != null && fotograf.Length > 0)
                {
                    var yeniYol = await FotografKaydetAsync(fotograf);
                    if (ModelState.IsValid)
                    {
                        FotografSil(personel.FotografYolu);
                        personel.FotografYolu = yeniYol;
                    }
                }

                if (ModelState.IsValid)
                {
                    _context.Update(personel);
                    await _context.SaveChangesAsync();
                    TempData["Mesaj"] = $"\"{personel.AdSoyad}\" adlı personel başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
            }

            await DepartmanListesiniYukleAsync(personel.DepartmanId);
            return View(personel);
        }

        // GET: /Personeller/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var personel = await _context.Personeller
                .Include(p => p.Departman)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (personel == null) return NotFound();

            return View(personel);
        }

        // POST: /Personeller/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var personel = await _context.Personeller.FindAsync(id);
            if (personel != null)
            {
                FotografSil(personel.FotografYolu);   // dosya coplugu birakma
                _context.Personeller.Remove(personel);
                await _context.SaveChangesAsync();
                TempData["Mesaj"] = $"\"{personel.AdSoyad}\" adlı personel silindi.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
