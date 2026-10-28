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
- [x] 3. tur — koştu (aşağıda, "Tur 3" bölümü)
- [ ] Etki yansıtma (Faz 5) — döngü TEMİZ döndükten sonra, son turun raporunda

---

# Cross-Review — 07 API Design (Tur 3)

**Tarih:** 2026-10-19 · **Hedef:** `Docs/07_API_DESIGN.md` v0.19 (girdi commit'i `58c8e17`, 1 190 211 bayt, 6 401 satır) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Ciddiyet ölçüsü:** 1. turla aynı (K-1219) · **Koşum:** 1. ve 2. turla aynı iki biçim — **iki birleşim** ve **sekiz parçalı kontrol koşumu**, aynı istem, aynı parça notu, aynı şablon (SHA-256 ilk 16 hane: istem `76e57ac0e4326f90`, parça notu `2b150c8107b170bb`, şablon `dfc8aa759c1e01c5` — 2. turla aynı; doküman v0.19 `44badc482724d803`), aynı betikler, parça sınırları başlıkla (K-1219) · **Tek parça çağrı:** yeniden denenmedi — girdi 1 094 838 karakter, sınır 1 048 576 (1. ve 2. turda aynı hatayla döndü; yönetici talimatı) · **Girdi:** yalnız doküman ve boş şablonu, her çağrıda boş ve izole çalışma klasöründe (K-431) · **Kullanım sınırı:** dolmadı — on çağrının hepsi çıktı verdi; yedeğe geçilmedi · **Sonuç:** TEMİZ değil — yedi ham bulgu (yedisi ayrı), 0 KABUL · 1 KISMİ · 6 RET; kısmi olan uygulandı (`07` v0.20); üç ret, 2. turda reddedilmiş iddianın dönüşüdür (yeni gerekçe yok; ikinci ret) · **Değerlendirme modeli:** Claude alt ajanları yalnız Sonnet / low (K-1207); Codex dışarıdadır

## 1. Koşum

**Betikler** 1. turdakilerdir (`C:\Users\TEKNOD~1\AppData\Local\Temp\claude\c--projects-Shopfolio\422e2f0a-46a6-4ced-809c-e90f0f447f55\scratchpad\cr07\`: `build_inputs.py`, `run_codex.sh`, `prompt.txt`, `partnote.txt`, `template07.md`); girdi klasörü bu tur için `..\cr07r3\in`, çıktı `..\cr07r3\out`. Komutlar: `python -I build_inputs.py Docs/07_API_DESIGN.md template07.md prompt.txt partnote.txt <girdi>` — kapsanmayan satır `none`; `bash run_codex.sh <girdi> <çıktı> c1 c2 p1 p2 p3 p4 p5 p6 p7 p8`. İstem ve parça notu 1. turun §1'indeki alıntı bloklarıdır, değiştirilmedi.

**Parçalar** (sınırlar 1. turdakiyle aynı başlıklar; satır numaraları v0.19'undur ve kaymıştır):

| Parça | Bölümler | Satırlar | Girdi | Token | Süre | Sonuç |
|---|---|---|---|---|---|---|
| 1 | §1'in girişi · §1.1.1–§1.1.2 | 111–638 | 135 518 bayt | 66 503 | 152 sn | 1 bulgu |
| 2 | §1.1.3–§1.1.5 | 639–1242 | 152 003 bayt | 66 902 | 37 sn | `SONUÇ: TEMİZ` |
| 3 | §1.1.6–§1.1.8 | 1243–2296 | 238 274 bayt | 100 179 | 110 sn | 1 bulgu |
| 4 | §1.1.9–§1.3 | 2297–3612 | 326 572 bayt | 131 069 | 30 sn | `SONUÇ: TEMİZ` |
| 5 | §2 · §3 | 3613–4119 | 158 651 bayt | 64 279 | 247 sn | 1 bulgu |
| 6 | §4 | 4120–4915 | 188 037 bayt | 73 231 | 177 sn | `SONUÇ: TEMİZ` |
| 7 | §5 · §6 · §7 · §8 | 4916–5411 | 130 161 bayt | 55 412 | 248 sn | `SONUÇ: TEMİZ` |
| 8 | §9 · §10 | 5412–6401 | 231 012 bayt | 84 779 | 37 sn | 1 bulgu |

| Çağrı | Parçalar | Girdi | Karakter | Token | Süre | Sonuç |
|---|---|---|---|---|---|---|
| Birleşim 1 | 2 · 3 · 4 · 5 · 6 · 7 · 8 | 1 114 479 bayt | 1 017 624 | 8 868 (CLI'nin bildirdiği; Birleşim 2'nin yarım milyonluk sayısına göre düşük — sayaç farkı, çıktı eksiksiz) | 293 sn | 3 bulgu |
| Birleşim 2 | 1 · 3 · 4 · 5 · 6 · 7 · 8 | 1 098 030 bayt | 1 004 096 | 408 954 | 127 sn | `SONUÇ: TEMİZ` |

**Kapsam sınırı ve web.** Parça öteki bölümleri görmez; birleşimler parçalar arası okumayı yapar (yalnız matrisin parça 1'i ile parça 2'si buluşmaz — K-1219). Yönetici değerlendirmesi her iddiayı dokümanın tamamına ve ilgili yerde `02` v0.92, `03` v0.39, `04` v0.39, `05` v0.28, `06` v0.30'a karşı okudu. **Web araması:** on çağrının hiçbirinde `web search` satırı yok; hiçbir bulgu yasal dayanak anmadı — Faz 2 madde 5 (hukuki kontrol) konusuz. **Toplam:** on Codex çağrısı, 1 060 176 token (CLI'nin bildirdiği; Birleşim 1'in sayısı düşük bildirilmiş olabilir — gerçek yük 1,4 milyon civarıdır).

## 2. Ham bulgular

İkinci modelin çıktıları, satır sonu boşlukları dışında düzenlenmeden (alıntı blokları içinde).

**Birleşim 1** (parça 2–8):
> BULGU-1
> Kriter: Güvenlik
> Seviye: Yüksek
> Yer: §1.1.8 — `04` 7.3.3.9
> Alıntı: "L-9 — doğrulama bağlantısının yeniden istenmesi ve bekleyenin yerine geçen yeni e-posta isteği; önceki bağlantı geçerli kalır"
> Sorun: Bu, UC-49, UC-67 ve UC-180 tanımlarındaki “öncekiler … geçersizleşir — `SupersededAt`” kuralıyla çelişir. Önceki bağlantı geçerliyse, yerine geçen e-posta değişikliğinin eski bağlantısı daha sonra kullanılarak hesabın e-postası istenmeyen eski adrese değiştirilebilir. Bu, hesap erişimi açısından ciddi bir güvenlik çelişkisidir (4).
> Öneri: `04` 7.3.3.9 matris özetini ve ilgili L-9 açıklamalarını, yeni bağlantının önceki bağlantıyı koşullu olarak geçersizleştirdiğini açıkça söyleyecek biçimde düzeltin.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §2.2.2 / §9.26.2 — UC-117
> Alıntı: "**Yanıt (başarı):** 200 — kayıt yazılmadığı için yönlendirme yoktur"
> Sorun: §2.2.2, başarılı yazan her POST’un 303 ile yönlendirilmesini zorunlu tutar. UC-117 ise 200 döndüğünü söyler; aynı tanımın yan etkisinde bağsız bir `Image` kaydının yazıldığı belirtilir. Bu doğrudan çelişkidir (4); 200 yanıtının yenilenmesi görüntü yüklemesini yeniden gönderebilir.
> Öneri: UC-117’yi 303 ile GET’e yönlendirecek biçimde tasarlayın ve form değerlerini koruma mekanizmasını tanımlayın; bu tercih edilmiyorsa §2.2.2’ye UC-117 için açık istisna ile yeniden gönderim davranışını ekleyin.
>
> BULGU-3
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §6.2.2 — `InvalidDateRange`
> Alıntı: "| `InvalidDateRange` | 422 | Bitiş başlangıçtan önce (`04` 7.3.2.12); liste GET'inde aynı kural alan mesajıdır ve durum 200'dür"
> Sorun: Kodun HTTP sütunu 422 iken aynı satır, UC-105 ve UC-106 liste GET’lerinde aynı kodun 200 döndüğünü söylüyor. `InvalidDate` satırında da aynı çelişki vardır. Bu, hata kodu ve HTTP davranışını uygulama/test için belirsiz bırakır (4).
> Öneri: Her iki satırın HTTP sütununu açıkça “422 (POST) / 200 (GET)” yapın veya GET doğrulaması için ayrı kodlar tanımlayın.
>
> SONUÇ: 3 BULGU

**Birleşim 2** (parça 1, 3–8):
> SONUÇ: TEMİZ

**Parça 1** (§1'in girişi, §1.1.1–§1.1.2):
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §1 alt bölüm haritası, §1.3 satırı
> Alıntı: "geri besleme satırı 19 — GAP-14 (2), K-1183 (yazım 2b), K-1192 (yazım 3a), K-1199…K-1204 (audit), K-1208…K-1217 (deep review)"
> Sorun: Listelenen satırlar 2 + 1 + 1 + 6 + 10 = 20 eder; belirtilen toplam 19’dur. Bu, geri besleme izlenebilirliğinde dokümanın kendi içinde çelişkidir (sınıf 4) ve hangi geri besleme kaydının kapsamda olduğunun belirsiz kalmasına yol açar.
> Öneri: Toplamı 20 olarak düzeltin veya kapsam dışı tutulacak kaydı açıkça belirtip listedeki K aralığını ve §1.3 tablosunu buna göre hizalayın.
>
> SONUÇ: 1 BULGU

**Parça 2** (§1.1.3–§1.1.5):
> SONUÇ: TEMİZ

**Parça 3** (§1.1.6–§1.1.8):
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §1.1.6 giriş / `04` 7.2.15.1
> Alıntı: “E-37'nin bütün işlemleri siparişin sürümünü taşır ve sipariş değiştiyse uygulanmaz” / “İç not — tek serbest metin, müşteriye görünmez; ekranın gösterdiği değere karşı yazılır”
> Sorun: İç not E-37 işlemi olmasına rağmen sipariş sürümü yerine alanın gösterilen değerine göre koşullu yazılır. Bu, E-37’deki tüm işlemlerin sipariş sürümü değişince uygulanmayacağı kuralıyla çelişir (sınıf 4).
> Öneri: UC-152’yi açık istisna olarak belirtin: iç notun sipariş sürümü değil kendi gösterilen değeriyle denetlendiğini ve sipariş sürümü değişse de uygulanıp uygulanmayacağını kesinleştirin.
>
> SONUÇ: 1 BULGU

**Parça 4** (§1.1.9–§1.3):
> SONUÇ: TEMİZ

**Parça 5** (§2 · §3):
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §2.4.3, §2.7.1
> Alıntı: “Müşterinin işleminde gösterilen açık adet tutmazsa … istek bütünüyle uygulanmaz: 303 → kök” / “sürüm çakışması ve gösterilen değerin tutmaması 409”
> Sorun: Aynı “gösterilen değer” uyuşmazlığı için iki farklı HTTP sonucu tanımlanmış. Bu, özellikle kalem iptali ve cayma uçlarında (UC-40, UC-43) istemcinin, testlerin ve hata akışının hangi davranışı uygulayacağını belirsizleştirir; dokümanın kendi iç çelişkisidir (4).
> Öneri: §2.7.1’i açık istisnalarla düzeltin: müşteri işlemlerindeki `OriginalOpenQuantity` ve geçici kapatma anahtarı uyuşmazlıklarının 303 ile güncel hâle yönlendirilmesini yazın; yalnız 409 dönen gösterilen-değer uyuşmazlıklarını ayrı belirtin.
>
> SONUÇ: 1 BULGU

**Parça 6** (§4):
> SONUÇ: TEMİZ

**Parça 7** (§5–§8):
> SONUÇ: TEMİZ

**Parça 8** (§9 · §10):
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §9.9 ortak kural / §9.9.31 UC-152
> Alıntı: “her form siparişin `Version`'ını taşır” / “Sürüm taşımaz.”
> Sorun: Sipariş ayrıntısının ortak kuralı UC-152 dâhil tüm formlarda sipariş sürümünü zorunlu kılarken, iç not formu bunu açıkça dışlar. Aynı uç için iki farklı eşzamanlılık sözleşmesi oluşuyor; bu, dokümanın kendi güvencesiyle çelişkidir (4).
> Öneri: Ortak kurala UC-152’nin açık istisnasını ekleyin ve iç notun yalnız `Original…` değeriyle koşullu yazıldığını belirtin; ya da UC-152’ye `Version` alanını ekleyin.
>
> SONUÇ: 1 BULGU

## 3. Bağımsız değerlendirme

Her bulgu dokümanın tamamına (v0.19) ve ilgili yerde `02` v0.92, `03` v0.39, `04` v0.39, `05` v0.28, `06` v0.30'a karşı okundu. **Koşum (K-1207):** her bulguyu önce bir Sonnet / low doğrulayıcı okudu; yedi bulgunun altısını "çürütüldü" dedi, biri doğrulandı (düşük) — kural gereği (çürütülen ve orta ya da yüksek iddialı bulgular ikinci bakışa gider) yedisi de ikinci, ayrı bir Sonnet / low doğrulayıcıdan geçti (temiz bağlam; birincinin kararı verilmedi). F5 ve F7 tek ajanda ayrı hükümlerle okundu (aynı konu, iki ayrı yer). İki doğrulayıcı F2 ve F5'te ayrıştı; son hüküm yönetici ajandadır (satır 3668, 6380–6383; 571, 1245, 3712, 5632 okundu). **2. turdan tekrar eden iddialar:** F1 (2. tur F3), F3 (2. tur F6), F7 (2. tur F7) — bu tur yeni gerekçe ya da kanıt getirmedi; doğrulayıcılar yeni bir şey olup olmadığını ayrıca yazdı (hayır). Üçü bu raporda **ikinci ret** olarak kaydedilir; bir daha dönerse "tekrar eden RET" sayılır ve turun sonucunu belirlemez (skill Faz 4 madde 5).

| # | Çağrı · bulgu | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|---|
| F1 | Birleşim 1 · BULGU-1 (§1.1.8 — `04` 7.3.3.9, L-9) | ❌ RET (ikinci ret — 2. tur F3) | **İki ayrı olay; çelişki yok.** Matris satırı (satır 2290) `04` 7.3.3.9'un özetidir; `04` L-9'un *limit mesajını* tarif eder ve yer sütunu "önceki bağlantı ömrü içinde geçerli kalır" der. `03 §6.1.1.9` (satır 1100): *"İstek engellenir; kayıt silinmez, önceki bağlantı ömrü içinde geçerli kalır"* — limit aşılınca yeni bağlantı üretilmez, bu yüzden eskisi kalır. Başarılı yeniden istek ise `03 §7.1.49` (satır 1277), `§9.1.3` (satır 1631), `04` 5.20.7 ve `07`'nin UC-49, UC-67, UC-180 yan etkilerinde öncekileri `SupersededAt`'a düşürür. İddia edilen saldırı (yerine geçen e-posta değişikliğinin eski bağlantısıyla adres değiştirme) `SupersededAt` yüzünden oluşmaz; "Yüksek" güvenlik iddiası dayanaksızdır. İki doğrulayıcı da çürüttü, ikisi de yeni gerekçe bulmadı. | — |
| F2 | Birleşim 1 · BULGU-2 (§2.2.2 / §9.26.2, UC-117) | ❌ RET (yönetici hükmü — doğrulayıcılar ayrıştı) | **Bilinçli karar; çelişki yok.** İlk doğrulayıcı Düşük / (4) ile doğruladı, ikinci doğrulayıcı çürüttü; hüküm yöneticide. §2.2.2 (satır 3668) yönlendirmeyi *yazan ve başarılı* POST'a koşar; yazmayan POST'u — *ara gönderim* dahil — sayfayı gönderilen değerlerle yeniden göstermeye bağlar. UC-117 (satır 6380) *formun kaydı* yazılmadığı için yönlendirmez ve aynı kalıpla formu bütün girdileriyle yeniden gösterir; görselin kayda bağlanması formun kaydındadır (UC-114, UC-158, UC-163). Yan etkideki bağsız `Image` satırı (satır 6382) formun kaydı sayılmaz: kaydın sürümü değişmez, işlem izine yazılmaz. Karar K-1171 (5)'tedir ve "yönlendirme yoktur" bilinçle yazılmıştır. Yenileme riski kapalıdır: *"her gönderim yeni bir bağsız görseldir"* (satır 6383), bağsız görsel bir gün sonra imhada silinir; para ya da hak kaybı yok, kullanıcı çıkışsız kalmaz (form girdileri korunur). Önerilen 303, yüklemenin sürerken form girdilerini korumasını bozar. Ölçünün sınıfı yok. | — |
| F3 | Birleşim 1 · BULGU-3 (§6.2.2 — `InvalidDateRange`, `InvalidDate`) | ❌ RET (ikinci ret — 2. tur F6) | **İstisna aynı satırda iki yerde yazılı.** Satır 5154: HTTP sütunu `422`, açıklama hücresi *"liste GET'inde aynı kural alan mesajıdır ve durum 200'dür (§2.6.6)"*, kullanım sütunu *"UC-105, UC-106 — liste GET'inde, 200"*; satır 5153 (`InvalidDate`) aynı kalıpta; §2.6.6 (satır 3764) ve satır 3628 aynısını söyler. İddianın alıntısı kullanım sütununu atlamış. Tabloda bu kalıp tek tip değil (`SalesClosed` `422 · 303` biçimini HTTP sütununda, `DeliveryProvinceNotServed` ve `InvalidDate` açıklama / kullanım sütunlarında taşır) — bu yazım kalıbı farkıdır, çelişki değildir; 200 yanıtında hata kodu zaten yoktur (alan mesajı gövdede gider). İki doğrulayıcı da çürüttü, yeni gerekçe yok. | — |
| F4 | Parça 1 · BULGU-1 (§1 alt bölüm haritası, §1.3 satırı) | ❌ RET | **Sayım yanlış okunmuş.** Başlık notunun §1.3 hücresi *"geri besleme satırı 19 — GAP-14 (2), K-1183 (yazım 2b), K-1192 (yazım 3a), K-1199…K-1204 (audit), K-1208…K-1217 (deep review)"* der; `GAP-14 (2)` parantezi satır sayısı değil **matrisin 2. oturumunun** numarasıdır (satır 3581 *"2 (2026-10-18; K-1161)"*, satır 3591 *"matrisin 2. oturumu"*; aynı hücrenin başındaki *"✓ (2) — GAP-14"* de aynı). §1.3'ün geri besleme tablosunun veri satırları 3591–3609 arasındadır ve 19 tanedir: GAP-14/K-1161 1 · K-1183 1 · K-1192 1 · K-1199…K-1204 6 · K-1208…K-1217 10 = 19. Başlık notunda "geri besleme satırı N" biçiminde ikinci bir sayı yok. İki doğrulayıcı bağımsız saydı, ikisi de 19 buldu. | — |
| F5 | Parça 3 · BULGU-1 (§1.1.6 giriş / `04` 7.2.15.1) | ⚠️ KISMİ (Düşük — (4), yönetici hükmü — doğrulayıcılar ayrıştı) | **Gövde doğru, iki özet yeri "bütün" diyordu.** `04` 7.2.15.1 satırı doğru yazıyor (iç not *ekranın gösterdiği değere karşı yazılır*); sorun matris satırı değil, **§1.1.6'nın giriş cümlesi** (satır 1245: *"E-37'nin bütün işlemleri siparişin sürümünü taşır ve sipariş değiştiyse uygulanmaz"*) ve **UC-123'ün matris hücresi** (satır 571: *"E-37'nin bütün işlemlerinde"*): ikisinde de iç not istisnası yok. Gövde tek ve tutarlı: §2.4.2 (satır 3712) *"iç not hariç; o gösterilen metni taşır"*, §9.9 (satır 5632) aynı, UC-152 *"Sürüm taşımaz"* (K-1051; başlık notu satır 31'de de sayılı). İlk doğrulayıcı Düşük / (4) dedi, ikinci doğrulayıcı gövdenin güvenceyi bozmadığını (UC-152'nin iç not metnine karşı 409'u var) ve cümlenin "özet kabalığı" olduğunu söyledi. Hüküm: ölçünün (4) sınıfında düşük şiddette metin farkı — 1. turun F7'sinde (§8.1.4) aynı sınıf KISMİ ile istisna yazılarak kapandı; "bütün" nicelleyicisi tek başına okuyan uygulayıcıyı yanıltabilir. Önerilen çözüm (iç not istisnası) kabul, gerekçe (güvence bozuluyor / sürüm yerine değer) reddedildi: güvence bozulmuyor. | §1.1.6 giriş cümlesi: *"E-37'nin bütün işlemleri — iç not hariç; o gösterilen metni taşır (§2.4.3; K-1051) — siparişin sürümünü taşır…"*; UC-123 matris hücresi: *"E-37'nin bütün işlemlerinde (iç not hariç; o gösterilen metni taşır — §2.4.3; K-1051)"* |
| F6 | Parça 5 · BULGU-1 (§2.4.3 / §2.7.1) | ❌ RET | **Aynı olay için iki sonuç yok.** §2.4.3 (satır 3714) gösterilen değerleri değer başına ayırır: iç not ve elle sıra tutmazsa 409; geçici kapatma anahtarı tutmazsa 303 ve güncel hâl (§9.16.3, §9.16.4; K-1194 (1)); müşterinin gösterilen açık adedi tutmazsa 303 → kök (UC-40, UC-43; K-1202). UC-40 (satır 4463) ve UC-43 (satır 4492) hata kollarında bu olay yalnız 303'tür; `QuantityExceedsOpen` §6.2 (satır 5249) ve §6.3.3 (satır 5335) ile tutarlı. §2.7.1 (satır 3780) genel bir cümledir ve **istisnaların yazılı olduğu yere atıf yapar** (*"gösterilen değerin tutmaması 409 (§2.4.3, §2.4.5, §2.4.6)"*); paragrafın sonu adı konmuş hâllerin durum kodunu §6.2'ye bırakır ve *sebebi yönlendirmeyle söyleyen ret 303 döner* der. F5'ten farkı: orada "bütün" nicelleyicisi atıfsız ve istisnayla çelişiyordu; burada cümle istisnanın evine bağlıdır. İki doğrulayıcı da çürüttü (ikincisi cümlenin kaba bir özet olduğunu, çelişki olmadığını yazdı). | — |
| F7 | Parça 8 · BULGU-1 (§9.9 ortak kural / UC-152) | ❌ RET (ikinci ret — 2. tur F7) | **İstisna ortak kuralın cümlesinde.** Satır 5632: *"her form siparişin `Version`'ını taşır, **iç not gösterilen metni** (§2.4.2, §2.4.3)"* — alıntı cümleyi `iç not gösterilen metni` kısmında kesmiş. UC-152 *"Sürüm taşımaz"* ve *"siparişin sürümü artmaz"*; §2.4.2 (satır 3712) *"iç not hariç"*; K-1051 aynı. Tek sözleşme var. İki doğrulayıcı da çürüttü, yeni gerekçe yok. | — |

**Dağılım:** yedi ayrı bulgu (yedi ham bulgu) — 0 KABUL · 1 KISMİ · 6 RET. **%100 ret şüphelidir:** bu yüzden her bulgu ikinci bir temiz bağlamlı doğrulayıcıdan geçti ve yönetici F2, F5, F6 ile F4'ün ilgili satırlarını bizzat okudu; retlerin altısında iki doğrulayıcı da çürüttü (F2'de ayrıştılar ve hüküm RET), F5'te ayrıştılar ve hüküm KISMİ çıktı. Retlerin hepsi dokümanın yazılı kararına ya da ilgili satırın tam metnine dayanır (K-1171 (5), `03 §6.1.1.9` / `§7.1.49`, §2.6.6, §2.4.3 / §6.2, §9.9 / K-1051); F4 modelin bir parantezi yanlış okumasıdır. Alıntıların atladığı cümle devamı ya da komşu sütun, ikinci modelin bölümleri parçalı görmesinin tipik kalıbıdır. **Sınıf:** KISMİ olan düşük şiddette bir metin farkıdır (özet cümlesinin gövdenin istisnasını taşımaması); yeni bir yüzey açmadı. Kabul edilen bulgu sayısı 1. turda 4, 2. turda 0 (KISMİ 1), bu turda 0 (KISMİ 1): kabul edilenlerin hepsi düşük şiddetli metin farkıdır, orta ya da yüksek yok. Faz 2 hukuki kontrolü: hiçbir bulgu yasal dayanak anmadı.

## 4. Ek bulgular

- **Aynı kalıbın taraması — F5.** `grep` "E-37'nin bütün" / "bütün işlemlerinde": başka yer yok (satır 571 ve 1245 düzeltildi); `04` 7.2.15.1 satırı iç notu zaten gösterilen değere bağlar. "Her form" ya da "bütün işlemleri" biçiminde sürümü genelleyen öteki yerler (§9.9 ortak kural, §2.4.2) istisnayı kendi cümlesinde taşıyor.
- **Mekanik tarama.** Düzeltme yeni K numarasını yalnız başlık notuna ekledi (K-1221); matris satırı eklenmedi ya da çıkmadı (UC-123 hücresinin metni değişti, satırın eşlemesi değişmedi) — kümeler ve çiftler (5.073 · 2.319 · 187) değişmez; hata kodu sayıları ve uç sayısı değişmez. Başlık sürümü ve dosya sonu dipnotu v0.20'yi gösterir.
- **Devredilen not (F3 — checkpoint), 1. turdan açık.** `05 §7.7.4` ve K-884 hakkın "düğmeye basıldığında **ve** sunucu dosyayı göndermeye başladığında" düştüğünü söyler; `07` §4.16.3 ve §4.31.1 tek adımı — UC-36'nın düğmesini — anlatır. Düşük şiddetlidir, `07`'nin bulgusu değildir; `05` cümlesine "bağlantı üretildiği an" eklemek istenirse Aşama 6'nın checkpoint'inde yapılır. Bu turda da yeniden gelmedi; açık not olarak taşınır ve etki sütununa checkpoint'te bağlanır.
- **Yeni devredilen not — §2.7.1'in genel cümlesi (F6).** *"Sürüm çakışması ve gösterilen değerin tutmaması 409"* cümlesi atıfla (§2.4.3) ve §6.2'nin satır satır kodlarıyla desteklenir ve ölçünün (4) sınıfında değildir; ama tek başına okunduğunda kabadır (geçici kapatma anahtarı ve müşterinin kalem işlemleri 303 alır). Uygulanmadı — düzeltmesi istenirse checkpoint'in iç tutarlılık merceği "özet cümlesi gövdenin istisnasını taşımıyor" sınıfını dokümanın tamamında arar (skill Faz 4 madde 5'in devredilen sınıfı); açık not olarak taşınır.
- **Ciddi bir sorun daha görülmedi.** Değerlendirme sırasında ikinci modelin listesi dışında ölçünün dört sınıfından birine giren bir sorun saptanmadı.

## 5. Kullanıcı onay checklist'i

K-1144 kararıyla (⚠ öneriyle kayıt, `docs:` PR'ında CI yeşilse merge yetkisi, yönetici + alt ajan düzeni) alt ajan değerlendirdi ve uyguladı (`07` v0.20); K-1207 gereği Claude alt ajanları yalnız Sonnet / low koştu. Bir karar öneriyle kaydedildi, ⚠ değildir: **K-1221** — 3. turun sonucu, 2. turdan dönen üç iddianın ikinci ret'i ve özet cümlelerinin gövdenin istisnasını taşıması ilkesi. ⚠ listesine ekleme yok.

- [x] F1 (ret — iki ayrı olay; ikinci ret)
- [x] F2 (ret — K-1171 (5); formun kaydı yazılmaz, ara gönderim kalıbı)
- [x] F3 (ret — istisna satırda iki yerde yazılı; ikinci ret)
- [x] F4 (ret — `GAP-14 (2)` oturum numarası; satır sayısı 19)
- [x] F5 (kısmi — §1.1.6 giriş cümlesi ve UC-123 hücresine iç not istisnası)
- [x] F6 (ret — istisnalar §2.4.3'te ve §6.2'de, §2.7.1 atıf yapar)
- [x] F7 (ret — istisna ortak kuralın cümlesinde; ikinci ret)
- [x] 4. tur — koştu (aşağıda, "Tur 4" bölümü)
- [ ] Etki yansıtma (Faz 5) — döngü TEMİZ döndükten sonra, son turun raporunda

---

# Cross-Review — 07 API Design (Tur 4)

**Tarih:** 2026-10-19 · **Hedef:** `Docs/07_API_DESIGN.md` v0.20 (girdi commit'i `9bc8b01`, 1 191 597 bayt, 6 403 satır) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Ciddiyet ölçüsü:** 1. turdan itibaren uygulanır (K-1219); bu turda yöneticinin kararıyla "bu turdan itibaren uygulanır" diye yeniden teyit edildi ve istem **değiştirilmedi** — ölçü paragrafı 1.–3. turun isteminde zaten vardı (istem SHA-256 `76e57ac0e4326f90`, öncekilerle aynı); bilinçli karar cümlesi değişmeden kalır · **Koşum:** 1.–3. turla aynı iki biçim — **iki birleşim** ve **sekiz parçalı kontrol koşumu**, aynı istem, aynı parça notu, aynı şablon (SHA-256 ilk 16 hane: istem `76e57ac0e4326f90`, parça notu `2b150c8107b170bb`, şablon `dfc8aa759c1e01c5` — 3. turla aynı; doküman v0.20 `a1ed9bf02807c65b`), aynı betikler, parça sınırları başlıkla (K-1219) · **Tek parça çağrı:** yeniden denenmedi — girdi 1 096 093 karakter, sınır 1 048 576 (1.–3. turda aynı hatayla döndü; yönetici talimatı) · **Girdi:** yalnız doküman ve boş şablonu, her çağrıda boş ve izole çalışma klasöründe (K-431) · **Kullanım sınırı:** dolmadı — on çağrının hepsi çıktı verdi; yedeğe geçilmedi · **Sonuç:** TEMİZ değil — altı ham bulgu (altısı ayrı), 0 KABUL · 0 KISMİ · 6 RET; doküman içeriği değişmedi (`07` v0.21 yalnız sürüm ve başlık notu); bir ret, 3. kez dönen iddiadır ("tekrar eden RET" — turun sonucunu belirlemez) · **Değerlendirme modeli:** Claude alt ajanları yalnız Sonnet / low (K-1207); Codex dışarıdadır

## 1. Koşum

**Betikler** 1. turdakilerdir (`C:\Users\TEKNOD~1\AppData\Local\Temp\claude\c--projects-Shopfolio\422e2f0a-46a6-4ced-809c-e90f0f447f55\scratchpad\cr07\`: `build_inputs.py`, `run_codex.sh`, `prompt.txt`, `partnote.txt`, `template07.md`); girdi klasörü bu tur için `..\cr07r4\in`, çıktı `..\cr07r4\out`. Komutlar: `python -I build_inputs.py Docs/07_API_DESIGN.md template07.md prompt.txt partnote.txt <girdi>` — kapsanmayan satır `none`; `bash run_codex.sh <girdi> <çıktı> c1 c2 p1 p2 p3 p4 p5 p6 p7 p8`. İstem ve parça notu 1. turun §1'indeki alıntı bloklarıdır, değiştirilmedi. **Ölçü ile ilgili not:** yöneticinin talimatı istemi "önceki istem + ciddiyet ölçüsü" diye tarif ediyordu; ölçü 1. turdan beri istemdedir (K-1219 — rapor başlıkları da "1. turla aynı" der), bu yüzden ikinci kez eklenmedi.

**Parçalar** (sınırlar 1. turdakiyle aynı başlıklar; satır numaraları v0.20'nindir ve kaymıştır):

| Parça | Bölümler | Satırlar | Girdi | Token | Süre | Sonuç |
|---|---|---|---|---|---|---|
| 1 | §1'in girişi · §1.1.1–§1.1.2 | 113–640 | 136 834 bayt | 63 178 | 66 sn | `SONUÇ: TEMİZ` |
| 2 | §1.1.3–§1.1.5 | 641–1244 | 153 253 bayt | 54 329 | 28 sn | `SONUÇ: TEMİZ` |
| 3 | §1.1.6–§1.1.8 | 1245–2298 | 239 594 bayt | 91 664 | 166 sn | 1 bulgu |
| 4 | §1.1.9–§1.3 | 2299–3614 | 327 822 bayt | 121 467 | 135 sn | 2 bulgu |
| 5 | §2 · §3 | 3615–4121 | 159 901 bayt | 58 341 | 34 sn | `SONUÇ: TEMİZ` |
| 6 | §4 | 4122–4917 | 189 287 bayt | 83 683 | 166 sn | 2 bulgu |
| 7 | §5 · §6 · §7 · §8 | 4918–5413 | 131 411 bayt | 49 881 | 99 sn | `SONUÇ: TEMİZ` |
| 8 | §9 · §10 | 5414–6403 | 232 262 bayt | 85 940 | 22 sn | `SONUÇ: TEMİZ` |

| Çağrı | Parçalar | Girdi | Karakter | Token | Süre | Sonuç |
|---|---|---|---|---|---|---|
| Birleşim 1 | 2 · 3 · 4 · 5 · 6 · 7 · 8 | 1 115 799 bayt | 1 018 821 | 404 355 | 229 sn | 1 bulgu |
| Birleşim 2 | 1 · 3 · 4 · 5 · 6 · 7 · 8 | 1 099 416 bayt | 1 005 351 | 396 854 | 110 sn | `SONUÇ: TEMİZ` |

**Kapsam sınırı ve web.** Parça öteki bölümleri görmez; birleşimler parçalar arası okumayı yapar (yalnız matrisin parça 1'i ile parça 2'si buluşmaz — K-1219). Yönetici değerlendirmesi her iddiayı dokümanın tamamına ve ilgili yerde `02` v0.92, `03` v0.39, `04` v0.39, `05` v0.28, `06` v0.30'a karşı okudu. **Web araması:** on çağrının hiçbirinde `web search` satırı yok; hiçbir bulgu yasal dayanak anmadı — Faz 2 madde 5 (hukuki kontrol) konusuz. **Toplam:** on Codex çağrısı, 1 409 692 token (CLI'nin bildirdiği; bu turda Birleşim 1'in sayacı da eksiksiz bildirildi).

## 2. Ham bulgular

İkinci modelin çıktıları, satır sonu boşlukları dışında düzenlenmeden (alıntı blokları içinde).

**Birleşim 1** (parça 2–8):
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §2.3.1; §4.22.2–§4.22.3
> Alıntı: “Çerez politikası ilk kez yayına alınana kadar vitrinin hiçbir yanıtı çerez yazmaz” / “oturum kaydı — her giriş yeni anahtar”
> Sorun: §2.3.1, çerez politikası yayımlanana dek tüm vitrin yanıtlarından çerez yazımını kaldırırken; müşteri girişi ve Google dönüşü başarılı olduğunda oturum ve tanınan tarayıcı çerezlerinin yazılmasını zorunlu tutuyor. Aynı koşulda başarılı giriş hem gerçekleşip hem kalıcı oturum kuramaz. Bu, dokümanın kendi içindeki çelişkidir (4).
> Öneri: Çerez politikası yayımlanana kadar oturum, sahteciliğe karşı koruma ve dış giriş akışı için zorunlu çerezlere açık istisna tanımlayın; ya da bu dönemde müşteri girişini de sunucu tarafında kapatın ve ilgili uçları hizalayın.
>
> SONUÇ: 1 BULGU

**Birleşim 2** (parça 1, 3–8):
> SONUÇ: TEMİZ

**Parça 1** (§1'in girişi, §1.1.1–§1.1.2):
> SONUÇ: TEMİZ

**Parça 2** (§1.1.3–§1.1.5):
> SONUÇ: TEMİZ

**Parça 3** (§1.1.6–§1.1.8):
> BULGU-1
> Kriter: güvenlik
> Seviye: Yüksek
> Yer: Kalite döngüsü — cross-review 1. tur; §1.1.6 `04` 9.25.5; §1.1.7 `04` 6.2.28.3
> Alıntı: "yönetici e-posta değişikliğini geri almada eski adresin başka bir yönetici hesabına geçmiş olması UC-187'de ele alınmamıştı — UC-76'nın 422 yolu eklendi"
> Sorun: Düzeltme yanlış uca uygulanmış görünüyor. UC-76 müşteri hesabının geri alma işlemidir; yönetici e-posta geri alma işlemi UC-187’dir. UC-187 için eski adresin bu arada başka yöneticiye atanması hâli ve güvenli ret davranışı tanımlı değildir. Bu durumda ele geçirilmiş yönetici hesabının e-postası geri alınamayabilir; yetkili yönetici erişimini kaybedip saldırgan panel ve kişisel verilere erişmeyi sürdürebilir.
> Öneri: UC-187’ye, eski adres başka yönetici hesabında ise geri almayı uygulamayan bir 422 yolu ekleyin; bağlantının tüketimi ve kullanıcıya sunulacak güvenli kurtarma yolunu açıkça tanımlayın.
>
> SONUÇ: 1 BULGU

**Parça 4** (§1.1.9–§1.3):
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §1.1.10 `03 §6.1.1.9`; §1.1.9 `03 §3.4.14`
> Alıntı: “önceki bağlantı geçerli kalır” / “yeni bağlantı öncekini geçersiz kılar, ömrü uzatmaz”
> Sorun: Doğrulama bağlantısının yeniden istenmesinde önceki bağlantının geçerliliği iki farklı biçimde tanımlanmış. Bu, aynı bağlantının sunucuda hem kabul hem reddedilmesine yol açabilecek doğrudan doküman içi çelişkidir (4).
> Öneri: Tek kural seçin ve kayıt ile yeni e-posta doğrulaması dahil bütün ilgili satırları hizalayın. Yeni bağlantı öncekini geçersiz kılacaksa `03 §6.1.1.9` düzeltilmelidir.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §1.1.9 `03 §2.3`, `03 §4.1.6`; §1.1.12 `05 §6.7.8`
> Alıntı: “Sepet kendiliğinden boşalmaz (Z-6)” / “Z-6 sepetin ömrü — dolmaz” / “Misafir sepeti çerezi 400 gün açılmazsa sunucudaki sepet gecelik temizlikte silinir”
> Sorun: Sepetin süresiz olduğu belirtilirken, misafir sepetinin 400 günlük hareketsizlikten sonra silineceği yazılmıştır. Bu, sepetin yaşam döngüsünde doğrudan çelişkidir (4).
> Öneri: 400 gün sonra silme bilinçli davranışsa Z-6 ve ilgili akış satırlarını “üye sepeti süresizdir; misafir sepeti 400 gün hareketsizlikten sonra silinir” şeklinde hizalayın; değilse temizleme kuralını kaldırın.
>
> SONUÇ: 2 BULGU

**Parça 5** (§2 · §3):
> SONUÇ: TEMİZ

**Parça 6** (§4):
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §4.29.1 UC-30
> Alıntı: "**Yöntem** GET'tir; seçilen sağlayıcı dönüşü POST'la yapıyorsa aynı adres POST'u da kabul eder"
> Sorun: Aynı uç hem yalnız GET olarak tanımlanıyor hem de koşullu olarak POST kabul ediyor. Bu, uç envanteri, yöntem kısıtları ve POST için sahteciliğe karşı koruma istisnası bakımından çelişkidir (4).
> Öneri: Sağlayıcının dönüş yöntemini kesinleştirip UC-30’u ve envanteri buna göre tek yöntemle tanımlayın; iki yöntem desteklenecekse ikisini de açıkça envantere ekleyin ve POST istisnasını kalıcı olarak belirtin.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §4 ortak yanıtlar / §4.34.1 UC-77
> Alıntı: "oturum gerektiren GET'in ve oturumu kapanmış POST'un girişe yönlendirmesi (§2.3.2)" / "tekrar gönderim oturumu bulamaz — aynı yönlendirmeyi alır"
> Sorun: Ortak kural, oturumu kapanmış POST’un girişe yönlenmesini zorunlu kılarken UC-77, silinmiş oturumla yeniden gönderilen çıkış isteğinin `returnUrl` hedefine yönleneceğini söylüyor. Aynı durum için iki farklı davranış tanımlanmıştır (4).
> Öneri: Çıkış ucu için açık bir §2 istisnası tanımlayın veya UC-77’nin tekrar gönderim davranışını girişe yönlendirme kuralına uyarlayın.
>
> SONUÇ: 2 BULGU

**Parça 7** (§5–§8):
> SONUÇ: TEMİZ

**Parça 8** (§9 · §10):
> SONUÇ: TEMİZ

## 3. Bağımsız değerlendirme

Her bulgu dokümanın tamamına (v0.20) ve ilgili yerde `02` v0.92, `03` v0.39, `04` v0.39, `05` v0.28, `06` v0.30'a karşı okundu. **Koşum (K-1207):** her bulguyu önce bir Sonnet / low doğrulayıcı okudu; altısını da "çürütüldü" dedi (şiddet Düşük, sınıf yok) — kural gereği (çürütülen bulgular ikinci bakışa gider) altısı da ikinci, ayrı bir Sonnet / low doğrulayıcıdan geçti (temiz bağlam; birincinin kararı verilmedi). İki doğrulayıcı altı bulguda da ayrışmadı. Yönetici ajan, doğrulayıcıların dayandığı satırları bizzat okudu: `07` 3692, 3694, 4137, 6361, 3913, 2292; `05` 777, 781; `02` Z-6 (1121). **Önceki turlardan dönen iddia:** F3 (`03 §6.1.1.9`) — 2. turda F3 ve 3. turda F1 olarak reddedilmişti; bu tur yeni gerekçe ya da kanıt getirmedi → **tekrar eden RET** (üçüncü ret; skill Faz 4 madde 5: turun sonucunu belirlemez). Öteki beş iddia yenidir; hiçbiri önceki turlarda görülmemişti.

| # | Çağrı · bulgu | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|---|
| F1 | Birleşim 1 · BULGU-1 (§2.3.1; §4.22.2–§4.22.3) | ❌ RET | **Çelişen iki durum aynı anda var olmaz.** §2.3.1 (satır 3692) çerez yasağını *"çerez politikası ilk kez yayına alınana kadar"* koyar ve `05 §6.7.10` (satır 781) mekanizmayı açıklar: asıl güvence **kapalı kapılardır** — satış ve veri toplayan girişler (`02 §3.1.5`) çerez gerektiren işlevleri zaten kapatır, ara katman ikinci kattır; kapı yayın anından sonraki ilk istekte açılır, geri kapanmaz. Kayıt ve Google ile ilk giriş kapalıyken politika yayımlanmadan hiçbir müşteri hesabı doğamaz; §4.22.2 (şifreyle giriş, satır 4598) ve §4.22.3 (Google dönüşü, satır 4606) oturum çerezi yazması gereken koşulda — yasağın sürdüğü dönemde — çalışacak bir hesap yoktur. "Başarılı giriş hem gerçekleşip hem kalıcı oturum kuramaz" durumu oluşmaz; yasak kalktığında giriş çerez yazar. İki doğrulayıcı da çürüttü; ikisi de bunun en fazla "§4.22.2 bağımlılığı anmıyor" biçiminde bir ifade eksiği olduğunu yazdı — ölçünün sınıfı değil. | — |
| F2 | Parça 3 · BULGU-1 (kalite döngüsü notu; §1.1.6 `04` 9.25.5; §1.1.7 `04` 6.2.28.3 — UC-187) | ❌ RET | **Eksik iddia yanlış; alıntı 1. turun değişiklik notundan.** Alıntı başlık notundaki 1. tur kaydıdır (*"UC-187'de ele alınmamıştı — UC-76'nın 422 yolu eklendi"*): **önceki** hâli ve yapılan eklemeyi anlatır. UC-187'nin 422 kolu v0.18'den beri yazılıdır: satır 6361 *"eski adres o arada doğrulanmış başka bir yönetici hesabının adresi oldu: geri alma uygulanmaz, bağlantının tüketimi geri döner ve sayfa 9.25.5'in hâlini gösterir (K-1210; K-1218; UC-76 ve UC-186'nın simetrisi)"*, satır 6362 *"422'de hiçbir şey yazılmaz"*, hata tablosu satır 5217 `EmailTakenByOtherAccount` için UC-76'yla birlikte UC-187'yi sayar. Saldırı senaryosu da tutmaz: tüketim geri döndüğü için bağlantı kullanılabilir kalır, hesabın e-postası değişmez, kurtarma yolu kapanmaz (9.25.5 başka yönetici ya da kurulum yolunu gösterir). 1. turun F1'inin (kabul) aynı yere işlenmiş hâlidir; model işlenmiş düzeltmeyi başlık notunun cümlesinden "eksik" diye okudu. İki doğrulayıcı da çürüttü. | — |
| F3 | Parça 4 · BULGU-1 (§1.1.10 `03 §6.1.1.9`; §1.1.9 `03 §3.4.14`) | ❌ RET (**tekrar eden RET** — 2. tur F3, 3. tur F1) | **İki ayrı olay; çelişki yok.** `03 §6.1.1.9` (`03` satır 1100) limit (L-9) isteği **engellediğinde** konuşur: *"İstek engellenir; kayıt silinmez, önceki bağlantı ömrü içinde geçerli kalır"* — yeni bağlantı üretilmediği için eskisi kalır. `03 §3.4.14` (satır 827), `§7.1.49` (satır 1277), `§9.1.3` (satır 1631) ve `06` (satır 844) başarılı yeni isteği anlatır: öncekiler geçersizleşir, ömür uzamaz. `04` 7.3.3.9 (satır 4642) aynı bağlamda L-9'un engel davranışını söyler. Aynı bağlantının hem kabul hem reddedilmesi oluşmaz. Yeni gerekçe yok; iki doğrulayıcı da çürüttü ve ikisi de matris özet satırlarının (`07` 2292 ve 2892: *"önceki bağlantı geçerli kalır"*) "engellendiğinde" niteliğini taşımadığını — yüzeyde çelişki gibi göründüğünü — okunabilirlik meselesi diye not etti. Matris satırının kaynağa uygunluğu ölçünün dışındadır (istem: matris satırının özetini denetleme); gövde tek ve tutarlıdır. | — |
| F4 | Parça 4 · BULGU-2 (§1.1.9 `03 §2.3`, `03 §4.1.6`; §1.1.12 `05 §6.7.8`) | ❌ RET | **Z-6 "boşalma" kuralıdır, ömür kuralı değil; çelişki dokümanın kendi gövdesinde çözülü.** `02` Z-6 (satır 1121): *"Kendiliğinden boşalmaz: misafirin sepeti tarayıcının verileri silinene, üyeninki hesap durdukça yaşar"*. `05 §6.7.8` (satır 777) çelişkiyi açıkça ele alır: misafir çerezi 400 gün açılmazsa sunucudaki sepet silinir — *"sepete artık hiçbir tarayıcı ulaşamaz"* — ve Z-6'nın "tarayıcının verileri silinene kadar" kuralı *"tarayıcının sınırıyla karşılanır"*; 400 gün çerez ömrünün teknik tavanıdır (`05 §6.7.5`), silme eşiği bu sınırın karşılığıdır. Kalıcılığın dayanağı bilinçli karardır (K-950; `05` satır 777), `06` (satır 5936, 2290) ve `05` 11.1.32 (satır 1376) aynı eşiği taşır; `07` bu üst dokümanları yalnız eşler (satır 2515, 2786, 3327). Üye sepeti hesapta durur, süresizlik korunur. Para ya da hak kaybı yok: silinen, 400 gündür hiçbir tarayıcının açamadığı misafir sepetidir. Ölçünün sınıfı yok. İki doğrulayıcı da çürüttü. | — |
| F5 | Parça 6 · BULGU-1 (§4.29.1 UC-30) | ❌ RET | **Bilinçli ertelenmiş karar; iki yöntem de envanterde yazılı.** UC-30 envanter satırı (satır 3913) *"GET `/odeme/donus/{erişim anahtarı}` — sağlayıcı POST'la dönüyorsa aynı adreste POST; yöntemi `08`"* der ve POST için *"form · 64 KB"* sütununu taşır; §4.29.1 (satır 4820) aynı koşullu ifadeyi taşır ve POST'u sahteciliğe karşı korumanın dışında bırakır; hata tablosu (satır 5130) *"korumanın dışındaki uçlar (§2.5.5) hariç"* der. Sağlayıcının seçimi `05 §3.6.4` ile `08`'e bırakılmıştır; `05 §6.5.1` *"ödeme dönüşü POST'la geliyorsa ikinci istisna"* der; `08` park satırı A4-3 yolu ve yöntemi *"bu aşamanın ve `07`'nin kararı"* diye bırakır (satır 3249, 3239, 3307). Yöntemi şimdi tek yönteme kilitlemek verilmemiş bir sağlayıcı kararını önceden vermek olurdu; dönüşün gövde ve sorgu değerleri okunmadığı için (satır 5040) POST gelse de durum yazılmaz. Çelişen iki yer yok, ölçünün sınıfı yok. İki doğrulayıcı da çürüttü. | — |
| F6 | Parça 6 · BULGU-2 (§4 ortak yanıtlar; §4.34.1 UC-77) | ❌ RET | **İstisna §2.3.2'nin son cümlesinde yazılı; alıntı onu atlamış.** §2.3.2 (satır 3694) oturumu kapanmış POST'u girişe yönlendirir ve **hemen ardından** der: *"Çıkış (UC-77, UC-109) bu kuralın dışındadır: oturumsuz çıkış isteği yazmaz ve çıkışın yönlendirmesini alır."* Ortak yanıtlar cümlesi (satır 4137) kuralı `(§2.3.2)` atfıyla verir; istisna aynı bölümdedir. UC-77 (satır 4904–4911): *"tekrar gönderim oturumu bulamaz — aynı yönlendirmeyi alır, hiçbir şey yazılmaz"* — "aynı yönlendirme" çıkışın kendi `returnUrl` yönlendirmesidir. Bulgunun önerdiği "çıkış için açık bir §2 istisnası" zaten vardır. Genel kural ile onun istisnasıdır, iki yer çelişmez; kullanıcı çıkışsız kalmaz. 3. turun F6'sıyla (§2.7.1) aynı kalıp — genel cümle istisnanın evine atıf yapar. İki doğrulayıcı da çürüttü. | — |

**Dağılım:** altı ayrı bulgu (altı ham bulgu) — 0 KABUL · 0 KISMİ · 6 RET. **%100 ret şüphelidir:** bu yüzden her bulgu ikinci bir temiz bağlamlı doğrulayıcıdan geçti ve yönetici ajan doğrulayıcıların dayandığı satırları (§2.3.2, §4.22.2–3, UC-187, UC-30, `05 §6.7.8`, `05 §6.7.10`, `02` Z-6) bizzat okudu; retlerin hepsi dokümanın yazılı kararına, ilgili cümlenin atlanan devamına ya da üst dokümanın yazılı mekanizmasına dayanır. Önceki üç turun dökümü 1. tur 4 kabul · 1 kısmi · 2 ret, 2. tur 0 · 1 · 7, 3. tur 0 · 1 · 6; bu tur ilk kez kısmi de çıkmadı. **Örüntü:** altı bulgunun beşi bölümün atlanan cümlesine ya da tam metne bakmayan parçalı okumadan (F1: kapı yapısı, F2: değişiklik notunun cümlesi, F5: ertelenmiş karar, F6: §2.3.2'nin son cümlesi, F4: `05 §6.7.8`'in çözümü) doğan okuma farklarıdır; biri (F3) üçüncü kez dönen iddiadır. **Sınıf:** kabul edilen bulgu yok; yeni yüzey açılmadı. Faz 2 hukuki kontrolü: hiçbir bulgu yasal dayanak anmadı.

## 4. Ek bulgular

- **Aynı kalıbın taraması.** Kabul yok; taranacak kalıp yok. Doğrulayıcıların not ettiği iki okunabilirlik ayrıntısı (`07` 2292 ve 2892'deki matris özetlerinin "engellendiğinde" niteliğini taşımaması; §4'ün ortak yanıt cümlesinin çıkış istisnasını anmaması) ölçünün sınıfı değildir ve uygulanmadı.
- **Mekanik tarama.** `07`'nin içeriği değişmedi: kümeler ve çiftler (5.073 · 2.319 · 187), hata kodu sayıları ve uç sayısı aynıdır; yalnız başlık sürümü, başlık notuna eklenen 4. tur kaydı ve dosya sonu dipnotu v0.21'e gitti.
- **Devredilen not (F3 — checkpoint), 1. turdan açık.** `05 §7.7.4` ve K-884 hakkın "düğmeye basıldığında **ve** sunucu dosyayı göndermeye başladığında" düştüğünü söyler; `07` §4.16.3 ve §4.31.1 tek adımı — UC-36'nın düğmesini — anlatır. Düşük şiddetlidir, `07`'nin bulgusu değildir; `05` cümlesine "bağlantı üretildiği an" eklemek istenirse Aşama 6'nın checkpoint'inde yapılır. Bu turda da yeniden gelmedi; açık not olarak taşınır ve etki sütununa checkpoint'te bağlanır.
- **Devredilen not — §2.7.1'in genel cümlesi (3. turun F6'sı), açık.** *"Sürüm çakışması ve gösterilen değerin tutmaması 409"* cümlesi tek başına okunduğunda kabadır (geçici kapatma anahtarı ve müşterinin kalem işlemleri 303 alır); ölçünün sınıfında değildir. Bu turun F6'sı aynı kalıptır (genel cümle istisnanın evine atıf yapar). Açık not olarak taşınır.
- **Yeni devredilen not — matrisin L-9 özet satırları (F3).** `07` 2292 (§1.1.9 `04` 7.3.3.9) ve 2892 (§1.1.10 `03 §6.1.1.9`) *"önceki bağlantı geçerli kalır"* diye özetler; kaynak cümle limit **engelinde** konuşur. Üç turda dönen bu iddia hep matris özetidir ve istem matris özetlerinin kaynağa uygunluğunu denetletmez; gövde tektir. Düzeltmesi istenirse ("(engellenen istekte)") checkpoint'in iç tutarlılık merceği "özet cümlesi gövdenin niteliğini taşımıyor" sınıfını dokümanın tamamında arar; açık not olarak taşınır.
- **Ciddi bir sorun daha görülmedi.** Değerlendirme sırasında ikinci modelin listesi dışında ölçünün dört sınıfından birine giren bir sorun saptanmadı.

## 5. Kullanıcı onay checklist'i

K-1144 kararıyla (⚠ öneriyle kayıt, `docs:` PR'ında CI yeşilse merge yetkisi, yönetici + alt ajan düzeni) alt ajan değerlendirdi (`07` v0.21 — içerik değişmedi); K-1207 gereği Claude alt ajanları yalnız Sonnet / low koştu. Bir karar öneriyle kaydedildi, ⚠ değildir: **K-1222** — 4. turun sonucu, ilk "tekrar eden RET" kaydı ve ciddiyet ölçüsünün uygulanış teyidi. ⚠ listesine ekleme yok.

- [x] F1 (ret — kapalı kapılar; yasağın sürdüğü dönemde giriş yapacak hesap yoktur)
- [x] F2 (ret — alıntı 1. turun değişiklik notu; UC-187'nin 422 kolu v0.18'den beri yazılı)
- [x] F3 (ret — iki ayrı olay; **tekrar eden RET**, üçüncü ret)
- [x] F4 (ret — Z-6 boşalma kuralı; 400 gün tarayıcı sınırının karşılığı, `05 §6.7.8`)
- [x] F5 (ret — dönüş yöntemi `08`'e bilinçle bırakılmış; iki yöntem envanterde)
- [x] F6 (ret — çıkış istisnası §2.3.2'nin son cümlesinde)
- [x] 4. tur — koştu (yukarıda, "Tur 4" bölümü); TEMİZ değil
- [x] 5. tur — koştu (aşağıda, "Tur 5" bölümü); 0 kabul, döngü çıkış kuralıyla kapandı. (Eski not: 4. turda hiçbir bulgu kabul edilmedi; TEMİZ aranır. Aynı istem, aynı betikler, aynı parça sınırları; Birleşim 1 ve 2 ile sekiz parça. **5. turdan itibaren çıkış kuralı işler** (skill Faz 4 madde 5): kabul edilen bulguların hepsi düşük şiddette metin farkıysa ya da hiç yoksa, bulgular uygulanır ve döngü o turda "TEMİZ — çıkış kuralıyla, 5 tur" diye kapanır; bir ret dördüncü kez gelirse yine "tekrar eden RET"tir)
- [ ] Etki yansıtma (Faz 5) — döngü TEMİZ döndükten sonra, son turun raporunda

---
# Cross-Review — 07 API Design (Tur 5)
**Tarih:** 2026-10-28 (tur 2026-10-19'da başladı; Codex kullanım sınırına takıldı ve 2026-10-28'de yeniden koştu) · **Hedef:** `Docs/07_API_DESIGN.md` v0.21 (girdi commit'i `966c497`, 1 192 666 bayt, 6 405 satır) · **İkinci model:** Codex CLI 0.153.0 — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Ciddiyet ölçüsü:** 1. turdan beri istemde (K-1219) · **Koşum:** 1.–4. turla aynı iki biçim — **iki birleşim** ve **sekiz parçalı kontrol koşumu**, aynı istem, aynı parça notu, aynı şablon (SHA-256 ilk 16 hane: istem `76e57ac0e4326f90`, parça notu `2b150c8107b170bb`, şablon `dfc8aa759c1e01c5` — 4. turla aynı; doküman v0.21 `018a9cb78bae6f4d`), betikler yeniden kuruldu (aşağıda), parça sınırları başlıkla (K-1219) · **Tek parça çağrı:** yeniden denenmedi — girdi 1 097 066 karakter, sınır 1 048 576 (1.–4. turda aynı hatayla döndü; yönetici talimatı) · **Girdi:** yalnız doküman ve boş şablonu, her çağrıda boş ve izole çalışma klasöründe (K-431) · **Kullanım sınırı:** 2026-10-19 05:47'de bu turun on çağrısı reddedildi (Codex token 0; hiçbiri çıktı vermedi); 2026-10-28'de sınır açıktı ve on çağrının hepsi çıktı verdi; yedeğe geçilmedi, tur tek modelle koştu · **Sonuç:** TEMİZ değil ham olarak — iki ham bulgu (ikisi ayrı), 0 KABUL · 0 KISMİ · 2 RET; **çıkış kuralı işler: döngü "TEMİZ — çıkış kuralıyla, 5 tur" diye kapanır** (skill Faz 4 madde 5; K-1223); doküman içeriği değişmedi (`07` v0.22 yalnız sürüm ve başlık notu) · **Değerlendirme modeli:** Claude alt ajanları yalnız Sonnet / low (K-1207); Codex dışarıdadır

## 1. Koşum

**Betikler yeniden kuruldu.** 1. turun scratchpad klasörü (`cr07\`) ve 3. turunki (`cr07r3\`) silinmişti; 1. turun §1'indeki tarife göre aynıları yeniden yazıldı: `C:\Users\TEKNOD~1\AppData\Local\Temp\claude\c--projects-Shopfolio\422e2f0a-46a6-4ced-809c-e90f0f447f55\scratchpad\cr07r5\` — `build_inputs.py` (girdileri kurar: istemi ve parça notunu **bu raporun 1. tur bölümündeki alıntı bloklarından** `> ` önekini atarak okur; başlık notunu, sekiz parçayı ve iki birleşimi başlıkla kurar; kod çiti içi başlık sayılmaz; kapsanmayan satırları basar), `run_codex.sh` (çağrıları koşar; en çok altısı paralel; çıkış kodu, süre, token ve `web search` satırı `summary.tsv`'ye), `template07.md` (`git show e74883f:Docs/07_API_DESIGN.md`). **Yeniden kurulan girdilerin 4. turla aynılığı:** istem SHA-256 `76e57ac0e4326f90` (1.–4. turla aynı), parça notu `2b150c8107b170bb` (aynı), şablon `dfc8aa759c1e01c5` (aynı); parça başlıkları aynı sekiz başlık (`## 1. Traceability Matrix` · `#### 1.1.3 ` · `#### 1.1.6 ` · `#### 1.1.9 ` · `## 2. Konvansiyonlar` · `## 4. Endpoint tanımları` · `## 5. DTO envanteri` · `## 9. Yönetim (admin) endpoint`); Birleşim 1 = parça 2–8, Birleşim 2 = parça 1 ve 3–8; kapsanmayan satır yoktur (betiğin denetimi: `none`); başlık notu 41 602 karakter. Parça girdi boyutları 4. turdan başlık notunun v0.21'de ~1 000 bayt büyümesi kadar farklıdır (ör. parça 1: 136 834 → 137 834 bayt) ve satır numaraları iki satır kaymıştır (parça 1: 113 → 115).

**Komut** (Git Bash; çalışma klasörü boş): `codex exec -s read-only -C <boş klasör> --skip-git-repo-check --ephemeral --ignore-user-config --color never -m gpt-5.6-terra -c 'model_reasoning_effort="high"' -o <çıktı> - < <girdi>`; çağrılar altı + dört biçiminde koştu (en çok altısı paralel).

**Parçalar** (sınırlar 1. turdakiyle aynı başlıklar; satır numaraları v0.21'indir):

| Parça | Bölümler | Satırlar | Girdi | Token | Süre | Sonuç |
|---|---|---|---|---|---|---|
| 1 | §1'in girişi · §1.1.1–§1.1.2 | 115–642 | 137 834 bayt | 63 587 | 76 sn | `SONUÇ: TEMİZ` |
| 2 | §1.1.3–§1.1.5 | 643–1246 | 154 218 bayt | 64 055 | 47 sn | `SONUÇ: TEMİZ` |
| 3 | §1.1.6–§1.1.8 | 1247–2300 | 240 563 bayt | 99 628 | 40 sn | 1 bulgu |
| 4 | §1.1.9–§1.3 | 2301–3616 | 328 757 bayt | 129 989 | 55 sn | `SONUÇ: TEMİZ` |
| 5 | §2 · §3 | 3617–4123 | 160 942 bayt | 71 047 | 101 sn | `SONUÇ: TEMİZ` |
| 6 | §4 | 4124–4919 | 190 313 bayt | 82 298 | 98 sn | `SONUÇ: TEMİZ` |
| 7 | §5 · §6 · §7 · §8 | 4920–5415 | 132 381 bayt | 51 568 | 43 sn | 1 bulgu |
| 8 | §9 · §10 | 5416–6405 | 233 285 bayt | 97 729 | 105 sn | `SONUÇ: TEMİZ` |

| Çağrı | Parçalar | Girdi | Karakter | Token | Süre | Sonuç |
|---|---|---|---|---|---|---|
| Birleşim 1 | 2 · 3 · 4 · 5 · 6 · 7 · 8 | 1 116 903 bayt | 1 019 823 | 413 908 | 151 sn | `SONUÇ: TEMİZ` |
| Birleşim 2 | 1 · 3 · 4 · 5 · 6 · 7 · 8 | 1 100 519 bayt | 1 006 354 | 408 449 | 48 sn | `SONUÇ: TEMİZ` |

**Kapsam sınırı ve web.** Parça öteki bölümleri görmez; birleşimler parçalar arası okumayı yapar (yalnız matrisin parça 1'i ile parça 2'si buluşmaz — K-1219). Yönetici değerlendirmesi her iddiayı dokümanın tamamına ve ilgili yerde `02` v0.92, `03` v0.39, `04` v0.39, `05` v0.28, `06` v0.30'a karşı okudu. **Web araması:** on çağrının hiçbirinde `web search` satırı yok; hiçbir bulgu yasal dayanak anmadı — Faz 2 madde 5 (hukuki kontrol) konusuz. **Toplam:** on Codex çağrısı, 1 482 258 token (CLI'nin bildirdiği).

## 2. Ham bulgular

İkinci modelin çıktıları, satır sonu boşlukları dışında düzenlenmeden (alıntı blokları içinde).

**Birleşim 1** (parça 2–8):
> SONUÇ: TEMİZ

**Birleşim 2** (parça 1, 3–8):
> SONUÇ: TEMİZ

**Parça 1** (§1'in girişi, §1.1.1–§1.1.2):
> SONUÇ: TEMİZ

**Parça 2** (§1.1.3–§1.1.5):
> SONUÇ: TEMİZ

**Parça 3** (§1.1.6–§1.1.8):
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §1.1.7, `04` 6.3.9 ve 6.3.9.9
> Alıntı: "her durumda açık üç işlem — iç not, e-postayı yeniden göndermek, misafir e-postasının düzeltmesi"
> Sorun: Bu ortak kural, e-posta yeniden gönderimini ve misafir e-postası düzeltmesini tüm E-37 durumlarında ve sipariş türlerinde açık gösterir. Oysa aynı verilen metinde e-posta yeniden gönderimi yalnız `"e-posta ulaşmadı"` işaretindeki satırlardan yapılır (`04` 9.9.4, 9.9.33); e-posta düzeltmesi ise yalnız girişsiz/misafir siparişlerde mümkündür (`04` 9.9.3, 9.9.38). Bu, dokümanın kendi içinde çelişkidir (sınıf 4) ve yanlış yetki/aksiyon uygulanmasına yol açabilir.
> Öneri: §1.1.7’deki ortak kuralı koşullu yazın: iç not her durumda; yeniden gönderim yalnız ulaşmayan e-posta kaydı varsa; e-posta düzeltmesi yalnız misafir siparişlerde açık olmalıdır. İlgili 6.3.9.9 satırını da bu koşullarla hizalayın.
>
> SONUÇ: 1 BULGU

**Parça 4** (§1.1.9–§1.3):
> SONUÇ: TEMİZ

**Parça 5** (§2 · §3):
> SONUÇ: TEMİZ

**Parça 6** (§4):
> SONUÇ: TEMİZ

**Parça 7** (§5–§8):
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §8.2.2 — Çift gönderimin engellenmesi
> Alıntı: "güncel hâlle karşılaştırılmayan işlemlerde (sepete ekleme, iletişim talebi, hesapta adres ekleme, indirme bağlantısı, panelin yeni kayıt formları, dışa aktarma — §2.5.1) tekrar gönderim ikinci kez işlenir, çifte karşı koruma gönder–yönlendir–göster'dir"
> Sorun: Gönder–yönlendir–göster, ilk yanıt gelmeden gönderilen paralel/tekrarlı POST isteklerini engellemez. UC-36’da iki istek de işlenirse iki indirme bağlantısı üretilebilir ve indirme hakkı iki kez düşebilir. Bu, dijital ürün alıcısının ücretini ödediği indirme hakkını kaybetmesine yol açar (sınıf 2).
> Öneri: UC-36 için form gösteriminde tekil bir istek anahtarı üretin; ilk POST bağlantıyı ve hak düşümünü atomik biçimde oluştursun, aynı anahtarla gelen tekrarlar aynı bağlantıyı döndürsün ve ek hak düşürmesin.
>
> SONUÇ: 1 BULGU

**Parça 8** (§9 · §10):
> SONUÇ: TEMİZ

## 3. Bağımsız değerlendirme

Her bulgu dokümanın tamamına (v0.21) ve ilgili yerde `02` v0.92, `03` v0.39, `04` v0.39, `05` v0.28, `06` v0.30'a karşı okundu. **Koşum (K-1207):** her bulguyu önce bir Sonnet / low okudu; ikisini de "çürütüldü" dedi (şiddet Düşük, sınıf yok) — kural gereği (çürütülen bulgular ikinci bakışa gider) ikisi de ikinci, ayrı, temiz bağlamlı bir Sonnet / low'dan geçti; ikisi iki bulguda da ayrışmadı. Yönetici (adım) ajan doğrulayıcıların dayandığı satırları da gördü: `07` 1950, 1959, 4440, 4855, 5407; `04` 6.3.9 (4045), 6.3.9.9 (4057), 9.9.38 (4920). **Önceki turlardan dönen iddia:** yok — F1 4. turun matris özeti notuyla (L-9 satırları) aynı kalıptan (özet hücresi niteleyiciyi taşımıyor) ama farklı yer ve farklı iddiadır; F2 1. turun UC-36 ret kaydına (F3) komşu konudur ama farklı iddiadır (paralel POST); ikisi de "tekrar eden RET" değildir.

| # | Çağrı · bulgu | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|---|
| F1 | Parça 3 · BULGU-1 (§1.1.7, `04` 6.3.9 ve 6.3.9.9 — E-37 "her durumda açık üç işlem") | ❌ RET | **Matris özeti; kaynak ve gövde koşulu taşır.** Alıntı `07` 1950'deki matris özet hücresidir (istem: matris satırının özetinin kaynağa uygunluğunu denetleme). Kaynak `04` 6.3.9 (4045) işlemleri koşullu yazar: iç not imhaya kadar, *"işaretli satırda"* e-postayı yeniden göndermek, *"girişsiz verilmiş siparişte"* misafir e-postasını düzeltmek; 6.3.9.9 (4057) *"ortak üç işlem (blok girişi)"* diyerek bu girişe atıf yapar; 9.9.38 (4920) koşulları açıkça yazar. `07` gövdesi de koşulludur: UC-153 ve UC-154 satır/işaret bazlı, UC-151 misafir siparişi için, `03 §1.11.36` *"'e-posta ulaşmadı' işaretliyken"* der; matrisin UC eşlemesindeki `konvansiyon — API8-01` koşullu işlem kuralını işaretler. Matris ile gövde çelişmez; "her durumda açık" `04`'te "durum satırlarında yinelenmez" demektir. Kusur en fazla ifadedir (özet niteleyiciyi atlar) ve bulgunun kendi önerisi "ortak kuralı koşullu yazın"dır — ifade tercihi, ölçünün sınıfı değil. İki doğrulayıcı da çürüttü (şiddet Düşük). | — (not olarak taşınır, §4) |
| F2 | Parça 7 · BULGU-1 (§8.2.2 — gönder–yönlendir–göster paralel POST'u engellemez; UC-36 iki indirme bağlantısı üretir) | ❌ RET | **Bilinçli karar; kalan risk yazılı.** UC-36'nın gövdesi (4440) bunu açıkça söyler: *"her gönderim yeni bir indirmedir ve bir hak düşürür — düğmeye her basış bir haktır (`05 §7.7.4`; K-1179 (1)); çift tıklamayı betikli yolda düğmenin kilidi önler."* §8.2.2 (5407) UC-36'yı tekrar gönderimin ikinci kez işlendiği sınıfa açıkça koyar; ikisi aynı şeyi söyler, çelişki yok. Tek koşullu güncelleme (K-980) sayacın aşılmasını önler, iki isteği bire indirmek için konmamıştır; betiksiz yolda eşzamanlı ikinci gönderim §4.31.1'de *kalan risk* diye adlandırılmıştır (4855) ve hak firmanın yenilemesiyle geri verilir (`02 §3.12.6`). Kayıp en fazla bir indirme hakkıdır, geri alınabilir; mevzuata aykırılık ve çıkışsız akış yok. Önerilen tekil istek anahtarı `05 §7.7.4`'ün mekanizmasını değiştirir (savunmada derinlik). İki doğrulayıcı da çürüttü (şiddet Düşük). | — |

**Dağılım:** iki ayrı bulgu (iki ham bulgu) — 0 KABUL · 0 KISMİ · 2 RET. **%100 ret şüphelidir:** bu yüzden iki bulgu da ikinci, temiz bağlamlı bir doğrulayıcıdan geçti ve yönetici ajan doğrulayıcıların dayandığı satırları okudu; iki ret de dokümanın yazılı kararına ya da üst dokümanın yazılı kuralına dayanır (F1: kaynak ve gövdenin koşulu; F2: UC-36'nın bilinçli kararı ve kalan riski). Önceki turların dökümü 1. tur 4 kabul · 1 kısmi · 2 ret, 2. tur 0 · 1 · 7, 3. tur 0 · 1 · 6, 4. tur 0 · 0 · 6; bu tur 0 · 0 · 2 — üst üste ikinci tur kabulsüz ve yeni bulgu sayısı 8 → 7 → 6 → 2. **Örüntü:** iki bulgunun biri matris özetinin niteleyiciyi taşımadığı (F1), biri bilinçli kararla yazılı kalan riskin yeniden yazımı (F2) — ikisi de ölçünün sınıfı dışındadır. **Sınıf:** kabul edilen bulgu yok; yeni yüzey açılmadı. Faz 2 hukuki kontrolü: hiçbir bulgu yasal dayanak anmadı.

## 4. Ek bulgular

- **Aynı kalıbın taraması.** Kabul yok; uygulanacak düzeltme yok. F1'in kalıbı (matris özet hücresi kaynağın niteleyicisini atlıyor) 4. turun L-9 notuyla aynı sınıftır; üç yer artık aynı açık nota bağlıdır (aşağıda).
- **Mekanik tarama.** `07`'nin içeriği değişmedi: kümeler ve çiftler (5.073 · 2.319 · 187), hata kodu sayıları ve uç sayısı aynıdır; yalnız başlık sürümü, başlık notuna eklenen 5. tur kaydı ve dosya sonu dipnotu v0.22'ye gitti.
- **Devredilen notlar — checkpoint'e (cross-review kapanışında üç açık not, karar kaydı §16.1'e taşındı).** (1) `05 §7.7.4` / K-884: hakkın düştüğü an — kaynak "düğmeye basıldığında **ve** sunucu dosyayı göndermeye başladığında" der, `07` §4.16.3 ve §4.31.1 tek adımı (UC-36'nın düğmesi) anlatır; 1. turdan, düşük. (2) §2.7.1'in genel *"Sürüm çakışması ve gösterilen değerin tutmaması 409"* cümlesi tek başına okunduğunda kabadır (geçici kapatma anahtarı ve müşterinin kalem işlemleri 303 alır); 3. turdan, düşük. (3) Matrisin özet satırları gövdenin niteleyicisini taşımıyor: `07` 2292 (§1.1.9 `04` 7.3.3.9) ve 2892 (§1.1.10 `03 §6.1.1.9`) *"önceki bağlantı geçerli kalır"* (kaynak limit **engelinde** konuşur — 4. turdan), **ve 5. turdan: `07` 1950 ve 1959 (E-37 / `04` 6.3.9 ve 6.3.9.9) *"her durumda açık üç işlem"* (kaynak e-posta yeniden gönderimini işarete, e-posta düzeltmesini girişsiz siparişe bağlar)**; üç tur ve dört yerde dönen sınıf; gövdeler tek ve tutarlıdır; düşük şiddetli. Checkpoint'in iç tutarlılık merceği "özet cümlesi gövdenin niteleyicisini taşımıyor" sınıfını dokümanın tamamında arar.
- **Ciddi bir sorun daha görülmedi.** Değerlendirme sırasında ikinci modelin listesi dışında ölçünün dört sınıfından birine giren bir sorun saptanmadı.

## 5. Kapanış özeti — döngü "TEMİZ — çıkış kuralıyla, 5 tur" (K-1223)

- **Çıkış kuralı (skill Faz 4 madde 5):** 5. tur beşincidir; kabul edilen bulgu yoktur (0 KABUL · 0 KISMİ) — kuralın koşulu ("kabul edilenlerin hepsi düşük şiddette iki yerin metin farkı ya da kabul yok") sağlanır; orta ya da yüksek kabul yoktur; mevzuata aykırılık, para ya da hak kaybı, çıkışsız akış yoktur. İki ret bilinçli kararlara ve yazılı kuralara dayanır; üst üste iki tur kabulsüz.
- **Sayılar (5 tur):** 30 ayrı bulgu — **4 KABUL · 3 KISMİ · 23 RET** (1. tur 7: 4·1·2 · 2. tur 8: 0·1·7 · 3. tur 7: 0·1·6 · 4. tur 6: 0·0·6 · 5. tur 2: 0·0·2). Kabul ve kısmi sürümler `07` v0.18 (1. tur) · v0.19 (2. tur) · v0.20 (3. tur) olarak işlendi; v0.21 ve v0.22 yalnız sürüm ve başlık notudur.
- **Codex çağrıları:** 51 çağrı çıktı verdi (1. tur 11 — biri girdi sınırında hemen döndü — ve 2.–5. turlarda 10'ar çağrı: iki birleşim + sekiz parça) ve 2026-10-19'da reddedilen 10 çağrı (Codex token 0); toplam yaklaşık **7,2 milyon bildirilen Codex token** (1. tur 1 462 827 · 2. tur ~1,42 M · 3. tur ~1,4 M · 4. tur 1 409 692 · 5. tur 1 482 258). Tek parça çağrı hiçbir turda koşamadı (girdi sınırı, K-1219). Claude alt ajanları yalnız Sonnet / low (K-1207): 5. turda 4 (2 ilk, 2 ikinci doğrulayıcı).
- **Kalite döngüsünün ardından:** etki yansıtma (Faz 5; `07` üzerinden upstream/downstream, yeni alan/kural, Kaynak satırları ve açık notlar taraması) ayrı adımda koşar; sonra checkpoint. Açık notlar yukarıda (§4) üç madde olarak checkpoint'e devredildi.

## 6. Kullanıcı onay checklist'i

K-1144 kararıyla (⚠ öneriyle kayıt, `docs:` PR'ında CI yeşilse merge yetkisi, yönetici + alt ajan düzeni) alt ajan değerlendirdi (`07` v0.22 — içerik değişmedi); K-1207 gereği Claude alt ajanları yalnız Sonnet / low koştu. Bir karar öneriyle kaydedildi, ⚠ değildir: **K-1223** — 5. turun sonucu ve cross-review'ın çıkış kuralıyla kapanışı. ⚠ listesine ekleme yok.

- [x] F1 (ret — matris özeti; kaynak ve gövde koşulu taşır)
- [x] F2 (ret — UC-36'nın her basış bir hak kararı ve yazılı kalan risk)
- [x] 5. tur — koştu; TEMİZ değil ham olarak ama çıkış kuralı koşulları sağlandı
- [x] Cross-review döngüsü kapandı — "TEMİZ — çıkış kuralıyla, 5 tur" (K-1223)
- [ ] Etki yansıtma (Faz 5) — ayrı adımda; sıradaki adım
