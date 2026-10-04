# Cross-Review — 04 UI Specs (Tur 1)

**Tarih:** 2026-10-04 · **Hedef:** `Docs/04_UI_SPECS.md` v0.13 (girdi commit'i `f6af17a`, 844 415 bayt) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Ciddiyet ölçüsü:** 1. turdan itibaren uygulanır (K-847) · **Koşum:** iki biçimde, beş çağrı — tek parça çağrı ve dört parçalı kontrol koşumu (K-848); turun sonucu ikisinin birleşimidir · **Girdi:** yalnız doküman ve boş şablonu (`95a168e` — Aşama 3 başlamadan önceki v0.1, 96 satır), stdin'den tek metin, boş ve izole bir çalışma klasöründe; karar kaydı, Ürün Gereksinimleri, Kullanıcı Akışları, MVP Kapsamı ve audit/deep review raporları verilmedi (K-431)

## 1. Koşum

**İstem.** Yedi kriter, bulgu biçimi (`BULGU-N` — Kriter · Seviye · Yer · Alıntı · Sorun · Öneri — ya da `SONUÇ: TEMİZ`), `cross-review` skill'inin Faz 1 madde 4'teki bilinçli karar cümlesi değiştirilmeden ve ciddiyet ölçüsünün paragrafı (K-615'in metni). İstem ayrıca belgenin kural koymadığını söyler: kural ayrıntısı Ürün Gereksinimleri'nde, akış adımları Kullanıcı Akışları'nda, kapsam MVP Kapsamı'nda yaşar ve üçü verilmedi; kurala bölüm numarası ve kimlikle işaret edilmesi tek başına bulgu değildir; yazım konvansiyonları başlık notundadır; düzey wireframe'dir. Koşum öncesinde çağrı kısa bir deneme metniyle doğrulandı. **2. tur aynı istemle koşar** — metnin tamamı aşağıdadır; girdi `istem + "=== DOKÜMAN ===" + güncel doküman + "=== ŞABLON ===" + git show 95a168e:Docs/04_UI_SPECS.md` biçiminde kurulur.

> ```text
> Sen bağımsız bir doküman denetçisisin. Aşağıda "=== DOKÜMAN ===" işaretinden sonra bir arayüz tanımları (UI specifications) dokümanının tam metni, "=== ŞABLON ===" işaretinden sonra o dokümanın boş şablonu var. Başka bir dosya okuma; çalışma klasörü boştur.
>
> Bağlam: Belge bir arayüz tanımları dokümanıdır. Kural koymaz: iş kurallarının ayrıntısı ayrı bir Ürün Gereksinimleri dokümanında (`02`), akış adımları ayrı bir Kullanıcı Akışları dokümanında (`03`), kapsam ayrı bir MVP Kapsamı dokümanında (`10`) durur; üçü de sana verilmedi. Bu doküman o kuralları ve adımları ekranlara çevirir — ekran envanteri, ortak bileşenler, navigasyon, ekran tanımları, durum × rol matrisi, form ve validasyon envanteri. Kural ayrıntısının burada yeniden yazılmamış olması ya da bir kurala yalnız bölüm numarası ve kimlikle (`02 §…`, Z-, L-, P-, S1…, Ö1…) işaret edilmesi tek başına bulgu değildir; o dokümanların içeriğini denetleme. Dokümandaki `K-123` gibi işaretler sana verilmeyen bir karar kaydına gider. Yazım konvansiyonları başlık notundadır ("Yazım konvansiyonları", 1–15). Düzey wireframe'dir: renk değeri, yazı tipi, piksel ölçüsü, teknoloji, API ve veri modeli beklenmez. Proje Aşama 3'tedir (UI/UX tasarım). Ürün, Türkiye'de tüketiciye satış yapan tek firmalık bir e-ticaret + kurumsal site ürünüdür.
>
> Dokümanı yedi kriterle denetle: tutarlılık · eksiklik · belirsizlik · teknik doğruluk · edge case · güvenlik · kullanıcı deneyimi.
>
> Dokümanda açıkça bilinçli karar, elenen seçenek ya da kabul edilmiş risk olarak yazılmış bir seçimi, yalnız seçime katılmadığın için bulgu yapma. Böyle bir karar dokümanın kendi içinde çelişki, olgusal ya da hukuki hata doğuruyorsa yaz.
>
> Yalnız CİDDİ sorunları yaz: (1) yürürlükteki mevzuata aykırılık, (2) müşteriye ya da firmaya para kaybı veya hak kaybı doğuran kural boşluğu, (3) kullanıcının takılıp kaldığı, çıkışı olmayan bir akış, (4) dokümanın iki yerinin birbiriyle çelişmesi. Nadir kenar durumları, iyileştirme önerilerini, savunmada derinlik önerilerini, üslup ve ifade tercihlerini bulgu yapma. Böyle bir sorun yoksa SONUÇ: TEMİZ yaz.
>
> Çıktı biçimi — her bulgu için (Türkçe):
>
> BULGU-N
> Kriter: <yedi kriterden biri>
> Seviye: Yüksek | Orta
> Yer: <bölüm / satır kimliği>
> Alıntı: "<dokümandan aynen>"
> Sorun: <ne yanlış, neden ciddi — dört sınıftan hangisi>
> Öneri: <somut düzeltme>
>
> Bulgu yoksa yalnız şunu yaz: SONUÇ: TEMİZ
> Bulgu varsa listenin sonuna şunu yaz: SONUÇ: <N> BULGU
> ```

**Komut** (Git Bash; çalışma klasörü boş): `codex exec -s read-only -C <boş klasör> --skip-git-repo-check --ephemeral --ignore-user-config --color never -m gpt-5.6-terra -c 'model_reasoning_effort="high"' -o <çıktı> - < <girdi>`.

**Parçalama — kontrol koşumu (K-848).** Tek parça çağrı bağlam sınırına takılmadı (315 103 token) ama 844 KB'lık dokümanı 59 saniyede `SONUÇ: TEMİZ` ile döndürdü. Kullanıcı Akışları'nın 365 KB'lık 1. turu tek parçada üç gerçek çelişki bulmuştu; bu sürenin dokümanın tamamının okunduğunu göstermediği düşünülerek aynı istemle dört parçalı bir kontrol koşumu yapıldı. Parçalar bölüm sınırındadır ve her biri başlık notunu (satır 1–73: sürüm notları, park blokları, on beş yazım konvansiyonu, alt bölüm haritası) ve şablonu taşır:

| Parça | Bölümler | Satırlar | Girdi | Token | Süre | Sonuç |
|---|---|---|---|---|---|---|
| — | Tek parça — dokümanın tamamı | 1–5180 | 852 799 bayt | 315 103 | 59 sn | `SONUÇ: TEMİZ` |
| 1 | §1 izlenebilirlik matrisi | 74–2243 | 321 350 bayt | 120 678 | 85 sn | 1 bulgu |
| 2 | §2 ortak bileşenler · §3 navigasyon · §4 envanter | 2244–2931 | 137 158 bayt | 61 135 | 181 sn | 2 bulgu |
| 3 | §5 müşteri tarafının ekran tanımları | 2932–3514 | 165 150 bayt | 66 332 | 97 sn | 1 bulgu |
| 4 | §6 durum × rol · §7 form · §8 lokalizasyon · §9 panel ekranları | 3515–5180 | 327 633 bayt | 126 893 | 84 sn | `SONUÇ: TEMİZ` |

Parçalı çağrıların istemine tek bir paragraf eklendi (bilinçli karar cümlesi ve ölçü değişmedi):

> ```text
> Parça notu: Doküman bu çağrıda parça parça verilmektedir. Aşağıdaki metin dokümanın başlık notu (sürüm, park edilen girdiler, yazım durumu, yazım konvansiyonları, alt bölüm haritası) ile <bölümler> bölümlerinden oluşur; öteki bölümler bu çağrıda verilmedi. Verilmeyen bir bölüme yapılan atıf ve orada yaşayan içeriğin burada görünmemesi tek başına bulgu değildir; yalnız verilen metnin içindeki sorunları yaz.
> ```

**Kapsam sınırı.** Parça öteki bölümleri görmez: parçalar arası çelişkiyi yalnız tek parça çağrı yakalayabilir — o çağrı TEMİZ döndü. Parça 2'nin iki bulgusu görmediği §9'un zaten yazdığı şeyi eksik saydı (2.13.2 → 9.8.4; 2.5.4 → 9.20.9); yönetici değerlendirmesi her iddiayı dokümanın tamamına karşı okudu (§3). Beş çağrının hiçbiri web araması yapmadı (koşum kayıtlarında arama satırı yok).

## 2. Ham bulgular

İkinci modelin çıktıları, düzenlenmeden (alıntı blokları içinde).

**Tek parça çağrı:**

> ```text
> SONUÇ: TEMİZ
> ```

**Parça 1 — §1:**

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §1.1 girişi / §1.1.6, `02 §2.1` satırı
> Alıntı: `"Ekran" sütununun değer kümesi §1.1.2'dedir.`
> Sorun: §1.1.6’daki bazı satırlar Ekran hücresine değer kümesinde olmayan `akış dizini — ...`, `kural düzeyinde — ...` ve `süre düzeyinde — ...` değerlerini yazıyor. Bu, matrisin kendi tanımladığı değer kümesi ve “eşlendi” tanımıyla çelişir; ileri izlenebilirlikte bu kaynakların hangi ekrana ulaştığı doğrudan doğrulanamaz. (Dokümanın iki yerinin çelişmesi.)
> Öneri: Bu satırlara gerçek ekran/bileşen eşlemelerini yazın; dolaylı kaynak zinciri gerekiyorsa bunu ayrı bir “Eşleme dayanağı” sütununa taşıyın.
>
> SONUÇ: 1 BULGU
> ```

**Parça 2 — §2–§4:**

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §2.13.2; §3.4.3; §3.4.5
> Alıntı: “sipariş listesinde sipariş numarası ve iletişim e-postasıyla arama ve durum süzgeci” / “E-36'nın bu işarete süzülmüş hâli” / “E-36'nın ‘IBAN bekleniyor’ süzgeciyle”
> Sorun: E-36 için tanımlı tek süzgeç durum süzgeciyken navigasyon, “e-posta ulaşmadı” ve “IBAN bekleniyor” gibi durum olmayan işaretlere göre süzülmüş görünümler gerektiriyor. Bu, iki bölüm arasında çelişkidir; yöneticinin bekleyen IBAN ve ulaştırılamayan bildirim işlerini hangi mekanizmayla tam listeleyip yöneteceği belirsiz kalır.
> Öneri: §2.13.2’de E-36’nın desteklediği işaret/iş-yükü süzgeçlerini açıkça ekleyin; bunlar kullanıcı tarafından değiştirilemeyen hazır görünümler olacaksa §3.4’te “süzgeç” yerine bu hazır görünüm mekanizmasını tanımlayın.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §2.5.3.1; §2.5.4
> Alıntı: “davet satırında, yeniden gönder düğmesi olmadan (E-49)” / “Yeniden gönderilebilenlerin kümesi işareti düşürenlerin kümesidir”
> Sorun: E-49’daki davet satırı “e-posta ulaşmadı” işareti taşırken yeniden gönderme özellikle yasaklanmış; buna karşılık §2.5.4, bu işareti doğuran e-postaların tamamını yeniden gönderilebilir kabul ediyor. Bu doğrudan çelişki, ulaştırılamayan yönetici davetinin çözüm yolunu belirsiz bırakır.
> Öneri: E-49 davetini §2.5.4’te açık istisna yapın ve çözüm eylemini belirtin (ör. daveti geri çekip yeni davet oluşturma); veya E-49’a yeniden gönder eylemini ekleyin.
>
> SONUÇ: 2 BULGU
> ```

**Parça 3 — §5:**

> ```text
> BULGU-1
> Kriter: Kullanıcı deneyimi
> Seviye: Yüksek
> Yer: §5.13.31; §5.15.2–§5.15.4
> Alıntı: “Onay beklenmeyen bir sebeple sonuçsuz kalırsa ekran "işlem yapılmadı" demez (K-745); müşteriyi sonucu görmeye götürür — üyeyi sipariş geçmişine, misafir alıcıyı sipariş takibi girişine (§2.7.3.3); yazılanlar ve sepet durur.”
> Sorun: Misafir alıcı, belirsiz sonuçlu onaydan sonra sipariş numarasını hiç görmemiş olabilir. Yönlendirildiği E-15 ise sipariş numarası ve e-posta eşleşmesini zorunlu tutar. Numara veya güvenli bir doğrudan erişim anahtarı taşınmadığı için müşteri siparişin oluşup oluşmadığını öğrenemez; sepetten yeniden deneyerek mükerrer sipariş ya da ödeme riski doğar. Bu, çıkışı olmayan bir kurtarma akışıdır.
> Öneri: Belirsiz sonuçta misafir alıcıyı, denemeye bağlı imzalı tek kullanımlık bağlantıyla doğrudan E-16’ya götürün; alternatif olarak sipariş numarasını güvenli biçimde gösterip E-15’te numara ve e-postayı ön doldurun. Sonuç kesinleşene kadar yeniden sipariş vermeyi engelleyin.
>
> SONUÇ: 1 BULGU
> ```

**Parça 4 — §6–§9:**

> ```text
> SONUÇ: TEMİZ
> ```

## 3. Bağımsız değerlendirme

Her bulgu dokümanın tamamına (v0.13), Ürün Gereksinimleri'ne (`02` v0.63) ve karar kaydına karşı okundu — ikinci model bunları görmedi, parçalı çağrılar dokümanın öteki bölümlerini de görmedi.

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| Tek parça | — | `SONUÇ: TEMİZ`. Değerlendirilecek bulgu yok; sonucun ağırlığı §1'in "Parçalama" paragrafında ve K-848'de tartıldı. | — |
| P1 · BULGU-1 | ⚠️ KISMİ | **Tanım ile on altı satır arasında gerçek bir metin farkı, ama eşleme eksik değil.** §1.1'in tanımı "Ekran" sütununun değer kümesini §1.1.2'ye bağlıyor ve "eşlendi"yi en az bir ekran ya da ortak bileşen diye tanımlıyordu; §1.1.6'nın on iki ve §1.1.9'un dört satırı ise ekran yerine *akış dizini*, *kural düzeyinde*, *süre düzeyinde* yazıyor. Bu satırlar bilinçli olarak toplu satırdır: `02`'nin alt bölüm ve kural düzeyindeki satırı, adımları `03`'ün satırlarıyla (§1.1.4, §1.1.8) ve kuralları §1.1.5 ile §1.1.9'un satırlarıyla matriste ayrıca durduğu için ekranı yinelemez — §1.1.6'nın girişi bunu yazar (*"eşleme, alt bölümün `03`'teki adımlarının ekranlarından türetilmiştir"*; K-727, K-730). Ekranlara ulaşılamayan bir kaynak yok; eksik olan tanım cümlesiydi — ölçünün (4). sınıfında dar bir iç tutarsızlık. **Öneri uygulanmadı:** on altı satıra ekranları yazmak ya da yeni bir "eşleme dayanağı" sütunu açmak aynı eşlemeyi iki yerde tutardı (K-727'nin birimi). | §1.1'in tanım paragrafı: *"tek istisna `02`'nin toplu satırlarıdır — alt bölüm ve kural düzeyindeki satır, adımları ya da kuralları matriste ayrıca satır taşıyorsa ekranı yinelemez, o satırları gösterir (akış dizini, kural düzeyinde, süre düzeyinde — §1.1.6, §1.1.9) ve "eşlendi" durumunu gösterdiği satırların eşlemesinden alır"*. Yeni karar yok. |
| P2 · BULGU-1 | ⚠️ KISMİ | **Özet ile ayrıntı arasında gerçek bir fark, ama süzgeç tanımlı.** 2.13.2 sipariş listesi için yalnız "arama ve durum süzgeci" sayıyordu ve cümle kapalı bir liste diye okunuyor ("yalnız kaynağın saydığı listelerde vardır"). Ekranın tanımı 9.8.4 üç süzgeç sayar: iki eksenin durumu ve tek seçimli "bekleyen iş ve işaret" süzgeci — "IBAN bekleniyor" ve "e-posta ulaşmadı" dahil (K-802); 3.4.3 ve 3.4.5'in götürdüğü süzülmüş hâller bunlardır. Modelin *"hangi mekanizmayla … belirsiz kalır"* iddiası bu yüzden doğru değil — parça §9'u görmedi; çelişki yalnız 2.13.2'nin eksik özetidir (ölçünün (4). sınıfı). Önerinin ilk yolu — 2.13.2'ye süzgeci eklemek — uygulandı; "hazır görünüm" ayrımı gereksiz. | 2.13.2: *"sipariş listesinde sipariş numarası ve iletişim e-postasıyla arama, iki eksenin durum süzgeci ve bekleyen iş ve işaret süzgeci — "IBAN bekleniyor" ve "e-posta ulaşmadı" dahil (9.8.4)"*. Yeni karar yok. |
| P2 · BULGU-2 | ⚠️ KISMİ | **2.5.4'ün genel cümlesi 2.5.3.1'in davet satırıyla çelişiyordu, ama davetin çıkışı yazılı.** 2.5.4 *"yeniden gönderilebilenlerin kümesi işareti düşürenlerin kümesidir"* diyordu; dayandığı `02 §9.1.6` bu kümeyi müşteri bildirimleriyle — B-1…B-16 — sınırlar, davet e-postası o kümede değildir. Davet satırı işareti taşır ama yeniden gönderilmez (2.5.3.1; K-540) ve yolu yeni davettir: 9.20.4, 9.20.6 (*"davet e-postasının yeniden gönderilmesi — yol yeni davettir"*) ve 9.20.9 (*"Yeni davet göndermek — 'e-posta ulaşmadı' işaretinin yolu"*; `03 §3.1.1`, §7.1.53; K-820). Çözüm yolu belirsiz değil — parça §9'u görmedi; çelişki 2.5.4'ün cümlesindeydi (ölçünün (4). sınıfı). Önerinin ilk yolu — 2.5.4'te istisna ve çözüm — uygulandı; ikinci yolu (E-49'a yeniden gönder) `02 §9.1.6`'nın kümesini genişletir, reddedildi. | 2.5.4: *"Yeniden gönderilebilenlerin kümesi işareti düşüren müşteri bildirimlerinin — B-1…B-16 — kümesidir (`02 §9.1.6`); yönetici davetinin satırı işareti taşır ama yeniden gönderilmez, yolu yeni davettir (9.20.4, 9.20.9)."* Yeni karar yok. |
| P3 · BULGU-1 | ❌ RET | **Çıkışı olmayan akış değil; çıkışların üçü de dokümanda yazılı.** (a) Sipariş onay anında doğduysa sipariş e-postası (B-1) misafir alıcıya gider ve içindeki bağlantı sayfayı e-posta sormadan açar — E-15 bunu ekranın üstünde hatırlatır (5.15.3'ün (4). bölgesi; `02 §3.22.3`). (b) Sepet ve yazılanlar durur (5.13.31); müşteri aynı sepetten yeniden onaylarsa, sipariş doğmuşsa onay bölümü onaydan önce önceki ödenmemiş siparişin **numarasını** ve bu onayla kendiliğinden iptal edileceğini söyler (5.13.9 (a), 5.13.18; K-837) ve önceki sipariş iptal edilir (`02 §3.17.7`; `03 §2.4.9`) — çift sipariş doğmaz (K-745'in gerekçesi). (c) Belirsiz onay anında ödeme alınmamıştır: kartta ödeme onaydan sonra sağlayıcının sayfasında yapılır (5.14.3, 5.14.6), havalede müşterinin gönderimiyle; "mükerrer ödeme" riski onay adımından doğmaz. Modelin önerisi (denemeye bağlı imzalı tek kullanımlık bağlantı, yeniden onayı engellemek) yeni bir mekanizmadır; bilinçli kararın — numara henüz bilinmeyebileceği için misafir alıcının sipariş takibi girişine gitmesi (2.7.3.3; K-745) — çıkışlarını görmemiştir. | Yok. |

**Dağılım:** 0 KABUL · 3 KISMİ · 1 RET (tek parça çağrı TEMİZ). **%100 kabul değil:** üç KISMİ'nin üçünde de modelin etki iddiası ("belirsiz", "doğrulanamaz") reddedildi ve önerinin yalnız en küçük yolu uygulandı; bir bulgu somut yerlerle reddedildi. Üç düzeltme de bir özet ya da tanım cümlesini aynı dokümandaki ayrıntıya hizalar; yeni yetenek, yeni alan ya da yeni kural eklemez. Bulgular bilinçli konvansiyonlara (konvansiyon 1–15; K-726, K-762) dokunmuyor; audit'in uygulanmayan sekiz bulgusundan (YASAL-6, DR-2, DR-4, KAPSAM-K-7, K-9, K-13, FORM-14, FORM-17) hiçbiri geri dönmedi.

## 4. Ek bulgular

- **Aynı kalıbın taraması (P2 · BULGU-1):** dokümanda "durum süzgeci" geçen öteki yerler — §1.2'nin K-714 ve GA-11 satırları, §1.3'ün Talepler satırı, 7.2.3.3 — matrisin tarihçesini ya da Talepler listesini anlatır; sipariş listesinin süzgeçlerini kapalı listeyle sayan başka yer yok. 3.4.2, 3.4.3, 3.4.5 ve 9.3.6 süzülmüş hâlleri 9.8.4'ün adlarıyla anar.
- **Aynı kalıbın taraması (P2 · BULGU-2):** yeniden gönderimi anan öteki yerler — başlık notunun Aşama 2 park satırı, §1.1'in `03 §7.1.1` ve §7.1.37 satırları, §1.2'nin aday listesi notu, §1.3'ün workshop notu — kapsamı karar kaydına ya da 2.5.4'e bırakır; davetin yeniden gönderildiğini söyleyen yer yok.
- **Aynı kalıbın taraması (P1 · BULGU-1):** toplu satır değerleri yalnız §1.1.6 (on iki satır) ve §1.1.9'da (dört satır) var; matrisin öteki alt bölümlerinin "Ekran" hücreleri §1.1.2'nin değerlerini taşır.
- **Mekanik tarama:** dokümanda anılan karar numaralarının hepsi karar kaydında satır olarak var; karar kaydında çift numara yok. Düzeltmeler gövdeye yeni K numarası eklemedi — Kaynak satırları değişmedi. Sürüm başlığı ve dosya sonu dipnotu aynı sürümü gösteriyor (v0.14).
- **Ciddi bir sorun görülmedi.** Bu turda ikinci modelin listesinin dışında ölçünün dört sınıfından birine giren bir sorun saptanmadı.
- **Önceki turlardan açık kalanlar:** yok (1. tur). Audit'in etki yansıtmaya bıraktığı iz — Ürün Gereksinimleri'nin `§3.12.9` ile §6.2.9 arasındaki metin farkı — bu turun girdisi değildir; Faz 5'te, TEMİZ turunda taranır.

## 5. Kullanıcı onay checklist'i

K-723 kararıyla (Aşama 3 boyunca ⚠ öneriyle kayıt, CI yeşilse `docs:` PR merge yetkisi ve yönetici + alt ajan düzeni) alt ajan değerlendirdi ve uyguladı (`04` v0.14). İki süreç kararı öneriyle kaydedildi: **K-847** (turdan önce) — ciddiyet ölçüsü 1. turdan, skill'in ikinci koşulu; **K-848** (tur içinde) — tek parça çağrının yanında dört parçalı kontrol koşumu, turun sonucu ikisinin birleşimi. İkisi de ⚠ değildir. Ürün kuralına dokunan karar yok; Ürün Gereksinimleri'ne, Kullanıcı Akışları'na ve MVP Kapsamı'na dönen değişiklik yok, `04 §1.3`'ün geri besleme tablosuna satır girmedi.

- [x] P1 · BULGU-1 (kısmi — §1.1'in tanımı toplu satırları anar; on altı satıra ekran yazılmadı)
- [x] P2 · BULGU-1 (kısmi — 2.13.2 9.8.4'ün süzgeçlerini sayar; "belirsiz" iddiası reddedildi)
- [x] P2 · BULGU-2 (kısmi — 2.5.4 kümeyi B-1…B-16 ile sınırlar ve davetin yolunu yazar; E-49'a yeniden gönder eklenmedi)
- [x] P3 · BULGU-1 (ret — çıkışlar 5.13.9, 5.13.18, 5.15.3'te ve `02 §3.17.7`'de)
- [ ] Etki yansıtma (Faz 5) — döngü TEMİZ döndükten sonra, son turun raporunda

**Hukuki kontrol** (2026-10-04): bulgularda yasal dayanaklı iddia yok; model web araması yapmadı. Düzeltmeler yeni hukuki içerik eklemiyor.

**Yakınsama ölçüsü:** doküman 844 415 bayttan 845 849 bayta (+1,4 KB) büyüdü; artışın yarısından çoğu başlıktaki sürüm notudur. Tablolara yeni satır girmedi.

## 6. Sonuç ve 2. tur

**Sonuç: TEMİZ DEĞİL** (ölçünün dört sınıfında) — tek parça çağrı TEMİZ, kontrol koşumu dört bulgu; üçü uygulandı. Faz 5 (etki yansıtma) bu turda koşmaz; TEMİZ turunda koşar.

**2. tur** aynı istemle (§1'in alıntı bloğu, değiştirilmeden) ve iki koşumla yürür (K-848): (1) tek parça — istem, `=== DOKÜMAN ===`, güncel `04`, `=== ŞABLON ===`, `95a168e` şablonu; (2) dört parça — aynı istem, parça notu (§1), başlık notu ve şablonla; parça sınırları bölüm başlıklarıdır (`## 1.` · `## 2.`–`## 4.` · `## 5.` · `## 6.`–`## 9.`) — satır numaraları sürümle kayar, başlıklar kaymaz. Önceki turun raporu, karar kaydı ve audit raporları verilmez (K-431). Tur, beş çağrının beşi de `SONUÇ: TEMİZ` dönerse TEMİZ'dir.
