# Deep Review Raporu — 06 Data Model

**Tarih:** 2026-10-18 · **Hedef:** `Docs/06_DATA_MODEL.md` v0.17 → v0.18 (bulgular v0.17'de üretildi, v0.18'de işlendi; taban `main` = 1c31545) · **Bağlam:** `02_PRODUCT_REQUIREMENTS.md` v0.84, `03_USER_FLOWS.md` v0.33, `04_UI_SPECS.md` v0.32, `05_TECHNICAL_ARCHITECTURE.md` v0.21, `10_MVP_SCOPE.md` v0.54 (bulgular bunlar üzerinde üretildi); `PRODUCT_DISCOVERY_STATUS.md` §2 (K-992…K-1103) ve §14.1; [`06_AUDIT.md`](06_AUDIT.md) ve [`06_GUARANTEE_LENS.md`](06_GUARANTEE_LENS.md); `07`, `08`, `09`, `12`, `DEPLOY_RUNBOOK`'un Aşama 5 park blokları · **Odak:** Tam analiz — ağırlık katman 3, 4, 6, 7, 8 (yönetici talimatı); katman 1–2 tek hafif mercekte (audit'in düzeltmelerinin doğurduğu yeni tutarsızlıklar), katman 5 yalnız güvence merceğinin kapsamadığı hata modlarında

### Koşum biçimi (K-431; `audit` skill'i, "Koşum biçimi")

Dokümanı yazan bağlamda koşmadı. Yöneticinin verdiği adım bir yönetici alt ajanında yürüdü; mercekler, şüpheciler ve uygulayıcılar onun **ön planda** başlattığı ayrı alt ajanlardı. Her mercek bulgusunu bulduğu anda kendi dosyasına yazdı.

- **Dokuz mercek iki dalgada.** Dalga 1: **L12** (katman 1 ve 2 — hafif) · **L3a** (katman 3: §2, §3, §4.1–§4.35) · **L3b** (katman 3: §4.36–§9) · **L4** (katman 4, OWASP Top 10:2025) · **L6a** (katman 6: sipariş → kalem → kayıtlar → geri ödeme → imha). Dalga 2: **L6b** (katman 6: hesap ve dosya yaşam döngüsü) · **L7** (katman 7) · **L8** (katman 8, resmî belgelere karşı) · **L5** (katman 5 — yalnız güvence merceğinin kapsamadığı hata modları).
- **Audit'le çakışma yasağı.** Her mercek `06_AUDIT.md`'yi, `06_GUARANTEE_LENS.md`'yi ve audit'in 161 bulgusunun hüküm tablosunu okudu; audit'in bulduğunu yinelemedi. Audit'in düzeltmesinin doğurduğu yeni sorun "Audit bağı" alanıyla yazıldı.
- **Bulgu eşiği audit'ten kaba.** Savunmada derinlik ve üslup bulgu değil; Low yalnız somut yanlış için. Bu yüzden 29 ham bulgunun yalnız ikisi Low çıktı.
- **Karşı-doğrulama: beş parti × iki şüpheci = on şüpheci.** PA (para ve kalem) · PB (geri ödeme, IBAN, ödeme, sepet) · PC (hesap ve güvenlik) · PD (dosya, iş, teknik) · PE (diğer). **a** baştan sona bulguyu çürütmeye çalıştı; **b** sondan başa "gerçek sorun mu, en küçük düzeltme ne" sorusunu sordu. Doğrulanmış seviye iki şüphecinin yüksek olanıdır; ayrışan hükümleri yönetici alt ajanı verdi (aşağıda "Uygulanmayanlar").
- **Uygulama.** Doğrulanmış bulgular dört konu kümesine bölündü ve dört uygulayıcı **sırayla** aynı çalışma ağacında uyguladı: **D1** para ve kalem · **D2** geri ödeme, IBAN, ödeme · **D3** hesap ve güvenlik · **D4** dosya, iş, metin ve bağımlılık. Her uygulayıcı değiştirdiği maddeyi anan özet yerlerini betikle buldu (`checklists/document-stage.md` §4), kararları yer tutucuyla (`KTMP-Dn-m`) yazdı; yer tutucular sonunda K-1104…K-1124'e çevrildi. Üst dokümanların başlık sürümleri tek PR'da bir kez arttı; `06`'nın başlık sürümünü (v0.18) ve yazım durumu notunu yönetici yazdı.
- **Mekanik denetimler betikle.** `verify67` (§6 ve §7'nin §4'ten toplanması), atıf betiği (K ve `§` atıfları, alan ve enum değerleri), park satırlarının iki yönlü karşılaştırması, matris kümelerinin `cmp`'si, kontrol karakteri ve CR taraması (ayrıntı "Mekanik denetimler").
- **Model dağılımı.** Mercekler, şüpheciler ve hükümler Opus; dört uygulayıcı (D1–D4) ve bu raporun taslağı Sonnet; betikler yönetici alt ajanınca koşuldu.
- **Yasal dayanak ve dış olgular resmî kaynaktan doğrulandı** (6563 m.11/3, PostgreSQL 18, Npgsql, ASP.NET Core veri koruma, OWASP Top 10:2025 — "Yasal dayanak ve teknik doğrulama günlüğü").

**Karşı-doğrulama sonucu:** 29 ham bulgunun 26'sı işlendi (20 uygulandı · 5 kısmen uygulandı · 1 başka bulguyla karşılandı); biri — L7-1 — iki şüphecinin birlikte çürüttüğüdür (rapora girdi, uygulanmadı); ikisi — L6a-1 ve L6b-4 — tartışmalıydı ve yönetici hükmüyle elendi; biri — L8-2 — tartışmalıydı ve dar tutuldu (yalnız `08` park cümlesi). Doğrulanmış seviye dağılımı (26 işlenen bulgu): **High 3 · Medium 21 · Low 2** (L12-3 ve L3a-1 aynı düzeltmeyi paylaşır; iki bulgu seviyesiz kalır). Mercekler L4-1 ve L5-1'i High önermişti; şüpheciler ikisini Medium'a indirdi. Critical yok. **Çok kritik soru çıkmadı.** Kararlar: K-1104…K-1124 (21 karar; yedisi ⚠).

### Özet Skor Tablosu

"Ham" sütunu merceklerin bulduğu bulgu sayısıdır; skor hükümden sonraki durumdur (düzeltmeler bu adımda uygulandığından v0.18'e geçerken kalan iş yoktur).

| # | Katman | Ham | Skor | Kritik bulgu | Güçlü yönler (özet) |
|---|---|---|---|---|---|
| 1 | Kapsam | 1 | ✓ | Yok. Tek kapsam boşluğu L12-4: hiç bağlanmamış dijital dosya yüklemesinin silinmesi (Medium → K-1118) | Park satırları iki yönde tam; K-1092'nin dağınık yüzleri aynı başlangıç anıyla tutarlı; K-1101'in kod adı değişikliği temiz |
| 2 | Tutarlılık | 4 | ⚠ | L12-3 / L3a-1 (High): ayıpta dönülmüş adedi olan kalemde son işlemin kalanı tanımsız (K-1104). L12-1, L12-2, L12-5 (Medium) | Audit düzeltmelerinin çoğu evde ve özet yerlerinde tutarlı; kalanlar bir kararın tek yüzünün komşu kurala taşınmamasıdır |
| 3 | Teknik derinlik | 9 | ⚠ | L3a-1 (High, K-1104) ve L3a-2 (High, K-1105): para hesabının dayandığı türetilen değer kalemin sonradan doğan hâllerini (ayıpta dönme, yeniden açılma) saymıyor; 7 Medium | Para ölçeği ve yuvarlamanın yeri tek yerde; kontrol kısıtları hesabın son savunmasını veritabanına taşıyor; durum geçişinin veri karşılığı açık |
| 4 | Güvenlik | 3 | ⚠ | L4-1 (mercek High → Medium): sipariş erişim anahtarının açık saklanmasının gerekçesi yalnız okuma yüzünü tartmış (K-1114, ⚠; karar değişmedi, kalan risk adıyla yazıldı) | K-1081'in türetmesi temiz kurulmuş; işlem izinin değiştirilemezliği veritabanı yetkisiyle korunuyor; tekrar oynatma ve çift işlem veritabanında kapalı |
| 5 | Hata modu | 3 | ⚠ | L5-1 (mercek High → Medium): daraltma göçü kuyruktaki veri dönüşümünü beklemiyor (K-1121) | Dış yan etkiler geçişle aynı işlemde; "sonucu bilinmiyor" hâllerinin çıkışı veride; şema imhayla uyumlu |
| 6 | Veri akışı | 6 | ⚠ | L6a-2, L6b-1, L6b-2, L6b-3 (Medium): sayaç dönüşü, silinmiş hesabın siparişi, dosya bağı, Google bağı | Donmuş değerler eksiksiz ve düz; imha alan alan izlenebilir; snapshot/referans ayrımı tutarlı |
| 7 | Ölçeklenebilirlik | 1 | ✓ | Yok (L7-1 Low, iki şüpheci çürüttü) | 129 bağın tamamında dizin yazılı; kuyruklar filtreli dizinle küçük kalıyor; pahalı göç gerektiren MVP kararı yok |
| 8 | Bağımlılık | 2 | ⚠ | L8-1 (Medium): `pg_trgm` eklentisi adıyla yok, kurulum yetkisi yazılı değil (K-1124); L8-2 (Medium, dar): tek ödeme sağlayıcısı varsayımı `08`'e park (K-1112) | Sağlayıcıya özgü özelliklere bağımlılık bilinçle düşük; "bilgim tarihli" işaretleri belgeye karşı doğru çıktı; dış servislerin veriye düşen yüzü sağlayıcıdan bağımsız |

Merceklerin Özet bölümüyle karşılaştırma: L12 (katman 1 ✓, katman 2 ⚠), L3a ⚠, L3b ✓, L4 ⚠, L5 ⚠, L6a ⚠, L6b ⚠, L7 ✓, L8 ⚠ (hafif). Katman 3 iki mercekten birleşir (L3a ⚠, L3b ✓): L3a'nın iki High'ı katmanı ⚠ yapar. Ayrışan skor yok; L3b'nin ✓'i katman skorunu değiştirmedi.

**Genel:** ⚠ İyileştirme gerekli — düzeltmeler bu adımda uygulandı; Critical yok.

**Sayılar:** 29 ham bulgu (katman 1–2: 5 · 3: 9 (L3a 7 + L3b 2) · 4: 3 · 5: 3 · 6: 6 (L6a 2 + L6b 4) · 7: 1 · 8: 2). Doğrulanmış seviye: High 3 (L12-3, L3a-1, L3a-2) · Medium 21 · Low 2 (L12-5, L3b-2) · seviyesiz 3 (L6a-1, L6b-4 elendi; L7-1 çürütüldü). 26 işlendi: **20 uygulandı** · **5 kısmen uygulandı** (L12-3, L6a-2, L3a-5, L3a-4, L4-3) · **1 başka bulguyla karşılandı** (L3b-2). 21 karar K-1104…K-1124; 7'si ⚠.

### Katman Detayları

Tablolarda "Seviye" sütunu `mercek → doğrulanmış` biçimindedir (aynıysa tek değer). "Öneri" sütunu uygulanan düzeltmeyi, kararı (K) ve sonucu (uygulandı / kısmen / karşılandı / elendi) verir. Düzeltmenin tam metni karar kaydındadır.

#### Katman 1 — Kapsam

Merceği L12 (hafif). Okunan: K-1075…K-1103'ün bütün kolları, `06` başlık notu, §2, §4.17–§4.36, §4.42, §4.58, §4.69, §4.70, §5.5.3, §7, §9; `02`–`05`'in audit farkı (`git diff 9c09ac2 1c31545`); beş dokümanın Aşama 5 park blokları.

| # | Seviye | Bulgu | Kaynak | Öneri |
|---|---|---|---|---|
| L12-4 | Medium | Tamamlanmış ama formu henüz kaydedilmemiş dijital dosya yüklemesi: `06 §4.70` onu hiç silmiyor, `05 §7.7.3` yaş korumasız siliyor; `07`'nin K-1100 park satırı mekanizmayı yanlış özetliyor | 06 §4.70 Silme, §9.3.1; 05 §7.7.3; 07 park | **K-1118:** silinen dosya "ürünü ve varyantı silinmiş ya da hiç bağlanmamış, yüklemesi bir günden eski ve hiçbir kalemin bağı olmayan" dosyadır (`06 §4.70`, §9.3.1, §2.7); `07` park satırı düzeltildi; `05` değişmedi. Uygulandı |

**Güçlü yönler:** (1) Park satırları iki yönde tam — K-1075…K-1103'ün etki sütunlarında `07`/`08`/`09`/`12`/`DEPLOY_RUNBOOK` geçen her kararın park satırı var, ters yönde her park etiketinin K'sı o dokümanı etki sütununda sayıyor. (2) K-1092'nin dağınık yüzleri (`06` §2.2.1, §2.2.5, §4.17, §4.30, §4.35, §5.5.3, §9; `02` Z-29, §12.2.7; `03` 4.1.33; `05`) aynı başlangıç anı ve aynı "imha parçasından önce değil" koşuluyla yazılı. (3) K-1101'in beş kod adı değişikliği hiçbir dokümanda eski adıyla kalmadı.

#### Katman 2 — Tutarlılık

Merceği L12. Bulgular audit'in düzeltmelerinin doğurduğu komşu kural çatışmalarıdır.

| # | Seviye | Bulgu | Kaynak | Öneri |
|---|---|---|---|---|
| L12-3 | Medium → High | K-1077'den sonra ayıpta dönülmüş adedi olan kalemde "son açık adetleri kapatan işlem" doğmuyor; kalan pay kuralı erişilemez, toplam geri ödeme kalemin ödenmiş tutarını bir kuruş aşabiliyor | 06 §2.7.2, §4.22, §4.33; 02 §7.1.7 (4) | **K-1104 (⚠):** L3a-1 ile aynı düzeltme (aşağıda). Merceğin ek önerisi — kapanış kolu — uygulanmadı (K-1077'nin elediği seçenek; "kapanmış sayılır" sözü "kalemi kapatmaz"la çelişir). Kısmen |
| L12-1 | Medium | "IBAN bekleniyor" listesinin kalemsiz satırı: K-1075 yalnız `02 §7.2.8`'e yazıldı; sözlük ve panel listesi kalem satırında kaldı | 02 §1.2 `IbanPendingMark`, §10.6.1; 04 2.5.3.4, 2.6.2, 9.3.6 | **K-1108:** kalemsiz satırda "geç gelen ödemenin geri ödemesi"; süre sayacı olmayan satırlar arasında sayılır. `06 §4.29` doğru, dokunulmadı. Uygulandı |
| L12-2 | Medium | Eski ana dönüşte "bütün bağlantıların ömrü çekilir" (K-1083) e-posta değişikliğini geri alma bağlantısını da kapatır; `02 §3.13.14`'ün "geri alma yolu kapatılamaz" güvencesiyle çelişir | 05 §8.5.10, §8.7.4; 06 §9.5.2, §4.42 | **K-1113:** silmelerin yeniden uygulanması geri alma bağlantısını (`EmailChangeRevert`) hariç tutar; ömrü içinde açık kalır. İkinci yüz (bekleyen kaydın yeniden isteği) eşiğin altı, yazılmadı. Uygulandı |
| L12-5 | Medium → Low | `06 §7.4`'ün "İç notun kaydı" satırı K-1095'in koşulunu (imha edilmiş siparişe yazılmaz) taşımıyor | 06 §7.4 | K-1095'in özet hizalaması (yeni K yok; K-1095'in etkisine `06 §7.4` eklendi). Uygulandı (yalnız §7.4 hücresi) |

**Güçlü yönler:** (1) K-1078, K-1080, K-1081, K-1086/K-1091'in ev ↔ özet hizası genel olarak tutuyor. (2) Yeni kuralların istediği veri (K-1075'in kontrol kısıtı ve filtreli tekil dizini, K-1092'nin imha kaydı türü) §7'de ve §9'da karşılığıyla duruyor. (3) Beş dokümanın park blokları ile karar kaydının etki sütunları birbirini iki yönde tutuyor.

#### Katman 3 — Teknik derinlik

Mercekleri L3a (§2, §3, §4.1–§4.35, §7.3–§7.4; `02 §7.1`–§7.5 ile karşılaştırma) ve L3b (§4.36–§9; `05 §4.3`–§5.4, §7.4, §8.5, §8.7.5, §10.2.2 ile karşılaştırma). Ortak kök (L3a): para hesabının dayandığı türetilen değerin okunuşu, kalem kayıtlarının sonradan doğan hâllerini bütünüyle saymıyor.

| # | Seviye | Bulgu | Kaynak | Öneri |
|---|---|---|---|---|
| L3a-1 | High | Ayıpta sözleşmeden dönülmüş adedi olan kalemde "son açık adetleri kapatan işlem" tanımsız: kalan kupon payı ve KDV ayrılmıyor, ücretsiz kargoda tavan denetimi yasal caymanın geri ödemesini reddediyor (Audit bağı K-1077) | 06 §2.7.2, §4.18, §4.22, §4.33; 02 §7.1.7 (4) | **K-1104 (⚠):** ölçüt "işleme açık son adetler" olur (`02 §7.1.7` (4) ve `06` dört yer); dönülmüş adedin kaynağı §4.18 Kısıtlar'da yazılır; `12` park satırına senaryo cümlesi. Uygulandı |
| L3a-2 | High | Yeniden açılan "mal dönmedi" kapanışında "bu yolla ödenmiş kısım" veriden bulunamıyor — tutar bazlı geri ödeme kapanışa bağ taşımıyor | 06 §4.33, §4.28 `ReopenedAt`; 02 §10.4.9 | **K-1105 (⚠):** `Refund.NotReturnedClosureId` (isteğe bağlı, yalnız `AmountBased`, kısıtlayıcı bağ); `06` §4.33, §4.28, §6, §7.2 (129 → 130 bağ), §7.3 kontrol kısıtı; `02 §10.4.9` yan cümle; `04` 9.9.22. Uygulandı |
| L3a-3 | Medium | IBAN'ın silineceği an yalnız beyanın girişinde yazılıyor; beyanın "teslimden önce/sonra" sınıfı sonradan değişince yeniden hesaplanmıyor | 06 §4.31 `ScheduledDeletionAt`; §9.1 Z-38 | **K-1109:** sınıfı değiştiren yazımda silinme anı aynı işlemde yeniden hesaplanır; `02`'ye dokunulmadı. Uygulandı |
| L3a-4 | Medium | Aynı kalemde birden çok bekleyen havale geri ödemesi varken IBAN eşlemesi ve silinme anı veriden çözülmüyor | 06 §4.31, §9.1 Z-35/Z-38; 02 §7.4.5 | **K-1110 (⚠):** IBAN, onu kullanan **son** bekleyen havale geri ödemesi tamamlanana kadar kalır (yalnız (b) kolu). (a) kolu ("kalemde IBAN dururken yeni geri ödeme o IBAN'ı kullanır") uygulanmadı — K-690 cevaplar. Kısmen |
| L3a-5 | Medium | Kupon hakkının iadeyle dönüşünü hangi kaydın işleminin yazdığı belirsiz (beyan mı, teslim alma mı, hizmet caymasında ne?) | 06 §4.14; 02 §5.11.3 | **K-1106:** kupon hakkı siparişin son açık adetlerini kapatan kaydın işleminde bir kez döner; `02 §5.11.3` yan cümle (gecikme feshiyle kapanan adet iptal sayılır). Hizmet kontenjanı kolu yazılmadı (PA-a: genel sayaç kuralı karşılar). Kısmen |
| L3a-6 | Medium | Bir işlem birden çok kalem kaydı yazdığında kargo ücretinin hangi geri ödemeye ekleneceği tanımsız | 06 §4.33 `IsShippingFeeIncluded` | **K-1107 (1):** ücret kalem kimliği en küçük olanın geri ödemesine eklenir; tutarı değiştirmez. Uygulandı |
| L3a-7 | Medium | "Bekleyen geri ödeme"nin okunuşu düzeltmeyle geri alınmış firma iptalini dışarıda bırakmıyor — havale hattında hayalet geri ödeme | 06 §4.33 İlişkiler | **K-1107 (2):** bekleyen geri ödeme geri alınmış firma iptali sayılmadan okunur. Uygulandı |
| L3b-1 | Medium | Sonradan gelen geri dönmenin (bounce) gönderim kaydında yeri yok — "Sent ⇔ SentAt" kısıtı K-913'ün "satırın sonucunu geri dönme değiştirir"ini kapatıyor | 06 §4.58 `DispatchState`, Kısıtlar, §7.3 | **K-1120:** `SentAt` `Undelivered`'da yalnız gönderildikten sonra geri dönen satırda doludur; saklama başlangıcı cümlesi hizalandı; `08` park satırı (geri dönmenin işlenişi `08`'in). Uygulandı |
| L3b-2 | Medium → Low | `05 §8.7.5`'in "büyük veri dönüşümü iş kuyruğunda" kuralı iş satırının biçimine sığmıyor (`RelatedRecordId` zorunlu, ilerleme alanı yok) | 06 §4.59; 05 §8.7.5 | L5-1'in `09` park cümlesine yan cümleyle karşılandı (**K-1121**): "dönüşüm işinin satırı ilerlemesini kendi kaydında ya da iş satırının bağıyla taşır — biçimi Kodlama Kılavuzu'nundur". Karşılandı |

**Güçlü yönler:** (1) Para tutarlarının ölçeği, yuvarlamanın yeri ve donan/türetilen değerlerin ayrımı (§2.5.1, §2.7) tek yerde; kalem kayıtlarının `ProductAmount`/`CouponShare`/`VatAmount` formülü `02 §7.1.7` (1)–(4) ile birebir. (2) §4.17'nin Sürüm tablosu sürümü artıran ve artırmayan her yazımı adıyla sayıyor; §2.6.5 ve §7.4 her koşullu güncellemenin koşulunu ve dayandığı dizini yazıyor. (3) Stok ayırmanın altı yazım yolu (§4.14), kart iadesinin istek kaydı (§4.34) ve gelen bildirimin tekilliği (§4.35) uygulamacının doğrudan kodlayabileceği düzeyde. (4) L3b: §9, §8 ve §7 "ama nasıl?" bırakmıyor; iki bulgu da birer cümlelik düzeltme ve park işaretiyle kapandı.

#### Katman 4 — Güvenlik

Merceği L4. Çerçeve **OWASP Top 10:2025** (https://top10.owasp.org/2025, okundu 2026-10-18). Okunan: `06` §2.3, §2.10, §4.15, §4.17, §4.31, §4.35, §4.36, §4.39–§4.44, §4.58, §4.59, §4.61, §4.63, §4.68, §4.70, §9.5; `05 §6`, §7.1.3, §7.4, §7.6, §7.7; `02 §3.22`, §8.3.5–§8.3.8, §9.1.6, §10.4.11; karar kaydı K-903, K-1048, K-1068, K-1081…K-1083, K-1026.

| # | Seviye | Bulgu | Kaynak | Öneri |
|---|---|---|---|---|
| L4-1 | High → Medium | Sipariş erişim anahtarı açık saklanır; K-903/K-1048'in "özet ek koruma vermez" gerekçesi yalnız okuma yüzünü tartmış — veritabanını okuyan, sipariş sayfasından IBAN girebilir/düzeltebilir (iz ve bildirim üretmez), iptal ve cayma beyanı verebilir (Audit bağı K-1082) | 06 §4.17 `OrderAccessKey`, §4.31 `Iban`; 05 §6.4.9 | **K-1114 (⚠):** karar (K-903, K-1048) **değişmedi**; gerekçe tamamlandı (sayfa numara + e-postayla da açılır, `02 §3.22.3` — türetme yolu kapatmaz) ve kalan risk adıyla yazıldı. Merceğin (a) türetme / (b) sütun yasağı önerileri uygulanmadı. Uygulandı |
| L4-2 | Medium | E-posta düzeltmesinden sonra eski "ulaşmadı" grubunun yeniden gönderimi yeni erişim anahtarını eski (yanlış) adrese götürür | 06 §4.58 "Uygulamada"; 02 §9.1.6 | **K-1115 (⚠):** yeniden gönderim siparişin güncel iletişim e-postasına gider (B-15 düzeltmeden önceki adrese); `02 §9.1.6`, `03` 7.1.37 ve 8.9.2 aynı sözle. Merceğin birinci önerisi (eski adrese giden grup işareti düşürmez) K-675'i değiştireceği için uygulanmadı. Uygulandı |
| L4-3 | Medium | `06 §2.10.2` "okumada temizleme yapılmaz" der; `05 §6.5.3` "kayıtta ve gösterimde atılır" der — temizleyici açığı kapandıktan sonra saklı yük vitrinde kalır | 06 §2.10.2; 05 §6.5.3 | **K-1122 (⚠):** `06`, `05`'e hizalanır — gösterimde aynı izin listesiyle süzülür, saklanan değer yeniden yazılmaz; `05` olduğu gibi kalır. Yeniden temizleme göçü kolu uygulanmadı (yayımlanmış yasal metin sürümü yalnız eklenir, `02 §3.33.3`). Kısmen |

**OWASP Top 10:2025 — kategori başına değerlendirme**

| Kategori | Sonuç | Veri modelindeki yüzü |
|---|---|---|
| A01 Broken Access Control | bulgu — L4-1, L4-2 | Erişim anahtarı sipariş sayfasının ve IBAN alanının tek kapısıdır; açık saklanışı ve düzeltmeden sonra eski adrese yeniden gönderim. İç anahtarın vitrinde sahiplik denetimiyle kullanılışı (§2.3.4) sorunsuz |
| A02 Security Misconfiguration | sorun yok | Bileşen başına veritabanı kullanıcıları, halka tablosunun salt okuma yetkisinden çıkarılması (`05 §7.1.3`), yedeğin durum kaydının tek satırlık yazarı, işlem izinin yetkileri; salt okuma rolünün kuruluşu `09` park satırında |
| A03 Software Supply Chain Failures | bulgu — L4-3 (yüzü) | Temizleyici kütüphanesinin açığı kapatıldıktan sonra saklı metnin yeniden süzülmemesi; veri koruma paketinin varlık türüne bağımlılık (K-1068) ve ICU sürümü (K-1087) — sorun yok |
| A04 Cryptographic Failures | bulgu — L4-1 | Anahtarlar özetle saklanıyor (§2.3.3); tek açık değer erişim anahtarı. Şifresiz halka (K-1082) gerekçeli kalan risk; şifre PBKDF2 (`05 §6.1.4`); IBAN alan şifrelemesi gerekçeyle elendi (`05 §6.8.6`) |
| A05 Injection | bulgu — L4-3 | Saklanan HTML (§2.10). Arama joker karakterleri ve sorgu parametreleri bu dokümanın düzeyi değil (`09`); imha fonksiyonunun arama yolu sabit (§4.63) |
| A06 Insecure Design | bulgu — L4-1 | Okuma → yazma yükselmesinin bir anahtarda High (K-1081), ötekinde "ek koruma yok" (K-903) diye tartılması tasarım düzeyinde tutarsızlık |
| A07 Authentication Failures | sorun yok | Sunucuda oturum, özetli anahtar, yeniden doğrulama izninin tek kullanımlık koşullu tüketimi (K-1084), bağlantının tek kullanımı, eski ana dönüşte oturumların silinmesi (K-1083) |
| A08 Software or Data Integrity Failures | sorun yok | Gelen bildirimin imzası ve tekilliği (§4.35); indirme bağlantısının bütünlüğü halkaya dayanır — kalan risk adıyla (K-1082); sürüm değeri ve koşullu güncellemeler (§2.6) |
| A09 Security Logging and Alerting Failures | sorun yok (yüzü L4-1'de) | Değiştirilemez işlem izi, her giriş denemesinin kaydı (§4.44), imha kaydı; müşterinin IBAN düzeltmesinin izsiz oluşu bilinçli karar (K-710, K-1058) |
| A10 Mishandling of Exceptional Conditions | sorun yok | Deneme satırı işlemden önce kesinleşir (kapalı başarısızlık); sır döndürmesinde eşleşmeyen bağlantı "kayıt yok"a düşer; tutarı tutmayan bildirim işlenmez |

**Bulgu çıkmayan öncelikli sorular** (gerekçeler L4 merceğinin dosyasında): K-1081 (türetilen hesap bağlantısı anahtarı; girdi tahmin edilebilir olsa da HMAC'in güvenliği sırra dayanır, her bağlantı kendi satırıdır, sır tek başına hiçbir şey açmaz, kuyruktaki e-posta kapalı başarısızlıkla düşer) · K-1082 (kalan riskin kapsamı doğru — aşağıda) · dışa görünen kimlikler (UUID v7 zaman sızdırması kazanç sağlamaz, sipariş numarası tek başına sayfa açmaz) · oturum, bağlantı, deneme, tanınan tarayıcı · gelen bildirim · işlem izi · iş kuyruğu · dosya indirme · arama (K-886).

**Güçlü yönler:** (1) Anahtar saklama kuralı tek yerde ve istisnalarıyla sayılı (§2.3.3) — L4-1 tek alana indirgenebildi. (2) K-1081'in türetmesi temiz: ham anahtar hiçbir tabloda yok, sırrın dağıtım yarıçapı ve döndürmesi `05 §6.x`'te bileşen bileşen yazılı. (3) İşlem izinin değiştirilemezliği veritabanı yetkisiyle korunuyor (yalnız ekleme ve okuma; imha `SECURITY DEFINER`, sabit arama yolu). (4) Tekrar oynatma ve çift işlem veritabanında kapalı (iki filtreli tekil dizin, koşullu güncellemeyle tüketim, bekleyen iş satırı tekilliği).

#### Katman 5 — Hata modu

Merceği L5; **yalnız güvence merceğinin (G1–G6) kapsamadığı hata modları**. Okunan: `06` §2.9, §4.17, §4.25–§4.28, §4.32–§4.35, §4.58–§4.60, §4.65–§4.67, §4.69, §5.2, §5.3.12, §5.4, §8, §9.2–§9.5; `05 §4.1`–§4.7, §5, §7.4, §7.6, §7.7, §8.6, §8.7, §10.

| # | Seviye | Bulgu | Kaynak | Öneri |
|---|---|---|---|---|
| L5-1 | High → Medium | Daraltma göçü arka plandaki veri dönüşümünün bitmesini beklemiyor: geride kalmış kurulumun art arda sürüm geçişinde dönüşmemiş satırların eski sütunu düşer (sessiz veri kaybı). Kusurun evi `05`'tir; `06 §2.9.3` kuralı doğru söyler; dönüşüm işi bugün yoktur | 05 §8.7.5, §8.7.1; 06 §2.9.3 | **K-1121:** `05 §8.7.5`'e "dönüşüm bitmeden eski yapı kaldırılmaz" cümlesi (göç önce dönüşecek satır kalmadığını denetler, kalmışsa hata verir); `09` ve `DEPLOY_RUNBOOK` park satırları; `06` değişmedi. Uygulandı |
| L5-2 | Medium | Kart iadesi işinin, geri ödeme kaydını yazamadan düşen denemesinin hâli yazılı değil; "kendiliğinden yeniden denenmez" okunursa geri ödeme kayıtsız ve işaretsiz kalır | 05 §4.3.4; 06 §4.59 `AttemptCount` | **K-1111:** "yeniden denenmez" kuralı **gönderilmiş isteğe** aittir; kaydı yazılmadan düşen deneme isteği göndermemiştir ve varsayılan kuralla yeniden denenir. `06 §4.34` ve `02` değişmedi. Uygulandı |
| L5-3 | Medium | Kargo şirketi listesinden çıkan şirketin kod adının kalıcılığı yazılı değil (kuralı yalnız tablodaki il/ülke listeleri taşır); on yıllık ticari kaydın şirketi çözülemez ya da kod yeniden kullanılırsa yanlış kurumu gösterir | 06 §5.3.12, §4.17 `Carrier` | **K-1123:** koddaki listede de kod adı kalıcıdır, başka şirkete verilmez; `08` park satırı (liste içeriği ve "seçilemez" işareti `08`'in). `02` değişmedi. Uygulandı |

**Güvence merceğinin kapsamı dışında bakılan ve bulgu çıkmayan hata modları:** (1) yeni enum değerini eski kodun okuması — `05 §8.7.1` önce okuyan sonra yazan sürümü ister, sürüm atlanmaz (K-960); (2) göçün uzun kilidi — H-1 hacminde (günde 10, tepe 50 sipariş) kısa kilit, kilit kuyruğunun sınırı `09` park satırında; (3) yarıda kalan göç — her göç kendi işleminde, geri alınır, fail-fast eksik seed'i adıyla yakalar; (4) veritabanı erişilemezliği ve havuz tükenmesi — iç ve dış yan etkiler aynı işlemde (outbox), çöken işleyicinin satırı yeniden alınır; (5) ödeme sağlayıcısı kesintisi — son sorgu yanıt gelene kadar yinelenir, "ödeme sonucu alınamadı" işareti süreden okunur, iki yolu firma açar; (6) e-posta göndericisi kesintisi — P-38 aralıklarıyla yeniden deneme, sonra "ulaşmadı", firmanın yeniden gönderimi çıkıştır; (7) diskin kalıcı kaybı — `05 §7.6.2` dosyasız kayıtları listeler, görselin anahtarı içerikten türediği için yeniden yükleme geri getirir; (8) gecelik imha işinin tek parçaya takılması — somut zehirli satır bulunamadı, uyarı 26 saatte çıkar (savunmada derinlik); (9) nabzın bozulması — `05 §5.4.6`'da kalan risk olarak adlandırılmış.

**Güçlü yönler:** (1) Dış yan etkilerin hepsi geçişle aynı işlemde yazılır — yarım yan etki bırakmaz. (2) "Sonucu bilinmiyor" hâllerinin her birinin çıkışı veride var; "başarısız" iş satırı silinmez. (3) Şema imhayla ve bağların silinmesiyle uyumlu (boşaltılan alanlar kontrol kısıtında birlikte boş; silinen kayda giden alanlar bağ değil değer). (4) Göç genişlet–daralt kuralı değerleri de kapsıyor; kapalı listelerin satırı silinmiyor, kodu yeniden kullanılmıyor.

#### Katman 6 — Veri akışı

Mercekleri L6a (sipariş → kalem → kayıtlar → geri ödeme → imha; K-1092'nin yasal okuması) ve L6b (hesap ve dosya yaşam döngüsü; `bin/fk4.tsv`'in hesaba, yöneticiye, görsele ve dosyaya giden 38 bağı).

| # | Seviye | Bulgu | Kaynak | Öneri |
|---|---|---|---|---|
| L6a-1 | Medium → elendi | K-1086'nın istisnası birleşmenin yalnız "silme" koluna yazılmış; "üyenin sepeti yoksa misafir sepeti üyeye geçer" kolunda (A)'yı eleyen zarar doğuyor | 06 §4.15, §4.17 `CartId`; 02 §3.16.12, §3.17.7 | Tartışmalı (a: kısmen, b: çürüttü) → **elendi.** Gerekçe "Uygulanmayanlar"da. Cross-review'a not |
| L6a-2 | Medium | Ödemeden sonraki sayaç dönüşü yalnız iptal ve çıkarma için yazılı: gecikme feshinin "kargoya verilmemişse" koşulu, caymada hizmet kontenjanı, kupon hakkının "iade" tetiği | 06 §4.14 son paragraf | **K-1106** (L3a-5 ile tek düzeltme): dönüş kaydı yazan geçişin işlemindedir; gecikme feshinde stok yalnız kalem kargoya verilmemişse. Hizmet kontenjanı kolu yazılmadı (genel kural karşılar). Kısmen |
| L6b-1 | Medium | Silinmiş hesabın misafir siparişinde e-posta düzeltmesi: `02 §10.4.11` (5) ile §3.15.3 çatışır; `IsAccountDeleted` ile `CustomerId`'nin birlikte dolu hâli tanımsız | 06 §4.17 `CustomerId`; 02 §10.4.11 | **K-1116:** bağ yeniden kurulmaz; kontrol kısıtı "`IsAccountDeleted` doğruysa `CustomerId` boştur" (§4.17 Kısıtlar, §7.3; hesabın silinmesinde bağ işaretten önce ya da onunla aynı ifadede boşalır); `02`, `03` yan cümleleri. Uygulandı |
| L6b-2 | Medium | Kalemin dosya bağı onayda yazıldığı için değiştirilen her dosya, hiçbir kalem indiremediği hâlde siparişin imhasına kadar diskte kalır; `05 §7.7.3`'ün "indiremeyeceği dosya silinir" ölçütüyle ayrışır | 06 §4.18 `DigitalFileId`, §4.70 | **K-1119** (b'nin düzeltmesi): bağ yeni dosya bağlanırken yeni dosyaya çekilir; bağ her an kalemin indireceği dosyadır, sürümü artırmaz. K-883'ün elediği "eski dosyalar hiç silinmesin" seçeneği yeniden açılmadı. Uygulandı |
| L6b-3 | Medium | Doğrulanmış e-posta değişikliğinden sonra `GoogleLinkedAt` dolu kalır ama bağ çalışmaz; hesap ekranı ve üye kaydı "Google — bağlı" der | 06 §4.36, §4.40; 02 §3.13.7, §3.13.14 | **K-1117 (⚠):** doğrulamada bağ sistemce kalkar (`GoogleLinkedAt` aynı işlemde boşalır); yeni adresle ilk Google girişi bağı yeniden kurar. Çelişen iki cümle (`03` 9.2.2, `04` 5.25.6) hizalandı; "ikinci hesap" kolu düştü. Uygulandı |
| L6b-4 | Medium → elendi | K-1080'in bekleyen kayıt silmesi hesap türünü sınırlamıyor: yöneticinin e-posta değişikliği müşteri tarafındaki bekleyen kaydı da siler (Audit bağı K-1080) | 06 §4.36, §4.37 | Tartışmalı (a: kısmen, b: çürüttü) → **elendi:** zarar yalnız posta kutusunun sahibi olduğunu kanıtlamış kişinin kayıt formunu yeniden doldurmasıdır; eşiğin altında |

**Güçlü yönler:** (1) Sipariş ve kalemin donmuş değerleri eksiksiz ve düz — canlı kayda hiçbir okuma kalmıyor; canlı/donmuş ayrımı sepet kaleminden kaleme açık. (2) `02 §7.1.7`'nin pay ve KDV formülü kalem kayıtlarına, parça parça teslim almaya (K-1057), ayıpta sözleşmeden dönmeye (K-1077) ve geri alınmış firma iptaline (K-1079) tutarlı taşınmış. (3) İmha akışı alan alan izlenebilir; değer/bağ ayrımı asılı bağ bırakmıyor. (4) Snapshot/referans ayrımı: yöneticiye giden on dört bağ boşaltılır, ad kaydın kendi alanında değer olarak kalır; hesabın silinmesinin dökümü `02 §3.15.1`'in her maddesine veride karşılık veriyor. (5) Dosyada satır ile disk nesnesinin silinme sırası kırık bağ doğurmayacak biçimde kurulmuş.

#### Katman 7 — Ölçeklenebilirlik

Merceği L7. Okunan: §2.2, §2.3, §2.7, §2.9, §4.15–§4.18, §4.43, §4.44, §4.58–§4.60, §4.63, §4.65, §7.1, §7.2 (129 bağın Dizin sütunu), §7.5, §7.6, §9.1, §9.2, §9.5; `10 §4.2` SK-5; `05 §5.2`, §6.7, §11.

| # | Seviye | Bulgu | Kaynak | Öneri |
|---|---|---|---|---|
| L7-1 | Low → çürütüldü | Hacim kabulü sipariş tarafını "on yıllık" sayıyor; sipariş tarafının satırları hiç silinmez — en uzun yaşayan tabloların ufku yoktur | 06 §7.6.1, §9.5.1 | **İki şüpheci çürüttü** (PE-a, PE-b). Rapora girdi, uygulanmadı |

**Güçlü yönler:** (1) Yabancı anahtarların dizin kapsamı tam: 129 bağın her birinin dizini yazılı; PostgreSQL'in yabancı anahtara kendiliğinden dizin açmadığı not edilmiş. (2) Kuyruklar ve zamanlayıcı taramaları filtreli dizinlerle çalışıyor — dizinler on yıllık birikimden bağımsız küçük kalıyor. (3) Büyüme sınırlanmış: ara tablolar gecelik temizlikle, süreli kayıtlar veriden hesaplanan sürelerle siliniyor; işlem izinin imhası `max_rows` ile parçalı; sık yazılan tek satırlar dizinsiz sütunlarda (HOT). (4) UUID v7, metin + kontrol kısıtlı enum'lar ve genişlet–daralt kuralı ileriyi açık tutuyor; pahalı göç gerektiren MVP kararı bulunmadı.

#### Katman 8 — Bağımlılık

Merceği L8; teknik dayanaklar resmî belge ve kaynak koduyla karşılaştırıldı (hepsi 2026-10-18). Okunan: başlık notu, §2, §4 girişi, §4.5, §4.6, §4.8, §4.15, §4.17, §4.20, §4.32, §4.34, §4.35, §4.37, §4.58, §4.60, §4.63, §4.68, §7.1.1, §7.2.1, §7.5, §7.6, §8.4.2–§8.5; `05` MK-03, MK-06, §2.3, §3.2.2, §3.6.4, §7.1.3, §7.9.1, §8.2, §8.8; `08` park blokları; `10` ÖK-4.

| # | Seviye | Bulgu | Kaynak | Öneri |
|---|---|---|---|---|
| L8-1 | Medium | Trigram dizininin dayandığı `pg_trgm` eklentisi hiçbir yerde adıyla yok; kimin, hangi yetkiyle kuracağı yazılı değil (eklenti "trusted": veritabanında `CREATE` yetkili kullanıcı kurabilir; `05 §7.1.3` rol modeli bu yetkiyi vermiyor) | 06 §4.5, §7.5.1, §8.5.2; 05 §7.1.3 | **K-1124** (b'nin kurucu kullanıcı kolu): `05 §7.1.3` — kurucu kullanıcı ilk kurulumda eklentiyi kurar; `06`'da eklentinin adı (§4.5, §7.5.1, §8.5.2); `DEPLOY_RUNBOOK` park satırı. Göç kullanıcısına veritabanında `CREATE` elendi; operatör sınıfı `09`'un. Uygulandı |
| L8-2 | Medium | Ödeme kaydı ve gelen bildirim hangi sağlayıcıya ait olduğunu taşımıyor; sağlayıcı sayısı ve kurulum ömründe sağlayıcı değişimi açıkken tekillik ve geri ödemenin yönü buna bağlı | 06 §4.32, §4.35; 08 park (K-932, K-1059) | Tartışmalı (a: çürüttü — değişim `03` 10.1.1.7 ve `08`'de parklı, bugünkü tek sağlayıcılı kararla arıza yolu yok; b: kısmen — park satırları işin veri sonucunu adlandırmıyor) → **tutuldu, dar: K-1112** — yalnız `08`'in K-1059 maddesine (K-932 Aşama 4 bloğunda olduğu için) PB-b'nin cümlesi; `06`'ya alan ya da koşullu kural yazılmadı. Uygulandı (dar) |

**Güçlü yönler:** (1) Sağlayıcıya özgü özelliklere bağımlılık bilinçle düşük: `xmin` yerine açık `version` sütunu, `citext`/`lower()` ifade dizini yerine normalleştirilmiş sütun, enum'lar metin + kontrol kısıtı; PostgreSQL'e özgü kalanlar adıyla ve yeriyle yazılı. (2) "Bilgim tarihli" işaretleri yerinde ve belgeye karşı doğru çıktı. (3) Dış servislerin veriye düşen yüzü sağlayıcıdan bağımsız: gelen bildirimin olay türü sağlayıcının değeri olarak metin; gönderim kaydı SMTP düzeyinde sonuç sınıfı taşıyor. (4) ICU sürüm riski açık (K-1087), çatının tablosu iki modelleme kuralının istisnası olarak işaretli.

**Doğrulanan dayanaklar (L8; hepsi 2026-10-18):**

| `06`'nın dayanağı | Sonuç | Kaynak |
|---|---|---|
| §2.3.1 — UUID v7, uygulama üretir | Doğru: Npgsql EF Core 9.0'dan itibaren GUID anahtarlarını sıralı (v7) üretir; PostgreSQL 18'de `uuidv7()` de var (`06` onu seçmemiş, gerekmiyor) | https://www.npgsql.org/efcore/modeling/generated-properties.html · https://www.postgresql.org/docs/18/functions-uuid.html |
| §4.68 — çatı anahtar tablosu (`Id` `int` kimlik; `FriendlyName`, `Xml` boş olabilir metin) | Doğru | https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.dataprotection.entityframeworkcore.dataprotectionkey.id · https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/implementation/key-storage-providers |
| §4.68 — çatı anahtarları silmez; eski anahtar açmak için tutulur | Doğru: "Created, active, and expired keys may all be used to unprotect incoming payloads."; varsayılan ömür 90 gün | https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/implementation/key-management |
| §4.68 — halka şifresiz (K-1082) | Doğru: açık saklama konumu verilince varsayılan şifreleme kaldırılır (audit de doğruladı) | key-storage-providers (yukarıda) |
| §4 girişi, §4.6, §4.8 — kısıtlayıcı bağ `NO ACTION`; ürün silinirken basamaklama sırası | Doğru (kaynak koddan): RI basamaklaması `spi.c`'de dış deyim kuyruğuna eklenir; varyantın basamaklanması seçenek değerinin denetiminden önce koşar | https://raw.githubusercontent.com/postgres/postgres/REL_18_STABLE/src/backend/executor/spi.c · .../utils/adt/ri_triggers.c · https://www.postgresql.org/docs/18/ddl-constraints.html |
| §4.20 — iç notun `FOR SHARE`'i imhanın satır kilidiyle çakışır (K-1095) | Doğru: Tablo 13.3, `FOR SHARE` ↔ `FOR NO KEY UPDATE`/`FOR UPDATE` | https://www.postgresql.org/docs/18/explicit-locking.html |
| §4.6, §7.1.1 — `NULLS NOT DISTINCT` | Doğru (PostgreSQL 15+; Npgsql `AreNullsDistinct(false)`) | https://www.postgresql.org/docs/18/ddl-constraints.html · https://www.npgsql.org/efcore/modeling/indexes.html |
| §4.5, §7.5.1 — trigram dizini | `pg_trgm` "trusted" — kuran kullanıcıda veritabanında `CREATE` yetkisi yeter → L8-1 | https://www.postgresql.org/docs/18/pgtrgm.html · https://www.postgresql.org/docs/18/sql-createextension.html |

### Karar listesi (K-1104…K-1124)

Kayıt modu K-992 (⚠ öneriyle kayıt): her kararın konu hücresi `(deep review — Dn kümesi)` ve işareti taşır. Tam metinleri ve elenen seçenekleri karar kaydında. Kümeye göre: D1 4 (K-1104…K-1107) · D2 5 (K-1108…K-1112) · D3 5 (K-1113…K-1117) · D4 7 (K-1118…K-1124).

| K | Karar (tek cümle) | Bulgular | İşaret | Dokunulan dokümanlar |
|---|---|---|---|---|
| K-1104 | Kupon payı ve KDV'nin kalanını kalemin **işleme açık** son adetlerini kapatan işlem alır; ayıpta sözleşmeden dönme de bu işlemlerden biridir | L3a-1, L12-3 | ⚠ | `02 §7.1.7`; `06` §2.7.2, §4.18, §4.22, §4.33; `12` park |
| K-1105 | `Refund.NotReturnedClosureId` ile yeniden açılan "mal dönmedi" kapanışında düşülen tutar o kapanışa bağlı tutar bazlı geri ödemelerle ödenmiş kısımdır | L3a-2 | ⚠ | `02 §10.4.9`; `04` 9.9.22; `06` §4.33, §4.28, §6, §7.2, §7.3 |
| K-1106 | Sayaç dönüşü dönüş kaydı yazan geçişin işlemindedir; kupon hakkı siparişin son açık adetlerini kapatan kaydın işleminde bir kez döner | L6a-2, L3a-5 | öneriyle | `02 §5.11.3`; `06 §4.14` |
| K-1107 | Kargo ücreti kalem kimliği en küçük olanın geri ödemesine eklenir; bekleyen geri ödeme geri alınmış firma iptalini saymaz | L3a-6, L3a-7 | öneriyle | `06 §4.33` |
| K-1108 | "IBAN bekleniyor" listesi kalemsiz satırda siparişi gösterir; geç ödemenin geri ödemesinin süre sayacı yoktur | L12-1 | öneriyle | `02 §1.2`, §10.6.1; `04` 2.5.3.4, 2.6.2, 9.3.6 |
| K-1109 | Beyanın sınıfını değiştiren yazımda IBAN'ın silinme anı aynı işlemde yeniden hesaplanır | L3a-3 | öneriyle | `06 §4.31`, §9.1 Z-38 |
| K-1110 | IBAN, onu kullanan son bekleyen havale geri ödemesi tamamlanana kadar kalır (aynı kalemin öteki kayıtları dahil) | L3a-4 | ⚠ | `02 §7.4.5`, §4.2 Z-35, Z-38; `06 §4.31`, §9.1 |
| K-1111 | Kart iadesinin "yeniden denenmez" kuralı gönderilmiş isteğe aittir; kaydı yazılmadan düşen deneme yeniden denenir | L5-2 | öneriyle | `05 §4.3.4`; `06 §4.59` |
| K-1112 | Veri modeli tek ödeme sağlayıcısını varsayar; çok sağlayıcı ya da değişim kabul edilirse sonuç Veri Modeli'ne döner (park cümlesi) | L8-2 | öneriyle | `08` park |
| K-1113 | Eski ana dönüşte geri alma bağlantısının ömrü çekilmez | L12-2 | öneriyle | `05 §8.5.10`, §8.7.4; `06 §9.5.2`, §4.42; `DEPLOY_RUNBOOK` park |
| K-1114 | Sipariş erişim anahtarının açık saklanması değişmez; veritabanını okuyanın sipariş sayfası işlemlerini yapabilmesi kalan risk olarak adıyla yazılır | L4-1 | ⚠ | `05 §6.4.9`; `06 §4.17` |
| K-1115 | Yeniden gönderim siparişin güncel iletişim e-postasına gider (B-15 düzeltmeden önceki adrese) | L4-2 | ⚠ | `02 §9.1.6`; `03` 7.1.37, 8.9.2; `06 §4.58` |
| K-1116 | Hesabı silinmiş siparişin e-posta düzeltmesi hesaba bağlanmaz; `IsAccountDeleted` doğruysa `CustomerId` boştur (kontrol kısıtı) | L6b-1 | öneriyle | `02 §10.4.11`; `03` 2.10.5, 6.2.4.2; `06` §4.17, §7.3 |
| K-1117 | Doğrulanan e-posta değişikliğinde Google bağı kalkar | L6b-3 | ⚠ | `02 §3.13.7`, §3.13.14; `03` 9.2.2; `04` 5.25.6; `06` §4.40, §4.36 |
| K-1118 | Hiç bağlanmamış dijital dosya yüklemesi bir günden eskiyse imhada silinir | L12-4 | öneriyle | `06` §4.70, §9.3.1, §2.7; `07` park |
| K-1119 | Kalemin dosya bağı yeni dosya bağlanırken yeni dosyaya çekilir | L6b-2 | öneriyle | `06` §4.18, §4.70 |
| K-1120 | Sonradan geri dönen e-posta `SentAt`'i korur; `Undelivered`'da `SentAt` yalnız gönderildikten sonra geri dönen satırda doludur | L3b-1 | öneriyle | `06 §4.58`, §7.3; `08` park |
| K-1121 | Büyük veri dönüşümünün bitmesi daraltma göçünün kapısıdır | L5-1, L3b-2 | öneriyle | `05 §8.7.5`; `09` park; `DEPLOY_RUNBOOK` park |
| K-1122 | Uzun metin gösterimde de süzülür; saklanan değer yeniden yazılmaz | L4-3 | ⚠ | `06 §2.10.2` |
| K-1123 | Koddaki kargo şirketi listesinde de kod adı kalıcıdır, yeniden kullanılmaz | L5-3 | öneriyle | `06 §5.3.12`, §4.17; `08` park |
| K-1124 | Kurucu kullanıcı `pg_trgm` eklentisini ilk kurulumda kurar | L8-1 | öneriyle | `05 §7.1.3`; `06` §4.5, §7.5.1, §8.5.2; `DEPLOY_RUNBOOK` park |

**⚠ toplamı (bu adımın):** 7 — K-1104, K-1105, K-1110, K-1114, K-1115, K-1117, K-1122. Yedisi de K-992'nin kuralıyla sorulmadan kaydedildi ve Aşama 5'in arşiv işaretinden önce proje sahibine tek listede gider. **Çok kritik soru çıkmadı.**

Önceki kararlara geri işaret: K-1077, K-788 (K-1104) · K-596 (K-1105) · K-791 (K-1106) · K-1076, K-1056 (K-1107) · K-1075 (K-1108) · K-1057 (K-1110) · K-1059 (K-1112) · K-1083 (K-1113) · K-903, K-1048 (K-1114) · K-759 (K-1115) · K-585 (K-1116) · K-733, K-1061 (K-1117) · K-1100 (K-1118) · K-1029 (K-1119) · K-1028, K-1066 (K-1120) · K-1026 (K-1122) · K-1095 (L12-5, yeni K yok).

### Yönetici talimatının öncelikli okumaları

- **K-1086 (L6a):** kalan risk (çift ödeme, fazla ödenenin iptalle geri ödenmesi) çok kritik değil; geçiş kolu bulgusu L6a-1 tartışmalıydı → **elendi** (gerekçe "Uygulanmayanlar"da). K-1086'nın (A)'yı eleyen öteki iki gerekçesi (K-967'nin kilidi, K-1052) ayakta.
- **K-1081 (L4):** sorun yok. Girdi tahmin edilebilir olsa da HMAC'in güvenliği girdinin gizliliğine değil sırra dayanır; her bağlantı kendi satırıdır ve `TokenHash` tekildir; sır tek başına hiçbir şey açmaz; kuyrukta bekleyen e-posta döndürmede kapalı başarısızlıkla "kayıt yok"a düşer.
- **K-1082 (L4):** kalan riskin kapsamı doğru. Halkanın koruduğu beş değerden (form güvenliği değeri, ödeme adımı çerezi, oturum çerezinin taşıdığı değer, Google `state`'i, indirme bağlantısının imzası) sahtelenebilen yalnız **indirme bağlantısı**dır; ötekiler ya kurbanın tarayıcısındaki çerez eşine bağlı ya da halkadan bağımsız. Salt okuma kullanıcısının kurulum notu `09` park satırının doğrulamasındadır.
- **K-1092 (L6a):** sorun yok. 6563 sayılı Kanun m.11/3, mevzuat.gov.tr'den 2026-10-18'de yeniden okundu — "iş veya işlemin gerçekleştiği tarihten itibaren on yıl".
- **⚠ bir kararın kendisini değiştirmeyi öneren tek bulgu L4-1 idi (K-903/K-1048).** İki şüpheci de kararın **sonucunu** doğru buldu: sipariş sayfası numara ve e-postayla da açılır, türetme ya da sütun yasağı yolu kapatmaz. K-1114 gerekçeyi tamamladı ve kalan riski adıyla yazdı. Çok kritik soru çıkmadı.

### Yasal dayanak ve teknik doğrulama günlüğü

| Dayanak | Madde / konu | Okunduğu tarih | Okuyan | URL | Sonuç |
|---|---|---|---|---|---|
| 6563 sayılı Elektronik Ticaretin Düzenlenmesi Hakkında Kanun | m.11/3 (ek cümle: 1/7/2022-7416/5 md.), m.2/1-ç | 2026-10-18 | L6a | https://www.mevzuat.gov.tr/anasayfa/MevzuatFihristDetayIframe?MevzuatTur=1&MevzuatNo=6563&MevzuatTertip=5 | Doğrulandı — "iş veya işlemin gerçekleştiği tarihten itibaren on yıl süreyle saklamakla yükümlüdür"; K-1092 tutar |
| OWASP Top 10:2025 | 10 kategori | 2026-10-18 | L4 | https://top10.owasp.org/2025 | Doğrulandı — kategori adları ve sırası güncel sürümdendir (katman 4'teki tablo) |
| PostgreSQL 18 — F.35 pg_trgm, CREATE EXTENSION | "trusted" eklenti, kuran kullanıcıda `CREATE` yetkisi | 2026-10-18 | L8; PE-a bağımsız doğruladı | https://www.postgresql.org/docs/18/pgtrgm.html · https://www.postgresql.org/docs/18/sql-createextension.html | Doğrulandı — L8-1'in teknik iddiası tutar |
| PostgreSQL 18 — açık kilitleme, kısıtlar, UUID işlevleri, kaynak kod | `FOR SHARE` çakışması, `NULLS NOT DISTINCT`, RI basamaklama, `uuidv7()` | 2026-10-18 | L8 | bkz. katman 8 tablosu | Doğrulandı |
| Npgsql EF Core | GUID anahtarı 9.0'dan itibaren sıralı (v7); `AreNullsDistinct(false)` | 2026-10-18 | L8 | https://www.npgsql.org/efcore/modeling/generated-properties.html · https://www.npgsql.org/efcore/modeling/indexes.html | Doğrulandı |
| ASP.NET Core veri koruma | anahtar tablosu, anahtarların silinmemesi, şifresiz saklama davranışı | 2026-10-18 | L8 | https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/implementation/key-management · .../key-storage-providers | Doğrulandı |

Doğrulanamayan dayanak yoktur. Bu adım yeni bir yasal kural koymadı; diğer mevzuat dayanakları (6698, Yönetmelik, Mesafeli Sözleşmeler, Fiyat Etiketi, 6102, 213) [`06_AUDIT.md`](06_AUDIT.md) "Yasal dayanak" günlüğünde 2026-10-17'de okundu ve düzeltmelerin hiçbiri bu kuralları değiştirmedi.

### Geri besleme — üst dokümanlara ve park satırlarına

Güncellenen her yer `06 §1.3`'ün geri besleme tablosundadır (tarama notu "kalite döngüsü — deep review"); karar kaydının geri besleme satırları aynı yerleri K numarasıyla sayar.

| Doküman | Sürüm | Değişen bölümler (K) |
|---|---|---|
| Ürün Gereksinimleri `02` | v0.84 → **v0.85** | §7.1.7 (K-1104) · §10.4.9 (K-1105) · §5.11.3 (K-1106) · §1.2, §10.6.1 (K-1108) · §7.4.5, §4.2 Z-35, Z-38 (K-1110) · §9.1.6 (K-1115) · §10.4.11 (K-1116) · §3.13.7, §3.13.14 (K-1117) |
| Kullanıcı Akışları `03` | v0.33 → **v0.34** | 7.1.37, 8.9.2 (K-1115) · 2.10.5, 6.2.4.2 (K-1116) · 9.2.2 (K-1117) |
| Arayüz Tanımları `04` | v0.32 → **v0.33** | 9.9.22 (K-1105) · 2.5.3.4, 2.6.2, 9.3.6 (K-1108) · 5.25.6 (K-1117) |
| Teknik Mimari `05` | v0.21 → **v0.22** | §4.3.4 (K-1111) · §8.5.10, §8.7.4 (K-1113) · §6.4.9 (K-1114) · §8.7.5 (K-1121) · §7.1.3 (K-1124) |
| MVP Kapsamı `10` | v0.54 | dokunulmadı |
| Park satırları | | `07` (K-1118) · `08` (K-1112, K-1120, K-1123) · `09` (K-1121) · `12` (K-1104) · `DEPLOY_RUNBOOK` (K-1113, K-1121, K-1124) |

`06` kendi içinde (üst doküman değişmeden): K-1106 (§4.14), K-1107 (§4.33), K-1109 (§4.31), K-1118 (§4.70, §9.3.1), K-1119 (§4.18, §4.70), K-1122 (§2.10.2; §1.3'e satır yok), K-1123 (§5.3.12, §4.17). Park satırı iki yönlü karşılaştırma temiz: yukarıdaki beş dokümanda eklenen K'lar ile karar kaydının etki sütunları birbirini tutuyor.

**Gerekçeyle elenen eşleşmeler:** geri besleme taramasının (betik 1) `01`–`05`, `10` ve park taramasında bulduğu ama değiştirilmeyen yerler genel anlatım, tarihli karar kaydı ya da kuralın evine işaret eden cümlelerdir (örneğin `04` satır 2169, `03` 1.7.3.4, `05` 576, `03` 3.2.1.13, `02` satır 51/75, `05 §7.7.3`, `05 §10.2.2`); tam listeler uygulama günlüklerindedir.

### Aksiyon Planı

- **Critical:** yok.
- **High (3 — L12-3, L3a-1, L3a-2; ikisi tek düzeltme; hepsi uygulandı):** ayıpta dönülmüş adedi olan kalemde son işlemin kalanı (L3a-1, L12-3 → **K-1104**) · yeniden açılan "mal dönmedi" kapanışında ödenmiş kısmın düşümü (L3a-2 → **K-1105**).
- **Medium (21 — hepsi bu dokümanda ya da park satırında kapandı):** `06`/üst dokümanda düzeltildi: L6a-2, L3a-5 (K-1106), L3a-6, L3a-7 (K-1107), L12-1 (K-1108), L3a-3 (K-1109), L3a-4 (K-1110), L5-2 (K-1111), L12-2 (K-1113), L4-1 (K-1114), L4-2 (K-1115), L6b-1 (K-1116), L6b-3 (K-1117), L12-4 (K-1118), L6b-2 (K-1119), L3b-1 (K-1120), L5-1 (K-1121), L4-3 (K-1122), L5-3 (K-1123), L8-1 (K-1124). Sonraki dokümanın işi olanlar park satırlarına talimat olarak girdi: `07` (K-1118) · `08` (K-1112, K-1120, K-1123) · `09` (K-1121) · `12` (K-1104) · `DEPLOY_RUNBOOK` (K-1113, K-1121, K-1124). L8-2 (K-1112) yalnız `08` park cümlesi olarak tutuldu.
- **Low (2):** L12-5 (K-1095'in özet hizalaması, uygulandı) · L3b-2 (L5-1'in `09` park cümlesiyle karşılandı).
- **İşlenmeyen (3):** L7-1 (çürütüldü) · L6a-1, L6b-4 (elendi).

### Cross-reference Haritası (bu adımın değişiklikleri)

`06` bölümleri `git diff -U0` ve uygulama günlüklerinden toplandı. "Üst doküman yüzü" sütunları geri beslenen maddedir; "—" o dokümana dokunulmadığını söyler. `10` hiçbir satırda dokunulmadı. Durum: **hizalandı** = üst doküman bu adımda güncellendi (v0.85 · v0.34 · v0.33 · v0.22); **`06` içi** = üst doküman değişmedi.

| Hedef Bölüm (`06`) | 02 | 03 | 04 | 05 | 10 | Durum |
|---|---|---|---|---|---|---|
| §2.7 (K-1104 — §2.7.2; K-1118 — envanter satırı) | §7.1.7 (4) | — | — | — | — | hizalandı |
| §2.6.4 (K-1119 — sürümü artırmayan yazımlar listesi) | — | — | — | — | — | `06` içi |
| §2.10.2 (K-1122) | — | — | — | §6.5.3 (hedef yön, değişmedi) | — | `06` içi |
| §4.14 (K-1106) | §5.11.3 | — | — | — | — | hizalandı |
| §4.17 (K-1114, K-1116, K-1123, K-1104) | §10.4.11 (5) | 2.10.5, 6.2.4.2 | — | §6.4.9 | — | hizalandı |
| §4.18 (K-1104, K-1119) | §7.1.7 (4) | — | — | — | — | hizalandı (K-1119 `06` içi) |
| §4.22 (K-1104) | §7.1.7 (4) | — | — | — | — | hizalandı |
| §4.28 (K-1105) | §10.4.9 | — | 9.9.22 | — | — | hizalandı |
| §4.31 (K-1109, K-1110) | §7.4.5, §4.2 Z-35, Z-38 | — | — | — | — | hizalandı (K-1109 `06` içi) |
| §4.33 (K-1104, K-1105, K-1107) | §7.1.7 (4), §10.4.9 | — | 9.9.22 | — | — | hizalandı (K-1107 `06` içi) |
| §4.36, §4.40 (K-1117) | §3.13.7, §3.13.14 | 9.2.2 | 5.25.6 | — | — | hizalandı |
| §4.42 (K-1113) | — | — | — | §8.5.10, §8.7.4 | — | hizalandı |
| §4.5, §7.5.1, §8.5.2 (K-1124) | — | — | — | §7.1.3 | — | hizalandı |
| §4.58 (K-1115, K-1120) | §9.1.6 | 7.1.37, 8.9.2 | — | — | — | hizalandı (K-1120 `06` içi + `08` park) |
| §4.59 (K-1111) | — | — | — | §4.3.4 | — | hizalandı |
| §4.70 (K-1118, K-1119) | — | — | — | §7.7.3 (değişmedi) | — | `06` içi + `07` park |
| §5.3.12 (K-1123) | §3.20.7–§3.20.8 (değişmedi) | — | — | — | — | `06` içi + `08` park |
| §6, §6.1, §6.3.3 (K-1105 — yeni bağ) | — | — | — | — | — | `06` içi |
| §7.2, §7.3 (K-1105, K-1116, K-1120) | §10.4.9, §10.4.11 | — | — | — | — | hizalandı |
| §7.4 (L12-5, K-1095) | §10.4.7 | — | — | — | — | `06` içi |
| §7.5 (K-1124, K-1105) | — | — | — | §7.1.3 | — | hizalandı |
| §8.5 (K-1124) | — | — | — | §7.1.3 | — | hizalandı |
| §9.1 Z-35, Z-38 (K-1109, K-1110) | §4.2 Z-35, Z-38 | — | — | — | — | hizalandı |
| §9.3 (K-1118 — §9.3.1 `DigitalFile` satırı, Kaynak) | — | — | — | §7.7.3 | — | `06` içi + `07` park |
| §9.4 (K-1109, K-1110 — Kaynak satırı) | §4.2 Z-35, Z-38 | — | — | — | — | hizalandı |
| §9.5.2 (K-1113) | — | — | — | §8.5.10, §8.7.4 | — | hizalandı |
| §1.3 (geri besleme tablosu) | v0.85 | v0.34 | v0.33 | v0.22 | v0.54 (dokunulmadı) | hizalandı |

### Mekanik denetimler (kapanış)

- **`verify67`: 130 / 130 / 130** (§4'ten toplanan bağ · §7.2 · §6) — tek yeni bağ `Refund.NotReturnedClosureId` (129 → 130; kısıtlayıcı 74 → 75); eksik, fazla ya da fark yok. Yeni bağın dizini "kendi dizini"dir; §7.2.1 kuralı gereği §7.5'te yinelenmez.
- **Atıf betiği:** çözülmeyen `§` atfı 457 → 457, alan/enum atfı eksik 1 → 1 (bilinçli `SocialLink.Platform` — K-1101 eski adı), kayıtta olmayan K yalnız bilinçli `K-1999` (betiğin sınama enjeksiyonu). Yeni çözülmeyen `§` ya da alan atfı yok.
- **Matris kümeleri** (1a 690 · 1b 345 · 1c 297 · 1d 173 · aday 73 · geri izlenebilirlik 2.915 çift) `origin/main` ile satır satır aynı (`cmp`).
- **Park satırları iki yönlü temiz:** `07` (K-1118) · `08` (K-1112, K-1120, K-1123) · `09` (K-1121) · `12` (K-1104) · `DEPLOY_RUNBOOK` (K-1113, K-1121, K-1124) — eklenen her K'nın karar kaydı etkisi o dokümanı sayıyor, ters yönde de.
- **K tekilliği:** K-1104…K-1124 karar kaydında bir kez; yinelenen yok; yer tutucu (`KTMP`) kalan yok.
- **Kontrol karakteri ve CR:** 0 (değişen bütün dokümanlarda).

### Uygulanmayanlar

**Hiçbir kolu uygulanmayan 4 bulgu:**

| Bulgu | Durum | Gerekçe |
|---|---|---|
| L7-1 | iki şüpheci çürüttü | Olgu doğru (sipariş satırı silinmez — §4.17 Silme davranışı, §9.1 Z-41, §9.1.4) ama doğru bilgi dokümanda üç yerde kendi evinde yazılı; §7.6.1'in "en uzun yaşayan" sözü yanlış bir değer, alan ya da atıf taşımıyor, sınırsız yaşayanla da tutarlı — eşiğin altında (savunmada derinlik / üslup) |
| L6a-1 | tartışmalı → elendi (yönetici) | PB-b'nin çürütmesi tutar: geçiş kolunda `02 §3.16.12`, §3.17.7'nin koşulunu ("birleşip silinmişse") açıkça sağlamaz, temel kural işler — uygulamacı için belirsizlik yok; bulgunun zararı (paylaşılan tarayıcıda başkasının ödenmemiş siparişinin sonraki onayla iptali) tarayıcıya bağlı misafir sepetinin kendisinde zaten vardır (`02 §3.16.1`; K-1044 (1)) — geçiş kolu yeni bir yol açmaz; önerilen istisna (`IsGuestOrder`) aynı kişinin bugün doğru işleyen yolunu bozar ve K-1086'nın çift ödeme kalan riskini yeni bir kola yayar. Cross-review'a not |
| L6b-4 | tartışmalı → elendi (yönetici) | Zarar yalnız posta kutusunun sahibi olduğunu kanıtlamış kişinin kayıt formunu yeniden doldurmasıdır; eşiğin altında (PC-b) |
| L3b-2 | tartışmalı → L5-1'in `09` park cümlesiyle karşılandı | PD-a: iş bugün yok; PD-b: yalnız `09` park satırına yan cümle |

**Kısmen uygulananların uygulanmayan kolları** (karar kaydı §14.1'in "Deep review'ın uygulanmayan bulguları" maddesinde de durur):

| Bulgu | Uygulanmayan kol | Gerekçe |
|---|---|---|
| L12-3 | kapanış kolu ("dönülmüş adedi kapanmış say") | K-1077'nin elediği seçenek; "kapanmış sayılır" sözü "kalemi kapatmaz"la çelişir. Çekirdek L3a-1 ile K-1104'te uygulandı |
| L6a-2, L3a-5 | caymada hizmet kontenjanının beyanın işleminde döndüğü kol (`02 §7.4.8`) | PA-a: genel sayaç kuralı karşılar; K-1106 stok ve kupon yüzünü yazdı |
| L3a-4 | (a) kolu — kalemde IBAN dururken yeni geri ödeme o IBAN'ı kullanır | K-690 cevaplar; yalnız (b) kolu uygulandı (K-1110). `12` park satırı da yazılmadı |
| L4-3 | yeniden temizleme göçü kolu; `05`/`09` değişikliği | Yayımlanmış yasal metin sürümü yalnız eklenir (`02 §3.33.3`); gösterimde süzme `05 §6.5.3`'ün mekanizması (K-1122) |

Ayrıca uygulanmayan seçenekler (gerekçeleriyle karar kaydında): L4-1'in (a) türetme / (b) sütun yasağı önerileri (numara + e-posta yolu anahtarsız açık; K-1114) · L4-2'nin birinci önerisi (K-675'i değiştirir) · L12-2'nin ikinci yüzü (eşiğin altı) · L3b-1'in kanal hâli kaydı kolu · L6b-3'ün "ikinci hesap" kolu · L5-1'in `06 §2.9.3`/§4.59 önerisi (`06` kuralın yüzünü doğru söylüyor).

### Cross-review için notlar

- **Boyut:** `06` v0.18 **6.011 satır, ~1,13 MB (1.158.117 bayt)** — `cross-review` skill'inin 400 KB eşiğinin çok üstünde. Teknik Mimari'nin kalıbı: her turda **tek parça çağrı** (parçalar arası çelişkiyi yalnız o görür) + **parçalı kontrol koşumu** (K-953). Önerilen parçalama (v0.18 satır numaraları; her parçaya başlık notu ~45 KB ve şablon eklenir; parça sınırı alt bölüm başlığıdır ve turlar boyunca değişmez):

  | Parça | Bölümler | Satır | Boyut |
  |---|---|---|---|
  | 1 | §1.1.1–§1.1.4 | 100–723 | ~99 KB |
  | 2 | §1.1.5–§1.1.8 | 724–1748 | ~189 KB |
  | 3 | §1.1.9–§1.3 | 1749–2592 | ~246 KB |
  | 4 | §2–§3 | 2593–2967 | ~69 KB |
  | 5 | §4.1–§4.35 | 2968–3877 | ~199 KB |
  | 6 | §4.36–§4.72 | 3878–4648 | ~147 KB |
  | 7 | §5–§9 | 4649–6011 | ~135 KB |

- **Bu adımda en çok değişen yerler (öncelikli okuma):** §4.33 (`NotReturnedClosureId`, kargo ücretinin seçimi, bekleyen geri ödeme) · §4.31 (IBAN'ın silinme anları) · §4.18 (işleme açık son adetler, dosya bağı) · §4.14 (sayaç dönüşü) · §4.58 (yeniden gönderimin alıcısı, sonradan geri dönen e-posta) · §4.17 (`CustomerId` kontrol kısıtı, erişim anahtarının kalan riski, `Carrier`) · §4.40 ve §4.36 (Google bağı) · §4.70 (hiç bağlanmamış dosya) · §2.10.2 · §9.5.2 ve §4.42 (geri alma bağlantısı istisnası). Üst dokümanlarda: `02 §7.1.7`, §10.4.9, §7.4.5, §9.1.6, §3.13.7; `05 §6.4.9`, §8.5.10, §8.7.4, §8.7.5, §4.3.4, §7.1.3.
- **İkinci modelin yeniden bulgu yapabileceği elenen yollar** (karar kaydı §14.1): **L6a-1** — K-1086'nın geçiş kolu (gerekçe yukarıda; ikinci model aynı kolu yeniden açarsa K-967'nin kilidini, K-1052'yi ve K-1044 (1)'in paylaşılan tarayıcı zararını birlikte okumalı). Ayrıca L6b-4 (yönetici e-posta değişikliğinin müşteri bekleyen kaydını silmesi) ve L4-1'in türetme önerisi (K-1114'ün gerekçesi: sipariş sayfası anahtarsız da açılır).
- **K-1114 ve K-1115 kalan riskleri** adıyla yazılıdır; ikinci modelin bunları "bulunmamış sorun" gibi yeniden raporlaması muhtemel — yanıt K-1114'ün gerekçesinde (`05 §6.4.9`, `06 §4.17`).
- **Etki yansıtma** bu adımın geri beslediği `02` v0.85, `03` v0.34, `04` v0.33 ve `05` v0.22'yi yeniden tarar; park satırlarının etiketleri ile karar kayıtlarının etki sütunları iki yönde eşleşir.
