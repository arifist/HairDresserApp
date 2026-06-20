# Kuaför Arif — Online Randevu Sistemi

Tek bir kuaför ("Arif") için kurumsal tanıtım sayfaları + online randevu sistemi + SMS bildirimleri + admin paneli içeren, sıfırdan geliştirilmiş bir web uygulaması.

## Teknolojiler
- **.NET 9** — Blazor Web App (global render mode: Interactive Server)
- **MSSQL** + **Entity Framework Core** (code-first migrations)
- **Netgsm** SMS API entegrasyonu (kimlik bilgisi girilmediğinde otomatik simülasyon moduna düşer)
- Cookie tabanlı kimlik doğrulama (müşteri: telefon + SMS OTP, admin: kullanıcı adı + şifre)
- Bootstrap 5 + özel "siyah/gold" berber teması, Bootstrap Icons, Google Fonts (Poppins / Playfair Display)

## Proje Yapısı
```
BerberArif/
  Components/Layout      MainLayout, NavMenu, Footer, AdminLayout, AdminNavMenu
  Components/Pages        Anasayfa, Hakkımda, Hizmetlerimiz, İletişim, Giriş, Randevu Al, Randevularım
  Components/Pages/Admin  Admin Girişi, Panel, Randevular, Çalışma Saatleri, Hizmetler
  Data                    AppDbContext, SeedData, Migrations
  Models                  Customer, Admin, ServiceType, WorkingHour, Appointment, OtpVerification, SmsLog, ContactMessage
  Services                ISmsService/NetgsmSmsService, IOtpService/OtpService, IAppointmentService/AppointmentService
  Endpoints               AuthEndpoints (OTP doğrulama, admin girişi, çıkış — minimal API)
```

## Kurulum

### Gereksinimler
- .NET 9 SDK
- MSSQL (LocalDB, SQL Server Express veya tam SQL Server)

### Adımlar
1. `BerberArif/appsettings.json` içindeki `ConnectionStrings:DefaultConnection` değerini kendi MSSQL ortamınıza göre düzenleyin. Varsayılan değer LocalDB kullanır:
   ```
   Server=(localdb)\MSSQLLocalDB;Database=BerberArifDb;Trusted_Connection=True;TrustServerCertificate=True;
   ```
2. Projeyi çalıştırın — veritabanı ve tablolar **otomatik olarak** oluşturulur ve örnek veriler (admin kullanıcı, hizmetler, çalışma saatleri) otomatik eklenir:
   ```
   cd BerberArif
   dotnet run
   ```
3. Tarayıcıda `http://localhost:5280` (veya konsolda görünen adres) adresine gidin.

> Not: `NuGet.Config` dosyası bu çözümde yalnızca nuget.org kaynağını kullanacak şekilde ayarlanmıştır (geliştirme ortamında erişilemeyen bir kurumsal NuGet kaynağı sorun çıkardığı için eklenmiştir). Farklı bir ortamda paket geri yükleme sorunu yaşarsanız bu dosyayı kontrol edin.

## Varsayılan Admin Girişi
- Adres: `/admin/giris`
- Kullanıcı adı: `arif`
- Şifre: `Arif2026!`

**Üretime almadan önce bu şifreyi değiştirmeniz önerilir** (doğrudan veritabanında `Admins` tablosunda güncellenebilir; ileride admin panelden şifre değiştirme ekranı eklenebilir).

## SMS (Netgsm) Yapılandırması
`appsettings.json` içindeki `Netgsm` bölümüne kendi hesap bilgilerinizi girin:
```json
"Netgsm": {
  "UserCode": "...",
  "Password": "...",
  "Header": "...",
  "ApiUrl": "https://api.netgsm.com.tr/sms/send/get"
}
```
Bu alanlar boş bırakıldığında SMS'ler **gerçekten gönderilmez**; sistem otomatik olarak "simülasyon modu"na geçer ve mesajı veritabanındaki `SmsLogs` tablosuna ve uygulama loglarına yazar. Bu sayede Netgsm hesabınız olmadan da projeyi uçtan uca test edebilirsiniz:
- Müşteri girişinde (`/giris`) doğrulama kodu istediğinizde, simülasyon modundaysa kod ekranda "Test modu" uyarısı içinde de gösterilir.
- Randevu onay/iptal SMS'leri de aynı şekilde `SmsLogs` tablosuna yazılır.

Gerçek bir Netgsm hesabı girildiğinde davranış otomatik olarak gerçek SMS gönderimine döner — kod değişikliği gerekmez.

## Randevu Mantığı
- Hizmet türüne göre süre otomatik belirlenir: **Saç Kesimi** 30 dk, **Sakal Tıraşı** 30 dk, **Saç + Sakal** 60 dk, **Diğer** 1 dk (bu süreler `/admin/hizmetler` sayfasından kolayca değiştirilebilir).
- Müşteriler yalnızca **bugünden itibaren 7 gün içindeki** tarihler için randevu alabilir (bu süre `appsettings.json` → `AppSettings:MaxBookingDaysAhead` ile değiştirilebilir).
- Müsait saatler, admin panelde tanımlanan çalışma saatlerine (`/admin/calisma-saatleri`) ve mevcut randevulara göre dinamik olarak hesaplanır; müşteri sadece boş saatleri görür ve seçebilir.
- Admin, `/admin/randevular` sayfasından herhangi bir randevuyu iptal edebilir; iptal anında müşteriye otomatik SMS bildirimi gönderilir.

## Geliştirme Notları
- Veritabanı migration'ları `BerberArif/Migrations` klasöründedir. Modelde değişiklik yaparsanız:
  ```
  dotnet ef migrations add <İsim>
  ```
  komutuyla yeni migration oluşturabilirsiniz (uygulama açılışta migration'ları otomatik uygular, manuel `database update` gerekmez).
- Build kontrolü: `dotnet build`
