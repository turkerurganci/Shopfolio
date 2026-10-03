# Cross-Review — 02 Product Requirements (Tur 9)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/02_PRODUCT_REQUIREMENTS.md` v0.23 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki turlar ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde; satır sonlarındaki boşluklar atıldı). İstem önceki turlarınkiyle aynıdır: yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi.

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §1.2 Terim sözlüğü — Yayında
> Alıntı: "Bir ürünün 'Yayında' olabilmesi için arşivlenmemiş en az bir varyantı olmalıdır"
> Sorun: Bu ifade Taslak varyantı da kapsar; oysa §3.7.3 ve §5.1, ürünün yayına alınması için en az bir Yayında varyant şartı koyar. Yayın kapısı iki farklı yorum üretir.
> Öneri: Tanımı “en az bir Yayında varyantı olmalıdır” diye değiştirin; “arşivlenmemiş” ifadesini kaldırın.
>
> BULGU-2
> Kriter: Kullanıcı deneyimi
> Seviye: Yüksek
> Yer: §6.2.13 Satın alma — Misafir alıcı e-posta adresini yanlış yazar
> Alıntı: "e-posta ulaşmaz ama hiçbir akış e-postaya bağlı değildir"
> Sorun: Misafir sipariş takibi §3.22.3 uyarınca sipariş numarası + e-posta ister; e-posta bağlantısı da yanlış adrese gider. Yanlış e-posta yazan gerçek alıcı sipariş sayfasına erişemez; takip, iptal, cayma ve ayıp talebi yollarını kullanamaz. Siparişin donmuş iletişim e-postasını düzeltme veya güvenli erişim kurtarma akışı da tanımlı değildir.
> Öneri: Onay ekranında gösterilen tek kullanımlık bir sipariş kurtarma anahtarıyla iletişim e-postasını güvenli biçimde güncelleme ve erişimi yeniden kurma kuralını ekleyin; eski e-posta bağlantısını geçersizleştirin ve işlemi kayda alın.
>
> BULGU-3
> Kriter: Eksiklik
> Seviye: Orta
> Yer: §3.27.13 Kurumsal içerik
> Alıntı: "her tipte ad ya da başlık"
> Sorun: Hakkımızda (§3.27.3) için ad/başlık alanı, Duyuru (§3.27.21) için de ad/başlık alanı tanımlanmamıştır. Buna rağmen §3.27.24 ve §5.2 yayına alma koşulunu bu alana bağlar. Bu iki içerik türünün hangi koşulda yayına alınacağı belirsizdir.
> Öneri: İçerik türü bazında açık yayın kapısı tablosu ekleyin; örneğin Hakkımızda için kısa tanıtım, Duyuru için metin zorunluluğunu tanımlayın ve genel “ad ya da başlık” kuralını buna göre daraltın.
>
> BULGU-4
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §10.5 Manuel adımlar ve bütçe
> Alıntı: "havale ile ödenen fiziksel + hizmet siparişi dört adımdır"
> Sorun: §10.5.1 karışık siparişte dört zorunlu manuel adım olduğunu söylerken §10.5.4 “Bir siparişin hattında ... üçü geçmez” kuralını koyar. Sonraki cümle bütçeyi tek tipli sipariş hattıyla sınırlar; ancak karışık siparişin bütçe ve kapasite hesabı net değildir.
> Öneri: Kuralı açıkça “teslim hattı başına en fazla üç adım” olarak yeniden yazın; karışık siparişler için toplam adım sınırını veya açık istisnayı ve kapasite hesabını ayrıca tanımlayın.
>
> SONUÇ: 4 BULGU
> ```

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ✅ KABUL | Doğru. K-538 ürünün yayın kapısını "en az bir Yayında varyant" diye okudu: taslak varyant ziyaretçiye görünmez ve koşulu karşılamaz. §3.7.3 ve §5.1 bu okunuşla yazıldı. Sözlükteki "Yayında" satırı ise K-55'in eski lafzını, *"arşivlenmemiş en az bir varyant"*ı taşıyordu. Bu lafız bütün varyantları taslak olan bir ürünü de yayına açıyordu. K-538 o güncellemede sözlüğü saymamıştı; satır geride kalmıştı. | Karar kaydı gerekmedi; K-538'in okunuşu sözlüğe taşındı. §1.2 "Yayında" satırı: *"en az bir Yayında varyantı olmalıdır — taslak varyant ziyaretçiye görünmediği için koşulu karşılamaz (K-54, K-55, K-538)"*. Satırın kurumsal içerik yarısı §3.27.13'e atıf aldı (BULGU-3). |
| BULGU-2 | ⚠️ KISMİ — **uygulanmadı, proje sahibine soruldu** | Sorun gerçek ve §6.2.13'ün cümlesi yanlış. Misafirin sipariş sayfasına iki yolu var: numara + e-posta ve e-postadaki bağlantı (§3.22.3, K-187). İkisi de siparişe donmuş adresten geçiyor. Adres onaydan sonra yanlış çıkarsa müşteri sipariş sayfasına hiç giremiyor. K-193'ün kendi gerekçesi de bunu söylüyor: *"yanlış adres müşteriyi siparişine **hiç** erişemez hâle getirir"*. K-193 bu riski doğrulama koduyla değil, onay özetinde adresi göstererek azaltmayı seçti. Onaydan sonra fark edilen hata için bir yol tanımlamadı. Teslimat etkilenmiyor, çünkü donmuş adrese yapılıyor. Firma da bazı durumlarda panelde "e-posta ulaşmadı" işaretini görüyor (K-321). Ama havale hattında bir boşluk açılıyor. Geri ödemenin IBAN'ı yalnız sipariş sayfasından giriliyor (K-498, K-575). Firmanın IBAN'ı telefonla ya da e-postayla alıp girmesi K-581 ile elendi. Bu yüzden yanlış e-posta yazmış misafir, iptalde ya da IBAN'sız bir caymada geri ödemesini alamıyor; yasal on dört günlük süre ise işlemeye devam ediyor. Codex'in önerdiği kurtarma anahtarı seçeneklerden biri ama tek seçenek değil. Seçenekler güvenlik ve para riskinde gerçekten ayrışıyor: firmanın e-postayı düzeltmesine izin vermek, kimliği teyit edilmemiş birinin siparişe ve IBAN alanına erişmesini mümkün kılıyor. Bu yüzden karar proje sahibine bırakıldı (§4). | Bu turda yok. §6.2.13'ün cümlesi de değiştirilmedi; doğru metin seçilecek yola bağlı. |
| BULGU-3 | ✅ KABUL | Doğru. K-250 ve K-275 yayın kapısını *"her tipte ad ya da başlık"* diye yazdı. Hakkımızda'nın alanları ise kısa tanıtım, uzun metin ve görseller (K-239); duyurunun alanları metin, bağlantı ve tarihler (K-269). İkisinde de ad ya da başlık alanı yok. Duyuru K-502 ile kurumsal içerik kaydı oldu ve kapıya girdi, ama kapının istediği alan duyuruda yoktu. Önerinin içeriği de doğru. Hakkımızda'da kısa tanıtım gerekli, çünkü ana sayfanın kurumsal bloğu onu gösteriyor. Kısa tanıtımı boş bir Hakkımızda yayına alınabilseydi blok boş kalırdı ve K-250'nin *"boş kutu görünmez"* kuralı delinirdi. Duyuruda kapı metin, çünkü şerit tek satırlık bir metinden ibaret. Ayrı bir tablo açılmadı; kural §3.27.13'te tipe göre sayıldı. | K-584 `(öneriyle kaydedildi)`: zorunlu alanlar tipe göre yazıldı. Hizmet tanıtımında ad; referans işte ve genel sayfada başlık; SSS'de soru ve cevap; şubede ad ve adres; Hakkımızda'da kısa tanıtım; duyuruda metin. Hakkımızda ve duyuru tekil oldukları için boş da taslak kaydedilir. Hakkımızda görselinin alternatif metni boşsa tipin adı kullanılır. Yayındaki Hakkımızda'nın kısa tanıtımını silen düzenleme K-538'in kalıbıyla kaydedilmez. Güncellenen yerler: §1.2 (Yayında, Hakkımızda, Duyuru), §3.27.13, §3.27.15, §3.27.24, §5.2. Karar kaydında K-250 ve K-275'in etki sütunlarına K-584 eklendi. |
| BULGU-4 | ❌ RET | Önerilen kural dokümanda zaten var ve bilinçli bir karar. K-454 bütçeyi hat başına okudu. Karışık siparişte her hat kendi adımını taşıyor ve adımlar toplanıyor. Elenen seçenekler de kayıtta: karışık siparişi yasaklamak (K-81 ile çelişir) ve hizmetin tamamlanmasını kendiliğinden yapmak (K-175, K-289, K-290). §10.5.1'in son cümlesi bunu yazıyor: *"Bütçe (§10.5.4) hat başına okunur"*. §10.5.4'ün ikinci cümlesi de: *"Bütçe tek tipli bir siparişin hattına uygulanır"*. Kapasite hesabı bir sınır değil, tasarım varsayımı (§10.5.5, K-403). Karışık siparişin dört adımı K-454'e göre *"iki işi birden yaptırır ve adım sayısı bunu dürüstçe gösterir"*. Konu önceki turlarda gelmedi. Bulgunun kaynağı §10.5.4'ün başlığıydı: *"Bir siparişin hattında"* tek başına okununca sipariş düzeyinde bir sınır gibi duruyordu. `01` Ü-3 aynı kuralı *"Bütçe hat başınadır"* diye açık yazıyor. | Karar kaydı gerekmedi. §10.5.4'ün başlığı `01` Ü-3'e hizalandı: *"Bütçe hat başınadır: tek tipli bir siparişin hattında sistem içi zorunlu elle adım üçü geçmez."* |

**Dağılım:** 2 KABUL · 1 KISMİ · 1 RET.

İki KABUL'ün ikisi de daha önce alınmış bir kararın her yere taşınmamasından doğdu. BULGU-1'de K-538'in okunuşu sözlüğe geçmemişti. BULGU-3'te K-502 duyuruyu kurumsal içerik yaptı, ama K-250'nin kapısı duyurunun alanlarına göre yeniden yazılmadı. RET verilen BULGU-4 doğru bir kurala yöneldi ama belirsiz bir başlıktan beslendi; R6 BULGU-1, R7 BULGU-2, BULGU-4 ve R8 BULGU-2'de olduğu gibi kural korundu, başlık netleştirildi. KISMİ verilen BULGU-2 bu turun tek yeni ürün boşluğu.

## 3. Ek bulgular

- **8. turun sürüm notu (kendi kontrolüm):** v0.23 notu e-posta ilkesinin değiştiği yerleri *"§2.2, §3.22.4, §6.1.1, §9.1.2"* diye sayıyordu. R8 raporu ve commit `05266f1` §2.2'nin değişmediğini gösteriyor. Not düzeltildi: *"§3.22.4, §6.1.1, §9.1.2"*.
- **BULGU-2'nin ikinci bir ayağı:** §6.1.1 ve §6.3.1 de *"misafir alıcı sipariş sayfasına sipariş numarası + e-postayla girer"* diyor. Bu cümleler doğru adres için doğru, yanlış adres için değil. Proje sahibinin kararından sonra §6.2.13 ile birlikte bu iki satır da gözden geçirilmeli.
- **Etki yansıtma için not:** K-584 `04`'ü (Hakkımızda ve duyuru formu, eksik alan gösterimi) ve `10 §2`'yi ilgilendiriyor. `01` ve `10`'da "ad ya da başlık" kapısı ve "arşivlenmemiş en az bir varyant" lafzı geçmiyor (`grep` ile kontrol edildi). §10.5.4'ün yeni başlığı `01` Ü-3'ün cümlesiyle aynı. Bu turda `01` ve `10`'a dokunulmadı.

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`02` v0.24). Bir karar satırı açıldı:

- K-584 `(öneriyle kaydedildi)`. Kişisel veriye, paraya ya da önceki bir karardan ayrılmaya dokunmuyor; K-250 ve K-275'i tamamlıyor, ⚠ taşımıyor.

**Proje sahibine sorulan (BULGU-2 — uygulanmadı):** Misafir alıcı e-posta adresini yanlış yazıp onayladıysa sipariş sayfasına hiç giremiyor. Havale ile ödediyse iptalde ve caymada geri ödeme için IBAN giremiyor, ama yasal süre işliyor. Seçenekler:

- **(A) Firma misafir siparişinin iletişim e-postasını panelden düzeltir (dokuzuncu müdahale).** Müşteri telefonla ya da iletişim formuyla firmaya ulaşır. Firma kimliği siparişteki bilgilerle (teslimat telefonu, kalemler, tutar) kendisi teyit eder. Yeni adrese yeni bir erişim anahtarıyla sipariş e-postası gider, eski anahtar geçersizleşir, eski adrese bir bildirim gider ve müdahale işlem izine eski ve yeni değerle yazılır. Kalan risk: firmayı kandıran biri siparişe ve IBAN alanına erişebilir. **Yönetici önerisi budur:** küçük bir firmanın gerçek hayattaki yolu bu, yapı da mevcut müdahale kalıbına (§10.4) oturuyor.
- **(B) Onay ekranında tek kullanımlık bir kurtarma kodu gösterilir (Codex'in önerisi).** Müşteri bu kodla e-postayı kendisi düzeltir. Firmaya iş düşmez ve kimlik sahtekârlığı riski yoktur, ama kodu saklamayan müşteri yine kapıda kalır ve ödeme adımına yeni bir öğe eklenir.
- **(C) Kurtarma yolu açılmaz, kalan risk olarak yazılır.** §6.2.13'ün cümlesi düzeltilir ve müşteri firmaya başka kanaldan ulaşır. Havale hattında geri ödemenin IBAN'ı için K-581'in "firma IBAN'ı telefonla almaz" kuralına dar bir istisna yazılması gerekir; yoksa geri ödeme yapılamaz.

- [x] BULGU-1 · [ ] BULGU-2 (proje sahibinin kararını bekliyor) · [x] BULGU-3 · [x] BULGU-4

**Hukuki kontrol:**

- **BULGU-1, BULGU-3, BULGU-4:** Hukuki iddia taşımıyor; dokümanın kendi kurallarına ve karar kaydına (K-538, K-239, K-250, K-269, K-275, K-502, K-454) karşı değerlendirildi.
- **BULGU-2:** Bulgu hukuki bir iddia taşımıyor. Değerlendirmedeki on dört günlük geri ödeme süresi dokümanın zaten dayandığı Mesafeli Sözleşmeler Yönetmeliği m.12'dir (K-491, 5. ve 7. turlarda Resmî Gazete metnine karşı doğrulandı); bu turda yeni bir hukuki iddia yazılmadı.
