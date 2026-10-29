# Cross-Review — 08 Integration Spec (Tur 1)

**Tarih:** 2026-10-29 · **Hedef:** `Docs/08_INTEGRATION_SPEC.md` v0.9 (girdi commit'i `5245931`, 285 824 bayt, 949 satır) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Ciddiyet ölçüsü:** 1. turdan itibaren uygulanır, mekanizma okuma paragrafıyla (K-1280; ayrıntısı başka dokümanlarda yaşayan, ürün kuralı koymayan türetim dokümanı — `cross-review` skill'i, "Ciddiyet ölçüsü") · **Koşum:** iki koşum — tek parça çağrı ve altı parçalı kontrol koşumu (doküman 400 KB'ın altındadır; 1. tur ölçer) · **Sonuç:** **TEMİZ DEĞİL** — altı ayrı bulgu; beşi kısmen kabul edilip uygulandı, biri reddedildi.

## 1. Koşum

**Betikler** (sonraki turlar aynılarını kullanır; depoda değil, adım ajanının scratchpad'inde): `C:\Users\TEKNOD~1\AppData\Local\Temp\claude\c--projects-Shopfolio\ab472d0d-cc27-40b7-9288-b725735c9991\scratchpad\cr08\` — `build_inputs.py` (girdileri kurar: `python -I build_inputs.py <doküman> template08.md prompt.txt partnote.txt <girdi klasörü>`; parça sınırları betiğin `BOUNDS` listesinde, başlık önekiyle; kapsanmayan satırları basar — bu turda `none`), `run_codex.sh` (çağrıları paralel koşar: `bash run_codex.sh <girdi> <çıktı> c1 p1 p2 p3 p4 p5 p6`), `prompt.txt`, `partnote.txt`, `template08.md` (şablon: `git show e74883f:Docs/08_INTEGRATION_SPEC.md`, playbook'tan gelen ilk hâl, 2 618 bayt).

**İstem.** API Tasarımı turunun istemi (`07_CROSS_REVIEW.md` §1) bu dokümana uyarlandı: yedi kriter, bulgu biçimi (`BULGU-N` — Kriter · Seviye · Yer · Alıntı · Sorun · Öneri — ya da `SONUÇ: TEMİZ`), `cross-review` skill'inin Faz 1 madde 4'teki bilinçli karar cümlesi değiştirilmeden, ciddiyet ölçüsünün paragrafı ve skill'in "mekanizma kararı koyan dokümanda okuma paragrafı" bu dokümanın mekanizmalarına (imza, bildirimin kabulü, tekilleştirme, yeniden deneme, bağlantı hatası ayrımı, gönderim sonucu sınıfı, dış varsayımın bağlanışı) uyarlanmış hâliyle. Girdi yalnız doküman ve şablondur (Faz 1 madde 2); karar kaydı ve önceki turların değerlendirmesi verilmedi.

> ```text
> Sen bağımsız bir doküman denetçisisin. Aşağıda "=== DOKÜMAN ===" işaretinden sonra bir entegrasyon spesifikasyonları (integration specifications) dokümanının metni, "=== ŞABLON ===" işaretinden sonra o dokümanın boş şablonu var. Başka bir dosya okuma; çalışma klasörü boştur.
>
> Bağlam: Belge bir entegrasyon spesifikasyonları dokümanıdır. Ürün kuralı koymaz (başlık notu, yazım konvansiyonu 1): ürünün dış servislerle konuşma biçimini — uç, kimlik doğrulama, alan eşlemesi, hata, yeniden deneme, tekilleştirme, imza, çökme davranışı — ve e-postaların metnini ve şablonunu yazar. Ürün davranışı ayrı bir Ürün Gereksinimleri dokümanında (`02`), akışlar `03`'te, ekranların davranışı `04`'te, mekanizma ayrı bir Teknik Mimari dokümanında (`05`), alan ve değer adları ayrı bir Veri Modeli dokümanında (`06`), uç envanteri ve adresler ayrı bir API Tasarımı dokümanında (`07`) durur; hiçbiri sana verilmedi. Bu doküman kendi kararlarını koyar — ödeme sağlayıcısının seçimi, e-posta altyapısının seçimi, dış servislerin istek, imza ve yanıt biçimine ilişkin eşlemeler, gelen bildirimin kabulü, tekilleştirme, bağlantı hatası ayrımı, e-posta şablon metinleri, dış varsayım tablosu. Dış servislerin belgelerinden okunan olgular "bilgim tarihli" yazılır ve §4'te bir dış varsayım satırına (DV) bağlanır; implementation'da yeniden doğrulanır. Kural ayrıntısının burada yeniden yazılmamış olması ya da bir kurala yalnız bölüm numarası ve kimlikle (`02 §…`, `03` …, `04 §…`, `05 §…`, `06 §…`, `07 §…`, UC-nn, ÖK-n, YH-n) işaret edilmesi tek başına bulgu değildir; o dokümanların içeriğini denetleme. Dokümandaki `K-123` gibi işaretler sana verilmeyen bir karar kaydına gider. Yazım konvansiyonları başlık notundadır ("Yazım konvansiyonları", 1–12). Proje Aşama 7'dedir (entegrasyon spesifikasyonları); kod henüz yazılmadı; veritabanı PostgreSQL 18, çatı ASP.NET Core Razor Pages ve EF Core. Ürün, Türkiye'de tüketiciye satış yapan tek firmalık bir e-ticaret + kurumsal sitedir. Yasal iddiaları (mesafeli satış, KVKK, ödeme hizmetleri, e-ticaret mevzuatı) yürürlükteki resmî metne göre değerlendir.
>
> Dokümanı yedi kriterle denetle: tutarlılık · eksiklik · belirsizlik · teknik doğruluk · edge case · güvenlik · kullanıcı deneyimi.
>
> Dokümanda açıkça bilinçli karar, elenen seçenek ya da kabul edilmiş risk olarak yazılmış bir seçimi, yalnız seçime katılmadığın için bulgu yapma. Böyle bir karar dokümanın kendi içinde çelişki, olgusal ya da hukuki hata doğuruyorsa yaz.
>
> Yalnız CİDDİ sorunları yaz: (1) yürürlükteki mevzuata aykırılık, (2) müşteriye ya da firmaya para kaybı veya hak kaybı doğuran kural boşluğu, (3) kullanıcının takılıp kaldığı, çıkışı olmayan bir akış, (4) dokümanın iki yerinin birbiriyle çelişmesi. Nadir kenar durumları, iyileştirme önerilerini, savunmada derinlik önerilerini, üslup ve ifade tercihlerini bulgu yapma. Böyle bir sorun yoksa SONUÇ: TEMİZ yaz.
>
> Teknik doğruluk ve güvenlik bu dört sınıfın içinde okunur: yazılan bir mekanizmanın — imza doğrulaması, bildirimin kabulü, tekilleştirme, yeniden deneme, bağlantı hatası ayrımı, e-posta gönderim sonucu sınıfı, bir dış varsayımın bağlanışı — dokümanın kendi koyduğu bir güvenceyi (ör. bir tahsilatın ya da iadenin tek kez uygulanması, bir bildirimin yalnız imzası doğruysa işlenmesi, kart verisinin ürüne girmemesi, bir sonucun tek bir sınıfa karşılık gelmesi) sağlamaması (4)'tür; bir mekanizmanın siparişin, paranın ya da kişisel verinin kaybına veya kişisel verinin yetkisiz kişiye açılmasına yol açması (1) ya da (2)'dir. Ek bir koruma katmanı, ek izleme, daha güçlü bir araç ya da algoritma önerisi savunmada derinliktir ve bulgu değildir.
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

Parçalı çağrıların istemine tek bir paragraf eklenir (bilinçli karar cümlesi ve ölçü değişmez; `<bölümler>` aşağıdaki tablonun adlarıdır):

> ```text
> Parça notu: Doküman bu çağrıda parça parça verilmektedir. Aşağıdaki metin dokümanın başlık notu (sürüm, park edilen girdiler, yazım durumu, yazım konvansiyonları, alt bölüm haritası) ile <bölümler> bölümlerinden oluşur; öteki bölümler bu çağrıda verilmedi. Verilmeyen bir bölüme yapılan atıf ve orada yaşayan içeriğin burada görünmemesi tek başına bulgu değildir; yalnız verilen metnin içindeki sorunları yaz.
> ```

**Komut** (Git Bash; çalışma klasörü boş): `codex exec -s read-only -C <boş klasör> --skip-git-repo-check --ephemeral --ignore-user-config --color never -m gpt-5.6-terra -c 'model_reasoning_effort="high"' -o <çıktı> - < <girdi>`. Yedi çağrı paralel koştu; kullanım sınırı dolmadı, çıktısız dönen çağrı olmadı.

**Parça sınırları — kayıt (K-1280).** Sınır satır numarasıyla değil **başlıkla** tanımlıdır ve sonraki turlarda değişmez. **Başlık notu** dokümanın ilk satırından `## 0. Yazım kuralları` başlığına kadardır (sürüm, beş park bloğu, yazım durumu, on iki yazım konvansiyonu, alt bölüm haritası; satır 1–106, yaklaşık 40 KB); her parça başlık notunu ve şablonu taşır.

| Parça | Başlar | Biter (dahil değil) | Bölümler | Satırlar | Girdi | Token | Süre | Sonuç |
|---|---|---|---|---|---|---|---|---|
| 1 | `## 0. ` | `#### 2.1.5 ` | §0 · §1 · §2.1 (2.1.1–2.1.4) | 107–245 | 95 591 bayt | 56 447 | 59 sn | `SONUÇ: TEMİZ` |
| 2 | `#### 2.1.5 ` | `### 2.2 ` | §2.1 (2.1.5–2.1.10) | 246–350 | 78 558 bayt | 59 461 | 217 sn | 1 bulgu |
| 3 | `### 2.2 ` | `#### 2.2.6 ` | §2.2 (2.2.1–2.2.5) | 351–470 | 89 472 bayt | 100 970 | 238 sn | 1 bulgu |
| 4 | `#### 2.2.6 ` | `### 2.3 ` | §2.2 (2.2.6–2.2.8: B-1…B-16, F-1…F-6, hesap ve yönetim e-postaları) | 471–677 | 91 748 bayt | 77 673 | 82 sn | `SONUÇ: TEMİZ` |
| 5 | `### 2.3 ` | `## 4. ` | §2.3 · §2.4 · §3 | 678–824 | 85 168 bayt | 61 184 | 152 sn | 1 bulgu |
| 6 | `## 4. ` | dosya sonu | §4 · §5 | 825–949 | 90 304 bayt | 64 883 | 182 sn | 1 bulgu |

Parça girdileri 79–96 KB'tır; kapsanmayan satır yoktur (betiğin denetimi: `uncovered lines: none`).

**(a) Tek parça çağrı.** Girdi 292 829 bayt (268 628 karakter; aracın 1 048 576 karakterlik sınırının çok altında), 133 428 token, 308 sn: 4 bulgu. Tek parça girdi sınırına takılmadığı için birleşim çağrısı gerekmedi.

**Kapsam sınırı.** Parça öteki bölümleri görmez; tek parça çağrı parçalar arası okumayı yapar. Değerlendirme her iddiayı dokümanın tamamına ve ilgili yerde Ürün Gereksinimleri'ne (`02`), Teknik Mimari'ye (`05`), Veri Modeli'ne (`06`) ve API Tasarımı'na (`07`) karşı okudu (§3). **Web araması:** ikinci modelin çağrılarından yalnız parça 6'nın çıktısı bir resmî kaynak bağlantısı (Ticaret Bakanlığı tüketici sayfası) taşıdı; yasal iddiaların hiçbiri ikinci modelin kaynağına dayanılarak kabul edilmedi — her biri doğrulayıcıda resmî metne (Resmî Gazete) karşı okundu (§3, okuma tarihi 2026-10-29).

**İki koşumun ölçümü (skill, Faz 1 madde 9).** Parçalı koşum tek parça çağrının görmediği iki bulguyu getirdi ve ikisi de kabul edildi (F5 — parça 3; F6 — parça 5); tek parça çağrı parçaların görmediği bir bulgu getirmedi, ama dört bulgunun üçünü (F1, F2, F3) tek başına buldu. Karar: iki koşum sonraki turlarda da sürer.

## 2. Ham bulgular

İkinci modelin çıktıları, satır sonu boşlukları dışında düzenlenmeden (alıntı blokları içinde).

**(a) Tek parça çağrı:**
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §2.1.5 “İadeye etkisi”; DV-10; §5.2 satır 15
> Alıntı: “Dönmüyorsa caymada ve iptalde farkı müşteri taşır.”
> Sorun: Bu, müşteriden tahsil edilen `total_amount` ile iade edilen `Order.Total` arasındaki taksit farkını tüketici üzerinde bırakır. Mesafeli Sözleşmeler Yönetmeliği’nde cayma iadelerinin kullanılan ödeme aracına uygun, tüketiciye masraf yüklemeden ve tek seferde yapılması gerekir (mevcut düzenleme m.12/2’dir; dokümandaki m.13/2 atfı da yanlıştır). Bu, (1) mevzuata aykırılık ve (2) müşterinin para kaybıdır.
> Öneri: Kart hattını DV-10 doğrulanana kadar canlıya açmayın. Tam iadenin müşteriden tahsil edilen tutarın tamamını aynı ödeme aracına iade ettiğini sağlayıcıdan doğrulayın; sağlamıyorsa farkı da kapsayan yasal iade yolunu tasarlayın ve atfı m.12/2 olarak düzeltin.
>
> BULGU-2
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §2.1.9 “İstek” alan eşlemesi
> Alıntı: “`status` = `success` | `RefundRecord.State` | **başarılı** (`Succeeded`) — `OutcomeAt` yazılır; yanıtın ‘talep alındı’ mı ‘iade tamamlandı’ mı anlamına geldiği DV-7’dedir”
> Sorun: Anlamı doğrulanmamış `success` yanıtı nihai iade sonucu kabul edilerek sorgu yolu kapanıyor ve B-8 bildirimi tetiklenebiliyor. Yanıt yalnız isteğin alındığını ifade ederse, sonradan başarısız olan iade müşteri ve firma tarafından görülmeden “gerçekleşti” sayılır. Bu, (2) para kaybına yol açar.
> Öneri: DV-7 doğrulanana kadar `success` yanıtını `OutcomePending` kabul edin ve `returns` ile tamamlanma doğrulanana kadar sorguyu sürdürün. Alternatif olarak, DV-7’yi kart hattının canlıya alınmasını engelleyen zorunlu kapı yapın.
>
> BULGU-3
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: Yazım durumu / §4.1 “Tablonun sayımı”
> Alıntı: “§4 dış varsayımlar — **37 satırlık DV tablosu**”; “**Tablonun sayımı.** 42 satır”
> Sorun: Dış-varsayım envanterinin sayısı dokümanın iki yerinde çelişiyor. Bu, ön-uçuş kontrollerinde hangi doğrulama kapılarının zorunlu olduğunun yanlış anlaşılmasına neden olur. Bu, (4) doküman içi çelişkidir.
> Öneri: Yazım durumu ve ilgili özetleri 42 satıra göre güncelleyin; sayı ve dağılımı §4.1 ile birebir hizalayın.
>
> BULGU-4
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §5.2 giriş paragrafı ve satır 14
> Alıntı: “her satır ilgili dokümanda aynı PR’da işlendi”; “Bekliyor — etki yansıtması”
> Sorun: Tablo, tüm geri beslemelerin işlendiğini söylerken UC-30’un 200 geçiş sayfası, CSP ve arayüz davranışını etkileyen satır 14’ü beklemede bırakıyor. Bu, §2.1.4’teki dönüş davranışı ile `07`/`05`/`04` için bildirilen durumun eşzamanlı olarak farklı kalmasına yol açar. Bu, (4) doküman içi çelişkidir.
> Öneri: Etki yansıtmasını tamamlayıp ilgili satırları “işlendi” olarak güncelleyin; tamamlanmayacaksa giriş paragrafındaki “her satır” iddiasını düzeltin.
>
> SONUÇ: 4 BULGU

**Parça 1** (§0 · §1 · §2.1'in 2.1.1–2.1.4'ü):
> SONUÇ: TEMİZ

**Parça 2** (§2.1'in 2.1.5–2.1.10'u):
> BULGU-1
> Kriter: teknik doğruluk
> Seviye: Yüksek
> Yer: §2.1.5 — Tutar karşılaştırması
> Alıntı: “Dönmüyorsa caymada ve iptalde farkı müşteri taşır.”
> Sorun: Taksit farkı nedeniyle müşteriden fiilen tahsil edilen `total_amount` ile `Order.Total` arasındaki farkın iade edilmemesi açıkça kabul ediliyor. Mesafeli Sözleşmeler Yönetmeliği m.12/4, tüm geri ödemelerin tüketicinin kullandığı ödeme aracına uygun, tek seferde ve tüketiciye masraf veya yükümlülük getirmeden yapılmasını zorunlu kılar. Metindeki m.13/2 atfı da bu kuralın maddesi değildir. Bu, tüketicinin eksik iade almasına yol açan mevzuat aykırılığıdır.
> Öneri: Cayma ve iptal iadelerinde müşteriden tahsil edilen toplamın tamamını iade edilecek tutar olarak tasarlayın; PayTR/banka taksit farkını otomatik iade etmiyorsa farkı firma karşılasın veya bu davranış doğrulanana dek taksitli ödeme seçeneğini açmayın. Hukuki atfı m.12/4 olarak düzeltin.
>
> SONUÇ: 1 BULGU

**Parça 3** (§2.2'nin 2.2.1–2.2.5'i):
> BULGU-1
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: §2.2.4 — Aşım / Kalan risk
> Alıntı: “Eşik dolduktan sonra aynı adrese oluşan müşteri bildirimi gönderilmez”
> Sorun: İlk B-1 de sınıra tabidir; hedefli susturma senaryosunda B-1’in “sözleşme metni” ile birlikte hiç gönderilmemesi açıkça kabul edilmiştir. Alternatif bir teyit kanalı tanımlanmamıştır. Bu, kendine ait elektronik ortamda satış yapan hizmet sağlayıcının siparişi hem işlem ortamı üzerinden hem de e-posta/SMS/telefon gibi ek bir araçla gecikmeksizin teyit etmesini zorunlu kılan Elektronik Ticaret Aracı Hizmet Sağlayıcı ve Elektronik Ticaret Hizmet Sağlayıcılar Hakkında Yönetmelik m.9/1’e aykırıdır. Hukuki teyidin hiç yapılmaması, tüketici ve firma için hak kaybı riski doğurur.
> Öneri: İlk B-1’i sayaçtan muaf tutun veya sınır aşıldığında B-1 teyidini gecikmeksizin gönderilecek tanımlı bir alternatif kanalla sağlayın; ödeme/sipariş akışının bu teyit yükümlülüğüyle ilişkisini açıkça yazın.
>
> SONUÇ: 1 BULGU

**Parça 4** (§2.2'nin 2.2.6–2.2.8'i):
> SONUÇ: TEMİZ

**Parça 5** (§2.3 · §2.4 · §3):
> BULGU-1
> Kriter: Güvenlik
> Seviye: Yüksek
> Yer: §2.3, Alan eşlemesi — `sub`, `iss`, `aud`, `exp`, `iat`
> Alıntı: "Bir adres başka bir Google hesabına geçerse [...] o hesap aynı e-postalı mağaza hesabına girer [...] Ürün bunu ayırt etmez;"
> Sorun: Google hesabını kalıcı `sub` yerine e-postayla eşlemek, yeniden atanmış kurumsal veya alan-adı e-postasının yeni sahibine mevcut müşterinin hesabını açar. Doküman bunun şifreli hesaplarda da geçerli olduğunu söylüyor. Bu, müşteri hesabındaki kişisel verilerin yetkisiz kişiye açılmasıdır (1). Kabul edilmiş risk olarak yazılması bu doğrudan hesap ele geçirme sonucunu ortadan kaldırmaz.
> Öneri: Google bağını `iss` + `sub` ile kalıcı olarak saklayın ve Google girişinde mevcut bağla eşleştirin. E-posta eşleşmesini yalnız ilk bağlama/hesap kurtarma için, mevcut hesabın kanıtlanmış sahipliğini gerektiren ayrı bir akışta kullanın. Bu veri modeli ve ürün kuralı değişikliği olduğundan karar kaydına açılıp `06` ve ilgili üst dokümanlara geri yansıtılmalıdır.
>
> SONUÇ: 1 BULGU

**Parça 6** (§4 · §5):
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §5.2, satır 15
> Alıntı: "Tam iadede fark müşteriye sağlayıcı ya da banka eliyle dönmüyorsa (DV-10) caymada ve iptalde müşteri masraf taşır (Mesafeli Sözleşmeler Yönetmeliği m.13/2)"
> Sorun: Atıf yanlıştır ve müşteriye taksit farkı yükleyen bir sonuç öngörür. Yönetmelik m.13/2, cayma süresindeki uygun kullanımdan doğan değişim/bozulmaya ilişkindir; taksit farkı veya iade masrafı için dayanak değildir. Caymada satıcı, tahsil edilen tüm ödemeleri tüketiciye masraf yüklemeden ve tek seferde iade etmelidir. Bu ifade müşterinin parasal hak kaybına ve mevzuata aykırı iadeye yol açar. [Ticaret Bakanlığı açıklaması](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/mesafeli-sozlesmeler-hakkinda-bilgilendirme)
> Öneri: “Müşteri masraf taşır” hükmünü kaldırın. DV-10 sonucu, tüketiciden tahsil edilen toplam ile ürünün sağlayıcıya gönderdiği iade tutarı arasındaki farkı ortaya çıkarırsa, iadenin tüketiciye masrafsız ve eksiksiz ulaşmasını sağlayacak kuralı karar kaydı üzerinden `02`’ye taşıyın; teknik eşlemeyi buna göre güncelleyin.
>
> SONUÇ: 1 BULGU

## 3. Bağımsız değerlendirme

Yedi ham bulgu altı ayrı bulguya indi: F1 üç çağrıdan (tek parça B-1, parça 2 B-1, parça 6 B-1), öteki beşi birer çağrıdan geldi. Her ayrı bulgu bir `[dogrulama]` Opus / high doğrulayıcıya verildi (altı doğrulayıcı); yasal dayanaklı bulgular (F1, F5, F6) resmî metne karşı okundu (okuma tarihi 2026-10-29). Hiçbiri tümden KABUL değildir: beşinde sorun gerçektir ama ikinci modelin önerisi ya da gerekçesi uygun değildir (KISMİ); biri RET'tir. %100 KABUL/%100 RET tablosu görünmüyor; beş KISMİ'nin her birinin kısmi kalış nedeni aşağıda ayrıdır.

| # | Bulgu (kaynak) | Karar | Gerekçe | Uygulama |
|---|---|---|---|---|
| F1 | Taksit farkı: "Dönmüyorsa caymada ve iptalde farkı müşteri taşır" (tek parça B-1, parça 2 B-1, parça 6 B-1; §2.1.5, DV-10, §5.2 satır 15) | ⚠️ KISMİ | **Gerçek kısım:** "m.13/2" atfı yanlış — m.13/2 olağan kullanımdan doğan bozulmayı düzenler; masrafsız ve tek seferde geri ödeme m.12/4'tür (m.12, 23.08.2022 değişikliğiyle yeniden yazıldı; 2025 değişikliği m.12/4 ve m.12/5'i düzeltti; 1.1.2026'dan beri uygulanır; firma iptalinde m.16/4). `02 §7.4.10` m.13/2'yi doğru konuda kullanıyor; `08` hem ona hem resmî metne aykırıydı. "Kalan risk" adlandırması da yanlış: firmanın yasal yükümlülüğüne dokunan açık konu. **Kısmi kalan:** hükmün kendisi bilinçli yazılmıştı (K-164, K-1255 (4), K-1270 (10)) ve doğru yere — DV-10'a ve `02`'ye geri beslemeye — bağlıydı; ikinci modelin "kart hattını DV-10'a kadar açmayın" önerisi bir ürün kararıdır, bu turda alınmadı. Yönetmelik taksit ve vade farkını anmaz; yorum hukukçu teyidi ister. | K-1275 ⚠: §2.1.5 ve §5.2 satır 15 yeniden yazıldı, DV-10'a m.12/4 bağı |
| F2 | `success` iade yanıtı nihai sayılıyor (tek parça B-2; §2.1.9 alan eşlemesi) | ⚠️ KISMİ | **Gerçek kısım:** DV-7 satırı kapı olarak eksikti (beklenen sonuç, "yanlış çıkarsa" hükmü, süre yoktu) ve iki güvence cümlesi (geç sonuç; üç doğrulama noktası (b)) açık bıraktığı şeyi kesin olgu gibi söylüyordu — (4). **Reddedilen kısım:** `success`'i `OutcomePending` saymak koruma sağlamaz (sorgu yolu da aynı belirsizliği taşır, PayTR iadenin reddini sorguda göstermiyor) ve her kart iadesini bekleyen işe düşürür (`05 §4.6.2`, K-1238 (5)); önerinin kalanı savunmada derinliktir. | K-1276: DV-7 yeniden yazıldı; §2.1.9'un üç yerine DV-7 bağı |
| F3 | DV sayısı 37 ve 42 (tek parça B-3; başlık notu ↔ §4.1) | ⚠️ KISMİ | §4.1 tablosu betikle sayıldı: 42 satır (DV-1…DV-42), dağılım "Tablonun sayımı"yla birebir. "37 satırlık DV tablosu" v0.6 oturum kaydıdır (git: v0.6/v0.7'de 37, v0.8'de 38, v0.9'da 42) — gerçek bir çelişki değil, ama 38'e ve 42'ye çıkış başlık notunda yazılı değildi. Bulgunun "yazım durumu paragrafı 37 diyor" kısmı yanlış: 37 yalnız 5. oturum paragrafındadır. Aynı sınıfta bir yan bulgu: §2.1.1'in PayTR'ye özgü DV listesi DV-39…DV-42'yi saymıyordu. | K-1277: köprü cümlesi, §2.1.1 listesi; `12` park bloğundaki sayı 43 (DV-43 ile) |
| F4 | §5.2 "her satır işlendi" ↔ satır 14 "Bekliyor" (tek parça B-4) | ❌ RET | Giriş cümlesi satır 14'ü noktalı virgülden sonra açıkça istisna tutuyor ("satır 14 etki yansıtmayı bekler (K-1259 (3))"); ikinci model bu istisna cümlesini atlamış. Hücre bayat değil: `07` UC-30/§4.29.1, `05 §6.5.4` ve `04` hâlâ eski çerçeve davranışını söylüyor; bekleyiş K-1259 (3), K-1274, §2.1.4 madde 3 ve başlık notunda kasıtlı yazılı. Yan gözlem (EK-1): giriş cümlesinin "her satır … sürümü arttı" genellemesi park blokları, `DEPLOY_RUNBOOK`, "değişmedi" satırları ve bekleyen ikinci satır (15) için tutmuyordu. | Yan gözlem K-1277'de uygulandı; bulgu uygulanmadı |
| F5 | Alıcı sayacına düşen ilk B-1 ↔ Hizmet Sağlayıcılar Yönetmeliği m.9/1 (parça 3 B-1; §2.2.4 kalan risk) | ⚠️ KISMİ | **Gerçek kısım:** ilk B-1 sayaçtan muaf değil ve kasten susturmada hiç gitmeyebilir; karar gerekçelerinde (K-1242 (1), K-1248 (3), K-1271) m.9/1 tartılmamıştı — projenin K-704 kaydı m.9/1'in iki kanalı birlikte istediğini yazar, iç tutarsızlık. m.9/1 (RG 29.12.2022/32058, okundu 2026-10-29): kendi sitesinde sipariş alan firma siparişi aldığını işlem ortamında **ve ayrıca** e-posta, SMS ya da telefon gibi bir araçla gecikmeksizin bildirir; ikinci kol firma e-postayı panelden yeniden gönderene kadar karşılanmaz. **Kısmi kalan:** m.9/1 sözleşme metnini değil siparişin alındığının bildirimini ister (ilk kol — sipariş sayfası, `02 §3.17.1` — karşılanır); "alternatif kanal tanımsız" kısmen yanlış (yeniden gönderim sınırdan muaf); sözleşme kurulmuş sayılır, hak kaybı kurulamaz — en fazla (2). Öneri "ilk B-1'i muaf tut" K-1271'in elemesini geri açar; bu yüzden firmanın yeniden gönderme yükümlülüğü yazıldı. | K-1278 ⚠: §2.2.4, `02 §12.5` yeni satır (`02` v0.98), §5.2 satır 17 |
| F6 | Google bağı `sub` yerine e-postayla (parça 5 B-1; §2.3 alan eşlemesi) | ⚠️ KISMİ | **Reddedilen kısım:** karar bilinçli (K-104, K-900, K-1061, K-1272 (6) ⚠), dokümanlar arası çelişki yok (`06` `sub` sütunu taşımaz; `05 §6.1.4`, MK-46 aynı); aynı itiraz `05` cross-review'unun 2.–9. turlarında geldi ve reddedildi; `sub` saklamak riski kapatmaz (bağ ilk girişte e-postayla kurulur ve adresin yeni sahibi "şifremi unuttum"la da girer — `02 §8.1.2`). **Kabul edilen kısım (ters yön):** Google "Verify the Google ID token" sayfasına (son güncelleme 2025-12-22, okundu 2026-10-29) göre yalnız `@gmail.com` ve `hd` taşıyan hesaplar için adresin sahibi olarak yetkilidir; diğerlerinde `email_verified` açılış anındaki doğrulamayı gösterir ve eski sahip, posta kutusunun şimdiki sahibinin hesabına şifre sıfırlamayla giremeyeceği hâlde girebilir. Doküman bu yönü yazmıyor, `02 §3.13.7`'nin "adresin sahibi olmayan biri onu kuramaz" cümlesi bu hâlde Google'ın kendi belgesiyle tutmuyor. Karar bir ürün kuralıdır (08 kural koymaz) | K-1279 ⚠: kalan risk yazıldı (§2.3), DV-43, `02 §3.13.7`, §8.1.2; çok kritik soru açık süreç maddesine (§18.1) |

## 4. Ek bulgular

- **EK-1 (§5.2'nin giriş cümlesi):** F4'ün doğrulayıcısı gördü; K-1277'de uygulandı.
- **EK-2 (karar kaydı izleme satırı):** `PRODUCT_DISCOVERY_STATUS.md` §18.3'ün ENT4 satırı "37 DV satırı — audit DV-38'i ekledi, 38" derken deep review'un DV-39…DV-42'sini saymıyordu; bu PR'da 43'e tamamlandı. Aynı satırda `02`'nin durum tablosu da v0.96'da kalmıştı (v0.97 yazılmamıştı); v0.97 ve v0.98 eklendi.

## 5. Kullanıcı onay checklist'i

- [x] F1 — taksit farkının iadesi yasal yükümlülüktür, kalan risk değildir (K-1275 ⚠; öneriyle kaydedildi)
- [x] F2 — DV-7 kapı olarak yeniden kuruldu (K-1276)
- [x] F3 — DV sayısı anlatımı (K-1277)
- [ ] F4 — RET (uygulanmadı); yan gözlem uygulandı
- [x] F5 — ulaşmayan B-1'in yeniden gönderimi firmanın yükümlülüğü (K-1278 ⚠; öneriyle kaydedildi)
- [x] F6 — Google ters yön kalan risk olarak yazıldı (K-1279 ⚠; öneriyle kaydedildi) — **çok kritik soru proje sahibine açık** (karar kaydı §18.1)

## Sonuç

**SONUÇ: TEMİZ DEĞİL — 6 ayrı bulgu (7 ham), 5 kısmi kabul, 1 ret.** Skill'in çıkış kuralı: bütün çağrılar aynı anda TEMİZ dönmedi ve 5. tur eşiği (beşinci turdan itibaren çıkış kuralı) henüz gelmedi; kabul edilen bulgular arasında mevzuat ve para/hak kaybı sınıfından olanlar (F1, F5) var. Sonraki tur: 2. tur (`08_CROSS_REVIEW_R2.md`), aynı betikler ve aynı parça sınırlarıyla, `08` v0.10 üzerinde. Etki yansıtma (Faz 5) döngü TEMİZ olunca ayrı adımdır; bu turda yapılmadı.
