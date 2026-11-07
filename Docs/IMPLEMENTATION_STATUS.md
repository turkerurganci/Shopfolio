# Shopfolio — Implementation Status

**Son güncelleme:** 2026-11-07 — T333 (Kategori ağacı, site içi arama ve liste süzgeçleri) doğrulama tur 2 ✗ FAIL (head `d42a5a4` + `origin/main` birleştirmesi, PR #442; kriter 1–8 ve KK1–KK8 ✓, tur 1 B1 düzeltmesi doğrulandı (iki listede sayfa sınırı testleri yeşil); güvenlik taraması temiz; Unit 4064, Integration `CategoryTreeAndListsTests` 32/32 + `ConcurrencyTests` 26/26; dal CI 42406747948 yeşil). **Bulgular:** B1 DÜŞÜK S1 — `09 §25` Performans "okuma sorgusu projeksiyonlu mu" hayır: `ProductListReader` sayfayı `Product` varlığının bütün sütunlarıyla (açıklama gövdesi dahil) okur, `09 §20` madde 3 yalnız gösterilen sütunların `Select` ile alınmasını ister (D-134 bunu T333'e yönlendirmişti); B2 DÜŞÜK S1 — `09 §20` madde 4: varyant rakamlarını yükleyen liste için sorgu sayısının sayfadaki ürün sayısından bağımsız olduğunu komut yakalayıcıyla sınayan entegrasyon testi yok. K1 → D-372 (ürün–kategori bağı değişince menü/ağaç önbellek alanı bildirimi, T361). Düzeltme 2 gerekli: B1 + B2 (ve D-134 kapanışı); iterasyon 1/2 kullanıldı (`12 §8.5`).

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
| F0 | Proje iskeleti | `T01`–`T10` (10) | ✓ PASS | `phase/F0-pass` | [GATE_CHECK_F0](CHECKPOINT_REPORTS/GATE_CHECK_F0.md) |
| F1 | Veri katmanı | `T101`–`T120` (20) | ✓ PASS | `phase/F1-pass` | [GATE_CHECK_F1](CHECKPOINT_REPORTS/GATE_CHECK_F1.md) |
| F2 | Çekirdek servisler | `T201`–`T233` (33) | ✓ PASS | `phase/F2-pass` | [GATE_CHECK_F2](CHECKPOINT_REPORTS/GATE_CHECK_F2.md) |
| F3 | İş mantığı | `T301`–`T390` (90) | ⬚ | — | |
| F4 | Entegrasyonlar | `T401`–`T410` (10) | ⬚ | — | |
| F5 | Kullanıcı arayüzü | `T501`–`T549` (49) | ⬚ | — | |
| F6 | Uçtan uca doğrulama | `T601`–`T603` (3) | ⬚ | — | |

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
| T01 | Canlılık ucu | ✓ | ✓ PASS (tur 4) | `6c07304` | #338 |
| T02 | Çözüm ve proje düzeni | ✓ | ✓ PASS (tur 2) | `f83876a` | #335 |
| T03 | Test altyapısı | ✓ | ✓ PASS (tur 3) | `e4de649` | #337 |
| T04 | CI hattı ve eşitlik denetimleri | ✓ | ✓ PASS (tur 2) | `20fdd0c` | #339 |
| T05 | Compose geliştirme ortamı | ✓ | ✓ PASS (tur 2) | `5509c5d` | #348 |
| T06 | Üç imaj ve kayıt defteri | ✓ | ✓ PASS (tur 2) | `aaf2f2a` | #347 |
| T07 | Yedek düzeni | ✓ | ✓ PASS (tur 4) | `6a8dc38` | #351 |
| T08 | Sır düzeni | ✓ | ✓ PASS (tur 4) | `ca42ca9` | #349 |
| T09 | İlk iniş provası: yerel referans kurulumda canlılık | ✓ | ✓ PASS (tur 2) | `93ba91f` | #354 |
| T10 | Veritabanı kullanıcıları | ✓ | ✓ PASS (tur 2) | `af4103c` | #350 |

---

## F1 — Veri katmanı

| Task | Ad | Durum | Doğrulama | Commit | PR |
|---|---|---|---|---|---|
| T101 | Veri modeli temeli | ✓ | ✓ PASS (tur 1) | `f2470d8` | #358 |
| T102 | Enum tanımları ve değer dönüşümü | ✓ | ✓ PASS (tur 1) | `597e14a` | #359 |
| T103 | Başlangıç listeleri, firma ve yasal metin sürümü | ✓ | ✓ PASS (tur 2) | `6b5cc83` | #360 |
| T104 | Ürün çekirdeği ve görsel | ✓ | ✓ PASS (tur 3) | `0c270de` | #361 |
| T105 | Fiyat, kupon ve dijital dosya | ✓ | ✓ PASS (tur 1) | `7b75bb5` | #362 |
| T106 | Yönetici hesabı, davet ve işlem izi | ✓ | ✓ PASS (tur 3) | `93c164e` | #363 |
| T107 | Müşteri hesabı | ✓ | ✓ PASS (tur 1) | `d374bc1` | #364 |
| T108 | Oturum, bağlantı, deneme ve giriş kayıtları | ✓ | ✓ PASS (tur 2) | `6c390c2` | #365 |
| T109 | Sepet ve sipariş çekirdeği | ✓ | ✓ PASS (tur 1) | `521bb61` | #366 |
| T110 | Ayırma ve düzeltme geçmişleri | ✓ | ✓ PASS (tur 1) | `f5b786c` | #367 |
| T111 | Kalem kayıtları | ✓ | ✓ PASS (tur 1) | `f90d538` | #369 |
| T112 | Ödeme ve geri ödeme kayıtları | ✓ | ✓ PASS (tur 1) | `3add87b` | #370 |
| T113 | İade reddi ve IBAN isteği | ✓ | ✓ PASS (tur 1) | `0476eed` | #371 |
| T114 | İçerik kayıtları ve marka | ✓ | ✓ PASS (tur 1) | `7fa45cf` | #372 |
| T115 | Şube, içerik–ürün bağları ve iletişim talebi | ✓ | ✓ PASS (tur 1) | `d2c7c36` | #373 |
| T116 | Galeri bağları | ✓ | ✓ PASS (tur 1) | `7afeaff` | #374 |
| T117 | Gönderim kaydı ve kanal hâli | ✓ | ✓ PASS (tur 1) | `b7bd9b2` | #375 |
| T118 | Zamanlayıcı, yedek, imha ve anahtar kayıtları | ✓ | ✓ PASS (tur 1) | `f088c3f` | #376 |
| T119 | Başlangıç verisi göçü ve seed olmayanların sınırı | ✓ | ✓ PASS (tur 1) | `07e6121` | #377 |
| T120 | Model eşitlik denetimleri | ✓ | ✓ PASS (tur 2) | `c59321d` | #378 |

---

## F2 — Çekirdek servisler

| Task | Ad | Durum | Doğrulama | Commit | PR |
|---|---|---|---|---|---|
| T201 | İmha düzenleyicisi, süre ayarları ve gecelik temizlik | ✓ | ✓ PASS (tur 1) | `cafdf86` | #395 |
| T202 | Dış servislerin bağlantı katmanı: ortak yüz, hata sınıfı, zaman aşımı, sahteler | ✓ | ✓ PASS (tur 1) | `a5cd5b1` | #396 |
| T203 | İmzalı makine ucu altyapısı: imza denetimi, gövde tavanı | ✓ | ✓ PASS (tur 2; tur 1 ✗ FAIL) | `aee8538` | #397 |
| T204 | Dış servis çökmesi davranışı ve sahtelerle sınama | ✓ | ✓ PASS (tur 1) | `4628f9c` | #398 |
| T205 | Açılışta veritabanı fail-fast denetimi | ✓ | ✓ PASS (tur 1) | `a36010b` | #385 |
| T206 | Adres yapısı, önekler ve sayfa–form işleyici kalıbı | ✓ | ✓ PASS (tur 1) | `494a34c` | #401 |
| T207 | Kimlik şemaları ve erişim reddi | ✓ | ✓ PASS (tur 1) | `3226fcd` | #403 |
| T208 | Sürümün ve gösterilen değerin formda taşınması, koşullu güncelleme | ✓ | ✓ PASS (tur 1) | `aeab023` | #404 |
| T209 | Tek kullanımlık değerler, tekrar gönderim, sahteciliğe karşı koruma ve güvenlik başlıkları | ✓ | ✓ PASS (tur 1) | `088168e` | #405 |
| T210 | Liste kalıbı | ✓ | ✓ PASS (tur 1) | `539d9cd` | #406 |
| T211 | Model bağlama kalıbı | ✓ | ✓ PASS (tur 2; tur 1 ✗ FAIL) | `da20c4e` | #402 |
| T212 | Hata gösterimi ve HTTP durumları | ✓ | ✓ PASS (tur 1) | `4694519` | #407 |
| T213 | Hata kodu kayıt defteri | ✓ | ✓ PASS (tur 1) | `11a2c7d` | #408 |
| T214 | Kısa süreli bildirim kodları | ✓ | ✓ PASS (tur 1) | `9a56f83` | #410 |
| T215 | Hız sınırı, deneme limitleri ve gövde tavanı | ✓ | ✓ PASS (tur 2) | `5ebe163` | #409 |
| T216 | Tarih, sayı ve para biçimi: istek ve gösterim | ✓ | ✓ PASS (tur 1) | `9ab9bec` | #411 |
| T217 | Yapılabilir işlemlerin sunucuda üretimi ve POST işleyicisinde yeniden denetim | ✓ | ✓ PASS (tur 3; tur 1–2 ✗ FAIL) | `d2b5379` | #412 |
| T218 | Dokuz modülün iskeleti ve Contracts | ✓ | ✓ PASS (tur 3) | `d82b943` | #380 |
| T219 | Arama motoru verileri kalıbı | ✓ | ✓ PASS (tur 1) | `4ce191c` | #413 |
| T220 | İş kuyruğu çalıştırıcısı | ✓ | ✓ PASS (tur 1) | `6a1ca3a` | #393 |
| T221 | Zamanlayıcı | ✓ | ✓ PASS (tur 3) | `f8feec5` | #394 |
| T222 | Anahtarlar ve tek kullanımlık bağlantılar | ✓ | ✓ PASS (tur 1) | `11fcfb3` | #392 |
| T223 | Dosya deposu | ✓ | ✓ PASS (tur 2; tur 1 ✗ FAIL) | `5cc7896` | #399 |
| T224 | Çerez politikası | ✓ | ✓ PASS (tur 2; tur 1 ✗ FAIL) | `bcedad3` | #400 |
| T225 | Sistem kayıtları | ✓ | ✓ PASS (tur 3) | `b8c180f` | #389 |
| T226 | Ayar katmanları, tasarım değerleri ve açılışta ayar denetimi | ✓ | ✓ PASS (tur 1) | `b8f20ad` | #383 |
| T227 | Zaman ve para | ✓ | ✓ PASS (tur 1) | `15daf95` | #382 |
| T228 | Eşzamanlılık denetimi | ✓ | ✓ PASS (tur 1) | `36b586f` | #391 |
| T229 | Uygulama belleğinde önbellek | ✓ | ✓ PASS (tur 1) | `f4da03c` | #414 |
| T230 | Uygulama kayıtlarının üç yazıcısı ve satır biçimleyicisi | ✓ | ✓ PASS (tur 2) | `a2da89f` | #386 |
| T231 | Maskelemenin üç katmanının testleri | ✓ | ✓ PASS (tur 1) | `dac48c8` | #387 |
| T232 | Veri koruma halkası | ✓ | ✓ PASS (tur 2) | `daace97` | #388 |
| T233 | Veritabanı oturum süreleri | ✓ | ✓ PASS (tur 2) | `810fe27` | #384 |

---

## F3 — İş mantığı

| Task | Ad | Durum | Doğrulama | Commit | PR |
|---|---|---|---|---|---|
| T301 | Siparişin imha parçası | ✓ | ✓ PASS (tur 1) | — | #431 |
| T302 | Öteki kayıtların süreyle imhası | ⬚ | — | — | — |
| T303 | İlk yönetici yönetim komutu ve komut çatısı | ⬚ | — | — | — |
| T304 | Alıcı başına bildirim gönderim sınırı | ✓ | ✓ PASS (tur 1) | `268ab8d` | #421 |
| T305 | E-posta ortak kalıbı ve gövde üretimi | ✓ | ✓ PASS (tur 1) | `7cc0db5` | #422 |
| T306 | Müşteri bildirimleri B-1…B-8 | ✓ | ✓ PASS (tur 3) | `9e3f8c3` | #427 |
| T307 | Müşteri bildirimleri B-9…B-16 | ✓ | ✓ PASS (tur 1) | — | #435 |
| T308 | Firma bildirimleri F-1…F-6 | ✓ | ✓ PASS (tur 1) | `ad8c48d` | #424 |
| T309 | Hesap ve yönetim e-postaları | ✓ | ✓ PASS (tur 1) | `987e438` | #429 |
| T310 | Kargo şirketi listesi ve takip bağlantısı kalıpları | ✓ | ✓ PASS (tur 1) | `e177fd4` | #423 |
| T311 | Ödeme sağlayıcısının bildirim ucu: imza, alan eşlemesi ve durum makinesine aktarım | ⬚ | — | — | — |
| T312 | Dosya uçları: imzalı indirme ucu ve parçalı yükleme ucu | ✓ | ✓ PASS (tur 1) | `1112ca6` | #426 |
| T313 | Site haritası ve robots.txt | ✓ | ✓ PASS (tur 1) | `f94b4d8` | #417 |
| T314 | Durum ucu: uyarı koşulları ve 500 sayacı | ✓ | ✓ PASS (tur 1) | `335e9cb` | #418 |
| T315 | Durum makinesi çekirdeği: envanter, geçiş tablosu ve kimlikleri, durum makinesi olmayan varlıkların sınırı | ✓ | ✓ PASS (tur 1) | — | #434 |
| T316 | Sipariş durumu geçişleri: sevkiyat ekseni | ✓ | ✓ PASS (tur 2) | — | #437 |
| T317 | Ödeme durumu geçişleri: ödeme ekseni | ⬚ | — | — | — |
| T318 | İki eksenin bağı, tipe göre hatlar ve manuel adım bütçesi | ⬚ | — | — | — |
| T319 | Geri alınamaz geçişler, onay gerekleri ve durum düzeltmesi | ⬚ | — | — | — |
| T320 | Kalem kayıtları, ayıp talebi durumu, iade reddi ve panel işaretleri | ⬚ | — | — | — |
| T321 | Ürün yayın durumları | ⬚ | — | — | — |
| T322 | İçerik yayın durumları ve iletişim talebi durumları | ✓ | ✓ PASS (tur 1) | — | #438 |
| T323 | Durum × rol × işlem tablosu | ⬚ | — | — | — |
| T324 | Zaman aşımı akışları: hesap, oturum, sepet ve ödeme süreleri | ⬚ | — | — | — |
| T325 | Zaman aşımı akışları: teslim, cayma, geri ödeme ve ayıp talebi süreleri | ⬚ | — | — | — |
| T326 | Zaman aşımı akışları: katalog, kupon, içerik, iletişim ve bildirim süreleri | ⬚ | — | — | — |
| T327 | Kendiliğinden işleyen anlar ve sitenin kesintisinde süreler | ⬚ | — | — | — |
| T328 | Bildirim haritası: tetikleyici eşlemesi, bildirim üretmeyen olaylar ve eşitlik denetimi | ✓ | ✓ PASS (tur 1) | — | #440 |
| T329 | İlk uçtan uca akış testi: ürün, sepet, sipariş, sahte ödeme sonucu, kargo, teslim (servis düzeyi) | ⬚ | — | — | — |
| T330 | Firma kimliği: tek firma, tip ve tipe göre zorunlu alanlar, yasal kimlik bilgilerinin erişilebilirliği, iade adresi | ✓ | ✓ PASS (tur 1) | `4924037` | #420 |
| T331 | Satış kapısı: dört koşul, veri toplayan girişlerin kapısı, satışın geçici kapatılması | ✓ | ✓ PASS (tur 1) | `eb91975` | #425 |
| T332 | Satış modeli, ürün tipleri, ürün–varyant–seçenek yapısı ve stok kodu | ✓ | ✓ PASS (tur 1) | `c4c872c` | #432 |
| T333 | Kategori ağacı, site içi arama ve liste süzgeçleri | ⏳ | ✗ FAIL (tur 2) | — | #442 |
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
| T366 | Yasal metinler: dört metin, sürüm, bağlantılar, e-postada tam metin | ✓ | ✓ PASS (tur 2) | — | #428 |
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
| T378 | Zaman sayımı: tek saat dilimi, iş günü ve resmî tatil listesi, takvim ve iş günü sayımı, gün sonu, tarih aralığı, çitler | ✓ | ✓ PASS (tur 1) | `552a13d` | #416 |
| T379 | Yönetici hesapları: ilk yönetici ve davet, kaldırma kuralları, tek rol, ayrı hesap türü, davetin geri çekilmesi, erişim kaybında kurtarma sınırı | ⬚ | — | — | — |
| T380 | İşlem izi: sekiz alan, kim-ne zaman-ne değişti, değiştirilemez ve silinemez, sistem olayları ize girmez, düz liste, on yıl | ✓ | ✓ PASS (tur 2) | `7f8d6c3` | #419 |
| T381 | Ayar katmanları ve parametre envanteri: firma ayarı, kurulum ayarı, ürün sabiti, varsayılan ve çit, değişikliğin ize yazılması | ✓ | ✓ PASS (tur 2) | — | #430 |
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
| T603 | Gerçek sunucuya ilk iniş: ilk sürüm etiketi, açık imajlar, gerçek alan adı ve bulut deposu | ⬚ | — | — | — |

---

## Borç kapatma paketleri (WP)

> Ayrıntı: [`DEBT_CLOSURE_PLAN.md`](DEBT_CLOSURE_PLAN.md)

| WP | Başlık | Durum | Doğrulama | Commit | PR |
|---|---|---|---|---|---|
