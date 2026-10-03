# Cross-Review — 10 MVP Scope (Tur 16)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/10_MVP_SCOPE.md` v0.26 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki turlar ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde). İstem önceki turlarla aynıdır: daraltılmış talimat (K-615, K-644), yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi.

> ```text
> SONUÇ: TEMİZ
> ```

Model sonuca varmadan önce beş hukuki konuyu web aramasıyla kontrol etti (koşum kaydında görünür, çıktıya girmedi): Mesafeli Sözleşmeler Yönetmeliği m.12'nin iade taşıyıcısı ve 1/1/2026'dan beri uygulanan metni, Elektronik Ticarette Hizmet Sağlayıcı ve Aracı Hizmet Sağlayıcılar Hakkında Yönetmelik m.5'in iletişim bilgileri (KEP, MERSİS), ETBİS kayıt yükümlülüğü, 2025/10 sayılı Cumhurbaşkanlığı Genelgesi'nin erişilebilirlik kontrol listesi ve Fiyat Etiketi Yönetmeliği'nde birim fiyat. Hiçbiri için bulgu yazmadı.

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| SONUÇ: TEMİZ | ✅ KABUL | Çıkış koşulu K-644'ün ölçüsüyle sağlandı: K-615'in daraltılmış talimatıyla model ciddi bir sorun bulmadı. Modelin kontrol ettiği beş konu dokümanda zaten karara bağlı. Geri ödemenin başlangıcı KP-22'de (K-491); 5.–15. turlarda dokuz kez geldi ve her seferinde Yönetmelik'in güncel metnine karşı reddedildi. Yasal kimlik ve iletişim bilgileri KP-35 ve KP-58'de (K-342, K-350, K-626, K-627). ETBİS kaydı ÖK-5'te (K-574, K-628). Erişilebilirlik KP-71'de (K-395, K-396, K-625). Birim fiyat ve ölçü birimi KP-39'da (K-573); beş kez geldi ve reddedildi. Model bu turda beşinde de metni yeterli buldu. Önceki iki turun (14. ve 15.) bulguları yalnız yeniden açılan K-491'di; 14. turdaki KISMİ de bir kural değişikliği değil, bir sözcüğün netleştirilmesiydi. TEMİZ bu gidişle tutarlıdır. | Yok. |

**Dağılım:** Bulgu yok.

## 3. Ek bulgular

- **Mekanik tarama:** sürüm başlığı, sürüm notu ve alt bilgi aynı sürümü gösteriyor (v0.27). Kapsam satırı sayısı değişmedi (77), kapsam dışı satırı sayısı değişmedi (63). `10`'da anılan bütün karar numaraları (en yükseği K-644) karar kaydında satır olarak var.
- **Ciddi bir sorun görülmedi.** Bu turda ikinci modelin listesinin dışında ciddi bir sorun saptanmadı.
- **Tekrarlayan konular — kapanış görünümü:** KP-22'nin geri ödeme başlangıcı (K-491) dokuz, KP-39'un ölçü birimi alanı (K-573) beş, KD-28'de satış engelinin olmaması (K-84, K-641) üç, KP-47'nin "stokta bulunamadı" sebebi (K-609) üç turda geldi; hepsi reddedildi ve kural değişmedi. Bu turda hiçbiri gelmedi; model KP-22 ve KP-39'u kendiliğinden kontrol edip metni yeterli buldu.
- **Önceki turlardan devreden notlar** yerinde duruyor ve etki yansıtma adımına devredilir: "İşlem izi" sözlük satırı, §8.5.2, §3.34.8 ve §12.1.12'ye 4733 m.8/5-k dayanağı (isteğe bağlı); `02 §3.1.4`'teki "kayıt yükümlülüğü her firmada doğmaz" gerekçesi (8. tur).
- **Etki yansıtma (Faz 5) ayrı bir adımda yapılacak:** görev tanımına göre 1–16. turların bütün değişiklikleri için topluca — `01` ve `02` upstream taraması, cross-review'da açılan kararların etki sütunları.

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı (`10` v0.27). Metinde değişiklik yok, yalnız sürüm notu eklendi ve alt bilgi "cross-review ✓ — 16. turda TEMİZ, sırada etki yansıtma" oldu. Yeni karar satırı açılmadı.

- [x] SONUÇ: TEMİZ (kabul — K-644'ün ölçüsüyle çıkış koşulu sağlandı)
- [x] Etki yansıtma (Faz 5) — ayrı adımda yapıldı; sonuç §5'te (`10` v0.28, `02` v0.45)

**Hukuki kontrol** (2026-10-03): Bulgu yok, yeni hukuki iddia yok. Modelin kontrol ettiği konular önceki turlarda resmî metne karşı doğrulanmıştı: Mesafeli Sözleşmeler Yönetmeliği m.12 (15. turda mevzuat.gov.tr konsolide metni ve RG 24/5/2025-32909 yeniden okundu), Fiyat Etiketi Yönetmeliği ve Bakanlık rehberi (13. tur). Aynı gün içinde yürürlükteki metinlerde değişiklik beklenmez; bu turda yeniden açılmadı.

## 5. Etki yansıtma sonucu (Faz 5)

Cross-review'ın on altı turunda yapılan değişiklikler (`git diff fd1c208..d6c9072 -- Docs/10_MVP_SCOPE.md` — KP-5, KP-22, KP-39, KP-47, KP-63, KP-75, KD-28, ÖK-5, ÖK-10 ve SK-3'teki düzeltmeler) ve `10`'un kalite döngüsünün kararları (K-618…K-644) dört taramayla kontrol edildi: Ürün Gereksinimleri'ne karşı, Proje Vizyonu'na karşı, MVP Kapsamı'nın kendi içi ve karar kaydı. Turlardan devreden dört not kapandı. Tarama bunlara ek olarak `10`'da bir yerde, `02`'de üç bölümün Kaynak satırında ve karar kaydında otuz üç satırda uyumsuzluk buldu; hepsi hedefli düzeltmeyle kapandı. Yeni karar alınmadı.

**Downstream:** `10`'u bağımlılık olarak listeleyen dokümanlar (`03`–`12`) henüz yazılmadı. Cross-review'ın düzeltmeleri kural değiştirmedi, kayıtlı kararların satırda eksik kalan parçasını yazdı. `04`'e ("stokta bulunamadı" uyarısının iki yarısı — K-609; teyit hatırlatması — K-607) ve `12`'ye (KP-22'nin kalan riski, KP-63'ün on yıllık süresi) giden işler kararların etki sütunlarında durur.

**Upstream — Ürün Gereksinimleri (`02` v0.45):**
- **Devreden dört not kapandı:**
  - **İşlem izinin süresi:** sözlükteki "İşlem izi" satırı ve §8.5.2 izi süre belirtmeden "değiştirilemez ve silinemez" diye yazıyordu; §10.3.2 de öyle. `02`'de çelişki yoktu, çünkü Z-30 ve §10.3.5 süreyi taşıyor. Ama KP-63'ün 1. turda düzeltilen okuması burada da mümkündü. Üç yer artık on yıllık saklama süresini ve süre sonundaki imhayı Z-30'a işaret ederek yazar (K-355).
  - **Sıfır kayıp listesi:** §3.34.8 işlem izini süre belirtmeden "kaybolamaz" diye sayıyordu. KP-75'in 2. turdaki okumasıyla hizalandı: liste saklama süreleri içindir, süre sonundaki imha bir kayıp değildir (§12.2.7; K-354).
  - **Alkol ve tütün:** §12.1.12 tüketiciye internetten satışın yasak olduğunu ve dayanağını yazar (4733 sayılı Kanun m.8/5-k; 6. turda mevzuat.gov.tr metnine karşı doğrulandı). Mevzuata uygunluğun satıcı olan firmada olduğu ve ürünün ticari zincirde yer almadığı KD-28'in diliyle yazıldı (K-12, K-84).
  - **ETBİS gerekçesi:** §3.1.4'teki *"kayıt yükümlülüğü her firmada doğmaz"* gerekçesi K-628'den sonra doğru değildi. Alanın satış kapısını tutmaması artık yalnız ürünün kaydı denetleyememesine dayanır; kaydın satış yapacak her firmanın yükümlülüğü olduğu yazılır (K-628). Sonuç değişmedi.
- **Öteki düzeltmeler `02` ile zaten hizalı:** KP-5 ve KP-39 (§3.8.5 — K-573) · KP-47 (§7.2.4, §6.7.6 — K-609; §10.4.10, §8.3.8 — K-607) · KP-22 (§7.4.1, §7.4.2, §7.4.4, §3.24.3, §12.1.9 — K-491, K-293, K-493) · ÖK-5 (§3.1.4 — K-574) · ÖK-10 (§3.33.2 — K-553, K-576) · SK-3 (§10.5.1, §10.5.4 — K-454) · KP-63'ün "havale IBAN'ı" (§10.3.1, §9.3.3). Turlarda gerekçeye giren elenen seçenekler kayıtta var: ayrı bir satış birimi seçimi K-573'te, "teyit bekliyor" durumu K-607'de, sebebin listeden çıkarılması K-609'da.
- **Kaynak satırları:** `10`'un audit'inin etki yansıtmasında (`02` v0.44) üç bölümün Kaynak satırı gövdede anılan altı kararı taşımıyordu: §3 K-625, K-626, K-627 · §10 K-627, K-628, K-643 · §12 K-626. Tamamlandı. Bu adımda gövdeye giren K-354 (§3), K-628 (§3) ve K-355 (§1) da eklendi. Mekanik tarama §1–§12'de eksik bırakmıyor; §6'da kural gereği yalnız K-495 ve sonrası aranır.

**Upstream — Proje Vizyonu (`01` v0.31, dokunulmadı):** §3.2'nin işlem izi cümlesi yalnız "değiştirilemez" der ve imhayla çelişmez. Vizyon dokümanına kural ayrıntısı taşınmaz (öğrenim adayı (1)); süre `02`'de kalır. §7'nin K-84 cümlesi KD-28 ile, Ü-1'in ETBİS ön koşulu ÖK-5 ile (K-628), Ü-3'ün hat başına bütçesi SK-3 ile (K-454) uyumlu. `01`'in MVP Kapsamı'na verdiği atıflar doğru yere gidiyor: cross-review satır eklemedi, kapsam satırları 77'de, kapsam dışı satırları 63'te kaldı.

**MVP Kapsamı'nın kendi içi (`10` v0.28):** §1'in veri kaybı maddesi KP-75'in özetidir ve işlem izini süresiz "kaybolmaz" diye sayıyordu. KP-75 2. turda düzeltilmişti, özet geride kalmıştı. Madde artık saklama süreleri boyunca kaybolmaz der ve imhayı KP-74'e bırakır (K-354). Öteki düzeltmeler satırın kendi içindedir. KP-63 ve KP-75 aynı imhayı (KP-74) gösterir. `10`'da anılan bütün karar numaraları karar kaydında var.

**Upstream — karar kaydı:**
- **Etki sütunları:** on dokuz kararın etki sütunu artık `10`'un cross-review'ının dokunduğu satırı ve sürümü ya da bu adımda `02` v0.45'te değişen yeri gösterir: K-12, K-84, K-293, K-354, K-355, K-407, K-454, K-491, K-493, K-523, K-553, K-557, K-573, K-574, K-576, K-607, K-609, K-628, K-641.
- **Geri işaretler:** `10`'un kalite döngüsü kararlarının tamamladığı, netleştirdiği ya da `10`'a yansıttığı on yedi eski karara geri işaret kondu: K-18 → K-631 · K-24 → K-638 · K-84 → K-641 · K-238 → K-630 · K-407 ve K-427 → K-629 · K-431 ve K-615 → K-644 · K-444 ve K-574 → K-628 · K-473 → K-639 · K-480 → K-627 · K-482 → K-620 · K-506 → K-622 · K-515 ve K-552 → K-623 · K-542 → K-620, K-642. Üç satır (K-84, K-407, K-574) hem etki hem geri işaret aldı; güncellenen satır sayısı otuz üçtür.
- **K-628'in açık işareti kapandı:** etki sütunu `01` cross-review R10 BULGU-2'nin kabulünün güncelleneceğini söylüyordu. Rapora tarihli bir not yazıldı (`01_CROSS_REVIEW_R10.md` §2): bulgunun "esnaf istisna olabilir" dayanağı ETBİS Tebliği m.5'in resmî metniyle çürüdü, Ü-1'in niteleyicisi "satış yapacak firmada" oldu.
- **K-618…K-644'ün öteki etki işaretleri** `02` v0.44, `01` v0.31 ve `10` v0.11'de kapalı. Etki sütunlarında "güncellenecek" ya da "etki yansıtmada" diye bekleyen işaret kalmadı. K-644 bir süreç kararıdır, dokümanlara dokunmaz.

**Yeni alan ve kural taraması:** cross-review yeni alan, enum değeri, parametre ya da iş kuralı eklemedi; on düzeltmenin hepsi kayıtlı bir kararın satırda eksik kalan parçasını yazdı. KP-63'teki "havale IBAN'ı" `02 §9.3.3`'ün ve KP-67'nin ifadesidir, sözlüğe yeni terim gerekmez. Audit turunun yeni alanları — KEP adresi, işletme adı ya da tescilli marka, meslekle ilgili davranış kuralları (K-627) ve "İşlem rehberi" sayfası (K-626) — `02 §3.1.2`, §3.1.3 ve §3.24.8'de tanımlı. `04` ve `06` devirleri etki sütunlarındadır; bu dokümanlar henüz yazılmadığı için orada kalır.

**Etki yansıtmada alınan karar:** yok.

**Açık kalan:** yok. `10`'un kalite döngüsü tamamlandı; K-437'nin 2. adımı (`01`, `02`, `10`) bitti, sırada çakışma taraması (K-429). Kaynak taramasının bulduğu eksik öğrenim adayı (7) olarak karar kaydının §6.1'ine yazıldı. Cross-review'dan gelmeyen bir eski eksik düzeltilmedi ve checkpoint'e devredilir: `02 §13`'ün Kaynak satırı gövdede anılan dört süreç kararını (K-430, K-431, K-437, K-615) taşımıyor; `02`'nin etki yansıtması Kaynak kuralını §1–§12'ye uyguladı.
