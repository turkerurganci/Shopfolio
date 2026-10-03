# Aşama 1 — Çakışma Taraması

**Tarih:** 2026-10-03 | **Aşama:** 1 — Product Discovery | **Kapanış adımı:** K-437'nin 3. adımı (K-429)
**Girdi sürümleri:** karar kaydı v0.48 · Proje Vizyonu (`01`) v0.31 · Ürün Gereksinimleri (`02`) v0.45 · MVP Kapsamı (`10`) v0.28 · `DEFERRED_BACKLOG.md` (2026-08-11)
**Çıktı sürümleri:** karar kaydı v0.49 · Ürün Gereksinimleri v0.46 · Proje Vizyonu ve MVP Kapsamı değişmedi

> **Ne işe yarar:** Aşama kapanmadan önce açık kalemlerin her birinin **tek bir evi** ve **gözlemlenebilir bir kapısı** olduğunu doğrular; vadesi geçmiş açıkları kapanış için adıyla listeler (K-429). Sonuç checkpoint'in (4. adım) girdisidir. Ayrıca kalite döngüsünde sonraki dokümanlara devredilen işleri tek listede toplar — Aşama 2'ye devrin (6. adım, K-438) girdisi.

---

## 1. Sonuç

| Kural | Sonuç |
|---|---|
| **(1) Aynı açık iki yerde durmaz** — tek evi K-428'in atamasıyla | ✓ — açık karar yok. Tracker §4'ün on sekiz satırının hepsi kapalı; Ürün Gereksinimleri §13 tablosu boş; Proje Vizyonu ve MVP Kapsamı'nda açık karar yok. Raporlarda kalmış bir açık not bulundu ve kapatıldı (§3.3). |
| **(2) Her açık satır gözlemlenebilir bir kapı taşır** (K-37) | ⚠ → ✓ — bir açık süreç maddesinin kapısı yoktu: **⚠ işaretli seksen üç öneriyle kaydın proje sahibince gözden geçirilmesi.** Kapısı yazıldı (§4.1). |
| **(3) Vadesi geçmiş ve açık satır kapanış raporunda adıyla listelenir** | ✓ — **yok.** On sekiz açığın hepsi kapısında ya da kapısından önce kapandı. |

**Düzeltilen uyumsuzluk: üç** (§5). **Devir girdisi:** kalite döngüsünün 147 kararı sonraki dokümanlara 292 devir taşıyor (§6). **Öğrenim terfisi adayları:** on altı bekleyen aday, karar kaydının beş yerine dağılmış hâlde; dizini §7'de.

---

## 2. Yöntem

Tarama gözle değil betikle yapıldı (K-429'un *"tarama gözle yapılır" elendi* gerekçesi). Üç çıkarım:

1. **Tracker §4 ve §6.1:** `## 4.` ile `## 5.` arasındaki `| A-NN |` satırları sütunlarına ayrıldı — konu, kapı, durum. Kapı hücresi K-37'nin yasakladığı ifadelere (tarih, "sonra", "ilerde", "ihtiyaç olursa") karşı denetlendi; "Blok 9 kapandıktan sonra" gibi bir olaya bağlı "sonra" kapı sayıldı. §6.1'in işaretsiz (`- [ ]`) maddeleri ayrıca listelendi.
2. **Dokümanların ileriye bırakılmış kalemleri:** Proje Vizyonu, Ürün Gereksinimleri ve MVP Kapsamı cümlelere bölündü ve altı desen arandı — sonraki dokümanı anma (`` `03` ``…`` `09` ``, `` `12` ``, `DEPLOY_RUNBOOK`), aşama ve faz ("Aşama 2", "implementation"), devir ("devred", "bırakıl", "ertelen", `DEFERRED`), kalan risk, A-kalemi ve belirsizlik ("açık karar", "açık kal", "belirlenecek", "netleşecek", "karara bağlanacak", "TBD"). Başlıktaki sürüm notları tarihçe olduğu için ayrı sayıldı. Her eşleşme sınıflandırıldı (§3.2).
3. **Devirler:** karar kaydında K-479…K-644'ün — kalite döngüsünün kararları — etki sütunu ` · ` ile parçalandı; `03`–`09`, `11`, `12`, `DEPLOY_RUNBOOK` ya da `DEFERRED_BACKLOG` ile başlayan parçalar alındı. Ayrıca kalite döngüsünün raporlarında (`Docs/AUDIT_REPORTS/`, `Docs/CROSS_REVIEW_REPORTS/`) "devredildi", "işidir", "ele almalı" geçen cümleler etki sütunlarıyla karşılaştırıldı.

---

## 3. Açık kalemler

### 3.1 Tracker §4

| # | Konu | Ev (K-428) | Kapı (K-37) | Durum | Kural 1 | Kural 2 | Kural 3 |
|---|---|---|---|---|---|---|---|
| A-01 | Ticari model | `01` | 01 §4 (değer önerisi) ve §6 (başarı kriterleri) yazılmadan önce | Kapandı — K-18 | tek yerde | var | kapısında kapandı |
| A-02 | `01 §5.2` alternatif matrisinin derece değerleri | `01` | `01 §6–§8` yazım oturumunda, Blok 9 kapandıktan sonra ve `01`'in kalite döngüsü başlamadan önce | Kapandı — K-442 | tek yerde | var | kapısında kapandı |
| A-03 | `01 §3.2` aktör tablosunun "neden geri döner" hücreleri | `01` | `01 §6–§8` yazım oturumunda, Blok 9 kapandıktan sonra ve `01`'in kalite döngüsü başlamadan önce | Kapandı — K-443 | tek yerde | var | kapısında kapandı |
| A-04 | `01 §1`'in sayısal somutluğu | `01` | `B9-09` ve `B9-10` karara bağlandığında, `01 §6` yazılırken | Kapandı — yön K-433, uygulama `01` v0.6 §1 | tek yerde | var | kapısında kapandı |
| A-05 | `02 §1.2` sözlüğünde İngilizce kod karşılıkları | `02` | `02`'nin son yazım oturumunda — Blok 9 kapandıktan sonra, `02`'nin kalite döngüsü başlamadan önce | Kapandı — K-455 | tek yerde | var | kapısında kapandı |
| A-06 | Fiziksel teslimin kaydı ve cayma penceresinin başladığı an | `02` | Blok 8'in ilk workshop oturumunda, ilk konu açılmadan önce | Kapandı — K-288 · K-289 · K-290 | tek yerde | var | kapısında kapandı |
| A-07 | Geri ödemenin yazılmamış iki parçası | `02` | Blok 8'in ilk workshop oturumunda, ilk konu açılmadan önce | Kapandı — K-291 · K-292 | tek yerde | var | kapısında kapandı |
| A-08 | İadede malın geri gönderilme biçimi | `02` | Blok 8'in ilk workshop oturumunda, ilk konu açılmadan önce | Kapandı — K-293 | tek yerde | var | kapısında kapandı |
| A-09 | Durum makinesinin yazılmamış dört köşesi | `02` | Blok 8'in ilk workshop oturumunda, ilk konu açılmadan önce | Kapandı — K-294 · K-295 · K-296 · K-297 | tek yerde | var | kapısında kapandı |
| A-10 | Ürün ve varyantın yayın durumunda iki geçiş | `02` | Blok 8'in ilk workshop oturumunda, ilk konu açılmadan önce | Kapandı — K-298 · K-299 | tek yerde | var | kapısında kapandı |
| A-11 | `10 §3`–`§5`'in derlenmesinde kayıt içi üç gerilim | `10` | `10 §3`–`§5` yazımında (yazım turu, `10` oturumu), `10`'un kalite döngüsünden önce | Kapandı — K-470 · K-471 · K-472 | tek yerde | var | kapısında kapandı |
| A-12 | `02`'nin yazımına kalan üç parça | `02` | `02 §3` ve `§11` yazımında (yazım turu, `02` oturumu), `02`'nin kalite döngüsünden önce | Kapandı — K-445 · K-456 · K-457 | tek yerde | var | kapısında kapandı |
| A-13 | K-423'ün üçüncü sıralama ölçütünün eşiği | `10` | `10`'un kalite döngüsünde, audit başlamadan önce | Kapandı — K-560 | tek yerde | var | kapısında kapandı |
| A-14 | K-209'un geri ödeme kuralının güncel mevzuata uyumu | `02` | `02`'nin kalite döngüsünde, yasal uyum merceği koşarken (audit'ten önce) | Kapandı — K-491 | tek yerde | var | kapısında kapandı |
| A-15 | Malı hiç dönmeyen caymanın iki ucu (K-491'in açık bıraktığı) | `02` | `02`'nin kalite döngüsünde, audit başlamadan önce | Kapandı — K-495, K-496 | tek yerde | var | kapısında kapandı |
| A-16 | K-292'nin — kısmi caymada gidiş kargo bedeli — güncel mevzuata uyumu | `02` | `02`'nin kalite döngüsünde, yasal uyum merceği koşarken (audit'ten önce) | Kapandı — K-501 | tek yerde | var | kapandı — yasal mercek audit'in içinde koştu (not 3) |
| A-17 | Malı dönmeyen caymada havale hattı IBAN'ının saklanma süresi | `02` | `02`'nin kalite döngüsünde, audit başlamadan önce | Kapandı — K-497 | tek yerde | var | kapısında kapandı |
| A-18 | Silinen IBAN'ın müşteriden yeniden isteniş biçimi (K-497'nin açık bıraktığı) | `02` | `02`'nin kalite döngüsünde, audit başlamadan önce | Kapandı — K-498 | tek yerde | var | kapısında kapandı |

**Not 3 — A-16:** kapı *"`02`'nin kalite döngüsünde, yasal uyum merceği koşarken (audit'ten önce)"* diyordu; yasal mercek `02`'nin audit'inin on beş merceğinden biri olarak koştu ve A-16 o turda K-501 ile kapandı. Satır kapalıdır; kapı ifadesiyle fiilî sıra arasındaki fark kapanışı etkilemez.

### 3.2 Ürün Gereksinimleri §13, Proje Vizyonu ve MVP Kapsamı

| Kaynak | Bulunan | Sınıf | Sonuç |
|---|---|---|---|
| Ürün Gereksinimleri §13 | Tablo boş; §13.4 açıkların tarihçesini taşır | — | Açık karar yok. §13.4 A-13'ün yalnız açıldığını söylüyordu — düzeltildi (§5, U-1) |
| Proje Vizyonu, MVP Kapsamı | Açık kararlar bölümü yok (şablonda da yok) | — | Açık karar yok. MVP Kapsamı'nın başlık notu *"çakışma taraması `10` için boş döner"* diyor ve doğru. Bölümün yokluğu `10` yazım oturumunun (3) numaralı öğrenim adayıdır (§7, 10 numara) |
| Üç doküman — "kalan risk" | Ürün Gereksinimleri 35, MVP Kapsamı 3, Proje Vizyonu 1 cümle | Kapalı karar | Bilinçle kabul edilmiş risk açık karar değildir; o kuralın kaydında ve `02`'deki karşılığında yaşar (`01 §8`, K-421, K-445). Tetikleyici taşıyan tek risk — siparişe özel uzun üretim (`02 §3.20`, K-549) — MVP Kapsamı SK-7'nin tetikleyicisidir; tek evi orası |
| Üç doküman — belirsizlik desenleri | "açık kalır", "açık kalem" gibi eşleşmeler | Gürültü | Bir durumun ya da yolun açık olduğunu anlatıyorlar ("kart yolu açık kalır", "açık kalemi kalmamış sipariş"); karar açığı değil |
| Proje Vizyonu §8 | V-1…V-7 varsayımları | Doğrulama | Her satırın "Nasıl doğrulanacak" hücresi gözlemlenebilir bir an yazıyor (ilk gerçek kurulumun ilk üç ayı, abonelik sözleşmesi, ilk yıllık yenileme, firma sahibiyle görüşme) |
| MVP Kapsamı §4.2 | SK-1…SK-10 | Kısıt | Her satır ya ölçülebilir bir tetikleyici ya da "Kalkmaz" taşıyor (K-26) |
| Üç doküman — sonraki dokümanı anan cümleler | 67 cümle | Devir 63, anma 4 | §6.2 |

### 3.3 Raporlarda kalmış açık not

`02`'nin cross-review'ı bir notu 21. turdan 26. tura *"`04` bunları ele almalı"* diye taşıdı ve `Docs/CROSS_REVIEW_REPORTS/02_CROSS_REVIEW_R26.md` §3'te bıraktı: **üye sipariş verdikten sonra hesabının e-postasını değiştirirse, adres düzeltmesinin (K-601) ve başka kanaldan gelen caymanın (K-607) teyidi hangi adrese yapılır?** Not hiçbir karar satırının etki sütununa geçmemişti — tek evi bir rapordu ve kapısı yoktu.

**Kapatıldı — cevap metinde var:** `02 §3.23.3` siparişin iletişim e-postasını siparişe dondurur ve hesaptaki sonraki değişikliğin geçmiş siparişe dokunmadığını yazar; tek istisna misafir siparişinin düzeltmesidir (§10.4.11). K-601 ve K-607'nin *"siparişin iletişim e-postası"* bu donmuş adrestir; teyit oraya ya da siparişteki teslimat telefonuna yapılır. `04`'e kalan yalnız arayüzdür — teyit hatırlatması siparişin donmuş adresini gösterir — ve K-601 ile K-607'nin etki sütunlarındaki `04` devri bunu kapsar. Yeni karar gerekmedi.

Aynı rapordaki diğer notlar etki sütunlarında karşılık buldu: K-606'nın hizmet ve dijital kalem notu (`04`, `12`), K-614 (`08`), K-613 (`04`), aydınlatma taslağında gecikme feshindeki IBAN (`12`).

---

## 4. Kapı yazılan süreç maddesi

### 4.1 ⚠ işaretli öneriyle kayıtların gözden geçirilmesi

Proje sahibinin 2026-10-03 talimatıyla (*"kolay soruları sorma, çok kritik konuları sor"*) ⚠ grubuna giren kararlar sorulmadan kaydedildi ve `(öneriyle kaydedildi — ⚠)` işaretini taşır. Karar kaydı bu satırların **proje sahibinin gözden geçirme listesi** olduğunu söylüyor (§6.1'in kalite döngüsü maddesi; `02 §13.4`), ama listenin **ne zaman** gösterileceğini hiçbir yer yazmıyor.

| Grup | Kararlar | Sayı |
|---|---|---|
| A-18 ve `02`'nin audit ve deep review'ı | K-498…K-525 | 28 |
| `02`'nin cross-review'ı | K-561 · K-562 · K-566 · K-569 · K-570 · K-571 · K-572 · K-573 · K-574 · K-575 · K-576 · K-578 · K-579 · K-581 · K-582 · K-583 · K-585 · K-586 · K-587 · K-589 · K-590 · K-591 · K-592 · K-593 · K-595 · K-596 · K-597 · K-598 · K-599 · K-601 · K-602 · K-603 · K-604 · K-605 · K-606 · K-607 · K-608 · K-609 · K-610 · K-611 · K-612 · K-614 | 42 |
| `02`'nin etki yansıtması | K-616 | 1 |
| `10`'un audit ve deep review'ı | K-618…K-629 | 12 |
| **Toplam** | | **83** |

**Aynı kaçak ikinci kez:** 2026-10-01'de toplu onaylı yirmi iki karar da gözlemlenebilir bir kapıya bağlanmadan *"gözden geçirilecek"* diye taşınmış ve ancak bir sonraki oturumun durum sorusunda yakalanmıştı (karar kaydı §6.1, 2026-10-01 maddesi). O maddenin tespiti — *"K-37'nin açık kararlar için koyduğu kapı kuralının süreç maddelerinde karşılığı yok"* — burada tekrarlandı; K-438'in birinci adayının kanıtına eklenir (§7, 1 numara).

**Yazılan kapı:** liste checkpoint raporunda (K-437'nin 4. adımı) adıyla yer alır ve **arşiv işaretinden (6. adım) önce** proje sahibine tek listede — karar başına bir cümle, 2026-10-01'deki biçimle — gösterilir. İtiraz gelen karar yeni bir karar satırıyla değişir (`02 §13.4`); itiraz gelmeyen satırın işareti `(öneriyle kaydedildi — ⚠ — gözden geçirildi YYYY-AA-GG, itiraz yok)` olur — 2026-10-01'deki dönüşümün kalıbı. Madde karar kaydının §6.1'ine işaretsiz madde olarak girdi.

**Neden 6. adımdan önce:** arşiv işaretinden sonra karar kaydı salt okunur sayılır (K-436); itiraz gelen bir kararın yeni satırı o zaman yazılamaz. **Neden checkpoint'ten önce değil:** K-437 sırayı kilitledi ve checkpoint bu taramanın çıktısını bekliyor; gözden geçirme bir kararı değiştirirse değişiklik 6. adımdan önce yazılır ve checkpoint raporuna retro bölümüyle eklenir (`Docs/CHECKPOINT_REPORTS/README.md`).

---

## 5. Düzeltilen uyumsuzluklar

| # | Yer | Uyumsuzluk | Düzeltme |
|---|---|---|---|
| U-1 | `02 §13.4` | A-13'ün `01`'in kalite döngüsünde açıldığını ve `10 §5`'e ait olduğunu yazıyor, K-560 ile kapandığını yazmıyordu — tracker §4'te satır kapalı | Cümle kapanışı yazar; §13'ün Kaynak satırına K-560 (A-13) girdi (`02` v0.46) |
| U-2 | `02 §6.8` | 6.8.11 satırı 6.8.1'den hemen sonra duruyordu — `02`'nin cross-review'ı checkpoint'e not etmişti (R26 §5) | Satır 6.8.10'dan sonraya taşındı; numara ve atıflar değişmedi |
| U-3 | `02 §10.7` | 10.7.4 maddesi 10.7.1'den hemen sonra duruyordu — aynı not | Madde 10.7.3'ten sonraya taşındı; numara ve atıflar değişmedi |

**Uyumsuzluk sayılmayan tekrar:** yedeklemenin yöntemi, sıklığı ve kurtarma süresinin `05` ile `DEPLOY_RUNBOOK`'a devri üç yerde aynı cümleyle geçer — `02 §3.34.8`, MVP Kapsamı §1 ve ÖK-2. Devrin evi K-407'nin ve K-629'un etki sütunudur; üç cümle aynı şeyi söylüyor ve her biri kendi bağlamının (veri kaybı toleransı, MVP'nin kabulü, kurulum ön koşulu) parçası. Çakışma iki yerin farklı şey söylemesidir; burada yok.

---

## 6. Devir girdisi — Aşama 2'ye devredilen işler

Bu dokümanlar henüz yazılmadığı için devirler kararların etki sütunlarında yaşar (`02` R26 §5, `10` R16 §5). Aşama 2'nin her doküman oturumu kendi satırlarını buradan ve etki sütunlarından alır. **Tek ev:** devrin evi karar satırının etki sütunudur; bu liste bir dizindir, devri ikinci kez kaydetmez.

- **`DEFERRED_BACKLOG.md`:** kalite döngüsünde bu dosyaya devredilen kalem **yok**. Tek aktif kalem D-01'dir (kurulum — SETUP §2 ve §4; hedefi Aşama 4 kapanışından sonra, ilk implementation task'ından önce).
- **MVP Kapsamı §4.1 (ÖK-1…ÖK-12):** Aşama 2 ve sonrasına devredilen dış ön koşulların evi orasıdır (K-438); burada tekrarlanmaz.
- **Yol haritası adayları:** MVP Kapsamı §5 (YH-1…YH-22; K-423, K-424); burada tekrarlanmaz.

### 6.1 Kalite döngüsü kararlarının devirleri (K-479…K-644)

| Hedef | Karar sayısı | Kararlar |
|---|---|---|
| `03` Kullanıcı Akışları | 2 | K-532 · K-578 |
| `04` Arayüz Tanımları | 80 | K-480 · K-484 · K-485 · K-491 · K-493 · K-494 · K-495 · K-496 · K-497 · K-498 · K-499 · K-503 · K-505 · K-509 · K-510 · K-511 · K-512 · K-514 · K-522 · K-525 · K-534 · K-535 · K-536 · K-537 · K-538 · K-540 · K-541 · K-544 · K-545 · K-547 · K-550 · K-553 · K-554 · K-561 · K-564 · K-565 · K-566 · K-569 · K-570 · K-573 · K-574 · K-575 · K-576 · K-577 · K-578 · K-580 · K-581 · K-584 · K-585 · K-586 · K-587 · K-588 · K-589 · K-590 · K-591 · K-592 · K-593 · K-594 · K-595 · K-596 · K-599 · K-600 · K-601 · K-602 · K-603 · K-604 · K-605 · K-606 · K-607 · K-608 · K-609 · K-610 · K-611 · K-612 · K-613 · K-616 · K-625 · K-626 · K-627 · K-643 |
| `05` Teknik Mimari | 25 | K-482 · K-515 · K-522 · K-524 · K-533 · K-552 · K-555 · K-556 · K-557 · K-562 · K-579 · K-582 · K-587 · K-588 · K-589 · K-590 · K-591 · K-592 · K-593 · K-594 · K-602 · K-603 · K-613 · K-614 · K-629 |
| `06` Veri Modeli | 35 | K-480 · K-491 · K-496 · K-497 · K-498 · K-500 · K-508 · K-511 · K-515 · K-519 · K-526 · K-527 · K-531 · K-532 · K-548 · K-561 · K-563 · K-565 · K-566 · K-567 · K-570 · K-575 · K-585 · K-586 · K-595 · K-596 · K-598 · K-600 · K-604 · K-605 · K-606 · K-608 · K-610 · K-617 · K-627 |
| `07` API Tasarımı | 5 | K-526 · K-562 · K-568 · K-579 · K-588 |
| `08` Entegrasyon Tanımı | 13 | K-498 · K-518 · K-521 · K-539 · K-556 · K-569 · K-581 · K-585 · K-589 · K-610 · K-614 · K-616 · K-624 |
| `12` Doğrulama Protokolü | 127 | K-480 · K-484 · K-485 · K-486 · K-487 · K-488 · K-489 · K-490 · K-491 · K-493 · K-494 · K-495 · K-496 · K-497 · K-498 · K-499 · K-500 · K-501 · K-503 · K-504 · K-505 · K-506 · K-507 · K-508 · K-509 · K-510 · K-511 · K-512 · K-513 · K-514 · K-515 · K-516 · K-517 · K-518 · K-519 · K-520 · K-521 · K-522 · K-523 · K-524 · K-525 · K-527 · K-528 · K-529 · K-530 · K-532 · K-533 · K-534 · K-535 · K-536 · K-537 · K-538 · K-539 · K-540 · K-542 · K-543 · K-544 · K-545 · K-546 · K-547 · K-548 · K-549 · K-550 · K-551 · K-552 · K-553 · K-554 · K-555 · K-556 · K-557 · K-558 · K-559 · K-560 · K-561 · K-562 · K-563 · K-564 · K-565 · K-566 · K-567 · K-568 · K-569 · K-570 · K-571 · K-572 · K-575 · K-576 · K-577 · K-578 · K-579 · K-580 · K-581 · K-582 · K-583 · K-584 · K-585 · K-586 · K-587 · K-588 · K-589 · K-591 · K-593 · K-595 · K-597 · K-600 · K-603 · K-604 · K-605 · K-606 · K-607 · K-608 · K-609 · K-611 · K-612 · K-616 · K-618 · K-619 · K-621 · K-622 · K-623 · K-624 · K-625 · K-626 · K-627 · K-628 · K-629 · K-643 |
| `DEPLOY_RUNBOOK.md` | 5 | K-481 · K-482 · K-620 · K-624 · K-629 |

`09` Kodlama Kuralları ve `11` Uygulama Planı'na kalite döngüsünde devir yok; `09`'un sözlüğü birebir devralma kuralı döngüden öncedir (K-17).

#### `03` Kullanıcı Akışları

**Yalnız doküman adıyla işaretli** (devrin içeriği karar satırının kendisindedir): K-532 · K-578

#### `04` Arayüz Tanımları

| Karar | Etki sütunundaki devir |
|---|---|
| K-484 | satış özeti |
| K-491 | iki modlu sayaç, teslim alma adımında tarih alanı |
| K-493 | form metni |
| K-494 | cayma beyanı ekranı, "iade malı bekleniyor" görünümü |
| K-495 | satış özetinde iptal ve iade sayısı |
| K-496 | kapatma düğmesi ve gerekçesi, kapalı kalemin görünümü |
| K-497 | IBAN'ın yeniden istenmesi — A-18 |
| K-498 | sipariş sayfasında yeniden açılan IBAN alanı; panelde "IBAN bekleniyor" |
| K-561 | adres formu |
| K-565 | kupon formu |
| K-566 | düzeltme ekranında yetmeyen kalem |
| K-569 | sipariş sayfasının IBAN alanı |
| K-570 | düzeltme ekranı |
| K-573 | ürün formunun hatırlatma metni |
| K-574 | firma kimliği formu, bandın yerleşimi |
| K-575 | sipariş sayfasının IBAN alanı, panel adımı |
| K-576 | kapı uyarıları |
| K-578 | hizmet kaleminin cayma ekranı |
| K-581 | panelde IBAN'sız kayıt |
| K-584 | Hakkımızda ve duyuru formu, eksik alan gösterimi |
| K-585 | panel düzeltme ekranı, "e-posta düzeltildi" işareti |
| K-586 | siparişin sayfasında e-posta geçmişi |
| K-587 | panelde "sipariş değişti" uyarısı |
| K-588 | iki giriş ekranı |
| K-589 | siparişin sayfasında geç ödeme ve geri ödemesi |
| K-590 | yeniden gönderimde kalem seçimi |
| K-591 | onay adımında metin değişikliği satırı, kutuların yeniden işaretlenmesi |
| K-592 | kapanış işlemi, sebep seçiminin olmaması |
| K-593 | hizmet kaleminin cayma düğmesi |
| K-594 | boş sepet mesajı |
| K-595 | sipariş sayfasında kutunun kaydı |
| K-596 | kaleme bağlı tutar bazlı geri ödeme |
| K-599 | bildirim metni ve geri alma ekranı |
| K-600 | panel uyarısı |
| K-601 | B-10 metni, düzeltme ekranındaki hatırlatma |
| K-602 | takip formunun nötr mesajı |
| K-603 | çerez politikası taslağında beşinci çerez |
| K-604 | cayma bildirimi kaydında kargoya verilmemiş kalem seçilemez |
| K-605 | kayıt formunda ad alanı, hesapta adı değiştirme |
| K-606 | panelde pencere dışı ve istisnalı kalem uyarısı, ayrı onay |
| K-607 | kayıt ekranında ve müşteri talebi iptalinde teyit hatırlatması |
| K-608 | teslim işaretinde, tarih düzeltmesinde ve başka kanaldan gelen bildirimin kaydında sıra sorusu |
| K-609 | "stokta bulunamadı" uyarısı |
| K-610 | ürün sayfasında logonun yeri |
| K-611 | "geri ödeme gerçekleşmedi" uyarısının metni |
| K-612 | "geri ödeme gerçekleşmedi" uyarısının metni |
| K-613 | açık ödenmemiş siparişlerin panel görünümü |
| K-616 | bildirim metni |
| K-625 | tasarım sistemi; 3.2.6, 3.3.7 |
| K-626 | altbilgi, sayfa |
| K-627 | iletişim yerleşimi, kimlik formu |

**Yalnız doküman adıyla işaretli** (devrin içeriği karar satırının kendisindedir): K-480 · K-485 · K-499 · K-503 · K-505 · K-509 · K-510 · K-511 · K-512 · K-514 · K-522 · K-525 · K-534 · K-535 · K-536 · K-537 · K-538 · K-540 · K-541 · K-544 · K-545 · K-547 · K-550 · K-553 · K-554 · K-564 · K-577 · K-580 · K-643

#### `05` Teknik Mimari

| Karar | Etki sütunundaki devir |
|---|---|
| K-482 | güncellemenin yolu |
| K-562 | anahtarın üretimi |
| K-587 | eşzamanlılık mekanizması |
| K-588 | hesap türü ve e-posta tekilliği |
| K-589 | siparişin ödeme kaydı |
| K-590 | geçiş koşulları |
| K-591 | onay anında sürüm karşılaştırması |
| K-592 | kapanış kaydı |
| K-602 | sayaç mekanizması |
| K-603 | işaretin ve iki sayacın mekanizması |

**Yalnız doküman adıyla işaretli** (devrin içeriği karar satırının kendisindedir): K-515 · K-522 · K-524 · K-533 · K-552 · K-555 · K-556 · K-557 · K-579 · K-582 · K-593 · K-594 · K-613 · K-614 · K-629

#### `06` Veri Modeli

| Karar | Etki sütunundaki devir |
|---|---|
| K-480 | firma tipi |
| K-491 | iade malının ulaşma tarihi |
| K-496 | kalemin kapanış kaydı |
| K-497 | IBAN'ın silinme anları |
| K-498 | IBAN isteği ve tarihi |
| K-586 | e-posta geçmişi siparişin parçası |
| K-595 | onay kutusu kaydı |
| K-598 | kalem alanları |
| K-604 | S5'in koşulu |
| K-605 | hesabın ad alanı |
| K-617 | beş adın şemadaki karşılığı |

**Yalnız doküman adıyla işaretli** (devrin içeriği karar satırının kendisindedir): K-500 · K-508 · K-511 · K-515 · K-519 · K-526 · K-527 · K-531 · K-532 · K-548 · K-561 · K-563 · K-565 · K-566 · K-567 · K-570 · K-575 · K-585 · K-596 · K-600 · K-606 · K-608 · K-610 · K-627

#### `07` API Tasarımı

| Karar | Etki sütunundaki devir |
|---|---|
| K-568 | adres üretimi ve tekillik |

**Yalnız doküman adıyla işaretli** (devrin içeriği karar satırının kendisindedir): K-526 · K-562 · K-579 · K-588

#### `08` Entegrasyon Tanımı

| Karar | Etki sütunundaki devir |
|---|---|
| K-498 | B-14 e-postası |
| K-581 | B-14'ün yeni tetiği |
| K-585 | B-15 |
| K-610 | logonun ürünle gelen varlık olarak bakımı |
| K-614 | bağlantı hatasının ayrımı |
| K-616 | F-5 |

**Yalnız doküman adıyla işaretli** (devrin içeriği karar satırının kendisindedir): K-518 · K-521 · K-539 · K-556 · K-569 · K-589 · K-624

#### `12` Doğrulama Protokolü

| Karar | Etki sütunundaki devir |
|---|---|
| K-597 | sözleşmede kişiye özel mal için otuz günlük taahhüt |
| K-605 | aydınlatma taslağında hesap verisi |
| K-625 | Kontrol Listesi eşlemesi |

**Yalnız doküman adıyla işaretli** (devrin içeriği karar satırının kendisindedir): K-480 · K-484 · K-485 · K-486 · K-487 · K-488 · K-489 · K-490 · K-491 · K-493 · K-494 · K-495 · K-496 · K-497 · K-498 · K-499 · K-500 · K-501 · K-503 · K-504 · K-505 · K-506 · K-507 · K-508 · K-509 · K-510 · K-511 · K-512 · K-513 · K-514 · K-515 · K-516 · K-517 · K-518 · K-519 · K-520 · K-521 · K-522 · K-523 · K-524 · K-525 · K-527 · K-528 · K-529 · K-530 · K-532 · K-533 · K-534 · K-535 · K-536 · K-537 · K-538 · K-539 · K-540 · K-542 · K-543 · K-544 · K-545 · K-546 · K-547 · K-548 · K-549 · K-550 · K-551 · K-552 · K-553 · K-554 · K-555 · K-556 · K-557 · K-558 · K-559 · K-560 · K-561 · K-562 · K-563 · K-564 · K-565 · K-566 · K-567 · K-568 · K-569 · K-570 · K-571 · K-572 · K-575 · K-576 · K-577 · K-578 · K-579 · K-580 · K-581 · K-582 · K-583 · K-584 · K-585 · K-586 · K-587 · K-588 · K-589 · K-591 · K-593 · K-595 · K-600 · K-603 · K-604 · K-606 · K-607 · K-608 · K-609 · K-611 · K-612 · K-616 · K-618 · K-619 · K-621 · K-622 · K-623 · K-624 · K-626 · K-627 · K-628 · K-629 · K-643

#### `DEPLOY_RUNBOOK.md`

| Karar | Etki sütunundaki devir |
|---|---|
| K-481 | abonelik sözleşmesi şablonu |
| K-482 | veri işleme sözleşmesi, bakım erişimi |

**Yalnız doküman adıyla işaretli** (devrin içeriği karar satırının kendisindedir): K-620 · K-624 · K-629

### 6.2 Dokümanların gövdesinde sonraki dokümanı anan cümleler

Mekanik çıkarım; sürüm notları hariç. **Devir:** işi o dokümana bırakan cümle. **Anma:** sonraki dokümanı yalnız anar ya da bir olasılığı anlatır (ör. *"`08`'in en ağır entegrasyonu olurdu"*).

| Doküman | Bölüm | Hedef | Tür | Cümle |
|---|---|---|---|---|
| 10 | 1. MVP tanımı | 05, DEPLOY_RUNBOOK | devir | Yedeklemenin sıklığı, yöntemi ve kurtarma süresi `05`'in ve `DEPLOY_RUNBOOK`'un işidir. |
| 10 | 2. Kapsam dahilinde | 12 | devir | Satırların biçimi. Her satır tam bir cümledir — kim, ne yapabilir — ve `12` onu çevirmeden doğrulama senaryosuna devralır (K-24). |
| 10 | 2. Kapsam dahilinde | 12 | devir | Bir kısım satır bir aktörün yeteneğini değil sistemin kuralını anlatır (KP-16, KP-28, KP-74, KP-75); bu satırlarda özne kayıt ya da kuraldır ve `12` onları sistem davranışı olarak devralır. "Kullanıcı" bir aktör değildir… |
| 10 | 3. Kapsam dışı | 08 | anma | Her kurulumda firmanın anlaştığı kargo şirketiyle ayrı bir bağlantı ister ve `08`'in en ağır entegrasyonu olurdu; teslim tarihini sistem kendisi öğrenemez (K-132, K-288, K-473). |
| 10 | 4.1 Kurulum kontrol listesi | 05, DEPLOY_RUNBOOK | devir | Yedeklemenin yöntemi, sıklığı ve kurtarma süresi `05`'in ve `DEPLOY_RUNBOOK`'un işidir. |
| 10 | 4.2 Kısıtlar ve kabuller | 05 | devir | Hacim kabulleri: günde ortalama 10, tepe günde 50 sipariş · fiziksel üründe birkaç yüz ürüne kadar katalog · elli kaleme kadar sepette ödeme öncesi yeniden değerlendirme `05`'in tasarım değeri içinde yanıt verir · yanıt … |
| 10 | 4.2 Kısıtlar ve kabuller | 05 | anma | Sipariş hacminde SK-3'ün tetikleyicisi gerçekleşirse; katalogda ilk gerçek kurulumun katalog ölçümü `01 §8` V-1'i yanlışlarsa; sepette ilk gerçek kurulumda elli kalemi aşan bir sipariş görülürse; eşzamanlı ziyaretçide `0… |
| 01 | 3.2 Ürünü kullanan: aktörler | DEPLOY_RUNBOOK | devir | - Platform operatörü uygulama içi aktör değildir. Shopfolio tek bir firmanın kendi sitesidir (K-01); kurulum bir deploy işidir (Aşama 4 + `DEPLOY_RUNBOOK.md`) ve uygulamaya operatör paneli koymak çok kiracılığı arka kapı… |
| 01 | 6. Başarı kriterleri | 12 | devir | Dört kriterin dördü de `12`'de sınanır ve MVP'nin kabul koşuludur (K-415, K-418). |
| 01 | 6. Başarı kriterleri | 12 | devir | `12`'de bir senaryo: ön koşullar tamamlandıktan sonra hiçbir kod değişikliği yapılmadan uçtan uca bir sipariş alınır |
| 01 | 6. Başarı kriterleri | 12 | devir | Her iş `12`'de ayrı bir senaryodur ve yalnız panel arayüzüyle tamamlanır — kod, veritabanı ya da kurulum ayarı değişikliği olmadan (K-08, K-478) |
| 01 | 6. Başarı kriterleri | 12 | devir | `12`'de her hat ve bir karışık sipariş için ayrı senaryoyla, K-401'in envanteriyle sayılır: kart + fiziksel sipariş 2 · havale + fiziksel 3 · hizmet 1 · dijital 0; havale ile ödenen hizmet ve dijital siparişe "ödendi" iş… |
| 01 | 6. Başarı kriterleri | 03, 12 | devir | Kural ileriye dönüktür: `03`–`12` yazılırken ve implementation'da doğan her yeni elle adım önerisi bu bütçeyle birlikte okunur (K-404) |
| 01 | 6. Başarı kriterleri | 12 | devir | `12`'nin kabul kanıtı. |
| 02 | 1.1 Sözlük kuralı | 06, 07, 09 | devir | - `06`, `07` ve `09` bu sözlüğü birebir devralır, isim icat etmez. |
| 02 | 1.1 Sözlük kuralı | 06 | devir | - Kapalı listeler sözlüğe liste adı ve İngilizce karşılığıyla girer; liste değerlerinin kod adları `06`'da verilir. Bu, ilk kuralın tek istisnasıdır: değerler gövdede kapalı listeyle tek yerde yazılıdır ve `06` onları bi… |
| 02 | 1.1 Sözlük kuralı | 03, 04, 06 | devir | Kuralın gerekçesi `03` ve `04`'ün `06`'dan önce yazılmasıdır: İngilizce karşılık veri modeline bırakılsaydı o aralıkta durum ve enum adları başıboş kalır, `06` geldiğinde geriye dönük hizalama gerekirdi (K-17). |
| 02 | 1.1 Sözlük kuralı | 06 | devir | Bedeli kayıtlıdır: henüz varlığa dönüşmemiş kavramlara erken isim verilir ve `06` yazılırken birkaç satırın revize edilmesi beklenir — bu bir sapma değil, normal akıştır (K-17). |
| 02 | 2. Temel akış | 03 | devir | Bu bölüm akışların listesini ve omurgasını taşır; adımların ayrıntısı ve dalları `03`'ün işidir (K-221). |
| 02 | 3. İş kuralları | 04 | anma | - Bir kuralın biçimini, metnini ya da ekrandaki yerini `04`'e bırakan ifade, arayüz kararının o dokümanda alınacağını söyler; kuralın kendisi burada tamdır. |
| 02 | 3.1 Firma, kurulum ve satış kapısı | 04 | devir | K-627); yerleşimi `04`'ün işidir. |
| 02 | 3.3 Ürün, varyant ve seçenek | 04 | devir | Kodun panelde önceden doldurulması ve düzenlenebilirliği `04`'ün işidir (K-88). |
| 02 | 3.9 İndirim ve referans fiyat | 04 | devir | İndirim beyanının göründüğü her yerde — ürün sayfası ve vitrin kartı — indirimin başlangıç ve bitiş tarihi de gösterilir; biçimi `04`'ün işidir (K-545). |
| 02 | 3.12 Dijital ürün | 05 | devir | Kabul edilen dosya türleri ve yüklenen dosyanın güvenliği `05`'in işidir (K-143). |
| 02 | 3.12 Dijital ürün | 05 | devir | İndirmenin sayıldığı an `05`'in işidir (K-146). |
| 02 | 3.13 Üyelik, e-posta doğrulama ve oturum | 04 | devir | Ödeme adımında "bu e-posta kayıtlı, giriş yaparsanız adresleriniz dolu gelir" hatırlatması gösterilir; biçimi `04`'ün işidir (K-99). |
| 02 | 3.16 Sepet | 04 | devir | Mesajın metni ve yeri `04`'ün işidir (K-125). |
| 02 | 3.16 Sepet | 05 | devir | Elli kaleme kadar sepet bir performans kabulüdür, sınır değildir: ödeme öncesi yeniden değerlendirme (§3.17.2) bu boyutta kabul edilebilir sürede yanıt verir; yanıt süresinin değeri `05`'in tasarım girdisidir (§11.3; |
| 02 | 3.19 Kargo ücreti ve ücretsiz kargo eşiği | 08 | anma | Kargo şirketiyle entegrasyon yoktur (`08`, K-132). |
| 02 | 3.19 Kargo ücreti ve ücretsiz kargo eşiği | 04 | devir | Metnin biçimi ve yeri `04`'ün işidir (K-138). |
| 02 | 3.20 Teslim: bölge, yol, kargoya verme ve tak | 04 | devir | Kısıt varsa site bunu müşteri adres adımına gelmeden görünür kılar; biçimi `04`'ün işidir. |
| 02 | 3.20 Teslim: bölge, yol, kargoya verme ve tak | 08 | devir | Listenin hangi şirketleri taşıdığı ve her şirketin takip bağlantısı kalıbı `08`'in işidir (K-142'nin devri); güncellenmesi ürünün bakımıdır (K-18) — bir şirket takip sayfasının adres biçimini değiştirdiğinde güncelleme g… |
| 02 | 3.24 Onay adımı ve yasal metinler | 04 | devir | Bilgi verilmezse tüketici siparişiyle bağlı değildir (Mesafeli Sözleşmeler Yönetmeliği m.8/1); metin ve biçim `04`'ün işidir (K-544). |
| 02 | 3.24 Onay adımı ve yasal metinler | 04 | devir | 3.24.2 Sepette dijital kalem varsa üçüncü bir kutu çıkar: "İndirme, ödemem onaylandığı anda açılacak; bu üründe cayma hakkımın düşeceğini kabul ediyorum" — havalede indirme firmanın "ödendi" işaretiyle açıldığı için meti… |
| 02 | 3.24 Onay adımı ve yasal metinler | 04 | devir | Kayıt siparişin saklama süresini izler (§4.2 Z-28) ve sipariş sayfasında görünür; kutunun metni `04`'ün işi olduğu için sonradan değişebilir ve geçmiş sipariş kendi metnini göstermeye devam eder. |
| 02 | 3.27 Kurumsal içerik | 04 | devir | Kendi sayfası olmayan kayıtlar — SSS sorusu, şube, duyuru — yöneticiye göründükleri yerde aynı işaretle önizlenir; biçimi `04`'ün işidir. |
| 02 | 3.28 Ana sayfa ve menü | 04 | devir | Blokların düzendeki yeri ve blok başına kaç kayıt görüneceği `04`'ün işidir (K-246). |
| 02 | 3.28 Ana sayfa ve menü | 04 | devir | İskeletin sırası ve görünümü `04`'ün işidir (K-248). |
| 02 | 3.29 Marka kimliği | 04 | devir | Rengin uygulandığı yerler `04`'ün tasarım sisteminin işidir (K-262). |
| 02 | 3.30 Sayfa adresleri, arama motorları ve payl | 07 | devir | `/urun/ahsap-masa`; önekin kendisi `07`'nin işidir). |
| 02 | 3.31 Panelde eşzamanlı düzenleme | 05, 06 | devir | Kural kurumsal içerik, ürün ve ayarlar için aynıdır; mekanizması `05`/`06`'nın eşzamanlılık kararıdır (K-270). |
| 02 | 3.31 Panelde eşzamanlı düzenleme | 05, 06 | devir | Mekanizması `05`/`06`'nın eşzamanlılık kararıdır. |
| 02 | 3.34 Site geneli: cihaz, tarayıcı, erişilebil | 04 | devir | Kırılma noktalarının sayısı ve genişlikleri `04`'ün işidir ve §11'e girmez (K-391, K-463). |
| 02 | 3.34 Site geneli: cihaz, tarayıcı, erişilebil | 12 | devir | Kontrol Listesi'nin A Seviyesi maddeleri ve WCAG 2.2'nin A seviyesi ölçütleri `12`'nin doğrulama kapsamındadır; 2.2'nin A seviyesine eklediği iki ölçüt ayrıca sayılır: 3.2.6 Tutarlı Yardım — iletişim bilgisi ve iletişim … |
| 02 | 3.34 Site geneli: cihaz, tarayıcı, erişilebil | 05, DEPLOY_RUNBOOK | devir | Yedeklemenin sıklığı, yöntemi ve kurtarma süresi `05`'in ve `DEPLOY_RUNBOOK`'un işidir (K-407). |
| 02 | 4.1 Sayım kuralları | 05, 06 | devir | Depolamanın biçimi `05` ve `06`'nın işidir (K-335). |
| 02 | 5. Durum tanımları | 03, 06 | devir | Durum makinesi mantığı burada başlar, `03`'te akışa, `06`'da şemaya döner. |
| 02 | 5. Durum tanımları | 06, 07, 09 | devir | `06`, `07` ve `09` onları birebir devralır (K-17). |
| 02 | 6. Hata ve istisna senaryoları | 04 | devir | Kullanıcıya gösterilen metinlerin biçimi ve yeri `04`'ün işidir. |
| 02 | 6.1 Bütün akışlara uygulanan ilkeler | 05 | devir | Art arda başarısız gönderimde panelin ana sayfasında kanal düzeyinde bir uyarı çıkar (*"e-posta gönderilemiyor — kurulum ayarını kontrol edin"*); eşik `05`'in tasarım girdisidir ve uyarı satış kapısına bağlanmaz — firma … |
| 02 | 6.1 Bütün akışlara uygulanan ilkeler | 08 | devir | Sorgunun teknik biçimi `08`'in işidir (Z-7; |
| 02 | 6.2 Satın alma (akış 1) | 08 | devir | Erişilemezliğin tespiti `08`'in işidir |
| 02 | 6.2 Satın alma (akış 1) | 08 | devir | Sağlayıcıya geri ödeme çağrısının biçimi `08`'in işidir |
| 02 | 8. Kötüye kullanım ve güvenlik kuralları | 05 | devir | Teknik güvenlik — genel istek hızı sınırı, yüklenen dosyanın güvenliği, depolama ve şifreleme — `05`'in işidir (K-331, K-143). |
| 02 | 8.2 Deneme limitleri | 05 | devir | Kupon kodu misafirce de denendiği için sepet ekseninde sayılır — misafirde tarayıcının sepeti, üyede hesabın sepeti; girişte birleşen sepet iki sayacın büyüğünü taşır, mekanizma `05`'tedir (K-330, K-332, K-528, K-555). |
| 02 | 8.2 Deneme limitleri | 05 | devir | Genel istek hızı sınırı bir altyapı kararıdır ve `05`'e aittir (K-331, K-154, K-150, K-53). |
| 02 | 8.5 Kayıtlar ve veri ihlali | 05 | devir | 8.5.1 Ürün ayrı bir ihlal kaydı tutmaz. İhlalin kapsamı üç kayıttan okunur: işlem izi (hangi yönetici neye dokundu), giriş kaydı (§3.13.20 — başarılı ve başarısız girişlerin zamanı, IP'si, hesabı ve sonucu; altı ay sakla… |
| 02 | 9. Bildirimler | 08 | devir | Kanal detayı `08`'de; burada tetikleyici ve alıcı. |
| 02 | 10.5 Manuel adımlar ve bütçe | 03, 12 | devir | Kural ileriye dönüktür: `03`–`12` yazılırken ve implementation sırasında doğan her yeni elle adım önerisi bu bütçeyle birlikte okunur ve sayıyı geçirecek her öneri karar kaydına geri döner (K-404, K-454). |
| 02 | 10.6 Bekleyen işler ve satış özeti | 04 | devir | Dönem seçimi ve ekranın düzeni `04`'ün işidir (K-398, K-441, K-400, K-534, K-643). |
| 02 | 10.6 Bekleyen işler ve satış özeti | 04 | devir | - Sıfır payda: bir oranın paydası seçilen dönemde sıfırsa — ör. hiç sipariş yoksa — oran "değerlendirilemez" sayılır, sıfır yüzde gösterilmez; gösterimi `04`'ün işidir (K-490). |
| 02 | 11. Sayısal parametreler (özet) | 06, 07, 09 | devir | `06`, `07` ve `09` değeri buradan okur; |
| 02 | 11. Sayısal parametreler (özet) | 04 | devir | K-355, K-533); ürünün kendi listeleri — kargo şirketi listesi, resmî tatil listesi, yaygın şifre listesi, konu tipleri ve sebep listeleri — ürünle gelir; tarayıcı tabanı bir kuraldır, sayı değildir; kırılma noktaları `04… |
| 02 | 11.3 Hacim kabulleri | 05 | devir | Elli kaleme kadar sepette ödeme öncesi yeniden değerlendirme kabul edilebilir sürede yanıt verir — yanıt süresinin değeri `05`'in tasarım girdisidir; kalem sayısına tavan yoktur |
| 02 | 11.3 Hacim kabulleri | 05 | devir | `05`'in tasarım girdisidir |
| 02 | 12.2 Kişisel verilerin korunması | 05, DEPLOY_RUNBOOK | devir | Güncellemenin kuruluma hangi yolla ve kimin yetkisiyle ulaştığı `05`'in ve `DEPLOY_RUNBOOK`'un işidir (K-482). |
| 02 | 12.4 Erişilebilirlik | 12 | devir | Genelge beyan değil uygunluk ister; beyan her ekranda her ölçütün denetimini ister ve tutulamayan beyan firmanın üstünde kalırdı — bu yüzden Kontrol Listesi'nin maddeleri `12`'de eşlenir ve yedi kural zorunlu ve test edi… |

---

## 7. Öğrenim terfisi adayları — 5. adımın girdisi

`00 §K` gereği aşama, öğrenimi yazılmadan kapanmaz (K-437). Adaylar karar kaydının beş yerine dağılmış durumda; bu dizin onları tek yerde gösterir, metinleri kendi yerlerinde kalır.

| # | Aday | Kaynak (karar kaydı) | Durum |
|---|---|---|---|
| 1 | `(toplu onayla kaydedildi)` işaretinin işlenişi ve listenin **ne zaman** gösterileceği | K-438 (1); §6.1 2026-09-26 ve 2026-10-01 maddeleri; **bu tarama §4.1** (⚠ öneriyle kayıtta aynı kaçak) | Bekliyor |
| 2 | K-404'ün manuel adım bütçesinin süreç tarafı | K-438 (2) | Bekliyor |
| 3 | K-432'nin yazım/döngü sıralaması `checklists/document-stage.md` §7'ye | K-438 (3) | Bekliyor |
| 4 | §6.2 "Hedef doküman" sütununun blok kapanışında etki birleşimine hizalanması | K-438 (4); §6.1 2026-09-19 ve 2026-09-30 maddeleri | Bekliyor |
| 5 | K-29'un mekanik taraması etki sütununa yazılmamış dokunuşları göremez — anlamsal tarama | §6.1 yazım turu `01` oturumu | Bekliyor |
| 6 | K-29'un blok kapanış taraması işaretin satırdaki **her** bölümü kapsayıp kapsamadığına bakmıyor | §6.1 yazım turu `02` oturumu (1) | Bekliyor |
| 7 | Kapanmamış devirler — blok kapanış taraması devrin varlığını görüyor, kapanışını görmüyor | §6.1 yazım turu `02` oturumu (2) | Bekliyor |
| 8 | Adsız devir — aday listesi işin türünü değil adını görür | §6.1 yazım turu `10` oturumu (1) | Bekliyor |
| 9 | Sonraki karar önceki kuralı daralttığında önceki satır iz taşımıyor | §6.1 yazım turu `10` oturumu (2) | Bekliyor |
| 10 | `01` ve `10`'un şablonunda açık kararlar bölümü yok; K-428 ve K-429 var olmayan bölüme işaret ediyor — bu tarama iki doküman için boş döndü | §6.1 yazım turu `10` oturumu (3) | Bekliyor |
| 11 | Vizyon dokümanına kural ayrıntısı taşınmaz, kurala işaret edilir | §6.1 kalite döngüsü (1) | Bekliyor |
| 12 | Pre-commit sır guard'ı düz metindeki bir sözcüğü sır ataması sanıyor (L5) | §6.1 kalite döngüsü (2) | Bekliyor |
| — | İkinci AI'nın hesap limiti — yedek yöntem | §6.1 kalite döngüsü (3) | **Terfi etti** (SETUP, 2026-10-03) |
| 13 | İkinci model bilinçli karara tekrar tekrar itiraz eder — talimat cümlesi `cross-review` skill'ine | §6.1 kalite döngüsü (4) | Bekliyor |
| 14 | Kural ayrıntısının yasal dayanağı cross-review'da sarsılabilir | §6.1 kalite döngüsü (5) | Bekliyor |
| 15 | Ciddiyet ölçüsü `cross-review` skill'inin istemine ve çıkış koşuluna (K-615, K-644) | §6.1 kalite döngüsü (6) | Bekliyor |
| 16 | Kaynak satırlarının mekanik taraması `cross-review` skill'inin Faz 5'ine | §6.1 kalite döngüsü (7) | Bekliyor |

**Bu taramanın gözlemi — yeni aday değil, 7 ve 16 numaraya kanıt:** cross-review raporunun *"önceki turlardan açık kalanlar"* listesindeki bir not etki sütununa geçmeden altı tur raporda taşındı (§3.3). Bir devir ya da açık not karar satırına bağlanmadıkça raporda kalır; Faz 5'in bu listeyi etki sütununa ya da bir kapanışa bağlaması bu iki adayla birlikte değerlendirilir.

---

## 8. Checkpoint'e notlar

- Tracker §4'te açık satır yok; `02 §13` boş — checkpoint'in açık karar kontrolü bu raporla karşılanır.
- ⚠ öneriyle kayıt listesi (§4.1) checkpoint raporunda adıyla yer alır; proje sahibinin gözden geçirmesi 6. adımdan önce.
- `02` cross-review'ının checkpoint'e bıraktığı iki sıra bozukluğu bu adımda düzeltildi (U-2, U-3); checkpoint'e devreden iş kalmadı.
- Devir dizini (§6) ve öğrenim adayları dizini (§7) 5. ve 6. adımların girdisidir.
