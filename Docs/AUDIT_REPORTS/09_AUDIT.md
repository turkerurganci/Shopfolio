# Audit Raporu — 09 Coding Guidelines

**Tarih:** 2026-10-30 · **Hedef:** `Docs/09_CODING_GUIDELINES.md` v0.16 (bulgular v0.16'da üretildi, v0.17'de işlendi; taban `main` = 5eacc19) · **Bağlam:** `PRODUCT_DISCOVERY_STATUS.md` §20.1 ve K-1302…K-1359; üst dokümanlar `02`, `04`, `05`, `06`, `07`, `08` · **Odak:** Tam denetim, betikle çıkarılmış dar kapsamlı paketlerle (`09` 421 KB; bütün okunmaz)

### Koşum biçimi (K-431, K-1232; `audit` skill'i, "Koşum biçimi")

Dokümanı yazan bağlamda koşmadı; iki parçaya bölündü. **A-a:** paketleyici betik (`scripts/doc-scans/09_audit_paketle.py`) on altı mercek paketi üretti, mercekler paketleri okudu, doğrulayıcılar bulguları hükme bağladı; sonuç dosyasında her bulgunun ESKİ → YENİ şartnamesi tutuldu, dokümana yazılmadı. **A-b:** şartnameler tek betikle uygulandı, raporlandı, karar kaydına işlendi.

- **Paketleyici betik.** Karar kaydı K-1302…K-1359, `09`'un bölümleri, terim sözlüğü, atıflar ve güvence cümleleri için on altı paket (her biri en çok 85 KB) üretti. Paket boyutları mercek tablosundadır.
- **On altı mercek.** K1…K10 (kapsama: başlık notu ve bölümler ↔ karar kaydı), TE (terim ve ad), AT (atıf: betik), G1…G3 (güvence cümleleri), DO (dış olgular). K6 paketi, 08 başlık düzeltmesinden sonra yeniden koştu. Mercekler `[ozet]` Sonnet / low (K-1232).
- **Kademeli doğrulama — 16 doğrulayıcı, hepsi `[dogrulama]` Opus / high.** Yüksek bulgu çıkmadı; orta bulgular 2–4'lü (9 doğrulayıcı: O1…O9), düşük bulgular 5–8'li (4 doğrulayıcı: D1…D4). İlk turda beş bulgu paketin eksik kanıtı yüzünden 'doğrulanamadı' çıktı; üç yeniden doğrulayıcı (R1…R3) tam bölümlerle çağrıldı ve beşi de 'doğru' buldu. Yeniden doğrulama gerekçesi paket eksikliğidir, bulgu eksikliği değil.
- **Uygulama.** Şartnameler tek betikle uygulandı (bir yönetici ajan; alt ajan açılmadı). Betik her ESKİ metnin dokümanda tam bir kez geçtiğini doğruladı (48 bulgu, 69 ESKİ → YENİ çifti); bir şartnamenin kendi tabloyu bozacak biçimi elle düzeltildi (aşağıda, Sınırlar).
- **Ölçüm.** Belirteç tüketimi bu raporda ölçülmedi (A-a çıktısı sayı vermedi); sayılar mercek ve doğrulayıcı sayılarıdır.

**Yürütücünün hükümleri (kısa):** çürütüldü, uygulanmaz — **B-K1359-4** (misafir sepeti çerezinin 400 günü Teknik Mimari'nin değeridir ve kaynağıyla anılır; 10 dakika kılavuzun kendi kararıdır — iki değerin evi farklı olması §3.5 madde 4'ün ve konvansiyon 7'nin sonucudur). **Kısmen doğrulanan** (öneri doğrulayıcı hükmüyle daraltıldı): B-K1-1, B-K7-2, B-G2-4, B-K8-3, B-K4-1, B-K1359-9, B-K1359-10.

**Sonuç:** 40 ham bulgu (B-K3-1 ve B-G1-1 aynı olgu: kayıtta tek sayılır → 39 benzersiz) + K-1359'un hükme kalan 10 konusu → 49 hüküm birimi → **41 doğru · 7 kısmen · 1 çürük · 0 doğrulanamadı** (yeniden doğrulama sonrası). **48 bulgu uygulandı**, 1 çürütüldü. Seviye değişimi: B-G2-2 orta → düşük, B-K1359-10 orta → düşük. **Çok kritik soru çıkmadı.** On iki karar satırı: **K-1360…K-1371**; öneriyle kaydedilen 7 ⚠ (K-1361, K-1363, K-1364, K-1365, K-1367, K-1368, K-1370) ve 15 sıradan öneri maddesi.

> **Sayımın denetimi.** Mercek tablosundaki ham bulgu toplamı 1 + 4 + 3 + 1 + 1 + 3 + 3 + 3 + 5 + 5 + 0 + 0 + 3 + 6 + 2 + 0 = **40**; doğrulayıcı tablosundaki bulgu sayıları (D1 5 · D2 6 · D3 6 · D4 4 · O1 3 · O2 3 · O3 4 · O4 3 · O5 3 · O6 4 · O7 3 · O8 2 · O9 3) = 49 hüküm birimi; yeniden doğrulama (R1…R3) beş bulguyu ikinci kez hükme bağladı ve ilk turdaki 'doğrulanamadı' hükümlerinin yerini aldı. Hüküm toplamı 41 + 7 + 1 = 49. Uygulanan 48 + çürütülen 1 = 49.

### Envanter Özeti

| Mercek | Paket (KB) | Ham bulgu | Not |
|---|---|---|---|
| K1 | 84.0 | 1 |  |
| K2 | 84.9 | 4 |  |
| K3 | 85.4 | 3 |  |
| K4 | 84.5 | 1 |  |
| K5 | 84.4 | 1 |  |
| K6 | 84.1 | 3 | ilk koşum eski pakette (08 bölümleri eksik); yeni pakette yeniden koştu |
| K7 | 84.9 | 3 |  |
| K8 | 84.4 | 3 |  |
| K9 | 83.8 | 5 |  |
| K10 | 61.6 | 5 |  |
| TE | 84.4 | 0 | bulgu yok; 574 'hiçbirinde yok' adın tamamı 09'un kendi kod yapısı |
| AT | 12.1 | 0 | betik 23 ölü/şüpheli atıf çıkardı; hepsi elle çözüldü, yanlış pozitif (iki yönlü sınama: bilerek bozulmuş 5 atıf yakalandı) |
| G1 | 79.4 | 3 | G1/G2/G3 toplam 579 güvence cümlesi |
| G2 | 79.1 | 6 |  |
| G3 | 64.5 | 2 |  |
| DO | 6.8 | 0 | 15 olgu: 12 doğru, 0 yanlış, 3 doğrulanamadı (xunit.v3 3.2.x çekirdek sayfası, Serilog.Sinks.File 7.0.0, AWSSDK.SimpleEmailV2 4.0.102.1 ikincil kaynak) |

Mercek notları: **AT** betiği 23 ölü ya da şüpheli atıf çıkardı; hepsi elle çözüldü, hepsi yanlış pozitif (08'deki gerçek başlıklar; paketleyicinin ilk sürümünde 08'de eşleşmeyen bir kod çiti yüzünden eksikti, düzeltilip yeniden koşuldu). İki yönlü sınamada bilerek bozulmuş beş atıf yakalandı. **TE** bulgu vermedi; 574 'hiçbirinde yok' adın tamamı `09`'un kendi kod yapısıdır. **G1/G2/G3** toplam 579 güvence cümlesi okudu. **DO** 15 olgudan 12'sini doğruladı, 0 yanlış, 3 doğrulanamadı.

### Doğrulayıcılar

| Doğrulayıcı | Kademe | Kapsam (bulgu) | Hüküm |
|---|---|---|---|
| D1 | düşük (5–8'li; D4 4'lü, tek bölüm §23) | B-K1-1, B-K2-4, B-K1359-1, B-K1359-4, B-K1359-5 | doğru 3 · kısmen 1 · çürük 1 · doğrulanamadı 0 |
| D2 | düşük (5–8'li; D4 4'lü, tek bölüm §23) | B-K3-3, B-G1-2, B-K6-2, B-G2-6, B-K7-2, B-K7-3 | doğru 5 · kısmen 1 · çürük 0 · doğrulanamadı 0 |
| D3 | düşük (5–8'li; D4 4'lü, tek bölüm §23) | B-G2-4, B-G2-5, B-K8-3, B-K9-3, B-K9-4, B-K9-5 | doğru 4 · kısmen 2 · çürük 0 · doğrulanamadı 0 |
| D4 | düşük (5–8'li; D4 4'lü, tek bölüm §23) | B-K10-5, B-K1359-6, B-K1359-7, B-K1359-8 | doğru 1 · kısmen 0 · çürük 0 · doğrulanamadı 3 |
| O1 | orta (2–4'lü) | B-K2-1, B-K2-2, B-K2-3 | doğru 3 · kısmen 0 · çürük 0 · doğrulanamadı 0 |
| O2 | orta (2–4'lü) | B-K3-1, B-K3-2, B-G1-3 | doğru 2 · kısmen 0 · çürük 0 · doğrulanamadı 1 |
| O3 | orta (2–4'lü) | B-K4-1, B-G2-1, B-K5-1, B-G2-2 | doğru 2 · kısmen 1 · çürük 0 · doğrulanamadı 1 |
| O4 | orta (2–4'lü) | B-K6-1, B-K6-3, B-G2-3 | doğru 3 · kısmen 0 · çürük 0 · doğrulanamadı 0 |
| O5 | orta (2–4'lü) | B-K7-1, B-K8-1, B-K8-2 | doğru 3 · kısmen 0 · çürük 0 · doğrulanamadı 0 |
| O6 | orta (2–4'lü) | B-K9-1, B-G3-2, B-K9-2, B-G3-1 | doğru 4 · kısmen 0 · çürük 0 · doğrulanamadı 0 |
| O7 | orta (2–4'lü) | B-K10-1, B-K10-2, B-K1359-9 | doğru 2 · kısmen 1 · çürük 0 · doğrulanamadı 0 |
| O8 | orta (2–4'lü) | B-K10-3, B-K10-4 | doğru 2 · kısmen 0 · çürük 0 · doğrulanamadı 0 |
| O9 | orta (2–4'lü) | B-K1359-2, B-K1359-3, B-K1359-10 | doğru 2 · kısmen 1 · çürük 0 · doğrulanamadı 0 |
| R1 | yeniden (eksik kanıt; tam bölümlerle) | B-K1359-6, B-K1359-7, B-K1359-8 | doğru 3 · kısmen 0 · çürük 0 · doğrulanamadı 0 |
| R2 | yeniden (eksik kanıt; tam bölümlerle) | B-K3-2 | doğru 1 · kısmen 0 · çürük 0 · doğrulanamadı 0 |
| R3 | yeniden (eksik kanıt; tam bölümlerle) | B-K5-1 | doğru 1 · kısmen 0 · çürük 0 · doğrulanamadı 0 |

### Bulgu → hüküm → uygulama

Seviye, doğrulayıcının hükmüdür. "Uygulama" sütunu, bulgunun işlendiği karar satırını (⚠ ya da sıradan öneri) ya da salt metin düzeltmesini ve yerini gösterir.

| Bulgu | Seviye | Hüküm | Uygulama |
|---|---|---|---|
| B-K1-1 | düşük | kısmen | K-1360 (öneri); 1 şartname çifti; §Başlık notu, park maddelerinin durumu |
| B-K2-4 | düşük | doğru | metin düzeltmesi; 1 şartname çifti; §3 Kaynak satırı |
| B-K1359-1 | düşük | doğru | K-1360 (öneri); 1 şartname çifti; başlık notu, Alt bölüm haritası |
| B-K1359-5 | düşük | doğru | K-1360 (öneri); 1 şartname çifti; §3.5 madde 4) ve gövde |
| B-K3-3 | düşük | doğru | metin düzeltmesi; 1 şartname çifti; §9.2 madde 6 |
| B-G1-2 | düşük | doğru | metin düzeltmesi; 1 şartname çifti; §8.2 |
| B-K6-2 | düşük | doğru | metin düzeltmesi; 1 şartname çifti; §12 madde 2 |
| B-G2-6 | düşük | doğru | metin düzeltmesi; 1 şartname çifti; §13.1 |
| B-K7-2 | düşük | kısmen | metin düzeltmesi; 3 şartname çifti; §14.2 giriş |
| B-K7-3 | düşük | doğru | K-1368 (⚠); 1 şartname çifti; §15.1 madde 6) ve satır 1569 (madde 8 |
| B-G2-4 | düşük | kısmen | metin düzeltmesi; 1 şartname çifti; §10.8 |
| B-G2-5 | düşük | doğru | metin düzeltmesi; 1 şartname çifti; §15.2 |
| B-K8-3 | düşük | kısmen | metin düzeltmesi; 1 şartname çifti; §15.6 madde 6 |
| B-K9-3 | düşük | doğru | K-1369 (öneri); 1 şartname çifti; §18.2 madde 1 |
| B-K9-4 | düşük | doğru | metin düzeltmesi; 1 şartname çifti; §21.2 |
| B-K9-5 | düşük | doğru | metin düzeltmesi; 2 şartname çifti; §20 Kaynak) ve 1857 (§17 Yasak |
| B-K10-5 | düşük | doğru | K-1371 (öneri); 1 şartname çifti; §23 Test ve süreç |
| B-K1359-6 | düşük | doğru | metin düzeltmesi; 1 şartname çifti; §23 |
| B-K1359-7 | düşük | doğru | K-1370 (⚠); 1 şartname çifti; §23 |
| B-K1359-8 | düşük | doğru | metin düzeltmesi; 1 şartname çifti; §23 |
| B-K2-1 | orta | doğru | metin düzeltmesi; 1 şartname çifti; §5.2 madde 3 |
| B-K2-2 | orta | doğru | metin düzeltmesi; 1 şartname çifti; §4.2 madde 1 |
| B-K2-3 | orta | doğru | K-1361 (⚠); 2 şartname çifti; §3.5 madde 2 |
| B-K3-1 | orta | doğru | metin düzeltmesi; 1 şartname çifti; §8.1 madde 9 |
| B-K3-2 | orta | doğru | K-1364 (⚠); 2 şartname çifti; §9.1 madde 4 |
| B-G1-3 | orta | doğru | K-1363 (⚠); 1 şartname çifti; §4.3 |
| B-K4-1 | orta | kısmen | metin düzeltmesi; 1 şartname çifti; §10.2 madde 4 |
| B-G2-1 | orta | doğru | metin düzeltmesi; 1 şartname çifti; §10.9 |
| B-K5-1 | orta | doğru | metin düzeltmesi; 2 şartname çifti; §10.8 madde 1, Kurucu satırı |
| B-G2-2 | düşük (mercek: orta) | doğru | metin düzeltmesi; 1 şartname çifti; §10.8 |
| B-K6-1 | orta | doğru | K-1365 (⚠); 1 şartname çifti; §11.3 madde 4 |
| B-K6-3 | orta | doğru | K-1366 (öneri); 4 şartname çifti; §11.1 madde 1 |
| B-G2-3 | orta | doğru | metin düzeltmesi; 2 şartname çifti; §11.1 |
| B-K7-1 | orta | doğru | K-1367 (⚠); 3 şartname çifti; §14.1 madde 1 |
| B-K8-1 | orta | doğru | metin düzeltmesi; 1 şartname çifti; 09 satır1713 (§15.7 madde 6 |
| B-K8-2 | orta | doğru | K-1369 (öneri); 1 şartname çifti; §16.2 madde 6.2 |
| B-K9-1 | orta | doğru | K-1369 (öneri); 3 şartname çifti; §18.2 madde 4 |
| B-G3-2 | orta | doğru | metin düzeltmesi; 1 şartname çifti; §18.1 |
| B-K9-2 | orta | doğru | K-1369 (öneri); 1 şartname çifti; §21.4 madde 3 |
| B-G3-1 | orta | doğru | K-1369 (öneri); 1 şartname çifti; §20 |
| B-K10-1 | orta | doğru | metin düzeltmesi; 1 şartname çifti; §25 Veri ve göç |
| B-K10-2 | orta | doğru | metin düzeltmesi; 1 şartname çifti; §25 Veri ve göç, "Değer ekleyen PR" yük biçimi sorusu |
| B-K1359-9 | orta | kısmen | K-1371 (öneri); 2 şartname çifti; §25 Veri ve göç) ve §12 (satır 1355-1397 |
| B-K10-3 | orta | doğru | metin düzeltmesi; 2 şartname çifti; §23) ve 2307 (§25 |
| B-K10-4 | orta | doğru | K-1371 (öneri); 1 şartname çifti; §23 giriş |
| B-K1359-2 | orta | doğru | K-1362 (öneri); 5 şartname çifti; §3.5 değer tablosu) ve satır 1108 (§10.7 madde 7 |
| B-K1359-3 | orta | doğru | K-1362 (öneri); 2 şartname çifti; Kütüphaneler tablosu |
| B-K1359-10 | düşük (mercek: orta) | kısmen | K-1362 (öneri); 1 şartname çifti; Docs/DEPLOY_RUNBOOK.md satır 67 (K-1312 park maddesi) ↔ 09 Kütüphaneler tablosu (satır 336-361 |
| B-K1359-4 | düşük | çürük | uygulanmaz (§5; 400 gün Teknik Mimari'nin değeri, 10 dakika kılavuzun kararı) |

### ⚠ ayrımı (öneriyle kaydedilen kararlar)

`INSTRUCTIONS §2` ⚠ grubuna göre (para, yasal hak, güvenlik güvencesi, geri dönüşü zor varlık kararı) yirmi iki maddelik 'öneriyle-uygulanır' listesi ikiye ayrıldı. **(a) ⚠ — 7 madde** (karar satırı konusu `(öneriyle kaydedildi — ⚠)`): B-K2-3 (K-1361: beşinci zorunlu ayar, güvenlik) · B-G1-3 (K-1363: geliştirmede e-postanın dışarı çıkmaması, güvenlik güvencesi) · B-K3-2 (K-1364: kalem ve kalem kaydının durum alanı tek yazıcıda, para tutarlılığı) · B-K6-1 (K-1365: Google `nonce` karşılaştırması, güvenlik) · B-K7-1 (K-1367: KDV işlevi ve testi, para) · B-K7-3 (K-1368: çerez katmanının sırası, güvenlik) · B-K1359-7 (K-1370: sır yasağının kapsamı, güvenlik). **(b) sıradan öneri — 15 madde** (`(öneriyle kaydedildi)`): B-K1-1, B-K1359-1, B-K1359-5 (K-1360) · B-K1359-2, B-K1359-3, B-K1359-10 (K-1362) · B-K6-3 (K-1366) · B-K8-2, B-K9-1, B-K9-3, B-G3-1, B-K9-2 (K-1369) · B-K10-4, B-K10-5, B-K1359-9 (K-1371). Seçenekleri gerçekten ayrışan ciddi para ya da hukuk riski çıkmadı; hiçbir karar mevcut bir kararın tersine gitmedi.

### Notlar

- **Eşzamanlı güncelleme (checklist §4).** Değişen maddeleri anan yerler `grep` ile arandı ve aynı PR'da hizalandı: §3.5 Kaynak satırı (K-1336, K-1350, K-1352 ve öteki eksik karar ile üst doküman atıfları; `07 §2.9.6` çıkarıldı) · gövdedeki değer yinelemeleri (§10.10, §11.2, §11.3, §13.2, §15.1, §15.4, §15.6, §15.7 — tabloya atıf) · §10.7 kod örneği ve madde 3 (bakım profilinin istemci süresi) · §14.1 kalıp kod bloğu (`VatOf` imzası) · Kütüphaneler tablosu (üç yeni satır) · `09 §2` geri besleme tablosu (`05 §8.4.6`) · `05` v0.39.
- **Üst dokümana geri besleme.** `05 §8.4.6` zorunlu ayar listesine beşinci ayarı aldı (`05` v0.38 → v0.39; K-1361). `07` zaten `UploadStatusResponse`'ta `id` taşıyor; `08 §2.3` zaten `nonce` satırını taşıyor (B-K8-2 ve B-K6-1'in 'üst doküman' yan yüzleri bu yüzden değişiklik gerektirmedi).
- **Bilinçli bırakılanlar.** Test cümlelerindeki sınır örnekleri (ör. dakikada 300 sayfa isteği, 60 saniye sapma) sayıyı taşır; yinelenen değer sayılmadı. `DEPLOY_RUNBOOK`'un K-1312 park bloğu talimattır ve yazımda değişmez; bloktaki 'koşullu lisanslı kütüphaneler' atfı numarasız kaldı, listenin kaynağı artık `09 §3.5` Kütüphaneler tablosunun giriş cümlesinde yazılı. `GUARDRAILS §8` ile §21.3'ün aynı yasakları farklı sözle anlatıp anlatmadığı bu pakette doğrulanamadı (sonraki turda). B-K9-4 (§21.2 noktalı virgül) için `INSTRUCTIONS §3.2`'de karşılık cümle bulunmadı; iki yerde de nokta oldu.

### Sınırlar

- **TE** merceğinin 'hiçbirinde geçmeyen' ad listesinin kuyruğu paket sınırı (85 KB) nedeniyle kesildi ve okunmadı; kalan risk deep review'a bırakıldı.
- **DO** merceğinde üç dış olgu doğrulanamadı: `xunit.v3` 3.2.x çekirdek sayfası, `Serilog.Sinks.File` 7.0.0, `AWSSDK.SimpleEmailV2` 4.0.102.1 (ikincil kaynak). İlgili tablo satırları zaten 'ekleme task'ında doğrulanır' ya da ikincil kaynak diye işaretli.
- **AT** merceğinin 23 'paketle doğrulanamaz' kaydı yöneticide elle çözüldü (bkz. mercek notları); gerçek ölü atıf yok.
- Bazı bulguların 'etki' maddeleri (ör. `07 §4.31.2`, `07 §6.1.3`, `05 §8.1.2`, `05 §8.3.9`, `05 §6.7.10`) paketlerde yoktu; bu yüzden doğrulanamadı ve ilgili karar satırlarının etki hücrelerine 'varsa hizalanır' diye yazıldı. Deep review bu atıfları okur.
- Kütüphaneler tablosuna eklenen üç satırın lisans ve sürümü 'bilgim tarihli; ekleme task'ında okunur' kalıbıyla yazıldı, doğrulanmadı. `dotnet-ef` ve `Microsoft.EntityFrameworkCore.Design` göç demeti araçlarının tablo eşitliği denetimine girip girmediği okunmadı.
- Bir şartname (B-K1359-3) Kütüphaneler tablosunda bir satırın yalnızca ilk iki sütununu ESKİ olarak veriyordu; düz uygulansa satır bozulurdu. Yeni satırlar mevcut satırın önüne eklenecek biçimde elle uyarlandı (altı sütun, sütun sayısı sınandı); Şartnamenin başlığı Npgsql ve EF Core sağlayıcı paketlerinin satırlarını da istediği için `Npgsql.EntityFrameworkCore.PostgreSQL` ve `Npgsql` satırları EF Core satırıyla birlikte eklendi.
