# Doküman Üretim Aşaması — İşletim Checklist'i

**Katman:** L4 (checklist) | **Son güncelleme:** 2026-10-05

> **Bu neden skill değil?**
> Doküman üretimi projeden projeye en çok değişen dönemdir ve bu adımlar bir referans projede
> **fiilen böyle işletildi** — ama hiç skill'e dönüştürülmedi. Yaşanmamış bir soyutlamayı skill
> olarak dondurmak yerine, yaşananı birebir checklist olarak koyuyoruz. Skill'e dönüştürme kararı
> her aşama kapanışının öğrenim terfisinde (§7, 5. adım) yeniden verilir. Ölçüt iki koşuldur:
> checklist izlenebilirlik matrisi zorunlu bir aşamada da işletilmiş olmalı (§2 ilk kez Aşama 3'te
> işletildi) ve bir aşama kapanışında yapısal değişiklik — yeni adım ya da yeni madde — almamış
> olmalı. Aşama 1 ve Aşama 2'nin kapanışlarında ikisi de karşılanmadı (Ö-23; Aşama 2'nin öğrenim
> terfisi, K-719); Aşama 3'te birincisi karşılandı, ikincisi karşılanmadı — §2'nin ilk işletimi ona
> yeni bir madde ekledi (okuma derinliği; Aşama 3'ün öğrenim terfisi, K-852). Sonraki değerlendirme
> Aşama 4'ün öğrenim terfisindedir. Ölçüt bu notta (L4) yaşar ve `00 §C.7`'nin genel cümlesini
> daraltır; `00 §C.7`'ye taşınması önerildi; proje sahibi onay vermedi, ölçüt bu notta kalır (K-722,
> düzeltmesi K-853). (Karar: yeni skill icat edilmez.)

---

## Her aşamada, sırayla

### 1. Aşama açılışı

- [ ] `Docs/00_PROJECT_METHODOLOGY.md` §C.1'den bu aşamanın **rolünü** al ve o role gir
- [ ] Aşamanın **girdi dokümanlarını** tam olarak oku (00 §C.6 katman tablosu). Girdiler tek bağlama sığmıyorsa açılış onları konu planının gerektirdiği derinlikte okur ve **neyi tam, neyi tarayarak okuduğunu planın girdiler notuna yazar**; izlenebilirlik matrisi zorunlu aşamada satır satır okuma matrisin betikli envanteriyle karşılanır (§2). **Neden:** Arayüz Tanımları'nın üç girdisi birlikte bir milyon baytı aştı; açılış tam okumayı matris adımına bıraktı ve bunu kayda yazdı (Aşama 3, K-724…K-727)
- [ ] Aşamanın **çıktısını** ve beklenen kapsamını proje sahibiyle netleştir
- [ ] Bu aşamada **konuşulmayacak** konuları hatırla (aşama disiplini — GUARDRAILS §1)
- [ ] **Önceki aşamada açılmış modları sor** — toplu onay, ⚠ öneriyle kayıt ya da bir kapanış otomasyonu yetkisi yeni aşamaya kendiliğinden geçmez; açılışta bir cümleyle sorulur, cevap gelene kadar `INSTRUCTIONS.md` §2'nin varsayılanı geçerlidir (merge yetkisi ve yönetici düzeni: `INSTRUCTIONS.md` §9)
- [ ] **Devir notunu kurala karşı oku** — önceki aşamanın devir notu bir aşama kaydıdır, şablonun üstünde değildir. (1) Taşıdığı her soru proje sahibine gitmeden önce L1–L5'e karşı taranır (`INSTRUCTIONS.md` §2, "Soru açmadan önce kural katmanlarını tara"); cevap şablondaysa soru sorulmaz, katmanlar arasındaki ayrılık hizalama hatası olarak düzeltilir. (2) Kapısı açık her maddesi — öneri, bekletilen öğrenim, gözden geçirilecek liste — yeni aşamanın açık süreç listesine satır olarak girer; kapısı doküman döneminin ötesindeyse `Docs/DEFERRED_BACKLOG.md`'ye. **Neden:** Aşama 2'nin açılışında devir notunun "seçilecek" dediği soru şablon taranmadan soruldu ve proje sahibi *"proje template'te bu belli değil mi"* diye geri çevirdi (K-647; aynı ihlal 2026-08-22'de de yaşanmıştı); devir notundaki iki madde — avukat teyidi önerisi ve bekletilen öğrenim Ö-23 — yeni listeye geçmedi ve ancak çakışma taramasında bulundu
- [ ] **Şablonu tara** — çıktı dokümanının şablonunda açık kararlar bölümü var mı; şablonun kendi içinde istediği bölümler ("ayrı bölümde yazılır" gibi) şablonda var mı? Eksik, konu planına plan önerisi olarak girer — şablonun bölüm numaraları değiştirilmez (GUARDRAILS §3) — ve `Docs/PLAYBOOK_FEEDBACK.md`'ye satır olarak yazılır. **Neden:** aynı eksik üç aşamada dört şablonda çıktı — Proje Vizyonu ve MVP Kapsamı (PF-22), Kullanıcı Akışları (PF-40, K-651), Arayüz Tanımları (PF-55…PF-57 — tarama açılışta üç eksiği buldu, K-724, K-726)
- [ ] **Konu kimliği aşamaya özgü bir önek taşır** — karar kaydı dönem boyu tek dosyadır (K-647); aşamanın konu kimliği (Aşama 1 `Bn-mm`, Aşama 2 `AKn-mm`, Aşama 3 `UIn-mm`) önceki aşamaların kimlikleriyle ve dokümanlardaki kimlik aileleriyle çakışmaz — aday önek betikle taranır. **Tarama şablonun getirdiği kimlikleri de kapsar** (ekran, bileşen, form kimliği gibi): şablonun önerdiği biçim var olan bir aileyle çakışıyorsa çıktı dokümanı aşamaya özgü, betikle taranmış bir kimlik alır ve şablon playbook listesine yazılır. **Neden:** Aşama 2 için ilk önerilen `B2-..` Aşama 1'in Blok 2'sinin kimliğiydi ve Ürün Gereksinimleri'nin B-1…B-15 ailesine yakındı; Arayüz Tanımları şablonunun ekran kimliği `S<NN>` sevkiyat geçişleriyle (S1…S11) aynı biçimdeydi ve ancak konu planında bulundu (K-725, PF-54)

### 2. Traceability matrisi (zorunluysa)

00 §C.1 tablosunda "Traceability zorunlu: **Evet**" ise, **çıktı üretilmeden önce**:

- [ ] Kaynak dokümanlardan öğe envanteri çıkar (numaralandırılmış). **Kaynak birimi** (satır, kural ya da alt bölüm düzeyi; hangi kaynak ailesi hangi düzeyde) envanterden önce kayda geçer; envanter **betikle** çıkarılır, komutları matrisin yanında durur ve kapsama betikle denetlenir — beklenen kümeyle karşılaştırmada eksik, fazla ve yinelenen sıfır. Matris birden çok oturuma bölünüyorsa ilk oturum matrisin alt bölüm haritasını sabitler (§4'ün çok oturumlu yazım kuralı). **Neden:** Arayüz Tanımları'nın matrisi 1.193 kaynak satırıyla üç oturumda kuruldu; birim ve harita baştan sabitlendiği için sonraki oturumlar aynı satırı iki kez eşlemedi (K-727, K-728, K-730)
- [ ] **İleri izlenebilirlik:** her kaynak madde → hangi çıktıya eşlenecek?
- [ ] **Okuma derinliği** — kaynak satırı eşlenirken **satırın tamamıyla** okunur; hücre başına kesilerek okuyan oturum bunu ve okumadığını kayda yazar, sonraki oturum kesilmiş satırları örneklemle doğrular: ayrışma oranı %10'u aşarsa doğrulama bütün kümeye genişler (anahtar sözcük taraması ve tam metin okuması) ve kalan satırlar çıktının yazımında, ekranı ya da bölümü yazan oturumda tam okunur. **Neden:** Arayüz Tanımları'nın 1a oturumu satırları hücre başına iki–üç yüz karakterle okudu; 82 satırlık örneklemde 9 satır ayrıştı (%11) — hepsinde hücrenin kesilen kısmındaki ikinci ekran eksikti; genişletilmiş doğrulama yirmi, yazım turu kalan satırları ekran ekran okuyarak otuz dört ayrışma daha düzeltti — 378 satırda elli dört (K-738; tracker §10.1)
- [ ] **Geri izlenebilirlik:** her planlanan çıktı → hangi kaynaktan besleniyor?
- [ ] Eşlenmeyen kaynak madde = **GAP** → listele. GAP adayı, dayandığı kaynak bölümü **tam okunmadan** GAP diye yazılmaz; cevabı kaynakta yazılı olan aday boşluk değil hizalama hatasıdır (kanıt: GA-2'nin cevabı `02 §3.10.1`'deydi ve kesilmiş okuma onu görmemişti — K-739)
- [ ] Kaynağı olmayan çıktı = **gerekçesiz ekleme** → listele
- [ ] GAP'leri proje sahibine sun, **karar al**, kararları kaydet
- [ ] Ancak bundan sonra çıktı üretimine başla

### 3. Workshop döngüsü (her konu için)

- [ ] Konuyu tanıt: neden bu konuyu şimdi konuşuyoruz
- [ ] **Tamlık taraması** liste hazırlanırken: konunun alanında plan tablosunda karşılığı olmayan parça var mı? Bulgular listeye girer; bloğun son konusunda tarama bloğun tamamına genişler (`INSTRUCTIONS.md` §2)
- [ ] Seçenekleri artı-eksileriyle sun
- [ ] **Kendi önerini** gerekçesiyle belirt
- [ ] Proje sahibinden karar al (onay veya farklı yön) — öneriyle kayıt yetkisi kapsamındaki soruda öneri kaydedilir, işaretlenir ve bildirilir; sunum biçimi konu başına tek mesajdır: bütün kararlar numaralı öneri listesi olarak sunulur, sorulmaya devam eden üç grup ⚠ ile işaretlenir (`INSTRUCTIONS.md` §2)
- [ ] **Edge case kontrolü:** *"bu kararın yaratacağı risk veya boşluk var mı?"* Gerekçe başka bir kuralın durumuna dayanan **olgusal bir öncül** taşıyorsa ("bu sürede veri toplayan girişler kapalıdır") öncül o kuralın evinde okunur — kapı hangi koşula bağlı, her sırada tutuyor mu. **Neden:** K-834 böyle bir öncüle dayandı; kapı yalnız aydınlatma metnine bağlıydı ve öncül bir yayın sırasında tutmuyordu — audit ve cross-review kararı gördü, öncülü görmedi; çakışma taraması buldu (K-850)
- [ ] Kararı **anında** `Docs/PRODUCT_DISCOVERY_STATUS.md`'ye yaz. **Satır biçimi:** konu · alınan karar · gerekçe · tarih · etkilediği dokümanlar. Gerekçe elenen **en az bir** seçeneği ve eleme gerekçesini taşır (K-34); etki sütunu hedef dokümanı **bölüm numarasıyla** yazar — doküman düzeyi tek başına yetmez (K-35); hedef doküman henüz yazılmamışsa bölüm numarası yerine parantez içinde **işin adı** yazılır — `06 (iade reddi kaydının alanı)` —, çıplak doküman numarası ("06 · 12") devir taramasında okunamaz (kanıt: Aşama 2'nin sonraki dokümanlara 137 atfının 86'sı çıplaktı ve çakışma taraması her birine sonradan ad vermek zorunda kaldı — `Docs/CHECKPOINT_REPORTS/PHASE2_CONFLICT_SCAN.md` §6). Yalnız **detay** açık kalabilir; açık detay tracker §4'e gözlemlenebilir bir kapıyla yazılır — tarih, "sonra", "ihtiyaç olursa" kapı değildir (K-37)
- [ ] **K numarası tekildir** — numara `origin/main`'deki kaydın son numarasından devam eder. Aynı anda iki `docs:` dalı karar yazıyorsa numara aralıkları baştan ayrılır; merge'den önce şu komut boş dönmelidir: `awk -F'|' '/^\| K-[0-9]+ /{gsub(/ /,"",$2); c[$2]++} END{for(k in c) if(c[k]>1) print k}' Docs/PRODUCT_DISCOVERY_STATUS.md`
- [ ] **Devir yazılırken işin adı yazılır** — karar bir işi sonraki bir konuya, dokümana ya da yol haritasına bırakıyorsa işin türünü değil adını yazar ("otomasyon" değil "kargo şirketi entegrasyonu ve havale eşleştirmesi"); adsız devri aday listesi bir kalem olarak görmez (kanıt: K-473)
- [ ] **Sonraki aşamaya talimat bırakan karar park edilir** — talimat doğduğu anda hedef dokümanın başındaki "Aşama N'den park edilen girdiler" bloğuna kaynak K satırıyla yazılır; aşama içi ileri referans etki sütununda `(downstream)` olarak kalır. Park satırı talimat yazar, karar yazmaz — teknik ya da tasarım kararı park notunda alınmaz (K-36, GUARDRAILS §1). **Etiket ve etki sütunu iki yönlü eşleşir:** park satırının etiketi aralık değil karar listesidir ve etiketteki her kararın etki sütunu hedef dokümanı taşır; etki sütunu sonraki bir dokümana iş bırakan her kararın park satırı vardır ya da satır "park gerekmez" der — karşılaştırma betikle, iki yönde. **Neden:** Aşama 3'ün çakışma taramasında beş kararın park satırı vardı ama etki sütunu hedefi saymıyordu; `12`'nin "K-787…K-796" etiketi senaryosu olmayan kararları da kapsıyordu (`PHASE3_CONFLICT_SCAN.md` §5.3)
- [ ] **Önceki bir kuralı daraltan ya da değiştiren karar geri işaret bırakır** — önceki satırın etki sütununa yeni karar numarası ve ilişki tipi eklenir; iz taşımayan eski satır sonraki yazımda yanlış okunur (kanıt: K-435 K-25'i daralttı, K-25'te iz yoktu ve A-11 doğdu). **Kaynak kararın park satırları da sayılır:** önceki kararın sonraki aşamaya park ettiği satır, hedef dokümanın park bloğunda aynı PR'da güncellenir (kanıt: K-97 dördüncü aktörü ekledi; K-06'nın dört dokümandaki park satırı üç aktör demeye devam etti — Aşama 2'nin çakışma taraması)
- [ ] **Önceki aşamanın dokümanına dokunan karar o dokümana aynı PR'da geri beslenir** (K-652) — workshop'ta ya da yazımda bulunan boşluğun kararı bir K satırıdır; karar üst dokümana (ör. Ürün Gereksinimleri, MVP Kapsamı) aynı PR'da sürüm artışıyla yazılır ve çıktı dokümanının geri besleme tablosuna satır girer. Üst dokümanın ✓ durumu korunur, kalite döngüsü yeniden açılmaz; çıktı dokümanının etki yansıtması (cross-review Faz 5) üst dokümanı yeniden tarar. Geri besleme PR'ı iki betik koşar: **(1)** kararın genişlettiği ya da daralttığı kuralın anahtar terimi üst dokümanlarda aranır, her eşleşme ya güncellenir ya gerekçesiyle elenir — kural tek yerde değişip öteki yüzlerinde eski hâlinde kalmaz. Terim listesi kuralın **adını** değil **nesnesini ve işlemini** de taşır ("kalemi seçer", "satırına işaret düşer", "onay bölümü"); kuralın başka bir işlemdeki eşi (iptalin kuralı değiştiyse fesih), sözlük terimleri, zaman satırları, genel giriş maddeleri, bildirim içerik listeleri ve **izlenebilirlik matrisinin özet hücreleri** terimle bulunsun bulunmasın ayrıca okunur; **(2)** gövdede anılan her K numarası bulunduğu bölümün sonundaki Kaynak satırıyla karşılaştırılır (cross-review Faz 5'in 4. maddesinin betiği). **Neden:** Aşama 2'de cross-review TEMİZ döndükten sonra checkpoint otuz sekiz yansıma kaçağı buldu — başka kanaldan gecikme feshi (K-706) Ürün Gereksinimleri'nde dokuz yerde eski hâlindeydi — ve on üç Kaynak satırında kırk bir atıf eksikti (CP02; Aşama 1'de CP01 B-20 aynı sınıfı bulmuştu). Aşama 3'te betik 1 her geri beslemede koştu, yine de çakışma taraması yirmi üç, checkpoint yirmi dokuz yan yüz buldu — terimler kuralın adını aradı, nesnesini aramadı; matrisin otuz üç özet hücresi kaynak satırı değiştiği hâlde eski kaldı (`PHASE3_CONFLICT_SCAN.md` §5.2; CP03 §3.3)
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
- [ ] **Devir taraması** — kayıtta bu dokümana ya da bölüme devredilmiş her işin (`(downstream)` parçaları, "…'nın işidir" cümleleri) burada karşılık bulduğu kontrol edilir; devralınan iş adsızsa adı karara bağlanır (§3). Tarama kaydın etki sütunlarıyla birlikte önceki dokümanların **gövdesinde** bu dokümanı anan cümleleri ve önceki aşamaların devir dizinini (çakışma taraması raporunun devir bölümü) de okur — karar satırı olmadan devredilen iş yalnız orada durur (kanıt: Kullanıcı Akışları'nın gövdesinde iki iş, `Docs/CHECKPOINT_REPORTS/PHASE2_CONFLICT_SCAN.md` §6.2)
- [ ] **Kural ayrıntısı evinde kalır** — bir doküman başka bir dokümanın kuralına dayandığında ayrıntıyı kopyalamaz, kurala bölüm numarasıyla işaret eder. **Neden:** Proje Vizyonu'na audit düzeltmesiyle yazılan kural ayrıntısı (cayma istisnalarının koşulları, bir ölçünün okunma kuralı) cross-review'ın 5.–7. turlarındaki bulguların çoğunu doğurdu; ayrıntı Ürün Gereksinimleri'ne bırakılınca yüzey küçüldü
- [ ] Header'ı doldur: **Versiyon** + **Bağımlılıklar** + **Son güncelleme**
- [ ] Konvansiyonları/ortak kararları **önce** yaz, detayları sonra — satır kimliği, atıf biçimi (önsüz `§`'in okunuşu) ve aktör listesi dahil (Kullanıcı Akışları §0.3: K-669, K-700, K-701, K-702)
      *(Ortak konvansiyonlar baştan sabitlenirse detaylar hem hızlı hem tutarlı yazılır.)*
- [ ] **Yazım birden çok oturuma bölünüyorsa** ilk oturum alt bölüm haritasını sabitler (K-670); henüz yazılmamış bir bölüme bağlanan hücre geçici olarak hedef alt bölümü taşır ve o bölümü yazan oturum hücreyi aynı PR'da satır numarasına çevirir (K-682). Haritayı değiştirmek zorunda kalan oturum tabloyu ve ona atıf yapan satırları aynı PR'da günceller. Bir oturumun ilk denemesi oturum limitinde düşerse **aynı kapsamla yeniden koşulmaz**: oturum bölünür, bölünme bir karar satırıyla haritaya işlenir ve yarım kalan denemenin çalışma ağacı yeni denemeye taşınmadan önce okunur (kanıt: Arayüz Tanımları'nın §5'i yirmi sekiz ekranla tek oturumda yazılamadan düştü, 2a ve 2b'ye bölündü — K-778). **Neden:** Kullanıcı Akışları beş oturumda yazıldı; harita ilk oturumda sabitlenmeseydi durum makinesinin "Akış" sütunu ve hata satırları dört oturum boyunca boş ya da yanlış atıf taşırdı
- [ ] Özet tabloları ile detay bölümlerini **eşzamanlı** güncelle — kalite döngüsünün ve kapanış adımlarının düzeltmeleri dahil: bir maddeyi değiştiren her düzeltme, o maddeyi ve ekran ya da bölüm kimliğini anan özet yerleri (genel kural, kapalı liste, geçiş satırı, matris hâli, "tek fark" cümlesi) **betikle bulur** ve aynı PR'da okur. **Neden:** Arayüz Tanımları'nın checkpoint'i özet cümle ile ayrıntının ayrıştığı kırk üç yer buldu; çoğu kalite döngüsünün bir kararının ya da bir cross-review düzeltmesinin yalnız ekranın maddesine yazılmasından doğmuştu (K-840 → 3.3.39'un hedefleri; K-843 → 2.13.3 — CP03 §3.1, §3.2)
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
- [ ] **3. Çakışma taraması** — tracker §4'ün açık satırları dokümanların açık kalem listeleriyle **betikle** karşılaştırılır: aynı açık iki yerde durmaz; her açık satır gözlemlenebilir bir kapı taşır; vadesi geçmiş açık adıyla listelenir (K-429). Dokümanın şablonunda açık kararlar bölümü yoksa açık kalem tracker §4'te kalır ve devir notuna yazılır. Kapısı olmayan süreç maddesi de (ör. gözden geçirilecek ⚠ listesi) burada kapı alır. **Park satırları da taranır:** önceki aşamaların park satırları, kaynak kararlarını sonradan değiştiren kararlara karşı okunur — tarama ilişki sözcüğüne ("daraltır", "değiştirir") değil, kaynak K numarasının sonraki satırlarda geçmesine dayanır ve her eşleşme okunur (kanıt: Aşama 2'nin çakışma taraması ilişki sözcüğüyle K-06 → K-97'yi buldu; checkpoint sözcük taşımayan K-14 → K-574 ve K-17 → K-531'i buldu). Rapor: `Docs/CHECKPOINT_REPORTS/PHASE<N>_CONFLICT_SCAN.md`
- [ ] **4. Checkpoint** — `/checkpoint`; rapor `Docs/CHECKPOINT_REPORTS/CP<NN>_….md`
- [ ] **5. Öğrenim terfisi** (00 §K) — bu adım atlanamaz. Aşama boyunca kayda düşen adaylar ve hafızadaki süreç kuralları tek dizinde toplanır; her biri için sonuç yazılır:
      - Tek seferlik gözlem → raporda kalır
      - Tekrarlanacak desen → `00_PROJECT_METHODOLOGY.md` §N **önerisi** — `00` proje sahibinin onayıyla değişir; onaylanmayan desen aşamanın öğrenim terfisi raporunda kalır (K-722)
      - İhlali önleyen kural → **terfi et** (L1–L5, hedef dosya belirtilerek); L3–L5 hedefliler aynı PR'da uygulanır, `00`, `CLAUDE.md` ve `SETUP.md` hedefliler önerilen metinleriyle proje sahibinin onayına sunulur (GUARDRAILS §2). Öneri raporda tam metniyle durur ve proje sahibine **ayrıca sade dille** yazılır: teknik terim ve K numarası taşımadan, "sorun ne / ne değişir" bir-iki cümle (`INSTRUCTIONS.md` §2; kanıt: Aşama 2'de `00` önerisinin ilk anlatımı üç kez geri döndü)
      - **Karar kaydında yaşayan süreç kararları** da aday sayılır: aşamanın kayıtları kapanınca otoriterliği biter (K-436, K-647); kural L1–L5'te değilse sonraki aşama onu bilmez
      - **Bu checklist'in skill'e dönüşmesi** bu adımda yeniden değerlendirilir — ölçüt dosyanın başındaki nottadır (L4; `00 §C.7`'nin genel cümlesini daraltır — K-722)
      **Playbook'a geri akış (K-649):** sonucu playbook'tan gelen bir dosyaya dokunan — ya da dokunması gereken — her öğrenim, uygulansın uygulanmasın, `Docs/PLAYBOOK_FEEDBACK.md`'ye satır olarak da girer. Liste proje tamamlandıktan sonra proje sahibince gönderilir.
      Rapor: `Docs/CHECKPOINT_REPORTS/PHASE<N>_LEARNING_PROMOTION.md`
- [ ] **6. Arşiv işareti ve devir** — sorulmadan kaydedilmiş ⚠ kararların listesi proje sahibine bu adımdan **önce** tek listede gösterilir (`INSTRUCTIONS.md` §2); ardından doküman durumu tablosunda `✓ Tamamlandı`, versiyon ve "son güncelleme" alanları, karar kaydında **aşamanın kayıtlarının** salt okunur işareti (K-436, K-647) ve bir sonraki aşamanın girdi bağımlılıklarının kontrolü. Karar kaydı dosyası dönem boyu tektir (`00 §B`, §G.1): sonraki aşama aynı dosyada K numarasını sürdürür; dosyanın tamamı yalnız doküman döneminin son aşamasının kapanışında arşivlenir
- [ ] `/handoff` ile oturumu kapat

---

## Aşama-spesifik hatırlatmalar

| Aşama | Sık kaçan nokta |
|---|---|
| Product Discovery | "MVP'de yok" kararları da **gerekçesiyle** kaydedilir; kapsam dışı bırakma bilinçli bir karardır; vizyon dokümanı kurala işaret eder, kuralın ayrıntısını taşımaz (§4) |
| Kullanıcı akışları | Durum makinesi (state) tanımları **arayüz tasarımından önce** netleşmeli; bildirim haritası akışlardan ayrı bir tabloda özetlenmeli; hata akışları happy path kadar detaylı |
| UI/UX | Ortak bileşen kütüphanesi **erken** tanımlanmalı; ekran navigasyon haritası ekran tanımlarından **önce**; {ekran × rol × durum} matrisi eksik varyantları yakalar; yönetim ekranları ikincil endişe sayılmaz — envanterde, navigasyonda, matriste ve form envanterinde müşteri tarafıyla aynı derinlikte yazılır (bu projede elli üç ekranın yirmi beşi panel) |
| Teknik mimari | "MVP olarak düşünme, sonrası için de düşün" — sonradan düzeltilmesi çok pahalı kararları (olay kaybı, denetim izi) baştan doğru kur; maliyet kısıtı varsa **erken** söylenmeli, mimariyi doğrudan etkiler |
| Veri modeli | Enum'lar kaynak dokümandan **birebir**, şablondan kopyalanmaz; silme/saklama stratejisi erken; eşzamanlılık kontrolü baştan; denormalizasyon kararı "nerede güncellenir, tutarsızlık riski ne?" ile birlikte alınır |
| API tasarımı | Konvansiyonlar (URL, zarf, auth, sayfalama, hata) endpoint'lerden **önce** sabitlenir; durum × rol iş mantığını sunucuda tutup istemciye "yapılabilir aksiyonlar" göndermek istemci karmaşıklığını azaltır |
| Entegrasyon spec | Alan eşlemesi yazılırken veri modeli **açık tutulur**, her alan adı kontrol edilir; "ücretsiz seçenek MVP'ye yeter mi?" sorusu **önce** sorulur; bu doküman doğası gereği yeni endpoint ihtiyacı doğurur — API dokümanına geri yazım beklenir |
| Kodlama kılavuzu | En çok tur alan dokümandır (hem çok dokümanla tutarlılık hem implementasyon-düzeyi detay); burada eklenen her yeni alan **veri modelinde** tanımlı mı diye kontrol edilmeli |
| Implementation planı | Her task'ta **test beklentisi** alanı olmalı; ara task ihtiyacında `TXXa`/`TXXb` ile faz aralığı bozulmaz; henüz yazılmamış dokümana verilen referans **forward pointer** olarak açıkça işaretlenir (eksik bağımlılık değildir) |
| Doğrulama protokolü | Kapsam MVP kapsamıyla **birebir** hizalı olmalı; MVP'de olmayan özellik doğrulama kriteri olamaz, MVP'de olan çıkarılamaz |
