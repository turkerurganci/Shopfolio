# Aşama 9 — Çakışma Taraması

**Tarih:** 2026-11-01 | **Aşama:** 9 — Implementation Planı | **Kapanış adımı:** K-437'nin 3. adımı (K-429; `checklists/document-stage.md` §7)
**Girdi sürümleri:** karar kaydı v0.280 · Implementation Planı (`11`) v0.52 · `02` v0.108 · `03` v0.46 · `04` v0.50 · `05` v0.43 · `06` v0.45 · `07` v0.37 · `08` v0.20 · `09` v0.28 · `10` v0.59 · `12` ve `DEPLOY_RUNBOOK`'un park blokları
**Çıktı sürümleri:** karar kaydı v0.281 · `11` değişmedi (v0.52) · `12` ve `DEPLOY_RUNBOOK` (sürüm alanı değişmez; park bloğu eklendi)

> **Ne işe yarar:** Aşama kapanmadan önce açık kalemlerin her birinin **tek bir evi** ve **gözlemlenebilir bir kapısı** olduğunu doğrular; vadesi geçmiş açıkları adıyla listeler (K-429). Önceki aşamaların park satırlarını, kaynak kararlarını sonradan değiştiren kararlara karşı okur. Aşama 9 kararlarının (K-1401…K-1493) eski kararlara geri işaretlerini denetler. Gösterilmemiş ⚠ kararlarının listesini karar kaydıyla eşitler (§4.1). Aşama 9 kararlarının sonraki dokümanlara devirlerini tek dizinde toplar (§6). Sonuç checkpoint'in (4. adım) girdisidir.

---

## 1. Sonuç

| Kural | Sonuç |
|---|---|
| **(1) Aynı açık iki yerde durmaz** | ✓ — açık karar yok. Tracker §4'ün on sekiz satırının hepsi kapalı; Aşama 9'da A-19 açılmadı. Dokümanlarda tracker'la çift duran açık yok. |
| **(2) Her açık satır gözlemlenebilir bir kapı taşır** (K-37) | ✓ — §22.1'in açık dört maddesinden biri ("Aşama kapanışının adımları") kapısızdı; **kapı yazıldı** (§4). |
| **(3) Vadesi geçmiş ve açık satır adıyla listelenir** | ✓ — vadesi geçmiş açık yok (§3.3). |

**Çakışmalar:** açık kalem, park satırı, geri işaret ve devir taramaları betikle çıkarıldı; bulgu arayan okuma **8 + 3 Sonnet / low** okuyucuda yapıldı (4 açık kalem, 4 park, 3 geri işaret) ve **20 aday** çıktı (geri işaret 16, devir 4). Her aday bir **Opus / high** doğrulayıcısı tarafından güncel metne karşı okundu (**4 doğrulayıcı**; adım başına sınır 20): **6 doğru, 2 kısmen, 12 çürütüldü**. Doğrulanan ve kısmen doğrulanan 8 bulgunun hepsi bu PR'da düzeltildi — **yeni karar istenmedi** (**K-1494**): altı geri işaret (K-1406, K-1407, K-1409, K-1410, K-508, K-509), `12` için üç ve `DEPLOY_RUNBOOK` için bir devir maddesi. ⚠ yeni karar yok.

---

## 2. Yöntem

Tarama gözle değil betikle yapıldı (K-429); anlamsal okuma ayrı alt ajanlarda ve ön planda koştu. Ortam: Git Bash ve Python 3.12 (`python -I -X utf8`). **Model dağılımı (`INSTRUCTIONS.md` §9 (1)):** bulgu arayan okuyucular Sonnet / low (11); bulgu doğrulayıcıları Opus / high (4, kademeli); tarama betikleri, sayımlar ve düzeltmelerin işlenmesi yönetici adım ajanında. Karar kaydı ve `11` hiçbir ajana bütünüyle okutulmadı.

1. **Tracker §4 ve §22.1:** `| A-NN |` satırları ayrıştırıldı (18 satır, hepsi "Kapandı"); `- [ ]` ile açık duran süreç maddeleri çıkarıldı ve her birinde kapı arandı. Dokümanların açık kalem desenleri (`açık karar`, `açık kalem`, `belirlenecek`, `netleşecek`, `karara bağlanacak`, `TBD`, `(downstream)`, `sonra doldurulacak`, `A-19`, `açık soru`, `bekliyor`, `kapı:`) `00`…`12`, `DEPLOY_RUNBOOK`, `DEFERRED_BACKLOG` ve `CI_CD_SETUP`'ta arandı: **458 bağlam**, dört dilimde (02+03 · 04+06 · 07+08 · geri kalan) okundu.
2. **⚠ listesi:** K-1401…K-1493'ün konu hücresinde `⚠` taşıyan satırlar betikle çıkarıldı (**8**) ve proje sahibine gösterilen ⚠ kararların eşleme dosyasıyla (`onerilen-uyarili.md`) ve tracker §22.1'in listesiyle karşılaştırıldı.
3. **Park satırları:** `04`…`12`, `DEPLOY_RUNBOOK` ve `DEFERRED_BACKLOG` bloklarındaki `> - **…**` maddeleri ayrıştırıldı (**253 madde**); etiketlerinden kaynak K'lar çıkarıldı (etiketinde K olmayan 4 maddede gövdeden). Her kaynak için K-1401…K-1493'te tam eşleşme arandı: **285 eşleşme, 59 maddede**; dört dilimde okundu.
4. **Geri işaret:** K-1401…K-1493'ün konu, karar ve gerekçe hücrelerinde anılan eski K'lar çıkarıldı; eski satırın etki sütununda yeni numara ya da aralığı olmayan **359 çift** (83 yeni karar) üç dilimde okundu. Ek olarak `geri_isaret_tara.py 1401 1493` (ilişki sözcüğü penceresi: 19 çift, 8 eksik) koştu ve çıktısı ayrı doğrulandı.
5. **Devir dizini:** Aşama 9 kararlarının etki sütunları `12`, `DEPLOY_RUNBOOK`, `DEFERRED_BACKLOG`, `CI_CD_SETUP` ve D-01 için tarandı (2 karar: K-1407, K-1467); `11`'in gövdesinde (§7 matrisi hariç) `12`, `DEPLOY_RUNBOOK`, `DEFERRED_BACKLOG`, `CI_CD_SETUP` ve "ileri işaret" anan **29 satır** çıkarıldı.
6. **Doğrulama:** her aday bir Opus / high doğrulayıcıda güncel metne karşı okundu; hüküm DOĞRU / KISMEN / ÇÜRÜTÜLDÜ ve gereken düzeltme dosyalara yazıldı. Düzeltmeler bir betikle yapıldı ve uygulanan metin eski metne karşı `assert` edildi.
7. **Kapanış taramaları:** karar kaydında 1.494 satır, çift K yok, boşluk yok; değişen dosyalarda CR, kontrol karakteri ve `$[0-9]` artığı sıfır. Kapılar: `11_matris_kapsama.py` sorun 0, `11_task_blok_denetle.py --tam` sorun 0, `11_yazim_kapanis.py` sorun 0, `11_geri_yon.py . --denetle` eşit (214 / 214), `geri_isaret_tara.py 1401 1494` — kalan satırlar §5.2'de adıyla.

Tarama betikleri ve okuyucu çıktıları yönetici yardımcısının çalışma dizininde kaldı; repoya girmedi.

---

## 3. Açık kalemler

### 3.1 Tracker §4

| Kapsam | Satır | Açık | Sonuç |
|---|---|---|---|
| A-01…A-18 (Aşama 1, salt okunur) | 18 | 0 | Aşama 1'in taramasında kapalı |
| A-19 ve sonrası | 0 | 0 | Aşama 9'da açık detay doğmadı: workshop'un, matrisin, yazımın ve kalite döngüsünün bulduğu her boşluk bir K satırıyla kapandı (K-1401…K-1493) |

### 3.2 Dokümanlar

458 desen bağlamı dört okuyucuda okundu: **açık kalem bulgusu 0 aday.** Eşleşmeler "açık kalem" sipariş kalemi terimi, "Bekliyor" ödeme durumu, kural cümleleri ("açık kalem A-19'dan sürer"), park bloklarının kalıp cümleleri, `08`'in DV-1…DV-43 doğrulama defteri satırları (kapısı her satırda yazılı) ve `DEPLOY_RUNBOOK` başlığındaki "Doldurulmayı bekliyor" şablon durumudur. `11`'de iki "Bekliyor" satırı (DV-5 — T510) kapısıyla duruyor.

### 3.3 Vadesi geçmiş açık madde

Yok. Tracker §22.1'in açık dört maddesinin kapıları geçmedi: D-01 (ilk implementation task'ından önce) · yardımcı düzeninin ölçümü (Aşama 9'un öğrenim terfisi) · ⚠ listesi (arşiv işaretinden önce) · aşama kapanışının adımları (§4).

### 3.4 Açık süreç maddelerinin yinelenmesi

`[ ]` işaretli satırlar tracker'da 7 yerde geçiyor: Aşama 3'ün salt okunur listesindeki Öğrenim adayı 13 (ardılında K-989 ile kapandı; arşiv işareti değiştirilemez) ve Aşama 7 ile Aşama 8'in D-01 satırları (arşivde `[ ]` kalır, ardılı §22.1'in D-01 maddesidir) — Aşama 8 taramasının M1/M2 hükümleriyle aynı; ve §22.1'in dört maddesi. Yeni yineleme yok.

---

## 4. Süreç maddeleri ve kapıları

| Madde | Ev | Kapı | Sonuç |
|---|---|---|---|
| Aşama kapanışının adımları (K-437) | Tracker §22.1 | **Yoktu — yazıldı:** dört adım bu sırada, her adım kendi `docs:` PR'ıyla; arşiv işareti öğrenim terfisinden sonra | Kapı eklendi; maddeye 3. adımın durumu (✓, K-1494) yazıldı |
| ⚠ öneriyle kayıt listesi | Tracker §22.1 | Aşama 9'un arşiv işaretinden önce proje sahibine tek mesajda | Kapı var; liste eksikti: 8 satırdan 7'si eşleme dosyasındaydı, tracker'ın maddesi yalnız ilk üçü anıyordu (§4.1) |
| Ertelenmiş iş D-01 | `DEFERRED_BACKLOG.md` | İlk implementation task'ından önce | Kapı var; task karşılığı F0'dadır |
| Yardımcı düzeninin ölçümü (K-1402) | Tracker §22.1 | Aşama 9'un öğrenim terfisi (`PHASE9_LEARNING_PROMOTION.md` §3) | Kapı var; ölçüm 5. adımın işi |

Tracker'da yer alan, §22.1 dışı iki dış kapı (`PLAYBOOK_FEEDBACK.md` — proje tamamlandıktan sonra proje sahibi; `DEFERRED_BACKLOG.md` D-01) değişmedi.

### 4.1 ⚠ öneriyle kayıt listesi — betikle eşitleme

**Betikle doğrulama.** Konu hücresi `⚠` taşıyan Aşama 9 satırı **8**: K-1411, K-1413, K-1432, K-1458, K-1463, K-1474, K-1476, K-1483. Eşleme dosyasında (`onerilen-uyarili.md`) giriş **7** idi; **eksik: K-1463** (dört ⚠ kararın — K-1411, K-1413, K-1432, K-1458 — proje sahibine gösterildiği kayıt; karar değil gösterim kaydı, ama konu hücresi ⚠'yı anıyor). Dosyaya eklendi: **8 / 8, eksik 0, fazla 0.** Tracker §22.1'in listesi yalnız açılış ve matris oturumlarının girişlerini (K-1411, K-1413, K-1432) taşıyordu; kalite döngüsü girişleri (K-1458, K-1463, K-1474, K-1476, K-1483) maddeye yazıldı.

**Gösterim durumu.** K-1411, K-1413, K-1432, K-1458 proje sahibine 2026-10-31'de gösterildi ve onaylandı (K-1463) — yeniden sorulmaz, dosyadaki işaret korundu. K-1474, K-1476, K-1483 toplu onayla (K-1472) kaydedildi ve kapanışta tek listede gösterilir. Bu tarama yeni ⚠ karar kaydetmedi.

---

## 5. Çakışmalar

### 5.1 Karar isteyen çakışma

Yok. K-1494 yeni karar istemedi; ⚠ yok.

### 5.2 Geri işaret — hizalanan çakışmalar (K-1494)

Hepsi var olan bir kararın kuralının değişmeden kaldığı yüzlerin, eski satırın etki sütununun ve devir dizininin hizalanmasıdır; ✓ durumları korunur, kalite döngüsü yeniden açılmaz (K-652). Karar ve gerekçe hücrelerine dokunulmadı.

| # | Yer | Eski hâl | Düzeltme | Şiddet |
|---|---|---|---|---|
| H1 | K-1406 ← K-1413 | K-1406 (3) task kimliğini "ardışık, faz aralıklarıyla" diye koyar; K-1413 (1)–(2) kimliği faz içi sıra gözetmeden verir ve kendi satırında "daraltır" der | K-1406'ya geri işaret | düşük |
| H2 | K-1410 ← K-1413 | K-1410 (1) "faz içi sıra modül akış sırasıdır" der; K-1413 (4) F1'in sırasını bağların kurduğu sırayla daraltır (göç bağlandığı tabloların göçünden sonra) | K-1410'a geri işaret | orta |
| H3 | K-1409 ← K-1413 | K-1409 (2) "planlarken ardışık yeni numara" der; K-1413 (1)–(2) bunu daraltır | K-1409'a geri işaret | düşük |
| H4 | K-1407 ← K-1432 | K-1407 (3)'ün F0 ve F6 içerik listesi; K-1432 yedek düzeni (T07), sır düzeni (T08) ve güncelleme komutu (T602) ekler, kendi satırında ⚠ ile "içerik sınırını değiştirir" der | K-1407'ye geri işaret | düşük |
| H5 | K-508 ← K-1483 | "Çabuk bozulan" malda referans hemen önceki fiyat; K-1483 (4) istisnayı yalnız çabuk bozulabilen mal ve hizmete bağlar, son kullanma tarihi sebepli mal 10 gün kuralındadır | K-508'e geri işaret (daraltır) | orta |
| H6 | K-509 ← K-1483 | K-509'un (c) bendi tek değer; K-1483 (4) iki değere böler (`Perishable`, `ExpiryDated`) | K-509'a geri işaret (liste değişir) | orta |

**Kalan `geri_isaret_tara.py` satırları (9).** Yedisi doğrulamada çürütüldü (§5.6: K-1415 ← K-1409, K-1419 ← K-1409, K-1424 ← K-1417, K-1427 / K-1430 / K-1433 ← K-1424, K-1436 ← K-1433 — hepsi eski kararın kuralını uygulayan ya da "kalıbı" anan atıf); ikisi K-1494'ün kendi atıflarıdır (K-1494 → K-1413, K-1494 → K-1463: bu karar o satırların kuralını değiştirmez). Betik ilişki sözcüğü penceresine dayandığı için çıkış kodu 1 kalır; her satır okunmuş ve gerekçesiyle kapanmıştır (Aşama 8 raporunda aynı durum).

### 5.3 Park satırları

**(a) Aşama 1–8'in park satırları — numaraya dayalı tarama.** 253 madde; K-1401…K-1493'te kaynak K'ların **285 eşleşmesi** (59 maddede) dört dilimde okundu. Okuyucular **0 aday** çıkardı. Eşleşmelerin dağılımı: (i) `05`'in Aşama 5 bloğunun yedi maddesi (K-652 geri besleme mekanizması) ← K-1460…K-1477'nin "Üst doküman (K-652)" kalıbı — mekanizmanın uygulanışı, kural değişmedi; (ii) `12`'nin ve `11`'in ilgili blokları ← cross-review turlarının "tekrar eden RET" atıfları (K-900, K-914, K-940, K-978, K-985…); (iii) `OK-1…OK-12`, `MK-12` gibi "K-n" gibi görünen ama karar atfı olmayan metinler (sahte eşleşme); (iv) K-1407 ← K-932: K-932'nin elenen seçeneğinin yalnız kodun modül düzeni için olduğu açıklanır, faz sırasını bağlamaz — kapsam açıklaması.

**(b) Aşama 9'un park satırları.** `11`'in "Aşama 8'den park edilen girdiler" bloğu beş madde taşır; her birinin task karşılığı "Aşama 8 park bloğunun durumu" tablosunda (P8.1…P8.5) yazılı ve `11_yazim_kapanis.py` B taraması geçiyor. Aşama 9 için hedef dokümanlarda park bloğu yoktu (§6'da açıldı).

### 5.4 Geri işaret ve zincirler

359 çift; eski satırın etki sütununda yeni numara olmayan çiftler okuyucuların üç diliminde incelendi: **7 aday** (3 + 3 + 1 zayıf); betiğin 8 eksik çiftiyle birlikte 16 ayrı geri işaret adayı doğrulayıcılara gitti (§5.6). Okuyucuların atıf olarak elediği çiftler: gerekçe alıntıları ("tekrar eden RET" listesindeki eski kararlar), uygulanış kayıtları ("K-652 geri beslemesi"), envanter ve sayımlar.

| Çift | İlişki |
|---|---|
| K-1406 ← K-1413; K-1409 ← K-1413; K-1410 ← K-1413 | K-1413'ün tek karar satırı üç eski satırın lafzını daraltır; eski satırlarda işaret yoktu |
| K-1407 ← K-1432 | iskelet fazının ve son fazın içerik listesi genişledi |
| K-508, K-509 ← K-1483 | m.15/1-(c) iki sebebe bölündü; iki eski satır |

### 5.5 Betikle aranan ve temiz dönen çakışma sınıfları

| # | Sınıf | Sonuç |
|---|---|---|
| 1 | **⚠ listesi ↔ kayıt** | ✓ düzeltildi — 7 / 8 → 8 / 8 (§4.1) |
| 2 | **K numarası tekilliği** — çift K taraması | ✓ — boş; K-01…K-1494, boşluk yok |
| 3 | **Tracker §4** | ✓ — 18 / 18 kapalı |
| 4 | **Park etiketi ↔ etki sütunu** (`11`'in Aşama 8 bloğu) | ✓ — P8.1…P8.5 karşılandı |
| 5 | **`11` kapıları** — matris kapsama, task blok denetimi, yazım kapanışı, geri yön | ✓ — sorun 0 / 214 = 214 |
| 6 | **Değişen dosyalarda CR, kontrol karakteri ve `$[0-9]` artığı** | ✓ — sıfır |

### 5.6 Bağımsız doğrulama (Opus / high, 4 doğrulayıcı)

| # | Aday | Hüküm | Sonuç |
|---|---|---|---|
| G1 | K-1406 ← K-1413 | Doğru | Geri işaret (H1) |
| G2 | K-1410 ← K-1413 | Doğru | Geri işaret (H2) |
| G3 | K-1407 ← K-1432 | Doğru | Geri işaret (H4) |
| G4 | K-508 ← K-1483 | Doğru | Geri işaret (H5) |
| G5 | K-509 ← K-1483 | Doğru | Geri işaret (H6) |
| G6 | K-1409 ← K-1413 | Kısmen | Geri işaret (H3, düşük); doğrulayıcı aynı kalıbı ters yönde de gördü (aşağıdaki yan gözlem) |
| G7 | K-1491 → K-1487 | Çürütüldü | K-1487'de kural yok, T370 KK7 uygulama eksiği tamamlandı |
| G8 | K-1489 → K-1481 | Çürütüldü | Yeni mercek alt sınıfı K-1481'den ayrılır; K-1481 daralmadı |
| G9 | K-1413 → K-1407 | Çürütüldü | Faz aralığı K-1407'den gelir — uygulanış |
| G10…G16 | K-1415, K-1419 ← K-1409 · K-1424 ← K-1417 · K-1427, K-1430, K-1433 ← K-1424 · K-1436 ← K-1433 | Çürütüldü (7) | Uygulama, kalıp atfı ve gerekçe alıntısı |
| D1 | `12` ← `11` "ileri işaret: 12" cümleleri | Kısmen | 7 alt maddenin 3'ü gerçek iş: vitrin eksik satır, erişilebilirlik taraması, F6 kanıt bağlanışı → `12` bloğu |
| D2 | `DEPLOY_RUNBOOK` ← CSV'nin şifreli iletimi ve silinmesi | Çürütüldü | Aşama 4 bloğunda zaten yazılı |
| D3 | `DEPLOY_RUNBOOK` ← SES IAM politikası ve hesap düzeyi liste | Çürütüldü | Aşama 7 bloğunda zaten yazılı; `AMAZON_SES.md` ayrı belge |
| D4 | `DEPLOY_RUNBOOK §I` ← K-1407 (F0 ve F6 provası) | Doğru | Aşama 9 bloğunda boot provasının iki turu |

Toplam: **20 ayrı aday — 6 doğru, 2 kısmen, 12 çürütüldü.**

**Yan gözlemler (düzeltilmedi, karar istemez).** (a) K-1413'ün kendi etki sütunu K-1407'yi "(daraltır)" diye anıyor; doğrulayıcı bunu fazla iddia buldu (K-1407'ye yalnız faz aralığı uygulandı) — okuyucuyu yanıltmaz, dokunulmadı. (b) Betik `geri_isaret_tara.py`, gerekçe hücresindeki "X'in lafzını daraltır / tamamlar" kalıplarını (H1…H3, H5, H6) ilişki sözcüğü penceresinin dışında bıraktı; tarama bu yüzden tüm eski K anmalarını kapsayan ikinci bir geçişle (359 çift) tamamlandı (§7).

---

## 6. Devir dizini — Aşama 9 kararlarının sonraki dokümanlara devirleri

**Tek ev:** devrin evi karar satırının etki sütunu ya da hedef dokümanın park satırıdır; bu dizin devri ikinci kez kaydetmez. Aşama 9'un çıktısı `11` son doküman değildir: sırada `12` (Doğrulama Protokolü, Aşama 10) vardır; `DEPLOY_RUNBOOK` implementation sırasında doldurulur.

- **Önceki aşamaların dizinleri:** `PHASE1_CONFLICT_SCAN.md` §6 … `PHASE8_CONFLICT_SCAN.md` §6.
- **`DEFERRED_BACKLOG.md`:** Aşama 9'dan yeni kalem yok; D-01'in task karşılığı F0'dadır (T02–T07, T09).
- **`CI_CD_SETUP.md`:** Aşama 9 kararı anmıyor.
- **Süreç kararları** (K-1401, K-1402, K-1403, K-1443, K-1472, K-1484 gibi) sonraki dokümana iş bırakmaz.

### 6.1 Etki sütunlarındaki ve gövdedeki devirler (K-1401…K-1493)

| Hedef | Atıf | Aşama 9 park bloğunda | Bu taramada eklenen | Park gerekmez |
|---|---|---|---|---|
| `12` Doğrulama Protokolü | 0 karar; `11` gövdesinde 11 "ileri işaret: 12" satırı | 0 → 3 | **Yeni blok, üç madde** (K-755 · K-841; K-625 · K-1411; K-23 · K-1453) | 4 alt madde (sepet elli kalem — Aşama 5 bloğunda; logo — Aşama 3 bloğunda; `E2E` kapsamı — Aşama 8 bloğunda; madde başına çeviri — `12` §5 giriş kuralı 3) |
| `DEPLOY_RUNBOOK.md` | 2 karar (K-1407, K-1467) | 0 → 1 | **Yeni blok, bir madde** (K-1407 · K-1411 · K-1453: boot provasının iki turu); K-1467 başlık notu K-1467 kendi PR'ında uygulandı | 2 (CSV iletimi — Aşama 4; SES politikası ve hesap düzeyi liste — Aşama 7) |

#### `12` Doğrulama Protokolü

"Aşama 9'dan park edilen girdiler" bloğu K-36 mekanizmasıyla açıldı; üç madde:

1. **Vitrinin eksik satır kuralı** (K-755, K-841): orta sınıfta son satırı doldurmayan kartlar gösterilmez, tam satır kuramayan vitrin bütünüyle görünür; iki düzenin kayıt sayılarıyla tarayıcıda doğrulanır (`11` T502 kabul kriteri 2).
2. **Erişilebilirlik taraması F5 kapısında** (K-625, K-1411): A seviyesi tarama F5 gate-check'inde koşar; taramanın maddeleri ve yöntemi `12`'de, F5 kapısından önce hazır olur (`11 §6.2`; T540, T543, T544).
3. **F6 kabul kanıtının senaryolara bağlanışı** (K-23, K-1453): `11` T601 kabul kriteri 10 ve §6.2'nin F6 notu ileri işarettir; `12` yazılınca senaryo kimlikleri T601'e eşlenir ve `11`'e geri yazılır.

#### `DEPLOY_RUNBOOK.md`

Bloğun tek maddesi boot provasının iki turudur: 1. tur F0'ın sonunda referans kuruluma ilk inişte (`11` T09 kabul kriteri 6), 2. tur F6'da T601 kabul kriteri 1 olarak. `§I`'nin "Ne zaman" satırı şu an yalnız kapanış ritüelini söylüyor; doldurulurken iki tur yazılır.

### 6.2 `11` gövdesinde sonraki dokümanı anan cümleler

Mekanik çıkarım: `11`'in gövdesinde (§7 matrisi hariç) `12`, `DEPLOY_RUNBOOK`, `DEFERRED_BACKLOG`, `CI_CD_SETUP` ve "ileri işaret" anan **29 satır**. Tamamı doğrulayıcı D1…D4'te karşılıkları açısından okundu: gerçek iş **4** (D1'in üç alt maddesi, D4), geri kalanı hedefte zaten karşılık bulan ya da `11`'in kendi kuralı olan cümlelerdir.

### 6.3 Doğrulama ve soru devirleri

`11`, dış dünyaya ait olguları kendi sınırında bırakır; `12`'ye bırakılan doğrulamalar `12`'nin Aşama 9 bloğundadır. **Çok kritik soru adayı yoktur.**

---

## 7. Öğrenim terfisi adayları — 5. adımın girdisi

1. **Geri işaret taraması yalnız ilişki sözcüğü penceresine bakarsa gerekçe hücresindeki "X'in lafzını daraltır / tamamlar" kalıplarını kaçırır.** `geri_isaret_tara.py` 19 çiftte 8 eksik buldu ve bunların 7'si çürüğe çıktı; asıl 5 geri işaret (K-1406, K-1410, K-1407, K-508, K-509) pencerenin dışında kaldı, tüm eski K anmalarını (359 çift) tarayan ikinci geçiş buldu. Aday: betik tüm anmaları aday listesine alsın, ilişki sözcüğü yalnız sıralama için kullanılsın (hedef: `scripts/doc-scans/geri_isaret_tara.py`, L5; `checklists/document-stage.md` §3). Aşama 2'nin çakışma taramasının dersiyle (ilişki sözcüğü yetmez) aynı sınıf.
2. **Ölçüm satırı (maliyet düzeni):** bu tarama 11 Sonnet / low okuyucu ve 4 Opus / high doğrulayıcı kullandı; 20 aday 4 doğrulayıcıya kademeli dağıtıldı (aday 4'ün kademeli toplu doğrulama düzeni, K-1300). Aşama 8'de 23 okuyucu ve 11 doğrulayıcı kullanılmıştı; Aşama 9'da okuma yükü dilimlerin 285 park eşleşmesiyle sınırlı kaldı. Bekçi olayı: bir doğrulama çağrısı, talimatta "hedefli mercek" adı geçtiği için `agent-rules.json` kuralıyla ("hedefli" yasak) başlamadan engellendi; sözcük çıkarılarak yeniden çağrıldı, sayaca girmedi.

---

## 8. Checkpoint'e notlar

- Tracker §4'te açık satır yok, A-19 açılmadı; dokümanlarda açık kalem yok — checkpoint'in açık karar kontrolü bu raporla karşılanır.
- ⚠ listesi **8 satır** (§4.1) — arşiv işaretinden önce proje sahibine tek mesajda gösterilir (yönetici). K-1411, K-1413, K-1432, K-1458 gösterildi ve onaylandı (K-1463).
- K-1494 bu taramanın kararıdır. Değişen yüzler: altı eski satırın etki sütunu, `12` ve `DEPLOY_RUNBOOK` park blokları, tracker §22.1 (üç madde), hafıza. **`11` değişmedi** — checkpoint'in iç tutarlılık merceği `11`'in tamamını okur (K-1492 (5) devri: kabul kriteri ya da KK'nin, kaynak cümlenin yanlış okunabilen kısaltması olması sınıfı).
- Park satırları K-1401…K-1493'e bir kez tarandı (285 eşleşme; 253 madde). Checkpoint yeniden taramaz; yalnız bu PR'ın değiştirdiği yerleri okur: `12` (Aşama 9 bloğu), `DEPLOY_RUNBOOK` (Aşama 9 bloğu), K-508, K-509, K-1406, K-1407, K-1409, K-1410 etki sütunları.
- Kalite döngüsünde checkpoint'e devredilen sınıflar (K-1456…K-1493 etki sütunlarında "checkpoint" geçenler) 4. adımın girdisidir; bu tarama onları taramadı.
- 6. adımda sonraki aşamaya (Aşama 10 — Doğrulama Protokolü) devrin tablosu bu raporun §6'sını ve `12` ile `DEPLOY_RUNBOOK`'un Aşama 9 park bloklarını gösterir.
