# Aşama 2 — Çakışma Taraması

**Tarih:** 2026-10-04 | **Aşama:** 2 — Kullanıcı Akışları | **Kapanış adımı:** K-437'nin 3. adımı (K-429; `checklists/document-stage.md` §7)
**Girdi sürümleri:** karar kaydı v0.67 · Kullanıcı Akışları (`03`) v0.10 · Ürün Gereksinimleri (`02`) v0.54 · MVP Kapsamı (`10`) v0.36 · Proje Vizyonu (`01`) v0.32 · `DEFERRED_BACKLOG.md` (2026-08-11) · `PLAYBOOK_FEEDBACK.md` (44 satır)
**Çıktı sürümleri:** karar kaydı v0.68 · `03`, `02`, `10`, `01` değişmedi · `04`, `05`, `06`, `07`, `08`, `12` ve `DEPLOY_RUNBOOK`'un park blokları · `PLAYBOOK_FEEDBACK.md` 46 satır

> **Ne işe yarar:** Aşama kapanmadan önce açık kalemlerin her birinin **tek bir evi** ve **gözlemlenebilir bir kapısı** olduğunu doğrular; vadesi geçmiş açıkları adıyla listeler (K-429). Sonuç checkpoint'in (4. adım) girdisidir. Ayrıca Aşama 2 kararlarının sonraki dokümanlara devirlerini tek dizinde toplar ve etki sütununda yalnız doküman numarasıyla duran atıflara işin adını verir (audit bulgusu KAPSAM-K2-5) — Aşama 3'e devrin (6. adım) ve sonraki dokümanların devir taramasının (checklist §4) girdisi.

---

## 1. Sonuç

| Kural | Sonuç |
|---|---|
| **(1) Aynı açık iki yerde durmaz** | ✓ — açık karar yok. Tracker §4'ün on sekiz satırının hepsi kapalı ve Aşama 2'de A-19 açılmadı; `03`'ün şablonunda açık kararlar bölümü yok ve dokümanda "açık" işaretli adım yok (§0.6.1); Ürün Gereksinimleri §13 tablosu boş; MVP Kapsamı'nda açık karar yok. Raporlarda taşınan açık not yok (§3.3). |
| **(2) Her açık satır gözlemlenebilir bir kapı taşır** (K-37) | ⚠ → ✓ — iki süreç maddesinin kapısı açık listede yoktu: **avukat teyidi önerisi** (K-491, K-627) ve **bekletilen öğrenim Ö-23**. İkisi de tracker §8.1'e kapısıyla girdi (§4.2, §4.3). ⚠ listesinin kapısı vardı; kayıtla bire bir olduğu betikle doğrulandı (§4.1). |
| **(3) Vadesi geçmiş ve açık satır adıyla listelenir** | ✓ — **yok.** Açık kalan dört süreç maddesinin kapısı (⚠ listesi, avukat teyidi, öğrenim adayları, playbook listesi) henüz gelmedi. |

**Düzeltilen çakışma: bir** — dört dokümanın K-06 park satırı üç aktör diyordu, K-97 dördüncü aktörü eklemişti (§5.1). **Betikle aranıp temiz dönen çakışma sınıfı: dokuz** (§5.2). **Devir dizini:** Aşama 2'nin altmış üç kararı sonraki dokümanlara 137 atıf taşır; 86'sı yalnız doküman numarasıdır ve dizin her birine işin adını verir (§6.1); `03`'ün gövdesinde iki iş karar satırı olmadan devredilmiş (§6.2). **Yeni karar yok;** çok kritik soru çıkmadı. **Öğrenim terfisi adayları:** on — yedisi aşama boyunca kayda düşmüştü, üçü bu taramada (§7).

---

## 2. Yöntem

Tarama gözle değil betikle yapıldı (K-429). Betikler oturumun çalışma alanındadır; her sayım aşağıdaki çıkarımla tekrarlanabilir.

1. **Tracker §4 ve §8:** `## 4.` ile `## 5.` arasındaki `| A-NN |` satırları sütunlarına ayrıldı, durum hücresi "Kapandı"ya karşı denetlendi. Dosyanın bütün işaretsiz (`- [ ]`) maddeleri listelendi. §8.3'ün altmış iki konu maddesi "Yazıldı" işaretine karşı sayıldı (62 / 62). §8.1'in ⚠ listesindeki K numaraları, karar kaydında konu hücresi `(öneriyle kaydedildi — ⚠)` taşıyan Aşama 2 satırlarıyla iki yönde karşılaştırıldı.
2. **Dokümanların açık kalemleri:** `03` ve `10`'da açık-kalem desenleri arandı — "açık karar", "açık kal-", "belirlenecek", "netleşecek", "karara bağlanacak", "TBD", `A-19`…`A-29`, `(downstream)`; her eşleşme okundu. Ürün Gereksinimleri §13'ün tablo satırları sayıldı.
3. **Devirler:** K-647…K-718'in etki sütunu ` · ` ile parçalandı; `04`–`09`, `11`, `12` ya da `DEPLOY_RUNBOOK` ile başlayan parçalar alındı ve üç türe ayrıldı — **adlı** (parantez içinde işin adı), **park** (parantezde "park") ve **yalnız numara**. Sayım audit'in KAPSAM-K2-5 sayımıyla doğrulandı: K-653…K-700 aralığında 104 atıf, 67'si yalnız numara, 37'si adlı ya da park — bire bir. Yalnız numara taşıyan her atfa, kararın metninden işin adı yazıldı (§6.1). Ayrıca `03`'ün gövdesinde sonraki dokümanı anan cümleler çıkarıldı (§6.2) ve Ürün Gereksinimleri ile MVP Kapsamı'nın sonraki dokümanı anan cümleleri Aşama 1'in kapanış commit'iyle (`d6c772e`) karşılaştırıldı.
4. **Çakışmalar:** `03`'ün kullandığı her kimlik, kod adı ve K numarası kaynağına karşı; geri besleme kararlarının sürümleri `03 §11`, Ürün Gereksinimleri ve MVP Kapsamı'nın başlık notlarına karşı; sayı cümleleri kaynak sayımlarına karşı; park blokları etki sütunlarına karşı denetlendi. Aşama 1'in on altı park satırı (`04`–`09`, `12`), kaynak kararlarını sonradan değiştiren kararlara karşı tarandı (bir sonraki kararın etki ya da karar hücresinde kaynak K numarasının "daralt", "değiş", "genişle", "açık bıraktığı" gibi bir ilişki sözcüğüyle geçmesi).

---

## 3. Açık kalemler

### 3.1 Tracker §4

| Kapsam | Satır | Açık | Sonuç |
|---|---|---|---|
| A-01…A-18 (Aşama 1, salt okunur) | 18 | 0 | Hepsi kapısında ya da kapısından önce kapandı — Aşama 1'in taraması §3.1 |
| A-19 ve sonrası (Aşama 2) | 0 | 0 | Aşama 2'de açık detay doğmadı: yazım ve kalite döngüsünün bulduğu her boşluk oturumunda bir K satırıyla kapandı (`03 §0.6.1`) |

### 3.2 Dokümanlar

| Kaynak | Bulunan | Sonuç |
|---|---|---|
| Kullanıcı Akışları (`03`) | Şablonda açık kararlar bölümü yok; `03 §0.6.2` açık kalemi tracker §4'e gönderir. Desen taramasının on bir eşleşmesi: "açık kalır" sekiz kez — bir yolun açık kaldığını anlatıyor ("kart yolu açık kalır") —; "açık karar", `A-19` ve `(downstream)` birer kez — üçü de §0.6.2'nin kural cümlesi | Açık kalem yok |
| Ürün Gereksinimleri §13 | Tablo boş; §13.4 açıkların tarihçesini taşır ve tablonun neden boş olduğunu yazar | Açık karar yok. Aşama 2'nin geri beslemesi (v0.48…v0.54) §13'e satır açmadı |
| MVP Kapsamı | Açık kararlar bölümü yok (şablonda da yok); "açık karar" üç eşleşmesi v0.2'nin başlık notudur ("`10`'a ait açık karar yoktur"), "açık kalır" iki eşleşmesi gürültü | Açık karar yok |
| Kapsam dışı "Açık" satırları, V-1…V-7, SK-1…SK-10 | Aşama 1'in taraması §3.2'de sınıflandı; Aşama 2 bunlara satır eklemedi | Değişiklik yok |

### 3.3 Raporlarda kalmış açık not

Yok. Cross-review'ın üç turunun "önceki turlardan açık kalanlar" listesi boştu (`03_CROSS_REVIEW.md`, `_R2`, `_R3`). Audit ve deep review'da işlenmeyen üç bulgu karar satırlarına bağlı:

| Bulgu | Bağlandığı yer |
|---|---|
| KAPSAM-K2-5 — sonraki dokümana giden adsız etki atıfları | Öğrenim adayı 7 (PF-44) ve bu raporun §6.1'i |
| YASAL-7 — kurtarma talebinin doğrulanması | K-699'un etki sütunu ve `DEPLOY_RUNBOOK` park satırı |
| DR-8 — K-696'nın kalan riski | K-696'nın etki sütunu: `08` (doğrulama e-postasının uyarı cümlesi) |

---

## 4. Süreç maddeleri ve kapıları

Tracker'ın bütün işaretsiz maddeleri bu taramadan önce iki taneydi, ikisi de §8.1'de: ⚠ listesi ve öğrenim terfisi adayları. Aşama 1'in salt okunur kayıtlarında (§6.1, §7) ve hafızada yaşayan süreç maddeleri ayrıca tarandı.

| Madde | Ev | Kapı (önce) | Sonuç |
|---|---|---|---|
| ⚠ öneriyle kayıt listesi (K-648) | Tracker §8.1 | Aşama 2'nin arşiv işaretinden önce | Kapı var. Liste kayıtla bire bir (§4.1) |
| Öğrenim terfisi adayları 1–7 | Tracker §8.1 · PF-36, PF-37, PF-40…PF-44 | Aşama 2 kapanışının öğrenim terfisi | Kapı var |
| **Avukat teyidi önerisi** — K-491, K-627 | Tracker §7 (Aşama 1, salt okunur) · hafıza | "İlk gerçek kurulumdan önce" — ama hiçbir açık listede değil | **Kapı yazıldı** (§4.2) |
| **Bekletilen öğrenim Ö-23** — checklist'in skill'e dönüşmesi | Tracker §7, 7. satır (salt okunur) · PF-32 | "Aşama 2'nin öğrenim terfisi" — §8.1'in aday listesinde yok | **Listeye girdi** (§4.3) |
| Playbook listesinin gönderimi (K-649) | `PLAYBOOK_FEEDBACK.md` | Proje tamamlandıktan sonra, proje sahibi | Kapı var |
| D-01 — kurulumun ertelenen parçası | `DEFERRED_BACKLOG.md` | Aşama 4 kapanışından sonra, ilk implementation task'ından önce | Kapı var; Aşama 2'den yeni kalem yok |

### 4.1 ⚠ öneriyle kayıt listesi — doğrulama

Konu hücresi `(öneriyle kaydedildi — ⚠)` taşıyan Aşama 2 satırı **39**; §8.1'in listesindeki K numarası **39** — iki küme bire bir, fazla ya da eksik yok. Aşama 2'nin yetmiş iki satırının işaret dağılımı: ⚠ 39 · `(öneriyle kaydedildi)` 30 · işaretsiz 3 (K-647, K-648, K-649 — proje sahibinin açılış kararları). `(toplu onayla kaydedildi)` yok.

| Adım | Kararlar | Sayı |
|---|---|---|
| Workshop | K-656 · K-657 · K-660 · K-662 · K-663 · K-664 · K-665 · K-666 · K-667 · K-668 | 10 |
| Yazım turu, 1.–5. oturum | K-673 · K-678 · K-680 · K-681 · K-683 · K-684 · K-686 · K-687 · K-688 · K-690 · K-692 · K-693 · K-694 · K-696 · K-698 · K-699 | 16 |
| Audit ve deep review | K-703 · K-704 · K-705 · K-706 · K-707 · K-708 · K-709 · K-710 · K-711 · K-712 · K-713 · K-714 · K-716 | 13 |
| **Toplam** | | **39** |

Oturum raporlarında listeler yöneticiye gösterildi; **proje sahibine gösterilmedi.** Kapı değişmedi: liste checkpoint raporunda adıyla yer alır ve arşiv işaretinden (6. adım) önce proje sahibine tek listede — karar başına bir sade cümle — gösterilir (`INSTRUCTIONS.md` §2). İtiraz gelmeyen satırın işareti `(öneriyle kaydedildi — ⚠ — gözden geçirildi YYYY-AA-GG, itiraz yok)` olur; itiraz gelen karar yeni bir satırla değişir.

**Kapının sonucu — 2026-10-04 (K-437'nin 6. adımı):** kapı işledi. Liste arşiv işaretinden önce, checkpoint raporunun §5.1'indeki tabloyla proje sahibine gösterildi; itiraz yok. Otuz dokuz satırın işareti `(öneriyle kaydedildi — ⚠ — gözden geçirildi 2026-10-04, itiraz yok)` oldu; sayım betikle doğrulandı (39 / 39). Madde karar kaydının §8.1'inde kapandı; arşiv işareti aynı PR'da düştü (v0.72).

### 4.2 Avukat teyidi önerisi — kapı yazıldı

Aşama 1'in devir notu (tracker §7) iki kararın — K-491: teslimden sonraki caymada geri ödeme süresinin başlangıcı · K-627: KEP adresi üç firma tipinde zorunlu — bir avukata teyit ettirilmesini proje sahibine öneri olarak yazdı; önerilen kapı "ilk gerçek kurulumdan (MVP Kapsamı ÖK-12) önce". İki sorun vardı:

1. **Öneri karara bağlanmamıştı.** Proje sahibinin kabul ya da reddi kayıtta yok; öneri yalnız salt okunur devir notunda ve hafızanın Güncel Durum bloğunda duruyordu.
2. **Kapı, kaydın ömrünün dışındaydı.** ÖK-12'nin referans kurulumu implementation döneminden sonradır; doküman döneminin karar kaydı o ana kadar arşivlenmiş olur (K-647) ve öneri hiçbir açık listede değildi.

**Yazılan kapı (tracker §8.1, yeni madde):** (1) öneri ⚠ listesiyle aynı mesajda, Aşama 2'nin arşiv işaretinden önce proje sahibine tek cümleyle sunulur — kabul ya da ret; (2) kabul edilirse aynı PR'da `DEFERRED_BACKLOG.md`'ye kalem olarak girer — hedef ÖK-12'den önce, bloklar: MVP kabulü —, teyit itiraz getirirse karar yeni bir satırla değişir; ret gelirse madde gerekçesiyle kapanır. **Kapının sonucu — 2026-10-04 (K-437'nin 6. adımı):** öneri ⚠ listesiyle aynı mesajda sunuldu ve proje sahibince reddedildi (K-721); `DEFERRED_BACKLOG.md`'ye kalem girmedi, madde §8.1'de kapandı. Aşama 2'nin kararlarında benzer bir kalan hukuki risk yazılmadı: audit'in yasal merceği Aşama 2'nin yasal dayanaklarını güncel resmî metne karşı okudu (K-704, K-706…K-709, K-716) ve cross-review'ın 3. turu K-491'in kuralını Yönetmelik m.12/1'in güncel metnine karşı yeniden okudu (`03_CROSS_REVIEW_R3.md`).

### 4.3 Bekletilen öğrenim Ö-23 — aday listesine girdi

Aşama 1'in öğrenim terfisi Ö-23'ü — checklist'in skill'e dönüştürülmesi (`00 §C.7`) — *"kapı: Aşama 2'nin öğrenim terfisi"* diye bekletti (`PHASE1_LEARNING_PROMOTION.md`, PF-32). Kayıt Aşama 1'in salt okunur devir notunda ve playbook listesinde duruyordu; Aşama 2'nin öğrenim terfisinin dizini tracker §8.1'in aday listesinden toplanacak (checklist §7, 5. adım) ve Ö-23 orada yoktu. Listeye 8. aday olarak girdi; kapısı değişmedi.

---

## 5. Çakışmalar

### 5.1 Düzeltilen çakışma

| # | Yer | Çakışma | Düzeltme |
|---|---|---|---|
| U-1 | `04`, `06`, `07`, `12` — "Aşama 1'den park edilen girdiler", K-06 satırı | Park satırı **üç aktör** diyordu (ziyaretçi · üye müşteri · firma yöneticisi); `04`'ünki ayrıca *"misafir alıcı henüz aktör değildir; `B4-01` üyeliksiz sipariş kararına bağlıdır"* diyordu. K-97 misafir alıcıyı **dördüncü aktör** yaptı ve K-06'nın açık bıraktığı envanteri kapattı; Proje Vizyonu §3.2, Ürün Gereksinimleri §1.3 ve `03 §0.1.1` dört aktör der. K-06'nın etki sütununda K-97'ye geri işaret yoktu. Ekran × rol matrisi (`04`), yetki modeli (`06`, `07`) ve doğrulama senaryoları (`12`) park satırından kurulsaydı misafir alıcının akışları dışarıda kalırdı | Dört park satırı dört aktörü yazar ve kaynağını gösterir (`02 §1.3`; K-97); K-06'nın etki sütununa geri işaret düştü — Aşama 1 kaydının tek istisnası (tracker başlığı). Yeni karar değildir: K-97 kayıtlı karardır |

Aynı taramanın öteki adayları park satırının söylediğini değiştirmiyor: K-14'ün park satırı (`04`) kimlik setinin tipe göre değiştiğini söyler — K-259 ve K-480 seti genişletti, satır yine doğru; K-17'nin üç park satırı (`06`, `07`, `09`) sözlüğün birebir devralınmasını söyler — K-526'nın daralttığı yalnız K-17'nin örnek notudur; K-12'nin park satırı (`08`) kart verisinin sınırını söyler — K-109'un "yüzeyi daraltır" ilişkisi yeniden doğrulama setiyle ilgilidir.

### 5.2 Betikle aranan ve temiz dönen çakışma sınıfları

| # | Sınıf | Sonuç |
|---|---|---|
| 1 | **Kimlikler** — `03`'ün kullandığı B-, F-, Z-, L-, P-, H-, S, Ö, KP-, KD-, ÖK-, SK- kimliklerinin her biri kaynağında (`02`, `10`) var | ✓ — `03 §0.3.3`'ün aralıkları (B-1…B-16, F-1…F-6, Z-1…Z-47, L-1…L-9, P-1…P-49, S1…S11, Ö1…Ö5) kaynağın en büyük kimliğine eşit; kaldırılmış P-2 ve P-11 `03`'te geçmiyor |
| 2 | **Kod adları** — `03`'teki İngilizce karşılıklar `02 §1.2` sözlüğünde | ✓ — 22 / 22 |
| 3 | **K atıfları** — `03`, `02` ve `10`'daki her K numarası kayıtta var; etki sütunu `03`'ü gösteren Aşama 2 kararı `03`'te anılıyor | ✓ — `03`'te 95 K numarası; etki sütunu `03`'ü gösteren 69 Aşama 2 kararının 69'u `03`'te; `02`'de 59, `10`'da 49 Aşama 2 atfı, hepsi kayıtta |
| 4 | **Geri besleme kümesi** — `02`'ye ya da `10`'a dönen her Aşama 2 kararı `03 §11`'de | ✓ — `02`'ye dönen 58, `10`'a dönen 32 karar; hepsi §11'in 45 satırında |
| 5 | **Geri besleme sürümleri** — §11 satırının sürüm hücresi, karar satırının sürümü ve iki dokümanın başlık notu | ✓ — `02`'nin v0.48…v0.54 notları karar kümeleriyle bire bir (16 · 7 · 4 · 7 · 3 · 7 · 15). `10`'un v0.30…v0.36 notlarındaki fazla K numaraları notun kendisinin *"bir kapsam satırı değiştirmez"* diye andığı kararlardır. §11'in dört çok kararlı satırında (11.2, 11.3, 11.28, 11.30) `10` sürümü satırdaki kararlardan yalnız birine aittir — satır birleşimidir, çakışma değil |
| 6 | **Sayı cümleleri** | ✓ — KP-64 "kırk yedi parametre" = `02 §11`'in 49 kimliği − kaldırılmış 2 (P-2, P-11); KP-66 "on altı olay" = B-1…B-16; KP-48, `02 §10.4.1` ve `03 §8.4` dokuz müdahale; Proje Vizyonu §3.2, `02 §1.3` ve `03 §0.1.1` dört aktör |
| 7 | **Aşama 2 park blokları** — etki sütununda "park" taşıyan kararlar ile blok satırları | ✓ — beş karar, bire bir: K-666, K-667, K-687 → `05`; K-705 → `08`; K-699 → `DEPLOY_RUNBOOK`. Ürün Gereksinimleri'nin Aşama 2 geri beslemesinde sonraki dokümana iş bırakan dört yeni cümle (§4.3, §7.2, §8.5, §10.2) bu beş kararın park satırlarıdır |
| 8 | **Doküman sürümleri** — tracker §1, başlıklar, dosya sonu dipnotları | ✓ — Proje Vizyonu v0.32, Ürün Gereksinimleri v0.54, MVP Kapsamı v0.36, Kullanıcı Akışları v0.10 |
| 9 | **Konu planı** — tracker §8.3'ün konuları ve §8.2'nin blok durumu | ✓ — 62 konunun 62'si yazıldı; on iki bloğun on ikisi ✓ |

**Uyumsuzluk sayılmayan tekrar:** sonraki dokümana bırakılan bazı işler `03`'ün gövdesinde ve Ürün Gereksinimleri'nde aynı cümleyle geçer — kesintinin tespiti (`02 §4.3`, `03 §4.2.12`, §10), giriş kaydının okuma yolu (`02 §8.5`, `03 §6.3.2.3`), yedekleme (`02 §3.34.8`, `03 §10.2.4`). Devrin evi karar satırının etki sütunu ya da park satırıdır; cümleler aynı şeyi söylüyor. Çakışma iki yerin farklı şey söylemesidir; burada yok.

---

## 6. Devir dizini — Aşama 2 kararlarının sonraki dokümanlara devirleri

Sonraki dokümanlar henüz yazılmadığı için devirler kararların etki sütunlarında ve park bloklarında yaşar. **Tek ev:** devrin evi karar satırının etki sütunu ya da park satırıdır; bu dizin devri ikinci kez kaydetmez. Yeni olan, etki sütununda **yalnız doküman numarası** taşıyan atfa dizinin verdiği **işin adıdır** (checklist §3: *"devir yazılırken işin adı yazılır"*) — karar satırlarının metni değişmedi. Her hedef dokümanın "Aşama 2'den park edilen girdiler" bloğu bu bölüme işaret eder (`04`, `06`, `07` ve `12`'de blok bu taramada açıldı).

- **Aşama 1'in kararları:** dizini `PHASE1_CONFLICT_SCAN.md` §6'dadır; Aşama 1'in yalnız numaralı atıflarına ad verilmedi.
- **`09` Kodlama Kılavuzu ve `11` Uygulama Planı:** Aşama 2'den devir yok.
- **`DEFERRED_BACKLOG.md`:** Aşama 2'den kalem yok; avukat teyidi kabul edilirse girer (§4.2) — öneri reddedildi (K-721), kalem girmedi.
- **Toplandığı yer (6. adım, 2026-10-04):** Aşama 3'e devrin bütün girdileri — bu dizin dahil — karar kaydının §9'unda tek tabloda.
- **Süreç kararları** (K-647, K-648, K-649, K-718) sonraki dokümana iş bırakmaz.
- **K-701'in parantezi:** etki sütunu `04 · 05 · 06 · 07 · 08 (atıfların okunuşu)` diye yazılıdır; parantez beş atfın hepsine aittir — betik dördünü yalnız numara saydı, dizin onlara aynı adı verdi.

### 6.1 Etki sütunlarındaki devirler (K-647…K-718)

Hedef doküman başına bir tablo. **Kaynağı** sütunu: *etki sütunu* — işin adı kararın etki sütunundaki parantezdir; *park satırı* — iş hedef dokümanın park bloğundadır; *dizin* — etki sütununda yalnız doküman numarası vardır ve işin adını bu dizin kararın metninden verdi. `12`'de iş, kararın doğrulama senaryosunun konusudur. Sayım betikle: 63 karar, 137 atıf.

| Hedef | Atıf | Karar | Adlı | Park | Yalnız numara → dizin adı verdi |
|---|---|---|---|---|---|
| `04` Arayüz Tanımları | 35 | 35 | 27 | 0 | 8 |
| `05` Teknik Mimari | 7 | 7 | 1 | 3 | 3 |
| `06` Veri Modeli | 23 | 23 | 7 | 0 | 16 |
| `07` API Tasarımı | 1 | 1 | 0 | 0 | 1 |
| `08` Entegrasyon Spesifikasyonu | 12 | 12 | 11 | 1 | 0 |
| `12` Doğrulama Protokolü | 58 | 58 | 0 | 0 | 58 |
| `DEPLOY_RUNBOOK.md` | 1 | 1 | 0 | 1 | 0 |
| **Toplam** | **137** | **63** | **46** | **5** | **86** |

#### `04` Arayüz Tanımları — 35 atıf

| Karar | İş | Kaynağı |
|---|---|---|
| K-653 | panelde ayrılmış adet ve reddin mesajı | etki sütunu |
| K-654 | kupon formunda kullanılmış ve ayrılmış hak sayısı; altına inen adedin ret mesajı | dizin (etki sütununda yalnız `04`) |
| K-655 | silme onayının metni | etki sütunu |
| K-656 | iade teslim alma adımında "iade reddedildi — koruyucu ambalaj açılmış" seçimi ve sonucunu söyleyen geri alınamaz onay | dizin (etki sütununda yalnız `04`) |
| K-657 | sipariş sayfasında reddedilen iade kaleminin görünümü (ret sebebi, geri ödeme yapılmayacağı) | dizin (etki sütununda yalnız `04`) |
| K-659 | yeniden isteme mesajı | etki sütunu |
| K-660 | davet satırında geri çekme | etki sütunu |
| K-662 | kaldırma onayının metni | etki sütunu |
| K-663 | havale IBAN'ı değişikliğinin onayında ödemesi beklenen sipariş sayısı; sipariş sayfasında ve "ödendi" işaretinde siparişin donmuş IBAN'ı | dizin (etki sütununda yalnız `04`) |
| K-664 | havaleyi kapatma onayında ödemesi beklenen havale siparişlerinin sayısı | dizin (etki sütununda yalnız `04`) |
| K-665 | beyan ekranında güncel iade adresi, sipariş sayfasında beyanın adresi; iade adresi değişikliğinde "iade malı bekleniyor" kalem sayısı | dizin (etki sütununda yalnız `04`) |
| K-669 | izlenebilirlik matrisinin atıf biçimi | etki sütunu |
| K-673 | sipariş sayfasında düğmenin görünürlüğü | etki sütunu |
| K-674 | silmenin engellenme mesajı | etki sütunu |
| K-675 | işaret varyantları | etki sütunu |
| K-676 | sipariş sayfasında talebin görünümü | etki sütunu |
| K-677 | onay penceresi | etki sütunu |
| K-678 | formun gönderim sonrası ekranı | etki sütunu |
| K-679 | kupon alanı | etki sütunu |
| K-680 | deftere kaydetme seçimi | etki sütunu |
| K-686 | IBAN isteği | etki sütunu |
| K-690 | IBAN alanı | etki sütunu |
| K-697 | yönlendirme mesajı | etki sütunu |
| K-698 | engelin mesajı | etki sütunu |
| K-700 | aktör listesi | etki sütunu |
| K-701 | izlenebilirlik matrisinde `03` atıflarının okunuşu (önsüz `§`'in devralınması) | dizin (etki sütununda yalnız `04`) |
| K-702 | aktör listesi | etki sütunu |
| K-703 | düzeltmenin engel mesajı | etki sütunu |
| K-704 | teyit ve bekleme ekranı | etki sütunu |
| K-706 | kayıt ekranı | etki sütunu |
| K-709 | uyarının metni | etki sütunu |
| K-712 | iade malının ulaşma tarihinin düzeltme ekranı (geçmişe dönük, ileri tarihsiz) | dizin (etki sütununda yalnız `04`) |
| K-714 | sipariş listesi | etki sütunu |
| K-715 | onay metni | etki sütunu |
| K-717 | kapatma düğmesi | etki sütunu |

#### `05` Teknik Mimari — 7 atıf

| Karar | İş | Kaynağı |
|---|---|---|
| K-658 | L-9'un iki sayacı — e-posta ve IP ekseni (P-49) | dizin (etki sütununda yalnız `05`) |
| K-666 | kaçan işlerin dönüşte sırayla çalışması | etki sütunu — **park satırı** |
| K-667 | kesintinin tespiti | etki sütunu — **park satırı** |
| K-687 | giriş kaydının ve sistem kayıtlarının okuma yolu | etki sütunu — **park satırı** |
| K-699 | kurulum tarafının yeni yönetici açması ve ele geçirilmiş hesabın oturumlarını sonlandırmasının teknik yolu (yöntemi `DEPLOY_RUNBOOK` park satırında) | dizin (etki sütununda yalnız `05`) |
| K-701 | `03` atıflarının okunuşu (önsüz `§`'in devralınması) | dizin (etki sütununda yalnız `05`) |
| K-711 | sayaç | etki sütunu |

#### `06` Veri Modeli — 23 atıf

| Karar | İş | Kaynağı |
|---|---|---|
| K-653 | varyantın ayrılmış adedi ve onu tutan siparişler; stok ve kontenjanın ayrılmış adedin altına inmemesi | dizin (etki sütununda yalnız `06`) |
| K-654 | kuponun kullanılmış ve ayrılmış hak sayıları | dizin (etki sütununda yalnız `06`) |
| K-655 | silinen ürüne ya da varyanta bağlı açık siparişlerin sayımı; ayırmanın varyantla düşmesi, siparişin donmuş kalemle sürmesi | dizin (etki sütununda yalnız `06`) |
| K-656 | ret kaydı | etki sütunu |
| K-659 | kayıt başına tek geçerli doğrulama bağlantısı; ömrün ilk gönderimden işlemesi (Z-1) | dizin (etki sütununda yalnız `06`) |
| K-660 | yönetici davetinin geri çekilme kaydı | dizin (etki sütununda yalnız `06`) |
| K-661 | adres başına tek geçerli yönetici daveti | dizin (etki sütununda yalnız `06`) |
| K-663 | siparişte IBAN | etki sütunu |
| K-665 | beyan kaydında adres | etki sütunu |
| K-671 | hizmet tamamlama işaretinin ödeme koşulu (ödeme Bekliyor ya da Başarısız değilken) | dizin (etki sütununda yalnız `06`) |
| K-672 | kapanış kaydı | etki sütunu |
| K-675 | panel işaretlerinin durum değil koşuldan türemesi ve kalkış koşulları | dizin (etki sütununda yalnız `06`) |
| K-676 | kalem başına tek ayıp talebi kaydı | dizin (etki sütununda yalnız `06`) |
| K-679 | siparişte en fazla bir kupon kodu | dizin (etki sütununda yalnız `06`) |
| K-680 | siparişe donan adres ve adres defterine isteğe bağlı kayıt | dizin (etki sütununda yalnız `06`) |
| K-685 | havale hatırlatmasının ödeme süresi başına bir kez gitmesi; Z-9'un düzeltmede yeniden başlaması | dizin (etki sütununda yalnız `06`) |
| K-686 | yöneticinin açtığı IBAN isteği kaydı ve "IBAN bekleniyor" listesi | dizin (etki sütununda yalnız `06`) |
| K-695 | şifre değişikliğinde öteki oturumların ve tanınan tarayıcı işaretlerinin düşmesi | dizin (etki sütununda yalnız `06`) |
| K-696 | Google ile ilk girişte doğrulanmamış bekleyen kaydın silinmesi ve hesabın şifresiz doğması | dizin (etki sütununda yalnız `06`) |
| K-701 | `03` atıflarının okunuşu (önsüz `§`'in devralınması) | dizin (etki sütununda yalnız `06`) |
| K-707 | talebin tipi saklamayı belirler | etki sütunu |
| K-708 | kayıt türleri | etki sütunu |
| K-713 | kalemin alanı | etki sütunu |

#### `07` API Tasarımı — 1 atıf

| Karar | İş | Kaynağı |
|---|---|---|
| K-701 | `03` atıflarının okunuşu (önsüz `§`'in devralınması) | dizin (etki sütununda yalnız `07`) |

#### `08` Entegrasyon Spesifikasyonu — 12 atıf

| Karar | İş | Kaynağı |
|---|---|---|
| K-657 | B-16 e-postası | etki sütunu |
| K-665 | B-9 e-postası | etki sütunu |
| K-681 | B-9 e-postası | etki sütunu |
| K-684 | B-8 e-postasının bu hâldeki metni | etki sütunu |
| K-689 | e-posta şablonları haritadan türer | etki sütunu |
| K-690 | B-14 metni | etki sütunu |
| K-691 | F-4 metni | etki sütunu |
| K-692 | F-6 e-postası | etki sütunu |
| K-693 | e-postanın metni | etki sütunu |
| K-696 | doğrulama e-postasının uyarı cümlesi | etki sütunu |
| K-701 | atıfların okunuşu | etki sütunu |
| K-705 | "Aşama 2'den park edilen girdiler" | etki sütunu — **park satırı** |

#### `12` Doğrulama Protokolü — 58 atıf

| Karar | İş | Kaynağı |
|---|---|---|
| K-653 | stok ya da kontenjan ayrılmış adedin altına indirilemez | dizin (etki sütununda yalnız `12`) |
| K-654 | kuponun kullanım adedi ayrılmış hakların altına indirilemez | dizin (etki sütununda yalnız `12`) |
| K-655 | açık siparişli ürünün ya da varyantın silinmesi — sayı uyarısı, donmuş kalemle süren sipariş | dizin (etki sütununda yalnız `12`) |
| K-656 | koşullu istisna kaleminde iade reddi ve sonuçları | dizin (etki sütununda yalnız `12`) |
| K-657 | reddedilen malın müşteriye dönüşü ve B-16 | dizin (etki sütununda yalnız `12`) |
| K-658 | doğrulama bağlantısının yeniden istenmesi limiti L-9 (P-49) | dizin (etki sütununda yalnız `12`) |
| K-659 | yeniden istenen doğrulama bağlantısı öncekileri geçersiz kılar, ömrü uzatmaz | dizin (etki sütununda yalnız `12`) |
| K-660 | yönetici davetinin geri çekilmesi | dizin (etki sütununda yalnız `12`) |
| K-661 | aynı adrese ikinci davet öncekini geçersiz kılar | dizin (etki sütununda yalnız `12`) |
| K-662 | gönderen yönetici kaldırılınca davetleri düşer | dizin (etki sütununda yalnız `12`) |
| K-663 | havale IBAN'ı siparişe donar | dizin (etki sütununda yalnız `12`) |
| K-664 | satış kapısının bir koşulunun düşmesi açık siparişleri etkilemez | dizin (etki sütununda yalnız `12`) |
| K-665 | beyana yazılan iade adresi sonradan değişmez | dizin (etki sütununda yalnız `12`) |
| K-666 | kesintide süreler dondurulmaz; kaçan işler dönüşte sırayla çalışır | dizin (etki sütununda yalnız `12`) |
| K-667 | kesintide dolan havale süresinde kendiliğinden iptal ertelenir | dizin (etki sütununda yalnız `12`) |
| K-668 | kullanılmış ya da hasarlı dönen malda tam geri ödeme | dizin (etki sütununda yalnız `12`) |
| K-671 | hizmet tamamlama ve S2'nin ödeme koşulu | dizin (etki sütununda yalnız `12`) |
| K-672 | Alındı ya da Hazırlanıyor'da son kalemin cayma ya da çıkarmayla kapanışı (S3, S4) | dizin (etki sütununda yalnız `12`) |
| K-673 | hizmet kaleminde cayma düğmesi ödeme onayından sonra açılır | dizin (etki sütununda yalnız `12`) |
| K-674 | yayındaki ürünün son Yayında varyantı silinemez | dizin (etki sütununda yalnız `12`) |
| K-675 | panel işaretlerinin kalkışı | dizin (etki sütununda yalnız `12`) |
| K-676 | kalem başına tek ayıp talebi | dizin (etki sütununda yalnız `12`) |
| K-677 | yeniden gönderimin (S8) geri alınamaz onayı | dizin (etki sütununda yalnız `12`) |
| K-678 | iletişim formunu gönderene e-posta gitmez | dizin (etki sütununda yalnız `12`) |
| K-679 | siparişe tek kupon kodu | dizin (etki sütununda yalnız `12`) |
| K-680 | ödeme adımında yeni adres ve deftere kaydetme seçimi | dizin (etki sütununda yalnız `12`) |
| K-681 | müşterinin ayıp talebini yeniden açmasında B-9 | dizin (etki sütununda yalnız `12`) |
| K-683 | beş hata ve sistem olayının bildirim üretmemesi | dizin (etki sütununda yalnız `12`) |
| K-684 | iptal edilmiş siparişe gelen kart ödemesinin iadesinde B-8 | dizin (etki sütununda yalnız `12`) |
| K-685 | düzeltmeyle yeniden başlayan havale süresinde B-3 bir kez daha | dizin (etki sütununda yalnız `12`) |
| K-686 | havale hattında IBAN'ı olmayan geri ödemede IBAN isteği | dizin (etki sütununda yalnız `12`) |
| K-687 | giriş kaydı panelde görünmez | dizin (etki sütununda yalnız `12`) |
| K-688 | cayma beyanı geri alınmaz ve düzeltilmez | dizin (etki sütununda yalnız `12`) |
| K-689 | bildirim haritası ile akışların bildirim hücrelerinin iki yönlü eşleşmesi | dizin (etki sütununda yalnız `12`) |
| K-690 | firma iptalinde ve kalem çıkarmasında havale IBAN isteği ve B-14 | dizin (etki sütununda yalnız `12`) |
| K-691 | F-4 her kendiliğinden kart iadesinde gider | dizin (etki sütununda yalnız `12`) |
| K-692 | F-6 — yönetici daveti ve kaldırma bütün yöneticilere | dizin (etki sütununda yalnız `12`) |
| K-693 | "hesabınız silindi" e-postası | dizin (etki sütununda yalnız `12`) |
| K-694 | hesap olaylarında bildirim yokluğu | dizin (etki sütununda yalnız `12`) |
| K-695 | şifre değişikliği öteki oturumları kapatır | dizin (etki sütununda yalnız `12`) |
| K-696 | Google ile ilk girişte doğrulanmamış bekleyen kayıt devralınmaz | dizin (etki sütununda yalnız `12`) |
| K-697 | Google doğrulanmamış e-posta verirse hesap açılmaz | dizin (etki sütununda yalnız `12`) |
| K-698 | geri alma bağlantısı açıkken hesap silinmez | dizin (etki sütununda yalnız `12`) |
| K-699 | panele erişim kaybında ürün içinde kurtarma yolu yoktur | dizin (etki sütununda yalnız `12`) |
| K-703 | durum düzeltmesinin koşulları ve yanlış aralıkta doğmuş kalem kayıtları | dizin (etki sütununda yalnız `12`) |
| K-704 | siparişin alındığının sitede gösterilmesi ve kart dönüşü (Hizmet Sağlayıcılar Yönetmeliği m.9/1) | dizin (etki sütununda yalnız `12`) |
| K-705 | "geri ödeme gerçekleşmedi" uyarısının ters ibraz ve iade hatırlatması | dizin (etki sütununda yalnız `12`) |
| K-706 | başka kanaldan gelen gecikme feshinin kaydı | dizin (etki sütununda yalnız `12`) |
| K-707 | "Sipariş hakkında" iletişim talebi üç yıl saklanır | dizin (etki sütununda yalnız `12`) |
| K-708 | kişisel veriyi hemen silen işlemlerin imha kaydı | dizin (etki sütununda yalnız `12`) |
| K-709 | başka kanaldan caymada süreye uygunluk uyarısı | dizin (etki sütununda yalnız `12`) |
| K-710 | müşterinin IBAN girişi bildirim üretmez | dizin (etki sütununda yalnız `12`) |
| K-711 | yeniden doğrulamadaki başarısız şifre denemesi L-1'e sayılır | dizin (etki sütununda yalnız `12`) |
| K-712 | iade malının ulaşma tarihinin düzeltilmesi ve Z-16 | dizin (etki sütununda yalnız `12`) |
| K-713 | indirme hakkının siparişe donması | dizin (etki sütununda yalnız `12`) |
| K-714 | panelde sipariş arama ve durum süzgeci | dizin (etki sütununda yalnız `12`) |
| K-716 | ayıp çözümünde derhâl geri ödeme yükümlülüğü (süre sayacı yok) | dizin (etki sütununda yalnız `12`) |
| K-717 | Kargoya verildi ya da Teslim edilemedi'de son kalemin kapanışı | dizin (etki sütununda yalnız `12`) |

#### `DEPLOY_RUNBOOK.md` — 1 atıf

| Karar | İş | Kaynağı |
|---|---|---|
| K-699 | kurtarmanın yöntemi; §H'nin ilk yönetici yolu | etki sütunu — **park satırı** |

### 6.2 Kullanıcı Akışları'nın gövdesinde sonraki dokümanı anan cümleler

Mekanik çıkarım; başlıktaki sürüm notları hariç. **Devir:** işi o dokümana bırakan cümle. **Anma:** sonraki dokümanı yalnız anar ya da bir park satırını gösterir. **Dayanak:** devrin karar satırı ya da üst dokümandaki evi; **yalnız `03`** — devrin karar satırı da üst dokümanda evi de yok, tek yazılı yeri bu cümledir.

| `03` | Hedef | Tür | İş | Dayanak |
|---|---|---|---|---|
| 0.1.4 | 04, 05, 12 | devir | `10 §2`'nin akış doğurmayan dört satırının (KP-37, KP-69, KP-70, KP-71) karşılığı ve doğrulaması | `10 §2` KP-37, KP-69…KP-71; `02 §3.34`, §12.4 — Aşama 1'in dizini §6.2 |
| 0.6.2 | 05 | anma | park bloğunun biçimi | — |
| 2.1.2 | 04 | devir | indirimdeki ürünün vitrin kartının düzeni | `02 §3.9.2` (K-545) |
| 2.5.1.2 | 08 | devir | kart ödemesinin başarı bildiriminin doğrulanması ve tutar eşleşmesi | K-705 — park |
| 3.1.6 | 04 | devir | hata mesajlarının biçimi | `02 §6` |
| 3.2.1.15 | 08 | devir | ödeme sağlayıcısının erişilemezliğinin tespiti | `02 §6.2` |
| 4.2.12 | 05 | devir | sitenin kesintisinin tespiti | K-667 — park |
| §6 girişi | 05 | devir | teknik güvenlik — genel istek hızı sınırı, dosya güvenliği, depolama | `02 §8` |
| 6.1.2.4 | 05 | devir | genel istek hızı sınırı | `02 §8.2` |
| 6.3.2.3 | 05 | devir | giriş kaydının ve sistem kayıtlarının okuma yolu | K-687 — park |
| §7 girişi | 08 | devir | e-postanın metni, şablonu ve gönderim biçimi | `02 §9`; K-689 (`08` — "e-posta şablonları haritadan türer") |
| §7 girişi | 08 | anma | e-postanın adı `02 §9`'dan alınır | — |
| 7.1.37 | 04, 08 | devir | **yeniden gönderilebilen e-postaların kapsamı** — firma bildirimleri dahil | **yalnız `03`** — `02 §9.1.6` "en az B-1 için" der, kapsamın genişliğini bir dokümana bırakmaz |
| 8.2.11 | 04 | devir | panelde sipariş arama ekranı | K-714 (`04` — "sipariş listesi") |
| 8.9.2 | 04, 08 | devir | yeniden gönderimin kapsamı ve firma bildirimlerinin "e-posta ulaşmadı" işaretinin biçimi | **yalnız `03`** — 7.1.37 ile aynı kök |
| §10 girişi | 08, 05 | devir | sağlayıcının erişilemezliğinin ve sitenin kesintisinin teknik tespiti | `02 §6.2`; K-667 — park |
| 10.1.1.1 | 08 | devir | sağlayıcının erişilemezliğinin tespiti | `02 §6.2` |
| 10.1.1.7 | 08, DEPLOY_RUNBOOK | devir | **ödeme sağlayıcısının anahtarlarının ya da sağlayıcının değişmesi** — açık kart siparişlerine (bekleyen son sorgu, geç gelen ödeme, kendiliğinden kart iadesi) etkisi ve geçişin yöntemi | **yalnız `03`** — satırı audit ekledi (v0.7), karar satırı yok; Kaynak `02 §3.21.4`, §10.1.3 anahtarların kurulum ayarı olduğunu söyler, değişmenin açık siparişlere etkisini yazmaz |
| 10.1.2.2 | 04 | devir | firma bildirimlerinin (F-1…F-4) "e-posta ulaşmadı" işaretinin biçimi | **yalnız `03`** — 7.1.37 ile aynı kök |
| 10.1.2.3 | 05 | devir | e-posta kanalı uyarısının eşiği | `02 §6.1` |
| 10.2.4 | 05, DEPLOY_RUNBOOK | devir | yedeklemenin sıklığı, yöntemi ve kurtarma süresi | `02 §3.34.8` (K-407) |
| 10.4 | DEPLOY_RUNBOOK | devir | panele erişimin kaybında kurtarmanın yöntemi | K-699 — park |
| 11.6 · 11.22 · 11.30 · 11.33 | 05, DEPLOY_RUNBOOK, 08 | anma | §11'in "park satırı" işaretleri | K-667, K-687, K-699, K-705 |

**Karar satırı olmadan devredilen iki iş:** (a) **yeniden gönderilebilen e-postaların kapsamı ve firma bildirimlerinin işaretlenme biçimi** → `04`, `08` (7.1.37, 8.9.2, 10.1.2.2); (b) **ödeme sağlayıcısının değişmesinin açık kart siparişlerine etkisi ve geçişin yöntemi** → `08`, `DEPLOY_RUNBOOK` (10.1.1.7). İkisi de detaydır — varlık kararları kayıtlıdır (yeniden gönderim `02 §9.1.6`; kurulum ayarı `02 §10.1.3`) — ve kapısı hedef dokümanın yazımıdır. İkisi yeni bir karar istemedi; hedef dokümanların park bloğundaki dizin satırı onları adıyla anar. Kural 1 açısından açık kalem değildir: tek evleri bu cümlelerdir ve tracker §4'te kopyaları yoktur.

**Üst dokümanların Aşama 2'de eklenen devir cümleleri:** Ürün Gereksinimleri'nde dört — `02 §4.3` kesintinin tespiti (`05`), §7.2 sonucu bilinmeyen iade isteği (`08`), §8.5 giriş kaydının okuma yolu (`05`), §10.2 kurtarmanın yöntemi (`DEPLOY_RUNBOOK`); dördü de park satırıdır (K-666/K-667, K-705, K-687, K-699). MVP Kapsamı ve Proje Vizyonu'nda yeni devir cümlesi yok (Aşama 1'in kapanış commit'iyle karşılaştırma: 9 → 9, 7 → 7).

---

## 7. Öğrenim terfisi adayları — 5. adımın girdisi

`00 §K` gereği aşama öğrenimi yazılmadan kapanmaz (K-437). Adayların metinleri tracker §8.1'dedir; bu dizin onları tek yerde gösterir.

| # | Aday | Kaynak | Playbook | Durum |
|---|---|---|---|---|
| 1 | Şablonun karar kaydı başlığı iki okumaya açık | K-647 | PF-36 | Bekliyor |
| 2 | Devir notundaki soru şablona karşı taranmadan soruldu | K-647 | PF-37 | Bekliyor |
| 3 | Kullanıcı Akışları şablonunun iki eksiği (anlatı bölümü, açık kararlar bölümü) | Konu planı | PF-40 | Bekliyor |
| 4 | Aşama planının konu kimliği aşamayı taşımıyor | Konu planı | PF-41 | Bekliyor |
| 5 | Ön sayım betiğinin eşleşmesi doğrulanmadan kullanıldı | `03`'ün audit'i | PF-42 | Bekliyor |
| 6 | Paralel mercek sayısı oturum limitine bağlı | `03`'ün audit'i | PF-43 | Bekliyor |
| 7 | Sonraki dokümana giden etki atıfları adsız kalıyor | KAPSAM-K2-5 | PF-44 | Bekliyor — bu raporun §6.1'i Aşama 2'nin 86 adsız atfına ad verdi; kural hâlâ yok |
| 8 | Bekletilen öğrenim Ö-23 — checklist'in skill'e dönüştürülmesi | Aşama 1 öğrenim terfisi; **bu tarama** (§4.3) | PF-32 | Bekliyor |
| 9 | **Park satırı kaynak kararı değişince güncellenmedi** — geri işaret kuralı park satırını kapsamıyor | **Bu tarama** (§5.1) | PF-45 | Bekliyor |
| 10 | **Salt okunur aşama kaydındaki açık süreç maddesi sonraki aşamanın listesine geçmedi** | **Bu tarama** (§4.2, §4.3) | PF-46 | Bekliyor |

**Bu taramanın gözlemi — yeni aday değil, 7 numaraya kanıt:** etki sütunundaki adsız atıfların dışında, `03`'ün gövdesinde karar satırı olmadan iki iş devredilmiş (§6.2). Devir taraması (checklist §4) yalnız kaydı okursa bunları da görmez; 7 numaranın kuralı yazılırken gövde cümleleri de kapsanır.

---

## 8. Checkpoint'e notlar

- Tracker §4'te açık satır yok, A-19 açılmadı; Ürün Gereksinimleri §13 boş, `03` ve MVP Kapsamı'nda açık kalem yok — checkpoint'in açık karar kontrolü bu raporla karşılanır.
- ⚠ listesi (39 karar, §4.1) checkpoint raporunda adıyla yer alır; proje sahibine 6. adımdan önce gösterilir. **Avukat teyidi önerisi aynı mesajda sunulur** (§4.2) — kabul edilirse `DEFERRED_BACKLOG.md`'ye girer.
- K-06 ↔ K-97 çakışması (§5.1) düzeltildi; checkpoint'e devreden iş kalmadı. Aşama 1 park satırlarının sonraki kararlara karşı taranması bu raporda bir kez yapıldı; kuralı öğrenim adayı 9'dur.
- 6. adımda Aşama 3'e devrin tablosu bu raporun §6'sını ve hedef dokümanların park bloklarındaki dizin satırını gösterir; öğrenim adayları dizini (§7) 5. adımın girdisidir.
