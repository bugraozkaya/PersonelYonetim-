# Personel Yönetim Sistemi

Bir şirketin çalışan kayıtlarını tutan, ASP.NET Core MVC ile geliştirilmiş CRUD web uygulaması. Staj projesi olarak adım adım geliştirilmiştir.

## Kullanılan Teknolojiler

| Katman | Teknoloji |
|---|---|
| Dil | C# (.NET 10) |
| Framework | ASP.NET Core MVC |
| ORM | Entity Framework Core |
| Veritabanı | SQLite (tablolar DB Browser ile elle oluşturuldu) |
| Arayüz | Razor View + Bootstrap 5 |
| Doğrulama | Data Annotations (sunucu) + jQuery Validation (istemci) |

## Özellikler

- **Tam CRUD**: Personel ekleme, listeleme, detay görüntüleme, güncelleme ve onaylı silme
- **İlişkisel veri modeli**: Departmanlar ayrı tabloda; Personel → Departman arasında foreign key ile 1-e-çok ilişki, sorgularda `Include()` (JOIN) kullanımı
- **Departman yönetimi**: Tek sayfadan departman ekleme/silme; içinde personel olan departman silinemez (referans bütünlüğü koruması)
- **Fotoğraf yükleme**: `IFormFile` ile .jpg/.png yükleme (2 MB sınırı), Guid ile benzersiz adlandırma, `wwwroot/fotograflar` altında saklama; listede avatar, detayda büyük görünüm
- **CSV dışa aktarma**: Aktif filtrelerle eşleşen kayıtları tek tuşla indirme (UTF-16 + tab ayıracı ile tüm Excel sürümlerinde Türkçe karakter uyumlu)
- **Arama ve filtreleme**: Ad/e-posta araması, departman açılır listesi, "sadece aktifler" filtresi
- **Sıralama**: Sütun başlığına tıklayarak artan/azalan sıralama (▲/▼ göstergeli)
- **Sayfalama**: Sayfa başına 5 kayıt, `Skip/Take` ile veritabanı seviyesinde
- **Kültür yönetimi**: Uygulama tr-TR kültürüne sabitlendi; virgüllü ondalık (120000,00) hem sunucu hem istemci doğrulamasında sorunsuz çalışır
- **Güvenlik**: Tüm POST işlemlerinde `[ValidateAntiForgeryToken]` (CSRF koruması), `[Bind]` ile overposting engeli, dosya yüklemede uzantı/boyut denetimi
- **Kullanıcı geri bildirimi**: İşlem sonrası TempData ile başarı/hata mesajları
- **Asenkron veri erişimi**: Tüm veritabanı işlemlerinde `async/await`

## Proje Yapısı

```
PersonelYonetim/
├── Controllers/
│   ├── PersonellerController.cs   # CRUD + arama/sıralama/sayfalama + CSV + fotoğraf
│   └── DepartmanlarController.cs  # Departman listeleme/ekleme/silme
├── Data/
│   └── UygulamaDbContext.cs       # EF Core DbContext (2 tablo)
├── Models/
│   ├── Personel.cs                # Entity + Data Annotations + FK (DepartmanId)
│   └── Departman.cs               # Departman entity'si
├── Views/
│   ├── Personeller/               # Index, Create, Edit, Details, Delete
│   ├── Departmanlar/              # Index (ekleme/silme aynı sayfada)
│   └── Shared/_Layout.cshtml      # Ortak şablon (navbar, TempData mesajı)
├── wwwroot/
│   └── fotograflar/               # Yüklenen personel fotoğrafları
├── Program.cs                     # DI kayıtları, tr-TR kültür ayarı, rota tanımı
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

Not: `personel.db` repoya dahil edilmediği için ilk çalıştırmadan önce SQLite'ta tabloların oluşturulması gerekir:

```sql
CREATE TABLE "Departmanlar" (
    "Id" INTEGER NOT NULL,
    "Ad" TEXT NOT NULL,
    PRIMARY KEY("Id" AUTOINCREMENT)
);

CREATE TABLE "Personeller" (
    "Id"             INTEGER NOT NULL,
    "AdSoyad"        TEXT NOT NULL,
    "DepartmanId"    INTEGER NOT NULL,
    "Pozisyon"       TEXT NOT NULL,
    "Eposta"         TEXT NOT NULL,
    "Maas"           TEXT NOT NULL,
    "IseGirisTarihi" TEXT NOT NULL,
    "AktifMi"        INTEGER NOT NULL DEFAULT 1,
    "FotografYolu"   TEXT NULL,
    PRIMARY KEY("Id" AUTOINCREMENT)
);
```

## Geliştirme Sürecinde Öğrendiklerim

- **MVC deseni**: Model (veri), View (arayüz) ve Controller (akış) sorumluluklarının ayrılması
- **ORM mantığı**: SQL yazmadan LINQ ile veritabanı sorgulama; `IQueryable` ile sorgunun veritabanına gitmeden önce parça parça inşa edilmesi
- **İlişkisel tasarım**: Serbest metin yerine foreign key kullanmanın veri tutarlılığına katkısı; navigation property ve `Include()` ile JOIN
- **Dependency Injection**: DbContext'in `Program.cs`'te kaydedilip controller'a otomatik verilmesi
- **GET/POST ayrımı**: Veri değiştiren işlemlerin neden asla GET ile yapılmaması gerektiği
- **Dosya yükleme**: `multipart/form-data` zorunluluğu, dosyayı diske / yolunu veritabanına yazma yaklaşımı, uzantı ve boyut denetimi
- **Kültür (localization) sorunları**: Türkçe ondalık virgülünün hem sunucu (request localization) hem istemci (jQuery validator override) tarafında ele alınması
- **Web güvenliği temelleri**: CSRF (antiforgery token) ve overposting (`[Bind]`) korumaları
- **Pratik dersler**:
  - Elle tablo oluştururken tablo adına karışan görünmez boşluk, EF'in tabloyu bulamamasına yol açtı
  - Razor'da `metin@degisken` yazımı e-posta sanılıyor; çözüm `@(degisken)`
  - Excel'de `sep=;` satırı UTF-8 BOM'u geçersiz kılıyor; kesin çözüm UTF-16 + tab ayıracı
  - Excel'in `########` göstermesi verinin yok olduğu değil, sütunun dar olduğu anlamına geliyor
