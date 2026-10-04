# Shopfolio — Integration Specifications

**Versiyon: v0.1** | **Bağımlılıklar:** `02`, `03`, `05`, `06`, `07` | **Son güncelleme:** YYYY-AA-GG

> **Aşama:** 7 — Entegrasyon Spesifikasyonları · **Rol:** Integration Engineer
> **Traceability zorunlu:** Hayır
> **Beklenti:** Bu doküman doğası gereği `07`'ye **geriye dönük endpoint** ekletir (geri çağrım/webhook uçları). Bu bir hata değil, beklenen sonuçtur.

> **Aşama 1'den park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36.
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-12 · Para akışı ve kart verisi:** Tahsilat doğrudan **firmanın kendi sanal POS / ödeme sağlayıcı hesabına** geçer; platform ticari zincirde yer almaz. Kart bilgisi sağlayıcının barındırdığı sayfada veya çerçevede girilir — **ürün kart verisini görmez, saklamaz, loglamaz.** Entegrasyon bu sınırı bozacak biçimde tasarlanamaz. Firmanın ödeme sağlayıcı sözleşmesi canlıya çıkış öncesi **dış ön koşuldur**.
> - **K-13 · 3D Secure:** **İstisnasız zorunludur.** Tutar eşiği veya sağlayıcı takdirine bırakma yoktur; doğrulama başarısızsa ödeme gerçekleşmez ve sipariş ödenmiş sayılmaz.

> **Aşama 2'den park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §8.3 AK0-04).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-705 · Sağlayıcıyla sonucu belirsiz kalan kart işlemleri:** (1) Sağlayıcıya ulaşılamayan kart iadesi sonucu bilinmeyen bir istektir; firmanın yeniden denemesi ya da müşteriye havale yolu açması parayı iki kez gönderebilir (`02 §7.2.8`). İade isteğinin tek işlenmesi ve sonucun sağlayıcıdan sorulması — ödeme tarafındaki son sorgunun (`02 §6.1.2`) iade karşılığı — bu aşamanın kararıdır; tasarım ürüne bir "sonuç alınamadı" işareti gerektirirse karar kaydına döner. (2) Kart ödemesinin onayının tek dayanağı sağlayıcının başarı bildirimidir (`02 §5.5` Ö1): bildirimin doğrulanması ve tutarının siparişin tutarıyla eşleşmesi bu aşamada tasarlanır; `03 §2.5.1.2` müşterinin dönüş anında sonucun beklendiğini gördüğünü yazar (K-704).
> - **Dizin — Aşama 2 kararlarının bu dokümana devirleri:** karar kaydında K-647…K-718'in etki sütunlarında bu dokümanı gösteren on iki atıf (on iki karar) ve Kullanıcı Akışları'nın (`03`) gövdesinde bu dokümana iş bırakan cümleler [`CHECKPOINT_REPORTS/PHASE2_CONFLICT_SCAN.md`](CHECKPOINT_REPORTS/PHASE2_CONFLICT_SCAN.md) §6'dadır. İki iş yalnız `03`'ün gövdesinde yaşar, karar satırı yoktur: yeniden gönderilebilen e-postaların kapsamı (`03 §7.1.37`, §8.9.2 — *Aşama 3'te karara bağlandı: K-759; aşağıdaki blok*) ve ödeme sağlayıcısının anahtarlarının ya da sağlayıcının değişmesinin açık kart siparişlerine etkisi ve geçiş yöntemi (`03 §10.1.1.7`). Devir taraması (`checklists/document-stage.md` §4) buradan başlar; devrin evi karar satırıdır, dizin onu ikinci kez kaydetmez. Aşama 1'in dizini: `PHASE1_CONFLICT_SCAN.md` §6.

> **Aşama 3'ten park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2 ve §3, mekanizma: K-36 (yeri: §10.3 UI0-04).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-735 · K-599, K-601, K-616 · Üç bildirimin metni:** Aşama 1'in üç kararı bir bildirim metnini Arayüz Tanımları'na (`04`) devretmişti; e-postanın metni, şablonu ve gönderim biçimi bu dokümanın işidir (`03 §7` girişi) ve devir buraya taşındı (`04 §1.3` GAP-5). Yazılacak metinler: hesabın e-postası değiştiğinde eski adrese giden bildirim — adresin değiştiği ve tarihi, yeni adres yok, "bu değişikliği ben yapmadım" bağlantısı (`02 §3.13.14`, §9.4) · B-10, siparişin adresi düzeltildiğinde — düzeltilen adres ve değişikliği istemeyen müşterinin firmaya ulaşması (`02 §9.2`, §10.4.2) · F-5, havale IBAN'ı girildiğinde ya da değiştiğinde yöneticilere — tarih ve saat, yapan yönetici, IBAN'ın yalnız son dört hanesi; tam IBAN ve geri alma bağlantısı yok (`02 §9.3.3`). İçerik kuralı `02`'dedir; bu doküman metni ve şablonu yazar. Ekrandaki izleri `04`'te kalır: geri alma ekranı, adres düzeltme ekranındaki teyit hatırlatması.
> - **K-759 · K-760 · Yeniden gönderilen bildirimler:** panelden yeniden gönderilebilen e-postalar siparişe bağlı on altı müşteri bildirimidir (B-1…B-16) — "e-posta ulaşmadı" işaretini düşüren her bildirim (`02 §9.1.6`); firma bildirimleri (F-1…F-6) ve hesap e-postaları yeniden gönderilmez. Bu aşamada yazılacak olan: yeniden gönderilen bildirimin ilk denemeden sonra değişmiş bilgiyi — düzeltilmiş takip bilgisi, yeniden başlamış ödeme süresi — nasıl taşıdığı ve e-postanın bir yeniden gönderim olduğunu alıcıya söyleyip söylemediği. B-1 siparişe donmuş sürümüyle gider ve bu değişmez (`02 §6.1.1`). Firmaya giden bildirimin (F-1…F-4) ulaşmaması panelin ana sayfasında uyarı doğurur; ulaşmadığının nasıl anlaşıldığı bu aşamanın ve Teknik Mimari'nin (`05`) işidir. Ekrandaki izleri `04`'te kalır: sipariş ayrıntısındaki ulaşmayan e-postalar listesi ve ana sayfanın uyarıları (K-761).
> - **K-824 · Tarih, saat ve tutarın e-postadaki biçimi:** arayüzün tarih, saat, sayı ve tutar biçimi Arayüz Tanımları'nda (`04 §8.2`) sabittir — "4 Ekim 2026", "14:30", "1.200 TL" ve "266,67 TL"; tek saat dilimi Türkiye saatidir (`02 §4.1.1`). Bildirimlerin metni ve şablonu yazılırken biçim buradan okunur; e-postada başka bir biçim seçilirse ayrılığın gerekçesi yazılır.

---

## 0. Yazım kuralları

- **Her entegrasyon ayrı bölüm.** API limitleri, hata senaryoları, retry stratejisi, geri düşme planı.
- **Bağımlılık riski:** Her entegrasyon için *"bu servis çökerse ne olur?"* cevaplanır.
- **Alan eşlemesi yazarken `06` açık tutulur** ve her alan adı kontrol edilir. Bu aşama, veri modeliyle uyumsuz alan adlarının en sık yakalandığı yerdir.
- **"Ücretsiz seçenek yeter mi?" sorusu ÖNCE sorulur.** Ücretli bir sağlayıcı seçilip sonra ücretsizin yettiğini keşfetmek sık görülen bir israftır.
- **Dış çağrıyı yapıp yapmamayı belirleyen kural burada da yazılır.** Bir iş kuralı (ör. minimum eşik) dış servis çağrısının yapılıp yapılmayacağını belirliyorsa, kaynağı `02` olsa bile burada tekrar edilir.

---

## 1. Entegrasyon envanteri

| # | Servis | Ne için | Kritiklik | Ücretli mi | Kota / limit |
|---|---|---|---|---|---|

## 2. Entegrasyon tanımları

### 2.x <Servis adı>

- **Amaç:**
- **Kimlik doğrulama:** (key türü, nerede saklanır, nasıl döndürülür)
- **Kullanılan uçlar:**
- **Alan eşlemesi:** (dış alan → iç entity.alan — `06` ile kontrol edilmiş)

| Dış alan | İç alan | Dönüşüm | `06` doğrulandı |
|---|---|---|---|

- **Hız limitleri ve kota:**
- **Hata senaryoları ve kodları:**
- **Retry stratejisi:** (kaç deneme, hangi gecikme, hangi hatalarda)
- **Idempotency / tekilleştirme anahtarı:**
- **Geri çağrım / webhook:** (imza doğrulama **zorunlu**, tekrar gönderim davranışı)
- **Servis çökerse:** (geri düşme, kullanıcıya ne gösterilir, süreler donar mı)
- **Test / sandbox imkânı:**
- **Runbook:** [`INTEGRATION_RUNBOOKS/<SERVIS>.md`](INTEGRATION_RUNBOOKS/)

## 3. Ortak hata yönetimi

## 4. Dış varsayımlar

> **Ne yazılır:** Bu dokümanın dayandığı her dış varsayım ve **doğrulama kanıtı**. Task'ların ön-uçuş kontrolü (00 §F.3) bu tabloyu başlangıç noktası alır.

| # | Varsayım | Kanıt | Doğrulama tarihi |
|---|---|---|---|

## 5. `07`'ye geri yansıtılanlar

| # | Yeni ihtiyaç | `07`'de eklenen |
|---|---|---|

---

*Shopfolio — Integration Specifications v0.1*
