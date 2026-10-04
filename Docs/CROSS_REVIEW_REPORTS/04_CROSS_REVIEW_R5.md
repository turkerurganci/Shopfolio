# Cross-Review — 04 UI Specs (Tur 5 — TEMİZ, K-849'un çıkış kuralıyla)

**Tarih:** 2026-10-05 · **Hedef:** `Docs/04_UI_SPECS.md` v0.16 (girdi commit'i `20ac253`, 848 470 bayt, 5186 satır) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Ciddiyet ölçüsü:** 1. turdan itibaren uygulanır (K-847) — bu tur ölçüyle koştu; **TEMİZ bu ölçüdedir** · **Koşum:** iki biçimde, beş çağrı — tek parça çağrı ve dört parçalı kontrol koşumu (K-848) · **Çıkış kuralı:** K-849 (turdan önce, yöneticinin kararı) — tekrar eden RET turun sonucunu belirlemez; 5. turda kabul edilen bulguların hepsi düşük şiddette metin farkıysa döngü bu turda kapanır · **Girdi:** yalnız doküman ve boş şablonu (`95a168e`), stdin'den tek metin, yeni, boş ve izole bir çalışma klasöründe; karar kaydı, K-849, önceki turların raporları, Ürün Gereksinimleri, Kullanıcı Akışları, MVP Kapsamı ve audit/deep review raporları verilmedi, önceki turlardan hiçbir şey taşınmadı (K-431)

## 1. Koşum

**İstem 1.–4. turla aynıdır** — `04_CROSS_REVIEW.md` §1'in iki alıntı bloğundan (istem ve parça notu) betikle çıkarıldı ve değiştirilmeden kullanıldı; parça notunda `<bölümler>` yerine parçanın bölümleri yazıldı (`§1` · `§2–§4` · `§5` · `§6–§9`). Girdi `istem + "=== DOKÜMAN ===" + doküman + "=== ŞABLON ===" + git show 95a168e:Docs/04_UI_SPECS.md` biçimindedir; tek parça girdinin istem ve ayraç payı (2 705 bayt) 4. turunkiyle aynıdır. Her parça başlık notunu (satır 1–79) taşır. Parça sınırları bölüm başlıklarıdır (`## 1.` · `## 2.`–`## 4.` · `## 5.` · `## 6.`–`## 9.`). Beş çağrı paralel koştu; her biri kendi boş klasöründe (klasörler koşumdan sonra da boştu). K-849 modele verilmedi; istemi değiştirmez, yalnız turun sonucunun okunuşunu belirler.

**Komut** (Git Bash; çalışma klasörü boş): `codex exec -s read-only -C <boş klasör> --skip-git-repo-check --ephemeral --ignore-user-config --color never -m gpt-5.6-terra -c 'model_reasoning_effort="high"' -o <çıktı> - < <girdi>`.

| Parça | Bölümler | Satırlar (`20ac253`) | Girdi | Token | Süre | Web araması | Sonuç |
|---|---|---|---|---|---|---|---|
| — | Tek parça — dokümanın tamamı | 1–5186 | 856 854 bayt | 418 125 | 137 sn | 3 | 1 bulgu |
| 1 | §1 izlenebilirlik matrisi | 80–2249 | 324 502 bayt | 121 818 | 71 sn | 0 | `SONUÇ: TEMİZ` |
| 2 | §2 ortak bileşenler · §3 navigasyon · §4 envanter | 2250–2937 | 140 243 bayt | 61 940 | 46 sn | 0 | `SONUÇ: TEMİZ` |
| 3 | §5 müşteri tarafının ekran tanımları | 2938–3520 | 167 831 bayt | 63 548 | 52 sn | 0 | `SONUÇ: TEMİZ` |
| 4 | §6 durum × rol · §7 form · §8 lokalizasyon · §9 panel ekranları | 3521–5186 | 330 697 bayt | 126 410 | 42 sn | 0 | `SONUÇ: TEMİZ` |

**Koşum kayıtlarından iki not.** (1) Tek parça çağrının kaydında bir kez `context compacted` satırı var: model web aramalarından sonra kendi bağlamını özetledi ve 418 bin token kullandı (3. turdaki gibi). (2) Üç web araması tek parça çağrınındır ve üçü de Mesafeli Sözleşmeler Yönetmeliği'nin cayma bedelinin iadesine dairdir (`site:mevzuat.gov.tr … cayma bedel iadesi malın geri gönderilmesi ispat` ve iki benzeri); bulgu bu aramalardan doğdu (§3).

**Kapsam sınırı (1. turla aynı).** Parça öteki bölümleri görmez; parçalar arası çelişkiyi yalnız tek parça çağrı yakalar. İddia dokümanın tamamına, Ürün Gereksinimleri'ne ve karar kaydına karşı okundu (§3).

## 2. Ham bulgular

İkinci modelin çıktıları, düzenlenmeden (alıntı blokları içinde; yalnız satır sonu boşlukları silindi).

**Tek parça çağrı:**

> ```text
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §5.18.6; §6.2.18.2
> Alıntı: "süre (Z-16) teslimden önceki caymada ve hizmette beyandan, teslimden sonraki caymada malın firmaya ulaşmasından işler"
> Sorun: Teslim sonrası caymada iade süresini yalnız malın firmaya fiziksel ulaşmasına bağlamak mevzuata aykırı sonuç doğurur. Belirtilen iade taşıyıcısına teslim edilen malda süre, taşıyıcıya teslimle başlar; taşıma gecikmesi tüketicinin iade süresini uzatamaz. Bu, tüketicinin bedel iadesini haksız biçimde geciktirerek hak ve para kaybına yol açar. [Ticaret Bakanlığı açıklaması](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/mesafeli-sozlesmeler-hakkinda-bilgilendirme)
> Öneri: E-18'de iade için belirtilen taşıyıcıyı açıkça gösterin; Z-16'nın bu taşıyıcıya teslim tarihinde başlayacağını, farklı taşıyıcı kullanılırsa firmanın teslim aldığı tarihin esas olacağını yazın. §6'daki ilgili durum satırını da aynı kuralla hizalayın.
>
> SONUÇ: 1 BULGU
> ```

**Parça 1 — §1:**

> ```text
> SONUÇ: TEMİZ
> ```

**Parça 2 — §2–§4:**

> ```text
> SONUÇ: TEMİZ
> ```

**Parça 3 — §5:**

> ```text
> SONUÇ: TEMİZ
> ```

**Parça 4 — §6–§9:**

> ```text
> SONUÇ: TEMİZ
> ```

## 3. Bağımsız değerlendirme

İddia dokümanın tamamına (v0.16), Ürün Gereksinimleri'ne (`02` v0.63) ve karar kaydına karşı okundu; modelin gösterdiği kaynak açıldı.

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| Tek parça · BULGU-1 | ❌ RET | **Mevzuata aykırılık yok; 3. turun tek parça bulgusu aynı yer ve aynı iddiayla döndü** (`04_CROSS_REVIEW_R3.md` §3 — orada da 5.18.6, aynı kaynak, aynı öneri; bu kez 6.2.18.2 de anıldı, 3. turun aynı kalıp taramasının saydığı satır). Önceden yalnız bir kez reddedildiği için K-849'un (1). maddesindeki "tekrar eden RET" tanımına girmez; kendi gerekçesiyle reddedildi. Mesafeli Sözleşmeler Yönetmeliği m.12/1 iki cümledir: satıcının ön bilgilendirmede belirttiği taşıyıcıya teslimde süre teslimden, *"öngörülenin haricinde bir taşıyıcı ile"* iadede malın satıcıya ulaşmasından işler. Firma iade taşıyıcısı belirlemez, taşıyıcıyı müşteri seçer (K-293; `02 §7.4.2`, K-492, K-493) — teslimden sonraki her iade ikinci cümleye girer ve süre malın firmaya ulaşmasıyla başlar (K-491; `02 §7.4.1`). Doküman koşulu kendi içinde taşır: 5.18.6'nın hemen üstündeki 5.18.5 malın *"müşterinin seçtiği taşıyıcıyla, iade adresine karşı ödemeli"* gönderileceğini yazar. Modelin önerisi (firmanın iade taşıyıcısı belirtmesi) K-293'ün ve K-492'nin elediği seçenektir; iade masrafının firmada kalmasının yasal dayanağı da taşıyıcı belirtilmemesidir (m.12/5; `02 §7.4.2`). **Modelin gösterdiği kaynak** (Ticaret Bakanlığı tüketici rehberi, 17 Ağustos 2026 tarihli sayfa; 2026-10-05'te yeniden açıldı) aynı ayrımı yazar: belirtilen kargoyla iadede süre *"tüketicinin malı kargoya teslim ettiği tarihten"*, *"ön bilgilendirmede belirtilenden farklı bir kargo ile"* iadede *"ürünün satıcıya ulaştığı tarihte"* başlar. Taşıyıcı hiç belirtilmediğinde bu okumanın lafızdan çıkan bir sonuç olduğu kayıtta adıyla yazılı kalan risktir; avukat teyidi önerisi proje sahibine sunuldu ve reddedildi (K-721). Bulgu yeni bilgi taşımıyor; karar yeniden açılmaz ve çok kritik soru doğmaz. | Yok. |
| P1 · P2 · P3 · P4 | — | `SONUÇ: TEMİZ`. | — |

**Dağılım:** 0 KABUL · 0 KISMİ · 1 RET (dört parça TEMİZ). **%100 ret savunmacılık mı?** Tek bulgu yeniden okundu ve küçük bir düzeltme yolu arandı: 5.18.6'ya "firma iade taşıyıcısı belirtmediği için" yan cümlesi 3. turda da düşünülmüş ve reddedilmişti — koşul bir satır üstte (5.18.5) yazılı, kural ayrıntısı evinde kalır (konvansiyon 12) ve `02 §7.4.1` koşulu aynı biçimde §7.4.2'ye bırakır; eklemek `04`'ü `02`'den farklı bir metne taşırdı. Dokümanın o yerleri 3. turdan beri değişmedi. RET bir karar satırına (K-491, K-293, K-492, K-721) ve dokümanın somut maddesine (5.18.5; `02 §7.4.1`, §7.4.2) dayanır. 1.–4. turun KABUL ve KISMİ ile kapanan on bulgusundan hiçbiri geri dönmedi; 1., 3. ve 4. turun tekrar eden RET'i (misafir alıcının belirsiz onaydan sonraki çıkışı) bu turda gelmedi.

## 4. Ek bulgular

- **Aynı kalıbın taraması (tek parça · BULGU-1):** geri ödeme süresinin başlangıcını anan yerler — 5.18.6, 6.2.18.2, 9.9.27, 2.12.5.1 ve §1.1'in K-491 satırı — aynı kuralı `02 §7.4.1`'e işaretle taşır; taşıyıcıya teslimden süre başlatan ya da firmanın iade taşıyıcısı belirlediğini söyleyen yer yok (3. turun taramasıyla aynı sonuç; o yerler değişmedi).
- **Ciddi bir sorun görülmedi.** Bulgunun çevresi (5.18, 6.2.18, 9.9.27) okunurken ölçünün dört sınıfından birine giren başka bir sorun saptanmadı.
- **Mekanik tarama:** etki yansıtmanın gövdeye eklediği K numaraları karar kaydında satır olarak var; karar kaydında çift numara yok; sürüm başlığı, başlık notunun yeni sürüm notu ve dosya sonu dipnotu v0.17'yi gösteriyor.
- **Önceki turlardan açık kalanlar:** 1.–4. turun bulguları işlendi. Audit'in etki yansıtmaya bıraktığı iz — Ürün Gereksinimleri'nin `§3.12.9` ile §6.2.9 arasındaki "Bu ürünü daha önce aldınız" metin farkı — bu turun etki yansıtmasında kapandı (§6). 4. turun "sonraki adımın kararı yöneticiye bırakıldı" notu K-849'la kapandı.

## 5. Kullanıcı onay checklist'i

K-723 kararıyla (Aşama 3 boyunca ⚠ öneriyle kayıt, CI yeşilse `docs:` PR merge yetkisi ve yönetici + alt ajan düzeni) alt ajan değerlendirdi ve uyguladı (`04` v0.17 — değişiklikler etki yansıtmanındır; turun kendisi dokümanı değiştirmedi). Bir süreç kararı kaydedildi: **K-849** (turdan önce, yöneticinin kararı) — çıkış kuralı, `(öneriyle kaydedildi)`; ⚠ değildir. Etki yansıtmada karar alınmadı; ⚠ karar yok.

- [x] Tek parça · BULGU-1 (ret — 3. turun bulgusunun dönüşü; K-491'in kuralı m.12/1'in ikinci cümlesidir, taşıyıcıyı müşteri seçer — 5.18.5; kalan risk K-721'de kapandı)
- [x] Cross-review döngüsü TEMİZ — K-849'un çıkış kuralıyla, beş turda
- [x] Etki yansıtma (Faz 5) — §6

**Hukuki kontrol** (2026-10-05): bulgu yasal dayanaklıdır. Dayanak hükmün metni K-491'de resmî konsolide metinden (mevzuat.gov.tr, MevzuatNo 20237) alıntılıdır ve m.12/1'in iki cümlesini taşır; modelin gösterdiği Ticaret Bakanlığı sayfası (17 Ağustos 2026) 2026-10-05'te yeniden açıldı ve aynı ayrımı yazar. Kuralın taşıyıcı hiç belirtilmediği hâle uygulanışı K-491'in kaydında ve K-721'de adıyla yazılı kalan risktir; proje sahibi doğrulama önerisini reddetti. Bu tur yeni hukuki içerik eklemedi; etki yansıtmanın hizaladığı uyarı metni (K-236) hukuki içerik taşımaz.

**Yakınsama ölçüsü:** bulgu sayısı beş turda 4 → 4 → 2 → 5 → 1; kabul edilen ya da kısmen kabul edilen 3 → 4 → 0 → 3 → 0. Bu turda dört parçanın dördü de ilk kez aynı anda TEMİZ döndü; tek bulgu bilinçli ve doğrulanmış bir hukuki karara döndü. Doküman cross-review boyunca 844 415 bayttan 848 470 bayta (+4,1 KB) büyüdü; etki yansıtmayla 850 694 bayttır (+2,2 KB — sürüm notu, §1.3'ün etki yansıtma satırı, Kaynak hücrelerine eklenen numaralar ve uyarının tam metni). Tablolara cross-review'ın beş turunda yeni satır girmedi; §1.3'ün geri besleme tablosuna bir hizalama satırı girdi.

## 6. Sonuç ve etki yansıtma (Faz 5)

**Sonuç: TEMİZ — K-849'un çıkış kuralıyla, 5 tur.** Dört parça `SONUÇ: TEMİZ`; tek parça çağrının tek bulgusu reddedildi; kabul edilen (KABUL ya da KISMİ) bulgu yoktur — K-849'un (2). maddesinin koşulu ("kabul edilenlerin hepsi düşük şiddette metin farkı") sağlanır, (3). madde (yüksek ya da orta şiddette kabul) tetiklenmez. Düşük şiddetteki iki yerin metin farkı sınıfının kalan riski Aşama 3'ün checkpoint'ine devredildi (karar kaydı §10.1, checkpoint maddesi).

Etki yansıtma `cross-review` skill'inin Faz 5'iyle koştu: cross-review'ın beş turunda `04`'te yapılan değişiklikler (`git diff f6af17a 20ac253 -- Docs/04_UI_SPECS.md`: §1.1'in tanımı · §1.1.10'un `03 §7.1.37` satırı · 2.5.4 · 2.12.3.1 · 2.13.2 · konvansiyon 4 · 6.2.16.9, 6.2.16.10 · 6.3.1.2, 6.3.1.4 · 9.9.18 · 9.19.3), Aşama 3'ün bütün geri beslemeleri ve Aşama 3 kararlarının sonraki dokümanlara bıraktığı işler tarandı.

**Upstream — Ürün Gereksinimleri (`02` v0.63 → v0.64), Kullanıcı Akışları (`03` v0.19 → v0.20), MVP Kapsamı (`10` v0.41 → v0.42), Proje Vizyonu (`01` v0.33):**
- **Betik 1 (Aşama 3 kararı ↔ geri besleme tablosu):** dört dokümanın andığı her Aşama 3 kararı (`01`: 2, `02`: 32, `03`: 25, `10`: 19 numara) `04 §1.3`'ün tablolarında var; tersine, `04 §1.3`'ün "Nereye yansıdı" sütununda `02`, `03`, `10` ya da `01` gösteren her satırın Aşama 3 kararı o dokümanda anılıyor. Eksik yok.
- **Cross-review düzeltmelerinin üst dokümana karşı okunuşu:** her düzeltme `04`'ün bir cümlesini `04`'ün başka bir yerine ya da üst dokümanın var olan kuralına hizaladı; üst dokümanda değişiklik gerektiren yok. 2.12.3.1 ↔ `02 §7.4.5` (kart iadesinin havale yolu istisnası) · 2.5.4 ve §1.1.10 ↔ `02 §9.1.6`, `03 §7.1.37` (yeniden gönderim B-1…B-16) · 9.9.18 ↔ `03 §8.2.7` · 6.3.1.2, 6.3.1.4 ↔ `03 §6.1` · 6.2.16.9, 6.2.16.10 ↔ `02 §7.2.9` · 9.19.3 ↔ `02 §3.1.5`, §3.33.2. **Gerekçesiyle elenen bir eşleşme:** 2.13.2 artık sipariş listesinin "bekleyen iş ve işaret" süzgecini sayar (9.8.4), `02 §10.1.2`'nin Siparişler satırı ise "arama, durum süzgeci (K-714)" der. Fark kural değildir: K-714 aramayı ve durum süzgecini ürün kuralı olarak kurdu, süzgecin biçimini `04`'e bıraktı (`03 §8.2.11`); bekleyen iş süzgeci ekran kurgusudur (K-802, öneriyle kaydedildi) ve üst dokümana dönmez (konvansiyon 1). Sonraki dokümandaki karşılığı API Tasarımı'nın park satırındadır (K-802).
- **Taşınan açık iş — "Bu ürünü daha önce aldınız" uyarısının metin farkı — kapandı.** Kural ve metin `02 §3.12.9`'dadır (K-236): *"Bu ürünü daha önce aldınız — sipariş sayfanızdan indirebilirsiniz"*. K-652'nin birinci betiği (anahtar terim: "daha önce aldınız") `01`, `02`, `03`, `04` ve `10`'da sekiz eşleşme buldu; `02 §3.12.9` dışındaki yedisi uyarıyı yalnız ilk cümlesiyle anıyordu ve tam metne getirildi: `02 §6.2.9` · `03 §2.3.1`, §3.2.1.9 · `10 §2` KP-9 · `04` 5.4.10 (ürün sayfasının metni; ekran E-04), 6.2.4.6 (E-04'ün durum satırı) ve §1.1'in `03 §3.2.1.9` satırı. Karar gerekmedi — metin K-236'nındır; hizalama `04 §1.3`'te "— (hizalama)" satırıdır (kalıp: audit turunun FORM-7 ve YASAL-4 satırları). 8.2.9'un hitap notu (5.4.10'da "sen" ve "siz" yan yana) değişmedi — iki cümle de "siz" der. Audit'in IC-14 ve EKRAN-M-2 izi kapandı.

**Downstream — park blokları (`05`, `06`, `07`, `08`, `12`, `DEPLOY_RUNBOOK`):** Aşama 3 kararlarının (K-723…K-849) etki sütunlarında sonraki bir dokümanı gösteren her atıf betikle çıkarıldı ve hedef dokümanın "Aşama 3'ten park edilen girdiler" bloğunda kararın numarası arandı. Gerçek atıfların hepsi karşılık buldu — `05`: K-745, K-758, K-760, K-800, K-801, K-834, K-839 · `06`: K-736, K-787…K-794 (adet), K-800, K-804, K-806, K-811, K-817 · `07`: K-737, K-787, K-794, K-802 · `08`: K-735, K-759, K-824, K-832 · `12`: K-740, K-747, K-756, K-787…K-796, K-798, K-808, K-823, K-831, K-833, K-834. İki satır park istemez ve bunu yazar: K-780 (`05` — "park gerekmez: `04 §5.21.9` işaret eder") ve K-825 (`06` — "`04 §7.3.2` işaret eder"); K-830'un `06`/`07` atfı "K-793'ün park satırları değişmedi" der. Betiğin öteki eşleşmeleri bölüm numaralarının parçasıdır (ör. "§1.1.11 (", "K-605 (") ve atıf değildir. `09`, `11` ve `DEPLOY_RUNBOOK`'u gösteren Aşama 3 kararı yok. Cross-review'ın düzeltmeleri park satırlarıyla uyumlu: `07`'nin K-802 satırı 9.8.4'ün süzgeçlerini sayar; `08`'in K-759 · K-760 satırı yeniden gönderimi B-1…B-16 ile sınırlar ve firma bildirimlerini dışarıda bırakır (2.5.4). Güncelleme gerekmedi; yeni park satırı doğmadı.

**Yeni alan ve kural taraması:** beş turda ve etki yansıtmada yeni alan, enum değeri, parametre, süre kimliği, bildirim ya da ekran eklenmedi.

**Kaynak satırları (mekanik, betikle):** betik, `04`'ün §2–§9'unda her Kaynak satırına kadarki gövdede ve Kaynak sütunu taşıyan her tablo satırında anılan Aşama 3 kararını (K-723 ve sonrası; "K-a…K-b" ve "K-a–K-b" aralıkları açılarak) Kaynak'la karşılaştırdı. Bölümlerin başındaki oturum notları ("Yazım turunun n. oturumu …") ölçüye girmez — alt bölüm değildir ve bölümün kararlarını §1.3'e gösterir. §1 matristir: satırları kaynağın kendisidir, §1.3'ün tabloları kararları içerik olarak sayar; betiğin dışında kaldı.
- **`04` — on üç yer** gövdede andığı Aşama 3 kararını Kaynak'ta taşımıyordu ve tamamlandı: 2.6 (K-776) · 6.1 (K-773) · 6.3.1.2 (K-779) · 6.3.3.5 (K-799) · 6.3.9.1 (K-798) · 6.3.9.27 (K-799) · 6.3.16.5 (K-799) · §7.2 (K-779, K-787) · §7.3 (K-736) · 8.2.9 (K-779, K-785, K-798, K-814) · 8.2.10 (K-802, K-806, K-811) · §9.5 (K-797, K-798) · §9.9 (K-791 — "K-790–K-797" aralığına katıldı). Hepsi cross-review'dan önceki yazım ve audit turlarından kalmadır; cross-review'ın değiştirdiği satırlarda eksik yoktu.
- **`02` — üç yer:** §3'ün sonu K-774'ü taşımıyordu ve kısmi adet kararlarını tek tek sayarken §3'ün gövdesinin andığı K-790, K-792, K-793'ü atlıyordu — Kaynak K-774'ü ve "K-787…K-794" aralığını taşır; §9.2'nin B-7 ve B-9 satırları gövdede andıkları K-794 ve K-790'ı Kaynak hücresine aldı.
- **`01` ve `10`:** eksik yok. `03` bölüm sonu Kaynak satırı taşımaz; tablo satırlarının Kaynak sütununda eksik yok.
- Yeniden koşumda üç dokümanda da eksik kalmadı.

**Karar kaydı:** K-849 (turdan önce) kaydedildi ve K-848'in etki sütununa geri işaret ("daraltır") bırakıldı. Etki yansıtmada karar alınmadı; K-236 Aşama 1 kaydıdır (salt okunur — K-647) ve metni değişmedi — öteki yerler ona hizalandı.

**Raporlarda taşınan açık notlar:** 1.–4. turun "önceki turlardan açık kalanlar" listesinde iki not vardı: (1) audit'in "Bu ürünü daha önce aldınız" izi — yukarıda kapandı; (2) 4. turun tekrar eden RET'i ve "sonraki adımın kararı yöneticiye bırakıldı" notu — K-849'un (1). maddesiyle karara bağlandı. Audit'in uygulanmayan sekiz bulgusu (YASAL-6, DR-2, DR-4, KAPSAM-K-7, K-9, K-13, FORM-14, FORM-17) audit raporunda gerekçeyle kapalıdır; `06`'ya bırakılan FORM-16 park satırındadır. Açık not kalmadı.

**Üst dokümanlara geri besleme (K-652):** Ürün Gereksinimleri v0.64, Kullanıcı Akışları v0.20, MVP Kapsamı v0.42 — üçünün de ✓ durumu korunur, kalite döngüsü yeniden açılmaz; Proje Vizyonu değişmedi (v0.33). Her birinin başlık notunda ve dosya sonu dipnotunda sürüm satırı var; `04 §1.3`'e "Cross-review'ın etki yansıtması" paragrafı ve tablosu girdi.

**Doküman durumu:** karar kaydının §1 tablosunda `04`'ün cross-review sütunu "✓ (5 tur — K-849)". `04`'ün durumu ⏳ kalır — ✓ aşama kapanışının arşiv adımında (K-437'nin 6. adımı; `checklists/document-stage.md` §7) konur. Checkpoint sütunu aşama kapanışında dolar.

**Açık kalan:** yok. `04`'ün kalite döngüsü tamamlandı; sırada Aşama 3'ün çakışma taraması (K-437'nin 3. adımı). Checkpoint'e (4. adım) K-849'un devri §10.1'de bekliyor.
