# Cross-Review — 02 Product Requirements (Tur 10)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/02_PRODUCT_REQUIREMENTS.md` v0.25 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki turlar ve raporlar verilmedi (K-431)

> **Bu turdan önce:** 9. turun BULGU-2'si — misafirin onaydan sonra fark ettiği yanlış e-posta — proje sahibinin talimatıyla yönetici kararıyla (seçenek A) K-585 olarak kayda geçti ve `02` v0.25'te ayrı bir commit'le işlendi. Bu tur v0.25'i okudu.

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde; satır sonlarındaki boşluklar atıldı). İstem önceki turlarınkiyle aynıdır: yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi.

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §10.3.1 İşlem izi
> Alıntı: "satır eski ve yeni adresi taşır"
> Sorun: Misafir siparişinin e-posta düzeltmesinde işlem izi iki e-posta adresini 10 yıl saklar; oysa ödemesi hiç alınmamış siparişlerin kişisel verisi 3 yıl sonra imha edilmelidir (Z-41). Ayrıca §12.2.7, sipariş verisi imha edilince işlem izinde kişisel veri kalmadığını söyler. Bu istisna iki kuralla çelişir.
> Öneri: E-posta değerlerini işlem izinden çıkarıp yalnız değişiklik olayını ve teknik kimliği saklayın; değer zorunluysa ayrı, erişimi kısıtlı kayıtta tutun ve bağlı siparişin kişisel veri saklama süresinde imha edin.
>
> BULGU-2
> Kriter: Edge case
> Seviye: Yüksek
> Yer: §3.31 Panelde eşzamanlı düzenleme
> Alıntı: "Kural kurumsal içerik, ürün ve ayarlar için aynıdır"
> Sorun: Eşzamanlılık koruması sipariş müdahalelerini kapsamaz. İki yönetici aynı anda geri ödeme, havale “ödendi” işareti, kalem çıkarma veya teslim işareti koyabilir; bu, çift geri ödeme ya da çelişkili sipariş sonucu doğurabilir.
> Öneri: Sipariş durum değişiklikleri ve para etkili müdahaleler için de yalnız ilk geçerli işlemin uygulanacağı, diğer yöneticinin güncel sipariş durumunu görüp işlemi yeniden değerlendireceği kuralını ekleyin.
>
> BULGU-3
> Kriter: Belirsizlik
> Seviye: Orta
> Yer: §10.2 Yönetici hesapları
> Alıntı: "yönetici hesabıyla vitrinde alışveriş yapılmaz — kendi mağazasından alacak yönetici ayrı bir müşteri hesabı açar"
> Sorun: Aynı e-posta adresinin hem müşteri hem yönetici hesabında kullanılabilip kullanılamayacağı, kullanılabiliyorsa giriş ve Google ile bağlama davranışının ne olacağı tanımlı değildir. Bu durum davet kabulünde hesap çakışmasına veya yanlış hesap/rol oturumu açılmasına yol açabilir.
> Öneri: E-posta adresinin tüm hesap türlerinde tekil mi, yoksa rol bazında ayrı mı olduğunu; davet edilen adres mevcut müşteri hesabına aitse sistemin reddetme, ayrı hesap oluşturma veya mevcut hesabı role yükseltme davranışını açıkça belirleyin.
>
> SONUÇ: 3 BULGU
> ```

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ✅ KABUL | Doğru, ve hatayı bu turdan önceki K-585 commit'i açtı. K-585 düzeltmeyi *"eski ve yeni değerle"* ize yazdı ve bunu K-527'nin kuralına dar istisna saydı. Ama iz on yıl saklanır ve değiştirilemez (Z-30, §10.3.2). Ödemesi alınmamış siparişin kişisel verisi ise üç yıl sonra silinir (Z-41). Ödenmiş siparişte de veri izden önce biter: Z-28 siparişin yılından, Z-30 satırın yılından işler. §12.2.7'nin son cümlesi bu yüzden yanlış kalmıştı: *"sipariş satırları müşterinin kişisel verisini değer olarak taşımadığı için siparişin kişisel verisi imha edildiğinde izde kalmaz"*. İki e-posta adresi amacından uzun saklanıyordu (KVKK m.4/2-d, 6698 sayılı Kanun metnine karşı kontrol edildi). Codex'in önerisi doğru yerde: değeri izden çıkar, ayrı bir kayıtta siparişle birlikte imha et. Kanıt ihtiyacı siparişin ömrüyle sınırlı, çünkü sahtekârlık iddiası geri ödeme ve cayma süreleri içinde doğar. Proje sahibinin tercihinin özü korunuyor: eski ve yeni değer yine kayda geçiyor, yalnız yeri değişiyor. | K-586 `(öneriyle kaydedildi — ⚠)`, K-585'in (6). sonucunu düzeltir. İz satırı §10.3.1'in kuralıyla yalnız işlemi adlandırır. Eski ve yeni adres siparişin kendi kaydında "e-posta geçmişi" olarak durur: düzeltmeyi yapan yönetici ve tarihle, panelde görünür, müşteriye görünmez ve siparişin kişisel verileriyle imha edilir (Z-28, Z-41). Güncellenen yerler: §6.3.5, §8.3.6, §10.3.1, §10.4.11, §12.2.7, §12.5. |
| BULGU-2 | ✅ KABUL | Doğru. K-270 sessiz üzerine yazmayı yalnız *"kurumsal içerik, ürün ve ayarlar"* için yasakladı (§3.31.1); sipariş işlemleri sayılmadı. Bütün yöneticiler aynı yetkide (§10.2.3) ve aynı siparişte aynı anda çalışabilir. Siparişte çakışmanın sonucu içerik kaybı değil, para ve durum: aynı geri ödeme iki kez kaydedilebilir, kargoya verme ile iptal çakışabilir. Codex'in önerisi K-270'in kalıbıyla aynı. Bir adım genişletildi, çünkü aynı yarış yöneticiyle müşterinin (iptal, cayma, IBAN) ve yöneticiyle sistemin (ödeme süresi, sağlayıcı bildirimi) arasında da var. Konu önceki turlarda gelmedi. | K-587 `(öneriyle kaydedildi — ⚠)`. Yeni §3.31.2: siparişe yönelik her işlem siparişin güncel hâline karşı uygulanır, önce tamamlanan geçerli olur, sonraki sipariş arada değiştiyse uygulanmaz ve panel güncel hâli gösterir. Sipariş kilitlenmez (K-270'in elediği yol). Kalan risk yazıldı: havale geri ödemesinin parası ürün dışından gider, kural yalnız panelde ikinci kaydı engeller. §10.1.4'e atıf ve yeni satır 6.7.15 eklendi. |
| BULGU-3 | ✅ KABUL | Doğru. K-312 yönetici ile müşteri hesabını ayırdı ve *"iki hesap birbirine bağlanmaz"* dedi. Ama aynı adresin iki türde kullanılıp kullanılamayacağı ve hangi girişin hangi hesabı aradığı yazılmamış. Firma sahibi kendi mağazasından alışverişte çoğu zaman aynı adresi kullanır. Davette, girişte, Google bağlamasında (§3.13.7) ve misafir siparişinin hesaba düşmesinde (§3.13.3) davranış tanımsızdı. Codex üç seçenek saydı (reddet, ayrı hesap aç, rolü yükselt). "Ayrı hesap" seçildi, çünkü K-312'nin kararıyla tek uyumlu seçenek bu. "Rolü yükselt" K-312'yi tersine çevirirdi. "Tüm hesaplarda tekil e-posta" ise firma sahibine ikinci bir adres açtırır ve kayıt ekranında yönetici adreslerini ele verir (K-107). | K-588 `(öneriyle kaydedildi)`. Hesap türleri ayrı kapılardan girilir. E-posta kendi türü içinde tekildir, iki tür arasında aynı adres kullanılabilir. Vitrinin kayıt, giriş, şifre sıfırlama, Google bağlaması ve misafir siparişi bağlaması yalnız müşteri hesaplarında işler. Davet, adres bir müşteri hesabına ait olsa da ayrı bir yönetici hesabı açar. Panelde Google ile giriş yok. K-313 yönetici rejimine Google girişini zaten taşımamıştı; burada yalnız kaydın okunuşu yazıldı. Güncellenen yerler: §3.13.7, §8.1.7, §10.2.5. |

**Dağılım:** 3 KABUL · 0 KISMİ · 0 RET.

%100 KABUL şüphe gerektirir; her bulgu karar kaydına karşı ayrı ayrı sınandı. Üçü de bilinçli bir kararın elediği bir seçeneğe yönelmiyor. Her biri bir kararın kapsamında yazılmamış bir parçayı gösteriyor:

- **BULGU-1:** K-585'in kendi istisnası §12.2.7 ve Z-41 ile çelişiyordu. Bulgu yeni karara, kaydın sorduğu yerden itiraz ediyor.
- **BULGU-2:** K-270'in kapsam listesinde sipariş yoktu.
- **BULGU-3:** K-312'nin ayrımı e-posta ve giriş düzeyine inmemişti.

Öneriler de kayıtla uyumlu çıktı. BULGU-3'te Codex seçenek sundu; seçim K-312'den okundu.

## 3. Ek bulgular

- **K-585'in ön adımı (kendi kontrolüm):** BULGU-1'in kaynağı bu turdan önce yazılan karar satırı. Yöneticinin talimatındaki *"eski ve yeni değerle işlem izine yazılır"* ifadesi harfiyen uygulanmış, ama §12.2.7'nin son cümlesi ve Z-41 ile çelişkisi o adımda taranmamıştı. K-586 bu çelişkiyi kapattı. K-585'in karar satırı değiştirilmedi; etki sütunu K-586'ya atıf alıyor. Proje sahibinin gözden geçirme listesinde iki satır birlikte okunmalı.
- **Etki yansıtma için not:** `10 §2`'de üç satır hizalanacak. KP-48 müdahale listesine dokuzuncu müdahaleyi (K-585), KP-66 müşteriye giden olayları on dörtten on beşe (B-15) çıkaracak. KP-76'nın eşzamanlı düzenleme satırı sipariş işlemlerine genişleyecek (K-587). `01`'de eşzamanlılık ve hesap türü cümlesi geçmiyor (`grep` ile kontrol edildi). Bu turda `01` ve `10`'a dokunulmadı.

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`02` v0.26). Üç karar satırı açıldı:

- K-586 `(öneriyle kaydedildi — ⚠)`: kişisel veriye ve kısa süre önce alınmış bir karara (K-585) dokunuyor.
- K-587 `(öneriyle kaydedildi — ⚠)`: paraya ve sipariş durumuna dokunuyor; K-270'i genişletiyor.
- K-588 `(öneriyle kaydedildi)`: K-312'yi tamamlıyor; önceki bir karardan ayrılmıyor, ⚠ taşımıyor.

- [x] BULGU-1 · [x] BULGU-2 · [x] BULGU-3

**Hukuki kontrol:**

- **BULGU-1:** Saklama süresinin amaçla sınırlı olması KVKK m.4/2-d'dir: *"İlgili mevzuatta öngörülen veya işlendikleri amaç için gerekli olan süre kadar muhafaza edilme."* 6698 sayılı Kanun'un metnine karşı doğrulandı. Bulgunun kendisi dokümanın iç süreleriyle (Z-28, Z-30, Z-41) ve §12.2.7 ile karşılaştırıldı.
- **BULGU-2, BULGU-3:** Hukuki iddia taşımıyor; dokümanın kendi kurallarına ve karar kaydına (K-06, K-270, K-308, K-312, K-313, K-104, K-107) karşı değerlendirildi.
