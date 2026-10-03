# Aşama 1 — Checkpoint (CP01)

**Tarih:** 2026-10-03 | **Aşama:** 1 — Product Discovery | **Kapanış adımı:** K-437'nin 4. adımı (`/checkpoint`)
**Girdi sürümleri:** karar kaydı v0.49 · Proje Vizyonu (`01`) v0.31 · Ürün Gereksinimleri (`02`) v0.46 · MVP Kapsamı (`10`) v0.28 — `origin/main` @ `cc2c666`
**Çıktı sürümleri:** karar kaydı v0.50 · Proje Vizyonu v0.32 · Ürün Gereksinimleri v0.47 · MVP Kapsamı v0.29
**Girdi raporu:** [Aşama 1 — Çakışma Taraması](PHASE1_CONFLICT_SCAN.md) (K-437'nin 3. adımı, K-429) — açık kalem durumu, ⚠ kayıtların listesi (§4.1) ve devirler (§6) oradan alındı.

> **Ne işe yarar:** Aşama kapanmadan önce üç dokümanın birbirine, karar kaydına ve metodolojiye karşı tutarlı olduğunu doğrular; bulunan her uyumsuzluğu ya kapatır ya da adıyla proje sahibine bırakır. Sonuç öğrenim terfisinin (5. adım, K-438) ve arşiv işaretinin (6. adım, K-436) girdisidir.

---

## Checkpoint Sonucu — 2026-10-03

**Aşama:** 1 — Product Discovery (kapanış sırası K-437: yazım turu ✓ · kalite döngüsü ✓ · çakışma taraması ✓ · **checkpoint** · öğrenim terfisi · arşiv işareti)
**Genel durum:** ⚠ Dikkat gerektiren noktalar — bulunan otuz iki maddenin otuzu bu checkpoint'in PR'ında kapandı, bilgi maddesi bu raporun §5'iyle karşılandı; biri yalnız proje sahibinin onayıyla değişebilir (`00`'ın alt bilgisi). Kapanıştan sonra durum **✓ Yolunda**; kritik konu yok.

### Kontrol Özeti

| # | Kontrol | Sonuç | Detay |
|---|---|---|---|
| 1 | Yol haritası | ✓ | Aşama 1'deyiz; K-437'nin ilk üç adımı kapalı (yazım turu 2026-10-02; kalite döngüsü `01`, `02`, `10` sırasıyla 2026-10-03; çakışma taraması 2026-10-03, PR #51). Atlanan ya da sırası değişen adım yok. `00 §C.1`'in Aşama 1 çıktıları — `01`, `02`, `10` — bu sırayla üretildi. |
| 2 | Doküman durumu | ⚠ → ✓ | Kayıt §1 tablosundaki sürümler başlık ve alt bilgilerle aynıydı (01 v0.31, 02 v0.46, 10 v0.28; tarih 2026-10-03). Tablonun altındaki üç notun etiketi eski sürümü taşıyordu (`01` v0.9, `02` v0.9, `10` v0.2) — "sürüm geçmişi" oldu (B-29). Beş kararın `10` parçasında K-29 işareti eksikti (B-30). `00`'ın başlığı v1.0.3, alt bilgisi v1.0.1 — **yalnız raporlandı**, GUARDRAILS §2 (B-31). `10 §2`'de iki satırın grubu açıklamasızdı (B-21). Şablon dokümanlarda (`03`–`09`, `11`, `12`) başlık ve alt bilgi aynı. |
| 3 | Tutarsızlık | ⚠ → ✓ | Sayımlar, sayısal değerler, durum adları ve atıflar üç dokümanda birebir (Notlar). Yirmi bir yerde aynı kural iki dokümanda farklı kapsamla yazılmıştı; hepsi `02`'nin güncel kuralına ya da kararın kendisine hizalandı (B-1…B-8, B-10…B-19, B-22, B-23, B-25). |
| 4 | Açık kararlar | ✓ | Tracker §4'ün on sekiz satırı kapalı, `02 §13` boş, `01` ve `10`'da açık karar yok (çakışma taraması §1). §6.1'in tek açık maddesi — seksen üç ⚠ kaydın gözden geçirilmesi — bu raporun §5'inde adıyla listelendi (B-32). |
| 5 | Aşama çıktıları | ✓ | `01`, `02` ve `10` yazıldı, kalite döngüsünden geçti (cross-review 18, 26 ve 16 turda TEMİZ) ve etkileri yansıtıldı. Kayıt §1'de üçünün Checkpoint sütunu ✓; Durum sütunu ⏳ kalır — arşiv işareti 6. adımdır (K-436). |
| 6 | Geriye dönük etki | ⚠ → ✓ | `10`'un kalite döngüsünün iki kararı `01`'e eksik yansımıştı: K-635 (`01 §6` dört ölçünün de sıralamayı beslediğini söylüyordu; §8'in M-4 örneği) ve K-627 (Ü-1'de KEP adresi). K-526 ve K-535'in etki sütunu `01`'i göstermiyordu. Hepsi `01` v0.32'de işlendi (B-2, B-5, B-9, B-22). |
| 7 | Yeni alan/kural | ⚠ → ✓ | K-609'un üç günlük yasal süresi `02 §4.2`'de yoktu → Z-47 (B-26). Kurumlara karşı iki süre (K-542, K-628) §4'ün kapsamıyla çelişiyordu → §4 girişi (B-27). `02 §5.8`'in iki kaydı ve bir panel işareti sözlükte yoktu → K-645 (B-28). Satış özetinin siparişi hangi tarihle döneme yazdığı tanımsızdı → `02 §10.6.2` (B-24). K-589'un kuralının evi iki satırın Kaynak'ında yoktu (B-20); K-473'ün adı `10 §5`'te eski hâlindeydi (B-23). |

### Aksiyon Gerektiren Maddeler

- [x] Otuz bulgu düzeltmeyle kapandı — `01` v0.32, `02` v0.47, `10` v0.29, karar kaydı v0.50 (aşağıdaki §3)
- [x] Yeni karar: K-645 — sözlüğe üç terim (öneriyle kaydedildi; ⚠ değildir)
- [x] Checkpoint'in dokunduğu yirmi iki kararın etki sütununa `(taslak güncellendi — …; checkpoint)` işareti eklendi
- [ ] **Seksen üç ⚠ kaydın proje sahibine gösterilmesi** — liste §5'te; kapı arşiv işaretinden (6. adım) önce, karar başına bir cümleyle (çakışma taraması §4.1). İtiraz gelen karar değişirse bu rapora `## Retro Güncelleme` bölümü eklenir.
- [ ] **`00`'ın alt bilgisi** `*Project Playbook — Metodoloji v1.0.1*` → `v1.0.3` — proje sahibinin tek satırlık onayıyla ayrı bir `docs:` PR'ında; aşama kapanışını engellemez (B-31)
- [ ] Sırada öğrenim terfisi (K-437'nin 5. adımı, K-438) — on altı bekleyen aday çakışma taramasının §7'sinde dizinli

### Notlar

**Yöntem.** Dört mercek paralel ve salt okuma olarak koştu: (1) `01` ↔ `02`, (2) `02` ↔ `10`, (3) `01` ↔ `10`, (4) yeni alan/kural, doküman durumu ve K-29 işaretleri. Dosyalar `origin/main`'den okundu, sayımlar betikle yapıldı. Her bulgu düzeltmeden önce güncel metne karşı yeniden okundu; çürütülen bulgu çıkmadı. Düzeltmeler var olan kararların yansıtmasıdır — kalite döngüsü yeniden açılmadı; tek yeni karar K-645.

**Betikle doğrulanıp tutarlı çıkanlar:**

- **`10`'un satırları:** KP-1…KP-77 (eksik yok) · KD-1…KD-63 (22 "Evet", 41 "Açık") · ÖK-1…ÖK-12 · SK-1…SK-10 (3 "Kalkmaz") · YH-1…YH-22. Yol haritasının 22 adayı "Evet" diyen 22 KD satırıyla aynı sırada birebir.
- **`01`'in satırları:** Ü-1…Ü-4 · M-1…M-4 · S-1…S-10 · V-1…V-7. Ü-1, ÖK-1…ÖK-11 ile birebir — yedi dış ön koşul ve dört satış kapısı koşulu, K-628'in "satış yapacak firmada" niteleyicisi dahil.
- **`02`'nin envanterleri:** kırk altı parametre (P-1…P-48, P-2 ve P-11 kaldırıldı; on firma ayarı, otuz altı ürün sabiti) = KP-64 · Z-1…Z-46 (checkpoint'ten sonra Z-47) · L-1…L-8 = KP-72'nin sekiz limiti · B-1…B-15 = KP-66, F-1…F-5 = KP-67 · dokuz müdahale = KP-48 · altı sayaç = KP-50 · işlem izinin sekiz alanı = KP-63 · sözlük 100 terim (checkpoint'ten sonra 103).
- **Sayısal değerler:** ortalama 10 / tepe 50 sipariş · 150 panel işlemi ≈ 1 saat · manuel adım bütçesi tavanı 3, hat başına 2/3/1/0, karışık sipariş 4 (Ü-3 = KP-51 = SK-3) · 10 iş günü ve 1–3 iş günü çitleri · 7 gün, 14 gün, 30 gün, 6 ay, 3 yıl, 10 yıl · üç yeniden deneme · 50 kalem · 320 piksel · üç aylık pencere ve başladığı gün · "üç aylık dönemin en az iki ayında" · Başarısız ödemeli siparişin sayılmaması.
- **Sayımlar:** dört aktör · üç ürün tipi · dört kurulum ayarı · marka kimliğinin dört ayarı · iki ana sayfa düzeni · firma tipinin üç değeri ve tip başına zorunlu alanlar.
- **Durum adları:** Başarısız, Tükendi, İptal edildi, Teslim edilemedi, Kargoya verildi, Teslim edildi, Çözüldü, "ödendi", "tamamlandı" — `02 §5` ile birebir.
- **Atıflar:** `10`'un `02`'ye verdiği 137 bölüm atfının hepsi var olan bir bölüme gidiyor; `02`'nin `10`'a (ÖK-2, ÖK-3, ÖK-4, ÖK-10, SK-3, §5) ve `01`'e (§3.1, §3.2, §6, §7 S-5, §8 V-1) atıfları ve `01`'in `02`'ye bölüm atıfları doğru. `10`'daki S- işaretleri doğru satıra gidiyor (SK-8 → S-10, SK-9 → S-6, SK-10 → S-5).
- **Ölçüler:** M-1 ve M-2'nin tanımı, eşik dayanakları ve okunma anı `01 §6`, `02 §10.6.3` ve `10 §5`'te aynı. V-1 ↔ SK-5, SK-6; V-2 ↔ SK-3, SK-5; V-3 ↔ §5'in ikinci ölçütü ve YH-4; V-4 ↔ §5'in sıfır payda notu; V-5 ↔ `10 §1`'in çıkış hakkı maddesi.
- **K-29 taraması:** K-479…K-644'ün 166 kararında `(taslak güncellenecek)` işareti kalmamış; etki sütununda olup dokümanda karşılığı olmayan karar yok. `02`'nin gövdesinde atıf alan ama `10`'da geçmeyen 43 kararın çoğu sözlük, sayım kuralı ya da durum makinesi kararıdır; kullanıcıya görünen davranışı `10`'da karşılıksız kalan iki karar (K-236, K-184) B-18 ve B-19'da işlendi.

---

## 3. Bulgular ve kapanışları

Seviye: **orta** — bir doküman bir kararı eksik uyguluyor ya da source-of-truth'ta bir kural eksik; **düşük** — ifade, kapsam ya da atıf farkı, anlamı değiştirmez. Kontrol sütunu skill'in yedi kontrolüne gider.

### 3.1 Proje Vizyonu ↔ Ürün Gereksinimleri

| # | Kontrol | Yer | Bulgu | Kapanış | Seviye |
|---|---|---|---|---|---|
| B-1 | 3 | `01 §5.3`, "Tanıtım ile mağaza eşit ağırlıkta" | "İçerik sayfaları katalogdan ürün bağlar" — `02 §3.27.10`'a göre bu bağı yalnız hizmet tanıtımı ve referans iş kurar | "Hizmet tanıtımı ve referans iş katalogdan ürün bağlar…" (K-245) | düşük |
| B-2 | 3, 6 | `01 §6` M-3, payda | `02 §10.6.3` paydayı K-535 ile "iptal edilmemiş fiziksel kalem taşıyan siparişler"e daraltmıştı; `01` eski tanımdaydı | Payda iptal edilmemiş fiziksel kalem taşıyan siparişlerdir; fiziksel kalemlerinin tamamı süre dolmadan iptal edilen sipariş paydaya girmez (K-484, K-535) | düşük |
| B-3 | 3 | `01 §6` M-4 ↔ `02 §10.6.3` ölçüm penceresi | Aynı cümle iki yerde farklı kapsamla: `01` ifası tamamlanmamış siparişi, `02` gecikme feshini anmıyordu | İki cümle aynı: "teslim edilmemiş ya da ifası tamamlanmamış sipariş beklenmez; o ana kadarki iptal, gecikme feshi ve iadesiyle sayılır" (K-580) | düşük |
| B-4 | 3 | `01 §6` Ü-4 | "…ve iade edilir" paranın dönüşünü anlatıyordu; sözlükte "İade" malın geri gönderilmesidir | "…ve bedeli geri ödenir (K-23)" | düşük |
| B-5 | 3, 6 | `01 §2`, §3.1, Ü-4, §8 (iki yer) | Tek başına "müşteri" firma anlamında; K-526'ya göre tek başına "müşteri" sipariş veren alıcıdır | "müşteri firma" / "ilk müşteri firma" (K-526) | düşük |
| B-6 | 3 | `01 §4.1` D-4 | "footer" — `02` aynı yeri "altbilgi" diye anar (tek ad kuralı) | "altbilgideki platform imzası" | düşük |
| B-7 | 3 | `01 §6` mağaza düzeyi maddesi | "Oran ve süre ölçüleri" — M-3 cross-review'da süreden orana döndü, süre ölçüsü kalmadı | "Mağaza düzeyi ölçüler" | düşük |
| B-8 | 3 | `01 §7` S-9 | Satış özetinin kaynağı "sipariş ve iletişim talebi"; `02 §10.6.2` üç kayıt sayar | "— sipariş, ödeme ve iletişim talebi —"; aynı fark `10` KP-53'ün gerekçesinde de vardı ve düzeltildi | düşük |
| B-9 | 6 | `01 §6` Ü-1 | K-627 KEP adresini satış kapısına bağladı (`10` ÖK-8 "KEP adresi dahil"); Ü-1 birebirlik iddiasına rağmen bunu taşımıyordu | "firma kimliği — seçilen tipin zorunlu alanları, KEP adresi dahil (K-14, K-627)" | düşük |

### 3.2 Ürün Gereksinimleri ↔ MVP Kapsamı

| # | Kontrol | Yer | Bulgu | Kapanış | Seviye |
|---|---|---|---|---|---|
| B-10 | 3 | `10` KP-29 | Hemen silinenler arasında hesabın adı (K-605) ve tanınan tarayıcı işaretlerinin geçersizleşmesi (`02 §8.2.6`) yoktu | Eklendi; Kaynak K-605, §3.15.1, §8.2.6 | düşük |
| B-11 | 3 | `10` SK-7 | İki çit sayıyordu; `02 §4.1.5` "Üç süre çitlidir" — hizmetin ifa süresinin alt çiti (P-44, K-503) eksikti | "hizmetin ifa süresi en az 1 gündür"; gerekçede alt çitin otuz gün hesabına girmediği yazıldı | düşük |
| B-12 | 3 | `10` KP-66 | On beş olay on yedi ad gibi okunuyordu — cayma, gecikme feshi ve ayıp kaydı tek bildirimdir (B-9) | "(`02 §9.2` B-1…B-15)" ve "cayma beyanının, gecikme feshinin ya da ayıp talebinin kaydı — tek bildirim" | düşük |
| B-13 | 3 | `10` KP-48 | Teyit her talepte gerekiyormuş gibi okunuyordu; fiziksel kalemi olmayan siparişte fatura adresinin Teslim edildi'ye kadar düzeltilmesi yoktu | `02 §10.4.2`'nin cümlesiyle; Kaynak §10.4.2 (K-601, K-537) | düşük |
| B-14 | 3 | `10` KP-17 | Donan değerlerde siparişin iletişim e-postası (`02 §3.23.3`) ve kargoya verme sözü (§3.23.4) yoktu | Eklendi, misafir siparişindeki tek istisnayla (KP-48); Kaynak K-140, K-585, §3.23.3, §3.23.4 | düşük |
| B-15 | 3 | `10` ÖK-10, "Karşılanmazsa" | Çerez politikası eksikken de formlar kapanıyormuş gibi okunuyordu; `02 §3.1.5`'e göre formları yalnız aydınlatma metni kapatır | "Satış kapalı kalır. Aydınlatma metni tamamlanıp yayına alınmamışsa ayrıca…; çerez politikası yalnız satış kapısını tutar" (K-517) | düşük |
| B-16 | 3 | `10` KP-21, KP-22 | Havale hattında IBAN'ın beyan, iptal ve fesih sırasında girildiği temel hâl yazılı değildi (`02 §7.2.8`, §7.2.3, §7.3.5, §7.4.5) | KP-21'e iptal ve fesih, KP-22'ye beyan için yan cümle; dört istisnai hâl "IBAN sonradan yeniden gerektiğinde" diye ayrıldı (K-499) | düşük |
| B-17 | 3 | `10` KP-72 | Şifresi sıfırlama bağlantısıyla yenilenmiş tanınan tarayıcı yoktu (`02 §3.13.17`, §8.2.6) | "…ya da şifresi sıfırlama bağlantısıyla yenilenmiş tarayıcı" (K-603) | düşük |
| B-18 | 3 | `10` KP-9 | Daha önce alınmış dijital varyantın sepete eklenmesindeki uyarının (`02 §3.12.9`, K-236) KP ya da KD karşılığı yoktu | KP-9'a eklendi; Kaynak K-236, §3.12.9 | düşük |
| B-19 | 3 | `10` KP-18 | Ödemesi alınmamış, kendiliğinden iptal edilmiş siparişin müşteri listesinde görünmemesinin (`02 §3.17.8`, K-184) karşılığı yoktu | KP-18'e eklendi; Kaynak K-184, K-528, §3.17.8 | düşük |
| B-20 | 7 | `10` KP-15, KP-53, Kaynak sütunu | Geç gelen kart ödemesinin kuralının evi `02 §6.2.16`'dır (K-589); iki satır onu göstermiyordu | İki Kaynak sütununa §6.2.16 | düşük |
| B-21 | 2 | `10 §2`, KP-49 ve KP-77'nin grubu | KP-49'un iletişim talebi kısmı akış 7'nin koludur (`02 §2.3`, K-468), KP-77 bir yönetici işlemidir; ikisi akışlarının dışındaki grupta | Satır kimlikleri ve yerleri korunarak §2'nin giriş paragrafına grup notu — iki satır komşu satırlarıyla birlikte okunur | düşük |

### 3.3 Proje Vizyonu ↔ MVP Kapsamı

| # | Kontrol | Yer | Bulgu | Kapanış | Seviye |
|---|---|---|---|---|---|
| B-22 | 3, 6 | `01 §6` mağaza düzeyi maddesi ↔ `10 §5`, §1; `01 §8` doğrulama anı | K-635'e göre yalnız M-1 ve M-2 sıralamayı besler; `01 §6`'nın iki cümlesi dördünü de sıralamaya bağlıyordu ve paragraf kendi içinde çelişiyordu. "İzleme ölçüsü" `01`'de dört ölçünün, `10`'da yalnız M-3 ve M-4'ün adıydı. §8'in örneği karşılığı olmayan M-4'ü gösteriyordu | `01 §6`: "M-1 ve M-2 post-MVP sıralamasını besler (K-418, K-635)", "Mağaza düzeyi ölçüler … izleme ölçüleridir"; `10 §5`: M-3 ve M-4 "yalnız izlenir, sıralamaya girmez"; `01 §8`'in M-4 örneği silindi | **orta** |
| B-23 | 3, 7 | `10 §5` ikinci ölçüt, YH-2, YH-3 | (2) maddesi "kargo ve havale otomasyonu" diyordu — K-473 adı "kargo şirketi entegrasyonu ve havale eşleştirmesi" diye sabitledi; YH-2 ve YH-3'ün notunda ikinci ölçüt yoktu (YH-4'te var) | Ad K-473'e göre, adaylar (YH-2, YH-3) adıyla; iki satırın notuna "İkinci ölçüt: hacim tetikleyicisi gerçekleşirse öne çıkar (`01 §8` V-2, SK-3)" (K-636) | düşük |
| B-24 | 7 | `02 §10.6.2` ↔ `10` SK-3, `01 §8` V-2 | SK-3'ün günlük sayısı satış özetinden okunuyor (K-643), ama özet siparişi hangi tarihle döneme yazdığını söylemiyordu; havale siparişi günler sonra ödendiğinde iki okuma ayrışabilirdi | `02 §10.6.2`: "sipariş, oluştuğu tarihin düştüğü döneme yazılır — ödemesi sonradan tamamlanan havale siparişi de" (K-474); `01` ve `10` değişmedi | düşük |
| B-25 | 3 | `10 §1` Kaynak satırı | Gövde K-354'ü taşıyordu, Kaynak'ta yoktu; K-478 Kaynak'ta vardı, gövdede yoktu | Kaynak'a K-354; gövdede "Google uygulamasının kimlik bilgileri (K-478)" | düşük |

### 3.4 Yeni alan/kural ve doküman durumu

| # | Kontrol | Yer | Bulgu | Kapanış | Seviye |
|---|---|---|---|---|---|
| B-26 | 7 | `02 §4.2`, §4.1.3, §11 | K-609'un yasal süresi — firma ifanın imkânsızlaştığını öğrendiği tarihten üç gün içinde bildirir (Mesafeli Sözleşmeler Yönetmeliği m.16/4) — yalnız §7.2.4 ve §12.5'te; §4 kendini "tüm süreler tek bölümde" diye tanımlıyor ve benzer sayaçsız yükümlülükler (Z-25, Z-37) orada | **Z-47** — başlangıç firmanın durumu öğrendiği an, 3 takvim günü, sayaç tutulmaz, Hayır — yasal, K-609; §4.1.3'ün takvim günü listesine ve §11'in "Envantere girmeyenler" cümlesine eklendi | **orta** |
| B-27 | 7 | `02 §4` girişi, §4.1.3, §11 | K-542'nin beş iş günü ve K-628'in otuz günü yalnız §12.5'te; §4.1.3 ve §11 "iki süre iş günüyle" diyordu ve düz okumada çelişiyordu | §4 girişine: firmanın resmî kurumlara karşı kayıt ve bildirim süreleri ürünün işlettiği süreler değildir, yerleri §12.5'tir, §4.1'in sayım kuralı uygulanmaz; §4.1.3 ve §11 "ürünün işlettiği süreler" ile sınırlandı. İki Z satırı eklemek elendi: sayaç yoktur ve tüketiciye bağlı değildir | düşük |
| B-28 | 7 | `02 §1.2` | `02 §5.8`'in iki kaydı — "mal dönmedi" kapanışı (K-496), IBAN isteği (K-498) — ve "geri ödeme gerçekleşmedi" işareti (K-521) gövdede tekrar ediyor — "mal dönmedi" 17, "IBAN bekleniyor" 10, "geri ödeme gerçekleşmedi" 7 kez — ama sözlükte yoktu; K-617 yalnız cross-review'da doğan kavramları taramıştı | **K-645** (öneriyle): üç † satır — `NotReturnedClosure`, `RefundIbanRequest`, `RefundFailedMark`; "havale gerçekleşmedi" bir panel adımıdır, sonucu IBAN isteğidir. Sözlük 100 → 103 | düşük |
| B-29 | 2 | Karar kaydı §1, tablo altı notlar | Etiketler "`01` v0.9 kapsamı", "`02` v0.9 kapsamı", "`10` v0.2 kapsamı" — v0.31, v0.46 ve v0.28'e kadar giden geçmişi taşıyordu | "`01` sürüm geçmişi" vb. — sürüm numarası yazılmaz, etiket bir daha eskimez | düşük |
| B-30 | 2 | Karar kaydı §2, K-528 ve K-550 (ayrıca K-482, K-506, K-539) | `10`'a yansıması yapılmış ama etki parçası `(taslak güncellendi — vX)` taşımıyordu; K-29 kapanış taramasında kaçak gibi görünürdü | K-528 → v0.11, K-550 → v0.8, K-482, K-506, K-539 → v0.11 | düşük |
| B-31 | 2 | `Docs/00_PROJECT_METHODOLOGY.md` başlık (v1.0.3) ↔ alt bilgi (v1.0.1) | Başlık PR #9 ve #22'de arttı, alt bilgi iki seferde de güncellenmedi | **Uygulanmadı** — GUARDRAILS §2 gereği `00` proje sahibinin açık onayı olmadan değişmez. Aksiyon maddesi; aşama kapanışını engellemez | düşük |
| B-32 | 4 | Karar kaydı §6.1 | Bilgi: ⚠ işaretli seksen üç kaydın listesi checkpoint raporunda adıyla yer almalı (çakışma taraması §4.1'in kapısı) | §5'te | bilgi |


---

## 4. Etki işaretleri

Checkpoint'in düzeltmeleri var olan kararların yansıtmasıdır; karar kaydında şu yirmi iki kararın etki sütununa `(taslak güncellendi — …; checkpoint)` parçası eklendi: K-140, K-184, K-236, K-473, K-474, K-499, K-503, K-517, K-526, K-535, K-537, K-542, K-580, K-589, K-601, K-603, K-605, K-609, K-627, K-628, K-635, K-636. K-635'in karar hücresi `10 §5` için *"izleme ölçüsüdür"* cümlesini alıntılar; `10 §5` artık *"yalnız izlenir"* der, çünkü `01 §6` "izleme ölçüsü"nü dört ölçünün ortak adı olarak kullanır — anlam değişmedi, etki parçası bunu kaydeder.

---

## 5. Proje sahibinin gözden geçirme listesi — ⚠ işaretli seksen üç karar

Kaynak: [çakışma taraması §4.1](PHASE1_CONFLICT_SCAN.md). Bu kararlar proje sahibinin 2026-10-03 talimatıyla (*"kolay soruları sorma, çok kritik konuları sor"*) sorulmadan kaydedildi ve `(öneriyle kaydedildi — ⚠)` işaretini taşır — kişisel veri, para ya da yasal hak, varlık ya da önceki bir karardan ayrılma. **Kapı:** liste arşiv işaretinden (K-437'nin 6. adımı) önce proje sahibine tek listede, karar başına bir cümleyle gösterilir; itiraz gelen karar yeni bir karar satırıyla değişir (`02 §13.4`), itiraz gelmeyen satırın işareti `(öneriyle kaydedildi — ⚠ — gözden geçirildi YYYY-AA-GG, itiraz yok)` olur. K-645 bu listede değildir — ⚠ grubuna girmez.

| Karar | Grup | Konu |
|---|---|---|
| K-498 | A-18 ve `02` audit/deep review | Silinen IBAN'ın müşteriden yeniden isteniş biçimi — kanal, bildirim ve IBAN beklenirken geri ödeme süresi |
| K-499 | A-18 ve `02` audit/deep review | Teslim gecikmesinde fesih: otuz günlük sınırın kargodaki siparişte aşılması ve kanuni faiz — **K-198'i değiştirir** (Mesafeli Sözleşmeler Yönetmeliği'nin güncel metnine karşı doğrulama) |
| K-500 | A-18 ve `02` audit/deep review | Kargo ücretinin KDV oranı — **K-219'u değiştirir** (KDV Kanunu'nun güncel metnine karşı doğrulama) |
| K-501 | A-18 ve `02` audit/deep review | `A-16` Kargo ücretinin geri ödenme tetikleyicisi — kısmi iptal ve kısmi caymada gidiş kargosu — **K-451'in ve K-292'nin tetiğini değiştirir** |
| K-502 | A-18 ve `02` audit/deep review | Duyuru metninin sınıfı — **K-412 ve K-447'nin duyuru satırını değiştirir** |
| K-503 | A-18 ve `02` audit/deep review | Hizmette cayma: onay kutusu, ifa süresi, karışık siparişte pencere ve cayılan kalemin kapanışı (Mesafeli Sözleşmeler Yönetmeliği'nin güncel metnine karşı doğrulama) |
| K-504 | A-18 ve `02` audit/deep review | Yönetici düzeltmesinin sınırları ve yan etkileri — K-372'ye sınır ekler |
| K-505 | A-18 ve `02` audit/deep review | Ödenmemiş siparişte kısmi iptal |
| K-506 | A-18 ve `02` audit/deep review | "sorun bildir" kanalının açıldığı an |
| K-507 | A-18 ve `02` audit/deep review | İletişim talebinin saklama süresinin başlangıcı |
| K-508 | A-18 ve `02` audit/deep review | Referans fiyatın penceresi — **K-63 ve K-67'yi değiştirir** (Fiyat Etiketi Yönetmeliği'nin güncel metnine karşı doğrulama) |
| K-509 | A-18 ve `02` audit/deep review | Cayma istisnası sebeplerinin Yönetmelik bentlerine hizalanması — **K-206'nın listesini değiştirir** (Mesafeli Sözleşmeler Yönetmeliği'nin güncel metnine karşı doğrulama) |
| K-510 | A-18 ve `02` audit/deep review | Başka kanaldan gelen cayma bildiriminin kaydı — K-369'un envanterine sekizinci müdahale (Mesafeli Sözleşmeler Yönetmeliği'nin güncel metnine karşı doğrulama) |
| K-511 | A-18 ve `02` audit/deep review | Fiyat etiketinin zorunlu bilgileri: üretim yeri, birim fiyat, fiyatın başlangıç tarihi (Fiyat Etiketi Yönetmeliği'nin güncel metnine karşı doğrulama) |
| K-512 | A-18 ve `02` audit/deep review | Panelde üye hesabı görünümü ve firmanın hesabı silmesi |
| K-513 | A-18 ve `02` audit/deep review | Ödeme sağlayıcısına kişisel veri aktarımı — **K-359'un öncülünü düzeltir** |
| K-514 | A-18 ve `02` audit/deep review | Veri ihlalinde ilgili kişilere ulaşma yolu |
| K-515 | A-18 ve `02` audit/deep review | Giriş kaydı ve saklama süresi |
| K-516 | A-18 ve `02` audit/deep review | Kişisel veri işlemenin hukuki sebepleri — K-344 ve K-345'in ifadesini değiştirir |
| K-517 | A-18 ve `02` audit/deep review | Aydınlatma kapısının tanımı — K-365'in kapısını genişletir |
| K-518 | A-18 ve `02` audit/deep review | Ödeme sağlayıcısının çerçevesindeki çerezler |
| K-519 | A-18 ve `02` audit/deep review | Ödemesi alınmamış siparişin kişisel verisinin saklama süresi (Mesafeli Sözleşmeler Yönetmeliği'nin güncel metnine karşı doğrulama) |
| K-520 | A-18 ve `02` audit/deep review | Teslim günü yapılan cayma — K-491'in ayrımını tamamlar |
| K-521 | A-18 ve `02` audit/deep review | Kart iadesinin sağlayıcıda başarısız olması |
| K-522 | A-18 ve `02` audit/deep review | Ulaşmayan e-postanın yeniden gönderilmesi ve kanal uyarısı |
| K-523 | A-18 ve `02` audit/deep review | İşlem izinin ayar kapsamı — K-414'e not |
| K-524 | A-18 ve `02` audit/deep review | Havale siparişiyle stok kilitleme: aynı anda açık ödenmemiş sipariş tavanı — K-332'ye not |
| K-525 | A-18 ve `02` audit/deep review | `A-18`'in kenar durumu — müşteri IBAN'ı hiç girmezse |
| K-561 | `02` cross-review | Adresin alanları |
| K-562 | `02` cross-review | Sipariş e-postasındaki bağlantının erişim anahtarı ve ömrü |
| K-566 | `02` cross-review | Yanlış iptalin düzeltilmesinin koşulu ve ayırmalar — **K-504'ün yan etki kuralını iptal düzeltmesi için değiştirir** |
| K-569 | `02` cross-review | Sağlayıcıda başarısız kart iadesinde havale yolunun kimin seçimi olduğu — K-521'i netleştirir |
| K-570 | `02` cross-review | Para gelmeden konmuş havale "ödendi" işaretinin düzeltilmesi — K-504'ün örneğini somutlaştırır |
| K-571 | `02` cross-review | Gecikme feshinde geri ödenen tutarın kapsamı — K-499'u netleştirir |
| K-572 | `02` cross-review | İletişim formundan gelen KVKK talebinin hukuki sebebi — K-516'yı netleştirir |
| K-573 | `02` cross-review | Ölçü birimi alanının isteğe bağlı olması ve birim fiyat yükümlülüğü — K-511'i netleştirir |
| K-574 | `02` cross-review | ETBİS kayıt bilgisinin üründeki karşılığı — K-14'ü netleştirir |
| K-575 | `02` cross-review | Geri ödeme IBAN'ının düzeltilmesi ve bankada gerçekleşmeyen havale — K-498'i tamamlar |
| K-576 | `02` cross-review | Satış kapısının yasal metin koşulunun tanımı — K-465'i K-517'ye hizalar |
| K-578 | `02` cross-review | Kısmen ifa edilmiş hizmette cayma — K-205'i netleştirir |
| K-579 | `02` cross-review | Ödemesiz sipariş limitlerinin e-posta ekseni ve misafirin yazdığı e-posta — K-528 ve K-524'ün eksenini daraltır |
| K-581 | `02` cross-review | Başka kanaldan gelen cayma bildirimi IBAN taşımadığında — K-510'u tamamlar |
| K-582 | `02` cross-review | Saklama süresi dolan siparişte kalan ticari kayıt — K-357'yi tamamlar |
| K-583 | `02` cross-review | Süresi dolan siparişin kalan kaydının anonimliği — **K-582'nin "kayıt anonim hâle gelir" cümlesini düzeltir** |
| K-585 | `02` cross-review | Misafir alıcının onaydan sonra fark ettiği yanlış e-posta — K-193'ü tamamlar, K-369'un envanterine dokuzuncu müdahaleyi, K-375'in matrisine B-15'i ekler |
| K-586 | `02` cross-review | E-posta düzeltmesinin eski ve yeni değerinin yeri — **K-585'in (6). sonucunu düzeltir** |
| K-587 | `02` cross-review | Siparişte eşzamanlı işlem — **K-270'i sipariş işlemlerine genişletir** |
| K-589 | `02` cross-review | İptal edilmiş siparişe geç gelip sistemce geri ödenen kart ödemesinin kaydı ve saklama süresi — K-233'ü ve K-519'u tamamlar |
| K-590 | `02` cross-review | Gecikme feshine uğramış fiziksel kalemin sevkiyat hattındaki yeri — K-499'u ve K-503'ü tamamlar |
| K-591 | `02` cross-review | Onay adımı açıkken yasal metnin sürümünün artması — K-129'u ve K-191'i tamamlar, K-363'ün kapsamını netleştirir |
| K-592 | `02` cross-review | Açık kalemi kalmamış siparişin kapanışı — K-590'ın kapanış cümlesini netleştirir, K-503'ün S9 kalıbına uygulanır |
| K-593 | `02` cross-review | Fiziksel kalemleri teslimden önce kapanan karışık siparişte hizmet kaleminin cayma penceresi — K-503'ü tamamlar |
| K-595 | `02` cross-review | Dijital ve hizmet kalemindeki ayrı onayın kaydı — K-204'ü ve K-503(a)'yı tamamlar |
| K-596 | `02` cross-review | Süresinde gönderildiği bildirilen ama firmaya ulaşmayan iade malı — K-496'yı netleştirir, K-539'un kullanımını genişletir |
| K-597 | `02` cross-review | Kişiye özel üretim istisnası işaretli üründe otuz günlük gecikme feshi — K-499 ile K-549'u uzlaştırır |
| K-598 | `02` cross-review | Sipariş kaleminin teslim hattını ve cayma rejimini belirleyen niteliklerin donması — K-77'nin listesini tamamlar |
| K-599 | `02` cross-review | Hesabın e-posta adresi değiştiğinde eski adrese bildirim ve geri alma — K-110'u ve K-384'ü tamamlar |
| K-601 | `02` cross-review | Siparişin adresini düzeltirken talebin teyidi — K-374'ü ve K-375'i tamamlar |
| K-602 | `02` cross-review | Misafir sipariş takibi sorgusunun ölçüm ekseni — K-330'un takip sorgusu için seçtiği tek ekseni genişletir, K-460'ın eşiğini tamamlar |
| K-603 | `02` cross-review | Girişin e-posta eksenindeki engelin hesap kilidine dönüşmesi — K-329'u ve K-330'u tamamlar, K-348'in çerez envanterine beşinci kullanımı ekler |
| K-604 | `02` cross-review | Kargoya verilmemiş fiziksel kalemde başka kanaldan gelen cayma bildirimi ve S5'in koşulu — K-510'un kaydını kargoya verilmiş kaleme sınırlar, K-537'nin çıkarma kuralını S5'e uygular |
| K-605 | `02` cross-review | Hesabın adı — K-300'ün ön doldurmasının ve K-514'ün ulaşma listesinin dayandığı alanı tanımlar |
| K-606 | `02` cross-review | Eksik bilgilendirmede cayma süresinin yasal uzaması — K-203'ün "sabit pencere"sini Yönetmelik m.10 ile tamamlar, K-510'un panel kaydını genişletir |
| K-607 | `02` cross-review | Başka kanaldan gelen cayma bildiriminin kimden geldiğinin teyidi — K-510'u ve K-581'i tamamlar, K-510'un IBAN aktarmasını daraltır, K-601'in teyit kalıbını cayma bildirimine ve "müşteriyle anlaşıldı (müşteri talebi)" iptaline uygular |
| K-608 | `02` cross-review | Teslim günü yapılan cayma beyanında teslimin sırası — K-520'yi daraltır, K-491'in ayrımını tamamlar |
| K-609 | `02` cross-review | Firma iptali ve ifanın imkânsızlaşması (Mesafeli Sözleşmeler Yönetmeliği m.16/4) — K-199'u, K-373'ü ve K-317'yi tamamlar |
| K-610 | `02` cross-review | Yerli üretim logosu (Fiyat Etiketi Yönetmeliği m.5/2-e) — **K-511'in logo cümlesini değiştirir** |
| K-611 | `02` cross-review | Ters ibraz (harcama itirazı) — K-172'nin ödeme eksenini ve K-569'un havale yolunu tamamlar |
| K-612 | `02` cross-review | Ters ibrazda çift ödemeye karşı kuralın sınırı — **K-611'in çift ödeme kuralını daraltır** |
| K-614 | `02` cross-review | Sağlayıcıya bağlanılamadığı için ödeme sayfası hiç açılmamış siparişin ödeme denemesi limitine sayılması — **K-332'nin sayımını daraltır** |
| K-616 | `02` etki yansıtma | Havale IBAN'ı değiştiğinde yöneticilere bildirim — **K-523'ün açık bıraktığı parçayı kapatır** |
| K-618 | `10` audit/deep review | Gecikme feshinin kapsam satırındaki yeri ve firmanın kanuni faiz uyarısı — K-499'un `10` yansımasını tamamlar |
| K-619 | `10` audit/deep review | Tutar bazlı kısmi geri ödemenin kapsam satırı — K-539'un ve K-596'nın `10` yansıması |
| K-620 | `10` audit/deep review | Veri işleme sözleşmelerinin kurulum listesindeki yeri — K-482'nin ve K-542'nin `10 §4.1` yansıması |
| K-621 | `10` audit/deep review | Caymada kart iadesini kimin başlattığı — KP-47'yi `02 §5.5`'e hizalar, K-491'in "Değişmeyen" maddesini uygular |
| K-622 | `10` audit/deep review | Ayıp talebi kanalının açıldığı an kapsam satırında — K-506'nın `10` yansıması |
| K-623 | `10` audit/deep review | Giriş kaydı ve imha kaydının kapsam satırı — K-515'in ve K-552'nin `10` yansıması |
| K-624 | `10` audit/deep review | Anahtarı olan ama beklenen yetenekleri taşımayan ödeme sağlayıcısı — K-556'nın (2). maddesini tamamlar |
| K-625 | `10` audit/deep review | Erişilebilirliğin yasal tabanı: 2025/10 sayılı Cumhurbaşkanlığı Genelgesi — **K-395'i değiştirir** (Genelge'nin resmî metnine karşı doğrulama) |
| K-626 | `10` audit/deep review | Ana sayfadan ulaşılan "İşlem rehberi" sayfası — K-547'yi tamamlar (Hizmet Sağlayıcılar Yönetmeliği'nin güncel metnine karşı doğrulama) |
| K-627 | `10` audit/deep review | "iletişim" başlığının bilgileri: KEP adresi, işletme adı ve marka, meslekle ilgili davranış kuralları — K-14, K-480 ve K-547'nin firma kimliğini tamamlar (Hizmet Sağlayıcılar Yönetmeliği ve ETBİS Tebliği'nin güncel metnine karşı doğrulama) |
| K-628 | `10` audit/deep review | ETBİS kaydının kimi bağladığı — K-14'ün niteleyicisini düzeltir, `01` cross-review R10 BULGU-2'nin kabulünü günceller (ETBİS Tebliği'nin güncel metnine karşı doğrulama) |
| K-629 | `10` audit/deep review | Yedeğin kurulum ön koşulu olması — K-407'nin ve K-427'nin dayandığı ön koşulu yazar |

**Sayım:** A-18 ve `02`'nin audit ve deep review'ı 28 · `02`'nin cross-review'ı 42 · `02`'nin etki yansıtması 1 · `10`'un audit ve deep review'ı 12 — **toplam 83**. Betikle sayıldı; seksen üçünün de konu hücresi ⚠ işaretini taşıyor.

---

*Kaynak: K-437 (kapanış sırası, 4. adım) · K-429 (çakışma taraması — girdi) · K-29 (etki işaretleri) · K-37 (kapı kuralı) · K-436 (arşiv işareti) · K-438 (öğrenim terfisi) · K-645 (sözlük) · `.claude/skills/checkpoint/SKILL.md` (yedi kontrol ve çıktı biçimi) · `Docs/CHECKPOINT_REPORTS/README.md` (rapor türü ve retro güncelleme).*
