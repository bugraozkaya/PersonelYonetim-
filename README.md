# Personel Yönetim Sistemi

Bir şirketin çalışan kayıtlarını tutan, ASP.NET Core MVC ile geliştirilmiş CRUD web uygulaması. Staj projesi olarak adım adım geliştirilmiştir.

## Kullanılan Teknolojiler

| Katman | Teknoloji |
|---|---|
| Dil | C# (.NET 10) |
| Framework | ASP.NET Core MVC |
| ORM | Entity Framework Core |
| Veritabanı | SQLite (tablo DB Browser ile elle oluşturuldu) |
| Arayüz | Razor View + Bootstrap 5 |
| Doğrulama | Data Annotations (sunucu) + jQuery Validation (istemci) |

## Özellikler

- **Tam CRUD**: Personel ekleme, listeleme, detay görüntüleme, güncelleme ve onaylı silme
- **Çift katmanlı form doğrulama**: Türkçe hata mesajlarıyla hem sunucu (`ModelState`) hem tarayıcı (jQuery Validation) tarafında
- **Arama ve filtreleme**: Ad/e-posta araması, departman açılır listesi, "sadece aktifler" filtresi
- **Sıralama**: Sütun başlığına tıklayarak artan/azalan sıralama (▲/▼ göstergeli)
- **Sayfalama**: Sayfa başına 5 kayıt, `Skip/Take` ile veritabanı seviyesinde
- **Güvenlik**: Tüm POST işlemlerinde `[ValidateAntiForgeryToken]` (CSRF koruması), `[Bind]` ile overposting engeli
- **Kullanıcı geri bildirimi**: İşlem sonrası TempData ile başarı mesajları
- **Asenkron veri erişimi**: Tüm veritabanı işlemlerinde `async/await`

## Proje Yapısı

```
PersonelYonetim/
├── Controllers/
│   └── PersonellerController.cs   # CRUD + arama/sıralama/sayfalama mantığı
├── Data/
│   └── UygulamaDbContext.cs       # EF Core DbContext
├── Models/
│   └── Personel.cs                # Entity + Data Annotations doğrulamaları
├── Views/
│   ├── Personeller/               # Index, Create, Edit, Details, Delete
│   └── Shared/_Layout.cshtml      # Ortak şablon (navbar, TempData mesajı)
├── Program.cs                     # DI kayıtları, rota tanımı
├── appsettings.json               # SQLite bağlantı dizesi
└── personel.db                    # Veritabanı (repoya dahil değildir)
```

## Kurulum ve Çalıştırma

.NET SDK (8+) kurulu olmalıdır.

```bash
git clone https://github.com/bugraozkaya/PersonelYonetim-.git
cd PersonelYonetim-
dotnet restore
dotnet run
```

Not: `personel.db` repoya dahil edilmediği için ilk çalıştırmadan önce SQLite'ta `Personeller` tablosunun oluşturulması gerekir:

```sql
CREATE TABLE "Personeller" (
    "Id"             INTEGER NOT NULL,
    "AdSoyad"        TEXT NOT NULL,
    "Departman"      TEXT NOT NULL,
    "Pozisyon"       TEXT NOT NULL,
    "Eposta"         TEXT NOT NULL,
    "Maas"           TEXT NOT NULL,
    "IseGirisTarihi" TEXT NOT NULL,
    "AktifMi"        INTEGER NOT NULL DEFAULT 1,
    PRIMARY KEY("Id" AUTOINCREMENT)
);
```

## Geliştirme Sürecinde Öğrendiklerim

- **MVC deseni**: Model (veri), View (arayüz) ve Controller (akış) sorumluluklarının ayrılması
- **ORM mantığı**: SQL yazmadan LINQ ile veritabanı sorgulama; `IQueryable` ile sorgunun veritabanına gitmeden önce parça parça inşa edilmesi
- **Dependency Injection**: DbContext'in `Program.cs`'te kaydedilip controller'a otomatik verilmesi
- **GET/POST ayrımı**: Veri değiştiren işlemlerin neden asla GET ile yapılmaması gerektiği
- **Web güvenliği temelleri**: CSRF (antiforgery token) ve overposting (`[Bind]`) korumaları
- **Pratik dersler**: Elle tablo oluştururken tablo adına karışan görünmez boşluğun EF'in tabloyu bulamamasına yol açması; Razor'da `metin@degisken` yazımının e-posta sanılması ve `@(degisken)` ile çözülmesi
