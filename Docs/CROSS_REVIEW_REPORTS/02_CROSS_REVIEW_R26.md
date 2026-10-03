# Cross-Review — 02 Product Requirements (Tur 26)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/02_PRODUCT_REQUIREMENTS.md` v0.41 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki turlar ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde). **İstem bu turda değişti (K-615):** yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi aynı kaldı. Önceki turların *"Yalnız gerçek ve önemli sorunları yaz; üslup tercihlerini bulgu yapma."* cümlesinin yerine proje sahibinin kararıyla şu paragraf girdi: *"Yalnız CİDDİ sorunları yaz: (1) yürürlükteki mevzuata aykırılık, (2) müşteriye ya da firmaya para kaybı veya hak kaybı doğuran kural boşluğu, (3) kullanıcının takılıp kaldığı, çıkışı olmayan bir akış, (4) dokümanın iki yerinin birbiriyle çelişmesi. Nadir kenar durumları, iyileştirme önerilerini, savunmada derinlik önerilerini, üslup ve ifade tercihlerini bulgu yapma. Böyle bir sorun yoksa SONUÇ: TEMİZ yaz."*

> ```text
> SONUÇ: TEMİZ
> ```

Model sonuca varmadan önce iki hukuki konuyu web aramasıyla kontrol etti (koşum kaydında görünür, çıktıya girmedi): Mesafeli Sözleşmeler Yönetmeliği m.12'nin iade taşıyıcısı ve geri ödeme hükmü, ve e-Arşiv faturada T.C. kimlik numarası olmayan nihai tüketici için kullanılan değer. İkisi için de bulgu yazmadı.

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| SONUÇ: TEMİZ | ✅ KABUL | Çıkış koşulu K-615'in ölçüsüyle sağlandı. Modelin kontrol ettiği iki konu dokümanda zaten karara bağlı: iade kargo bedeli ve taşıyıcı §7.4.2'de yazılı (K-210), geri ödemenin başlangıcı §7.4.1'de K-491 ile Yönetmelik'in güncel metnine göre kuruldu (A-14). T.C. kimlik numarası konusu 22. ve 23. turlarda reddedildi ve §3.14.6'da açıkça yazıldı. Model iki konuda da metni yeterli buldu. TEMİZ, istemin daraltılmasından sonraki ilk turda geldi. Bu, 25. turun bulgularının (K-613, K-614) kenar durum düzeyinde olduğu tespitiyle tutarlıdır. | Yok. |

**Dağılım:** Bulgu yok.

## 3. Ek bulgular

- **Mekanik tarama:** `02`'de anılan bütün karar numaraları (en yükseği K-614) karar kaydında satır olarak var. `ÖK-2…ÖK-4` gibi ön koşul kodları dışında boşta kalan atıf yok. Sürüm başlığı ve alt bilgi aynı sürümü gösteriyor.
- **Ciddi bir sorun görülmedi.** Bu turda ikinci modelin listesinin dışında ciddi bir sorun saptanmadı.
- **Önceki turlardan açık kalanlar:** 21. turun "üyenin e-postasını değiştirmesinde teyidin hangi adrese yapılacağı" notu ve 20. turun K-606 hizmet ve dijital kalem notu `04`'ün işidir. 25. turun etki yansıtma notları (`08`'de bağlantı hatasının ayrımı — K-614; `04`'te açık ödenmemiş siparişlerin panel görünümü — K-613; `12`'nin aydınlatma taslağında gecikme feshindeki IBAN) etki yansıtma adımına devredilir.
- **Etki yansıtma (Faz 5) turdan sonra ayrı bir adımda yapıldı; sonucu §5'tedir.** Görev tanımına göre ayrı bir adımda, 1–26. turların bütün değişiklikleri için topluca yapılır: `01` ve `10` downstream ve upstream taraması, K-499…K-615 arasında eklenen alan, enum, parametre ve kuralların ilgili dokümanlarda karşılığı.

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı (`02` v0.42). Metinde değişiklik yok, yalnız sürüm notu eklendi. Yeni karar satırı açılmadı. Bu turdan önce proje sahibinin kararı K-615 olarak kaydedildi: istem ciddi sorunlara daraltıldı ve çıkış koşulu bu ölçüde TEMİZ sayılır. K-615, `cross-review` skill'inin çıkış koşulundan bilinçli bir sapmadır ve §6.1'de öğrenim adayı (6) olarak listelendi.

- [x] SONUÇ: TEMİZ (kabul — K-615'in ölçüsüyle çıkış koşulu sağlandı)
- [x] Etki yansıtma (Faz 5) — ayrı adımda yapıldı (§5; `02` v0.43, `01` v0.30, `10` v0.10)

**Hukuki kontrol** (2026-10-03): Bulgu yok, yeni hukuki iddia yok. Modelin kontrol ettiği iki konu, Mesafeli Sözleşmeler Yönetmeliği m.12 (§7.4.1–§7.4.2; K-210, K-491, A-14) ve e-Arşiv'de T.C. kimlik numarası alanı (§3.14.6, 22. ve 23. turlar), önceki turlarda resmî metne karşı doğrulanmıştı. Bu turda değişmedi.

## 5. Etki yansıtma sonucu (Faz 5)

TEMİZ'den sonra cross-review'ın 26 turunda yapılan değişiklikler — kararlar K-561…K-615 ve turlarda yapılan, karar gerektirmeyen düzeltmeler (`git diff b851a47..a54e913 -- Docs/02_PRODUCT_REQUIREMENTS.md`) — diğer dokümanlara ve karar kaydına karşı üç taramayla kontrol edildi: MVP Kapsamı'na karşı, Proje Vizyonu'na karşı, Ürün Gereksinimleri'nin kendi içi ve karar kaydı. 85 uyumsuzluk bulundu; hepsi hedefli düzeltmeyle kapandı. Etki yansıtmada iki karar alındı (K-616, K-617).

**Downstream — MVP Kapsamı (`10` v0.10):**
- **Sayımlar:** KP-48'in müdahaleleri dokuz oldu — misafir siparişinin e-postasının düzeltilmesi (K-585, K-586) · KP-66'nın olayları on beş oldu — eski adrese giden değişiklik bildirimi (B-15) · KP-64'ün parametreleri kırk altı oldu — P-47 (K-599) ve P-48 (K-603).
- **Yeni yetenekler satırlara girdi:** e-posta değişikliğinin eski adresten geri alınması (KP-27, KP-62 — K-599) · hesabın adı (KP-25, KP-27, KP-77 — K-605) · ayrı hesap türleri ve panelde Google girişinin olmaması (KP-62 — K-588) · siparişte eşzamanlı işlem (KP-76 — K-587) · onay kutusu kaydının donması (KP-17, KP-75 — K-595) · yerli üretim logosu (KP-5 — K-610) · isteğe bağlı ETBİS doğrulama bilgisi (KP-35, KP-58, ÖK-5 — K-574) · tanınan tarayıcı ve limitlerin eksenleri (KP-72 — K-579, K-602, K-603, K-613, K-614).
- **Değişen kurallar:** havale hattında IBAN'ın müşteriden istendiği bütün hâller ve düzeltilmesi (KP-22, KP-47, KP-66 — K-569, K-575, K-581) · başka kanaldan gelen caymanın teyidi, pencere dışı kaydı, teslim günü sırası ve kargodan önce iptalle işlenmesi (KP-47 — K-604, K-606, K-607, K-608) · gecikme feshinin içeriği (KP-21 — K-571, K-592, K-597) · fiziksel kalemin kargodan sonra çıkarılamaması ve adres düzeltmesinin teyidi (KP-48 — K-577, K-601) · sürüm artışının siparişi durdurması (KP-14 — K-591) · 0 TL'lik siparişin kapanması (KP-11, KP-39, KP-43 — K-565, K-600) · silinen adresin yeniden kullanımı (KP-7 — K-568) · adresin alanları (KP-12 — K-561) · alternatif metnin geri düşüşü (KP-42 — K-564) · Teslim edilemedi hattında kapanış (KP-46 — K-590, K-592) · satış kapısının yasal metin koşulu (KP-38, ÖK-10 — K-576) · gecikme feshinin ve geç ödemenin satış özetindeki yeri (KP-15, KP-53 — K-580, K-589) · Hakkımızda'nın kapısı (KP-54 — K-584) · hizmetin cayma penceresi ve kısmi ifa (KP-22 — K-578, K-593).
- **§3'e KD-62 girdi:** ters ibraz ürüne girmez (K-611, K-612 — Açık). Kapsam dışı satırları altmış iki, "Açık" olanlar kırk.
- **Karar gerektirmeyen iki düzeltme yansıdı:** KP-18'in gerekçesi 8. turda daraltılan `02 §9.1.2`'ye — yalnız sipariş ve talep akışları e-postaya bağlı değildir —, SK-7'nin gerekçesi 17. ve 20. turlarda düzeltilen `02 §4.1.5`'e — çit bir güvence değildir.
- **MVP Kapsamı audit'iyle sınır:** audit'in konusu olan yerlerde yalnız cross-review'ın değiştirdiği içerik yazıldı. KP-21'deki gecikme feshi cümlesi içeriğiyle düzeltildi ama hâlâ "Neden MVP'de" sütunundadır; Özellik sütununa taşınması audit'in işidir. KP-48'e tutar bazlı kısmi geri ödeme audit'le girer (K-539); audit bu cümleyi eklerken K-596'nın kullanımını da yazar: firma "mal dönmedi" kapatmasından sonra geri ödemeye karar verirse kaleme bağlı tutar bazlı geri ödeme kullanır, mal sonradan ulaşırsa ödenmiş kısım düşülür. Caymadaki kart iadesini kimin başlattığı (KP-47), ÖK-5'in kayıt yükümlülüğü, SK-7'nin K-549 tetikleyicisi, B-9'daki gecikme feshi ve §1'in kayıp listesi audit'te kalır.

**Upstream — Proje Vizyonu (`01` v0.30):** M-4 gecikme feshini de sayar: ölçü iptal, gecikme feshi ve cayma kayıtlarından hesaplanır ve feshedilen kalem fesih bildiriminin tarih damgasında iptal edilmiş kalem gibi sayılır (K-580). İki doküman aynı ölçüyü farklı tanımlıyordu. Ü-1'in yasal metin koşulu `02 §3.1.5`'in tanımıyla yazıldı (K-576). Çelişki bulunmayan kararlar: K-585 (Ü-3'ün bütçesi değişmez — yeni müdahale zorunlu adım değildir), K-603 (S-9'un gerekçesi geçerli — yeni işaret girişten sonra bırakılan zorunlu bir birinci taraf çerezidir, ölçüm için kullanılmaz), K-582, K-583, K-588, K-589, K-591, K-594, K-598, K-599, K-600, K-611, K-612, K-615. `01`'in Ürün Gereksinimleri'ne verdiği bölüm atıflarının hepsi doğru yere gidiyor.

**Ürün Gereksinimleri'nin kendi içi (`02` v0.43):**
- **Sözlük gövdenin gerisinde kalmıştı:** İptal edildi (K-592 — gecikme feshinde sipariş kargoya verilmiş olabildiği için tanım gövdeyle çelişiyordu), Sosyal giriş ve Firma yöneticisi (K-588), Geri ödeme (K-569, K-611, K-612), Cayma (K-604, K-606) ve Bildirim (F-5) hizalandı.
- **Beş yeni terim (K-617):** cross-review'da doğan kavramlar sözlüğe girdi — Erişim anahtarı, E-posta geçmişi, Hesap türü, Tanınan tarayıcı işareti, Ters ibraz. Sözlük 95 → 100 terim. §8.2.6'daki "tanınma işareti" sözlüğün adıyla yazıldı.
- **Kaynak satırları:** audit'ten sonra her bölümün Kaynak satırı gövdede anılan kararların tamamını taşıyordu; cross-review bu kuralı on iki bölümde bozmuştu. §1–§12'nin Kaynak satırları tamamlandı — §6'da yalnız K-495 ve sonrası, çünkü tablo satırları kendi Kaynak sütununu taşır. On üç yerde satır içi atıf eklendi.
- **Güncel sayımlar:** sözlük 100 terim · Z-1…Z-46 · P-1…P-48 (kırk altı parametre) · L-1…L-8 · dokuz müdahale · B-1…B-15 ve F-1…F-5.

**Upstream — karar kaydı:**
- Kırk altı cross-review kararının etki sütunundaki `10` ve `01` işaretleri satır ve sürümle kapandı: "etki yansıtmada" yerine "taslak güncellendi — v0.10" ya da "— v0.30" yazıldı.
- Etki sütunu gövdede değişen yeri göstermeyen beş karar düzeltildi: K-571, K-584, K-589, K-603, K-608.
- Cross-review kararlarının değiştirdiği ya da daralttığı otuz yedi eski karara geri işaret kondu. Örnekler: K-520 → K-608, K-511 → K-610, K-524 → K-579, K-495 → K-580, K-431 → K-615.
- K-585'in etki sütunundaki bölümsüz `01` işareti "dokunmaz — kontrol edildi" diye kapandı.

**Etki yansıtmada alınan kararlar:**
- **K-616 (öneriyle — ⚠):** havale IBAN'ı panelde girildiğinde ya da değiştiğinde bütün yöneticilere kendi adreslerinden bildirim gider. Bu, K-523'ün açık bıraktığı parçadır ve ele geçirilmiş hesaba karşıdır. F- matrisine F-5 girdi (`02 §9.3`, yeni §9.3.3; `10` KP-67). Proje sahibinin gözden geçirme listesine girer.
- **K-617 (öneriyle):** sözlüğe beş terim.

**Yeni alan ve kural taraması — devirler:** cross-review'da eklenen alan ve kuralların `02` ve `10` dışındaki karşılıkları etki sütunlarındadır.
- `04`: sipariş sayfasının IBAN alanı, iki giriş ekranı, kayıt formunda ad, "geri ödeme gerçekleşmedi" ve "stokta bulunamadı" uyarıları, cayma kaydında teyit hatırlatması, açık ödenmemiş siparişlerin panel görünümü (K-613), F-5'in metni.
- `06`: hesap türü, erişim anahtarı, e-posta geçmişi, onay kutusu kaydı, kalemin donan nitelikleri, beş yeni sözlük adı.
- `05`: eşzamanlılık mekanizması, tanınan tarayıcı işaretinin ve iki sayacın mekanizması.
- `08`: B-15, F-5, bağlantı hatasının ayrımı (K-614), yerli üretim logosu.
- `12`: sözleşmede kişiye özel mal için otuz günlük taahhüt, aydınlatma taslağında hesap verisi ve gecikme feshindeki IBAN.

Bu dokümanlar henüz yazılmadığı için devirler etki sütunlarında kalır.

**Açık kalan:** yok. `02`'nin kalite döngüsü tamamlandı; sırada `10`'un kalite döngüsü. Cross-review'dan gelmeyen iki eski sıra bozukluğu düzeltilmedi ve checkpoint'e not edildi: 6.8.11 6.8.1'den hemen sonra, 10.7.4 10.7.1'den hemen sonra geliyor.
