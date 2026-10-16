# Shopfolio — Technical Architecture

**Versiyon: v0.2** | **Bağımlılıklar:** `01_PROJECT_VISION.md`, `02_PRODUCT_REQUIREMENTS.md`, `03_USER_FLOWS.md`, `04_UI_SPECS.md`, `10_MVP_SCOPE.md` | **Son güncelleme:** 2026-10-16

> **Aşama:** 4 — Teknik Mimari · **Rol:** Senior Software Architect / System Designer
> **Traceability zorunlu:** Hayır
> **İlke:** Teknik kararlar ürün ihtiyaçlarından **türetilir**. "Şu teknolojiyi kullanmak istiyorum" değil, "şu gereksinimi en iyi karşılayan ne?"

> **Aşama 1'den park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36.
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-10 · Çok kiracılık:** MVP'de çok kiracılık için **hazırlık yapılmaz** — kurulum başına tek firma. Pazaryeri / çok satıcı **kalıcı ürün sınırıdır**; çok kiracılı SaaS post-MVP yol haritası adayıdır ancak mimaride bugün karşılığı yoktur. *Workshop'ta karara bağlandı (2026-10-16): K-929 (büyüme yolu) ve plan konusu TM11-02; yazımı §0.6'da ve §11.3'tedir (2. oturum).*

> **Aşama 2'den park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §8.3 AK0-04).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-666 · K-667 · Sitenin kesintisinde süreler:** kesinti sırasında zamanı gelen kendiliğinden işler — kendiliğinden iptal (kart hattında iptal öncesi son sorguyla, `02 §6.1.2`), havale hatırlatması (yalnız havale süresi dönüşte hâlâ işliyorsa; ertelenen iptalde yeni hatırlatma gitmez — K-685), IBAN'ın silinmesi, periyodik imha, e-postanın yeniden denenmesi — site döndüğünde **kaçırdıkları sırayla** çalışır. Havale ödeme süresi kesinti sırasında dolarsa kendiliğinden iptal, sitenin dönüşünden sonraki ilk iş gününün sonuna ertelenir; bu, kesintinin — panelin ve kendiliğinden işlerin çalışmadığı aralığın — tespit edilmesini gerektirir. Tespitin yöntemi bu aşamanın kararıdır (`02 §4.3`, Z-8). *Workshop'ta karara bağlandı (2026-10-16): K-898; yazımı §5.4'tedir.*
> - **K-687 · Giriş kaydının okunduğu yer:** giriş kaydı (`02 §3.13.20`, Z-40) panelde görünmez; ihlal ya da hesap ele geçirme incelemesinde firma onu sistem kayıtlarıyla birlikte kurulumun barındırma tarafında okur (`02 §8.5.1`, §12.2.9). Kaydın ve sistem kayıtlarının nasıl ve kim tarafından okunacağı — erişim yolu, süzme, dışarı alma — bu aşamanın kararıdır. *Workshop'ta karara bağlandı (2026-10-16): K-915, K-917; yazımı §9.1 ve §9.3'tedir (2. oturum).*
> - **Dizin — Aşama 2 kararlarının bu dokümana devirleri:** karar kaydında K-647…K-718'in etki sütunlarında bu dokümanı gösteren yedi atıf (yedi karar) ve Kullanıcı Akışları'nın (`03`) gövdesinde bu dokümana iş bırakan cümleler [`CHECKPOINT_REPORTS/PHASE2_CONFLICT_SCAN.md`](CHECKPOINT_REPORTS/PHASE2_CONFLICT_SCAN.md) §6'dadır; etki sütununda yalnız doküman numarası taşıyan üç atfın işinin adını dizin verir. Devir taraması (`checklists/document-stage.md` §4) buradan başlar; devrin evi karar satırıdır, dizin onu ikinci kez kaydetmez. Aşama 1'in dizini: `PHASE1_CONFLICT_SCAN.md` §6. *Yazım turunun devir taraması dizinin `05` satırlarını her oturumda okur; sonucu karar kaydının §12.2'sindedir.*

> **Aşama 3'ten park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2 ve §3, mekanizma: K-36 (yeri: §10.3 UI0-04).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-745 · Beklenmeyen hatanın üç hâli:** ekran beklenmeyen hatayı üç hâlde karşılar — işlem düzeyi (form korunur, kullanıcı yeniden dener), sayfa düzeyi (çerçevenin içinde hata sayfası) ve sonucu belirsiz işlem (kullanıcı sonucu görmeye götürülür: sipariş onayı, müşterinin geri alınamaz dört işlemi, yöneticinin onay isteyen işlemleri). Bir hatanın hangi hâle düştüğünün ayrımı ve hatanın kaydı bu aşamanın kararıdır; ekran teknik ayrıntı göstermez (Arayüz Tanımları `04 §2`). *Workshop'ta karara bağlandı (2026-10-16): K-914; yazımı §10.3'tedir (2. oturum).*
> - **K-758 · Ödeme adımında yazılanlar:** misafir alıcının ödeme adımında yazdığı e-posta, adres ve kupon, girişe gidip dönerken ve oturumu kapanan üye yeniden girerken korunur; yalnız o tarayıcıda ve o ziyaret boyunca yaşar ve sipariş onaylanmadan bir kayıt oluşturmaz (`04 §5`; veri asgariliği `02 §8.4`). Nerede ve nasıl tutulduğu bu aşamanın kararıdır. *Workshop'ta karara bağlandı (2026-10-16): K-907; yazımı §6.7'dedir (2. oturum).*
> - **K-760 · Firma bildirimlerinin ulaşmaması:** firmaya giden bildirim (F-1…F-4) üç denemeden sonra da ulaşmazsa panelin ana sayfasında "firma bildirimleri ulaşmıyor" uyarısı çıkar; uyarı firmaya giden bir sonraki bildirim ulaştığında ya da iletişim e-postası değiştiğinde kalkar (`02 §9.1.6`). Uyarının çıktığı ve kalktığı anın nasıl tespit edildiği bu aşamanın kararıdır ve kanal uyarısının eşiğiyle (`02 §6.1.1`; K-522) birlikte tasarlanır. *Workshop'ta karara bağlandı (2026-10-16): K-913; yazımı §10.2'dedir (2. oturum).*
> - **K-800 · Eşzamanlı düzenleme:** panelin düzenleme ekranlarında — ürün, kupon, kurumsal içerik, ayarlar — başka bir yöneticinin araya giren kaydı kaydetme anında yakalanır ve kullanıcıya iki yol sunulur: güncel hâli açmak ya da yine de kaydetmek; sipariş işlemlerinde üzerine yazma yoktur (`02 §3.31.1`, §3.31.2; Arayüz Tanımları `04 §2.10`). Araya giren kaydın nasıl tespit edildiği bu aşamanın kararıdır ve Veri Modeli'nin (`06`) kaydın karşılaştırılan hâliyle birlikte tasarlanır. *Workshop'ta karara bağlandı (2026-10-16): K-873; yazımı §7.3.1'dedir.*
> - **K-801 · Önerilen stok kodu:** yeni varyantın stok kodu alanı sistemin önerdiği tekil bir kodla gelir; yönetici kodu değiştirebilir, kod zorunlu ve tekildir (`04 §9.5`). Önerilen kodun biçimi ve üretimi bu aşamanın kararıdır. *Workshop'ta karara bağlandı (2026-10-16): K-885; yazımı §7.8'dedir.*
> - **K-834 · K-850 · İlk kurulumda çerez:** çerez politikası ilk kez yayına alınana kadar vitrin ziyaretçinin tarayıcısına çerez yazmaz — bu sürede satış da veri toplayan girişler de kapalıdır: iki kapı da çerez politikasının yayınını bekler, aydınlatma metni önce yayına alınsa bile (`02 §3.1.5`, §12.2.5; `04 §5.11`; K-850 — Aşama 3'ün çakışma taramasında hizalandı). Bu pencerede hiçbir çerezin doğmadığının nasıl güvenceye alındığı bu aşamanın kararıdır. *Workshop'ta karara bağlandı (2026-10-16): K-908; yazımı §6.7'dedir (2. oturum).*
> - **K-839 · Geri alma bağlantısının açılması:** e-posta değişikliğinin geri alma bağlantısı açıldığında değişiklik geri alınmaz; geri alma açılan ekrandaki tek düğmeyle olur — e-posta güvenlik tarayıcıları ve bağlantı önizlemeleri bağlantıyı kullanıcıdan önce açabilir (`02 §3.13.14`; `04 §5.28`, §9.25). Bağlantının açılması (GET isteği) durum değiştirmez; aynı ilkenin öteki tek kullanımlık bağlantılara uygulanıp uygulanmadığı bu aşamanın kararıdır. *Workshop'ta karara bağlandı (2026-10-16): K-904; yazımı §6.4'tedir (2. oturum).*
> - **Dizin — Aşama 3 kararlarının bu dokümana devirleri:** karar kaydında K-723…K-850'nin etki sütunlarında bu dokümanı gösteren dokuz atıf (dokuz karar) ve Arayüz Tanımları'nın (`04`) gövdesinde bu dokümana iş bırakan cümleler [`CHECKPOINT_REPORTS/PHASE3_CONFLICT_SCAN.md`](CHECKPOINT_REPORTS/PHASE3_CONFLICT_SCAN.md) §6'dadır; Aşama 3'ün atıflarının hepsi işin adını taşır. Bir iş yalnız `04`'ün gövdesinde yaşar, karar satırı yoktur: üç genişlik sınıfının sınırlarının nasıl uygulandığı (`04 §2.1.1`). Devir taraması (`checklists/document-stage.md` §4) buradan başlar; devrin evi karar satırıdır, dizin onu ikinci kez kaydetmez. Önceki aşamaların dizinleri: `PHASE1_CONFLICT_SCAN.md` §6, `PHASE2_CONFLICT_SCAN.md` §6. *Yazım turunun devir taraması dizinin `05` satırlarını her oturumda okur; sonucu karar kaydının §12.2'sindedir.*

> **Yazım durumu (K-28, K-30, K-432; karar kaydı §12.2):** doküman Teknik mimari aşamasının **tek yazım turunda**, iki oturumda yazılır (K-856'nın sırası). Girdi karar kaydıdır — workshop'un sohbet geçmişi değil (K-30); yazım kaydın yeterlilik testidir ve kayıtta olmayan bir şey gerekiyorsa karara bağlanır.
> - **1. oturum (2026-10-16, v0.2):** başlık notu — yazım konvansiyonları (K-930), alt bölüm haritası (K-931) ve üst dokümanlara geri besleme tablosu (K-857; karar kaydının §12.4'ünden taşındı) · §0 karar prensipleri ve kısıtlar · §2 teknoloji kararları (MK-01…MK-12) · §3 servis yapısı ve sınırlar (MK-13…MK-18) · §7 veri katmanı (MK-19…MK-34) · §4 durum yönetimi ve olaylar (MK-35…MK-40) · §5 arka plan işleri (MK-41…MK-44). Yazımın bulduğu üç boşluk karara bağlandı — modüller ve aralarındaki iletişim (K-932), arama motoru verilerinin ve dizine kapatmanın biçimi (K-933), kuruşa yuvarlamanın yönü (K-934); sonuncusu Ürün Gereksinimleri'ne geri beslendi (v0.69).
> - **2. oturum:** §6 (§6.x dahil), §10, §9, §8, §11, §1, §12 — aşağıdaki haritanın alt bölümleriyle ve bu sırayla. Bu bölümler o oturuma kadar şablondaki hâlindedir; başka bölümden onlara giden atıflar "(2. oturum)" ekini taşır (konvansiyon 9).

**Yazım konvansiyonları (K-857, K-930).** Bu on iki madde §0–§12'nin bütün yazım oturumlarını bağlar; "konvansiyon n" diye anılır.

1. **Bu doküman kural koymaz; sistemin biçimini kararlaştırır.** Ürün Gereksinimleri'nin (`02`) kurallarını, Kullanıcı Akışları'nın (`03`) adımlarını ve Arayüz Tanımları'nın (`04`) ekranlarını taşıyacak yapıyı yazar: kısıtlar, teknoloji, servis yapısı, durumun ve olayların yeri, arka plan işleri, güvenlik, veri katmanının stratejisi, ortamlar, gözlemlenebilirlik, hata toleransı ve büyüme yolu. Mimari bir ürün kuralını değiştirmek ya da tamamlamak zorunda kalırsa bu bir **boşluktur:** uydurulmaz, karar kaydında bir K satırıyla karara bağlanır, aynı PR'da üst dokümana yazılır ve aşağıdaki geri besleme tablosuna satır girer (K-652, K-857). Şema Veri Modeli'nin (`06`), uçlar ve sözleşmeler API Tasarımı'nın (`07`), dış servislerin biçimi Entegrasyon Spesifikasyonu'nun (`08`), kod standartları Kodlama Kılavuzu'nun (`09`), kurulumun adımları ve sır envanteri `DEPLOY_RUNBOOK`'un işidir. Kapsam dışı satırlar (`10 §3`) ve yol haritası adayları (`10 §5`) mimaride yer tutmaz; büyüme yolu (§11) onları kapatmaz.
2. **Mimari karar kimliği `MK-nn`'dir** (K-858): iki hanelidir, yeniden numaralanmaz ve düşen kimlik yeniden verilmez. §2'nin tablosu teknoloji seçimlerine MK-01'den başlayarak kimlik verir; öteki bölümler kararlarına yazım sırasıyla (K-856) devam eder — 1. oturumda §3, §7, §4, §5; 2. oturumda §6, §10, §9, §8, §11. §1 ve §12 yeni kimlik vermez; §12 bütün kimlikleri numara sırasıyla özetler. Her MK'nın tek evi bir alt bölümdür: orada kalın kimliğiyle (**MK-15**) yazılır, öteki bölümler ona kimlikle işaret eder ve kararı yeniden anlatmaz. Bir MK birden çok K satırına dayanabilir; K numaraları alt bölümün Kaynak satırında ve §12'nin gerekçe hücresinde durur. Kimlik aralıkları alt bölüm haritasındadır. **Kısıtlar** (§0) MK taşımaz — kararların girdisidir.
3. **Satır ve madde kimliği bölüm numaralıdır** (Kullanıcı Akışları'nın kalıbı — K-669): alt bölümün maddesi kalın bir numarayla başlar ve dışarıdan `05 §n.m.k` diye anılır — §7.4'ün ikinci maddesi `05 §7.4.2`. Tablo satırları da aynı numarayı taşır. §0'ın alt bölümü yoktur; kısıtlar tablosunun satırları `0.n` diye numaralanır ve `05 §0.n` diye anılır. Harf önekli yeni bir kimlik ailesi açılmaz; MK tek istisnadır.
4. **Adlar ve kimlikler kaynaktan birebir alınır** (K-17; `03 §0.3.3`): durum adları `02 §5`'ten, terimler `02 §1.2`'den; kimlikler olduğu gibi — S1…S11, Ö1…Ö5, B-1…B-16, F-1…F-6, Z-, L-, P-, H- (`02`) · KP-, KD-, ÖK-, SK-, YH- (`10`) · E-, OB- (`04`). Bu doküman kod adı — sınıf, tablo, sütun, uç adı — yazmaz; onlar `06`, `07` ve `09`'un işidir. Ürün ve araç adları (PostgreSQL, Caddy, pgBackRest) özgün yazımıyla yazılır.
5. **Değer yerine kimlik; tasarım değeri ayrıdır.** Ürünün süre, limit ve parametre değerleri `02 §4`, §8.2 ve §11'de yaşar; bu doküman Z-, L-, P-, H- kimliğini yazar (`03 §0.5.3`). İstisna bu dokümanın kendi **tasarım değerleridir** — sunucunun ölçüsü, zamanlayıcının ve nabzın aralıkları, işlerin saatleri, hız sınırı, uyarı eşikleri, yedeğin aralıkları —; evleri burasıdır ve değerleriyle yazılır. Tasarım değeri `02 §11`'in parametresi değildir — panelden değişmez; kurulum ayar dosyasında değiştirilebilenler `DEPLOY_RUNBOOK §C`'de varsayılanlarıyla da durur (K-922). Bir tasarım değerinin ürünün davranışını değiştirmesi gerekirse bu bir boşluktur (konvansiyon 1).
6. **Atıf.** Önsüz `§` bu dokümanı gösterir; başka dokümana atıf numarasıyla yazılır (`02 §3.31.2`, `04` 2.10.1, `10 §4.1` ÖK-2). Atıf dizisinde devralma Kullanıcı Akışları'nın kuralıdır (K-701; `03 §0.3.4`): `0X §` ile başlayan dizideki sonraki önsüz `§`'ler o dokümanı gösterir; dizi ` · `, `;`, parantezin kapanışı ya da cümle sonuyla biter. Kullanıcı Akışları'nın ve Arayüz Tanımları'nın madde numaraları `§`'siz de yazılır (`03` 10.2.4, `04` 2.10.1). Metodoloji, ajan katmanı ve işletim dosyaları adıyla anılır (`INSTRUCTIONS §3.2`, `DEPLOY_RUNBOOK §B`).
7. **Kaynak satırı.** Her alt bölüm bir *Kaynak* satırıyla kapanır. Kaynak önce **Aşama 4'ün K numaralarıdır** — bu dokümanın kararlarının gerekçesi ve elenen seçenekleri karar kaydındadır; kayıt dönem sonunda arşivlenince gerekçenin evi §12'dir. Üst dokümanın kuralı `02 §…`, `03 …`, `04 …`, `10 …` diye gösterilir; Aşama 1–3'ün kararları kuralın evi doküman olduğu için önce doküman atfıyla, gerekiyorsa K numarasıyla birlikte yazılır. Park satırlarının ve devir dizinlerinin bu dokümana bıraktığı işi kapatan alt bölüm devreden kararı ya da cümleyi Kaynak satırında "devir:" önekiyle anar.
8. **Kural ayrıntısı evinde kalır** (`checklists/document-stage.md` §4). Mimari bir ürün kuralına dayanıyorsa kuralın koşullarını kopyalamaz; mekanizmayı yazar ve kurala bölüm numarasıyla işaret eder. İki işaret sabittir: dal stratejisi `INSTRUCTIONS §3.2`'dedir ve §8 ona işaret eder (K-859); sır envanteri `DEPLOY_RUNBOOK §B`'dedir — §6.x stratejiyi, runbook envanteri taşır.
9. **Geçici hedef.** Henüz yazılmamış bir alt bölüme bağlanan atıf hedef alt bölümü ve oturumunu taşır — "§9.4 (2. oturum)", parantez içinde "(§9.4 — 2. oturum)"; o bölümü yazan oturum eki aynı PR'da kaldırır (K-682'nin kalıbı). Alt bölüm haritasını değiştirmek zorunda kalan oturum tabloyu ve ona atıf yapan satırları aynı PR'da günceller (K-670'in kalıbı).
10. **"Yoktur", belirsiz ifade ve tarihli bilgi.** Mimarinin bilinçli olarak taşımadığı bileşen ilgili alt bölümde tek cümleyle "yoktur" diye anılır (ayrı önbellek sunucusu yoktur) — boş bırakılmaz. "Muhtemelen", "belki", "gerekirse karar verilir" yazılmaz (`GUARDRAILS §5`). Dış dünyaya ait olgular — sürüm ve destek tarihleri, ücretsiz katmanın sınırları, bir aracın resmî imajının varlığı — karar kaydında "bilgim tarihli" notunu taşıyorsa burada da taşır ve kurulumda ya da implementation'da doğrulanır; bu doküman onlara yeni kesinlik eklemez.
11. **Açık kalem ve devir** (TM0-04). Bu dokümanın şablonunda açık kararlar bölümü yoktur: karara bağlanmış bir varlığın yalnız **detayı** açık kalabilir ve karar kaydının §4'ünde A-19'dan, gözlemlenebilir bir kapıyla sürer (K-37; `00 §N.1`). Sonraki dokümana talimat bırakan karar o dokümanın başındaki **"Aşama 4'ten park edilen girdiler"** bloğuna satır olarak girer — blok yoksa bu dokümandaki park bloklarının biçimiyle açılır (`06`, `07`, `08`, `09`, `11`, `12`, `DEPLOY_RUNBOOK`; K-36). Park satırının etiketi ile kararın etki sütunu iki yönde eşleşir (`checklists/document-stage.md` §3). Aşama içi ileri referans etki sütununda `(downstream)` olarak kalır. Bu dokümanda "açık" işaretli madde bırakılmaz.
12. **Bileşen diyagramının biçimi.** §1'in diyagramı metin tabanlıdır — Mermaid akış şeması: GitHub onu çizer ve repoda satır satır karşılaştırılır. Diyagramdaki her kutu §1.2'nin tablosunda tek cümlelik sorumluluğuyla bir satırdır; diyagram §3.2'nin bileşenlerini ve §3.3'ün modüllerini adlarıyla taşır, yeni ad icat etmez.

**Alt bölüm haritası (K-931).** Şablonun §0…§12 numaraları değişmez (GUARDRAILS §3); alt düzey aşağıdaki tabloyla sabittir. "Plan konuları" karar kaydının §12.3'üdür; "MK" sütunu bölümün verdiği kimliklerin aralığıdır (konvansiyon 2).

| Bölüm | Alt bölümler | Plan konuları | MK | Oturum |
|---|---|---|---|---|
| Başlık notu | Park blokları (Aşama 1, 2, 3) · Yazım durumu · Yazım konvansiyonları (1–12) · Alt bölüm haritası · Üst dokümanlara geri besleme | TM0-01…TM0-05 | — | 1 ✓ |
| §0 Karar prensipleri | Dört ilke (şablon) · Bu projenin kısıtları (0.1–0.13) | TM0-06…TM0-09 | — | 1 ✓ |
| §1 Sistem genel görünümü | 1.1 Bileşen diyagramı · 1.2 Bileşenler ve sorumlulukları | TM1-01 | — | 2 |
| §2 Teknoloji kararları | 2.1 Teknoloji tablosu · 2.2 Seçimi başka dokümana ya da kuruluma bırakılanlar · 2.3 Yığının kuruluma yansıması (D-01) | TM2-01…TM2-04 | MK-01…MK-12 | 1 ✓ |
| §3 Servis yapısı ve sınırlar | 3.1 Uygulamanın biçimi · 3.2 Sunucudaki bileşenler ve ağ · 3.3 Modüller · 3.4 Modüller arası iletişim kuralı · 3.5 Vitrin ve panel · 3.6 Dış servislerin sınırı — bağlantı katmanı · 3.7 Sayfa adresleri ve arama motoru verileri · 3.8 Ön yüz ve tasarım tabanının uygulanması | TM3-01…TM3-06 | MK-13…MK-18 | 1 ✓ |
| §4 Durum yönetimi ve olaylar | 4.1 Durum makinesinin yeri — geçiş birimi · 4.2 Sipariş onayı · 4.3 Yan etkiler ve iş kuyruğu · 4.4 E-postanın gönderimi ve gönderim kaydı · 4.5 Tek işlenme — sipariş onayı, sağlayıcı bildirimi ve son sorgu · 4.6 Karta para gönderen isteğin tek işlenmesi · 4.7 İşlem izi | TM4-01…TM4-04 | MK-35…MK-40 | 1 ✓ |
| §5 Arka plan işleri ve zamanlanmış görevler | 5.1 Çalıştırıcılar — zamanlayıcı ve iş kuyruğu · 5.2 İşlerin envanteri · 5.3 İşlerin saatleri ve gecenin sırası · 5.4 Kesintinin tespiti ve dönüşteki sıra | TM5-01, TM5-02 | MK-41…MK-44 | 1 ✓ |
| §6 Güvenlik mimarisi | 6.1 Kimlik doğrulama ve oturum · 6.2 Yetkilendirme · 6.3 Deneme limitleri ve istek hızı · 6.4 Anahtarlar ve tek kullanımlık bağlantılar · 6.5 Form ve site güvenliği · 6.6 Yüklenen dosya · 6.7 Çerezler ve tarayıcıda tutulan veri · 6.8 Hassas verinin taşınması ve saklanması · 6.9 Kurulum tarafının işlemleri ve bakım erişimi — yönetim komutlarına §8.5, sunucuya erişime §8.9 işaret eder · 6.x Secret yönetimi (şablonun zorunlu alt bölümü; başlığı şablondaki gibi kalır) | TM6-01…TM6-10 | MK-45'ten, yazım sırasıyla | 2 |
| §7 Veri katmanı kararları | 7.1 Veritabanı ve işlem bütünlüğü · 7.2 Zamanın ve paranın temsili · 7.3 Eşzamanlılık kontrolü · 7.4 Saklama ve imha · 7.5 İşlem izinin korunması · 7.6 Yedekleme ve kurtarma · 7.7 Dosya depolama ve dijital teslim · 7.8 Önerilen stok kodu · 7.9 Site içi arama · 7.10 Önbellek | TM7-01…TM7-06 | MK-19…MK-34 | 1 ✓ |
| §8 Ortamlar ve deployment | 8.1 Ortamlar · 8.2 Paketleme ve kayıt defteri · 8.3 Dal stratejisi ve CI/CD hattı — dal stratejisi `INSTRUCTIONS §3.2`'ye işaret eder · 8.4 Konfigürasyon yönetimi · 8.5 Yönetim komutları ve ilk kayıtlar · 8.6 Güncellemenin kurulumlara ulaşması · 8.7 Göçler ve geri alma · 8.8 Güvenlik güncellemeleri · 8.9 Sunucuya bakım erişimi | TM8-01…TM8-04 | yazım sırasıyla (§6, §10, §9'dan sonra) | 2 |
| §9 Gözlemlenebilirlik | 9.1 Sistem kayıtları — kaynaklar, içerik ve maskeleme · 9.2 Kayıtların saklanması ve korunması · 9.3 Kayıtların okuma yolu · 9.4 İzleme ve uyarı | TM9-01, TM9-02 | yazım sırasıyla | 2 |
| §10 Hata toleransı | 10.1 Dış bağımlılıklar — "çökerse ne olur" · 10.2 E-posta kanalının ve firma bildirimlerinin uyarısı · 10.3 Beklenmeyen hata | TM10-01…TM10-03 | yazım sırasıyla | 2 |
| §11 Ölçeklenebilirlik notları | 11.1 Tasarım değerleri · 11.2 Bilinçli tekil bırakılanlar · 11.3 Büyüme yolu | TM11-01, TM11-02 | yazım sırasıyla | 2 |
| §12 Kararların özeti | Tek tablo — MK-01'den numara sırasıyla | TM12-01 | — | 2 |

Yazım sırası K-856'dır: başlık notu ve §0 → §2 → §3 → §7 → §4 → §5 (1. oturum) → §6 → §10 → §9 → §8 → §11 → §1 → §12 (2. oturum).

**Üst dokümanlara geri besleme (K-652, K-857).** Teknik Mimari'nin workshop'unda ya da yazımında alınıp üst dokümana dönen her karar bu tabloda bir satırla durur. Tablo karar kaydının §12.4'ünden yazım turunun 1. oturumunda taşındı — satırlar değişmedi, yalnız karar kaydının bir bölümüne giden atıf "karar kaydı" önekini aldı (konvansiyon 6); geri beslemelerin betik kayıtları orada kalır. Üst dokümanın ✓ durumu korunur, kalite döngüsü yeniden açılmaz; bu dokümanın etki yansıtması (cross-review Faz 5) üst dokümanları yeniden tarar.

| Bulgu | Karar | Değişen bölümler | Sürüm |
|---|---|---|---|
| Barındırmanın biçimi ve yedeğin yeri ÖK-2'de yazılı değildi; yedeğin "firmanın barındırmasında" durabilmesi sunucuyla birlikte kaybolan bir yedeğe izin veriyordu (workshop 1, TM0-08 ve TM7-04) | K-863 (proje sahibi), K-864, K-878, K-879 (K-629'u daraltır), K-880 | MVP Kapsamı `10 §4.1` ÖK-2 — ön koşul, "Kim sağlar" ve Kaynak hücreleri; §4'ün Kaynak satırı; başlık notu ve dipnot | `10` v0.44 → v0.45 |
| Sipariş tarafındaki sıfır kayıp kuralı sunucunun diskinin kalıcı kaybındaki bir dakikalık pencereyi yazmıyordu (karar kaydı §12.1'in çok kritik sorusu; workshop 2, TM7-04) | K-888 (proje sahibi) | Ürün Gereksinimleri `02 §3.34.8`, §12.3.3, §3'ün ve §12'nin Kaynak satırları · MVP Kapsamı `10 §1` ve Kaynak satırı, §2 KP-75 · Kullanıcı Akışları `03` 10.2.4 · Arayüz Tanımları `04 §1.1`'in KP-75 ve `02 §3.34.8` satırlarının özeti · dördünün başlık notu, sürümü ve dipnotu | `02` v0.66 → v0.67 · `10` v0.45 → v0.46 · `03` v0.22 → v0.23 · `04` v0.20 → v0.21 |
| Doğrulama bağlantısı açıldığı anda doğruluyordu; e-posta güvenlik tarayıcısı bağlantıyı adresin sahibinden önce açarsa başkasının açtığı kayıt doğrulanır ve adresin misafir siparişleri o hesaba düşerdi (workshop 2, TM6-05; K-839'un park satırı) | K-904 | Ürün Gereksinimleri `02 §8.1.3`, §3.13.5, §3.13.14, §3'ün ve §8'in Kaynak satırları · Kullanıcı Akışları `03` 9.1.4, 9.3.5 · Arayüz Tanımları `04 §1.1`'in `03 §9.1.4` ve §9.3.5 satırlarının özeti, §1.1.2 (E-21, E-54), 3.5.4, §4.1 E-21, §4.2 E-54, 5.21.2, 5.21.3, 5.21.7, 5.21.9, 6.2.21'in giriş cümlesi, 6.2.21.1, 6.2.21.5, 9.25.2, 9.25.4, 9.25.8, 6.3.25.3, 5.21'in, 9.25'in ve §6.2'nin Kaynak satırları | aynı sürümler (`02` v0.67 · `03` v0.23 · `04` v0.21) |
| Ödeme adımında yazılanların korunması zorunlu çerezlerin listesinde değildi (workshop 2, TM6-07; K-758'in park satırı) | K-907 | Ürün Gereksinimleri `02 §12.2.5`, §12'nin Kaynak satırı | `02` v0.67 (aynı sürüm) |
| Sistem kayıtları ihlal incelemesinin üç kaydından biriydi (`02 §8.5.1`), ama IP taşıyan bu kayıtların süresi süre envanterinde yoktu — K-515 süreyi ürün düzeyinde tanımsız bırakmıştı (workshop 3, TM9-01) | K-916 | Ürün Gereksinimleri `02 §4.2` Z-40, §11.2 P-45, §8.5.1, §12.2.3, §12.2.7, §4'ün, §8'in, §11'in ve §12'nin Kaynak satırları · Kullanıcı Akışları `03` 4.1.42 · Arayüz Tanımları `04 §1.1`'in `03 §4.1.42` satırının özeti · üçünün başlık notu, sürümü ve dipnotu | `02` v0.67 → v0.68 · `03` v0.23 → v0.24 · `04` v0.21 → v0.22 |
| Ürün Gereksinimleri kuruşa yuvarlamanın yönünü yazmıyordu — .NET'in varsayılan yuvarlaması tam yarımı çift sayıya götürür; kargo ücretinin oranlara bölüşümünde bölünmeden kalan kuruşun yeri de yazılı değildi (yazım turu 1. oturum, TM7-02) | K-934 | Ürün Gereksinimleri `02 §3.8.3`, §3'ün Kaynak satırı · başlık notu, sürümü ve dipnotu | `02` v0.68 → v0.69 |

---

## 0. Karar prensipleri

- **Basitlikten başla.** MVP için en basit çalışan mimariyi seç; ölçekleme ihtiyacı gerçekten doğduğunda ölçekle.
- **Ama sonrasını da düşün.** Sonradan düzeltilmesi çok pahalı olan konular (olay kaybı, denetim izi eksikliği, eşzamanlılık) baştan doğru kurulur. "MVP olarak düşünme, sonrası için de düşün" bu iki maddenin dengesidir.
- **Kısıtları erken söyle.** Maliyet, ekip yetkinliği ve review edilebilirlik mimariyi doğrudan belirler. Geç açıklanan bir kısıt alınmış kararları geri aldırır.
- **Her kararın gerekçesi yazılır.** Gerekçesiz karar, altı ay sonra tartışılamaz.

**Bu projenin kısıtları (K-857).** Aşağıdaki satırlar kararların girdisidir; mimari onları seçmez, onlara uyar. Satırlardan biri değişirse ona dayanan kararlar — "Mimariye düşen yüzü" sütununun gösterdikleri — yeniden açılır. İlk dört satır Teknik mimari aşamasının açılışında proje sahibine sorulan çok kritik soruların cevaplarıdır (karar kaydı §12.1); geri kalanı üst dokümanların kabulleridir.

| # | Kısıt | Mimariye düşen yüzü | Kaynak |
|---|---|---|---|
| 0.1 | **Ürünü firmaların sunucusuna proje sahibi kurar ve bir şey bozulduğunda o bakar; iş yerinde C# / .NET kullanır.** Her task'ın kodu ayrı bir doğrulama oturumunda kabul kriterlerine, dokümanlara, CI'a ve mini güvenlik kontrolüne karşı denetlenir; bir insanın her task'ta kodu okuması bir kural değildir (`00 §D.2`, §D.6; `INSTRUCTIONS §3.3`). | Yığın, kurulumu ve arızayı üstlenen kişinin en hızlı okuyacağı yığındır (MK-01); bileşen ve araç sayısı az tutulur — her kurulumu aynı kişi kurar ve bakar (§3.2). | K-860, K-866 |
| 0.2 | **Aylık altyapı gideri tavanı yaklaşık 10–20 ABD dolarıdır — olabildiğince düşük.** Tavan ödeme sağlayıcısının komisyonunu ve alan adını kapsamaz; sunucuyu, sunucunun dışındaki yedek alanını, e-posta gönderimini ve izlemeyi kapsar. Referans kurulum dahil her kurulumda aynıdır; geliştirme ve CI giderleri firmanın gideri değildir ve tavana girmez. | Tavan dört kalem taşır: tek bir kiralık sanal sunucu · kullanım kadar ödenen nesne depolama (yedek — §7.6) · e-posta gönderimi (seçimi `08`'in) · ücretsiz katmanlı dış erişilebilirlik kontrolü (§9.4 — 2. oturum). Üç tür seçimi dışlar: ücretli lisanslı yazılım · ücretli yönetilen servis — ücretsiz katmanı yeten araç bu yasağa girmez · ikinci bir sürekli sunucu — gerekirse proje sahibine soru olarak döner. Kayıtlı ürün kararları — sıfır veri kaybı (0.13) ve yedeğin kurulum ön koşulu olması (ÖK-2) — geçerliliğini korur. | K-861 (proje sahibi), K-862 · K-407, K-629 |
| 0.3 | **Kurulum firmanın adına kiralanan bir sanal sunucuda (VPS) çalışır; ürün orada konteyner olarak kurulur. Tek desteklenen hedef budur** — paylaşımlı hosting, yönetilen bulut platformu ve firmaya göre değişen hedefler desteklenmez. Sunucu ve yedek alanı firmanın hesabındadır; veritabanı ve yedekler firmanın elindedir. | MK-07; sunucudaki bileşenler §3.2; yedeğin yeri §7.6; ortamlar §8.1 (2. oturum). | K-863 (proje sahibi) · K-410, K-629 · `10 §4.1` ÖK-2 |
| 0.4 | **Sunucunun ve yedek alanının konumu bir ürün kuralı değil, kurulum önerisidir:** kurulumu yapan Türkiye'yi önerir; referans kurulum (ÖK-12) Türkiye'dedir. Yurt dışında bir sunucu ya da yedek alanı seçilirse aktarımın şartları firmanın yükümlülüğüdür ve konum aydınlatma metnine yazılır (`02 §12.2.8`; ÖK-2, ÖK-10). | Ürün çalıştığı sunucunun konumunu denetlemez ve konuma göre davranış değiştirmez; öneri ve seçim ölçütleri `DEPLOY_RUNBOOK`'tadır. | K-864 |
| 0.5 | **Bir kurulumun sunucusu:** Linux ve konteyner çalıştırıcısı kurulabilen bir sanal sunucu; en az 2 sanal işlemci, 4 GB bellek ve 60 GB SSD disk — dijital ürün satan firmada disk, dosyaların toplamı ve yedeğin yerel kopyası kadar büyür; sabit bir genel IP adresi. Seçim ölçütleri — sağlayıcı diski kendi tarafında yedekli tutar, Türkiye'de veri merkezi, aylık bedel tavanda yedek alanına ve e-posta gönderimine yer bırakır, sağlayıcı anlık görüntüye izin verir — `DEPLOY_RUNBOOK`'tadır. | Tasarım değerleri bu ölçüde geçerlidir (§11.1 — 2. oturum); sunucu bilinçli olarak tekildir (§11.2 — 2. oturum). Anlık görüntü güncelleme öncesi geri dönüş içindir, yedeğin yerine geçmez (§7.6). | K-865 |
| 0.6 | **Kurulum başına tek firma; MVP'de çok kiracılık için hazırlık yapılmaz.** Pazaryeri kalıcı ürün sınırıdır; çok kiracılı SaaS yol haritası adayıdır ve bugünkü mimaride karşılığı yoktur. | Firma seçimi, firmalar arası ayırıcı ve kiracı yalıtımı yoktur; ikinci firma ikinci kurulumdur. SaaS'a geçiş büyüme yolunun bir adımı değil, yeni bir mimari karardır (§11.3 — 2. oturum). | devir: K-10 (park) · K-929 · `10 §4.2` SK-1 |
| 0.7 | **Tek dil Türkçe;** dil bir ayar değildir. | Arayüz, yasal metinler ve e-postalar tek dildedir; dil seçimi ve çeviri altyapısı yoktur, adreslerde dil öneki yoktur (`02 §3.30.2`). | `10 §4.2` SK-2 |
| 0.8 | **Hacim kabulleri** H-1…H-4 (`02 §11.3`) — sipariş hacmi tabanı, katalog, elli kaleme kadar sepet ve eşzamanlı ziyaretçi. Bunlar taban, tavan değildir: sistem hiçbirini uygulamaz, aşıldığında çalışmayı sürdürür, performans garanti edilmez. | Tek uygulama örneği ve tek veritabanı bu hacmi taşır (MK-13, MK-06); H-3 ve H-4'ün değerleri ve okunuşları §11.1'dedir (2. oturum). Aşıldığında tetikleyici SK-5'tedir. | `10 §4.2` SK-5 · K-403, K-409, K-928 |
| 0.9 | **Tek pazar Türkiye ve tek para birimi TRY** — kalıcı sınır. | Para tek birimde tutulur (§7.2). | `10 §4.2` SK-8 · `01 §7` S-10 |
| 0.10 | **Tek depo** — kalıcı sınır. | Stok ve kontenjan varyant başına tek sayıdır; depo ayrımı yoktur. | `10 §4.2` SK-10 · `01 §7` S-5 |
| 0.11 | **Ürün çalışma süresi taahhüdü vermez; ilan edilen planlı bakım penceresi yoktur.** Kesinti olağan kabul edilir; güncellemedeki kısa kesinti bu kabulün içindedir. | Tek örnek ve tek sunucu kabul edilir (MK-13); güncellemede eski örnek durur, yenisi başlar (§8.6 — 2. oturum). Süreler kesintide de işler ve kesinti ölçülür (§5.4); izleme kurulumu yapana gider (§9.4 — 2. oturum). | K-406, K-408 · `02 §3.34.7` |
| 0.12 | **Tek saat dilimi Türkiye saatidir;** yaz saati geçişi yoktur. | Zamanın tutuluşu ve sayımı §7.2. | K-335 · `02 §4.1.1` |
| 0.13 | **Sipariş tarafında veri kaybı toleransı sıfırdır; tek istisna sunucunun diskinin kalıcı kaybındaki en çok son bir dakikadır.** Sepet ve oturum kaybı kabul edilir. Neyin kaybolamayacağı `02 §3.34.8`'dedir. | Onaylanan işlem diske yazılmadan başarı dönmez (§7.1); sunucu dışı yedek işlem günlüğünü en geç bir dakikada bir taşır (§7.6). | K-407, K-557, K-888 (proje sahibi) · `02 §3.34.8` · `10 §1` |

*Kaynak: K-857 (tablonun yeri) · K-860, K-861, K-862, K-863, K-864, K-865, K-866 · K-10, K-335, K-403, K-406, K-407, K-408, K-409, K-410, K-557, K-629, K-888, K-928, K-929 · `10 §1`, §4.1 ÖK-2, ÖK-10, ÖK-12, §4.2 SK-1, SK-2, SK-5, SK-8, SK-10 · `02 §3.34.7`, §3.34.8, §4.1.1, §11.3, §12.2.8 · `00 §D.2`, §D.6.*

---

## 1. Sistem genel görünümü

> **Ne yazılır:** Bileşen diyagramı ve her bileşenin tek cümlelik sorumluluğu.

## 2. Teknoloji kararları

### 2.1 Teknoloji tablosu

Tablo teknoloji seçimlerini taşır; her satırın ayrıntısı "Gerekçe"nin gösterdiği bölümdedir. Mimari kararların geri kalanı kendi bölümlerinde kimlik alır (konvansiyon 2).

| # | Alan | Seçim | Alternatifler | Gerekçe |
|---|---|---|---|---|
| MK-01 | Dil ve uygulamanın biçimi | **C# / .NET; tek uygulama, sayfalar sunucuda üretilir.** Vitrin ve panel aynı uygulamanın sayfalarıdır; ayrı bir JavaScript ön yüz uygulaması yoktur. | TypeScript (Node.js) · PHP · .NET arka uç ve ayrı JavaScript ön yüz | Arızada koda bakacak kişinin rahat okuduğu dil kararı belirler (§0.1); arama motorlarının istediği sunucuda üretilen sayfa (KP-37) tek pakette karşılanır. TypeScript ve PHP o kişinin dili değildir ve PHP'nin avantajı — paylaşımlı hostinge uygunluk — barındırma kararıyla düştü (§0.3); ayrı ön yüz iki dağıtım birimi ve iki dil demektir. Karar neredeyse geri alınamaz: dili değiştirmek ürünü yeniden yazmaktır. Proje sahibinin kararı (K-866). |
| MK-02 | Uygulama çatısı ve sürüm politikası | **ASP.NET Core; .NET 10 (uzun süreli destekli — LTS).** Ürün yalnız LTS sürümlerinde yürür; bir sonraki LTS'e kullanılan sürümün desteği bitmeden geçilir — geçiş bir bakım güncellemesidir ve kurulumlara güncelleme yoluyla ulaşır (§8.6 — 2. oturum). | Kısa destekli (STS) sürümleri izlemek | .NET 10 Kasım 2025'te çıktı ve Kasım 2028'e kadar desteklenir; .NET 8'in desteği Kasım 2026'da biter — yeni ürün desteği bitmek üzere olan sürümle başlamaz. STS her ara sürümde bütün kurulumlara zorunlu bir geçiş doğurur ve bakım aboneliği sona eren kurulumu desteksiz sürümde daha erken bırakır (K-481). Kütüphane eklemenin ölçütü — lisansın ücretsiz ve ticari kullanıma açık olması (§0.2), bakımı süren bir proje olması — Kodlama Kılavuzu'ndadır (`09` park satırı) (K-867). |
| MK-03 | Veri erişimi ve şema göçleri | **Entity Framework Core; şema değişiklikleri onun göçleriyle (migration) taşınır.** | Dapper ya da elle yazılmış SQL | Eşzamanlılık denetimi (§7.3) ve göçler EF Core'da hazırdır; elle SQL her sorguda iş ve hata yüzeyi açar — gereken özel sorgu EF Core'un içinden yazılır. Göçün kuruluma uygulanması ve geri alınması §8.7'dedir (2. oturum) (K-867). |
| MK-04 | Sayfaların üretimi | **ASP.NET Core Razor Pages; vitrinin ve panelin bütün sayfaları sunucuda tam HTML olarak üretilir.** Form gönderimi gönder–yönlendir–göster (POST–redirect–GET) düzenindedir — yenilenen sayfa işlemi yinelemez; uyarılar ve alan mesajları sunucunun yanıtıdır, tarayıcıda tutulan uygulama durumu yoktur. **Ayrı bir JSON API katmanı yoktur;** makine uçları yalnız gerektiği yerde doğar — ödeme sağlayıcısının bildirimi (§4.5), parçalı dosya yükleme ve indirme (§7.7), site haritası (§3.7), canlılık ve durum uçları (§9.4 — 2. oturum). | Blazor (etkileşimli sunucu) · Blazor WebAssembly ya da tarayıcıda çalışan uygulama · MVC denetleyicileri | Arama motorlarının okuduğu veri ilk yanıtın içindedir (KP-37; `02 §3.30.5`) ve kapalı sayfalar (`02 §3.30.6`) ek bir ön yüz katmanı olmadan karşılanır. Etkileşimli sunucu her açık sekme için süren bir bağlantı ve bellek tutar — tek küçük sunucuda (§0.5) ziyaretçi sayısıyla büyür ve bağlantı koptuğunda sayfa çalışmaz; tarayıcıda çalışan uygulamanın ilk yanıtında içerik yoktur ve mobil öncelikli vitrinde (`02 §3.34.2`) sayfa ağırlaşır; MVC eşdeğer bir yoldur, ama Razor Pages sayfa başına bir dosyayla ekran envanterine (`04 §4`) bire bir oturur (K-868). |
| MK-05 | Ön yüz araçları | **Düz JavaScript modülleri ve düz CSS; ön yüz çatısı ve npm tabanlı derleme hattı yoktur.** JavaScript yalnız iyileştirir: gezinme, sepet, ödeme adımı, hesap ekranları ve panelin formları JavaScript kapalıyken de tamamlanır; parçalı dosya yükleme panelde JavaScript ister. Üçüncü taraf betik ve yazı tipi sunucusu kullanılmaz. Uygulanışı §3.8. | React, Vue gibi bir çatı ve npm hattı · kısmi sayfa güncellemesi için bir kütüphane (htmx vb.) · CSS çatısı (Bootstrap, Tailwind) | Klavyeyle tam gezinme ve fare olmadan tamamlanan ödeme (`02 §3.34.5`) sunucuda üretilen standart formla en az kodla sağlanır. Çatı ve npm hattı ikinci bir dil ekosistemi ve derleme zinciri getirir (§0.1); bugünkü ekranların hiçbiri kısmi sayfa güncellemesi gerektirmez — ihtiyaç doğarsa kütüphane ekleme ölçütüyle (MK-02) karara bağlanır; Tailwind derleme hattı ister, Bootstrap'in hazır görünümü tasarım tabanının (`04 §2.1`) üstüne ikinci bir görsel dil koyar (K-869; `02 §8.3.3`). |
| MK-06 | Veritabanı | **PostgreSQL 18** — ilişkisel, tek örnek; uygulamanın yanında aynı sunucuda kendi konteynerinde çalışır. Veri katmanının stratejisi §7'dedir. | SQL Server Express · MySQL / MariaDB · SQLite | Ücretsiz ve açık kaynaktır (§0.2), az bellekle çalışır, satır kilidini ve koşullu güncellemeyi (§7.3) ve sürekli günlük aktarımıyla noktasal geri dönüşü (§7.6) yerleşik taşır; EF Core sağlayıcısı (Npgsql) olgundur. SQL Server Express'in veritabanı başına 10 GB tavanı on yıllık saklama (Z-28, Z-30) boyunca dolma riski taşır, küçük sunucuda uygulamayla belleği paylaşır ve tavanı kaldıran sürümler ücretli lisanstır; MySQL ve MariaDB'nin EF Core sağlayıcısı üçüncü taraftır ve noktasal geri dönüşün araçları daha az yerleşiktir; SQLite'ın tek yazıcı kilidi arka plan işleri ve panel işlemleri biriktiğinde bekletir ve ayrı bir veritabanı sunucusuna büyümeyi kapatır (K-870). |
| MK-07 | Barındırma ve paketleme biçimi | **Firmanın adına kiralanan sanal sunucu; ürün orada Linux konteynerleri olarak kurulur — tek desteklenen hedef.** Sunucunun asgari tanımı §0.5'tedir. | Paylaşımlı hosting · yönetilen bulut platformu · firmaya göre birkaç hedef | Kendiliğinden işler (`02 §4.3`), büyük dijital dosya (P-26) ve sunucu dışı sürekli yedek paylaşımlı hostingte güvenilir çalışmaz; yönetilen platformun gideri tavanı aşar (§0.2), çoğu yurt dışındadır — kişisel verinin yurt dışına aktarımı yükümlülüğü doğar (`02 §12.2.8`) — ve ürünü platforma bağlar; birden çok hedef kurulum, test ve destek yükünü hedef sayısıyla katlar (§0.1). Konteyner aynı paketi ileride başka bir ortama taşımayı kapatmaz. Proje sahibinin kararı (K-863; K-865). |
| MK-08 | Ters vekil ve sertifika | **Caddy 2** — HTTPS'i sonlandırır, sertifikayı Let's Encrypt'ten kendiliğinden alır ve yeniler, HTTP'yi HTTPS'e yönlendirir, isteği uygulamaya geçirir ve görselleri sunar. Yeri §3.2'dedir. | Sertifikayı uygulamanın kendisinin alması · nginx ve ayrı sertifika aracı (certbot) | Sertifika yenilemesi ve yönlendirme uygulamanın sorumluluğuna girmez; uygulama yeniden başlarken sertifika işi durmaz. nginx ile certbot iki araç ve bir yenileme zamanlayıcısı ister; Caddy aynı işi tek yapılandırma dosyasıyla yapar (K-872). |
| MK-09 | Yedek aracı ve yedeğin yeri | **pgBackRest; yedek S3 uyumlu bir nesne depolamada, sunucunun dışında ve firmanın hesabında.** Strateji §7.6'dadır. | Yalnız gecelik yedek · sağlayıcının anlık görüntüsünü yedek saymak · yedeğin kurulumu yapanın hesabında durması | Sürekli günlük aktarımı tavanın içinde kalan en dar penceredir. Yalnız gecelik yedekte sunucunun diski kaybolursa bir günün siparişleri gider (§0.13); anlık görüntü aynı sağlayıcıda durur, noktasal dönüş vermez ve firmanın elinde ayrı bir kopya değildir; kurulumu yapanın hesabındaki yedeğe abonelik bitince firma ulaşamaz (ÖK-2; K-410) (K-878, K-879). |
| MK-10 | Konteyner tanımı | **Tek bir Compose tanımı; her imaj sürüm numarasıyla sabitlenir.** Tanım ürünün parçasıdır ve repoda sürümle gelir; kuruluma özgü ayar ve sır dosyaları tanımın dışında, sunucudadır. Ayrıntısı §8.2'dedir (2. oturum). | Ürünü konteynersiz, doğrudan sunucuya kurmak · imajları "latest" etiketiyle çekmek | Proje sahibinin kararı ürünü konteyner olarak kurmaktır (§0.3); veritabanının sürümü de paketle birlikte sabitlenir. Tek tanım her kurulumu aynı yapar ve kurulum ile kurtarma adımlarını kısaltır (§7.6); "latest" aynı sürümü iki kurulumda iki farklı imajla çalıştırır ve geri dönüşü belirsizleştirir (K-872, K-920). |
| MK-11 | İmaj kayıt defteri | **GitHub Container Registry.** Repo herkese açık kaldıkça imajlar da açıktır ve sunucu kimlik bilgisi olmadan çeker; repo özel yapılırsa her sunucu salt okuma bir çekme belirteci alır. | Docker Hub | Repo GitHub'dadır ve yayın aynı kimlikle yapılır; Docker Hub'ın ücretsiz katmanında çekme sınırı vardır (bilgim tarihli — karar kaydının notu) (K-920). |
| MK-12 | CI ve yayın | **GitHub Actions; iskeletin adımlarının adları, sırası ve kapıları değişmez, her adımın işi yığına bağlanır; `publish.yml` kalır.** Ana dala her merge'de imajlar commit kimliğiyle, sürüm etiketinde (`vANA.ARA.YAMA`) sürüm numarasıyla yayımlanır; **kurulumlara otomatik dağıtım yoktur** — yayımlanan sürüm kurulumlara güncelleme turuyla ulaşır (§8.6 — 2. oturum). Adımların işi §8.3'tedir (2. oturum); komutların kendisi D-01'in işidir (§2.3). | `publish.yml`'in silinmesi · contract adımında uç şeması testleri · ana daldan kurulumlara otomatik dağıtım | Kurulumlar imajı kayıt defterinden çeker; yayın olmadan her kurulumda kaynaktan derleme gerekirdi. Ayrı bir JSON API katmanı olmadığı için (MK-04) servisler arası sözleşme yoktur ve contract adımı NOT_APPLICABLE'dır. Otomatik dağıtım firmaların sitesine insan bakmadan değişiklik indirirdi (K-919, K-921). |

*Kaynak: K-858 (MK kimliği) · K-863, K-866 (proje sahibinin kararları) · K-865, K-867, K-868, K-869, K-870, K-872, K-878, K-879, K-919, K-920, K-921 · K-263, K-410, K-481 · `02 §3.30`, §3.34, §4.3, §8.3.3, §12.2.8 · `10 §2` KP-37 · `04 §2.1`, §4.*

### 2.2 Seçimi başka dokümana ya da kuruluma bırakılanlar

Aşağıdaki seçimler bu dokümanın değildir; burada yalnız ölçütü ve evi yazılır.

| # | Seçim | Ölçüt | Evi |
|---|---|---|---|
| 2.2.1 | E-posta gönderim altyapısı | Kullanım kadar ödenen ya da ücretsiz katmanlı; tavanın içinde (§0.2); yanıtları firmanın iletişim e-postasına düşecek biçimde kurulur (ÖK-3) | Entegrasyon Spesifikasyonu (`08` park satırı) |
| 2.2.2 | Ödeme sağlayıcısı | Firmanın sözleşmesidir; 3D Secure'lu barındırılan ödeme sayfası, ödeme sonucunu sorgulama ve kısmi iade dahil iade (ÖK-4). Desteklenen sağlayıcı sayısı çok kritik soru adayıdır (§3.6.4) | Firma · biçimi `08` |
| 2.2.3 | Görüntü işleme kütüphanesi | Lisansı ücretsiz ve ticari kullanıma açık (§0.2); görevleri §7.7.2'dedir | Kodlama Kılavuzu (`09` park satırı) |
| 2.2.4 | Kayıt kütüphanesi | Yapılandırılmış satır yazar; maskeleme kodda uygulanır (§9.1 — 2. oturum) | Kodlama Kılavuzu (`09` park satırı) |
| 2.2.5 | Dış erişilebilirlik kontrolü | Ücretsiz katmanında kurulum sayısı kadar kontrol taşır, HTTP yanıt koduna bakar ve kendi e-postasıyla uyarır; örnek UptimeRobot'un ücretsiz katmanı — bilgim tarihli, kurulumda doğrulanır (§9.4 — 2. oturum) | Kurulumu yapan · `DEPLOY_RUNBOOK` |
| 2.2.6 | Sanal sunucu ve nesne depolama sağlayıcısı | §0.5'in seçim ölçütleri; nesne depolama S3 uyumlu ve silmeye karşı korumayı (sürüm tutma ya da nesne kilidi) destekler (§7.6) | Firma seçer, kurulumu yapan kurar · `DEPLOY_RUNBOOK` |
| 2.2.7 | Bağımlılık güncelleme önerileri | GitHub Dependabot — ücretsiz; CI'ın derleme adımı bilinen açıkları ayrıca tarar (§8.8 — 2. oturum) | `SETUP.md` §4 ve `CI_CD_SETUP.md` — D-01'in kapanışında |
| 2.2.8 | Geliştirme ortamının yerel araçları | E-postaları yakalayan yerel bir araç — hiçbir e-posta dışarı çıkmaz; ödeme sağlayıcısının test ortamı (§8.1 — 2. oturum) | Kodlama Kılavuzu (`09` park satırı) |

*Kaynak: K-862, K-882, K-915, K-918, K-919, K-926 · K-865, K-879 · `10 §4.1` ÖK-3, ÖK-4.*

### 2.3 Yığının kuruluma yansıması (D-01)

`DEFERRED_BACKLOG.md`'nin D-01 kalemi — `SETUP.md` §2 ve §4'ün yığına bağlı parçaları — Teknik mimari aşamasının kapanışından sonra, ilk implementation task'ından önce kapanır; `SETUP.md`'nin değişikliği proje sahibinin onayıyla yapılır (GUARDRAILS §2). Bu alt bölüm D-01'in her parçasına bu dokümanın verdiğini yazar; komutların ve dosya desenlerinin kendisi D-01'in işidir.

| # | D-01'in parçası | Bu dokümanın verdiği |
|---|---|---|
| 2.3.1 | `CONTEXT.md` §1 — teknoloji yığını | MK-01…MK-12: C# / .NET 10, ASP.NET Core Razor Pages, EF Core, PostgreSQL 18, düz JavaScript ve CSS; Linux konteynerleri, Compose, Caddy 2, pgBackRest; GitHub Actions ve GitHub Container Registry |
| 2.3.2 | `CONTEXT.md` §3 — kod klasörleri | Modüller §3.3'tedir; klasör düzeni Kodlama Kılavuzu'nun (`09`) işidir ve ilk implementation task'ında `CONTEXT.md` §3'e klasör düzeyinde yazılır |
| 2.3.3 | `.gitignore` ve `.gitattributes`'ın yığına uyarlanması | .NET derleme çıktısı, test sonuçları ve IDE dosyaları; kuruluma özgü ayar ve sır dosyaları repoya girmez — yerleri sunucudadır (§6.x — 2. oturum) |
| 2.3.4 | `.env.example` | Kurulumun sır olmayan ayar dosyasının şablonudur; sır değerleri ne bu dosyada ne repoda durur (§6.x — 2. oturum) |
| 2.3.5 | `ci-commands.env`'in komutları | Lint — .NET biçim ve çözümleyici denetimi · Build — yayın yapılandırmasıyla derleme, uyarılar hata sayılır, bilinen açık taraması · Unit — birim testleri ve modül sınırlarının mimari testleri (§3.4.5) · Integration — gerçek PostgreSQL 18'e karşı · Contract — `NOT_APPLICABLE` · Migration dry-run — göç demeti boş veritabanına ve son yayımlanan sürümün şemasına · Package build — uygulama ve yedek aracı imajları · E2E — uçtan uca doğrulama fazında bağlanır. Adımların ayrıntısı §8.3'tedir (2. oturum) |
| 2.3.6 | `ci.yml`'in runtime adımları | .NET 10 SDK; entegrasyon adımı için PostgreSQL 18 hizmeti; paketleme adımı için konteyner derlemesi |
| 2.3.7 | `publish.yml` kararı | Kalır (MK-12); imajlar GitHub Container Registry'ye yayımlanır ve yazma izni D-01'de açılır |

*Kaynak: K-866, K-867, K-910, K-920, K-921, K-932 · `DEFERRED_BACKLOG.md` D-01 · `SETUP.md` §2, §4 · `CONTEXT.md` §1, §3 · `CI_CD_SETUP.md` §1, §2.*

---

## 3. Servis yapısı ve sınırlar

> **Ne yazılır:** Monolit mi, modüler monolit mi, servisler mi; modül sınırları; modüller arası iletişim kuralı.

### 3.1 Uygulamanın biçimi

**3.1.1 Ürün tek dağıtılabilir uygulamadır — modüler monolit (MK-13).** Vitrin, panel ve arka plan işleri — zamanlanmış işler, iş kuyruğu, imha, görsel türevleri — aynı süreçte, aynı konteynerde çalışır ve tek bir veritabanını kullanır. Modüller kod içinde sınırlıdır (§3.3, §3.4). Arka plan işleri uygulamanın içinde barındırılan hizmetlerdir; kalıcı durumları veritabanındadır ve süreç yeniden başladığında kaldıkları yerden sürerler (§5.1).

**3.1.2 Tek örnek çalışır.** Aynı anda iki uygulama örneği yoktur; güncellemede eski örnek durur, yeni örnek başlar — kısa kesinti §0.11'in kabulündedir (§8.6 — 2. oturum). Tek örnek bellek içi önbelleğin (§7.10) ve zamanlayıcının (§5.1) dayanağıdır; ikinci örneğin koşulları büyüme yolundadır (§11.3 — 2. oturum).

**3.1.3 Elenenler.** **Ayrık servisler** (vitrin, panel, ödeme, bildirim) her servis için ayrı dağıtım, ayrı sır ve servisler arası kimlik ister ve tek küçük sunucuda (§0.5) belleğe sığmaz; hacim tabanı (§0.8) ve kurulum başına tek firma (§0.6) bu ölçekte onlara iş bırakmaz. **Ayrı bir arka plan işçisi süreci** ikinci bir dağıtım birimi ve sır yüzeyi açar; kazancı — web isteklerinin işlerden etkilenmemesi — bu hacimde ölçülemez. Uzun işler sınırlı eşzamanlılıkla koşar (§5.1); modül sınırı korunduğu için işler ileride ayrı sürece alınabilir (§11.3 — 2. oturum).

*Kaynak: K-871 · K-10, K-865 · `10 §4.2` SK-1, SK-5.*

### 3.2 Sunucudaki bileşenler ve ağ

**Bir kurulumun sunucusunda dört bileşen vardır; dördü de konteynerdir ve tek bir Compose tanımıyla birlikte kurulur (MK-14; MK-10).**

| # | Bileşen | Sorumluluğu | Ağ |
|---|---|---|---|
| 3.2.1 | **Uygulama** (MK-13) | Vitrinin ve panelin sayfaları, makine uçları, zamanlayıcı ve iş kuyruğu; kök olmayan bir kullanıcıyla çalışır | İsteği ters vekilden alır; veritabanına bağlanır; ödeme sağlayıcısına, e-posta altyapısına ve Google'a giden bağlantıları şifrelidir |
| 3.2.2 | **Veritabanı** — PostgreSQL 18 (MK-06) | Ürünün bütün kalıcı durumu; çatının şifreleme anahtar halkası dahil (§7.6.3) | Yalnız sunucunun iç ağında; dışarıya kapalı |
| 3.2.3 | **Ters vekil** — Caddy 2 (MK-08) | HTTPS'i sonlandırır, sertifikayı alır ve yeniler, HTTP'yi HTTPS'e yönlendirir, isteği uygulamaya geçirir, görselleri önbellek başlıklarıyla sunar (§7.7.1); erişim kaydını yazar (§9.1 — 2. oturum) | Dışarıya açık tek bileşen — 80 ve 443 |
| 3.2.4 | **Yedek aracı** — pgBackRest (MK-09) | Veritabanının işlem günlüğünü ve tam yedeğini, dosyaları ve kayıt dosyalarını sunucunun dışındaki nesne depolamaya taşır (§7.6) | Veritabanına bağlanır; dışarıya yalnız nesne depolamaya giden şifreli bağlantı |

**3.2.5 İç ağlar ayrıdır:** ters vekil uygulamaya, uygulama ve yedek aracı veritabanına ayrı iç ağlardan ulaşır; ters vekil veritabanına ulaşamaz. Her bileşen veritabanına kendi kullanıcısıyla bağlanır (§7.1.3); kullanıcıların sırları ve dağıtım yarıçapı §6.x'tedir (2. oturum).

**3.2.6 İki tek seferlik çalışma sürekli bir bileşen değildir:** **göç hizmeti** yalnız güncellemede, uygulama imajındaki göç demetini şema sahibi kullanıcısıyla uygular (§8.7 — 2. oturum); **yönetim komutları** kurulum tarafının işlemlerinde uygulama konteynerinde tek seferlik çalışır (§8.5 — 2. oturum). Sunucuya bakım erişiminin sınırı §8.9'dadır (2. oturum).

**3.2.7 İkinci bileşen doğduğu için şablonun §6.x'indeki üç soru — sırrın nerede durduğu, dağıtım yarıçapı, döndürülmesi — gün 0'da cevaplanmıştır;** cevap bu bileşen listesine göre yazılır (§6.x — 2. oturum).

**3.2.8 Sunucuda başka sürekli bileşen yoktur:** ayrı bir kuyruk sunucusu (§4.3), önbellek sunucusu (§7.10), arama motoru (§7.9), izleme yığını (§9.4 — 2. oturum) ve işçi süreci (§3.1.3) yoktur — her biri tavanın dışladığı bellek, sır ve bakım yüzeyidir (§0.2).

**3.2.9 Elenenler.** **Uygulamanın sertifikayı kendisinin alması:** sertifika yenilemesi ve yönlendirme uygulamanın sorumluluğuna girer, uygulama yeniden başlarken sertifika işi de durur (MK-08). **Veritabanının sunucuya doğrudan kurulması:** proje sahibinin kararı ürünü konteyner olarak kurmaktır (§0.3) ve veritabanının sürümü paketle birlikte sabitlenir.

*Kaynak: K-872 · K-870, K-878, K-879, K-910 (iç ağlar), K-920 (göç hizmeti, kök olmayan kullanıcı), K-923 (yönetim komutları), K-927 · K-862, K-886, K-887, K-891, K-918 (olmayan bileşenler) · `10 §4.1` ÖK-1.*

### 3.3 Modüller

**3.3.1 Uygulama dokuz modüldür (MK-15).** Modüller Ürün Gereksinimleri'nin kural gruplarından ve sekiz akışından türer (`02 §2.1`, §3); her modül kendi verisinin tek sahibidir.

| # | Modül | Sahip olduğu | Türediği yer |
|---|---|---|---|
| 3.3.2 | **Katalog** | Ürün, varyant ve seçenek, kategori, stok ve hizmet kontenjanı, fiyat ve KDV oranı, indirim, kupon, görseller, dijital ürün dosyası, site içi arama | `02 §3.3`–§3.12; akış 5 |
| 3.3.3 | **Sepet ve sipariş** | Sepet, sipariş onayı ve ayırma (§4.2), sipariş ve kalemleri, durum makinesinin geçiş birimi (§4.1), sipariş numarası ve erişim anahtarı, sipariş anında donan değerler, onay adımı ve sözleşme belgeleri, kargo ve teslim | `02 §3.16`–§3.20, §3.22–§3.25, §5; akış 1, 2, 6 |
| 3.3.4 | **Ödeme** | Ödemenin başlatılması, sağlayıcı bildiriminin gelen kaydı, son sorgu, geri ödeme kaydı ve karta para gönderen istek (§4.5, §4.6) | `02 §3.21`, §7.4; akış 1, 6 |
| 3.3.5 | **İptal, cayma ve iade** | İptal, gecikme feshi, cayma beyanı, iade ve iade malının teslim alınması, iade reddi, ayıp talebi, müşterinin geri ödeme IBAN'ı | `02 §3.26`, §7; akış 3 |
| 3.3.6 | **Üyelik** | Müşteri hesabı, e-posta doğrulaması ve e-posta değişikliği, profil ve adres defteri, hesabın silinmesi, Google ile giriş | `02 §3.13`–§3.15; akış 4 |
| 3.3.7 | **Kurumsal içerik** | İçerik kayıtları, ana sayfa ve menü, marka, duyuru, iletişim talebi | `02 §3.27`–§3.29, §3.32; akış 7 |
| 3.3.8 | **Bildirim** | Gönderim kaydı — e-postanın kuyruğu —, gövdenin gönderim anında üretilmesi, gönderici, kanal hâli (§4.4) | `02 §9` |
| 3.3.9 | **Yönetim ve işlem izi** | Yönetici hesapları ve davet, işlem izi (§4.7), bekleyen işler, satış özeti, dışa aktarma, kurulum kontrol listesi | `02 §10`; akış 6, 8 |
| 3.3.10 | **Ayarlar** | Firma kimliği ve satış kapısı, ödeme yöntemleri ve firmanın IBAN'ı, kargo ücreti ve süreler, iade adresi, teslimat illeri, yasal metinler ve sürümleri | `02 §3.1`, §3.33, §11.1; akış 8 |

**3.3.11 Modüllerin altında ortak bir altyapı vardır; o bir modül değildir ve iş kuralı taşımaz:** oturum, bağlantı ve deneme kayıtları (§6.1, §6.3, §6.4 — 2. oturum) · zaman kaynağı, iş günü takvimi ve yuvarlama işlevi (§7.2) · zamanlayıcı ve iş kuyruğunun çalıştırıcısı (§5.1) · imha işinin düzenleyicisi — her modül kendi verisinin imhasını kendi arayüzünden yapar (§7.4) · adresin addan üretilmesi (§3.7.1) · dosya deposu arayüzü (§7.7.1) · bellek içi önbellek (§7.10) · hata yakalama katmanı ve kayıt (§10.3, §9.1 — 2. oturum) · dış servislerin bağlantı katmanı (§3.6).

**3.3.12 Vitrin ve panel modül değildir;** aynı uygulamanın iki sayfa alanıdır (§3.5). Bir sayfa bir ya da birkaç modülün arayüzünü çağırır; veriye doğrudan dokunmaz. Yönetim komutları (§8.5 — 2. oturum) da modüllerin arayüzünü çağıran bir giriş noktasıdır.

**3.3.13 Elenen — modülleri teknik katmanlara (sunum, iş, veri) göre ayırmak:** bir kuralın değişikliği her katmanı dolaşır, `02`'nin kural grupları kodda görünmez olur ve mimari testler (§3.4.5) korunacak bir sınır bulamaz. **Elenen — sipariş, ödeme ve iptalin tek modülde toplanması:** en büyük kural kümesi tek modülde birikir; geçiş biriminin tek yazıcı ilkesi (§3.4.4) zaten bu üç modülün geçişlerini tek yerde toplar.

*Kaynak: K-932 · K-871 · `02 §2.1`, §3 · `03 §0.2`.*

### 3.4 Modüller arası iletişim kuralı

**3.4.1 Bir modül öteki modülün verisine yalnız onun açık arayüzünden erişir (MK-15)** — yazma da okuma da; başka bir modülün tablosuna doğrudan yazılmaz ve doğrudan okunmaz.

**3.4.2 Çağrı eşzamanlıdır ve çağıranın veritabanı işleminin içindedir.** Bir geçişin iç yan etkisi başka bir modüldeyse — iptalde stoğun, kontenjanın ve kupon hakkının dönmesi katalogda, işlem izinin satırı yönetimde — geçiş birimi o modülün arayüzünü aynı veritabanı işlemi içinde çağırır; geçiş ya bütün iç yan etkileriyle olur ya hiç olmaz (§4.3). Uygulama içinde olay yayını (olay veriyolu) yoktur.

**3.4.3 Dış yan etki iş kuyruğundan gider:** e-posta, kart iadesinin başlatılması, ödeme sonucunun sorulması ve görsel türevleri geçişle aynı işlemde iş kuyruğuna bir satır olarak yazılır; satırın işleyicisi işin sahibi modüldedir (§4.3).

**3.4.4 Tek yazıcılar.** Siparişin, kalemlerinin ve kalem kayıtlarının durum alanlarına yalnız sepet ve sipariş modülünün geçiş birimi yazar; öteki modüller, sayfalar ve arka plan işleri geçişi adıyla ister (§4.1). İşlem izine yalnız yönetim modülünün iz arayüzü (§4.7), gönderim kaydına yalnız bildirim modülü (§4.4) yazar.

**3.4.5 Sınır kodda sınanır:** bir modül öteki modülün iç türlerine başvurmaz; CI'ın birim adımındaki mimari testler bunu denetler (§8.3 — 2. oturum). Sınırların kodda nasıl korunduğu Kodlama Kılavuzu'nun işidir (`09` park satırı).

**3.4.6 Elenen — uygulama içi olay yayını:** iç yan etkinin geçişle aynı işlemde yazıldığını görünmez kılar ve bir işleyici başarısız olduğunda geçişin geri alınıp alınmadığı belirsizleşir; dış yan etkiler için kalıcı kuyruk zaten vardır (§4.3). **Elenen — modüllerin birbirinin tablosunu okumasının serbest olması:** sınır yalnız yazmada korunursa okuyan modül ötekinin iç yapısına bağlanır ve bir şema değişikliği (§8.7 — 2. oturum) iki modülü birden kırar.

*Kaynak: K-932 · K-871, K-889, K-891, K-892, K-921.*

### 3.5 Vitrin ve panel

**3.5.1 Vitrin ve panel aynı uygulamanın iki sayfa alanıdır ve aynı alan adındadır (MK-16; ÖK-1):** panel ayrı bir yol alanıdır, alt alan adı değildir; yolun adı API Tasarımı'nın (`07`) işidir. Tek sertifika, tek ters vekil yapılandırması ve kendi alan adına kilitli çerezler (§6.7 — 2. oturum) bu düzenle çalışır.

**3.5.2 İki hesap türü iki kapıdan girer** (`02 §10.2.5`, §8.1.7): vitrinin girişi yalnız müşteri hesaplarını, panelin girişi yalnız yönetici hesaplarını arar. İki tür ayrı tablolarda, ayrı adlı çerezlerle ve iki ayrı kimlik şemasıyla tutulur; e-posta adresi kendi türü içinde tekildir. Panelde Google ile giriş yoktur. Oturumun ve şemaların ayrıntısı §6.1'dedir (2. oturum).

**3.5.3 Panel varsayılan olarak kapalıdır:** panelin her sayfası yönetici oturumu ister; açık kalan sayfalar panel girişi, şifre sıfırlama, davet kabulü ve doğrulama ekranlarıdır (§6.2 — 2. oturum). Panelin bütün sayfaları arama motorlarına kapalıdır (§3.7.4).

**3.5.4 Vitrin panelin oturumunu yalnız iki iş için tanır:** yönetici şeridi (`04` 2.9, §3.7) ve taslak kaydın önizlemesi (`02 §3.27.25`; `04` 3.7.2). Yönetici oturumu vitrinde müşteri sayılmaz — hesap alanını, sepeti ve siparişi değiştirmez (`04` 2.9.4); vitrinin kimlik şeması yalnız müşteri hesabını tanır.

**3.5.5 Çerez süzgeci yalnız vitrinin yanıtlarına uygulanır;** panel ziyaretçinin değil yöneticinin tarayıcısıdır (§6.7 — 2. oturum).

**3.5.6 Elenen — panel için alt alan adı:** ikinci bir DNS kaydı ve sertifika ister ve kurulumun sırasına (ÖK-1) bir adım ekler; vitrindeki yönetici şeridi ve taslak önizlemesi oturumun iki alan adı arasında taşınmasını gerektirir.

*Kaynak: K-932 · K-588, K-868, K-899, K-906, K-908, K-912 · `02 §3.27.25`, §8.1.7, §10.2.5 · `04` 2.9, §3.2, §3.7 · `10 §4.1` ÖK-1.*

### 3.6 Dış servislerin sınırı — bağlantı katmanı

**3.6.1 Üç dış servis vardır ve her biri bir bağlantı katmanının arkasındadır (MK-17):** ödeme sağlayıcısı (ÖK-4), e-posta gönderim altyapısı (ÖK-3) ve Google (ÖK-6). Modül servisin ürün düzeyindeki işini çağırır — ödemeyi başlat, sonucu sor, iade iste, gelen bildirimi doğrula · e-postayı gönder · kimliği doğrula —; sağlayıcıya özgü biçim — istek, imza, yanıt kodları ve onların sınıfları — bağlantı katmanındadır. Biçimin kendisi Entegrasyon Spesifikasyonu'nun (`08`) işidir.

**3.6.2 Ürün kuralı çekirdekte kalır, bağlantı katmanına inmez:** son sorgu yanıtsız kalırsa sipariş iptal edilmez (`02 §6.1.2`; §4.5) · sağlayıcıya bağlanılamadığı için ödeme sayfası açılmamış siparişin iptali ödeme denemesi sayılmaz (L-7; `02 §8.2`) — bağlantı hatasının ayırt edilmesi `08`'dedir · karta para gönderen istek bir kez işlenir (§4.6) · e-posta denemesinin sonucu üç sınıftır — gönderildi, alıcı hatası, kanal hatası (§10.2 — 2. oturum) — ve yanıt kodlarının sınıflara eşlenmesi `08`'dedir.

**3.6.3 Dış çağrı veritabanı kilidinin içinde beklenmez:** siparişin kilidini tutan işlem dış servisi çağırmaz; dış iş kuyruktan yürür ve sonucu kilit alınarak yazılır (§4.3, §7.3.2). Kart hattında sağlayıcıya yönlendirme sipariş onayının işleminden sonradır (§4.2). Sağlayıcının bildirimi önce kaydedilir, sonra işlenir (§4.5).

**3.6.4 Desteklenen ödeme sağlayıcısının sayısı Entegrasyon Spesifikasyonu'nun (`08`) kararıdır** ve Entegrasyon spesifikasyonları aşamasının açılışında proje sahibine sorulacak çok kritik soru adayıdır; bağlantı katmanı ödeme işlerini sağlayıcıdan bağımsız adlarla taşır.

**3.6.5 Altyapı da aynı kalıptadır:** dosyalara dosya deposu arayüzüyle erişilir — bugün sunucunun diski; nesne depolamaya geçiş kod değişikliği değil yapılandırmadır (§7.7.1; §11.3 — 2. oturum). Yol haritası adaylarının getirdiği dış servisler — kargo şirketi, banka bağlantısı, SMS (YH-2, YH-3, YH-16) — aynı kalıpla eklenir ve büyüme yolunu kapatmaz (§11.3 — 2. oturum).

**3.6.6 Bir dış servis çökerse** — yeniden deneme, geri düşme, kullanıcının gördüğü — §10.1'dedir (2. oturum); ürün davranışı `03 §10.1`'dedir.

*Kaynak: K-932 · K-556, K-614, K-881, K-890, K-893, K-894, K-913, K-929 · `10 §4.1` ÖK-3, ÖK-4, ÖK-6 · `02 §3.21`, §6.1.2, §8.2 · `03 §10.1` · plan konusu TM3-04 (sağlayıcı sayısının çok kritik soru adaylığı).*

### 3.7 Sayfa adresleri ve arama motoru verileri

**3.7.1 Adres kaydın adından sunucuda üretilir (MK-18).** Kuralın kendisi `02 §3.30.1`–§3.30.2'dedir: Türkçe harfler sadeleşir, aynı adres doluysa sonuna sayı eklenir, kayıt ilk kez yayına alındığında adres sabitlenir ve bir daha değişmez; yönlendirme sistemi yoktur (`02 §3.30.3`). Sadeleştirme tek bir ortak işlevdir — site içi aramanın (§7.9) ve önerilen stok kodunun (§7.8) kullandığı işlevle aynıdır. Adresin tekilliği veritabanında korunur; adresin veride tutuluşu Veri Modeli'nin (`06`), adreslerin önekleri API Tasarımı'nın (`07`) işidir.

**3.7.2 Arama motoru verileri ilk yanıttadır** (MK-04): sayfa başlığı ve açıklama sayfanın başlık bölümünde; **yapılandırılmış veri** schema.org sözlüğüyle JSON-LD biçiminde — ürün (fiyat, stok durumu), firma, kırıntı yolu, SSS —; **paylaşım önizlemesi** Open Graph etiketleriyle — başlık, açıklama, sayfanın ana görseli, görsel yoksa logo. Değerler `02 §3.30.5`'in kuralıyla kayıttan üretilir; firmanın elle doldurduğu bir alan yoktur.

**3.7.3 Site haritası** bir makine ucundan, XML site haritası biçiminde, yayındaki ürünlerden, kategorilerden, içerik sayfalarından ve sabit liste sayfalarından istek anında üretilir; bellek içi önbellekte tutulur ve bu kayıtlardan biri kaydedildiğinde geçersiz kılınır (§7.10). Arşivlenmiş ürün ve taslak site haritasına girmez.

**3.7.4 Arama motorlarına kapalı sayfalar** (`02 §3.30.6`) ve panelin bütün sayfaları her yanıtta `noindex` taşır — HTML'de robots meta etiketi, HTML olmayan yanıtta `X-Robots-Tag` başlığı. **robots.txt kapalı sayfaları engellemez;** yalnız site haritasının adresini gösterir — engellenen sayfayı arama motoru okuyamaz, `noindex`'i göremez ve başka sitelerdeki bağlantılardan bildiği adresi dizinde tutar. Arşivlenmiş ürün sayfası doğrudan bağlantıyla açılmaya devam eder (`02 §3.30.6`).

**3.7.5 Elenen — yapılandırılmış veri için sayfa içi işaretleme (microdata):** veri şablonların HTML'ine dağılır ve sayfa düzeni her değiştiğinde yeniden sınanmak zorundadır; JSON-LD sayfada tek bir bloktur. **Elenen — kapalı sayfaları robots.txt ile engellemek:** 3.7.4'ün gerekçesi.

*Kaynak: K-933 · K-868, K-885, K-886, K-887 · devir: K-283 (dizine kapatmanın biçimi), `03` 0.1.4 ve `04` 4.4.5 (arama motoru verileri, site haritası, paylaşım önizlemesi) · `02 §3.30` · `10 §2` KP-37 · `04 §1.1` (matrisin "mimari" satırları — KP-37, `02 §3.30.1`, §3.30.2, §3.30.5, §3.30.6, `03 §0.1.4`).*

### 3.8 Ön yüz ve tasarım tabanının uygulanması

**3.8.1 Ön yüz düz JavaScript ve düz CSS'tir (MK-05);** sayfalar sunucuda üretilir ve temel akışlar JavaScript kapalıyken de tamamlanır. Statik dosyalar çatının kendi hattıyla sıkıştırılır ve sürüm izi taşır — değişen dosya yeni adres alır. Üçüncü taraf betik ve yazı tipi sunucusu yoktur; yazı tipleri sitenin kendisinden gelir (`02 §8.3.3`).

**3.8.2 Üç genişlik sınıfı CSS medya sorgularıyla uygulanır** (`04 §2.1.1`): sınır değerleri tek bir stil dosyasında tanımlanır ve `04 §2.1.1`'in değerleriyle birebirdir — değerlerin evi orasıdır. Sayfa cihazın genişliğini görüntü alanı olarak alır. Geniş sınıfta içeriğin sınırlı bir genişlikte ortalanması ve hiçbir içeriğin bir sınıfta gizlenmemesi (`04` 2.1.2) stil kuralıdır; ayrı bir mobil site ve sunucuda cihaz ayrımı yoktur (`02 §3.34.1`). En dar desteklenen genişlik P-42'dir; altında düzenin bozulması kabuldür (`02 §3.34.4`).

**3.8.3 Marka rengi** (`02 §3.29`) sayfaya CSS değişkeniyle girer — değer ayardan sunucuda yazılır, stil dosyası yeniden üretilmez.

**3.8.4 Tarayıcı ve erişilebilirlik tabanı.** Tarayıcı tabanı son iki büyük sürümdür (`02 §3.34.3`; KP-70); derleme hattı olmadığı için JavaScript ve CSS'te bu tabanın desteklediği özellikler kullanılır — düzeni Kodlama Kılavuzu'ndadır (`09` park satırı). Erişilebilirlik tabanı (`02 §3.34.5`; KP-71) sunucuda üretilen standart form ve bağlantıyla karşılanır; doğrulaması Doğrulama Protokolü'ndedir (`12`).

*Kaynak: K-869, K-868 · devir: `04` 2.1.1 (genişlik sınıflarının sınırlarının uygulanması — karar satırı yok; `PHASE3_CONFLICT_SCAN.md` §6.2) · K-263, K-462 (P-42) · `02 §3.29`, §3.34.1, §3.34.3, §3.34.4, §3.34.5, §8.3.3 · `10 §2` KP-69, KP-70, KP-71 · `03` 0.1.4.*

---

## 4. Durum yönetimi ve olaylar

> **Ne yazılır:** Durum makinesi nerede yaşar, geçişler nasıl korunur, yan etkiler nerede tetiklenir, olaylar nasıl güvenilir taşınır.

Durumların adları, geçişler ve koşulları `02 §5`'te, akışa bağlanışları `03 §1`'dedir; bu bölüm onları taşıyan mekanizmayı yazar. Checklist'in Teknik mimari hatırlatmasının iki "baştan doğru kur" maddesi buradadır: **olay kaybı** (§4.3, §4.4) ve **denetim izi** (§4.7).

### 4.1 Durum makinesinin yeri — geçiş birimi

**4.1.1 Sipariş durum makinesi sepet ve sipariş modülünün içinde, tek bir geçiş biriminde yaşar (MK-35).** Her geçiş adı olan bir işlemdir — S1…S11, Ö1…Ö5 ve kalem kayıtları (`02 §5.4`, §5.5, §5.8) — ve izinli geçişler kodda tek bir tabloda durur (beyaz liste). Siparişin, kalemlerinin ve kalem kayıtlarının durum alanlarına yalnız bu birim yazar; panel, vitrin ve arka plan işleri geçişi adıyla ister (§3.4.4).

**4.1.2 Her geçiş aynı sırayla işler:** siparişin satır kilidini alır (§7.3.2) → koşulunu siparişin o anki hâline karşı denetler — gecikme feshine uğramış kalemin hattı, açık kalemi kalmayan siparişin kapanışı ve hizmet kaleminin penceresi dahil (`02 §5.7`, §5.8; K-590, K-592, K-593) → durumu yazar → **geçiş kaydına** yalnız eklenen bir satır yazar: kim (müşteri, yönetici ya da sistem), ne zaman, hangi geçiş, sebep → iç yan etkilerini aynı veritabanı işleminde yazar (§4.3). Koşul tutmazsa geçiş uygulanmaz ve sebebi söylenir (`02 §3.31.2`).

**4.1.3 Açık kalemi kalmayan siparişin kapanışı** (`02 §5.4`) aynı işlemin sonunda değerlendirilir; ayrı bir iş beklemez.

**4.1.4 Geri alınamaz geçişin "geri al" yolu yoktur** (`02 §5.9`): düzeltme yeni bir geçiştir ve geçiş kaydı temizlenmez. Geçiş kaydı sipariş tarafındaki sıfır kaybın (§0.13) "durum geçişleri" satırının karşılığıdır; işlem izi (§4.7) yalnız yönetici işlemlerini taşır ve onun yerine geçmez.

**4.1.5 Satış kapısı** (`02 §3.1.5`) saklanmaz: her istekte veritabanındaki ayarlardan hesaplanır ve sipariş onayının işleminde yeniden denetlenir (§4.2). Dış ön koşulların panelin hesaplayabildiği yüzü yalnız ayarlardır (`10 §4.1` ÖK-5, ÖK-8…ÖK-11).

**4.1.6 Elenenler.** **Durum alanlarının serbest güncellenip kaydetmeden önce denetlenmesi:** her ekran ve iş kendi denetimini yazar ve bir yol atlandığında beyaz liste sessizce delinir. **Durum makinesi kütüphanesi:** iki eksen ve kalem kayıtlarıyla kurulan bağ (`02 §5.6`) bir kütüphanenin tek eksenli modeline sığmaz. **Veritabanı tetikleyicileriyle denetim:** kural iki yerde yazılır ve hatanın sebebi kullanıcıya taşınamaz.

*Kaynak: K-889 · devir: K-590, K-591, K-592, K-593, K-594 (geçiş koşulları, onay anı, kapanış kaydı) · K-874, K-887 · `02 §3.1.5`, §3.31.2, §3.34.8, §5 · `03 §1` · `10 §4.1`.*

### 4.2 Sipariş onayı

**4.2.1 Onay isteği müşterinin gördüğünü taşır (MK-36):** özetin parmak izi — kalemler, adetler, birim fiyatlar, indirim, kuponun payı, kargo ve toplam — ile Ön Bilgilendirme Formu'nun ve Mesafeli Satış Sözleşmesi'nin sürüm kimlikleri; ayrıca tek kullanımlık onay anahtarı (§4.5.1).

**4.2.2 Sunucu onayı tek bir veritabanı işleminde, bu sırayla işler:** (1) satış kapısını ve limitleri (L-7, L-8) denetler → (2) sepeti yeniden değerlendirir, parmak izini ve sürümleri karşılaştırır — fark varsa sipariş oluşmaz ve güncel özet farkı söyler (`02 §3.17.2`, §3.24.3); girecek kalem yoksa ödeme adımı açılmaz (`02 §3.16.7`) → (3) **stoğu, hizmet kontenjanını ve kupon hakkını koşullu güncellemeyle ayırır:** ayırma yalnız kalan adet yetiyorsa yazılır; satırlar sabit bir sırayla — varyant kimliğine göre, kupon en sonda — kilitlenir → (4) sipariş numarasını ve erişim anahtarını üretir (§6.4 — 2. oturum), değerleri dondurur (`02 §3.23`), aynı sepete bağlı ödenmemiş önceki siparişi iptal eder (`02 §4.3`) ve bildirimleri kuyruğa yazar (§4.3).

**4.2.3 Ayırma tutmazsa işlem bütünüyle geri alınır** ve müşteri kalemin ayrılamadığını görür (`02 §6.2`). Aynı son adedi aynı anda onaylayan iki müşteriden birinin ayırması tutar, ötekininki koşullu güncellemede düşer; sabit kilit sırası iki onayın birbirini beklemede kilitlemesini önler.

**4.2.4 Kart hattında sağlayıcıya yönlendirme işlemden sonra yapılır;** dış çağrı kilidin içinde beklenmez (§3.6.3).

*Kaynak: K-890 · devir: K-129, K-591, K-594 (onay anında sürüm karşılaştırması ve boş sepet) · K-150, K-182 · `02 §3.16.7`, §3.17, §3.23, §3.24.3, §4.3, §6.2.*

### 4.3 Yan etkiler ve iş kuyruğu — olay kaybı

**4.3.1 İki tür yan etki vardır (MK-37).** (1) **İç yan etkiler** — stok, kontenjan ve kupon hakkının dönmesi, geçiş kaydı, işlem izinin satırı, panel işaretleri — geçişle **aynı veritabanı işleminde** yazılır: geçiş ya hepsiyle olur ya hiç olmaz. (2) **Dış yan etkiler** — e-posta, kart iadesinin başlatılması, ödeme sonucunun sorulması, görsel türevlerinin üretilmesi — geçişle aynı işlemde veritabanındaki **iş kuyruğuna** bir satır olarak yazılır (outbox) ve işlem bittikten sonra çalıştırılır. Uygulama çökerse satır kaybolmaz; uygulama yeniden başlayınca işlenir.

**4.3.2 Kuyruğun çalıştırıcısı** uygulamanın içinde tek bir arka plan döngüsüdür (§5.1): işlem bitince uyandırılır, ayrıca birkaç saniyede bir kuyruğa bakar, satırı kilitleyerek alır — aynı satırı iki çalışma almaz — ve sonucu satıra yazar.

**4.3.3 Teslim en az bir kezdir.** Her işleyici aynı satırı ikinci kez gördüğünde sonucu değiştirmeyecek biçimde yazılır; satırın kimliği dış isteğe anahtar olarak gider (§4.6). Gönderimden sonra ama sonucu yazmadan çöken süreçte e-posta ikinci kez gider; kayıp yerine bu kabul edilir.

**4.3.4 Yeniden deneme iş türünün kuralıdır:** e-posta P-38'in aralıklarıyla (§4.4) · kart iadesi kendiliğinden yeniden denenmez (§4.6; `02 §7.4`). Tamamlanan iş satırı gecelik temizlikte silinir (§5.2); satır kişisel veri taşımaz, kayda kimlikle işaret eder.

**4.3.5 Elenenler.** **Ayrı bir kuyruk sunucusu (RabbitMQ gibi):** sunucuda ayrı bileşen, bellek ve sır (§0.2) ve veritabanıyla aynı işleme giremez — kayıp penceresi yine doğar. **Hazır iş kütüphanesi (Hangfire, Quartz.NET):** ihtiyaç tek bir tablo ve tek bir döngüdür; kütüphane kendi şemasını ve panosunu getirir ve iş kaydını geçişle aynı işleme bağlamak ek yapılandırma ister (bilgim tarihli — karar kaydının notu; implementation'da doğrulanır). **E-postayı geçişin içinde göndermek:** dış servisi beklerken siparişin kilidi uzar ve geri alınan işlemin e-postası gitmiş olur.

*Kaynak: K-891 · K-683, K-871, K-874 · `02 §4.3`, §9 · `03 §7` · checklist'in Teknik mimari hatırlatması (olay kaybı).*

### 4.4 E-postanın gönderimi ve gönderim kaydı

**4.4.1 Her e-posta gönderim kaydında bir satır olarak doğar (MK-38)** — doğduğu geçişle aynı veritabanı işleminde, "gönderilecek" hâlinde; gönderim kaydı aynı zamanda e-postanın kuyruğudur (§4.3). Satır türü (B-1…B-16, F-1…F-6, hesap ve yönetim e-postaları), alıcıyı, ilgili kaydı (sipariş, talep, hesap) ve olayın değerlerini (takip bilgisi gibi) taşır.

**4.4.2 Gövde gönderim anında** olayın değerlerinden ve siparişin donmuş verisinden üretilir ve kalıcı olarak saklanmaz; B-1'in donmuş sürümü siparişte durur (`02 §9.1`). Gövdenin ve şablonların biçimi Entegrasyon Spesifikasyonu'nun (`08`) işidir.

**4.4.3 Başarısız gönderim** P-38'in aralıklarıyla aynı satırda yeniden denenir (Z-26); üç yeniden deneme de başarısızsa satır "ulaşmadı" olur ve `02 §9.1.6`'nın işareti ya da uyarısı bu hâlden okunur. Panelden yeniden gönderim yeni bir satırdır; başarılı olursa işaret kalkar (`02 §6.1.1`).

**4.4.4 Satır Z-34 ya da Z-43'ün süresi kadar saklanır** ve imha işinde silinir (§7.4); tür, alıcı, zaman, deneme sayısı ve sonuç sipariş ayrıntısının e-posta geçmişini besler (`04` E-37). Denemenin sonucunun sınıfları, kanal uyarısının eşiği ve firma bildirimleri uyarısının çıkış ve kalkış anı §10.2'dedir (2. oturum).

*Kaynak: K-892 · K-321, K-462 (P-38), K-522, K-759, K-761, K-804, K-913 · `02 §4.2` Z-26, Z-34, Z-43, §6.1.1, §9.1.6 · `03 §7`.*

### 4.5 Tek işlenme — sipariş onayı, sağlayıcı bildirimi ve son sorgu

**4.5.1 Sipariş bir kez oluşur (MK-39):** ödeme adımı her gösterildiğinde forma tek kullanımlık bir onay anahtarı konur; anahtar sipariş kaydında tekildir. Aynı anahtarla ikinci gönderim — çift tıklama, geri tuşu, yeniden gönderilen form — yeni sipariş açmaz, var olan siparişin sayfasına götürür.

**4.5.2 Sağlayıcının bildirimi önce kaydedilir, sonra işlenir:** bildirim doğrulanır (imza ve tutar — `08`), gerekli alanlarıyla gelen bildirim kaydına yazılır ve sağlayıcıya hemen "alındı" denir; işleme kuyruktan yapılır (§4.3). Sağlayıcının işlem kimliği tekildir — aynı bildirim ikinci kez gelirse işlenmez. Bildirim ucu imzayla korunur ve IP hız sınırına girmez (§6.3 — 2. oturum).

**4.5.3 Ödeme sonucu tek bir yoldan uygulanır:** bildirim de son sorgu da aynı işleve gider; işlev siparişin kilidini alır (§7.3.2) ve sonucu o anki hâle göre uygular — Bekliyor'da Ö1 ya da başarısızlık; zaten Ödendi'yse hiçbir şey yapmaz; İptal edildi'yse kendiliğinden iadeyi kuyruğa yazar (`02 §6.2.16`; §4.6). Son sorgunun yinelenmesi §5'tedir.

**4.5.4 Müşterinin ve yöneticinin tekrarlanan işlemi** güncel hâle göre ele alınır: yapılmış işlem ikinci kez işlenmez, kullanıcı güncel hâli görür (§7.3.2).

**4.5.5 Tanınmayan bildirim** — siparişi kayıtta olmayan ya da doğrulanamayan — işlenmez; gelen bildirim kaydında ve sistem kaydında kalır ve diskin kalıcı kaybından sonraki kurtarmada firmanın karşılaştırmasında okunur (§7.6.2). Ekranın "sonucu belirsiz" hâline hangi hatanın düştüğü §10.3'tedir (2. oturum).

*Kaynak: K-893 · devir: K-556, K-589 (son sorgu ve geç gelen ödeme — tek işlenme) · K-874, K-888, K-745 · `02 §3.31.2`, §6.1.2, §6.2.16 · `08` park satırı (bildirimin doğrulanması).*

### 4.6 Karta para gönderen isteğin tek işlenmesi

**4.6.1 Karta para gönderen her istekten önce geri ödeme kaydı yazılır (MK-40)** — tutar, kalem ve adetler, "istek gönderiliyor" hâli —; istek bu kaydın kimliğini sağlayıcıya tekrar anahtarı ya da referans olarak taşır.

**4.6.2 Üç sonuç vardır:** **başarılı** → Ö3–Ö5 işler · **başarısız** — sağlayıcı reddetti → "geri ödeme gerçekleşmedi" işareti düşer ve sistem kendiliğinden yeniden denemez (`02 §7.2.8`) · **sonucu bilinmiyor** — zaman aşımı, bağlantının kopması → istek yeniden gönderilmez, ödeme ekseni değişmez ve sonuç sağlayıcıya sorulur; firmanın yeniden denemesi ve havale yolu yalnız "gerçekleşmedi" hâlinde açılır.

**4.6.3 Bu dokümanda ilke, Entegrasyon Spesifikasyonu'nda biçim:** sorgunun biçimi ve sıklığı, sağlayıcının tekrar anahtarını destekleyip desteklemediği ve sonucu bilinmeyen iadenin panelde bir işaret isteyip istemediği `08`'in kararıdır; işaret gerekirse karar kaydına döner (K-705'in park satırı).

*Kaynak: K-894 · devir: K-705 (iade isteğinin tek işlenmesi — ilkesi burada, biçimi `08`'de) · K-683, K-624 · `02 §7.2.8` · `10 §4.1` ÖK-4.*

### 4.7 İşlem izi — denetim izi

**4.7.1 İşlem izi yönetici işlemlerinin kaydıdır;** kuralı `02 §10.3`'tedir — sekiz alan, her satırda kim, ne zaman, ne değişti; ize girmeyen işlemler ve sistem olayları orada sayılır. İz on yıllık saklama süresi boyunca değiştirilemez ve silinemez (`02 §8.5.2`).

**4.7.2 Satır işlemle aynı veritabanı işleminde yazılır:** iz satırı iç yan etkidir (§4.3.1) — yönetici işlemi ya iz satırıyla olur ya hiç olmaz. Satırı yalnız yönetim modülünün iz arayüzü yazar (§3.4.4); öteki modüller işlemlerinin satırını o arayüzle, kendi işlemlerinin içinde yazar.

**4.7.3 Değiştirilemezlik veritabanında korunur:** iz tablosunda uygulamanın veritabanı kullanıcısı yalnız ekler ve okur; süresi dolan satırı (Z-30) yalnız veritabanındaki imha fonksiyonu siler (§7.5). Kod hatası da izi değiştiremez.

**4.7.4 Kurulum tarafının işlemleri ize yazılmaz** ve bildirim üretmez (`02 §10.2.7`); kaydı uygulama kaydıdır — komutun adı ve hedef hesabın iç kimliği (§8.5, §9.1 — 2. oturum).

**4.7.5 İz ihlal incelemesinin üç kaydından biridir** (`02 §8.5.1`) ve panelde okunur (`02 §10.3.4`); kayıtların okuma komutunun kapsamı değildir (§9.3 — 2. oturum). Geçiş kaydı (§4.1.2) izden ayrıdır: müşterinin, yöneticinin ve sistemin bütün geçişlerini taşır.

*Kaynak: plan konusu TM4-04 · K-877, K-889, K-891, K-917, K-923, K-932 · devir: K-380 (ihlal incelemesinin üç kaydı) · K-310, K-702 · `02 §8.5`, §10.2.7, §10.3 · checklist'in Teknik mimari hatırlatması (denetim izi).*

---

## 5. Arka plan işleri ve zamanlanmış görevler

Kendiliğinden işleyen anlar ve süreler `02 §4.2`, §4.3 ve `03 §4`'tedir; bu bölüm onları çalıştıran mekanizmayı ve işlerin envanterini yazar. Bütün işler uygulamanın içindedir (§3.1.1).

### 5.1 Çalıştırıcılar — zamanlayıcı ve iş kuyruğu

**5.1.1 Zamanlanmış işleri uygulamanın içinde tek bir zamanlayıcı çalıştırır; dakikada bir uyanır (MK-41).** İşin zamanı kendi kaydındadır — siparişin ödeme süresinin bittiği an, hatırlatmanın anı, IBAN'ın silineceği gün, e-postanın sonraki denemesi — ve sipariş onaylandığında ya da süre yeniden başladığında hesaplanıp yazılır. Zamanlayıcı her uyanışta zamanı gelmiş kayıtları sorar; ayrı bir "iş takvimi" tutulmaz. Böylece kaçırılan bir çalışma sonrakinde kendiliğinden yakalanır ve aynı işi iki kez görmek sonucu değiştirmez.

**5.1.2 Zamanı gelen her kayıt kendi geçişiyle işlenir** — siparişin kilidi altında, o anki hâle karşı (§4.1).

**5.1.3 Tek çalışma:** her iş türü çalışırken veritabanında bir kilit tutar — aynı iş aynı anda iki kez çalışmaz; güncellemede eski ve yeni örnek bir an üst üste binse bile. Uzun işler — imha, görsel türevleri — sınırlı eşzamanlılıkla koşar ve web isteklerini bekletmez.

**5.1.4 İş kuyruğu** ikinci çalıştırıcıdır: geçişlerin dış yan etkilerini işler (§4.3.2).

**5.1.5 Elenenler.** **Her iş için ayrı zamanlanmış görev** ("şu siparişi şu saatte iptal et"): süre değiştiğinde ya da sipariş başka yoldan kapandığında görevlerin güncellenmesi ayrı bir doğruluk yükü açar; zaman kayıttaysa tek gerçek kayıttır. **İşletim sisteminin zamanlayıcısı (cron) ya da ayrı bir işçi konteyneri:** ikinci bir dağıtım birimi ve sır yüzeyi (§3.1.3). **Hazır zamanlama kütüphanesi:** dakikalık bir döngü ve kayıttaki zaman ihtiyacı karşılar; kütüphane kendi tablolarını getirir (§4.3.5).

*Kaynak: K-895 · K-336, K-354, K-449, K-556, K-871, K-875, K-891 · `02 §4.3` · `03 §4`.*

### 5.2 İşlerin envanteri

**5.2.1 İşler üç çalıştırıcıya ve bir "iş yok" grubuna dağılır (MK-42):**

| # | İş | Çalıştırıcı ve an | Kaynak |
|---|---|---|---|
| 5.2.2 | Kart ödeme süresinin dolması — önce son sorgu; yanıtsızsa sorgu yinelenir (5.3.3), ödeme alınmamışsa kendiliğinden iptal | Zamanlayıcı — kayıttaki an | Z-7 · `02 §6.1.2` |
| 5.2.3 | Havale ödeme süresinin dolması — kendiliğinden iptal; kesintide erteleme 5.4.3'tedir | Zamanlayıcı — kayıttaki an | Z-8 · K-667 |
| 5.2.4 | Havale hatırlatması (B-3) — düzeltmeyle yeniden başlayan sürede bir kez daha | Zamanlayıcı — son iş gününün 08:00'i (5.3.1) | Z-9 · `02 §3.21.6` |
| 5.2.5 | Malı dönmeyen caymada IBAN'ın silinmesi | Zamanlayıcı — kayıttaki gün | Z-38 · `02 §4.3` |
| 5.2.6 | E-postanın yeniden denenmesi | Zamanlayıcı — gönderim kaydındaki sonraki deneme anı (§4.4.3) | Z-26 |
| 5.2.7 | Doğrulanmamış kaydın ve bekleyen e-posta değişikliğinin ömrü dolunca silinmesi; kayıt sırasında süresi dolmuş bekleyen kayda rastlanırsa o da o anda silinir | Zamanlayıcı — kayıttaki an | Z-1 · `02 §3.13` |
| 5.2.8 | Periyodik imha (§7.4) — sistem kayıt dosyaları, iz satırları, imha kaydının kendisi ve kalemlerin indiremeyeceği dosya kayıtları dahil | Gecelik iş — 03:00 (5.3.2) | Z-36 · `02 §12.2.7` |
| 5.2.9 | Geçici kayıtların temizliği — tamamlanmayan parçalı yükleme (§7.7.3), süresi dolan oturum ve bağlantı kayıtları, penceresi geçen deneme kayıtları, tamamlanmış iş kuyruğu satırları, uzun süre açılmayan misafir sepeti (§6.7 — 2. oturum) | Gecelik iş — imhayla birlikte | `02 §4.2` Z-6 |
| 5.2.10 | Görsel türevlerinin üretilmesi (§7.7.2) | İş kuyruğu — yüklemeden sonra | `02 §3.11` |
| 5.2.11 | İptal edilmiş siparişe geç gelen kart ödemesinin iadesi ve kart hattındaki kendiliğinden iadeler (§4.6) | İş kuyruğu — geçişten sonra | `02 §6.2.16`, §7.4 |
| 5.2.12 | Bildirimlerin gönderimi (§4.4) | İş kuyruğu — gönderim kaydının satırı | `02 §9` |

**5.2.13 İş gerektirmeyen süreler okuma anında denetlenir:** indirimin ve kuponun tarih aralığı (Z-20, Z-22) — fiyat ve geçerlilik her okumada tarihe bakılarak hesaplanır, fiyat geçmişi indirimin aralığını kendi kaydı olarak taşır —, duyurunun aralığı (Z-23), bağlantı ve oturum ömürleri (Z-1…Z-5, Z-45) ve tanınan tarayıcı işareti (Z-46). Aynı kalıpla `03 §4.1`'in "kendiliğinden işlem yok" dediği süreler de — kargoya verme süresi, yasal teslim sınırı, cayma pencereleri, ayıp talebinin süresi (Z-10, Z-11, Z-13, Z-14, Z-18) — iş doğurmaz: düğmenin açık olup olmadığı her okumada süreye bakılarak hesaplanır.

**5.2.14 Saat:** sunucunun saati ağ zaman servisiyle (NTP) eşitlenir; uygulama zamanı tek bir kaynaktan okur (§7.2.4). Saatin eşitlenmesinin kurulumu `DEPLOY_RUNBOOK`'tadır.

*Kaynak: K-896 · K-66, K-75, K-102, K-129, K-556, K-685, K-883, K-891, K-901, K-906 · `02 §4.2`, §4.3, §5.11 · `03 §4.1`, §4.2.*

### 5.3 İşlerin saatleri ve gecenin sırası

**5.3.1 Havale hatırlatması son iş gününün 08:00'inde (Türkiye saati) gider (MK-43)** — o günün ilk gönderim çalışması budur; kural "son iş gününün başı"dır (`02 §3.21.6`), saat onun uygulanışıdır.

**5.3.2 Gecelik imha her gece 03:00'te başlar;** gecelik tam yedek ondan önce biter — yedeğin saati `DEPLOY_RUNBOOK`'tadır (§7.6.1).

**5.3.3 Son sorgu yanıtsız kalırsa beş dakikada bir yinelenir,** sağlayıcı yanıt verene kadar; her yineleme sistem kaydına yazılmaz, yalnız ilk yanıtsızlık ve sonuç yazılır. Aralığın sağlayıcının kotasıyla uyumu Entegrasyon Spesifikasyonu'nun (`08`) işidir.

**5.3.4 Gecenin sırası:** gecelik tam yedek ve dosya eşitlemesi — yedek aracının kendi zamanlamasıyla (§7.6.1) → 03:00 imha ve geçici kayıtların temizliği → 04:30 işletim sisteminin yeniden başlatma gerektiren güvenlik güncellemesi — yalnız gerektiğinde (§8.8 — 2. oturum).

**5.3.5 Saatler ürün sabitidir ve panelden değişmez;** `02 §11`'e girmez — tasarım değeridir (konvansiyon 5).

*Kaynak: K-897 · K-556, K-875, K-878, K-926 · `02 §3.21.6`.*

### 5.4 Kesintinin tespiti ve dönüşteki sıra

**5.4.1 Kesinti nabızla ölçülür (MK-44).** Zamanlayıcı otuz saniyede bir, önce sitenin genel adresindeki sağlık ucuna kendi sunucusundan ters vekil üzerinden bir istek yapar; istek başarılıysa veritabanına bir **nabız** yazar. İki nabız arasındaki boşluk **iki dakikayı** aşarsa aralık — son nabızdan yeni nabza — bir **kesinti kaydı** olur. Böylece uygulamanın, veritabanının ya da ters vekilin durduğu ve sitenin dışarıdan açılmadığı aralık — panelin ve kendiliğinden işlerin çalışmadığı aralık — ölçülür. Sağlık ucunun ters vekilden açık olması kurulumun parçasıdır (`DEPLOY_RUNBOOK`).

**5.4.2 Süreler kesintide dondurulmaz** (`02 §3.34.7`, §4.3): kesintide zamanı gelen işler site döndüğünde kaçırdıkları sırayla çalışır.

**5.4.3 Dönüşte sıra:** önce kesinti kaydı yazılır; sonra ödeme süresi kesinti aralığında dolmuş her havale siparişinin son anı dönüşten sonraki ilk iş gününün 23:59'una taşınır — sipariş sayfası yeni son günü gösterir, yeni hatırlatma gitmez (`02 §3.21.6`); sonra zamanı geçmiş bütün işler **zamanlarına göre tek sırada** işlenir — kart hattında iptal önce son sorguyla. Nabız yazılamıyorken havale iptali işlenmez; başka işler sürer.

**5.4.4 Öz denetimin uzun başarısızlığı** — son nabzın eskimesi — durum ucunun uyarı koşuludur ve kurulumu yapana gider (§9.4 — 2. oturum).

**5.4.5 Sitenin kesintisinde ürünün içinden bilgilendirme yoktur** (`03 §10.3.1`); kesintinin ürün davranışı `03 §10.3`'tedir, dış bağımlılıkların tablosu §10.1'dedir (2. oturum).

*Kaynak: K-898 · devir: K-666, K-667 (park — kesintinin tespiti ve kaçan işlerin dönüşte sırayla çalışması), `02 §4.3`, `03` 4.2.12 ve §10 girişi · K-685, K-183, K-918 · `02 §3.34.7`, §4.2 Z-8 · `03` 4.2.11, §10.3.*

---

## 6. Güvenlik mimarisi

> **Ne yazılır:** Kimlik doğrulama, yetkilendirme, secret yönetimi, servisler arası kimlik, hız sınırlama, hassas veri saklama.

### 6.x Secret yönetimi (bu alt bölüm zorunludur)

Üç soru **gün 0'da** cevaplanır — ikinci bileşen (servis, worker, yardımcı süreç) doğduğu anda karar verilmiş olur ve sonradan geri almak tüm dağıtım yüzeyini yeniden yazmak demektir:

| Soru | Ne yazılır |
|---|---|
| **Nerede durur** | Ortam değişkeni / secret store / dosya. Değerler **asla** repo'da; repo yalnız **şablonu** izler (`.env.example`) ve `.gitignore` bunu mekanik olarak zorlar. |
| **Dağıtım yarıçapı (blast radius)** | Her secret **yalnız onu kullanan bileşene** geçilir. İmza/anahtar materyali diğer bileşenlere hiç verilmez. Paylaşılan bir secret varsa **bileşen-başı varyantı** olur — böylece bir sızıntı ikinci yüzeyi açmaz. |
| **Rotation** | Her secret için: kim üretir, ne sıklıkla döner, dönerken sistem ayakta kalır mı, eski değer ne kadar geçerli kalır. |

**Ek kurallar:**
- Loglarda maskeleme: secret ve token değerleri log çıktısına **hiçbir seviyede** yazılmaz.
- Secret envanteri `DEPLOY_RUNBOOK.md §B`'de tutulur — bu bölüm **stratejiyi**, runbook **envanteri** taşır.

> Template'in geri kalanı sırrı yalnız tek eksende yasaklar ("koda gömme"). Sırrın **nereye dağıtıldığı** ayrı bir karardır ve yalnız burada sorulur.

## 7. Veri katmanı kararları

> **Ne yazılır:** Veritabanı seçimi, eşzamanlılık kontrolü, saklama/silme stratejisi, yedekleme. Şema `06`'da.

Bu bölüm verinin stratejisini yazar; tabloların, alanların ve türlerin kendisi Veri Modeli'nin (`06`) işidir ve bu dokümanın kararlarının veriye düşen yüzleri `06`'nın "Aşama 4'ten park edilen girdiler" bloğundadır.

### 7.1 Veritabanı ve işlem bütünlüğü

**7.1.1 Veritabanı PostgreSQL 18'dir (MK-06):** ilişkisel, tek örnek; uygulamanın yanında aynı sunucuda kendi konteynerinde, yalnız iç ağda çalışır (§3.2.2).

**7.1.2 Siparişin bütün yazımları işlem (transaction) içindedir ve onaylanan işlem diske yazılmadan başarı dönmez.** Bu, sipariş tarafındaki sıfır kaybın (§0.13) sunucu içindeki yarısıdır: uygulama, veritabanı ya da sunucu çökerse onaylanmış işlem kaybolmaz. Sunucunun dışındaki yarısı yedeklemedir (§7.6).

**7.1.3 Veritabanı kullanıcıları bileşen başınadır:** uygulama kullanıcısı şema değiştiremez ve işlem izinde yalnız ekler ve okur (§7.5); şema sahibi kullanıcısı yalnız göç hizmetinde (§8.7 — 2. oturum), yedek kullanıcısı yalnız yedek aracında çalışır; bakımda ve kayıtların okunmasında salt okuma kullanıcısı kullanılır (§9.3 — 2. oturum). Kullanıcıların sırları ve dağıtım yarıçapı §6.x'tedir (2. oturum).

*Kaynak: K-870 · K-872, K-877, K-910, K-917, K-927 · `02 §3.34.8`.*

### 7.2 Zamanın ve paranın temsili

**7.2.1 Zaman saat dilimli damga olarak, UTC'de tutulur; Türkiye saatiyle sayılır ve gösterilir (MK-19).** Tarih olarak anlam taşıyan değer — bir sürenin son günü, iş günü — tarih olarak tutulur. Ürün ikinci bir saat dilimi desteklemez (§0.12).

**7.2.2 Süre sayımı `02 §4.1`'in kuralıdır:** gün olarak belirlenen süre başlangıç gününün ertesi günü başlar ve son günün 23:59'unda — Türkiye saatiyle — dolar; dakika ve saatle ölçülen süreler başladıkları andan işler. Bir sürenin bittiği an kaydına UTC damgası olarak yazılır ve zamanlayıcı oradan okur (§5.1.1).

**7.2.3 İş günü takvimi ürünle gelen bir veri dosyasıdır:** resmî tatiller dosyadadır ve firma panelden düzenlemez; liste bakım sürümleriyle yenilenir (§8.8 — 2. oturum), kapsamadığı yılda iş günü hesabı yalnız hafta sonunu dışlar (`02 §4.1.2`). İş günüyle sayılan anlar Türkiye saatiyle hesaplanır ve UTC olarak saklanır.

**7.2.4 Uygulama zamanı tek bir kaynaktan okur;** sunucunun saati ağ zaman servisiyle eşitlenir (§5.2.14). Tek kaynak sürelerin sınanabilmesinin de dayanağıdır (`09` park satırı).

**7.2.5 Para ondalıklı sayıyla tutulur; kayan noktalı sayı kullanılmaz.** Hassasiyet kuruştur (`02 §3.8.4`); tek para birimi TRY'dir ve kalıcıdır (§0.9). Alanların türleri Veri Modeli'nin (`06`) işidir.

**7.2.6 Kuruşa yuvarlamanın yönü tek kuraldır ve evi `02 §3.8.3`'tür:** tek bir tutar kuruşa yuvarlanırken en yakın kuruşa gidilir ve tam yarım kuruş yukarı yuvarlanır; bir tutar parçalara orantılı bölünürken parçalar aşağı yuvarlanır ve bölünmeden kalan kuruş en büyük parçaya yazılır; işleme giren adetlerin payı `02 §7.1.7`'nin kuralıyla ayrılır. **Hesap kodda tek bir yuvarlama işlevinden yapılır ve yön açıkça yazılır** — .NET'in varsayılan yuvarlaması tam yarımı çift sayıya götürür ve bu kurala uymaz (`09` park satırı).

*Kaynak: K-870, K-934 · devir: K-335 (zamanın depolanma biçimi), K-336, K-338 · K-558, K-895, K-896 · `02 §3.8`, §4.1, §7.1.7 · `10 §4.2` SK-8.*

### 7.3 Eşzamanlılık kontrolü

**7.3.1 Kayıt varyantı — iyimser eşzamanlılık (MK-20).** Her düzenlenebilir kayıt bir sürüm değeri taşır. Düzenleme formu kaydı açtığı andaki sürümü taşır; kaydetmede veritabanı yalnız sürüm hâlâ aynıysa yazar ve sürümü artırır — tek bir koşullu güncelleme, arada ayrı bir okuma yoktur. Sürüm değişmişse yazma olmaz ve `04` 2.10.1'in mesajı çıkar: "Güncel hâli aç" kaydın güncel hâlini açar ve kullanıcının yazdıkları ekranda kalır; "Yine de kaydet" formu güncel sürümle yeniden gönderir — üzerine yazma bilinçli ikinci bir gönderimdir ve o arada bir değişiklik daha olduysa uyarı yeniden çıkar. Kilit yoktur. Kapsam ürün ve varyantları, kupon, kurumsal içerik ve ayarlardır (`02 §3.31.1`); **ürünün sürümü varyant tablosunun değişikliğini de kapsar**; bölümleri ayrı kaydedilen ekranda her bölüm kendi sürümünü taşır. Sürümün veride tutuluşu Veri Modeli'nin (`06`) işidir.

**7.3.2 Sipariş varyantı — sürüm ve kısa satır kilidi (MK-21).** Sipariş bir sürüm değeri taşır; sürüm siparişin, kalemlerinin, kalem kayıtlarının ve siparişe bağlı müşteri girdilerinin — geri ödeme IBAN'ı dahil — her değişikliğinde artar. Her işlem siparişin satırını kısa bir veritabanı kilidiyle tutarak uygulanır:
- **Yöneticinin işlemi** ekranda gördüğü sürümü taşır; işlem siparişin satırını veritabanı işlemi boyunca kilitler ve sürümü karşılaştırır — sürüm değişmişse uygulanmaz ve panel güncel hâli gösterir (`04` 2.10.2).
- **Müşterinin işlemi** — iptal, cayma, IBAN girişi — ve **sistemin kendiliğinden geçişi** — ödeme süresinin dolması, sağlayıcı bildirimi, son sorgu — aynı kilidi alır ve geçişin koşulunu siparişin **o anki** hâline karşı denetler: beyaz listede izinliyse uygulanır, değilse reddedilir ve sebebi söylenir (§4.1.2).
- Geçiş yan etkileriyle aynı veritabanı işleminde yazılır (§4.3). Kilit yalnız işlemin süresi kadardır ve dış çağrıyı beklemez (§3.6.3); kullanıcıya görünen bir kilit yoktur (`02 §3.31.2`).

**7.3.3 Ayırma** — stok, hizmet kontenjanı, kupon hakkı — koşullu güncellemeyle korunur: ayırma yalnız kalan adet yetiyorsa yazılır (§4.2.2).

**7.3.4 Elenenler.** **Kayıt açıldığında kilit** ve **sessiz üzerine yazma** `02 §3.31.1`'in eledikleridir. **Kaydetme anında alanların tek tek karşılaştırılması:** her kayıt türü için ayrı karşılaştırma kodu ister ve alan alan birleştirme zaten elendi. **Son değişiklik zamanıyla karşılaştırma:** aynı anda düşen iki kayıt ayırt edilmez; sürüm sayısı kesindir. **Siparişte yalnız sürüm karşılaştırması, kilitsiz:** yan etkilerini ayrı tablolara yazan işlem — geri ödeme kaydı, stok dönüşü — koşul denetiminden önce yazma riski taşır; kilit bütün işlemi tek sıraya sokar. **Sistem geçişlerinin de sürüm taşıması:** sistemin gördüğü bir ekran yoktur; geçişin koşulu o anki hâldir.

*Kaynak: K-873, K-874 · devir: K-270, K-587 (eşzamanlılık mekanizması), K-800 (park — araya giren kaydın tespiti) · K-752 · `02 §3.31` · `03 §3.5` · `04` 2.10.*

### 7.4 Saklama ve imha

**7.4.1 Tek bir imha işi her gece Türkiye saatiyle 03:00'te çalışır (MK-22; §5.3.2)** ve süre envanterinin (Z-28…Z-44) bütün satırlarını sırayla işler. **Süre veriden hesaplanır:** iş "son çalışmadan beri" değil, her kaydın başlangıç değerinden — oluştuğu yılın sonu, kapatılış, gönderim anı (`02 §12.2.7`) — bugüne bakar; kaçırılan bir gece sonraki çalışmada kendiliğinden telafi edilir ve aynı gün iki kez çalışmak sonucu değiştirmez. Her kayıt türü küçük parçalar hâlinde, her parça kendi veritabanı işleminde imha edilir; yarıda kesilen çalışma sonrakinde kaldığı yerden sürer. İmha ile aynı siparişe dokunan bir yönetici işlemi çakışırsa sıra siparişin satır kilidiyle belirlenir (§7.3.2).

**7.4.2 İmha kaydı veritabanında ayrı, yalnız eklenen bir tablodadır** — tarih, kayıt türü, sayı; kişisel veri yoktur (Z-44). Panelde görünmez, yedekle birlikte korunur ve üç yılı dolan satırını aynı iş siler; okunma yolu §9.3'tedir (2. oturum). Ürün Gereksinimleri'nin imha kaydı için "sistem kaydı" dediği yer (`02 §10.3.3`, §12.2.7) bu tablodur — panelde görünmeyen, sistemin tuttuğu kayıt; §9.1'in sistem kayıtları — kayıt dosyaları, Z-40 (2. oturum) — değildir. Kişisel veriyi hemen silen işlemler — hesabın silinmesi, bekleyen kaydın silinmesi, IBAN'ın silinmesi — imha kaydını silmeyle aynı veritabanı işleminde yazar.

**7.4.3 İmhanın biçimi (MK-23):** (1) **Silme fiziksel silmedir:** hesap, iletişim talebi, giriş kaydı, bildirim kaydı ve IBAN veritabanından silinir; "silindi" işaretli satır bırakılmaz. (2) **Siparişte** kişisel alanlar — ad, adres, e-posta, telefon, IP — aynı satırda boşaltılır ve hesap bağı kaldırılır; kalan ticari kayıt `02 §12.2.7`'nin saydığı alanlardır. (3) **Siparişe ait üretilen belgeler** — Ön Bilgilendirme Formu ve Mesafeli Satış Sözleşmesi — kişisel veriyi siparişin alanlarından alır; belge siparişe ayrı bir kopya olarak saklanıyorsa imha o kopyadaki kişisel veriyi de kapsar. Belgenin kopya olarak mı saklandığı yoksa donmuş şablon sürümünden mi yeniden üretildiği Veri Modeli'nin (`06`) kararıdır ve bu kuralla birlikte verilir.

**7.4.4 İmha işinin kapsamına ayrıca girenler:** süresi dolan sistem kayıt dosyaları — imha kaydına kayıt türü ve dosya sayısıyla yazılır (§9.2 — 2. oturum) · süresi dolan işlem izi satırları — veritabanındaki imha fonksiyonuyla (§7.5) · hiçbir kalemin indiremeyeceği dijital ürün dosyası kayıtları (§7.7.3). Aynı gece geçici kayıtlar da temizlenir (§5.2.9).

**7.4.5 Yedeklerdeki kopyalar:** imha edilen veri yedeğin saklama süresi (§7.6.6) dolunca kendiliğinden kaybolur. Yedekten geri dönüş en son ana yapılır; o zaman imhalar işlem günlüğüyle birlikte yeniden uygulanmış olur. Daha eski bir ana dönüş — yalnız veri bozulmasından kurtarmak için — o andan sonraki anlık silmeleri geri getirir: dönüşten hemen sonra imha işi çalıştırılır ve kurulumu yapan firmayı bilgilendirir (`DEPLOY_RUNBOOK`).

**7.4.6 Elenenler.** **İmha kaydının uygulamanın kayıt dosyasına yazılması:** dosya döndürülürken ya da sunucu kaybolduğunda üç yıl dolmadan gider. **Son çalışma zamanına göre artımlı imha:** kaçırılan ya da yarıda kesilen çalışma kayıt atlatır. **Her kayıt türü için ayrı iş:** sıra ve saat yönetimi çoğalır.

*Kaynak: K-875, K-876 · devir: K-353, K-354, K-357, K-449, K-552, K-582 (saklama süreleri, imhanın işletimi ve biçimi), K-102 · K-708, K-879, K-883, K-916 · `02 §4.2`, §10.3.3, §12.2.7 · `03 §4.1`.*

### 7.5 İşlem izinin korunması

**7.5.1 İz tablosunda uygulamanın veritabanı kullanıcısı yalnız ekler ve okur; güncelleme ve silme yetkisi yoktur (MK-24).** Süresi dolan iz satırını (Z-30) yalnız veritabanında tanımlı bir imha fonksiyonu siler; fonksiyon süreyi kendisi hesaplar ve süresi dolmamış satıra dokunmaz — imha işi (§7.4) onu çağırır. Böylece iz `02 §8.5.2`'nin istediği gibi saklama süresi boyunca değiştirilemez ve silinemez — bir kod hatası da onu değiştiremez.

**7.5.2 Kurulum tarafının veritabanına doğrudan erişimi bu korumanın dışındadır;** erişimin sınırı §8.9'dadır (2. oturum). İzin yazılışı §4.7'dedir.

*Kaynak: K-877 · K-310, K-482, K-927 · `02 §8.5.2`, §10.3.*

### 7.6 Yedekleme ve kurtarma

**7.6.1 Veritabanının sunucu dışı yedeği iki parçadır (MK-25):** (1) **sürekli günlük aktarımı** — PostgreSQL'in işlem günlüğü (WAL) en geç **bir dakikada** bir sunucunun dışındaki nesne depolamaya taşınır; (2) **günlük tam yedek** — her gece, imha işinden (03:00) önce biter; saati `DEPLOY_RUNBOOK`'tadır. Geri dönüş saklama süresi içindeki herhangi bir ana yapılabilir (noktasal geri dönüş). Araç pgBackRest'tir (MK-09); gecelik zamanlaması yedek aracının kendi konteynerindedir. **Dosyalar** — görseller ve dijital ürün dosyaları — ve **sistem kayıt dosyaları** her gece aynı nesne depolamaya eşitlenir. Sunucunun içinde **yerel bir kopya** son tam yedeği ve günlükleri tutar; sunucu ayaktayken hızlı geri dönüş içindir, sunucu dışı yedeğin yerine geçmez.

**7.6.2 Sıfır kaybın karşılığı ve tek pencere.** Onaylanan her işlem diske yazılmadan başarı dönmez (§7.1.2) — uygulama, veritabanı ya da sunucu çökerse kayıp yoktur. **Sunucunun diski kalıcı olarak kaybolursa** kayıp penceresi en çok son bir dakikadır — yedeğe henüz ulaşmamış kayıtlar; bu pencere proje sahibinin kararıyla kabul edildi ve ikinci bir sunucuya eşzamanlı kopya eklenmez (§0.13). O pencerede verilmiş bir siparişte firmanın adımları kuralın evindedir (`02 §3.34.8`); kurtarmadan sonra kurtarılan son anın firmaya bildirilmesi `DEPLOY_RUNBOOK`'tadır. Kurtarmadan sonra eşleşmeyen sağlayıcı bildirimi gelen bildirim kaydında okunur (§4.5.5).

**7.6.3 Kapsam (MK-26):** veritabanı, yüklenen dosyalar, sistem kayıt dosyaları ve çatının şifreleme anahtar halkası — oturum çerezleri ve form güvenliği bu anahtarlarla korunur; anahtar halkası veritabanında tutulur ve onunla birlikte yedeklenir. Sırlar — kurulum ayarları — yedeğe girmez; yerleri ve kurtarmada yeniden kurulmaları §6.x'tedir (2. oturum).

**7.6.4 Yer:** S3 uyumlu bir nesne depolama, **sunucunun dışında ve firmanın hesabında** (ÖK-2); Türkiye'de olması önerilir (§0.4).

**7.6.5 Şifreleme:** yedek sunucudan çıkmadan şifrelenir. Anahtar kurulumda firmaya teslim edilir ve kurulumu yapanda da durur — anahtarsız yedek açılamaz; anahtarın teslimi `DEPLOY_RUNBOOK`'tadır, döndürülmemesinin sebebi §6.x'tedir (2. oturum).

**7.6.6 Saklama:** otuz gün; daha eski tam yedek ve günlükler kendiliğinden silinir. Nesne depolamada silmeye karşı koruma — sürüm tutma ya da nesne kilidi — açılır: sunucu ele geçirilse de yedek oradan silinemez.

**7.6.7 Kurtarma süresi — tasarım hedefi (MK-27):** sunucu kaybolursa kurulum yeni bir sunucuda aynı gün, birkaç saat içinde çalışır — yeni sunucu kiralanır, Compose tanımı kurulur, veritabanı yedekten en son ana döndürülür, dosyalar indirilir ve alan adı yeni sunucuya yönlendirilir. Hedef bir taahhüt değildir; ürün çalışma süresi taahhüdü vermez (§0.11).

**7.6.8 Kurtarma provası:** kurulumda bir kez — ÖK-2'nin "firmanın yedeğe erişimini doğrular" adımıyla birlikte — ve sonra her güncellemede (§8.6 — 2. oturum) yedek ayrı bir konteynerde geri döndürülür ve sonuç `DEPLOY_RUNBOOK`'a yazılır — denenmemiş yedek yedek sayılmaz (`00 §L.1`).

**7.6.9 Aktarımın hatası uyarı olur:** işlem günlüğü nesne depolamaya ulaşamazsa günlükler yerel diskte birikir ve bağlantı dönünce aktarım sürer; süren aktarım hatası ve eskiyen gecelik yedek durum ucunun uyarı koşuludur (§9.4 — 2. oturum).

**7.6.10 Elenenler.** **Yalnız gecelik yedek:** sunucunun diski kaybolursa bir günün siparişleri gider. **Sağlayıcının anlık görüntüsünü yedek saymak:** aynı sağlayıcıda, çoğu kez aynı hesapta durur, noktasal dönüş vermez ve firmanın elinde ayrı bir kopya değildir — güncelleme öncesi geri dönüş içindir (§0.5). **Yedeğin kurulumu yapanın hesabında durması:** abonelik bitince firma yedeğine ulaşamaz. **Şifresiz yedek:** nesne depolama hesabının ele geçirilmesi bütün kişisel veriyi açar. **Doksan gün ya da daha uzun saklama:** imha edilen verinin yedekte yaşadığı süreyi uzatır (§7.4.5); otuz gün fark edilmeyen bir veri bozulmasından dönmeye yeter.

*Kaynak: K-878, K-879, K-880, K-888 (proje sahibi) · devir: K-407, K-557, K-629 (yedekleme ve kurtarma), `10 §1`, §4.1, `02 §3.34.8`, `03` 10.2.4 · K-410, K-864, K-876, K-893, K-915, K-918 · `10 §4.1` ÖK-2 · `00 §L.1` · `04 §1.1` (matrisin "mimari" satırları — KP-75, `02 §3.34.8`, `03 §10.2.4`).*

### 7.7 Dosya depolama ve dijital teslim

**7.7.1 Dosyalar sunucunun diskinde, uygulamanın dosya alanında durur (MK-28)** — görseller ve dijital ürün dosyaları; yedekleri nesne depolamadadır (§7.6.3). Uygulama dosyalara bir **dosya deposu arayüzü** üzerinden erişir: ileride nesne depolamaya taşımak kod değişikliği değil yapılandırmadır (§11.3 — 2. oturum). **Görseller** herkese açıktır ve ters vekilden önbellek başlıklarıyla sunulur; adresleri içerikten türeyen bir parmak izi taşır — değişen görsel yeni adres alır. **Dijital ürün dosyası hiçbir zaman genel bir adresle sunulmaz:** indirme yalnız sipariş sayfasının erişimi denetlenerek uygulamadan geçer (7.7.4); dosyanın diskteki adı tahmin edilemez ve dosya alanı dışarıdan listelenemez. Disk dolarsa yükleme reddedilir ve sebebi söylenir; disk doluluğu durum ucunun uyarı koşuludur (§9.4 — 2. oturum).

**7.7.2 Yüklenen her görsel sunucuda yeniden kodlanır (MK-29):** en uzun kenarı 2560 pikseli aşmayacak biçimde küçültülmüş bir kaynak kopya saklanır ve vitrinin kullandığı genişliklerde WebP türevleri üretilir; yüklenen özgün dosya saklanmaz. Yeniden kodlama dosyanın meta verisini — çekim yeri (GPS) ve cihaz bilgisi dahil — atar. Türevler yüklemeden sonra, isteği bekletmeden iş kuyruğunda üretilir (§5.2.10); türevi henüz olmayan görselin yerine kaynak kopya sunulur. Görüntü işleme kütüphanesinin seçimi Kodlama Kılavuzu'nun (`09`) işidir (§2.2.3); dosyanın içeriğinin doğrulanması §6.6'dadır (2. oturum).

**7.7.3 Dijital ürün dosyası (MK-30):** **yükleme parçalıdır** — panel dosyayı küçük parçalar hâlinde gönderir, kesilen yükleme kaldığı yerden sürer; parçalar tamamlanmadan dosya varyanta bağlanmaz ve tamamlanmayan yükleme bir gün sonra gecelik temizlikte silinir (§5.2.9). **Her yükleme yeni bir dosya kaydıdır;** varyantın "o anki dosyası" ona en son bağlanan kayıttır ve kalem indirmede varyantın o anki dosyasını verir (`02 §3.12.7`). Ürün kalıcı silinirse ya da dosya kaldırılıp yerine yenisi konmazsa varyantın son dosya kaydı, ona bağlı bir kalem indirilebildiği sürece silinmez (`02 §3.12.8`). **Dosyanın silinmesi:** hiçbir varyanta bağlı olmayan ve sipariş sayfası açılabilen — kişisel verisi imha edilmemiş — hiçbir kalemin indiremeyeceği dosya kaydı imha işinde silinir (§7.4.4).

**7.7.4 İndirmenin sayıldığı an (MK-31):** hak, müşteri sipariş sayfasındaki indirme düğmesine bastığında ve sunucu dosyayı göndermeye başladığında bir kez düşer (`02 §3.12.5`). Düğme o kaleme bağlı, kısa ömürlü — bir saat — bir indirme bağlantısı üretir; bu bağlantıyla yapılan devam istekleri — kesilen indirmenin kaldığı yerden sürdürülmesi (HTTP aralık istekleri) — yeni hak düşürmez. Bağlantının ömrü dolduktan sonra yeni bir indirme yeni bir haktır. Hakkı dolmuş kalemde düğme bağlantı üretmez. İndirme bağlantısı e-postayla gitmez (§6.4 — 2. oturum).

**7.7.5 Elenenler.** **Dosyaların baştan nesne depolamada durması:** indirme başına çıkış trafiği ücreti tavanı zorlar (§0.2) ve her istek ikinci bir servise bağımlı olur; sanal sunucunun trafiği kiraya dahildir. **Dosyaların veritabanında durması:** veritabanını ve sürekli yedeğini yüzlerce MB'lık nesnelerle şişirir, geri dönüş süresi uzar. **Dijital dosyaya süreli imzalı genel adres:** adres paylaşılırsa süre içinde sayaçsız indirilir ve P-8'in adet sınırı delinir.

*Kaynak: K-881, K-882, K-883, K-884 · devir: K-146 (indirmenin sayıldığı an), K-143 (dosyanın teslim yolu) · K-92, K-148, K-461, K-875, K-905 · `02 §3.11`, §3.12.*

### 7.8 Önerilen stok kodu

**7.8.1 Önerilen kod ürünün adresinden ve varyantın sırasından üretilir (MK-32):** ürün adresinin (§3.7.1 — Türkçe harfler sadeleşmiş) büyük harfli ilk yirmi karakteri, tire ve iki haneli sıra — `AHSAP-MASA-01`, `AHSAP-MASA-02`. Seçeneği olmayan ürünün tek varyantı `-01` alır. Önerilen kod başka bir varyantınkiyle çakışırsa sıra bir artırılır. Öneri yalnız yeni varyantın formunda gelir; ürünün adı sonradan değişse de var olan kodlar değişmez. Yöneticinin yazdığı kod yazıldığı gibi saklanır ve tekilliği `02 §3.3.4`'ün kuralıdır.

*Kaynak: K-885 · devir: K-801 (park — önerilen kodun biçimi ve üretimi) · `02 §3.3.4`, §3.30.2 · `04` 9.5.5, 9.5.23.*

### 7.9 Site içi arama

**7.9.1 Arama veritabanında yapılır; ayrı bir arama motoru yoktur (MK-33).** Ürün adının arama biçimi — küçük harfe çevrilmiş ve Türkçe harfleri sadeleşmiş hâli (`ı` ile `i`, `ş` ile `s`, `ğ` ile `g`, `ç` ile `c`, `ö` ile `o`, `ü` ile `u` aynı) — ürünle birlikte ayrı bir alanda tutulur ve ad değiştikçe yenilenir; aranan ifade aynı biçime çevrilir ve alanda **içerir** eşleşmesiyle aranır (trigram dizini). Sadeleştirme sayfa adresinin üretimindeki işlevdir (§3.7.1). Kuralın kendisi `02 §3.5.1`'dedir; sonuçlar `02 §3.5.3`'ün düzeninde sayfalanır.

**7.9.2 Elenenler.** **Ayrı arama motoru (Elasticsearch, Meilisearch):** sunucuda ayrı bir bileşen ve bellek (§3.2.8). **Veritabanının Türkçe tam metin araması:** kök bulma Türkçede beklenmeyen eşleşmeler üretir ve yarım yazılan sözcüğü bulmaz; `02` adın içinde arama ister, dil çözümlemesi istemez.

*Kaynak: K-886 · K-47 · `02 §3.5`, §3.30.2 · `02 §11.3` H-1.*

### 7.10 Önbellek

**7.10.1 Ayrı bir önbellek sunucusu yoktur; önbellek uygulamanın belleğindedir (MK-34).** Vitrinin sık okunan parçaları — menü, kategori ağacı, ayarlar, ana sayfa blokları — ve site haritası (§3.7.3) bellekte tutulur ve ilgili kayıt panelden kaydedildiğinde hemen geçersiz kılınır; fiyat, stok, sepet ve satış kapısının hâli her istekte veritabanından okunur. Sayfanın tamamı önbelleğe alınmaz. Tek örnek çalıştığı için (§3.1.2) geçersiz kılma tek süreçtedir; ikinci örnekte paylaşılması gerekir (§11.3 — 2. oturum). Geçersiz kılmanın kodda kalıbı Kodlama Kılavuzu'nun işidir (`09` park satırı).

**7.10.2 Elenenler.** **Ayrı önbellek sunucusu (Redis gibi):** ayrı bileşen, bellek ve sır yüzeyi; tek örnekte kazancı yoktur. **Sayfaların tamamının önbellekte tutulması:** fiyat, stok, "Tükendi" ve satışın kapalı hâli sayfada değişir; bayat sayfa ziyaretçiye yanlış bilgi gösterir. **Önbelleğin hiç olmaması:** menü ve ayarlar her sayfada okunur; küçük sunucuda (§0.5) gereksiz yük.

*Kaynak: K-887 · K-871, K-933 · `02 §3.16`.*

---

## 8. Ortamlar ve deployment

> **Ne yazılır:** Ortam yapısı, konteynerleştirme, dal stratejisi, CI/CD hattı, konfigürasyon yönetimi.
> **Not:** Dal stratejisi burada `.claude/INSTRUCTIONS.md §3.2` ile **birebir aynı** yazılmalıdır; iki farklı yerde iki farklı strateji en sık görülen çelişkidir.

## 9. Gözlemlenebilirlik

> **Ne yazılır:** Loglama, metrik, izleme, uyarı. Maliyet kısıtı varsa hangi araçların seçildiği ve neden.

## 10. Hata toleransı

> **Ne yazılır:** Her dış bağımlılık için "çökerse ne olur", retry stratejisi, devre kesici, geri düşme davranışı.

## 11. Ölçeklenebilirlik notları

> **Ne yazılır:** MVP'de bilinçli olarak tekil bırakılan bileşenler ve büyüme yolu. "Büyüdüğünde X'e geçilir" ifadeleri gerçekçi olmalı.

## 12. Kararların özeti

| # | Karar | Tarih | Gerekçe | Geri alınabilir mi |
|---|---|---|---|---|

---

*Shopfolio — Technical Architecture v0.2 (v0.2 yazım turunun 1. oturumudur — başlık notu, §0, §2, §3, §7, §4, §5; K-930…K-934 · v0.1 şablondur — açılış ve workshop karar kaydının §12'sindedir)*
