# Audit Raporu — 08 Integration Spec

**Tarih:** 2026-10-29 · **Hedef:** `Docs/08_INTEGRATION_SPEC.md` v0.7 (bulgular v0.7'de üretildi, v0.8'de işlendi; taban `main` = e74950e) · **Bağlam:** `PRODUCT_DISCOVERY_STATUS.md` §2 (Aşama 7 kararları K-1233…K-1258), §18.2 ve §18.3; `02`, `03`, `04`, `05`, `06`, `07` · **Odak:** Tam denetim

### Koşum biçimi (K-431, K-1232; `audit` skill'i, "Koşum biçimi")

Dokümanı yazan bağlamda koşmadı. Adım ajanı mercekleri, doğrulayıcıları ve uygulayıcıları **ön planda** başlattı (`run_in_background: false`). Model dağılımı K-1232'dir:

- **Dokuz mercek iki dalgada, hepsi `[ozet]` Sonnet / low.** Dalga 1 — K1, K2, K3, IT, CR · dalga 2 — YU, TE, G, DV. Kapsama dilimleri: **K1** başlık notu, §0, §1, §2.1 ↔ K-1233…K-1258 ve karar kaydı §18.3 · **K2** §2.2 ↔ `02 §9`, `04 §2.1.3`, §3.5, §8.2 ve kararlar · **K3** §2.3, §2.4, §3, §4, §5 · **IT** iç tutarlılık (özet taramasının kapsamadığı yerler) · **CR** çapraz referans (`02`…`07`) · **YU** yasal uyum · **TE** terim ve enum · **G** güvence cümleleri · **DV** dış olgular. **AT** (atıflar) merceği ajansız, betikle koştu.
- **Doğrulama.** Bulgu başına bir `[dogrulama]` Opus / high, ön planda: 50 doğrulayıcı (19 + 19 + 12; üç dalga, hiçbiri yirmiyi aşmadı). Aynı olguyu üç ayrı mercek bulduğunda tek doğrulayıcıya verildi: **GA** (K1-1 + IT-2 + CR-3), **GB** (K1-2 + IT-1), **GC** (K3-1 + IT-4 + CR-1); yan yüzler hüküm dosyasında bulguyla birlikte döndü ve uygulayıcıya şartnameyle verildi. İki olumlu doğrulama notu (DV-7, DV-8) düzeltme gerektirmediği için doğrulayıcıya gitmedi. Yönetici bulgusu **Y-1** ayrı bir doğrulayıcıdadır.
- **Uygulama.** Dört `[ozet]` Sonnet / low uygulayıcı sırayla aynı çalışma ağacında: **A1** GA, GB, GC, Y1, K1-3…K1-10, IT-5, IT-6 · **A2** CR-2, CR-4, TE-3, TE-4, TE-5, DV-2…DV-5, DV-9, G-1, IT-3 · **A3** K2-1…K2-8, TE-1, TE-2, CR-5, CR-6 · **A4** G-2…G-6, K3-2, K3-3, DV-1, YU-1, YU-2. Günlükler `apply\A<n>.md`. Toplam 192 çift uygulandı (45 + 40 + 56 + 51), 6 çift uyarlandı (eski metin önceki uygulayıcıda değişmişti), uygulanamayan yok. Karar kaydı, sürümler, durum tablosu ve K satırları yöneticinin betiğiyle yazıldı.
- **Mekanik denetimler (betikle; mercekler başlamadan sınandı).** `atif.py`: karar kaydındaki K varlığı, doküman önekli `§` atıfları (`02`…`12`; hedef başlığın ya da kalın paragraf numarasının varlığı) ve `UC-` kimlikleri; bozulmuş bir kopyada (K-9999, `05 §3.99.9`, `07 §4.999`, UC-777) dört enjeksiyonun dördünü yakaladı, gerçek 08'de sıfır bulgu verdi. `own.py`: 1.045 `§` anmasının 1.036'sı hedefini buldu; kalan dokuz, karar kaydına (`§8.3`…`§16.2`), RFC'ye ve devam eden doküman önekli listelere (`07 §4.29.1`, `04 §5.25.7` vb.) giden anmaydı — hepsi elle okunup elendi. Uygulamadan sonra iki betik yeniden koştu: `atif.py` 1.120 satırda sıfır eksik K, `§` ya da UC hedefi verdi (yeni K-1259…K-1268 dahil); `own.py` aynı on bilinen yanlış pozitifi (karar kaydı, RFC ve devam eden doküman önekli listeler) yeniden verdi.
- **Ölçüm.** Mercekler 9 · Sonnet / low · ~1,65 M token; doğrulayıcılar 50 · Opus / high · ~4,17 M; uygulayıcılar 4 · Sonnet / low · ~0,43 M. Toplam 63 ajan.

**Yürütücünün hükümleri (kısa):** çürütüldü, uygulanmaz — **YU-3** (altbilgideki "pazarlama ifadesi yoktur" cümlesinin dayanağı `02 §9.1.3`'te zaten var; 6563 m.2/1-c ve m.6/1 okundu), **DV-6** (kota, şikâyet nedeni ve harf ayrımı §2.2'de yazılı). **Kısmen doğrulanan** (öneri doğrulayıcı hükmüyle daraltıldı): CR-2, CR-4, CR-6, DV-2, DV-4, DV-9, G-2…G-6, K2-1, K2-3, K2-4, K2-6, YU-1, YU-2. **Seviye değişimleri** (Opus): G-2, G-3, DV-1, TE-4 Medium → Low; CR-4 ve K2-1 High korundu. **Tek düzeltmede birleşenler:** K1-1 + IT-2 + CR-3, K1-2 + IT-1, K3-1 + IT-4 + CR-1. **Y-1 doğrulandı (Medium):** sorun gerçektir ve K-1258 (3) onu bilinçli kabul etmişti, ama gerekçesi ("betiksiz yönlendirilemez") eksikti — betik izinli, `MK-05` yasaklamıyor, `target="_top"` betik gerektirmiyor. Doğrulayıcı yönetici önerisinden daha iyisini buldu: UC-30 hiç yönlendirmez, çerçevesiz bir geçiş sayfası verir; `frame-ancestors` sipariş sayfasından geçiş sayfasına taşınır (K-1259). `04`, `05 §6.5.4` çerçeve cümlesi ve `07` UC-30 hizası etki yansıtmaya not olarak `08 §5.2`'dedir (görev gereği `08`'deki metin bu PR'dadır).

**Sonuç:** 56 ham bulgu → **52 doğrulandı** (17 kısmen) · **2 çürütüldü** (YU-3, DV-6) · **2 olumlu doğrulama notu** (DV-7, DV-8) → doğrulanmış seviye, hüküm birimi başına **High 2 · Medium 10 · Low 36** (48 birim: üç birim birden fazla bulguyu taşır) → **52 bulgu uygulandı**, uygulanmayan yok, Y-1 uygulandı. **Çok kritik soru çıkmadı.** **On karar** (K-1259…K-1268, hepsi ⚠; en ağırı K-1261 — doğrulayıcının ÇOK KRİTİK diye işaretlediği telefon kararı, iki yolu da geri alınabilir ve para taşımaz). `08` v0.8; Ürün Gereksinimleri v0.96, Kullanıcı Akışları v0.41, Arayüz Tanımları v0.44, Teknik Mimari v0.32, Veri Modeli v0.34, API Tasarımı v0.28'e geri beslendi; `DEPLOY_RUNBOOK` kurulum maddeleri güncellendi (sürümsüz).

> **Sayımın denetimi.** Bulgu sayıları betikle toplandı (mercek dosyaları ↔ hüküm dosyaları): K1 10 · K2 8 · K3 3 · IT 6 · CR 6 · YU 3 · TE 5 · G 6 · DV 9 = **56**; hüküm dosyaları 50 (49 grup + Y1), grup başına bulgu 1 (GA 3, GB 2, GC 3) → 46 + 8 = 54 doğrulayıcıya giden bulgu + 2 olumlu not = 56. Çürütülen iki hüküm dışında 52 bulgu uygulandı. Mercek envanterlerinin toplamları aşağıdadır; her biri merceğin kendi dosyasındaki ✓ / ⚠ / ✗ eşleştirmesidir.

### Envanter Özeti

| Mercek | Envanter | Eşleştirme | Ham bulgu | Doğrulanan | Çürütülen / not |
|---|---|---|---|---|---|
| K1 kapsama — başlık notu, §0, §1, §2.1 | 54 | 37 ✓ · 11 ⚠ · 0 ✗ · 6 kapsam dışı | 10 | 10 | 0 |
| K2 kapsama — §2.2 (28 e-posta) | 44 | 22 ✓ · 16 ⚠ · 0 ✗ · 6 kapsam dışı | 8 | 8 | 0 |
| K3 kapsama — §2.3, §2.4, §3, §4, §5 | 36 | 33 ✓ · 3 ⚠ · 0 ✗ | 3 | 3 | 0 |
| IT iç tutarlılık | 38 | 32 ✓ · 6 ⚠ · 0 ✗ | 6 | 6 | 0 |
| CR çapraz referans | 118 | 102 ✓ · 6 ⚠ · 0 ✗ · 10 kapsam dışı | 6 | 6 | 0 |
| YU yasal uyum | 10 | 6 ✓ · 3 ⚠ · 0 ✗ · 1 kapsam dışı | 3 | 2 | 1 (YU-3) |
| TE terim ve enum | 130 | 118 ✓ · 5 ⚠ · 0 ✗ · 7 kapsam dışı | 5 | 5 | 0 |
| G güvence cümleleri | 15 | 7 ✓ · 8 ⚠ · 0 ✗ | 6 | 6 | 0 |
| DV dış olgular | 37 | 21 ✓ · 6 ⚠ · 1 ✗ · 9 doğrulanamadı | 9 | 6 | 1 (DV-6) · 2 olumlu not |
| AT atıflar (betik) | 1.120 anma | 1.120 hedef bulundu (üç hedef ayrı doküman satırı) | 0 | — | — |
| **Genel** | | | **56** | **52** | **2 + 2 not** |

Merceklerin kendi sayım notları: TE'nin envanteri kısmen betik sayımlarından toplandı (yaklaşık); CR'ın atıf hedeflerinin anlam kontrolü örneklemle yapıldı; G'nin betiği (`g_scan.py`) 117 satır yakaladı ve grep ile sınandı; YU'da mevzuatın birincil kaynağına erişilemeyen yerler "doğrulanamadı" yazıldı (6563'ün iki maddesi; KVKK m.9'un fıkra ayrıntısı ikincil kaynaktan); DV'de sayfası okunamayan dokuz satır "doğrulanamadı" (TCMB kaydı, kargo adresleri, Bakanlık logosu, Google kota sayfası, AWS IAM eylem adları, üç SES olgusu).

Kapsamanın özü: Aşama 7'nin 26 kararı (K-1233…K-1258) ve §18.3'ün konu kimlikleri gövdede karşılık buluyor; 28 e-posta şablonunun hepsi `02 §9`, `03 §7.1` ve `06 §4.58` ile örtüşüyor (eksik ya da fazla yok); geri yansıma satırlarının (`08 §5`) hepsi üst dokümanlarda mevcut ve sürümler uyuşuyor. Bulgular yazımın kenarlarındadır: aynı olgunun iki yerde iki sonuçla yazılması (jeton hatasında sepet / teyit ekranı), kararın tablo ve `07` ile örtüşmemesi (ikinci jeton reddi), bildirim alan sayısı (on / on bir), üst dokümanlarda karşılığı olmayan iki mekanizma (geri dönen adres sorgusunun iş satırı, tanınmayan bildirim değeri) ve kesin cümlelerin kapsamı (sabit zamanlı karşılaştırma, yeniden deneme penceresi, "bir kez" kullanım).

### Bulgu → hüküm → uygulama

Seviye, doğrulayıcının hükmüdür. "Yer" `08`'in bölümüdür; ÜST etiketli yerler hükmün üst dokümanda yaptığı değişikliktir.

| Bulgu | Seviye | Hüküm | Uygulama |
|---|---|---|---|
| Y-1 | Medium | Doğrulandı; öneri iyileştirildi | K-1259: çerçevesiz geçiş sayfası; `08 §2.1.4`, §5.2; `05 §6.5.4` form-action; `04` / `07` etki yansıtmaya not |
| GA — K1-1 + IT-2 + CR-3 | Medium | Doğrulandı | K-1260 (2): jeton hatasında sepete dönüş; §2.1, §3.1, §2.1.8, §3.3 |
| GB — K1-2 + IT-1 | Medium | Doğrulandı | K-1260 (1): ikinci jeton reddi; §2.1.8 satırı; ÜST `07 §4.14.2` |
| GC — K3-1 + IT-4 + CR-1 | Low | Doğrulandı | bildirim gövdesi on alan (yedi zorunlu, üç koşullu); §2.1.5; ÜST `07 §4.30.1`, §10 |
| CR-4 | High | Kısmen | K-1261: `user_phone` ← `Order.CompanyPhone`; DV-38; ÜST yok |
| CR-2 | Low | Kısmen | tutar karşılaştırmasının taksit farkıyla bağı `08`'e yazıldı (üst dokümanlar karşılaştırmanın biçimini `08`'e bırakıyor) |
| CR-5 | Medium | Doğrulandı | K-1264 (1): `BounceQuery` iş satırı; ÜST `05 §4.3.4`, §5.1.4, §5.2.16; `06 §4.59`, §1 |
| CR-6 | Low | Kısmen | `07 §3.4.1` ve matrisin `04` 3.5.8 satırı güncel hâle; ÜST `07` |
| K2-1 | High | Kısmen | K-1262: ilk e-postanın satır kimliği; `08 §2.2.5`; ÜST `06 §4.58` |
| K2-2 | Medium | Doğrulandı | B-9 gövdesi yalnız tarih; `08 §2.2.6` |
| K2-3 | Medium | Kısmen | K-1263 (1): marka rengi yalnız düğme zemininde |
| K2-4 | Low | Kısmen | "adres silinir" cümlesi hesabın adresi diye netleşti; ÜST `02 §3.15.1` |
| K2-5 | Low | Doğrulandı | K-1263 (2): B-6 kaynak notu |
| K2-6 | Low | Kısmen | yeniden istenen doğrulama e-postasında süre; ÜST `03` 7.1.51 |
| K2-7 | Low | Doğrulandı | K-1263 (3): B-2 yeniden gönderim listesi |
| K2-8 | Low | Doğrulandı | K-1263 (6): silinen hesabın adresi; ÜST `02 §3.15.1`, `03` 7.1.54, `04` 5.25.13 |
| TE-1 | Medium | Doğrulandı | K-1263 (4): B-7 etiketleri `02 §7.2.4` ile birebir |
| TE-2 | Medium | Doğrulandı | K-1264 (2): `UnrecognizedNotification`; ÜST `06 §4.35`, §5.6.6 |
| TE-3 | Medium | Doğrulandı | başarısız ödemede sipariş Bekliyor'da kalmaz, iptal olur (`02 §6.2.1`, `05 §4.5.3`); `08 §2.1.4` |
| TE-4 | Low | Doğrulandı | "kanal hatası" terimi son sorguda "bağlantı hatası"; §2.1.6, §3.1 |
| TE-5 | Low | Doğrulandı | Z-16, Z-17; §2.1.9 |
| K3-2 | Low | Doğrulandı | K-1265: Google adı yedeği; ÜST `02 §3.13.21`, `06 §4.36` |
| K3-3 | Low | Doğrulandı | Google'a ulaşılamadı satırı (keşif belgesi, anahtar kümesi); §2.3 |
| IT-3 | Low | Doğrulandı | iade sorgusu aralığı beş basamak; §3.1 |
| IT-5 | Low | Doğrulandı | yazım sırası cümlesi; başlık notu |
| IT-6 | Low | Doğrulandı | `ENT2-13` tekrarı; başlık notu |
| K1-3, K1-4, K1-5, K1-7 | Low | Doğrulandı | tutar / başarı koşulu / gövde tavanı / zaman aşımı atfı ifadeleri; §2.1.3, §2.1.5, §2.1 |
| K1-6 | Low | Doğrulandı | `ENT2-04` ve `ENT2-08` konu kimlikleri; K-1078 (2) konusuz |
| K1-8 | Low | Doğrulandı | K-1259 (2): `form-action` PayTR adresini taşımaz; ÜST `05 §6.5.4` |
| K1-9 | Low | Doğrulandı | DV-15 bağı; §2.1.2 |
| G-1 | Low | Doğrulandı | K-1267 (1): sabit zamanlı karşılaştırma |
| G-2, G-3 | Low | Kısmen | K-1267 (2), (3): yeniden deneme penceresi; sayaç; ÜST yok |
| G-4 | Low | Kısmen | K-1267 (4): açık Google oturumu; `Doğrulanacaklar (2)`, DV-26 |
| G-5 | Low | Kısmen | K-1267 (5): bağlantı süresinin başlangıcı (ortak kural) |
| G-6 | Low | Kısmen | K-1267 (6): kalan risk; ÜST `07 §4.22.3` |
| YU-1 | Medium | Kısmen | K-1266 (1): yurt dışı aktarım; 6698 m.9/4-c ve m.9/5 (mevzuat.gov.tr, okuma 2026-10-29); §2.2.1 (f), DV-24; `DEPLOY_RUNBOOK` |
| YU-2 | Low | Kısmen | K-1266 (2): 6493 m.18/3-a, m.23/1; KVKK m.8, m.9, m.10; PayTR elektronik para kuruluşudur (TCMB kodu 863); ÜST `02 §12.2.8` |
| DV-1 | Low | Doğrulandı | K-1268 (1): Google test kullanıcısı gerekmez (support.google.com/cloud/answer/15549945, okuma 2026-10-29); §2.3, DV-37 |
| DV-2, DV-4, DV-5, DV-9 | Low | Doğrulandı / kısmen | K-1268 (3)…(5): alan adları, taksit farkı, DMARC, Non3D; §4.1 |
| DV-3 | Low | Doğrulandı | K-1268 (2): test kartı kalemi kalktı; `08`, `DEPLOY_RUNBOOK` |
| YU-3, DV-6 | — | **Çürütüldü** | uygulanmaz |
| DV-7, DV-8 | — | olumlu not | doğrulayıcıya gitmedi; düzeltme yok |

### Notlar

- **Hükme bağlanmayan gözlemler** (doğrulayıcıların ayrı bulgu adayı diye bıraktıkları; yeni K gerekmeden bir sonraki turda ele alınır): `08` satır 207'deki `failed_reason_code` değeri `3` (3D ile ilgisi yok) · `frame-src`'nin `'self'` taşımaması (dönüş geçiş sayfası çerçevede yüklendiği için K-1259 bunu zaten karşılıyor; DV-5 sınar) · `06 §4.58` grup tanımı, `05 §4.4.3` ve `07`'deki "grubun en son satırı" ifadesi (K-1262'nin kapsamı dışında kaldı) · CR-2'nin ayrı gözlemi: iç alan `Amount` ↔ `06 §4.17` `Total`.
- **Eşzamanlı alt ajan sınırı.** Birinci ve ikinci dalgada tek mesaja yirmiden bir fazla çağrı yazıldı (sayım hatası); araç fazlayı (birer çağrı) reddetti ve reddedilen doğrulayıcılar sonraki dalgada yeniden başlatıldı, iş tekrarlanmadı. Hiçbir ajan hook tarafından engellenmedi; etiket ya da model değiştirilmedi.
- **Çok kritik soru, yönetici onayı bekleyen karar yok.** On ⚠ kararın listesi aşama sonunda proje sahibine gösterilecek ⚠ listesine eklendi (`PRODUCT_DISCOVERY_STATUS.md` §18.1).
