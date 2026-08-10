# Shopfolio — User Flows

**Versiyon: v0.1** | **Bağımlılıklar:** `01_PROJECT_VISION.md`, `02_PRODUCT_REQUIREMENTS.md` | **Son güncelleme:** YYYY-AA-GG

> **Aşama:** 2 — Kullanıcı Akışları · **Rol:** Product Owner / Business Analyst
> **Traceability zorunlu:** Hayır (doğrudan türetim) — ama akışlar yazıldıktan sonra `02`'ye **geri dönülür**, tutarsızlık varsa düzeltilir.

---

## 0. Nasıl yazılır

- **Aktör bazlı ilerleme:** Her aktör için ayrı akış seti.
- **Normal akış + hata akışları:** Önce "her şey yolunda", sonra *"ne ters gidebilir?"* ile hata ve alternatif akışlar. **Hata akışları happy path kadar önemlidir.**
- **Durum makinesi mantığı:** Her adım bir durumdan diğerine geçiştir. Durum isimleri `02 §5` ile birebir aynı.
- **Bildirim entegrasyonu:** Her adımda *"burada kime bildirim gider?"* sorulur; tetikleyiciler akışa gömülür ve §7'de özetlenir.
- **Narrative gerektiren senaryolar:** Bazı edge case'ler if/then ile anlatılamaz — uçtan uca anlatım gerektirir. Onları ayrı bölümde yaz.

---

## 1. Durum makinesi

> **Ne yazılır:** Tüm durumlar, izin verilen geçişler, yasak geçişler, her geçişin tetikleyicisi. **UI tasarımından önce netleşmelidir** — ekran varyantları buradan türetilir.

| Durum | Anlamı | Sonraki olası durumlar | Tetikleyici |
|---|---|---|---|

## 2. Ana akışlar (aktör bazlı)

### 2.x <Aktör> — <Akış adı>
> **Ne yazılır:** Adım adım. Her adımda: kullanıcı ne yapar · sistem ne kontrol eder · durum ne olur · kime bildirim gider.

## 3. Hata akışları

> **Ne yazılır:** Timeout, iptal, doğrulama hatası, dış servis hatası senaryoları.

## 4. Zaman aşımı yönetimi

> **Ne yazılır:** Tüm timeout akışları **tek bölümde** — aktör akışlarına gömülmez. Uyarı, dondurma ve normale dönüş adımları dahil.

## 5. İtiraz / anlaşmazlık akışları

## 6. Kötüye kullanım ve inceleme akışları

## 7. Bildirim haritası

> **Ne yazılır:** Akışlardan çıkarılmış, tek tabloda özetlenmiş bildirim listesi. Bildirim sistemi tasarımını bu tablo besler.

| # | Tetikleyici (durum geçişi / olay) | Alıcı | İçerik özeti | Kanal |
|---|---|---|---|---|

## 8. Yönetim (admin) akışları

> **Ne yazılır:** Kullanıcı akışlarıyla **aynı derinlikte**. Yönetim akışları genellikle beklenenden karmaşıktır.

## 9. Destekleyici akışlar

> **Ne yazılır:** Kayıt, profil yönetimi, hesap silme, tercih yönetimi.

## 10. Operasyonel akışlar

> **Ne yazılır:** Platform bakımı, dış servis kesintisi, bakım modu. Kesinti sırasında sürelerin dondurulması, kullanıcı bilgilendirmesi ve normale dönüş adımları.

## 11. `02`'ye geri besleme

> **Ne yazılır:** Akışlar yazılırken fark edilen gereksinim boşlukları ve `02`'de yapılan düzeltmeler.

| # | Bulgu | `02`'de yapılan değişiklik |
|---|---|---|

---

*Shopfolio — User Flows v0.1*
