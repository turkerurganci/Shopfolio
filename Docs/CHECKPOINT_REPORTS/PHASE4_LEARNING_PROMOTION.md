# Aşama 4 — Öğrenim Terfisi

**Tarih:** 2026-10-17 | **Aşama:** 4 — Teknik mimari | **Kapanış adımı:** K-437'nin 5. adımı (`00 §K`; `checklists/document-stage.md` §7 madde 5)
**Girdi:** karar kaydı v0.114 — §12.1'in on iki öğrenim adayı, bekletilen Ö-23 ve Aşama 3'ün öğrenim adayı 13; Aşama 4'ün süreç kararları (K-855…K-988 içinden); [çakışma taraması](PHASE4_CONFLICT_SCAN.md) §7, [checkpoint](CP04_PHASE4_CHECKPOINT.md) §6; kalite döngüsünün raporları (`AUDIT_REPORTS/05_AUDIT.md`, `05_DEEP_REVIEW.md`, `05_GUARANTEE_LENS.md`, `CROSS_REVIEW_REPORTS/05_CROSS_REVIEW*.md` — dokuz tur); `Docs/PLAYBOOK_FEEDBACK.md` (PF-70…PF-74); yöneticinin beş koşum gözlemi; proje hafızası (`.claude/memory/`) ve kullanıcı hafızasının on iki notu — `origin/main` @ `efe981e`
**Çıktı:** karar kaydı v0.115 · `.claude/checklists/document-stage.md`, `.claude/INSTRUCTIONS.md`, `.claude/skills/audit/`, `cross-review/`, `.claude/memory/MEMORY.md`, `MEMORY_ARCHIVE.md` · `Docs/PLAYBOOK_FEEDBACK.md` 85 satır · karar K-989 — Teknik Mimari ve öteki dokümanlar, `GUARDRAILS.md`, `CONTEXT.md`, `checkpoint` ve `handoff` skill'leri değişmedi

> **Ne işe yarar:** `00 §K` gereği aşama, öğrenimi yazılmadan kapanmaz. Bu rapor Aşama 4 boyunca kayda, raporlara ve hafızaya düşen her öğrenim adayının sonucunu yazar: terfi etti mi, hangi katmana ve dosyaya, metni ne. L3–L4 hedefli terfiler bu raporun PR'ında uygulandı. `00`, `SETUP.md` ve checklist'in skill'e dönüşmesi için üç öneri GUARDRAILS §2 ve §3 gereği uygulanmadı; tam metinleriyle ve sade diliyle §4'te proje sahibinin onayını bekliyor.

---

## 1. Sonuç

| Sonuç | Sayı | Adaylar |
|---|---|---|
| **Terfi etti — bu PR'da uygulandı** (L3, L4) | 19 | Ö-60…Ö-72 · S-6, S-8, S-9 (süreç kararları) · Ö-73, Ö-75 · H-12 |
| **Önceden terfi etmişti ya da kuralı bu aşamada işledi** | 11 | S-1, S-10, S-12 · Ö-77, Ö-78 · H-1, H-3, H-4, H-9, H-10 (biçimi), H-11 (biçimi) |
| **Dokümanda yaşar** | 6 | S-2, S-3, S-4, S-5, S-7, S-11 |
| **Proje sahibinin onayını bekliyor** | 2 aday · 3 öneri | Ö-23 (checklist'in skill'e dönüşmesi — §3, §4 Öneri 3) · Ö-74 (`SETUP.md`'nin ikinci AI satırları; Ö-73'ün SETUP yüzü — §4 Öneri 2) · `00 §N.1`'e Aşama 4'ün yedi deseni (§4 Öneri 1) — **2026-10-17: kapandı (K-991)** — Öneri 1 uygulandı (`00` v1.0.6); Öneri 2 onaylanmadı — `SETUP.md` olduğu gibi kalır; Öneri 3 — checklist liste olarak kalır, bugünkü gibi devam eder (Ö-23'ün sonraki değerlendirmesi Veri modeli aşamasının öğrenim terfisinde) |
| **Tek seferlik gözlem — raporda kalır** | 1 | Ö-76 (uzun adımda ara durum) — kullanıcı hafızasına not önerisi, ikinci vakada terfi |
| **Kapısıyla bekliyor** | 1 | Ö-79 (Veri Modeli şablonunun bağımlılık alanı — PF-74; kapı Veri modeli aşamasının açılışı) |
| **Hafızada kalır** — kişisel tercih ya da referans | 5 | H-2, H-5, H-6, H-7, H-8 |

Toplam kırk beş aday: karar kaydının §12.1'inden on iki, Aşama 3'ten devralınan aday 13 ve Ö-23, süreç kararlarından on iki satır (on beş karar), bu adımda bulunan yedi ders ve kullanıcı hafızasından on iki not. Yöneticinin beş gözleminin üçü var olan adaylarla birleşti (§2.3). `CLAUDE.md` için ayrı öneri yok — oturum başlangıcı ve katman kuralı aşamanın hiçbir öğrenimiyle çelişmiyor; checklist skill'e dönüşürse oturum başlangıcının 2. maddesine bir sözcük eklenmesi Öneri 3'ün parçasıdır. Aşama 4'ün öteki kararları (K-861…K-951, K-954…K-975, K-978…K-987'nin süreç sınıfına girmeyenleri) teknik mekanizma, kalan risk ya da ürün kuralı kararıdır, dokümanlarda yaşar ve aday değildir.

**Aşamanın asıl öğrenimi.** Teknik Mimari, ürün dokümanlarının aksine kural koymayan ama **söz veren** bir dokümandı: "kaybolmaz", "tek işlenir", "silinemez", "en geç bir dakika". Kalite döngüsünün audit'i bu sözleri envantere almadı, cross-review onları dokuz turda tek tek buldu — 3.–8. turların orta bulgularının çoğu bir güvencenin mekanizmayla tutmamasıydı ve döngü ancak hedefli bir güvence merceği dokümanın kalanını da taradıktan sonra kapandı. Bu adımda mercek audit skill'ine, "sınıf toplanıyorsa mercek" kuralı cross-review'a, güvencenin mekanizmasıyla yazılması Teknik mimari hatırlatmasına girdi. Öteki dersler önceki aşamaların sürdürülmesidir: devir yükü matrissiz aşamada da matris kadar büyüktü; geri beslemenin terim taraması bu aşamada dört yeni yerden kaçtı; boyut eşiği ikinci modelin dikkatini tam ölçmedi. Aşama 3'ün terfi ettirdiği kurallar bu aşamada da iş yaptı (Ö-77) — terfi işe yarıyor. Checklist bu kapanışta yeni adım ya da madde almadı; skill'e dönüşmenin iki koşulu ilk kez birlikte karşılandı (§3).

---

## 2. Adaylar ve sonuçları

**Kimlikler:** Ö-60…Ö-71 karar kaydı §12.1'in on iki adayıdır (sırasıyla aday 1…12); Ö-72 Aşama 3'ten devralınan öğrenim adayı 13'tür (Ö numaralandırması Aşama 1–3'ten sürer; Ö-23 Aşama 1'den gelir). S-1…S-12 Aşama 4'ün süreç kararlarının satırlarıdır (bu rapora özgü). Ö-73…Ö-79 bu adımda bulunan derslerdir — kaynakları raporlar ve yöneticinin koşum gözlemleridir. H-1…H-12 kullanıcı hafızasının notlarıdır — Aşama 1–3 raporlarının numaralandırmasıyla aynı; H-12 Aşama 3'ten sonra açılan nottur.

### 2.1 Karar kaydındaki adaylar (§12.1) ve devralınanlar

| # | §12.1 | Aday | Sınıf (`00 §K`) | Sonuç | Hedef katman ve dosya | Uygulanan metin (özet) | Playbook |
|---|---|---|---|---|---|---|---|
| Ö-60 | 1 | İzlenebilirlik matrisi zorunlu olmayan aşamada devir yükü matris kadar büyük — yetmiş dokuz karar, on park maddesi, otuz beş gövde cümlesi, matrisin on üç satırı; checklist §1 açılışta devirlerin konu planına eşlenmesini söylemiyordu | İhlali önleyen kural | **Terfi etti** | L4 checklist §1 (girdi maddesi — genişletme) · §4 ("Devir taraması" — işaretçi) | Matris zorunlu olmayan aşamada etki sütunlarındaki devirler, park satırları, gövde devir cümleleri ve devir dizinleri konu planına betikle eşlenir; eşlenmeyen satır ★ konu; eşleme §4'ün devir taramasının ilk koşumudur, workshop kapanışında kapanışı arayarak yeniden koşar | PF-75 |
| Ö-61 | 2 | Devir notu kısıt sorularını workshop'a bıraktı; checklist kısıtın erken söylenmesini ister — açılış onları workshop'tan önce çok kritik soru olarak ayırdı | İhlali önleyen kural | **Terfi etti** | L4 checklist, aşama-spesifik hatırlatmalar (Teknik mimari) | Kısıt soruları (maliyet tavanı, barındırma biçimi, kurulumu ve arızayı üstlenenin yetkinliği, yığın) açılışta belirlenir, workshop'un ilk konusundan önce çok kritik soru olarak, öncülleri taranarak sorulur | PF-76 |
| Ö-62 | 3 | Çok kritik sorunun öncülü kural katmanlarına karşı taranmadı — "her işte bir insan kodu okur" (K-860); proje sahibi *"anlamadım bunu bana neden sorduğunu"* dedi. **Yöneticinin 1. gözlemi bu adayla birleşti** | İhlali önleyen kural | **Terfi etti** | L3 `INSTRUCTIONS.md` §2 ("Soru açmadan önce kural katmanlarını tara") | Sorunun öncülü de taranır: soru bir süreç ya da kural varsayımına dayanıyorsa varsayım L1–L5'te aranır; kuralda olmayan varsayım öncül olarak sorulmaz | PF-77 |
| Ö-63 | 4 | Yalnız numaralı atfın işi gerekçede saklı kaldı (K-515 → K-916) | İhlali önleyen kural | **Terfi etti** (Ö-60 ile aynı madde) | L4 checklist §1 (girdi maddesi) | Yalnız doküman numarası taşıyan atfın satırı gerekçe hücresiyle birlikte okunur, işin adı eşleme tablosuna yazılır | PF-75 |
| Ö-64 | 5 | Terim taraması Türkçe ses değişimini görmedi — "sistem kayıt" "sistem kaydında"yı bulmadı (K-916) | İhlali önleyen kural | **Terfi etti** | L4 checklist §3 (geri besleme maddesi, betik 1) | Terim listesi ses değişimli gövdeleri iki biçimiyle taşır (kayıt / kayd-, hak / hakk-); terimin kapsamı değiştiyse her eşleşme yeni anlamıyla okunur | PF-78 |
| Ö-65 | 6 | Cross-review'da iki koşum 400 KB'ın altında da kazandırdı — 300 KB'ta parçalar beş kabul edilen bulgu getirdi (K-953). **Yöneticinin 3. gözlemi bu adayla birleşti** | İhlali önleyen kural | **Terfi etti** | L4 `skills/cross-review/SKILL.md` Faz 1 madde 9 | Eşiğin altında 1. tur iki koşumla ölçer: parçalar tek parçanın görmediği bir bulguyu kabul ettirirse iki koşum sürer, ettirmezse 2. turdan tek parça yeter; ölçüm karar kaydına. **Elenen — eşiği 300 KB'a indirmek:** tek bir ölçüme dayanan yeni bir sabit olurdu; ölçüm kuralı her dokümanda kanıtı kendisi üretir. **Elenen — eşiği kaldırıp her dokümanda iki koşum:** kanıtsız ek maliyet | PF-61 (güncellendi) |
| Ö-66 | 7 | Güvence cümleleri mekanizmaya karşı okunmadı — 3.–8. turların orta bulgularının çoğu; mercek 614 güvencenin 55'inin tutmadığını, 126'sının kısmen tuttuğunu gösterdi; mercek yalnız bulguların çıktığı bölgede koşunca sınıf öteki bölgelerden çıkmaya devam etti; merceğin düzeltmesi iki metin farkı doğurdu. **Yöneticinin 2. gözlemi bu adayla birleşti** — "güvence merceği audit'e erken alınmalı mı": evet, audit'in merceği olarak, dokümanın tamamında ve cross-review'ın 1. turundan önce | İhlali önleyen kural | **Terfi etti** | L4 `skills/audit/SKILL.md` "Koşum biçimi" 1. madde (güvence merceği) ve Faz 1.2 (iç envanter) · L4 `skills/cross-review/SKILL.md` Faz 4 madde 6 (sınıf toplanıyorsa mercek) · L4 checklist §4 ("Özet tabloları…" — düzeltmenin andığı hedefler) · L4 checklist, Teknik mimari hatırlatması (güvence mekanizmasıyla yazılır) | Güvence cümleleri iç envantere girer, bileşen, ayar, sıra ve arıza hâline karşı okunur; tutmayan güvence ya mekanizmasıyla tamamlanır ya kalan risk olarak daraltılır; mercek dokümanın tamamında bir kez; art arda iki turda orta bulgular aynı sınıftaysa sonraki turdan önce sınıf dokümanın tamamında taranır. **Elenen — yazım turunun sonuna mercek eklemek:** yazım tek bağlamda yapılır ve çok mercekli denetim kalite döngüsünün işidir (checklist §4); yazımın payı güvencenin mekanizmasıyla birlikte yazılmasıdır (hatırlatma). **Elenen — checklist §4'e yeni madde:** güvence kuralı Teknik mimari hatırlatmasında ve audit'te evini bulur; yeni madde gerektirmez | PF-79 · PF-62 (güncellendi) |
| Ö-67 | 8 | Kalite döngüsünde güncellenen park satırı kararın bütün kollarını taşımadı — otuz iki kol, on yedi karar–hedef çifti; checkpoint'te düzeltmelerde yedi yüz | İhlali önleyen kural | **Terfi etti** | L4 checklist §3 (park maddesi) | Satırı güncelleyen karar ya da düzeltme her numaralı kolunu hedefe dokunup dokunmadığıyla tek tek okur; "güncellendi" işareti kolları kapsamaz | PF-80 |
| Ö-68 | 9 | Geri beslemede kuralın evi alt dokümanın Kaynak hücresinden bulunmadı (K-940 → `02 §10.4.6`) | İhlali önleyen kural | **Terfi etti** | L4 checklist §3 (betik 1) | Geri beslenen satırların Kaynak hücrelerindeki üst doküman atıfları toplanır ve kuralın yeni hâline karşı okunur | PF-78 |
| Ö-69 | 10 | Mimari doküman ürünün tanımadığı bir işlemi varsaydı (iletişim talebinin elle silinmesi) | İhlali önleyen kural | **Terfi etti** | L4 checklist §3 (edge case maddesi) · L4 `skills/audit` "Koşum biçimi" 3. madde | Öncülün bir türü ürünün işlem envanteridir: teknik doküman bir işlemi andığında işlemin varlığı ürün dokümanında okunur | PF-65 (güncellendi) |
| Ö-70 | 11 | Kalite döngüsünün düzeltmesi mimari kararın özet satırının gerekçe hücresine yansımadı (CP04 C-6) | İhlali önleyen kural | **Terfi etti** | L4 checklist §3 (betik 2) · L4 `skills/cross-review` Faz 5 madde 4 | Kararların özet tablosu olan dokümanda maddeyi değiştiren her K özet satırının gerekçe hücresinde de aranır | PF-78 |
| Ö-71 | 12 | Bir kayda yeni sınıf veren karar o sınıfı ölçüt alan kapalı listeleri güncellemedi (K-951 → K-983) | İhlali önleyen kural | **Terfi etti** | L4 checklist §3 (betik 1) | Karar bir kaydın sınıfını değiştiriyorsa (yasal kayıt, kişisel veri, sipariş tarafı) o sınıfı ölçüt alan kapalı listeler sınıfın adıyla aranır | PF-78 |
| Ö-72 | Aday 13 (Aşama 3) | Birden çok öneriyi taşıyan mesaja gelen tek "evet" her öneri için ayrı karar sayılmaz (K-722 → K-853); Aşama 4'ün açılışı üç modu üç ayrı soruyla sordu (K-855) — biçim ilk kez uygulandı ve yanlış okuma doğmadı | İhlali önleyen kural | **Terfi etti** | L3 `INSTRUCTIONS.md` §2 ("Soruları ve seçenekleri numaralandır") · L4 checklist §7 (5. adım) | Birden çok öneri ya da karar her biri ayrı numaralı soru olur, seçenekleri yönü açık yazılır; tek "evet" birden çok öneriye okunmaz, belirsiz cevap kaydedilmeden teyit edilir. **Elenen — `INSTRUCTIONS §9` işletim kuralı 1:** kural yönetici düzenine özgü değildir, her soru biçimine uygulanır | PF-81 |
| Ö-23 | — | Checklist'in skill'e dönüştürülmesi | Karar | **İki koşul karşılandı — dönüşüm proje sahibine öneri** | L4 checklist başlık notu · öneri §4 Öneri 3 | §3'te gerekçesiyle | PF-32 (güncellendi) |

### 2.2 Karar kaydında yaşayan süreç kararları

Checklist §7 madde 5: aşamanın kayıtları arşivlenince otoriterliği biter (K-436, K-647); kural L1–L5'te değilse sonraki aşama onu bilmez. Aşama 4'ün yüz otuz dört kararı tarandı: gerekçe hücresinin sınıf cümlesiyle ("Süreç kararıdır", "Süreç ve yerleşim kararıdır", "Yerleşim kararıdır", "Yazım konvansiyonudur", "Yazım düzeni kararıdır", "Hizalamadır") betik on iki karar süzdü — K-856…K-860, K-930, K-931, K-952, K-953, K-976, K-977, K-988 —; konu hücreleri okunarak üç karar eklendi — K-855 (çalışma modları) ile K-888 ve K-951 (adımın ortasında doğan çok kritik sorular). Kalan yüz on dokuz karar teknik mekanizma, kalan risk ya da ürün kuralıdır (gerekçe hücresi "Teknik mekanizmadır" ya da ⚠ sınıfı).

| # | Karar | Konu | Sonuç | Hedef | Uygulanan metin (özet) |
|---|---|---|---|---|---|
| S-1 | K-855 | Çalışma modları — ⚠ öneriyle kayıt, merge yetkisi, yönetici + alt ajan düzeni Aşama 4 boyunca | **Önceden terfi etmişti** — aşama sonu notu | L3 `INSTRUCTIONS.md` §2, §9 · L4 checklist §1 | Üç düzen Aşama 4'le sınırlıdır, Veri modeli aşamasının açılışında yeniden sorulur; kural yazılıydı. Üç ayrı soruyla soruldu — Ö-72'nin kanıtı |
| S-2 | K-856 | Teknik Mimari'nin iskeleti; workshop kısıtlardan ve yığından başlar | **Dokümanda yaşar** (`05` başlık notu) | — | "Kısıtlardan başla" yüzü Ö-61 ile hatırlatmaya girdi; blok numarasının şablonun bölüm numarası olması K-724'ün kalıbıdır |
| S-3 | K-857 | Şablonun taşımadığı üç yer — konvansiyonlar, geri besleme tablosu, kısıtlar | **Dokümanda yaşar** (`05` başlık notu, §0) | — | Şablon yüzü PF-71; checklist §1'in şablon taramasına kanıt (Ö-77) |
| S-4 | K-858 | Mimari karar kimliği `MK-nn` | **Dokümanda yaşar** (`05 §2`, §12) | — | Şablon yüzü PF-73; önek taraması (checklist §1) işledi — `MK-` sıfır eşleşme |
| S-5 | K-859 | Dal stratejisinin §8'deki yazım biçimi — işaret, kopya değil | **Dokümanda yaşar** (`05 §8`) | — | Şablon notu PF-72; checklist §4'ün "Kural ayrıntısı evinde kalır" maddesi işledi |
| S-6 | K-860 | Kodu kim inceler — soru geri çevrildi; kısıt hizalandı | **Terfi etti** (Ö-62 ile) | L3 `INSTRUCTIONS.md` §2 | Ö-62'de |
| S-7 | K-930, K-931 | On iki yazım konvansiyonu; alt bölüm haritası | **Dokümanda yaşar** (`05` başlık notu) | — | Genel biçimi checklist §4'te (K-669, K-670, K-682) — kural işledi |
| S-8 | K-952 | Ciddiyet ölçüsü Teknik Mimari'ye 1. turdan; mimari için okuma paragrafı | **Hizalama terfisi** — skill'in ikinci koşulu "kapsam, özet, akış ya da arayüz dokümanı" diyordu | L4 `skills/cross-review` "Ciddiyet ölçüsü" | İkinci koşul mimari dokümanını da sayar; mekanizma kararı koyan dokümanda istemin okuma paragrafı — güvencenin mekanizmayla tutmaması (4)'tür, savunmada derinlik bulgu değildir |
| S-9 | K-953 | Eşiğin altında iki koşum; eşiğin yeniden okunması | **Terfi etti** (Ö-65 ile) | L4 `skills/cross-review` Faz 1 madde 9 | Ö-65'te |
| S-10 | K-976 | Cross-review çıkış kuralıyla kapanır (9 tur) | **Kural işledi** — kanıt eklendi | L4 `skills/cross-review` Faz 4 madde 5 | Kuralın ikinci uygulaması; devredilen sınıf checkpoint'te altmış sekiz yerde bulundu — devir yine zorunluydu |
| S-11 | K-977, K-988 | Teknik kavramların kod adının evi Veri Modeli; sözlük kuralının kapsamı | **Dokümanda yaşar** (`02 §1.1`; `06`, `07`, `09` park satırları) | — | Ürün dokümanının kuralıdır; L1–L5'e gerek yok |
| S-12 | K-888, K-951 | Yazımın ve audit'in ortasında doğan iki çok kritik soru — bugünkü kural yazıldı, soru yöneticiye gitti, cevap ayrı adımda kayda | **Kural işledi** (`INSTRUCTIONS.md` §9, işletim kuralı 2) | — | Aşama 3'te terfi eden kural iki kez işledi; adımlar durmadı |

### 2.3 Bu adımda bulunan adaylar

**Yöneticinin beş gözlemi:** (1) "kodu kim inceleyecek" sorusu → Ö-62 ile birleşti; (2) güvence merceğinin erken alınması → Ö-66 ile birleşti; (3) 400 KB eşiğinin altında parçalı koşum → Ö-65 ile birleşti; (4) uzun adımda ara durum mesajı → Ö-76; (5) Codex'in web araması → Ö-73.

| # | Aday | Kaynak | Sınıf | Sonuç | Hedef | Uygulanan metin (özet) | Playbook |
|---|---|---|---|---|---|---|---|
| Ö-73 | **İkinci modelin web araması ve kaynağı** — `--ignore-user-config` aramayı kapatmıyor; dokuz turun sekizinde en az bir çağrı arama yaptı ve 7. turda bir bulgu mevzuat iddiasını resmî olmayan bir derleme sitesine (`mevzuat.mturkoglu.av.tr`) dayandırdı. Skill "hukuki kontrol"ü anıyor ama tanımlamıyordu; SETUP'ın koşum satırı aramadan söz etmiyor | `05_CROSS_REVIEW.md` §1, `_R2.md`…`_R9.md` §1 ("Web araması" satırları), `_R7.md` §1; yöneticinin 5. gözlemi | İhlali önleyen kural | **Terfi etti** (L4) · SETUP yüzü **öneri** (Ö-74 ile, §4 Öneri 2) | L4 `skills/cross-review` Faz 2 madde 5 | Yasal dayanak taşıyan her bulgu resmî metnin güncel hâline karşı okunur; derleme ya da yorum sitesine dayanan iddia doğrulanmadan kabul edilmez; rapor başlığı aramayı yapan çağrıyı ve kaynağı yazar; kamuya açık mevzuat proje bilgisi değildir. **Elenen — aramayı kapatma talimatı:** kapatan bir seçenek bu turlarda doğrulanmadı ("doğrulanamadı"); aramanın bulduğu resmî metin (mevzuat.gov.tr) bulguyu güçlendirdi — risk aramada değil, kaynağın okunmamasındadır | PF-82 |
| Ö-74 | **SETUP'ın ikinci AI satırları ters** — SETUP birincil yöntemi `cursor-agent`, yedeği Codex CLI diye yazar; `CROSS_REVIEW_REPORTS`'un yetmiş sekiz turunun yalnız yedisi (Proje Vizyonu 1.–7.) `cursor-agent` ile koştu, yetmiş biri Codex ile — Aşama 1'in ortasından beri asıl yol Codex'tir (kullanıcı hafızası: "2026-10-02'den beri asıl yol"). Aşama 3'ün terfisi SETUP'ı "uyumlu" saymıştı (skill'in yedek yöntem cümlesi karşılıyor) | Raporların başlıkları (betik: ilk eşleşen model adı); H-8 | Hizalama — SETUP | **Öneri** (§4 Öneri 2) | `SETUP.md` kayıt tablosu | Uygulanmadı — GUARDRAILS §2 | PF-83 |
| Ö-75 | **Betiğin sınanması iki yönlü olmalı** — güvence envanteri betiğinin ilk sürümü "her …", "beklenmez" taşıyan maddeleri kaçırdı (kapsam karşılaştırması buldu); Kaynak betiğinin üç ilk sürümü örneklemde düzeltildi ve atıf betiği bilerek bozulmuş dört atıfla denendi; etki yansıtmanın park betiği gerçek bir eksiği (K-900'ün Entegrasyon Spesifikasyonu hedefi) "yanlış pozitif" diye eledi ve ancak çakışma taraması buldu. `audit` "Koşum biçimi" 4. madde yalnız eşleşmelerin örneklenmesini (fazla) söylüyordu | `05_GUARANTEE_LENS.md` (2. koşum, "Doğrulama"); CP04 Notlar; `PHASE4_CONFLICT_SCAN.md` §5.3 (c) | İhlali önleyen kural | **Terfi etti** | L4 `skills/audit` "Koşum biçimi" 4. madde | Fazla için eşleşme örneklenir; eksik için betik bilerek bozulmuş ya da bilinen eksik örnekle denenir, kapsamın numaralı maddeleri çıktıyla karşılaştırılır; "yanlış pozitif" diye elenecek satır okunarak elenir | PF-84 |
| Ö-76 | **Uzun adımda ara durum** — proje sahibi bir ara "neredesin" diye sordu; bir adım yaklaşık bir saat sürdü ve yönetici ara durum vermedi. `INSTRUCTIONS §9` işletim kuralı 1 proje sahibinden *cevap beklenen* şeyleri sınırlar, bilgi mesajını yasaklamaz; ihlal edilen bir kural yok | Yöneticinin 4. gözlemi | Tek seferlik gözlem | **Raporda kalır** — kullanıcı hafızasına not (yönetici yazar); ikinci kez yaşanırsa `INSTRUCTIONS §9` işletim kuralı 1'e terfi eder (`00 §G.3`) | Kullanıcı hafızası (yeni not önerisi, §7) | — | — |
| Ö-77 | **Aşama 3'ün terfileri bu aşamada işledi** — park etiketi ↔ etki sütunu iki yönlü betiği her kalite döngüsü adımında koştu ve çakışma taramasında yalnız bir eksik bıraktı; kararın olgusal öncülü kuralı iki yeni öncül buldu (K-978, K-979); şablon taraması açılışta dört eksik buldu (PF-70…PF-73); yazım ve audit ortasında doğan iki çok kritik soru adımı durdurmadı (K-888, K-951); her PR'da CR ve `\$[0-9]` taraması koştu | `PHASE4_CONFLICT_SCAN.md` §7; §12.1'in adım maddeleri | Kural işledi | **Önceden terfi etmişti** — kanıt | L4 checklist §1 ("Şablonu tara" — beşinci şablonun kanıtı eklendi) | Desen 7'nin kanıtı (§4 Öneri 1) | — |
| Ö-78 | **Düzeltmenin bölge bölge şartnameyle uygulanması** — checkpoint'te beş alt ajan bölgelerine birebir değiştirme şartnamesi yazdı, yönetici 268 değiştirmeyi tam eşleşme isteyen tek betikle uyguladı; çakışma yok | CP04 §6 (tek seferlik gözlem) | Kural işledi | **Önceden terfi etmişti** (`INSTRUCTIONS.md` §7 — "toplu düzeltme … her değiştirmede tam bir eşleşme isteyen bir betikle"; Aşama 3, Ö-53) | — | — | — |
| Ö-79 | **Veri Modeli şablonunun bağımlılık alanı** henüz yazılmamış Kodlama Kılavuzu'nu sayar | CP04 §3.5 (C-82) | Şablon kusuru | **Kapısıyla bekliyor** — Veri modeli aşamasının açılışının şablon taraması (checklist §1) | — | — | PF-74 (değişmedi) |

### 2.4 Hafızadaki adaylar

CLAUDE.md: *"Bir süreç kuralı yalnız hafızada yaşıyorsa kırılgandır — L1–L5'e terfi eder."* `00 §G.3`: bir hafıza notu ikinci kez bir süreç ihlalini önlemek için kullanılıyorsa kuraldır. Proje hafızasında (`.claude/memory/`) `MEMORY.md`, `MEMORY_ARCHIVE.md` ve `README.md` dışında not yok. Kullanıcı hafızası (`~/.claude/projects/c--projects-Shopfolio/memory/`) bu adımda okundu, **yazılmadı** — güncellenmesi gereken notlar §7'de.

| # | Not | Aşama 4'teki değişiklik | Sonuç | Hedef | Hafızada kalan |
|---|---|---|---|---|---|
| H-1 | Workshop sorusu öncesi template taraması | Kural işledi — devir §11'in altı maddesi sorulmadı (K-855); ama sorunun **öncülü** taranmadı (K-860) — Ö-62 kuralı genişletti | **Önceden terfi etmişti** (`INSTRUCTIONS §2`); genişleme Ö-62 | — | İşaretçi; üçüncü vakanın satırı önerilir (§7) |
| H-2 | Kısa diyalog tarzı workshop sorusu | — | **Hafızada kalır** — kişisel üslup | — | Soru biçimi ve uzunluğu |
| H-3 | CI beklerken izleyiciye güvenme | — | **Önceden terfi etmişti** (`INSTRUCTIONS §3.2`) | — | Hatırlatıcı |
| H-4 | Toplu onayda ⚠ maddeleri ayrı işaretle | — | **Önceden terfi etmişti** (`INSTRUCTIONS §2`) | — | Hatırlatıcı |
| H-5 | Limit dolarsa kaldığın yerden devam | — | **Hafızada kalır** — kişisel tercih | — | Tamamı |
| H-6 | Yazım oturumunda tek bağlam | — | **Hafızada kalır** (kuralı checklist §4'te) | — | Oturum ayarı |
| H-7 | Sonraki chat mesajı | Yönetici düzeninde adımlar alt ajanla yürüdü; kullanılmadı | **Hafızada kalır** — kişisel tercih | — | Tamamı |
| H-8 | Cross-review ikinci model koşumu | Dokuz tur Codex ile koştu; web araması açık (Ö-73); not Codex'i "asıl yol" der, SETUP "yedek" der (Ö-74) | **Hafızada kalır** — referans; koşumun kuralları skill'e terfi etti (Ö-65, Ö-73); SETUP hizası **öneri** (§4 Öneri 2) | — | Çağrı kalıbı, model ve hesap bilgisi; güncellenmesi önerilir (§7) |
| H-9 | Dokümanları isimleriyle, kısa ve açık yaz | — | **Önceden terfi etmişti** (`INSTRUCTIONS §2`) | — | Hatırlatıcı |
| H-10 | Kapanış otomasyonu yetkisi | Aşama 4 için yenilendi (K-855) — notta Aşama 4 satırı yok | **Biçimi önceden terfi etmişti** (`INSTRUCTIONS §9`); **yetki süreli** — Veri modeli aşamasının açılışında yeniden sorulur | — | Yetkinin kendisi; güncellenmesi önerilir (§7) |
| H-11 | Yalnız çok kritik konuları sor | Aşama 4 için yenilendi (K-855) — notta Aşama 4 satırı yok | **Biçimi önceden terfi etmişti** (`INSTRUCTIONS §2`, §9); **mod süreli** | — | Modun kendisi; güncellenmesi önerilir (§7) |
| H-12 | Toplu öneride "evet" ret sayılmaz | Aday 13'ün kaynağı; Aşama 4'ün açılışında uygulandı (K-855) — ikinci kullanım | **Terfi etti** (Ö-72 ile) | L3 `INSTRUCTIONS.md` §2 · L4 checklist §7 | `00`/SETUP önerisini sade anlatma ve "hangi adımlar" ölçüsü (kuralı checklist §7'de); `Terfi` satırı önerilir (§7) |

---

## 3. Ö-23 — checklist skill'e dönüşür mü

> **Sonuç — 2026-10-17 (K-991):** proje sahibi dönüşmemesini seçti — *"Hayır, liste olarak kalsın — bugünkü gibi devam eder"*. Ölçüt ve yeniden değerlendirme checklist'in başlık notunda değişmeden kalır; nota yalnız bu kapanışın sonucu eklendi; sonraki değerlendirme Veri modeli aşamasının öğrenim terfisindedir. Aşağıdaki metin 5. adımın hâlidir, olduğu gibi bırakıldı.

**Sonuç: iki koşul da karşılandı — dönüşüm proje sahibine öneri olarak sunulur (§4 Öneri 3); bu PR'da uygulanmadı.** Ölçüt iki koşuldur ve ikisi birlikte aranır (checklist başlık notu, L4; K-722, K-853). Değerlendirme betikle yapıldı:

| Koşul | Sonuç | Kanıt |
|---|---|---|
| **1. Checklist izlenebilirlik matrisi zorunlu bir aşamada da işletilmiş olmalı** | ✓ **Karşılandı** — Aşama 3'te, kalıcı | `PHASE3_LEARNING_PROMOTION.md` §3: §2'nin yedi maddesi Arayüz Tanımları'nda işletildi (`04 §1`). Aşama 4'te matris zorunlu değildi (`00 §C.1`) |
| **2. Bir aşama kapanışında yapısal değişiklik — yeni adım ya da yeni madde — almamış olmalı** | ✓ **Karşılandı** — ilk kez | `grep -c '^- \[ \]'`: Aşama 3'ün kapanışından sonra **66** · Aşama 4 boyunca değişmedi — `git log 3e3a8e6..efe981e -- .claude/checklists/` boş · **bu kapanıştan sonra 66**. Bölüm bazında önce ve sonra aynı: §1 8 · §2 8 · §3 19 · §4 12 · §5 3 · §6 9 · §7 7. Adım sayısı (§7'nin altı adımı) aynı. Değişen: **sekiz madde genişletmesi** — §1'de iki (girdi maddesi: matrissiz aşamada devirlerin eşlenmesi — Ö-60, Ö-63; şablon taraması: beşinci şablonun kanıtı — Ö-77), §3'te üç (edge case: işlem envanteri — Ö-69; park: kararın bütün kolları — Ö-67; geri besleme: betik 1'e üç yüz, betik 2'ye özet tablo — Ö-64, Ö-68, Ö-70, Ö-71), §4'te iki (devir taraması: işaretçi; özet tablolar: düzeltmenin andığı hedefler — Ö-66), §7'de bir (5. adım: öneri başına ayrı soru — Ö-72) —, Teknik mimari hatırlatmasının satırı (Ö-61, Ö-66) ve başlık notu |

**Dürüstlük notu — sınır vakası.** Koşulun sözü "yeni adım ya da yeni madde"dir ve Aşama 3'ün kararı onu içerikle okudu: yeni bir yükümlülüğü var olan bir maddeye sıkıştırıp koşulu karşılanmış saymak "ölçütü biçimle atlatmak" olurdu (K-852). Bu adımın her terfisi bu ölçüyle sınandı. En yakın sınır **Ö-60**'tır — açılışta devirlerin konu planına betikle eşlenmesi: yükümlülüğün konusu (gelen her devrin karşılık bulması) checklist'te zaten iki maddede yaşıyor — §1'in girdi maddesi matrisli aşamanın karşılığını yazıyordu, §4'ün devir taraması her devrin karşılığını arıyordu; yeni olan, matrissiz aşamada bu taramanın açılışta başlamasıdır. Ayrı bir madde açmak aynı yükümlülüğü ikinci kez yazmak olurdu. İkinci sınır **Ö-66**'nın yazım yüzüdür (güvence mekanizmasıyla yazılır): checklist §4'e yeni madde olarak değil, aşamaya özgü hatırlatmaya girdi — sınıf teknik mimariye özgüdür ve denetimi audit'in merceğidir. Öteki altı genişletme (öncül, park, betik 1 ve 2, özet tablolar, öneri başına soru) var olan maddelerin kendi konusunu inceltir — Aşama 3'te de on bir genişletme yapısal sayılmamıştı. Okur bu iki sınır vakasından birini yeni madde sayarsa ikinci koşul karşılanmamış olur ve Ö-23 bir aşama daha bekler; öneri bunu proje sahibine açıkça söyler (§4 Öneri 3).

**Neden şimdi önerilir.** Ölçütün koruduğu durum — işletilmemiş bir bölümü skill olarak dondurmak — artık geçerli değil: yedi bölümün hepsi en az bir aşamada işletildi, §2 dahil. Skill'ler de her öğrenim terfisinde güncelleniyor (`audit` ve `cross-review` dört kapanışın dördünde değişti); dönüşüm içeriği dondurmaz, yalnız evini değiştirir. Kazanç: oturum başlangıcının 2. maddesi (`CLAUDE.md`) başlangıç kapılarını `.claude/skills/`'ten okur ve skill'ler her oturumda adıyla listelenir; checklist ise yalnız `00 §C.7` ve `INSTRUCTIONS §10`'un işaretiyle bulunuyor. Bedel: tek bir yapısal PR — dosya yolu otuz üç dosyada iki yüz yirmi dört kez anılıyor, arşivlenmiş kayıtlar dahil; bölüm numaraları korunur ve eski dosya yeni yeri gösteren bir işaretçi olarak kalırsa hiçbir atıf kırılmaz.

**Elenen — dönüşümü bu PR'da uygulamak:** yeni skill yaratmak yapısal değişikliktir ve onay ister (GUARDRAILS §3); `00 §C.7`'nin metni değişir (GUARDRAILS §2). **Elenen — koşullar karşılandığı hâlde yeniden bekletmek:** ölçüt proje sahibinin kabul ettiği iki koşuldur; karşılandığında karar ona aittir (K-722'nin kalıbı).

---

## 4. Proje sahibinin onayını bekliyor — `00`, `SETUP.md`, checklist'in skill'e dönüşmesi

> **Sonuç — 2026-10-17 (K-991; ⚠ listesiyle aynı mesajda, ayrı numaralı sorular):** **Öneri 1 — uygulandı:** *"1 · Evet, eklensin"* — yedi desen `00 §N.1`'in "Aşama 4 — Teknik mimari" paragrafına aşağıdaki metinle girdi; `00` v1.0.6 (başlık ve alt bilgi; başka değişiklik yok). **Öneri 2 — uygulanmadı:** *"2 · Hayır, olduğu gibi kalsın"* — `SETUP.md`'nin iki satırı değişmedi; karar yalnız SETUP'ın metnine ilişkindir, çalışma biçimi aynıdır (cross-review Codex CLI ile koşar; hukuki kontrol `cross-review` Faz 2 madde 5'te). **Öneri 3 — uygulanmadı:** *"2 · Hayır, liste olarak kalsın"* — seçeneğin proje sahibine giden sözü *"bugünkü gibi devam eder"*di; bu yüzden aşağıdaki "Reddedilirse (seçenek 2)" paragrafının *"ölçüt … diye kapanır"* okuması uygulanmadı: ölçüt ve yeniden değerlendirme checklist'in başlık notunda yürürlükte kalır (§3'ün sonuç notu). Aşağıdaki metin 5. adımın hâlidir, olduğu gibi bırakıldı.

GUARDRAILS §2: `00`, `CLAUDE.md` ve `SETUP.md` proje sahibinin **açık onayı** olmadan değişmez; GUARDRAILS §3: yeni skill yaratmak yapısal değişikliktir. Aşağıdaki üç öneri bu PR'da **uygulanmadı**. Her biri proje sahibine **ayrı numaralı soru** olarak gider (`INSTRUCTIONS §2`; Ö-72) ve onay gelirse ayrı bir `docs:` PR'ında uygulanır. Hiçbiri aşamanın kapanışını engellemez: kurallar L3–L4'te yürürlükte.

### Öneri 1 — `00 §N.1`'e Aşama 4'ün desenleri

**Sade dille.** *Sorun:* Yöntem dokümanının her aşamanın derslerini yazdığı bölümünde Teknik Mimari aşamasının dersleri henüz yok. *Ne değişir:* Bu aşamanın yedi dersi — örneğin "kesin söz veren cümleler onları sağlayan düzenekle birlikte yazılır ve ayrıca kontrol edilir" — önceki üç aşamadaki gibi o bölüme eklenir; reddederseniz dersler yalnız bu raporda kalır.

**Önerilen metin** — `§N.1`'in "Aşama 3 — UI/UX tasarım" paragrafının ve yedi maddesinin **ardına**:

```markdown
**Aşama 4 — Teknik mimari (2026-10-16 → 2026-10-17).** Kurallar `.claude/checklists/document-stage.md` §1, §3, §4, §7 ve aşama hatırlatmaları, `.claude/INSTRUCTIONS.md` §2 ve `audit`, `cross-review` skill'lerine terfi etti; rapor `Docs/CHECKPOINT_REPORTS/PHASE4_LEARNING_PROMOTION.md`. Tekrarlanacak desenler:

1. **Kesin güvence, onu sağlayan düzenekle birlikte yazılmazsa ikinci model onu tur tur bulur; hedefli bir mercek sınıfı bir kerede kapatır.** Mimari doküman "kaybolmaz", "tek işlenir", "silinemez" gibi güvenceler koydu; cross-review'ın 3.–8. turlarının orta bulgularının çoğu bir güvencenin mekanizmayla tutmamasıydı ve ikinci model her turda başka bir bölgede bir tanesini buldu. Güvence cümlelerini envantere çıkarıp mekanizmaya karşı okuyan mercek 614 cümlenin 55'inin tutmadığını, 126'sının kısmen tuttuğunu gösterdi; mercek dokümanın kalanında da koşunca ikinci model bu sınıftan bulgu getirmedi ve döngü kapandı. Mercek yalnız bulguların çıktığı bölgede koşarsa sınıf öteki bölgelerden çıkmaya devam eder — dokümanın tamamında, ilk turdan önce koşar.
2. **İzlenebilirlik matrisi zorunlu olmayan aşamada da devir yükü matris kadar büyüktür.** Mimari dokümana yetmiş dokuz karar, on park maddesi, otuz beş gövde cümlesi ve matrisin on üç satırı devredilmişti; açılış bunları konu planına betikle eşleyince hiçbiri sahipsiz kalmadı, ama yalnız numara taşıyan bir atfın işi bir gerekçe hücresinde saklı kaldı ve ancak kaynak tam okunurken bulundu. Aşama 1'in 7. ve Aşama 2'nin 5. deseninin devamıdır: devrin adı yazılmadıkça eşleme de onu adıyla göremez.
3. **Kısıt soruları açılışta ve öncülüyle sorulur.** Maliyet, barındırma ve yığın soruları workshop'tan önce soruldu ve mimarinin bütün kararlarını belirledi; proje sahibi maliyet tavanında önerinin tersini seçti. Bir soru ise kurallarda olmayan bir öncüle ("her işte bir insan kodu okur") dayandı ve geri döndü — soru taraması yalnız cevabı değil, sorunun öncülünü de kurallara karşı okur.
4. **Kuralın yüzlerinin listesi kapalı değildir; her aşama terim taramasının kaçırdığı yeni bir yüz buldu.** Bu aşamada dört: ünsüz değişimine uğrayan terim, kuralın alt dokümanların kaynak hücresinden bulunan evi, kararın verdiği sınıfı (yasal kayıt) ölçüt alan liste ve kararların özet tablosundaki gerekçe hücresi. Aşama 2'nin 2. ve Aşama 3'ün 3. deseninin devamıdır.
5. **Boyut eşiği ikinci modelin dikkatini tam ölçmez; ilk tur ölçer.** 300 KB'lık doküman parçalı koşumun eşiğinin altındaydı; ilk turda iki koşumla ölçüldü ve parçalar tek çağrının görmediği beş bulgu getirdi — beşi kabul edildi. Aşama 3'ün 2. deseninin devamıdır.
6. **Kalite döngüsünün kendi düzeltmesi yeni yüzey açar.** Döngünün güncellediği park satırlarında kararların otuz iki kolu eksik kaldı; güvence merceğinin düzeltmesi iki yeni metin farkı doğurdu. Düzeltme de taranır: kararın her kolu ve düzeltmenin andığı her hedef.
7. **Terfi edilen kural üçüncü aşamada da iş yaptı; yönetici düzeni üçüncü kez tuttu.** Önceki aşamanın kuralları — park satırının iki yönlü eşlenmesi, kararın öncülünün kuralın evinde okunması, şablon taraması, yazımın ortasında doğan kritik sorunun adımı durdurmaması — bu aşamada iki yeni öncül, açılışta dört şablon eksiği ve adımı durdurmayan iki çok kritik soru verdi. Açılıştan checkpoint'e on dokuz adım iki günde yürüdü; proje sahibine altı çok kritik soru ve sorulmadan kaydedilen kırk dört kararın listesi gitti. Aşama 2'nin 6. ve Aşama 3'ün 5. ve 7. desenlerinin devamıdır.
```

**Kaynak:** Ö-66 (desen 1) · Ö-60, Ö-63 (2) · Ö-61, Ö-62 (3) · Ö-64, Ö-68, Ö-70, Ö-71 (4) · Ö-65 (5) · Ö-67, Ö-66 (6) · Ö-77, S-12, K-855 (7).

**Sürüm satırları:** başlık `**Versiyon: v1.0.5** | … | **Son güncelleme:** 2026-10-05` → `**Versiyon: v1.0.6** | … | **Son güncelleme:** <onay tarihi>`; alt bilgi `*Project Playbook — Metodoloji v1.0.5*` → `*Project Playbook — Metodoloji v1.0.6*`. Öneri 3 de onaylanırsa `§C.7`'nin değişikliği aynı sürüme girer.

**Reddedilirse:** madde gerekçesiyle kapanır; desenler bu raporda kalır (checklist §7 madde 5; K-722).

### Öneri 2 — `SETUP.md`: ikinci AI satırlarının yer değiştirmesi ve web araması notu

**Sade dille.** *Sorun:* Kurulum dosyası ikinci görüş için asıl aracı Cursor, yedeğini ChatGPT diye yazıyor; oysa Proje Vizyonu'nun ortasından beri bütün kontroller ChatGPT ile yapıldı ve ChatGPT internette arama da yapabiliyor — bir kez yasal bir iddiayı resmî olmayan bir siteye dayandırdı. *Ne değişir:* Dosyada ChatGPT asıl, Cursor yedek olur ve "ChatGPT internette arayabilir, yasal iddiaları resmî metinle kontrol ediyoruz" notu girer; çalışma biçimi değişmez.

**Bugünkü metin** — "Doldurulan parametreler (kayıt)" tablosunun iki satırı:

```markdown
| İkinci AI (cross-review) | **`cursor-agent`** (CLI, hesap tabanlı auth — API key kullanılmıyor). Raporlar `Docs/CROSS_REVIEW_REPORTS/` altına yazılır. | 2026-08-11 |
| İkinci AI — yedek yöntem | **Codex CLI, ChatGPT hesabıyla** (`codex exec`, model `gpt-5.6-terra`, düşünme düzeyi `high`). `cursor-agent`'ın Cursor kullanım limiti dolduğunda kullanılır. Koşum: boş bir klasörde, salt okunur (`-s read-only`), kişisel ayarlar yüklenmeden (`--ignore-user-config`). Talimat, doküman ve şablon stdin'den tek metin olarak verilir, karar kaydı verilmez (K-431). Talimatta şu cümle bulunur: *"dokümanda bilinçli karar olarak yazılmış seçime yalnız katılmadığın için bulgu yazma"*. Raporlar aynı dizine yazılır; raporun başlığı turun hangi modelle koştuğunu söyler. **Kaynak:** Proje Vizyonu'nun cross-review'ı, 8.–18. tur — proje sahibinin kararı. | 2026-10-03 |
```

**Önerilen metin:**

```markdown
| İkinci AI (cross-review) | **Codex CLI, ChatGPT hesabıyla** (`codex exec`, model `gpt-5.6-terra`, düşünme düzeyi `high`) — birincil yöntem. Koşum: boş bir klasörde, salt okunur (`-s read-only`), kişisel ayarlar yüklenmeden (`--ignore-user-config`). Talimat, doküman ve şablon stdin'den tek metin olarak verilir, karar kaydı verilmez (K-431). Talimatta şu cümle bulunur: *"dokümanda bilinçli karar olarak yazılmış seçime yalnız katılmadığın için bulgu yazma"*. **Web araması:** bu koşum kalıbı aramayı kapatmaz — ikinci model mevzuat ve araç belgesi arayabilir; yasal dayanak taşıyan bulgu resmî metne karşı okunur (`cross-review` skill'i, Faz 2 madde 5). Raporlar `Docs/CROSS_REVIEW_REPORTS/` altına yazılır; raporun başlığı turun hangi modelle koştuğunu ve hangi çağrının web araması yaptığını söyler. **Kaynak:** Proje Vizyonu'nun 8. turundan bu yana yetmiş bir turun hepsi bu yolla koştu (Proje Vizyonu, Ürün Gereksinimleri, MVP Kapsamı, Kullanıcı Akışları, Arayüz Tanımları, Teknik Mimari) — proje sahibinin kararı (2026-10-02); birincil yönteme geçiş <onay tarihi>. | <onay tarihi> |
| İkinci AI — yedek yöntem | **`cursor-agent`** (CLI, hesap tabanlı auth — API key kullanılmıyor; ücretsiz planın kullanım limiti vardır — Proje Vizyonu'nun 8. turunda doldu). Codex'in limiti dolduğunda ya da hesabı açılmadığında kullanılır; Proje Vizyonu'nun 1.–7. turları bu yolla koştu. Raporlar aynı dizine yazılır. | 2026-08-11 · yedeğe geçiş <onay tarihi> |
```

**Elenen — yalnız web araması notunu eklemek:** satırların sırası Aşama 1'in ortasından beri fiilî kullanımın tersini söylüyor; skill'in Faz 1 madde 1'i ("birincil yöntemin limiti dolarsa SETUP'ın yedek yöntemiyle") her turda yedek yöntemle koşmayı "limit doldu" diye okutuyor. **Elenen — hiçbir şey yapmamak (Aşama 3'ün okuması):** iki satır yan yana durduğu için yöntem bulunuyor, ama birincil satır Proje Vizyonu'nun 7. turundan beri kullanılmayan bir aracı gösteriyor. **Reddedilirse:** satırlar olduğu gibi kalır; web araması kuralı skill'de (Faz 2 madde 5) yürürlüktedir.

### Öneri 3 — Checklist'in skill'e dönüşmesi (Ö-23)

**Sade dille.** *Sorun:* Doküman aşamalarının adım adım kontrol listesi "henüz oturmadı" diye ajanın her oturumda kendiliğinden tanıdığı bir beceriye çevrilmedi; koyduğumuz iki şart — en zor aşamada denenmiş olmak ve bir aşama kapanışında yeni adım almamak — bu aşamada ilk kez birlikte sağlandı. *Ne değişir:* Liste içeriği ve bölüm numaraları aynı kalarak beceriye dönüşür, eski dosya yeni yeri gösteren kısa bir not olur; reddederseniz liste olduğu gibi kalır.

**Seçenekler** (proje sahibine ayrı numaralı soru): **1 ·** dönüştür (öneri) · **2 ·** checklist olarak kalsın — Ö-23 kapanır, istenirse sonra açılır · **3 ·** bir aşama daha beklensin — Veri modeli aşamasının öğrenim terfisinde, matrisin ikinci kez işletilmesinden sonra yeniden okunur. **Not:** bu kapanışın iki genişletmesi (§3'ün sınır vakaları) yeni madde sayılırsa ikinci koşul karşılanmamış olur — o okumada doğru cevap 3'tür.

**Onay gelirse uygulanacak değişiklikler** (ayrı bir `docs:` PR'ı; GUARDRAILS §2 ve §3):

1. **Yeni skill** `.claude/skills/document-stage/SKILL.md` — frontmatter (`name: document-stage`; `description`: "Doküman üretim aşamasının adımlarını işletir — açılış, izlenebilirlik matrisi, workshop, yazım, tamamlama protokolü, kalite döngüsü ve altı adımlı kapanış. Kullan: 'aşamayı aç', 'workshop', 'yazım turu', 'kapanış adımı', /document-stage."; `user-invocable: true`) ve checklist'in bugünkü gövdesi **bölüm numaraları korunarak** (§1–§7, aşama-spesifik hatırlatmalar). Başlık notu ("Bu neden skill değil?") skill'in kökenini anlatan iki cümleye iner; Ö-23'ün ölçütü düşer.
2. **`.claude/checklists/document-stage.md`** işaretçiye döner: *"Bu checklist 20XX-XX-XX'te `.claude/skills/document-stage/SKILL.md`'ye dönüştü; bölüm numaraları (§1–§7) aynıdır — eski atıflar (`checklists/document-stage.md` §N) o dosyanın §N'ini gösterir."* Arşivlenmiş kayıtlardaki iki yüze yakın atıf değişmez.
3. **`00 §C.7`** (onay — GUARDRAILS §2) — bugünkü metin:

   ```markdown
   Doküman üretim aşamalarının adım adım işletimi: [`.claude/checklists/document-stage.md`](../.claude/checklists/document-stage.md).

   > Bu bilinçli olarak bir skill değil, checklist'tir. Doküman üretimi projeden projeye en çok değişen dönemdir; skill'e dönüştürme, checklist bir kez gerçek bir projede işletildikten **sonra** yapılır.
   ```

   önerilen metin:

   ```markdown
   Doküman üretim aşamalarının adım adım işletimi: [`.claude/skills/document-stage/SKILL.md`](../.claude/skills/document-stage/SKILL.md).

   > Doküman üretimi projeden projeye en çok değişen dönemdir; bu yüzden işletim önce checklist olarak yazılır ve skill'e, gerçek bir projede kararlı hâle geldikten sonra dönüştürülür. Ölçüt iki koşuldur: izlenebilirlik matrisi zorunlu bir aşamada da işletilmiş olmak (§C.1) ve bir aşama kapanışında yapısal değişiklik — yeni adım ya da yeni madde — almamak. Karar her doküman aşamasının öğrenim terfisinde yeniden verilir (§K). Bu projede iki koşul Teknik mimari aşamasında birlikte karşılandı ve checklist skill'e dönüştü.
   ```

   **Elenen — `§C.7`'yi olduğu gibi bırakmak:** "skill değil, checklist'tir" cümlesi dönüşümden sonra yanlış olur ve L1 ile L4 ayrışır.
4. **`CLAUDE.md`** (onay — GUARDRAILS §2), oturum başlangıcının 2. maddesi: `Bir task veya doğrulama başlatılacaksa ilgili skill'in başlangıç kapılarını işlet` → `Bir task, doğrulama ya da doküman aşamasının bir adımı başlatılacaksa ilgili skill'in başlangıç kapılarını işlet`.
5. **L3–L4 hizası** (aynı PR): `INSTRUCTIONS.md` §10 ("Doküman üretim aşamaları: skill değil, checklist" → skill listesine `/document-stage`) ve §2, §7, §9'un yol atıfları · `CONTEXT.md` `.claude/checklists/` satırının rolü · `audit` skill'inin üç atfı · `memory/README.md` (terfi kuralı 3. madde) · `MEMORY.md`.
6. **Playbook:** PF-32 `Uygulandı` olur; hedef `skills/document-stage/` ve `00 §C.7`.

**Reddedilirse (seçenek 2):** checklist olarak kalır, başlık notunun ölçütü "karşılandı; proje sahibi checklist'te kalmasını seçti" diye kapanır; PF-32 `Reddedildi` olur ve gönderimde kararıyla gider.

### Proje sahibine gidecek metin — sade dille

Yönetici aşağıdaki üç maddeyi **üç ayrı numaralı soru** olarak, olduğu gibi iletebilir (her biri "uygula · uygulama" yönünde):

> **Soru A — Yöntem dokümanına bu aşamanın dersleri.** *Sorun:* Yöntem dokümanının her aşamanın derslerini yazdığı bölümünde Teknik Mimari aşamasının dersleri henüz yok. *Ne değişir:* Bu aşamanın yedi dersi — örneğin "kesin söz veren cümleler onları sağlayan düzenekle birlikte yazılır ve ayrıca kontrol edilir" — önceki aşamalardaki gibi o bölüme eklenir; reddederseniz dersler yalnız raporda kalır. **1 ·** ekle · **2 ·** ekleme
>
> **Soru B — Kurulum dosyasında ikinci görüş aracı.** *Sorun:* Kurulum dosyası ikinci görüş için asıl aracı Cursor, yedeğini ChatGPT diye yazıyor; oysa Proje Vizyonu'nun ortasından beri bütün kontroller ChatGPT ile yapıldı ve ChatGPT internette arama da yapabiliyor — bir kez yasal bir iddiayı resmî olmayan bir siteye dayandırdı. *Ne değişir:* Dosyada ChatGPT asıl, Cursor yedek olur ve "yasal iddiaları resmî metinle kontrol ediyoruz" notu girer; çalışma biçimi değişmez. **1 ·** değiştir · **2 ·** değiştirme
>
> **Soru C — Aşama kontrol listesinin beceriye dönüşmesi.** *Sorun:* Doküman aşamalarının adım adım kontrol listesi "henüz oturmadı" diye ajanın her oturumda kendiliğinden tanıdığı bir beceriye çevrilmedi; koyduğunuz iki şart bu aşamada ilk kez birlikte sağlandı. *Ne değişir:* Liste içeriği ve bölüm numaraları aynı kalarak beceriye dönüşür, eski dosya yeni yeri gösteren kısa bir not olur. **1 ·** dönüştür (öneri) · **2 ·** liste olarak kalsın · **3 ·** bir aşama daha bekle

---

## 5. Uygulanan terfiler — dosya ve bölüm

| Dosya | Bölüm | Değişiklik | Aday |
|---|---|---|---|
| `.claude/checklists/document-stage.md` | Başlık notu, son güncelleme | Ö-23'ün Aşama 4 sonucu | Ö-23 |
| | §1 — girdi maddesi | Matrissiz aşamada devirlerin konu planına betikle eşlenmesi; ★ konu; yalnız numaralı atfın gerekçe hücresi | Ö-60, Ö-63 |
| | §1 — "Şablonu tara" | Beşinci şablonun kanıtı (PF-70…PF-73) | Ö-77 |
| | §3 — edge case maddesi | Öncülün türü: ürünün işlem envanteri | Ö-69 |
| | §3 — park maddesi | Satırı güncelleyen karar bütün kollarını taşır | Ö-67 |
| | §3 — geri besleme maddesi | Betik 1: ses değişimi, kuralın evi (Kaynak hücreleri), sınıfı ölçüt alan listeler; betik 2: özet tablonun gerekçe hücresi; Aşama 4'ün kanıtı | Ö-64, Ö-68, Ö-70, Ö-71 |
| | §4 — "Devir taraması" | İlk koşum açılışın eşlemesidir (işaretçi) | Ö-60 |
| | §4 — "Özet tabloları…" | Düzeltmenin andığı hedefler de betikle | Ö-66 |
| | §7 — 5. adım | Birden çok öneri, birden çok numaralı soru | Ö-72 |
| | Aşama-spesifik hatırlatmalar — Teknik mimari | Kısıt soruları açılışta; güvence mekanizmasıyla; güvence merceği | Ö-61, Ö-66 |
| `.claude/INSTRUCTIONS.md` | Son güncelleme; §2 — "Soruları ve seçenekleri numaralandır" | Birden çok öneri, birden çok soru; tek "evet" | Ö-72, H-12 |
| | §2 — "Soru açmadan önce kural katmanlarını tara" | Sorunun öncülü de taranır | Ö-62, S-6 |
| `.claude/skills/cross-review/SKILL.md` | Faz 1 madde 9 | Eşiğin altında 1. tur ölçer | Ö-65, S-9 |
| | Ciddiyet ölçüsü — ikinci koşul | Mimari dokümanı; mekanizma kararı koyan dokümanda okuma paragrafı | S-8 |
| | Faz 2 madde 5 (yeni) | Hukuki kontrol — bulgunun kaynağı, web araması | Ö-73 |
| | Faz 4 madde 5 — Neden | Çıkış kuralının ikinci kanıtı (K-976, CP04) | S-10 |
| | Faz 4 madde 6 (yeni) | Orta bulgular bir sınıfta toplanıyorsa hedefli mercek | Ö-66 |
| | Faz 5 madde 4 | Özet tablonun gerekçe hücresi | Ö-70 |
| `.claude/skills/audit/SKILL.md` | "Koşum biçimi" 1. madde | Güvence merceği | Ö-66 |
| | "Koşum biçimi" 3. madde | Öncülün türü: işlem envanteri | Ö-69 |
| | "Koşum biçimi" 4. madde | Betiğin iki yönlü sınanması | Ö-75 |
| | Faz 1.2 | İç envantere kesin güvence cümleleri | Ö-66 |

**L5 (hook, CI) elendi:** bu aşamanın derslerinin hiçbiri mekanik bir ihlal değildir — güvence ↔ mekanizma, öncül ve geri besleme yüzleri anlamsal okumadır; Aşama 3'ün iki mekanik taraması (CR, `\$[0-9]`) `INSTRUCTIONS §7`'de işliyor.

---

## 6. Playbook'a geri akış (K-649)

Playbook'tan gelen bir dosyaya dokunan ya da dokunması gereken her öğrenim `Docs/PLAYBOOK_FEEDBACK.md`'dedir; liste 74 satırdan **85**'e çıktı. Var olan satırlar tekrar açılmadı, güncellendi:

| Satır | Değişiklik |
|---|---|
| PF-27 | İkinci koşul Aşama 4'ten beri mimari dokümanını da kapsar; mekanizma kararı koyan dokümanda okuma paragrafı (K-952, S-8) |
| PF-32 | Ö-23'ün Aşama 4 sonucu: iki koşul karşılandı — `Onay bekliyor` (§4 Öneri 3) |
| PF-61 | Eşiğin altında 1. tur iki koşumla ölçer (Ö-65, K-953) |
| PF-62 | Çıkış kuralının ikinci kanıtı; orta bulgular bir sınıfta toplanıyorsa hedefli mercek (S-10, Ö-66) |
| PF-65 | Öncülün türü: ürünün işlem envanteri (Ö-69) |
| **PF-75** (yeni) | Matrissiz aşamada devirlerin konu planına betikle eşlenmesi (Ö-60, Ö-63) — checklist §1, §4 |
| **PF-76** (yeni) | Teknik mimari: kısıt soruları açılışta; güvence mekanizmasıyla (Ö-61, Ö-66) — checklist hatırlatmaları |
| **PF-77** (yeni) | Sorunun öncülü kural katmanlarına karşı taranır (Ö-62) — `INSTRUCTIONS §2` |
| **PF-78** (yeni) | Geri beslemenin betik 1'ine üç, betik 2'sine bir yüz (Ö-64, Ö-68, Ö-70, Ö-71) — checklist §3, `cross-review` Faz 5 |
| **PF-79** (yeni) | Güvence merceği (Ö-66) — `audit`, `cross-review` Faz 4 madde 6, checklist §4 |
| **PF-80** (yeni) | Park satırını güncelleyen karar bütün kollarını taşır (Ö-67) — checklist §3 |
| **PF-81** (yeni) | Birden çok öneri, birden çok soru (Ö-72) — `INSTRUCTIONS §2`, checklist §7 |
| **PF-82** (yeni) | Cross-review'da hukuki kontrol ve web araması (Ö-73) — `cross-review` Faz 2 |
| **PF-83** (yeni) | SETUP'ın ikinci AI satırları (Ö-73, Ö-74) — `Onay bekliyor` |
| **PF-84** (yeni) | Betiğin iki yönlü sınanması (Ö-75) — `audit` |
| **PF-85** (yeni) | Aşama 4'ün desenleri — `00 §N.1` (§4 Öneri 1) — `Onay bekliyor` |

PF-74 (Veri Modeli şablonu) değişmedi; kapısı Veri modeli aşamasının açılışıdır.

---

## 7. Kayda ve hafızaya yansıma

- **Karar kaydı v0.115:** K-989 (bu adımın kararları, öneriyle kaydedildi — ⚠ değildir: süreç kararıdır, ürün kuralına dokunmaz). K-855 (aşama sonu notu), K-860, K-916, K-952, K-953, K-976 ve K-983'ün etki sütununa K-989'a geri işaret; Aşama 3'ün K-852 (Ö-23'ün kapısı) ve K-853 (aday 13) satırlarına da (salt okunur kaydın tek istisnası — geri işaret). §12.1: öğrenim terfisinin adım maddesi [x]; Ö-23 maddesi sonucuyla [x]; aday 13 [x]; öğrenim adayları maddesi [x]; **yeni açık madde** — proje sahibinin onayını bekleyen üç öneri (kapı: arşiv işaretinden önce, ⚠ listesiyle aynı adımda, her biri ayrı numaralı soru); ⚠ listesi maddesine bu adımın boş listesi. §5'te CP04'ün 2. aksiyon maddesi kapandı. ⚠ listesi değişmedi (44). K tekilliği betiği boş.
- **Repo hafızası:** `MEMORY.md` Güncel Durum bu adımın hâline getirildi — önceki adımın (checkpoint) paragrafı `MEMORY_ARCHIVE.md`'nin "Aşama 4" başlığına taşındı (`INSTRUCTIONS §7`); "Terfi Edenler" tablosuna iki satır girdi (H-12; Aşama 4'ün süreç kararları ve öğrenimleri).
- **Kullanıcı hafızası — bu adımda yazılmadı; yönetici günceller:**
  - `toplu-oneride-evet-ret-sayilmaz.md` (H-12) — `Terfi` satırı: kural `INSTRUCTIONS.md` §2'ye ("Soruları ve seçenekleri numaralandır") ve checklist §7'nin 5. adımına yazıldı (K-989); notta `00`/SETUP önerisini sade anlatma ölçüsü kalır.
  - `asama1-kapanis-otomasyonu-yetkisi.md` (H-10) ve `yalniz-cok-kritik-konulari-sor.md` (H-11) — "Aşama 4 (2026-10-16): üç ayrı soruyla yenilendi (K-855)" ve kapanışta "Aşama 4 kapandı — Veri modeli aşamasının açılışında yeniden sorulur" satırları; `MEMORY.md` dizinindeki iki satır da ("Teknik mimari açılışında yeniden sorulur" → "Aşama 4 için yenilendi (K-855); Veri modeli açılışında yeniden sorulur").
  - `cursor-agent-cross-review-kosumu.md` (H-8) — `--ignore-user-config` web aramasını kapatmaz, ikinci modelin yasal iddiası resmî metne karşı okunur (`cross-review` Faz 2 madde 5); SETUP'ın birincil/yedek sırası öneride (Öneri 2) — karar gelince "Ortak" bölümü buna göre.
  - `workshop-sorusu-oncesi-template-taramasi.md` (H-1) — üçüncü vaka satırı: sorunun öncülü de taranır (K-860), kuralın evi `INSTRUCTIONS.md` §2 (K-989).
  - **Yeni not önerisi (Ö-76):** "Uzun adımda ara durum ver" — yönetici düzeninde yarım saati aşan bir adımda proje sahibine tek satırlık bilgi (hangi adım, ne bitti, sırada ne); soru değil. Kaynak: Teknik mimari aşaması, proje sahibinin "neredesin" sorusu. İkinci kez yaşanırsa `INSTRUCTIONS §9` işletim kuralı 1'e terfi eder.
- **Mekanik:** değişen dosyalarda CR 0 (`tr -cd '\r'`); eklenen satırlarda geri başvuru artığı yok — `\$[0-9]` eşleşmeleri yalnız kuralın ve bu raporun anlattığı örneklerdir; düzenlemeler birebir değiştiren, her değiştirmede tam bir eşleşme isteyen node betiğiyle yapıldı; Python kullanılmadı.

**Sonuç (6. adım, 2026-10-17):** proje sahibine tek mesaj gitti — kırk dört ⚠ karar ve üç öneri, her biri ayrı numaralı soru. ⚠ listesine itiraz yok (K-990). Öneri 1 uygulandı — `00` v1.0.6; Öneri 2 ve 3 uygulanmadı — `SETUP.md` değişmedi, checklist liste olarak kalır ve Ö-23 Veri modeli aşamasının öğrenim terfisinde yeniden değerlendirilir (K-991). Arşiv işareti düştü (`05` ✓ v0.17); devir karar kaydının §13'ünde — çalışma modlarının yeniden sorulacağını, PF-74'ün şablon taramasını ve Ö-23'ün kapısını taşır. Kullanıcı hafızasının §7'deki notları yöneticinindir; `cursor-agent-cross-review-kosumu.md`'nin SETUP sırası notu Öneri 2'nin sonucuyla — SETUP değişmedi, yol Codex — güncellenir. Aşağıdaki satır 5. adımın hâlidir.

**Sırada:** K-437'nin 6. adımı — proje sahibine tek mesaj: kırk dört ⚠ kararın listesi (çakışma taramasının §4.1'i ve CP04 §5) ve bu raporun §4'ündeki üç öneri, **her biri ayrı numaralı soru**; cevapların kayda işlenmesi (onaylanan öneriler ayrı `docs:` PR'larında); ardından arşiv işareti (Aşama 4'ün kayıtları salt okunur) ve Aşama 5'e — Veri modeli — devir. Devir notu çalışma modlarının yeniden sorulacağını, PF-74'ün şablon taramasını ve onaylanmayan önerilerin sonucunu taşır.

---

*Kaynak: K-437 (kapanış sırası, 5. adım) · `00 §K`, §G.3, §C.7, §N · GUARDRAILS §2, §3 · karar kaydı §12.1 (aday listesi ve adım maddeleri) · çakışma taraması §5, §7 · CP04 §3, §6 · `AUDIT_REPORTS/05_GUARANTEE_LENS.md` · `CROSS_REVIEW_REPORTS/05_CROSS_REVIEW.md`…`_R9.md` · K-855…K-988 (süreç kararları) · K-722, K-852, K-853 (Ö-23'ün ölçütü ve kapısı; aday 13) · K-649 (playbook'a geri akış) · K-989 (bu adımın kararları) · `Docs/CHECKPOINT_REPORTS/PHASE3_LEARNING_PROMOTION.md` (yapı örneği).*
