# Cross-Review — 06 Data Model (Tur 1)

**Tarih:** 2026-10-18 · **Hedef:** `Docs/06_DATA_MODEL.md` v0.18 (girdi commit'i `a6432c9`, 1 158 135 bayt, 6 011 satır) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Ciddiyet ölçüsü:** 1. turdan itibaren uygulanır, mekanizma okuma paragrafıyla (K-1125) · **Koşum:** iki biçimde — (a) tek parça çağrı modelin girdi sınırını aştı (1 048 576 karakter; girdi 1 067 118 karakter) ve yerine **en büyük iki birleşim** koştu; (b) **yedi parçalı kontrol koşumu**, parça sınırları başlıkla sabit (K-1126); turun sonucu ikisinin birleşimidir · **Girdi:** yalnız doküman ve boş şablonu (`e74883f` — project-playbook'tan gelen v0.1, 98 satır, olduğu gibi), stdin'den tek metin, her çağrıda boş ve izole bir çalışma klasöründe; karar kaydı, Ürün Gereksinimleri, Kullanıcı Akışları, Arayüz Tanımları, Teknik Mimari, MVP Kapsamı ve audit/deep review raporları verilmedi (K-431) · **Sonuç:** TEMİZ değil — yedi ayrı bulgu, 1 KABUL · 3 KISMİ · 3 RET; dördü uygulandı (`06` v0.19)

## 1. Koşum

**Betikler** (sonraki turlar aynılarını kullanır; depoda değil, yönetici oturumunun scratchpad'inde): `C:\Users\TEKNOD~1\AppData\Local\Temp\claude\c--projects-Shopfolio\3a694994-cfb4-4b7e-a92e-eea67b22c07d\scratchpad\cr1\` — `build_inputs.py` (girdileri kurar: `python -I build_inputs.py <doküman> <şablon> prompt.txt partnote.txt <girdi klasörü>`; parça ve birleşim sınırları betiğin `PARTS` ve `COMBOS` listelerinde, başlık önekiyle; kapsanmayan satırları da basar — bu turda `none`), `run_codex.sh` (çağrıları koşar: `bash run_codex.sh <girdi> <çıktı> [adlar…]`; en çok altısı paralel; her çağrının çıkış kodunu, süresini, token sayısını ve web araması satırlarını `summary.tsv`'ye yazar), `prompt.txt` ve `partnote.txt` (aşağıdaki iki alıntı bloğunun metni). Şablon: `git show e74883f:Docs/06_DATA_MODEL.md` (scratchpad'de `audit06/template06.md` — aynı). SHA-256 (ilk 16 hane): istem `7c31919b40b3209c`, parça notu `2b150c8107b170bb`, şablon `5d3e5023c4926bfe`, doküman (v0.18) `c1b8f540b293c4a1`. **2. tur aynı istemle, aynı parça notuyla ve aynı betiklerle koşar;** yalnız doküman güncellenir. Scratchpad kaybolursa betikler bu bölümden yeniden kurulur: istem ve parça notu aşağıdadır, sınırlar parça tablosundadır. Koşum öncesinde çağrı kısa bir deneme metniyle doğrulandı (oturum canlı).

**İstem.** Teknik Mimari'nin istemi (`05_CROSS_REVIEW.md` §1) veri modeli dokümanına uyarlandı: yedi kriter, bulgu biçimi (`BULGU-N` — Kriter · Seviye · Yer · Alıntı · Sorun · Öneri — ya da `SONUÇ: TEMİZ`), `cross-review` skill'inin Faz 1 madde 4'teki bilinçli karar cümlesi değiştirilmeden, ciddiyet ölçüsünün paragrafı ve skill'in "mekanizma kararı koyan dokümanda okuma paragrafı" (K-1125). Bağlam paragrafı dokümanın ürün kuralı koymadığını (konvansiyon 1), kuralın verilmeyen dokümanlarda yaşadığını, §1'in izlenebilirlik matrisi olduğunu — matris satırının özetinin verilmeyen kaynağa uygunluğu denetlenmez, matrisle dokümanın öteki bölümleri arasındaki çelişki ise dokümanın kendi içindedir — ve `07`, `08`, `09`'un işinin burada beklenmediğini söyler.

> ```text
> Sen bağımsız bir doküman denetçisisin. Aşağıda "=== DOKÜMAN ===" işaretinden sonra bir veri modeli (data model) dokümanının tam metni, "=== ŞABLON ===" işaretinden sonra o dokümanın boş şablonu var. Başka bir dosya okuma; çalışma klasörü boştur.
>
> Bağlam: Belge bir veri modeli dokümanıdır. Ürün kuralı koymaz (başlık notu, konvansiyon 1): ayrı bir Ürün Gereksinimleri dokümanının (`02`) kurallarının ve ayrı bir Kullanıcı Akışları dokümanının (`03`) akışlarının sakladığı bilgiyi entity'ye, alana, ilişkiye, enum'a ve kısıta çevirir; ayrı bir Teknik Mimari dokümanının (`05`) veri katmanı kararlarının veriye düşen yüzünü kurar. İş kurallarının ayrıntısı — süreler, limitler, durumlar, saklama süreleri, bildirimler — `02`'de, akış adımları `03`'te, ekranlar ayrı bir Arayüz Tanımları dokümanında (`04`), mekanizma `05`'te, kapsam ayrı bir MVP Kapsamı dokümanında (`10`) durur; hiçbiri sana verilmedi. Bu doküman kendi modelleme kararlarını koyar — silme sınıfları, anahtarlar, adlandırma, türler ve ölçek, eşzamanlılık, denormalizasyon, entity'lerin alanları, ilişkiler, kısıtlar, dizinler, başlangıç verisi, saklama ve imha. §1 izlenebilirlik matrisidir: satırları verilmeyen kaynak dokümanların maddelerini özetler ve entity/alana eşler; bir matris satırının özetinin verilmeyen kaynağa uygunluğunu denetleme — matrisle dokümanın öteki bölümleri arasındaki çelişki ise dokümanın kendi içindedir. Kural ayrıntısının burada yeniden yazılmamış olması ya da bir kurala yalnız bölüm numarası ve kimlikle (`02 §…`, `03` …, `04` …, `05 §…`, `10` KP-, Z-, L-, P-, H-, B-, F-, S-, Ö-, KD-, ÖK-, SK-) işaret edilmesi tek başına bulgu değildir; o dokümanların içeriğini denetleme. Endpoint ve istek biçimi, dış servislerin alan eşlemesi, sorgu ve kod kalıpları sonraki dokümanların (`07`, `08`, `09`) işidir; burada beklenmez. Dokümandaki `K-123` gibi işaretler sana verilmeyen bir karar kaydına gider. Yazım konvansiyonları başlık notundadır ("Yazım konvansiyonları", 1–14). Proje Aşama 5'tedir (veri modeli); kod henüz yazılmadı; veritabanı PostgreSQL 18, çatı EF Core. Ürün, Türkiye'de tüketiciye satış yapan tek firmalık bir e-ticaret + kurumsal site ürünüdür; her firma ürünü kendi kiraladığı tek bir sanal sunucuya kurar.
>
> Dokümanı yedi kriterle denetle: tutarlılık · eksiklik · belirsizlik · teknik doğruluk · edge case · güvenlik · kullanıcı deneyimi.
>
> Dokümanda açıkça bilinçli karar, elenen seçenek ya da kabul edilmiş risk olarak yazılmış bir seçimi, yalnız seçime katılmadığın için bulgu yapma. Böyle bir karar dokümanın kendi içinde çelişki, olgusal ya da hukuki hata doğuruyorsa yaz.
>
> Yalnız CİDDİ sorunları yaz: (1) yürürlükteki mevzuata aykırılık, (2) müşteriye ya da firmaya para kaybı veya hak kaybı doğuran kural boşluğu, (3) kullanıcının takılıp kaldığı, çıkışı olmayan bir akış, (4) dokümanın iki yerinin birbiriyle çelişmesi. Nadir kenar durumları, iyileştirme önerilerini, savunmada derinlik önerilerini, üslup ve ifade tercihlerini bulgu yapma. Böyle bir sorun yoksa SONUÇ: TEMİZ yaz.
>
> Teknik doğruluk ve güvenlik bu dört sınıfın içinde okunur: yazılan bir alanın, kısıtın, silme davranışının ya da saklama kuralının dokümanın kendi koyduğu bir güvenceyi (ör. bir hakkın ya da sayacın tek bir koşullu güncellemeyle düşmesi, sipariş tarafında veri kaybı olmaması, süresi dolan kişisel verinin silinmesi) sağlamaması (4)'tür; bir alanın, kısıtın ya da silme davranışının siparişin, paranın ya da kişisel verinin kaybına veya kişisel verinin yetkisiz kişiye açılmasına yol açması (1) ya da (2)'dir. Ek bir dizin, kısıt, koruma katmanı ya da izleme önerisi, daha güçlü bir tür, araç ya da algoritma önerisi savunmada derinliktir ve bulgu değildir.
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

Girdi `istem + [parça notu +] "=== DOKÜMAN ===" + metin + "=== ŞABLON ===" + şablon` biçiminde kurulur. Parçalı ve birleşik çağrıların istemine tek bir paragraf eklenir (bilinçli karar cümlesi ve ölçü değişmez; `<bölümler>` aşağıdaki tabloların adlarıdır):

> ```text
> Parça notu: Doküman bu çağrıda parça parça verilmektedir. Aşağıdaki metin dokümanın başlık notu (sürüm, park edilen girdiler, yazım durumu, yazım konvansiyonları, alt bölüm haritası) ile <bölümler> bölümlerinden oluşur; öteki bölümler bu çağrıda verilmedi. Verilmeyen bir bölüme yapılan atıf ve orada yaşayan içeriğin burada görünmemesi tek başına bulgu değildir; yalnız verilen metnin içindeki sorunları yaz.
> ```

**Komut** (Git Bash; çalışma klasörü boş): `codex exec -s read-only -C <boş klasör> --skip-git-repo-check --ephemeral --ignore-user-config --color never -m gpt-5.6-terra -c 'model_reasoning_effort="high"' -o <çıktı> - < <girdi>`.

**Parça sınırları — kayıt (K-1126).** Sınır satır numarasıyla değil **başlıkla** tanımlıdır ve sonraki turlarda değişmez (satır numaraları kayar). **Başlık notu** dokümanın ilk satırından `## 1. Traceability Matrix` başlığına kadardır (sürüm, dört park bloğu, yazım durumu, on dört yazım konvansiyonu, alt bölüm haritası; v0.18'de satır 1–99, 46 294 bayt) ve her parçanın başındadır. Her parça bir başlıkta başlar ve sonraki parçanın başlığından önce biter; betik kod çiti içindeki satırları başlık saymaz. Satır numaraları v0.18'indir.

| Parça | Başlar | Biter (dahil değil) | Bölümler | Satırlar | Girdi | Token | Süre | Sonuç |
|---|---|---|---|---|---|---|---|---|
| 1 | `## 1. Traceability Matrix` | `#### 1.1.5 ` | §1'in girişi · §1.1.1–§1.1.4 (kaynak envanteri ve sayım, aday entity listesi, MVP Kapsamı, Ürün Gereksinimleri §0–§2) | 100–723 | 156 456 bayt | 59 473 | 95 sn | 1 bulgu |
| 2 | `#### 1.1.5 ` | `#### 1.1.9 ` | §1.1.5–§1.1.8 (Ürün Gereksinimleri §3–§13) | 724–1748 | 248 722 bayt | 82 876 | 33 sn | `SONUÇ: TEMİZ` |
| 3 | `#### 1.1.9 ` | `## 2. Modelleme kuralları` | §1.1.9–§1.3 (Kullanıcı Akışları, devirler, GAP adayları, geri izlenebilirlik, boşluklar) | 1749–2592 | 307 191 bayt | 128 066 | 223 sn | 3 bulgu |
| 4 | `## 2. Modelleme kuralları` | `## 4. Entity tanımları` | §2 · §3 | 2593–2967 | 126 110 bayt | 60 519 | 169 sn | 1 bulgu |
| 5 | `## 4. Entity tanımları` | `### 4.36 ` | §4'ün girişi · §4.1–§4.35 | 2968–3877 | 258 887 bayt | 105 163 | 92 sn | `SONUÇ: TEMİZ` |
| 6 | `### 4.36 ` | `## 5. Enum tanımları` | §4.36–§4.72 | 3878–4648 | 205 919 bayt | 91 046 | 195 sn | 2 bulgu |
| 7 | `## 5. Enum tanımları` | dosya sonu | §5 · §6 · §7 · §8 · §9 | 4649–6011 | 193 602 bayt | 70 712 | 76 sn | `SONUÇ: TEMİZ` |

Parçalar deep review'ın önerisini (v0.18 sürüm notları; her parçaya başlık notu ve şablon) izler; kapsanmayan satır yoktur (betiğin denetimi). Parça girdileri 126–307 KB'tır — Teknik Mimari'nin tek parça çağrısının (357 KB) altında.

**(a) Tek parça çağrı — girdi sınırı ve yerine koşan birleşimler (K-1126).** Tek parça girdi (1 166 277 bayt) iki saniyede `input_too_large` ile döndü: *"Input exceeds the maximum length of 1048576 characters … actual_chars 1067118"*. Hata modelin bağlam sınırı değil, aracın girdi uzunluğu sınırıdır; doküman şablonsuz da sınırın üstündedir. Brifin yolu izlendi: (a)'nın yerine bölümler arası çelişkiyi görecek **en büyük birleşimler** koştu — sınıra sığan en büyük birleşim yedi parçanın altısıdır; ikisi birlikte her parça çiftini en az bir çağrıda buluşturur, yalnız matrisin parça 1'i ile parça 2–3'ü (matrisle matris) buluşmaz:

| Çağrı | Parçalar | Bölümler | Girdi | Karakter | Token | Süre | Sonuç |
|---|---|---|---|---|---|---|---|
| Birleşim 1 | 2 · 3 · 4 · 5 · 6 · 7 | §1.1.5–§9 (verilmeyen yalnız §1'in girişi ve §1.1.1–§1.1.4) | 1 065 460 bayt | 974 179 | 457 792 | 194 sn | 1 bulgu |
| Birleşim 2 | 1 · 4 · 5 · 6 · 7 | §1'in girişi, §1.1.1–§1.1.4 ve §2–§9 (verilmeyen yalnız §1.1.5–§1.3) | 720 922 bayt | 661 304 | 268 261 | 52 sn | 1 bulgu |

İki birleşim parça notunu taşır (verilmeyen bölüm adıyla). Birleşim 1'in bulgusu (§4.57 ile matrisin Z-33 özetleri) hiçbir parçanın tek başına göremediği bir iki-yer farkıdır — parça 3 matrisi, parça 6 §4.57'yi gördü ve ikisi de bulmadı; birleşim yolu bu turda iş gördü.

**Kapsam sınırı.** Parça öteki bölümleri görmez; birleşimler parçalar arası okumayı (a)'nın yerine yapar. Yönetici değerlendirmesi her iddiayı dokümanın tamamına ve ilgili yerde Ürün Gereksinimleri'ne (`02` v0.85) ve Kullanıcı Akışları'na (`03` v0.34) karşı okudu (§3). **Web araması:** `--ignore-user-config` aramayı kapatmıyor; parça 3 ve birleşim 1 web araması yaptı — ikisi de 6563 sayılı Kanun m.11/3'ü mevzuat.gov.tr'de aradı (koşum kayıtlarındaki `web search` satırları); öteki yedi çağrı aramadı. Hiçbir bulgu bir web kaynağına bağlantı vermedi; yasal dayanak anan tek bulgu (parça 6 · BULGU-2) aramayan çağrıdandır. Girdi kuralı (K-431) dokümanın dışındaki proje bilgisini yasaklar; kamuya açık mevzuat proje bilgisi değildir — aramanın açık olduğu yine de buraya yazılır.

## 2. Ham bulgular

İkinci modelin çıktıları, satır sonu boşlukları dışında düzenlenmeden (alıntı blokları içinde).

**(a) Tek parça çağrı:** koşmadı — girdi sınırı (§1). Yerine:

**Birleşim 1 — §1.1.5–§9:**

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §4.57 `ContactRequest.LastClosedAt`; §1.1.5 `02 §3.32.4`; §1.1.6 Z-33
> Alıntı: "yeniden açılıp açık duran talepte başlangıç son kapatılıştır, talep yeniden kapatılınca an yenilenir. **Sonucu:** yeniden açılmış talep açık durduğu sürece de bu anla silinir"
> Sorun: Matris, açık talebin saklama süresinin oluşturulma anından başladığını söylerken model, yeniden açılmış açık talebi önceki kapanış anına göre imha eder. Bu iç çelişkidir (4); açık bir sipariş veya KVKK talebinin işlenmeden silinerek hak kaybına yol açması mümkündür (2).
> Öneri: Saklama başlangıcını `ContactRequestStatus` ile belirleyin: açık talepte `CreatedAt`, kapatılmış talepte `LastClosedAt`. §9.1 ve §9.3’teki imha seçimini aynı kuralla güncelleyin.
>
> SONUÇ: 1 BULGU
> ```

**Birleşim 2 — §1'in girişi, §1.1.1–§1.1.4, §2–§9:**

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §4.40 `PendingEmailChange` / §4.42 `LinkRecord.ExpiresAt`
> Alıntı: “yeniden istenen bağlantı ilk bağlantının ömrünü taşır ve uzatmaz” / “yeni bağlantının ömrü kendi gönderiminden işler”
> Sorun: Aynı bekleyen e-posta değişikliği için yeniden gönderilen doğrulama bağlantısının süresi iki farklı biçimde tanımlanmış. Bu, saklama/silme zamanının ve kullanıcıya sunulan doğrulama akışının belirsiz kalmasına yol açar; dokümanın kendi içinde çelişkidir (4).
> Öneri: Tek davranışı seçin. Süre ilk istekten hesaplanacaksa ikinci ifadeyi kaldırın; her yeniden istekte yenilenecekse §4.40 ve §4.42’deki “ilk bağlantının ömrü” ifadelerini buna göre değiştirin.
>
> SONUÇ: 1 BULGU
> ```

**Parça 1 — başlık notu, §1'in girişi, §1.1.1–§1.1.4:**

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §1.1.4
> Alıntı: "**`02 §1.2` — terim sözlüğü (119 — 1a 105; 2b on dört terim ekledi: GAP-12 ve GAP-13, `02` v0.77)**"
> Sorun: §1.1.4 başlığı bu bölümü 167 kaynak birimi olarak sayar; alt başlıkların beyan ettiği sayılar ise 16 + 119 + 26 = 161’dir. Ayrıca §1.1.1 sayımı, workshop eklerinden sonra sözlüğün 125 terim olduğunu belirtir. Bu, izlenebilirlik matrisinin kaynak kapsamı ve toplam sayımıyla doğrudan çelişir (4).
> Öneri: Sözlük alt başlığını güncel 125 terim sayısına düzeltin; §1.1.4 alt toplamını 167 ile doğrulayın ve tablo satırlarının bu sayıyla bire bir eşlendiğini yeniden denetleyin.
>
> SONUÇ: 1 BULGU
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
> Yer: §1.1.9, `03 §1.2` S2/S10 ve §1.4.3 m1–m2
> Alıntı: "Alındı → Teslim edildi — kapanış kuralını tamamlayan son olay: ... son açık kalemin iptali, çıkarılması ya da cayma beyanı" / "Hiçbir kalem teslim edilmemişse İptal edildi: iptal ya da kapanış"
> Sorun: Aynı son açık kalemin iptal, çıkarma veya cayma ile kapanmasında siparişin `Delivered` mı `Cancelled` mı olacağı çelişiyor. Teslim edilmiş kalem yoksa iki farklı terminal durum mümkündür; bu, müşterinin sipariş durumu, bildirimleri ve geri ödeme/iptal işlemlerini yanlış hatta sokabilir (4).
> Öneri: S2 ve S10’u, ancak siparişte en az bir kalemin teslim işareti varsa `Delivered`a götürecek şekilde şartlandırın; teslim işareti yoksa kapanışı yalnız S3/S4/S9 üzerinden `Cancelled`a bağlayın.
>
> BULGU-2
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: §1.1.9, `03 §1.4.4.9` ve §1.8.2
> Alıntı: "ürünün kalıcı silinmesi durum değil, açık siparişler donmuş kalemleriyle sürer" / "Silinen varyantın ayırması düşer"
> Sorun: Açık, henüz ödenmemiş sipariş donmuş kalemiyle sürerken varyant silinince ona ait ayırmanın düşmesi öngörülüyor. Böylece bekleyen siparişin ayırdığı stok/kontenjan yeniden satılabilir; sonradan ödeme gelirse firma aynı mal veya hizmeti iki kez satmış olur ya da müşterinin siparişi karşılanamaz (2).
> Öneri: Etkin `StockReservation` bulunan varyantın kalıcı silinmesini engelleyin; silme gerekiyorsa ayırmayı, siparişin olağan iptal/ödeme sonucuna kadar donmuş kalem üzerinden koruyun.
>
> BULGU-3
> Kriter: Güvenlik
> Seviye: Yüksek
> Yer: §1.1.9, `03 §1.11.35`–§1.11.36; §1.3, K-1115
> Alıntı: "eski ve yeni adres, yönetici, an; `OrderAccessKey` yenilenir" / "yeniden gönderim ayrı satır, ilk alıcıya"
> Sorun: Misafir siparişi e-postası düzeltildiğinde erişim anahtarı yenilenirken, ulaşmayan müşteri bildirimlerinin yeniden gönderimi ilk—artık geçersiz ya da yanlış—adrese yöneliyor. Dokümanın kendi K-1115 kaydı da bunun yeni erişim anahtarını eski adrese götürebileceğini belirtir. Bu, sipariş verisi ve erişim anahtarının yetkisiz kişiye açılması riskidir (1).
> Öneri: Yeniden gönderimin alıcısını “ilk alıcı” yerine, bildirim türü için geçerli güncel sipariş iletişim adresi olarak tanımlayın; e-posta düzeltmesinden önceki erişim anahtarını içeren gönderimler yeniden gönderilemez olmalıdır.
>
> SONUÇ: 3 BULGU
> ```

**Parça 4 — başlık notu, §2–§3:**

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §2.2.5, §2.7.1, §3.1 — StockReservation
> Alıntı: “Basamaklı silme yalnız sahibiyle silinen bağlı kayıttadır: hesap → oturum, adres defteri, bağlantı kaydı, bekleyen e-posta değişikliği, tanınan tarayıcı, üyenin sepeti; yönetici daveti → bağlantı kaydı (§4.38); ürün → varyant, seçenek, seçenek değeri, fiyat geçmişi, indirim, ürün–kategori bağı, içerik–ürün bağı, galeri bağları; kurumsal içerik kaydı → içerik–ürün bağı, galeri bağları; varyant → stok ayırma; sepet → sepet kalemi (K-1022 (6), K-1026 (5), K-1029 (2), K-1015 (1); K-1062 (1); §4.16).”
> Sorun: Varyant silinince aktif stok/kupon ayırmaları geçiş birimi dışında basamaklı siliniyor; buna karşılık §2.7.1, ayrılmış stok ve kupon sayaçlarının yalnız geçiş işleminde güncellendiğini söylüyor. Ayırma satırının silinmesi kuponun ayrılmış hakkını serbest bırakmadan sayaçta bırakabilir; ödeme bekleyen sipariş de ayırmasını kaybedebilir. Bu, dokümanın kendi sayaç güvencesiyle çelişir ve müşterinin siparişinin karşılanamaması veya kupon hakkının haksız kilitlenmesiyle para/hak kaybı doğurabilir.
> Öneri: Aktif `StockReservation` varken varyant silme bağını `NO ACTION` yapın. Silme gerekiyorsa ayırmaları, ilgili sayaçları aynı geçiş işlemi içinde serbest bırakan mevcut akıştan sonra gerçekleştirin; basamaklı silme sayaç güncellemesinin yerine geçmesin.
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
> Seviye: Yüksek
> Yer: §4.40 PendingEmailChange
> Alıntı: “yeniden istenen bağlantı ilk bağlantının ömrünü taşır ve uzatmaz” / “yeni bağlantının ömrü kendi gönderiminden işler”
> Sorun: Aynı bağlantının yeniden istenmesinde sona erme anı iki farklı şekilde tanımlanmış. İlk yorum uygulanırsa süresi geçmiş ilk bağlantıdan sonra yeniden istenen bağlantılar da derhal geçersiz kalabilir; ikinci yorum uygulanırsa süre uzar. Bu, e-posta değişikliği doğrulamasında kullanıcının ilerleyememesine yol açabilecek iç çelişkidir (4).
> Öneri: Yeniden istemede önceki `ExpiresAt` değerinin devralınacağını açıkça belirleyin; süre dolduktan sonra silinen bekleyen değişiklik için yapılacak yeni isteğin ise yeni bir başlangıç ve yeni `ExpiresAt` oluşturduğunu yazın.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §4.58 DispatchRecord — saklama başlangıcı
> Alıntı: “Z-34 ve Z-43'ün ‘gönderim anı’ — satırın oluşturulma anıdır”
> Sorun: Doküman `SentAt` alanını “gönderim servisinin iletiyi kabul ettiği deneme” olarak tanımlıyor ve oluşturulma anı ile ilk deneme arasında kuyruk gecikmesi olabileceğini açıkça kabul ediyor. Buna rağmen on yıllık saklama başlangıcını oluşturulma anına çekmek, gerçekten gönderilmiş bir bildirimin kaydının gönderimden on yıl dolmadan silinmesine neden olabilir. Bu, saklama güvencesiyle çelişir ve elektronik ticaret kaydının eksik saklanması riski yaratır (1), (4).
> Öneri: Başarılı gönderilmiş satırlarda Z-34/Z-43 saklama başlangıcını `SentAt` olarak tanımlayın; hiç gönderilemeyen satırlar için uygulanacak başlangıç kuralını ayrıca ve çelişkisiz biçimde belirtin.
>
> SONUÇ: 2 BULGU
> ```

**Parça 7 — başlık notu, §5–§9:**

> ```text
> SONUÇ: TEMİZ
> ```

## 3. Bağımsız değerlendirme

Her bulgu dokümanın tamamına (v0.18) ve ilgili yerde Ürün Gereksinimleri'ne (`02` v0.85), Kullanıcı Akışları'na (`03` v0.34), karar kaydına ve kalite döngüsünün raporlarına (`AUDIT_REPORTS/06_AUDIT.md`, `06_GUARANTEE_LENS.md`, `06_DEEP_REVIEW.md`) karşı okundu — ikinci model bunları görmedi, parçalar dokümanın öteki bölümlerini de görmedi. Birleşim 2'nin BULGU-1'i ile parça 6'nın BULGU-1'i aynı yer ve aynı iddiadır; parça 3'ün BULGU-2'si ile parça 4'ün BULGU-1'i aynı kuralın (varyantın silinmesinde ayırmanın düşmesi) iki yüzüdür — ikisi de tek satırda değerlendirildi.

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| P1 · BULGU-1 | ✅ KABUL | **Ara başlığın sayısı eskimişti.** §1.1.4'ün sözlük ara başlığı "119" diyordu; altındaki tablo 125 satırdır (betikle sayıldı) ve §1.1.1'in sayım tablosu "workshop 2'de 125" der — workshop'un iki oturumu sözlüğe altı terim ekledi (K-1018, K-1031), tablo ve §1.1.1 güncellendi, ara başlık güncellenmedi. 16 + 125 + 26 = 167, bölüm başlığıyla tutar. Ölçünün (4). sınıfı, düşük şiddet. "Tablo satırlarının yeniden eşlenmesi" gerekmez: kapsama betikleri her geri beslemede yeniden koştu (`06 §1.1.1`). | §1.1.4'ün sözlük ara başlığı: 125 — 1a 105; 2b on dört; workshop 1 iki (K-1018); workshop 2 dört (K-1031). |
| P3 · BULGU-1 | ❌ RET | **Çelişki yok.** Matrisin `03 §1.2` S2 satırı Kullanıcı Akışları'nın S2 satırını özetler: "kapanış kuralını tamamlayan son olay" — kural `03 §1.4.3`'tür ve iki satır aşağıda matris onu yazar: m1 "En az bir kalem teslim edilmişse Teslim edildi (S2, S10, S11)", m2 "Hiçbir kalem teslim edilmemişse İptal edildi". Kaynağın kendisi de S2 ve S10 satırlarında koşulu §1.4.3'e bırakır. Hangi geçişin işleyeceği ürün kuralıdır (`02 §5.4`, `03 §1.4.3`); Veri Modeli geçiş kimliğini ve kaydını taşır (§5.2, §4.19). İstem matris özetinin kaynağa uygunluğunun denetlenmemesini söyler; matrisin satırları birlikte okununca tek okuma kalır. | — |
| P3 · BULGU-2 = P4 · BULGU-1 | ❌ RET | **Bilinçli ürün kararı; kupon kolu metne uymuyor.** Açık siparişi olan varyantın silinebilmesi ve ayırmasının varyantla düşmesi Ürün Gereksinimleri'nin kuralıdır (`02 §3.7.7`; K-655): *"silinen varyantın ayırması varyantla birlikte düşer — ödeme gelince kesin düşülecek, iptalde geri dönecek bir stok yoktur. Siparişi karşılamayacak firma onu firma iptaliyle kapatır"*; panel silmeden önce açık siparişlerin sayısını söyler. Veri Modeli bunu dördüncü yazım yolu olarak yazar (§4.14 (4)) ve §2.7.1 geçiş biriminin dışındaki bu tek yolu adıyla sayar — parça 4'ün "§2.7.1 yalnız geçiş işlemi der" okuması §2.7.1'in hücresiyle tutmaz. Kupon iddiası da metne uymaz: kupon hakkının ayırma satırında `VariantId` boştur (`VariantId` ile `CouponId`'den tam biri doludur — §4.14); basamaklanma yalnız varyant satırını siler, kuponun ayrılmış hakkı kendi satırında kalır ve ödemede ya da serbest bırakmada olağan yolla işlenir. Önerilen `NO ACTION` K-655'i tersine çevirir. | — |
| P3 · BULGU-3 | ⚠️ KISMİ | **Güvenlik iddiası K-1115'le kapanmıştı; kalan matrisin iki özetiydi.** Ulaşmayan e-postanın yeniden gönderimi deep review'da siparişin güncel iletişim e-postasına çevrildi (K-1115, ⚠) ve gövde bunu yazar (§4.58 Kısıtlar — *"siparişin güncel iletişim e-postasına (B-15'te düzeltmeden önceki adrese)"*); Ürün Gereksinimleri `02 §9.1.6` ve Kullanıcı Akışları 7.1.37 güncellendi. Matrisin iki hücresi (`03 §1.11.36` ve `03 §7.1`) eski "ilk alıcıya" sözünü taşıyordu — K-1115'in matrisin özet hücrelerine yansımamış yüzü (checklist §3, geri besleme betiği 1 — "izlenebilirlik matrisinin özet hücreleri"); dokümanın iki yeri farklı şey söylüyordu (ölçünün (4). sınıfı, düşük şiddet). Önerinin ikinci kolu — düzeltmeden önceki erişim anahtarını taşıyan gönderimlerin yeniden gönderilmemesi — gerekmez: gövde saklanmaz, gönderim anında üretilir (§4.58) ve güncel adrese gider. | Matrisin `03 §1.11.36` ve `03 §7.1` hücreleri: "siparişin güncel iletişim e-postasına — B-15'te düzeltmeden önceki adres (K-1115)". |
| P6 · BULGU-1 = B2 · BULGU-1 | ⚠️ KISMİ | **Kural çelişkili değil, iki ayrı işlem; metin ayrımı adlandırmıyordu.** Ürün Gereksinimleri iki işlemi ayırır: aynı bekleyen değişikliğin bağlantısını ekrandan **yeniden istemek** ilk bağlantının ömrünü taşır (`02 §3.13.5`; K-659); yeniden doğrulamadan geçilerek yazılan **yeni değişiklik isteği** öncekinin yerine geçer ve bağlantısının ömrü kendi gönderiminden işler (`02 §3.13.14`; K-734). §4.40 ikisini "yeniden istenen bağlantı" ve "yeni istek" diye yazıyordu ama farkı söylemiyordu; §4.42'nin `ExpiresAt`'i yalnız birincisini anıyordu — iki çağrı aynı farkı çelişki diye okudu. Ölçünün (4). sınıfına okunan metin farkı, düşük şiddet; ürün kuralı değişmedi. Önerinin "tek davranışı seçin" kolu uygulanmadı — iki davranış Ürün Gereksinimleri'nin kuralıdır. | §4.40 İlişkiler ve Kısıtlar: ekrandan yeniden istek ilk bağlantının ömrünü taşır (`02 §3.13.5`); yeni değişiklik isteği yeni bir ilk bağlantıdır (`02 §3.13.14`; K-734) · §4.42 `ExpiresAt` aynı ayrımla. |
| P6 · BULGU-2 | ❌ RET | **Bilinçli karar; olgusal öncülü tutmuyor.** Saklamanın başlangıcını satırın oluşturulma anına bağlamak K-1066'nın kararıdır ve gerekçesi metindedir: yeniden denemeleri başarısız satırın gönderim anı yoktur, denemeler aynı satırdadır. 6563 m.11/3'ün on yılı elektronik ticaret işleminin kaydına bağlıdır; satır işlemi doğuran olayla aynı veritabanı işleminde yazılır (§4.58) — oluşturulma anı işlemin anıdır, `SentAt` ondan sonradır. Başlangıcı `SentAt`'e taşımak süreyi işlemden değil teslimden saydırır; fark ilk denemeye kadarki kuyruk gecikmesi ve P-38'in aralıklarıdır (K-1066). "Gönderimden on yıl dolmadan silinir" kanunun ölçüsü değildir. Hukuki kontrol aşağıda. | — |
| B1 · BULGU-1 | ⚠️ KISMİ | **Mekanizma kabul edilmiş kalan sonuçtur; matrisin dört özeti kaynaktan geniş yazılmıştı.** Yeniden açılıp açık duran talebin son kapatılıştan P-40 sonra silinmesi audit'te güvence merceğinin okumasıyla kararlaştırıldı ve kalan sonuç olarak adıyla yazıldı (K-1072, K-1099; §4.57, §9.3.1): Z-33 açık talebin de silinmesini ister ve "talep kapalıysa" koşulu Z-33'ü kırardı. Önerilen "açık talepte `CreatedAt`" talebi daha da erken sildirir — talep son kapatılıştan önce açılmıştır —; öneri reddedildi. **Gerçek olan:** matrisin `02 §3.32.4`, Z-33, P-40 ve `03 §4.1.37` satırları kaynağı "açık talep açılıştan" diye özetliyordu; kaynak "hiç kapatılmayan açık talep" der (`02 §3.32.4`; Z-33'ün başlangıç hücresi) — özet kaynaktan genişti ve §4.57'nin yeniden açılmış talebiyle çelişir gibi okunuyordu (ölçünün (4). sınıfı, düşük şiddet). | Matrisin `02 §3.32.4`, `02 §4.2` Z-33, `02 §11.2` P-40 ve `03 §4.1.37` hücreleri: "hiç kapatılmamış açık talep(te) açılıştan" — kaynağın sözü. |

**Dağılım:** yedi ayrı bulgu (dokuz ham bulgu; iki çift aynı) — 1 KABUL · 3 KISMİ · 3 RET. **%100 kabul değil, %100 ret değil:** üç RET'in ikisi bilinçli kararla (K-655 — ürün kuralı; K-1066 — saklamanın başlangıcı), biri matrisin satırlarının birlikte okunmasıyla düştü; üç KISMİ'nin üçünde modelin önerisi reddedildi (`ExpiresAt`'i tek davranışa indirmek, açık talepte `CreatedAt`, düzeltme öncesi gönderimleri kilitlemek) ve gerçek metin farkı en küçük düzeltmeyle kapandı. **Sınıf:** kabul edilen dört bulgunun dördü de düşük şiddette iki yerin metin farkıdır; üçü kalite döngüsünün önceki bir kararının — workshop'un sözlük terimleri, K-1115, K-1072/K-1099 — matrisin özet hücrelerine ya da ara başlığına yansımamış yüzüdür. Yeni ürün kararı yok; Ürün Gereksinimleri'ne, Kullanıcı Akışları'na, Arayüz Tanımları'na, Teknik Mimari'ye ve MVP Kapsamı'na dönen değişiklik yok; park satırına dokunan düzeltme yok.

## 4. Ek bulgular

- **Aynı kalıbın taraması — matrisin ara başlık sayıları (P1 · BULGU-1).** §1.1.3–§1.1.11'in kalın ara başlıklarındaki sayılar altlarındaki tablo satırlarıyla betikle karşılaştırıldı (scratchpad `cr1/subcounts.py`): ara başlıkların yalnız sözlüğünkü ayrışıyordu; betiğin işaretlediği iki öteki fark (`02 §6` girişi, gövde cümleleri) ara başlıksız komşu tablonun satırlarını sayan betik yanılmasıdır — gözle okundu.
- **Aynı kalıbın taraması — "ilk alıcı" (P3 · BULGU-3).** `06`'da geçen üç yerden ikisi düzeltilen matris hücreleridir; üçüncüsü §1.3'ün geri besleme tablosundaki K-1115 satırıdır ve eski hâli tarih olarak anar — doğru.
- **Aynı kalıbın taraması — "açık talep açılıştan" (B1 · BULGU-1).** Z-33'ün başlangıcını anan matris satırları (`02 §3.32` metin, §3.32.4, Z-33, P-40, `02 §12.5` m12, `03 §1.9.2` metin, §4.1.37, K-707, K-951, park A4-11) ve §4.57, §9.1 okundu; "hiç kapatılmamış" sözünü taşımayan dördü düzeltildi, `03 §1.9.2` zaten taşıyordu.
- **Mekanik tarama.** Düzeltmeler gövdeye yeni K numarası olarak yalnız matris hücrelerine K-1115'i ekledi (§1'de Kaynak satırı yoktur; K-1115 karar kaydında var); bağ, dizin ya da silme davranışı değişmedi — §6/§7 karşılaştırması gerekmedi; matris satırı eklenmedi ya da çıkmadı — kümeler değişmez. Başlık sürümü, yazım durumu notu ve dosya sonu dipnotu v0.19'u gösterir. Değişen dosyalarda CR ve kontrol karakteri sıfır; karar kaydında çift numara yok.
- **Deep review'ın uygulanmayan bulguları (karar kaydı §14.1):** bu turun hiçbir bulgusu onların yolunu (L6a-1, L6b-4, L7-1 ve kısmen uygulananların kolları) yeniden açmadı; madde cross-review TEMİZ döndüğünde kapanır.
- **Ciddi bir sorun görülmedi.** Değerlendirme sırasında ikinci modelin listesi dışında ölçünün dört sınıfından birine giren bir sorun saptanmadı.
- **Önceki turlardan açık kalanlar:** yok (1. tur).

## 5. Kullanıcı onay checklist'i

K-992 kararıyla (Veri modeli aşaması boyunca ⚠ öneriyle kayıt, CI yeşilse `docs:` PR merge yetkisi ve yönetici + alt ajan düzeni) alt ajan değerlendirdi ve uyguladı (`06` v0.19). İki karar öneriyle kaydedildi, ikisi de süreç kararıdır ve ⚠ değildir: **K-1125** — ciddiyet ölçüsü 1. turdan, okuma paragrafıyla; **K-1126** — tek parça çağrının girdi sınırı, yerine en büyük iki birleşim ve yedi parçalı kontrol koşumunun başlıkla sabitlenen sınırları. ⚠ listesine ekleme yok.

- [x] P1 · BULGU-1 (kabul — sözlük ara başlığı 125)
- [x] P3 · BULGU-1 (ret — S2 satırı §1.4.3'ün kuralını anar; m1 ve m2 matriste)
- [x] P3 · BULGU-2 = P4 · BULGU-1 (ret — K-655'in ürün kuralı; kupon satırı basamaklanmaz)
- [x] P3 · BULGU-3 (kısmi — K-1115 matrisin iki hücresine yansıdı)
- [x] P6 · BULGU-1 = B2 · BULGU-1 (kısmi — yeniden istek ile yeni istek §4.40 ve §4.42'de ayrıldı)
- [x] P6 · BULGU-2 (ret — K-1066; oluşturulma anı işlemin anıdır)
- [x] B1 · BULGU-1 (kısmi — K-1099'un kalan sonucu korundu; matrisin dört Z-33 özeti kaynağın sözüne)
- [ ] Etki yansıtma (Faz 5) — döngü TEMİZ döndükten sonra, son turun raporunda

**Hukuki kontrol** (2026-10-18): yasal dayanak anan tek bulgu P6 · BULGU-2'dir (6563 sayılı Kanun m.11/3). Metin Teknik Mimari'nin kalite döngüsünde resmî kaynaktan okunmuştu (K-951; 7416 sayılı Kanunla eklenen son cümle — on yıl) ve iddia yeni bilgi taşımıyor: kanun süreyi işlemin kaydına bağlar; tartışılan yalnız başlangıç anının işlemin anına mı teslim anına mı bağlandığıdır ve satır işlemle aynı veritabanı işleminde doğar. Parça 3 ve birleşim 1'in web araması aynı maddeyi mevzuat.gov.tr'de aradı; o çağrıların bulguları yasal dayanak taşımaz.
