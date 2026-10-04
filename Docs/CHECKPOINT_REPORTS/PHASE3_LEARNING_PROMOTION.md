# Aşama 3 — Öğrenim Terfisi

**Tarih:** 2026-10-05 | **Aşama:** 3 — UI/UX tasarım | **Kapanış adımı:** K-437'nin 5. adımı (`00 §K`; `checklists/document-stage.md` §7 madde 5)
**Girdi:** karar kaydı v0.92 — §10.1'in on iki öğrenim adayı ve bekletilen Ö-23, Aşama 3'ün süreç kararları (K-723…K-851 içinden), §10.1'in adım maddelerindeki dersler, [çakışma taraması](PHASE3_CONFLICT_SCAN.md) §7, [checkpoint](CP03_PHASE3_CHECKPOINT.md) §6, `Docs/PLAYBOOK_FEEDBACK.md` (PF-54…PF-58), yöneticinin koşum gözlemleri, proje hafızası (`.claude/memory/`) ve kullanıcı hafızasının on bir notu — `origin/main` @ `8035fea`
**Çıktı:** karar kaydı v0.93 · `.claude/checklists/document-stage.md`, `.claude/INSTRUCTIONS.md`, `.claude/skills/cross-review/`, `audit/`, `checkpoint/`, `.claude/memory/MEMORY.md`, `MEMORY_ARCHIVE.md` · `Docs/PLAYBOOK_FEEDBACK.md` 69 satır · karar K-852 — Proje Vizyonu, Ürün Gereksinimleri, Kullanıcı Akışları, MVP Kapsamı, Arayüz Tanımları, `GUARDRAILS.md` ve `CONTEXT.md` değişmedi

> **Ne işe yarar:** `00 §K` gereği aşama, öğrenimi yazılmadan kapanmaz. Bu rapor Aşama 3 boyunca kayda, raporlara ve hafızaya düşen her öğrenim adayının sonucunu yazar: terfi etti mi, hangi katmana ve dosyaya, metni ne. L3–L4 hedefli terfiler bu raporun PR'ında uygulandı; `00` hedefli tek öneri GUARDRAILS §2 gereği uygulanmadı ve tam metniyle, sade diliyle §4'te proje sahibinin onayını bekliyor.

---

## 1. Sonuç

| Sonuç | Sayı | Adaylar |
|---|---|---|
| **Terfi etti — bu PR'da uygulandı** (L3, L4) | 26 | Ö-40, Ö-42…Ö-51 · S-3, S-5, S-8, S-9, S-10, S-11 (süreç kararları) · Ö-52…Ö-59 · H-11 (ek) |
| **Önceden terfi etmişti ya da kuralı bu aşamada işledi** | 8 | Ö-41 · S-1 (K-723) · S-7 (K-763, K-778'in harita yüzü) · H-1, H-3, H-4, H-9, H-10 (biçimi) |
| **Dokümanda yaşar** | 3 | S-2 (K-724…K-726) · S-4 (K-729, K-731) · S-6 (K-762) |
| **Bekletildi** — kapısıyla | 1 | Ö-23 — kapı: Aşama 4'ün öğrenim terfisi (§3) |
| **Hafızada kalır** — kişisel tercih ya da referans | 5 | H-2, H-5, H-6, H-7, H-8 |
| **Proje sahibinin onayını bekliyor** (`00`) | 1 öneri | §4: `00 §N.1`'e Aşama 2 ve 3 için "desenler raporda" işaret satırı (Ö-59'un L1 yüzü) |

Toplam kırk üç aday: karar kaydının §10.1'inden on iki ve Ö-23, süreç kararlarından on bir satır (on sekiz karar), bu adımda bulunan sekiz ders, kullanıcı hafızasından on bir not. `CLAUDE.md` ve `SETUP.md` için terfi önerisi yok: CLAUDE.md'nin oturum başlangıcı ve katman kuralı aşamanın hiçbir öğrenimiyle çelişmiyor; SETUP'ın ikinci AI satırları (birincil yöntem ve yedeği) aşamanın koşumuyla uyumlu — beş tur yedek yöntemle koştu ve skill'in Faz 1'i bunu karşılıyor. Aşama 3'ün öteki kararları (K-732…K-737, K-740…K-761, K-764…K-777, K-779…K-795, K-797…K-846, K-850, K-851) ürün ya da ekran kurgusu kararıdır, dokümanlarda yaşar ve aday değildir; K-834'ün öncülü ve K-850'nin bulunuşu Ö-49'un kanıtıdır.

**Aşamanın asıl öğrenimi.** Aşama 3 izlenebilirlik matrisini ilk kez işletti ve 844 KB'lık bir türetim dokümanı yazdı; iki büyüklük de yöntemin sınırlarını gösterdi. Matrisin kesilerek okunan hücreleri ikinci ekranı kaçırdı (%11) ve matris ancak yazım turunda ekran ekran tam okunarak tamamlandı; ikinci modelin tek çağrısı dikkat sınırına takıldı, parçalı koşumla döngü her turda başka bir metin farkı buldu ve ancak bir çıkış kuralıyla kapandı — devredilen sınıf checkpoint'te kırk üç yerde çıktı. Geri beslemelerin kuralı tek yerde değişip öteki yüzlerde eski kaldı: anahtar terim betiği her geri beslemede koştu ama kuralın adını aradı, nesnesini aramadı. Bunların hepsi yalnız karar kaydında (K-738, K-848, K-849), raporlarda ya da yöneticinin görev metninde yaşıyordu; bu PR'da checklist'e, talimatlara ve skill'lere taşındı. Aşama 2'nin terfi ettirdiği kurallar bu aşamada ölçülebilir iş yaptı (§5, desen 5) — terfi işe yarıyor.

---

## 2. Adaylar ve sonuçları

**Kimlikler:** Ö-40…Ö-51 karar kaydı §10.1'in on iki adayıdır (Ö numaralandırması Aşama 1 ve 2'den sürer; Ö-23 Aşama 1'den gelir). S-1…S-11 Aşama 3'ün süreç kararlarının satırlarıdır. Ö-52…Ö-59 bu adımda bulunan derslerdir — kaynakları §10.1'in adım maddeleri, raporlar ve yöneticinin koşum gözlemleridir. H-1…H-11 kullanıcı hafızasının notlarıdır — Aşama 1 ve 2 raporlarının numaralandırmasıyla aynı.

### 2.1 Karar kaydındaki adaylar (§10.1)

| # | §10.1 | Aday | Sınıf (`00 §K`) | Sonuç | Hedef katman ve dosya | Uygulanan metin (özet) | Playbook |
|---|---|---|---|---|---|---|---|
| Ö-40 | 1 | Arayüz Tanımları şablonunun ekran kimliği `S<NN>` sevkiyat geçişleriyle (S1…S11) çakışıyor; checklist §1'in önek taraması yalnız konu kimliğini söylüyordu (K-725) | İhlali önleyen kural | **Terfi etti** | L4 checklist §1 ("Konu kimliği aşamaya özgü bir önek taşır") | Tarama şablonun getirdiği kimlikleri de kapsar (ekran, bileşen, form); çakışan biçim yerine aşamaya özgü, betikle taranmış kimlik; şablon playbook listesine | PF-54 (Uygulandı) |
| Ö-41 | 2 | Şablonun üç eksiği — açık kararlar bölümü, §9'un §4–§6 ile ilişkisi, konvansiyon · taban · geri besleme yeri (K-724, K-726) | Şablon kusuru | **Kural işledi** — Aşama 2'de terfi etmişti (checklist §1 "Şablonu tara") | L4 checklist §1 (yalnız kanıt) | "Neden"e dördüncü şablonun kanıtı eklendi; şablonun kendisi playbook'a gider | PF-55, PF-56, PF-57 (Uygulandı — çevre karar) |
| Ö-42 | 3 | Girdiler tek bağlama sığmıyor — üç girdi bir milyon baytı aşar; açılış satır satır okumayı matrise bıraktı | İhlali önleyen kural | **Terfi etti** | L4 checklist §1 ("girdi dokümanlarını tam olarak oku") | Sığmıyorsa açılış neyi tam, neyi tarayarak okuduğunu planın girdiler notuna yazar; matris zorunlu aşamada satır satır okuma matrisin betikli envanteriyle karşılanır | PF-59 |
| Ö-43 | 4 | Kesilerek okunan matris hücresi ikinci ekranı kaçırıyor — 82 satırda 9 (%11); kesik okuma bir GAP adayını da yanlış doğurdu (GA-2) | İhlali önleyen kural | **Terfi etti** | L4 checklist §2 — **yeni madde** "Okuma derinliği"; GAP maddesi genişledi | Kaynak satırı tamamıyla okunur; kesilerek okuyan oturum bunu kayda yazar, sonraki oturum örneklemle doğrular; oran %10'u aşarsa doğrulama bütün kümeye genişler, kalan satırlar yazımda tam okunur. GAP adayı kaynak bölüm tam okunmadan yazılmaz | PF-60 |
| Ö-44 | 5 | Şablonun iki metin hatası ve checklist'in UI/UX hatırlatmasındaki aynı belirsiz cümle ("yarısı kadar olabilir") — audit ATIF-7, ATIF-9 | Hizalama + şablon kusuru | **Terfi etti** (hizalama) | L4 checklist, aşama-spesifik hatırlatmalar (UI/UX) | "Yönetim ekranları ikincil endişe sayılmaz — envanterde, navigasyonda, matriste ve form envanterinde aynı derinlikte yazılır (bu projede elli üç ekranın yirmi beşi panel)" | PF-58 (Uygulandı — checklist; şablon değişmedi) |
| Ö-45 | 6 | Büyük dokümanda tek parça cross-review çağrısı dikkat sınırına takılıyor (K-848) | İhlali önleyen kural | **Terfi etti** | L4 `skills/cross-review/SKILL.md` Faz 1 madde 9 | 400 KB üstünde her tur iki koşum: tek parça çağrı ve bölüm sınırında parçalı kontrol; parça notu; sınırlar sabit; bağlam hatası alınmaması parçalamamaya gerekçe değildir. **Eşik okuması:** 365 KB'lık Kullanıcı Akışları tek parçada üç gerçek çelişki buldu, 844 KB'lık Arayüz Tanımları tek parçada TEMİZ dönerken dört parça dört bulgu getirdi — eşik ikisinin arasında ihtiyatla seçildi ve sonraki büyük dokümanın 1. turunda yeniden okunur | PF-61 |
| Ö-46 | 7 | Büyük dokümanda cross-review'ın çıkış koşulu (K-849) | İhlali önleyen kural | **Terfi etti** | L4 `cross-review` Faz 4 madde 5 · L4 `checkpoint` ("Kalite döngüsünün devri") | Tekrar eden RET — en az iki kez reddedilmiş, aynı yer ve iddia — turun sonucunu belirlemez; **beşinci turdan itibaren** kabul edilenlerin hepsi düşük şiddetli iki yerin metin farkıysa döngü kapanır, kalıp dokümanın tamamında taranır, sınıf adıyla checkpoint'e devredilir ve orada bir iç tutarlılık merceği onu arar. **Eşik okuması:** turların kabulü 3 → 4 → 0 → 3 → 0; aynı RET üç turda döndü; devredilen sınıf checkpoint'te kırk üç yerde bulundu — devir zorunludur | PF-62 |
| Ö-47 | 8 | Geri besleme betiği 1 kuralın yan yüzlerini kaçırıyor — çakışma taraması yirmi üç, checkpoint yirmi dokuz yan yüz | İhlali önleyen kural | **Terfi etti** | L4 checklist §3 (geri besleme maddesi, betik 1) | Terim listesi kuralın adını değil nesnesini ve işlemini de taşır; kuralın başka işlemdeki eşi, sözlük terimleri, zaman satırları, genel giriş maddeleri, bildirim içerik listeleri ve matrisin özet hücreleri terimle bulunsun bulunmasın ayrıca okunur | PF-63 |
| Ö-48 | 9 | Park satırının etiketi etki sütunuyla iki yönlü eşlenmiyor; `12`'nin etiketi aralıktı | İhlali önleyen kural | **Terfi etti** | L4 checklist §3 ("Sonraki aşamaya talimat bırakan karar park edilir") | Etiket aralık değil karar listesidir; etiketteki her kararın etki sütunu hedefi taşır, etki sütunu sonraki dokümana iş bırakan her kararın park satırı vardır ya da "park gerekmez" der — iki yönde betikle | PF-64 |
| Ö-49 | 10 | Kararın gerekçesindeki olgusal öncül kuralın evine karşı doğrulanmadı (K-834 → K-850) | İhlali önleyen kural | **Terfi etti** | L4 checklist §3 (edge case maddesi) · L4 `audit` "Koşum biçimi" 3. madde | Gerekçe başka bir kuralın durumuna dayanan olgusal cümle taşıyorsa öncül o kuralın evinde okunur — kapı hangi koşula bağlı, her sırada tutuyor mu; karar anında ve audit'te | PF-65 |
| Ö-50 | 11 | Özet cümle ile ayrıntının ayrışması düzeltme turlarında büyüyor — checkpoint'te kırk üç yer | İhlali önleyen kural | **Terfi etti** | L4 checklist §4 ("Özet tabloları ile detay bölümlerini eşzamanlı güncelle") · L4 `audit` "Koşum biçimi" 1. madde | Kalite döngüsünün ve kapanışın düzeltmeleri dahil: maddeyi değiştiren düzeltme o maddeyi ve ekran kimliğini anan özet yerleri betikle bulur ve aynı PR'da okur; iç tutarlılık merceği genel kural, kapalı liste, geçiş satırı, matris hâli ve "tek fark" cümlesini ayrıntıya karşı okur | PF-66 |
| Ö-51 | 12 | İzlenebilirlik matrisinin özet hücresi kaynak satırı değişince eski kalıyor — otuz üç özet | İhlali önleyen kural | **Terfi etti** (Ö-47 ile aynı madde) | L4 checklist §3 (betik 1) | Matrisin özet hücreleri betik 1'in ayrıca okunan yüzleri arasında | PF-63 |
| Ö-23 | — | Checklist'in skill'e dönüştürülmesi | Karar | **Bekletildi** — kapı: Aşama 4'ün öğrenim terfisi | L4 checklist başlık notu | §3'te gerekçesiyle | PF-32 (Bekletildi) |

### 2.2 Karar kaydında yaşayan süreç kararları

Checklist §7 madde 5: aşamanın kayıtları arşivlenince otoriterliği biter (K-436, K-647); kural L1–L5'te değilse sonraki aşama onu bilmez. Aşama 3'ün yüz yirmi dokuz kararından süreç, yöntem ya da yazım biçimine dokunan on sekizi tarandı: gerekçe hücresinin sınıf cümlesiyle ("Süreç kararıdır", "Yöntem … kararıdır", "Yazım konvansiyonudur", "Yerleşim kararıdır") betik on altı karar süzdü — K-724…K-731, K-738, K-739, K-762, K-763, K-778, K-847…K-849 —; bütün kararların konu hücresi okunarak iki karar eklendi — K-723 (açılışın çalışma modları) ve K-796 (görev metninin sorunun kapsamını aşması). Kalan yüz on bir karar ekran kurgusu ya da ürün kuralıdır (gerekçe hücresi "Ekran kurgusudur" ya da ⚠ sınıfı).

| # | Karar | Konu | Sonuç | Hedef | Uygulanan metin (özet) |
|---|---|---|---|---|---|
| S-1 | K-723 | Çalışma modları — ⚠ öneriyle kayıt, merge yetkisi, yönetici + alt ajan düzeni Aşama 3 boyunca | **Önceden terfi etmişti** — aşama sonu notu | L3 `INSTRUCTIONS.md` §2, §9 · L4 checklist §1 | Modlar Aşama 3'le sınırlıdır ve Teknik Mimari aşamasının açılışında yeniden sorulur; kural yazılıydı, yeni kural eklenmedi. K-723'e geri işaret |
| S-2 | K-724, K-725, K-726 | Arayüz Tanımları'nın iskeleti, ekran kimliği `E-nn`, şablonun taşımadığı üç yerin yerleşimi | **Dokümanda yaşar** (`04` başlık notu, §2.1, §1.3) | — | Şablon yüzü PF-54…PF-57; kimlik yüzü Ö-40 |
| S-3 | K-727, K-728, K-730 | Matrisin kaynak birimi, oturum bölümü ve alt bölüm haritası, taraf ayrımı | **Genel biçimi terfi etti; ayrıntısı dokümanda** (`04 §1.1.1`) | L4 checklist §2 (envanter maddesi) | Kaynak birimi envanterden önce kayda geçer; envanter betikle, komutları matrisin yanında; kapsama betikle denetlenir (eksik, fazla, yinelenen sıfır); çok oturumda ilk oturum matrisin haritasını sabitler |
| S-4 | K-729, K-731 | Matrisin "Ekran" sütununun değer kümesi; bildirim haritasının satır satır eşlenme ölçütü | **Dokümanda yaşar** (`04 §1.1.2`, §1.1) | — | Arayüz Tanımları'na özgü değer kümeleri; sonraki matrisler (Veri Modeli, API Tasarımı) kendi birimini S-3'ün kuralıyla kurar |
| S-5 | K-738, K-739 | 1a satırlarının örneklem doğrulaması; GAP adaylarının doğrulanması | **Terfi etti** (Ö-43 ile) | L4 checklist §2 | "Okuma derinliği" maddesi ve GAP maddesinin genişlemesi |
| S-6 | K-762 | On beş yazım konvansiyonu | **Dokümanda yaşar** (`04` başlık notu) | — | Genel biçimi zaten checklist §4'te ("Konvansiyonları önce yaz" — K-669, K-700…K-702) |
| S-7 | K-763, K-778 | Alt bölüm haritası; 2. oturumun 2a ve 2b'ye bölünmesi | **Harita yüzü önceden terfi etmişti** (K-670'in kalıbı, checklist §4); **limit yüzü terfi etti** (Ö-54) | L4 checklist §4 | Ö-54'te |
| S-8 | K-796 | Gecikme feshi — görev metni proje sahibine sorulan kapsamı aştı | **Terfi etti** (Ö-56 ile) | L3 `INSTRUCTIONS.md` §9, işletim kuralı 2 | Ö-56'da |
| S-9 | K-847 | Ciddiyet ölçüsü Arayüz Tanımları'na 1. turdan — skill'in ikinci koşulu | **Hizalama terfisi** — kural skill'deydi; K-718 "Arayüz Tanımları da bir türetim dokümanıdır; koşulun ona uyup uymadığı o dokümanın cross-review'ından önce kaydedilir" demişti | L4 `cross-review` "Ciddiyet ölçüsü" | İkinci koşul "kapsam, özet, akış ya da arayüz dokümanı — türetim dokümanı" olur; kanıt K-847 (beş tur, kabul edilen on bulgunun hiçbiri ağır sınıfta değil) |
| S-10 | K-848 | Tek parça çağrının yanında parçalı kontrol koşumu | **Terfi etti** (Ö-45) | L4 `cross-review` Faz 1 madde 9 | Ö-45'te |
| S-11 | K-849 | Çıkış kuralı; tekrar eden RET | **Terfi etti** (Ö-46) | L4 `cross-review` Faz 4 madde 5 · `checkpoint` | Ö-46'da |

### 2.3 Bu adımda bulunan adaylar

| # | Aday | Kaynak | Sınıf | Sonuç | Hedef | Uygulanan metin (özet) | Playbook |
|---|---|---|---|---|---|---|---|
| Ö-52 | **Düzenleme aracı satır sonlarını değiştiriyor** — Edit aracı Windows'ta CRLF yazabiliyor; `.gitattributes` (`eol=lf`) commit'te LF'e çevirdiği için repoda CR yok (`git grep` sıfır), ama çalışma ağacındaki CR, hücreleri dikey çizgiyle bölen ve satır sonuna bağlanan betikleri sessizce bozar. Aşama 3'ün adımları "CR 0" denetimini her PR'da koştu — kural yalnız yöneticinin görev metninde yaşıyordu. **Bu adımda ölçümün kendisi yanılttı:** `grep -c $'\r'` bir döngünün komut ikamesinde boş desene dönüşüp satır sayısını verdi ve düzenlenen beş dosya CR'li sanıldı; `tr -cd '\r' \| wc -c` gerçek sayının sıfır olduğunu gösterdi | Yöneticinin görev metni; §10.1'in "CR 0" satırları (2b, 3., 4., 5. oturum, çakışma taraması, checkpoint); bu adımın koşumu | İhlali önleyen kural | **Terfi etti** | L3 `INSTRUCTIONS.md` §7 | Her PR'dan önce değişen dosyalarda CR sayısı `tr -cd '\r' < dosya \| wc -c` ile sıfır; komut ikamesinde `grep -c $'\r'` güvenilmez | PF-67 |
| Ö-53 | **Geri başvurulu değiştirme metni bozuyor** — 3. oturumun bir perl değiştirmesi `$1` ve `$2`'yi metne yazılı bıraktı ve Arayüz Tanımları'nın altı satırını bozdu (OB-04'ün tablo satırı, dört Kaynak satırı, 2.10.1'in yarısı); 4. oturum v0.9'daki metinden ve kayıttaki niyetten yeniden kurdu | §10.1, yazım turunun 4. oturumu | İhlali önleyen kural | **Terfi etti** | L3 `INSTRUCTIONS.md` §7 (Ö-52 ile aynı madde) | Geri başvurulu değiştirmeden sonra diff'in eklenen satırlarında `\$[0-9]` artığı aranır, her eşleşme okunur; toplu düzeltme mümkünse birebir metin değiştiren, tam eşleşme isteyen betikle (CP03'ün yöntemi). **L5 elendi:** bir pre-commit denetimi metindeki meşru betik komutlarına (checklist §3'ün awk komutu, `04 §1.1.1`'in betikleri, bu rapor) takılır | PF-67 |
| Ö-54 | **Limitte düşen oturumun yeniden koşumu** — Arayüz Tanımları'nın §5'i yirmi sekiz ekranla tek oturumda yazılamadan oturum limitinde düştü; ikinci deneme bölünerek koştu (K-778). Kural yoktu: aynı görev aynı kapsamla yeniden verilebilirdi | K-778; §10.1, 2a oturumu | İhlali önleyen kural | **Terfi etti** | L4 checklist §4 (çok oturumlu yazım maddesi) · L3 `INSTRUCTIONS.md` §9, işletim kuralı 4 (işaretçi) | İlk deneme limitte düşerse aynı kapsamla yeniden koşulmaz: bölünür, bölünme karar satırıyla haritaya işlenir, yarım denemenin çalışma ağacı yeni denemeden önce okunur | PF-68 |
| Ö-55 | **Yazımın ortasında doğan çok kritik soru** — 2b oturumu müşterinin bir kalemin yalnız bir kısmını iptal edip edemeyeceğinin kaynakta yazılı olmadığını gördü; bugünkü kuralı (kalem düzeyi) yazdı, soruyu kapısıyla kaydedip yöneticiye iletti; proje sahibi adet seçimini seçti ve karar bir sonraki adımda dört dokümana geri beslendi (K-787…K-795). `INSTRUCTIONS §9` yalnız "karar vermez, durur" diyordu — bugünkü kural yazılabildiğinde adımın sürüp süremeyeceğini söylemiyordu | §10.1, 2b oturumu ve kısmi adet kararı | İhlali önleyen kural | **Terfi etti** | L3 `INSTRUCTIONS.md` §9, işletim kuralı 2 | Bugünkü kural yazılabiliyorsa alt ajan yazar, soruyu ve kapısını açık süreç maddesine yazar, adımı kapatır, soruyu raporunda iletir; yazılamıyorsa durur. Yönetici sorar; cevap ayrı adımda kayda ve geri beslenen dokümanlara işlenir | PF-68 |
| Ö-56 | **Görev metni sorunun kapsamını aştı** — kısmi adet adımının görev metni gecikme feshini de seçenek (1)'e saymıştı, oysa proje sahibine yalnız iptal ve cayma sorulmuştu; alt ajan farkı yakalayıp soru olarak açtı, yönetici düzeltti ve fesih ayrı bir satırla kaydedildi (K-796) | K-796; §10.1 | İhlali önleyen kural | **Terfi etti** | L3 `INSTRUCTIONS.md` §9, işletim kuralı 2 | Kayıt sorunun kapsamını aşmaz: proje sahibinin kararı yalnız ona sorulan kapsamdır; görev metninin eklediği kapsam öneriyle kayıt modunda ayrı bir satırdır | PF-68 |
| Ö-57 | **⚠ listesinin kapıdan önce gösterimi** — liste kırk karara çıktı ve proje sahibi kapanıştan önce görmek istedi (2026-10-05; yöneticinin bildirimi). Kural kapıyı "en geç arşiv işaretinden önce" diye yazıyordu; erken gösterimden sonra kaydedilen kararların ve sonucun kayda işlenmesinin yeri yazılı değildi | Yöneticinin bildirimi; §10.1'in ⚠ maddesi | İhlali önleyen kural (netleştirme) | **Terfi etti** | L3 `INSTRUCTIONS.md` §9, işletim kuralı 3 | Liste kapıdan önce de gösterilebilir; işaret dönüşümü gösterim tarihini taşır, en geç arşiv işaretinin PR'ında kayda işlenir; gösterimden sonraki ⚠ kararlar arşivden önce ayrıca gösterilir | PF-68 |
| Ö-58 | **Kaynak betiğinin (betik 2) kapsamı dar** — cross-review'ın 5. turunun betiği yalnız Aşama 3 kararlarına baktı ve Arayüz Tanımları'nın Kaynak satırlarındaki Aşama 1–2 atıflarının on dokuzunu görmedi (CP03 C-74); CP02'nin betiği de iki Aşama 2 satırını kaçırmıştı (C-75). CP03 bunu "tek seferlik" diye raporda bıraktı — iki vaka kuraldır | CP03 §6 (tek seferlik gözlem) | İhlali önleyen kural | **Terfi etti** | L4 `cross-review` Faz 5 madde 4 | Kapsam: aşamanın kendi dokümanında bütün kararlar (önceki aşamalarınki dahil); geri beslenen dokümanlarda bu ve önceki aşamanın kararları; aralıklar açılır | PF-28 (güncellendi) |
| Ö-59 | **`00 §N` ile ret kararı arasındaki ayrılık** — `00 §N` "boş bırakılmış bir öğrenim satırı kapanmamış bir gate demektir" der, `00 §K` ve checklist §7 "tekrarlanacak desen → `00 §N`" der; proje sahibi Aşama 2'nin desenlerinin §N.1'e girmesini reddetti (K-722) ve §N.1 o aşama için boş kaldı | Bu adımın taraması — `00 §K`, §N; checklist §7; K-722 | Hizalama + L1 önerisi | **Terfi etti** (L4) · **`00` önerisi** (L1) | L4 checklist §7 (5. adım) · L1 `00 §N.1` (§4) | Checklist: desen `00 §N` **önerisidir**, onaylanmazsa raporda kalır (K-722); `00` önerisi proje sahibine sade dille, "sorun ne / ne değişir". `00`: aşama başına işaret satırı (§4) | PF-69 |

### 2.4 Hafızadaki adaylar

CLAUDE.md: *"Bir süreç kuralı yalnız hafızada yaşıyorsa kırılgandır — L1–L5'e terfi eder."* `00 §G.3`: bir hafıza notu ikinci kez bir süreç ihlalini önlemek için kullanılıyorsa kuraldır. Proje hafızasında (`.claude/memory/`) `MEMORY.md`, `MEMORY_ARCHIVE.md` ve `README.md` dışında not yok. Kullanıcı hafızası bu adımda okundu, yazılmadı.

| # | Not | Aşama 3'teki değişiklik | Sonuç | Hedef | Hafızada kalan |
|---|---|---|---|---|---|
| H-1 | Workshop sorusu öncesi template taraması | Değişiklik yok; kural işledi — açılışta devir §9'un altı maddesi taranıp soru olmaktan çıktı (K-723), GA-2 sorulmadı (K-739) | **Önceden terfi etmişti** | `INSTRUCTIONS §2` · checklist §1 | İşaretçi |
| H-2 | Kısa diyalog tarzı workshop sorusu | — | **Hafızada kalır** | — | Soru biçimi ve uzunluğu — kişisel üslup |
| H-3 | CI beklerken izleyiciye güvenme | — | **Önceden terfi etmişti** (`INSTRUCTIONS §3.2`) | — | Hatırlatıcı |
| H-4 | Toplu onayda ⚠ maddeleri ayrı işaretle | — | **Önceden terfi etmişti** (`INSTRUCTIONS §2`) | — | Hatırlatıcı |
| H-5 | Limit dolarsa kaldığın yerden devam | Aynı hattaki ders Ö-54 (limitte düşen görev bölünür) | **Hafızada kalır** — kişisel tercih | — | Tamamı; Ö-54 ayrı kuraldır |
| H-6 | Yazım oturumunda tek bağlam | — | **Hafızada kalır** (kuralı checklist §4'te) | — | Oturum ayarı |
| H-7 | Sonraki chat mesajı | — | **Hafızada kalır** — kişisel tercih | — | Tamamı |
| H-8 | Cross-review ikinci model koşumu | Beş tur Codex ile koştu; büyük doküman için iki koşum (K-848) | **Hafızada kalır** — referans; koşumun kuralı skill'e terfi etti (Ö-45) | — | Çağrı kalıbı, model ve hesap bilgisi |
| H-9 | Dokümanları isimleriyle, kısa ve açık yaz | — | **Önceden terfi etmişti** (`INSTRUCTIONS §2`) | — | Hatırlatıcı |
| H-10 | Kapanış otomasyonu yetkisi (Aşama 3 için yenilendi — K-723) | Yetki ve düzen Aşama 3 boyunca açık | **Biçimi önceden terfi etmişti** (`INSTRUCTIONS §9`); **yetki süreli** — Teknik Mimari aşamasının açılışında yeniden sorulur | — | Yetkinin kendisi |
| H-11 | Yalnız çok kritik konuları sor (Aşama 3 için yenilendi — K-723) | Notun 2026-10-04 eki: konu planı da onay sorusu değildir; cevap beklenen üç şey; `00` önerisi sade dille — ilk anlatım üç kez geri döndü. Ek Aşama 3'ün açılışında kullanıldı (dört plan önerisi öneriyle kaydedildi, K-724…K-727) ve bu adımın görev metninde yeniden istendi — ikinci kullanım | **Terfi etti** (ek) | L3 `INSTRUCTIONS.md` §9, işletim kuralı 1 · L4 checklist §7 (5. adım) | Modun kendisi (süreli); notun ekine `Terfi` satırı yönetici tarafından eklenebilir — bu adım kullanıcı hafızasına yazmadı |

---

## 3. Ö-23 — checklist skill'e dönüşür mü

**Karar: şimdi dönüşmez.** Ölçüt iki koşuldur ve ikisi birlikte aranır (checklist başlık notu, L4; K-722). Değerlendirme betikle yapıldı (`LC_ALL=C.UTF-8`):

| Koşul | Sonuç | Kanıt |
|---|---|---|
| **1. Checklist izlenebilirlik matrisi zorunlu bir aşamada da işletilmiş olmalı** | ✓ **Karşılandı** — ilk kez | `00 §C.1`: Aşama 3'te matris zorunlu. §2'nin yedi maddesi işletildi: envanter (`04 §1.1.1` — 1.193 kaynak satırı, kapsama betikle: eksik 0, fazla 0, yinelenen 0) · ileri izlenebilirlik (`04 §1.1.3`–§1.1.10) · geri izlenebilirlik (`04 §1.2`) · GAP listesi (`04 §1.1.11`) · gerekçesiz ekleme listesi (yok) · GAP'ler karara (`04 §1.3`; tracker §3'te on bir GAP satırı — K-732…K-737, K-739) · ancak sonra çıktı: `git log --reverse 95a168e..8035fea` sırası — açılış (#75), matris 1a, 1b, 2 (#76–#78), ardından workshop (#79) ve yazım turu (#80) |
| **2. Bir aşama kapanışında yapısal değişiklik — yeni adım ya da yeni madde — almamış olmalı** | ✗ **Karşılanmadı** | `grep -c '^- \[ \]'` ile madde sayısı: Aşama 1'in terfisinden önce 41 → sonra 60 (`f4f3fbc`) · Aşama 2'nin terfisi 65 (`cbc6373`) · Aşama 3 boyunca 65 — `git log 95a168e..8035fea -- .claude/checklists/document-stage.md` boş · **bu kapanış 66**: §2'ye yeni madde (okuma derinliği — Ö-43); ayrıca on bir madde genişletmesi (§1'de üç, §2'de iki, §3'te üç, §4'te iki, §7'de bir) ve aşama hatırlatmalarında bir satır düzeltmesi. Bölüm bazında: §1 8 · §2 7 → **8** · §3 19 · §4 12 · §5 3 · §6 9 · §7 7 |

**Gerekçe.** Ölçütün koruduğu durum tam olarak budur: §2 ilk gerçek koşumunda yeni bir yükümlülük doğurdu (okuma derinliği ve oran eşiği); bir skill bu bölümü işletilmeden dondurmuş olurdu. **Elenen — okuma derinliğini var olan "İleri izlenebilirlik" maddesine sıkıştırıp koşulu karşılanmış saymak:** yeni bir yükümlülüktür ve ölçütü biçimle atlatmak olurdu. **Elenen — şimdi dönüştürmek:** değişen bir metni dondurmak her kapanışta skill'i birlikte değiştirmeyi gerektirir; yeni skill yaratmak yapısal değişikliktir ve onay ister (GUARDRAILS §3).

**Kapı:** Aşama 4'ün (Teknik Mimari) öğrenim terfisi. Birinci koşul artık kalıcı olarak karşılanmıştır; Aşama 4'ün kapanışı checklist'e yeni adım ya da madde eklemezse dönüşüm o adımda proje sahibine öneri olarak sunulur. Not: matris Aşama 5, 6 ve 9'da yeniden zorunludur — §2 Aşama 4'te işletilmez. Kapı checklist'in başlık notunda, PF-32'de ve karar kaydı §10.1'de yazılı; 6. adımın devir notu onu Aşama 4'e taşır (checklist §1).

---

## 4. Proje sahibinin onayını bekliyor — `00_PROJECT_METHODOLOGY.md`

GUARDRAILS §2: `00` proje sahibinin **açık onayı** olmadan değişmez. Aşağıdaki öneri bu PR'da **uygulanmadı**. Onay gelirse ayrı bir `docs:` PR'ında uygulanır ve `00`'ın sürümü **v1.0.5** olur. Öneri aşamanın kapanışını engellemez: L4 tarafı (checklist §7'nin desen satırı) bu PR'da hizalandı.

### Öneri 1 — `§N.1 Dönem 1 — Doküman üretimi`'ne aşama başına işaret satırı

**Sorun.** `00 §N` her aşama kapanışında doldurulur ve *"boş bırakılmış bir öğrenim satırı, kapanmamış bir gate demektir"* der; `00 §K` tekrarlanacak deseni §N'e yazar. Aşama 2'nin altı desenini §N.1'e yazma önerisi reddedildi (K-722) — desenler raporda kaldı ve §N.1'de Aşama 2'nin satırı yok. Aşama 3'ün desenleri de raporda (§5). L1'in metni, işletilen pratiği "kapanmamış gate" diye okutuyor.

**Önerilen metin** — `§N.1`'in Aşama 1 paragrafının ve dokuz maddesinin **ardına**:

```markdown
**Aşama 2 — Kullanıcı akışları (2026-10-04) · Aşama 3 — UI/UX tasarım (2026-10-05).** Kurallar `.claude/checklists/document-stage.md`, `.claude/INSTRUCTIONS.md` ve `audit`, `checkpoint`, `cross-review` skill'lerine terfi etti. Tekrarlanacak desenler aşamanın öğrenim terfisi raporunda yaşar: `Docs/CHECKPOINT_REPORTS/PHASE2_LEARNING_PROMOTION.md` §4 ve `PHASE3_LEARNING_PROMOTION.md` §5.
```

**Elenen — Aşama 3'ün desenlerini §N.1'e tam metniyle önermek:** aynı türden öneri bir gün önce reddedildi (K-722). **Elenen — öneri yapmamak:** L1 ile pratik ayrık kalır ve her aşama kapanışı aynı "boş satır" okumasıyla karşılaşır.

**Sürüm satırları:** başlık `**Versiyon: v1.0.4** | … | **Son güncelleme:** 2026-10-03` → `**Versiyon: v1.0.5** | … | **Son güncelleme:** <onay tarihi>`; alt bilgi `*Project Playbook — Metodoloji v1.0.4*` → `*Project Playbook — Metodoloji v1.0.5*`.

**Reddedilirse:** madde gerekçesiyle kapanır; desenler raporda kalır, checklist §7'nin desen satırı zaten bu kalıbı yazar (K-722).

### Proje sahibine gidecek metin — sade dille

Yönetici aşağıdaki iki maddeyi olduğu gibi iletebilir:

> **1. Yöntem dokümanına küçük bir ek (onayınızı istiyor).** *Sorun:* Yöntem dokümanı her aşamanın derslerinin kendi "Öğrenimler" bölümüne yazılmasını istiyor ve bu bölümü boş bırakmayı "kapanmamış aşama" sayıyor. Kullanıcı Akışları aşamasının dersleri sizin kararınızla ayrı raporda kaldı; bölüm o aşama için ve şimdi UI/UX tasarım aşaması için boş görünüyor. *Ne değişir:* Bölüme ders metni girmez; iki aşama için "dersler şu raporda" diyen tek bir satır girer. Reddederseniz dersler yine raporda kalır, hiçbir şey bozulmaz.
>
> **2. Bilgi — sizden bir şey beklenmiyor.** Aşama adımlarını izleyen kontrol listesinin otomatik bir "beceri"ye dönüştürülmesi bu aşamada da yapılmadı: liste bu aşamada yine değişti — izlenebilirlik tablosunun ilk kullanımı ona yeni bir adım ekledi. Karar Teknik Mimari aşamasının sonunda yeniden verilir.

---

## 5. Aşama 3'ün desenleri — raporda kalır

`00 §K` tekrarlanacak deseni `00 §N`'e yazar; `00` proje sahibinin onayıyla değişir ve Aşama 2'nin desenleri reddedildi (K-722). Aşama 3'ün desenleri burada kalır; §4'ün önerisi onaylanırsa `00 §N.1` bu bölüme işaret eder. Kural yüzleri §2'de terfi etti.

1. **İzlenebilirlik matrisi bir kerelik tablo değil, yazım boyunca doğrulanan bir kayıttır.** Arayüz Tanımları'nın matrisi 1.193 kaynak satırıyla üç oturumda kuruldu, on bir boşluk buldu ve altısı üst dokümanlara geri döndü; ama hücre başına kesilerek okunan 378 satırın elli dördünde ikinci ekran eksikti. Matris ancak yazım turu ekranları tek tek tam okuyunca tamamlandı. (Ö-43; K-738)
2. **Büyük dokümanda ikinci modelin dikkati tek çağrıya, döngünün çıkışı da "hepsi TEMİZ" kuralına sığmaz.** 844 KB'lık doküman tek çağrıda TEMİZ döndü, dört parça dört bulgu buldu; parçalı koşumla döngü her turda başka bir metin farkı buldu ve beşinci turda bir çıkış kuralıyla kapandı. Devredilen sınıf checkpoint'te kırk üç yerde çıktı: çıkış kuralı riski silmez, yerini değiştirir. (Ö-45, Ö-46; K-848, K-849)
3. **Kuralın adını arayan tarama kuralın yüzlerini bulmaz.** Her geri beslemede anahtar terim betiği koştu; yine de çakışma taraması yirmi üç, checkpoint yirmi dokuz yan yüz ve otuz üç matris özeti buldu. Terim kuralın nesnesini ve işlemini taşımalı. (Ö-47, Ö-51)
4. **Türetim dokümanının yazımı üst dokümanı yeniden açar; en ağır soru yazımın ortasında doğar.** Kısmi adet sorusu müşteri ekranlarının yazımında çıktı; bugünkü kural yazıldı, soru yöneticiden proje sahibine gitti, cevap tek adımda dört dokümana geri beslendi. Aşama boyunca Ürün Gereksinimleri on bir, Kullanıcı Akışları dokuz, MVP Kapsamı yedi sürüm aldı. (Ö-55; K-787…K-796)
5. **Terfi ettirilen kural sonraki aşamada ölçülebilir iş yaptı.** Aşama 2'nin kuralları: numaraya dayalı park taraması önceki aşamaların üç hizasız satırını buldu; "devirde işin adı yazılır" yalnız numara taşıyan atfı seksen altıdan sıfıra indirdi; şablon taraması dört eksiği açılışta buldu; ciddiyet ölçüsü 1. turdan uygulandı ve kabul edilen on bulgunun hiçbiri ağır sınıfta değildi. (`PHASE3_CONFLICT_SCAN.md` §7; K-847)
6. **Metni değiştiren araç sessizce bozar.** Bir düzenli ifade değiştirmesi geri başvuruyu metne yazılı bıraktı ve altı satır bozuldu; düzenleme aracı Windows'ta satır sonlarını değiştirebiliyor ve satır sonunu ölçen komut bile yanıltabildi. Her PR'dan önce iki mekanik tarama yeter. (Ö-52, Ö-53)
7. **Yönetici ve alt ajan düzeni aşamayı iki günde kapattı; proje sahibine üç şey gitti.** Açılıştan öğrenim terfisine yirmi bir adım (PR #75'ten bu adıma) temiz bağlamlı alt ajanlarla ve CI yeşilse merge yetkisiyle yürüdü; proje sahibine açılış sorusu, tek bir kritik soru (kısmi adet) ve ⚠ listesi gitti. Düzenin işletim kuralları — plan bilgidir, yazımın ortasındaki kritik soru adımı durdurmaz, kayıt sorunun kapsamını aşmaz, liste erken gösterilebilir, limitte düşen görev bölünür — bu adımda yazıya geçti. (Ö-54…Ö-57, H-11)

---

## 6. Playbook'a geri akış (K-649)

Playbook'tan gelen bir dosyaya dokunan ya da dokunması gereken her öğrenim `Docs/PLAYBOOK_FEEDBACK.md`'dedir; liste 58 satırdan **69**'a çıktı. Var olan satırlar tekrar açılmadı, güncellendi:

| Satır | Değişiklik |
|---|---|
| PF-54 | `Uygulandı (çevre karar) — checklist §1'in genişletilmesi aday` → `Uygulandı (checklist §1; şablon değişmedi)` (Ö-40) |
| PF-58 | `Aday` → `Uygulandı (checklist; şablon değişmedi)` (Ö-44) |
| PF-32 | Ö-23'ün Aşama 3 kararı: birinci koşul karşılandı, ikincisi karşılanmadı; kapı Aşama 4'ün öğrenim terfisi |
| PF-27 | İkinci koşul arayüz dokümanını da kapsar (K-847) |
| PF-28 | Kaynak betiğinin kapsamı (Ö-58) |
| **PF-59** (yeni) | Girdilerin okunma derinliği (Ö-42) — checklist §1 |
| **PF-60** (yeni) | İzlenebilirlik matrisinin işletimi: birim, betikli envanter, okuma derinliği, örneklem, GAP doğrulaması (Ö-43; K-727…K-739) — checklist §2 |
| **PF-61** (yeni) | Büyük dokümanda cross-review iki koşum (Ö-45) — `cross-review` Faz 1 |
| **PF-62** (yeni) | Çok çağrılı cross-review'da çıkış kuralı ve checkpoint'e devir (Ö-46) — `cross-review` Faz 4, `checkpoint` |
| **PF-63** (yeni) | Betik 1 kuralın nesnesini arar; yan yüzler ve matris özetleri (Ö-47, Ö-51) — checklist §3 |
| **PF-64** (yeni) | Park etiketi ↔ etki sütunu iki yönlü (Ö-48) — checklist §3 |
| **PF-65** (yeni) | Kararın olgusal öncülü (Ö-49) — checklist §3, `audit` |
| **PF-66** (yeni) | Düzeltmenin özet yüzleri; iç tutarlılık merceği (Ö-50) — checklist §4, `audit` |
| **PF-67** (yeni) | CR ve `$[0-9]` artığı taraması her PR'da (Ö-52, Ö-53) — `INSTRUCTIONS §7` |
| **PF-68** (yeni) | Yönetici düzeninin dört işletim kuralı (Ö-54…Ö-57, H-11) — `INSTRUCTIONS §9`, checklist §4 |
| **PF-69** (yeni) | Desen `00 §N` önerisidir; `00` önerisi sade dille; `00 §N.1` işaret satırı (Ö-59) — checklist §7 · `00` onay bekliyor |

Arayüz Tanımları şablonunun kendisi (PF-54…PF-58) değişmedi; satırlar şablonun playbook'taki hedefini taşır.

---

## 7. Kayda ve hafızaya yansıma

- **Karar kaydı v0.93:** K-852 (bu adımın kararları, öneriyle kaydedildi — ⚠ değildir: süreç kararıdır, ürün kuralına dokunmaz). K-723, K-725, K-727, K-728, K-730, K-738, K-739, K-778, K-796, K-847…K-850'nin etki sütununa K-852'ye geri işaret; Aşama 2'nin K-652, K-718 ve K-722 satırlarına da (salt okunur kaydın tek istisnası — geri işaret). §10.1: öğrenim terfisinin adım maddesi [x]; Ö-23 maddesi sonucuyla [x] (yeniden bekletildi, kapı Aşama 4); öğrenim adayları maddesi [x]; `00` önerisinin açık maddesi girdi (kapı: proje sahibine tek mesaj, arşiv işaretinden önce); ⚠ listesi maddesine bu adımın boş listesi ve erken gösterimin notu. §5'te CP03'ün 2. aksiyon maddesi kapandı. ⚠ listesi değişmedi (40). K tekilliği betiği boş.
- **Repo hafızası:** `MEMORY.md` Güncel Durum bu adımın hâline getirildi — önceki adımın (checkpoint) paragrafı `MEMORY_ARCHIVE.md`'ye taşındı (`INSTRUCTIONS §7`); "Terfi Edenler" tablosuna iki satır girdi (H-11'in eki; Aşama 3'ün süreç kararları ve öğrenimleri).
- **Kullanıcı hafızası:** bu adımda okundu, yazılmadı. H-11'in 2026-10-04 ekinin `INSTRUCTIONS §9` (işletim kuralı 1) ve checklist §7'ye terfi ettiğini söyleyen `Terfi` satırını yönetici ekleyebilir; süreli yetki notları (H-10'un yetkisi, H-11'in modu) Aşama 3'le sınırlıdır.
- **Mekanik:** değişen dosyalarda CR 0 (`tr -cd '\r'`), eklenen satırlarda geri başvuru artığı yok — `\$[0-9]` eşleşmeleri yalnız kuralın ve bu raporun anlattığı örneklerdir; Python kullanılmadı.

**Sırada:** K-437'nin 6. adımı — proje sahibine tek mesaj (§4'ün sade metni: `00` önerisi ve Ö-23'ün sonucu); ⚠ listesinin sonucunun işaretlere işlenmesi; ardından arşiv işareti (`04` ✓, Aşama 3'ün kayıtları salt okunur) ve Aşama 4'e — Teknik Mimari — devir. Devir notu Ö-23'ün kapısını, `00` önerisinin sonucunu ve çalışma modlarının yeniden sorulacağını taşır.

---

*Kaynak: K-437 (kapanış sırası, 5. adım) · `00 §K`, §A.4, §G.3, §C.7, §N · GUARDRAILS §2, §3 · karar kaydı §10.1 (aday listesi ve adım maddeleri) · çakışma taraması §5, §7 · CP03 §3, §6 · K-723…K-851 (süreç kararları) · K-722 (Aşama 2'nin `00` önerilerinin reddi; Ö-23 ölçütünün evi) · K-649 (playbook'a geri akış) · K-852 (bu adımın kararları) · `Docs/CHECKPOINT_REPORTS/PHASE2_LEARNING_PROMOTION.md` ve `PHASE1_LEARNING_PROMOTION.md` (yapı örneği).*
