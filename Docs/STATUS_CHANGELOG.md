# Shopfolio — Status Changelog

> [`IMPLEMENTATION_STATUS.md`](IMPLEMENTATION_STATUS.md)'nin **tarihsel arşivi**.
>
> **Neden ayrı dosya:** Güncel snapshot kısa kalmalı, tarihsel detay büyüyebilir (00 §G.2).
> Status dosyasının "son güncelleme" bloğu bir sonraki güncellemeye yer açarken **eski giriş buraya taşınır** — status dosyasında birikmez.
>
> **Sıra:** En yeni giriş en üstte.

---

## 2026-11-08 — T342 (Giriş ve oturum: oturum ömrü, şifre politikası, sıfırlama, yeniden doğrulama, yönetici hesaplarına aynı rejim, giriş limitleri) doğrulama tur 1 ✗ FAIL

2026-11-08 — T342 doğrulama tur 1 ✗ FAIL (head `0fadc90` + `origin/main` (`bcb856d`, T302) birleştirmesi, PR #453; dal CI 42436720596 yeşil; Unit 4035 + Consistency 241 + betik 4, Integration Membership 43 + komşu 103 yerelde yeşil; kriter 1, 3–9 ve KK1, KK3–KK9 ✓). **B1 KRİTİK S1:** giriş hesabın satırını paylaşımlı kilitle okumuyor ve özetin hâlâ aynı olduğunu o kilit altında denetlemiyor (`09 §15.1` m.3, `05 §6.1.2`); şifre sıfırlama/değiştirmeyle aynı anda süren eski şifreli giriş, sıfırlamanın oturum silmesinden sonra kesinleşen oturumla açık kalır — kriter 2 / KK2 ~ Kısmi, `09 §25` eşzamanlılık satırı ✗. K1 D-251 metni düzeltildi (yönetim komutu T303'te), K2 durum satırı doğrulamadan önce ✓ yazılmıştı → ⏳. `12` KP-27 satırları `BEKLEMEDE` kaldı (uç yanıtı sayfa task'larında). Backlog aktif 163 (D-428 notu).

---

## 2026-11-08 — T342 (Giriş ve oturum: oturum ömrü, şifre politikası, sıfırlama, yeniden doğrulama, yönetici hesaplarına aynı rejim, giriş limitleri) yapım tamamlandı, doğrulama bekliyor (dal `task/T342-giris-oturum`, PR #453; kriter 1–9, KK1–KK9, `09 §25` ve güvenlik taraması öz denetimde ✓; Unit 4276, Integration Membership 50 + komşu 114 + Web 594; D-114, D-133, D-251, D-255, D-364 çözüldü, D-127 T342 payı çözüldü; yeni D-428–D-432)

## 2026-11-08 — T302 (Öteki kayıtların süreyle imhası) doğrulama tur 1 PASS

2026-11-08 — T302 (Öteki kayıtların süreyle imhası) doğrulama tur 1 ✓ PASS (PR #446; kriter 1–9, KK1–KK9, `09 §25` ✓, güvenlik taraması temiz, bulgu 0, K 0; `VAL-D051` PASS; dal CI 42417352638 yeşil; `origin/main` (T321, #439) kod çakışması yalnız ekleme olarak çözüldü, birleşik ağaçta Unit 4123, Integration dokunulan sınıflar yeşil; backlog aktif 157). Üçüncü main birleştirmesi: `origin/main` (T341, #433) birleştirildi; `IOrderingModule` ve `OrderingModule` kurucusu iki tarafın eklemeleriyle birlikte korundu, Unit 4237, Integration RecordDisposal 13, OrderDisposal 27, Membership 34, OrderTransitions 31 yeşil; backlog aktif 157 aynı. Dördüncü main birleştirmesi: `origin/main` (T317 ödeme ekseni, #441) birleştirildi; `PaymentModule` kullanımları ve `OrderTransitionsTests` kurulumu (`OrderingTestServices.Build`) iki tarafın eklemeleriyle birlikte korundu, ödeme kodunun davranışı değişmedi; backlog aktif 158. Merge ve post-merge CI yardımcıda. Önceki kayıt `STATUS_CHANGELOG.md`'de.

## 2026-11-08 — T317 (Ödeme durumu geçişleri: ödeme ekseni) doğrulama tur 3 PASS

2026-11-08 — T317 (Ödeme durumu geçişleri: ödeme ekseni) doğrulama tur 3 ✓ PASS (head `24be2b5` + `origin/main` (`9bb46eb`, T339, #445) birleştirmesi, PR #441; kriter 1–8, KK1–KK8, `09 §25` ve güvenlik taraması ✓; tur 2 B1 giderildi — ödenmemiş iptalin tek yolu `CancelOrderAsync`, Ö1'in tek yolu ödeme ekseni (D-389); Unit Architecture 933 + Consistency 241 + öteki 2942, betik testleri, Integration yerelde PaymentTransitions 33, OrderTransitions 31, ShipmentTransitions 94, Disposal 45 ve 4 geçici yoklama; dal CI 42421489591 yeşil; bulgu 0; K1 bu PR'da kapatıldı (D-291 metni güncel yola ve test adına bağlandı); `12` `Durum` değişmedi (KP-18 satırları sayfa ve uç kanıtı ister); backlog aktif 154).

## 2026-11-08 — T341 (Üyelik: kayıt, e-posta doğrulama kapısı, misafir siparişinin hesaba bağlanması, hesap adı, giriş kaydı) doğrulama tur 3 ✓ PASS (FAIL sonrası tam yeniden doğrulama; head `b2c6cca`, PR #433; kriter 1–9, KK1–KK9 ve `09 §25` ✓, güvenlik taraması temiz; tur 2 Bulgu 1 giderildi — kayıt e-postası `InvalidEmailAttribute` ile denetlenir; Unit 4069 (Consistency 241), Integration MembershipTests 34/34 ve komşu üç sınıf 95/95; dal CI 42410595147 success; `VAL-D018`…`VAL-D020` `BEKLEMEDE` kaldı (uç yanıtı sonraki task'larda); K1 → D-385 (T509). `origin/main` (T334) birleştirildi: `Modules/Ordering` iki dosyasında iki tarafın üyeleri birlikte korundu; backlog aktif sayısı yeniden sayıldı: 153; ikinci main birleştirmesi: `origin/main` (T321, #439) de birleştirildi, `OrderingModule` kurucusu ve `IOrderingModule` üyeleri iki tarafın eklemeleriyle birlikte korundu).

## 2026-11-08 — T339 (Ürün görseli ve metni: ana görsel, kısıtlar, alternatif metin, metin biçimi seti) doğrulama tur 2 ✓ PASS (head `8a5b0a9` + `origin/main` (`a89710b`, T321, #439) birleştirmesi, PR #445; kriter 1–9, KK1–KK9, `09 §25` ve güvenlik taraması ✓; tur 1 B1 giderildi: birleşik ağaçta `ProductGalleryTests` 18/18; Unit 3032 + Architecture 943 + Consistency 241, betik testleri, Integration yerelde Gallery 18, Concurrency + SearchEngineData 44, GalleryLink + CatalogCore + SampleDatabase + StockAndCapacity 144, ImageFiles 14, VersionedFormPage + ProductTransitions 61/62 (düşen tek test D-392, T339 dışı); temizleyiciye ek 24 örneklik XSS yoklaması temiz; dal CI 42419057833 yeşil; bulgu 0, yeni K-notu yok; `12` `Durum` değişmedi (KP-5/KP-42 satırları uç ve tarayıcı kanıtı ister); backlog aktif 153).

## 2026-11-08 — T302 (Öteki kayıtların süreyle imhası) yapım tamamlandı

2026-11-08 — T302 (Öteki kayıtların süreyle imhası) yapım tamamlandı, doğrulama bekliyor (dal `task/T302-kayit-imha`, PR #446; kriter 1–9, KK1–KK9 öz denetimde ✓; Unit 4110 (Architecture 933, Consistency 242, Disposal 23), Integration RecordDisposal 13, Disposal 18, OrderDisposal 27, Files 81, Ordering 125, Concurrency 26, Model 1106, Database 36 yeşil; format temiz; şema yok; `12` `Durum` değişmedi; ⚠ gönderim kaydının kolu yalnız ödeme kaydından okunur, siparişi kilitlemez; ayıp talebi silmesi sipariş sürümünü artırmaz; gelen bildirim 10 yıl tasarım değeridir (09 §3.5 satırı eklendi); `VersionedFormPageTests` sınıf birlikte koşunca titriyor (D-378); backlog D-102 ✅, D-99 T302 payı ✅, yeni D-374…D-378, aktif 143). Önceki kayıt `STATUS_CHANGELOG.md`'de.
## 2026-11-08 — T317 (Ödeme durumu geçişleri: ödeme ekseni) doğrulama tur 3 ✓ PASS (head `24be2b5` + `origin/main` (`9bb46eb`, T339, #445) birleştirmesi, PR #441; kriter 1–8, KK1–KK8, `09 §25` ve güvenlik taraması ✓; tur 2 B1 giderildi — ödenmemiş iptalin tek yolu `CancelOrderAsync`, Ö1'in tek yolu ödeme ekseni (D-389); Unit Architecture 933 + Consistency 241 + öteki 2942, betik testleri, Integration yerelde PaymentTransitions 33, OrderTransitions 31, ShipmentTransitions 94, Disposal 45 ve 4 geçici yoklama; dal CI 42421489591 yeşil; bulgu 0; K1 bu PR'da kapatıldı (D-291 metni güncel yola ve test adına bağlandı); `12` `Durum` değişmedi (KP-18 satırları sayfa ve uç kanıtı ister); backlog aktif 154).

## 2026-11-08 — T341 (Üyelik: kayıt, e-posta doğrulama kapısı, misafir siparişinin hesaba bağlanması, hesap adı, giriş kaydı) doğrulama tur 3 PASS

2026-11-08 — T341 (Üyelik: kayıt, e-posta doğrulama kapısı, misafir siparişinin hesaba bağlanması, hesap adı, giriş kaydı) doğrulama tur 3 ✓ PASS (FAIL sonrası tam yeniden doğrulama; head `b2c6cca`, PR #433; kriter 1–9, KK1–KK9 ve `09 §25` ✓, güvenlik taraması temiz; tur 2 Bulgu 1 giderildi — kayıt e-postası `InvalidEmailAttribute` ile denetlenir; Unit 4069 (Consistency 241), Integration MembershipTests 34/34 ve komşu üç sınıf 95/95; dal CI 42410595147 success; `VAL-D018`…`VAL-D020` `BEKLEMEDE` kaldı (uç yanıtı sonraki task'larda); K1 → D-385 (T509). `origin/main` (T334) birleştirildi: `Modules/Ordering` iki dosyasında iki tarafın üyeleri birlikte korundu; backlog aktif sayısı yeniden sayıldı: 153; ikinci main birleştirmesi: `origin/main` (T321, #439) de birleştirildi, `OrderingModule` kurucusu ve `IOrderingModule` üyeleri iki tarafın eklemeleriyle birlikte korundu).

---

## 2026-11-08 — T333 (Kategori ağacı, site içi arama ve liste süzgeçleri) ikinci kod birleştirmesi (T339)

2026-11-08 — T333 (Kategori ağacı, site içi arama ve liste süzgeçleri) `origin/main` (T339, #445) ile ikinci kod birleştirmesi yapıldı ⏳ yeniden doğrulama bekliyor (PR #442; `CatalogModule`, `ICatalogModule` ve `SitemapSourceTests` çakışmaları iki tarafın eklemesiyle çözüldü, `CategoryTreeAndListsTests` tohumu main'in özel `Description` ayarlayıcısı için `SetDescription` çağırır; birleşik ağaçta Integration `CategoryTreeAndListsTests` 33/33, `ProductGalleryTests` 18/18, `ProductTransitionsTests` 41/41; Unit Architecture 946, Consistency 241, kalan 3044; `build -warnaserror` ve `dotnet format` temiz; backlog aktif kalem 158). Önceki kayıtlar `STATUS_CHANGELOG.md`'de.

---

## 2026-11-08 — T339 (Ürün görseli ve metni: ana görsel, kısıtlar, alternatif metin, metin biçimi seti) doğrulama tur 2 PASS

2026-11-08 — T339 (Ürün görseli ve metni: ana görsel, kısıtlar, alternatif metin, metin biçimi seti) doğrulama tur 2 ✓ PASS (head `8a5b0a9` + `origin/main` (`a89710b`, T321, #439) birleştirmesi, PR #445; kriter 1–9, KK1–KK9, `09 §25` ve güvenlik taraması ✓; tur 1 B1 giderildi: birleşik ağaçta `ProductGalleryTests` 18/18; Unit 3032 + Architecture 943 + Consistency 241, betik testleri, Integration yerelde Gallery 18, Concurrency + SearchEngineData 44, GalleryLink + CatalogCore + SampleDatabase + StockAndCapacity 144, ImageFiles 14, VersionedFormPage + ProductTransitions 61/62 (düşen tek test D-392, T339 dışı); temizleyiciye ek 24 örneklik XSS yoklaması temiz; dal CI 42419057833 yeşil; bulgu 0, yeni K-notu yok; `12` `Durum` değişmedi (KP-5/KP-42 satırları uç ve tarayıcı kanıtı ister); backlog aktif 153). Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-08 — T333 (Kategori ağacı, site içi arama ve liste süzgeçleri) kod birleştirmesi (T321, T334)

2026-11-08 — T333 (Kategori ağacı, site içi arama ve liste süzgeçleri) `origin/main` (T321, T334) ile kod birleştirmesi yapıldı ⏳ yeniden doğrulama bekliyor (PR #442; kurucu çakışması iki tarafın eklemesiyle çözüldü, vitrin görünürlüğü `ShowcaseVisibility.Listed()` tek yerine bağlandı, backlog aktif kalem 150; önceki durum: düzeltme 2 bitti, doğrulama tur 3 bekliyordu; tur 2 ✗ FAIL B1 ve B2 (DÜŞÜK S1) giderildi: B1 `ProductListReader` sayfayı artık yalnız kartın sütunlarıyla `Select` projeksiyonuyla okuyor (`init` üyeli özel sınıf; sıra `CreatedAt` azalan, `Id` azalan aynı), D-134 ✅; B2 `TheListsQueryCount_DoesNotGrowWithThePageFill_…` komut yakalayıcıyla 1 ürünlü ve dolu sayfada sorgu sayısının eşitliğini sınar; `origin/main` (T334) birleşince tohum uyarlandı (Physical varyantta stok sayısı zorunlu, sınırsız varyant için Digital ürün); Integration `CategoryTreeAndListsTests` 31/31 + iki sınır testi ayrı 1/1 + 1/1; Unit Architecture 936, Consistency 241, Catalog 61; backlog aktif kalem 144; `12` değişmedi). Önceki kayıtlar `STATUS_CHANGELOG.md`'de.
## 2026-11-08 — T341 (Üyelik) doğrulama tur 3 PASS

2026-11-08 — T341 (Üyelik: kayıt, e-posta doğrulama kapısı, misafir siparişinin hesaba bağlanması, hesap adı, giriş kaydı) doğrulama tur 3 ✓ PASS (FAIL sonrası tam yeniden doğrulama; head `b2c6cca`, PR #433; kriter 1–9, KK1–KK9 ve `09 §25` ✓, güvenlik taraması temiz; tur 2 Bulgu 1 giderildi — kayıt e-postası `InvalidEmailAttribute` ile denetlenir; Unit 4069 (Consistency 241), Integration MembershipTests 34/34 ve komşu üç sınıf 95/95; dal CI 42410595147 success; `VAL-D018`…`VAL-D020` `BEKLEMEDE` kaldı (uç yanıtı sonraki task'larda); K1 → D-385 (T509). `origin/main` (T334) birleştirildi: `Modules/Ordering` iki dosyasında iki tarafın üyeleri birlikte korundu; backlog aktif sayısı yeniden sayıldı: 153; ikinci main birleştirmesi: `origin/main` (T321, #439) de birleştirildi, `OrderingModule` kurucusu ve `IOrderingModule` üyeleri iki tarafın eklemeleriyle birlikte korundu).

## 2026-11-08 — T339 (Ürün görseli ve metni: ana görsel, kısıtlar, alternatif metin, metin biçimi seti) doğrulama tur 2 PASS

2026-11-08 — T339 (Ürün görseli ve metni: ana görsel, kısıtlar, alternatif metin, metin biçimi seti) doğrulama tur 2 ✓ PASS (head `8a5b0a9` + `origin/main` (`a89710b`, T321, #439) birleştirmesi, PR #445; kriter 1–9, KK1–KK9, `09 §25` ve güvenlik taraması ✓; tur 1 B1 giderildi: birleşik ağaçta `ProductGalleryTests` 18/18; Unit 3032 + Architecture 943 + Consistency 241, betik testleri, Integration yerelde Gallery 18, Concurrency + SearchEngineData 44, GalleryLink + CatalogCore + SampleDatabase + StockAndCapacity 144, ImageFiles 14, VersionedFormPage + ProductTransitions 61/62 (düşen tek test D-392, T339 dışı); temizleyiciye ek 24 örneklik XSS yoklaması temiz; dal CI 42419057833 yeşil; bulgu 0, yeni K-notu yok; `12` `Durum` değişmedi (KP-5/KP-42 satırları uç ve tarayıcı kanıtı ister); backlog aktif 153).

## 2026-11-08 — T317 (Ödeme durumu geçişleri: ödeme ekseni) düzeltme turu 3 yapıldı

2026-11-08 — T317 (Ödeme durumu geçişleri: ödeme ekseni) düzeltme turu 3 yapıldı (PR #441; tur 2 B1 giderildi: ödenmemiş iptalin tek yolu T316'nın `CancelOrderAsync`'i — `CancelUnpaidOrderAsync`, `UnpaidOrderCancellation` ve kendi denetimi kaldırıldı, KK4 testleri `CancelOrderAsync` üzerinden; onaysız yönetici iptali `ConfirmationRequired`, "Diğer" dışı notlu / "Diğer" notsuz yönetici iptali `InvalidInput` sınandı; K1/D-389 kapandı: `StartPreparationAsync` yalnız S1, Ö1'in tek yolu ödeme kaydı yazan `ApplyCardPaymentAsync`/`MarkTransferPaidAsync`, D-307 T318'e daraldı; `origin/main` `a89710b` (T321) birleştirildi, kayıt çakışmaları çözüldü; birleşik ağaçta build `-warnaserror` 0/0, format temiz, Unit 4116, Integration Payment 33, Shipment + OrderTransitions 125, Disposal + Concurrency 71 (postgres:18); backlog aktif 146). Doğrulama tur 3 bekliyor.

## 2026-11-08 — T317 (Ödeme durumu geçişleri: ödeme ekseni) doğrulama tur 2 FAIL

2026-11-08 — T317 (Ödeme durumu geçişleri: ödeme ekseni) doğrulama tur 2 ✗ FAIL (head `66bcf16` — T316 ile kod birleştirmesi, `origin/main` `af6e7d3` güncel, PR #441; kriter 1–8 ve KK1–KK8 ✓, `09 §25` ✗, güvenlik taraması temiz; B1 (ORTA, S1): birleştirmeden sonra ödenmemiş iptalin iki açık yolu var, `CancelUnpaidOrderAsync`'in yönetici yolu firma iptalini (S3) geri alınamaz onaysız ve "Diğer ⇔ not" kuralı denetlenmeden yazar (`02 §5.9`, `03 §1.6.1.3`, `06 §4.19`); K1 → D-389 (Ö1'in iki yolu, D-307); Unit 3862 + Consistency 241, betik testleri, Integration Payment+OrderTransitions 60, Shipment 94, Disposal 45; dal CI 42414305022 yeşil; `12` `Durum` değişmedi; backlog aktif 141).
## 2026-11-08 — T339 doğrulama tur 2 ✓ PASS (PR #445)

T339 (Ürün görseli ve metni: ana görsel, kısıtlar, alternatif metin, metin biçimi seti) doğrulama tur 2 ✓ PASS (head `8a5b0a9` + `origin/main` (`a89710b`, T321, #439) birleştirmesi, PR #445; kriter 1–9, KK1–KK9, `09 §25` ve güvenlik taraması ✓; tur 1 B1 giderildi: birleşik ağaçta `ProductGalleryTests` 18/18; Unit 3032 + Architecture 943 + Consistency 241, betik testleri, Integration yerelde Gallery 18, Concurrency + SearchEngineData 44, GalleryLink + CatalogCore + SampleDatabase + StockAndCapacity 144, ImageFiles 14, VersionedFormPage + ProductTransitions 61/62 (düşen tek test D-392, T339 dışı); temizleyiciye ek 24 örneklik XSS yoklaması temiz; dal CI 42419057833 yeşil; bulgu 0, yeni K-notu yok; `12` `Durum` değişmedi (KP-5/KP-42 satırları uç ve tarayıcı kanıtı ister); backlog aktif 153). Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-08 — T339 (Ürün görseli ve metni: ana görsel, kısıtlar, alternatif metin, metin biçimi seti) düzeltme turu 1 yapıldı

2026-11-08 — T339 (Ürün görseli ve metni: ana görsel, kısıtlar, alternatif metin, metin biçimi seti) düzeltme turu 1 yapıldı, doğrulama tur 2 bekliyor (PR #445; `origin/main` (`af6e7d3`, T334) dala birleştirildi; B1 giderildi: `ProductGalleryTests.cs` üç tohum `Variant.Create(..., Now, 100)` ile stoklu, ürün koduna dokunulmadı; K2 → D-384 kapatıldı (test adı `…WhenTheFormMarksNone`), K1 → D-383 açık; kayıt belgesi çakışmaları çözüldü; backlog aktif 147; yeni K-notları D-391, D-392).

---

## 2026-11-08 — T339 (Ürün görseli ve metni: ana görsel, kısıtlar, alternatif metin, metin biçimi seti) doğrulama tur 1 FAIL

2026-11-08 — T339 (Ürün görseli ve metni: ana görsel, kısıtlar, alternatif metin, metin biçimi seti) doğrulama tur 1 ✗ FAIL (head `7d65caf`, PR #445; dalın kendi tabanında kriter 1–9, KK1–KK9, `09 §25` ve güvenlik taraması ✓, dal CI 42411851018 yeşil, Unit 922 + 241 + 2996, Integration `ProductGalleryTests` 18, Concurrency 26, `SearchEngineDataTests` 18; B1 ORTA S2: `origin/main` (`af6e7d3`, T334) birleştirilince `ProductGalleryTests` 14/18 düşer — T334'ün `VariantStockGuard`'ı fiziksel varyantta stok numarası ister, testin üç tohumu stoksuz varyant kurar; birleştirme push edilmedi; düzeltme: birleştir + `Variant.Create(..., Now, 100)`; K1 → D-383, K2 → D-384; `12` `Durum` değişmedi (satırlar uç yanıtı ve tarayıcı gözlemi ister — T503, T525); backlog aktif 142).

---

## 2026-11-07 — T321 (Ürün yayın durumları) doğrulama tur 3 PASS

2026-11-07 — T321 (Ürün yayın durumları) doğrulama tur 3 ✓ PASS (head `c5b9afd` + `origin/main` (T316, #437) birleştirmesi, PR #439; kriter 1–9, KK1–KK9 ve `09 §25` ✓, güvenlik taraması temiz; tur 2 Bulgu 1 — `IOrderingModule` sepet metotlarının kendi `SaveChangesAsync`'i — giderildi: iki metot yalnız izlenen değişiklik bırakır, kaydı `ITransactionRunner` yapar (`TheCartMethods_DoNotSaveOnTheirOwn_...KK7`); bulgu 0, yeni K-notu 0; dal CI 42406697716 yeşil; yerelde derleme `-warnaserror` 0 uyarı, biçim üç proje temiz, Unit 4062/4062 (Architecture ve Consistency dahil), betik testleri 4/4, Integration `ProductTransitionsTests` 41, Sitemap + SearchEngine + Scheduler + ContentStatus 65, OrderTransitions + Concurrency 57 (postgres:18); `12` KP-7/KP-40 satırları uç yanıtı, panel ya da adres sayfası ister → `BEKLEMEDE` kalır; kayıt belgeleri çakışması `origin/main` ile çözüldü, backlog Aktif kalem 145). Merge ve post-merge CI yardımcıda.

## 2026-11-07 — T317 (Ödeme durumu geçişleri: ödeme ekseni) doğrulama tur 1 PASS

2026-11-07 — T317 (Ödeme durumu geçişleri: ödeme ekseni) doğrulama tur 1 ✓ PASS (head `0926ba7`, `origin/main` `c4c872c` güncel, PR #441; kriter 1–8, KK1–KK8, `09 §25` ✓, güvenlik taraması temiz; Unit 3965 (Architecture 906, Consistency 230, öteki 2829), betik testleri, Integration yerelde Ordering 60, Concurrency 26, PaymentRecordTables + JobCleanupAndRefund 92, web açılışı 18; dal CI 42396093293 yeşil; bulgu 0, K-notu 1 (D-322 → T370); `12` `Durum` değişmedi (KP-18 satırları sipariş sayfası kanıtı ister); backlog aktif 140).
## 2026-11-07 — T334 (Stok ve hizmet kontenjanı: takip, tükenme, ayrılmış adedin altına inmeme) doğrulama tur 1 PASS

2026-11-07 — T334 (Stok ve hizmet kontenjanı: takip, tükenme, ayrılmış adedin altına inmeme) doğrulama tur 1 ✓ PASS (head `2b3e600` + `origin/main` (T328) birleştirmesi, PR #443; kriter 1–8, KK1–KK8 ve `09 §25` ✓, güvenlik taraması temiz; Unit 4069 (birleştirmeden önce), 4093 (sonra); Integration yerelde StockAndCapacity 27 ve dokunulan sınıflar; dal CI 42403928586 yeşil; bulgu 0, K-notu 1 → D-362 (T525); `12` `Durum` değişmedi; backlog aktif sayısı yeniden sayıldı: 139).
2026-11-07 — T334 (Stok ve hizmet kontenjanı: takip, tükenme, ayrılmış adedin altına inmeme) doğrulama tur 1 ✓ PASS (head `2b3e600` + `origin/main` (T328) birleştirmesi, PR #443; kriter 1–8, KK1–KK8 ve `09 §25` ✓, güvenlik taraması temiz; Unit 4069 (birleştirmeden önce), 4093 (sonra); betik testleri; Integration yerelde StockAndCapacity 27, CatalogCore+SalesModel 88, Concurrency+ImageFiles+CartAndOrderCore+StockReservation 133, AppCache+SearchEngineData+VersionedFormPage 53 (yapım raporundaki `VersionedFormPageTests` düşmesi yinelenmedi), birleştirme sonrası NotificationMapQueue+SalesModel 31; dal CI 42403928586 yeşil; bulgu 0, K-notu 1 → D-362 (T525); `12` `Durum` değişmedi (satırların kanıtı uç yanıtı, tarayıcı gözlemi ya da ödeme onayı — T503, T525, T348); backlog aktif sayısı yeniden sayıldı: 139). Önceki kayıt `STATUS_CHANGELOG.md`'de.

## 2026-11-07 — T341 (Üyelik) doğrulama tur 2 FAIL

2026-11-07 — T341 (Üyelik: kayıt, e-posta doğrulama kapısı, misafir siparişinin hesaba bağlanması, hesap adı, giriş kaydı) doğrulama tur 2 ✗ FAIL (CI düzeltmesi sonrası tam yeniden doğrulama; head `2aa0153`, PR #433; kriter 1–6, 8, 9 ve KK1–KK9 ✓, kriter 7 ~ Kısmi; `09 §25` bir "hayır": Bulgu 1 ORTA S1 — `MembershipModule.RegisterAsync` e-posta biçimini `04` 7.3.2.1'in tek kuralı `InvalidEmailAttribute` yerine çatının `MailAddress` ayrıştırıcısıyla kurulan ayrı bir `IsValidEmail` ile denetliyor (`09 §3.4` m.1, §24 m.4); iki kural iki yönde ayrışıyor (`a..b@example.com` kuralın kabul ettiği adres kayıtta red, 70 karakterlik yerel kısım, `_`'lı ve `-` ile başlayan alan adı, boşluklu tırnaklı yerel kısım kayıtta kabul); build `-warnaserror` 0 uyarı, format üç proje temiz, Unit 4037 (Architecture 918, Consistency 230, öteki 2889), betik testleri, Integration yerelde MembershipTests 29 + komşu dört sınıf 109; dal CI 42404071089 yeşil; K-notu 2: K1 D-364 (`09 §15.1` Test: hesap yokken sahte özetle doğrulamanın sayaçlı dekoratör testi yok → T342), K2 D-365 (`PasswordService.IterationCount` 210000 `09`'da yazılı değil → F3 gate); `12` `VAL-D018`…`VAL-D020` `BEKLEMEDE` kaldı; `origin/main` (T328) birleştirildi; backlog aktif 136). Düzeltme r2 yapıldı (Bulgu 1: `IsValidEmail` kaldırıldı, kayıt e-postası `InvalidEmailAttribute` ile; `a..b@example.com` kabul, 70 karakterlik yerel kısım, `_`'lı ve `-` ile başlayan alan adı, boşluklu tırnaklı yerel kısım ret testleri; Integration MembershipTests 34, AccountEmailQueueTests 14; D-379 gönderim tarafı; backlog aktif 142), tur 3 bekliyor. Önceki kayıt `STATUS_CHANGELOG.md`'de.

## 2026-11-07 — T316 (Sipariş durumu geçişleri: sevkiyat ekseni) doğrulama tur 2 PASS

2026-11-07 — T316 (Sipariş durumu geçişleri: sevkiyat ekseni) doğrulama tur 2 ✓ PASS (head `2d04809` + `origin/main` birleştirmesi, PR #437; kriter 1–9, KK1–KK9 ve `09 §25` ✓, güvenlik taraması temiz; tur 1 bulgusu B1 giderildi ve S3, S4, S9 için sınandı; bulgu 0, K-notu 0; dal CI 42399324842 yeşil; Unit Architecture 906 + Consistency 230 + öteki 2840, Integration `ShipmentTransitionsTests` 94, `OrderTransitionsTests` 31, `ConcurrencyTests` 26; `12` KP-46 satırları `BEKLEMEDE` kalır). Merge ve post-merge CI yardımcıda.

## 2026-11-07 — T328 (Bildirim haritası) doğrulama tur 1 PASS

2026-11-07 — T328 (Bildirim haritası: tetikleyici eşlemesi, bildirim üretmeyen olaylar ve eşitlik denetimi) doğrulama tur 1 ✓ PASS (head `0edb3bd`, PR #440; kriter 1–7, KK1–KK7 ve `09 §25` ✓, güvenlik taraması temiz; KK7 iki mutasyonla (kodda fazla satır, belgede eksik akış atfı) kırıldığı görüldü; Unit Consistency 241, öteki 3746; betik testleri; Integration yerelde NotificationMapQueue 3, komşu kuyruk sınıfları 47; dal CI 42394956374 yeşil; bulgu 0, K-notu 1 (D-295 sayımı ve backlog aktif sayısı bu PR'da düzeltildi); `12` `Durum` değişmedi (kaynak birimi `03 §7.1`–§7.3'e bağlı satır yok); backlog aktif 130).

## 2026-11-07 — T341 (Üyelik) doğrulama tur 1 PASS

2026-11-07 — T341 (Üyelik: kayıt, e-posta doğrulama kapısı, misafir siparişinin hesaba bağlanması, hesap adı, giriş kaydı) doğrulama tur 1 ✓ PASS (head `391cdc3` + `origin/main` birleştirmesi, PR #433; kriter 1–9, KK1–KK9 ve `09 §25` ✓ (ekran, POST ve sipariş ucu yüzleri modül düzeyinde; bağlar D-254), güvenlik taraması temiz; build `-warnaserror` 0 uyarı, format üç proje temiz, Unit 3534 (Architecture 842, Consistency 213, öteki 2479), betik testleri, Integration yerelde MembershipTests 29, SessionLinkAttemptLoginTables + LinkRecords + AccountEmailQueue 95, AppCache 14; dal CI 42378956325 yeşil; bulgu 0; K-notu 3: K1 D-252 hedefi T309 → T514, K2 D-289 (P-12 iki sabitte; CI düzeltmesinde tek sabite indi, ✅), K3 D-290 (`09 §15.1` m.5 metni); `12` `VAL-D018`…`VAL-D020` `BEKLEMEDE` kaldı (sipariş geçmişinin uç yanıtı sonraki task'larda); CI düzeltmesi: main'e giren T381 yüzünden `GateOnlySettings` yeni üyeleri uyguladı ve P-12 için tek sabit `EmailVerificationLinkHours` kullanıldı; backlog aktif 132).
---

## 2026-11-07 — T334 (Stok ve hizmet kontenjanı: takip, tükenme, ayrılmış adedin altına inmeme) doğrulama tur 1 PASS

2026-11-07 — T334 (Stok ve hizmet kontenjanı: takip, tükenme, ayrılmış adedin altına inmeme) doğrulama tur 1 ✓ PASS (head `2b3e600` + `origin/main` (T328) birleştirmesi, PR #443; kriter 1–8, KK1–KK8 ve `09 §25` ✓, güvenlik taraması temiz; Unit 4069 (birleştirmeden önce), 4093 (sonra); betik testleri; Integration yerelde StockAndCapacity 27, CatalogCore+SalesModel 88, Concurrency+ImageFiles+CartAndOrderCore+StockReservation 133, AppCache+SearchEngineData+VersionedFormPage 53 (yapım raporundaki `VersionedFormPageTests` düşmesi yinelenmedi), birleştirme sonrası NotificationMapQueue+SalesModel 31; dal CI 42403928586 yeşil; bulgu 0, K-notu 1 → D-362 (T525); `12` `Durum` değişmedi (satırların kanıtı uç yanıtı, tarayıcı gözlemi ya da ödeme onayı — T503, T525, T348); backlog aktif sayısı yeniden sayıldı: 139).

---

## 2026-11-07 — T339 (Ürün görseli ve metni: ana görsel, kısıtlar, alternatif metin, metin biçimi seti) yapım bitti

2026-11-07 — T339 (Ürün görseli ve metni: ana görsel, kısıtlar, alternatif metin, metin biçimi seti) yapım tamamlandı, doğrulama bekliyor (dal `task/T339-urun-gorsel-metin`, PR #445; kriter 1–9, KK1–KK9 öz denetimde ✓; Unit 4159 (Architecture + Consistency dahil), Integration `ProductGalleryTests` 18, Model 690, Concurrency 26, `VersionedFormPageTests` 21, `SearchEngineDataTests` 18; derleme `-warnaserror` ve biçim üç proje temiz; şema yok; yeni bağımlılık `HtmlSanitizer` 10.0.1120 (K-1577, 09 tablosu 9.0.892 yerine; AngleSharp açığı nedeniyle); kapı: kurumsal uzun metin ve yasal metin yazım işlevleri temizleyiciyi o task'larda çağıracak (D-360, D-219); yeni backlog D-357…D-361, Aktif kalem 140).

---
## 2026-11-07 — T334 (Stok ve hizmet kontenjanı) doğrulama tur 1 PASS

2026-11-07 — T334 (Stok ve hizmet kontenjanı: takip, tükenme, ayrılmış adedin altına inmeme) doğrulama tur 1 ✓ PASS (head `2b3e600` + `origin/main` (T328) birleştirmesi, PR #443; kriter 1–8, KK1–KK8 ve `09 §25` ✓, güvenlik taraması temiz; Unit 4069 (birleştirmeden önce), 4093 (sonra); betik testleri; Integration yerelde StockAndCapacity 27, CatalogCore+SalesModel 88, Concurrency+ImageFiles+CartAndOrderCore+StockReservation 133, AppCache+SearchEngineData+VersionedFormPage 53 (yapım raporundaki `VersionedFormPageTests` düşmesi yinelenmedi), birleştirme sonrası NotificationMapQueue+SalesModel 31; dal CI 42403928586 yeşil; bulgu 0, K-notu 1 → D-362 (T525); `12` `Durum` değişmedi (satırların kanıtı uç yanıtı, tarayıcı gözlemi ya da ödeme onayı — T503, T525, T348); backlog aktif sayısı yeniden sayıldı: 139). Önceki kayıt `STATUS_CHANGELOG.md`'de.

## 2026-11-07 — T333 (Kategori ağacı, site içi arama ve liste süzgeçleri) doğrulama tur 2 FAIL

2026-11-07 — T333 (Kategori ağacı, site içi arama ve liste süzgeçleri) doğrulama tur 2 ✗ FAIL (head `d42a5a4` + `origin/main` birleştirmesi, PR #442; kriter 1–8 ve KK1–KK8 ✓, tur 1 B1 düzeltmesi doğrulandı (iki listede sayfa sınırı testleri yeşil); güvenlik taraması temiz; Unit 4064, Integration `CategoryTreeAndListsTests` 32/32 + `ConcurrencyTests` 26/26; dal CI 42406747948 yeşil). **Bulgular:** B1 DÜŞÜK S1 — `09 §25` Performans "okuma sorgusu projeksiyonlu mu" hayır: `ProductListReader` sayfayı `Product` varlığının bütün sütunlarıyla (açıklama gövdesi dahil) okur, `09 §20` madde 3 yalnız gösterilen sütunların `Select` ile alınmasını ister (D-134 bunu T333'e yönlendirmişti); B2 DÜŞÜK S1 — `09 §20` madde 4: varyant rakamlarını yükleyen liste için sorgu sayısının sayfadaki ürün sayısından bağımsız olduğunu komut yakalayıcıyla sınayan entegrasyon testi yok. K1 → D-372 (ürün–kategori bağı değişince menü/ağaç önbellek alanı bildirimi, T361). Düzeltme 2 gerekli: B1 + B2 (ve D-134 kapanışı); iterasyon 1/2 kullanıldı (`12 §8.5`).
## 2026-11-07 — T321 (Ürün yayın durumları) düzeltme 2 yapıldı

2026-11-07 — T321 (Ürün yayın durumları) düzeltme 2 yapıldı (tur 2 Bulgu 1: `IOrderingModule.RemoveFromCartsAsync` ve `TakeRemovedItemNoticesAsync` artık `SaveChangesAsync` çağırmaz — kaydı işlemin sahibi (`ITransactionRunner`) yapar, `09 §6` m.3, m.7; yeni test `TheCartMethods_DoNotSaveOnTheirOwn_TheTransactionOwnerDoes_KK7`; `TakeNoticesAsync` test yardımcısı `ITransactionRunner` üzerinden; `origin/main` (T328, #440) birleştirildi; 11'deki kriterler ve KK1–KK9 düzeltilmiş diff'te yeniden denetlendi; doğrulama tur 3 bekliyor; backlog aktif 136). Önceki kayıt `STATUS_CHANGELOG.md`'de.

## 2026-11-07 — T316 (Sipariş durumu geçişleri: sevkiyat ekseni) doğrulama tur 2 PASS

2026-11-07 — T316 (Sipariş durumu geçişleri: sevkiyat ekseni) doğrulama tur 2 ✓ PASS (head `2d04809` + `origin/main` birleştirmesi, PR #437; kriter 1–9, KK1–KK9 ve `09 §25` ✓, güvenlik taraması temiz; tur 1 bulgusu B1 — yöneticinin bütün sipariş iptali teslim işaretli kalemi denetlemiyordu — giderildi ve S3, S4, S9 için sınandı; bulgu 0, K-notu 0; dal CI 42399324842 yeşil; yerelde derleme `-warnaserror` 0 uyarı, biçim üç proje temiz, Unit Architecture 906 + Consistency 230 + öteki 2840, betik testleri 4/4, Integration `ShipmentTransitionsTests` 94, `OrderTransitionsTests` 31, `ConcurrencyTests` 26 (postgres:18); `12` KP-46 satırları uç yanıtı ister → `BEKLEMEDE` kalır; kayıt belgeleri çakışması `origin/main` (T322) ile çözüldü, backlog Aktif kalem 135). Merge ve post-merge CI yardımcıda.

## 2026-11-07 — T333 (Kategori ağacı, site içi arama ve liste süzgeçleri) düzeltme 1 bitti

2026-11-07 — T333 (Kategori ağacı, site içi arama ve liste süzgeçleri) düzeltme 1 bitti ⏳ doğrulama tur 2 bekliyor (PR #442; tur 1 ✗ FAIL B1 DÜŞÜK S1: kategori sayfası ve arama listesi için eşit `CreatedAt`'li ürünleri sayfa sınırına düşüren test yoktu → eklendi: iki liste için gerçek okuyucuyla bütün sayfalar yürünür, yinelenme ve kayıp yok, son sayfa dahil, süzgeçli koşular dahil; arama sözcüğündeki ters bölü harfi harfine aranır; üretim kodu değişmedi; `origin/main` birleştirildi; build, format ×3, Unit 4064, Integration `CategoryTreeAndListsTests` 32 + `ConcurrencyTests` 26 yerelde yeşil; `12` `Durum` değişmedi; backlog aktif 135). Önceki kayıt `STATUS_CHANGELOG.md`'de.

## 2026-11-07 — T328 (Bildirim haritası) doğrulama tur 1 PASS

2026-11-07 — T328 (Bildirim haritası: tetikleyici eşlemesi, bildirim üretmeyen olaylar ve eşitlik denetimi) doğrulama tur 1 ✓ PASS (head `0edb3bd`, PR #440; kriter 1–7, KK1–KK7 ve `09 §25` ✓, güvenlik taraması temiz; KK7 iki mutasyonla (kodda fazla satır, belgede eksik akış atfı) kırıldığı görüldü; Unit Consistency 241, öteki 3746; betik testleri; Integration yerelde NotificationMapQueue 3, komşu kuyruk sınıfları 47; dal CI 42394956374 yeşil; bulgu 0, K-notu 1 (D-295 sayımı ve backlog aktif sayısı bu PR'da düzeltildi); `12` `Durum` değişmedi (kaynak birimi `03 §7.1`–§7.3'e bağlı satır yok); backlog aktif 130).

## 2026-11-07 — T333 (Kategori ağacı, site içi arama ve liste süzgeçleri) doğrulama tur 1 FAIL

2026-11-07 — T333 doğrulama tur 1 ✗ FAIL (head `62d03a8` + `origin/main` birleştirmesi, PR #442; kriter 1–8 ve KK1–KK8 ✓, güvenlik taraması temiz; `09 §25` "yeni sayfalı liste sınır testi" HAYIR → B1 DÜŞÜK S1: iki yeni sayfalı liste (kategori sayfası, arama) için eşit `CreatedAt`'li ürünleri sayfa sınırına düşüren test yok (`09 §10.5` madde 6, §19.2 madde 5); build, format, Unit 3983, betik testleri, Integration `CategoryTreeAndListsTests` 29 + `ConcurrencyTests` 26 yerelde yeşil, dal CI 42401392964 yeşil; `12` `Durum` değişmedi; K1 rapor cümlesi düzeltildi; backlog aktif 133).

## 2026-11-07 — T333 (Kategori ağacı, site içi arama ve liste süzgeçleri) yapım bitti

2026-11-07 — T333 (Kategori ağacı, site içi arama ve liste süzgeçleri) yapım bitti ⏳ doğrulama bekliyor (dal `task/T333-kategori-arama-suzgec`; kriter 1–8 ve KK1–KK8 öz denetimde ✓; Unit 3983, Integration `CategoryTreeAndListsTests` 29, Model ad alanı 1105; şema değişmedi; backlog D-330…D-334, aktif 130; T332 Commit hücresi `c4c872c`).
## 2026-11-07 — T328 (Bildirim haritası) doğrulama tur 1 PASS

2026-11-07 — T328 (Bildirim haritası: tetikleyici eşlemesi, bildirim üretmeyen olaylar ve eşitlik denetimi) doğrulama tur 1 ✓ PASS (head `0edb3bd`, PR #440; kriter 1–7, KK1–KK7 ve `09 §25` ✓, güvenlik taraması temiz; KK7 iki mutasyonla (kodda fazla satır, belgede eksik akış atfı) kırıldığı görüldü; Unit Consistency 241, öteki 3746; betik testleri; Integration yerelde NotificationMapQueue 3, komşu kuyruk sınıfları 47; dal CI 42394956374 yeşil; bulgu 0, K-notu 1 (D-295 sayımı ve backlog aktif sayısı bu PR'da düzeltildi); `12` `Durum` değişmedi (kaynak birimi `03 §7.1`–§7.3'e bağlı satır yok); backlog aktif 130). Önceki kayıt `STATUS_CHANGELOG.md`'de.

## 2026-11-07 — T321 (Ürün yayın durumları) doğrulama tur 2 FAIL

2026-11-07 — T321 (Ürün yayın durumları) doğrulama tur 2 ✗ FAIL (head `921491a` + `origin/main` (T322, #436) birleştirmesi, PR #439; kriter 1–9 ve KK1–KK9 ✓, tur 1'in iki bulgusu kapandı; `09 §25` bir "hayır": Bulgu 1 DÜŞÜK S1 — `IOrderingModule` arayüz metotları `RemoveFromCartsAsync` ve `TakeRemovedItemNoticesAsync` `SaveChangesAsync` çağırıyor (`09 §6` m.3, m.7); güvenlik taraması temiz; Unit Consistency 230 + Architecture 906 + öteki 2845, betik testleri, Integration yerelde ProductTransitions 40, Catalog + Ordering + Concurrency 97, Sitemap + SearchEngineData + Scheduler 31, Web host 77 (birleştirme öncesi `921491a`); dal CI 42399951671 yeşil; K-notu 1 (D-353 → T502: sayfalı liste projeksiyonsuz); `12` `Durum` değişmedi (KP-7/KP-40 satırları uç ve panel kanıtı ister); iki iterasyon sınırı doldu — eskalasyon (`12 §8.5`); backlog aktif 134). Önceki kayıt `STATUS_CHANGELOG.md`'de.

## 2026-11-07 — T321 (Ürün yayın durumları) doğrulama tur 1 FAIL, düzeltme yapıldı

2026-11-07 — T321 (Ürün yayın durumları) doğrulama tur 1 ✗ FAIL (head `8d71d89`, PR #439; kriter 1–9 ve KK1–KK9 ✓, `09 §25` iki "hayır": Bulgu 1 DÜŞÜK S1 — yeni sayfalı vitrin listesi `ShowcaseReader.ListAsync` sınır testi olmadan eklendi (`09 §19.2` m.5, §10.5 m.6); Bulgu 2 DÜŞÜK S1 — `OrderingModule.TakeRemovedItemNoticesAsync` `DbUpdateConcurrencyException`'ı kayıt varyantı dışında ret olarak yakalıyor (`09 §10.2` m.2); güvenlik taraması temiz; Unit Architecture 906 + öteki 3070, betik testleri, Integration yerelde ProductTransitions 35 + SitemapEndpoint 10; dal CI 42394867534 yeşil; K-notu yok; `12` `Durum` değişmedi (KP-7/KP-40 satırları uç ve panel kanıtı ister); backlog aktif 130). Düzeltme yapıldı (iki bulgu: vitrin listesi sınır testleri, `TakeRemovedItemNoticesAsync` sepet kilidi altında ve yakalamasız; Integration ProductTransitions 40, Unit 906 + 3075), doğrulama tur 2 bekliyor.
---

## 2026-11-07 — T328 (Bildirim haritası: tetikleyici eşlemesi, bildirim üretmeyen olaylar ve eşitlik denetimi) doğrulama tur 1 PASS

2026-11-07 — T328 (Bildirim haritası: tetikleyici eşlemesi, bildirim üretmeyen olaylar ve eşitlik denetimi) doğrulama tur 1 ✓ PASS (head `0edb3bd`, PR #440; kriter 1–7, KK1–KK7 ve `09 §25` ✓, güvenlik taraması temiz; KK7 iki mutasyonla (kodda fazla satır, belgede eksik akış atfı) kırıldığı görüldü; Unit Consistency 241, öteki 3746; betik testleri; Integration yerelde NotificationMapQueue 3, komşu kuyruk sınıfları 47; dal CI 42394956374 yeşil; bulgu 0, K-notu 1 (D-295 sayımı ve backlog aktif sayısı bu PR'da düzeltildi); `12` `Durum` değişmedi (kaynak birimi `03 §7.1`–§7.3'e bağlı satır yok); backlog aktif 130).

## 2026-11-07 — T334 (Stok ve hizmet kontenjanı: takip, tükenme, ayrılmış adedin altına inmeme) yapım bitti

2026-11-07 — T334 (Stok ve hizmet kontenjanı: takip, tükenme, ayrılmış adedin altına inmeme) yapım tamamlandı, doğrulama bekliyor (dal `task/T334-stok-kontenjan`; kriter 1–8, KK1–KK8 öz denetimde ✓; Unit 4069 (Consistency dahil), Integration StockAndCapacity 27 ve ilgili Model/Concurrency/Ordering/Jobs sınıfları yeşil, format ve derleme `-warnaserror` temiz; şema yok; `12` `Durum` değişmedi; ⚠ `VersionedFormPageTests` OrderPost_..._KK8 sınıf birlikte koşunca düşüyor, tek başına geçiyor, T334'ten bağımsız görünüyor; yeni backlog D-335…D-337, aktif 131; T301, T307, T315, T322, T332, T366, T381 merge kaydı dolduruldu).
T328 (Bildirim haritası: tetikleyici eşlemesi, bildirim üretmeyen olaylar ve eşitlik denetimi) doğrulama tur 1 ✓ PASS (head `0edb3bd`, PR #440; kriter 1–7, KK1–KK7 ve `09 §25` ✓, güvenlik taraması temiz; KK7 iki mutasyonla (kodda fazla satır, belgede eksik akış atfı) kırıldığı görüldü; Unit Consistency 241, öteki 3746; betik testleri; Integration yerelde NotificationMapQueue 3, komşu kuyruk sınıfları 47; dal CI 42394956374 yeşil; bulgu 0, K-notu 1 (D-295 sayımı ve backlog aktif sayısı bu PR'da düzeltildi); `12` `Durum` değişmedi (kaynak birimi `03 §7.1`–§7.3'e bağlı satır yok); backlog aktif 130).

---

## 2026-11-07 — T316 (Sipariş durumu geçişleri: sevkiyat ekseni) doğrulama tur 1 FAIL

2026-11-07 — T316 (Sipariş durumu geçişleri: sevkiyat ekseni) doğrulama tur 1 ✗ FAIL (head `620b368` + `origin/main` birleştirmesi, PR #437; kriter 5 ~: yöneticinin bütün sipariş iptali (S3/S4/S9) teslim işaretli kalemi denetlemez ve teslim edilmiş dijital kalemli siparişi İptal edildi'ye alır — `02 §5.4`, §7.2.2, §7.2.5 (B1 ORTA S1); öteki kriterler, KK1–KK4, KK6–KK9 ve `09 §25` ✓, güvenlik taraması temiz; Unit Architecture 868 + Consistency 230 + öteki 2809, Integration Ordering 122, betik testleri ✓; dal CI 42393227539 yeşil; `12` `Durum` değişmedi (KP-46 satırları uç yanıtı ister); backlog aktif 130). Düzeltme r1 yapıldı (B1: teslim işaretli kalemli sipariş bütün sipariş iptaliyle İptal edildi'ye geçmez; S3/S4/S9 testleri; Integration Ordering 125), tur 2 bekliyor. Önceki kayıt `STATUS_CHANGELOG.md`'de.

## 2026-11-07 — T332 (Satış modeli, ürün tipleri, ürün–varyant–seçenek yapısı ve stok kodu) doğrulama tur 1 PASS

2026-11-07 — T332 (Satış modeli, ürün tipleri, ürün–varyant–seçenek yapısı ve stok kodu) doğrulama tur 1 ✓ PASS (head `48db1e8` + `origin/main` (T309) birleştirmesi, PR #432; kriter 1–10, KK1–KK10 ve `09 §25` ✓, güvenlik taraması temiz; Unit 3585 (birleştirme sonrası), betik testleri, Integration yerelde SalesModel 28, AccountEmailQueue 14, ImageDisposal + Sitemap 10; dal CI 42376259158 yeşil; bulgu 0, K-notu 1 (D-283 → T525); `12` `Durum` değişmedi (KP-4/KP-5/KP-6/KP-39 satırları vitrin ve panel kanıtı ister); backlog aktif 125).
## 2026-11-07 — T322 (İçerik yayın durumları ve iletişim talebi durumları) doğrulama tur 1 PASS

2026-11-07 — T322 (İçerik yayın durumları ve iletişim talebi durumları) doğrulama tur 1 ✓ PASS (head `106c8ae`, PR #438; kriter 1–9, KK1–KK9 ve `09 §25` ✓, güvenlik taraması temiz; Unit 3790 + Consistency 230, betik testleri, Integration yerelde CorporateContent 45; dal CI 42394560527 yeşil; bulgu 0, K-notu 0; `12` `Durum` değişmedi (T322'nin KP satırı yok); backlog aktif 128).

## 2026-11-07 — T322 (İçerik yayın durumları ve iletişim talebi durumları) yapım bitti

2026-11-07 — T322 (İçerik yayın durumları ve iletişim talebi durumları) yapım tamamlandı, doğrulama bekliyor (dal `task/T322-icerik-iletisim-durumlari`; kriter 1–9, KK1–KK9 öz denetimde ✓; Unit Consistency 230, Architecture 906, CorporateContent 57, Integration CorporateContent 45, format ve derleme `-warnaserror` temiz; şema yok; `12` `Durum` değişmedi; yeni backlog D-315…D-317, aktif 128).

## 2026-11-07 — T316 (Sipariş durumu geçişleri: sevkiyat ekseni) yapım bitti

2026-11-07 — T316 (Sipariş durumu geçişleri: sevkiyat ekseni) yapım bitti, doğrulama bekliyor (dal `task/T316-sevkiyat-ekseni`, PR #437; kriter 1–9 ve KK1–KK9 öz denetimde ✓; Unit Architecture 868 + öteki 3039, Integration Ordering 122 (ShipmentTransitionsTests 91); backlog D-305…D-309 yeni, D-291 (b) sevkiyat ekseni için kapandı, aktif 125).

## 2026-11-07 — T301 (Siparişin imha parçası) doğrulama tur 1 PASS

2026-11-07 — T301 (Siparişin imha parçası) doğrulama tur 1 ✓ PASS (head `78b8449` + `origin/main` birleştirmesi, PR #431; kriter 1–9, KK1–KK9 ve `09 §25` ✓, güvenlik taraması temiz; Unit 3469 (Architecture 830, Consistency 209, öteki 2430), betik testleri, Integration yerelde OrderDisposal 27, Disposal + ModuleCallTransaction 21, CookiePolicy + LoggingFiles 25; dal CI 42376054521 yeşil; bulgu 0, K-notu 0; `12` `Durum` değişmedi (KP-74'ün T301'e düşen satırı `VAL-D047` zaten PASS); backlog aktif 125).

## 2026-11-07 — T381 (Ayar katmanları ve parametre envanteri) doğrulama tur 2 PASS

2026-11-07 — T381 (Ayar katmanları ve parametre envanteri) doğrulama tur 2 ✓ PASS (head `25e420c`, PR #430; kriter 1–10, KK1–KK10 ve `09 §25` ✓, güvenlik taraması temiz; Unit Consistency 219, Architecture 838, öteki 2529; betik testleri; Integration yerelde FirmSettings + FirmSettingDefaults 24, CompanyIdentity + SalesGate + StartupData 71; dal CI 42381345942 yeşil; bulgu 0, K-notu 1 (rapordaki eski metin, bu PR'da kapandı); `12` `Durum` değişmedi (KP-64 satırları ekran/ürün formu/uçtan uca — BEKLEMEDE); backlog aktif 120, main ile birleşik).

## 2026-11-07 — T315 (Durum makinesi çekirdeği) doğrulama tur 1 PASS

2026-11-07 — T315 (Durum makinesi çekirdeği) doğrulama tur 1 ✓ PASS (head `0d30a6f` + `origin/main` birleştirmesi, PR #434; kriter 1–8 ve KK1–KK8 ✓, bulgu yok; dal CI 42381390535 yeşil; Unit 3570 (Architecture 844, Consistency 224), Integration OrderTransitionsTests 31; `12`'de T315 satırı yok; K1 → D-291 (S1'in sistem aktörü ve havale onayı; yönetici geçişinde sürüm — T316/T317), backlog aktif 118).

## 2026-11-07 — T307 (Müşteri bildirimleri B-9…B-16) doğrulama tur 1 PASS

2026-11-07 — T307 (Müşteri bildirimleri B-9…B-16) doğrulama tur 1 ✓ PASS (head `43be541` + `origin/main` birleştirmesi, PR #435; kriter 1–10, KK1–KK10, `09 §25` ve güvenlik taraması ✓, bulgu yok; Unit 3536 + Consistency 213 (birleştirme sonrası Unit 3572), betik testleri, Integration yerelde parçalı (CustomerNoticeAfterOrderQueueTests + CustomerNoticeQueueTests 22) geçti, dal CI 42385697297 yeşil; `12` `Durum` değişmedi (A321–A323 gönderimi ve tetiği ister — T305/T306 emsali); K1 → D-293 (gönderen ve yeniden gönderim yazıcısı: il adları, B-9 bağlı kaydı, iade adresi koşulu); backlog aktif 116). Önceki kayıt `STATUS_CHANGELOG.md`'de.

## 2026-11-07 — T307 (Müşteri bildirimleri B-9…B-16) yapım bitti

2026-11-07 — T307 (Müşteri bildirimleri B-9…B-16) yapım tamamlandı, doğrulama bekliyor (PR #435; kriter 1–10, KK1–KK10 öz denetimde ✓; Unit 3749, Integration CustomerNotice 22, format ve derleme `-warnaserror` temiz; `12` `Durum` değişmedi (A324, A335 gönderimi ister); yeni backlog kaydı yok, aktif 108).

## 2026-11-07 — T366 (Yasal metinler) doğrulama tur 2

2026-11-07 — T366 (Yasal metinler: dört metin, sürüm, bağlantılar, e-postada tam metin) doğrulama tur 2 ✓ PASS (head `171fc5d`, PR #428; tam yeniden doğrulama: kriter 1–10, KK1–KK10, `09 §25` ve güvenlik taraması ✓ — tur 1 bulgusu kapandı: aydınlatma kimlik eksikken yayımlanmaz, yer tutucular gösterimde güncel kimlikle HTML kodlu dolar; 4, 5, 6 modül düzeyinde; Unit 3500, betik testleri, Integration yerelde parçalı 121 geçti, dal CI 42371931110 yeşil; `12` `Durum` değişmedi; K1 → D-268 (T356), K2 → D-269 (T507); backlog aktif 115).
## 2026-11-07 — T341 (Üyelik) yapım bitti

2026-11-07 — T341 (Üyelik: kayıt, e-posta doğrulama kapısı, misafir siparişinin hesaba bağlanması, hesap adı, giriş kaydı) yapım bitti, PR #433 açık, doğrulama bekliyor (task `⏳ Devam ediyor`; Unit 3477 (Architecture dahil), Integration yerelde MembershipTests 29 + SessionLinkAttemptLoginTables/LinkRecords/Disposal 99 geçti; modül yüzü: kayıt, doğrulama bağlantısı okuma ve doğrulama, yeniden gönderim, şifre ve kapı denetimi, sipariş hesabı çözümü, misafir siparişlerinin bağlanması, hesap adı, giriş kaydı; ekran ve POST bağları D-254, şifre politikası D-251, e-posta gönderimi D-252, gece silmesi D-253, L-1 ve oturum D-255; D-64 ve D-96 kapandı, backlog aktif 106). Önceki kayıt `STATUS_CHANGELOG.md`de.

## 2026-11-07 — T306 (Müşteri bildirimleri B-1…B-8) doğrulama tur 3 PASS

2026-11-07 — T306 (Müşteri bildirimleri B-1…B-8) doğrulama tur 3 ✓ PASS — eskalasyon sonrası tam doğrulama (head `3e9eb28`, PR #427; kriter 1–10 ve KK1–KK10 ✓, bulgu yok; düzeltme 2 ile B-1 düzeltmesinin `OriginalDispatchId`'si zincirin köküne iner; dal CI 42373330980 yeşil; Unit 3572 (Architecture 830), Integration CustomerNoticeQueueTests 14 + komşu 41; `12` `Durum` değişmedi (A317–A320, A324 gönderimi ister, `BEKLEMEDE`); K1 → D-272 (`EventValues` yük biçimi örnek dosyası), backlog aktif 108).

---

## 2026-11-07 — T306 (Müşteri bildirimleri B-1…B-8) doğrulama tur 3 PASS

2026-11-07 — T306 (Müşteri bildirimleri B-1…B-8) doğrulama tur 3 ✓ PASS — eskalasyon sonrası tam doğrulama (head `3e9eb28`, PR #427; kriter 1–10 ve KK1–KK10 ✓, bulgu yok; düzeltme 2 ile B-1 düzeltmesinin `OriginalDispatchId`'si zincirin köküne iner; dal CI 42373330980 yeşil; Unit 3572 (Architecture 830), Integration CustomerNoticeQueueTests 14 + komşu 41; `12` `Durum` değişmedi (A317–A320, A324 gönderimi ister, `BEKLEMEDE`); K1 → D-272 (`EventValues` yük biçimi örnek dosyası), backlog aktif 108).

---

## 2026-11-07 — T381 (Ayar katmanları ve parametre envanteri) düzeltme turu 1 bitti

2026-11-07 — T381 (Ayar katmanları ve parametre envanteri) düzeltme turu 1 bitti, doğrulama bekliyor (PR #430; B1: çit dışı değer `07 §6.2.6`'daki `ValueOutOfBounds` koduyla reddediliyor; B2: `PeriodsInput`/`PeriodsSection.ShippingPeriod` adı `DefaultShippingPeriod` oldu; `origin/main` birleştirildi, T309'un yinelenen ömür sabitleri kaldırıldı).

## 2026-11-07 — T381 (Ayar katmanları ve parametre envanteri) doğrulama tur 1 FAIL

2026-11-07 — T381 (Ayar katmanları ve parametre envanteri) doğrulama tur 1 ✗ FAIL (head `98f5550`, PR #430; B1 ORTA S1: çit dışı değer `07 §6.2`'nin `ValueOutOfBounds` kodu yerine `InvalidQuantity` ile reddediliyor (UC-170, UC-171); B2 DÜŞÜK S1: `PeriodsInput`/`PeriodsSection` alanı `ShippingPeriod`, `07 UC-170`/`06 §4.2` adı `DefaultShippingPeriod`; öteki kriterler ✓; build, format, Unit 3318 + Consistency 215, betik testleri, Integration FirmSetting 24 + SalesGate/CompanyIdentity 46 yerelde yeşil, dal CI 42375448881 yeşil; `12` `Durum` değişmedi; K1 → D-279, backlog aktif 105; dal merge edilmez, düzeltme turu gerekir).

---

## 2026-11-07 — T381 (Ayar katmanları ve parametre envanteri) yapım bitti

2026-11-07 — T381 (Ayar katmanları ve parametre envanteri) yapım tamamlandı, PR açıldı, doğrulama bekliyor (dal `task/T381-ayar-katmanlari`; Unit 3318 + Consistency 215, Integration FirmSetting 24 + SalesGate/CompanyIdentity 46; D-164 ve D-176 yeniden ileri atıldı, D-256 yeni, backlog aktif 104).

---

## 2026-11-07 — T306 (Müşteri bildirimleri B-1…B-8) doğrulama tur 2

T306 (Müşteri bildirimleri B-1…B-8) doğrulama tur 2 ✗ FAIL (head `26e99f2`, PR #427; kriter 3 ~ Kısmi — Bulgu 1 ORTA S1: düzeltme sonrası B-1 satırı, verilen ilk satır kendisi bir düzeltme satırıysa onun taşıdığı ilk kimliği değil kendi kimliğini `OriginalDispatchId`'ye yazar, grup bölünür — `06 §4.58`, `08 §2.2.5` m.6; öteki kriterler ✓; dal CI 42370396257 yeşil; Unit 3363 + Consistency 209, Integration CustomerNoticeQueueTests 13 + komşu 49; `12` `Durum` değişmedi; yeni K-notu yok, backlog aktif 105; iki iterasyon sınırı — eskalasyon).

## 2026-11-07 — T306 (Müşteri bildirimleri B-1…B-8) düzeltme 1

2026-11-07 — T306 (Müşteri bildirimleri B-1…B-8) düzeltme 1 yapıldı, doğrulama tur 2 bekliyor (PR #427; tur 1 ✗ FAIL bulguları kapatıldı: B1 ORTA — düzeltme sonrası B-1 satırı olay değerlerine ilk e-postanın oluşma anını (`FirstEmailSentAt`) yazar, kuyruk ilk satırı (aynı sipariş, `B1`) okur, `Read` anı döndürür, yeniden gönderim cümlesi bu değerden üretilir, ilk satır silinse de tarih kalır; B2 DÜŞÜK — B-5 "Takip et" dolu düğme değil, sabit renkli bağlantı, `noreferrer` ve tam adres korunur, marka rengi yalnız ana düğmede; `origin/main` (T308 #424) birleştirildi, `INotificationsModule`/`NotificationsModule` iki tarafın üyeleriyle; Unit Notifications 202, Architecture 830, Infrastructure 1162, kalan 2331, Integration yerelde CustomerNoticeQueueTests 13, format ve derleme `-warnaserror` temiz; `12` `Durum` değişmedi; backlog aktif 105).

## 2026-11-07 — T306 (Müşteri bildirimleri B-1…B-8) doğrulama tur 1 FAIL

2026-11-07 — T306 (Müşteri bildirimleri B-1…B-8) doğrulama tur 1 ✗ FAIL (head `deb3a3f` + `origin/main` birleştirmesi, PR #427; kriter 3 ~ Kısmi — düzeltme sonrası B-1 satırı ilk e-postanın oluşma anını olay değerlerine yazmıyor (`06 §4.58` `EventValues`, `08 §2.2.5` m.6) — bulgu B1 ORTA S1; B2 DÜŞÜK S1 — B-5 "Takip et" marka renkli ikinci düğme (`08 §2.2.5` m.2); öteki kriterler ve KK ✓; Consistency 209, Unit 3418, betik testleri, Integration yerelde parçalı (CustomerNoticeQueueTests 11, altı komşu sınıf 92) geçti; dal CI 42365665924 yeşil; `12` `Durum` değişmedi; K1 → D-231, K2 → D-232; backlog aktif 104). Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-07 — T306 (Müşteri bildirimleri B-1…B-8) doğrulama tur 3 PASS

2026-11-07 — T306 (Müşteri bildirimleri B-1…B-8) doğrulama tur 3 ✓ PASS — eskalasyon sonrası tam doğrulama (head `3e9eb28`, PR #427; kriter 1–10 ve KK1–KK10 ✓, bulgu yok; düzeltme 2 ile B-1 düzeltmesinin `OriginalDispatchId`'si zincirin köküne iner; dal CI 42373330980 yeşil; Unit 3572 (Architecture 830), Integration CustomerNoticeQueueTests 14 + komşu 41; `12` `Durum` değişmedi (A317–A320, A324 gönderimi ister, `BEKLEMEDE`); K1 → D-272 (`EventValues` yük biçimi örnek dosyası), backlog aktif 108).

---

## 2026-11-07 — T366 (Yasal metinler) doğrulama tur 1 FAIL

2026-11-07 — T366 (Yasal metinler: dört metin, sürüm, bağlantılar, e-postada tam metin) doğrulama tur 1 ✗ FAIL (head `24163d5` + `origin/main` birleştirmesi, PR #428; kriter 2 ✗: aydınlatma metni firma kimliği eksikken yayına alınabiliyor, kimlik yer tutucuları boş değerle donuyor ve kimlik sonradan dolunca liste "tamamlandı" der, kapının aydınlatma koşulu açılır, vitrindeki metin veri sorumlusunun unvanı ve KEP'i boş kalır (`06 §4.56`, `04` 9.19.7, 9.19.11, `02 §3.1.5`); öteki kriterler ✓ (4, 5, 6 modül düzeyinde); Consistency 209, Unit 3234, betik testleri, Integration yerelde parçalı 118 geçti, dal CI 42368746924 yeşil; `12` `Durum` değişmedi; K1 → D-240; backlog aktif 108; düzeltme turu gerekli).

---

## 2026-11-07 — T366 (Yasal metinler) yapım bitti

2026-11-07 — T366 (Yasal metinler: dört metin, sürüm, bağlantılar, e-postada tam metin) yapım tamamlandı, doğrulama bekliyor (dal `task/T366-yasal-metinler`, PR #428; kriter 1–10, KK1–KK10 öz denetimde ✓ — 4, 5, 6'nın ekran ve e-posta yüzleri T356/T306/T385/T540/T507'de, D-222; Unit Architecture 846 + öteki 2597, Integration yerelde LegalText 13, SalesGate 22, CompanyIdentity 24, Model 974, Web 492, Files 81, Logging 30 geçti; Lint yerelde temiz; yeni D-219…D-222, D-165 (b) kapandı, D-167 T356'ya ileri, D-198 T366 payı kapandı; backlog aktif 106).

---

## 2026-11-07 — T308 (Firma bildirimleri F-1…F-6) doğrulama tur 1 PASS

2026-11-07 — T308 (Firma bildirimleri F-1…F-6) doğrulama tur 1 ✓ PASS (head `77c623d` + `origin/main` birleştirmesi, PR #424; kriter 1–10, KK1–KK10 ve `09 §25` ✓; Unit 3317 (Architecture 797, öteki 2520), betik testleri, Integration yerelde CompanyNoticeQueue + MigrationBundle 13, Dispatch/Email/Job komşuları 84, Logging 30, dal CI 42363737559 yeşil; `12` `Durum` değişmedi (KP-34, KP-67 satırları olayın tetiğini ve sahte altyapıyla gönderimi ister, `BEKLEMEDE`); doğrulamada `origin/main` birleştirildi; K1 → D-224, backlog aktif 105). Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-07 — T306 (Müşteri bildirimleri B-1…B-8) yapım tamamlandı

2026-11-07 — T306 (Müşteri bildirimleri B-1…B-8) yapım tamamlandı, PR açıldı, doğrulama bekliyor (Unit 3418, Integration yerelde CustomerNoticeQueueTests 11, format ve derleme `-warnaserror` temiz; öz denetim kriter 1–10, KK1–KK10 ✓; backlog aktif 104).
---

## 2026-11-07 — T309 (Hesap ve yönetim e-postaları) doğrulama tur 1 PASS

2026-11-07 — T309 (Hesap ve yönetim e-postaları) doğrulama tur 1 ✓ PASS (head `2ca3caa`, PR #429; kriter 1–10, KK1–KK10 ve `09 §25` ✓, güvenlik taraması temiz; Unit 3521 (Consistency 213, Architecture 830, öteki 2478), betik testleri, Integration yerelde AccountEmailQueue 14, CompanyNoticeQueue 11, RecipientSendLimit 11; dal CI 42371984874 yeşil; bulgu 0, K-notu 0; `12` `Durum` değişmedi (KP-66/KP-67 satırlarında hesap e-postası yok); backlog aktif 105).

---

## 2026-11-07 — T332 (Satış modeli) yapım tamamlandı

**Son güncelleme:** 2026-11-07 — T332 (Satış modeli, ürün tipleri, ürün–varyant–seçenek yapısı ve stok kodu) yapım tamamlandı, PR açıldı (dal `task/T332-satis-modeli`; ürün tanımı kuralları bekçide, seçenek yapısı ve stok kodu okumaları modül yüzünde; Unit 3528, Integration yerelde parçalı 1638; yeni backlog D-246…D-249, backlog aktif 107). Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-07 — T309 (Hesap ve yönetim e-postaları) yapım tamamlandı

**Son güncelleme:** 2026-11-07 — T309 (Hesap ve yönetim e-postaları) yapım tamamlandı, PR açıldı (dal `task/T309-hesap-yonetim-epostalari`; yedi hesap e-postası, ömürler P-12…P-14/P-47 sabitlerinden, Google teyidi için tek bağlantı kuralı; Unit 3521 (Architecture 830), Integration `AccountEmailQueueTests` 14; yeni backlog D-233, D-234, backlog aktif 105).


---

## 2026-11-07 — T312 (Dosya uçları) doğrulama tur 1 PASS

2026-11-07 — T312 (Dosya uçları: imzalı indirme ve parçalı yükleme) doğrulama tur 1 ✓ PASS (head `93e5beb` + `origin/main` birleştirmesi, PR #426; kriter 1–10, KK1–KK10 ve `09 §25` ✓, güvenlik taraması temiz; Consistency 209, Unit 3114, betik testleri; Integration yerelde parçalı — Files 81, Web üç sınıf 97, Concurrency 26 — hepsi geçti; dal CI 42362008048 yeşil; bulgu 0; `12` `Durum` değişmedi (KP-20/KP-44 satırları T511 ve formun kaydını bekler); K1 → D-217 (indirme 404'ünün E-06 sayfası, T504), K2 → D-218 ("kalemi indirilemez"in kapsamı, T511), K3 kod kanıtıyla kapandı; backlog aktif 100).

---

## 2026-11-07 — T331 (Satış kapısı) doğrulama tur 1 PASS

2026-11-07 — T331 (Satış kapısı) doğrulama tur 1 ✓ PASS (head `461c66b` + `origin/main` birleştirmesi, PR #425; kriter 1–8, KK1–KK8 ve `09 §25` ✓ (4 ve ekran yüzleri modül düzeyinde; ekran/POST bağları D-198, D-199 ile T346/T347/T341/T365/T508/T535'e); Unit 3282 (Architecture+Consistency+InstallationSettings 1185, öteki 2097), betik testleri, Integration yerelde SalesGate 22 + CompanyIdentity/AuditTrail 46, dal CI 42361992653 yeşil; `12` `Durum` değişmedi (KP-25, KP-38, KP-59, KP-60 satırları uç yanıtı/tarayıcı kanıtı ister, `BEKLEMEDE`); doğrulamada `origin/main` birleştirildi, backlog aktif 104; K-notu yok).

---

## 2026-11-07 — T308 (Firma bildirimleri F-1…F-6) yapım bitti

2026-11-07 — T308 (Firma bildirimleri F-1…F-6) yapım bitti, PR #424 açık, doğrulama bekliyor (task `⏳ Devam ediyor`; Unit 3286 (T308 57), Integration T308 11 yerelde geçti; `12` `VAL-A326…A330` ve `VAL-A754` olayı üreten task'lar `QueueCompanyNoticeAsync` / `QueueAdminNoticeAsync`'i bağlayana kadar `BEKLEMEDE`; T304, T305, T330, T380 squash hash'leri status'a işlendi).

---

## 2026-11-07 — T310 (Kargo şirketi listesi) doğrulama tur 1 PASS

2026-11-07 — T310 (Kargo şirketi listesi) doğrulama tur 1 ✓ PASS (head `c3e1670` + `origin/main` birleştirmesi, PR #423; kriter 1–9, KK1–KK9 ve `09 §25` ✓; plan düzeltmesi K-1576 kurala uygun (DV-32 tarayıcı denemesi T601 kriter 12'de); Consistency 209, Unit 2815, betik testleri; Integration dokunulmadı, dal CI 42353336593 yeşil; `12` `Durum`: `VAL-E034` → `PASS`, `VAL-E033` (T601) ve `VAL-A060`…`A063` (sipariş sayfası) `BEKLEMEDE`; K1 → D-196).

---

## 2026-11-07 — T305 (E-posta ortak kalıbı ve gövde üretimi) doğrulama tur 1 PASS

2026-11-07 — T305 (E-posta ortak kalıbı ve gövde üretimi) doğrulama tur 1 ✓ PASS (head `2ee4f63`, PR #422; kriter 1–9, KK1–KK9 ve `09 §25` ✓; Unit 3049 (T305 56), betik testleri, Integration yerelde parçalı 1793 hepsi geçti, dal CI 42352393327 yeşil; `12` `Durum` değişmedi — KP-66/KP-68 satırları gönderen yolunu ister, `BEKLEMEDE`; K1 raporda kapatıldı (D-97/D-166 sahibi), K2 → D-189, K3 → D-190). Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-07 — T330 (Firma kimliği) doğrulama tur 1 PASS

2026-11-07 — T330 (Firma kimliği) doğrulama tur 1 ✓ PASS (head `04eafd6`, PR #420; kriter 1–8, KK1–KK8 ve `09 §25` ✓ — sayfa ve panel bağı T535/T506/T537/T361'in kendi kriterleri; Unit 3010 (Architecture 769, Consistency 207, kalan 2034; birleşik dalda 3050), betik testleri; Integration yerelde parçalı 1806 hepsi geçti; dal CI 42348609633 yeşil; `12` `Durum` değişmedi (KP-35, KP-58 satırları sayfa/uç kanıtı ister, `BEKLEMEDE`); doğrulamada `origin/main` (T304) dala birleştirildi; K-notu yok). Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## YYYY-AA-GG — <özet başlık>

`<Taşınan status girişi buraya olduğu gibi yazılır.>`

---

---

## 2026-11-07 — T380 düzeltme turu 1

**Son güncelleme:** 2026-11-07 — T380 (İşlem izi) düzeltme turu 1 yapıldı, doğrulama tur 2 bekliyor (PR #419; tur 1 bulgu 1 KRİTİK S1 kapatıldı: `AuditEntryRules` Order satırında ve `DeleteCustomerAccount`'ta `Details`'in her yuvasını denetler — Order'da görünen ad yalnız sipariş numarası, hesap silmede görünen ad yok, değer adları kapalı listede ve her adın değeri kapalı biçimde (kod, tarih, adet, tutar, kimlik), serbest metin taşıyabilecek adlar (kargo adı, takip numarası, kalem adı) listeden çıkarıldı; `09 §9.2` m.4 yuva cümlesi eklendi; K1 backlog numarası D-161 → D-164 (main'deki D-161 ile çakışma); Unit Administration 100/100, Integration AuditTrailTests 22/22; `12` `Durum` değişmedi; doğrulama ve merge yardımcıda).

---

## 2026-11-07 — T304 doğrulama tur 1 PASS

**Son güncelleme:** 2026-11-07 — T304 (Alıcı başına bildirim gönderim sınırı) doğrulama tur 1 ✓ PASS (head `a8f38c1`, PR #421; kriter 1–9, KK1–KK9 ve `09 §25` ✓; Consistency 209, Unit 2759, betik testleri; Integration yerelde `RecipientSendLimitTests` 11 ve dal CI 42346270356 yeşil; `12` `Durum`: `VAL-E086`, `VAL-E095` → `PASS`; K1 → D-172). Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-07 — T313 doğrulama tur 1 PASS

**Son güncelleme:** 2026-11-07 — T313 (Site haritası ve robots.txt) doğrulama tur 1 ✓ PASS (head `336f075`, PR #417; kriter 1–9, KK1–KK9 ve `09 §25` ✓; Unit 2851, Integration yerelde parçalı hepsi geçti (dal CI 42339118325 yeşil); `12` `Durum`: `VAL-A378`, `VAL-A379`, `VAL-A381`, `VAL-A398` → `PASS`, öteki KP-37 satırları `BEKLEMEDE`; K1–K3 raporda kapatıldı, açık uçlar D-163). Merge commit'i sonraki task dalında yazılır. Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-07 — T314 doğrulama tur 1 PASS

**Son güncelleme:** 2026-11-07 — T314 (Durum ucu: uyarı koşulları ve 500 sayacı) doğrulama tur 1 ✓ PASS (head `a57c4ca`, PR #418; kriter 1–10, KK1–KK10 ve `09 §25` ✓; Unit 2878, Integration yerelde parçalı (StatusEndpointTests 41) ve dal CI 42339425149 yeşil; `12` `Durum` değişmedi — KP-50 satırları ve durum ucu satırları `BEKLEMEDE`; K1 kod kanıtıyla kapandı, K2 → D-162).

---

## 2026-11-07 — T313 doğrulama tur 1 PASS

**Son güncelleme:** 2026-11-07 — T313 (Site haritası ve robots.txt) doğrulama tur 1 ✓ PASS (head `336f075`, PR #417; kriter 1–9, KK1–KK9 ve `09 §25` ✓; Unit 2851, Integration yerelde parçalı hepsi geçti (dal CI 42339118325 yeşil); `12` `Durum`: `VAL-A378`, `VAL-A379`, `VAL-A381`, `VAL-A398` → `PASS`, öteki KP-37 satırları `BEKLEMEDE`; K1–K3 raporda kapatıldı, açık uçlar D-163). Merge commit'i sonraki task dalında yazılır. Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-07 — T378 doğrulama tur 1 PASS

**Son güncelleme:** 2026-11-07 — T378 (Zaman sayımı) doğrulama tur 1 ✓ PASS (head `33e4d94`, PR #416; kriter 1–9, KK1–KK9 ve `09 §25` ✓; Unit 2669 + Consistency 207, betik testleri; Integration dokunulmadı (dal CI 42337007822 yeşil); `12` `Durum` değişmedi — task'ın KP satırı yok, ilgili `VAL-A801` tüketici task'larda `BEKLEMEDE`; K1 bu PR'da kapatıldı (`DateTimeSignatures` özet yorumu yerine taşındı); K2 → D-161 (sıfır iş günlük kargoya verme süresi sayımda istisna atar → T325)). Merge commit'i sonraki task dalında yazılır. Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-07 — F2 gate check PASS

**Son güncelleme:** 2026-11-07 — F2 (Çekirdek servisler) gate check: ✓ PASS (main `f4da03c`; T201–T233; Consistency 207/207, Unit 2826/2826, Integration 1730/1730 (parçalı), betik testleri 4 dosya; Release `-warnaserror` 0/0; üç imaj `--no-cache`; yerel referans kurulum sıfırdan, canlılık 200, denetimler yeşil; F2'de göç yok, 18 göç ileri ve idempotent; `11 §6.2` F2: mimari testler kasıtlı ihlalle kırmızı, maskeleme testleri yeşil; matris F2 189 satır, S3 0; `VAL-A991` gate'te yeniden PASS; backlog: D-32, D-60, D-68, D-85, D-123, D-128, D-157 ✅, yeni D-159, D-160; rapor `CHECKPOINT_REPORTS/GATE_CHECK_F2.md`; merge ve `phase/F2-pass` yardımcıda).

---

## 2026-11-07 — T229 doğrulama tur 1 PASS

**Son güncelleme:** 2026-11-07 — T229 (Uygulama belleğinde önbellek) doğrulama tur 1 ✓ PASS (head `6d25c8d`, PR #414; kriter 1–7, KK1–KK7 ve `09 §25` ✓; Unit 2826, Integration 1729 (yerelde dokuz parça; dal CI 42300942345 yeşil); `12` `Durum` değişmedi — ilgili `VAL-A835` `BEKLEMEDE`; K1 → D-158 (`09 §20` m.9'un yorumu yalnız testte, taramanın darlığı)). Merge commit'i sonraki task dalında yazılır. Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-07 — T229 yapım tamamlandı, doğrulama bekliyor

**Son güncelleme:** 2026-11-07 — T229 (Uygulama belleğinde önbellek) yapım tamamlandı, doğrulama bekliyor (dal `task/T229-uygulama-bellegi-onbellek`, PR #414; kriter 1–7, KK1–KK7 ve `09 §25` öz denetim tablosu raporda; Unit 2826, Integration 1729; ⚠ K1–K4 raporda). Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-07 — T219 doğrulama tur 1 PASS

**Son güncelleme:** 2026-11-07 — T219 (Arama motoru verileri kalıbı) doğrulama tur 1 ✓ PASS (head `f4e4505`, PR #413; kriter 1–8, KK1–KK8 ve `09 §25` ✓; Unit 2773, Integration 1715 (dal CI 42287139738 yeşil); `12` `Durum` değişmedi — KP-37'nin 38 satırı `BEKLEMEDE`; K1 → D-157 (Integration'ın göç paketi her koşuda yeniden kuruluyor)). Merge commit'i sonraki task dalında yazılır. Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-06 — T219 yapım tamamlandı, doğrulama bekliyor

**Son güncelleme:** 2026-11-06 — T219 (Arama motoru verileri kalıbı) yapım tamamlandı, doğrulama bekliyor (dal `task/T219-arama-motoru-verileri`, PR #413; kriter 1–8, KK1–KK8 ve `09 §25` öz denetim tablosu raporda; Unit 2773, Integration 1715; yeni tablo/göç yok; backlog'da hedefi T219 olan kayıt yok). T217 Commit hücresi `d2b5379` ve raporunun Post-merge satırı dolduruldu. Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-06 — T217 doğrulama tur 3 PASS

**Son güncelleme:** 2026-11-06 — T217 (Yapılabilir işlemlerin sunucuda üretimi ve POST işleyicisinde yeniden denetim) doğrulama tur 3 ✓ PASS (head `e6cfe5a`, PR #412; tam yeniden doğrulama: kriter 1–8, KK1–KK8 ve `09 §25` ✓; B1-r2 kapandı — kural modülde, sayfa yalnız `Contracts` okuma/yazma yöntemini çağırır, `09 §8.3` 05/07/11 ile çelişmiyor; ret sonrası bütün tablolar değişmedi, Türkiye gece yarısı sınırı doğru (doğrulayıcı sondası); Unit 2640, Integration 1695; dal CI 42267704705 yeşil; bulgu 0; `12` `Durum` değişmedi (KP kaynak birimi yok); K1 → D-156 (iptal–cayma kuralının ve yazımının modülü; örnek modül başka modülün varlığını yazar)).

---

## 2026-11-06 — T217 düzeltme 2, doğrulama bekliyor

**Son güncelleme:** 2026-11-06 — T217 (Yapılabilir işlemlerin sunucuda üretimi ve POST işleyicisinde yeniden denetim) yapım tamamlandı, doğrulama bekliyor (dal `task/T217-yapilabilir-islemler`, PR #412; kriter 1–8 ve KK1–KK8 öz denetimde ✓; Unit 2640, Integration 1695 (doğrulama tur 1: ✗ FAIL, B1; düzeltme 1: çalıştırıcı ortak altyapıya; doğrulama tur 2: ✗ FAIL, B1-r2 — `09 §8.3` m.1 ürün kuralını sayfa katmanına koyuyordu, `05 §3.3` ve T323 ile çelişti; yeni D-155; düzeltme 2 yapıldı: kural modülde yaşar, sayfa yalnız `Contracts` okuma/yazma yöntemini çağırır, sıra `OperationGate.ExecuteAsync`'te, `09 §8.3` m.1/m.3/m.6/(5) buna göre — 05/07/11 ile aynı, karar kaydı gerekmedi); `09 §8.3` + `§25` satırı; kalıp `IOperationRule`/`OperationGate`/`OperationRunner`/`OperationRefusals` + `[GatedOperation]` kaynak taraması; gerçek ekran kuralları F3–F5'te; yeni D-153 → T367; T216 Commit hücresi `9ab9bec` dolduruldu). Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-06 — T216 doğrulama tur 1 PASS

**Son güncelleme:** 2026-11-06 — T216 (Tarih, sayı ve para biçimi: istek ve gösterim) doğrulama tur 1 ✓ PASS (head `05fd381`, PR #411; kriter 1–8, KK1–KK8 ve `09 §25` ✓; tutar ayrıştırması Unicode rakam, görünmez boşluk, `0x10`, işaret, üs ve hassasiyet dışı metinle yoklandı — değer hiçbir girdide değişmez; Unit 2569, Integration 1682; dal CI 42224854554 yeşil; bulgu 0; `12` `Durum` değişmedi (KP kaynak birimi yok); K1 → D-150 (tam sayıda sondaki `\0`), K2 → D-151 (IBAN/telefonda bölünmez boşluk), K3 → D-152 (öteki tam sayı türleri ve `TimeSpan`)). Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-06 — T216 yapım tamamlandı, doğrulama bekliyor

2026-11-06 — T216 (Tarih, sayı ve para biçimi: istek ve gösterim) yapım tamamlandı, doğrulama bekliyor (dal `task/T216-tarih-sayi-para-bicimi`; kriter 1–8 ve KK1–KK8 öz denetimde ✓; Unit 2569, Integration 1682; tek bağlayıcı `RequestValueBinder` (decimal/DateOnly/int/long), hassasiyeti aşan tutar metni okunmaz; D-124 ✅, D-123 (a), D-125 ve D-127 T216 payları ✅, yeni D-149 (`MachineJson` → T312)).

---

## 2026-11-06 — T215 doğrulama tur 2 PASS

2026-11-06 — T215 (Hız sınırı, deneme limitleri ve gövde tavanı) doğrulama tur 2 ✓ PASS (head `1ba392d`, PR #409; kriter 1–8, KK1–KK8 ve `09 §25` ✓; tur 1 B1 kapandı — gerçek Kestrel'de chunked gövde tavana kadar kabul, tavan+1 413, bildirim ucunda gövdesiz 413; Unit 2438, Integration 1649; dal CI 42211263821 yeşil; bulgu 0; `12` satırlarında `Durum` değişmedi — KP-72 satırları limit task'larının; K1 → D-147 (chunked okumada istemcinin vazgeçmesi ve bozuk çerçeve beklenmeyen hata sayılır), K2 → D-148 (büyük sınıfın chunked gövdesi kimlik doğrulamasından önce bellekte)).

---

## 2026-11-06 — T215 yapım tamamlandı, doğrulama bekliyor

2026-11-06 — T215 (Hız sınırı, deneme limitleri ve gövde tavanı) yapım tamamlandı, doğrulama bekliyor (dal `task/T215-hiz-siniri-deneme-govde`; kriter 1–8 ve KK1–KK8 öz denetimde ✓; Unit 2438, Integration 1649; `origin/main` (T214) birleştirildi; D-103, D-112 ve D-143 ✅, D-64 `AttemptRecord` yarısı ✅ (`LoginRecord` yarısı T341'e), D-108 tavan konmadı → F2 gate, D-135'e 429/413 yalın sayfa notu).

---

## 2026-11-06 — T214 doğrulama tur 1 PASS

2026-11-06 — T214 (Kısa süreli bildirim kodları) doğrulama tur 1 ✓ PASS (head `479c891`, PR #410; kriter 1–6, KK1–KK6 ve `09 §25` ✓; Unit 2307, Integration 1599; dal CI 42201202210 yeşil; bulgu 0; `12` satırlarında `Durum` değişmedi — `07 §6.3` ve VAL-A607 satırları uç ve giriş ekranı task'larının; K1 → D-146 (`notice` kaynak taramasının tırnaksız bağlama boşluğu)). Önceki kayıt aşağıda.

---

## 2026-11-06 — T214 yapım tamamlandı, doğrulama bekliyor

2026-11-06 — T214 (Kısa süreli bildirim kodları) yapım tamamlandı, doğrulama bekliyor (dal `task/T214-bildirim-kodlari`; kriter 1–6 ve KK1–KK6 öz denetimde ✓; Unit 2307, Integration 1600; D-47 (2) ve D-121 ✅, D-109 T214 payı ✅ — kalan T343/T409). Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-06 — T213 doğrulama tur 1 PASS

2026-11-06 — T213 (Hata kodu kayıt defteri) doğrulama tur 1 ✓ PASS (head `a8f172b`, PR #408; kriter 1–8, KK1–KK8 ve `09 §25` ✓; Unit 2247, Integration 1537; dal CI 42187650016 yeşil; bulgu 0; `12` satırlarında `Durum` değişmedi — `07 §6.2`'nin 119 satırı uç task'larının; K-notları: K1 → D-142 (`Result`/`Result<T>`, F3), K2 → D-143 (429 ve içerik başlıklarının kod sızma taraması, T215), K3 → D-89'a not (red satırı taramasının darlığı)). Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-06 — T213 yapım tamamlandı

2026-11-06 — T213 (Hata kodu kayıt defteri) yapım tamamlandı, doğrulama bekliyor (dal `task/T213-hata-kodu-kayit-defteri`; kriter 1–8 ve KK1–KK8 öz denetimde ✓; Unit 2247, Integration 1538; `ErrorCode` 115 üye + `ErrorStatuses` + tek `RejectionLog`; D-47 (2) `ErrorCode` payı, D-132, D-136, D-140 ✅; D-90 K1/K2/K4 ✅; D-89 ve D-90 K3 gerekçeyle yeniden hedeflendi; D-141 açıldı).

---

## 2026-11-06 — T212 doğrulama tur 1 PASS (merge #407)

2026-11-06 — T212 (Hata gösterimi ve HTTP durumları) doğrulama tur 1 ✓ PASS (head `ec13690`, PR #407; kriter 1–9, KK1–KK9 ve `09 §25` ✓; Unit 2221, Integration 1536; dal CI 42172712753 yeşil; bulgu 0; `VAL-A424`, `A425`, `A429`, `A430`, `A433` PASS; K-notları: K1 → D-138'e not (`09 §15.1` m.6 istisnası, F2 gate), K2 → D-135 hedefi T537 + T540, en geç F5 gate, K3 → D-140 (katmanın `no-store` başlığı, T213)). Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-06 — T212 yapım tamamlandı, doğrulama bekliyor

2026-11-06 — T212 (Hata gösterimi ve HTTP durumları) yapım tamamlandı, doğrulama bekliyor (dal `task/T212-hata-gosterimi-http`; kriter 1–9 ve KK1–KK9 öz denetimde ✓; Unit 2221, Integration Web/Pages/SignedEndpoints/Security/Logging/Jobs/Outages/Files/Disposal/Concurrency ad alanları yerelde gerçek PostgreSQL ile yeşil, Database ve Model ad alanları dal CI'ında; D-25, D-84, D-107, D-113, D-115, D-117, D-130 kapandı; yeni D-135, D-136).

---

## 2026-11-06 — T210 doğrulama tur 1 PASS (merge #406)

2026-11-06 — T210 (Liste kalıbı) doğrulama tur 1 ✓ PASS (head `5476b9f`, PR #406; kriter 1–9, KK1–KK9 ve `09 §25` ✓; Unit 2197, Integration 1510; dal CI 42165996784 yeşil; bulgu 0; `VAL-A006`–`A008` BEKLEMEDE (gerçek vitrin listesi T333/T503); K-notu D-134 (örnek vitrin listesinin fiyat/stok süzgeci ve projeksiyonu, T333); D-127'nin T210 payı kapandı).

---

## 2026-11-06 — T210 yapım tamamlandı, doğrulama bekliyor

2026-11-06 — T210 (Liste kalıbı) yapım tamamlandı, doğrulama bekliyor (dal `task/T210-liste-kalibi`; kriter 1–9 ve KK1–KK9 öz denetimde ✓; Unit 2197; Integration: Web ad alanı 338 yerelde (liste testleri 64 dahil), kalanı dal CI'ında; D-127'nin T210 payı kapandı; `09 §10.5` madde 3 `FromQuery` ekiyle hizalandı, ⚠ 1).

---

## 2026-11-06 — T209 doğrulama tur 1 PASS (merge #405)

2026-11-06 — T209 (Tek kullanımlık değerler, tekrar gönderim, sahteciliğe karşı koruma ve güvenlik başlıkları) doğrulama tur 1 ✓ PASS (head `449f94e`, PR #405; kriter 1–9, KK1–KK9 ve `09 §25` ✓; Unit 2035, Integration 1446; dal CI 42159301722 yeşil; `VAL-A982` PASS; K-notları D-132 (sahtecilik reddinin red satırı, T213), D-133 (yeniden doğrulama değerinin oturum yazımı `ISessionService` altına, T342); `09 §15.2` madde 2 yazım düzeltmesi). Önceki kayıt `STATUS_CHANGELOG.md`'de.

---

## 2026-11-06 — T209 yapım tamamlandı, doğrulama bekliyor

2026-11-06 — T209 (Tek kullanımlık değerler, tekrar gönderim, sahteciliğe karşı koruma ve güvenlik başlıkları) yapım tamamlandı, doğrulama bekliyor (kriter 1–9 ve KK1–KK9 öz denetimde ✓; Unit 2035, Integration 1446; D-131 açıldı (UC-83 ucunun başlık jetonu, T312); `09 §15.2` madde 1, 2, 5 metni gerçek mekanizmaya uyarlandı, 404 yanıtı da `no-store`).

---

## 2026-11-06 — T208 doğrulama tur 1 PASS

2026-11-06 — T208 (Sürümün ve gösterilen değerin formda taşınması, koşullu güncelleme) doğrulama tur 1 ✓ PASS (head `3247026`, PR #404; kriter 1–9, KK1–KK9 ve `09 §25` ✓; Unit 1931, Integration 1413; dal CI 42150380946 yeşil; `VAL-A610`–`A614` PASS; D-95 (c) kapandı; K-notları D-129, D-130).

---

## 2026-11-06 — T208 yapım tamamlandı, doğrulama bekliyor

2026-11-06 — T208 (Sürümün ve gösterilen değerin formda taşınması, koşullu güncelleme) yapım tamamlandı, doğrulama bekliyor (kriter 1–9 ve KK1–KK9 öz denetimde ✓; Unit 1931, Integration 1413 yerelde gerçek PostgreSQL 18’de yeşil; main ile birleştirildi (T211); D-95 (c) kapandı, (a)(b) T348; ⚠ hata kodları T213’te, öteki değerlerin yanıtı T212 madde 4).

---

## 2026-11-06 — T211 doğrulama tur 2 PASS (merge #402)

2026-11-06 — T211 (Model bağlama kalıbı) doğrulama tur 2 ✓ PASS (head `50622d5`, PR #402; kriter 1–8, KK1–KK8 ve `09 §3.4` m.1–7 ✓; tur 1 bulguları B1 ve B2 kapandı; Unit 1757, Integration 1392; dal CI 42143462814 yeşil; K-notları D-126–D-128).

---

## 2026-11-06 — T207 doğrulama tur 1 PASS (merge #403)

2026-11-06 — T207 (Kimlik şemaları ve erişim reddi) bağımsız doğrulama tur 1: ✓ PASS (head `cade300`, PR #403; kriter 1–9 ve KK1–KK9 ✓; Unit 1628, Integration 1360 yerelde gerçek PostgreSQL 18'de yeşil, betik testleri yeşil; dal CI 42133128685 yeşil; bulgu 0; D-118 kapandı; K1 → D-120 (açık sayfa listesi adres şablonuyla, dönüş adresi kendi denetimiyle — `09 §15.1` m.2, §15.2 m.6 cümleleri karar kaydıyla hizalanır, F2 kapısı), K2 → D-121 (oturumu kapanmış POST'un 303'ü `notice=SessionExpired` taşımıyor — T214), K3 → D-122 (örnek üye kökü oturumsuz 404, `07` UC-33 302 ister; VAL-A609 gerilimi — T511); `12` VAL-A606 PASS, VAL-A602…A605, A607…A609, A736 BEKLEMEDE (kayıt ve ekranları sonraki task'larda)).

---

## 2026-11-06 — T211 yapım bitti, doğrulama bekliyor

2026-11-06 — T211 (Model bağlama kalıbı) yapım tamamlandı, doğrulama bekliyor (dal `task/T211-model-baglama-kalibi`; kriter 1–8, KK1–KK8 ve `09 §3.4` m.1–7 öz denetimde ✓; girdi doğrulama kalıbı yönetici kararıyla T211'de yazıldı, D-119 ✅; Unit 1757, Integration 1392; main ile birleştirildi (T207) yerelde gerçek PostgreSQL 18'de yeşil).

---

## 2026-11-06 — T207 yapım bitti, doğrulama bekliyor; T206 doğrulama tur 1 PASS

2026-11-06 — T207 (Kimlik şemaları ve erişim reddi) yapım tamamlandı, doğrulama bekliyor (dal `task/T207-kimlik-semalari-erisim-reddi`; Unit 1628, Integration 1360; D-118 kapandı). Önceki: T206 (Adres yapısı, önekler ve sayfa–form işleyici kalıbı) bağımsız doğrulama tur 1: ✓ PASS (head `686f019`, PR #401; kriter 1–9 ve KK1–KK9 ✓; Unit 1515, Integration 1324 yerelde gerçek PostgreSQL 18'de yeşil, betik testleri yeşil; dal CI 42123791130 yeşil; bulgu 0; K1 → D-117 (`HandlerRequiredFilter` PUT/PATCH/DELETE'e 400 verir, `07 §2.7.1` yöntem uymazsa 405 ister — T212), K2 → D-118 (`FormResults.IsLocalPath` sekme ve öteki denetim karakterlerini geçirir — T207), K3 D-73 ve D-116 metin düzeltmesi bu PR'da; `12`'de T206'nın satırı yok, `Durum` hücresi değişmedi).

---

## 2026-11-06 — T206 yapım bitti, doğrulama bekliyor

2026-11-06 — T206 (Adres yapısı, önekler ve sayfa–form işleyici kalıbı) yapım tamamlandı (dal `task/T206-adres-sayfa-form-kalibi`; kriter 1–9 ve KK1–KK9 öz denetimde ✓; Unit 1515 (Consistency 165 dahil), Integration Web 153 yerelde yeşil). Adres kuralları `Infrastructure/Routing/` (`AddressRules` beş önek, kapalı ilk bölüm listesi, kayıt adresi; `AddressTemplates`, `OrderRoutes` iki sipariş kökü, `AddressConvention` listede olmayan ilk bölümü açılışta reddeder), form sonuçları `Infrastructure/Forms/` (`FormResults` 303 / 422, `CsvFileResult`, `HandlerRequiredFilter`); taramalar `PageSourceScan` (API/sürüm, görünüm bağlamı, `post` formunda işleyici) ve `PageHandlerRuleTests`, ayrı ihlal örnekleriyle; eşitlik `AddressListConsistencyTests`. D-73 kapandı; D-116 açıldı (`09 §16.1` ile `07 §2.1.2` statik yol çelişkisi, T537). ⚠: işleyicisiz ya da bilinmeyen işleyicili POST 400 (`07 §2.7.1` "404 ya da 400"); yol deseni sonu `$` yerine `\z`; ÖRNEK sayfa `[IgnoreAntiforgeryToken]` yalnız test için, sahtecilik koruması T209'da; yerel `localhost` IPv6 gecikmesi canlılık testlerini kırar (CI etkilenmez).

---

## 2026-11-06 — T224 doğrulama tur 2 PASS

2026-11-06 — T224 (Çerez politikası) bağımsız doğrulama tur 2: ✓ PASS (head `b684a29`, PR #400; kriter 1–9 ve KK1–KK9 ✓; tur 1'in B1–B2'si ve K1–K2'si kapandı; Unit 1347, Integration 1258 yerelde yeşil; dal CI 42107824627 yeşil; bulgu 0; K1 → D-113 notu (`09 §15.1` m.6 ile sıra), K2 → D-114 (tarama kesinliği, T342), K3 → D-115 (kararsız canlılık testi, T212); `12` satırları A363–A367 BEKLEMEDE — akış task'ları). Önceki: T224 bağımsız doğrulama tur 1: ✗ FAIL (head `4737a61`, PR #400; B1 KK9 `~` — ödeme çerçevesi istisnası `object`/`embed`/`frameset`'i de serbest bırakıyor; B2 KK3 `~` — çerez taraması ham `Set-Cookie` başlık yazımını görmüyor; K1 kapının veritabanı hatası yakalama katmanının dışında, K2 çerçeve kaynağı — düzeltme turunda; Unit 1330, Integration 1258 yerelde yeşil; dal CI 42102317150 yeşil; `12` satırları değişmedi). Önceki: T224 yapım tamamlandı (kriter 1–9 ve KK1–KK9 öz denetimde ✓; ters vekil başlıkları D-112 → T215). Önceki: T223 (Dosya deposu) bağımsız doğrulama tur 2: ✓ PASS (head `6614402`, PR #399; kriter 1–9 ve KK1–KK9 ✓; tur 1'in B1–B3'ü ve K1–K4'ü kapandı; Unit 1288, Integration 1246 yerelde yeşil; dal CI 42089604427 yeşil; bulgu 0; K1–K5 → D-111; `12` satırları A186–A190, A465, A834 BEKLEMEDE — T339/T525/T302). Önceki: T204 (Dış servis çökmesi davranışı ve sahtelerle sınama) bağımsız doğrulama tur 1: ✓ PASS (head `548c4db`, PR #398; kriter 1–8 ve KK1–KK8 ✓; Unit 1228, Integration yerelde yeşil; dal CI 42077481379 yeşil; bulgu 0; K1–K2 → D-110; `12` satırları E056, E060–E066 sahip task'larda — BEKLEMEDE) (dal `task/T204-dis-servis-cokmesi`; sahtelere üç hata türü ve kesinti anahtarı, ödeme başlatma ve son sorgu kuralları `Modules/Payment/`, e-posta deneme kuralı `DispatchRetrySchedule` — ilk denemeden 5 dk / 30 dk / 2 sa, sonra "ulaşmadı" — ve tek denemenin yazımı `DispatchAttempts` — satır + kanal hâli tek işlemde, gönderici T305/T385/T407'de bağlanır (D-97), Google hata kuralı `Modules/Membership/` (kodların `NoticeCode`'a bağlanması D-109); e-posta ve Google ağ geçidinin kendi klasörleri dışında anılmaması taraması; Unit 1074 + Consistency 154, Integration 1157 yerelde yeşil).

---

## 2026-11-05 — T203 doğrulama tur 2 PASS

2026-11-05 — T203 (İmzalı makine ucu altyapısı) bağımsız doğrulama tur 2: ✓ PASS (head `af2e64f`, PR #397; kriter 1–9 ve KK1–KK9 ✓; tur 1 B1, B2 kapandı; gerçek Kestrel'de veritabanı hatası 500 + boş gövde görüldü; `VAL-E045` PASS, `E046`/`E047`/`E051`/`E052` PASS, `E050` ve öteki satırlar sahip task'larda — `BEKLEMEDE`; Unit 1176, Integration 1206 yerelde yeşil; dal CI 42056504243 yeşil; bulgu 0; K1 → D-108'e eklendi: sayılı pencere yeniden başlatmaya kadar kalır, T215; K2 → D-107 kapsar, T212) (dal `task/T203-imzali-makine-ucu`; ortak ardışık düzen `Infrastructure/SignedEndpoints/` — 64 KB tavan okumadan ve imzadan önce 413, imza ucun `Verify`ı ile 400 gövdesiz + seyreltilmiş güvenlik satırı, kayıt ve iş satırı tek işlemde, "alındı" kesinleşmeden sonra, veritabanı hatası yakalanmaz — tek yakalama katmanı 500 verir (T212, D-107); sabit zamanlı `SignatureComparison.Matches` ve kaynak taraması; gerçek uç T311, PayTR imzası T402).

---

## 2026-11-05 — T202 yapım bitti, doğrulama bekliyor

2026-11-05 — T202 (Dış servislerin bağlantı katmanı) yapım bitti, PR #396 açıldı, doğrulama bekliyor (dal `task/T202-dis-servis-katmani`; üç arayüz ve sekiz sonuç ailesi `Infrastructure/Integrations/`'ta, ortak çağrı yüzü `ExternalCallRunner` — TimeProvider'lı süre, tek çağrı, `ExternalCallFailed` kaydı; adlandırılmış HTTP istemcileri yeniden denemesiz; geliştirmede `SmtpCaptureEmailGateway` → Mailpit, ayar `compose.dev.yaml`; sahteler `UnitTests/Fakes/`; Unit 1126 yeşil; D-101 kapandı). Önceki giriş (T201 doğrulama tur 1 PASS) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-05 — T201 doğrulama tur 1 PASS

2026-11-05 — T201 (İmha düzenleyicisi, süre ayarları ve gecelik temizlik) bağımsız doğrulama tur 1: ✓ PASS (head `bed034e`, PR #395; kriter 1–9 ve KK1–KK9 ✓; parça başına tek işlemde silme + imha kaydı, kayıt reddinde silme geri alınır; kalıcı kayıt, hesap ve yönetici bağlantı kaydı, üye sepeti ve "başarısız" iş satırı yerinde; iz yalnız DB fonksiyonuyla; kayıt dosyası satırı dosyadan önce, döngü satırı aralıkla; `VAL-D046`/`D047`/`D078`/`D117`/`D118` PASS, `VAL-D048`–`D051`, `D080` sahip task'larda — `BEKLEMEDE`; Unit 1026, Integration 1192 yerelde yeşil; dal CI 42025909535 yeşil; bulgu 0; K1–K2 → D-104).

---

## 2026-11-05 — T201 yapım bitti, doğrulama bekliyor

2026-11-05 — T201 (İmha düzenleyicisi, süre ayarları ve gecelik temizlik) yapım bitti, PR #395 açıldı, doğrulama bekliyor (dal `task/T201-imha-duzenleyicisi`; gecelik işin ikinci adımı `DisposalStep`, parça başına tek işlemde silme ve imha kaydı; altı geçici kayıt parçası, işlem izi fonksiyonu ve kayıt dosyası parçası; misafir sepeti ve davet parçaları kendi modüllerinde; Unit 1026, `DisposalTests` 18; D-59 ve D-91 K1 ✅, yeni D-102, D-103). Önceki giriş (T221 doğrulama tur 3 PASS) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-05 — T221 bağımsız doğrulama tur 3: PASS

2026-11-05 — T221 (Zamanlayıcı) bağımsız doğrulama tur 3: ✓ PASS (head `f277e08`, PR #394; kriter 1–9 ve KK1–KK9 ✓; tur 2 B1 kapandı — `JobRunnerTests` kilit kopması testi yalnız kendi veritabanını vurur, `Jobs` ad alanı paralel koleksiyonlarla 3/3 yeşil; K1 `compose.dev` tanımı, K2 nabız sınırı `TimeProvider`'da; `VAL-C111` PASS, `VAL-C109`/`C110`/`A826`/`A827` iptal kaynağı gelince — `BEKLEMEDE`; Unit 996, Integration 1175 yerelde yeşil; dal CI 42017091348 yeşil; bulgu 0; K1 → D-101).

---

## 2026-11-05 — T221 yapım bitti, doğrulama bekliyor

2026-11-05 — T221 (Zamanlayıcı) yapım bitti, PR açıldı, doğrulama bekliyor (dal `task/T221-zamanlayici`; uygulamada tek `SchedulerService`: dakikalık tarama ve ayrı otuz saniyelik nabız döngüsü, zamanlayıcı ve gecelik iş oturum kilitleri, gecelik iş 03:00 / bir saatlik yineleme, kesinti kaydı + ilk nabız + taşıma tek işlemde, havale iptali erteleme; D-98 ve D-39 (f) kapandı, yeni D-99; Unit 996, Integration 1171, betik testleri 4/4 yerelde yeşil).

---

## 2026-11-05 — T220 doğrulama tur 1 PASS

2026-11-05 — T220 (İş kuyruğu çalıştırıcısı) bağımsız doğrulama tur 1: ✓ PASS (head `2deb469`, PR #393; kriter 1–9 ve KK1–KK9 ✓; tek `JobRunnerService`, oturum kilidi + `FOR UPDATE SKIP LOCKED`, işleyici işlem dışında, kesinleşince tek uyandırma, varsayılan 1/5/15/60/240 dk ve altıncı hatada `Failed`, başarısız satır silinmez, gönderilmiş kart iadesi yeniden gönderilmez; `KP-68` satırları e-posta task'larında, `VAL-A828`/`A823`/`A829`/`A987` işleyici ve gecelik iş gelince — `BEKLEMEDE`; Unit 972, Integration 1141 yerelde yeşil; dal CI 41994864604 yeşil; bulgu 0; K1 → D-98, K2 raporda düzeltildi).

---

## 2026-11-05 — T220 yapım bitti

2026-11-05 — T220 (İş kuyruğu çalıştırıcısı) yapım bitti, PR açıldı, doğrulama bekliyor (dal `task/T220-is-kuyrugu`; tek arka plan döngüsü `JobRunnerService`: tek çalışma oturum kilidi, sinyal ya da beş saniye, `FOR UPDATE SKIP LOCKED` ile satır alma, işleyici işlem dışında; `IJobQueue.EnqueueAsync` çağıranın işleminde ve kesinleşince tek uyandırma; varsayılan yeniden deneme 1/5/15/60/240 dk, altıncı hata `Failed`; kart iadesi kuralı; açılış kapısı, D-96 K1 kapandı; Unit 972, Integration 1141 yerelde yeşil; işleyiciler sahip task'larda → D-97). Önceki giriş (T222 doğrulama tur 1) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-05 — T222 doğrulama tur 1 PASS

2026-11-05 — T222 (Anahtarlar ve tek kullanımlık bağlantılar) bağımsız doğrulama tur 1: ✓ PASS (head `fcd3009`, PR #392; kriter 1–8 ve KK1–KK8 ✓; OS üreteci `SecretKey`, HMAC türetme `LinkKeys`, yalnız özet saklayan bağlantı kaydı, beş hâl, salt okuyan `ReadAsync`, tek koşullu `TryConsumeAsync` (eşzamanlı iki istekte biri), açılışta kuyruktan önce geri alma bağlantısı özetinin yeniden yazımı; KP satırı yok, ilgili `VAL-A616`/`A677` sayfa task'larında `BEKLEMEDE`; Unit 932, Integration 1093 yerelde yeşil; dal CI 41977809448 yeşil; bulgu 0; K1–K2 → D-96).

---

## 2026-11-05 — T222 yapım bitti

2026-11-05 — T222 (Anahtarlar ve tek kullanımlık bağlantılar) yapım bitti, PR açıldı, doğrulama bekliyor (dal `task/T222-anahtarlar-baglantilar`; `SecretKey`, `LinkKeys` (HMAC türetme, `DigestFor`), `LinkRecords` (hâl, salt okuma, tek koşullu tüketim, öncekini geçersiz kılma), açılışta geri alma bağlantısı özetinin yeniden yazımı `RevertLinkDigestRewrite(Startup)`; Unit 932, Integration 1093 (yerelde 1 ortam düşüşü, rapor ⚠ 7)). Önceki giriş (T228 doğrulama tur 1) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-05 — T228 doğrulama tur 1 PASS

2026-11-05 — T228 (Eşzamanlılık denetimi) bağımsız doğrulama tur 1: ✓ PASS (head `c67a1e2`, PR #391; kriter 1–9 ve KK1–KK9 ✓; tek işlem sınırı `ITransactionRunner`, kilit sırasını çalışma anında tutan `LockSequence`, sayaçların tek yazıcısı `StockCounters`, ürün ve sipariş sürüm bekçileri; KP-76 satırları uç düzeyinde, `BEKLEMEDE`; CI 41969947908 yeşil; Unit 897/897, Integration 1072/1072; bulgu 0; K1 → D-95, K2 raporda kapandı).

---

## 2026-11-05 — T225 doğrulama tur 3 PASS

2026-11-05 — T225 (Sistem kayıtları) bağımsız doğrulama tur 3: ✓ PASS (head `820fd66`, PR #389; kriter 1–7 ve KK1–KK7 ✓; tur 2 B1 kapandı — erişim kaydı sorguyu parça parça maskeler, çift sonrası çıplak belirteç dahil (gerçek `caddy:2.11.7-alpine` sondası, 24 istek); B2 kapandı — satır yalnız `05 §9.1.2` alanları ve zarf; `VAL-A991` PASS; CI 41962307990 yeşil; Unit 849/849, Integration 1046/1046; bulgu 0; K1 → D-93).

---

## 2026-11-05 — T232 doğrulama tur 2 PASS

2026-11-05 — T232 (Veri koruma halkası) bağımsız doğrulama tur 2: ✓ PASS (head `c8e798b`, PR #388; kriter 1–8 ve KK1–KK8 ✓; tur 1 B1 kapandı — veri erişim kategorileri uyarıda, yalnız iki EF hata olayı kimlikle süzülür, aynı kategorinin uyarısı yazılır; B2 kapandı — tarama alt amaç argümanını ve `GetDataProtector`'ı bulur; CI 41954716044 yeşil; Unit 811/811, Integration 1030/1030; bulgu 0; K1–K4 → D-90).

---

## 2026-11-05 — T231 doğrulama tur 1 PASS

2026-11-05 — T231 (Maskelemenin üç katmanının testleri) bağımsız doğrulama tur 1: ✓ PASS (head `e20aef8`, PR #387; kriter 1–7 ve KK1–KK7 ✓; yazım, yol, biçimleyici ve istisna katmanları birim testli, kaynak taraması ihlal örnekleriyle kırmızı; `PathMasker` boş bölümlü yolda anahtarı maskeler; CI 41948281331 yeşil; Unit 729/729; bulgu 0; K1–K3 → D-89). Önceki giriş (T230 doğrulama tur 2) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-05 — T230 doğrulama tur 2 PASS

2026-11-05 — T230 (Uygulama kayıtlarının üç yazıcısı ve satır biçimleyicisi) bağımsız doğrulama tur 2: ✓ PASS (head `9938934`, PR #386; kriter 1–9 ve KK1–KK9 ✓; tur 1 B1 kapandı — satırlar çatının sağlayıcılarına iletilmez, konteyner çıktısına uygulama satırı inmez; bozma sınaması: `writeToProviders: true` iken sağlayıcı testi, `retainedFileCountLimit: 31` iken KK3 testi kırmızı; CI 41943697506 yeşil; Unit 648/648, Integration 1018/1018; bulgu 0; K1–K2 → D-88).

---

## 2026-11-05 — T205 doğrulama tur 1 PASS

2026-11-05 — T205 (Açılışta veritabanı fail-fast denetimi) bağımsız doğrulama tur 1: ✓ PASS (head `501f37f`, PR #385; kriter 1–7 ve KK1–KK7 ✓; boş il/ülke listesi, altı kayıttan her biri ve tablosu olmayan kayıt açılışı adıyla reddeder, değer iletide yok, Hakkımızda/duyuru/teslimat illeri/taslak yokluğu açılışı engellemez, eksik ayar veritabanına sorulmadan reddeder; bozma sınaması: atma kapatılınca 11 test kırmızı; `package-build.sh` adım 5 kurucu SQL + göç demeti sonra uygulama, CI 41924447176 yeşil; Unit 548/548, Integration 1004/1004; bulgu 0; K1–K3 → D-86).

---

## 2026-11-05 — T205 yapım bitti

2026-11-05 — T205 (Açılışta veritabanı fail-fast denetimi) yapım bitti, doğrulama bekliyor (dal `task/T205-acilista-veritabani-fail-fast`, PR #385; `StartupDatabaseCheck` ikinci hosted service, `DatabaseAudit` saf işlev, varlığı `AppDbContext.ReadStartupRecordPresenceAsync` okur; il, ülke ve altı tek satırlık kayıt; Unit 548, Integration 1004).

---

## 2026-11-05 — T233 doğrulama tur 2 PASS

2026-11-05 — T233 (Veritabanı oturum süreleri) bağımsız doğrulama tur 2: ✓ PASS (head `cbdfe10`, PR #384; kriter 1–7 ve KK1–KK7 ✓; tur 1'in B1'i ve K2–K4 kapandı: testler süreleri sabitlerden türetir, web ev sahibinin `Web` profili testle korunur, `09 §10.7` madde 2–3 ve `11` kriter 4/KK4 "kilit ve sorgu sürelerinden uzun" diye daraltıldı (D-83 ✅); bozma sınamaları: `Program.cs` ham dizi, sabit 16 sn, `src/`'e `SET`/`SET LOCAL` ve gövdede `HttpClient` → kırmızı; Unit 543/543, Integration 987/987; K1 → D-85 (sabitlerin özet yorumları değeri yineler)).

---

## 2026-11-04 — T233 yapım bitti

2026-11-04 — T233 (Veritabanı oturum süreleri) yapım bitti, doğrulama bekliyor (dal `task/T233-veritabani-oturum-sureleri`; `DatabaseTimeouts` altı sabit, tek kuruluş noktası `DatabaseConnection.Configure` ve iki profil `Web`/`Maintenance`; web ev sahibi `Web`, göç hizmetinin Compose bağlantısı `Maintenance` ile bağlanır; iki yeni kaynak taraması (`SET` biçimleri, `ITransactionRunner` gövdesinde dış çağrı) kasıtlı ihlal örnekli; yeniden deneme stratejisi kapalı; Npgsql 10.0.3 ile `Options` ve `CommandTimeout` adları doğrulandı; D-22 ve D-26 kapandı). Doğrulama tur 1 (2026-11-04): ✗ FAIL (B1: testler süre sayılarını sabitten değil kendileri yazıyordu); düzeltme 1 (2026-11-05, head `cbdfe10`).

---

## 2026-11-04 — T226 doğrulama tur 1 PASS

2026-11-04 — T226 (Ayar katmanları, tasarım değerleri ve açılışta ayar denetimi) bağımsız doğrulama tur 1: ✓ PASS (head `2381b8b`, PR #383; kriter 1–6 ve KK1–KK6 ✓; derlenmiş uygulamayla gerçek açılış sondası: ürün sabiti adlı ayar, eksik gönderen adresi, kısa türetme sırrı, veritabanı yokluğu ve tip hatası açılışı durdurdu, iletide değer yok; sekiz kasıtlı bozma ilgili testleri kırmızıya çevirdi; Unit 520/520, Integration 980/980; dal CI 41898811183 yeşil; bulgu 0; K1 → D-80, K2 → D-81). Merge yardımcıda. Önceki giriş (T226 yapım bitti) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T226 yapım bitti

2026-11-04 — T226 (Ayar katmanları, tasarım değerleri ve açılışta ayar denetimi) yapım bitti, doğrulama bekliyor (dal `task/T226-ayar-katmanlari`; ayar sınıfları ve açılış denetimi `Infrastructure/InstallationSettings/`, beş sır ayarı sır dosyasından, iki yeni tarama kuralı ihlal örnekli; ayrıntı `TASK_REPORTS/T226_REPORT.md`). Önceki giriş (T227 doğrulama tur 1 PASS) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T227 doğrulama tur 1 PASS

2026-11-04 — T227 (Zaman ve para) bağımsız doğrulama tur 1: ✓ PASS (head `cdc6648`, PR #382; kriter 1–7 ve KK1–KK7 ✓; kuruş yuvarlaması ve orantılı bölme sınır değerleriyle yeniden sınandı (0,015 → 0,02, 0,025 → 0,03, 9,595 → 9,60, 4,995 → 5,00; 10 TL 50:100:150 → 1,66 + 3,33 + 5,01; 0,02 TL üç eşit ağırlık → 0,02 + 0 + 0); on kaynak mutasyonu ilgili testleri kırmızıya çevirdi — `TurkeyTime.DateOf`'un sunucu dilimiyle okunması +03:00 makinede yalnız mimari testle yakalandı; Unit 458/458 (Consistency dahil); dal CI 41889663878 yeşil; bulgu 0; K1 → D-76, K2 → D-77). Merge yardımcıda. Önceki giriş (T227 yapım bitti) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T227 yapım bitti

2026-11-04 — T227 (Zaman ve para) yapım bitti, doğrulama bekliyor (dal `task/T227-zaman-ve-para`, PR #382; tek zaman kaynağı `TimeSource` (`TimeProvider.System` tekil), sabit +03:00 `TurkeyTime` (gün, gün başı, gün süresinin sonu), ürünle gelen tatil dosyası ve `BusinessCalendar` (2026–2027; kapsam dışı yılda yalnız hafta sonu), `Money.Round` (tam yarım yukarı) ve `Money.Allocate` (parçalar aşağı, kalan en büyük ağırlığa); derlenmiş kod üzerinden zaman ve para mimari testi ile göç dışı SQL saat taraması, ikisi de ihlal örnekli; kriter 1–7 ve KK1–KK7 öz denetimde ✓; Unit 458/458 (Consistency 101 dahil); D-74, D-75 açıldı; T218 Commit hücresi `d82b943`). Önceki giriş (F1 gate check PASS) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — F1 gate check PASS

2026-11-04 — F1 (Veri katmanı) gate check: ✓ PASS (main `c59321d`; T101–T120; Consistency 101/101, Unit 241/241, Integration 952/952, betik testleri 4 dosya; Release `-warnaserror` 0/0; yerel referans kurulumda 18 göç ileri ve idempotent, seed sayıları beklenen, canlılık 200; matriste F1'e bağlı 144 satır implement edildi, S3 0; `VAL-A838` PASS; S2 yok). Gate PR'ı bu commit'i taşır; merge ve `phase/F1-pass` etiketi yardımcıda. Önceki giriş (T120 doğrulama tur 2 PASS) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T120 doğrulama tur 2 PASS

2026-11-04 — T120 (Model eşitlik denetimleri) bağımsız doğrulama tur 2: ✓ PASS (head `e0827c3`, PR #378; kapsam tam, 8 kriter ve KK1–KK8 ✓; tur 1 bulgusu kapandı: §7.2.1'de kendi dizini vaat edilmeyen bağa eklenen tek sütunlu dizin artık §7.5.1 denetimini kırar — kaynak mutasyonuyla sınandı; KK1–KK6 kaynak mutasyonlarıyla (davranış, tekil dizin ve filtresi, kısıt, sorgu ve bağ dizini, saklanan alan) yeniden sınandı; 130 çizgi = 130 satır = 130 FK; Consistency 101/101, Unit 241/241; dal CI `41855256783` yeşil; bulgu 0; K2 bu PR'da kapandı, K1 → D-70). Merge yardımcıda. Önceki giriş (T119 doğrulama tur 1 PASS) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T119 doğrulama tur 1 PASS

2026-11-04 — T119 (Başlangıç verisi göçü ve seed olmayanların sınırı) bağımsız doğrulama tur 1: ✓ PASS (head `7bf60e6`, PR #377; kapsam tam, 8 kriter ve KK1–KK8 ✓; 81 il plakayla, 249 ülke, altı tek satır, 81 teslimat ili ve iki yayımlanmamış taslak üretilen SQL'de ve gerçek PostgreSQL'de doğrulandı; `company_settings` varsayılanları `02 §11.1` ile birebir (P-1 0,00 · P-3/P-4 boş · P-5 2 · P-7 2 · P-8 5 · P-10 20,00), eşitlik denetimi mutasyonla sınandı; D-52 genişletme göçü 11 sütun `DEFAULT`; ikinci uygulama değer değiştirmez; Unit 267/267 (Consistency dahil), Integration 952/952; dal CI `41843874734` yeşil; bulgu 0; K1 → D-69, K2 bu PR'da kapandı). Merge yardımcıda. Önceki giriş (T118 doğrulama tur 1 PASS) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T118 doğrulama tur 1 PASS

2026-11-04 — T118 (Zamanlayıcı, yedek, imha ve anahtar kayıtları) bağımsız doğrulama tur 1: ✓ PASS (head `01d6473`, PR #376; kapsam tam, 8 kriter ve KK1–KK8 ✓; altı tablonun sütun, varsayılan, dizin ve kontrol kısıtları `06 §4.59`, §4.64–§4.68, §5, §7.1–§7.5 ile birebir; bekleyen iş tekil dizini `NULLS NOT DISTINCT`; anahtar halkası tamsayı kimlik, `created_at` yok; yetki ayrımı rol ayarından — mutasyonla sınandı, iki deyim kaldırılınca beş test kırmızı; D-39 (a) betiği sabit deyimli, yerel soket, parolasız; yeni paket MIT, kilitli; Unit 245/245, betik 4/4, Integration 926 (T118 sınıfı 55/55); dal CI `41835537615` yeşil; bulgu 0; K-notu yok). Merge yardımcıda. Önceki giriş (T118 yapım bitti) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T118 yapım bitti

2026-11-04 — T118 (Zamanlayıcı, yedek, imha ve anahtar kayıtları) yapım bitti, doğrulama bekliyor (dal `task/T118-zamanlayici-yedek-imha-anahtar`, PR #376; altı tablo tek göçle, `Down` yok; bekleyen iş (tür, ilgili kayıt) `NULLS NOT DISTINCT` tekil; nabız ve yedek durumu tek satır; kesinti > 2 dk; imha sayı/tarih türe göre; anahtar halkası paketin şeması, salt okuma kullanıcısına kapalı; durum kullanıcısına iki sütunda güncelleme — adlar rol ayarından; D-39 (a) kapandı: veritabanı yedeği sonucunu durum kaydına yazar; kriter 1–8 ve KK1–KK8 öz denetimde ✓; Unit 245/245, Integration 926, betik testleri 4/4). Önceki giriş (T117 doğrulama tur 1 PASS) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T117 doğrulama tur 1 PASS

2026-11-04 — T117 (Gönderim kaydı ve kanal hâli) bağımsız doğrulama tur 1: ✓ PASS (head `ba81b23`, PR #375; kapsam tam, 6 kriter ve KK1–KK6 ✓; iki tablonun sütun, varsayılan, dizin, FK ve kontrol kısıtları `06 §4.58`, §4.60, §2.2.5, §2.2.6, §7.1–§7.5 ile birebir; türe göre sipariş ve ilgili kayıt türü `CASE` kısıtı tür tablosunun 11 satırıyla eşit, boş tür `IN` listesinden geçmiyor; `Undelivered`'da gönderim anı iki yönlü (K-1120); sipariş bağı `NO ACTION`, ilgili kayıt ve ilk e-posta değer; kanal hâli tek satır; Unit 245/245, Integration 871 (T117 sınıfı 30/30); dal CI `41827953732` yeşil; bulgu 0; K-notu yok). Merge yardımcıda.

---

## 2026-11-04 — T117 yapım bitti

2026-11-04 — T117 (Gönderim kaydı ve kanal hâli) yapım bitti, doğrulama bekliyor (dal `task/T117-gonderim-kaydi-kanal-hali`, PR #375; `DispatchRecord` ve `ChannelStateRecord` `Notifications` modülünde tek göçle, `Down` yok; sipariş bağı kısıtlayıcı, ilgili kayıt ve ilk e-posta değer; türe göre sipariş ve ilgili kayıt türü kontrol kısıtı, `Undelivered` satırında gönderim anı dolu da boş da kabul; kanal hâli tek satır, ilk hata anı sayı sıfırsa ve yalnız o zaman boş; kriter 1–6 ve KK1–KK6 öz denetimde ✓; Unit 245/245, Integration 871; öncesi: T116 doğrulama tur 1 PASS, giriş `STATUS_CHANGELOG.md`'ye taşındı).

---

## 2026-11-04 — T116 doğrulama tur 1 PASS

2026-11-04 — T116 (Galeri bağları) bağımsız doğrulama tur 1: ✓ PASS (head `2c19c0c`, PR #374; kapsam tam, 6 kriter ve KK1–KK6 ✓; beş tablonun sütun, varsayılan, dizin ve FK'leri `06 §4.71`, §2.2.5, §7.1, §7.2 ile birebir; (sahip, görsel) çifti tekil, `is_main` filtreli tekil dizin, sahip bağı basamaklı, görsel bağı `NO ACTION`, sıra tekil değil; Unit 245/245, Integration 841 (T116 sınıfı 47/47); dal CI `41822520597` yeşil; bulgu 0; K-notu yok). Merge yardımcıda.

---

## 2026-11-04 — T116 yapım bitti

2026-11-04 — T116 (Galeri bağları) yapım bitti, doğrulama bekliyor (dal `task/T116-galeri-baglari`; beş galeri bağı tek göçle, `Down` yok; öncesi: T115 doğrulama tur 1 PASS, giriş `STATUS_CHANGELOG.md`'ye taşındı).

---

## 2026-11-04 — T115 doğrulama tur 1 PASS

2026-11-04 — T115 (Şube, içerik–ürün bağları ve iletişim talebi) bağımsız doğrulama tur 1: ✓ PASS (head `cbced3e`, PR #373; kapsam tam, 7 kriter ve KK1–KK7 ✓; dört tablonun sütun, varsayılan, dizin, FK ve kontrol kısıtları `06 §4.50`, §4.53, §4.57, §7.1–§7.5 ile birebir; telefon `^[0-9]{10}$`, görselsiz şubede alt metin NULL, yayın durumu iki değer, il ve görsel bağı `NO ACTION`, içerik–ürün çifti tekil ve iki uca basamaklı, kapalı talepte kapanış anı dolu, imha taramasının ifade dizini; Unit 245/245, Integration 794 (T115 sınıfı 39/39); dal CI `41818311284` yeşil; bulgu 0; K-notu yok). Merge yardımcıda.

---

## 2026-11-04 — T115 yapım bitti

2026-11-04 — T115 (Şube, içerik–ürün bağları ve iletişim talebi) yapım bitti, doğrulama bekliyor (dal `task/T115-sube-icerik-urun-iletisim`, PR #373; dört entity — `Branch`, `OfferingProduct`, `PortfolioItemProduct`, `ContactRequest` — `CorporateContent` modülünde tek göçle, `Down` yok; şube ve iletişim talebi telefonu on rakam, görselsiz şubede alt metin boş, içerik–ürün çifti tekil ve içerik kaydıyla ve ürünle basamaklı, kapalı talepte son kapanış anı dolu, talebin imha taraması için ifade dizini; kriter 1–7 ve KK1–KK7 öz denetimde ✓; Unit 245/245, Integration T115 sınıfı 39/39). Önceki giriş (T114 doğrulama tur 1) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T114 doğrulama tur 1 PASS

2026-11-04 — T114 (İçerik kayıtları ve marka) bağımsız doğrulama tur 1: ✓ PASS (head `c976977`, PR #372; kapsam tam, 9 kriter ve KK1–KK9 ✓; sekiz tablonun sütun, varsayılan, dizin, FK ve kontrol kısıtları `06 §4.46`…§4.55, §7.1–§7.5 ile birebir; üç tek satırlık tablo `((true))` tekil dizinli, `Slug` filtresiz tekil ve biçim kısıtlı, marka rengi `^#[0-9A-F]{6}$`, logo ve simge bağı `NO ACTION`; Unit 245/245, Integration 882 (T114 sınıfı 79/79); bekleyen model değişikliği yok; dal CI `41812838046` yeşil; bulgu 0; K-notu yok — yerel sınıf temizliği zaman aşımı zaten D-68'de). Merge yardımcıda.

---

## 2026-11-04 — T114 yapım bitti

2026-11-04 — T114 (İçerik kayıtları ve marka) yapım bitti, doğrulama bekliyor (dal `task/T114-icerik-kayitlari-marka`; sekiz entity — `AboutPage`, `Offering`, `PortfolioItem`, `FaqItem`, `CustomPage`, `Announcement`, `SocialLink`, `Branding` — `CorporateContent` modülünde tek göçle, `Down` yok; üç tek satırlık tablo sabit ifadenin tekil dizinini taşır; yayın durumu yalnız taslak ve yayımlanmış; `CorporateContentRecordsGuard` hakkımızda ve marka satırının silinmesini reddeder; kriter 1–9 ve KK1–KK9 öz denetimde ✓; Unit 245/245, Integration yerel 754/755, kalan bir test yerel zaman aşımı). Önceki giriş (T113 doğrulama tur 1) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T113 doğrulama tur 1 PASS

2026-11-04 — T113 (İade reddi ve IBAN isteği) bağımsız doğrulama tur 1: ✓ PASS (head `429ce56`, PR #371; kapsam tam, 7 kriter ve KK1–KK7 ✓; `refund_by_reason` ve `itemless_on_refund_failure` `06 §4.29`/§7.3 ile birebir, iki filtreli tekil dizinin filtresi `06 §7.1` ile eşit; FK silme davranışları `06 §7.2` ile birebir; iade reddi bekçide silinmez, IBAN isteği imhanın açık silmesine açık; Unit 245/245, Integration 676 (T113 sınıfı 45/45); bekleyen model değişikliği yok; dal CI `41807318144` yeşil; bulgu 0; K1 → D-68 (yerel test altyapısında sınıf veritabanı düşürme zaman aşımı)). Merge yardımcıda.

---

## 2026-11-04 — T113 yapım bitti

2026-11-04 — T113 (İade reddi ve IBAN isteği) yapım bitti, doğrulama bekliyor (dal `task/T113-iade-reddi-iban-istegi`, PR #371; iki entity — `ReturnRejection`, `RefundIbanRequest` — `AfterSales` modülünde tek göçle, `Down` yok; iade reddinin silinmesini `AfterSalesRecordsGuard` reddeder, IBAN isteği imhanın açık silmesine açık; kriter 1–7 ve KK1–KK7 öz denetimde ✓; Unit 245/245, Integration 676 (T113 sınıfı 45/45); D-55 eki (numaralı dizin adları)).

---

## 2026-11-04 — T112 doğrulama tur 1 PASS

2026-11-04 — T112 (Ödeme ve geri ödeme kayıtları) bağımsız doğrulama tur 1: ✓ PASS (head `d452256`, PR #370; kapsam tam, 7 kriter ve KK1–KK7 ✓; tutar sütunları `numeric(12,2)` (`06 §2.5.1`), `ck_refunds_links_by_type` `06 §4.33` tür tablosuyla birebir, altı filtreli tekil dizin `06 §7.1` ile eşit; FK ve dizinler `06 §7.2`, §7.5 ile birebir; Unit 245/245, Integration 631 (T112 sınıfı 85/85); bekleyen model değişikliği yok; dal CI `41802135141` yeşil; bulgu 0; K-notu yok). Merge yardımcıda. Önceki giriş (T112 yapım bitti) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T112 yapım bitti

2026-11-04 — T112 (Ödeme ve geri ödeme kayıtları) yapım bitti, doğrulama bekliyor (dal `task/T112-odeme-geri-odeme-kayitlari`, PR #370; dört entity — `PaymentRecord`, `Refund`, `RefundRecord`, `InboundNotificationRecord` — `Payment` modülünde tek göçle, `Down` yok; üç kalıcı kaydın silinmesini `PaymentRecordsGuard` reddeder; kriter 1–7 ve KK1–KK7 öz denetimde ✓; Unit 245/245, Integration 631 (T112 sınıfı 85/85); D-51 eki (iki kırpılmış ad), D-55 eki (numaralı dizin adları)). Önceki giriş (T111 doğrulama tur 1 PASS) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T111 doğrulama tur 1 PASS

2026-11-04 — T111 (Kalem kayıtları) bağımsız doğrulama tur 1: ✓ PASS (head `6cefd1c`, PR #369; kapsam tam, 8 kriter ve KK1–KK8 ✓; tutar sütunları `numeric(12,2)` ve tutar kısıtları `06 §2.5`, §4.22–4.25 ile eşit; FK ve dizinler `06 §7.2`, §7.5 ile birebir; Unit 245/245, Integration 546 (T111 sınıfı 68/68); yetki matrisi sapmasız (59/59); dal CI `41796325905` yeşil; bulgu 0; K1 → D-67, K2 → D-51 eki). Merge yardımcıda. Önceki giriş (T111 yapım bitti) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T111 yapım bitti

2026-11-04 — T111 (Kalem kayıtları) yapım bitti, doğrulama bekliyor (dal `task/T111-kalem-kayitlari`; sekiz entity `AfterSales` modülünde tek göçle, `Down` yok; altı kalıcı kaydın silinmesini `AfterSalesRecordsGuard` reddeder; kriter 1–8 ve KK1–KK8 öz denetimde ✓; Unit 245/245, Integration 546 (T111 sınıfı 68/68); D-67 yeni — `DefectClaim.StatusHistory` düz `jsonb`, hedef T320; D-51 eki). Önceki giriş (T110 doğrulama tur 1 PASS) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T110 doğrulama tur 1 PASS

2026-11-04 — T110 (Ayırma ve düzeltme geçmişleri) bağımsız doğrulama tur 1: ✓ PASS (head `a7954b7`, PR #367; kapsam tam, 7 kriter ve KK1–KK7 ✓; 15 ham SQL yoklaması, `confdeltype` `06 §7.2` ile eşit; Unit 245/245, Integration 478 (T110 sınıfı 28/28); yetki matrisi 8/8; dal CI `41784121098` yeşil; bulgu 0; K1 → D-66 — iki geçmişin public ayarlayıcıları, hedef T375). Merge yardımcıda. Önceki giriş (T110 yapım bitti) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T110 yapım bitti

2026-11-04 — T110 (Ayırma ve düzeltme geçmişleri) yapım bitti, doğrulama bekliyor (dal `task/T110-ayirma-duzeltme-gecmisleri`; `StockReservation`, `OrderEmailHistory`, `OrderAddressHistory` tek göçle, `Down` yok; iki geçmiş `OrderingRecordsGuard` ile yalnız eklenir; Unit 245/245, Integration 478/478 (T110 sınıfı 28/28); D-54 ✅ — kupon bağı kısıtlayıcı, ham `DELETE` `fk_stock_reservations_coupons_coupon_id` ile reddedilir).

---

## 2026-11-04 — T109 doğrulama tur 1 PASS

2026-11-04 — T109 (Sepet ve sipariş çekirdeği) bağımsız doğrulama tur 1: ✓ PASS (head `de1ae6c`, PR #366; kapsam tam, 8 kriter ve KK1–KK8 ✓; ~95 ham SQL yoklaması, tutar sütunları `numeric(12,2)`; Unit 245/245, Integration 450 (T109 sınıfı 65/65); yetki matrisi sapmasız; dal CI `41780080190` yeşil; bulgu 0; K1 → D-65 — `Order.ShippingFeeVatBreakdown` `PayloadJson` dönüştürücüsü bekliyor; `12`'de T109'a bağlı altı satır BEKLEMEDE). Merge yardımcıda. Önceki giriş (T109 yapım) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T109 yapım bitti

2026-11-04 — T109 (Sepet ve sipariş çekirdeği) yapım bitti, doğrulama bekliyor (dal `task/T109-sepet-siparis-cekirdegi`; sekiz tablo tek göçle, `Down` yok; `OrderingRecordsGuard`; Unit 245/245, Integration 450/450 (T109 sınıfı 65/65); D-54'ün T109 payı kapandı, T110 payı açık; D-51'e dört kırpılmış ad eklendi). Önceki giriş (T108 doğrulama tur 2) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T108 doğrulama tur 2 PASS

2026-11-04 — T108 (Oturum, bağlantı, deneme ve giriş kayıtları) bağımsız doğrulama tur 2: ✓ PASS (head `04113f7`, PR #365; kapsam tam, 8 kriter ✓; tur 1 bulgusu — kayıtların yeri — giderildi; Unit 245/245, Integration 385 (T108 sınıfı 70/70); dal CI `41775169027` yeşil; bulgu 0; K1 → D-64 — normalleştirilmiş değerin tek yazarı `09 §9.4` m.1 ile `§15.4` m.5–6 arasında; `12`'de T108'e bağlı beş satır BEKLEMEDE). Merge yardımcıda. Önceki giriş (T108 yapım ve düzeltme 1) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T108 yapım bitti, doğrulama tur 1 FAIL ve düzeltme 1

2026-11-04 — T108 (Oturum, bağlantı, deneme ve giriş kayıtları) yapım bitti; doğrulama tur 1 FAIL sonrası düzeltme 1 yapıldı (beş varlık Infrastructure/Security/ altına taşındı, D-62 kapandı), doğrulama tur 2 bekliyor (dal `task/T108-oturum-baglanti-deneme-giris`): beş entity (`Session`, `RecognizedBrowserMark`, `LinkRecord`, `AttemptRecord`, `LoginRecord`) tek göçle; dört modüller arası bağ D-61 kalıbıyla adla kuruldu ve D-61 kapandı (`09 §10.1` + kaynak taraması). Önceki giriş (T107 doğrulama tur 1) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T107 doğrulama tur 1 PASS

2026-11-04 — T107 (Müşteri hesabı) bağımsız doğrulama tur 1: ✓ PASS (head `e765c68`, PR #364; kriter 1–6 ve KK1–KK6 ✓; Unit 240/240, Integration 315/315, betik testleri 4/4; dal CI `41769181335` yeşil; bulgu 0; K-notu yok — modüller arası bağın ad kalıbı D-61'de, gerekçesi yerinde; `12`'de T107'ye bağlı satır yok, `VAL-D121` BEKLEMEDE). Merge yardımcıda. Önceki giriş (T107 yapım) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T107 yapım bitti

2026-11-04 — T107 (Müşteri hesabı) yapım bitti, doğrulama bekliyor (dal `task/T107-musteri-hesabi`): üç entity (`Customer`, `PendingEmailChange`, `AddressBook`) tek göçle; ilk modüller arası yabancı anahtarlar adla bağlandı (⚠ D-61); Unit 240/240, T107 sınıfı 39/39. Önceki giriş (T106 doğrulama tur 3) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T106 doğrulama tur 3 PASS

2026-11-04 — T106 (Yönetici hesabı, davet ve işlem izi) bağımsız doğrulama tur 3 (eskalasyon sonrası): ✓ PASS (head `d3baaa6`, PR #363; kapsam tam; kriter 1–7 ve KK1–KK7 ✓; tur 1–2'nin sır bekçisi bulguları kapandı; Unit 240/240, Integration 276/276, bekçi betiği 208/208; dal CI `41765579435` yeşil; bulgu 0; `VAL-A838` PASS; K1 → D-59 — imha testlerinin sabit 2020 tarihi 2031'de eskir, K2 → D-60 — bekçi istisnasının kalan dar açıkları README'de). Merge yardımcıda.

---

## 2026-11-04 — T106 yapım bitti, doğrulama tur 1–2 FAIL ve düzeltmeler

2026-11-04 — T106 (Yönetici hesabı, davet ve işlem izi) yapım bitti, doğrulama bekliyor: üç entity (`Admin`, `AdminInvitation`, `AuditLog`) tek göçle; işlem izi uygulama kullanıcısına yalnız `INSERT`/`SELECT`, imha fonksiyonu `dispose_expired_audit_logs`; `AuditLog.Operation` düz metin, `Details` `string` + `jsonb`; `MigrationSourceScan` genişledi; sır bekçisine EF göç sütun kurucusu istisnası; D-48 (a) ve D-49 ✅, D-57 açıldı; Unit 240/240, Integration 276/276. Doğrulama tur 1 FAIL (sır bekçisi istisnası geniş); düzeltme 1: istisna yalnız `type: "…"` dizgisine daraltıldı, tur 2 FAIL (açık çok satırlı biçim istisna dışında kalmalı); düzeltme 2: istisna yalnız aynı satırda kapanan `table.Column<Tür>(…)` biçimine uygulanır, yeniden doğrulama bekliyor. Önceki giriş (T105 doğrulama tur 1) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T105 doğrulama tur 1 PASS

2026-11-04 — T105 (Fiyat, kupon ve dijital dosya) bağımsız doğrulama tur 1: ✓ PASS (head `67e983a`, PR #362; kriter 1–7 ve KK1–KK7 ✓ — göç PostgreSQL 18'e uygulandı, 34 sütun, kısıtlar, dizinler ve silme eylemleri `06` ile birebir, 42 ham SQL yoklamasıyla sınandı; Unit 230/230, Integration 234/234; dal CI `41749429553` yeşil; bulgu 0; K1 → D-56 — `06 §7.3`'ün kupon satırında yüzde aralığı yok). Merge yardımcıda.

---

## 2026-11-04 — T105 yapım bitti

2026-11-04 — T105 (Fiyat, kupon ve dijital dosya) yapım bitti: dört entity (`PriceHistory`, `Discount`, `Coupon`, `DigitalFile`) tek göçle; fiyat geçmişi güncellemesi ve kupon silmesi `CatalogRecordsGuard` ile reddedilir; Unit 230/230, Integration 234/234; yeni D-54 (bağlı kuponun DB testi T109/T110'a), D-55 (aynı sütunda iki dizinin `…_id1` adı). Doğrulama bekliyor. Önceki giriş (T104 doğrulama tur 3 PASS) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T104 doğrulama tur 3 PASS

2026-11-04 — T104 (Ürün çekirdeği ve görsel) bağımsız doğrulama tur 3: ✓ PASS (head `fa4c2a0`, PR #361; kapsam tam; kriter 1–8 ve KK1–KK8 ✓ — göç PostgreSQL 18'e uygulandı, altı sütunun veritabanı varsayılanı `06` ile birebir (tur 2 bulgusu kapandı), kısıtlar ve silme eylemleri 22 ham SQL yoklamasıyla sınandı; Unit 230/230, Integration 187/187; dal CI `41743500401` yeşil; bulgu 0; K1 → D-53 — kaynak kopyanın 2560 piksellik kenarı `ImagingLimits` gelince tek yerde kalmalı). Merge yardımcıda. Önceki giriş (T104 düzeltme 2 ve doğrulama tur 2 FAIL) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T104 düzeltme 2 ve doğrulama tur 2 FAIL

2026-11-04 — T104 düzeltme 2: altı sütuna veritabanı varsayılanı verildi (`HasDefaultValue`, göç yerinde; Unit 230, Integration 187), yeniden doğrulama bekliyor. Önceki: T104 (Ürün çekirdeği ve görsel) bağımsız doğrulama tur 2: ✗ FAIL (head `1b9132d`, PR #361; kapsam tam; kriter 2–8 ve KK1–KK8 ✓, tur 1'in üç bulgusu kapandı, D-50 (a)(b)(c) doğru; Unit 230/230, Integration 185/185; dal CI `41739943430` yeşil; bulgu 1 S1 DÜŞÜK, kriter 1: `06 §4` girişi "Varsayılan veritabanının varsayılanıdır" der, altı sütunda DB default yok — `products.publish_status` 'Draft', `products.version` 1, `variants.publish_status` 'Draft', `variants.reserved_quantity` 0, `product_categories.is_primary_category` false, `product_images.is_main` false; ham `INSERT` 23502 ile düşüyor; K1 → D-52 — aynı boşluk T103'te). İkinci iterasyon FAIL → `12 §8.5` eskalasyon — yardımcı karar verecek. Önceki giriş (T104 doğrulama tur 1 FAIL) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T104 doğrulama tur 1 FAIL

2026-11-04 — T104 (Ürün çekirdeği ve görsel) bağımsız doğrulama tur 1: ✗ FAIL (head `b4c73c8`, PR #361; kriter 1–8 ve KK1–KK8 ✓ — göç PostgreSQL 18'e uygulandı, silme eylemleri `pg_constraint`'te, trigram dizini planda sınandı; Unit 225/225, Integration 183/183; dal CI `41735064804` yeşil; `09 §25` üç soruda "hayır" → bulgu 3, hepsi S1 DÜŞÜK: (1) trigram dizinini kuran göçün ilk deyiminde `pg_trgm` denetimi yok ve `09 §10.6` m.11'in iki testi eksik — eklentisiz veritabanında göç çatının iletisiyle (`42704 operator class "gin_trgm_ops" does not exist`) düşüyor; (2) `Variant.Sku`/`NormalizedSku` — `06 §4.6` ve §4'ün girişi `SKU`/`NormalizedSKU` der; (3) `TextNormalizer.Simplify` Latin olmayan harflerin de işaretini atıyor (`й`→`и`, `ά`→`α`, `कु`→`क`) — `09 §9.4` m.2 "yalnız aksanlı Latin harfler; başka karaktere dokunulmaz"; K1 → D-51, K2 → D-45). Sırada: T104 düzeltme 1. Önceki giriş (T103 doğrulama tur 2 PASS) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T103 doğrulama tur 2 PASS

2026-11-04 — T103 (Başlangıç listeleri, firma ve yasal metin sürümü) bağımsız doğrulama tur 2: ✓ PASS (head `7c04318`; kapsam tam; kriter 1–7 ve KK1–KK7 ✓ — göç kurucu SQL'le kurulan PostgreSQL 18'e göç kullanıcısıyla uygulandı, KK2–KK7 uygulama ve salt okuma kullanıcısıyla ham SQL'le de sınandı; Unit 217/217, Integration 123/123; dal CI `41725873130` yeşil; tur 1 bulgusu kapandı — tek satırlık iki tabloda yalnız `pk_*` ve `ix_*_single_row`, yeni dizin kuralı öteki tablolarda çerçevenin kuralıyla aynı dizinleri üretir; bulgu 0, K1–K3 → D-50). PR #360 merge ve post-merge CI yardımcıda. Önceki giriş (T103 doğrulama tur 1 FAIL) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-04 — T103 doğrulama tur 1 FAIL

2026-11-04 — T103 (Başlangıç listeleri, firma ve yasal metin sürümü) bağımsız doğrulama tur 1: ✗ FAIL (`6e88812`; kapsam tam; kriter 1–7 ve KK1–KK7 ✓ — göç kurucu SQL'le kurulan PostgreSQL 18'e uygulandı, KK2–KK6 ham SQL'le de sınandı; dal CI run 41721390301 yeşil; bulgu 1 S1 DÜŞÜK: tek satırlık `companies` ve `company_settings`'te yabancı anahtar dizini var, `06 §7.5.2` "yalnız tek satırın tekil dizini" der — kaldırılabilir olduğu gösterildi; K1, K3 düzeltme turunda, K2 → D-49). Sırada: T103 düzeltme 1. Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T103 yapım bitti).

---

## 2026-11-04 — T103 yapım bitti

2026-11-04 — T103 (Başlangıç listeleri, firma ve yasal metin sürümü) yapım bitti: altı tablo tek göçle, tekil satır dizinleri, kontrol kısıtları, kalıcı satır bekçisi, göç geçmişi tablosu kapatma; Unit 217/217 + Integration 119/119 (PostgreSQL 18); D-37 (a), D-46 (a)(b), D-48 (b) çözüldü. Önceki giriş (T102 doğrulama tur 1) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-03 — T102 doğrulama tur 1 PASS

2026-11-03 — T102 (Enum tanımları ve değer dönüşümü) bağımsız doğrulama tur 1: ✓ PASS (head `8277427`; kriter 1–6 ve KK1–KK6 ✓; Unit 199/199, Integration 94/94 PostgreSQL 18; dal CI `41712894839` yeşil; bulgu 0, K1–K2 → D-48; D-47 hedefleri düzeltildi). PR #359 merge ve post-merge CI yardımcıda.

---

## 2026-11-03 — T102 yapım bitti

2026-11-03 — T102 (Enum tanımları ve değer dönüşümü) yapım bitti: 42 enum, metin dönüşümü, izinli değer kısıtı, kilit dosyaları; Unit 199/199, Integration 94/94 (PostgreSQL 18); doğrulama bekliyor.

---

## 2026-11-03 — T101 doğrulama tur 1 PASS

2026-11-03 — T101 (Veri modeli temeli) bağımsız doğrulama tur 1: ✓ PASS (head `f7b33c7`; kriter 1–8 ve KK1–KK8 ✓; Unit 128/128, Integration 81/81 PostgreSQL 18.6; dal CI `41706961892` yeşil; bulgu 0, K1–K3 → D-46). PR #358 merge ve post-merge CI yardımcıda.

---

## 2026-11-03 — T101 yapım bitti

2026-11-03 — T101 (Veri modeli temeli) yapım bitti, PR #358, doğrulama bekliyor.

---

## 2026-11-03 — F0 faz kapısı PASS

2026-11-03 — F0 (Proje iskeleti) faz kapısı: ✓ PASS (`GATE_CHECK_F0.md`; ana dal `93ba91f`; testler, üç imaj, yerel referans kurulumda taze ortam ve canlılık 200, Lint kırmızı→yeşil; `11 §7.1`'in F0'a bağlı 66 satırı dolduruldu; backlog hijyeni: D-07, D-11, D-18, D-23 ✅, yeni D-43, D-44). Sırada: gate PR'ının merge'ü ve `phase/F0-pass` etiketi (yardımcı), sonra F1. Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T09 doğrulama tur 2 PASS).

---

## 2026-11-03 — T09 doğrulama tur 2 PASS

2026-11-03 — T09 (İlk iniş provası: yerel referans kurulumda canlılık) bağımsız doğrulama tur 2: ✓ PASS (`303c074`; kapsam tam; kriter 1–7 ve KK1–KK7 ✓ — kurulum validator tarafından reçeteyle sıfırdan kuruldu; dal CI run 41692602124 yeşil; tur 1 bulgu 1 ve K1–K5 kapandı; K-notları D-42). Sırada: merge (yardımcı), sonra F0 gate. Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T09 doğrulama tur 1 FAIL).

---

## 2026-11-03 — T09 doğrulama tur 1 FAIL

2026-11-03 — T09 (İlk iniş provası: yerel referans kurulumda canlılık) bağımsız doğrulama tur 1: ✗ FAIL (`a4632b1`; kapsam tam; kriter 1–7 ve KK1–KK7 ✓ — kurulum validator tarafından reçeteyle sıfırdan kuruldu; dal CI run 41687547294 yeşil; bulgu 1 S1 DÜŞÜK: `E2E_CMD=NOT_APPLICABLE` gerekçesi "uçtan uca faz yok" `11` F6 (Uçtan uca doğrulama) ve `05 §8.3.11` ile çelişir, bağlama izlenmiyor; K1–K5 düzeltme turunda). Sırada: T09 düzeltme 1. Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T09 yapım bitti).

---

## 2026-11-03 — T09 yapım bitti

2026-11-03 — T09 (İlk iniş provası: yerel referans kurulumda canlılık) yapım bitti, ⏳ doğrulama bekliyor: yerel referans kurulum (dind sunucu, MinIO, Caddy iç sertifikası, sahte sırlar) iki kez sıfırdan koştu; canlılık 200, sır ve kullanıcı doğrulamaları, `DOCKER-USER` çıkış denemesi, IPv4 ve IPv6 istemci adresi, yedek ve kurtarma provası, imaj kimlikleri `DEPLOY_RUNBOOK` §K'ya yazıldı; `SETUP_COMPLETE=true`, D-01 ✅, gerçek sunucu artıkları D-40/T603. Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T09 plan düzeltmesi).

---

## 2026-11-03 — T09 plan düzeltmesi

2026-11-03 — T09 plan düzeltmesi (PLAN_CORRECTION_REQUIRED; PR #353; proje sahibinin kararı): T09 "İlk iniş provası: yerel referans kurulumda canlılık" oldu (Docker içinde ayrıcalıklı Linux konteyneri, MinIO, sahte sırlar) ve ⬚ Bekliyor'a döndü; gerçek sunucuya ilk iniş yeni F6 task'ı T603'e taşındı (⬚); D-01'in gerçek sunucu artıkları `DEFERRED_BACKLOG.md` D-40'ta. Toplam 215 task. T01–T08 ve T10 ✓. Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T09 BLOCKED OWNER_INPUT).

---

## 2026-11-03 — T09 BLOCKED (OWNER_INPUT)

2026-11-03 — T09 (İlk yayın ve referans kuruluma ilk iniş) ⛔ BLOCKED (OWNER_INPUT): kabul kriteri 3–8 referans sunucuyu (Linux, iptables, IPv4 + IPv6), firmanın nesne depolama kovası ve anahtarlarını, alan adını ve kurulum sırlarını ister; bunlar proje sahibinden beklenir. T09 bu girdiler gelince yapılır; F0 gate'i T09'dan sonra (`00`: gate PASS olmadan sonraki faza geçilmez). T01–T08 ve T10 ✓.

---

## 2026-11-03 — T07 doğrulama tur 4 PASS

2026-11-03 — T07 (Yedek düzeni) bağımsız doğrulama tur 4 (düzeltme 2 sonrası, `main` ile birleşik hâl): ✓ PASS (`aa1cfe6`; kapsam tam; 7/7 kabul kriteri, KK1–KK7 ✓; dal CI run 41670381320 yeşil; tur 3'ün bulgusu (`VAL-F016`, prova son anı) kapandı — yerel Docker'da gerçek imajla prova kurtarmanın bitmesini bekledi, `last-moment` birincil sunucudaki son işlemle aynı, günlükler `--repo=2` ile dış depodan; bulgu 0; K-notları: `core.hooksPath` yine mutlak yol (düzeltildi), D-39 (d) kapandı). Merge yardımcıda (PR #351). Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T07 doğrulama tur 3 FAIL).

---

## 2026-11-03 — T07 doğrulama tur 3 FAIL

2026-11-03 — T07 (Yedek düzeni) bağımsız doğrulama tur 3 (tur 2 PASS'ten sonra `main` ile birleşik hâl): ✗ FAIL (`c929163`; kapsam tam; kriter 1–6 ✓, 7 ✗; dal CI run 41665382289 yeşil; bulgu 1 S1 ORTA `VAL-F016`: `database-restore-drill.sh` sıcak beklemede açılan kopyada son anı günlük uygulaması bitmeden okur — kayıt son tam yedeğin anını ve yalnız temel yedeğin indirme süresini taşır; K-notları: `core.hooksPath` mutlak yol (düzeltildi), PR açıklamasındaki test sayıları; iki doğrulayıcıyla, ilki bölündü). Düzeltme turu gerekli; merge yok (PR #351). Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T07 doğrulama tur 2 PASS).

---

## 2026-11-03 — T07 doğrulama tur 2 PASS

2026-11-03 — T07 (Yedek düzeni) bağımsız doğrulama tur 2: ✓ PASS (`501c24b`; kapsam tam; 7/7 kabul kriteri, KK1–KK7; dal CI run 41660924636 yeşil; yerel Docker'da gerçek iki imajla dosya yedeğinin dört adımı `success`, prova üç kayıt alanını yazdı; tur 1'in dört bulgusu ve K1–K7 kapandı; bulgu 0; K-notu D-39 (f); iki doğrulayıcıyla, ilki bölündü); merge yardımcıda (PR #351). F0 sırada sonraki: T10. Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T07 yapım bitti).

---

## 2026-11-03 — T10 doğrulama tur 2 PASS

2026-11-03 — T10 (Veritabanı kullanıcıları) bağımsız doğrulama tur 2: ✓ PASS (`76979dc`; kapsam tam; 8/8 kabul kriteri, KK1–KK8; dal CI run 41661056056 yeşil; gerçek veritabanı imajında `log_statement=all` ile ilk kurulum — günlükte açık parola yok, yalnız üç SCRAM özeti; tur 1'in bulgu 1'i kapandı; bulgu 0; K-notları D-38); merge yardımcıda (PR #350). F0'da kalan: T07, ardından T09 (T09 T07'ye bağlı). Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T10 yapım bitti).

---

## 2026-11-03 — T07 yapım bitti, doğrulama bekliyor

2026-11-03 — T07 (Yedek düzeni) yapım bitti, PR açıldı, bağımsız doğrulama bekliyor: gecelik tam yedek (pgBackRest, iki depo) ve dosya yedeği (restic, dört adım) betikleri, `archive_timeout` 50 s, kova kuralı ve anahtar politikası, kurtarma provası runbook adımı (`DEPLOY_RUNBOOK` §J); betik testleri ve 15 eşitlik denetimi yerelde yeşil, imaj çalıştırılarak sınandı; D-28, D-30 (b), D-34 (a) kapandı, D-39 açıldı. Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T08 doğrulama tur 4 PASS).

---

## 2026-11-03 — T10 yapım bitti

2026-11-03 — T10 (Veritabanı kullanıcıları) yapım bitti, doğrulama bekliyor: kurucu betik (`deploy/images/database/initdb/10-bootstrap.sh`; SQL'ler ve `pg_hba`/`pg_ident` şablonları `bootstrap/`'ta) dört kullanıcıyı, veritabanını (UTF-8, ICU tr-TR), yetkileri, `pg_trgm`'i ve üç rol ayarını kurar; yedek durum kullanıcısı `peer` + `pg_ident`, kurucu ağdan `reject`, `trust` yok; yetki matrisi ve kullanıcı testleri gerçek PostgreSQL 18'de betiğin SQL'leriyle koşar; CI Paketleme adımı ilk kurulumu sınar; D-01 ve D-31'e not. Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T08 doğrulama tur 4 PASS).

---

## 2026-11-03 — T08 doğrulama tur 4 PASS

2026-11-03 — T08 (Sır düzeni) bağımsız doğrulama tur 4: ✓ PASS (`363dd4d`; kapsam tam; 6/6 kabul kriteri, KK1–KK6; dal CI run 41646506249 yeşil; yerel Docker'da 15 sır dosyası × 3 kullanıcı ve dalın yayını uygulama imajında — dosya ortamı yener, geliştirme yolu ortamdan, ikisi yoksa açılmaz; tur 3'ün iki bulgusu ve K1–K5 kapandı; bulgu 0; K-notları D-36); merge yardımcıda (PR #349). F0 sırada sonraki: T07. Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T05 doğrulama tur 2 PASS).

---

## 2026-11-03 — T05 doğrulama tur 2 PASS

2026-11-03 — T05 (Compose geliştirme ortamı) bağımsız doğrulama tur 2: ✓ PASS (9/9 kabul kriteri, KK1–KK9; dal CI run 41627237288 yeşil; yerel Docker'da üç imaj dal Dockerfile'larından derlendi — yalıtım, HTTP→HTTPS + bir yıllık HSTS, Mailpit, `DOCKER-USER` kuralı atılık konteynerde sınandı; plan düzeltmesi `11`/`12`/D-31'e tutarlı; bulgu 0; K-notları D-34; iki doğrulayıcıyla, ilki bölündü); merge yardımcıda (PR #348). F0 sırada sonraki: T08. Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T05 yapım bitti).

---

## 2026-11-03 — T05 yapım bitti, doğrulama bekliyor

2026-11-03 — T05 (Compose geliştirme ortamı) yapım bitti, PR #348 açık, doğrulama bekliyor (⏳); `deploy/` altında Compose, geliştirme bindirmesi, Caddyfile, sürüm dosyası ve güvenlik duvarı betikleri; yerelde Docker kapalıydı, `docker compose config` geçerli, daemon gereken sınamalar (KK2, KK4, KK8, KK9) D-31 ile T09'a bağlandı; D-01 K-987 doğrulamaları yazıldı (`SETUP_COMPLETE` false), D-29 (c) kapandı, D-31..D-33 açıldı; önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T06 doğrulama tur 2 PASS).

---

## 2026-11-03 — T06 doğrulama tur 2 PASS

2026-11-03 — T06 (Üç imaj ve kayıt defteri) bağımsız doğrulama tur 2: ✓ PASS (8/8 kabul kriteri, KK1–KK8; dal CI run 41594485717 yeşil — Paketleme 7/7: üç imaj, yanlış restic özeti `did NOT match` ile düşer, kullanıcılar kök değil, canlılık 200, `track_commit_timestamp` on; taban imaj özetleri ve restic özeti/lisansı kayıt defterlerinden doğrulandı; yerelde Docker kapalıydı; K-notları D-29, D-30); merge yardımcıda (PR #347). F0 sırada sonraki: T05.

---

## 2026-11-03 — T06 yapım bitti, doğrulama bekliyor

2026-11-03 — T06 (Üç imaj ve kayıt defteri) yapım bitti, PR #347 açık, doğrulama bekliyor (⏳); D-01 kalemleri (PACKAGE_BUILD/PUBLISH, doğrulanacaklar) ve D-19 kararı raporda; önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T01 doğrulama tur 4 PASS).

---

## 2026-11-03 — T01 doğrulama tur 4 PASS

2026-11-03 — T01 (Canlılık ucu) bağımsız doğrulama tur 4: ✓ PASS (6/6 kabul kriteri; 62/62 test — Lint 9, Unit 24, Integration 29 PostgreSQL 18.6'da; Kestrel'de dondurulmuş veritabanında 503 ayarın süresinde; dal CI yeşil; K-notları D-25, D-26); merge yardımcıda (PR #338). F0 sırada sonraki: T06.

---

## 2026-11-03 — T01 yapım bitti, doğrulama bekliyor

**Son güncelleme:** 2026-11-03 — T01 (Canlılık ucu) yapım bitti, PR açık, doğrulama bekliyor (⏳); önceki: T03 (Test altyapısı) bağımsız doğrulama tur 3: ✓ PASS (9/9 kabul kriteri; 36/36 test PostgreSQL 18.6'da, kilit dosyaları koşu sonrası değişmiyor; K-notu D-17'ye eklendi); merge yardımcıda (PR #337). F0 sırada sonraki: T01.

---

## 2026-11-03 — T04 doğrulama tur 2 PASS

T04 (CI hattı ve eşitlik denetimleri) bağımsız doğrulama tur 2: ✓ PASS (9/9 kabul kriteri; dal CI'ı run 41548360499 yeşil — Lint 9, Unit 15, Integration 14 PostgreSQL 18.6 hizmetinde, dry-run atlama satırları; sondajlar geçici kopyada; tur 1'in iki bulgusu kapandı; K-notları D-23, D-24); merge yardımcıda (PR #339).

---

## 2026-11-03 — T03 doğrulama tur 3 PASS

**Son güncelleme:** 2026-11-03 — T03 (Test altyapısı) bağımsız doğrulama tur 3: ✓ PASS (9/9 kabul kriteri; 36/36 test PostgreSQL 18.6'da, kilit dosyaları koşu sonrası değişmiyor; K-notu D-17'ye eklendi); merge yardımcıda (PR #337). F0 sırada sonraki: T01.

---

## 2026-11-03 — T03 yapım bitti, doğrulama bekliyor

2026-11-03 — T03 (Test altyapısı) yapım bitti, ⏳ doğrulama bekliyor (PR #337). T02 ✓ (PR #335).

---

## 2026-11-02 — Implementation dönemi açıldı

Implementation dönemi açıldı: tracker `11`'in faz ve task listesiyle dolduruldu (7 faz, 214 task, hepsi ⬚); sıradaki iş `/task T02` (önce D-01 kapısı, `Docs/PRODUCT_DISCOVERY_STATUS.md` §25).

---
