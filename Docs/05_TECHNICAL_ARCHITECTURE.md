# Shopfolio — Technical Architecture

**Versiyon: v0.1** | **Bağımlılıklar:** `01`, `02`, `03`, `04`, `10_MVP_SCOPE.md` | **Son güncelleme:** YYYY-AA-GG

> **Aşama:** 4 — Teknik Mimari · **Rol:** Senior Software Architect / System Designer
> **Traceability zorunlu:** Hayır
> **İlke:** Teknik kararlar ürün ihtiyaçlarından **türetilir**. "Şu teknolojiyi kullanmak istiyorum" değil, "şu gereksinimi en iyi karşılayan ne?"

> **Aşama 1'den park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36.
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-10 · Çok kiracılık:** MVP'de çok kiracılık için **hazırlık yapılmaz** — kurulum başına tek firma. Pazaryeri / çok satıcı **kalıcı ürün sınırıdır**; çok kiracılı SaaS post-MVP yol haritası adayıdır ancak mimaride bugün karşılığı yoktur.

> **Aşama 2'den park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §8.3 AK0-04).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-666 · K-667 · Sitenin kesintisinde süreler:** kesinti sırasında zamanı gelen kendiliğinden işler — kendiliğinden iptal (kart hattında iptal öncesi son sorguyla, `02 §6.1.2`), havale hatırlatması (yalnız havale süresi dönüşte hâlâ işliyorsa; ertelenen iptalde yeni hatırlatma gitmez — K-685), IBAN'ın silinmesi, periyodik imha, e-postanın yeniden denenmesi — site döndüğünde **kaçırdıkları sırayla** çalışır. Havale ödeme süresi kesinti sırasında dolarsa kendiliğinden iptal, sitenin dönüşünden sonraki ilk iş gününün sonuna ertelenir; bu, kesintinin — panelin ve kendiliğinden işlerin çalışmadığı aralığın — tespit edilmesini gerektirir. Tespitin yöntemi bu aşamanın kararıdır (`02 §4.3`, Z-8).
> - **K-687 · Giriş kaydının okunduğu yer:** giriş kaydı (`02 §3.13.20`, Z-40) panelde görünmez; ihlal ya da hesap ele geçirme incelemesinde firma onu sistem kayıtlarıyla birlikte kurulumun barındırma tarafında okur (`02 §8.5.1`, §12.2.9). Kaydın ve sistem kayıtlarının nasıl ve kim tarafından okunacağı — erişim yolu, süzme, dışarı alma — bu aşamanın kararıdır.
> - **Dizin — Aşama 2 kararlarının bu dokümana devirleri:** karar kaydında K-647…K-718'in etki sütunlarında bu dokümanı gösteren yedi atıf (yedi karar) ve Kullanıcı Akışları'nın (`03`) gövdesinde bu dokümana iş bırakan cümleler [`CHECKPOINT_REPORTS/PHASE2_CONFLICT_SCAN.md`](CHECKPOINT_REPORTS/PHASE2_CONFLICT_SCAN.md) §6'dadır; etki sütununda yalnız doküman numarası taşıyan üç atfın işinin adını dizin verir. Devir taraması (`checklists/document-stage.md` §4) buradan başlar; devrin evi karar satırıdır, dizin onu ikinci kez kaydetmez. Aşama 1'in dizini: `PHASE1_CONFLICT_SCAN.md` §6.

---

## 0. Karar prensipleri

- **Basitlikten başla.** MVP için en basit çalışan mimariyi seç; ölçekleme ihtiyacı gerçekten doğduğunda ölçekle.
- **Ama sonrasını da düşün.** Sonradan düzeltilmesi çok pahalı olan konular (olay kaybı, denetim izi eksikliği, eşzamanlılık) baştan doğru kurulur. "MVP olarak düşünme, sonrası için de düşün" bu iki maddenin dengesidir.
- **Kısıtları erken söyle.** Maliyet, ekip yetkinliği ve review edilebilirlik mimariyi doğrudan belirler. Geç açıklanan bir kısıt alınmış kararları geri aldırır.
- **Her kararın gerekçesi yazılır.** Gerekçesiz karar, altı ay sonra tartışılamaz.

---

## 1. Sistem genel görünümü

> **Ne yazılır:** Bileşen diyagramı ve her bileşenin tek cümlelik sorumluluğu.

## 2. Teknoloji kararları

| # | Alan | Seçim | Alternatifler | Gerekçe |
|---|---|---|---|---|

## 3. Servis yapısı ve sınırlar

> **Ne yazılır:** Monolit mi, modüler monolit mi, servisler mi; modül sınırları; modüller arası iletişim kuralı.

## 4. Durum yönetimi ve olaylar

> **Ne yazılır:** Durum makinesi nerede yaşar, geçişler nasıl korunur, yan etkiler nerede tetiklenir, olaylar nasıl güvenilir taşınır.

## 5. Arka plan işleri ve zamanlanmış görevler

## 6. Güvenlik mimarisi

> **Ne yazılır:** Kimlik doğrulama, yetkilendirme, secret yönetimi, servisler arası kimlik, hız sınırlama, hassas veri saklama.

### 6.x Secret yönetimi (bu alt bölüm zorunludur)

Üç soru **gün 0'da** cevaplanır — ikinci bileşen (servis, worker, yardımcı süreç) doğduğu anda karar verilmiş olur ve sonradan geri almak tüm dağıtım yüzeyini yeniden yazmak demektir:

| Soru | Ne yazılır |
|---|---|
| **Nerede durur** | Ortam değişkeni / secret store / dosya. Değerler **asla** repo'da; repo yalnız **şablonu** izler (`.env.example`) ve `.gitignore` bunu mekanik olarak zorlar. |
| **Dağıtım yarıçapı (blast radius)** | Her secret **yalnız onu kullanan bileşene** geçilir. İmza/anahtar materyali diğer bileşenlere hiç verilmez. Paylaşılan bir secret varsa **bileşen-başı varyantı** olur — böylece bir sızıntı ikinci yüzeyi açmaz. |
| **Rotation** | Her secret için: kim üretir, ne sıklıkla döner, dönerken sistem ayakta kalır mı, eski değer ne kadar geçerli kalır. |

**Ek kurallar:**
- Loglarda maskeleme: secret ve token değerleri log çıktısına **hiçbir seviyede** yazılmaz.
- Secret envanteri `DEPLOY_RUNBOOK.md §B`'de tutulur — bu bölüm **stratejiyi**, runbook **envanteri** taşır.

> Template'in geri kalanı sırrı yalnız tek eksende yasaklar ("koda gömme"). Sırrın **nereye dağıtıldığı** ayrı bir karardır ve yalnız burada sorulur.

## 7. Veri katmanı kararları

> **Ne yazılır:** Veritabanı seçimi, eşzamanlılık kontrolü, saklama/silme stratejisi, yedekleme. Şema `06`'da.

## 8. Ortamlar ve deployment

> **Ne yazılır:** Ortam yapısı, konteynerleştirme, dal stratejisi, CI/CD hattı, konfigürasyon yönetimi.
> **Not:** Dal stratejisi burada `.claude/INSTRUCTIONS.md §3.2` ile **birebir aynı** yazılmalıdır; iki farklı yerde iki farklı strateji en sık görülen çelişkidir.

## 9. Gözlemlenebilirlik

> **Ne yazılır:** Loglama, metrik, izleme, uyarı. Maliyet kısıtı varsa hangi araçların seçildiği ve neden.

## 10. Hata toleransı

> **Ne yazılır:** Her dış bağımlılık için "çökerse ne olur", retry stratejisi, devre kesici, geri düşme davranışı.

## 11. Ölçeklenebilirlik notları

> **Ne yazılır:** MVP'de bilinçli olarak tekil bırakılan bileşenler ve büyüme yolu. "Büyüdüğünde X'e geçilir" ifadeleri gerçekçi olmalı.

## 12. Kararların özeti

| # | Karar | Tarih | Gerekçe | Geri alınabilir mi |
|---|---|---|---|---|

---

*Shopfolio — Technical Architecture v0.1*
