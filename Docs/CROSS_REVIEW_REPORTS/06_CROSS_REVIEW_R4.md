# Cross-Review — 06 Data Model (Tur 4)

**Tarih:** 2026-10-18 · **Hedef:** `Docs/06_DATA_MODEL.md` v0.21 (girdi commit'i `fac8a92`, 1 166 566 bayt, 6 018 satır) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Ciddiyet ölçüsü:** 1. turdan itibaren uygulanır, mekanizma okuma paragrafıyla (K-1125) · **Koşum:** 1. turun biçimiyle (K-1126) — (a) tek parça çağrının yerine **en büyük iki birleşim**, (b) **yedi parçalı kontrol koşumu**; istem, parça notu, parça ve birleşim sınırları değişmedi; turun sonucu ikisinin birleşimidir · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), stdin'den tek metin, her çağrıda boş ve izole bir çalışma klasöründe; karar kaydı, öteki dokümanlar ve önceki turların raporları verilmedi (K-431) · **Kullanım sınırı:** dokuz çağrının beşi ilk koşumda hesabın kullanım sınırına takıldı; sınır açılınca (08:49) aynı girdi dosyalarıyla yeniden koşuldu — model, istem ve girdi değişmedi (§1) · **Sonuç:** TEMİZ değil — dokuz çağrının beşi `SONUÇ: TEMİZ`; beş ayrı bulgu (1 KABUL · 1 KISMİ · 3 RET — ikisi ikinci ret); düzeltmeler `06` v0.22

## 1. Koşum

**Betikler** 1. turun betikleridir (`06_CROSS_REVIEW.md` §1), değiştirilmeden kopyalandı (dosyalar 1. turunkilerle `cmp` ile aynı): `C:\Users\TEKNOD~1\AppData\Local\Temp\claude\c--projects-Shopfolio\3a694994-cfb4-4b7e-a92e-eea67b22c07d\scratchpad\cr4\` — `build_inputs.py`, `run_codex.sh`, `prompt.txt`, `partnote.txt`; girdiler `cr4/in/`, ilk koşumun çıktıları `cr4/out/`, yeniden koşulan beş çağrının çıktıları `cr4/out2/`. SHA-256 (ilk 16 hane, betiğin hesabı): istem `7c31919b40b3209c`, parça notu `2b150c8107b170bb`, şablon `5d3e5023c4926bfe` — üçü 1.–3. turla aynı —, doküman (v0.21) `3ce2eb3c6ccbc977`. Kapsanmayan satır yok (betiğin denetimi). Koşumdan önce çağrı kısa bir deneme metniyle doğrulandı.

**Birleşim 1'in sınır payı.** Birleşim 1 981 803 karakterdir — aracın 1 048 576 karakterlik sınırının **66 773 karakter** altında (3. turda 69 641); pay 10 000 karakterin üstündedir. Tek parça girdi 1 074 811 karakterdir ve sınırın üstündedir — koşulmadı (K-1126).

**Kullanım sınırı ve yeniden koşum.** Dokuz çağrı 05:42'de paralel başladı. Birleşim 1, birleşim 2, parça 5 ve parça 6 tamamlandı; parça 1, 2 ve 4 üç-dört saniyede, parça 3 ve 7 yaklaşık iki dakika sonra *"You've hit your usage limit … try again at 8:47 AM"* hatasıyla döndü (çıkış kodu 1, çıktı yok; kayıtlar `cr4/out/`). Sınır açılınca bir deneme çağrısı doğrulandı ve beş çağrı **aynı girdi dosyalarıyla** (`cr4/in/` — v0.21; bu turun düzeltmelerinden önce kuruldu) yeniden koşuldu; beşi hatasız döndü. Yedek yönteme (`cursor-agent`) geçilmedi: tur aynı model ve aynı girdiyle tamamlanabiliyordu ve iki modelin bulgularını tek turda karıştırmak turlar arası ölçümü bozardı.

| Çağrı | Bölümler | Girdi | Token | Süre | Sonuç |
|---|---|---|---|---|---|
| Birleşim 1 | parça 2–7 — §1.1.5–§9 | 1 073 823 bayt (981 803 karakter) | 377 448 | 205 sn | 1 bulgu |
| Birleşim 2 | parça 1, 4–7 — §1'in girişi, §1.1.1–§1.1.4, §2–§9 | 723 937 bayt (664 052 karakter) | 258 277 | 111 sn | `SONUÇ: TEMİZ` |
| Parça 1 | §1'in girişi · §1.1.1–§1.1.4 | 158 271 bayt | 68 094 | 43 sn (yeniden koşum) | `SONUÇ: TEMİZ` |
| Parça 2 | §1.1.5–§1.1.8 | 250 807 bayt | 82 777 | 28 sn (yeniden koşum) | `SONUÇ: TEMİZ` |
| Parça 3 | §1.1.9–§1.3 | 314 016 bayt | 124 718 | 91 sn (yeniden koşum) | 2 bulgu |
| Parça 4 | §2 · §3 | 127 929 bayt | 60 087 | 158 sn (yeniden koşum) | 1 bulgu |
| Parça 5 | §4'ün girişi · §4.1–§4.35 | 261 164 bayt | 96 549 | 108 sn | `SONUÇ: TEMİZ` |
| Parça 6 | §4.36–§4.72 | 208 160 bayt | 80 322 | 153 sn | 1 bulgu |
| Parça 7 | §5 · §6 · §7 · §8 · §9 | 195 453 bayt | 81 813 | 90 sn (yeniden koşum) | `SONUÇ: TEMİZ` |

**Web araması:** tamamlanan dokuz çağrının hiçbiri arama yapmadı. Sınıra takılan ilk koşumda parça 3 ve parça 7 6563 sayılı Kanun m.11/3'ü mevzuat.gov.tr'de aradı; o çağrılar çıktı üretmedi. Birleşim 1'in bulgusu yasal dayanak anar (6563 m.11/3) ama dayanağı dokümanın kendi cümlesidir — arama yapmadı; dayanak resmî metne karşı okundu (§3).

## 2. Ham bulgular

İkinci modelin çıktıları, satır sonu boşlukları dışında düzenlenmeden (alıntı blokları içinde).

**(a) Tek parça çağrı:** koşmadı — girdi sınırı (§1; K-1126). Yerine:

**Birleşim 1 — §1.1.5–§9:**

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §9.1 Z-28; §1.3 K-1092
> Alıntı: "`Order.CreatedAt`'in Türkiye saatindeki takvim yılının sonu"
> Sorun: Siparişin kişisel verisinin saklaması işlem tarihinden değil yıl sonundan başlatılmıştır. Doküman, K-1092’de 6563 m.11/3 bakımından sürenin “işlemin gerçekleştiği tarihten” sayıldığını kendisi de kabul eder. Bu, aynı saklama ilkesinin siparişlerde farklı uygulanmasıdır; kişisel verilerin bazı siparişlerde yaklaşık bir yıl gereksiz fazla tutulmasına yol açar (1 ve 4).
> Öneri: Z-28 başlangıcını siparişin işlem/oluşma anına göre on yıl olarak tanımlayın; §4.17, §9.1 ve ilgili imha taramalarını aynı başlangıçla hizalayın.
>
> SONUÇ: 1 BULGU
> ```

**Birleşim 2 — §1'in girişi, §1.1.1–§1.1.4, §2–§9:**

> ```text
> SONUÇ: TEMİZ
> ```

**Parça 1 — başlık notu, §1'in girişi, §1.1.1–§1.1.4:**

> ```text
> SONUÇ: TEMİZ
> ```

**Parça 2 — başlık notu, §1.1.5–§1.1.8:**

> ```text
> SONUÇ: TEMİZ
> ```

**Parça 3 — başlık notu, §1.1.9–§1.3:**

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §1.1.9, `03 §1.2` S2 ve S10; §1.4.3 m1–m2
> Alıntı: “Alındı → Teslim edildi — ... son açık kalemin iptali, çıkarılması ya da cayma beyanı”; “Hiçbir kalem teslim edilmemişse İptal edildi”
> Sorun: Son açık kalemin iptal/cayma ile kapanması S2 ve S10’da siparişi doğrudan `Delivered` durumuna geçirirken, §1.4.3 hiçbir kalem teslim edilmemişse sonucu `Cancelled` olarak zorunlu kılar. Durum; geri ödeme, müşteri işlemleri ve siparişin kapanış yolunu belirlediğinden aynı olayda iki farklı sonuç doğurur. Doküman içi çelişkidir (4).
> Öneri: S2 ve S10’u §1.4.3 ile uyumlu biçimde ayırın: yalnız en az bir teslim edilmiş kalem varsa `Delivered`; aksi durumda uygun `Cancelled` geçişi yazılsın.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §1.1.9, `03 §1.2` S6 ve `03 §4.1.12`; §1.1.10, `03 §8.2`
> Alıntı: “`DeliveryDate` (fizikselde sipariş düzeyinde tek tarih — `02 §5.8` m1)”; “| OrderItem | `DeliveryDate`”; “`DeliveryDate` (sipariş düzeyi)”
> Sorun: Fiziksel teslim tarihi sipariş düzeyinde tekil olarak tanımlanmışken S6 ve Z-12 eşlemesi alanı yalnız `OrderItem` üzerinde gösteriyor; başka bir satır ise aynı alanı sipariş düzeyinde gösteriyor. Teslim tarihi cayma ve ayıp sürelerinin başlangıcını belirlediği için iki depolama yeri farklılaşırsa müşteri hakkı yanlış hesaplanabilir. Doküman içi çelişkidir (4).
> Öneri: Fiziksel teslim tarihi için tek yetkili alanı açıkça `Order` veya `OrderItem` olarak belirleyin; ilgili tüm matris satırlarını buna göre hizalayın. Kalem düzeyinde ayrı teslim tarihleri gerekiyorsa fiziksel sipariş kuralından açıkça ayrıştırın.
>
> SONUÇ: 2 BULGU
> ```

**Parça 4 — başlık notu, §2–§3:**

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §2.2.5, §2.7.1
> Alıntı: “varyantın kalıcı silinmesiyle ayırma — geçiş biriminin dışında, basamaklanmayla — düşer”
> Sorun: Ödeme bekleyen siparişlerin stok/kupon ayırmaları, ürün veya varyant silinince basamaklı silme ile yok olur. Sipariş ise silinmez ve sonradan ödeme bildirimi gelebilir. Böylece ödeme alınmış siparişin ayırması kaybolur; stok yeniden satılabilir veya kupon hakkı yeniden kullanılabilir. Bu, siparişin para ve teslim hakkı kaybına yol açabilecek bir boşluktur (2).
> Öneri: Aktif `StockReservation` bulunan `Product`/`Variant` için silmeyi `NO ACTION` ile engelleyin. Zorunlu kaldırma gerekiyorsa, ayırmayı silmeden önce ilgili bekleyen siparişleri ve ödeme-bildirimi işlemesini güvenli biçimde sonlandıran, açıkça tanımlı tek bir işlem önkoşulu ekleyin.
>
> SONUÇ: 1 BULGU
> ```

**Parça 5 — başlık notu, §4'ün girişi, §4.1–§4.35:**

> ```text
> SONUÇ: TEMİZ
> ```

**Parça 6 — başlık notu, §4.36–§4.72:**

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §4.61, §4.62
> Alıntı: "**Yalnız eklenen** — satırları güncellenmez (§2.2.3)."
> Sorun: Her iki geçmiş kaydı için `AdminId` alanının “kaldırılınca boşaltılacağı” da yazılmıştır. Bu, satırı güncellemeden yapılamaz. Uygulama iki zıt davranıştan birini seçmek zorunda kalır: yönetici silme işlemi yabancı anahtara takılır ya da yalnız-eklenen geçmiş değiştirilebilir hâle gelir. Doküman içi çelişkidir (4).
> Öneri: `AdminId`yi baştan bağ olmayan, silinen yöneticinin kimliğini değer olarak saklayan alan yapın; veya yalnız-eklenen kuralını referans bağının `SET NULL` güncellemesini açıkça istisna tutacak şekilde değiştirin.
>
> SONUÇ: 1 BULGU
> ```

**Parça 7 — başlık notu, §5–§9:**

> ```text
> SONUÇ: TEMİZ
> ```

## 3. Bağımsız değerlendirme

Her bulgu dokümanın tamamına (v0.21), Ürün Gereksinimleri'ne (`02` v0.86), Kullanıcı Akışları'na (`03` v0.35), önceki turların raporlarına ve karar kaydına karşı okundu — parça 3 §2–§9'u, parça 4 §1'i ve §4'ü, parça 6 §2'yi görmedi.

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| B1 · BULGU-1 | ❌ RET | **Kuralın evi Ürün Gereksinimleri'dir ve yasal dayanağı farklıdır; "gereksiz fazla saklama" iddiası tutmuyor.** Z-28'in başlangıcı — *"Siparişin oluştuğu takvim yılının sonu"*, on yıl — `02 §4.2` Z-28'in kuralıdır (K-353, K-357, K-519, K-582); Veri Modeli kural koymaz, başlangıcın alanını yazar (§9.1). Siparişin verisi — sipariş, ödeme, fatura, sözleşme kayıtları — ticari defterlerin dayandığı belgedir: 6102 sayılı Türk Ticaret Kanunu m.82/1 (d) ve /5 bu belgeleri **on yıl** saklatır, m.82/6 süreyi belgenin oluştuğu **takvim yılının sonundan** başlatır (mevzuat.gov.tr, 2026-10-18; audit'in hukuki tablosu da aynı sonucu kaydetmişti — `AUDIT_REPORTS/06_AUDIT.md`, 6102 m.82 ve 213 m.253 satırları). En uzun yükümlülük geçerlidir; yıl sonu başlangıcı TTK'nın asgarisidir, fazlası değildir. K-1092 6563 m.11/3'ün *"işlemin gerçekleştiği tarihten"* ilkesini **ticari defter belgesi olmayan** iki kayda — ayıp talebi ve gelen bildirim — uyguladı ve onları siparişin yıl sonu saklamasından ayırdı; siparişin kendisine dokunmadı. İki kural aynı ilkenin farklı uygulanışı değil, iki ayrı yükümlülüktür. Önerilen değişiklik `02 §4.2` Z-28'i ve TTK'nın başlangıcını kırar. Bulgu ilk kez geldi. | — |
| P3 · BULGU-1 | ❌ RET (ikinci ret) | **1. turun reddettiği bulgunun aynısı (`06_CROSS_REVIEW.md` §3, P3 · BULGU-1) — aynı yer, aynı iddia.** Matrisin `03 §1.2` S2 ve S10 satırları Kullanıcı Akışları'nın satırlarını özetler: S2'nin olayı *"kapanış kuralını tamamlayan son olay"*dır, kural `03 §1.4.3`'tür ve matris iki satır aşağıda onu yazar — m1 *"En az bir kalem teslim edilmişse Teslim edildi (S2, S10, S11)"*, m2 *"Hiçbir kalem teslim edilmemişse İptal edildi"*. Kaynağın kendisi de S2 ve S10'da koşulu §1.4.3'e bırakır; satırlar birlikte okununca tek okuma kalır. Hangi geçişin işleyeceği ürün kuralıdır (`02 §5.4`, `03 §1.4.3`); Veri Modeli geçiş kimliğini ve kaydını taşır (§5.2, §4.19). İstem matris özetinin kaynağa uygunluğunun denetlenmemesini söyler. Bulgu 2. ve 3. turda dönmedi; bu ikinci reddidir — 5. turda aynı yer ve iddiayla dönerse tekrar eden RET'tir. | — |
| P3 · BULGU-2 | ⚠️ KISMİ (düşük) | **İki depolama yeri yok; matrisin bir özet hücresi alanın yerini belirsiz söylüyordu.** Teslim tarihi tek alandır: `OrderItem.DeliveryDate` (§4.18) — *"sipariş düzeyinde bir kez girilir ve iptal edilmemiş ve çıkarılmamış fiziksel kalemlerin tamamına aynı yazılır"*; düzeltmesi bütün teslim işaretli fiziksel kalemlerde birlikte yapılır (`02 §3.20.11`, §10.4.5). `Order`'da teslim tarihi alanı yoktur (§4.17); "sipariş düzeyinde" sözü girişin ve kuralın düzeyidir, saklamanın değil. Matrisin S6, `03 §1.7.1.1`, `02 §3.20.11` ve §5.8 m1 satırları bunu doğru söyler; parça 3'ün göremediği §4.18 de. Cayma ve ayıp sürelerinin yanlış hesaplanması yolu yoktur. **Kalan gerçek fark:** `03 §8.2` satırının hücresi *"`DeliveryDate` (sipariş düzeyi)"* diyor ve satırın entity sütunu `Order`'ı da sayıyor — alan `Order`'da sanılabilir. İki yerin metin farkı (ölçünün (4). sınıfı, düşük). Önerinin "tek yetkili alanı seç" kısmı zaten karşılanmıştır. | `06 §1.1.10` `03 §8.2` satırı: *"`DeliveryDate` (fizikselde sipariş düzeyinde bir kez girilir, kalemlere yazılır — `02 §3.20.11`)"*. Kalıp taraması: matriste "sipariş düzeyi" geçen öteki satırlar (`02 §3.20.11`, §5.8 m1, §7.2.2, §10.4.5, §10.5, `03 §1.2` S6, §1.7.1.1, §2.8) alanın yerini doğru söyler ya da teslim tarihini anmaz — değişmedi. |
| P4 · BULGU-1 | ❌ RET (ikinci ret) | **1. turun reddettiği bulgunun aynısı (`06_CROSS_REVIEW.md` §3, P3 · BULGU-2 = P4 · BULGU-1) — aynı yer, aynı iddia.** Açık siparişi olan varyantın silinebilmesi ve ayırmasının varyantla düşmesi Ürün Gereksinimleri'nin bilinçli kuralıdır (`02 §3.7.7`; K-655): *"silinen varyantın ayırması varyantla birlikte düşer — ödeme gelince kesin düşülecek, iptalde geri dönecek bir stok yoktur. Siparişi karşılamayacak firma onu firma iptaliyle kapatır"*; panel silmeden önce açık siparişlerin sayısını söyler. Veri Modeli bunu dördüncü yazım yolu olarak yazar (§4.14 (4)) ve §2.7.1 geçiş biriminin dışındaki bu tek yolu adıyla sayar. Kupon iddiası metne uymaz: kupon hakkının ayırma satırında `VariantId` boştur (`VariantId` ile `CouponId`'den tam biri doludur — §4.14); basamaklanma yalnız varyant satırını siler, kuponun ayrılmış hakkı kendi satırında kalır. Önerilen `NO ACTION` K-655'i tersine çevirir. Parça 4 §4.14'ü görmedi; §2 ile §4'ü birlikte gören iki birleşim bu iddiayı getirmedi. Bu ikinci reddidir — 5. turda dönerse tekrar eden RET'tir. | — |
| P6 · BULGU-1 | ✅ KABUL (düşük) | **İstisna yalnız geçiş kaydı için yazılıydı.** §2.2.3 siparişin iki geçmişini "uygulama düzeyinde yalnız eklenen" kayıtlar arasında sayar ve istisnayı yalnız geçiş kaydına yazar: *"geçiş kaydında imhanın ve yöneticinin kaldırılmasının boşaltması istisnadır (§4.19)"*. Oysa §2.2.5 iki geçmişin yönetici bağının da boşaltıldığını sayar ve §4.61, §4.62'nin `AdminId` satırı, indeks ve silme maddesi bunu yazar; iki tablonun giriş ve Kısıtlar maddeleri ise istisnasız *"satırları güncellenmez"* der. Mekanizma tek okumalıdır — §2.2.5'in bağı boşaltılır, ad değer olarak kalır; "yalnız eklenen" veritabanı yetkisi değil uygulama düzeyinde bir korumadır (§2.2.3; yalnız işlem izi yetkiyle korunur) — yönetici silinmesi takılmaz, veri kaybı yoktur. Model şiddeti "Orta" dedi; değerlendirme: düşük, iki yerin metin farkı (ölçünün (4). sınıfı). Önerinin ilk kolu (bağı kaldırıp kimliği değer olarak saklamak) K-1010 (4)'ün kalıbını değiştirir — elendi; ikinci kolu (istisnayı açıkça yazmak) uygulandı. | §2.2.3 istisnası siparişin iki geçmişini de sayar (§4.19, §4.61, §4.62; §2.2.5); §4.61 ve §4.62'nin giriş ve Kısıtlar maddeleri *"yöneticinin kaldırılmasının boşaltması dışında"* der. **Kalıp taraması:** §2.2.3'ün saydığı yalnız eklenen kayıtların hepsi bağlarına karşı okundu — fiyat geçmişi (§4.11) varyantla basamaklanır, giriş kaydı (§4.44), yasal metin sürümü (§4.56), imha kaydı (§4.64) ve kesinti kaydı (§4.66) boşaltılan bağ taşımaz; geçiş kaydı istisnayı zaten taşır (§4.19). §3'ün özet satırları ("yalnız eklenen") ve §6, §7.2'nin bağ satırları ("boşaltılır") değişmez. |

**Dağılım:** beş ayrı bulgu — 1 KABUL · 1 KISMİ · 3 RET (ikisi ikinci ret). **Şiddet:** kabul edilen ikisi de düşük, ikisi de iki yerin metin farkı; ölçünün (1)–(3). sınıflarında kabul yoktur. **%100 kabul ya da ret değil.** Ret gerekçeleri somut yerlere (`02 §4.2` Z-28, TTK m.82; `03 §1.4.3`; `02 §3.7.7`, K-655, §4.14) ve 1. turun raporuna dayanır. **Yeni karar:** yok — iki düzeltme metin hizalamasıdır, ürün kuralına ve mekanizmaya dokunmaz.

**Hukuki kontrol** (2026-10-18). Birleşim 1'in bulgusu 6563 sayılı Kanun m.11/3'e dayanır; dayanağın kendisi tartışılmıyor — iddia bu ilkenin siparişe de uygulanması gerektiğidir. Siparişin saklamasının dayanağı 6102 sayılı Türk Ticaret Kanunu m.82'dir; metni mevzuat.gov.tr'de okundu (m.82/1 (d), /5 on yıl, /6 takvim yılının sonundan başlangıç) — audit'in 2026-10-17 tarihli hukuki tablosuyla aynı. Bulgu yeni bir hukuki içerik getirmedi.

## 4. Ek bulgular

- **Kullanım sınırı turun biçimini bozmadı.** Yeniden koşulan beş çağrı ilk koşumla aynı girdi dosyalarını okudu; turun dokuz çağrısının hepsi v0.21'e karşı koştu. Ölçüm: tamamlanan dokuz çağrı yaklaşık 1,23 milyon token kullandı (sınıra takılan iki çağrı ayrıca 0,2 milyon); bir günde dört turun sonunda hesap sınıra takıldı — sonraki turlarda dokuz çağrıyı tek seferde koşmadan önce sınırın açık olduğu deneme çağrısıyla doğrulanır; sınıra takılan çağrı aynı girdiyle yeniden koşulur.
- **Mekanik tarama.** İki düzeltme K numarası eklemedi ya da çıkarmadı; matris satırı eklenmedi ya da çıkmadı — kümeler değişmez; bağ, dizin ve silme kategorisi değişmedi — §6/§7 karşılaştırması gerekmedi. Başlık sürümü, yazım durumu notu ve dosya sonu dipnotu v0.22'yi gösterir. Değişen dosyalarda CR ve kontrol karakteri sıfır, eklenen satırlarda `$` artığı yok; karar kaydında çift numara yok.
- **Etki yansıtmaya hazırlık (Faz 5'in girdisi, bu turda dokunulmadı).** 3. turun iki notu (`04` 2.12.8 — K-1100; `04` 9.11.3 — K-1099), K-1036'nın `08` hücresine "park gerekmez" eki ve deep review'ın uygulanmayan bulguları maddesi yerinde bekler. Park satırlarının iki yönlü taraması (`cr3/f5_park.py`, K-992…K-1128) bu turda yeniden koştu — 3. turla aynı sonuç: park bloğunda olup etki sütununda hedefi olmayan karar yok; tek kaçak K-1036 → `08`. **Kaynak satırları taraması** (Faz 5'in 4. maddesi) için bir betik hazırlandı (`cr4/kaynak_scan.py` — bölümün gövdesinde anılan her K numarası bölümün Kaynak satırında mı; aralıklar açılır); `06`'da ön koşum 114 Kaynak satırının 24'ünde eksik atıf gösteriyor (ör. §4.5, §4.9, §4.47, §4.48, §4.51 — K-1070; §4.17 — K-1051, K-1086; §2.5; §3.1'in tablo hücreleri). Bir kısmı tablonun kendi Kaynak sütunudur ve okunarak elenmelidir; tarama TEMİZ turun Faz 5'inde koşar ve kapanır.
- **Ciddi bir sorun görülmedi** — ikinci modelin listesi dışında ölçünün dört sınıfından birine giren bir sorun saptanmadı.
- **Önceki turlardan açık kalanlar:** 3. turun reddi (ayıp talebinin saklama başlangıcı — K-1092) dönmedi. 1. turun iki reddi bu turda ikinci kez döndü (P3 · BULGU-1, P4 · BULGU-1); 2. turun dört reddi dönmedi.

## 5. Kullanıcı onay checklist'i

K-992 kararıyla (Veri modeli aşaması boyunca ⚠ öneriyle kayıt, CI yeşilse `docs:` PR merge yetkisi ve yönetici + alt ajan düzeni) alt ajan değerlendirdi ve uyguladı (`06` v0.22). Bu turda karar satırı açılmadı — iki düzeltme metin hizalamasıdır.

- [x] B1 · BULGU-1 (ret — Z-28'in yıl sonu başlangıcı Ürün Gereksinimleri'nin ve TTK m.82'nin kuralıdır)
- [x] P3 · BULGU-1 (ikinci ret — S2/S10'un kapanış koşulu `03 §1.4.3`'tedir; 1. tur)
- [x] P3 · BULGU-2 (kısmi — matrisin `03 §8.2` hücresi)
- [x] P4 · BULGU-1 (ikinci ret — varyantın silinmesinde ayırmanın düşmesi K-655; 1. tur)
- [x] P6 · BULGU-1 (kabul — §2.2.3, §4.61, §4.62)
- [ ] Etki yansıtma (Faz 5) — döngü TEMİZ döndükten ya da çıkış kuralıyla kapandıktan sonra, son turun raporunda

## 6. Sonuç ve sonraki tur

**Sonuç: TEMİZ değil** — dokuz çağrının dördü bulgu döndürdü; bir bulgu kabul, biri kısmen kabul edildi. Turun TEMİZ sayılması için bütün çağrıların `SONUÇ: TEMİZ` dönmesi ya da yalnız tekrar eden RET getirmesi gerekir (skill Faz 4 madde 4–5); bu turda tekrar eden RET yoktur — dönen iki bulgu önceki turlarda bir kez reddedilmişti. **Çıkış kuralı** (skill Faz 4 madde 5; Arayüz Tanımları'nda K-849, Teknik Mimari'de K-976) *"beşinci turdan itibaren"* uygulanır; bu tur dördüncüdür — uygulanmaz. Faz 5 (etki yansıtma) bu turda koşmaz.

**Yakınsama ölçüsü:** ayrı bulgu 7 → 8 → 1 → 5; kabul ya da kısmi 4 → 4 → 0 → 2; turun yeni ürün kararı 0 → 1 → 0 → 0; tekrar eden RET 0 → 0 → 0 → 0; TEMİZ çağrı 3 → 2 → 8 → 5. Kabul edilen iki bulgu düşük şiddette iki yerin metin farkıdır; doküman bu turda yalnız beş cümle eki kadar büyüdü. Üç reddin ikisi 1. turun bulgularının dönüşüdür — modelin örnekleme davranışı, dokümanın yeni kusuru değil.

**5. tur için not:** aynı istem, parça notu ve betiklerle (`06_CROSS_REVIEW.md` §1; K-1126); çağrılardan önce deneme çağrısı; birleşim 1'in payı yaklaşık 66 000 karakter. **Çıkış kuralı 5. turda uygulanabilir:** kabul edilen bulguların hepsi düşük şiddette iki yerin metin farkıysa bulgular uygulanır, kalıp dokümanın tamamında taranır ve döngü kapanır ("TEMİZ — çıkış kuralıyla, 5 tur"); sınıfın kalan riski Aşama 5'in checkpoint'ine devredilir (§14.1'e sınıfın adı ve 1.–5. turların örnekleriyle). Dönerse tekrar eden RET sayılacaklar (önceki turlarda iki kez reddedildi): gönderim kaydının saklama başlangıcı (K-1066; 1. ve 2. tur), S2/S10'un kapanış koşulu (1. ve 4. tur), varyantın silinmesinde ayırmanın düşmesi (K-655; 1. ve 4. tur). Bir kez reddedilenler kendi gerekçesiyle yeniden okunur: Z-28'in yıl sonu başlangıcı (bu tur), ayıp talebinin saklama başlangıcı (K-1092; 3. tur), 2. turun dört reddi. TEMİZ ya da çıkış kuralıyla kapanan turun Faz 5'i §4'ün hazırlık maddesini kapatır.
