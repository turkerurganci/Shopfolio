# Aşama 10 — Çakışma Taraması

**Tarih:** 2026-11-02 | **Aşama:** 10 — Doğrulama Protokolü | **Kapanış adımı:** K-437'nin 3. adımı (K-429; `checklists/document-stage.md` §7)
**Girdi sürümleri:** karar kaydı v0.326 · Doğrulama Protokolü (`12`) v0.31 · `DEPLOY_RUNBOOK` ve park blokları taşıyan dokümanlar (`01`…`12`, `DEFERRED_BACKLOG`)
**Çıktı sürümleri:** karar kaydı v0.327 · `12` değişmedi (v0.31) · `DEPLOY_RUNBOOK` (sürüm alanı değişmez; park bloğu eklendi)

> **Ne işe yarar:** Aşama kapanmadan önce açık kalemlerin her birinin **tek bir evi** ve **gözlemlenebilir bir kapısı** olduğunu doğrular; vadesi geçmiş açıkları adıyla listeler (K-429). Önceki aşamaların park satırlarını, kaynak kararlarını sonradan değiştiren kararlara karşı okur. Aşama 10 kararlarının (K-1504…K-1570) eski kararlara geri işaretlerini denetler. Gösterilmemiş ⚠ kararlarının listesini karar kaydıyla eşitler (§4.1). Karar kaydında yanlış bölüme yazılmış Aşama 10 maddelerini yerine taşır (§5.1). Aşama 10 kararlarının sonraki dokümanlara devirlerini tek dizinde toplar (§6). Sonuç checkpoint'in (4. adım) girdisidir.

---

## 1. Sonuç

| Kural | Sonuç |
|---|---|
| **(1) Aynı açık iki yerde durmaz** | ✓ — açık karar yok. Tracker §4'ün on sekiz satırının hepsi kapalı; Aşama 10'da A-19 açılmadı. Dokümanlarda tracker'la çift duran açık yok. |
| **(2) Her açık satır gözlemlenebilir bir kapı taşır** (K-37) | ✓ — §24.1'in açık maddelerinden üçü (Aşama 9 `DEPLOY_RUNBOOK` bloğu, Aşama 10 kapanış adımları, Açık nokta K-1538 (4)) kapısızdı; **kapı yazıldı**. Bu taramanın açtığı iki yeni açık madde (⚠ listesi, `DEPLOY_RUNBOOK` Aşama 10 bloğu) kapıyla açıldı (§4). |
| **(3) Vadesi geçmiş ve açık satır adıyla listelenir** | ✓ — vadesi geçmiş açık yok (§3.3). |

**Çakışmalar:** açık kalem, park satırı, geri işaret ve devir taramaları betikle çıkarıldı; bulgu arayan okuma **9 park + 3 açık kalem + 2 geri işaret = 14 Sonnet / low** okuyucuda yapıldı. Geri işaret adayları (45) **yedi Opus / high** doğrulayıcısı tarafından güncel metne karşı okundu (D1…D7; adım başına sınır 20): **10 doğru, 35 çürütüldü.** Doğrulanan yedi geri işaretin hepsi bu PR'da düzeltildi — **yeni karar istenmedi** (**K-1571**): K-547 ve K-627 (← K-1568), K-793 ve K-783 (← K-1566), K-606 ve K-798 (← K-1563), K-1545 (← K-1561). `DEPLOY_RUNBOOK` için üç maddelik Aşama 10 devir bloğu açıldı; kalite döngüsü ve yazım turu maddelerinden **29'u** yanlış bölümden §24.1'e taşındı; ⚠ eşitlemesi **7 / 7**. ⚠ yeni karar yok.

---

## 2. Yöntem

Tarama gözle değil betikle yapıldı (K-429); anlamsal okuma ayrı alt ajanlarda ve ön planda koştu. Ortam: Git Bash ve Python 3.12 (`python -I -X utf8`). **Model dağılımı (`INSTRUCTIONS.md` §9 (1)):** bulgu arayan okuyucular Sonnet / low (14); bulgu doğrulayıcıları Opus / high (7, kademeli); tarama betikleri, sayımlar ve düzeltmelerin işlenmesi yönetici adım ajanında. Karar kaydı ve `12` hiçbir ajana bütünüyle okutulmadı.

1. **Tracker §4 ve §24.1:** `| A-NN |` satırları ayrıştırıldı (18 satır, hepsi "Kapandı"); `- [ ]` ile açık duran süreç maddeleri çıkarıldı ve her birinde kapı arandı. Dokümanların açık kalem desenleri `00`…`12`, `DEPLOY_RUNBOOK`, `DEFERRED_BACKLOG` ve `IMPLEMENTATION_STATUS`'ta arandı: **501 bağlam**, üç dilimde okundu.
2. **⚠ listesi:** K-1504…K-1570'in konu hücresinde `⚠` taşıyan satırlar betikle çıkarıldı (**7**) ve eşleme dosyasıyla (`onerilen-uyarili.md`) karşılaştırıldı.
3. **Park satırları:** `01`…`12`, `DEPLOY_RUNBOOK` ve `DEFERRED_BACKLOG` bloklarındaki `> - **…**` maddeleri ayrıştırıldı (**310 madde**); etiketlerinden kaynak K'lar çıkarıldı. Her kaynak için K-1504…K-1570'te tam eşleşme arandı: **754 eşleşme, 134 maddede**; dokuz dilimde okundu (**20 aday satırı**).
4. **Geri işaret:** `geri_isaret_tara.py 1504 1570` (ilişki sözcüğü penceresi: 2 çift, 2 eksik) ve ikinci geçiş olarak `--tum` (tüm eski K anmaları: **230 çift, 227 eksik**) koştu; iki okuyucu **27 aday** çıkardı, ilişki sözcüğü kovalayan okuma ve betik çıktısıyla birlikte **45 hüküm** verildi.
5. **Devir dizini:** Aşama 10 kararlarının etki sütunları `DEPLOY_RUNBOOK`, `DEFERRED_BACKLOG`, `IMPLEMENTATION_STATUS` ve skill'ler için tarandı; `12`'nin gövdesinde bu dokümanları anan **48 satır** okundu.
6. **Yer düzeltmesi:** karar kaydında `- [` maddeleri `A10-` / K-1504…K-1570 / `12` v0.xx taşıyanlar betikle bulundu; Aşama 9'a ait (`11` yazım maddeleri, 2026-10-31) olanlar elendi; seçilen **29** madde metin değişmeden §24.1'e taşındı.
7. **Kapanış taramaları:** karar kaydında çift K yok (K-01…K-1571, boşluk yok); değişen dosyalarda CR, kontrol karakteri ve `$[0-9]` artığı sıfır. Kapılar: `12_kapsam_hizasi.py`, `12_matris_denetle.py --kapsam 1-77` (77 / 77 KP kapsandı, hata 0), `12_atif_denetle.py` (hedefi bulunamayan atıf 0, hata 0), `geri_isaret_tara.py 1504 1570` — kalan tek satır §5.2'de adıyla.

Tarama betikleri ve okuyucu çıktıları yönetici yardımcısının çalışma dizininde kaldı; repoya girmedi.

---

## 3. Açık kalemler

### 3.1 Tracker §4

| Kapsam | Satır | Açık | Sonuç |
|---|---|---|---|
| A-01…A-18 (Aşama 1, salt okunur) | 18 | 0 | Aşama 1'in taramasında kapalı |
| A-19 ve sonrası | 0 | 0 | Aşama 10'da açık detay doğmadı: yazımın ve kalite döngüsünün bulduğu her boşluk bir K satırıyla kapandı (K-1504…K-1570) |

### 3.2 Dokümanlar

501 desen bağlamı üç okuyucuda okundu: **açık kalem bulgusu 0 aday.** Eşleşmeler "açık kalem" sipariş kalemi terimi, "Bekliyor" ödeme ve talep durumu, `08`'in DV-1…DV-43 doğrulama defteri satırları (kapısı her satırda yazılı), `12`'de `BEKLEMEDE` sonuç kodu ve park bloklarının kalıp cümleleridir.

### 3.3 Vadesi geçmiş açık madde

Yok. §24.1'in açık maddelerinin kapıları geçmedi (§4).

### 3.4 Açık süreç maddelerinin yinelenmesi

Aşama 9 taramasındaki yineleme hükümleri (Aşama 3'ün Öğrenim adayı 13'ü; Aşama 7 ve 8'in D-01 satırları) değişmedi. Yeni yineleme yok.

---

## 4. Süreç maddeleri ve kapıları

| Madde | Ev | Kapı | Sonuç |
|---|---|---|---|
| Aşama 10 kapanış adımları (K-437) | Tracker §24.1 | **Yoktu — yazıldı:** dört adım bu sırada, her adım kendi `docs:` PR'ıyla; arşiv işareti öğrenim terfisinden sonra, ⚠ listesi ondan önce | Kapı eklendi; maddeye 3. adımın durumu (✓, K-1571) yazıldı |
| `DEPLOY_RUNBOOK.md` Aşama 9 bloğu — boot provasının iki turu | Tracker §24.1 | **Yoktu — yazıldı:** 1. tur F0 kapısında (`11` T09 kabul kriteri 6), 2. tur F6 kapısında (`11` T601 kabul kriteri 1) | Kapı eklendi |
| Açık nokta K-1538 (4) — Erişilebilirlik Kontrol Listesi | Tracker §24.1 | **Yoktu — yazıldı:** F5 gate-check'inin tarama kaydı; F5 kapısından önce | Kapı eklendi (taşınan maddenin tek lafız değişikliği) |
| `DEPLOY_RUNBOOK.md` Aşama 10 bloğu (yeni) | Tracker §24.1 | F6 kapısı (`11` T601); üç madde bu kapıdan önce doldurulmuş olmalı | Açıldı, kapılı |
| ⚠ öneriyle kayıt listesi — Aşama 10 (yeni) | Tracker §24.1 | Aşama 10 kapanışının 6. adımı (arşiv işareti) açılmadan önce | Açıldı, kapılı (§4.1) |
| Ertelenmiş iş D-01 | `DEFERRED_BACKLOG.md` | İlk implementation task'ından önce | Kapı var; değişmedi |

Tracker'da yer alan, §24.1 dışı iki dış kapı (`PLAYBOOK_FEEDBACK.md` — proje tamamlandıktan sonra proje sahibi; `DEFERRED_BACKLOG.md` D-01) değişmedi.

### 4.1 ⚠ öneriyle kayıt listesi — betikle eşitleme

**Betikle doğrulama.** Konu hücresi `⚠` taşıyan Aşama 10 satırı **7**: K-1504 (anma kaydı), K-1547, K-1557, K-1560, K-1563, K-1566, K-1568. Eşleme dosyasında (`onerilen-uyarili.md`) giriş **6** idi; **eksik: K-1504** — "⚠ öneriyle kayıt" modunu adıyla anan kayıt, ürün kararı değil (Aşama 9'daki K-1494 ve K-1463 gibi anma kaydı). Dosyaya eklendi: **7 / 7, eksik 0, fazla 0.**

**Gösterim durumu.** Altı ⚠ ürün kararı (K-1547, K-1557, K-1560, K-1563, K-1566, K-1568) toplu onayla kaydedildi ve proje sahibine arşiv işaretinden **önce** tek listede gösterilir (`INSTRUCTIONS §2`). Bu tarama yeni ⚠ karar kaydetmedi.

> **Sonuç (2026-11-02, K-1574):** liste proje sahibine tek mesajda gösterildi; altı ürün kararı (K-1547, K-1557, K-1560, K-1563, K-1566, K-1568) ve checkpoint'te eklenen K-1572 'kalsın', itiraz yok. Yedi işaret `(… — ⚠ — gözden geçirildi 2026-11-02, itiraz yok)` oldu (betikle 7 / 7). Bu taramadaki 7 / 7, K-1504…K-1570 aralığınaydı; sonradan K-1572 eklenmiş (8 / 8, K-1572) ve arşiv işaretinde K-1571'in konu hücresi de ⚠ listesini sözle anıyor (K-1494 gibi anma kaydı, işaret taşımaz, dönüşmez); eşleme dosyasına eklenerek küme 9 / 9, eksik 0, fazla 0 oldu.

---

## 5. Çakışmalar

### 5.1 Yer düzeltmesi — karar kaydında yanlış bölüme yazılmış maddeler (K-1571)

Aşama 10 maddelerinin 29'u §24.1 yerine Aşama 9'un bölümü §22.1'e (28) ve §12.1 bölgesine (1) yazılmıştı. Betikle bulundu, **metin değişmeden** §24.1'e (Y2 maddesinden sonra, kronolojik sırayla) taşındı:

| Kaynak bölüm | Madde | Sayı |
|---|---|---|
| §22.1 | Yazım Y3a-1…Y5 (yazım turu, `12`) | 12 |
| §22.1 | audit, audit doğrulama 2a/2b/2c/2r, deep review | 6 |
| §22.1 | deep review doğrulama 2a/2b | 2 |
| §22.1 | cross-review 1.–6. tur ve hedefli mercek | 7 |
| §22.1 | Açık nokta K-1538 (4) (kapı cümlesi eklendi) | 1 |
| §12.1 | A10-KD-etki (K-1570) | 1 |
| | **Toplam** | **29** (22.1: 28, 12.1: 1) |

Seçim kuralı Aşama 9'a ait maddeleri (`11` yazım maddeleri Y1…Y8, 2026-10-31 tarihli) dışarıda bıraktı. Taşınmayan: bu taramanın kendi üç yeni maddesi (çakışma taraması tamamlandı, ⚠ listesi, `DEPLOY_RUNBOOK` Aşama 10 bloğu), doğrudan §24.1'e yazıldı.

### 5.2 Geri işaret — hizalanan çakışmalar (K-1571)

Hepsi var olan bir kararın kuralının değişmeden kaldığı yüzlerin ya da değişen kuralın eski satırın etki sütununa yansıtılmasıdır; ✓ durumları korunur, kalite döngüsü yeniden açılmaz (K-652). Karar ve gerekçe hücrelerine dokunulmadı.

| # | Yer | Eski hâl | Düzeltme | Şiddet |
|---|---|---|---|---|
| H1 | K-547 ← K-1568 | Meslek odası ve davranış kuralları "isteğe bağlı" kimlik alanıdır; K-1568 üç firma tipinde zorunlu yapar, elenen "zorunlu alan olsun" seçeneğini kabul eder | K-547'ye geri işaret | orta |
| H2 | K-627 ← K-1568 | Davranış kurallarına bağlantı isteğe bağlı; K-1568 üç tipte zorunlu yapar (KEP kuralı değişmedi) | K-627'ye geri işaret | orta |
| H3 | K-793 ← K-1566 | "Adet talep açılırken seçilir, sonraki adet e-postayla bildirilir"; K-1566 yeniden açmada ayıplı adedin artırılmasına izin verir | K-793'e geri işaret (değiştirir) | orta |
| H4 | K-783 ← K-1566 | "Tek düğmeli" yeniden açma ekranı; K-1566 ekrana adet alanı ekler | K-783'e geri işaret (genişletir) | düşük |
| H5 | K-606 ← K-1563 | Bir yıldan sonraya tarihli kayıt reddedilir; K-1563 reddi kaldırır, firmanın uyarılı onayına bağlar | K-606'ya geri işaret (değiştirir) | orta |
| H6 | K-798 ← K-1563 | Pencere dışı uyarının metni (U3a); K-1563 bir yıldan sonra ulaşan bildirimi de anar | K-798'e geri işaret (genişletir) | düşük |
| H7 | K-1545 ← K-1561 | F2: (1)'deki yük satırlarının kanıt ortamı `mock`; K-1561 `yok` yapar; K-1570'in kayıt notundaki "K-1546 ile" atfı K-1561 (F2) olarak okunur | K-1545'e geri işaret | düşük |

**Kalan `geri_isaret_tara.py` satırı (1).** `geri_isaret_tara.py 1504 1570`: K-1561 → K-1517 — doğrulamada çürütüldü (K-1561 F4, K-1517 (5)'i uygular; kural değişmedi). Betik ilişki sözcüğü penceresine dayandığı için çıkış kodu 1 kalır. `1504 1571` aralığında K-1571'in kendi atıfları da satır olarak çıkar (K-606, K-709, K-798, K-884, K-1517, K-1547, K-1561): bunlar K-1571'in çürüyen çiftleri ve doğrulanan geri işaretleri anan metnidir, bu karar o satırların kuralını değiştirmez.

### 5.3 Park satırları

**Aşama 1–9'un park satırları — numaraya dayalı tarama.** 310 madde; K-1504…K-1570'te kaynak K'ların **754 eşleşmesi** (134 maddede) dokuz dilimde okundu. Okuyucular 20 aday satırı çıkardı; hiçbiri gerçek aday olmadı. Eşleşmelerin çoğu K-652 geri besleme kalıbı ("Üst doküman" atıfları), cross-review turlarının "tekrar eden RET" atıfları ve açılış devir listeleridir.

### 5.4 Geri işaret ve zincirler

230 çift (`--tum`); eski satırın etki sütununda yeni numara olmayan çiftler iki okuyucuda incelendi: **27 aday**, betiğin ek adaylarıyla birlikte 45 hükme gitti (§5.5). Okuyucuların atıf olarak elediği çiftler: gerekçe alıntıları ("tekrar eden RET" listesindeki eski kararlar), uygulanış kayıtları, envanter ve sayımlar. K-1504'ün sekiz eski açılış satırına (K-648…K-1401) geri işaret **eklenmedi**: süreleri kendi arşiv işaretleriyle dolmuş, K-1504 yeni ayrı bir karardır; K-1401, K-1402 ve K-1472'de geri işaret zaten var.

### 5.5 Bağımsız doğrulama (Opus / high, 7 doğrulayıcı)

| Doğrulayıcı | Aday grubu | Doğru | Çürütüldü |
|---|---|---|---|
| D1…D7 | 45 hüküm (yedi doğrulayıcıya kademeli) | **10** | **35** |

Doğru: 10 hüküm; yedi geri işaret (H1…H7) bunların sonucudur. Çürütülenler: K-648…K-1303 süresi kendi arşiv işaretiyle dolmuş (K-1504); K-808 · K-676 · K-790 · K-830 · K-297 (K-1566 kuralı uyguluyor); K-709 · K-607 · K-710 · K-203 · K-548 (K-1563, K-709'u korur); K-606 · K-709 · K-798 ← K-1547 (boşluğu doldurur, kuralı değiştirmez); K-1517 ← K-1561 ve öteki dört çift (uygular).

### 5.6 Betikle aranan ve temiz dönen çakışma sınıfları

| # | Sınıf | Sonuç |
|---|---|---|
| 1 | **⚠ listesi ↔ kayıt** | ✓ düzeltildi — 6 / 7 → 7 / 7 (§4.1) |
| 2 | **K numarası tekilliği** — çift K taraması | ✓ — boş; K-01…K-1571, boşluk yok |
| 3 | **Tracker §4** | ✓ — 18 / 18 kapalı |
| 4 | **Doküman kapıları** — `12_kapsam_hizasi.py`, `12_matris_denetle.py --kapsam 1-77`, `12_atif_denetle.py` | ✓ — hata 0 (77 / 77 KP; hedefi bulunamayan atıf 0) |
| 5 | **Yer düzeltmesi** — §22.1 ve §12.1'de kalan Aşama 10 maddesi | ✓ — 29 taşındı; kalan 0 |
| 6 | **Değişen dosyalarda CR, kontrol karakteri ve `$[0-9]` artığı** | ✓ — sıfır |

---

## 6. Devir dizini — Aşama 10 kararlarının sonraki dokümanlara devirleri

**Tek ev:** devrin evi karar satırının etki sütunu ya da hedef dokümanın park satırıdır; bu dizin devri ikinci kez kaydetmez. Aşama 10'un çıktısı doküman döneminin sonudur: sırada Implementation dönemi vardır (`IMPLEMENTATION_STATUS.md`, `task` / `validate` / `gate-check` skill'leri).

- **Önceki aşamaların dizinleri:** `PHASE1_CONFLICT_SCAN.md` §6 … `PHASE9_CONFLICT_SCAN.md` §6.
- **`DEFERRED_BACKLOG.md`:** Aşama 10'dan yeni kalem yok; `KABUL EDİLMİŞ RİSK` kaydının biçimi lejant ve kayıt kurallarıyla karşılanıyor. D-01'in task karşılığı F0'dadır.
- **`CI_CD_SETUP.md`:** Aşama 10 kararı anmıyor.
- **`IMPLEMENTATION_STATUS.md` ve skill'ler:** `12`'de iş yok (48 satır okundu).
- **Süreç kararları** (K-1504 ve çalışma modları) sonraki dokümana iş bırakmaz.

### 6.1 Etki sütunlarındaki ve gövdedeki devirler (K-1504…K-1570)

| Hedef | Atıf | Aşama 10 park bloğunda | Bu taramada eklenen | Park gerekmez |
|---|---|---|---|---|
| `DEPLOY_RUNBOOK.md` | 0 karar etki sütununda; `12` gövdesinde ilgili satırlar | 0 → 3 | **Yeni blok, üç madde** (K-1518 · K-1537; K-1550 (#16) · K-1558; K-1518 (3)) | `DEFERRED_BACKLOG` ve `IMPLEMENTATION_STATUS` için iş yok |

#### `DEPLOY_RUNBOOK.md`

"Aşama 10'dan park edilen girdiler" bloğu K-36 mekanizmasıyla Aşama 9 bloğunun sonuna açıldı; üç madde:

1. **Canlı kanıt satırları** (K-1518 (3), K-1537 (3)): zincirin her halkası (`12` §12.1, `VAL-F001`…`VAL-F012`) ayrı bir satırdır ve madde kimliğini taşır; sipariş numarası, tutar, işlem anı ve kanıt türü yazılır, kart verisi ve sır yazılmaz. Son fazın gate raporu (`GATE_CHECK_F6.md`) bu satırlara kimlikle bağlanır. Aşama 7 bloğunun K-1239 (6) tarifindeki dört alanlı kayıt bu biçimle genişler.
2. **Ters vekilin gövde tavanı** (K-1550 #16, K-1558): ürünle gelen ters vekil yapılandırması görsel yükleme yolunda tavanı aşan isteğe uygulamanın çerçeveli 413 yanıtıyla aynı görünümü vermelidir (`12` VAL-A629); yöntem bu aşamanın kararıdır, `07 §2.8.7` ile birlikte verilir.
3. **DV gözlem kayıtları** (K-1518 (3)): `12`'de kanıt türü "Runbook kaydı" olan E satırlarının gözlemleri (yeniden gönderim sayısı ve aralığı, `auth_time`, bildirimlerin sırası; DV-1, DV-2, DV-9) bu runbook'ta madde kimliğiyle işaretli satır olarak tutulur; `08 §4` yalnız sonucu taşır.

Bloğun kapısı: F6 kapısı (`11` T601); tracker §24.1'de açık madde olarak izlenir.

### 6.2 `12` gövdesinde sonraki dokümanı anan cümleler

Mekanik çıkarım: `12`'nin gövdesinde `DEPLOY_RUNBOOK`, `DEFERRED_BACKLOG`, `IMPLEMENTATION_STATUS` ve skill anan **48 satır**. Üç iş gerçek devir çıktı (yukarıdaki üç madde); geri kalanı hedefte zaten karşılık bulan ya da `12`'nin kendi kuralı olan cümlelerdir.

### 6.3 Doğrulama ve soru devirleri

`12` dış dünyaya ait olguları kendi sınırında bırakır; Implementation döneminde gate-check ve validate skill'leri satırları çalıştırır. **Çok kritik soru adayı yoktur.**

---

## 7. Öğrenim terfisi adayları — 5. adımın girdisi

1. **Karar kaydı maddeleri yanlış bölüme yazıldı.** 29 Aşama 10 maddesi önceki aşamanın bölümüne (§22.1, §12.1) yazılmıştı: bölüm numarası, aşama açılışında "bu aşamanın yeri" olarak tracker'a yazılmamış ya da işçiler son yazılan bölümü kopyalamış. Aday: aşama açılışında hedef bölüm numarasını tracker'ın başına yaz; kapanışta "bölüm ↔ aşama" betik kontrolü (`scripts/doc-scans/`, L5; `checklists/document-stage.md` §7 adım 3).
2. **`geri_isaret_tara.py` yeni kararın kendi atıflarını satır sayar.** Yeni K numarası aralığa girince, o kararın anıp kuralını değiştirmediği eski K'lar "eksik" çıkar; aralık son karar hariç verilmelidir (`1504 1570`) ya da betik, hedef karar metninde "değiştirmez / uygular" hükmünü taşıyan çiftleri ayırmalıdır.
3. **Ölçüm satırı (maliyet düzeni):** bu tarama 14 Sonnet / low okuyucu ve 7 Opus / high doğrulayıcı kullandı; 45 hüküm 7 doğrulayıcıya kademeli dağıtıldı (sınır 20). İşçi bağlam sınırında bölündü (1/2, 2/2); bölünme noktası tarama ile uygulama arasındaydı ve devir notu bunu karşıladı. Bekçi olayı yok.
4. **Seçim mantığı kalıbı doğrulanmadan çalıştırılmamalı.** Uygulama betiğinin ilk seçimi 39 madde verdi (Aşama 9'a ait yazım maddeleri `11` simgesini farklı biçimde taşıdığı için elenmemişti); `assert` hatayı yakaladı ve seçim 29'a düşürüldü. Aday: toplu taşıma betikleri hep sayı `assert`'i taşır (bu görevde taşıdı).

---

## 8. Checkpoint'e notlar

- Tracker §4'te açık satır yok, A-19 açılmadı; dokümanlarda açık kalem yok — checkpoint'in açık karar kontrolü bu raporla karşılanır.
- ⚠ listesi **7 satır** (§4.1; altı ürün kararı + K-1504 anma kaydı) — arşiv işaretinden önce proje sahibine tek mesajda gösterilir (yönetici); tracker §24.1'de açık madde olarak izlenir.
- K-1571 bu taramanın kararıdır. Değişen yüzler: yedi eski satırın etki sütunu (K-547, K-627, K-793, K-783, K-606, K-798, K-1545), `DEPLOY_RUNBOOK` Aşama 10 park bloğu, tracker §24.1 (taşınan 29 madde, kapılar, üç yeni madde) ile §22.1 ve §12.1 (çıkarılan maddeler), hafıza. **`12` değişmedi (v0.31)** — checkpoint'in iç tutarlılık merceği `12`'nin tamamını okur.
- Park satırları K-1504…K-1570'e bir kez tarandı (754 eşleşme; 310 madde). Checkpoint yeniden taramaz; yalnız bu PR'ın değiştirdiği yerleri okur: yedi etki sütunu, `DEPLOY_RUNBOOK` Aşama 10 bloğu.
- Kalite döngüsünde checkpoint'e devredilen sınıflar bu taramanın girdisi değildir ve 4. adımın girdisidir: (1) K-1569 — Düşük şiddette "kapı/istisna cümlesi ↔ kapı satırı" ve "satır ↔ satır" metin farkı (iç tutarlılık merceği `12`'nin tamamında); (2) VAL-A489 severity tutarlılığı (K-1570); (3) K-1504…K-1570 etki sütunlarında "checkpoint" geçenler.
- 6. adımda Implementation dönemine devrin tablosu bu raporun §6'sını ve `DEPLOY_RUNBOOK`'un Aşama 10 park bloğunu gösterir.
