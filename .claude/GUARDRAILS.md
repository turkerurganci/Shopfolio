# AI Sınırları ve Yasakları

**Katman:** L3 | **Son güncelleme:** 2026-07-27

> Bu dosya "yapılmayacaklar"ı tanımlar. Yapılacaklar [`INSTRUCTIONS.md`](INSTRUCTIONS.md)'de.

---

## 1. Aşama disiplini

- Bulunduğun aşama teknik değilse teknik detaya girme: *"Bu teknik bir karar, şimdi ürün kararını alalım."*
- Teknoloji seçimi, mimari, API, veritabanı yalnızca ilgili aşamada konuşulur.
- Hangi aşamada hangi konunun konuşulacağı: `Docs/00_PROJECT_METHODOLOGY.md` §C.1.

---

## 2. Değiştirilemez dosyalar

Proje sahibinden **açık onay** almadan değiştirilemez:

- `Docs/00_PROJECT_METHODOLOGY.md` — metodolojinin yapısı
- `CLAUDE.md` — AI giriş noktası
- `SETUP.md` — kurulum parametreleri (bir kez doldurulur; değişimi rejim değişimidir)

---

## 3. Onay gerektiren aksiyonlar

- Yeni doküman oluşturma
- Mevcut dokümanda yapısal değişiklik (bölüm ekleme/silme)
- Kapsam değişikliği (MVP'ye özellik ekleme/çıkarma)
- Karar değiştirme (daha önce alınmış bir kararı revize etme)
- Geri alınamaz git işlemleri (force-push, hard reset, dal silme)
- Ana dala merge, deploy, dış sisteme mesaj

---

## 4. Yıkıcı önerilerde kendini sorgula

- Dosya silme, birleştirme, kaldırma veya büyük yapısal değişiklik önermeden önce *"bu gerçekten gerekli mi?"* sorusunu sor.
- Bir şeyin "gereksiz kaldığını" düşünmek yeterli değil — hâlâ taşıdığı veya gelecekte taşıyacağı değeri de değerlendir.
- **Varsayılan tavır korumaktır, silmek değil.** Silme ancak açık bir gerekçe varsa önerilir.

---

## 5. Doküman kuralları

- "Muhtemelen", "belki", "olabilir", "gereksinime göre belirlenecek" gibi belirsiz ifadeler doküman içinde kullanılamaz.
- Her doküman diğerleriyle tutarlı olmalı; tutarsızlık fark edilirse düzeltme önerilmeli.
- Doküman versiyonları korunur, sessizce üzerine yazılmaz.
- **Ölü placeholder bırakma:** "Sonra doldurulacak" satırı, kim tarafından ne zaman doldurulacağı bir kapıya bağlanmadan yazılmaz. Sahipsiz placeholder dosyanın kendisini çürütür.

---

## 6. Karar kuralları

- AI kendi başına karar almaz, proje sahibinden onay ister.
- Birden fazla seçenek sunduğunda öneri belirtir ama "ben bunu uyguluyorum" demez.
- Soru sorduğunda cevabını almadan başka konuya geçmez.
- Bir öneriyi savunur; kullanıcının her itirazına refleks olarak "haklısın" demez.

---

## 7. Yerleşim kontrolü

- "Bunu X dosyasına ekle" dendiğinde **körü körüne uyma** — içeriğin gerçekten oraya ait olup olmadığını değerlendir.
- Yer yanlışsa doğru yeri **gerekçesiyle** öner. Bu, mevcut bir dosyada farklı bir bölüm olabileceği gibi yeni bir dosya da olabilir.

---

## 8. Süreç yasakları

Aşağıdakiler koşulsuz yasaktır:

**Disiplin:**
- Kırık CI üstüne task başlatmak veya doğrulama yapmak
- CI sonucunu beklemeden "yapım bitti" demek
- Başka bir task'ın PR'ına commit gömmek (bundled PR)
- Doğrulamayı yapım chat'inde yapmak
- Yapım chat'inin kendi PR'ını merge etmesi
- Post-merge kozmetik düzeltmeyi doğrudan ana dala push etmek
- CI izleme sorumluluğunu kullanıcıya sormak veya devretmek

**Kapsam ve kural:**
- Dokümanda tanımlı olmayan iş kuralı uydurmak
- Sessizce kapsam genişletmek
- Geçici hack'i kalıcı çözüm gibi bırakmak
- Bilinen riskli kodu "sonra düzeltiriz" diyerek kayıtsız bırakmak
- Ertelenen bir işi backlog'a yazmadan ertelemek

**Kanıt:**
- Kanıtsız PASS vermek
- Eski veya başka ortamdan alınan kanıtı kullanmak
- Dış varsayımı doğrulamadan implementasyona başlamak
- "Lokal makinemde geçiyor" ile CI'yi ikame etmek

**Güvenlik:**
- Secret'ı koda gömmek
- Hassas veriyi loglamak
- Yetkilendirme kontrolünü yalnızca istemci tarafına bırakmak
- Webhook/callback imza doğrulamasını atlamak veya geçici devre dışı bırakmak
