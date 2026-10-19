# Cross-Review — 07 API Design (Tur 1)

**Tarih:** 2026-10-19 · **Hedef:** `Docs/07_API_DESIGN.md` v0.17 (girdi commit'i `fda7b1b`, 1 186 845 bayt, 6 397 satır) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Ciddiyet ölçüsü:** 1. turdan itibaren uygulanır, mekanizma okuma paragrafıyla (K-1219; ayrıntısı başka dokümanda yaşayan türetim dokümanı — `cross-review` skill'i) · **Koşum:** iki biçimde — (a) tek parça çağrı aracın girdi sınırını aştı (1 048 576 karakter; girdi 1 091 742 karakter) ve yerine **en büyük iki birleşim** koştu; (b) **sekiz parçalı kontrol koşumu**, parça sınırları başlıkla sabit (K-1219); turun sonucu ikisinin birleşimidir · **Girdi:** yalnız doküman ve boş şablonu (`e74883f` — project-playbook'tan gelen v0.1, 2 684 bayt, olduğu gibi), stdin'den tek metin, her çağrıda boş ve izole bir çalışma klasöründe; karar kaydı, Ürün Gereksinimleri, Kullanıcı Akışları, Arayüz Tanımları, Teknik Mimari, Veri Modeli, MVP Kapsamı ve audit/deep review raporları verilmedi (K-431) · **Kullanım sınırı:** dolmadı — on bir çağrının on'u çıktı verdi, biri girdi sınırında hemen döndü; yedeğe geçilmedi, tur tek modelle koştu · **Sonuç:** TEMİZ değil — yedi ayrı bulgu (on ham bulgu), 4 KABUL · 1 KISMİ · 2 RET; beşi uygulandı (`07` v0.18) · **Değerlendirme modeli:** Claude alt ajanları yalnız Sonnet / low (K-1207); Codex dışarıdadır

## 1. Koşum

**Betikler** (sonraki turlar aynılarını kullanır; depoda değil, yönetici oturumunun scratchpad'inde): `C:\Users\TEKNOD~1\AppData\Local\Temp\claude\c--projects-Shopfolio\422e2f0a-46a6-4ced-809c-e90f0f447f55\scratchpad\cr07\` — `build_inputs.py` (girdileri kurar: `python -I build_inputs.py <doküman> template07.md prompt.txt partnote.txt <girdi klasörü>`; parça ve birleşim sınırları betiğin `PARTS` ve `COMBOS` listelerinde, başlık önekiyle; kapsanmayan satırları da basar — bu turda `none`), `run_codex.sh` (çağrıları koşar: `bash run_codex.sh <girdi> <çıktı> [adlar…]`; en çok altısı paralel; her çağrının çıkış kodunu, süresini, token sayısını ve web araması satırlarını `summary.tsv`'ye yazar), `prompt.txt` ve `partnote.txt` (aşağıdaki iki alıntı bloğunun metni). `build_inputs.py` 06'nın betiğinden (`CROSS_REVIEW_REPORTS/06_CROSS_REVIEW.md` §1) yalnız parça listesiyle ayrılır. Şablon: `git show e74883f:Docs/07_API_DESIGN.md`. SHA-256 (ilk 16 hane): istem `76e57ac0e4326f90`, parça notu `2b150c8107b170bb`, şablon `dfc8aa759c1e01c5`, doküman (v0.17) `ef8472f2a2a01b13`. **2. tur aynı istemle, aynı parça notuyla ve aynı betiklerle koşar;** yalnız doküman güncellenir. Scratchpad kaybolursa betikler bu bölümden yeniden kurulur: istem ve parça notu aşağıdadır, sınırlar parça tablosundadır; girdi `istem + [parça notu +] "=== DOKÜMAN ===" + metin + "=== ŞABLON ===" + şablon` biçimindedir.

**İstem.** Veri Modeli turunun istemi (`06_CROSS_REVIEW.md` §1) API tasarım dokümanına uyarlandı: yedi kriter, bulgu biçimi (`BULGU-N` — Kriter · Seviye · Yer · Alıntı · Sorun · Öneri — ya da `SONUÇ: TEMİZ`), `cross-review` skill'inin Faz 1 madde 4'teki bilinçli karar cümlesi değiştirilmeden, ciddiyet ölçüsünün paragrafı ve skill'in "mekanizma kararı koyan dokümanda okuma paragrafı"nın API tasarımına uyarlanışı: yazılan bir ucun, yetki hücresinin, hata kodunun, tekrar gönderim ya da sürüm mekanizmasının dokümanın kendi koyduğu bir güvenceyi sağlamaması (4)'tür; siparişin, paranın ya da kişisel verinin kaybına veya yetkisiz kişiye açılmasına yol açması (1) ya da (2)'dir; ek koruma katmanı, izleme ve daha güçlü araç önerisi savunmada derinliktir. Bağlam paragrafı dokümanın ürün kuralı koymadığını (yazım konvansiyonu 1), kuralın verilmeyen dokümanlarda yaşadığını, §1'in izlenebilirlik matrisi olduğunu — matris satırının özetinin verilmeyen kaynağa uygunluğu denetlenmez, matrisle dokümanın öteki bölümleri arasındaki çelişki ise dokümanın kendi içindedir — ve teknolojinin seçilmiş olduğunu (Razor Pages, ayrı JSON API yok) söyler.

> ```text
> Sen bağımsız bir doküman denetçisisin. Aşağıda "=== DOKÜMAN ===" işaretinden sonra bir API tasarım (API design) dokümanının metni, "=== ŞABLON ===" işaretinden sonra o dokümanın boş şablonu var. Başka bir dosya okuma; çalışma klasörü boştur.
>
> Bağlam: Belge bir API tasarım dokümanıdır. Ürün kuralı koymaz (başlık notu, yazım konvansiyonu 1): ayrı bir Arayüz Tanımları dokümanının (`04`) ekranlarının ve ayrı bir Kullanıcı Akışları dokümanının (`03`) adımlarının her isteğini bir uca — adres, yöntem, işleyici — çevirir; uçların uyduğu kuralları §2'de sabitler; ayrı bir Teknik Mimari dokümanının (`05`) makine uçlarını ve uçlara düşen mekanizmasını uç tanımına çevirir. Ürün davranışı ayrı bir Ürün Gereksinimleri dokümanında (`02`), ekranın davranışı `04`'te, mekanizma `05`'te, alan ve değer adları ayrı bir Veri Modeli dokümanında (`06`), kapsam ayrı bir MVP Kapsamı dokümanında (`10`) durur; hiçbiri sana verilmedi. Bu doküman kendi tasarım kararlarını koyar — uç envanteri, adres yapısı, işleyici adları, sayfa ve form ucunun biçimi, kimlik doğrulama ve erişim reddi, sürümün formda taşınması, tek kullanımlık değerler ve tekrar gönderim, gövde tavanları, DTO envanteri, hata kodları. §1 izlenebilirlik matrisidir: satırları verilmeyen kaynak dokümanların (`04`, `03`, `05`) maddelerini özetler ve uca eşler; bir matris satırının özetinin verilmeyen kaynağa uygunluğunu denetleme — matrisle dokümanın öteki bölümleri arasındaki çelişki ise dokümanın kendi içindedir. Kural ayrıntısının burada yeniden yazılmamış olması ya da bir kurala yalnız bölüm numarası ve kimlikle (`02 §…`, `03` …, `04` …, `05 §…`, `06 §…`, `10` KP-, Z-, L-, P-, H-, B-, F-, S-, Ö-, E-, OB-, SK-) işaret edilmesi tek başına bulgu değildir; o dokümanların içeriğini denetleme. Teknoloji seçilmiştir: ASP.NET Core Razor Pages, sayfalar sunucuda üretilir, ayrı bir JSON API katmanı yoktur; uç envanteri sayfa ve form uçlarını (gönder–yönlendir–göster) ve az sayıdaki makine ucunu içerir; dışarıya açık bir API ve sürümlemesi yoktur. Dış servislerin istek, imza ve yanıt biçimi, sayfa modellerinin ve kodun kalıpları, doğrulama senaryoları sonraki dokümanların (`08`, `09`, `12`) işidir; burada beklenmez. Dokümandaki `K-123` gibi işaretler sana verilmeyen bir karar kaydına gider. Yazım konvansiyonları başlık notundadır ("Yazım konvansiyonları", 1–14). Proje Aşama 6'dadır (API tasarımı); kod henüz yazılmadı; veritabanı PostgreSQL 18, çatı EF Core. Ürün, Türkiye'de tüketiciye satış yapan tek firmalık bir e-ticaret + kurumsal site ürünüdür; her firma ürünü kendi kiraladığı tek bir sanal sunucuya kurar.
>
> Dokümanı yedi kriterle denetle: tutarlılık · eksiklik · belirsizlik · teknik doğruluk · edge case · güvenlik · kullanıcı deneyimi.
>
> Dokümanda açıkça bilinçli karar, elenen seçenek ya da kabul edilmiş risk olarak yazılmış bir seçimi, yalnız seçime katılmadığın için bulgu yapma. Böyle bir karar dokümanın kendi içinde çelişki, olgusal ya da hukuki hata doğuruyorsa yaz.
>
> Yalnız CİDDİ sorunları yaz: (1) yürürlükteki mevzuata aykırılık, (2) müşteriye ya da firmaya para kaybı veya hak kaybı doğuran kural boşluğu, (3) kullanıcının takılıp kaldığı, çıkışı olmayan bir akış, (4) dokümanın iki yerinin birbiriyle çelişmesi. Nadir kenar durumları, iyileştirme önerilerini, savunmada derinlik önerilerini, üslup ve ifade tercihlerini bulgu yapma. Böyle bir sorun yoksa SONUÇ: TEMİZ yaz.
>
> Teknik doğruluk ve güvenlik bu dört sınıfın içinde okunur: yazılan bir ucun, yetki hücresinin, hata kodunun, tekrar gönderim ya da sürüm mekanizmasının dokümanın kendi koyduğu bir güvenceyi (ör. bir işlemin tek kez uygulanması, bir ucun yalnız yetkili aktöre açık olması, sahteciliğe karşı korumanın her form ucunda bulunması, bir kodun tek bir hâle karşılık gelmesi) sağlamaması (4)'tür; bir ucun, yetki hücresinin ya da hata davranışının siparişin, paranın ya da kişisel verinin kaybına veya kişisel verinin yetkisiz kişiye açılmasına yol açması (1) ya da (2)'dir. Ek bir koruma katmanı, ek izleme, daha güçlü bir araç ya da algoritma önerisi savunmada derinliktir ve bulgu değildir.
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
> ```

Parçalı ve birleşik çağrıların istemine tek bir paragraf eklenir (bilinçli karar cümlesi ve ölçü değişmez; `<bölümler>` aşağıdaki tabloların adlarıdır):

> ```text
> Parça notu: Doküman bu çağrıda parça parça verilmektedir. Aşağıdaki metin dokümanın başlık notu (sürüm, park edilen girdiler, yazım durumu, yazım konvansiyonları, alt bölüm haritası) ile <bölümler> bölümlerinden oluşur; öteki bölümler bu çağrıda verilmedi. Verilmeyen bir bölüme yapılan atıf ve orada yaşayan içeriğin burada görünmemesi tek başına bulgu değildir; yalnız verilen metnin içindeki sorunları yaz.
> ```

**Komut** (Git Bash; çalışma klasörü boş): `codex exec -s read-only -C <boş klasör> --skip-git-repo-check --ephemeral --ignore-user-config --color never -m gpt-5.6-terra -c 'model_reasoning_effort="high"' -o <çıktı> - < <girdi>`.

**Parça sınırları — kayıt (K-1219).** Sınır satır numarasıyla değil **başlıkla** tanımlıdır ve sonraki turlarda değişmez (satır numaraları kayar). **Başlık notu** dokümanın ilk satırından `## 1. Traceability Matrix` başlığına kadardır (sürüm, beş park bloğu ve dizinleri, yazım durumu, on dört yazım konvansiyonu, alt bölüm haritası; v0.17'de satır 1–106, 37 109 karakter) ve her parçanın başındadır. Her parça bir başlıkta başlar ve sonraki parçanın başlığından önce biter; betik kod çiti içindeki satırları başlık saymaz. Satır numaraları v0.17'nindir.

| Parça | Başlar | Biter (dahil değil) | Bölümler | Satırlar | Girdi | Token | Süre | Sonuç |
|---|---|---|---|---|---|---|---|---|
| 1 | `## 1. Traceability Matrix` | `#### 1.1.3 ` | §1'in girişi · §1.1.1–§1.1.2 (kaynak envanteri ve sayım, aday uç listesi) | 107–634 | 132 810 bayt | 54 413 | 132 sn | 2 bulgu |
| 2 | `#### 1.1.3 ` | `#### 1.1.6 ` | §1.1.3–§1.1.5 (Arayüz Tanımları: vitrin ekranları, navigasyon, ortak bileşenler) | 635–1238 | 149 418 bayt | 66 879 | 74 sn | `SONUÇ: TEMİZ` |
| 3 | `#### 1.1.6 ` | `#### 1.1.9 ` | §1.1.6–§1.1.8 (Arayüz Tanımları: panel ekranları, durum × rol, formlar) | 1239–2292 | 235 689 bayt | 99 249 | 111 sn | `SONUÇ: TEMİZ` |
| 4 | `#### 1.1.9 ` | `## 2. Konvansiyonlar` | §1.1.9–§1.3 (Kullanıcı Akışları, devirler, Teknik Mimari, GAP, geri izlenebilirlik, boşluklar) | 2293–3608 | 323 987 bayt | 129 337 | 72 sn | `SONUÇ: TEMİZ` |
| 5 | `## 2. Konvansiyonlar` | `## 4. Endpoint tanımları` | §2 · §3 | 3609–4115 | 156 066 bayt | 69 608 | 110 sn | `SONUÇ: TEMİZ` |
| 6 | `## 4. Endpoint tanımları` | `## 5. DTO envanteri` | §4 (vitrin ve makine uçları) | 4116–4911 | 185 452 bayt | 80 786 | 36 sn | 1 bulgu |
| 7 | `## 5. DTO envanteri` | `## 9. Yönetim (admin) endpoint` | §5 · §6 · §7 · §8 | 4912–5407 | 127 286 bayt | 52 993 | 159 sn | 1 bulgu |
| 8 | `## 9. Yönetim (admin) endpoint` | dosya sonu | §9 · §10 | 5408–6397 | 228 059 bayt | 86 511 | 131 sn | 1 bulgu |

Parça girdileri 127–324 KB'tır; kapsanmayan satır yoktur (betiğin denetimi).

**(a) Tek parça çağrı — girdi sınırı ve yerine koşan birleşimler (K-1219).** Tek parça girdi (1 194 401 bayt) dört saniyede `input_too_large` ile döndü: *"Input exceeds the maximum length of 1048576 characters … actual_chars 1091742"*. Hata modelin bağlam sınırı değil, aracın girdi uzunluğu sınırıdır; doküman şablonsuz da sınırın üstündedir. `cross-review` skill'inin Faz 1 madde 9'u izlendi: (a)'nın yerine sınıra sığan **en büyük birleşimler** koştu — en büyük birleşim sekiz parçanın yedisidir; ikisi birlikte her parça çiftini en az bir çağrıda buluşturur, yalnız matrisin parça 1'i ile parça 2'si (matrisle matris) buluşmaz:

| Çağrı | Parçalar | Bölümler | Girdi | Karakter | Token | Süre | Sonuç |
|---|---|---|---|---|---|---|---|
| Birleşim 1 | 2 · 3 · 4 · 5 · 6 · 7 · 8 | §1.1.3–§10 (verilmeyen yalnız §1'in girişi, §1.1.1 ve §1.1.2) | 1 111 236 bayt | 1 014 645 | 413 359 | 173 sn | 2 bulgu |
| Birleşim 2 | 1 · 3 · 4 · 5 · 6 · 7 · 8 | §1'in girişi, §1.1.1, §1.1.2 ve §1.1.6–§10 (verilmeyen yalnız §1.1.3–§1.1.5) | 1 094 664 bayt | 1 001 006 | 409 692 | 173 sn | 3 bulgu |

İki birleşim parça notunu taşır (verilmeyen bölüm adıyla). Birleşim 2'nin BULGU-1'i (indirme hakkı) hiçbir parçanın ve Birleşim 1'in tek başına getirmediği bir iddiadır; ama RET'tir (§3) — birleşim yolu bu turda kabul edilen bir bulgu getirmedi, parçalı koşum dört getirdi (F4, F5, F6-ret, F7-kısmi); ikisi (F1, F2) iki koşumdan birden geldi.

**Kapsam sınırı.** Parça öteki bölümleri görmez; birleşimler parçalar arası okumayı (a)'nın yerine yapar. Yönetici değerlendirmesi her iddiayı dokümanın tamamına ve ilgili yerde Ürün Gereksinimleri'ne (`02` v0.92), Kullanıcı Akışları'na (`03` v0.39), Arayüz Tanımları'na (`04` v0.39), Teknik Mimari'ye (`05` v0.28) ve Veri Modeli'ne (`06` v0.30) karşı okudu (§3). **Web araması:** `--ignore-user-config` aramayı kapatmıyor; on çağrının hiçbiri web araması yapmadı (koşum kayıtlarında `web search` satırı yok); hiçbir bulgu yasal dayanak anmadı — Faz 2 madde 5 (hukuki kontrol) konusuz. **Toplam:** on çağrı, 1 462 827 token; ilk çağrı girdi sınırında düştü.

## 2. Ham bulgular

İkinci modelin çıktıları, satır sonu boşlukları dışında düzenlenmeden (alıntı blokları içinde).

**(a) Tek parça çağrı:** koşmadı — girdi sınırı (§1). Yerine:

**Birleşim 1** (parça 2–8):
> BULGU-1
> Kriter: güvenlik
> Seviye: Yüksek
> Yer: §9.25.4 UC-187 / §4.28.2
> Alıntı: "Yan etkiler: bağlantı tüketilir; hesabın e-postası bağlantının eski adresine döner (`PreviousEmail`; `06 §4.42`)"
> Sorun: Yönetici e-posta geri alımında, eski adresin bu sırada başka doğrulanmış yönetici hesabına geçmiş olmasına ilişkin denetim yoktur. Buna karşılık müşteri eşleniği UC-76 bu hâlde geri almayı uygulamaz ve bağlantı tüketimini geri alır. §9.25, E-28 kalıbını devraldığını söylerken UC-187 bu korumayı atlar. Bu, yönetici e-posta tekilliğinin ihlaline, beklenmeyen hatayla geri alma akışının çıkışsız kalmasına veya hatalı bir uygulamada başka yönetici hesabının e-postasıyla çakışmaya yol açar.
> Öneri: UC-187’ye UC-76 ile aynı `EmailTakenByOtherAccount` 422 yolunu ekleyin: eski adres başka doğrulanmış yönetici hesabındaysa e-posta değişmesin, bağlantı tüketilmesin ve açıkça geri alma yapılamadığı hâl gösterilsin.
>
> BULGU-2
> Kriter: tutarlılık
> Seviye: Orta
> Yer: Başlık notu / doküman sonu
> Alıntı: "**Versiyon: v0.17**" / "*Shopfolio — API Design v0.16*"
> Sorun: Aynı doküman iki farklı sürüm kimliği taşıyor. Bu, hangi sürümün denetlendiği, uygulanacağı ve sonraki dokümanların hangi sürüme bağlanacağı konusunda çelişki yaratır.
> Öneri: Alt bilgiyi v0.17 olarak güncelleyin ya da başlık notunu doğru sürümle hizalayın.
>
> SONUÇ: 2 BULGU

**Birleşim 2** (parça 1, 3–8):
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §4.16.3 / §4.31.1
> Alıntı: "İndirme hakkı tek bir koşullu güncellemeyle düşer"
> Sorun: Hak, dosya aktarımı başlamadan UC-36’da düşürülüyor; UC-36’dan sonra gelen UC-37 isteği hiç ulaşmayabilir veya dosya 500 ile verilemeyebilir. Buna rağmen §4.31.1, hakkın “düğmeye basılıp gönderim başlayınca” düşeceğini söyler. Kullanıcı dosyayı alamadan indirme hakkını kaybedebilir (para/hak kaybı ve doküman içi çelişki).
> Öneri: Hakkı UC-36’da değil, UC-37’nin ilk başarılı dosya aktarımını kabul ederken atomik olarak düşürün; aynı imzalı değerin Range isteklerini bu tüketimi tekrarlamayacak şekilde bağlayın.
>
> BULGU-2
> Kriter: Güvenlik
> Seviye: Orta
> Yer: §9.25.4 / §9.21.3
> Alıntı: "hesabın e-postası bağlantının eski adresine döner (`PreviousEmail`; `06 §4.42`)"
> Sorun: Yönetici e-posta geri alma işleminde eski adresin bu arada başka bir yönetici hesabına atanmış olması ele alınmamış. Oysa yeni yönetici e-postası için başka yönetici hesabındaki adresin reddedildiği açıkça yazılıdır. Benzersizlik ihlali geri alma işleminde 500’e veya belirsiz sonuca düşebilir; ele geçirilmiş yönetici hesabının geri alınması akışı güvenilir biçimde tamamlanamaz. Bu, hesap erişimi ve güvenlik kurtarma akışında ciddi boşluktur.
> Öneri: UC-187’de eski adresin başka yönetici hesabına ait olup olmadığını, bağlantıyı tüketmeden denetleyin; çakışmada hiçbir veriyi değiştirmeden tanımlı bir 422 sonucu ve alternatif kurtarma yolunu verin.
>
> BULGU-3
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: Başlık notu / doküman alt bilgisi
> Alıntı: "**Versiyon: v0.17**" / "*Shopfolio — API Design v0.16*"
> Sorun: Dokümanın geçerli sürümü iki farklı değerle belirtilmiş. Bu, denetlenen kararların, değişikliklerin ve bağımlı dokümanların hangi sürüme bağlandığını belirsizleştirir; dokümanın kendi içinde çelişkidir.
> Öneri: Alt bilgiyi v0.17 olarak güncelleyin ve sürüm bilgisinin tüm dokümanda tekil olmasını doğrulayın.
>
> SONUÇ: 3 BULGU

**(b) Parçalı kontrol koşumu:**

**Parça 1** (§1'in girişi, §1.1.1–§1.1.2):
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §1.1.1 — kaynak envanteri, betik komutları ve devir ailesi düzeltmesi
> Alıntı: “**Genel toplam** | **2.464** | **2.636**”
> Sorun: İzlenebilirlik matrisinin sayım kanıtları birbirini tutmuyor: bileşen toplamları 1.615 + 827 + 90 + 103 = 2.635 ederken genel toplam 2.636 yazılmıştır; betik açıklaması ise 2.634, `e1c()` için 601 ve `e1f()` için 192 beklemektedir. Aynı bölüm Teknik Mimari ailesini hem 103 hem de `e1f()` içinde 102 sayacak şekilde sunar. Bu, §1’in “§3’e geçmeden önce” tamamlanması gereken kapsama/izlenebilirlik güvencesinin kendi içindeki çelişkisidir (sınıf 4).
> Öneri: Kaynak birimlerini ve matris satırlarını tek güncel betikle yeniden sayın; §1.1.1 tabloları, ara toplamlar, betik yorumları ve `e1f()` düzeltme notunu aynı sonuçlara göre birlikte güncelleyin.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §1.1.1 — §1.2 geri izlenebilirlik denetimi
> Alıntı: “geri07 | wc -l; uc07 | grep -cE '^UC-[0-9]+$'                            # 5071 · 5071”
> Sorun: Dokümanın güncel durumu §1.2 için “5.073 çift” olduğunu üç ayrı yerde söylerken, aynı güncel betik denetim satırı 5.071 çift beklediğini belirtir. Bu nedenle 187 adayın tamamının güncel kaynak eşleşmesiyle beslendiği iddiası doğrulanabilir değildir; matrisin denetim çıktısı ile beyan edilen sonuç çelişmektedir (sınıf 4).
> Öneri: Deep review’da eklenen iki çiftin hangi kaynak–UC eşleşmeleri olduğunu betik ve §1.2 sayımına dahil edin; ardından 5.073 sonucu tek kanonik değer olarak tüm ilgili satırlarda doğrulayın.
>
> SONUÇ: 2 BULGU

**Parça 2** (§1.1.3–§1.1.5):
> SONUÇ: TEMİZ

**Parça 3** (§1.1.6–§1.1.8):
> SONUÇ: TEMİZ

**Parça 4** (§1.1.9–§1.3):
> SONUÇ: TEMİZ

**Parça 5** (§2 · §3):
> SONUÇ: TEMİZ

**Parça 6** (§4):
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §4.25.7 UC-69; §4.23.4 UC-59; §4.28.2 UC-76
> Alıntı: “istek vitrinin oturum çerezini (`__Host-session`) taşıyor ve çerezin gösterdiği oturum kaydı yoksa sonuç hâli gösterilir”
> Sorun: UC-69, oturum kaydının yokluğunu hesap silmenin tek kanıtı sayıyor. Ancak UC-59 ve UC-76 da hesabı silmeden “hesabın bütün oturumları silinir” der. Bu işlemlerden sonra elde kalan herhangi bir oturum çereziyle `/hesap/silindi` açılması, kullanıcıya hesabının silindiğini ve silinenleri yanlış biçimde bildirir. Bu, hesabını sildiğini sanan kullanıcının hesabı ve kişisel verileri hâlâ dururken işlem yapmayı bırakmasına yol açabilir; doküman içi çelişkidir ve hak kaybı riski doğurur.
> Öneri: UC-69’u, yalnız UC-68’in başarılı silme işleminde üretilen ayrı, imzalı ve tek kullanımlık bir silme-sonucu işaretine bağlayın. Genel olarak geçersizleşmiş oturum çerezleri bu ekranı açmamalı; girişe ya da ana sayfaya yönlenmelidir.
>
> SONUÇ: 1 BULGU

**Parça 7** (§5–§8):
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §8.1.4; §5.2.1 `CancelOrderItemsInput` / `SubmitWithdrawalInput`; §6.3.3
> Alıntı: “gönderilen form türetilmiş değeri taşımaz, taşırsa okunmaz ve sunucu yeniden hesaplar (§2.4; K-1172 (3)).”
> Sorun: Bu hüküm, UC-40 ve UC-43 modellerinin taşıdığı `OriginalOpenQuantity` ile çelişir. Bu değer özellikle ekranda gösterilen açık adet sonradan değiştiğinde işlemin bütünüyle reddedilmesi için tanımlanmıştır (§6.3.3); okunmaması, eski ekrandan gelen iptal veya cayma isteğinin bu güvence uygulanmadan işlenmesine yol açar. Dokümanın kendi tekrar-gönderim ve güncel değer güvencesiyle çelişkidir (4).
> Öneri: §8.1.4’e açık istisna ekleyin: UC-40 ve UC-43’te `OriginalOpenQuantity` formda taşınır, sunucu bunu güncel açık adetle karşılaştırır; değer miktar hesabında güvenilir kaynak olarak kullanılmaz. Fark varsa işlem bütünüyle uygulanmaz ve `QuantityExceedsOpen` ile 303 döner.
>
> SONUÇ: 1 BULGU

**Parça 8** (§9 · §10):
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: Başlık notu / doküman sonu
> Alıntı: "**Versiyon: v0.17**" / "*Shopfolio — API Design v0.16*"
> Sorun: Dokümanın iki yerinde sürüm farklıdır; bu, hangi revizyonun denetlendiği ve değişikliklerin hangi sürüme ait olduğu konusunda çelişki yaratır (4).
> Öneri: Alt bilgiyi v0.17 olarak güncelleyin veya başlıktaki sürümü gerçek revizyona çekin.
>
> SONUÇ: 1 BULGU

## 3. Bağımsız değerlendirme

Her bulgu dokümanın tamamına (v0.17) ve ilgili yerde `02` v0.92, `03` v0.39, `04` v0.39, `05` v0.28, `06` v0.30'a karşı okundu — ikinci model bunları görmedi, parçalar dokümanın öteki bölümlerini de görmedi. Birleşim 1'in BULGU-1'i ile Birleşim 2'nin BULGU-2'si aynı yer ve aynı iddiadır (F1); sürüm alt bilgisi bulgusu üç çağrıdan gelir (F2). **Koşum (K-1207):** her bulgu önce bir Sonnet / low doğrulayıcıyla güncel metne karşı okundu; yüksek ya da orta önemli iddialar ve "çürütüldü" denenler ikinci, ayrı bir Sonnet / low doğrulayıcıdan geçti (F1, F3, F6, F7); ikisi ayrışırsa son hüküm yönetici ajandadır (F6). F2 betikle doğrulandı (`grep`).

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| F1 · Birleşim 1 · BULGU-1 = Birleşim 2 · BULGU-2 | ✅ KABUL (Orta — (4), kısmen (3)) | **UC-187 müşteri eşleniğinin çitini atlıyordu.** Yönetici e-postasını değiştirince eski adres Z-45 boyunca kimsenin değildir; başka bir yönetici o arada davetle o adrese hesap açabilir (`02 §10.2.6` yalnız bir yönetici hesabına *zaten ait* adrese davete yasak koyar). Geri alma `02 §3.13.14` ve `05 §6.4.6`'da hesap türünü ayırmadan "geri alma uygulanmaz, tüketim geri döner" der (K-1210); UC-76 bunu 422 olarak yazar, §6'nın `EmailTakenByOtherAccount` satırı "yöneticide yönetici hesabı" der, ama UC-187 "Yanıt (hata)"da yalnız 200 taşıyor, "Yan etkiler" doğrudan eski adresi geri yazıyordu. Tekil dizin çakışmayı reddeder; UC-187 bunu tanımlamadığı için beklenmeyen hata, `§6.4.2` gereği 303 ve aynı geri alma adımı — çıkışsız döngü. İki doğrulayıcı da doğruladı. Üst dokümanlar (`02`, `03` 9.3.7, `04` 9.25.5 "aynen", `05`) kuralı zaten taşıyor — yalnız `07` eksikti. | UC-187: "Yanıt (hata)" başına 422 kolu, "Yan etkiler" başına "422 hâlinde hiçbir şey yazılmaz", Kaynak satırına K-1210 ve K-1218; §6 `EmailTakenByOtherAccount` kullanım sütununa UC-187 |
| F2 · Birleşim 1 · BULGU-2 = Birleşim 2 · BULGU-3 = Parça 8 · BULGU-1 | ✅ KABUL (Düşük — (4)) | **Alt bilgi sürümü eskimişti.** Başlık v0.17, dosya sonu dipnotu v0.16 diyordu (`grep`; deep review dipnotu yükseltmeyi atlamış). | Başlık v0.18, dipnot v0.18 |
| F3 · Birleşim 2 · BULGU-1 | ❌ RET | **Çelişki yok; bilinçli karar, kurtarma yolu yazılı.** Hak UC-36'nın düğmesinde (bağlantı üretilirken, tek koşullu güncellemeyle) düşer; UC-37 yeni hak düşürmez ve §4.31.1 bunu aynen söyler: *"yoktur — hak UC-36'nın düğmesinde düşmüştür (4.16.3; K-884)"*. İddianın alıntıladığı "düğmeye basılıp gönderim başlayınca" ifadesi §4.31.1'de değil, K-884'te ve `05 §7.7.4`'te geçer. Dosya depoda okunamazsa 500 ve düşen hak firmanın hak yenilemesiyle geri verilir (`02 §3.12.6`) — §4.31.1'de **kalan risk** diye adlandırılmıştır; bağlantı bir saat içinde yeniden kullanılabilir. Önerilen "UC-37'de düşür" bağlantıyı kayıtsız ve imzalı tutan mimariyle (aralık istekleri hak düşürmez) uyuşmaz. İki doğrulayıcı da çürüttü. | — |
| F4 · Parça 1 · BULGU-1 | ✅ KABUL (Düşük — (4)) | **Matrisin `04` toplamı bir eksik yazılmıştı.** Betik bloğunu doğrulayıcı çalıştırdı: 1616 + 827 + 90 + 103 = 2.636 — "Genel toplam 2.636" doğru; hatalı olan "`04` toplamı" satırı (1.615, fark +147; doğrusu 1.616, +148 — 583 + 431 + 602 = 1.616; `04` satırlarının farkları +148). İddianın 2.635'i yanlış toplamdan çıkmıştı. Teknik Mimari "103 / 102" farkı ve 601 / 192 sayıları bayat betik yorumlarındaydı (2026-10-18 sayımı); güncel çıktı 103, 602, 193. | §1.1.1: `04` toplamı 1.616 / +148; betik yorumları `1616 · 827 · 90 · 103 — toplam 2636`, `583 · 431 · 602 · 536 · 291 · 193`; `exp04` dağılım yorumunda §6 numaralı 356 → 357 (toplam 1.616) |
| F5 · Parça 1 · BULGU-2 | ✅ KABUL (Düşük — (4)) | **Geri izlenebilirlik betiğinin yorum sayıları audit öncesinde kalmıştı.** Betik `geri07 \| wc -l` için 5073 verir; yorum 5071 diyordu. Gövde (satır 129, 404, 3371) 5.073'ü söylüyor (5.051 → 5.056 → 5.071 → 5.073 zinciri yazılı). Aynı bayatlık iki komşu yorumda: satır sayısı 2318 → 2319 ve aile dağılımı `03` 1579 / `04` 2961 → 1580 / 2962. Beyan edilen sonuç doğru; denetim satırı eskiydi. | §1.1.1 betik yorumları `5073 · 5073`, `2319 · 187`, `03 1580 · 04 2962 · 05 246 · devir 285` |
| F6 · Parça 6 · BULGU-1 | ❌ RET (yönetici hükmü — doğrulayıcılar ayrıştı) | **Bilinçli karar; kenar durum ölçünün sınıfına girmez.** İlk doğrulayıcı Düşük / (4) ile doğruladı, ikinci doğrulayıcı çürüttü; hüküm yöneticide. UC-69 oturum kaydının yokluğunu işaret sayar — UC-68 çerezi bilerek silmez, işaret olarak bırakır (K-1184; `07` 3946 ve 4112'de bilinçli tasarım olarak yazılı). Bayat çerezle (başka tarayıcıda şifre sıfırlandı / geri alma yapıldı) adresi elle yazan ya da geçmişten açan kullanıcı sonuç hâlini bir kez görür; UC-69 salt okunur bir GET'tir, hesaba ve veriye dokunmaz, silme için yeniden doğrulama ve onay gerekir — "hesabını sildiğini sanıp işlem bırakma" senaryosu oluşmaz; sonraki işlemler oturumsuz sayılıp girişe yönlenir. Sayfaya normal akışla UC-68'in 303'ünden başka yoldan ulaşılmaz. Ölçünün (1)–(3)'ü yok; (4) için çelişen iki yer yok — UC-59 / UC-76 ve UC-69 aynı mekanizmanın sonuçları. Nadir kenar durumdur. Önerilen imzalı tek kullanımlık işaret `05 §6.7.1`'in çerez listesini değiştiren bir tasarımdır (savunmada derinlik). | — |
| F7 · Parça 7 · BULGU-1 | ⚠️ KISMİ (Düşük — (4)) | **Çelişki yok, cümle kabaydı; sorun gerçek ama düşük.** İki doğrulayıcı da çürüttü: §2.4.3 `Original` önekli gösterilen değeri ayrı bir sınıf olarak tanımlar, `OriginalOpenQuantity` açıkça sayılmıştır (K-1202); formdan gelen okunur, ama karar verilirken kullanılmaz — yalnız güncel değerle karşılaştırılır, farklıysa işlem bütünüyle uygulanmaz, 303 `QuantityExceedsOpen` (§6.3.3, UC-40, UC-43, §5.2.1). Birlikte okunan §2.4.3 ile §8.1.4 aynı metinde uzlaşıktır. Ama §8.1.4'ün listesi "kalemin o işleme açık adedi"ni türetilmiş değer sayar ve son cümle "taşırsa okunmaz" der; cümleyi tek başına okuyan bir uygulayıcı güvenceyi bozabilir. Önerilen çözüm (§8.1.4'e açık istisna) kabul, önerilen gerekçe (çelişki) reddedildi. | §8.1.4 son cümlesine `Original` önekli gösterilen değer alanları için istisna (§2.4.3; K-1202) |

**Dağılım:** yedi ayrı bulgu (on ham bulgu; F1 ve F2 çoklu) — 4 KABUL · 1 KISMİ · 2 RET. **%100 kabul değil, %100 ret değil:** iki RET'in biri bilinçli kararla ve yazılı kalan riskle (F3 — K-884), biri bilinçli kararla ve ölçünün dışında kalan kenar durumla (F6 — K-1184) düştü; KISMİ'de modelin gerekçesi reddedildi, cümle netleştirildi. **Sınıf:** kabul edilen dört bulgudan biri orta (F1 — kural var, `07` eksikti, çıkışsız geri alma döngüsü), üçü düşük şiddette metin farkıdır (F2, F4, F5); F1 yeni bir yüzey açmadı — UC-76'nın çitini UC-187'ye taşıdı. Faz 2 hukuki kontrolü: hiçbir bulgu yasal dayanak anmadı.

## 4. Ek bulgular

- **Aynı kalıbın taraması — sürüm alt bilgisi (F2).** `grep v0.16` / `v0.17`: kalan yerler tarihsel anlatımlardır (audit ve deep review paragrafları, §1.2'nin "Audit'in hücre düzeltmeleri"); güncel sürüm iddiası başlık ve dipnottur — ikisi v0.18.
- **Aynı kalıbın taraması — betik yorum sayıları (F4, F5).** §1.1.1'in betik bloğunun bütün `#` yorumları dokümanın betiğiyle yeniden koşuldu (doğrulayıcı): kalan bayatlık yalnız yukarıda düzeltilen yorumlardı; tablo satırı 1a (583) · 1b (431) · 1c (602) ve 1f (193) doğruydu.
- **Aynı kalıbın taraması — yönetici geri almanın eşleri (F1).** `EmailTakenByOtherAccount` kodunun üç öteki kullanımı (UC-53, UC-186, UC-76) 422 yolunu taşıyor; UC-187 yalnız eksik olandı. `§3.2`, `§1.1`, `§1.2`, `§5.2.4` ve `LinkNoLongerValid` satırı UC-187'nin hata yollarını ayrı saymıyor — özet yeri değişmedi.
- **Mekanik tarama.** Düzeltmeler gövdeye yeni K numarası olarak K-1218'i (UC-187 Kaynak satırı, başlık notu) ve K-1210'u (UC-187 Kaynak satırı) ekledi; K-1210 `07`'de zaten vardı (§1.3, UC-76); matris satırı eklenmedi ya da çıkmadı — kümeler ve çiftler (5.073 · 2.319 · 187) değişmez; hata kodu sayıları (§6.2 112, §6.3 22) değişmez, `EmailTakenByOtherAccount` zaten tanımlı; §3.2 uç sayısı 187 aynı. Başlık sürümü ve dosya sonu dipnotu v0.18'i gösterir. Değişen dosyalarda CR ve kontrol karakteri sıfır; karar kaydında çift numara yok.
- **Devredilen not (F3 — checkpoint).** Teknik Mimari'nin `05 §7.7.4`'ü ve K-884 hakkın "düğmeye basıldığında **ve** sunucu dosyayı göndermeye başladığında" düştüğünü söyler; mekanizma paragrafı (ve `07` §4.16.3, §4.31.1) tek adımı — UC-36'nın düğmesi — anlatır. Kelimesi kelimesine örtüşmeyen ifade düşük şiddetlidir ve `07`'nin bulgusu değildir; `05` cümlesine "bağlantı üretildiği an" eklemek istenirse Aşama 6'nın checkpoint'inde yapılır. Önceki turlardan açık kalan yok (1. tur); bu not bir sonraki raporda "açık notlar" listesinde taşınır.
- **Ciddi bir sorun daha görülmedi.** Değerlendirme sırasında ikinci modelin listesi dışında ölçünün dört sınıfından birine giren bir sorun saptanmadı.
- **Deep review'ın açık gözlemleri (`AUDIT_REPORTS/07_DEEP_REVIEW.md`, "Cross-review için notlar"):** L12-8'in "422 ya da beklenmeyen hata yolu" ifadesi `12`'de senaryoyla sabitlenecek — bu turun hiçbir bulgusu onu yeniden açmadı; ikinci model silinmiş kategori seçimi hakkında bulgu getirmedi, madde `12`'nin yazımına devredilmiştir. Öteki üç gözlem (apply log'ları, ⚠ satırlarının tek mesajda gösterilmesi, IT-6'nın matris hücreleri) bu turun konusu değildir.

## 5. Kullanıcı onay checklist'i

K-1144 kararıyla (API tasarımı aşaması boyunca ⚠ öneriyle kayıt, CI yeşilse `docs:` PR merge yetkisi ve yönetici + alt ajan düzeni) alt ajan değerlendirdi ve uyguladı (`07` v0.18); K-1207 gereği alt ajanlar yalnız Sonnet / low koştu. İki karar öneriyle kaydedildi, ikisi de ⚠ değildir: **K-1218** — UC-187'nin eski adres çiti (UC-76'nın simetrisi; K-1210'un yöneticiye uygulanışı); **K-1219** — cross-review'ın ölçü ve koşum kararı (ölçü 1. turdan, okuma paragrafıyla; tek parça çağrı girdi sınırını aşar, yerine en büyük iki birleşim ve sekiz parçalı kontrol koşumunun başlıkla sabitlenen sınırları). ⚠ listesine ekleme yok.

- [x] F1 (kabul — UC-187'ye 422 kolu; K-1218)
- [x] F2 (kabul — alt bilgi sürümü v0.18)
- [x] F3 (ret — K-884 bilinçli karar, kurtarma yolu ve kalan risk yazılı)
- [x] F4 (kabul — matrisin `04` toplamı 1.616 / +148)
- [x] F5 (kabul — geri izlenebilirlik betik yorumları 5073 · 2319 · 1580 / 2962)
- [x] F6 (ret — K-1184 bilinçli karar; salt okunur nadir kenar durum)
- [x] F7 (kısmi — §8.1.4'e `Original` önekli alanlar için istisna)
- [x] 2. tur — koştu (aşağıda, "Tur 2" bölümü)
- [ ] Etki yansıtma (Faz 5) — döngü TEMİZ döndükten sonra, son turun raporunda (2. turun sonunda hâlâ açık)

---

# Cross-Review — 07 API Design (Tur 2)

**Tarih:** 2026-10-19 · **Hedef:** `Docs/07_API_DESIGN.md` v0.18 (girdi commit'i `95a6486`, 1 188 786 bayt, 6 399 satır) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Ciddiyet ölçüsü:** 1. turla aynı (K-1219) · **Koşum:** 1. turla aynı iki biçim — **iki birleşim** ve **sekiz parçalı kontrol koşumu**, aynı istem, aynı parça notu, aynı şablon (SHA-256 ilk 16 hane: istem `76e57ac0e4326f90`, parça notu `2b150c8107b170bb`, şablon `dfc8aa759c1e01c5` — 1. turla aynı; doküman v0.18 `8277b8538961f695`), aynı betikler, parça sınırları başlıkla (K-1219) · **Tek parça çağrı:** yeniden denenmedi — girdi 1 093 530 karakter, sınır 1 048 576 (1. turda aynı hatayla döndü; yönetici talimatı) · **Girdi:** yalnız doküman ve boş şablonu, her çağrıda boş ve izole çalışma klasöründe (K-431) · **Kullanım sınırı:** dolmadı — on çağrının hepsi çıktı verdi; yedeğe geçilmedi · **Sonuç:** TEMİZ değil — sekiz ham bulgu (sekizi ayrı), 0 KABUL · 1 KISMİ · 7 RET; kısmi olan uygulandı (`07` v0.19) · **Değerlendirme modeli:** Claude alt ajanları yalnız Sonnet / low (K-1207); Codex dışarıdadır

## 1. Koşum

**Betikler** 1. turdakilerdir (`C:\Users\TEKNOD~1\AppData\Local\Temp\claude\c--projects-Shopfolio\422e2f0a-46a6-4ced-809c-e90f0f447f55\scratchpad\cr07\`: `build_inputs.py`, `run_codex.sh`, `prompt.txt`, `partnote.txt`, `template07.md`); girdi klasörü bu tur için `..\cr07r2\in`, çıktı `..\cr07r2\out`. Komutlar: `python -I build_inputs.py Docs/07_API_DESIGN.md template07.md prompt.txt partnote.txt <girdi>` — kapsanmayan satır `none`; `bash run_codex.sh <girdi> <çıktı> c1 c2 p1 p2 p3 p4 p5 p6 p7 p8`. İstem ve parça notu 1. turun §1'indeki alıntı bloklarıdır, değiştirilmedi.

**Parçalar** (sınırlar 1. turdakiyle aynı başlıklar; satır numaraları v0.18'indir ve kaymıştır):

| Parça | Bölümler | Satırlar | Girdi | Token | Süre | Sonuç |
|---|---|---|---|---|---|---|
| 1 | §1'in girişi · §1.1.1–§1.1.2 | 109–636 | 134 093 bayt | 54 013 | 34 sn | 1 bulgu |
| 2 | §1.1.3–§1.1.5 | 637–1240 | 150 701 bayt | 67 106 | 63 sn | `SONUÇ: TEMİZ` |
| 3 | §1.1.6–§1.1.8 | 1241–2294 | 236 972 bayt | 87 977 | 19 sn | `SONUÇ: TEMİZ` |
| 4 | §1.1.9–§1.3 | 2295–3610 | 325 270 bayt | 130 541 | 28 sn | 1 bulgu |
| 5 | §2 · §3 | 3611–4117 | 157 349 bayt | 68 512 | 60 sn | `SONUÇ: TEMİZ` |
| 6 | §4 | 4118–4913 | 186 735 bayt | 71 864 | 44 sn | 2 bulgu |
| 7 | §5 · §6 · §7 · §8 | 4914–5409 | 128 859 bayt | 53 156 | 51 sn | 1 bulgu |
| 8 | §9 · §10 | 5410–6399 | 229 710 bayt | 88 128 | 165 sn | 2 bulgu |

| Çağrı | Parçalar | Girdi | Karakter | Token | Süre | Sonuç |
|---|---|---|---|---|---|---|
| Birleşim 1 | 2 · 3 · 4 · 5 · 6 · 7 · 8 | 1 113 177 bayt | 1 016 427 | 403 184 | 145 sn | 1 bulgu |
| Birleşim 2 | 1 · 3 · 4 · 5 · 6 · 7 · 8 | 1 096 605 bayt | 1 002 788 | 396 759 | 139 sn | `SONUÇ: TEMİZ` |

**Kapsam sınırı ve web.** Parça öteki bölümleri görmez; birleşimler parçalar arası okumayı yapar (yalnız matrisin parça 1'i ile parça 2'si buluşmaz — K-1219). Yönetici değerlendirmesi her iddiayı dokümanın tamamına ve ilgili yerde `02` v0.92, `03` v0.39, `04` v0.39, `05` v0.28, `06` v0.30'a karşı okudu. **Web araması:** on çağrının hiçbirinde `web search` satırı yok; hiçbir bulgu yasal dayanak anmadı — Faz 2 madde 5 (hukuki kontrol) konusuz. **Toplam:** on Codex çağrısı, 1 421 240 token.

## 2. Ham bulgular

İkinci modelin çıktıları, satır sonu boşlukları dışında düzenlenmeden (alıntı blokları içinde).

**Birleşim 1** (parça 2–8):
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §2.5.1 / §4.13.6 (UC-27)
> Alıntı: “aynı anahtarla gelen ama bu arada geçersizleşen gönderim 422/404 alır”
> Sorun: Bu, aynı onay anahtarının ikinci gönderimini daima mevcut siparişin teyit ekranına götüren §2.5.1 kuralıyla çelişir. Ağ hatasında kullanıcı ilk gönderimin sipariş oluşturup oluşturmadığını güvenilir biçimde öğrenemeyebilir; sipariş oluşmuş olsa bile değişen form girdisi nedeniyle 422/404 alabilir. Bu, siparişin tek kez uygulanması ve sonuç-belirsizliği güvencesinde iç çelişkidir (sınıf 4).
> Öneri: Onay anahtarının mevcut bir siparişe bağlı olup olmadığını, değişebilir form/sahiplik doğrulamalarından önce ve atomik biçimde kontrol edin; bağlıysa her durumda o siparişin uygun teyit ya da sonuç ekranına 303 dönün. Yalnız anahtar hiçbir siparişe bağlı değilse olağan 422/404 doğrulamalarını uygulayın.
>
> SONUÇ: 1 BULGU

**Birleşim 2** (parça 1, 3–8):
> SONUÇ: TEMİZ

**Parça 1** (§1'in girişi, §1.1.1–§1.1.2):
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §1.1.1 — “Devir ailesinin betiği düzeltildi”
> Alıntı: “Sonuç: `e1f()` 212 → 192 — `e1f_dev()` 110 → 90”
> Sorun: Bu, aynı bölümdeki sayımlarla çelişir: `e1f()` tanımı `e1f_dev()` ile `exp05()` çıktısını birleştirir; metin `exp05()`i 103 olarak verir. Dolayısıyla düzeltilmiş sonuç 90 + 103 = 193 olmalıdır. Nitekim betik beklenen çıktılarında da `e1f` 193, §1.1.12 ise 103 olarak yazılmıştır. Bu, izlenebilirlik kapsam denetiminin yeniden üretilebilir sayım kanıtını çelişkili kılar (sınıf 4).
> Öneri: “`e1f()` 212 → 192” ifadesini `193` olarak düzeltin; 192 amaçlanıyorsa, dışarıda bırakılan Teknik Mimari birimini ve ilgili tüm beklenen sayımları açıkça güncelleyin.
>
> SONUÇ: 1 BULGU

**Parça 2** (§1.1.3–§1.1.5):
> SONUÇ: TEMİZ

**Parça 3** (§1.1.6–§1.1.8):
> SONUÇ: TEMİZ

**Parça 4** (§1.1.9–§1.3):
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §1.1.10 — `03 §6.1.1.9`, `03 §7.1.49`, `03 §9.1.3`
> Alıntı: “önceki bağlantı geçerli kalır” / “öncekileri geçersiz kılar”
> Sorun: Aynı doğrulama bağlantısının yeniden istenmesinde eski bağlantının geçerliliği birbirine zıt tanımlanmış. Bu, bağlantının eski bir e-postadan kullanılabilmesi ve kullanıcıya farklı sonuçlar dönmesi bakımından dokümanın kendi içindeki çelişkidir (4).
> Öneri: Tek davranışı seçip ilgili tüm satırları hizalayın; bağlantı güvenliği açısından yeni doğrulama bağlantısının öncekini geçersiz kıldığı kuralını `03 §6.1.1.9` satırına da uygulayın.
>
> SONUÇ: 1 BULGU

**Parça 5** (§2 · §3):
> SONUÇ: TEMİZ

**Parça 6** (§4):
> BULGU-1
> Kriter: kullanıcı deneyimi
> Seviye: Yüksek
> Yer: §4.13.6 UC-27; §4.15.2 UC-32
> Alıntı: “**303 → sonucu gösteren ekran** — onay beklenmeyen bir sebeple sonuçsuz kaldı: üyede `/hesap/siparisler`, misafirde `/siparis-takibi`”
> Sorun: Misafir alıcı, sonuç belirsiz kaldığında sipariş numarası veya erişim anahtarı almadan sipariş takip girişine yönlenir. Oysa bu ekranın isteği “sipariş numarası ve e-posta”dır. Misafir, siparişin oluşup oluşmadığını öğrenemez; çıkışı olmayan akış oluşur (3).
> Öneri: Sonucu belirsiz misafir onayı için `ConfirmationKey` ile bağlı, tek kullanımlık ve yalnız işlem sonucunu gösteren bir durum/hedef akışı tanımlayın; sipariş oluşmuşsa erişim anahtarlı sipariş sayfasına, oluşmamışsa yeniden ödeme adımına güvenle yönlendirin.
>
> BULGU-2
> Kriter: tutarlılık
> Seviye: Orta
> Yer: §4 giriş ortak kurallar; §4.4.2 UC-05
> Alıntı: “çerez politikası ilk kez yayına alınana kadar vitrin çerez yazmaz”
> Sorun: Aynı doküman, çerez politikası henüz yayımlanmamışken vitrin çerezi yazmayı yasaklarken UC-05’te ilk misafir sepete eklemede `__Host-cart` çerezinin açılacağını söyler. Bu, aynı koşul için çelişkili davranış tanımlar (4).
> Öneri: Çerez politikası yayımlanana dek sepete eklemeyi engelleyin veya zorunlu sepet/oturum çerezlerinin bu kuralın açık istisnası olduğunu ve hangi koşulda yazılacağını belirtin.
>
> SONUÇ: 2 BULGU

**Parça 7** (§5–§8):
> BULGU-1
> Kriter: tutarlılık
> Seviye: Orta
> Yer: §6.2.2 — `InvalidDate`, `InvalidDateRange`
> Alıntı: "`InvalidDate` | 422 | ... — liste GET'inde 200"
> Sorun: Aynı hata kodu için HTTP sütunu 422 iken, UC-105 ve UC-106 liste GET’lerinde 200 döndüğü yazılmıştır. `InvalidDateRange` satırında da aynı çelişki vardır. Bu, aynı geçersiz girdi için endpoint davranışını belirsiz ve test edilemez kılar (4: doküman içi çelişki).
> Öneri: Her iki satırın HTTP hücresini `422 · 200` yapın; 200’ün yalnız liste GET’lerindeki alan mesajı durumu olduğunu açıkça belirtin.
>
> SONUÇ: 1 BULGU

**Parça 8** (§9 · §10):
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §9.9 ortak kural / UC-152
> Alıntı: “her form siparişin `Version`'ını taşır” / “Sürüm taşımaz.”
> Sorun: Aynı sipariş işlemleri için sürüm zorunluluğu iki yerde çelişiyor. UC-152’nin yalnız `Original` metin değerine göre çalışması amaçlanıyorsa, §9.9’daki tüm formlar için konan `Version` ve sürüm-409 güvencesinden açıkça istisna olduğu belirtilmelidir. Bu, eşzamanlı yazım güvence kuralının hangi işlemlere uygulandığını belirsiz bırakır.
> Öneri: §9.9 ortak kuralını iç notun açık istisna olduğunu söyleyecek şekilde düzeltin; UC-152’nin yalnız `Original` değer koşuluyla 409 döndüğünü belirtin.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §9.9.10 UC-131 — kart hattının sırası
> Alıntı: “geri ödeme kaydı istekten önce kendi işleminde kesinleşir” / “geri ödeme kaydı iş kuyruğunda, kısa bir gecikmeyle doğar”
> Sorun: Kart iadesi kaydının POST işlemi içinde mi, yoksa iş kuyruğunda sonradan mı oluşturulduğu çelişkili tanımlanmış. Bu kayıt, ikinci gönderimin engellenmesi, bekleyen durumun gösterilmesi ve yeniden deneme akışının dayanağı olduğundan para iadesi işleminin kalıcılığı ile idempotency uygulaması farklı yorumlanabilir.
> Öneri: POST’un atomik olarak yazdığı kayıtları ve iş kuyruğunun sonradan oluşturduğu/işlediği kayıtları farklı adlarla açıkça ayırın; geri ödeme kaydının kesin oluşturulma anını tek biçimde tanımlayın.
>
> SONUÇ: 2 BULGU

## 3. Bağımsız değerlendirme

Her bulgu dokümanın tamamına (v0.18) ve ilgili yerde `02`, `03`, `04`, `05`, `06`'ya karşı okundu. **Koşum (K-1207):** her bulguyu önce bir Sonnet / low doğrulayıcı okudu; sekizini de "çürütüldü" dedi — kural gereği (çürütülen bulgu ikinci bakışa gider) sekiz bulgunun hepsi ikinci, ayrı bir Sonnet / low doğrulayıcıdan da geçti (temiz bağlam; birincinin kararı verilmedi). İkisi ayrışan F2'de son hüküm yönetici ajandadır (git geçmişi ve satır 435 / 439 okundu). F6'nın hücreleri (satır 5151, 5152) ve F1'in cümlesi (satır 4361) yönetici ajanca da okundu. **1. turdan tekrar eden bulgu yok:** F3 (K-884) ve F6 (K-1184) bu turda dönmedi; 1. turun reddettiği iddialar yeniden gelmedi.

| # | Çağrı · bulgu | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|---|
| F1 | Birleşim 1 · BULGU-1 (§2.5.1 / §4.13.6 UC-27) | ❌ RET | **Bilinçli karar, yazılı kalan risk.** §4.13.6'nın sırası açık: oturum → alan, kutu ve adres sahipliği doğrulaması (işlemden önce; 422 / 404) → `05 §4.2.2`'nin işlemi; hemen ardından *"aynı anahtarla gelen ama bu arada geçersizleşen gönderim 422/404 alır — kalan risk (K-1206)"*. §2.5.1 (satır 3730) "daima" demiyor; §2.5.8 aynı kalıbı K-1206 altında kalan risk diye anar. Sipariş zaten oluşmuş, ikinci sipariş açılmıyor; kullanıcı formu görür, veri bozulmaz — ölçünün dört sınıfı yok. Önerilen sıra (anahtarı doğrulamadan önce kontrol) K-1181 (6)'nın kararını bozar. İki doğrulayıcı da çürüttü. | — |
| F2 | Parça 1 · BULGU-1 (§1.1.1 "Devir ailesinin betiği düzeltildi") | ⚠️ KISMİ (Düşük — (4)) | **Geçmiş anlatım, ama güncel metnin hemen yanında.** `e1f()` 212 → 192 cümlesi yazım turunun 4. oturumunun sayımıdır (v0.13, K-1197): o gün `exp05()` 102'ydi (`git show 7560bb4`: `exp05 \| wc -l # 1615 · 827 · 90 · 102`) ve 90 + 102 = 192 doğruydu; sonra 103'e çıktı ve güncel yorumlar (satır 350, 351, 435) 193 diyor — 1. turun F4'ü bunu düzeltmişti. İlk doğrulayıcı "geçmiş anlatım, çelişki yok" dedi; ikinci doğrulayıcı 435 ile 439'un aynı bölümde birbirini tutmadığını söyledi. Hüküm: beklenen küme doğru yazılı, ama tarihsiz bir "Sonuç" cümlesi yanında 192 okuyan bir okuyucu çelişki görür; önerilen "192 → 193" geçmişi yanlış aktarır ve reddedildi, sorun gerçek ama düşük. | §1.1.1 paragrafında cümleye *"(o günün sayımıdır: `exp05()` 102'ydi, sonra 103 oldu; güncel beklenen küme 193'tür — yukarıdaki 1f paragrafı)"*; başlık notundaki v0.13 oturum özetinde *"(o günün sayımı; güncel 193)"* |
| F3 | Parça 4 · BULGU-1 (§1.1.10 `03 §6.1.1.9` / `§7.1.49` / `§9.1.3`) | ❌ RET | **İki ayrı olay.** `03` 6.1.1.9 (satır 1100) limit (L-9) isteği **engellediğinde** konuşur: *"İstek engellenir; kayıt silinmez, önceki bağlantı ömrü içinde geçerli kalır"* — yeni bağlantı üretilmediği için eskisi kalır. `03 §7.1.49` / `§9.1.3` ve `07`'nin uç tanımları (UC-49, UC-67, yönetici karşılığı) başarılı yeni isteği anlatır: öncekiler `SupersededAt`. `03` 3.4.14 (satır 827) iki olayı yan yana tutar. Matris satırı özetinin kaynağa uygunluğu ölçünün dışındadır; matris satırı (2288, 2888) kısa yazılmış olsa da dokümanın gövdesi tek ve tutarlı. İki doğrulayıcı da çürüttü. | — |
| F4 | Parça 6 · BULGU-1 (§4.13.6 UC-27; §4.15.2 UC-32) | ❌ RET | **Üst dokümanda yazılı bilinçli karar, çıkış var.** `04` 2.7.3.3 (satır 2552): *"sipariş onayında numara henüz bilinmeyebileceği için üye sipariş geçmişine, misafir alıcı sipariş takibi girişine gider (5.13.31)"*; `04` 5.13.31 (K-745) aynı; `07` yalnız bunu aktarır. `/siparis-takibi` çıkış verir: sipariş oluşmuşsa e-postadaki erişim anahtarlı bağlantı sayfayı açar (`04` 5.15.3), numara ve e-posta UC-32'de siparişi bulur; oluşmamışsa nötr mesaj döner ve sepet ve yazılanlar durur. "Oluşup oluşmadığını söylememe" K-772'dir. Önerilen durum akışı yeni tasarımdır. İki doğrulayıcı da çürüttü. | — |
| F5 | Parça 6 · BULGU-2 (§4 ortak kurallar; §4.4.2 UC-05) | ❌ RET | **Çelişki yok.** Kural `02 §12.2.5` ve `05 §6.7.10`'dadır: çerez politikası ilk kez yayına alınana kadar satış ve veri toplayan girişler kapalıdır, zorunlu çerez gerektiren işlev yoktur. UC-05'in hata listesinde *"satış kapalı"* → 422 vardır ve *"Ekleme reddedilirse (422, 404) sepet ve çerez açılmaz"* (satır 4192); yayından önce `__Host-cart` açılamaz. Önerilen ikinci çözüm ("sepete eklemeyi engelle") zaten uygulanmıştır; zorunlu çerez istisnası `02 §12.2.5`'in bilinçli kararına aykırı olurdu. İki doğrulayıcı da çürüttü. | — |
| F6 | Parça 7 · BULGU-1 (§6.2.2 `InvalidDate`, `InvalidDateRange`) | ❌ RET | **İstisna hücrede yazılı.** Satır 5151: HTTP hücresi `422`, açıklama hücresi *"— liste GET'inde 200 (`04` 7.3.2.11; K-554)"*; satır 5152: *"liste GET'inde aynı kural alan mesajıdır ve durum 200'dür (§2.6.6)"*, kullanım sütunu *"UC-105, UC-106 — liste GET'inde, 200"*. İki yer çelişmiyor; yazım kalıbı farkı var (`SalesClosed` `422 · 303` biçimini ikinci uç ayrı bir yanıtta kullanır, `DeliveryProvinceNotServed` `InvalidDate` gibi açıklama hücresinde yazar) — ölçünün sınıfı değil. İki doğrulayıcı da çürüttü. | — |
| F7 | Parça 8 · BULGU-1 (§9.9 ortak kural / UC-152) | ❌ RET | **İstisna ortak kuralın cümlesinde.** Satır 5630: *"her form siparişin `Version`'ını taşır, **iç not gösterilen metni** (§2.4.2, §2.4.3)"*; UC-152 (5867–5875) *"Sürüm taşımaz"*, *"siparişin sürümü artmaz"*; K-1051 (satır 3197) aynı. Bulgu alıntının devamını atlamış. İki doğrulayıcı da çürüttü. | — |
| F8 | Parça 8 · BULGU-2 (§9.9.10 UC-131) | ❌ RET | **İki ayrı nesne.** POST'un yazdığı *"geri ödeme **satırı**"* (`06 §4.33` Refund) ile iş kuyruğu işleyicisinin kendi işleminde yazdığı *"geri ödeme **kaydı**"* (`06 §4.34` RefundRecord); `05 §4.6.1` (satır 550) kaydı işleyiciye bırakır, istek ancak ondan sonra sağlayıcıya gider. Satır 5704 sırayı açık yazar: sürüm artmış olduğundan ikinci gönderim 409 alır, doğana kadar satır bekleyen hâlindedir. *"İstekten önce"* sağlayıcıya giden isteği anlatır, POST'u değil. Önerilen ayrım ("satır" / "kayıt") dokümanda zaten bu adlarla yapılmış. İki doğrulayıcı da çürüttü. | — |

**Dağılım:** sekiz ayrı bulgu (sekiz ham bulgu) — 0 KABUL · 1 KISMİ · 7 RET. **%100 ret şüphelidir:** bu yüzden her ret ikinci bir temiz bağlamlı doğrulayıcıdan geçti ve yönetici F1, F6 ve F2'nin ilgili satırlarını bizzat okudu; yedi retten altısında iki doğrulayıcı da bağımsız olarak çürüttü, birinde (F2) ayrıştılar ve KISMİ çıktı. Retlerin hepsi dokümanın yazılı kararına ya da ilgili satırın tam metnine dayanır (K-1206, `04` 2.7.3.3 / K-745, `02 §12.2.5`, K-554, K-1051, `05 §4.6.1`); bulgunun alıntısı cümlenin devamını ya da komşu cümleyi atlamıştır — bu, ikinci modelin bölümleri parçalı görmesinin tipik kalıbıdır. **Sınıf:** KISMİ olan düşük şiddette bir sayı metni farkıdır. Faz 2 hukuki kontrolü: hiçbir bulgu yasal dayanak anmadı.

## 4. Ek bulgular

- **Aynı kalıbın taraması — F2.** `192` sayısı güncel bölümlerde geçmiyor; geçtiği yerler (başlık notundaki v0.13 özeti, §1.1.1 paragrafı) tarihsel anlatımdır ve ikisine de tarih notu düşüldü; öteki geçişler ilgisiz sayılardır (doğrulayıcının `grep` listesi).
- **Mekanik tarama.** Düzeltme yeni K numarasını yalnız başlık notuna ekledi (K-1220); matris satırı eklenmedi ya da çıkmadı — kümeler ve çiftler (5.073 · 2.319 · 187) değişmez; hata kodu sayıları ve uç sayısı değişmez. Başlık sürümü ve dosya sonu dipnotu v0.19'u gösterir.
- **Devredilen not (F3 — checkpoint), 1. turdan açık.** `05 §7.7.4` ve K-884 hakkın "düğmeye basıldığında **ve** sunucu dosyayı göndermeye başladığında" düştüğünü söyler; `07` §4.16.3 ve §4.31.1 tek adımı — UC-36'nın düğmesini — anlatır. Düşük şiddetlidir, `07`'nin bulgusu değildir; `05` cümlesine "bağlantı üretildiği an" eklemek istenirse Aşama 6'nın checkpoint'inde yapılır. Bu turda yeniden gelmedi; açık not olarak taşınır ve etki sütununa checkpoint'te bağlanır.
- **Ciddi bir sorun daha görülmedi.** Değerlendirme sırasında ikinci modelin listesi dışında ölçünün dört sınıfından birine giren bir sorun saptanmadı.

## 5. Kullanıcı onay checklist'i

K-1144 kararıyla (⚠ öneriyle kayıt, `docs:` PR'ında CI yeşilse merge yetkisi, yönetici + alt ajan düzeni) alt ajan değerlendirdi ve uyguladı (`07` v0.19); K-1207 gereği Claude alt ajanları yalnız Sonnet / low koştu. Bir karar öneriyle kaydedildi, ⚠ değildir: **K-1220** — 2. turun sonucu ve geçmiş sayım cümlelerine tarih notu düşme ilkesi. ⚠ listesine ekleme yok.

- [x] F1 (ret — K-1206 kalan risk)
- [x] F2 (kısmi — geçmiş sayım cümlesine tarih notu)
- [x] F3 (ret — iki ayrı olay)
- [x] F4 (ret — `04` 2.7.3.3 bilinçli karar, çıkış var)
- [x] F5 (ret — satış kapısı kapalıyken çerez açılmaz)
- [x] F6 (ret — istisna hücrede yazılı)
- [x] F7 (ret — istisna ortak kuralın cümlesinde)
- [x] F8 (ret — satır ile kayıt ayrı nesne)
- [ ] 3. tur — 2. turda bir kısmi bulgu uygulandı (düşük şiddette sayı metni farkı); TEMİZ aranır. Aynı istem, aynı betikler, aynı parça sınırları; Birleşim 1 ve 2 ile sekiz parça
- [ ] Etki yansıtma (Faz 5) — döngü TEMİZ döndükten sonra, son turun raporunda
