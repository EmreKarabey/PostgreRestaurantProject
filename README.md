# ☕ Postgre - Kafe Yönetim ve Rezervasyon Sistemi

Postgre; kafe müşterilerinin menüyü inceleyip rezervasyon yapabildiği, işletme yetkililerinin ise ürün, masa ve rezervasyon süreçlerini yönetebildiği çift panelli bir web projesidir. ASP.NET Core 9, PostgreSQL ve katmanlı mimari kullanılarak geliştirilmiştir.

---

## 📸 Ekran Görüntüleri

### 👤 Kullanıcı Arayüzü

> Ekran görüntülerini `docs/images/` klasörüne ekleyip aşağıdaki görsel yollarını kullanın.

| Ana Sayfa | Menü |
|---|---|
| ![Kullanıcı ana sayfası](docs/images/user-home.png) | ![Menü sayfası](docs/images/user-menu.png) |

| Rezervasyon | Giriş / Kayıt |
|---|---|
| ![Rezervasyon sayfası](docs/images/user-reservation.png) | ![Giriş ve kayıt sayfası](docs/images/user-auth.png) |

### 🛡️ Yönetici Paneli

| Yönetim Paneli | Ürün ve Kategori Yönetimi |
|---|---|
| ![Yönetim paneli](docs/images/admin-dashboard.png) | ![Ürün ve kategori yönetimi](docs/images/admin-products.png) |

| Rezervasyon Yönetimi | Masa ve QR Yönetimi |
|---|---|
| ![Rezervasyon yönetimi](docs/images/admin-reservations.png) | ![Masa ve QR yönetimi](docs/images/admin-tables-qr.png) |

---

## 🌟 Öne Çıkan Özellikler

### 👤 1. Müşteri & Kullanıcı Arayüzü (`PostgreDB`)

- **Menü görüntüleme:** Kafe ürün ve kategorilerini görüntüleme.
- **Hesap işlemleri:** Üye olma ve oturum açma.
- **Rezervasyon:** Rezervasyon oluşturma ve kendi rezervasyonlarını görüntüleme.
- **Değerlendirmeler:** Müşteri yorumlarını görüntüleme.
- **Gerçek zamanlı altyapı:** SignalR bağlantısı.

### 🛡️ 2. Yönetici Paneli (`PostgreAdminDB`)

- **Gösterge paneli:** Yönetim ekranına genel bakış.
- **Ürün ve kategori yönetimi:** Menü içeriğini düzenleme.
- **Rezervasyon ve masa yönetimi:** Rezervasyonları ve restoran masalarını yönetme.
- **QR kodları:** Masa QR kodları ekranı.
- **Kullanıcı ve rol yönetimi:** Yönetici kullanıcılarını ve rollerini düzenleme.
- **Yorum ve profil yönetimi.**
- **Görsel depolama:** Cloudflare R2 servis entegrasyonu.

---

## 🏗️ Mimari & Katmanlı Yapı

```text
PostgreCofee/
├── PostgreDB/                         # Kullanıcı web uygulaması ve katmanları
│   ├── PostgreDB/                     # ASP.NET Core MVC arayüzü
│   ├── Application/                   # Uygulama servisleri ve kullanım durumları
│   ├── Domain/                        # Alan modeli
│   ├── Infrastructure/                # Harici servisler ve altyapı
│   └── Persistence/                   # EF Core ve veritabanı erişimi
│
├── PostgreAdminDB/                    # Yönetim paneli ve katmanları
│   ├── CofeeAdmin/                    # ASP.NET Core MVC yönetim arayüzü
│   ├── BusinessLayer/                 # İş servisleri
│   ├── DataAccessLayer/               # Veri erişimi ve repository'ler
│   └── EntityLayer/                   # Varlıklar ve DTO'lar
│
└── docs/images/                       # README ekran görüntüleri
```

---

## 🛠️ Kullanılan Teknolojiler

| Kategori | Teknoloji / Kütüphane |
|---|---|
| Çatı (Framework) | .NET 9, ASP.NET Core MVC |
| Programlama dili | C# |
| Veritabanı | PostgreSQL, Entity Framework Core, Npgsql |
| Mimari ve desenler | Katmanlı mimari, Repository, Dependency Injection |
| Kimlik doğrulama | JWT; yönetim panelinde ASP.NET Core Identity |
| Gerçek zamanlı iletişim | ASP.NET Core SignalR |
| Medya depolama | Cloudflare R2 (S3 uyumlu API) |
| Ön yüz | HTML, CSS, JavaScript, Bootstrap, Razor Views |

---

## 🚀 Kurulum ve Çalıştırma

### 1. Gereksinimler

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL](https://www.postgresql.org/download/)
- Görsel depolama kullanılacaksa Cloudflare R2 hesabı
- E-posta gönderimi kullanılacaksa SMTP hesabı

### 2. Yapılandırma

İki uygulama da `DefaultConnection` ayarını bekler:

- `PostgreDB/PostgreDB/appsettings.json`
- `PostgreAdminDB/CofeeAdmin/appsettings.json`

Bağlantı örneği:

```text
Host=localhost;Port=5432;Database=cofeedb;Username=postgres;Password=YOUR_PASSWORD
```

Gizli değerleri `appsettings.json` dosyasına veya Git'e koymayın. Geliştirme ortamında User Secrets ile tanımlayın. Komutları her uygulamanın kendi proje klasöründe çalıştırın:

```powershell
# Her iki uygulama için veritabanı bağlantısı
 dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=cofeedb;Username=postgres;Password=YOUR_PASSWORD"

# İki uygulama için de en az 32 byte uzunluğunda JWT anahtarı gerekir
 dotnet user-secrets set "Jwt:SecretKey" "YOUR_LONG_RANDOM_SECRET_KEY_AT_LEAST_32_BYTES"
```

Yönetim paneli ayrıca aşağıdaki R2 ayarlarını bekler:

```powershell
 dotnet user-secrets set "CloudflareR2:AccessKey" "YOUR_R2_ACCESS_KEY"
 dotnet user-secrets set "CloudflareR2:SecretKey" "YOUR_R2_SECRET_KEY"
 dotnet user-secrets set "CloudflareR2:ServiceUrl" "https://<account-id>.r2.cloudflarestorage.com"
```

SMTP ve diğer servis ayarlarını ihtiyaç duyduğunuz özelliklere göre yapılandırın. PostgreSQL veritabanını oluşturduktan sonra şema/migration durumunu projedeki migration'lara göre hazırlayın.

### 3. Uygulamayı Başlatma

**Kullanıcı Arayüzü:**

```powershell
cd PostgreDB/PostgreDB
dotnet run
```

**Yönetim Paneli:**

```powershell
cd PostgreAdminDB/CofeeAdmin
dotnet run
```

Uygulamaların yerel adresleri ilgili `Properties/launchSettings.json` dosyalarında tanımlıdır; çalıştırma sırasında terminalde gösterilen adresleri açın.

---

## 🔐 Güvenlik Notu

JWT anahtarları, veritabanı parolaları, SMTP bilgileri ve R2 anahtarları gizli tutulmalıdır. Üretim ortamında User Secrets yerine uygun bir secret yönetim çözümü ve HTTPS kullanın.

---

## 📄 Lisans

Bu depoda lisans dosyası bulunmuyor. Projeyi dağıtmadan veya yeniden kullanmadan önce lisans ekleyin.
