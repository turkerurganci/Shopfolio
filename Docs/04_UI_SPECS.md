# Shopfolio — UI Specifications

**Versiyon: v0.11** | **Bağımlılıklar:** `02_PRODUCT_REQUIREMENTS.md`, `03_USER_FLOWS.md`, `10_MVP_SCOPE.md` | **Son güncelleme:** 2026-10-04

> **Aşama:** 3 — UI/UX Tasarım · **Rol:** Senior Product Designer / UX Architect
> **Traceability zorunlu: EVET** — §1 tamamlanmadan §3'e (ekran envanteri) geçilmez.
> **Düzey:** Wireframe — bilgi mimarisi ve etkileşim odaklı, pixel-perfect değil.

> **Aşama 1'den park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36.
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-06 · K-97 · Aktör envanteri:** Ekran × rol matrisi dört aktör üzerinden kurulur — ziyaretçi · misafir alıcı · üye müşteri · firma yöneticisi (`02 §1.3`). Yönetim tarafı **tek roldür** (çoklu kullanıcı, aynı yetki). Misafir alıcıyı K-97 ekledi ve K-06'nın *"üyeliksiz sipariş kararına bağlıdır"* diye açık bıraktığı envanteri kapattı; satır Aşama 2'nin çakışma taramasında hizalandı (2026-10-04).
> - **K-14 · K-574 · Firma kimlik bilgileri:** Firma tipine göre değişen zorunlu kimlik seti ve — firma ETBİS doğrulama bilgisini girdiyse — doğrulama bandı sitede **sürekli erişilebilir** olur; alan boşsa band görünmez (K-574). **Tam yerleşim bu dokümanın kararıdır** — hangi bilgi hangi ekranda ve hangi alanda görünecek. *Workshop'ta karara bağlandı (2026-10-04): K-747 — tam set her vitrin sayfasının altbilgisinde ve İletişim sayfasında, band bloğun yanında; yazımı §2 ve §3'tedir.*
> - **K-27 · Ana sayfa kompozisyonu:** İki hazır düzen tasarlanır — **tanıtım öncelikli** ve **mağaza öncelikli**. Taban kural: her iki düzende de kurumsal tanıtım ile ürün vitrini ana sayfada **birlikte** bulunur; değişen yalnız ağırlık ve sıradır. Serbest sayfa kurgusu (page builder) kapsam dışıdır. *Workshop'ta karara bağlandı (2026-10-04): K-753 (blokların sırası), K-754 (ürün vitrininin içeriği), K-755 (kayıt sayıları); yazımı §5'tedir.*

> **Aşama 2'den park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §8.3 AK0-04).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **Dizin — Aşama 2 kararlarının bu dokümana devirleri:** karar kaydında K-647…K-718'in etki sütunlarında bu dokümanı gösteren otuz beş atıf (otuz beş karar) ve Kullanıcı Akışları'nın (`03`) gövdesinde bu dokümana iş bırakan cümleler [`CHECKPOINT_REPORTS/PHASE2_CONFLICT_SCAN.md`](CHECKPOINT_REPORTS/PHASE2_CONFLICT_SCAN.md) §6'dadır; etki sütununda yalnız doküman numarası taşıyan sekiz atfın işinin adını dizin verir. Bir iş yalnız `03`'ün gövdesinde yaşar, karar satırı yoktur: yeniden gönderilebilen e-postaların kapsamı ve firma bildirimlerinin "e-posta ulaşmadı" işaretinin biçimi (`03 §7.1.37`, §8.9.2, §10.1.2.2) — *workshop'ta karara bağlandı (2026-10-04): K-759, K-760, K-761; yazımı §2 ve §9'dadır*. Devir taraması (`checklists/document-stage.md` §4) buradan başlar; devrin evi karar satırıdır, dizin onu ikinci kez kaydetmez. Aşama 1'in dizini: `PHASE1_CONFLICT_SCAN.md` §6.

> **Yazım durumu (K-28, K-30, K-432; karar kaydı §10.1):** doküman UI/UX tasarım aşamasının **tek yazım turunda**, beş oturumda yazılır. Girdi karar kaydı, bu dokümanın şablonu ve üst dokümanlardır — workshop'un sohbet geçmişi değil.
> - **1. oturum (2026-10-04, v0.6):** yazım konvansiyonları ve alt bölüm haritası (blok UI0; K-762, K-763) · §2 ortak bileşen kütüphanesi — tasarım tabanı ve yirmi bileşen (blok UI2) · §3 navigasyon haritası (blok UI3) · §4 ekran envanteri — 53 ekran, bir düşen kimlik (blok UI4; K-764). Yazımın bulduğu beş ekran kurgusu boşluğu karara bağlandı (K-765…K-769); üst dokümana dönen karar yoktur. §1 aynı PR'da envanterle hizalandı: E-48'in sekiz satırı E-44'e taşındı, altı satır E-38'i aldı (§1.1.2, §1.2).
> - **2. oturum** — iki parçaya bölündü (K-778). **2a (2026-10-04, v0.7):** §5.1…§5.15 — müşteri tarafının ilk on beş ekranı (E-01…E-15: vitrin, içerik, sepet, ödeme adımı, sipariş teyit ekranı, sipariş takibi girişi; blok UI5); 1a'nın 119 matris satırı tam metinle okundu ve on üç satır düzeldi (§1.2); yazımın bulduğu dokuz boşluk karara bağlandı (K-770…K-778), biri Ürün Gereksinimleri'ne ve MVP Kapsamı'na geri beslendi (K-774; §1.3). **2b (2026-10-04, v0.8):** §5.16…§5.28 — müşteri tarafının kalan on üç ekranı (E-16…E-28: sipariş sayfası, iptal ve gecikme feshi, cayma beyanı, ayıp talebi, üyelik ve hesap ekranları); §5 tamamlandı. 1a'nın 378 matris satırının tamamı yazım turunda tam metinle okundu; bu oturumda yirmi beş satır düzeldi (§1.2). Yazımın bulduğu sekiz boşluk karara bağlandı (K-779…K-786), biri Ürün Gereksinimleri'ne geri beslendi (K-786; §1.3). **Kısmi adet kararı (2026-10-04, v0.9):** proje sahibinin kararıyla iptal, cayma ve iadede müşteri adedi birden büyük kalemin kaç adedini işleme sokacağını seçer (K-787); açtığı detaylar K-788…K-795'tir. Ürün Gereksinimleri v0.60'a, Kullanıcı Akışları v0.16'ya, MVP Kapsamı v0.41'e ve Proje Vizyonu v0.33'e geri beslendi (§1.3). Bu dokümanda: adet alanı OB-12'nin varyantıdır (2.12.1.8 — yeni bileşen açılmadı) · E-16, E-17, E-18 ve E-19'un tanımları (5.16.5, 5.17.4–5.17.6, 5.17.8, 5.17.12, 5.18.4, 5.18.6, 5.18.7, 5.18.9, 5.18.12, 5.19.3, 5.19.4, 5.19.9) · §4'ün E-17, E-18, E-19 satırları. §3'ün geçişleri değişmedi; panelin adet yüzü (E-37) 3. oturumundur. Matrisin satır sayısı değişmedi (1.193). **3. oturum (2026-10-04, v0.10):** §9.1…§9.9 — panelin ilk dokuz ekranı (E-29…E-37: panel girişi ve şifre sıfırlama, davet kabulü, panel ana sayfası, ürün listesi ve ürün formu, kategori ağacı, kuponlar, sipariş listesi ve sipariş ayrıntısı; blok UI9); dokuz ekranın Kullanıcı Akışları'ndaki 364 kaynak satırı tam metinle okundu ve on bir matris satırı düzeldi (§1.2); yazımın bulduğu on iki boşluk karara bağlandı (K-797…K-808), üçü Ürün Gereksinimleri'ne ve Kullanıcı Akışları'na geri beslendi (K-805, K-806, K-808; §1.3). §3.4'e üç geçiş girdi (3.4.18…3.4.20); §2'nin "Kullanıldığı ekranlar" listeleri ve iki geçici hedefi madde numarasına çevrildi. **4. oturum (2026-10-04, v0.11):** §9.10…§9.25 — panelin kalan on altı ekranı (E-38…E-47, E-49…E-54: talepler ve iletişim talebi ayrıntısı, üye kaydı görünümü, kurumsal içerik, ana sayfa ve menü, marka, dört ayar ekranı, yönetici hesapları ve yöneticinin kendi hesabı, satış özeti, işlem izi, dışa aktarma, yönetici hesabının doğrulama ekranları; blok UI9); **§9 tamamlandı.** On altı ekranın 225 kaynak satırı okundu — Kullanıcı Akışları'nın ve MVP Kapsamı'nın satırları tam metinle — ve on sekiz matris satırı düzeldi (§1.2). 3. oturumun bir değiştirme betiğinin bozduğu altı satır — §2'nin beşi, §5.1'in Kaynak satırı — önceki sürümden ve 3. oturumun kaydındaki niyetten onarıldı. Yazımın bulduğu on beş boşluk karara bağlandı (K-809…K-823), üçü Ürün Gereksinimleri'ne geri beslendi (K-813, K-818, K-821; §1.3). §3.4'e iki geçiş girdi (3.4.21, 3.4.22); §2'nin "Kullanıldığı ekranlar" listeleri ve 9.2.5'in geçici hedefi güncellendi. **5. oturum:** §6, §7, §8 ve çapraz denetim (bloklar UI6, UI7, UI8). Kalite döngüsü yazım turu bittikten sonra başlar.

**Yazım konvansiyonları (K-726, K-762).** Bu on beş madde §2–§9'un bütün yazım oturumlarını bağlar; "konvansiyon n" diye anılır.

1. **Bu doküman kural koymaz.** Ürün Gereksinimleri'nin (`02`) kurallarını ve Kullanıcı Akışları'nın (`03`) adımlarını ekranlara çevirir; `02`, `03` ya da `10 §2`'nin saymadığı bir yetenek ekrana girmez. Ekran yazılırken doğan ihtiyaç bir **boşluktur:** uydurulmaz, karar kaydında bir K satırıyla karara bağlanır; ürün kuralı doğuruyorsa aynı PR'da üst dokümana yazılır ve §1.3'ün geri besleme tablosuna satır girer (K-652, K-726). Ekranın kendi kurgusu — yer, sıra, gruplama, geçiş — bu dokümanın kararıdır ve üst dokümana dönmez. `02`'nin bir kuralın "biçimini, metnini ya da yerini `04`'e bıraktığı" her cümle burada karşılık bulur (`02 §3` girişi).
2. **Ekran kimliği `E-nn`'dir** (K-725): §4'ün envanterinde verilir; §1, §3, §5, §6, §7 ve §9 aynı kimliği kullanır. Kimlik yeniden numaralanmaz — matrisin satırları ona bağlıdır; düşen kimlik §4.3'te "düştü → nereye" diye kalır ve yeniden verilmez. Ekranın adı §4'teki addır ve metinde tırnaksız yazılır. Bir kimlik, aynı amacı paylaşan kısa bir ekran dizisini kapsayabilir (E-23, E-29, E-54); dizinin adımları ekran tanımının "Bilgi hiyerarşisi" alanında ayrı ayrı yazılır.
3. **Ekran tanımının başlığı ve yeri.** Müşteri tarafının ekranları §5'te, panelin ekranları §9'da, aynı şablonla yazılır (K-724). Başlık `### 5.n E-nn — Ad` ya da `### 9.n E-nn — Ad` biçimindedir; `n` aşağıdaki alt bölüm haritasında sabittir. Şablonun `S<NN>` kimliği kullanılmaz.
4. **Ekran tanımının sekiz alanı** şablonun sırasıyla ve şu biçimde doldurulur:
   - **Aktör · Giriş noktası · Çıkış noktaları** — aktör konvansiyon 7'nin adlarından; giriş ve çıkış §3'ün geçiş satırlarına atıfla yazılır (`§3.3.11`), geçiş yeniden anlatılmaz.
   - **Kullanıcı buraya geldiğinde ilk ne görmeli** — tek cümle: ekranın birincil bilgisi ve birincil eylemi. Ekran başına tek birincil düğme vardır (§2.1.3).
   - **Bilgi hiyerarşisi** — yukarıdan aşağıya numaralı bölgeler; vitrinde dar sınıfın sırasıyla, panelde geniş sınıfın düzeniyle yazılır (§2.1.2). Çerçeve (§2.2) yeniden yazılmaz; yalnız ekrana özgü farkı yazılır.
   - **Aksiyonlar** — her aksiyon bir madde: ne yapılır · hangi akış adımı (`03 §n.m.k`) · açık olduğu koşul (`03 §1.11.k`) · onay isteyip istemediği (OB-04) · sonucun hangi mesaj yerinde söylendiği (OB-03).
   - **Validasyonlar** — alan · kuralın kimliği (`02 §…`, P-, L-) · mesajın yeri; alan envanterinin tamamı §7'dedir.
   - **Durum × rol varyantları** — ekranın durumla ve aktörle değişen parçaları; tam matris §6'dadır ve durum adları konvansiyon 8'e uyar.
   - **Boş / yükleniyor / hata durumları** — kalıp OB-07'dir; burada yalnız ekrana özgü boş hâl ve ekrana özgü hata yazılır.
   - **Responsive notları** — yalnız fark: **dar · orta · geniş** (§2.1.1); bir sınıfta gizlenen içerik ya da işlev yoktur (§2.1.2).
   Her ekran tanımı bir *Kaynak* satırıyla kapanır (konvansiyon 11).
5. **Satır ve alt madde kimliği bölüm numaralıdır** (Kullanıcı Akışları'nın kalıbı — K-669): bir alt bölümün kuralı ya da tablo satırı `n.m.k` diye numaralanır ve dışarıdan `04 §n.m.k` diye anılır — §3.3'ün on birinci geçişi `04 §3.3.11`, E-13'ün tanımındaki yedinci madde `04 §5.13.7`. Ekran tanımında maddeler alan fark etmeksizin ekran içinde artan tek bir sırayla numaralanır. §1'in matris satırları kaynağın kimliğini, §4'ün envanter satırları ekran kimliğini taşır; onlara ayrıca numara verilmez.
6. **Ortak bileşenin kimliği `OB-nn`'dir** ("OB" = ortak bileşen; K-762): §2'nin tablosunda verilir ve bileşenin kuralları kendi alt bölümünde `2.n.k` diye numaralanır. Ekran tanımı bileşene `OB-04 onay` ya da `§2.4.3` diye atıf yapar ve bileşenin kuralını **yinelemez** — yalnız hangi varyantı kullandığını ve ekrana özgü farkını yazar. Matrisin "ortak bileşen: *ad*" değerleri (§1.1.2) §2'nin tablosunun "Matris adı" sütunuyla bileşen kimliğine bağlanır; matrisin hücreleri değişmez.
7. **Aktör adları `02 §1.3` ile birebirdir:** ziyaretçi · misafir alıcı · üye müşteri · firma yöneticisi. Tablolarda Kullanıcı Akışları'nın kısa adları kullanılır (`03 §0.3.2`; K-700, K-702): **Ziyaretçi** · **Misafir alıcı** · **Üye** · **Müşteri** (misafir alıcı ya da üye) · **Yönetici** · **Kullanıcı** (hesabı henüz açılmamış ya da hesabına giremeyen kişi — doğrulama, sıfırlama, davet ve geri alma bağlantısını açan) · **Sistem**. "Firma" satıcının kendisidir; ekran aktörü değildir. Yönetim tarafı tek roldür (`02 §1.3`); panel ekranlarında rol varyantı yoktur.
8. **Durum, terim ve işaret adları `02` ile birebirdir** (`02 §1.2`, §5; `03 §1`; K-17) ve şablondan değil kaynaktan alınır; eş anlamlı kullanılmaz, kod karşılığı yazılmaz. Sevkiyat ekseni — Sipariş durumu: Alındı · Hazırlanıyor · Kargoya verildi · Teslim edildi · Teslim edilemedi · İptal edildi (`02 §5.4`). Ödeme ekseni — Ödeme durumu: Bekliyor · Ödendi · Başarısız · Kısmen geri ödendi · Geri ödendi (`02 §5.5`). Yayın durumu: Taslak · Yayında · Arşiv — kurumsal içerikte yalnız Taslak ve Yayında (`02 §5.1`, §5.2). Ayıp talebi: Açık · Çözüldü (`02 §5.10`). İletişim talebi: Açık · Kapatıldı (`02 §5.13`). İki eksen birlikte `Alındı + Bekliyor` diye, geçiş kimliğiyle (`S5`, `Ö1`), kalem kaydı adıyla yazılır (`03 §0.3.5`). Panel işaretleri `02`'deki tırnaklı biçimiyle yazılır: "e-posta ulaşmadı" (`03 §1.7.3`).
9. **Arayüz metni.** Tırnak içindeki metin kaynağın ya da kararın verdiği metindir ve birebir yazılır ("Siparişi onayla — ödeme yükümlülüğü doğar", "Panele dön"). Kaynağın metnini vermediği öğe tırnaksız, işleviyle anlatılır (güncel hâli gösteren düğme). Bu doküman kaynakta olmayan bir metni tırnak içinde yazmaz; `02`'nin nihai metnini bu dokümana bıraktığı öğeler adıyla sayılır ve ekranının yazımında karara bağlanır.
10. **`02`, `03` ve `10`'un kimlikleri olduğu gibi kullanılır** (K-532; `03 §0.3.3`): S1…S11, Ö1…Ö5, B-1…B-16, F-1…F-6, Z-, L-, P-, H-, KP-, KD-, ÖK-, SK-. **Değer yazılmaz, kimlik yazılır** (`03 §0.5.3`): süre, limit, tutar ve parametre değerleri `02 §4`, §8.2 ve §11'de yaşar. İstisna bu dokümanın kendi tasarım sabitleridir — genişlik sınıflarının sınırları, ana sayfa bloklarının kayıt sayıları, liste sayfasının boyu —; evleri burasıdır ve değerleriyle yazılır.
11. **Atıf ve Kaynak.** Önsüz `§` bu dokümanı gösterir; başka dokümana atıf numarasıyla yazılır (`02 §7.3.4`, `03 §2.4.8`, `10 §2` KP-12). Bir hücrede ya da cümlede `0X §` ile başlayan dizideki sonraki önsüz `§`'ler o dokümanı gösterir; dizi ` · `, `;`, parantezin kapanışı ya da cümle sonuyla biter (K-701). Her alt bölüm ve her ekran tanımı bir *Kaynak* satırıyla kapanır; tablo satırı Kaynak sütunu taşır. Kaynak önce dokümandır: Aşama 1 ve 2'nin kararları `02`, `03` ve `10` atfıyla gösterilir; **Aşama 3'ün kararları** (K-723 ve sonrası) K numarasıyla, üst dokümana döndüğü yerde `02 §…` ile birlikte yazılır. Devir dizinlerinin bu dokümana bıraktığı işi kapatan alt bölüm, devreden kararı Kaynak satırında "devir:" diye anar.
12. **Kural ayrıntısı evinde kalır.** Ekran ve bileşen bir kurala dayanıyorsa kuralın koşullarını kopyalamaz; ne gösterdiğini bir cümleyle yazar ve kurala bölüm numarasıyla işaret eder (`checklists/document-stage.md` §4).
13. **Geçici hedef hücre.** Yazılmamış bir bölüme bağlanan hücre hedef alt bölümü ve oturumunu taşır — "§5.25 (2. oturum)" —; o bölümü yazan oturum hücreyi aynı PR'da madde numarasına çevirir (K-682'nin kalıbı). Alt bölüm haritasını değiştirmek zorunda kalan oturum tabloyu ve ona atıf yapan satırları aynı PR'da günceller (K-670'in kalıbı).
14. **"Yoktur" ve belirsiz ifade.** Ürünün bilinçli olarak sunmadığı öğe ilgili ekranda tek cümleyle ve `02` ya da `10 §3` atfıyla "yoktur" diye anılır; boş bırakılmaz. "Muhtemelen", "belki", "gereksinime göre" yazılmaz (`GUARDRAILS §5`).
15. **Düzey ve dışarıda kalanlar.** Düzey wireframe'dir: bilgi mimarisi, yerleşim ve etkileşim. Renk değeri, yazı tipi, piksel ölçüsü, teknoloji, API ve veri modeli yazılmaz (`GUARDRAILS §1`); "ortak bileşen" tekrar eden bir arayüz kalıbının tanımıdır, bir yazılım paketi değildir. E-postanın metni ve şablonu Entegrasyon Spesifikasyonu'nun (`08`) işidir; burada yalnız e-postanın ekranda bıraktığı iz yazılır. Bu dokümanın şablonunda açık kararlar bölümü yoktur: açık kalan **detay** karar kaydının §4'ünde A-19'dan sürer; sonraki aşamaya bırakılan talimat hedef dokümanın "Aşama 3'ten park edilen girdiler" bloğuna yazılır (K-36, K-37).

**Alt bölüm haritası (K-763).** Şablonun §1…§9 numaraları değişmez; alt düzey aşağıdaki tabloyla sabittir. §1'in haritası §1'in başındadır (K-728). Ekran tanımlarının alt bölüm numarası kimlik sırasını izler: §5.n = E-n (n = 1…28); §9.n = E-(n+28) (n = 1…19), E-48 düştüğü için ardından §9.20 = E-49 … §9.25 = E-54.

| Bölüm | Alt bölümler | Plan konuları | Oturum |
|---|---|---|---|
| Başlık notu | Yazım durumu · yazım konvansiyonları (1–15) · alt bölüm haritası | UI0-01…UI0-05 | 1 ✓ |
| §2 Ortak bileşen kütüphanesi | 2.1 Tasarım tabanı · 2.2 Çerçeve · 2.3 Mesaj · 2.4 Onay · 2.5 Rozet ve işaretler · 2.6 Sayaç ve süre göstergeleri · 2.7 Boş, yükleniyor ve hata hâlleri · 2.8 Satış kapalı hâli · 2.9 "Taslak" bandı ve yönetici şeridi · 2.10 Eşzamanlı düzenleme uyarısı · 2.11 Ürün kartı · 2.12 Form bileşenleri (2.12.1–2.12.8) · 2.13 Liste ve tablo | UI2-01…UI2-08 | 1 ✓ |
| §3 Navigasyon haritası | 3.1 Vitrinin gezinme iskeleti · 3.2 Panelin gezinme iskeleti · 3.3 Müşteri tarafının geçişleri · 3.4 Panelin geçişleri · 3.5 Sitenin dışından girişler · 3.6 Girişten ve çıkıştan dönüş · 3.7 Panel ile vitrin arasındaki geçişler | UI3-01…UI3-05 | 1 ✓ |
| §4 Ekran envanteri | 4.1 Müşteri tarafının ekranları · 4.2 Panelin ekranları · 4.3 Aday listeden envantere farklar ve düşen kimlik · 4.4 Ekran olmayan yüzeyler | UI4-01…UI4-03 | 1 ✓ |
| §5 Ekran tanımları — müşteri tarafı | 5.1 E-01 Ana sayfa · 5.2 E-02 Kategori sayfası · 5.3 E-03 Arama sonuçları · 5.4 E-04 Ürün sayfası · 5.5 E-05 Arşivlenmiş ürün sayfası · 5.6 E-06 "Sayfa bulunamadı" sayfası · 5.7 E-07 İçerik liste sayfası · 5.8 E-08 İçerik sayfası · 5.9 E-09 Sık sorulan sorular sayfası · 5.10 E-10 İletişim sayfası ve formu · 5.11 E-11 Yasal metin sayfası · 5.12 E-12 Sepet · 5.13 E-13 Ödeme adımı · 5.14 E-14 Sipariş teyit ve bekleme ekranı · 5.15 E-15 Sipariş takibi girişi | UI5-01…UI5-07 | 2a ✓ |
| | 5.16 E-16 Sipariş sayfası · 5.17 E-17 İptal ve gecikme feshi ekranı · 5.18 E-18 Cayma beyanı ekranı · 5.19 E-19 Ayıp talebi ekranı · 5.20 E-20 Kayıt ekranı · 5.21 E-21 Doğrulama bağlantısının iniş ekranı · 5.22 E-22 Müşteri girişi · 5.23 E-23 Şifre sıfırlama ekranları · 5.24 E-24 Yeniden doğrulama ekranı · 5.25 E-25 Hesap — profil ve güvenlik · 5.26 E-26 Adres defteri · 5.27 E-27 Sipariş geçmişi · 5.28 E-28 E-posta değişikliğini geri alma ekranı | UI5-07…UI5-10 | 2b ✓ |
| §6 Durum × Rol matrisi | 6.1 Kapsam ve okunuş — durum taşıyan ekranlar · 6.2 Müşteri tarafı · 6.3 Panel | UI6-01…UI6-03 | 5 |
| §7 Form ve validasyon envanteri | 7.1 Müşteri tarafının formları · 7.2 Panelin formları · 7.3 Ortak alan kuralları | UI7-01…UI7-03 | 5 |
| §8 Lokalizasyon etkileri | 8.1 Tek dil ve dil seçiminin yokluğu · 8.2 Türkçenin biçimleri — tarih, sayı, para · 8.3 Metin uzunluğunun yerleşime etkisi | UI8-01 | 5 |
| §9 Yönetim (admin) ekranları | 9.1 E-29 Panel girişi ve şifre sıfırlama · 9.2 E-30 Yönetici daveti kabul ekranı · 9.3 E-31 Panel ana sayfası · 9.4 E-32 Ürün listesi · 9.5 E-33 Ürün formu · 9.6 E-34 Kategori ağacı · 9.7 E-35 Kuponlar · 9.8 E-36 Sipariş listesi · 9.9 E-37 Sipariş ayrıntısı | UI9-01…UI9-06, UI9-12 | 3 ✓ |
| | 9.10 E-38 Talepler · 9.11 E-39 İletişim talebi ayrıntısı · 9.12 E-40 Üye kaydı görünümü · 9.13 E-41 Kurumsal içerik · 9.14 E-42 Ana sayfa ve menü · 9.15 E-43 Marka · 9.16 E-44 Firma kimliği ve satış · 9.17 E-45 Ödeme yöntemleri · 9.18 E-46 Kargo, süreler ve varsayılanlar · 9.19 E-47 Yasal metinler · 9.20 E-49 Yönetici hesapları · 9.21 E-50 Yöneticinin kendi hesabı · 9.22 E-51 Satış özeti · 9.23 E-52 İşlem izi · 9.24 E-53 Dışa aktarma · 9.25 E-54 Yönetici hesabının doğrulama ekranları | UI9-07…UI9-12 | 4 ✓ |

Yazım sırası §2 → §3 → §4 → §5 → §9 → §6 → §7 → §8'dir (K-724). §6, §7 ve §8'in alt bölümleri plandır; 5. oturum iki tarafın ekran tanımları bittikten sonra kurar ve gerekirse bu tabloyu aynı PR'da günceller.

---

## 1. Traceability Matrix (ÖNCE BU)

> **Ne yazılır:** `02` gereksinimleri + `03` akış adımları → ekran eşlemesi. İleri ve geri izlenebilirlik.
> Eşlenmeyen kaynak madde = **GAP**. GAP'ler proje sahibine sunulur, karar alınır, **sonra** ekran tanımlarına geçilir.

**Alt bölüm haritası (K-728).** Matris üç oturumda kuruldu: **1a** (2026-10-04) envanteri, aday ekran listesini ve müşteri tarafının ileri izlenebilirliğini yazdı; **1b** (2026-10-04) firma tarafını ve kalanı yazdı — ileri izlenebilirlik 1.193 satırla tamamdır; **2** (2026-10-04) geri izlenebilirliği, gerekçesiz ekleme listesini ve GAP kararlarını yazdı — **matris tamamdır** (K-738, K-739). Şablonun §1.1, §1.2 ve §1.3 numaraları değişmez; alt düzey aşağıdaki tabloyla sabittir. Bir oturum bir alt bölümü bölmek ya da birleştirmek zorunda kalırsa bu tabloyu ve ona atıf yapan satırları aynı PR'da günceller (K-670'in kalıbı).

| Alt bölüm | İçerik | Oturum | Durum |
|---|---|---|---|
| §1.1.1 | Kaynak envanteri ve sayım — aile × bölüm, betik komutları | 1a | ✓ |
| §1.1.2 | Aday ekran listesi (`E-nn`) ve ekran dışı değer kümesi | 1a; 1b bir aday (E-54) ve dört ortak bileşen adı ekledi; 2 aday eklemedi, geri izlenebilirliğin önerilerini yazdı | ✓ (54 aday — 53 ekran, 1 ayar; nihai envanter §4'te — E-48 düştü → E-44, v0.6) |
| §1.1.3 | MVP Kapsamı — `10 §2` KP satırları, tamamı (77) | 1a | ✓ |
| §1.1.4 | Kullanıcı Akışları — müşteri tarafı: `03 §1.6.1`, §1.11.1–§1.11.14, §2, §3.1–§3.4, §4, §5, §6.1–§6.2, §9 (378 satır) | 1a | ✓ |
| §1.1.5 | Ürün Gereksinimleri — kural düzeyi, vitrin ve site: `02 §3.27`–§3.30, §3.32–§3.34, §12.4 ve `04`'e iş bırakan cümleler (87 satır) | 1a | ✓ |
| §1.1.6 | Ürün Gereksinimleri — alt bölüm düzeyi (97 alt bölümün 79'u; panel alt bölümleri §1.1.9'da) | 1a | ✓ |
| §1.1.7 | Devir dizinleri ve park satırları — müşteri tarafı (59 satır) | 1a | ✓ |
| §1.1.8 | Kullanıcı Akışları — firma tarafı ve kalan: `03 §1`'in kalan satırları, §3.5, §6.3, §7, §8, §10 (415 satır) | 1b | ✓ |
| §1.1.9 | Ürün Gereksinimleri — panel tarafı: `02 §3.31`, §3.3.4, §10.1, §10.6, §10.8 kural düzeyinde (14 satır); 18 panel alt bölümü | 1b | ✓ |
| §1.1.10 | Devir dizinleri — panel tarafı (Aşama 1: 45, Aşama 2: 17, gövde: 4) | 1b | ✓ |
| §1.1.11 | GAP adayları — karara bağlandı → §1.3 | 1a açtı (GA-1…GA-7); 1b ekledi (GA-8…GA-12); 2 doğruladı ve karara bağladı | ✓ (10 doğrulandı, 2 düştü) |
| §1.2 | Geri izlenebilirlik (ekran → kaynak); gerekçesiz ekleme listesi; örneklem doğrulaması | 2 | ✓ |
| §1.3 | Boşluklar (GAP) ve kararlar; geri besleme satırları (K-726) | 2 | ✓ (11 GAP) |

### 1.1 İleri izlenebilirlik (kaynak → ekran)

Her satır dört sütun taşır: **Kaynak ID · Kaynak özeti · Ekran · Durum**. "Ekran" sütununun değer kümesi §1.1.2'dedir. "Durum" üç değerden biridir: **eşlendi** (en az bir aday ekran ya da ortak bileşen), **ekran dışı** (§1.1.2'nin adıyla yazılmış ekran dışı değerlerinden biri — GAP değildir), **GAP adayı** (1a ve 1b'nin işaretiydi; 2. oturum hepsini karara bağladı ve matriste bu değeri taşıyan satır kalmadı). Bir boşluğun dokunduğu satır "Ekran" hücresinin sonunda boşluğun numarasını taşır — `(GAP-n)`, §1.3. Atıflar K-669 ve K-701'in okunuşuyla yazılır.

#### 1.1.1 Kaynak envanteri ve sayım

Birim K-727'dir; taraf ayrımı ve toplu satırlar K-730'dadır. Sayımlar `02` v0.55, `03` v0.13 ve `10` v0.37 üzerinde, 2026-10-04'te alındı; 2. oturumun geri beslemesinden sonra (`02` v0.56, `03` v0.14, `10` v0.38) betikler yeniden koşuldu ve aile sayıları değişmedi — geri besleme yeni satır ve yeni kural numarası eklemedi. Tek fark `02`'de `04`'ü anan satır sayısıdır: 21 iş bırakan satıra geri beslemenin sekiz atfı eklendi (altı Kaynak satırı, başlık notu, dipnot — hepsi `04 §1.3`'ü gösterir, iş bırakmaz) ve betik 29 döner; workshop'un (v0.57) ve yazım turunun 2a (v0.58) ve 2b (v0.59) oturumlarının geri beslemeleri sayıyı aynı türden atıflarla 33'e, 34'e ve 35'e çıkardı. Kısmi adet kararının geri beslemesi (v0.60; K-787…K-794) aynı türden atıflarla sayıyı 38'e çıkardı ve `02 §7`'ye iki kural numarası ekledi (§7.1.6, §7.1.7 — kalın numaralı kural 438); §7 kural düzeyinde matrise girmez, kuralın ekran yüzü `03`'ün satırlarıyla eşlenmiştir. Yazım turunun 3. oturumunun geri beslemesi (v0.61; K-805, K-806) sayıyı başlık notuyla 39'a çıkardı ve kural numarası eklemedi — `02 §3.4.1` ve §3.10.5'e birer cümle girdi; `03`'e (v0.17) satır eklenmedi. Yazım turunun 4. oturumunun geri beslemesi (v0.62; K-813, K-818, K-821) sayıyı başlık notuyla 40'a çıkardı ve kural numarası eklemedi — `02 §3.11.6`, §3.28.1 ve §10.6.2'ye birer cümle girdi. `02 §6`'ya ve `03`'e satır eklenmedi — `03`'ün yeni kuralı 1.7.4 bir paragraftır, tablo satırı değildir; matrisin aile sayıları değişmedi.

| Aile | Betiğin saydığı | 1a | 1b | Matrise girmeyen |
|---|---|---|---|---|
| `10 §2` KP satırları | 77 | 77 | — | — |
| `03` numaralı satırlar (`n.m.k` ve daha derin) | 793 | 378 | 415 | `03 §11`'in 45 satırı (`n.m` biçimli; `02`'ye geri besleme tablosu) |
| `02` kuralları — kural düzeyi (K-727'nin saydığı bölümler ve `04`'e iş bırakan cümleler) | 101 | 87 | 14 | `02 §1.1`'in `04`'ü anan gerekçe cümlesi (iş bırakmaz; §1.1.6'nın `02 §1.1` satırında) |
| `02` alt bölümleri — alt bölüm düzeyi | 97 | 79 | 18 | — |
| Aşama 1 devir dizini — `04` | 80 | 35 | 45 | — |
| Aşama 2 devir dizini — `04` | 35 | 18 | 17 | — |
| `03`'ün gövdesinde `04`'e iş bırakan cümleler | 7 | 3 | 4 | — |
| `04` park satırları | 3 | 3 | — | Aşama 2 dizin satırı (dizinin kendisine işaret eder) |
| **Toplam** | **1.193** | **680** | **513** | |

**`03` — bölüm bazında** (793): §1 98 (§1.4 13 · §1.5 7 · §1.6 12 · §1.7 17 · §1.10 8 · §1.11 41) · §2 99 · §3 147 (§3.1 6 · §3.2 28 · §3.3 36 · §3.4 17 · §3.5 60) · §4 59 (§4.1 47 · §4.2 12) · §5 33 · §6 52 (§6.1 22 · §6.2 21 · §6.3 9) · §7 131 (§7.1 54 · §7.2 30 · §7.3 47) · §8 106 · §9 35 · §10 33. **1a:** §1.6.1 8 + §1.11.1–§1.11.14 14 + §2 99 + §3.1–§3.4 87 + §4 59 + §5 33 + §6.1–§6.2 43 + §9 35 = 378. **1b:** §1'in kalanı 76 + §3.5 60 + §6.3 9 + §7 131 + §8 106 + §10 33 = 415.

**`02` — kural envanteri.** Kalın numaralı kural (`**n.m.k `) 436'dır: §3 257 · §4 5 · §5 18 · §6 6 · §7 42 · §8 28 · §9 10 · §10 42 · §12 28; `02 §6`'nın numaralı tablo satırı 141'dir ve her biri `03 §3`'ün bir satırının kaynağıdır (`03 §3` girişi). Öteki kimlik aileleri: B- 16 · F- 6 · Z- 47 · L- 9 · P- 49 · H- 4 · S 11 · Ö 5 — `03`'ün satırları üzerinden eşlenir (Z- → `03 §4.1`, L- → `03 §6.1.1`, B- ve F- → `03 §7`). **Kural düzeyinde matrise giren 101 satır:** `02 §3.27`–§3.34 76 kural (§3.27 28 · §3.28 6 · §3.29 6 · §3.30 7 · §3.31 2 · §3.32 10 · §3.33 9 · §3.34 8) + §10.1 5 + §10.6 3 + §10.8 2 + §12.4 1 = 87; bu bölümlerin dışında `04`'e iş bırakan 10 kural (§3.1.4, §3.3.4, §3.9.2, §3.13.3, §3.16.4, §3.19.5, §3.20.1, §3.24.1, §3.24.2, §3.24.6) ve kural numarası taşımayan 4 cümle (§3 girişi, §6 girişi, §10.6.3'ün "sıfır payda" maddesi, §11'in "envantere girmeyenler" notu). `02`'de `04`'ü anan satır 21'dir: 16'sı kural (6'sı sayılan bölümlerin içinde: §3.27.25, §3.28.3, §3.28.4, §3.29.5, §3.34.1, §10.6.2), 5'i kural dışı cümle. Aşama 1 dizininin §6.2'sindeki `04` hedefli 21 gövde cümlesi bu 21 satırın kendisidir ve matrise **bir kez**, `02` kimliğiyle girer. **1a:** 74 + 1 + 9 kural + 3 cümle = 87. **1b:** §3.31 2 + §10.1 5 + §10.6 3 + §10.8 2 + §3.3.4 + §10.6.3'ün "sıfır payda" maddesi = 14. `02 §1.1`'in gerekçe cümlesi `04`'ü anar ama iş bırakmaz; ayrı satır olmaz.

**Betik komutları** (Git Bash; depo kökünden). Sayımlar yukarıdaki sayılarla aynı çıkmalıdır:

```sh
# 10 §2 — KP satırları (77)
grep -cE '^\| KP-[0-9]+ ' Docs/10_MVP_SCOPE.md
# 03 — numaralı satırlar, üst bölüm ve alt bölüm bazında (793; {2,} §11'in n.m satırlarını dışarıda bırakır)
grep -oE '^\| [0-9]+(\.[0-9]+){2,} \|' Docs/03_USER_FLOWS.md \
  | awk '{split($2,a,"."); c[a[1]]++; d[a[1]"."a[2]]++} END{for(k in c) print "S"k, c[k]; for(k in d) print k, d[k]}' | sort -V
# 03 — yinelenen satır kimliği (boş dönmeli)
grep -oE '^\| [0-9]+(\.[0-9]+){2,} \|' Docs/03_USER_FLOWS.md | sort | uniq -d
# 02 — kalın numaralı kurallar, üst bölüm bazında (436) ve K-727'nin bölümleri
grep -oE '^\*\*[0-9]+\.[0-9]+\.[0-9]+ ' Docs/02_PRODUCT_REQUIREMENTS.md | tr -d '*' \
  | awk -F. '{c[$1]++; d[$1"."$2]++} END{for(k in c) print "S"k, c[k]; for(k in d) print k, d[k]}' | sort -V
# 02 — §6'nın numaralı tablo satırları (141) ve alt bölüm başlıkları (97)
grep -cE '^\| [0-9]+\.[0-9]+\.[0-9]+ \|' Docs/02_PRODUCT_REQUIREMENTS.md
grep -cE '^### [0-9]+\.[0-9]+ ' Docs/02_PRODUCT_REQUIREMENTS.md
# 02 — `04`'ü anan satırlar (v0.55'te 21; v0.56'da 29, v0.57'de 33, v0.58'de 34, v0.59'da 35, v0.60'ta 38, v0.61'de 39, v0.62'de 40 — artış `04 §1.3`'e geri besleme atfıdır, iş bırakmaz)
grep -cE '`04[` ]|`04_' Docs/02_PRODUCT_REQUIREMENTS.md
# bu dokümanın matris satırları — aile bazında sayım ve yinelenen kaynak kimliği (boş dönmeli)
awk -F'|' '/^#### 1\.1\./{s=$0} /^\| (`0[23] §|KP-|K-[0-9]|Park )/{c[s]++} END{for(k in c) print c[k], k}' Docs/04_UI_SPECS.md | sort -k3 -V
awk -F'|' '/^#### 1\.1\.([3-9]|10)/{s=substr($0,6,6)} /^\| (`0[23] §|KP-|K-[0-9]|Park )/{print s $2}' Docs/04_UI_SPECS.md | sort | uniq -d
```

**Kapsama denetimi (1b, 2026-10-04).** Beklenen kimlik kümesi kaynaklardan, matrisin kimlikleri bu dokümandan betikle çıkarıldı ve `comm` ile karşılaştırıldı: `03`'ün 793 satır kimliği §1.1.4 ve §1.1.8'de birer kez durur (eksik 0, fazla 0, yinelenen 0); `02`'nin 97 alt bölümü §1.1.6 ve §1.1.9'da birer kez durur; §1.1.10'un 62 kararı önceki sürümün bu alt bölüme yazdığı K listesiyle birebirdir. Aile bazında sayım 77 + 378 + 87 + 79 + 59 + 415 + 32 + 66 = **1.193**'tür. Durum dağılımı: 1a 680 satır — 592 eşlendi · 74 ekran dışı · 14 GAP adayı; 1b 513 satır — 410 eşlendi · 94 ekran dışı · 9 GAP adayı; toplam 1.002 eşlendi · 168 ekran dışı · 23 GAP adayı.

**Durum dağılımı — 2. oturumdan sonra (2026-10-04).** 1.193 satır: **1.026 eşlendi · 167 ekran dışı · 0 GAP adayı** — 1a 680 satır: 608 eşlendi · 72 ekran dışı; 1b 513 satır: 418 eşlendi · 95 ekran dışı. Değişim üç yerden gelir: (1) 23 GAP adayı satırı karara bağlandı ya da düştü — 22'si "eşlendi", biri (`03 §10.3.1`) "ekran dışı" oldu (§1.1.11, §1.3); (2) örneklem doğrulaması 1a'nın 20 satırına eksik kalan ekranı ekledi ve ikisinin durumu "ekran dışı"ndan "eşlendi"ye döndü (`03 §2.9.4`, §9.5.4 — §1.2'nin notu); (3) iki satır bir GAP numarası aldı, durumu değişmedi (`03 §1.7.3.4`, §8.7.4.4 — GAP-11). **Yazım turunun 2b oturumundan sonra (v0.8):** 1.193 satır — **1.028 eşlendi · 165 ekran dışı** (1a 610 · 70; 1b 418 · 95); iki 1a satırı (`03 §6.1.3.2`, §9.4.2) "ekran dışı"ndan "eşlendi"ye döndü — §1.2'nin 2b notu. **Yazım turunun 4. oturumundan sonra (v0.11):** 1.193 satır — **1.030 eşlendi · 163 ekran dışı** (1a 611 · 69; 1b 419 · 94); iki satır "ekran dışı"ndan "eşlendi"ye döndü — 1a'nın `03 §9.5.6`'sı ve 1b'nin §8.9.7'si; §1.2'nin 4. oturum notu. Sayım betiği:

```sh
# durum dağılımı — oturum ve toplam (1a: §1.1.3–§1.1.7, 1b: §1.1.8–§1.1.10)
awk -F'|' '/^#### 1\.1\./{split($0,h," "); s=h[2]} /^#### 1\.1\.11/{s=""}
  s!="" && /^\| (`0[23] §|KP-|K-[0-9]|Park )/{d=$5; gsub(/^ +| +$|\*/,"",d);
  g=(s=="1.1.8"||s=="1.1.9"||s=="1.1.10")?"1b":"1a"; c[g" "d]++; t[d]++}
  END{for(k in c) print k, c[k]; for(k in t) print "toplam", k, t[k]}' Docs/04_UI_SPECS.md | sort
```

**Yanlış pozitif denetimi (1a).** `03` kalıbı yalnız satır başındaki `| n.m.k |` hücresini yakalar: tarih, `S1…S11`, `Ö1…Ö5` ve öteki kimlik aileleri ilk hücrede bu biçimde durmaz (ilk hücre aileleri sayıldı: `n.m.k` 480 · `n.m.k.j` 313 · `n.m` 45 · `Sn` 11 · `Ön` 5 · başlık ve durum adları). `02` kalıbı yalnız satır başındaki kalın `n.m.k`'yı yakalar; dört düzeyli kalın kural yoktur (sıfır eşleşme). Yinelenen satır kimliği yoktur.

#### 1.1.2 Aday ekran listesi ve ekran dışı değer kümesi

Bu liste matrisin "Ekran" sütununun değer kümesidir; **nihai ekran envanteri değildir** — envanter `04 §4`'te, yazım turunda doğar (UI4-01, UI4-02) ve bir adayı bölebilir, birleştirebilir ya da adını değiştirebilir. Kimlik `E-nn`'dir (K-725); adayın numarası envantere kadar değişmez, yeni aday listenin **sonuna** eklenir (K-729). 1b panel satırlarını eşlerken bir aday ekledi (E-54) ve iki zayıf adayı kaynaklara karşı netleştirdi (E-38, E-48 — tablonun altındaki not).

| # | Aday ekran | Taraf | Aktör(ler) | Amaç |
|---|---|---|---|---|
| E-01 | Ana sayfa | Müşteri | Ziyaretçi · Müşteri | Firmanın seçtiği düzende kurumsal tanıtımı ve ürün vitrinini birlikte gösterir. |
| E-02 | Kategori sayfası | Müşteri | Ziyaretçi · Müşteri | Bir kategorinin ve alt dallarının ürünlerini tek listede gösterir ve süzdürür. |
| E-03 | Arama sonuçları | Müşteri | Ziyaretçi · Müşteri | Ürün adında yapılan aramanın sonucunu kategori sayfasının düzeniyle gösterir. |
| E-04 | Ürün sayfası | Müşteri | Ziyaretçi · Müşteri | Ürünün bilgisini gösterir, varyantı seçtirir ve sepete ekletir. |
| E-05 | Arşivlenmiş ürün sayfası | Müşteri | Ziyaretçi · Müşteri | "Bu ürün artık satılmıyor" bilgisini fiyatsız gösterir. |
| E-06 | "Sayfa bulunamadı" sayfası | Müşteri | Ziyaretçi · Müşteri | Var olmayan ya da ziyaretçiye kapalı adreste dönüş yollarını gösterir. |
| E-07 | İçerik liste sayfası | Müşteri | Ziyaretçi · Müşteri | Hizmet tanıtımlarını ya da referans işleri firmanın sırasıyla listeler. |
| E-08 | İçerik sayfası | Müşteri | Ziyaretçi · Müşteri | Hakkımızda'yı, bir hizmet tanıtımını, bir referans işi ya da bir genel sayfayı gösterir. |
| E-09 | Sık sorulan sorular sayfası | Müşteri | Ziyaretçi · Müşteri | Bütün soruları ve cevapları tek sayfada listeler. |
| E-10 | İletişim sayfası ve formu | Müşteri | Ziyaretçi · Müşteri | Firmanın iletişim ve yasal kimlik bilgilerini, şubeleri ve iletişim formunu — gönderim sonrası hâliyle — taşır. |
| E-11 | Yasal metin sayfası | Müşteri | Ziyaretçi · Müşteri | Aydınlatma metnini, çerez politikasını ya da "İşlem rehberi"ni gösterir. |
| E-12 | Sepet | Müşteri | Ziyaretçi · Müşteri | Kalemleri güncel fiyat ve satın alınabilirlikle gösterir ve ödeme adımına geçirir. |
| E-13 | Ödeme adımı | Müşteri | Müşteri | E-postayı, adresleri, kuponu ve ödeme yöntemini alır; onay özetini ve onay kutularını gösterir ve siparişi onaylatır. |
| E-14 | Sipariş teyit ve bekleme ekranı | Müşteri | Müşteri | Siparişin alındığını, numarasını, havalede IBAN'ı ve kartta ödemenin beklendiğini gösterir. |
| E-15 | Sipariş takibi girişi | Müşteri | Misafir alıcı | Sipariş numarası ve e-postayla sipariş sayfasını açtırır. |
| E-16 | Sipariş sayfası | Müşteri | Müşteri | Siparişin durumunu ve bütün bilgisini gösterir; o anda açık işlemleri sunar. |
| E-17 | İptal ve gecikme feshi ekranı | Müşteri | Müşteri | Siparişi ya da kalemi iptal ettirir ya da gecikme nedeniyle feshettirir; havalede IBAN'ı alır. |
| E-18 | Cayma beyanı ekranı | Müşteri | Müşteri | Kalemi seçtirir, iade adresini ve yükümlülüğü gösterir, beyanı ve havalede IBAN'ı alır. |
| E-19 | Ayıp talebi ekranı | Müşteri | Müşteri | Kalemi seçtirir ve sorunun açıklamasını alır; talebi yeniden açtırır. |
| E-20 | Kayıt ekranı | Müşteri | Ziyaretçi | Ad, e-posta ve şifreyle hesap açtırır; doğrulama bağlantısını yeniden istetir. |
| E-21 | Doğrulama bağlantısının iniş ekranı | Müşteri | Kullanıcı | Kayıt ya da yeni e-posta doğrulamasının sonucunu — geçersiz bağlantı dahil — söyler. |
| E-22 | Müşteri girişi | Müşteri | Ziyaretçi · Üye | E-posta ve şifreyle ya da Google ile giriş yaptırır. |
| E-23 | Şifre sıfırlama ekranları | Müşteri | Kullanıcı | Sıfırlama bağlantısını istetir ve yeni şifreyi kurdurur. |
| E-24 | Yeniden doğrulama ekranı | Müşteri | Üye | Şifre, e-posta değişikliği ve hesap silmeden önce kimliği yeniden doğrular. |
| E-25 | Hesap — profil ve güvenlik | Müşteri | Üye | Adı, e-postayı ve şifreyi değiştirtir; hesabı sildirir. |
| E-26 | Adres defteri | Müşteri | Üye | Kayıtlı adresleri ekletir, düzenletir ve sildirir. |
| E-27 | Sipariş geçmişi | Müşteri | Üye | Hesaba bağlı siparişleri listeler ve sipariş sayfasına götürür. |
| E-28 | E-posta değişikliğini geri alma ekranı | Müşteri | Kullanıcı | "Bu değişikliği ben yapmadım" bağlantısının sonucunu söyler ve şifreyi yeniden belirletir. |
| E-29 | Panel girişi ve şifre sıfırlama | Panel | Yönetici | Yöneticiyi şifreyle panele alır; panel şifresini sıfırlatır. |
| E-30 | Yönetici daveti kabul ekranı | Panel | Kullanıcı | Davet bağlantısından yönetici hesabını açtırır. |
| E-31 | Panel ana sayfası | Panel | Yönetici | Bekleyen işleri sayaçlarla, kurulum kontrol listesini ve kanal uyarısını gösterir. |
| E-32 | Ürün listesi | Panel | Yönetici | Ürünleri yayın durumlarıyla listeler. |
| E-33 | Ürün formu | Panel | Yönetici | Ürünü, varyantlarını, stoğunu, görsellerini, indirimini, dijital dosyasını ve cayma istisnasını düzenletir. |
| E-34 | Kategori ağacı | Panel | Yönetici | Kategori ağacını kurdurur ve dalları taşıtır. |
| E-35 | Kuponlar | Panel | Yönetici | Kuponları listeler ve düzenletir. |
| E-36 | Sipariş listesi | Panel | Yönetici | Siparişleri aratır ve durumuna göre süzdürür. |
| E-37 | Sipariş ayrıntısı | Panel | Yönetici | Siparişin bütün bilgisini gösterir; yürütüm işlemlerini ve müdahaleleri yaptırır. |
| E-38 | Ayıp talepleri | Panel | Yönetici | Açık ayıp taleplerini kalem ve sipariş bağlamıyla listeler; talebi çözüldü işaretletir ve yeniden açtırır. |
| E-39 | İletişim talepleri | Panel | Yönetici | İletişim taleplerini listeler, okutur, kapattırır ve yeniden açtırır. |
| E-40 | Üye kaydı görünümü | Panel | Yönetici | Bir üyeyi e-postayla aratır, kaydını salt okunur gösterir ve talep üzerine sildirir. |
| E-41 | Kurumsal içerik | Panel | Yönetici | İçerik tiplerinin, genel sayfaların ve duyurunun kayıtlarını listeler ve düzenletir. |
| E-42 | Ana sayfa, menü ve sosyal bağlantılar | Panel | Yönetici | Ana sayfa düzenini seçtirir, menü adlarını ve sosyal bağlantıları düzenletir. |
| E-43 | Marka | Panel | Yönetici | Logoyu, marka adını, site simgesini, marka rengini ve platform imzasını ayarlatır. |
| E-44 | Firma kimliği | Panel | Yönetici | Firma tipine göre yasal kimlik ve iletişim bilgilerini girdirir. |
| E-45 | Ödeme yöntemleri | Panel | Yönetici | Havaleyi açtırır, kapattırır ve IBAN'ı girdirir. |
| E-46 | Kargo, teslimat, süreler, eşikler ve iade adresi | Panel | Yönetici | Kargo ücretini, teslimat illerini, firma ayarı olan süreleri ve eşikleri ve iade adresini düzenletir. |
| E-47 | Yasal metinler | Panel | Yönetici | Aydınlatma metnini ve çerez politikasını düzenletip yayımlatır; üretilen iki metni gösterir. |
| E-48 | Satışın geçici kapatılması | Panel | Yönetici | Satışı geçici olarak kapatan ve yeniden açan anahtarı taşır — bir ayardır; yeri envanterde belirlenir. |
| E-49 | Yönetici hesapları | Panel | Yönetici | Yöneticileri listeler; davet ettirir, daveti geri çektirir ve yönetici kaldırtır. |
| E-50 | Yöneticinin kendi hesabı | Panel | Yönetici | Yöneticinin kendi e-postasını ve şifresini değiştirtir. |
| E-51 | Satış özeti | Panel | Yönetici | Seçilen dönemin satış ölçülerini gösterir. |
| E-52 | İşlem izi | Panel | Yönetici | Yönetici işlemlerinin izini tarih ve yönetici süzgeciyle okutur. |
| E-53 | Dışa aktarma | Panel | Yönetici | Siparişleri, üye listesini ve iletişim taleplerini dosya olarak dışa aktartır. |
| E-54 | Yönetici hesabının doğrulama ekranları | Panel | Yönetici · Kullanıcı | Yöneticinin kimliğini şifre ve e-posta değişikliğinden önce yeniden doğrular; yeni adresin doğrulama bağlantısının ve "bu değişikliği ben yapmadım" bağlantısının sonucunu söyler. |

Aday sayısı 54'tür: müşteri tarafı 28 (E-01…E-28), panel 26 (E-29…E-54). Aktör adları `03 §0.3.2`'nin adlarıdır (K-700).

**Envanter doğdu (yazım turunun 1. oturumu, 2026-10-04, v0.6; K-764).** Nihai ekran envanteri §4'tedir: **53 ekran** — müşteri tarafı 28, panel 25; E-48 düştü ve satırları E-44'e taşındı. Yukarıdaki tablo aday listesi olarak, o günkü adlarıyla durur; adı ya da kapsamı değişen adaylar — E-38 (Talepler), E-39 (İletişim talebi ayrıntısı), E-42, E-44, E-46 — ve her farkın gerekçesi §4.3'tedir. Matrisin "Ekran" sütunu bundan sonra §4'ün kimlikleriyle okunur; E-48 değeri matriste kalmadı.

**1b'nin netleştirdiği adaylar (2026-10-04).** Numaralar değişmedi (K-729); birleştirme ya da bölme envanterin işidir (UI4-02).

- **E-38 — aday kalır, kaynağı vardır.** Ayıp talebi "kalem ve sipariş bağlamıyla panele düşer" ve "açık talep" sayacındadır (`03 §8.4.10`); her sayaç kendi süzülmüş listesine götürür (`03 §8.9.1`; `02 §10.6.1`) ve `02 §10.1.2` "Talepler"i panelin on iki alanından biri sayar. Talebin işlemleri — çözüldü işareti, yeniden açma, para gerektiren çözüm — siparişin bağlamında yürür (E-37). Kaynakların söylemediği: "açık talep" sayacı tektir ve iletişim taleplerini de sayar; sayacın götürdüğü listenin tek mi (iki tür birlikte) iki mi (E-38 ve E-39 ayrı) olduğu yazılı değildir — GA-9.
- **E-48 — ayrı ekran değil, ayardır.** Kaynak onu "anahtar" diye adlandırır (`02 §3.1.5`'in üçüncü koşulu, §3.1.6), `02 §10.1.2` "Firma kimliği ve satış" alanında sayar, `03 §8.7.5` mağaza ayarlarının bir alt bölümü olarak yazar ve işlem izi onu "panelden değişen ayar" diye kaydeder (`02 §10.3.1`). Aday, matriste anahtarın ve onun onayının satırlarını toplamak için durur; anahtarın hangi ayar ekranında duracağı envanterde ve panelin gezinme iskeletinde belirlenir (UI3-02, UI9-09). Etkilenen 1a satırları (KP-60) değişmedi.
- **E-54 — yeni aday.** Yönetici hesabı müşteri hesabından ayrı bir hesap türüdür ve ayrı kapıdan girilir (`02 §10.2.5`); yeniden doğrulama, e-posta değişikliği ve geri alınması yönetici hesabında müşteriyle aynı kurallarla işler (`02 §10.2.4`; `03 §9.3.7`). Müşteri tarafının E-21, E-24 ve E-28 adayları vitrinin kapısındadır; panelin karşılıkları bu adayda toplanır. 1a'nın `03 §9.3.7` satırı E-50 · E-54 olarak güncellendi.
- **Aday açılmayanlar.** Sitenin kesintisi ve bakım için ekran yoktur: kesintide ürün çalışmaz ve ürünün içinden bilgilendirme yoktur (`03 §10.3.1`), bakım modu yoktur (`03 §10.2.1`; `02 §3.1.6`). Bekleyen işler için ayrı ekran yoktur: sayaçlar ana sayfadadır ve var olan listelerin süzülmüş hâline götürür — "yeni bir ekran ve yeni veri yoktur" (`02 §10.6.1`). Toplu işlem ekranı yoktur (`03 §8.1.15`; `02 §10.7.2`). İlk kurulumda sihirbaz yoktur; kurulum kontrol listesi ana sayfadadır (`02 §10.8.2`).

**2. oturumun önerileri — geri izlenebilirlikten (2026-10-04; K-738).** Kaynağı olmayan aday yoktur: 54 adayın ve on beş ortak bileşen adının her biri §1.1.3–§1.1.10'un en az bir satırında geçer (§1.2). Kaynağı zayıf ya da ekran olmayan adaylar için öneriler aşağıdadır; numaralar değişmedi (K-729), kesin karar ekran envanterinindir (`04 §4`; UI4-01, UI4-02).

| Aday | Bulgu | Öneri |
|---|---|---|
| E-48 Satışın geçici kapatılması | Ekran değil, ayardır — kaynak "anahtar" der (`02 §3.1.5`, §3.1.6; `03 §8.7.5`) ve `02 §10.1.2` onu "Firma kimliği ve satış" alanında sayar | **Bir ayar ekranına iner.** Envanterde ayrı ekran olmaz; sekiz satırı anahtarın duracağı ayar ekranına taşınır. `02 §10.1.2`'nin alanı firma kimliğiyle aynıdır (E-44); kesin yer panelin gezinme iskeletinde belirlenir (UI3-02, UI9-09). **Workshop (K-752):** anahtar "Firma kimliği ve satış" ekranının (E-44) başındaki "Satış durumu" bölümündedir |
| E-54 Yönetici hesabının doğrulama ekranları | Beş satır; müşteri tarafındaki E-21, E-24 ve E-28 ile aynı kurallar (`02 §10.2.4`) | **Kalır; müşteri tarafıyla birleşmez.** İki hesap türü ayrı kapıdan girer (`02 §10.2.5`), panelde Google ile yeniden doğrulama yoktur ve ekranlar panelin çerçevesindedir. Düzen müşteri tarafındaki üç ekranla aynı kalıptır; yazım turu kalıbı bir kez tanımlar, iki tarafta kullanır. Üç ekrana bölünmesi ya da E-50 ile birleşmesi envanterin işidir |
| E-50 Yöneticinin kendi hesabı | Dört satır | **Kalır.** Yönetici hesabının adı da burada değişir (GAP-8); E-54 ile komşudur |
| E-09 Sık sorulan sorular · E-05 Arşivlenmiş ürün sayfası · E-26 Adres defteri | Dört–beş satır | **Kalır.** Üçünün de kendi kuralı vardır (`02 §3.27.6`; `02 §3.7`, KP-7; `02 §3.14`, `03 §9.3.2`); satır azlığı ekranın küçüklüğündendir, kaynaksızlıktan değil |
| ortak bileşen: *boş* | Tek satır (`02 §10.8.1` — site boş kurulur) | **Kalır.** Şablon her ekran tanımında "boş / yükleniyor / hata durumları"nı ister (`04 §5`) ve konu planı kalıbı sayar (UI2-06); kaynaklar boş hâli çoğu yerde ekranın kendi satırında söyler (boş kategori — KP-1, tarayıcıda boş görünen sepet — `03 §2.3.7`). Beklenmeyen hatanın hâli aynı kalıpta karara bağlanır (GAP-10) |
| E-24, E-29, E-35, E-50, E-54 | `10 §2`'den beslenen satırı yok | **Kalır — gerekçesiz ekleme değildir.** Beşinin yeteneği kapsam satırlarının içindedir (yeniden doğrulama ve panel girişi KP-27 ve KP-62'de, kuponun tanımlanması KP-43'te); 1a kapsam satırlarını yalnız birincil ekranlarına eşledi. Beşi de `02` ve `03`'ten beslenir |

**Ekran dışı değer kümesi (K-729).** Ekran doğurmayan bir kaynak satırı GAP değildir; "Ekran" sütununa aşağıdaki sabit değerlerden biri, adıyla yazılır. Bir satır hem bir ekrana hem bir ekran dışı değere eşlenebilir (` · ` ile); en az bir ekran ya da ortak bileşen taşıyan satırın durumu "eşlendi"dir.

| Değer | Ne zaman yazılır | Gerekçesi |
|---|---|---|
| ortak bileşen: *ad* | Satır tek bir ekranın değil, birden çok ekranda yinelenen bir kalıbın işidir | Kalıp `04 §2`'de bir kez tanımlanır (UI2); durumu "eşlendi"dir. 1a'nın kullandığı adlar: *çerçeve* (vitrinin üst bölümü, menüsü, duyuru şeridi, altbilgisi) · *mesaj* (hata, uyarı, bilgi ve nötr limit mesajının kalıbı) · *onay* (geri alınamaz işlemin onay penceresi) · *taban* (tasarım tabanı: kırılma noktaları, marka rengi, erişilebilirlik) · *kart* (ürün kartı) · *adres* (adres formu) · *IBAN* (IBAN alanı) · *rozet* (durum rozetleri ve panel işaretleri) · *süre* (sayaç ve süre göstergeleri) · *taslak* ("Taslak" önizleme bandı) · *satış-kapalı* (satış kapalıyken site hâli). 1b'nin eklediği adlar: *eşzamanlı* (kaydın ya da siparişin arada değiştiğini söyleyen uyarı) · *sebep* (kapalı listeden sebep seçimi) · *tarih* (geçmişe dönük, ileri tarihsiz tarih alanı ve sıra sorusu) · *boş* (boş liste ve boş site hâli). Adlar adaydır; bileşen kimliği yazım turunda verilir (UI0-05) |
| ekran dışı — e-posta (`08`) | Satırın çıktısı bir e-postadır | E-postanın metni ve şablonu `08`'in işidir; `04` yalnız e-postanın ekranda bıraktığı izi taşır (karar kaydı §10.2, "`04`'ün kapsamı") |
| ekran dışı — arka plan | Sistemin kendiliğinden yaptığı işlem ya da zamanlayıcı: ayırma, donma, kendiliğinden iptal, imha | Kullanıcının karşısına bir ekran çıkmaz; sonucu bir ekranda görünüyorsa o ekran da yazılır |
| ekran dışı — ürünün dışında | Ödeme sağlayıcısının sayfası, banka, kargo, firmanın kendi yazışması, merci | Ürün o adımı göstermez ve izlemez (`03 §0.3.2`'nin dış olayı; UI4-03) |
| ekran dışı — mimari (`05`) | Arama motoru verileri, adres üretimi, veri kaybı toleransı, genel istek hızı sınırı | Karşılığı mimaridedir (`03 §0.1.4`) |
| ekran dışı — doğrulama (`12`) | Tarayıcı tabanı ve erişilebilirlik denetimi gibi yalnız doğrulanan kurallar | Ekrana değil doğrulama ölçütüne dönüşür (`03 §0.1.4`) |
| ekran dışı — kurulum (`10 §4.1`) | Kurulum tarafının panelin dışındaki işi (ÖK-1…ÖK-7) | Ekran doğurmaz (karar kaydı §10.3, tamlık taraması) — 1a'da kullanılmadı, 1b'nin `03 §10` satırları için |
| ekran dışı — yazım konvansiyonu | Sözlük, aktör listesi, atıf biçimi | `04`'ün başlık notuna yazılır (K-726; UI0-05), ekran değildir |
| yoktur — kapsam dışı | Kaynağın "yoktur" dediği yetenek (`03 §0.1.3`; `10 §3`) | Ekrana girmez; ilgili ekran tanımında "yoktur" diye anılır. Yokluk bir ekranın görünümünü değiştiriyorsa (sunulmayan düğme) satır o ekrana eşlenir |

#### 1.1.3 MVP Kapsamı — `10 §2` KP satırları (77)

Kapsam ağıdır: her satır en az bir ekrana ya da adıyla yazılmış bir ekran dışı değere eşlenir (K-727). Sıra `10 §2`'nin sırasıdır.

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| KP-1 | Üç seviyeli kategori ağacında gezinme; boş kategori menüde görünmez | E-02 · ortak bileşen: çerçeve | eşlendi |
| KP-2 | Ürün adında arama; Türkçe karaktere duyarsız | E-03 · ortak bileşen: çerçeve | eşlendi |
| KP-3 | Fiyat aralığı ve stok durumuna göre süzme; tek sabit düzen | E-02 · E-03 | eşlendi |
| KP-4 | Varyant seçimi; tükenmiş ya da açılmamış kombinasyon seçilemez | E-04 | eşlendi |
| KP-5 | Ürün sayfası: görsel, açıklama, KDV dahil fiyat, fiyat tarihi, üretim yeri | E-04 | eşlendi |
| KP-6 | Üç ürün tipi aynı katalogda ve aynı sepette | E-04 · E-12 · E-13 | eşlendi |
| KP-7 | Arşivlenmiş ürün sayfası; taslak ve silinmiş ürün "sayfa bulunamadı" | E-05 · E-06 | eşlendi |
| KP-8 | Hesap açmadan sipariş; misafirin e-postası özette görünür ve düzeltilir | E-13 · E-37 | eşlendi |
| KP-9 | Sepete ekleme; canlı fiyat; stok ve ürün sınırı | E-04 · E-12 | eşlendi |
| KP-10 | Üyenin sepeti hesapta, misafirinki tarayıcıda; girişte birleşir | E-12 | eşlendi |
| KP-11 | Kupon kodu; sipariş tutarını sıfıra indiremez | E-13 | eşlendi |
| KP-12 | Teslimat ve fatura adresi; adres defterinden seçim, deftere kaydetme | E-13 · E-26 · ortak bileşen: adres | eşlendi |
| KP-13 | Kargo ücreti, ücretsiz kargoya kalan tutar, asgari tutar, teslimat illeri | E-12 · E-13 | eşlendi |
| KP-14 | Onay özeti, Ön Bilgilendirme Formu, onay kutuları, ödeme yükümlülüğü düğmesi | E-13 | eşlendi |
| KP-15 | Kart (sağlayıcının sayfasında, 3D Secure) ya da havale; ekranda sipariş numarası ve IBAN | E-14 · E-16 · ekran dışı — ürünün dışında | eşlendi |
| KP-16 | Stok, kontenjan ve kupon hakkı ödeme süresince ayrılır | ekran dışı — arka plan | ekran dışı |
| KP-17 | Tahmin edilemez sipariş numarası; sipariş anında donan değerler | E-14 · E-16 · ekran dışı — arka plan | eşlendi |
| KP-18 | Sipariş sayfasına üç giriş yolu: geçmiş, numara ve e-posta, erişim anahtarı | E-15 · E-16 · E-27 | eşlendi |
| KP-19 | Kargoya verme süresi vitrinde; takip bilgisi ve "Takip et" sipariş sayfasında | E-04 · E-16 | eşlendi |
| KP-20 | Dijital ürünün sipariş sayfasından indirilmesi | E-16 | eşlendi |
| KP-21 | Müşterinin kalem ya da sipariş iptali, firma onayı beklemeden | E-16 · E-17 | eşlendi |
| KP-22 | Sipariş sayfasından cayma — fiziksel ve hizmet kalemi | E-16 · E-18 | eşlendi |
| KP-23 | Sipariş sayfasından ayıp talebi | E-16 · E-19 | eşlendi |
| KP-24 | Tek kalemin iptali ya da iadesi; para kısmen döner | E-16 · E-17 · E-18 · E-37 | eşlendi |
| KP-25 | Ad, e-posta ve şifreyle hesap açma; doğrulanana kadar çalışmaz | E-20 · E-21 | eşlendi |
| KP-26 | Google hesabıyla giriş | E-22 · E-20 · ekran dışı — ürünün dışında | eşlendi |
| KP-27 | "Beni hatırla", şifre sıfırlama ve değiştirme, ad ve e-posta değiştirme, adres defteri, geri alma | E-22 · E-23 · E-25 · E-26 · E-28 | eşlendi |
| KP-28 | Misafir siparişlerinin hesaba düşmesi; e-posta düzeltmesinde bağın güncellenmesi | E-27 · E-37 · ekran dışı — arka plan | eşlendi |
| KP-29 | Üyenin hesabını silmesi ya da firmanın talep üzerine silmesi | E-25 · E-40 | eşlendi |
| KP-30 | Kişisel veri başvurusu: iletişim formunun "KVKK talebi" tipi | E-10 · E-11 | eşlendi |
| KP-77 | Panelin salt okunur üye kaydı görünümü ve talep üzerine hesap silme | E-40 | eşlendi |
| KP-31 | Ana sayfada kurumsal tanıtım ve ürün vitrini birlikte | E-01 | eşlendi |
| KP-32 | Hakkımızda, hizmet tanıtımı, referans iş, SSS ve genel sayfalar; "İlgili ürünler" | E-07 · E-08 · E-09 | eşlendi |
| KP-33 | İletişim sayfası: iletişim bilgileri, şubeler, "Haritada aç", form | E-10 | eşlendi |
| KP-34 | İletişim formundan girişsiz talep | E-10 · E-38 · E-39 | eşlendi |
| KP-35 | Yasal kimlik ve iletişim bilgileri her sayfadan erişilebilir; ETBİS bandı; "İşlem rehberi" | ortak bileşen: çerçeve · E-10 · E-11 | eşlendi |
| KP-36 | Sosyal medya ve WhatsApp bağlantıları, duyuru şeridi, "Paylaş" düğmesi | ortak bileşen: çerçeve · E-04 · E-08 | eşlendi |
| KP-37 | Arama motoru verileri otomatik; kapalı sayfalar | ekran dışı — mimari (`05`) | ekran dışı |
| KP-38 | Satış kapalıyken vitrin: ürünler görünür, sepete ekleme ve ödeme kapalı | ortak bileşen: satış-kapalı · E-04 · E-12 · E-31 | eşlendi |
| KP-39 | Ürünün ve varyantlarının tanımlanıp yayına alınması | E-32 · E-33 | eşlendi |
| KP-40 | Ürünün taslak, yayında, arşiv arasında taşınması; "Taslak" bandıyla önizleme; kalıcı silme | E-32 · E-33 · ortak bileşen: taslak | eşlendi |
| KP-41 | Kategori ağacı; ürünün birden çok kategoriye asılması | E-34 · E-33 | eşlendi |
| KP-42 | Ürün görselleri, alternatif metin, metin biçimi seti | E-33 | eşlendi |
| KP-43 | Tarihli yüzde indirim; referans fiyatı sistem hesaplar | E-33 · E-04 | eşlendi |
| KP-44 | Dijital dosyanın yüklenmesi; indirme hakkının yenilenmesi | E-33 · E-37 | eşlendi |
| KP-45 | Cayma hakkı istisnası işareti ve kapalı listeden sebep | E-33 · E-13 · E-16 | eşlendi |
| KP-46 | Sipariş listesinde arama ve süzme; "ödendi", kargoya verme, teslim, hizmet tamamlama | E-36 · E-37 | eşlendi |
| KP-47 | Firma iptali — kapalı listeden sebep; "stokta bulunamadı" uyarısı | E-37 · ortak bileşen: onay | eşlendi |
| KP-48 | Sipariş müdahaleleri: adres, kalem çıkarma, takip, tarih, durum düzeltmesi, iç not, başka kanal kaydı, e-posta düzeltmesi | E-37 | eşlendi |
| KP-49 | Ayıp talebini çözme ve yeniden açma; iletişim taleplerini listeleme ve kapatma | E-38 · E-39 | eşlendi |
| KP-50 | Panel ana sayfasında altı sayaç ve listeleri | E-31 · E-36 | eşlendi |
| KP-51 | Manuel adım bütçesi: sipariş başına en fazla üç zorunlu elle adım | E-37 | eşlendi |
| KP-52 | Siparişlerin CSV olarak dışa aktarılması | E-53 | eşlendi |
| KP-53 | Satış özeti: dönem, sipariş sayısı, ciro, oranlar | E-51 | eşlendi |
| KP-54 | Kurumsal içerik: taslak, önizleme, yayın, silme | E-41 · ortak bileşen: taslak | eşlendi |
| KP-55 | Ana sayfa düzeni seçimi, menü adları, sosyal bağlantılar | E-42 | eşlendi |
| KP-56 | Logo, marka adı, site simgesi, marka rengi, platform imzası | E-43 | eşlendi |
| KP-57 | Duyurunun taslakta hazırlanıp tarihli yayımlanması | E-41 | eşlendi |
| KP-58 | Firma kimliği — firma tipine göre zorunlu alanlar | E-44 | eşlendi |
| KP-59 | Havale ve IBAN; kargo ücreti, eşik, teslimat illeri, süreler, iade adresi | E-45 · E-46 | eşlendi |
| KP-60 | Satışın geçici kapatılması | E-44 | eşlendi |
| KP-61 | Aydınlatma metni ve çerez politikasının düzenlenmesi; üretilen iki metin | E-47 | eşlendi |
| KP-62 | Yönetici daveti, geri çekme, kaldırma | E-49 · E-30 | eşlendi |
| KP-63 | İşlem izinin okunması | E-52 | eşlendi |
| KP-64 | İşletme parametrelerinin panelden değiştirilmesi | E-46 | eşlendi |
| KP-65 | Kurulum kontrol listesi; site boş kurulur | E-31 | eşlendi |
| KP-66 | Müşteriye on altı olayda e-posta (B-1…B-16) | ekran dışı — e-posta (`08`) | ekran dışı |
| KP-67 | Firmaya ve yöneticilere bildirim e-postaları (F-1…F-6) | ekran dışı — e-posta (`08`) | ekran dışı |
| KP-68 | Yanıt adresi; yeniden deneme; panelde "e-posta ulaşmadı" işareti ve kanal uyarısı | ekran dışı — e-posta (`08`) · E-37 · E-31 | eşlendi |
| KP-69 | Duyarlı tek site; panelin her işi telefondan | ortak bileşen: taban | eşlendi |
| KP-70 | Tarayıcı tabanı — son iki büyük sürüm | ekran dışı — doğrulama (`12`) | ekran dışı |
| KP-71 | Erişilebilirlik tabanı — Kontrol Listesi A Seviyesi ve WCAG 2.2 | ortak bileşen: taban | eşlendi |
| KP-72 | Deneme limitleri — nötr mesaj | ortak bileşen: mesaj | eşlendi |
| KP-73 | Çerez onayı yoktur; yalnız zorunlu çerezler | yoktur — kapsam dışı · E-11 | eşlendi |
| KP-74 | Aydınlatma metni bağlantısı her kişisel veri ekranında; periyodik imha | E-20 · E-22 · E-13 · E-10 · E-16 · E-17 · E-18 · E-19 · ekran dışı — arka plan | eşlendi |
| KP-75 | Sipariş tarafında veri kaybı toleransı sıfır | ekran dışı — mimari (`05`) | ekran dışı |
| KP-76 | Eşzamanlı düzenleme uyarısı; sipariş işlemleri güncel hâle karşı | E-33 · E-41 · E-37 · ortak bileşen: mesaj | eşlendi |

#### 1.1.4 Kullanıcı Akışları — müşteri tarafı (378)

`03`'ün satırları satır kimliğiyle, tek tek (K-727). Özet satırın kendisinden — aktörün yaptığı ve sistemin cevabı — alınmıştır. Hata, zaman aşımı, itiraz ve kötüye kullanım satırlarının ekranı, bağlandıkları ana akış adımının ekranıdır. Yöneticinin müşteri akışını ilerleten adımları panel adayına (çoğu E-37) eşlenmiştir; panelin kendi akış satırları (`03 §8`) §1.1.8'dedir. `03 §2.10.2`–§2.10.5 anlatılardır, numaralı satır taşımaz ve işaret ettikleri satırlar üzerinden eşlenir (K-730).

**§1.6.1 Geri alınamaz onay isteyen işlemler (8)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §1.6.1.1` | Havale "ödendi" işaretinin onayı; dijital kalemde düzeltilemeyeceği önceden söylenir | E-37 · ortak bileşen: onay | eşlendi |
| `03 §1.6.1.2` | Kargoya verme ve yeniden gönderim onayı | E-37 · ortak bileşen: onay | eşlendi |
| `03 §1.6.1.3` | Firma iptali onayı; "stokta bulunamadı" sebebinde yasal uyarı | E-37 · ortak bileşen: onay | eşlendi |
| `03 §1.6.1.4` | Geri ödemenin işlenmesi onayı — tutar bazlı kısmi geri ödeme dahil | E-37 · ortak bileşen: onay | eşlendi |
| `03 §1.6.1.5` | Kart hattında karta para gönderen kalem çıkarma onayı | E-37 · ortak bileşen: onay | eşlendi |
| `03 §1.6.1.6` | İade reddi onayı — sonucu tek cümleyle söyler | E-37 · ortak bileşen: onay | eşlendi |
| `03 §1.6.1.7` | Başka kanaldan gelen cayma ya da gecikme feshi bildiriminin kaydı onayı | E-37 · ortak bileşen: onay | eşlendi |
| `03 §1.6.1.8` | Üye hesabının talep üzerine silinmesi onayı | E-40 · ortak bileşen: onay | eşlendi |

**§1.11 Durum × rol × işlem — müşteri, sipariş sayfası (14)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §1.11.1` | Siparişi bütünüyle iptal etmek — Alındı + Bekliyor | E-16 · E-17 | eşlendi |
| `03 §1.11.2` | Fiziksel kalemi iptal etmek — ödeme onayından kargoya verilene kadar | E-16 · E-17 | eşlendi |
| `03 §1.11.3` | Hizmet kalemini iptal etmek — "tamamlandı" işaretine kadar | E-16 · E-17 | eşlendi |
| `03 §1.11.4` | Dijital kalemi iptal etmek — yoktur, düğme sunulmaz | E-16 | eşlendi |
| `03 §1.11.5` | Fiziksel kalemden caymak — Kargoya verildi'den sonra | E-16 · E-18 | eşlendi |
| `03 §1.11.6` | Hizmet kaleminden caymak — ödeme onayından sonra | E-16 · E-18 | eşlendi |
| `03 §1.11.7` | Dijital kalemden caymak — yoktur, düğme sunulmaz | E-16 | eşlendi |
| `03 §1.11.8` | Gecikme nedeniyle fesih — Z-11 geçmiş, teslim tarihi girilmemiş | E-16 · E-17 | eşlendi |
| `03 §1.11.9` | Ayıp talebi açmak ("sorun bildir") | E-16 · E-19 | eşlendi |
| `03 §1.11.10` | Ayıp talebini yeniden açmak — Çözüldü'yken, Z-18 içinde | E-16 · E-19 | eşlendi |
| `03 §1.11.11` | Geri ödeme IBAN'ını girmek — havale hattında | E-16 · E-17 · E-18 · ortak bileşen: IBAN | eşlendi |
| `03 §1.11.12` | Girilmiş IBAN'ı düzeltmek — geri ödeme işlenene kadar | E-16 · ortak bileşen: IBAN | eşlendi |
| `03 §1.11.13` | Havale bilgisini görmek — donmuş IBAN, Alındı + Bekliyor | E-14 · E-16 | eşlendi |
| `03 §1.11.14` | Dijital dosyayı indirmek — indirme hakkı kaldıkça | E-16 | eşlendi |

**§2.1 Ziyaretçi: vitrinde gezinme ve ürünü bulma (11)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §2.1.1` | Ana sayfa: firmanın seçtiği düzen; duyuru şeridi her sayfanın üstünde | E-01 · ortak bileşen: çerçeve | eşlendi |
| `03 §2.1.2` | Menüden kategoriye giriş; kategori sayfası alt dallarla tek liste, kırıntı yolu | E-02 · ortak bileşen: çerçeve · ortak bileşen: kart | eşlendi |
| `03 §2.1.3` | Ürün adıyla arama; sonuç kategori sayfasının düzeniyle | E-03 · ortak bileşen: çerçeve | eşlendi |
| `03 §2.1.4` | Listeyi süzme: fiyat aralığı ve stok durumu | E-02 · E-03 | eşlendi |
| `03 §2.1.5` | Ürün kartından ürün sayfası: görsel, fiyat, üretim yeri, indirim, kargoya verme süresi | E-04 · ortak bileşen: kart | eşlendi |
| `03 §2.1.6` | Varyant seçimi; tükenmiş ve açılmamış kombinasyon görünür, seçilemez | E-04 | eşlendi |
| `03 §2.1.7` | Bütün varyantları tükenmiş ürün "Tükendi" işaretiyle kalır | E-04 · ortak bileşen: kart | eşlendi |
| `03 §2.1.8` | Varyantı adediyle sepete ekleme | E-04 · E-12 | eşlendi |
| `03 §2.1.9` | Satış kapalıyken gezme: ürünler görünür, sepete ekleme ve ödeme kapalı | E-04 · E-12 · ortak bileşen: satış-kapalı | eşlendi |
| `03 §2.1.10` | Arşivlenmiş ürünün adresi: "Bu ürün artık satılmıyor" sayfası | E-05 | eşlendi |
| `03 §2.1.11` | Taslak ya da silinmiş ürünün adresi: "Sayfa bulunamadı"; yöneticiye "Taslak" bandı | E-06 · E-04 · ortak bileşen: taslak | eşlendi |

**§2.2 Ziyaretçi: kurumsal içerik ve iletişim formu (10)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §2.2.1` | Ana sayfanın kurumsal blokları: Hakkımızda, işaretli hizmet ve referanslar | E-01 | eşlendi |
| `03 §2.2.2` | Menüden içerik tipine giriş; liste sayfaları elle sırayla | E-07 · ortak bileşen: çerçeve | eşlendi |
| `03 §2.2.3` | İçerik sayfası; SSS tek sayfa; hizmet tanıtımında "Bize ulaşın"; video bağlantısı | E-08 · E-09 | eşlendi |
| `03 §2.2.4` | "İlgili ürünler"den ürün sayfasına geçiş | E-08 · ortak bileşen: kart | eşlendi |
| `03 §2.2.5` | İletişim sayfası: iletişim ve yasal kimlik bilgileri, şubeler, form | E-10 | eşlendi |
| `03 §2.2.6` | Her sayfanın üst ve alt bölümü: sosyal bağlantılar, altbilgi bağlantıları, platform imzası, ETBİS bandı | ortak bileşen: çerçeve | eşlendi |
| `03 §2.2.7` | İletişim formunu açma; aydınlatma kapısı; üyede ön dolu alanlar | E-10 | eşlendi |
| `03 §2.2.8` | Formu doldurup gönderme; kapalı beş konu tipi | E-10 | eşlendi |
| `03 §2.2.9` | Cevabı bekleme: gönderene numara ve takip sayfası verilmez | E-10 · E-39 | eşlendi |
| `03 §2.2.10` | Taslak ya da silinmiş içeriğin adresi: "Sayfa bulunamadı" | E-06 | eşlendi |

**§2.3 Müşteri: sepet (8)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §2.3.1` | Sepete ekleme; stok ve "bir siparişte en fazla" sınırı mesajları | E-04 · E-12 · ortak bileşen: mesaj | eşlendi |
| `03 §2.3.2` | Sepeti açma; canlı fiyat, "fiyatı değişti" işareti | E-12 | eşlendi |
| `03 §2.3.3` | Adet değiştirme ve kalem çıkarma | E-12 | eşlendi |
| `03 §2.3.4` | Satın alınamaz kalem sepette kalır: "Tükendi", "Bu adette stok yok", sınır mesajı | E-12 | eşlendi |
| `03 §2.3.5` | Vitrinden kalkan kalem sepetten çıkar, bir kez söylenir | E-12 · ortak bileşen: mesaj | eşlendi |
| `03 §2.3.6` | Girişte iki sepetin birleşmesi; eklenen ürünler bir kez adıyla söylenir | E-12 · E-22 · ortak bileşen: mesaj | eşlendi |
| `03 §2.3.7` | Çıkışta sepet hesapla gider, tarayıcıda boş görünür | E-12 · ortak bileşen: çerçeve | eşlendi |
| `03 §2.3.8` | Ödeme adımına geçiş; asgari sipariş tutarı ve satış kapısı denetimi | E-12 · E-13 | eşlendi |

**§2.4 Müşteri: ödeme adımı ve sipariş onayı (9)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §2.4.1` | Ödeme adımının açılışı; misafirin e-postası; "bu e-posta kayıtlı" giriş hatırlatması | E-13 · E-22 (GAP-4) | eşlendi |
| `03 §2.4.2` | Teslimat adresi: defterden seçim ya da yeni adres, deftere kaydetme | E-13 · ortak bileşen: adres | eşlendi |
| `03 §2.4.3` | Fatura adresi: varsayılan aynı, "fatura adresim farklı" | E-13 · ortak bileşen: adres | eşlendi |
| `03 §2.4.4` | Kupon kodu girme, değiştirme, kaldırma | E-13 | eşlendi |
| `03 §2.4.5` | Ödeme yöntemi seçimi; kart "şu an kullanılamıyor", havale tavanı (L-8) | E-13 | eşlendi |
| `03 §2.4.6` | Onay özeti: kalemler, döküm, toplam, misafirin e-postası, Ön Bilgilendirme Formu | E-13 | eşlendi |
| `03 §2.4.7` | Onay kutuları: iki kutu; dijital ve hizmet kaleminde ek kutular | E-13 | eşlendi |
| `03 §2.4.8` | "Siparişi onayla — ödeme yükümlülüğü doğar"; onay anında yeniden değerlendirme | E-13 · E-12 | eşlendi |
| `03 §2.4.9` | Sipariş oluşur: numara, donma, ayırma | E-14 · E-27 · ekran dışı — arka plan | eşlendi |

**§2.5 Müşteri: ödeme ve teslim (17)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §2.5.1.1` | Kart bilgisi sağlayıcının sayfasında ya da çerçevesinde girilir | ekran dışı — ürünün dışında | ekran dışı |
| `03 §2.5.1.2` | 3D Secure sonrası dönüş sipariş sayfasına; ödeme bekleniyor hâli | E-16 | eşlendi |
| `03 §2.5.1.3` | Kart reddedilir: sipariş sayfası ödemenin gerçekleşmediğini söyler; sepet durur | E-16 · E-12 · E-27 · E-36 | eşlendi |
| `03 §2.5.1.4` | Ödeme yarıda kalır; Z-7 dolunca son sorgu | E-16 · E-12 · E-36 · E-37 · ortak bileşen: rozet · ekran dışı — arka plan | eşlendi |
| `03 §2.5.2.1` | Havaleyle onay: ekranda donmuş IBAN ve sipariş numarası | E-14 · E-16 | eşlendi |
| `03 §2.5.2.2` | Müşteri bankasından havale yapar | ekran dışı — ürünün dışında | ekran dışı |
| `03 §2.5.2.3` | Havale ödeme hatırlatması (B-3) | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §2.5.2.4` | Yönetici "ödendi" işaretler; panel donmuş IBAN'ı gösterir | E-37 | eşlendi |
| `03 §2.5.2.5` | Süre dolunca kendiliğinden iptal | E-16 · ekran dışı — arka plan | eşlendi |
| `03 §2.5.2.6` | Ödeme beklenirken vazgeçme: sipariş bütünüyle iptal | E-16 · E-17 | eşlendi |
| `03 §2.5.3.1` | Ödeme onayının işlenmesi: stok düşer, süreler başlar, dijital teslim | E-16 · E-12 · ekran dışı — arka plan | eşlendi |
| `03 §2.5.3.2` | Dijital dosyanın sipariş sayfasından indirilmesi; hak dolunca iletişim formu | E-16 · E-10 · E-37 | eşlendi |
| `03 §2.5.3.3` | Kargoya verme: sipariş sayfasında şirket, takip numarası, "Takip et" | E-37 · E-16 | eşlendi |
| `03 §2.5.3.4` | "Teslim edilemedi": gönderi firmaya döner | E-37 · E-16 | eşlendi |
| `03 §2.5.3.5` | Teslim işareti ve teslim tarihi; cayma penceresi başlar | E-37 · E-16 | eşlendi |
| `03 §2.5.3.6` | Hizmet kalemi "tamamlandı" işareti | E-37 · E-16 | eşlendi |
| `03 §2.5.3.7` | Fiziksel kalemsiz siparişin izlenmesi: Alındı → Teslim edildi | E-16 | eşlendi |

**§2.6 Müşteri: sipariş takibi (7)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §2.6.1` | Üye sipariş geçmişinden siparişi açar | E-27 · E-16 | eşlendi |
| `03 §2.6.2` | Misafir alıcı sipariş numarası ve e-postayla sorgular (L-5) | E-15 · E-16 | eşlendi |
| `03 §2.6.3` | Sipariş e-postasındaki erişim anahtarlı bağlantı | E-16 | eşlendi |
| `03 §2.6.4` | Sipariş sayfasının içeriği: iki eksen, kalemler, adresler, takip, indirme, IBAN, metin sürümleri, açık işlemler | E-16 | eşlendi |
| `03 §2.6.5` | E-posta ulaşmaz: bilgi sipariş sayfasında; panelde "e-posta ulaşmadı" | E-16 · E-36 · E-37 | eşlendi |
| `03 §2.6.6` | Hesap silinir ya da e-posta değişir: sipariş misafir yolundan sürer | E-15 · E-16 | eşlendi |
| `03 §2.6.7` | Misafir e-postasını yanlış yazmış: firmaya ulaşır, yönetici düzeltir | E-10 · E-37 | eşlendi |

**§2.7 Müşteri: iptal ve gecikme feshi (8)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §2.7.1` | Ödeme beklenirken siparişin bütünüyle iptali | E-16 · E-17 · E-27 · ortak bileşen: onay (GAP-1) | eşlendi |
| `03 §2.7.2` | Ödenmiş siparişte fiziksel kalem iptali; havalede IBAN girişi | E-16 · E-17 · E-37 | eşlendi |
| `03 §2.7.3` | Ödenmiş siparişte hizmet kalemi iptali | E-16 · E-17 · E-37 | eşlendi |
| `03 §2.7.4` | Ödenmiş dijital kalemin iptali yoktur | E-16 | eşlendi |
| `03 §2.7.5` | Geri ödeme IBAN'ının girilmesi ve düzeltilmesi; aydınlatma bağlantısı | E-16 · E-17 · E-18 · ortak bileşen: IBAN | eşlendi |
| `03 §2.7.6` | İptalin geri ödemesi: kartta kendiliğinden, havalede yönetici | E-37 · E-16 · ekran dışı — arka plan | eşlendi |
| `03 §2.7.7` | Gecikme nedeniyle fesih; havalede IBAN | E-16 · E-17 · E-37 | eşlendi |
| `03 §2.7.8` | Feshin geri ödemesi; kargodaki mal firmaya döner | E-37 · E-16 | eşlendi |

**§2.8 Müşteri: cayma ve iade (15)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §2.8.1.1` | Cayma düğmesinin görünürlüğü; koşullu istisnada koşul metni | E-16 · E-18 | eşlendi |
| `03 §2.8.1.2` | Cayma beyanı: kalem seçimi, iade adresi, gönderme yükümlülüğü, IBAN | E-18 · E-37 · ortak bileşen: IBAN (GAP-1) | eşlendi |
| `03 §2.8.1.3` | Teslim beyanla aynı güne işaretlenir: panel sırayı sorar | E-37 | eşlendi |
| `03 §2.8.1.4` | Müşteri malı karşı ödemeli gönderir | ekran dışı — ürünün dışında | ekran dışı |
| `03 §2.8.1.5` | İade malının teslim alınması ve ulaşma tarihi | E-37 · E-16 | eşlendi |
| `03 §2.8.1.6` | Koruyucu ambalajı açılmış mal: iade reddi | E-37 · E-16 | eşlendi |
| `03 §2.8.1.7` | Kullanılmış ya da hasarlı dönen mal: ret ve kesinti yoktur | E-37 | eşlendi |
| `03 §2.8.1.8` | Mal hiç gönderilmez: "iade malı bekleniyor", "mal dönmedi" kapanışı | E-37 · E-16 | eşlendi |
| `03 §2.8.2.1` | Hizmet kaleminin cayma düğmesinin görünürlüğü | E-16 | eşlendi |
| `03 §2.8.2.2` | Hizmet kaleminden cayma: iade adresi gösterilmez; IBAN | E-18 | eşlendi |
| `03 §2.8.3.1` | Dijital kalemden cayma yoktur | E-16 | eşlendi |
| `03 §2.8.4.1` | Caymanın geri ödemesinin işlenmesi | E-37 · E-16 | eşlendi |
| `03 §2.8.4.2` | Geri ödeme havalesi gerçekleşmez: IBAN alanı yeniden açılır | E-37 · E-16 | eşlendi |
| `03 §2.8.4.3` | Kart iadesi gerçekleşmez: havale yolu açılır, sipariş sayfası söyler | E-37 · E-16 | eşlendi |
| `03 §2.8.4.4` | Başka kanaldan cayma bildirimi: yönetici kaydeder; IBAN alanı | E-37 · E-16 | eşlendi |

**§2.9 Müşteri: ayıp talebi (6)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §2.9.1` | "Sorun bildir"in görünürlüğü ve süresi (Z-18) | E-16 · E-10 | eşlendi |
| `03 §2.9.2` | Kalem seçimi ve sorunun açıklaması; iade adresi; aydınlatma bağlantısı | E-19 · E-38 · E-37 | eşlendi |
| `03 §2.9.3` | Talep açıkken sipariş sayfası talebin durumunu gösterir | E-16 | eşlendi |
| `03 §2.9.4` | Çözüm sistemin dışında yürür | ekran dışı — ürünün dışında · E-37 · E-16 | eşlendi |
| `03 §2.9.5` | Yönetici talebi "çözüldü" işaretler | E-38 · E-37 · E-16 | eşlendi |
| `03 §2.9.6` | Müşteri talebi yeniden açar | E-16 · E-19 | eşlendi |

**§2.10 Uçtan uca anlatılar — 2.10.1'in adım tablosu (8)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §2.10.1.1` | Ürünü bulma ve varyant seçimi (2.1.2–2.1.6) | E-02 · E-03 · E-04 | eşlendi |
| `03 §2.10.1.2` | Sepete ekleme (2.3.1) | E-04 · E-12 | eşlendi |
| `03 §2.10.1.3` | Ödeme adımında adres, kupon, yöntem (2.4.1–2.4.5) | E-13 | eşlendi |
| `03 §2.10.1.4` | Özet, form, iki kutu ve onay (2.4.6–2.4.8) | E-13 | eşlendi |
| `03 §2.10.1.5` | Sipariş oluşur (2.4.9) | E-14 | eşlendi |
| `03 §2.10.1.6` | Ödeme: kartta 3D Secure, havalede "ödendi" | E-14 · E-16 · E-37 | eşlendi |
| `03 §2.10.1.7` | Hazırlama ve kargoya verme | E-37 · E-16 | eşlendi |
| `03 §2.10.1.8` | Teslim işareti ve tarihi | E-37 · E-16 | eşlendi |

**§3.1 Bütün akışlara uygulanan ilkeler (6)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §3.1.1` | Hiçbir akış bir e-postanın ulaşmasına bağlı değildir | E-16 · E-20 · E-23 · E-36 · E-37 · E-49 | eşlendi |
| `03 §3.1.2` | Sistem müşterinin ödediği siparişi kendiliğinden iptal etmez | E-16 · E-37 · ekran dışı — arka plan | eşlendi |
| `03 §3.1.3` | Onaylanan özet ile oluşan sipariş birebir aynıdır | E-13 | eşlendi |
| `03 §3.1.4` | Sepet hatalarda korunur | E-12 | eşlendi |
| `03 §3.1.5` | Limit aşımı nötr konuşur | ortak bileşen: mesaj | eşlendi |
| `03 §3.1.6` | Hata ve uyarının anlamı yalnız renkle taşınmaz; biçimi `04`'ün işi | ortak bileşen: mesaj · ortak bileşen: taban | eşlendi |

**§3.2.1 Satın alma hataları (22)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §3.2.1.1` | 3D Secure başarısız ya da kart reddi | E-16 · E-12 · E-27 · E-36 | eşlendi |
| `03 §3.2.1.2` | Banka ekranı kapanır ya da ödeme yarıda kalır | E-16 · E-12 | eşlendi |
| `03 §3.2.1.3` | Onay anında özet değişmiş: güncel özet, yeniden onay | E-13 | eşlendi |
| `03 §3.2.1.4` | Sepetteki kalem tükenmiş ya da kontenjan dolmuş | E-12 · E-13 | eşlendi |
| `03 §3.2.1.5` | Sepetteki ürün arşive ya da taslağa alınmış, silinmiş | E-12 | eşlendi |
| `03 §3.2.1.6` | Stoktan fazla adet: "Bu adette stok yok" | E-04 · E-12 | eşlendi |
| `03 §3.2.1.7` | "Bir siparişte en fazla" sınırı aşılır | E-04 · E-12 | eşlendi |
| `03 §3.2.1.8` | Aynı dijital varyant ikinci kez: "Bu ürün zaten sepetinde" | E-04 · E-12 | eşlendi |
| `03 §3.2.1.9` | Önceden alınmış dijital varyant: "Bu ürünü daha önce aldınız" | E-04 · E-12 | eşlendi |
| `03 §3.2.1.10` | Teslimat yapılmayan il: adres kabul edilmez | E-13 · ortak bileşen: adres | eşlendi |
| `03 §3.2.1.11` | Asgari sipariş tutarının altı: eksik tutar gösterilir | E-12 | eşlendi |
| `03 §3.2.1.12` | Kupon kabul edilmez | E-13 | eşlendi |
| `03 §3.2.1.13` | Misafir e-postasını yanlış yazar: özet gösterir, onaydan sonra firma düzeltir | E-13 · E-10 · E-37 | eşlendi |
| `03 §3.2.1.14` | Hesabı olan kişi misafir sipariş verir: giriş hatırlatması | E-13 | eşlendi |
| `03 §3.2.1.15` | Ödeme sağlayıcısına erişilemez | E-13 · E-12 · E-27 | eşlendi |
| `03 §3.2.1.16` | İptal edilmiş siparişe geç gelen ödeme: kendiliğinden geri ödeme | E-16 · E-37 · ekran dışı — arka plan | eşlendi |
| `03 §3.2.1.17` | Satış kapalıdır | E-04 · E-12 · ortak bileşen: satış-kapalı | eşlendi |
| `03 §3.2.1.18` | Art arda geçersiz kupon (L-6): kupon alanı kapanır | E-13 · ortak bileşen: mesaj | eşlendi |
| `03 §3.2.1.19` | L-7 eşiği: yeni sipariş onaylanamaz | E-13 · E-36 · ortak bileşen: mesaj | eşlendi |
| `03 §3.2.1.20` | Sipariş notu alanı yoktur; yol iletişim formu | E-13 · E-10 | eşlendi |
| `03 §3.2.1.21` | L-8 doluyken havale seçilemez | E-13 · ortak bileşen: mesaj | eşlendi |
| `03 §3.2.1.22` | Harcama itirazı (ters ibraz): ürün bilmez | ekran dışı — ürünün dışında | ekran dışı |

**§3.2.2 Sipariş takibi hataları (6)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §3.2.2.1` | Sipariş e-postası ulaşmaz: bilgi sipariş sayfasında | E-15 · E-16 · E-37 | eşlendi |
| `03 §3.2.2.2` | Yanlış numara ya da e-posta: sayfa açılmaz, L-5 | E-15 · ortak bileşen: mesaj | eşlendi |
| `03 §3.2.2.3` | Hesap silinmiş, sipariş yürüyor: misafir yolu | E-15 · E-16 | eşlendi |
| `03 §3.2.2.4` | Kendiliğinden iptal edilmiş ödenmemiş sipariş listede görünmez | E-27 · E-36 | eşlendi |
| `03 §3.2.2.5` | Misafir siparişinin e-postası sahibi istemeden düzeltilir: B-15, yeniden düzeltme | E-37 · ekran dışı — e-posta (`08`) | eşlendi |
| `03 §3.2.2.6` | Teslimat adresi sahibi istemeden düzeltilir: B-10, geri düzeltme | E-37 · ekran dışı — e-posta (`08`) | eşlendi |

**§3.3 İptal, cayma ve iade hataları (36)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §3.3.1` | Kargodan sonra iptal istenir: iptal düğmesi yok, yol cayma | E-16 | eşlendi |
| `03 §3.3.2` | Z-10 ya da Z-11 aşılır: iptal ve fesih düğmeleri; panelde kanuni faiz uyarısı | E-16 · E-17 · E-37 | eşlendi |
| `03 §3.3.3` | Ödenmiş dijital kalemde iptal ve cayma düğmesi sunulmaz | E-16 | eşlendi |
| `03 §3.3.4` | Cayma istisnalı kalem: mutlakta düğme yok, koşulluda koşul metni | E-16 · E-18 · E-37 | eşlendi |
| `03 §3.3.5` | Tamamlanmış hizmetten cayma: düğme sunulmaz | E-16 | eşlendi |
| `03 §3.3.6` | Cayma penceresi geçmiş: düğme kapanır | E-16 | eşlendi |
| `03 §3.3.7` | Havale geri ödemesinde IBAN beyanla girilir; öteki hâllerde istenir | E-16 · E-17 · E-18 · ortak bileşen: IBAN | eşlendi |
| `03 §3.3.8` | Geri ödeme süresi mal ulaşana kadar başlamaz; panel kalan süreyi gösterir | E-37 · ortak bileşen: süre | eşlendi |
| `03 §3.3.9` | Müşteri kaynaklı teslim edilememe: gidiş kargosu ödenmez | E-37 | eşlendi |
| `03 §3.3.10` | Kısmi iptalden sonra kupon ve kargo yeniden hesaplanmaz | E-16 · E-37 | eşlendi |
| `03 §3.3.11` | Kargodayken vazgeçme: cayma beyanı açık | E-16 · E-18 | eşlendi |
| `03 §3.3.12` | Geç işaretlenen teslimde pencere dolmuş: düğme kapalı | E-16 | eşlendi |
| `03 §3.3.13` | Tek kalemden caymada kargo ücreti geri ödemeye girmez | E-18 · E-37 | eşlendi |
| `03 §3.3.14` | İade kargosu karşı ödemeli gönderilir | E-18 · ekran dışı — ürünün dışında | eşlendi |
| `03 §3.3.15` | Çözülmüş ayıp tekrarlar: talep yeniden açılır | E-16 · E-19 · E-38 · E-37 | eşlendi |
| `03 §3.3.16` | Mal hiç gönderilmez: "iade malı bekleniyor", "mal dönmedi" kapanışı | E-37 | eşlendi |
| `03 §3.3.17` | Teslim alma işareti gecikir: tarih geçmişe dönük girilir | E-37 | eşlendi |
| `03 §3.3.18` | "Mal dönmedi"den sonra mal ulaşır: kalem yeniden açılır | E-37 | eşlendi |
| `03 §3.3.19` | IBAN silindikten sonra mal ulaşır: sipariş sayfasında IBAN alanı açılır | E-16 · E-37 · ortak bileşen: IBAN | eşlendi |
| `03 §3.3.20` | IBAN geç girilir: süre durmaz, alan e-postadan bağımsız görünür | E-16 | eşlendi |
| `03 §3.3.21` | Tamamlanmamış hizmet kaleminden cayma: kalem kapanır | E-18 · E-16 | eşlendi |
| `03 §3.3.22` | Kargodayken cayılan gönderi firmaya döner: teslim alma adımı | E-37 | eşlendi |
| `03 §3.3.23` | Ödeme onayından önce kalem iptali sunulmaz | E-16 · E-17 | eşlendi |
| `03 §3.3.24` | Kart geri ödemesi başarısız: "geri ödeme gerçekleşmedi" işareti | E-37 · E-31 | eşlendi |
| `03 §3.3.25` | Başka kanaldan cayma bildirimi: yönetici teyit eder ve kaydeder | E-37 · E-16 | eşlendi |
| `03 §3.3.26` | Yeniden istenen IBAN hiç girilmez: "IBAN bekleniyor" listesi | E-31 · E-37 | eşlendi |
| `03 §3.3.27` | Yanlış IBAN ya da reddedilen havale: düzeltme, "havale gerçekleşmedi" | E-16 · E-37 · ortak bileşen: IBAN | eşlendi |
| `03 §3.3.28` | Kısmen ifa edilmiş hizmetten cayma: tam geri ödeme | E-18 · E-37 | eşlendi |
| `03 §3.3.29` | Fiziksel kalemler teslimden önce kapanır: hizmetin düğmesi sürer | E-16 | eşlendi |
| `03 §3.3.30` | İade gönderisi yolda kaybolur: ürünün dışında | ekran dışı — ürünün dışında | ekran dışı |
| `03 §3.3.31` | Kargodan önce başka kanaldan cayma: "müşteriyle anlaşıldı" iptali | E-37 | eşlendi |
| `03 §3.3.32` | Pencere dışı ya da mutlak istisnada cayma isteği: düğme açılmaz | E-16 · E-37 | eşlendi |
| `03 §3.3.33` | Sahte cayma bildirimi: siparişin kanalından teyit | E-37 | eşlendi |
| `03 §3.3.34` | Beyan ve teslim aynı gün: panel sırayı sorar, beyan saatini gösterir | E-37 | eşlendi |
| `03 §3.3.35` | Ambalajı açılmış mal: iade reddi seçimi | E-37 | eşlendi |
| `03 §3.3.36` | Kullanılmış mal: ret ve kesinti yoktur | E-37 | eşlendi |

**§3.4 Üyelik hataları (17)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §3.4.1` | Doğrulama bağlantısının süresi dolmuş: "bağlantı geçersiz, yeniden kayıt olun" | E-21 | eşlendi |
| `03 §3.4.2` | Doğrulamadan giriş denemesi: beklendiği söylenir, yeniden isteme yolu | E-22 · E-20 | eşlendi |
| `03 §3.4.3` | Kayıtta e-posta zaten kayıtlı | E-20 | eşlendi |
| `03 §3.4.4` | Sıfırlamada kayıtlı olmayan e-posta: nötr mesaj | E-23 · ortak bileşen: mesaj | eşlendi |
| `03 §3.4.5` | Sıfırlama bağlantısı kullanılmış ya da süresi dolmuş | E-23 | eşlendi |
| `03 §3.4.6` | Zayıf ya da yaygın şifre reddedilir | E-20 · E-23 · E-25 | eşlendi |
| `03 §3.4.7` | Google e-postayı doğrulanmamış verir: kayda yönlendirme | E-22 · E-20 | eşlendi |
| `03 §3.4.8` | Şifresiz Google hesabında "şifremi unuttum" şifre belirler | E-23 | eşlendi |
| `03 §3.4.9` | Yeni e-posta doğrulanmaz: değişiklik geçerli olmaz | E-25 (GAP-3) | eşlendi |
| `03 §3.4.10` | Yürüyen siparişle hesap silme engellenmez | E-25 | eşlendi |
| `03 §3.4.11` | L-1, L-2, L-3 aşımı: geçici engel, nötr mesaj | E-20 · E-22 · E-23 · ortak bileşen: mesaj | eşlendi |
| `03 §3.4.12` | Aydınlatma metni tamamlanmamış: kayıt ve ilk Google girişi kapalı | E-20 · E-22 | eşlendi |
| `03 §3.4.13` | E-posta sahibinin haberi olmadan değişir: geri alma bağlantısı | E-28 · ekran dışı — e-posta (`08`) | eşlendi |
| `03 §3.4.14` | Doğrulama bağlantısı defalarca istenir (L-9) | E-20 · ortak bileşen: mesaj | eşlendi |
| `03 §3.4.15` | Geri alma bağlantısı açıkken hesap silinemez: sebep ve bitiş tarihi | E-25 · E-40 | eşlendi |
| `03 §3.4.16` | Google e-postası bekleyen kayda eşleşir: kayıt devralınmaz | E-22 | eşlendi |
| `03 §3.4.17` | Yeniden doğrulamada art arda yanlış şifre (L-1) | E-24 · ortak bileşen: mesaj | eşlendi |

**§4.1 Süre envanterinden zaman aşımı akışları (47)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §4.1.1` | Z-1 doğrulama bağlantısının ömrü | E-21 · E-20 · E-25 · ekran dışı — arka plan | eşlendi |
| `03 §4.1.2` | Z-2 şifre sıfırlama bağlantısının ömrü | E-23 · ekran dışı — arka plan | eşlendi |
| `03 §4.1.3` | Z-3 yönetici daveti bağlantısının ömrü | E-30 · E-49 | eşlendi |
| `03 §4.1.4` | Z-4 kısa oturum: oturum kapanır | E-22 · ekran dışı — arka plan | eşlendi |
| `03 §4.1.5` | Z-5 uzun oturum — "beni hatırla" | E-22 · ekran dışı — arka plan | eşlendi |
| `03 §4.1.6` | Z-6 sepetin ömrü: dolmaz | E-12 | eşlendi |
| `03 §4.1.7` | Z-7 kart ödeme süresi: son sorgu, kendiliğinden iptal | E-16 · E-36 · E-37 · ortak bileşen: rozet · ekran dışı — arka plan | eşlendi |
| `03 §4.1.8` | Z-8 havale ödeme süresi: kendiliğinden iptal | E-16 · ortak bileşen: süre · ekran dışı — arka plan | eşlendi |
| `03 §4.1.9` | Z-9 havale ödeme hatırlatması (B-3) | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §4.1.10` | Z-10 kargoya verme süresi: iptal düğmesi açık, panelde faiz uyarısı | E-04 · E-16 · E-37 | eşlendi |
| `03 §4.1.11` | Z-11 yasal teslim üst sınırı: gecikme feshi düğmesi açılır | E-16 | eşlendi |
| `03 §4.1.12` | Z-12 teslim tarihi girilmezse sipariş Kargoya verildi'de kalır | E-37 · E-16 | eşlendi |
| `03 §4.1.13` | Z-39 ifa süresi: iptal düğmesi açık, panelde faiz uyarısı | E-16 · E-37 | eşlendi |
| `03 §4.1.14` | Z-13 cayma penceresi — fiziksel: düğme kapanır | E-16 · ortak bileşen: süre | eşlendi |
| `03 §4.1.15` | Z-14 cayma penceresi — hizmet | E-16 | eşlendi |
| `03 §4.1.16` | Z-15 dijital kalemde hak üçüncü kutuyla düşer | E-13 | eşlendi |
| `03 §4.1.17` | Z-16 caymanın geri ödeme süresi | E-37 · ortak bileşen: süre | eşlendi |
| `03 §4.1.18` | Z-42 iade malını gönderme süresi: "mal dönmedi" kapanışı açılır | E-18 · E-37 | eşlendi |
| `03 §4.1.19` | Z-17 iptalin geri ödeme süresi | E-37 · ortak bileşen: süre | eşlendi |
| `03 §4.1.20` | Z-47 ifanın imkânsızlaştığının bildirimi: sayaç tutulmaz | E-37 · ekran dışı — e-posta (`08`) | eşlendi |
| `03 §4.1.21` | Z-18 ayıp talebinin süresi: kanal kapanır | E-16 · E-10 | eşlendi |
| `03 §4.1.22` | Z-19 indirme hakkı: adet dolunca indirme kapanır | E-16 · E-37 | eşlendi |
| `03 §4.1.23` | Z-20 indirimin tarih aralığı | E-33 · E-04 · ekran dışı — arka plan | eşlendi |
| `03 §4.1.24` | Z-21 referans fiyatın geriye bakış penceresi | ekran dışı — arka plan | ekran dışı |
| `03 §4.1.25` | Z-22 kuponun tarih aralığı | E-35 · E-13 | eşlendi |
| `03 §4.1.26` | Z-23 duyurunun tarih aralığı | ortak bileşen: çerçeve · E-41 | eşlendi |
| `03 §4.1.27` | Z-24 ileri tarihli yayın | E-33 · E-41 | eşlendi |
| `03 §4.1.28` | Z-25 KVKK başvurusuna cevap: sayaç tutulmaz | ekran dışı — ürünün dışında | ekran dışı |
| `03 §4.1.29` | Z-26 e-postanın yeniden denenmesi: "e-posta ulaşmadı" işareti | E-36 · E-37 · E-31 · E-49 · ekran dışı — arka plan | eşlendi |
| `03 §4.1.30` | Z-27 deneme limitlerinin penceresi | ortak bileşen: mesaj · ekran dışı — arka plan | eşlendi |
| `03 §4.1.31` | Z-28 sipariş verisinin periyodik imhası | ekran dışı — arka plan | ekran dışı |
| `03 §4.1.32` | Z-41 ödemesi alınmamış siparişin kişisel verisinin imhası | ekran dışı — arka plan | ekran dışı |
| `03 §4.1.33` | Z-29 ayıp talebi kaydının imhası | ekran dışı — arka plan | ekran dışı |
| `03 §4.1.34` | Z-30 işlem izinin imhası | E-52 · ekran dışı — arka plan | eşlendi |
| `03 §4.1.35` | Z-31 hesap verisi silmeyle birlikte silinir | E-25 · ekran dışı — arka plan | eşlendi |
| `03 §4.1.36` | Z-32 sepet hesapla birlikte silinir | ekran dışı — arka plan | ekran dışı |
| `03 §4.1.37` | Z-33 iletişim talebinin silinmesi | E-39 · E-38 · ekran dışı — arka plan | eşlendi |
| `03 §4.1.38` | Z-34 bildirim gönderim kaydı — hesap, yönetim, firma | ekran dışı — arka plan | ekran dışı |
| `03 §4.1.39` | Z-43 bildirim gönderim kaydı — B-1…B-16 | ekran dışı — arka plan | ekran dışı |
| `03 §4.1.40` | Z-35 geri ödeme IBAN'ının silinmesi | E-16 · ekran dışı — arka plan | eşlendi |
| `03 §4.1.41` | Z-36 periyodik imha: panelde düğme yoktur | ekran dışı — arka plan | ekran dışı |
| `03 §4.1.42` | Z-40 giriş kaydının imhası | ekran dışı — arka plan | ekran dışı |
| `03 §4.1.43` | Z-44 imha kaydının silinmesi | ekran dışı — arka plan | ekran dışı |
| `03 §4.1.44` | Z-45 geri alma bağlantısının ömrü | E-28 · E-25 | eşlendi |
| `03 §4.1.45` | Z-46 tanınan tarayıcı işareti | ekran dışı — arka plan | ekran dışı |
| `03 §4.1.46` | Z-37 veri ihlalinde Kurul'a bildirim: sayaç tutulmaz | ekran dışı — ürünün dışında | ekran dışı |
| `03 §4.1.47` | Z-38 malı dönmeyen caymada IBAN'ın kendiliğinden silinmesi | E-16 · E-37 · ekran dışı — arka plan | eşlendi |

**§4.2 Kendiliğinden işleyen anlar (12)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §4.2.1` | Sipariş onayı: numara, donma, ayırma | E-14 · ekran dışı — arka plan | eşlendi |
| `03 §4.2.2` | Ödeme onayı: kesin düşme, süreler, dijital teslim | E-16 · E-12 · ekran dışı — arka plan | eşlendi |
| `03 §4.2.3` | Kendiliğinden iptal | E-16 · ekran dışı — arka plan | eşlendi |
| `03 §4.2.4` | İptal, çıkarma ve feshte stok ve kupon dönüşü | ekran dışı — arka plan | ekran dışı |
| `03 §4.2.5` | İadede stok ve kontenjan dönüşü | E-37 · ekran dışı — arka plan | eşlendi |
| `03 §4.2.6` | Kısmi iptal ve iadede kupon ve kargo | ekran dışı — arka plan | ekran dışı |
| `03 §4.2.7` | Kart hattında kendiliğinden geri ödeme; onay penceresi yoktur | E-37 · ekran dışı — arka plan | eşlendi |
| `03 §4.2.8` | Malı dönmeyen caymada IBAN'ın silinmesi | ekran dışı — arka plan | ekran dışı |
| `03 §4.2.9` | Periyodik imha ve imha kaydı | ekran dışı — arka plan | ekran dışı |
| `03 §4.2.10` | E-postanın yeniden denenmesi; panel ana sayfasında kanal uyarısı | E-31 · ekran dışı — arka plan | eşlendi |
| `03 §4.2.11` | Sitenin kesintisinde süreler işler | ekran dışı — arka plan | ekran dışı |
| `03 §4.2.12` | Havale süresi kesintide dolarsa iptal ertelenir | E-37 · E-16 · ekran dışı — arka plan | eşlendi |

**§5.1 Üç yol ve itirazın yeri (7)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §5.1.1` | Sipariş sayfası üç yolu ayrı düğmelerle sunar: iptal, cayma, ayıp talebi | E-16 | eşlendi |
| `03 §5.1.2` | Üç yola girmeyen konu: iletişim formu, "Sipariş hakkında" | E-10 | eşlendi |
| `03 §5.1.3` | Yönetici talebi okur, cevabı e-postayla verir; işlem panelin var olan işlemiyle | E-39 · E-37 | eşlendi |
| `03 §5.1.4` | Çözülmüş ayıp tekrarlar: talep yeniden açılır; süre dolduysa form | E-16 · E-19 · E-10 | eşlendi |
| `03 §5.1.5` | Müşteri cevabı kabul etmez: uyuşmazlık yolları donmuş formda ve sözleşmede | E-16 · ekran dışı — ürünün dışında | eşlendi |
| `03 §5.1.6` | Merci müşteri lehine karar verir: tutar bazlı geri ödeme, IBAN isteği | E-37 · E-16 | eşlendi |
| `03 §5.1.7` | Yönetici kanıt hazırlar: donmuş metinler, gönderim kaydı, işlem izi | E-37 · E-52 | eşlendi |

**§5.2 Ürünün dışında çözülen anlaşmazlıklar (17)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §5.2.1.1` | Ters ibraz: ürün bilmez | ekran dışı — ürünün dışında | ekran dışı |
| `03 §5.2.1.2` | Yönetici itirazı sağlayıcının panelinde cevaplar; sonucu iç nota yazar | ekran dışı — ürünün dışında · E-37 | eşlendi |
| `03 §5.2.1.3` | Geri ödeme adımından önce ters ibraza bakma uyarısı | E-37 | eşlendi |
| `03 §5.2.1.4` | Ters ibrazlı işlemde kendiliğinden iade: fazla ürünün dışında çözülür | ekran dışı — ürünün dışında | ekran dışı |
| `03 §5.2.2.1` | İade gönderisi ulaşmaz: "iade malı bekleniyor" | E-37 · E-16 | eşlendi |
| `03 §5.2.2.2` | Müşteri kaybı iletişim formundan bildirir; dosya eki yoktur | E-10 | eşlendi |
| `03 §5.2.2.3` | "Mal dönmedi" kapanışı | E-37 | eşlendi |
| `03 §5.2.2.4` | Firma ödemeye karar verir: tutar bazlı geri ödeme, IBAN isteği | E-37 · E-16 | eşlendi |
| `03 §5.2.2.5` | Mal sonradan ulaşır: kalem yeniden açılır | E-37 | eşlendi |
| `03 §5.2.3.1` | Eksik bilgilendirme iddiasıyla cayma isteği: düğme açılmaz | E-16 · E-10 | eşlendi |
| `03 §5.2.3.2` | Yönetici pencere dışı kaydı ayrıca onaylar; panel uyarır | E-37 · ortak bileşen: onay | eşlendi |
| `03 §5.2.3.3` | Kayıttan sonra olağan cayma hattı | E-37 · E-16 | eşlendi |
| `03 §5.2.3.4` | Yönetici kayıt yapmaz: cevap e-postayla | ekran dışı — ürünün dışında | ekran dışı |
| `03 §5.2.4.1` | Teslim tarihine itiraz: düğme açılmaz | E-16 | eşlendi |
| `03 §5.2.4.2` | Müşteri caymayı başka kanaldan bildirir | ekran dışı — ürünün dışında · E-37 · E-10 | eşlendi |
| `03 §5.2.4.3` | Teslim tarihinin düzeltilmesi ya da bildirimin kaydı; sıra sorusu | E-37 | eşlendi |
| `03 §5.2.4.4` | Yönetici tarihi doğru bulur: ürün karar vermez | ekran dışı — ürünün dışında | ekran dışı |

**§5.3 İade reddi ve değer kaybı (9)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §5.3.1.1` | Koşullu istisnalı kalemde beyan ekranı ambalaj koşulunu yazar | E-18 | eşlendi |
| `03 §5.3.1.2` | Teslim alma adımında "iade reddedildi" seçimi ve geri alınamaz onay | E-37 · ortak bileşen: onay | eşlendi |
| `03 §5.3.1.3` | Reddin sonuçları: geri ödeme yok, IBAN silinir, sayaçtan düşer | E-37 · E-16 · E-31 | eşlendi |
| `03 §5.3.1.4` | Mal müşteriye geri gönderilir: sistemin dışında | ekran dışı — ürünün dışında | ekran dışı |
| `03 §5.3.1.5` | Müşteri reddi kabul etmez: iletişim formu ve uyuşmazlık yolu | E-10 · ekran dışı — ürünün dışında | eşlendi |
| `03 §5.3.1.6` | Karardan dönülür: tutar bazlı geri ödeme, IBAN isteği | E-37 · E-16 | eşlendi |
| `03 §5.3.2.1` | Hasarlı dönen malda ret seçeneği sunulmaz | E-37 | eşlendi |
| `03 §5.3.2.2` | Geri ödeme tam işlenir; kesinti alanı yoktur | E-37 | eşlendi |
| `03 §5.3.2.3` | Değer kaybı talebi ürünün dışında; iç not | ekran dışı — ürünün dışında · E-37 | eşlendi |

**§6.1 Deneme limitleri (22)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §6.1.1.1` | L-1 başarısız şifreli giriş ve yeniden doğrulama | E-22 · E-24 · E-29 · ortak bileşen: mesaj | eşlendi |
| `03 §6.1.1.2` | L-2 şifre sıfırlama talebi | E-23 · E-29 · ortak bileşen: mesaj | eşlendi |
| `03 §6.1.1.3` | L-3 hesap kaydı | E-20 · ortak bileşen: mesaj | eşlendi |
| `03 §6.1.1.4` | L-4 iletişim formu gönderimi; tuzak alan sessizce düşer | E-10 · ortak bileşen: mesaj | eşlendi |
| `03 §6.1.1.5` | L-5 başarısız misafir sipariş takibi sorgusu | E-15 · ortak bileşen: mesaj | eşlendi |
| `03 §6.1.1.6` | L-6 geçersiz kupon kodu | E-13 · ortak bileşen: mesaj | eşlendi |
| `03 §6.1.1.7` | L-7 kendiliğinden iptal edilen ödenmemiş sipariş | E-13 · ortak bileşen: mesaj | eşlendi |
| `03 §6.1.1.8` | L-8 aynı anda açık ödenmemiş sipariş | E-13 · ortak bileşen: mesaj | eşlendi |
| `03 §6.1.1.9` | L-9 doğrulama bağlantısının yeniden istenmesi | E-20 · E-25 · ortak bileşen: mesaj | eşlendi |
| `03 §6.1.2.1` | Eşik aşılır: yalnız o işlem geçici engellenir, mesaj nötr | ortak bileşen: mesaj | eşlendi |
| `03 §6.1.2.2` | Engel kendiliğinden açılır | ekran dışı — arka plan | ekran dışı |
| `03 §6.1.2.3` | Yönetici limite takılır: muafiyet yoktur | E-29 · ortak bileşen: mesaj | eşlendi |
| `03 §6.1.2.4` | Ürün düzeyinde limiti olmayan işlem: `05`'in işi | ekran dışı — mimari (`05`) | ekran dışı |
| `03 §6.1.3.1` | Hedefin e-postasıyla şifre denemesi: e-posta ekseni engeller | ekran dışı — arka plan | ekran dışı |
| `03 §6.1.3.2` | Tanınan tarayıcı e-posta ekseninde engellenmez | E-22 · E-29 · ekran dışı — arka plan | eşlendi |
| `03 §6.1.3.3` | Yeni tarayıcıdan giremeyen kullanıcı şifre sıfırlamayı kullanır | E-22 · E-23 | eşlendi |
| `03 §6.1.3.4` | Sıfırlama talepleri de doldurulur (L-2): kalan risk | E-23 · ortak bileşen: mesaj | eşlendi |
| `03 §6.1.3.5` | Google ile giriş L-1 engelinden etkilenmez | E-22 | eşlendi |
| `03 §6.1.4.1` | Ödemeden art arda sipariş: kendiliğinden iptal ve L-7 | ekran dışı — arka plan · E-13 | eşlendi |
| `03 §6.1.4.2` | Havale siparişleriyle stok kilitleme: L-8 | E-13 | eşlendi |
| `03 §6.1.4.3` | Çok IP'den misafir havale siparişi: firmanın incelemesi | E-31 · E-36 | eşlendi |
| `03 §6.1.4.4` | Paylaşılan IP'de gerçek müşteri engellenir: kalan risk | ortak bileşen: mesaj | eşlendi |

**§6.2 Teyit ve kandırılma akışları (21)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §6.2.1.1` | Siparişin iletişim e-postasından gelen talep teyittir | E-37 | eşlendi |
| `03 §6.2.1.2` | Başka kanaldan gelen talep: sipariş bulunur, siparişin kanalına dönülür | E-36 · E-37 | eşlendi |
| `03 §6.2.1.3` | Misafir e-postası düzeltmesinde kimlik siparişteki bilgilerle teyit edilir | E-37 | eşlendi |
| `03 §6.2.1.4` | Sahibine ulaşılamaz: değerlendirme firmanın | ekran dışı — ürünün dışında | ekran dışı |
| `03 §6.2.2.1` | Adres çevirtme girişimi: teyit kalıbı | E-37 | eşlendi |
| `03 §6.2.2.2` | Adres düzeltmesi işlem izine yazılır | E-37 · E-52 | eşlendi |
| `03 §6.2.2.3` | Gerçek sahip B-10'u görür: adres geri düzeltilir | E-37 · ekran dışı — e-posta (`08`) | eşlendi |
| `03 §6.2.3.1` | Sahte cayma ya da iptal bildirimi: teyit | E-37 | eşlendi |
| `03 §6.2.3.2` | Teyit atlanır: bildirimdeki IBAN aktarılmaz | E-37 · E-16 | eşlendi |
| `03 §6.2.3.3` | Yanlış iptal düzeltilir; cayma beyanı geri alınmaz | E-37 | eşlendi |
| `03 §6.2.4.1` | Misafir e-postasını çevirtme girişimi: teyit | E-37 | eşlendi |
| `03 §6.2.4.2` | E-posta düzeltilir: yeni anahtar, "e-posta düzeltildi" işareti | E-37 | eşlendi |
| `03 §6.2.4.3` | Kandıran sipariş sayfasına girer ve işlem yapar | E-16 | eşlendi |
| `03 §6.2.4.4` | Gerçek sahip B-15'i görür: yeniden düzeltme | E-37 · E-16 · ekran dışı — e-posta (`08`) | eşlendi |
| `03 §6.2.5.1` | Hesabın e-postası ele geçirilmiş oturumdan değiştirilir: yeniden doğrulama | E-24 · E-25 | eşlendi |
| `03 §6.2.5.2` | Gerçek sahip geri alma bağlantısını kullanır | E-28 · E-23 | eşlendi |
| `03 §6.2.5.3` | Z-45 dolmuş: geri alma yolu yok; siparişlere e-postadaki bağlantıyla girilir | E-16 · E-28 | eşlendi |
| `03 §6.2.6.1` | Havale IBAN'ı ele geçirilmiş yönetici hesabıyla değiştirilir | E-45 · E-52 | eşlendi |
| `03 §6.2.6.2` | Yönetici F-5'i görür, IBAN'ı düzeltir, şifresini değiştirir | E-45 · E-50 · ekran dışı — e-posta (`08`) | eşlendi |
| `03 §6.2.6.3` | Yanlış IBAN'lı siparişler firma iptaliyle kapatılır | E-37 | eşlendi |
| `03 §6.2.6.4` | Öteki yöneticiler kaldırılır: herkese bildirim; son yönetici kaldırılamaz | E-49 · ekran dışı — e-posta (`08`) | eşlendi |

**§9.1 Kayıt, e-posta doğrulama ve Google ile ilk giriş (7)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §9.1.1` | Kayıt ekranı; aydınlatma kapısı ve bağlantısı; "Google ile giriş" düğmesi | E-20 | eşlendi |
| `03 §9.1.2` | Ad, e-posta ve şifreyle kayıt; şifre politikası; "bu e-posta zaten kayıtlı" | E-20 | eşlendi |
| `03 §9.1.3` | Doğrulama bağlantısını ekrandan yeniden isteme (L-9) | E-20 · E-22 | eşlendi |
| `03 §9.1.4` | Doğrulama bağlantısı açılır: hesap doğrulanır, misafir siparişleri hesaba düşer | E-21 · E-27 | eşlendi |
| `03 §9.1.5` | Bağlantı süresinde açılmaz: kayıt silinir, "bağlantı geçersiz" | E-21 | eşlendi |
| `03 §9.1.6` | Google ile ilk giriş: hesap doğrulanmış ve şifresiz doğar | E-22 · ekran dışı — ürünün dışında | eşlendi |
| `03 §9.1.7` | Google e-postayı doğrulanmamış verir: kayda yönlendirme | E-22 · E-20 | eşlendi |

**§9.2 Giriş, oturum, şifre sıfırlama ve yeniden doğrulama (8)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §9.2.1` | E-posta ve şifreyle giriş; "beni hatırla" | E-22 · E-12 | eşlendi |
| `03 §9.2.2` | Google ile giriş mevcut hesaba bağlanır; hesap iki giriş yolu taşır | E-22 · E-25 (GAP-2) | eşlendi |
| `03 §9.2.3` | Çıkış ya da oturumun kendiliğinden kapanması | ortak bileşen: çerçeve · E-22 · E-12 | eşlendi |
| `03 §9.2.4` | "Şifremi unuttum": nötr mesaj; şifresiz hesapta "şifre belirle" | E-23 | eşlendi |
| `03 §9.2.5` | Bağlantıdan yeni şifre kurma; bütün oturumlar kapanır | E-23 · E-22 | eşlendi |
| `03 §9.2.6` | Yeniden doğrulama isteyen işlemler: şifre, e-posta, hesap silme | E-24 | eşlendi |
| `03 §9.2.7` | Yönetici panele girer: ayrı giriş, yalnız şifre | E-29 | eşlendi |
| `03 §9.2.8` | Yönetici panel şifresini sıfırlar | E-29 | eşlendi |

**§9.3 Profil ve adres defteri (7)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §9.3.1` | Adın değiştirilmesi | E-25 · E-10 · E-40 | eşlendi |
| `03 §9.3.2` | Adres defteri: ekleme, adlandırma, düzenleme, silme | E-26 · E-13 · ortak bileşen: adres | eşlendi |
| `03 §9.3.3` | Şifre değiştirme | E-25 · E-24 | eşlendi |
| `03 §9.3.4` | E-posta değiştirme isteği: yeni adres, doğrulanana kadar geçersiz | E-25 · E-24 (GAP-3) | eşlendi |
| `03 §9.3.5` | Yeni adresteki bağlantı açılır: değişiklik geçerli olur | E-21 · E-25 · E-27 · ekran dışı — e-posta (`08`) | eşlendi |
| `03 §9.3.6` | Eski adresteki geri alma bağlantısı açılır | E-28 · E-23 | eşlendi |
| `03 §9.3.7` | Yönetici kendi e-posta adresini değiştirir | E-50 · E-54 | eşlendi |

**§9.4 Hesabın silinmesi ve kişisel veri başvurusu (7)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §9.4.1` | Üye hesabını silmek ister: yeniden doğrulama; geri alma bağlantısı açıkken engel | E-25 · E-24 · ortak bileşen: onay (GAP-1) | eşlendi |
| `03 §9.4.2` | Sistem hesabı siler | E-25 · ekran dışı — arka plan | eşlendi |
| `03 §9.4.3` | Silinmiş hesabın siparişi misafir yolundan izlenir | E-15 · E-16 | eşlendi |
| `03 §9.4.4` | Kişisel veri başvurusu: iletişim formunun "KVKK talebi" tipi | E-10 | eşlendi |
| `03 §9.4.5` | Yönetici üye kaydı görünümünde e-postayla arar | E-40 · E-36 · E-37 | eşlendi |
| `03 §9.4.6` | Yönetici talep üzerine hesabı siler: geri alınamaz onay | E-40 · ortak bileşen: onay | eşlendi |
| `03 §9.4.7` | Aynı e-postayla yeniden kayıt: yeni hesap | E-20 | eşlendi |

**§9.5 Tercih yönetimi (6)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §9.5.1` | Bildirim tercihi ekranı yoktur | yoktur — kapsam dışı | ekran dışı |
| `03 §9.5.2` | Pazarlama onayı alanı yoktur | yoktur — kapsam dışı | ekran dışı |
| `03 §9.5.3` | Dil seçimi yoktur | yoktur — kapsam dışı | ekran dışı |
| `03 §9.5.4` | Çerez onay bandı yoktur; çerez politikası altbilgide | yoktur — kapsam dışı · ortak bileşen: çerçeve | eşlendi |
| `03 §9.5.5` | Uygulama içi veri indirme yoktur | yoktur — kapsam dışı | ekran dışı |
| `03 §9.5.6` | Panelde bildirim tercihi ve ayrı bildirim adresi yoktur | yoktur — kapsam dışı · E-44 | eşlendi |

#### 1.1.5 Ürün Gereksinimleri — kural düzeyi: vitrin ve site kuralları, `04`'e iş bırakan cümleler (87)

Akışa girmeyen görünüm ve site kuralları ile `04`'e açıkça iş bırakan cümleler kural kimliğiyle (K-727). İki yüzü olan kural (vitrin ve panel) burada iki tarafın adayına birlikte eşlenmiştir; yalnız panele bakan `02 §3.31`, §10.1, §10.6 ve §10.8 §1.1.9'dadır.

**`04`'e açıkça iş bırakan cümleler — §3.27–§3.34 dışındakiler (12)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `02 §3 (giriş)` | Bir kuralın biçimini, metnini ya da yerini `04`'e bırakan ifade arayüz kararını `04`'e bırakır | ortak bileşen: taban | eşlendi |
| `02 §3.1.4` | Yasal kimlik bilgileri sürekli erişilebilir, "İletişim" başlığı altında; yerleşimi `04`'ün işi; ETBİS bandı | ortak bileşen: çerçeve · E-10 · E-44 | eşlendi |
| `02 §3.9.2` | İndirimin başlangıç ve bitiş tarihi ürün sayfasında ve kartta; biçimi `04`'ün işi | E-04 · ortak bileşen: kart | eşlendi |
| `02 §3.13.3` | "Bu e-posta kayıtlı" giriş hatırlatması; biçimi `04`'ün işi | E-13 | eşlendi |
| `02 §3.16.4` | Sepet birleşme mesajı; metni ve yeri `04`'ün işi | E-12 · ortak bileşen: mesaj | eşlendi |
| `02 §3.19.5` | "Ücretsiz kargoya … kaldı" ve "Kargo ücretsiz"; biçimi ve yeri `04`'ün işi | E-12 · E-13 | eşlendi |
| `02 §3.20.1` | Teslimat illeri kısıtı adres adımından önce görünür; biçimi `04`'ün işi | E-04 · E-12 · E-13 | eşlendi |
| `02 §3.24.1` | Ödeme yükümlülüğü yazan düğme; metin ve biçim `04`'ün işi | E-13 | eşlendi |
| `02 §3.24.2` | Dijital kalemde üçüncü onay kutusu; nihai metin `04`'ün işi | E-13 | eşlendi |
| `02 §3.24.6` | Onay kutularının kaydı sipariş sayfasında görünür; kutu metni `04`'ün işi | E-16 · E-13 | eşlendi |
| `02 §6 (giriş)` | Kullanıcıya gösterilen hata metinlerinin biçimi ve yeri `04`'ün işi | ortak bileşen: mesaj | eşlendi |
| `02 §11 (envantere girmeyenler)` | Kırılma noktaları parametre değildir, `04`'ün işidir | ortak bileşen: taban | eşlendi |

**§3.27 Kurumsal içerik (28)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `02 §3.27.1` | Kurumsal içerik hazır tiplerle girilir; sayfa kurucu yoktur | E-41 · E-08 | eşlendi |
| `02 §3.27.2` | Beş hazır tip, genel sayfa ve duyuru; ayrı galeri tipi yoktur | E-41 | eşlendi |
| `02 §3.27.3` | Hakkımızda tek kayıttır, silinmez | E-41 · E-08 | eşlendi |
| `02 §3.27.4` | Hizmet tanıtımı fiyatsızdır; "Bize ulaşın" düğmesi | E-08 · E-07 · E-10 | eşlendi |
| `02 §3.27.5` | Referans iş: başlık, kısa açıklama, metin, görseller, kendi sayfası | E-08 · E-07 | eşlendi |
| `02 §3.27.6` | SSS tek sayfada, başlıksız listelenir | E-09 | eşlendi |
| `02 §3.27.7` | Şubeler İletişim sayfasında listelenir; "Haritada aç" | E-10 · E-41 | eşlendi |
| `02 §3.27.8` | İletişim sayfası ayrı içerik tipi değildir: kimlik, şubeler, form | E-10 | eşlendi |
| `02 §3.27.9` | Genel sayfa: başlık, metin, görseller; düzeni sabit | E-08 · E-41 | eşlendi |
| `02 §3.27.10` | Bağlı ürünler "İlgili ürünler" başlığıyla vitrin kartı olarak | E-08 · E-41 · ortak bileşen: kart | eşlendi |
| `02 §3.27.11` | Listeler panelde elle sıralanır; vitrin bu sırayı izler | E-41 · E-07 | eşlendi |
| `02 §3.27.12` | Sosyal medya bağlantıları ve WhatsApp sitenin üst ve alt bölümünde | ortak bileşen: çerçeve · E-42 | eşlendi |
| `02 §3.27.13` | Hiçbir içerik tipi zorunlu değildir; yayın kapısının zorunlu alanları | E-41 | eşlendi |
| `02 §3.27.14` | Hakkımızda boşken kurumsal blok marka adını ve logoyu gösterir | E-01 | eşlendi |
| `02 §3.27.15` | Görsel ve kısa alan kuralları; alternatif metin | E-41 · E-08 | eşlendi |
| `02 §3.27.16` | Uzun metinler kapalı metin biçimi setini kullanır | E-41 · E-08 | eşlendi |
| `02 §3.27.17` | Gömülü video oynatıcı yoktur; video bağlantıdır | E-08 | eşlendi |
| `02 §3.27.18` | Dosya eki yoktur | yoktur — kapsam dışı | ekran dışı |
| `02 §3.27.19` | Müşteri görüşleri ayrı tip değildir; dönen alıntı bloğu yoktur | yoktur — kapsam dışı | ekran dışı |
| `02 §3.27.20` | Blog yoktur | yoktur — kapsam dışı | ekran dışı |
| `02 §3.27.21` | Duyuru şeridi: tek, kısa, tarihli; her sayfanın en üstünde | ortak bileşen: çerçeve · E-41 | eşlendi |
| `02 §3.27.22` | Site boş kurulur; boş sitenin görünüşü kurallarla tanımlıdır | E-01 · ortak bileşen: çerçeve | eşlendi |
| `02 §3.27.23` | Her içerik kaydı Taslak ya da Yayında | E-41 | eşlendi |
| `02 §3.27.24` | Taslak yalnız adla kaydedilir; yayın zorunlu alanları ister | E-41 | eşlendi |
| `02 §3.27.25` | Taslak içerik yöneticiye "Taslak" bandıyla; sayfasız kaydın önizlemesi `04`'ün işi | ortak bileşen: taslak · E-06 · E-41 | eşlendi |
| `02 §3.27.26` | İçerikte sürüm geçmişi yoktur | E-41 | eşlendi |
| `02 §3.27.27` | Kalıcı silme geri alınamaz işlem uyarısıyla | E-41 · ortak bileşen: onay | eşlendi |
| `02 §3.27.28` | Kurumsal içerikte ileri tarihli yayın yoktur | yoktur — kapsam dışı | ekran dışı |

**§3.28 Ana sayfa ve menü (6)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `02 §3.28.1` | İki hazır ana sayfa düzeni: tanıtım öncelikli, mağaza öncelikli | E-01 · E-42 | eşlendi |
| `02 §3.28.2` | Kurumsal blok her zaman Hakkımızda'dır | E-01 | eşlendi |
| `02 §3.28.3` | "Ana sayfada göster" işaretli kayıtların blokları; blok yeri ve kayıt sayısı `04`'ün işi | E-01 · E-41 | eşlendi |
| `02 §3.28.4` | Menü sabit iskeletten kurulur; adlar değişir; sıra ve görünüm `04`'ün işi | ortak bileşen: çerçeve · E-42 | eşlendi |
| `02 §3.28.5` | Genel sayfalar altbilgide; "menüde göster" işareti | ortak bileşen: çerçeve · E-41 · E-42 | eşlendi |
| `02 §3.28.6` | Boş kategori menüde görünmez; adresinde boş kategori sayfası | ortak bileşen: çerçeve · E-02 · E-34 · E-42 | eşlendi |

**§3.29 Marka kimliği (6)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `02 §3.29.1` | Logo, marka adı, site simgesi ve tek marka rengi | E-43 · ortak bileşen: taban | eşlendi |
| `02 §3.29.2` | Logo yoksa marka adı yazıyla | ortak bileşen: çerçeve · E-01 | eşlendi |
| `02 §3.29.3` | Marka adı zorunludur; üst bölümde, sekmede, e-postalarda | ortak bileşen: çerçeve · E-43 | eşlendi |
| `02 §3.29.4` | Site simgesi isteğe bağlı; yoksa baş harf | E-43 · ortak bileşen: taban | eşlendi |
| `02 §3.29.5` | Marka rengi okunurluğu bozamaz; rengin yerleri `04`'ün işi | ortak bileşen: taban | eşlendi |
| `02 §3.29.6` | "Shopfolio ile kuruldu" imzası altbilgide; panelden kapatılır | ortak bileşen: çerçeve · E-43 | eşlendi |

**§3.30 Sayfa adresleri, arama motorları ve paylaşım (7)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `02 §3.30.1` | Ürün ve kategori adresi addan üretilir | ekran dışı — mimari (`05`) | ekran dışı |
| `02 §3.30.2` | Adres ilk yayında sabitlenir | ekran dışı — mimari (`05`) | ekran dışı |
| `02 §3.30.3` | Yönlendirme sistemi yoktur | yoktur — kapsam dışı | ekran dışı |
| `02 §3.30.4` | "Sayfa bulunamadı" sayfası sitenin görünümünde, dönüş yollarıyla | E-06 | eşlendi |
| `02 §3.30.5` | Arama motoru bilgileri otomatik üretilir | ekran dışı — mimari (`05`) | ekran dışı |
| `02 §3.30.6` | Arama motorlarına kapalı sayfalar | ekran dışı — mimari (`05`) | ekran dışı |
| `02 §3.30.7` | Ürün ve içerik sayfasında tek "Paylaş" düğmesi | E-04 · E-08 | eşlendi |

**§3.32 İletişim talebi (10)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `02 §3.32.1` | İletişim formu girişsiz açıktır; üyede ön dolu | E-10 | eşlendi |
| `02 §3.32.2` | Formun alanları; dosya eki yoktur | E-10 | eşlendi |
| `02 §3.32.3` | Kapalı beş konu tipi | E-10 | eşlendi |
| `02 §3.32.4` | Talebin iki durumu: Açık, Kapatıldı | E-38 · E-39 | eşlendi |
| `02 §3.32.5` | Sistem talebi kaydeder ve iletir; panelde yanıt yazma ekranı yoktur | E-39 · ekran dışı — e-posta (`08`) | eşlendi |
| `02 §3.32.6` | Müşteri talebini takip etmez; numara ve "taleplerim" sayfası yoktur | E-10 | eşlendi |
| `02 §3.32.7` | Görünmez tuzak alan ve gönderim limiti; captcha yoktur | E-10 | eşlendi |
| `02 §3.32.8` | Aydınlatma metni tamamlanmadan form kapalıdır | E-10 | eşlendi |
| `02 §3.32.9` | Ayrı şikâyet kanalı ve canlı destek yoktur | yoktur — kapsam dışı | ekran dışı |
| `02 §3.32.10` | İndirme hakkı yenileme ve siparişe dair özel istek bu formdan | E-10 | eşlendi |

**§3.33 Yasal metinler (9)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `02 §3.33.1` | Dört sürümlü yasal metin | E-47 · E-11 · E-13 | eşlendi |
| `02 §3.33.2` | Firmanın düzenlediği iki metin zorunludur | E-47 | eşlendi |
| `02 §3.33.3` | Sürüm kendiliğinden artar | E-47 · ekran dışı — arka plan | eşlendi |
| `02 §3.33.4` | Metin değişince yeniden onay alınmaz; onay adımı açıkken güncel metin | E-13 | eşlendi |
| `02 §3.33.5` | Aydınlatma metni bağlantısı kişisel veri alanı açan her ekranda | E-20 · E-22 · E-13 · E-16 · E-17 · E-18 · E-19 · E-10 | eşlendi |
| `02 §3.33.6` | Form ve sözleşme sipariş onay e-postasının gövdesinde | ekran dışı — e-posta (`08`) | ekran dışı |
| `02 §3.33.7` | Uyuşmazlık bilgisi iki metnin zorunlu parçası | E-13 · E-16 | eşlendi |
| `02 §3.33.8` | Ayarlarla çelişen serbest metin firmanın sorumluluğunda | E-41 · E-45 · E-46 | eşlendi |
| `02 §3.33.9` | Dört metin tamamlanmadan satış açılmaz; panel eksik metni adıyla gösterir | E-31 · E-47 | eşlendi |

**§3.34 Site geneli (8)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `02 §3.34.1` | Tek site, duyarlı tasarım; kırılma noktaları `04`'ün işi | ortak bileşen: taban | eşlendi |
| `02 §3.34.2` | Vitrin mobil öncelikli, panel masaüstü öncelikli; panelde hiçbir işlev mobilde kapanmaz | ortak bileşen: taban | eşlendi |
| `02 §3.34.3` | Tarayıcı tabanı son iki büyük sürüm | ekran dışı — doğrulama (`12`) | ekran dışı |
| `02 §3.34.4` | En dar desteklenen ekran genişliği bir parametredir | ortak bileşen: taban | eşlendi |
| `02 §3.34.5` | Erişilebilirlik tabanı: Kontrol Listesi A Seviyesi ve WCAG 2.2 | ortak bileşen: taban | eşlendi |
| `02 §3.34.6` | Ziyaretçi ölçümü yoktur | yoktur — kapsam dışı | ekran dışı |
| `02 §3.34.7` | Çalışma süresi taahhüdü ve planlı bakım penceresi yoktur | yoktur — kapsam dışı | ekran dışı |
| `02 §3.34.8` | Sipariş tarafında veri kaybı toleransı sıfır | ekran dışı — mimari (`05`) | ekran dışı |

**§12.4 Erişilebilirlik (1)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `02 §12.4.1` | Erişilebilirlik yasal yükümlülüktür; uyumluluk beyanı verilmez | ortak bileşen: taban · ekran dışı — doğrulama (`12`) | eşlendi |

#### 1.1.6 Ürün Gereksinimleri — alt bölüm düzeyi (79)

`02`'nin öteki kuralları alt bölüm düzeyinde eşlenir: adım adım hâlleri `03`'ün satırlarıdır ve §1.1.4 ile §1.1.8'de tek tek durur (K-727). Özet sütunu alt bölümün başlığıdır; eşleme, alt bölümün `03`'teki adımlarının ekranlarından türetilmiştir. Yalnız panele bakan 18 alt bölüm (`02 §3.31`, §6.6–§6.9, §8.5, §9.3, §10.1–§10.8, §11.1–§11.3) §1.1.9'dadır.

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `02 §1.1` | Sözlük kuralı | ekran dışı — yazım konvansiyonu | ekran dışı |
| `02 §1.2` | Terim sözlüğü | ekran dışı — yazım konvansiyonu | ekran dışı |
| `02 §1.3` | Aktörler | ekran dışı — yazım konvansiyonu | ekran dışı |
| `02 §2.1` | Akış envanteri | akış dizini — adımları `03 §2`, §8, §9 satırlarında (§1.1.4, §1.1.8) | eşlendi |
| `02 §2.2` | Uçtan uca ana akış | akış dizini — adımları `03 §2.10.1` satırlarında (§1.1.4) | eşlendi |
| `02 §2.3` | Kurumsal içerik akışı | akış dizini — adımları `03 §2.2`, §8.6 satırlarında (§1.1.4, §1.1.8) | eşlendi |
| `02 §3.1` | Firma, kurulum ve satış kapısı | E-44 · E-31 · ortak bileşen: satış-kapalı · ortak bileşen: çerçeve | eşlendi |
| `02 §3.2` | Satış modeli ve ürün tipleri | E-04 · E-33 | eşlendi |
| `02 §3.3` | Ürün, varyant ve seçenek | E-04 · E-33 | eşlendi |
| `02 §3.4` | Kategori | E-02 · E-34 · ortak bileşen: çerçeve | eşlendi |
| `02 §3.5` | Arama, süzgeç ve liste düzeni | E-02 · E-03 | eşlendi |
| `02 §3.6` | Stok ve tükenme | E-04 · E-12 · E-33 | eşlendi |
| `02 §3.7` | Ürünün yayın durumu, arşiv ve silme | E-05 · E-06 · E-32 · E-33 · ortak bileşen: taslak | eşlendi |
| `02 §3.8` | Fiyat ve KDV | E-04 · E-33 | eşlendi |
| `02 §3.9` | İndirim ve referans fiyat | E-04 · ortak bileşen: kart · E-33 | eşlendi |
| `02 §3.10` | Kupon | E-13 · E-35 | eşlendi |
| `02 §3.11` | Ürün görseli ve metni | E-04 · E-33 | eşlendi |
| `02 §3.12` | Dijital ürün | E-16 · E-33 · E-37 | eşlendi |
| `02 §3.13` | Üyelik, e-posta doğrulama ve oturum | E-20 · E-21 · E-22 · E-23 · E-24 · E-13 | eşlendi |
| `02 §3.14` | Adres | E-13 · E-26 · ortak bileşen: adres | eşlendi |
| `02 §3.15` | Hesabın silinmesi ve kişisel veri başvurusu | E-25 · E-40 · E-10 | eşlendi |
| `02 §3.16` | Sepet | E-12 | eşlendi |
| `02 §3.17` | Sipariş onayı ve stok ayırma | E-13 · ekran dışı — arka plan | eşlendi |
| `02 §3.18` | Asgari sipariş tutarı | E-12 · E-46 | eşlendi |
| `02 §3.19` | Kargo ücreti ve ücretsiz kargo eşiği | E-12 · E-13 · E-46 | eşlendi |
| `02 §3.20` | Teslim: bölge, yol, kargoya verme ve takip | E-04 · E-13 · E-16 · E-37 · E-46 | eşlendi |
| `02 §3.21` | Ödeme | E-13 · E-14 · E-37 · E-45 · ekran dışı — ürünün dışında | eşlendi |
| `02 §3.22` | Sipariş numarası ve takip erişimi | E-15 · E-16 · E-27 | eşlendi |
| `02 §3.23` | Sipariş anında donan değerler | E-16 · ekran dışı — arka plan | eşlendi |
| `02 §3.24` | Onay adımı ve yasal metinler | E-13 · E-16 | eşlendi |
| `02 §3.25` | Fatura | E-13 · E-53 · ekran dışı — ürünün dışında | eşlendi |
| `02 §3.26` | İptal, cayma, iade ve ayıp talebi | E-16 · E-17 · E-18 · E-19 | eşlendi |
| `02 §3.27` | Kurumsal içerik | kural düzeyinde — §1.1.5 | eşlendi |
| `02 §3.28` | Ana sayfa ve menü | kural düzeyinde — §1.1.5 | eşlendi |
| `02 §3.29` | Marka kimliği | kural düzeyinde — §1.1.5 | eşlendi |
| `02 §3.30` | Sayfa adresleri, arama motorları ve paylaşım | kural düzeyinde — §1.1.5 | eşlendi |
| `02 §3.32` | İletişim talebi | kural düzeyinde — §1.1.5 | eşlendi |
| `02 §3.33` | Yasal metinler | kural düzeyinde — §1.1.5 | eşlendi |
| `02 §3.34` | Site geneli: cihaz, tarayıcı, erişilebilirlik ve süreklilik | kural düzeyinde — §1.1.5 | eşlendi |
| `02 §4.1` | Sayım kuralları | ortak bileşen: süre · ekran dışı — arka plan | eşlendi |
| `02 §4.2` | Süreler | süre düzeyinde — `03 §4.1` satırlarında (§1.1.4) | eşlendi |
| `02 §4.3` | Kendiliğinden işleyen anlar | an düzeyinde — `03 §4.2` satırlarında (§1.1.4) | eşlendi |
| `02 §5.1` | Ürün ve varyantın yayın durumu | E-04 · E-05 · E-32 · E-33 · ortak bileşen: rozet | eşlendi |
| `02 §5.2` | Kurumsal içeriğin yayın durumu | E-08 · E-41 · ortak bileşen: rozet | eşlendi |
| `02 §5.3` | Sipariş: iki eksen ve oluşma anı | E-16 · E-37 · ortak bileşen: rozet | eşlendi |
| `02 §5.4` | Sevkiyat ekseni — Sipariş durumu | E-16 · E-37 · ortak bileşen: rozet | eşlendi |
| `02 §5.5` | Ödeme ekseni — Ödeme durumu | E-16 · E-37 · ortak bileşen: rozet | eşlendi |
| `02 §5.6` | İki eksenin bağı ve kendiliğinden geçişler | E-16 · E-37 · ekran dışı — arka plan | eşlendi |
| `02 §5.7` | Tipe göre sevkiyat hattı | E-16 · E-37 | eşlendi |
| `02 §5.8` | Sipariş kalemi düzeyindeki kayıtlar | E-16 · E-37 · ortak bileşen: rozet | eşlendi |
| `02 §5.9` | Geri alınamaz geçişler | E-37 · E-40 · ortak bileşen: onay | eşlendi |
| `02 §5.10` | Ayıp talebinin durumu | E-16 · E-38 · ortak bileşen: rozet | eşlendi |
| `02 §5.11` | İndirim ve kuponun zamana ve kullanıma bağlı geçerliliği | E-33 · E-35 · ekran dışı — arka plan | eşlendi |
| `02 §5.12` | Hesap ve oturum | E-22 · E-25 · ekran dışı — arka plan | eşlendi |
| `02 §5.13` | İletişim talebinin durumu | E-38 · E-39 · ortak bileşen: rozet | eşlendi |
| `02 §6.1` | Bütün akışlara uygulanan ilkeler | satır düzeyinde — `03 §3.1` satırlarında (§1.1.4) | eşlendi |
| `02 §6.2` | Satın alma (akış 1) | satır düzeyinde — `03 §3.2.1` satırlarında (§1.1.4) | eşlendi |
| `02 §6.3` | Sipariş takibi (akış 2) | satır düzeyinde — `03 §3.2.2` satırlarında (§1.1.4) | eşlendi |
| `02 §6.4` | İptal, cayma ve iade (akış 3) | satır düzeyinde — `03 §3.3` satırlarında (§1.1.4) | eşlendi |
| `02 §6.5` | Üyelik (akış 4) | satır düzeyinde — `03 §3.4` satırlarında (§1.1.4) | eşlendi |
| `02 §7.1` | Üç yol ve ortak ilkeler | E-16 | eşlendi |
| `02 §7.2` | İptal | E-16 · E-17 · E-37 | eşlendi |
| `02 §7.3` | Cayma | E-16 · E-18 · E-37 | eşlendi |
| `02 §7.4` | İade ve geri ödeme | E-16 · E-18 · E-37 · ortak bileşen: IBAN | eşlendi |
| `02 §7.5` | Ayıp talebi | E-16 · E-19 · E-38 | eşlendi |
| `02 §7.6` | Geri alma sınırları | E-16 · E-37 · E-41 · ortak bileşen: onay | eşlendi |
| `02 §8.1` | Kimlik doğrulama tabanı | E-20 · E-22 · E-23 · E-24 · E-29 | eşlendi |
| `02 §8.2` | Deneme limitleri | ortak bileşen: mesaj — satır düzeyinde `03 §6.1` (§1.1.4) | eşlendi |
| `02 §8.3` | Form ve site güvenliği | E-10 · E-13 · E-16 · E-37 · ekran dışı — mimari (`05`) | eşlendi |
| `02 §8.4` | Veri asgariliği | E-16 · E-20 · E-37 · yoktur — kapsam dışı | eşlendi |
| `02 §9.1` | Kanal ve ilkeler | ekran dışı — e-posta (`08`) · E-37 | eşlendi |
| `02 §9.2` | Müşteriye giden bildirimler | ekran dışı — e-posta (`08`) · E-16 | eşlendi |
| `02 §9.4` | Hesap ve yönetim e-postaları | ekran dışı — e-posta (`08`) · E-20 · E-23 · E-25 · E-28 | eşlendi |
| `02 §9.5` | Olmayan bildirimler | yoktur — kapsam dışı | ekran dışı |
| `02 §12.1` | Ticari çerçeve ve tüketici rejimi | E-04 · E-13 · E-16 · ortak bileşen: çerçeve · ekran dışı — ürünün dışında | eşlendi |
| `02 §12.2` | Kişisel verilerin korunması | E-11 · E-10 · E-47 · yoktur — kapsam dışı · ekran dışı — arka plan | eşlendi |
| `02 §12.3` | Veri sahipliği ve süreklilik | ekran dışı — ürünün dışında | ekran dışı |
| `02 §12.4` | Erişilebilirlik | kural düzeyinde — §1.1.5 | eşlendi |
| `02 §12.5` | Firmanın yükümlülükleri | ekran dışı — ürünün dışında | ekran dışı |

#### 1.1.7 Devir dizinleri ve park satırları — müşteri tarafı (59)

Devrin evi karar satırıdır; dizin onu ikinci kez kaydetmez (`PHASE1_CONFLICT_SCAN.md` §6, `PHASE2_CONFLICT_SCAN.md` §6). Özet, dizinin verdiği iş adıdır; dizinin yalnız doküman numarasıyla işaretlediği Aşama 1 kararlarında kararın konu hücresinden alınmıştır. Aşama 1 dizininin §6.2'sindeki gövde cümleleri `02` kimliğiyle §1.1.5'tedir. Karşılıksız kalan devir GAP adayıdır (UI1-06).

**Park satırları (3)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| Park K-06 · K-97 | Ekran × rol matrisi dört aktör üzerinden; yönetim tek rol | §1.1.2'nin aktör sütunu · ekran dışı — yazım konvansiyonu | eşlendi |
| Park K-14 · K-574 | Firma kimlik bilgileri ve ETBİS bandı sitede sürekli erişilebilir; yerleşim `04`'ün kararı (UI3-01) | ortak bileşen: çerçeve · E-10 · E-44 | eşlendi |
| Park K-27 | İki hazır ana sayfa düzeni tasarlanır (UI5-01) | E-01 · E-42 | eşlendi |

**Aşama 1 dizini — müşteri tarafı (35 / 80)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| K-493 | Ön bilgilendirmede iade taşıyıcısı bilgisi — "form metni" | E-13 | eşlendi |
| K-494 | Cayma beyanı ekranı; "iade malı bekleniyor" görünümü | E-18 · E-16 · E-37 | eşlendi |
| K-497 | Malı dönmeyen caymada IBAN'ın yeniden istenmesi | E-16 · E-37 | eşlendi |
| K-498 | Sipariş sayfasında yeniden açılan IBAN alanı; panelde "IBAN bekleniyor" | E-16 · E-31 · ortak bileşen: IBAN | eşlendi |
| K-499 | Gecikme feshi: kargodaki siparişte sınırın aşılması ve kanuni faiz | E-16 · E-17 · E-37 | eşlendi |
| K-503 | Hizmette cayma: onay kutusu, ifa süresi, karışık siparişte pencere | E-13 · E-16 · E-18 | eşlendi |
| K-505 | Ödenmemiş siparişte kısmi iptal yoktur | E-16 · E-17 | eşlendi |
| K-509 | Cayma istisnası sebeplerinin kapalı listesi | E-33 · E-13 · E-18 | eşlendi |
| K-511 | Fiyat etiketinin zorunlu bilgileri: üretim yeri, birim fiyat, fiyat tarihi | E-04 · E-33 | eşlendi |
| K-541 | Platform imzasının kuralı | ortak bileşen: çerçeve · E-43 | eşlendi |
| K-544 | Onaydan hemen önce ödeme yükümlülüğü bilgisi | E-13 | eşlendi |
| K-545 | İndirimin tarihlerinin vitrinde gösterilmesi | E-04 · ortak bileşen: kart | eşlendi |
| K-547 | Sözleşme öncesi bilgiler ve meslek odası | E-11 · E-10 · E-44 | eşlendi |
| K-550 | Üçüncü onay kutusunun metni | E-13 | eşlendi |
| K-553 | Aydınlatma bağlantısının göründüğü yerler ve başvuru kanalları | E-20 · E-22 · E-13 · E-16 · E-10 | eşlendi |
| K-561 | Adres formu | ortak bileşen: adres · E-13 · E-26 | eşlendi |
| K-569 | Sipariş sayfasının IBAN alanı | E-16 · ortak bileşen: IBAN | eşlendi |
| K-574 | Firma kimliği formu; ETBİS bandının yerleşimi | E-44 · ortak bileşen: çerçeve | eşlendi |
| K-575 | Sipariş sayfasının IBAN alanı; panel adımı | E-16 · E-37 | eşlendi |
| K-576 | Satış kapısı uyarıları | ortak bileşen: satış-kapalı · E-31 | eşlendi |
| K-578 | Hizmet kaleminin cayma ekranı | E-18 | eşlendi |
| K-586 | Sipariş sayfasında e-posta geçmişi | E-37 · E-16 | eşlendi |
| K-588 | İki giriş ekranı — müşteri ve yönetici | E-22 · E-29 | eşlendi |
| K-589 | Sipariş sayfasında geç ödeme ve geri ödemesi | E-37 | eşlendi |
| K-591 | Onay adımında metin değişikliği satırı; kutuların yeniden işaretlenmesi | E-13 | eşlendi |
| K-593 | Hizmet kaleminin cayma düğmesi | E-16 | eşlendi |
| K-594 | Boş sepet mesajı | E-12 | eşlendi |
| K-595 | Sipariş sayfasında onay kutusunun kaydı | E-16 | eşlendi |
| K-599 | Hesabın e-postası değişince bildirim metni ve geri alma ekranı | E-28 · ekran dışı — e-posta (`08`) (GAP-5) | eşlendi |
| K-602 | Takip formunun nötr mesajı | E-15 · ortak bileşen: mesaj | eşlendi |
| K-605 | Kayıt formunda ad alanı; hesapta adı değiştirme | E-20 · E-25 | eşlendi |
| K-610 | Ürün sayfasında yerli üretim logosunun yeri | E-04 | eşlendi |
| K-625 | Erişilebilirlik tabanı — tasarım sistemi | ortak bileşen: taban | eşlendi |
| K-626 | "İşlem rehberi" sayfası ve altbilgi bağlantısı | E-11 · ortak bileşen: çerçeve | eşlendi |
| K-627 | "İletişim" başlığının bilgileri ve yerleşimi; kimlik formu | E-10 · ortak bileşen: çerçeve · E-44 | eşlendi |

**Aşama 2 dizini — müşteri tarafı (18 / 35)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| K-657 | Sipariş sayfasında reddedilen iade kaleminin görünümü | E-16 | eşlendi |
| K-659 | Doğrulama bağlantısını yeniden isteme mesajı | E-20 · E-22 · ortak bileşen: mesaj | eşlendi |
| K-663 | Sipariş sayfasında ve "ödendi" işaretinde donmuş IBAN; IBAN değişikliği onayında sayı | E-16 · E-14 · E-37 · E-45 | eşlendi |
| K-665 | Beyan ekranında güncel iade adresi; sipariş sayfasında beyanın adresi | E-18 · E-16 · E-46 | eşlendi |
| K-669 | İzlenebilirlik matrisinin atıf biçimi | ekran dışı — yazım konvansiyonu | ekran dışı |
| K-673 | Sipariş sayfasında hizmet cayma düğmesinin görünürlüğü | E-16 | eşlendi |
| K-676 | Sipariş sayfasında ayıp talebinin görünümü | E-16 | eşlendi |
| K-678 | İletişim formunun gönderim sonrası ekranı | E-10 | eşlendi |
| K-679 | Kupon alanı: kodu değiştirme ve kaldırma | E-13 | eşlendi |
| K-680 | Ödeme adımında yeni adresi deftere kaydetme seçimi | E-13 | eşlendi |
| K-686 | Öteki hâllerde havale geri ödemesi için IBAN isteği | E-16 · E-37 | eşlendi |
| K-690 | Firma iptalinde ve kalem çıkarmada IBAN alanı | E-16 · E-37 · ortak bileşen: IBAN | eşlendi |
| K-697 | Google e-postayı doğrulanmamış verince yönlendirme mesajı | E-22 · E-20 | eşlendi |
| K-698 | Geri alma bağlantısı açıkken hesap silme engelinin mesajı | E-25 · E-40 | eşlendi |
| K-700 | Aktör listesi | ekran dışı — yazım konvansiyonu | ekran dışı |
| K-701 | Matriste `03` atıflarının okunuşu | ekran dışı — yazım konvansiyonu | ekran dışı |
| K-702 | Aktörü olmayan dış olayın yazımı | ekran dışı — yazım konvansiyonu | ekran dışı |
| K-704 | Teyit ve bekleme ekranı | E-14 · E-16 | eşlendi |

**Gövde cümleleri — müşteri tarafı (3 / 7)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §0.1.4` | KP-37, KP-69, KP-70, KP-71'in karşılığı ekran tasarımında ve mimaride | ortak bileşen: taban · ekran dışı — mimari (`05`) · ekran dışı — doğrulama (`12`) | eşlendi |
| `03 §2.1.2` | İndirimdeki ürünün vitrin kartının düzeni | ortak bileşen: kart · E-02 | eşlendi |
| `03 §3.1.6` | Hata mesajlarının biçimi | ortak bileşen: mesaj | eşlendi |

#### 1.1.8 Kullanıcı Akışları — firma tarafı ve kalan (415)

`03`'ün satırları satır kimliğiyle, tek tek (K-727, K-730). Kapsam: `03 §1.4` 13 · §1.5 7 · §1.6.2 4 · §1.7 17 · §1.10 8 · §1.11.15–§1.11.41 27 (= §1'in kalan 76 satırı) · §3.5 60 · §6.3 9 · §7 131 · §8 106 · §10 33. Özet satırın kendisinden — aktörün yaptığı, sistemin denetimi ve cevabı — alınmıştır; satırların tamamı okundu (1b). Sütunlar ve değer kümesi §1.1'in başındaki ve §1.1.2'deki gibidir.

- **Durum makinesi satırları (`03 §1.4`–§1.10):** bir geçişin, kaydın ya da işaretin ekranı, onun göründüğü ve işlendiği ekrandır — çoğu sipariş ayrıntısı (E-37) ve sipariş sayfası (E-16). Sistemin kendiliğinden yaptığı ayırma ve geri dönüş "ekran dışı — arka plan"dır; sonucu panelde görünüyorsa (stok alanının yanındaki ayrılmış adet, kuponun ayrılmış hakları) o ekran da yazılır.
- **Hata satırları (`03 §3.5`):** ekran, satırın bağlandığı akış adımının ekranıdır; panelin değeri reddettiği ya da işlemi durdurduğu satır "ortak bileşen: mesaj"ı da taşır.
- **Bildirim haritası (`03 §7`):** satır satır okundu; ölçüt K-731'dedir (K-727'yi netleştirir). Bir bildirim satırı yalnız **kendine özgü bir ekran izi** taşıyorsa ekrana eşlenir: e-postadaki bağlantının açtığı ekran (sipariş sayfası, doğrulama ve sıfırlama ekranları, davet kabul ekranı), IBAN isteğinin sipariş sayfasında açtığı alan ve paneldeki "IBAN bekleniyor" listesi, satırın adıyla andığı panel işareti ya da sayacı, yeniden gönderim. Müşteriye giden sipariş bildirimlerinin (B-1…B-16) tamamına ortak olan "e-posta ulaşmadı" işareti ve yeniden gönderim her bildirim satırında yinelenmez; kendi satırlarında eşlenmiştir (`03 §1.7.3.1`, §7.1.37, §8.9.2, §10.1.2.2). Firmaya giden bildirimlerde (F-1…F-4) işaretin düştüğü satır yazılmıştır; işaretin biçimi workshop konusudur (UI9-01). `03 §7.2`'nin bildirim üretmeyen olayları da bildirim matrisinin satırlarıdır ve "ekran dışı — e-posta (`08`)" değerini alır; gerekçesi bir ekranı adıyla anan satır ("panel sayaçlarla gösterir", "sipariş sayfası yeni son günü gösterir") o ekrana da eşlenir. `03 §7.3` aynı eşlemenin ters okunuşudur: her satırın ekranı, "Harita" sütununun gösterdiği §7.1 ve §7.2 satırlarının ekranlarının birleşimidir (betikle türetildi).
- **Yönetim akışları (`03 §8`):** onay isteyen işlem "ortak bileşen: onay"ı, kapalı listeden sebep seçtiren işlem "ortak bileşen: sebep"i, geçmişe dönük ve ileri tarihsiz tarih alan işlem "ortak bileşen: tarih"i, kaydın ya da siparişin arada değiştiğini söyleyen uyarı "ortak bileşen: eşzamanlı"yı taşır. Manuel adım bütçesinin satırları (`03 §8.5`) adımların atıldığı ekrana eşlenir; bütçenin kendisi ekran değil, ekran tasarımının sınırıdır (UI9-12).
- **Operasyonel akışlar (`03 §10`):** kurulum tarafının işi "ekran dışı — kurulum (`10 §4.1`)"dir. Sitenin kesintisinde ürün çalışmaz ve ürünün içinden bilgilendirme yoktur (`03 §10.3.1`); bakım modu yoktur (`03 §10.2.1`) — kesinti ve bakım için aday ekran açılmamıştır.

**§1.4 İki eksenin bağı — eşzamanlı geçişler ve stok ile kupon hakkının anları (13)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §1.4.2.1` | Ödeme onayı (Ö1) sevkiyat eksenini birlikte ilerletir: fiziksel kalemde S1, yalnız dijitalde S2, dijital kalemlere teslim işareti | E-37 · E-16 · ortak bileşen: rozet | eşlendi |
| `03 §1.4.2.2` | Ödenmemiş siparişin iptali — sistemin, müşterinin ya da yöneticinin — iki ekseni birlikte kapatır (S3 + Ö2) | E-37 · E-16 · ortak bileşen: rozet | eşlendi |
| `03 §1.4.2.3` | Teslim işareti almamış son açık kalemin kapanması ya da teslim işareti alması kapanış kuralını işletir | E-37 · E-16 · ortak bileşen: rozet | eşlendi |
| `03 §1.4.2.4` | Havale "ödendi" işaretinin düzeltilmesi iki ekseni birlikte geri alır (Ödendi → Bekliyor, Hazırlanıyor → Alındı) | E-37 · ortak bileşen: rozet | eşlendi |
| `03 §1.4.4.1` | Sipariş oluşunca stok ve kupon hakkı ayrılır | ekran dışı — arka plan · E-33 · E-35 | eşlendi |
| `03 §1.4.4.2` | Ödeme onayında ayrılan stok kesin düşer, kupon kullanılmış sayılır | ekran dışı — arka plan · E-33 · E-35 | eşlendi |
| `03 §1.4.4.3` | Ödenmeden iptalde ayrılan stok serbest kalır, kupon hakkı geri döner | ekran dışı — arka plan · E-33 · E-35 | eşlendi |
| `03 §1.4.4.4` | Ödenmiş kalemin iptali ve çıkarmada adet stoğa kendiliğinden döner; kupon hakkı yalnız siparişin tamamı iptal edilince | ekran dışı — arka plan · E-37 | eşlendi |
| `03 §1.4.4.5` | Gecikme feshinde kargoya verilmemiş kalemin stoğu kendiliğinden döner; kargodan dönen mal teslim alma adımıyla eklenir | ekran dışı — arka plan · E-37 | eşlendi |
| `03 §1.4.4.6` | Cayma ve iadede fiziksel mal yöneticinin teslim alıp eklemesiyle stoğa döner; reddedilen mal dönmez | E-37 | eşlendi |
| `03 §1.4.4.7` | Yanlış iptalin düzeltilmesinde stok yeniden ayrılır; yetmezse düzeltme yapılamaz | E-37 · ortak bileşen: mesaj | eşlendi |
| `03 §1.4.4.8` | Havale işaretinin düzeltilmesinde ayırma ve kupon hakkı tutulu kalır | ekran dışı — arka plan · E-37 | eşlendi |
| `03 §1.4.4.9` | Varyantın kalıcı silinmesiyle silinen varyantın ayırması düşer | E-33 · ortak bileşen: onay | eşlendi |

**§1.5 Tipe göre hatlar (7)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §1.5.1` | Fiziksel — kart hattı: sistem ödemeyi onaylar, yönetici kargoya verir ve teslim işaretini koyar; iki zorunlu elle adım | E-37 · E-16 | eşlendi |
| `03 §1.5.2` | Fiziksel — havale hattı: ödeme onayını yöneticinin "ödendi" işareti tetikler; üç zorunlu elle adım | E-37 · E-16 | eşlendi |
| `03 §1.5.3` | Yalnız dijital hat: ödeme onayıyla teslim edilir; kartta sıfır, havalede bir elle adım | E-37 · E-16 | eşlendi |
| `03 §1.5.4` | Yalnız hizmet hattı: yöneticinin son "tamamlandı" işaretiyle teslim edilir; bir, havalede iki elle adım | E-37 · E-16 | eşlendi |
| `03 §1.5.5` | Dijital + hizmet hattı: dijital kalem ödeme onayında, hizmet "tamamlandı" işaretinde teslim edilir | E-37 · E-16 | eşlendi |
| `03 §1.5.6` | Karışık, fiziksel kalemli hat: fiziksel hat yürür; dijital ve hizmet kalemi kalem düzeyinde teslim edilir; adımlar toplanır | E-37 · E-16 | eşlendi |
| `03 §1.5.7` | Fiziksel kalemleri kapanmış karışık sipariş bulunduğu durumda kapanış kuralıyla biter | E-37 · E-16 | eşlendi |

**§1.6.2 Düzeltme (4)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §1.6.2.1` | Erken ya da yanlış konmuş teslim işareti bir önceki duruma düzeltilir; cayma penceresi ve ayıp süresi yeni tarihe kadar işlemez | E-37 · ortak bileşen: sebep | eşlendi |
| `03 §1.6.2.2` | Yanlış siparişte yapılmış firma iptali iptalden önceki duruma düzeltilir; ayırma yetmezse düzeltme yapılamaz; kapanış düzeltmenin konusu değildir | E-37 · ortak bileşen: sebep · ortak bileşen: mesaj | eşlendi |
| `03 §1.6.2.3` | Yanlış konmuş Kargoya verildi ya da Teslim edilemedi işareti önceki duruma düzeltilir; o aralıkta doğmuş beyan ve talep kalır | E-37 · ortak bileşen: sebep | eşlendi |
| `03 §1.6.2.4` | Para gelmeden konmuş havale "ödendi" işareti iki eksende birlikte düzeltilir; kalem kaydı varsa düzeltme yapılamaz, ödeme süresi yeniden başlar | E-37 · ortak bileşen: sebep · ortak bileşen: mesaj | eşlendi |

**§1.7 Kalem kayıtları ve panel işaretleri (17)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §1.7.1.1` | Teslim işareti ve teslim tarihi — fizikselde yöneticinin S6'sı, dijitalde ödeme onayı, hizmette "tamamlandı" işareti | E-37 · E-16 · ortak bileşen: rozet | eşlendi |
| `03 §1.7.1.2` | İptal kaydı — müşteri ya da yönetici; yöneticide sebepli; kalemi kapatır | E-37 · E-16 · E-17 | eşlendi |
| `03 §1.7.1.3` | Çıkarma kaydı — yönetici; kalem iptal edilmiş gibi sayılır | E-37 · E-16 | eşlendi |
| `03 §1.7.1.4` | Gecikme feshi kaydı — müşteri sipariş sayfasından ya da yönetici başka kanaldan gelen bildirimi kaydederek | E-17 · E-37 · E-16 | eşlendi |
| `03 §1.7.1.5` | Cayma beyanı kaydı — müşteri sipariş sayfasından ya da yöneticinin kaydıyla; fiziksel kalemde iade adresini taşır | E-18 · E-37 · E-16 | eşlendi |
| `03 §1.7.1.6` | İade teslim alma kaydı — yönetici, ulaşma tarihiyle; stoğa ekleme ayrı adımdır | E-37 | eşlendi |
| `03 §1.7.1.7` | İade reddi kaydı — yönetici, koşullu istisna kaleminde, teslim alma adımında | E-37 · E-16 | eşlendi |
| `03 §1.7.1.8` | "Mal dönmedi" kapanışı — yönetici, müşterinin gönderme süresi geçtikten sonra | E-37 | eşlendi |
| `03 §1.7.1.9` | IBAN isteği kaydı — sistem yöneticinin beş adımıyla birlikte açar ya da yönetici isteği kendisi açar | E-37 · E-16 · E-31 · ortak bileşen: IBAN | eşlendi |
| `03 §1.7.1.10` | Ayıp talebi kaydı — müşteri açar; hattı etkilemez | E-19 · E-16 · E-38 | eşlendi |
| `03 §1.7.1.11` | İndirme sayacı — sistem her indirmede sayar; yönetici yeniler | E-16 · E-37 | eşlendi |
| `03 §1.7.3.1` | "e-posta ulaşmadı" işareti siparişin — ayıp talebinde talebin de — ya da davetin satırına düşer; firmaya giden bildirim satıra işaret düşürmez (K-760); e-posta yeniden gönderilip ulaşınca kalkar | E-36 · E-37 · E-38 · E-49 · ortak bileşen: rozet | eşlendi |
| `03 §1.7.3.2` | "ödeme sonucu alınamadı" işareti kart son sorgusu yanıtsız kalınca siparişin satırına düşer; sonuç gelince kalkar | E-36 · E-37 · ortak bileşen: rozet | eşlendi |
| `03 §1.7.3.3` | "geri ödeme gerçekleşmedi" işareti kart iadesi sağlayıcıda başarısız olunca siparişin satırına düşer | E-36 · E-37 · ortak bileşen: rozet | eşlendi |
| `03 §1.7.3.4` | "IBAN bekleniyor" işareti kalemde durur ve panelde liste olarak görünür; müşteri IBAN'ı girince kalkar | E-31 · E-37 · ortak bileşen: rozet (GAP-11) | eşlendi |
| `03 §1.7.3.5` | "iade malı bekleniyor" işareti teslimden sonraki cayma beyanıyla kaleme düşer; teslim alma, ret ya da "mal dönmedi" ile kalkar | E-37 · ortak bileşen: rozet | eşlendi |
| `03 §1.7.3.6` | "e-posta düzeltildi" işareti misafir siparişinin e-postası düzeltilince siparişin satırına düşer ve kalkmaz | E-36 · E-37 · ortak bileşen: rozet | eşlendi |

**§1.10 Durum makinesi olmayan varlıklar (8)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §1.10.1` | İndirim tarih aralığının içinde geçerlidir; sistem başlatır ve bitirir | E-33 · E-04 · ekran dışı — arka plan | eşlendi |
| `03 §1.10.2` | Kupon tarih aralığında ve kullanım hakkı kaldıkça geçerlidir; adet kullanılmış ve ayrılmış hakların altına indirilemez | E-35 | eşlendi |
| `03 §1.10.3` | Kupon hakkı ve stok ayırma: ayrılır, kesin düşer ya da serbest kalır | ekran dışı — arka plan · E-33 · E-35 | eşlendi |
| `03 §1.10.4` | Duyuru Yayında'yken — tarihleri girilmişse — yalnız aralığında görünür | E-41 · ortak bileşen: çerçeve · ekran dışı — arka plan | eşlendi |
| `03 §1.10.5` | Hesap — müşteri ve yönetici: doğrulanmamış kayıt bekler; doğrulama, silme, kaldırma ve kurulum tarafının açtığı yeni yönetici hesabı geçerliliği değiştirir | E-20 · E-21 · E-25 · E-30 · E-49 · E-50 · E-22 · E-28 | eşlendi |
| `03 §1.10.6` | Oturum "beni hatırla" seçimine göre kısa ya da uzundur; şifre sıfırlama, silme, geri alma ve kaldırma oturumları kapatır | E-22 · E-29 · ekran dışı — arka plan | eşlendi |
| `03 §1.10.7` | Yönetici daveti gönderiminden itibaren süreli ve tek kullanımlıktır; süre, geri çekme, yeni davet ya da gönderenin kaldırılmasıyla geçersizleşir | E-49 · E-30 | eşlendi |
| `03 §1.10.8` | Mağazanın satış açıklığı dört koşula bağlıdır; bir koşulun düşmesi ya da geçici kapatma yeni siparişi durdurur, açık siparişlere dokunmaz | E-31 · E-44 · ortak bileşen: satış-kapalı | eşlendi |

**§1.11 Durum × rol × işlem — yönetici, panel (§1.11.15–§1.11.41) (27)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §1.11.15` | Havale "ödendi" işareti — havale siparişinde ödeme beklenirken; geri alınamaz | E-37 · ortak bileşen: onay | eşlendi |
| `03 §1.11.16` | Kargoya vermek — Hazırlanıyor'da, açık fiziksel kalem varken; geri alınamaz | E-37 · ortak bileşen: onay | eşlendi |
| `03 §1.11.17` | Teslim işaretini ve tarihini koymak — Kargoya verildi'de | E-37 · ortak bileşen: tarih | eşlendi |
| `03 §1.11.18` | "Teslim edilemedi" işaretlemek — Kargoya verildi'de | E-37 | eşlendi |
| `03 §1.11.19` | Yeniden göndermek — Teslim edilemedi'de, açık fiziksel kalem varken; geri alınamaz | E-37 · ortak bileşen: onay | eşlendi |
| `03 §1.11.20` | Hizmet kalemini "tamamlandı" işaretlemek — ödeme onaylanmışken, kalem açıkken | E-37 | eşlendi |
| `03 §1.11.21` | Firma iptali — ödeme beklenirken yalnız siparişin tamamı, ödemeden sonra kalem düzeyinde; sebepli ve geri alınamaz | E-37 · ortak bileşen: onay · ortak bileşen: sebep | eşlendi |
| `03 §1.11.22` | Kalem çıkarmak ya da adedini azaltmak — ödeme onayından sonra, kalemin sınırına kadar; kart hattında geri alınamaz | E-37 · ortak bileşen: onay | eşlendi |
| `03 §1.11.23` | Adresi düzeltmek — sipariş Kargoya verildi'ye geçene kadar | E-37 · ortak bileşen: adres | eşlendi |
| `03 §1.11.24` | Kargo şirketini ve takip numarasını düzeltmek — kargoya verildikten sonra | E-37 | eşlendi |
| `03 §1.11.25` | Teslim tarihini ya da iade malının ulaşma tarihini düzeltmek — işaretten ya da teslim almadan sonra | E-37 · ortak bileşen: tarih | eşlendi |
| `03 §1.11.26` | Durum geçişini düzeltmek — düzeltmenin dört satırının sınırlarında | E-37 · ortak bileşen: sebep | eşlendi |
| `03 §1.11.27` | İç not yazmak — her durumda | E-37 | eşlendi |
| `03 §1.11.28` | İade malını teslim almak — cayma beyanlı ya da feshedilmiş kalemin malı ulaşınca; kapatılmış kalemi yeniden açar | E-37 · ortak bileşen: tarih | eşlendi |
| `03 §1.11.29` | İade malını reddetmek — koşullu istisna kaleminde, teslim alma adımında; geri alınamaz | E-37 · ortak bileşen: onay | eşlendi |
| `03 §1.11.30` | "Mal dönmedi" kapanışı — müşterinin gönderme süresi geçmiş ve mal ulaşmamışken | E-37 | eşlendi |
| `03 §1.11.31` | Geri ödemeyi işlemek — havale hattında IBAN varken, caymada iki hatta, tutar bazlı geri ödemede; geri alınamaz | E-37 · ortak bileşen: onay · ortak bileşen: IBAN | eşlendi |
| `03 §1.11.32` | Kart iadesini yeniden denemek ya da havale yolunu açmak — "geri ödeme gerçekleşmedi" işaretliyken | E-37 | eşlendi |
| `03 §1.11.33` | "Havale gerçekleşmedi" bildirmek — geri ödeme işlenmeden önce; IBAN silinir ve yeniden istenir | E-37 | eşlendi |
| `03 §1.11.34` | Başka kanaldan gelen caymayı kaydetmek — pencere dışında ve mutlak istisnada ayrıca onayla; geri alınamaz | E-37 · ortak bileşen: onay · ortak bileşen: tarih | eşlendi |
| `03 §1.11.35` | Misafir siparişinin e-postasını düzeltmek — kişisel veriler imha edilene kadar | E-37 | eşlendi |
| `03 §1.11.36` | E-postayı yeniden göndermek — "e-posta ulaşmadı" işaretli satırda | E-36 · E-37 | eşlendi |
| `03 §1.11.37` | Ayıp talebini "çözüldü" işaretlemek ya da yeniden açmak | E-38 · E-37 | eşlendi |
| `03 §1.11.38` | İndirme hakkını yenilemek — dijital kalemde | E-37 | eşlendi |
| `03 §1.11.39` | Geri ödeme için müşteriden IBAN istemek — havale hattında IBAN'ı olmayan geri ödemede | E-37 | eşlendi |
| `03 §1.11.40` | Siparişi kapatmak (S9 kapanışı) — Teslim edilemedi'de açık kalem kalmamışken; sebep seçilmez | E-37 | eşlendi |
| `03 §1.11.41` | Başka kanaldan gelen gecikme feshi bildirimini kaydetmek — fesih aralığında; geri alınamaz | E-37 · ortak bileşen: onay · ortak bileşen: tarih | eşlendi |

**§3.5.1 Hatalar — katalog yönetimi (16)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §3.5.1.1` | İndirimli fiyat referans fiyatın altına inmez: indirim kaydedilmez, panel sebebini söyler | E-33 · ortak bileşen: mesaj | eşlendi |
| `03 §3.5.1.2` | İçinde ürün ya da alt kategori bulunan kategori silinemez; engelleyenler sayısıyla gösterilir | E-34 · ortak bileşen: mesaj | eşlendi |
| `03 §3.5.1.3` | Üç seviyeyi aşan ya da kendi alt ağacına giden taşıma yapılmaz | E-34 · ortak bileşen: mesaj | eşlendi |
| `03 §3.5.1.4` | Yayın kapısının bir koşulu eksikse ürün yayına alınamaz; panel eksik koşulu gösterir | E-33 · ortak bileşen: mesaj | eşlendi |
| `03 §3.5.1.5` | Dijital ürünün yayındaki bir varyantı dosyasızsa ürün yayına alınamaz; panel varyantı gösterir | E-33 · ortak bileşen: mesaj | eşlendi |
| `03 §3.5.1.6` | Yayındaki varyantı dosyasız bırakacak dosya silme engellenir | E-33 · ortak bileşen: mesaj | eşlendi |
| `03 §3.5.1.7` | Fiyata ikiden fazla ondalık girilemez | E-33 · ortak bileşen: mesaj | eşlendi |
| `03 §3.5.1.8` | Sabit tutarlı kuponun asgari sepet tutarı kupon tutarının üstünde olmalıdır; yüzdesel kupon %100 girilemez | E-35 · ortak bileşen: mesaj | eşlendi |
| `03 §3.5.1.9` | Ürünün kargoya verme süresi üst çiti aşamaz | E-33 · ortak bileşen: mesaj | eşlendi |
| `03 §3.5.1.10` | İki yönetici aynı kaydı düzenlerse ikinci kaydeden uyarılır | E-33 · ortak bileşen: eşzamanlı | eşlendi |
| `03 §3.5.1.11` | Yayındaki ürünün son Yayında varyantı arşive ya da taslağa alınamaz; panel önce ürünü taslağa almaya yönlendirir | E-33 · ortak bileşen: mesaj | eşlendi |
| `03 §3.5.1.12` | Varyanta 0,00 TL fiyat girilemez | E-33 · ortak bileşen: mesaj | eşlendi |
| `03 §3.5.1.13` | %100 indirim ya da bir varyantı 0,00 TL'ye indiren indirim kaydedilmez | E-33 · ortak bileşen: mesaj | eşlendi |
| `03 §3.5.1.14` | Stok ya da kontenjan ödemesi beklenen siparişlerin ayırdığı adedin altına indirilemez; panel ayırmayı tutan siparişleri gösterir | E-33 · E-37 · ortak bileşen: mesaj | eşlendi |
| `03 §3.5.1.15` | Kuponun kullanım adedi kullanılmış ve ayrılmış hakların altına çekilemez; kampanya bitiş tarihi öne çekilerek durdurulur | E-35 · ortak bileşen: mesaj | eşlendi |
| `03 §3.5.1.16` | Açık siparişi olan ürün ya da varyant silinebilir; panel onaydan önce açık sipariş sayısını söyler | E-33 · ortak bileşen: onay | eşlendi |

**§3.5.2 Hatalar — sipariş yürütümü (17)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §3.5.2.1` | Havalede gelen tutar farklıdır: sistem tutar sormaz, yönetici "ödendi" işaretler ya da farkı dışarıda çözer | E-37 | eşlendi |
| `03 §3.5.2.2` | Havale iptal edilmiş siparişe gelir: sipariş "ödendi" işaretlenemez; para sistemin dışında geri ödenir | E-37 · ekran dışı — ürünün dışında | eşlendi |
| `03 §3.5.2.3` | Kargo gönderiyi geri döndürür: yönetici Teslim edilemedi işaretler, yeniden gönderir ya da iptal eder; sipariş "kargoya verilecek" sayacındadır | E-37 · E-31 | eşlendi |
| `03 §3.5.2.4` | Kargoya verirken takip numarası ya da "kendi aracımızla teslim" beyanı olmadan işaret konamaz | E-37 · ortak bileşen: mesaj | eşlendi |
| `03 §3.5.2.5` | Yanlış geçiş: panel "geri al" sunmaz; düzeltme sebep seçilerek yapılan yeni bir geçiştir | E-37 · ortak bileşen: sebep | eşlendi |
| `03 §3.5.2.6` | Firma siparişi karşılayamaz: sebepli firma iptali; "stokta bulunamadı"da onaydan önce yasal uyarı | E-37 · ortak bileşen: onay · ortak bileşen: sebep | eşlendi |
| `03 §3.5.2.7` | İndirme hakkı dolar: müşteri iletişim formundan başvurur, yönetici hakkı panelden yeniler | E-37 · E-16 · E-10 | eşlendi |
| `03 §3.5.2.8` | Satılan dijital dosya ayıplıdır: yönetici dosyayı yeniden yükler, yeni hâl eski alıcılara da iner | E-33 · E-16 | eşlendi |
| `03 §3.5.2.9` | İptal edilmiş siparişe kart ödemesi gelir: sistem kendiliğinden geri öder | ekran dışı — arka plan · E-37 | eşlendi |
| `03 §3.5.2.10` | Teslim hiç işaretlenmez: kendiliğinden geçiş yoktur; sipariş "teslim işareti bekleyen" sayacında kalır | E-37 · E-31 · E-16 | eşlendi |
| `03 §3.5.2.11` | Teslim tarihi ya da takip bilgisi yanlış girilir: yönetici panelden düzeltir, pencereler yeniden işler | E-37 · ortak bileşen: tarih | eşlendi |
| `03 §3.5.2.12` | Siparişin adresi yanlıştır: yönetici müşterinin talebiyle Kargoya verildi'ye kadar düzeltir ve teyit eder | E-37 · ortak bileşen: adres | eşlendi |
| `03 §3.5.2.13` | E-posta üç denemede de ulaşmaz: panel satırına "e-posta ulaşmadı" düşer, yönetici yeniden gönderir | E-36 · E-37 · ortak bileşen: rozet | eşlendi |
| `03 §3.5.2.14` | Ödenmemiş siparişte hizmet kalemi "tamamlandı" işaretlenemez: panel işareti sunmaz | E-37 | eşlendi |
| `03 §3.5.2.15` | Aynı siparişte çakışan işlemler: önce tamamlanan uygulanır, sonraki uygulanmaz ve panel güncel hâli — güncel IBAN dahil — gösterir | E-37 · ortak bileşen: eşzamanlı | eşlendi |
| `03 §3.5.2.16` | Gecikme feshinden sonra kalem kargoya verilmez, yeniden gönderilmez ve iptal edilmez; dönen gönderide yönetici kapanışı yapar | E-37 · E-16 | eşlendi |
| `03 §3.5.2.17` | Kesintide havale süresi dolar: iptal ertelenir, yönetici o ana kadar "ödendi" işaretler, sipariş sayfası yeni son günü gösterir | E-37 · E-16 · ekran dışı — arka plan | eşlendi |

**§3.5.3 Hatalar — kurumsal içerik (11)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §3.5.3.1` | Zorunlu alanı eksik kayıt yayına alınamaz; panel eksik alanı gösterir | E-41 · ortak bileşen: mesaj | eşlendi |
| `03 §3.5.3.2` | Hakkımızda silinmez, yalnız taslağa alınır | E-41 | eşlendi |
| `03 §3.5.3.3` | Hakkımızda boş ya da yayında değilse ana sayfanın kurumsal bloğu marka adını ve logoyu gösterir | E-01 | eşlendi |
| `03 §3.5.3.4` | İçeriğe bağlı ürün taslağa ya da arşive alınınca bağda kalır ama görünmez; silinen ürün bağdan kalkar | E-08 · E-07 · E-41 | eşlendi |
| `03 §3.5.3.5` | Silinmiş ya da taslaktaki içeriğin adresi "sayfa bulunamadı" döner | E-06 | eşlendi |
| `03 §3.5.3.6` | İki yönetici aynı içerik kaydını düzenlerse ikinci kaydeden uyarılır | E-41 · ortak bileşen: eşzamanlı | eşlendi |
| `03 §3.5.3.7` | Botun doldurduğu iletişim formu sessizce düşer; talep kaydı oluşmaz | E-10 · ekran dışı — arka plan | eşlendi |
| `03 §3.5.3.8` | Aynı IP'den çok sayıda form gönderimi geçici olarak engellenir; mesaj nötrdür | E-10 · ortak bileşen: mesaj | eşlendi |
| `03 §3.5.3.9` | Aydınlatma metni tamamlanmamışken iletişim formu çalışmaz | E-10 | eşlendi |
| `03 §3.5.3.10` | Müşterinin firma cevabına yanıtı firmanın e-postasına düşer; sisteme girmez | ekran dışı — ürünün dışında | ekran dışı |
| `03 §3.5.3.11` | Yayındaki kaydın zorunlu alanı boşaltılamaz; panel önce taslağa almaya yönlendirir | E-41 · ortak bileşen: mesaj | eşlendi |

**§3.5.4 Hatalar — mağaza ayarları (16)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §3.5.4.1` | Firma tipinin zorunlu kimlik alanları eksikse ya da tip değişmişse satış açılmaz | E-44 · E-31 · ortak bileşen: satış-kapalı | eşlendi |
| `03 §3.5.4.2` | IBAN boşken havale açılamaz | E-45 · ortak bileşen: mesaj | eşlendi |
| `03 §3.5.4.3` | Sağlayıcı anahtarları kurulumda tanımlı değilse kart yöntemi görünmez | E-45 · E-13 | eşlendi |
| `03 §3.5.4.4` | Hiçbir ödeme yöntemi açık değilse satış açılmaz | E-45 · E-31 · E-44 · ortak bileşen: satış-kapalı | eşlendi |
| `03 §3.5.4.5` | Çitlerin dışındaki havale ödeme süresi ya da kargoya verme süresi kabul edilmez; panel sebebini söyler | E-46 · ortak bileşen: mesaj | eşlendi |
| `03 §3.5.4.6` | Marka adı girilmemişse site alan adını gösterir | E-43 · ortak bileşen: çerçeve | eşlendi |
| `03 §3.5.4.7` | Logo yüklenmemişse marka adı yazıyla gösterilir | E-43 · ortak bileşen: çerçeve | eşlendi |
| `03 §3.5.4.8` | Site simgesi yüklenmemişse marka adının baş harfi marka rengi zeminde gösterilir | E-43 · ortak bileşen: çerçeve | eşlendi |
| `03 §3.5.4.9` | Marka rengi seçilmemişse varsayılan renk kullanılır; üstündeki yazının rengini sistem seçer | E-43 · ortak bileşen: taban | eşlendi |
| `03 §3.5.4.10` | Aydınlatma metni, çerez politikası ya da iade adresi eksikse satış açılmaz; panel eksik olanı adıyla gösterir | E-47 · E-46 · E-31 · E-44 · ortak bileşen: satış-kapalı | eşlendi |
| `03 §3.5.4.11` | Son yönetici ve yöneticinin kendi hesabı kaldırılamaz; panel sebebini söyler | E-49 · ortak bileşen: mesaj | eşlendi |
| `03 §3.5.4.12` | Geçersizleşmiş davet bağlantısı hesap açmaz; tıklayan süresi dolmuş davetin mesajını görür | E-30 · ortak bileşen: mesaj | eşlendi |
| `03 §3.5.4.13` | SSS ya da sayfa metni ayarla çelişirse ürün tespit etmez; ilgili ayarın yanında hatırlatma satırı durur | E-45 · E-46 | eşlendi |
| `03 §3.5.4.14` | Ödemesi beklenen havale siparişleri varken IBAN değişir: onay sipariş sayısını söyler; eski hesap kullanılamıyorsa siparişler firma iptaliyle kapatılır | E-45 · E-37 · ortak bileşen: onay | eşlendi |
| `03 §3.5.4.15` | Ödemesi beklenen havale siparişleri varken havale kapatılır ya da kapının bir koşulu düşer: açık siparişler sürer; panel sipariş sayısını söyler | E-45 · E-44 · E-37 · ortak bileşen: onay | eşlendi |
| `03 §3.5.4.16` | İade malı beklenen kalemler varken iade adresi değişir: yapılmış beyanın adresi değişmez; panel bekleyen kalem sayısını söyler | E-46 · E-16 · E-18 · E-19 · ortak bileşen: onay | eşlendi |

**§6.3 Firmanın inceleme akışları (9)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §6.3.1.1` | Yönetici stoğun ya da kupon haklarının açık havale siparişlerince kilitlendiğini görür: sayaçtan süzülmüş listeye gider; stok alanında ayrılmış adet ve siparişler görünür | E-31 · E-36 · E-33 | eşlendi |
| `03 §6.3.1.2` | Yönetici şüpheli siparişleri firma iptaliyle ("diğer", açıklamayla) kapatır; ayrılanlar serbest kalır | E-37 · ortak bileşen: onay · ortak bileşen: sebep | eşlendi |
| `03 §6.3.1.3` | Yönetici havaleyi geçici olarak kapatır; kart açık kalır, açık siparişler etkilenmez | E-45 · ortak bileşen: onay | eşlendi |
| `03 §6.3.2.1` | Firma ihlalden şüphelenir: ürün ihlali tespit etmez; panelde ihlal ekranı yoktur, bildirim firmanın yükümlülüğüdür | yoktur — kapsam dışı · ekran dışı — ürünün dışında | ekran dışı |
| `03 §6.3.2.2` | Yönetici işlem izini tarih ve yönetici süzgeciyle okur; iz dışa aktarmaları da söyler | E-52 | eşlendi |
| `03 §6.3.2.3` | Giriş kaydı panelde görünmez; sistem kayıtlarıyla birlikte barındırma tarafında okunur | yoktur — kapsam dışı · ekran dışı — mimari (`05`) | ekran dışı |
| `03 §6.3.2.4` | Yönetici ele geçirilmiş yönetici hesabını kaldırır; oturumları sonlanır, davetleri düşer | E-49 · ortak bileşen: onay | eşlendi |
| `03 §6.3.3.1` | Yönetici etkilenen kişilerin listesini çıkarır: sipariş dışa aktarması, üye listesi ve iletişim talepleri | E-53 | eşlendi |
| `03 §6.3.3.2` | Firma kişilere kendi e-posta aracıyla yazar; sitedeki araçları duyuru şeridi ve genel sayfadır | ekran dışı — ürünün dışında · E-41 | eşlendi |

**§7.1 Bildirimler ve tetikleyicileri (54)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §7.1.1` | B-1 Sipariş alındı — sipariş oluşur; e-posta sipariş sayfasının bağlantısını ve donan form ile sözleşmeyi taşır; yeniden gönderilebilen e-postadır | ekran dışı — e-posta (`08`) · E-16 · E-36 · E-37 | eşlendi |
| `03 §7.1.2` | B-1 yeni adrese — yönetici misafir siparişinin e-postasını düzeltir; bağlantı yeni erişim anahtarını taşır | ekran dışı — e-posta (`08`) · E-16 · E-37 | eşlendi |
| `03 §7.1.3` | B-2 Havale/EFT seçildi — sipariş havaleyle oluşur; siparişe donan IBAN ve sipariş numarası | ekran dışı — e-posta (`08`) · E-14 · E-16 | eşlendi |
| `03 §7.1.4` | B-3 Havale ödeme hatırlatması — ödeme süresinin son iş gününün başı, süre başına bir kez | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.1.5` | B-3 — "ödendi" işaretinin düzeltilmesiyle yeniden başlayan sürenin son iş günü, bir kez daha | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.1.6` | B-3 — kesintide zamanı gelmiş hatırlatma site dönünce, süre hâlâ işliyorsa gider | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.1.7` | B-4 Ödeme onaylandı — kart ödemesi onaylanır; dijital kalem varsa indirmenin hazır olduğu aynı e-postada | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.1.8` | B-4 — yönetici havale "ödendi" işaretini koyar | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.1.9` | B-5 Kargoya verildi — kargo şirketi, takip numarası ve "Takip et" bağlantısı ya da "kendi aracımızla teslim" beyanı | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.1.10` | B-5 — yönetici açık fiziksel kalemleri yeniden gönderir; yeni takip bilgisiyle | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.1.11` | B-6 Teslim edilemedi — yönetici geri dönen gönderiyi işaretler | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.1.12` | B-7 İptal edildi — müşteri ödeme beklenirken iptal eder; kendi işleminin kayıt kopyası | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.1.13` | B-7 — müşteri ödenmiş siparişte kalemi iptal eder; kayıt kopyası | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.1.14` | B-7 — yönetici sebep seçerek iptal eder; ifanın imkânsızlaşmasının yasal bildirimi, iptal anında | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.1.15` | B-7 — sistem siparişi kendiliğinden iptal eder; iptal ve sebebi | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.1.16` | B-8 Geri ödeme işlendi — sistemin başlattığı kart iadesi sağlayıcıda gerçekleşir | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.1.17` | B-8 — yönetici geri ödemeyi işler: havale hattı, caymada kart hattı, tutar bazlı geri ödeme, ayıbın çözümü | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.1.18` | B-8 — yönetici kart iadesini yeniden dener ve iade gerçekleşir | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.1.19` | B-8 — sistem iptal edilmiş siparişe gelen kart ödemesini kendiliğinden geri öder | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.1.20` | B-9 Cayma beyanı alındı — müşteri sipariş sayfasından beyan eder; tarih damgası ve iade adresi | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.1.21` | B-9 — yönetici başka kanaldan gelen cayma bildirimini kaydeder; beyanın tarihi bildirimin ulaştığı tarihtir | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.1.22` | B-9 Gecikme feshi alındı — müşteri fesheder ya da yönetici fesih bildirimini kaydeder | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.1.23` | B-9 Ayıp talebi alındı — müşteri ayıp talebi açar; tarih damgası ve iade adresi | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.1.24` | B-9 — müşteri çözülmüş ayıp talebini yeniden açar | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.1.25` | B-10 Adres düzeltildi — düzeltilen adres; istemediyse firmaya ulaşması gerektiği | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.1.26` | B-11 Kalem çıkarıldı — yönetici kalemi çıkarır ya da adedini azaltır | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.1.27` | B-12 Takip bilgisi düzeltildi — düzeltilmiş takip bilgisi | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.1.28` | B-13 Durum düzeltildi — düzeltilmiş durum; havale işaretinin düzeltilmesinde havale bilgisi ve yeni son ödeme günü | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.1.29` | B-14 IBAN gerekli — IBAN silindikten sonra iade malı ulaşır; e-posta sipariş sayfasının bağlantısını taşır ve IBAN oradan girilir | ekran dışı — e-posta (`08`) · E-16 · E-31 · ortak bileşen: IBAN | eşlendi |
| `03 §7.1.30` | B-14 — kart iadesi gerçekleşmez ve yönetici havale yolunu açar; IBAN girmek müşterinin seçimidir; bağlantı | ekran dışı — e-posta (`08`) · E-16 · E-31 · ortak bileşen: IBAN | eşlendi |
| `03 §7.1.31` | B-14 — yönetici geri ödeme havalesinin bankada gerçekleşmediğini bildirir; bağlantı | ekran dışı — e-posta (`08`) · E-16 · E-31 · ortak bileşen: IBAN | eşlendi |
| `03 §7.1.32` | B-14 — yönetici havale hattında IBAN'sız cayma ya da gecikme feshi kaydı yapar; bağlantı | ekran dışı — e-posta (`08`) · E-16 · E-31 · ortak bileşen: IBAN | eşlendi |
| `03 §7.1.33` | B-14 — yönetici IBAN'ı olmayan bir geri ödeme için IBAN ister; bağlantı | ekran dışı — e-posta (`08`) · E-16 · E-31 · ortak bileşen: IBAN | eşlendi |
| `03 §7.1.34` | B-14 — havale hattında ödenmiş kalemde firma iptali ya da kalem çıkarması; bağlantı | ekran dışı — e-posta (`08`) · E-16 · E-31 · ortak bileşen: IBAN | eşlendi |
| `03 §7.1.35` | B-15 E-posta adresi değiştirildi — eski adrese; yeni adresi ve bağlantıyı taşımaz | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.1.36` | B-16 İade reddedildi — reddedilen kalem ve sebebi, geri ödeme yapılmayacağı, uyuşmazlık yolları | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.1.37` | Yeniden gönderim — yönetici "e-posta ulaşmadı" işaretli sipariş satırından e-postayı yeniden gönderir; işareti düşüren on altı müşteri bildiriminin hepsi yeniden gönderilir (UI9-01; K-759) | ekran dışı — e-posta (`08`) · E-36 · E-37 | eşlendi |
| `03 §7.1.38` | F-1 Yeni sipariş — firmaya; ulaşmazsa işaretin biçimi `04`'ün işidir (UI9-01) | ekran dışı — e-posta (`08`) · E-36 | eşlendi |
| `03 §7.1.39` | F-2 Yeni iletişim talebi — KVKK talebi dahil; firmaya; işaretin biçimi UI9-01 | ekran dışı — e-posta (`08`) · E-38 · E-39 | eşlendi |
| `03 §7.1.40` | F-3 Yeni cayma beyanı — müşterinin beyanında firmaya; işaretin biçimi UI9-01 | ekran dışı — e-posta (`08`) · E-36 | eşlendi |
| `03 §7.1.41` | F-3 Yeni gecikme feshi — müşterinin feshinde firmaya; işaretin biçimi UI9-01 | ekran dışı — e-posta (`08`) · E-36 | eşlendi |
| `03 §7.1.42` | F-3 Yeni ayıp talebi — firmaya; işaretin biçimi UI9-01 | ekran dışı — e-posta (`08`) · E-38 | eşlendi |
| `03 §7.1.43` | F-3 — müşteri çözülmüş ayıp talebini yeniden açar; firma yeni talep gibi öğrenir; işaretin biçimi UI9-01 | ekran dışı — e-posta (`08`) · E-38 | eşlendi |
| `03 §7.1.44` | F-4 Kendiliğinden kart iadesi — sistemin karta yaptığı geri ödeme firmaya bildirilir; işaretin biçimi UI9-01 | ekran dışı — e-posta (`08`) · E-36 | eşlendi |
| `03 §7.1.45` | F-4 — sistem iptal edilmiş siparişe gelen kart ödemesini geri öder; işaretin biçimi UI9-01 | ekran dışı — e-posta (`08`) · E-36 | eşlendi |
| `03 §7.1.46` | F-5 Havale IBAN'ı değişti — bütün yöneticilerin kendi adreslerine; işaret düşmez | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.1.47` | F-6 Yönetici hesapları değişti — davet gönderilir; bütün yöneticilere; işaret düşmez | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.1.48` | F-6 — bir yönetici kaldırılır; bütün yöneticilere, kaldırılan dahil | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.1.49` | E-posta doğrulama bağlantısı — hesap kaydı ve bağlantının yeniden istenmesi; bağlantı iniş ekranına götürür | ekran dışı — e-posta (`08`) · E-21 · E-20 | eşlendi |
| `03 §7.1.50` | Şifre sıfırlama bağlantısı — müşteri ve yönetici hesabı, şifre belirleme ve geri almadan sonraki sıfırlama | ekran dışı — e-posta (`08`) · E-23 · E-29 · E-28 | eşlendi |
| `03 §7.1.51` | Yeni e-posta adresinin doğrulanması — müşteri ve yönetici hesabı; yeni adrese doğrulama bağlantısı | ekran dışı — e-posta (`08`) · E-21 · E-54 | eşlendi |
| `03 §7.1.52` | E-posta değişikliğinin eski adrese bildirimi — "bu değişikliği ben yapmadım" bağlantısıyla | ekran dışı — e-posta (`08`) · E-28 · E-54 | eşlendi |
| `03 §7.1.53` | Yönetici daveti — tek kullanımlık davet bağlantısı; ulaşmazsa davet satırına işaret düşer | ekran dışı — e-posta (`08`) · E-30 · E-49 | eşlendi |
| `03 §7.1.54` | Hesabın silindiği bildirimi — silinen hesabın adresine; bağlantı ve geri alma yolu taşımaz | ekran dışı — e-posta (`08`) | ekran dışı |

**§7.2 Bildirim üretmeyen olaylar ve olmayan bildirimler (30)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §7.2.1` | Hazırlanıyor'a geçiş bildirim üretmez — ödeme onayının e-postası gitmiştir | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.2.2` | Teslim işareti bildirim üretmez — tarih çoğu zaman geriye dönük girilir | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.2.3` | Teslim ve ulaşma tarihinin düzeltilmesi ve iç not bildirim üretmez | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.2.4` | İade malının teslim alınması bildirim üretmez — IBAN isteğinin düştüğü hâl dışında | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.2.5` | "Mal dönmedi" kapanışı bildirim üretmez | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.2.6` | Havale hattında IBAN'ın silinmesi bildirim üretmez | ekran dışı — e-posta (`08`) · ekran dışı — arka plan | ekran dışı |
| `03 §7.2.7` | Ayıp talebinin "Çözüldü" işareti ve yöneticinin yeniden açması bildirim üretmez | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.2.8` | Kart iadesinin sağlayıcıda başarısız olması bildirim üretmez; firma panel işaretini ve uyarıyı görür | ekran dışı — e-posta (`08`) · E-36 · E-37 · ortak bileşen: rozet | eşlendi |
| `03 §7.2.9` | Kart son sorgusunun yanıtsız kalması bildirim üretmez; sonuç gelince olağan bildirim gider | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.2.10` | İndirme hakkının yenilenmesi bildirim üretmez — cevap firmanın e-postasıdır | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.2.11` | Dijital dosyanın güncellenmesi bildirim üretmez; yeni hâl sipariş sayfasından iner | ekran dışı — e-posta (`08`) · E-16 | eşlendi |
| `03 §7.2.12` | Doğrulanmamış kaydın süre dolunca silinmesi bildirim üretmez | ekran dışı — e-posta (`08`) · ekran dışı — arka plan | ekran dışı |
| `03 §7.2.13` | Açık kalemi kalmamış siparişin kapanışında B-7 gitmez — müşteri olayın kendi bildirimini almıştır | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.2.14` | Havale süresinin kesintide dolması ve iptalin ertelenmesi bildirilmez; sipariş sayfası yeni son günü gösterir | ekran dışı — e-posta (`08`) · E-16 | eşlendi |
| `03 §7.2.15` | Sürelerin aşılması ve bekleyen işler e-postayla hatırlatılmaz; panel sayaçlarla gösterir | ekran dışı — e-posta (`08`) · E-31 · ortak bileşen: süre | eşlendi |
| `03 §7.2.16` | Firmanın panelde kendi yaptığı işlem firmaya bildirilmez; panelde görünür — F-5 ve F-6 dışında | ekran dışı — e-posta (`08`) · E-36 · E-37 | eşlendi |
| `03 §7.2.17` | Müşterinin kendi iptali firmaya bildirilmez; havale geri ödemesi sayaca düşer | ekran dışı — e-posta (`08`) · E-31 | eşlendi |
| `03 §7.2.18` | İletişim talebinin kapatılması ve yeniden açılması bildirilmez; talep müşteriye görünmez | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.2.19` | İletişim formunu gönderene alındı e-postası gitmez | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.2.20` | Davetin geri çekilmesi ve süresinin dolması davetliye bildirilmez | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.2.21` | Duyuru sitede durur, kimseye iletilmez | ekran dışı — e-posta (`08`) · ortak bileşen: çerçeve | eşlendi |
| `03 §7.2.22` | Yasal metnin değişmesi bildirilmez; güncel metin altbilgidedir | ekran dışı — e-posta (`08`) · ortak bileşen: çerçeve · E-11 | eşlendi |
| `03 §7.2.23` | Pazarlama iletisi, terk edilmiş sepet hatırlatması ve "stokta haber ver" yoktur | yoktur — kapsam dışı | ekran dışı |
| `03 §7.2.24` | Veri ihlalinde toplu bildirim yoktur; firma kendi e-posta aracıyla ulaşır | yoktur — kapsam dışı · ekran dışı — ürünün dışında | ekran dışı |
| `03 §7.2.25` | Bildirim tercihi yoktur: işlem bildirimleri kapatılamaz | yoktur — kapsam dışı | ekran dışı |
| `03 §7.2.26` | Hesap olayları — doğrulama, giriş, çıkış, şifre, ad, adres defteri, davetlinin hesabı açması — bildirim üretmez; kullanıcının kendi ekranında tamamladığı işlemlerdir | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.2.27` | Kurulum tarafının panele erişimi kurtarması panel işlemi değildir; ize yazılmaz ve bildirim üretmez | ekran dışı — kurulum (`10 §4.1`) | ekran dışı |
| `03 §7.2.28` | Dış servisin ya da sitenin kesintisi ve normale dönüş bildirilmez; firmanın yolu duyuru şerididir | ekran dışı — e-posta (`08`) · E-41 | eşlendi |
| `03 §7.2.29` | Periyodik imha, imha kaydı ve e-postanın yeniden denenmesi sistemin iç işidir; ulaşmayan e-postada panel işareti düşer | ekran dışı — arka plan · E-36 | eşlendi |
| `03 §7.2.30` | Müşterinin geri ödeme IBAN'ını girmesi ve düzeltmesi bildirim üretmez; sipariş sayfasının işlemidir | ekran dışı — e-posta (`08`) · E-16 | eşlendi |

**§7.3 Geçiş ve olay bazlı eşleme (47)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §7.3.1` | S1 Alındı → Hazırlanıyor — bildirim: Ayrıca yok — Ö1'in B-4'ü | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.3.2` | S2 Alındı → Teslim edildi — bildirim: Yalnız dijital siparişte ödeme onayında B-4; son teslim işaretinde yok; kapanışta olayın kendi bildirimi | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.3.3` | S3 Alındı → İptal edildi — bildirim: B-7; kapanışta yok — olayın B-9'u ya da B-11'i | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.3.4` | S4 Hazırlanıyor → İptal edildi — bildirim: B-7; kapanışta yok | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.3.5` | S5 Hazırlanıyor → Kargoya verildi — bildirim: B-5 | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.3.6` | S6 Kargoya verildi → Teslim edildi — bildirim: Yok | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.3.7` | S7 Kargoya verildi → Teslim edilemedi — bildirim: B-6 | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.3.8` | S8 Teslim edilemedi → Kargoya verildi — bildirim: B-5, yeniden | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.3.9` | S9 Teslim edilemedi → İptal edildi — bildirim: B-7; kapanışta yok | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.3.10` | S10 Hazırlanıyor → Teslim edildi — bildirim: Olayın kendi bildirimi — B-7, B-9, B-11 ya da hizmet tamamlamada yok | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.3.11` | S11 Teslim edilemedi → Teslim edildi — bildirim: Olayın kendi bildirimi — S7'de B-6 | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.3.12` | Ö1 Bekliyor → Ödendi — bildirim: B-4 | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.3.13` | Ö2 Bekliyor → Başarısız — bildirim: S3'ün B-7'si | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.3.14` | Ö3–Ö5 geri ödeme geçişleri — bildirim: B-8; sistemin kart iadesinde ayrıca F-4 | ekran dışı — e-posta (`08`) · E-36 | eşlendi |
| `03 §7.3.15` | İptal edilmiş siparişe gelen kart ödemesinin geri ödenmesi — eksen değişmez — bildirim: B-8 · F-4 | ekran dışı — e-posta (`08`) · E-36 | eşlendi |
| `03 §7.3.16` | Kart iadesinin başarısız olması — eksen değişmez — bildirim: Yok | ekran dışı — e-posta (`08`) · E-36 · E-37 · ortak bileşen: rozet | eşlendi |
| `03 §7.3.17` | Sipariş oluşur — bildirim: B-1 · havalede B-2 · F-1 · aynı sepetin önceki siparişinde B-7 | ekran dışı — e-posta (`08`) · E-16 · E-36 · E-37 · E-14 | eşlendi |
| `03 §7.3.18` | Havale hatırlatması (Z-9) — bildirim: B-3 | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.3.19` | Kart son sorgusu yanıtsız kalır · havale süresi kesintide dolar — bildirim: Yok | ekran dışı — e-posta (`08`) · E-16 | eşlendi |
| `03 §7.3.20` | IBAN'ın silinmesi · periyodik imha · e-postanın yeniden denenmesi · doğrulanmamış kaydın silinmesi — bildirim: Yok — e-posta ulaşmazsa panel işareti | ekran dışı — e-posta (`08`) · ekran dışı — arka plan · E-36 | eşlendi |
| `03 §7.3.21` | İptal kaydı — bildirim: B-7 · kart hattında ödenmiş kalemde B-8 ve F-4 · havale hattında firma iptalinde B-14 | ekran dışı — e-posta (`08`) · E-16 · E-31 · ortak bileşen: IBAN · E-36 | eşlendi |
| `03 §7.3.22` | Çıkarma kaydı — bildirim: B-11 · kartta B-8 ve F-4 · havale hattında B-14 | ekran dışı — e-posta (`08`) · E-16 · E-31 · ortak bileşen: IBAN · E-36 | eşlendi |
| `03 §7.3.23` | Gecikme feshi — bildirim: B-9 · müşterinin feshinde F-3 · kartta B-8 ve F-4 · havale hattında IBAN'sız kayıtta B-14 | ekran dışı — e-posta (`08`) · E-16 · E-31 · ortak bileşen: IBAN · E-36 | eşlendi |
| `03 §7.3.24` | Cayma beyanı — bildirim: B-9; müşterinin beyanında F-3; IBAN'sız kayıtta B-14 | ekran dışı — e-posta (`08`) · E-36 · E-16 · E-31 · ortak bileşen: IBAN | eşlendi |
| `03 §7.3.25` | İade teslim alma — bildirim: Yok; IBAN silinmişse B-14 | ekran dışı — e-posta (`08`) · E-16 · E-31 · ortak bileşen: IBAN | eşlendi |
| `03 §7.3.26` | İade reddi — bildirim: B-16 | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.3.27` | "Mal dönmedi" kapanışı — bildirim: Yok | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.3.28` | IBAN isteği — bildirim: B-14 | ekran dışı — e-posta (`08`) · E-16 · E-31 · ortak bileşen: IBAN | eşlendi |
| `03 §7.3.29` | Ayıp talebi — doğuş ve müşterinin yeniden açması — bildirim: B-9 · F-3 | ekran dışı — e-posta (`08`) · E-38 | eşlendi |
| `03 §7.3.30` | Ayıp talebi — "Çözüldü" işareti ve yöneticinin yeniden açması — bildirim: Yok | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.3.31` | İndirme sayacı — yenileme · dosya güncellemesi — bildirim: Yok | ekran dışı — e-posta (`08`) · E-16 | eşlendi |
| `03 §7.3.32` | Adres düzeltmesi — bildirim: B-10 | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.3.33` | Takip bilgisinin düzeltilmesi — bildirim: B-12 | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.3.34` | Teslim tarihinin ve iade malının ulaşma tarihinin düzeltilmesi · iç not — bildirim: Yok | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.3.35` | Durum geçişinin düzeltilmesi — bildirim: B-13 · havale işaretinde ayrıca B-3 | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.3.36` | Misafir siparişinin e-posta düzeltmesi — bildirim: B-1 → yeni adres · B-15 → eski adres | ekran dışı — e-posta (`08`) · E-16 · E-37 | eşlendi |
| `03 §7.3.37` | İletişim talebi doğar — bildirim: F-2 | ekran dışı — e-posta (`08`) · E-38 · E-39 | eşlendi |
| `03 §7.3.38` | İletişim talebi kapatılır ya da yeniden açılır — bildirim: Yok | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.3.39` | Havale IBAN'ının girilmesi ya da değişmesi — bildirim: F-5 | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.3.40` | Yönetici daveti gönderilir · bir yönetici kaldırılır — bildirim: F-6 · davette davet e-postası | ekran dışı — e-posta (`08`) · E-30 · E-49 | eşlendi |
| `03 §7.3.41` | Davet geri çekilir ya da süresi dolar — bildirim: Yok | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.3.42` | Katalog, içerik, marka, ayar ve yayın durumu değişiklikleri · satışın kapatılması · duyuru — bildirim: Yok | ekran dışı — e-posta (`08`) · E-36 · E-37 · ortak bileşen: çerçeve · E-11 | eşlendi |
| `03 §7.3.43` | Hesap kaydı, şifre sıfırlama ve e-posta değişikliği — bildirim: Hesap ve yönetim e-postaları | ekran dışı — e-posta (`08`) · E-21 · E-20 · E-23 · E-29 · E-28 · E-54 | eşlendi |
| `03 §7.3.44` | Ulaşmayan e-postanın yeniden gönderilmesi — bildirim: İlk bildirim, yeniden | ekran dışı — e-posta (`08`) · E-36 · E-37 | eşlendi |
| `03 §7.3.45` | Hesabın silinmesi — üyenin ya da yöneticinin — bildirim: Hesabın silindiği bildirimi | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.3.46` | Hesabın doğrulanması, Google ile açılması ve bağlanması, giriş ve çıkış, şifrenin değiştirilmesi, ad ve adres defteri, davetlinin hesabı açması — bildirim: Yok | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §7.3.47` | Kurulum tarafının kurtarması · dış servisin ya da sitenin kesintisi — bildirim: Yok | ekran dışı — kurulum (`10 §4.1`) · ekran dışı — e-posta (`08`) · E-41 | eşlendi |

**§8.1 Katalog yönetimi (15)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §8.1.1` | Yönetici ürün oluşturur ya da düzenler: tip, ad, kategoriler ve ana kategori; yeni ürün Taslak doğar; aynı anda düzenlemede ikinci kaydeden uyarılır | E-32 · E-33 · ortak bileşen: eşzamanlı (GAP-9) | eşlendi |
| `03 §8.1.2` | Yönetici varyantları tanımlar: en fazla iki seçenek boyutu, tekil stok kodu, KDV dahil fiyat; varyant kendi yayın durumunu taşır | E-33 | eşlendi |
| `03 §8.1.3` | Yönetici stoğu ya da kontenjanı girer; alanın yanında ayrılmış adet ve onu tutan siparişler görünür; ayrılmış adedin altına inen değer reddedilir | E-33 · E-37 · ortak bileşen: mesaj | eşlendi |
| `03 §8.1.4` | Yönetici tipe özgü alanları girer: üretim yeri, ölçü birimi ve hatırlatması, kargoya verme süresi, cayma istisnası ve sebebi, ifa süresi, siparişte en fazla adet | E-33 | eşlendi |
| `03 §8.1.5` | Yönetici görselleri yükler, sıralar, ana görseli işaretler ve açıklamayı yazar; alternatif metin boşsa sistem üretir | E-33 | eşlendi |
| `03 §8.1.6` | Yönetici dijital ürünün dosyasını yükler ya da günceller; güncel dosya geçmiş alıcılara da iner; yayındaki varyantı dosyasız bırakan silme engellenir | E-33 · E-16 | eşlendi |
| `03 §8.1.7` | Yönetici taslak ürünü sitede "Taslak" bandıyla önizler ve yayına alır; eksik yayın koşulu panelde gösterilir | E-33 · E-04 · ortak bileşen: taslak (GAP-6) | eşlendi |
| `03 §8.1.8` | Yönetici ürünü ya da varyantı taslağa, arşive ya da yeniden yayına alır; son Yayında varyant engellenir; arşivlenen adres "artık satılmıyor" sayfasını döner | E-32 · E-33 · E-05 | eşlendi |
| `03 §8.1.9` | Yönetici ürünü ya da varyantı kalıcı olarak siler; onaydan önce panel açık sipariş sayısını söyler | E-33 · ortak bileşen: onay | eşlendi |
| `03 §8.1.10` | Yönetici varyantın fiyatını değiştirir; fiyat geçmişe yazılır, sepette "fiyatı değişti" satırı görünür | E-33 · E-12 | eşlendi |
| `03 §8.1.11` | Yönetici ürüne tarihli, yüzde olarak indirim tanımlar; referans fiyatı sistem hesaplar, kaydedilmeyen indirimin sebebini panel söyler | E-33 · ortak bileşen: mesaj | eşlendi |
| `03 §8.1.12` | Yönetici kupon tanımlar: yüzde ya da sabit tutar, tarih aralığı, toplam kullanım adedi | E-35 | eşlendi |
| `03 §8.1.13` | Yönetici kuponun kullanım adedini değiştirir ya da kampanyayı durdurur; panel kullanılmış ve ayrılmış hakları gösterir | E-35 · ortak bileşen: mesaj | eşlendi |
| `03 §8.1.14` | Yönetici kategori ağacını kurar, kategoriyi taşır ya da siler; en fazla üç seviye; silmeyi engelleyenler sayısıyla gösterilir | E-34 · ortak bileşen: mesaj | eşlendi |
| `03 §8.1.15` | Toplu yükleme, liste ekranından toplu fiyat ve stok güncellemesi ve elle sıralama yoktur; ürünler tek tek yönetilir | yoktur — kapsam dışı · E-32 | eşlendi |

**§8.2 Sipariş yürütümü: olağan hat (11)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §8.2.1` | Yönetici yeni siparişi öğrenir: firmaya bildirim gider, sipariş listededir, havale siparişi "ödeme onayı bekleyen" sayacındadır | E-36 · E-31 · ekran dışı — e-posta (`08`) | eşlendi |
| `03 §8.2.2` | Yönetici havale parasını görür ve "ödendi" işaretler: panel donmuş IBAN'ı ve tutarı gösterir, gelen tutarı sormaz; onay geri alınamaz | E-37 · ortak bileşen: onay | eşlendi |
| `03 §8.2.3` | Sistem ödeme onayını işler: ayrılan kesin düşer, süreler başlar, dijital kalem teslim edilir, sipariş ilgili sayaca girer | ekran dışı — arka plan · E-31 · E-37 | eşlendi |
| `03 §8.2.4` | Yönetici siparişi hazırlar ve faturayı kendi aracıyla keser; panel kargoya verme sözünü ve kalan süreyi gösterir, süre aşılınca kanuni faiz uyarısı | E-37 · ortak bileşen: süre · ekran dışı — ürünün dışında | eşlendi |
| `03 §8.2.5` | Yönetici siparişi kargoya verir: kargo şirketi ve takip numarası ya da "kendi aracımızla teslim" beyanı; onay geri alınamaz | E-37 · ortak bileşen: onay | eşlendi |
| `03 §8.2.6` | Yönetici teslim işaretini ve teslim tarihini girer; ileri ve kargodan önceki tarih reddedilir; cayma günüyle çakışırsa panel sırayı sorar | E-37 · ortak bileşen: tarih · E-31 | eşlendi |
| `03 §8.2.7` | Yönetici geri dönen gönderiyi "teslim edilemedi" işaretler; sipariş yeniden "kargoya verilecek" sayacına girer | E-37 · E-31 | eşlendi |
| `03 §8.2.8` | Yönetici açık fiziksel kalemleri yeni takip bilgisiyle yeniden gönderir; onay geri alınamaz | E-37 · ortak bileşen: onay | eşlendi |
| `03 §8.2.9` | Yönetici hizmet kalemini "tamamlandı" işaretler; işaret ödeme onaylanmışken sunulur | E-37 | eşlendi |
| `03 §8.2.10` | Yönetici müşterinin başvurusu üzerine dijital kalemin indirme hakkını yeniler; başvuru iletişim formundan gelir | E-37 · E-39 | eşlendi |
| `03 §8.2.11` | Yönetici siparişi sipariş numarasıyla ya da iletişim e-postasıyla arar, durumuna göre süzer; ekranın biçimi `04`'ün işidir | E-36 · E-39 | eşlendi |

**§8.3 Firma iptali, iade teslim alma ve geri ödeme (16)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §8.3.1.1` | Yönetici siparişi ya da kalemi kapalı listeden sebep seçerek iptal eder; "stokta bulunamadı"da ve süre aşımında uyarı; onay geri alınamaz | E-37 · ortak bileşen: onay · ortak bileşen: sebep | eşlendi |
| `03 §8.3.1.2` | Müşteri kaynaklı dönen gönderide iptal: ürün bedeli geri ödenir, gidiş kargosu ödenmez; ayrım sebep kaydından okunur | E-37 · ortak bileşen: sebep | eşlendi |
| `03 §8.3.2.1` | Yönetici dönen malı teslim alır ve ulaşma tarihini girer; "iade malı bekleniyor" kalkar; yanlış kalemde teslim almanın geri alınması yoktur | E-37 · ortak bileşen: tarih | eşlendi |
| `03 §8.3.2.2` | Yönetici malı kontrol eder ve stoğa ekler; stok kendiliğinden dönmez, silinmiş varyantta ekleme sunulmaz | E-37 | eşlendi |
| `03 §8.3.2.3` | Yönetici koşullu istisna kaleminde koruyucu ambalajı açılmış malı reddeder; onay sonucu tek cümleyle söyler ve geri alınmaz | E-37 · ortak bileşen: onay | eşlendi |
| `03 §8.3.2.4` | Yönetici reddedilen malı müşteriye kendi bedeliyle geri gönderir; ürün izlemez | ekran dışı — ürünün dışında | ekran dışı |
| `03 §8.3.2.5` | Yönetici açık kalemi kalmamış ve gönderisi dönmüş siparişi kapatır; sebep seçilmez, iptal kaydı açılmaz | E-37 | eşlendi |
| `03 §8.3.3.1` | Sistem kart hattında geri ödemeyi kendiliğinden başlatır; onay penceresi yoktur | ekran dışı — arka plan · E-37 | eşlendi |
| `03 §8.3.3.2` | Yönetici havale hattındaki geri ödemeyi bankasından gönderir ve panelden işler; panel kalan süreyi ve müşterinin IBAN'ını gösterir; onay geri alınamaz | E-37 · ortak bileşen: onay · ortak bileşen: süre · ortak bileşen: IBAN | eşlendi |
| `03 §8.3.3.3` | Yönetici caymanın geri ödemesini kart hattında işler; onay geri alınamaz | E-37 · ortak bileşen: onay | eşlendi |
| `03 §8.3.3.4` | Kart iadesi sağlayıcıda gerçekleşmez: satıra "geri ödeme gerçekleşmedi" düşer, kalem sayaca girer ve panelde uyarı çıkar | E-36 · E-37 · E-31 · ortak bileşen: rozet | eşlendi |
| `03 §8.3.3.5` | Yönetici kart iadesini yeniden dener ya da müşteriye havale yolunu açar; sipariş sayfasında IBAN alanı açılır | E-37 · E-16 | eşlendi |
| `03 §8.3.3.6` | Yönetici IBAN'ı olmayan bir geri ödeme için müşteriden IBAN ister; istek "IBAN bekleniyor" listesinde görünür ve sayaçta sayılmaz | E-37 · E-31 · E-16 | eşlendi |
| `03 §8.3.3.7` | Yönetici tutar bazlı kısmi geri ödeme işler; tutar ödenmiş ve geri ödenmemiş tutarı aşamaz; onay geri alınamaz | E-37 · ortak bileşen: onay | eşlendi |
| `03 §8.3.3.8` | Yönetici bankada gerçekleşmeyen geri ödeme havalesini "havale gerçekleşmedi" diye bildirir; IBAN alanı yeniden açılır | E-37 · E-31 · E-16 | eşlendi |
| `03 §8.3.3.9` | Müşteri IBAN isteğini cevaplamaz: kalem "IBAN bekleniyor" listesinde kalır; yönetici siparişin iletişim bilgisini görür | E-31 · E-37 | eşlendi |

**§8.4 Sipariş müdahaleleri ve ayıp talebinin yönetimi (11)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §8.4.1` | Yönetici teslimat ya da fatura adresini müşterinin talebiyle düzeltir ve talebi siparişin kanalından teyit eder | E-37 · ortak bileşen: adres | eşlendi |
| `03 §8.4.2` | Yönetici kalem adedini azaltır ya da kalemi çıkarır; sebep listesi yoktur; kart hattında geri alınamaz onay | E-37 · ortak bileşen: onay | eşlendi |
| `03 §8.4.3` | Yönetici kargo şirketini ve takip numarasını düzeltir | E-37 | eşlendi |
| `03 §8.4.4` | Yönetici teslim tarihini ya da iade malının ulaşma tarihini düzeltir; ileri tarih reddedilir; beyan günüyle çakışırsa panel sırayı sorar | E-37 · ortak bileşen: tarih | eşlendi |
| `03 §8.4.5` | Yönetici yanlış durum geçişini kapalı listeden sebep seçerek düzeltir; panel "geri al" sunmaz | E-37 · ortak bileşen: sebep | eşlendi |
| `03 §8.4.6` | Yönetici siparişe iç not yazar; not müşteriye hiçbir yerde görünmez | E-37 | eşlendi |
| `03 §8.4.7` | Yönetici malı dönmemiş cayma kalemini "mal dönmedi" gerekçesiyle kapatır; gönderme süresi geçmişken sunulur | E-37 | eşlendi |
| `03 §8.4.8` | Yönetici başka kanaldan gelen cayma ya da gecikme feshi bildirimini kaydeder: kalem ve tarih; pencere dışında ayrıca onay; kayıt geri alınmaz | E-37 · ortak bileşen: onay · ortak bileşen: tarih · ortak bileşen: IBAN | eşlendi |
| `03 §8.4.9` | Yönetici misafir siparişinin iletişim e-postasını düzeltir; satırda "e-posta düzeltildi" işareti görünür; adresler e-posta geçmişinde durur | E-37 · E-36 · ortak bileşen: rozet | eşlendi |
| `03 §8.4.10` | Yönetici yeni ayıp talebini okur: talep kalem ve sipariş bağlamıyla panele düşer ve "açık talep" sayacındadır; çözüm sistemin dışında yürür | E-38 · E-37 · E-31 (GAP-7) | eşlendi |
| `03 §8.4.11` | Yönetici ayıp talebini "çözüldü" işaretler ya da yeniden açar; yeni talep açılmaz | E-38 · E-37 | eşlendi |

**§8.5 Manuel adım bütçesi (8)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §8.5.1` | Kart ile ödenen fiziksel sipariş: iki zorunlu elle adım — kargoya verme, teslim işareti | E-37 | eşlendi |
| `03 §8.5.2` | Havale ile ödenen fiziksel sipariş: üç zorunlu elle adım — "ödendi" işareti ve iki adım | E-37 | eşlendi |
| `03 §8.5.3` | Hizmet siparişi: bir zorunlu elle adım, havalede iki | E-37 | eşlendi |
| `03 §8.5.4` | Dijital sipariş: sıfır zorunlu elle adım, havalede bir | E-37 | eşlendi |
| `03 §8.5.5` | Karışık sipariş: hatların adımları toplanır; "ödendi" işareti bir kez sayılır | E-37 | eşlendi |
| `03 §8.5.6` | Koşullu adımlar: firma iptali, "teslim edilemedi" ve yeniden gönderme, iade teslim alma, elle geri ödeme, ayıbı "çözüldü" işaretleme, indirme hakkını yenileme | E-37 · E-38 | eşlendi |
| `03 §8.5.7` | Bütçeye girmeyen işler: müdahaleler, iade reddi, kart iadesinin yeniden denenmesi, IBAN isteği, yeniden gönderim, üye hesabının silinmesi | E-37 · E-36 · E-40 | eşlendi |
| `03 §8.5.8` | Sistem dışı adımlar: fatura, reddedilen malın geri gönderilmesi, paranın bankadan gönderilmesi | ekran dışı — ürünün dışında | ekran dışı |

**§8.6 Kurumsal içerik, marka, duyuru ve iletişim talepleri (17)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §8.6.1.1` | Yönetici içerik tipini seçer ve kaydı açar; kayıt Taslak doğar; Hakkımızda tek kayıttır | E-41 | eşlendi |
| `03 §8.6.1.2` | Yönetici içeriği girer: metin, görseller, video bağlantısı; hizmet tanıtımına ve referans işe ürün bağlar | E-41 | eşlendi |
| `03 §8.6.1.3` | Yönetici taslağı "Taslak" bandıyla önizler; kendi sayfası olmayan kayıtlar göründükleri yerde aynı işaretle önizlenir | ortak bileşen: taslak · E-08 · E-09 · E-10 · E-41 · ortak bileşen: çerçeve (GAP-6) | eşlendi |
| `03 §8.6.1.4` | Yönetici kaydı yayına alır; zorunlu alanlar dolmadan yayına alınmaz ve panel eksik alanı gösterir | E-41 · ortak bileşen: mesaj | eşlendi |
| `03 §8.6.1.5` | Yönetici kaydı ana sayfada ve menüde gösterir, listeleri elle sıralar | E-41 · E-42 | eşlendi |
| `03 §8.6.1.6` | Yönetici yayındaki kaydı düzenler; değişiklik anında yayındadır; yayın kapısını bozan düzenleme kaydedilmez | E-41 · ortak bileşen: eşzamanlı · ortak bileşen: mesaj | eşlendi |
| `03 §8.6.1.7` | Yönetici kaydı taslağa alır ya da geri alınamaz uyarısıyla siler; Hakkımızda silinmez | E-41 · ortak bileşen: onay | eşlendi |
| `03 §8.6.2.1` | Yönetici iki hazır ana sayfa düzeninden birini seçer | E-42 | eşlendi |
| `03 §8.6.2.2` | Yönetici menüdeki adları değiştirir; menünün iskeleti sabittir, menü düzenleyici yoktur | E-42 | eşlendi |
| `03 §8.6.2.3` | Yönetici sosyal medya bağlantılarını ve WhatsApp numarasını girer; platform listesi kapalıdır | E-42 | eşlendi |
| `03 §8.6.2.4` | Yönetici marka ayarlarını yapar: logo, marka adı, site simgesi, marka rengi; platform imzasını kapatır | E-43 | eşlendi |
| `03 §8.6.3.1` | Yönetici duyuruyu hazırlar, önizler ve yayına alır: tek kısa metin, isteğe bağlı bağlantı ve tarih aralığı | E-41 · ortak bileşen: taslak | eşlendi |
| `03 §8.6.3.2` | Sistem duyuruyu tarih aralığında gösterir; yönetici taslağa alarak da kaldırır | ekran dışı — arka plan · ortak bileşen: çerçeve · E-41 | eşlendi |
| `03 §8.6.4.1` | Yönetici yeni iletişim talebini öğrenir: talep panele düşer ve "açık talep" sayacındadır | E-38 · E-39 · E-31 | eşlendi |
| `03 §8.6.4.2` | Yönetici talebi okur ve kendi e-postasıyla cevap verir; panelde yanıt ekranı, yazışma dizisi ve hazır şablon yoktur | E-39 · E-37 · ekran dışı — ürünün dışında | eşlendi |
| `03 §8.6.4.3` | Yönetici KVKK talebini işler; süre sayacı yoktur; cevabın dayanağı sipariş listesi ve üye kaydı görünümüdür | E-39 · E-40 · E-36 | eşlendi |
| `03 §8.6.4.4` | Yönetici talebi kapatır ya da yeniden açar; talep müşteriye görünmez | E-39 | eşlendi |

**§8.7 Mağaza ayarları, satış kapısı ve kurulum kontrol listesi (15)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §8.7.1.1` | Yönetici panele ilk kez girer: site boş kurulur; ana sayfadaki kurulum kontrol listesi satış için eksik olanları adıyla sayar; sihirbaz yoktur, liste tamamlanınca kaybolur | E-31 · ortak bileşen: satış-kapalı | eşlendi |
| `03 §8.7.1.2` | Sistem satış kapısını sürekli denetler: bir koşul düşerse sepete ekleme ve ödeme kapanır, panel eksik koşulu adıyla gösterir | E-31 · E-44 · ortak bileşen: satış-kapalı · ekran dışı — arka plan | eşlendi |
| `03 §8.7.2.1` | Yönetici firma tipini seçer ve kimlik alanlarını doldurur; zorunlu alanlar boşaltılamaz; ETBİS alanının yanında hatırlatma; alan doluysa band sitede görünür | E-44 · ortak bileşen: çerçeve | eşlendi |
| `03 §8.7.2.2` | Yönetici aydınlatma metnini ve çerez politikasını ürünün taslağından düzenler ve yayına alır; sürüm kendiliğinden artar | E-47 | eşlendi |
| `03 §8.7.2.3` | Sistem Ön Bilgilendirme Formu'nu ve sözleşmeyi ayarlardan üretir; panel ilgili ayarın yanında metinlerle çelişki hatırlatması gösterir | ekran dışı — arka plan · E-47 · E-44 · E-46 · E-45 | eşlendi |
| `03 §8.7.3.1` | Yönetici havale/EFT'yi açar ve IBAN girer; kartın panelde anahtarı yoktur | E-45 | eşlendi |
| `03 §8.7.3.2` | Yönetici havale IBAN'ını değiştirir; onay ödemesi beklenen havale siparişlerinin sayısını söyler | E-45 · E-37 · ortak bileşen: onay | eşlendi |
| `03 §8.7.3.3` | Yönetici havaleyi kapatır; panel ödemesi beklenen havale siparişlerinin sayısını söyler; başka yöntem yoksa satış kapanır | E-45 · ortak bileşen: onay | eşlendi |
| `03 §8.7.4.1` | Yönetici kargo ücretini, ücretsiz kargo eşiğini ve asgari sipariş tutarını girer | E-46 | eşlendi |
| `03 §8.7.4.2` | Yönetici teslimat yaptığı illeri seçer | E-46 | eşlendi |
| `03 §8.7.4.3` | Yönetici kargoya verme süresinin varsayılanını ve havale ödeme süresini girer; çit dışındaki değer sebebiyle reddedilir | E-46 · ortak bileşen: mesaj | eşlendi |
| `03 §8.7.4.4` | Yönetici indirme hakkını ve KDV oranının varsayılanını değiştirir | E-46 (GAP-11) | eşlendi |
| `03 §8.7.4.5` | Yönetici iade adresini girer ya da değiştirir; değişiklikte panel "iade malı bekleniyor" kalemlerin sayısını söyler | E-46 · ortak bileşen: onay | eşlendi |
| `03 §8.7.5.1` | Yönetici satışı geçici olarak kapatır; vitrin yayında kalır, sepet korunur, açık siparişler yürür | E-44 · ortak bileşen: satış-kapalı | eşlendi |
| `03 §8.7.5.2` | Yönetici satışı yeniden açar; kapının öteki üç koşulu da sağlanıyorsa satış açılır | E-44 | eşlendi |

**§8.8 Yönetici hesapları ve davet (6)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §8.8.1` | Yönetici bir e-posta adresine davet gönderir; davet yönetici listesinde satır olarak görünür; e-posta ulaşmazsa satıra işaret düşer | E-49 · ortak bileşen: rozet | eşlendi |
| `03 §8.8.2` | Davetli bağlantıdan girer ve şifresini kurar; geçersiz bağlantıda süresi dolmuş davetin mesajını görür | E-30 · ortak bileşen: mesaj (GAP-8) | eşlendi |
| `03 §8.8.3` | Yönetici kullanılmamış daveti yönetici listesindeki satırından geri çeker | E-49 | eşlendi |
| `03 §8.8.4` | Sistem süresi dolan daveti geçersiz kılar; yönetici yeni davet gönderir | ekran dışı — arka plan · E-49 · E-30 | eşlendi |
| `03 §8.8.5` | Yönetici başka bir yöneticiyi kaldırır; son yönetici ve kendi hesabı kaldırılamaz; onay kullanılmamış davetlerin sayısını söyler | E-49 · ortak bileşen: onay · ortak bileşen: mesaj | eşlendi |
| `03 §8.8.6` | Davet yanlış adrese gitmiştir: yönetici daveti geri çeker ya da açılan hesabı kaldırır ve işlem izini okur | E-49 · E-52 | eşlendi |

**§8.9 Bekleyen işler, satış özeti, işlem izi ve dışa aktarma (7)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §8.9.1` | Yönetici ana sayfada bekleyen işleri görür: altı sayaç kendi süzülmüş listesine götürür; "IBAN bekleniyor" listesi ayrıdır; kanal uyarısı | E-31 · E-36 · E-38 · E-39 · ortak bileşen: süre | eşlendi |
| `03 §8.9.2` | Yönetici "e-posta ulaşmadı" işaretli sipariş satırından e-postayı yeniden gönderir; B-1…B-16'nın hepsi yeniden gönderilir, firma bildirimi satıra işaret düşürmez (UI9-01; K-759, K-760) | E-36 · E-37 · ortak bileşen: rozet | eşlendi |
| `03 §8.9.3` | Yönetici satış özetini seçtiği dönem için okur: sayılar, dağılım ve dört ölçü; paydası sıfır olan oran "değerlendirilemez" | E-51 | eşlendi |
| `03 §8.9.4` | Yönetici işlem izini tarih aralığı ve yönetici süzgeciyle okur; arama, gruplama ve dışa aktarma yoktur | E-52 | eşlendi |
| `03 §8.9.5` | Yönetici siparişleri tarih aralığıyla CSV olarak dışa aktarır | E-53 | eşlendi |
| `03 §8.9.6` | Yönetici üye listesini ve iletişim taleplerini dışa aktarır | E-53 · E-40 · E-38 | eşlendi |
| `03 §8.9.7` | Katalog, içerik, işlem izi ve satış özeti dışa aktarılmaz | yoktur — kapsam dışı · E-51 · E-52 · E-53 | eşlendi |

**§10.1 Dış servis kesintileri (17)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §10.1.1.1` | Sağlayıcıya erişilemezken kart "şu an kullanılamıyor" görünür ve seçilemez; havale açıksa müşteri ona yönelir | E-13 · ortak bileşen: mesaj | eşlendi |
| `03 §10.1.1.2` | Onaydan sonra ödeme sayfası açılmaz: sipariş kendiliğinden iptal edilir, sepet korunur | ekran dışı — arka plan · E-14 · E-12 | eşlendi |
| `03 §10.1.1.3` | Kart son sorgusu yanıtsız kalır: sipariş iptal edilmez, panelde "ödeme sonucu alınamadı" işareti düşer | ekran dışı — arka plan · E-36 · E-37 · E-14 · ortak bileşen: rozet | eşlendi |
| `03 §10.1.1.4` | Sağlayıcı döner: bekleyen sorgunun sonucu gelir, işaret kalkar, kart yeniden seçilir | ekran dışı — arka plan · E-37 · E-13 | eşlendi |
| `03 §10.1.1.5` | Kesintide başlatılan kart iadesi gerçekleşmez: "geri ödeme gerçekleşmedi" işareti; yönetici yeniden dener ya da havale yolunu açar | E-36 · E-37 · ortak bileşen: rozet | eşlendi |
| `03 §10.1.1.6` | Yönetici kesintide satışı sürdürür ya da durdurur: geçici kapatma ve duyuru şeridi | E-45 · E-44 · E-41 | eşlendi |
| `03 §10.1.1.7` | Kurulum tarafı sağlayıcı anahtarlarını ya da sağlayıcıyı değiştirir; panel işlemi değildir | ekran dışı — kurulum (`10 §4.1`) | ekran dışı |
| `03 §10.1.2.1` | Sistem e-postayı gönderemez: üç kez yeniden dener; akış etkilenmez, sipariş sayfası her bilgiyi taşır | ekran dışı — arka plan · E-16 | eşlendi |
| `03 §10.1.2.2` | Üç deneme de başarısız olur: müşteriye giden bildirimde siparişin, davette davetin satırına "e-posta ulaşmadı" düşer; firma bildirimlerinde ana sayfada "firma bildirimleri ulaşmıyor" uyarısı çıkar (UI9-01; K-760) | E-31 · E-36 · E-38 · E-49 · ortak bileşen: rozet | eşlendi |
| `03 §10.1.2.3` | Gönderim art arda başarısız olur: panelin ana sayfasında kanal düzeyinde uyarı çıkar | E-31 | eşlendi |
| `03 §10.1.2.4` | Müşteri kesintide sipariş verir ya da izler: sipariş akışı e-postaya bağlı değildir | E-16 · E-15 · E-27 | eşlendi |
| `03 §10.1.2.5` | Kullanıcı kesintide hesap bağlantısı bekler ya da davet gönderilir: bağlantı ulaşmadan işlem tamamlanmaz; yeniden istenir | E-20 · E-23 · E-29 · E-49 · E-25 | eşlendi |
| `03 §10.1.2.6` | Altyapı döner: yönetici işaretli satırlardan e-postayı yeniden gönderir; ürün kendiliğinden göndermez | E-36 · E-37 | eşlendi |
| `03 §10.1.3.1` | Google'a ulaşılamaz: Google ile giriş tamamlanmaz; şifresi olan hesap şifreyle girer | E-22 | eşlendi |
| `03 §10.1.3.2` | Şifresi olmayan hesap "şifremi unuttum" ile şifre belirler; o zamana kadar yeniden doğrulama isteyen işlemler yapılamaz | E-23 · E-24 | eşlendi |
| `03 §10.1.3.3` | Google uygulaması tanımlı değilse "Google ile giriş" düğmesi görünmez; panelde anahtarı yoktur | E-22 · E-20 · ekran dışı — kurulum (`10 §4.1`) | eşlendi |
| `03 §10.1.3.4` | Yönetici Google kesintisinde panele girer: panel etkilenmez | E-29 | eşlendi |

**§10.2 Bakım ve güncelleme (4)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §10.2.1` | Yönetici bir süre sipariş almamak ister: satışı geçici olarak kapatır; bakım modu ve siteyi kapatan bir yol yoktur | E-44 · yoktur — kapsam dışı | eşlendi |
| `03 §10.2.2` | Yönetici ziyaretçiyi bilgilendirmek ister: duyuru şeridini yayına alır | E-41 · ortak bileşen: çerçeve | eşlendi |
| `03 §10.2.3` | Kurulum tarafı ürünü günceller; planlı bakım penceresi yoktur, güncelleme panelin işi değildir | ekran dışı — kurulum (`10 §4.1`) | ekran dışı |
| `03 §10.2.4` | Kurulum tarafı veriyi yedekler ya da geri döner; ürünün akışı değildir | ekran dışı — kurulum (`10 §4.1`) · ekran dışı — mimari (`05`) | ekran dışı |

**§10.3 Sitenin kesintisi (6)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §10.3.1` | Site kesintidedir: ürün çalışmaz; vitrin, sipariş sayfası ve panel durur; ürünün içinden bilgilendirme yoktur | ekran dışı — ürünün dışında (GAP-10) | ekran dışı |
| `03 §10.3.2` | Kesinti sürer: süreler dondurulmaz; başka kanaldan bildirim yolu açıktır | ekran dışı — arka plan | ekran dışı |
| `03 §10.3.3` | Site döner: zamanı gelen kendiliğinden işler kaçırdıkları sırayla çalışır | ekran dışı — arka plan | ekran dışı |
| `03 §10.3.4` | Havale ödeme süresi kesintide dolmuştur: iptal ertelenir, sipariş sayfası yeni son günü gösterir, yönetici o ana kadar "ödendi" işaretler | ekran dışı — arka plan · E-16 · E-37 | eşlendi |
| `03 §10.3.5` | Yönetici dönüşte panele girer: sayaçlar güncel hâli gösterir; kesintide gelen cayma bildirimi ulaştığı tarihle kaydedilir | E-31 · E-37 | eşlendi |
| `03 §10.3.6` | Yönetici kesintiyi duyurmak ister: ürün bildirim göndermez; yol duyuru şerididir | yoktur — kapsam dışı · E-41 | eşlendi |

**§10.4 Panele erişimin kaybı (6)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §10.4.1` | Yönetici şifresini unutur: panelin şifre sıfırlamasını kullanır | E-29 | eşlendi |
| `03 §10.4.2` | Yönetici girişte deneme limitinin engeline takılır: tanınan tarayıcıdan girer ya da şifre sıfırlar | E-29 · ortak bileşen: mesaj | eşlendi |
| `03 §10.4.3` | Bir yönetici erişimini kaybeder, başka yönetici vardır: yeni adrese davet gönderilir ve eski hesap kaldırılır | E-49 · E-30 | eşlendi |
| `03 §10.4.4` | Hiçbir yönetici panele giremez: kurulum tarafı yeni yönetici hesabı açar; üründe kurtarma akışı yoktur | ekran dışı — kurulum (`10 §4.1`) | ekran dışı |
| `03 §10.4.5` | Ele geçirilmiş hesap tek yönetici kalmıştır: kurulum tarafı yeni hesap açar ve eski hesabın oturumlarını sonlandırır | ekran dışı — kurulum (`10 §4.1`) | ekran dışı |
| `03 §10.4.6` | Yeni yönetici paneli geri alır: hesabı kaldırır, IBAN'ı denetler, siparişleri iptal eder, davetleri düzenler, işlem izini okur | E-49 · E-45 · E-37 · E-52 | eşlendi |

#### 1.1.9 Ürün Gereksinimleri — panel tarafı (32)

Yalnız panele bakan kurallar kural kimliğiyle (14), panel alt bölümleri alt bölüm düzeyinde (18) — K-727, K-730. Kuralların tamamı okundu; alt bölüm satırının eşlemesi, alt bölümün `03`'teki adımlarının (§1.1.8) ekranlarından türetilmiştir. "Sıfır payda" maddesi `02 §10.6.3`'ün madde listesindedir; §1.1.1'in sayımı onu §10.6.2'nin maddesi diye anmıştı ve bu oturumda düzeltildi.

**Kural düzeyi (14)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `02 §3.3.4` | Her varyant zorunlu ve tekil bir stok kodu taşır; kodun panelde önceden doldurulması ve düzenlenebilirliği `04`'ün işi | E-33 | eşlendi |
| `02 §3.31.1` | Sessiz üzerine yazma yoktur: aynı kaydı düzenleyen ikinci yönetici kaydın değiştiğini görür ve uyarılır — içerik, ürün ve ayarlarda | ortak bileşen: eşzamanlı · E-33 · E-41 · E-44 | eşlendi |
| `02 §3.31.2` | Siparişe yönelik her işlem güncel hâle karşı uygulanır: sipariş değişmişse işlem uygulanmaz, panel bunu söyler ve güncel hâli gösterir; sipariş kilitlenmez | ortak bileşen: eşzamanlı · E-37 | eşlendi |
| `02 §10.1.1` | Panel tek firmayı yönetir; firma seçimi, operatör paneli ve kurtarma akışı yoktur | yoktur — kapsam dışı | ekran dışı |
| `02 §10.1.2` | Yöneticinin yapabildikleri ve yapamadıkları — on iki alan | E-32 · E-33 · E-34 · E-35 · E-36 · E-37 · E-38 · E-39 · E-40 · E-41 · E-42 · E-43 · E-44 · E-45 · E-46 · E-47 · E-49 · E-51 · E-52 · E-53 | eşlendi |
| `02 §10.1.3` | Parametreler üç katmandadır: firma ayarı panelden değişir, kurulum ayarı panelden değişmez, ürün sabiti değiştirilemez | E-46 · E-33 · ekran dışı — kurulum (`10 §4.1`) | eşlendi |
| `02 §10.1.4` | Panelde hiçbir işlev mobilde kapatılmaz; panel masaüstü öncelikli tasarlanır | ortak bileşen: taban | eşlendi |
| `02 §10.1.5` | Firmaya giden bildirimler ve gittikleri adres | ekran dışı — e-posta (`08`) | ekran dışı |
| `02 §10.6.1` | Panel ana sayfası bekleyen işleri altı sayaçla sayar; her sayaç süzülmüş listesine götürür; "IBAN bekleniyor" listesi ve kanal uyarısı ayrıdır; yeni bir ekran yoktur | E-31 · E-36 · E-38 · E-39 · ortak bileşen: süre | eşlendi |
| `02 §10.6.2` | Panelde analitik olmayan bir satış özeti vardır; dönem seçimi ve ekranın düzeni `04`'ün işi | E-51 | eşlendi |
| `02 §10.6.3` | Ölçülerin tanımı: ödeme tamamlama, iletişim talebi sayısı, kargoya verme sözüne uyum, iptal ve iade oranı, ölçüm penceresi | E-51 | eşlendi |
| `02 §10.6.3 (sıfır payda)` | Paydası sıfır olan oran "değerlendirilemez" sayılır, sıfır yüzde gösterilmez; gösterimi `04`'ün işi | E-51 | eşlendi |
| `02 §10.8.1` | Site boş kurulur; örnek içerik, ürün ve görsel yoktur | E-31 · ortak bileşen: boş | eşlendi |
| `02 §10.8.2` | Panel ana sayfasında kurulum kontrol listesi durur: eksik koşulları adıyla sayar, sihirbaz yoktur, tamamlanınca kaybolur | E-31 | eşlendi |

**Alt bölüm düzeyi (18)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `02 §3.31` | Panelde eşzamanlı düzenleme | kural düzeyinde — §1.1.9 | eşlendi |
| `02 §6.6` | Hatalar — katalog yönetimi (akış 5) | E-32 · E-33 · E-34 · E-35 · ortak bileşen: mesaj | eşlendi |
| `02 §6.7` | Hatalar — sipariş yürütümü (akış 6) | E-36 · E-37 · E-31 | eşlendi |
| `02 §6.8` | Hatalar — kurumsal içerik (akış 7) | E-41 · E-10 · E-01 · E-06 | eşlendi |
| `02 §6.9` | Hatalar — mağaza ayarları (akış 8) | E-43 · E-44 · E-45 · E-46 · E-47 · E-49 · E-30 · E-31 · E-37 | eşlendi |
| `02 §8.5` | Kayıtlar ve veri ihlali — panelde ihlal ekranı yoktur; iz panelde, giriş kaydı barındırma tarafında okunur | E-52 · yoktur — kapsam dışı | eşlendi |
| `02 §9.3` | Firmaya giden bildirimler | ekran dışı — e-posta (`08`) | ekran dışı |
| `02 §10.1` | Panelin kapsamı | kural düzeyinde — §1.1.9 | eşlendi |
| `02 §10.2` | Yönetici hesapları | E-29 · E-30 · E-49 · E-50 · E-54 | eşlendi |
| `02 §10.3` | İşlem izi | E-52 | eşlendi |
| `02 §10.4` | Sipariş müdahaleleri | E-37 | eşlendi |
| `02 §10.5` | Manuel adımlar ve bütçe | E-37 | eşlendi |
| `02 §10.6` | Bekleyen işler ve satış özeti | kural düzeyinde — §1.1.9 | eşlendi |
| `02 §10.7` | Toplu veri işlemleri | E-53 · E-32 | eşlendi |
| `02 §10.8` | İlk kurulum kontrol listesi | kural düzeyinde — §1.1.9 | eşlendi |
| `02 §11.1` | Firma ayarları | E-46 · E-33 | eşlendi |
| `02 §11.2` | Ürün sabitleri — panelde ayarı yoktur | yoktur — kapsam dışı | ekran dışı |
| `02 §11.3` | Hacim kabulleri | ekran dışı — mimari (`05`) | ekran dışı |

#### 1.1.10 Devir dizinleri — panel tarafı (66)

Devrin evi karar satırıdır; dizin onu ikinci kez kaydetmez (`PHASE1_CONFLICT_SCAN.md` §6, `PHASE2_CONFLICT_SCAN.md` §6). Özet, dizinin verdiği iş adıdır; Aşama 1 dizininin yalnız doküman adıyla işaretlediği on sekiz kararda (K-480, K-485, K-510, K-512, K-514, K-522, K-525, K-534, K-535, K-536, K-537, K-538, K-540, K-554, K-564, K-577, K-580, K-643) kararın konu ve karar hücresinden alınmıştır. `03`'ün gövdesindeki dört cümlenin satırları §1.1.8'de de durur; burada devir olarak ikinci kez eşlenir (§1.1.7'nin kalıbı). Üçü karar satırı olmadan devredilen iştir ve workshop konusudur (UI9-01); karşılıksız değildir. Karşılıksız kalan devir GAP adayıdır (UI1-06).

**Aşama 1 dizini — panel tarafı (45 / 80)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| K-480 | Firma tipi üçtür — esnaf, gerçek kişi tacir, tüzel kişi; kimlik alanları tipe göre | E-44 | eşlendi |
| K-484 | Satış özeti — kargoya verme sözüne uyum oranı | E-51 | eşlendi |
| K-485 | Satış özeti — iptal ve iade oranının payı ve paydası | E-51 | eşlendi |
| K-491 | İki modlu geri ödeme sayacı; teslim alma adımında tarih alanı | E-37 · E-31 · ortak bileşen: süre · ortak bileşen: tarih | eşlendi |
| K-495 | Satış özetinde iptal ve iade sayısı | E-51 | eşlendi |
| K-496 | "Mal dönmedi" kapatma düğmesi ve gerekçesi; kapalı kalemin görünümü | E-37 | eşlendi |
| K-510 | Başka kanaldan gelen cayma bildiriminin panelden kaydı — kalem, tarih, IBAN | E-37 | eşlendi |
| K-512 | Salt okunur üye kaydı görünümü ve hesabın talep üzerine silinmesi | E-40 | eşlendi |
| K-514 | İhlalde ulaşma: sipariş, üye listesi ve iletişim talepleri dışa aktarması; yönetici listesi | E-53 · E-49 | eşlendi |
| K-522 | Ulaşmayan e-postanın yeniden gönderilmesi ve ana sayfadaki kanal uyarısı | E-36 · E-37 · E-31 | eşlendi |
| K-525 | Müşteri IBAN girmezse kalem "IBAN bekleniyor" listesinde kalır; sayaçta sayılmaz | E-31 · E-37 | eşlendi |
| K-534 | Satış özetinde iptal ve iade oranı ayrı kalem; ödeme tamamlama oranının dönemi | E-51 | eşlendi |
| K-535 | "Kargoya verilecek" sayacı; "teslim edilemedi" işareti ve yeniden gönderim panelde | E-31 · E-37 | eşlendi |
| K-536 | Hizmetin "tamamlandı" işareti ödenmemiş siparişte sunulmaz | E-37 | eşlendi |
| K-537 | Kalem çıkarmanın sınırı; kart hattında geri alınamaz onay | E-37 · ortak bileşen: onay | eşlendi |
| K-538 | Yayın kapısını bozan işlem engellenir; panel eksik alanı gösterir ve taslağa almaya yönlendirir | E-33 · E-41 · ortak bileşen: mesaj | eşlendi |
| K-540 | Ayıp talebini yeniden açma; davet satırında "e-posta ulaşmadı" işareti; müşteri iptali sayaçta | E-38 · E-49 · E-31 | eşlendi |
| K-554 | Teslim ve ulaşma tarihinin alt sınırı; panel kural dışı değeri sebebiyle reddeder | E-37 · ortak bileşen: tarih · ortak bileşen: mesaj | eşlendi |
| K-564 | Boş alternatif metnin üretilen biçimi; alan formda isteğe bağlı | E-33 · E-41 | eşlendi |
| K-565 | Kupon formu | E-35 | eşlendi |
| K-566 | Düzeltme ekranında ayırması yetmeyen kalem | E-37 · ortak bileşen: mesaj | eşlendi |
| K-570 | Durum düzeltme ekranı | E-37 · ortak bileşen: sebep | eşlendi |
| K-573 | Ürün formunda ölçü birimi alanının hatırlatma metni | E-33 | eşlendi |
| K-577 | Fiziksel kalem Kargoya verildi'den sonra çıkarılamaz | E-37 | eşlendi |
| K-580 | Gecikme feshi satış özetinin iptal ve iade oranına ve sayısına girer | E-51 | eşlendi |
| K-581 | Panelde IBAN'sız cayma kaydı | E-37 · E-31 | eşlendi |
| K-584 | Hakkımızda ve duyuru formu; eksik alan gösterimi | E-41 · ortak bileşen: mesaj | eşlendi |
| K-585 | Misafir siparişinin e-posta düzeltme ekranı; "e-posta düzeltildi" işareti | E-37 · E-36 · ortak bileşen: rozet | eşlendi |
| K-587 | Panelde "sipariş değişti" uyarısı | ortak bileşen: eşzamanlı · E-37 | eşlendi |
| K-590 | Yeniden gönderimde kalem seçimi | E-37 | eşlendi |
| K-592 | Kapanış işlemi; sebep seçiminin olmaması | E-37 | eşlendi |
| K-596 | Kaleme bağlı tutar bazlı geri ödeme | E-37 | eşlendi |
| K-600 | Sıfır fiyat ve %100 indirim için panel uyarısı | E-33 · ortak bileşen: mesaj | eşlendi |
| K-601 | B-10 metni; adres düzeltme ekranındaki teyit hatırlatması | E-37 · ekran dışı — e-posta (`08`) (GAP-5) | eşlendi |
| K-603 | Çerez politikası taslağında beşinci çerez (tanınan tarayıcı işareti) | E-47 | eşlendi |
| K-604 | Cayma bildirimi kaydında kargoya verilmemiş kalem seçilemez | E-37 | eşlendi |
| K-606 | Panelde pencere dışı ve istisnalı kalem uyarısı; ayrı onay | E-37 · ortak bileşen: onay | eşlendi |
| K-607 | Kayıt ekranında ve müşteri talebi iptalinde teyit hatırlatması | E-37 | eşlendi |
| K-608 | Teslim işaretinde, tarih düzeltmesinde ve başka kanaldan gelen bildirimin kaydında sıra sorusu | E-37 | eşlendi |
| K-609 | "Stokta bulunamadı" sebebinde yasal uyarı | E-37 · ortak bileşen: onay | eşlendi |
| K-611 | "Geri ödeme gerçekleşmedi" uyarısının metni | E-37 · E-36 · ortak bileşen: rozet | eşlendi |
| K-612 | "Geri ödeme gerçekleşmedi" uyarısının metni — sağlayıcı panelini kontrol hatırlatması | E-37 · E-36 · ortak bileşen: rozet | eşlendi |
| K-613 | Açık ödenmemiş siparişlerin panel görünümü | E-36 · E-31 | eşlendi |
| K-616 | Havale IBAN'ı değişince yöneticilere giden bildirimin metni (F-5) | E-45 · ekran dışı — e-posta (`08`) (GAP-5) | eşlendi |
| K-643 | Satış özetinde sipariş sayısı ve cirosunun tanımı; dönem seçimi | E-51 | eşlendi |

**Aşama 2 dizini — panel tarafı (17 / 35)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| K-653 | Panelde ayrılmış adet ve reddin mesajı | E-33 · ortak bileşen: mesaj | eşlendi |
| K-654 | Kupon formunda kullanılmış ve ayrılmış hak sayısı; altına inen adedin ret mesajı | E-35 · ortak bileşen: mesaj | eşlendi |
| K-655 | Kalıcı silme onayının metni — açık sipariş sayısıyla | E-33 · ortak bileşen: onay | eşlendi |
| K-656 | Teslim alma adımında "iade reddedildi" seçimi ve geri alınamaz onay | E-37 · ortak bileşen: onay | eşlendi |
| K-660 | Davet satırında geri çekme | E-49 | eşlendi |
| K-662 | Yönetici kaldırma onayının metni — davet sayısıyla | E-49 · ortak bileşen: onay | eşlendi |
| K-664 | Havaleyi kapatma onayında ödemesi beklenen havale siparişlerinin sayısı | E-45 · E-37 · ortak bileşen: onay | eşlendi |
| K-674 | Son Yayında varyantın silinmesinin engellenme mesajı | E-33 · ortak bileşen: mesaj | eşlendi |
| K-675 | Panel işaretlerinin varyantları ve kalkışları | ortak bileşen: rozet · E-36 · E-37 | eşlendi |
| K-677 | Yeniden gönderimin onay penceresi | E-37 · ortak bileşen: onay | eşlendi |
| K-703 | Durum düzeltmesinin engel mesajı | E-37 · ortak bileşen: mesaj | eşlendi |
| K-706 | Başka kanaldan gelen gecikme feshi bildiriminin kayıt ekranı | E-37 | eşlendi |
| K-709 | Pencere dışı kayıtta uyarının metni | E-37 · ortak bileşen: onay | eşlendi |
| K-712 | İade malının ulaşma tarihinin düzeltme ekranı — geçmişe dönük, ileri tarihsiz | E-37 · ortak bileşen: tarih | eşlendi |
| K-714 | Sipariş listesi — arama ve durum süzgeci | E-36 | eşlendi |
| K-715 | Başka kanaldan gelen bildirimin kaydında ve hesap silmede onay metni | E-37 · E-40 · ortak bileşen: onay | eşlendi |
| K-717 | Teslim edilemedi'deki siparişi kapatma düğmesi | E-37 | eşlendi |

**`03`'ün gövdesinde `04`'e iş bırakan cümleler — panel tarafı (4 / 7)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §7.1.37` | Yeniden gönderilebilen e-postaların kapsamı — firma bildirimleri dahil (karar satırı yoktu; UI9-01'de karara bağlandı — K-759) | E-36 · E-37 | eşlendi |
| `03 §8.2.11` | Panelde sipariş arama ekranı (K-714) | E-36 | eşlendi |
| `03 §8.9.2` | Yeniden gönderimin kapsamı ve firma bildirimlerinin "e-posta ulaşmadı" işaretinin biçimi (karar satırı yoktu; UI9-01'de karara bağlandı — K-759, K-760, K-761) | E-31 · E-36 · E-37 · ortak bileşen: rozet | eşlendi |
| `03 §10.1.2.2` | Firma bildirimlerinin (F-1…F-4) "e-posta ulaşmadı" işaretinin biçimi (karar satırı yoktu; UI9-01'de karara bağlandı — K-760: satıra işaret düşmez, ana sayfada uyarı) | E-31 · ortak bileşen: rozet | eşlendi |

#### 1.1.11 GAP adayları — karara bağlandı → §1.3

**Kapı kapandı: matrisin 2. oturumu (2026-10-04).** On iki adayın her biri kaynakların tam metnine karşı doğrulandı (K-739): **on aday doğrulandı** ve §1.3'te GAP numarası aldı; **iki aday düştü** — cevabı kaynaktaydı, satırları "eşlendi"ye döndü. Aşağıdaki ilk tablo 1a ve 1b'nin yazdığı aday listesidir ve tarihsel kayıt olarak durur; ikinci tablo her adayın sonucudur. Karar ve gerekçe §1.3'te ve karar kaydının §3'ündedir.

| # | Aday | Dayandığı satırlar | Not |
|---|---|---|---|
| GA-1 | Müşterinin geri alınamaz işlemlerinde onay adımı var mı — siparişin iptali, cayma beyanı, gecikme feshi, hesabın silinmesi | `03 §2.7.1`, §2.8.1.2, §9.4.1 | `03 §1.6.1` ve `02 §7.6.1` onay penceresini yalnız yöneticinin işlemleri için sayar; `02 §7.6.7` cayma beyanının geri alınmadığını, `03 §9.4.1` silmenin geri alınmadığını söyler, müşteriye onay sorulup sorulmadığını söylemez |
| GA-2 | Kupon alanı sepette mi, ödeme adımında mı | `03 §2.4.4`, §3.2.1.12, §3.2.1.18 · KP-11 · K-679 | `03 §2.4.4` kuponu ödeme adımında yazar; `03 §3.2.1.18` "o sepetin kupon alanı" der; karar kaydının konu planı kupon alanını sepet ekranının konusunda sayar (§10.3 UI5-05). `02 §3.10` 1a'da satır satır okunmadı — 2. oturum okur |
| GA-3 | Üye, hesabına bağlanmış Google girişini kaldırabilir mi | `03 §9.2.2` | Hesap iki giriş yolu taşır; bağın kalktığı tek yer e-posta değişikliğinin geri alınmasıdır (`03 §9.3.6`). Hesap ekranının giriş yollarını gösterip göstermediği ve bağın kaldırılıp kaldırılamadığı yazılı değil |
| GA-4 | Bekleyen e-posta değişikliği hesap ekranında nasıl görünür; üye onu kendisi iptal edebilir mi | `03 §9.3.4`, §3.4.9 | Değişiklik yeni adres doğrulanana kadar geçerli olmaz ve bağlantının ömrüyle düşer; arada üyenin vazgeçme yolu yazılı değil |
| GA-5 | Ödeme adımındaki giriş hatırlatmasından girişe giden müşteri girişten sonra nereye döner | `03 §2.4.1` | Sepetlerin girişte birleştiği yazılı (`03 §2.3.6`); dönüş yeri ve o ana kadar yazılan e-posta ile adresin korunup korunmadığı yazılı değil |
| GA-6 | Devri `04`'e yazılmış bildirim metinleri `04`'ün kapsamında değil | K-599 ("bildirim metni") — panel tarafında K-601 ("B-10 metni"), K-616 ("bildirim metni") | E-postanın metni `08`'in işidir (karar kaydı §10.2); üç devir `08`'in park bloğuna taşınmalı ya da `04`'te yalnız ekrandaki izi kalmalı. 1b eşledi: K-601'in ekrandaki izi adres düzeltme ekranındaki teyit hatırlatmasıdır (E-37), K-616'nın ekrandaki izi yoktur — F-5 ulaşmazsa işaret de düşmez (`03 §10.1.2.2`) |
| GA-7 | Devri `04`'e yazılmış yasal metin içeriği bir ekran değil | K-493 ("form metni" — Ön Bilgilendirme Formu'nda iade taşıyıcısı bilgisi) | Üretilen metnin içeriği `02 §3.33`'ün ve metin taslağının işidir; `04` yalnız metnin gösterildiği yeri (E-13) taşır. Panel tarafında aynı sınıftan K-603 (çerez politikası taslağında beşinci çerez) var — 1b eşledi: metnin düzenlendiği yer E-47'dir, taslağın içeriği ekran değildir |
| GA-8 | Giriş yapmış yöneticinin vitrindeki görünümü: "Taslak" bandı yazılı; yöneticinin vitrinden panele nasıl döndüğü ve vitrinin hesap ile sepet alanının yönetici oturumunda ne gösterdiği yazılı değil | `03 §8.1.7`, §8.6.1.3 · `02 §3.7.5`, §3.27.25, §10.2.5 | Yönetici hesabının sepeti ve siparişi olmaz, iki hesap türü ayrı kapıdan girer (`02 §10.2.5`); taslağı band ile gören kişi vitrinde yönetici oturumuyla gezmektedir. Panelden vitrine ve önizlemeye geçiş workshop konusudur (UI3-02); vitrinin yönetici oturumundaki hâli o konunun kapsamına yazılmamış |
| GA-9 | "Açık talep" sayacı tektir ve hem iletişim hem ayıp taleplerini sayar; sayacın götürdüğü liste tek mi, iki ayrı liste mi | `03 §8.4.10`, §8.9.1, §8.6.4.1 · `02 §10.6.1`, §10.1.2 | E-38 ve E-39 ayrı adaydır. Kaynak "her sayaç kendi süzülmüş listesine götürür" der; iki talep türünün durumları ve işlemleri ayrıdır (`03 §1.9`). Bir yetenek değil, ekran kurgusu sorusudur — 2. oturum envanterin işi diye düşürebilir (UI4-02, UI9-07) |
| GA-10 | Yönetici hesabının adı nerede girilir ve değiştirilir | `03 §8.8.2`, §8.8.5, §8.9.4 · `02 §10.2.1`, §10.2.2, §9.3 (F-5, F-6) | İşlem izi satırları kaldırılan yöneticiyi "adıyla ve e-posta adresiyle" taşır, F-5 ve F-6 "yapan yöneticiyi" söyler; davetli hesabını açarken yalnız şifre kurar (`03 §8.8.2`; `02 §10.2.1`) ve yöneticinin kendi hesabında yalnız e-posta ve şifre değişir (`03 §9.3.7`). Müşteri hesabında ad kayıtta alınır ve hesapta değiştirilir (`02 §3.13.21`); yönetici hesabında karşılığı yazılı değil |
| GA-11 | Panelin ürün listesinde arama ve süzme var mı | `03 §8.1.1`, §8.1.8, §8.1.15 · `02 §10.1.2`, §11.3 (H-1) | Sipariş listesinin araması ve durum süzgeci sonradan yazıldı (K-714; `03 §8.2.11`); ürün listesi için karşılığı yok. Katalog birkaç yüz ürüne kadardır (H-1) ve yayın durumu değişiklikleri, kalıcı silme ve stok girişi listeden ürüne varmayı gerektirir. Kupon, içerik ve talep listeleri için de aynı soru geçerlidir; üye kaydı görünümü yalnız e-postayla arar (`03 §9.4.5`) |
| GA-12 | Ürün çalışırken bir işlem beklenmeyen bir sebeple tamamlanamazsa ekranın ne söylediği yazılı değil | `03 §10.3.1` · `03 §3.1` | Kesintide ürün çalışmaz ve ürünün içinden bilgilendirme yoktur — kesinti ekranı ürünün işi değildir. Ürünün çalıştığı ama isteğin başarısız olduğu hâl (sistem hatası) için `03 §3`'ün satırları adı konmuş hataları sayar, genel hâli saymaz. Hata durumunun ortak kalıbı plandadır (UI2-06); müşteriye ve yöneticiye ne söylendiği — yeniden deneme, sepetin korunduğu — bir karara bağlı değil |

**Adayların sonucu (2. oturum)**

| Aday | Sonuç | Dayanak |
|---|---|---|
| GA-1 | Doğrulandı → **GAP-1** | `02 §5.9` ve §7.6.1 onayı yalnız yöneticinin işlemleri için sayıyordu; `02 §7.2.2`, §7.2.3, §7.3.5, §3.15 ve `03 §2.7`, §2.8, §9.4 müşterinin onayını yazmıyordu ("emin misiniz", "onay adımı" taraması: sıfır eşleşme) |
| GA-2 | **Düştü** | `02 §3.10.1`: *"Kupon, ödeme adımında girilen ve sepet toplamına inen bir koddur"* — alan ödeme adımındadır (E-13). `02 §3.10.5`'in "o sepetin kupon alanı" sözü ekranı değil, geçersiz deneme sayacının (L-6) sepete bağlı sayıldığını söyler (`03 §2.3.6`: sayaç sepetlerin birleşmesinde büyüğünü taşır). Ayrılan yer konu planıydı: karar kaydının §10.3'ü kupon alanını sepet ekranının konusunda (UI5-05) sayıyordu — hizalama hatası olarak düzeltildi, alan ödeme adımının konusuna (UI5-06) taşındı. Beş satır "eşlendi" |
| GA-3 | Doğrulandı → **GAP-2** | `02 §3.13.7`, §3.13.8, §3.13.14 bağın kurulduğu ve sistemce kalktığı anı yazar; üyenin kaldırıp kaldıramayacağını yazmaz |
| GA-4 | Doğrulandı → **GAP-3** | `02 §3.13.5`, §3.13.14 ve §6.1.1 bekleyen değişikliğin bağlantısının ekrandan yeniden istenebildiğini ve bağlantının ömrüyle düştüğünü yazar; yanlış yazılmış yeni adresin düzeltilmesini yazmaz |
| GA-5 | Doğrulandı → **GAP-4** (ekran kurgusu) | `03 §2.4.1`, §2.3.6 ve `02 §3.13.3` hatırlatmayı ve sepetlerin birleşmesini yazar; girişten sonraki dönüş yeri bir ürün kuralı değil, geçiş tasarımıdır |
| GA-6 | Doğrulandı → **GAP-5** (yanlış adrese devir) | `03 §7` girişi: *"metni, şablonu ve gönderim biçimi `08`'in işidir"*; üç kararın etki sütunu metni `04`'e yazmıştı |
| GA-7 | **Düştü** | İçerik kuralı kaynaktadır: Ön Bilgilendirme Formu'nun taşıyıcı cümlesi `02 §3.24.3` ve §7.4.4'te (K-493), beş çerezin sayımı `02 §12.2.5`'te ve taslağın taşıdıkları `02 §3.33.2`'de (K-603). `04`'e düşen yalnız metnin göründüğü ve düzenlendiği yerdir — E-13 ve E-47 — ve ikisi de eşlenmiştir. Metinlerin kendisinin yazımı ekran işi değildir; `02`'nin kuralı olarak Implementation Planı'nın izlenebilirlik matrisine (`11`) kendiliğinden girer. İki satır "eşlendi" |
| GA-8 | Doğrulandı → **GAP-6** (ekran kurgusu) | `02 §3.7.5`, §3.27.25 ve §10.2.5 bandı ve iki hesap türünün ayrılığını yazar; yöneticinin vitrindeki çerçevesini yazmaz |
| GA-9 | Doğrulandı → **GAP-7** (ekran kurgusu) | `02 §10.6.1` *"her sayaç kendi süzülmüş listesine götürür"* ve `02 §10.1.2` "Talepler"i tek alan sayar; listenin tek mi iki mi olduğu bir yetenek değil, ekran düzenidir |
| GA-10 | Doğrulandı → **GAP-8** | `02 §10.2.2` ve karar kaydının K-314'ü yöneticinin iz satırında "adıyla ve e-posta adresiyle" okunduğunu söyler; adın girildiği ve değiştiği yer hiçbir kaynakta yoktu |
| GA-11 | Doğrulandı → **GAP-9** | `02 §10.1.2` arama ve süzgeci yalnız sipariş listesi (K-714), üye kaydı görünümü ve işlem izi için sayıyordu; ürün listesi için karşılığı yoktu |
| GA-12 | Doğrulandı → **GAP-10** (ekran kurgusu) | `02 §6.1` ve `03 §3.1` ilkeleri (sepet korunur, anlam yalnız renkle taşınmaz) ve adı konmuş hataları yazar; beklenmeyen hatanın ekranını yazmaz. Kesinti satırı (`03 §10.3.1`) "ekran dışı" kalır |

**Aday listesine girmemiş boşluk (2. oturumun taraması).** 1b'nin eşlediği ama kaynağın yerini yazmadığı üç yerleşim tek boşlukta toplandı — **GAP-11** (ekran kurgusu): indirme hakkı ve KDV oranı varsayılanının değiştiği ayar ekranı (`03 §8.7.4.4` — `02 §10.1.2`'nin alan tablosu ikisini adıyla saymaz) ve "IBAN bekleniyor" listesinin panel ana sayfasındaki yeri (`03 §1.7.3.4`, §8.9.1; `02 §10.6.1` — "sayaçlardan ayrıdır" der, yerini söylemez). Karar satırı olmadan devredilen üç iş — yeniden gönderilebilen e-postaların kapsamı ve firma bildirimlerinde "e-posta ulaşmadı" işaretinin biçimi (`03 §7.1.37`, §8.9.2, §10.1.2.2) — yeni bir boşluk değildir: workshop konusudur (UI9-01) ve §1.1.10'da öyle eşlenmiştir.

### 1.2 Geri izlenebilirlik (ekran → kaynak)

**Matrisin 2. oturumu (2026-10-04; K-738).** Tablo §1.1.3–§1.1.10'un "Ekran" sütunundan betikle üretildi: her aday ekranın ve her ortak bileşen adının beslendiği kaynak satırları aile bazında sayılır. **Sonuç: kaynağı olmayan aday yoktur** — 54 adayın 54'ü ve on beş ortak bileşen adının on beşi en az bir kaynak satırından beslenir; gerekçesiz ekleme listesi boştur. Kaynağı zayıf olan ya da ekran olmayan adaylar ve öneriler §1.1.2'nin "2. oturumun önerileri" tablosundadır (UI1-04). **Workshop'un hizalaması (2026-10-04, v0.5):** "e-posta ulaşmadı" işaretinin kararları (K-759…K-761) yedi matris satırının ekran hücresini değiştirdi — işaret iletişim talebinin satırına düşmez (E-39 üç satırdan çıktı), firma bildiriminin uyarısı ve işaretli siparişlere götüren satır panel ana sayfasındadır (E-31 üç satıra girdi); E-31, E-36, E-38 ve E-39'un sayıları aşağıda betikle yenilendi. Durum dağılımı değişmedi: 1.026 eşlendi · 167 ekran dışı. 1a'nın ara denetimi 53 aday üzerindeydi; 1b'nin eklediği E-54 ile aday sayısı 54'tür. **Envanterin hizalaması (yazım turunun 1. oturumu, 2026-10-04, v0.6; K-764):** ekran envanteri (§4) on dört matris satırının ekran hücresini değiştirdi — E-48'in sekiz satırı E-44'e taşındı (`10 §2` KP-60; `03 §1.10.8`, §3.5.4.15, §8.7.5.1, §8.7.5.2, §10.1.1.6, §10.2.1; `02 §10.1.2`'nin satırında E-44 zaten vardı, E-48 çıktı) ve iletişim talebinin "Talepler" listesindeki yüzünü taşıyan altı satır E-38'i de aldı (`10 §2` KP-34; `03 §7.1.39`, §7.3.37, §8.6.4.1; `02 §3.32.4`, §5.13). E-38, E-39, E-44 ve E-48'in satırları aşağıda betikle yenilendi; öteki satırlar ve durum dağılımı değişmedi: 1.193 satır — 1.026 eşlendi · 167 ekran dışı. Ortak bileşen adlarının bileşen kimlikleri (OB-nn) §2'nin tablosundadır.

**Hücrenin okunuşu.** Kalın sayı adayı anan kaynak satırlarının toplamıdır; ardından dört aile gelir: `10 §2` — kapsam satırları, kimlikleriyle · `03` — Kullanıcı Akışları'nın satırları, üst bölüm bazında sayıyla (§2 müşteri akışları, §8 yönetim akışları, §3 hatalar, §1 durum makinesi, §7 bildirim haritası…) · `02` — Ürün Gereksinimleri'nin kural ve alt bölüm satırları, ilk beş kimlikle · devir — devir dizinlerinin ve park satırlarının satırları, ilk dört kimlikle. Yüzlerce kimlik hücreye yazılmaz: bir adayın bütün kaynak satırları, aşağıdaki ikinci betikle adayın kimliği verilerek listelenir — ekran tanımını yazan oturum (`04 §5`, §9) bu listeden okur. Bir satır birden çok adaya eşlenebildiği için sayıların toplamı 1.193'ü aşar.

| Ekran | Beslendiği kaynak(lar) | Durum |
|---|---|---|
| E-01 Ana sayfa | **12** — `10 §2` 1 (KP-31) · `03` 3 (§2 2 · §3 1) · `02` 7 (§3.27.14, §3.27.22, §3.28.1, §3.28.2, §3.28.3, …) · devir 1 (Park K-27) | kaynaklı |
| E-02 Kategori sayfası | **9** — `10 §2` 2 (KP-1, KP-3) · `03` 3 (§2 3) · `02` 3 (§3.28.6, §3.4, §3.5) · devir 1 (03 §2.1.2) | kaynaklı |
| E-03 Arama sonuçları | **6** — `10 §2` 2 (KP-2, KP-3) · `03` 3 (§2 3) · `02` 1 (§3.5) | kaynaklı |
| E-04 Ürün sayfası | **41** — `10 §2` 8 (KP-4, KP-5, KP-6, KP-9, KP-19, KP-36, KP-38, KP-43) · `03` 18 (§1 1 · §2 9 · §3 5 · §4 2 · §8 1) · `02` 12 (§3.9.2, §3.20.1, §3.30.7, §3.2, §3.3, …) · devir 3 (K-511, K-545, K-610) | kaynaklı |
| E-05 Arşivlenmiş ürün sayfası | **5** — `10 §2` 1 (KP-7) · `03` 2 (§2 1 · §8 1) · `02` 2 (§3.7, §5.1) | kaynaklı — az satır, kalır (§1.1.2) |
| E-06 "Sayfa bulunamadı" sayfası | **8** — `10 §2` 1 (KP-7) · `03` 3 (§2 2 · §3 1) · `02` 4 (§3.27.25, §3.30.4, §3.7, §6.8) | kaynaklı |
| E-07 İçerik liste sayfası | **6** — `10 §2` 1 (KP-32) · `03` 2 (§2 1 · §3 1) · `02` 3 (§3.27.4, §3.27.5, §3.27.11) | kaynaklı |
| E-08 İçerik sayfası | **17** — `10 §2` 2 (KP-32, KP-36) · `03` 4 (§2 2 · §3 1 · §8 1) · `02` 11 (§3.27.1, §3.27.3, §3.27.4, §3.27.5, §3.27.9, …) | kaynaklı |
| E-09 Sık sorulan sorular sayfası | **4** — `10 §2` 1 (KP-32) · `03` 2 (§2 1 · §8 1) · `02` 1 (§3.27.6) | kaynaklı — az satır, kalır (§1.1.2) |
| E-10 İletişim sayfası ve formu | **50** — `10 §2` 5 (KP-30, KP-33, KP-34, KP-35, KP-74) · `03` 24 (§2 7 · §3 6 · §4 1 · §5 6 · §6 1 · §8 1 · §9 2) · `02` 16 (§3.1.4, §3.27.4, §3.27.7, §3.27.8, §3.32.1, …) · devir 5 (Park K-14 · K-574, K-547, K-553, K-627, …) | kaynaklı |
| E-11 Yasal metin sayfası | **9** — `10 §2` 3 (KP-30, KP-35, KP-73) · `03` 2 (§7 2) · `02` 2 (§3.33.1, §12.2) · devir 2 (K-547, K-626) | kaynaklı |
| E-12 Sepet | **46** — `10 §2` 5 (KP-6, KP-9, KP-10, KP-13, KP-38) · `03` 33 (§2 15 · §3 12 · §4 2 · §8 1 · §9 2 · §10 1) · `02` 7 (§3.16.4, §3.19.5, §3.20.1, §3.6, §3.16, …) · devir 1 (K-594) | kaynaklı |
| E-13 Ödeme adımı | **73** — `10 §2` 8 (KP-6, KP-8, KP-11, KP-12, KP-13, KP-14, KP-45, KP-74) · `03` 34 (§2 11 · §3 13 · §4 2 · §6 5 · §9 1 · §10 2) · `02` 21 (§3.13.3, §3.19.5, §3.20.1, §3.24.1, §3.24.2, …) · devir 10 (K-493, K-503, K-509, K-544, …) | kaynaklı |
| E-14 Sipariş teyit ve bekleme ekranı | **15** — `10 §2` 2 (KP-15, KP-17) · `03` 10 (§1 1 · §2 4 · §4 1 · §7 2 · §10 2) · `02` 1 (§3.21) · devir 2 (K-663, K-704) | kaynaklı |
| E-15 Sipariş takibi girişi | **11** — `10 §2` 1 (KP-18) · `03` 8 (§2 2 · §3 3 · §6 1 · §9 1 · §10 1) · `02` 1 (§3.22) · devir 1 (K-602) | kaynaklı |
| E-16 Sipariş sayfası | **227** — `10 §2` 11 (KP-15, KP-17, KP-18, KP-19, KP-20, KP-21, KP-22, KP-23, KP-24, KP-45, KP-74) · `03` 170 (§1 33 · §2 45 · §3 32 · §4 15 · §5 11 · §6 4 · §7 22 · §8 4 · §9 1 · §10 3) · `02` 26 (§3.24.6, §3.33.5, §3.33.7, §3.12, §3.20, …) · devir 20 (K-494, K-497, K-498, K-499, …) | kaynaklı |
| E-17 İptal ve gecikme feshi ekranı | **24** — `10 §2` 3 (KP-21, KP-24, KP-74) · `03` 16 (§1 7 · §2 6 · §3 3) · `02` 3 (§3.33.5, §3.26, §7.2) · devir 2 (K-499, K-505) | kaynaklı |
| E-18 Cayma beyanı ekranı | **30** — `10 §2` 3 (KP-22, KP-24, KP-74) · `03` 18 (§1 4 · §2 4 · §3 8 · §4 1 · §5 1) · `02` 4 (§3.33.5, §3.26, §7.3, §7.4) · devir 5 (K-494, K-503, K-509, K-578, …) | kaynaklı |
| E-19 Ayıp talebi ekranı | **13** — `10 §2` 2 (KP-23, KP-74) · `03` 8 (§1 3 · §2 2 · §3 2 · §5 1) · `02` 3 (§3.33.5, §3.26, §7.5) | kaynaklı |
| E-20 Kayıt ekranı | **33** — `10 §2` 3 (KP-25, KP-26, KP-74) · `03` 21 (§1 1 · §3 8 · §4 1 · §6 2 · §7 2 · §9 5 · §10 2) · `02` 5 (§3.33.5, §3.13, §8.1, §8.4, §9.4) · devir 4 (K-553, K-605, K-659, K-697) | kaynaklı |
| E-21 Doğrulama bağlantısının iniş ekranı | **11** — `10 §2` 1 (KP-25) · `03` 9 (§1 1 · §3 1 · §4 1 · §7 3 · §9 3) · `02` 1 (§3.13) | kaynaklı |
| E-22 Müşteri girişi | **35** — `10 §2` 3 (KP-26, KP-27, KP-74) · `03` 24 (§1 2 · §2 2 · §3 5 · §4 2 · §6 4 · §9 7 · §10 2) · `02` 4 (§3.33.5, §3.13, §5.12, §8.1) · devir 4 (K-553, K-588, K-659, K-697) | kaynaklı |
| E-23 Şifre sıfırlama ekranları | **22** — `10 §2` 1 (KP-27) · `03` 18 (§3 6 · §4 1 · §6 4 · §7 2 · §9 3 · §10 2) · `02` 3 (§3.13, §8.1, §9.4) | kaynaklı |
| E-24 Yeniden doğrulama ekranı | **10** — `03` 8 (§3 1 · §6 2 · §9 4 · §10 1) · `02` 2 (§3.13, §8.1) | kaynaklı |
| E-25 Hesap — profil ve güvenlik | **25** — `10 §2` 2 (KP-27, KP-29) · `03` 18 (§1 1 · §3 4 · §4 3 · §6 2 · §9 7 · §10 1) · `02` 3 (§3.15, §5.12, §9.4) · devir 2 (K-605, K-698) | kaynaklı |
| E-26 Adres defteri | **5** — `10 §2` 2 (KP-12, KP-27) · `03` 1 (§9 1) · `02` 1 (§3.14) · devir 1 (K-561) | kaynaklı — az satır, kalır (§1.1.2) |
| E-27 Sipariş geçmişi | **13** — `10 §2` 2 (KP-18, KP-28) · `03` 10 (§2 4 · §3 3 · §9 2 · §10 1) · `02` 1 (§3.22) | kaynaklı |
| E-28 E-posta değişikliğini geri alma ekranı | **12** — `10 §2` 1 (KP-27) · `03` 9 (§1 1 · §3 1 · §4 1 · §6 2 · §7 3 · §9 1) · `02` 1 (§9.4) · devir 1 (K-599) | kaynaklı |
| E-29 Panel girişi ve şifre sıfırlama | **16** — `03` 13 (§1 1 · §6 4 · §7 2 · §9 2 · §10 4) · `02` 2 (§8.1, §10.2) · devir 1 (K-588) | kaynaklı |
| E-30 Yönetici daveti kabul ekranı | **12** — `10 §2` 1 (KP-62) · `03` 9 (§1 2 · §3 1 · §4 1 · §7 2 · §8 2 · §10 1) · `02` 2 (§6.9, §10.2) | kaynaklı |
| E-31 Panel ana sayfası | **67** — `10 §2` 4 (KP-38, KP-50, KP-65, KP-68) · `03` 45 (§1 3 · §3 7 · §4 2 · §5 1 · §6 2 · §7 14 · §8 13 · §10 3) · `02` 7 (§3.33.9, §3.1, §10.6.1, §10.8.1, §10.8.2, …) · devir 11 (K-498, K-576, K-491, K-522, …) | kaynaklı |
| E-32 Ürün listesi | **10** — `10 §2` 2 (KP-39, KP-40) · `03` 3 (§8 3) · `02` 5 (§3.7, §5.1, §10.1.2, §6.6, §10.7) | kaynaklı |
| E-33 Ürün formu | **66** — `10 §2` 8 (KP-39, KP-40, KP-41, KP-42, KP-43, KP-44, KP-45, KP-76) · `03` 33 (§1 6 · §3 13 · §4 2 · §6 1 · §8 11) · `02` 16 (§3.2, §3.3, §3.6, §3.7, §3.8, …) · devir 9 (K-509, K-511, K-538, K-564, …) | kaynaklı |
| E-34 Kategori ağacı | **8** — `10 §2` 1 (KP-41) · `03` 3 (§3 2 · §8 1) · `02` 4 (§3.28.6, §3.4, §10.1.2, §6.6) | kaynaklı |
| E-35 Kuponlar | **16** — `03` 10 (§1 5 · §3 2 · §4 1 · §8 2) · `02` 4 (§3.10, §5.11, §10.1.2, §6.6) · devir 2 (K-565, K-654) | kaynaklı |
| E-36 Sipariş listesi | **67** — `10 §2` 2 (KP-46, KP-50) · `03` 52 (§1 5 · §2 3 · §3 5 · §4 2 · §6 3 · §7 21 · §8 8 · §9 1 · §10 4) · `02` 3 (§10.1.2, §10.6.1, §6.7) · devir 10 (K-522, K-585, K-611, K-612, …) | kaynaklı |
| E-37 Sipariş ayrıntısı | **340** — `10 §2` 10 (KP-8, KP-24, KP-28, KP-44, KP-46, KP-47, KP-48, KP-51, KP-68, KP-76) · `03` 260 (§1 70 · §2 30 · §3 49 · §4 14 · §5 19 · §6 14 · §7 10 · §8 46 · §9 1 · §10 7) · `02` 23 (§3.12, §3.20, §3.21, §5.3, §5.4, …) · devir 47 (K-494, K-497, K-499, K-575, …) | kaynaklı |
| E-38 Talepler (aday adı: Ayıp talepleri) | **28** — `10 §2` 2 (KP-34, KP-49) · `03` 19 (§1 3 · §2 2 · §3 1 · §4 1 · §7 5 · §8 6 · §10 1) · `02` 6 (§3.32.4, §5.10, §5.13, §7.5, §10.1.2, …) · devir 1 (K-540) | kaynaklı — envanterde tek "Talepler" listesi; iletişim talebinin listedeki yüzünü taşıyan altı satır eklendi (§4.3) |
| E-39 İletişim talebi ayrıntısı (aday adı: İletişim talepleri) | **19** — `10 §2` 2 (KP-34, KP-49) · `03` 12 (§2 1 · §4 1 · §5 1 · §7 2 · §8 7) · `02` 5 (§3.32.4, §3.32.5, §5.13, §10.1.2, §10.6.1) | kaynaklı — envanterde tek talebin ayrıntısı; liste E-38'dedir (§4.3) |
| E-40 Üye kaydı görünümü | **16** — `10 §2` 2 (KP-29, KP-77) · `03` 8 (§1 1 · §3 1 · §8 3 · §9 3) · `02` 3 (§3.15, §5.9, §10.1.2) · devir 3 (K-698, K-512, K-715) | kaynaklı |
| E-41 Kurumsal içerik | **53** — `10 §2` 3 (KP-54, KP-57, KP-76) · `03` 23 (§1 1 · §3 5 · §4 2 · §6 1 · §7 2 · §8 9 · §10 3) · `02` 24 (§3.27.1, §3.27.2, §3.27.3, §3.27.7, §3.27.9, …) · devir 3 (K-538, K-564, K-584) | kaynaklı |
| E-42 Ana sayfa, menü ve sosyal bağlantılar | **12** — `10 §2` 1 (KP-55) · `03` 4 (§8 4) · `02` 6 (§3.27.12, §3.28.1, §3.28.4, §3.28.5, §3.28.6, …) · devir 1 (Park K-27) | kaynaklı |
| E-43 Marka | **13** — `10 §2` 1 (KP-56) · `03` 5 (§3 4 · §8 1) · `02` 6 (§3.29.1, §3.29.3, §3.29.4, §3.29.6, §10.1.2, …) · devir 1 (K-541) | kaynaklı |
| E-44 Firma kimliği ve satış (aday adı: Firma kimliği) | **25** — `10 §2` 2 (KP-58, KP-60) · `03` 13 (§1 1 · §3 4 · §8 5 · §9 1 · §10 2) · `02` 5 (§3.1.4, §3.1, §3.31.1, §10.1.2, §6.9) · devir 5 (Park K-14 · K-574, K-547, K-574, K-627, …) | kaynaklı — E-48'in sekiz satırını devraldı (§4.3) |
| E-45 Ödeme yöntemleri | **23** — `10 §2` 1 (KP-59) · `03` 15 (§3 6 · §6 3 · §8 4 · §10 2) · `02` 4 (§3.33.8, §3.21, §10.1.2, §6.9) · devir 3 (K-663, K-616, K-664) | kaynaklı |
| E-46 Kargo, teslimat, süreler, eşikler ve iade adresi | **21** — `10 §2` 2 (KP-59, KP-64) · `03` 10 (§3 4 · §8 6) · `02` 8 (§3.33.8, §3.18, §3.19, §3.20, §10.1.2, …) · devir 1 (K-665) | kaynaklı |
| E-47 Yasal metinler | **12** — `10 §2` 1 (KP-61) · `03` 3 (§3 1 · §8 2) · `02` 7 (§3.33.1, §3.33.2, §3.33.3, §3.33.9, §12.2, …) · devir 1 (K-603) | kaynaklı |
| E-48 Satışın geçici kapatılması | **0** — sekiz satırı E-44'e taşındı (yazım turunun 1. oturumu, v0.6) | düştü → E-44 (§4.3; K-738, K-752, K-764) |
| E-49 Yönetici hesapları | **28** — `10 §2` 1 (KP-62) · `03` 20 (§1 3 · §3 2 · §4 2 · §6 2 · §7 2 · §8 5 · §10 4) · `02` 3 (§10.1.2, §6.9, §10.2) · devir 4 (K-514, K-540, K-660, K-662) | kaynaklı |
| E-50 Yöneticinin kendi hesabı | **4** — `03` 3 (§1 1 · §6 1 · §9 1) · `02` 1 (§10.2) | kaynaklı — az satır, kalır (§1.1.2) |
| E-51 Satış özeti | **13** — `10 §2` 1 (KP-53) · `03` 2 (§8 2) · `02` 4 (§10.1.2, §10.6.2, §10.6.3, §10.6.3 (sıfır payda)) · devir 6 (K-484, K-485, K-495, K-534, …) | kaynaklı |
| E-52 İşlem izi | **13** — `10 §2` 1 (KP-63) · `03` 9 (§4 1 · §5 1 · §6 3 · §8 3 · §10 1) · `02` 3 (§10.1.2, §8.5, §10.3) | kaynaklı |
| E-53 Dışa aktarma | **9** — `10 §2` 1 (KP-52) · `03` 4 (§6 1 · §8 3) · `02` 3 (§3.25, §10.1.2, §10.7) · devir 1 (K-514) | kaynaklı |
| E-54 Yönetici hesabının doğrulama ekranları | **5** — `03` 4 (§7 3 · §9 1) · `02` 1 (§10.2) | kaynaklı — kalır, müşteri tarafıyla birleşmez (§1.1.2) |

**Ortak bileşen adları (15).** Ekran değildir; `04 §2`'de birer kalıp olarak tanımlanır (UI2). Adlar adaydır.

| Ortak bileşen | Beslendiği kaynak(lar) | Durum |
|---|---|---|
| ortak bileşen: IBAN | **27** — `03` 23 (§1 4 · §2 2 · §3 3 · §7 12 · §8 2) · `02` 1 (§7.4) · devir 3 (K-498, K-569, K-690) | kaynaklı |
| ortak bileşen: adres | **10** — `10 §2` 1 (KP-12) · `03` 7 (§1 1 · §2 2 · §3 2 · §8 1 · §9 1) · `02` 1 (§3.14) · devir 1 (K-561) | kaynaklı |
| ortak bileşen: boş | **1** — `02` 1 (§10.8.1) | kaynaklı — tek satır, kalır (§1.1.2) |
| ortak bileşen: eşzamanlı | **8** — `03` 5 (§3 3 · §8 2) · `02` 2 (§3.31.1, §3.31.2) · devir 1 (K-587) | kaynaklı |
| ortak bileşen: kart | **9** — `03` 4 (§2 4) · `02` 3 (§3.9.2, §3.27.10, §3.9) · devir 2 (K-545, 03 §2.1.2) | kaynaklı |
| ortak bileşen: mesaj | **80** — `10 §2` 2 (KP-72, KP-76) · `03` 63 (§1 3 · §2 3 · §3 32 · §4 1 · §6 13 · §8 9 · §10 2) · `02` 3 (§3.16.4, §6 (giriş), §6.6) · devir 12 (K-602, K-659, 03 §3.1.6, K-538, …) | kaynaklı |
| ortak bileşen: onay | **61** — `10 §2` 1 (KP-47) · `03` 47 (§1 18 · §2 1 · §3 5 · §5 2 · §6 3 · §8 16 · §9 2) · `02` 3 (§3.27.27, §5.9, §7.6) · devir 10 (K-537, K-606, K-609, K-655, …) | kaynaklı |
| ortak bileşen: rozet | **37** — `03` 23 (§1 11 · §2 1 · §3 1 · §4 1 · §7 2 · §8 4 · §10 3) · `02` 8 (§5.1, §5.2, §5.3, §5.4, §5.5, …) · devir 6 (K-585, K-611, K-612, K-675, …) | kaynaklı |
| ortak bileşen: satış-kapalı | **12** — `10 §2` 1 (KP-38) · `03` 9 (§1 1 · §2 1 · §3 4 · §8 3) · `02` 1 (§3.1) · devir 1 (K-576) | kaynaklı |
| ortak bileşen: sebep | **13** — `03` 12 (§1 6 · §3 2 · §6 1 · §8 3) · devir 1 (K-570) | kaynaklı |
| ortak bileşen: süre | **12** — `03` 9 (§3 1 · §4 4 · §7 1 · §8 3) · `02` 2 (§4.1, §10.6.1) · devir 1 (K-491) | kaynaklı |
| ortak bileşen: taban | **17** — `10 §2` 2 (KP-69, KP-71) · `03` 2 (§3 2) · `02` 11 (§3 (giriş), §11 (envantere girmeyenler), §3.29.1, §3.29.4, §3.29.5, …) · devir 2 (K-625, 03 §0.1.4) | kaynaklı |
| ortak bileşen: tarih | **13** — `03` 10 (§1 5 · §3 1 · §8 4) · devir 3 (K-491, K-554, K-712) | kaynaklı |
| ortak bileşen: taslak | **8** — `10 §2` 2 (KP-40, KP-54) · `03` 4 (§2 1 · §8 3) · `02` 2 (§3.27.25, §3.7) | kaynaklı |
| ortak bileşen: çerçeve | **42** — `10 §2` 4 (KP-1, KP-2, KP-35, KP-36) · `03` 20 (§1 1 · §2 6 · §3 3 · §4 1 · §7 3 · §8 3 · §9 2 · §10 1) · `02` 13 (§3.1.4, §3.27.12, §3.27.21, §3.27.22, §3.28.4, …) · devir 5 (Park K-14 · K-574, K-541, K-574, K-626, …) | kaynaklı |

**Betikler** (Git Bash; depo kökünden). Birincisi yukarıdaki iki tabloyu üretir; ikincisi tek bir adayın bütün kaynak satırlarını listeler.

```sh
# geri izlenebilirlik — aday başına: toplam, aile bazında sayı ve temsilî kimlikler
awk -F'|' '
/^#### 1\.1\./{split($0,h," "); s=h[2]} /^#### 1\.1\.11/{s=""}
s!="" && /^\| (`0[23] §|KP-|K-[0-9]|Park )/{
  id=$2; gsub(/^ +| +$|`/,"",id); n=split($4,a," · ")
  for(i=1;i<=n;i++){ v=a[i]; gsub(/^ +| +$/,"",v); sub(/ \(GAP-[0-9]+\)/,"",v)
    if(v ~ /^E-[0-9]+$/ || v ~ /^ortak bileşen: [^ ]+$/){ tot[v]++
      if(s=="1.1.3"){kp[v]=kp[v] (kp[v]?", ":"") id; nkp[v]++}
      else if(s=="1.1.4"||s=="1.1.8"){split(id,p,"[ §.]+"); c3[v,p[2]+0]++; n3[v]++}
      else if(s=="1.1.5"||s=="1.1.6"||s=="1.1.9"){sub(/^02 /,"",id); n2[v]++; if(n2[v]<=5) l2[v]=l2[v] (l2[v]?", ":"") id}
      else {nd[v]++; if(nd[v]<=4) ld[v]=ld[v] (ld[v]?", ":"") id} } } }
END{ for(v in tot){ o=""
  if(nkp[v]) o=o "10 §2 " nkp[v] " (" kp[v] ")"
  if(n3[v]){t=""; for(j=1;j<=10;j++) if((v,j) in c3) t=t (t?" · ":"") "§" j " " c3[v,j]; o=o (o?" · ":"") "03 " n3[v] " (" t ")"}
  if(n2[v]) o=o (o?" · ":"") "02 " n2[v] " (" l2[v] (n2[v]>5?", …":"") ")"
  if(nd[v]) o=o (o?" · ":"") "devir " nd[v] " (" ld[v] (nd[v]>4?", …":"") ")"
  print v "\t" tot[v] "\t" o } }' Docs/04_UI_SPECS.md | sort -t"$(printf '\t')" -k1,1V
# tek bir adayın bütün kaynak satırları (örnek: E-48)
awk -F'|' -v e='E-48' '/^#### 1\.1\./{s=$0} /^#### 1\.1\.11/{s=""}
  s!="" && /^\| (`0[23] §|KP-|K-[0-9]|Park )/ && (" " $4 " ") ~ ("[ ·]" e "[ ·(]"){print $2 "|" $3}' Docs/04_UI_SPECS.md
# kaynağı olmayan aday: §1.1.2'nin ve §4'ün kimlikleri eksi "Ekran" sütununda geçenler
# (v0.6'dan itibaren yalnız E-48 dönmeli — envanterde düştü, satırları E-44'e taşındı: §4.3)
comm -23 <(grep -oE '^\| E-[0-9]+ \| ' Docs/04_UI_SPECS.md | grep -oE 'E-[0-9]+' | sort -u) \
         <(awk -F'|' '/^#### 1\.1\.([3-9]|10)/{s=1} /^#### 1\.1\.11/{s=0} s && /^\| /{print $4}' Docs/04_UI_SPECS.md | grep -oE 'E-[0-9]+' | sort -u)
```

**Örneklem doğrulaması — 1a'nın satırları (2026-10-04).** 1a oturumu `03`'ün 378 satırını hücreleri iki–üç yüz karakterle keserek okumuştu (karar kaydı §10.1). 2. oturum bir örneklemi **satırın tamamını** okuyarak yeniden eşledi.

- **İlk örneklem — 82 satır:** `03 §2.3`–§2.9'dan 36, §9'dan 25, §3.1–§3.4'ten 14, §4'ten 4, §5'ten 3. **Ayrışan 9 satır (%11,0):** 2.7.1, 2.7.2, 2.7.3, 2.7.7, 2.8.1.2, 2.9.2, 2.9.4, 4.1.7, 9.5.4. Dokuzunun da ayrışması aynı türdendir: satırın eşlendiği ekran doğruydu, hücrenin kesilen ikinci yarısında adı geçen **ikinci bir ekran eksik kalmıştı** — çoğu panelin siparişte gösterdiği uyarı ya da işaret (E-37), biri panele düşen talep (E-38), biri altbilgi. Yanlış ekrana eşlenmiş satır çıkmadı.
- **Oran %10'un üstünde olduğu için örneklem genişletildi:** ayrışmanın türü belli olduğundan 378 satırın **tamamı** betikle tarandı — kaynak satırın tam metninde "panel", "sipariş sayfası", "müşterinin listesi", "hesap ekranı", "iletişim formu" ve "sepet" geçip eşlemede karşılığı olmayan satırlar. Tarama 43 satır işaretledi; ilk örneklemde olmayan 29'u tam metinle okundu ve **11'i daha ayrıştı:** 2.2.9, 2.5.1.3, 2.5.1.4, 3.2.1.1, 3.2.1.15, 3.2.1.19, 3.2.2.1, 3.2.2.4, 3.3.25, 4.2.12, 6.2.4.4. Kalan işaretler ekran doğurmayan anmalardı ("sepet olduğu gibi durur", "panelde düğme yoktur").
- **Düzeltme:** 20 satırın "Ekran" hücresine eksik ekran eklendi; ikisinin durumu "ekran dışı"ndan "eşlendi"ye döndü (2.9.4, 9.5.4). Hiçbir satır yeni bir GAP doğurmadı ve hiçbir GAP kararını değiştirmedi; E-36, E-37, E-27 ve E-16'nın kaynak satırı sayısı arttı.
- **Kalan belirsizlik:** tam metinle okunan satır 111'dir; 267 satır yalnız anahtar sözcük taramasından geçti. İlk örneklemdeki dokuz ayrışmanın ikisi (4.1.7, 9.5.4) taramanın sözcüklerini taşımıyordu — aynı oran (82 satırda 2) okunmayan 267 satıra uygulanınca tahmin altı–yedi satırdır; bu satırlar doğrulanmamıştır. Beklenen tür "eksik ikinci ekran"dır ve ekran tanımını yazan oturum kaynak satırlarını tam metinle okurken kapanır: **kapı `04 §5` ve §9'un yazımıdır** — ekranı yazan oturum, o ekranın `03` satırlarını yukarıdaki ikinci betikle listeler, kaynakta tam okur ve eksik eşlemeyi aynı PR'da düzeltir. 1b'nin 415 satırı satırın tamamıyla okunmuştu ve bu doğrulamanın dışındadır.
- **Yazım turunun 2a oturumu (2026-10-04, v0.7) — E-01…E-15'in kaynak satırları.** On beş ekranın kaynak satırları yukarıdaki ikinci betikle listelendi ve tam metinle okundu: Kullanıcı Akışları'nın 141 satırı — 119'u 1a'nın satırları, 22'si 1b'nin —, ayrıca 1a'nın 378 satırının tamamı bu on beş ekranın adlarıyla ve yerleriyle ("ana sayfa", "ürün sayfa", "sepet", "ödeme adım", "iletişim form", "İşlem rehberi", "sipariş takib" …) betikle tarandı ve işaretlenen 35 eşleşmenin 34 satırı tam okundu. **Ayrışan 13 satır:** on ikisinde "eksik ikinci ekran" — 2.1.2 (ortak bileşen: kart), 2.1.11 (E-04 — yöneticiye "Taslak" bandıyla açılan ürün sayfası), 2.4.8 (E-12 — siparişe girecek kalem kalmayınca sepete dönüş), 2.4.9 (E-27 — hesaba düşen sipariş), 2.5.3.1 ve 4.2.2 (E-12 — ödeme onayında sepetten çıkan kalemler), 2.5.3.2, 2.9.1 ve 4.1.21 (E-10 — hakkın iletişim formundan sürmesi), 4.1.29 (E-31, E-49 — firma bildiriminin ana sayfa uyarısı ve davet satırının işareti), 9.2.3 (E-12 — o tarayıcıda boş görünen sepet), 9.3.1 (E-10, E-40 — adın ön dolduruluşu ve üye kaydı görünümü); birinde **yanlış eşleme** — 2.5.1.2'den E-14 çıktı: kart dönüşü sipariş sayfasına iner (K-704; 3.5.8). Satırlar düzeltildi; E-04, E-10, E-12, E-14, E-27, E-31, E-40, E-49 ve "kart"ın sayıları yukarıdaki tabloda betikle yenilendi. Durum dağılımı değişmedi (1.193 satır — 1.026 eşlendi · 167 ekran dışı); hiçbir satır yeni bir GAP doğurmadı. **Kalan:** 1a'nın tam metinle hiç okunmamış satırlarından bu oturumun okuduğu pay yukarıdaki 119 satırın içindedir; hangi 111 satırın 2. oturumda okunduğu satır satır kayıtlı olmadığı için kesişim sayılamaz. Kapı değişmedi: 2b oturumu E-16…E-28'in satırlarını aynı yolla okur ve madde orada kapanır (karar kaydı §10.1).
- **Yazım turunun 2b oturumu (2026-10-04, v0.8) — E-16…E-28'in kaynak satırları ve 1a'nın kalanı.** On üç ekranın kaynak satırları yukarıdaki ikinci betikle listelendi ve tam metinle okundu: Kullanıcı Akışları'nın 246 satırı — 181'i 1a'nın, 65'i 1b'nin —, on üç ekranın beslendiği 30 devir satırı ve `02` ile `10 §2`'nin satırları. Ardından 1a'nın müşteri ekranlarına (E-01…E-28) eşlenmemiş **121 satırı** — yalnız panel ekranlarına eşlenmiş 73, yalnız ekran dışı değer ya da ortak bileşen taşıyan 48 — da tam metinle okundu. **Ayrışan 21 satır (1a):** on dokuzunda "eksik ikinci ekran" — 2.5.3.2 ve 4.1.22 (E-37 — indirme hakkını yönetici yeniler) · 2.9.2 ve 3.3.15 (E-37 — ayıp talebinin ayrıntısı ve işlemleri siparişin bölümündedir; K-751, K-764) · 3.3.4 (E-37 — iade reddi teslim alma adımındadır) · 3.1.1 (E-49 — davette yol yeni davettir) · 3.4.2 (E-20 — yeniden kayıt) · 4.1.1 (E-20, E-25 — yeniden kayıt ve değişikliğin yeniden başlatılması) · 5.2.4.2 (E-10 — formdan gelen cayma bildirimi) · 6.1.3.2 (E-22, E-29 — tanınan tarayıcıdan giriş) · 6.2.3.2 (E-16 — havalede IBAN yalnız sipariş sayfasından girilir) · 6.2.5.2 ve 9.3.6 (E-23 — şifre eski adrese giden bağlantıyla kurulur) · 6.2.5.3 (E-28 — süresi dolmuş geri alma bağlantısı) · 9.2.1 (E-12 — girişte sepet birleşmesi) · 9.2.5 (E-22 — yeni şifreyle giriş) · 9.3.2 (E-13 — ödeme adımında deftere kaydetme) · 9.3.5 (E-25, E-27 — bağlantı açıkken engel ve yeni adrese bağlanan siparişler) · 9.4.2 (E-25 — silmenin sonuç hâli; K-781); ikisinde **yanlış eşleme** — devir satırları K-586 ve K-589 E-16'ya eşlenmişti, iki karar da kaydı panelin sipariş ayrıntısına koyar (E-37); K-586'da E-16, e-posta geçmişinin müşteriye görünmediğini taşıdığı için kalır (5.16.10). İki satırın durumu "ekran dışı"ndan "eşlendi"ye döndü (6.1.3.2, 9.4.2). **1b'nin dört satırı** da aynı türden eksik taşıyordu ve düzeldi: 1.10.5 (E-22, E-28) · 3.5.4.16 (E-18, E-19 — yeni beyanlar güncel iade adresini alır) · 10.1.2.4 (E-27) · 10.1.2.5 (E-25). **Ölçüt:** "işlem izine yazılır" cümlesi E-52'ye eşleme doğurmadı — iz bütün yönetici işlemlerini toplar, satırın kendine özgü ekran izi değildir. Hiçbir satır yeni bir GAP doğurmadı. E-10, E-12, E-13, E-16, E-18…E-20, E-22, E-23, E-25, E-27…E-29, E-37 ve E-49'un sayıları yukarıdaki tabloda betikle yenilendi; durum dağılımı 1.193 satır — 1.028 eşlendi · 165 ekran dışı (§1.1.1). **Örneklem doğrulaması kapandı:** 1a'nın 378 satırının tamamı yazım turunda tam metinle okundu — E-01…E-15'e eşlenmiş 102 satırı 2a, E-16…E-28'e eşlenmiş 181 satırı ve müşteri ekranına eşlenmemiş 121 satırı 2b okudu (iki ekran kümesinin kesişimi 26 satır); okunmamış 1a satırı kalmadı. Yazım turu boyunca 1a'da bulunan ayrışma: 2. oturum 20, 2a 13, 2b 21 (karar kaydı §10.1).
- **Yazım turunun 3. oturumu (2026-10-04, v0.10) — E-29…E-37'nin kaynak satırları.** Dokuz ekranın kaynak satırları yukarıdaki ikinci betikle listelendi: Kullanıcı Akışları'nın 364 satırı — 130'u 1a'nın, 234'ü 1b'nin — ve `10 §2`'nin 20 satırı tam metinle okundu; Ürün Gereksinimleri'nin 47 ve devir dizinlerinin 65 satırı özet hücresiyle listelendi ve dayandıkları bölümler ve kararlar okundu (okuma derinliği karar kaydının §10.1'indedir). Ardından Kullanıcı Akışları'nın 793 satırının tamamı bu dokuz ekranın adları ve izleriyle ("panel giriş", "davet", "sayaç", "kurulum kontrol listesi", "ürün listesi", "stok alanı", "kategori", "kupon", "sipariş listesi", "panel satırı", "teslim alma adımı" …) betikle tarandı; işaretlenen 92 satırın kümede olmayan 51'i de tam okundu. **Ayrışan 11 satır:** yedisinde "eksik ikinci ekran" — 2.6.5, 3.1.1 ve 4.1.29 (E-36 — "e-posta ulaşmadı" işareti sipariş listesinin satırında da durur; 2.5.3.1) · 9.4.5 (E-36, E-37 — üye kaydı görünümünün siparişleri listeden okunur ve ayrıntıya açılır; 3.4.14) · 8.1.3 (E-37 — ayırmayı tutan sipariş firma iptaliyle kapatılır; 3.5.1.14'ün kalıbı) · 8.6.4.2 (E-37 — talebin gerektirdiği sipariş işlemi; 5.1.3'ün kalıbı) · 8.7.3.2 (E-37 — eski IBAN'lı siparişlerin firma iptali; 3.5.4.14'ün kalıbı); dördünde **yanlış eşleme** — 3.5.1.11, 3.5.1.16, 8.1.9 ve devir satırı K-655'ten E-32 çıktı: ürün listesinin satırında işlem yoktur, yayın durumu geçişleri ve kalıcı silme ürün formundadır (K-802). Ayrışanların dördü 1a'nın, yedisi 1b'nindir. **Ölçüt:** "sayaçta görünür", "sayaçtan düşer" cümlesi E-31'e eşleme doğurmaz — sayaçlar mevcut durumlardan hesaplanır ve bütün durumları toplar (`02 §10.6.1`); satırın kendine özgü ekran izi değildir (2b'nin işlem izi ölçütünün kalıbı). Yönetici işlemiyle tetiklenen B satırları K-731'in ölçütüyle "ekran dışı — e-posta" kaldı. Hiçbir satır yeni bir GAP doğurmadı. E-32, E-36 ve E-37'nin sayıları yukarıdaki tabloda betikle yenilendi; durum dağılımı değişmedi (1.193 satır — 1.028 eşlendi · 165 ekran dışı).
- **Yazım turunun 4. oturumu (2026-10-04, v0.11) — E-38…E-54'ün kaynak satırları.** On altı ekranın kaynak satırları yukarıdaki ikinci betikle listelendi — 279 eşleme, 225 tekil satır: Kullanıcı Akışları'nın 119 satırı (23'ü 1a'nın, 96'sı 1b'nin), `10 §2`'nin 18, Ürün Gereksinimleri'nin 60 ve devir dizinlerinin 28 satırı. Kullanıcı Akışları'nın ve `10 §2`'nin satırları tam metinle okundu; Ürün Gereksinimleri'nin satırlarının dayandığı bölümler ve devir kararları karar kaydının §10.1'indeki derinlikte okundu. Ardından Kullanıcı Akışları'nın 793 satırının tamamı bu ekranların adları ve izleriyle ("talep", "üye kaydı", "kurumsal içerik", "duyuru", "menü adı", "marka", "satış durumu", "firma kimliği", "ETBİS", "havale IBAN'ı", "iade adresi", "teslimat il", "aydınlatma metni", "yönetici listesi", "davet", "satış özeti", "işlem izi", "dışa aktar", "geri alma bağlantısı" …) betikle tarandı; işaretlenen 59 satır tam okundu. **Ayrışan 18 satır:** on beşinde "eksik ikinci ekran" — `03 §4.1.37` (E-38 — imha edilen talep listeden düşer) · `03 §8.2.11` (E-39 — talepteki numarayla sipariş araması; 9.11.9) · `03 §8.6.1.3` (E-41 — önizleme kaydın formundan başlar) · `03 §8.7.2.3` (E-45 — ayarın yanındaki hatırlatma) · `03 §9.5.6` (E-44 — firma bildirimlerinin adresi kimlik formunda yazar; durum "eşlendi"ye döndü) · `03 §3.5.4.15` ve devir satırı K-664 (E-37 — bekleyen havale siparişlerini kapatmanın yolu firma iptalidir; 3.5.4.14'ün kalıbı) · `02 §6.9` (E-37 — aynı iki satırın alt bölümü) · `03 §8.7.1.2`, §3.5.4.4, §3.5.4.10 (E-44 — "Satış durumu" bölümü karşılanmayan koşulu adıyla gösterir; K-752) · `03 §8.9.7` (E-51, E-52, E-53 — üç ekranın "yoktur" satırı; durum "eşlendi"ye döndü) · `02 §3.1.4` (E-44 — ETBİS alanı ve hatırlatması) · `02 §3.28.5` (E-42 — menüde gösterilen genel sayfalar) · `02 §3.28.6` (E-34, E-42 — kategorinin ve menü öğesinin görünmediğini söyleyen satır metni); üçünde **yanlış eşleme** — `03 §8.9.6`'dan E-39 çıktı, E-38 girdi: dışa aktarmanın "yoktur" satırı liste ekranındadır · `02 §3.33.8`'den E-47 çıktı, E-45 ve E-46 girdi · `03 §3.5.4.13`'ten E-44 çıktı, E-45 girdi: serbest metin hatırlatması ilgili ayarın yanındadır (K-816). Ayrışanların altısı 1a'nın, on ikisi 1b'nindir. **Ölçüt:** "talep doğar" ve "İletişim talebi: Açık" durumu E-38 eşlemesi doğurmaz — liste bütün talepleri toplar (3. oturumun "sayaçta görünür" ölçütünün kalıbı); bir ayarın üretilen metinlerin sürümünü artırması E-47 eşlemesi doğurmaz. Hiçbir satır yeni bir GAP doğurmadı. E-34, E-37, E-38, E-41, E-42, E-44…E-47 ve E-51…E-53'ün sayıları yukarıdaki tabloda betikle yenilendi; durum dağılımı 1.193 satır — 1.030 eşlendi · 163 ekran dışı (§1.1.1).

### 1.3 Boşluklar (GAP) ve kararlar

**Matrisin 2. oturumu (2026-10-04).** On bir boşluk: onu §1.1.11'in adaylarından doğrulandı, biri 2. oturumun taramasından geldi. Çalışma modu ⚠ öneriyle kayıttır (karar kaydı K-723): belirgin önerisi olan boşluk öneriyle karara bağlandı ve karar kaydının §2'sine K satırı, §3'üne GAP satırı olarak yazıldı; ⚠ işaretliler proje sahibine tek listede gösterilir (karar kaydı §10.1). Üç sınıf vardır: **(a) üst doküman boşluğu** — Ürün Gereksinimleri'nin ve Kullanıcı Akışları'nın cevaplamadığı ürün kuralı; karar aynı PR'da üst dokümana geri beslendi (K-652, K-726) · **(b) ekran kurgusu** — bu dokümanın kendi kararı; karara bağlanmadı, bir workshop ya da yazım konusuna bağlandı · **(c) yanlış adrese devir** — iş başka dokümanındır; hedef dokümanın park bloğuna satır yazıldı (K-36). "Proje sahibi kararı" sütunu kararın nasıl alındığını da söyler.

**Workshop (2026-10-04; karar kaydı K-740…K-761).** (b) sınıfındaki beş boşluk — GAP-4, GAP-6, GAP-7, GAP-10, GAP-11 — bağlandıkları workshop konularında karara bağlandı; satırları aşağıda güncellendi. Beşi de ekran kurgusudur ve üst dokümana kural döndürmez. Tabloda açık boşluk kalmadı: altısı matrisin 2. oturumunda (K-732…K-737), beşi workshop'ta (K-745, K-748, K-751, K-758; GAP-11 iki kararla — K-750, K-752) kapandı. Workshop'un yedi konusunun kararları — tasarım tabanı, mesaj ve onay kalıbı, vitrin çerçevesi, panelin gezinme iskeleti, iki ana sayfa düzeni, ödeme adımının yapısı, yeniden gönderilebilen e-postalar — karar kaydının §2'sinde yaşar; bu dokümanın §2–§9'u yazım turunda onlardan yazılır (K-30).

| # | Boşluk | Proje sahibi kararı | Nereye yansıdı |
|---|---|---|---|
| GAP-1 | **(a)** Müşterinin geri alınamaz dört işleminde — siparişin ya da kalemin iptali, gecikme nedeniyle fesih, cayma beyanı, hesabın silinmesi — onay adımı olup olmadığı yazılı değildi; onay penceresi yalnız yöneticinin işlemleri için sayılıyordu (GA-1) | **K-732 (öneriyle kaydedildi — ⚠).** Dört işlem tek dokunuşla tamamlanmaz: son adımda sonucu tek cümleyle gösterir ve müşteri onaylayarak tamamlar. Onay işlemin kendi ekranının son adımıdır — ek bekleme, vazgeçirme metni ya da ikinci kanal istemez; hakkın kullanımını zorlaştırmaz | `02 §5.9`, §7.6.1 (v0.56) · `03 §1.6.1` (v0.14) · bu doküman §2 (onay kalıbının müşteri varyantı — UI2-02, UI2-04), §5 (E-17, E-18, E-25) |
| GAP-2 | **(a)** Üyenin hesabına bağlanmış Google girişini kaldırıp kaldıramayacağı yazılı değildi (GA-3) | **K-733 (öneriyle kaydedildi — ⚠).** Kaldıramaz; hesap ekranında böyle bir işlem yoktur. Bağ yalnız e-posta değişikliğinin geri alınmasında, sistemce kalkar | `02 §3.13.7` (v0.56) · `03 §9.2.2` (v0.14) · bu doküman §5 (E-25 — kaldırma işlemi sunulmaz; giriş yollarının gösterimi ekranın yazımındadır, UI5-10) |
| GAP-3 | **(a)** Bekleyen e-posta değişikliğinin hesap ekranında nasıl göründüğü ve yeni adresini yanlış yazan üyenin ne yapacağı yazılı değildi (GA-4) | **K-734 (öneriyle kaydedildi).** Bekleyen değişiklik hesap ekranında yeni adresiyle görünür; yeni bir istek onun yerine geçer ve önceki bağlantı geçersizleşir. Ayrı "vazgeç" işlemi yoktur | `02 §3.13.14`, §8.2 L-9 (v0.56) · `03 §9.3.4`, §6.1.1.9 (v0.14) · bu doküman §5 (E-25) |
| GAP-4 | **(b)** Ödeme adımındaki giriş hatırlatmasından girişe giden müşterinin girişten sonra nereye döndüğü ve o ana kadar yazdığı e-posta ile adresin korunup korunmadığı yazılı değildi (GA-5) | **K-758 (öneriyle kaydedildi) — workshop, UI5-02.** Müşteri girişten sonra ödeme adımına döner: iletişim e-postası hesabın e-postasıdır, sepetler birleşir, misafirken yazdığı adres formda yeni adres olarak durur ve adres defteri seçenek olarak gelir, kupon yeniden uygulanır. Aynı dönüş, oturumu ödeme adımında kapanan üyede de işler. Yazılanlar yalnız o tarayıcıda ve o ziyaret boyunca korunur | Karar kaydı K-758, §3 · bu doküman §3 (girişten dönüş kuralı), §5 (E-13, E-22) · `05` "Aşama 3'ten park edilen girdiler" |
| GAP-5 | **(c)** Üç karar bir bildirim metnini bu dokümana devretmişti; e-postanın metni Entegrasyon Spesifikasyonu'nun (`08`) işidir (GA-6) | **K-735 (öneriyle kaydedildi).** Üç bildirimin metni — e-posta değişikliğinin eski adrese bildirimi, B-10, F-5 — `08`'e devredilir; bu dokümanda yalnız ekrandaki izleri kalır: geri alma ekranı (E-28) ve adres düzeltme ekranındaki teyit hatırlatması (E-37). F-5'in ekranda izi yoktur | `08` "Aşama 3'ten park edilen girdiler" · karar kaydı K-599, K-601, K-616 (geri işaret) · bu doküman §5 (E-28), §9 (E-37) |
| GAP-6 | **(b)** Giriş yapmış yöneticinin vitrindeki görünümü: "Taslak" bandı yazılı; yöneticinin vitrinden panele nasıl döndüğü ve vitrinin hesap ile sepet alanının yönetici oturumunda ne gösterdiği yazılı değildi (GA-8) | **K-748 (öneriyle kaydedildi) — workshop, UI3-01; panelden vitrine geçiş K-749 — UI3-02.** Yönetici vitrini ziyaretçi gibi görür; sayfanın başında "Panele dön" taşıyan bir yönetici şeridi durur, taslak önizlemesinde şerit "Taslak" bandıdır ve "Düzenlemeye dön" taşır. Hesap alanı ve sepet yönetici oturumundan etkilenmez — yönetici hesabı vitrinde oturum sayılmaz. Panel çerçevesi her ekranda "Siteyi görüntüle" bağlantısını taşır | Karar kaydı K-748, K-749, §3 · bu doküman §2 (taslak bandı ve yönetici şeridi), §3 |
| GAP-7 | **(b)** "Açık talep" sayacı tektir ve iletişim ile ayıp taleplerini birlikte sayar; sayacın götürdüğü listenin tek mi iki mi olduğu yazılı değildi (GA-9) | **K-751 (öneriyle kaydedildi) — workshop, UI3-02.** "Talepler" tek menü girişi ve tek listedir: iki tür tür sütunuyla birlikte listelenir, türe ve duruma göre süzülür; sayaç listenin açık taleplere süzülmüş hâline götürür. Satır türüne göre kendi ayrıntısına açılır; E-38 ve E-39'un envanterdeki karşılığı — tek liste, iki ayrıntı — §4'te yazılır | Karar kaydı K-751, §3 · bu doküman §3, §4 (E-38, E-39), §9 (UI9-07) |
| GAP-8 | **(a)** Yönetici işlem izinde ve bildirimlerde "adıyla" anılıyordu; yönetici hesabının adının nerede girildiği ve değiştiği yazılı değildi (GA-10) | **K-736 (öneriyle kaydedildi — ⚠).** Davetli hesabını açarken adını yazar (zorunlu); ilk yöneticinin adı kurulumda verilir; yönetici adını kendi hesabından değiştirir, yeniden doğrulama istemez | `02 §1.2` (Yönetici daveti), §10.2.1, §10.2.4 (v0.56) · `03 §8.8.2`, §9.3.7 (v0.14) · `10 §2` KP-62 (v0.38) · bu doküman §9 (E-30, E-50) |
| GAP-9 | **(a)** Panelin ürün listesinde arama ve süzme olup olmadığı yazılı değildi; katalog birkaç yüz ürüne kadardır (GA-11) | **K-737 (öneriyle kaydedildi — ⚠).** Ürün listesinde ürün adıyla arama ve yayın durumu süzgeci vardır. Kupon ve kurumsal içerik listelerine arama eklenmez; talep listeleri sayaçların götürdüğü süzülmüş listelerdir | `02 §10.1.2` (v0.56) · `03 §8.1.1` (v0.14) · `10 §2` KP-39 (v0.38) · bu doküman §9 (E-32) |
| GAP-10 | **(b)** Ürün çalışırken bir işlem beklenmeyen bir sebeple tamamlanamazsa ekranın müşteriye ve yöneticiye ne söylediği yazılı değildi (GA-12) | **K-745 (öneriyle kaydedildi) — workshop, UI2-02.** Ekran üç şeyi söyler: işlemin tamamlanmadığını, yazılanların ve sepetin durduğunu, ne yapılacağını; teknik ayrıntı göstermez. Üç hâl vardır: işlem düzeyi (mesaj işlemin yerinde, form korunur), sayfa düzeyi (çerçevenin içinde hata sayfası), sonucu belirsiz işlem (kullanıcı sonucu görmeye götürülür). Kalıbın yazımı boş, yükleniyor ve hata durumlarındadır (UI2-06) | Karar kaydı K-745, §3 · bu doküman §2 · `05` "Aşama 3'ten park edilen girdiler" |
| GAP-11 | **(b)** Üç yerleşim yazılı değildi: indirme hakkının ve KDV oranı varsayılanının değiştiği ayar ekranı; "IBAN bekleniyor" listesinin panel ana sayfasındaki yeri (2. oturumun taraması) | **K-750, K-752 (öneriyle kaydedildi) — workshop, UI3-02.** "IBAN bekleniyor" listesi panel ana sayfasının dördüncü bölümüdür ("Müşteriden beklenenler"); sayaç değildir. İndirme hakkı ve KDV oranının varsayılanı "Kargo, süreler ve varsayılanlar" ekranının "ürün varsayılanları" bölümündedir (E-46). Satışın geçici kapatılması anahtarı "Firma kimliği ve satış" ekranının başındaki "Satış durumu" bölümündedir (E-44); E-48 ayrı ekran olmaz | Karar kaydı K-750, K-752, §3 · bu doküman §9 (E-31, E-44, E-46 — UI9-02, UI9-09) |

**Düşen adaylar.** GA-2 (kupon alanının yeri — cevap `02 §3.10.1`'de: ödeme adımı) ve GA-7 (yasal metin içeriği — içerik kuralı `02`'de, `04`'e düşen yalnız metnin yeri) boşluk değildir; dayanakları §1.1.11'dedir.

**Geri besleme satırları (K-726).** Bu tablonun "Nereye yansıdı" sütunu Kullanıcı Akışları'ndaki `03 §11`'in karşılığıdır: bu dokümanın matrisinden ya da yazımından üst dokümana dönen her karar burada bir satırdır. 2. oturumun geri beslemesi: Ürün Gereksinimleri v0.56 · Kullanıcı Akışları v0.14 · MVP Kapsamı v0.38; üçünün de ✓ durumu korunur ve kalite döngüsü yeniden açılmaz — bu dokümanın etki yansıtması onları yeniden tarar.

**Workshop'un geri beslemesi (2026-10-04).** Yedi konunun yirmi iki kararından üçü bir ürün kuralı doğurdu ya da değiştirdi ve aynı PR'da üst dokümanlara yazıldı: Ürün Gereksinimleri v0.57 · Kullanıcı Akışları v0.15 · MVP Kapsamı v0.39. Öteki kararlar ekran kurgusudur ve bu dokümanda yaşar.

| Karar | Üst dokümana dönen kural | Nereye yansıdı |
|---|---|---|
| K-754 **(öneriyle kaydedildi — ⚠)** | Ana sayfanın ürün vitrini yayındaki en yeni ürünleri gösterir; firma ana sayfa için ürün seçmez | `02 §3.28.1` · `03 §2.1.1` · `10 §2` KP-31 · bu doküman §5 (E-01) |
| K-759 **(öneriyle kaydedildi — ⚠)** | "E-posta ulaşmadı" işaretini düşüren her müşteri bildirimi (B-1…B-16) panelden yeniden gönderilir | `02 §6.1.1`, §9.1.6 · `03 §7.1.37`, §8.9.2 · `10 §2` KP-68 · bu doküman §9 (E-37) · `08` park satırı |
| K-760 **(öneriyle kaydedildi — ⚠)** · K-761 **(öneriyle kaydedildi)** | Firmaya giden bildirim (F-1…F-4) ulaşmadığında satıra işaret düşmez; panel ana sayfasında "firma bildirimleri ulaşmıyor" uyarısı çıkar. İşaretli siparişler ana sayfanın uyarılarından süzülmüş listeye götüren bir satırla bulunur | `02 §6.1.1`, §9.1.6, §9.3.1, §10.6.1 · `03 §1.7.3.1`, §8.9.1, §10.1.2.2 · `10 §2` KP-50, KP-68 · bu doküman §2, §9 (E-31, E-36, E-37) · `05` park satırı |

**Yazım turunun 1. oturumu (2026-10-04, v0.6).** Yazım, kayıtta cevabı olmayan beş ekran kurgusu boşluğu buldu ve ⚠ öneriyle kayıt modunda (karar kaydı K-723) karara bağladı; beşi de bu dokümanın kendi kurgusudur, bir ürün kuralı doğurmaz ve üst dokümana dönmez — yukarıdaki geri besleme tablosuna satır girmedi, GAP numarası açılmadı (matrisin boşluğu değil, yazımın boşluğudur).

| Karar | Yazımın bulduğu boşluk | Karar özeti | Bu dokümanda |
|---|---|---|---|
| K-765 **(öneriyle kaydedildi)** | Boş ve yükleniyor hâlinin ortak kalıbı yazılı değildi | Boş bölge ya görünmez ya da neyin olmadığını söyleyen tek satır ve çıkış yolu taşır; içerik beklenen bölge kendi yerinde bekleme göstergesi gösterir, ekran örtülmez | §2.7.1, §2.7.2 |
| K-766 **(öneriyle kaydedildi)** | Uzun listelerin nasıl bölündüğü yazılı değildi | Sayfa numaralı gezinme; sonsuz kaydırma yok; vitrinde sayfa başına 24 ürün, panelde 25 kayıt | §2.13.3 |
| K-767 **(öneriyle kaydedildi)** | Süre göstergesinin birimi yazılı değildi | Kalan gün ve son günün tarihi; saat ve dakika sayan canlı geri sayım yok | §2.6.6 |
| K-768 **(öneriyle kaydedildi)** | Ödeme adımı dışındaki girişlerde ve çıkışta dönüş yeri yazılı değildi | Oturum isteyen ekrana gelen kullanıcı girişten sonra o ekrana, girişi kendisi açan müşteri geldiği sayfaya döner; çıkışta müşteri sayfasında ya da ana sayfada, yönetici panel girişinde kalır; hesap ekranları iç gezinme taşır | §3.6, §3.3.32 |
| K-769 **(öneriyle kaydedildi)** | Panel ana sayfasının uyarılarının götürdüğü yerlerden ikisi yazılı değildi | Kart iadesi uyarısı "iade ve geri ödeme bekleyen" listesine götürür; kanal uyarısı geçiş taşımaz — çözümü kurulum ayarıdır | §3.4.3, §2.5.5 |

Yazım konvansiyonları, alt bölüm haritası ve ekran envanteri de kayda geçti (K-762, K-763, K-764); üçü yöntem ve yerleşim kararıdır.

**Yazım turunun 2a oturumu (2026-10-04, v0.7).** On beş müşteri ekranının yazımı dokuz boşluk buldu ve ⚠ öneriyle kayıt modunda (karar kaydı K-723) karara bağladı; dördü 1. oturumun K satırı açmadan yazdığı türetimlerdi ve kaynakta dayanakları ya eksik ya da örtüktü. Biri bir ürün kuralının metnini ekranla hizaladı ve üst dokümanlara geri beslendi (K-774 — Ürün Gereksinimleri v0.58, MVP Kapsamı v0.40); ötekiler ekran kurgusu, arayüz metni ya da süreç kararıdır. GAP numarası açılmadı — matrisin değil yazımın boşluklarıdır.

| Yazımın bulduğu boşluk | Karar | Karar özeti | Bu dokümanda |
|---|---|---|---|
| Yayına hiç alınmamış aydınlatma metninin ve çerez politikasının vitrindeki hâli | K-770 **(öneriyle kaydedildi — ⚠)** | İlk yayına kadar altbilgide bağlantı yoktur ve adres "sayfa bulunamadı" döner; taslak metin gösterilmez | §5.11.9 |
| Dijital ve hizmet kalemi kutularının nihai metni (devir: K-550) | K-771 **(öneriyle kaydedildi — ⚠)** | `02 §3.24.2`'nin metni; kutu kapsadığı kalemleri adıyla sayar, çok kalemde çoğul söyler | §2.12.7.6, §5.13.9 |
| Kaynağın metnini bıraktığı mesajlar (devir: K-125, K-138, K-594, K-602, K-678) | K-772 **(öneriyle kaydedildi)** | Kaynağın örneği nihai metindir; örneği olmayan beş mesajın metni sabitlendi | §5.10, §5.12, §5.13, §5.15 |
| Teslimat il kısıtının adres adımından önce görünmesi (devir: `02 §3.20.1`) | K-773 **(öneriyle kaydedildi)** | Aynı satır ürün sayfasında, sepette ve ödeme adımının teslimat bölümünde | §5.4.5, §5.12.3, §5.13.6 |
| Ürün kartının sepete ekleme taşımaması — 1. oturumun türetimi | K-774 **(öneriyle kaydedildi — ⚠)** | Kart yalnız ürün sayfasını açar; K-39'un "kartın içinde"si "ürün başına tek kart" diye okunur | §2.11.1, §5.2.4, §5.4.7 |
| Sepet kaleminden ürün sayfasına geçiş — 1. oturumun türetimi | K-775 **(öneriyle kaydedildi)** | Kalemin görseli ve adı ürün sayfasını açar | §3.3.13, §5.12.4 |
| Teyit ekranından sipariş sayfasına bağlantı — 1. oturumun türetimi | K-776 **(öneriyle kaydedildi)** | Teyit ekranı yeni giriş yolu açmaz: üyeye sipariş sayfası, misafir alıcıya sipariş takibi; kartta süreli yönlendirme yok | §3.3.19, §5.14 |
| Siparişin iki ekseninin iki rozetle gösterilmesi — 1. oturumun türetimi | K-777 **(öneriyle kaydedildi)** | İki ayrı, adlı rozet; birleşik etiket yok | §2.5.1 |
| 2. oturumun bölünmesi | K-778 **(öneriyle kaydedildi)** | 2a §5.1…§5.15, 2b §5.16…§5.28 | Başlık notu |

**2a oturumunun geri beslemesi (K-652, K-726).** Ürün Gereksinimleri v0.58 · MVP Kapsamı v0.40; ikisinin de ✓ durumu korunur ve kalite döngüsü yeniden açılmaz.

| Üst dokümana dönen kural | Karar | Nereye yansıdı |
|---|---|---|
| Ziyaretçi vitrinde ürün başına tek kart görür — varyant başına kart yoktur — ve varyant seçimini kartın açtığı ürün sayfasında yapar (kural değişmedi; metin ekranla hizalandı) | K-774 **(öneriyle kaydedildi — ⚠)** | `02 §3.3.1` · `10 §2` KP-4 · bu doküman §2.11.1, §5.2, §5.4 |

1. oturumun beşinci türetimi — eşzamanlı düzenleme uyarısının kayıt varyantındaki iki yol (§2.10.1: güncel hâli açmak ya da uyarıyı görerek üzerine yazmak) — panelin ekranlarına aittir; `02 §3.31.1` "üzerine yazmadan önce uyarılır" der ve iki yolu adıyla saymaz. Gözden geçirmesi panelin ilk ekranlarını yazan 3. oturumundur (karar kaydı §10.1).

**Yazım turunun 2b oturumu (2026-10-04, v0.8).** On üç müşteri ekranının yazımı sekiz boşluk buldu ve ⚠ öneriyle kayıt modunda (karar kaydı K-723) karara bağladı. Biri Ürün Gereksinimleri'nin bir sayımını Kullanıcı Akışları'nın satırıyla (`03 §9.3.4`) hizaladı ve geri beslendi (K-786 — v0.59); ötekiler ekran kurgusu ve arayüz metnidir. GAP numarası açılmadı — matrisin değil yazımın boşluklarıdır.

| Yazımın bulduğu boşluk | Karar | Karar özeti | Bu dokümanda |
|---|---|---|---|
| Kaynağın metnini bu dokümana bıraktığı hesap ve giriş mesajları (devir: K-659, K-697, K-698) | K-779 **(öneriyle kaydedildi)** | Altı mesajın metni sabitlendi; doğrulanmamış hesabın mesajı yalnız e-posta ve şifre doğruysa görünür | §5.20, §5.22, §5.25 |
| Doğrulama bağlantısının iniş ekranında süresi dolmuş bağlantı dışındaki hâller | K-780 **(öneriyle kaydedildi)** | Altı hâl, her birinde tek sonraki adım; yerine yenisi gönderilmiş bağlantı yeniden kayda yönlendirmez | §5.21, §3.3.29, §3.3.34, §3.3.35 |
| Hesap silmenin sonuç ekranı (devir: `04 §3.3.31`'in geçici hedefi) | K-781 **(öneriyle kaydedildi)** | Hesap ekranının sonuç hâli; bir kez görünür, yenilenince ana sayfa | §5.25.13, §3.3.31 |
| Geri alma ekranının hâlleri ve geçersiz bağlantıdaki yol (devir: K-735) | K-782 **(öneriyle kaydedildi)** | Geri alındı ya da geçersiz; geçersiz bağlantıda ürün içi yol olmadığı ve İletişim sayfası | §5.28 |
| Ayıp talebinin yeniden açılmasında açıklama alınıp alınmadığı | K-783 **(öneriyle kaydedildi)** | Açıklama alanı yoktur; ek bilgi firmanın e-postasına yazılır | §5.19.4 |
| Sipariş geçmişinin sırası, sayfa boyu ve satırı | K-784 **(öneriyle kaydedildi)** | En yeni önce, sayfa başına 25 sipariş; satır numara, tarih, ilk kalem, toplam ve iki rozet | §5.27.4 |
| Müşterinin geri alınamaz dört işleminde onay düğmesinin metni ve sonuç cümlesi | K-785 **(öneriyle kaydedildi — ⚠)** | Beş düğme metni ve beş sonuç cümlesi; her biri "Bu işlem geri alınamaz"la | §5.17.6, §5.18.7, §5.25.13 |
| E-posta değişikliğinde yeni adres başka bir müşteri hesabınınsa ekranın ne söylediği | K-786 **(öneriyle kaydedildi — ⚠)** | Dürüst mesaj; hesabın varlığını ele veren üçüncü yer olarak Ürün Gereksinimleri'ne yazıldı | §5.25.11 |

**2b oturumunun geri beslemesi (K-652, K-726).** Ürün Gereksinimleri v0.59; ✓ durumu korunur ve kalite döngüsü yeniden açılmaz.

| Üst dokümana dönen kural | Karar | Nereye yansıdı |
|---|---|---|
| Hesabın e-posta değişikliği, yeni adres başka bir müşteri hesabınınsa bunu söyler; ekranların hesabın varlığını ele verdiği yerlerden biridir (kural `03 §9.3.4`'te vardı; `02`'nin sayımı hizalandı) | K-786 **(öneriyle kaydedildi — ⚠)** | `02 §3.13.11`, §8.1.4 · bu doküman §5.25 |

**Kısmi adet kararı (2026-10-04, v0.9).** 2b oturumunun yöneticiye ilettiği risk — müşterinin iptali ve caymayı bir kalemin bütün adedi yerine bir kısmı için kullanıp kullanamayacağı — proje sahibinin kararıyla kapandı: adet seçilir (K-787; K-178'i genişletir). Kararın açtığı detaylar ⚠ öneriyle kayıt modunda (karar kaydı K-723) karara bağlandı. Gecikme feshi sorunun kapsamında değildi; fesihte seçim yoktur ve K-571 sürer (K-796 — yönetici kararı).

| Karar | Karar özeti | Bu dokümanda |
|---|---|---|
| K-787 (proje sahibinin kararı) | İptal, cayma ve iadede müşteri adedi birden büyük kalemin kaç adedini işleme sokacağını seçer; para işleme giren adet kadar döner | §5.17.4, §5.18.4 |
| K-788 **(öneriyle kaydedildi — ⚠)** | İşleme giren adetlerin tutarı kalemin donmuş değerlerinden ayrılır; kupon payı ve KDV adetle orantılı, kuruşa aşağı; kalemi kapatan son işlem kalanı alır | — (ekran tutar hesaplamaz — K-785) |
| K-789 **(öneriyle kaydedildi — ⚠)** | Kargo ücretinin iki tetiği bütün adetlerle okunur; bir adet bile kargoyla gidiyor ya da müşteride kalıyorsa kargo ücreti geri ödenmez; adede bölünmez | §5.17.5, §5.18.6 |
| K-790 **(öneriyle kaydedildi)** | Yeni durum yoktur; kalem kayıtları adet taşır, açık adet kayıtlardan okunur, ardışık işlemler ayrı kayıttır | §5.16.5, §5.18.4 |
| K-791 **(öneriyle kaydedildi)** | Stok, hizmet kontenjanı ve kupon hakkı işleme giren adet kadar döner; kupon hakkı yalnız bütün adetlerle | — (ekran dışı — arka plan) |
| K-792 **(öneriyle kaydedildi)** | Dijital kalemin adedi birdir; hizmet kaleminde iptal ve cayma adetle, "tamamlandı" işareti açık adetlerin tamamına | §2.12.1.8, §5.18.6 |
| K-793 **(öneriyle kaydedildi)** | Ayıp talebi ayıplı adedi taşır; tek talep kuralı değişmez | §5.19.3, §5.19.4, §5.19.9 |
| K-794 **(öneriyle kaydedildi — ⚠)** | Firma iptali, iade teslim alma, iade reddi ve "mal dönmedi" kapanışı adetle; manuel adım bütçesi değişmez | §9.9.19, §9.9.25, §9.9.27, §9.9.28 |
| K-796 **(öneriyle kaydedildi — ⚠)** | Gecikme feshinde kalem ve adet seçilmez; fesih teslim edilmemiş bütün kalemlerin bütün adetlerine uygulanır (K-571 sürer) | §5.17.4 |
| K-795 **(öneriyle kaydedildi)** | Adet alanı OB-12'nin varyantıdır; varsayılan iptalde ve caymada açık adedin tamamı, ayıp talebinde bir; son adım kalemleri adetle yazar ve sonuç cümlesinin sayısı adetlerin toplamıdır | §2.12.1.8, §5.16.5, §5.17.4, §5.17.6, §5.17.8, §5.17.12, §5.18.4, §5.18.7, §5.18.9, §5.18.12, §5.19.3, §5.19.9 |

**Kısmi adet kararının geri beslemesi (K-652, K-726).** Ürün Gereksinimleri v0.60, Kullanıcı Akışları v0.16, MVP Kapsamı v0.41, Proje Vizyonu v0.33; dördünün ✓ durumu korunur ve kalite döngüsü yeniden açılmaz.

| Üst dokümana dönen kural | Karar | Nereye yansıdı |
|---|---|---|
| İptal, cayma ve iade kalem düzeyinde ve adetle işler; kalem parça parça kapanabilir; "kalemlerin tamamı" bütün adetlerdir; gecikme feshinde seçim yoktur | K-787, K-790 | `02 §1.2`, §5.8, §7.1.3, yeni §7.1.6, §7.2.2, §7.3.5 · `03 §1.4.3`, yeni 1.7.4, §1.11 · `10 §2` KP-21, KP-22, KP-24 · `01 §6` M-4 |
| İşleme giren adetlerin tutarı: birim fiyat × adet, kupon payı ve KDV adetle orantılı ve aşağı yuvarlanır, son işlem kalanı alır | K-788 **(öneriyle kaydedildi — ⚠)** | `02 §3.8.3`, §3.10.3, yeni §7.1.7, §7.2.7, §10.4.3 · `03 2.7.6`, 2.8.4.1, 8.3.3.3 · `10 §2` KP-24, KP-47 |
| Kargo ücreti ve ücretsiz kargo kararı kısmi adet işleminde yeniden hesaplanmaz; iki tetik bütün adetlerle okunur | K-789 **(öneriyle kaydedildi — ⚠)** | `02 §3.19.6`, §4.3, §6.4.10, §7.2.9, §7.4.6 · `03 3.3.10`, 4.2.6 · `10 §2` KP-24 |
| Sayaçlar adet kadar döner | K-791 **(öneriyle kaydedildi)** | `02 §3.6.6`, §7.2.6, §7.4.8 · `03 §1.4.4` |
| Hizmet kaleminde adet; kısmen ifa edilmiş hizmetten cayılan adetlerin bedelinin tamamı | K-792 **(öneriyle kaydedildi)** | `02 §7.2.5`, §7.3.3, §6.4.28, §12.1.9 · `03 2.7.3`, 2.8.2.2 · `10 §2` KP-22 |
| Ayıp talebinde ayıplı adet | K-793 **(öneriyle kaydedildi)** | `02 §5.8`, §7.5.2, §7.5.3 · `03 2.9.2`, 8.4.10 · `10 §2` KP-23 |
| Firma iptali, iade teslim alma, iade reddi ve "mal dönmedi" kapanışı adetle; bütçe değişmez | K-794 **(öneriyle kaydedildi — ⚠)** | `02 §7.2.4`, §7.4.7, §7.4.9, §9.2 B-7, §10.4.9, §10.5.2 · `03 8.3.1.1`, 8.3.2.1–8.3.2.3, 8.4.7, §8.5 · `10 §2` KP-47, KP-48 |

**Yazım turunun 3. oturumu (2026-10-04, v0.10).** Panelin ilk dokuz ekranının (E-29…E-37) yazımı on iki boşluk buldu ve ⚠ öneriyle kayıt modunda (karar kaydı K-723) karara bağladı; dördü ⚠ grubundadır. Üçü bir ürün kuralının kaynakta yazılmamış yüzünü yazdı ya da iki üst dokümanı hizaladı ve geri beslendi (K-805, K-806, K-808); ötekiler arayüz metni ve ekran kurgusudur ve bu dokümanda yaşar. GAP numarası açılmadı — matrisin değil yazımın boşluklarıdır.

| Yazımın bulduğu boşluk | Karar | Karar özeti | Bu dokümanda |
|---|---|---|---|
| Sipariş ayrıntısının ve ürün formunun düğmeleri ile onay cümleleri (devir: K-537, K-655, K-656, K-677, K-715) | K-797 **(öneriyle kaydedildi)** | Düğme işlemin adını taşır; onay sonucu tek cümleyle, geri alınamaz işlemde ardından "Bu işlem geri alınamaz." ile söyler; tutar sistemin hesabıdır, süre yazılmaz | §9.5.15, §9.9.15–§9.9.30 |
| Panelin yasal içerikli uyarıları (devir: K-573, K-601, K-606, K-607, K-609, K-611, K-612, K-709) | K-798 **(öneriyle kaydedildi — ⚠)** | Yedi uyarının metni; süre dışı ya da istisnalı kayıtta ayrı onay bir onay kutusudur | §2.4.3, §9.5.6, §9.9.4, §9.9.19, §9.9.20, §9.9.31 |
| Panelin engel, ret ve uyarı satırı mesajları (devir: K-538, K-554, K-566, K-600, K-653, K-654, K-674, K-703) | K-799 **(öneriyle kaydedildi)** | Mesaj sebebi ve çıkış yolunu söyler; ana sayfanın uyarı satırları sabit sırayladır | §9.2.4, §9.3.3, §9.5, §9.6, §9.7, §9.9 |
| Eşzamanlı düzenleme uyarısının kayıt varyantındaki iki yol — 1. oturumun türetimi (2a'nın notu) | K-800 **(öneriyle kaydedildi)** | İki yol kalır — "Güncel hâli aç" · "Yine de kaydet"; kaynağın "üzerine yazmadan önce uyarılır" cümlesinin (`02 §3.31.1`) karşılığıdır | §2.10.1, §9.5.20, §9.7.7 |
| Stok kodunun ön doldurulması ve düzenlenmesi (devir: `02 §3.3.4`) | K-801 **(öneriyle kaydedildi)** | Sistemin önerdiği tekil kodla gelir ve her zaman düzenlenir | §9.5.5 |
| Panel listelerinin sırası ve satırı; sipariş listesinin araması, süzgeci ve dar sınıftaki kartı (devir: `03 §8.2.11`, K-714, K-741) | K-802 **(öneriyle kaydedildi)** | En yeni önce, süreli süzgeçte kalan süresi en az olan önce; tam eşleşmeli arama; bekleyen iş süzgeci; kart dört bilgi taşır | §9.4, §9.7, §9.8 |
| Sipariş ayrıntısının kurgusu (devir: `04 §2.5.3`, K-741) | K-803 **(öneriyle kaydedildi)** | İki sütun; birincil düğme hattın sıradaki zorunlu adımıdır; "Sipariş işlemleri" ve "Düzeltmeler" grupları | §9.9.2–§9.9.13 |
| Siparişin gönderim kaydının ve e-posta geçmişinin panelde okunması | K-804 **(öneriyle kaydedildi)** | Sipariş ayrıntısının "E-posta geçmişi" bölümü | §9.9.12 |
| Aynı seviyedeki kategorilerin sırası | K-805 **(öneriyle kaydedildi — ⚠)** | Alfabetik; elle sıralama yoktur | §2.2.3, §5.1.6, §9.5.3, §9.6.3 |
| Kupon kodunun tekilliği, kuponun düzenlenmesi ve silinmesi | K-806 **(öneriyle kaydedildi — ⚠)** | Kod tekil ve büyük-küçük harf ayrımsız; alanlar düzenlenir; silme yoktur | §2.12.6.2, §9.7 |
| Davet kabulünden sonra | K-807 **(öneriyle kaydedildi)** | Panel girişine götürür; hesabı açmak oturum açmaz | §9.2.6, §3.4.18 |
| Ayıp talebinde sözleşmeden dönmenin panel yolu | K-808 **(öneriyle kaydedildi — ⚠)** | Talebin ayıplı adetleri için iade hattı: teslim alma, ardından geri ödeme | §9.9.27, §9.9.30 |

**3. oturumun geri beslemesi (K-652, K-726).** Ürün Gereksinimleri v0.61 · Kullanıcı Akışları v0.17; ikisinin de ✓ durumu korunur ve kalite döngüsü yeniden açılmaz. MVP Kapsamı değişmedi.

| Üst dokümana dönen kural | Karar | Nereye yansıdı |
|---|---|---|
| Aynı seviyedeki kategoriler adlarının alfabetik sırasıyla dizilir; firma kategorileri elle sıralamaz | K-805 **(öneriyle kaydedildi — ⚠)** | `02 §3.4.1` · `03 §2.1.2`, §8.1.14 · bu doküman §2.2.3, §5.1.6, §9.6 |
| Kupon kodu kuponlar arasında tekildir ve büyük-küçük harf ayrımı olmadan eşleşir; kupon silinmez, alanlarının değişikliği yeni siparişlere işler | K-806 **(öneriyle kaydedildi — ⚠)** | `02 §3.10.5` · `03 §2.4.4`, §8.1.12 · bu doküman §2.12.6.2, §9.7 |
| Ayıp talebinde sözleşmeden dönmenin geri alınan malı teslim alma adımıyla alınır (kural `02 §7.5.3`'teydi; `03`'ün aralığı hizalandı) | K-808 **(öneriyle kaydedildi — ⚠)** | `03 §1.11.28`, §8.3.2.1 · bu doküman §9.9 |

**Yazım turunun 4. oturumu (2026-10-04, v0.11).** Panelin kalan on altı ekranının (E-38…E-47, E-49…E-54) yazımı on beş boşluk buldu ve ⚠ öneriyle kayıt modunda (karar kaydı K-723) karara bağladı; altısı ⚠ grubundadır. Üçü bir ürün kuralının kaynakta yazılı olmayan parçasıydı ve Ürün Gereksinimleri'ne geri beslendi (K-813, K-818, K-821); öteki on ikisi bu dokümanın kendi kurgusudur ve üst dokümana dönmez. Devir maddesinin 4. oturum payı bu kararlarla kapandı: onay cümleleri (K-814), işaretli kayıt sayısı (K-812), marka renginin örneği (9.15.6), eksik alan gösterimi (K-812), başka bir yöneticinin adresinin mesajı (K-819), E-54'ün farkları (K-819), satış özetinin dönemi ve sıfır paydası (K-821).

| Yazımın bulduğu boşluk | Karar | Karar özeti | Bu dokümanda |
|---|---|---|---|
| Talepler listesinin süzgeç değerleri, satırı ve sırası (devir: K-751) | K-809 **(öneriyle kaydedildi)** | Tür ve durum süzgeci — durumun üç değeri birlikte; iki türün satırı aynı sütunlarda; açılış tarihine göre en yeni önce | §9.10 |
| İletişim talebi ayrıntısının içeriği, düğmeleri ve KVKK talebinde dayanağa geçiş | K-810 **(öneriyle kaydedildi)** | Konu, ulaşma anı, gönderen, tam mesaj; "Talebi kapat" ve "Yeniden aç" onaysız; "Siparişlerde ara" ve "Üye kaydında ara" | §9.11, 3.4.21 |
| Üye kaydı görünümünün araması, doğrulanmamış kayıt, silme engelinin ve sonucunun metni (devir: K-698) | K-811 **(öneriyle kaydedildi — ⚠)** | Tek e-posta alanı, tam eşleşme; doğrulanmamış kayıt da bulunur; üç mesajın metni | §9.12 |
| Kurumsal içerik ekranının kurgusu, işaretli kayıt sayısı ve eksik alan gösterimi (devir: K-584, K-755) | K-812 **(öneriyle kaydedildi)** | Yedi gruplu liste, elle sıranın düğmeleri, "yayın için zorunlu" etiketi; beş metin | §9.13 |
| Ana sayfa ve menü ile marka ekranlarının kurgusu; ana sayfa düzeninin kurulumdaki değeri yazılı değildi | K-813 **(öneriyle kaydedildi)** | Tek form, tek "Kaydet"; menü adı boş kaydedilmez; renk alanı boşaltılabilir; kurulumda tanıtım öncelikli | §9.14, §9.15 |
| Satışın geçici kapatılması, IBAN değişikliği, havalenin kapatılması, iade adresi değişikliği ve yönetici kaldırmanın onay cümleleri (devir: K-662…K-665, K-752) | K-814 **(öneriyle kaydedildi — ⚠)** | Beş cümle ve beş onay düğmesi; iade adresinde eski adrese gelen malın yükümlülüğü | 9.16.7, 9.17.7, 9.17.8, 9.18.10, 9.20.10 |
| Firma kimliği ve satış ekranının "Satış durumu" satırları ve ETBİS hatırlatmasının metni (devir: K-574, K-752) | K-815 **(öneriyle kaydedildi — ⚠)** | Beş koşul satırı; tip değişikliği ve siparişe donma notu; ETBİS hatırlatması | §9.16 |
| Ayarla çelişen serbest metnin hatırlatma satırının yeri ve metni (`02 §6.9.13`) | K-816 **(öneriyle kaydedildi)** | Havale, kargo ve eşikler, süreler ve iade adresi bölümlerinin altında tek satır | 9.17.4, 9.18.3, 9.18.5, 9.18.7 |
| Ödeme ve kargo ayarlarının alan kuralları ve mesajları | K-817 **(öneriyle kaydedildi)** | Firmanın IBAN'ı biçim ve sağlama denetimiyle; hiç il seçilmemesinin satırı; iki çit mesajı; iade adresinin alanları | §9.17, §9.18 |
| Yasal metinler ekranının kurgusu — taslak ve yayındaki sürüm, üretilen metinlerin gösterimi, biçim seti | K-818 **(öneriyle kaydedildi — ⚠)** | Dört metin satırı; "Taslağı kaydet" ve "Yayına al"; yayından çekme yoktur; kapalı biçim seti iki metinde de | §9.19, 3.4.22 |
| Yöneticinin kendi hesabının mesajları ve doğrulama ekranlarının müşteri tarafından farkları (devir: K-780, K-782, K-786) | K-819 **(öneriyle kaydedildi)** | İki mesaj; yeniden doğrulama yalnız şifreyle; iniş ekranlarının panele özgü hâlleri | §9.21, §9.25 |
| Yönetici hesapları ekranının listeleri ve mesajları | K-820 **(öneriyle kaydedildi)** | Yöneticiler ve yalnız geçerli davetler; "Yeni davet gönder"; iki engel mesajı | §9.20 |
| Satış özetinin dönem seçimi, düzeni, sıfır payda gösterimi ve cironun geri ödemelerle ilişkisi (devir: `02 §10.6.2`, §10.6.3; K-490) | K-821 **(öneriyle kaydedildi — ⚠)** | Hazır dönemler ve ölçüm penceresi; pay ve payda sayıyla; "Değerlendirilemez"; ciro geri ödemeleri düşmez | §9.22 |
| İşlem izi ekranının satırı ve süzgeçleri | K-822 **(öneriyle kaydedildi)** | Kim, ne zaman, ne değişti; kaldırılmış yönetici süzgeçte; satırdan geçiş yok | §9.23 |
| Dışa aktarma ekranının sorumluluk satırı ve dosyaların kapsamı | K-823 **(öneriyle kaydedildi — ⚠)** | Sorumluluk satırının metni; sipariş aralığı oluşma gününe göre; üye ve talep listesi tümü | §9.24 |

**4. oturumun geri beslemesi (K-652, K-726).** Ürün Gereksinimleri v0.62; ✓ durumu korunur ve kalite döngüsü yeniden açılmaz. Kullanıcı Akışları, MVP Kapsamı ve Proje Vizyonu değişmedi.

| Üst dokümana dönen kural | Karar | Nereye yansıdı |
|---|---|---|
| Ana sayfa düzeni kurulumda tanıtım öncelikli seçili gelir | K-813 **(öneriyle kaydedildi)** | `02 §3.28.1` · bu doküman 9.14.3 |
| Kapalı metin biçimi seti aydınlatma metninde ve çerez politikasında da geçerlidir | K-818 **(öneriyle kaydedildi — ⚠)** | `02 §3.11.6` · bu doküman 9.19.3 |
| Satış özetinin cirosu ödemesi tamamlanmış siparişlerin onaylanan toplamıdır; sonraki geri ödemeler düşülmez | K-821 **(öneriyle kaydedildi — ⚠)** | `02 §10.6.2` · bu doküman 9.22.4 |

> **Vaka:** Bu matris bir referans projede 7 boşluk yakaladı — hiçbiri o ana kadar hiçbir dokümanda adreslenmemiş ama arayüzde cevap gerektiren sorulardı.

---

## 2. Ortak bileşen kütüphanesi

> **Ne yazılır:** Tekrar eden UI kalıpları — durum rozetleri, modal'lar, geri sayım göstergeleri, boş/yükleniyor/hata durumları.
> **Ekran tanımlarından ÖNCE gelir.** Sonradan çıkarılırsa ekranlar tutarsız yazılır.

**Yazım turunun 1. oturumu (2026-10-04, v0.6).** Bölüm tasarım tabanıyla açılır (K-726) ve yirmi bileşeni tanımlar; her bileşen ne gösterdiğini, varyantlarını, davranışını, kullanıldığı ekranları ve kaynağını taşır. Bileşen bir arayüz kalıbının tanımıdır; teknoloji ya da paket seçimi değildir (konvansiyon 15). "Kullanıldığı ekranlar"ın ilk kümesi matristen betikle çıkarıldı — bileşen adını taşıyan matris satırlarının "Ekran" hücresindeki kimlikler (§1.2'nin betikleri) —; ikinci kümesi bileşeni kuran kararın saydığı ekranlardır. Ekran tanımını yazan oturum listeyi tanımın gösterdiği kullanımla aynı PR'da günceller. Matrisin on beş ortak bileşen adının her biri aşağıdaki tabloda bir kimliğe bağlıdır; beş bileşen (OB-12, OB-17…OB-20) matriste ad taşımaz, konu planının saydığı kalıplardır (karar kaydı §10.3 UI2-07) ya da yazımın bulduğudur (OB-20; K-766).

| Bileşen | Matris adı | Ne gösterir | Varyantlar | Kullanıldığı ekranlar | Alt bölüm |
|---|---|---|---|---|---|
| OB-01 Tasarım tabanı | taban | Üç genişlik sınıfını, sınıflara göre düzen ilkelerini, marka renginin yerlerini ve erişilebilirlik tabanını | dar · orta · geniş | Bütün ekranlar | §2.1 |
| OB-02 Çerçeve | çerçeve | Her ekranı saran sabit bölgeleri: vitrinde duyuru şeridi, üst bölüm, menü ve altbilgi; panelde üst satır ve menü | vitrin · sadeleşmiş vitrin · panel | E-01…E-28 (vitrin; E-13 sadeleşmiş) · E-31…E-54 (panel) | §2.2 |
| OB-03 Mesaj | mesaj | Hatayı, uyarıyı, bilgiyi ve başarıyı dört yerde, simge ve metinle | alan mesajı · yerinde kalıcı mesaj · bir kez söylenen mesaj · kısa süreli bildirim | Form ya da işlem taşıyan bütün ekranlar | §2.3 |
| OB-04 Onay | onay | Geri alınamaz ya da sonucu sayıyla bildirilen işlemin son adımını | yöneticinin penceresi · müşterinin son adımı · "geri alınamaz" cümlesi olmayan ayar onayı | E-16, E-17, E-18, E-25 · E-33, E-37, E-40, E-41, E-44, E-45, E-46, E-49 | §2.4 |
| OB-05 Rozet ve işaretler | rozet | Durumu, kalem kaydını ve koşulu süren panel işaretini metinli bir etiketle | durum rozeti · kalem kaydı · panel işareti · vitrin işareti | E-04, E-05, E-08, E-12, E-14, E-16, E-19, E-27 · E-31, E-32, E-33, E-36, E-37, E-38, E-39, E-40, E-41, E-49 | §2.5 |
| OB-06 Sayaç ve süre göstergeleri | süre | Bekleyen işlerin sayısını, işleyen sürenin kalanını ve eşiğe kalan tutarı | bekleyen iş sayacı · kalan süre · müşteriye görünen süre · eşiğe kalan tutar | E-04, E-12, E-13, E-16, E-18, E-20, E-25 · E-31, E-36, E-37, E-49, E-50 | §2.6 |
| OB-07 Boş, yükleniyor ve hata hâlleri | boş | İçeriği olmayan, içeriği beklenen ve beklenmeyen bir hatayla açılamayan bölgeyi | boş · yükleniyor · beklenmeyen hata (üç hâl) | Bütün ekranlar | §2.7 |
| OB-08 Satış kapalı hâli | satış-kapalı | Satış kapısının kapalı olduğunu vitrinde ve panelde | vitrin · panel · veri toplayan girişlerin kapısı | E-04, E-10, E-12, E-13, E-20, E-22 · E-31, E-44, E-45, E-46, E-47 | §2.8 |
| OB-09 "Taslak" bandı ve yönetici şeridi | taslak | Giriş yapmış yöneticiye vitrinde panele dönüş yolunu ve önizlenen kaydın taslak olduğunu | yönetici şeridi · "Taslak" bandı · "Taslak" etiketi | E-01…E-28 (şerit) · E-04, E-08 (band) · E-09, E-10 ve duyuru şeridi (etiket) · E-33, E-41 (önizlemeye çıkış) | §2.9 |
| OB-10 Eşzamanlı düzenleme uyarısı | eşzamanlı | Kaydın ya da siparişin, kullanıcı ekrandayken başkasınca değiştiğini | kayıt · sipariş | E-16 · E-33, E-35, E-37, E-41…E-47 | §2.10 |
| OB-11 Ürün kartı | kart | Bir ürünü ya da içerik kaydını listede tek kartla | ürün kartı · indirimli ürün kartı · tükenmiş ürün kartı · içerik kartı | E-01, E-02, E-03, E-07, E-08 | §2.11 |
| OB-12 Form alanı ve form düzeni | — | Etiketli alanı, zorunluluğu, alan mesajını ve kaydetme düğmesini | tek sütun · bölümlü form | Form taşıyan bütün ekranlar | §2.12.1 |
| OB-13 Adres formu | adres | Teslimat ve fatura adresinin alanlarını | teslimat · fatura · defter kaydı · panelde düzeltme | E-13, E-26 · E-37 | §2.12.2 |
| OB-14 IBAN alanı | IBAN | Havale hattında geri ödemenin gideceği IBAN'ı | beyanla giriş · istekle açılan alan · panelde salt okunur | E-16, E-17, E-18 · E-31, E-36, E-37 | §2.12.3 |
| OB-15 Sebep seçimi | sebep | Kapalı listeden seçilen sebebi | firma iptali · durum düzeltmesi | E-37 | §2.12.4 |
| OB-16 Tarih alanı | tarih | Geçmişe dönük girilebilen, ileri tarih kabul etmeyen tarihi ve tarih aralığını | olay tarihi · sıra sorusu · tarih aralığı | E-33, E-35, E-37, E-41, E-51, E-52, E-53 | §2.12.5 |
| OB-17 Kupon alanı | — | Ödeme adımında kupon kodunun girildiği ve uygulanan kodun göründüğü satırı | kapalı · açık · uygulanmış · L-6 ile kapanmış | E-13 | §2.12.6 |
| OB-18 Onay kutuları | — | Sipariş onayının kutularını | iki kutu · dijital kalem kutusu · hizmet kalemi kutusu · sipariş sayfasındaki kayıt | E-13, E-16 | §2.12.7 |
| OB-19 Görsel ve dosya yükleme | — | Kayda yüklenen görselleri ve dijital ürünün dosyasını | galeri · tek görsel · dijital dosya | E-33, E-41, E-43 | §2.12.8 |
| OB-20 Liste ve tablo | — | Kayıtların listesini, aramasını, süzgecini ve sayfalarını | ürün ızgarası · içerik listesi · panel tablosu ve kart listesi | E-02, E-03, E-07, E-27 · E-32, E-35, E-36, E-38, E-40, E-41, E-49, E-51, E-52 | §2.13 |

### 2.1 Tasarım tabanı (OB-01)

Tasarım tabanı bir bileşen değil, bütün bileşenlerin ve ekranların uyduğu dört ilke kümesidir (K-726).

**2.1.1 Üç genişlik sınıfı vardır: dar, orta ve geniş** (K-740). **Dar** sınıf en dar desteklenen genişlikten (P-42) 767 CSS pikseline kadardır — telefon; **orta** sınıf 768–1023 arasıdır — dik tutulan tablet ve küçük pencere; **geniş** sınıf 1024 ve üstüdür — yatay tablet, dizüstü ve masaüstü. Dördüncü bir sınıf yoktur: geniş sınıfta içerik sayfanın ortasında sınırlı bir genişlikte durur ve ekran büyüdükçe yayılmaz. Yatay tutulan telefon orta sınıfın düzenini görür. P-42'nin altında düzenin bozulması kabuldür (`02 §3.34.4`). Sınırlar bu dokümanın tasarım sabitidir; nasıl uygulandıkları Teknik Mimari'nin işidir.

**2.1.2 Vitrin dar sınıftan, panel geniş sınıftan başlayarak tasarlanır; iki tarafta da hiçbir içerik ve işlev bir sınıfta gizlenmez — yalnız yerleşimi değişir** (K-741; `02 §3.34.2`, §10.1.4).

| Sınıf | Vitrin | Panel |
|---|---|---|
| Dar | Tek sütun · menü, arama ve sosyal bağlantılar açılır menüde · ürün listesi iki sütun · süzgeçler açılır bölümde | Menü açılır menüdür · tablo listeler kart listesine döner (§2.13.4) · formlar tek sütun · onay pencereleri tam ekran açılır |
| Orta | Ürün listesi üç sütun | Yan menü daralır · tablolar ikincil sütunlarını satırın altına alır |
| Geniş | Ürün listesi dört sütun · süzgeçler listenin yanında · ürün sayfası ve ödeme adımı iki sütun (içerik ve yan sütun) | Sabit yan menü · tablo listeler |

Zamana duyarlı beş iş — kargoya verme, havale onayı, hizmet tamamlama, iptal, teslim işareti — dar sınıfta sayaçtan en çok üç adımda yapılır: sayaç → liste → sipariş (K-741). %200 yakınlaştırma ayrı bir düzen değildir; bir alt sınıfın düzenini verir (K-740).

**2.1.3 Marka rengi vitrinde yalnız dolu zemin olarak kullanılır; yazı, ince çizgi ve simge rengi olarak kullanılmaz** (K-742). Yerleri: ekranın birincil eylem düğmesi — ekran başına bir tane · duyuru şeridinin zemini · sepetteki kalem sayısı rozeti · etkin menü öğesinin ve etkin sekmenin kalın işareti · site simgesinin zemini (`02 §3.29.4`). Üstündeki yazıyı sistem siyah ya da beyaz seçer (`02 §3.29.5`). **Kullanılmadığı yerler:** bağlantı ve gövde metni, başlıklar, odak göstergesi, form alanlarının kenarı, ikincil düğmeler, anlam renkleri (hata, uyarı, başarı, "Tükendi", indirim), altbilginin yasal bloğu ve panelin tamamı (`02 §3.29.1`). Renk seçilmemişse ürünün varsayılan rengi aynı yerlere uygulanır. E-postadaki yeri Entegrasyon Spesifikasyonu'nun işidir.

**2.1.4 Erişilebilirlik tabanı bileşenlere şöyle yansır.** Taban ve hedef `02 §3.34.5` ile §12.4.1'dedir; uyumluluk beyanı verilmez ve Kontrol Listesi'nin maddeleri Doğrulama Protokolü'nde eşlenir. Bu doküman tabanın test edilen yedi kuralını ve WCAG 2.2'nin iki ölçütünü bileşenlere bağlar; yeni bir erişilebilirlik kuralı koymaz.

| Kural (`02 §3.34.5`) | Bileşendeki karşılığı |
|---|---|
| (1) Yeterli kontrast | Marka rengi yalnız dolu zemindir ve üstündeki yazıyı sistem seçer (2.1.3) |
| (2) Her görselin alternatif metni | OB-19 alternatif metni alır, boşsa sistem üretir; OB-11 kartın görseli aynı metni taşır (`02 §3.11.4`) |
| (3) Klavyeyle tam gezinme | Her bileşenin her eylemi klavyeyle yapılır; onay penceresinde odak "Vazgeç"tedir (2.4.4), hata özetinden sonra odak ilk hatalı alandadır (2.3.2) |
| (4) Görünür odak göstergesi | Odak göstergesi marka renginden bağımsızdır (2.1.3) |
| (5) Etiketli form alanları | OB-12: her alan görünür etiket taşır; alan mesajı etikete bağlıdır (2.12.1.1) |
| (6) Anlam yalnız renkle taşınmaz | OB-03 her mesajda simge ve metin, OB-05 her rozette metin taşır (`02 §6.1.6`) |
| (7) %200 yakınlaştırmada içerik kaybı ve yatay kaydırma olmaz | Yakınlaştırma bir alt sınıfın düzenini verir (2.1.1); OB-20 tabloyu yatay kaydırmaz, karta çevirir (2.13.4) |
| Tutarlı Yardım (WCAG 2.2 — 3.2.6) | İletişim bilgisi ve "İletişim" bağlantısı her vitrin sayfasında aynı yerdedir: altbilginin kimlik bloğu ve menünün son öğesi (2.2.5, 2.2.3) |
| Tekrarlı Giriş (WCAG 2.2 — 3.3.7) | Fatura adresi teslimat adresiyle aynı gelir (2.12.2.2); girişten dönen müşterinin ödeme adımında yazdıkları durur (§3.6.2); üyenin formları ön dolu gelir |

*Kullanıldığı ekranlar:* bütün ekranlar; matriste E-43 (marka renginin örneği). *Kaynak: `02 §3` girişi, §3.29.1, §3.29.4, §3.29.5, §3.34.1, §3.34.2, §3.34.4, §3.34.5, §6.1.6, §10.1.4, §12.4.1, §11 (envantere girmeyenler) · `03 §0.1.4`, §3.1.6, §3.5.4.9 · `10 §2` KP-69, KP-71 · K-726, K-740, K-741, K-742 · devir: K-262, K-391, K-463, K-625.*

### 2.2 Çerçeve (OB-02)

**Ne gösterir.** Ekranın içeriğini saran ve ekrandan ekrana değişmeyen bölgeleri. Üç varyantı vardır: vitrin çerçevesi, ödeme adımının sadeleşmiş vitrin çerçevesi ve panel çerçevesi. Bölgelerin götürdüğü yerler §3.1 ve §3.2'dedir.

**2.2.1 Vitrinin her sayfası yukarıdan aşağıya beş bölge taşır: duyuru şeridi (yayındaysa) · üst bölüm · menü · içerik · altbilgi** (K-746). Giriş yapmış yöneticinin tarayıcısında duyuru şeridinin üstüne yönetici şeridi eklenir (§2.9).

**2.2.2 Duyuru şeridi** tek satırlık metin ve isteğe bağlı bağlantıdır; yalnız duyuru Yayında'yken ve — tarihleri girilmişse — aralığındayken görünür (`02 §3.27.21`; Z-23). Duyuru yoksa bölge de yoktur.

**2.2.3 Üst bölüm ve menü.** Üst bölümde solda logo — yoksa marka adı, o da girilmemişse alan adı (`02 §3.29.2`, §3.29.3) —, ortada ürün araması, sağda hesap alanı ve kalem sayısını gösteren sepet durur. Hesap alanı ziyaretçiye "Giriş yap"ı ve "Sipariş takibi"ni, üyeye adını ve altında "Hesabım", "Siparişlerim" ve "Çıkış"ı gösterir. Sosyal medya ve WhatsApp bağlantıları üst bölümün ince bir satırında ve altbilgidedir; ekranda yüzen bir WhatsApp düğmesi ve sohbet penceresi yoktur (`02 §3.27.12`, §3.32.9). **Menünün sırası ana sayfa düzenini izler:** mağaza öncelikli düzende "Ürünler" önce, içerik tipleri — Hizmetlerimiz, Referanslarımız, Hakkımızda, SSS — sonra; tanıtım öncelikli düzende içerik tipleri önce — Hakkımızda başta —, "Ürünler" sonra; "menüde göster" işaretli genel sayfalar ardından gelir, İletişim her zaman sondadır. Kategoriler tek bir "Ürünler" öğesinin altında açılır: birinci seviye kategoriler ve altlarında ikinci ve üçüncü seviye — her seviyede adlarının alfabetik sırasıyla (K-805). Menüye sığmayan öğeler "Diğer" altında toplanır. Görünmeyen öğelerin kuralı `02 §3.28.4` ve §3.28.6'dadır; adlar firmanın değiştirdiği adlardır (K-746).

**2.2.4 Dar sınıfta** menü, arama ve sosyal bağlantılar açılır menüye girer; logo, sepet ve menü düğmesi üst bölümde kalır. Açılır menüde üç seviyeli kategori ağacı katlanarak açılır. Çok uzun marka adı tek satırda kesilir; tam adı sekme başlığında ve altbilgide okunur (K-746).

**2.2.5 Altbilgi firmanın yasal kimlik ve iletişim bilgilerinin tam setini "İletişim" başlığı altında taşır** (K-747). Blok firma tipinin zorunlu setini eksiltmeden ve doluysa isteğe bağlı alanlarıyla gösterir — setin içeriği `02 §3.1.3`'tedir —; bir bağlantının arkasına saklanmaz ve dar sınıfta da katlanmaz. **ETBİS doğrulama bandı** bloğun hemen yanında — dar sınıfta hemen altında — durur ve doğrulama sayfasına götürür; alan boşsa band da yeri de yoktur (`02 §3.1.4`). Altbilginin öteki bölümleri: genel sayfalar (`02 §3.28.5`) · yasal bağlantılar — aydınlatma metni, çerez politikası, "İşlem rehberi" (`02 §3.24.8`, §3.33.5) · sosyal medya ve WhatsApp bağlantıları · en altta, firma kapatmadıysa, platform imzası (`02 §3.29.6`). Çerez onay bandı yoktur (`02 §3.33.5`). Uzun unvan ve adres satır kırar, kesilmez.

**2.2.6 Altbilgi hiçbir vitrin ekranında kaldırılmaz ve kısaltılmaz** — sepet, ödeme adımı, sipariş teyit ekranı, giriş ve kayıt, sipariş sayfası ve "sayfa bulunamadı" dahil (K-747).

**2.2.7 Sadeleşmiş vitrin çerçevesi yalnız ödeme adımındadır (E-13):** üst bölüm logo ya da marka adı ile "Sepete dön"ü taşır; menü, arama ve duyuru şeridi yoktur; altbilgi eksiksiz kalır (K-757).

**2.2.8 Panel çerçevesi üst satır ve menüden oluşur** (K-749). Üst satır her panel ekranında üç şey taşır: satış durumu göstergesi ("Satış açık" ya da "Satış kapalı") · "Siteyi görüntüle" · yöneticinin adı ve altında "Hesabım" ile "Çıkış". Menü sekiz girişlidir; girişleri ve götürdükleri ekranlar §3.2'dedir. Geniş sınıfta menü solda sabittir ve grupları açıktır; orta sınıfta daralır; dar sınıfta açılır menüdür, gruplar katlanır ve etkin grup açık gelir. Menüde sayaç yinelenmez: Siparişler ve Talepler girişleri bekleyen iş varken yalnız bir işaret taşır; sayılar ana sayfadadır (2.6.1). Uzun yönetici adı üst satırda kesilir. Panelde kimlik bloğu, ETBİS bandı ve marka rengi yoktur (2.1.3; K-747).

**2.2.9 Çerçevesi olmayan panel ekranları.** Panel girişi ve şifre sıfırlama (E-29) ile yönetici daveti kabul ekranı (E-30) oturumdan önce açılır; panel menüsünü ve üst satırını taşımaz, yalnız ürünün panel görünümünde bir başlık taşır. E-54'ün bağlantıyla açılan iki adımı da böyledir (§3.5).

*Kullanıldığı ekranlar:* vitrin çerçevesi E-01…E-12 ve E-14…E-28; sadeleşmiş E-13; panel çerçevesi E-31…E-47 ve E-49…E-53, E-54'ün yeniden doğrulama adımı. Matriste: E-01, E-02, E-03, E-04, E-07…E-13, E-16, E-22, E-31, E-34, E-36, E-37, E-41…E-44. *Kaynak: `02 §3.1.3`, §3.1.4, §3.24.8, §3.27.12, §3.27.21, §3.27.22, §3.28.4–§3.28.6, §3.29.2, §3.29.3, §3.29.6, §3.32.9, §3.33.5 · `03 §2.1.1`, §2.1.2, §2.2.2, §2.2.6, §4.1.26 · `10 §2` KP-1, KP-2, KP-35, KP-36 · park satırı K-14 · K-574 · K-746, K-747, K-749, K-757, K-805 · devir: K-248, K-249, K-541, K-574, K-626, K-627.*

### 2.3 Mesaj (OB-03)

**Ne gösterir.** Hatayı, uyarıyı, bilgiyi ve başarıyı. Her mesaj simge ile metin taşır; anlam yalnız renkle taşınmaz (`02 §6.1.6`) ve anlam renkleri marka renginden bağımsızdır (2.1.3).

**2.3.1 Mesaj dört yerde görünür** (K-743):

| # | Yer | Nerede durur | Ne için |
|---|---|---|---|
| 2.3.1.1 | **Alan mesajı** | Hatalı alanın hemen altında, alanın etiketine bağlı | Form alanının doğrulama hatası |
| 2.3.1.2 | **Yerinde kalıcı mesaj** | İlgili satırın ya da bölümün içinde, koşul sürdükçe | Sepet satırındaki "Tükendi" ve fiyat değişimi, ödeme adımındaki özet farkı, teslimat yapılmayan il, panelde engellenen işlemin sebebi, nötr limit mesajı, işlem düzeyindeki beklenmeyen hata |
| 2.3.1.3 | **Bir kez söylenen mesaj** | İçerik alanının başında, kapatılabilen bir kutu; görüldükten sonra yinelenmez ve sayfa yenilenince geri gelmez | Sepet birleşmesi (`02 §3.16.4`), sepetten çıkan kalem (`03 §2.3.5`) |
| 2.3.1.4 | **Kısa süreli bildirim** | Ekranın kenarında kendiliğinden kaybolan satır | Yalnız sonucu ekranda zaten görünen küçük başarılar: sepete eklendi, kaydedildi |

**2.3.2 Form gönderilince** hatalı alanlar birlikte işaretlenir, formun başında hata sayısını söyleyen bir özet çıkar ve odak ilk hatalı alana gider; yazılanlar silinmez. Her alan kendi mesajını taşır (K-743).

**2.3.3 Panelin engel mesajı** işlemin başlatıldığı yerde, yerinde kalıcı mesaj olarak çıkar; sebebi, varsa engelleyen kayıtların sayısını ve çıkış yolunu söyler ("önce taslağa alın") — pencere açmaz (K-743). Yayın kapısını bozan işlem (`03 §1.8.2`), kural dışı değerin reddi ve yapılamayan durum düzeltmesi bu biçimdedir.

**2.3.4 Hata, uyarı ve geri alınamaz bir işlemin sonucu kısa süreli bildirimle verilmez;** sipariş, beyan ve talep gibi sonuçlar kendi teyit ekranında ya da sayfada kalıcı durur (K-743).

**2.3.5 Nötr limit mesajı** engellenen işlemin düğmesinin yanında, yerinde kalıcı mesaj olarak çıkar; metni ve söylemedikleri `02 §6.1.5`'tedir. Hangi limitin hangi formda işlediği §7.3'te yazılır (5. oturum).

**2.3.6 Beklenmeyen hatanın mesajı** üç hâliyle §2.7.3'tedir.

*Varyantlar:* dört yer × dört anlam (hata, uyarı, bilgi, başarı); kısa süreli bildirim yalnız başarıdır. *Kullanıldığı ekranlar:* form ya da işlem taşıyan bütün ekranlar. Matriste: E-04, E-10, E-12, E-13, E-15, E-20, E-22…E-25, E-29, E-30, E-32…E-37, E-41, E-45, E-46, E-49. Ekrana özgü mesajın metni ve koşulu ekranın tanımındadır. *Kaynak: `02 §3.16.4`, §6 girişi, §6.1.5, §6.1.6, §6.6 · `03 §2.3.5`, §2.3.6, §3.1.5, §3.1.6, §1.8.2 · `10 §2` KP-72, KP-76 · K-743 · devir: K-125, K-329, K-395, K-538, K-554, K-566, K-584, K-600, K-602, K-653, K-654, K-659, K-674, K-703 (kalıp burada; her birinin metni ekranının tanımında — §5, §9).*

### 2.4 Onay (OB-04)

**Ne gösterir.** Geri alınamaz ya da sonucu sayıyla bildirilen bir işlemin son adımını. Onayın tek anatomisi vardır; yönetici onu pencerede, müşteri işlemin kendi ekranının son adımında görür (K-744).

**2.4.1 Anatomi:** işlemin adı · sonucu söyleyen tek cümle — sayı taşıyan onayda sayıyla · geri alınamaz işlemde "Bu işlem geri alınamaz" · iki düğme: işlemin adını taşıyan onay düğmesi ("Kargoya ver", "Siparişi iptal et" — "Evet" ya da "Tamam" değil) ve "Vazgeç" (K-744).

**2.4.2 Onay isteyen işlemler kaynakta sayılıdır; bu doküman listeye ekleme yapmaz.**

| # | Varyant | İşlemler | Liste nerede |
|---|---|---|---|
| 2.4.2.1 | Yöneticinin penceresi — geri alınamaz | Siparişe dokunan sekiz işlem: havale "ödendi" işareti · kargoya verme ve yeniden gönderim · firma iptali · geri ödemenin işlenmesi · kart hattında karta para gönderen kalem çıkarma · iade reddi · başka kanaldan gelen cayma ya da gecikme feshi bildiriminin kaydı · üye hesabının talep üzerine silinmesi | `02 §5.9` · `03 §1.6.1.1`–§1.6.1.8 |
| 2.4.2.2 | Yöneticinin penceresi — geri alınamaz, siparişe dokunmayan | Ürünün ya da varyantın kalıcı silinmesi · kurumsal içerik kaydının silinmesi · yöneticinin kaldırılması | `02 §3.7.7`, §3.27.27, §10.2.2 · `03 §8.1.9`, §8.6.1.7, §8.8.5 |
| 2.4.2.3 | Yöneticinin penceresi — "geri alınamaz" cümlesi olmadan | Geri alınabilen ama sonucu sayıyla bildirilen dört ayar değişikliği: havale IBAN'ının değişmesi · havalenin kapatılması · iade adresinin değişmesi · satışın geçici kapatılması | `03 §8.7.3.2`, §8.7.3.3, §8.7.4.5, §8.7.5.1 · K-744, K-752 |
| 2.4.2.4 | Müşterinin son adımı | Dört işlem: siparişin ya da kalemin iptali · gecikme nedeniyle fesih · cayma beyanı · hesabın silinmesi | `02 §5.9` · `03 §1.6.1` · K-732 |

**2.4.3 Sayı ve uyarı taşıyan onaylar** sonucu sayıyla söyler; sayının ne olduğu kaynaktadır: kalıcı silmede açık sipariş sayısı (`03 §8.1.9`) · yönetici kaldırmada geçersizleşecek davet sayısı (`03 §8.8.5`) · IBAN değişikliğinde ve havalenin kapatılmasında ödemesi beklenen havale siparişi sayısı (`03 §8.7.3.2`, §8.7.3.3) · iade adresi değişikliğinde "iade malı bekleniyor" kalem sayısı (`03 §8.7.4.5`) · satışın geçici kapatılmasında açık siparişlerin etkilenmeyeceği (K-752). Onaydan önce gösterilen uyarılar: "stokta bulunamadı" sebebinde yasal uyarı (`02 §7.2.4`) · dijital kalemli siparişte "ödendi" işaretinin düzeltilemeyeceği (`03 §1.6.1.1`) · pencere dışı ya da istisnalı kalemde cayma kaydının ayrıca onayı (`03 §1.11.34`). Uyarıların metni K-798'de, onay cümleleri K-797'dedir; ekrandaki yerleri 9.9.15, 9.9.19 ve 9.9.20'dedir. Dört ayar onayının ve yönetici kaldırmanın cümleleri K-814'tedir (9.16.7, 9.17.7, 9.17.8, 9.18.10, 9.20.10).

**2.4.4 Yöneticinin penceresi.** Açıldığında odak "Vazgeç"tedir; pencerenin dışına dokunmak ve pencereyi kapatmak vazgeçmektir; yazarak teyit ve ikinci pencere yoktur. Sebep, tarih ve tutar gibi girdiler pencerede değil işlemin formunda alınır (OB-15, OB-16); pencere formun son adımıdır. Dar sınıfta pencere tam ekran açılır (2.1.2). Pencere açıkken sipariş başkasınca değiştiyse işlem uygulanmaz ve eşzamanlı düzenleme uyarısı çıkar (§2.10) (K-744).

**2.4.5 Müşterinin son adımı.** Aynı içerik sayfanın içindedir, pencere değildir: neyin iptal edildiği ya da hangi kalemden cayıldığı, sonuç cümlesi, onay düğmesi ve sipariş sayfasına dönen "Vazgeç". Vazgeçirme metni, bekleme, ek soru ve ikinci kanal yoktur; onay iptal, fesih ve cayma hakkının kullanımını zorlaştırmaz (K-732, K-744).

**2.4.6 Onay verildikten sonra** düğme işlem bitene kadar ikinci kez basılamaz (2.7.2). Sonucu belirsiz kalan onayın hâli §2.7.3.3'tedir. Sistemin iptalde kendiliğinden başlattığı kart iadesi onay istemez (`02 §5.9`); "e-posta ulaşmadı" işaretli e-postanın yeniden gönderilmesi de istemez (K-759). Panel "geri al" düğmesi sunmaz (`02 §7.6.1`).

*Kullanıldığı ekranlar:* müşterinin son adımı E-17, E-18, E-25 (E-16 işlemleri bu ekranlara açar); yöneticinin penceresi E-33, E-37, E-40, E-41, E-44, E-45, E-46, E-49 — ürün listesinde (E-32) satır işlemi yoktur, kalıcı silme ürün formundadır (K-802). Matriste ayrıca E-24 ve E-27 (hesap silmenin ve müşteri iptalinin komşu satırları). *Kaynak: `02 §3.7.7`, §3.27.27, §5.9, §7.2.4, §7.6.1, §7.6.4, §10.2.2 · `03 §1.6.1`, §1.11.15, §1.11.16, §1.11.19, §1.11.21, §1.11.22, §1.11.29, §1.11.31, §1.11.34, §1.11.41, §8.1.9, §8.6.1.7, §8.7.3.2, §8.7.3.3, §8.7.4.5, §8.7.5.1, §8.8.5 · `10 §2` KP-47 · K-732, K-744, K-752, K-759, K-797, K-798, K-802, K-814 · devir: K-225, K-537, K-606, K-609, K-655, K-656, K-662, K-663, K-664, K-665, K-677, K-709, K-715 (kalıp burada; onay cümleleri ekranının tanımında).*

### 2.5 Rozet ve işaretler (OB-05)

**Ne gösterir.** Bir kaydın durumunu, kalemin kaydını ve koşulu süren işareti kısa, metinli bir etiketle. Rozet her zaman metin taşır; renk yalnız yardımcıdır (`02 §6.1.6`). Adlar konvansiyon 8'e uyar.

**2.5.1 Durum rozetleri** — adlar kaynakla birebirdir:

| # | Rozet ailesi | Değerler | Nerede görünür | Kaynak |
|---|---|---|---|---|
| 2.5.1.1 | Sipariş durumu (sevkiyat ekseni) | Alındı · Hazırlanıyor · Kargoya verildi · Teslim edildi · Teslim edilemedi · İptal edildi | E-16, E-27 · E-36, E-37 | `02 §5.4` |
| 2.5.1.2 | Ödeme durumu (ödeme ekseni) | Bekliyor · Ödendi · Başarısız · Kısmen geri ödendi · Geri ödendi | E-16, E-27 · E-36, E-37 | `02 §5.5` |
| 2.5.1.3 | Ürün ve varyantın yayın durumu | Taslak · Yayında · Arşiv | E-32, E-33 | `02 §5.1` |
| 2.5.1.4 | Kurumsal içeriğin yayın durumu | Taslak · Yayında | E-41 | `02 §5.2` |
| 2.5.1.5 | Ayıp talebinin durumu | Açık · Çözüldü | E-16, E-19 · E-37, E-38 | `02 §5.10` |
| 2.5.1.6 | İletişim talebinin durumu | Açık · Kapatıldı | E-38, E-39 | `02 §5.13` |

Siparişin iki ekseni her yerde **iki ayrı rozetle**, yan yana gösterilir; tek rozete indirilmez (`02 §5.3.1`; K-777). Hesabın ve davetin durum makinesi yoktur (`02 §5.12.1`; `03 §1.10.7`): doğrulanmamış kayıt ve davetin geçerliliği rozet değil, ekranın kendi satır metnidir.

**2.5.2 Kalem kayıtları** sipariş kaleminin satırında kaydın adıyla görünür — iptal kaydı, çıkarma kaydı, gecikme feshi, cayma beyanı, iade teslim alma, iade reddi, "mal dönmedi" kapanışı, IBAN isteği, ayıp talebi, teslim işareti; liste ve her kaydın doğduğu an `02 §5.8` ile `03 §1.7.1`'dedir. Kalem ayrı bir durum rozeti taşımaz.

**2.5.3 Panel işaretleri** bir durum değildir: doğuran koşul sürdükçe görünür ve koşul kalkınca kalkar. Altı işaretin düştüğü ve kalktığı an `03 §1.7.3`'tedir; işaret metinlidir.

| # | İşaret | Sipariş listesinde (E-36) | Sipariş ayrıntısında (E-37) | Başka yerde |
|---|---|---|---|---|
| 2.5.3.1 | "e-posta ulaşmadı" | Satırda | Altında ulaşmayan e-postalar tek tek — bildirimin adı, ilk gönderim tarihi, gittiği adres — ve her satırda "Yeniden gönder"; birden çok satır varsa "Hepsini yeniden gönder" | Ana sayfanın uyarılarında sayısını söyleyen satır (E-31) · ayıp talebinin bildiriminde talebin satırında (E-38) · davet satırında, yeniden gönder düğmesi olmadan (E-49) |
| 2.5.3.2 | "ödeme sonucu alınamadı" | Satırda | Siparişin işaretleri arasında | — |
| 2.5.3.3 | "geri ödeme gerçekleşmedi" | Satırda | Siparişin işaretleri arasında; yeniden deneme ve havale yolunu açma işlemleri yanında (`03 §1.11.32`) | Ana sayfanın uyarılarında (E-31) · "iade ve geri ödeme bekleyen" sayacında |
| 2.5.3.4 | "IBAN bekleniyor" | — | Kalemde | Ana sayfanın "Müşteriden beklenenler" bölümünde liste (E-31; 2.6.2) |
| 2.5.3.5 | "iade malı bekleniyor" | — | Kalemde | — |
| 2.5.3.6 | "e-posta düzeltildi" | Satırda | Siparişin işaretleri arasında; adresler e-posta geçmişinde (`03 §8.4.9`) | — |

**2.5.4 "E-posta ulaşmadı" işaretinin davranışı** (K-759, K-760, K-761). Yeniden gönderilebilenlerin kümesi işareti düşürenlerin kümesidir (`02 §9.1.6`). Başarılı gönderimde satır listeden düşer, son satır düşünce işaret kalkar; gönderim yine başarısızsa satır kalır ve yerinde kalıcı mesaj bunu söyler. Gönderim sürerken düğme ikinci kez basılamaz. Sipariş ayrıntısı, adres yanlışsa yolun e-posta düzeltmesi olduğunu hatırlatır (`02 §10.4.11`). Firmaya giden bildirim (F-1…F-4) satıra işaret düşürmez ve yeniden gönderilmez; ana sayfada "firma bildirimleri ulaşmıyor" uyarısı çıkar (2.5.5). İletişim talebinin satırı işaret taşımaz. Siparişler arası toplu yeniden gönderim yoktur. İşaret "e-posta düzeltildi" ile birlikte durduğunda ikisi ayrı ayrı okunur.

**2.5.5 Panel ana sayfasının uyarıları** rozet değil, çözüleceği ekrana götüren satırlardır (E-31'in "Uyarılar" bölümü — K-750): satışın kapalı olduğu ve sebebi (§2.8) · kanal uyarısı (`02 §6.1.1`) · "firma bildirimleri ulaşmıyor" uyarısı — bildirimlerin gittiği adresi gösterir (K-760) · "e-posta ulaşmadı" işaretli siparişlerin sayısını söyleyen satır (K-761) · kart iadesinin gerçekleşmediği uyarısı (`03 §8.3.3.4`). Uyarının götürdüğü yer §3.4'tedir; koşul kalkınca uyarı kalkar.

**2.5.6 Vitrin işaretleri:** "Tükendi" — üründe, varyantta ve sepet satırında (`02 §3.6.3`, §3.6.4) · indirim işareti — referans fiyat ve indirimin tarihleriyle (`02 §3.9.2`; §2.11) · sepet satırındaki fiyat değişimi mesajı bir işaret değil, yerinde kalıcı mesajdır (2.3.1.2). Vitrinde yayın durumu rozeti gösterilmez; taslak kaydı yalnız yönetici, "Taslak" bandı ya da etiketiyle görür (§2.9).

*Kullanıldığı ekranlar:* E-01, E-02, E-03 (ürün kartının vitrin işaretleri), E-04, E-05, E-08, E-12, E-14, E-16, E-19, E-27 · E-31, E-32, E-33, E-36, E-37, E-38, E-39, E-40, E-41, E-49 (matristen; E-27 kararla; E-01…E-03 2a, E-19 2b, E-40 4. oturumun tanımlarıyla — bağlı siparişlerin rozetleri, 9.12.6). *Kaynak: `02 §3.6.3`, §3.6.4, §3.9.2, §5.1–§5.5, §5.8, §5.10, §5.12.1, §5.13, §6.1.1, §6.1.2, §6.1.6, §9.1.6, §10.4.11 · `03 §1.7.1`, §1.7.3, §1.10.7, §8.3.3.4, §8.9.2, §10.1.2.2 · K-750, K-759, K-760, K-761, K-777 · devir: K-585, K-611, K-612, K-675; `03 §7.1.37`, §8.9.2, §10.1.2.2'nin karar satırı olmayan devri.*

### 2.6 Sayaç ve süre göstergeleri (OB-06)

**Ne gösterir.** Bekleyen işlerin sayısını, işleyen bir sürenin kalanını ve bir eşiğe kalan tutarı. Değerler yazılmaz; süre, limit ve tutarların evi `02 §4`, §11'dir (konvansiyon 10).

**2.6.1 Bekleyen iş sayacı.** Panel ana sayfasının "Bekleyen işler" bölümü altı sayaç taşır; sayaçların adları, sırası ve neyi saydıkları `02 §10.6.1`'dedir. Sıfır olan sayaç da görünür ve sıfır yazar; her sayaç kendi süzülmüş listesine götürür (§3.4.2); dar sınıfta sayaçlar ikişerli dizilir (K-750). Yedinci sayaç yoktur. Menüde sayı yinelenmez (2.2.8).

**2.6.2 "IBAN bekleniyor" listesi sayaç değildir** (K-750): ana sayfanın "Müşteriden beklenenler" bölümünde kalemler satır satır, süresi olan hatta kalan süreyle ve kalan süresi en az olan önce listelenir; her satır siparişine götürür. Uzun liste ilk on satırını gösterir; tamamına sipariş listesinden gidilir.

**2.6.3 Panelde kalan süre** siparişte ve kalemde şu sürelerde gösterilir: kargoya verme sözü ve kalanı (`03 §8.2.4`; Z-10) · iptalin ve caymanın geri ödeme süresinin kalanı (`03 §8.3.3.2`; Z-16, Z-17) · teslimden sonraki caymada, mal ulaşana kadar geri ödeme süresi işlemez ve gösterge beyandan geçen günü gönderme süresiyle (Z-42) birlikte gösterir, mal ulaşınca geri ödemenin kalan süresine döner (`03 §2.8.1.8`, §3.3.8). Süre aşıldığında gösterge aşımı ve kanuni faiz uyarısını yerinde kalıcı mesajla söyler (`03 §8.2.4`; `02 §7.2.3`). Süre sayacı olmayan hâller kaynaktadır (`02 §10.6.1`).

**2.6.4 Müşteriye görünen süreler:** ürün sayfasında fiziksel üründe kargoya verme süresi, hizmette ifa süresi (`03 §2.1.5`) · havale siparişinde son ödeme günü — sipariş sayfasında (`03 §2.6.4`; Z-8) · cayma beyanı ekranında malı gönderme süresi (`03 §2.8.1.2`; Z-42) · kaydın ve bekleyen e-posta değişikliğinin doğrulama bağlantısının geçerli olduğu son gün — kayıt ekranının gönderim sonrası hâlinde ve hesap ekranında (`03 §9.1.3`, §9.3.4; K-659) · e-posta değişikliğini geri alma bağlantısının bitiş tarihi — hesap ekranında, değiştirme ve silme kapalıyken (`02 §3.15.6`; Z-45). Bir işlemin süresi dolduğunda sipariş sayfası o işlemi sunmaz (`03 §1.11`); süresi dolmak üzere olan bir işlem için ekranda geri sayan bir uyarı yoktur (2.6.6).

**2.6.5 Eşiğe kalan tutar** sepette ve ödeme adımının özetinde, tutarın dökümünün içinde yerinde kalıcı mesajdır: ücretsiz kargoya kalan tutar ya da "Kargo ücretsiz" (`02 §3.19.5`) · asgari sipariş tutarının eksik kalan tutarı (`03 §2.3.2`). Koşulu olmayan sepette mesaj da yoktur.

**2.6.6 Süre gün olarak ve son günün tarihiyle birlikte gösterilir** (K-767): kalan gün sayısı ve sürenin dolduğu tarih; saat, dakika ve saniye sayan canlı bir geri sayım yoktur. İş günüyle sayılan süre birimini "iş günü" diye yazar; sayım kuralı `02 §4.1`'dedir. Dolmuş sürede gösterge kaç gün geçtiğini yazar ve aşımı metinle söyler — yalnız renkle değil.

*Varyantlar:* bekleyen iş sayacı · kalan süre (panel) · müşteriye görünen süre · eşiğe kalan tutar. *Kullanıldığı ekranlar:* E-04, E-12, E-13, E-14 (havalede son ödeme günü — K-776), E-16, E-18, E-20, E-25 (bağlantıların son günü — 2b) · E-31, E-36, E-37, E-49 (davetin son günü — 9.20.4), E-50 (bekleyen e-posta değişikliğinin son günü — 9.21.4). Matriste: E-16, E-31, E-36…E-39. *Kaynak: `02 §3.15.6`, §3.19.5, §4.1, §7.2.3, §10.6.1 · `03 §1.11`, §2.1.5, §2.3.2, §2.6.4, §2.8.1.2, §2.8.1.8, §9.1.3, §9.3.4, §3.3.8, §4.1.8, §4.1.14, §4.1.17, §4.1.19, §7.2.15, §8.2.4, §8.3.3.2, §8.9.1 · K-750, K-767 · devir: K-138, K-491, K-659.

### 2.7 Boş, yükleniyor ve hata hâlleri (OB-07)

**Ne gösterir.** Bir ekranın ya da bölgenin içeriği olmadığını, içeriğin beklendiğini ya da beklenmeyen bir hatayla açılamadığını. Her ekran tanımı bu üç hâli şablonun yedinci alanında, bu kalıba atıfla yazar.

**2.7.1 Boş hâl** (K-765). Boş bir liste ya da bölge iki biçimden biriyle davranır ve hangisinin işlediği ekranın tanımında yazılır:
- **2.7.1.1 Görünmez.** Kaynağın "görünmez" dediği bölge yer tutmaz, boş kutu göstermez: kaydı olmayan içerik tipinin menü öğesi ve ana sayfa bloğu (`02 §3.28.3`, §3.28.4) · yayında ürünü olmayan kategorinin menü öğesi ve ana sayfanın ürün vitrini (`02 §3.28.6`, §3.28.1) · duyurusu olmayan sitenin duyuru şeridi · panel ana sayfasının boş bölümü (K-750). Hakkımızda boşken kurumsal blok geri düşüşünü gösterir (`02 §3.27.14`).
- **2.7.1.2 Boş hâl satırı.** Kullanıcının açtığı ama içeriği olmayan liste, içeriğin yerinde tek cümleyle neyin olmadığını söyler ve bir çıkış yolu taşır: vitrinde ana sayfaya ya da ürünlere dönüş, panelde — listenin kaydı panelden açılıyorsa — ilk kaydı açan düğme. Boş kategori sayfası (`02 §3.28.6`), sonucu olmayan arama ve süzgeç, boş sepet (`03 §2.3.7`), siparişi olmayan üyenin sipariş geçmişi, adresi olmayan adres defteri ve panelin boş listeleri bu biçimdedir. Boş hâl bir hata değildir ve hata simgesi taşımaz.
- **2.7.1.3** Site boş kurulur; örnek kayıt yoktur (`02 §10.8.1`). Sayaç boş kalmaz, sıfır yazar (2.6.1); paydası sıfır olan oran "değerlendirilemez" yazar (`02 §10.6.3`).

**2.7.2 Yükleniyor hâli** (K-765). Çerçeve yerinde kalır; içeriği beklenen bölge kendi yerinde, yerini koruyan ve metin taşıyan bir bekleme göstergesi gösterir — ekranın tamamı örtülmez. Bir işlem sürerken işlemi başlatan düğme ikinci kez basılamaz ve sürdüğünü metinle söyler (K-744). Dosya yüklemesi ilerlemesini gösterir (2.12.8.4). Ödeme sonucu beklenen sipariş bir yükleniyor hâli değildir: sipariş sayfası ödemenin beklendiğini kalıcı olarak söyler ve sonuç gelince güncellenir (`03 §2.5.1.2`).

**2.7.3 Beklenmeyen hata** üç hâlde gösterilir (K-745); ekran işlemin tamamlanmadığını, kullanıcının yazdıklarının ve sepetinin durduğunu ve ne yapacağını söyler, hata kodu ve teknik ayrıntı göstermez:
- **2.7.3.1 İşlem düzeyi.** Bir form ya da düğme işlemi tamamlanamazsa mesaj işlemin yerinde, yerinde kalıcı mesaj olarak çıkar (2.3.1.2); form içeriği korunur ve kullanıcı aynı düğmeyle yeniden dener.
- **2.7.3.2 Sayfa düzeyi.** Sayfa açılamazsa vitrinde vitrinin, panelde panelin çerçevesi içinde bir hata sayfası çıkar: kısa açıklama, "Yeniden dene" ve ana sayfaya dönüş. "Sayfa bulunamadı" sayfasından (E-06) ayrıdır.
- **2.7.3.3 Sonucu belirsiz işlem.** Sipariş onayı, müşterinin geri alınamaz dört işlemi ve yöneticinin onay isteyen işlemleri hata verirse mesaj "işlem yapılmadı" demez; kullanıcıyı sonucu görmeye götürür — müşteriyi sipariş sayfasına ya da sipariş takibine, yöneticiyi siparişin güncel hâline.
- **2.7.3.4** Mesaj müşteriye İletişim sayfasını, yöneticiye kurulumu yapanı işaret eder.

**2.7.4 Adı konmuş hatalar bu kalıbın dışındadır:** `02 §6`'nın senaryoları ve `03 §3`'ün satırları kendi mesajlarıyla, OB-03'ün yerlerinde gösterilir. Ödeme yönteminin "şu an kullanılamıyor" hâli böyledir: yöntem listede kalır, seçilemez ve sebebini yanında yazar (`03 §2.4.5`; K-757).

**2.7.5 Kesinti ve bakım için ekran yoktur:** sitenin hiç çalışmadığı kesintide ürünün içinden bilgilendirme yoktur (`03 §10.3.1`) ve bakım modu yoktur (`02 §3.1.6`).

*Kullanıldığı ekranlar:* bütün ekranlar; matriste E-31. Hata sayfası ayrı bir ekran kimliği almaz — her ekranın sayfa düzeyi hata hâlidir (§4.4). *Kaynak: `02 §3.1.6`, §3.27.14, §3.27.22, §3.28.1, §3.28.3, §3.28.4, §3.28.6, §6.1.4, §10.6.3, §10.8.1 · `03 §2.3.7`, §2.4.5, §2.5.1.2, §10.3.1 · K-744, K-745, K-750, K-757, K-765 · §1.3 GAP-10.*

### 2.8 Satış kapalı hâli (OB-08)

**Ne gösterir.** Satış kapısının kapalı olduğunu. Kapının dört koşulu ve geçici kapatma anahtarı `02 §3.1.5` ile §3.1.6'dadır; bileşen yalnız hâlin ekrandaki görünümüdür.

**2.8.1 Vitrinde** ürünler ve kurumsal içerik görünür kalır; sepete ekleme ve ödeme adımına geçiş kapalıdır ve ziyaretçi satışın kapalı olduğunu görür: ürün sayfasında (E-04) sepete ekleme düğmesinin yerinde, sepette (E-12) ödeme adımına geçiş düğmesinin yerinde yerinde kalıcı mesaj durur (2.3.1.2). Var olan sepet korunur ve içeriği görünür (`02 §3.1.6`, §6.1.4). Vitrin yalnız satışın kapalı olduğunu söyler; karşılanmayan koşulu adıyla gösteren paneldir (`02 §3.1.5`). Ana sayfanın ürün vitrini satış kapalıyken de görünür (K-754). Satış kapalıyken ödeme adımına geçilemez (`03 §2.3.8`, §3.2.1.17).

**2.8.2 Panelde** hâl dört yerde görünür: panel çerçevesinin üst satırındaki satış durumu göstergesi (2.2.8) · ana sayfanın "Uyarılar" bölümünde satışın kapalı olduğu ve sebebi — geçici kapatma ya da karşılanmayan koşul, adıyla (K-750) · ana sayfanın kurulum kontrol listesi — eksik maddeler adıyla, her madde kendi ayar ekranına götürür ve hepsi tamamlanınca liste kaybolur (`02 §10.8.2`) · E-44'ün başındaki "Satış durumu" bölümü — kapının dört koşulu tek tek, karşılanmayanı düzeltileceği ekranın bağlantısıyla ve geçici kapatma anahtarı (K-752). Anahtar kimlik formunun kaydından bağımsızdır ve kendi onayıyla işler (2.4.2.3). Satış bir koşul eksik olduğu için kapalıyken anahtar görünür kalır ve tek başına satışı açmaz.

**2.8.3 Veri toplayan girişlerin kapısı** ayrı bir hâldir (`02 §3.1.5`, §3.13.19, §3.32.8): aydınlatma metni tamamlanıp yayına alınmadıysa iletişim formu (E-10), kayıt ekranı (E-20) ve yeni hesap açacak Google ile giriş (E-22) kapalıdır; form yerinde kapalı olduğunu söyler, kurumsal sayfalar ve mevcut hesapların girişi etkilenmez. Panel karşılanmayan koşulu kurulum kontrol listesinde adıyla gösterir.

*Kullanıldığı ekranlar:* E-04, E-10, E-12, E-13, E-20, E-22 · E-31, E-44, E-45, E-46, E-47. Matriste: E-04, E-12, E-31, E-44…E-47. *Kaynak: `02 §3.1.5`, §3.1.6, §3.13.19, §3.32.8, §6.1.4, §10.8.2 · `03 §1.10.8`, §2.1.9, §3.2.1.17, §3.5.4.1, §3.5.4.4, §3.5.4.10, §8.7.1.1, §8.7.1.2, §8.7.5.1 · `10 §2` KP-38 · K-750, K-752, K-754 · devir: K-576.*

### 2.9 "Taslak" bandı ve yönetici şeridi (OB-09)

**Ne gösterir.** Giriş yapmış yöneticiye, vitrinde, panele dönüş yolunu ve baktığı kaydın taslak olduğunu. Yönetici vitrini ziyaretçinin gördüğü gibi görür; üstüne yalnız bu bileşen eklenir (K-748).

**2.9.1 Yönetici şeridi** sayfanın en başında, duyuru şeridinin üstünde durur, yalnız panel oturumu açık olan tarayıcıda görünür ve "Panele dön" bağlantısını taşır.

**2.9.2 "Taslak" bandı** taslak bir kaydın önizlemesinde şeridin yerini alır: kaydın taslak olduğunu söyler ve "Düzenlemeye dön" ile kaydın panel formuna götürür. Taslak ürünün ve taslak içerik kaydının sayfası ziyaretçiye "sayfa bulunamadı" döner; yöneticiye bu bantla açılır (`02 §3.7.5`, §3.27.25).

**2.9.3 "Taslak" etiketi** kendi sayfası olmayan taslak kaydı — SSS sorusu, şube, duyuru — yöneticiye göründüğü yerde işaretler: SSS sayfasında (E-09), İletişim sayfasının şube listesinde (E-10) ve duyuru şeridinde (`02 §3.27.25`). Ziyaretçi kaydı ve etiketi görmez.

**2.9.4 Vitrinin hesap alanı ve sepeti yönetici oturumundan etkilenmez:** yönetici hesabı vitrinde oturum sayılmaz; hesap alanı o tarayıcıdaki müşteri oturumunu — yoksa "Giriş yap"ı —, sepet o tarayıcının sepetini gösterir (`02 §10.2.5`). Şerit müşteriye hiçbir koşulda görünmez; panel oturumu kapanınca şerit kalkar ve taslak adresi "sayfa bulunamadı" döner.

*Kullanıldığı ekranlar:* şerit E-01…E-28; band E-04, E-08; etiket E-09, E-10 ve çerçevenin duyuru şeridi; önizlemeye çıkış E-33, E-41 (§3.7). Matriste: E-04, E-05, E-06, E-08, E-09, E-10, E-32, E-33, E-41. *Kaynak: `02 §3.7.5`, §3.27.25, §10.2.5 · `03 §2.1.11`, §8.1.7, §8.6.1.3, §8.6.3.1 · `10 §2` KP-40, KP-54 · K-748 · §1.3 GAP-6.*

### 2.10 Eşzamanlı düzenleme uyarısı (OB-10)

**Ne gösterir.** Kullanıcının ekranında gördüğü kaydın ya da siparişin, o ekrandayken başkasınca değiştiğini. Sessiz üzerine yazma yoktur; kural `02 §3.31`'dedir ve mekanizması Teknik Mimari ile Veri Modeli'nin kararıdır.

**2.10.1 Kayıt varyantı** — kurumsal içerik, ürün ve ayarlar (`02 §3.31.1`). İkinci kaydeden, kaydetme düğmesinin yerinde yerinde kalıcı mesajla kaydın o arada değiştiğini görür; kaydı kendiliğinden yazılmaz. Mesaj iki yol sunar: kaydın güncel hâlini açmak — yazdıkları ekranda görünür kalır — ya da uyarıyı görerek üzerine yazmak. Mesajın metni ve iki düğmesi K-800'dedir. Bölümleri ayrı kaydedilen ekranda (E-46) uyarı yalnız kaydedilen bölüm için çıkar (K-752).

**2.10.2 Sipariş varyantı** (`02 §3.31.2`). Yöneticinin gördüğü hâlden sonra sipariş değiştiyse işlem uygulanmaz: panel siparişin değiştiğini yerinde kalıcı mesajla söyler ve güncel hâli — güncel IBAN dahil — gösterir; yönetici işlemi o hâle göre yeniden başlatır. Üzerine yazma seçeneği yoktur. Onay penceresi açıkken değişen siparişte pencere kapanır ve aynı mesaj çıkar (2.4.4). Kural müşterinin sipariş sayfasındaki işleminde de işler: işlem o arada geçersizleştiyse sipariş sayfası sebebini söyler ve güncel hâli gösterir (E-16).

*Kullanıldığı ekranlar:* kayıt varyantı E-33, E-35, E-41…E-47 — E-46'da bölüm başına (9.18.11); sipariş varyantı E-37 ve E-16. Matriste: E-32, E-33, E-37, E-41, E-44. *Kaynak: `02 §3.31.1`, §3.31.2, §10.1.4 · `03 §3.5.1.10`, §3.5.2.15, §3.5.3.6, §8.1.1 · K-744, K-752, K-800 · devir: K-587.*

### 2.11 Ürün kartı (OB-11)

**Ne gösterir.** Bir ürünü liste içinde tek kartla; ürün başına tek kart vardır, varyant başına kart yoktur (`03 §2.1.5`).

**2.11.1 Ürün kartı** yukarıdan aşağıya şunları taşır: ana görsel işaretli görsel (`02 §3.11.2`) · ürün adı · KDV dahil fiyat. Kartın tek eylemi ürün sayfasını açmaktır; varyant ürün sayfasında seçildiği için kart sepete ekleme taşımaz (`03 §2.1.5`, §2.1.6; `02 §3.3.1`; K-774). Stok adedi, puan ve yorum gösterilmez (`03 §2.1.5`). Uzun ürün adı kartta iki satırda kesilir (K-741).

**2.11.2 İndirimli ürünün kartı** indirimli fiyatın yanında referans fiyatı ve — fiyat satırının hemen altında, tek satırda — indirimin başlangıç ve bitiş tarihini gösterir (`02 §3.9.2`). İndirim işareti metin taşır (2.5.6). Referansın nasıl hesaplandığı `02 §3.9.3`'tedir.

**2.11.3 Tükenmiş ürünün kartı** — bütün varyantları tükenmiş ürün — "Tükendi" işaretini taşır ve listede kalır (`02 §3.6.4`).

**2.11.4 İçerik kartı** hizmet tanıtımını ve referans işi liste sayfasında ve ana sayfa bloğunda gösterir: ana görsel, ad ya da başlık ve kısa açıklama (`02 §3.27.4`, §3.27.5); ana görseli olmayan kayıt kartta görselsiz, yalnız metinle görünür (K-755). Fiyat ve sepete ekleme taşımaz.

**2.11.5 Kartın kullanıldığı ızgara** ürün listelerinde sınıfa göre iki, üç ve dört sütundur (2.1.2). "İlgili ürünler" bloğu ve ana sayfanın ürün vitrini aynı kartı kullanır (`02 §3.27.10`; K-754).

*Varyantlar:* ürün kartı · indirimli · tükenmiş · içerik kartı. *Kullanıldığı ekranlar:* E-01, E-02, E-03, E-07, E-08. Matriste: E-02, E-04, E-08, E-33, E-41 (E-04, E-33 ve E-41 kartın beslendiği alanların ekranlarıdır). *Kaynak: `02 §3.3.1`, §3.6.4, §3.9.2, §3.9.3, §3.11.2, §3.27.4, §3.27.5, §3.27.10 · `03 §2.1.2`, §2.1.5, §2.1.6, §2.1.7, §2.2.4 · K-741, K-754, K-755, K-774 · devir: K-545; `03 §2.1.2`'nin "kartın düzeni" devri.*

### 2.12 Form bileşenleri

#### 2.12.1 Form alanı ve form düzeni (OB-12)

**2.12.1.1** Her alan görünür bir etiket taşır; zorunlu ve isteğe bağlı alan metinle ayırt edilir. Alan mesajı alanın hemen altındadır ve etikete bağlıdır (2.3.1.1).
**2.12.1.2** Form dar sınıfta tek sütundur (2.1.2). Gönderimde hatalar birlikte işaretlenir, yazılanlar silinmez (2.3.2); işlem düzeyindeki beklenmeyen hatada form içeriği korunur (2.7.3.1).
**2.12.1.3** Kapalı listeden seçilen alan — il, konu tipi, firma tipi, sebep — serbest metin kabul etmez; listeler ürünle gelir (`02 §3.14.3`, §3.32.3, §11).
**2.12.1.4** Sayısal sınırı olan alan sınırı aşan değeri alan mesajıyla reddeder; sınırın değeri mesajda kimliğinin (P-) gösterdiği değerdir ve bu dokümanda yazılmaz. Çitli süre ayarı çit dışındaki değeri sebebiyle reddeder (`03 §8.7.4.3`).
**2.12.1.5** Kaydetme başarısı kısa süreli bildirimle söylenir (2.3.1.4); bölümleri ayrı kaydedilen ekranda her bölüm kendi kaydet düğmesini taşır (K-752). Zorunlu alanı boşaltan ya da yayın kapısını bozan kayıt kaydedilmez ve engel yerinde söylenir (2.3.3).
**2.12.1.6** Yeni bir kişisel veri alanı açan formda aydınlatma metninin bağlantısı görünür; hangi ekranlar olduğu `02 §3.33.5`'tedir. Aydınlatmanın onay kutusu yoktur.
**2.12.1.7** Üyenin açtığı formda hesabın bilgisi ön dolu ve düzenlemeye açık gelir (`02 §3.32.1`).
**2.12.1.8 Adet alanı.** Tam sayı alır; birden küçük ve üst sınırı aşan değer alan mesajıyla reddedilir. Dijital kalemde ve adedi bir olan kalemde görünmez (`02 §3.12.4`). *Varyantlar:* **Sepete ekleme** (E-04, E-12) — üst sınır stok ve P-9'dur, aşım ekranın mesajlarıyla söylenir (5.4.10, 5.12.5). **İşlem adedi** (K-795) — iptal, cayma ve ayıp talebinde (E-17, E-18, E-19) ve panelin kalem işlemlerinde (E-37 — 9.9.19, 9.9.20, 9.9.25, 9.9.27, 9.9.28): alan kalem seçildiğinde kalemin satırında açılır; üst sınır kalemin o işleme açık adedidir ve satır onu kalemin adediyle birlikte söyler; varsayılan iptalde ve caymada açık adedin tamamıdır — kalemi seçmek kalemin tamamını seçer —, ayıp talebinde birdir; seçilen adet son adımın listesinde kalemin adıyla birlikte yazılır (`02 §7.1.6`; K-787, K-793).

*Kullanıldığı ekranlar:* form taşıyan bütün ekranlar; adet alanı E-04, E-12, E-17, E-18, E-19 · E-37 (§9.9); alan envanteri §7'dedir (5. oturum). *Kaynak: `02 §3.12.4`, §3.14.3, §3.16.8, §3.16.9, §3.32.1, §3.32.3, §3.33.5, §3.34.5, §7.1.6, §11 · `03 §8.7.4.3`, 1.7.4 · K-743, K-752, K-787, K-793, K-795.*

#### 2.12.2 Adres formu (OB-13)

**2.12.2.1** Alanlar ve zorunlulukları `02 §3.14.6`'dadır: alıcının adı ve soyadı, il — kapalı listeden —, ilçe, tek metin alanında açık adres; teslimat adresi ayrıca telefon taşır. Posta kodu ve T.C. kimlik numarası alanı yoktur (`03 §2.4.2`).
**2.12.2.2 Varyantlar.** *Teslimat* (E-13): üye defterinden seçer ya da yeni adres yazar ve isterse aynı adımda deftere kaydeder; misafir alıcı her siparişte yazar; telefonu olmayan defter kaydı seçilirse telefon burada istenir (`03 §2.4.2`). *Fatura* (E-13): "teslimat adresimle aynı" işaretli gelir; işaret kaldırılınca ikinci form açılır; telefon taşımaz (`03 §2.4.3`; K-756). *Defter kaydı* (E-26): adres ayrıca bir ad taşır; telefon isteğe bağlıdır (`03 §9.3.2`). *Panelde düzeltme* (E-37): yönetici siparişin adresini düzeltir; ekran teyit hatırlatmasını gösterir (`03 §8.4.1`; K-735).
**2.12.2.3** Teslimat yapılmayan ildeki adres kabul edilmez ve sebebi yerinde söylenir; defterdeki o adres seçilemez (`02 §3.20.1`). Sepette fiziksel kalem yoksa teslimat formu hiç görünmez (`02 §3.14.4`).

*Kullanıldığı ekranlar:* E-13, E-26 · E-37 (matristen). *Kaynak: `02 §3.14`, §3.20.1 · `03 §1.11.23`, §2.4.2, §2.4.3, §3.2.1.10, §8.4.1, §9.3.2 · `10 §2` KP-12 · K-735, K-756 · devir: K-561.*

#### 2.12.3 IBAN alanı (OB-14)

**2.12.3.1** Alan yalnız havale hattında görünür; kart hattında IBAN alanı yoktur ve istenmez. IBAN girişte biçim ve sağlama basamağıyla denetlenir; hata alan mesajıdır. Alanın yanında aydınlatma metninin bağlantısı durur (`02 §7.4.5`, §3.33.5).
**2.12.3.2 Varyantlar.** *Beyanla giriş:* iptal, gecikme feshi ve cayma beyanı ekranlarında formun alanıdır (E-17, E-18). *İstekle açılan alan:* IBAN isteği doğduğunda sipariş sayfasında (E-16) o kalem için açılır; kart iadesi gerçekleşmeyip havale yolu açıldığında sayfa önce kart iadesinin gerçekleşmediğini söyler ve IBAN girmek müşterinin seçimidir (`03 §2.8.4.3`). *Panelde salt okunur:* geri ödemeyi işleyen yönetici müşterinin IBAN'ını görür, giremez (`03 §8.3.3.2`); tek istisna `02 §10.1.2`'nin saydığı aktarmadır.
**2.12.3.3** Girilen IBAN geri ödeme işlenene kadar sipariş sayfasından düzeltilir (`03 §1.11.12`). IBAN isteğinin ekrandaki öteki yüzü "IBAN bekleniyor" işareti ve listesidir (2.5.3.4, 2.6.2).
**2.12.3.4** Havale siparişinde firmanın siparişe donmuş IBAN'ı bu bileşen değildir: teyit ekranında ve sipariş sayfasında salt okunur bir bilgidir (`03 §2.5.2.1`).

*Kullanıldığı ekranlar:* E-16, E-17, E-18 · E-31, E-36, E-37 (matristen). *Kaynak: `02 §3.33.5`, §7.4.5, §10.1.2 · `03 §1.7.1.9`, §1.11.11, §1.11.12, §2.5.2.1, §2.7.5, §2.8.4.3, §3.3.7, §3.3.19, §3.3.27, §8.3.3.2 · devir: K-498, K-569, K-690.*

#### 2.12.4 Sebep seçimi (OB-15)

**2.12.4.1** Sebep kapalı listeden seçilir; listeler ürünle gelir ve firma düzenlemez. Sebep seçilmeden işlemin onayı açılmaz. "Diğer" seçilince açıklama alanı zorunlu olur.
**2.12.4.2 Varyantlar.** *Firma iptali:* liste `03 §8.3.1.1` ile `02 §7.2.4`'tedir; "stokta bulunamadı" seçilince yasal uyarı onaydan önce görünür (2.4.3). *Durum düzeltmesi:* liste `03 §8.4.5`'tedir. Kalem çıkarmada sebep listesi yoktur (`03 §8.4.2`).
**2.12.4.3** Seçilen sebep işlemin kaydında kalır ve sonucu belirler — hangi sebebin hangi geri ödemeyi doğurduğu `03 §8.3.1.2`'dedir; ekran bu sonucu onay cümlesinde söyler.

*Kullanıldığı ekranlar:* E-37 (matristen). *Kaynak: `02 §7.2.4`, §10.4.6 · `03 §1.6.2`, §1.11.21, §1.11.26, §8.3.1.1, §8.3.1.2, §8.4.2, §8.4.5 · devir: K-570.*

#### 2.12.5 Tarih alanı (OB-16)

**2.12.5.1 Olay tarihi** — teslim tarihi, iade malının ulaşma tarihi, başka kanaldan gelen bildirimin firmaya ulaştığı tarih: geçmişe dönük girilebilir, ileri tarih reddedilir; alt sınırı olan tarih sınırın altındaki değeri sebebiyle reddeder — sınırlar `03 §8.2.6`, §8.3.2.1 ve §8.4.8'dedir. Ret alan mesajıdır.
**2.12.5.2 Sıra sorusu.** Girilen tarih cayma beyanının günüyle çakışırsa panel hangisinin önce olduğunu sorar; soru formun alanıdır, pencere değildir (`03 §2.8.1.3`, §8.2.6, §8.4.4).
**2.12.5.3 Tarih aralığı** — indirimin, kuponun ve duyurunun başlangıç ve bitişi: ileri tarih serbesttir; duyuruda iki tarih de isteğe bağlıdır (`02 §3.27.21`).
**2.12.5.4** Tarihin yazım biçimi §8.2'de yazılır (5. oturum).

*Kullanıldığı ekranlar:* olay tarihi ve sıra sorusu E-37; tarih aralığı E-33, E-35, E-41 ve raporların dönem süzgeci — E-51, E-52, E-53. Matriste: E-31, E-37. *Kaynak: `02 §3.9.2`, §3.27.21, §10.1.2 · `03 §1.11.17`, §1.11.25, §1.11.28, §2.8.1.3, §8.2.6, §8.3.2.1, §8.4.4, §8.4.8 · devir: K-491, K-554, K-712.*

#### 2.12.6 Kupon alanı (OB-17)

**2.12.6.1** Alan ödeme adımındadır (`02 §3.10.1`), sipariş özetinin içinde, toplamın üstünde; kapalı bir satır olarak gelir ("Kupon kodum var") (K-757).
**2.12.6.2** Açılınca kod girilir — büyük-küçük harf ayrımı yoktur (K-806) —; uygulanan kod adı ve indirdiği tutarla görünür ve "Değiştir" ile "Kaldır" taşır. Siparişe en fazla bir kod uygulanır.
**2.12.6.3** Geçersiz kod alanın altında alan mesajıyla söylenir. L-6 aşılınca alan kapanır ve nötr mesajı kendi yerinde taşır (2.3.5).
**2.12.6.4** Girişten dönen müşteride girilmiş kod yeniden uygulanır; geçersizse söylenir (§3.6.2).

*Kullanıldığı ekranlar:* E-13. *Kaynak: `02 §3.10.1`, §3.10.5 · `03 §2.4.4`, §3.2.1.18 · K-757, K-758, K-806 · §1.1.11 GA-2.*

#### 2.12.7 Onay kutuları (OB-18)

**2.12.7.1** Sipariş onayı iki kutuyla, sepete göre üçüncü ve dördüncü kutuyla verilir; hangi kutunun ne zaman çıktığı `02 §3.24.1` ve §3.24.2'dedir. Hiçbir kutu işaretli gelmez (K-756).
**2.12.7.2** Kutular onay bölümünde, iki yasal metnin altında ve onay düğmesinin hemen üstünde durur; kutular ile düğme arasına başka içerik girmez (K-756). Bölümün tam sırası E-13'ün tanımındadır (§5.13.9).
**2.12.7.3** İşaretlenmemiş kutuyla onaya basılırsa sipariş oluşmaz; eksik kutu alan mesajıyla işaretlenir ve ekran ilk eksiğe gider. Onay adımı açıkken metnin sürümü değişirse kutuların işareti kalkar (K-756).
**2.12.7.4** KVKK rıza kutusu, yaş beyanı ve pazarlama onayı kutusu yoktur (`02 §3.24.1`, §3.24.4, §3.13.18).
**2.12.7.5** Sipariş sayfası işaretlenen kutuların kaydını — kutunun metni ve kapsadığı kalemlerle — salt okunur gösterir (`02 §3.24.6`).
**2.12.7.6** Dijital kalem ve hizmet kalemi kutularının nihai metni `02 §3.24.2`'nin metnidir; kutu kapsadığı kalemleri adıyla sayar ve kapsamda birden çok kalem varsa metin çoğul söylenir (K-771; §5.13.9).

*Kullanıldığı ekranlar:* E-13, E-16. *Kaynak: `02 §3.13.18`, §3.24.1, §3.24.2, §3.24.4, §3.24.6 · `03 §2.4.7`, §2.6.4 · K-756, K-771.*

#### 2.12.8 Görsel ve dosya yükleme (OB-19)

**2.12.8.1 Galeri** — ürünün ve kurumsal içerik kaydının görselleri: birden çok görsel yüklenir, sıra elle belirlenir ve ana görsel işareti sıradan bağımsızdır (`02 §3.11.2`, §3.27.15).
**2.12.8.2** Her görselin alternatif metin alanı isteğe bağlıdır; boşsa sistem metni üretir ve alan bunu söyler (`02 §3.11.4`).
**2.12.8.3** Biçim, tek dosya boyutu ve adet sınırını aşan dosya yüklenmez ve sebebi alan mesajıyla söylenir; sınırların kimlikleri P-22…P-25'tir.
**2.12.8.4** Yükleme sürerken bileşen ilerlemeyi gösterir ve form kaydedilemez (2.7.2).
**2.12.8.5 Tek görsel** — logo ve site simgesi: tek dosya; yüklenmemişse geri düşüşü `02 §3.29.2` ve §3.29.4'tedir. **Dijital dosya** — ürün ya da varyant düzleminde tek dosya (P-26; `03 §8.1.6`).
**2.12.8.6** Müşteri tarafında dosya yükleme yoktur: iletişim formu ve ayıp talebi dosya eki almaz (`02 §3.32.2`; `03 §2.9.2`).

*Kullanıldığı ekranlar:* E-33, E-41, E-43. *Kaynak: `02 §3.11.1`–§3.11.4, §3.27.15, §3.29.2, §3.29.4, §3.32.2 · `03 §2.9.2`, §8.1.5, §8.1.6.*

### 2.13 Liste ve tablo (OB-20)

**Ne gösterir.** Aynı türden kayıtların listesini; aramasını, süzgecini ve sayfalarını.

**2.13.1 Sıra.** Ürün listeleri tek bir sabit düzende gelir ve ziyaretçiye sıralama seçeneği sunulmaz (`02 §3.5.3`); kurumsal içerik listeleri firmanın panelde verdiği sırayı izler (`02 §3.27.11`). Panel listelerinin sırası ekranının tanımındadır.

**2.13.2 Arama ve süzgeç yalnız kaynağın saydığı listelerde vardır:** vitrinde ürün adında arama ve iki sabit süzgeç ekseni (`02 §3.5.1`, §3.5.2) · sipariş listesinde sipariş numarası ve iletişim e-postasıyla arama ve durum süzgeci · ürün listesinde ürün adıyla arama ve yayın durumu süzgeci (K-737) · üye kaydı görünümünde e-postayla arama · işlem izinde tarih aralığı ve yönetici süzgeci (`02 §10.1.2`; `03 §8.9.4`) · Talepler listesinde tür ve durum süzgeci (K-751). Kupon ve kurumsal içerik listelerinde arama ve süzgeç yoktur (K-737). Bekleyen iş sayacının götürdüğü liste aynı listenin süzülmüş hâlidir; ayrı bir ekran değildir (`02 §10.6.1`).

**2.13.3 Uzun liste sayfalara bölünür** (K-766). Gezinme sayfa numaralıdır — önceki, sonraki ve sayfa numaraları —; kendiliğinden yüklenen sonsuz kaydırma yoktur. Vitrinin ürün listeleri sayfa başına 24 ürün, panelin listeleri sayfa başına 25 kayıt gösterir; sayılar düzenin sabitidir, firma ayarı değildir. Arama ve süzgeç değişince liste ilk sayfasına döner; kayıttan listeye dönen kullanıcı bıraktığı sayfayı ve süzgeci bulur. Tek sayfaya sığan listede gezinme görünmez.

**2.13.4 Panel listeleri** geniş sınıfta tablodur; orta sınıfta ikincil sütunlarını satırın altına alır; dar sınıfta kart listesine döner — kart satırın başlığını, durumunu, işaretlerini ve birincil işlemini taşır, kalanı ayrıntıdadır (K-741). Tablo hiçbir sınıfta yatay kaydırılmaz. Hangi sütunların birincil olduğu ekranın tanımında bu kuraldan türetilir.

**2.13.5 Vitrin listeleri** ürün ızgarasıdır (2.11.5); süzgeçler dar ve orta sınıfta açılır bölümde, geniş sınıfta listenin yanındadır (2.1.2). Boş liste hâli 2.7.1.2'dir.

**2.13.6** Toplu işlem yoktur: liste ekranından toplu güncelleme, toplu yeniden gönderim ve içe aktarma bulunmaz (`02 §10.7.2`; `03 §8.1.15`; K-761).

*Varyantlar:* ürün ızgarası · içerik listesi · panel tablosu ve kart listesi. *Kullanıldığı ekranlar:* E-02, E-03, E-07, E-27 · E-32, E-35, E-36, E-38, E-40, E-41, E-49, E-51, E-52. *Kaynak: `02 §3.5.1`–§3.5.3, §3.27.11, §3.34.5, §10.1.2, §10.6.1, §10.7.2 · `03 §2.1.2`–§2.1.4, §8.1.15, §8.2.11, §8.9.4 · K-737, K-741, K-751, K-761, K-766.*

## 3. Navigasyon haritası

> **Ne yazılır:** Ekranlar arası geçişler. **Ekran tanımlarından önce** konumlandırılır.

**Yazım turunun 1. oturumu (2026-10-04, v0.6).** Harita iki tarafı birlikte taşır (K-724): vitrinin ve panelin gezinme iskeleti, iki tarafın geçişleri, sitenin dışından girişler, girişten ve çıkıştan dönüş ve panel ile vitrin arasındaki geçişler. Geçişler tablodur: **nereden · tetikleyici · nereye · koşul · Kaynak**; satır kimliği `3.n.k`'dır (konvansiyon 5). Ekranlar §4'ün kimliğiyle anılır. Harita geçişi yazar, ekranın içini yazmaz: bir geçişin koşulu bir akış kuralıysa kural kopyalanmaz, `03`'ün satırına işaret edilir. Aynı ekranın içinde kalan işlemler — süzme, sayfa değiştirme, satır içi düzenleme, onay penceresi — geçiş değildir ve ekranının tanımındadır.

### 3.1 Vitrinin gezinme iskeleti

Vitrin çerçevesinin (§2.2) her bölgesinin götürdüğü yer. İskelet E-01…E-28'in tamamında aynıdır; tek fark E-13'ün sadeleşmiş üst bölümüdür (3.1.10).

| # | Nereden | Tetikleyici | Nereye | Koşul | Kaynak |
|---|---|---|---|---|---|
| 3.1.1 | Duyuru şeridi | Duyurunun bağlantısı | Firmanın girdiği adres | Duyuru Yayında ve aralığında; bağlantı girilmişse | `02 §3.27.21` · K-746 |
| 3.1.2 | Üst bölüm — logo, marka adı ya da alan adı | Dokunma | E-01 | — | `02 §3.29.2`, §3.29.3 · K-746 |
| 3.1.3 | Üst bölüm — arama | Ürün adı yazıp arama | E-03 | — | `03 §2.1.3` · K-746 |
| 3.1.4 | Üst bölüm — hesap alanı (oturum yok) | "Giriş yap" | E-22 | Girişten sonra dönüş §3.6 | `03 §9.2.1` · K-746 |
| 3.1.5 | Üst bölüm — hesap alanı (oturum yok) | "Sipariş takibi" | E-15 | — | `02 §6.1.1` · `03 §2.6.2` · K-746 |
| 3.1.6 | Üst bölüm — hesap alanı (üye) | "Hesabım" | E-25 | Üye oturumu açık | K-746 |
| 3.1.7 | Üst bölüm — hesap alanı (üye) | "Siparişlerim" | E-27 | Üye oturumu açık | `03 §2.6.1` · K-746 |
| 3.1.8 | Üst bölüm — hesap alanı (üye) | "Çıkış" | §3.6.4 | Üye oturumu açık | `03 §9.2.3` · K-746 |
| 3.1.9 | Üst bölüm — sepet | Dokunma | E-12 | — | `03 §2.3.2` · K-746 |
| 3.1.10 | E-13'ün sadeleşmiş üst bölümü | Logo ya da marka adı · "Sepete dön" | E-01 · E-12 | Menü, arama ve duyuru şeridi yoktur | K-757 |
| 3.1.11 | Menü — "Ürünler" | Birinci, ikinci ya da üçüncü seviye kategori | E-02 | Kategorinin kendisinde ya da alt dallarında yayında ürün var | `02 §3.28.4`, §3.28.6 · `03 §2.1.2` · K-746 |
| 3.1.12 | Menü — Hizmetlerimiz · Referanslarımız | Dokunma | E-07 | Tipin yayında kaydı var | `02 §3.28.4` · `03 §2.2.2` |
| 3.1.13 | Menü — Hakkımızda | Dokunma | E-08 | Hakkımızda Yayında | `02 §3.28.4` |
| 3.1.14 | Menü — SSS | Dokunma | E-09 | Yayında soru var | `02 §3.27.6`, §3.28.4 |
| 3.1.15 | Menü — "menüde göster" işaretli genel sayfa | Dokunma | E-08 | Sayfa Yayında ve işaretli | `02 §3.28.5` |
| 3.1.16 | Menü — İletişim | Dokunma | E-10 | Her zaman; menünün son öğesi | `02 §3.27.8` · K-746, K-747 |
| 3.1.17 | Üst bölümün ince satırı ve altbilgi — sosyal medya ve WhatsApp | Dokunma | Platformun kendi sayfası (ürünün dışında) | Bağlantı girilmişse | `02 §3.27.12` · K-746 |
| 3.1.18 | Altbilgi — genel sayfalar | Dokunma | E-08 | Sayfa Yayında | `02 §3.28.5` · K-747 |
| 3.1.19 | Altbilgi — aydınlatma metni · çerez politikası · "İşlem rehberi" | Dokunma | E-11 | Her sayfada | `02 §3.24.8`, §3.33.5 · K-747 |
| 3.1.20 | Altbilgi — ETBİS doğrulama bandı | Dokunma | ETBİS'in doğrulama sayfası (ürünün dışında) | ETBİS doğrulama bilgisi girilmişse | `02 §3.1.4` · K-747 |
| 3.1.21 | Altbilgi — kimlik bloğu | — | Geçiş değildir; bilgi yerinde okunur | Her sayfada | `02 §3.1.4` · K-747 |

Dar sınıfta 3.1.3 ve 3.1.11–3.1.17 açılır menünün içindedir; hedefleri değişmez (2.2.4). Menünün sırası ana sayfa düzenine göre değişir (2.2.3); sıra hedefi değiştirmez.

*Kaynak: `02 §3.1.4`, §3.24.8, §3.27.6, §3.27.8, §3.27.12, §3.27.21, §3.28.4–§3.28.6, §3.29.2, §3.29.3, §3.33.5, §6.1.1 · `03 §2.1.2`, §2.1.3, §2.2.2, §2.3.2, §2.6.1, §2.6.2, §9.2.1, §9.2.3 · K-746, K-747, K-757.*

### 3.2 Panelin gezinme iskeleti

Panel çerçevesinin (2.2.8) menüsü sekiz girişlidir ve sırası günlük işten ayara iner (K-749). `02 §10.1.2`'nin on iki alanının her biri bu girişlerden birindedir.

| # | Menü girişi | Alt giriş | Ekran | `02 §10.1.2`'nin alanı | Kaynak |
|---|---|---|---|---|---|
| 3.2.1 | Ana sayfa | — | E-31 | — (bekleyen işler: `02 §10.6.1`) | K-749, K-750 |
| 3.2.2 | Siparişler | — | E-36 → E-37 | Siparişler | K-749 |
| 3.2.3 | Talepler | — | E-38 → E-39 ya da E-37 | Talepler | K-749, K-751 |
| 3.2.4 | Katalog | Ürünler | E-32 → E-33 | Katalog | K-749 |
| 3.2.5 | Katalog | Kategoriler | E-34 | Katalog | K-749 |
| 3.2.6 | Katalog | Kuponlar | E-35 | Katalog | K-749 |
| 3.2.7 | İçerik ve site | Kurumsal içerik | E-41 | Kurumsal içerik ve site | K-749 |
| 3.2.8 | İçerik ve site | Ana sayfa ve menü | E-42 | Kurumsal içerik ve site | K-749 |
| 3.2.9 | İçerik ve site | Marka | E-43 | Marka | K-749 |
| 3.2.10 | Üyeler | — | E-40 | Üye hesapları | K-749 |
| 3.2.11 | Raporlar | Satış özeti | E-51 | İz, özet ve dışa aktarma | K-749 |
| 3.2.12 | Raporlar | İşlem izi | E-52 | İz, özet ve dışa aktarma | K-749 |
| 3.2.13 | Raporlar | Dışa aktarma | E-53 | İz, özet ve dışa aktarma | K-749 |
| 3.2.14 | Ayarlar | Firma kimliği ve satış | E-44 | Firma kimliği ve satış | K-749, K-752 |
| 3.2.15 | Ayarlar | Ödeme yöntemleri | E-45 | Ödeme yöntemleri | K-749 |
| 3.2.16 | Ayarlar | Kargo, süreler ve varsayılanlar | E-46 | Kargo, teslimat ve ödeme süreleri | K-749, K-752 |
| 3.2.17 | Ayarlar | Yasal metinler | E-47 | Yasal metinler | K-749 |
| 3.2.18 | Ayarlar | Yönetici hesapları | E-49 | Yönetici hesapları | K-749 |

**Üst satır** her panel ekranında üç geçiş taşır:

| # | Nereden | Tetikleyici | Nereye | Koşul | Kaynak |
|---|---|---|---|---|---|
| 3.2.19 | Üst satır — satış durumu göstergesi | Dokunma | E-44, "Satış durumu" bölümü | — | K-749, K-752 |
| 3.2.20 | Üst satır | "Siteyi görüntüle" | E-01 (vitrin; §3.7) | — | K-749 |
| 3.2.21 | Üst satır — yöneticinin adı | "Hesabım" | E-50 | — | K-749 |
| 3.2.22 | Üst satır — yöneticinin adı | "Çıkış" | E-29 (§3.6.4) | — | K-749 · `03 §9.2.3` |

Menüde yeri olmayan panel ekranları: E-29 ve E-30 oturumdan önce açılır (§3.5); E-37 ve E-39'a listelerinden, E-33'e ürün listesinden, E-54'e E-50'den ve bağlantıdan gidilir. E-48 ekran değildir (§4.3).

*Kaynak: `02 §10.1.2`, §10.6.1 · `03 §9.2.3` · K-749, K-750, K-751, K-752.*

### 3.3 Müşteri tarafının geçişleri

Çerçevenin geçişleri §3.1'dedir ve burada yinelenmez.

| # | Nereden | Tetikleyici | Nereye | Koşul | Kaynak |
|---|---|---|---|---|---|
| 3.3.1 | E-01 ürün vitrini | Ürün kartı | E-04 | Yayında ürün var | `03 §2.1.1`, §2.1.5 · K-754 |
| 3.3.2 | E-01 ürün vitrini | Bloğun altındaki birinci seviye kategori bağlantısı | E-02 | Kategoride yayında ürün var | K-754 |
| 3.3.3 | E-01 kurumsal blok | Hakkımızda sayfasının bağlantısı | E-08 | Hakkımızda Yayında | `03 §2.2.1` · K-753 |
| 3.3.4 | E-01 Hizmetlerimiz ve Referanslarımız blokları | İçerik kartı · "Tümünü gör" | E-08 · E-07 | İşaretli kayıt var | `03 §2.2.1` · K-753, K-755 |
| 3.3.5 | E-02 · E-03 | Ürün kartı | E-04 | — | `03 §2.1.5` |
| 3.3.6 | E-02 · E-04 | Kırıntı yolu | E-02 (üst kategori) | Ürün sayfasında yol ana kategoriden üretilir | `02 §3.30.1` · `03 §2.1.2` |
| 3.3.7 | E-04 | Varyantı adediyle sepete ekleme | E-04'te kalır; sonuç kısa süreli bildirimle söylenir, sepete çerçeveden gidilir (3.1.9) | Satış açık; varyant satın alınabilir | `03 §2.1.8`, §2.3.1 · K-743 |
| 3.3.8 | E-07 | İçerik kartı | E-08 | — | `03 §2.2.2`, §2.2.3 |
| 3.3.9 | E-08 (hizmet tanıtımı) | "Bize ulaşın" | E-10, iletişim formu | — | `02 §3.27.4` · `03 §2.2.7` |
| 3.3.10 | E-08 (hizmet tanıtımı, referans iş) | "İlgili ürünler"de ürün kartı | E-04 | Bağlı ürün Yayında | `03 §2.2.4` |
| 3.3.11 | E-10 | İletişim formunun gönderilmesi | E-10'da kalır; ekran gönderim sonrası hâline geçer | Form açık (2.8.3) | `03 §2.2.8`, §2.2.9 |
| 3.3.12 | E-05 · E-06 | Dönüş bağlantıları | E-01 · ürünler | — | `02 §3.30.4`, §3.7.6 |
| 3.3.13 | E-12 | Kalemin adı ya da görseli | E-04 | Ürün Yayında | `03 §2.3.2` · K-775 |
| 3.3.14 | E-12 | Ödeme adımına geçiş | E-13 | Satış açık; siparişe girebilecek en az bir kalem var; asgari sipariş tutarı karşılanıyor | `03 §2.3.8` |
| 3.3.15 | E-13 | "Sepete dön" · siparişe girebilecek kalem kalmaması · hiçbir ödeme yönteminin kullanılamaması | E-12 | — | `03 §2.4.8`, §3.2.1.15 · K-757 |
| 3.3.16 | E-13 İletişim bölümü | "Giriş yap" — bölümün başındaki bağlantı ya da e-posta alanının altındaki hatırlatma | E-22 → E-13 | Dönüş §3.6.2 | `03 §2.4.1` · K-758 |
| 3.3.17 | E-13 | "Siparişi onayla — ödeme yükümlülüğü doğar" | E-14 | Özet değişmemiş; kutular işaretli; sipariş doğar | `03 §2.4.8`, §2.4.9 · K-756 |
| 3.3.18 | E-14 (kart) | Sipariş numarası gösterildikten sonra | Ödeme sağlayıcısının sayfası (ürünün dışında) | Ödeme yöntemi kart | `03 §2.4.9`, §2.5.1.1 |
| 3.3.19 | E-14 (havale) | Siparişe ulaşma bağlantısı | Üyede E-16 · misafir alıcıda E-15 | Ödeme yöntemi havale; ekran IBAN'ı, sipariş numarasını, ödenecek toplamı ve son ödeme gününü gösterir; misafire sipariş sayfasına yeni bir giriş yolu açılmaz (`02 §3.22.3`) | `03 §2.5.2.1` · K-776 |
| 3.3.20 | E-27 | Sipariş satırı | E-16 | Üye oturumu açık | `03 §2.6.1` |
| 3.3.21 | E-15 | Sipariş numarası ve e-postayla sorgu | E-16 | İkisi eşleşirse; eşleşmezse E-15'te kalır | `03 §2.6.2` |
| 3.3.22 | E-16 | İptal · gecikme nedeniyle fesih | E-17 | İşlem açık: `03 §1.11.1`–§1.11.3, §1.11.8 | `03 §2.7.1`–§2.7.3, §2.7.7 |
| 3.3.23 | E-16 | Cayma | E-18 | İşlem açık: `03 §1.11.5`, §1.11.6 | `03 §2.8.1.1`, §2.8.2.1 |
| 3.3.24 | E-16 | "Sorun bildir" · ayıp talebini yeniden açma | E-19 | İşlem açık: `03 §1.11.9`, §1.11.10 | `03 §2.9.1`, §2.9.6 |
| 3.3.25 | E-17 · E-18 · E-19 | Son adımın onayı · "Vazgeç" | E-16 | Onayda sonuç sipariş sayfasında kalıcı durur | K-732, K-744 |
| 3.3.26 | E-22 | Kayıt bağlantısı · "şifremi unuttum" | E-20 · E-23 | Kayıt ekranı açık (2.8.3) | `03 §9.1.1`, §9.2.4 |
| 3.3.27 | E-22 · E-20 | "Google ile giriş" | Google'ın sayfası (ürünün dışında) → §3.6 | Google uygulaması kurulumda tanımlı | `03 §9.1.1`, §9.1.6, §9.2.2 |
| 3.3.28 | E-20 | Kaydın gönderilmesi | E-20'de kalır; ekran doğrulama bağlantısının gönderildiğini söyleyen hâline geçer | — | `03 §9.1.2`, §9.1.3 |
| 3.3.29 | E-21 · E-23 (yeni şifre kurulduktan sonra) | Giriş bağlantısı | E-22 | Kayıt doğrulandı ya da daha önce doğrulanmıştı · bağlantının yerine yenisi gönderilmişti — girişte yeniden istenir · şifre kuruldu | `03 §9.1.3`, §9.1.4, §9.2.5 · K-780 |
| 3.3.30 | E-25 | Şifre değiştirme · e-posta değiştirme · hesap silme | E-24 → E-25 | Üç işlem yeniden doğrulama ister | `03 §9.2.6` |
| 3.3.31 | E-25 | Hesap silmenin son adımının onayı | E-25'in sonuç hâli (5.25.13); oturum kapanmıştır | Geri alma bağlantısı açık değil | `03 §9.4.1`, §9.4.2 · K-732, K-781 |
| 3.3.32 | E-25 · E-26 · E-27 | Hesap ekranlarının iç gezinmesi | E-25 · E-26 · E-27 | Üye oturumu açık | K-768 |
| 3.3.33 | E-28 | Şifre sıfırlama bağlantısı eski adrese gönderilir | E-28'de kalır; yeni şifre §3.5.5'in bağlantısıyla E-23'te kurulur | Geri alma bağlantısı geçerli | `03 §9.3.6` |
| 3.3.34 | E-21 (kayıt bağlantısının süresi dolmuş) | Kayıt bağlantısı | E-20 | Kayıt silinmiş; e-posta yeniden kayda açık | `03 §9.1.5`, §3.4.1 · K-780 |
| 3.3.35 | E-21 (yeni e-posta adresinin bağlantısı) | Sonraki adımın bağlantısı | Oturum açıksa E-25 · değilse E-22 (§3.6.1) | Değişiklik geçerli oldu ya da bağlantı geçersiz | `03 §9.3.5`, §4.1.1 · K-734, K-780 |
| 3.3.36 | E-25 (şifresi olmayan hesap) | Şifre belirleme | E-23, isteme adımı; e-posta ön dolu | Hesapta şifre yok — Google ile açılmış hesap | `02 §3.13.8`, §3.13.10 · `03 §9.3.3`, §9.2.4 |
| 3.3.37 | E-24 (Google girişi taşıyan hesap) | Google ile doğrulama | Google'ın sayfası (ürünün dışında) → E-25, başlatılan işlemin formu | Google uygulaması kurulumda tanımlı | `02 §3.13.13` · `03 §9.2.6` |

**Sipariş sayfasına (E-16) üç yoldan girilir** (`02 §3.22.3`): üyenin sipariş geçmişi (3.3.20) · misafir alıcının sipariş takibi girişi (3.3.21) · sipariş e-postasındaki bağlantı (3.5.1). Kart ödemesinden dönüş de sipariş sayfasına iner (3.5.8).

*Kaynak: `02 §3.7.6`, §3.13.8, §3.13.10, §3.13.13, §3.22.3, §3.27.4, §3.30.1, §3.30.4 · `03 §1.11.1`–§1.11.10, §2.1–§2.9, §3.4.1, §4.1.1, §9.1, §9.2, §9.3.3, §9.3.5, §9.3.6, §9.4 · K-732, K-743, K-744, K-753…K-758, K-768, K-775, K-776, K-780, K-781.

### 3.4 Panelin geçişleri

Menünün geçişleri §3.2'dedir. Panelin omurgası sayaçtan siparişe iner: ana sayfa → süzülmüş liste → kayıt → işlem (K-741).

| # | Nereden | Tetikleyici | Nereye | Koşul | Kaynak |
|---|---|---|---|---|---|
| 3.4.1 | E-29 | Giriş | E-31 ya da istenen panel ekranı (§3.6.1) | Yönetici hesabı; şifreyle | `03 §9.2.7` |
| 3.4.2 | E-31 "Bekleyen işler" | Sayaç | E-36'nın o sayaca süzülmüş hâli — beş sayaç; "açık talep" sayacında E-38'in açık taleplere süzülmüş hâli | Sıfır sayaç da boş listeye götürür | `02 §10.6.1` · `03 §8.9.1` · K-750, K-751 |
| 3.4.3 | E-31 "Uyarılar" | Uyarı satırı | Satış kapalı: karşılanmayan koşulun ekranı — E-44, E-45, E-46 ya da E-47 · "firma bildirimleri ulaşmıyor": E-44 · "e-posta ulaşmadı" satırı: E-36'nın bu işarete süzülmüş hâli · kart iadesinin gerçekleşmediği uyarısı: E-36'nın "iade ve geri ödeme bekleyen" sayacına süzülmüş hâli · kanal uyarısı: geçiş taşımaz — çözümü kurulum ayarıdır | Koşul sürerken | `02 §6.1.1`, §10.6.1 · K-750, K-760, K-761, K-769 |
| 3.4.4 | E-31 kurulum kontrol listesi | Madde | Kimlik alanları: E-44 · ödeme yöntemi: E-45 · aydınlatma metni ve çerez politikası: E-47 · iade adresi: E-46 | Madde eksikken | `02 §10.8.2` · `03 §8.7.1.1` · K-750, K-752 |
| 3.4.5 | E-31 "Müşteriden beklenenler" | "IBAN bekleniyor" satırı · "Tümünü gör" | Satır: E-37 · "Tümünü gör": E-36'nın "IBAN bekleniyor" süzgeciyle | — | K-750, K-802 |
| 3.4.6 | E-36 | Sipariş satırı | E-37 | — | `03 §8.2.11` |
| 3.4.7 | E-37 | Sipariş işlemi ya da müdahale | E-37'de kalır: işlemin formu, ardından onay penceresi (OB-04); sonuçta siparişin güncel hâli | İşlem açık: `03 §1.11.15`–§1.11.41 | `03 §8.2`–§8.4 · K-744 |
| 3.4.8 | E-38 | Talep satırı | İletişim talebi: E-39 · ayıp talebi: E-37, siparişin ayıp talebi bölümü | — | `03 §8.4.10`, §8.6.4.2 · K-751 |
| 3.4.9 | E-39 · E-37 (ayıp talebi bölümü) | Listeye dönüş | E-38, bırakılan süzgeçle | — | K-766 |
| 3.4.10 | E-32 | Yeni ürün · ürün satırı | E-33 | — | `03 §8.1.1` |
| 3.4.11 | E-33 | "Önizle" ya da "Sitede gör" | E-04 (vitrin; §3.7) | Taslak üründe "Taslak" bandıyla | `03 §8.1.7` · K-748, K-749 |
| 3.4.12 | E-41 | Kayıt satırı · yeni kayıt | E-41'in kayıt formu | — | `03 §8.6.1.1` |
| 3.4.13 | E-41 kayıt formu | "Önizle" ya da "Sitede gör" | E-08; kendi sayfası olmayan kayıtta göründüğü yer — E-09, E-10 ya da duyuru şeridi (vitrin; §3.7) | — | `03 §8.6.1.3` · K-748 |
| 3.4.14 | E-40 | Üyenin bağlı siparişi | E-37 | Kayıt bulunmuşsa | `03 §9.4.5` |
| 3.4.15 | E-44 "Satış durumu" bölümü | Karşılanmayan koşulun bağlantısı | E-44'ün kimlik formu · E-45 · E-46 · E-47 | Koşul karşılanmıyorken | K-752 |
| 3.4.16 | E-50 | Şifre ya da e-posta değiştirme | E-54 (yeniden doğrulama) → E-50 | — | `03 §9.3.7` |
| 3.4.17 | E-49 | Davet · geri çekme · kaldırma | E-49'da kalır | — | `03 §8.8.1`, §8.8.3, §8.8.5 |
| 3.4.18 | E-30 | Hesabın açılması | E-29, davetin adresi ön dolu | Geçerli davet | `03 §8.8.2` · K-807 |
| 3.4.19 | E-33 — stok ya da kontenjan alanı | Ayrılmış adedi tutan sipariş | E-37 | Ödemesi beklenen sipariş varken | `02 §3.6.7` · `03 §8.1.3` |
| 3.4.20 | E-33 · E-37 | "Listeye dön" | E-32 · E-36 — bırakılan sayfa, arama ve süzgeçle | — | K-766, K-802 |
| 3.4.21 | E-39 | "Siparişlerde ara" · "Üye kaydında ara" | E-36 · E-40 — arama talebin e-postasıyla yapılmış | — | `03 §8.6.4.2`, §8.6.4.3 · K-810 |
| 3.4.22 | E-47 — aydınlatma metninin tamamlanma satırı | Eksik firma kimliği alanının bağlantısı | E-44, kimlik formu | Aydınlatmanın yer tutucusunu besleyen kimlik alanı boşken | `02 §3.33.2` · K-818 |

*Kaynak: `02 §3.6.7`, §3.33.2, §6.1.1, §10.6.1, §10.8.2 · `03 §1.11.15`–§1.11.41, §8.1.1, §8.1.3, §8.1.7, §8.2–§8.4, §8.6.1, §8.6.4.2, §8.6.4.3, §8.7.1.1, §8.8, §8.9.1, §9.2.7, §9.3.7, §9.4.5 · K-741, K-744, K-748…K-752, K-760, K-761, K-766, K-769, K-802, K-807, K-810, K-818 · devir: K-653.*

### 3.5 Sitenin dışından girişler

Kullanıcının bir e-postadaki bağlantıyla, bir dış sayfadan dönerek ya da adresi doğrudan açarak geldiği girişler. E-postanın metni ve bağlantının biçimi Entegrasyon Spesifikasyonu'nun, adreslerin biçimi API Tasarımı'nın işidir; burada yalnız inilen ekran yazılır.

| # | Nereden | Tetikleyici | Nereye | Koşul | Kaynak |
|---|---|---|---|---|---|
| 3.5.1 | Sipariş e-postası | Sipariş sayfasının bağlantısı (erişim anahtarı) | E-16, e-posta sormadan | Siparişin kişisel verileri imha edilmemiş | `02 §3.22.5` · `03 §2.6.3` |
| 3.5.2 | Sipariş e-postası | Dijital ürünün indirme bağlantısı | E-16 — aynı kapı | 3.5.1 | `02 §3.22.3` · `03 §2.6.3` |
| 3.5.3 | B-14 "IBAN gerekli" e-postası | Sipariş sayfasının bağlantısı | E-16; kalemin IBAN alanı açıktır (2.12.3.2) | IBAN isteği sürüyor | `03 §7.1.29`–§7.1.34 |
| 3.5.4 | E-posta doğrulama bağlantısı (kayıt) | Bağlantı | E-21 | Geçerliyse hesap doğrulanır; süresi dolmuşsa ekran bunu söyler | `03 §9.1.4`, §9.1.5 |
| 3.5.5 | Şifre sıfırlama bağlantısı | Bağlantı | Müşteri hesabında E-23 · yönetici hesabında E-29 | Bağlantı tek kullanımlık ve ömrü içinde | `03 §9.2.4`, §9.2.5, §9.2.8 |
| 3.5.6 | Yeni e-posta adresinin doğrulama bağlantısı | Bağlantı | Müşteri hesabında E-21 · yönetici hesabında E-54 | Bağlantı ömrü içinde | `03 §9.3.5`, §9.3.7 |
| 3.5.7 | "Bu değişikliği ben yapmadım" bağlantısı | Bağlantı | Müşteri hesabında E-28 · yönetici hesabında E-54 | Z-45 içinde | `03 §9.3.6`, §9.3.7 |
| 3.5.8 | Ödeme sağlayıcısının sayfası | 3D Secure'dan dönüş | E-16; sonuç gelmemişse sayfa ödemenin beklendiğini söyler | Kart ödemesi | `03 §2.5.1.2`, §2.5.1.3 |
| 3.5.9 | Google'ın sayfası | Girişten dönüş | §3.6'nın dönüş yeri; Google e-postayı doğrulanmamış verdiyse E-20 | — | `03 §9.1.6`, §9.1.7, §9.2.2 |
| 3.5.10 | Yönetici daveti e-postası | Davet bağlantısı | E-30; bağlantı kullanılmış, süresi dolmuş ya da geçersizleşmişse ekran süresi dolmuş davetin mesajını gösterir | — | `03 §8.8.2` |
| 3.5.11 | Paylaşılmış ya da arama motorundan gelen ürün adresi | Adres | Yayında: E-04 · Arşiv: E-05 · Taslak ya da silinmiş: E-06 — yöneticiye taslak, bantla (§2.9) | — | `03 §2.1.10`, §2.1.11 |
| 3.5.12 | Paylaşılmış içerik adresi | Adres | Yayında: E-08 · Taslak ya da silinmiş: E-06 — yöneticiye taslak, bantla | — | `03 §2.2.10` |
| 3.5.13 | Var olmayan adres | Adres | E-06 | — | `02 §3.30.4` |
| 3.5.14 | Oturum gerektiren ekranın adresi | Adres | Vitrinde E-22, panelde E-29 → istenen ekran (§3.6.1) | Oturum yok | K-768 |

Firmaya ve yöneticilere giden bildirimlerin (F-1…F-6) panelde indiği ekran bu tabloda yoktur: bildirimin taşıdığı bağlantı Entegrasyon Spesifikasyonu'nda yazılır; bağlantı bir panel ekranına iniyorsa 3.5.14 işler.

*Kaynak: `02 §3.22.3`, §3.22.5, §3.30.4 · `03 §2.1.10`, §2.1.11, §2.2.10, §2.5.1.2, §2.5.1.3, §2.6.3, §7.1.29–§7.1.34, §8.8.2, §9.1.4–§9.1.7, §9.2.2, §9.2.4, §9.2.5, §9.2.8, §9.3.5–§9.3.7 · K-768.*

### 3.6 Girişten ve çıkıştan dönüş

**3.6.1 Oturum gerektiren bir ekrana oturumsuz gelen kullanıcı kendi kapısının giriş ekranına götürülür ve girişten sonra istediği ekrana iner** (K-768): hesap ekranlarında (E-25, E-26, E-27) vitrin girişi (E-22), panel ekranlarında panel girişi (E-29). İki kapı ayrıdır ve birbirinin hesabını aramaz (`02 §10.2.5`). Panel girişini doğrudan açan yönetici E-31'e iner.

**3.6.2 Ödeme adımından girişe giden müşteri girişten sonra ödeme adımına döner ve yazdıkları durur** (K-758). Müşteri ödeme adımını üye olarak görür: iletişim e-postası hesabın e-postasıdır; sepetler birleşmiştir — birleşme bir şey eklediyse bu ekranın başında bir kez söylenir (2.3.1.3) ve özet yeniden hesaplanır —; misafirken yazdığı teslimat ve fatura adresi formda yeni adres olarak durur ve adres defteri seçenek olarak yanına gelir; girilmiş kupon kodu yeniden uygulanır, geçersizse söylenir. Girişten vazgeçen müşteri ödeme adımına yazdıklarıyla döner. Yazılanlar yalnız o tarayıcıda ve o ziyaret boyunca korunur ve sipariş onaylanmadan bir kayıt oluşturmaz.

**3.6.3 Giriş ekranını çerçeveden kendisi açan müşteri girişten sonra geldiği vitrin sayfasına döner** (K-768); geldiği sayfa bir giriş, kayıt, sıfırlama ya da bağlantı iniş ekranıysa ana sayfaya iner. Google ile giriş aynı dönüş yerini kullanır (3.5.9). Giriş yerine kayda ya da şifre sıfırlamaya sapan kullanıcıya dönüş sözü verilmez: bu yollar e-postadaki bağlantıyla sürer ve giriş ekranında biter (3.3.29); sepeti durur (K-758).

**3.6.4 Çıkış.** Çıkış yapan müşteri bulunduğu sayfa oturum gerektirmiyorsa o sayfada kalır, gerektiriyorsa ana sayfaya iner; sepeti hesapta kalır ve o tarayıcıda sepet boş görünür (`03 §2.3.7`). Çıkış yapan yönetici panel girişine iner (K-768).

**3.6.5 Kendiliğinden kapanan oturum.** Oturumu kapanan kullanıcı oturum gerektiren bir işlem yaptığında ekran oturumun kapandığını söyler ve girişe götürür; dönüş 3.6.1'dir. Ödeme adımında kapanan oturumda dönüş 3.6.2'dir: müşteri ödeme adımına döner, sepeti hesabındadır ve yazmakta olduğu yeni adres durur (K-758). Ödeme sağlayıcısının sayfasındayken kapanan oturum siparişi etkilemez.

*Kaynak: `02 §3.16.4`, §10.2.5 · `03 §2.3.6`, §2.3.7, §2.4.1, §9.2.1, §9.2.3, §9.2.7 · K-758, K-768 · §1.3 GAP-4 · devir: K-99.*

### 3.7 Panel ile vitrin arasındaki geçişler

| # | Nereden | Tetikleyici | Nereye | Koşul | Kaynak |
|---|---|---|---|---|---|
| 3.7.1 | Panelin her ekranı — üst satır | "Siteyi görüntüle" | E-01; sayfanın başında yönetici şeridi | Panel oturumu açık | K-748, K-749 |
| 3.7.2 | E-33 · E-41 kayıt formu | "Önizle" (taslak kayıt) ya da "Sitede gör" (yayındaki kayıt) | Kaydın vitrindeki sayfası; taslakta "Taslak" bandıyla | Kayıt kaydedilmiş | `03 §8.1.7`, §8.6.1.3 · K-748, K-749 |
| 3.7.3 | Vitrinin her sayfası — yönetici şeridi | "Panele dön" | E-31 | Panel oturumu açık | K-748 |
| 3.7.4 | Önizlenen taslak kaydın sayfası — "Taslak" bandı | "Düzenlemeye dön" | Kaydın panel formu — E-33 ya da E-41 | Panel oturumu açık | K-748 |
| 3.7.5 | Vitrin — yönetici oturumu kapanmışken taslak adresi | Adres | E-06 | Panel oturumu yok | `02 §3.27.25` · K-748 |

Yönetici oturumu vitrinin hesap alanını ve sepetini değiştirmez (2.9.4): yönetici şeridi açıkken hesap alanının geçişleri §3.1'deki gibi, o tarayıcıdaki müşteri oturumuna göre işler.

*Kaynak: `02 §3.27.25`, §10.2.5 · `03 §8.1.7`, §8.6.1.3 · K-748, K-749 · §1.3 GAP-6.*

## 4. Ekran envanteri

**Yazım turunun 1. oturumu (2026-10-04, v0.6; K-764).** Envanter matrisin aday listesinden (§1.1.2) doğdu: 54 aday kimliğin 53'ü ekrandır, biri düşmüştür. **Müşteri tarafı 28 ekran (E-01…E-28), panel 25 ekran (E-29…E-47, E-49…E-54).** Kimlikler yeniden numaralanmadı (konvansiyon 2); aday listeden her fark §4.3'te gerekçesiyle durur. "Tanım" sütunu ekranın yazılacağı alt bölümdür (başlık notunun alt bölüm haritası; K-763). Aktör adları konvansiyon 7'nin adlarıdır.

### 4.1 Müşteri tarafının ekranları

| # | Ekran | Aktör(ler) | Amaç | Tanım |
|---|---|---|---|---|
| E-01 | Ana sayfa | Ziyaretçi · Müşteri | Firmanın seçtiği düzende kurumsal tanıtımı ve ürün vitrinini birlikte gösterir. | §5.1 |
| E-02 | Kategori sayfası | Ziyaretçi · Müşteri | Bir kategorinin ve alt dallarının ürünlerini tek listede gösterir ve süzdürür. | §5.2 |
| E-03 | Arama sonuçları | Ziyaretçi · Müşteri | Ürün adında yapılan aramanın sonucunu kategori sayfasının düzeniyle gösterir. | §5.3 |
| E-04 | Ürün sayfası | Ziyaretçi · Müşteri | Ürünün bilgisini gösterir, varyantı seçtirir ve sepete ekletir. | §5.4 |
| E-05 | Arşivlenmiş ürün sayfası | Ziyaretçi · Müşteri | "Bu ürün artık satılmıyor" bilgisini fiyatsız gösterir. | §5.5 |
| E-06 | "Sayfa bulunamadı" sayfası | Ziyaretçi · Müşteri | Var olmayan ya da ziyaretçiye kapalı adreste dönüş yollarını gösterir. | §5.6 |
| E-07 | İçerik liste sayfası | Ziyaretçi · Müşteri | Hizmet tanıtımlarını ya da referans işleri firmanın sırasıyla listeler. | §5.7 |
| E-08 | İçerik sayfası | Ziyaretçi · Müşteri | Hakkımızda'yı, bir hizmet tanıtımını, bir referans işi ya da bir genel sayfayı gösterir. | §5.8 |
| E-09 | Sık sorulan sorular sayfası | Ziyaretçi · Müşteri | Bütün soruları ve cevapları tek sayfada listeler. | §5.9 |
| E-10 | İletişim sayfası ve formu | Ziyaretçi · Müşteri | Firmanın iletişim ve yasal kimlik bilgilerini, şubeleri ve iletişim formunu — gönderim sonrası hâliyle — taşır. | §5.10 |
| E-11 | Yasal metin sayfası | Ziyaretçi · Müşteri | Aydınlatma metnini, çerez politikasını ya da "İşlem rehberi"ni gösterir. | §5.11 |
| E-12 | Sepet | Ziyaretçi · Müşteri | Kalemleri güncel fiyat ve satın alınabilirlikle gösterir ve ödeme adımına geçirir. | §5.12 |
| E-13 | Ödeme adımı | Müşteri | Tek ekranda e-postayı, adresleri, kuponu ve ödeme yöntemini alır; onay özetini, iki yasal metni ve onay kutularını gösterir ve siparişi onaylatır. | §5.13 |
| E-14 | Sipariş teyit ve bekleme ekranı | Müşteri | Siparişin alındığını, numarasını, havalede IBAN'ı ve kartta ödemenin beklendiğini gösterir. | §5.14 |
| E-15 | Sipariş takibi girişi | Misafir alıcı | Sipariş numarası ve e-postayla sipariş sayfasını açtırır. | §5.15 |
| E-16 | Sipariş sayfası | Müşteri | Siparişin durumunu ve bütün bilgisini gösterir; o anda açık işlemleri sunar. | §5.16 |
| E-17 | İptal ve gecikme feshi ekranı | Müşteri | Siparişi, kalemi ya da kalemin bir kısmını (adet) iptal ettirir ya da gecikme nedeniyle feshettirir; havalede IBAN'ı alır; son adımda onaylatır. | §5.17 |
| E-18 | Cayma beyanı ekranı | Müşteri | Kalemi ve adedini seçtirir, iade adresini ve yükümlülüğü gösterir, beyanı ve havalede IBAN'ı alır; son adımda onaylatır. | §5.18 |
| E-19 | Ayıp talebi ekranı | Müşteri | Kalemi ve ayıplı adedi seçtirir ve sorunun açıklamasını alır; talebi yeniden açtırır. | §5.19 |
| E-20 | Kayıt ekranı | Ziyaretçi | Ad, e-posta ve şifreyle hesap açtırır; doğrulama bağlantısını yeniden istetir. | §5.20 |
| E-21 | Doğrulama bağlantısının iniş ekranı | Kullanıcı | Kayıt ya da yeni e-posta doğrulamasının sonucunu — geçersiz bağlantı dahil — söyler. | §5.21 |
| E-22 | Müşteri girişi | Ziyaretçi · Üye | E-posta ve şifreyle ya da Google ile giriş yaptırır. | §5.22 |
| E-23 | Şifre sıfırlama ekranları | Kullanıcı | Sıfırlama bağlantısını istetir ve yeni şifreyi kurdurur. | §5.23 |
| E-24 | Yeniden doğrulama ekranı | Üye | Şifre, e-posta değişikliği ve hesap silmeden önce kimliği yeniden doğrular. | §5.24 |
| E-25 | Hesap — profil ve güvenlik | Üye | Adı, e-postayı ve şifreyi değiştirtir; bekleyen e-posta değişikliğini gösterir; hesabı sildirir. | §5.25 |
| E-26 | Adres defteri | Üye | Kayıtlı adresleri ekletir, düzenletir ve sildirir. | §5.26 |
| E-27 | Sipariş geçmişi | Üye | Hesaba bağlı siparişleri listeler ve sipariş sayfasına götürür. | §5.27 |
| E-28 | E-posta değişikliğini geri alma ekranı | Kullanıcı | "Bu değişikliği ben yapmadım" bağlantısının sonucunu söyler ve şifrenin yeniden belirlenmesine götürür. | §5.28 |

### 4.2 Panelin ekranları

Panel ekranlarının aktörü Yönetici'dir; yönetim tarafı tek roldür (`02 §1.3`). Oturumdan önce açılan ekranlarda aktör Kullanıcı'dır. "Menü" sütunu ekranın panel menüsündeki yeridir (§3.2).

| # | Ekran | Aktör(ler) | Amaç | Menü | Tanım |
|---|---|---|---|---|---|
| E-29 | Panel girişi ve şifre sıfırlama | Yönetici · Kullanıcı | Yöneticiyi şifreyle panele alır; panel şifresini sıfırlatır. | — (oturumdan önce) | §9.1 |
| E-30 | Yönetici daveti kabul ekranı | Kullanıcı | Davet bağlantısından, ad ve şifreyle yönetici hesabını açtırır. | — (oturumdan önce) | §9.2 |
| E-31 | Panel ana sayfası | Yönetici | Uyarıları, kurulum kontrol listesini, bekleyen işleri sayaçlarla ve "IBAN bekleniyor" listesini gösterir. | Ana sayfa | §9.3 |
| E-32 | Ürün listesi | Yönetici | Ürünleri yayın durumlarıyla listeler, adla aratır ve yayın durumuna göre süzdürür. | Katalog › Ürünler | §9.4 |
| E-33 | Ürün formu | Yönetici | Ürünü, varyantlarını, stoğunu, görsellerini, indirimini, dijital dosyasını ve cayma istisnasını düzenletir. | Katalog › Ürünler | §9.5 |
| E-34 | Kategori ağacı | Yönetici | Kategori ağacını kurdurur ve dalları taşıtır. | Katalog › Kategoriler | §9.6 |
| E-35 | Kuponlar | Yönetici | Kuponları listeler ve düzenletir. | Katalog › Kuponlar | §9.7 |
| E-36 | Sipariş listesi | Yönetici | Siparişleri aratır ve durumuna göre süzdürür; sayaçların süzülmüş listesidir. | Siparişler | §9.8 |
| E-37 | Sipariş ayrıntısı | Yönetici | Siparişin bütün bilgisini gösterir; yürütüm işlemlerini ve müdahaleleri yaptırır; siparişin ayıp talebini okutur, çözüldü işaretletir ve yeniden açtırır. | Siparişler | §9.9 |
| E-38 | Talepler | Yönetici | İletişim ve ayıp taleplerini tür sütunuyla tek listede gösterir; türe ve duruma göre süzdürür. | Talepler | §9.10 |
| E-39 | İletişim talebi ayrıntısı | Yönetici | Bir iletişim talebini okutur, kapattırır ve yeniden açtırır. | Talepler | §9.11 |
| E-40 | Üye kaydı görünümü | Yönetici | Bir üyeyi e-postayla aratır, kaydını salt okunur gösterir ve talep üzerine sildirir. | Üyeler | §9.12 |
| E-41 | Kurumsal içerik | Yönetici | İçerik tiplerinin, genel sayfaların ve duyurunun kayıtlarını listeler ve düzenletir. | İçerik ve site › Kurumsal içerik | §9.13 |
| E-42 | Ana sayfa ve menü | Yönetici | Ana sayfa düzenini seçtirir, menü adlarını ve sosyal bağlantıları düzenletir. | İçerik ve site › Ana sayfa ve menü | §9.14 |
| E-43 | Marka | Yönetici | Logoyu, marka adını, site simgesini, marka rengini ve platform imzasını ayarlatır. | İçerik ve site › Marka | §9.15 |
| E-44 | Firma kimliği ve satış | Yönetici | Satış durumunu ve kapının koşullarını gösterir, satışı geçici olarak kapattırır; firma tipine göre yasal kimlik ve iletişim bilgilerini girdirir. | Ayarlar › Firma kimliği ve satış | §9.16 |
| E-45 | Ödeme yöntemleri | Yönetici | Havaleyi açtırır, kapattırır ve IBAN'ı girdirir. | Ayarlar › Ödeme yöntemleri | §9.17 |
| E-46 | Kargo, süreler ve varsayılanlar | Yönetici | Kargo ücretini ve eşikleri, teslimat illerini, süreleri, ürün varsayılanlarını ve iade adresini düzenletir. | Ayarlar › Kargo, süreler ve varsayılanlar | §9.18 |
| E-47 | Yasal metinler | Yönetici | Aydınlatma metnini ve çerez politikasını düzenletip yayımlatır; üretilen iki metni gösterir. | Ayarlar › Yasal metinler | §9.19 |
| E-49 | Yönetici hesapları | Yönetici | Yöneticileri listeler; davet ettirir, daveti geri çektirir ve yönetici kaldırtır. | Ayarlar › Yönetici hesapları | §9.20 |
| E-50 | Yöneticinin kendi hesabı | Yönetici | Yöneticinin kendi adını, e-postasını ve şifresini değiştirtir. | Üst satır › "Hesabım" | §9.21 |
| E-51 | Satış özeti | Yönetici | Seçilen dönemin satış ölçülerini gösterir. | Raporlar › Satış özeti | §9.22 |
| E-52 | İşlem izi | Yönetici | Yönetici işlemlerinin izini tarih ve yönetici süzgeciyle okutur. | Raporlar › İşlem izi | §9.23 |
| E-53 | Dışa aktarma | Yönetici | Siparişleri, üye listesini ve iletişim taleplerini dosya olarak dışa aktartır. | Raporlar › Dışa aktarma | §9.24 |
| E-54 | Yönetici hesabının doğrulama ekranları | Yönetici · Kullanıcı | Yöneticinin kimliğini şifre ve e-posta değişikliğinden önce yeniden doğrular; yeni adresin doğrulama bağlantısının ve "bu değişikliği ben yapmadım" bağlantısının sonucunu söyler. | — (E-50'den ve bağlantıdan) | §9.25 |

### 4.3 Aday listeden envantere farklar ve düşen kimlik

Aday listenin 54 kimliğinden envantere geçişte değişen her şey bu tablodadır (K-764); tabloda olmayan aday adı, aktörü ve amacıyla aynen geçti. Düşen kimlik yeniden verilmez.

| Aday (§1.1.2) | Envanterde | Gerekçe |
|---|---|---|
| E-48 Satışın geçici kapatılması | **Düştü → E-44.** Anahtar "Firma kimliği ve satış" ekranının başındaki "Satış durumu" bölümündedir | Ekran değil, ayardır: kaynak onu "anahtar" diye adlandırır ve `02 §10.1.2` "Firma kimliği ve satış" alanında sayar; tek anahtar için ayrı ekran bir menü girişi açardı (K-738, K-752). Sekiz matris satırı E-44'e taşındı (§1.2) |
| E-44 Firma kimliği | Ad: **Firma kimliği ve satış**; amaç satış durumunu ve geçici kapatma anahtarını da taşır | E-48'in düştüğü yer; ad `02 §10.1.2`'nin alan adıdır (K-752) |
| E-38 Ayıp talepleri | Ad: **Talepler**; iletişim ve ayıp taleplerinin tek listesi | "Açık talep" sayacı tektir ve iki türü birlikte sayar; iki liste ya da iki sekme sayaç ile listeyi ayrıştırırdı (K-751). Listenin iletişim talebi yüzünü taşıyan altı matris satırı E-38'i de aldı (§1.2) |
| E-39 İletişim talepleri | Ad: **İletişim talebi ayrıntısı**; liste E-38'e geçti, ekran tek talebin okunması, kapatılması ve yeniden açılmasıdır | "Tek liste, iki ayrıntı" (K-751): liste birleşince E-39'un kalan işi ayrıntıdır |
| — (ayıp talebinin ayrıntısı) | **Yeni kimlik açılmadı;** ayrıntı E-37'nin siparişe bağlı ayıp talebi bölümüdür | Ayıp talebi kalem ve sipariş bağlamıyla panele düşer ve işlemleri — çözüldü işareti, yeniden açma, para gerektiren çözüm — siparişte yürür (`03 §8.4.10`, §8.4.11); ayrı bir ekran aynı kalemi iki yerde gösterirdi (K-751, K-764) |
| E-42 Ana sayfa, menü ve sosyal bağlantılar | Ad: **Ana sayfa ve menü** | Panel menüsündeki adıdır (K-749); kapsamı değişmedi — sosyal bağlantılar bu ekranda kalır |
| E-46 Kargo, teslimat, süreler, eşikler ve iade adresi | Ad: **Kargo, süreler ve varsayılanlar**; indirme hakkı ile KDV oranının varsayılanı da buradadır | Beş bölümlü ekran ve iki varsayılanın yeri (K-752; §1.3 GAP-11) |
| E-54 Yönetici hesabının doğrulama ekranları | **Kalır, tek kimlik;** üç adımı vardır — yeniden doğrulama, yeni adresin doğrulamasının iniş ekranı, geri alma ekranı | Müşteri tarafındaki E-21, E-24 ve E-28 ile birleşmez: iki hesap türü ayrı kapıdan girer ve panelde Google ile yeniden doğrulama yoktur (`02 §10.2.5`; K-738). Üç kimliğe bölünmedi: matrisin beş satırı tek kimliğe bağlıdır ve üç adım aynı kalıbı paylaşır — kalıp müşteri tarafında bir kez tanımlanır (§5.21, §5.24, §5.28), §9.25 farkları yazar (K-764) |
| E-50 Yöneticinin kendi hesabı | Kalır; amaç yöneticinin adını da taşır | Yönetici adını kendi hesabından değiştirir (K-736). E-54 ile birleşmedi: E-50 oturum içindeki hesap ekranıdır, E-54 doğrulama adımlarıdır |
| E-30 Yönetici daveti kabul ekranı | Kalır; amaç adı da taşır | Davetli hesabını açarken adını yazar (K-736) |
| E-31 · E-32 · E-13 · E-17 · E-18 · E-25 · E-28 · E-36 · E-37 | Kalır; amaç cümlesi matrisin ve workshop'un kararlarıyla hizalandı | E-31 dört bölüm (K-750) · E-32 arama ve süzgeç (K-737) · E-13 tek ekran (K-756) · E-17, E-18 son adımda onay (K-732) · E-25 bekleyen e-posta değişikliği (K-734) · E-28 şifre geri alma ekranında değil, bağlantıyla kurulur (`03 §9.3.6`) · E-36 sayaçların süzülmüş listesi (`02 §10.6.1`) · E-37 ayıp talebi bölümü (K-751) |
| E-05 · E-09 · E-26 | Kalır | Az satırlı adaylardır; üçünün de kendi kuralı vardır (§1.1.2; K-738) |

**Sayım.** 54 aday kimlik − 1 düşen (E-48) = **53 ekran:** müşteri tarafı 28, panel 25. Panel ekranları toplamın yarısına yakındır ve envanterde, navigasyonda, durum × rol matrisinde ve form envanterinde müşteri tarafıyla aynı derinlikte yer alır (K-724).

### 4.4 Ekran olmayan yüzeyler

Kullanıcının karşısına çıkan ama bu dokümanda ekran olarak tanımlanmayan yüzeyler. Her biri adıyla ve evine işaretle yazılır; boş bırakılmaz (karar kaydı §10.3 UI4-03).

| # | Yüzey | Neden ekran değildir | Evi |
|---|---|---|---|
| 4.4.1 | E-postalar — B-1…B-16, F-1…F-6, hesap ve yönetim e-postaları | Ürünün ekranı değil, gönderdiği iletidir; bu dokümanda yalnız ekranda bıraktığı iz yazılır (§3.5, 2.5.3) | Metin ve şablon: Entegrasyon Spesifikasyonu (`08`) · içerik kuralı: `02 §9` · tetikleyiciler: `03 §7` |
| 4.4.2 | Ödeme sağlayıcısının sayfası ve 3D Secure ekranı | Ürün göstermez; kart bilgisi sağlayıcıda girilir (`03 §2.5.1.1`) | Ürünün dışında · entegrasyon: `08` |
| 4.4.3 | Google'ın giriş sayfası | Ürün göstermez | Ürünün dışında · entegrasyon: `08` |
| 4.4.4 | ETBİS doğrulama sayfası · sosyal medya ve WhatsApp sayfaları · kargo şirketinin takip sayfası · video platformu · şubenin harita bağlantısı | Bağlantının götürdüğü dış sayfalardır | Ürünün dışında |
| 4.4.5 | Arama motorlarına verilen veriler, site haritası ve paylaşım önizlemesi | Ziyaretçinin gördüğü bir ekran değil, sayfanın taşıdığı veridir (`02 §3.30.5`; `10 §2` KP-37) | Teknik Mimari (`05`) |
| 4.4.6 | Tarayıcı sekmesinin başlığı ve site simgesi | Tarayıcının yüzeyidir; kuralı `02 §3.29.3`, §3.29.4 ve §3.30.5'tedir | `02` — burada yeniden yazılmaz |
| 4.4.7 | Cihazın paylaşım menüsü | "Paylaş" düğmesinin açtığı, cihaza ait menüdür (`02 §3.30.7`) | Ürünün dışında |
| 4.4.8 | Dışa aktarılan dosyalar | Ekran değil dosyadır; taşıdığı alanlar `02 §10.7`'dedir | `02 §10.7` |
| 4.4.9 | Onay penceresi, hata sayfası ve bekleme göstergesi | Kendi ekran kimliği olmayan kalıplardır: onay OB-04, sayfa düzeyindeki beklenmeyen hata ve bekleme OB-07'dir | §2.4, §2.7 |
| 4.4.10 | Kesinti ve bakım ekranı | Yoktur: kesintide ürünün içinden bilgilendirme ve bakım modu bulunmaz (`03 §10.3.1`; `02 §3.1.6`) | — |
| 4.4.11 | Bekleyen işler, toplu işlem ve kurulum sihirbazı ekranı | Yoktur: sayaçlar var olan listelerin süzülmüş hâline götürür (`02 §10.6.1`), toplu işlem yoktur (`02 §10.7.2`), sihirbaz yoktur (`02 §10.8.2`) | — |
| 4.4.12 | Fatura, banka havalesi ve kargo teslimi | Sistem dışı adımlardır; ürün izlemez (`03 §8.5.8`) | Ürünün dışında |

*Kaynak: `02 §1.3`, §3.1.6, §3.29.3, §3.29.4, §3.30.5, §3.30.7, §9, §10.1.2, §10.2.5, §10.6.1, §10.7, §10.8.2 · `03 §0.1.3`, §0.1.4, §2.5.1.1, §7, §8.4.10, §8.4.11, §8.5.8, §9.3.6, §10.3.1 · `10 §2` KP-15, KP-37 · K-724, K-725, K-729, K-732, K-734, K-736, K-737, K-738, K-749, K-750, K-751, K-752, K-756, K-763, K-764.*

## 5. Ekran tanımları

**Yazım turunun 2a oturumu (2026-10-04, v0.7).** Müşteri tarafının ilk on beş ekranı (E-01…E-15) — vitrin, kurumsal içerik, sepet, ödeme adımı, sipariş teyit ekranı ve sipariş takibi girişi — başlık notunun sekiz alanlı şablonuyla yazıldı (konvansiyon 4). Her ekranın maddeleri alanlardan bağımsız tek sırayla numaralanır (`04 §5.n.k`; konvansiyon 5): ilk madde aktörü ve §3'ün giriş-çıkış satırlarını, ikinci madde ilk görüleni taşır. "Durum × rol varyantları" alanı ekranın durumla ve aktörle değişen parçalarını yazar; tam matris §6.2'dedir ve 5. oturumda kurulur (konvansiyon 13). Ekranın kaynak satırları §1.2'nin ikinci betiğiyle listelendi ve Kullanıcı Akışları'nda, Ürün Gereksinimleri'nde ve MVP Kapsamı'nda tam metinle okundu; matrisin bu okumada düzelen satırları §1.2'nin notundadır. Yazımın bulduğu boşluklar §1.3'te ve karar kaydında K-770…K-778'dir. **2b oturumu (2026-10-04, v0.8)** kalan on üç ekranı (E-16…E-28) — sipariş sayfası, sipariş sayfasından açılan üç işlemin ekranları, üyelik ve hesap ekranları — aynı biçimle yazdı ve §5 tamamlandı; şablonun `S<NN>` bloğu kaldırıldı (konvansiyon 3). Müşterinin geri alınamaz dört işleminin son adımı OB-04'ün müşteri varyantıdır ve ekranların içindedir (E-17, E-18, E-25); düğme metinleri ve sonuç cümleleri K-785'tedir. 2b'nin bulduğu boşluklar K-779…K-786'dır (§1.3).

### 5.1 E-01 — Ana sayfa

- **5.1.1 Aktör · giriş · çıkış.** Ziyaretçi · Müşteri. Giriş: logo (3.1.2, 3.1.10), dönüş bağlantıları (3.3.12), "Siteyi görüntüle" (3.7.1), girişten ve çıkıştan dönüş (3.6.3, 3.6.4). Çıkış: blokların geçişleri 3.3.1–3.3.4 ve çerçevenin geçişleri (§3.1).
- **5.1.2 İlk görülen:** firmanın seçtiği düzende ilk blok — tanıtım öncelikli düzende kurumsal blok, mağaza öncelikli düzende ürün vitrini. Ekranın birincil düğmesi yoktur; her blok kendi bağlantısını taşır.
- **Bilgi hiyerarşisi** — çerçevenin (OB-02) içinde dört blok, sırası düzene göre (K-753):
  - **5.1.3 Tanıtım öncelikli:** (1) kurumsal blok, geniş hâliyle — Hakkımızda'nın kısa tanıtımı, ana görseli ve Hakkımızda sayfasının bağlantısı · (2) Hizmetlerimiz · (3) Referanslarımız · (4) ürün vitrini, tek satır.
  - **5.1.4 Mağaza öncelikli:** (1) ürün vitrini, iki satır · (2) kurumsal blok, dar hâliyle — kısa tanıtım ile ana görsel yan yana, tek bant · (3) Hizmetlerimiz · (4) Referanslarımız.
  - **5.1.5 Blok başlığı** menüdeki addır — firmanın değiştirdiği ad dahil — ve kendi liste sayfasına götüren bağlantıyı taşır (K-753). Hizmetlerimiz ve Referanslarımız en çok üçer kayıt gösterir: "ana sayfada göster" işaretli kayıtların firmanın elle sırasındaki ilk üçü, içerik kartıyla (OB-11, §2.11.4); fazlası "Tümünü gör" ile liste sayfasındadır (K-755; `02 §3.28.3`).
  - **5.1.6 Ürün vitrini** yayındaki en yeni ürünleri ürün kartıyla gösterir — mağaza öncelikli düzende en çok sekiz, tanıtım öncelikli düzende en çok dört; firma ürün seçmez (`02 §3.28.1`; K-754, K-755). Sırası gelen tükenmiş ürün "Tükendi" işaretiyle görünür (§2.11.3). Bloğun altında yayında ürünü olan birinci seviye kategorilerin bağlantıları, adlarının alfabetik sırasıyla durur (K-805); ayrı bir "tüm ürünler" sayfası yoktur (K-754).
  - **5.1.7 Yoktur:** kayan afiş, kampanya görseli, alıntı bloğu ve kategori vitrini (`02 §3.27.19`, §3.27.21); SSS, şube ve genel sayfa ana sayfaya çıkmaz (`02 §3.28.3`). Duyuru şeridi ve altbilgi çerçevenindir, blok değildir.
- **Aksiyonlar**
  - **5.1.8** Ürün kartı → E-04 (3.3.1; `03 §2.1.5`) · kategori bağlantısı → E-02 (3.3.2) · Hakkımızda bağlantısı → E-08 (3.3.3) · içerik kartı → E-08, "Tümünü gör" → E-07 (3.3.4; `03 §2.2.1`). Hiçbiri onay istemez; ekranda mesaj yoktur.
- **5.1.9 Validasyonlar:** form yoktur.
- **5.1.10 Durum × rol varyantları.** Aktörle değişen parça yoktur; giriş yapmış yöneticiye sayfanın başında yönetici şeridi görünür (§2.9.1). Satış kapalıyken ürün vitrini görünür kalır (§2.8.1; K-754). Tam matris §6.2'dedir (5. oturum).
- **Boş / yükleniyor / hata durumları** (OB-07)
  - **5.1.11 Görünmez bloklar** (§2.7.1.1): işaretli kaydı olmayan Hizmetlerimiz ya da Referanslarımız bloğu ve yayında ürünü yokken ürün vitrini — bağlantılarıyla — hiç görünmez; tek kayıtlı blok tek kartla görünür, boşluk doldurulmaz (K-755). Hakkımızda boşken ya da yayında değilken kurumsal blok marka adını ve logoyu gösterir; boş kutu görünmez (`02 §3.27.14`; `03 §3.5.3.3`). Hiç içeriği ve ürünü olmayan yeni sitede ana sayfa yalnız bu geri düşüşten oluşur (`02 §3.27.22`; K-755).
  - **5.1.12** Bloklar kendi yerlerinde yüklenir (§2.7.2); sayfa açılamazsa sayfa düzeyindeki hata hâli işler (§2.7.3.2).
- **5.1.13 Responsive notları.** Dar sınıfta bloklar aynı sırayla alt alta gelir; kurumsal bloğun dar hâli de alt alta dizilir (K-753). Ürün vitrininin kart sayısı sınıfın sütun sayısıyla (§2.11.5) satır doldurur: orta sınıfta son satırı doldurmayan kartlar gösterilmez — vitrin eksik satır göstermez (K-755). Dar sınıfın iki sütununda dört ve sekiz kart tam satır kurar.

*Kaynak: `02 §3.27.14`, §3.27.19, §3.27.21, §3.27.22, §3.28.1–§3.28.3, §3.29.2, §6.8.3 · `03 §2.1.1`, §2.2.1, §3.5.3.3 · `10 §2` KP-31 · park satırı K-27 · K-753, K-754, K-755, K-805 · devir: K-246.*

### 5.2 E-02 — Kategori sayfası

- **5.2.1 Aktör · giriş · çıkış.** Ziyaretçi · Müşteri. Giriş: menüdeki kategori (3.1.11), ana sayfanın kategori bağlantısı (3.3.2), kırıntı yolu (3.3.6). Çıkış: ürün kartı → E-04 (3.3.5), kırıntı yolu → üst kategori (3.3.6).
- **5.2.2 İlk görülen:** kategorinin adı ve ürün ızgarası; ekranın birincil düğmesi yoktur.
- **Bilgi hiyerarşisi**
  - **5.2.3** (1) Kırıntı yolu — kategorinin ağaçtaki yolu · (2) kategorinin adı · (3) süzgeçler — fiyat aralığı ve stok durumu (`02 §3.5.2`) · (4) ürün ızgarası (OB-20, OB-11): kategoriye doğrudan asılı ürünler ve bütün alt dallarındakiler tek listede, en yeni önce (`02 §3.4.3`, §3.5.3) · (5) sayfa gezinmesi (§2.13.3).
  - **5.2.4 Yoktur:** ziyaretçiye sıralama seçeneği, seçenek değerine göre süzgeç (`02 §3.5.2`, §3.5.3) ve ürün kartında sepete ekleme (§2.11.1; K-774).
- **Aksiyonlar**
  - **5.2.5** Süzgeci uygulamak ya da kaldırmak — liste ilk sayfasına döner (§2.13.3); stok süzgecinin varsayılanı tükenmiş ürünleri de gösterir (`03 §2.1.4`). Onay ve mesaj yoktur.
  - **5.2.6** Ürün kartı → E-04 (3.3.5; `03 §2.1.5`); sayfa değiştirmek ekranın içinde kalır.
- **5.2.7 Validasyonlar.** Fiyat aralığının alt ve üst değeri sayıdır; alt değer üst değerden büyükse alan mesajı çıkar ve süzgeç uygulanmaz (OB-12, §2.12.1.1). Alan envanteri §7.1'dedir (5. oturum).
- **5.2.8 Durum × rol varyantları.** Aktörle değişen parça yoktur. Tükenmiş ürün kartta "Tükendi" işaretiyle kalır (§2.11.3), indirimli ürün indirimli kartla görünür (§2.11.2). Tam matris §6.2'dedir (5. oturum).
- **Boş / yükleniyor / hata durumları** (OB-07)
  - **5.2.9 Boş kategori:** yayında ürünü olmayan kategorinin adresi boş kategori sayfası döner — "sayfa bulunamadı" değil (`02 §3.28.6`); ekran boş hâl satırını ana sayfaya dönüş yoluyla gösterir (§2.7.1.2). Süzgecin sonucu boşsa aynı satır süzgeci kaldırma yolunu taşır.
  - **5.2.10** Izgara kendi yerinde yüklenir (§2.7.2); sayfa açılamazsa §2.7.3.2.
- **5.2.11 Responsive notları.** Izgara dar sınıfta iki, orta sınıfta üç, geniş sınıfta dört sütundur; süzgeçler dar ve orta sınıfta açılır bölümde, geniş sınıfta listenin yanındadır (§2.1.2, §2.13.5). Uzun kırıntı yolu dar sınıfta satır kırar.

*Kaynak: `02 §3.4.3`, §3.4.4, §3.5.2, §3.5.3, §3.28.6 · `03 §2.1.2`, §2.1.4, §2.10.1.1 · `10 §2` KP-1, KP-3 · K-766, K-774 · devir: `03 §2.1.2`'nin "kartın düzeni" devri — karşılığı OB-11.*

### 5.3 E-03 — Arama sonuçları

- **5.3.1 Aktör · giriş · çıkış.** Ziyaretçi · Müşteri. Giriş: üst bölümün araması (3.1.3). Çıkış: ürün kartı → E-04 (3.3.5).
- **5.3.2 İlk görülen:** aranan sözcük ve sonuç ızgarası; ekranın birincil düğmesi yoktur.
- **Bilgi hiyerarşisi**
  - **5.3.3** (1) Aranan sözcük, arama alanında düzenlenebilir hâliyle · (2) süzgeçler — kategori sayfasıyla aynı iki eksen · (3) sonuç ızgarası, kategori sayfasının sabit düzeniyle, en yeni önce (`03 §2.1.3`; `02 §3.5.3`) · (4) sayfa gezinmesi (§2.13.3).
  - **5.3.4** Arama yalnız ürün adında, büyük-küçük harfe ve Türkçe karaktere duyarsız çalışır; açıklama, kategori adı ve seçenek değerleri aranmaz (`02 §3.5.1`). Arşivlenmiş ürün sonuçlarda görünmez (`02 §3.7.6`).
- **Aksiyonlar**
  - **5.3.5** Aramayı yenilemek ve süzmek — liste ilk sayfasına döner (§2.13.3) · ürün kartı → E-04 (3.3.5).
- **5.3.6 Validasyonlar.** Boş arama gönderilmez; fiyat aralığı 5.2.7 gibidir.
- **5.3.7 Durum × rol varyantları.** Aktörle değişen parça yoktur; kart varyantları 5.2.8 gibidir. Tam matris §6.2'dedir (5. oturum).
- **Boş / yükleniyor / hata durumları** (OB-07)
  - **5.3.8** Sonucu olmayan arama boş hâl satırını gösterir: aranan sözcükle sonuç bulunmadığını söyler ve ürünlere — ana sayfanın kategori bağlantılarına — dönüş yolunu taşır (§2.7.1.2). Yükleniyor ve hata 5.2.10 gibidir.
- **5.3.9 Responsive notları.** Izgara ve süzgeçler 5.2.11 gibidir; dar sınıfta arama alanı açılır menüdedir ve sonuç sayfasında aranan sözcük içerik alanının başında da yazar (§2.2.4).

*Kaynak: `02 §3.5.1`–§3.5.3, §3.7.6 · `03 §2.1.3`, §2.1.4, §2.10.1.1 · `10 §2` KP-2, KP-3 · K-766.*

### 5.4 E-04 — Ürün sayfası

- **5.4.1 Aktör · giriş · çıkış.** Ziyaretçi · Müşteri. Giriş: ürün kartı (3.3.1, 3.3.5, 3.3.10), sepet kalemi (3.3.13), paylaşılmış ya da arama motorundan gelen adres (3.5.11), yöneticinin önizlemesi (3.7.2). Çıkış: kırıntı yolu (3.3.6), sepete ekleme — sayfada kalır (3.3.7), çerçevenin sepeti (3.1.9), taslak önizlemede "Düzenlemeye dön" (3.7.4).
- **5.4.2 İlk görülen:** ürünün görseli, adı ve KDV dahil fiyatı ile varyant seçimi; birincil düğme sepete eklemedir.
- **Bilgi hiyerarşisi** — dar sınıfın sırasıyla:
  - **5.4.3** (1) Kırıntı yolu — ürünün ana kategorisinden (`02 §3.4.4`) · (2) görsel galerisi — firmanın galeri sırasıyla; seçilen varyantın kendi görseli varsa galeri onunla açılır, yoksa ürününki görünür (`02 §3.11.1`, §3.11.2) · (3) ürün adı · (4) fiyat bloğu (5.4.4) · (5) varyant seçimi (5.4.6) · (6) adet ve sepete ekleme (5.4.7) · (7) teslimat bilgisi (5.4.5) · (8) açıklama — kapalı metin biçimi setiyle (`02 §3.11.6`) · (9) "Paylaş" düğmesi (`02 §3.30.7`).
  - **5.4.4 Fiyat bloğu** fiyat etiketinin zorunlu bilgilerini taşır (`02 §3.8.5`, §12.1.5): seçilen varyantın KDV dahil fiyatı · fiyatın uygulanmaya başlandığı tarih · ölçü birimi ve net miktar doluysa birim fiyat, satış fiyatının yanında — satış fiyatıyla aynıysa gösterilmez · fiziksel üründe üretim yeri ve üretim yeri Türkiye olan fiziksel üründe fiyatın yanında yerli üretim logosu; firma logoyu kapatamaz (K-610). **İndirimdeki üründe** indirimli fiyatın yanında referans fiyat ve — fiyat satırının hemen altında, tek satırda — indirimin başlangıç ve bitiş tarihi durur; düzen ürün kartıyla aynıdır (§2.11.2; `02 §3.9.2`), indirim işareti metinlidir (§2.5.6) ve tarihin yazım biçimi §8.2'dedir (5. oturum). Varyant seçilmeden önce fiyat, ürünün varsayılan varyantınınkidir; varyantların fiyatı farklıysa seçim fiyatı ve indirim satırını günceller. Taksit bilgisi yoktur (`02 §3.21.9`).
  - **5.4.5 Teslimat bilgisi** ürün tipine göre: fiziksel üründe kargoya verme süresi — `02 §3.20.4`'ün biçimiyle, ör. "2 iş günü içinde kargoya verilir"; değer ayardandır (§2.6.4) — ve firmanın teslimat il kısıtı varsa kısıt satırı (K-773) · hizmette ifa süresi, ödeme onayından itibaren gün olarak (`02 §3.2.2`) · dijital üründe indirmenin ödeme onayında açıldığı. Stok adedi hiçbir biçimde gösterilmez; "son 2 adet" türünden eşik uyarısı yoktur (`02 §3.6.5`).
  - **5.4.6 Varyant seçimi** en çok iki seçenek boyutudur (`02 §3.3.2`); seçeneği olmayan ürünün tek varyantı vardır ve seçim alanı görünmez (`02 §3.3.1`). Tükenmiş varyant seçenek listesinde "Tükendi" işaretiyle görünür ve seçilemez; firmanın açmadığı kombinasyon da görünür ve seçilemez; taslak varyant ziyaretçiye görünmez (`02 §3.3.3`, §3.6.3, §3.7.5; `03 §2.1.6`). Satın alınabilirlik ayrılmış adetler düşülerek okunur.
  - **5.4.7 Adet ve sepete ekleme.** Adet alanı fiziksel ürün ve hizmette vardır; dijital üründe adet birdir ve alan görünmez (`02 §3.12.4`). Sepete ekleme düğmesi ekranın birincil düğmesidir; metnini kaynak vermez, işleviyle adlandırılır (konvansiyon 9).
  - **5.4.8 Yoktur:** yorum, puan ve öneri bloğu (`02 §3.3.6`) · "bu ürünün geçtiği referanslar" (`02 §3.27.10`) · "Stokta haber ver" ve istek listesi (`02 §3.16.13`) · gömülü paylaşım kodları (`02 §3.30.7`) · ürün sayfasında cayma bilgisi ayrı bir blok değildir — istisna ve koşulu Ön Bilgilendirme Formu'ndadır (`02 §3.24.3`; E-13).
- **Aksiyonlar**
  - **5.4.9 Varyantı seçmek** — fiyat bloğu, görsel ve satın alınabilirlik seçilen varyanta göre güncellenir (`03 §2.1.6`).
  - **5.4.10 Varyantı adediyle sepete eklemek** (`03 §2.1.8`, §2.3.1; 3.3.7): satış açık ve varyant satın alınabilirken. Onay istemez; başarı kısa süreli bildirimle söylenir ve çerçevenin sepet sayısı artar (§2.3.1.4). Eklenmeyen durumlar düğmenin yanında yerinde kalıcı mesajla söylenir (§2.3.1.2): adet stoğu aşarsa "Bu adette stok yok" — adet söylenmez (`03 §3.2.1.6`) · ürünün "bir siparişte en fazla" sınırı aşılırsa sınır sayısıyla, ör. "Bu üründen bir siparişte en fazla 2 adet alınabilir" — sınır ve stok birlikte aşılıyorsa sınır söylenir (`02 §3.16.9`; `03 §3.2.1.7`) · aynı dijital varyant ikinci kez eklenirse "Bu ürün zaten sepetinde" (`03 §3.2.1.8`). Üye daha önce aldığı dijital varyantı eklerse ekleme yapılır ve düğmenin yanında "Bu ürünü daha önce aldınız" uyarısı kalır; misafirde uyarı yoktur (`03 §2.3.1`, §3.2.1.9).
  - **5.4.11 Paylaşmak** — cihazın paylaşım menüsü açılır, yoksa bağlantı kopyalanır ve kısa süreli bildirimle söylenir (`02 §3.30.7`; §4.4.7).
- **5.4.12 Validasyonlar.** Varyant seçilmeden sepete ekleme yapılmaz; seçilmemiş boyut alan mesajıyla işaretlenir (OB-12). Adet birden küçük olamaz; üst sınırı stok ve P-9 sınırıdır ve aşımı 5.4.10'un mesajlarıyla söylenir (`02 §3.16.8`, §3.16.9). Alan envanteri §7.1'dedir (5. oturum).
- **Durum × rol varyantları** (tam matris §6.2'de — 5. oturum)
  - **5.4.13 Bütün varyantları tükenmiş ürün:** ürün sayfası açılır, sepete ekleme düğmesinin yerinde "Tükendi" durur (`02 §3.6.4`; `03 §2.1.7`).
  - **5.4.14 Satış kapalı:** ürün görünür; sepete ekleme düğmesinin yerinde satışın kapalı olduğunu söyleyen yerinde kalıcı mesaj durur (OB-08, §2.8.1; `03 §2.1.9`, §3.2.1.17).
  - **5.4.15 Taslak ürün:** ziyaretçiye "sayfa bulunamadı" (E-06) döner; giriş yapmış yöneticiye sayfa "Taslak" bandıyla, taslak varyantlar dahil açılır (§2.9.2; `02 §3.7.5`; `03 §2.1.11`, §8.1.7). Arşivlenmiş ürün E-05'tir.
  - **5.4.16 Aktör farkı** yalnız 5.4.10'un dijital varyant uyarısıdır (üye). Kart ödemesinin, kuponun ve hesabın ürün sayfasında yüzü yoktur.
- **Boş / yükleniyor / hata durumları** (OB-07)
  - **5.4.17** Ekrana özgü boş hâl yoktur — görselsiz ürün, ürün adından üretilen alternatif metinle görselsiz görünür (`02 §3.11.4`); açıklama boşsa açıklama bölgesi görünmez. Sepete ekleme sürerken düğme ikinci kez basılamaz (§2.7.2); ekleme beklenmeyen bir sebeple tamamlanmazsa §2.7.3.1.
- **5.4.18 Responsive notları.** Geniş sınıfta ekran iki sütundur: solda galeri ve açıklama, sağda ad, fiyat bloğu, varyant seçimi, adet, sepete ekleme ve teslimat bilgisi (§2.1.2). Dar sınıfta 5.4.3'ün sırasıyla tek sütundur; galeri kaydırılan tek görsel alanıdır. Fiyat bloğunun indirim tarihleri ve yerli üretim logosu hiçbir sınıfta gizlenmez.

*Kaynak: `02 §3.2.2`, §3.3.1–§3.3.3, §3.3.6, §3.4.4, §3.6.3–§3.6.5, §3.7.5, §3.8.5, §3.9.2, §3.11.1, §3.11.2, §3.11.4, §3.11.6, §3.12.4, §3.16.8, §3.16.9, §3.16.13, §3.20.1, §3.20.4, §3.21.9, §3.24.3, §3.27.10, §3.30.7, §12.1.5 · `03 §1.10.1`, §2.1.5–§2.1.11, §2.3.1, §2.10.1.1, §2.10.1.2, §3.2.1.6–§3.2.1.9, §3.2.1.17, §4.1.10, §8.1.7 · `10 §2` KP-4, KP-5, KP-6, KP-9, KP-19, KP-36, KP-38 · K-773, K-774 · devir: K-545 (indirim tarihlerinin biçimi), K-610 (logonun yeri), `02 §3.20.1` (il kısıtının görünürlüğü).*

### 5.5 E-05 — Arşivlenmiş ürün sayfası

- **5.5.1 Aktör · giriş · çıkış.** Ziyaretçi · Müşteri. Giriş: arşivlenmiş ürünün paylaşılmış adresi (3.5.11). Çıkış: dönüş bağlantıları (3.3.12) ve çerçeve.
- **5.5.2 İlk görülen:** ürünün adı ve "Bu ürün artık satılmıyor" bilgisi; birincil düğme yoktur.
- **Bilgi hiyerarşisi**
  - **5.5.3** (1) Ürün adı · (2) ana görsel · (3) "Bu ürün artık satılmıyor" — durum bilgisi, metinli (`02 §3.7.6`) · (4) ana sayfaya ve ürünlere dönüş bağlantıları (3.3.12).
  - **5.5.4** Sayfa yalnız bu üç bilgiyi taşır; fiyat ve sepete ekleme yoktur (`02 §3.7.6`; `03 §2.1.10`). Sayfa listelerde, kategori sayfalarında, aramada ve site haritasında görünmez; yalnız doğrudan adresle açılır (`02 §3.7.6`, §3.30.6).
- **5.5.5 Aksiyonlar:** yalnız dönüş bağlantıları; onay ve mesaj yoktur.
- **5.5.6 Validasyonlar:** form yoktur.
- **5.5.7 Durum × rol varyantları.** Ziyaretçiye ve yöneticiye aynı görünür — arşiv bir önizleme değildir; yönetici şeridi çerçevenin parçasıdır (§2.9.1). Ürün yeniden yayına alınırsa adres E-04'ü döner, kalıcı silinirse E-06'yı (`03 §8.1.8`; `02 §3.7.7`). Tam matris §6.2'dedir (5. oturum).
- **5.5.8 Boş / yükleniyor / hata durumları.** Görseli olmayan arşiv ürünü görselsiz, yalnız adıyla görünür. Yükleniyor ve hata OB-07'nin kalıbıdır (§2.7.2, §2.7.3.2).
- **5.5.9 Responsive notları.** Tek sütundur ve sınıflar arasında yalnız görselin genişliği değişir.

*Kaynak: `02 §3.7.6`, §3.7.7, §3.30.6, §5.1 · `03 §2.1.10`, §8.1.8 · `10 §2` KP-7.*

### 5.6 E-06 — "Sayfa bulunamadı" sayfası

- **5.6.1 Aktör · giriş · çıkış.** Ziyaretçi · Müşteri. Giriş: taslak ya da silinmiş ürünün ve içeriğin adresi (3.5.11, 3.5.12), var olmayan adres (3.5.13), yönetici oturumu kapanmışken taslak adresi (3.7.5); boş kategori bu sayfaya düşmez (`02 §3.28.6`; 5.2.9). Çıkış: dönüş bağlantıları (3.3.12).
- **5.6.2 İlk görülen:** sayfanın bulunamadığını söyleyen başlık ve iki dönüş yolu; birincil düğme yoktur.
- **Bilgi hiyerarşisi**
  - **5.6.3** Sayfa vitrinin tam çerçevesindedir — marka, menü, altbilgi (`02 §3.30.4`; §2.2.6) — ve içerik alanında: (1) sayfanın bulunamadığını söyleyen başlık · (2) ana sayfaya dönüş · (3) ürünlere dönüş — ana sayfanın kategori bağlantılarıyla aynı birinci seviye kategoriler; yayında ürün yoksa bu yol görünmez (§2.7.1.1). İçeriği firma düzenlemez (`02 §3.30.4`).
  - **5.6.4** Sayfa adresin neden bulunamadığını — taslak, silinmiş ya da hiç olmamış — ayırt etmez; ziyaretçiye kapalı kaydın varlığı açığa çıkmaz (`02 §3.30.4`).
- **5.6.5 Aksiyonlar:** yalnız dönüş bağlantıları.
- **5.6.6 Validasyonlar:** form yoktur.
- **5.6.7 Durum × rol varyantları.** Taslak adresi giriş yapmış yöneticiye bu sayfayı değil, kaydı "Taslak" bandıyla açar (§2.9.2; `02 §3.7.5`, §3.27.25); panel oturumu kapanınca adres yeniden bu sayfayı döner (3.7.5). Tam matris §6.2'dedir (5. oturum).
- **5.6.8 Boş / yükleniyor / hata durumları.** Sayfa beklenmeyen hata sayfasından ayrıdır (§2.7.3.2); ekrana özgü boş hâl yoktur.
- **5.6.9 Responsive notları.** Tek sütun; dönüş yolları dar sınıfta alt alta dizilir.

*Kaynak: `02 §3.7`, §3.27.25, §3.28.6, §3.30.4, §6.8.5 · `03 §2.1.11`, §2.2.10, §3.5.3.5 · `10 §2` KP-7.*

### 5.7 E-07 — İçerik liste sayfası

- **5.7.1 Aktör · giriş · çıkış.** Ziyaretçi · Müşteri. Giriş: menüdeki Hizmetlerimiz ya da Referanslarımız (3.1.12), ana sayfa bloğunun "Tümünü gör"ü (3.3.4). Çıkış: içerik kartı → E-08 (3.3.8).
- **5.7.2 İlk görülen:** içerik tipinin adı — menüdeki ad — ve kayıtların kartları; birincil düğme yoktur.
- **Bilgi hiyerarşisi**
  - **5.7.3** (1) Tipin adı, firmanın değiştirdiği ad dahil (§2.2.3) · (2) içerik kartları (OB-11, §2.11.4): ana görsel, ad ya da başlık ve kısa açıklama; firmanın panelde verdiği elle sırayla (`02 §3.27.11`) · (3) sayfa gezinmesi — liste sayfa boyunu aşarsa (§2.13.3).
  - **5.7.4 Yoktur:** fiyat, sepete ekleme, süzgeç ve arama (`02 §3.27.4`; §2.13.2).
- **5.7.5 Aksiyonlar:** içerik kartı → E-08 (3.3.8; `03 §2.2.2`).
- **5.7.6 Validasyonlar:** form yoktur.
- **5.7.7 Durum × rol varyantları.** Ekran iki tipte aynı düzendedir; taslak kayıt ziyaretçiye listede görünmez (`02 §3.27.25`). Tam matris §6.2'dedir (5. oturum).
- **5.7.8 Boş / yükleniyor / hata durumları.** Yayında kaydı olmayan tipin menü öğesi görünmez (§2.7.1.1); adresi doğrudan açılırsa boş hâl satırı ana sayfaya dönüş yoluyla görünür (§2.7.1.2). Ana görseli olmayan kayıt kartta görselsiz, yalnız metinle görünür (K-755).
- **5.7.9 Responsive notları.** Kart ızgarası ürün ızgarasının sütun düzenini izler (§2.11.5).

*Kaynak: `02 §3.27.4`, §3.27.5, §3.27.11, §3.27.25, §3.28.4 · `03 §2.2.2`, §3.5.3.4 · `10 §2` KP-32 · K-755, K-766.*

### 5.8 E-08 — İçerik sayfası

- **5.8.1 Aktör · giriş · çıkış.** Ziyaretçi · Müşteri. Giriş: menü ve altbilgi (3.1.13, 3.1.15, 3.1.18), ana sayfa (3.3.3, 3.3.4), liste sayfası (3.3.8), paylaşılmış adres (3.5.12), yöneticinin önizlemesi (3.7.2). Çıkış: "Bize ulaşın" → E-10 (3.3.9), "İlgili ürünler" → E-04 (3.3.10), taslak önizlemede "Düzenlemeye dön" (3.7.4).
- **5.8.2 İlk görülen:** kaydın adı ya da başlığı ve metni; hizmet tanıtımında birincil düğme "Bize ulaşın"dır, öteki türlerde birincil düğme yoktur.
- **Bilgi hiyerarşisi** — dört tür aynı iskeletle (`03 §2.2.3`):
  - **5.8.3** (1) Ad ya da başlık · (2) görseller — firmanın galeri sırasıyla (`02 §3.27.15`) · (3) metin — kapalı metin biçimi setiyle (`02 §3.27.16`) · (4) türe özgü bölge (5.8.4) · (5) "Paylaş" düğmesi (`02 §3.30.7`).
  - **5.8.4 Türe özgü bölge:** **Hakkımızda** — uzun metin ve görseller; kısa tanıtım ana sayfanın bloğundadır (`02 §3.28.2`) · **hizmet tanıtımı** — fiyatsızdır ve satın alınmaz; "Bize ulaşın" düğmesi iletişim formuna götürür (`02 §3.27.4`); "İlgili ürünler" · **referans iş** — başlıkta müşteri adı; ayrı müşteri adı, yıl ve kategori alanı yoktur (`02 §3.27.5`); "İlgili ürünler" · **genel sayfa** — başlık, metin, görseller; düzeni sabittir (`02 §3.27.9`).
  - **5.8.5 "İlgili ürünler"** hizmet tanıtımında ve referans işte, firmanın bağladığı ürünlerden yalnız yayındakileri ürün kartıyla gösterir; bağlı ürün yoksa ya da hiçbiri yayında değilse başlığıyla birlikte görünmez (`02 §3.27.10`; §2.7.1.1).
  - **5.8.6 Video** firmanın verdiği bir bağlantıdır ve kendi platformunda açılır (`02 §3.27.17`; §4.4.4). **Yoktur:** gömülü video oynatıcı, dosya eki ve blog (`03 §2.2.3`).
- **Aksiyonlar**
  - **5.8.7** "Bize ulaşın" → E-10'un iletişim formu (3.3.9; `03 §2.2.7`) · "İlgili ürünler"de ürün kartı → E-04 (3.3.10; `03 §2.2.4`) · "Paylaş" (5.4.11 gibi) · video bağlantısı → dış sayfa. Onay ve mesaj yoktur.
- **5.8.8 Validasyonlar:** form yoktur.
- **5.8.9 Durum × rol varyantları.** Taslak kaydın sayfası ziyaretçiye E-06 döner, yöneticiye "Taslak" bandıyla açılır (§2.9.2; `02 §3.27.25`). Hakkımızda silinmez; taslaktaysa adres ziyaretçiye E-06 döner (`02 §3.27.3`). Bağlı ürün taslağa ya da arşive alınırsa kartı görünmez, yayına dönünce geri gelir (`03 §3.5.3.4`). Tam matris §6.2'dedir (5. oturum).
- **5.8.10 Boş / yükleniyor / hata durumları.** Görselsiz kayıt yalnız metinle görünür. Yükleniyor ve hata OB-07'nin kalıbıdır.
- **5.8.11 Responsive notları.** Tek sütundur; "İlgili ürünler" ürün ızgarasının sütun düzenini izler (§2.11.5). Görsel galerisi dar sınıfta kaydırılan tek görsel alanıdır.

*Kaynak: `02 §3.27.1`, §3.27.3–§3.27.5, §3.27.9, §3.27.10, §3.27.15–§3.27.17, §3.27.25, §3.28.2, §3.30.7, §5.2 · `03 §2.2.3`, §2.2.4, §2.2.7, §3.5.3.4, §8.6.1.3 · `10 §2` KP-32, KP-36.*

### 5.9 E-09 — Sık sorulan sorular sayfası

- **5.9.1 Aktör · giriş · çıkış.** Ziyaretçi · Müşteri. Giriş: menüdeki SSS (3.1.14). Çıkış: çerçeve.
- **5.9.2 İlk görülen:** soruların listesi; birincil düğme yoktur.
- **Bilgi hiyerarşisi**
  - **5.9.3** (1) Sayfanın adı — menüdeki ad · (2) bütün sorular ve cevapları tek listede, başlıklara bölünmeden, firmanın elle sırasıyla (`02 §3.27.6`, §3.27.11) · (3) "Paylaş" düğmesi (`02 §3.30.7`). Her soru açık hâlde, cevabıyla birlikte görünür; cevap kapalı metin biçimi setini kullanır (`02 §3.11.6`).
  - **5.9.4 Yoktur:** soru kategorisi ve ara başlık (`02 §3.27.6`) ve sayfa içi arama — site içi arama yalnız ürün adındadır (`02 §3.5.1`).
- **5.9.5 Aksiyonlar:** cevabın içindeki bağlantılar; onay ve mesaj yoktur.
- **5.9.6 Validasyonlar:** form yoktur.
- **5.9.7 Durum × rol varyantları.** Taslak soru ziyaretçiye görünmez; yöneticiye listede yerinde "Taslak" etiketiyle görünür (§2.9.3; `03 §8.6.1.3`). Tam matris §6.2'dedir (5. oturum).
- **5.9.8 Boş / yükleniyor / hata durumları.** Yayında soru yoksa menü öğesi görünmez (§2.7.1.1; 3.1.14); adres doğrudan açılırsa boş hâl satırı ana sayfaya dönüş yoluyla görünür (§2.7.1.2).
- **5.9.9 Responsive notları.** Tek sütundur; sınıflar arasında fark yoktur.

*Kaynak: `02 §3.5.1`, §3.11.6, §3.27.6, §3.27.11, §3.30.7 · `03 §2.2.3`, §8.6.1.3 · `10 §2` KP-32.*

### 5.10 E-10 — İletişim sayfası ve formu

- **5.10.1 Aktör · giriş · çıkış.** Ziyaretçi · Müşteri. Giriş: menünün son öğesi İletişim (3.1.16), hizmet tanıtımının "Bize ulaşın"ı (3.3.9) — formun başına iner. Çıkış: formun gönderilmesi — ekranda kalır (3.3.11); şubenin "Haritada aç" bağlantısı (§4.4.4).
- **5.10.2 İlk görülen:** "İletişim" başlığı altında firmanın iletişim ve yasal kimlik bilgileri ile iletişim formu; birincil düğme formun gönderme düğmesidir.
- **Bilgi hiyerarşisi** — dar sınıfın sırasıyla:
  - **5.10.3** (1) **"İletişim" başlığı altında kimlik bloğu** — altbilgideki blokla aynı tam set: firma tipinin zorunlu seti, iletişim bilgileri ve KEP adresi, doluysa işletme adı ya da tescilli marka, meslek odası ve meslekle ilgili davranış kuralları; setin içeriği `02 §3.1.3`'tedir ve burada kopyalanmaz. ETBİS doğrulama bilgisi girilmişse doğrulama bandı bloğun hemen yanında — dar sınıfta hemen altında — durur; alan boşsa band da yeri de yoktur (`02 §3.1.4`; K-747). Firma bu sayfaya ayrıca metin girmez (`02 §3.27.8`).
  - **5.10.4** (2) **İletişim formu** (OB-12) · (3) **şubeler** — firmanın elle sırasıyla; her şubede ad, adres, doluysa telefon, çalışma saatleri, görsel ve "Haritada aç" bağlantısı (`02 §3.27.7`, §3.27.11). Gömülü harita yoktur (`02 §8.3.3`).
  - **5.10.5 Formun alanları** (`02 §3.32.2`, §3.32.3): ad, e-posta, konu tipi ve mesaj zorunlu; telefon isteğe bağlı. Konu tipi kapalı beş değerden seçilir: Genel soru · Sipariş hakkında · Ürün hakkında · KVKK talebi · Diğer. Formun altında aydınlatma metninin bağlantısı durur; onay kutusu yoktur (`02 §3.33.5`; §2.12.1.6). Görünmez tuzak alan ziyaretçiye görünmez (`02 §3.32.7`).
  - **5.10.6 Yoktur:** dosya eki, ayrı sipariş numarası alanı — numara mesaja yazılır (`02 §3.32.2`) · ayıp talebi konu tipi — ayıp talebi sipariş sayfasından gider (`02 §3.32.3`) · üçüncü taraf captcha (`02 §8.3.1`) · gönderene talep numarası, "taleplerim" sayfası ve durum sorgusu (`02 §3.32.6`) · canlı destek, sohbet penceresi ve ayrı şikâyet kanalı (`02 §3.32.9`).
- **Aksiyonlar**
  - **5.10.7 Formu göndermek** (`03 §2.2.8`; 3.3.11). Onay istemez. Geçen gönderim bir iletişim talebi kaydı açar (İletişim talebi: Açık) ve ekran gönderim sonrası hâline geçer (5.10.8); gönderene alındı e-postası gitmez (K-678). Tuzak alanı dolu gönderim hata vermeden sessizce düşer ve ekran aynı gönderim sonrası hâlini gösterir (`03 §3.5.3.7`). L-4 aşılınca gönderim engellenir ve gönderme düğmesinin yanında nötr limit mesajı çıkar (`03 §3.5.3.8`; §2.3.5); yazılanlar silinmez.
  - **5.10.8 Gönderim sonrası hâli** formun yerinde, sayfa değişmeden görünür (K-678 devri; K-772): talebin firmaya iletildiğini ve cevabın yazılan e-posta adresine — sistemin dışında — geleceğini söyler; talep numarası ve takip yolu vermez (`02 §3.32.6`). Hâl yeni bir form açan bir bağlantı taşır; kimlik bloğu ve şubeler yerinde kalır.
  - **5.10.9 Formun kullanıldığı başka yollar** — ürünün ayrıca ekran açmadığı konular bu formdan, uygun konu tipiyle gelir: fiyat sorusu (`02 §3.2.1`), siparişe dair özel istek (`03 §3.2.1.20`), indirme hakkının yenilenmesi (`02 §3.32.10`), e-postasını yanlış yazan misafir alıcının düzeltme talebi (`03 §2.6.7`, §3.2.1.13), sipariş sayfasının üç yoluna girmeyen konular ve süresi dolan ayıp talebi (`03 §5.1.2`, §5.1.4), kaybolan gönderi (`03 §5.2.2.2`), bilgilendirmenin eksik kaldığı iddiasıyla cayma (`03 §5.2.3.1`), iade reddine itiraz (`03 §5.3.1.5`) ve KVKK başvurusu — "KVKK talebi" tipi (`03 §9.4.4`; `02 §3.15.4`). Ekran bu konular için ayrı alan ya da yönlendirme metni taşımaz; konu tipi listesi yeterlidir.
- **Validasyonlar** (alan envanteri §7.1'de — 5. oturum)
  - **5.10.10** Zorunlu dört alan boşsa ve e-posta biçimi geçersizse alan mesajı; gönderimde hatalar birlikte işaretlenir, özet çıkar ve odak ilk hatalı alana gider (§2.3.2). Konu tipi serbest metin kabul etmez (§2.12.1.3). L-4 için 5.10.7.
- **Durum × rol varyantları** (tam matris §6.2'de — 5. oturum)
  - **5.10.11 Üye girişliyken** ad ve e-posta ön dolu ve düzenlenebilir gelir (`02 §3.32.1`; §2.12.1.7); misafir alıcıda ve ziyaretçide boştur.
  - **5.10.12 Veri toplayan girişlerin kapısı kapalıyken** — aydınlatma metni tamamlanıp yayına alınmamışsa — form yerinde kapalı olduğunu söyler ve çalışmaz; kimlik bloğu ve şubeler görünür kalır (`02 §3.32.8`; `03 §3.5.3.9`; OB-08, §2.8.3).
  - **5.10.13** Taslak şube ziyaretçiye görünmez; yöneticiye listede "Taslak" etiketiyle görünür (§2.9.3).
- **Boş / yükleniyor / hata durumları** (OB-07)
  - **5.10.14** Şubesi olmayan firmada şubeler bölgesi görünmez (§2.7.1.1). Gönderim sürerken düğme ikinci kez basılamaz (§2.7.2); gönderim beklenmeyen bir sebeple tamamlanmazsa mesaj düğmenin yanında çıkar ve yazılanlar korunur (§2.7.3.1).
- **5.10.15 Responsive notları.** Geniş sınıfta iki sütundur: solda kimlik bloğu ve şubeler, sağda form; dar sınıfta 5.10.3–5.10.4'ün sırasıyla tek sütundur. Kimlik bloğu hiçbir sınıfta katlanmaz ve bir bağlantının arkasına saklanmaz (K-747); uzun unvan ve adres satır kırar.

*Kaynak: `02 §3.1.3`, §3.1.4, §3.2.1, §3.15.4, §3.27.7, §3.27.8, §3.27.11, §3.32.1–§3.32.3, §3.32.6–§3.32.10, §3.33.5, §8.3.1, §8.3.3, §6.8.7–§6.8.9, §12.2.6 · `03 §2.2.5`, §2.2.7–§2.2.9, §2.6.7, §3.2.1.13, §3.2.1.20, §3.5.2.7, §3.5.3.7–§3.5.3.9, §5.1.2, §5.1.4, §5.2.2.2, §5.2.3.1, §5.3.1.5, §6.1.1.4, §9.4.4 · `10 §2` KP-30, KP-33, KP-34, KP-35, KP-74 · park satırı K-14 · K-574 · K-747, K-772 · devir: K-547, K-553, K-627, K-678 (gönderim sonrası hâli).*

### 5.11 E-11 — Yasal metin sayfası

- **5.11.1 Aktör · giriş · çıkış.** Ziyaretçi · Müşteri. Giriş: altbilginin yasal bağlantıları (3.1.19) ve kişisel veri alanı açan ekranlardaki aydınlatma bağlantısı (`02 §3.33.5`). Çıkış: çerçeve.
- **5.11.2 İlk görülen:** metnin başlığı ve metnin kendisi; birincil düğme yoktur.
- **Bilgi hiyerarşisi** — üç sayfa aynı iskeletle:
  - **5.11.3** (1) Başlık: aydınlatma metni · çerez politikası · "İşlem rehberi" · (2) metnin tamamı, tek sayfada, bağlantının ya da pencerenin arkasında değil.
  - **5.11.4 Aydınlatma metni ve çerez politikası** firmanın yayına aldığı **güncel sürümdür** (`02 §3.33.1`–§3.33.3; `03 §7.2.22`); içeriğin kuralı `02 §3.33.2` ve §12.2'dedir. Siparişe donmuş eski sürümler bu sayfada değil, siparişin sayfasında okunur (`02 §3.33.3`; E-16).
  - **5.11.5 "İşlem rehberi"** onay adımındaki sabit "sipariş nasıl kurulur" metnini taşır — dört bilginin listesi `02 §3.24.8`'dedir; metin ürünün ürettiği sabit metindir ve satış kapalıyken de görünür (K-626).
  - **5.11.6 Yoktur:** Ön Bilgilendirme Formu ve Mesafeli Satış Sözleşmesi bu sayfada genel bir metin olarak durmaz — ikisi siparişe göre üretilir; ödeme adımında (E-13) ve siparişin sayfasında (E-16) okunur (`02 §3.33.1`). Ayrı üyelik sözleşmesi yoktur (`02 §3.24.4`). Çerez onay bandı yoktur; sitede yalnız zorunlu çerezler vardır (`02 §3.33.5`, §12.2.5; `10 §2` KP-73).
- **5.11.7 Aksiyonlar:** metnin içindeki bağlantılar; onay ve mesaj yoktur.
- **5.11.8 Validasyonlar:** form yoktur.
- **5.11.9 Durum × rol varyantları.** Firmanın düzenlediği iki metin henüz hiç yayına alınmamışsa — ilk kurulumda — altbilgide o metnin bağlantısı görünmez ve adresi E-06 döner (K-770); bu sürede satış ve veri toplayan girişler kapalıdır (`02 §3.33.9`, §3.32.8). "İşlem rehberi" bu kapıya bağlı değildir. Tam matris §6.2'dedir (5. oturum).
- **5.11.10 Boş / yükleniyor / hata durumları.** Ekrana özgü boş hâl yoktur — firmanın düzenlediği iki metin zorunludur ve boşaltılamaz (`02 §3.33.2`). Yükleniyor ve hata OB-07'nin kalıbıdır.
- **5.11.11 Responsive notları.** Tek sütun; uzun metin sınıflar arasında yalnız satır uzunluğuyla değişir.

*Kaynak: `02 §3.24.4`, §3.24.8, §3.32.8, §3.33.1–§3.33.3, §3.33.5, §3.33.9, §12.2 · `03 §7.2.22`, §7.3.42 · `10 §2` KP-30, KP-35, KP-73 · K-770 · devir: K-547, K-626 ("İşlem rehberi" sayfası).*

### 5.12 E-12 — Sepet

- **5.12.1 Aktör · giriş · çıkış.** Ziyaretçi · Müşteri. Giriş: çerçevenin sepeti (3.1.9), ödeme adımından "Sepete dön" ve ödeme adımının sepete geri gönderdiği hâller (3.1.10, 3.3.15). Çıkış: kalemin adı ya da görseli → E-04 (3.3.13), ödeme adımına geçiş → E-13 (3.3.14).
- **5.12.2 İlk görülen:** sepetteki kalemler ve ödenecek tutar; birincil düğme ödeme adımına geçiştir.
- **Bilgi hiyerarşisi** — dar sınıfın sırasıyla:
  - **5.12.3** (1) Bir kez söylenen mesajlar, içerik alanının başında (§2.3.1.3): girişte sepet birleştiyse eklenenler adıyla (`02 §3.16.4`) ve vitrinden kalkıp sepetten çıkan kalem (`02 §3.16.7`; `03 §2.3.5`) — metinleri K-772'dedir · (2) kalemler · (3) teslimat il kısıtı satırı — sepette fiziksel kalem varsa ve kısıt varsa (K-773) · (4) tutarın dökümü · (5) ödeme adımına geçiş düğmesi.
  - **5.12.4 Kalem satırı:** görsel ve ürün adı — ürün sayfasına götürür (K-775) — · varyantın seçenek değerleri · güncel birim fiyat · adet ve adedi değiştiren alan · satır tutarı · kalemi çıkarma. Dijital kalemin adedi birdir ve adet alanı yoktur (`02 §3.12.4`). Kalem fiyatını ve satın alınabilirliğini güncel üründen okur (`02 §3.16.5`).
  - **5.12.5 Kalemin satırındaki mesajlar** yerinde kalıcıdır ve koşul sürdükçe durur (§2.3.1.2): güncel fiyat ilk eklenme anındakinden farklıysa "Sepete eklediğinden beri fiyatı değişti" — eski fiyat ve değişimin yönü gösterilmez (`02 §3.16.5`) · tükenmiş varyant ya da kontenjanı dolmuş hizmet "Tükendi" (§2.5.6) · stok sepetteki adedin altına düştüyse "Bu adette stok yok" · ürünün sınırı aşıldıysa sınır mesajı (`02 §3.16.6`, §3.16.8, §3.16.10). Bu kalemler sepette kalır, siparişe girmez ve tutara, kargo hesabına ve eşiklere girmez; adet kendiliğinden düşürülmez (`03 §2.3.4`).
  - **5.12.6 Tutarın dökümü:** siparişe giren kalemlerin toplamı · kargo ücreti — sepette fiziksel kalem varsa; sipariş başına sabittir (`02 §3.19.1`) · ücretsiz kargo mesajı — eşik tanımlıysa ve fiziksel kalem varsa, kargo ücretinin hemen altında: eşiğe kalan tutar ya da "Kargo ücretsiz" (`02 §3.19.5`; §2.6.5; metni K-772) · ödenecek toplam. Asgari sipariş tutarı tanımlıysa ve fiziksel kalemli sepet altındaysa eksik tutar dökümün altında, ödeme adımına geçiş düğmesinin hemen üstünde söylenir (`02 §3.18.1`; `03 §3.2.1.11`; metni K-772). Kupon ödeme adımında girilir; sepette kupon alanı yoktur (`02 §3.10.1`).
  - **5.12.7 Yoktur:** taksit bilgisi (`02 §3.21.9`) · stok adedi (`02 §3.6.5`) · istek listesi ve sepet hatırlatması (`02 §3.16.13`) · sepetin süresi — sepet kendiliğinden boşalmaz (`02 §3.16.2`; `03 §4.1.6`).
- **Aksiyonlar**
  - **5.12.8 Adedi değiştirmek** — artırma sepete eklemenin denetiminden geçer ve aşım kalemin satırında 5.12.5'in mesajlarıyla söylenir (`03 §2.3.3`, §2.3.1). Onay istemez; tutar yerinde güncellenir.
  - **5.12.9 Kalemi çıkarmak** — onay istemez; kalem satırdan kalkar ve tutar güncellenir (`03 §2.3.3`). Sonuç ekranda görünür olduğu için ayrı bildirim yoktur.
  - **5.12.10 Ödeme adımına geçmek** → E-13 (3.3.14; `03 §2.3.8`): satış açık, siparişe girebilecek en az bir kalem var ve — fiziksel kalem varsa — asgari tutar karşılanıyorsa. Geçiş kapalıysa düğmenin yerinde sebebi yerinde kalıcı mesajla söylenir: siparişe girebilecek kalem kalmadığı (K-594; metni K-772) · asgari tutarın eksiği (5.12.6) · satışın kapalı olduğu (5.12.14).
  - **5.12.11 Kalemin adı ya da görseli** → E-04 (3.3.13; K-775).
- **5.12.12 Validasyonlar.** Adet birden küçük olamaz ve tam sayıdır; üst sınırın aşımı 5.12.5'in mesajlarıyla söylenir (`02 §3.16.8`, §3.16.9). Alan envanteri §7.1'dedir (5. oturum).
- **Durum × rol varyantları** (tam matris §6.2'de — 5. oturum)
  - **5.12.13 Üye ve misafir** aynı ekranı görür; üyenin sepeti hesabındadır ve her cihazda aynıdır, misafirinki tarayıcıya bağlıdır (`02 §3.16.1`). Çıkış yapan üyenin tarayıcısında sepet boş görünür (`03 §2.3.7`).
  - **5.12.14 Satış kapalıyken** sepet ve içeriği görünür kalır; ödeme adımına geçiş düğmesinin yerinde satışın kapalı olduğunu söyleyen yerinde kalıcı mesaj durur (OB-08, §2.8.1; `03 §2.1.9`, §3.2.1.17).
  - **5.12.15 Ödeme yarıda kaldıysa ya da ödeme sağlayıcısına erişilemediyse** sepet olduğu gibi durur ve müşteri aynı sepetten yeniden dener; ödeme başarılı olunca yalnız siparişe giren kalemler sepetten çıkar (`03 §3.1.4`, §2.5.3.1; `02 §3.16.12`). Ödeme adımında hiçbir ödeme yöntemi kullanılamadığı için sepete dönen müşteri daha sonra denemesi gerektiğini bu ekranın başında bir kez görür (`03 §10.1.1.1`; §2.3.1.3).
- **Boş / yükleniyor / hata durumları** (OB-07)
  - **5.12.16 Boş sepet** boş hâl satırını ürünlere dönüş yoluyla gösterir (§2.7.1.2); tutarın dökümü ve geçiş düğmesi görünmez.
  - **5.12.17** Adet değişikliği ve kalem çıkarma sürerken o satırın denetimi ikinci kez kullanılamaz ve döküm kendi yerinde bekler (§2.7.2); işlem beklenmeyen bir sebeple tamamlanmazsa mesaj satırda çıkar (§2.7.3.1).
- **5.12.18 Responsive notları.** Geniş sınıfta iki sütundur: solda kalemler, sağda döküm, il kısıtı satırı ve geçiş düğmesi. Dar sınıfta tek sütundur; kalem satırı görsel ile adı bir satıra, fiyat, adet ve satır tutarını alt satıra alır; ödenecek toplam ve geçiş düğmesi kalemlerin altında durur.

*Kaynak: `02 §3.6.5`, §3.10.1, §3.12.4, §3.16.1–§3.16.13, §3.18.1–§3.18.3, §3.19.1, §3.19.5, §3.20.1, §3.21.9 · `03 §2.1.8`, §2.1.9, §2.3.1–§2.3.8, §2.5.1.3, §2.5.1.4, §2.5.3.1, §2.10.1.2, §3.1.4, §3.2.1.1–§3.2.1.11, §3.2.1.15, §3.2.1.17, §4.1.6, §8.1.10, §10.1.1.1, §10.1.1.2 · `10 §2` KP-6, KP-9, KP-10, KP-13, KP-38 · K-772, K-773, K-775 · devir: K-125 (birleşme mesajının metni ve yeri), K-138 (ücretsiz kargo mesajının biçimi ve yeri), K-594 (siparişe girebilecek kalem kalmadığının mesajı), `02 §3.20.1` (il kısıtının görünürlüğü).*

### 5.13 E-13 — Ödeme adımı

- **5.13.1 Aktör · giriş · çıkış.** Müşteri — misafir alıcı ya da üye. Giriş: sepetten geçiş (3.3.14), girişten dönüş (3.3.16, §3.6.2) ve ödeme adımında kapanan oturumdan dönüş (§3.6.5). Çıkış: "Sepete dön" ve sepete geri gönderen hâller → E-12 (3.1.10, 3.3.15) · giriş → E-22 → E-13 (3.3.16) · onay → E-14 (3.3.17).
- **5.13.2 İlk görülen:** İletişim bölümü ve — geniş sınıfta yanında, dar ve orta sınıfta üstte katlanmış — sipariş özeti ile ödenecek toplam. Ekranın tek birincil düğmesi onay bölümünün sonundaki "Siparişi onayla — ödeme yükümlülüğü doğar"dır (K-756).
- **Bilgi hiyerarşisi** — ekran tek sayfadır; bölümler alt alta ve hepsi açık durur, ardışık ekranlara bölünmez (K-756):
  - **5.13.3 Çerçeve:** sadeleşmiş vitrin çerçevesi — üst bölümde logo ya da marka adı ve "Sepete dön"; menü, arama ve duyuru şeridi yoktur; altbilgi kimlik bloğu ve ETBİS bandı dahil eksiksiz kalır (OB-02, §2.2.7; K-747, K-757).
  - **5.13.4 Bölümlerin sırası** `03 §2.4`'ün sırasıdır: (1) İletişim · (2) Teslimat adresi — sepette fiziksel kalem varsa · (3) Fatura adresi · (4) Ödeme yöntemi · (5) Onay (K-756).
  - **5.13.5 (1) İletişim.** Misafir alıcıda bölümün başında "Hesabınız var mı? Giriş yapın" bağlantısı ve e-posta alanı durur; e-posta doğrulama koduyla sınanmaz ve ikinci kez yazdırılmaz (`02 §3.24.5`). Yazılan e-posta kayıtlı bir müşteri hesabınınsa alanın hemen altında "bu e-posta kayıtlı, giriş yaparsanız adresleriniz dolu gelir" hatırlatması "Giriş yap" bağlantısıyla, yerinde kalıcı mesaj olarak çıkar ve siparişi engellemez (`02 §3.13.3`; `03 §2.4.1`; K-758). Üyede alan yoktur; siparişin iletişim e-postası olarak hesabın doğrulanmış e-postası gösterilir (`03 §2.4.1`).
  - **5.13.6 (2) Teslimat adresi** — adres formunun teslimat varyantı (OB-13, §2.12.2.2): üye adres defterinden seçer ya da yeni adres yazar ve yeni adresi aynı adımda deftere kaydetmeyi seçebilir; misafir alıcı her siparişte yazar; telefon zorunludur ve telefonu olmayan defter kaydı seçilince burada istenir (`03 §2.4.2`). Bölümün başında firmanın teslimat il kısıtı — varsa — K-773'ün satırıyla yinelenir; teslimat yapılmayan ildeki defter kaydı seçilemez ve sebebini yanında yazar (`02 §3.20.1`). Teslimat seçeneği yoktur — tek teslimat yolu kargodur (`02 §3.20.2`). Sepette fiziksel kalem yoksa bölüm hiç görünmez (`02 §3.14.4`).
  - **5.13.7 (3) Fatura adresi** — adres formunun fatura varyantı: sepette fiziksel kalem varsa "Teslimat adresimle aynı" işaretli gelir ve işaret kaldırılınca ikinci form açılır; fiziksel kalemsiz siparişte işaret yoktur ve form doğrudan görünür. Telefon, kurumsal fatura alanı, posta kodu ve T.C. kimlik numarası yoktur (`03 §2.4.3`; `02 §3.14.6`; K-756).
  - **5.13.8 (4) Ödeme yöntemi:** kart — kurulumda sağlayıcı anahtarları tanımlıysa — ve Havale/EFT — firma açtıysa (`02 §3.21.4`, §3.21.5). Tek yöntem açıksa seçili gelir (K-757). Kullanılamayan yöntem listede kalır, seçilemez ve sebebini yanında yazar: sağlayıcıya erişilemiyorsa kart "şu an kullanılamıyor"; L-8 doluysa havale, nötr mesajla — kart yolu açık kalır (`03 §2.4.5`, §3.2.1.21; §2.7.4). Kapıda ödeme ve taksit bilgisi yoktur; taksit yalnız sağlayıcının kart ekranındadır (`02 §3.21.9`).
  - **5.13.9 (5) Onay bölümü** yasal bilgilendirmeyi tek yerde, şu sırayla ve hiçbirini bir bağlantının ya da pencerenin arkasına koymadan taşır (K-756):
    - (a) **Özet farkı mesajı** — varsa, bölümün başında, yerinde kalıcı (5.13.20).
    - (b) **Onay özeti** — siparişe girecek kalemler, tutarın dökümü (KDV, indirim, kuponun payı, kargo ücreti ya da "Kargo ücretsiz") ve ödenecek toplam (`03 §2.4.6`); misafir alıcıda "Siparişiniz şu adrese gönderilecek: …" satırı ve yanında İletişim bölümüne götüren "Düzelt" (`02 §3.24.5`).
    - (c) **Ön Bilgilendirme Formu'nun tam metni**, sayfanın içinde kaydırılan bir kutuda, açık. Form özetle birebir aynıdır; içeriği `02 §3.24.3`'tedir ve burada kopyalanmaz — cayma hakkının koşulları ve süresi, istisna işaretli kalemde istisna ve sebebi, hizmette ifa süresi, firmanın kimliği ve iade adresi formun içindedir.
    - (d) **Mesafeli Satış Sözleşmesi'nin tam metni**, aynı biçimde.
    - (e) Sabit **"sipariş nasıl kurulur" metni** ve **aydınlatma metninin bağlantısı** (`02 §3.24.3`, §3.33.5).
    - (f) **Onay kutuları** (OB-18): her siparişte iki kutu — Ön Bilgilendirme Formu'nun okunduğunun teyidi ve Mesafeli Satış Sözleşmesi'nin onayı —; sepette dijital kalem varsa üçüncü, hizmet kalemi varsa dördüncü kutu (`02 §3.24.1`, §3.24.2). Dijital ve hizmet kutusunun nihai metni ve kapsadığı kalemlerin gösterimi K-771'dedir. Hiçbir kutu işaretli gelmez.
    - (g) **Ödenecek toplam** ve hemen altında **"Siparişi onayla — ödeme yükümlülüğü doğar"** düğmesi (`02 §3.24.1`; K-544). Kutular ile düğme arasına başka içerik girmez (§2.12.7.2).
  - **5.13.10 Sipariş özeti sütunu** (K-757): kalemler, döküm, kargo ücreti ya da "Kargo ücretsiz", kupon alanı ve ödenecek toplam. Ücretsiz kargo eşiğine kalan tutar dökümde, kargo ücretinin hemen altında durur (`02 §3.19.5`; §2.6.5; metni K-772). **Kupon alanı** özetin içinde, toplamın üstündedir ve kapalı bir satır olarak gelir ("Kupon kodum var") — varyantları OB-17'dedir (§2.12.6).
  - **5.13.11 Yoktur:** sipariş notu ya da teslimat talimatı alanı — özel istek iletişim formundan gelir (`02 §3.24.7`; 5.10.9) · KVKK rıza kutusu, yaş beyanı ve doğum tarihi (`02 §3.24.1`, §3.24.4) · pazarlama onayı kutusu (§2.12.7.4) · e-postanın doğrulama kodu (`02 §3.24.5`) · ayrı üyelik sözleşmesi (`02 §3.24.4`).
- **Aksiyonlar**
  - **5.13.12 E-postayı yazmak** (misafir alıcı; `03 §2.4.1`) — kayıtlı adreste hatırlatma çıkar (5.13.5).
  - **5.13.13 Giriş yapmak** → E-22 ve geri E-13 (3.3.16): girişten — şifreyle ya da Google ile — dönen müşteri ödeme adımını üye olarak görür; sepetler birleşir ve birleşme bir şey eklediyse ekranın başında bir kez söylenir; misafirken yazdığı adres yeni adres olarak durur ve defter yanına gelir; kupon yeniden uygulanır (§3.6.2; K-758). Girişten vazgeçen müşteri yazdıklarıyla döner.
  - **5.13.14 Adresleri seçmek, yazmak ve yeni adresi deftere kaydetmeyi seçmek** (`03 §2.4.2`, §2.4.3); deftere kayıt müşterinin seçimidir ve kendiliğinden yapılmaz (K-680).
  - **5.13.15 Kupon kodunu uygulamak, değiştirmek ya da kaldırmak** (`03 §2.4.4`; OB-17): kabul edilen kod adı ve indirdiği tutarla görünür ve özet yerinde güncellenir; kabul edilmeyen kod alanın altında alan mesajıyla söylenir (`03 §3.2.1.12`); L-6 aşılınca alan kapanır ve nötr mesajı kendi yerinde taşır (`03 §3.2.1.18`; §2.3.5). Kupon ücretsiz kargo eşiğinin ve asgari tutarın tabanını düşürmez (`02 §3.18.2`, §3.19.4).
  - **5.13.16 Ödeme yöntemini seçmek** (`03 §2.4.5`).
  - **5.13.17 Kutuları işaretlemek** (`03 §2.4.7`).
  - **5.13.18 Siparişi onaylamak** — "Siparişi onayla — ödeme yükümlülüğü doğar" (`03 §2.4.8`, §2.4.9; 3.3.17). Onay penceresi açılmaz: onay bölümü onayın kendisidir. Eksik ya da hatalı bölüm varken basılırsa sipariş oluşmaz, eksikler alan mesajıyla işaretlenir ve ekran ilk eksiğe gider — işaretlenmemiş kutu dahil (§2.3.2, §2.12.7.3; K-756). Sepet onay anında yeniden değerlendirilir: fark yoksa sipariş doğar ve müşteri E-14'e geçer (Alındı + Bekliyor).
  - **5.13.19 "Sepete dön"** → E-12 (3.1.10); yazılanlar o tarayıcıda ve o ziyaret boyunca durur (§3.6.2).
  - **5.13.20 Onay anında fark çıkarsa** sipariş oluşmaz (`03 §3.2.1.3`; `02 §3.17.2`): ekran onay bölümüne döner; bölümün başında neyin değiştiğini söyleyen yerinde kalıcı mesaj çıkar — ör. "Lambanın fiyatı 1.000 TL'den 1.200 TL'ye değişti" (`02 §3.17.2`) — ve güncel özet ile metinler gösterilir; Ön Bilgilendirme Formu'nun ya da sözleşmenin sürümü değiştiyse kutuların işareti kalkar ve müşteri onları yeniden işaretler (§2.12.7.3). Ayrılamayan kalem için sayı söylenmez. Siparişe girebilecek kalem kalmadıysa sipariş oluşmaz ve müşteri E-12'ye döner (`03 §2.4.8`, §3.2.1.4).
  - **5.13.21 Onaylanamayan hâller** düğmenin yanında yerinde kalıcı mesajla söylenir ve sipariş oluşmaz: L-7'nin eşiği — nötr mesaj (`03 §3.2.1.19`; §2.3.5) · satışın o arada kapanmış olması (5.13.27). Hiçbir ödeme yöntemi kullanılamıyorsa sipariş verilemez ve müşteri daha sonra denemesi söylenerek sepetine döner (`03 §3.2.1.15`, §10.1.1.1; 3.3.15).
- **Validasyonlar** (alan envanteri §7.1'de — 5. oturum)
  - **5.13.22 İletişim:** e-posta zorunlu ve biçimi geçerli (misafir alıcı).
  - **5.13.23 Adresler:** alıcının adı ve soyadı, il — kapalı listeden —, ilçe ve açık adres zorunlu; teslimat adresinde telefon zorunlu (`02 §3.14.6`; OB-13). Teslimat ili firmanın teslimat yaptığı illerden biri olmalıdır; değilse alan mesajı sebebi söyler — "Bu firma Ankara iline teslimat yapmıyor" biçiminde (`02 §3.20.1`; `03 §3.2.1.10`; metni K-772).
  - **5.13.24 Ödeme ve onay:** ödeme yöntemi seçilmiş olmalıdır; kutular işaretli olmalıdır (`02 §3.24.1`, §3.24.2). Kupon `02 §3.10`'un koşullarıyla ve L-6 ile denetlenir. L-7 ve L-8 nötr mesajla söylenir (`02 §6.1.5`).
- **Durum × rol varyantları** (tam matris §6.2'de — 5. oturum)
  - **5.13.25 Misafir alıcı ve üye:** misafirde e-posta alanı, giriş bağlantısı ve hatırlatma, özette "Siparişiniz şu adrese gönderilecek" satırı; üyede hesabın e-postası, adres defteri ve deftere kaydetme seçimi. Giriş yapmadan sipariş veren hesap sahibinin siparişi hesabına anında düşer — ekran bunu engellemez (`02 §3.13.3`; `03 §3.2.1.14`).
  - **5.13.26 Sepetin içeriğine göre:** fiziksel kalem yoksa teslimat bölümü, il kısıtı satırı ve kargo satırı görünmez ve fatura formu doğrudan açılır; dijital kalem varsa üçüncü, hizmet kalemi varsa dördüncü kutu çıkar (`02 §3.14.4`, §3.24.2). Ödeme yöntemine göre ekran değişmez; havale ve kartta düğme metni aynıdır (`02 §3.24.1`).
  - **5.13.27 Satış kapalı:** satış kapalıyken ödeme adımına geçilemez (`03 §2.3.8`); müşteri ekrandayken satış kapanırsa onay sipariş doğurmaz ve düğmenin yanında satışın kapalı olduğu söylenir; sepet korunur (OB-08, §2.8.1; `03 §3.2.1.17`).
  - **5.13.28 Oturumu ödeme adımında kapanan üye** oturumun kapandığını görür ve girişe götürülür; dönüşü 5.13.13'tür (§3.6.5; K-758).
- **Boş / yükleniyor / hata durumları** (OB-07)
  - **5.13.29** Ekranın boş hâli yoktur: siparişe girebilecek kalemi olmayan sepette ödeme adımı açılmaz (`03 §2.3.8`).
  - **5.13.30** Kupon, adres ya da yöntem değişince özet kendi yerinde yeniden hesaplanır ve bu sürede özet bölgesi bekleme göstergesi gösterir; onay sürerken düğme ikinci kez basılamaz ve sürdüğünü metinle söyler (§2.7.2).
  - **5.13.31** Onay beklenmeyen bir sebeple sonuçsuz kalırsa ekran "sipariş verilmedi" demez; müşteriyi sonucu görmeye götürür — üyeyi sipariş geçmişine, misafir alıcıyı sipariş takibi girişine (§2.7.3.3); yazılanlar ve sepet durur. Ödeme sağlayıcısına erişilemezliği adı konmuş hatadır ve 5.13.8'in "şu an kullanılamıyor" hâliyle gösterilir (§2.7.4).
- **5.13.32 Responsive notları** (K-757). **Geniş** sınıfta iki sütundur: solda beş bölüm, sağda sayfayla birlikte görünür kalan sipariş özeti. **Dar ve orta** sınıfta tek sütundur: özet bölümlerin üstünde katlanmış bir satırdır — kalem sayısı ve ödenecek toplam; açılınca tamamı ve kupon alanı — ve onay bölümünde tam hâliyle yeniden durur; ekranın altında ödenecek toplamı gösteren sabit bir satır kalır. İki yasal metnin kutuları her sınıfta sabit yükseklikte kaydırılır; kutular ve düğme metinlerin altında kalır ve hiçbir sınıfta metinler katlanmaz ya da bağlantıya dönüşmez (K-756).

*Kaynak: `02 §3.10`, §3.13.3, §3.14, §3.17.1, §3.17.2, §3.18.2, §3.19.4, §3.19.5, §3.20.1, §3.20.2, §3.21.4, §3.21.5, §3.21.9, §3.24.1–§3.24.7, §3.33.1, §3.33.4, §3.33.5, §3.33.7, §6.1.5, §8.3, §12.1 · `03 §2.3.8`, §2.4.1–§2.4.9, §2.10.1.3, §2.10.1.4, §3.1.3, §3.2.1.3, §3.2.1.4, §3.2.1.10, §3.2.1.12–§3.2.1.15, §3.2.1.17–§3.2.1.21, §3.5.4.3, §4.1.16, §4.1.25, §6.1.1.6–§6.1.1.8, §6.1.4.1, §6.1.4.2, §10.1.1.1, §10.1.1.4 · `10 §2` KP-6, KP-8, KP-11, KP-12, KP-13, KP-14, KP-45, KP-74 · K-747, K-756, K-757, K-758, K-771, K-772, K-773 · devir: K-99 (hatırlatmanın biçimi), K-493, K-503, K-509 (formun içeriği), K-544 (düğmenin metni ve biçimi), K-550 (dijital kutunun nihai metni), K-553, K-561, K-591, K-679, K-680.*

### 5.14 E-14 — Sipariş teyit ve bekleme ekranı

- **5.14.1 Aktör · giriş · çıkış.** Müşteri. Giriş: yalnız ödeme adımında onaydan hemen sonra (3.3.17); başka bir yoldan açılmaz — kart dönüşü sipariş sayfasına iner (3.5.8). Çıkış: kartta ödeme sağlayıcısının sayfası (3.3.18); havalede üyeye sipariş sayfası, misafir alıcıya sipariş takibi girişi (3.3.19; K-776).
- **5.14.2 İlk görülen:** siparişin alındığı ve sipariş numarası (`02 §3.17.1`; K-704).
- **Bilgi hiyerarşisi** — iki varyant (K-776):
  - **5.14.3 Kart:** (1) siparişin alındığı ve sipariş numarası · (2) ödemenin henüz alınmadığı ve ödemenin ödeme sağlayıcısının sayfasında yapılacağı · (3) birincil düğme: ödeme sağlayıcısının sayfasına geçiş. Ekran kendiliğinden, süreli bir yönlendirme yapmaz (K-776). Kart bilgisi bu ekranda ve üründe girilmez (`02 §8.3.4`).
  - **5.14.4 Havale/EFT:** (1) siparişin alındığı ve sipariş numarası · (2) firmanın siparişe donmuş IBAN'ı — salt okunur bilgi, OB-14 değildir (§2.12.3.4; `02 §3.21.6`; K-663) · (3) ödenecek toplam · (4) son ödeme günü (§2.6.4; Z-8) — sipariş sayfasının gösterdiği aynı bilgi (`03 §2.5.2.1`, §2.6.4) · (5) siparişe nereden ulaşılacağı: üyede sipariş sayfasına giden bağlantı, misafir alıcıda sipariş e-postasındaki bağlantı ve sipariş takibi girişine giden bağlantı (K-776).
  - **5.14.5** Aynı bilgi sipariş e-postasıyla da gider (B-1, havalede B-2 — `03 §2.4.9`); ekran e-postanın metnini taşımaz (§4.4.1).
- **Aksiyonlar**
  - **5.14.6 Kart:** ödeme sağlayıcısının sayfasına geçmek (3.3.18; `03 §2.5.1.1`). Ödeme orada yarıda kalırsa sepet olduğu gibi durur (`03 §2.5.1.4`); dönüş sipariş sayfasına iner ve sonuç gelmemişse sayfa ödemenin beklendiğini söyler (3.5.8; `03 §2.5.1.2`).
  - **5.14.7 Havale:** IBAN'ı kopyalamak — kısa süreli bildirimle söylenir (§2.3.1.4) · siparişe gitmek (5.14.4; 3.3.19).
- **5.14.8 Validasyonlar:** form yoktur.
- **5.14.9 Durum × rol varyantları.** Sipariş Alındı + Bekliyor'dadır; ekran durum rozeti göstermez — iki eksenin rozetleri sipariş sayfasındadır (§2.5.1; K-777). Üye ile misafir alıcı arasındaki tek fark 5.14.4'ün (5) satırıdır. Tam matris §6.2'dedir (5. oturum).
- **5.14.10 Boş / yükleniyor / hata durumları.** Ekranın boş hâli yoktur. Sayfa yenilenirse ya da geri gelinirse ekran yeniden sipariş oluşturmaz; sipariş onay anında doğmuştur (`02 §5.3.2`). Ödeme sağlayıcısına onaydan sonra bağlanılamazsa ödeme sayfası açılmaz, sipariş kendiliğinden iptal edilir ve sepet korunur (`03 §10.1.1.2`); bu adı konmuş bir hatadır (§2.7.4) ve müşteri sepete döner.
- **5.14.11 Responsive notları.** Tek sütundur; IBAN dar sınıfta satır kırmadan ve kopyalanabilir biçimde tek blokta durur.

*Kaynak: `02 §3.17.1`, §3.21.6, §5.3.2, §8.3.4 · `03 §1.11.13`, §2.4.9, §2.5.1.1, §2.5.1.2, §2.5.1.4, §2.5.2.1, §2.6.4, §2.10.1.5, §4.2.1, §10.1.1.2 · `10 §2` KP-15, KP-17 · K-776, K-777 · devir: K-663 (donmuş IBAN), K-704 (teyit ve bekleme ekranı).*

### 5.15 E-15 — Sipariş takibi girişi

- **5.15.1 Aktör · giriş · çıkış.** Misafir alıcı — ekran çerçevenin "Sipariş takibi" bağlantısıyla oturumsuz her kullanıcıya açıktır (3.1.5). Giriş: 3.1.5; havale teyit ekranından (5.14.4). Çıkış: eşleşen sorgu → E-16 (3.3.21).
- **5.15.2 İlk görülen:** sipariş numarası ve e-posta alanları; birincil düğme sorgudur.
- **Bilgi hiyerarşisi**
  - **5.15.3** (1) Başlık · (2) sipariş numarası ve e-posta alanları (OB-12) · (3) sorgu düğmesi · (4) başka iki yolun hatırlatması: sipariş e-postasındaki bağlantı sayfayı e-posta sormadan açar; üye siparişini hesabındaki sipariş geçmişinden açar — "Giriş yap" bağlantısıyla (`02 §3.22.3`; `03 §2.6.1`, §2.6.3).
- **Aksiyonlar**
  - **5.15.4 Sorgulamak** (`03 §2.6.2`; 3.3.21): numara ve e-posta eşleşirse E-16 açılır; numara tek başına açmaz. Eşleşmezse ekranda kalınır ve formun başında nötr bir mesaj hangi alanın yanlış olduğunu ve siparişin var olup olmadığını söylemeden eşleşmediğini söyler (`03 §3.2.2.2`; metni K-772). L-5 iki eksende ayrı ayrı sayılır; eşik aşılınca sorgu düğmesinin yanında `02 §6.1.5`'in nötr limit mesajı çıkar ve 5.15.3'ün (4) hatırlatması yerinde kalır (`03 §6.1.1.5`; K-602; §2.3.5). Onay istemez.
- **5.15.5 Validasyonlar.** İki alan zorunludur; e-posta biçimi denetlenir — alan mesajı. Biçimce geçerli sorgunun sonucu 5.15.4'ün mesajlarıdır. Alan envanteri §7.1'dedir (5. oturum).
- **Durum × rol varyantları** (tam matris §6.2'de — 5. oturum)
  - **5.15.6** Hesabı silinmiş müşteri ve e-postası değişmiş hesabın siparişi bu ekrandan siparişe donmuş e-postayla açılır (`03 §2.6.6`, §3.2.2.3, §9.4.3). E-postası yanlış yazılmış misafir siparişi bu ekrandan açılamaz; yol iletişim formu ve firmanın düzeltmesidir (`03 §3.2.2.1`, §3.2.1.13; 5.10.9).
- **5.15.7 Boş / yükleniyor / hata durumları.** Ekranın boş hâli yoktur. Sorgu sürerken düğme ikinci kez basılamaz (§2.7.2); beklenmeyen hata §2.7.3.1'dir. Kesinti sürerken de sipariş akışı e-postaya bağlı değildir ve bu yol açıktır (`03 §10.1.2.4`).
- **5.15.8 Responsive notları.** Tek sütundur; iki alan her sınıfta alt alta durur.

*Kaynak: `02 §3.22.3`, §3.22.5, §6.1.5, §8.3.5 · `03 §2.6.1`–§2.6.3, §2.6.6, §3.2.1.13, §3.2.2.1–§3.2.2.3, §6.1.1.5, §9.4.3, §10.1.2.4 · `10 §2` KP-18 · K-772, K-776 · devir: K-602 (takip formunun nötr mesajı).*

### 5.16 E-16 — Sipariş sayfası

- **5.16.1 Aktör · giriş · çıkış.** Müşteri — misafir alıcı ya da üye. Giriş üç yoldandır (`02 §3.22.3`): üyenin sipariş geçmişi (3.3.20), misafir alıcının sipariş takibi girişi (3.3.21), sipariş e-postasındaki bağlantı — dijital ürünün indirme bağlantısı ve B-14'ün bağlantısı dahil (3.5.1–3.5.3); ayrıca kart ödemesinden dönüş (3.5.8) ve havale teyit ekranından üyenin bağlantısı (3.3.19). Çıkış: iptal ve gecikme feshi → E-17 (3.3.22) · cayma → E-18 (3.3.23) · "sorun bildir" ve ayıp talebini yeniden açma → E-19 (3.3.24) · kargo şirketinin takip sayfası (§4.4.4) · üyede çerçevenin "Siparişlerim"i (3.1.7).
- **5.16.2 İlk görülen:** sipariş numarası ve siparişin iki ekseninin iki rozeti (§2.5.1; K-777); müşteriden bir şey bekleniyorsa — havale ödemesi, açılmış bir IBAN alanı — hemen altında o. Birincil düğme yalnız müşteriden bir giriş beklenirken vardır: açılmış IBAN alanının kaydetme düğmesi. Öteki hâllerde birincil düğme yoktur; iptal, cayma ve "sorun bildir" eşit ağırlıkta ikincil düğmelerdir — hiçbiri öne çıkarılmaz ve hiçbiri saklanmaz.
- **Bilgi hiyerarşisi** — dar sınıfın sırasıyla:
  - **5.16.3** (1) **Başlık:** sipariş numarası, sipariş tarihi ve iki rozet — Sipariş durumu ve Ödeme durumu, yan yana, adlarıyla (§2.5.1.1, §2.5.1.2; `02 §5.4`, §5.5) · (2) **müşteriden beklenen** (5.16.4) — varsa · (3) **kalemler** (5.16.5) · (4) **işlemler** (5.16.6) · (5) **tutarın dökümü** (5.16.7) · (6) **teslimat** (5.16.8) · (7) **adresler ve iletişim e-postası** — siparişe donmuş hâliyle (`02 §3.23.3`) · (8) **belgeler** (5.16.9) · (9) yokluklar (5.16.10).
  - **5.16.4 Müşteriden beklenen** — bölgenin başında yerinde kalıcı mesaj olarak (§2.3.1.2), koşul sürdükçe: **havale ödemesi** — Alındı + Bekliyor'da firmanın siparişe donmuş IBAN'ı, ödenecek toplam, sipariş numarası ve son ödeme günü (`03 §1.11.13`, §2.6.4; §2.6.4, §2.6.6; Z-8); IBAN salt okunur bir bilgidir ve OB-14 değildir (§2.12.3.4; K-663); sitenin kesintisinde ertelenen sürede sayfa yeni son günü gösterir (`03 §3.5.2.17`, §7.2.14) · **kart ödemesinin beklendiği** — 3D Secure dönüşünde sonuç henüz gelmemişse; sonuç gelince bölge güncellenir (`03 §2.5.1.2`; K-704) · **kart ödemesinin gerçekleşmediği** — sipariş İptal edildi + Başarısız'dır ve sepet olduğu gibi durur; müşteri sepetten yeni bir siparişle yeniden dener, sepete çerçeveden gidilir (`03 §2.5.1.3`; 3.1.9) · **IBAN gereken kalem** — bir ya da daha çok kalemde IBAN alanı açıksa, sayısıyla ve kalemlerine götüren bir bağlantıyla (5.16.12).
  - **5.16.5 Kalem satırı** — siparişe donmuş hâliyle (`02 §3.23.1`): görsel, ürün adı ve varyantın seçenek değerleri · adet — adetlerinin bir kısmı kapanmış kalemde açık adetle birlikte (K-790) —, birim fiyat ve satır tutarı · cayma istisnası işaretli kalemde istisna ve sebebi — koşullu sebepte koşulu (`02 §7.3.4`) · **kalemin kayıtları**, kaydın adı, tarihi ve — adedi birden büyük kalemde — kapsadığı adetle; aynı türden ardışık kayıtlar ayrı ayrı görünür (OB-05, §2.5.2; K-790, K-795): teslim işareti ve teslim tarihi · iptal kaydı · çıkarma kaydı · gecikme feshi · cayma beyanı — fiziksel kalemde beyana yazılan iade adresiyle; adres sonradan değişse de o beyan için değişmez (`02 §3.1.7`; K-665) · iade teslim alma · iade reddi — reddin sebebiyle ve malın siparişteki teslimat adresine firmanın bedeliyle geri gönderileceğiyle (`02 §7.4.9`; K-657) · "mal dönmedi" kapanışı · **ayıp talebi** — durum rozetiyle (Açık · Çözüldü; §2.5.1.5) ve açılış ya da yeniden açılış tarihiyle; talep Açık'ken "sorun bildir"in yerinde talebin durumu durur (`03 §2.9.3`; K-676) · **IBAN alanı** — istekle açıldığında (5.16.12) · **dijital kalemde indirme düğmesi** (5.16.13). Kalem ayrı bir durum rozeti taşımaz (§2.5.2). Bir kaleme dokunan geri ödeme kalemin satırında değil, ödeme ekseninin rozetinde okunur (`02 §5.5`).
  - **5.16.6 İşlemler** — sayfanın tek bölümünde, o anda açık olanlar; açık olmayan işlem hiç gösterilmez (`03 §1.11`): **siparişi iptal etmek** — yalnız Alındı + Bekliyor'da · **kalem iptali** · **gecikme nedeniyle fesih** · **cayma** · **"sorun bildir"**. Üç yol — iptal, cayma, ayıp talebi — ayrı düğmelerdir ve ayrı kayıt taşır (`03 §5.1.1`; `02 §7.1.1`). Her düğme seçimi kendi ekranında yaptırır; kalemin satırı işlemi başlatmaz, yalnız sonucunu gösterir (5.16.5).
  - **5.16.7 Tutarın dökümü** — donmuş değerlerle (`02 §3.23.2`): kalemlerin toplamı, indirim ve kuponun payı, KDV, kargo ücreti ya da "Kargo ücretsiz", ödenen ya da ödenecek toplam ve ödeme yöntemi. Kısmi iptalden ve iadeden sonra döküm yeniden hesaplanmaz (`03 §3.3.10`; `02 §7.2.7`, §7.4.6).
  - **5.16.8 Teslimat** — fiziksel kalem varsa: kargoya verildikten sonra kargo şirketi ve takip numarası, listedeki şirkette "Takip et" bağlantısı; "Diğer"de yalnız şirket adı ve takip numarası; ya da "kendi aracımızla teslim" beyanı (`02 §3.20.7`, §3.20.8; `03 §2.5.3.3`) · teslim edildiyse teslim tarihi (`03 §2.5.3.5`) · Teslim edilemedi'de gönderinin firmaya döndüğü (`03 §2.5.3.4`). Fiziksel kalemsiz siparişte bölge yoktur; sipariş Hazırlanıyor ve Kargoya verildi'yi kullanmaz (`03 §2.5.3.7`).
  - **5.16.9 Belgeler** — sayfanın içinde açılıp kapanan bölümler; hiçbiri bir dış bağlantının ya da dosyanın arkasında değildir: siparişin onaylandığı sürümleriyle Ön Bilgilendirme Formu ve Mesafeli Satış Sözleşmesi — uyuşmazlık yolları ikisinin içindedir (`03 §2.6.4`, §5.1.5; `02 §3.33.7`) · siparişin onaylandığı gün yürürlükte olan aydınlatma metni (`02 §3.23.3`, §3.33.3) · dijital ve hizmet kalemi için işaretlenen onay kutularının kaydı — kutunun gösterilen metni ve kapsadığı kalemler, salt okunur (§2.12.7.5; `02 §3.24.6`; K-595). Donmuş sürümler sonradan değişen metinden etkilenmez (`02 §3.33.4`).
  - **5.16.10 Yoktur:** fatura — firma onu kendi kanalından iletir (`02 §3.25.2`; `03 §2.6.4`) · siparişin e-posta geçmişi — panelde durur, müşteriye görünmez (K-586) · müşterinin siparişin adresini ya da e-postasını değiştirmesi — ikisini firma, müşterinin talebi üzerine teyitle düzeltir; yol iletişim formudur (`02 §8.3.6`, §8.3.7; 5.10.9) · "hakkımı yenile" düğmesi (`02 §3.12.6`) · sipariş notu ve ek yükleme (`02 §3.24.7`, §8.4.1) · ayrı bir şikâyet kanalı — üç yolun dışındaki her konu iletişim formundandır (`02 §7.1.5`). Sayfa arama motorlarına kapalıdır (`03 §2.6.4`; §4.4.5).
- **Aksiyonlar** — işlemin açık olduğu aralık `03 §1.11`'dedir ve burada yinelenmez; aralığın dışında düğme sunulmaz:
  - **5.16.11 İptal, fesih, cayma ve "sorun bildir"i başlatmak** — sayfada onay istemez; işlemin kendi ekranını açar ve onay orada, son adımdadır (§2.4.5; K-732): **siparişi iptal etmek** → E-17 (`03 §1.11.1`, §2.7.1) — ödeme onayından önce kalem iptali sunulmaz (`03 §3.3.23`; K-505) · **kalem iptali** → E-17 (`03 §1.11.2`, §1.11.3) — dijital kalemde yoktur (`03 §1.11.4`); sipariş kargoya verildikten sonra fiziksel kalemde sunulmaz, yol caymadır (`03 §3.3.1`) · **gecikme nedeniyle fesih** → E-17 (`03 §1.11.8`; K-499) — sipariş kargodayken de açıktır (`03 §3.3.2`) · **cayma** → E-18 (`03 §1.11.5`, §1.11.6) — hizmet kaleminde ödeme onayından sonra açılır ve karışık siparişte fiziksel kalemler teslimden önce kapanmışsa "tamamlandı" işaretine kadar sürer (K-673; K-593; `03 §3.3.29`); dijital kalemde, mutlak istisnada, tamamlanmış hizmette ve penceresi geçmiş kalemde sunulmaz (`03 §1.11.7`, §3.3.3–§3.3.6) · **"sorun bildir"** → E-19 (`03 §1.11.9`) — Çözüldü talepte talebin satırındaki yeniden açma düğmesi → E-19 (`03 §1.11.10`).
  - **5.16.12 Geri ödeme IBAN'ını girmek ya da düzeltmek** — sayfada kalır (`03 §1.11.11`, §1.11.12): IBAN isteği doğunca alan o kalemin satırında açılır (OB-14, istekle açılan alan; §2.12.3.2) ve alanın üstünde neden istendiği söylenir — iade malı ulaştı ve IBAN silinmişti · geri ödeme havalesi bankada gerçekleşmedi · başka kanaldan bildirilen cayma ya da fesih IBAN taşımıyordu · firma kalemi iptal etti ya da çıkardı · firma bir geri ödemeye karar verdi (`02 §7.4.5`, §9.2 B-14; K-497, K-498, K-575, K-686, K-690). Kart iadesi sağlayıcıda gerçekleşmeyip firma havale yolunu açtıysa alan önce kart iadesinin gerçekleşmediğini söyler; IBAN girmek müşterinin seçimidir ve alan boş bırakılabilir (`03 §2.8.4.3`; K-569). Kaydetme onay istemez; alanın yanında aydınlatma metninin bağlantısı durur (`02 §3.33.5`). Kayıttan sonra girilen IBAN alanda görünür ve geri ödeme işlenene kadar düzeltilir; işlenince alan kalkar (`02 §7.4.5`). Kaydetme e-posta üretmez (`02 §9.2`; K-710); başarı kısa süreli bildirimle söylenir — sonuç alanda görünür (§2.3.1.4). Alan e-postanın ulaşmasından bağımsızdır (`03 §3.3.20`).
  - **5.16.13 Dijital dosyayı indirmek** (`03 §1.11.14`, §2.5.3.2): kalem satırındaki düğme kalemin bağlı olduğu varyantın güncel dosyasını indirir (`02 §3.12.7`). Hak dolunca düğmenin yerinde yerinde kalıcı mesaj durur: hakkın dolduğu ve yenileme için iletişim formunun "Sipariş hakkında" tipiyle başvurulacağı; forma çerçeveden gidilir (`02 §3.12.6`; `03 §3.5.2.7`; 3.1.16). Havalede indirme firmanın "ödendi" işaretiyle açılır (`02 §3.12.1`).
  - **5.16.14 Havale bilgisini kullanmak** — IBAN'ı kopyalamak; kısa süreli bildirimle söylenir (5.14.7'nin kalıbı) · **"Takip et"** → kargo şirketinin sayfası.
  - **5.16.15 Eşzamanlı değişiklik.** Müşteri işlemin ekranındayken sipariş değişir ve işlem geçersizleşirse — sipariş kargoya verildi, kalem tamamlandı, kalem başka bir işlemle kapandı — onay uygulanmaz; ekran sebebini söyler ve sipariş sayfasının güncel hâlini gösterir (OB-10, §2.10.2; `02 §3.31.2`).
- **5.16.16 Validasyonlar.** IBAN girişte biçim ve sağlama basamağıyla denetlenir; hata alan mesajıdır (OB-14, §2.12.3.1; `02 §7.4.5`). Sayfanın başka formu yoktur. Alan envanteri §7.1'dedir (5. oturum).
- **Durum × rol varyantları** (tam matris §6.2'de — 5. oturum)
  - **5.16.17 İki eksenin hâlleri** `03 §1.4.1`'in izinli çiftleridir ve sayfa her birinde iki rozeti ve o an açık işlemleri gösterir: Alındı + Bekliyor — havalede ödeme bilgisi, kartta ödemenin beklendiği · Hazırlanıyor + Ödendi · Kargoya verildi · Teslim edilemedi · Teslim edildi · Kısmen geri ödendi ve Geri ödendi · İptal edildi + Başarısız. Kendiliğinden iptal edilmiş ödenmemiş sipariş üyenin sipariş geçmişinde görünmez, e-postadaki bağlantıyla açılır (`02 §3.17.8`; `03 §2.5.1.3`). Kart son sorgusu yanıtsız kalan sipariş Alındı + Bekliyor'da ödemenin beklendiğini söyler (`03 §2.5.1.4`). Kalem kayıtları ve kapanış kuralı rozetleri `03 §1.4.3`'e göre değiştirir; sayfa kendi başına durum türetmez.
  - **5.16.18 Aktör farkı yoktur:** üye ve misafir alıcı aynı sayfayı görür; fark girişin yoludur. Hesabı silinmiş müşterinin ve e-postası değişmiş hesabın siparişi aynı sayfayla, misafir yolundan ya da e-postadaki bağlantıyla açılır (`03 §2.6.6`, §9.4.3). Satış kapısının kapanması ve ürünün arşive alınması açık siparişi etkilemez (`03 §1.10.8`; `02 §3.23.1`). Yönetici şeridi çerçevenin parçasıdır (§2.9.1).
- **Boş / yükleniyor / hata durumları** (OB-07)
  - **5.16.19** Ekranın boş hâli yoktur: sipariş en az bir kalem taşır. Ödeme sonucu beklenen sipariş bir yükleniyor hâli değildir (§2.7.2). E-postanın ulaşmaması sayfayı etkilemez — sayfa her bilgiyi taşır (`03 §2.6.5`, §3.1.1, §10.1.2.1).
  - **5.16.20** Geçersiz erişim anahtarıyla — misafir siparişinin e-postası düzeltildikten sonraki eski bağlantı (`02 §3.22.5`; `03 §6.2.4.2`) — ya da kişisel verileri imha edilmiş siparişin bağlantısıyla açılan adres var olmayan adrestir ve E-06 döner (3.5.13); sayfa siparişin varlığını açığa çıkarmaz. IBAN kaydı tamamlanmazsa §2.7.3.1; sayfa açılamazsa §2.7.3.2.
- **5.16.21 Responsive notları.** Geniş sınıfta iki sütundur: solda başlık, müşteriden beklenen, kalemler ve işlemler; sağda tutarın dökümü, teslimat, adresler; belgeler altta tam genişlikte. Dar sınıfta 5.16.3'ün sırasıyla tek sütundur; kalem satırı görsel ile adı bir satıra, kayıtları altına alır. İki rozet hiçbir sınıfta tek rozete inmez; IBAN dar sınıfta satır kırmadan kopyalanabilir tek blokta durur.

*Kaynak: `02 §3.1.7`, §3.12.1, §3.12.6, §3.12.7, §3.17.8, §3.20.7, §3.20.8, §3.22.3–§3.22.5, §3.23.1–§3.23.3, §3.24.6, §3.24.7, §3.25.2, §3.31.2, §3.32.2, §3.33.3, §3.33.4, §3.33.5, §3.33.7, §5.4, §5.5, §7.1.1, §7.1.5, §7.2.7, §7.3.4, §7.4.5, §7.4.6, §7.4.9, §8.3.6, §8.3.7, §9.2 · `03 §1.4`, §1.5, §1.7.1, §1.10.8, §1.11.1–§1.11.14, §2.5.1.2–§2.5.1.4, §2.5.2.1, §2.5.3, §2.6, §2.7, §2.8, §2.9, §3.1.1, §3.2.1.16, §3.2.2.1, §3.2.2.3, §3.3, §3.5.2.7, §3.5.2.17, §4.1, §5.1.1, §5.1.5, §6.2.4.2, §6.2.4.3, §7.1.29–§7.1.34, §7.2.14, §7.2.30, §10.1.2.1 · `10 §2` KP-15, KP-17–KP-24, KP-74 · K-732, K-744, K-777, K-790, K-795 · devir: K-494, K-497, K-498, K-499, K-503, K-505, K-553, K-569, K-575, K-586 (e-posta geçmişinin yokluğu), K-593, K-595, K-657 (reddedilen kalemin görünümü), K-663 (donmuş IBAN), K-665, K-673, K-676, K-686, K-690, K-704 (kart dönüşünün bekleme hâli); K-777 (iki rozetin yazımı).*

### 5.17 E-17 — İptal ve gecikme feshi ekranı

- **5.17.1 Aktör · giriş · çıkış.** Müşteri. Giriş: sipariş sayfasının iptal ve gecikme feshi düğmeleri (3.3.22). Çıkış: son adımın onayı ve "Vazgeç" → E-16 (3.3.25).
- **5.17.2 İlk görülen:** işlemin adı ve işlemin kapsadığı kalemler; birincil düğme son adımın onay düğmesidir.
- **Bilgi hiyerarşisi** — ekran üç işlemden birini, aynı iskeletle taşır (K-744):
  - **5.17.3** (1) İşlemin adı — siparişin iptali · kalem iptali · gecikme nedeniyle fesih · (2) kapsam (5.17.4) · (3) geri ödeme ve — havale hattında — IBAN alanı (5.17.5) · (4) son adım (5.17.6).
  - **5.17.4 Kapsam.** **Siparişin iptali** — Alındı + Bekliyor'da siparişin bütün kalemleri, dijital kalem dahil, salt okunur listedir; ödeme onaylanmadığı için kalem seçimi ve IBAN yoktur (`03 §2.7.1`; `02 §7.2.2`). **Kalem iptali** — ödenmiş siparişte iptali açık kalemler seçilebilir listedir: fiziksel kalem sipariş Kargoya verildi'ye geçene kadar, hizmet kalemi "tamamlandı" işaretine kadar (`03 §1.11.2`, §1.11.3); dijital kalem ve kapanmış kalem listede yer almaz (`03 §1.11.4`). Müşteri bir ya da birden çok kalemi aynı işlemde seçer — iptal tek seferde ya da ardışık yapılır (`02 §7.2.9`); adedi birden büyük kalemde seçilen kalemin satırında adet alanı açılır (OB-12, 2.12.1.8): üst sınırı kalemin iptale açık adedidir, varsayılanı onun tamamıdır; bir kısmı daha önce kapanmış kalem kalan adediyle listede kalır (`02 §7.1.3`, §7.1.6; K-787, K-795). **Gecikme feshi** — seçim yoktur: fesih teslim edilmemiş fiziksel kalemlerin açık adetlerinin tamamına uygulanır ve liste bunları açık adetleriyle salt okunur gösterir — kalem ve adet seçilmez, adet alanı yoktur (`02 §7.1.6`; K-796); istisna işaretli kalem de feshin içindedir — fesih cayma değildir; teslim edilmiş dijital ve hizmet kalemleri ile tamamlanmamış hizmet kalemi feshin dışındadır ve listede yer almaz (`02 §7.2.3`; `03 §2.7.7`).
  - **5.17.5 Geri ödeme.** Ekran paranın hangi yoldan döneceğini söyler: kart hattında karta — sistem kendiliğinden başlatır; havale hattında müşterinin IBAN'ına — firma işler (`02 §7.2.8`). Süre gün olarak söylenir (§2.6.6): iptalde Z-17, fesihte `02 §7.2.3`'ün süresidir. Seçim siparişin kargoya verilmemiş bütün fiziksel kalemlerinin bütün açık adetlerini kapsıyorsa ekran ödenmiş kargo ücretinin de geri ödeneceğini söyler (`02 §7.2.9`; K-789); fesihte kargo ücreti her durumda geri ödenir (`02 §7.2.3`). Siparişin iptalinde geri ödeme satırı yoktur. **IBAN alanı** (OB-14, beyanla giriş; §2.12.3.2) yalnız ödenmiş havale siparişinde çıkar ve zorunludur; boş gelir — önceki bir işlemin IBAN'ı taşınmaz (`02 §7.4.5`); yanında aydınlatma metninin bağlantısı durur (`02 §3.33.5`). Kart hattında IBAN alanı yoktur.
  - **5.17.6 Son adım** (OB-04, müşterinin son adımı; §2.4.5) — sayfanın içinde, kapsamın ve IBAN alanının altında: neyin iptal edildiği ya da feshedildiği — seçilen kalemler adıyla ve adetle · sonuç cümlesi (K-785) — sayısı seçilen adetlerin toplamıdır (K-795) · "Bu işlem geri alınamaz" · işlemin adını taşıyan onay düğmesi — "Siparişi iptal et", "Seçilen ürünleri iptal et" ya da "Gecikme nedeniyle feshet" (K-744, K-785) · E-16'ya dönen "Vazgeç". Vazgeçirme metni, bekleme, ek soru ve ikinci kanal yoktur; iptal ve fesih firma onayı beklemez (K-732; `02 §7.2.1`).
  - **5.17.7 Yoktur:** sebep alanı — müşterinin iptali ve feshi sebep istemez; sebep yalnız firma iptalinde seçilir (`02 §7.2.4`) · kanuni faiz bilgisi — uyarı panelde firmaya gösterilir (`02 §7.2.3`; `03 §3.3.2`) · ödenmemiş siparişte kalem iptali — müşteri siparişi iptal edip yeniden verir (`03 §3.3.23`; K-505).
- **Aksiyonlar**
  - **5.17.8 Kalemleri ve adetlerini seçmek ve — havalede — IBAN'ı yazmak.** Seçim ya da adet değişince son adımın kalem listesi, sonuç cümlesi ve kargo ücreti satırı yerinde güncellenir. Onay istemez.
  - **5.17.9 Onaylamak** (`03 §2.7.1`–§2.7.3, §2.7.7; 3.3.25). Kalemler iptal kaydını ya da gecikme feshini alır ve müşteri E-16'ya döner; sonuç orada kalıcı durur — kalemlerin kaydı ve, kapanış kuralı işlediyse, siparişin güncel rozetleri (`03 §1.4.3`; §2.3.4). Kayıt kopyası e-postayla gider — iptalde B-7, fesihte B-9 (`02 §7.1.4`). Sonuç kısa süreli bildirimle verilmez (§2.3.4).
  - **5.17.10 "Vazgeç"** → E-16; hiçbir kayıt doğmaz ve girilen IBAN saklanmaz.
  - **5.17.11 İşlem o arada kapandıysa** — sipariş kargoya verildi, kalem tamamlandı ya da başka bir işlemle kapandı — onay uygulanmaz; ekran sebebini söyler ve E-16'nın güncel hâline götürür (5.16.15; §2.10.2).
- **5.17.12 Validasyonlar.** Kalem iptalinde en az bir kalem seçilmeden onaylanmaz; eksik seçim alan mesajıdır. Adet 2.12.1.8'in sınırlarındadır. Havale hattında IBAN zorunludur ve biçim ile sağlama basamağıyla denetlenir (OB-14). Gönderimde hatalar birlikte işaretlenir (§2.3.2). Alan envanteri §7.1'dedir (5. oturum).
- **5.17.13 Durum × rol varyantları.** Ekranın üç işlemi siparişin durumuna göre açılır (`03 §1.11.1`–§1.11.3, §1.11.8); kart ve havale hattı yalnız 5.17.5'te ayrılır. Üye ile misafir alıcı arasında fark yoktur. Tam matris §6.2'dedir (5. oturum).
- **5.17.14 Boş / yükleniyor / hata durumları.** Ekranın boş hâli yoktur: açık işlemi olmayan siparişte sipariş sayfası düğmeyi sunmaz (`03 §1.11`); o arada kapanmış işlemde 5.17.11 işler. Onay sürerken düğme ikinci kez basılamaz ve sürdüğünü metinle söyler (§2.7.2). Onay beklenmeyen bir sebeple sonuçsuz kalırsa ekran "işlem yapılmadı" demez; müşteriyi E-16'ya, sonucu görmeye götürür (§2.7.3.3).
- **5.17.15 Responsive notları.** Tek sütundur; dar sınıfta kalem listesi görsel ve adla kısalır. Son adımın dört parçası — kalemler, sonuç cümlesi, "geri alınamaz" cümlesi ve iki düğme — hiçbir sınıfta birbirinden ayrılmaz.

*Kaynak: `02 §3.33.5`, §7.1.3, §7.1.4, §7.1.6, §7.2.1–§7.2.4, §7.2.8, §7.2.9, §7.4.5 · `03 §1.4.3`, §1.7.1.2, §1.7.1.4, §1.11.1–§1.11.3, §1.11.8, §1.11.11, §2.5.2.6, §2.7, §3.3.2, §3.3.7, §3.3.23 · `10 §2` KP-21, KP-24, KP-74 · K-732, K-744, K-785, K-787, K-789, K-795, K-796 · devir: K-499, K-505.*

### 5.18 E-18 — Cayma beyanı ekranı

- **5.18.1 Aktör · giriş · çıkış.** Müşteri. Giriş: sipariş sayfasının cayma düğmesi (3.3.23). Çıkış: son adımın onayı ve "Vazgeç" → E-16 (3.3.25).
- **5.18.2 İlk görülen:** caymanın açık olduğu kalemler; birincil düğme son adımın onay düğmesidir.
- **Bilgi hiyerarşisi**
  - **5.18.3** (1) İşlemin adı — cayma beyanı · (2) kalem seçimi (5.18.4) · (3) fiziksel kalemde iade bilgisi (5.18.5) · (4) geri ödeme ve — havale hattında — IBAN alanı (5.18.6) · (5) son adım (5.18.7).
  - **5.18.4 Kalem seçimi** — caymanın açık olduğu kalemler seçilebilir listedir (`03 §1.11.5`, §1.11.6): fiziksel kalem sipariş Kargoya verildi'ye geçtikten sonra, penceresi içinde — teslim tarihi girilmemişse açıktır, mal yoldayken de (`03 §2.8.1.1`, §3.3.11) · hizmet kalemi ödeme onayından sonra, "tamamlandı" işaretine ya da Z-14'ün dolmasına kadar (`03 §2.8.2.1`; K-673, K-593). Mutlak istisna işaretli kalem, dijital kalem ve tamamlanmış hizmet listede yer almaz (`03 §1.11.7`, §3.3.3–§3.3.5). **Koşullu istisna işaretli kalemin satırı** kaynağın koşulunu yazar: *"koruyucu ambalajı açılmamışsa cayabilirsiniz"* (`02 §7.3.4`; `03 §5.3.1.1`; K-509). Müşteri bir ya da birden çok kalemi aynı beyanda seçer — cayma tek seferde ya da ardışık beyanlarla yapılır (`02 §7.4.6`); adedi birden büyük kalemde seçilen kalemin satırında adet alanı açılır (OB-12, 2.12.1.8): üst sınırı kalemin caymaya açık adedidir, varsayılanı onun tamamıdır; bir kısmından daha önce cayılmış kalem kalan adediyle listede kalır ve yeni beyan ayrı bir kayıttır (`02 §7.1.3`, §7.1.6; K-787, K-790, K-795).
  - **5.18.5 İade bilgisi** — seçimde fiziksel kalem varsa: firmanın **güncel iade adresi** — beyanla birlikte kayda yazılır ve sonradan değişmez (`02 §3.1.7`, §7.3.5; K-665) · malın beyandan itibaren gönderilmesi gereken süre, gün olarak ve son günün tarihiyle (§2.6.4, §2.6.6; Z-42) · malın, müşterinin seçtiği taşıyıcıyla, iade adresine karşı ödemeli gönderileceği — masrafı teslim alırken firma öder (`02 §7.4.2`, §7.4.4; K-494). Yalnız hizmet kalemi seçildiyse bölüm yoktur: geri gönderilecek mal yoktur (`02 §7.3.5`; `03 §2.8.2.2`; K-578).
  - **5.18.6 Geri ödeme.** Ekran paranın hangi yoldan ve hangi andan itibaren döneceğini söyler: kart hattında karta, havale hattında müşterinin IBAN'ına (`02 §7.4.5`); süre (Z-16) teslimden önceki caymada ve hizmette beyandan, teslimden sonraki caymada malın firmaya ulaşmasından işler (`02 §7.4.1`). Kısmen ifa edilmiş hizmetten caymada cayılan adetlerin ödenmiş bedelinin tamamı geri ödenir (`02 §7.3.3`; K-578, K-792). Kargoyla gönderilen fiziksel kalemlerin bütün adetleri — önceki beyanlarla birlikte — seçildiyse ekran kargo ücretinin de geri ödeneceğini söyler (`02 §7.4.6`; `03 §3.3.13`; K-789). **IBAN alanı** 5.17.5'in kuralıyla — yalnız havale hattında, zorunlu, boş gelir, aydınlatma bağlantısıyla (OB-14; `02 §7.3.5`, §3.33.5).
  - **5.18.7 Son adım** (OB-04, müşterinin son adımı; §2.4.5): hangi kalemden cayıldığı — kalemler adıyla ve adetle · sonuç cümlesi (K-785) — sayısı seçilen adetlerin toplamıdır (K-795) · "Bu işlem geri alınamaz" — cayma beyanı geri alınmaz ve düzeltilmez (`02 §7.6.7`) · "Cayma beyanını gönder" (K-785) · E-16'ya dönen "Vazgeç". Vazgeçirme metni, bekleme ve ek soru yoktur (K-732, K-744).
  - **5.18.8 Yoktur:** cayma sebebi alanı — cayma sebepsizdir ve ayrı bir form doldurma zorunluluğu yoktur (`02 §7.3.5`, §7.1.1) · iade etiketi, taşıyıcı seçimi ve takip numarası alanı (`02 §7.4.4`) · fotoğraf ve dosya yükleme (`02 §8.4.1`).
- **Aksiyonlar**
  - **5.18.9 Kalemleri ve adetlerini seçmek ve — havalede — IBAN'ı yazmak.** Seçim ya da adet değişince iade bilgisi bölümü, son adımın kalem listesi ve sonuç cümlesi yerinde güncellenir.
  - **5.18.10 Onaylamak** (`03 §2.8.1.2`, §2.8.2.2; 3.3.25). Kalemler cayma beyanını tarih damgasıyla alır ve müşteri E-16'ya döner; sonuç kalemin satırında kalıcı durur — beyanın tarihi ve fiziksel kalemde beyana yazılan iade adresi (5.16.5). Teslim işaretsiz kalemi beyan kapatır ve kapanış kuralı işler (`03 §1.4.3`, §2.8.1.2; `02 §5.6.6`). Kayıt kopyası B-9'la gider — beyanın tarihi ve iade adresi (`02 §9.2`).
  - **5.18.11 "Vazgeç"** → E-16; hiçbir kayıt doğmaz. **Pencere ya da hak o arada kapandıysa** — teslim tarihi geç girildi ve pencere dolmuş görünüyor, hizmet tamamlandı — onay uygulanmaz ve ekran sebebini söyler (5.17.11'in kalıbı; `03 §3.3.12`).
- **5.18.12 Validasyonlar.** En az bir kalem seçilmeden onaylanmaz; adet 2.12.1.8'in sınırlarındadır; havalede IBAN 5.17.12'nin kuralıyla. Alan envanteri §7.1'dedir (5. oturum).
- **5.18.13 Durum × rol varyantları.** Kalem tipine göre: fiziksel kalemde iade bilgisi, hizmette iade bilgisi yok; koşullu istisnada koşul satırı. Siparişin durumuna göre: Kargoya verildi, Teslim edilemedi ve Teslim edildi'de fiziksel kalem; ödeme onayından sonra hizmet kalemi (`03 §1.11.5`, §1.11.6). Üye ile misafir alıcı arasında fark yoktur. Tam matris §6.2'dedir (5. oturum).
- **5.18.14 Boş / yükleniyor / hata durumları.** Ekranın boş hâli yoktur (5.17.14'ün kalıbı). Onay sürerken düğme ikinci kez basılamaz; sonucu belirsiz onay müşteriyi E-16'ya götürür (§2.7.3.3). Pencere kapandıktan sonra ya da mutlak istisna kaleminde cayma isteyen müşterinin yolu iletişim formudur ve değerlendirme firmanındır (`03 §3.3.32`, §5.2.3.1; 5.10.9).
- **5.18.15 Responsive notları.** Tek sütundur; iade adresi dar sınıfta satır kırar, kesilmez. Son adımın parçaları 5.17.15'in kuralıyla birlikte durur.

*Kaynak: `02 §3.1.7`, §3.33.5, §5.6.6, §7.1.1, §7.1.3, §7.3.1–§7.3.5, §7.4.1, §7.4.2, §7.4.4–§7.4.6, §7.6.7, §8.4.1, §9.2 · `03 §1.7.1.5`, §1.11.5–§1.11.7, §2.8.1.1, §2.8.1.2, §2.8.2, §3.3.4, §3.3.7, §3.3.11–§3.3.14, §3.3.21, §3.3.28, §4.1.18, §5.3.1.1 · `10 §2` KP-22, KP-24, KP-74 · K-732, K-744, K-785, K-787, K-789, K-790, K-792, K-795 · devir: K-494 (beyan ekranı), K-503, K-509 (koşul satırı), K-578 (hizmet kaleminin beyanı), K-665 (güncel iade adresi).*

### 5.19 E-19 — Ayıp talebi ekranı

- **5.19.1 Aktör · giriş · çıkış.** Müşteri. Giriş: sipariş sayfasının "sorun bildir"i ve Çözüldü talebin yeniden açma düğmesi (3.3.24). Çıkış: gönderim ve "Vazgeç" → E-16 (3.3.25).
- **5.19.2 İlk görülen:** talebin açılabileceği kalemler ve açıklama alanı; yeniden açmada talebin kendisi. Birincil düğme gönderimdir.
- **Bilgi hiyerarşisi** — iki hâl:
  - **5.19.3 Yeni talep:** (1) kalem seçimi — talebin açık olduğu kalemlerden biri: fiziksel kalemde sipariş Kargoya verildi'ye geçtikten, dijitalde ödeme onayından, hizmette "tamamlandı" işaretinden sonra ve kalemde talep yokken (`03 §1.11.9`, §2.9.1; K-676); bir talep tek kalemi taşır (`02 §5.10`); adedi birden büyük kalemde ayıplı adet de seçilir (OB-12, 2.12.1.8) — varsayılanı birdir, üst sınırı kalemin teslim edilmiş ve cayma beyanı taşımayan adedidir (`02 §7.5.2`; K-793, K-795) · (2) sorunun açıklaması — zorunlu (`02 §7.5.2`) · (3) fiziksel kalemde firmanın güncel iade adresi — talebe yazılır (`02 §3.1.7`; `03 §2.9.2`; K-665) · (4) aydınlatma metninin bağlantısı (`02 §3.33.5`) · (5) gönderme düğmesi.
  - **5.19.4 Yeniden açma:** (1) talebin kalemi ve ayıplı adedi, ilk açıklaması, açılış tarihi ve Çözüldü rozeti (§2.5.1.5) · (2) yeniden açmanın yeni bir talep açmadığı ve ayıbın geçmişinin tek kayıtta kaldığı (`02 §5.10`) · (3) ek bilginin firmanın e-postasına yazılacağı — firmanın iletişim e-postasıyla; açıklama alanı yoktur (K-783; `03 §2.9.3`'ün kalıbı) · (4) yeniden açma düğmesi.
  - **5.19.5 Yoktur:** fotoğraf ve dosya yükleme — firma kanıtı e-postayla ister (`02 §7.5.2`; §2.12.8.6) · seçimlik hakkın — onarım, değişim, bedel indirimi, sözleşmeden dönme — ekranda seçilmesi; çözüm firmanın müşteriyle sistemin dışında yürüttüğü süreçtir (`02 §7.5.3`; `03 §2.9.4`) · onay adımı — talep geri alınamaz dört işlemden biri değildir (§2.4.2.4).
- **Aksiyonlar**
  - **5.19.6 Talebi göndermek** (`03 §2.9.2`). Onay istemez. Talep Açık doğar ve kalem ve sipariş bağlamıyla panele düşer (`02 §7.5.2`); müşteri E-16'ya döner ve sonuç kalemin satırında kalıcı durur — talebin rozeti ve tarihi; "sorun bildir"in yerinde talebin durumu görünür (`03 §2.9.3`; 5.16.5). Kayıt kopyası B-9'la gider — talebin tarihi ve fiziksel kalemde iade adresi (`02 §9.2`).
  - **5.19.7 Talebi yeniden açmak** (`03 §2.9.6`, §1.11.10). Onay istemez; talep Çözüldü → Açık olur, müşteri E-16'ya döner ve B-9 yeniden açmanın tarihini taşır (K-681).
  - **5.19.8 "Vazgeç"** → E-16; yazılan açıklama saklanmaz.
- **5.19.9 Validasyonlar.** Kalem seçilmeli, adet 2.12.1.8'in sınırlarında olmalı ve açıklama boş olmamalıdır — alan mesajı (OB-12). Alan envanteri §7.1'dedir (5. oturum).
- **5.19.10 Durum × rol varyantları.** Kalem tipine göre iade adresi satırı fiziksel kalemde vardır. Talep Açık'ken "sorun bildir" yoktur (K-676); Çözüldü'yken yeniden açma Z-18 içinde açıktır; Z-18 dolunca ikisi de sunulmaz ve yol iletişim formudur (`03 §4.1.21`, §5.1.4; 5.10.9). Teslim tarihi girilmemiş fiziksel kalemde süre işlemez (`02 §7.5.2`). Üye ile misafir alıcı arasında fark yoktur. Tam matris §6.2'dedir (5. oturum).
- **5.19.11 Boş / yükleniyor / hata durumları.** Ekranın boş hâli yoktur. Gönderim sürerken düğme ikinci kez basılamaz (§2.7.2); gönderim beklenmeyen bir sebeple tamamlanmazsa mesaj düğmenin yanında çıkar ve yazılan açıklama korunur (§2.7.3.1).
- **5.19.12 Responsive notları.** Tek sütundur; açıklama alanı her sınıfta formun tam genişliğindedir.

*Kaynak: `02 §3.1.7`, §3.33.5, §5.10, §7.5.1–§7.5.3, §9.2 · `03 §1.7.1.10`, §1.11.9, §1.11.10, §2.9, §3.3.15, §4.1.21, §5.1.4 · `10 §2` KP-23, KP-74 · K-681, K-783, K-793, K-795 · devir: K-553, K-665, K-676.*

### 5.20 E-20 — Kayıt ekranı

- **5.20.1 Aktör · giriş · çıkış.** Ziyaretçi. Giriş: giriş ekranının kayıt bağlantısı (3.3.26), doğrulama bağlantısının iniş ekranı (3.3.34), Google'ın e-postayı doğrulanmamış verdiği dönüş (3.5.9). Çıkış: kaydın gönderilmesi — ekranda kalır (3.3.28) · "Google ile giriş" → Google'ın sayfası (3.3.27) · çerçevenin "Giriş yap"ı (3.1.4).
- **5.20.2 İlk görülen:** ad, e-posta ve şifre alanları; birincil düğme kayıttır.
- **Bilgi hiyerarşisi**
  - **5.20.3** (1) Başlık · (2) "Google ile giriş" — Google uygulaması kurulumda tanımlıysa (`02 §3.13.6`; `03 §9.1.1`) · (3) form (OB-12): ad, e-posta, şifre — üçü zorunlu (`02 §3.13.21`, §3.13.15; K-605) · (4) aydınlatma metninin bağlantısı (`02 §3.33.5`) · (5) kayıt düğmesi.
  - **5.20.4 Gönderim sonrası hâli** — formun yerinde, sayfa değişmeden (3.3.28): doğrulama bağlantısının yazılan adrese gönderildiği, hesabın e-posta doğrulanınca açılacağı ve bağlantının geçerli olduğu son gün; altında bağlantıyı yeniden isteyen düğme (`03 §9.1.2`, §9.1.3; metinler K-779). Hâl giriş ve sipariş açmaz: doğrulanmamış kayıt bir bekleme aşamasıdır (`02 §3.13.4`).
  - **5.20.5 Yoktur:** pazarlama onayı kutusu (`02 §3.13.18`) · üyelik sözleşmesi ve onay kutusu (`02 §3.24.4`) · aydınlatmanın onay kutusu (§2.12.1.6) · şifrede büyük harf, rakam ve sembol dayatması (`02 §3.13.15`).
- **Aksiyonlar**
  - **5.20.6 Kaydolmak** (`03 §9.1.2`; 3.3.28). Onay istemez. Geçen kayıt doğrulanmamış bir kayıt açar ve ekran 5.20.4'ün hâline geçer; doğrulama bağlantısı kaydın adresine gider (`02 §9.4`).
  - **5.20.7 Bağlantıyı yeniden istemek** (`03 §9.1.3`; K-659): yeni bağlantı öncekileri geçersiz kılar ve kaydın ömrünü uzatmaz — hâl aynı son günü yineler (K-779). L-9 aşılınca düğmenin yanında nötr limit mesajı çıkar (`03 §3.4.14`; §2.3.5).
  - **5.20.8 "Google ile giriş"** — Google'ın sayfasına (3.3.27); dönüşte hesap doğrulanmış ve şifresiz doğar (`03 §9.1.6`) ya da Google e-postayı doğrulanmamış verdiyse kullanıcı bu ekrana, formun başındaki yönlendirme mesajıyla döner (`03 §9.1.7`; K-697; metni K-779).
- **5.20.9 Validasyonlar** (OB-12; alan envanteri §7.1 — 5. oturum). Ad boş olamaz · e-posta biçimi geçerli olmalıdır · şifre asgari uzunluğu (P-17) karşılamalı ve çok yaygın şifreler listesinde olmamalıdır — ret alan mesajıdır (`03 §3.4.6`) · e-posta bir müşteri hesabına — doğrulanmamış bekleyen kayıt dahil — aitse alan mesajı kaynağın metnini söyler: *"bu e-posta zaten kayıtlı"* ve çerçevenin "Giriş yap"ına götüren bağlantıyı taşır (`02 §3.13.11`; `03 §3.4.3`). Ekran hesabın varlığını burada bilerek ele verir; L-3 sorgulamayı daraltır (`02 §8.1.4`). L-3 aşılınca nötr limit mesajı (`03 §3.4.11`; §2.3.5).
- **Durum × rol varyantları** (tam matris §6.2'de — 5. oturum)
  - **5.20.10 Veri toplayan girişlerin kapısı kapalıyken** — aydınlatma metni tamamlanıp yayına alınmamışsa — form yerinde kapalı olduğunu söyler ve çalışmaz; Google ile yeni hesap da açılmaz (OB-08, §2.8.3; `03 §3.4.12`). Mevcut hesapların girişi etkilenmez.
  - **5.20.11** Google uygulaması kurulumda tanımlı değilse "Google ile giriş" görünmez ve üyelik e-posta ve şifreyle yürür (`03 §10.1.3.3`). Kayıt yalnız müşteri hesabı açar; yönetici hesabı davetle doğar (`02 §10.2.5`; K-588).
- **5.20.12 Boş / yükleniyor / hata durumları.** Ekranın boş hâli yoktur. Gönderim sürerken düğme ikinci kez basılamaz (§2.7.2); beklenmeyen hatada yazılanlar korunur (§2.7.3.1). E-posta altyapısının kesintisinde bağlantı ulaşmadan kayıt tamamlanmaz; kullanıcı altyapı dönünce bağlantıyı yeniden ister (`03 §10.1.2.5`).
- **5.20.13 Responsive notları.** Tek sütundur; "Google ile giriş" her sınıfta formun üstündedir.

*Kaynak: `02 §3.13.4`–§3.13.6, §3.13.11, §3.13.15, §3.13.18, §3.13.19, §3.13.21, §3.24.4, §3.33.5, §8.1.4, §9.4, §10.2.5 · `03 §1.10.5`, §3.1.1, §3.4.3, §3.4.6, §3.4.7, §3.4.11, §3.4.12, §3.4.14, §6.1.1.3, §6.1.1.9, §7.1.49, §9.1.1–§9.1.3, §9.1.6, §9.1.7, §9.4.7, §10.1.2.5, §10.1.3.3 · `10 §2` KP-25, KP-26, KP-74 · K-779 · devir: K-553, K-605 (ad alanı), K-659 (yeniden isteme mesajı), K-697 (yönlendirme mesajı).*

### 5.21 E-21 — Doğrulama bağlantısının iniş ekranı

- **5.21.1 Aktör · giriş · çıkış.** Kullanıcı. Giriş: kaydın doğrulama bağlantısı (3.5.4) ve hesabın yeni e-posta adresinin doğrulama bağlantısı (3.5.6). Çıkış: giriş → E-22 (3.3.29) · yeniden kayıt → E-20 (3.3.34) · hesap → E-25 (3.3.35).
- **5.21.2 İlk görülen:** bağlantının sonucunu söyleyen tek cümle ve sonraki adımın bağlantısı — ekranın birincil düğmesidir.
- **Bilgi hiyerarşisi** — ekran bağlantının hâline göre altı hâlden birini gösterir (K-780); her hâl yerinde kalıcı mesajdır (§2.3.1.2) ve tek bir sonraki adım taşır:
  - **5.21.3 Kayıt doğrulandı** — hesabın açıldığı; o e-postaya ait geçmiş misafir siparişleri varsa "Siparişlerim"e düştükleri (`03 §9.1.4`; `02 §3.13.2`). Sonraki adım: giriş (3.3.29) — doğrulama oturum açmaz.
  - **5.21.4 Kayıt bağlantısının süresi dolmuş** — kaynağın metni: *"bağlantı geçersiz, yeniden kayıt olun"*; kayıt silinmiştir ve e-posta yeniden kayda açıktır (`02 §3.13.5`; `03 §3.4.1`, §9.1.5). Sonraki adım: kayıt (3.3.34).
  - **5.21.5 Kayıt bağlantısının yerine yenisi gönderilmiş** — kayıt hâlâ bekliyor: bu bağlantının geçerliliğini yitirdiği ve en son gönderilen bağlantının kullanılacağı (K-659; K-780). Sonraki adım: giriş — doğrulanmamış hesapla girişte bağlantı yeniden istenir (5.22.8).
  - **5.21.6 Kayıt bağlantısı daha önce kullanılmış** — hesabın zaten doğrulandığı. Sonraki adım: giriş.
  - **5.21.7 Yeni e-posta adresi doğrulandı** — değişikliğin geçerli olduğu, hesabın bundan sonra yeni adresle çalıştığı ve önceki adrese bildirim gittiği; yeni adrese ait geçmiş misafir siparişleri varsa hesaba düştükleri (`03 §9.3.5`). Sonraki adım: oturum açıksa "Hesabım", değilse giriş (3.3.35).
  - **5.21.8 Yeni e-posta bağlantısı geçersiz** — süresi dolmuş, yerine yeni bir istek geçmiş ya da kullanılmış: değişikliğin bu bağlantıyla geçerli olmadığı; hesap önceki adresiyle çalışır ve değişiklik hesap ekranından yeniden başlatılır (`03 §3.4.9`, §4.1.1; K-734). Kullanılmış bağlantıda değişiklik zaten geçerlidir. Sonraki adım: 5.21.7'nin bağlantısı.
- **5.21.9 Aksiyonlar:** yalnız sonraki adımın bağlantısı; ekranın formu ve onayı yoktur. Bağlantının hangi hâlde olduğunun tespiti Teknik Mimari'nin işidir (`05`).
- **5.21.10 Validasyonlar:** form yoktur.
- **5.21.11 Durum × rol varyantları.** Ekran oturumdan bağımsızdır; yalnız 5.21.7 ve 5.21.8'in sonraki adımı oturuma göre değişir. Yönetici hesabının yeni adres doğrulaması bu ekrana değil E-54'e iner (3.5.6). Tam matris §6.2'dedir (5. oturum).
- **5.21.12 Boş / yükleniyor / hata durumları.** Ekranın boş hâli yoktur; tanınmayan bağlantı 5.21.4'ün hâlini gösterir. Sayfa açılamazsa §2.7.3.2.
- **5.21.13 Responsive notları.** Tek sütundur; sınıflar arasında fark yoktur.

*Kaynak: `02 §3.13.2`, §3.13.4, §3.13.5, §3.13.14 · `03 §1.10.5`, §3.4.1, §3.4.9, §4.1.1, §7.1.49, §7.1.51, §9.1.4, §9.1.5, §9.3.5 · `10 §2` KP-25 · K-734, K-780 · devir: K-659.*

### 5.22 E-22 — Müşteri girişi

- **5.22.1 Aktör · giriş · çıkış.** Ziyaretçi · Üye. Giriş: çerçevenin "Giriş yap"ı (3.1.4), ödeme adımının giriş bağlantısı ve hatırlatması (3.3.16), oturum gerektiren ekranın adresi (3.5.14), doğrulama ve sıfırlama ekranlarının giriş bağlantısı (3.3.29). Çıkış: girişten sonra dönüş yeri §3.6'dadır — ödeme adımı (§3.6.2), istenen ekran (§3.6.1) ya da geldiği vitrin sayfası (§3.6.3) · kayıt → E-20, "şifremi unuttum" → E-23 (3.3.26) · "Google ile giriş" → Google'ın sayfası (3.3.27).
- **5.22.2 İlk görülen:** e-posta ve şifre alanları; birincil düğme giriştir.
- **Bilgi hiyerarşisi**
  - **5.22.3** (1) Başlık · (2) "Google ile giriş" — Google uygulaması kurulumda tanımlıysa — ve yanında aydınlatma metninin bağlantısı (`02 §3.13.6`, §3.33.5) · (3) form (OB-12): e-posta, şifre, "beni hatırla" — işaretsiz gelir; işaretliyse uzun, değilse kısa oturum açılır (`02 §3.13.12`; Z-4, Z-5) · (4) giriş düğmesi · (5) "şifremi unuttum" ve kayıt bağlantıları.
  - **5.22.4** Ekran yalnız müşteri hesaplarını arar; yönetici panelin kendi girişinden (E-29) girer ve iki kapı birbirinin hesabını aramaz (`02 §10.2.5`; `03 §9.2.1`; K-588).
- **Aksiyonlar**
  - **5.22.5 E-posta ve şifreyle girmek** (`03 §9.2.1`): oturum açılır, tarayıcı o hesap için tanınan tarayıcı olur ve misafir sepeti hesabın sepetiyle birleşir — birleşme bir şey eklediyse dönülen sayfada bir kez söylenir (`03 §2.3.6`; §2.3.1.3). Dönüş §3.6'dadır.
  - **5.22.6 Google ile girmek** (`03 §9.1.6`, §9.2.2; 3.3.27): Google'ın doğrulanmış verdiği e-posta bir müşteri hesabıyla eşleşiyorsa o hesaba girilir ve hesap bundan sonra iki giriş yolunu taşır; eşleşen hesap yoksa hesap doğrulanmış ve şifresiz doğar; doğrulanmamış bekleyen kayda eşleşiyorsa kayıt devralınmaz, silinir ve hesap Google ile doğar (`03 §3.4.16`; K-696). Google e-postayı doğrulanmamış verirse kullanıcı kayda yönlendirilir (3.5.9; `03 §9.1.7`; K-697). Google ile giriş L-1'e girmez (`03 §6.1.3.5`).
  - **5.22.7 Hatalı giriş.** E-posta ya da şifre yanlışsa formun başında hangi alanın yanlış olduğunu ve hesabın var olup olmadığını söylemeyen tek mesaj çıkar (K-779; `02 §8.1.4`). L-1 aşılınca giriş düğmesinin yanında nötr limit mesajı çıkar; mesaj ekseni söylemez ve hesap kilitlenmez (`03 §3.4.11`, §6.1.1.1; §2.3.5). Daha önce başarıyla girilmiş tarayıcı e-posta ekseninde engellenmez (`03 §6.1.3.2`); engellenen gerçek kullanıcının yolu şifre sıfırlamadır (`03 §6.1.3.3`).
  - **5.22.8 Doğrulanmamış hesapla girmek** (`03 §3.4.2`; K-659): e-posta ve şifre doğruysa ekran doğrulamanın beklendiğini söyler ve bağlantıyı yeniden isteyen düğmeyi gösterir — 5.20.7'nin kuralı ve L-9'la; e-posta ya da şifre yanlışsa 5.22.7'nin mesajı çıkar ve hesabın varlığı açığa çıkmaz (`02 §8.1.4`; metinler K-779). Doğrulama bağlantısının süresi dolmuşsa kayıt silinmiştir; giriş 5.22.7'nin mesajını verir ve yol yeniden kayıttır (`03 §3.4.2`).
- **5.22.9 Validasyonlar.** E-posta ve şifre zorunludur; e-posta biçimi denetlenir — alan mesajı (OB-12). Alan envanteri §7.1'dedir (5. oturum).
- **Durum × rol varyantları** (tam matris §6.2'de — 5. oturum)
  - **5.22.10 Veri toplayan girişlerin kapısı kapalıyken** "Google ile giriş" görünür kalır: mevcut hesap Google ile girer; yeni hesap açacak giriş yapılmaz ve dönüşte bu ekran formun başında yeni hesabın şu an açılamadığını söyler (OB-08, §2.8.3; `03 §3.4.12`, §9.1.6). Bekleyen kayıt Z-1'in sonuna kadar durur (`03 §3.4.16`).
  - **5.22.11** Google uygulaması tanımlı değilse "Google ile giriş" görünmez (`03 §10.1.3.3`); Google'a ulaşılamazsa Google ile giriş tamamlanmaz ve şifresi olan hesap şifreyle girer, şifresi olmayan hesap "şifremi unuttum" ile şifre belirler (`03 §10.1.3.1`, §10.1.3.2).
  - **5.22.12** Ödeme adımından gelen müşteri girişten sonra ödeme adımına döner ve yazdıkları durur; girişten vazgeçerse de yazdıklarıyla döner (§3.6.2; K-758). Oturumu kendiliğinden kapanmış kullanıcı ekranı oturumun kapandığını söyleyen mesajla görür (§3.6.5).
- **5.22.13 Boş / yükleniyor / hata durumları.** Ekranın boş hâli yoktur. Giriş sürerken düğme ikinci kez basılamaz (§2.7.2); beklenmeyen hata §2.7.3.1'dir.
- **5.22.14 Responsive notları.** Tek sütundur; "Google ile giriş" her sınıfta formun üstündedir.

*Kaynak: `02 §3.13.4`, §3.13.6, §3.13.7, §3.13.12, §3.13.17, §3.13.19, §3.33.5, §8.1.4, §8.2.6, §10.2.5 · `03 §1.10.5`, §1.10.6, §2.3.6, §2.4.1, §3.4.2, §3.4.7, §3.4.11, §3.4.12, §3.4.16, §4.1.4, §4.1.5, §6.1.1.1, §6.1.3.2, §6.1.3.3, §6.1.3.5, §9.1.3, §9.1.6, §9.1.7, §9.2.1–§9.2.3, §9.2.5, §10.1.3 · `10 §2` KP-26, KP-27, KP-74 · K-758, K-768, K-779 · devir: K-553, K-588 (iki giriş ekranı), K-659, K-697.*

### 5.23 E-23 — Şifre sıfırlama ekranları

- **5.23.1 Aktör · giriş · çıkış.** Kullanıcı. Kimlik iki adımı kapsar (konvansiyon 2): sıfırlama isteği ve yeni şifrenin kurulması. Giriş: giriş ekranının "şifremi unuttum"u (3.3.26) · hesap ekranının şifre belirleme bağlantısı (3.3.36) · e-postadaki sıfırlama bağlantısı — geri alma ekranından sonraki dahil (3.5.5, 3.3.33). Çıkış: yeni şifre kurulduktan sonra giriş → E-22 (3.3.29).
- **5.23.2 İlk görülen:** isteme adımında e-posta alanı, kurma adımında yeni şifre alanı; birincil düğme adımın düğmesidir.
- **Bilgi hiyerarşisi**
  - **5.23.3 İsteme adımı:** (1) başlık · (2) e-posta alanı (OB-12) — hesap ekranından gelindiyse hesabın e-postasıyla ön dolu (3.3.36) · (3) gönderme düğmesi. Gönderimden sonra formun yerinde kaynağın nötr metni durur: *"bu adres kayıtlıysa sıfırlama bağlantısını gönderdik"* (`02 §3.13.11`; `03 §9.2.4`, §3.4.4).
  - **5.23.4 Kurma adımı:** (1) başlık — hesabın şifresi yoksa adım şifre belirleme işlevini adlandırır (`02 §3.13.8`; `03 §3.4.8`) · (2) yeni şifre alanı · (3) kurma düğmesi. Kurulduktan sonra adımın yerinde şifrenin kurulduğu, hesabın bütün oturumlarının kapandığı ve giriş bağlantısı durur (`03 §9.2.5`; 3.3.29).
- **Aksiyonlar**
  - **5.23.5 Bağlantıyı istemek** (`03 §9.2.4`). Onay istemez. Ekran adresin kayıtlı olup olmadığını söylemez (`02 §8.1.4`). L-2 aşılınca nötr limit mesajı çıkar (`03 §6.1.1.2`, §6.1.3.4; §2.3.5).
  - **5.23.6 Yeni şifreyi kurmak** (`03 §9.2.5`). Bağlantı tek kullanımlıktır; şifre kurulunca hesabın bütün oturumları kapanır, bu tarayıcı tanınan tarayıcı olur ve öteki işaretler düşer — e-posta ekseninde engellenen kullanıcının çıkış yolu budur (`03 §6.1.3.3`). Google ile açılmış hesap bundan sonra iki giriş yolunu taşır. Kullanıcı yeni şifreyle girer; kurma oturum açmaz.
- **5.23.7 Validasyonlar.** İsteme adımında e-posta biçimi; kurma adımında şifre politikası — asgari uzunluk (P-17) ve çok yaygın şifreler listesi; ret alan mesajıdır (`02 §3.13.15`; `03 §3.4.6`). Alan envanteri §7.1'dedir (5. oturum).
- **5.23.8 Durum × rol varyantları.** Bağlantı kullanılmış ya da süresi (Z-2) dolmuşsa kurma adımı açılmaz: ekran bağlantının geçersiz olduğunu söyler ve isteme adımına götürür — yeni talep L-2 içindedir (`03 §3.4.5`). Yönetici hesabının sıfırlaması bu ekranda değil panel girişinde (E-29) yürür (3.5.5; `02 §10.2.5`). Geri alma bağlantısından sonraki sıfırlamada şifre eski adrese giden bağlantıyla kurulur (`03 §9.3.6`, §6.2.5.2). Tam matris §6.2'dedir (5. oturum).
- **5.23.9 Boş / yükleniyor / hata durumları.** Ekranın boş hâli yoktur. Gönderim sürerken düğme ikinci kez basılamaz (§2.7.2). E-posta altyapısının kesintisinde bağlantı ulaşmaz ve kullanıcı altyapı dönünce yeniden ister — bu kalan risktir (`03 §10.1.2.5`).
- **5.23.10 Responsive notları.** Tek sütundur; sınıflar arasında fark yoktur.

*Kaynak: `02 §3.13.8`–§3.13.11, §3.13.15, §8.1.4, §8.2.6, §10.2.5 · `03 §3.1.1`, §3.4.4–§3.4.6, §3.4.8, §3.4.11, §4.1.2, §6.1.1.2, §6.1.3.3, §6.1.3.4, §6.2.5.2, §7.1.50, §9.2.4, §9.2.5, §9.3.6, §10.1.2.5, §10.1.3.2 · `10 §2` KP-27.*

### 5.24 E-24 — Yeniden doğrulama ekranı

- **5.24.1 Aktör · giriş · çıkış.** Üye. Giriş: hesap ekranının şifre değiştirme, e-posta değiştirme ve hesap silme işlemleri (3.3.30). Çıkış: doğrulamadan sonra hesap ekranına, başlatılan işlemin formuna ya da son adımına (3.3.30) · Google ile doğrulama → Google'ın sayfası (3.3.37) · "Vazgeç" → E-25.
- **5.24.2 İlk görülen:** hangi işlem için doğrulandığı ve şifre alanı — şifresi olmayan hesapta Google ile doğrulama düğmesi; birincil düğme doğrulamadır.
- **Bilgi hiyerarşisi**
  - **5.24.3** (1) Başlatılan işlemin adı — şifre değiştirme · e-posta değiştirme · hesap silme · (2) şifre alanı — hesabın şifresi varsa · (3) Google ile doğrulama düğmesi — hesap Google girişi taşıyorsa; şifreli ve Google girişli hesapta iki yol da görünür (`02 §3.13.13`) · (4) doğrulama düğmesi ve "Vazgeç".
  - **5.24.4** Yeniden doğrulama yalnız başlatılan işlem içindir: üç işlemden başkası onu istemez ve doğrulama bir işlemden ötekine taşınmaz (`02 §3.13.13`; `03 §9.2.6`).
- **Aksiyonlar**
  - **5.24.5 Doğrulamak** (`03 §9.2.6`): şifreyle ya da Google ile; başarıda üye hesap ekranına, işlemin formuna döner (3.3.30). Onay istemez.
- **5.24.6 Validasyonlar.** Şifre zorunludur. Yanlış şifre alan mesajıdır ve deneme L-1'e sayılır; eşik aşılınca o eksende şifreyle yeniden doğrulama ve giriş geçici olarak engellenir, nötr limit mesajı çıkar ve işlem yapılmaz (`03 §3.4.17`, §6.1.1.1; §2.3.5). Google ile doğrulama L-1'e girmez.
- **5.24.7 Durum × rol varyantları.** Şifresi olmayan hesapta Google'a ulaşılamıyorsa üç işlem yapılamaz: ekran sebebini söyler ve şifre belirlemenin yolunu — hesap ekranındaki şifre belirleme bağlantısını — gösterir (`03 §10.1.3.2`; 3.3.36). Panelde Google ile yeniden doğrulama yoktur; yöneticinin karşılığı E-54'tür (`02 §10.2.5`). Tam matris §6.2'dedir (5. oturum).
- **5.24.8 Boş / yükleniyor / hata durumları.** Ekranın boş hâli yoktur. Doğrulama sürerken düğme ikinci kez basılamaz (§2.7.2). Oturum ekrandayken kapanırsa ekran bunu söyler ve girişe götürür; dönüş §3.6.5'tir.
- **5.24.9 Responsive notları.** Tek sütundur; sınıflar arasında fark yoktur.

*Kaynak: `02 §3.13.13`, §8.1.5, §10.2.5 · `03 §3.4.17`, §6.1.1.1, §6.2.5.1, §9.2.6, §9.3.3, §9.3.4, §9.4.1, §10.1.3.2.*

### 5.25 E-25 — Hesap — profil ve güvenlik

- **5.25.1 Aktör · giriş · çıkış.** Üye. Giriş: çerçevenin "Hesabım"ı (3.1.6), hesap ekranlarının iç gezinmesi (3.3.32), oturumsuz gelişte girişten sonra (§3.6.1), doğrulama ve yeniden doğrulama ekranlarından dönüş (3.3.30, 3.3.35). Çıkış: üç işlem → E-24 → E-25 (3.3.30) · şifre belirleme → E-23 (3.3.36) · iç gezinme → E-26, E-27 (3.3.32) · hesap silmenin sonucu → bu ekranın sonuç hâli (3.3.31).
- **5.25.2 İlk görülen:** hesabın adı ve e-postası; birincil düğme yoktur — her bölüm kendi düğmesini taşır.
- **Bilgi hiyerarşisi** — üstte hesap ekranlarının iç gezinmesi: Hesabım · Adres defteri · Siparişlerim (K-768); altında beş bölüm:
  - **5.25.3 (1) Ad** — hesabın adı ve düzenleme; yeniden doğrulama istemez (`02 §3.13.21`; `03 §9.3.1`; K-605).
  - **5.25.4 (2) E-posta** — hesabın e-postası ve e-posta değiştirme düğmesi. **Bekleyen değişiklik** varsa altında yeni adresiyle görünür: doğrulamanın beklendiği, bağlantının geçerli olduğu son gün (§2.6.6) ve bağlantıyı yeniden isteyen düğme; yeni bir adres yazmak e-posta değiştirme düğmesiyle yapılır ve bekleyenin yerine geçer — ayrı bir "vazgeç" işlemi yoktur (K-734; `02 §3.13.14`; `03 §9.3.4`). **Geri alma bağlantısı açıkken** (Z-45) e-posta değiştirme düğmesi kapalıdır ve yerinde sebebi ile bağlantının bitiş tarihi durur (`02 §3.13.14`; metni K-779).
  - **5.25.5 (3) Şifre** — şifresi olan hesapta şifre değiştirme düğmesi · şifresi olmayan, Google ile açılmış hesapta şifre değiştirme yerine şifre belirlemenin yolu: sıfırlama ekranına, e-posta ön dolu götürür (`02 §3.13.10`; `03 §9.3.3`; 3.3.36).
  - **5.25.6 (4) Giriş yolları** — hesabın taşıdığı iki yol, salt okunur: e-posta ve şifre — şifre var ya da yok · Google — bağlı ya da bağlı değil. **Bağlı Google girişini kaldırma işlemi yoktur;** bağ yalnız e-posta değişikliğinin geri alınmasında sistemce kalkar (K-733; `02 §3.13.7`; `03 §9.2.2`). Google'ı bu ekrandan bağlama işlemi de yoktur: bağ, hesabın e-postasıyla Google ile girişte kendiliğinden kurulur (`02 §3.13.7`).
  - **5.25.7 (5) Hesabı sil** — en altta: silmenin sonucu — hesabın adı, giriş bilgileri, oturumları, adres defteri ve sepeti hemen silinir; siparişler sürer ve sipariş e-postasındaki bağlantıyla ya da sipariş numarası ve e-postayla izlenir (`02 §3.15.1`, §3.15.2) — ve silmeyi başlatan düğme. Yürüyen sipariş silmeyi engellemez (`03 §3.4.10`). **Geri alma bağlantısı açıkken** düğme kapalıdır ve yerinde sebebi ile bağlantının bitiş tarihi durur (`02 §3.15.6`; `03 §3.4.15`; K-698; metni K-779).
  - **5.25.8 Yoktur:** bildirim tercihi ve pazarlama onayı — işlem bildirimleri kapatılamaz, pazarlama iletisi gönderilmez (`03 §9.5.1`, §9.5.2) · dil seçimi (`03 §9.5.3`) · uygulama içi veri indirme — yol iletişim formunun "KVKK talebi" tipidir ve bölüm forma giden yolu söyler (`03 §9.5.5`, §9.4.4; 5.10.9) · iki adımlı doğrulama (`02 §3.13.16`).
- **Aksiyonlar**
  - **5.25.9 Adı değiştirmek** (`03 §9.3.1`) — onay istemez; başarı kısa süreli bildirimle söylenir (§2.3.1.4). Geçmiş siparişin alıcı adına dokunmaz (`02 §3.13.21`).
  - **5.25.10 Şifreyi değiştirmek** (`03 §9.3.3`): E-24'ten sonra bölümün içinde yeni şifre alanı açılır; kaydedilince bölümde şifrenin değiştiği ve öteki oturumların kapandığı yerinde kalıcı mesajla söylenir — değişikliği yapan oturum açık kalır (`02 §3.13.10`; K-695).
  - **5.25.11 E-postayı değiştirmek** (`03 §9.3.4`): E-24'ten sonra bölümün içinde yeni adres alanı açılır; gönderilince yeni adrese doğrulama bağlantısı gider ve değişiklik bekleyen değişiklik olarak görünür (5.25.4). Hesap yeni adres doğrulanana kadar eski adresle çalışır. Yeni adres başka bir müşteri hesabınınsa değişiklik kabul edilmez ve alan mesajı bunu söyler (K-786). Değişiklik geçerli olunca — E-21'de — öteki oturumlar kapanmaz (`02 §3.13.14`).
  - **5.25.12 Bağlantıyı yeniden istemek** — bekleyen değişikliğin bağlantısı; 5.20.7'nin kuralıyla, L-9'la (`03 §6.1.1.9`; K-659).
  - **5.25.13 Hesabı silmek** (`03 §9.4.1`): E-24'ten sonra bölümün içinde son adım açılır (OB-04, müşterinin son adımı; §2.4.5): sonuç cümlesi (K-785) · "Bu işlem geri alınamaz" · "Hesabımı sil" (K-785) · "Vazgeç". Onayla hesap silinir (`03 §9.4.2`) ve ekran **sonuç hâline** geçer (K-781): hesabın silindiği; silinenlerin — ad, giriş bilgileri, oturumlar, adres defteri, sepet —; siparişlerin sürdüğü ve sipariş e-postasındaki bağlantıyla ya da sipariş takibinden izleneceği; hesabın adresine silindiğine dair bir e-posta gittiği (`02 §3.15.1`; `03 §9.4.3`; K-693). Oturum kapanmıştır: çerçeve hesap alanında "Giriş yap"ı ve "Sipariş takibi"ni gösterir (3.1.5); sonuç hâli bir kez görünür, sayfa yenilenirse ana sayfaya iner (§3.6.4). Hesabın silindiği kısa süreli bildirimle söylenmez (§2.3.4).
- **5.25.14 Validasyonlar.** Ad boş olamaz · yeni e-posta biçimi geçerli olmalı ve müşteri hesapları içinde tekil olmalıdır (`03 §9.3.4`; K-786) · yeni şifre politikası 5.23.7'dir. L-9 aşılınca nötr limit mesajı (§2.3.5). Alan envanteri §7.1'dedir (5. oturum).
- **Durum × rol varyantları** (tam matris §6.2'de — 5. oturum)
  - **5.25.15** Hesabın giriş yollarına göre: şifreli · şifresiz Google · ikisi birden — 5.25.5 ve 5.25.6 buna göre değişir. Geri alma bağlantısı açıkken yeni bir değişiklik başlatılamadığı için bekleyen değişiklik satırı ile geri alma engeli birlikte durmaz (`02 §3.13.14`).
  - **5.25.16** Şifresi olmayan hesapta Google'a ulaşılamıyorsa üç işlem yapılamaz (5.24.7).
- **5.25.17 Boş / yükleniyor / hata durumları.** Ekranın boş hâli yoktur. Her bölümün kaydı kendi yerinde bekler ve düğmesi ikinci kez basılamaz (§2.7.2); beklenmeyen hatada mesaj bölümün içinde çıkar (§2.7.3.1). Silmenin onayı sonuçsuz kalırsa ekran "işlem yapılmadı" demez (§2.7.3.3): yeniden açılan ekran — hesap duruyorsa — hesabı, silindiyse girişi gösterir. Oturum ekrandayken kapanırsa §3.6.5.
- **5.25.18 Responsive notları.** Geniş sınıfta iç gezinme solda dikey listedir, bölümler sağda; dar sınıfta iç gezinme ekranın başında yatay sekmelerdir ve bölümler alt alta durur. Hesabı sil bölümü her sınıfta en alttadır.

*Kaynak: `02 §3.13.7`, §3.13.8, §3.13.10, §3.13.13, §3.13.14, §3.13.16, §3.13.21, §3.15.1, §3.15.2, §3.15.6, §5.12, §8.1.5, §9.4 · `03 §1.10.5`, §3.4.6, §3.4.9, §3.4.10, §3.4.15, §4.1.35, §4.1.44, §6.1.1.9, §6.2.5.1, §9.2.2, §9.2.6, §9.3.1, §9.3.3–§9.3.5, §9.4.1–§9.4.3, §9.5, §10.1.2.5 · `10 §2` KP-27, KP-29 · K-732, K-733, K-734, K-744, K-768, K-779, K-781, K-785, K-786 · devir: K-605 (adı değiştirme), K-698 (silme engelinin mesajı); `04 §3.3.31`'in geçici hedefi (silmenin sonuç hâli).*

### 5.26 E-26 — Adres defteri

- **5.26.1 Aktör · giriş · çıkış.** Üye. Giriş: hesap ekranlarının iç gezinmesi (3.3.32; K-768). Çıkış: iç gezinme → E-25, E-27.
- **5.26.2 İlk görülen:** kayıtlı adresler; birincil düğme adres ekleme düğmesidir.
- **Bilgi hiyerarşisi**
  - **5.26.3** (1) İç gezinme · (2) adres ekleme düğmesi · (3) adres kayıtları — her kayıtta adres adı (ör. Ev, İş), alıcının adı ve soyadı, il, ilçe, açık adres ve doluysa telefon; kayıt başına düzenle ve sil (`02 §3.14.1`, §3.14.6; `03 §9.3.2`).
  - **5.26.4 Adres formu** — OB-13'ün defter varyantı (§2.12.2.2): adres adı ve teslimat adresinin alanları; il kapalı listeden, telefon isteğe bağlı (`02 §3.14.6`; K-561). Form ekranın içinde açılır.
  - **5.26.5** Defterdeki değişiklik ve silme geçmiş siparişe dokunmaz — sipariş adreslerini dondurmuştur (`02 §3.14.5`); ekran bunu formun altında bir cümleyle söyler. Telefonu olmayan kayıt ödeme adımında teslimat adresi olarak seçilince telefon orada istenir (`02 §3.14.6`). Firmanın teslimat yapmadığı ildeki kayıt defterde tutulur; kısıt ödeme adımında işler (`02 §3.20.1`; 5.13.6).
- **Aksiyonlar**
  - **5.26.6 Adres eklemek, düzenlemek, silmek** (`03 §9.3.2`). Onay istemez — adres silme geri alınamaz dört işlemden biri değildir (§2.4.2). Ekleme ve düzenlemenin başarısı kısa süreli bildirimle söylenir; silinen kayıt listeden kalkar ve ayrı bildirim yoktur (5.12.9'un kalıbı). Ödeme adımında yazılan yeni adres üye isterse orada deftere kaydedilir (`03 §9.3.2`; 5.13.14).
- **5.26.7 Validasyonlar.** Adres adı, alıcının adı ve soyadı, il, ilçe ve açık adres zorunludur; telefon isteğe bağlıdır (OB-13; `02 §3.14.6`). Alan envanteri §7.1'dedir (5. oturum).
- **5.26.8 Durum × rol varyantları.** Aktörle değişen parça yoktur. Tam matris §6.2'dedir (5. oturum).
- **5.26.9 Boş / yükleniyor / hata durumları.** Adresi olmayan defter boş hâl satırını gösterir: kayıtlı adres olmadığını söyler ve adres ekleme düğmesini taşır (§2.7.1.2). Kayıt sürerken düğme ikinci kez basılamaz (§2.7.2); beklenmeyen hatada form içeriği korunur (§2.7.3.1).
- **5.26.10 Responsive notları.** Geniş sınıfta kayıtlar iki sütunlu kart ızgarasıdır ve iç gezinme 5.25.18'in düzenindedir; dar sınıfta kayıtlar alt alta durur.

*Kaynak: `02 §3.14.1`, §3.14.3, §3.14.5, §3.14.6, §3.20.1 · `03 §9.3.2` · `10 §2` KP-12, KP-27 · K-768 · devir: K-561 (adres formu).*

### 5.27 E-27 — Sipariş geçmişi

- **5.27.1 Aktör · giriş · çıkış.** Üye. Giriş: çerçevenin "Siparişlerim"i (3.1.7), hesap ekranlarının iç gezinmesi (3.3.32). Çıkış: sipariş satırı → E-16 (3.3.20) · iç gezinme → E-25, E-26.
- **5.27.2 İlk görülen:** hesaba bağlı siparişlerin listesi, en yeni önce; birincil düğme yoktur.
- **Bilgi hiyerarşisi**
  - **5.27.3** (1) İç gezinme · (2) sipariş listesi (OB-20) · (3) sayfa gezinmesi — liste sayfa boyunu aşarsa (§2.13.3).
  - **5.27.4 Satır** (K-784): sipariş numarası · sipariş tarihi · ilk kalemin adı ve öteki kalemlerin sayısı · ödenen ya da ödenecek toplam · iki rozet — Sipariş durumu ve Ödeme durumu, yan yana (§2.5.1; K-777). Satırın tamamı sipariş sayfasını açar.
  - **5.27.5 Listede görünenler:** hesaba bağlı bütün siparişler — hesaba düşmüş misafir siparişleri dahil: hesap açılmadan önce o e-postayla verilenler, hesabı olan kişinin giriş yapmadan verdikleri ve e-posta değişikliğinden sonra yeni adrese bağlananlar (`02 §3.13.2`, §3.13.3, §3.13.14; `03 §9.1.4`, §9.3.5) · müşterinin ya da firmanın iptal ettiği ödenmemiş sipariş. **Görünmeyen:** ödemesi hiç alınmamış ve kendiliğinden iptal edilmiş sipariş — panelde görünür (`02 §3.17.8`; `03 §2.6.1`, §3.2.2.4) · firmanın e-postasını düzelttiği misafir siparişi eski adresin hesabından düşer (`02 §3.13.3`; `10 §2` KP-28).
  - **5.27.6 Yoktur:** arama ve süzgeç (§2.13.2) · listeden iptal, cayma ya da yeniden sipariş — işlemler sipariş sayfasındadır (`03 §5.1.1`).
- **5.27.7 Aksiyonlar:** sipariş satırı → E-16 (3.3.20; `03 §2.6.1`); sayfa değiştirmek ekranın içinde kalır ve listeye dönen üye bıraktığı sayfayı bulur (§2.13.3). Onay ve mesaj yoktur.
- **5.27.8 Validasyonlar:** form yoktur.
- **5.27.9 Durum × rol varyantları.** Satırın rozetleri siparişin iki ekseninin bütün değerlerini alır (§2.5.1.1, §2.5.1.2). Hesap silinirse liste de silinir; siparişler hiçbir hesaba yeniden bağlanmaz (`02 §3.15.3`). Tam matris §6.2'dedir (5. oturum).
- **5.27.10 Boş / yükleniyor / hata durumları.** Siparişi olmayan üyenin listesi boş hâl satırını gösterir: henüz sipariş olmadığını söyler ve ürünlere dönüş yolunu taşır (§2.7.1.2). Liste kendi yerinde yüklenir (§2.7.2); sayfa açılamazsa §2.7.3.2.
- **5.27.11 Responsive notları.** Geniş sınıfta satırlar tablo düzenindedir; dar sınıfta her sipariş bir karttır — numara ve iki rozet üstte, tarih, ilk kalem ve toplam altta; rozetler tek rozete inmez. Liste hiçbir sınıfta yatay kaydırılmaz (§2.1.4).

*Kaynak: `02 §3.13.2`, §3.13.3, §3.13.14, §3.15.3, §3.17.8 · `03 §2.4.9`, §2.5.1.3, §2.6.1, §2.7.1, §3.2.1.1, §3.2.1.15, §3.2.2.4, §9.1.4, §9.3.5, §10.1.2.4 · `10 §2` KP-18, KP-28 · K-766, K-768, K-777, K-784 · devir: K-777 (iki rozetin yazımı).*

### 5.28 E-28 — E-posta değişikliğini geri alma ekranı

- **5.28.1 Aktör · giriş · çıkış.** Kullanıcı — adresin önceki sahibi. Giriş: eski adrese giden bildirimdeki "bu değişikliği ben yapmadım" bağlantısı (3.5.7). Çıkış: yeni şifre e-postadaki sıfırlama bağlantısıyla E-23'te kurulur (3.3.33) · çerçeve.
- **5.28.2 İlk görülen:** bağlantının sonucunu söyleyen tek cümle; birincil düğme yoktur — sonraki adım e-postadadır.
- **Bilgi hiyerarşisi** — iki hâl (K-782):
  - **5.28.3 Geri alındı** — bağlantı Z-45 içinde ve ilk kez açıldıysa: hesabın e-postasının önceki adrese döndüğü · hesabın bütün oturumlarının kapandığı · değişiklik süresince bağlanmış Google girişinin kaldırıldığı — varsa · şifrenin geçersizleştiği ve önceki adrese şifre sıfırlama bağlantısı gönderildiği — yeni şifre o bağlantıyla kurulur · değişiklik süresince hesaba bağlanmış siparişlerin hesapta kaldığı (`02 §3.13.14`; `03 §9.3.6`, §6.2.5.2; K-599).
  - **5.28.4 Bağlantı geçersiz** — süresi dolmuş ya da daha önce kullanılmış: bağlantının geçersiz olduğu ve ürünün içinden geri alma yolu kalmadığı; siparişlere sipariş e-postalarındaki bağlantıyla girilmeye devam edildiği; firmaya İletişim sayfasından ulaşılabileceği — İletişim'e çerçeveden gidilir (`03 §6.2.5.3`, §4.1.44; 3.1.16). Daha önce kullanılmış bağlantı geri almanın yapıldığını söyler.
  - **5.28.5 Yoktur:** ekranda şifre alanı — şifre e-postadaki bağlantıyla kurulur (`03 §9.3.6`; 3.3.33) · değişikliği yapanın kimliği ve yeni adres — bildirim yeni adresi taşımaz (`02 §3.13.14`) · ikinci bir onay adımı — bağlantının açılması geri almadır (`03 §9.3.6`).
- **5.28.6 Aksiyonlar:** yoktur; ekran sonucu söyler. Bildirimin metni Entegrasyon Spesifikasyonu'nun işidir (K-735; §4.4.1).
- **5.28.7 Validasyonlar:** form yoktur.
- **5.28.8 Durum × rol varyantları.** Ekran oturumdan bağımsızdır; geri alma, açık olan oturumu da kapatır. Yönetici hesabının karşılığı E-54'tür (3.5.7; `03 §9.3.7`). Tam matris §6.2'dedir (5. oturum).
- **5.28.9 Boş / yükleniyor / hata durumları.** Ekranın boş hâli yoktur. Sayfa açılamazsa §2.7.3.2; geri almanın sonucu belirsiz kalırsa ekran "işlem yapılmadı" demez — bağlantı yeniden açılınca sonucu söyler (§2.7.3.3).
- **5.28.10 Responsive notları.** Tek sütundur; sınıflar arasında fark yoktur.

*Kaynak: `02 §3.13.14`, §8.1.5, §8.2.6 · `03 §1.10.5`, §3.4.13, §4.1.44, §6.2.5.2, §6.2.5.3, §7.1.50, §7.1.52, §9.3.6 · `10 §2` KP-27 · K-735, K-782 · devir: K-599 (geri alma ekranı), K-735 (geri alma ekranı — ekrandaki iz).*

## 6. Durum × Rol matrisi

> **Kapı — yazım turunun 5. oturumu.** Matris iki tarafın ekran tanımları (§5, §9) bittikten sonra kurulur (K-724); alt bölümleri başlık notunun alt bölüm haritasındadır (§6.1–§6.3).

> **Ne yazılır:** Her ekran için {ekran × rol × durum} kombinasyonları. Eksik kombinasyonlar burada yakalanır — bir referans projede tek bir detay ekranı 13 durum × 3 rol = ~52 varyant gerektirdi.

| Ekran | Rol | Durum | Ne gösterilir | Hangi aksiyonlar aktif |
|---|---|---|---|---|

## 7. Form ve validasyon envanteri

> **Kapı — yazım turunun 5. oturumu.** Envanter iki tarafın ekran tanımları bittikten sonra kurulur (K-724); alt bölümleri başlık notunun alt bölüm haritasındadır (§7.1–§7.3). Ortak form bileşenleri §2.12'dedir.

## 8. Lokalizasyon etkileri

> **Kapı — yazım turunun 5. oturumu.** Alt bölümleri başlık notunun alt bölüm haritasındadır (§8.1–§8.3); tek dil Türkçedir (`10 §4.2` SK-2) ve bölüm "yoktur" ile Türkçenin biçimleriyle yazılır.

> **Ne yazılır:** Çoklu dil desteğinin **bilgi mimarisine** etkisi — metin uzunluk farkları, tarih/sayı biçimleri, yazı yönü. Bu yüzeysel bir konu değildir; layout ve bileşen boyutlarını etkiler.

## 9. Yönetim (admin) ekranları

**Yazım turunun 3. oturumu (2026-10-04, v0.10).** Panelin ilk dokuz ekranı (E-29…E-37) — panel girişi ve şifre sıfırlama, davet kabulü, panel ana sayfası, katalog ekranları, sipariş listesi ve sipariş ayrıntısı — §5'in biçimiyle yazıldı: sekiz alan, ekranın içinde alanlardan bağımsız tek sıralı madde numarası (`04 §9.n.k`; konvansiyon 5) ve Kaynak satırı; ilk madde aktörü ve §3'ün giriş-çıkış satırlarını, ikinci madde ilk görüleni taşır. Panelin dört özelliği bütün tanımlarda işler: panel geniş sınıftan tasarlanır, dar sınıfta tablolar kart listesine döner ve hiçbir işlev bir sınıfta gizlenmez (§2.1.2; K-741) · yöneticinin onayı penceredir (OB-04, §2.4.4) · panel tek roldür ve rol varyantı yoktur (konvansiyon 7) · bir yönetici işleminin müşteriye giden bildirimi ve işlem izi ekranda anlatılmaz; ekranda bıraktığı iz — kalemin kaydı, rozet, işaret, e-posta geçmişinin satırı — yazılır (konvansiyon 15; `03 §7`). "Durum × rol varyantları" alanının tam matrisi §6.3'tedir (5. oturum). Ekranların kaynak satırları §1.2'nin ikinci betiğiyle listelendi ve Kullanıcı Akışları'nda, Ürün Gereksinimleri'nde ve MVP Kapsamı'nda tam metinle okundu; matrisin bu okumada düzelen satırları §1.2'nin notundadır. Yazımın bulduğu boşluklar K-797…K-808'dir (§1.3).

**Yazım turunun 4. oturumu (2026-10-04, v0.11).** Panelin kalan on altı ekranı (E-38…E-47, E-49…E-54) aynı biçimle yazıldı ve §9 tamamlandı; alt bölüm numaraları başlık notunun alt bölüm haritasındadır — E-48 düştüğü için §9.20 E-49'dur (§4.3). Ayar ekranları ilk kurulumdaki ve satış kapalıyken hâllerini kurulum kontrol listesiyle (9.3.4) ve E-44'ün "Satış durumu" bölümüyle aynı adlarla yazar. Ekranların kaynak satırları §1.2'nin ikinci betiğiyle listelendi ve okundu; matrisin bu okumada düzelen satırları §1.2'nin 4. oturum notundadır. Yazılan ekranlar sipariş başına yeni bir zorunlu elle adım eklemez — talebin kapatılması, üye hesabının silinmesi, içerik, ayar, yönetici ve rapor işlemleri sipariş hattının adımı değildir; hat başına sayılar değişmez (`02 §10.5`; `03 §8.5.7`; K-404). Yazımın bulduğu boşluklar K-809…K-823'tür (§1.3).

> **Not:** Yönetim ekranları toplam ekranların yarısı kadar olabilir. İkincil endişe olarak ele alınmaz.

### 9.1 E-29 — Panel girişi ve şifre sıfırlama

- **9.1.1 Aktör · giriş · çıkış.** Yönetici · Kullanıcı — şifresini unutan ve sıfırlama bağlantısını açan yönetici. Kimlik üç adımı kapsar (konvansiyon 2): giriş, sıfırlama isteği ve yeni şifrenin kurulması. Giriş: panelin adresi ve oturum gerektiren panel ekranının adresi (3.5.14, §3.6.1) · çıkış (3.2.22, §3.6.4) · kendiliğinden kapanan oturum (§3.6.5) · e-postadaki sıfırlama bağlantısı — kurma adımına (3.5.5). Çıkış: girişten sonra E-31 ya da istenen panel ekranı (3.4.1, §3.6.1).
- **9.1.2 İlk görülen:** e-posta ve şifre alanları; birincil düğme giriştir. Ekran panel çerçevesini taşımaz (§2.2.9).
- **Bilgi hiyerarşisi** — üç adım; düzen müşteri tarafındaki giriş ve sıfırlama ekranlarının kalıbıdır (5.22.3, 5.23.3, 5.23.4), kurallar `02 §10.2.4` gereği aynıdır:
  - **9.1.3 Giriş adımı:** (1) panelin girişi olduğunu söyleyen başlık (§2.2.9) · (2) form (OB-12): e-posta, şifre ve "beni hatırla" — işaretsiz gelir; işaretliyse uzun, değilse kısa oturum açılır (`03 §9.2.7`; Z-4, Z-5) · (3) giriş düğmesi · (4) şifre sıfırlamaya götüren bağlantı.
  - **9.1.4 Sıfırlama isteği adımı:** e-posta alanı ve gönderme düğmesi; gönderimden sonra formun yerinde kaynağın nötr metni durur: *"bu adres kayıtlıysa sıfırlama bağlantısını gönderdik"* (`03 §9.2.4`, §9.2.8).
  - **9.1.5 Yeni şifre adımı:** e-postadaki bağlantıyla açılır (3.5.5): yeni şifre alanı ve kurma düğmesi; kurulduktan sonra adımın yerinde şifrenin kurulduğu, hesabın bütün oturumlarının kapandığı ve giriş adımına dönen bağlantı durur (`03 §9.2.5`, §9.2.8).
  - **9.1.6 Yoktur:** Google ile giriş, kayıt bağlantısı ve kendi kendine yönetici kaydı (`02 §10.2.1`, §10.2.5; `03 §9.2.7`) · iki adımlı doğrulama (`02 §10.2.4`) · müşteri hesabının aranması — panel girişi yalnız yönetici hesaplarını arar; aynı adresin müşteri hesabı bu ekranda bulunmaz (`02 §10.2.5`) · ürünün içinden erişim kurtarma — hiçbir yönetici panele giremiyorsa yol kurulum tarafıdır (`02 §10.2.7`; `03 §10.4.4`).
- **Aksiyonlar**
  - **9.1.7 Girmek** (`03 §9.2.7`): oturum açılır, tarayıcı o yönetici hesabı için tanınan tarayıcı olur ve yönetici E-31'e ya da istediği panel ekranına iner (3.4.1). Vitrinin hesap alanı ve sepeti panel girişinden etkilenmez (§2.9.4). Onay istemez.
  - **9.1.8 Hatalı giriş.** E-posta ya da şifre yanlışsa formun başında K-779'un metni çıkar — "E-posta adresi ya da şifre hatalı." —; hangi alanın yanlış olduğunu ve hesabın var olup olmadığını söylemez (`02 §8.1.4`). L-1 aşılınca giriş düğmesinin yanında nötr limit mesajı çıkar; panel girişine muafiyet yoktur ve hesap kilitlenmez (`03 §6.1.1.1`, §6.1.2.3; §2.3.5). Tanınan tarayıcı e-posta ekseninde engellenmez; engellenen yöneticinin yolu tanınan tarayıcısından girmek ya da şifresini sıfırlamaktır (`03 §6.1.3.2`, §10.4.2).
  - **9.1.9 Sıfırlama bağlantısını istemek** (`03 §9.2.8`): onay istemez; ekran adresin bir yönetici hesabına ait olup olmadığını söylemez (`02 §8.1.4`); L-2 aşılınca nötr limit mesajı çıkar (`03 §6.1.1.2`). Bağlantı yöneticinin kendi adresine gider (`03 §10.4.1`).
  - **9.1.10 Yeni şifreyi kurmak** (`03 §9.2.5`, §9.2.8): bağlantı tek kullanımlıktır; şifre kurulunca hesabın bütün oturumları kapanır ve bu tarayıcı tanınan tarayıcı olur (`03 §1.10.6`). Kurma oturum açmaz; yönetici yeni şifreyle girer.
- **9.1.11 Validasyonlar.** E-posta ve şifre zorunludur, e-posta biçimi denetlenir; yeni şifrede şifre politikası — P-17 ve yaygın şifreler listesi (`02 §3.13.15`, §10.2.4). Ret alan mesajıdır (OB-12). Alan envanteri §7.2'dedir (5. oturum).
- **9.1.12 Durum × rol varyantları.** Panel tek roldür. Bağlantı kullanılmış ya da süresi (Z-2) dolmuşsa yeni şifre adımı açılmaz: ekran bağlantının geçersiz olduğunu söyler ve sıfırlama isteği adımına götürür (5.23.8'in kalıbı; `03 §3.4.5`). Kendiliğinden kapanan oturumla gelen yönetici ekranı oturumun kapandığını söyleyen mesajla görür (§3.6.5). Satış kapısı ve kurulumun durumu ekranı etkilemez. Tam matris §6.3'tedir (5. oturum).
- **9.1.13 Boş / yükleniyor / hata durumları.** Ekranın boş hâli yoktur. Giriş ve gönderim sürerken düğme ikinci kez basılamaz (§2.7.2); beklenmeyen hata §2.7.3.1'dir. E-posta altyapısının kesintisinde sıfırlama bağlantısı ulaşmaz ve yönetici altyapı dönünce yeniden ister — bu kalan risktir (`03 §10.1.2.5`). Google'ın kesintisi panel girişini etkilemez (`03 §10.1.3.4`).
- **9.1.14 Responsive notları.** Tek sütundur; sınıflar arasında fark yoktur.

*Kaynak: `02 §3.13.15`, §8.1.3, §8.1.4, §8.1.7, §8.2.4, §8.2.6, §10.2.1, §10.2.4, §10.2.5, §10.2.7 · `03 §1.10.6`, §3.4.5, §6.1.1.1, §6.1.1.2, §6.1.2.3, §6.1.3.2, §7.1.50, §7.3.43, §9.2.4, §9.2.5, §9.2.7, §9.2.8, §10.1.2.5, §10.1.3.4, §10.4.1, §10.4.2, §10.4.4 · K-768, K-779 · devir: K-588 (iki giriş ekranı).*

### 9.2 E-30 — Yönetici daveti kabul ekranı

- **9.2.1 Aktör · giriş · çıkış.** Kullanıcı — davetli. Giriş: davet e-postasındaki bağlantı (3.5.10). Çıkış: hesap açıldıktan sonra panel girişi, davetin adresi ön dolu (3.4.18; K-807).
- **9.2.2 İlk görülen:** hesabın açılacağı e-posta adresi ile ad ve şifre alanları; birincil düğme hesabı açmaktır. Ekran panel çerçevesini taşımaz (§2.2.9).
- **Bilgi hiyerarşisi** — iki hâl:
  - **9.2.3 Geçerli davet:** (1) panelin daveti olduğunu söyleyen başlık · (2) davetin gönderildiği e-posta adresi — salt okunurdur; hesap bu adresle doğar (`02 §10.2.1`) · (3) form (OB-12): ad — zorunlu (K-736) — ve şifre · (4) hesabı açan düğme.
  - **9.2.4 Geçersiz bağlantı** — kullanılmış, süresi (Z-3) dolmuş, geri çekilmiş, aynı adrese giden yeni bir davetle ya da daveti gönderenin kaldırılmasıyla geçersizleşmiş: hesap açılmaz ve ekran süresi dolmuş davetin mesajını gösterir — metni K-799'dadır ve yeni davetin mevcut bir yöneticiden isteneceğini söyler (`02 §6.9.12`; `03 §3.5.4.12`, §8.8.4). Mesaj geçersizliğin sebebini ayırt etmez; bağlantıyı açan kişiye firma hakkında bilgi verilmez (`03 §7.2.20`).
  - **9.2.5 Yoktur:** Google ile hesap açma ve kendi kendine kayıt (`02 §10.2.5`; `03 §8.8.2`) · e-posta adresinin bu ekranda değiştirilmesi — hesap davetin adresiyle doğar; yönetici adresini sonra kendi hesabından değiştirir (E-50; 9.21.8).
- **Aksiyonlar**
  - **9.2.6 Hesabı açmak** (`03 §8.8.2`): onay istemez. Hesap o anda doğar; şifre kuralları müşteriyle aynıdır (`02 §10.2.4`). Adres bir müşteri hesabına aitse de ayrı bir yönetici hesabı doğar ve iki hesap bağlanmaz (`02 §10.2.5`). Davetliye ayrıca bildirim gitmez (`03 §7.2.26`); hesabın açılması işlem izine yazılır (`02 §10.3.1`). Ekran hesabın açıldığını söyler ve panel girişine götürür; açma oturum açmaz, yönetici şifresiyle girer (K-807).
- **9.2.7 Validasyonlar.** Ad boş olamaz; şifre politikası (P-17, yaygın şifreler) işler — ret alan mesajıdır (OB-12; `02 §3.13.15`). Alan envanteri §7.2'dedir (5. oturum).
- **9.2.8 Durum × rol varyantları.** Ekran oturumdan bağımsızdır; hâli davetin geçerliliği belirler (`03 §1.10.7`). Satış kapısı ekranı etkilemez. Tam matris §6.3'tedir (5. oturum).
- **9.2.9 Boş / yükleniyor / hata durumları.** Ekranın boş hâli yoktur. Gönderim sürerken düğme ikinci kez basılamaz (§2.7.2); hesap beklenmeyen bir sebeple açılamazsa form korunur ve aynı düğmeyle yeniden denenir (§2.7.3.1) — bağlantı o arada kullanılmışsa ekran 9.2.4'ün hâlini gösterir. Davet e-postası altyapının kesintisinde ulaşmazsa yol yeni davettir (`03 §10.1.2.5`).
- **9.2.10 Responsive notları.** Tek sütundur; sınıflar arasında fark yoktur.

*Kaynak: `02 §3.13.15`, §6.9.12, §10.2.1, §10.2.4, §10.2.5, §10.2.6, §10.3.1, §1.2 — Yönetici daveti · `03 §1.10.5`, §1.10.7, §3.5.4.12, §4.1.3, §7.1.53, §7.2.20, §7.2.26, §7.3.40, §8.8.2, §8.8.4, §10.1.2.5, §10.4.3 · `10 §2` KP-62 · K-736, K-799, K-807 · §1.3 GAP-8.*

### 9.3 E-31 — Panel ana sayfası

- **9.3.1 Aktör · giriş · çıkış.** Yönetici. Giriş: menünün "Ana sayfa"sı (3.2.1), panel girişi (3.4.1), vitrindeki "Panele dön" (3.7.3). Çıkış: sayaçlar — E-36'nın ya da E-38'in süzülmüş hâli (3.4.2) · uyarı satırları (3.4.3) · kurulum kontrol listesinin maddeleri (3.4.4) · "Müşteriden beklenenler"in satırları ve "Tümünü gör" (3.4.5) · çerçevenin geçişleri (§3.2).
- **9.3.2 İlk görülen:** varsa uyarılar, yoksa kurulum kontrol listesi, o da yoksa bekleyen işlerin sayaçları. Ekranın birincil düğmesi yoktur; her satır ve her sayaç kendi ekranına götürür (K-750).
- **Bilgi hiyerarşisi** — panel çerçevesinin (OB-02) içinde yukarıdan aşağıya dört bölüm; boş bölüm görünmez (K-750; §2.7.1.1):
  - **9.3.3 (1) Uyarılar** — koşulu sürerken görünen, çözüleceği ekrana götüren satırlar, şu sırayla (2.5.5; K-799): satışın kapalı olduğu ve sebebi — geçici kapatma ya da karşılanmayan koşul, adıyla (§2.8.2) · kanal uyarısı — *"e-posta gönderilemiyor — kurulum ayarını kontrol edin"* (`03 §10.1.2.3`); geçiş taşımaz, çözümü kurulum ayarıdır (K-769) · "firma bildirimleri ulaşmıyor" uyarısı — bildirimlerin gittiği adresle (K-760) · "e-posta ulaşmadı" işaretli siparişlerin sayısını söyleyen satır — bekleyen iş sayacı değildir (K-761) · kart iadesinin gerçekleşmediği uyarısı — sayısıyla (`03 §8.3.3.4`). Satırların metni K-799'da, götürdükleri yer 3.4.3'tedir.
  - **9.3.4 (2) Kurulum kontrol listesi** — satışın açılması için eksik maddeler adıyla (`02 §10.8.2`; `03 §8.7.1.1`; `10 §4.1` ÖK-8…ÖK-11): firma tipinin zorunlu kimlik alanları — KEP adresi dahil · en az bir açık ödeme yöntemi — havale açıksa IBAN · aydınlatma metni ve çerez politikası — ikisi de yayında; maddenin altında, aydınlatma tamamlanıp yayına alınana kadar iletişim formunun, hesap kaydının ve yeni hesap açan Google ile girişin de kapalı olduğu yazar (OB-08, §2.8.3) · iade adresi. Tamamlanan madde işaretli görünür; hepsi tamamlanınca bölüm kaybolur. Sihirbaz ve adım sırası yoktur; kurulumun dış ön koşulları (ÖK-1…ÖK-7) listede değildir.
  - **9.3.5 (3) Bekleyen işler** — altı sayaç, `02 §10.6.1`'in sırasıyla ve adlarıyla (OB-06, 2.6.1): ödeme onayı bekleyen · kargoya verilecek — geri dönen Teslim edilemedi siparişleri dahil · teslim işareti bekleyen · tamamlanmayı bekleyen hizmet · açık talep · iade ve geri ödeme bekleyen — sağlayıcıda gerçekleşmeyen kart iadesi dahil. Sıfır olan sayaç da görünür ve sıfır yazar. Sayaç kalan süre göstermez; süreler listede ve siparişte görünür (2.6.3).
  - **9.3.6 (4) Müşteriden beklenenler** — "IBAN bekleniyor" listesi (2.6.2; K-750): kalemler satır satır — sipariş numarası, kalemin adı, isteğin sebebi ve tarihi; süresi olan hatta geri ödemenin kalan süresi —; kalan süresi en az olan önce, süre sayacı olmayan satırlar — ayıp talebinin çözümü ve tutar bazlı geri ödeme — onların ardından isteğin tarihiyle (`02 §10.6.1`; K-716). İlk on satır gösterilir; tamamı sipariş listesinin "IBAN bekleniyor" süzgecindedir (9.8.4). Liste sayaç değildir ve bekleyen işlerde sayılmaz — iş müşteridedir (K-525).
  - **9.3.7 Yoktur:** satış özeti — Raporlar'dadır (K-750) · yedinci sayaç (`02 §10.6.1`) · bekleyen işlerin e-postayla hatırlatılması (`02 §10.6.1`) · ayrı bir "bekleyen işler" ekranı — sayaç var olan listenin süzülmüş hâline götürür (`02 §10.6.1`) · kurulum sihirbazı ve örnek içerik (`02 §10.8.1`, §10.8.2).
- **Aksiyonlar**
  - **9.3.8** Uyarı satırı → 3.4.3'ün hedefi · kontrol listesi maddesi → kendi ayar ekranı (3.4.4) · sayaç → süzülmüş liste (3.4.2) · "IBAN bekleniyor" satırı → E-37 · "Tümünü gör" → E-36'nın "IBAN bekleniyor" süzgeci (3.4.5). Hiçbiri onay istemez; ekranın kendi işlemi ve mesajı yoktur.
- **9.3.9 Validasyonlar:** form yoktur.
- **Durum × rol varyantları** (tam matris §6.3'te — 5. oturum)
  - **9.3.10 İlk kurulumda** site boştur (`02 §10.8.1`; `10 §2` KP-65): sayfa uyarılarda satışın kapalı olduğunu ve karşılanmayan koşulları, altında kurulum kontrol listesini gösterir; sayaçlar sıfırdır; "Müşteriden beklenenler" görünmez (K-750).
  - **9.3.11 Satış kapalıyken** (OB-08, 2.8.2): uyarı satırı sebebi adıyla söyler; geçici kapatmada kontrol listesi tamamlanmış olsa da satır durur — anahtarın yeri E-44'tür. Açık siparişler etkilenmez: sayaçlar ve liste işlemeye devam eder (`03 §1.10.8`, §8.7.1.2).
  - **9.3.12** Uyarı koşulu kalkınca kendiliğinden kalkar (2.5.5): "firma bildirimleri ulaşmıyor" firmaya giden bir sonraki bildirim ulaşınca ya da iletişim e-postası değiştirilince kalkar (K-760); kanal uyarısıyla birlikte de görünebilir. Sitenin kesintisinden dönüşte sayaçlar mevcut durumlardan hesaplanır ve güncel hâli gösterir (`03 §10.3.5`).
- **9.3.13 Boş / yükleniyor / hata durumları.** Boş bölüm görünmez (§2.7.1.1); bekleyen işler bölümü hiçbir zaman boş değildir — sayaçlar sıfır yazar (§2.7.1.3). Bölümler kendi yerlerinde yüklenir (§2.7.2); sayfa açılamazsa panel çerçevesinin içinde §2.7.3.2.
- **9.3.14 Responsive notları.** Dar sınıfta dört bölüm aynı sırayla alt alta gelir ve sayaçlar ikişerli dizilir (K-750); orta sınıfta sayaçlar üçerli, geniş sınıfta tek sırada durur. Uyarı satırları satır kırar, kesilmez. Sayaçtan siparişe en çok üç adımdır (2.1.2; K-741).

*Kaynak: `02 §3.1.5`, §3.1.6, §3.33.9, §6.1.1, §6.9, §9.1.6, §10.6.1, §10.8.1, §10.8.2 · `03 §1.7.3`, §3.3.24, §3.3.26, §3.5.2.3, §3.5.2.10, §3.5.4.1, §3.5.4.4, §3.5.4.10, §4.1.29, §4.2.10, §6.3.1.1, §7.1.29–§7.1.34, §7.2.15, §8.6.4.1, §8.7.1.1, §8.7.1.2, §8.9.1, §10.1.2.2, §10.1.2.3, §10.3.5 · `10 §2` KP-38, KP-50, KP-65, KP-68 · `10 §4.1` ÖK-8…ÖK-11 · K-741, K-750, K-760, K-761, K-769, K-799, K-802 · devir: K-491, K-498, K-522 (kanal uyarısının metni), K-525, K-535, K-540, K-576 (kapı uyarıları), K-581, K-613.*

### 9.4 E-32 — Ürün listesi

- **9.4.1 Aktör · giriş · çıkış.** Yönetici. Giriş: menünün Katalog › Ürünler'i (3.2.4) · ürün formundan listeye dönüş (3.4.20). Çıkış: "Yeni ürün" ve ürün satırı → E-33 (3.4.10).
- **9.4.2 İlk görülen:** ürünler en yeni eklenen önce, arama alanı ve yayın durumu süzgeciyle; birincil düğme "Yeni ürün".
- **Bilgi hiyerarşisi**
  - **9.4.3** (1) Başlık ve "Yeni ürün" · (2) arama — ürün adıyla — ve yayın durumu süzgeci: Taslak · Yayında · Arşiv (K-737; 2.13.2) · (3) tablo (OB-20) — satır: ana görsel, ürün adı, tip, yayın durumu rozeti (2.5.1.3), varyant sayısı, fiyat — varyantların fiyatı ayrışıyorsa en düşük ve en yüksek —, stok durumu: fiziksel üründe toplam stok ya da "Tükendi", hizmette kontenjan ya da sınırsız olduğu, dijital üründe boş (K-802) · (4) sayfa gezinmesi — sayfa başına 25 kayıt (§2.13.3).
  - **9.4.4 Yoktur:** satırda işlem — yayın durumu geçişleri ve kalıcı silme ürün formundadır (K-802) · stok durumu, kategori ve tip süzgeci (K-737) · ürünlerin elle sıralanması, CSV ile içe aktarma ve listeden toplu fiyat ve stok güncellemesi (`02 §10.7.2`; `03 §8.1.15`).
- **Aksiyonlar**
  - **9.4.5 Aramak ve süzmek** — liste ilk sayfasına döner (§2.13.3); arşivdeki ve taslaktaki ürün yalnız burada aranarak bulunur — vitrinin araması onu göstermez (K-737). Onay ve mesaj yoktur.
  - **9.4.6** "Yeni ürün" → E-33'ün boş formu; yeni ürün ilk kayıtta Taslak doğar (`03 §8.1.1`) · ürün satırı → E-33 (3.4.10).
- **9.4.7 Validasyonlar.** Arama alanı serbest metindir; biçim denetimi yoktur.
- **9.4.8 Durum × rol varyantları.** Satır ürünün yayın durumunu rozetle taşır; varyantların ayrı durumu ürün formundadır. Bütün varyantları tükenmiş fiziksel ürün satırda "Tükendi" yazar ve listede kalır (`02 §3.6.4`). Satış kapısı listeyi etkilemez. Tam matris §6.3'tedir (5. oturum).
- **9.4.9 Boş / yükleniyor / hata durumları.** Hiç ürün yoksa boş hâl satırı "Yeni ürün"le ilk kaydı açtırır (§2.7.1.2); arama ya da süzgecin sonucu boşsa satır aramayı ve süzgeci kaldırma yolunu taşır. Tablo kendi yerinde yüklenir (§2.7.2); sayfa açılamazsa §2.7.3.2.
- **9.4.10 Responsive notları.** Orta sınıfta tip ve varyant sayısı satırın altına iner; dar sınıfta kart listesidir — kart ana görseli, ürün adını, yayın durumu rozetini ve stok durumunu taşır; birincil işlemi formu açmaktır (§2.13.4; K-802).

*Kaynak: `02 §3.6.4`, §3.7, §5.1, §6.6, §10.1.2, §10.7.2 · `03 §8.1.1`, §8.1.8, §8.1.15 · `10 §2` KP-39, KP-40 · K-737, K-766, K-802 · §1.3 GAP-9.*

### 9.5 E-33 — Ürün formu

- **9.5.1 Aktör · giriş · çıkış.** Yönetici. Giriş: ürün listesinin "Yeni ürün"ü ve ürün satırı (3.4.10) · "Taslak" bandının "Düzenlemeye dön"ü (3.7.4). Çıkış: "Önizle" ya da "Sitede gör" → E-04 (3.4.11, 3.7.2) · stok alanının yanındaki sipariş → E-37 (3.4.19) · listeye dönüş → E-32 (3.4.20).
- **9.5.2 İlk görülen:** ürünün adı, tipi ve yayın durumu rozeti; birincil düğme "Kaydet". Yayın durumunun geçişleri ve kalıcı silme ikincil düğmelerdir.
- **Bilgi hiyerarşisi** — bölümlü form (OB-12); geniş sınıfta bölümler solda, yayın durumu yan sütunda:
  - **9.5.3 (1) Temel bilgiler:** tip — fiziksel ürün, dijital ürün, hizmet; ürün düzlemindedir ve varyantlar devralır (`02 §3.2.2`, §3.2.3) · ad (P-27) · kategoriler — ağaçtan birden çok seçim, biri ana kategori; ağaç E-34'ün sırasıyla gelir (`02 §3.4.4`; K-805) · açıklama — kapalı metin biçimi setiyle (`02 §3.11.6`).
  - **9.5.4 (2) Görseller** (OB-19, galeri): yükleme, elle sıra, ana görsel işareti ve görsel başına isteğe bağlı alternatif metin; alan boşken sistemin üreteceği metni gösterir (`02 §3.11.2`, §3.11.4; K-564).
  - **9.5.5 (3) Seçenekler ve varyantlar:** en fazla iki seçenek boyutu ve değerleri (P-21; `02 §3.3.2`) · varyant tablosu — her satır gerçekten açılmış bir kombinasyondur: seçenek değerleri · stok kodu (K-801) · KDV dahil fiyat (`02 §3.8.1`) · fiziksel üründe stok, hizmette isteğe bağlı kontenjan — boşsa sınırsız —, dijitalde stok alanı yoktur (`02 §3.6.2`) · stok ya da kontenjanın yanında ödemesi beklenen siparişlerin ayırdığı adet ve onu tutan siparişler, siparişe giden bağlantıyla (`02 §3.6.7`; K-653) · varyantın yayın durumu rozeti · isteğe bağlı varyant görseli · dijitalde varyantın dosyası. Açılmamış kombinasyonun satırı yoktur; vitrinde görünür ama seçilemez (`02 §3.3.3`). Seçeneği olmayan ürün tek varyantlıdır ve tablo tek satırdır (`02 §3.3.1`).
  - **9.5.6 (4) Fiyat etiketi ve vergi:** KDV oranı — ürün düzleminde; yeni üründe E-46'nın varsayılanıyla (P-10) gelir (`02 §3.8.2`) · üretim yeri — fiziksel üründe zorunlu · ölçü birimi ve net miktar — isteğe bağlı; yanında K-798'in ölçü birimi hatırlatması durur (K-573) · fiyatın başlangıç tarihi ve yerli üretim logosu sistemin gösterdiği bilgilerdir, formda alanları yoktur (`02 §3.8.5`).
  - **9.5.7 (5) Teslim ve cayma:** fiziksel üründe kargoya verme süresi (P-6; çiti P-5) · hizmette ifa süresi — zorunlu (P-44) · ürün başına "bir siparişte en fazla" adet — isteğe bağlı (P-9) · cayma istisnası işareti ve kapalı listeden sebebi — mutlak ya da koşullu, listenin metinleri Yönetmelik bentlerinindir (`02 §7.3.4`; K-509). Bu alanların değişikliği yalnız yeni siparişlere işler ve bölüm bunu bir cümleyle söyler (`02 §3.23.1`; `03 §8.1.4`).
  - **9.5.8 (6) İndirim:** yüzde, başlangıç ve bitiş (OB-16, tarih aralığı) · her varyant için sistemin hesapladığı referans fiyat ve indirimli fiyat, salt okunur (`02 §3.9.3`) · indirimin hâli — başlamadı, sürüyor ya da bitti — satır metniyle (`03 §1.10.1`).
  - **9.5.9 (7) Dijital dosya** — dijital üründe: ürün düzleminde tek dosya; varyant kendi dosyasını taşıyabilir (OB-19; P-26; `02 §3.12.2`) · yayındaki dosyasız varyant işaretlidir (`02 §3.7.4`) · dosya güncellenince geçmiş alıcıların da yeni hâli indirdiği ve indirmenin haktan düştüğü bir cümleyle söylenir (`02 §3.12.7`, §3.12.8).
  - **9.5.10 (8) Yayın durumu yan sütunu:** ürünün yayın durumu rozeti (2.5.1.3) · yayın kapısının koşulları — karşılanmayan adıyla: ad, ana kategori, tip, en az bir Yayında varyant; dijitalde her yayındaki varyantın dosyası, hizmette ifa süresi (`02 §3.7.3`, §3.7.4) · "Önizle" (taslak ürün) ya da "Sitede gör" (yayındaki ürün) · durum geçişlerinin düğmeleri — yayına al, taslağa al, arşive al — ve kalıcı silme · yayındaki ürünün adresi — ilk yayında sabitlenir (`02 §3.30.2`).
  - **9.5.11 Yoktur:** ileri tarihli yayın (`02 §3.7.1`) · ürünlerin elle sıralanması (`02 §3.5.3`) · garanti bilgisi alanı (`02 §3.11.7`) · referans fiyatın elle girilmesi (`02 §3.9.3`) · ürün içe aktarma ve toplu güncelleme (`02 §10.7.2`).
- **Aksiyonlar**
  - **9.5.12 Kaydetmek** (`03 §8.1.1`–§8.1.6): yeni ürün ilk kayıtta Taslak doğar; başarı kısa süreli bildirimle söylenir (2.3.1.4). Zorunlu alanı boşaltan ya da yayın kapısını bozan kayıt kaydedilmez ve engel yerinde söylenir (2.12.1.5; K-799). Fiyat değişikliği fiyat geçmişine yazılır; fiyat düzenlemesi ve stok girişi işlem izine yazılmaz (`03 §8.1.3`, §8.1.10). Onay istemez.
  - **9.5.13 Yayına almak** (`03 §8.1.7`): kapı eksikse ürün yayına alınmaz ve eksik koşullar yan sütunda adıyla gösterilir — dijitalde dosyası eksik varyant, hizmette ifa süresi (K-799). Onay istemez; yayın durumu değişikliği işlem izine yazılır.
  - **9.5.14 Taslağa ve arşive almak** (`03 §8.1.8`): ürünün ve varyantın — varyantın kendi satırından — geçişleri serbesttir; yayındaki ürünün son Yayında varyantının arşive ya da taslağa alınması engellenir ve K-799'un mesajı önce ürünü taslağa ya da arşive almayı söyler (`03 §1.8.2`). Vitrinden kalkan kalem sepetlerden çıkar ve müşteriye bir kez söylenir (`03 §2.3.5`). Onay istemez.
  - **9.5.15 Kalıcı silmek** (`03 §8.1.9`): ürün yan sütundan, varyant kendi satırından silinir. Onay penceresi (OB-04, geri alınamaz — 2.4.2.2) silinen kayda bağlı açık siparişlerin sayısını söyler; silme engellenmez ve açık sipariş donmuş kalemiyle sürer (K-655; cümlesi K-797). Yayındaki ürünün son Yayında varyantı silinmez (K-674; K-799).
  - **9.5.16 Önizlemek** → E-04 (3.4.11): taslak ürün "Taslak" bandıyla açılır (§2.9.2); önizleme kaydedilmiş hâli gösterir.
  - **9.5.17 İndirim tanımlamak** (`03 §8.1.11`): indirimli fiyat bir varyantta referansın altına inmiyorsa, %100 girildiyse ya da yuvarlanmış indirimli fiyat bir varyantta sıfıra iniyorsa indirim kaydedilmez ve sebebi yerinde söylenir (`02 §3.9.1`, §3.9.5; K-600; K-799).
  - **9.5.18 Stoğu ya da kontenjanı girmek** (`03 §8.1.3`): ayrılmış adedin altına inen değer reddedilir; mesaj ayrılmış adedi ve onu tutan siparişleri gösterir; malı gerçekten olmayan firma siparişi E-37'de firma iptaliyle kapatır (K-653; 3.4.19). Artırmak ve kontenjanı boşaltmak serbesttir.
  - **9.5.19 Dosya yüklemek ve silmek** (`03 §8.1.6`): yükleme ilerlemeyi gösterir ve sürerken form kaydedilemez (2.12.8.4); yayındaki bir varyantı dosyasız bırakacak silme engellenir (K-799). Dosya güncellemesi bildirim üretmez (`03 §3.5.2.8`).
  - **9.5.20 Eşzamanlı düzenleme** (OB-10, kayıt varyantı; `02 §3.31.1`): ikinci kaydeden kaydın o arada değiştiğini görür ve iki yoldan birini seçer — güncel hâli açmak ya da uyarıyı görerek üzerine yazmak; metinler K-800'dedir.
- **9.5.21 Validasyonlar.** Ad zorunlu ve P-27 ile sınırlı · fiyat sıfırdan büyük ve en çok iki ondalıklı (`02 §3.8.4`) · stok kodu zorunlu ve tekil (K-801) · stok tam sayı ve ayrılmış adedin altında olamaz · kargoya verme süresi P-5'in çitinde, ifa süresi P-44'ün çitinde · görsel ve dosya P-22…P-26'nın sınırlarında (2.12.8.3) · en fazla iki seçenek boyutu · fiziksel üründe üretim yeri zorunlu. Ret alan mesajıdır, yayın kapısının ve engellerin mesajı yerinde kalıcıdır (K-799). Alan envanteri §7.2'dedir (5. oturum).
- **9.5.22 Durum × rol varyantları.** Tipe göre alanlar değişir: fiziksel üründe stok, üretim yeri ve kargoya verme süresi; dijital üründe dosya, stok alanı yok; hizmette ifa süresi ve kontenjan (`02 §3.2.2`, §3.6.2). Yayın durumuna göre yan sütunun düğmeleri değişir: taslakta "Önizle" ve yayına alma, yayında "Sitede gör", taslağa ve arşive alma; arşivdeki ürünün adresi "Bu ürün artık satılmıyor" sayfasını döner (E-05; `02 §3.7.6`). Tam matris §6.3'tedir (5. oturum).
- **9.5.23 Boş / yükleniyor / hata durumları.** Yeni ürünün formu boş gelir: KDV oranı varsayılanla, stok kodu sistemin önerisiyle (K-801). Görsel ve dosya yüklemesi kendi yerinde ilerler (2.12.8.4); kayıt beklenmeyen bir sebeple tamamlanamazsa form korunur (§2.7.3.1); sayfa açılamazsa §2.7.3.2.
- **9.5.24 Responsive notları.** Dar ve orta sınıfta tek sütundur; yan sütunun yayın durumu ve kapı koşulları formun başına gelir. Varyant tablosu dar sınıfta varyant kartlarına döner — kart seçenek değerlerini, fiyatı, stoğu ve yayın durumunu taşır, öteki alanlar kart açılınca gelir (§2.13.4). Hiçbir alan bir sınıfta gizlenmez.

*Kaynak: `02 §3.2.2`, §3.2.3, §3.3.1–§3.3.4, §3.4.4, §3.5.3, §3.6.2, §3.6.4, §3.6.7, §3.7.1, §3.7.3–§3.7.8, §3.8.1, §3.8.2, §3.8.4, §3.8.5, §3.9.1, §3.9.3, §3.9.5, §3.11.2, §3.11.4, §3.11.6, §3.11.7, §3.12.2, §3.12.7, §3.12.8, §3.23.1, §3.30.2, §3.31.1, §5.1, §5.11, §6.6, §7.3.4, §10.1.2, §10.1.3, §10.7.2, §11.1 · `03 §1.4.4`, §1.8.1, §1.8.2, §1.10.1, §2.3.5, §3.5.1.1, §3.5.1.4–§3.5.1.7, §3.5.1.9–§3.5.1.14, §3.5.1.16, §3.5.2.8, §4.1.23, §4.1.27, §6.3.1.1, §8.1.1–§8.1.11 · `10 §2` KP-39–KP-45, KP-76 · K-737, K-799, K-800, K-801, K-805 · devir: K-509 (sebep listesi), K-511 (fiyat etiketi alanları), K-538 (yayın kapısının engeli), K-564 (üretilen alternatif metin), K-573 (ölçü birimi hatırlatması), K-600 (sıfır fiyat ve %100 indirim), K-653 (ayrılmış adet), K-655 (silme onayı), K-674 (son varyantın silinmesi); `02 §3.3.4`'ün stok kodu devri — K-801.*

### 9.6 E-34 — Kategori ağacı

- **9.6.1 Aktör · giriş · çıkış.** Yönetici. Giriş: menünün Katalog › Kategoriler'i (3.2.5). Çıkış: çerçevenin geçişleri (§3.2). Ürünün kategorilere asılması ürün formundadır (9.5.3).
- **9.6.2 İlk görülen:** kategori ağacı; birincil düğme "Yeni kategori" — birinci seviyede kategori açar.
- **Bilgi hiyerarşisi**
  - **9.6.3** (1) Başlık ve "Yeni kategori" · (2) ağaç — en fazla üç seviye (P-20; `02 §3.4.1`); aynı seviyedeki kategoriler adlarının alfabetik sırasıyla dizilir (K-805). Her düğüm: kategorinin adı · doğrudan asılı ürün sayısı ve alt kategori sayısı · yayında ürünü yoksa — alt dallarında da — vitrin menüsünde görünmediğini söyleyen satır metni (`02 §3.28.6`) · düğümün işlemleri: alt kategori ekle — üçüncü seviyede yoktur —, yeniden adlandır, taşı, sil.
  - **9.6.4 Yoktur:** kategorilerin elle sıralanması (K-805) · üç seviyeden derin dal (`02 §3.4.1`) · ürünün bu ekrandan asılması — asma ürün formundadır (`02 §3.4.4`).
- **Aksiyonlar**
  - **9.6.5 Eklemek ve yeniden adlandırmak** (`03 §8.1.14`): ad girilir; başarı kısa süreli bildirimle söylenir (2.3.1.4). Onay istemez.
  - **9.6.6 Taşımak** (`03 §8.1.14`): düğüm için yeni üst kategori ya da birinci seviye bir seçim listesinden seçilir — işlem klavyeyle de yapılır (§2.1.4). Üç seviyeyi aşacak ya da kategoriyi kendi alt dalına götürecek taşıma yapılmaz ve sebebi yerinde söylenir (`02 §3.4.6`; K-799). Onay istemez.
  - **9.6.7 Silmek** (`03 §8.1.14`): içinde ürün ya da alt kategori bulunan kategori silinmez; engel, engelleyenlerin sayısıyla yerinde söylenir ve önce taşımayı önerir (`02 §3.4.5`; K-799). Boş kategori silinir; onay istemez — kalıcı silme onayları kaynakta sayılıdır ve kategoriyi saymaz (2.4.2).
- **9.6.8 Validasyonlar.** Ad zorunludur — alan mesajı (OB-12). Alan envanteri §7.2'dedir (5. oturum).
- **9.6.9 Durum × rol varyantları.** Kategorinin durumu yoktur; menüdeki görünürlüğü yayındaki ürünlerden okunur ve düğümün satır metni bunu söyler (9.6.3). Yayında ürünü olmayan kategorinin adresi vitrinde boş kategori sayfası döner (E-02; `02 §3.28.6`). Tam matris §6.3'tedir (5. oturum).
- **9.6.10 Boş / yükleniyor / hata durumları.** Hiç kategori yoksa boş hâl satırı "Yeni kategori"yle ilk kaydı açtırır (§2.7.1.2); ürün ana kategorisi olmadan yayına alınamadığı için satır bunu da söyler (`02 §3.7.3`). Sayfa açılamazsa §2.7.3.2.
- **9.6.11 Responsive notları.** Dar sınıfta ağaç katlanarak açılır, düğümün işlemleri düğümün kendi menüsündedir ve taşıma seçim listesiyle yapılır — sürükle-bırak gerekmez. Uzun kategori adı satır kırar.

*Kaynak: `02 §3.4.1`, §3.4.4–§3.4.6, §3.7.3, §3.28.6, §6.6.2, §6.6.3, §10.1.2 · `03 §2.1.2`, §3.5.1.2, §3.5.1.3, §8.1.14 · `10 §2` KP-41 · K-799, K-805.*

### 9.7 E-35 — Kuponlar

- **9.7.1 Aktör · giriş · çıkış.** Yönetici. Giriş: menünün Katalog › Kuponlar'ı (3.2.6). Çıkış: çerçevenin geçişleri (§3.2). Kupon listesi ve kuponun formu aynı ekrandadır.
- **9.7.2 İlk görülen:** kupon listesi, en yeni eklenen önce; birincil düğme "Yeni kupon".
- **Bilgi hiyerarşisi**
  - **9.7.3 (1) Liste** (OB-20; K-802): satır — kod · tür ve değer — yüzde ya da tutar · asgari sepet tutarı · geçerlilik aralığı · kullanım — kullanılmış, ayrılmış ve toplam adet · geçerlilik hâli: başlamadı · geçerli · süresi doldu · hakkı doldu. Hâl bir satır metnidir, rozet değildir — kuponun durum makinesi yoktur (`03 §1.10.2`).
  - **9.7.4 (2) Kuponun formu** — listenin satırı ya da "Yeni kupon" ekranın içinde açar (OB-12): kod — kuponlar arasında tekil, büyük-küçük harf ayrımı olmadan (K-806) · tür — yüzde ya da sabit tutar (`02 §3.10.2`) · değer · asgari sepet tutarı — sabit tutarlı kuponda zorunlu, yüzdeselde isteğe bağlı (`02 §3.10.4`) · başlangıç ve bitiş (OB-16, tarih aralığı) · toplam kullanım adedi — kişiye özel kod adedi 1 olan koddur (`02 §3.10.5`) · kullanılmış ve ayrılmış hakların sayısı, salt okunur (`02 §3.10.6`; K-654).
  - **9.7.5 Yoktur:** kuponun silinmesi — kampanya bitiş tarihi öne çekilerek durdurulur (K-806; `03 §8.1.13`) · arama ve süzgeç (K-737) · siparişe ikinci kod (`02 §3.10.1`).
- **Aksiyonlar**
  - **9.7.6 Kuponu kaydetmek** (`03 §8.1.12`, §8.1.13): başarı kısa süreli bildirimle söylenir (2.3.1.4); onay istemez. Değişiklik yalnız yeni siparişlere işler; onaylanmış siparişteki kupon payı donmuştur (`02 §3.10.3`; K-806). Kullanım adedi kullanılmış ve ayrılmış hakların toplamının altına indirilemez; K-799'un mesajı kampanyayı durdurmanın yolunu da söyler (K-654). Kampanyayı durdurmak bitiş tarihini öne çekmektir: yeni siparişte kupon kabul edilmez, ayrılmış haklar onaylanmış siparişlerde kalır.
  - **9.7.7 Eşzamanlı düzenleme** — OB-10'un kayıt varyantı (`02 §3.31.1`; K-800).
- **9.7.8 Validasyonlar.** Kod zorunlu ve tekil (K-806) · değer sıfırdan büyük · yüzdesel kupon %100'e ulaşamaz · sabit tutarlı kuponun asgari sepet tutarı kupon tutarından büyük · bitiş başlangıçtan önce olamaz · kullanım adedi tam sayı ve 9.7.6'nın sınırında (`02 §3.10.4`, §3.10.6; `03 §3.5.1.8`, §3.5.1.15). Ret alan mesajıdır; metinleri K-799'dadır. Alan envanteri §7.2'dedir (5. oturum).
- **9.7.9 Durum × rol varyantları.** Geçerlilik hâli tarih aralığından ve kalan haktan okunur (`03 §1.10.2`; Z-22); süresi ya da hakkı dolmuş kupon düzenlenebilir — bitiş ileri alınır ya da adet artırılırsa yeniden geçerli olur. Tam matris §6.3'tedir (5. oturum).
- **9.7.10 Boş / yükleniyor / hata durumları.** Hiç kupon yoksa boş hâl satırı "Yeni kupon"la ilk kaydı açtırır (§2.7.1.2). Kayıt beklenmeyen bir sebeple tamamlanamazsa form korunur (§2.7.3.1); sayfa açılamazsa §2.7.3.2.
- **9.7.11 Responsive notları.** Geniş sınıfta form listenin yanında açılır; orta ve dar sınıfta listenin yerine gelir ve listeye dönüş taşır. Dar sınıfta liste kart listesidir — kart kodu, değeri, geçerlilik hâlini ve kullanımı taşır (§2.13.4; K-802).

*Kaynak: `02 §3.10.1`–§3.10.6, §5.11, §6.6.8, §6.6.15, §10.1.2 · `03 §1.4.4`, §1.10.2, §3.5.1.8, §3.5.1.15, §4.1.25, §8.1.12, §8.1.13 · K-737, K-799, K-800, K-802, K-806 · devir: K-565 (kupon formu), K-654 (kullanılmış ve ayrılmış haklar).*

### 9.8 E-36 — Sipariş listesi

- **9.8.1 Aktör · giriş · çıkış.** Yönetici. Giriş: menünün Siparişler'i (3.2.2) · ana sayfanın sayaçları — beşinin süzülmüş hâli (3.4.2) · ana sayfanın "e-posta ulaşmadı" ve kart iadesi uyarı satırları (3.4.3) · "Müşteriden beklenenler"in "Tümünü gör"ü (3.4.5) · sipariş ayrıntısından listeye dönüş (3.4.20). Çıkış: sipariş satırı → E-37 (3.4.6).
- **9.8.2 İlk görülen:** siparişler en yeni önce; sayaçtan ya da uyarıdan gelindiyse süzgeç seçili ve adı listenin başında durur. Ekranın birincil düğmesi yoktur; satır siparişi açar (K-802).
- **Bilgi hiyerarşisi**
  - **9.8.3 (1) Arama** — tek alan: sipariş numarası ya da siparişin iletişim e-postası (`03 §8.2.11`; K-714). Değer tam eşleşmeyle aranır; e-postada büyük-küçük harf ayrımı yoktur (K-802). Teyit akışları, iletişim talebindeki numara ve telefonla ulaşan müşteri siparişe bu yoldan varır (`03 §6.2.1.2`).
  - **9.8.4 (2) Süzgeçler** (K-802): Sipariş durumu — altı değer, birden çok seçilebilir · Ödeme durumu — beş değer, birden çok seçilebilir · bekleyen iş ve işaret — tek seçim: ödeme onayı bekleyen · kargoya verilecek · teslim işareti bekleyen · tamamlanmayı bekleyen hizmet · iade ve geri ödeme bekleyen · "IBAN bekleniyor" · "e-posta ulaşmadı". İlk beşi ana sayfanın aynı adlı sayaçlarının kapsamını süzer ve sayaçtaki sayı süzülmüş listenin satır sayısına eşittir (`02 §10.6.1`); "açık talep" sayacı Talepler'e götürür (3.4.2). Arama ile süzgeçler birlikte uygulanır.
  - **9.8.5 (3) Tablo** (OB-20; K-802) — satır: sipariş numarası · sipariş tarihi · alıcı adı ve iletişim e-postası · kalemler — ilk kalemin adı ve kalan kalem sayısı · toplam · ödeme yöntemi · Sipariş durumu ve Ödeme durumu rozetleri, yan yana (2.5.1.1, 2.5.1.2; K-777) · panel işaretleri — "e-posta ulaşmadı", "ödeme sonucu alınamadı", "geri ödeme gerçekleşmedi", "e-posta düzeltildi" (2.5.3) · süre — siparişte işleyen sürelerden kalanı en az olanı, adıyla; aşılmışsa aşım (2.6.3, 2.6.6).
  - **9.8.6 (4) Sayfa gezinmesi** — sayfa başına 25 sipariş (§2.13.3).
  - **9.8.7 Sıra** (K-802): en yeni önce. Süreli bir bekleyen iş süzgecinde — kargoya verilecek, tamamlanmayı bekleyen hizmet, iade ve geri ödeme bekleyen, "IBAN bekleniyor" — kalan süresi en az olan önce, süresi aşılmış sipariş en başta; süre sayacı olmayan satır sonda. Ziyaretçiye ve yöneticiye sıralama seçeneği sunulmaz.
  - **9.8.8 Yoktur:** toplu işlem — toplu kargoya verme, toplu durum değişikliği, toplu yeniden gönderim (`02 §10.7.2`; K-761) · satırdan işlem — işlemler siparişin ayrıntısındadır; zamana duyarlı iş sayaç → liste → sipariş yolunda en çok üç adımdır (2.1.2; K-741) · tarih aralığı, ödeme yöntemi ve tutar süzgeci (K-714, K-802) · elle sipariş oluşturma (`02 §10.4.1`) · listeden dışa aktarma — Raporlar'dadır (E-53; `02 §10.7.1`).
- **Aksiyonlar**
  - **9.8.9 Aramak ve süzmek** — liste ilk sayfasına döner; seçili süzgeçlerin adı listenin başında durur ve tek dokunuşla kaldırılır (§2.13.3). Onay ve mesaj yoktur.
  - **9.8.10 Siparişi açmak** — satır → E-37 (3.4.6); listeye dönüş bırakılan sayfayı, aramayı ve süzgeci bulur (3.4.20).
- **9.8.11 Validasyonlar.** Arama alanı serbest metindir; biçim denetimi yoktur — eşleşme yoksa boş hâl satırı çıkar.
- **9.8.12 Durum × rol varyantları.** Kendiliğinden iptal edilmiş ödenmemiş sipariş müşterinin listesinde görünmez, burada İptal edildi + Başarısız rozetleriyle görünür (`03 §3.2.2.4`, §3.2.1.19). Açık ödenmemiş siparişler "ödeme onayı bekleyen" ve Bekliyor süzgeçleriyle görülür; şüpheli olanlar siparişin ayrıntısında firma iptaliyle kapatılır (`03 §6.3.1.2`; K-613). Teslim edilemedi siparişleri "kargoya verilecek" süzgecindedir (`03 §3.5.2.3`; K-535). Satış kapısının durumu listeyi etkilemez (`03 §1.10.8`). Tam matris §6.3'tedir (5. oturum).
- **9.8.13 Boş / yükleniyor / hata durumları.** Hiç sipariş yoksa boş hâl satırı neyin olmadığını söyler; ilk kaydı açan düğme yoktur — siparişler vitrinden gelir (§2.7.1.2; `02 §10.4.1`). Arama ya da süzgecin sonucu boşsa satır aramayı ve süzgeci kaldırma yolunu taşır; sıfır sayaçtan gelinen liste boş hâl satırıyla açılır (3.4.2). Tablo kendi yerinde yüklenir (§2.7.2); sayfa açılamazsa §2.7.3.2.
- **9.8.14 Responsive notları.** Geniş sınıfta tablodur. Orta sınıfta alıcı, kalemler ve ödeme yöntemi satırın altına iner. Dar sınıfta kart listesidir ve kart dört bilgiyi şu öncelikle taşır: (1) sipariş numarası ve tarihi · (2) iki rozet · (3) işaretler · (4) toplam ve süre; kartın birincil işlemi siparişi açmaktır, alıcı ve kalemler ayrıntıdadır (§2.13.4; K-741, K-802). Arama ve süzgeçler dar sınıfta listenin üstünde açılır bölümdedir.

*Kaynak: `02 §3.17.8`, §5.4, §5.5, §6.7, §9.1.6, §10.1.2, §10.4.1, §10.6.1, §10.7.1, §10.7.2 · `03 §1.7.3`, §2.5.1.3, §2.5.1.4, §2.6.5, §3.1.1, §3.2.1.1, §3.2.1.19, §3.2.2.4, §3.5.2.3, §3.5.2.13, §4.1.7, §4.1.29, §6.1.4.3, §6.2.1.2, §6.3.1.1, §7.1.37, §7.1.38, §8.2.1, §8.2.11, §8.6.4.3, §8.9.1, §9.4.5, §10.1.1.3, §10.1.2.6 · `10 §2` KP-46, KP-50 · K-741, K-761, K-766, K-777, K-802 · devir: K-522, K-535, K-585 ("e-posta düzeltildi" işareti), K-611, K-612 (işaret), K-613 (açık ödenmemiş siparişlerin görünümü), K-675 (işaretlerin kalkışı), K-714 (arama ve süzgeç); `03 §8.2.11`'in "ekranın biçimi" devri — K-802.*

### 9.9 E-37 — Sipariş ayrıntısı

- **9.9.1 Aktör · giriş · çıkış.** Yönetici. Giriş: sipariş listesinin satırı (3.4.6) · ana sayfanın "IBAN bekleniyor" satırı (3.4.5) · Talepler'in ayıp talebi satırı — siparişin ayıp talepleri bölümüne (3.4.8) · üye kaydı görünümünün bağlı siparişi (3.4.14) · ürün formunda ayrılmış adedi tutan sipariş (3.4.19) · oturumsuz gelişte girişten sonra (3.5.14, §3.6.1). Çıkış: listeye dönüş — bırakılan sayfa ve süzgeçle (3.4.9, 3.4.20) · işlemler ekranda kalır (3.4.7) · kargo şirketinin takip sayfası (§4.4.4).
- **9.9.2 İlk görülen:** sipariş numarası, iki rozet ve — varsa — panel işaretleri; altında hattın sıradaki zorunlu adımı birincil düğme olarak, işleyen süresiyle (K-803). Zorunlu adım kalmamışsa birincil düğme yoktur.
- **Bilgi hiyerarşisi** — geniş sınıfta iki sütun: solda işin, sağda kaydın bölümleri (K-803):
  - **9.9.3 (1) Başlık:** sipariş numarası ve tarihi · Sipariş durumu ve Ödeme durumu rozetleri, yan yana ve adlarıyla (2.5.1.1, 2.5.1.2; K-777) · ödeme yöntemi · siparişin üye hesabıyla mı girişsiz mi verildiği — e-posta düzeltmesi yalnız girişsiz siparişte sunulur (`02 §10.4.11`) · "Listeye dön".
  - **9.9.4 (2) İşaretler** — siparişin dört işareti, koşul sürdükçe ve metinleriyle (2.5.3; `03 §1.7.3`): **"e-posta ulaşmadı"** — altında ulaşmayan e-postalar tek tek: bildirimin adı, ilk gönderim tarihi ve gittiği adres; her satırda "Yeniden gönder", birden çok satır varsa "Hepsini yeniden gönder"; altında adres hatırlatması (2.5.4; K-761, K-799) · **"ödeme sonucu alınamadı"** — sağlayıcının son sorgusunun sonucunun beklendiği; işlem yoktur, sonuç gelince olağan geçiş işler ve işaret kalkar (`03 §1.7.3.2`, §10.1.1.3) · **"geri ödeme gerçekleşmedi"** — K-798'in uyarısı ve yanında kart iadesini yeniden deneme ile havale yolunu açma (9.9.24) · **"e-posta düzeltildi"** — e-posta geçmişine (9.9.12) götüren bağlantıyla; işaret kalkmaz (`03 §1.7.3.6`). İşaret "e-posta ulaşmadı" ile birlikte durduğunda ikisi ayrı ayrı okunur (2.5.4). Kalemin işaretleri — "IBAN bekleniyor" ve "iade malı bekleniyor" — kalemin satırındadır (9.9.6).
  - **9.9.5 (3) Sıradaki adım ve işlem grupları** — birincil düğme hattın sıradaki zorunlu adımıdır (`02 §10.5.1`; K-803): Alındı + Bekliyor'daki havale siparişinde "Ödendi olarak işaretle" — yanında siparişe donmuş IBAN, ödenecek toplam ve son ödeme günü (`03 §8.2.2`; K-663) · Hazırlanıyor'da açık fiziksel kalem varken "Kargoya ver" — yanında kargoya verme sözü ve kalanı (2.6.3; Z-10) · Kargoya verildi'de "Teslim edildi olarak işaretle" · sipariş düzeyinde zorunlu adım kalmamış ve açık hizmet kalemi varken ilk açık hizmet kaleminin "Tamamlandı olarak işaretle"si — ifa süresinin kalanıyla (Z-39). Süre aşıldıysa gösterge aşımı ve K-798'in kanuni faiz uyarısını yerinde kalıcı mesajla söyler (2.6.3; `03 §8.2.4`). Altında açık olan öteki sipariş işlemleri iki grupta durur (K-803) — **"Sipariş işlemleri":** teslim edilemedi işareti, kargoya yeniden verme, siparişi kapatma, firma iptali, başka kanaldan gelen cayma ya da gecikme feshi bildiriminin kaydı, tutar bazlı geri ödeme, IBAN isteği · **"Düzeltmeler":** adres, kargo şirketi ve takip numarası, teslim tarihi, durum geçişi, misafir siparişinin e-postası. Açık olmayan işlem gösterilmez (`03 §1.11.15`–§1.11.41). Kalemin işlemleri kalemin satırındadır.
  - **9.9.6 (4) Kalemler** — her kalem siparişe donmuş hâliyle (`02 §3.23.1`): görsel, ürün adı ve seçenek değerleri · tip · adet ve açık adet — bir kısmı kapanmış kalemde ikisi birlikte (K-790, K-795) · birim fiyat, indirim ve kupon payı, satır tutarı · cayma istisnası ve sebebi — koşullu sebepte koşulu (`02 §7.3.4`) · **kalemin kayıtları** — kaydın adı, tarihi ve kapsadığı adet; aynı türden ardışık kayıtlar ayrı ayrı (2.5.2; `02 §5.8`): teslim işareti ve teslim tarihi · iptal kaydı — firma iptalinde sebebiyle · çıkarma kaydı · gecikme feshi — K-798'in kanuni faiz uyarısıyla · cayma beyanı — tarih damgası, fiziksel kalemde iade adresi ve, teslimden sonra, gönderme süresinin kalanı ya da geçtiği (Z-42; K-494) · iade teslim alma — ulaşma tarihi ve ulaşan adet · iade reddi · "mal dönmedi" kapanışı · IBAN isteği — tarihi ve sebebi · dijital kalemde indirme sayacı — yapılan indirme ve hak (`03 §1.7.1.11`) · **kalemin işaretleri** — "IBAN bekleniyor" ve "iade malı bekleniyor", bekleyen adetle (2.5.3.4, 2.5.3.5) · **bekleyen geri ödeme** — işleme giren adetler, sistemin hesapladığı tutar (`02 §7.1.7`; K-788), hat ve — havalede — müşterinin girdiği IBAN, salt okunur (OB-14, panelde salt okunur), kalan süresiyle (2.6.3; K-491) · **kalemin işlemleri** (9.9.25–9.9.29).
  - **9.9.7 (5) Ayıp talepleri** — siparişin ayıp talebi bölümü (K-751, K-764): her talep kalemin adı ve ayıplı adediyle, açıklaması, açılış ve yeniden açılış tarihleri, durum rozeti (2.5.1.5) ve fiziksel kalemde talebe yazılmış iade adresiyle (`03 §8.4.10`; K-793). Talebin B-9'u ulaşmamışsa "e-posta ulaşmadı" işareti talebin satırında da görünür (2.5.3.1; K-760). İşlemleri 9.9.30'dadır. Talep yoksa bölüm görünmez.
  - **9.9.8 (6) Tutar ve ödeme kaydı** — donmuş döküm: kalemlerin toplamı, indirim ve kuponun payı — kupon koduyla —, KDV, kargo ücreti ya da "Kargo ücretsiz", ödenen toplam (`02 §3.23.2`). Döküm kısmi iptal ve iadeden sonra yeniden hesaplanmaz (`03 §3.3.10`). Ödeme kaydı tarih sırasıyla: ödeme — yöntem, tarih, tutar · her geri ödeme — tarih, tutar, yol, bağlı olduğu kalem ve adet ya da tutar bazlı olduğu, kart iadesinde sonucu (başlatıldı, gerçekleşti, gerçekleşmedi) · iptal edilmiş siparişe geç gelen kart ödemesi ve sistemin onu geri ödemesi — sipariş İptal edildi + Başarısız kalır (`03 §3.2.1.16`; K-589).
  - **9.9.9 (7) Teslimat** — fiziksel kalem varsa: kargoya verme sözü — siparişe donmuş (`02 §3.23.1`) · kargo şirketi ve takip numarası ya da "kendi aracımızla teslim" beyanı, listedeki şirkette "Takip et" · teslim tarihi (`02 §3.20.7`, §3.20.8). Fiziksel kalemsiz siparişte bölge yoktur.
  - **9.9.10 (8) Müşteri ve adresler** — alıcı adı, siparişin iletişim e-postası, teslimat telefonu, teslimat ve fatura adresi, siparişe donmuş hâliyle (`02 §3.23.3`); yönetici müşteriye bu bilgilerle sistemin dışında ulaşır (`03 §8.3.3.9`).
  - **9.9.11 (9) İç not** — tek serbest metin alanı; alanın üstünde notun müşteriye hiçbir yerde görünmediği yazar (`02 §10.4.7`).
  - **9.9.12 (10) E-posta geçmişi** (K-804): siparişin müşteriye giden bildirimleri tarih sırasıyla — bildirimin adı (B-1…B-16), gönderim tarihi, gittiği adres ve sonucu: gönderildi ya da "e-posta ulaşmadı"; panelden yeniden gönderim ayrı satırdır (Z-43; `03 §5.1.7`) · misafir siparişinin e-posta düzeltmeleri — eski ve yeni adres, düzeltmeyi yapan yönetici ve tarih (`02 §10.4.11`; K-586). Geçmiş müşteriye görünmez (5.16.10). Firmaya ve yöneticilere giden bildirimler (F-1…F-6) burada değildir.
  - **9.9.13 (11) Belgeler** — açılıp kapanan bölümler: siparişin onaylandığı sürümleriyle Ön Bilgilendirme Formu ve Mesafeli Satış Sözleşmesi · siparişin onaylandığı gün yürürlükteki aydınlatma metni · işaretlenen onay kutularının kaydı (`02 §3.23.3`, §3.24.6; `03 §5.1.7`).
  - **9.9.14 Yoktur:** elle sipariş oluşturma, kalem ekleme ve fiyat değiştirme (`02 §10.4.1`, §10.4.3) · panelden müşteriye yanıt yazma (`02 §10.1.2`) · "geri al" düğmesi — yanlış geçiş düzeltmeyle düzeltilir (`02 §7.6.1`) · fatura (`02 §3.25.2`) · havalede gelen tutar alanı (`02 §3.21.7`) · müşterinin IBAN'ını panelden girme — tek istisna başka kanaldan gelen bildirimdeki IBAN'ın aktarılmasıdır (`02 §10.1.2`) · siparişe süzülmüş işlem izi — iz E-52'de düz bir listedir (`02 §10.3.4`) · ters ibraz kaydı (`02 §3.21.11`) · kanuni faizin hesabı (`02 §7.2.3`).
- **Aksiyonlar** — her işlemin açık olduğu aralık `03 §1.11.15`–§1.11.41'dedir ve burada yinelenmez. İşlemin formu sayfanın içinde açılır; onay isteyen işlem formun son adımında pencereyi açar (2.4.4; 3.4.7); sonuç siparişin güncel hâlinde kalıcı durur — kısa süreli bildirimle verilmez (2.3.4). Düğme metinleri ve onay cümleleri K-797'dedir.
  - **9.9.15 "Ödendi" işareti** (`03 §8.2.2`, §1.11.15): onay penceresi siparişin IBAN'ını ve tutarını yineler; dijital kalemli siparişte K-798'in uyarısı cümleden önce gelir (`03 §1.6.1.1`). Gelen tutar sorulmaz (`02 §3.21.7`). İptal edilmiş siparişte sunulmaz; sitenin kesintisinde ertelenen iptalde ertelenen ana kadar sunulur (`03 §4.2.12`). Onay ister. Sonuç: Ö1 ve eşzamanlı geçişler, müşteriye B-4.
  - **9.9.16 Kargoya vermek ve yeniden vermek** (`03 §8.2.5`, §8.2.8, §1.11.16, §1.11.19): form — kargo şirketi ürünle gelen listeden, listede yoksa "Diğer" ve şirketin adı, takip numarası; ya da "kendi aracımızla teslim" (`02 §3.20.7`, §3.20.8). Form gidecek kalemleri salt okunur listeler: açık fiziksel kalemler — iptal edilmiş, çıkarılmış, feshedilmiş ve cayma beyanıyla kapanmış kalem gitmez; seçim yoktur, sipariş tek parça ve tek takip numarasıyla gider (`02 §3.20.3`; K-590). İkisi de onay ister. Sonuç: S5 ya da S8, B-5.
  - **9.9.17 Teslim işareti ve tarihi** (`03 §8.2.6`, §1.11.17): tarih alanı (OB-16, olay tarihi). Tarih bir cayma beyanının günüyle çakışırsa form teslimin beyandan önce mi sonra mı olduğunu sorar ve beyanın saatini gösterir; cevap zorunludur (2.12.5.2; `03 §3.3.34`; K-608). Onay istemez; sonuç S6, bildirim gitmez (`02 §9.2`).
  - **9.9.18 "Teslim edilemedi" ve siparişin kapatılması** (`03 §8.2.7`, §8.3.2.5, §1.11.18, §1.11.40): "Teslim edilemedi olarak işaretle" onay istemez; sonuç S7 — kapanışı tamamlıyorsa S11 —, B-6; sipariş "kargoya verilecek" sayacına girer (K-535). Açık kalemi kalmamış ve hiçbir kalemi teslim edilmemiş Teslim edilemedi siparişinde "Siparişi kapat" sunulur: sebep seçilmez, iptal kaydı ve geri ödeme açılmaz, B-7 gitmez (K-592, K-717); onay istemez — kaynak onu onay isteyen işlemler arasında saymaz (2.4.2).
  - **9.9.19 Firma iptali** (`03 §8.3.1`, §1.11.21): form — **kapsam:** ödeme beklenirken siparişin tamamı, salt okunur liste; ödemeden sonra açık kalemler seçilebilir listedir ve seçilen kalemin satırında adet alanı açılır (OB-12, 2.12.1.8 — üst sınır kalemin iptale açık adedi, varsayılan tamamı; K-794, K-795); Teslim edilemedi'de geri dönen açık fiziksel kalemler; dijital kalem, cayma beyanlı ve feshedilmiş adetler listede yer almaz · **sebep** (OB-15, firma iptali; "diğer"de açıklama zorunlu) · **uyarılar:** "stokta bulunamadı"da K-798'in yasal uyarısı (K-609); "müşteriyle anlaşıldı (müşteri talebi)"nde teyit hatırlatması (K-607); Z-10 ya da Z-39 aşılmışsa kanuni faiz uyarısı (`03 §8.3.1.1`). Onay ister; pencerenin cümlesi kapsama, hatta ve sebebe göre söyler (K-797). Sonuç: ödenmemiş siparişte S3 + Ö2; ödenmişte iptal kaydı ve kapanış kuralı; B-7 iptal anında, sebebiyle ve adetle; kart hattında kart iadesi kendiliğinden başlar ve onay istemez (2.4.6); havale hattında kalemde IBAN isteği ve B-14 (K-690).
  - **9.9.20 Başka kanaldan gelen bildirimin kaydı** (`03 §8.4.8`, §1.11.34, §1.11.41): form — **cayma:** kalem — kargoya verilmiş fiziksel kalemler ve ödemesi onaylanmış hizmet kalemleri; kargoya verilmemiş fiziksel kalem seçilemez ve form yolun "müşteriyle anlaşıldı" iptali olduğunu söyler (K-604) — ve seçilen kalemin satırında adet alanı (2.12.1.8 — varsayılan açık adedin tamamı; K-787) · **gecikme feshi:** kalem ve adet seçilmez; form feshin kapsadığı kalemleri açık adetleriyle salt okunur listeler ve K-798'in kanuni faiz uyarısını gösterir (K-706, K-796) · ikisinde de bildirimin firmaya ulaştığı tarih (OB-16; ileri tarih ve kabul süresinin dışındaki tarih K-799'un mesajıyla reddedilir) · bildirimin siparişin iletişim e-postasından geldiğini söyleyen seçenek ve onunla açılan IBAN alanı — biçim ve sağlama basamağıyla denetlenir; seçenek işaretsizse kayıt IBAN'sız yapılır (OB-14; `02 §10.4.10`; K-581, K-607) · formun başında K-798'in teyit hatırlatması (K-607) · tarih pencerenin dışındaysa ya da kalem mutlak istisnalıysa K-798'in uyarısı ve onay kutusu — kutu işaretlenmeden onay penceresi açılmaz (K-606, K-709) · tarih kalemin teslim günüyse sıra sorusu (K-608). Onay ister (K-715). Sonuç: kalemde cayma beyanı ya da gecikme feshi, B-9; IBAN'sızsa IBAN isteği ve B-14.
  - **9.9.21 Geri ödemeyi işlemek** (`03 §8.3.3.2`, §8.3.3.3, §1.11.31): kalemin bekleyen geri ödemesinin yanında durur; havale hattında IBAN yoksa düğmenin yerinde IBAN isteğinin beklendiği yazar (K-686). Onay penceresi tutarı ve — havalede — IBAN'ı yineler (K-797). Pencere açıkken müşteri IBAN'ı düzelttiyse işlem uygulanmaz ve güncel IBAN gösterilir (`03 §3.5.2.15`; 9.9.34). Havale hattında aynı yerde "Havale gerçekleşmedi" durur: onay istemez; IBAN silinir, sipariş sayfasında alan yeniden açılır ve kalem "IBAN bekleniyor"a döner (`03 §8.3.3.8`, §1.11.33). Sonuç: Ö3–Ö5, B-8; kalem "iade ve geri ödeme bekleyen" sayacından düşer.
  - **9.9.22 Tutar bazlı kısmi geri ödeme** (`03 §8.3.3.7`): form — tutar ve isteğe bağlı olarak bağlı olduğu kalem; tutar siparişte ödenmiş ve henüz geri ödenmemiş tutarı aşamaz ve aşan değer alan mesajıyla reddedilir. Havale hattında önce IBAN istenir (9.9.23). Onay ister. Sonuç: Ö3–Ö5, B-8; kapatılmış kalem kapalı kalır (K-596).
  - **9.9.23 IBAN istemek** (`03 §8.3.3.6`, §1.11.39): havale hattında IBAN'ı olmayan bir geri ödeme için — ayıp talebinin para gerektiren çözümü, tutar bazlı geri ödeme, iade reddinden ya da "mal dönmedi" kapanışından sonraki ödeme kararı —; kalem ve sebep seçilir. Onay istemez. Sonuç: kalemde IBAN isteği, B-14 ve ana sayfanın "Müşteriden beklenenler" listesinde satır (9.3.6). Yönetici IBAN girmez (`02 §10.1.2`).
  - **9.9.24 Kart iadesini yeniden denemek ya da havale yolunu açmak** (`03 §8.3.3.5`, §1.11.32): "geri ödeme gerçekleşmedi" işaretinin yanında, K-798'in uyarısıyla. Yeniden deneme bir geri ödemenin işlenmesidir ve onay ister (`03 §1.6.1.4`); müşteri IBAN girdikten sonra sunulmaz. Havale yolunu açmak onay istemez; sonuç kalemde IBAN isteği ve B-14; IBAN girilene kadar yeniden deneme açık kalır.
  - **9.9.25 Kalem çıkarmak ya da adedini azaltmak** (`03 §8.4.2`, §1.11.22): kalemin satırında; adet alanı (2.12.1.8 — üst sınır kalemin açık adedi, varsayılan tamamı); sebep listesi yoktur. Kart hattında karta para gönderen çıkarma onay ister (`03 §1.6.1.5`); havale hattında IBAN isteği düşer (K-690). Sonuç: çıkarma kaydı, B-11; son açık kalemse kapanış kuralı.
  - **9.9.26 Hizmet kalemini tamamlamak** (`03 §8.2.9`, §1.11.20): kalemin satırında, ödeme onaylanmışken; adet seçilmez — işaret açık adetlerin tamamına düşer (K-792). Onay istemez; ödenmemiş siparişte sunulmaz (K-536).
  - **9.9.27 İade malını teslim almak, reddetmek ve stoğa eklemek** (`03 §8.3.2`, §1.11.28, §1.11.29): kalemin satırında — cayma beyanlı ya da feshedilmiş fiziksel kalemde, "mal dönmedi" ile kapatılmış kalemde ve ayıp talebinde sözleşmeden dönmenin geri alınan malında (K-808). Form: ulaşma tarihi (OB-16 — ileri tarih ve kalemin teslim tarihinden önceki tarih reddedilir; K-554) ve ulaşan adet (2.12.1.8 — varsayılan beyanın henüz ulaşmamış adedinin tamamı; K-794). Koşullu istisna sebebi kaleme donmuş kalemde form ikinci bir seçim taşır: teslim al ya da "iade reddedildi — koruyucu ambalaj açılmış"; ret onay ister (`03 §1.6.1.6`; K-656), teslim alma istemez. Ardından "Stoğa ekle" — teslim alınan adet kadar; varyant silinmişse ve retta sunulmaz (`03 §8.3.2.2`; K-791). Teslim alma geri ödemeyi başlatmaz; teslimden sonraki caymada geri ödemenin süresi ulaşma tarihinden işler ve kalemin bekleyen geri ödemesinde görünür (K-491). Kullanılmış, hasarlı ya da eksik dönen malda ret seçeneği yoktur (`03 §5.3.2.1`).
  - **9.9.28 "Mal dönmedi" kapanışı** (`03 §8.4.7`, §1.11.30): kalemin satırında, Z-42 geçmiş ve beyanın ulaşmamış adedi varken; adet alanı — üst sınır ve varsayılan ulaşmamış adet (2.12.1.8; K-794). Onay istemez; geri ödeme yapılmaz ve kapanış geri ödeme borcuna karar vermez — ödeme kararı 9.9.22 ile, havalede 9.9.23'ten sonra işler (K-596). Mal sonradan ulaşırsa 9.9.27 kalemi yeniden açar.
  - **9.9.29 İndirme hakkını yenilemek** (`03 §8.2.10`, §1.11.38): dijital kalemin satırında; onay istemez; sayaç sıfırlanır ve bildirim gitmez (K-683).
  - **9.9.30 Ayıp talebini yürütmek** (`03 §8.4.10`, §8.4.11, §1.11.37): talebin satırında "Çözüldü olarak işaretle" ve — Z-18 içinde — "Yeniden aç"; ikisi de onay istemez ve müşteriye bildirilmez (`02 §9.2`). Para gerektiren çözüm talebin bölümünden başlar: sözleşmeden dönmede talebin ayıplı adetleri için iade hattı — fiziksel kalemde teslim alma (9.9.27), ardından geri ödeme (9.9.21); dijital ve hizmet kaleminde doğrudan geri ödeme —, tutarı `02 §7.1.7` ile (K-808); bedel indiriminde tutar bazlı geri ödeme (9.9.22); havale hattında önce IBAN isteği (9.9.23). Ayıpta geri ödemenin süre sayacı yoktur (`02 §12.5`; K-716).
  - **9.9.31 Düzeltmeler** — sipariş düzeyindeki müdahaleler (`02 §10.4`; `03 §8.4`); hiçbiri onay istemez: **adres** — teslimat ve fatura adresi ve teslimat telefonu, OB-13'ün panelde düzeltme varyantıyla; formun başında K-798'in teyit hatırlatması (K-601, K-735); sonuç B-10 · **kargo şirketi ve takip numarası** — sonuç B-12 · **teslim tarihi** — OB-16 ve sıra sorusu (K-608); bildirim gitmez; ulaşma tarihi teslim alma kaydının satırında aynı kalıpla düzeltilir (K-712) · **durum geçişi** — form düzeltilecek geçişi ve dönülecek durumu gösterir, sebep kapalı listeden seçilir (OB-15, durum düzeltmesi; "diğer"de açıklama); yapılamayan düzeltme engel mesajıyla söylenir (K-566, K-703; K-799); sonuç B-13 — havale işaretinin düzeltilmesinde B-3 de (`03 §8.4.5`; K-570) · **misafir siparişinin e-postası** — yalnız girişsiz verilmiş siparişte ve kişisel veriler imha edilene kadar; formun başında K-798'in kimlik teyidi hatırlatması; sonuç yeni erişim anahtarı, yeni adrese B-1, eski adrese B-15, "e-posta düzeltildi" işareti ve e-posta geçmişinde iki adres (`03 §8.4.9`; K-585, K-586).
  - **9.9.32 İç not** (`03 §8.4.6`, §1.11.27): kaydetme onay istemez; notun yazılması ve değiştirilmesi işlem izine yazılır, metni yazılmaz.
  - **9.9.33 E-postayı yeniden göndermek** (`03 §8.9.2`, §1.11.36): işaretin altındaki satırdan; onay istemez (2.4.6; K-759). Gönderim sürerken düğme ikinci kez basılamaz; başarılı gönderimde satır düşer ve son satırla işaret kalkar; gönderim yine başarısızsa satır kalır ve K-799'un mesajı çıkar (2.5.4). Sipariş e-postası (B-1) donmuş sürümüyle gider. Siparişler arası toplu yeniden gönderim yoktur (K-761).
  - **9.9.34 Eşzamanlı değişiklik** (OB-10, sipariş varyantı; `02 §3.31.2`; K-587): yöneticinin gördüğü hâlden sonra sipariş değiştiyse — başka bir yöneticinin işlemi, müşterinin iptali, caymayı ya da IBAN girişi veya düzeltmesi, sistemin kendiliğinden geçişi — işlem uygulanmaz; K-799'un mesajı çıkar ve güncel hâl gösterilir. Onay penceresi açıkken değişen siparişte pencere kapanır (2.4.4). Üzerine yazma yoktur.
  - **9.9.35 Manuel adım bütçesi** (`02 §10.5`; K-404): ekran sipariş başına yeni bir zorunlu elle adım eklemez. Birincil düğme `02 §10.5.1`'in zorunlu adımlarıdır; onay penceresi, tarih ve adet alanları ve takip bilgisi var olan adımların parçasıdır; teslim alma ile stoğa ekleme `02 §10.5.2`'nin tek koşullu adımıdır; müdahaleler, iade reddi, kart iadesinin yeniden denenmesi, havale yolunun açılması, IBAN isteği ve e-postanın yeniden gönderilmesi bütçeye girmez (`03 §8.5`; K-794). Hat başına sayılar değişmez.
- **9.9.36 Validasyonlar.** Kargo şirketi ve takip numarası ya da araç beyanı olmadan kargoya verme formu gönderilmez (K-799) · tarihler OB-16'nın sınırlarındadır — teslim tarihi ileri ya da kargoya verildiği günden önce olamaz, ulaşma tarihi ileri ya da teslim tarihinden önce olamaz, bildirimin tarihi ileri ve kabul süresinin dışında olamaz (K-554; `03 §8.4.8`) · adet 2.12.1.8'in sınırlarındadır · sebep seçilmeden firma iptalinin ve durum düzeltmesinin onayı açılmaz; "diğer"de açıklama zorunludur (2.12.4.1) · aktarılan IBAN biçim ve sağlama basamağıyla denetlenir (OB-14) · tutar bazlı geri ödemenin tutarı ödenmiş ve geri ödenmemiş tutarı aşamaz · adres `02 §3.14.6`'nın alanlarıyla denetlenir (OB-13). Ret alan mesajıdır. Alan envanteri §7.2'dedir (5. oturum).
- **Durum × rol varyantları** (tam matris §6.3'te — 5. oturum)
  - **9.9.37** Bütün işlemler iki eksenin hâline ve kalemin kayıtlarına göre açılır (`03 §1.11.15`–§1.11.41); ekran kendi başına durum türetmez. Tipe göre: fiziksel kalemsiz siparişte teslimat bölgesi, kargoya verme ve teslim işareti yoktur (`03 §1.5`); dijital kalemde indirme sayacı ve hak yenileme vardır, iptal ve çıkarma sunulmaz — kalem ödeme onayında teslim edilir (`03 §1.11.4`, §1.11.22); hizmet kaleminde tamamlama işareti ve ifa süresi vardır. Hatta göre: havale hattında "ödendi" işareti, IBAN satırları ve "Havale gerçekleşmedi"; kart hattında kart iadesinin sonucu ve yeniden deneme.
  - **9.9.38** Ödenmemiş siparişte kalem düzeyinde iptal ve çıkarma yoktur (`02 §10.1.2`; K-505). Kendiliğinden iptal edilmiş ödenmemiş sipariş salt okunurdur; iç not ve — işaret varsa — e-postanın yeniden gönderilmesi açık kalır; durum düzeltmesi firmanın yanlış iptali içindir (`03 §1.6.2.2`). İptal edildi ya da Teslim edildi siparişte açık kalan işlemler kaydın kendisidir: iç not, ayıp talebi, iade hattı, geri ödemeler ve durum düzeltmesi. Satış kapısının kapanması açık siparişi etkilemez; bütün işlemler sürer (`03 §1.10.8`, §8.7.5.1).
  - **9.9.39** Panel tek roldür; her yönetici her işlemi yapar ve siparişin bütün verisini görür (`02 §10.2.3`).
- **Boş / yükleniyor / hata durumları** (OB-07)
  - **9.9.40** Ekranın boş hâli yoktur; boş bölüm — işaretler, ayıp talepleri, e-posta geçmişinde düzeltme satırları — görünmez (§2.7.1.1). Bölümler kendi yerlerinde yüklenir (§2.7.2); işlem sürerken düğmesi ikinci kez basılamaz ve sürdüğünü metinle söyler.
  - **9.9.41** Onay isteyen bir işlem beklenmeyen bir sebeple sonuçsuz kalırsa ekran "işlem yapılmadı" demez; siparişin güncel hâlini yeniden yükler ve sonucu orada gösterir (§2.7.3.3). Onay istemeyen işlemde mesaj işlemin yerindedir ve form korunur (§2.7.3.1). Sayfa açılamazsa §2.7.3.2. E-postanın ulaşmaması hiçbir işlemi durdurmaz (`03 §3.1.1`).
- **9.9.42 Responsive notları.** Geniş sınıfta iki sütundur: solda başlık, işaretler, sıradaki adım ve işlem grupları, kalemler ve ayıp talepleri; sağda tutar ve ödeme kaydı, teslimat, müşteri ve adresler, iç not, e-posta geçmişi ve belgeler (K-803). Orta sınıfta tek sütundur ve aynı sırayı izler. Dar sınıfta tek sütun 9.9.3–9.9.13'ün sırasıyladır; işlem grupları açılır bölümlerdir — işlem gizlenmez, katlanır (§2.1.2); kalem satırı görsel ile adı bir satıra, kayıtları, işaretleri ve işlemleri altına alır; onay penceresi tam ekran açılır. Sayaçtan siparişin birincil düğmesine en çok üç adımdır (2.1.2; K-741). İki rozet hiçbir sınıfta birleşmez.

*Kaynak: `02 §3.12`, §3.20.3, §3.20.7, §3.20.8, §3.21.5, §3.21.7, §3.21.11, §3.23.1–§3.23.3, §3.24.6, §3.25.2, §3.31.2, §5.3–§5.9, §6.7, §7.1.7, §7.2, §7.3.4, §7.4, §7.5.3, §7.6, §8.3, §8.4, §9.1, §9.2, §10.1.2, §10.2.3, §10.3.4, §10.4, §10.5, §12.5 · `03 §1.4`, §1.5, §1.6, §1.7, §1.11.15–§1.11.41, §2.5–§2.10, §3.1, §3.2.1.13, §3.2.1.16, §3.2.2, §3.3, §3.5.1.14, §3.5.2, §3.5.4.14, §4.1, §4.2, §5.1–§5.3, §6.2, §6.3.1.2, §7.1, §7.2.8, §7.2.16, §7.3, §8.2–§8.5, §8.6.4.2, §8.9.2, §10.1.1, §10.1.2.6, §10.3.4, §10.3.5 · `10 §2` KP-8, KP-24, KP-28, KP-44, KP-46–KP-48, KP-51, KP-68, KP-76 · K-741, K-744, K-751, K-759, K-760, K-761, K-764, K-777, K-787, K-788, K-790, K-792–K-797, K-798, K-799, K-803, K-804, K-808 · devir: K-491 (iki modlu sayaç, teslim almada tarih), K-494 ("iade malı bekleniyor" görünümü), K-496 ("mal dönmedi" kapanışı), K-497, K-499 (kanuni faiz), K-510 (başka kanaldan kayıt), K-525, K-535, K-536, K-537 (kalem çıkarmanın sınırı ve onayı), K-554 (tarihlerin alt sınırı), K-566 (yetmeyen kalem), K-570 (durum düzeltmesi), K-575, K-577, K-581, K-585, K-586 (e-posta geçmişi), K-587 ("sipariş değişti"), K-589 (geç gelen ödeme), K-590 (yeniden gönderimin kalemleri), K-592 (kapanış), K-596, K-601 (teyit hatırlatması), K-604, K-606, K-607, K-608 (sıra sorusu), K-609 ("stokta bulunamadı" uyarısı), K-611, K-612 ("geri ödeme gerçekleşmedi" uyarısı), K-656 (ret), K-663, K-675, K-677 (yeniden gönderimin onayı), K-686, K-690, K-703 (düzeltmenin engeli), K-706, K-709 (pencere dışı uyarı), K-712, K-715 (kaydın onayı), K-717 (kapatma düğmesi); K-735 (adres düzeltmede teyit hatırlatması — ekrandaki iz).*

### 9.10 E-38 — Talepler

- **9.10.1 Aktör · giriş · çıkış.** Yönetici. Giriş: menünün Talepler'i (3.2.3) · ana sayfanın "açık talep" sayacı — açık taleplere süzülmüş hâliyle (3.4.2) · iletişim talebi ayrıntısından ve siparişin ayıp talebi bölümünden listeye dönüş — bırakılan sayfa ve süzgeçle (3.4.9). Çıkış: iletişim talebinin satırı → E-39 · ayıp talebinin satırı → E-37, siparişin ayıp talepleri bölümü (3.4.8).
- **9.10.2 İlk görülen:** talepler en yeni açılan önce, tür ve durum süzgeçleriyle; sayaçtan gelindiyse "Açık" süzgeci seçili ve adı listenin başında durur. Ekranın birincil düğmesi yoktur; satır talebi açar — talep panelden açılmaz, vitrinden gelir (K-809).
- **Bilgi hiyerarşisi**
  - **9.10.3 (1) Süzgeçler** (K-751, K-809): **tür** — İletişim talebi · Ayıp talebi · **durum** — Açık · Kapatıldı · Çözüldü, birden çok seçilebilir; "Kapatıldı" yalnız iletişim talebine, "Çözüldü" yalnız ayıp talebine uyar (`02 §5.13`, §5.10). İki süzgeç birlikte uygulanır.
  - **9.10.4 (2) Tablo** (OB-20; K-809) — iki türün satırı aynı sütunlardadır: tür · talebin özü — iletişim talebinde konu tipi (`02 §3.32.3`) ile gönderenin adı ve e-postası; ayıp talebinde kalemin adı, ayıplı adet ve sipariş numarası (K-751, K-793) · açılış tarihi — yeniden açılmış ayıp talebinde son açılış tarihi de · durum rozeti — iletişim talebinde Açık ya da Kapatıldı, ayıp talebinde Açık ya da Çözüldü (2.5.1.5, 2.5.1.6) · ayıp talebinde, müşteriye giden bildirim (B-9) ulaşmamışsa "e-posta ulaşmadı" işareti (2.5.3.1; `03 §1.7.3.1`). İletişim talebinin satırı işaret taşımaz (2.5.4).
  - **9.10.5 (3) Sayfa gezinmesi** — sayfa başına 25 talep (§2.13.3).
  - **9.10.6 Sıra** (K-809): açılış tarihine göre en yeni önce; yeniden açılan ayıp talebi son açılış tarihiyle sıralanır. Sıralama seçeneği sunulmaz.
  - **9.10.7 Yoktur:** arama ve konu tipi süzgeci (K-737; 2.13.2) · panelden yanıt yazma, yazışma dizisi ve hazır şablon (`02 §3.32.5`) · ayrı bir şikâyet listesi (`02 §3.32.9`) · ayıp talebinin ayrı ayrıntı ekranı — ayrıntısı ve işlemleri siparişindedir (§4.3; 9.9.7, 9.9.30) · talebin listeden kapatılması ve toplu kapatma (2.13.6) · listeden dışa aktarma — iletişim taleplerinin dışa aktarması Raporlar'dadır (E-53; `02 §10.7.4`).
- **Aksiyonlar**
  - **9.10.8 Süzmek** — liste ilk sayfasına döner; seçili süzgeçlerin adı listenin başında durur ve tek dokunuşla kaldırılır (§2.13.3). Onay ve mesaj yoktur.
  - **9.10.9 Talebi açmak** — iletişim talebi → E-39; ayıp talebi → E-37'nin ayıp talepleri bölümü, talebin satırına odaklanmış (3.4.8; 9.9.7). Kapatma, çözüldü işareti ve yeniden açma açılan ekrandadır (9.11.8, 9.9.30); bu ekranın kendi işlemi yoktur.
- **9.10.10 Validasyonlar:** form yoktur.
- **9.10.11 Durum × rol varyantları.** "Açık" süzgeciyle liste ana sayfanın "açık talep" sayacının kapsamıdır ve satır sayısı sayaçtaki sayıya eşittir (`02 §10.6.1`; K-751). Talep saklama süresinin sonunda listeden düşer — kapatılmamış açık talep de (`03 §4.1.37`; Z-33). Satış kapısı listeyi etkilemez; aydınlatma metni tamamlanmamışken iletişim formu kapalı olduğu için yeni iletişim talebi gelmez (§2.8.3). Tam matris §6.3'tedir (5. oturum).
- **9.10.12 Boş / yükleniyor / hata durumları.** Hiç talep yoksa boş hâl satırı neyin olmadığını söyler; ilk kaydı açan düğme yoktur — talepler vitrinden gelir (§2.7.1.2). Süzgecin sonucu boşsa satır süzgeci kaldırma yolunu taşır; sıfır sayaçtan gelinen liste boş hâl satırıyla açılır (3.4.2). Tablo kendi yerinde yüklenir (§2.7.2); sayfa açılamazsa §2.7.3.2.
- **9.10.13 Responsive notları.** Orta sınıfta talebin özünün ikinci satırı — gönderenin e-postası ya da sipariş numarası — satırın altına iner. Dar sınıfta kart listesidir; kart türü, talebin özünü, durum rozetini ve işareti taşır, birincil işlemi talebi açmaktır (§2.13.4; K-741). Süzgeçler dar sınıfta listenin üstünde açılır bölümdedir.

*Kaynak: `02 §3.32.3`–§3.32.6, §3.32.9, §5.10, §5.13, §9.1.6, §10.1.2, §10.6.1, §10.7.4 · `03 §1.7.1.10`, §1.7.3.1, §1.11.37, §2.9.2, §2.9.5, §3.3.15, §4.1.37, §7.1.39, §7.1.42, §7.1.43, §7.3.29, §7.3.37, §8.4.10, §8.4.11, §8.5.6, §8.6.4.1, §8.9.1, §8.9.6, §10.1.2.2 · `10 §2` KP-34, KP-49 · K-737, K-741, K-751, K-766, K-793, K-809 · devir: K-540 (ayıp talebinin yeniden açılması ve bildirimi).*

### 9.11 E-39 — İletişim talebi ayrıntısı

- **9.11.1 Aktör · giriş · çıkış.** Yönetici. Giriş: Talepler'in iletişim talebi satırı (3.4.8) · oturumsuz gelişte girişten sonra (3.5.14, §3.6.1). Çıkış: listeye dönüş — bırakılan sayfa ve süzgeçle (3.4.9) · "Siparişlerde ara" → E-36 ve "Üye kaydında ara" → E-40, ikisi de talebin e-postasıyla aranmış hâlde (3.4.21; K-810).
- **9.11.2 İlk görülen:** konu tipi, durum rozeti ve mesaj; birincil düğme Açık talepte "Talebi kapat", Kapatıldı talepte "Yeniden aç" (K-810).
- **Bilgi hiyerarşisi**
  - **9.11.3 (1) Başlık:** konu tipi — Genel soru · Sipariş hakkında · Ürün hakkında · KVKK talebi · Diğer (`02 §3.32.3`) · durum rozeti (2.5.1.6) · talebin ulaştığı tarih ve saat — KVKK talebinde başvurunun firmaya ulaştığı an budur (`03 §8.6.4.3`; Z-25) · talep Kapatıldı'daysa son kapatılış tarihi — saklama süresi ondan işler (`02 §3.32.4`; Z-33) · "Listeye dön".
  - **9.11.4 (2) Gönderen:** ad, e-posta ve — girildiyse — telefon (`02 §3.32.2`); e-posta seçilip kopyalanabilen metindir — cevabı firma kendi e-postasıyla verir (`02 §3.32.5`). Altında iki bağlantı: "Siparişlerde ara" · "Üye kaydında ara" (K-810).
  - **9.11.5 (3) Mesaj** — formdan gelen metnin tamamı, satır sonlarıyla; kısaltılmaz. Sipariş numarası ayrı bir alan değildir, mesajın içindedir (`02 §3.32.2`).
  - **9.11.6 (4) İşlem:** "Talebi kapat" ya da "Yeniden aç" (K-810).
  - **9.11.7 Yoktur:** yanıt yazma ekranı, yazışma dizisi ve hazır şablon — müşterinin yanıtı firmanın iletişim e-postasına düşer ve yeni talep açmaz (`02 §3.32.5`; `03 §8.6.4.2`) · dosya eki (`02 §3.32.2`) · KVKK talebi için ayrı modül, ayrı durum ve süre sayacı — talep iletişim talebinin iki durumunu kullanır (`02 §3.15.4`, §5.13) · "e-posta ulaşmadı" işareti — firmaya giden F-2 satıra işaret düşürmez (2.5.4) · talebin elle silinmesi — talep saklama süresinin sonunda kendiliğinden imha edilir (`03 §4.1.37`) · gönderene verilen numara ve durum sorgusu (`02 §3.32.6`).
- **Aksiyonlar**
  - **9.11.8 Talebi kapatmak ve yeniden açmak** (`03 §8.6.4.4`, §5.1.3): onay istemez; rozet değişir ve sonuç kısa süreli bildirimle söylenir (2.3.1.4). Müşteriye bildirim gitmez (`02 §3.32.6`). Kapatma saklama süresini son kapatılıştan başlatır; yeniden açılan talep yeniden kapatılana kadar Açık'tır ve "açık talep" sayacına döner (`02 §3.32.4`, §10.6.1). Başka bir yönetici durumu o arada değiştirmişse işlem tekrarlanmaz ve ekran güncel durumu gösterir (K-810).
  - **9.11.9 Siparişlerde ve üye kaydında aramak** (K-810): sipariş listesini (E-36) ve üye kaydı görünümünü (E-40) talebin e-postasıyla aranmış hâlde açar. Talep bir sipariş işlemi gerektiriyorsa — indirme hakkının yenilenmesi, adres düzeltmesi, başka kanaldan cayma kaydı, misafir siparişinin e-posta düzeltmesi, "müşteriyle anlaşıldı" iptali — yönetici işlemi siparişin ayrıntısında yapar ve işlemin kendi bildirimi gider (`03 §8.6.4.2`; §9.9); KVKK talebinde cevabın dayanağı bu iki görünümdür ve hesabın talep üzerine silinmesi E-40'tadır (`03 §8.6.4.3`, §9.4.5, §9.4.6). Siparişin iletişim e-postası talebin e-postasından farklıysa arama sonuç vermez; yönetici numarayı mesajdan okuyup sipariş listesinde arar (9.8.3). Kimliğin ve talebin siparişin sahibinden geldiğinin teyidi firmanın yükümlülüğüdür; ekran onu denetlemez (`02 §12.2.6`, §10.4.2).
- **9.11.10 Validasyonlar:** form yoktur.
- **9.11.11 Durum × rol varyantları.** Açık talepte "Talebi kapat", Kapatıldı talepte "Yeniden aç" görünür (`02 §5.13`). Konu tipi ekranın düzenini değiştirmez; "Sipariş hakkında" tipinin daha uzun saklanması ekranda ayrıca gösterilmez (`02 §3.32.4`; P-40). Satış kapısı ekranı etkilemez. Tam matris §6.3'tedir (5. oturum).
- **9.11.12 Boş / yükleniyor / hata durumları.** Ekranın boş hâli yoktur. Talep saklama süresinin sonunda imha edildiyse ekran talebin artık bulunmadığını söyler ve listeye götürür. Durum değişikliği sürerken düğme ikinci kez basılamaz (§2.7.2); beklenmeyen hata §2.7.3.1'dir; sayfa açılamazsa §2.7.3.2.
- **9.11.13 Responsive notları.** Geniş sınıfta başlık ile gönderen yan yana, mesaj altlarında tam genişliktedir; orta ve dar sınıfta bölgeler 9.11.3–9.11.6'nın sırasıyla alt alta gelir. Uzun mesaj satır kırar; yatay kaydırma yoktur.

*Kaynak: `02 §3.15.4`, §3.32.2–§3.32.6, §5.13, §10.1.2, §10.4.2, §10.6.1, §12.2.6 · `03 §1.9.2`, §2.2.9, §4.1.37, §5.1.3, §7.1.39, §7.3.37, §8.2.10, §8.6.4.1–§8.6.4.4, §9.4.5, §9.4.6 · `10 §2` KP-34, KP-49 · K-751, K-810.*

### 9.12 E-40 — Üye kaydı görünümü

- **9.12.1 Aktör · giriş · çıkış.** Yönetici. Giriş: menünün Üyeler'i (3.2.10) · iletişim talebi ayrıntısının "Üye kaydında ara"sı — arama talebin e-postasıyla yapılmış (3.4.21). Çıkış: bağlı siparişin satırı → E-37 (3.4.14) · çerçevenin geçişleri (§3.2).
- **9.12.2 İlk görülen:** e-posta arama alanı; birincil düğme "Ara". Arama bir hesap bulduysa hesabın e-postası, adı ve giriş yolları.
- **Bilgi hiyerarşisi** — arama ve tek hesabın salt okunur kaydı (`02 §3.15.5`; K-811):
  - **9.12.3 (1) Arama** — tek alan: e-posta adresi. Değer müşteri hesaplarında tam eşleşmeyle aranır, büyük-küçük harf ayrımı yoktur; yönetici hesapları aranmaz — iki hesap türü ayrıdır ve e-posta kendi türü içinde tekildir (`02 §10.2.5`; K-811). Arama tek hesap bulur ya da hiçbirini bulmaz.
  - **9.12.4 (2) Hesap** — e-posta · ad (`02 §3.13.21`) · e-posta adresi doğrulanmamış kayıtta bunu söyleyen satır metni — rozet değildir (2.5.1; K-811) · giriş yolları: e-posta ve şifre — şifre var ya da yok · Google — bağlı ya da bağlı değil (`02 §3.15.5`; 5.25.6'nın kalıbı).
  - **9.12.5 (3) Adres defteri** — kayıtlı adresler; her biri adresin adı, alıcının adı ve soyadı, il, ilçe, açık adres ve — girildiyse — telefonla (OB-13'ün defter kaydı alanları, salt okunur; `02 §3.14`). Adres yoksa bölüm bunu tek satırla söyler.
  - **9.12.6 (4) Bağlı siparişler** (OB-20; `03 §9.4.5`) — hesaba bağlı siparişler en yeni önce: sipariş numarası · sipariş tarihi · toplam · Sipariş durumu ve Ödeme durumu rozetleri (2.5.1.1, 2.5.1.2; K-777); sayfa başına 25 sipariş (§2.13.3). Satır E-37'yi açar (3.4.14). Sipariş yoksa bölüm bunu tek satırla söyler.
  - **9.12.7 (5) Hesabı sil** — en altta: silmenin sonucu — hesabın adı, giriş bilgileri, oturumları, adres defteri ve sepeti hemen silinir; siparişler sürer (`02 §3.15.1`, §3.15.2) — ve "Hesabı sil" (K-797). **Geri alma bağlantısı açıkken** düğme kapalıdır ve yerinde sebebi ile bağlantının bitiş tarihi durur (`02 §3.15.6`; `03 §3.4.15`; K-698; metni K-811).
  - **9.12.8 Yoktur:** kaydın düzenlenmesi — ad, e-posta ve şifre dahil; görünüm salt okunurdur (`02 §3.15.5`, §10.1.2) · üyelerin listesi ve adla arama — görünüm e-postayla aranan tek hesabı gösterir (`02 §3.15.5`) · uygulama içi veri indirme — cevabı firma hazırlar (`02 §3.15.5`) · görünümden dışa aktarma — üye listesinin dışa aktarması Raporlar'dadır (E-53; `02 §10.7.4`) · yönetici hesabının bu görünümde aranması (`02 §10.2.5`).
- **Aksiyonlar**
  - **9.12.9 Aramak** — onay istemez; hesap bulunamazsa sonucun yerinde K-811'in satırı durur: "Bu e-posta adresiyle kayıtlı üye hesabı yok." Arama işlem izine yazılmaz (`02 §10.3.1`).
  - **9.12.10 Hesabı silmek** (`03 §9.4.6`): onay penceresi (OB-04, geri alınamaz — 2.4.2.1) K-797'nin cümlesini — "Hesap ve hesaptaki ad, giriş bilgileri, adres defteri ve sepet hemen silinecek; siparişler sürer." — ve ardından "Bu işlem geri alınamaz."ı söyler; onay düğmesi "Hesabı sil"dir. Onayla hesap `02 §3.15.1`'in rejimiyle silinir (`03 §9.4.2`), hesabın adresine "hesabınız silindi" e-postası gider ve silme işlem izine yazılır — satır hesabı değil, işlemi ve tarihini adlandırır (`02 §10.3.1`). Ekran arama alanına döner ve yerinde kalıcı mesaj silmenin yapıldığını söyler (K-811); sonuç kısa süreli bildirimle verilmez (2.3.4). Sipariş başına elle adım değildir ve bütçeye girmez (`03 §8.5.7`).
  - **9.12.11 Bağlı siparişi açmak** → E-37 (3.4.14); satır onay ve mesaj taşımaz.
- **9.12.12 Validasyonlar.** E-posta biçimi denetlenir; ret alan mesajıdır (OB-12). Alan envanteri §7.2'dedir (5. oturum).
- **9.12.13 Durum × rol varyantları.** Arama sonucu üç hâllidir: hesap yok · doğrulanmamış kayıt · doğrulanmış hesap; giriş yollarına göre şifreli, şifresiz Google ya da ikisi birden. Geri alma bağlantısı açıkken (Z-45) silme kapalıdır — hesabın kendi silmesindeki engelin aynısıdır (5.25.7; K-698). Yürüyen sipariş silmeyi engellemez (`02 §3.15.2`). Satış kapısı ekranı etkilemez. Tam matris §6.3'tedir (5. oturum).
- **9.12.14 Boş / yükleniyor / hata durumları.** Arama yapılmadan ekran yalnız arama alanını gösterir; hesap bulunamazsa 9.12.9'un satırı durur — bir hata değildir (§2.7.1.2). Sonuç kendi yerinde yüklenir (§2.7.2). Silmenin onayı sonuçsuz kalırsa ekran "işlem yapılmadı" demez; aramayı yeniler ve sonucu gösterir — hesap duruyorsa kaydı, silindiyse bulunamadığını (§2.7.3.3). Sayfa açılamazsa §2.7.3.2.
- **9.12.15 Responsive notları.** Geniş sınıfta hesap ve giriş yolları solda, adres defteri sağda, bağlı siparişler altlarında tam genişliktedir. Orta ve dar sınıfta bölümler 9.12.3–9.12.7'nin sırasıyla alt alta gelir; bağlı siparişler dar sınıfta kart listesidir — kart numarayı, tarihi ve iki rozeti taşır (§2.13.4). Hesabı sil her sınıfta en alttadır.

*Kaynak: `02 §3.13.21`, §3.14, §3.15.1, §3.15.2, §3.15.5, §3.15.6, §5.9, §10.1.2, §10.2.5, §10.3.1, §10.7.4 · `03 §1.6.1.8`, §3.4.15, §8.5.7, §8.6.4.3, §8.9.6, §9.3.1, §9.4.2, §9.4.5, §9.4.6 · `10 §2` KP-29, KP-77 · K-777, K-797, K-811 · devir: K-512 (üye kaydı görünümü ve talep üzerine silme), K-698 (silme engelinin mesajı), K-715 (silmenin onayı).*

### 9.13 E-41 — Kurumsal içerik

- **9.13.1 Aktör · giriş · çıkış.** Yönetici. Giriş: menünün İçerik ve site › Kurumsal içerik'i (3.2.7) · önizlenen taslak kaydın "Taslak" bandındaki "Düzenlemeye dön" (3.7.4). Çıkış: kayıt satırı ya da yeni kayıt → kaydın formu (3.4.12) · formun "Önizle" ya da "Sitede gör"ü → E-08 ya da kaydın göründüğü yer — E-09, E-10, duyuru şeridi (3.4.13, 3.7.2).
- **9.13.2 İlk görülen:** kayıtların tipe göre gruplanmış listesi, yayın durumu rozetleriyle; birincil düğme yoktur — her grup kendi "Yeni" düğmesini taşır, tekil iki kayıt kendi satırından açılır (K-812). Formda birincil düğme "Kaydet"tir.
- **Bilgi hiyerarşisi** — iki görünüm: liste ve kaydın formu (`03 §8.6.1`).
  - **9.13.3 (1) Liste** (OB-20; K-812) — yedi grup, bu sırayla: **Hakkımızda** — tek satır · **Hizmetlerimiz** — hizmet tanıtımları · **Referanslarımız** — referans işler · **SSS** — sorular · **Şubeler** · **Genel sayfalar** · **Duyuru** — tek satır (`02 §3.27.2`, §3.27.3, §3.27.21). Grup adı ürünün tip adıdır; menü adı firmanın değiştirdiği addır ve satırın yanında okunur (`02 §3.28.4`; E-42). Satır: kaydın adı, başlığı ya da sorusu — Hakkımızda'da ve duyuruda tipin adı ve duyurunun metni (`02 §3.27.24`) · yayın durumu rozeti — Taslak ya da Yayında (2.5.1.4) · hizmet tanıtımında ve referans işte "ana sayfada göster" işareti, genel sayfada "menüde göster" işareti · elle sıranın "yukarı" ve "aşağı" düğmeleri — Hakkımızda ve duyuru dışındaki gruplarda. Duyuru satırı yayın durumunun yanında görünürlüğünü de söyler: tarih aralığı girilmişse aralık; Yayında olduğu hâlde aralığın dışındaysa sitede şu an görünmediği (`03 §1.10.4`; Z-23; K-812). Hizmetlerimiz ve Referanslarımız gruplarının başında işaretli kayıtların sayısı ve ana sayfada sıradaki ilk üçünün göründüğü yazar (K-755; metni K-812).
  - **9.13.4 (2) Kaydın formu** — bölümlü form (OB-12); geniş sınıfta alanlar solda, yayın durumu yan sütunda (9.5'in düzeni). Alanlar tipe göredir (`02 §3.27.3`–§3.27.9, §3.27.21); zorunlu olan alan yayın kapısıdır ve "yayın için zorunlu" diye etiketlenir (K-584):
    - **Hakkımızda:** kısa tanıtım — düz metin, P-29; yayın için zorunlu · uzun metin · görseller.
    - **Hizmet tanıtımı:** ad — P-27; yayın için zorunlu · kısa açıklama — listedeki kartta görünür, P-28 · metin · görseller · ilgili ürünler · "ana sayfada göster".
    - **Referans iş:** başlık — P-27; yayın için zorunlu · kısa açıklama — P-28 · metin · görseller · ilgili ürünler · "ana sayfada göster". Müşteri adı, yıl ve kategori alanı yoktur; müşteri adı başlıkta yazılır (`02 §3.27.5`).
    - **Sık sorulan soru:** soru — düz metin, P-28 · cevap; ikisi de yayın için zorunlu.
    - **Şube:** ad ve adres — il kapalı listeden; ikisi de yayın için zorunlu · telefon · çalışma saatleri — P-28 · görsel · harita bağlantısı (`02 §3.27.7`).
    - **Genel sayfa:** başlık — P-27; yayın için zorunlu · metin · görseller · "menüde göster".
    - **Duyuru:** metin — düz metin, P-30; yayın için zorunlu · isteğe bağlı bağlantı · isteğe bağlı başlangıç ve bitiş (OB-16, tarih aralığı; `02 §3.27.21`).
    Uzun metinler — Hakkımızda'nın uzun metni, hizmetin ve referansın metni, SSS cevabı, genel sayfanın metni — kapalı metin biçimi setini kullanır; video bağlantısı metnin bir bağlantısıdır (`02 §3.11.6`, §3.27.16, §3.27.17). Görseller OB-19'un galerisidir — elle sıra, ana görsel işareti, görsel başına isteğe bağlı alternatif metin; alan boşken sistemin üreteceği metni gösterir (`02 §3.27.15`; K-564). **İlgili ürünler:** katalogdan ürün adıyla aranarak seçilir; seçilen ürünler yayın durumu rozetleriyle listelenir ve Yayında olmayan ürünün yanında vitrinde görünmediği yazar (`02 §3.27.10`; `03 §3.5.3.4`). "Ana sayfada göster" işaretinin yanında 9.13.3'ün sayısı yinelenir (K-755).
  - **9.13.5 (3) Yayın durumu yan sütunu:** kaydın yayın durumu rozeti · yayın kapısının alanları — karşılanmayan adıyla (`02 §3.27.13`; K-584) · "Önizle" (taslak kayıt) ya da "Sitede gör" (yayındaki kayıt) · "Yayına al" ya da "Taslağa al" · "Kalıcı olarak sil" — Hakkımızda'da yoktur (`02 §3.27.3`) · kendi sayfası olan kayıtta — Hakkımızda, hizmet tanıtımı, referans iş, genel sayfa — sayfanın adresi; ilk yayında sabitlenir (`02 §3.30.2`).
  - **9.13.6 Yoktur:** sayfa kurucu ve düzen seçimi (`02 §3.27.1`) · galeri, blog ve müşteri görüşü tipi (`02 §3.27.2`, §3.27.19, §3.27.20) · ikinci Hakkımızda ve ikinci duyuru (`02 §3.27.3`, §3.27.21) · gömülü video ve dosya eki (`02 §3.27.17`, §3.27.18) · ileri tarihli yayın — tek istisna duyurunun tarih aralığıdır (`02 §3.27.28`) · sürüm geçmişi ve eski hâle dönme (`02 §3.27.26`) · arşiv durumu (`02 §5.2`) · listede arama ve süzgeç (K-737) · şubenin ve SSS sorusunun ayrı sayfası (`02 §3.27.6`, §3.27.7) · ayarlarla çelişen metnin denetimi — hatırlatma ilgili ayarın yanındadır (`02 §3.33.8`; 9.17, 9.18).
- **Aksiyonlar**
  - **9.13.7 Kaydı açmak ve kaydetmek** (`03 §8.6.1.1`, §8.6.1.2, §8.6.1.6): yeni kayıt ilk kayıtta Taslak doğar ve adla, başlıkla ya da — SSS'de — soruyla kaydedilir; Hakkımızda ve duyuru boş da kaydedilir (`02 §3.27.24`). Yayındaki kaydın düzenlemesi kaydedildiği anda yayındadır. Başarı kısa süreli bildirimle söylenir (2.3.1.4). Yayın kapısını bozan düzenleme kaydedilmez: K-812'nin mesajı boşaltılan alanı adıyla söyler ve önce kaydı taslağa almayı önerir (`03 §3.5.3.11`; 2.3.3). Metin düzenlemesi işlem izine yazılmaz (`02 §3.27.26`). Onay istemez.
  - **9.13.8 Yayına almak ve taslağa almak** (`03 §8.6.1.4`, §8.6.1.7): kapı eksikse kayıt yayına alınmaz ve eksik alanlar yan sütunda adıyla, K-812'nin mesajıyla gösterilir (`03 §3.5.3.1`; K-584). İkinci bir onay yoktur; her yönetici yayımlar (`02 §3.27.23`). Taslağa alınan kayıt menüden, ana sayfadan ve listelerden kalkar. Yayın durumu değişikliği işlem izine yazılır. Onay istemez.
  - **9.13.9 Önizlemek** (`03 §8.6.1.3`): kendi sayfası olan kayıt E-08'de "Taslak" bandıyla açılır (§2.9.2); SSS sorusu, şube ve duyuru göründükleri yerde "Taslak" etiketiyle önizlenir — E-09, E-10, duyuru şeridi (§2.9.3). Önizleme kaydedilmiş hâli gösterir.
  - **9.13.10 Kalıcı olarak silmek** (`03 §8.6.1.7`): onay penceresi (OB-04, geri alınamaz — 2.4.2.2) K-812'nin cümlesini ve "Bu işlem geri alınamaz."ı söyler; onay düğmesi "Kalıcı olarak sil"dir (K-797). Silinen kaydın sayfası "sayfa bulunamadı" döner; ana sayfa işareti, menüdeki yeri ve ürün bağları kalkar, adresi serbest kalır (`02 §3.27.27`, §3.30.2). Hakkımızda'da düğme yoktur; silme isteği yerine taslağa alma sunulur (`03 §3.5.3.2`).
  - **9.13.11 Elle sıralamak ve işaretlemek** (`03 §8.6.1.5`): listenin "yukarı" ve "aşağı" düğmeleri kaydı grubunda bir sıra kaydırır; sıra anında kaydedilir ve vitrinin liste sayfaları, ana sayfa blokları ve menü bu sırayı izler (`02 §3.27.11`). İşlem klavyeyle yapılır, sürükle-bırak gerekmez (§2.1.4; K-812). "Ana sayfada göster" ve "menüde göster" işaretleri kaydın formunda değişir ve sıradan bağımsızdır (`02 §3.28.3`, §3.28.5). Onay istemez.
  - **9.13.12 Eşzamanlı düzenleme** (OB-10, kayıt varyantı; `02 §3.31.1`; `03 §3.5.3.6`): ikinci kaydeden kaydın o arada değiştiğini görür; iki yol — "Güncel hâli aç" · "Yine de kaydet" — K-800'ün metniyle.
- **9.13.13 Validasyonlar.** Ad, başlık, soru, kısa açıklama, kısa tanıtım, çalışma saatleri ve duyuru metni karakter tavanlarındadır (P-27…P-30) · görseller P-22, P-23 ve P-25'in sınırlarındadır (2.12.8.3) · şubenin ili kapalı listedendir · duyurunun bitişi başlangıcından önce olamaz (OB-16) · bağlantı ve harita bağlantısı adres biçimiyle denetlenir. Taslak kaydı adsız bırakmak — Hakkımızda ve duyuru dışında — kaydı engeller ve alan mesajıyla söylenir (`02 §3.27.24`). Ret alan mesajıdır; yayın kapısının mesajı yerinde kalıcıdır (K-812). Alan envanteri §7.2'dedir (5. oturum).
- **Durum × rol varyantları** (tam matris §6.3'te — 5. oturum)
  - **9.13.14** Her kayıt kendi yayın durumunu taşır: Taslak'ta "Önizle" ve "Yayına al", Yayında'da "Sitede gör" ve "Taslağa al" (`02 §5.2`). Duyuru Yayında olduğu hâlde tarih aralığının dışındaysa sitede görünmez ve satırı bunu söyler (`03 §1.10.4`). Hakkımızda boşken ya da yayında değilken ana sayfanın kurumsal bloğu marka adını ve logoyu gösterir (`03 §3.5.3.3`); liste satırı bunu söylemez — vitrinin kuralıdır.
  - **9.13.15 İlk kurulumda** site boştur (`02 §3.27.22`, §10.8.1): Hakkımızda ve duyuru satırları kaydın henüz olmadığını söyler ve boş formu açar; öteki gruplar boş hâl satırıyla ilk kaydı açtırır. Kurumsal taraf satış kapısına bağlı değildir: satış kapalıyken de kayıtlar yayına alınır ve sitede görünür (`03 §8.6`).
- **9.13.16 Boş / yükleniyor / hata durumları.** Boş grup boş hâl satırıyla grubun "Yeni" düğmesini taşır (§2.7.1.2); kaydı olmayan tipin vitrinde görünmemesi vitrinin kuralıdır (§2.7.1.1). Görsel yüklemesi kendi yerinde ilerler ve sürerken form kaydedilemez (2.12.8.4); kayıt beklenmeyen bir sebeple tamamlanamazsa form korunur (§2.7.3.1); silme onayı sonuçsuz kalırsa liste yenilenir ve sonucu gösterir (§2.7.3.3); sayfa açılamazsa §2.7.3.2.
- **9.13.17 Responsive notları.** Dar ve orta sınıfta form tek sütundur; yan sütunun yayın durumu ve kapı alanları formun başına gelir. Liste dar sınıfta kart listesidir — kart kaydın adını, yayın durumu rozetini ve işaretini taşır; "yukarı" ve "aşağı" kartın kendi menüsündedir (§2.13.4). Hiçbir alan bir sınıfta gizlenmez.

*Kaynak: `02 §3.11.4`, §3.11.6, §3.27.1–§3.27.28, §3.28.3–§3.28.5, §3.30.2, §3.31.1, §5.2, §6.8, §7.6.4, §10.1.2, §10.3.1 · `03 §1.8.4`, §1.10.4, §3.5.3.1–§3.5.3.4, §3.5.3.6, §3.5.3.11, §4.1.26, §4.1.27, §6.3.3.2, §7.2.28, §8.6.1, §8.6.3, §10.1.1.6, §10.2.2, §10.3.6 · `10 §2` KP-54, KP-57, KP-76 · K-737, K-748, K-755, K-797, K-800, K-812 · devir: K-538 (yayın kapısını bozan düzenlemenin engeli), K-564 (üretilen alternatif metin), K-584 (Hakkımızda ve duyuru formunun eksik alan gösterimi).*

### 9.14 E-42 — Ana sayfa ve menü

- **9.14.1 Aktör · giriş · çıkış.** Yönetici. Giriş: menünün İçerik ve site › Ana sayfa ve menü'sü (3.2.8). Çıkış: çerçevenin geçişleri (§3.2); sonucu görmek için "Siteyi görüntüle" (3.2.20).
- **9.14.2 İlk görülen:** seçili ana sayfa düzeni; birincil düğme "Kaydet".
- **Bilgi hiyerarşisi** — tek form, üç bölüm, tek "Kaydet" (OB-12; K-813):
  - **9.14.3 (1) Ana sayfa düzeni** — iki seçenek: **tanıtım öncelikli** · **mağaza öncelikli** (`02 §3.28.1`). Her seçeneğin altında bloklarının sırası yazılıdır — tanıtım öncelikli: kurumsal blok geniş hâliyle, Hizmetlerimiz, Referanslarımız, ürün vitrini tek satır; mağaza öncelikli: ürün vitrini iki satır, kurumsal blok dar hâliyle, Hizmetlerimiz, Referanslarımız (K-753; §5.1) — ve iki düzende de kurumsal tanıtımın ve ürün vitrininin birlikte durduğu söylenir. Kurulumda tanıtım öncelikli seçili gelir (K-813; `02 §3.28.1`).
  - **9.14.4 (2) Menü adları** — sabit iskeletin öğeleri, seçili düzenin menü sırasıyla (2.2.3): Ürünler · Hizmetlerimiz · Referanslarımız · Hakkımızda · SSS · İletişim. Her öğede ad alanı — varsayılan ad ürünle gelir ve alanın altında yazar — ve öğe şu an menüde görünmüyorsa sebebi: yayında kaydı yok · yayında ürünü olan kategori yok (`02 §3.28.4`, §3.28.6; K-813). Ad ana sayfa bloğunun başlığında da kullanılır (K-753). "Menüde göster" işaretli genel sayfalar İletişim'den önce, salt okunur listelenir; işaret genel sayfanın formundadır (`02 §3.28.5`; 9.13.4).
  - **9.14.5 (3) Sosyal bağlantılar** — kapalı platform listesinin her biri için isteğe bağlı bağlantı alanı: Instagram · Facebook · X · LinkedIn · YouTube · TikTok · ve isteğe bağlı WhatsApp numarası (`02 §3.27.12`). Bölüm, bağlantıların sitenin üst bölümünün ince satırında ve altbilgide göründüğünü söyler (§2.2.3).
  - **9.14.6 Yoktur:** serbest sayfa kurgusu ve blok ekleme (`02 §3.28.1`) · menü düzenleyici — öğe ekleme, çıkarma ve sırayı değiştirme (`02 §3.28.4`) · ana sayfa için ürün seçme ve ürüne "ana sayfada göster" işareti (`02 §3.28.1`) · kayan afiş ve kampanya görseli (`02 §3.27.21`) · gömülü gönderi akışı ve sohbet penceresi (`02 §3.27.12`) · listenin dışında bir platform (`02 §3.27.12`).
- **Aksiyonlar**
  - **9.14.7 Kaydetmek** (`03 §8.6.2.1`–§8.6.2.3): onay istemez; başarı kısa süreli bildirimle söylenir (2.3.1.4) ve değişiklik vitrinde hemen geçerlidir. Değişen her ayar eski ve yeni değerle işlem izine yazılır (`02 §10.3.1`). Sosyal bağlantılar siparişe donmaz (`02 §3.27.12`).
  - **9.14.8 Eşzamanlı düzenleme** (OB-10, kayıt varyantı; `02 §3.31.1`; K-800).
- **9.14.9 Validasyonlar.** Menü adı boş kaydedilmez; alan mesajı varsayılan adı hatırlatır (K-813) · bağlantılar adres biçimiyle, WhatsApp numarası telefon numarası biçimiyle denetlenir. Ret alan mesajıdır (OB-12). Alan envanteri §7.2'dedir (5. oturum).
- **9.14.10 Durum × rol varyantları.** Menü öğesinin görünürlüğü içerikten ve katalogdan okunur ve öğenin satırında yazar (9.14.4); düzen seçimi ana sayfanın ve menünün sırasını değiştirir (K-746, K-753). Satış kapısı ekranı etkilemez — ana sayfanın ürün vitrini satış kapalıyken de görünür (K-754). Tam matris §6.3'tedir (5. oturum).
- **9.14.11 Boş / yükleniyor / hata durumları.** Ekranın boş hâli yoktur; ilk kurulumda düzen seçili, menü adları varsayılan, sosyal bağlantılar boş gelir ve menünün bütün öğeleri görünmüyor satırını taşır — İletişim dışında. Kayıt beklenmeyen bir sebeple tamamlanamazsa form korunur (§2.7.3.1); sayfa açılamazsa §2.7.3.2.
- **9.14.12 Responsive notları.** Dar ve orta sınıfta bölümler alt alta, alanlar tek sütundur; düzen seçenekleri dar sınıfta alt alta durur ve blok sıraları metin olarak okunur. Hiçbir alan bir sınıfta gizlenmez.

*Kaynak: `02 §3.27.12`, §3.27.21, §3.28.1, §3.28.4–§3.28.6, §3.31.1, §10.1.2, §10.3.1 · `03 §8.6.1.5`, §8.6.2.1–§8.6.2.3 · `10 §2` KP-55 · park satırı K-27 · K-746, K-753, K-754, K-800, K-813.*

### 9.15 E-43 — Marka

- **9.15.1 Aktör · giriş · çıkış.** Yönetici. Giriş: menünün İçerik ve site › Marka'sı (3.2.9). Çıkış: çerçevenin geçişleri (§3.2); sonucu görmek için "Siteyi görüntüle" (3.2.20).
- **9.15.2 İlk görülen:** marka adı ve logo; birincil düğme "Kaydet".
- **Bilgi hiyerarşisi** — tek form, beş bölüm, tek "Kaydet" (OB-12; K-813); başında marka ayarlarının vitrine ve e-postalara uygulandığı ve siparişe donmadığı yazar (`02 §3.29.1`; `03 §8.6.2.4`):
  - **9.15.3 (1) Marka adı** — zorunlu; bir kez girildikten sonra boşaltılamaz. Girilene kadar alanın altında sitenin alan adını gösterdiği yazar (`02 §3.29.3`; `03 §3.5.4.6`). Ad sitenin üst bölümünde — logo yoksa —, tarayıcı sekmesinde ve e-postalarda görünür; unvan değildir — unvan firma kimliğindedir (E-44).
  - **9.15.4 (2) Logo** — isteğe bağlı tek görsel (OB-19, tek görsel); yüklenmemişse marka adının yazıyla gösterildiği söylenir (`02 §3.29.2`; `03 §3.5.4.7`).
  - **9.15.5 (3) Site simgesi** — isteğe bağlı tek görsel (OB-19); yüklenmemişse sistemin göstereceği simge — marka adının baş harfi, marka rengi zemin üzerinde — örnek olarak görünür (`02 §3.29.4`; `03 §3.5.4.8`).
  - **9.15.6 (4) Marka rengi** — tek renk seçimi; boş bırakılabilir — boşken ürünün varsayılan rengi uygulanır (`02 §3.29.5`; `03 §3.5.4.9`; K-813). Seçimin hemen yanında **renk örneği**: bir birincil düğme ve bir duyuru şeridi, sistemin seçtiği siyah ya da beyaz yazıyla (K-742). Bölüm rengin vitrinde yalnız dolu zemin olarak kullanıldığını ve yerlerini bir cümleyle söyler (2.1.3).
  - **9.15.7 (5) Platform imzası** — "Shopfolio ile kuruldu" açık ya da kapalı; varsayılan açıktır ve imza yalnız sitenin altbilgisindedir (`02 §3.29.6`).
  - **9.15.8 Yoktur:** yazı tipi, sayfa düzeni ve marka rengi dışındaki renklerin seçimi; koyu tema (`02 §3.29.1`) · ikinci renk ve ayrı e-posta logosu (`02 §3.29.2`) · siteye kod ekleme — analitik, reklam etiketi, piksel (`02 §8.3.2`, §10.1.2) · panelin marka rengiyle boyanması — panel ürünün kendi görünümündedir (`02 §3.29.1`).
- **Aksiyonlar**
  - **9.15.9 Kaydetmek** (`03 §8.6.2.4`): onay istemez; başarı kısa süreli bildirimle söylenir (2.3.1.4) ve marka vitrinde hemen geçerlidir. Marka ayarları ve platform imzası eski ve yeni değerle işlem izine yazılır (`02 §10.3.1`).
  - **9.15.10 Görsel yüklemek ve kaldırmak** — OB-19'un tek görsel varyantı; yükleme ilerlemeyi gösterir ve sürerken form kaydedilemez (2.12.8.4). Kaldırılan logo ve simgenin yerine 9.15.4 ve 9.15.5'in geri düşüşü geçer.
  - **9.15.11 Eşzamanlı düzenleme** (OB-10, kayıt varyantı; `02 §3.31.1`; K-800).
- **9.15.12 Validasyonlar.** Marka adı girildikten sonra boşaltılamaz — alan mesajı (`02 §3.29.3`) · logo ve site simgesi P-22 ve P-23'ün sınırlarındadır (2.12.8.3). Ret alan mesajıdır (OB-12). Alan envanteri §7.2'dedir (5. oturum).
- **9.15.13 Durum × rol varyantları.** Logo, simge ve renk girilmemişken her bölüm geri düşüşünü gösterir (9.15.3–9.15.6). Satış kapısı ekranı etkilemez; marka ayarları kurulum kontrol listesinde değildir (`02 §10.8.2`). Tam matris §6.3'tedir (5. oturum).
- **9.15.14 Boş / yükleniyor / hata durumları.** İlk kurulumda alanlar boş, imza açık gelir; ekran her bölümün geri düşüşünü gösterir. Kayıt beklenmeyen bir sebeple tamamlanamazsa form korunur (§2.7.3.1); sayfa açılamazsa §2.7.3.2.
- **9.15.15 Responsive notları.** Geniş sınıfta alanlar solda, renk örneği ve simge örneği sağda; dar ve orta sınıfta örnek ilgili alanın hemen altına iner. Hiçbir alan bir sınıfta gizlenmez.

*Kaynak: `02 §3.23.5`, §3.29.1–§3.29.6, §3.31.1, §6.9.6–§6.9.9, §8.3.2, §10.1.2, §10.3.1, §10.8.2 · `03 §3.5.4.6`–§3.5.4.9, §8.6.2.4 · `10 §2` KP-56 · K-742, K-800, K-813 · devir: K-541 (platform imzası), K-742 (marka renginin örneği).*

### 9.16 E-44 — Firma kimliği ve satış

- **9.16.1 Aktör · giriş · çıkış.** Yönetici. Giriş: menünün Ayarlar › Firma kimliği ve satış girişi (3.2.14) · üst satırın satış durumu göstergesi — "Satış durumu" bölümüne (3.2.19) · ana sayfanın uyarıları — satış kapalıyken ve "firma bildirimleri ulaşmıyor"da (3.4.3) · kurulum kontrol listesinin kimlik maddesi (3.4.4). Çıkış: "Satış durumu" bölümünde karşılanmayan koşulun bağlantısı → bu ekranın kimlik formu, E-45, E-46 ya da E-47 (3.4.15) · çerçevenin geçişleri (§3.2).
- **9.16.2 İlk görülen:** "Satış durumu" bölümü — satışın açık mı kapalı mı olduğu ve sebebi; birincil düğme kimlik formunun "Kaydet"idir. Geçici kapatma anahtarı kendi onayıyla işler ve formun kaydından bağımsızdır (K-752).
- **Bilgi hiyerarşisi** — iki bölüm (K-752):
  - **9.16.3 (1) Satış durumu** — en üstte (OB-08, 2.8.2; K-815): durum — "Satış açık" ya da "Satış kapalı" · kapının koşulları tek tek, her biri karşılanıyor ya da karşılanmıyor diye ve karşılanmayanı adıyla — firma kimliğinin zorunlu alanları, KEP adresi dahil · en az bir açık ödeme yöntemi — havale açıksa IBAN · aydınlatma metninin tamamlanıp yayına alınması ve çerez politikasının yayına alınması · iade adresi · geçici kapatma anahtarının kapalı olması (`02 §3.1.5`). Karşılanmayan koşul düzeltileceği ekranın bağlantısını taşır (3.4.15). Aydınlatma metninin satırı, metin tamamlanıp yayına alınana kadar iletişim formunun, hesap kaydının ve yeni hesap açacak Google ile girişin de kapalı olduğunu söyler (§2.8.3). Altında **geçici kapatma anahtarı:** "Satışı geçici olarak kapat" — açıkken "Satışı yeniden aç" (`02 §3.1.6`; `03 §8.7.5`).
  - **9.16.4 (2) Firma kimliği** — bölümlü form (OB-12; K-815); başında kimliğin her siparişe o günkü hâliyle yazıldığını ve değişikliğin Ön Bilgilendirme Formu ile sözleşmenin yeni sürümünü ürettiğini söyleyen satır (`02 §3.23.3`; `03 §8.7.2.1`, §8.7.2.3):
    - **Firma tipi** — kapalı liste: Esnaf · Gerçek kişi tacir · Tüzel kişi (`02 §3.1.3`). Tip seçilmeden tipin alanları açılmaz; tipi değiştirirken alanın altında yeni tipin zorunlu alanları dolana kadar satışın kapalı kalacağı yazar (`03 §3.5.4.1`; K-815).
    - **Tipin zorunlu alanları** — esnafta ad-soyad ve vergi kimlik numarası; gerçek kişi tacirde adını ve soyadını taşıyan ticaret unvanı, MERSİS numarası ve ticaret sicil numarası; tüzel kişide ticaret unvanı, MERSİS numarası ve ticaret sicil numarası (`02 §3.1.3`).
    - **Üç tipte zorunlu alanlar** — adres: il kapalı listeden, ilçe, açık adres · telefon · e-posta — firmaya giden bildirimlerin adresidir ve alanın altında bunu söyler (`02 §9.3`) · KEP adresi — ürün adresin geçerliliğini doğrulamaz (`02 §3.1.3`; K-627).
    - **İsteğe bağlı alanlar** — meslek odası · işletme adı ya da tescilli marka · meslekle ilgili davranış kuralları — bağlantı ya da kısa metin (`02 §3.1.3`; K-547, K-627). Doluysa yasal bilgilerle birlikte görünür; satış kapısını tutmaz.
    - **ETBİS doğrulama bilgisi** — isteğe bağlı; ETBİS'in verdiği doğrulama bağlantısı. Alanın yanında yükümlülüğün hatırlatması durur — metni K-815'tedir: ETBİS'e kaydın satış yapan her firmanın yükümlülüğü olduğu, ürünün kaydı denetlemediği ve alan boşken sitede doğrulama bandının görünmediği (`02 §3.1.4`, §12.5; `03 §8.7.2.1`; K-574, K-628).
    "Firma bildirimleri ulaşmıyor" uyarısı sürerken e-posta alanının yanında K-799'un metni yerinde kalıcı mesaj olarak durur; adres değiştirilince uyarı kalkar (K-760; 9.3.12).
  - **9.16.5 Yoktur:** kimlik kaydının silinmesi ve zorunlu alanın boşaltılması (`02 §3.1.1`, §10.1.2) · bakım modu ve siteyi kapatan bir yol (`02 §3.1.6`; `03 §10.2.1`) · ürünün içinden ETBİS kaydı — ürün kaydı yapmaz, yalnız bilgisini taşır (`02 §3.1.4`) · alan adı, e-posta gönderim kimliği, ödeme sağlayıcı anahtarları ve Google uygulamasının kimlik bilgileri — kurulum ayarlarıdır, panelden değişmez (`02 §3.1.2`, §10.1.3) · kimliğin doğruluğunun denetimi — firmanın sorumluluğudur (`02 §3.1.2`).
- **Aksiyonlar**
  - **9.16.6 Kimliği kaydetmek** (`03 §8.7.2.1`): onay istemez; başarı kısa süreli bildirimle söylenir (2.3.1.4). Dolu zorunlu alanı boşaltan kayıt kaydedilmez ve alan mesajıyla söylenir; tip değişikliği kaydedilir ve satış kapısı yeni tipin setini denetler (`02 §3.1.3`). Değişiklik eski ve yeni değerle işlem izine yazılır (`02 §10.3.1`). Kayıt satış durumunu değiştirdiyse "Satış durumu" bölümü ve üst satırın göstergesi güncel hâli hemen gösterir.
  - **9.16.7 Satışı geçici olarak kapatmak** (`03 §8.7.5.1`): onay penceresi (OB-04 — "geri alınamaz" cümlesi olmadan, 2.4.2.3) açık siparişlerin etkilenmeyeceğini söyler; cümlesi ve onay düğmesi "Satışı kapat" K-814'tedir (K-752). Sonuç: sepete ekleme ve ödeme kapanır, vitrin ve kurumsal içerik yayında kalır, müşterinin sepeti korunur; açık siparişler — havale onayı dahil — yürür (`02 §3.1.6`). Anahtar işlem izine yazılır (`02 §10.3.1`).
  - **9.16.8 Satışı yeniden açmak** (`03 §8.7.5.2`): onay istemez. Kapının öteki koşulları karşılanıyorsa satış açılır; karşılanmıyorsa anahtar kapanır ama satış kapalı kalır ve bölüm karşılanmayan koşulu adıyla söyler — anahtar tek başına satışı açmaz (§2.8.2).
  - **9.16.9 Eşzamanlı düzenleme** (OB-10, kayıt varyantı; `02 §3.31.1`; K-800): kimlik formunda. Anahtar o arada başka bir yöneticice değiştirildiyse işlem tekrarlanmaz ve bölüm güncel hâli gösterir.
- **9.16.10 Validasyonlar.** Seçili tipin zorunlu alanları ve üç tipin ortak zorunlu alanları girildikten sonra boşaltılamaz · e-posta ve KEP adresi e-posta biçimiyle, ETBİS doğrulama bilgisi ve davranış kurallarının bağlantısı adres biçimiyle denetlenir · il kapalı listedendir. Ret alan mesajıdır (OB-12). Ürün numaraların ve adreslerin doğruluğunu denetlemez (`02 §3.1.2`). Alan envanteri §7.2'dedir (5. oturum).
- **Durum × rol varyantları** (tam matris §6.3'te — 5. oturum)
  - **9.16.11 İlk kurulumda** kimlik boştur ve tip seçili değildir; "Satış durumu" bölümü satışın kapalı olduğunu ve karşılanmayan koşulları — kurulum kontrol listesinin maddeleriyle aynı adlarla — gösterir (`02 §10.8.2`; `03 §8.7.1.1`; 9.3.4). Kurumsal taraf kapıya bağlı değildir: kimlik eksikken de içerik yayına alınır (`02 §3.1.5`).
  - **9.16.12 Satış kapalıyken** bölüm sebebi adıyla söyler — geçici kapatma ya da karşılanmayan koşul; ikisi birlikte olduğunda ikisi de yazar. Bir koşulun düşmesi açık siparişleri etkilemez (`03 §1.10.8`, §3.5.4.1; K-664). Açık siparişlerin yürümesi için ekranda bir işlem gerekmez.
- **9.16.13 Boş / yükleniyor / hata durumları.** Ekranın boş hâli yoktur. Form ve bölüm kendi yerlerinde yüklenir (§2.7.2); kayıt beklenmeyen bir sebeple tamamlanamazsa form korunur (§2.7.3.1); anahtarın onayı sonuçsuz kalırsa ekran "işlem yapılmadı" demez, bölümü yeniler ve güncel durumu gösterir (§2.7.3.3); sayfa açılamazsa §2.7.3.2.
- **9.16.14 Responsive notları.** "Satış durumu" her sınıfta en üsttedir; dar sınıfta koşul satırları alt alta, anahtar bölümün sonundadır. Form dar ve orta sınıfta tek sütundur. Hiçbir alan bir sınıfta gizlenmez.

*Kaynak: `02 §3.1.1`–§3.1.6, §3.23.3, §3.31.1, §6.9.1, §9.3, §10.1.2, §10.1.3, §10.3.1, §10.8.2, §12.5 · `03 §1.10.8`, §3.5.4.1, §8.7.1.1, §8.7.1.2, §8.7.2.1, §8.7.2.3, §8.7.5.1, §8.7.5.2, §10.1.1.6, §10.2.1 · `10 §2` KP-38, KP-58, KP-60 · `10 §4.1` ÖK-8 · park satırı K-14 · K-574 · K-744, K-752, K-760, K-799, K-800, K-814, K-815 · devir: K-480 (üç firma tipi), K-547 (meslek odası), K-574 (ETBİS alanı ve hatırlatması), K-627 (KEP adresi ve isteğe bağlı iki alan).*

### 9.17 E-45 — Ödeme yöntemleri

- **9.17.1 Aktör · giriş · çıkış.** Yönetici. Giriş: menünün Ayarlar › Ödeme yöntemleri girişi (3.2.15) · kurulum kontrol listesinin ödeme yöntemi maddesi (3.4.4) · "Satış durumu"nun ödeme yöntemi koşulu (3.4.15) · ana sayfanın satış kapalı uyarısı (3.4.3). Çıkış: çerçevenin geçişleri (§3.2).
- **9.17.2 İlk görülen:** iki yöntemin açık ya da kapalı olduğu; birincil düğme "Kaydet".
- **Bilgi hiyerarşisi** — tek form (OB-12):
  - **9.17.3 (1) Kart** — salt okunur satır: sağlayıcı anahtarları kurulumda tanımlıysa kartın açık olduğu ve panelden açılıp kapatılmadığı; tanımlı değilse kartın kurulumda tanımlı olmadığı ve ödeme adımında görünmediği (`02 §3.21.4`; `03 §3.5.4.3`).
  - **9.17.4 (2) Havale/EFT** — açık ya da kapalı anahtarı ve IBAN alanı; havale açıkken IBAN zorunludur (`02 §3.21.5`). Bölüm IBAN'ın her siparişe onay anındaki hâliyle yazıldığını, IBAN'ın girilmesinin ve değişmesinin bütün yöneticilere e-postayla bildirildiğini söyler (`02 §3.21.5`, §9.3.3; K-616) ve havale ödeme süresinin E-46'da olduğunu gösterir. Altında serbest metin hatırlatması (K-816).
  - **9.17.5 Yoktur:** kapıda ödeme (`02 §3.21.2`) · kartın panelden açılıp kapatılması ve sağlayıcı anahtarlarının girilmesi (`02 §3.21.4`, §10.1.3) · birden çok IBAN · ödemesi beklenen siparişlerin IBAN'ını panelden değiştirmek — IBAN siparişe donmuştur (`02 §3.21.5`).
- **Aksiyonlar**
  - **9.17.6 Havaleyi açmak ve ilk IBAN'ı girmek** (`03 §8.7.3.1`): onay istemez. IBAN boşken havale açılmaz ve alan mesajı K-817'nin metniyle bunu söyler (`03 §3.5.4.2`). IBAN'ın ilk girişi de bütün yöneticilere bildirilir; değişiklik eski ve yeni değerle işlem izine yazılır (`02 §10.3.1`). Başarı kısa süreli bildirimle söylenir.
  - **9.17.7 IBAN'ı değiştirmek** (`03 §8.7.3.2`): onay penceresi (OB-04, 2.4.2.3) ödemesi beklenen havale siparişlerinin sayısını ve onların eski IBAN'ı göstermeye devam edeceğini söyler; cümlesi ve "IBAN'ı değiştir" K-814'tedir (K-663). Yeni IBAN yalnız yeni siparişlere işler. Eski hesap kullanılamıyorsa ya da değişiklik yetkisiz yapılmışsa o siparişler E-37'de firma iptaliyle ("diğer", açıklamayla) kapatılır (`03 §8.7.3.2`; 9.9.19).
  - **9.17.8 Havaleyi kapatmak** (`03 §8.7.3.3`): onay penceresi (2.4.2.3) ödemesi beklenen havale siparişlerinin sayısını ve etkilenmeyeceklerini, açık başka yöntem yoksa satışın da kapanacağını söyler; cümlesi ve "Havaleyi kapat" K-814'tedir (K-664). Bekleyen havale siparişi donmuş IBAN'ıyla sürer; onları kapatmak isteyen yönetici firma iptalini kullanır (`03 §3.5.4.15`).
  - **9.17.9 Eşzamanlı düzenleme** (OB-10, kayıt varyantı; `02 §3.31.1`; K-800).
- **9.17.10 Validasyonlar.** IBAN biçim ve sağlama basamağıyla denetlenir (K-817) · havale açıkken IBAN boşaltılamaz. Ret alan mesajıdır (OB-12). Alan envanteri §7.2'dedir (5. oturum).
- **Durum × rol varyantları** (tam matris §6.3'te — 5. oturum)
  - **9.17.11** Kart tanımlı ve havale kapalı · kart tanımlı ve havale açık · kart tanımsız ve havale açık — satış yalnız havaleyle açıktır · ikisi de kapalı — satış kapısının ikinci koşulu karşılanmaz ve "Satış durumu" bunu söyler (`03 §3.5.4.3`, §3.5.4.4).
  - **9.17.12 İlk kurulumda** havale kapalı ve IBAN boştur; kartın hâli kurulumdan gelir. Kart tanımlı değilse kurulum kontrol listesinin ödeme maddesi bu ekrana götürür (9.3.4).
  - **9.17.13** Yetkisiz bir değişiklikten şüphelenen yönetici IBAN'ı bu ekranda düzeltir; değişikliği yapanı işlem izinden okur (E-52) ve eski IBAN'lı siparişleri firma iptaliyle kapatır — ürün değişikliği kendiliğinden geri almaz (`03 §6.2.6.1`, §6.2.6.2, §10.4.6). Şifresini E-50'den değiştirir.
- **9.17.14 Boş / yükleniyor / hata durumları.** Ekranın boş hâli yoktur. Kayıt beklenmeyen bir sebeple tamamlanamazsa form korunur (§2.7.3.1); onay sonuçsuz kalırsa ekran "işlem yapılmadı" demez, güncel hâli yeniden yükler (§2.7.3.3); sayfa açılamazsa §2.7.3.2.
- **9.17.15 Responsive notları.** Tek sütundur; sınıflar arasında yalnız satır uzunluğu değişir. IBAN dar sınıfta satır kırmadan okunacak biçimde dörderli gruplarla gösterilir.

*Kaynak: `02 §3.1.5`, §3.1.6, §3.21.2, §3.21.4, §3.21.5, §3.23.3, §3.31.1, §6.9.2–§6.9.4, §6.9.13–§6.9.15, §9.3.3, §10.1.2, §10.1.3, §10.3.1 · `03 §3.5.4.2`–§3.5.4.4, §3.5.4.13–§3.5.4.15, §6.2.6.1, §6.2.6.2, §6.3.1.3, §8.7.3, §10.1.1.6, §10.4.6 · `10 §2` KP-59, KP-60 · `10 §4.1` ÖK-9 · K-744, K-800, K-814, K-816, K-817 · devir: K-616 (IBAN bildiriminin kapsamı), K-663 (IBAN değişikliğinin onayı), K-664 (havalenin kapatılmasının onayı).*

### 9.18 E-46 — Kargo, süreler ve varsayılanlar

- **9.18.1 Aktör · giriş · çıkış.** Yönetici. Giriş: menünün Ayarlar › Kargo, süreler ve varsayılanlar girişi (3.2.16) · kurulum kontrol listesinin iade adresi maddesi (3.4.4) · "Satış durumu"nun iade adresi koşulu (3.4.15) · ana sayfanın satış kapalı uyarısı (3.4.3). Çıkış: çerçevenin geçişleri (§3.2).
- **9.18.2 İlk görülen:** beş bölüm, kurulumda dolu gelen değerleriyle; birincil düğme yoktur — her bölüm kendi "Kaydet"ini taşır (K-752; 2.12.1.5).
- **Bilgi hiyerarşisi** — beş bölüm, bu sırayla (K-752); her bölümün başında değerlerin siparişe donduğu ve değişikliğin yalnız yeni siparişlere işlediği yazar (`02 §3.23`; `03 §8.7`):
  - **9.18.3 (1) Kargo ücreti ve eşikler** — kargo ücreti, sipariş başına ve KDV dahil (P-1) · ücretsiz kargo eşiği (P-3) · asgari sipariş tutarı (P-4); eşik ve asgari tutar boşken özellik kapalıdır ve alan bunu söyler (`02 §3.18.1`, §3.19.1, §3.19.3). Bölüm kargonun KDV'sinin bir ayar olmadığını, fiziksel kalemlerin oranını izlediğini söyler (`02 §3.19.2`). Altında serbest metin hatırlatması (K-816).
  - **9.18.4 (2) Teslimat illeri** — il listesinden çoklu seçim; varsayılan tüm Türkiye'dir ve listede "Tümünü seç" ile "Seçimi kaldır" vardır (`02 §3.20.1`; K-817). Kısıt varsa vitrinde adres adımından önce görünür (K-773). Hiçbir il seçili değilse bölüm fiziksel ürün içeren siparişin verilemeyeceğini söyler (K-817).
  - **9.18.5 (3) Süreler** — kargoya verme süresinin firma varsayılanı (P-5) · havale ödeme süresi (P-7); ikisi de iş günüyle (`02 §3.17.4`, §3.20.4, §3.20.5). Bölüm ürünün kendi kargoya verme süresini ürün formunda taşıyabileceğini söyler (9.5.7). Altında serbest metin hatırlatması (K-816).
  - **9.18.6 (4) Ürün varsayılanları** — dijital kalemin indirme hakkı (P-8) · KDV oranının varsayılanı (P-10) (K-752). Bölüm indirme hakkının sipariş anında kaleme donduğunu ve KDV varsayılanının yalnız yeni ürünün formuna geldiğini, var olan ürünün oranını değiştirmediğini söyler (`02 §3.8.2`, §3.12.5; `03 §8.7.4.4`). Ürün formundaki firma ayarları — P-6, P-9, P-44 — ürün formundadır (9.5.7).
  - **9.18.7 (5) İade adresi** — alıcının adı, il — kapalı listeden —, ilçe ve açık adres; telefon isteğe bağlıdır (K-817). Zorunlu bir ayardır ve satış kapısının koşuludur; firma kimliğindeki adresten ayrıdır (`02 §3.1.7`). Altında serbest metin hatırlatması (K-816).
  - **9.18.8 Yoktur:** ağırlık, adet ya da ile göre kargo ücreti ve kargo şirketi entegrasyonu (`02 §3.19.1`) · ilçe düzeyinde teslimat kısıtı (`02 §3.20.1`) · teslimat seçenekleri ve mağazadan teslim (`02 §3.20.2`) · çitin dışında süre (`02 §10.1.2`) · kargonun KDV oranı ayarı (`02 §3.19.2`) · ürün sabitlerinin değiştirilmesi (`02 §10.1.3`).
- **Aksiyonlar**
  - **9.18.9 Bölümü kaydetmek** (`03 §8.7.4.1`–§8.7.4.4): onay istemez; başarı kısa süreli bildirimle söylenir (2.3.1.4). Kargoya verme süresinin ve kargo ücretinin değişikliği üretilen metinlerin sürümünü artırır (`03 §8.7.2.3`, §8.7.4.3). Değişen her ayar eski ve yeni değerle işlem izine yazılır (`02 §10.3.1`). Sepeti açık müşteri değişikliği sepette görür; onay anına denk gelirse özet farkı olarak söylenir (`03 §8.7.4.1`).
  - **9.18.10 İade adresini girmek ve değiştirmek** (`03 §8.7.4.5`): ilk giriş onay istemez. Değişiklikte onay penceresi (OB-04, 2.4.2.3) "iade malı bekleniyor" kalemlerin sayısını, onların beyanındaki adresin değişmediğini ve eski adrese gelen malı teslim almanın firmanın yükümlülüğü olduğunu söyler; cümlesi ve "İade adresini değiştir" K-814'tedir (K-665). Yeni beyanlar güncel adresi alır (`03 §3.5.4.16`); üretilen metinlerin sürümü artar.
  - **9.18.11 Eşzamanlı düzenleme** (OB-10, kayıt varyantı; `02 §3.31.1`; K-800): uyarı yalnız kaydedilen bölüm için çıkar (K-752).
- **9.18.12 Validasyonlar.** Tutarlar sıfır ya da sıfırdan büyük, en çok iki ondalıklı · kargoya verme süresi P-5'in üst çitini, havale ödeme süresi P-7'nin alt ve üst çitini aşamaz ve çit dışı değer sebebiyle reddedilir — metinleri K-817'dedir (`03 §3.5.4.5`; 2.12.1.4) · indirme hakkı birden küçük olamaz · iade adresinin zorunlu alanları boşaltılamaz. Ret alan mesajıdır (OB-12). Alan envanteri §7.2'dedir (5. oturum).
- **Durum × rol varyantları** (tam matris §6.3'te — 5. oturum)
  - **9.18.13 İlk kurulumda** firma ayarları dolu gelir — kargo ücreti 0 TL, eşik ve asgari tutar boş, süreler ve varsayılanlar §11'in değerleriyle, teslimat illeri tüm Türkiye — ve iade adresi boştur; satış kapısının bu ekrandaki tek koşulu iade adresidir ve kurulum kontrol listesinin maddesi buraya götürür (`02 §10.1.3`, §10.8.2, §11.1; 9.3.4).
  - **9.18.14** İade adresi boşaltılamadığı için girildikten sonra satış kapısının bu koşulu düşmez. Satış kapalıyken ekran aynı işler; ayarlar yeni siparişler içindir.
- **9.18.15 Boş / yükleniyor / hata durumları.** Ekranın boş hâli yoktur. Her bölüm kendi yerinde kaydedilir ve düğmesi ikinci kez basılamaz (§2.7.2); beklenmeyen hatada mesaj bölümün içinde çıkar ve bölüm korunur (§2.7.3.1); iade adresinin onayı sonuçsuz kalırsa ekran güncel adresi yeniden yükler (§2.7.3.3); sayfa açılamazsa §2.7.3.2.
- **9.18.16 Responsive notları.** Bölümler her sınıfta alt alta, 9.18.3–9.18.7'nin sırasıyladır; il listesi dar sınıfta açılır bölümdür ve seçili il sayısını başlığında taşır. Hiçbir alan bir sınıfta gizlenmez.

*Kaynak: `02 §3.1.7`, §3.8.2, §3.12.5, §3.17.4, §3.18.1, §3.19.1–§3.19.3, §3.20.1, §3.20.2, §3.20.4, §3.20.5, §3.23, §3.31.1, §3.33.3, §3.33.8, §6.9.5, §6.9.13, §6.9.16, §10.1.2, §10.1.3, §10.3.1, §10.8.2, §11.1 · `03 §3.5.4.5`, §3.5.4.10, §3.5.4.13, §3.5.4.16, §8.7.2.3, §8.7.4 · `10 §2` KP-59, KP-64 · `10 §4.1` ÖK-11 · K-752, K-773, K-800, K-814, K-816, K-817 · §1.3 GAP-11 · devir: K-665 (iade adresi değişikliğinin onayı).*

### 9.19 E-47 — Yasal metinler

- **9.19.1 Aktör · giriş · çıkış.** Yönetici. Giriş: menünün Ayarlar › Yasal metinler girişi (3.2.17) · kurulum kontrol listesinin yasal metin maddesi (3.4.4) · "Satış durumu"nun yasal metin koşulu (3.4.15) · ana sayfanın satış kapalı uyarısı (3.4.3). Çıkış: aydınlatma metninin eksik firma kimliği alanının bağlantısı → E-44 · çerçevenin geçişleri (§3.2).
- **9.19.2 İlk görülen:** dört yasal metnin satırları — her birinin yayındaki sürümü ve tarihi ya da henüz yayına alınmadığı; birincil düğme düzenlenen metnin "Yayına al"ıdır (K-818).
- **Bilgi hiyerarşisi** — dört metin, iki grup (`02 §3.33.1`; K-818):
  - **9.19.3 (1) Firmanın düzenlediği iki metin** — aydınlatma metni ve çerez politikası. Her biri bir bölümdür: yayındaki sürümün numarası ve yayın tarihi ya da metnin henüz yayına alınmadığı · tamamlanma satırı — eksik olan adıyla · düzenlenen metin · "Taslağı kaydet" ve "Yayına al". Düzenlenen metin yayındaki sürümden farklıysa bölüm bunu söyler (K-818). Düzenleme ürünün taslağıyla başlar ve kapalı metin biçimi setini kullanır (`02 §3.11.6`, §3.33.2; K-818).
    - **Aydınlatma metni:** taslak firma kimliği alanlarından beslenen yer tutucuları kimlikteki değerlerle gösterir; kimliğin eksik alanı tamamlanma satırında adıyla ve E-44'e götüren bağlantıyla durur. **Barındırma ve e-posta altyapısının konumu** bölümü firmanın doldurduğu ayrı, zorunlu bir alandır; **alıcı grupları** bölümünü firma adıyla doldurur — kapıya girmez (`02 §3.1.5`, §3.33.2). Tamamlanma satırı metin tamamlanıp yayına alınana kadar iletişim formunun, hesap kaydının ve yeni hesap açacak Google ile girişin de kapalı olduğunu söyler (§2.8.3).
    - **Çerez politikası:** taslak sitenin zorunlu çerezlerini ve ödeme sağlayıcısının çerçevesi için firmanın dolduracağı yer tutucuyu taşır; içeriğin kuralı `02 §12.2.5`'tedir (K-518, K-603).
  - **9.19.4 (2) Ayarlardan üretilen iki metin** — Ön Bilgilendirme Formu ve Mesafeli Satış Sözleşmesi; salt okunur. Her biri güncel sürümün numarasını ve tarihini ve metnin güncel hâlini gösterir; siparişe göre değişen yerler — kalemler, tutar, alıcı — yer tutucu olarak görünür (`02 §3.33.1`, §3.33.3; `03 §8.7.2.3`; K-818). Bölüm metni hangi ayarların beslediğini sayar: firma kimliği, kargo ücreti, kargoya verme süresi, iade adresi (`02 §3.33.3`).
  - **9.19.5 Yoktur:** yayındaki metni yayından çekmek — iki metin zorunludur ve boşaltılamaz; değişiklik yeni sürümle yapılır (`02 §3.33.2`; K-818) · ileri tarihli yayın ve metin değiştiğinde yeniden onay ya da bildirim (`02 §3.33.4`) · Ön Bilgilendirme Formu'nun ve sözleşmenin elle düzenlenmesi (`02 §10.1.2`) · "İşlem rehberi"nin düzenlenmesi — ürünün sabit metnidir (`02 §3.24.8`) · ayrı üyelik sözleşmesi ve envanterin dışında yasal metin (`02 §3.33.1`) · eski sürümlerin bu ekrandaki listesi — siparişe donmuş sürüm siparişin belgelerinden okunur (9.9.13; K-818) · çerez onay bandı (`02 §3.33.5`).
- **Aksiyonlar**
  - **9.19.6 Taslağı kaydetmek** — yayındaki sürümü değiştirmez; onay istemez, başarı kısa süreli bildirimle söylenir (2.3.1.4). Metin boşaltılamaz (`02 §3.33.2`).
  - **9.19.7 Yayına almak** (`03 §8.7.2.2`): aydınlatma metni tamamlanmadan yayına alınmaz ve tamamlanma satırı eksik olanı söyler (`02 §3.33.2`). Yayına alınınca sürüm kendiliğinden artar ve vitrinde güncel sürüm görünür (E-11); yeniden onay alınmaz ve bildirim gitmez (`02 §3.33.3`, §3.33.4). Onay penceresi yoktur — kaynak yayına almayı onay isteyen işlemler arasında saymaz (2.4.2); bölüm yeni sürümün numarasını ve tarihini kalıcı olarak gösterir. Sürüm değişikliği işlem izine yazılır (`02 §10.3.1`). İki metnin ilk yayını satış kapısının ve veri toplayan girişlerin kapısının koşuludur (`02 §3.33.9`).
  - **9.19.8 Eşzamanlı düzenleme** (OB-10, kayıt varyantı; `02 §3.31.1`; K-800): her metin kendi bölümünde.
- **9.19.9 Validasyonlar.** Firmanın düzenlediği iki metin ve aydınlatmanın barındırma konumu bölümü boşaltılamaz; ret alan mesajıdır, tamamlanma eksiği yerinde kalıcıdır (OB-12; `02 §3.33.2`). Alan envanteri §7.2'dedir (5. oturum).
- **Durum × rol varyantları** (tam matris §6.3'te — 5. oturum)
  - **9.19.10 İlk kurulumda** iki metin ürünün taslağıyla gelir ve hiç yayına alınmamıştır; vitrinde bağlantıları yoktur (K-770). Aydınlatma metni barındırma konumu bölümü ve firma kimliği dolana kadar tamamlanmamıştır; satış ve veri toplayan girişler kapalıdır ve kurulum kontrol listesinin maddesi buraya götürür (`02 §3.1.5`, §10.8.2; 9.3.4).
  - **9.19.11** Firma kimliğinin bir alanı sonradan değişirse aydınlatmanın yer tutucusu yeni değeri gösterir; üretilen iki metnin sürümü kendiliğinden artar (`03 §8.7.2.1`, §8.7.2.3). Bir sürüm artışı onay adımı açık bir müşteride siparişin oluşmasını durdurur — ekranın değil sipariş onayının kuralıdır (`03 §2.4.8`).
- **9.19.12 Boş / yükleniyor / hata durumları.** Ekranın boş hâli yoktur — iki metin taslakla gelir. Yayına alma sürerken düğme ikinci kez basılamaz (§2.7.2); beklenmeyen hatada düzenlenen metin korunur (§2.7.3.1); sayfa açılamazsa §2.7.3.2.
- **9.19.13 Responsive notları.** Dört metnin satırları her sınıfta alt alta durur; düzenleme alanı dar sınıfta tam genişliktedir ve "Taslağı kaydet" ile "Yayına al" alanın altında kalır. Uzun metin satır kırar; yatay kaydırma yoktur.

*Kaynak: `02 §3.1.5`, §3.11.6, §3.24.8, §3.31.1, §3.33.1–§3.33.5, §3.33.9, §10.1.2, §10.3.1, §12.2, §12.2.5, §12.5 · `03 §2.4.8`, §3.5.4.10, §8.7.2.2, §8.7.2.3 · `10 §2` KP-61 · `10 §4.1` ÖK-10 · K-770, K-800, K-818 · devir: K-603 (çerez politikası taslağının çerezleri — içerik `02 §12.2.5`).*

### 9.20 E-49 — Yönetici hesapları

- **9.20.1 Aktör · giriş · çıkış.** Yönetici. Giriş: menünün Ayarlar › Yönetici hesapları girişi (3.2.18). Çıkış: davet, geri çekme ve kaldırma ekranda kalır (3.4.17) · çerçevenin geçişleri (§3.2).
- **9.20.2 İlk görülen:** yöneticilerin listesi ve altında geçerli davetler; birincil düğme "Yönetici davet et".
- **Bilgi hiyerarşisi** — iki liste (OB-20; K-820):
  - **9.20.3 (1) Yöneticiler** — satır: ad · e-posta · ekranı açan yöneticinin satırında "siz" (`02 §10.2.4`). Ad ve e-posta yöneticilerin iletişim bilgisidir; ihlalde ulaşmanın kaynağı bu listedir (`02 §10.7.4`, §12.2.9; K-514). Sıra adın alfabetik sırasıdır (K-820). Satırın işlemi "Yöneticiyi kaldır"dır; ekranı açan yöneticinin kendi satırında yoktur (`02 §10.2.2`).
  - **9.20.4 (2) Davetler** — yalnız geçerli davetler: davet edilen adres · daveti gönderen yönetici · gönderim tarihi · bağlantının geçerli olduğu son gün (§2.6.6; Z-3) · davet e-postası ulaşmamışsa "e-posta ulaşmadı" işareti — yeniden gönder düğmesi taşımaz (2.5.3.1; K-540). Satırın işlemleri "Daveti geri çek" ve "Yeni davet gönder"dir (K-820). Geçersizleşen davet — kullanılan, geri çekilen, süresi dolan, aynı adrese giden yeni davetle ya da gönderenin kaldırılmasıyla düşen — listeden çıkar; kullanılan davetin yerinde açılan hesap yöneticiler listesinde görünür (`03 §1.10.7`; K-820). Sıra gönderim tarihine göre en yeni önce. Davet yoksa liste görünmez (§2.7.1.1).
  - **9.20.5 Davet formu** — "Yönetici davet et" ekranın içinde açılır: e-posta alanı ve "Daveti gönder". Adrese gönderilmiş geçerli bir davet varsa formda yeni davetin onu geçersiz kılacağı yazar (`02 §10.2.6`; K-820).
  - **9.20.6 Yoktur:** rol, yetki matrisi ve rol bazlı veri kısıtı — bütün yöneticiler aynı yetkidedir (`02 §10.2.3`) · pasifleştirme (`02 §10.2.2`) · kendi hesabını kaldırma (`02 §10.2.2`) · başka bir yöneticinin adını, e-postasını ya da şifresini değiştirme — her yönetici kendi hesabını E-50'den yönetir (`02 §10.2.4`) · davet e-postasının yeniden gönderilmesi — yol yeni davettir (`03 §3.1.1`, §7.1.53) · ürünün içinden erişim kurtarma (`02 §10.2.7`).
- **Aksiyonlar**
  - **9.20.7 Davet göndermek** (`03 §8.8.1`): onay istemez. Adres bir yönetici hesabına aitse davet gönderilmez ve K-820'nin alan mesajı bunu söyler; müşteri hesabına ait adres engel değildir — ayrı bir yönetici hesabı açılır (`02 §10.2.5`). Aynı adrese giden yeni davet öncekini geçersiz kılar ve ömrü kendi gönderiminden işler (Z-3). Davet listede satır olarak görünür; sonuç kısa süreli bildirimle söylenir (2.3.1.4). Gönderim işlem izine yazılır ve bütün yöneticilere bildirilir (`02 §10.2.6`; F-6).
  - **9.20.8 Daveti geri çekmek** (`03 §8.8.3`): onay istemez; bağlantı o anda geçersizleşir ve satır listeden çıkar. Davetliye bildirim gitmez; geri çekme işlem izine yazılır (`02 §10.2.6`).
  - **9.20.9 Yeni davet göndermek** — "e-posta ulaşmadı" ya da yanlış yazılmış adresin yolu: satırın adresiyle 9.20.7'yi başlatır; önceki davet geçersizleşir (`03 §8.8.1`, §8.8.6; K-820).
  - **9.20.10 Yöneticiyi kaldırmak** (`03 §8.8.5`): onay penceresi (OB-04, geri alınamaz — 2.4.2.2) kaldırılanın adını ve e-postasını, açık oturumlarının hemen kapanacağını, gönderdiği kullanılmamış davetlerin sayısını ve kaldırmanın bütün yöneticilere bildirileceğini söyler; ardından "Bu işlem geri alınamaz."; cümlesi ve "Yöneticiyi kaldır" K-814'tedir (K-662). Sonuç: hesap kalkar, açık oturumları anında sonlanır, davetleri geçersizleşir ve davetler listesinden çıkar; işlem izindeki satırları adıyla ve e-postasıyla okunur kalır (`02 §10.2.2`). Kaldırma işlem izine yazılır ve kaldırılan dahil bütün yöneticilere bildirilir (F-6). Kaldırılan yönetici o anda paneldeyse oturumu kapanır ve bir sonraki işleminde panel girişine iner (§3.6.5).
  - **9.20.11 Engeller** (`03 §3.5.4.11`): kendi satırında kaldırma yoktur; iki yöneticinin birbirini aynı anda kaldırdığı hâlde ikinci kaldırma uygulanmaz ve K-820'nin mesajı son yöneticinin kaldırılamayacağını söyler — engel yerinde kalıcıdır (2.3.3).
- **9.20.12 Validasyonlar.** Davetin adresi e-posta biçimiyle denetlenir ve yönetici hesapları içinde kullanılmamış olmalıdır. Ret alan mesajıdır (OB-12). Alan envanteri §7.2'dedir (5. oturum).
- **Durum × rol varyantları** (tam matris §6.3'te — 5. oturum)
  - **9.20.13** Panel tek roldür; her yönetici her yöneticiyi — kendisi dışında — kaldırır ve her daveti geri çeker (`02 §10.2.3`). Tek yönetici varken kaldırma işlemi görünmez — tek satır ekranı açanın satırıdır.
  - **9.20.14 İlk kurulumda** listede kurulumda açılan ilk yönetici durur — adı kurulumda verilmiştir (`02 §10.2.1`; K-736) — ve davet yoktur. Satış kapısı ekranı etkilemez.
  - **9.20.15** Ele geçirilmiş bir hesabın kaldırılması bu ekranın olağan kaldırmasıdır; ardından yönetici havale IBAN'ını denetler (E-45) ve işlem izini okur (E-52) (`03 §6.3.2.4`, §10.4.6). Erişimini kaybeden yöneticinin yolu: kalan yönetici yeni adrese davet gönderir ve eski hesabı kaldırır (`03 §10.4.3`).
- **9.20.16 Boş / yükleniyor / hata durumları.** Ekranın boş hâli yoktur — en az bir yönetici vardır; davetler listesi boşken görünmez. Kaldırmanın onayı sonuçsuz kalırsa ekran "işlem yapılmadı" demez, listeyi yeniler ve sonucu gösterir (§2.7.3.3); davet gönderimi beklenmeyen bir sebeple tamamlanamazsa form korunur (§2.7.3.1); sayfa açılamazsa §2.7.3.2. E-posta altyapısının kesintisinde davet ulaşmaz; yol altyapı dönünce yeni davettir (`03 §10.1.2.5`).
- **9.20.17 Responsive notları.** Geniş sınıfta iki tablo alt alta; orta sınıfta e-posta ve gönderen satırın altına iner; dar sınıfta kart listesidir — yönetici kartı adı ve e-postayı, davet kartı adresi, son günü ve işareti taşır; işlemler kartın menüsündedir (§2.13.4). Davet formu dar sınıfta listenin üstünde açılır.

*Kaynak: `02 §6.9.11`, §6.9.12, §10.1.2, §10.2.1–§10.2.7, §10.3.1, §10.7.4, §12.2.9 · `03 §1.7.3.1`, §1.10.5, §1.10.7, §3.1.1, §3.5.4.11, §4.1.3, §4.1.29, §6.2.6.4, §6.3.2.4, §7.1.53, §7.3.40, §8.8, §10.1.2.2, §10.1.2.5, §10.4.3, §10.4.6 · `10 §2` KP-62 · K-736, K-744, K-814, K-820 · devir: K-514 (yöneticilerin iletişim bilgisi), K-540 (davet satırının işareti), K-660 (geri çekme), K-662 (kaldırmanın onayı ve davet sayısı).*

### 9.21 E-50 — Yöneticinin kendi hesabı

- **9.21.1 Aktör · giriş · çıkış.** Yönetici. Giriş: üst satırın "Hesabım"ı (3.2.21) · yeniden doğrulamadan dönüş (3.4.16) · yeni adresin doğrulamasından sonra, oturum açıksa (E-54; 3.5.6). Çıkış: şifre ya da e-posta değiştirme → E-54'ün yeniden doğrulama adımı → E-50 (3.4.16).
- **9.21.2 İlk görülen:** yöneticinin adı ve e-postası; birincil düğme yoktur — her bölüm kendi düğmesini taşır (E-25'in kalıbı).
- **Bilgi hiyerarşisi** — üç bölüm (`02 §10.2.4`; 5.25'in kalıbı):
  - **9.21.3 (1) Ad** — hesabın adı ve düzenleme; yeniden doğrulama istemez. Bölüm adın yönetici listesinde, işlem izinde ve yöneticilere giden bildirimlerde okunduğunu, müşteriye görünmediğini söyler (`02 §10.2.4`; K-736).
  - **9.21.4 (2) E-posta** — hesabın e-postası ve "E-postayı değiştir". Bölüm bu adresin firma kimliğindeki iletişim e-postası olmadığını ve firmaya giden bildirimlerin adresini değiştirmediğini söyler (`02 §10.2.4`; `03 §9.3.7`). **Bekleyen değişiklik** varsa yeni adresiyle, bağlantının geçerli olduğu son günle ve bağlantıyı yeniden isteyen düğmeyle görünür; yeni bir adres yazmak bekleyenin yerine geçer (5.25.4'ün kalıbı; K-734). **Geri alma bağlantısı açıkken** (Z-45) "E-postayı değiştir" kapalıdır ve yerinde sebebi ile bağlantının bitiş tarihi durur (K-819).
  - **9.21.5 (3) Şifre** — "Şifreyi değiştir".
  - **9.21.6 Yoktur:** kendi hesabını silme ya da kaldırma (`02 §10.2.2`) · Google ile giriş, Google bağlama ve Google ile yeniden doğrulama (`02 §10.2.5`) · iki adımlı doğrulama (`02 §10.2.4`) · sepet, sipariş ve adres defteri — yönetici hesabının sepeti ve siparişi olmaz (`02 §10.2.5`) · bildirim tercihi.
- **Aksiyonlar**
  - **9.21.7 Adı değiştirmek** (`03 §9.3.7`): onay istemez; başarı kısa süreli bildirimle söylenir (2.3.1.4). Yeni ad üst satırda, yönetici listesinde ve işlem izinin sonraki satırlarında görünür.
  - **9.21.8 E-postayı değiştirmek** (`03 §9.3.7`, §9.3.4): E-54'ün yeniden doğrulamasından sonra bölümün içinde yeni adres alanı açılır; gönderilince yeni adrese doğrulama bağlantısı gider ve değişiklik bekleyen değişiklik olarak görünür. Hesap yeni adres doğrulanana kadar eski adresle çalışır. Yeni adres başka bir yönetici hesabınınsa değişiklik başlamaz ve alan mesajı bunu söyler — metni K-819'dadır, K-786'nın kalıbıdır; bir müşteri hesabının adresi engel değildir (`02 §10.2.5`, §3.13.16). Değişiklik ve geri alınması işlem izine yazılır (`02 §10.3.1`).
  - **9.21.9 Bağlantıyı yeniden istemek** — bekleyen değişikliğin bağlantısı; L-9 ile limitlidir ve yeniden istek ilk bağlantının ömrünü uzatmaz; mesajı K-779'un ikinci metnidir (`03 §9.3.4`; 5.25.12).
  - **9.21.10 Şifreyi değiştirmek** (`02 §10.2.4`): E-54'ün yeniden doğrulamasından sonra bölümün içinde yeni şifre alanı açılır; kaydedilince bölüm şifrenin değiştiğini ve öteki oturumların kapandığını yerinde kalıcı mesajla söyler — değişikliği yapan oturum açık kalır (5.25.10'un kalıbı; K-695).
- **9.21.11 Validasyonlar.** Ad boş olamaz (K-736) · yeni e-posta biçimi geçerli ve yönetici hesapları içinde tekil olmalıdır · yeni şifre politikası P-17 ve yaygın şifreler listesidir (`02 §3.13.15`). L-9 aşılınca nötr limit mesajı (§2.3.5). Alan envanteri §7.2'dedir (5. oturum).
- **9.21.12 Durum × rol varyantları.** Bekleyen değişiklik satırı ile geri alma engeli birlikte durmaz (5.25.15'in kuralı). Satış kapısı ekranı etkilemez. Tam matris §6.3'tedir (5. oturum).
- **9.21.13 Boş / yükleniyor / hata durumları.** Ekranın boş hâli yoktur. Her bölümün kaydı kendi yerinde bekler (§2.7.2); beklenmeyen hatada mesaj bölümün içinde çıkar (§2.7.3.1). Oturum ekrandayken kapanırsa §3.6.5.
- **9.21.14 Responsive notları.** Bölümler her sınıfta alt alta; dar sınıfta tek sütundur.

*Kaynak: `02 §3.13.14`–§3.13.16, §10.2.2, §10.2.4, §10.2.5, §10.3.1 · `03 §1.10.5`, §6.2.6.2, §9.3.4, §9.3.7 · K-734, K-736, K-779, K-786, K-819 · §1.3 GAP-8.*

### 9.22 E-51 — Satış özeti

- **9.22.1 Aktör · giriş · çıkış.** Yönetici. Giriş: menünün Raporlar › Satış özeti girişi (3.2.11). Çıkış: çerçevenin geçişleri (§3.2). Özetin satırları başka bir ekrana götürmez (K-821).
- **9.22.2 İlk görülen:** seçili dönem ve dönemin sipariş sayısı ile cirosu; ekranın birincil düğmesi yoktur — dönem seçimi özeti hemen yeniler (K-821).
- **Bilgi hiyerarşisi** — dönem seçimi ve dört bölge (`02 §10.6.2`; K-821):
  - **9.22.3 (1) Dönem seçimi** — hazır dönemler: Bugün · Dün · Son 7 gün · Son 30 gün · Bu ay · Geçen ay · Ölçüm penceresi · Özel aralık (OB-16, tarih aralığı; ileri tarih reddedilir). Ekran Son 30 gün seçili açılır. Tek gün seçmek — Bugün, Dün ya da başı ve sonu aynı gün olan özel aralık — tepe gün tetikleyicisinin okuduğu günlük sayıyı verir (`02 §10.6.2`; K-474, K-643). **Ölçüm penceresi** satışın kurulumda ilk açıldığı günden üç aydır; satış hiç açılmadıysa seçenek görünmez (`02 §10.6.3`; K-486). Seçili dönemin başı ve sonu tarihleriyle seçimin yanında yazar.
  - **9.22.4 (2) Sayılar** — **sipariş sayısı** · **ciro** · **iptal ve iade sayısı**. Sipariş sayısı ve ciro ödemesi tamamlanmış siparişleri sayar; Başarısız ve ödemesi bekleyen sipariş girmez; sipariş oluştuğu günün dönemine yazılır (`02 §10.6.2`; K-643). Ciro bu siparişlerin onaylanan toplamlarıdır; sonraki geri ödemeler düşülmez ve kart başlığının altında bunu söyleyen satır durur (K-821). İptal ve iade sayısı gecikme feshini de sayar ve caymayı beyan anında sayar (`02 §10.6.3`).
  - **9.22.5 (3) Ölçüler** — `02 §10.6.3`'ün dört ölçüsü, her biri ayrı bir kartta: **ödeme tamamlama oranı** · **iletişim talebi sayısı** — "KVKK talebi" ve "Sipariş hakkında" tipi dışındakiler · **kargoya verme sözüne uyum oranı** · **iptal ve iade oranı**. Oran kartı yüzdeyi ve altında payını ve paydasını sayıyla gösterir; tanımın tek cümlelik özeti kartın altındadır ve tanımın evi `02 §10.6.3`'tür (K-821). **Sıfır payda:** paydası seçili dönemde sıfır olan oran yüzde göstermez; kart "Değerlendirilemez" yazar ve altında paydanın neden boş olduğunu söyler — metni K-821'dedir (`02 §10.6.3`; K-490; §2.7.1.3).
  - **9.22.6 (4) Dağılımlar** — **en çok satan ürünler:** dönemin ödemesi tamamlanmış siparişlerinde en çok satılan on ürün, satılan adetle — ürün düzeyindedir, varyantlar birlikte sayılır; adet eşitse ciroya göre (K-821) · **ödeme yöntemi dağılımı:** kart ve havale/EFT, sipariş sayısı ve payıyla (`02 §10.6.2`).
  - **9.22.7 Yoktur:** ziyaretçi verisi, dönüşüm hunisi ve ürün görüntülenme sayısı — özet analitik değildir (`02 §10.6.2`) · önceki dönemle karşılaştırma ve grafik (K-821) · özetin dışa aktarılması ve saklanması (`02 §10.6.2`, §10.7.3) · satırlardan siparişe ya da ürüne geçiş (K-821).
- **Aksiyonlar**
  - **9.22.8 Dönem seçmek** — özet hemen yeniden hesaplanır; onay ve mesaj yoktur. Özel aralığın bitişi başlangıcından önce olamaz (OB-16).
- **9.22.9 Validasyonlar.** Özel aralıkta iki tarih zorunludur; ileri tarih ve ters aralık alan mesajıyla reddedilir (OB-16). Alan envanteri §7.2'dedir (5. oturum).
- **9.22.10 Durum × rol varyantları.** Özet her açılışta mevcut kayıttan hesaplanır, saklanmaz (`02 §10.6.2`); ödeme süresi dolmamış denemesi olan sepet ödeme tamamlama oranının paydasına süre dolunca girer (`02 §10.6.3`; K-534). Satış kapalıyken de özet okunur; kapalı geçen günler dönemin içindedir. Tam matris §6.3'tedir (5. oturum).
- **9.22.11 Boş / yükleniyor / hata durumları.** İlk kurulumda ve siparişsiz dönemde sayılar sıfır yazar, oranlar "Değerlendirilemez"dir ve dağılım tabloları boş hâl satırını taşır — ekran boş kalmaz (§2.7.1.3). Hesap sürerken kartlar kendi yerlerinde bekler (§2.7.2); sayfa açılamazsa §2.7.3.2.
- **9.22.12 Responsive notları.** Geniş sınıfta sayılar ve ölçüler kart sıralarıdır; orta sınıfta ikişerli, dar sınıfta tek tek alt alta dizilir. Dağılım tabloları dar sınıfta kart listesine döner (§2.13.4). Dönem seçimi dar sınıfta açılır listedir.

*Kaynak: `02 §10.6.2`, §10.6.3, §10.7.3 · `03 §8.7.1.2`, §8.9.3 · `10 §2` KP-53 · K-474, K-643, K-821 · devir: K-484 (uyum oranının paydası), K-485, K-490 (sıfır paydanın gösterimi), K-495, K-534 (ödeme tamamlama oranının dönemi), K-580 (gecikme feshinin sayılması); `02 §10.6.2`'nin "dönem seçimi ve ekranın düzeni" ve §10.6.3'ün "sıfır payda" devri.*

### 9.23 E-52 — İşlem izi

- **9.23.1 Aktör · giriş · çıkış.** Yönetici. Giriş: menünün Raporlar › İşlem izi girişi (3.2.12). Çıkış: çerçevenin geçişleri (§3.2); satırlar başka bir ekrana götürmez (K-822).
- **9.23.2 İlk görülen:** izin satırları en yeni önce, tarih aralığı ve yönetici süzgeçleriyle; ekranın birincil düğmesi yoktur.
- **Bilgi hiyerarşisi** — düz bir liste (`02 §10.3.4`; K-822):
  - **9.23.3 (1) Süzgeçler** — tarih aralığı (OB-16; ileri tarih reddedilir) · yönetici — kapalı liste; kaldırılmış yöneticiler de adlarıyla ve "kaldırıldı" ekiyle listededir (`02 §10.2.2`, §10.3.5). Ekran süzgeçsiz açılır; iki süzgeç birlikte uygulanır.
  - **9.23.4 (2) Liste** (OB-20) — satır **kim, ne zaman, ne değişti** üçlüsünü taşır (`02 §10.3.1`): tarih ve saat · yöneticinin adı ve e-postası — kaldırılmış yöneticide de (`02 §10.2.2`) · izin alanı — sekiz alandan biri (`02 §10.3.1`) · işlem ve dokunduğu kayıt — sipariş numarası, ürünün ya da içerik kaydının adı, ayarın adı, davet edilen adres · ayar değişikliğinde eski ve yeni değer. Sipariş satırı müşterinin kişisel verisini değer olarak taşımaz — adres düzeltmesinde alan ve işlem, iç notta yalnız notun yazıldığı (`02 §10.3.1`, §10.4.7).
  - **9.23.5 (3) Sayfa gezinmesi** — sayfa başına 25 satır (§2.13.3).
  - **9.23.6 Sıra:** en yeni önce; sıralama seçeneği yoktur.
  - **9.23.7 Yoktur:** arama, gruplama ve dışa aktarma (`02 §10.3.4`) · satırın düzeltilmesi ve silinmesi — iz saklama süresi boyunca değişmez (`02 §10.3.2`, §8.5.2) · sistem olayları, giriş kaydı ve sistem kayıtları — barındırma tarafında okunur (`02 §10.3.3`, §8.5.1; K-687) · ihlal bildirimi ekranı ve formu (`02 §8.5.1`) · fiyat düzenlemesi, stok girişi ve içeriğin metin düzenlemesi satırları — ize yazılmazlar (`02 §10.3.1`) · siparişe süzülmüş iz — iz bu ekranda düz listedir (`02 §10.3.4`) · satırdan kayda geçiş (K-822).
- **Aksiyonlar**
  - **9.23.8 Süzmek** — liste ilk sayfasına döner; seçili süzgeçlerin adı listenin başında durur ve tek dokunuşla kaldırılır (§2.13.3). Onay ve mesaj yoktur.
- **9.23.9 Validasyonlar.** Tarih aralığının bitişi başlangıcından önce olamaz ve ileri tarih reddedilir — alan mesajı (OB-16). Alan envanteri §7.2'dedir (5. oturum).
- **9.23.10 Durum × rol varyantları.** İz bütün yöneticilere aynıdır (`02 §10.2.3`). Satırlar saklama süresinin sonunda kendiliğinden imha edilir ve listeden düşer (`03 §4.1.34`; Z-30). Bir ihlal şüphesinde yönetici izi tarih ve yönetici süzgeciyle okur — dışa aktarmalar dahil (`03 §6.3.2.2`, §8.8.6, §10.4.6); itirazda iz firmanın kanıtlarından biridir (`03 §5.1.7`). Tam matris §6.3'tedir (5. oturum).
- **9.23.11 Boş / yükleniyor / hata durumları.** Süzgecin sonucu boşsa boş hâl satırı süzgeci kaldırma yolunu taşır; kurulumdan sonra ilk işlemlere kadar liste boş hâl satırıyla açılır — ilk kaydı açan düğme yoktur (§2.7.1.2). Liste kendi yerinde yüklenir (§2.7.2); sayfa açılamazsa §2.7.3.2.
- **9.23.12 Responsive notları.** Orta sınıfta yöneticinin e-postası ve eski-yeni değer satırın altına iner; dar sınıfta kart listesidir — kart tarihi, yöneticiyi ve işlemi taşır, değerler kartın içinde alt alta durur (§2.13.4). Değerler satır kırar, kesilmez.

*Kaynak: `02 §8.5`, §10.2.2, §10.2.3, §10.3, §10.4.7 · `03 §4.1.34`, §5.1.7, §6.2.2.2, §6.2.6.1, §6.3.2.2, §6.3.2.3, §8.8.6, §8.9.4, §10.4.6 · `10 §2` KP-63 · K-687, K-822.*

### 9.24 E-53 — Dışa aktarma

- **9.24.1 Aktör · giriş · çıkış.** Yönetici. Giriş: menünün Raporlar › Dışa aktarma girişi (3.2.13). Çıkış: dosya tarayıcının indirmesiyle iner; ekran değişmez (§4.4.8).
- **9.24.2 İlk görülen:** üç dışa aktarma bölümü ve üstlerinde dosyanın sorumluluğunu söyleyen satır; birincil düğme siparişlerin "CSV olarak indir"idir (K-823).
- **Bilgi hiyerarşisi** — başta tek satır: her dışa aktarmanın işlem izine yazıldığı ve indirilen dosyadaki kişisel verinin sorumluluğunun firmaya geçtiği — metni K-823'tedir (`02 §10.7.1`, §12.5; K-823). Altında üç bölüm:
  - **9.24.3 (1) Siparişler** — tarih aralığı (OB-16; ileri tarih reddedilir) — aralık siparişin oluştuğu güne göredir (K-823) — ve "CSV olarak indir". Bölüm dosyanın taşıdığı alanları bir cümleyle sayar; listenin evi `02 §10.7.1`'dir.
  - **9.24.4 (2) Üye listesi** — üyelerin adı ve e-postası; tarih aralığı yoktur, dosya bütün üye hesaplarını taşır (`02 §10.7.4`; K-823). "CSV olarak indir".
  - **9.24.5 (3) İletişim talepleri** — talep sahiplerinin formdaki adı ve e-postası; tarih aralığı yoktur, dosya saklanan bütün talepleri taşır (`02 §10.7.4`; K-823). "CSV olarak indir".
  - **9.24.6 Yoktur:** katalog, kurumsal içerik, işlem izi ve satış özeti dışa aktarması — firmanın çıkış hakkı barındırma düzlemindedir (`02 §10.7.3`; `03 §8.9.7`) · ürün içe aktarma ve toplu güncelleme (`02 §10.7.2`) · zamanlanmış dışa aktarma ve dosyanın e-postayla gönderilmesi · fatura üretimi — fatura sistemin dışında kesilir, dosya fatura verisini taşır (`02 §3.25`) · ürünün toplu e-posta hattı — firma kişilere kendi aracıyla ulaşır (`02 §12.2.9`; `03 §6.3.3.2`).
- **Aksiyonlar**
  - **9.24.7 Dışa aktarmak** (`03 §8.9.5`, §8.9.6): onay istemez — kaynak dışa aktarmayı onay isteyen işlemler arasında saymaz (2.4.2); düğme olağandır (`02 §10.7.4`). Dosya hazırlanırken düğme ikinci kez basılamaz ve sürdüğünü söyler (§2.7.2). Her dışa aktarma işlem izine yazılır (`02 §10.3.1`). Seçilen aralıkta sipariş yoksa dosya üretilmez ve bölüm bunu yerinde kalıcı mesajla söyler (K-823).
- **9.24.8 Validasyonlar.** Sipariş aralığında iki tarih zorunludur; ileri tarih ve ters aralık alan mesajıyla reddedilir (OB-16). Alan envanteri §7.2'dedir (5. oturum).
- **9.24.9 Durum × rol varyantları.** Her yönetici dışa aktarır (`02 §10.2.3`). Bir ihlalde etkilenen kişilerin listesi bu üç dosyadan çıkarılır (`03 §6.3.3.1`; `02 §12.2.9`). Satış kapısı ekranı etkilemez. Tam matris §6.3'tedir (5. oturum).
- **9.24.10 Boş / yükleniyor / hata durumları.** İlk kurulumda üç bölüm de görünür; boş üye ve talep listesinde dosya üretilmez ve bölüm bunu söyler (K-823). Dosya beklenmeyen bir sebeple hazırlanamazsa mesaj bölümün içinde çıkar ve seçim korunur (§2.7.3.1); sayfa açılamazsa §2.7.3.2.
- **9.24.11 Responsive notları.** Bölümler her sınıfta alt alta; dar sınıfta tarih aralığının iki alanı alt alta durur. İndirme her sınıfta aynıdır.

*Kaynak: `02 §3.25`, §10.2.3, §10.3.1, §10.7, §12.2.9, §12.5 · `03 §6.3.3.1`, §6.3.3.2, §8.9.5–§8.9.7 · `10 §2` KP-52 · K-823 · devir: K-514 (ihlalde ulaşma listeleri).*

### 9.25 E-54 — Yönetici hesabının doğrulama ekranları

- **9.25.1 Aktör · giriş · çıkış.** Yönetici — yeniden doğrulamada · Kullanıcı — bağlantıyı açan, adresin sahibi. Kimlik üç adımı kapsar (konvansiyon 2; §4.3): yeniden doğrulama, yeni adresin doğrulama bağlantısının iniş ekranı, "bu değişikliği ben yapmadım" bağlantısının iniş ekranı. Giriş: E-50'nin şifre ve e-posta değiştirme işlemleri (3.4.16) · yeni adresin doğrulama bağlantısı (3.5.6) · geri alma bağlantısı (3.5.7). Çıkış: yeniden doğrulamadan sonra E-50, başlatılan işlemin formu (3.4.16) · iniş ekranlarından oturum açıksa E-50, değilse E-29 (§3.6.1) · geri almadan sonra şifre, eski adrese giden sıfırlama bağlantısıyla E-29'un yeni şifre adımında kurulur (3.5.5).
- **9.25.2 İlk görülen:** yeniden doğrulamada hangi işlem için doğrulandığı ve şifre alanı — birincil düğme doğrulamadır; iniş ekranlarında sonucu söyleyen tek cümle ve sonraki adımın bağlantısı.
- **Bilgi hiyerarşisi** — üç adım; düzen müşteri tarafındaki E-24, E-21 ve E-28'in kalıbıdır (§4.3; K-764) ve burada yalnız farklar yazılır (K-819):
  - **9.25.3 Yeniden doğrulama** — panel çerçevesinin içindedir (§2.2.9). E-24'ün kalıbıyla iki farkı vardır: başlatılan işlem iki tanedir — şifre değiştirme · e-posta değiştirme; hesap silme yoktur (`02 §10.2.2`) — ve doğrulama yalnız şifreyledir — panelde Google ile yeniden doğrulama yoktur (`02 §10.2.5`).
  - **9.25.4 Yeni adresin doğrulama bağlantısının iniş ekranı** — çerçevesizdir ve ürünün panel görünümünde bir başlık taşır (§2.2.9). E-21'in altı hâlinden yalnız e-posta değişikliğinin iki hâli vardır — kayıt hâlleri yoktur, yönetici hesabı davetle doğar (`02 §10.2.1`): **yeni adres doğrulandı** — değişikliğin geçerli olduğu, hesabın bundan sonra yeni adresle çalıştığı ve önceki adrese bildirim gittiği; misafir siparişlerinin bağlanması yoktur (5.21.7'nin farkı) · **bağlantı geçersiz** — süresi dolmuş, yerine yeni bir istek geçmiş ya da kullanılmış; değişikliğin bu bağlantıyla geçerli olmadığı ve E-50'den yeniden başlatılacağı (5.21.8'in kalıbı; K-780). Sonraki adım: oturum açıksa "Hesabım" (E-50), değilse panel girişi (E-29).
  - **9.25.5 Geri alma bağlantısının iniş ekranı** — çerçevesizdir. E-28'in iki hâli (K-782) şu farklarla işler (K-819): **geri alındı** — hesabın e-postasının önceki adrese döndüğü, bütün oturumlarının kapandığı, tanınan tarayıcı işaretlerinin düştüğü, şifrenin geçersizleştiği ve önceki adrese şifre sıfırlama bağlantısı gönderildiği; Google girişinin kaldırılması ve siparişlerin hesapta kalması satırları yoktur — yönetici hesabında ikisi de bulunmaz (`02 §10.2.5`; `03 §9.3.6`, §9.3.7) · **bağlantı geçersiz** — süresi dolmuş ya da daha önce kullanılmış: ürünün içinden geri alma yolu kalmadığı; yolun panele erişimi olan başka bir yönetici, hiçbir yönetici giremiyorsa kurulumu yapan olduğu (`02 §10.2.7`; `03 §10.4.3`, §10.4.4) — müşterinin İletişim sayfası yönlendirmesi yoktur (5.28.4'ün farkı).
  - **9.25.6 Yoktur:** iki adımlı doğrulama (`02 §10.2.4`) · iniş ekranlarında şifre alanı — şifre e-postadaki sıfırlama bağlantısıyla E-29'da kurulur (5.28.5'in kalıbı) · ikinci bir onay adımı — bağlantının açılması geri almadır (`03 §9.3.6`).
- **Aksiyonlar**
  - **9.25.7 Doğrulamak** (`03 §9.2.6`; `02 §10.2.4`): şifreyle; başarıda E-50'ye, başlatılan işlemin formuna döner. Onay istemez. Yeniden doğrulama yalnız başlatılan işlem içindir (5.24.4).
  - **9.25.8 İniş ekranlarında** yalnız sonraki adımın bağlantısı vardır; formları ve onayları yoktur. Bağlantının hâlinin tespiti Teknik Mimari'nin işidir (5.21.9).
- **9.25.9 Validasyonlar.** Şifre zorunludur. Yanlış şifre alan mesajıdır ve L-1'e sayılır; eşik aşılınca nötr limit mesajı çıkar ve işlem yapılmaz — panel girişine muafiyet yoktur (5.24.6'nın kalıbı; `03 §6.1.1.1`; §2.3.5).
- **9.25.10 Durum × rol varyantları.** Yeniden doğrulama oturum ister; iniş ekranları oturumdan bağımsızdır ve yalnız sonraki adımları oturuma göre değişir. Geri alma, açık olan oturumu da kapatır. Satış kapısı ekranları etkilemez. Tam matris §6.3'tedir (5. oturum).
- **9.25.11 Boş / yükleniyor / hata durumları.** Ekranların boş hâli yoktur; tanınmayan bağlantı "bağlantı geçersiz" hâlini gösterir. Doğrulama sürerken düğme ikinci kez basılamaz (§2.7.2); oturum yeniden doğrulamada kapanırsa ekran bunu söyler ve panel girişine götürür (§3.6.5). Geri almanın sonucu belirsiz kalırsa ekran "işlem yapılmadı" demez — bağlantı yeniden açılınca sonucu söyler (§2.7.3.3). Sayfa açılamazsa §2.7.3.2.
- **9.25.12 Responsive notları.** Üç adım da tek sütundur; sınıflar arasında fark yoktur.

*Kaynak: `02 §3.13.13`, §3.13.14, §10.2.1, §10.2.2, §10.2.4, §10.2.5, §10.2.7 · `03 §1.10.5`, §6.1.1.1, §7.1.51, §7.1.52, §7.3.43, §9.2.6, §9.3.4–§9.3.7, §10.4.3, §10.4.4 · K-764, K-779, K-780, K-782, K-819.*

---

*Shopfolio — UI Specifications v0.11*
