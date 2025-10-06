# Keylogger - Eğitim Amaçlı

Bu proje, C# ve Windows Forms kullanılarak geliştirilmiş eğitim amaçlı bir keylogger uygulamasıdır.

## ⚠️ ÖNEMLİ UYARI
Bu uygulama sadece eğitim amaçlıdır. Başkalarının bilgisayarlarında izinsiz kullanımı yasaktır ve etik değildir. Sadece kendi bilgisayarınızda veya açık izin alınmış test ortamlarında kullanın.

## Özellikler

- Klavye tuş basımlarını gerçek zamanlı yakalama
- Yakalanan tuşları dosyaya kaydetme
- Basit Windows Forms arayüzü
- HTTP üzerinden log gönderme
- Basit HTTP sunucu (log alma)

## Nasıl Çalışır

### 1. Projeyi Derleme
```bash
dotnet build
```

### 2. Ana Uygulamayı Çalıştırma
```bash
dotnet run
```

### 3. Sunucuyu Çalıştırma (Ayrı Terminal)
Sunucuyu çalıştırmak için ayrı bir terminal açın:
```bash
dotnet run --project . ServerApp.cs
```

## Kullanım

### Ana Uygulama (Keylogger)
1. Uygulamayı çalıştırın
2. "Başlat" butonuna tıklayın
3. Tuş basımları otomatik olarak kaydedilmeye başlar
4. Log alanında gerçek zamanlı olarak tuş basımlarını görebilirsiniz
5. "Log Gönder" butonu ile sunucuya veri gönderebilirsiniz

### Sunucu Uygulaması
1. Sunucu uygulamasını çalıştırın
2. Sunucu http://localhost:8080 adresinde dinlemeye başlar
3. Gelen keylog verileri "Belgelerim/ReceivedKeylogs" klasörüne kaydedilir

## Dosya Yapısı

- `Program.cs` - Ana giriş noktası
- `MainForm.cs` - Windows Forms arayüzü
- `KeyloggerEngine.cs` - Keylogger ana mantığı
- `SimpleServer.cs` - HTTP sunucu sınıfı
- `ServerApp.cs` - Sunucu konsol uygulaması

## Teknik Detaylar

### Windows API Kullanımı
Uygulama, Windows'un düşük seviye klavye hook'larını kullanır:
- `SetWindowsHookEx` - Klavye hook'u kurar
- `WH_KEYBOARD_LL` - Düşük seviye klavye hook'u
- `CallNextHookEx` - Hook zincirini devam ettirir

### Log Formatı
```
2024-01-15 14:30:25 - A
2024-01-15 14:30:26 - [SPACE]
2024-01-15 14:30:27 - B
```

## Güvenlik Notları

1. **Antivirüs Uyarıları**: Bu tür uygulamalar antivirüs yazılımları tarafından tehdit olarak algılanabilir
2. **Yönetici Yetkileri**: Bazı durumlarda yönetici yetkileri gerekebilir
3. **Gizlilik**: Hassas bilgilerin loglanmaması için dikkatli olun

## Test Senaryosu

Arkadaşınızın bilgisayarında test etmek için:

1. Arkadaşınızın bilgisayarında keylogger uygulamasını çalıştırın
2. Kendi bilgisayarınızda sunucu uygulamasını çalıştırın
3. Arkadaşınızın bilgisayarındaki uygulamada sunucu URL'sini kendi IP adresiniz olarak ayarlayın
4. "Log Gönder" butonu ile verileri kendi bilgisayarınıza gönderin

## Yasal Uyarı

Bu yazılımı kullanmadan önce:
- Yerel yasaları kontrol edin
- Sadece kendi cihazlarınızda veya açık izin alınmış ortamlarda kullanın
- Başkalarının gizliliğini ihlal etmeyin
- Eğitim amaçlı kullanımla sınırlı tutun