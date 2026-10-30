# Aşama 8 — Çakışma Taraması

**Tarih:** 2026-10-30 | **Aşama:** 8 — Kodlama Kılavuzu | **Kapanış adımı:** K-437'nin 3. adımı (K-429; `checklists/document-stage.md` §7)
**Girdi sürümleri:** karar kaydı v0.220 · Kodlama Kılavuzu (`09`) v0.22 · `05` v0.40 · `06` v0.42 · `02` v0.103 · `07`, `08`, `10` ve `11`'in Aşama 8 blokları · `12` ve `DEPLOY_RUNBOOK`'un park blokları
**Çıktı sürümleri:** karar kaydı v0.221 · `09` v0.23 · `02`, `05`, `06`, `07`, `08`, `10` değişmedi · `11`, `12` ve `DEPLOY_RUNBOOK`'un park blokları (sürüm alanı değişmez)

> **Ne işe yarar:** Aşama kapanmadan önce açık kalemlerin her birinin **tek bir evi** ve **gözlemlenebilir bir kapısı** olduğunu doğrular; vadesi geçmiş açıkları adıyla listeler (K-429). Önceki aşamaların park satırlarını ve bu aşamanın park satırlarını, kaynak kararlarını sonradan değiştiren kararlara karşı okur. Aşama 8 kararlarının (K-1305…K-1390) eski kararlara geri işaretlerini ve birbirleriyle zincirlerini denetler. Gösterilmemiş ⚠ kararlarının listesini karar kaydıyla eşitler (§4.1). Aşama 8 kararlarının sonraki dokümanlara devirlerini tek dizinde toplar (§6). Sonuç checkpoint'in (4. adım) girdisidir.

---

## 1. Sonuç

| Kural | Sonuç |
|---|---|
| **(1) Aynı açık iki yerde durmaz** | ✓ — açık karar yok. Tracker §4'ün on sekiz satırının hepsi kapalı; Aşama 8'de A-19 açılmadı. Bir çelişki karar istedi (§5.1, K-1392); başka açık karar doğmadı. |
| **(2) Her açık satır gözlemlenebilir bir kapı taşır** (K-37) | ✓ — tracker §20.1'in açık süreç maddelerinin hepsinde kapı yazılı (§4). |
| **(3) Vadesi geçmiş ve açık satır adıyla listelenir** | ⚠ — **bir madde:** "Girdilerin tarayarak okunan kısımları" (kapı: W konularının listesi) kapısı geçtiği hâlde açık kalmıştı; okumalar kayıtlıydı, madde kapatıldı (§3.3). Başka vadesi geçmiş açık yok. |

**Çakışmalar:** açık kalem, park satırı, geri işaret ve devir dizini taramaları betikle çıkarıldı; bulgu arayan okuma **23 Sonnet / low** okuyucusunda yapıldı (8 park, 11 geri işaret, 3 açık kalem, 1 devir) ve 35 aday çıkardı (doğrulayıcı yan gözlemleri dahil). Her aday bir **Opus / high** doğrulayıcısı tarafından güncel metne karşı okundu (11 doğrulayıcı, A–K; yüksek bulgu tek tek, orta ikili–dörtlü, düşük beşli–sekizli): **15 doğru, 8 kısmen, 12 çürütüldü**. Doğrulanan ve kısmen doğrulanan 23 bulgunun hepsi bu PR'da düzeltildi — **22'si yeni karar istemedi** (**K-1391**: eksik geri işaret, eksik devir maddesi, açık süreç maddesi düzeltmesi), **biri karar istedi** (§5.1): **K-1392** — disk doluluk eşiği kurulum ayarıdır, kodda sabit değildir (`(öneriyle kaydedildi — ⚠)`).

---

## 2. Yöntem

Tarama gözle değil betikle yapıldı (K-429); anlamsal okuma ayrı alt ajanlarda ve ön planda koştu. Ortam: Git Bash ve Python 3.12 (`python -I -X utf8`). **Model dağılımı (`INSTRUCTIONS.md` §9 (1)):** bulgu arayan okuyucular Sonnet / low (23); bulgu doğrulayıcıları Opus / high (11, kademeli); tarama betikleri, sayımlar ve düzeltmelerin işlenmesi yönetici adım ajanında. Her okuyucuya yalnız kendi dilimi ayrı bir dosyada verildi; karar kaydı ve `09` hiçbir ajana bütünüyle okutulmadı.

1. **Tracker §4 ve §20.1:** `| A-NN |` satırları ayrıştırıldı (18 satır, hepsi "Kapandı"); dosyada `- [ ]` ile açık duran süreç maddeleri çıkarıldı ve her birinde kapı arandı. Dokümanların açık kalem desenleri (`açık karar`, `açık kalem`, `belirlenecek`, `netleşecek`, `karara bağlanacak`, `TBD`, `(downstream)`, `sonra doldurulacak`, `A-19`, `açık soru`, `bekliyor`, `kapı:`) `01`…`12`'de arandı: **481 bağlam**, üç dilimde okundu.
2. **⚠ listesi:** K-1305…K-1392'nin konu hücresinde `⚠` taşıyan satırlar betikle çıkarıldı (**20**) ve tracker §20.1'in listesindeki `K-n` girişleriyle karşılaştırıldı.
3. **Park satırları:** `04`, `05`, `06`, `07`, `08`, `09`, `11`, `12` ve `DEPLOY_RUNBOOK` bloklarındaki `> - **…**` maddeleri ayrıştırıldı (**242 madde**); etiketlerinden kaynak K'lar çıkarıldı. Her kaynak için K-1305…K-1390'da tam eşleşme arandı: **164 eşleşme, 79 maddede**; sekiz dilimde okundu. Aşama 8 blokları için iki yönlü denetim (etiketteki karar hedefi etki sütununda anıyor mu; etki sütunu hedefi anan karar etikette ya da metinde geçiyor mu).
4. **Geri işaret ve zincirler:** K-1305…K-1390'ın konu, karar ve gerekçe hücrelerinde anılan eski K'lar çıkarıldı (**223 çift**); her çift için eski satırın etki sütununda yeni numara (ya da aralığı) aranıp geri işaret var / yok işaretlendi: **208 çiftte yok**. Çiftler 11 okuyucu diliminde okundu.
5. **Devir dizini:** etki sütunu ` · ` ile parçalandı; `11`, `12` ya da `DEPLOY_RUNBOOK`'u anan parçalar alındı ve kararın hedef dokümanın Aşama 8 bloğundaki yeri bulundu (**14 etki çifti**). `09`'un gövdesinde sonraki dokümanı numarasıyla ya da adıyla anan **31 cümle** çıkarıldı ve hedeflerin park blokları ile birlikte tek okuyucuya verildi.
6. **Doğrulama:** her aday bulgu için bir Opus / high ajanı güncel metne karşı okudu (`[dogrulama]`; A–K gruplarında kademeli); hüküm DOĞRU / KISMEN / ÇÜRÜTÜLDÜ ve gereken düzeltme dosyalara yazıldı. Düzeltmeler yönetici ajanında bir betikle yapıldı ve uygulanan metin eski metne karşı `assert` edildi. Doğrulayıcıların yan gözlemleri (grup J) ve açık süreç maddelerinin yinelenmesi (grup K) ayrı bulgu olarak yeniden doğrulandı; doğrulanmamış bulgu uygulanmadı.
7. **Kapanış taramaları:** karar kaydında 1.392 satır, çift K yok, altı sütun, boşluk yok; değişen her dosyada CR ve kontrol karakteri sıfır; eklenen satırlarda `$[0-9]` artığı sıfır (`check.py`).

Tarama betikleri ve okuyucu çıktıları oturumun scratchpad klasöründe kaldı (`scan1.py`…`scan5.py`, `out\park*`, `geri*`, `acik*`, `devir01`, `hukum_A…K`); repoya girmedi.

---

## 3. Açık kalemler

### 3.1 Tracker §4

| Kapsam | Satır | Açık | Sonuç |
|---|---|---|---|
| A-01…A-18 (Aşama 1, salt okunur) | 18 | 0 | Aşama 1'in taraması §3.1 |
| A-19 ve sonrası | 0 | 0 | Aşama 8'de açık detay doğmadı: workshop'un, yazımın ve kalite döngüsünün bulduğu her boşluk bir K satırıyla kapandı (K-1305…K-1390); bu tarama K-1391 ve K-1392 ile kapattı |

### 3.2 Dokümanlar

Desen eşleşmeleri (481 bağlam) tek okuyucu üç dilimde okundu: **açık kalem bulgusu 2 aday** — "yeniden gönderim `08`'de doğrulanır" atıfları (`05`, `07`) ve `DEPLOY_RUNBOOK` başlığındaki "Doldurulmayı bekliyor" durumu; ikisi de doğrulamada **çürütüldü** (O1: doğrulama `08 §4.1`'de DV-1 olarak kapısıyla duruyor; O2: şablon belgesinin doğal durumu, kapısı tracker'da yazılı). Diğer eşleşmeler kural cümleleri, siparişin açık kalemi terimi ve park bloklarının kalıp cümleleridir.

### 3.3 Vadesi geçmiş açık madde

**Bir madde:** "Girdilerin tarayarak okunan kısımları — kapı: W konularının listesi" (tracker §20.1). Kapısı geçmişti (workshop tamam, kalite döngüsü tamam), madde `[ ]` kalmıştı. Kayıtlı okumalar: W kararlarının konu hücrelerinde "(workshop; tarama: …)", yazım kararlarında "(yazım; tarama: …)" kalıbı; kalite döngüsü `09`'u üst doküman bölümleriyle betik paketlerinde okudu (`AUDIT_REPORTS/09_AUDIT.md`). Madde bu adımda kapatıldı (K-1391). Aşama 7'de aynı madde aynı gerekçeyle kapanmıştı.

### 3.4 Açık süreç maddelerinin yinelenmesi

`[ ]` işaretli satırlar tracker'da 9 yerde geçiyor; hepsi satır başı. Aşama 3'ün salt okunur listesindeki bir satır (Öğrenim adayı 13) ve Aşama 7'nin salt okunur listesindeki D-01 satırı arşivde `[ ]` kalır: arşiv salt okunur olduğundan işaret değiştirilemez, ardılında ya kapanmıştır (aday 13 → K-989) ya da yeniden açılmıştır (D-01 → §20.1). Bulgu M1 çürütüldü. M2 kısmen doğru çıktı: PR #216, Aşama 8'in D-01 cümlesini arşivdeki satıra yazmıştı; cümle §20.1'in D-01 maddesine taşındı, arşivdeki satır PR öncesi metnine döndü (K-1391).

---

## 4. Süreç maddeleri ve kapıları

Tracker §20.1'in işaretsiz maddeleri ve devir §19'dan taşınanlar tarandı. Kapısı yazılı olmayan madde yok.

| Madde | Ev | Kapı | Sonuç |
|---|---|---|---|
| Aşama kapanışının adımları (K-437) | Tracker §20.1 | Dört adım sırada: checkpoint, öğrenim terfisi, arşiv işareti ve devir | Kapı var; 3. adımın durumu maddeye yazıldı |
| ⚠ öneriyle kayıt listesi (K-1303) | Tracker §20.1 | Aşama 8'in arşiv işaretinden önce proje sahibine tek mesajda | **Kapı somutlaştı; liste eksikti:** konu hücresi ⚠ taşıyan 20 karardan 9'u sayılıyordu, 11 eklendi (20 / 20) — §4.1 |
| Ertelenmiş iş D-01 | `DEFERRED_BACKLOG.md` | İlk implementation task'ından önce | Kapı var; Aşama 8'in iki işi (K-1356, K-1358) §20.1'in maddesine taşındı (§3.4) |
| Girdilerin tarayarak okunan kısımları | Tracker §20.1 | W konularının listesi | **Kapandı** (§3.3) |
| Öğrenim terfisi adayları | Tracker §20.1 | Öğrenim terfisi (5. adım) | Kapı var; bu taramadan yeni aday yok (§7) |

Tracker'da yer alan, §20.1 dışı iki dış kapı (`PLAYBOOK_FEEDBACK.md` — proje tamamlandıktan sonra proje sahibi; `DEFERRED_BACKLOG.md` D-01) değişmedi.

### 4.1 ⚠ öneriyle kayıt listesi — doğrulama

> **Sonuç (2026-10-30, K-1400):** bu taramadaki liste 20 karardı; checkpoint iki karar ekledi (K-1394, K-1396) ve liste 22 oldu. Yirmi iki karar, K-1386'nın G1 kolu ve 'süreç' başlığındaki üç karar (K-1383, K-1385, K-1387) proje sahibine tek listede gösterildi; itiraz yok. Yirmi iki işaret `(öneriyle kaydedildi — ⚠ — gözden geçirildi 2026-10-30, itiraz yok)` oldu (betikle 22 / 22; K-1305'in işareti "işletim" ve "koşullu lisans sorusu AÇIK" değişkesiydi, aynı biçime getirildi; açıklama K-1310 ile eskimişti).

**Betikle doğrulama.** Konu hücresi `(öneriyle kaydedildi — ⚠)` taşıyan Aşama 8 satırı **20** (K-1305, K-1312, K-1317, K-1318, K-1326, K-1343, K-1351, K-1361, K-1363, K-1364, K-1365, K-1367, K-1368, K-1370, K-1372, K-1373, K-1374, K-1375, K-1390, K-1392); tracker §20.1'in listesindeki giriş bu taramadan önce **9** idi (workshop, deep review ve yazım turunun girişleri). **Fark 11 karar** listeye alındı: **20 / 20, eksik 0, fazla 0.**

**K-1386 notu.** K-1386 (cross-review 2. tur) konu hücresinde `⚠` taşımaz; yalnız karar hücresindeki kısmi kabul kolu (G1) ⚠ olarak işaretlidir. Betik konu hücresine baktığından listeye girmedi; kol, K-1374 ile aynı konuya (istisna iletisinin maskesi) dokunduğundan K-1374'ün ⚠ girişi altında gösterilir. Başka kol yok.

**Kapı.** Liste arşiv işaretinden (6. adım) önce proje sahibine **tek mesajda**, karar başına bir sade cümleyle ve gerekçesiz gösterilir (`INSTRUCTIONS.md` §2, "Ortak kural"); yöneticiden proje sahibine gider (§9). İtiraz gelmeyen satırın işareti `(öneriyle kaydedildi — ⚠ — gözden geçirildi YYYY-AA-GG, itiraz yok)` olur. Checkpoint ya da öğrenim terfisi yeni bir ⚠ karar kaydederse liste onunla birlikte gösterilir. Bu adımda işaret dönüştürülmedi; **gösterilmeden kaydedilen ⚠ karar** bu taramada yalnız K-1392'dir.

---

## 5. Çakışmalar

### 5.1 Karar isteyen çakışma — K-1392

| Yer | Çakışma | Karar |
|---|---|---|
| `09 §3.5` değer tablosu ve §15.7 madde 5 ↔ `05 §9.4.2`, `§8.4.5`, `§7.7.1` (grup J, S1) | `09`'un iki yeri disk doluluk eşiğini (%85) "kodda sabit" saydı (`DiskUsageGuard.ThresholdFraction`); Teknik Mimari durum ucunun eşiklerini kurulum ayar dosyasında değiştirilebilir sayar ve yükleme reddi durum ucunun disk eşiğini kullanır. İşletmeci eşiği ayarda değiştirse durum ucu yeni değerle, yükleme reddi eski %85 ile çalışırdı. Audit, deep review ve beş cross-review turu görmedi | **K-1392 (öneriyle kaydedildi — ⚠):** kaynak doküman kazanır (`09 §2` madde (1)); §3.5'te satır ikiye bölündü — parça boyutu ve başlatma gövdesi "kodda sabit", disk doluluk eşiği "kurulum ayarı (varsayılanı kodda)"; yükleme başlatmanın denetimi eşiği durum ucuyla aynı ayardan okur; ayarın adı `StatusThresholdOptions.DiskUsageFraction`, `DiskUsageGuard.ThresholdFraction` sabiti kalktı. `05` değişmedi. Elenen: `05`'i sabite çevirmek (durum ucunun öteki eşikleriyle tutarsız olur) |

### 5.2 Hizalanan çakışmalar — yeni karar istemeyen (K-1391)

Hepsi var olan bir kararın kuralının değişmeden kaldığı yüzlerin, devir dizininin ya da açık süreç maddelerinin hizalanmasıdır; ✓ durumları korunur, kalite döngüsü yeniden açılmaz (K-652). Karar ve gerekçe hücrelerine dokunulmadı.

| # | Yer | Eski hâl | Düzeltme | Şiddet |
|---|---|---|---|---|
| H1 | K-1232 ← K-1300 (1) (G1) | Bulgu başına bir doğrulayıcı kuralı; K-1300 kademeli toplu doğrulamaya geçti | K-1232'ye geri işaret | orta |
| H2 | K-1298 ← K-1301 (1) (G2) | "Toplam ~33 M" ölçümü eksik çıktı (gerçek 692,3 M) | K-1298'e geri işaret | düşük |
| H3 | K-969 ← K-1317 (G3, kısmen) | K-969 istisnası "satır diske inmeden çökme"; K-1317 okunuşu pencereyi işletim sistemi önbelleğine kadar genişletir | K-969'a "netleştirir" geri işareti (kural değişmedi) | düşük |
| H4 | K-1217 ← K-1321 (3) (G4) | `=,+,-,@` önekinin CSV'de genel uygulanışı; K-1321 yalnız metin sütunlarına daralttı | K-1217'ye daralma geri işareti | orta |
| H5 | K-1308 ← K-1332 (G5) | Dosya deposu işlemleri altı; `AppendAsync` ile yedi oldu | K-1308'e geri işaret | düşük |
| H6 | K-1333 ← K-1335 (12), K-1374 (G6, S3) | "İmha fonksiyonunun anı `09 §10`'da" — yeri §9.2 madde 6; "veritabanı zaman kaynağı değildir" kuralının tek istisnası eklendi | K-1333'e iki geri işaret | orta |
| H7 | K-1340 ← K-1343 (4) (G7) | Tek rol ayarı üçe çıktı | K-1340'a geri işaret; `DEPLOY_RUNBOOK` satır 69'a K-1343 atfı (P4) | orta |
| H8 | K-1334 ← K-1349 (6), K-1384 · K-1349 ← K-1384 (G8) | Geniş `catch` yerleri üçten beşe çıktı | K-1334'e ve K-1349'a geri işaret (düzeltme adaydan geniş) | orta |
| H9 | K-1328 ← K-1362 (1) (G10) | İstemci süresi 20 sn her profilde geçerli değil; bakım profilinde 10 dk 5 sn | K-1328'e geri işaret; `09` Kaynak atfına K-1362 (1) (v0.23, kural değişmedi) | orta |
| H10 | K-1337 ← K-1341 (G11, kısmen) | Etki sütunu "geri besleme yok" diyordu; K-1341 K-1337'ye işaret ekliyor | Yanlış ibare düzeltildi, geri işaret eklendi | düşük |
| H11 | `11` bloğu ← K-1354 (D1) | K-1354'ün etki sütunu `11`'e iş bırakıyor; blokta madde yoktu | `11` bloğuna K-1354 maddesi (yükleme betiği, kayıt biçimleyicisi, maskeleme testleri) | orta |
| H12 | `DEPLOY_RUNBOOK` K-1352 maddesi ← K-1361 (D2, kısmen) | Beşinci zorunlu ayar (türetme sırrı) blokta yoktu | K-1352 maddesine K-1361 ve `§A` satırı cümlesi; park işareti | düşük |
| H13 | `12` ← `09` gövdesi (D7…D9, I grubu) | `12`'nin "Aşama 8'den park edilen girdiler" bloğu yoktu; `09` ön yüzün tarayıcıda doğrulanmasını ve çatının kendi yanıtlarını `12`'ye bırakıyor | Blok açıldı, iki madde: ön yüzün tarayıcıda doğrulanması (K-1325, K-1354, K-1355); çatının kendi yanıtları (K-1334) | orta |
| H14 | K-1355, K-1357 → `11` etki sütunu (S2) | `11` bloğunda etiketleri var, etki sütunları `11`'i anmıyor | İki etki sütununa `11` parçası; `12` etki işareti K-1325, K-1334, K-1354, K-1355'e | düşük |
| H15 | Tracker §20.1 — "Girdilerin tarayarak okunan kısımları" (M3) | Kapısı geçti, madde açık | Madde kapatıldı (§3.3) | orta |
| H16 | Tracker §20.1 / Aşama 7 salt okunur listesi — D-01 (M2) | PR #216, Aşama 8'in D-01 cümlesini arşivdeki satıra yazmıştı | Cümle §20.1'e taşındı, arşivdeki satır PR öncesi metnine döndü | düşük |
| H17 | Tracker §20.1 — ⚠ listesi (M4, kısmen) | 9 / 20 | 20 / 20 (§4.1); "Aşama kapanışının adımları" maddesine 3. adımın durumu | orta |

### 5.3 Park satırları

**(a) Aşama 1–7'nin park satırları — numaraya dayalı tarama.** 242 madde; K-1305…K-1390'da kaynak K'ların **164 eşleşmesi** (79 maddede) sekiz dilimde okundu. Okuyucular 4 aday çıkardı; doğrulama sonucu:

| # | Park satırı | Değiştiren karar | Hüküm |
|---|---|---|---|
| P1 | `09` Aşama 4 (K-866 · K-867 · K-1310 · K-1312 — koşullu lisanslı kütüphane) | K-1305 (6) | **Çürütüldü** — K-1310 K-1305 (6)'yı zaten kaldırdı; park satırı talimattır, K-1316 (6), K-1329 ve K-1362 (3) ile tutarlı |
| P2 | `09` Aşama 4 ("Hata yakalama ve kayıt"; "diske inmesi beklenir") | K-1317 | **Çürütüldü** — K-1317 okunuşu `05 §7.4.5`'e geri beslendi; park satırı çelişmiyor |
| P3 | `09` Aşama 7 (K-1027 · K-1271 — değişken metnin kaçışı) | K-1323 | **Çürütüldü** — park talimattır; K-1323 kararın nasıl uygulandığıdır |
| P4 | `DEPLOY_RUNBOOK` Aşama 8, satır 69 (K-1340 — tek rol ayarı) | K-1343 (4) | **Kısmen** — satır tek başına okununca eksik; K-1343 atfı eklendi (H7) |

Geri kalan 160 eşleşme yalnız atıf, "değişmez" ya da uygulanış kaydıdır.

**(b) Aşama 8'in park satırları (iki yönlü betik).** `09` yok (kaynak doküman) · `11` · `12` · `DEPLOY_RUNBOOK`. Etiketteki her kararın etki sütunu hedefi anıyor mu ve etki sütunu hedefi anan karar etikette ya da metinde geçiyor mu: ilk koşumda `11` için K-1354, K-1355, K-1357; `12` için K-1325, K-1334, K-1354, K-1355; `DEPLOY_RUNBOOK` için K-1361. Doğrulama: hepsi H11…H14 olarak işlendi; son koşum temiz.

### 5.4 Geri işaret, zincirler ve öncüller

223 çift; eski satırın etki sütununda yeni numara **olmayan** çiftler: 208. Okuyucular 11 dilimde **11 aday satır** çıkardı; doğrulama: **8 doğru, 2 kısmen, 1 çürütüldü** (G9: K-1359 hiçbir kural koymadı, kayıt K-1362 (1) tarafından karara bağlandı; G10'a katıldı). Düzeltmeler H1…H10.

| Çift | İlişki |
|---|---|
| K-1232 → K-1300; K-1298 → K-1301 | kural kademeli doğrulamaya geçti; ölçüm düzeltildi |
| K-969 → K-1317; K-1217 → K-1321; K-1308 → K-1332 | netleştirir / daraltır / genişletir; kuralın değişikliği yeni satırda açık, eski satırda işaret yoktu |
| K-1333 → K-1335, K-1374; K-1334 → K-1349, K-1384; K-1340 → K-1343 | genişletir / daraltır; zincirler `09`'un gövdesinde son hâlini söylüyor |
| K-1328 → K-1362; K-1337 → K-1341 | bakım profilinin süresi; eski satırın yanlış "geri besleme yok" ibaresi |

**Zincirler.** İstisna iletisinin maskesi zinciri (K-1349 → K-1374 → K-1384 → K-1386) ve süre zinciri (K-1328 → K-1359 → K-1362) `09 §3.5`, §7.1 ve §10.7'de son hâlini söylüyor; eksik olan yalnız kayıttaki geri işaretlerdi.

### 5.5 Betikle aranan ve temiz dönen çakışma sınıfları

| # | Sınıf | Sonuç |
|---|---|---|
| 1 | **⚠ listesi ↔ kayıt** | ✓ düzeltildi — 9 / 20 → 20 / 20 (§4.1) |
| 2 | **K numarası tekilliği** — çift K taraması | ✓ — boş; K-01…K-1392, boşluk yok |
| 3 | **Karar satırının biçimi** — altı sütun | ✓ — 1.392 satırın hepsi |
| 4 | **Park etiketi ↔ etki sütunu** (Aşama 8 blokları, iki yön) | ✓ — son koşum temiz |
| 5 | **Tracker §4** | ✓ — 18 / 18 kapalı |
| 6 | **Aşama 8 etki sütunlarında yalnız doküman numarası taşıyan devir atfı** | ✓ — 14 etki çifti; hepsinin işi hedefin park bloğunda adıyla bulundu (§6.1) |
| 7 | **Değişen dosyalarda CR, kontrol karakteri ve `$[0-9]` artığı** | ✓ — sıfır (PR öncesi üç tarama; hafıza dosyaları dahil) |

### 5.6 Bağımsız doğrulama (Opus / high, A–K gruplar)

| # | Aday | Hüküm | Sonuç |
|---|---|---|---|
| P1 | `09` park (K-1310) ↔ K-1305 (6) | Çürütüldü | K-1310 zaten değiştirdi |
| P2 | `09` park ("diske inmesi") ↔ K-1317 | Çürütüldü | Geri besleme yapıldı |
| P3 | `09` park (K-1271) ↔ K-1323 | Çürütüldü | Talimat |
| P4 | `DEPLOY_RUNBOOK` satır 69 ↔ K-1343 (4) | Kısmen | Atıf eklendi |
| G1 | K-1232 ← K-1300 | Doğru | Geri işaret |
| G2 | K-1298 ← K-1301 | Doğru | Geri işaret |
| G3 | K-969 ← K-1317 | Kısmen | "Netleştirir" geri işareti |
| G4 | K-1217 ← K-1321 | Doğru | Geri işaret |
| G5 | K-1308 ← K-1332 | Doğru | Geri işaret |
| G6 | K-1333 ← K-1335 | Doğru | Geri işaret |
| G7 | K-1340 ← K-1343 | Doğru | Geri işaret |
| G8 | K-1334 ← K-1349 | Doğru | Geri işaret (düzeltme adaydan geniş) |
| G9 | K-1328 ← K-1359 | Çürütüldü | G10'a katıldı |
| G10 | K-1328 ← K-1362 | Doğru | Geri işaret |
| G11 | K-1337 ← K-1341 | Kısmen | Yanlış ibare düzeltildi |
| O1 | `05`, `07` "yeniden gönderim `08`'de doğrulanır" | Çürütüldü | DV-1 kapısıyla duruyor |
| O2 | `DEPLOY_RUNBOOK` başlık durumu | Çürütüldü | Şablonun doğal durumu |
| D1 | `11` ← K-1354 | Doğru | Yeni madde |
| D2 | `DEPLOY_RUNBOOK` ← K-1361 | Kısmen | K-1352 maddesine cümle |
| D3 | `DEPLOY_RUNBOOK` ← K-1389 | Çürütüldü | Maske listesi ve prova kaydı blokta var |
| D4 | `DEPLOY_RUNBOOK §C` ← `09` durum ucu eşikleri | Çürütüldü | Devrin evi `05 §8.4.5` |
| D5 | `DEPLOY_RUNBOOK` ← ters vekil listesi | Çürütüldü | Evi `07`; yan gözlem aşağıda |
| D6 | `DEPLOY_RUNBOOK` ← ayrıntısız kip ayarı | Çürütüldü | `05 §9.1.5` gövdesinde |
| D7 | `12` ← çatının kendi yanıtları | Kısmen | Bloğun 2. maddesi |
| D8 | `12` ← ön yüz tarayıcı davranış testi | Doğru | Bloğun 1. maddesi |
| D9 | `12` ← tarayıcıda uçtan uca yükleme | Kısmen | Bloğun 1. maddesine katıldı |
| D10 | `12` ← yük senaryosu | Çürütüldü | `12` gövdesinde karşılığı var |
| S1 | `09` disk eşiği "kodda sabit" ↔ `05 §9.4.2` | Doğru | **K-1392** |
| S2 | K-1355, K-1357 → `11` etki sütunu | Doğru | İki etki parçası |
| S3 | K-1333 ← K-1374 | Doğru | Geri işaret |
| S4 | `12` etki işareti K-1325, K-1334, K-1354, K-1355 | Doğru | Atıf düzeyinde, düşük önem |
| M1 | Tracker satır 1989 (§10.1, aday 13) | Çürütüldü | Ardılında kapandı (K-989); arşiv salt okunur |
| M2 | Tracker D-01 yinelenmesi | Kısmen | PR #216'nın cümlesi §20.1'e taşındı |
| M3 | "Girdilerin tarayarak okunan kısımları" | Doğru | Kapatıldı |
| M4 | Açık süreç maddeleri — kapılar | Kısmen | ⚠ listesi eşitlendi; adımlar maddesine durum yazıldı |

Toplam: **35 ayrı bulgu — 15 doğru, 8 kısmen, 12 çürütüldü.**

**Yan gözlemler (düzeltilmedi, karar istemez).** (a) D5 doğrulayıcısı: ters vekilin kapalı yol listesinin `Caddyfile`'da da güncelleneceği `DEPLOY_RUNBOOK`'ta yazılı değil; isteğe bağlı, kurulum belgesi aşamasında düşer. (b) `09` satır 17'de "tabloda ile" yazım kusuru var; bir sonraki etki yansıtmada düzeltilir (kural değişikliği değil).

---

## 6. Devir dizini — Aşama 8 kararlarının sonraki dokümanlara devirleri

**Tek ev:** devrin evi karar satırının etki sütunu ya da park satırıdır; bu dizin devri ikinci kez kaydetmez. Her hedef dokümanın "Aşama 8'den park edilen girdiler" bloğu bu bölüme işaret eder (`11`, `12`, `DEPLOY_RUNBOOK`).

- **Önceki aşamaların dizinleri:** `PHASE1_CONFLICT_SCAN.md` §6 … `PHASE7_CONFLICT_SCAN.md` §6.
- **`DEFERRED_BACKLOG.md`:** Aşama 8'den yeni kalem yok; D-01'in iki işi (K-1356, K-1358) §20.1'in maddesinde ve `11` bloğundadır.
- **Süreç kararları** (K-1302, K-1303, K-1358, K-1389, K-1390 gibi) sonraki dokümana kendi içeriği için iş bırakmaz; iş bırakan kollar aşağıda adlandırılmıştır.

### 6.1 Etki sütunlarındaki devirler (K-1305…K-1392)

| Hedef | Atıf = karar | Aşama 8 park bloğunda | Bu taramada eklenen | Park gerekmez |
|---|---|---|---|---|
| `11` Uygulama Planı | 6 | 5 | K-1354 maddesi; K-1355 ve K-1357 etki işareti | 0 |
| `12` Doğrulama Protokolü | 4 | 0 → 4 | **Yeni blok, iki madde** (K-1325 · K-1354 · K-1355; K-1334) | 0 |
| `DEPLOY_RUNBOOK.md` | 4 | 3 | K-1361 (K-1352 maddesine); K-1343 atfı (satır 69) | 1 (K-1389 — maske listesi ve prova kaydı blokta var) |

#### `11` Uygulama Planı

Bloğa K-1354 maddesi eklendi: yükleme betiğinin, kayıt biçimleyicisinin ve maskeleme testlerinin task'ları (`09 §16.1`, `§17`, `§18.1`, `§18.2`). K-1302 (4) · K-1357 (okunacak `09` bölümleri) ve K-1355 (sahtelerin paylaşımı) maddeleri zaten blokta olup etki sütunlarına `11` parçası eklendi.

#### `12` Doğrulama Protokolü

`12`'de Aşama 8 bloğu yoktu. K-36 mekanizmasıyla açıldı; iki madde:

1. **Ön yüzün tarayıcıda doğrulanması** (K-1325, K-1354, K-1355): tarayıcıda çalışan betiğin davranışı ve uçtan uca yükleme senaryosu (`09 §15.7`, `§16.1`).
2. **Çatının kendi yanıtları** (K-1334): 405, 416 ve işleyicisi bulunamayan POST katmana düşmez; davranış `12` senaryosuyla sabitlenir (`07 §2.7.1`).

Yük senaryosu (`05 §11.1`, `02 §11.3`) `12`'nin gövdesinde karşılık bulduğundan yeni madde açmadı (D10).

#### `DEPLOY_RUNBOOK.md`

Satır 69 (K-1340) K-1343 (4)'ün üç rol ayarına atıf aldı; K-1352 maddesi K-1361'in beşinci zorunlu ayarını (türetme sırrı) ve `§A` tablosundaki satırı cümlesini taşır. K-1389'un maske listesi ve prova kaydı satırları (`§I.3`) blokta adıyla var.

### 6.2 `09` gövdesinde sonraki dokümanı anan cümleler

Mekanik çıkarım: `09`'un gövdesinde (başlık notu, Kaynak satırları ve matris hariç) sonraki dokümanı numarasıyla ya da adıyla anan **31 cümle**. Bir okuyucu her cümlenin işi bırakıp bırakmadığını ve hedefin park bloğunda karşılık bulup bulmadığını okudu: aday D4…D10 (7 aday) — doğrulamada **3 gerçek iş** (D7, D8, D9; hepsi `12` bloğuna girdi), 4 çürütme (D4: devrin evi `05 §8.4.5`; D5: evi `07`; D6: `05 §9.1.5` gövdesinde; D10: `12`'de karşılığı var).

### 6.3 Doğrulama ve soru devirleri

`09`, dış dünyaya ait olguları kendi sınırında bırakır; implementation'a bırakılan doğrulamalar `12`'nin Aşama 8 bloğundadır. **Çok kritik soru adayı yoktur.**

---

## 7. Öğrenim terfisi adayları — 5. adımın girdisi

Bu taramadan **yeni aday çıkmadı**; Aşama 7'nin adayı 5 (⚠ listesinin betikle eşitlenmesi) Aşama 8'de de karşılığını buldu: liste 9 / 20 çıktı. Aday zaten tracker'dadır; terfi hedefi `checklists/document-stage.md` §7 ve `INSTRUCTIONS §2`.

**Ölçüm satırı (maliyet düzeni):** bu tarama 23 Sonnet / low okuyucu ve 11 Opus / high doğrulayıcı kullandı; 35 ayrı bulgu 11 doğrulayıcıya kademeli dağıtıldı (aday 4'ün kademeli toplu doğrulama düzeni, K-1300). Aşama 7'de bulgu başına bir doğrulayıcıydı (44 bulgu → 44 doğrulayıcı).

---

## 8. Checkpoint'e notlar

- Tracker §4'te açık satır yok, A-19 açılmadı; dokümanlarda açık kalem yok — checkpoint'in açık karar kontrolü bu raporla karşılanır.
- ⚠ listesi **20 karar** (§4.1) — arşiv işaretinden önce proje sahibine tek mesajda gösterilir (yönetici). Listeye bu taramanın K-1392'si dahildir.
- K-1391 ve K-1392 bu taramanın kararlarıdır; yüzleri §5'tedir. Checkpoint'in iç tutarlılık merceği `09 §3.5` değer tablosu ile `09 §15.7` madde 5'in ve `05 §9.4.2`'nin aynı hâlde olduğunu bir kez daha okuyabilir.
- **Devredilen sınıf** (K-1129): iki yerin metin farkı — özet hücresi ya da tablo ile uygulama bölümü. Bu taramanın bulgusu aynı sınıftadır: `09 §3.5` ile `§15.7`'nin "sabit" ibaresi.
- Park satırları K-1305…K-1390'a bir kez tarandı (164 eşleşme; 242 madde). Checkpoint yeniden taramaz; yalnız bu PR'ın değiştirdiği yerleri okur: `09` (§3.5 disk eşiği satırı, §15.7 madde 5, Kaynak atıfları), `11` (K-1354 maddesi), `12` (yeni blok), `DEPLOY_RUNBOOK` (satır 69, K-1352 maddesi).
- 6. adımda sonraki aşamaya (Aşama 9) devrin tablosu bu raporun §6'sını ve hedef dokümanların park bloklarını gösterir.
