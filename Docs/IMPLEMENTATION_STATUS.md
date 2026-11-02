# Shopfolio — Implementation Status

**Son güncelleme:** 2026-11-02 — Implementation dönemi açıldı: tracker `11`'in faz ve task listesiyle dolduruldu (7 faz, 214 task, hepsi ⬚); sıradaki iş `/task T02` (önce D-01 kapısı, `Docs/PRODUCT_DISCOVERY_STATUS.md` §25).

> ⚠️ **Bu dosya task durumu için TEK OTORİTER KAYNAKTIR.** Hafıza snapshot'ı bilgilendiricidir, otoriter değildir.
> ⚠️ **"Son güncelleme" bloğu prepend-only changelog'a DÖNÜŞTÜRÜLMEZ.** Yalnız en son giriş burada durur; tarihsel geçmiş [`STATUS_CHANGELOG.md`](STATUS_CHANGELOG.md)'de.
> **Neden:** Bir referans projede bu alan 63 KB'lık tek satıra büyüdü ve okuma araçları tarafından kırpıldı — "nerede kaldık" sorusu cevaplanamaz hâle geldi.
>
> **Okuma tavsiyesi:** Tek bir task'ın durumu için tüm dosyayı açma; ilgili satırı `grep`'le (`grep -n "^| T42 " Docs/IMPLEMENTATION_STATUS.md`).

---

## Durum lejandı

| Simge | Durum | Açıklama |
|---|---|---|
| ⬚ | Bekliyor | Henüz başlanmadı |
| ⏳ | Devam ediyor | Yapım chat'inde aktif |
| ✓ | Tamamlandı | Doğrulama PASS, merge edildi |
| ✗ | FAIL | Doğrulama başarısız |
| ⛔ | BLOCKED | İlerleyemiyor (alt tür belirtilir) |

**Doğrulama durumları:** ✓ PASS / ✗ FAIL / ⛔ BLOCKED
**Detaylı raporlar:** `Docs/TASK_REPORTS/TXX_REPORT.md`

---

## Faz gate durumu

| Faz | Ad | Task aralığı | Gate | Tag | Rapor |
|---|---|---|---|---|---|
| F0 | Proje iskeleti | `T01`–`T10` (10) | ⬚ | — | |
| F1 | Veri katmanı | `T101`–`T120` (20) | ⬚ | — | |
| F2 | Çekirdek servisler | `T201`–`T233` (33) | ⬚ | — | |
| F3 | İş mantığı | `T301`–`T390` (90) | ⬚ | — | |
| F4 | Entegrasyonlar | `T401`–`T410` (10) | ⬚ | — | |
| F5 | Kullanıcı arayüzü | `T501`–`T549` (49) | ⬚ | — | |
| F6 | Uçtan uca doğrulama | `T601`–`T602` (2) | ⬚ | — | |

---

## Açık bulgular (cross-task)

> Bir task'ta ortaya çıkıp başka bir task/faz tarafından kapatılacak bulgular.
> **Not:** "Şimdi değil" kararıyla ertelenen işler buraya değil, [`DEFERRED_BACKLOG.md`](DEFERRED_BACKLOG.md)'ye yazılır.

| # | Kaynak | Bulgu | Çözüm yetkisi / tetikleyici |
|---|---|---|---|

**Kapatılanlar:**
- *(henüz yok)*

---

## F0 — Proje iskeleti

| Task | Ad | Durum | Doğrulama | Commit | PR |
|---|---|---|---|---|---|
| T01 | Canlılık ucu | ⬚ | — | — | — |
| T02 | Çözüm ve proje düzeni | ⬚ | — | — | — |
| T03 | Test altyapısı | ⬚ | — | — | — |
| T04 | CI hattı ve eşitlik denetimleri | ⬚ | — | — | — |
| T05 | Compose geliştirme ortamı | ⬚ | — | — | — |
| T06 | Üç imaj ve kayıt defteri | ⬚ | — | — | — |
| T07 | Yedek düzeni | ⬚ | — | — | — |
| T08 | Sır düzeni | ⬚ | — | — | — |
| T09 | İlk yayın ve referans kuruluma ilk iniş | ⬚ | — | — | — |
| T10 | Veritabanı kullanıcıları | ⬚ | — | — | — |

---

## F1 — Veri katmanı

| Task | Ad | Durum | Doğrulama | Commit | PR |
|---|---|---|---|---|---|
| T101 | Veri modeli temeli | ⬚ | — | — | — |
| T102 | Enum tanımları ve değer dönüşümü | ⬚ | — | — | — |
| T103 | Başlangıç listeleri, firma ve yasal metin sürümü | ⬚ | — | — | — |
| T104 | Ürün çekirdeği ve görsel | ⬚ | — | — | — |
| T105 | Fiyat, kupon ve dijital dosya | ⬚ | — | — | — |
| T106 | Yönetici hesabı, davet ve işlem izi | ⬚ | — | — | — |
| T107 | Müşteri hesabı | ⬚ | — | — | — |
| T108 | Oturum, bağlantı, deneme ve giriş kayıtları | ⬚ | — | — | — |
| T109 | Sepet ve sipariş çekirdeği | ⬚ | — | — | — |
| T110 | Ayırma ve düzeltme geçmişleri | ⬚ | — | — | — |
| T111 | Kalem kayıtları | ⬚ | — | — | — |
| T112 | Ödeme ve geri ödeme kayıtları | ⬚ | — | — | — |
| T113 | İade reddi ve IBAN isteği | ⬚ | — | — | — |
| T114 | İçerik kayıtları ve marka | ⬚ | — | — | — |
| T115 | Şube, içerik–ürün bağları ve iletişim talebi | ⬚ | — | — | — |
| T116 | Galeri bağları | ⬚ | — | — | — |
| T117 | Gönderim kaydı ve kanal hâli | ⬚ | — | — | — |
| T118 | Zamanlayıcı, yedek, imha ve anahtar kayıtları | ⬚ | — | — | — |
| T119 | Başlangıç verisi göçü ve seed olmayanların sınırı | ⬚ | — | — | — |
| T120 | Model eşitlik denetimleri | ⬚ | — | — | — |

---

## F2 — Çekirdek servisler

| Task | Ad | Durum | Doğrulama | Commit | PR |
|---|---|---|---|---|---|
| T201 | İmha düzenleyicisi, süre ayarları ve gecelik temizlik | ⬚ | — | — | — |
| T202 | Dış servislerin bağlantı katmanı: ortak yüz, hata sınıfı, zaman aşımı, sahteler | ⬚ | — | — | — |
| T203 | İmzalı makine ucu altyapısı: imza denetimi, gövde tavanı | ⬚ | — | — | — |
| T204 | Dış servis çökmesi davranışı ve sahtelerle sınama | ⬚ | — | — | — |
| T205 | Açılışta veritabanı fail-fast denetimi | ⬚ | — | — | — |
| T206 | Adres yapısı, önekler ve sayfa–form işleyici kalıbı | ⬚ | — | — | — |
| T207 | Kimlik şemaları ve erişim reddi | ⬚ | — | — | — |
| T208 | Sürümün ve gösterilen değerin formda taşınması, koşullu güncelleme | ⬚ | — | — | — |
| T209 | Tek kullanımlık değerler, tekrar gönderim, sahteciliğe karşı koruma ve güvenlik başlıkları | ⬚ | — | — | — |
| T210 | Liste kalıbı | ⬚ | — | — | — |
| T211 | Model bağlama kalıbı | ⬚ | — | — | — |
| T212 | Hata gösterimi ve HTTP durumları | ⬚ | — | — | — |
| T213 | Hata kodu kayıt defteri | ⬚ | — | — | — |
| T214 | Kısa süreli bildirim kodları | ⬚ | — | — | — |
| T215 | Hız sınırı, deneme limitleri ve gövde tavanı | ⬚ | — | — | — |
| T216 | Tarih, sayı ve para biçimi: istek ve gösterim | ⬚ | — | — | — |
| T217 | Yapılabilir işlemlerin sunucuda üretimi ve POST işleyicisinde yeniden denetim | ⬚ | — | — | — |
| T218 | Dokuz modülün iskeleti ve Contracts | ⬚ | — | — | — |
| T219 | Arama motoru verileri kalıbı | ⬚ | — | — | — |
| T220 | İş kuyruğu çalıştırıcısı | ⬚ | — | — | — |
| T221 | Zamanlayıcı | ⬚ | — | — | — |
| T222 | Anahtarlar ve tek kullanımlık bağlantılar | ⬚ | — | — | — |
| T223 | Dosya deposu | ⬚ | — | — | — |
| T224 | Çerez politikası | ⬚ | — | — | — |
| T225 | Sistem kayıtları | ⬚ | — | — | — |
| T226 | Ayar katmanları, tasarım değerleri ve açılışta ayar denetimi | ⬚ | — | — | — |
| T227 | Zaman ve para | ⬚ | — | — | — |
| T228 | Eşzamanlılık denetimi | ⬚ | — | — | — |
| T229 | Uygulama belleğinde önbellek | ⬚ | — | — | — |
| T230 | Uygulama kayıtlarının üç yazıcısı ve satır biçimleyicisi | ⬚ | — | — | — |
| T231 | Maskelemenin üç katmanının testleri | ⬚ | — | — | — |
| T232 | Veri koruma halkası | ⬚ | — | — | — |
| T233 | Veritabanı oturum süreleri | ⬚ | — | — | — |

---

## F3 — İş mantığı

| Task | Ad | Durum | Doğrulama | Commit | PR |
|---|---|---|---|---|---|
| T301 | Siparişin imha parçası | ⬚ | — | — | — |
| T302 | Öteki kayıtların süreyle imhası | ⬚ | — | — | — |
| T303 | İlk yönetici yönetim komutu ve komut çatısı | ⬚ | — | — | — |
| T304 | Alıcı başına bildirim gönderim sınırı | ⬚ | — | — | — |
| T305 | E-posta ortak kalıbı ve gövde üretimi | ⬚ | — | — | — |
| T306 | Müşteri bildirimleri B-1…B-8 | ⬚ | — | — | — |
| T307 | Müşteri bildirimleri B-9…B-16 | ⬚ | — | — | — |
| T308 | Firma bildirimleri F-1…F-6 | ⬚ | — | — | — |
| T309 | Hesap ve yönetim e-postaları | ⬚ | — | — | — |
| T310 | Kargo şirketi listesi ve takip bağlantısı kalıpları | ⬚ | — | — | — |
| T311 | Ödeme sağlayıcısının bildirim ucu: imza, alan eşlemesi ve durum makinesine aktarım | ⬚ | — | — | — |
| T312 | Dosya uçları: imzalı indirme ucu ve parçalı yükleme ucu | ⬚ | — | — | — |
| T313 | Site haritası ve robots.txt | ⬚ | — | — | — |
| T314 | Durum ucu: uyarı koşulları ve 500 sayacı | ⬚ | — | — | — |
| T315 | Durum makinesi çekirdeği: envanter, geçiş tablosu ve kimlikleri, durum makinesi olmayan varlıkların sınırı | ⬚ | — | — | — |
| T316 | Sipariş durumu geçişleri: sevkiyat ekseni | ⬚ | — | — | — |
| T317 | Ödeme durumu geçişleri: ödeme ekseni | ⬚ | — | — | — |
| T318 | İki eksenin bağı, tipe göre hatlar ve manuel adım bütçesi | ⬚ | — | — | — |
| T319 | Geri alınamaz geçişler, onay gerekleri ve durum düzeltmesi | ⬚ | — | — | — |
| T320 | Kalem kayıtları, ayıp talebi durumu, iade reddi ve panel işaretleri | ⬚ | — | — | — |
| T321 | Ürün yayın durumları | ⬚ | — | — | — |
| T322 | İçerik yayın durumları ve iletişim talebi durumları | ⬚ | — | — | — |
| T323 | Durum × rol × işlem tablosu | ⬚ | — | — | — |
| T324 | Zaman aşımı akışları: hesap, oturum, sepet ve ödeme süreleri | ⬚ | — | — | — |
| T325 | Zaman aşımı akışları: teslim, cayma, geri ödeme ve ayıp talebi süreleri | ⬚ | — | — | — |
| T326 | Zaman aşımı akışları: katalog, kupon, içerik, iletişim ve bildirim süreleri | ⬚ | — | — | — |
| T327 | Kendiliğinden işleyen anlar ve sitenin kesintisinde süreler | ⬚ | — | — | — |
| T328 | Bildirim haritası: tetikleyici eşlemesi, bildirim üretmeyen olaylar ve eşitlik denetimi | ⬚ | — | — | — |
| T329 | İlk uçtan uca akış testi: ürün, sepet, sipariş, sahte ödeme sonucu, kargo, teslim (servis düzeyi) | ⬚ | — | — | — |
| T330 | Firma kimliği: tek firma, tip ve tipe göre zorunlu alanlar, yasal kimlik bilgilerinin erişilebilirliği, iade adresi | ⬚ | — | — | — |
| T331 | Satış kapısı: dört koşul, veri toplayan girişlerin kapısı, satışın geçici kapatılması | ⬚ | — | — | — |
| T332 | Satış modeli, ürün tipleri, ürün–varyant–seçenek yapısı ve stok kodu | ⬚ | — | — | — |
| T333 | Kategori ağacı, site içi arama ve liste süzgeçleri | ⬚ | — | — | — |
| T334 | Stok ve hizmet kontenjanı: takip, tükenme, ayrılmış adedin altına inmeme | ⬚ | — | — | — |
| T335 | Ürün yayın durumu: yayın kapısı, arşiv, silme ve engelleme kalıbı | ⬚ | — | — | — |
| T336 | Fiyat ve KDV: kuruş hassasiyeti, kalem bazlı hesap sırası, yuvarlama yönü, fiyat etiketi bilgileri | ⬚ | — | — | — |
| T337 | İndirim ve referans fiyat: tarihli indirim, fiyat geçmişinden referans, kaydı engelleyen koşullar | ⬚ | — | — | — |
| T338 | Kupon: tanım, kalem payı, sınırlar, kullanım hakkının ayrılması | ⬚ | — | — | — |
| T339 | Ürün görseli ve metni: ana görsel, kısıtlar, alternatif metin, metin biçimi seti | ⬚ | — | — | — |
| T340 | Dijital ürün: dosya, indirme hakkı, hak yenileme, yeniden satın alma | ⬚ | — | — | — |
| T341 | Üyelik: kayıt, e-posta doğrulama kapısı, misafir siparişinin hesaba bağlanması, hesap adı, giriş kaydı | ⬚ | — | — | — |
| T342 | Giriş ve oturum: oturum ömrü, şifre politikası, sıfırlama, yeniden doğrulama, yönetici hesaplarına aynı rejim, giriş limitleri | ⬚ | — | — | — |
| T343 | Google ile giriş ve hesap bağlama, e-posta değişikliği ve geri alma | ⬚ | — | — | — |
| T344 | Adres: adres defteri, fatura adresi, kapalı il listesi, tipe göre istenen adresler, siparişe donma | ⬚ | — | — | — |
| T345 | Hesabın silinmesi ve kişisel veri başvurusu | ⬚ | — | — | — |
| T346 | Sepet: sahiplik, giriş anında birleşme, canlı kalem, siparişe girmeyen kalemler | ⬚ | — | — | — |
| T347 | Sepet adet kuralları: stok ve sipariş başına sınır, boşalma, istek listesi yokluğu | ⬚ | — | — | — |
| T348 | Sipariş onayı: oluşma anı, özet eşitliği, stok ayırma, ödeme süresi, kendiliğinden iptal | ⬚ | — | — | — |
| T349 | Asgari sipariş tutarı, kargo ücreti ve ücretsiz kargo eşiği | ⬚ | — | — | — |
| T350 | Teslim: il kısıtı, tek yol, kargoya verme süresi ve sözü, kargoya verildi işareti, kargo şirketi | ⬚ | — | — | — |
| T351 | Teslim işaretleri: kalem tipine göre teslim, karışık sipariş, fiziksel teslim tarihi | ⬚ | — | — | — |
| T352 | Ödeme: satıcı ve kart verisi, iki yöntem, 3D Secure, taksit, ters ibraz | ⬚ | — | — | — |
| T353 | Havale/EFT: IBAN ve siparişe donması, ödendi işareti, hatırlatma, süresi dolmuş sipariş | ⬚ | — | — | — |
| T354 | Sipariş numarası ve takip erişimi: numara, üç giriş yolu, erişim anahtarı | ⬚ | — | — | — |
| T355 | Sipariş anında donan değerler | ⬚ | — | — | — |
| T356 | Onay adımı: onay kutuları, Ön Bilgilendirme Formu içeriği, işlem rehberi | ⬚ | — | — | — |
| T357 | Fatura verisi ve dışa aktarmada donmuş kimlik | ⬚ | — | — | — |
| T358 | Kurumsal içerik tipleri: Hakkımızda, hizmet tanıtımı, referans iş, SSS, şube, genel sayfa, sosyal bağlantılar | ⬚ | — | — | — |
| T359 | Kurumsal içerik: ürün bağı, elle sıralama, zorunlu alanlar, alan tavanları, kapsam dışı yetenekler | ⬚ | — | — | — |
| T360 | Duyuru şeridi, boş kurulum, içerik yayın durumu, önizleme ve silme | ⬚ | — | — | — |
| T361 | Ana sayfa düzeni ve üst menü | ⬚ | — | — | — |
| T362 | Marka kimliği: logo, marka adı, site simgesi, marka rengi, platform imzası | ⬚ | — | — | — |
| T363 | Sayfa adresleri, arama motoru bilgileri, kapalı sayfalar ve paylaşım | ⬚ | — | — | — |
| T364 | Panelde eşzamanlı düzenleme ve siparişte çakışan işlemler | ⬚ | — | — | — |
| T365 | İletişim talebi: form, konu tipi, durumlar, saklama, istenmeyen ileti koruması | ⬚ | — | — | — |
| T366 | Yasal metinler: dört metin, sürüm, bağlantılar, e-postada tam metin | ⬚ | — | — | — |
| T367 | Müşteri iptali: kalem ve adet düzeyi, kargoya verilene kadar, dijital ve hizmet sınırı, sayaçların dönmesi, kısmi iptalde kupon | ⬚ | — | — | — |
| T368 | Gecikme feshi ve firma iptali: iki fesih yolu, kanuni faiz uyarısı, kapalı sebep listesi, ifanın imkânsızlaşması bildirimi | ⬚ | — | — | — |
| T369 | İşlemin adet birimi ve geri ödeme tutarı: açık adet, kupon payı, KDV, kargo ücretinin iptalde ve caymada ayrılması | ⬚ | — | — | — |
| T370 | Geri ödeme hattı: ödemenin geldiği yol, kart ve havale, IBAN'ın ömrü, geri ödeme süresi, kart iadesi hataları | ⬚ | — | — | — |
| T371 | Cayma hakkı: pencere, dijital ve hizmet istisnası, ürün istisna işareti, tek rejim | ⬚ | — | — | — |
| T372 | Cayma beyanı: sipariş sayfasından ve başka kanaldan, beyanın etkisi, kargoya verilmemiş kalemde iptal yolu, firmanın teyidi | ⬚ | — | — | — |
| T373 | İade: iade kargo bedeli, ödemeli gönderim ve iade adresi, teslim alma adımı, stoğa ekleme, koşullu istisnada ret, kullanılmış mal | ⬚ | — | — | — |
| T374 | Ayıp talebi: kanal, iki yıllık süre, durumlar, çözüm, dijital dosya güncellemesi | ⬚ | — | — | — |
| T375 | Yöneticinin sipariş düzeltmeleri: ileri yönlü kayıt, tutar değişmez, firmanın teyidiyle e-posta ve adres düzeltmesi, cayma beyanı geri alınmaz | ⬚ | — | — | — |
| T376 | Deneme limitleri: dokuz limit, kayan pencere, hesap kilitlenmez, nötr mesaj | ⬚ | — | — | — |
| T377 | Limit ilkeleri: iki eksen, ödenmemiş sipariş sayımı, yönetici muafiyeti yok, tanınan tarayıcı işareti | ⬚ | — | — | — |
| T378 | Zaman sayımı: tek saat dilimi, iş günü ve resmî tatil listesi, takvim ve iş günü sayımı, gün sonu, tarih aralığı, çitler | ⬚ | — | — | — |
| T379 | Yönetici hesapları: ilk yönetici ve davet, kaldırma kuralları, tek rol, ayrı hesap türü, davetin geri çekilmesi, erişim kaybında kurtarma sınırı | ⬚ | — | — | — |
| T380 | İşlem izi: sekiz alan, kim-ne zaman-ne değişti, değiştirilemez ve silinemez, sistem olayları ize girmez, düz liste, on yıl | ⬚ | — | — | — |
| T381 | Ayar katmanları ve parametre envanteri: firma ayarı, kurulum ayarı, ürün sabiti, varsayılan ve çit, değişikliğin ize yazılması | ⬚ | — | — | — |
| T382 | Panel ana sayfası verisi: altı bekleyen iş sayacı, kanal ve firma bildirimi uyarıları, kurulum kontrol listesi | ⬚ | — | — | — |
| T383 | Satış özeti ve ölçüler: dönem seçimi, ciro, sayılar, dört mağaza ölçüsü, sıfır payda, tepe gün sayısı | ⬚ | — | — | — |
| T384 | Dışa aktarma: sipariş CSV (fatura verisi), üye ve iletişim talebi listesi, işlem izine yazım, imha edilmiş siparişin boş sütunları | ⬚ | — | — | — |
| T385 | Gönderim yeniden denemesi ve "e-posta ulaşmadı" işareti: üç deneme, işaretin düşüşü, panelden yeniden gönderim, işaretsiz bildirimler | ⬚ | — | — | — |
| T386 | Sipariş müdahaleleri: kalem adedi azaltma ve çıkarma, kargo bilgisinin ve teslim tarihinin düzeltilmesi, iç not | ⬚ | — | — | — |
| T387 | Kurulum tarafı yönetim komutları: iş satırını yeniden kuyruğa alma ya da kapatma, anahtar yenilemesi sonrası ödeme sonucunun uygulanması, kayıt okuma komutu | ⬚ | — | — | — |
| T388 | Dönüş denetimi komutu ve dönüşten sonra silmelerin yeniden uygulanması | ⬚ | — | — | — |
| T389 | Hesap ve durum yönetim komutları: oturumları sonlandırma, şifreyi geçersiz kılma, durum | ⬚ | — | — | — |
| T390 | Uçtan uca dal akış testleri (servis düzeyi): karışık sipariş, kargoda cayma, IBAN'sız iade, e-posta düzeltme | ⬚ | — | — | — |

---

## F4 — Entegrasyonlar

| Task | Ad | Durum | Doğrulama | Commit | PR |
|---|---|---|---|---|---|
| T401 | PayTR: anahtarlar, ödemenin başlatılması, dönüş ve çerçeve | ⬚ | — | — | — |
| T402 | PayTR: bildirim imzası ve alan eşlemesi | ⬚ | — | — | — |
| T403 | PayTR: son sorgu ve tek tahsilat | ⬚ | — | — | — |
| T404 | PayTR: bağlantı hatası ve ayrım | ⬚ | — | — | — |
| T405 | PayTR: kart iadesi | ⬚ | — | — | — |
| T406 | PayTR: 3D Secure kurulum şartı, test kanıtı ve canlı kanıtın satırları | ⬚ | — | — | — |
| T407 | SES: gönderim ve sonuç sınıfları | ⬚ | — | — | — |
| T408 | SES: geri dönen adreslerin öğrenilmesi | ⬚ | — | — | — |
| T409 | Google: giriş, kayıt ve yeniden doğrulama bağlayıcısı | ⬚ | — | — | — |
| T410 | Dış olgu varsayımlarının doğrulama kanıtları (DV) | ⬚ | — | — | — |

---

## F5 — Kullanıcı arayüzü

| Task | Ad | Durum | Doğrulama | Commit | PR |
|---|---|---|---|---|---|
| T501 | Yerli üretim logosu varlığı ve gösterimi | ⬚ | — | — | — |
| T502 | Vitrin: ana sayfa ve kategori sayfası (E-01, E-02) | ⬚ | — | — | — |
| T503 | Vitrin: arama sonuçları, ürün sayfası ve sepete ekleme (E-03, E-04) | ⬚ | — | — | — |
| T504 | Hata sayfaları: bulunamayan adres ve beklenmeyen hata (E-06, 500) | ⬚ | — | — | — |
| T505 | İçerik liste ve içerik sayfaları (E-07, E-08) | ⬚ | — | — | — |
| T506 | Sık sorulan sorular ve iletişim sayfası ve formu (E-09, E-10) | ⬚ | — | — | — |
| T507 | Yasal metin sayfaları ve panelde yönetimi (E-11, E-47) | ⬚ | — | — | — |
| T508 | Sepet (E-12) | ⬚ | — | — | — |
| T509 | Ödeme adımı (E-13) | ⬚ | — | — | — |
| T510 | Sipariş teyit ve bekleme ekranı, ödeme sağlayıcısına geçiş ve dönüş (E-14) | ⬚ | — | — | — |
| T511 | Sipariş takibi girişi, sipariş sayfası ve indirme bağlantısı (E-15, E-16) | ⬚ | — | — | — |
| T512 | İptal ve gecikme feshi ekranı, geri ödeme IBAN girişi (E-17, E-16 IBAN formu) | ⬚ | — | — | — |
| T513 | Cayma beyanı ve ayıp talebi ekranları (E-18, E-19) | ⬚ | — | — | — |
| T514 | Kayıt ve doğrulama bağlantısının inişi (E-20, E-21) | ⬚ | — | — | — |
| T515 | Müşteri girişi ve Google ile giriş (E-22) | ⬚ | — | — | — |
| T516 | Şifre sıfırlama ve yeniden doğrulama (E-23, E-24) | ⬚ | — | — | — |
| T517 | Hesap — profil ve güvenlik (E-25) | ⬚ | — | — | — |
| T518 | Adres defteri ve sipariş geçmişi (E-26, E-27) | ⬚ | — | — | — |
| T519 | E-posta değişikliğini geri alma ve müşteri çıkışı (E-28) | ⬚ | — | — | — |
| T520 | Panel girişi, şifre sıfırlama, davet kabulü ve yönetici çıkışı (E-29, E-30) | ⬚ | — | — | — |
| T521 | Panel ana sayfası ve yönetici hesabının doğrulama ekranları (E-31, E-54) | ⬚ | — | — | — |
| T522 | Yönetici hesapları ve işlem izi (E-49, E-52) | ⬚ | — | — | — |
| T523 | Yöneticinin kendi hesabı (E-50) | ⬚ | — | — | — |
| T524 | Satış özeti ve dışa aktarma (E-51, E-53) | ⬚ | — | — | — |
| T525 | Ürün listesi ve ürün formu, görsel yükleme (E-32, E-33) | ⬚ | — | — | — |
| T526 | Kategori ağacı ve kuponlar (E-34, E-35) | ⬚ | — | — | — |
| T527 | Sipariş listesi, sipariş ayrıntısı ve durum geçişleri (E-36, E-37) | ⬚ | — | — | — |
| T528 | Sipariş ayrıntısı: firma iptali, bildirim kaydı, kalem çıkarma, iade malı ve ayıp talebi (E-37) | ⬚ | — | — | — |
| T529 | Sipariş ayrıntısı: geri ödeme işlemleri (E-37) | ⬚ | — | — | — |
| T530 | Sipariş ayrıntısı: adres, kargo, teslim tarihi, durum geçişi ve e-posta düzeltmeleri (E-37) | ⬚ | — | — | — |
| T531 | Sipariş ayrıntısı: hizmet kalemi, indirme hakkı, iç not ve e-posta yeniden gönderim (E-37) | ⬚ | — | — | — |
| T532 | Talepler ve iletişim talebi ayrıntısı (E-38, E-39) | ⬚ | — | — | — |
| T533 | Üye kaydı görünümü (E-40) | ⬚ | — | — | — |
| T534 | Kurumsal içerik ve marka yönetimi (E-41, E-43) | ⬚ | — | — | — |
| T535 | Ana sayfa ve menü, firma kimliği ve satış (E-42, E-44) | ⬚ | — | — | — |
| T536 | Ödeme yöntemleri, kargo, süreler ve varsayılanlar (E-45, E-46) | ⬚ | — | — | — |
| T537 | Vitrin çerçevesinin modeli: marka, menü, duyuru şeridi, hesap alanı, sepet sayısı | ⬚ | — | — | — |
| T538 | Uç ve model eşitlik denetimleri: uç envanteri, form girdi ve sorgu modelleri, makine ucu gövdeleri | ⬚ | — | — | — |
| T539 | Betiksiz yol: JavaScript kapalıyken temel akışlar ve betikli iyileştirme | ⬚ | — | — | — |
| T540 | Tasarım tabanı, vitrin ve panel çerçevesi, yönetici şeridi ve Taslak bandı (OB-01, OB-02, OB-09) | ⬚ | — | — | — |
| T541 | Mesaj, onay, boş ve hata hâlleri ve satış kapalı hâli (OB-03, OB-04, OB-07, OB-08) | ⬚ | — | — | — |
| T542 | Rozet ve işaretler, sayaç ve süre göstergeleri, eşzamanlı düzenleme uyarısı (OB-05, OB-06, OB-10) | ⬚ | — | — | — |
| T543 | Form alanı ve düzeni, adres formu, IBAN alanı, sebep seçimi ve ortak alan kuralları (OB-12…OB-15) | ⬚ | — | — | — |
| T544 | Tarih alanı, kupon alanı, onay kutuları, görsel ve dosya yükleme arayüzü (OB-16…OB-19) | ⬚ | — | — | — |
| T545 | Liste ve tablo, ürün kartı (OB-20, OB-11) | ⬚ | — | — | — |
| T546 | Gezinme ve geçiş eşitlik denetimleri: iskeletler, geçişler, dışarıdan girişler, dönüşler | ⬚ | — | — | — |
| T547 | Durum × rol matrisi ve form envanteri eşitlik denetimleri | ⬚ | — | — | — |
| T548 | Parçalı yükleme betiği: başlatma–parça–durum akışı, 409/404/400/413 yanıtları, ağ hatasında üç kez sürdürme | ⬚ | — | — | — |
| T549 | Vitrin: arşivlenmiş ürün sayfası (E-05) | ⬚ | — | — | — |

---

## F6 — Uçtan uca doğrulama

| Task | Ad | Durum | Doğrulama | Commit | PR |
|---|---|---|---|---|---|
| T601 | `DEPLOY_RUNBOOK` provası, canlı kanıt, dış sistemde kayıtlı adresler ve yansıyan tablolar | ⬚ | — | — | — |
| T602 | Güncelleme komutu: referanstan başlayıp sırayla, geride kalmış kurulumun ara sürümlerden geçişi, geri alma ve yedekten dönüş koşulu | ⬚ | — | — | — |

---

## Borç kapatma paketleri (WP)

> Ayrıntı: [`DEBT_CLOSURE_PLAN.md`](DEBT_CLOSURE_PLAN.md)

| WP | Başlık | Durum | Doğrulama | Commit | PR |
|---|---|---|---|---|---|
