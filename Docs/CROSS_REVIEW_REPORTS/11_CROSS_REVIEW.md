# Cross-Review — 11 Implementation Planı (Tur 1)

**Tarih:** 2026-10-31 · **Hedef:** `Docs/11_IMPLEMENTATION_PLAN.md` v0.37 (girdi commit'i `d886d54`, 748 006 bayt, 7 760 satır) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Ciddiyet ölçüsü:** 1. turdan itibaren uygulanır, mekanizma okuma paragrafıyla (K-1399; kuralın ayrıntısını `02`–`10`'dan devralan ve kendi mekanizma kararlarını koyan türetim dokümanı — `cross-review` skill'i, "Ne zaman uygulanır" ikinci madde) · **Koşum:** iki koşum — tek parça çağrı ve beş parçalı kontrol koşumu (doküman 400 KB'ı aşar) · **Sonuç:** **TEMİZ DEĞİL** — dokuz ayrı bulgu (dokuz ham); dördü (üç kısmi, bir doğru) uygulandı, beşi reddedildi.

## 1. Koşum

**Betikler** (sonraki turlar aynılarını kullanır; depoda değil, adım ajanının çalışma dizininde): `C:\Users\teknodeveloper\.claude\helper-state\shopfolio\kd-cross\betik\` — `build_inputs.py` (girdileri kurar: `python -I build_inputs.py <doküman> template11.md prompt.txt partnote.txt <girdi klasörü>`; parça sınırları betiğin `BOUNDS` listesinde, başlık önekiyle; kapsanmayan satırları basar — bu turda `none`), `run_codex.sh` (çağrıları paralel koşar: `bash run_codex.sh <girdi> <çıktı> c1 p1 p2 p3 p4 p5`), `prompt.txt`, `partnote.txt`, `template11.md` (şablon: `git show e74883f:Docs/11_IMPLEMENTATION_PLAN.md`, 4 879 bayt; depoda ayrı bir `templates/` klasörü yoktur), `paket.py` (doğrulayıcı paketlerini kurar), `bayt.py` (blok bayt ölçümü).

**İstem.** Kodlama Kılavuzu turunun istemi (`09_CROSS_REVIEW.md` §1) bu dokümana uyarlandı: bağlam paragrafı (Implementation Planı; kuralı `02`–`10`'dan devralır; fazlar, task blokları, kabul kriterleri, faz geçiş kapıları, yürütme sırası, ileri ve geri izlenebilirlik matrisi, boşluk raporu; o dokümanlar verilmedi), yedi kriter, bulgu biçimi (`BULGU-N` — Kriter · Seviye · Yer · Alıntı · Sorun · Öneri — ya da `SONUÇ: TEMİZ`), `cross-review` skill'inin Faz 1 madde 4'teki bilinçli karar cümlesi değiştirilmeden, ciddiyet ölçüsünün paragrafı ve skill'in "mekanizma kararı koyan dokümanda okuma paragrafı" bu dokümanın mekanizmalarına (task sırası ve bağımlılığı, faz geçiş kapısı, kabul kriteri ↔ test beklentisi, blok yazım konvansiyonu, izlenebilirlik kuralı) uyarlanmış hâliyle. Girdi yalnız doküman ve şablondur (Faz 1 madde 2); karar kaydı ve önceki turların değerlendirmesi verilmedi.

> Sen bağımsız bir doküman denetçisisin. Aşağıda "=== DOKÜMAN ===" işaretinden sonra bir Implementation Planı (uygulama planı) dokümanının metni, "=== ŞABLON ===" işaretinden sonra o dokümanın boş şablonu var. Başka bir dosya okuma; çalışma klasörü boştur.
>
> Bağlam: Belge bir Implementation Planı dokümanıdır; kodlayan ve doğrulayan yapay zeka ajanları her task'ta bu planın ilgili bloğunu okur. Ürün kuralı koymaz: ürünün davranışı ayrı bir Ürün Gereksinimleri dokümanında (`02`), kullanıcı akışları `03`'te, ekranların davranışı `04`'te, mekanizma ayrı bir Teknik Mimari dokümanında (`05`), alan ve değer adları ayrı bir Veri Modeli dokümanında (`06`), uç envanteri ve adresler ayrı bir API Tasarımı dokümanında (`07`), dış servislerle konuşma biçimi ayrı bir Entegrasyon Spesifikasyonları dokümanında (`08`), kodlama kuralları ayrı bir Kodlama Kılavuzu dokümanında (`09`), kapsam `10`'da durur; hiçbiri sana verilmedi. Bu doküman o dokümanların kuralını task'lara böler ve kendi mekanizma kararlarını koyar — fazlar ve faz bağımlılıkları, task blokları (bağımlılık, okunacak dokümanlar, kabul kriterleri, test beklentisi, doğrulama kontrol listesi), task yürütme sırası, faz geçiş kapıları, kaynak öğelerden task'lara ileri ve geri izlenebilirlik matrisi, boşluk raporu, blok yazım konvansiyonu. Kural ayrıntısının burada yeniden yazılmamış olması ya da bir kurala yalnız bölüm numarası ve kimlikle (`02 §…`, `04 §…`, `05 §…`, `06 §…`, `07 §…`, `08 §…`, `09 §…`, UC-nn, ÖK-n, YH-n) işaret edilmesi tek başına bulgu değildir; o dokümanların içeriğini denetleme. Dokümandaki `K-123` gibi işaretler sana verilmeyen bir karar kaydına gider. Yazım konvansiyonları başlık notundadır ("Yazım konvansiyonları"). Proje Aşama 9'dadır (Implementation Planı); kod henüz yazılmadı; veritabanı PostgreSQL 18, çatı ASP.NET Core Razor Pages ve EF Core. Ürün, Türkiye'de tüketiciye satış yapan tek firmalık bir e-ticaret + kurumsal sitedir. Yasal iddiaları (mesafeli satış, KVKK, ödeme hizmetleri, e-ticaret mevzuatı) yürürlükteki resmî metne göre değerlendir.
>
> Dokümanı yedi kriterle denetle: tutarlılık · eksiklik · belirsizlik · teknik doğruluk · edge case · güvenlik · kullanıcı deneyimi.
>
> Dokümanda açıkça bilinçli karar, elenen seçenek ya da kabul edilmiş risk olarak yazılmış bir seçimi, yalnız seçime katılmadığın için bulgu yapma. Böyle bir karar dokümanın kendi içinde çelişki, olgusal ya da hukuki hata doğuruyorsa yaz.
>
> Yalnız CİDDİ sorunları yaz: (1) yürürlükteki mevzuata aykırılık, (2) müşteriye ya da firmaya para kaybı veya hak kaybı doğuran kural boşluğu, (3) kullanıcının takılıp kaldığı, çıkışı olmayan bir akış, (4) dokümanın iki yerinin birbiriyle çelişmesi. Nadir kenar durumları, iyileştirme önerilerini, savunmada derinlik önerilerini, üslup ve ifade tercihlerini bulgu yapma. Böyle bir sorun yoksa SONUÇ: TEMİZ yaz.
>
> Teknik doğruluk ve güvenlik bu dört sınıfın içinde okunur: yazılan bir mekanizmanın — bir task sırası ya da bağımlılığı (bir task'ın, kendisinden sonra gelen ya da hiç tanımlanmamış bir task'ın çıktısına dayanması), bir faz geçiş kapısı, bir kabul kriteri ile test beklentisinin birbirini tutması (kriterin hiçbir testle sınanmaması ya da test beklentisinin kriteri aşması), bir blok yazım konvansiyonu ya da bir izlenebilirlik kuralı (bir kaynak öğenin hiçbir task'a bağlanmaması ya da bir task'ın hiçbir kaynak öğeye dayanmaması) — dokümanın kendi koyduğu bir güvenceyi (ör. her kaynak öğenin bir task'ta karşılık bulması, bir task'ın başlamadan önce bütün girdilerinin hazır olması, bir fazın kapanmadan sonraki faza geçilmemesi, bir kabul kriterinin sınanabilir olması) sağlamaması (4)'tür; bir mekanizmanın para, sipariş ya da kişisel veri akışının uygulanmadan kalmasına veya yanlış uygulanmasına yol açması (1) ya da (2)'dir. Ek bir koruma katmanı, ek izleme, daha güçlü bir araç ya da yöntem önerisi savunmada derinliktir ve bulgu değildir.
>
> Çıktı biçimi — her bulgu için (Türkçe):
>
> BULGU-N
> Kriter: <yedi kriterden biri>
> Seviye: Yüksek | Orta
> Yer: <bölüm / madde kimliği>
> Alıntı: "<dokümandan aynen>"
> Sorun: <ne yanlış, neden ciddi — dört sınıftan hangisi>
> Öneri: <somut düzeltme>
>
> Bulgu yoksa yalnız şunu yaz: SONUÇ: TEMİZ
> Bulgu varsa listenin sonuna şunu yaz: SONUÇ: <N> BULGU

Parçalı çağrıların istemine aynı tek paragraflık parça notu eklenir (`partnote.txt`; `<bölümler>` aşağıdaki tablonun adlarıdır): verilmeyen bölüme yapılan atıf ve orada yaşayan içeriğin görünmemesi bulgu değildir.

**Komut** (Git Bash; çalışma klasörü boş): `codex exec -s read-only -C <boş klasör> --skip-git-repo-check --ephemeral --ignore-user-config --color never -m gpt-5.6-terra -c 'model_reasoning_effort="high"' -o <çıktı> - < <girdi>`. Altı çağrı paralel koştu (her çağrının çalışma klasörü kendi boş klasörüdür); kullanım sınırı dolmadı, çıktısız dönen çağrı olmadı, yedek modele geçilmedi. Giriş denetimi: `codex login status` → "Logged in using ChatGPT".

**Parça sınırları — kayıt (K-1478).** Sınır satır numarasıyla değil **başlıkla** tanımlıdır ve sonraki turlarda değişmez. **Başlık notu** dokümanın ilk satırından `## 1. Amaç ve kapsam` başlığına kadardır (sürüm, Aşama 8 park bloğu ve durum tablosu, yazım konvansiyonları, okuma tablosu, yazım turunun haritası; satır 1–64, yaklaşık 11,6 KB); her parça başlık notunu ve şablonu taşır. Parçalar faz sınırlarından geçer; F3 (213 KB) `T346` başlığından ikiye bölünür (T301–T345 ve T346–T390).

| Parça | Başlar | Biter (dahil değil) | Bölümler | Satırlar | Girdi | Token | Süre | Sonuç |
|---|---|---|---|---|---|---|---|---|
| 1 | `## 1. ` | `### F2 ` | §1–§4 · §5 Task yapısı · F0 · F1 | 65–866 | 95 573 bayt | 66 774 | 112 sn | 1 bulgu |
| 2 | `### F2 ` | `#### T346: ` | F2 · F3 (T301–T345) | 867–2841 | 195 983 bayt | 99 543 | 290 sn | 2 bulgu |
| 3 | `#### T346: ` | `### F5 ` | F3 (T346–T390) · F4 | 2842–4219 | 150 247 bayt | 86 414 | 215 sn | TEMİZ |
| 4 | `### F5 ` | `## 7. ` | F5 · F6 · §6 | 4220–5633 | 148 879 bayt | 83 268 | 195 sn | 1 bulgu |
| 5 | `## 7. ` | dosya sonu | §7 · §8 · §9 | 5634–7760 | 253 915 bayt | 132 782 | 78 sn | 2 bulgu |

Parça girdileri 96–254 KB'tır (başlık notu 11,6 KB'ı her parçada tekrarlar); kapsanmayan satır yoktur (betiğin denetimi: `uncovered lines: none`).

**(a) Tek parça çağrı.** Girdi 757 551 bayt (701 808 karakter; aracın 1 048 576 karakterlik sınırının altında), 414 619 token, 353 sn: 3 bulgu. Tek parça girdi sınırına takılmadığı için birleşim çağrısı gerekmedi.

**Kapsam sınırı.** Parça öteki bölümleri görmez; tek parça çağrı parçalar arası okumayı yapar. Değerlendirme her iddiayı dokümanın tamamına ve ilgili yerde `02`–`10`'a karşı okudu (§3). **Web araması:** çağrı günlüklerindeki web araması satırı sayısı: tek parça 3, her parçada 2 (mevzuat.gov.tr, ticaret.gov.tr, gib.gov.tr sitelerine kısıtlı aramalar: Mesafeli Sözleşmeler Yönetmeliği, ETBİS, fatura düzenleme); yasal dayanak taşıyan tek bulgu F1'dir ve resmî metne (Mesafeli Sözleşmeler Yönetmeliği m.10, RG 27.11.2014/29188) karşı doğrulayıcıda okundu (okuma tarihi 2026-10-31); bulgunun gösterdiği bağlantı Yönetmelik değil 6502 sayılı Kanun'un PDF'iydi.

**İki koşumun ölçümü (skill, Faz 1 madde 9).** Parçalı koşum tek parça çağrının görmediği altı ayrı bulguyu getirdi (F4–F9); ikisi kabul/kısmi edildi (F5, F6), biri kabul (F7), üçü reddedildi (F4, F8, F9). Tek parça çağrı F1–F3'ü getirdi: F1 (T371 ↔ T528, iki fazı birlikte okuyarak; kısmi) parçalı koşumda yoktu, F2 ve F3 reddedildi. **İki koşum sonraki turlarda da sürer.**

## 2. Ham bulgular

İkinci modelin çıktıları, satır sonu boşlukları dışında düzenlenmeden (alıntı blokları içinde).

**(a) Tek parça çağrı:**
> BULGU-1
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: T371 kabul kriteri 1 / KK1; §3.1
> Alıntı: “Cayma penceresi on dört gündür ve sabittir; firma uzatamaz, ayar değildir; m.10'un yasal uzaması pencere dışı kayıtla işler (`02 §7.3.1`; T528 k5).”
> Sorun: T371 F3’te kapanırken dayanak gösterilen T528 F5’tedir; fazlar arasında örtüşme yasaktır. Ayrıca T371’in KK1’i yalnız sabit 14 günlük süreyi sınar. Bu nedenle eksik cayma bilgilendirmesinde uygulanması gereken uzatılmış süre için F3 kapısından önce uygulanmış ve sınanmış bir mekanizma yoktur. Mesafeli Sözleşmeler Yönetmeliği m.10, eksik bilgilendirmede tüketiciyi 14 günlük süreyle bağlı tutmaz; süre en geç bir yıl sonra biter ve bilgi sonradan doğru verilirse yeni 14 gün başlar. Bu boşluk tüketicinin cayma hakkının reddine yol açabilir. [Ticaret Bakanlığı – Mesafeli Sözleşmeler Yönetmeliği m.10](https://tuketici.ticaret.gov.tr/data/5e81982d13b876a1b04c7a42/2023-6502%20Say%C4%B1l%C4%B1%20T%C3%BCketicinin%20Korunmas%C4%B1%20Hakk%C4%B1nda%20Kanun.pdf)
> Öneri: Uzatılmış cayma süresi hesaplamasını ve bilgilendirme kanıtını T371/F3’e alın; normal süre, eksik bilgilendirmede bir yıllık üst sınır ve sonradan bilgilendirmede başlayan yeni 14 gün için kabul kriteri ve entegrasyon testi ekleyin. T528 yalnız arayüzü tüketmelidir.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: T117 kabul kriteri 3 / KK3
> Alıntı: “`Undelivered`'da yalnız gönderildikten sonra geri dönen satırda doludur (K-1120).” / “gönderim anı dolu da boş da `Undelivered` satır kabul edilir”
> Sorun: Aynı durum için kabul kriteri gönderim anını zorunlu, doğrulama ise isteğe bağlı kılıyor. Bu, kabul kriteri ile testi çeliştiren bir plan mekanizması hatasıdır (4); gate, boş gönderim anlı kaydın geçerli olup olmadığına karar veremez.
> Öneri: Kaynak kurala uygun tek davranışı seçin. “Gönderildikten sonra geri dönen” anlamı korunacaksa `Undelivered` kaydında gönderim anını zorunlu tutup KK3’ü buna göre değiştirin.
>
> BULGU-3
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: T312 kabul kriteri 8 / KK8
> Alıntı: “`Content-Range` yoksa ya da toplamı aşarsa 400 alır” / “uymayan başlangıçlı ve toplamı aşan parça 409 alır”
> Sorun: Toplam boyutu aşan aynı parça için iki farklı HTTP sonucu tanımlanmış. Kabul kriteri ile doğrulama kontrol listesi birbirini tutmadığından uygulama ve yükleme betiği için doğrulanabilir tek sözleşme yoktur (4).
> Öneri: Toplam boyut aşımının durum kodunu tekleştirin; başlangıç uyuşmazlığı 409 olarak kalacaksa bunu toplam boyut aşımından açıkça ayırın ve KK8’i kabul kriteriyle aynı sonucu sınayacak biçimde güncelleyin.
>
> SONUÇ: 3 BULGU

**(b) Parça 1 (§1–§4, F0, F1):**
> BULGU-1
> Kriter: tutarlılık
> Seviye: Orta
> Yer: Başlık notu “Not” / §5, T02 kabul kriteri 8
> Alıntı: “Bu doküman **tüketici**dir — `02`–`10`'u tüketir, onlara geriye etki yaratmaz.” / “çıkan fark ilgili `09` satırına aynı PR'da yazılır”
> Sorun: Plan, kaynak dokümanlara geriye etki yapmayacağını açıkça söylerken T02, bir kaynak olan `09`un aynı PR’da değiştirilmesini zorunlu kılıyor. Bu, dokümanın kendi tüketici ilkesiyle çelişir (4).
> Öneri: T02’den `09`u değiştirme zorunluluğunu çıkarın; farkı karar kaydı/GAP olarak kaydedip kaynak doküman sahibine iletin. `09` değişirse planı sonradan kaynakla hizalayın.
>
> SONUÇ: 1 BULGU

**(b) Parça 2 (F2; F3 T301–T345):**
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: T209 kabul kriteri 7; T224 kabul kriteri 9
> Alıntı: “içerik güvenlik politikası dış betiği ve gömme içeriği yükletmez” / “tek istisna ödeme sağlayıcısı çerçevesinin zorunlu çerezleridir”
> Sorun: Ödeme sağlayıcısının çerçevesi gömülü içeriktir; T209 bunu tüm yanıtlarda yasaklarken T224 ödeme çerçevesini varsayar. Ödeme adımı tarayıcıda engellenebilir ve müşteri satın almayı tamamlayamaz. Bu, doküman içi çelişki ve çıkışsız akıştır.
> Öneri: T209’a, yalnız ödeme adımında ve `08`’de tanımlı sağlayıcı kaynakları için dar kapsamlı `frame-src` (gerekiyorsa `script-src`) istisnası ekleyin; ödeme çerçevesinin CSP altında açıldığını doğrulayan kabul testi koyun.
>
> BULGU-2
> Kriter: Teknik doğruluk
> Seviye: Orta
> Yer: T203 kabul kriteri 9 ve KK9; F2 yürütme sırası
> Alıntı: “imzası tutmayan istek form gönderimi sınırını aşarsa 429 alır (T215 madde 4)”
> Sorun: T203’ün tek bağımlılığı T202’dir ve F2 sırası T203’ü T215’ten önce yürütür. Buna rağmen T203, T215’in henüz üretilmemiş hız sınırı davranışını kabul kriteri ve test koşulu yapar. Task, bütün girdileri hazır olmadan tamamlanmış sayılamaz; planın kendi bağımlılık güvencesi bozulur.
> Öneri: Bu 429 davranışını ve ilgili KK9 alt senaryosunu T215’e taşıyın; T215, T203’ün hazır imzalı uç altyapısına hız sınırını bağlayıp doğrulasın. Alternatif olarak hız sınırı altyapısını T203’ten önce tamamlanan ayrı bir task’a ayırın.
>
> SONUÇ: 2 BULGU

**(b) Parça 3 (F3 T346–T390; F4):**
> SONUÇ: TEMİZ

**(b) Parça 4 (F5, F6, §6):**
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: T518 KK3; T543 KK6
> Alıntı: “teslimat yapılmayan ildeki girdi tutulur” / “Teslimat yapılmayan ildeki adres kabul edilmez”
> Sorun: Aynı ortak adres formu için iki zıt uygulama kuralı veriliyor. Bu, adres defterine teslimat dışı ilde adres kaydedilip kaydedilemeyeceğini belirsiz bırakır; task’lar birbirinin çıktısını geçersiz kılar (4).
> Öneri: T543’te kuralı ödeme/teslimat adresiyle sınırlandırın: adres defteri teslimat dışı ili saklayabilsin; ancak sipariş teslimat adresi olarak seçildiğinde reddedilsin. Alternatif olarak T518’de saklama kuralını kaldırın.
>
> SONUÇ: 1 BULGU

**(b) Parça 5 (§7–§9):**
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §7.1.2, T548 ve §7.2 T548
> Alıntı: "`| T548 | F5 | Parçalı yükleme betiği ... | `09 §16.2`; `07 §4.31.2`; K-1354 |`" / "`| T548 | `09` §16; `devir` K-1354, K-1391, P8.5 |`"
> Sorun: T548’in aday kaynağı olan `07 §4.31.2`, ileri matriste kaynak satırı olarak, geri matriste ise T548 kaynağı olarak hiç yer almıyor. Bu, planın “çift yönlü eşitlik” ve her kaynak öğenin task’a bağlanması güvencesini bozar; parçalı yüklemenin API gereksinimi gate denetiminden kaçabilir.
> Öneri: `07:§4.31.2` için §7.1.2’ye T548 bağlantılı kaynak satırı ekleyin; §7.2’de T548 kaynaklarına aynı kimliği ekleyin ve envanter/bağ sayımlarını güncelleyin.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §7.1.2, aday task tablosu T549 ve §7.2 T549
> Alıntı: "`| T549 | F5 | Vitrin: arşivlenmiş ürün sayfası ... | `04` E-05; `07` UC-04 (K-1437) |`" / "`| 07 UC-04 GET `/urun/{Slug}` — Ürün adresi | T503 |`"
> Sorun: T549, `UC-04` kaynağına dayandırılmıştır; fakat ileri matriste UC-04 yalnız T503’e, geri matriste T549 ise yalnız `E-05`, `02 §3.7.6` ve `KP-7`ye bağlanmıştır. Bu, aday tablosu ile çift yönlü matrisin farklı kaynak kümeleri göstermesine neden olur ve arşivlenmiş ürün adresi davranışının API kaynağı gate’te doğrulanmaz.
> Öneri: `07 UC-04` ileri satırına T549’u ekleyin; §7.2 T549 satırına `07 UC-04`ü ekleyin ve bağlantı sayısını güncelleyin.
>
> SONUÇ: 2 BULGU

## 3. Bağımsız değerlendirme

Dokuz ham bulgu dokuz ayrı bulguydu (yinelenen yok). Yedi `[dogrulama]` Opus / high doğrulayıcıya verildi: yüksek F1 ve F5 tek tek (D1, D2); orta F2 ve F4 aynı birimde ikili (D3: T117 ve T02); orta F3 (D4), F6 (D5), F7 (D6) tek tek, çünkü ayrı fazlardandır; orta F8 ve F9 aynı bölümden (matris, §7) ikili (D7). Doğrulayıcı paketleri, hüküm dosyaları ve talimat `C:\Users\teknodeveloper\.claude\helper-state\shopfolio\kd-cross\r1\` altındadır (`dogrulayici_<k>.md`, `hukum_D<k>.md`, `talimat_dogrulayici.md`); hükümler `11_audit_hukum_denetle.py` ile (sorun 0) sınandı. %100 KABUL/%100 RET tablosu görünmüyor: bir doğru, üç kısmi, beş ret. Seviyeler doğrulayıcıda yeniden okundu: F1 Yüksek → Orta, F5 Yüksek → Düşük; reddedilenler ölçünün sınıflarına girmediği için Düşük.

| # | Bulgu (kaynak) | Karar | Gerekçe | Uygulama |
|---|---|---|---|---|
| F1 | Cayma süresinin yasal uzaması T371'de hesaplanmalı; T371 k1'in dayanağı F5'teki T528 (tek parça B-1; T371 k1; §3.1) | ⚠️ KISMİ | Yasal içerik doğru: Mesafeli Sözleşmeler Yönetmeliği m.10 (RG 27.11.2014/29188) eksik bilgilendirmede 14 günlük süreyi bir yıla uzatır; bulgunun bağlantısı Yönetmelik değil Kanun PDF'iydi. Ama ürün süreyi bilerek hesaplamaz: `02 §7.3.1` "ürün bilgilendirmenin eksik kalıp kalmadığına karar vermez … kapanmış düğmeyi yeniden açmaz"; m.10 yolu firmanın pencere dışı kaydıdır (`02 §10.4.10`, K-709: ayrı onay, pencere bitiminden bir yıl sonrası ret; `02 §6.4.32`); önerinin "T371'de hesapla" kolu üst dokümanla çelişirdi. Geçerli kısım sınıf (4): kural iş mantığıdır ve F3'tedir, ama T371 k1 onu F5'teki T528 k5'e işaret ediyordu (fazlar örtüşmez); F3'te bu davranışı sınayan task T372 k6'dır, o da yalnız ileri tarihi sınıyordu — F3 kapısı m.10 kaydı uygulanmadan geçebilirdi. Seviye Orta: canlı açılış F6'dadır ve T528 k5 F5'te sınar, açılışta hak kaybı doğmaz. | 5 çift: T371 k1 dayanağı T372 k6'ya; T372 k6 pencere dışı kaydı ve K-709'u yazar, k7 ve k8 bayt için kısaltıldı (anlam aynı), KK6 "pencere dışı onay, bir yıl" |
| F2 | `Undelivered` gönderim anı: k3 "yalnız gönderildikten sonra geri dönen satırda doludur" ↔ KK3 "dolu da boş da kabul edilir" (tek parça B-2; T117 k3/KK3) | ❌ RET | Çelişki yok: k3 iki hâli birlikte tarif eder — yeniden denemeleri başarısız kalıp hiç gönderilmemiş `Undelivered` satırda boş, gönderildikten sonra geri dönen satırda dolu (`06 §4.58` Kısıtlar; `08` "`SentAt` kalır"; K-1120). Veritabanı kısıtı `Undelivered`'da gönderim anını serbest bırakır (`06` kısıt özeti aynı cümle); KK3 kaynağın doğrudan karşılığıdır. Önerinin "zorunlu tut" yolu `06 §4.58`'e ve P-38'in "yeniden denemeler de başarısızsa ulaşmadı" koluna aykırı olurdu. | Yok |
| F3 | Toplamı aşan parça: k8 "400" ↔ KK8 "409" (tek parça B-3; T312 k8/KK8) | ❌ RET | Çelişki yok: k8 toplamı aşan parçaya tek başına 400 der; KK8 "denetim sırası:" başlığı altında iki koşulu birden taşıyan parçayı (uymayan başlangıç **ve** toplam aşımı) sınar ve kaynaktaki sıra "oturum → 404 → 413 → biçim 400 → 409 → boy aşımı 400/422" olduğundan o parça 409 alır (`07 §4.31.2`). KK8'in öteki iki örneği de aynı kalıpla öncelik sınar; "iki 400" ifadesi başlangıcı uyan toplam aşımı 400'ünü de kapsar. | Yok |
| F4 | T02 k8, `09`'u aynı PR'da değiştirmeyi zorunlu kılar ↔ "tüketici, geriye etki yaratmaz" (parça 1 B-1; başlık notu "Not"; T02) | ❌ RET | Başlık notunun tüketici ilkesi planın kendi yazımını ve düzeltmelerini kapsar ("Buradaki düzeltmeler yalnız bu dokümanı hizalar"). T02 k8'in `09` satırını aynı PR'da düzeltme yükü plandan değil kaynağın kendi kuralından gelir: `09 §4.1` madde 1 "SDK `.slnx`'i üretmiyorsa `Shopfolio.sln` yazılır ve bu satır aynı PR'da düzeltilir", yazım konvansiyonu 3 ve 8 ("fark kaynakta düzeltilir"). Tüketici plan kaynağın buyruğunu işaret eder; aynı kalıp planın `08 §4.1`.den türeyen başka satırlarında da vardır. Önerinin "GAP'e çevir" yolu planı `09 §4.1` ile çelişkiye sokardı. | Yok |
| F5 | CSP "gömme içeriği yükletmez" (T209 k7) ↔ ödeme sağlayıcısı çerçevesi (T224 k9); ödeme tarayıcıda engellenebilir (parça 2 B-1) | ⚠️ KISMİ | "Çıkışsız akış" iddiası geçersiz: ödeme sayfasının çerçeve izni üst dokümanda (`05 §6.5.4`: `frame-src` yalnız ödeme sayfasında, `'self'` ve sağlayıcı adresi) ve planda T510'dadır (T510 KK4 "Çerçeve izni yalnız bu sayfada, yalnız sağlayıcı adresi ve `'self'` için açılır"; DV-5); T209 KK7'nin testi yalnız dış betiği sınar, ödeme çerçevesini kırmaz. Kalan kısım sınıf (4): T209 k7 mutlak "gömme içeriği yükletmez" der ve `05 §6.5.4`'ün ödeme çerçevesi istisnasını düşürür; T510 KK4 ve T224 k9 ile lafzen çelişir. Önerinin T209'a kabul testi eklemesi gereksiz (test T510 KK4'te). Seviye Düşük. | 2 çift: T209 k7 "ödeme çerçevesi T510'dadır" parantezini taşır, KK7 kısaltıldı (T209 2.555 bayt) |
| F6 | T203 k9/KK9 "bozuk imzanın 429'u" (T215 k4) ancak T203 T215'ten önce yürür (parça 2 B-2) | ⚠️ KISMİ | T203'ün tek bağımlılığı T202'dir ve F2 sırasında T203 on altıncı, T215 yirmi dokuzuncudur; ama T203 k9 ve KK9 bozuk imzanın 429'unu kendi kabul koşulu yapıyordu, o davranışı kuran ve sınayan T215'tir (k4/KK4; `07 §2.8.3`; `05 §6.3.5`) — bildirilmemiş ileri bağımlılık, planın "her bağımlılık kendinden önce gelir" güvencesini bozar (4). Kısmi: önerinin "T215'e taşıyın" kolu gereksiz, T215 zaten taşıyor; düzeltme T203'te 429'u işaret parantezine indirmektir (T212 k5'in "413 T215'te" kalıbı). | 2 çift: T203 k9 ve KK9 429'u T215'e işaret eder (T203 2.022 bayt) |
| F7 | Adres: T518 KK3 "teslimat yapılmayan ildeki girdi tutulur" ↔ T543 k6 "kabul edilmez" (parça 4 B-1) | ✅ KABUL | T543 ortak adres bileşeninin k6'sı kapsamsızdır; T518 defter varyantında aynı bileşeni kullanıp girdiyi tutar ve T543'e bağımlıdır — bileşen kuralı bütün varyantlara uygulanırsa T518 KK3 karşılanamaz (4). Üst doküman tutarlıdır ve kapsamı teslimat varyantına sınırlar: `04` 2.12.2.3 "defterdeki o adres seçilemez", `04` 5.26.5 "Firmanın teslimat yapmadığı ildeki kayıt defterde tutulur; kısıt ödeme adımında işler"; seçim kısıtı T350 KK2 ve UC-27 (T509) tarafındadır. Önerinin ikinci yolu (T518'den saklamayı kaldırmak) `04` 5.26.5'e aykırıdır. | 2 çift: T543 k6 ve KK6 varyanta göre yazıldı (teslimat: reddedilir; defter: tutulur, ödemede seçilemez; T543 2.372 bayt) |
| F8 | T548'in aday kaynağı `07 §4.31.2` §7.1'de ve §7.2'de yok (parça 5 B-1) | ❌ RET | `07 §4.31.2` ayrı bir envanter birimi değildir; `07`'nin matris birimi uçtur ve §4.31.2'nin birimi `07 UC-83` satırıdır (UC-83 → T312); ileri yönde eşlenmemiş kaynak yoktur ve §7.1 ↔ §7.2 eşitliği betikle sağlanmıştır ("yalnız 7.1'de: 0, yalnız 7.2'de: 0"). T548'in matriste yalnız `09` + devir ile bağlanması bilinçli karardır (K-1435: "matrisin kaynak öğelerinde adıyla geçmeyen işlerdir; kaynak `09` bölümü olarak geri yönde yazılır"; K-1354); gerekçesiz ekleme taraması T548'i adıyla gerekçeli sayar. Aday tablonun "Kaynak birimi" sütunu §7.2'nin aynası değildir (T230, T503 de böyle), iki yer çelişmez; ucun API sözleşmesi T548 KK3/KK6'nın `07 §5.4.2` ve `§4.31.2` kanıtıyla denetlenir. | Yok |
| F9 | T549 aday kaynağı `07` UC-04, §7.1'de yalnız T503'te; §7.2 T549'da yok (parça 5 B-2) | ❌ RET | UC-04 satırının yalnız T503'te kalması K-1437 (3)'ün açık hükmüdür ("`04:E-05` satırının Task hücresi T503'ten T549'a geçti; UC-04 satırı T503'te kalır (uç tek, E-04'ün gövdesi)"); `07 §4.5.1` E-05 için "ayrı uç yoktur" der, yani T549'un özgün birimi `04:E-05`'tir ve §7.2 satırı §7.1 ile eşittir. Aday tablodaki `07 UC-04 (K-1437)` birim sayımı değil bölmenin kaynağına işarettir (T503'te "(3)" sayımı taşır); arşiv davranışı T549 KK1, KK3, KK6, KK7'nin `07 §4.5.1` atıflarıyla denetlenir. | Yok |

**Kısmi kalış:** F1, F5, F6 ikinci modelin önerisinin daraltılmış hâliyle uygulandı: F1'in "süreyi T371'de hesapla" kolu üst dokümanla çeliştiği için reddedildi, kalan (4) kısmı dayanağın F3'e alınmasıyla kapandı; F5'te yasaklayıcı cümle istisnayı işaret eder (test T510'da); F6'da 429 T215'e işaret eder. Beş ret doğrulayıcıda kaynakla (`06 §4.58`, `07 §4.31.2`, `09 §4.1` madde 1, K-1435/K-1354, K-1437 (3)) kanıtlandı; "ben böyle düşünüyorum" gerekçesi yoktur. Tekrar eden RET yok (ilk tur).

## 4. Ek bulgular

- **EK-1 (F6'nın doğrulayıcısı):** T212 k5 ve KK5 "limit aşımı 429" de T215'ten önce yürür (T212 T215'e bağımlı değil; T215 T212'ye bağımlı). T212'nin işi durum kodu eşlemesidir; 429'u üreten sınırlayıcı T215'tedir. F6 ile aynı kalıp; ayrı doğrulayıcıdan geçmedi, bu turda uygulanmadı — 2. turda aynı kalıp dönerse ele alınır.
- **EK-2 (F1'in doğrulayıcısı):** T372 bloğu 2.559 bayttır (sınır 2.560); sonraki ekler başka bir maddenin kısalmasını gerektirir. Uygulanacak bir şey yok.

## 5. Kullanıcı onay checklist'i

- [x] F1 — T371 k1 dayanağı T372 k6'ya; T372 k6 pencere dışı kayıt (K-1478)
- [ ] F2 — RET (uygulanmadı)
- [ ] F3 — RET (uygulanmadı)
- [ ] F4 — RET (uygulanmadı)
- [x] F5 — T209 k7 ödeme çerçevesi istisnasını işaret eder (K-1478)
- [x] F6 — T203 k9/KK9 429'u T215'e işaret eder (K-1478)
- [x] F7 — T543 k6/KK6 teslimat varyantına sınırlı (K-1478)
- [ ] F8 — RET (uygulanmadı)
- [ ] F9 — RET (uygulanmadı)

Gösterilmeden kaydedilen ⚠ karar yok; çok kritik soru yok. Önerilen `00`/`CLAUDE.md`/`SETUP.md` değişikliği yok.

## Sonuç

**SONUÇ: TEMİZ DEĞİL — 9 ayrı bulgu (9 ham), 1 kabul + 3 kısmi (11 şartname çifti), 5 ret.** Skill'in çıkış kuralı: bütün çağrılar aynı anda TEMİZ dönmedi (altı çağrının beşi bulgu getirdi, parça 3 TEMİZ) ve 5. tur eşiği (beşinci turdan itibaren çıkış kuralı) henüz gelmedi; kabul edilen bulgular arasında mevzuata aykırılık ya da para/hak kaybı sınıfından olan yok (F1 sınıf (4), yasal içerik bilinçli kararla karşılanmış; F5, F6, F7 sınıf (4)). Sonraki tur: 2. tur (`11_CROSS_REVIEW_R2.md`), aynı betikler ve aynı parça sınırlarıyla, `11` v0.38 üzerinde. Etki yansıtma (Faz 5) döngü TEMİZ olunca ayrı adımdır; bu turda yapılmadı.
