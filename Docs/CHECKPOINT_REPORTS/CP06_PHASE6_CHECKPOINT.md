# Aşama 6 — Checkpoint (CP06)

**Tarih:** 2026-10-28 | **Aşama:** 6 — API tasarımı | **Kapanış adımı:** K-437'nin 4. adımı (`/checkpoint`; `checklists/document-stage.md` §7)
**Girdi sürümleri:** karar kaydı v0.170 · API Tasarımı (`07`) v0.24 · Ürün Gereksinimleri (`02`) v0.93 · Kullanıcı Akışları (`03`) v0.40 · Arayüz Tanımları (`04`) v0.41 · Teknik Mimari (`05`) v0.29 · Veri Modeli (`06`) v0.32 · MVP Kapsamı (`10`) v0.55 · Proje Vizyonu (`01`) v0.33 · `08`, `09`, `12` ve `DEPLOY_RUNBOOK`'un park blokları — `origin/main` @ `150c12f`
**Çıktı sürümleri:** karar kaydı v0.171 · API Tasarımı v0.25 · Arayüz Tanımları v0.42 · Teknik Mimari v0.30 · Ürün Gereksinimleri v0.94 · Veri Modeli v0.33 · Kullanıcı Akışları, MVP Kapsamı ve Proje Vizyonu değişmedi · `08`, `09`, `12` ve `DEPLOY_RUNBOOK`'un park satırları (sürüm alanı değişmez)
**Girdi raporu:** [Aşama 6 — Çakışma Taraması](PHASE6_CONFLICT_SCAN.md) (K-437'nin 3. adımı) — açık kalem durumu, ⚠ listesinin hazır metni (§4.1), devir dizini (§6) ve checkpoint'e notlar (§8) oradan alındı; çakışma taramasının bulguları ikinci kez açılmadı. **Devredilen sınıf** (K-1129; `checkpoint` skill'inin "kalite döngüsünün devri" maddesi) — cross-review çıkış kuralıyla kapandı (5 tur, K-1223); iki yerin metin farkı sınıfı `07`'nin ve geri beslenen dokümanların tamamında arandı.

> **Ne işe yarar:** Aşama kapanmadan önce API Tasarımı'nın kendi içinde, Aşama 6'nın geri beslediği dokümanlarla (`02`–`06`) ve park satırlarıyla (`08`, `09`, `12`, `DEPLOY_RUNBOOK`) karar kaydına ve metodolojiye karşı tutarlı olduğunu doğrular; bulunan her uyumsuzluğu ya kapatır ya da adıyla proje sahibine bırakır. Sonuç öğrenim terfisinin (5. adım) ve arşiv işaretinin (6. adım, K-436 — kapsamı K-647) girdisidir.

---

## Checkpoint Sonucu — 2026-10-28

**Aşama:** 6 — API tasarımı (kapanış sırası K-437: yazım turu ✓ · kalite döngüsü ✓ · çakışma taraması ✓ · **checkpoint** · öğrenim terfisi · arşiv işareti)
**Genel durum:** ⚠ Dikkat gerektiren noktalar — 15 mercek ve iki betik 36 ayrı bulgu getirdi; her biri bir Opus / `high` doğrulayıcıdan geçti: **20 doğru** (üçü orta), **8 kısmen doğru**, **8 çürütüldü**. Doğrulananların hepsi bu checkpoint'in PR'ında kapandı — çoğu var olan kararların yansımasıyla, biri yeni kararla (K-1229, ⚠ öneriyle kaydedildi). Çok kritik konu yok. Kapanıştan sonra durum **✓ Yolunda**; açık kalan kapı ⚠ listesinin (çakışma taramasının dokuz kararı ve K-1229) proje sahibine gösterilmesidir (arşiv işaretinden önce, §5).

### Kontrol Özeti

| # | Kontrol | Sonuç | Detay |
|---|---|---|---|
| 1 | Yol haritası | ✓ | Aşama 6'dayız (`00 §C.1`: rol API Designer, girdi `02`–`06`, `10`, çıktı `07`; izlenebilirlik matrisi zorunlu). Açılış, matrisin yedi oturumu, workshop, yazım turu (altı oturum), özet ↔ ayrıntı taraması ve hükmü, audit (K-1199…K-1206), deep review (K-1207…K-1217), cross-review beş tur — 5. turda K-1129'un çıkış kuralıyla TEMİZ (K-1218…K-1223) —, etki yansıtma (K-1224…K-1226) ve çakışma taraması (K-1227, K-1228; #173) bu sırayla kapandı. Atlanan ya da sırası değişen adım yok. |
| 2 | Doküman durumu | ✓ | Başlık ve dosya sonu sürümleri tracker §1 ile birebir (`01` v0.33, `02` v0.94, `03` v0.40, `04` v0.42, `05` v0.30, `06` v0.33, `07` v0.25, `10` v0.55 — betikle, çıkış sürümleriyle). `07` ⏳ kalır (✓ Tamamlandı işareti arşiv adımında); Checkpoint sütunu ✓ oldu. `08`, `09`, `11`, `12` ⬚ — şablon dokümanlarda başlık v0.1, park eklemeleri başlığa dokunmaz. |
| 3 | Tutarsızlık | ⚠ → ✓ | **Devredilen sınıf** (özet / genel cümle ↔ ayrıntı): 36 ayrı bulgunun 28'i doğrulandı (§3). En ağırları: `07 §4.13`'ün girişi UC-27'yi 303 listesine koyuyordu (C-1); UC-83'ün iki 400 hâli kodsuzdu (C-3); CSV formül nötrlemesi sekmeyi ve CR'yi saymıyordu (C-4). Sayılar, kimlikler ve atıflar betikle tutarlı (Notlar). |
| 4 | Açık kararlar | ✓ | Tracker §4'te açık satır yok, A-19 açılmadı; `07`'de "açık" işaretli madde yok; Ürün Gereksinimleri §13 boş (çakışma taraması §1, §3). §16.1'in açık süreç maddeleri kapısındadır: bu checkpoint kapandı; ⚠ listesi — §5; öğrenim terfisi adayları 1–12 ve Ö-23 — 5. adım. Desteklenen ödeme sağlayıcısının sayısı Entegrasyon spesifikasyonları aşamasının açılışındadır (kontrol 6). |
| 5 | Aşama çıktıları | ✓ | `07` yazıldı ve şablonun bütün bölümleri doludur (§1 matris 187 aday, §2 konvansiyonlar, §3 envanter 187 uç, §4 ve §9 uç tanımları, §5 DTO envanteri, §6 hata kodları — §6.2 114 kod, §6.3 22 kod —, §7, §8, §10). UC kapsaması betikle: §1.1.2'nin 187 adayı, §3'ün 187 satırı ve §4 / §9'un tanımları birebir (UC-78 uç değildir, §3.4.3); tekrar yok. Kalite döngüsünden geçti, etkileri üst dokümanlara geri beslendi (K-652). |
| 6 | Geriye dönük etki | ⚠ → ✓ | Proje Vizyonu'na etki yok. Önceki dokümanların eski kalan yüzleri §3'te kapandı — Ürün Gereksinimleri (`02 §10.7.4`), Arayüz Tanımları (5.13.20, 6.2.13.10, 6.2.12.7), Teknik Mimari (§3.6.3, §4.2.2), Veri Modeli (matris satırı). **Entegrasyon spesifikasyonları aşamasının girdileri hazır** (§4). |
| 7 | Yeni alan/kural | ⚠ → ✓ | Kuralın evinde eksik kalan öğeler: UC-83'ün iki hâlinin kodu (C-3), kayıt ekranlarının ortak parametresinin modeli (C-8), `InvalidAmount`'un üst sınır hâli (C-13), onay anında asgari tutarın yanıtı (C-2 — K-1229). Kaynak satırları: değişen üç bölümün (`07 §4.13`, `04 §5.13`, `05 §4.2`) Kaynak satırına K-1229 eklendi. |

### Aksiyon Gerektiren Maddeler

- [x] 36 ayrı bulgunun 28'i (20 doğru, 8 kısmen) düzeltmeyle kapandı — `07` v0.25, `04` v0.42, `05` v0.30, `02` v0.94, `06` v0.33, park satırları, karar kaydı v0.171 (§3)
- [x] Bir karar — K-1229 (öneriyle kaydedildi — ⚠); ⚠ listesi 9 → **10**
- [x] K-1129'un devri kapandı (`07`'nin tamamında iki yerin metin farkı arandı)
- [x] Dokunulan Aşama 6 kararlarının etki sütununa `(checkpoint — CP06 …)` işareti, K-1181 ve K-155'e K-1229'un geri işareti eklendi (§4)
- [ ] **⚠ listesinin proje sahibine gösterilmesi** — çakışma taramasının dokuz kararı (`PHASE6_CONFLICT_SCAN.md` §4.1) ve K-1229 (§5.2); kapı: arşiv işaretinden önce, tek mesajda (`INSTRUCTIONS.md` §2, §9 işletim kuralı 3)
- [ ] Sırada öğrenim terfisi (K-437'nin 5. adımı) — tracker §16.1'in on iki adayı (10–12 bu checkpoint'te eklendi, §6), Ö-23 ve maliyet düzeni ölçüm karşılaştırması

### Notlar

**Yöntem.** Mekanik kontroller betikle koştu (Python 3.12, `python -I -X utf8`): sürümler (başlık, alt bilgi, tracker §1), karar kaydı K tekilliği ve altı sütun, bütün dokümanlarda K atıflarının çözülmesi, UC kapsaması (§1.1.2 ↔ §3 ↔ §4 / §9), dokümanlar arası `NN §x.y` atıflarının hedefinin varlığı, `07 §6`'daki kod adlarının tekilliği ve `07`'deki PascalCase adların Veri Modeli'nde / `§5`'te / `§6`'da bulunması (161 adın hepsi `07 §5`'te; tek istisna çerçevenin başlığı `RequestVerificationToken`), ⚠ listesi, park etiketi ↔ etki sütunu (iki yön), CR / kontrol karakteri / `$[0-9]`. Anlamsal tarama on beş salt okuma merceğiyle, ayrı alt ajanlarda ve ön planda koştu; her mercek dilimini ayrı dosyada (satır numaralı) aldı ve bulgusunu bulduğu anda dosyaya yazdı: **S01** — başlık notları ve §2 (1 bulgu) · **S02** — §3 ve §5 (5) · **S03**, **S04** — §4'ün iki yarısı (1, 2) · **S05** — §6 ve §8 (2) · **S06**, **S07** — §9'un iki yarısı (3, 4) · **M1…M4** — matrisin dört dilimi (1, 2, 1, 4) · **K1…K3** — karar satırları ↔ hedef metin, üç dilim (2, 3, 2) · **P** — Aşama 6 park blokları (5). 38 ham bulgu üç eşle (S04-1 = S05-1, S02-3 = K2-3, K3-1 = P-1) 35 ayrı bulguya indi; betiğin bulduğu bir satır daha eklendi (K-1151). Her bulguyu ya da aynı yeri anan bulgu kümesini bir Opus / `high` doğrulayıcı (`[dogrulama]`; K-1224) güncel metne karşı yeniden okudu, çürütmeye çalıştı ve doğruysa dosya başına birebir şartname yazdı; şartnameler betikle uygulandı — her eski parça dosyada tam bir kez geçtiği uygulamadan önce ve sırasında doğrulandı (61 parça, hata sıfır). **Model dağılımı:** mercekler Sonnet / `low`; doğrulayıcılar Opus / `high`; betikler ve uygulama yönetici adım ajanında. Ortam: Git Bash ve Python 3.12.

**Eşzamanlı ajan sınırı.** 27 doğrulayıcı tek mesajda başlatıldı; sınır yirmidir, beşi reddedildi ve ilk yirmi bitince aynı talimatla yeniden başlatıldı (öğrenim adayı 10).

**Betikle doğrulanıp tutarlı çıkanlar:**

- **Sürümler:** `01`–`07` ve `10`'un başlık / alt bilgi sürümleri ile tracker §1 birebir; karar kaydı başlığı v0.171.
- **K tekilliği:** 1.229 satır, tekrar yok; en büyük numara K-1229; K-01…K-09 sıfır dolgulu. Tablonun sütun sayısı: K-1151 ve K-1220 `awk -F'|'` ile 6'dan fazla çıkıyor — K-1220 `\|` kullandığı için yanlış alarm, K-1151'in kaçışsız `|` karakteri gerçek bir kusurdu (C-27).
- **K atıfları:** 13 dokümanın hepsinde (`01`–`12`, `DEPLOY_RUNBOOK`) her K numarası karar kaydında var.
- **UC kapsaması:** 187 aday = 187 envanter satırı; envanterde olup tanımı olmayan ya da tanımı olup envanterde olmayan uç yok.
- **⚠ listesi:** konu hücresi `(öneriyle kaydedildi — ⚠` taşıyan satırlar 10 (K-1144 mod kararının kendi metnini anması hariç): K-1161, K-1164, K-1185, K-1192, K-1199, K-1202, K-1210, K-1211, K-1213, K-1229.
- **Park etiketi ↔ etki sütunu:** etki sütunu bir park satırını anan her K bir etikette geçiyor (iki dokümanda etiket düzeltildi — C-25); etiketi olan ama etki sütunu eksik kalan yalnız K-1204 idi, eklendi.
- **Değişen dosyalar:** CR, kontrol karakteri ve `$[0-9]` artığı sıfır.

---

## 3. Bulgular ve kapanışları

Seviye: **orta** — iki yer çelişiyor; kapalı liste, sayı ya da hedef yanlış; ya da kuralın evinde bir kural eksik; **düşük** — ifade, kapsam ya da atıf farkı, anlamı değiştirmez. Kontrol sütunu skill'in yedi kontrolüne gider. Yer sütununda önsüz sayı `07`'nin bölümüdür. Bir satır aynı yeri anan bulguları toplayabilir; mercek kimlikleri çalışma dosyalarınındır. Hüküm: **D** doğru · **K** kısmen doğru.

### 3.1 Doğrulanan bulgular (28)

| # | Kontrol | Yer | Bulgu | Kapanış | Hüküm / seviye |
|---|---|---|---|---|---|
| C-1 | 3 | §4.13 girişi, UC-27, §6.2 (`SalesClosed`, `NoOrderableItem`, `MinimumOrderAmountNotMet`) (S03-1) | Giriş UC-27'yi "ödeme adımı açılamıyorsa 303 → `/sepet`" listesine koyuyordu; UC-27 satış o arada kapanınca 422 verir (K-1181 (6); `04` 5.13.27). §6.2'nin üç satırı UC-23…UC-25'in 303'ünü saymıyordu | Giriş daraltıldı, UC-27 kendi tanımına gönderildi; üç satıra UC-22…UC-25 eklendi | D / **orta** |
| C-2 | 3 | UC-27, `05 §4.2.2` (2), `04` 5.13.20 (S03-1'in karar kolu) | `02 §3.18.1` (K-155) onay anında asgari tutarın altına düşen sepette "sipariş durur" der; yanıtın biçimi hiçbir yerde yazılı değildi ve `05 §4.2.2` bu hâli kapı saymıyordu | **K-1229 ⚠** — 303 → `/sepet`; `07` UC-27 ile iki matris hücresi, `04` 5.13.20 ile 6.2.13.10, `05 §4.2.2` (2) | D / **orta** |
| C-3 | 3 | UC-83 ↔ §6.2.9 (S04-1, S05-1) | UC-83'ün 400'ü üç adı konmuş hâl sayıyordu (boy aşımı, geçersiz başlatma gövdesi, `Content-Range` yok ya da biçimsiz); §6.2.9'da yalnız birinin kodu vardı. Boy aşımının tanımı da farklıydı (gövde ↔ alınan bayt toplamı) | Yeni iki kod: `UploadStartInvalid`, `UploadContentRangeInvalid` (§6.2 112 → 114 kod); tanım hizalandı | D / **orta** |
| C-4 | 3 | §9.24.2 UC-182 (UC-183, UC-184 atıfla), `09` park satırı (K-1193 · K-1195), `12` park satırı (K-1193 …) (K3-1, P-1) | K-1217 (3) CSV formül nötrlemesine sekme ve CR'yi de katıyor; üç yer yalnız `=`, `+`, `-`, `@` sayıyor ya da listesizdi | Üçünde "sekmeyle ya da CR ile" | D / **orta** |
| C-5 | 3 | UC-49, UC-50, UC-55, UC-61, UC-62, UC-77, UC-110, UC-185 İstek maddeleri ↔ §5.2 (S02-1, S02-2) | POST formlarının alanı uç tanımlarında camelCase (`returnUrl`, `pendingRegistration`, `reauthenticatedOperation`); §5.1.5 ve K-1172 (5) form alanını Pascal der. Doğrulayıcı üç eş yüz daha buldu (UC-49, UC-50, UC-77) | Sekiz yerde Pascal; GET sorgu parametreleri camelCase kaldı | D / düşük |
| C-6 | 3 | UC-83 İstek ve 400 maddeleri ↔ §5.4.2 (S02-3, K2-3) | JSON alanı Pascal (`FileName`, `SizeBytes`); aynı madde "JSON alan adları camelCase" der, §5.4.2 ve K-1196 (6) `fileName`, `sizeBytes` | camelCase; `06 §4.70` adı parantezde | D / düşük |
| C-7 | 3 | §3.2'nin Yetki hücreleri (UC-27, UC-35, UC-36, UC-40, UC-43, UC-45, UC-46) (S02-4) | UC-20, 21, 72, 73'te "sahiplik" yazılı; aynı denetimi taşıyan yedi ucun envanter hücresinde yoktu (uç tanımlarında var) | "; sahiplik" eklendi | D / düşük |
| C-8 | 3 | §5.3.2 ↔ §5.1.1 (S02-5) | §5.1.1 "parametre alan her GET'in modeli" der; UC-88, UC-92, UC-94 `returnUrl` alır, satırı yoktu | §5.3.2'nin sonuna ortak parametre cümlesi | D / düşük |
| C-9 | 3 | UC-83 hata maddesi ↔ §5.4.2 (S04-2) | §5.4.2 "400, 404 ve 413 gövdesizdir (§4.31.2)" der; §4.31.2 bunu söylemiyordu | Cümle §4.31.2'ye eklendi | D / düşük |
| C-10 | 3 | UC-61, UC-185 ↔ §6.2.5 (S05-2) | Uç tanımları "şifre boş ya da yanlış" 422'sini L-1'e sayıyordu; boş şifre `FieldRequired` alır ve sayılmaz (`04` 5.24.6, 9.25.9) | İki hâl ayrıldı; §6.2.5 satırı doğruydu | D / düşük |
| C-11 | 3 | UC-113, UC-116, UC-174, UC-187 vb. (altı yer) (S06-1) | `07` §9.2'de olmayan "9.2.4", "5.21.8", "9.25.5", "5.21.4" `04`'ün maddeleridir; önek eksikti | `04` öneki eklendi | D / düşük |
| C-12 | 3 | §9 girişi ↔ §2.4.2, §2.4.4 (S06-2, S07-1) | "Her form siparişin `Version`'ını taşır" iç not istisnasını yazmıyordu; OB-10'u olmayan kayıtlar üç sayılıyordu, kuralın evi beş sayar (yönetici hesapları panelde) | İstisna ve "yönetici hesapları" eklendi | D / düşük |
| C-13 | 3 | §6.2.2 `InvalidAmount` ↔ UC-168, §2.9.4 (S07-3) | Para türünün üst sınırı aşımı 422 sayılıyor (K-1214); kod satırının kapsamı bu hâli anmıyordu | Kapsam cümlesine eklendi | D / düşük |
| C-14 | 3 | `02 §10.7.4` ↔ K-823, UC-183; `06` matris satırı (S07-4) | Üye dosyası "hiç sipariş vermemiş üyeler" diye daraltılmıştı; K-823 ve UC-183 bütün üye hesaplarını taşır | `02` (v0.94) ve `06` (v0.33) cümleleri K-823'e hizalandı | D / düşük |
| C-15 | 3 | Matris `04` 9.9.20 ↔ 7.2.10.5, UC-130 (M2-1) | Özet hücresi "istisnalı kalemde onay kutusu" der; ayrıntı yalnız mutlak istisnalı kalemde tanır | "mutlak istisnalı" | D / düşük |
| C-16 | 3 | Matris `04` 9.18.9 ↔ UC-170 (M2-2) | Özet "süre" der; yalnız kargoya verme süresi üretilen metnin sürümünü artırır, havale ödeme süresi artırmaz (K-1215) | "kargoya verme süresi" | D / düşük |
| C-17 | 3 | Matris `03 §6.1.1.9` ↔ `04` 7.3.3.9, UC-66, UC-179 (M3-1) | L-9'u eşleyen iki satırın uç kümesi ayrışıyordu; yeni e-posta isteyen UC-66 ve UC-179 L-9'a girer (K-734) ama listede yoktu. Bulgunun UC-175 kısmı yanlıştı — UC-175 "yeni davet limite girmez" cümlesinin ucudur | UC-66 ve UC-179 eklendi, UC-175 kaldı | K / düşük |
| C-18 | 3 | Matris `03 §8.5` ↔ `03 §8.5.5` (M4-3) | Özet "tavan üç" koşulsuz; `03` ve `02 §10.5.4` "tek tipli siparişin hattında" der, karışık sipariş dört adıma çıkar | Niteleyici eklendi | D / düşük |
| C-19 | 3 | `05 §3.6.3` ↔ K-1211 (1) (K3-2) | K-1211 açık oturumun yeniden verilmesini ve sağlayıcıya tekil referansı birlikte koyar; `05` "ya da" yazıyordu | "ve" (Teknik Mimari v0.30) | D / düşük |
| C-20 | 3 | `08` ve `09` park satırları ↔ K-1217 (2) (P-4) | Park satırları "saatlik" pencereyi ve "(B-1, B-2)" kapsamını yazıyordu; karar yalnız "alıcı başına gönderim sayacı" der ve pencereyi `08`'e bırakır | İki satır karara indirildi | D / düşük |
| C-21 | 3 | `12` park satırı ↔ K-1185 (2) (P-5) | "Geçersiz anahtar 404" ödeme dönüşünün 404'ü sanılabilir; uç yalnız biçimsiz değere 404 verir, 404'ü sipariş sayfası verir | Cümle K-1185 (2)'ye hizalandı | K / düşük |
| C-22 | 3 | UC-131 ikinci 409 (S06-3) | "Zaten başlatılmış" hâli kodsuz görünüyordu; başlatma sürümü artırdığı için tekrar gönderim ortak kuralın 409'udur (`OrderVersionConflict`) | Madde ortak kurala ve koda bağlandı; §6.2 değişmedi | K / düşük |
| C-23 | 3 | `04` 6.2.12.7 ↔ 6.2.12.6, K-1200 (K2-1) | "Sağlayıcıya erişilemedi" hücresi mesajsız görünüyordu; iki hâl ayrı değil — sepete her dönüşte mesaj var. Bulgunun "onaydan önce / sonra" ayrımı yanlıştı | `04` 6.2.12.7 ve matris hücresi mesajın 6.2.12.6'da olduğunu anar | K / düşük |
| C-24 | 3 | K-1206 (2) ↔ `07 §2.5.8` (K2-2) | Karar kaydı oturumsuz açık formları dört sayar; `07` "giriş"i de ekler (deep review L12-11) — `07` doğru | K-1206'nın etki sütununa geri işaret; `07` değişmedi | K / düşük |
| C-25 | 3 | `09`, `12`, `DEPLOY_RUNBOOK`, `08` park etiketleri (P-3) | Dört etiketin K numaralarından bazıları maddenin kuralını taşımıyordu (K-1211 `09`'da ve `DEPLOY_RUNBOOK`'ta; K-1217 `12`'nin 67. satırında); karar kaydında olmayan kurallar deep review bulgularıdır (K'sız uygulama). Sahte özet maddesinin kuralı K-1204 etikette yoktu. Bulgunun "kayda yeni K ekle" önerisi reddedildi | Etiketler kuralın gerçek evine hizalandı, K'sız bulgu kimlikleri yazıldı; K-1211 ve K-1204'ün etki sütunu | K / **orta** |
| C-26 | 3 | §1.1.2 aday listesi ve §1.2 başlıkları (M4-1) | Liste devir hâlinde: UC-26 "bir ya da iki işleyici", UC-30 yolu `08`'de, UC-50 / UC-182…UC-184 "GET ya da POST". Hücre düzeltmesi önerisi reddedildi — §3.4.1 listeyi önvarsayar; kusur, listenin devir hâli olduğunun yazılı olmamasıydı | §1.1.2'nin girişine ve §1.2'ye iki yönlendirme cümlesi | K / düşük |
| C-27 | 3 | Karar kaydı K-1151 (V27) | Gerekçe hücresindeki kaçışsız `| n.m.k |` parçası tabloyu bölüp etki sütununu görüntüden siliyordu. K-1220 `\|` kullanıyor — yalnız `awk` yanlış alarmı | `\| n.m.k \|` | K / **orta** |
| C-28 | 7 | `05 §4.2.2`, `04` 5.13.20, 6.2.13.10 (C-2'nin kendi yüzleri) | K-1229'un yansıması: kuralın evi `02 §3.18.1`, hücreler `07` matrisinin iki satırı ve `04`'ün iki yeri | K-1229 satırı, geri işaretler (K-1181 (6), K-155) ve Kaynak satırları | — |

### 3.2 Elenenler (8 çürütüldü)

| # | Bulgu | Neden elendi |
|---|---|---|
| E-1 | §2.1.6'daki "sipariş kalemi" K-1011 (3)'te yok (S01-1) | Liste K-1162 (6)'nın birebir metnidir; sipariş kalemini kullanan uç K-1182 (3)'le bağlı (`orderItemId`). Park satırı Aşama 5'in talimat aynasıdır — hizalamak geçmişi yeniden yazmak olurdu |
| E-2 | UC-105 hazır dönemleri beş sayıyor, `§5.3.2` yedi (S07-2) | UC-105'in cümlesi liste değil sınır cümlesidir; `04` 9.22.3 (1) ve K-1215 (1) ile birebir aynı. Bugün ve Dün'ün sınırı aynı cümlenin başında; tam liste §5.3.2'de |
| E-3 | UC-30'un yolu aday listesinde `08`'e bırakılmış (M1-1) | Yol §3.4.1'de K-1185 ile kayıtlı; aday listesi devir hâlidir (C-26) |
| E-4 | UC-26 "bir ya da iki işleyici" (K1-1) | K-1161 (4)'ün birebir ifadesi; güncel hâl §3.2'de |
| E-5 | UC-182…UC-184 "GET ya da POST" (K1-2) | POST'a bağlama §3.4.1'de adlarıyla yazılı; §1.2'nin başlıkları yöntemi listeden alır (K-1160 (1)). Önerilen düzeltme §3.4.1'i bozardı |
| E-6 | `03 §8.9.2` özeti "tek tek" derken UC-154 toplu (M4-2) | 03 §8.9.2'nin kendi metni "tek tek … her biri ayrı"; toplu düğme ekran düzeyinde (`04` 2.5.3.1, 9.9.4), UC-154 her e-posta için ayrı gönderim satırı yazar |
| E-7 | K-830 → park A3-1 ve K-1130 → park A5-3, park etiketinde yok (M4-4) | Ok "bu park maddesiyle karşılanır" demektir; etikete yalnız maddenin kuralını değiştiren kararlar girer. Etikete eklemek K-830'un "park satırları değişmedi" hükmüyle çelişirdi |
| E-8 | K-1189 (3) ad boşken yalnız stok kodu mesaj alır, `07` ve `12` "iki alan" der (P-2) | Adın zorunluluğu `04` 7.2.4.2 ve 9.5.21'de ayrıca yazılı; karar adı dışlamıyor, öneriyi anlatıyor |

### 3.3 Betiğin yanlış alarmı

`awk -F'|'` kaçışlı `\|` karakterini tanımadığı için K-1220'yi 6'dan fazla sütunlu saydı. Satır düzgün görüntüleniyor (GFM'de `\|` kod aralığında da hücreyi bölmez); düzeltme gerekmedi. K-1151'de gerçek kusur vardı (C-27). İki betik (`07 §1.1.1` satır 262 ve tracker §16.3'ün açılış komutu) etki sütununu `$7` ile okur; bu iki satırda kayar ama bugün sonuçlarını değiştirmezler (aday 11).

### 3.4 Çakışma taramasının notları

- **`04` 9.25.5 ↔ `07` UC-76 / UC-187 (K-1227):** üçü aynı hâlde (K3 mercek ve doğrulayıcı).
- **`06` §4.5, §4.47, §7.5.1 ↔ `07 §2.6.5` ↔ `09` (K-1228):** aynı hâlde.
- Aşama 1–5'in park satırları yeniden taranmadı; yalnız çakışma taramasının değiştirdiği satırlar (`08` K-906 · K-973, `09` iki madde, `12` dört etiket) P merceğinde okundu — bulgu yok, etiketler C-25'te hizalandı.

---

## 4. Kontrol 6 — Entegrasyon spesifikasyonları aşamasının girdileri

`00 §C.1`: Aşama 7 — Integration Engineer, girdi `02`, `03`, `05`, `06`, `07`, çıktı `08`, traceability zorunlu değil.

| Girdi | Durum |
|---|---|
| `02` v0.94, `03` v0.40, `05` v0.30, `06` v0.33 | ✓ Tamamlandı; bu checkpoint'in geri beslemeleri işlendi |
| `07` v0.25 | ⏳ → ✓ arşiv adımında (K-437'nin 6. adımı); checkpoint ve öğrenim terfisi sonrası |
| `08`'in park blokları | Aşama 1–6, altısı da var. Aşama 6 bloğu 4 madde (K-1166 · K-1169, K-1175, K-1185, K-1211 · K-1216 · K-1217); eksik girdi yok |
| Devir dizini | `PHASE6_CONFLICT_SCAN.md` §6 — `08` için 8 karar; `07`'nin gövdesinde sonraki dokümanı anan 56 cümle okundu (bu checkpoint'te `08`'e iş bırakan yeni cümle eklenmedi) |
| `07 §10` | Değişiklik kaydı boş açılır; `08` yazılırken geriye dönük eklenen uçlar buraya yazılır (şablonun beklentisi) — ödeme sağlayıcısından dönüşün yöntemi (UC-30) ilk aday |
| **Desteklenen ödeme sağlayıcısı sayısı** (çok kritik soru adayı) | Bu aşamanın sorusu değildir. Devir dizininde doğru yerdedir: `08`'in K-932 maddesi (Aşama 4 bloğu, satır 38) ve K-1112 (Aşama 5 bloğu, K-1059 maddesinin içinde, satır 46); Aşama 6 bloğunda yoktur. Kapısı Entegrasyon spesifikasyonları aşamasının açılışıdır |

Eksik girdi yok; `08` aşaması açılışta bu tabloyu ve çakışma taramasının §6'sını okur.

---

## 5. Proje sahibinin gözden geçirme listesi

### 5.1 Çakışma taramasının dokuz kararı — gösterilecek

> **Sonuç (2026-10-29, K-1231):** §5.1'in dokuz kararı ve §5.2'nin K-1229'u proje sahibine tek mesajda gösterildi; itiraz yok.

Kaynak: [çakışma taraması §4.1](PHASE6_CONFLICT_SCAN.md) — karar başına bir sade cümle (K-1161, K-1164, K-1185, K-1192, K-1199, K-1202, K-1210, K-1211, K-1213). Bu checkpoint o metinleri değiştirmedi.

### 5.2 Checkpoint'in ⚠ kararı — aynı mesajda gösterilir

Bu karar çakışma taramasının listesinin gösteriminden önce kaydedildi (`INSTRUCTIONS.md` §9 işletim kuralı 3). **Kapı:** arşiv işaretinden önce, tek mesajda, karar başına bir sade cümleyle ve gerekçesiz.

*Sipariş*
- K-1229 — Müşteri siparişi onayladığı anda sepetin toplamı firmanın koyduğu asgari sipariş tutarının altına düşmüşse sipariş oluşmaz; müşteri sepete döner ve eksik tutarı orada görür.

**Sayım:** çakışma taraması 9 · checkpoint 1 — **toplam 10**. Betikle sayıldı; konu hücresi ⚠ işaretini taşıyan Aşama 6 satırları on (K-1144 mod kararının kendi metnini anması hariç).

---

## 6. Öğrenim terfisine notlar — 5. adımın girdisi

Adayların metinleri tracker §16.1'dedir; bu checkpoint üç aday ekledi.

| # | Aday | Kaynak | Playbook |
|---|---|---|---|
| 10 | Bir mesajda yirmiden fazla alt ajan başlatılamaz — doğrulayıcılar en çok yirmişerli dalgalarla açılır | Eşzamanlı ajan sınırı (§Notlar) | — |
| 11 | Karar kaydının tablo hücresinde `|` her zaman `\|` yazılır; sütun sayısı betiği kaçışlıları çıkarır | C-27, §3.3 | `checklists/document-stage.md` §3 |
| 12 | Matris aday listesinin devir hâli olduğu `07 §1.1.2`'de yazılı değildi; dört mercek bulgusu bundan çıktı | C-26, E-3…E-5 | — (tek seferlik) |

**Kanıt — mevcut adaylara:** aday 7'nin ("deep review'un düzeltmesi gövdeye karar satırı olmadan kural ve devir yazabilir") sınıfı P merceğinde yeniden görüldü: dört park etiketinin K'sız uygulamaları deep review bulgularıdır (C-25); tracker'a K'sız kural yazılmadı, etiketlerin kaynağı bulgu kimliğiyle gösterildi. Aday 8'in ("geri işaret taraması kuralın değişmediği çiftleri okutuyor") benzeri bu checkpoint'te de çıktı: çürütülen sekiz bulgunun beşi "kuralın değişmediği" yüzlerdir (E-3…E-5, E-6, E-7). Aday 9'un ("park etiketi ↔ etki sütunu betiği etiketi okur, metni okumaz") iki yönlü betiği bu checkpoint'te de etiketi okudu; K-1204'ün eksik etkisi betikle, K-1217'nin etiket kaymasını doğrulayıcı buldu.

**Tek seferlik gözlem (raporda kalır):** 27 doğrulayıcının 61 şartname parçasının hiçbiri çakışmadı; aynı satıra iki doğrulayıcı dokundu (UC-83 İstek ve hata maddeleri: C-3, C-6, C-9) ve üç şartname birbirini bozmadı — ESKI parçalar satırın örtüşmeyen kesimleridir.

---

*Kaynak: K-437 (kapanış sırası, 4. adım) · K-429 (çakışma taraması — girdi) · K-1129 (cross-review'ın çıkış kuralı ve checkpoint'e devri) · K-652 (geri besleme) · K-29 (etki işaretleri) · K-647 (kaydın evi) · K-1144 (⚠ öneriyle kayıt) · K-1224 (bulgu doğrulaması Opus / high) · K-1229 (bu checkpoint'in kararı) · `.claude/skills/checkpoint/SKILL.md` · `.claude/skills/audit/SKILL.md` ("Koşum biçimi") · `Docs/CHECKPOINT_REPORTS/CP05_PHASE5_CHECKPOINT.md` (biçim örneği) · `Docs/CHECKPOINT_REPORTS/PHASE6_CONFLICT_SCAN.md` (girdi).*
