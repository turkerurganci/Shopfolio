# Aşama 7 — Checkpoint (CP07)

**Tarih:** 2026-10-29 | **Aşama:** 7 — Entegrasyon spesifikasyonları | **Kapanış adımı:** K-437'nin 4. adımı (`/checkpoint`; `checklists/document-stage.md` §7)
**Girdi sürümleri:** karar kaydı v0.190 · Entegrasyon Spesifikasyonları (`08`) v0.16 · Ürün Gereksinimleri (`02`) v0.100 · Kullanıcı Akışları (`03`) v0.43 · Arayüz Tanımları (`04`) v0.46 · Teknik Mimari (`05`) v0.35 · Veri Modeli (`06`) v0.37 · API Tasarımı (`07`) v0.31 · MVP Kapsamı (`10`) v0.57 · Proje Vizyonu (`01`) v0.33 · `09`, `12` ve `DEPLOY_RUNBOOK`'un park blokları — `origin/main` @ `e3994e6`
**Çıktı sürümleri:** karar kaydı v0.191 · `08` v0.17 · `02` v0.101 · `04` v0.47 · `05` v0.36 · `06` v0.38 · `07` v0.32 · `10` v0.58 · `01` ve `03` değişmedi · `09`, `12` ve `DEPLOY_RUNBOOK`'un park metinleri (sürüm alanı değişmez)
**Girdi raporu:** [Aşama 7 — Çakışma Taraması](PHASE7_CONFLICT_SCAN.md) (K-437'nin 3. adımı) — açık kalem durumu, ⚠ listesinin hazır metni (§4.1), devir dizini (§6) ve checkpoint'e notlar (§8) oradan alındı; çakışma taramasının, audit'in, deep review'un ve beş cross-review turunun bulguları ikinci kez açılmadı. **Devredilen sınıf** (K-1129; `checkpoint` skill'inin "kalite döngüsünün devri" maddesi) — cross-review çıkış kuralıyla kapandı (4 tur, K-1289); iki yerin metin farkı sınıfı `08`'in tamamında ve geri beslenen dokümanlarda arandı.

> **Ne işe yarar:** Aşama kapanmadan önce Entegrasyon Spesifikasyonları'nın kendi içinde, Aşama 7'nin geri beslediği dokümanlarla (`02`–`07`, `10`) ve park metinleriyle (`09`, `12`, `DEPLOY_RUNBOOK`) karar kaydına ve metodolojiye karşı tutarlı olduğunu doğrular; bulunan her uyumsuzluğu ya kapatır ya da adıyla proje sahibine bırakır. Sonuç öğrenim terfisinin (5. adım) ve arşiv işaretinin (6. adım, K-436 — kapsamı K-647) girdisidir.

---

## Checkpoint Sonucu — 2026-10-29

**Aşama:** 7 — Entegrasyon spesifikasyonları (kapanış sırası K-437: yazım turu ✓ · kalite döngüsü ✓ · çakışma taraması ✓ · **checkpoint** · öğrenim terfisi · arşiv işareti)
**Genel durum:** ⚠ Dikkat gerektiren noktalar — 16 mercek ve iki betik 39 ayrı bulgu getirdi (46 ham bulgu); her biri bir Opus / `high` doğrulayıcıdan geçti: **27 doğru** (sekizi orta), **4 kısmen doğru** (biri orta), **8 çürütüldü**. Doğrulananların hepsi bu checkpoint'in PR'ında kapandı — hepsi var olan kararların yansımasıyla, yeni karar yok. Çok kritik konu yok. Kapanıştan sonra durum **✓ Yolunda**; açık kalan kapı ⚠ listesinin (30 karar; `PHASE7_CONFLICT_SCAN.md` §4.1) proje sahibine gösterilmesidir (arşiv işaretinden önce, §5).

### Kontrol Özeti

| # | Kontrol | Sonuç | Detay |
|---|---|---|---|
| 1 | Yol haritası | ✓ | Aşama 7'deyiz (`00 §C.1`: rol Integration Engineer, girdi `02`, `03`, `05`, `06`, `07`, çıktı `08`; izlenebilirlik matrisi zorunlu değil). Açılış (K-1233, K-1234), workshop, yazım turu (beş oturum), özet ↔ ayrıntı taraması, audit (K-1259…K-1268), deep review (K-1269…K-1274), cross-review dört tur — 4. turda çıkış kuralıyla TEMİZ (K-1289, K-1290) —, etki yansıtma (K-1291, K-1292) ve çakışma taraması (K-1293, K-1294; #193) bu sırayla kapandı. Atlanan ya da sırası değişen adım yok. |
| 2 | Doküman durumu | ✓ | Başlık ve dosya sonu sürümleri tracker §1 ile birebir (`01` v0.33, `02` v0.101, `03` v0.43, `04` v0.47, `05` v0.36, `06` v0.38, `07` v0.32, `08` v0.17, `10` v0.58 — betikle, çıkış sürümleriyle). `08` ⏳ kalır (✓ Tamamlandı işareti arşiv adımında); Checkpoint sütunu ✓ oldu. `09`, `11`, `12` ⬚ — şablon dokümanlarda başlık v0.1, park eklemeleri başlığa dokunmaz. |
| 3 | Tutarsızlık | ⚠ → ✓ | **Devredilen sınıf** (özet / genel cümle ↔ ayrıntı): 39 ayrı bulgunun 31'i doğrulandı (§3). En ağırları: `08 §3.1` tablosu ikinci jeton reddini ve `001`/`002`/`003` yanıtlarının farklı yinelemesini söylemiyordu (C-8, C-9); `07` UC-29'un çerçeve izni `'self'`i saymıyordu (C-3); `DEPLOY_RUNBOOK` beş kurulum işinden dördünü karar kaydından eksik taşıyordu (C-7, C-17, C-21, C-31); `05 §8.1.2` yanlış DV'ye atıf yapıyordu (C-18); `08 §2.2` kalan riski 2,5 saat diyordu, yeniden denemeler ilk denemeden sayılınca 2 saattir (C-19); `09` sorgu tutarının ayracını yalnız virgül sayıyordu (C-27). Sayılar, kimlikler ve atıflar betikle tutarlı (Notlar). |
| 4 | Açık kararlar | ✓ | Tracker §4'te açık satır yok, A-19 açılmadı; `08`'de "açık" işaretli madde yok; Ürün Gereksinimleri §13 boş (çakışma taraması §1, §3). §18.1'in açık süreç maddeleri kapısındadır: bu checkpoint kapandı; ⚠ listesi — §5; öğrenim terfisi adayları 1–7 ve Ö-23 — 5. adım. |
| 5 | Aşama çıktıları | ✓ | `08` yazıldı ve şablonun bütün bölümleri doludur (§0 yazım kuralları, §1 üç servislik envanter, §2.1 PayTR, §2.2 Amazon SES — 29 e-posta: B-1…B-16, F-1…F-6, yedi hesap ve yönetim e-postası —, §2.3 Google, §2.4 kargo listesi, §3 ortak hata yönetimi, §4 dış varsayımlar — 43 DV satırı —, §5 geri yansıtılanlar — §5.1 beş satır, §5.2 yirmi satır). Kalite döngüsünden geçti, etkileri üst dokümanlara geri beslendi (K-1292). |
| 6 | Geriye dönük etki | ⚠ → ✓ | Proje Vizyonu'na ve Kullanıcı Akışları'na etki yok. Önceki dokümanların eski kalan yüzleri §3'te kapandı — `07` (UC-29, UC-52, eşleme), `05` (§6.5.1, §8.1.2), `04` (5.21.15), `02` (§9.1.6, §3.22.3), `06` (§4.35), `10` (ÖK-3). **Kodlama Kılavuzu aşamasının (Aşama 8) girdileri hazır** (§4). |
| 7 | Yeni alan/kural | ⚠ → ✓ | Kuralın evinde eksik kalan öğeler: kurulum işleri (Google uygulamasının yayına alınması, IAM kullanıcısının üç izni, Non3D teyidi, saniyelik hızın kaydı — C-7, C-17, C-21, C-31), `DV-26`'nın doğru çıkma kolu (C-12), `DV-38`'in e-posta kolunun kanıtı (C-13), ön sorgunun `§3.1` tablosundaki yeri (C-10). Kaynak satırları: değişen bölümlerde yeni karar anılmadı (yeni karar yok). |

### Aksiyon Gerektiren Maddeler

- [x] 39 ayrı bulgunun 31'i (27 doğru, 4 kısmen) düzeltmeyle kapandı — `08` v0.17, `07` v0.32, `05` v0.36, `04` v0.47, `02` v0.101, `06` v0.38, `10` v0.58, `09`, `12`, `DEPLOY_RUNBOOK` park metinleri, karar kaydı v0.191 (§3)
- [x] Yeni karar yok (K-1295 ve K-1296 sonuç kayıtlarıdır); ⚠ listesi 30 kalır
- [x] Dokunulan Aşama 7 kararlarının etki sütununa `(checkpoint — CP07 …)` işareti eklendi (§3.3)
- [ ] **⚠ listesinin proje sahibine gösterilmesi** — 30 karar (`PHASE7_CONFLICT_SCAN.md` §4.1); kapı: arşiv işaretinden önce, tek mesajda (`INSTRUCTIONS.md` §2, §9 işletim kuralı 3)
- [ ] Sırada öğrenim terfisi (K-437'nin 5. adımı) — tracker §18.1'in yedi adayı (7. aday bu checkpoint'te eklendi, §6), Ö-23 ve maliyet düzeni ölçüm karşılaştırması

### Notlar

**Yöntem.** Mekanik kontroller betikle koştu (Python 3.12, `python -I -X utf8`): sürümler (başlık, alt bilgi, tracker §1), karar kaydı K tekilliği ve altı sütun (1.294 satır; sütun sorunu yok), bütün dokümanlarda K atıflarının çözülmesi, K-1233…K-1294'ün etki sütunundaki hedeflerin karar numarasını anması (52 hedef anmıyordu; hepsi aşağıdaki merceklerde okundu), `08`'in DV tablosu (43 satır; hepsi tanımlı) ↔ gövde atıfları, `08`'de anılan UC'ların `07`'de varlığı, PascalCase ve camelCase adların `02`/`04`/`05`/`06`/`07`'de bulunması, e-posta kimlikleri ve `10`'un KD / YH satırları (64 / 23). Anlamsal tarama on altı salt okuma merceğiyle, ayrı alt ajanlarda ve ön planda koştu; her mercek dilimini ayrı dosyada (satır numaralı) aldı ve bulgusunu bulduğu anda dosyaya yazdı: **A1…A7** — `08`'in yedi dilimi (§0–§2.1.3 · §2.1.4–§2.1.10 · §2.2.1–§2.2.5 · §2.2.6 · §2.2.7–§2.4 · §3–§4 · §5; 2, 3, 2, 0, 3, 6, 2 bulgu) · **K1…K3** — K-1233…K-1294'ün karar satırları ↔ etki sütunundaki hedefler, üç dilim (4, 8, 0) · **C** — `07 §10` ↔ `07` gövdesi ↔ `08 §5.1` (3) · **D** — `06` adları ↔ `08` alan eşlemeleri (1) · **E** — e-posta kapalı listesi (29) ↔ `02 §9` (1) · **F** — `10` kapsam sayıları (1) · **G** — Aşama 7 park blokları (`09`, `12`, `DEPLOY_RUNBOOK`) ↔ karar ve `08` (8) · **H** — sayısal değerler `08` ↔ `02`/`05`/`07` (2). 46 ham bulgu, aynı yeri anan bulguların birleşmesiyle (V02, V10, V21, V22, V27: toplam yedi bulgu) 39 ayrı bulguya indi. Her bulguyu ya da aynı yeri anan bulgu kümesini bir Opus / `high` doğrulayıcı (`[dogrulama]`; K-1224) güncel metne karşı yeniden okudu, çürütmeye çalıştı ve doğruysa dosya başına birebir şartname yazdı; şartnameler betikle uygulandı — her eski parça dosyada tam bir kez geçtiği uygulamadan önce doğrulandı (46 parça, hata sıfır). **Model dağılımı:** mercekler Sonnet / `low` (16); doğrulayıcılar Opus / `high` (39); betikler ve uygulama yönetici adım ajanında. Yaklaşık belirteç: mercekler 2,5 milyon, doğrulayıcılar 2,6 milyon. Ortam: Git Bash ve Python 3.12.

**Eşzamanlı ajan sınırı.** 39 doğrulayıcı on üçerli üç dalgada başlatıldı (sınır on dokuz); reddedilen çağrı olmadı.

**Betikle doğrulanıp tutarlı çıkanlar:**

- **Sürümler:** `01`–`08` ve `10`'un başlık / alt bilgi sürümleri ile tracker §1 birebir (`02`–`05`'in alt bilgisi sürüm notu biçimindedir, başlık sürümü ile aynı sayıyla başlar); karar kaydı başlığı v0.191.
- **K tekilliği:** 1.296 satır (K-1295, K-1296 dahil), tekrar yok; en büyük numara K-1296; K-01…K-09 sıfır dolgulu; her satır altı sütunlu.
- **K atıfları:** 12 dokümanın hepsinde (`01`–`10`, `12`, `DEPLOY_RUNBOOK`) her K numarası karar kaydında var; tek istisna `06` ve `07`'deki iki örnek ad (K-1999, K-9999: betik açıklamasında uydurma numara).
- **DV tablosu:** 43 satır, hepsi tanımlı; `07`/`02`/`05`/`06`/`10`/`12`'de tanımsız DV atfı yok. `DV-26`, `DV-27`, `DV-34` `§2`/`§3` gövdesinde `DV-n` olarak anılmıyor: `DV-26` gerçek bulgu çıktı (C-12), öteki ikisinin varsayımı gövdede adsız ve doğru anlatılıyor (mercek A6 okudu).
- **UC kapsaması:** `08`'de anılan bütün UC'lar `07`'de var.
- **E-posta kapalı listesi:** B-1…B-16 (16) + F-1…F-6 (6) + hesap ve yönetim e-postaları (7) = 29; `02 §9`, `06 §5.5.1` (`DispatchType` 29 değer), `03 §7.1.1…7.1.55` ile aynı; B-14'ün altı olayı `06 §5.6.4`'ün altı değeri.
- **Kapsam sayıları (`10`):** §3'te 64 satır (23 "Evet", 41 "Açık"), §5'te 23 aday; KD-64 ve YH-23, `08 §1`, `§2.1.1`, `§5.2` satır 13, K-1235 (5) ve K-1293 ile aynı. Başlıktaki "Yazılanlar" notu (KD-1…KD-60, YH-1…YH-22) `10` v0.2 anının tarihsel notudur; güncel sayılar sürüm notlarında ilerler.
- **Bölüm atıfları:** betiğin iki "eksik başlık" alarmı (`06` → `02 §13.1…13.4`; `08` → `05 §0.2`) yanlış alarm — iki hedef de kalın başlıklı numaralı maddelerdir, vardır.
- **Değişen dosyalar:** CR, kontrol karakteri ve `$[0-9]` artığı sıfır.

---

## 3. Bulgular ve kapanışları

Seviye: **orta** — iki yer çelişiyor; kapalı liste, sayı ya da hedef yanlış; ya da kuralın evinde bir kural eksik; **düşük** — ifade, kapsam ya da atıf farkı, anlamı değiştirmez. Kontrol sütunu skill'in yedi kontrolüne gider. Yer sütununda önsüz sayı `08`'in bölümüdür. Bir satır aynı yeri anan bulguları toplayabilir; mercek kimlikleri çalışma dosyalarınındır. Hüküm: **D** doğru · **K** kısmen doğru.

### 3.1 Doğrulanan bulgular (31)

| # | Kontrol | Yer | Bulgu | Kapanış | Hüküm / seviye |
|---|---|---|---|---|---|
| C-1 | 3 | `05` 6.x.3 (uygulama sırları) ↔ §2.1.2 (A1-1) | `05` "anahtarlar ve bildirim doğrulama anahtarı"nı iki ayrı sır gibi sayıyordu; PayTR'de bildirim doğrulaması aynı `merchant_key` ve `merchant_salt` ile yapılır | `05` cümlesi `08 §2.1.2`'ye bağlandı | D / düşük |
| C-2 | 3 | §1 (2.1 satırı) ↔ §2.1.4 `timeout_limit` (A1-2, H-2) | Özet "varsayılan 30 dakika" diyordu; ayrıntıda ürün her istekte siparişin ödeme süresinin kalanını gönderir (30 dakika `02` P-18'in ürün sabitidir) | Hücre ayrıntıya hizalandı | D / düşük |
| C-3 | 3 | `07` UC-29 çerçeve izni ↔ §2.1.4 (2), `05 §6.5.4` (A2-1) | `07` izni yalnız PayTR adresi için sayıyordu; `08` ve `05`'e göre `frame-src` aynı sayfada iki kaynak içindir (`'self'` ve PayTR) — kapalı liste yanlış | `07` UC-29 cümlesi `'self'`i ve K-1274'ü anar | D / **orta** |
| C-4 | 3 | `07` UC-29 ve UC-80 ikinci tahsilat cümleleri ↔ §2.1.7 (2) (A2-2) | `07` "Ödendi siparişe gelen ikinci başarılı ödeme iade edilir" der; `08` ve `05 §4.5.3` PayTR'de bu kolun doğmadığını söyler (K-1284) | İki cümleye PayTR kaydı eklendi | D / düşük |
| C-5 | 3 | `07 §2.1.4`, UC-52 başlıkları (beş yer) ↔ §2.3 (A5-1) | E-21 yalnız "kayıt ve yeni e-posta" diye anılıyordu; Google girişinin teyidi aynı bağlantıdan iner | Beş yerde "Google teyidi" eklendi | D / düşük |
| C-6 | 3 | `04` 5.21.15, `07` UC-52 ↔ `02 §3.13.7`, §2.3 (A5-2) | "Kullanılmış" bağlantı geçersiz sayılıyor ve yeni bağlantı gideceği söyleniyordu; bağlantı onayla tüketilir ve bağ kurar, kullanılmış bağlantıda giriş zaten onaylıdır | Kullanılmış hâl ayrıldı (`04`, `07`) | D / düşük |
| C-7 | 7 | `DEPLOY_RUNBOOK` Google (4) ↔ K-1272 (4), §2.3, DV-25 (K2-3, A5-3, G-1) | Karar uygulamanın yayına alınmasını, doğrulatılmasını ve "Session age claims" ayarını kurulum adımı yapar; blokta üçü de yoktu — "test" durumunda `auth_time` gelmez, Google ile yeniden doğrulama tutmaz | Blokta beşinci iş eklendi | D / **orta** |
| C-8 | 3 | §3.1 (ödemeyi başlat satırı) ↔ §2.1.8 (A6-1) | Tablo sonuç sınıflarını "başarı · bağlantı hatası" sayıyor ve iptali koşulsuz yazıyordu; ikinci jeton reddi üçüncü sınıftır ve siparişi iptal etmez. `§3.3` kısmı çürütüldü (o satır servis kesintisini anlatır) | Satıra üçüncü sınıf ve iptalin yalnız bağlantı hatasına bağlandığı yazıldı | K / **orta** |
| C-9 | 3 | §3.1 (sonucu sor satırı) ↔ §2.1.6 (A6-2) | Tablo bağlantı hatası sınıfı için "beş dakikada bir" diyordu; `001`/`002`/`003` yanıtları kalıcıdır, `05 §4.3.4`'ün kod hatası kuralıyla yinelenir | Hücreye ve §2.1.6'ya ayrım yazıldı | D / **orta** |
| C-10 | 7 | §3.1 (iletiyi gönder satırı) ↔ §2.2, §2.2.2 (A6-3) | E-posta için tablo iki iş sayıyor; gönderimden önceki askıdaki alıcı ön sorgusu hiçbir yerde anılmıyordu. Önerilen ayrı satır reddedildi: ön sorgu "iletiyi gönder"in adımıdır | Ön sorgu "iletiyi gönder" hücresinde adıyla anıldı | K / düşük |
| C-11 | 5 | §3.1 sonrası paragraf ↔ §2.3 hata tablosu (A6-4) | "Hata tablosunun son üç satırı" yanlış satırları gösteriyordu | "`email_verified` ve `auth_time` satırları" | D / düşük |
| C-12 | 7 | DV-26 ↔ §2.3 (A6-5) | Satır yalnız "yanlış çıkarsa" kolunu yazıyordu; doğru çıkarsa istek `max_age` taşır (K-968) yalnız §2.3'te yazılıydı | Doğru çıkma kolu satıra eklendi | D / düşük |
| C-13 | 7 | DV-38 kanıt hücresi (A6-6) | Varsayım iki olguyu kapsıyor (firma telefonu, sınıra uymayan müşteri e-postasının yerine firma e-postası); kanıt yalnız telefonu sınıyordu | Kanıta e-posta kolunun denemesi eklendi | D / düşük |
| C-14 | 5 | §5.2 satır 3 (A7-1) | `05 §3.6.1`'den tırnaklı alıntı hedefte yoktu | Alıntı hedeftekine hizalandı | D / düşük |
| C-15 | 3 | `07` eşlemesi (`05 §3.6.4` satırı) ↔ K-1235 (K1-1) | Eşleme "sayı `08`'in kararı" diyordu; sayı kapandı (tek sağlayıcı) | Eşleme kapanmış hâle getirildi | D / düşük |
| C-16 | 3 | `02 §9.1.6` ↔ §2.2.3, §2.2.4 (K1-3, E-1) | "Ulaşmadı" işaretinin tek yolu üç yeniden deneme olarak anlatılıyordu; `08` iki yol daha tanımlar (sınır aşımı, sonradan bildirilen geri dönme) | `02` cümlesine iki yol eklendi | D / düşük |
| C-17 | 7 | `DEPLOY_RUNBOOK` SES (4) ↔ §2.2 "Kimlik doğrulama", DV-16, K-1271 (3), (4) (K1-4, K2-7, G-3) | IAM kullanıcısı yalnız iki izinle anılıyordu; üçüncü izin (`ses:GetSuppressedDestination`) ve gönderme izninin kaynak / koşul bağlaması yoktu — ön sorgu hata verir, askıdaki adrese giden ileti sessizce gider | Üç izin ve bağlamalar blokta yazıldı | D / **orta** |
| C-18 | 5 | `05 §8.1.2` ↔ DV-39, DV-41 (K2-1) | "Ayrı test anahtarı yoktur" olgusu DV-39'a (IPv6) bağlıydı; doğru satır DV-41 | `DV-41` | D / **orta** |
| C-19 | 3 | §2.2 "Servis çökerse" ↔ "Retry stratejisi", K-1271 (1) (K2-2) | Yeniden deneme anları ilk denemeden sayılınca son deneme ilk denemeden 2 saat sonradır; kalan risk cümlesi hâlâ 5 + 30 + 120 dakikaya göre "yaklaşık 2,5 saat" diyordu | "Yaklaşık 2 saat" | D / **orta** |
| C-20 | 3 | `02 §3.22.3` ↔ §3.22.5, K-1256 (K2-4) | Genel cümle "bağlantı sayfayı doğrudan açar" diyordu; üyenin bağlantısı girişsiz açılmaz | Cümle üç hâle ayrıldı | D / düşük |
| C-21 | 7 | `DEPLOY_RUNBOOK` PayTR (4) ↔ K-1268 (4), §2.1.3 (K2-6, G-2) | Yazılı teyide "mağazada Non3D yetkisi kapalı" eklenmişti; blokta yoktu | Blok teyidi karara hizaladı | D / **orta** |
| C-22 | 3 | `05 §6.5.1` ↔ `07 §4.29.1`, K-1255 (1), K-1259 (1) (K2-8) | Kapanmış kararı hâlâ "park satırının kararı" ve "POST'sa ikinci istisna" diye koşullu anıyordu | Cümle kapanmış hâle getirildi | D / düşük |
| C-23 | 5 | `07 §10` UC-29 satırı ve Kaynak satırı, `08` §2.1.8 ↔ K-1260 (1) (C-2) | İkinci jeton reddi hata kolunun kararı K-1260 (1)'dir; üç yerin kaynak hücresi yalnız K-1255 (6) diyordu | K-1260 üç yere eklendi | D / düşük |
| C-24 | 3 | `07 §10` giriş, `08` §5.1 giriş ↔ tablolar (C-3) | Girişler v0.27 anını anlatıyordu ("dört satır"); tablo sonradan v0.28 ve v0.31 satırları aldı | İki girişe sonradan giren satırlar yazıldı | D / düşük |
| C-25 | 3 | `06 §4.35` `UnprocessedReason` ↔ §5.6.6, §2.1.5 (D-1) | Alan açıklaması `UnrecognizedNotification`'ı yalnız "tanınmayan" diye özetliyordu; değer sağlayıcıya özgü kabul şartını tutmayanı da kapsar | Açıklama §5.6.6'ya hizalandı | D / düşük |
| C-26 | 3 | `10 §4.1` ÖK-3 ↔ §2.2.1 (F-1) | ÖK-3 hesabı "kurulumu yapan"a veriyordu; `08` AWS hesabını firmanın açtığını söyler. Mercek önerisinin "Karşılanmazsa" kısmı çürütüldü (ret metni zaten kapsar) | "Kim sağlar" hücresi ve K-1247 atfı | K / düşük |
| C-27 | 3 | `09` park (K-1257 · K-1258) ↔ §2.1.6, DV-12 (G-4) | `09` sorgu tutarını "ondalık virgüllü" sabitliyordu; `08` ayracı virgül ya da nokta sayar (belge ornegi iki biçimde) — yalnız virgül okuyan uygulama noktalı yanıtı tanımaz | `09` cümlesi §2.1.6'ya hizalandı | D / **orta** |
| C-28 | 5 | `09` park ve `08` §5.2 satır 10 ↔ §2.2.6 B-1 madde 4 (G-5) | Kaçış kuralının atfı §2.2.5 madde 4'ü gösteriyordu; kural B-1'in 4. maddesindedir | İki atıf §2.2.6'ya düzeltildi | D / düşük |
| C-29 | 3 | `12` park (1) ↔ DV-5, DV-43 (G-6) | Kabul kanıtına düşen satırların listesi DV-5'in canlı kanıt adımını anmıyordu. DV-43 ekleme önerisi reddedildi (o satır (5)'teki senaryoyla sınanır) | Liste DV-5'le tamamlandı, DV-43 yönlendirmesi yazıldı | K / düşük |
| C-30 | 3 | `09` park ↔ §2.1.5, §2.1.6, K-934 (G-7) | "Her dönüşüm kuruşa yuvarlama kuralıyla" diyordu; sağlayıcı dönüşümleri yuvarlamaz (tam sayı kuruş), K-934 tutar hesabının kuralıdır | Cümle ayrıldı | D / düşük |
| C-31 | 7 | `DEPLOY_RUNBOOK` SES (2) ↔ DV-18, K-1248 (5) (G-8) | Okunan günlük kota kuruluma yazılır diyen karar saniyelik hızı (`MaxSendRate`) da yazar; blok yalnız kotanın doğrulanmasını söylüyordu | Kayıt cümlesi eklendi | D / düşük |

### 3.2 Elenenler (8 çürütüldü)

| # | Bulgu | Neden elendi |
|---|---|---|
| E-1 | §2.1.6 sorguda `TL` ya da `TRY`, bildirimde yalnız `TL`; "eşi" ifadesi kümeleri ayırıyor (A2-3) | K-1286 farkı bilerek koymuş: "eşi" ifadesi ve iki yazım aynı kararda, "yalnız TL" gerekçeyle elenmiş |
| E-2 | §2.2.4 hesap e-postalarının limitleri tek atıfla L-9'a bağlı; L-2 (şifre sıfırlama) eksik (A3-1) | Atıf önce `02 §8.2`'nin tamamına gider (dokuz limit); metin K-1242 (1)'in ifadesidir |
| E-3 | `RecipientError` sınıfı servise çağrı yapılmayan üç durumda kullanılıyor; `05 §10.2.2` tanımı bunları kapsamıyor (A3-2) | Üç durum kararda adıyla yazılı (K-1242 (3), K-1271 (2), (3)); bedeli de yazılı — sınıfı bilerek genişletmişler |
| E-4 | §5.2 satır 1 ve 5 aynı değişiklik için satır 14'ten farklı sürüm gösteriyor (A7-2) | K-1259 (3) geri beslemeyi satır 14'e bağlar; satır 1 ve 5 "(K-1259; satır 14)" ile yönlendirir, kendi asıl geri beslemelerinin sürümünü gösterir |
| E-5 | `03` 10.1.1.7 "sağlayıcıyı değiştirir" durumunu `08`'e bırakıyor; K-1239 (1) sağlayıcı değişikliğini desteklemez (K1-2) | K-1239'un etki sütunu "devir kapandı — değişiklik yok" der; `08 §2.1.2` devredilen konuyu tutarlı yanıtlıyor |
| E-6 | `04` 5.25.13 K-1263 (6)'nın cümlesini taşımıyor (K2-5) | 5.25.13'teki "sipariş e-postasındaki bağlantıyla" izleme yolunu `04` v0.44 silmişti; cümleye gerek ve çelişki yok |
| E-7 | `07 §10` v0.27 satırı "sipariş sayfası `frame-ancestors 'self'` ister" diyor, sonra taşındı (C-1) | §10 bir değişiklik kaydıdır; v0.27 satırı o anı doğru anlatır, v0.31 satırı taşımayı açıkça kaydeder |
| E-8 | `08 §3.2` 64 KB için `05 §6.3.5`'e atıf yapıyor, tavan §6.3.6'dadır (H-1) | Parantez cümlenin tamamına kaynak: 64 KB ve 413 için `07 §4.30.1`, IP sınırı dışında kalma ve form sınırına sayılma için `05 §6.3.5` — iki atıf da doğru |

### 3.3 Etki işaretleri

Dokunulan Aşama 7 kararlarının etki sütununa `(checkpoint — CP07 C-n)` işareti eklendi: K-1235 (C-15), K-1241 · K-1242 (C-16), K-1247 (C-26), K-1248 (C-31), K-1254 (C-1), K-1255 (C-22), K-1256 (C-20), K-1259 · K-1274 (C-3), K-1260 (C-23), K-1268 (C-21), K-1271 (C-17), K-1272 (C-7), K-1281 · K-1282 (C-5, C-6), K-1284 (C-4). Kaynak satırlarına yeni karar eklenmedi.

---

## 4. Kontrol 6 — Kodlama Kılavuzu aşamasının girdileri

`00 §C.1`: Aşama 8 — Kodlama kılavuzu (`09`).

| Girdi | Durum |
|---|---|
| `08` v0.17 | ⏳ → ✓ arşiv adımında (K-437'nin 6. adımı); checkpoint ve öğrenim terfisi sonrası |
| `09`'un park blokları | Aşama 1–7, yedisi de var. Aşama 7 bloğu K-1257 · K-1258 maddesinden başlar (bağlantı katmanının kodlama kalıpları, imzalı uç, tiresiz sipariş numarası, imza ve tutar doğrulaması, alan eşlemesinin okunmayan alanları, Google kimlik belirteci); bu checkpoint iki cümleyi düzeltti (C-27, C-30) ve bir atfı (C-28); eksik girdi yok |
| Devir dizini | `PHASE7_CONFLICT_SCAN.md` §6 — `09` için ilgili kararlar; `08`'in gövdesinde `09`'a bırakılan beş cümle okundu (checkpoint `09`'a iş bırakan yeni cümle eklemedi) |
| `12` ve `DEPLOY_RUNBOOK` | Aşama 7 blokları var; checkpoint `DEPLOY_RUNBOOK`'ta dört kurulum işini karara hizaladı (C-7, C-17, C-21, C-31), `12`'de bir listeyi tamamladı (C-29) |

Eksik girdi yok; Aşama 8 açılışı bu tabloyu ve çakışma taramasının §6'sını okur.

---

## 5. Proje sahibinin gözden geçirme listesi

> **Sonuç (2026-10-29, K-1299):** otuz ⚠ karar proje sahibine tek mesajda gösterildi; itiraz yok. Otuz işaret `(öneriyle kaydedildi — ⚠ — gözden geçirildi 2026-10-29, itiraz yok)` oldu.

Bu checkpoint yeni ⚠ karar eklemedi (yeni karar yok). Liste çakışma taramasının sayımıyla aynıdır: **30 karar** (`PHASE7_CONFLICT_SCAN.md` §4.1 — hazır metin orada). **Kapı:** arşiv işaretinden önce, tek mesajda, karar başına bir sade cümleyle ve gerekçesiz (`INSTRUCTIONS.md` §2, §9 işletim kuralı 3). Gösterim yöneticidedir.

---

## 6. Öğrenim terfisine notlar — 5. adımın girdisi

Adayların metinleri tracker §18.1'dedir; bu checkpoint bir aday ekledi.

| # | Aday | Kaynak | Playbook |
|---|---|---|---|
| 7 | Etki sütunu `DEPLOY_RUNBOOK`'u gösteren kararın her kurulum işi (izin, yetki, ayar, saklanan değer) bloktaki işin metniyle yan yana okunmalı; etki yansıtmanın betiği yalnız kararın anıldığını arıyor | C-7, C-17, C-21, C-31 | `checklists/document-stage.md` §7 (etki yansıtma) |

**Kanıt — mevcut adaylara:** aday 5'in ("⚠ listesinin elle tutulması") çakışma taramasında düzeltilen sayımı bu checkpoint'te yeniden betikle koşuldu: 30 / 30. Aday 6'nın ("kıyas tablosunun sonuç cümlesi uygulama bölümüyle çelişebilir") benzeri bu checkpoint'te `08 §3.1` tablosunda çıktı (C-8, C-9, C-10): tablonun özet hücreleri §2.1.6 ve §2.1.8'in ayrıntısını daraltıyordu.

**Tek seferlik gözlem (raporda kalır):** 46 parça şartnamenin hiçbiri çakışmadı; aynı dosyaya (`07`, `08`) on üçer parça indi ve ESKI parçalar örtüşmeyen kesimlerdir. Çürütülen sekiz bulgunun beşi kapanmış bir kararın bilinçli sonucuydu (E-1, E-2, E-3, E-4, E-5) — doğrulayıcılar karar kaydını ilk iş okuyor.

---

*Kaynak: K-437 (kapanış sırası, 4. adım) · K-429 (çakışma taraması — girdi) · K-1129 (cross-review'ın çıkış kuralı ve checkpoint'e devri) · K-652 (geri besleme) · K-29 (etki işaretleri) · K-647 (kaydın evi) · K-1144 (⚠ öneriyle kayıt) · K-1224 (bulgu doğrulaması Opus / high) · K-1295, K-1296 (bu checkpoint'in kayıtları) · `.claude/skills/checkpoint/SKILL.md` · `.claude/skills/audit/SKILL.md` ("Koşum biçimi") · `Docs/CHECKPOINT_REPORTS/CP06_PHASE6_CHECKPOINT.md` (biçim örneği) · `Docs/CHECKPOINT_REPORTS/PHASE7_CONFLICT_SCAN.md` (girdi).*
