# Doküman Üretim Aşaması — İşletim Checklist'i

**Katman:** L4 (checklist) | **Son güncelleme:** 2026-10-03

> **Bu neden skill değil?**
> Doküman üretimi projeden projeye en çok değişen dönemdir ve bu adımlar bir referans projede
> **fiilen böyle işletildi** — ama hiç skill'e dönüştürülmedi. Yaşanmamış bir soyutlamayı skill
> olarak dondurmak yerine, yaşananı birebir checklist olarak koyuyoruz. Bir kez gerçek bir projede
> işletildikten sonra skill'e dönüştürülebilir. (Karar: yeni skill icat edilmez.)

---

## Her aşamada, sırayla

### 1. Aşama açılışı

- [ ] `Docs/00_PROJECT_METHODOLOGY.md` §C.1'den bu aşamanın **rolünü** al ve o role gir
- [ ] Aşamanın **girdi dokümanlarını** tam olarak oku (00 §C.6 katman tablosu)
- [ ] Aşamanın **çıktısını** ve beklenen kapsamını proje sahibiyle netleştir
- [ ] Bu aşamada **konuşulmayacak** konuları hatırla (aşama disiplini — GUARDRAILS §1)
- [ ] **Önceki aşamada açılmış modları sor** — toplu onay, ⚠ öneriyle kayıt ya da bir kapanış otomasyonu yetkisi yeni aşamaya kendiliğinden geçmez; açılışta bir cümleyle sorulur, cevap gelene kadar `INSTRUCTIONS.md` §2'nin varsayılanı geçerlidir

### 2. Traceability matrisi (zorunluysa)

00 §C.1 tablosunda "Traceability zorunlu: **Evet**" ise, **çıktı üretilmeden önce**:

- [ ] Kaynak dokümanlardan öğe envanteri çıkar (numaralandırılmış)
- [ ] **İleri izlenebilirlik:** her kaynak madde → hangi çıktıya eşlenecek?
- [ ] **Geri izlenebilirlik:** her planlanan çıktı → hangi kaynaktan besleniyor?
- [ ] Eşlenmeyen kaynak madde = **GAP** → listele
- [ ] Kaynağı olmayan çıktı = **gerekçesiz ekleme** → listele
- [ ] GAP'leri proje sahibine sun, **karar al**, kararları kaydet
- [ ] Ancak bundan sonra çıktı üretimine başla

### 3. Workshop döngüsü (her konu için)

- [ ] Konuyu tanıt: neden bu konuyu şimdi konuşuyoruz
- [ ] **Tamlık taraması** liste hazırlanırken: konunun alanında plan tablosunda karşılığı olmayan parça var mı? Bulgular listeye girer; bloğun son konusunda tarama bloğun tamamına genişler (`INSTRUCTIONS.md` §2)
- [ ] Seçenekleri artı-eksileriyle sun
- [ ] **Kendi önerini** gerekçesiyle belirt
- [ ] Proje sahibinden karar al (onay veya farklı yön) — öneriyle kayıt yetkisi kapsamındaki soruda öneri kaydedilir, işaretlenir ve bildirilir; sunum biçimi konu başına tek mesajdır: bütün kararlar numaralı öneri listesi olarak sunulur, sorulmaya devam eden üç grup ⚠ ile işaretlenir (`INSTRUCTIONS.md` §2)
- [ ] **Edge case kontrolü:** *"bu kararın yaratacağı risk veya boşluk var mı?"*
- [ ] Kararı **anında** `Docs/PRODUCT_DISCOVERY_STATUS.md`'ye yaz. **Satır biçimi:** konu · alınan karar · gerekçe · tarih · etkilediği dokümanlar. Gerekçe elenen **en az bir** seçeneği ve eleme gerekçesini taşır (K-34); etki sütunu hedef dokümanı **bölüm numarasıyla** yazar — doküman düzeyi tek başına yetmez (K-35). Yalnız **detay** açık kalabilir; açık detay tracker §4'e gözlemlenebilir bir kapıyla yazılır — tarih, "sonra", "ihtiyaç olursa" kapı değildir (K-37)
- [ ] **K numarası tekildir** — numara `origin/main`'deki kaydın son numarasından devam eder. Aynı anda iki `docs:` dalı karar yazıyorsa numara aralıkları baştan ayrılır; merge'den önce şu komut boş dönmelidir: `awk -F'|' '/^\| K-[0-9]+ /{gsub(/ /,"",$2); c[$2]++} END{for(k in c) if(c[k]>1) print k}' Docs/PRODUCT_DISCOVERY_STATUS.md`
- [ ] **Devir yazılırken işin adı yazılır** — karar bir işi sonraki bir konuya, dokümana ya da yol haritasına bırakıyorsa işin türünü değil adını yazar ("otomasyon" değil "kargo şirketi entegrasyonu ve havale eşleştirmesi"); adsız devri aday listesi bir kalem olarak görmez (kanıt: K-473)
- [ ] **Sonraki aşamaya talimat bırakan karar park edilir** — talimat doğduğu anda hedef dokümanın başındaki "Aşama N'den park edilen girdiler" bloğuna kaynak K satırıyla yazılır; aşama içi ileri referans etki sütununda `(downstream)` olarak kalır. Park satırı talimat yazar, karar yazmaz — teknik ya da tasarım kararı park notunda alınmaz (K-36, GUARDRAILS §1)
- [ ] **Önceki bir kuralı daraltan ya da değiştiren karar geri işaret bırakır** — önceki satırın etki sütununa yeni karar numarası ve ilişki tipi eklenir; iz taşımayan eski satır sonraki yazımda yanlış okunur (kanıt: K-435 K-25'i daralttı, K-25'te iz yoktu ve A-11 doğdu)
- [ ] **Firmaya sipariş başına yeni bir elle adım ekleyen karar** manuel adım bütçesiyle birlikte okunur (K-404, `02 §10`); tavanı aşacaksa bütçe kararıyla birlikte yeniden açılır. Kural `03`–`12`'nin workshop ve yazımında doğan her "firma şunu da işaretlesin" önerisine uygulanır
- [ ] Kararın etki sütunu **taslağı yazılmış** bir doküman bölümünü taşıyorsa satıra `(taslak güncellenecek)` işaretini koy — işaret satırın taşıdığı taslağı yazılmış **her** bölümü kapsar. Güncelleme küçükse aynı bloğun `docs:` PR'ında yapılır; inkremental bir düzeltmeyi aşan hacim yazım turuna kalır (§4, K-432). Güncellenince işaret `(taslak güncellendi — vX.Y)` olur (K-29)
- [ ] **Bloğun son konusu kapanırken** bloğun bütün karar satırlarını taslağı yazılmış bölümlerle karşılaştır: etki sütunu böyle bir bölümü taşıyıp işaret taşımayan her satır — ya da işareti o bölümü kapsamayan satır — bir **K-29 kaçağıdır** ve blok kaçak kapanmadan ✓ olmaz. İşaretli satırı arayan kapanış hiç işaretlenmemiş satırı görmez; satırda işaret var mı diye bakan kapanış işaretin kapsamadığı bölümü görmez (kanıt: K-81; yazım turunun `02` oturumunda dokuz satır)
- [ ] **Blok kapanışında devirler kapatılır** — önceki kararların bu bloğun konularına bıraktığı her iş bu blokta karara bağlandı mı? Devralan konu yalnız biçimi kurup değeri yazmadıysa devir **açıktır** (kanıt: elli iki karar sayısını `B9-08`'e bıraktı; `B9-08` biçimi kurdu, değerleri yazmadı ve K-413 onları `02 §11`'e devretti — yazım otuz sekiz değeri karara bağlamak zorunda kaldı). Kapanış taraması devrin varlığını değil kapanışını arar
- [ ] **Blok planının "Hedef doküman" sütunu** blok kapanışında bloğun etki sütunlarının birleşimine **betikle** hizalanır — sütun sonraki blokların taslak kapısını belirler (K-28) ve hizasız sütun yazımı yanlış zamanda başlatır (kanıt: Blok 1–3, tracker §6.1 2026-09-19)
- [ ] Sonraki konuya geç (cevapsız soru bırakma)
- [ ] Oturum `docs:` PR'ıyla kapanır; PR'ın içeriğine Güncel Durum bloğu, karar kaydının sürüm başlığı ve blok durum tablosu girer (K-33, K-38 — `INSTRUCTIONS.md` §7)

### 4. Doküman yazımı

- [ ] **Ayrı oturum, kayıt girdisi** — yazım workshop'tan ayrı bir oturumda yapılır; girdi karar kaydı (§2, §4), doküman şablonu, 00 §C.6'nın ilgili katmanı ve varsa üst dokümandır, workshop'un sohbet geçmişi değil (K-30). Yazım kaydın yeterlilik testidir: kayıtta olmayan bir şey gerekiyorsa uydurulmaz, boşluk olarak tek mesajlık öneri listesiyle karara bağlanır (`INSTRUCTIONS.md` §2)
- [ ] **Ne zaman** — bir bölümü besleyen blokların tamamı kapandığında o bölümün taslağı yazılabilir (K-28); aşamanın bütün blokları kapandıktan sonra **tek bir yazım turu** koşar: önceki taslakların bekleyen K-29 güncellemeleri ve yazılmamış bölümlerin ilk yazımı birlikte. Kalite döngüsü bu tur bittikten sonra başlar, taslakta koşmaz (K-432)
- [ ] **Tek bağlamda yaz** — yazım oturumunda çok ajanlı geniş tarama açılmaz; mekanik kontroller betikle yapılır (K atıfları kayıtta var mı, etki sütunundaki her bölüm atfı karşılık buluyor mu, belirsiz ifade var mı). Çok mercekli denetim kalite döngüsünün işidir (`audit` skill'i, "Koşum biçimi"). **Neden:** Aşama 1'in `01` yazım oturumunda on üç ajanlı bir tarama oturum limitini doldurdu ve karşı-doğrulama aşaması hiç koşamadı
- [ ] **Anlamsal tarama** — taslağı yazılmış bölümler kaydın **tamamına** karşı okunur, yalnız etki sütunu o bölümü gösteren satırlara değil; etki sütununa yazılmamış dokunuşları mekanik tarama göremez (Aşama 1 yazım turu: `01`'de yirmi iki, `02`'de elli, `10`'da on bir karar). Bulunan kararın etki sütununa `XX §N (taslak güncellendi — vX.Y)` eklenir
- [ ] **Devir taraması** — kayıtta bu dokümana ya da bölüme devredilmiş her işin (`(downstream)` parçaları, "…'nın işidir" cümleleri) burada karşılık bulduğu kontrol edilir; devralınan iş adsızsa adı karara bağlanır (§3)
- [ ] **Kural ayrıntısı evinde kalır** — bir doküman başka bir dokümanın kuralına dayandığında ayrıntıyı kopyalamaz, kurala bölüm numarasıyla işaret eder. **Neden:** Proje Vizyonu'na audit düzeltmesiyle yazılan kural ayrıntısı (cayma istisnalarının koşulları, bir ölçünün okunma kuralı) cross-review'ın 5.–7. turlarındaki bulguların çoğunu doğurdu; ayrıntı Ürün Gereksinimleri'ne bırakılınca yüzey küçüldü
- [ ] Header'ı doldur: **Versiyon** + **Bağımlılıklar** + **Son güncelleme**
- [ ] Konvansiyonları/ortak kararları **önce** yaz, detayları sonra
      *(Ortak konvansiyonlar baştan sabitlenirse detaylar hem hızlı hem tutarlı yazılır.)*
- [ ] Özet tabloları ile detay bölümlerini **eşzamanlı** güncelle
- [ ] Belirsiz ifade kullanma ("muhtemelen", "gereksinime göre belirlenecek")
- [ ] Ölü placeholder bırakma — "sonra doldurulacak" satırı bir kapıya bağlı değilse yazma

### 5. Doküman Tamamlama Protokolü (00 §C.4)

`✓ Tamamlandı` işaretinden **önce**:

- [ ] **Çapraz referans doğrulaması** — başka dokümandan alınan her enum/sayı/kural/terim kaynakla birebir mi? Kaynak referansı yazıldı mı?
- [ ] **İç tutarlılık** — özet tablolar ile detay bölümler çelişiyor mu?
- [ ] **Bağımlılık taraması** — header'daki her bağımlılık dokümanı hedefli tarandı mı? Orada tanımlı olup burada farklı/eksik kalan kural var mı?

### 6. Kalite döngüsü (00 §C.5)

- [ ] **Birim ve sıra** — birim dokümandır, bölüm değil. Aşamanın birden çok çıktısı varsa döngü üst dokümandan başlayarak doküman başına **bir kez** koşar; bir doküman TEMİZ olmadan sonrakine geçilmez — üst dokümanın etki yansıtması alt dokümana dokunabilir (K-430)
- [ ] **Koşum biçimi** — audit ve deep review dokümanı yazan bağlamda koşmaz; mercekler paralel, bulgular karşı-doğrulamalı (`audit` skill'i, "Koşum biçimi"). Cross-review'ın girdisi yalnız doküman ve şablonudur (K-431; `cross-review` skill'i, Faz 1)
- [ ] `/audit XX` — envanter bazlı sistematik denetim; sayılar tutuyor mu?
- [ ] Audit bulgularını uygula
- [ ] `/deep-review XX` — 8 katman
- [ ] Deep review bulgularını uygula
- [ ] `/cross-review Docs/XX_....md` — TEMİZ olana kadar tur tekrarla; döngü yakınsamıyorsa ciddiyet ölçüsü (`cross-review` skill'i)
- [ ] **Etki yansıtma** (cross-review Faz 5) — downstream + upstream + yeni alan taraması
- [ ] `/checkpoint` — aşama geçiş taraması; doküman başına değil, aşama kapanışında çakışma taramasından sonra koşar (§7 adım 4)

### 7. Aşama kapanışı

Kapanış altı adımla ve **bu sırayla** yürür; hiçbir adım atlanmaz, her adımın girdisi bir öncekinin çıktısıdır (K-437). Her adım kendi `docs:` PR'ıyla kapanır.

- [ ] **1. Yazım turu** (§4)
- [ ] **2. Kalite döngüsü** (§6)
- [ ] **3. Çakışma taraması** — tracker §4'ün açık satırları dokümanların açık kalem listeleriyle **betikle** karşılaştırılır: aynı açık iki yerde durmaz; her açık satır gözlemlenebilir bir kapı taşır; vadesi geçmiş açık adıyla listelenir (K-429). Dokümanın şablonunda açık kararlar bölümü yoksa açık kalem tracker §4'te kalır ve devir notuna yazılır. Kapısı olmayan süreç maddesi de (ör. gözden geçirilecek ⚠ listesi) burada kapı alır. Rapor: `Docs/CHECKPOINT_REPORTS/PHASE<N>_CONFLICT_SCAN.md`
- [ ] **4. Checkpoint** — `/checkpoint`; rapor `Docs/CHECKPOINT_REPORTS/CP<NN>_….md`
- [ ] **5. Öğrenim terfisi** (00 §K) — bu adım atlanamaz. Aşama boyunca kayda düşen adaylar ve hafızadaki süreç kuralları tek dizinde toplanır; her biri için sonuç yazılır:
      - Tek seferlik gözlem → raporda kalır
      - Tekrarlanacak desen → `00_PROJECT_METHODOLOGY.md` §N
      - İhlali önleyen kural → **terfi et** (L1–L5, hedef dosya belirtilerek); L3–L5 hedefliler aynı PR'da uygulanır, `00`, `CLAUDE.md` ve `SETUP.md` hedefliler önerilen metinleriyle proje sahibinin onayına sunulur (GUARDRAILS §2)
      - **Karar kaydında yaşayan süreç kararları** da aday sayılır: kayıt arşivlenince otoriterliği biter (K-436); kural L1–L5'te değilse sonraki aşama onu bilmez
      Rapor: `Docs/CHECKPOINT_REPORTS/PHASE<N>_LEARNING_PROMOTION.md`
- [ ] **6. Arşiv işareti ve devir** — sorulmadan kaydedilmiş ⚠ kararların listesi proje sahibine bu adımdan **önce** tek listede gösterilir (`INSTRUCTIONS.md` §2); ardından doküman durumu tablosunda `✓ Tamamlandı`, versiyon ve "son güncelleme" alanları, karar kaydının arşiv işareti (K-436) ve bir sonraki aşamanın girdi bağımlılıklarının kontrolü
- [ ] `/handoff` ile oturumu kapat

---

## Aşama-spesifik hatırlatmalar

| Aşama | Sık kaçan nokta |
|---|---|
| Product Discovery | "MVP'de yok" kararları da **gerekçesiyle** kaydedilir; kapsam dışı bırakma bilinçli bir karardır; vizyon dokümanı kurala işaret eder, kuralın ayrıntısını taşımaz (§4) |
| Kullanıcı akışları | Durum makinesi (state) tanımları **arayüz tasarımından önce** netleşmeli; bildirim haritası akışlardan ayrı bir tabloda özetlenmeli; hata akışları happy path kadar detaylı |
| UI/UX | Ortak bileşen kütüphanesi **erken** tanımlanmalı; ekran navigasyon haritası ekran tanımlarından **önce**; {ekran × rol × durum} matrisi eksik varyantları yakalar; yönetim ekranları toplam ekranların yarısı kadar olabilir — ikincil endişe sayma |
| Teknik mimari | "MVP olarak düşünme, sonrası için de düşün" — sonradan düzeltilmesi çok pahalı kararları (olay kaybı, denetim izi) baştan doğru kur; maliyet kısıtı varsa **erken** söylenmeli, mimariyi doğrudan etkiler |
| Veri modeli | Enum'lar kaynak dokümandan **birebir**, şablondan kopyalanmaz; silme/saklama stratejisi erken; eşzamanlılık kontrolü baştan; denormalizasyon kararı "nerede güncellenir, tutarsızlık riski ne?" ile birlikte alınır |
| API tasarımı | Konvansiyonlar (URL, zarf, auth, sayfalama, hata) endpoint'lerden **önce** sabitlenir; durum × rol iş mantığını sunucuda tutup istemciye "yapılabilir aksiyonlar" göndermek istemci karmaşıklığını azaltır |
| Entegrasyon spec | Alan eşlemesi yazılırken veri modeli **açık tutulur**, her alan adı kontrol edilir; "ücretsiz seçenek MVP'ye yeter mi?" sorusu **önce** sorulur; bu doküman doğası gereği yeni endpoint ihtiyacı doğurur — API dokümanına geri yazım beklenir |
| Kodlama kılavuzu | En çok tur alan dokümandır (hem çok dokümanla tutarlılık hem implementasyon-düzeyi detay); burada eklenen her yeni alan **veri modelinde** tanımlı mı diye kontrol edilmeli |
| Implementation planı | Her task'ta **test beklentisi** alanı olmalı; ara task ihtiyacında `TXXa`/`TXXb` ile faz aralığı bozulmaz; henüz yazılmamış dokümana verilen referans **forward pointer** olarak açıkça işaretlenir (eksik bağımlılık değildir) |
| Doğrulama protokolü | Kapsam MVP kapsamıyla **birebir** hizalı olmalı; MVP'de olmayan özellik doğrulama kriteri olamaz, MVP'de olan çıkarılamaz |
