# ☕ Postgre - Kafe Yönetim ve Rezervasyon Sistemi

Postgre; kafe müşterilerinin menüyü inceleyip rezervasyon yapabildiği, işletme yetkililerinin ise ürün, masa ve rezervasyon süreçlerini yönetebildiği çift panelli bir web projesidir. ASP.NET Core 9, PostgreSQL ve katmanlı mimari kullanılarak geliştirilmiştir.

---

## 📸 Ekran Görüntüleri

### 👤 Kullanıcı Arayüzü
<img width="1840" height="872" alt="image" src="https://github.com/user-attachments/assets/4a6cb5ae-9742-4267-ab83-ac7d78e7b8fd" />

<img width="1821" height="872" alt="image" src="https://github.com/user-attachments/assets/7be3e625-f0b2-42b2-bb4a-0ac5229158c6" />

<img width="1827" height="872" alt="image" src="https://github.com/user-attachments/assets/10da5655-95a9-4ac5-98f3-70e868ffcb49" />

<img width="1830" height="876" alt="image" src="https://github.com/user-attachments/assets/9b47a595-32fc-435a-bd96-729107952f00" />

<img width="1810" height="867" alt="image" src="https://github.com/user-attachments/assets/f464a98f-9dda-4a00-af86-43bf09cc23b3" />

<img width="1810" height="868" alt="image" src="https://github.com/user-attachments/assets/bf6516c0-c528-4801-88c0-e91dcbb29973" />

<img width="1817" height="876" alt="image" src="https://github.com/user-attachments/assets/09a426fe-d79f-4788-bbd1-711c1b1f1bf8" />

### 🛡️ Yönetici Paneli
<img width="1818" height="876" alt="image" src="https://github.com/user-attachments/assets/670cbc35-8604-4d9f-9bca-0d49ee072b67" />

<img width="1830" height="872" alt="image" src="https://github.com/user-attachments/assets/64df1af7-fcb5-4574-8be2-5c55d28eb2ed" />

<img width="1825" height="875" alt="image" src="https://github.com/user-attachments/assets/4b8af046-969e-4480-8780-3d7f349e5e7b" />

<img width="1831" height="872" alt="image" src="https://github.com/user-attachments/assets/a1c8cbed-f789-4000-a56a-71fb8f8f61b3" />

<img width="1817" height="871" alt="image" src="https://github.com/user-attachments/assets/6e91ac37-4969-4bcd-a33f-2c99a004e274" />

<img width="1848" height="866" alt="image" src="https://github.com/user-attachments/assets/92cca200-6626-4861-a722-7c231c5d9d51" />

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
