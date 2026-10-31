# Shopfolio — Implementation Plan

**Versiyon: v0.21** | **Bağımlılıklar:** `02`–`10` | **Son güncelleme:** 2026-10-31

> **Aşama:** 9 — Implementation Planı · **Rol:** Delivery Lead
> **Traceability zorunlu: EVET** — §7 tamamlanmadan §5'e (task listesi) geçilmez.
> **Not:** Bu doküman **tüketici**dir — `02`–`10`'u tüketir, onlara geriye etki yaratmaz. Buradaki düzeltmeler yalnız bu dokümanı hizalar.

> **Aşama 8'den park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §20.1, yazım turu Y-6b).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-1302 (4) · K-1357 · Okunacak `09` bölümleri:** Kodlama Kılavuzu (`09`) 100 KB'ı aştığı için her görevde bütün okunmaz (`INSTRUCTIONS §9` (7)); `00 §C.6` ("her kodlama görevinde") değişmedi, okunma biçimi bölüm bölümdür. Bu doküman her task satırında okunacak `09` bölümlerini yazar: **çekirdek yedi bölüm** (§1, §2, §5, §6, §23, §24, §25 — toplamı 100 KB'ın altındadır) her satırda "çekirdek" diye anılır ve ayrıca sayılmaz; çekirdeğe **ek** bölümler task'ın dokunduğu alana göre `09` başlık notunun **okuma tablosundan** alınır (ör. göç yazan task §10 ve §12; dış servis çağıran task §11 ve §15; C# yazan her task §3.1 ve §3.2). Task satırının kabul kriterleri ilgili bölümlerin Test maddelerine ve §25'in ilgili satırlarına dayanır; yapım PR'ı okunan bölümleri açıklamasında yazar (`09 §21.3`, `§24` madde 1).
> - **K-1356 · D-01 — eşitlik denetimlerinin Lint adımına girmesi:** `09 §21.4` madde 1'in beş eşitlik denetimi (kütüphane tablosu, parametre envanteri, hata kodu envanteri, geçiş listesi, işaretli işlem listesi) `UnitTests/Consistency/` klasöründe `Consistency` özelliğiyle işaretli birim testleridir; Lint adımı onları koşar, Unit adımı dışarıda bırakır. Özelliğe göre süzen komut D-01'in işidir ve `.github/ci-commands.env`'e yazılır (`05 §2.3.5`); komutların değerini `09` ikinci kez yazmaz.
> - **K-1356 · D-01 — yama sürümü denetiminin `publish.yml`'e girmesi:** "Yama sürümü göç taşımaz" denetimi yayın akışında, etiket imaja basılmadan önce koşar (`09 §21.4` madde 2; `§10.6` madde 7); PR'da koşmaz, çünkü PR sürüm etiketini bilmez. İstisna — veri bozan bir göçün düzeltmesi — etiket mesajındaki `migration-fix: K-<numara>` satırıdır. İlk yayından önce son yayımlanan sürüm bulunmadığından geri alma sınaması atlanır ve atlandığı CI çıktısında yazılır (`09 §21.4` madde 3).
> - **K-1355 · Test projeleri — sahtelerin paylaşımı:** `UnitTests/Fakes/` altındaki sahteleri `IntegrationTests` bağlantılı dosya olarak derler (ikinci kopya yok, iki projeyi bağlayan proje başvurusu yok); çalıştırılabilir bir test projesine başvuru xUnit v3'te sorun çıkarabilir (bilgim tarihli) — test altyapısını kuran ilk task'ta sınanır (`09 §19.1`).
> - **K-1354 · Yükleme betiği, kayıt biçimleyicisi ve maskeleme testleri:** Kodlama Kılavuzu (`09`) üç yapım işi bırakır ve her birinin bu planda bir task karşılığı bulunur: (1) parçalı yükleme betiği — başlatma–parça–durum akışını, 409/404/400/413 yanıtlarını ve ağ hatasında üç kez sürdürmeyi kapsar (`09 §16.2`; `07 §4.31.2`). (2) Uygulama kayıtlarının üç yazıcısı ve satır biçimleyicisi `ApplicationLogFormatter` — alan adlarını ve kapalı olay listesini (`LogEvents`) kapsar (`09 §17`, `§18.1`, `§18.2`; `05 §9.1`). (3) Maskelemenin üç katmanının testleri: yazım, `PathMasker`, biçimleyici ve `ExceptionMasker` (`09 §18.2`). Bu işler izlenebilirlik matrisinin (§7.1) kaynak öğelerinde adıyla geçmeyebilir; task'ın kaynağı `09` bölümüdür. Task'ın nasıl bölünüceğine bu aşama karar verir.

**Aşama 8 park bloğunun durumu (K-1443).** Yukarıdaki beş madde talimattır; bu tablo her birinin task karşılığını ve yazan oturumu söyler. Blok yazım turu bitene kadar yerinde kalır; son oturum (Y8) her satırın karşılığını task'ta bulduğunu doğrular ve durum sütununu kapatır.

| Park maddesi | Karşılığı | Yazan oturum | Durum |
|---|---|---|---|
| P8.1 — okunacak `09` bölümleri (K-1302 (4), K-1357) | Okuma biçimi: bu başlık notunun okuma tablosu ve §5 "Task yapısı"nın `Dokümanlar` kuralı; her task'ın alanı kendi bloğunda | Y1 (biçim) · her task'ı yazan oturum (alan) | Biçim ✓ — alanlar task'larla |
| P8.2 — eşitlik denetimlerinin Lint adımına girmesi (K-1356) | T04 (komutlar `.github/ci-commands.env`) | Y2 | Task yazıldı ✓ — Y8 doğrular |
| P8.3 — yama sürümü denetiminin `publish.yml`'e girmesi (K-1356, K-1358) | T04, T09 | Y2 | Task yazıldı ✓ — Y8 doğrular |
| P8.4 — sahtelerin paylaşımı, xUnit v3 sınaması (K-1355) | T03 | Y2 | Task yazıldı ✓ — Y8 doğrular |
| P8.5 — yükleme betiği, kayıt biçimleyicisi, maskeleme testleri (K-1354, K-1391) | T230 (üç yazıcı ve biçimleyici), T231 (maskeleme testleri), T548 (yükleme betiği) | Y3 (T230, T231) · Y6c (T548) | T230, T231 yazıldı ✓ — T548 Y6c'de; Y8 doğrular |

**Yazım konvansiyonları (K-1443).** (1) **Sıra:** W-1 ✓ → başlık notu ve §3, §3.1, §4, §5'in "Task yapısı"nı (Y1) → matris ✓ → §5 task blokları fazlar halinde (aşağıdaki harita) → §6, §8, §9 → §1, §2 son (sayımlar bloklar bitince kesinleşir) → özet ↔ ayrıntı taraması → kalite döngüsü. (2) **Dil:** bu doküman Türkçe; kod, sembol ve dosya adları İngilizce. (3) **Kimlikler (K-1406 (3)):** konu `IPn-mm`; task `TXX`, araya `TXXa`/`TXXb`, devam turu `TXX.1` (`00 §J`); matris kaynak kimliği ailesine göre (§7'nin "Kaynak kimliğinin biçimi" tablosu). (4) **Atıf biçimi:** `02 §3.8.3`, `07 UC-12`, `09 §10`, `05 MK-12` — önsüz `§` bu dokümanın bölümüdür; başka doküman her zaman numarasıyla anılır. (5) **Tüketici ilkesi:** kaynakla çelişki çıkarsa kaynak kazanır, fark bu dokümana hizalama olarak yazılır; kaynağın kendisi yanlışsa §8'e GAP olarak girer (K-1406 (2)(d)). (6) **Kural ayrıntısı evinde kalır:** task bloğu kaynağın kuralını kopyalamaz, kimlikle işaret eder. (7) **Açık kararlar bölümü yoktur:** açık kalem karar kaydının §4'ündedir; sonraki dokümana talimat o dokümanın park bloğundadır (PF-143).

**Okuma tablosu ve boyut (K-1406 (4), K-1443).** Bu doküman 100 KB'ı çoktan aştı ve `INSTRUCTIONS §9` (7) bütün okumayı yasaklar; `task` ve `validate` skill'leri "hedef task'ın tanımını" bu tabloyla okur. Komutlar depo kökünden, dosya `Docs/11_IMPLEMENTATION_PLAN.md`:

- **Bölüm başlıkları:** `grep -n '^## ' <dosya>` — satır numaraları; bölüm aralıkla okunur.
- **Faz başlığı ve yürütme sırası:** `grep -n -A 3 '^### F3 ' <dosya>` — fazın adı ve `**Yürütme sırası:**` satırı.
- **Task bloğu:** `grep -n -A 30 '^#### T123: ' <dosya>` — tek blok (en çok 2,5 KB, yaklaşık 30 satır; sonraki `####` başlığında biter).
- **Aday tablo ve geri yön satırı:** `grep -n '^| T123 |' <dosya>` — iki satır: §7 aday tablosu (kısa ad, kaynak birimi) ve §7.2 (kaynak kimlikleri).
- **Kaynak satırları (ileri yön):** `grep -nE '(\| |, )T123(,| \|)' <dosya>` — her satırın ilk hücresi kaynak kimliğidir; kimlikten kaynak dokümanın aralığı bulunur.
- **Matris alt bölümü:** `grep -n '^#### 7\.1\.' <dosya>` — 7.1.1…7.1.5 başlık satırları.
- **Faz ek kontrolü:** `grep -n '^| F3 |' <dosya>` — §3 faz satırı ve §6.2 satırı.
- **Kaynak doküman aralığı:** `grep -n '<kimliğin başlığı>' Docs/0N_*.md`, sonra `sed -n 'a,bp'` — yalnız task'ın kaynak birimleri.

Bir task okunurken yalnız şunlar okunur: kendi bloğu, aday tablo ve §7.2 satırı, kaynak satırlarının kimlikleri, ilgili §6.2 satırı, kaynak dokümanın o birimleri ve `09` için çekirdek yedi bölüm artı `Dokümanlar` alanının saydığı ek bölümler (`09` başlık notunun okuma tablosu). **Boyut hedefi:** bütün dosya 1,5 MB'ın altında; task bloğu en çok 2,5 KB; matris satırı en çok 500 bayt. Sınır aşılırsa karar kaydında konu açılır; doküman bölünmez (dosya adı ve bölüm numaraları değişmez). Blok boyutu ve alan eksiksizliği `scripts/doc-scans/11_task_blok_denetle.py` ile, matris kapsamı `11_matris_kapsama.py` ile denetlenir.

**Yazım turunun haritası (K-1443).** Yazım oturumları on üç tanedir (Y1…Y8; F3 dörde, F5 üçe bölünür); harita burada sabittir, değişirse tablo ve ona atıf yapan satırlar aynı PR'da güncellenir. **Bölüm çizgisi kimlik sırasıyla çekilir:** F3 dört parçadır — `T301`–`T322`, `T323`–`T345`, `T346`–`T367`, `T368`–`T390` (22 + 23 + 22 + 23 task); `T345`/`T346` çizgisi modül sınırındadır (Üyelik modülünün son task'ı ile Sepet ve sipariş modülünün sıradaki bloğu arasında), öteki iki çizgi boyuta göredir: Y2 30, Y3 33 blokta 250k bağlam sınırına ulaştığı için 45'lik yarı bir oturuma sığmaz (K-1448); F5 üç parçadır — `T501`–`T516`, `T517`–`T532`, `T533`–`T549` (16 + 16 + 17 task); çizgiler boyuta göredir, çünkü F2, F3 ve F4 oturumları 20–33 blokta 250k bağlam sınırına ulaştığı için 24–25'lik yarı bir oturuma sığmaz; son parça ortak bileşenleri ve yükleme betiğini taşır ve F5 `Yürütme sırası`nı yazar (K-1451). Bloklar fazın içinde kimlik sırasıyla yazılır; yürütme sırasını bloğun `Bağımlılık` alanı ve fazın `Yürütme sırası` satırı verir (§5, "Task yapısı"). Task'ın kimliği yarısını belirler; ortak bileşen ya da sonraki yarıdaki task'a bağımlılık kimlikle yazılır, o blok henüz yazılmamış olsa da aday tabloda tanımlıdır.

| Oturum | Kapsam | Task | Konu | Durum |
|---|---|---|---|---|
| Y1 | Başlık notu, §3, §3.1, §4, §5 "Task yapısı" | — | `IP0-05`, `IP3-01`/`IP3-02` metni, `IP4-01`, `IP5-03` | ✓ |
| Y2 | §5 F0 ve F1 | `T01`–`T10`, `T101`–`T120` (30) | `IP5-04`, `IP5-05`, `IP5-06` | ✓ |
| Y3 | §5 F2 | `T201`–`T233` (33) | `IP5-07`, `IP5-08` (F2 payı) | ✓ |
| Y4a | §5 F3 birinci çeyrek (F3 başlığı dahil) | `T301`–`T322` (22) | `IP5-09` | ✓ |
| Y4b | §5 F3 ikinci çeyrek | `T323`–`T345` (23) | `IP5-09`, `IP5-13` | ✓ |
| Y4c | §5 F3 üçüncü çeyrek | `T346`–`T367` (22) | `IP5-09`, `IP5-13` | ✓ |
| Y4d | §5 F3 son çeyrek ve F3 `Yürütme sırası` | `T368`–`T390` (23) | `IP5-09`, `IP5-13` | ✓ |
| Y5 | §5 F4 | `T401`–`T410` (10) | `IP5-10` | ✓ |
| Y6a | §5 F5 başlığı ve birinci üçte bir | `T501`–`T516` (16) | `IP5-11` | ✓ |
| Y6b | §5 F5 ikinci üçte bir | `T517`–`T532` (16) | `IP5-11` | ✓ |
| Y6c | §5 F5 son üçte bir ve F5 `Yürütme sırası` | `T533`–`T549` (17) | `IP5-11`, `IP5-08` (T548) | ⏳ |
| Y7 | §5 F6, §6.1, §6.2 | `T601`–`T602` (2) | `IP5-12`, `IP6-01`, `IP6-02` | ⏳ |
| Y8 | §8 son hali, §9, §1, §2 (sayımlar kesin) | — | `IP8-01`, `IP9-01`, `IP1-01`, `IP2-01` | ⏳ |

Task toplamı 30 + 33 + 90 + 10 + 49 + 2 = 214 (aday tablo; sayım `11_task_blok_denetle.py` çıktısındadır). Yedi faz biter bitmez `11_task_blok_denetle.py --tam` sorunsuz koşar.

---

## 1. Amaç ve kapsam

> **Ne yazılır:** Tüm işi küçük, bağımsız, sıralı task'lara bölmek. Her task'ın ne olduğu, neye bağımlı olduğu, hangi dokümanlarla yapılacağı ve nasıl kabul edileceği.

## 2. Kaynak envanteri özeti

> **Ne yazılır:** Her kaynak dokümandan çıkarılan öğe sayısı. Traceability matrisinin denetlenebilirliği bu sayılara dayanır.

| Doküman | Öğe sayısı | Kapsam |
|---|---|---|

## 3. Faz tanımları

> **Ne yazılır:** Fazlar ve task aralıkları. Faz sayısı projeye göre değişir; **şekil sabittir**: iskelet → veri → çekirdek servisler → iş mantığı → entegrasyonlar → arayüz → uçtan uca doğrulama.

Yedi faz sabittir (K-1407 (1)); faz sayısı artırılmaz ya da azaltılmaz. Task aralıkları yüzlük bloklardır, arada `TXXa` girer (`00 §J`; K-1407 (2)). Task sayıları §7 aday task tablosundandır (214 task; sayım `scripts/doc-scans/11_task_blok_denetle.py` çıktısında).

| Faz | Ad | Kapsam | Task aralığı |
|---|---|---|---|
| F0 | Proje iskeleti | Çözüm ve proje düzeni (D-01 dahil), test altyapısı, CI hattı ve eşitlik denetimleri, Compose geliştirme ortamı, üç imaj, yedek ve sır düzeni, veritabanı kullanıcıları, canlılık ucu, ilk yayın. **Çıkış ürünü:** boş uygulama imajı canlılık ucuna yanıt verir, ilk yayın yapılmıştır; referans kuruluma ilk iniş F0'ın sonundadır | `T01`–`T10` (10) |
| F1 | Veri katmanı | Veri modeli temeli ve enum'lar, 79 entity on altı göçte (en çok 8 entity, tek modül), başlangıç verisi, model eşitlik denetimleri. **Çıkış ürünü:** göçler ileri, idempotent ve geri alınabilir; eşitlik denetimleri yeşil; referans kuruluma göç | `T101`–`T120` (20) |
| F2 | Çekirdek servisler | Ortak altyapı (`05 §3.3.11`): modül iskeletleri ve Contracts, kimlik şemaları ve sayfa–form kalıbı, iş kuyruğu ve zamanlayıcı, dosya deposu, kayıt ve maskeleme, ayar katmanları, veri koruma halkası, dış servislerin bağlantı katmanı arayüzleri ve sahteleri (gerçek bağlayıcılar F4'tedir). **Çıkış ürünü:** modül sınırı mimari testle korunur, kayıt maskeleme testleri geçer | `T201`–`T233` (33) |
| F3 | İş mantığı | Dokuz modülün iş mantığı: sipariş ve ödeme durum makinesi (`02 §5.4`, §5.5), stok, kupon, iptal–cayma–iade, zaman kuralları, bildirim kararları, yönetim komutları, sayfasız uçlar. **Çıkış ürünü:** ürün → sepet → sipariş → `FakePaymentGateway` sonucu → kargo → teslim akışı servis düzeyinde entegrasyon testiyle geçer (K-1410 (2)) | `T301`–`T390` (90) |
| F4 | Entegrasyonlar | Gerçek bağlayıcılar: PayTR (başlatma, bildirim, sorgu, iade, 3D Secure), SES (gönderim, geri dönen adresler), Google girişi; dış olgu varsayımlarının doğrulama kanıtları. **Çıkış ürünü:** üç servisin sandbox kanıtı, imza doğrulaması, DV-1, DV-2, DV-9 | `T401`–`T410` (10) |
| F5 | Kullanıcı arayüzü | Ortak bileşenler (`OB-01`…`OB-20`), ekranlar (`E-01`…`E-53`), sayfa işleyicileri, betiksiz yol, parçalı yükleme betiği. **Çıkış ürünü:** betiksiz yol çalışır, erişilebilirlik (A seviyesi) taraması geçer, referans kuruluma iniş | `T501`–`T549` (49) |
| F6 | Uçtan uca doğrulama | `DEPLOY_RUNBOOK §I` provası ve güncelleme komutu; canlı kanıt ve dağıtım; `12` senaryolarına bağlanış (`12` henüz yazılmadı — bağlanış ileri işaret olarak `IP5-12`'de yazılır). **Çıkış ürünü:** gerçek yapılandırmayla açılış provası ve canlı ödeme kanıtı (K-1411) | `T601`–`T602` (2) |

Faz başına kapı ölçütleri §6.2'dedir (`IP6-01`, K-1411).

### 3.1 Faz bağımlılıkları

> **Ne yazılır:** Zincir ve varsa kontrollü paralellik istisnaları.

**Zincir (K-1408 (1)).** F0 → F1 → F2 → F3 → F4 → F5 → F6. Faz gate'i PASS almadan sonraki faza geçilmez (`00 §E.1`); fazlar arasında örtüşme yoktur. Bu yüzden task'ın `Bağımlılık` alanı yalnız **aynı fazdaki** task'ları yazar; önceki fazların tamamı gate ile karşılanır. Her fazın sonunda, gate'ten önce, yeni sürüm referans kuruluma iner (`05 §8.1.4`; K-1407 (4)).

**Faz içi kontrollü paralellik (K-1408 (2)).** `Bağımlılık` alanı ayrık olan ve farklı modül klasörüne dokunan task'lar eşzamanlı yapılabilir. Ortak dosyalara dokunan task'lar sıralıdır; liste kapalıdır: göç klasörü ve model anlık görüntüsü, `Directory.Packages.props`, `Program.cs`, `.github/ci-commands.env`. Sıralılık blokta yazılıdır: bu dosyalardan birine dokunan task'lar, aynı fazda, `Bağımlılık` zinciriyle birbirine bağlanır (K-1444 (2)).

**Faz içi yürütme sırası (K-1410 (1), K-1413 (4), K-1444 (2)).** Faz içi sıra modül akış sırasıdır (Katalog → Sepet ve sipariş → Ödeme → İptal, cayma ve iade → Üyelik → …; `03 §2`) ve `Bağımlılık` zincirini bozmaz; F1'de bir göç, bağlandığı tabloların göçünden sonra gelir. Bloklar fazın içinde kimlik sırasıyla yazıldığından yürütme sırası her fazın başlığı altındaki `**Yürütme sırası:**` satırında kimliklerle yazılır; satırı fazı tamamlayan oturum yazar ve `11_task_blok_denetle.py` her bağımlılığın kendinden önce geldiğini denetler.

**Dış ön koşullar task değildir (K-1408 (3)).** OK-1, OK-2, OK-3, OK-4, OK-6 ve OK-12'nin kurulumu yapanın işidir ve F0'da başlar (SES üretim erişimi ile PayTR sözleşmesi gün ya da hafta sürer); F4 ön koşullarının hazır olduğu F3 gate'inde sorulur (`IP6-01`, K-1411).

## 4. Hata sınıflandırması ve çözüm akışı

| Seviye | Tanım | Tespit |
|---|---|---|
| **S1 — Sapma** | Task tamamlandı ama dokümanla uyumsuz | Task doğrulama |
| **S2 — Kırılma** | Bir task başka bir task'ın çıktısını bozuyor | Regresyon (gate check) |
| **S3 — Eksik** | Eşlenmiş öğe implement edilmemiş | Boşluk taraması (gate check) |

**Kritik kural:** Düzeltme task'ı bir sonraki faza ertelenmez — hatanın oluştuğu fazda çözülür.

**Düzeltme task'ının numarası (K-1444 (3)).** (a) Task'ın PR'ı merge edilmeden bulunan S1 aynı PR'da düzeltilir; yeni numara açılmaz. (b) Merge'den sonra bulunan S1, S2 ve S3'ün düzeltmesi `TXXa`, `TXXb` numarasını alır (`00 §J`): `XX` düzeltmenin dayandığı task'tır, faz aralığı bozulmaz, blok §5'te o fazın başlığı altına kimlik sırasıyla eklenir ve §7 aday tablosuna satır olur. (c) Aynı task'ın devam turu (sonradan eklenen savunma katmanı, kapanış işi) `TXX.1`'dir. (d) S3 bir kaynak öğenin hiçbir task'a bağlı olmamasından çıkarsa düzeltme yeni bir `TXXa`, matris satırı (§7) ve `11_matris_kapsama.py` koşusudur. Çözüm akışı (kayıt → etki analizi → karar → güncelleme) `00 §D.8`'dedir; burada tekrarlanmaz.

## 5. Task listesi

> **Kapı:** §7 tamamdır (ileri yön 1.501/1.501, §8'de açık boşluk yok). Aday task'ların kimliği, fazı, kısa adı ve kaynak birimi §7'nin aday task tablosundadır; bu bölüm yalnız task'ın gövdesini yazar. Yazım oturumları ve sırası başlık notunun yazım haritasındadır; bir fazın başlığı (`### Fn — <ad>`) o fazın ilk oturumu gelince eklenir.

### Task yapısı

Her task bir `####` bloğudur; alanlar şu sırayla ve birer kez yazılır (K-1406 (5), K-1445):

```
#### TXX: <task adı>
- **Bağımlılık:** T01, T02 (ya da —)
- **Dokümanlar:** çekirdek; `09` §10, §12; `02 §3.8.3`; `07 UC-12`; `06` Order
- **Kabul kriterleri:**
  1. <tek gözlenebilir sonuç> (`02 §3.8.3`)
  2. <...>
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: <neye bakılır>
  - `09 §25` <ilgili satır>; güvenlik taraması
```

**Konvansiyonlar (K-1445).**

1. **Yerleşim.** Faz başlığı `### F0 — <faz adı>` (ad §3'tendir); bloklar fazın içinde kimlik sırasıyla yazılır; başlığın hemen altındaki `**Yürütme sırası:** T01 → T02 → …` satırı fazı tamamlayan oturumun işidir (§3.1).
2. **Başlık.** Kimlik ve ad §7 aday tablosundandır (ad kısaltılabilir, kapsam değişmez). Kapsam yazımda değişirse §7 aynı PR'da hizalanır; yeni task gerekirse `TXXa`, aday tablo satırı ve matris satırı eklenir ve `11_matris_kapsama.py` koşar.
3. **Bağımlılık.** Yalnız aynı fazdaki task'lar, kimlikle (§3.1); bağımlılığı olmayan task `—` yazar. Ortak dosyaya dokunan task'lar zincirlenir (§3.1).
4. **Dokümanlar.** `09` için `çekirdek` (çekirdek yedi bölüm — `§1`, `§2`, `§5`, `§6`, `§23`, `§24`, `§25` — her satırda anılır, ayrıca sayılmaz) ve ek bölümler `09` başlık notunun okuma tablosundan alınır (`çekirdek; 09 §10, §12`); öteki girdiler kaynak kimliğiyle (`02 §3.8.3`, `07 UC-12`, `06` entity adı, `05 MK-12`) ve yalnız task'ın kaynak birimleri için yazılır; ek bölümler ile kaynak toplamı en çok 150 KB (K-1409). Task'ı yapan PR, okunan `09` bölümlerini açıklamasında yazar (`09 §21.3`, `§24` madde 1).
5. **Kabul kriterleri.** En çok 10 numaralı madde (K-1409). Madde kaynak satırın cümlesinden türer; kural ayrıntısı **kopyalanmaz**, parantez içinde kimlikle işaret edilir (`(02 §3.8.3)`). Her madde tek bir gözlenebilir sonuçtur ve doğru ya da yanlış denebilir; böylece `12` çevirisi madde başına bir doğrulama maddesi çıkarır (`12`, değişmez giriş kuralı 3).
6. **Test beklentisi.** Değer kümesi şablonun beşidir — Unit, Integration, Contract, E2E, Yok — ve birden çok değer yazılabilir (`Unit, Integration`). `Yok` yalnız kod üretmeyen task'ta, tek başına ve gerekçesiyle yazılır (`Yok — gerekçe`). `Contract` bu projede yoktur (sayfa ve uç testleri Integration'dır; `05 §8.3.8`, `09 §19.1`) ve `E2E` `12`'nin kapsamıdır, bu yüzden task'ta yalnız F6'da yazılır.
7. **Doğrulama kontrol listesi.** Kabul kriteri başına bir madde (`KK1`, `KK2`, …), ardından `09 §25`'in task'a düşen satırları ve güvenlik taraması; madde sayısı kabul kriteri sayısından az olamaz.
8. **Boyut.** Blok en çok 2,5 KB (başlık satırından sonraki başlığa kadar); aşım task'ı bölmenin işaretidir (`TXXb`, K-1409 (2)). Boyut, alan eksiksizliği, bağımlılık ve yürütme sırası `11_task_blok_denetle.py` ile denetlenir.
9. **İleri işaret.** Henüz yazılmamış dokümana (`12`) verilen atıf `(ileri işaret: 12)` diye açıkça işaretlenir; bu bir eksik bağımlılık değildir.

> **Numaralandırma:** Araya task gerektiğinde `TXXa`/`TXXb` (faz aralığını bozmadan). Aynı task'ın devam turu `TXX.1`. Konvansiyonların tamamı: `00_PROJECT_METHODOLOGY.md` §J.
> **Forward pointer:** Henüz yazılmamış bir dokümana verilen referans **açıkça** öyle işaretlenir — bu bir eksik bağımlılık değildir.

### F0 — Proje iskeleti

**Yürütme sırası:** T02 → T01 → T03 → T04 → T06 → T05 → T08 → T07 → T10 → T09

#### T01: Canlılık ucu
- **Bağımlılık:** T02
- **Dokümanlar:** çekirdek; `09` §8, §10 (§10.7), §19; `07 UC-81`; `05 MK-67`, §9.4.1
- **Kabul kriterleri:**
  1. Uygulama ve veritabanı yanıt veriyorsa canlılık ucu 200 döner (`07 UC-81`; `05 §9.4.1`).
  2. Veritabanına ulaşılamıyorsa uç 503 döner.
  3. Yanıt gövdesiz ve ayrıntısızdır; hata metni, bağlantı bilgisi ya da sürüm sızmaz (`05 §9.4.1`).
  4. Uç imzasız açık GET'tir; çerez ve oturum gerektirmez (`07 UC-81`).
  5. Veritabanı denetimi sabit sürede biter; süre dolarsa 503 döner (`09 §10.7`).
  6. Uç `Endpoints/` altındadır, sayfa değildir ve sayfa işleyicisinin hata kalıbına girmez (`09 §4.1`, §8).
- **Test beklentisi:** Integration
- **Doğrulama kontrol listesi:**
  - KK1: sağlıklı veritabanıyla `WebApplicationFactory` isteği 200 ve boş gövde.
  - KK2: veritabanı bağlantısı kesilince 503.
  - KK3: iki yanıtın gövdesi boş; başlıklarda sürüm bilgisi yok.
  - KK4: çerezsiz istek 200.
  - KK5: yavaşlatılmış veritabanı sınırda 503 verir (`09 §19.2` madde 4).
  - KK6: dosya yeri ve ad alanı `09 §4.1` ağacına uyar.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T02: Çözüm ve proje düzeni
- **Bağımlılık:** —
- **Dokümanlar:** çekirdek; `09` §3 (§3.1, §3.2), §4; `05 MK-01`…`MK-05`, §2.3, §3.1, §3.8
- **Kabul kriterleri:**
  1. Kökteki `Shopfolio.slnx` üç projeyi toplar: `Shopfolio.Web`, `Shopfolio.UnitTests`, `Shopfolio.IntegrationTests`; dördüncü proje yoktur (`09 §4.1` madde 1; `05 MK-13`).
  2. `Shopfolio.Web` .NET 10 Razor Pages uygulamasıdır; `Modules/`, `Infrastructure/`, `Pages/`, `Endpoints/`, `wwwroot/` klasörleri vardır (`05 MK-01`…`MK-04`; `09 §4.1` madde 2).
  3. Kökteki `Directory.Build.props` boş olabilir başvuru denetimini açar ve uyarıları hata sayar (`09 §3.1` madde 2).
  4. Kökte tek `.editorconfig` vardır ve `dotnet format` doğrulaması temiz geçer (`09 §3.1` madde 3).
  5. `Directory.Packages.props` sürümleri merkezî ve tam sabit tutar; `packages.lock.json` ve yalnız nuget.org kaynaklı `nuget.config` repodadır (`09 §3.2`).
  6. `wwwroot/` düz JavaScript ve CSS taşır; npm hattı ve ön yüz çatısı yoktur (`05 MK-05`; `09 §4.1` madde 4).
  7. D-01 yığın parçaları — `CONTEXT.md` §1 ve §3, `.gitignore`, `.gitattributes`, `.env.example` — yığına uyarlanır; `CONTEXT.md` §3'ün "kod klasörü henüz yok" satırı kalkar (`05 §2.3.1`…`§2.3.4`; `09 §4.2`).
  8. Bilgim tarihli ayar adları SDK'ya karşı derlenerek doğrulanır; çıkan fark ilgili `09` satırına aynı PR'da yazılır (`09 §3.1` madde 2, §4.1 madde 1).
- **Test beklentisi:** Yok — davranış kodu yoktur; çözümün derlenmesi ve biçim denetimi Build ve Lint adımlarının sınamasıdır
- **Doğrulama kontrol listesi:**
  - KK1: `dotnet build` yapılandırmasında uyarısız derlenir; proje sayısı üçtür.
  - KK2: klasör ağacı `09 §4.1` ile aynıdır.
  - KK3: `Directory.Build.props` ayarları `09 §3.1`'dekidir.
  - KK4: `dotnet format --verify-no-changes` temiz.
  - KK5: proje dosyalarında `Version` yazılı paket yok; kilit dosyası var.
  - KK6: `package.json` yoktur.
  - KK7: `CONTEXT.md` §3 klasör düzeyinde güncel; `.env.example` sır taşımaz.
  - KK8: doğrulanan bilgim tarihli ayarlar PR açıklamasında listeli.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T03: Test altyapısı
- **Bağımlılık:** T02, T01
- **Dokümanlar:** çekirdek; `09` §3 (§3.2), §4, §6, §19, §20; `05 §8.3`, §3.3; `05 MK-15`, `MK-70`
- **Kabul kriterleri:**
  1. Her iki test projesi xUnit v3 ile çalışır; çalıştırılabilir proje biçimi .NET 10 SDK'da sınanır ve sonuç kayda geçer (`09 §19.1` madde 1; K-1355).
  2. `UnitTests/Fakes/` beş sahteyi taşır: `ManualTimeProvider`, `FakePaymentGateway`, `FakeEmailGateway`, `FakeGoogleIdentityGateway`, `InMemoryFileStore`; `IntegrationTests` onları bağlantılı dosya olarak derler (`09 §19.1` madde 3).
  3. Bağlantılı dosya paylaşımı xUnit v3'te derlenir; olmazsa çıkan yol karar kaydına yazılır (K-1355).
  4. Entegrasyon bağlantısı `SHOPFOLIO_TEST_DATABASE` ya da `Tests:Database` ayarından gelir; ortam değişkeni kazanır (`09 §19.1` madde 4).
  5. `shopfolio_test_template` her çalıştırmada göç demetiyle kurulur; her koleksiyon şablondan kendi veritabanını alır ve siler (`09 §19.1` madde 4).
  6. `WebApplicationFactory` tabanlı sayfa ve uç testi örneği geçer; `Program` erişimi `09 §19.1` madde 5'e göredir.
  7. `UnitTests/Architecture/` ArchUnitNET ile modül sınırı testini taşır; kasıtlı ihlalli derleme onu kırar (`09 §19.2` madde 3; `05 MK-15`).
  8. Yasak test kalıplarını arayan kaynak taraması test projelerinde koşar (`09 §19.2` madde 6).
  9. `UnitTests/Consistency/` klasörü ve `Consistency` özelliği vardır; kütüphane tablosu eşitliği denetimi `Directory.Packages.props` ile Kütüphaneler tablosunu paket kimliğiyle karşılaştırır (`09 §3.2`, §21.4 madde 1; K-1446).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: iki proje boş çözümde `dotnet test` ile koşar.
  - KK2: beş sahte tek kopyadır; ikinci kopya yok.
  - KK3: bağlantılı derleme yeşil.
  - KK4: ayar sırası testle gösterilir.
  - KK5: şablon ve koleksiyon veritabanı oluşur ve silinir.
  - KK6: örnek uç testi geçer.
  - KK7: ihlalli örnekte mimari test kırmızı.
  - KK8: tarama `Skip` içeren örnekte kırmızı.
  - KK9: tablo ile paket kümesi eşit; fazla ve eksik satır testi kırar.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T04: CI hattı ve eşitlik denetimleri
- **Bağımlılık:** T03
- **Dokümanlar:** çekirdek; `09` §3 (§3.2), §10 (§10.6), §19, §21; `05 §8.3`, §8.7; `05 MK-12`, `MK-70`, `MK-74`
- **Kabul kriterleri:**
  1. `.github/ci-commands.env` sekiz adımın komutlarını taşır; Contract `NOT_APPLICABLE`'dır (`05 §2.3.5`, §8.3.8).
  2. Lint komutu `dotnet format` doğrulamasını ve `Consistency` özellikli testleri koşar; Unit komutu o testleri dışarıda bırakır (`09 §21.4` madde 1; K-1356).
  3. Build komutu yayın yapılandırmasında uyarıları hata sayar, kilitli geri yükleme ve bilinen açık taraması koşar (`09 §3.2`, §21.4).
  4. Integration komutu gerçek PostgreSQL 18 hizmetine karşı koşar (`05 §2.3.6`).
  5. Migration dry-run komutu göç demetini boş veritabanına uygular; son yayımlanan sürüm yoksa geri alma sınaması atlanır ve atlandığı CI çıktısında yazılır (`09 §21.4` madde 3; K-1356).
  6. Package build komutu üç imajı derler, yayımlamaz (`05 §8.3.10`); komutun sınaması T06'dadır.
  7. `publish.yml` yama sürümünde göç klasörü değişmişse etiket imaja basılmadan önce düşer; `migration-fix: K-<numara>` etiket mesajı istisnadır; ilk etikette denetim koşmaz (`09 §21.4` madde 2; `05 §8.3.12`, §8.7.4; K-1356).
  8. `publish.yml` ana dala her merge'de imajı commit kimliğiyle, sürüm etiketinde sürüm numarasıyla yayımlar; kayıt defteri yazma izni açılır (`05 MK-12`, §8.3.12).
  9. Dependabot açıktır; boş komut kapıyı geçmez (`05 §8.3.14`; `09 §21.4` madde 5).
- **Test beklentisi:** Integration
- **Doğrulama kontrol listesi:**
  - KK1: sekiz komut dolu; Contract değeri `NOT_APPLICABLE`.
  - KK2: Lint ve Unit koşusunda `Consistency` testi yalnız Lint'te görünür.
  - KK3: uyarı içeren örnek derleme düşer.
  - KK4: PostgreSQL hizmetli koşu yeşil.
  - KK5: CI çıktısında atlama satırı var.
  - KK6: komut yazılıdır, sınama T06'ya bırakılmıştır.
  - KK7: yama etiketli, göç eklenmiş dalda yayın düşer; istisna mesajında geçer.
  - KK8: yayın akışı dosyası imaj adlarını commit ve sürüm etiketiyle üretir.
  - KK9: boş komutla `SETUP_COMPLETE=true` CI'ı düşürür.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T05: Compose geliştirme ortamı
- **Bağımlılık:** T06, T01
- **Dokümanlar:** çekirdek; `09` §4 (§4.3), §17; `05 §3.2`, §6.3, §6.8, §8.1; `05 MK-06`, `MK-08`, `MK-10`, `MK-14`, `MK-68`, `MK-76`
- **Kabul kriterleri:**
  1. `deploy/compose.yaml` dört hizmeti ve tek seferlik göç hizmetini tanımlar (`05 §3.2`, §8.2.4; `MK-14`).
  2. Ters vekil ile uygulama, uygulama ile veritabanı ayrı iç ağlardadır; ters vekil ve yedek aracı veritabanına ulaşamaz (`05 §3.2.5`).
  3. Dışarıya açık kapı yalnız 80 ve 443'tür (`05 §3.2.3`; `MK-08`).
  4. Veritabanı ve yedek aracının dışarıya çıkışı `DOCKER-USER` zincirinde hedef adresli kuralla sınırlıdır (`05 §3.2.5`; `MK-76`).
  5. Veri, dosya, kayıt, sertifika ve durum dosyası adlandırılmış bölümlerdedir; ters vekile yalnız görseller salt okuma bağlıdır (`05 §8.2.4`).
  6. `deploy/compose.dev.yaml` üstüne bindirilir; e-posta yakalayıcıyı ekler ve gönderimi ona yönlendirir; ayrı imaj ya da veritabanı sürümü tanımlamaz (`09 §4.3` madde 2; `05 §8.1.2`).
  7. Her imaj etiket ve özetle sabittir; `latest` yoktur (`05 MK-10`; `09 §4.3` madde 4).
- **Test beklentisi:** Yok — Compose ve ağ tanımıdır; sınama Paketleme adımı ve kurulumda konteyner içinden çıkış denemesidir (`05 §3.2.5`)
- **Doğrulama kontrol listesi:**
  - KK1: `docker compose config` geçerli; hizmet sayısı dört.
  - KK2: ters vekilden veritabanına bağlantı reddedilir.
  - KK3: ana makinede yalnız 80 ve 443 dinler.
  - KK4: veritabanı konteynerinden başka adrese çıkış reddedilir, nesne depolamaya geçer.
  - KK5: volume adları listeyle eşit; vekilde yalnız görsel bölümü.
  - KK6: geliştirme varyantıyla e-posta yakalayıcıya düşer, dışarı çıkmaz.
  - KK7: dosyalarda `latest` yok.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T06: Üç imaj ve kayıt defteri
- **Bağımlılık:** T02, T04
- **Dokümanlar:** çekirdek; `09` §4 (§4.3), §17, §21; `05 §3.2`, §8.2; `05 MK-02`, `MK-09`…`MK-11`, `MK-69`
- **Kabul kriterleri:**
  1. Uygulama imajı çok aşamalıdır, kök olmayan kullanıcıyla çalışır ve göç demetini taşır (`05 §8.2.1`; `MK-69`).
  2. Veritabanı imajı resmî PostgreSQL 18 imajına pgBackRest ve gecelik tam yedeğin zamanlayıcısını ekler (`05 §8.2.2`; `09 §17` madde 1).
  3. Yedek aracı imajı restic ikilisini SHA-256 özetiyle sabitler; özet tutmazsa derleme düşer (`09 §17` madde 6).
  4. Her derleme dosyası taban imajı etiket ve özetle sabitler; `latest` yoktur (`05 MK-10`; `09 §4.3` madde 4).
  5. Derleme bağlamı repo köküdür; Dockerfile `src/` altında değildir (`09 §4.3` madde 1).
  6. Imajlara sır girmez; sırlar dosya olarak bağlanır (`09 §17` madde 2).
  7. Kayıt defteri GitHub Container Registry'dir; paket görünürlüğünün ilk yayından sonra açılışı adım olarak yazılıdır (`05 MK-11`, §8.2.5).
  8. CI Paketleme adımı üç imajı derler ve uygulama konteyneri canlılık ucuna yanıt verene kadar ayağa kalkar (`05 §8.3.10`).
- **Test beklentisi:** Unit
- **Doğrulama kontrol listesi:**
  - KK1: uygulama imajı kullanıcısı kök değildir; demet imajdadır.
  - KK2: veritabanı imajında pgBackRest ve zamanlayıcı bulunur.
  - KK3: yanlış özetli ikili derlemeyi düşürür.
  - KK4: kaynak taraması `latest` bulmaz.
  - KK5: derleme komutu `-f deploy/images/<ad>/Dockerfile .` biçimindedir.
  - KK6: sır değeri taraması temiz.
  - KK7: paket görünürlüğü adımı kayıtlı.
  - KK8: Paketleme adımı yeşil ve canlılık denemesi geçer.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T07: Yedek düzeni
- **Bağımlılık:** T06
- **Dokümanlar:** çekirdek; `09` §17, §21; `05 §7.6`, §6.8, §9.2, §9.4; `05 MK-09`, `MK-25`…`MK-27`; `10 KP-75`
- **Kabul kriterleri:**
  1. İşlem günlüğü en geç dakikada bir nesne depolamaya taşınır; `archive_timeout` elli saniyedir (`05 §7.6.1`; `MK-25`).
  2. Gecelik tam yedek imha işinden (03:00) önce biter (`05 §7.6.1`; `09 §17` madde 3).
  3. Dosya yedeği sırası `backup`, `forget --keep-within 30d`, `prune --max-unused 0`, `check`'tir; bir adım düşerse sonrakiler atlanır (`09 §17` madde 4).
  4. Yedek durum dosyası atomik yazılır; başarısız turda `lastSuccessAt` önceki değeri taşır (`09 §17` madde 5).
  5. İki yedeğin ayrı şifreleme anahtarı vardır; kovada sürüm tutma ve otuz günlük yaşam döngüsü kuralı kurulur (`05 §7.6.5`, §7.6.6).
  6. Otuz günden eski tam yedek ve günlükler silinir; sınırdan eski en yeni tam yedek tutulur (`05 §7.6.6`).
  7. Kurtarma provası sunucu dışındaki depodan döner; sonuç `DEPLOY_RUNBOOK`'a yazılır (`05 §7.6.8`).
- **Test beklentisi:** Unit
- **Doğrulama kontrol listesi:**
  - KK1: arşiv ayarı dosyada elli saniye.
  - KK2: zamanlama saati imhadan önce biter.
  - KK3: budama komutu `--max-unused 0` ile koşar (komut satırı testi).
  - KK4: durum dosyası her sonuç için şemaya uyar.
  - KK5: anahtarlar ayrı sır dosyalarıdır; kova kuralı kayıtlı.
  - KK6: saklama ayarı otuz gün.
  - KK7: prova kaydı depo adını ve sonucu taşır.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T08: Sır düzeni
- **Bağımlılık:** T05
- **Dokümanlar:** çekirdek; `09` §4 (§4.3), §17, §18; `05 §6.x`; `05 MK-59`, `MK-60`
- **Kabul kriterleri:**
  1. Sırlar bileşen başına ayrı dosyadadır, yalnız köke açık dizinde durur ve Compose sır tanımıyla yalnız ilgili konteynere bağlanır (`05 §6.x.1`; `MK-59`).
  2. Uygulama sırları dosyadan okur; ortam değişkenine konmaz (`05 §6.x.1`).
  3. Her sırrın sahibi onu okuyan konteyner kullanıcısıdır (`05 §6.x.1`).
  4. Paylaşılan sır yoktur; her bileşen yalnız `05 §6.x.3`…`§6.x.7` tablosundaki sırları alır (`05 §6.x.8`).
  5. Döndürme kuralları `05 §6.x.9` tablosunda yazılıdır; şifreleme anahtarları döndürülmez (`MK-60`).
  6. Repo yalnız `.env.example` şablonunu izler; sır değeri repoya ve kayda girmez (`05 §6.x.16`; `09 §18`).
- **Test beklentisi:** Unit
- **Doğrulama kontrol listesi:**
  - KK1: Compose'ta her sır yalnız bir hizmette tanımlı.
  - KK2: kaynak taraması ortam değişkeninden sır okuyan kod bulmaz.
  - KK3: sır dosyası sahipliği konteyner kullanıcısıdır.
  - KK4: bileşen–sır eşlemesi tabloyla aynı.
  - KK5: döndürme tablosu ile `DEPLOY_RUNBOOK §B` uyumlu.
  - KK6: repoda sır değeri taraması temiz.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T09: İlk yayın ve referans kuruluma ilk iniş
- **Bağımlılık:** T04, T07, T08, T10
- **Dokümanlar:** çekirdek; `09` §4, §17, §21; `05 §8.1`, §8.2, §8.6; `05 MK-68`; `10 §4.1`
- **Kabul kriterleri:**
  1. İlk sürüm etiketi açıklamalı `vANA.ARA.YAMA` biçimindedir ve `publish.yml` imajları yayımlar (`05 §8.3.12`; `09 §21.4` madde 2, 4).
  2. İlk yayında geri alma sınaması atlanır ve atlandığı CI çıktısında yazılıdır (`09 §21.4` madde 3; K-1358).
  3. Üç imaj paketi ilk yayından sonra açık yapılır ve sunucu kimlik bilgisiz çeker (`05 §8.2.5`).
  4. Referans kuruluma ilk iniş yapılır; boş uygulama imajı canlılık ucuna 200 verir (`05 §8.1.4`; `MK-68`).
  5. Kurulum, referans kurulumda imaj özetini kaydeder (`05 §8.2.3`).
  6. Gerçek yapılandırmayla ayağa kaldırma provası sonucu `DEPLOY_RUNBOOK`'a yazılır (`05 §8.1.4`).
- **Test beklentisi:** Yok — yayın ve kurulum işlemidir; kanıt referans kurulumdaki canlılık yanıtı ve CI çıktısıdır
- **Doğrulama kontrol listesi:**
  - KK1: etiket açıklamalı; kayıt defterinde imajlar var.
  - KK2: CI çıktısında atlama satırı.
  - KK3: sunucu imajı kimliksiz çeker.
  - KK4: referans kurulumda canlılık ucu 200.
  - KK5: kaydedilen özet kurulumdakiyle eşit.
  - KK6: prova kaydı runbook'ta.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T10: Veritabanı kullanıcıları
- **Bağımlılık:** T06, T08, T03
- **Dokümanlar:** çekirdek; `09` §10 (§10.6, §10.8), §17, §19; `05 §7.1.3`, §6.x; `05 §7.1`
- **Kabul kriterleri:**
  1. Kurucu betik beş kullanıcıyı kurar; dördünde `SUPERUSER`, `CREATEDB`, `CREATEROLE`, `REPLICATION`, `BYPASSRLS` yoktur (`09 §10.8` madde 1).
  2. `pg_trgm` kurucu kullanıcıyla kurulur; veritabanı UTF-8 ve ICU Türkçe sıralamayla açılır (`05 §7.1.3`; `06 §2.8.3`).
  3. Uygulama kullanıcısı şema değiştiremez; `audit_logs`'ta yalnız ekler ve okur (`09 §10.8` madde 2, 3).
  4. Salt okuma kullanıcısı yalnız `SELECT` alır; `data_protection_keys`'i okuyamaz (`09 §10.8` madde 3).
  5. Yedek durum kullanıcısı yalnız `backup_status_records` iki sütununda `UPDATE` yapar; `pg_hba.conf` genel `trust` satırı kaldırılır (`09 §10.8` madde 1).
  6. Kullanıcı adları göç kullanıcısının üç rol ayarından gelir (`09 §10.8` madde 5).
  7. Yetki matrisi testi `public` şemasındaki her tabloyu karşılaştırır (`09 §10.8` madde 7).
  8. Hiçbir bileşen ağdan kurucu kullanıcıyla bağlanmaz (`05 §7.1.3`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: `rolsuper` ve yaratma bayrakları rol başına yanlış.
  - KK2: `pg_trgm` ve sıralama ayarı kurulumda var.
  - KK3: uygulama kullanıcısı `CREATE TABLE` ve `UPDATE audit_logs` deyimlerinde reddedilir.
  - KK4: okuyucu `INSERT` ve `data_protection_keys` okumada reddedilir.
  - KK5: durum kullanıcısı ikinci sütunda reddedilir.
  - KK6: ayar yokken ilgili göç geri alınır.
  - KK7: matris testi istisna dışı fazlalıkta kırılır.
  - KK8: ağdan kurucu bağlantısı reddedilir (Compose denemesi).
  - `09 §25` ilgili satırlar; güvenlik taraması

### F1 — Veri katmanı

**Yürütme sırası:** T101 → T102 → T103 → T104 → T105 → T106 → T107 → T108 → T109 → T110 → T111 → T112 → T113 → T114 → T115 → T116 → T117 → T118 → T119 → T120

#### T101: Veri modeli temeli
- **Bağımlılık:** —
- **Dokümanlar:** çekirdek; `09` §5, §9, §10 (§10.1, §10.6), §12; `06 §2.1`–§2.6, §2.8–§2.10, §5.1; `02 §4.1.1`; `05 MK-03`, `MK-06`, `MK-19`, `MK-74`
- **Kabul kriterleri:**
  1. Veritabanı bağlamı tektir ve `Infrastructure/` altındadır; modül kendi bağlamını açmaz (`09 §4.1` madde 2).
  2. Birincil anahtar tek sütunlu `id`, sürüm 7 UUID'dir ve uygulama üretir (`06 §2.3.1`).
  3. Tablo adı çoğul, sütun adı küçük harf ve alt çizgilidir; `pk_`, `fk_`, `ix_` ve `ck_` adları kuralla türer, elle verilmez (`06 §2.4.1`, §2.4.3).
  4. Türler `06 §2.5.1` tablosuna uyar: para `numeric(12,2)`, an `timestamptz`, tarih `date`.
  5. Sürüm taşıyan kayıtta `version` tamsayı sütunudur ve yazım koşullu güncellemedir (`06 §2.6.1`).
  6. Kimlik karşılaştırması tek normalleştirme işleviyle, ayrı sütunda yapılır; veritabanı sıralaması ICU Türkçedir (`06 §2.8.1`…`§2.8.3`).
  7. Her tablo `created_at` taşır; boş değer yalnız `NULL`'dur (`06 §2.8.4`, §2.8.5).
  8. Bağ `NO ACTION` ya da yalnız sahibiyle basamaklıdır; `RESTRICT` kullanılmaz (`06 §2.2.5`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: çözümde tek bağlam sınıfı var.
  - KK2: örnek kaydın anahtarı UUID v7.
  - KK3: örnek tablonun kısıt adları kuraldan türer.
  - KK4: tür eşlemesi tabloyla eşit.
  - KK5: eski sürümle güncelleme sıfır satır etkiler.
  - KK6: `I`, `İ`, `ı`, `i` aynı normal biçime iner.
  - KK7: boş metin kaydedilmez, `NULL` kaydedilir.
  - KK8: silme davranışı modelde `NO ACTION`.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T102: Enum tanımları ve değer dönüşümü
- **Bağımlılık:** T101
- **Dokümanlar:** çekirdek; `09` §10 (§10.1), §12; `06 §5.2`–§5.6
- **Kabul kriterleri:**
  1. Her enum `06 §5.2`…`§5.6` tablosundaki kod adlarıyla, birebir tanımlıdır (`06 §5.1.2`).
  2. Enum değeri veritabanında metin olarak kod adıyla saklanır; sayısal değer anlam taşımaz (`06 §2.9.1`).
  3. Enum sütunu izinli değerlerin kontrol kısıtını taşır; alt küme kullanan alan o alt kümeyi sayar (`06 §2.9.2`).
  4. Değer dönüşümü her enum için tek yerde tanımlıdır ve bilinmeyen metinde hata verir.
  5. Kalıcı her enum için `UnitTests/Locks/<EnumTamAdı>.txt` kilit dosyası vardır ve eşitlik iki yönlü sınanır (`09 §12`).
  6. Davranışı olan değerin kuralı enum'da yinelenmez; kuralın evine işaret edilir (`06 §5.1.3`).
- **Test beklentisi:** Unit
- **Doğrulama kontrol listesi:**
  - KK1: tablo ile enum üye kümesi eşit.
  - KK2: örnek kayıt veritabanında metin olarak görünür.
  - KK3: izinli olmayan metin kısıtı ihlal eder.
  - KK4: bilinmeyen metin okunurken hata verir.
  - KK5: üye eklenip kilit dosyası güncellenmezse test kırılır.
  - KK6: enum dosyalarında kural kopyası yok.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T103: Başlangıç listeleri, firma ve yasal metin sürümü
- **Bağımlılık:** T102
- **Dokümanlar:** çekirdek; `09` §10 (§10.6), §12; `06` Province, Country, Company, CompanySettings, CompanyDeliveryProvince, LegalTextVersion; `02 §3.1.1`, §3.14.3
- **Kabul kriterleri:**
  1. Altı entity `06 §4.1`…`§4.4` ve `§4.56` alan tablolarıyla tek göçle kurulur.
  2. `Company` ve `CompanySettings` tek satırdır; sabit ifadenin tekil dizini korur (`06 §2.3.2`).
  3. `CompanySettings` havale açıksa IBAN dolu ve iade adresinin dört alanı birlikte dolu ya da boş kısıtlarını, bölüm sürümü sütunlarını taşır (`06 §4.2`; `§2.6.3`).
  4. `Province.Code` tekil ve iki rakam, `Country.Code` tekil ve iki büyük harftir; `ProvinceId` teslimat ili satırında tekildir.
  5. `LegalTextVersion` (`LegalTextType`, `VersionNumber`) çiftini dolu satırlarda tekil tutar; sürüm numarası ile yayın anı birlikte doludur (`06 §4.56`).
  6. İl ve ülke satırı silinmez; gelen bağlar kısıtlayıcıdır; teslimat ili satırı olayla silinir (`06 §4.2`…`§4.4`).
  7. Göç tek konudur; `Down` ve işlem dışı adım yoktur (`09 §10.6` madde 1, 2).
- **Test beklentisi:** Integration
- **Doğrulama kontrol listesi:**
  - KK1: göç boş veritabanına uygulanır, altı tablo var.
  - KK2: ikinci `Company` satırı reddedilir.
  - KK3: havale açık ve IBAN boşken kayıt reddedilir.
  - KK4: kodu üç haneli il reddedilir.
  - KK5: aynı tür ve sürüm numarası ikinci kez reddedilir.
  - KK6: il silme, bağlı satır varken reddedilir.
  - KK7: kaynak taraması `Down` bulmaz.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T104: Ürün çekirdeği ve görsel
- **Bağımlılık:** T103
- **Dokümanlar:** çekirdek; `09` §10 (§10.6), §12; `06` Image, Product, Option, OptionValue, Variant, Category, ProductCategory, ProductImage
- **Kabul kriterleri:**
  1. Sekiz entity `06 §4.5`…`§4.10`, `§4.69`, `§4.71` alan tablolarıyla tek göçle kurulur.
  2. `Product.VatRate` 0 ≤ oran < 100 ve çit kısıtlarını taşır (`06 §4.5`).
  3. `Variant` stok kodunun normalleştirilmiş sütunu tekildir; (`ProductId`, birinci ve ikinci değer) kombinasyonu `NULLS NOT DISTINCT` tekildir (`06 §4.6`).
  4. `Option` (`ProductId`, `SortOrder`) tekildir ve sıra 1 ile P-21 arasındadır (`06 §4.7`).
  5. `Category.Level` 1 ile P-20 arasındadır; üst kategori boşsa 1'dir (`06 §4.9`).
  6. Ana kategori ürün başına en çoktur (filtreli tekil dizin); ana görsel sahip başına en çoktur (`06 §4.10`, §4.71).
  7. `Image` boyutu sıfırdan büyük ve 2560'ı aşmaz; ürün silinince varyant, seçenek ve bağlar basamaklanır, dolu kategori silinmez (`06 §4.5`, §4.9, §4.69).
  8. Göç tek konudur; `Down` yoktur (`09 §10.6`).
- **Test beklentisi:** Integration
- **Doğrulama kontrol listesi:**
  - KK1: göç boş veritabanına uygulanır, sekiz tablo.
  - KK2: yüz oranlı KDV reddedilir.
  - KK3: aynı stok kodu farklı harfle reddedilir.
  - KK4: üçüncü seçenek reddedilir.
  - KK5: dördüncü düzey kategori reddedilir.
  - KK6: ikinci ana kategori ve ikinci ana görsel reddedilir.
  - KK7: ürün silmede varyant gider; dolu kategori silme reddedilir.
  - KK8: kaynak taraması temiz.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T105: Fiyat, kupon ve dijital dosya
- **Bağımlılık:** T104
- **Dokümanlar:** çekirdek; `09` §10 (§10.6), §12; `06` PriceHistory, Discount, Coupon, DigitalFile
- **Kabul kriterleri:**
  1. Dört entity `06 §4.11`…`§4.13` ve `§4.70` alan tablolarıyla tek göçle kurulur.
  2. `PriceHistory.Price` sıfırdan büyüktür; varyantla basamaklanır (`06 §4.11`).
  3. `Discount` ürün başına tekildir; 0 < yüzde < 100 ve bitiş ≥ başlangıç kısıtlarını taşır (`06 §4.12`).
  4. `Coupon` normalleştirilmiş kodda tekildir; yüzde ile tutardan tam biri doludur; tutar doluysa asgari sepet tutarı doludur ve büyüktür (`06 §4.13`).
  5. Kupon silinmez; gelen bağlar kısıtlayıcıdır (`06 §4.13`).
  6. `DigitalFile` ürün ya da varyanttan en çok birine bağlıdır; yükleme tamamlanmadan bağ ve bağlanma anı boştur; bayt boyutu `bigint` ve sıfırdan büyüktür (`06 §4.70`).
  7. Göç tek konudur; `Down` yoktur (`09 §10.6`).
- **Test beklentisi:** Integration
- **Doğrulama kontrol listesi:**
  - KK1: göç boş veritabanına uygulanır, dört tablo.
  - KK2: sıfır fiyat reddedilir.
  - KK3: ikinci indirim ve %100 indirim reddedilir.
  - KK4: yüzde ve tutar birlikte dolu kupon reddedilir.
  - KK5: kupon silme bağlıyken reddedilir.
  - KK6: tamamlanmamış yüklemede bağ dolu satır reddedilir.
  - KK7: kaynak taraması temiz.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T106: Yönetici hesabı, davet ve işlem izi
- **Bağımlılık:** T105
- **Dokümanlar:** çekirdek; `09` §9, §10 (§10.6, §10.8), §12; `06` Admin, AdminInvitation, AuditLog; `02 §1.3`, `Z-3`, `Z-30`, §6.9, §10.3; `05 MK-24`, §7.5
- **Kabul kriterleri:**
  1. Üç entity `06 §4.37`, `§4.38`, `§4.63` alan tablolarıyla tek göçle kurulur.
  2. `Admin` normalleştirilmiş e-postada tekildir (`06 §4.37`).
  3. `AdminInvitation` geri çekilme, geçersiz kılınma ve kabul anlarından en çok birini taşır; üçü boş satırlarda adres tekildir (`06 §4.38`).
  4. `audit_logs` göçü uygulama kullanıcısından `UPDATE` ve `DELETE` yetkisini rol ayarından okuduğu adla geri alır (`09 §10.8` madde 3; `05 §7.5.1`).
  5. İz satırı yöneticiyi ad ve e-postayla değer olarak taşır; `AuditLog` başka tabloya bağ taşımaz (`06 §2.2.5`).
  6. Yönetici hesabına giden bağlar boşaltılır (`06 §2.2.5`).
  7. Göç tek konudur; `Down` yoktur (`09 §10.6`).
- **Test beklentisi:** Integration
- **Doğrulama kontrol listesi:**
  - KK1: göç boş veritabanına uygulanır, üç tablo.
  - KK2: aynı e-posta farklı harfle reddedilir.
  - KK3: iki sonuçlanmış davet anı reddedilir.
  - KK4: uygulama kullanıcısı `audit_logs` güncellemede reddedilir.
  - KK5: rol ayarı yokken göç geri alınır.
  - KK6: `audit_logs` yabancı anahtar taşımaz.
  - KK7: kaynak taraması sabit kullanıcı adı bulmaz.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T107: Müşteri hesabı
- **Bağımlılık:** T106
- **Dokümanlar:** çekirdek; `09` §10 (§10.6), §12; `06` Customer, AddressBook, PendingEmailChange; `02 §1.3`
- **Kabul kriterleri:**
  1. Üç entity `06 §4.36`, `§4.40`, `§4.45` alan tablolarıyla tek göçle kurulur.
  2. `Customer` normalleştirilmiş e-postada tekildir; doğrulanmamış hesapta şifre özeti dolu ve Google bağı boştur (`06 §4.36`).
  3. `PendingEmailChange` müşteri ya da yöneticiden tam birine bağlıdır; her biri ayrı tekildir, yeni e-posta tekil değildir (`06 §4.40`).
  4. `AddressBook.Phone` on rakamdır (`06 §4.45`).
  5. Hesapla birlikte adres defteri ve bekleyen değişiklik basamaklanır (`06 §2.2.5`).
  6. Göç tek konudur; `Down` yoktur (`09 §10.6`).
- **Test beklentisi:** Integration
- **Doğrulama kontrol listesi:**
  - KK1: göç boş veritabanına uygulanır, üç tablo.
  - KK2: aynı e-posta farklı harfle reddedilir.
  - KK3: doğrulanmamış hesapta şifresiz kayıt reddedilir.
  - KK4: iki hesaba bağlı bekleyen değişiklik satırı reddedilir.
  - KK5: dokuz haneli telefon reddedilir.
  - KK6: hesap silmede adres defteri gider.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T108: Oturum, bağlantı, deneme ve giriş kayıtları
- **Bağımlılık:** T107
- **Dokümanlar:** çekirdek; `09` §10 (§10.6), §12; `06` Session, LinkRecord, AttemptRecord, RecognizedBrowserMark, LoginRecord; `02 §3.13.20`, `P-45`; `05 MK-48`, `MK-50`
- **Kabul kriterleri:**
  1. Beş entity `06 §4.39`, `§4.41`…`§4.44` alan tablolarıyla tek göçle kurulur.
  2. `Session` müşteri ya da yöneticiden tam birine bağlıdır; belirteç özeti tekildir (`06 §4.39`).
  3. `RecognizedBrowserMark` işaret özeti tekil, hesap bağı tam birdir (`06 §4.41`).
  4. `LinkRecord` müşteri, yönetici ve davetten tam birine bağlıdır; `AdminInvitation` türünde ve yalnız o zaman davet bağı doludur (`06 §4.42`).
  5. `AttemptRecord` ekseni limitin eksenine uyar (`06 §4.43`).
  6. `LoginRecord` yalnız eklenir; satır güncellenmez (`06 §4.44`).
  7. Oturum ve bağlantı kaydı sahibiyle basamaklanır (`06 §2.2.5`).
  8. Göç tek konudur; `Down` yoktur (`09 §10.6`).
- **Test beklentisi:** Integration
- **Doğrulama kontrol listesi:**
  - KK1: göç boş veritabanına uygulanır, beş tablo.
  - KK2: hem müşteri hem yönetici bağlı oturum reddedilir.
  - KK3: aynı belirteç özeti reddedilir.
  - KK4: davet türünde davetsiz bağlantı reddedilir.
  - KK5: L4 satırında e-posta ekseni reddedilir.
  - KK6: giriş kaydına güncelleme yolu kodda yoktur.
  - KK7: hesap silmede oturum gider.
  - KK8: kaynak taraması temiz.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T109: Sepet ve sipariş çekirdeği
- **Bağımlılık:** T108
- **Dokümanlar:** çekirdek; `09` §9, §10 (§10.6), §12; `06` Cart, CartItem, Order, OrderItem, TransitionRecord, InternalNote, CheckboxAcceptance, CheckboxAcceptanceOrderItem
- **Kabul kriterleri:**
  1. Sekiz entity `06 §4.15`…`§4.21` alan tablolarıyla tek göçle kurulur.
  2. `Cart` müşteri ya da misafir anahtarı özetinden tam birini taşır; ikisi ayrı tekildir (`06 §4.15`).
  3. `CartItem` (`CartId`, `VariantId`) tekildir; varyant bağı boşsa kaldırılan ürün adı doludur (`06 §4.16`).
  4. `Order` numara, erişim anahtarı ve onay anahtarında ayrı ayrı tekildir; iki eksenin yasak çiftleri kontrol kısıtıdır (`06 §4.17`).
  5. `OrderItem` adet, fiyat, indirim, KDV ve kupon payı kısıtlarını taşır (`06 §4.18`).
  6. `TransitionRecord` neden alanlarından en çok birini taşır; düzeltme satırı yalnız düzeltme alanlarını taşır (`06 §4.19`).
  7. `InternalNote` siparişte tekildir; `CheckboxAcceptance` (`OrderId`, tür) tekildir (`06 §4.20`, §4.21).
  8. Sipariş ve kalem silinmez; bağlar kısıtlayıcı, hesap ve yönetici bağı boşaltılır (`06 §4.17`…§4.19).
- **Test beklentisi:** Integration
- **Doğrulama kontrol listesi:**
  - KK1: göç boş veritabanına uygulanır, sekiz tablo.
  - KK2: sahipsiz ya da iki sahipli sepet reddedilir.
  - KK3: işaretsiz varyantsız kalem reddedilir.
  - KK4: ödeme Başarısız ve sipariş Alındı birleşimi reddedilir.
  - KK5: sıfır adetli kalem reddedilir.
  - KK6: iki nedenli geçiş satırı reddedilir.
  - KK7: siparişe bağlı kalem varken sipariş silme reddedilir.
  - KK8: kaynak taraması temiz.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T110: Ayırma ve düzeltme geçmişleri
- **Bağımlılık:** T109
- **Dokümanlar:** çekirdek; `09` §9, §10 (§10.6), §12; `06` StockReservation, OrderEmailHistory, OrderAddressHistory
- **Kabul kriterleri:**
  1. Üç entity `06 §4.14`, `§4.61`, `§4.62` alan tablolarıyla tek göçle kurulur.
  2. `StockReservation` varyant ile kuponun tam birine bağlıdır; adet sıfırdan büyük, kupon satırında 1'dir (`06 §4.14`).
  3. (`OrderId`, `VariantId`) varyant satırlarında, `OrderId` kupon satırlarında tekildir (filtreli tekil dizinler).
  4. Ayırma varyantla basamaklanır; sipariş ve kupon bağı kısıtlayıcıdır (`06 §4.14`).
  5. `OrderAddressHistory` teslimat adresinde iki telefonu dolu, fatura adresinde boş tutar; telefonlar on rakamdır (`06 §4.62`).
  6. İki geçmiş uygulama düzeyinde yalnız eklenir (`06 §2.2.3`).
  7. Göç tek konudur; `Down` yoktur (`09 §10.6`).
- **Test beklentisi:** Integration
- **Doğrulama kontrol listesi:**
  - KK1: göç boş veritabanına uygulanır, üç tablo.
  - KK2: varyantsız ve kuponsuz ayırma reddedilir.
  - KK3: ikinci aynı varyant ayırması reddedilir.
  - KK4: varyant silmede ayırma gider.
  - KK5: fatura adresinde telefon dolu satır reddedilir.
  - KK6: geçmiş tablolarında güncelleme yolu yok.
  - KK7: kaynak taraması temiz.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T111: Kalem kayıtları
- **Bağımlılık:** T110
- **Dokümanlar:** çekirdek; `09` §9, §10 (§10.6), §12; `06` RefundIban, DefectClaim, Cancellation, ItemRemoval, LateDeliveryTermination, Withdrawal, Return, NotReturnedClosure
- **Kabul kriterleri:**
  1. Sekiz entity `06 §4.22`…`§4.26`, `§4.28`, `§4.30`, `§4.31` alan tablolarıyla tek göçle kurulur.
  2. İptal, çıkarma, gecikme feshi ve cayma kayıtlarında adet ve ürün tutarı sıfırdan büyük, 0 ≤ kupon payı ≤ ürün tutarı, KDV ≥ 0'dır (`06 §4.22`…`§4.25`).
  3. Eylemi yapanın türü müşteri ya da yönetici olan kayıtlarda yönetici alanları tür kuralına uyar (`06 §4.22`, §4.24, §4.25).
  4. `Return` cayma, gecikme feshi ve ayıp talebi bağlarından en çok birini taşır (`06 §4.26`).
  5. `DefectClaim` kalem başına tekildir; iade adresinin dört alanı birlikte dolu ya da boştur (`06 §4.30`).
  6. `RefundIban` biçim kısıtını taşır; IBAN olayla silinir (`06 §4.31`).
  7. Kayıtlar silinmez; kalem bağı kısıtlayıcı, yönetici ve IBAN bağı boşaltılır (`06 §4.22`…§4.28).
  8. Göç tek konudur; `Down` yoktur (`09 §10.6`).
- **Test beklentisi:** Integration
- **Doğrulama kontrol listesi:**
  - KK1: göç boş veritabanına uygulanır, sekiz tablo.
  - KK2: kupon payı ürün tutarını aşan kayıt reddedilir.
  - KK3: müşteri kaydında yönetici alanı dolu satır reddedilir.
  - KK4: iki bağlı `Return` satırı reddedilir.
  - KK5: aynı kaleme ikinci ayıp talebi reddedilir.
  - KK6: küçük harfli IBAN reddedilir.
  - KK7: kalemi bağlı kayıt varken kalem silme reddedilir.
  - KK8: kaynak taraması temiz.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T112: Ödeme ve geri ödeme kayıtları
- **Bağımlılık:** T111
- **Dokümanlar:** çekirdek; `09` §9, §10 (§10.6), §12, §14; `06` PaymentRecord, Refund, RefundRecord, InboundNotificationRecord
- **Kabul kriterleri:**
  1. Dört entity `06 §4.32`…`§4.35` alan tablolarıyla tek göçle kurulur.
  2. `PaymentRecord` sağlayıcı işlem kimliğinde dolu satırlarda tekildir; işlem kimliği ile geçiş kaydından en az biri doludur (`06 §4.32`).
  3. `Refund` türe göre bağları taşır; kalem kaydı bağı yalnız tutar temelli türde doludur; tutar ve adet sıfırdan büyüktür (`06 §4.33`).
  4. `RefundRecord` sonuç anını yalnız başarılı ya da gerçekleşmedi hâlinde taşır; firma işareti hâli gerçekleşmedi yapar (`06 §4.34`).
  5. `InboundNotificationRecord` bildirim kimliği ya da işlem kimliğinden en az birini taşır; iki filtreli tekil dizin bildirimin tekrarını engeller (`06 §4.35`).
  6. Tutar sütunları `numeric(12,2)` türündedir (`06 §2.5.1`).
  7. Göç tek konudur; `Down` yoktur (`09 §10.6`).
- **Test beklentisi:** Integration
- **Doğrulama kontrol listesi:**
  - KK1: göç boş veritabanına uygulanır, dört tablo.
  - KK2: aynı işlem kimlikli ikinci ödeme kaydı reddedilir.
  - KK3: tutar temelsiz türde kapanış bağı dolu satır reddedilir.
  - KK4: bekleyen hâlde sonuç anı dolu satır reddedilir.
  - KK5: kimliksiz bildirim satırı reddedilir; aynı bildirim ikinci kez reddedilir.
  - KK6: tutar sütunu türü tabloyla eşit.
  - KK7: kaynak taraması temiz.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T113: İade reddi ve IBAN isteği
- **Bağımlılık:** T112
- **Dokümanlar:** çekirdek; `09` §9, §10 (§10.6), §12; `06` ReturnRejection, RefundIbanRequest
- **Kabul kriterleri:**
  1. İki entity `06 §4.27` ve `§4.29` alan tablolarıyla tek göçle kurulur.
  2. `ReturnRejection` adedi sıfırdan büyüktür; kalem ve cayma beyanı bağları kısıtlayıcıdır (`06 §4.27`).
  3. `RefundIbanRequest` karşılanma ve kapanış anlarından en çok birini taşır (`06 §4.29`).
  4. IBAN bağı yalnız karşılanma anı doluyken doludur (`06 §4.29`).
  5. Geri ödeme bağı sebebe göre dolu ya da boştur: kart geri ödemesi başarısızlığında ve kalemsiz istekte aktarım başarısızlığında doludur (`06 §4.29`).
  6. Kayıtlar silinmez; yönetici bağı boşaltılır, IBAN olayla silinir (`06 §2.2.5`).
  7. Göç tek konudur; `Down` yoktur (`09 §10.6`).
- **Test beklentisi:** Integration
- **Doğrulama kontrol listesi:**
  - KK1: göç boş veritabanına uygulanır, iki tablo.
  - KK2: sıfır adetli ret reddedilir.
  - KK3: hem karşılanmış hem kapanmış istek reddedilir.
  - KK4: karşılanmamış istekte IBAN bağı dolu satır reddedilir.
  - KK5: sebebe aykırı geri ödeme bağı reddedilir.
  - KK6: IBAN silinince istek satırı kalır, bağ boşalır.
  - KK7: kaynak taraması temiz.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T114: İçerik kayıtları ve marka
- **Bağımlılık:** T113
- **Dokümanlar:** çekirdek; `09` §10 (§10.6), §12; `06` AboutPage, Offering, PortfolioItem, FaqItem, CustomPage, Announcement, SocialLink, Branding
- **Kabul kriterleri:**
  1. Sekiz entity `06 §4.46`…`§4.49`, `§4.51`, `§4.52`, `§4.54`, `§4.55` alan tablolarıyla tek göçle kurulur.
  2. `AboutPage`, `Announcement` ve `Branding` en çok bir satırdır; sabit ifadenin tekil dizini korur (`06 §2.3.2`).
  3. Yayın durumu yalnız taslak ya da yayımlanmış değerini alır (`06 §2.9.2`).
  4. `Announcement` bitiş tarihi başlangıçtan önce olamaz (`06 §4.52`).
  5. `SocialLink` platform başına tekildir (`06 §4.55`).
  6. `Branding` marka rengi `#` ve altı büyük onaltılık hane biçimindedir (`06 §4.54`).
  7. Elle sıra tekil değildir (`06 §4.47`).
  8. Uzun metin `text` sütunundadır (`06 §2.10.1`); kurumsal içerik kaydı silinince bağlı kayıtları basamaklanır (`06 §2.2.5`).
- **Test beklentisi:** Integration
- **Doğrulama kontrol listesi:**
  - KK1: göç boş veritabanına uygulanır, sekiz tablo.
  - KK2: ikinci `Announcement` satırı reddedilir.
  - KK3: üçüncü yayın durumu değeri reddedilir.
  - KK4: bitişi başlangıçtan önce duyuru reddedilir.
  - KK5: aynı platformda ikinci bağlantı reddedilir.
  - KK6: kısa renk kodu reddedilir.
  - KK7: aynı sıra değerli iki satır kaydedilir.
  - KK8: kaynak taraması temiz.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T115: Şube, içerik–ürün bağları ve iletişim talebi
- **Bağımlılık:** T114
- **Dokümanlar:** çekirdek; `09` §10 (§10.6), §12; `06` Branch, OfferingProduct, PortfolioItemProduct, ContactRequest
- **Kabul kriterleri:**
  1. Dört entity `06 §4.50`, `§4.53`, `§4.57` alan tablolarıyla tek göçle kurulur.
  2. `Branch` ve `ContactRequest` telefonu on rakamdır (`06 §4.50`, §4.57).
  3. `Branch` görseli yoksa görsel alt metni boştur (`06 §4.50`).
  4. İki içerik–ürün bağı (içerik, `ProductId`) çiftinde tekildir (`06 §4.53`).
  5. İçerik–ürün bağı ürün ve içerik kaydıyla basamaklanır (`06 §2.2.5`).
  6. Kapalı iletişim talebinde son kapanış anı doludur (`06 §4.57`).
  7. Göç tek konudur; `Down` yoktur (`09 §10.6`).
- **Test beklentisi:** Integration
- **Doğrulama kontrol listesi:**
  - KK1: göç boş veritabanına uygulanır, dört tablo.
  - KK2: dokuz haneli telefon reddedilir.
  - KK3: görselsiz şubede alt metin dolu satır reddedilir.
  - KK4: aynı ürünü ikinci kez bağlamak reddedilir.
  - KK5: ürün silmede bağ gider.
  - KK6: kapalı ve kapanış anı boş talep reddedilir.
  - KK7: kaynak taraması temiz.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T116: Galeri bağları
- **Bağımlılık:** T115
- **Dokümanlar:** çekirdek; `09` §10 (§10.6), §12; `06` AboutPageImage, OfferingImage, PortfolioItemImage, FaqItemImage, CustomPageImage
- **Kabul kriterleri:**
  1. Beş galeri bağı `06 §4.71` alan tablosuyla tek göçle kurulur; `ProductImage` T104'tedir.
  2. Her bağ (sahibin bağı, `ImageId`) çiftinde tekildir (`06 §4.71`).
  3. Ana görsel sahip başına en çoktur; filtreli tekil dizin korur (`06 §4.71`).
  4. Bağ sahibiyle basamaklanır; görsele giden bağ kısıtlayıcıdır ve görseli imhadan korur (`06 §2.2.5`).
  5. Elle sıra tekil değildir (`06 §4.71`).
  6. Göç tek konudur; `Down` yoktur (`09 §10.6`).
- **Test beklentisi:** Integration
- **Doğrulama kontrol listesi:**
  - KK1: göç boş veritabanına uygulanır, beş tablo.
  - KK2: aynı görseli galeriye ikinci kez eklemek reddedilir.
  - KK3: ikinci ana görsel reddedilir.
  - KK4: sahip silmede bağ gider; bağlı görsel silme reddedilir.
  - KK5: aynı sıra değerli iki bağ kaydedilir.
  - KK6: kaynak taraması temiz.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T117: Gönderim kaydı ve kanal hâli
- **Bağımlılık:** T116
- **Dokümanlar:** çekirdek; `09` §10 (§10.6), §12; `06` DispatchRecord, ChannelStateRecord; `05 MK-38`, §4.4
- **Kabul kriterleri:**
  1. İki entity `06 §4.58` ve `§4.60` alan tablolarıyla tek göçle kurulur.
  2. `DispatchRecord` sipariş bağı ve ilgili kayıt türü, gönderim türüne göre dolu ya da boştur; tür ile kimlik birlikte doludur (`06 §4.58`).
  3. Gönderim anı yalnız gönderilmiş hâlinde doludur (`06 §4.58`).
  4. İlgili kaydın kimliği bağ değil değer olarak durur; siparişe bağlı gönderim kaydı siparişi silinemez kılar (`06 §2.2.5`, §2.2.6).
  5. `ChannelStateRecord` tek satırdır; art arda hata sayısı sıfırdan küçük olamaz ve ilk hata anı yalnız sayı sıfırken boştur (`06 §4.60`).
  6. Göç tek konudur; `Down` yoktur (`09 §10.6`).
- **Test beklentisi:** Integration
- **Doğrulama kontrol listesi:**
  - KK1: göç boş veritabanına uygulanır, iki tablo.
  - KK2: türe aykırı sipariş bağı reddedilir.
  - KK3: sırada bekleyen satırda gönderim anı dolu ise reddedilir.
  - KK4: ilgili kayıt silinince gönderim kaydı kalır.
  - KK5: ikinci `ChannelStateRecord` satırı reddedilir.
  - KK6: kaynak taraması temiz.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T118: Zamanlayıcı, yedek, imha ve anahtar kayıtları
- **Bağımlılık:** T117
- **Dokümanlar:** çekirdek; `09` §10 (§10.6, §10.8), §12; `06` Job, HeartbeatRecord, OutageRecord, BackupStatusRecord, DataProtectionKey, DisposalRecord; `02 §3.34`; `10 KP-75`
- **Kabul kriterleri:**
  1. Altı entity `06 §4.59`, `§4.64`…`§4.68` alan tablolarıyla tek göçle kurulur.
  2. `Job` (iş türü, ilgili kayıt) çiftinde bekleyen satırlarda tekildir; tamamlandı hâli ile tamamlanma anı birlikte doludur; deneme sayısı ≥ 0'dır (`06 §4.59`).
  3. `HeartbeatRecord` ve `BackupStatusRecord` tek satırdır (`06 §2.3.2`).
  4. `OutageRecord` süresi iki dakikadan uzundur (`06 §4.66`).
  5. `DisposalRecord` sayı ve tarih alanları türe göre dolu ya da boştur; bitiş ≥ başlangıçtır (`06 §4.64`).
  6. `DataProtectionKey` paketin şemasıdır: tamsayı anahtar, oluşturulma anı yok (`06 §4.68`).
  7. Göç `data_protection_keys` okumasını salt okuma kullanıcısından alır ve `backup_status_records` iki sütununda güncelleme yetkisini durum kullanıcısına verir; adlar rol ayarından okunur (`09 §10.8` madde 3).
  8. Göç tek konudur; `Down` yoktur (`09 §10.6`).
- **Test beklentisi:** Integration
- **Doğrulama kontrol listesi:**
  - KK1: göç boş veritabanına uygulanır, altı tablo.
  - KK2: aynı bekleyen iş ikinci kez reddedilir.
  - KK3: tamamlanma anı dolu bekleyen iş reddedilir.
  - KK4: ikinci `HeartbeatRecord` satırı reddedilir.
  - KK5: iki dakikalık kesinti satırı reddedilir.
  - KK6: sayısız imha satırı reddedilir.
  - KK7: okuyucu `data_protection_keys` okumada reddedilir; durum kullanıcısı ikinci sütunda reddedilir.
  - KK8: kaynak taraması temiz.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T119: Başlangıç verisi göçü ve seed olmayanların sınırı
- **Bağımlılık:** T118
- **Dokümanlar:** çekirdek; `09` §10 (§10.6), §12; `06 §8.1`, §8.4 (2)
- **Kabul kriterleri:**
  1. Seed göçün veri adımında yazılır; kodda "kayıt yoksa oluştur" dalı yoktur (`06 §8.1.1`).
  2. Göç 81 il ile ülke listesini yazar; il kodu plakadır (`06 §8.1.1`).
  3. Göç `Company`, `CompanySettings`, `Branding`, `ChannelStateRecord`, `HeartbeatRecord`, `BackupStatusRecord` tek satırlarını başlangıç değerleriyle yazar (`06 §8.1.1`).
  4. `CompanySettings` varsayılanları `02 §11.1`'dendir; havale kapalı, ana sayfa düzeni `ContentFirst`'tür (`06 §8.1.1`).
  5. `CompanyDeliveryProvince` 81 il satırıyla, varsayılan tüm Türkiye olarak yazılır (`06 §8.1.1`).
  6. İki `LegalTextVersion` taslağı yayın anı ve sürüm numarası boş yazılır (`06 §8.1.1`).
  7. Göç var olan satırı değiştirmez; `UPDATE` ve `DELETE` izinli tablolar dışında yazılmaz (`06 §8.1.2`; `09 §10.6` madde 4).
  8. Seed olmayanlar koddadır ya da kuruluma özgüdür: enum'lar, iş günü takvimi, kargo şirketi listesi koda; ilk yönetici ve kurulum ayarları kuruluma aittir (`06 §8.3`, §8.4).
- **Test beklentisi:** Integration
- **Doğrulama kontrol listesi:**
  - KK1: boş veritabanında 81 il ve 81 teslimat ili satırı var.
  - KK2: altı tek satırlık kayıt başlangıç değerlerinde.
  - KK3: iki taslak metin yayımlanmamış.
  - KK4: seed göçü ikinci uygulamada satır sayısını değiştirmez.
  - KK5: kaynak taraması `UPDATE` ve `DELETE` için izinli tablo dışında eşleşme bulmaz.
  - KK6: kodda varsayılan kaydı oluşturan yol yoktur.
  - KK7: ayar varsayılanı tabloyla eşit.
  - KK8: seed olmayanlar listede açıkça yazılı.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T120: Model eşitlik denetimleri
- **Bağımlılık:** T119
- **Dokümanlar:** çekirdek; `09` §5, §10, §19; `06 §2.7`, §6.1…§6.6, §7.1…§7.6
- **Kabul kriterleri:**
  1. İlişki diyagramının 130 çizgisi modelin yabancı anahtarlarıyla birebir eşleşir (`06 §6`, §7.2).
  2. Tekil dizin listesi modelle eşittir; filtreli tekil dizinlerin filtresi de eşleşir (`06 §7.1`).
  3. Yabancı anahtar ve silme davranışı listesi modelle eşittir; `RESTRICT` yoktur (`06 §7.2`, §2.2.5).
  4. Kontrol kısıtı listesi modelle eşittir (`06 §7.3`).
  5. Sorgu dizinleri modelle eşittir (`06 §7.5`).
  6. Saklanan türetilmiş değerler denormalizasyon envanterindeki dört türle sınırlıdır (`06 §2.7.1`).
  7. Modelde göçe dönüşmemiş değişiklik yoktur (`09 §10.6` madde 11).
  8. Denetimler birim testidir; veritabanına ve ağa dokunmaz (`09 §19.2` madde 2).
- **Test beklentisi:** Unit
- **Doğrulama kontrol listesi:**
  - KK1: 130 çizgi, 130 yabancı anahtar.
  - KK2: tekil dizin tablosundan bir satır silinince test kırılır.
  - KK3: bir bağın davranışı değişince test kırılır.
  - KK4: kontrol kısıtı eksilince test kırılır.
  - KK5: dizin eksilince test kırılır.
  - KK6: envanterde olmayan saklanan alan testi kırar.
  - KK7: göçsüz model değişikliği Migration dry-run'ı düşürür.
  - KK8: testler `UnitTests` altındadır, bağlantı dizesi okumaz.
  - `09 §25` ilgili satırlar; güvenlik taraması

### F2 — Çekirdek servisler

**Yürütme sırası:** T218 → T227 → T226 → T233 → T205 → T225 → T230 → T231 → T232 → T228 → T222 → T220 → T221 → T201 → T202 → T203 → T204 → T223 → T224 → T206 → T211 → T207 → T208 → T209 → T210 → T212 → T213 → T214 → T215 → T216 → T217 → T219 → T229

#### T201: İmha düzenleyicisi, süre ayarları ve gecelik temizlik
- **Bağımlılık:** T221, T226
- **Dokümanlar:** çekirdek; `09` §9; `06 §9.1`, §9.4, §9.5; `02 §4.2` Z-36, Z-44; `05 MK-22`, `MK-23`, §7.4, §7.5
- **Kabul kriterleri:**
  1. İmha işi her gece Türkiye saatiyle 03:00'te çalışır ve süre envanterinin satırlarını sırayla işler (`05 §7.4.1`; MK-22).
  2. Süre kaydın başlangıç değerinden bugüne hesaplanır; kaçırılan gece sonraki çalışmada telafi edilir (`06 §9.1.1`).
  3. Süre değerleri `02 §11`'den okunur; kodda ve tabloda yinelenmez (`06 §9.1.2`).
  4. Silme fiziksel silmedir; "silindi" işaretli satır ve arşiv tablosu yoktur (`05 §7.4.3`; `06 §9.5.1`).
  5. İmha kaydı silmeyle aynı veritabanı işleminde yazılır (`06 §9.4.1`).
  6. Gecelik temizlik aynı çalışmadadır; kişisel veri taşıyan satırın silinmesi imha kaydına tür ve sayıyla yazılır (`06 §9.4.3`).
  7. Kalıcı kayıtlar ile sipariş tarafının ticari kayıtları süreyle silinmez; "başarısız" iş satırı temizlikte silinmez (`06 §9.1.4`, §9.4.4).
  8. İşlem izi satırını yalnız veritabanındaki imha fonksiyonu siler (`05 §7.5.1`).
  9. Süresi dolan kayıt dosyaları aynı çalışmada silinir ve imha kaydına dosya sayısıyla yazılır (`05 §7.4.4`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: zamanlayıcı 03:00 Türkiye anında işi tetikler.
  - KK2: iki gece atlanınca üçüncü gece iki gecenin kaydı silinir.
  - KK3: süre değeri kodda sabit yazılı değil.
  - KK4: silinen satır sorguda yok; işaret sütunu yok.
  - KK5: imha kaydı hata verince silme geri alınır.
  - KK6: temizlik satırı tür ve sayı taşır, değer taşımaz.
  - KK7: kalıcı kayıt ve "başarısız" iş satırı yerinde kalır.
  - KK8: uygulama kullanıcısı iz satırını silemez.
  - KK9: eski kayıt dosyası silinir, sayısı kayda girer.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T202: Dış servislerin bağlantı katmanı: ortak yüz, hata sınıfı, zaman aşımı, sahteler
- **Bağımlılık:** T201
- **Dokümanlar:** çekirdek; `09` §4 (§4.3), §11, §19 (§19.1); `08 §3.1`; `05 MK-17`, `MK-61`, §3.6, §10.1; `02 §8.3.4`
- **Kabul kriterleri:**
  1. Üç dış servis tek bağlantı katmanının arkasındadır; modül ürün düzeyindeki işi çağırır (`08 §3.1` kural 1; `05 §3.6.1`; MK-17).
  2. Uç, alan ve değer adları sağlayıcının adını taşımaz (`08 §3.1` kural 1).
  3. Kullanıcının beklediği çağrı 15 sn, kuyruktan giden 30 sn sürelidir; katman kendiliğinden yeniden denemez (`08 §3.1` kural 2; MK-61).
  4. İstemci kitaplığının kendi yeniden denemesi kapalıdır (`08 §3.1` kural 2).
  5. Zaman aşımı ve yanıtsızlık "sonucu bilinmeyen" çağrıdır (`08 §3.1` kural 3).
  6. Katman bağlantı hatasını ödeme hatasından, alıcı hatasını kanal hatasından ayırır; ürün sonucunu çekirdek verir (`08 §3.1` kural 4).
  7. Yanıtta okunmayan alan kayda girmez; hata kaydına yalnız hata nedeni yazılır (`08 §3.1` kural 5).
  8. Tabloda olmayan yanıt ödemede bağlantı hatası, e-postada kanal hatası, Google'da başarısızdır ve kayda yazılır (`08 §3.1`).
  9. Her servisin sahte sağlayıcısı vardır; geliştirmede e-posta `SmtpCaptureEmailGateway` ile Mailpit'e gider (`09 §4.3`, §19.1; K-1441).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: modül kodu sağlayıcı türüne başvurmaz.
  - KK2: arayüz adlarında sağlayıcı adı yok.
  - KK3: iki süre sınıfı sahte gecikmeyle sınanır.
  - KK4: istemci yeniden deneme ayarı kapalı.
  - KK5: zaman aşımı "bilinmeyen" döner.
  - KK6: bağlantı ve alıcı hataları ayrı sonuç.
  - KK7: kayıtta istek alanı ve anahtar yok.
  - KK8: tanınmayan yanıt sınıf tablosuna göre döner.
  - KK9: sahteler gerçek ağ çağrısı yapmaz; Mailpit'te ileti görünür.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T203: İmzalı makine ucu altyapısı: imza denetimi, gövde tavanı
- **Bağımlılık:** T202
- **Dokümanlar:** çekirdek; `09` §11, §12; `08 §3.2`; `05 MK-17`, `MK-49`, §3.6
- **Kabul kriterleri:**
  1. İmza bağlantı katmanında, işleyiciden ve iş kuyruğundan önce doğrulanır (`08 §3.2` ilke 1).
  2. Hesaplanan imza gelenle sabit zamanlı karşılaştırılır (`08 §3.2` ilke 1).
  3. İmzası tutmayan ya da imzasız istek kayda yazılmaz, işlenmez; 400 ve gövdesiz yanıt alır (`08 §3.2` ilke 2).
  4. Reddedilen istek uygulama kaydına seyreltilmiş güvenlik olayı olarak yazılır (`08 §3.2` ilke 2; `09 §18.2`).
  5. İmzası doğru istek önce kaydedilir; diske kesinleştikten sonra "alındı" der (`08 §3.2` ilke 3).
  6. Aynı bildirimin tekrarı kaydedilmez, işlenmez ve yine "alındı" alır (`08 §3.2` ilke 3).
  7. Kaydedilemeyen bildirim "alındı" almaz; yanıt 500'dür (`08 §3.2` ilke 3).
  8. Gelen kayıt kişisel alan taşımaz (`08 §3.2` ilke 4).
  9. Gövde tavanı 64 KB'tır, aşan istek 413 alır; imzalı uç sahteciliğe karşı korumanın dışındadır (`08 §3.2` ilke 5).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: imza denetimi işleyiciden önce koşar.
  - KK2: karşılaştırma sabit zamanlı işlevle yapılır.
  - KK3: bozuk imzada 400, gövde boş, satır yok.
  - KK4: güvenlik olayı satırı seyreltilmiş yazılır.
  - KK5: kayıt diske kesinleştikten sonra yanıt döner.
  - KK6: tekrar gönderimde ikinci satır yok, yanıt aynı.
  - KK7: veritabanı hatasında 500 döner.
  - KK8: kayıtta kişisel alan yok.
  - KK9: 64 KB üstü gövde 413; sahtecilik denetimi bu uca uygulanmaz.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T204: Dış servis çökmesi davranışı ve sahtelerle sınama
- **Bağımlılık:** T203, T220
- **Dokümanlar:** çekirdek; `09` §19 (§19.1); `08 §3.3`; `03 §10.1`; `05 §10.1`
- **Kabul kriterleri:**
  1. Sahte sağlayıcı zaman aşımı, bağlantı hatası ve beklenmeyen yanıt üretebilir; gerçek servise çağrı yapılmaz (`09 §19.1`).
  2. Ödemeyi başlatma bağlantı hatasında "bağlantı hatası", ikinci jeton reddinde ayrı sonuç döner (`08 §3.1`, §3.3).
  3. Son sorgu yanıtsızlığı "sonucu bilinmeyen" döner; katman siparişe dokunmaz (`08 §3.1` kural 3–4).
  4. E-posta gönderimi alıcı hatasını kanal hatasından ayırır; kanal hatası 5 dk, 30 dk, 2 saatlik deneme anını kurar (`08 §3.3`; `05 §4.4.3`).
  5. Daha uzun kesintide satır "ulaşmadı" olur; toplu yeniden gönderim yoktur (`08 §3.3`).
  6. Google çağrısı yanıtsızsa sonuç "kullanılamıyor" ya da "başarısız"dır (`08 §3.1` tablosu).
  7. Hiçbir süre servise bağlı değildir; kesinti süre saymayı durdurmaz (`08 §3.3`).
  8. Ürün kesinti için müşteriye bildirim üretmez (`08 §3.3`).
- **Test beklentisi:** Integration
- **Doğrulama kontrol listesi:**
  - KK1: sahteler üç hata türünü üretir.
  - KK2: iki ödeme sonucu ayrı sınıfta.
  - KK3: yanıtsız sorguda sipariş satırı değişmez.
  - KK4: deneme anları 5 dk, 30 dk, 2 saat.
  - KK5: son denemeden sonra "ulaşmadı".
  - KK6: Google sonuçları tablodaki sınıfta.
  - KK7: sahte kesintide süre ilerler.
  - KK8: kesintide e-posta satırı üretilmez.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T205: Açılışta veritabanı fail-fast denetimi
- **Bağımlılık:** T233
- **Dokümanlar:** çekirdek; `09` §8; `06 §8.2`
- **Kabul kriterleri:**
  1. Açılışta il listesinin dolu olduğu denetlenir (`06 §8.2.1` madde 1).
  2. Açılışta ülke listesinin dolu olduğu denetlenir (`06 §8.2.1` madde 2).
  3. Altı tek satırlık kaydın varlığı denetlenir: `Company`, `CompanySettings`, `Branding`, `ChannelStateRecord`, `HeartbeatRecord`, `BackupStatusRecord` (`06 §8.2.1` madde 3).
  4. Eksik varsa uygulama açılmaz ve eksik kaydı adıyla yazar; eksiklik yarım kalmış göç sayılır (`06 §8.2.1`).
  5. Denetlenen satırın varlığıdır, değeri değildir (`06 §8.2.1`).
  6. Hakkımızda ve duyuru kaydının yokluğu geçerli hâldir; teslimat illeri ve taslaklar denetlenmez (`06 §8.2.2`).
  7. Denetim beklenmeyen hata kalıbına girer ve ayar denetimiyle aynı açılış adımındadır (`09 §8`; `06 §8.2.3`).
- **Test beklentisi:** Integration
- **Doğrulama kontrol listesi:**
  - KK1: boş il tablosuyla açılış düşer.
  - KK2: boş ülke tablosuyla açılış düşer.
  - KK3: altı kayıttan biri silinince açılış düşer ve adı yazılır.
  - KK4: hata iletisinde kayıt adı var, değer yok.
  - KK5: değeri boş ama satırı olan kayıt açılışı engellemez.
  - KK6: Hakkımızda satırsız açılış geçer.
  - KK7: ayar eksikliği ile veritabanı eksikliği ayrı iletidir.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T206: Adres yapısı, önekler ve sayfa–form işleyici kalıbı
- **Bağımlılık:** T224
- **Dokümanlar:** çekirdek; `09` §5, §8; `07 §2.1`, §2.2, §2.10; `05 MK-04`, `MK-16`, `MK-18`, §3.5, §3.7; `10 KD-20`
- **Kabul kriterleri:**
  1. Adresin yol bölümleri küçük ASCII harf, rakam ve tekli tiredir; kaydın adresi `/{önek}/{Slug}`'dır (`07 §2.1`; K-1162).
  2. Beş önek kapalıdır — `/urun/`, `/kategori/`, `/hizmet/`, `/referans/`, `/sayfa/`; dil öneki yoktur (`07 §2.1.1`).
  3. Sipariş sayfası iki kökten açılır: üyenin numara kökü ve erişim anahtarlı kök (`07 §2.1.3`).
  4. Panel `/panel` yol alanındadır; vitrinde iç anahtar yola girmez, forma ya da sorguya girer (`07 §2.1.5`, §2.1.6).
  5. Gelen yol harf büyüklüğüne ve sondaki eğik çizgiye duyarsız eşlenir, yönlendirme yapılmaz; anahtar değerleri harf duyarlıdır (`07 §2.1.7`).
  6. Bir ekran bir Razor sayfasıdır; her POST adlandırılmış işleyicidir ve `handler` parametresiyle gelir (`07 §2.2`, §2.2.1).
  7. Yazan başarılı POST 303 ile GET'e yönlendirir; yazmayan POST sayfayı gönderilen değerlerle yeniden gösterir (`07 §2.2.2`).
  8. GET hiçbir kaydı değiştirmez; istisnalar kapalı listedir (`07 §2.2.5`).
  9. Değişen statik dosya yeni adres alır; dışarıya açık sürümlü API yoktur (`07 §2.10.1`, §2.10.3).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: Türkçe harfli ad ASCII yol bölümüne iner.
  - KK2: beş önek dışındaki ilk bölüm 404 verir.
  - KK3: iki sipariş kökü ayrı sayfada açılır.
  - KK4: sepet kalemi kimliği yolda yok.
  - KK5: büyük harfli ve eğik çizgili yol aynı sayfayı döner.
  - KK6: işleyici adı `handler` ile eşlenir.
  - KK7: başarılı POST 303, hatalı POST 422 değil yeniden gösterim.
  - KK8: GET sonrası listedeki kayıtlar değişmez.
  - KK9: statik dosya adresi içerikle değişir.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T207: Kimlik şemaları ve erişim reddi
- **Bağımlılık:** T206
- **Dokümanlar:** çekirdek; `09` §15; `07 §2.3`; `02 §1.3`; `05 MK-16`, `MK-45`, `MK-47`, §3.5, §6.1, §6.2, §6.5; `10 KD-18`
- **Kabul kriterleri:**
  1. Panel ve müşteri için iki kimlik şeması vardır; iki hesap türü ayrı tabloda ve ayrı adlı çerezdedir (`07 §2.3.1`; `05 §3.5.2`).
  2. `/panel` altındaki her sayfa panel şemasını ister; açık sayfalar kapalı listedir (`07 §2.3.1`; `05 §3.5.3`).
  3. `/hesap` altı müşteri oturumu ister; tek istisna `/hesap/silindi`'dir (`07 §2.3.1`).
  4. Oturum gerektiren GET kendi girişine 302 ile yönlendirir ve `returnUrl` taşır (`07 §2.3.2`).
  5. Oturumu kapanmış POST yazmaz; girişe 303 ile yönlendirir (`07 §2.3.2`).
  6. `returnUrl` yalnız sitenin kendi yoludur (`05 §6.5.6`).
  7. Sahiplik denetiminden geçmeyen istek ve var olmayan kayıt aynı yanıtı alır — 404 ve E-06; POST hiçbir şey yazmaz (`07 §2.3.3`).
  8. Misafir siparişe yalnız erişim anahtarlı adresle erişir (`07 §2.3.4`).
  9. Yönetici oturumu vitrinde müşteri sayılmaz; yalnız şeridi ve taslak önizlemesini açar (`07 §2.3.5`; `05 §3.5.4`).
- **Test beklentisi:** Integration
- **Doğrulama kontrol listesi:**
  - KK1: iki çerez adı ve iki şema ayrı.
  - KK2: oturumsuz `/panel/siparisler` girişe gider; açık sayfa gitmez.
  - KK3: `/hesap/silindi` oturumsuz açılır.
  - KK4: 302 yanıtında `returnUrl` var.
  - KK5: kapanmış oturumda POST 303 ve yazma yok.
  - KK6: `//evil.example` dönüş adresi reddedilir.
  - KK7: başkasının siparişi ile olmayan sipariş aynı 404'ü verir.
  - KK8: anahtarsız misafir isteği sayfayı açmaz.
  - KK9: yönetici oturumuyla `/hesap` açılmaz.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T208: Sürümün ve gösterilen değerin formda taşınması, koşullu güncelleme
- **Bağımlılık:** T207, T228
- **Dokümanlar:** çekirdek; `09` §10; `07 §2.4`; `02 §3.31.1`; `05 MK-20`, §7.3; `10 KP-76`
- **Kabul kriterleri:**
  1. Kayıt formu kaydı açtığı andaki sürümü gizli alanda `06 §4`'teki sütun adıyla taşır (`07 §2.4.1`).
  2. Yönetici sipariş işlemi (iç not hariç) siparişin görülen sürümünü taşır; müşteri işlemleri sürüm taşımaz (`07 §2.4.2`).
  3. Gösterilen değere karşı yazılan alan `Original` önekli gizli alan taşır (`07 §2.4.3`).
  4. Karşılaştırma sunucunun tek koşullu güncellemesidir (`07 §2.4`; `05 §7.3.1`).
  5. Sürüm değişmişse yazma olmaz; sayfa 409 ile yazılanlarla yeniden gösterilir ve tek düğme "Yine de kaydet"tir (`07 §2.4.5`).
  6. Sipariş varyantında sürüm değişmişse işlem uygulanmaz; güncel hâl 409 ile gösterilir, üzerine yazma yoktur (`07 §2.4.6`).
  7. Sürüm ve gösterilen değer adreste ya da HTTP başlığında taşınmaz (`07 §2.4.7`).
  8. Çift tıklamayla gelen ikinci gönderim birincinin artırdığı sürüme takılır (`07 §2.4.8`).
  9. Sürüm taşımayan kayıtlarda durum değiştiren işlem o anki hâle karşı denetlenir (`07 §2.4.4`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: form çıktısında sürüm gizli alanı var.
  - KK2: sipariş formunda sürüm var, müşteri formunda yok.
  - KK3: `Original…` alanı gösterilen değeri taşır.
  - KK4: eski sürümle güncelleme sıfır satır etkiler.
  - KK5: çakışmada 409 ve "Yine de kaydet" düğmesi.
  - KK6: sipariş çakışmasında yazma yok, güncel hâl var.
  - KK7: adreste ve başlıkta sürüm yok.
  - KK8: çift gönderimde ikincisi çakışır.
  - KK9: kategori güncellemesi sürümsüz çalışır.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T209: Tek kullanımlık değerler, tekrar gönderim, sahteciliğe karşı koruma ve güvenlik başlıkları
- **Bağımlılık:** T208
- **Dokümanlar:** çekirdek; `09` §15; `07 §2.5`; `02 §8.3.2`, §8.3.3, §12.2.5; `05 MK-52`, `MK-53`, `MK-58`, §6.5, §6.8; `10 KD-47`
- **Kabul kriterleri:**
  1. Tekrar gönderimin genel kuralı güncel hâldir; ayrı istek kimliği yoktur (`07 §2.5.1`; `05 §4.5.4`).
  2. Sipariş onayı tek istisnadır: aynı onay anahtarıyla ikinci gönderim yeni sipariş açmaz, var olan siparişin teyit ekranına gider (`07 §2.5.1`; `05 §4.5.1`).
  3. Yeniden doğrulamanın değeri formun gizli alanıdır ve gönderim onu tüketir (`07 §2.5.2`).
  4. Bütün tarayıcı POST'ları çatının sahteciliğe karşı korumasından geçer; parçalı yüklemede değer `RequestVerificationToken` başlığındadır (`07 §2.5.4`; `05 §6.5.1`).
  5. Doğrulama başarısızsa istek yazılmaz; sayfa 400 ve yeni değerle yeniden gösterilir (`07 §2.5.4`).
  6. Korumanın dışındaki uçlar kapalı listedir; yalnız imzalı bildirim bu listededir (`07 §2.5.5`).
  7. Güvenlik başlıkları makine uçları dahil her yanıta uygulanır (`07 §2.5.6`; `05 §6.5.4`; MK-53).
  8. Kişisel veri, oturum, sepet ya da erişim anahtarı taşıyan yanıt `Cache-Control: no-store` taşır (`07 §2.5.6`).
  9. Site yalnız HTTPS'tir ve tarayıcıya bir yıllık HSTS verilir (`05 §6.8.1`; MK-58).
- **Test beklentisi:** Integration
- **Doğrulama kontrol listesi:**
  - KK1: iki kez gönderilen aynı formun etkisi tektir.
  - KK2: aynı onay anahtarı ikinci sipariş açmaz.
  - KK3: değer tüketilince ikinci gönderim yeniden doğrulama ister.
  - KK4: doğrulama değersiz POST reddedilir.
  - KK5: reddedilen formda yazılanlar korunur.
  - KK6: imzalı uç dışında korumasız POST ucu yok.
  - KK7: başlıklar bir makine ucunda da var.
  - KK8: oturumlu yanıtta `no-store` var.
  - KK9: HSTS başlığı bir yıllıktır.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T210: Liste kalıbı
- **Bağımlılık:** T209
- **Dokümanlar:** çekirdek; `09` §10, §20; `07 §2.6`; `10 KP-3`, `KD-32`
- **Kabul kriterleri:**
  1. Liste bir GET'tir; parametre adları sorgu modelinin camelCase alan adlarıdır (`07 §2.6.1`).
  2. `page` birden başlar, birinci sayfa parametresizdir; sayfa boyu parametre değildir (`07 §2.6.2`).
  3. Sayı olmayan ya da birden küçük değer birinci sayfadır; son sayfayı aşan değer son sayfayı gösterir (`07 §2.6.2`).
  4. Sıralama parametresi yoktur; eşit anahtarda sıra iç kimlikle kesinleşir (`07 §2.6.5`).
  5. Geçersiz değer süzgeci uygulamaz ve hata vermez; bilinmeyen parametre yok sayılır (`07 §2.6.6`).
  6. Alt fiyat üst fiyattan büyükse fiyat süzgeci uygulanmaz ve alan mesajı çıkar (`07 §2.6.3`).
  7. Boş ya da yalnız boşluktan oluşan arama sözcüğü sonuç listesi üretmez (`07 §2.6.3`).
  8. `page` dışında parametre taşıyan vitrin listesi ve arama sayfası `noindex` taşır (`07 §2.6.7`).
  9. Panel listesinden kayda bağlantı `returnUrl` taşır; değer yerel yol denetiminden geçer (`07 §2.6.8`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: sorgu modeli alan adı parametre adıyla bağlanır.
  - KK2: `page=1` ile parametresiz aynı sayfa.
  - KK3: `page=abc`, `page=0`, aşırı büyük değer beklenen sayfa.
  - KK4: eşit anahtarlı kayıtlar iç kimlikle sıralı.
  - KK5: geçersiz süzgeç hata vermez.
  - KK6: ters fiyat aralığında mesaj var, süzgeç yok.
  - KK7: boşluk arama sözcüğü boş sonuç değil, boş arama ekranı.
  - KK8: `?minPrice=1` listesinde `noindex` var.
  - KK9: `returnUrl` dış adresi listeye döndürmez.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T211: Model bağlama kalıbı
- **Bağımlılık:** T206
- **Dokümanlar:** çekirdek; `09` §3, §7; `07 §5.1`
- **Kabul kriterleri:**
  1. Üç model türü vardır: form girdi (`…Input`), sorgu (`…Query`), makine ucu gövdesi (`…Request`, `…Response`) (`07 §5.1.1`).
  2. Model adı formun asıl işleyicisinin adı ve türüdür; aynı formu gönderen işleyiciler aynı modeli bağlar (`07 §5.1.5`).
  3. Bağlama yalnız modelin alanlarınadır; modelde olmayan alan yok sayılır (`07 §5.1.3`).
  4. `06`'nın varlıkları doğrudan bağlanmaz (`07 §5.1.3`).
  5. Sunucunun türettiği değer istekten bağlanmaz (`07 §5.1.3`).
  6. Alan adı `06 §4`'teki karşılığının adıdır; kapalı liste değeri `06 §5`'in kod adıdır (`07 §5.1.2`).
  7. Doğrulama sunucuda yetkilidir; HTML nitelikleri iyileştirmedir (`07 §5.1.4`).
  8. Doğrulama hatası 422 ve alan mesajıyla gösterilir (`07 §5.1.4`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: örnek üç model türünün ad kalıbı doğru.
  - KK2: iki işleyici aynı girdi türünü kullanır.
  - KK3: fazladan gelen alan modele girmez.
  - KK4: işleyici imzasında entity türü yok.
  - KK5: fiyat gibi türetilmiş değer formdan okunmaz.
  - KK6: alan adları `06 §4` ile eşit.
  - KK7: HTML'i değiştirilmiş istek sunucuda reddedilir.
  - KK8: hata 422 ve alan mesajı.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T212: Hata gösterimi ve HTTP durumları
- **Bağımlılık:** T210, T211
- **Dokümanlar:** çekirdek; `09` §8; `07 §2.7`, §6.1, §6.4; `03 §3.1`; `05 MK-63`, §10.3
- **Kabul kriterleri:**
  1. Yanıtın durum kodu sonucu söyler; hata kodu kullanıcıya gösterilmez (`07 §2.7`, §2.7.3).
  2. GET başarı 200, başarılı yazan POST 303, oturum gerektiren GET'in girişe yönlendirmesi 302'dir (`07 §2.7.1`).
  3. Doğrulama hatası ve adı konmuş iş kuralı reddi 422'dir ve form değerleriyle yeniden gösterilir (`07 §2.7.1`).
  4. Sürüm çakışması ve gösterilen değerin tutmaması 409'dur (`07 §2.7.1`).
  5. Deneme limiti ve genel hız sınırı 429'dur (`07 §2.7.1`).
  6. Makine ucu ekran göstermez: bildirim 200, 400, 500; canlılık ve durum 200 ya da 503, gövdesiz, `no-store` (`07 §2.7.2`).
  7. Beklenmeyen hata tek yakalama katmanında üç hâlden birine düşer (`05 §10.3`; `09 §8`).
  8. Sonucu belirsiz işlemde yanıt 303 ve güncel hâli gösteren ekrandır; kod taşınmaz (`07 §6.4.2`).
  9. Silinmiş yüklemeyle form kaydı adı konmuş hâldir ve 422 alır (`07 §6.4.1`).
- **Test beklentisi:** Integration
- **Doğrulama kontrol listesi:**
  - KK1: yanıt gövdesinde ve başlığında hata kodu yok.
  - KK2: üç durum kodu yoklanır.
  - KK3: iş kuralı reddinde 422 ve değerler korunur.
  - KK4: çakışma 409.
  - KK5: limit aşımı 429.
  - KK6: makine ucu yanıtları gövdesiz.
  - KK7: kaçan istisna tek katmanda yakalanır.
  - KK8: belirsiz sonuç 303'tür.
  - KK9: silinmiş görselle kayıt 422.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T213: Hata kodu kayıt defteri
- **Bağımlılık:** T212
- **Dokümanlar:** çekirdek; `09` §5, §8; `07 §6.2`
- **Kabul kriterleri:**
  1. Kod İngilizce PascalCase bir addır, uygulamada tekildir ve bir kez verilen kod değişmez (`07 §6.1.1`).
  2. Kodlar tek kayıt defterindedir ve `06 §5`'in kapalı değerlerinin biçimindedir (`07 §6.1.1`).
  3. Aynı hâl birden çok uçta dönüyorsa kod tektir (`07 §6.1.1`).
  4. Her kodun HTTP durumu `07 §2.7.1`'in değeridir (`07 §6.1.2`).
  5. Alan doğrulaması kural türü başına koddur (`07 §6.1.3`).
  6. Kod yanıtın gövdesinde ve başlığında taşınmaz (`07 §2.7.3`).
  7. `02 §6`'nın her satırı bir kodun anlamında ya da hata yanıtı doğurmayan satırlar tablosunda geçer (`07 §6.1.5`).
  8. Kod uygulama kaydına yalnız red satırında yazılır (`09 §18.2` madde 4).
- **Test beklentisi:** Unit
- **Doğrulama kontrol listesi:**
  - KK1: kod adı kalıp testinden geçer; yinelenen ad yok.
  - KK2: tek kayıt defteri sınıfı var.
  - KK3: iki uçta dönen hâl aynı koda gider.
  - KK4: durum sütunu `07 §2.7.1` ile uyumlu.
  - KK5: kural türü başına tek kod var.
  - KK6: yanıt gövdesi testinde kod görünmez.
  - KK7: `02 §6` kapsama betiği temiz.
  - KK8: red dışı satırda kod alanı yok.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T214: Kısa süreli bildirim kodları
- **Bağımlılık:** T213
- **Dokümanlar:** çekirdek; `09` §8; `07 §6.3`
- **Kabul kriterleri:**
  1. `notice` parametresinin kapalı listesi vardır; listenin dışındaki değer yok sayılır (`07 §6.3`).
  2. Kod yönlendirme adresinde taşınır; serbest metin ve kişisel veri taşımaz (`07 §2.2.3`, §6.3).
  3. Bildirim için yeni çerez ve sunucuda geçici kayıt açılmaz (`07 §2.2.3`).
  4. Kısa süreli bildirim ile sonuç hâli ayrı listelerdir; sonuç hâli formun yerinde kalıcıdır (`07 §6.3.2`).
  5. Kodun biçimi `07 §6.1.1`'inkidir (`07 §6.3`).
  6. Başarılı kayıt `Saved`, sepete ekleme `CartItemAdded` koduyla yönlendirir (`07 §6.3.1`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: listede olmayan kod ekranda görünmez.
  - KK2: adreste serbest metin kabul edilmez.
  - KK3: yanıtta ek çerez yok.
  - KK4: sonuç hâli sayfa yenilenince kaybolmaz, bildirim yinelenebilir.
  - KK5: kod adları kalıp testinden geçer.
  - KK6: `CartItemAdded` ve `Saved` yönlendirmeleri beklenen hedefte.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T215: Hız sınırı, deneme limitleri ve gövde tavanı
- **Bağımlılık:** T212, T225
- **Dokümanlar:** çekirdek; `09` §15; `07 §2.8`; `03 §6.1`; `02 §8.3.1`, §11.2 P-31…P-37, P-46; `05 MK-48`, `MK-49`, §6.3; `10 KP-72`
- **Kabul kriterleri:**
  1. Gövde tavanı uç sınıfı başınadır: GET gövde taşımaz, genel form 64 KB, uzun metinli panel formu 1 MB, parça 5 MB, bildirim 64 KB (`07 §2.8.1`).
  2. Tavanı aşan istek 413 alır; gövde okunmadığı için form yeniden gösterilmez (`07 §2.8.7`).
  3. Genel sınır kayan penceredir: altmış saniye, altı dilim; eksen IP, IPv6'da /64 (`07 §2.8.2`).
  4. Statik dosyalar ve görseller sınırın dışındadır; imzası tutmayan bildirim form gönderimi sınırına sayılır (`07 §2.8.3`).
  5. Genel sınırın yanıtı 429'dur: sayfada nötr mesaj, makine ucunda gövdesiz; `Retry-After` yoktur (`07 §2.8.4`).
  6. Deneme limitleri L-1…L-9 işleyicide sayılır, 429 verir ve form değerleriyle yeniden gösterilir (`07 §2.8.5`).
  7. L-7 ve L-8'in e-posta ekseni oturumu açık üyenin hesap e-postasını sayar (`07 §2.8.6`).
  8. Limit ve süre değerleri `02 §11`'den okunur; uçta yeniden yazılmaz (`07 §2.9.7`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: her uç sınıfının tavanı yoklanır.
  - KK2: 64 KB üstü gövde 413, form gösterilmez.
  - KK3: pencere dilimleri sahte saatle sınanır; /64 ekseni.
  - KK4: statik dosya sınıra girmez; bozuk imza sayılır.
  - KK5: 429 sayfası nötr; `Retry-After` yok.
  - KK6: limit aşımında 429 ve değerler korunur.
  - KK7: misafirin yazdığı e-posta eksene girmez.
  - KK8: kodda limit sayısı sabit yazılı değil.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T216: Tarih, sayı ve para biçimi: istek ve gösterim
- **Bağımlılık:** T215
- **Dokümanlar:** çekirdek; `09` §7; `07 §2.9`; `04 §8.1`, §8.2; `02 §4.1.1`, §12.1; `10 KD-20`
- **Kabul kriterleri:**
  1. Sayfanın gösterdiği tarih, saat, sayı, yüzde, tutar, süre, telefon ve IBAN vitrinde ve panelde aynı biçimdedir (`07 §2.9.1`; K-824).
  2. İstek saat taşımaz; tarih alanı Türkiye saatiyle o gündür; saat dilimi yazılmaz (`07 §2.9.2`).
  3. Tarih alanı istekte `yyyy-AA-GG` biçiminde gelir (`07 §2.9.3`).
  4. Tutar ve ondalıklı sayı metin alanıdır; ondalık ayırıcı virgüldür ve sunucu Türkçe kültürle ayrıştırır (`07 §2.9.4`).
  5. Telefon ve IBAN boşluklu ya da boşluksuz gelir; boşluk yok sayılır (`07 §2.9.5`).
  6. Metin alanının baştaki ve sondaki boşluğu yok sayılır; yalnız boşluktan oluşan değer boştur (`07 §2.9.10`).
  7. E-posta, kupon kodu ve stok kodu karşılaştırması Türkçe büyük-küçük harf dönüşümüyle yapılır (`07 §2.9.5`).
  8. Makine ucu gövdeleri bayt sayısını JSON sayısı olarak taşır ve tarih taşımaz (`07 §2.9.6`).
- **Test beklentisi:** Unit
- **Doğrulama kontrol listesi:**
  - KK1: aynı değer vitrinde ve panelde aynı metin.
  - KK2: gece yarısına yakın an doğru güne düşer.
  - KK3: `2026-10-04` ayrıştırılır, başka biçim reddedilir.
  - KK4: `1234,50` ayrıştırılır; "TL" ve binlik ayırıcı kabul edilmez.
  - KK5: boşluklu IBAN boşluksuzla eşit.
  - KK6: yalnız boşluk zorunlu alanda "boş olamaz".
  - KK7: `I`, `İ`, `ı`, `i` beklenen biçime iner.
  - KK8: JSON'da bayt sayısı sayı, tarih yok.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T217: Yapılabilir işlemlerin sunucuda üretimi ve POST işleyicisinde yeniden denetim
- **Bağımlılık:** T216, T207
- **Dokümanlar:** çekirdek; `09` §8; `07 §8.1`; `03 §1.11`
- **Kabul kriterleri:**
  1. Sayfanın GET'i ekranın işlemlerini sunucunun kararından üretir; hangi düğmenin göründüğü yanıtın içindedir (`07 §8.1.1`).
  2. Tarayıcı durumdan işlem türetmez (`07 §8.1.1`).
  3. Her POST işleyicisi aynı kuralı yeniden denetler; siparişte satır kilidi altında, o anki hâle karşı (`07 §8.1.2`).
  4. Düğmenin gizlenmesi ya da etkisiz gösterilmesi denetimin yerine geçmez (`07 §8.1.2`).
  5. İşlem o anda açık değilse uygulanmaz ve hiçbir şey yazılmaz (`07 §8.1.3`).
  6. Sunucunun türettiği değerler sayfadadır: açık adet, varsayılan, üst sınır, işlemin son günü (`07 §8.1.4`).
  7. İşlemin açık olduğu aralık `03 §1.11`'in aralığıdır (`07 §8.1.4`).
  8. Kural tek yerde yazılır; GET ve POST aynı işlevi çağırır (`07 §8.1.2`, §8.1.5).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: kapalı işlemin düğmesi yanıtta yok.
  - KK2: sayfadaki betik işlem kararı vermiyor.
  - KK3: elle gönderilen kapalı işlem POST'u uygulanmaz.
  - KK4: gizli düğmenin POST'u da reddedilir.
  - KK5: reddedilen işlemde veritabanı değişmez.
  - KK6: açık adet ve üst sınır sunucudan gelir.
  - KK7: aralığın son günü sınır testinden geçer.
  - KK8: GET ve POST aynı kural işlevini çağırır.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T218: Dokuz modülün iskeleti ve Contracts
- **Bağımlılık:** —
- **Dokümanlar:** çekirdek; `09` §6, §9; `05 MK-13`, `MK-15`, §3.1, §3.3, §3.4; `10 KD-21`
- **Kabul kriterleri:**
  1. Dokuz modül kendi klasöründe iskelet taşır: Katalog, Sepet ve sipariş, Ödeme, İptal–cayma–iade, Üyelik, Kurumsal içerik, Bildirim, Yönetim ve işlem izi, Ayarlar (`05 §3.3.1`; MK-15).
  2. Ortak altyapı modül değildir ve iş kuralı taşımaz (`05 §3.3.11`).
  3. Her modül yalnız `Contracts` altındaki açık arayüzünü dışa açar; iç türler `internal`'dır (`05 §3.4.1`; MK-13).
  4. Başka modülün tablosuna doğrudan yazılmaz ve doğrudan okunmaz (`05 §3.4.1`).
  5. Modüller arası çağrı eşzamanlıdır ve çağıranın veritabanı işleminin içindedir (`05 §3.4.2`).
  6. Dış yan etki iş kuyruğuna satır olarak yazılır; uygulama içi olay yayını yoktur (`05 §3.4.3`, §3.4.6).
  7. Mimari test, bir modülün başka modülün iç türüne başvurmasında kırılır (`05 §3.4.5`; `09 §6`).
  8. Vitrin ve panel modül değildir; sayfa modül arayüzünü çağırır, veriye doğrudan dokunmaz (`05 §3.3.12`).
  9. Her modül `Program.cs`'e tek kayıt uzantısıyla girer; bu dosya F2'de zincirli değişir (`09 §4.1`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: dokuz modül klasörü ve her birinde `Contracts` var.
  - KK2: ortak altyapı klasörü modül sayılmaz.
  - KK3: `Contracts` dışındaki türler `internal`.
  - KK4: başka modül tablosuna erişim mimari testte kırmızı.
  - KK5: örnek çağrı aynı işlemde yürür.
  - KK6: kodda olay yayıncısı yok; yan etki kuyruk satırı.
  - KK7: ihlal eden örnek proje başvurusu testi düşürür.
  - KK8: sayfa sınıfı `DbContext`'e başvurmaz.
  - KK9: `Program.cs` modül başına tek çağrı içerir.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T219: Arama motoru verileri kalıbı
- **Bağımlılık:** T217
- **Dokümanlar:** çekirdek; `09` §6, §9, §15; `05 MK-18`, §3.7; `10 KP-37`, `KD-48`
- **Kabul kriterleri:**
  1. Adres kaydın adından sunucuda üretilir: Türkçe harfler sadeleşir, dolu adrese sayı eklenir, ilk yayında sabitlenir (`05 §3.7.1`; MK-18).
  2. Sadeleştirme, site içi aramanın ve önerilen stok kodunun kullandığı ortak işlevdir (`05 §3.7.1`).
  3. Sayfa başlığı ve açıklama ilk yanıttadır (`05 §3.7.2`).
  4. Yapılandırılmış veri schema.org sözlüğüyle JSON-LD'dir (`05 §3.7.2`).
  5. Paylaşım önizlemesi Open Graph etiketleriyle verilir; ana görsel yoksa logo kullanılır (`05 §3.7.2`).
  6. Değerler kayıttan üretilir; firmanın elle doldurduğu SEO alanı yoktur (`05 §3.7.2`).
  7. Site haritası istek anında üretilir ve bellekte tutulur; arşivlenmiş ürün ve taslak girmez (`05 §3.7.3`).
  8. Kapalı sayfalar ve panelin bütün sayfaları `noindex` taşır: HTML'de robots meta, HTML olmayanda `X-Robots-Tag` (`05 §3.7.4`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: "Çiçekli Şal" `cicekli-sal` olur; çakışmada sayı eklenir; yayından sonra değişmez.
  - KK2: üç kullanım aynı işlevi çağırır.
  - KK3: başlık ve açıklama ilk HTML'de.
  - KK4: JSON-LD geçerli ve alanlar kayıttan.
  - KK5: görselsiz sayfada logo.
  - KK6: panelde SEO alanı yok.
  - KK7: haritada taslak ve arşivli ürün yok.
  - KK8: panel sayfasında `noindex`.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T220: İş kuyruğu çalıştırıcısı
- **Bağımlılık:** T222
- **Dokümanlar:** çekirdek; `09` §12, §13, §20; `05 MK-37`, `MK-38`, `MK-42`, §4.3, §4.4, §5.1, §5.2; `10 KP-68`
- **Kabul kriterleri:**
  1. Çalıştırıcı uygulama içinde tek arka plan döngüsüdür; işlem bitince uyanır, ayrıca beş saniyede bir bakar (`05 §4.3.2`).
  2. Zamanı gelmiş satırı kilitleyerek alır; aynı satır iki kez alınmaz (`05 §4.3.2`).
  3. Teslim en az bir keredir; işleyici ikinci görüşte sonucu değiştirmez (`05 §4.3.3`).
  4. İç yan etki geçişle aynı veritabanı işleminde, dış yan etki iş satırı olarak aynı işlemde yazılır (`05 §4.3.1`; MK-37).
  5. Yeniden deneme iş türünün kuralıdır; beş yeniden denemeden sonra satır "başarısız" olur (`05 §4.3.4`; MK-42).
  6. "Başarısız" satır silinmez (`06 §9.4.4`).
  7. Gönderilmiş kart iadesi isteği kendiliğinden yeniden denenmez (`05 §4.3.4`).
  8. E-posta ayrı kuyruk satırı yazmaz; gönderim kaydı onun kuyruğudur (`05 §4.4.1`; MK-38).
  9. Dış çağrı veritabanı kilidinin içinde beklenmez (`05 §3.6.3`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: yazma sonrası çalıştırıcı beş saniyeyi beklemeden uyanır.
  - KK2: iki çalıştırıcı aynı satırı almaz.
  - KK3: aynı satır iki kez işlenince sonuç aynı.
  - KK4: geçiş geri alınınca kuyruk satırı da yok.
  - KK5: altı başarısız denemeden sonra durum "başarısız".
  - KK6: temizlik "başarısız" satırı silmez.
  - KK7: gönderilmiş iade isteği tekrar gönderilmez.
  - KK8: e-posta için ayrı satır yok.
  - KK9: işleyici kilit altında dış çağrı yapmaz.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T221: Zamanlayıcı
- **Bağımlılık:** T220
- **Dokümanlar:** çekirdek; `09` §10, §13; `05 MK-41`, `MK-43`, `MK-44`, §5.1–§5.4
- **Kabul kriterleri:**
  1. Zamanlayıcı uygulamanın içinde tektir; dakikada bir uyanıp zamanı gelen işleri tarar (`05 §5.1.1`; MK-41).
  2. Nabız ayrı, otuz saniyelik bir döngüdür; işin süresi nabzı geciktirmez (`05 §5.4.1`; MK-44).
  3. Zamanı gelen kayıt kendi geçişiyle, siparişin kilidi altında işlenir (`05 §5.1.2`).
  4. Her iş türü çalışırken veritabanında kilit tutar; aynı iş aynı anda iki kez çalışmaz (`05 §5.1.3`).
  5. İşin zamanı kendi kaydındadır; ayrı takvim tablosu yoktur (`05 §5.2.1`; `06 §9.1.3`).
  6. Havale hatırlatması son iş gününün 08:00'inde, gecelik iş 03:00'te çalışır; saatler tasarım değeridir (`05 §5.3.1`, §5.3.2, §5.3.5; MK-43).
  7. Dönüşte önce kesinti kaydı yazılır: ilk nabız, kesinti kaydı ve son anların taşınması tek işlemdedir (`05 §5.4.3`).
  8. Süreler kesintide dondurulmaz; kaçırılan işler kaçırdıkları sırayla çalışır (`05 §5.4.2`).
  9. Saat tek kaynaktan okunur (`05 §5.2.14`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: sahte saatle dakika taraması işi bir kez tetikler.
  - KK2: yavaş iş nabzı geciktirmez.
  - KK3: iş siparişin kilidi altında koşar.
  - KK4: iki örnek aynı işi aynı anda çalıştırmaz.
  - KK5: iş zamanı kayıt alanından okunur.
  - KK6: 08:00 ve 03:00 Türkiye anları doğru.
  - KK7: kesinti sonrası kayıt ve ilk nabız tek işlemde.
  - KK8: kaçırılan işler sırayla çalışır.
  - KK9: kodda sistem saati doğrudan okunmaz.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T222: Anahtarlar ve tek kullanımlık bağlantılar
- **Bağımlılık:** T228, T232
- **Dokümanlar:** çekirdek; `09` §15; `05 MK-50`, `MK-51`, §6.4
- **Kabul kriterleri:**
  1. Bütün anahtarlar işletim sisteminin kriptografik rastgele üreteciyle üretilir (`05 §6.4.1`; MK-50).
  2. Hesap bağlantılarının anahtarı, ayardaki türetme sırrından türetilir (`05 §6.4.1`; K-1081).
  3. Veritabanında yalnız anahtarın özeti saklanır (`05 §6.8.2`).
  4. Bağlantının hâli bağlantı kaydından okunur: kayıt yok, ömrü dolmuş, kullanılmış, yerine yenisi geçmiş, geçerli (`05 §6.4.6`).
  5. Bağlantıyı açmak (GET) hiçbir kaydı değiştirmez (`05 §6.4.7`; MK-51).
  6. Tüketim tek koşullu güncellemedir; güncellenen satır yoksa güncel hâl gösterilir (`07 §2.5.3`).
  7. Anahtar taşıyan yol sistem kayıtlarında maskelenir (`05 §6.4.8`).
  8. Açılışta, iş kuyruğundan önce, bekleyen geri alma bildiriminin bağlantı özeti o anki sırla yeniden yazılır (K-1137; K-1441).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: üreteç kodda `RandomNumberGenerator`'dır.
  - KK2: sır değişince üretilen anahtar değişir.
  - KK3: tabloda ham anahtar yok.
  - KK4: beş hâl de ayrı sonuç verir.
  - KK5: GET sonrası oturum anı ve sayaçlar değişmez.
  - KK6: ikinci tüketim sıfır satır etkiler.
  - KK7: kayıtta anahtarlı yol maskeli.
  - KK8: açılış sonrası kuyruk işlenmeden özet yeni sırla yazılmış.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T223: Dosya deposu
- **Bağımlılık:** T203
- **Dokümanlar:** çekirdek; `09` §6, §15, §20; `05 MK-28`, `MK-29`, `MK-54`, §6.6, §7.7; `10 KP-42`
- **Kabul kriterleri:**
  1. Dosyalara dosya deposu arayüzünden erişilir; bugünkü gerçekleme sunucunun diskidir (`05 §7.7.1`; MK-28).
  2. Dosyanın diskteki adı rastgeledir; yükleyenin adı yalnız görünen addır ve yol olmaz (`05 §6.6.4`).
  3. Görselin türü içeriğinden okunur; yalnız P-22'nin biçimleri kabul edilir (`05 §6.6.1`; MK-54).
  4. Görsel sunucuda yeniden kodlanır; en uzun kenar 2560 pikseli aşmaz (`05 §7.7.2`; MK-29).
  5. Vitrinin genişliklerinde WebP türevleri üretilir (`05 §7.7.2`).
  6. En uzun kenarı 10.000 pikseli ya da 40 megapikseli aşan görsel çözülmeden reddedilir (`05 §6.6.2`).
  7. Çözülemeyen dosya reddedilir (`05 §6.6.1`).
  8. Dijital ürün dosyasında tür sınırı yoktur; dosya indirilecek ek olarak sunulur; boyut tavanı P-26'dır (`05 §6.6.3`).
  9. Hiçbir bağı kalmamış ve bir günden eski görsel imha işinde silinir (`05 §7.7.2`; `06 §9.1`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: modül dosyaya yalnız arayüzden erişir.
  - KK2: disk adı rastgele, yükleyen adı yol değil.
  - KK3: uzantısı sahte görsel türünden reddedilir.
  - KK4: büyük görsel 2560'a iner.
  - KK5: türev genişlikleri üretilir.
  - KK6: devasa boyutlu küçük dosya reddedilir.
  - KK7: bozuk dosya reddedilir.
  - KK8: dijital dosya ek olarak gider, tavan aşımı reddedilir.
  - KK9: bağsız eski görsel imhada silinir.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T224: Çerez politikası
- **Bağımlılık:** T223
- **Dokümanlar:** çekirdek; `09` §5, §15; `05 MK-55`, `MK-56`, `MK-57`, §6.7; `10 KP-73`, `KD-15`
- **Kabul kriterleri:**
  1. Bütün çerezler birinci taraf ve zorunludur: Secure, HttpOnly, SameSite=Lax (`05 §6.7.1`; MK-55).
  2. Çerezler `__Host-` önekli ve kendi alan adına kilitlidir (`05 §6.7.1`).
  3. Çerez envanteri kapalıdır: iki oturum çerezi, form güvenliği, misafir sepeti, tanınan tarayıcı, ödeme adımı, Google akışı (`05 §6.7`; `02 §12.2.5`).
  4. Misafir sepeti çerezi 400 gün ömürlüdür ve her ziyarette yenilenir (`05 §6.7.5`).
  5. Ödeme adımında yazılanlar oturumluk, şifreli ve imzalı çerezde durur; sipariş onaylanınca silinir (`05 §6.7.9`; MK-56).
  6. Çerez politikası ilk kez yayına alınana kadar vitrin çerez yazmaz; ara katman çerez başlıklarını yanıttan çıkarır (`05 §6.7.10`; MK-57).
  7. Çerez süzgeci yalnız vitrinin yanıtlarına uygulanır (`05 §3.5.5`).
  8. Google akış çerezinin yolu köktür; `__Host-` öneki onu kapsar (`05 §6.7.12`).
- **Test beklentisi:** Integration
- **Doğrulama kontrol listesi:**
  - KK1: her çerezde üç nitelik var.
  - KK2: çerez adları `__Host-` ile başlar.
  - KK3: envanter dışı çerez adı testi kırar.
  - KK4: sepet çerezi ömrü ve yenilenmesi.
  - KK5: ödeme çerezi şifreli; onayda silinir.
  - KK6: politika yayında değilken yanıtta `Set-Cookie` yok.
  - KK7: panel yanıtı süzülmez.
  - KK8: akış çerezi yolu `/`.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T225: Sistem kayıtları
- **Bağımlılık:** T205, T226
- **Dokümanlar:** çekirdek; `09` §18; `05 MK-64`, `MK-65`, §9.1, §9.2; `10 KP-74`, `KD-58`
- **Kabul kriterleri:**
  1. Sistem kayıtları üç kaynaktır ve sunucu diskinde günlük dosyalarda durur (`05 §9.1.1`; MK-64).
  2. Her isteğe bir istek kimliği verilir ve kayıt satırları onu taşır (`05 §9.1`; `09 §18.2`).
  3. Şifre, anahtar, sır, kart verisi, IBAN, e-posta, adres, telefon, form gövdesi ve çerez değeri hiçbir kayda, hiçbir düzeyde yazılmaz (`05 §9.1.5`; §6.8.3).
  4. Kayıt düzeyi tasarım değeridir ve ayar dosyasında değişir; çatı kategorileri uyarıda kalır (`05 §9.1.6`; `09 §18.1` madde 4).
  5. Kayıt dosyaları yazıldığı andan P-45 kadar saklanır ve yalnız imha işi siler (`05 §9.2.1`, §9.2.2; MK-65).
  6. İmha işi silinen kayıt dosyalarını imha kaydına tür ve dosya sayısıyla yazar (`05 §9.2.2`; `06 §9.1`).
  7. Ayrı metrik toplama ve gösterge panosu yoktur (`05 §9.1.7`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: üç kaynak ayrı dosyaya yazar.
  - KK2: aynı isteğin satırlarında aynı istek kimliği.
  - KK3: örnek satırlarda yasaklı alan yok.
  - KK4: düzey ayarı uygulama kategorisini değiştirir, çatıyı değiştirmez.
  - KK5: süresi dolmamış dosya silinmez.
  - KK6: imha kaydı dosya sayısını taşır.
  - KK7: metrik paketi bağımlılıkta yok.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T226: Ayar katmanları, tasarım değerleri ve açılışta ayar denetimi
- **Bağımlılık:** T227
- **Dokümanlar:** çekirdek; `09` §3 (§3.5), §6, §15; `05 MK-71`, §8.4, §11.1; `10 KP-64`
- **Kabul kriterleri:**
  1. Her değerin tek evi vardır; ürün parametreleri üç katmandadır ve katmanlar karışmaz (`05 §8.4.1`; MK-71; `02 §11`).
  2. Ürün sabitleri kodda, tasarım değerleri `09 §3.5` tablosundaki yerinde, kurulum ayarları ayar dosyası ve ortamdadır (`05 §8.4`; `09 §3.5`).
  3. Sır olmayan ayar tek dosyada, sırlar ayrı yerde durur (`05 §8.4.7`).
  4. Açılışta bütün ayarlar doğrulanır; zorunlu ayar eksikse uygulama açılmaz ve eksiğin adını yazar (`05 §8.4.6`).
  5. Zorunlu ayarlar: alan adı, gönderen adresi, e-posta gönderim kimliği, veritabanı bağlantısı, türetme sırrı (`05 §8.4.6`; `06 §8.2.3`).
  6. Tasarım değerleri referans sunucu ölçüsünde geçerlidir ve sistem onları uygulamaz (`05 §11.1.1`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: aynı değer iki yerde tanımlı değil.
  - KK2: örnek değer her katmanda beklenen yerde.
  - KK3: sır ayar dosyasında yok.
  - KK4: zorunlu ayar eksikken uygulama açılmaz.
  - KK5: hata iletisi eksik ayarın adını taşır, değerini taşımaz.
  - KK6: tasarım değeri aşımı çalışmayı durdurmaz.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T227: Zaman ve para
- **Bağımlılık:** T218
- **Dokümanlar:** çekirdek; `09` §7, §14; `05 MK-19`, §7.2
- **Kabul kriterleri:**
  1. Zaman UTC'de saat dilimli damga olarak saklanır; sayım ve gösterim Türkiye saatiyledir (`05 §7.2.1`; MK-19).
  2. Uygulama zamanı tek bir kaynaktan okur; testte o kaynak değiştirilir (`05 §7.2.4`).
  3. Gün olarak belirlenen süre başlangıç gününün ertesi günü başlar ve son günün 23:59'unda dolar (`05 §7.2.2`).
  4. İş günü takvimi ürünle gelen veri dosyasıdır; kapsamadığı yılda yalnız hafta sonu sayılır (`05 §7.2.3`).
  5. Para ondalıklı sayıyla tutulur; kayan noktalı tür yoktur (`05 §7.2.5`).
  6. Tek bir kuruşa yuvarlama işlevi vardır: en yakın kuruş, tam yarım yukarı (`05 §7.2.6`; `02 §3.8.3`).
  7. Parçalara orantılı bölme kalan kuruşu kayıpsız dağıtır (`05 §7.2.6`).
- **Test beklentisi:** Unit
- **Doğrulama kontrol listesi:**
  - KK1: yaz saati sınırında bir an UTC'ye ve geri doğru çevrilir.
  - KK2: sahte saatle süre testi yazılabilir.
  - KK3: 23:59 sınırı ve ertesi gün başlangıcı.
  - KK4: kapsam dışı yılda hafta sonu dışında tatil yok.
  - KK5: kodda `double` ve `float` para yok.
  - KK6: 0,005 yukarı, 0,004 aşağı yuvarlanır.
  - KK7: bölünen parçaların toplamı tutara eşit.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T228: Eşzamanlılık denetimi
- **Bağımlılık:** T232
- **Dokümanlar:** çekirdek; `09` §9, §10; `05 MK-20`, `MK-21`, §7.1, §7.3; `10 KP-76`
- **Kabul kriterleri:**
  1. Kayıt sürüm taşır; yazma yalnız sürüm aynıysa olur ve sürümü artırır — tek koşullu güncelleme, arada okuma yok (`05 §7.3.1`; MK-20).
  2. Kayıt varyantında kilit yoktur (`05 §7.3.1`).
  3. Sipariş sürümü siparişin, kalemlerinin, kalem kayıtlarının ve bağlı müşteri girdilerinin her değişikliğinde artar; artırmayan sistem yazımları kapalı listedir (`05 §7.3.2`; `06 §2.6.4`).
  4. Her sipariş işlemi siparişin satırını kısa kilitle alır ve koşulu o anki hâle karşı denetler (`05 §7.3.2`; MK-21).
  5. Ayırma koşullu güncellemeyle korunur: yalnız kalan adet yetiyorsa yazılır (`05 §7.3.3`).
  6. Panelin stok yazımı ayırmanın tuttuğu adedin altına inmez (`05 §7.3.3`).
  7. Kilit sırası: sipariş satırları (kimliğe göre), varyantlar (kimliğe göre), kupon en sonda (`05 §7.3.5`).
  8. Sipariş onayı, öteki kilitlerden önce sepetin danışma kilidini alır (`05 §7.3.5`).
  9. Dış çağrı veritabanı kilidinin içinde beklenmez (`05 §3.6.3`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: eski sürümle güncelleme sıfır satır etkiler.
  - KK2: kayıt güncellemesi kilit almaz.
  - KK3: sipariş alt kaydı değişince sürüm artar.
  - KK4: iki eşzamanlı sipariş işleminden biri bekler.
  - KK5: son adet iki işlemde yalnız birine ayrılır.
  - KK6: panel stoğu ayrılan adedin altına inmez.
  - KK7: ters sıralı kilit isteyen kod testte yakalanır.
  - KK8: aynı sepetin iki onayı seri işler.
  - KK9: kilit altında dış çağrı yok.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T229: Uygulama belleğinde önbellek
- **Bağımlılık:** T219
- **Dokümanlar:** çekirdek; `09` §10, §20; `05 MK-34`, §7.10
- **Kabul kriterleri:**
  1. Önbellek uygulamanın belleğindedir; ayrı önbellek sunucusu yoktur (`05 §7.10.1`; MK-34).
  2. Önbelleğe menü, kategori ağacı, ayarlar, ana sayfa blokları ve site haritası girer (`05 §7.10.1`).
  3. İlgili kayıt panelden kaydedilince, işlem kesinleştikten sonra önbellek hemen geçersiz kılınır (`05 §7.10.1`).
  4. Araya giren eski okumanın önbelleğe yazılmasını kuşak sayacı önler (`05 §7.10.1`).
  5. Zamana bağlı görünürlük önbellekteki kayda karşı her istekte hesaplanır (`05 §7.10.1`).
  6. Fiyat, stok ve sepet her istekte veritabanından okunur; satış kapısının hâli her istekte hesaplanır (`05 §7.10.1`, §4.1.5).
  7. Sayfanın tamamı önbelleğe alınmaz (`05 §7.10.1`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: bağımlılıklarda ayrı önbellek istemcisi yok.
  - KK2: beş öğe önbellekten gelir.
  - KK3: kayıt sonrası bir sonraki okuma yeni değeri verir.
  - KK4: eski okuma yarışında bayat değer yazılmaz.
  - KK5: aralığı biten duyuru önbellekte olsa da gizlenir.
  - KK6: fiyat değişince anında yansır.
  - KK7: sayfa çıktısı önbelleğe alınmaz.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T230: Uygulama kayıtlarının üç yazıcısı ve satır biçimleyicisi
- **Bağımlılık:** T225
- **Dokümanlar:** çekirdek; `09` §18 (§18.1, §18.2), §4 (§4.1); `05 §9.1`
- **Kabul kriterleri:**
  1. Kütüphane Serilog'dur; tek yapılandırma noktası `Infrastructure/Logging/LoggingSetup`'tır (`09 §18.1` madde 1).
  2. Üç yazıcı üç dosyaya yazar; günlük dosya günü adından okunur (`09 §18.1` madde 2).
  3. Dosya sayısı ve boyut sınırı kapalıdır; dosyayı yalnız imha işi siler (`09 §18.1` madde 3).
  4. Düzey ayarı çatı ve veri erişim kategorilerini indirmez; denetim ve komut yazıcısı düzeyden bağımsızdır (`09 §18.1` madde 4).
  5. Denetim yazıcısı hata atarsa hata çağırana döner; normal yazıcı istek düşürmez (`09 §18.1` madde 6, madde 8).
  6. Her satır tek JSON nesnesidir; alan adları camelCase'tir; biçimleyici `ApplicationLogFormatter`'dır (`09 §18.2` madde 1).
  7. Olay adları kapalı `LogEvents` listesidir; serbest metin `event` alanına yazılmaz (`09 §18.2` madde 3).
  8. Red satırı yalnız red olan istek için yazılır ve hata kodunu taşır (`09 §18.2` madde 4).
  9. Anlık silme ve komut satırları yalnız iç kimlik taşır (`09 §18.2` madde 5).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: yapılandırma tek yerde; modül Serilog'a başvurmaz.
  - KK2: üç dosya ayrı ayrı oluşur.
  - KK3: otuz birinci dosyada eski dosya silinmez.
  - KK4: ayar bilgiye inince çatı satır yazmaz.
  - KK5: yazılamayan dizinde denetim yazıcısı hata verir.
  - KK6: her ortak alan adı ve türü beklenen.
  - KK7: listede olmayan olay adı derlemede ya da testte reddedilir.
  - KK8: alan doğrulama reddi tek satır, alanlar dizisiyle.
  - KK9: silme ve komut satırında alan değeri yok.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T231: Maskelemenin üç katmanının testleri
- **Bağımlılık:** T230
- **Dokümanlar:** çekirdek; `09` §18 (§18.2), §19; `05 §9.1.5`
- **Kabul kriterleri:**
  1. Yazım katmanı: kayıt çağrısı değeri değil iç kimliği yazar; şablonda kişisel veri adlı özellik yoktur (`09 §18.2` madde 2a).
  2. Kaynak taraması `ILogger` şablonlarında yasaklı alan adlarını (`Email`, `Phone`, `Address`, `Password`, `Token`, `Key`, `Secret`, `Iban`, `Card`) bulursa kırılır (`09 §18.2` madde 2a).
  3. Yol katmanı: `PathMasker.Mask` anahtarlı yolu maskeler; sorgu dizgesi değerleri `*` olur (`09 §18.2` madde 2b, madde 7).
  4. Anahtarlı her yol için tabloda bir satır vardır; üyenin sipariş yolu ve anahtarsız yol değişmez (`09 §18.2` madde 7).
  5. Biçimleyici katmanı: istisna iletisinde ve iç istisnanın iletisinde kişisel dizgi geçmez (`09 §18.2` madde 2c, madde 7).
  6. `ExceptionMasker` istisna iletisini kayda yazmadan önce temizler (`09 §18.2`).
  7. Her ortak alan adı ve türü biçimleyici testinde doğrulanır (`09 §18.2` madde 7).
- **Test beklentisi:** Unit
- **Doğrulama kontrol listesi:**
  - KK1: örnek çağrı kimlik yazar, adres yazmaz.
  - KK2: yasaklı ada sahip sahte şablon taramayı kırar.
  - KK3: `/siparis/erisim/ABC123/teyit?email=a@b.com` maskelenir.
  - KK4: tablo her anahtarlı yolu kapsar.
  - KK5: `a@b.com` içeren istisna kayıtta yok.
  - KK6: iç istisna iletisi de temiz.
  - KK7: alan adı ya da türü bozulunca test kırılır.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T232: Veri koruma halkası
- **Bağımlılık:** T230
- **Dokümanlar:** çekirdek; `09` §15 (§15.5); `05 §7.6.3`; `06 §4.68`
- **Kabul kriterleri:**
  1. Halka kurulumu tektir: `DataProtectionSetup.Add`; uygulama adı sabittir ve anahtarlar veritabanında durur (`09 §15.5` madde 1).
  2. Amaç dizgeleri tek yerdedir: `DataProtectionPurposes`; altı amaç kapalı listedir (`09 §15.5` madde 2).
  3. `CreateProtector` yalnız bu sabitlerle çağrılır; dizge sabiti başka yerde yazılmaz (`09 §15.5` madde 2).
  4. Çözme istisna fırlatmaz: `ProtectedValues.Unprotect` `Valid` ya da `Invalid` döner (`09 §15.5` madde 3).
  5. Boş, bozuk ya da başka amaçla korunmuş değer `Invalid`'dir ve hiç gelmemiş sayılır (`09 §15.5` madde 3).
  6. Bir amaçla korunan değer öteki beş amaçta çözülmez (`09 §15.5` madde 7).
  7. Biçim değişirse amaç soneki artar; eski değer çözülemez (`09 §15.5` madde 2).
  8. Anahtar halkası yedeğin kapsamındadır (`05 §7.6.3`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: halka tek noktada kurulu; anahtar `DataProtectionKey` tablosunda.
  - KK2: altı amaç sabiti ve biçimi `Shopfolio.<Kullanım>.v1`.
  - KK3: kaynak taraması başka yerde amaç dizgesi bulmaz.
  - KK4: bozuk değer istisna atmaz.
  - KK5: boş, bozuk, yanlış amaçlı değer `Invalid`.
  - KK6: 6×5 çapraz çözme çifti reddedilir.
  - KK7: `v2` amaçlı çözme eski değeri reddeder.
  - KK8: yedek kapsamı anahtar tablosunu içerir.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T233: Veritabanı oturum süreleri
- **Bağımlılık:** T226
- **Dokümanlar:** çekirdek; `09` §10 (§10.7), §3 (§3.5)
- **Kabul kriterleri:**
  1. Süre değerleri `Infrastructure/DatabaseTimeouts`'ta adlı sabitlerdir; değerin tek yeri `09 §3.5` tablosudur (`09 §10.7` madde 1).
  2. Bağlantı kuruluşu tek yerdedir: `DatabaseConnection.Configure` üç sunucu ayarını ve istemci süresini kurar (`09 §10.7` madde 2).
  3. İki profil vardır: `Web` ve `Maintenance`; göç ve yönetim komutları `Maintenance` kullanır (`09 §10.7` madde 2).
  4. İstemci süresi her profilde o profilin sunucu sürelerinden uzundur; komut başına süre atanmaz (`09 §10.7` madde 3).
  5. Kilit aşımı, sorgu iptali ve boşta işlem aşımı beklenmeyen hâle düşer; kod onları yakalamaz, çevirmez, yeniden denemez (`09 §10.7` madde 4).
  6. Veri sağlayıcının otomatik yeniden deneme stratejisi kapalıdır (`09 §10.7` madde 4).
  7. Bilgim tarihli `Options` ve `CommandTimeout` adları Npgsql sürümüne karşı doğrulanır (`09 §10.7`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: sabitler `09 §3.5` satırlarıyla bire bir.
  - KK2: tek kuruluş noktası; başka yerde bağlantı dizgesi kurulmuyor.
  - KK3: Maintenance profili daha uzun sorgu süresi taşır.
  - KK4: istemci süresi sunucu süresinden uzun.
  - KK5: aşım testi beklenmeyen hata yanıtına düşer.
  - KK6: yeniden deneme stratejisi etkin değil.
  - KK7: doğrulanan ad farkı PR açıklamasında.
  - `09 §25` ilgili satırlar; güvenlik taraması

### F3 — İş mantığı

**Yürütme sırası:** T313 → T314 → T378 → T330 → T331 → T303 → T304 → T305 → T366 → T306 → T307 → T308 → T309 → T328 → T312 → T332 → T333 → T334 → T336 → T337 → T338 → T339 → T341 → T342 → T379 → T380 → T381 → T389 → T343 → T344 → T345 → T301 → T302 → T388 → T310 → T315 → T321 → T335 → T340 → T316 → T346 → T347 → T349 → T350 → T354 → T355 → T357 → T356 → T317 → T324 → T318 → T325 → T319 → T348 → T376 → T377 → T351 → T364 → T311 → T387 → T352 → T329 → T353 → T320 → T323 → T367 → T368 → T369 → T370 → T371 → T372 → T375 → T386 → T373 → T374 → T322 → T326 → T327 → T358 → T359 → T360 → T361 → T362 → T363 → T365 → T382 → T383 → T384 → T385 → T390

#### T301: Siparişin imha parçası
- **Bağımlılık:** —
- **Dokümanlar:** çekirdek; `09` §9; `06 §9.2`; `02 §4.2` Z-28, Z-41; `05 MK-23`, §7.4
- **Kabul kriterleri:**
  1. Parça tek veritabanı işlemidir; ödemesi alınmış siparişte Z-28'in, hiç alınmamış siparişte Z-41'in kişisel veri süresi sonunda çalışır (`06 §9.2.1`).
  2. Parça siparişin satır kilidini alır, kilitten sonra koşulu yeniden okur ve siparişin sürümünü artırır (`06 §9.2.1`).
  3. Parça yalnız henüz imha edilmemiş, yani `ContactEmail` dolu siparişi seçer (`06 §9.2.1`).
  4. Siparişin kişisel alanları — iletişim, adres, hesap ve sepet bağı, IP — birlikte boşalır; ticari alanlar yerinde kalır (`06 §9.2.1` tablo).
  5. Bağlı kayıtlarda yönetici kimliği ve adı, serbest not ve IBAN bağı boşalır (`06 §9.2.1` tablo).
  6. İç not, e-posta ve adres geçmişi, IBAN istekleri ve duran IBAN silinir (`06 §9.2.1` ikinci tablo).
  7. Ödeme, geri ödeme, onay kutusu ve yasal metin kayıtlarına dokunulmaz (`06 §9.2.2`).
  8. Hiç ödemesi alınmamış sipariş kaydı kişisel verisiz kalır ve silinmez (`06 §9.2.3`).
  9. Parça imha kaydına `Order` türüyle ve sayıyla yazılır; silecek kaydı olmayan parça satır yazmaz (`06 §9.2.1`, §9.3.1).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: iki süre ayrı anda tetiklenir; parça tek işlemdir.
  - KK2: yarışan panel işlemi sürüm uyarısı alır.
  - KK3: imha edilmiş sipariş ikinci kez seçilmez.
  - KK4: kontrol kısıtı birlikte boşalmayı korur.
  - KK5: yönetici adı ve not boşalır.
  - KK6: silinen bağlı kayıt sorguda yok.
  - KK7: ödeme ve geri ödeme satırları aynı.
  - KK8: ödemesiz sipariş satırı kalır, kişisel alanı yok.
  - KK9: imha kaydında tür ve sayı var.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T302: Öteki kayıtların süreyle imhası
- **Bağımlılık:** T301
- **Dokümanlar:** çekirdek; `09` §9; `06 §9.3`; `02 §4.2` Z-29, Z-30, Z-33, Z-34, Z-40, Z-43, Z-44; `05 MK-23`, §7.4, §7.7
- **Kabul kriterleri:**
  1. Her parça kendi veritabanı işleminde küçük parçalar hâlinde siler ve imha kaydına satırını yazar (`06 §9.3.1`).
  2. Gönderim kaydı siparişe bağlı değilse Z-34'te, bağlıysa Z-43'ün kuralına göre silinir ya da boşaltılır (`06 §9.3.1`).
  3. İletişim talebi konu tipinin süresi dolunca, açıkken de silinir (`06 §9.3.1`).
  4. Giriş kaydı Z-40'ta, imha kaydı Z-44'te silinir; kendi silmesi de satırdır (`06 §9.3.1`).
  5. İşlem izini yalnız imha fonksiyonu parça parça siler ve döndürdüğü sayı yazılır (`06 §9.3.3`).
  6. Gelen bildirim ve ayıp talebi on yıl sonra, siparişin imha parçasından önce olmamak üzere silinir (`06 §9.3.1`).
  7. Bağı kalmamış ve bir günden eski görsel ve dosya silinir; önce satır, sonra dosya (`06 §9.3.1`; `05 §7.7`).
  8. Bağlı görsel ve açılabilen kalemin dosyası silinmez (`06 §9.3.2`).
  9. Dosya silen parça satırını dosyayı silmeden önce yazar (`06 §9.3.4`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: bir parçanın hatası ötekileri geri almaz.
  - KK2: bağlı ve bağsız gönderim satırı ayrı kurala girer.
  - KK3: açık iletişim talebi süresi dolunca silinir.
  - KK4: imha kaydının silinmesi yeni satır yazar.
  - KK5: uygulama kullanıcısı iz satırını silemez.
  - KK6: sipariş imhadan önce talep ve bildirim kalır.
  - KK7: bağsız görsel ve dosya silinir.
  - KK8: bağlı görsel ve dosya yerinde.
  - KK9: dosya silinmeden önce satır diskte.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T303: İlk yönetici yönetim komutu ve komut çatısı
- **Bağımlılık:** —
- **Dokümanlar:** çekirdek; `09` §13 (§13.2), §15 (§15.1), §18 (§18.1); `06 §8.3`; `05 MK-72`, §8.5; `03 §10.4`; `02 §10.2.7`
- **Kabul kriterleri:**
  1. `Program.cs` ilk argüman `admin` ise komut kipine geçer; komut kipinde zamanlayıcı, nabız, iş kuyruğu ve imha işi çalışmaz (`05 §8.5.1`; `09 §13.2` madde 1).
  2. Komutlar tek statik listede kayıtlıdır; bilinmeyen komut ya da seçenek çıkış kodu 2 döner ve iletide argümanın değeri yoktur (`09 §13.2` madde 2).
  3. `--help` komut listesini ve seçenekleri yazar; listesi `05 §8.5` tablosuyla eşittir (`09 §13.2` madde 3).
  4. Komutlar modüllerin `Contracts` arayüzünü çağırır ve `Maintenance` veritabanı profilini kullanır (`05 §8.5.1`; `09 §13.2` madde 6).
  5. `create-admin` adı, e-postayı ve şifreyi terminalden ister; şifre yankısızdır ve iki kez sorulur (`06 §8.3.1`; `05 §8.5.2`).
  6. Şifre politikasını web'in kullandığı aynı işlev uygular ve hata iletisi girilen değeri yazmaz (`09 §13.2` madde 4).
  7. Girdi yönlendirilmişse kişisel veri komutu hiçbir satır yazmadan 2 döner (`05 §8.5.8`; `09 §13.2` madde 4).
  8. Yönetici doğrulanmış doğar; rol ataması yoktur (`06 §8.3.1`, §8.3.2).
  9. Komut işlem izine yazılmaz, bildirim üretmez; adı komuttan önce kendi kayıt dosyasına yazılır ve ad ile e-posta içermez (`05 §8.5.7`; `02 §10.2.7`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: komut kipinin hizmet koleksiyonunda `IHostedService` yok.
  - KK2: yanlış komutta 2 döner, iletide değer yok.
  - KK3: `--help` ile tablo satırları eşit.
  - KK4: komut `Maintenance` profiliyle bağlanır.
  - KK5: şifre ekranda görünmez, farklı iki girişte reddedilir.
  - KK6: zayıf şifre reddedilir, ileti değer taşımaz.
  - KK7: `stdin` boruda komut 2 döner.
  - KK8: açılan hesap doğrulanmış, rolsüz.
  - KK9: işlem izinde satır yok, kayıt dosyasında kişisel veri yok.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T304: Alıcı başına bildirim gönderim sınırı
- **Bağımlılık:** —
- **Dokümanlar:** çekirdek; `09` §11; `08 §2.2.4`; `02 §9.1`, §9.1.6; `06 §2.8.1`, §4.58
- **Kabul kriterleri:**
  1. Sınır yalnız müşteri bildirimlerinin (B-1…B-16) alıcı adresi başınadır; firma ve hesap e-postaları sayılmaz (`08 §2.2.4` Sayılmayanlar).
  2. Sayaç saklanmaz; gönderim kaydından, satırın oluşma anından önceki 24 saatte oluşmuş satırlar okunur (`08 §2.2.4` Sayaç).
  3. Alıcı karşılaştırması harf ayrımsızdır ve yalnız siparişe bağlı satırlar sayılır (`08 §2.2.4` Sayaç).
  4. Eşik 24 saatte 20'dir; satırın kendisi ve sonradan oluşanlar sayılmaz (`08 §2.2.4` Eşik).
  5. Aşım yüzünden gönderilmeyen satır da sayılır; pencere kaydıkça düşer (`08 §2.2.4` Sayaç).
  6. Aşan satır ilk denemede `Undelivered` ve `RecipientError` olur; servise çağrı yapılmaz ve kanal hâli etkilenmez (`08 §2.2.4` Aşım).
  7. Sipariş, ödeme ve durum geçişi sınırdan etkilenmez; siparişin satırına "e-posta ulaşmadı" işareti düşer (`08 §2.2.4`; `02 §9.1.2`).
  8. Panelden yeniden gönderim ve düzeltme sonrası B-1 sınırdan muaftır ama sayılır (`08 §2.2.4` Muafiyet).
  9. Sınır ilk denemede bir kez sorulur, yeniden denemelerde sorulmaz; aşım güvenlik olayı olarak adressiz yazılır (`08 §2.2.4` Kayıt).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: F ve hesap e-postası sayaca girmez.
  - KK2: sayaç tabloda saklı değil, kayıttan okunur.
  - KK3: büyük ve küçük harfli adres aynı sayılır.
  - KK4: yirmibirinci satır aşar, yirminci geçer.
  - KK5: aşım satırı sayıya girer; pencere kayınca düşer.
  - KK6: aşımda sahte servise çağrı yok, kanal hâli aynı.
  - KK7: sipariş durumu değişmez, işaret düşer.
  - KK8: yeniden gönderim geçer ve sayılır.
  - KK9: yeniden denemede sınır sorulmaz; kayıtta adres yok.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T305: E-posta ortak kalıbı ve gövde üretimi
- **Bağımlılık:** —
- **Dokümanlar:** çekirdek; `09` §11; `08 §2.2.5`; `02 §6.1`, §9.1.1–§9.1.4; `05 MK-37`, `MK-38`, §4.4
- **Kabul kriterleri:**
  1. E-posta HTML ve düz metin parçalıdır; düz metin eşdeğerdir ve bağlantılar tam adrestir (`08 §2.2.5` madde 1).
  2. HTML tek sütundur, en çok 600 piksel; betik, dış stil, form ve izleme pikseli yoktur (`08 §2.2.5` madde 1).
  3. Başlıkta logonun kaynak kopyası mutlak adresle bağlanır; yoksa marka adı yazılır ve alt metin marka adıdır (`08 §2.2.5` madde 2).
  4. Konu *"{marka adı}: {başlık}"*'dır; siparişe bağlılarda sipariş numarası eklenir ve konuda müşteri metni yoktur (`08 §2.2.5` madde 3).
  5. Marka adı satır sonu ve denetim karakterlerinden arındırılır (`08 §2.2.5` madde 3).
  6. Bağlantı tam `https` adrestir; misafirde erişim anahtarlı sipariş bağlantısı, üyede hesabın sipariş adresi (`08 §2.2.5` madde 4).
  7. Tarih, tutar ve sayı `04 §8.2` biçimindedir ve gönderim anında olayın kaydından üretilir (`08 §2.2.5` madde 5).
  8. Yeniden gönderimde konu aynıdır; gövdenin üstünde ilk bildirimin tarihli tek cümlesi durur (`08 §2.2.5` madde 6).
  9. Altbilgide unvan, iletişim e-postası ve yanıt cümlesi vardır; abonelik bağlantısı ve pazarlama ifadesi yoktur (`08 §2.2.5` madde 8).
- **Test beklentisi:** Unit
- **Doğrulama kontrol listesi:**
  - KK1: iki parça üretilir, düz metin tam adres taşır.
  - KK2: HTML'de betik ve izleme pikseli yok.
  - KK3: logosuz kurulumda marka adı metni çıkar.
  - KK4: konu biçimi ve sipariş eki doğru.
  - KK5: satır sonlu marka adı konuda tek satır.
  - KK6: misafir ve üye bağlantıları ayrı.
  - KK7: "1.200 TL" ve "4 Ekim 2026" biçimi.
  - KK8: yeniden gönderim cümlesi ilk satırın tarihini taşır.
  - KK9: altbilgi içeriği doğru, abonelik yok.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T306: Müşteri bildirimleri B-1…B-8
- **Bağımlılık:** T304, T305
- **Dokümanlar:** çekirdek; `09` §11; `08 §2.2.6`; `03 §7.1`; `02 §3.33.6`, §6.1, §7.2, §7.3, §9.2 B-1…B-8; `10` KP-66
- **Kabul kriterleri:**
  1. Her bildirim bir `DispatchType` değeridir (`B1`…`B8`); gövde iskeleti ortaktır ve hitapta ad yoktur (`08 §2.2.6` madde 1).
  2. Sipariş numarası konuda ve olayın cümlesindedir; ana düğme misafir ve hesabı silinmiş siparişte erişim anahtarlı, üyede hesap adresidir (`08 §2.2.6` madde 2, 3).
  3. B-1 sipariş oluşunca ve misafir e-postası düzeltilince gider; iki yasal metin siparişe donan sürümleriyle, 12 puntodan küçük olmayan tam metin olarak gelir, dosya eki yoktur (`08 §2.2.6` B-1 ve yerleşim).
  4. B-2 siparişe donan IBAN'ı, sipariş numarasını ve son ödeme gününü taşır (`08 §2.2.6` B-2).
  5. B-3 süre başına bir kez, son iş gününün başında gider; kesintide zamanı gelen site dönünce gönderilir (`08 §2.2.6` B-3).
  6. B-4 kart onayında ve "ödendi" işaretinde gider; dijital kalemde indirme cümlesi sipariş sayfasına işaret eder (`08 §2.2.6` B-4).
  7. B-5 kargo bloğunu şirket türüne göre yazar; takip bağlantısı yalnız listedeki şirkette ve `noreferrer`'lidir (`08 §2.2.6` B-5; `08 §2.4`).
  8. B-6 firma iletişimini metinde yazar (`08 §2.2.6` B-6, madde 6).
  9. B-7 müşteri, yönetici ve sistem söyleyişini ayırır; yönetici sebebi aynen ve on dört gün cümlesiyle yazar (`08 §2.2.6` B-7).
  10. B-8 nedeni, tutarı ve yöntemi yazar; IBAN yazılmaz, geç ödemede ayrı cümle gelir (`08 §2.2.6` B-8).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: sekiz tür kapalı listede; hitapta ad yok.
  - KK2: misafir ve üye bağlantısı ayrı.
  - KK3: yasal metinler tam, ek yok.
  - KK4: B-2 donan IBAN'ı yazar.
  - KK5: B-3 süre başına bir.
  - KK6: dijital kalemde indirme cümlesi.
  - KK7: üç kargo biçimi ayrı.
  - KK8: B-6'da telefon ve e-posta.
  - KK9: yönetici sebebi ve on dört gün cümlesi.
  - KK10: B-8'de IBAN yok.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T307: Müşteri bildirimleri B-9…B-16
- **Bağımlılık:** T304, T305
- **Dokümanlar:** çekirdek; `09` §11; `08 §2.2.6`; `03 §7.1`; `02 §6.1`, §7.1, §7.2, §7.3, §7.4.9, §8.3, §9.2 B-9…B-16, §10.4; `10` KP-66
- **Kabul kriterleri:**
  1. B-9 dört başlıktan birini taşır (cayma, gecikme feshi, ayıp talebi, yeniden açma); cayma ve fesihte yalnız tarih, ayıp talebinde tarih ve saat yazılır (`08 §2.2.6` B-9).
  2. B-10 yeni adresi yazar, eski adresi yazmaz ve firma iletişimini taşır (`08 §2.2.6` B-10).
  3. B-11 kalemi ve adedi yazar, tutar yazmaz (`08 §2.2.6` B-11).
  4. B-12 düzeltilmiş kargo bloğunu taşır (`08 §2.2.6` B-12).
  5. B-13 durum adını kapalı listeden yazar; sebebi yazmaz (`08 §2.2.6` B-13).
  6. B-13 "ödendi" düzeltmesinde yeni son ödeme gününü ve IBAN bloğunu ekler (`08 §2.2.6` B-13).
  7. B-14 altı olay için tek bildirimdir; IBAN taşımaz, yanıtla IBAN göndermeme cümlesini yazar (`08 §2.2.6` B-14).
  8. B-15 eski adrese gider; yeni adres, ad, adres bilgisi, bağlantı ve düğme yoktur (`08 §2.2.6` B-15).
  9. B-16 kalemi, ambalaj sebebini, geri ödeme yapılmayacağını ve geri gönderimi yazar (`08 §2.2.6` B-16).
  10. Yeniden gönderim ilk e-postanın olay değerleriyle ve yeniden gönderim cümlesiyle gider (`08 §2.2.6` madde 5).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: dört konu başlığı ve tarih biçimi doğru.
  - KK2: eski adres gövdede yok.
  - KK3: B-11'de tutar yok.
  - KK4: düzeltilen takip bilgisi yeni.
  - KK5: durum adı listede, sebep yok.
  - KK6: ödendi düzeltmesinde yeni gün ve IBAN.
  - KK7: altı olay cümlesi, IBAN ve güvenlik cümlesi.
  - KK8: B-15'te bağlantı ve düğme yok.
  - KK9: B-16 dört parça tam.
  - KK10: yeniden gönderimde değerler ilk olayınki.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T308: Firma bildirimleri F-1…F-6
- **Bağımlılık:** T305
- **Dokümanlar:** çekirdek; `09` §11; `08 §2.2.7`; `03 §7.1`; `02 §3.32.5`, §9.3, §10.1, §10.2; `10` KP-34, KP-67
- **Kabul kriterleri:**
  1. F-1…F-4 firma iletişim e-postasına gider; F-5 ve F-6 her yönetici hesabının kendi adresine ayrı satırla gider (`08 §2.2.7`).
  2. Konuda sipariş soneki yoktur; gövde müşterinin kişisel verisini taşımaz (`08 §2.2.7` ortak kurallar).
  3. Ana düğme `/panel/…` ekranına iner; iç anahtar olayın değerindedir; oturum yoksa giriş sonrası ekrana yönlenir (`08 §2.2.7`).
  4. Yeniden gönderim cümlesi yoktur; ulaşmayan F-1…F-4 satıra işaret düşürmez, F-5 ve F-6 yeniden denenir (`08 §2.2.7`).
  5. F-1 yeni siparişin tarihini, numarasını, ödeme yöntemini ve toplamını yazar (`08 §2.2.7` F-1).
  6. F-2 yalnız konu tipini yazar; talep metni, ad ve e-posta yoktur (`08 §2.2.7` F-2).
  7. F-3 dört başlıktan birini taşır: cayma, gecikme feshi, ayıp talebi, yeniden açma (`08 §2.2.7` F-3).
  8. F-4 nedeni ve tutarı yazar ve işlem gerekmediğini söyler (`08 §2.2.7` F-4).
  9. F-5 yeni IBAN'ın yalnız son dört hanesini yazar; yönetici adı ve haneler olayın değeri olarak satıra yazılır (`08 §2.2.7` F-5).
  10. F-6 davet ve kaldırmayı ayrı konu ve gövdeyle yazar; adlar ve adres olayın değeridir (`08 §2.2.7` F-6).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: F-5 her yöneticiye ayrı satır yazar.
  - KK2: konuda sipariş yok; gövdede kişisel veri yok.
  - KK3: bağlantı panel adresine iner.
  - KK4: ulaşmayan F-1'de işaret yok, F-5 yeniden denenir.
  - KK5: F-1 dört değeri taşır.
  - KK6: F-2 gövdesinde mesaj yok.
  - KK7: dört F-3 konusu ayrı.
  - KK8: F-4 tutar ve neden yazar.
  - KK9: F-5 satırında tam IBAN yok.
  - KK10: yönetici silinse de F-6 değerleri satırda.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T309: Hesap ve yönetim e-postaları
- **Bağımlılık:** T305
- **Dokümanlar:** çekirdek; `09` §11; `08 §2.2.8`; `03 §7.1`; `02 §3.15.1`, §6.5, §9.4, §10.2; `10` KP-66, KP-67
- **Kabul kriterleri:**
  1. Yedi e-posta kapalı listededir; konuda sipariş soneki ve hitapta ad yoktur (`08 §2.2.8` ortak kurallar).
  2. Bağlantı anahtarı gövde üretilirken kayıt kimliğinden türetilir; ham anahtar satırda durmaz (`08 §2.2.8`).
  3. Gönderim anında geçersizleşmiş bağlantının e-postası gönderilmez (`08 §2.2.8`).
  4. Bağlantı işlem yapmaz; metin onay ekranı ve düğme cümlesini söyler (`08 §2.2.8`).
  5. Yeniden gönderim cümlesi yoktur; ulaşmama işareti yalnız yönetici davetinde düşer (`08 §2.2.8`).
  6. Müşteri ve yönetici hesabı aynı metni kullanır; yol `/…` ya da `/panel/…`'dır (`08 §2.2.8`).
  7. Bağlantı ömrü metinde değeriyle yazılır ve olayın anından işler; değer `02 §11`'dedir (`08 §2.2.8`).
  8. Yeni adres doğrulaması yeni adrese, değişiklik bildirimi eski adrese gider; bildirim yeni adresi yazmaz ve geri alma bağlantısı taşır (`08 §2.2.8`).
  9. Yönetici daveti davetlinin adresine gider ve davet edenin adını yazar; silinme bildirimi silinenleri ve saklananları sayar (`08 §2.2.8`).
  10. Google teyit bağlantısı yalnız müşteri hesabına gider; geçerli bağlantı varken yenisi üretilmez (`08 §2.2.8`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: yedi tür kapalı listede.
  - KK2: satırda ham anahtar yok.
  - KK3: geçersiz bağlantının e-postası gitmez.
  - KK4: onay ekranı cümlesi her gövdede.
  - KK5: yeniden istek yeni satır ve yeni bağlantı.
  - KK6: yönetici hesabında `/panel/` yolu.
  - KK7: ömür metinde, olay anından sayılır.
  - KK8: bildirim yeni adresi yazmaz.
  - KK9: davet satırına ulaşmama işareti düşer.
  - KK10: ikinci Google girişinde e-posta gitmez.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T310: Kargo şirketi listesi ve takip bağlantısı kalıpları
- **Bağımlılık:** —
- **Dokümanlar:** çekirdek; `09` §3 (§3.1); `08 §2.4`; `02 §3.20.7`, §3.20.8; `06 §5.3.12`
- **Kabul kriterleri:**
  1. Şirket listesi ve takip kalıpları kodda durur; panelden düzenlenmez (`08 §2.4`; `06 §8.4.1`).
  2. Kod adı ASCII, tek kelime ve kalıcıdır; başka şirkete verilmez (`08 §2.4`; `06 §5.3.12`).
  3. Kalıp `https` adrestir ve tek `{takipNumarasi}` yer tutucusu taşır (`08 §2.4`).
  4. Takip numarası yüzde kodlanır; ondan başka adres parçası türetilmez (`08 §2.4` alan eşlemesi).
  5. "Takip et" yalnız listedeki şirket seçilmiş ve numara girilmişse görünür (`08 §2.4`).
  6. "Diğer", kendi aracımızla teslim ve listeden çıkmış şirkette bağlantı yoktur; ad ve numara görünür (`08 §2.4`).
  7. Listeden çıkan şirket yeni kargo işaretinde seçilemez, eski siparişte adıyla çözülür (`08 §2.4`).
  8. Bağlantı yeni sekmede açılır ve `noreferrer` taşır (`08 §2.4`).
  9. Ürün kargo şirketine sunucudan istek göndermez (`08 §2.4`).
- **Test beklentisi:** Unit
- **Doğrulama kontrol listesi:**
  - KK1: liste kodda; ayar ve tablo yok.
  - KK2: kod adları ASCII ve tekil.
  - KK3: her kalıpta tek yer tutucu, `https`.
  - KK4: boşluklu numara kodlanır.
  - KK5: numarasız şirkette bağlantı yok.
  - KK6: üç durumda bağlantı yok, numara var.
  - KK7: çıkmış şirket seçilemez, eski sipariş çözülür.
  - KK8: bağlantıda `noreferrer`.
  - KK9: giden istek yok.
  - `09 §25` ilgili satırlar; güvenlik taraması


#### T311: Ödeme sağlayıcısının bildirim ucu: imza, alan eşlemesi ve durum makinesine aktarım
- **Bağımlılık:** T317
- **Dokümanlar:** çekirdek; `09` §11, §12; `07 UC-80`; `08 §2.1.5`; `03 §2.5`; `02 §5.6`, §6.1.2, §9.2 B-4; `05 MK-39`, §4.5
- **Kabul kriterleri:**
  1. Uç `POST /sistem/odeme-bildirimi`'dir; imza ve tutar bağlantı katmanında doğrulanır (`07 UC-80`; `08 §2.1.5`).
  2. İmzası tutmayan istek 400 alır, kaydedilmez, işlenmez; güvenlik olayı seyreltilmiş yazılır (`07 UC-80`).
  3. İmzası doğru bildirim gelen bildirim kaydına yazılıp diske kesinleştikten sonra 200 `OK` alır (`07 UC-80`).
  4. Aynı bildirimin tekrarı kaydedilmez, işlenmez ve yine 200 alır (`07 UC-80` Idempotency).
  5. Tutarı ya da siparişi tutmayan, türü tanınmayan ya da çözümlenemeyen bildirim işaretli kaydedilir, işlenmez ve 200 alır (`07 UC-80`).
  6. Bildirim kaydı ve işleyecek iş satırı aynı veritabanı işleminde yazılır; işleme kuyruktandır (`07 UC-80` Yan etkiler).
  7. Ödeme sonucu durum makinesine tek yoldan aktarılır; uç siparişin durumunu kendisi yazmaz (`07 UC-80`; `05 §4.5.3`).
  8. Gövde tavanı 64 KB'tır (413); imzalı uç sahteciliğe karşı korumanın dışındadır (`07 UC-80`).
  9. Kaydedilemeyen bildirim 500 alır; hata yanıtlarının gövdesi boştur (`07 UC-80`).
  10. İptal edilmiş siparişe gelen ödeme geri ödenir ve firmaya F-4 gider (`07 UC-80` Yan etkiler).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: bozuk imza ya da tutar bağlantı katmanında düşer.
  - KK2: 400'de satır yok, olay seyreltilmiş.
  - KK3: yanıt kayıt diske inmeden dönmez.
  - KK4: ikinci gönderimde tek satır, yanıt aynı.
  - KK5: tutar uyuşmazlığı işaretli kayıt ve 200.
  - KK6: kayıt ve iş satırı birlikte geri alınır.
  - KK7: sipariş yalnız kuyruk işinde değişir.
  - KK8: 64 KB üstü 413; sahtecilik denetimi yok.
  - KK9: veritabanı hatasında 500, gövde boş.
  - KK10: iptal edilmiş siparişte iade ve F-4 satırı.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T312: Dosya uçları: imzalı indirme ucu ve parçalı yükleme ucu
- **Bağımlılık:** —
- **Dokümanlar:** çekirdek; `09` §15 (§15.5); `07 UC-37`, `UC-83`, §5.4; `03 §8.1`; `02 §3.12.5`, §3.12.7, P-26; `05 MK-30`, `MK-31`, §7.7
- **Kabul kriterleri:**
  1. İndirme ucu imzalı değerle açılır; oturum ve sipariş kökü aranmaz (`07 UC-37`).
  2. Ömrü dolmuş, imzası tutmayan ya da kalemi indirilemez değer 404 alır (`07 UC-37`).
  3. 200 yanıtı ek olarak iner, genel ikili türdedir ve `no-store` taşır; `Range` isteği 206 alır (`07 UC-37`).
  4. Ömür yalnız isteğin kabulünde denetlenir; indirme yeni hak düşürmez (`07 UC-37`).
  5. Başlatma yeni dosya satırı açar ve 201 döner; geçersiz ad ya da boy 400 alır (`07 UC-83`).
  6. Bildirilen boy P-26'yı ya da disk eşiğini aşarsa 422 döner ve gövde sınırı taşır (`07 UC-83`).
  7. Parça yalnız alınan baytın ardındaysa kabul edilir; uymayan parça 409 alır ve alınan baytı söyler (`07 UC-83`).
  8. Parça 5 MB'ı aşarsa 413, `Content-Range` yoksa ya da toplamı aşarsa 400 alır (`07 UC-83`).
  9. Son parça aynı işlemde yüklemeyi tamamlar; durum isteği alınan baytı ve tamamlandığını söyler; bilinmeyen yükleme 404 alır (`07 UC-83`).
  10. Yükleme işlem izine yazılmaz; HTML olmayan yanıtlar `X-Robots-Tag: noindex` taşır (`07 UC-83`; `07 §4.32.2`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: değer imzasız ya da oturumsuz açılır.
  - KK2: üç 404 nedeni ayrı sınanır.
  - KK3: başlıklar ve 206 doğru.
  - KK4: ikinci indirme sayacı değiştirmez.
  - KK5: başlatma satırı açar, 400 yollarında satır yok.
  - KK6: 422 gövdesinde sınır var.
  - KK7: eşzamanlı iki parçadan biri 409 alır.
  - KK8: 413 ve iki 400 gövdesiz.
  - KK9: çökme sonrası son parça tamamlar.
  - KK10: izde satır yok; başlık var.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T313: Site haritası ve robots.txt
- **Bağımlılık:** —
- **Dokümanlar:** çekirdek; `09` §14; `07 UC-79`; `02 §3.30.5`, §3.30.6; `05 §3.7`
- **Kabul kriterleri:**
  1. `/sitemap.xml` ve `/robots.txt` istek anında üretilir ve 200 döner (`07 UC-79`).
  2. Site haritası yayındaki ürünleri, kategorileri, içerik sayfalarını ve sabit liste sayfalarını taşır (`07 UC-79`; `02 §3.30.5`).
  3. Arşivlenmiş ürün, taslak ve arama motorlarına kapalı sayfa haritaya girmez (`07 UC-79`).
  4. Adresler küçük harfli tek biçimdedir (`07 UC-79`).
  5. `/robots.txt` yalnız site haritasının adresini kurulumun alan adıyla gösterir; kapalı sayfaları engellemez (`07 UC-79`).
  6. Kapalı sayfalar HTML'de robots meta etiketiyle kapatılır (`07 §4.32.2`; `02 §3.30.6`).
  7. HTML olmayan yanıtlar `X-Robots-Tag: noindex` taşır (`07 §4.32.2`).
  8. Arşivlenmiş ürün sayfası doğrudan bağlantıyla açılır ve `noindex`'tir (`07 §4.32.2`).
  9. İçerik bellek önbelleğinden gelir; ilgili kayıt kaydedilince önbellek geçersiz olur (`07 UC-79`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: iki adres 200 döner.
  - KK2: dört liste haritada.
  - KK3: arşiv, taslak ve kapalı sayfa yok.
  - KK4: adreslerde büyük harf yok.
  - KK5: robots.txt tek satır site haritası, `Disallow` yok.
  - KK6: hesap ve panel sayfasında meta etiketi.
  - KK7: indirme yanıtında başlık.
  - KK8: arşiv ürün sayfası açılır, `noindex`.
  - KK9: ürün kaydı önbelleği düşürür.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T314: Durum ucu: uyarı koşulları ve 500 sayacı
- **Bağımlılık:** —
- **Dokümanlar:** çekirdek; `09` §18; `07 UC-82`; `05 MK-62`, `MK-67`, §9.4, §10.2
- **Kabul kriterleri:**
  1. Uç `GET /sistem/durum`'dur; uyarı koşulu yoksa 200, varsa 503 döner; gövde boştur ve `no-store` taşır (`07 UC-82`).
  2. Hangi koşulun açık olduğu yanıtta yoktur; uç imzasız açıktır (`07 UC-82`).
  3. Disk doluluğu %85'i, işlem günlüğü aktarımının on dakikadır durması ya da on dosyadan fazla beklemesi koşuldur (`05 §9.4.3`, §9.4.4).
  4. Gecelik yedeğin son başarılısı yirmi altı saatten eskiyse koşul açılır (`05 §9.4.5`).
  5. Son nabız beş dakikadan eskiyse koşul açılır (`05 §9.4.6`).
  6. E-posta kanal uyarısı açıksa koşul açılır (`05 §9.4.7`).
  7. Son on beş dakikada en az on beklenmeyen hata koşuldur; yalnız 500 sayılır, 422, 409 ve 429 girmez (`05 §9.4.8`; `07 UC-82`).
  8. Ödeme başlatmada art arda beş bağlantı hatası ve sitenin sertifikasına on dört günden az kalması koşuldur; ilk başarılı jetonda hata dizisi kalkar (`05 §9.4.9`, §9.4.18).
  9. Zamanı gelmiş en eski satır on beş dakikadan eskiyse ya da "başarısız" satır varsa; gecelik imha yirmi altı saatten eskiyse koşul açılır (`05 §9.4.16`, §9.4.17).
  10. Eşikler kurulum ayar dosyasından okunur; uyarı koşulları canlılık ucuna girmez (`05 §9.4.2`, §9.4.15).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: koşulsuzken 200, koşulda 503, gövde boş.
  - KK2: yanıtta koşul adı yok.
  - KK3: disk ve aktarım eşikleri sahte ölçümle sınanır.
  - KK4: yirmi altı saatlik yedek eşiği.
  - KK5: nabız beş dakika eşiği.
  - KK6: kanal uyarısı açık ve kapalı.
  - KK7: dokuz hata kapalı, on hata açık; 409 sayılmaz.
  - KK8: hata dizisi ilk başarıda kalkar; sertifika eşiği.
  - KK9: başarısız iş satırı açar; imha eşiği.
  - KK10: eşik ayarı değişir; canlılık ucu 200 kalır.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T315: Durum makinesi çekirdeği: envanter, geçiş tablosu ve kimlikleri, durum makinesi olmayan varlıkların sınırı
- **Bağımlılık:** —
- **Dokümanlar:** çekirdek; `09` §9; `03 §1.1`, §1.10; `02 §5.3`, §5.4, §5.5; `05 MK-35`, §4.1
- **Kabul kriterleri:**
  1. Altı durum seti — sevkiyat ekseni, ödeme ekseni, ürün ve varyant yayını, içerik yayını, ayıp talebi, iletişim talebi — kodda tek enum'dur (`03 §1.1`).
  2. Sipariş durum makinesi Sepet ve sipariş modülünde tek geçiş biriminde yaşar; durum alanlarına yalnız bu birim yazar (`05 §4.1.1`; MK-35).
  3. İzinli geçişler tek tabloda durur ve S1…S11, Ö1…Ö5 kimlikleriyle adlıdır; tabloda olmayan geçiş reddedilir (`05 §4.1.1`; `03 §1.2`).
  4. Panel, vitrin ve arka plan işi geçişi adıyla ister; her geçişin tetikleyen aktör türü tabloda yazılıdır (`05 §4.1.1`).
  5. Geçiş sırası: satır kilidi, koşulun siparişin o anki hâline karşı denetimi, yazım, geçiş kaydı (`05 §4.1.2`).
  6. Geçiş kaydı yalnız eklenir ve kim, ne zaman, hangi geçiş, sebep taşır; kalem kaydı geçiş kaydına satır yazmaz (`05 §4.1.2`).
  7. Açık kalemi kalmayan siparişin kapanışı aynı işlemin sonunda değerlendirilir (`05 §4.1.3`).
  8. Sipariş kalemi, indirim, kupon, stok ayırma, hesap, oturum, davet ve satış açıklığı durum makinesi taşımaz (`03 §1.1`, §1.10).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: altı enum'un değerleri `02 §5` ile eşit.
  - KK2: durum alanına birim dışından yazım derlenmez.
  - KK3: tabloda olmayan geçiş hata verir.
  - KK4: her geçişin aktör türü var.
  - KK5: yarışan iki geçişten biri sürüm uyarısı alır.
  - KK6: geçiş kaydı güncellenemez, kalem kaydı satır yazmaz.
  - KK7: son kalem kapanınca sipariş aynı işlemde kapanır.
  - KK8: varlıklarda durum alanı yok.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T316: Sipariş durumu geçişleri: sevkiyat ekseni
- **Bağımlılık:** T315
- **Dokümanlar:** çekirdek; `09` §9; `03 §1.2`, §2.5, §8.2; `02 §5.3.2`, §5.4, §5.6, §7.2, §9.2 B-6; `10` KP-46
- **Kabul kriterleri:**
  1. S1…S11 beyaz listedir; geri yönlü geçiş ve listede olmayan geçiş yasaktır (`03 §1.2`).
  2. Teslim edildi ve İptal edildi terminaldir; yalnız yöneticinin düzeltmesiyle çıkılır (`03 §1.2`).
  3. S1 ödeme onayıyla aynı anda ve yalnız en az bir fiziksel kalemli siparişte işler; B-4 gider (`03 §1.2` S1).
  4. S2, S10 ve S11 kapanış kuralını tamamlayan son olayla işler; ayrı tetikleyen yoktur (`03 §1.2`).
  5. S3, S4 ve S9 iptaldir; müşteri, sebepli yönetici ve sistem tetikleyebilir; B-7 gider (`03 §1.2`).
  6. Kapanışla işleyen S3, S4 ve S9'da sebep seçilmez ve B-7 gitmez (`03 §1.2`).
  7. S5 ve S8 yalnız yöneticidir, geri alınamaz onayı ister ve yalnız açık fiziksel kalemleri gönderir; B-5 gider (`03 §1.2`).
  8. S6 yöneticinin teslim işareti ve tarihidir; kendiliğinden geçiş yoktur, bildirim yoktur (`03 §1.2` S6).
  9. S7 yöneticinin işaretidir; B-6 gider (`03 §1.2` S7).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: tablo dışı ve geri yönlü geçiş reddedilir.
  - KK2: terminal durumdan çıkış yok.
  - KK3: dijital kalemli sipariş S1'e girmez.
  - KK4: son olay kapanışı geçişi doğurur.
  - KK5: üç tetikleyen ayrı sınanır, B-7 gider.
  - KK6: kapanışta sebep ve B-7 yok.
  - KK7: onaysız S5 reddedilir.
  - KK8: S6'dan sonra kendiliğinden geçiş yok.
  - KK9: S7'de B-6 satırı yazılır.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T317: Ödeme durumu geçişleri: ödeme ekseni
- **Bağımlılık:** T315
- **Dokümanlar:** çekirdek; `09` §9; `03 §1.3`, §2.5; `02 §3.21.8`, §5.3.2, §5.5, §5.6.3, §6.1.2; `10` KP-18
- **Kabul kriterleri:**
  1. Ö1…Ö5 beyaz listedir; Başarısız ve Geri ödendi terminaldir (`03 §1.3`).
  2. Ö1 kartta sağlayıcının başarı bildirimiyle ya da süre dolunca son sorgunun olumlu cevabıyla işler (`03 §1.3` Ö1; `02 §6.1.2`).
  3. Ö1 havalede yöneticinin "ödendi" işaretidir ve geri alınamaz onayı ister; B-4 gider (`03 §1.3` Ö1).
  4. Ö2, S3'le aynı anda işler: ödenmemiş siparişin her iptali, kim yaparsa yapsın (`03 §1.3` Ö2).
  5. Ö3, Ö4 ve Ö5 sistemin kart iadesi sağlayıcıda gerçekleşince ya da yöneticinin geri ödemeyi işlemesiyle işler (`03 §1.3`).
  6. Geri ödemede B-8 müşteriye gider; sistemin kendiliğinden kart iadesinde F-4 firmaya gider (`03 §1.3`).
  7. Kart iadesi başarısız ya da sonucu bilinmiyorsa eksen değişmez, panel işareti düşer ve müşteriye bildirim gitmez (`03 §1.3`).
  8. İptal edilmiş siparişe gelen kart ödemesi ödeme kaydında durur; sistem onu geri öder (`03 §1.3`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: tablo dışı geçiş reddedilir, terminal durum kalkmaz.
  - KK2: başarı bildirimi ve son sorgu Ö1'i işler.
  - KK3: onaysız "ödendi" reddedilir.
  - KK4: iptalde S3 ve Ö2 aynı işlemdedir.
  - KK5: kısmi iadede Ö3, kalan iadede Ö5.
  - KK6: B-8 ve F-4 doğru alıcıya yazılır.
  - KK7: yanıtsız iadede eksen aynı, işaret düşer.
  - KK8: iptal edilmiş siparişin ödemesi iade edilir.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T318: İki eksenin bağı, tipe göre hatlar ve manuel adım bütçesi
- **Bağımlılık:** T316, T317
- **Dokümanlar:** çekirdek; `09` §9; `03 §1.4`, §1.5, §8.2, §8.5; `02 §5.3`, §5.6, §5.7, §10.5; `10` KP-46, KP-51
- **Kabul kriterleri:**
  1. Birleşim tablosu kodda durur; ulaşılamaz çifti üreten geçiş reddedilir (`03 §1.4.1`; `02 §5.6.1`).
  2. Teslim edildi ile Bekliyor ve Başarısız birleşimi yasaktır (`03 §1.4.1` (d)).
  3. Hazırlanıyor'a yalnız ödeme onaylanınca geçilir; ödeme Bekliyor'a yalnız havale işaretinin düzeltilmesiyle döner (`03 §1.4.1` (c)).
  4. Eşzamanlı geçişler tek işlemde yazılır: Ö1 ile S1 ya da S2, S3 ile Ö2, havale düzeltmesi (`03 §1.4.2`).
  5. Kapanışta en az bir kalem teslim edilmişse Teslim edildi, hiçbiri edilmemişse İptal edildi olur (`03 §1.4.3`).
  6. Kargoya verildi ve Teslim edilemedi'de sipariş gönderinin dönüşünü bekler; kart son sorgusu yanıtsızken iptal ertelenir (`03 §1.4.3`).
  7. Kalem adetle açık sayılır; kapanışta iptal kaydı ve ikinci geri ödeme açılmaz (`03 §1.4.3`).
  8. Geçişin stok, kontenjan ve kupon hakkına etkisi an tablosuyla uyar (`03 §1.4.4`).
  9. Yedi hattın olağan dizisi ve zorunlu elle adım sayısı tabloyla uyar; karışık hatta adımlar toplanır (`03 §1.5`, §8.5).
  10. Tek tipli hatta zorunlu elle adım üçü geçmez (`03 §8.5`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: yasak çiftlerin tamamı reddedilir.
  - KK2: teslim edilmiş sipariş ödeme Bekliyor'a düşmez.
  - KK3: ödemesiz sipariş Hazırlanıyor'a geçmez.
  - KK4: Ö1 ve S1 birlikte yazılır.
  - KK5: üç kapanış sonucu ayrı sınanır.
  - KK6: yanıtsız sorguda iptal ertelenir.
  - KK7: kapanışta ikinci iade satırı yok.
  - KK8: dokuz an için sayaç değişimi tabloyla aynı.
  - KK9: yedi hattın adım sayısı doğru.
  - KK10: en ağır hat üç adım.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T319: Geri alınamaz geçişler, onay gerekleri ve durum düzeltmesi
- **Bağımlılık:** T318
- **Dokümanlar:** çekirdek; `09` §9; `03 §1.6`; `02 §5.4`, §5.9, §10.4.6, §10.4.8; `05 MK-35`, §4.1; `10` KP-46
- **Kabul kriterleri:**
  1. Geri alınamaz sekiz işlem onaydan önce sonucunu tek cümleyle söyler; panel "geri al" düğmesi sunmaz (`03 §1.6.1`; `05 §4.1.4`).
  2. Sistemin iptalde kendiliğinden başlattığı kart iadesi onay penceresi istemez (`03 §1.6.1`).
  3. Düzeltme yeni bir geçiştir; yanlış geçiş ve düzeltme kayıtta kalır (`03 §1.6.2`; `05 §4.1.4`).
  4. Düzeltme kapalı listeden sebep seçilerek yapılır ve B-13 gider (`03 §1.6.2`).
  5. Teslim işareti düzeltilince sipariş bir önceki duruma döner ve cayma ile ayıp süresi yeni tarihe kadar işlemez (`03 §1.6.2.1`).
  6. Yanlış iptal yalnız ödeme Ödendi'deyken ve kart iadesi sonucu beklenmiyorken düzeltilir; ayırmalar yeniden yapılır, yetmezse düzeltme yapılamaz (`03 §1.6.2.2`).
  7. Yanlış Kargoya verildi ya da Teslim edilemedi işareti eksen bağını bozmadan geri alınır (`03 §1.6.2.3`).
  8. Havale "ödendi" düzeltmesi yalnız sevkiyat, iade ve kalem kaydı yokken yapılır; Z-8 yeniden başlar, B-3 yeni sürede bir kez daha gider (`03 §1.6.2.4`).
  9. Kart hattında ödeme ekseninin düzeltmesi yoktur; cayma beyanı düzeltilmez (`03 §1.6.2`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: sekiz işlemde onay cümlesi var, geri al yok.
  - KK2: sistem iadesinde onay istenmez.
  - KK3: geçiş kaydında iki satır kalır.
  - KK4: sebepsiz düzeltme reddedilir, B-13 yazılır.
  - KK5: teslim düzeltmesi pencereyi öteler.
  - KK6: bekleyen iadede düzeltme reddedilir.
  - KK7: yanlış kargo işareti geri döner.
  - KK8: kayıt varken havale düzeltmesi reddedilir.
  - KK9: kart ödemesi ve cayma düzeltilemez.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T320: Kalem kayıtları, ayıp talebi durumu, iade reddi ve panel işaretleri
- **Bağımlılık:** T318
- **Dokümanlar:** çekirdek; `09` §9; `03 §1.7`, §1.9, §5.3; `02 §5.4`, §5.8, §5.10, §7.5, §7.5.3; `10` KP-23
- **Kabul kriterleri:**
  1. Kalem durum taşımaz; yolunu kayıtlar anlatır ve her kayıt kendi satırında yapanı, anı ve adedi taşır (`03 §1.7.1`; `05 §4.1.2`).
  2. Her kaydın hatta sayılışı — teslim edilmiş, kapanmış, etkilemez — tabloyla uyar (`03 §1.7.1`).
  3. İşlemin birimi adettir; seçilebilen adet açık adettir ve aşan değer reddedilir (`03 §1.7.4`).
  4. Ardışık cayma beyanları ve parça parça ulaşan iadenin her teslim alması ayrı kayıttır (`03 §1.7.1`).
  5. İade reddi yalnız koşullu istisna kaleminde ve teslim alma adımında yapılır; B-16 gider (`03 §1.7.1`).
  6. Kalem başına tek ayıp talebi vardır; Açık'ken yeni talep açılmaz, Çözüldü'yken aynı kayıt yeniden açılır (`03 §1.9.1`).
  7. Açık'tan Çözüldü'ye yalnız yönetici geçirir; yeniden açmayı müşteri ya da yönetici iki yıllık süre içinde yapar (`03 §1.9.1`).
  8. Ayıp talebi doğunca ve müşteri yeniden açınca B-9 ve F-3 gider; çözüldü işareti bildirim üretmez (`03 §1.9.1`).
  9. Ayıp talebi sevkiyat hattını etkilemez; para gerektiren çözüm ödeme ekseni geçişleriyle işler (`03 §1.7.1`, §1.9.1).
  10. Panel işaretleri durum değildir; koşul sürdükçe görünür, kalkınca kalkar (`03 §1.7.3`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: kalemde durum alanı yok, kayıt satırı üç alan taşır.
  - KK2: kayıt türü başına hat etkisi tabloyla aynı.
  - KK3: açık adedi aşan adet reddedilir.
  - KK4: iki beyan iki satır, iki teslim alma iki satır.
  - KK5: izinsiz kalemde iade reddi reddedilir.
  - KK6: ikinci talep açılmaz, çözülen talep yeniden açılır.
  - KK7: iki yıldan sonra yeniden açma reddedilir.
  - KK8: bildirim satırları doğru alıcıya yazılır.
  - KK9: ayıp talebi sipariş durumunu değiştirmez.
  - KK10: koşul kalkınca işaret kalkar.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T321: Ürün yayın durumları
- **Bağımlılık:** T315
- **Dokümanlar:** çekirdek; `09` §9; `03 §1.8`; `02 §3.7.1`, §5.1; `10` KP-7, KP-40
- **Kabul kriterleri:**
  1. Ürün ve varyant Taslak, Yayında ve Arşiv değerlerini ayrı ayrı taşır (`03 §1.8.1`).
  2. Bütün geçişler yöneticinin panel işlemidir; bildirim üretmez ve işlem izine yazılır (`03 §1.8.1`).
  3. Taslak ürünün adresi ziyaretçiye "sayfa bulunamadı" döner, yöneticiye "Taslak" bandıyla görünür (`03 §1.8.1`).
  4. Arşiv ürünün adresi "Bu ürün artık satılmıyor" sayfasını döner (`03 §1.8.1`).
  5. Yayında'ya geçiş yayın kapısına bağlıdır (`03 §1.8.1`; `02 §3.7.1`).
  6. Yayın kapısını bozan işlem engellenir; panel sebebini söyler ve yöneticiyi ürünü taslağa ya da arşive almaya yönlendirir (`03 §1.8.2`).
  7. Vitrinden kalkan kalem sepetten çıkar ve müşteriye bir kez söylenir (`03 §1.8.3`).
  8. Yayın durumu siparişi etkilemez; satışın geçici kapatılması yayın durumunu değiştirmez (`03 §1.8.3`).
  9. Tükenmişlik ayrı bir durum değildir; Yayında ürün tükenmişken de listelerde ve aramada görünür (`03 §1.8.1`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: ürün Yayında iken varyantı Taslak olabilir.
  - KK2: geçişte bildirim yok, iz satırı var.
  - KK3: taslak adres 404 benzeri sayfa, yöneticide bant.
  - KK4: arşiv adresi açıklama sayfasını döner.
  - KK5: kapı koşulu eksikken Yayında reddedilir.
  - KK6: son Yayında varyantın arşive alınması engellenir, ileti yönlendirir.
  - KK7: arşive alınan ürün sepetten çıkar, bir kez söylenir.
  - KK8: açık sipariş yayın durumundan etkilenmez.
  - KK9: tükenmiş Yayında ürün aramada görünür.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T322: İçerik yayın durumları ve iletişim talebi durumları
- **Bağımlılık:** T315
- **Dokümanlar:** çekirdek; `09` §9; `03 §1.8`, §1.9; `02 §3.27.23`, §3.32.4, §5.2, §5.13; `10` KD-38
- **Kabul kriterleri:**
  1. İçerik kaydı Taslak ve Yayında değerlerini taşır; arşiv, ileri tarihli yayın ve sürüm geçmişi yoktur (`03 §1.8.4`).
  2. Taslaktan Yayında'ya geçiş yalnız zorunlu alanlar doluysa yapılır; düzenleme kaydedildiği anda yayındadır (`03 §1.8.4`).
  3. Yayın kapısını bozan düzenleme kaydedilmez (`03 §1.8.4`).
  4. Taslak içerik sitede yoktur; yönetici onu "Taslak" bandıyla önizler (`03 §1.8.4`).
  5. Yayındaki duyuru tarihleri girilmişse yalnız tarih aralığında görünür (`03 §1.8.4`).
  6. Hakkımızda silinmez, yalnız taslağa alınır; öteki kayıtların silinmesi geri alınamaz uyarısıyla yapılır (`03 §1.8.4`).
  7. İletişim talebi, KVKK talebi dahil, Açık ve Kapatıldı değerlerini taşır; ayrı durum makinesi ve süre sayacı yoktur (`03 §1.9.2`).
  8. Talep Açık doğar ve F-2 firmaya gider; yalnız yönetici kapatır ve yeniden açar, bildirim üretilmez (`03 §1.9.2`).
  9. Saklama süresi son kapatılıştan, hiç kapatılmamış talepte açılıştan işler (`03 §1.9.2`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: içerikte üçüncü durum ve sürüm tablosu yok.
  - KK2: zorunlu alan eksikse Yayında reddedilir.
  - KK3: yayındaki kaydın bozucu düzenlemesi kaydedilmez.
  - KK4: taslak sitede yok, yöneticide bant.
  - KK5: süresi dolmuş duyuru yayında olsa görünmez.
  - KK6: Hakkımızda'da silme yok, taslağa alma var.
  - KK7: iki talep durumu dışında değer yok.
  - KK8: doğuşta F-2 yazılır, kapatmada bildirim yok.
  - KK9: yeniden açılan talebin süresi yeni kapatılıştan sayılır.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T323: Durum × rol × işlem tablosu
- **Bağımlılık:** T319, T320
- **Dokümanlar:** çekirdek; `09` §9; `03 §1.11`; `02 §10.1`, §10.1.2; `05 MK-47`, §6.2
- **Kabul kriterleri:**
  1. `03 §1.11`'in 27 işlemi (müşteri 14, yönetici 13) tek bir "yapılabilir işlemler" kararında yaşar; her işlemin açık olduğu aralık o karardan okunur (`03 §1.11`).
  2. Karar sipariş durumundan, ödeme durumundan, kalem tipinden ve kalemin açık adedinden hesaplanır; ekran ve uç aynı kararı kullanır (`03 §1.11`).
  3. Açık aralığın dışındaki işlem sipariş sayfasında ve panelde sunulmaz (`03 §1.11`).
  4. Fiziksel kalemde cayma ve ayıp talebi yalnız kargoya verilmiş kalemde açılır (`03 §1.11`; `02 §5.8`).
  5. Dijital kalemde iptal ve cayma yoktur; hizmet kaleminde iptal "tamamlandı" işaretine kadar açıktır (`03 §1.11`).
  6. Her işlem sunucuda yeniden denetlenir; düğmenin gizlenmesi denetimin yerine geçmez (`05 MK-47`, §6.2).
  7. Rol modeli yoktur; panel işlemleri firma yöneticisinindir (`02 §10.1.2`; `05 MK-47`).
  8. Karar tablosu `03 §1.11`'in satır kimlikleriyle birebir eşleşir; tabloda olmayan işlem açık sayılmaz (`03 §1.11`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: 27 işlem için karar tablosu testte tam sayılır.
  - KK2: aynı sipariş durumunda ekran ve uç aynı sonucu verir.
  - KK3: kapalı aralıktaki işlem sayfada yok, uca gelirse reddedilir.
  - KK4: kargoya verilmemiş fiziksel kalemde cayma yok.
  - KK5: dijital kalemde iptal ve cayma yok, hizmette iptal sınırı işler.
  - KK6: gizli düğmenin ucu doğrudan çağrılınca reddedilir.
  - KK7: rol ayrımı yok, tek yönetici yetkisi.
  - KK8: tabloda olmayan işlem kapalıdır.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T324: Zaman aşımı akışları: hesap, oturum, sepet ve ödeme süreleri
- **Bağımlılık:** T317
- **Dokümanlar:** çekirdek; `09` §7, §13; `03 §4.1`; `02 §3.17.4`, §3.17.5, §4.1.3, Z-1…Z-9, §5.6.3, §6.1.2, §10.2.6, P-7, P-12…P-16, P-18
- **Kabul kriterleri:**
  1. Z-1, Z-2 ve Z-3 bağlantıları süre dolunca, yenisi istenince ya da `03 §4.1.3`'ün öteki koşullarında geçersizleşir; Z-1'de doğrulanmamış kayıt silinir (`03 §4.1.1`–§4.1.3).
  2. Z-4 ve Z-5 oturumları son işlemden itibaren sayılır ve süre dolunca kapanır; üyenin sepeti hesabında kalır (`03 §4.1.4`, §4.1.5).
  3. Sepet kendiliğinden boşalmaz; Z-6 için zamanlayıcı yoktur (`03 §4.1.6`).
  4. Z-7 dolunca sağlayıcıya son sorgu yapılır; ödeme alınmışsa Ö1 işler, alınmamışsa sipariş iptal edilir ve ayrılanlar serbest kalır (`03 §4.1.7`; `02 §6.1.2`).
  5. Sorgu yanıtsızsa sipariş iptal edilmez, "ödeme sonucu alınamadı" işaretiyle bekler ve sorgu yinelenir (`03 §4.1.7`).
  6. Z-8 dolunca havale siparişi kendiliğinden iptal edilir; iptal edilmiş sipariş sonradan "ödendi" yapılamaz (`03 §4.1.8`).
  7. Z-9 hatırlatması (B-3) son iş gününün başında bir kez gider; havale işareti düzeltilirse bir kez daha gider (`03 §4.1.9`).
  8. Süre değerleri ayar katmanından gelir, kodda sabit yazılmaz (`02 §11.1`, P-7, P-12…P-18).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: bağlantı süresi dolunca ve yenisi istenince eskisi geçersiz.
  - KK2: oturum süreleri son işlemden sayılır.
  - KK3: sepet için süre işi kayıtlı değil.
  - KK4: Z-7'de ödeme alınmışsa Ö1, alınmamışsa iptal.
  - KK5: yanıtsız sorguda iptal yok, işaret var, sorgu yinelenir.
  - KK6: iptal edilmiş havale siparişi "ödendi" yapılamaz.
  - KK7: B-3 bir kez gider, düzeltmede yeniden gider.
  - KK8: süreler ayardan okunur.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T325: Zaman aşımı akışları: teslim, cayma, geri ödeme ve ayıp talebi süreleri
- **Bağımlılık:** T318, T324
- **Dokümanlar:** çekirdek; `09` §7, §13; `03 §4.1`; `02 §3.20.6`, §4.1.3, Z-10…Z-19, Z-39, Z-42, Z-47, §7.2.3, §7.3.1, §7.4.1, §7.5.1, P-5, P-44
- **Kabul kriterleri:**
  1. Z-10 ve Z-39 ödeme onayından sayılır; süre aşımı kendiliğinden işlem üretmez, kalem panelde ilgili sayaçta görünür (`03 §4.1.10`, §4.1.13).
  2. Süre aşıldıktan sonraki iptalde panel kanuni faiz uyarısını gösterir (`03 §4.1.10`, §4.1.13).
  3. Z-11 dolunca teslim tarihi girilmemiş fiziksel kalemde gecikme feshi açılır (`03 §4.1.11`; `02 §7.2.3`).
  4. Z-12 süre değildir; teslim tarihi girilmezse Z-13 ve Z-18 başlamaz (`03 §4.1.12`).
  5. Z-13 ve Z-14 pencereleri kapanınca cayma düğmesi kapanır, önceki beyan geçerli kalır; Z-15 ödeme onayında düşer (`03 §4.1.14`–§4.1.16).
  6. Z-16, Z-17 ve Z-47 için sayaç ya da kendiliğinden işlem yoktur; kalan süre panelde gösterilir (`03 §4.1.17`, §4.1.19, §4.1.20).
  7. Z-42 dolunca kalem "iade malı bekleniyor"da kalır ve yöneticiye "mal dönmedi" kapanışı açılır (`03 §4.1.18`).
  8. Z-18 dolunca ayıp talebi kanalı kapanır; yeni talep açılmaz, çözülmüş talep yeniden açılmaz (`03 §4.1.21`).
  9. Z-19 süre değil adet sınırıdır; zamanlayıcı yoktur (`03 §4.1.22`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: Z-10 aşımında kendiliğinden işlem yok, sayaç var.
  - KK2: aşımdan sonra iptalde faiz uyarısı çıkar.
  - KK3: Z-11 sonrası fesih düğmesi açılır.
  - KK4: teslim tarihi yoksa Z-13 ve Z-18 işlemez.
  - KK5: pencere kapanınca beyan korunur.
  - KK6: Z-16, Z-17, Z-47 için iş kaydı yok.
  - KK7: Z-42 sonrası "mal dönmedi" kapanışı açılır.
  - KK8: Z-18 sonrası yeni talep reddedilir.
  - KK9: Z-19 için zamanlayıcı yok.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T326: Zaman aşımı akışları: katalog, kupon, içerik, iletişim ve bildirim süreleri
- **Bağımlılık:** T325, T321, T322, T304
- **Dokümanlar:** çekirdek; `09` §7, §13; `03 §4.1`; `02 §4.2`, Z-20…Z-24, Z-26, Z-27, P-38
- **Kabul kriterleri:**
  1. Z-20 bitişinde sistem indirimi kendisi bitirir; onay anında biten indirim özet farkıdır (`03 §4.1.23`).
  2. Z-21 hesap penceresidir; dolması işlem başlatmaz (`03 §4.1.24`).
  3. Z-22 dolunca kupon yeni siparişte kabul edilmez; ayrılmış haklar onaylanmış siparişlerde kalır (`03 §4.1.25`).
  4. Z-23 dolunca duyuru görünmez; elle kaldırma gerekmez (`03 §4.1.26`).
  5. Z-24 için ileri tarihli yayın yoktur; yayın anındadır (`03 §4.1.27`).
  6. Z-25 ve Z-37 için sayaç ve iş yoktur; yükümlülük firmanındır (`03 §4.1.28, §4.1.46).
  7. Z-26 başarısız e-postayı artan aralıkla üç kez dener; üçüncü başarısızlıkta türüne göre işaret ya da uyarı düşer (`03 §4.1.29`).
  8. Z-27 kayan pencerede deneme sayar; eşik altına inince engel kendiliğinden açılır, hesap kilitlenmez (`03 §4.1.30`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: biten indirim fiyatı eski haline döner.
  - KK2: referans penceresi dolunca iş çalışmaz.
  - KK3: süresi dolan kupon yeni siparişte reddedilir, eski hak kalır.
  - KK4: süresi dolan duyuru sitede yok.
  - KK5: ileri tarihli yayın tablosu ve işi yok.
  - KK6: Z-25 ve Z-37 için zamanlayıcı yok.
  - KK7: üçüncü başarısız gönderimde işaret ya da uyarı çıkar.
  - KK8: engel pencere dolunca açılır, hesap kilitli değil.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T327: Kendiliğinden işleyen anlar ve sitenin kesintisinde süreler
- **Bağımlılık:** T326, T317
- **Dokümanlar:** çekirdek; `09` §7, §13; `03 §4.2`, §10.3; `02 §3.34`, §3.34.7, §3.34.8, Z-8, Z-38, §4.3, P-7, P-43, §12.3.3; `05 MK-44`, §5.4; `10` KP-75
- **Kabul kriterleri:**
  1. `03 §4.2`'nin 12 anı, kendi sistem eylemleriyle bir kayıtta eşlenir ve her an için kayıt vardır (`03 §4.2`; `02 §4.3`).
  2. Süreler sitenin kesintisinde de işler; ürün kesintiyi sürelere eklemez (`03 §4.2.11`).
  3. Kesintide zamanı gelen kendiliğinden işlemler site dönünce kaçırdıkları sırayla hemen çalışır (`03 §4.2.11`).
  4. Z-8'in kesintide dolması iptali dönüşten sonraki ilk iş gününün sonuna erteler; ayrılanlar tutulur (`03 §4.2.12`).
  5. Ertelenen iptal, ödeme işaretlenmezse ertelenen anda işler (`03 §4.2.12`).
  6. Z-38 dolunca mal ulaşmamışsa IBAN silinir; kalem kapanmaz, başka bekleyen geri ödeme IBAN'ı tutuyorsa silinmez (`03 §4.2.8`).
  7. Kesinti tespiti ve yeniden başlatma `05 MK-44`'ün yoluyla yapılır (`05 MK-44`, §5.4).
  8. Yasal süreler hiçbir durumda dondurulmaz ve ayarla uzamaz (`03 §4`; `02 §4.3`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: 12 anın her biri için bir sistem eylemi var.
  - KK2: kesinti süreyi uzatmaz.
  - KK3: dönüşte işler kaçırılma sırasıyla çalışır.
  - KK4: kesintide dolan Z-8 iptali ertelenir, ayrılan tutulur.
  - KK5: erteleme anında iptal işler.
  - KK6: Z-38'de IBAN silinir, kalem açık kalır.
  - KK7: kesinti tespiti `05 MK-44` yoluyla çalışır.
  - KK8: yasal süre dondurulamaz.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T328: Bildirim haritası: tetikleyici eşlemesi, bildirim üretmeyen olaylar ve eşitlik denetimi
- **Bağımlılık:** T306, T307, T308, T309
- **Dokümanlar:** çekirdek; `09` §11; `03 §7.1`, §7.2, §7.3; `02 §9.1`, §9.1.1…§9.1.3, §9.5, §12.1.11; `10` KP-66, KP-67, KD-10, KD-16, KD-17, KD-52
- **Kabul kriterleri:**
  1. Her durum geçişi ve olay, tek bir eşleme tablosunda bildirim ya da "bildirim yok" satırına bağlanır (`03 §7.3`).
  2. B-1…B-16, F-1…F-6 ve hesap ile yönetim e-postaları yalnız eşlemedeki tetikleyiciden gönderilir (`03 §7.1`; `02 §9.5`).
  3. Bildirim üretmeyen olaylar kaynağıyla kayıtlıdır; ekran mesajları bildirim sayılmaz (`03 §7.2`).
  4. Tek kanal e-postadır; tercih ekranı ve SMS ya da uygulama içi bildirim yoktur (`03 §7.1`; `02 §9.1`).
  5. İşlem bildirimleri kapatılamaz ve ticari ileti sayılmaz (`02 §9.1`).
  6. Adres kuralı: müşteri bildirimi sipariş e-postasına, firma bildirimi firma e-postasına gider (`03 §7.1`; `02 §9.1.1`).
  7. Eşitlik denetimi akıştaki bildirimi ve haritadaki bildirimi iki yönde karşılaştırır; fazlası ya da eksiği CI'ı kırar (`03 §7`; `10` KD-52).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: her geçiş eşlemede bir satıra bağlı.
  - KK2: eşleme dışı tetikleyiciden e-posta çıkmaz.
  - KK3: bildirimsiz olay satırı kaynağıyla duruyor.
  - KK4: SMS, uygulama içi bildirim ve tercih ekranı yok.
  - KK5: işlem bildirimini kapatan ayar yok.
  - KK6: müşteri ve firma adres seçimi doğru.
  - KK7: haritada olup akışta olmayan bildirim eşitlik denetimini kırar.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T329: İlk uçtan uca akış testi: ürün, sepet, sipariş, sahte ödeme sonucu, kargo, teslim (servis düzeyi)
- **Bağımlılık:** T311, T318, T332, T335, T346, T348, T350, T351, T352
- **Dokümanlar:** çekirdek; `09` §9, §19; `03 §2.10.1`; `02 §2.1`, §2.2, §3.17.1
- **Kabul kriterleri:**
  1. `02 §2.2`'nin sekiz adımı tek bir Integration testinde, servis katmanından, `FakePaymentGateway` ile baştan sona geçer (`03 §2.10.1`; `02 §2.2`).
  2. Adım 1–2: yayındaki ürünün varyantı bulunur ve sepete eklenir; stok bu adımda ayrılmaz (`03 §2.10.1`).
  3. Adım 3–4: adresler ve ödeme yöntemi girilir, onay anında sipariş oluşur ve stok ile kupon hakkı ayrılır (`03 §2.10.1`; `02 §3.17.1`).
  4. Sahte ödeme sonucu başarılı döner ve ödeme bildirim ucu Ö1'i işler; B-4 bildirimi üretilir (`03 §2.10.1`).
  5. Yönetici kargoya verir (S5) ve teslim işaretini koyar (S6); kart hattında zorunlu elle adım ikidir (`03 §2.10.1`, §1.5).
  6. Her geçişte sipariş ve ödeme durumları iki eksende beklenen değerdedir; yan etkiler ve işlem izi satırları yazılmıştır (`03 §2.10.1`).
  7. Test yalnız ana akışı sınar; karışık sipariş, kargoda cayma, IBAN'sız iade ve e-posta düzeltme anlatıları T390'dadır (K-1441).
- **Test beklentisi:** Integration
- **Doğrulama kontrol listesi:**
  - KK1: sekiz adım tek testte, sahte ödemeyle geçer.
  - KK2: sepete eklemede stok ayrılmaz.
  - KK3: onayda sipariş oluşur, stok ve kupon ayrılır.
  - KK4: ödeme bildirimi Ö1'i işler, B-4 üretilir.
  - KK5: kargo ve teslim işareti iki zorunlu elle adımla biter.
  - KK6: her adımda iki eksenin durumu doğru.
  - KK7: kapsam dışı anlatılar bu testte yok.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T330: Firma kimliği: tek firma, tip ve tipe göre zorunlu alanlar, yasal kimlik bilgilerinin erişilebilirliği, iade adresi
- **Bağımlılık:** —
- **Dokümanlar:** çekirdek; `09` §10, §15; `02 §3.1.1`…§3.1.4, §3.1.7, §6.9, §7.3.5, §9.1.4, §10.1.1, §12.1.1, §12.1.4; `10` KP-35, KP-58, KD-21
- **Kabul kriterleri:**
  1. Kurulum tek firmaya hizmet eder; firma ekleme, seçme ve geçiş yoktur ve firma kimlik kaydı silinemez, yalnız düzenlenir (`02 §3.1.1`).
  2. Firma kimliği panelden, altyapısı kurulumdan gelir; iki kaynağın alanları ayrıdır (`02 §3.1.2`).
  3. Firma tipi Esnaf, Gerçek kişi tacir ve Tüzel kişi değerlerinden biridir; zorunlu kimlik alanları tipe göre değişir (`02 §3.1.3`).
  4. Üç tipte de adres, telefon, e-posta ve KEP adresi zorunludur; eksik kayıt kaydedilmez (`02 §3.1.3`).
  5. İsteğe bağlı işletme adı ve marka alanları tipten bağımsız saklanır (`02 §3.1.3`).
  6. Yasal kimlik bilgileri ana sayfadan ulaşılan İletişim başlığı altında sürekli erişilebilir durur (`02 §3.1.4`).
  7. ETBİS kaydı yapılmaz; kayıt bilgisi isteğe bağlı alandır ve doluysa doğrulama bandı gösterilir (`02 §3.1.4`).
  8. İade adresi zorunlu ayrı bir firma ayarıdır; beyanla birlikte beyan kaydına yazılır ve sonradan değişirse yapılmış beyanın adresi değişmez (`02 §3.1.7`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: ikinci firma kaydı ve firma silme yok.
  - KK2: panel ve kurulum alanları karışmaz.
  - KK3: tip değerleri üç ve kapalı.
  - KK4: KEP boşsa kayıt reddedilir.
  - KK5: isteğe bağlı alanlar boş geçer.
  - KK6: İletişim sayfasında yasal kimlik görünür.
  - KK7: ETBİS bilgisi doluysa bant çıkar, kayıt yapılmaz.
  - KK8: adres değişince eski beyanın adresi sabit kalır.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T331: Satış kapısı: dört koşul, veri toplayan girişlerin kapısı, satışın geçici kapatılması
- **Bağımlılık:** T330
- **Dokümanlar:** çekirdek; `09` §10, §13; `03 §10.2`; `02 §3.1.5`, §3.1.6, §3.13.19, §3.32.8, §3.33.9, §6.1.4, §6.9, §10.8.2, §12.2.5; `10` KP-25, KP-38, KP-59, KP-60, KD-60
- **Kabul kriterleri:**
  1. Satış yalnız dört koşul birlikte sağlanınca açıktır: kimlik alanları, açık ödeme yöntemi, kapalı geçici kapatma anahtarı, tamamlanmış dört yasal metin (`02 §3.1.5`).
  2. Koşullardan biri bozulunca yeni sipariş onayı reddedilir; sebep ve eksik koşul söylenir (`02 §3.1.5`; `03 §10.2`).
  3. Veri toplayan girişler (hesap kaydı, yeni hesap açacak Google girişi, iletişim formu) aydınlatma metni ve çerez politikası koşuluna bağlıdır; mevcut hesapların girişi etkilenmez (`02 §3.13.19`, §3.32.8, §3.33.9).
  4. Satışı geçici kapatma anahtarı panelden açılıp kapanır ve işlem izine yazılır (`02 §3.1.6`).
  5. Anahtar açıkken açık siparişler etkilenmez; havale onayı dahil yürümeye devam eder (`02 §3.1.6`).
  6. Kural dört koşulun hepsinde aynıdır: kimlik alanının boşalması ya da havalenin kapanması açık siparişleri etkilemez; yayındaki yasal metin yayından çekilmez (`02 §3.1.6`).
  7. Havale siparişi siparişe donan IBAN'ıyla sürer (`02 §3.1.6`).
  8. Kapı kararı tek yerde hesaplanır; vitrin, sepet ve ödeme aynı sonucu okur (`02 §6.1.4`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: dört koşuldan biri eksikken satış kapalı.
  - KK2: kapalı kapıda onay reddedilir, eksik koşul söylenir.
  - KK3: kayıt, yeni hesaplı Google girişi ve form kapı kapalıyken reddedilir, mevcut hesap girişi sürer.
  - KK4: anahtar değişimi iz satırı bırakır.
  - KK5: açık sipariş anahtardan etkilenmez.
  - KK6: kimlik alanı boşalınca açık sipariş sürer, yasal metin çekilmez.
  - KK7: havale siparişi donmuş IBAN'la sürer.
  - KK8: üç yüzey aynı kapı sonucunu okur.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T332: Satış modeli, ürün tipleri, ürün–varyant–seçenek yapısı ve stok kodu
- **Bağımlılık:** —
- **Dokümanlar:** çekirdek; `09` §10, §12; `02 §3.2.1`…§3.2.3, §3.3.1…§3.3.6, Z-39, §7.3.4, P-44, P-21; `05 MK-32`, §7.8; `10` KP-4, KP-5, KP-6, KP-39
- **Kabul kriterleri:**
  1. Satış yalnız doğrudandır; "fiyat sorunuz" durumu, teklif ya da talep hattı, randevu ve rezervasyon modeli yoktur (`02 §3.2.1`).
  2. Katalog fiziksel ürün, dijital ürün ve hizmet tiplerini taşır; tip ürün düzleminde yaşayan üç değerli kapalı bir alandır (`02 §3.2.2`, §3.2.3).
  3. Satılan birim varyanttır; ürün vitrin ve katalog birimidir (`02 §3.3.1`).
  4. Ürün en fazla iki seçenek boyutu taşır; sayı ürün sabitinden okunur (`02 §3.3.2`, P-21).
  5. İki boyutun birleşim matrisinin tam olması gerekmez; olmayan birleşim vitrinde görünür ama seçilemez (`02 §3.3.3`).
  6. Her varyant zorunlu ve tekil bir stok kodu taşır; yinelenen kod kaydedilmez (`02 §3.3.4`).
  7. Ürün başına asgari varyant ya da adet sınırı yoktur (`02 §3.3.5`).
  8. Ürün sayfasında yorum, puan ve öneri bloğu yoktur; bunlar için tablo da yoktur (`02 §3.3.6`).
  9. Ürün sayfasında yorum, puan ve öneri için bir ayar ya da kanca bırakılmaz (`02 §3.3.6`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: teklif ve "fiyat sorunuz" hattı yok.
  - KK2: tipin dördüncü değeri kaydedilmez.
  - KK3: sepete ürün değil varyant girer.
  - KK4: üçüncü boyut reddedilir.
  - KK5: eksik birleşim seçilemez.
  - KK6: aynı stok koduyla ikinci varyant reddedilir.
  - KK7: tek varyantlı ürün kaydedilir.
  - KK8: yorum ve puan tablosu yok.
  - KK9: yorum ve puan için ayar ya da kanca yok.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T333: Kategori ağacı, site içi arama ve liste süzgeçleri
- **Bağımlılık:** T332
- **Dokümanlar:** çekirdek; `09` §9, §10; `02 §3.4.1`…§3.4.6, §3.5.1…§3.5.3, §3.28.6, §6.6, P-20; `05 MK-33`, §7.9; `10` KP-1, KP-2, KP-3, KP-41
- **Kabul kriterleri:**
  1. Kategori ağacı en fazla üç seviyedir; dördüncü seviye kaydedilmez (`02 §3.4.1`).
  2. Ürün ağacın her düğümüne asılabilir (`02 §3.4.2`).
  3. Kategori sayfası alt dallarındaki ürünleri de gösterir (`02 §3.4.3`).
  4. Ürün birden çok kategoriye asılabilir ve biri ana kategoridir (`02 §3.4.4`).
  5. İçinde ürün ya da alt kategori bulunan kategori silinemez (`02 §3.4.5`).
  6. Kategori taşıma serbesttir; iki sınır (üç seviye, döngü) korunur (`02 §3.4.6`).
  7. Site içi arama yalnız ürün adında çalışır ve büyük-küçük harf ile Türkçe karakter duyarsızdır (`02 §3.5.1`).
  8. Listeler fiyat aralığı ve stok durumu ekseninde süzülür; ziyaretçiye sıralama seçeneği sunulmaz (`02 §3.5.2`, §3.5.3).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: dördüncü seviye reddedilir.
  - KK2: ürün ara düğüme asılabilir.
  - KK3: üst kategori alt dal ürünlerini listeler.
  - KK4: ana kategori her üründe tek.
  - KK5: dolu kategori silinemez.
  - KK6: kendi altına taşıma ve dördüncü seviyeye taşıma reddedilir.
  - KK7: arama açıklamada ve kategori adında eşleşme getirmez, AMPUL ile ampul aynı.
  - KK8: sıralama parametresi yok, iki süzgeç var.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T334: Stok ve hizmet kontenjanı: takip, tükenme, ayrılmış adedin altına inmeme
- **Bağımlılık:** T332
- **Dokümanlar:** çekirdek; `09` §9, §10; `02 §3.6.1`…§3.6.7, §6.2, §6.6, §7.2.6, §7.4.7, §7.4.8, §10.4.3, §10.4.6; `10` KP-4, KP-16, KP-39, KD-9, KD-33
- **Kabul kriterleri:**
  1. Stok varyantın sayısal adedidir (`02 §3.6.1`).
  2. Fiziksel üründe sayısal stok zorunludur; dijital üründe stok alanı yoktur; hizmette kontenjan isteğe bağlıdır ve girilmezse sınırsızdır (`02 §3.6.2`).
  3. Tükenmiş varyant seçenek listesinde "Tükendi" işaretiyle görünür ve seçilemez (`02 §3.6.3`).
  4. Bütün varyantları tükenmiş ürün vitrinde kalır (`02 §3.6.4`).
  5. Stok adedi ziyaretçiye hiçbir yerde gösterilmez (`02 §3.6.5`).
  6. İptal edilen adet stoğa kendiliğinden döner; iade edilen fiziksel adedi yönetici teslim alıp kontrol ettikten sonra panelden ekler (`02 §3.6.6`).
  7. Stok ve kontenjan, ödemesi beklenen siparişlerin ayırdığı adedin altına indirilemez (`02 §3.6.7`).
  8. Stok değişiklikleri eşzamanlı işlemde tutarlıdır; ayırma ve düşme tek işlemde yapılır (`02 §6.2`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: stok tam sayıdır, eksiye düşmez.
  - KK2: dijital üründe stok alanı yok, hizmette kontenjansız sınırsız.
  - KK3: tükenmiş varyant seçilemez, işaret görünür.
  - KK4: tükenmiş ürün vitrinde kalır.
  - KK5: hiçbir yanıtta stok adedi yok.
  - KK6: iptalde stok döner, iadede dönmez.
  - KK7: ayrılan adedin altına indirme reddedilir.
  - KK8: iki eşzamanlı onayda fazla ayırma olmaz.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T335: Ürün yayın durumu: yayın kapısı, arşiv, silme ve engelleme kalıbı
- **Bağımlılık:** T332, T321
- **Dokümanlar:** çekirdek; `09` §9, §10; `02 §3.7.1`…§3.7.8, §5.1, §5.2, §6.6, Z-24, P-44; `10` KP-7, KP-39, KP-40, KD-28, KD-38
- **Kabul kriterleri:**
  1. Ürün ve varyant yayın durumunu ayrı taşır; durum geçişleri T321'in makinesinden geçer (`02 §3.7.1`).
  2. Arşiv silme değildir; arşivlenen ürünün kaydı korunur, yalnız vitrinden çıkar (`02 §3.7.2`).
  3. Ürün yayına yalnız adı, ana kategorisi ve tipi girilmişse ve en az bir Yayında varyantı varsa alınır; açıklama ve görsel zorunlu değildir (`02 §3.7.3`).
  4. Dijital üründe yayındaki her varyant bir dosyaya bağlıdır; hizmette kapı ifa süresini de ister (`02 §3.7.4`).
  5. Yayındaki dijital varyantı dosyasız bırakacak dosya silme engellenir (`02 §3.7.4`).
  6. Taslak ürünün adresi ziyaretçiye "sayfa bulunamadı" döner, yöneticiye "Taslak" bandıyla görünür; taslak varyant ziyaretçiye görünmez (`02 §3.7.5`).
  7. Arşiv ürünün adresi "Bu ürün artık satılmıyor" sayfasını döner; fiyat ve sepete ekleme yoktur, listelerde ve aramada görünmez (`02 §3.7.6`).
  8. Silme kalıcıdır ve ayrı yoldur; geçmiş siparişler silmeden etkilenmez, açık siparişi olan ürün de panel uyarısıyla silinebilir (`02 §3.7.7`).
  9. Yayındaki ürünün son Yayında varyantı arşivlenemez, taslağa çekilemez ve silinemez; panel durdurur ve ürünü taslağa ya da arşive almaya yönlendirir (`02 §3.7.8`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: ürün ve varyant durumu bağımsız değişir.
  - KK2: arşivde kayıt durur, vitrinde yok.
  - KK3: eksik ad, kategori, tip ya da varyantla Yayında reddedilir.
  - KK4: dosyasız dijital varyantla kapı kapalı.
  - KK5: dosya silme yayındaki varyantı dosyasız bırakıyorsa engellenir.
  - KK6: taslak adres ziyaretçide yok, yöneticide bant.
  - KK7: arşiv sayfasında fiyat ve sepet düğmesi yok.
  - KK8: silinen ürünün eski siparişi bozulmaz.
  - KK9: son Yayında varyant üç yolla da korunur.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T336: Fiyat ve KDV: kuruş hassasiyeti, kalem bazlı hesap sırası, yuvarlama yönü, fiyat etiketi bilgileri
- **Bağımlılık:** T332
- **Dokümanlar:** çekirdek; `09` §7, §14; `02 §3.8.1`…§3.8.5, §6.6, P-10, §12.1.5; `10` KP-5, KP-39
- **Kabul kriterleri:**
  1. Fiyat varyantta yaşar ve KDV dahil girilir; yöneticinin yazdığı sayı ile ziyaretçinin gördüğü sayı aynıdır (`02 §3.8.1`).
  2. KDV oranı ürün düzleminde tutulur, varyantlar devralır ve varsayılanı ayardan gelir (`02 §3.8.2`).
  3. Matrah ve KDV tutarı sipariş kaydında ayrışır; kargo ücretinin oranı siparişteki fiziksel kalemlerin oranını izler (`02 §3.8.2`).
  4. KDV kalem bazında hesaplanır ve kuruşa kalemde yuvarlanır; sipariş toplamı yuvarlanmış kalemlerin toplamıdır (`02 §3.8.3`).
  5. Hesap sırası sabittir: indirimli birim fiyat × adet, kupon payı düşümü, kalan tutardan KDV (`02 §3.8.3`).
  6. Tek tutar kuruşa en yakın kuruşa yuvarlanır, tam yarım kuruş yukarı gider; orantılı bölünen tutarın parçaları aşağı yuvarlanır ve kalan kuruş en büyük parçaya yazılır (`02 §3.8.3`).
  7. Fiyatın hassasiyeti iki ondalıktır; fiyat sıfırdan büyüktür ve 0,00 TL kaydedilmez; tutarlar ondalık türde tutulur (`09` §7) (`02 §3.8.4`).
  8. Ürün sayfası "üretim yeri (ülke)" ve ölçüyle satılan üründe ölçü birimi ile net miktarı taşır; ölçü birimi ürün, net miktar varyant düzlemindedir (`02 §3.8.5`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: girilen fiyat ekranda aynen görünür.
  - KK2: KDV oranı ürüne yazılır, varyant devralır.
  - KK3: sipariş kaydında matrah ve KDV ayrı.
  - KK4: toplam kalem toplamıdır, yeniden türetilmez.
  - KK5: üç adımlı sıra sınanmış örneklerle doğru.
  - KK6: 12,345 -> 12,35; orantılı bölümde parçalar aşağı, artık en büyük parçada.
  - KK7: üç ondalık ve 0,00 TL reddedilir.
  - KK8: fiziksel üründe ülke zorunlu, dijitalde isteğe bağlı.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T337: İndirim ve referans fiyat: tarihli indirim, fiyat geçmişinden referans, kaydı engelleyen koşullar
- **Bağımlılık:** T336
- **Dokümanlar:** çekirdek; `09` §10, §14; `02 §3.9.1`…§3.9.6, §4.1.4, Z-20, Z-21, §5.11.1, §6.6, §12.1.6; `10` KP-5, KP-43
- **Kabul kriterleri:**
  1. İndirim ürün düzleminde yüzde olarak tanımlanır ve her varyanta kendi fiyatı üzerinden iner (`02 §3.9.1`).
  2. Hiçbir varyantın fiyatı sıfıra inmez; %100 indirim girilemez, yuvarlanmış indirimli fiyat 0,00 TL ise indirim kaydedilmez ve sebep söylenir (`02 §3.9.1`).
  3. İndirim tarihlidir; sistem indirimi kendisi başlatır ve bitirir (`02 §3.9.2`).
  4. İndirim beyanı gösterilen her yerde başlangıç ve bitiş tarihi de görünür (`02 §3.9.2`).
  5. Referans fiyatı sistem hesaplar, yönetici giremez; sistem her varyantın fiyat geçmişini tutar (`02 §3.9.3`).
  6. Referans, indirimin başladığı andan önceki 10 gün içinde o varyanta uygulanan en düşük fiyattır (`02 §3.9.3`).
  7. 10 günlük geçmişi olmayan varyantta referans, varyantın satışa sunulduğu andan beri en düşük fiyattır; öteki varyantların fiyatı referans olmaz (`02 §3.9.4`).
  8. İndirimli fiyat referansın altına inmiyorsa indirim kaydedilmez ve yönetici sebebiyle uyarılır (`02 §3.9.5`).
  9. Beyan edilen referans fiyat sipariş kalemine donar (`02 §3.9.6`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: indirim her varyantta kendi fiyatından hesaplanır.
  - KK2: %100 ve sıfıra yuvarlanan indirim reddedilir.
  - KK3: başlangıç ve bitişte indirim kendiliğinden açılıp kapanır.
  - KK4: tarihler ürün sayfasında ve kartta görünür.
  - KK5: referans alanı panelde girilemez.
  - KK6: 10 günlük en düşük fiyat örnekle doğrulanır.
  - KK7: yeni eklenen varyantta referans kendi geçmişinden gelir.
  - KK8: referansın altına inmeyen indirim kaydedilmez.
  - KK9: sipariş kaleminde referans donmuş.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T338: Kupon: tanım, kalem payı, sınırlar, kullanım hakkının ayrılması
- **Bağımlılık:** T336
- **Dokümanlar:** çekirdek; `09` §10, §14; `02 §3.10.1`…§3.10.7, §4.1.4, Z-22, §5.11.2…§5.11.4, §6.2, §7.2.6, §7.4.8, §8.2, L-6, P-36; `10` KP-11, KP-16, KP-43
- **Kabul kriterleri:**
  1. Kupon ödeme adımında girilen ve sepet toplamına inen bir koddur (`02 §3.10.1`).
  2. Kupon yüzde ya da sabit tutar olarak tanımlanır (`02 §3.10.2`).
  3. Kuponun payı kalem kalem belirlenir; yüzdesel kuponda her kalem kendi içinde küçülür, sabit tutar tutarlarıyla orantılı dağıtılır ve kalan kuruş en büyük kaleme yazılır (`02 §3.10.3`).
  4. Kalemin kupon payı kaleme donar ve KDV bu pay düşüldükten sonra hesaplanır (`02 §3.10.3`; `02 §3.8.3`).
  5. Sepet toplamına eşit ya da onu aşan kupon reddedilir; kupon siparişi sıfır tutara indirmez (`02 §3.10.4`).
  6. Sınır tarih aralığı ve toplam kullanım adedidir; kişiye özel kod toplam kullanım adedi 1 olan koddur (`02 §3.10.5`).
  7. Kupon kodu kuponlar arasında tekildir; art arda geçersiz denemede o sepetin kupon alanı geçici kapanır (`02 §3.10.5`, §8.2).
  8. Kullanım hakkı onayda ayrılır, ödeme başarılı olunca kullanılmış sayılır, ödeme süresi dolunca ya da başarısızlıkta geri döner (`02 §3.10.6`).
  9. Kupon ücretsiz kargo eşiğinin ve asgari sipariş tutarının tabanını düşürmez (`02 §3.10.7`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: kupon kodu ödeme adımında uygulanır.
  - KK2: yüzde ve sabit tutar iki tanım da çalışır.
  - KK3: kuruş artığı en büyük kaleme yazılır.
  - KK4: kalem payı donar, KDV paydan sonra hesaplanır.
  - KK5: sepete eşit kupon reddedilir.
  - KK6: süresi ya da hakkı dolan kod reddedilir.
  - KK7: yinelenen kod kaydedilmez, deneme limiti alanı kapatır.
  - KK8: son hakkı iki sipariş birden alamaz, iptalde hak döner.
  - KK9: kuponlu sepet eşiği düşürmez.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T339: Ürün görseli ve metni: ana görsel, kısıtlar, alternatif metin, metin biçimi seti
- **Bağımlılık:** T332
- **Dokümanlar:** çekirdek; `09` §10, §15; `02 §3.11.1`…§3.11.7, §3.27.16, §11.2, P-22, P-23, P-24, P-27; `05 MK-29`, MK-54, §6.5; `10` KP-5, KP-42, KD-63
- **Kabul kriterleri:**
  1. Görsel ürün düzleminde yaşar; varyant isterse kendi görselini taşır, taşımıyorsa ürününkini devralır (`02 §3.11.1`).
  2. Ayrı bir ana görsel işareti vardır ve galeri sırasından bağımsızdır; ilk eklenen görsel işaretle gelir, kaldırılınca sıradaki ilk görsel ana olur (`02 §3.11.2`).
  3. Galeri sırasını firma panelde elle belirler (`02 §3.11.2`).
  4. Görsele format listesi, tek dosya boyut tavanı ve ürün başına adet tavanı uygulanır; değerler ayardan okunur (`02 §3.11.3`).
  5. Yüklenen görsel yeniden kodlanır; yön uygulanır, meta veri atılır, hareketli görsel ilk karesiyle işlenir (`05 MK-29`, §6.5).
  6. Alternatif metin isteğe bağlıdır ve geri düşüşlüdür; yazılmazsa sistem ürün adından ve varyant değerlerinden üretir, çıktı boş kalmaz (`02 §3.11.4`).
  7. Ürün adına karakter tavanı konur; açıklama serbesttir (`02 §3.11.5`).
  8. Ürün açıklaması, kurumsal içeriğin uzun metinleri ve iki yasal metin tek kapalı metin biçimi setini kullanır; ham HTML yoktur ve setin dışındaki biçim düşer (`02 §3.11.6`).
  9. Garanti bilgisinin ayrı alanı yoktur (`02 §3.11.7`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: varyantsız görsel ürünün, varyantlı görsel varyantın.
  - KK2: ana görsel kaldırılınca sıradaki ana olur.
  - KK3: galeri sırası panelden değişir.
  - KK4: format, boyut ve adet tavanı aşılınca reddedilir.
  - KK5: yüklenen görselin meta verisi atılmış, yönü uygulanmış çıkar.
  - KK6: alternatif metin boş bırakılınca üretilen metin dolar.
  - KK7: ad tavanı aşılınca kaydedilmez.
  - KK8: betik içeren metin temizlenir, set dışı biçim düşer.
  - KK9: garanti alanı yok.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T340: Dijital ürün: dosya, indirme hakkı, hak yenileme, yeniden satın alma
- **Bağımlılık:** T332, T312, T335
- **Dokümanlar:** çekirdek; `09` §10, §15; `03 §8.1`; `02 §3.12.1`…§3.12.9, §3.7.4, Z-19, §6.6, §7.2.5, §7.5.4, B-4, P-8, P-26, P-44; `05 MK-30`, MK-31, §7.7; `10` KP-9, KP-20, KP-44
- **Kabul kriterleri:**
  1. Dijital ürünü sistem teslim eder; ödeme başarılı olunca sipariş sayfasında indirme düğmesi açılır (havalede "ödendi" işaretiyle) (`02 §3.12.1`).
  2. Dosya ürün düzleminde yaşar; varyant kendi dosyasını taşıyabilir, taşımıyorsa ürününkini devralır (`02 §3.12.2`).
  3. Tek dosyanın boyut tavanı vardır ve değer ayardan okunur (`02 §3.12.3`).
  4. Dijital üründe adet her zaman 1'dir (`02 §3.12.4`).
  5. İndirmenin adet sınırı vardır, süre sınırı yoktur; adet dolunca indirme kapanır (`02 §3.12.5`; `03 §4.1.22`).
  6. Hakkı dolan müşteri için yönetici o kalemin hakkını sipariş sayfasından yeniler ve sayaç sıfırlanır (`02 §3.12.6`).
  7. Sipariş sayfası kalemin bağlı olduğu varyantın o anki dosyasını verir; dondurma kuralının tek istisnasıdır (`02 §3.12.7`).
  8. Dosya güncellemesi indirme hakkını değiştirmez; yeni hâli indirmek de haktan bir indirme harcar (`02 §3.12.8`).
  9. Daha önce alınmış varyantı sepete ekleyen üyeye uyarı gösterilir, yine de satın alabilir; misafirde uyarı yoktur (`02 §3.12.9`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: ödeme onayında dijital kalem teslim edilir.
  - KK2: varyant dosyasızsa ürünün dosyası indirilir.
  - KK3: tavanı aşan dosya reddedilir.
  - KK4: dijital kalemde adet 1'den fazla yazılamaz.
  - KK5: hak dolunca indirme reddedilir, süre kontrolü yok.
  - KK6: yenilemede sayaç sıfırlanır.
  - KK7: dosya değişince eski alıcı yeni dosyayı indirir.
  - KK8: güncelleme hakkı artırmaz.
  - KK9: üyede uyarı var, misafirde yok, satın alma engellenmez.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T341: Üyelik: kayıt, e-posta doğrulama kapısı, misafir siparişinin hesaba bağlanması, hesap adı, giriş kaydı
- **Bağımlılık:** —
- **Dokümanlar:** çekirdek; `09` §10, §15; `02 §3.13.1`…§3.13.5, §3.13.18…§3.13.21, Z-1, Z-40, §5.12.1, §6.5, §8.1, §8.4, §8.5.1, L-3, L-9, §10.2.5, §10.3.3, §10.7.4; `10` KP-28
- **Kabul kriterleri:**
  1. Sipariş vermek için üyelik zorunlu değildir; misafir siparişi doğrulama kapısının dışındadır (`02 §3.13.1`, §3.13.4).
  2. Hesap açıldığında o e-postaya ait geçmiş misafir siparişleri hesaba düşer; tek dayanak doğrulanmış e-postadır ve silinmiş hesabın siparişleri bağlanmaz (`02 §3.13.2`).
  3. Hesabı olan kişi giriş yapmadan sipariş verirse sipariş kabul edilir ve hesaba anında tam olarak bağlanır; giriş duvarı yoktur (`02 §3.13.3`).
  4. E-posta doğrulaması sert kapıdır: doğrulanmamış hesapla giriş yapılamaz ve sipariş verilemez; tek tip hesap vardır (`02 §3.13.4`).
  5. Doğrulanmamış hesap geçicidir; Z-1 dolunca kayıt silinir, e-posta yeniden kayda açılır ve süresi geçmiş bağlantı "bağlantı geçersiz, yeniden kayıt olun" mesajını verir (`02 §3.13.5`).
  6. Doğrulama bağlantının açılmasıyla değil, açılan ekrandaki doğrulama düğmesiyle olur (`02 §3.13.5`).
  7. Kayıt formu ad, e-posta ve şifre ister; ad zorunludur ve kullanıcı adını yeniden doğrulamasız değiştirir (`02 §3.13.21`).
  8. Kayıtta ve hesapta pazarlama onayı alanı yoktur (`02 §3.13.18`).
  9. Hesap kaydı veri toplayan girişlerin kapısına bağlıdır; her giriş denemesi zaman, IP, e-posta ve sonuçla giriş kaydına yazılır ve Z-40 ile imha edilir (`02 §3.13.19`, §3.13.20).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: misafir sipariş verir, doğrulama aranmaz.
  - KK2: geçmiş misafir siparişi doğrulanmış e-postayla hesaba düşer.
  - KK3: hesaplı e-postayla misafir sipariş hesaba anında bağlanır.
  - KK4: doğrulanmamış hesap girişte ve siparişte reddedilir.
  - KK5: Z-1 sonrası kayıt silinir, bağlantı geçersiz mesajı çıkar.
  - KK6: bağlantıya tıklamak doğrulamaz, düğme doğrular.
  - KK7: adsız kayıt reddedilir, ad değişimi yeniden doğrulama istemez.
  - KK8: formda ve kayıtta pazarlama alanı yok.
  - KK9: başarılı ve başarısız giriş kaydı yazılır.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T342: Giriş ve oturum: oturum ömrü, şifre politikası, sıfırlama, yeniden doğrulama, yönetici hesaplarına aynı rejim, giriş limitleri
- **Bağımlılık:** T341
- **Dokümanlar:** çekirdek; `09` §8, §15; `02 §3.13.9`…§3.13.13, §3.13.15…§3.13.17, Z-2, Z-4, Z-5, §5.12.2, §6.5, §8.1, §8.2, L-1, L-2, §10.2.4, §10.2.5, P-13, P-15, P-16, P-17, P-31, P-32, P-48; `05 MK-45`, MK-46, MK-51; `10` KP-27
- **Kabul kriterleri:**
  1. Şifre sıfırlama bağlantısının ömrü vardır ve tek kullanımlıktır; kullanılmış ya da süresi dolmuş bağlantı geçersizdir (`02 §3.13.9`).
  2. Şifre sıfırlanınca hesabın bütün oturumları kapanır; hesaptan değiştirilince öteki oturumlar kapanır, değişikliği yapan oturum açık kalır (`02 §3.13.10`).
  3. Şifre sıfırlama ekranı nötr, kayıt ekranı dürüst konuşur (`02 §3.13.11`).
  4. Oturum ömrünü kullanıcı seçer: "beni hatırla" uzun, işaretsiz kısa oturum açar (`02 §3.13.12`).
  5. Şifre değiştirme, e-posta değiştirme ve hesap silme açık oturumda da yeniden doğrulama ister; başka işlem istemez (`02 §3.13.13`).
  6. Google hesabında yeniden doğrulama Google ile yapılır; yeniden doğrulamadaki başarısız şifre L-1'e sayılır (`02 §3.13.13`).
  7. Şifre politikası uzunluk tabanlıdır; asgari uzunluk aranır, karakter türü dayatılmaz, yaygın şifreler yerel listeyle reddedilir (`02 §3.13.15`).
  8. Bu kurallar yönetici hesaplarına aynı değerlerle uygulanır; iki adımlı doğrulama yoktur (`02 §3.13.16`).
  9. Giriş, şifre sıfırlama, kayıt ve doğrulama bağlantısı isteği limitlidir; eşikte işlem geçici engellenir, hesap kilitlenmez, mesaj nötrdür (`02 §3.13.17`; L-1, L-2).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: sıfırlama bağlantısı ikinci kullanımda reddedilir.
  - KK2: sıfırlamada tüm oturumlar kapanır, değiştirmede yalnız öteki oturumlar.
  - KK3: iki ekranın mesajları iki ayrı biçimde.
  - KK4: "beni hatırla" iki ayrı süre açar.
  - KK5: üç işlem yeniden doğrulama ister, öteki işlem istemez.
  - KK6: Google hesabı Google ile doğrulanır.
  - KK7: kısa şifre ve yaygın şifre reddedilir, karakter dayatması yok.
  - KK8: yönetici girişinde aynı süre ve politika işler.
  - KK9: eşikte engel açılır, hesap kilitlenmez, kalan sayı söylenmez.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T343: Google ile giriş ve hesap bağlama, e-posta değişikliği ve geri alma
- **Bağımlılık:** T342
- **Dokümanlar:** çekirdek; `09` §11, §15; `02 §3.13.6`…§3.13.8, §3.13.14, Z-2, Z-45, §5.12.2, §6.5, §8.1.2, §8.1.3, §8.1.5, §8.2, L-9, §10.2.4, §10.2.5, P-13, P-47, P-49; `05 MK-46`, §6.1; `10` KP-26, KP-27, KD-11
- **Kabul kriterleri:**
  1. Sosyal giriş yalnız Google iledir; Facebook girişi yoktur (`02 §3.13.6`).
  2. Google uygulamasının kimlik bilgileri kurulum ayarıdır; tanımlı değilse düğme görünmez ve panelde açma kapama anahtarı yoktur (`02 §3.13.6`).
  3. Sağlayıcının doğrulanmış verdiği e-posta mevcut hesapla eşleşirse hesap tek kalır ve iki giriş yolunu da taşır (`02 §3.13.7`).
  4. Google'ın adres için yetkili olmadığı ve hesabın Google'a bağlı olmadığı durumda önce teyit istenir (`02 §3.13.7`).
  5. Google ile açılan hesap şifresiz başlar ve sonradan şifre konabilir; sıfırlama akışı "şifre belirle" işlevi görür (`02 §3.13.8`).
  6. Hesabın e-posta adresi, yeni adres doğrulanmadan değişmez; doğrulama yeni adrese giden bağlantının ekranındaki düğmeyle olur (`02 §3.13.14`).
  7. Doğrulanınca bağlama kuralı yeni adres için yeniden işler; eski adrese bağlı siparişler hesapta kalır, bağlanmış sipariş çözülmez (`02 §3.13.14`).
  8. Eski adrese Z-45 ömürlü geri alma bağlantısı gider; ömrü dolunca bağlantı ve eski adres silinir (`02 §3.13.14`; Z-45).
  9. Giriş ve hesap bağlama istekleri limitlidir ve Google bağlayıcısının arayüzü sahteyle çalışır (`02 §3.13.17`; L-9).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: Facebook yolu yok.
  - KK2: kimlik bilgisi yokken Google düğmesi görünmez, panelde anahtar yok.
  - KK3: eşleşen doğrulanmış e-posta tek hesaba bağlanır.
  - KK4: yetkisiz sağlayıcıda teyit istenir.
  - KK5: şifresiz hesapta sıfırlama şifre belirler.
  - KK6: yeni adres doğrulanmadan e-posta değişmez.
  - KK7: eski adrese bağlı sipariş hesapta kalır.
  - KK8: geri alma bağlantısı Z-45'te geçersizleşir, eski adres silinir.
  - KK9: sahte Google bağlayıcısıyla akış testi geçer.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T344: Adres: adres defteri, fatura adresi, kapalı il listesi, tipe göre istenen adresler, siparişe donma
- **Bağımlılık:** T341
- **Dokümanlar:** çekirdek; `09` §3 (§3.4), §10; `02 §3.14.1`…§3.14.6, §3.20.1, §3.23.3; `10` KP-12, KP-13, KP-27, KD-27
- **Kabul kriterleri:**
  1. Üye müşteri adres defterinde birden çok adres kaydeder ve adlandırır; misafirin adres defteri yoktur (`02 §3.14.1`).
  2. Üye ödeme adımında yeni adres yazabilir; yeni adres siparişe donar, deftere yalnız kaydetmeyi seçerse yazılır (`02 §3.14.1`).
  3. Fatura adresi varsayılan olarak teslimat adresiyle aynıdır; kullanıcı ayrı bir fatura adresi seçebilir (`02 §3.14.2`).
  4. İl serbest metin değildir; 81 ilden oluşan kapalı listeden seçilir (`02 §3.14.3`).
  5. En az bir fiziksel kalemli sipariş teslimat ve fatura adresi ister (`02 §3.14.4`).
  6. Yalnız dijital ve hizmet kalemli sipariş teslimat adresi alanlarını göstermez ve yalnız fatura adresi ister (`02 §3.14.4`).
  7. Sipariş adreslerini kendi içine dondurur; defterdeki sonraki değişiklik ya da silme geçmiş siparişe dokunmaz (`02 §3.14.5`).
  8. Adres alanları ad-soyad, il, ilçe ve açık adrestir ve dördü zorunludur; ilçe serbest metindir (`02 §3.14.6`).
  9. Teslimat adresi ayrıca zorunlu telefon taşır (`02 §3.14.6`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: üye ikinci adresi adlandırarak kaydeder, misafirde defter yok.
  - KK2: kaydetmeden kullanılan adres deftere girmez.
  - KK3: fatura adresi seçilmeyince teslimat adresiyle aynı.
  - KK4: listede olmayan il reddedilir, liste 81 il.
  - KK5: fiziksel kalemli siparişte iki adres istenir.
  - KK6: dijital siparişte teslimat alanı yok, fatura adresi var.
  - KK7: deftere sonradan yapılan düzenleme eski siparişi değiştirmez.
  - KK8: dört alandan biri boşsa kayıt reddedilir.
  - KK9: telefonsuz teslimat adresi reddedilir.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T345: Hesabın silinmesi ve kişisel veri başvurusu
- **Bağımlılık:** T341, T343
- **Dokümanlar:** çekirdek; `09` §10, §13; `02 §3.15.1`…§3.15.6, Z-31, Z-32, §5.3.5, §5.9, §6.5, §7.6.5, §9.4, §10.3.1, §12.2.6, §12.2.7; `05 §6.1`; `10` KP-29, KP-30, KP-77, KD-36
- **Kabul kriterleri:**
  1. Hesap silinince hesabın adı, giriş bilgileri, şifre, açık oturumlar, adres defteri ve sepet hemen silinir (`02 §3.15.1`; Z-31, Z-32).
  2. Siparişin kişisel verileri yasal saklama süresi dolunca imha edilir; tutar ve tarih gibi alanlar ticari kayıt olarak kalır (`02 §3.15.1`).
  3. Silme anında hesabın e-posta adresine bir kez hesabın silindiği bildirimi gider (`02 §3.15.1`; `02 §9.4`).
  4. Yürüyen sipariş hesap silmeyi engellemez; takip, cayma ve iade siparişin iletişim e-postası ile sürer (`02 §3.15.2`).
  5. Silinmiş hesabın siparişleri hiçbir hesaba bağlanmaz (`02 §3.15.3`).
  6. Başvuru kanalı iletişim formunun "KVKK talebi" konu tipidir; ayrı modül, durum makinesi ve süre sayacı yoktur (`02 §3.15.4`).
  7. Uygulama içi veri indirme yoktur; hesap verisi panelde düzenlemesiz üye kaydı görünümünden okunur (`02 §3.15.5`).
  8. Firma, talep üzerine hesabı panelden silebilir; silme aynı kuralla işler (`02 §3.15.5`).
  9. E-posta değişikliğini geri alma bağlantısı açıkken hesap silinmez; ekran sebebi ve bitiş tarihini söyler (`02 §3.15.6`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: silmeden sonra ad, şifre, oturum, defter ve sepet yok.
  - KK2: sipariş kişisel verisi saklama süresine kadar durur.
  - KK3: silme e-postası bir kez gider.
  - KK4: açık siparişli hesap silinir, sipariş misafir yoluyla izlenir.
  - KK5: silinmiş hesabın siparişi yeni hesaba bağlanmaz.
  - KK6: KVKK talebi için ayrı tablo ve sayaç yok.
  - KK7: veri indirme ucu yok, üye kaydı görünümünde düzenleme yok.
  - KK8: yönetici silmesi kendi silmesiyle aynı sonucu verir.
  - KK9: açık geri alma bağlantısında silme reddedilir, sebep gösterilir.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T346: Sepet: sahiplik, giriş anında birleşme, canlı kalem, siparişe girmeyen kalemler
- **Bağımlılık:** T332, T334, T335, T336, T342
- **Dokümanlar:** çekirdek; `09` §10, §15; `02 §3.16.1`…§3.16.7, Z-6, §6.1.4, §6.2, §12.2.5; `10` KP-9, KP-10, KD-10
- **Kabul kriterleri:**
  1. Üyenin sepeti hesabında durur ve her cihazda aynıdır; misafirin sepeti tarayıcıya bağlıdır ve üye çıkınca o tarayıcıda sepet boş görünür (`02 §3.16.1`).
  2. Sepet kendiliğinden boşalmaz; süre parametresi yoktur (`02 §3.16.2`).
  3. Girişte iki sepet birleşir; aynı varyantta büyük adet kalır, farklı varyantlar ayrı kalem olur; kural giriş yolundan bağımsızdır (`02 §3.16.3`).
  4. Birleştirme müşterinin görmediği ürün ya da adet eklediyse bir kez söylenir ve eklenenler adıyla listelenir; eklemediyse mesaj yoktur (`02 §3.16.4`).
  5. Kalem fiyatını ve satın alınabilirliğini güncel üründen okur; ilk eklenme fiyatından farklıysa tek metinle işaretlenir (`02 §3.16.5`).
  6. Satın alınamayan "Tükendi" kalem sepette kalır ve siparişe girmez; stok dönünce yeniden alınabilir (`02 §3.16.6`).
  7. Vitrinden kalkan kalem sepetten çıkar ve bir kez söylenir; siparişe girebilecek kalem kalmazsa ödeme adımı açılmaz (`02 §3.16.7`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: üye başka tarayıcıda aynı sepeti görür, çıkışta boş görünür.
  - KK2: sepet süreyle boşalmaz.
  - KK3: aynı varyantta büyük adet kalır, giriş yolu fark etmez.
  - KK4: görülmeyen ekleme bir kez söylenir, eklenmediyse mesaj yok.
  - KK5: fiyat farkı tek metinle işaretlenir, yön gösterilmez.
  - KK6: tükenen kalem siparişe girmez, stok dönünce girer.
  - KK7: kalkan kalem çıkar, boş sepette ödeme açılmaz.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T347: Sepet adet kuralları: stok ve sipariş başına sınır, boşalma, istek listesi yokluğu
- **Bağımlılık:** T332, T334, T346
- **Dokümanlar:** çekirdek; `09` §3 (§3.4), §10; `02 §3.16.8`…§3.16.13, §6.2, §11.1, P-9; `10` KP-9, KD-7, KD-35, KD-59
- **Kabul kriterleri:**
  1. Stoktan fazla adet sepete eklenmez; mesaj alınabilecek adedi söylemez ve stok ayrılmış adetler düşülerek okunur (`02 §3.16.8`).
  2. Adet artırma ve girişteki birleştirme aynı stok kuralından geçer; stok sepet adedinin altına düşerse kalem sepette kalır ve adet kendiliğinden indirilmez (`02 §3.16.8`).
  3. "Bir siparişte en fazla" sınırı ürünün bütün varyantlarının toplamına uygulanır; aşan adet eklenmez ve sınır sayısıyla söylenir (`02 §3.16.9`).
  4. Sonradan aşılan sınırda kalemler mesajla sepette kalır, siparişe girmez ve toplama sayılmaz (`02 §3.16.10`).
  5. Sepetteki kalem sayısına tavan yoktur; elli kalem performans kabulüdür (`02 §3.16.11`).
  6. Sepet ödeme başarılı olunca yalnız siparişe giren kalemlerden boşalır; birleşen misafir sepetinin siparişi için ayrı istisna işler (`02 §3.16.12`).
  7. İstek listesi, favori düğmesi ve sepet hatırlatma e-postası yoktur (`02 §3.16.13`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: fazla adet reddedilir, mesajda adet yok.
  - KK2: birleşmede ve artırmada aynı kural çalışır, adet indirilmez.
  - KK3: iki varyantın toplamı sınırı aşamaz.
  - KK4: düşürülen sınırda kalem sepette kalır, toplama girmez.
  - KK5: kalem sayısı için sınır yok.
  - KK6: ödemeden sonra yalnız giren kalemler çıkar, istisna sınanır.
  - KK7: favori ucu ve hatırlatma e-postası yok.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T348: Sipariş onayı: oluşma anı, özet eşitliği, stok ayırma, ödeme süresi, kendiliğinden iptal
- **Bağımlılık:** T331, T338, T347, T349, T354, T355, T356, T316, T317, T324
- **Dokümanlar:** çekirdek; `09` §9, §10, §13, §14; `02 §3.17.1`…§3.17.8, §3.10.6, Z-7, Z-8, §4.3, §5.3, §5.6, §6.1, §6.3, §8.2, L-7, L-8, §10.6, §11.1, P-7, P-18; `05` MK-21, MK-36, MK-39; `10` KP-8, KP-11, KP-14…KP-17
- **Kabul kriterleri:**
  1. Sipariş ödeme adımında onay anında oluşur; aynı anda numara verilir, kalem, adres ve sürümler donar, stok ayrılır; iki ödeme yolunda da alındığı ve numara gösterilir (`02 §3.17.1`).
  2. Onay anında sepet yeniden değerlendirilir; tutarı, dökümü ya da kalemleri değiştiren her fark siparişi durdurur ve güncel özet yeniden onaylanır (`02 §3.17.2`).
  3. Onayda adet stoktan, hizmet kontenjanından ve kupon hakkından ayrılır; ödeme başarılı olunca kesin düşer (`02 §3.17.3`).
  4. Kart süresi dakikayla ve ürün sabitidir; havale süresi iş günüyle ve panel girdisidir, çit dışı reddedilir; havalede ayırma süre boyunca sürer (`02 §3.17.4`).
  5. Süresi dolan ya da kart ödemesi başarısız sipariş kendiliğinden iptal olur, ayırmalar aynı anda serbest kalır; kartta iptalden önce sonuç son kez sorulur (`02 §3.17.5`).
  6. Başarısız ödemenin tekrarı yeni siparişle olur; aynı siparişi sürdürme yolu yoktur (`02 §3.17.6`).
  7. Aynı sepetten yeni onayda ödenmemiş önceki sipariş iptal edilir; birleşen misafir sepeti istisnadır (`02 §3.17.7`).
  8. Ödemesi hiç başarılı olmamış ve kendiliğinden iptal olmuş sipariş müşteri listesinde görünmez, panelde görünür (`02 §3.17.8`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: onayda numara, donma ve ayırma birlikte olur.
  - KK2: her fark siparişi durdurur, yeniden onay ister.
  - KK3: ayırma ve kesin düşme ödeme sonucuna göre işler.
  - KK4: havale çit dışı değer panelde reddedilir.
  - KK5: süre dolunca ayırmalar tek anda döner, ödemesi Başarısız olur.
  - KK6: tekrar yeni numarayla olur.
  - KK7: önceki ödenmemiş sipariş iptal edilir, istisna sınanır.
  - KK8: görünürlük müşteri ve panel için doğru.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T349: Asgari sipariş tutarı, kargo ücreti ve ücretsiz kargo eşiği
- **Bağımlılık:** T332, T336, T346
- **Dokümanlar:** çekirdek; `09` §14; `02 §3.18.1`…§3.19.6, §3.10.7, §6.2, §7.2.9, §7.4.6, §11.1, P-1, P-3, P-4; `10` KP-13, KP-59, KD-26
- **Kabul kriterleri:**
  1. Asgari sipariş tutarı isteğe bağlı tektir; altındaki sipariş verilemez, eksik tutar gösterilir; sepette ve onayda denetlenir (`02 §3.18.1`).
  2. Karşılaştırma tabanı kupondan önceki indirimli toplamdır; kupon siparişi engellemez (`02 §3.18.2`; `02 §3.19.4`).
  3. Asgari tutar yalnız fiziksel kalemli siparişe uygulanır; koşul siparişe giren kalemlere bakar (`02 §3.18.3`).
  4. Kargo ücreti sipariş başına sabittir; fiziksel kalem varsa bir kez alınır, yoksa alınmaz; 0 TL her siparişi ücretsiz yapar (`02 §3.19.1`).
  5. Ücret KDV dahil girilir; farklı oranlı fiziksel kalemlerde orantılı bölünür ve her parça kendi oranıyla dökümde yer alır (`02 §3.19.2`).
  6. Ücretsiz kargo eşiği isteğe bağlıdır; taban eşiğe eşit ya da üstündeyse ücret alınmaz (`02 §3.19.3`).
  7. Eşik tabanı indirimli tutardır; indirim tabanı düşürür, kupon düşürmez (`02 §3.19.4`).
  8. Eşik ve fiziksel kalem varsa sepet ve özet kalan tutarı ya da "Kargo ücretsiz"i yazar; yoksa mesaj yoktur (`02 §3.19.5`).
  9. Ücret ve ücretsiz kararı sipariş düzleminde donar; kısmi iptal ya da iade yeniden hesaplatmaz (`02 §3.19.6`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: tutar altında sipariş durur, eksik tutar yazar.
  - KK2: kuponlu sepet tabanı değişmez.
  - KK3: dijital sepette asgari tutar uygulanmaz.
  - KK4: ücret bir kez alınır, dijital siparişte yok.
  - KK5: karışık oranda bölüşüm ve kalan kuruş doğru.
  - KK6: eşikte ve üstünde ücret sıfır.
  - KK7: indirim tabanı düşürür, kupon düşürmez.
  - KK8: kalan tutar mesajı koşula bağlı çıkar.
  - KK9: iptalden sonra ücret yeniden hesaplanmaz.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T350: Teslim: il kısıtı, tek yol, kargoya verme süresi ve sözü, kargoya verildi işareti, kargo şirketi
- **Bağımlılık:** T310, T316, T332, T344
- **Dokümanlar:** çekirdek; `09` §9, §10; `02 §3.20.1`…§3.20.8, §4.1.2, Z-10, Z-11, §5.7, §6.2, §6.4, §6.9, §7.2.3, §9.2, B-5, §10.4.4, §10.6.3, §11.1, P-5, P-6; `10` KP-13, KP-19, KP-46, KP-59, KD-2, KD-24, KD-25
- **Kabul kriterleri:**
  1. Firma teslimat illerini seçer (varsayılan tüm Türkiye); seçili olmayan ildeki fiziksel sipariş verilemez ve sebep söylenir; kısıt il düzeyindedir (`02 §3.20.1`).
  2. Defterdeki teslimat yapılmayan ildeki adres ödemede seçilemez (`02 §3.20.1`).
  3. Tek teslimat yolu kargodur; teslimat seçeneği, mağazadan teslim ve adlandırılmış seçenek yoktur (`02 §3.20.2`).
  4. Sipariş tek parça gider ve tek takip numarası taşır; kalemler en uzun süreyi birlikte bekler (`02 §3.20.3`).
  5. Teslimat sözü kargoya verme süresidir; panel üst çit koyar ve çit üstünü reddeder (`02 §3.20.4`).
  6. Firma varsayılanını ürün ezer, varyant devralır; siparişin sözü en uzun süredir ve onayda donar (`02 §3.20.5`).
  7. Süre ödemenin onaylandığı anda başlar: kartta başarı bildiriminde, havalede ödendi işaretinde (`02 §3.20.6`).
  8. Kargoya verildi işareti kargo şirketi ve takip numarası ya da "kendi aracımızla teslim" olmadan konmaz; müşteri sipariş sayfasında görür (`02 §3.20.7`).
  9. Kargo şirketi ürünle gelen listeden seçilir; listede olmayan "Diğer" olarak adla yazılır; "Takip et" bağlantısı yalnız listedekilerde çıkar (`02 §3.20.8`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: kapalı ildeki teslimat sebebiyle reddedilir.
  - KK2: kapalı ildeki defter adresi seçilemez.
  - KK3: teslimat seçeneği ve mağaza teslimi yok.
  - KK4: tek parça, tek takip numarası.
  - KK5: çit üstü süre reddedilir.
  - KK6: ürün ve varyant önceliği ve donan söz doğru.
  - KK7: süre başlangıcı kart ve havalede doğru anda.
  - KK8: bilgisiz kargoya verildi işareti reddedilir.
  - KK9: liste ve "Diğer" davranışı doğru.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T351: Teslim işaretleri: kalem tipine göre teslim, karışık sipariş, fiziksel teslim tarihi
- **Bağımlılık:** T316, T318, T350
- **Dokümanlar:** çekirdek; `09` §9; `02 §3.20.9`…§3.20.12, Z-12, Z-13, §5.8, §6.4, §6.7, §7.2.5, §7.3.1, §7.3.3, §10.4.5; `10` KP-6
- **Kabul kriterleri:**
  1. Teslim tipe göre işler: fiziksel kalem kargo hattından ve firma teslim işaretiyle, dijital kalem ödeme onayında sistemce, hizmet kalemi firmanın "tamamlandı" işaretiyle (`02 §3.20.9`).
  2. Teslim işareti ödeme onaylanmışken konur; ödeme Bekliyor ya da Başarısız iken konmaz (`02 §3.20.9`).
  3. Karışık siparişte sevkiyat tek eksende kalır ve iptal edilmemiş fiziksel kalem sürdükçe fiziksel hattı izler; dijital kalem beklemeden indirilebilir (`02 §3.20.10`).
  4. Kalem düzeyinde tutulan şey teslim işaretidir; ayrı durum makinesi yoktur (`02 §3.20.10`).
  5. Fiziksel teslimi firma işaretler ve tarihi girer; geçmiş tarih olur, ileri tarih ve kargoya verme gününden önceki tarih panelde sebebiyle reddedilir (`02 §3.20.11`).
  6. Fiziksel kalemlerin tamamı iptal edilirse sipariş fiziksel kalemsiz siparişin hattına düşer (`02 §3.20.10`).
  7. Fiziksel kalemi olmayan sipariş kargo adımlarından geçmez; Alındı'da bekler ve tüm kalemler teslim işareti alınca Teslim edildi olur (`02 §3.20.12`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: üç tipin teslim yolu ayrı ayrı çalışır.
  - KK2: ödeme Bekliyor iken işaret reddedilir.
  - KK3: karışık siparişte dijital indirme fiziksel hattı beklemez.
  - KK4: kalem için durum makinesi tablosu yok.
  - KK5: tarih sınırları doğru, ret sebep söyler.
  - KK6: fiziksel kalemler iptal edilince hat değişir.
  - KK7: kalemsiz siparişte kargo adımı yok, son işaretle Teslim edildi.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T352: Ödeme: satıcı ve kart verisi, iki yöntem, 3D Secure, taksit, ters ibraz
- **Bağımlılık:** T317, T348
- **Dokümanlar:** çekirdek; `09` §9, §11; `02 §3.21.1`…§3.21.4, §3.21.9…§3.21.11, Z-7, §6.1.2, §6.1.4, §6.2, §6.9, §8.3.3, §8.3.4, §11.2, P-18, §12.1.3; `10` KP-15, KD-4, KD-23, KD-62, KD-64
- **Kabul kriterleri:**
  1. Satıcı firmadır; tahsilat firmanın kendi sağlayıcı hesabına geçer, kart bilgisi sağlayıcının sayfasında girilir ve ürün kart verisini görmez, saklamaz, loglamaz (`02 §3.21.1`).
  2. İki ödeme yöntemi vardır: kart ve havale/EFT; kapıda ödeme yoktur (`02 §3.21.2`).
  3. Her kart ödemesi istisnasız 3D Secure'den geçer; doğrulama başarısızsa sipariş ödenmiş sayılmaz; tutar eşiği ya da sağlayıcı takdiri yoktur (`02 §3.21.3`).
  4. Kartın panelde açık/kapalı anahtarı yoktur; sağlayıcı anahtarları tanımlıysa açık, değilse görünmez ve satış yalnız havaleyle açılır (`02 §3.21.4`).
  5. Ürün taksiti bilmez; hiçbir yerde taksit bilgisi gösterilmez ve sipariş tek tutar taşır (`02 §3.21.9`).
  6. Sipariş hazırlığa ancak ödeme onaylandığında geçer (`02 §3.21.10`).
  7. Ters ibraz ürünün dışındadır; ürün sağlayıcıdan ters ibraz bilgisi almaz ve ödeme eksenini değiştirmez (`02 §3.21.11`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: kart verisi kayıtta, veritabanında ve günlükte yok.
  - KK2: yalnız iki yöntem sunulur.
  - KK3: 3D başarısızsa ödeme onaylanmaz, eşik yok.
  - KK4: anahtarsız kurulumda kart görünmez, havale açık kalır.
  - KK5: taksit bilgisi hiçbir ekranda ve modelde yok.
  - KK6: ödeme onayı olmadan hazırlığa geçiş reddedilir.
  - KK7: ters ibraz için uç, durum ve alan yok.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T353: Havale/EFT: IBAN ve siparişe donması, ödendi işareti, hatırlatma, süresi dolmuş sipariş
- **Bağımlılık:** T317, T324, T352
- **Dokümanlar:** çekirdek; `09` §9, §11, §13; `02 §3.21.5`…§3.21.8, §4.1.2, Z-8, Z-9, §6.2, §6.7, §6.9, §7.2.8, §7.6.2, §9.2, B-2, B-3, B-13, §9.3.3, F-5, §10.4.6, §10.6.1, §11.1, P-7, P-46; `10` KP-15, KP-46, KP-59, KD-3
- **Kabul kriterleri:**
  1. Havale panelden açılıp kapatılır; açıkken IBAN zorunludur; kapalıyken yöntem ödemede görünmez (`02 §3.21.5`).
  2. Açık ödenmemiş sipariş tavanı (L-8) dolunca havale o müşteriye seçilemez ve kart yolu açık kalır (`02 §3.21.5`).
  3. IBAN siparişe donar; IBAN'ın girilmesi ve değişmesi bütün yöneticilere e-postayla bildirilir (`02 §3.21.5`; `02 §9.3.3`).
  4. Havale ile onaylayan müşteri donan IBAN'ı ve sipariş numarasını görür; firma "ödendi" işaretleyince ödeme onaylanır (`02 §3.21.6`).
  5. Süre dolmadan müşteriye bir kez, son iş gününün ilk gönderim çalışmasında hatırlatma gider (`02 §3.21.6`).
  6. Gelen tutar alanı, tutar karşılaştırması ve "eksik ödeme" durumu yoktur (`02 §3.21.7`).
  7. Süresi dolup iptal edilmiş sipariş sonradan "ödendi" işaretlenemez; kesintide dolan sürenin iptali ertelenir; sınır yönetici düzeltmesinin de dışındadır (`02 §3.21.8`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: IBAN'sız havale açılmaz, kapalıyken yöntem yok.
  - KK2: tavan dolunca havale kapanır, kart açık kalır.
  - KK3: IBAN siparişte sabit, değişiklik e-postası gider.
  - KK4: ödendi işareti ödemeyi onaylar.
  - KK5: hatırlatma bir kez ve doğru günde gider.
  - KK6: eksik ödeme durumu ve gelen tutar alanı yok.
  - KK7: iptalli siparişte işaret reddedilir, yönetici de aşamaz.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T354: Sipariş numarası ve takip erişimi: numara, üç giriş yolu, erişim anahtarı
- **Bağımlılık:** T340, T341
- **Dokümanlar:** çekirdek; `09` §10, §15; `02 §3.22.1`…§3.22.5, §6.1.1, §6.3, §8.2, L-5, §8.3.5, §8.3.6, §10.4.11, §11.2, P-19, P-35; `10` KP-17, KP-18
- **Kabul kriterleri:**
  1. Sipariş numarası yıl ve rastgele bloktur; karışabilen karakterler kullanılmaz ve blok uzunluğu §11 değeridir (`02 §3.22.1`).
  2. Numara sipariş oluşurken verilir, iptalde de yeniden kullanılmaz ve tekildir; çakışmada sipariş oluşmadan yenisi üretilir; fatura numarası değildir (`02 §3.22.2`).
  3. Sipariş sayfasına üç yoldan girilir: üye geçmişten, misafir numara ve e-postayla, e-posta bağlantısıyla (`02 §3.22.3`).
  4. Misafir ve silinmiş hesap için e-posta bağlantısı siparişe özgü, tahmin edilemez bir erişim anahtarı taşır ve e-posta sormaz (`02 §3.22.5`).
  5. Üyenin bağlantısı hesaptaki sayfanın adresidir ve girişsiz açılmaz; numara tek başına sayfayı açmaz (`02 §3.22.5`).
  6. Sipariş sayfası durum, takip bağlantısı, indirme düğmesi ve donmuş IBAN'ı taşır; hiçbir akış e-postanın ulaşmasına bağlı değildir (`02 §3.22.4`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: numara biçimi ve karakter kümesi doğru.
  - KK2: çakışma ve iptalde yeniden kullanım yok.
  - KK3: üç giriş yolu çalışır.
  - KK4: anahtarlı bağlantı e-posta sormadan açar, anahtar tahmin edilemez.
  - KK5: üye bağlantısı girişsiz açılmaz, yalnız numara reddedilir.
  - KK6: sayfa dört bilgiyi taşır, e-postasız da erişilir.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T355: Sipariş anında donan değerler
- **Bağımlılık:** T338, T344, T349
- **Dokümanlar:** çekirdek; `09` §9, §10; `02 §3.23.1`…§3.23.5, §3.8.3, §3.9.6, §3.14.5, §3.19.6, §3.24.6, Z-39, §5.3, §5.11.4, §10.4.2, §11.1, P-8, §12.1.6, §12.1.7; `10` KP-17
- **Kabul kriterleri:**
  1. Kalem ödenen tutarı üreten değerleri (birim fiyat, KDV oranı ve tutarı, indirim oranı, referans fiyat, kupon payı) ve ürünün o anki tanımını (ad, seçenekler, görsel) dondurur (`02 §3.23.1`).
  2. Teslim hattını ve cayma rejimini belirleyen nitelikler de donar: ürün tipi, istisna işareti, sebebi ve koşulu (`02 §3.23.1`).
  3. Donan değerler kalemde düz yaşar; sipariş düzleminde yalnız kupon kodu, kargo ücreti ve toplamlar durur; ara gruplama katmanı yoktur (`02 §3.23.2`).
  4. Sipariş adreslerini, iletişim e-postasını, firmanın o günkü yasal kimliğini, Ön Bilgilendirme Formu ve sözleşme sürümlerini, onay kutusu kaydını, havale IBAN'ını ve aydınlatma sürümünü dondurur (`02 §3.23.3`).
  5. Donma anı siparişin oluştuğu andır; onaylanan tutar bağlayıcıdır ve ödeme sonucundan bağımsızdır; kargoya verme sözü de o anda donar (`02 §3.23.4`).
  6. Marka ayarları siparişe donmaz; geçmiş sipariş güncel logo, ad ve renkle görünür (`02 §3.23.5`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: ürün sonradan değişince kalem eski değerleri korur.
  - KK2: tip ve istisna nitelikleri donar.
  - KK3: kalem alanları düzdür, ara katman tablosu yok.
  - KK4: adres, kimlik, sürüm, kutu, IBAN donar.
  - KK5: donma anı doğru, söz onayda sabit.
  - KK6: marka değişikliği eski siparişte görünür.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T356: Onay adımı: onay kutuları, Ön Bilgilendirme Formu içeriği, işlem rehberi
- **Bağımlılık:** T350, T355, T366
- **Dokümanlar:** çekirdek; `09` §9, §11, §12; `02 §3.24.1`…§3.24.8, §6.1.3, §7.3.2…§7.3.4, §7.4.2, §7.4.4, §8.4.1, §8.4.3, §12.1.2, §12.1.7, §12.2.2, §12.2.4, §12.2.12; `05` MK-56; `10` KP-6, KP-8, KP-14, KP-35, KD-29; `devir` 04-1948
- **Kabul kriterleri:**
  1. Sipariş iki ayrı kutuyla onaylanır (Ön Bilgilendirme Formu okundu, sözleşme onayı); biri eksikse onaylanamaz; KVKK rızası kutusu yoktur (`02 §3.24.1`).
  2. Dijital kalem için üçüncü, hizmet kalemi için ayrı kutu çıkar; işaretlenmeden o kalem siparişe giremez (`02 §3.24.2`).
  3. Form onay özetiyle birebir aynıdır ve dört bilgi ekler: il kısıtı, kargoya verme süresi, cayma koşulları ve süresi, firma kimliği (`02 §3.24.3`).
  4. Özeti ya da sürümü değiştiren her fark siparişi durdurur (`02 §3.24.3`).
  5. Yaş beyanı kutusu, doğum tarihi alanı ve üyelik sözleşmesi yoktur (`02 §3.24.4`).
  6. Misafir e-postası kodla sınanmaz; özette gösterilir ve onaydan önce düzeltilebilir (`02 §3.24.5`).
  7. Onaylanan sürümler, firma kimliği ve işaretlenen kutular metniyle ve kapsadığı kalemlerle siparişe donar (`02 §3.24.6`).
  8. Ödeme adımında müşteri notu ya da teslimat talimatı alanı yoktur (`02 §3.24.7`).
  9. "İşlem rehberi" sabit sayfası altbilgide her sayfada bağlantılıdır ve sabit metni taşır (`02 §3.24.8`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: tek kutuyla onay reddedilir, rıza kutusu yok.
  - KK2: dijital ve hizmet kutuları kalemi koşullar.
  - KK3: form özetle eşit, dört ek bilgi var.
  - KK4: sürüm artışı siparişi durdurur.
  - KK5: yaş ve doğum tarihi alanı yok.
  - KK6: e-posta özette görünür ve düzeltilebilir.
  - KK7: kutu kaydı ve sürümler siparişe donar.
  - KK8: not alanı ve ucu yok.
  - KK9: rehber bağlantısı ana sayfada da görünür.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T357: Fatura verisi ve dışa aktarmada donmuş kimlik
- **Bağımlılık:** T349, T355
- **Dokümanlar:** çekirdek; `09` §9, §10; `02 §3.25.1`…§3.25.3, §10.5.3, §10.7.1, §12.1.10, §12.5; `10` KP-52, KD-1
- **Kabul kriterleri:**
  1. Ürün fatura kesmez; e-Arşiv ve e-Fatura entegrasyonu yoktur (`02 §3.25.1`).
  2. Sistem fatura verisini taşır: kalem dökümü, KDV, indirim ve kupon payı, kargo ücreti ve oranlara bölünmüş KDV'si, fatura adresi, donmuş kimlik, ödeme ve kargo tarihleri, alan adı; sipariş dışa aktarmasıyla verilir (`02 §3.25.1`).
  3. Fatura sistemde saklanmaz ve gösterilmez; sipariş sayfasında yükleme, iletme ve arşiv yoktur (`02 §3.25.2`).
  4. Dışa aktarmada siparişin donmuş firma kimliği kullanılır, panelin o anki kimliği değil (`02 §3.25.3`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: fatura kesen ya da entegre eden uç yok.
  - KK2: dışa aktarma bütün alanları taşır, kargo KDV'si bölünmüş.
  - KK3: fatura dosyası için alan, uç ve ekran yok.
  - KK4: firma kimliği değişince eski sipariş dışa aktarması eski kimliği verir.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T358: Kurumsal içerik tipleri: Hakkımızda, hizmet tanıtımı, referans iş, SSS, şube, genel sayfa, sosyal bağlantılar
- **Bağımlılık:** T322, T339
- **Dokümanlar:** çekirdek; `09` §10, §15; `02 §3.27.1`…§3.27.9, §3.27.12, §2.3, §6.8, §11.2, P-28, P-29; `10` KP-32, KP-33, KP-36, KP-54, KD-14, KD-39, KD-41…KD-45, KD-63
- **Kabul kriterleri:**
  1. İçerik hazır tiplerle ve sabit düzenli genel sayfayla girilir; sayfa kurucu yoktur (`02 §3.27.1`).
  2. Beş hazır tip (Hakkımızda, hizmet tanıtımı, referans iş, SSS, şube) vardır; genel sayfa ve duyuru eklenir; galeri tipi yoktur (`02 §3.27.2`).
  3. Hakkımızda tek kayıttır; ikincisi açılmaz, silinmez, yalnız taslağa alınır (`02 §3.27.3`).
  4. Hizmet tanıtımı fiyatsızdır, satın alınmaz; "Bize ulaşın" iletişim formuna götürür (`02 §3.27.4`).
  5. Referans iş başlık, kısa açıklama, metin ve görselden oluşur; müşteri adı, yıl, kategori alanı yoktur (`02 §3.27.5`).
  6. SSS soru ve cevaptır; tek sayfada listelenir, başlıklara bölünmez (`02 §3.27.6`).
  7. Şube İletişim sayfasında listelenir; ad ve adres zorunlu, il kapalı listeden; gömülü harita yok (`02 §3.27.7`).
  8. İletişim sayfası ayrı tip değildir; kimlik bilgileri, şubeler ve form birleşir (`02 §3.27.8`).
  9. Genel sayfa adet sınırsızdır; yasal metin genel sayfa olarak girilmez (`02 §3.27.9`).
  10. Sosyal bağlantılar kapalı altı platformla ve isteğe bağlı WhatsApp numarasıyla girilir; siparişe donmaz (`02 §3.27.12`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: sayfa kurucu ucu yok.
  - KK2: tip listesi kapalı, galeri tipi yok.
  - KK3: ikinci Hakkımızda ve silme reddedilir.
  - KK4: hizmette sepet yok, form bağlantısı var.
  - KK5: referans işte ek alan yok.
  - KK6: SSS tek listedir.
  - KK7: şubede zorunlu alan ve harita bağlantısı doğru.
  - KK8: iletişim sayfası kendi kaydı yok.
  - KK9: genel sayfada sınır yok.
  - KK10: platform listesi kapalı, donma yok.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T359: Kurumsal içerik: ürün bağı, elle sıralama, zorunlu alanlar, alan tavanları, kapsam dışı yetenekler
- **Bağımlılık:** T335, T358
- **Dokümanlar:** çekirdek; `09` §10, §15; `02 §3.27.10`, §3.27.11, §3.27.13…§3.27.20, §6.8, §11.2, P-22, P-23, P-25, P-27…P-29; `10` KP-32, KP-54, KD-8, KD-34, KD-44, KD-63
- **Kabul kriterleri:**
  1. Hizmet ve referans işe katalogdan ürün bağlanır; yalnız yayındakiler "İlgili ürünler"de görünür, silinen bağdan kalkar (`02 §3.27.10`).
  2. Beş liste elle sıralanır; vitrin, ana sayfa ve menü sırayı izler; ürün listeleri etkilenmez (`02 §3.27.11`).
  3. Hiçbir tip zorunlu değildir; yayın kapısı tipe göre zorunlu alanlardır (`02 §3.27.13`).
  4. Hakkımızda boşken ya da yayında değilken kurumsal blok marka adı ve logoyu gösterir (`02 §3.27.14`).
  5. Görseller ve kısa alanlar ürün kurallarına tabidir: ana görsel sıradan bağımsız, alternatif metin isteğe bağlı, tavanlar parametrede (`02 §3.27.15`).
  6. Uzun metin ürünün metin biçimi setini kullanır (`02 §3.27.16`).
  7. Gömülü video ve dosya eki yoktur; video bağlantısı platformunda açılır (`02 §3.27.17`; `02 §3.27.18`).
  8. Müşteri görüşü tipi, dönen alıntı bloğu ve blog yoktur (`02 §3.27.19`; `02 §3.27.20`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: taslak ürün bağda kalır, görünmez.
  - KK2: sıra liste, ana sayfa, menüde aynı.
  - KK3: eksik alanlı kayıt yayına alınamaz.
  - KK4: boş Hakkımızda blokta marka adı çıkar.
  - KK5: alt metin boşsa ad ve sıra kullanılır.
  - KK6: biçim seti dışı etiket temizlenir.
  - KK7: video oynatıcı ve ek yükleme ucu yok.
  - KK8: görüş tipi ve blog uçları yok.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T360: Duyuru şeridi, boş kurulum, içerik yayın durumu, önizleme ve silme
- **Bağımlılık:** T322, T326, T358
- **Dokümanlar:** çekirdek; `09` §10, §13; `03 §10.2`; `02 §3.27.21`…§3.27.28, §2.3, §4.1.4, Z-23, Z-24, §5.2, §6.8, §7.6.4, §10.8.1, §11.2, P-30; `10` KP-36, KP-54, KP-57, KD-38, KD-45
- **Kabul kriterleri:**
  1. Duyuru tektir: her sayfanın üstünde bir satır metin ve isteğe bağlı bağlantı; tarih girilirse yalnız aralıkta görünür (`02 §3.27.21`).
  2. Site boş kurulur; içerik, katalog ve duyuru örnek kayıt taşımaz (`02 §3.27.22`).
  3. Her kayıt Taslak ya da Yayında'dır; arşiv yoktur; ikinci onay yoktur (`02 §3.27.23`).
  4. Taslak yalnız adla kaydedilir; yayına almak zorunlu alanları ister; eksik alanı panel gösterir (`02 §3.27.24`).
  5. Taslak sayfa ziyaretçiye "sayfa bulunamadı" döner, yöneticiye "Taslak" bandıyla görünür (`02 §3.27.25`).
  6. Sürüm geçmişi yoktur; kayıt hemen yayına girer; yayına alma ve kaldırma işlem izindedir (`02 §3.27.26`).
  7. Kalıcı silme geri alınamaz uyarısıyla yapılır ve menü, işaret ve ürün bağlarını kaldırır; Hakkımızda silinmez (`02 §3.27.27`).
  8. İleri tarihli yayın yoktur; tek istisna duyuru aralığıdır (`02 §3.27.28`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: aralık dışında duyuru görünmez.
  - KK2: temiz kurulumda örnek kayıt yok.
  - KK3: iki durum var, arşiv ve onay yok.
  - KK4: eksik alanlı yayına alma reddedilir.
  - KK5: taslak bandı yalnız yöneticide.
  - KK6: eski sürüme dönüş yok, izde kim yazılı.
  - KK7: silme uyarılı, Hakkımızda silinmez.
  - KK8: ileri tarih yalnız duyuruda.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T361: Ana sayfa düzeni ve üst menü
- **Bağımlılık:** T333, T358
- **Dokümanlar:** çekirdek; `09` §10; `02 §3.28.1`…§3.28.6, §2.3, §6.8; `10` KP-1, KP-31, KP-55, KD-39, KD-40
- **Kabul kriterleri:**
  1. İki hazır ana sayfa düzeninden biri seçilir; kurumsal tanıtım ve vitrin birlikte durur; vitrin en yeni yayındaki ürünlerdir (`02 §3.28.1`).
  2. Kurumsal blok her zaman Hakkımızda'dır; uzun metin kendi sayfasında durur (`02 §3.28.2`).
  3. "Ana sayfada göster" işaretli hizmet ve referanslar elle sırayla görünür; işaretli yoksa blok çıkmaz (`02 §3.28.3`).
  4. Menü düzenleyici yoktur; sabit iskelette kaydı olmayan tip görünmez; firma menü adlarını değiştirebilir (`02 §3.28.4`).
  5. Genel sayfalar altbilgide listelenir; "menüde göster" işaretiyle üst menüye çıkar (`02 §3.28.5`).
  6. Yayında ürünü olmayan kategori menüde görünmez; boş kategori adresi boş sayfa gösterir (`02 §3.28.6`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: düzen seçimi iki değerle sınırlı.
  - KK2: blokta kısa tanıtım ve ana görsel var.
  - KK3: işaretsiz blok hiç görünmez.
  - KK4: menü adı değişir, tip kaydı yoksa gizlenir.
  - KK5: genel sayfa altbilgide, işaretliyse menüde.
  - KK6: boş kategori menüde yok, adresi 404 vermez.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T362: Marka kimliği: logo, marka adı, site simgesi, marka rengi, platform imzası
- **Bağımlılık:** T330, T339
- **Dokümanlar:** çekirdek; `09` §10, §15; `02 §3.29.1`…§3.29.6, §6.9, §9.1.4, §11.2, P-22, P-23; `10` KP-56, KD-46, KD-47
- **Kabul kriterleri:**
  1. Dört ayar vardır: logo, marka adı, site simgesi, tek marka rengi; vitrine ve e-postaya uygulanır, panele değil; koyu tema yoktur (`02 §3.29.1`).
  2. Logo zorunlu değildir; yoksa marka adı yazıyla gösterilir; tek logo her yerde kullanılır (`02 §3.29.2`).
  3. Marka adı unvandan ayrı ve zorunludur; boşaltılamaz; girilene kadar alan adı görünür (`02 §3.29.3`).
  4. Site simgesi yoksa marka adının baş harfi marka rengi zeminde çıkar (`02 §3.29.4`).
  5. Yazı rengini (siyah ya da beyaz) sistem yeterli kontrastla seçer; anlam renkleri sabittir (`02 §3.29.5`).
  6. "Shopfolio ile kuruldu" imzası varsayılan açıktır, yalnız altbilgidedir; kapatma işlem izine yazılır (`02 §3.29.6`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: dört ayar var, panel görünümü sabit.
  - KK2: logosuz sitede ad yazıyla çıkar.
  - KK3: marka adı boşaltılamaz.
  - KK4: simge yoksa baş harf çıkar.
  - KK5: açık ve koyu renklerde yazı okunur.
  - KK6: imza anahtarı çalışır, e-postada imza yok.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T363: Sayfa adresleri, arama motoru bilgileri, kapalı sayfalar ve paylaşım
- **Bağımlılık:** T313, T333, T335, T358
- **Dokümanlar:** çekirdek; `09` §10; `02 §3.30.1`…§3.30.7; `10` KP-7, KP-36, KP-37, KD-48, KD-49
- **Kabul kriterleri:**
  1. Ürün ve kategori adresi ağaçtaki yerden bağımsız, adından üretilir (`02 §3.30.1`).
  2. Adres ilk yayında sabitlenir; Türkçe harfler sadeleşir; çakışmada sayı eklenir; ad boşsa kök tür adıdır (`02 §3.30.2`).
  3. Yönlendirme sistemi yoktur (`02 §3.30.3`).
  4. Olmayan, silinmiş ya da taslak adres sitenin görünümünde "sayfa bulunamadı"ya düşer (`02 §3.30.4`).
  5. Arama motoru bilgileri otomatik üretilir; elle SEO alanı yoktur (`02 §3.30.5`).
  6. Sepet, ödeme, hesap, takip, panel, arama sonuçları, taslak ve arşiv sayfası arama motorlarına kapalıdır (`02 §3.30.6`).
  7. Tek "Paylaş" düğmesi cihaz menüsünü ya da bağlantı kopyalamayı kullanır; platform kodu yoktur (`02 §3.30.7`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: kategori taşınınca ürün adresi değişmez.
  - KK2: sadeleşme, çakışma ve boş ad kökü doğru.
  - KK3: yönlendirme ucu yok.
  - KK4: üç hâl aynı sayfayı verir.
  - KK5: başlık ve açıklama kuralla üretilir.
  - KK6: kapalı sayfalar işaretli, haritada yok.
  - KK7: paylaşım betiği tek düğme, harici kod yok.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T364: Panelde eşzamanlı düzenleme ve siparişte çakışan işlemler
- **Bağımlılık:** T316, T317
- **Dokümanlar:** çekirdek; `09` §10; `02 §3.31.1`, §3.31.2, §6.6…§6.8, §10.1.4; `10` KP-76
- **Kabul kriterleri:**
  1. Sessiz üzerine yazma yoktur: ikinci kaydeden kaydın değiştiğini görür ve üzerine yazmadan önce uyarılır (`02 §3.31.1`).
  2. Kural kurumsal içerik, ürün ve ayarlar için aynıdır (`02 §3.31.1`).
  3. Siparişe yönelik her işlem siparişin güncel hâline karşı uygulanır; eşzamanlı işlemde önce tamamlanan uygulanır (`02 §3.31.2`).
  4. Yöneticinin gördüğü hâlden sonra sipariş değiştiyse işlem uygulanmaz; panel değiştiğini söyler ve güncel hâli gösterir (`02 §3.31.2`).
  5. Aynı geçiş, aynı geri ödeme kaydı ve aynı kalem çıkarma iki kez işlenmez (`02 §3.31.2`).
  6. Kural müşterinin sayfadaki işlemiyle ve sistemin kendiliğinden geçişiyle çakışmada da işler; sonraki işlem uygulanır ya da sebebi söylenerek reddedilir; sipariş kilitlenmez (`02 §3.31.2`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: eski sürümle kayıt uyarı verir, sessiz yazmaz.
  - KK2: içerik, ürün ve ayarda aynı davranış.
  - KK3: iki yönetici aynı işlemde yalnız biri uygulanır.
  - KK4: değişmiş siparişte ret ve güncel hâl gösterimi var.
  - KK5: çift geçiş ve çift geri ödeme kaydı oluşmaz.
  - KK6: müşteri ve sistem çakışmasında sebep söylenir, kilit yok.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T365: İletişim talebi: form, konu tipi, durumlar, saklama, istenmeyen ileti koruması
- **Bağımlılık:** T308, T322, T331, T366
- **Dokümanlar:** çekirdek; `09` §3 (§3.4), §10, §15; `02 §3.15.4`, §3.32.1…§3.32.10, Z-33, §5.13, §6.8, §7.1.5, §8.2, L-4, §8.3.1, §8.4.1, §9.3, F-2, §9.5, §11.2, P-34, P-40, §12.2.6, §12.2.12; `10` KP-30, KP-34, KP-49, KD-42, KD-50, KD-51, KD-57
- **Kabul kriterleri:**
  1. Form girişsizdir; üye girişliyse ad ve e-posta ön dolu ve değiştirilebilir; her gönderim bir talep kaydı oluşturur (`02 §3.32.1`).
  2. Ad, e-posta, konu tipi ve mesaj zorunlu, telefon isteğe bağlıdır; dosya eki ve sipariş numarası alanı yoktur (`02 §3.32.2`).
  3. Konu tipi kapalı beş değerdir; ayıp talebi listede yoktur (`02 §3.32.3`).
  4. Talep Açık doğar; firma kapatır ve yeniden açabilir; KVKK talebi aynı iki durumu kullanır (`02 §3.32.4`).
  5. Saklama son kapatılmadan işler, kapatılmamış talep açılıştan itibaren imha edilir; "Sipariş hakkında" on yıl saklanır (`02 §3.32.4`).
  6. Talep kaydedilir ve firmaya bildirim e-postası gider; yanıt ekranı ve yazışma dizisi yoktur (`02 §3.32.5`).
  7. Müşteriye talep numarası, "taleplerim" sayfası ya da alındı e-postası yoktur (`02 §3.32.6`).
  8. Görünmez tuzak alan dolarsa gönderim sessizce düşer; IP başına gönderim limiti vardır; captcha yoktur (`02 §3.32.7`).
  9. Aydınlatma ya da çerez politikası yayında değilse form kapalıdır ve formda aydınlatma bağlantısı görünür (`02 §3.32.8`).
  10. Şikâyet kanalı ve canlı destek yoktur; indirme hakkı yenileme başvurusu "Sipariş hakkında" tipiyle gelir (`02 §3.32.9`; `02 §3.32.10`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: girişsiz gönderim çalışır, üyede alanlar dolu.
  - KK2: eksik zorunlu alan reddedilir, ek alanı yok.
  - KK3: altı değer ve ayıp tipi yok.
  - KK4: durum geçişleri iki durumla sınırlı.
  - KK5: saklama sayacı iki yolda doğru.
  - KK6: bildirim gider, yanıt ucu yok.
  - KK7: müşteri tarafında takip ucu ve e-posta yok.
  - KK8: dolu tuzak sessiz düşer, limit aşımı reddedilir.
  - KK9: metin yayında değilken form kapalı.
  - KK10: şikâyet ve sohbet uçları yok.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T366: Yasal metinler: dört metin, sürüm artışı, aydınlatma bağlantıları, e-postada tam metin
- **Bağımlılık:** T305, T330
- **Dokümanlar:** çekirdek; `09` §10, §11, §12; `02 §3.24.3`, §3.33.1…§3.33.9, §6.5, §6.9, §7.1.5, §7.4.4, §10.8.2, §12.1.2, §12.1.7, §12.2.1…§12.2.5, §12.2.8, §12.5; `10` KP-14, KP-25, KP-35, KP-61, KP-73, KD-53, KD-55, KD-56; `devir` 04-1948
- **Kabul kriterleri:**
  1. Dört yasal metin vardır (Ön Bilgilendirme Formu, sözleşme, aydınlatma, çerez); sürümlüdür, genel sayfa olarak girilmez; ilk ikisi üretilir, son ikisini ürün taslakla getirir ve firma düzenler (`02 §3.33.1`).
  2. Firmanın düzenlediği iki metin boşaltılamaz; barındırma konumu bölümü doldurulmadan aydınlatma tamamlanmış sayılmaz (`02 §3.33.2`).
  3. Sürüm kendiliğinden artar: firma metni yayına alınca, üretilen metni besleyen ayar değişince, ürün şablonu değişince; elle girilmez (`02 §3.33.3`).
  4. Metin değişince yeniden onay alınmaz; açık onay adımında sürüm artarsa sipariş oluşmaz; yeni sürüm yalnız yeni siparişlere uygulanır; "kabul et" penceresi ve değişiklik bildirimi yoktur (`02 §3.33.4`).
  5. Aydınlatma bağlantısı yeni kişisel veri alanı açan her ekranda görünür; çerez politikası altbilgidedir; çerez onay bandı yoktur (`02 §3.33.5`).
  6. Ön Bilgilendirme Formu ve sözleşme onay e-postasının gövdesinde donan sürümleriyle tam metin gider; dosya eki yoktur; firma donmuş sürümle yeniden gönderebilir (`02 §3.33.6`).
  7. Form ve sözleşme uyuşmazlık başvuru bilgisini yazar; parasal görev sınırı rakamla yazılmaz (`02 §3.33.7`).
  8. Ayarlarla çelişen serbest metin firmanın sorumluluğundadır; ürün çelişkiyi tespit etmez (`02 §3.33.8`).
  9. Dört metin tamamlanmadan satış açılmaz; panel eksik metni adıyla gösterir (`02 §3.33.9`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: dört metin ayrı kayıt, genel sayfa değil.
  - KK2: boşaltma reddedilir, bölüm eksikse tamamlanmamış.
  - KK3: değeri değişmeyen kayıt sürüm yazmaz.
  - KK4: açık adımda sürüm artışı siparişi durdurur.
  - KK5: bağlantı sayılan ekranlarda ve altbilgide var.
  - KK6: e-posta gövdesinde tam metin, ek dosya yok.
  - KK7: metinde mercii bilgisi var, rakam yok.
  - KK8: çelişki denetimi ve bilgi sayfası yok.
  - KK9: eksik metin satış kapısını kapatır, adıyla görünür.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T367: Müşteri iptali: kalem ve adet düzeyi, kargoya verilene kadar, dijital ve hizmet sınırı, sayaçların dönmesi, kısmi iptalde kupon
- **Bağımlılık:** T316, T318, T320, T334, T338, T348, T350
- **Dokümanlar:** çekirdek; `09` §9, §10, §14; `02 §7.1.1`, §7.1.4, §7.2.1, §7.2.2, §7.2.5…§7.2.7, Z-39, §5.8, §5.9, §5.11.3, §6.4, §7.3.7, §9.2, B-7, §10.4.3; `10` KP-21, KP-24
- **Kabul kriterleri:**
  1. Müşteri fiziksel kalemi sipariş "Kargoya verildi"ye geçene kadar sipariş sayfasından, firma onayı beklemeden iptal eder; sonrasında yol caymadır (`02 §7.2.1`).
  2. Firma hazırlamaya başladığı siparişin iptalini engelleyemez (`02 §7.2.1`).
  3. İptal kalem ve adet düzeyindedir; siparişin tamamı ancak bütün adetler iptal edilince İptal edildi olur; fiziksel kalemlerin tamamı iptal edilen karışık sipariş kalan hatta düşer (`02 §7.2.2`).
  4. Kalem ve adet iptali ödeme onayından sonra açılır; onaydan önce yalnız sipariş bütünüyle iptal edilir (`02 §7.2.2`).
  5. Dijital kalem ödeme onayından sonra iptal edilemez; hizmet kalemi "tamamlandı"ya kadar adetle iptal edilir (`02 §7.2.5`).
  6. İptal edilen adet stoğa, hizmet kontenjanı havuza döner; kupon hakkı yalnız siparişin tamamı iptal ya da iade edilince döner (`02 §7.2.6`).
  7. Kısmi iptalde kupon yeniden hesaplanmaz; iptal edilen adetlerin kupon payı geri ödemeye girer ve kalan tutar için asgari sepet tutarına bakılmaz (`02 §7.2.7`).
  8. Müşteri kendi iptalinin tarihli kayıt kopyası e-postasını alır (`02 §7.1.4`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: kargoya verildi sonrası iptal reddedilir, öncesi onaysız işler.
  - KK2: hazırlanan siparişte iptal engellenmez.
  - KK3: kısmi iptalde sipariş yaşar, tam iptalde kapanır.
  - KK4: ödeme öncesi kalem iptali reddedilir.
  - KK5: dijital iptal reddedilir, hizmet adetle iptal edilir.
  - KK6: stok, kontenjan ve kupon sayaçları doğru döner.
  - KK7: kupon payı geri ödemeye girer, kalanlar korunur.
  - KK8: iptal e-postası tarihle gider.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T368: Gecikme feshi ve firma iptali: iki fesih yolu, kanuni faiz uyarısı, kapalı sebep listesi, ifanın imkânsızlaşması bildirimi
- **Bağımlılık:** T306, T319, T325, T367
- **Dokümanlar:** çekirdek; `09` §9, §10, §14; `03 §4.1`; `02 §7.2.3`, §7.2.4, Z-11, Z-39, Z-47, §5.8, §5.9, §6.4, §8.3.8, §9.2, B-7, B-9, §10.4.8, §10.4.10, §12.5; `10` KP-21, KP-47
- **Kabul kriterleri:**
  1. Kargoya verme süresi aşılınca fesih mevcut iptal düğmesiyle kullanılır; otuz günlük yasal sınır aşılınca sipariş sayfasında ayrı kayıt tipiyle "gecikme nedeniyle fesih" düğmesi açılır (`02 §7.2.3`).
  2. Düğme, firmaya ulaşmadan otuz gün geçmiş, teslim tarihi girilmemiş, işleme açık adedi kalan fiziksel kalemli siparişte görünür ve cayma istisnalı ürünü de kapsar (`02 §7.2.3`).
  3. Fesih teslim edilmemiş fiziksel kalemlerin açık adetlerinin tamamına uygulanır; kalem ve adet seçilmez, teslim işaretli dijital ve hizmet kalemi dışarıda kalır (`02 §7.2.3`).
  4. Feshedilen kalem kargoya verilmez ve firma iptaline konu olmaz; açık kalem kalmazsa sipariş İptal edildi'ye geçer, B-7 üretmez (`02 §7.2.3`).
  5. Başka kanaldan bildirilen fesih panelden kaydedilir, düğmeyle yapılmış gibi işler; süre bildirimin ulaştığı tarihten başlar (`02 §7.2.3`; §10.4.10).
  6. Panel fesih satırında ve taahhüt süresi aşıldıktan sonraki iptalde kanuni faiz uyarısını gösterir; sistem faiz hesaplamaz (`02 §7.2.3`).
  7. Firma iptali kapalı altı sebepten birini seçer, "diğer"de açıklama zorunludur, geri alınamaz onayı ister ve sebep müşteriye B-7 ile gider (`02 §7.2.4`; §5.9).
  8. "Stokta bulunamadı" seçilince panel onaydan önce stok yokluğunun yasal imkânsızlık sayılmadığını söyler (`02 §7.2.4`).
  9. "Müşteriyle anlaşıldı" iptalinde talep siparişin e-postasından gelmediyse firma teyidi ister (`02 §7.2.4`; §8.3.8).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: iki fesih yolu ve ayrı kayıt tipi.
  - KK2: düğme koşulları ve istisnalı ürün.
  - KK3: seçimsiz, tüm açık adetler; dijital ve hizmet dışarıda.
  - KK4: kargo ve firma iptali yok, B-7 yok.
  - KK5: bildirim tarihinden işleyen süre.
  - KK6: faiz uyarısı görünür, hesap yok.
  - KK7: sebep listesi kapalı, onay istenir, B-7 gider.
  - KK8: stok uyarısı onaydan önce.
  - KK9: teyit istenmeden "anlaşıldı" iptali yok.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T369: İşlemin adet birimi ve geri ödeme tutarı: açık adet, kupon payı, KDV, kargo ücretinin iptalde ve caymada ayrılması
- **Bağımlılık:** T320, T338, T355, T367
- **Dokümanlar:** çekirdek; `09` §9, §14; `02 §7.1.3`, §7.1.6, §7.1.7, §7.2.7, §7.2.9, §7.4.6, §6.4, §7.2, §7.4; `10` KP-21, KP-22, KP-24
- **Kabul kriterleri:**
  1. İşlem birimi adettir; işlemin adedi kalemin işleme açık adedini aşamaz, açık adet iptal, çıkarma, gecikme feshi ve cayma beyanıyla kapanan adetler düşülerek okunur (`02 §7.1.6`).
  2. Bir işlemin kapattığı adet başka işlemin konusu olamaz; ayıpta sözleşmeden dönülen adet de işleme açık adetten düşer (`02 §7.1.6`).
  3. Ürün bedeli donmuş birim fiyatla işleme giren adedin çarpımıdır; kupon payı oranla ve aşağı yuvarlanır, KDV aynı oranla ve yuvarlamayla ayrılır (`02 §7.1.7` (1)–(3)).
  4. Kalemin son açık adetlerini kapatan işlem kupon payının ve KDV'nin kalanını alır; ayrılan paylar kalemin payına kuruşu kuruşuna eşittir (`02 §7.1.7` (4)).
  5. Teslim almaları parça parça gelen cayma beyanında her parçanın payı beyanın payından orantıyla ayrılır; son parça kalanı alır (`02 §7.1.7` (5)).
  6. Kısmi iptalde kupon yeniden hesaplanmaz ve kalan tutar için asgari sepet tutarına bakılmaz (`02 §7.2.7`).
  7. Kargoya verilmeden bütün fiziksel adetler iptal edilirse kargo ücreti son iptalin geri ödemesine eklenir; bir adet bile gidecekse alıkonur ve adede bölünmez (`02 §7.2.9`).
  8. Kısmi iadede kargo ücreti ve ücretsiz kargo kararı yeniden hesaplanmaz; kargoyla gönderilen bütün adetlerden cayılınca kargo ücreti son geri ödemeye eklenir (`02 §7.4.6`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: açık adedi aşan adet reddedilir.
  - KK2: kapanmış adet ikinci işleme girmez.
  - KK3: üç adet × 300 TL örneği 266,67 / 266,67 / 266,66.
  - KK4: paylar toplamı kalemin payına eşit.
  - KK5: parçalı teslim almada pay orantılı.
  - KK6: kısmi iptalde kupon sabit.
  - KK7: kargo ücreti iptalde tetiğe göre.
  - KK8: iadede kargo ücreti yeniden hesaplanmaz.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T370: Geri ödeme hattı: ödemenin geldiği yol, kart ve havale, IBAN'ın ömrü, geri ödeme süresi, kart iadesi hataları
- **Bağımlılık:** T317, T352, T353, T369
- **Dokümanlar:** çekirdek; `09` §9, §10, §11, §14; `05 MK-40`, §4.6; `02 §7.2.8`, §7.4.1, §7.4.5, §8.4.2, Z-16, Z-17, Z-35, Z-38, §4.3, §5.5, B-8, B-14, F-4, §10.4.9, §10.6.1, P-43; `10` KP-15, KP-21, KP-22; `03 §1.7`
- **Kabul kriterleri:**
  1. Geri ödeme ödemenin geldiği yoldan gider: kart sağlayıcı üzerinden karta, havale müşterinin IBAN'ına (`02 §7.4.5`).
  2. Kart hattında sistem iadeyi kendiliğinden başlatır, onay penceresi yoktur ve IBAN alanı açılmaz; havale hattında firma panelden işler (`02 §7.2.8`).
  3. Süre iptalde iptalden, teslimden önceki caymada beyandan, teslimden sonraki caymada malın ulaştığı tarihten başlar; başlangıç ikisinden geç olanıdır (`02 §7.4.1`).
  4. IBAN girişte biçim ve sağlamayla denetlenir, işlenene kadar müşterice düzeltilir; firma IBAN girmez (`02 §7.4.5`).
  5. IBAN geri ödeme tamamlanınca silinir; başka bekleyen geri ödeme kullanıyorsa ondan sonra silinir (`02 §7.4.5`).
  6. Firma iptalinde IBAN isteği iptal kaydıyla düşer, B-14 gider ve süre IBAN beklenirken durmaz (`02 §7.2.8`).
  7. "Havale gerçekleşmedi" bildirimi IBAN'ı siler, alanı yeniden açar ve B-14 gönderir (`02 §7.4.5`).
  8. Kart iadesi reddedilirse ödeme ekseni değişmez, "geri ödeme gerçekleşmedi" işareti düşer ve kalem bekleyen işlere girer (`02 §7.2.8`).
  9. Sonuç bilinmezse "geri ödeme sonucu bekleniyor" işareti düşer; sonuç beklenirken yeniden deneme ve havale yolu kapalıdır (`02 §7.2.8`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: yol ödeme yönteminden.
  - KK2: kartta alan yok, onay yok.
  - KK3: süre başlangıcı üç durumda.
  - KK4: IBAN denetimi ve düzeltme.
  - KK5: silme zamanı, ortak kullanımda erteleme.
  - KK6: firma iptalinde alan açılır, B-14.
  - KK7: havale hatasında alan yeniden açılır.
  - KK8: ret işareti, eksen sabit.
  - KK9: bekleme işaretinde deneme kapalı.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T371: Cayma hakkı: pencere, dijital ve hizmet istisnası, ürün istisna işareti, tek rejim
- **Bağımlılık:** T325, T332, T351, T356
- **Dokümanlar:** çekirdek; `09` §9, §10, §14; `02 §7.1.1`, §7.1.2, §7.3.1…§7.3.4, Z-13, Z-14, Z-15, §6.4, §7.4.9, §10.4.5; `10` KP-22, KP-45
- **Kabul kriterleri:**
  1. Cayma penceresi on dört gündür ve sabittir; firma uzatamaz, ayar değildir (`02 §7.3.1`).
  2. Pencere fiziksel kalemde teslim tarihinden, hizmette sipariş tarihinden, fiziksel kalemli siparişte teslimden işler (`02 §7.3.1`; Z-13, Z-14).
  3. Müşteri malı teslim almadan, sipariş kargoya verildikten sonra da cayabilir (`02 §7.3.1`).
  4. Pencere kapanınca düğme kapanır; önceden yapılmış beyan geçerli kalır ve pencere işaret anından yeniden başlatılmaz (`02 §7.3.1`).
  5. Dijital kalemde cayma hakkı yoktur; hak ödeme başarısında düşer ve ayrı onay kutusu zorunludur (`02 §7.3.2`; Z-15).
  6. Hizmette hak, firma "tamamlandı" işaretleyene kadar sürer; onay kutusu zorunludur ve ürün ifanın başlangıcını kaydetmez (`02 §7.3.3`).
  7. Kısmen ifa edilmiş hizmetten cayma tam caymadır; yapılmış kısım için bedel kesilmez (`02 §7.3.3`).
  8. Fiziksel üründe istisna işareti ürün düzeyinde isteğe bağlıdır; mutlak sebepte düğme yoktur, koşullu sebepte açıktır ve koşul yazılır (`02 §7.3.4`).
  9. İşaret ve sebep sipariş anında kaleme donar; sonraki ürün değişikliği verilmiş siparişi etkilemez (`02 §7.3.4`).
  10. Ürün tek tüketici rejimini işletir; alıcıdan ticari bilgi istemez (`02 §7.1.2`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: ayar yok, süre sabit.
  - KK2: başlangıç üç durumda.
  - KK3: kargodaki kalemde düğme açık.
  - KK4: kapanış ve eski beyanın korunması.
  - KK5: dijital düğme yok, hak ödemede düşer.
  - KK6: hizmet tamamlanana kadar açık.
  - KK7: kısmi ifada tam geri ödeme.
  - KK8: mutlak ve koşullu sebep düğmesi.
  - KK9: donma sonradan değişmez.
  - KK10: ticari bilgi alanı yok.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T372: Cayma beyanı: sipariş sayfasından ve başka kanaldan, beyanın etkisi, kargoya verilmemiş kalemde iptal yolu, firmanın teyidi
- **Bağımlılık:** T307, T320, T330, T371
- **Dokümanlar:** çekirdek; `09` §9, §10; `02 §7.3.5`…§7.3.7, §7.1.4, §7.1.6, §7.6.7, §5.6.6, §5.8, §5.9, §6.4, §8.3.8, §9.2, B-9, B-14, §9.3, F-3, §10.4.10, §12.5; `10` KP-22, KP-47
- **Kabul kriterleri:**
  1. Müşteri sipariş sayfasından kalemi ve adedi seçip beyan eder; beyan tarih damgasıyla kaydolur, ayrı form ya da noter kanalı gerekmez; aynı kalemde ardışık beyan yapılır (`02 §7.3.5`).
  2. Beyan ekranı güncel iade adresini ve on dört gün içinde gönderme yükümlülüğünü gösterir; adres beyan kaydına yazılır ve sonradan değişse de o beyan için değişmez (`02 §7.3.5`).
  3. Koşullu istisnalı üründe beyan ekranı koruyucu ambalaj koşulunu gösterir (`02 §7.3.5`).
  4. Beyan siparişi kapatmaz; sevkiyat durumu değişmez, ödeme durumu para geri ödenince değişir (`02 §7.3.6`).
  5. Müşteri kayıt kopyası e-postasını (B-9) alır, firmaya F-3 gider (`02 §7.1.4`; §9.2).
  6. Başka kanaldan gelen bildirim panelden kalem ve adetle kaydedilir; tarih bildirimin ulaştığı tarihtir, geçmişe dönük olabilir, ileri tarihli olamaz (`02 §7.3.5`; §10.4.10).
  7. Firma bildirimin siparişin sahibinden geldiğini teyit eder; teyit kaydın tarihini değiştirmez, IBAN yalnız siparişin e-postasından gelen bildirimde aktarılır (`02 §8.3.8`).
  8. Kargoya verilmemiş fiziksel kalemde cayma düğmesi yoktur; başka kanaldan gelen bildirim "müşteriyle anlaşıldı" iptali olarak işlenir ve cayma kaydı açılmaz (`02 §7.3.7`).
  9. Beyan geri alınmaz ve düzeltilmez; firmanın kaydettiği de aynıdır (`02 §7.6.7`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: damga, ardışık beyan, ek kanal zorunluluğu yok.
  - KK2: iade adresi beyan kaydında donar.
  - KK3: koşul metni yalnız koşullu üründe.
  - KK4: eksenler beyanla değişmez.
  - KK5: B-9 ve F-3 gider.
  - KK6: ileri tarih reddedilir.
  - KK7: teyitsiz kayıt yok, tarih sabit.
  - KK8: kargo öncesi iptal yolu, cayma kaydı yok.
  - KK9: geri alma ve düzeltme yolu yok.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T373: İade: iade kargo bedeli, ödemeli gönderim ve iade adresi, teslim alma adımı, stoğa ekleme, koşullu istisnada ret, kullanılmış mal
- **Bağımlılık:** T334, T370, T372
- **Dokümanlar:** çekirdek; `09` §9, §10, §14; `02 §7.4.1`…§7.4.4, §7.4.6…§7.4.10, Z-16, Z-42, Z-38, §5.8, §6.4, §8.4.2, B-14, B-16, §10.4.5, §10.4.9, §10.6.1, P-43; `10` KP-22, KP-24, KP-47; `devir` 04-1948
- **Kabul kriterleri:**
  1. Cayma ve ayıpta iade kargo bedeli firmaya aittir, bunu değiştiren ayar yoktur; müşteri masrafı önden ödemez (`02 §7.4.2`, §7.4.3, §7.4.4).
  2. Taşıyıcıyı müşteri seçer; ürün iade etiketi üretmez, firma malı geri almayı teklif etmez (`02 §7.4.4`).
  3. Müşteri malı beyandan on dört gün içinde gönderir; süre dolunca sistem kalemde kendiliğinden işlem başlatmaz (`02 §7.4.4`).
  4. Teslim alma adımı ulaşan adetle ve ulaşma tarihiyle yapılır; tarih geçmişe dönük olabilir, ileri tarihli ya da kalemin teslim tarihinden önce olamaz (`02 §7.4.7`).
  5. İade edilen adet stoğa kendiliğinden eklenmez; firma kontrolden sonra panelden ekler ve ekleme teslim alınan adet kadardır (`02 §7.4.7`).
  6. IBAN silindikten sonra ulaşan malda adım IBAN isteğini yeniden açar ve B-14 gider; firma IBAN girmez (`02 §7.4.5`).
  7. Koşullu istisnalı kalemde firma malı kabul etmek yerine "iade reddedildi — koruyucu ambalaj açılmış" kaydını seçebilir; ret yalnız bu kalemde sunulur (`02 §7.4.9`).
  8. Ret geri ödemeyi ve süreyi durdurur, kalemi bekleyen işlerden düşürür, stoğa eklemez, B-16 gönderir ve geri alınmaz (`02 §7.4.9`).
  9. İstisnasız ya da ambalajı açılmamış malda kullanılmış ya da hasarlı mal reddedilmez, kesinti yapılmaz; ürün değer kaybı tutarı kaydetmez (`02 §7.4.10`).
  10. Malı dönmeyen kalem süre geçince "mal dönmedi" ile kapatılır; geri ödeme olmaz, bildirim gitmez (`02 §10.4.9`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: iade masrafı ayarı yok.
  - KK2: etiket yok, taşıyıcı müşteride.
  - KK3: süre dolunca otomatik işlem yok.
  - KK4: tarih sınırları reddedilir.
  - KK5: stoğa ekleme ayrı adım, adet kadar.
  - KK6: IBAN yeniden istenir, B-14.
  - KK7: ret yalnız koşullu kalemde.
  - KK8: ret sonuçları ve B-16.
  - KK9: kesinti ve değer kaybı alanı yok.
  - KK10: erken kapatma reddedilir, geç kapatma işler.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T374: Ayıp talebi: kanal, iki yıllık süre, durumlar, çözüm, dijital dosya güncellemesi
- **Bağımlılık:** T320, T340, T351, T369
- **Dokümanlar:** çekirdek; `09` §9, §10; `02 §7.5.1`…§7.5.4, §7.1.1, §7.1.4, §7.4.3, Z-18, Z-29, §5.8, §5.10, §6.4, §9.2, B-9, §9.3, F-3, §12.1.9; `10` KP-23, KP-49, KD-42, KD-61
- **Kabul kriterleri:**
  1. Ayıp talebi sebeplidir; sipariş sayfasındaki kanal iki yıllık zamanaşımı boyunca açıktır ve süre sabittir, ayar değildir (`02 §7.5.1`).
  2. Süre teslim işaretinden işler: fiziksel kalemde teslim tarihi, hizmette "tamamlandı", dijitalde ödeme onayı (`02 §7.5.1`).
  3. Kanal "sorun bildir"dir: müşteri kalemi, adedi birden büyükse ayıplı adedi seçer ve açıklama yazar; dosya ya da fotoğraf yüklenmez (`02 §7.5.2`; KD-42).
  4. Kanal fiziksel kalemde "Kargoya verildi"den, dijitalde ödeme onayından, hizmette "tamamlandı"dan açılır (`02 §7.5.2`).
  5. Teslim tarihi girilmemişse süre işlemez; risk firmadadır (`02 §7.5.2`).
  6. Süre dolunca kanal kapanır ve müşteri iletişim formuna yönlenir; ayıp talebi iletişim formunun konu tiplerinde yoktur (`02 §7.5.1`, §7.5.2).
  7. Talep Açık ve Çözüldü durumludur; firma çözüldü işaretler, firma ya da müşteri yeniden açar ve ayıp tek kayıtta kalır; sistem çözümü yürütmez (`02 §7.5.3`).
  8. Sözleşmeden dönmede iade hattı geri alınan adetlerle, bedel indiriminde tutar bazlı kısmi geri ödeme işler; dönülen adet işleme açık adetten düşer (`02 §7.5.3`).
  9. Ayıplı dijital üründe firma dosyayı yeniden yükler ve düzeltilmiş dosya eski alıcılara da ulaşır; garanti süresi alanı yoktur (`02 §7.5.4`; KD-61).
  10. Müşteriye B-9, firmaya F-3 gider (`02 §7.1.4`, §9.2).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: süre sabit, ayar yok.
  - KK2: başlangıç üç tipte.
  - KK3: dosya alanı yok, adet seçimi.
  - KK4: kanalın açılış anı tipe göre.
  - KK5: tarihsiz kalemde süre işlemez.
  - KK6: kapanışta forma yönlendirme.
  - KK7: durumlar ve yeniden açma.
  - KK8: iki çözüm yolu doğru hatta.
  - KK9: dosya güncellemesi eski alıcıya ulaşır.
  - KK10: B-9 ve F-3 gider.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T375: Yöneticinin sipariş düzeltmeleri: ileri yönlü kayıt, tutar değişmez, firmanın teyidiyle e-posta ve adres düzeltmesi, cayma beyanı geri alınmaz
- **Bağımlılık:** T307, T344, T369, T372
- **Dokümanlar:** çekirdek; `09` §9, §10; `02 §7.6.6`, §7.6.7, §8.3.6, §8.3.7, §10.4.1, §10.4.2, §10.4.11, §6.1.1, §6.3, §9.2, B-1, B-3, B-10…B-13, B-15, §12.2.7; `05` §4.4; `10` KP-17, KP-28, KP-48, KD-13
- **Kabul kriterleri:**
  1. Düzeltmeler ileri yönlü kayıttır ve onaylanan tutarı değiştirmez; kalem eklenemez, fiyat değiştirilemez (`02 §7.6.6`).
  2. Müdahaleler kapalı dokuz maddelik listedir ve her biri işlem izine yazılır; elle sipariş oluşturma yoktur (`02 §10.4.1`; KD-13).
  3. Teslimat ve fatura adresi "Kargoya verildi"ye kadar düzeltilir; fiziksel kalemsiz siparişte fatura adresi Teslim edildi'ye kadar (`02 §10.4.2`).
  4. Adres düzeltmesi müşterinin talebiyle yapılır; talep siparişin e-postasından gelmediyse firma e-postayla ya da telefonla teyit eder (`02 §8.3.7`).
  5. Adres düzeltmesi B-10 ile bildirilir; eski ve yeni adres siparişin geçmişinde kalır (`02 §8.3.7`, §10.4.2).
  6. Misafir siparişinin e-postası yalnız firma teyidiyle değişir; müşteri sayfasında alan yoktur ve numara tek başına teyit değildir (`02 §8.3.6`).
  7. Teyit ad, teslimat telefonu, kalemler ve tutarla yapılır; düzeltme eski erişim anahtarını geçersiz kılar, eski adrese B-15 gönderir (`02 §8.3.6`).
  8. Eski ve yeni e-posta siparişin geçmişinde kalır; yeni adres doğrulanmış bir hesabınsa sipariş o hesaba düşer, eskisinden düşer (`02 §10.4.11`; KP-28).
  9. Cayma beyanı geri alınmaz ve düzeltilmez; firmanın başka kanaldan kaydettiği de aynıdır (`02 §7.6.7`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: tutar sabit, kalem ekleme yok.
  - KK2: dokuz müdahale, her biri izde.
  - KK3: adres sınırı iki durumda.
  - KK4: teyitsiz adres düzeltmesi yok.
  - KK5: B-10 gider, geçmiş durur.
  - KK6: müşteri e-posta alanı yok.
  - KK7: eski anahtar geçersiz, B-15 gider.
  - KK8: hesap bağı doğru taşınır.
  - KK9: beyanı geri alma yolu yok.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T376: Deneme limitleri: dokuz limit, kayan pencere, hesap kilitlenmez, nötr mesaj
- **Bağımlılık:** T342, T348
- **Dokümanlar:** çekirdek; `09` §15; `03 §6.1`; `02 §8.2`, §8.2.1, L-1…L-9, §5.3, §5.12, §6.1, §6.2, §6.3, §6.5, §8.1, §11.2, P-31…P-37, P-46, P-49; `10` KP-72
- **Kabul kriterleri:**
  1. Dokuz limit vardır: giriş, şifre sıfırlama, hesap kaydı, iletişim formu, misafir takip sorgusu, geçersiz kupon, iptal edilen ödenmemiş sipariş, açık ödenmemiş sipariş, doğrulama bağlantısı isteği (`02 §8.2` L-1…L-9).
  2. Eşik ve pencere değerleri `02 §11.2` P-31…P-37, P-46, P-49'dan okunur; kodda yeniden yazılmaz (`02 §11.2`).
  3. Sayaç kayan penceredir; engel pencere içindeki deneme eşiğin altına inince kendiliğinden kalkar (`02 §8.2.1`).
  4. Aşımda yalnız o işlem geçici engellenir; hesap kilitlenmez (`02 §8.2.1`).
  5. Kullanıcı nötr mesaj görür; mesaj kalan deneme sayısını ve engelin süresini söylemez (`02 §8.2.1`; §6.1.5).
  6. L-1 yeniden doğrulamadaki başarısız şifre denemesini de sayar (`02 §8.2` L-1).
  7. L-6 sepet başınadır; girişte birleşen sepet iki sayacın büyüğünü taşır (`02 §8.2` L-6; P-36).
  8. L-8 pencere taşımaz; ödemesi Bekliyor'daki açık sipariş sayısıdır ve biri ödenince ya da iptal edilince boşalır (`02 §8.2` L-8; P-46).
  9. Limit envanteri `02 §8.2` tablosunun dokuz satırıyla eşittir (`02 §8.2`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: dokuz limit adı ve kapsamı.
  - KK2: sabit sayı kodda yok.
  - KK3: pencere kayar, sahte saatle sınanır.
  - KK4: engel yalnız işleme, hesap açık.
  - KK5: mesajda sayı ve süre yok.
  - KK6: yeniden doğrulama sayılır.
  - KK7: birleşen sepette büyük sayaç.
  - KK8: L-8 pencere yok, boşalır.
  - KK9: envanter tabloyla eşit.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T377: Limit ilkeleri: iki eksen, ödenmemiş sipariş sayımı, yönetici muafiyeti yok, tanınan tarayıcı işareti
- **Bağımlılık:** T342, T376
- **Dokümanlar:** çekirdek; `09` §15; `03 §6.1`; `02 §8.2.2`…§8.2.6, §8.3.5, Z-27, Z-46, §6.1, §6.1.5, §11.2, P-48, §12.2.5; `10` KP-72
- **Kabul kriterleri:**
  1. IP ve e-posta ekseni ayrı sayılır; hangisi aşılırsa yalnız o eksen engellenir (`02 §8.2.2`).
  2. Kupon denemesi sepet ekseninde sayılır: misafirde tarayıcının, üyede hesabın sepeti (`02 §8.2.2`).
  3. L-5'in e-posta ekseni sorguya yazılan adresi sayar; siparişin var olup olmadığına bakmaz (`02 §8.2.2`).
  4. L-7 tekrarı ölçer; sağlayıcıya bağlanılamadığı için ödeme sayfası hiç açılmamış sipariş sayılmaz (`02 §8.2.3`).
  5. L-8 yalnız havale yolunu kapatır; kart yolu açık kalır, e-posta ekseni yalnız giriş yapmış üyenin siparişini sayar (`02 §8.2.3`).
  6. Yönetici aynı limitlere aynı değerlerle tabidir; panel girişine muafiyet yoktur (`02 §8.2.4`).
  7. Sepet kalem sayısına ve stok yoklamaya ürün düzeyinde limit konmaz (`02 §8.2.5`).
  8. Başarılı girişte tarayıcıya hesap için zorunlu, birinci taraf işaret çerezi bırakılır; e-posta ve şifre taşımaz (`02 §8.2.6`).
  9. L-1'in e-posta ekseni yalnız işaretsiz tarayıcıları sayar ve engeller; işaretli tarayıcı hesap ve tarayıcı başına kendi sayacındadır, IP ekseni aynen işler (`02 §8.2.6`).
  10. Şifre sıfırlayan tarayıcı da tanınan olur; Google girişi L-1'e girmez; işaretin ömrü son başarılı girişten 90 gündür (`02 §8.2.6`; P-48).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: iki eksen birbirinden bağımsız.
  - KK2: kupon sayacı sepette.
  - KK3: sorgudaki e-posta sayılır.
  - KK4: bağlantı hatası L-7'ye girmez.
  - KK5: L-8 yalnız havaleyi kapatır.
  - KK6: yöneticide muafiyet yok.
  - KK7: sepet ve stok limiti yok.
  - KK8: işaret çerezinde kişisel veri yok.
  - KK9: işaretsiz tarayıcı sayılır, işaretli ayrı.
  - KK10: sıfırlama, Google ve ömür.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T378: Zaman sayımı: tek saat dilimi, iş günü ve resmî tatil listesi, takvim ve iş günü sayımı, gün sonu, tarih aralığı, çitler
- **Bağımlılık:** —
- **Dokümanlar:** çekirdek; `09` §7; `02 §4.1.1`…§4.1.5
- **Kabul kriterleri:**
  1. Tek saat dilimi Türkiye saatidir; ikinci dilim ve dilim seçimi yoktur, yaz saati geçişi yoktur (`02 §4.1.1`).
  2. İş günü pazartesi–cumadır, resmî tatiller hariç; arife tam iş günüdür, firmanın kapalı günleri hesaba girmez (`02 §4.1.2`).
  3. Resmî tatil listesi ürünle gelir, panelden düzenlenmez; listenin kapsamadığı yılda yalnız hafta sonu dışlanır (`02 §4.1.2`).
  4. İş günüyle yalnız kargoya verme ve havale ödeme süresi sayılır; yasal süreler takvim günüdür (`02 §4.1.3`).
  5. Gün süresi başlangıcın ertesi günü başlar, son günün 23:59'unda dolar; saat bazlı sayım yoktur (`02 §4.1.4`).
  6. Dakika, saat ve son işlemden işleyen süreler gün sayımına girmez; başladıkları andan işler (`02 §4.1.4`).
  7. Tarih aralığı başlangıç gününün 00.00'ında başlar, bitiş gününün 23.59'unda biter (`02 §4.1.4`).
  8. Kargoya verme süresinin üst çiti yasal otuz günden geriye hesaplanmıştır; havale süresinin alt ve üst çiti vardır (`02 §4.1.5`).
  9. Gün ve süre sayımı tek zaman kaynağından ve `TurkeyTime` yardımcısından geçer (`09 §7.1`).
- **Test beklentisi:** Unit
- **Doğrulama kontrol listesi:**
  - KK1: yaz saati ve ikinci dilim yok.
  - KK2: hafta sonu, tatil ve arife.
  - KK3: liste dışı yılda yalnız hafta sonu.
  - KK4: iki süre iş günü, gerisi takvim günü.
  - KK5: 23:59 sınırı sahte saatle.
  - KK6: saatli süre gün sayımında yok.
  - KK7: aralığın iki ucu.
  - KK8: çit dışı değer reddedilir.
  - KK9: sayım `TurkeyTime`'dan geçer.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T379: Yönetici hesapları: ilk yönetici ve davet, kaldırma kuralları, tek rol, ayrı hesap türü, davetin geri çekilmesi, erişim kaybında kurtarma sınırı
- **Bağımlılık:** T303, T308, T309, T342
- **Dokümanlar:** çekirdek; `09` §10, §15; `02 §10.2.1`…§10.2.7, §9.3, F-6, §9.3.4, §9.4, §10.1.2, §10.3.1, §12.2.10; `05` §6.1; `10` KP-62, KD-18
- **Kabul kriterleri:**
  1. İlk yönetici kurulumda doğar; sonrakiler mevcut yöneticinin e-posta davetiyle açılır, kendi kendine yönetici kaydı yoktur (`02 §10.2.1`).
  2. Davet bağlantısı tek kullanımlık ve sürelidir; davetli adını yazar, şifresini kurar ve hesap o anda doğar (`02 §10.2.1`).
  3. Yönetici başkasını kaldırabilir; son yönetici kaldırılamaz, kimse kendini kaldıramaz, panel sebebi söyler ve geri alınamaz onayı ister (`02 §10.2.2`).
  4. Kaldırılan yöneticinin oturumları anında biter, kullanılmamış davetleri geçersizleşir ve onay bunu sayıyla söyler; iz satırları adıyla okunur kalır, F-6 bütün yöneticilere gider (`02 §10.2.2`).
  5. Bütün yöneticiler aynı yetkidedir; rol ve yetki matrisi yoktur (`02 §10.2.3`; KD-18).
  6. Yönetici müşteriyle aynı kimlik doğrulama rejimine ve limitlere tabidir; adını kendi hesabından yeniden doğrulamasız değiştirir (`02 §10.2.4`).
  7. Yönetici hesabının sepeti ve siparişi olmaz; yönetici ve müşteri ayrı hesap türleridir, e-posta kendi türü içinde tekildir (`02 §10.2.5`).
  8. Kullanılmamış davet geri çekilir; aynı adrese yeni davet öncekini geçersiz kılar; yöneticisi olan adrese davet gönderilemez (`02 §10.2.6`).
  9. Erişim kaybında ürün içinde kurtarma akışı yoktur; kurtarma kurulumu yapanın yönetici açma komutudur (`02 §10.2.7`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: kayıt yolu yok, davetle açılış.
  - KK2: bağlantı tek kullanımlık, ad zorunlu.
  - KK3: son yönetici ve kendini kaldırma engellenir.
  - KK4: oturum ve davet düşer, F-6 gider.
  - KK5: rol alanı yok.
  - KK6: limitler ve yeniden doğrulama aynı.
  - KK7: iki hesap türü ayrı kapılardan.
  - KK8: geri çekme ve yeni davet.
  - KK9: ürün içinde kurtarma ucu yok.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T380: İşlem izi: sekiz alan, kim-ne zaman-ne değişti, değiştirilemez ve silinemez, sistem olayları ize girmez, düz liste, on yıl
- **Bağımlılık:** T379
- **Dokümanlar:** çekirdek; `09` §9, §10; `02 §10.3.1`…§10.3.5, §10.2, §10.2.2, §10.2.4, §10.2.6, §10.4, §10.4.1, §12.2, §12.2.9; `05 MK-24`, §4.7, §7.5; `10` KP-63, KP-77, KD-58
- **Kabul kriterleri:**
  1. İz sekiz alanda tutulur: firma kimliği ve marka, yayın durumu, siparişe yönelik yönetici işlemleri, yönetici davet ve hesapları, yasal metin sürümü, panelden değişen ayarlar, dışa aktarma, üye hesabının silinmesi (`02 §10.3.1`).
  2. Her satır kimin, ne zaman, neyi değiştirdiğini ve eski ile yeni değeri taşır (`02 §10.3.1`; `05 §4.7.1`).
  3. Sipariş satırları kişisel veriyi değer olarak taşımaz: adres düzeltmesinde sipariş, alan ve işlem yazılır; iç notun metni ize girmez (`02 §10.3.1`).
  4. İz satırı işlemle aynı veritabanı işleminde yazılır; işlem ya iz satırıyla olur ya hiç olmaz (`05 §4.7.2`).
  5. İz süresi boyunca değiştirilemez ve silinemez; uygulamanın veritabanı kullanıcısı yalnız ekler ve okur, süresi dolanı veritabanı fonksiyonu siler (`02 §10.3.2`; MK-24).
  6. Yanlış işlemin düzeltmesi yeni satır üretir; yanlış işlemin satırı izde kalır (`02 §10.3.2`).
  7. Sistem olayları ize yazılmaz: "e-posta ulaşmadı" işareti, imha kaydı, giriş denemeleri; kurulum tarafının komutları da yazılmaz (`02 §10.3.3`; `05 §4.7.4`).
  8. Panelde düz liste görünür; tarih aralığı ve yönetici süzgeci vardır, arama, gruplama ve dışa aktarma yoktur (`02 §10.3.4`).
  9. İz on yıl saklanır; kaldırılan yöneticinin satırları adıyla durur (`02 §10.3.5`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: sekiz alanın her biri satır üretir.
  - KK2: satır alanları ve eski–yeni değer.
  - KK3: adres ve iç not metni satırda yok.
  - KK4: işlem geri alınınca iz de geri alınır.
  - KK5: güncelleme ve silme yetkisi reddedilir.
  - KK6: düzeltme yeni satır, eskisi durur.
  - KK7: sistem olayı ve komut satır üretmez.
  - KK8: süzgeçler var, arama ve dışa aktarma yok.
  - KK9: on yıl dolmadan silinmez.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T381: Ayar katmanları ve parametre envanteri: firma ayarı, kurulum ayarı, ürün sabiti, varsayılan ve çit, değişikliğin ize yazılması
- **Bağımlılık:** T330, T380
- **Dokümanlar:** çekirdek; `09` §3; `02 §10.1.3`, §10.3.1, §11.1, §11.2, P-1…P-49; `10` KP-64, §4.2
- **Kabul kriterleri:**
  1. Parametre üç katmandadır: firma ayarı panelden değişir, kurulum ayarı kurulumdan gelir ve panelden değişmez, ürün sabiti değişmez (`02 §10.1.3`).
  2. Atama ölçütü: zarar yalnız firmaya dokunuyorsa ve karar işletme kararıysa firma ayarı; güvenliğe, yasal uyuma ya da müşteriye dokunuyorsa ürün sabiti (`02 §10.1.3`).
  3. Kurulum ayarları alan adı, e-posta gönderim kimliği, ödeme sağlayıcı anahtarları ve Google uygulamasının kimlik bilgileridir (`02 §10.1.3`).
  4. On firma ayarı vardır: P-1, P-3…P-10, P-44; panelden değişir (`02 §11.1`; KP-64).
  5. Firma ayarları kurulumda varsayılanla dolu gelir; firma sayı girmeden sistem uçtan uca çalışır, tek istisna ürün formundaki hizmet ifa süresidir (P-44) (`02 §10.1.3`; KP-64).
  6. Çitler panelde uygulanır: P-5 üst çit 10 iş günü, P-7 alt 1 üst 3 iş günü, P-8 ve P-9 en az 1, P-44 en az 1 gün; çit dışı değer reddedilir (`02 §11.1`).
  7. Ürün sabitleri (`02 §11.2`: P-12…P-30, P-31…P-43, P-45…P-49) panelde alan olarak yoktur ve her değer tek yerde tanımlıdır (`02 §11.2`).
  8. P-2 ve P-11 kaldırılmıştır; kimlikleri yeniden kullanılmaz (`02 §11.1`).
  9. Panelden değişen her ayar eski ve yeni değerle işlem izine yazılır (`02 §10.3.1`).
  10. Parametre envanteri `02 §11` tablosuyla eşittir; eşitlik `Consistency` testidir (`09 §21.4` madde 1).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: üç katman ve değişebilirlik.
  - KK2: yeni parametre ölçütle katmana girer.
  - KK3: kurulum ayarı panelde yok.
  - KK4: on ayar panelde, varsayılanlarla.
  - KK5: boş kurulumda uçtan uca çalışır.
  - KK6: çit dışı değer reddedilir.
  - KK7: ürün sabiti için form alanı yok.
  - KK8: P-2 ve P-11 envanterde yok.
  - KK9: ayar değişikliği izde.
  - KK10: envanter eşitlik testi.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T382: Panel ana sayfası verisi: altı bekleyen iş sayacı, kanal ve firma bildirimi uyarıları, kurulum kontrol listesi
- **Bağımlılık:** T314, T331, T365, T370, T373, T374
- **Dokümanlar:** çekirdek; `09` §9, §10; `03 §10.1`; `02 §9.1.6`, §9.3.1, §9.3.2, §10.4.9, §10.6.1, §10.8.2; `05 MK-62`, §10.2; `10` KP-50, KP-65, §4.1
- **Kabul kriterleri:**
  1. Ana sayfada altı sayaç vardır; yedinci sayaç açılmaz (`02 §10.6.1`).
  2. Sayaçlar: ödeme onayı bekleyen (havale), kargoya verilecek, teslim işareti bekleyen, tamamlanmayı bekleyen hizmet, açık talep (iletişim ve ayıp), iade ve geri ödeme bekleyen (`02 §10.6.1`).
  3. "Kargoya verilecek" Hazırlanıyor'daki iptal edilmemiş ve feshe uğramamış fiziksel kalemli siparişleri ve geri dönen Teslim edilemedi siparişlerini sayar (`02 §10.6.1`).
  4. "İade ve geri ödeme bekleyen" teslim alınacak iade malını, işlenecek geri ödemeyi ve sağlayıcıda başarısız ya da sonucu beklenen kart geri ödemesini sayar (`02 §10.6.1`).
  5. "Mal dönmedi" ile kapatılan ve iade reddi alan kalem sayaçtan düşer; mal ulaşıp kalem yeniden açılırsa geri gelir (`02 §10.6.1`; §10.4.9).
  6. Her sayaç kendi listesine gider (`10` KP-50).
  7. Kanal uyarısı art arda beş kanal hatası ve ilk hatadan on beş dakika geçince açılır, ilk başarılı gönderimde kalkar; firma bildirimleri uyarısı en son sonucu "ulaşmadı" olan bildirimden okunur; ikisi okunan hâldir, saklanan işaret değildir (`05 §10.2.1`, §10.2.3, §10.2.4).
  8. Kurulum kontrol listesi satışın açılması için karşılanmamış koşulları adıyla sayar: tipin zorunlu kimlik alanları, açık ödeme yöntemi ve IBAN, yayındaki aydınlatma ve çerez politikası, iade adresi (`02 §10.8.2`).
  9. Sihirbaz ve zorunlu adım sırası yoktur; madde tamamlandıkça işaretlenir, hepsi tamamlanınca liste kaybolur (`02 §10.8.2`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: altı sayaç, yedincisi yok.
  - KK2: altı sayacın adı ve kaynağı.
  - KK3: geri dönen sipariş kargoya verilecekte.
  - KK4: üç tür geri ödeme sayılır.
  - KK5: kapatma ve ret sayaçtan düşürür.
  - KK6: sayaçtan listeye gidilir.
  - KK7: uyarılar okunan hâlden, eşikler doğru.
  - KK8: eksik koşul adıyla görünür.
  - KK9: sihirbaz yok, liste kendiliğinden kalkar.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T383: Satış özeti ve ölçüler: dönem seçimi, ciro, sayılar, dört mağaza ölçüsü, sıfır payda, tepe gün sayısı
- **Bağımlılık:** T350, T365, T367, T372
- **Dokümanlar:** çekirdek; `09` §9, §14; `02 §10.6.2`, §10.6.3, §10.4.9, §11.3, H-2; `10` KP-53
- **Kabul kriterleri:**
  1. Satış özeti analitik değildir; ürünün kendi kayıtlarından türer, ziyaretçi verisi, çerez ve yeni kişisel veri kullanmaz; saklanmaz ve dışa aktarılmaz (`02 §10.6.2`).
  2. Seçilen dönem için sipariş sayısı ve ciro ödemesi tamamlanmış siparişleri sayar; Başarısız ve ödemesi bekleyen girmez (`02 §10.6.2`).
  3. Ciro onaylanan toplamdır; sonraki geri ödemeler düşülmez, iptal ve iade ayrıca sayılır (`02 §10.6.2`).
  4. Sipariş oluştuğu tarihin dönemine yazılır; ödemesi sonradan tamamlanan havale siparişi de (`02 §10.6.2`).
  5. Ödeme tamamlama oranında aynı sepetten art arda verilen siparişler tek deneme sayılır; ödemesi alınmamış sipariş Başarısız kaydı taşır (`02 §10.6.3`).
  6. İletişim talebi sayısı yalnız Genel soru, Ürün hakkında ve Diğer tiplerini sayar (`02 §10.6.3`).
  7. Kargoya verme sözüne uyum oranı süresi dolan ve iptal edilmemiş fiziksel kalemli siparişleri sayar; süre dolunca kargoya verilmemiş sipariş sonradan gönderilse de uyulmamıştır (`02 §10.6.3`).
  8. İptal ve iade oranı iptali, gecikme feshini (bildirim damgasında) ve cayma beyanını (beyan anında) sayar; "mal dönmedi" kapanışı ve iade reddi sayıyı değiştirmez (`02 §10.6.3`).
  9. Paydası sıfır olan oran "değerlendirilemez" sayılır, sıfır yüzde gösterilmez (`02 §10.6.3`).
  10. Tek gün seçilen özet tepe gün tetikleyicisinin okuduğu günlük sayıyı verir; sayı ödemesi tamamlanmış siparişlerdir (`02` H-2).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: ziyaretçi verisi ve saklama yok.
  - KK2: Başarısız ve bekleyen sayılmaz.
  - KK3: ciro geri ödemeden düşmez.
  - KK4: dönem sipariş tarihine göre.
  - KK5: art arda siparişler tek deneme.
  - KK6: iki tip sayıya girmez.
  - KK7: geç gönderilen sipariş uymamış.
  - KK8: fesih ve cayma sayılır, kapanış sayıyı değiştirmez.
  - KK9: sıfır paydada değerlendirilemez.
  - KK10: tek gün sayısı tetikleyiciyle aynı.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T384: Dışa aktarma: sipariş CSV (fatura verisi), üye ve iletişim talebi listesi, işlem izine yazım, imha edilmiş siparişin boş sütunları
- **Bağımlılık:** T341, T357, T365, T380
- **Dokümanlar:** çekirdek; `09` §10; `03 §8.9`; `02 §10.7.1`…§10.7.4, §10.3.1, §12.1.10, §12.2.9, §12.3.2, §12.5; `10` KP-52, KD-1, KD-54
- **Kabul kriterleri:**
  1. Firma tarih aralığı seçip siparişleri CSV indirir; dosya ödemesi tamamlanmış siparişleri taşır, Başarısız ve ödemesi bekleyen girmez (`02 §10.7.1`).
  2. Satır bir sipariş kalemidir; sipariş alanları kalemlerde yinelenir (`02 §10.7.1`).
  3. Dosya fatura verisini taşır: tutar, KDV oranı ve tutarı, kalem dökümü, indirim ve kupon payı, kargo ücreti ve oranlara bölünmüş KDV'si, donmuş fatura adresi ve firma kimliği (`02 §10.7.1`).
  4. Dosya ödeme yöntemini ve onay tarihini, kargoya verme tarihini, kargo şirketini, alan adını, donmuş iletişim e-postasını ve alıcı adını da taşır (`02 §10.7.1`).
  5. İptal edilmiş siparişe geç gelip geri ödenen kart ödemesinde satır ödemenin ve geri ödemenin tarih ve tutarını taşır (`02 §10.7.1`).
  6. Kişisel verisi imha edilmiş siparişin satırı dosyada kalır; ad, e-posta ve adres sütunları boş gelir (`02 §10.7.1`).
  7. Bütün üye hesapları — hiç sipariş vermemişler dahil — ve saklanan bütün iletişim talepleri ad ve e-postasıyla CSV iner; tarih aralığı yoktur, listelerin kendisinden dışa aktarma yoktur (`02 §10.7.4`).
  8. Her dışa aktarma işlem izine yazılır (`02 §10.7.1`, §10.7.4; §10.3.1).
  9. Ürün içe aktarma, toplu güncelleme, katalog, içerik, işlem izi ve satış özeti dışa aktarması yoktur; ürün fatura kesmez (`02 §10.7.2`, §10.7.3; KD-1, KD-54).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: aralık süzgeci, yalnız ödenmiş siparişler.
  - KK2: kalem başına satır.
  - KK3: fatura alanları donmuş değerden.
  - KK4: ödeme, kargo ve iletişim alanları.
  - KK5: geç ödemede iki tarih ve tutar.
  - KK6: imha edilmişte üç sütun boş.
  - KK7: hiç sipariş vermemiş üye listede.
  - KK8: her indirme izde.
  - KK9: ikinci dışa aktarma yolu yok.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T385: Gönderim yeniden denemesi ve "e-posta ulaşmadı" işareti: üç deneme, işaretin düşüşü, panelden yeniden gönderim, işaretsiz bildirimler
- **Bağımlılık:** T304, T306, T307, T382
- **Dokümanlar:** çekirdek; `09` §11; `03 §1.7`, §7.1, §8.9, §10.1; `02 §9.1.6`, §9.3, §9.3.3, §9.3.4, §10.6.1, §11.2, P-38; `05` §4.4, §5.2, §10.2; `10` KP-68
- **Kabul kriterleri:**
  1. Gönderilemeyen e-posta ilk denemeden 5 dakika, 30 dakika ve 2 saat sonra üç kez yeniden denenir (`02 §9.1.6`; P-38).
  2. Üçü de başarısızsa müşteriye giden bildirimde ilgili siparişin panel satırına "e-posta ulaşmadı" işareti düşer; ayıp talebinin B-9'unda işaret talebin satırında da görünür (`02 §9.1.6`).
  3. İşaret işlem izine yazılmaz ve akışı etkilemez (`02 §9.1.6`).
  4. İşaret, alıcı başına gönderim sınırı aşılınca yeniden denenmeden ve servisin sonradan bildirdiği kalıcı geri dönmede de düşer (`02 §9.1.6`).
  5. İşaret yalnız siparişe bağlı müşteri bildirimlerinde düşer; firma bildirimlerinin ulaşmaması ana sayfa uyarısıdır (`05 §4.4.3`, §10.2.4).
  6. Firma işaretli siparişin ayrıntısından e-postayı yeniden gönderir; işareti düşüren her müşteri bildirimi — B-1…B-16 — yeniden gönderilir (`02 §9.1.6`).
  7. Yeniden gönderim siparişin güncel iletişim e-postasına gider; B-15 düzeltmeden önceki adrese (`02 §9.1.6`).
  8. Yeniden gönderim işlem izine yazılır (`02 §10.3.1`).
  9. Yeniden gönderilen e-postanın gönderilmesi olağan kuyruktan ve olağan yeniden denemeyle olur (`05 §4.4.3`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: üç aralık sahte saatle.
  - KK2: işaret üçüncü başarısızlıktan sonra düşer.
  - KK3: izde işaret satırı yok, akış sürer.
  - KK4: sınır ve kalıcı geri dönmede işaret.
  - KK5: firma bildirimi satır işareti düşürmez.
  - KK6: B-1…B-16 yeniden gönderilir.
  - KK7: B-15 eski adrese gider.
  - KK8: yeniden gönderim izde.
  - KK9: yeniden gönderim kuyruktan geçer.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T386: Sipariş müdahaleleri: kalem adedi azaltma ve çıkarma, kargo bilgisinin ve teslim tarihinin düzeltilmesi, iç not
- **Bağımlılık:** T367, T369, T375, T380
- **Dokümanlar:** çekirdek; `09` §9, §10; `02 §10.4.1`, §10.4.3…§10.4.5, §10.4.7, §7.1.7, §7.2.2, §7.2.8, §7.2.9, §7.4.1; `10` KP-8, KP-48
- **Kabul kriterleri:**
  1. Kalem adedi azaltılır ya da kalem çıkarılır; kalem eklenemez ve fiyat değiştirilemez (`02 §10.4.3`).
  2. Ödeme onayından önce kalem çıkarılamaz; yalnız sipariş bütünüyle iptal edilir (`02 §7.2.2`).
  3. Çıkarılan kalemin ya da azaltılan adedin parası `02 §7.1.7` tutarıyla ve `02 §7.2.8` hattından geri ödenir; havale hattında IBAN isteği çıkarma kaydıyla düşer (`02 §10.4.3`).
  4. Stok, hizmet kontenjanı ve kupon hakkı iptaldeki rejimle döner; kargo ücreti `02 §7.2.9` tetiğiyle geri ödenir (`02 §10.4.3`).
  5. Çıkarma sebep listesi istemez, iz bırakır ve müşteriye bildirilir (`02 §10.4.3`).
  6. Kargo şirketi ve takip numarası düzeltilir; düzeltilmiş bilgi müşteriye bildirilir (`02 §10.4.4`).
  7. Teslim tarihi sipariş düzeyinde düzeltilir; kargoya verildiği günden önce ve ileri tarihli olamaz, tek kalemin tarihi ayrıca düzeltilmez (`02 §10.4.5`).
  8. Cayma penceresi ve ayıp süresi düzeltilmiş tarihten işler; önceki cayma beyanı geçerli kalır; tarih bir beyanın günüyle çakışırsa panel teslim sorusunu sorar (`02 §10.4.5`; §7.4.1).
  9. Teslim tarihi düzeltmesi bildirim üretmez; izde eski ve yeni tarihle durur (`02 §10.4.5`).
  10. İç not müşteriye görünmez; sipariş sayfasına, e-postalara ve sözleşmeye girmez; yazılması ve değişmesi izde durur, notun metni ize girmez, imha edilmiş siparişe not yazılamaz (`02 §10.4.7`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: kalem ekleme ve fiyat değişimi yok.
  - KK2: ödeme öncesi çıkarma reddedilir.
  - KK3: tutar kalemin donmuş değerinden, hat doğru.
  - KK4: sayaçlar ve kargo ücreti iptal gibi.
  - KK5: sebepsiz çıkarma, iz ve bildirim.
  - KK6: takip düzeltmesi bildirilir.
  - KK7: tarih sınırları reddedilir.
  - KK8: pencere yeniden hesaplanır, eski beyan sürer.
  - KK9: tarih düzeltmesinde e-posta yok.
  - KK10: iç not hiçbir çıktıda yok.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T387: Kurulum tarafı yönetim komutları: iş satırını yeniden kuyruğa alma ya da kapatma, anahtar yenilemesi sonrası ödeme sonucunun uygulanması, kayıt okuma komutu
- **Bağımlılık:** T303, T311
- **Dokümanlar:** çekirdek; `09` §13 (§13.2); `03 §1.7`; `05 MK-66`, MK-72, §6.9, §8.5, §9.3
- **Kabul kriterleri:**
  1. `job` komutu "Başarısız" iş satırını `--id` ve `--action` (`requeue`, `close`) ile yeniden kuyruğa alır ya da kapatır; durum ucunun iş kuyruğu koşulu ancak böyle kalkar (`05 §8.5.9`).
  2. `read-logs` giriş kaydını, sistem kayıtlarını ve imha kaydını `--source`, `--from`, `--to` ve `--filter` ile süzerek okur (`05 §8.5.5`; `09 §13.2` madde 3).
  3. IP ve hesabın e-postası argümanla verilmez; terminalden okunur ve e-posta hesabın iç kimliğine çevrilir (`09 §13.2` madde 3).
  4. Sonuç CSV'dir; kayıtların ve yedeğin dışındaki geçici dizinde üretilir, firmaya şifreli iletilir ve iletimden sonra silinir (`05 §9.3.3`).
  5. `read-logs` her çalışmasını uygulama kaydına yazar; satır okumadan önce yazılır ve diske inmesi beklenir (`05 §9.3.4`).
  6. Kayıtlar panelde görünmez; yalnız bu komutla okunur (`05 §9.3.1`; MK-66).
  7. `apply-payment-result` yalnız "ödeme sonucu alınamadı" işaretli kart siparişinde çalışır; başka siparişi ve işaretsizi reddeder (`05 §8.5.12`).
  8. Komut sipariş numarasını ve panelde okunan sonucu alır; başarılı ödeme var ve tutar `Order.Total`'a eşitse başarı, ödeme yoksa ya da başarısızsa başarısızlık olarak olağan tek yoldan uygulanır; tutar eşit değilse uygulanmaz ve 3 döner (`05 §8.5.12`; `09 §13.2` madde 2).
  9. Komut sağlayıcıyı çağırmaz, sır istemez, e-posta göndermez ve iş kuyruğunu çalıştırmaz (`05 §8.5.7`, §8.5.12).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: iş satırı iki eylemle, koşul kalkar.
  - KK2: kaynak, aralık ve süzgeç çalışır.
  - KK3: IP ve e-posta argümanı reddedilir.
  - KK4: dosya yedek dizininde değil, iletimden sonra silinir.
  - KK5: kayıt satırı okumadan önce yazılır.
  - KK6: panelde kayıt ekranı yok.
  - KK7: işaretsiz sipariş reddedilir.
  - KK8: eşit olmayan tutarda 3, başarıda B-4 yolu.
  - KK9: sağlayıcı çağrısı ve e-posta yok.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T388: Dönüş denetimi komutu ve dönüşten sonra silmelerin yeniden uygulanması
- **Bağımlılık:** T301, T302, T303, T345
- **Dokümanlar:** çekirdek; `09` §13 (§13.2); `05 MK-72`, MK-74, §8.5, §8.7
- **Kabul kriterleri:**
  1. `rollback-check` salt okumadır ve veritabanı döndürülmeden önce, uygulama dururken çalışır (`05 §8.5.11`).
  2. `--restore-point` ofsetli an olmak zorundadır; ofsetsiz ya da yalnız tarih kullanım hatasıdır ve 2 döner (`09 §13.2` madde 3).
  3. Komut kaydın her tablosunda ve gelen bildirim kaydında dönüş noktasından sonra kesinleşen işlemlerin eklediği ya da güncellediği satırları sayar; ölçüt kesinleşme anıdır (`05 §8.5.11`).
  4. Silme yalnız süreye bağlı silme ve anlık silmedir; ikisi dönüşten sonra yeniden uygulanır; silmenin yazdığı boşaltma ve bağ güncellemeleri güncelleme sayılır (`05 §8.5.11`).
  5. Sayı sıfır değilse dönülmez; dönüş yalnız sipariş tarafında dönüş noktasından sonra kayıt yazılmamışsa yapılır (`05 §8.5.11`, §8.7.4).
  6. `reapply-deletions` dönüşten sonra, uygulama başlamadan önce çalışır; uygulama kaydındaki anlık silme satırlarını dönüş anından bugüne okur ve her birini aynı silme yoluyla yeniden uygular (`05 §8.5.10`).
  7. Kullanıcı işleminin kapıları uygulanmaz; kaydı dönüşte zaten yoksa satır atlanır (`05 §8.5.10`).
  8. Komut bütün oturum kayıtlarını siler ve kullanılmamış hesap bağlantılarının ömrünü dönüş anına çeker; e-posta değişikliğini geri alma bağlantısı hariçtir (`05 §8.5.10`).
  9. Göçler genişlet–daralt kuralıyla yazılır; geri alma önceki imaja dönmektir, şema geri alınmaz (`05 §8.7.1`, §8.7.3; MK-74).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: komut yalnız okur, uygulama durmuşken.
  - KK2: ofsetsiz an 2 döner.
  - KK3: sayım kesinleşme anına göre.
  - KK4: boşaltma güncellemesi sayılır.
  - KK5: sıfırdan büyük sayı dönüşü reddeder.
  - KK6: silmeler dönüş anından sonrası için uygulanır.
  - KK7: kapılar atlanır, eksik kayıt atlanır.
  - KK8: oturumlar silinir, geri alma bağlantısı sürer.
  - KK9: şema geri alınmadan önceki imaj çalışır.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T389: Hesap ve durum yönetim komutları: oturumları sonlandırma, şifreyi geçersiz kılma, durum
- **Bağımlılık:** T303, T314, T342
- **Dokümanlar:** çekirdek; `09` §13 (§13.2, §13.3); `05` §6.9, §8.5
- **Kabul kriterleri:**
  1. `end-sessions` e-postayla gösterilen yönetici hesabının bütün oturum kayıtlarını siler; e-posta terminalden okunur (`05 §8.5.3`).
  2. `invalidate-password` hesabın şifresini kullanılamaz kılar ve aynı veritabanı işleminde bütün oturum kayıtlarını siler; hesap yalnız şifre sıfırlamayla yeniden açılır (`05 §8.5.4`).
  3. İki komut hesabı e-postayla bulur; hesap yoksa kayıt satırına hedef "yok" yazılır ve komut 3 ile biter (`09 §13.2` madde 5).
  4. Sıra: e-posta terminalden, hesabın iç kimliği bulunur, kayıt satırı yazılır, işlem yapılır; e-posta ve ad kayda girmez (`09 §13.2` madde 5).
  5. Kurtarmada ele geçirilmiş hesabın oturumları bu komutlarla sonlandırılır ve şifresi geçersiz kılınır (`05 §6.9.2`).
  6. `status` durum ucunun koşullarını, çalışan sürümü, açık ve kapalı hatları ayrıntısıyla yazar; "başarısız" iş satırları dahildir (`05 §8.5.6`).
  7. `status` sağlayıcı anahtarlarını kaldırmanın şartlarını sayar: açık kart işi sayısı ve geri ödemesi doğabilecek kart siparişi sayısı; ikinci sayı panel sayaçlarına girmez (`05 §8.5.6`).
  8. `status` kayıtlı her büyük veri dönüşümü için kalan satır sayısını yazar (`09 §13.3` madde 5).
  9. Üç komut da işlem izine yazılmaz ve bildirim üretmez (`05 §6.9.3`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: hesabın bütün oturumları silinir.
  - KK2: şifre kullanılamaz, oturumlar aynı işlemde silinir.
  - KK3: bilinmeyen e-postada 3 ve "yok" satırı.
  - KK4: kayıtta e-posta ve ad yok.
  - KK5: kurtarma sırası uçtan uca.
  - KK6: durum çıktısı koşulları ve iş satırlarını içerir.
  - KK7: iki sayı yazılır, panel sayacı değişmez.
  - KK8: dönüşüm için kalan sayı yazılır.
  - KK9: izde satır, bildirim yok.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T390: Uçtan uca dal akış testleri (servis düzeyi): karışık sipariş, kargoda cayma, IBAN'sız iade, e-posta düzeltme
- **Bağımlılık:** T329, T351, T353, T367, T370, T372, T373, T375
- **Dokümanlar:** çekirdek; `09` §9, §19; `03 §2.10.2`…§2.10.5; `02 §7.3.3`, §7.4.1, §7.4.5, §10.4.9, §10.4.11
- **Kabul kriterleri:**
  1. Dört anlatı `FakePaymentGateway` ile servis katmanından ayrı Integration testlerinde baştan sona geçer (`03 §2.10.2`…§2.10.5; K-1441).
  2. Karışık sipariş: ödeme onayında Ö1 ve S1 birlikte işler, dijital kalem o anda teslim edilir ve B-4 gider; dijital kalemin iptal ve cayma yolu kapanır (`03 §2.10.2`).
  3. Lamba kargodan önce iptal edilir: B-7 gider, kart iadesi kendiliğinden başlar, kargo ücreti eklenir, ödeme Kısmen geri ödendi olur (Ö3, B-8) ve kupon hakkı dönmez (`03 §2.10.2`).
  4. Hizmet son açık kalem olarak tamamlanınca sipariş Teslim edildi'ye geçer; teslim tarihi oluşmadığı için cayma hakkı tamamlanma işaretine kadar sürer (`03 §2.10.2`).
  5. Kargoda cayma: beyan IBAN'ı ve iade adresini yazar, kalemi kapatır, B-9 ve F-3 gider; sipariş Kargoya verildi'de kalır ve süre beyandan işler (`03 §2.10.3`).
  6. Dönüşte "teslim edilemedi" B-6 gönderir; kapanışta iptal kaydı, ikinci geri ödeme ve B-7 yoktur; teslim alma alt sınırsızdır ve süreyi başlatmaz (`03 §2.10.3`).
  7. Geri ödeme IBAN'a kargo ücretiyle gider; ödeme Geri ödendi olur (Ö4, B-8) (`03 §2.10.3`).
  8. IBAN'sız iade: mal gelmezse kendiliğinden işlem yoktur; "mal dönmedi" kapatması IBAN'ı siler; ulaşan mal kalemi yeniden açar, B-14 gider ve süre IBAN beklerken durmaz (`03 §2.10.4`).
  9. E-posta düzeltmesi: yeni anahtar üretilir, yeni adrese B-1 ve eski adrese B-15 gider; durumlar, süreler, kalemler ve IBAN değişmez (`03 §2.10.5`).
  10. Her adımda iki eksen beklenen durumdadır; ana akış T329'dadır (`03 §2.10.1`).
- **Test beklentisi:** Integration
- **Doğrulama kontrol listesi:**
  - KK1: dört anlatı dört testte, sahte ödemeyle.
  - KK2: dijital teslim ve iptal yolu kapanışı.
  - KK3: iptalde B-7, kargo ücreti, kupon dönmez.
  - KK4: hizmet kapanışı sipariş durumunu geçirir.
  - KK5: kargoda cayma kalemi kapatır, durum sabit.
  - KK6: dönüşte ikinci iptal ve B-7 yok.
  - KK7: geri ödeme tutarı ve Ö4.
  - KK8: IBAN silme ve yeniden açılış.
  - KK9: düzeltmede değişmeyenler sabit.
  - KK10: her adımda iki eksen doğru.
  - `09 §25` ilgili satırlar; güvenlik taraması

### F4 — Entegrasyonlar

**Yürütme sırası:** T401 → T402 → T403 → T404 → T405 → T406 → T407 → T408 → T409 → T410

#### T401: PayTR: anahtarlar, ödemenin başlatılması, dönüş ve çerçeve
- **Bağımlılık:** —
- **Dokümanlar:** çekirdek; `09` §3 (§3.5), §11, §14 (§14.2), §19; `08 §2.1`, §2.1.1, §2.1.2, §2.1.4; `03 §2.5`; `02 §3.21.1`, §3.21.4, §12.2.8; `05` §3.6, §6.5; `10` KP-15, KD-64
- **Kabul kriterleri:**
  1. `PayTrPaymentGateway` başlatma işini `POST /odeme/api` altındaki başlatma ucuyla yapar: sunucudan sunucuya, form kodlu (`08 §2.1`; `09 §11.1`).
  2. `merchant_id`, `merchant_key`, `merchant_salt` tek ayardır ve sır dosyasındadır; biri eksikse kart yöntemi görünmez ve çağrı yapılmaz (`08 §2.1.2`; `02 §3.21.4`).
  3. `paytr_token` jeton isteğinin on bir girdisinden HMAC-SHA256 ve Base64'le üretilir; `merchant_key` ile `merchant_salt` istekle gitmez (`08 §2.1`).
  4. Tutarlar `PaymentAmounts` işlevlerinden geçer: `payment_amount` kuruş tam sayısı, `user_basket` tek kalemin Base64'üdür (`08 §2.1.4`; `09 §14.2`).
  5. `merchant_oid` sipariş numarasının tiresiz biçimidir; gelen değer aynı kuralla geri eşlenir (`08 §2.1.4`).
  6. Sınıra uymayan e-posta, telefon ve fiziksel kalemsiz sipariş için firma değeri gider; ad 60, adres 400 karakterde kesilir (`08 §2.1.4`).
  7. `user_ip` istek anındaki adrestir; `timeout_limit` ödeme süresinin bitişine kalan dakikadır, yukarı yuvarlanır, en az 1 (`08 §2.1.4`).
  8. Başarılı yanıt çerçeve adresini döndürür, jeton saklanmaz; dönüş adresleri `/odeme/donus/{erişim anahtarı}`'dır; sayfa T510'dadır (`08 §2.1.4`).
  9. `INTEGRATION_RUNBOOKS/PAYTR.md` aynı PR'da açılır; DV-11, DV-14, DV-38, DV-39, DV-41 test modunda gözlenip satıra yazılır (`08 §4.1`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: başlatma sahte işleyiciyle form kodlu gider.
  - KK2: üç ayardan biri eksikse çağrı yok.
  - KK3: özet bilinen örnekle eşleşir.
  - KK4: 34,56 → 3456; kültür etkisiz.
  - KK5: numara tiresiz, geri eşleme tam.
  - KK6: yedek değer ve kesme sınırları.
  - KK7: IP biçimi ve dakika yuvarlaması.
  - KK8: jeton kayıtta ve veritabanında yok.
  - KK9: DV satırlarında tarih ve kanıt yeri.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T402: PayTR: bildirim imzası ve alan eşlemesi
- **Bağımlılık:** T401
- **Dokümanlar:** çekirdek; `09` §11 (§11.1), §14 (§14.2); `08 §2.1.5`, §4.1; `06 §4.35`; `05` MK-39
- **Kabul kriterleri:**
  1. `merchant_oid` + `merchant_salt` + `status` + `total_amount`'ın HMAC-SHA256 Base64'ü `hash` ile sabit zamanlı karşılaştırılır (`08 §2.1.5`).
  2. `hash` yok ya da tutmuyorsa sonuç `SignatureInvalid`'dir; `payment_amount`, `payment_type`, `currency`, `test_mode` imzasızdır, yalnız tutarlılık denetimidir (`08 §2.1.5`).
  3. Bildirim yedi sırayla sınıflanır: imza, tekrar, eşlenmeyen sipariş, tanınmayan değer, `failed`, tutmayan `success`, tutan `success` (`08 §2.1.5`).
  4. Tutar koşulu `payment_amount` = `Order.Total` × 100 ve `total_amount` ≥ aynı değerdir, kuruşta; `failed`'de tutar aranmaz (`08 §2.1.5`; `09 §14.2`).
  5. `merchant_oid` iki alana tiresiz yazılır; `status` → `Outcome`, `total_amount` → `Amount`; olay türü `Sale`'dir (`08 §2.1.5`; `06 §4.35`).
  6. `failed_reason_code` ve ileti saklanmaz, seyreltilerek uygulama kaydına yazılır (`08 §2.1.5`).
  7. Yanıt kararı (`OK`, 400, 500) T311'in ucundadır; bu katman yalnız sınıfı verir (`08 §2.1.5`; `05` MK-39).
  8. Test modunda gerçek bildirimle DV-1, DV-2, DV-4, DV-9, DV-35, DV-36, DV-42 gözlenir; kayıt referans kurulumun işaretli kaydından okunur (`08 §2.1.10`, §4.1).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: örnek bildirimde özet tutar.
  - KK2: bozuk ve eksik `hash` ayrı testte.
  - KK3: yedi sıranın her biri bir test.
  - KK4: taksit farkı geçer, eksik tutar geçmez.
  - KK5: üç alan eşlemesi.
  - KK6: red kodu kayda seyreltilmiş yazılır.
  - KK7: katman 400 ya da 500 üretmez.
  - KK8: DV-1, 2, 4, 9, 35, 36, 42 satırları.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T403: PayTR: son sorgu ve tek tahsilat
- **Bağımlılık:** T402
- **Dokümanlar:** çekirdek; `09` §11, §14 (§14.2); `08 §2.1.6`, §2.1.7; `03 §2.5`, §3.2, §10.1; `05` MK-39, §4.5
- **Kabul kriterleri:**
  1. Son sorgu `POST /odeme/durum-sorgu`, form kodlu; `paytr_token` üç girdilidir; süre 30 sn'dir (`08 §2.1.6`).
  2. Eşleşen başarılı ödeme `Paid`'dir; `004` "ödeme yok" sayılır ve `NoPayment` döner (`08 §2.1.6`).
  3. `001`, `002`, `003`, zaman aşımı, bağlantı hatası ve tabloda olmayan yanıt `NoResponse`'dur; sipariş iptal edilmez, olay kayda yazılır (`08 §2.1.6`).
  4. Beş dakikalık aralık yalnız kesinti ve tablosuz yanıt içindir; `001`–`003` kod hatası kuralıyla yinelenir (`08 §2.1.6`; `05 §4.3.4`).
  5. Başarı için `payment_amount` eşit, `payment_total` ≥, `odeme_tipi` `KART`, para birimi `TL` ya da `TRY` olmalı; canlıda `test_mode` 1 ise uygulanmaz (`08 §2.1.6`).
  6. Tutar metni tek ayraçlı ve en çok iki ondalıklıdır; başkası tanınmaz ve `NoResponse` olur (`08 §2.1.6`; `09 §14.2`).
  7. Kart ve kişisel alanlar okunmaz, saklanmaz (`08 §2.1.6`).
  8. Aynı `merchant_oid`'li ikinci başarılı bildirim tekrardır ve yazılmaz; ikinci tahsilatı ürün görmez, kalan risk DV-11'dedir (`08 §2.1.7`).
  9. DV-12'nin tutar ayracı ve para birimi yazımı gözlenip `08 §2.1.6`'ya eklenir (`08 §4.1`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: istek biçimi ve süre.
  - KK2: `004` ayrı sonuç.
  - KK3: altı hata girdisi `NoResponse`, sipariş sabit.
  - KK4: iki aralık ayrı sınıfta.
  - KK5: beş koşuldan biri bozulunca uygulanmaz.
  - KK6: `10,8`, `9.76`, `1.234,56` tablosu.
  - KK7: okunmayan alan sonuçta yok.
  - KK8: ikinci bildirimde satır yok.
  - KK9: DV-12 satırı.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T404: PayTR: bağlantı hatası ve ayrım
- **Bağımlılık:** T403
- **Dokümanlar:** çekirdek; `09` §11 (§11.1, §11.2); `08 §2.1.8`, §4.1; `03 §3.2`, §10.1; `05` MK-40, §3.6, §4.6
- **Kabul kriterleri:**
  1. Bağlanılamama, 15 sn aşımı, 5xx, `failed` + `reason` ve tablosuz yanıt `ConnectionFailed`'dır; kayda yalnız `reason` yazılır (`08 §2.1.8`).
  2. Aynı `merchant_oid`'nin ikinci jeton reddi `SecondTokenRejected`'dır; ayırt eden `reason` metni tabloya ve runbook'a yazılır (`08 §2.1.8`; DV-11).
  3. Form açılamadan iptal L-7'ye sayılmaz; açılıp başarısız olan ödeme sayılır (`08 §2.1.8`; `02 §8.2`).
  4. Bağlantı hatasından sonra ödeme modülü bellekte iki dakika tutulur: kart "şu an kullanılamıyor" görünür, kart onayı 422 alır; hâl yalnız onaydan öncesini etkiler (`08 §2.1.8`).
  5. Son sorgu ve iade sorgusunda yanıtsız çağrı sonucu bilinmeyen sayılır; para gönderen çağrı yeniden gönderilmez (`08 §2.1.8`; `05` MK-40).
  6. Hiçbir çağrı yeniden denenmez; dayanıklılık işleyicisi yoktur, istemci fabrikadan gelir, yönlendirme ve çerez kapalıdır (`08 §2.1.8`; `09 §11.2`).
  7. Beş hatalı jeton ve on beş dakika durum ucunun uyarı koşuludur; başarılı jeton diziyi sıfırlar, ikinci ret saymaz (`08 §2.1.8`; `05 §9.4`).
  8. DV-12'nin HTTP kodları ve tablosuz yanıtlar gözlenip `08 §2.1.8` tablosuna eklenir (`08 §4.1`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: altı yanıt sınıfı birer test.
  - KK2: ikinci ret bağlantı hatası değil.
  - KK3: iki iptal biçimi L-7'de ayrı.
  - KK4: iki dakika sahte saatle; hâl yeniden başlatmada silinir.
  - KK5: yanıtsız çağrıda ikinci istek yok.
  - KK6: çağrı sayısı bir.
  - KK7: dizinin sıfırlanması ve eşiği.
  - KK8: DV-12 satırı.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T405: PayTR: kart iadesi
- **Bağımlılık:** T404
- **Dokümanlar:** çekirdek; `09` §11, §14 (§14.2); `08 §2.1.9`, §2.1.2, §4.1; `03 §2.7`, §2.8, §3.3, §8.3; `05` MK-40, §4.6
- **Kabul kriterleri:**
  1. İade `POST /odeme/iade` ile istenir; `paytr_token` dört girdilidir, `reference_no` özete girmez, süre 30 sn'dir (`08 §2.1.9`).
  2. `return_amount` iki ondalıklı, noktalı, kültürden bağımsız metindir; özete giren dizge gönderilendir (`08 §2.1.9`; `09 §14.2`).
  3. `reference_no` iade kaydının tiresiz kimliğidir; eşleştirme içindir, tekrar koruması değildir (`08 §2.1.9`).
  4. İstek, kayıt "istek gönderiliyor" kesinleştikten sonra gider ve yeniden gönderilmez; geri ödeme başına bir açık ve bir başarılı istek vardır (`08 §2.1.9`; `05 §4.6`).
  5. `success` → `Accepted`; `failed` ya da `error` → `NotCompleted`; yanıtsızlık → `Unknown` (`08 §2.1.9`).
  6. Sorguda `returns` içinde kimliği ve tutarı eşit öğe `Completed`'dır; `004` ve öğe yokluğu dahil başkası `NoResult`'tır (`08 §2.1.9`).
  7. Sorgu 1, 5, 15, 60 dakika, sonra saatlik yinelenir; üst sınırı yoktur (`08 §2.1.9`).
  8. İki sonuç yolu aynı işlevle uygulanır; firma işaretinden sonra sorgu durur ve geç iade işlenmez (`08 §2.1.9`; `05 §4.6.2`).
  9. Anahtarlar kaldırılmışsa istek yapılmaz, geri ödeme "gerçekleşmedi" olur (`08 §2.1.2`).
  10. DV-6, DV-7, DV-10 gözlenir; DV-10 sonucu `02`'ye geri beslemedir (`08 §4.1`, §5.2).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: istek biçimi ve özet.
  - KK2: 10 → `10.00`, `tr-TR`'de de.
  - KK3: `reference_no` tiresiz kimlik.
  - KK4: kayıt istekten önce.
  - KK5: üç yanıt sınıfı.
  - KK6: dört sorgu durumu.
  - KK7: beş aralık.
  - KK8: ikinci uygulama sonucu değiştirmez.
  - KK9: anahtarsız istek yok.
  - KK10: DV-6, 7, 10 satırları.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T406: PayTR: 3D Secure kurulum şartı, test ve canlı kanıt
- **Bağımlılık:** T402, T405
- **Dokümanlar:** çekirdek; `09` §11, §19; `08 §2.1.3`, §2.1.10, §4.1; `02 §3.21.3`; `05` §8.1
- **Kabul kriterleri:**
  1. Her kart ödemesi 3D Secure'dan geçer; doğrulama başarısızsa ödeme gerçekleşmez, sipariş ödenmiş sayılmaz (`02 §3.21.3`; `08 §2.1.3`).
  2. Başarı `status` = `success` ve `payment_type` = `card` ister; `failed_reason_code` 1, 2, 10 ödeme gerçekleşmedi demektir (`08 §2.1.3`).
  3. Kurulum şartı: PayTR'den yazılı teyit ve test modunda 3D'siz işlemin tamamlanmaması; teyit yoksa kart anahtarları tanımlanmaz (`08 §2.1.3`; DV-3).
  4. Test `test_mode` alanıyla seçilir; test mağazanın canlı anahtarıyla imzalanır; CI PayTR'ye bağlanmaz, firma anahtarı geliştirmeye geçmez (`08 §2.1.10`).
  5. Canlı kurulumda `test_mode` 1 olan bildirim işaretlenir ve uygulanmaz; sorgu sonucu da uygulanmaz (`08 §2.1.10`).
  6. Sandbox kanıtı: başlatma, 3D, bildirim, sorgu ve iade referans kurulumda test modunda uçtan uca geçer; sonuç runbook'a yazılır (`08 §2.1.10`; K-1411).
  7. Canlı kanıtın satırları `DEPLOY_RUNBOOK`'ta yazılıdır; taksit varsa taksitli yapılır; yürütme F6'dadır (`08 §2.1.10`).
  8. DV-3 satırı kanıtla kapanır; test kanıtı canlı kanıtın yerini tutmaz (`08 §2.1.10`, §4.1).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: 3D'siz başarı sayılmaz.
  - KK2: `payment_type` farklıysa uygulanmaz.
  - KK3: teyitsiz kurulumda anahtar yok.
  - KK4: testte ağ çağrısı yok.
  - KK5: canlıda test bildirimi işaretli.
  - KK6: sandbox tablosu doludur.
  - KK7: runbook canlı kanıt satırları.
  - KK8: DV-3 tarihli.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T407: SES: gönderim ve sonuç sınıfları
- **Bağımlılık:** —
- **Dokümanlar:** çekirdek; `09` §3 (§3.2, §3.3), §11 (§11.1, §11.2), §19; `08 §2.2`, §2.2.1, §2.2.2; `03 §10.1`; `02 §9.1`, §9.1.5, §9.1.6; `10` KP-66
- **Kabul kriterleri:**
  1. `SesEmailGateway` AWS istemcisiyle SendEmail yapar: bir çağrı bir alıcı; Cc, Bcc, yapılandırma kümesi, ek ve başlık gönderilmez (`08 §2.2.2`).
  2. İstemci tekildir; yeniden deneme kapalı, süre 30 sn, kimlik açıktır, ortam kimlik zinciri kullanılmaz (`09 §11.2`).
  3. Erişim anahtarı, gizli anahtar, bölge ve gönderen adresi zorunlu ayardır; eksikse uygulama açılmaz (`08 §2.2`; `02 §9.1.5`).
  4. `FromEmailAddress` kaçışlı marka adıyla, `ReplyToAddresses` firma e-postasıyla gider; HTML ve düz metin birlikte, UTF-8 (`08 §2.2.2`).
  5. Her gönderimden önce alıcı sorulur: 200 ve `BOUNCE` → `RecipientError`, çağrı yok; 404 → gönderilir; başka yanıt gönderimi durdurmaz (`08 §2.2.2`).
  6. Hata eşlemesi saf işlevdir: 200 `Sent`; doğrulanmamış kimlik, `MessageRejected`, kota, `NotFoundException`, kimlik reddi, 5xx, zaman aşımı ve tablosuz yanıt `ChannelError` (`08 §2.2.2`).
  7. `MessageId` saklanmaz; kayda yalnız hata kodu ve sınıf yazılır (`08 §2.2.2`; `09 §11.1`).
  8. IAM kullanıcısının üç izni vardır; gönderme gönderen alan adı kimliğine ve `ses:FromAddress`'e bağlıdır, silme izni yoktur (`08 §2.2`).
  9. `AMAZON_SES.md` aynı PR'da açılır; DV-16, DV-17, DV-19, DV-21, DV-23 gözlenip satıra yazılır (`08 §4.1`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: tek alıcı, yasak alanlar yok.
  - KK2: yeniden deneme sıfır, çağrı bir.
  - KK3: ayar eksikse açılış durur.
  - KK4: ad kaçışı ve yanıt adresi.
  - KK5: ön sorgunun üç kolu.
  - KK6: tablonun her satırı bir test.
  - KK7: kayıtta adres ve anahtar yok.
  - KK8: politika üç ifadeli.
  - KK9: DV satırlarında tarih.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T408: SES: geri dönen adreslerin öğrenilmesi
- **Bağımlılık:** T407
- **Dokümanlar:** çekirdek; `09` §11, §13 (§13.1); `08 §2.2.3`, §4.1; `05` §4.3.4, §5.2
- **Kabul kriterleri:**
  1. Yöntem sorgudur: iş türü her on beş dakikada hesap listesinden son 24 saatin `BOUNCE` adreslerini okur; web kancası ve SNS yoktur (`08 §2.2.3`).
  2. İstek `StartDate`, `Reason` = `BOUNCE` ve sayfalama taşır; `COMPLAINT` işlenmez (`08 §2.2.3`).
  3. Eşleşme harf ayrımsızdır; pencerede `Sent` olan bütün satırlar `Undelivered` ve `RecipientError` olur, `SentAt` kalır (`08 §2.2.3`).
  4. Kanal hâli kaydı etkilenmez; `Undelivered` satıra ikinci uygulama bir şey değiştirmez (`08 §2.2.3`).
  5. Başarısız tur boş geçer, kanal hâline yazılmaz, imleç saklanmaz; hata kayda yazılır ve satır başarısız olmaz (`08 §2.2.3`; `05 §4.3.4`).
  6. İş türünün tek bekleyen satırı vardır; ilgili kayıt kimliği boştur (`05 §5.2`; `08 §2.2.3`).
  7. Ürün listeden adres silmez; silme çağrısı kodda yoktur (`08 §2.2.3`).
  8. DV-22 üretim erişimli hesapta konsoldan eklenen adresle gözlenir ve satıra yazılır (`08 §4.1`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: on beş dakikalık yineleme sahte saatle.
  - KK2: şikâyet adresi işlenmez.
  - KK3: büyük harfli adres eşleşir.
  - KK4: kanal hâli değişmez, ikinci uygulama etkisiz.
  - KK5: 429 turu boş geçer.
  - KK6: ikinci bekleyen satır yok.
  - KK7: kodda silme çağrısı yok.
  - KK8: DV-22 satırı.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T409: Google: giriş, kayıt ve yeniden doğrulama bağlayıcısı
- **Bağımlılık:** T407
- **Dokümanlar:** çekirdek; `09` §3 (§3.2, §3.3), §11 (§11.1–§11.3), §15; `08 §2.3`, §4.1; `03 §9.1`, §10.1; `02 §3.13.6`, §3.13.7, P-13; `05` MK-46, §6.1; `10` KP-26, KD-11
- **Kabul kriterleri:**
  1. `GoogleIdentityGateway` yetki kodu akışını uygular: kod değişimi `POST` ile gider, `userinfo` çağrılmaz (`08 §2.3`; `09 §11.3`).
  2. İstemci kimliği ve sırrı kurulum ayarıdır; biri eksikse çağrı yapılmaz (`08 §2.3`).
  3. Yetkilendirme ve kod değişimi adresleri sabittir; istek `openid email profile`, `state`, `nonce`, S256 taşır, `prompt` ve `max_age` taşımaz (`08 §2.3`).
  4. Belirteçte yalnız `RS256`, imza, `iss`, `aud`, `exp` doğrulanır; `email` yoksa tutmaz; `nonce` ve `auth_time` sonuca alan olarak girer (`08 §2.3`; `09 §11.3`).
  5. Keşif belgesi ve anahtar kümesi önbelleğe alınır; bilinmeyen `kid` yenileme getirir, yine yoksa imza tutmaz (`08 §2.3`).
  6. Sonuç yalnız e-posta, doğrulama bayrağı, ad, `hd` ve `auth_time` taşır; `sub` alınmaz, kayda yazılmaz (`08 §2.3`; K-1061).
  7. Vazgeçme ve doğrulama hatası `Failed`'dır; bağlanamama, 15 sn aşımı ve Google hata yanıtı `Unavailable`'dır; yeniden deneme yoktur (`08 §2.3`).
  8. `GOOGLE.md` aynı PR'da açılır; DV-25, DV-26, DV-27, DV-28, DV-31, DV-37, DV-43 gerçek uygulamayla gözlenir (`08 §4.1`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: kod değişimi gövdesi, `userinfo` çağrısı yok.
  - KK2: ayarsız çağrı yok.
  - KK3: adresler sabit, istekte yasak alan yok.
  - KK4: yedi belirteç durumu.
  - KK5: bilinmeyen `kid` bir kez yenilenir.
  - KK6: `sub` sonuçta ve kayıtta yok.
  - KK7: iki sonuç sınıfı ve çağrı sayısı bir.
  - KK8: DV satırlarında tarih.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T410: Dış olgu varsayımlarının doğrulama kanıtları (DV)
- **Bağımlılık:** T404, T406, T408, T409
- **Dokümanlar:** çekirdek; `08 §4.1`, §5.2; `10` ÖK-4, ÖK-12
- **Kabul kriterleri:**
  1. Her DV satırına kanıtı alan task tarihi ve kanıt yerini yazmıştır; T401–T409 payları kapalıdır (`08 §4.1`).
  2. Kurulumu yapanın payı olan satırlar (DV-7, DV-8, DV-13, DV-15, DV-18, DV-20, DV-24, DV-29, DV-30, DV-40) referans kurulumun yazılı yanıtı ya da okumasıyla kapatılır (`08 §4.1`).
  3. DV-1, DV-2, DV-9 referans kurulumun işaretli kaydından okunmuştur; PayTR imza doğrulaması tutan, bozuk ve imzasız bildirimle gösterilmiştir (`08 §2.1.5`; K-1411).
  4. Üç servisin sandbox kanıtı tek tabloda yazılıdır: PayTR test modu, SES simülatörü ve doğrulanmış alıcı, Google gerçek uygulama (`08 §2.1.10`, §2.2, §2.3).
  5. Beklenen sonucu vermeyen kanıt satırın "Yanlış çıkarsa" hükmünü işletir; yöntem karar kaydıyla kurulur, `08` aynı değişiklikte güncellenir (`08 §4.1`).
  6. Doğrulanamayan kart yeteneği varsa kart anahtarları tanımlanmaz ve satış havaleyle sürer (`08 §4.1`; ÖK-4).
  7. DV-10 sonucu `02`'ye geri beslemedir; §5.2'nin bekleyen satırları sonuçlarla eşitlenir (`08 §5.2`).
  8. DV-5 gözlemi çerçeveli sayfa gelince T510'da alınır; satır "Bekliyor — T510" yazar ve F4 kapısına girmez (K-1450).
  9. Runbook ve PR'da firma anahtarı, sır, belirteç ve bildirim gövdesi yoktur (`08 §2.1.2`).
- **Test beklentisi:** Yok — kod üretmez; kanıt referans kurulumda alınır ve runbook'a yazılır
- **Doğrulama kontrol listesi:**
  - KK1: tabloda bu fazın payı olan "Bekliyor" yok.
  - KK2: kurulumu yapan satırları tarihli.
  - KK3: DV-1, 2, 9 ve üç imza durumu kayıtta.
  - KK4: sandbox tablosunda üç servis.
  - KK5: tutmayan kanıtın karar kaydı.
  - KK6: doğrulanamayan yetenekte anahtar yok.
  - KK7: DV-10 geri beslemesi.
  - KK8: DV-5 satırı T510'a bağlı.
  - KK9: sır taraması temiz.
  - `09 §25` ilgili satırlar; güvenlik taraması

### F5 — Kullanıcı arayüzü

#### T501: Yerli üretim logosu varlığı ve gösterimi
- **Bağımlılık:** T540
- **Dokümanlar:** çekirdek; `09` §3.1, §3.2, §16; `08 §4.3`; `02 §3.8.5`, §12.1, §12.1.5; `04` 5.4.4; `10` KP-5
- **Kabul kriterleri:**
  1. Logo dosyası kodla birlikte gelir; firma panelden değiştiremez ve kapatamaz (`08 §4.3` madde 1; K-610).
  2. Logo yalnız üretim yeri Türkiye olan fiziksel ürünün sayfasında, fiyatın yanında görünür; dijital üründe ve hizmette görünmez (`02 §3.8.5`).
  3. Üretim yeri başka ülke ya da boşsa logo yoktur; logolu etikette ülke adı yine yazılır (`02 §3.8.5`).
  4. Logo hiçbir genişlik sınıfında gizlenmez (`04` 5.4.18).
  5. Logoyu gösteren bileşen tektir; ürün sayfasının fiyat bloğu (T503) onu çağırır (`04` 5.4.4).
  6. Güncel dosya Bakanlığın duyurusundan alınır, kullanım koşulu okunur ve DV-34 satırına tarihle yazılır; duyuru satıra kaynak olur (`08 §4.3` madde 2–3, §4.1).
  7. Malın yerli sayılması firmanın beyanıdır; ürün bunu denetlemez (`08 §4.3` madde 4).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: dosya imajda var, panelde ayarı yok.
  - KK2: dört ürün türü ve ülke birleşimi.
  - KK3: başka ülke ve boş üretim yeri.
  - KK4: dar ve geniş sınıfta görünür.
  - KK5: bileşen tek, ürün sayfası çağırır.
  - KK6: DV-34 satırında tarih ve kaynak.
  - KK7: logo kararı üretim yerinden başka alana bakmaz.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T502: Vitrin: ana sayfa ve kategori sayfası (E-01, E-02)
- **Bağımlılık:** T537, T540, T545
- **Dokümanlar:** çekirdek; `09` §3.4, §7, §16 (§16.2), §20; `07 UC-01`, `UC-02`; `04` E-01, E-02; `03 §2.1`; `02 §3.4.3`, §3.5.3, §3.28.1…§3.28.3; `10` KP-1
- **Kabul kriterleri:**
  1. `GET /` dört blok gösterir — kurumsal blok, Hizmetlerimiz, Referanslarımız, ürün vitrini — firmanın seçtiği düzende (`07 UC-01`).
  2. Hizmet ve referans blokları "ana sayfada göster" işaretlilerin elle sıradaki ilk üçüdür; vitrin yayındaki en yeni ürünlerdir, düzene göre en çok sekiz ya da dört (`07 UC-01`; K-754, K-755).
  3. Yayında ürünü olan birinci seviye kategorilerin bağlantıları görünür; görünmez bloklar ve boş sitenin geri düşüşü `04` 5.1.11'dedir (`07 UC-01`; K-805).
  4. Satış kapalıyken vitrin aynıdır; firmanın yapılandırılmış verisi ilk yanıttadır (`07 UC-01`; `05 §3.7.2`).
  5. `GET /kategori/{Slug}` kategorinin ve alt dallarının yayındaki ürünlerini tek listede, en yeni önce verir (`07 UC-02`; `02 §3.4.3`).
  6. `page`, `minPrice`, `maxPrice`, `inStock` parametreleri çalışır; son sayfayı aşan `page` son sayfayı gösterir; `page` dışı parametreli adres noindex'tir (`07 UC-02`; `07 §2.6.2`, §2.6.7).
  7. Alt fiyat üst fiyattan büyükse süzgeç uygulanmaz, alan mesajı çıkar ve durum 200'dür (`07 UC-02`; `04` 7.1.1.1).
  8. Yayında ürünü olmayan kategori boş sayfadır, 404 değildir; süzgeç sonucu boşsa süzgeci kaldırma yolu görünür (`07 UC-02`; `02 §3.28.6`).
  9. `Slug` bir kategoriye ait değilse 404 döner (`07 UC-06`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: dört blok iki düzende.
  - KK2: işaretli üç kayıt, sekiz ve dört ürün.
  - KK3: ürünsüz kategori bağlantısı yok.
  - KK4: satış kapalı vitrin; yapılandırılmış veri.
  - KK5: alt dal ürünü listede, sıra en yeni önce.
  - KK6: son sayfa aşımı ve noindex.
  - KK7: ters fiyat aralığı 200.
  - KK8: boş kategori ve boş süzgeç.
  - KK9: yabancı Slug 404.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T503: Vitrin: arama sonuçları, ürün sayfası ve sepete ekleme (E-03, E-04)
- **Bağımlılık:** T501, T537, T540, T541, T545
- **Dokümanlar:** çekirdek; `09` §3.4, §7, §8, §15 (§15.2), §16 (§16.2); `07 UC-03`…`UC-05`; `04` E-03, E-04; `03 §2.1`, §8.1; `02 §3.5.1`, §3.6.5, §3.16.5…§3.16.9; `10` KP-2…KP-7
- **Kabul kriterleri:**
  1. `GET /arama` ürün adında büyük-küçük harfe ve Türkçe karaktere duyarsız arar; sözcük 150 karakterde kırpılır, `%` ve `_` harfi harfine eşlenir (`07 UC-03`; `05 §7.9`).
  2. Boş `search` sonuç üretmez ve arama alanını gösterir; sonuçsuz arama boş hâl satırıdır; arşivlenmiş ürün sonuçta yoktur; sayfa noindex'tir (`07 UC-03`).
  3. `GET /urun/{Slug}` yayında E-04'ü, arşivde E-05'i (T549), panel oturumuyla taslakta önizlemeyi verir; taslak ve yok 404'tür, sebep ayırt edilmez (`07 UC-04`).
  4. Seçim `firstOptionValue` ve `secondOptionValue`'dur; eksik ya da tutmayan değer varsayılan varyantla üretilir, hata vermez (`07 UC-04`; K-1176 (1), K-1178).
  5. Fiyat, birim fiyat, indirim ve görsel seçilen varyantındır; satın alınabilirlik ayrılmış adetler düşülerek okunur; logo T501'in bileşenidir (`07 UC-04`; `04` 5.4.4, 5.4.6).
  6. Betikli yol için sayfa yayındaki varyantların verisini taşır; stok adedi bu veride yoktur (`07 UC-04`; `02 §3.6.5`).
  7. `POST ?handler=AddCartItem` 303 ile ürün sayfasına döner: seçilen varyantın iki parametresi ve "sepete eklendi" bildirimi, sepet sayısı yeni yanıtta (`07 UC-05`).
  8. Stok, "bir siparişte en fazla" sınırı (sınır önce), sepetteki aynı dijital varyant, tükenmiş varyant, kapalı satış ve geçersiz adet 422 verir; yayında olmayan `VariantId` 404'tür (`07 UC-05`; `03 §3.2.1.6`…§3.2.1.8).
  9. Tekrar gönderim ikinci eklemedir; çift tıklamayı betikli yolda düğmenin kilidi önler (`07 UC-05`; K-1179 (1)).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: Türkçe karakter ve `%` aramaları.
  - KK2: boş arama, sonuçsuz arama, arşiv ürünü.
  - KK3: üç yayın durumu ve 404.
  - KK4: bozuk parametre varsayılana düşer.
  - KK5: varyanta göre fiyat; ayrılmış adet düşülür.
  - KK6: sayfa verisinde stok adedi yok.
  - KK7: 303 adresi ve sepet sayısı.
  - KK8: sekiz ret sebebi ayrı testte.
  - KK9: ikinci gönderim adedi artırır.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T504: Hata sayfaları: bulunamayan adres ve beklenmeyen hata (E-06, 500)
- **Bağımlılık:** T537, T540, T541
- **Dokümanlar:** çekirdek; `09` §8, §15 (§15.2), §18; `07 UC-06`, `UC-78`; `04` E-06; `02 §3.30.4`; `05 §10.3`; `10` KP-7
- **Kabul kriterleri:**
  1. 404 yanıtı vitrinin tam çerçevesinde E-06'dır: bulunamadığını söyleyen başlık, ana sayfaya ve ürünlere dönüş (`07 UC-06`; `04` 5.6.3).
  2. 404 şu hâllerde döner: bilinmeyen ilk yol bölümü ya da kayda ait olmayan `Slug`, oturumsuza taslak ve silinmiş kayıt, sahiplik denetimini geçmeyen istek, geçersiz erişim anahtarı, imha edilmiş sipariş bağlantısı, ömrü dolmuş indirme değeri, yayına alınmamış aydınlatma metni ve çerez politikası (`07 UC-06`).
  3. Yanıt sebebi ayırt etmez; boş kategori bu yanıt değildir (`07 UC-06`; `07 §2.3.3`).
  4. Beklenmeyen hata GET'te vitrinde vitrinin, panelde panelin çerçevesinde 500 hata sayfasıdır (`07 UC-78`; `04` 2.7.3.2).
  5. POST'ta 500, formu gönderilen değerlerle ve işlem düzeyindeki kalıcı mesajla yeniden gösterir (`07 UC-78`; `04` 2.7.3.1).
  6. Sonucu belirsiz işlemde hata nerede olursa olsun 303 sonucu gösteren ekrana gider (`07 UC-78`; `04` 2.7.3.3; `05 §10.3.4`).
  7. Yanıt hatanın ayrıntısını ve kodunu taşımaz; hata uygulama kaydına yazılır ve durum ucunun sayacına girer (`07 UC-78`; `05 §9.4.8`).
  8. Makine ucunda 500 gövdesizdir (`07 UC-78`; `07 §2.7.2`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: 404 çerçevesi ve iki bağlantı.
  - KK2: sekiz düşme hâli ayrı testte.
  - KK3: iki sebebin yanıtı aynı; boş kategori 200.
  - KK4: vitrin ve panel 500 çerçevesi.
  - KK5: POST 500'de değerler korunur.
  - KK6: belirsiz işlem 303 hedefi.
  - KK7: gövdede istisna metni yok; kayıtta var.
  - KK8: makine ucu gövdesiz.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T505: İçerik liste ve içerik sayfaları (E-07, E-08)
- **Bağımlılık:** T537, T540, T545
- **Dokümanlar:** çekirdek; `09` §3.4, §16 (§16.2); `07 UC-07`…`UC-12`; `04` E-07, E-08; `03 §2.2`; `02 §3.27.4`, §3.27.5, §3.27.10, §3.27.25; `10` KP-32
- **Kabul kriterleri:**
  1. `GET /hizmetler` ve `GET /referanslar` tipin yayındaki kayıtlarını firmanın elle sırasıyla listeler; `page` 24 kayıttan sonra; tipin adı firmanın verdiği addır (`07 UC-07`, `UC-08`; K-851).
  2. Taslak kayıt listede yoktur; yayında kaydı olmayan tipin adresi boş hâl satırıdır ve menü öğesi görünmez (`07 UC-07`; `04` 5.7.8).
  3. Hakkımızda tek kayıttır ve sabit adrestedir; hizmet, referans ve genel sayfa `{Slug}`'la açılır (`07 UC-09`…`UC-12`).
  4. Sayfa ad ya da başlığı, görselleri, metni ve türe özgü bölgeyi gösterir (`07 UC-09`; `04` 5.8.3, 5.8.4).
  5. "İlgili ürünler" bağlı ürünlerin yalnız yayındakilerini gösterir (`07 UC-10`, `UC-11`; `04` 5.8.5).
  6. "Bize ulaşın" E-10'un formuna giden bağlantıdır; video bağlantısı ve "Paylaş" uç doğurmaz (`07 UC-09`…`UC-12`).
  7. Taslak kayıt panel oturumuyla "Taslak" bandıyla açılır ve noindex'tir; oturumsuza 404'tür (`07 UC-09`; `04` 5.8.9; `02 §3.30.6`).
  8. `Slug` kayda ait değilse 404 döner (`07 UC-06`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: elle sıra, 24'lük sayfa, ad değişikliği.
  - KK2: taslak listede yok; boş tip.
  - KK3: dört adres ve sabit Hakkımızda.
  - KK4: türe özgü bölge.
  - KK5: taslak ürün ilgili ürünlerde yok.
  - KK6: bağlantı var, ek uç yok.
  - KK7: bant, noindex, oturumsuz 404.
  - KK8: yabancı Slug 404.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T506: Sık sorulan sorular ve iletişim sayfası ve formu (E-09, E-10)
- **Bağımlılık:** T537, T540, T541, T543
- **Dokümanlar:** çekirdek; `09` §3.4, §8, §15 (§15.2, §15.4), §16 (§16.2); `07 UC-13`…`UC-15`; `04` E-09, E-10; `03 §2.2`, §5.1; `02 §3.27.6`, §3.32.1…§3.32.3; `10` KP-30, KP-33, KP-34
- **Kabul kriterleri:**
  1. `GET /sss` yayındaki soruları firmanın elle sırasıyla tek listede verir; sayfalama ve sayfa içi arama yoktur; soru yoksa boş hâl satırı çıkar (`07 UC-13`; `04` 5.9.4, 5.9.8).
  2. Panel oturumuyla taslak sorular yerinde "Taslak" etiketiyle görünür; SSS'nin yapılandırılmış verisi ilk yanıttadır (`07 UC-13`; `05 §3.7.2`).
  3. `GET /iletisim` kimlik bloğunu, formu ve şubeleri gösterir; üyede ad ve e-posta ön dolu ve düzenlenebilir gelir (`07 UC-14`; `04` 5.10.11).
  4. Veri toplayan girişlerin kapısı kapalıyken form kapalı olduğunu söyler ve gönderim düğmesi yoktur; kimlik bloğu ve şubeler görünür kalır (`07 UC-14`; K-850).
  5. Gönderim sonrası hâli koda göre formun yerinde durur: talep iletildi, cevap e-postayla sistem dışında gelir, talep numarası yok (`07 UC-14`; K-1180).
  6. `POST ?handler=SendContactRequest` talebi Açık durumunda yazar, 303 → `/iletisim` koduyla döner; gönderene alındı e-postası gitmez (`07 UC-15`; K-678).
  7. 422: boş zorunlu alan, geçersiz e-posta ya da telefon, kapalı beş değerin dışında konu tipi; hatalar birlikte işaretlenir; kapı kapalıysa sayfa kapalı hâliyle döner (`07 UC-15`).
  8. 429 (L-4): nötr mesaj düğmenin yanındadır, yazılanlar korunur; her deneme kaydedilir (`07 UC-15`; `03 §3.5.3.8`).
  9. `Website` tuzak alanı doluysa kayıt yazılmaz, yanıt başarıyla aynıdır; deneme kaydı ve L-4 sayımı yoktur (`07 UC-15`; `02 §3.32.7`).
  10. Tekrar gönderim ikinci talep açar ve L-4'e sayılır; firmaya F-2 tuzak boşken gider (`07 UC-15`; K-1179 (1)).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: elle sıra, boş hâl.
  - KK2: taslak etiketi; veri ilk yanıtta.
  - KK3: üye ön dolu, misafir boş.
  - KK4: kapalı kapıda düğme yok.
  - KK5: kod başına hâl.
  - KK6: talep Açık; alındı e-postası yok.
  - KK7: üç 422 sebebi ve kapalı kapı.
  - KK8: 429 ve deneme kaydı.
  - KK9: tuzakta kayıt ve sayaç yok.
  - KK10: ikinci talep ve F-2.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T507: Yasal metin sayfaları ve panelde yönetimi (E-11, E-47)
- **Bağımlılık:** T540, T541, T542, T543
- **Dokümanlar:** çekirdek; `09` §15 (§15.3); `07 UC-16`…`UC-18`, `UC-102`, `UC-173`, `UC-174`; `04` E-11, E-47; `03 §8.7`; `02 §3.33.1`, §3.33.2; `10` KP-61
- **Kabul kriterleri:**
  1. Aydınlatma metni ve çerez politikası güncel yayındaki sürümdür; işlem rehberi sabit metindir ve satış kapalıyken de görünür (`07 UC-16`…`UC-18`; K-626).
  2. İlk kurulumda hiç yayına alınmamış aydınlatma metni ve çerez politikası 404 verir; işlem rehberi bu kapıya bağlı değildir (`07 UC-16`…`UC-18`; K-770).
  3. `GET /panel/yasal-metinler` dört metnin satırını, düzenlenen iki metnin bölümünü ve üretilen iki metnin güncel hâlini verir; her form taslağın `Version`'ını taşır (`07 UC-102`).
  4. Sayfa yayındaki sürümün numarasını ve tarihini, düzenlenenin yayından farkını, tamamlanma satırını ve satışın kapalı olduğunu söyler (`07 UC-102`; K-818, K-850).
  5. Aydınlatma taslağının yer tutucuları kimlikteki güncel değerlerle gösterilir (`07 UC-102`; `04` 9.19.11).
  6. `SaveLegalTextDraft` taslağı kapalı biçim setiyle süzüp yazar; yayındaki sürüm ve işlem izi değişmez, taslağın sürümü artar (`07 UC-173`; `05 §6.5.3`).
  7. UC-173 422: metin boşaltılıyor, doldurulmuş barındırma konumu boşaltılıyor, tür listede yok ya da üretilen metin; 409 yalnız o bölümde iki düğme gösterir (`07 UC-173`; `07 §2.4.5`).
  8. `PublishLegalText` aynı metinde de yeni sürüm numarası açar; onay penceresi, bildirim ve yeniden onay yoktur; işlem izi yazılır (`07 UC-174`; `02 §3.33.3`).
  9. Tamamlanmamış aydınlatma metni 422 alır: istek uygulanmaz, taslak da yazılmaz; eşzamanlı ikinci yayın 409 alır (`07 UC-174`; K-1194 (3)).
  10. İki metnin ilk yayını satış ve veri toplayan girişlerin kapılarını açar; çerez politikasının ilk yayınından sonra vitrin çerez yazar (`07 UC-174`; `05 §6.7.10`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: üç adres, satış kapalı rehber.
  - KK2: yayınsızda 404, rehber 200.
  - KK3: dört satır, iki bölüm.
  - KK4: sürüm, fark, kapı.
  - KK5: yer tutucu güncel değerle.
  - KK6: yayındaki sürüm sabit, iz yok.
  - KK7: dört 422 sebebi, 409 düğmeleri.
  - KK8: aynı metinde yeni numara.
  - KK9: eksik alanda taslak yok.
  - KK10: kapılar, çerez.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T508: Sepet (E-12)
- **Bağımlılık:** T537, T540, T541
- **Dokümanlar:** çekirdek; `09` §3.4, §7, §9, §14, §16 (§16.2); `07 UC-19`…`UC-21`; `04` E-12; `03 §2.3`, §3.2; `02 §3.16.1`…§3.16.10, §3.18.1; `05 §6.7.7`, §6.7.8; `10` KP-9…KP-11, KP-38
- **Kabul kriterleri:**
  1. `GET /sepet` kalemleri, tutarın dökümünü, kargo ücretini, ücretsiz kargo mesajını ve asgari tutarın eksiğini sunucunun hesabıyla gösterir (`07 UC-19`; `04` 5.12.6).
  2. Kalem fiyatı ve satın alınabilirliği güncel üründen okunur; satır mesajları koşul sürdükçe durur (`07 UC-19`; `02 §3.16.5`).
  3. Ödeme adımına geçişin hâli — açık ya da sebebiyle kapalı — sunucunun kararıdır: siparişe girecek kalem yok, asgari tutar eksik, satış kapalı (`07 UC-19`; `04` 5.12.10, 5.12.14).
  4. Bir kez söylenen mesajlar içerik alanının başındadır; gösterilen işaretli kalem silinir ve yenilemede mesaj gelmez (`07 UC-19`; `04` 2.3.1.3; K-1179 (3), K-1183).
  5. Çerezsiz misafirin sepeti boştur ve GET çerez açmaz; çerezli misafirde son erişim yazılır ve çerez yenilenir (`07 UC-19`; `05 §6.7.8`).
  6. Sepet boşalırsa yanıt ödeme adımının çerezini siler (`07 UC-19`, `UC-21`; `05 §6.7.7`).
  7. `UpdateCartItem` adedi mutlak değer olarak yazar ve 303 → `/sepet` verir; ilk eklenme fiyatı değişmez, dijital kalemde adet alanı yoktur (`07 UC-20`; `02 §3.16.5`).
  8. UC-20 422: adet birden küçük ya da tam sayı değil; artırma stoku ya da sınırı aşıyor (adet yazılmaz, satırda mesaj durur); 404: kalem bu sepette değil (`07 UC-20`; K-1179 (2)).
  9. Vitrinden kalkmış kalemde adet yazılmaz; kalem bir kez söylenip silinir (`07 UC-20`; K-1179 (3)).
  10. `RemoveCartItem` işaretli kalemi de yayın durumundan bağımsız çıkarır; yabancı ya da çıkarılmış kalem 404 alır (`07 UC-21`; K-1209 (2)).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: dökümün her satırı sunucu hesabında.
  - KK2: fiyat değişince satır güncel.
  - KK3: üç kapalı sebep ayrı testte.
  - KK4: mesaj bir kez, kalem silinir.
  - KK5: çerezsiz GET çerez açmaz.
  - KK6: son kalemden sonra çerez silinir.
  - KK7: mutlak adet, tekrar aynı sonuç.
  - KK8: iki 422 sebebi ve 404.
  - KK9: kalkmış kalemde adet sabit.
  - KK10: çıkarma ve ikinci 404.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T509: Ödeme adımı (E-13)
- **Bağımlılık:** T508, T537, T540, T541, T543, T544
- **Dokümanlar:** çekirdek; `09` §3.4, §7, §8, §9, §14, §15 (§15.2, §15.4), §16 (§16.2); `07 UC-22`…`UC-27`; `04` E-13; `03 §2.4`, §6.1; `02 §3.14.1`…§3.14.6, §3.17.1, §3.17.2, §3.20.1; `05 §4.2.2`, §6.7.9; `10` KP-8, KP-12…KP-15
- **Kabul kriterleri:**
  1. `GET /odeme` beş bölümü tek sayfada verir; form her gösterimde yeni `ConfirmationKey`, özet parmak izi ve iki metnin sürüm kimliklerini taşır (`07 UC-22`; `05 §4.2.1`).
  2. Satış kapalı, siparişe girecek kalem yok ya da asgari tutar eksikse 303 → `/sepet`; sebep sepettedir (`07 UC-22`; K-1181 (3)).
  3. Alanlar ödeme adımının çerezinden dolar; çerez yalnız ayrılışta yazılır ve yalnız `05 §6.7.9`'un alanlarını taşır (`07 UC-22`, `UC-26`).
  4. `ApplyCoupon` yazmadan 200 döner; 422: asgari tutarın altındaki geçerli kod asgari tutarla, öteki retler tek mesajla; 429 (L-6); hak ayrılmaz (`07 UC-23`; K-845).
  5. `RemoveCoupon` kodu kaldırır; `Recalculate` özeti yeniden hesaplar, zorunluluk denetlemez ve hatırlatma L-3'e sayılmaz (`07 UC-24`, `UC-25`; K-758).
  6. `Leave` çerezi yazar: `login` → `/giris?returnUrl=/odeme`, `cart` → `/sepet`; başka hedef sepet sayılır (`07 UC-26`; K-1181 (2)).
  7. `PlaceOrder` siparişi `05 §4.2.2`'nin tek işleminde kurar, 303 → `{kök}/teyit` verir ve çerezi siler (`07 UC-27`; K-1181 (6)).
  8. UC-27 422: eksik alan ya da kutu, teslimat yapılmayan il, özet farkı, kapanan satış, seçilemeyen yöntem; 429 (L-7, L-8); yabancı defter girdisi 404 (`07 UC-27`; `04` 5.13.18).
  9. UC-27 303'leri: kalem kalmadı → `/sepet`; oluşmuş sipariş → teyit; belirsiz sonuç → sonucu gösteren ekran; kapanan oturum → giriş (`07 UC-27`; K-1199).
  10. Aynı onay anahtarıyla ikinci gönderim yeni sipariş açmaz; iki sekme sepet kilidiyle sıraya girer (`07 UC-27`; `07 §2.5.8`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: beş bölüm, taze anahtar.
  - KK2: üç kapalı sebep.
  - KK3: çerez alan listesi; kutu yazılmaz.
  - KK4: üç kupon sonucu ve L-6.
  - KK5: kaldırma, ara gönderim, hatırlatma.
  - KK6: üç hedef değeri.
  - KK7: tek işlem, çerez silinir.
  - KK8: beş 422 sebebi, 429, 404.
  - KK9: dört 303 hedefi.
  - KK10: eşzamanlı iki sipariş sıraya girer.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T510: Sipariş teyit ve bekleme ekranı, ödeme sağlayıcısına geçiş ve dönüş (E-14)
- **Bağımlılık:** T509, T511, T537, T540, T541
- **Dokümanlar:** çekirdek; `09` §3.4, §11 (§11.1), §15 (§15.2), §16; `07 UC-28`…`UC-30`; `08 §2.1.4`, §4.1; `04` E-14; `03 §2.4`, §2.5; `02 §3.17.1`, §3.17.6; `05 §6.5.4`; `10` KP-15, KD-23
- **Kabul kriterleri:**
  1. `GET {kök}/teyit` sipariş numarasını gösterir; kartta geçiş formu, havalede donmuş IBAN, toplam ve son ödeme günü durur; kendiliğinden yönlendirmez (`07 UC-28`; K-776).
  2. Sipariş Alındı + Bekliyor değilse 303: üye sipariş sayfasına, misafir takip girişine; kök tutmuyorsa 404 (`07 UC-28`; K-1209 (1)).
  3. `StartPayment` ödeme sayfasını 200 verir: sağlayıcı formu çerçevede açılır; jeton saklanmaz, her açılış yeni jeton ister (`07 UC-29`; `08 §2.1.4`).
  4. Çerçeve izni yalnız bu sayfada, yalnız sağlayıcı adresi ve `'self'` için açılır; `form-action` sağlayıcı adresini taşımaz (`07 UC-29`; `05 §6.5.4`).
  5. Sağlayıcıya bağlanılamazsa sipariş iptal edilir, 303 → `/sepet` ve sepet bir kez söyler; L-7'ye sayılmaz (`07 UC-29`; K-1183, K-1200).
  6. Sipariş artık Alındı + Bekliyor değilse UC-28'in yönlendirmesi; havale siparişinde teyit ekranı (`07 UC-29`).
  7. `GET /odeme/donus/{anahtar}` çerçevesiz tek cümlelik geçiş sayfasıdır; durum yazmaz, oturum çerezine dayanmaz (`07 UC-30`; K-1259 (1)).
  8. Geçiş sayfası üst pencereyi `/siparis/erisim/{anahtar}`'a götürür — betik, betiksizde `target="_top"` bağlantısı; `frame-ancestors 'self'` yalnız bu yanıttadır (`07 UC-30`).
  9. Anahtar biçimi tutmuyorsa 404 döner; geçersiz anahtarın 404'ünü sipariş sayfası verir (`07 UC-30`).
  10. DV-5 gözlenir: dönüşün yüklendiği pencere, iki hâlde üst pencereye geçiş, yükseklik, 3D Secure'un `frame-src`'ye takılmaması; sonuç satıra tarihle yazılır (`08 §4.1`; K-1450 (4)).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: kart ve havale görünümü.
  - KK2: iki yönlendirme ve 404.
  - KK3: jeton kayıtta yok; her açılışta yeni.
  - KK4: başlıkta çerçeve izni yalnız bu sayfada.
  - KK5: iptal, işaret, tek mesaj, L-7.
  - KK6: yönlendirme ve havale dönüşü.
  - KK7: geçiş sayfası durum yazmaz.
  - KK8: betikli ve betiksiz geçiş.
  - KK9: biçim 404.
  - KK10: DV-5 satırında tarih ve ihlal kaydı.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T511: Sipariş takibi girişi, sipariş sayfası ve indirme bağlantısı (E-15, E-16)
- **Bağımlılık:** T537, T540, T541, T542
- **Dokümanlar:** çekirdek; `09` §3.4, §15 (§15.4, §15.7), §16 (§16.2); `07 UC-31`…`UC-34`, `UC-36`; `04` E-15, E-16; `03 §2.6`; `02 §3.12.1`, §3.22.3…§3.22.5; `10` KP-18…KP-20
- **Kabul kriterleri:**
  1. `GET /siparis-takibi` sorgu formunu ve iki yolun hatırlatmasını verir; sayfa noindex'tir (`07 UC-31`).
  2. `FindOrder` numara (harf büyüklüğüne duyarsız) ve normalleştirilmiş e-posta eşleşirse 303 → `/siparis/erisim/{anahtar}` verir; silinmiş hesabın ve değişmiş e-postanın siparişi donmuş e-postayla eşleşir (`07 UC-32`; `04` 5.15.6).
  3. UC-32 422: boş alan ya da biçim; eşleşmeme alan ve sipariş varlığı söylemeyen nötr mesajdır, imha edilmiş sipariş eşleşmez; 429 (L-5) iki eksende ayrı sayılır; eşleşmeyen sorgu kaydedilir (`07 UC-32`; K-772).
  4. `GET /siparis/{numara}` üyenin kendi siparişidir: oturum yoksa 302 girişe; yabancı ya da bağlanmamış sipariş 404 (`07 UC-33`).
  5. `GET /siparis/erisim/{anahtar}` üyeye de açıktır; geçersiz, yenilenmiş ya da imha edilmiş sipariş 404'tür; yönetici oturumu müşteri sayılmaz (`07 UC-34`; `02 §3.22.3`).
  6. Sayfa iki ekseni, beklenen işlemi, kalemleri, dökümü, geri ödemeleri, teslimatı, adresleri ve belgeleri gösterir; belgeler onaylanan sürümlerdir (`07 UC-33`; `04` 5.16.3, 5.16.9).
  7. Yapılabilir işlemleri sunucu belirler; açık olmayan işlem hiç sunulmaz (`07 UC-33`; `03 §1.11`).
  8. Kart dönüşünde sonuç gelmediyse sayfa ödemenin beklendiğini söyler ve kendiliğinden yenilenmez (`07 UC-33`; `04` 5.16.4).
  9. `DownloadDigitalFile` bir saatlik imzalı bağlantı üretir, 303 → `/indir/{değer}`; hak tek koşullu güncellemeyle düşer, her basış bir haktır (`07 UC-36`; `05 §7.7.4`).
  10. UC-36 422: hak dolmuş, havalede ödeme işaretsiz, dosya yok; 404: kalem bu siparişin dijital kalemi değil (`07 UC-36`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: form, hatırlatma, noindex.
  - KK2: eşleşme, büyük harf, donmuş e-posta.
  - KK3: nötr mesaj, L-5, imha.
  - KK4: üç yetki sonucu.
  - KK5: anahtar durumları ve yönetici.
  - KK6: bölümler ve belge sürümü.
  - KK7: kapalı işlem sunulmaz.
  - KK8: bekleme metni, yenileme yok.
  - KK9: bir saatlik bağlantı, hak düşer.
  - KK10: üç 422 sebebi ve 404.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T512: İptal ve gecikme feshi ekranı, geri ödeme IBAN girişi (E-17, E-16 IBAN formu)
- **Bağımlılık:** T511, T541, T543
- **Dokümanlar:** çekirdek; `09` §3.4, §8, §9, §16 (§16.2); `07 UC-35`, `UC-38`…`UC-41`; `04` E-16, E-17; `03 §2.7`, §3.3, §5.1…§5.3; `10` KP-21
- **Kabul kriterleri:**
  1. `SaveRefundIban` IBAN'ı yazar ya da işlenene kadar yerinde düzeltir; boşluk yok sayılır; 303 → kök "kaydedildi" bildirimiyle (`07 UC-35`; `07 §2.9.5`).
  2. UC-35 422: biçim ya da sağlama basamağı tutmuyor, alan açık değil; 404: kalem bu siparişin değil (`07 UC-35`).
  3. IBAN isteği karşılanır; kart iadesi gerçekleşmeyip havale yolu açıldıysa geri ödeme havaleye döner, e-posta üretmez (`07 UC-35`; K-569, K-710).
  4. `GET {kök}/iptal` ve `…/gecikme-feshi` kapsamı, geri ödemeyi ve son adımı gösterir; `…/iptal` Bekliyor'da sipariş iptalini, ödenmişte kalem iptalini açar (`07 UC-38`; K-1162 (3)).
  5. Kalem başına açık adet, varsayılan ve üst sınır sunucudandır; dijital ve adedi bir kalemde adet alanı yoktur; IBAN alanı yalnız ödenmiş havale siparişindedir (`07 UC-38`; K-1170 (2)).
  6. İşlem açık değilse 303 → kök sebep koduyla; kök tutmuyorsa 404 (`07 UC-38`; K-1170 (1)).
  7. `CancelOrder` onaysız ilk gönderimde son adımı 200 verir ve yazmaz; onaylıda 303 → kök, iptal geçiş biriminde işlenir, ayırmalar serbest kalır (`07 UC-39`; `05 §4.1`).
  8. `CancelOrderItems` 422: kalem seçilmemiş, adet geçersiz, havalede IBAN eksik; işlem kapanmış ya da açık adet farklıysa 303 kök ve istek bütünüyle uygulanmaz (`07 UC-40`; K-1170 (4), K-1202).
  9. UC-40 geri ödemesi kart hattında kendiliğinden başlar, havalede IBAN'la firma işler; bütün fiziksel adetler kapsanırsa kargo ücreti de iade edilir (`07 UC-40`; `05 §4.6`; K-789).
  10. `SubmitLateDeliveryTermination` açık adetlerin tamamını kapsar, kalem ve adet alanlarını yok sayar; kargo ücreti her durumda iade edilir (`07 UC-41`; K-796).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: boşluklu IBAN, yerinde düzeltme.
  - KK2: iki 422 sebebi ve 404.
  - KK3: havale yoluna dönüş, e-posta yok.
  - KK4: iki adres ve iki işlem.
  - KK5: üç değer; alan görünürlüğü.
  - KK6: sebep kodlu 303 ve 404.
  - KK7: onaysızda kayıt yok; onaylıda iptal.
  - KK8: bütün istek reddi.
  - KK9: iki hat ve kargo iadesi.
  - KK10: alan yok sayma; kargo iadesi.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T513: Cayma beyanı ve ayıp talebi ekranları (E-18, E-19)
- **Bağımlılık:** T511, T541, T543, T544
- **Dokümanlar:** çekirdek; `09` §3.4, §8, §9, §16 (§16.2); `07 UC-42`…`UC-46`; `04` E-18, E-19; `03 §2.8`, §2.9, §3.3, §5.1; `02 §3.1.7`, §7.5.2; `10` KP-22, KP-23
- **Kabul kriterleri:**
  1. `GET {kök}/cayma` caymaya açık kalemleri, adetleri, koşullu istisna satırını, güncel iade adresini ve süresini, geri ödeme bilgisini gösterir; açık değilse 303 → kök sebep koduyla (`07 UC-42`; K-509, K-665).
  2. `SubmitWithdrawal` onaysız ilk gönderimde son adımı 200 verir; onaylıda 303 → kök, tarih ve iade adresi kalemde kalıcı durur (`07 UC-43`; `04` 5.18.10).
  3. Beyan kalem başına tarih damgası ve adetle yazılır; fiziksel kalemde iade adresi o anki değerle yazılır ve değişmez; geri alınmaz (`07 UC-43`; `06 §4.25`; `02 §7.6.7`).
  4. UC-43 422: kalem seçilmemiş, adet geçersiz, havalede IBAN; pencere kapandıysa ya da açık adet farklıysa 303 kök, istek uygulanmaz; yabancı kalem 404 (`07 UC-43`; K-1202).
  5. Kısmi seçimin tekrarı ikinci cayma olmaz: 303 kök, sebep koduyla (`07 UC-43`).
  6. `GET {kök}/ayip-talebi` yeni talepte açık kalemleri, varsayılan adet bir ve üst sınırı, iade adresini; yeniden açmada `orderItemId`'li talebi ve Çözüldü rozetini gösterir (`07 UC-44`; K-783, K-830).
  7. UC-44 303 kök: yeni talep açık değil, talep Çözüldü değil ya da Z-18 doldu; 404: kök tutmuyor, `orderItemId` yabancı ya da talep yok (`07 UC-44`).
  8. `SubmitDefectClaim` kalem başına tek talebi Açık durumunda açar; iade adresi talebe yazılır; onay adımı yoktur (`07 UC-45`; `04` 5.19.5).
  9. UC-45 422: kalem seçilmemiş, adet geçersiz, açıklama boş; kalemde talep varsa ya da adet aşılıyorsa 303 kök (`07 UC-45`; `03 §1.11.9`).
  10. `ReopenDefectClaim` Çözüldü talebi Z-18 içinde Açık yapar, açıklama almaz, durum geçmişine müşteri satırı yazar; aksi hâlde 303 kök (`07 UC-46`; K-681).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: sayfa içeriği; kapalı hâl.
  - KK2: onaysızda kayıt yok.
  - KK3: iade adresi sonradan değişmez.
  - KK4: üç 422 sebebi, pencere, 404.
  - KK5: ikinci gönderim cayma açmaz.
  - KK6: iki hâl, varsayılan adet.
  - KK7: dört 303 sebebi ve 404.
  - KK8: tek talep, Açık.
  - KK9: üç 422 sebebi, 303.
  - KK10: Z-18 içi ve dışı.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T514: Kayıt ve doğrulama bağlantısının inişi (E-20, E-21)
- **Bağımlılık:** T537, T540, T541, T543
- **Dokümanlar:** çekirdek; `09` §3.4, §8, §15 (§15.1, §15.4), §16 (§16.2); `07 UC-47`…`UC-49`, `UC-52`, `UC-53`; `04` E-20, E-21; `03 §3.4`, §9.1; `02 §3.13.4`, §3.13.5, §3.13.11, §3.13.18; `10` KP-25
- **Kabul kriterleri:**
  1. `GET /kayit` ad, e-posta ve şifre formunu verir; kapı kapalıyken form kapalı olduğunu söyler; Google düğmesi ve işleyicisi T515'tedir (`07 UC-47`; `04` 5.20.10; K-850).
  2. Gönderim sonrası hâli koda ve `pendingRegistration` değerine bağlıdır; değer çatının anahtarıyla şifreli ve imzalıdır, kayıtta tutulmaz (`07 UC-47`; K-1186 (2)).
  3. `Register` doğrulanmamış bekleyen kaydı açar: `EmailVerifiedAt` boş, bağlantı Z-1 ömürlü, doğrulama e-postası; oturum açılmaz; 303 → `/kayit` (`07 UC-48`; `02 §3.13.4`).
  4. UC-48 422: ad boş, e-posta geçersiz, şifre P-17 altı ya da yaygın, e-posta kayıtlı, kapı kapalı; 429 (L-3) (`07 UC-48`; `03 §3.4.6`).
  5. Tekrar gönderim 422 alır; ikinci kayıt ve bağlantı açılmaz (`07 UC-48`; `06 §4.36`).
  6. `ResendVerificationLink` yeni bağlantı gönderir; öncekiler `SupersededAt` alır, kaydın ömrü uzamaz; 429 (L-9) (`07 UC-49`; `05 §6.4.6`).
  7. `GET /dogrulama/{anahtar}` bağlantının hâline göre altı hâlden birini verir; geçerli bağlantıda doğrulama adımıdır, açılış doğrulamaz (`07 UC-52`; K-904, K-780).
  8. Başka türün ya da yönetici hesabının bağlantısı tanınmaz ve tüketilmez (`07 UC-52`).
  9. `VerifyEmail` bağlantıyı tek koşullu güncellemeyle tüketir, 303 sonuç koduyla döner, oturum açmaz; kayıtta `EmailVerifiedAt` yazılır ve misafir siparişleri hesaba bağlanır (`07 UC-53`; K-1186 (1)).
  10. UC-53: bağlantı o arada geçersizleştiyse 200; yeni adres başkasına geçtiyse 422 ve tüketim geri döner (`07 UC-53`; `02 §3.13.14`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: kapalı kapıda düğme yok.
  - KK2: değer kayıtta yok.
  - KK3: bekleyen kayıt, Z-1, oturum yok.
  - KK4: beş 422 sebebi ve 429.
  - KK5: ikinci kayıt açılmaz.
  - KK6: eski bağlantı geçersiz, ömür sabit.
  - KK7: altı hâl; açılış yazmaz.
  - KK8: yabancı tür tüketilmez.
  - KK9: tek tüketim; sipariş bağlama.
  - KK10: yarış ve başkasına geçen adres.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T515: Müşteri girişi ve Google ile giriş (E-22)
- **Bağımlılık:** T514, T537, T540, T541, T543
- **Dokümanlar:** çekirdek; `09` §3.4, §8, §11, §15 (§15.1, §15.4), §16 (§16.2); `07 UC-50`, `UC-51`, `UC-54`, `UC-55`; `04` E-22; `03 §3.4`, §6.1, §9.1, §9.2; `02 §3.13.6`, §3.13.12; `10` KP-26, KP-27
- **Kabul kriterleri:**
  1. `GET /giris` e-posta, şifre, "beni hatırla", Google, şifremi unuttum ve kayıt bağlantılarını verir; Google tanımsızsa düğme yoktur; kapı kapalıyken giriş çalışır (`07 UC-54`).
  2. `returnUrl` yerel yol denetiminden geçerse dönüş yeridir; giriş, kayıt, sıfırlama ya da iniş ekranıysa ya da yoksa ana sayfaya dönülür (`07 UC-55`; `04` 3.6.1…3.6.3).
  3. `SignIn` her girişte yeni oturum anahtarı açar, sepetler birleşir ve dönülen sayfa eklenenleri bir kez söyler (`07 UC-55`; `05 §6.1.1`; K-1183).
  4. UC-55 422: boş alan ya da biçim; yanlış e-posta ya da şifre hesabın varlığını söylemeyen tek mesajdır, hesap yokken de sahte özete karşı doğrulanır (`07 UC-55`; K-779, K-1204 (1)).
  5. Doğrulanmamış hesapta oturum açılmaz; doğrulamanın beklendiği hâl ve yeniden gönderim durur; gönderim T514'ün servisini çağırır (`07 UC-55`, `UC-49`).
  6. `StartSocialLogin` (`/giris` ve `/kayit`) 303 → Google adresi verir; akış çerezi `state`, `nonce` ve PKCE taşır; Google tanımsızsa 422 (`07 UC-50`; `05 §6.7.12`).
  7. `GET /giris/google`: doğrulanmış e-posta eşleşen hesaba girer ve Google bağı yazılır — adres için yetkiliyse ya da hesap bağlıysa —; eşleşme yoksa ve kapı açıksa şifresiz hesap açılır (`07 UC-51`; `06 §4.36`).
  8. Yetkisiz adreste bağ yerine `GoogleLinkConfirmation` teyit bağlantısı gider, oturum açılmaz (`07 UC-51`).
  9. Doğrulanmamış Google e-postası 303 → `/kayit`, eşleşme yok ya da kapı kapalı 303 → `/giris` kodla döner (`07 UC-51`; K-697).
  10. Akış değerleri bir kez kullanılır: yenilenen dönüş hiçbir şey yazmadan 303 → `/giris` alır (`07 UC-51`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: Google tanımsız ve kapı kapalı.
  - KK2: dönüş yeri üç durumda.
  - KK3: oturum, işaret, birleşme mesajı.
  - KK4: tek mesaj; sabit süre.
  - KK5: doğrulanmamış hesap hâli.
  - KK6: çerez alanları, 422.
  - KK7: üç Google sonucu.
  - KK8: teyit bağlantısı, oturum yok.
  - KK9: iki kodlu 303.
  - KK10: tekrar dönüş yazmaz.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T516: Şifre sıfırlama ve yeniden doğrulama (E-23, E-24)
- **Bağımlılık:** T515, T537, T540, T541, T543
- **Dokümanlar:** çekirdek; `09` §3.4, §8, §15 (§15.1, §15.4), §16 (§16.2); `07 UC-56`…`UC-62`; `04` E-23, E-24; `03 §3.4`, §6.1, §9.2; `02 §3.13.8`…§3.13.11
- **Kabul kriterleri:**
  1. `GET /sifre-sifirlama` e-posta alanını verir; müşteri oturumu açıksa alan hesabın e-postasıyla dolar ve e-posta adreste taşınmaz (`07 UC-56`; K-1186 (6)).
  2. `RequestPasswordReset` 303 → `/sifre-sifirlama` nötr hâliyle döner; yanıt adresin kayıtlı olup olmamasına göre değişmez (`07 UC-57`; `02 §8.1.4`).
  3. Doğrulanmış hesapta `PasswordReset` bağlantısı (Z-2) ve e-postası yazılır; şifresiz hesapta bağlantı şifre belirlemedir; kayıtsız adrese e-posta gitmez (`07 UC-57`; K-1204 (2)).
  4. UC-57 422: boş ya da biçim; 429 (L-2) nötr mesajdır; her istek kaydedilir (`07 UC-57`).
  5. `GET /sifre-sifirlama/{anahtar}` geçerli bağlantıda kurma adımını açar; kullanılmış, dolmuş, yenilenmiş ya da tanınmayan bağlantı adımı açmaz (`07 UC-58`).
  6. `ResetPassword` şifreyi kurar, bağlantıyı tek koşullu güncellemeyle tüketir, hesabın bütün oturumlarını siler, oturum açmaz (`07 UC-59`; `05 §6.1.2`).
  7. UC-59: P-17 altı ya da yaygın şifre 422 alır, bağlantı tüketilmez; bağlantı o arada geçersizleştiyse 200 döner (`07 UC-59`; `03 §3.4.6`).
  8. `GET /hesap/yeniden-dogrulama` işlemin adını, şifre alanını (şifre varsa) ve Google düğmesini (`GoogleLinkedAt` doluysa) gösterir; işlem listede yoksa 303 → `/hesap` (`07 UC-60`; K-1186 (4)).
  9. `Reauthenticate` şifre doğruysa 303 → `/hesap`'a işlem ve tek kullanımlık değerle gider, oturumda tek izin kalır; yanlış şifre 422'dir, L-1'e sayılır (`07 UC-61`; K-971).
  10. `StartSocialReauthentication` taze giriş ister (`auth_time`), dönüş UC-51'in adresine iner; Google'sız hesapta 422; L-1'e girmez (`07 UC-62`; `05 §6.1.5`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: oturumlu ön dolu, adreste e-posta yok.
  - KK2: kayıtlı ve kayıtsız aynı yanıt.
  - KK3: üç bağlantı hâli, e-posta yok.
  - KK4: 422, 429 ve deneme kaydı.
  - KK5: beş bağlantı durumu.
  - KK6: oturumlar silinir, oturum açılmaz.
  - KK7: ret tüketmez; yarışta 200.
  - KK8: üç işlem, iki alan koşulu.
  - KK9: tek izin; L-1 sayımı.
  - KK10: Google dönüşü; L-1 dışı.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T517: Hesap — profil ve güvenlik (E-25)
- **Bağımlılık:** T516, T537, T540, T541, T542, T543
- **Dokümanlar:** çekirdek; `09` §3.4, §8, §15 (§15.1, §15.4), §16 (§16.2); `07 UC-63`…`UC-69`; `04` E-25; `03 §3.4`, §9.3, §9.4; `02 §3.13.10`, §3.13.14, §3.15.6; `10` KP-27, KP-29
- **Kabul kriterleri:**
  1. `GET /hesap` beş bölümü verir; oturum yoksa 302 girişe gider (`07 UC-63`).
  2. İşlemin formu yalnız oturumun izni o işlem içinse ve değerin özeti oturum kaydındakiyle aynıysa açık gelir; tutmayan parametre yok sayılır (`07 UC-63`; `05 §6.1.5`; K-971).
  3. Geri alma bağlantısı açıkken e-posta değiştirme ve hesap silme kapalıdır, sebep ve bitiş tarihi yerindedir; şifresiz hesapta şifre belirleme bağlantısı sunulur (`07 UC-63`; `02 §3.15.6`).
  4. `ChangeName` adı yazar, 303 → `/hesap`; boş ad 422'dir; geçmiş siparişin alıcı adı değişmez (`07 UC-64`; `02 §3.13.21`).
  5. `ChangePassword` değeri tek koşullu güncellemeyle tüketir ve değiştiren dışındaki oturumları siler (`07 UC-65`; K-695).
  6. UC-65: değer yok ya da tutmuyorsa 303 → yeniden doğrulama; P-17 altı ya da yaygın şifre 422'dir ve değeri tüketmez; şifresiz hesapta işlem sunulmaz (`07 UC-65`).
  7. `ChangeEmail` bekleyen değişikliği yazar, öncekinin yerine geçer, yeni adrese bağlantı gönderir; hesap eski adresle çalışır (`07 UC-66`; K-734).
  8. UC-66 422: biçim, doğrulanmış başka hesabın adresi, açık geri alma bağlantısı; 429 (L-9) yalnız yerine geçen istekte; hatada değer tüketilmez (`07 UC-66`; K-1134).
  9. `ResendEmailChangeLink` yeni bağlantı verir, öncekiler `SupersededAt` alır, ömür uzamaz; bekleyen yoksa 422, 429 (L-9) (`07 UC-67`).
  10. `DeleteAccount` onaysızda 200 verir ve yazmaz; onaylıda tek işlemde siler, siparişe işaret yazar, 303 → `/hesap/silindi`; sonuç hâli çerezle bir kez görünür (`07 UC-68`, `UC-69`; K-1184).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: oturumsuz 302.
  - KK2: işlem izni ve özet eşleşmesi.
  - KK3: açık geri alma bağlantısında kapalı işlemler.
  - KK4: ad değişir, sipariş adı sabit.
  - KK5: tek tüketim; öteki oturumlar silinir.
  - KK6: üç ret sebebi.
  - KK7: bekleyen değişiklik ve yeni bağlantı.
  - KK8: üç 422 sebebi, L-9.
  - KK9: eski bağlantılar geçersiz.
  - KK10: onaysızda kayıt yok; ikinci istek ana sayfaya iner.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T518: Adres defteri ve sipariş geçmişi (E-26, E-27)
- **Bağımlılık:** T516, T537, T540, T541, T543, T545
- **Dokümanlar:** çekirdek; `09` §3.4, §8, §16 (§16.2); `07 UC-70`…`UC-74`; `04` E-26, E-27; `03 §2.6`, §9.3; `02 §3.14.1`, §3.14.6, §3.17.8; `10` KP-27
- **Kabul kriterleri:**
  1. `GET /hesap/adresler` kayıtlı adresleri verir; defter boşsa boş hâl satırı çıkar (`07 UC-70`; `04` 5.26.9).
  2. `addressBookId` düzenleme formunu, `create` ekleme formunu ekranın içinde açar; başkasının girdisi 404'tür (`07 UC-70`; K-1186 (3)).
  3. `CreateAddressBook` girdi yazar, 303 → `/hesap/adresler` "kaydedildi" bildirimiyle; teslimat yapılmayan ildeki girdi tutulur (`07 UC-71`).
  4. UC-71 ve UC-72 422: boş zorunlu alan, listede olmayan il, geçersiz telefon; form değerleriyle açık kalır (`07 UC-71`, `UC-72`).
  5. `UpdateAddressBook` yalnız sahibine yazar; yabancı ya da yok girdi 404; geçmiş siparişe dokunmaz (`07 UC-72`; `02 §3.14.5`).
  6. `DeleteAddressBook` onaysız siler; ikinci gönderim 404'tür, yazmaz; geçmiş siparişe dokunmaz (`07 UC-73`).
  7. Tekrar gönderim ikinci girdi açar; çift tıklamayı betikli yolda düğmenin kilidi önler (`07 UC-71`; K-1179 (1)).
  8. `GET /hesap/siparisler` hesaptaki siparişleri en yeni önce, sayfa başına 25 verir; arama ve süzgeç yoktur (`07 UC-74`; K-784).
  9. Hesaba düşmüş misafir siparişi listededir; ödemesi hiç alınmamış kendiliğinden iptal edilen sipariş yoktur (`07 UC-74`; `02 §3.17.8`).
  10. Her satır siparişin üye köküne bağlanır; siparişsiz üyede boş hâl satırı çıkar (`07 UC-74`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: dolu ve boş defter.
  - KK2: iki form ve yabancı girdi 404.
  - KK3: ekleme ve bildirim; kısıtlı il tutulur.
  - KK4: üç 422 sebebi iki işleyicide.
  - KK5: sahiplik; sipariş adresi sabit.
  - KK6: silme ve ikinci 404.
  - KK7: ikinci gönderim ikinci girdi.
  - KK8: sıra, 25'lik sayfa.
  - KK9: misafir siparişi var, iptal edilen yok.
  - KK10: üye kökü bağlantısı; boş hâl.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T519: E-posta değişikliğini geri alma ve müşteri çıkışı (E-28)
- **Bağımlılık:** T517, T537, T540, T541
- **Dokümanlar:** çekirdek; `09` §3.4, §8, §15 (§15.1, §15.4), §16 (§16.2); `07 UC-75`…`UC-77`; `04` E-28; `03 §3.4`, §6.2, §9.2, §9.3; `02 §3.13.14`; `10` KP-27
- **Kabul kriterleri:**
  1. `GET /geri-al/{anahtar}` geçerli bağlantıda geri alma adımını verir; açılış geri almaz (`07 UC-75`; K-839).
  2. Kullanılmış bağlantı sonuç koduyla sonuçları, kodsuz geçersiz hâli gösterir; dolmuş ya da tanınmayan bağlantı geçersiz hâldir (`07 UC-75`; K-1186 (1)).
  3. Başka türün ya da yönetici hesabının bağlantısı tanınmaz ve tüketilmez (`07 UC-75`).
  4. `RevertPendingEmailChange` bağlantıyı tüketir, e-postayı eski adrese döndürür, 303 → `/geri-al/{anahtar}` sonuç koduyla (`07 UC-76`).
  5. Geri alma hesabın bütün oturumlarını siler, değişiklik süresince bağlanan Google girişini kaldırır, şifreyi geçersiz kılar ve eski adrese sıfırlama bağlantısı gönderir (`07 UC-76`; K-599, K-1117).
  6. Adresi tutan doğrulanmamış bekleyen kayıt silinir ve imha kaydına yazılır; değişiklik süresince bağlanan siparişler hesapta kalır (`07 UC-76`; K-1080).
  7. Eski adres o arada başka doğrulanmış hesabın olduysa 422: hiçbir şey yazılmaz, tüketim geri döner (`07 UC-76`; K-1210).
  8. Bağlantı o arada geçersizleştiyse 200 döner ve sayfa o anki hâli gösterir; tekrar gönderim aynı sonucu verir (`07 UC-76`).
  9. `SignOut` oturum kaydını ve çerezi siler; sepet hesapta kalır (`07 UC-77`; `05 §6.1.2`).
  10. `ReturnUrl` yerel yol denetiminden geçen, oturum gerektirmeyen sayfaysa oraya, değilse ana sayfaya 303 verir (`07 UC-77`; K-1186 (6)).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: açılış yazmaz; adım görünür.
  - KK2: üç hâl.
  - KK3: yabancı tür tüketilmez.
  - KK4: eski adres döner, tek tüketim.
  - KK5: oturumlar, Google bağı, şifre, e-posta.
  - KK6: bekleyen kayıt silinir; imha kaydı.
  - KK7: çakışmada hiçbir şey yazılmaz.
  - KK8: yarışta 200; ikinci gönderim.
  - KK9: oturum silinir, sepet durur.
  - KK10: üç dönüş yeri.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T520: Panel girişi, şifre sıfırlama, davet kabulü ve yönetici çıkışı (E-29, E-30)
- **Bağımlılık:** T540, T541, T543
- **Dokümanlar:** çekirdek; `09` §3.4, §8, §15 (§15.1), §16 (§16.2); `07 UC-84`, `UC-85`, `UC-109`…`UC-113`; `04` E-29, E-30; `03 §8.8`, §9.2; `02 §10.2`; `10` KP-62
- **Kabul kriterleri:**
  1. `GET /panel/giris` "beni hatırla" işaretsiz verir; oturumun kapandığı kodla mesaj olur; davetten gelen korumalı değer e-postayı ön doldurur, çözülmeyen yok sayılır (`07 UC-84`; K-807, K-1190 (2)).
  2. Kullanılmış, dolmuş, yenilenmiş ya da tanınmayan sıfırlama bağlantısı kurma adımını açmaz; sayfa isteme adımına götürür (`07 UC-84`).
  3. `SignIn` her girişte yeni anahtarla `__Host-admin-session` açar; 303 → yerel `returnUrl` ya da `/panel`; vitrin oturumu ve sepet etkilenmez (`07 UC-110`; K-899).
  4. UC-110 422: boş alan ya da biçim; yanlış e-posta ya da şifre varlığı söylemeyen tek mesajdır, hesap yokken de sahte özetle doğrulanır (`07 UC-110`; K-1204).
  5. UC-110 429 (L-1): muafiyet yoktur, hesap kilitlenmez; her deneme giriş kaydındadır (`07 UC-110`; K-972).
  6. `RequestPasswordReset` 303 → nötr hâl verir; yönetici olmayan adrese e-posta gitmez; her istek L-2'ye sayılır; 422 biçim (`07 UC-111`).
  7. `ResetPassword` bağlantıyı tek koşullu tüketir, bütün oturumları siler, oturum açmaz (`07 UC-112`).
  8. UC-112: P-17 altı ya da yaygın şifre 422'dir, bağlantı tüketilmez; bağlantı arada geçersizleştiyse 200 döner (`07 UC-112`).
  9. `GET /panel/davet/{anahtar}` geçerli davette adresi salt okunur verir, geçersizde formu açmaz ve sebebi ayırt etmez; `AcceptAdminInvitation` hesabı davetin adresiyle açar, bağlantıyı tüketir, iz yazar, oturum açmaz (`07 UC-85`, `UC-113`; K-1085).
  10. UC-113 422: ad boş, P-17 altı ya da yaygın şifre; `SignOut` oturumu siler, 303 → `/panel/giris` (`07 UC-113`, `UC-109`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: kod ve korumalı değer.
  - KK2: dört geçersiz bağlantı.
  - KK3: iki oturum ayrı; dönüş yeri.
  - KK4: tek mesaj; sabit süre.
  - KK5: 429 ve giriş kaydı.
  - KK6: e-posta yok; L-2.
  - KK7: tek tüketim; oturumlar silinir.
  - KK8: ret tüketmez; yarışta 200.
  - KK9: geçersiz davet; hesap açılır.
  - KK10: iki 422; çıkış.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T521: Panel ana sayfası ve yönetici hesabının doğrulama ekranları (E-31, E-54)
- **Bağımlılık:** T520, T540, T541, T542, T543
- **Dokümanlar:** çekirdek; `09` §3.4, §8, §15 (§15.1), §16 (§16.2); `07 UC-86`, `UC-108`, `UC-185`…`UC-187`; `04` E-31, E-54; `03 §8.9`, §10.1; `10` KP-50, KP-65
- **Kabul kriterleri:**
  1. `GET /panel` uyarıları, kurulum kontrol listesini, altı sayacı ve "Müşteriden beklenenler"i verir; boş bölüm görünmez (`07 UC-86`).
  2. Değerler istek anında sunucunun hesabıdır, sayaçlar saklanmaz; eksik madde satış kapısının koşullarındandır ve kendi ayar ekranına bağlanır (`07 UC-86`; `02 §10.8.2`).
  3. Sayaçlar ilgili listeye süzgeçle bağlanır; "IBAN bekleniyor" ilk on satırı kalan süreye göre verir (`07 UC-86`).
  4. `GET /panel/yeniden-dogrulama` işlemin adını ve şifre alanını verir, Google yoktur; işlem `PasswordChange`, `EmailChange` dışındaysa 303 → `/panel/hesabim`; açılış önceki izni düşürür (`07 UC-108`).
  5. `Reauthenticate` doğru şifrede 303 → `/panel/hesabim`'a işlem ve tek kullanımlık değerle gider; yanlış şifre 422'dir ve L-1'e sayılır; 429 işlemi yapmaz (`07 UC-185`; K-971).
  6. İniş ekranları çerçevesiz ve oturumdan bağımsızdır; hâl bağlantı kaydından, kod yalnız kullanılmış bağlantıdan okunur (`07 UC-108`).
  7. `VerifyEmail` bağlantıyı tek koşullu tüketir, e-postayı yeni adres yapar, eski adrese bildirim ve geri alma bağlantısı gönderir, iz yazar; oturum açmaz (`07 UC-186`).
  8. UC-186: bağlantı arada geçersizleştiyse 200 ve yazmaz; yeni adres başka yöneticinin olduysa 422 ve tüketilmez (`07 UC-186`).
  9. `RevertPendingEmailChange` e-postayı eski adrese döndürür, bütün oturumları siler, şifreyi geçersiz kılar, sıfırlama bağlantısı gönderir (`07 UC-187`; K-1210).
  10. UC-187: eski adres başka yöneticinin olduysa 422, tüketim geri döner; bağlantı arada geçersizleştiyse 200 (`07 UC-187`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: boş bölüm gizli.
  - KK2: durum değişince sayaç değişir.
  - KK3: üç süzgeç; ilk on satır.
  - KK4: iki işlem; dışı 303.
  - KK5: yanlış şifre L-1; tek izin.
  - KK6: üç iniş hâli, kod koşulu.
  - KK7: tek tüketim; bildirim ve bağlantı.
  - KK8: yarışta 200; başkasına geçen adres.
  - KK9: oturumlar, şifre, e-posta.
  - KK10: çakışmada geri dönüş; yarışta 200.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T522: Yönetici hesapları ve işlem izi (E-49, E-52)
- **Bağımlılık:** T520, T540, T541, T543, T544, T545
- **Dokümanlar:** çekirdek; `09` §3.4, §8, §16 (§16.2); `07 UC-103`, `UC-106`, `UC-175`…`UC-177`; `04` E-49, E-52; `03 §8.8`; `02 §10.2`, §10.3; `10` KP-62
- **Kabul kriterleri:**
  1. `GET /panel/yonetici-hesaplari` yöneticileri Türkçe alfabetik sırayla verir; kendi satırında kaldırma yoktur, tek yönetici varken hiçbirinde görünmez (`07 UC-103`).
  2. Yalnız geçerli davetler görünür, en yeni önce; satır gönderen adını, son günü ve "e-posta ulaşmadı" işaretini taşır (`07 UC-103`; K-540).
  3. `SendAdminInvitation` davet satırını, Z-3 ömürlü bağlantıyı ve e-postayı yazar, iz bırakır, bütün yöneticilere F-6 gönderir; onay istemez (`07 UC-175`; `02 §10.2.6`).
  4. Aynı adresin önceki daveti aynı işlemde geçersiz olur; L-9'a girmez; 422: biçim, yönetici hesabının adresi (`07 UC-175`; K-661, K-820).
  5. `RevokeAdminInvitation` bağlantıyı geçersiz kılar, iz yazar, bildirim göndermez; kullanılmış davette 303 yazmaz; satır yoksa 404 (`07 UC-176`; K-660).
  6. `RemoveAdmin` onaysızda onay hâlini 200 verir ve yazmaz; onaylıda 303; 422: kendi satırı, son yönetici; 404: yok (`07 UC-177`; K-662, K-814).
  7. Kilidin altında önce işlemi yapanın hâlâ yönetici olduğu, sonra son yönetici denetlenir; yönetici değilse hiçbir şey yazılmaz, istek girişe iner (`07 UC-177`).
  8. Kaldırma tek işlemde hesabı, oturumları ve bağlantıları siler, davetlerini geçersiz kılar; iz, `Admin` imha kaydı ve F-6 yazılır (`07 UC-177`).
  9. `GET /panel/islem-izi` satırları en yeni önce, sayfada 25 verir; satırda işlem anındaki ad ve e-posta vardır, müşterinin kişisel verisi yoktur; kaldırılmış yönetici seçilir (`07 UC-106`; K-1067).
  10. Tarih ve yönetici süzgeci birlikte uygulanır; ileri ya da ters aralıkta alan mesajı çıkar, süzgeç uygulanmaz, durum 200'dür; boş sonuç süzgeci kaldırma yolunu taşır (`07 UC-106`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: sıra, tek yönetici.
  - KK2: davet satırı.
  - KK3: bağlantı, e-posta, iz, F-6.
  - KK4: önceki davet; iki 422.
  - KK5: geri çekme, 303, 404.
  - KK6: onay hâli, iki 422, 404.
  - KK7: karşılıklı kaldırma.
  - KK8: basamaklı silme, iz, imha kaydı.
  - KK9: sayfa; kişisel veri yok.
  - KK10: süzgeç birleşimi, ters aralık.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T523: Yöneticinin kendi hesabı (E-50)
- **Bağımlılık:** T521, T540, T541, T543
- **Dokümanlar:** çekirdek; `09` §3.4, §8, §16 (§16.2); `07 UC-104`, `UC-178`…`UC-181`; `04` E-50; `03 §9.3`; `02 §3.13.16`, §10.3.1; `10` KP-62
- **Kabul kriterleri:**
  1. `GET /panel/hesabim` üç bölümü verir; işlemin formu yalnız oturumun izni o işlem içinse ve değerin özeti oturum kaydındakiyle aynıysa açık gelir; tutmayan parametre yok sayılır (`07 UC-104`; `05 §6.1.5`).
  2. Bekleyen değişiklik yeni adresi, son günü ve yeniden isteme düğmesiyle görünür; geri alma bağlantısı açıkken e-posta değiştirme kapalıdır, sebep yerindedir (`07 UC-104`; K-819).
  3. `ChangeName` adı yazar, 303 döner; boş ad 422'dir; kayıtlarda ve izin eski satırlarında ad değer olarak kalır (`07 UC-178`; K-736).
  4. `ChangePassword` değeri tek koşullu güncellemeyle tüketir, değiştiren dışındaki oturumları siler, öteki tarayıcıların işaretlerini düşürür (`07 UC-181`; K-695).
  5. UC-181: değer yok ya da tutmuyorsa 303 → yeniden doğrulama (`PasswordChange`); P-17 altı ya da yaygın şifre 422'dir ve tüketmez (`07 UC-181`).
  6. Şifre değişikliği işlem izine yazılmaz (`07 UC-181`; `02 §10.3.1`).
  7. `ChangeEmail` bekleyen değişikliği yazar, öncekinin yerine geçirir, yeni adrese `NewEmailVerification` bağlantısı ve işlem izi bırakır; hesap eski adresle çalışır (`07 UC-179`; K-734).
  8. UC-179: değer yok ya da tutmuyorsa 303 → yeniden doğrulama (`EmailChange`); 422: biçim, başka yöneticinin adresi, açık geri alma bağlantısı; müşteri adresi engel değildir; hatada değer tüketilmez (`07 UC-179`; `02 §3.13.16`).
  9. UC-179 429 (L-9) yalnız bekleyen değişikliğin yerine geçen istektedir; `ResendEmailChangeLink` yeni bağlantı verir, öncekiler `SupersededAt` alır, ömür uzamaz; bekleyen yoksa 422 (`07 UC-180`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: izin ve özet eşleşmesi.
  - KK2: bekleyen değişiklik; kapalı işlem.
  - KK3: ad, eski kayıt adı.
  - KK4: tek tüketim; oturumlar.
  - KK5: iki ret; tüketilmez.
  - KK6: iz satırı yok.
  - KK7: bekleyen değişiklik, bağlantı, iz.
  - KK8: üç 422, yeniden doğrulama.
  - KK9: L-9 koşulu; eski bağlantılar geçersiz.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T524: Satış özeti ve dışa aktarma (E-51, E-53)
- **Bağımlılık:** T520, T540, T541, T543, T544
- **Dokümanlar:** çekirdek; `09` §3.4, §8, §16 (§16.2); `07 UC-105`, `UC-107`, `UC-182`…`UC-184`; `04` E-51, E-53; `03 §6.3`, §8.9; `02 §3.25.1`, §10.6, §10.7; `10` KP-52, KP-53, KD-54
- **Kabul kriterleri:**
  1. `GET /panel/satis-ozeti` parametresizken "Son 30 gün"dür; hazır dönemler ve özel aralık çalışır, sınırlar Türkiye günüyledir (`07 UC-105`; K-1215).
  2. Değerler istek anında sunucunun hesabıdır, saklanmaz; sipariş sayısı ve ciro oluştuğu güne, iptal ve iade olay tarihine yazılır (`07 UC-105`; `02 §10.6.3`; K-580).
  3. Üç oran payı ve paydasıyla gelir, payda sıfırsa "Değerlendirilemez" yazar; "Ölçüm penceresi" satış hiç açılmadıysa sunulmaz (`07 UC-105`; K-486, K-821).
  4. Özel aralıkta tarih eksik, ileri ya da ters ise alan mesajı çıkar, özet hesaplanmaz, durum 200'dür (`07 UC-105`).
  5. `GET /panel/disa-aktarma` üç bölümü ve sorumluluk satırını verir (`07 UC-107`; K-823).
  6. `ExportOrders` 200 ile CSV verir: `siparisler-{başlangıç}-{bitiş}.csv`, `attachment`, `noindex`; satır bir kalemdir, yalnız ödemesi tamamlanmış siparişler girer; imha edilmiş siparişin kişisel sütunları boştur (`07 UC-182`; K-1213).
  7. Müşteri girdisi taşıyan hücre `=`, `+`, `-`, `@`, sekme ya da CR ile başlıyorsa etkisizleştirilir; dosya sunucuda kalmaz (`07 UC-182`; K-1195 (3), K-1217 (3)).
  8. `ExportCustomers` bekleyen kayıt dahil bütün hesapları, `ExportContactRequests` saklanan bütün talepleri verir (`07 UC-183`, `UC-184`).
  9. Her dışa aktarma dosya üretildikten sonra işlem izine yazılır, her indirme ayrı satırdır; üçü de onay istemez (`07 UC-182`…`UC-184`; `02 §10.3.1` (7)).
  10. 422: tarih eksik, ileri ya da ters; dosyaya girecek kayıt yok — dosya üretilmez, iz yazılmaz, bölümde kalıcı mesaj; 500'de iz yazılmaz (`07 UC-182`…`UC-184`; K-1195 (4)).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: hazır dönemler ve özel aralık.
  - KK2: ciro günü ve iptal günü ayrı.
  - KK3: sıfır payda; pencere yok.
  - KK4: üç hatalı aralık, 200.
  - KK5: sorumluluk satırı.
  - KK6: başlıklar, ad, küme, imha satırı.
  - KK7: altı önek; dosya kalmaz.
  - KK8: iki dosyanın içeriği.
  - KK9: iz satırı sayısı.
  - KK10: üç 422 ve boş küme; 500'de iz yok.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T525: Ürün listesi ve ürün formu, görsel yükleme (E-32, E-33)
- **Bağımlılık:** T521, T540, T541, T542, T543, T544, T545
- **Dokümanlar:** çekirdek; `09` §3.4, §8, §16 (§16.2); `07 UC-87`, `UC-88`, `UC-114`…`UC-117`; `04` E-32, E-33; `03 §8.1`; `02 §3.6`, §3.9, §3.11; `10` KP-39, KP-40
- **Kabul kriterleri:**
  1. `GET /panel/urunler` ürünleri en yeni önce verir; arama taslak ve arşivi de bulur; fiyat aralığı ve stok durumu sunucunun hesabıdır (`07 UC-87`; K-737).
  2. `GET /panel/urunler/yeni` ve `/{iç anahtar}` formu verir; var olan form `Version`, gösterilen stok, ayrılmış adet ve yayın kapısı eksiklerini taşır (`07 UC-88`; K-653).
  3. `SaveProduct` ürünü, varyantları, galeriyi ve indirimi tek işlemde yazar; yeni ürün ve varyant Taslak doğar; açıklama kapalı biçim setiyle süzülür (`07 UC-114`).
  4. Stok ya da kontenjan gösterilen değere karşı kendi koşullu güncellemesiyle yazılır; fiyat değişikliği fiyat geçmişine girer; ikisi de işlem izine girmez (`07 UC-114`; K-980).
  5. UC-114 422: alan kuralı, yayın kapısını bozan kayıt, indirim sınırları, ayrılmış adedin altı, stok kodu çakışması, silinmiş görsel ya da kategori (`07 UC-114`; K-1171 (4)).
  6. UC-114 409: sürüm ya da gösterilen stok farklıdır; ret hiçbir şey yazmaz, sıra 404, 422, 409'dur (`07 UC-114`; K-800).
  7. `ChangeProductPublishStatus` ve `ChangeVariantPublishStatus` hedef durumu mutlak yazar; ilk yayın adresi sabitler, vitrinden kalkan kalemi sepetten çıkarır, iz yazar (`07 UC-115`).
  8. UC-115 422: yayın kapısı eksik, son Yayında varyant, yayındaki üründe dosyasız ya da fiyatsız varyantı yayına alma (`07 UC-115`).
  9. `DeleteProduct` ve `DeleteVariant` onaysızda açık sipariş sayısıyla 200 verir; onaylıda siler; yayındaki ürünün son Yayında varyantı 422'dir (`07 UC-116`; K-674).
  10. `UploadImage` kaydı yazmadan formu yeni görselle 200 döner; tür içerikten okunur; P-22, P-23, P-24 aşımı 422, gövde tavanı 413'tür (`07 UC-117`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: arama, süzgeç, fiyat aralığı.
  - KK2: yeni ve var olan form.
  - KK3: tek işlem; Taslak doğum.
  - KK4: koşullu stok; fiyat geçmişi.
  - KK5: 422 sebepleri ayrı testte.
  - KK6: 409 ve ret sırası.
  - KK7: mutlak durum, adres, sepet.
  - KK8: üç 422 sebebi.
  - KK9: onay hâli; son varyant.
  - KK10: üç ret; 413.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T526: Kategori ağacı ve kuponlar (E-34, E-35)
- **Bağımlılık:** T521, T540, T541, T543, T544, T545
- **Dokümanlar:** çekirdek; `09` §3.4, §8, §16 (§16.2); `07 UC-89`, `UC-90`, `UC-118`…`UC-122`; `04` E-34, E-35; `03 §8.1`; `02 §3.4`, §3.10; `10` KP-41, KP-43
- **Kabul kriterleri:**
  1. `GET /panel/kategoriler` en çok üç seviyeli ağacı aynı seviyede alfabetik verir; üçüncü seviyede "alt kategori ekle" yoktur; boş ağaç boş hâl satırıdır (`07 UC-89`; K-805).
  2. Düğümün ürün ve alt kategori sayısı ile yayında ürünü yoksa menüde görünmediği satırı sunucunun hesabıdır (`07 UC-89`; `02 §3.28.6`).
  3. `CreateCategory` ad ve üst kategoriyle açar; adres adından üretilip sabitlenir, çakışırsa sayı eklenir; ağacın danışma kilidiyle sıralanır (`07 UC-118`; K-1089).
  4. UC-118 422: ad boş, üst kategori üçüncü seviyede; 404: üst kategori yok; tekrar gönderim ikinci kategori açar (`07 UC-118`; K-1191).
  5. `RenameCategory` adı değiştirir, adres değişmez; boş ad 422, yok 404 (`07 UC-119`).
  6. `MoveCategory` sınırları kilidin altında yeniden denetler; üç seviyeyi aşan ya da kendi alt dalına giden taşıma 422, yok 404 (`07 UC-120`).
  7. `DeleteCategory` onaysız siler; içinde ürün ya da alt kategori varsa 422 engelleyenlerin sayısıyla ve taşımayı önerir; yok 404 (`07 UC-121`; K-799).
  8. `GET /panel/kuponlar` en yeni önce verir; satırın hâli tarihten ve kalan haktan okunur; kullanılmış ve ayrılmış hak salt okunurdur; yabancı anahtar 404 (`07 UC-90`; K-654).
  9. `SaveCoupon` yazar; değişiklik yalnız yeni siparişlere işler; bitiş öne çekilince yeni siparişte kupon kabul edilmez, ayrılmış haklar kalır; iz yazılmaz (`07 UC-122`; K-806).
  10. UC-122 422: boş ya da harf ayrımsız çakışan kod, sıfır değer, %100, asgari tutar kuralları, ters tarih, kullanımın altında adet; 409: sürüm (`07 UC-122`; K-799, K-800).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: sıra, seviye sınırı, boş ağaç.
  - KK2: sayılar ve menü satırı.
  - KK3: adres, çakışma, kilit.
  - KK4: iki ret, 404.
  - KK5: ad değişir, adres sabit.
  - KK6: iki taşıma reddi.
  - KK7: dolu kategori silinmez.
  - KK8: dört hâl; yabancı 404.
  - KK9: yeni siparişe işler; bitiş çekme.
  - KK10: 422 sebepleri ayrı testte; 409.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T527: Sipariş listesi, sipariş ayrıntısı ve durum geçişleri (E-36, E-37)
- **Bağımlılık:** T520, T540, T541, T542, T543, T544, T545
- **Dokümanlar:** çekirdek; `09` §3.4, §8, §9, §16 (§16.2); `07 UC-91`, `UC-92`, `UC-123`…`UC-128`; `04` E-36, E-37; `03 §8.2`; `02 §3.20.7`, §3.20.11; `10` KP-46, KD-13
- **Kabul kriterleri:**
  1. `GET /panel/siparisler` arama (numara ya da e-posta, tam eşleşme), sipariş ve ödeme durumu süzgeçleri ile bekleyen iş süzgecini birlikte uygular; geçersiz değer süzgeci uygulamaz (`07 UC-91`).
  2. Sıra en yeni önce, süreli süzgeçte kalanı en az olan önce, süresi aşılmış en başta; kendiliğinden iptal edilmiş ödenmemiş sipariş görünür (`07 UC-91`).
  3. `GET /panel/siparisler/{iç anahtar}` bölümleri, işaretleri ve açık işlemleri verir; birincil düğme hattın sıradaki adımıdır; yabancı anahtar 404 (`07 UC-92`; K-803).
  4. Kalem başına açık adet, süreler, kalanı ve bekleyen geri ödeme tutarı sunucunun hesabıdır; adres ve e-posta geçmişi ile iç not yalnız paneldedir (`07 UC-92`; K-788, K-1188 (3)).
  5. `MarkPaid` onaysızda donmuş IBAN ve tutarla 200 verir; onaylıda ayırmayı kesinleştirir, kuponu kullanılmış sayar, kalemleri sepetten çıkarır; açık değilse 422 (`07 UC-123`).
  6. `MarkShipped` ve `MarkReshipped` kargo şirketi ve takip numarasını ya da araç beyanını ister, kalem seçtirmez; yalnız açık fiziksel kalemleri gönderir; eksikse 422 (`07 UC-124`, `UC-125`; K-590).
  7. `MarkDelivered` teslim tarihini yazar; ileri ya da kargo gününden önceki tarih 422'dir; beyanla çakışan tarih sıra sorusu ister (`07 UC-126`; K-554, K-608).
  8. `MarkDeliveryFailed` açık kalem kalmadıysa siparişi Teslim edildi'ye geçirir; `CloseDeliveryFailed` iptal kaydı ve geri ödeme açmaz (`07 UC-127`, `UC-128`; K-717).
  9. Hepsi `Version` taşır; eşzamanlı ikinci gönderim sürüme takılır ve hiçbir şey yazmaz (`07 UC-123`…`UC-128`).
  10. Her geçiş işlem izine yazılır (`07 UC-123`…`UC-128`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: arama, üç süzgeç, geçersiz değer.
  - KK2: üç sıra kuralı.
  - KK3: birincil düğme; yabancı 404.
  - KK4: sunucu hesapları.
  - KK5: onay hâli; Ö1 yan etkileri; 422.
  - KK6: iki işleyici; eksik bilgi 422.
  - KK7: üç tarih durumu.
  - KK8: Teslim edildi'ye geçiş; kapatma.
  - KK9: eşzamanlı iki gönderim.
  - KK10: iz satırı.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T528: Sipariş ayrıntısı: firma iptali, bildirim kaydı, kalem çıkarma, iade malı ve ayıp talebi (E-37)
- **Bağımlılık:** T527, T529, T540, T541, T543, T544
- **Dokümanlar:** çekirdek; `09` §3.4, §8, §9, §16 (§16.2); `07 UC-129`, `UC-130`, `UC-138`, `UC-140`…`UC-142`, `UC-144`, `UC-145`; `04` E-37; `03 §5.3`, §8.3; `10` KP-47
- **Kabul kriterleri:**
  1. `CancelOrderByCompany` ödeme beklerken siparişin tamamını, ödemeden sonra kalem ve adedi sebeple iptal eder; onaysız ilk gönderim uyarılı onay hâlini 200 verir, onaylı 303 (`07 UC-129`; K-797).
  2. UC-129 422: sebep ya da "diğer"de açıklama yok, kalem seçilmemiş, adet geçersiz ya da açık adedi aşıyor, kalem iptale açık değil; istek bütünüyle uygulanmaz (`07 UC-129`).
  3. İptal ödenmemişte ayırmayı bırakır, kuponu geri verir; ödenmişte iptal kaydı yazar, adedi stoğa döndürür; kart iadesi kendiliğinden başlar, havalede IBAN istenir (`07 UC-129`).
  4. `RecordWithdrawal` kalem ve adetle cayma yazar; `RecordLateDeliveryTermination` kalem ve adet almaz, teslim edilmemiş fiziksel kalemlerin tamamına düşer (`07 UC-130`).
  5. UC-130 422: ileri ya da bir yıl sonrası tarih, IBAN hatası, gereken ayrı onay kutusu ya da sıra cevabı yok, Z-11 geçmemiş fesih (`07 UC-130`; K-709).
  6. `RemoveOrderItem` ödenmiş siparişte kalemi ya da adedi çıkarır; kart hattında onay ister, havalede istemez; dijital, ödenmemiş ve aralık dışı kalem 422'dir (`07 UC-138`).
  7. `ReceiveReturn` her teslim almayı ayrı kayıt yazar, geri ödemeyi başlatmaz, "mal dönmedi" kalemi yeniden açar; ileri ya da teslimden önceki tarih 422; `RejectReturn` yalnız koşullu istisna kaleminde ve onayla çalışır (`07 UC-140`).
  8. `RestockReturn` teslim alınan adedi stoğa ekler; ikinci eklemede ve ret kaydında 422'dir (`07 UC-141`).
  9. `CloseNotReturned` Z-42 geçince ulaşmamış adedi kapatır, geri ödeme yapmaz; Z-42 geçmemişse 422 (`07 UC-142`).
  10. `ResolveDefectClaim` Açık talebi Çözüldü, `ReopenDefectClaim` Z-18 içinde Açık yapar, yeni talep açmaz; aksi 422 (`07 UC-144`, `UC-145`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: dört uyarı, onay hâli.
  - KK2: dört 422 sebebi.
  - KK3: iki hat ve ayırma.
  - KK4: cayma ve fesih alanları.
  - KK5: altı 422 sebebi.
  - KK6: iki hat.
  - KK7: ayrı kayıt; ret koşulu.
  - KK8: ikinci ekleme.
  - KK9: Z-42 sınırı.
  - KK10: durum geçmişi.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T529: Sipariş ayrıntısı: geri ödeme işlemleri (E-37)
- **Bağımlılık:** T527, T540, T541, T543
- **Dokümanlar:** çekirdek; `09` §3.4, §8, §9, §11 (§11.1), §16 (§16.2); `07 UC-131`…`UC-137`; `04` E-37; `03 §5.2`, §5.3, §8.3; `02 §3.21.11`; `10` KP-47
- **Kabul kriterleri:**
  1. `ProcessRefund` geri ödemeyi doğuran kaydın anahtarını alır; onaysız ilk gönderim tutarı ve havalede IBAN'ı yineler (200), onaylı 303; yabancı ya da yok kayıt 404 (`07 UC-131`).
  2. Havalede geri ödeme işlemle gönderilmiş sayılır ve IBAN silinir; IBAN yoksa 422'dir (`07 UC-131`; K-686, K-1110).
  3. Kart hattında POST sağlayıcıyı çağırmaz; geri ödeme satırı, iş satırı, iz ve sürüm tek işlemde yazılır; sonuç sağlayıcının yanıtıyla işler (`07 UC-131`; `05 §4.6`; K-970).
  4. İkinci gönderim ve onay hâli açıkken müşterinin IBAN'ı düzeltmesi 409'dur, güncel hâl gösterilir (`07 UC-131`; K-1188 (2)).
  5. "Mal dönmedi" kapanışına bağlı tutar bazlı geri ödemeler sonraki teslim almalardan sırayla düşülür; hesap sunucunundur (`07 UC-131`; K-1105).
  6. `MarkBankTransferRefundFailed` IBAN'ı siler, alanı yeniden açar, kalemi "IBAN bekleniyor"a döndürür; işlenmiş geri ödemede 422 (`07 UC-132`; K-1131).
  7. `ProcessAmountBasedRefund` boş, sıfır ya da geri ödenmemiş tutarı aşan tutarı 422 reddeder; havalede IBAN yoksa 422; kapatılmış kalem kapalı kalır (`07 UC-133`; K-596, K-1078).
  8. `RequestRefundIban` sebebi sunucu yazar, yönetici IBAN girmez; kalem seçilmemiş ya da havale hattında değilse 422 (`07 UC-134`; K-1201).
  9. `RetryCardRefund` onayla yeni iade isteği yollar; sonuç beklenirken ya da IBAN girilmişse 422'dir (`07 UC-135`; K-940).
  10. `SwitchRefundToBankTransfer` `CardRefundFailed` sebepli IBAN isteği açar; `MarkCardRefundFailed` işareti çevirir, sorguyu durdurur, bildirim göndermez; sonradan gelen "gerçekleşti" işlenmez (`07 UC-136`, `UC-137`; K-978).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: yedi bağdan biri; 404.
  - KK2: havale hattı; IBAN yok.
  - KK3: kart hattı sırası, sonuç.
  - KK4: iki 409 sebebi.
  - KK5: düşümün sırası.
  - KK6: IBAN silinir; işlenmişte 422.
  - KK7: dört tutar durumu.
  - KK8: sebep sunucuda.
  - KK9: iki ret.
  - KK10: iki işlem; geç gelen sonuç.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T530: Sipariş ayrıntısı: adres, kargo, teslim tarihi, durum geçişi ve e-posta düzeltmeleri (E-37)
- **Bağımlılık:** T527, T540, T541, T543, T544
- **Dokümanlar:** çekirdek; `09` §3.4, §8, §16 (§16.2); `07 UC-146`…`UC-151`; `04` E-37; `03 §2.8`, §5.2, §6.2, §8.4; `02 §3.23.3`; `10` KP-48
- **Kabul kriterleri:**
  1. `CorrectAddress` teslimat ve fatura adresini ve telefonu düzeltir; eski ve yeni değer adres geçmişine yazılır, adres değeri ize girmez (`07 UC-146`; K-1136).
  2. UC-146 422: alan kuralı; Kargoya verildi'den sonra (fiziksel kalemsizde Teslim edildi'den sonra) işlem açık değildir (`07 UC-146`).
  3. `CorrectCarrier` takip bilgisini düzeltir; boş bilgi ya da kargoya verilmemiş sipariş 422'dir (`07 UC-147`).
  4. `CorrectDeliveryDate` cayma penceresini ve ayıp süresini düzeltilmiş tarihten işletir, caymanın teslimden önce mi sonra mı olduğunu yeniden ayırır; teslim işaretlenmemişse 422 (`07 UC-148`).
  5. `CorrectArrivalDate` Z-16'yı yeni tarihten işletir; ileri ya da teslimden önceki tarih 422'dir (`07 UC-149`; K-554).
  6. `CorrectTransition` düzeltilen geçişe bağlı yeni geçiş yazar; dönülecek durumu kayıttan okur; sebep ve "diğer"de açıklama ister (`07 UC-150`).
  7. UC-150 422: kart iadesi beklenirken yanlış iptal, yetmeyen stok ya da kontenjan, kaydı olan havale siparişinde "ödendi", kart hattında ödeme ekseni — engel mesajıyla (`07 UC-150`; K-703).
  8. Yanlış iptalin düzeltmesi ayırmaları yeniden yapar, başlatılmış geri ödemeyi ve IBAN isteğini kapatır; dış dünyaya çıkmış sonuç geri alınmaz (`07 UC-150`; K-1056).
  9. `CorrectOrderEmail` e-postayı ve erişim anahtarını yeniler, eski bağlantıları açmaz, hesap bağını yeni adrese göre değiştirir, gönderilmemiş bildirimlerin alıcısını yeni adres yapar (`07 UC-151`; K-1116, K-1138).
  10. UC-151: B-1 yeni adrese, B-15 eski adrese gider, e-posta geçmişine yazılır; üyeli ya da imha edilmiş sipariş ve geçersiz e-posta 422'dir (`07 UC-151`).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: geçmiş satırı; izde adres yok.
  - KK2: alan 422; iki sınır.
  - KK3: iki 422 sebebi.
  - KK4: pencere ve süre yeni tarihten.
  - KK5: Z-16; iki tarih reddi.
  - KK6: sebep zorunlu; geçiş kaydı.
  - KK7: dört engel ayrı testte.
  - KK8: ayırma ve geri ödeme kapanışı.
  - KK9: anahtar, hesap bağı, alıcı.
  - KK10: iki e-posta; üç 422.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T531: Sipariş ayrıntısı: hizmet kalemi, indirme hakkı, iç not ve e-posta yeniden gönderim (E-37)
- **Bağımlılık:** T527, T540, T541
- **Dokümanlar:** çekirdek; `09` §3.4, §8, §15 (§15.2), §16 (§16.2); `07 UC-139`, `UC-143`, `UC-152`…`UC-154`; `04` E-37; `03 §8.2`, §8.4, §8.9; `02 §3.12.6`, §3.20.9; `05` MK-31; `10` KP-44, KP-48
- **Kabul kriterleri:**
  1. `CompleteServiceItem` ödemesi onaylanmış hizmet kaleminin açık adetlerinin tamamını tamamlandı işaretler; cayma düşer, "sorun bildir" açılır; son açık kalemse kapanış kuralı işler (`07 UC-139`; K-792).
  2. UC-139: ödenmemiş sipariş ya da kapanmış kalem 422'dir; onay istenmez (`07 UC-139`).
  3. `ResetDownloadCount` dijital kalemin sayacını sıfırlar ve bildirim göndermez; dijital olmayan kalem 422'dir (`07 UC-143`; K-683).
  4. `SaveInternalNote` notu yazar, boşaltılan notu siler; siparişin sürümü artmaz; notun metni ize girmez (`07 UC-152`; K-1051).
  5. UC-152 409: notun o anki metni gösterilenden farklıdır, güncel not gösterilir; imha edilmiş siparişte alan kapalıdır, 422 (`07 UC-152`; K-1095).
  6. `ResendNotification` "ulaşmadı" işaretli bildirimin grubuna yeni "gönderilecek" satırı yazar ve gönderimi iş kuyruğuna bırakır; sonuç sonraki gösterimdedir (`07 UC-153`; K-1262).
  7. Sipariş e-postası donmuş sürümüyle gider; koşul sipariş kilidi altında okunur, eşzamanlı ikinci istek ikinci satır yazmaz (`07 UC-153`; K-1115).
  8. UC-153: satır işaretli değilse ya da grubunun son satırı değilse 422'dir; siparişin sürümü artmadığından 409 çıkmaz (`07 UC-153`).
  9. `ResendAllNotifications` işaretli her satır için tek işlemde satır yazar, hepsi ya da hiç; siparişler arası toplu gönderim yoktur (`07 UC-154`; K-761).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: tamamlama, cayma düşer, kapanış.
  - KK2: iki 422 sebebi.
  - KK3: sayaç sıfır; bildirim yok.
  - KK4: boşaltma siler; izde metin yok.
  - KK5: 409 ve imha edilmiş sipariş.
  - KK6: yeni satır ve sonuç.
  - KK7: donmuş sürüm; eşzamanlı iki istek.
  - KK8: ikinci gönderim 422.
  - KK9: hepsi ya da hiç.
  - `09 §25` ilgili satırlar; güvenlik taraması

#### T532: Talepler ve iletişim talebi ayrıntısı (E-38, E-39)
- **Bağımlılık:** T520, T540, T541, T542, T545
- **Dokümanlar:** çekirdek; `09` §3.4, §8, §16 (§16.2); `07 UC-93`, `UC-94`, `UC-155`, `UC-156`; `04` E-38, E-39; `03 §8.6`; `02 §3.32.4`, §3.32.5; `10` KP-49, KD-50, KD-51
- **Kabul kriterleri:**
  1. `GET /panel/talepler` iletişim ve ayıp taleplerini tek listede verir; tür ve durum süzgeçleri birden çok değer alır; arama ve konu tipi süzgeci yoktur (`07 UC-93`; K-751, K-809).
  2. Sıra açılış tarihine göre en yeni önce; yeniden açılan ayıp talebi son açılış tarihiyle sıralanır (`07 UC-93`).
  3. "Açık" süzgeciyle satır sayısı ana sayfanın açık talep sayacına eşittir; saklama süresi dolan talep listede yoktur (`07 UC-93`).
  4. İletişim satırı UC-94'e, ayıp satırı siparişin ayıp talepleri bölümüne gider; ikisi listenin adresini `returnUrl`'de taşır; ulaşmamış ayıp talebi işaret taşır (`07 UC-93`).
  5. Geçersiz süzgeç değeri süzgeci uygulamaz; boş sonuç boş hâl satırıdır (`07 UC-93`).
  6. `GET /panel/talepler/{iç anahtar}` konu tipini, durumu, göndereni, mesajı, ulaştığı anı, son kapatılışı ve yeniden açılmışta sürenin işlediği tarihi verir (`07 UC-94`; K-1099).
  7. Birincil düğme Açık talepte "Talebi kapat", Kapatıldı talepte "Yeniden aç"tır; "Siparişlerde ara" ve "Üye kaydında ara" talebin e-postasıyla aranmış açılır, adres kayıtlarda maskelenir (`07 UC-94`; K-810).
  8. Talep yoksa ya da saklama süresinin sonunda imha edilmişse 404 döner; iki işleyicide de aynıdır (`07 UC-94`, `UC-155`; K-1192).
  9. `CloseContactRequest` durumu Kapatıldı yapar, `LastClosedAt` yazar, açık talep sayacından düşer, bildirim göndermez; zaten Kapatıldı'daysa 303 verir (`07 UC-155`; K-810).
  10. `ReopenContactRequest` durumu Açık yapar, `LastClosedAt`'i değiştirmez: süre durmaz, talep açık dururken de sürenin sonunda imha edilir; zaten Açık'taysa 303 (`07 UC-156`; K-1099).
- **Test beklentisi:** Unit, Integration
- **Doğrulama kontrol listesi:**
  - KK1: iki tür, üç durum; arama yok.
  - KK2: iki sıra durumu.
  - KK3: sayaç eşitliği; süresi dolan yok.
  - KK4: iki bağlantı; işaret.
  - KK5: geçersiz değer; boş hâl.
  - KK6: tarihler; KVKK ulaşma anı.
  - KK7: iki düğme; iki arama; maske.
  - KK8: iki işleyicide 404.
  - KK9: kapatma; ikinci gönderim.
  - KK10: süre durmaz; ikinci gönderim.
  - `09 §25` ilgili satırlar; güvenlik taraması

## 6. Faz geçiş kapıları

### 6.1 Genel gate check (tüm fazlarda)

> Detaylı prosedür: `.claude/skills/gate-check/SKILL.md`. Burada faz-spesifik ek kontroller listelenir.

### 6.2 Faz-spesifik kontroller

| Faz | Ek kontrol |
|---|---|

## 7. Traceability Matrix (ÖNCE BU)

> **Ne yazılır:** Matris **çift yönlüdür** (00 §C.3). Tek yön yeterli değildir — ileri yön eksik işi, geri yön gereksiz işi yakalar.

### 7.1 İleri izlenebilirlik (kaynak öğe → task)

Tüm önceki doküman çıktıları (entity, endpoint, ekran, entegrasyon, iş kuralı) → task eşlemesi.
**Eşlenmeyen çıktı = eksik task.**

**Alt bölüm haritası (K-1412).** Matris yedi oturumda kurulur (K-1404); O1 haritayı sabitledi. Her alt bölüm `####` başlığıdır; yeni bölüm numarası açılmaz (K-1406 (2)(c)).

| Alt bölüm | İçerik | Kaynak ailesi (birim) | Oturum | Durum |
|---|---|---|---|---|
| 7.1.1 | Veri modeli → task | `06` (117: entity 79 · enum alt bölümü 6 · alt bölüm 32) | O1 | ✓ |
| 7.1.2 | API → task | `07` (208: uç 187 · alt bölüm 21) | O2 | ✓ |
| 7.1.3 | Entegrasyon → task | `08` (30: entegrasyon 3 · çağrısı olmayan bağlantı 1 · alt bölüm 26) | O1 | ✓ |
| 7.1.4 | Arayüz → task | `04` (102: ekran 53 · ortak bileşen 20 · alt bölüm 29) ve `03` (55 alt bölüm) | O3 | ✓ |
| 7.1.5 | İş kuralı → task — kalın etiketli beş grup: **`02`** · **`05`** · **`09`** · **`10`** · **Devir satırları** | `02 §1`–§3 (297) O4a; `02 §4`–§8 (193) O4b 1/2; `02 §9`–§13 (176) O4b 2/2; `05` (136) O5 1/2; `09` (25) · `10` (143) · devir (19) O5 | O4a, O4b, O5 | ✓ (`02` §1–§13 ✓ O4a, O4b; `05` ✓ O5 1/2; `09`, `10`, devir ✓ O5 2/2 — ileri yön 1.501/1.501) |
| 7.2, §8 | Geri izlenebilirlik, gerekçesiz eklemeler, GAP kararları | task → kaynak | O6 | ✓ (7.2 ve gerekçesiz ekleme taraması O6 1/2; okuma derinliği örneklemi, GAP-15…GAP-21 ve §8 O6 2/2 — **matris tamam**) |

**Kaynak kimliğinin biçimi (K-1412).** Satırın ilk hücresi kimlikle başlar; kimliğin ardından başlık ya da ad gelir (en çok 500 bayt/satır). Kapsama betiği yalnız kimliği okur.

| Aile | Kimlik | Örnek |
|---|---|---|
| `06` entity | `06:§4.<n> <Entity>` — `n` §3.1 tablosunun "§4" sütunundan; aynı alt bölümü paylaşan entity'ler adla ayrılır | `06:§4.17 Order` |
| `06` enum alt bölümü ve alt bölüm | `06:§<n.m> <başlık>` | `06:§5.2 Durum setleri ve geçiş kimlikleri` |
| `08` entegrasyon, çağrısı olmayan bağlantı, alt bölüm | `08:§<n.m[.k]> <başlık>` | `08:§2.1.4 …`, `08:§3.1 …` |
| `02` kural ve alt bölüm (O4a) | `02:§3.<n.m> <kural başlığı>` (kalın başlık cümlesi), `02:§<n.m> <başlık>` — `02 §1`–§3 | `02:§3.6.7 Stok ve hizmet kontenjanı…`, `02:§3.6 Stok ve tükenme` |
| `02` kimlikli satır (O4b 1/2) | `02:<Z-n \| L-n> <satır adı>` — `02 §4.2` (Z-1…Z-47) ve `§8.2` (L-1…L-9); `02 §4`–§8 kural ve alt bölümü O4a biçiminde | `02:Z-7 Kart ödeme süresi`, `02:L-5 Başarısız misafir sipariş takibi sorgusu` |
| `05` karar ve alt bölüm (O5 1/2) | `05:MK-<nn>` (§12 tablosu), `05:§<n.m>` | `05:MK-31`, `05:§6.1` |
| `09` bölüm (O5 2/2) | `09:§<n>` — `## n.` başlığı, n = 1…25 | `09:§10 Veri erişimi` |
| `10` satır ve alt bölüm (O5 2/2) | `10:KP-<n>` (§2), `10:KD-<n>` (§3, negatif denetim), `10:§4.1`, `10:§4.2` | `10:KP-15 …`, `10:KD-3 …` |
| Devir satırı (O5 2/2) | `devir:K-<n>` (karar), `devir:P8.<n>` (park maddesi), `devir:<doküman>-<satır>` (gövde anması), `devir:D-01` | `devir:K-1356 …`, `devir:04-1948 …` |

**Satır sütunları (K-1412).** **Kaynak öğe** (kimlik + ad) · **Task** (aday task kimliği; birden çok ise virgülle; task gerektirmeyen birim `—`) · **Implement edildi** (boş — gate'te dolar) · **Kanıt** (boş — gate'te dolar). Kapsama betiği son iki sütunun bu aşamada boş olduğunu da denetler.

**Task gerektirmeyen birim.** `08:§5.1` — `07`'ye geri yansıyan uçların kaydı: yeni uç çıkmamıştır ve üç mevcut ucun (UC-29, UC-30, UC-80) işi `07` satırlarının task'larındadır (O2: UC-29 ve UC-30 T510'da, UC-80 T311'de; ikisi de `08:§2.1.4` ve `08:§2.1.5` satırlarına eklendi). GAP değildir.

`03:§9.5` — Tercih yönetimi: bölüm tercih yönetiminin ürünün içinde karşılığı olmadığını yazar (işlem bildirimleri kapatılamaz, bildirim tercihi ekranı yoktur — `02 §9.1.3`); yapılacak iş çıkmaz, "Task" hücresi `—` (O3). GAP değildir.

`02:§1.1` — Sözlük kuralı ve `02:§1.2` — Terim sözlüğü: doküman yazım konvansiyonudur ("tek ad, İngilizce karşılık Aşama 1'de, `06`/`07`/`09` isim icat etmez"); kod adları `06` ve `07` satırlarındaki task'larda (T101–T120, T206–T217, T502–T539) ve her task'ın kendi kavramının adında yaşar, yapılacak ayrı iş çıkmaz — "Task" hücresi `—` (O4a). GAP değildir. `02:§3.26` — İptal, cayma, iade ve ayıp talebi: bölüm kuralların `§7`'de olduğunu söyleyen bir işarettir, kendi kuralı yoktur; kuralları `02 §7` satırlarında eşlenir (O4b) — "Task" hücresi `—` (O4a). GAP değildir.

Kaynak (O4b 2/2): `02 §9`–§13 (176: iş kuralı 80 `02:§n.m.k` · kimlikli satır 75 — B-1…B-16, F-1…F-6, P-1…P-49 ve H-1…H-4 · alt bölüm 21 `02:§n.m`; §9 37 · §10 50 · §11 56 · §12 33 · §13 0). `§13` tablosu boştur ve numaralı kural taşımaz (`§13.1`–§13.4 iki basamaklıdır, K-1404'ün kalıp dışı içerik kuralı): açık karar yoktur. `§11`'in P satırları parametre kimliğidir: her satır T381'e (ayar katmanları ve envanter) ve "Nerede kullanılır" sütununun işaret ettiği kuralların task'larına (en çok beş) bağlanır; `P-2` ve `P-11` "Kaldırıldı" satırlarıdır. `§12.5` firma yükümlülükleri tablosunun satırları birim değildir; alt bölüm satırı onların task birleşimini taşır. Alt bölüm satırı altındaki birimlerin task birleşimidir (`§9.4`, `§9.5` ve `§12.5` kural taşımadığı için açık yazılır).

Kaynak (O5 1/2): `05` (136: mimari karar 78 `05:MK-nn` · `###` alt bölümü 58 `05:§n.m`; §3 8 · §4 7 · §5 4 · §6 10 · §7 10 · §8 9 · §9 4 · §10 3 · §11 3). `### 6.x Secret yönetimi` başlığı kimlikte `05:§6 6.x Secret yönetimi` diye yazılır (betik `05:§6` okur). Bir mimari karar birden çok task alır; alt bölüm satırı o bölümün task birleşimini taşır (K-1431). Yeni aday 23: F0 T02–T09 (iskele), F2 T218–T229 (ortak altyapı), F3 T387–T388 (yönetim komutları), F6 T602 (güncelleme komutu) — K-1432 (⚠). **Task gerektirmeyen dört birim (GAP değil):** `05:MK-77` (H-3, H-4 hacim kestirimleri; sistem hiçbirini uygulamaz — `02 §11.3`), `05:MK-78` (büyüme yolunun sırası ve tetikleyicileri — bugün alınan kararların açtığı kapılar), `05:§11.2` (bilinçli tekil bırakılanlar) ve `05:§11.3` (büyüme yolu) — "Task" hücresi `—` (O5 1/2).

Kaynak (O5 2/2): `09` (25: `## n.` bölümü), `10` (143: KP satırı 77 · KD satırı 64 · alt bölüm 2 — `§4.1`, `§4.2`) ve devir satırları (19: karar 9 · park maddesi 5 · gövde anması 4 · D-01 1; `11_devir_esle.py` A–F). **`09` bölümü task üretmez; task'ın "Dokümanlar" alanının kaynağıdır** (K-1404): her satırın "Task" hücresi, bölümün kurallarını koda yazan ya da denetleyen task'lardır. **Çekirdek yedi bölüm** (`§1`, `§2`, `§5`, `§6`, `§23`, `§24`, `§25`) her task'ın "Dokümanlar" alanında "çekirdek" diye anılır ve ayrıca sayılmaz (park bloğu K-1302 (4), K-1357); `§5`, `§6` ve `§23`'ün kuralları ayrıca kod ya da denetim işi taşıdığından ilgili task'a da bağlıdır. **`KD` satırı negatif denetimdir:** task açılmaz; satır, "yoktur" kararının kabul kriterinde ya da testinde korunduğu kapsamdaki task'a bağlanır. **Yeni aday 3:** F2 `T230` (kayıt yazıcıları ve biçimleyici), F2 `T231` (maskeleme testleri), F5 `T548` (parçalı yükleme betiği) — park bloğunun üç yapım işi (K-1354, K-1391; kararın biçimi K-1406 (6)). **Task gerektirmeyen birim (GAP değil):** `09:§1`, `§2`, `§22`, `§24`, `§25` — yazım ve süreç kuralı; her task'ın "Dokümanlar" alanı ve kabul kriteri `§5`'in task yapısındadır (`IP5-03`), gate ve doğrulama skill'lerinin işidir; ayrı iş çıkmaz. Devirden `—`: `K-423`, `K-424` (yol haritası adayları, `§9` parkı ve `10 §5`; iş bırakmaz), `K-1302`, `K-1357`, `P8.1`, `09-117`, `09-2249` (okunacak `09` bölümleri — `§5`'in "Dokümanlar" alanı biçimi, `IP5-03`), `12-177` (kabul kriterleri ve kontrol listesi `§5` task yapısında tanımlıdır). `04-1948` (GA-7) ★: yasal metinlerin içeriği `02 §3.24.3`, §7.4.4, §12.2.5, §3.33.2 satırlarının task'larına (T356, T366, T373) bağlıdır — metinlerin yazımı `02 §3.33.1`–§3.33.2'de yazılı olduğundan soru açılmaz (K-1423 (3), K-1430 (3)). `D-01` F0'ın yedi task'ına dağılır (T02–T07, T09). `05:§9.1` satırına T230 ve T231 eklendi.

**Task gerektirmeyen birimler (O4b 2/2, GAP değil).** (1) `02:P-2` ve `02:P-11` — "Kaldırıldı" satırları: kimlik yeniden kullanılmaz, uygulanacak değer yoktur. (2) `02:H-1`, `02:H-3`, `02:H-4` — hacim kabulleri parametre değildir ve "sistem hiçbirini uygulamaz" (§11.3): tasarım tabanıdır; H-3'ün yanıt süresi ve H-4 `05`'in tasarım girdisidir; H-2 ise satış özetinin günlük sayısını okuyan tetikleyiciye dayandığı için T383'e bağlıdır. (3) `02:§10.5.5` — hacim varsayımı, "bir hedef değil, tasarım varsayımıdır". (4) `02:§12.1.12` ve `02:§12.2.11` — ürün ne satıldığını ve kurumsal içerikte yayımlanan üçüncü kişi verisinin iznini denetlemez; yükümlülük firmanındır (§12.5), yazılacak kod yoktur (Z-25 ve Z-37'nin kalıbı, K-1426 (3)). Bu sekiz birimin "Task" hücresi `—`.

**Aday task kimliği (K-1412).** "Task" sütunu aday kimliktir; adı ve gövdesi `§5`'te yazılır. Kurallar: (1) kimlik K-1407'nin faz aralığından gelir ve aşağıdaki aday tablosunda tanımlıdır (kapsama betiği denetler). (2) F1'in kimlikleri baştan ayrıldı; öteki fazlarda her matris oturumu kendi satırlarının gerek duyduğu kimliği sayaçtaki sonraki boş numaradan alır ve tabloya ekler. (3) Kimlikler faz içi sıra gözetmeden verilebilir; sıra `§5`'in işidir, araya `TXXa` girer (`00 §J`); gerekirse O6 betikli tek bir yeniden numaralama yapar ve kapsama betiği öncesini ve sonrasını koşar. (4) Var olan satıra aday kimlik eklenebilir (ör. UC-80'in F3 task'ı `08:§2.1.5` satırına girer); satır silinmez. (5) F1'in sırası modül akış sırasını (K-1410) bağların kurduğu sırayla daraltır: bir göç, bağlandığı tabloların göçünden sonra gelir (`06 §7.2`'nin 130 bağı betikle sınandı — sıra ihlali 0); Yönetim ve Üyelik ile iptal–cayma–iade ve Ödeme bu yüzden akış sırasından ayrılır.

| Faz | Son ayrılan | Sonraki boş |
|---|---|---|
| F0 | T10 | T11 |
| F1 | T120 | T121 |
| F2 | T233 | T234 |
| F3 | T390 | T391 |
| F4 | T410 | T411 |
| F5 | T549 | T550 |
| F6 | T602 | T603 |

**Aday task tablosu (O1).** F1 eksiksiz ayrıldı (79 entity, en çok 8 entity/göç — K-1409; sıra yukarıdaki (5)); öteki fazlarda yalnız `06`, `07` ve `08` satırlarının gerek duyduğu task'lar vardır (O2 55 aday ekledi: F0 1, F2 12, F3 4, F5 38; toplam 102; O3 23 aday ekledi: F3 15 `T315`–`T329`, F5 8 `T540`–`T547`; toplam 125; O4a 37 aday ekledi: F3 `T330`–`T366`, `02 §3`'ün iş kuralı task'ları; toplam 162; O4b 1/2 12 aday ekledi: F3 `T367`–`T378`, `02 §4`–§8'in iş kuralı task'ları; toplam 174; O4b 2/2 8 aday ekledi: F3 `T379`–`T386`, `02 §9`–§12'nin iş kuralı task'ları; toplam 182); O5 1/2 23 aday ekledi: F0 `T02`–`T09`, F2 `T218`–`T229`, F3 `T387`–`T388`, F6 `T602`, `05`in mimari kararları; toplam 205); O5 2/2 3 aday ekledi: F2 `T230`, `T231`, F5 `T548`, `09`'un park bloğunun üç yapım işi; toplam 208); O6 1/2 1 aday ekledi: F5 `T549`, K-1409 ekran sınırını aşan T503'ün E-05 payı (K-1437); toplam 209; O6 2/2 5 aday ekledi: F0 `T10`, F2 `T232`, `T233`, F3 `T389`, `T390`, okuma derinliği örnekleminin genişlemesinden çıkan sahipsiz işler (K-1441); toplam 214. Sayaçlar: F0 T11 · F2 T234 · F3 T391 · F4 T411 · F5 T550 · F6 T603).

| Aday | Faz | Kısa ad | Modül | Kaynak birimi |
|---|---|---|---|---|
| T01 | F0 | Canlılık ucu: uygulama ve veritabanı yanıtı, 200 ya da 503, gövdesiz | Ortak altyapı | `07` UC-81 (1) |
| T02 | F0 | Çözüm ve proje düzeni: C#/.NET 10 tek uygulama, Razor Pages, düz JavaScript ve CSS (npm hattı yok), yığının kuruluma yansıması (D-01) | Ortak altyapı | `05 §2.1`–§2.3, §3.1, §3.8 (MK-01…MK-05) |
| T03 | F0 | Test altyapısı: birim, mimari ve gerçek veritabanıyla entegrasyon testi projeleri | Ortak altyapı | `05 §8.3`, §3.3 (mimari test) (MK-15, MK-70) |
| T04 | F0 | CI hattı ve eşitlik denetimleri: Lint, `publish.yml` denetimleri, sürüm etiketi, göç adımının önceki imajla sınaması | Ortak altyapı | `05 §8.3`, §8.7.4 (MK-12, MK-70) |
| T05 | F0 | Compose geliştirme ortamı: dört konteyner, ayrı iç ağlar, ters vekil, güvenlik duvarı ve açık kapılar | Ortak altyapı | `05 §3.2`, §8.1, §6.8 (MK-06, MK-08, MK-10, MK-14, MK-68, MK-76) |
| T06 | F0 | Üç imaj ve kayıt defteri: uygulama, pgBackRest'li veritabanı ve yedek aracı; özetle sabitleme, tek eser sürüm | Ortak altyapı | `05 §8.2`, §3.2 (MK-02, MK-09…MK-11, MK-69) |
| T07 | F0 | Yedek düzeni: sürekli günlük aktarımı, gecelik tam ve dosya yedeği, iki anahtarla şifreleme, saklama ve silmeye karşı koruma | Ortak altyapı | `05 §7.6.1`–§7.6.6 (MK-09, MK-25, MK-26) |
| T08 | F0 | Sır düzeni: bileşen başına dosya sırları, paylaşılan sır yok, döndürme | Ortak altyapı | `05 §6.x` (MK-59, MK-60) |
| T09 | F0 | İlk yayın ve referans kuruluma ilk iniş: boş uygulama imajı canlılık ucuna yanıt verir | Ortak altyapı | `05 §8.1`, §8.2 (MK-68) · K-1407 (4) |
| T101 | F1 | Veri modeli temeli: anahtarlar, türler, adlandırma, sürüm sütunu, metin karşılaştırma, enum biçimi, bağların silme davranışı | Ortak | `06 §2.1`–§2.6, §2.8–§2.10, §5.1 (10) |
| T102 | F1 | Enum tanımları ve değer dönüşümü | Ortak | `06 §5.2`–§5.6 (5) |
| T103 | F1 | Başlangıç listeleri, firma ve yasal metin sürümü: Province, Country, Company, CompanySettings, CompanyDeliveryProvince, LegalTextVersion | Ayarlar | 6 entity |
| T104 | F1 | Ürün çekirdeği ve görsel: Image, Product, Option, OptionValue, Variant, Category, ProductCategory, ProductImage | Katalog | 8 entity |
| T105 | F1 | Fiyat, kupon ve dijital dosya: PriceHistory, Discount, Coupon, DigitalFile | Katalog | 4 entity |
| T106 | F1 | Yönetici hesabı, davet ve işlem izi: Admin, AdminInvitation, AuditLog | Yönetim ve işlem izi | 3 entity |
| T107 | F1 | Müşteri hesabı: Customer, AddressBook, PendingEmailChange | Üyelik | 3 entity |
| T108 | F1 | Oturum, bağlantı, deneme ve giriş kayıtları: Session, LinkRecord, AttemptRecord, RecognizedBrowserMark, LoginRecord | Ortak altyapı | 5 entity |
| T109 | F1 | Sepet ve sipariş çekirdeği: Cart, CartItem, Order, OrderItem, TransitionRecord, InternalNote, CheckboxAcceptance, CheckboxAcceptanceOrderItem | Sepet ve sipariş | 8 entity |
| T110 | F1 | Ayırma ve düzeltme geçmişleri: StockReservation, OrderEmailHistory, OrderAddressHistory | Sepet ve sipariş | 3 entity |
| T111 | F1 | Kalem kayıtları: RefundIban, DefectClaim, Cancellation, ItemRemoval, LateDeliveryTermination, Withdrawal, Return, NotReturnedClosure | İptal, cayma ve iade | 8 entity |
| T112 | F1 | Ödeme ve geri ödeme kayıtları: PaymentRecord, Refund, RefundRecord, InboundNotificationRecord | Ödeme | 4 entity |
| T113 | F1 | İade reddi ve IBAN isteği: ReturnRejection, RefundIbanRequest | İptal, cayma ve iade | 2 entity |
| T114 | F1 | İçerik kayıtları ve marka: AboutPage, Offering, PortfolioItem, FaqItem, CustomPage, Announcement, SocialLink, Branding | Kurumsal içerik | 8 entity |
| T115 | F1 | Şube, içerik–ürün bağları ve iletişim talebi: Branch, OfferingProduct, PortfolioItemProduct, ContactRequest | Kurumsal içerik | 4 entity |
| T116 | F1 | Galeri bağları: AboutPageImage, OfferingImage, PortfolioItemImage, FaqItemImage, CustomPageImage | Kurumsal içerik | 5 entity |
| T117 | F1 | Gönderim kaydı ve kanal hâli: DispatchRecord, ChannelStateRecord | Bildirim | 2 entity |
| T118 | F1 | Zamanlayıcı, yedek, imha ve anahtar kayıtları: Job, HeartbeatRecord, OutageRecord, BackupStatusRecord, DataProtectionKey, DisposalRecord | Ortak altyapı | 6 entity |
| T119 | F1 | Başlangıç verisi göçü ve seed olmayanların sınırı | Ortak | `06 §8.1`, §8.4 (2) |
| T120 | F1 | Model eşitlik denetimleri: ilişki diyagramı, tekillik, yabancı anahtar, kontrol kısıtı, dizin, denormalizasyon envanteri | Ortak | `06 §2.7`, §6.1–§6.6, §7.1–§7.6 (13) |
| T201 | F2 | İmha düzenleyicisi, süre ayarları ve gecelik temizlik | Ortak altyapı | `06 §9.1`, §9.4, §9.5 (3) |
| T202 | F2 | Dış servislerin bağlantı katmanı: ortak yüz, hata sınıfı, zaman aşımı, sahte sağlayıcılar, geliştirme e-posta yakalayıcı ağ geçidi (`SmtpCaptureEmailGateway`) | Ortak altyapı | `08 §3.1`; `09 §4.3`, §19.1 (K-1441) |
| T203 | F2 | İmzalı makine ucu altyapısı: imza denetimi, gövde tavanı | Ortak altyapı | `08 §3.2` |
| T204 | F2 | Dış servis çökmesi davranışı ve sahtelerle sınama | Ortak altyapı | `08 §3.3` |
| T205 | F2 | Açılışta veritabanı fail-fast denetimi | Ortak altyapı | `06 §8.2` |
| T206 | F2 | Adres yapısı, önekler ve sayfa–form işleyici kalıbı: gönder–yönlendir–göster, statik dosya sürüm izi | Ortak altyapı | `07 §2.1`, `§2.2`, `§2.10` (3) |
| T207 | F2 | Kimlik şemaları ve erişim reddi: panel ve müşteri şeması, oturumsuz geliş, sahiplik ve erişim anahtarlı kök | Ortak altyapı | `07 §2.3` (1) |
| T208 | F2 | Sürümün ve gösterilen değerin formda taşınması, koşullu güncelleme | Ortak altyapı | `07 §2.4` (1) |
| T209 | F2 | Tek kullanımlık değerler, tekrar gönderim, sahteciliğe karşı koruma ve güvenlik başlıkları | Ortak altyapı | `07 §2.5` (1) |
| T210 | F2 | Liste kalıbı: sayfalama, süzgeç parametreleri, geçersiz değer, arama motorlarına kapalı sayfa | Ortak altyapı | `07 §2.6` (1) |
| T211 | F2 | Model bağlama kalıbı: form girdi, sorgu ve makine ucu modeli türleri | Ortak altyapı | `07 §5.1` (1) |
| T212 | F2 | Hata gösterimi ve HTTP durumları: sayfa, form ve makine ucu; özel hâller | Ortak altyapı | `07 §2.7`, `§6.1`, `§6.4` (3) |
| T213 | F2 | Hata kodu kayıt defteri | Ortak altyapı | `07 §6.2` (1) |
| T214 | F2 | Kısa süreli bildirim kodları | Ortak altyapı | `07 §6.3` (1) |
| T215 | F2 | Hız sınırı, deneme limitleri ve gövde tavanı | Ortak altyapı | `07 §2.8` (1) |
| T216 | F2 | Tarih, sayı ve para biçimi: istek ve gösterim | Ortak altyapı | `07 §2.9` (1) |
| T217 | F2 | Yapılabilir işlemlerin sunucuda üretimi ve POST işleyicisinde yeniden denetim | Ortak altyapı | `07 §8.1` (1) |
| T218 | F2 | Dokuz modülün iskeleti ve Contracts: açık arayüz, eşzamanlı ve aynı işlemde erişim, mimari testle sınır | Ortak altyapı | `05 §3.3`, §3.4 (MK-13, MK-15) |
| T219 | F2 | Arama motoru verileri kalıbı: adres üretimi, JSON-LD, Open Graph, kapalı sayfada `noindex` | Ortak altyapı | `05 §3.7` (MK-18) |
| T220 | F2 | İş kuyruğu çalıştırıcısı: iş satırı, sıralı işleme, beş yeniden deneme, başarısız durum, geçişle aynı işlemde iç yan etki | Ortak altyapı | `05 §4.3`, §5.1, §5.2 (MK-37, MK-42) |
| T221 | F2 | Zamanlayıcı: dakikalık işler, nabız döngüsü, gecelik iş, işin zamanı kendi kaydında, kesinti tespiti ve dönüş sırası | Ortak altyapı | `05 §5.1`–§5.4 (MK-41, MK-43, MK-44) |
| T222 | F2 | Anahtarlar ve tek kullanımlık bağlantılar: kriptografik üreteç, türetme sırrı, özetli bağlantı kaydı, açılışın işlem yapmaması, açılışta bekleyen geri alma bildiriminin özetinin yeniden yazımı | Ortak altyapı | `05 §6.4` (MK-50, MK-51); `05 §6.x` (K-1137; K-1441) |
| T223 | F2 | Dosya deposu: arayüz, görselin yeniden kodlanması, WebP türevleri, yetim görsel imhası, yüklenen dosya sınırları | Ortak altyapı | `05 §6.6`, §7.7.1, §7.7.2 (MK-28, MK-29, MK-54) |
| T224 | F2 | Çerez politikası: birinci taraf, `__Host-` önekli çerezler, ödeme adımı çerezi, çerez başlığı ara katmanı | Ortak altyapı | `05 §6.7` (MK-55…MK-57) |
| T225 | F2 | Sistem kayıtları: kaynaklar, istek kimliği, maskeleme, saklama süresi ve imha kaydı satırı | Ortak altyapı | `05 §9.1`, §9.2 (MK-64, MK-65) |
| T226 | F2 | Ayar katmanları, tasarım değerleri ve açılışta ayar denetimi | Ortak altyapı | `05 §8.4`, §11.1 (MK-71) |
| T227 | F2 | Zaman ve para: UTC saklama, Türkiye saatiyle sayım, tek yuvarlama işlevi | Ortak altyapı | `05 §7.2` (MK-19) |
| T228 | F2 | Eşzamanlılık denetimi: kayıt sürümü, sipariş sürümü ve satır kilidi, kilit sırası | Ortak altyapı | `05 §7.3` (MK-20, MK-21) |
| T229 | F2 | Uygulama belleğinde önbellek: kapsam ve geçersiz kılma | Ortak altyapı | `05 §7.10` (MK-34) |
| T230 | F2 | Uygulama kayıtlarının üç yazıcısı ve satır biçimleyicisi: alan adları ve kapalı olay listesi (`LogEvents`) | Ortak altyapı | `09 §18.1`, §18.2; K-1354, K-1391 |
| T231 | F2 | Maskelemenin üç katmanının testleri: yazım, yol maskeleyici, biçimleyici ve istisna maskeleyici | Ortak altyapı | `09 §18.2`; K-1354, K-1391 |
| T301 | F3 | Siparişin imha parçası | Sepet ve sipariş | `06 §9.2` |
| T302 | F3 | Öteki kayıtların süreyle imhası | Modüller | `06 §9.3` |
| T303 | F3 | İlk yönetici yönetim komutu ve komut çatısı (`CommandHost`, dağıtıcı, terminalden okuma, komut kaydı) | Yönetim ve işlem izi | `06 §8.3`; `05 §8.5` (8.5.1, 8.5.7); `09 §13.2` (K-1441) |
| T304 | F3 | Alıcı başına bildirim gönderim sınırı | Bildirim | `08 §2.2.4` |
| T305 | F3 | E-posta ortak kalıbı ve gövde üretimi | Bildirim | `08 §2.2.5` |
| T306 | F3 | Müşteri bildirimleri B-1…B-8 | Bildirim | `08 §2.2.6` (yarısı) |
| T307 | F3 | Müşteri bildirimleri B-9…B-16 | Bildirim | `08 §2.2.6` (yarısı) |
| T308 | F3 | Firma bildirimleri F-1…F-6 | Bildirim | `08 §2.2.7` |
| T309 | F3 | Hesap ve yönetim e-postaları | Bildirim | `08 §2.2.8` |
| T310 | F3 | Kargo şirketi listesi ve takip bağlantısı kalıpları | Sepet ve sipariş | `08 §2.4` |
| T311 | F3 | Ödeme sağlayıcısının bildirim ucu: imza, alan eşlemesi ve durum makinesine aktarım | Ödeme | `07` UC-80 (1) |
| T312 | F3 | Dosya uçları: imzalı indirme ucu ve parçalı yükleme ucu | Katalog | `07` UC-37, UC-83 (2) · `07 §5.4` (1) |
| T313 | F3 | Site haritası ve robots.txt | Ortak altyapı | `07` UC-79 (1) |
| T314 | F3 | Durum ucu: uyarı koşulları ve 500 sayacı | Ortak altyapı | `07` UC-82 (1) |
| T315 | F3 | Durum makinesi çekirdeği: envanter, geçiş tablosu ve kimlikleri, durum makinesi olmayan varlıkların sınırı | Sepet ve sipariş | `03 §1.1`, §1.10 (2) |
| T316 | F3 | Sipariş durumu geçişleri: sevkiyat ekseni | Sepet ve sipariş | `03 §1.2` |
| T317 | F3 | Ödeme durumu geçişleri: ödeme ekseni | Ödeme | `03 §1.3` |
| T318 | F3 | İki eksenin bağı, tipe göre hatlar ve manuel adım bütçesi | Sepet ve sipariş | `03 §1.4`, §1.5, §8.5 (3) |
| T319 | F3 | Geri alınamaz geçişler, onay gerekleri ve durum düzeltmesi | Sepet ve sipariş | `03 §1.6` · `02 §10.4.6`, §10.4.8 (2) |
| T320 | F3 | Kalem kayıtları, ayıp talebi durumu, iade reddi ve panel işaretleri | İptal, cayma ve iade | `03 §1.7`, §1.9, §5.3 (3) |
| T321 | F3 | Ürün yayın durumları | Katalog | `03 §1.8` |
| T322 | F3 | İçerik yayın durumları ve iletişim talebi durumları | Kurumsal içerik | `03 §1.8`, §1.9 |
| T323 | F3 | Durum × rol × işlem tablosu | Sepet ve sipariş | `03 §1.11` |
| T324 | F3 | Zaman aşımı akışları: hesap, oturum, sepet ve ödeme süreleri | Ortak altyapı | `03 §4.1` (Z-1…Z-47'nin ilk iki grubu) |
| T325 | F3 | Zaman aşımı akışları: teslim, cayma, geri ödeme ve ayıp talebi süreleri | Ortak altyapı | `03 §4.1` (üçüncü ve dördüncü grup) |
| T326 | F3 | Zaman aşımı akışları: katalog, kupon, içerik, iletişim ve bildirim süreleri | Ortak altyapı | `03 §4.1` (beşinci ve altıncı grup; yedinci grup saklama ve imha T201, T301, T302'dedir) |
| T327 | F3 | Kendiliğinden işleyen anlar ve sitenin kesintisinde süreler | Sepet ve sipariş | `03 §4.2`, §10.3 (2) |
| T328 | F3 | Bildirim haritası: tetikleyici eşlemesi, bildirim üretmeyen olaylar ve eşitlik denetimi | Bildirim | `03 §7.1`–§7.3 (3) |
| T329 | F3 | İlk uçtan uca akış testi: ürün, sepet, sipariş, sahte ödeme sonucu, kargo, teslim (servis düzeyi) | Sepet ve sipariş | `03 §2.10` (K-1410) |
| T330 | F3 | Firma kimliği: tek firma, tip ve tipe göre zorunlu alanlar, yasal kimlik bilgilerinin erişilebilirliği, iade adresi | Ayarlar | `02 §3.1.1`–§3.1.4, §3.1.7 (5) |
| T331 | F3 | Satış kapısı: dört koşul, veri toplayan girişlerin kapısı, satışın geçici kapatılması | Ayarlar | `02 §3.1.5`, §3.1.6 (2) |
| T332 | F3 | Satış modeli, ürün tipleri, ürün–varyant–seçenek yapısı ve stok kodu | Katalog | `02 §3.2.1`–§3.3.6 (9) |
| T333 | F3 | Kategori ağacı, site içi arama ve liste süzgeçleri | Katalog | `02 §3.4.1`–§3.5.3 (9) |
| T334 | F3 | Stok ve hizmet kontenjanı: takip, tükenme, ayrılmış adedin altına inmeme | Katalog | `02 §3.6.1`–§3.6.7 (7) |
| T335 | F3 | Ürün yayın durumu: yayın kapısı, arşiv, silme ve engelleme kalıbı | Katalog | `02 §3.7.1`–§3.7.8 (8) |
| T336 | F3 | Fiyat ve KDV: kuruş hassasiyeti, kalem bazlı hesap sırası, yuvarlama yönü, fiyat etiketi bilgileri | Katalog | `02 §3.8.1`–§3.8.5 (5) |
| T337 | F3 | İndirim ve referans fiyat: tarihli indirim, fiyat geçmişinden referans, kaydı engelleyen koşullar | Katalog | `02 §3.9.1`–§3.9.6 (6) |
| T338 | F3 | Kupon: tanım, kalem payı, sınırlar, kullanım hakkının ayrılması | Katalog | `02 §3.10.1`–§3.10.7 (7) |
| T339 | F3 | Ürün görseli ve metni: ana görsel, kısıtlar, alternatif metin, metin biçimi seti | Katalog | `02 §3.11.1`–§3.11.7 (7) |
| T340 | F3 | Dijital ürün: dosya, indirme hakkı, hak yenileme, yeniden satın alma | Katalog | `02 §3.12.1`–§3.12.9 (9) |
| T341 | F3 | Üyelik: kayıt, e-posta doğrulama kapısı, misafir siparişinin hesaba bağlanması, hesap adı, giriş kaydı | Üyelik | `02 §3.13.1`–§3.13.5, §3.13.18–§3.13.21 (9) |
| T342 | F3 | Giriş ve oturum: oturum ömrü, şifre politikası, sıfırlama, yeniden doğrulama, yönetici hesaplarına aynı rejim, giriş limitleri | Üyelik | `02 §3.13.9`–§3.13.13, §3.13.15–§3.13.17 (8) |
| T343 | F3 | Google ile giriş ve hesap bağlama, e-posta değişikliği ve geri alma | Üyelik | `02 §3.13.6`–§3.13.8, §3.13.14 (4) |
| T344 | F3 | Adres: adres defteri, fatura adresi, kapalı il listesi, tipe göre istenen adresler, siparişe donma | Üyelik | `02 §3.14.1`–§3.14.6 (6) |
| T345 | F3 | Hesabın silinmesi ve kişisel veri başvurusu | Üyelik | `02 §3.15.1`–§3.15.6 (6) |
| T346 | F3 | Sepet: sahiplik, giriş anında birleşme, canlı kalem, siparişe girmeyen kalemler | Sepet ve sipariş | `02 §3.16.1`–§3.16.7 (7) |
| T347 | F3 | Sepet adet kuralları: stok ve sipariş başına sınır, boşalma, istek listesi yokluğu | Sepet ve sipariş | `02 §3.16.8`–§3.16.13 (6) |
| T348 | F3 | Sipariş onayı: oluşma anı, özet eşitliği, stok ayırma, ödeme süresi, kendiliğinden iptal | Sepet ve sipariş | `02 §3.17.1`–§3.17.8 (8) |
| T349 | F3 | Asgari sipariş tutarı, kargo ücreti ve ücretsiz kargo eşiği | Sepet ve sipariş | `02 §3.18.1`–§3.19.6 (9) |
| T350 | F3 | Teslim: il kısıtı, tek yol, kargoya verme süresi ve sözü, kargoya verildi işareti, kargo şirketi | Sepet ve sipariş | `02 §3.20.1`–§3.20.8 (8) |
| T351 | F3 | Teslim işaretleri: kalem tipine göre teslim, karışık sipariş, fiziksel teslim tarihi | Sepet ve sipariş | `02 §3.20.9`–§3.20.12 (4) |
| T352 | F3 | Ödeme: satıcı ve kart verisi, iki yöntem, 3D Secure, taksit, ters ibraz | Ödeme | `02 §3.21.1`–§3.21.4, §3.21.9–§3.21.11 (7) |
| T353 | F3 | Havale/EFT: IBAN ve siparişe donması, ödendi işareti, hatırlatma, süresi dolmuş sipariş | Ödeme | `02 §3.21.5`–§3.21.8 (4) |
| T354 | F3 | Sipariş numarası ve takip erişimi: numara, üç giriş yolu, erişim anahtarı | Sepet ve sipariş | `02 §3.22.1`–§3.22.5 (5) |
| T355 | F3 | Sipariş anında donan değerler | Sepet ve sipariş | `02 §3.23.1`–§3.23.5 (5) |
| T356 | F3 | Onay adımı: onay kutuları, Ön Bilgilendirme Formu içeriği, işlem rehberi | Sepet ve sipariş | `02 §3.24.1`–§3.24.8 (8) |
| T357 | F3 | Fatura verisi ve dışa aktarmada donmuş kimlik | Yönetim ve işlem izi | `02 §3.25.1`–§3.25.3 (3) |
| T358 | F3 | Kurumsal içerik tipleri: Hakkımızda, hizmet tanıtımı, referans iş, SSS, şube, genel sayfa, sosyal bağlantılar | Kurumsal içerik | `02 §3.27.1`–§3.27.9, §3.27.12 (10) |
| T359 | F3 | Kurumsal içerik: ürün bağı, elle sıralama, zorunlu alanlar, alan tavanları, kapsam dışı yetenekler | Kurumsal içerik | `02 §3.27.10`, §3.27.11, §3.27.13–§3.27.20 (10) |
| T360 | F3 | Duyuru şeridi, boş kurulum, içerik yayın durumu, önizleme ve silme | Kurumsal içerik | `02 §3.27.21`–§3.27.28 (8) |
| T361 | F3 | Ana sayfa düzeni ve üst menü | Kurumsal içerik | `02 §3.28.1`–§3.28.6 (6) |
| T362 | F3 | Marka kimliği: logo, marka adı, site simgesi, marka rengi, platform imzası | Kurumsal içerik | `02 §3.29.1`–§3.29.6 (6) |
| T363 | F3 | Sayfa adresleri, arama motoru bilgileri, kapalı sayfalar ve paylaşım | Kurumsal içerik | `02 §3.30.1`–§3.30.7 (7) |
| T364 | F3 | Panelde eşzamanlı düzenleme ve siparişte çakışan işlemler | Sepet ve sipariş | `02 §3.31.1`, §3.31.2 (2) |
| T365 | F3 | İletişim talebi: form, konu tipi, durumlar, saklama, istenmeyen ileti koruması | Kurumsal içerik | `02 §3.32.1`–§3.32.10 (10) |
| T366 | F3 | Yasal metinler: dört metin, sürüm artışı, aydınlatma bağlantıları, e-postada tam metin | Ayarlar | `02 §3.33.1`–§3.33.9 (9) |
| T367 | F3 | Müşteri iptali: kalem ve adet düzeyi, kargoya verilene kadar, dijital ve hizmet sınırı, sayaçların dönmesi, kısmi iptalde kupon | İptal, cayma ve iade | `02 §7.1.1`, §7.1.4, §7.2.1, §7.2.2, §7.2.5–§7.2.7 (7) |
| T368 | F3 | Gecikme feshi ve firma iptali: iki fesih yolu, kanuni faiz uyarısı, kapalı sebep listesi, ifanın imkânsızlaşması bildirimi | İptal, cayma ve iade | `02 §7.2.3`, §7.2.4 (2) |
| T369 | F3 | İşlemin adet birimi ve geri ödeme tutarı: açık adet, kupon payı, KDV, kargo ücretinin iptalde ve caymada ayrılması | İptal, cayma ve iade | `02 §7.1.3`, §7.1.6, §7.1.7, §7.2.9, §7.4.6 (5) |
| T370 | F3 | Geri ödeme hattı: ödemenin geldiği yol, kart ve havale, IBAN'ın ömrü, geri ödeme süresi, kart iadesi hataları | Ödeme | `02 §7.2.8`, §7.4.1, §7.4.5, §8.4.2 (4) |
| T371 | F3 | Cayma hakkı: pencere, dijital ve hizmet istisnası, ürün istisna işareti, tek rejim | İptal, cayma ve iade | `02 §7.1.2`, §7.3.1–§7.3.4 (5) |
| T372 | F3 | Cayma beyanı: sipariş sayfasından ve başka kanaldan, beyanın etkisi, kargoya verilmemiş kalemde iptal yolu, firmanın teyidi | İptal, cayma ve iade | `02 §7.3.5`–§7.3.7, §8.3.8 (4) · `02 §10.4.10` (1) |
| T373 | F3 | İade: iade kargo bedeli, ödemeli gönderim ve iade adresi, teslim alma adımı, stoğa ekleme, koşullu istisnada ret, kullanılmış mal | İptal, cayma ve iade | `02 §7.4.2`–§7.4.4, §7.4.7–§7.4.10 (7) · `02 §10.4.9` (1) |
| T374 | F3 | Ayıp talebi: kanal, iki yıllık süre, durumlar, çözüm, dijital dosya güncellemesi | İptal, cayma ve iade | `02 §7.5.1`–§7.5.4 (4) |
| T375 | F3 | Yöneticinin sipariş düzeltmeleri: ileri yönlü kayıt, tutar değişmez, firmanın teyidiyle e-posta ve adres düzeltmesi, cayma beyanı geri alınmaz | Sepet ve sipariş | `02 §7.6.6`, §7.6.7, §8.3.6, §8.3.7 (4) · `02 §10.4.1`, §10.4.2, §10.4.11 (3) |
| T376 | F3 | Deneme limitleri: dokuz limit, kayan pencere, hesap kilitlenmez, nötr mesaj | Ortak altyapı | `02 §8.2.1`, L-1…L-9 (10) |
| T377 | F3 | Limit ilkeleri: iki eksen, ödenmemiş sipariş sayımı, yönetici muafiyeti yok, tanınan tarayıcı işareti | Ortak altyapı | `02 §8.2.2`–§8.2.6 (5) |
| T378 | F3 | Zaman sayımı: tek saat dilimi, iş günü ve resmî tatil listesi, takvim ve iş günü sayımı, gün sonu, tarih aralığı, çitler | Ortak altyapı | `02 §4.1.1`–§4.1.5 (5) |
| T379 | F3 | Yönetici hesapları: ilk yönetici ve davet, kaldırma kuralları, tek rol, ayrı hesap türü, davetin geri çekilmesi, erişim kaybında kurtarma sınırı | Yönetim ve işlem izi | `02 §10.2.1`–§10.2.7 (7) |
| T380 | F3 | İşlem izi: sekiz alan, kim-ne zaman-ne değişti, değiştirilemez ve silinemez, sistem olayları ize girmez, düz liste, on yıl | Yönetim ve işlem izi | `02 §10.3.1`–§10.3.5 (5) |
| T381 | F3 | Ayar katmanları ve parametre envanteri: firma ayarı, kurulum ayarı, ürün sabiti, varsayılan ve çit, değişikliğin ize yazılması | Ayarlar | `02 §10.1.3` (1) · `02 §11.1`, §11.2 P-1…P-49 (47) |
| T382 | F3 | Panel ana sayfası verisi: altı bekleyen iş sayacı, kanal ve firma bildirimi uyarıları, kurulum kontrol listesi | Yönetim ve işlem izi | `02 §10.6.1`, §10.8.2 (2) |
| T383 | F3 | Satış özeti ve ölçüler: dönem seçimi, ciro, sayılar, dört mağaza ölçüsü, sıfır payda, tepe gün sayısı | Yönetim ve işlem izi | `02 §10.6.2`, §10.6.3, H-2 (3) |
| T384 | F3 | Dışa aktarma: sipariş CSV (fatura verisi), üye ve iletişim talebi listesi, işlem izine yazım, imha edilmiş siparişin boş sütunları | Yönetim ve işlem izi | `02 §10.7.1`–§10.7.4 (4) |
| T385 | F3 | Gönderim yeniden denemesi ve "e-posta ulaşmadı" işareti: üç deneme, işaretin düşüşü, panelden yeniden gönderim, işaretsiz bildirimler | Bildirim | `02 §9.1.6` (1) · P-38 |
| T386 | F3 | Sipariş müdahaleleri: kalem adedi azaltma ve çıkarma, kargo bilgisinin ve teslim tarihinin düzeltilmesi, iç not | Sepet ve sipariş | `02 §10.4.3`–§10.4.5, §10.4.7 (4) |
| T387 | F3 | Kurulum tarafı yönetim komutları: iş satırını yeniden kuyruğa alma ya da kapatma, anahtar yenilemesi sonrası ödeme sonucunun uygulanması, kayıt okuma komutu | Yönetim ve işlem izi | `05 §8.5`, §9.3, §6.9 (MK-66, MK-72) |
| T388 | F3 | Dönüş denetimi komutu ve dönüşten sonra silmelerin yeniden uygulanması | Yönetim ve işlem izi | `05 §8.5`, §8.7 (MK-72, MK-74) |
| T401 | F4 | PayTR: anahtarlar, ödemenin başlatılması, dönüş ve çerçeve | Ödeme | `08 §2.1`, §2.1.1, §2.1.2, §2.1.4 |
| T402 | F4 | PayTR: bildirim imzası ve alan eşlemesi | Ödeme | `08 §2.1.5` |
| T403 | F4 | PayTR: son sorgu ve tek tahsilat | Ödeme | `08 §2.1.6`, §2.1.7 |
| T404 | F4 | PayTR: bağlantı hatası ve ayrım | Ödeme | `08 §2.1.8` |
| T405 | F4 | PayTR: kart iadesi | Ödeme | `08 §2.1.9` |
| T406 | F4 | PayTR: 3D Secure kurulum şartı, test ve canlı kanıt | Ödeme | `08 §2.1.3`, §2.1.10 |
| T407 | F4 | SES: gönderim ve sonuç sınıfları | Bildirim | `08 §2.2`, §2.2.1, §2.2.2 |
| T408 | F4 | SES: geri dönen adreslerin öğrenilmesi | Bildirim | `08 §2.2.3` |
| T409 | F4 | Google: giriş, kayıt ve yeniden doğrulama bağlayıcısı | Üyelik | `08 §2.3` |
| T410 | F4 | Dış olgu varsayımlarının doğrulama kanıtları (DV) | Ortak | `08 §4.1`, §5.2 |
| T501 | F5 | Yerli üretim logosu varlığı ve gösterimi | Katalog | `08 §4.3` |
| T502 | F5 | Vitrin: ana sayfa ve kategori sayfası (E-01, E-02) | Katalog | `07` UC-01, UC-02 (2) |
| T503 | F5 | Vitrin: arama sonuçları, ürün sayfası ve sepete ekleme (E-03, E-04; arşivdeki E-05 T549'da) | Katalog | `07` UC-03…UC-05 (3) |
| T504 | F5 | Hata sayfaları: bulunamayan adres ve beklenmeyen hata (E-06, 500) | Ortak altyapı | `07` UC-06, UC-78 (2) |
| T505 | F5 | İçerik liste ve içerik sayfaları (E-07, E-08) | Kurumsal içerik | `07` UC-07…UC-12 (6) |
| T506 | F5 | Sık sorulan sorular ve iletişim sayfası ve formu (E-09, E-10) | Kurumsal içerik | `07` UC-13…UC-15 (3) |
| T507 | F5 | Yasal metin sayfaları ve panelde yönetimi (E-11, E-47) | Ayarlar | `07` UC-16…UC-18, UC-102, UC-173, UC-174 (6) |
| T508 | F5 | Sepet (E-12) | Sepet ve sipariş | `07` UC-19…UC-21 (3) |
| T509 | F5 | Ödeme adımı (E-13) | Sepet ve sipariş | `07` UC-22…UC-27 (6) |
| T510 | F5 | Sipariş teyit ve bekleme ekranı, ödeme sağlayıcısına geçiş ve dönüş (E-14) | Ödeme | `07` UC-28…UC-30 (3) |
| T511 | F5 | Sipariş takibi girişi, sipariş sayfası ve indirme bağlantısı (E-15, E-16) | Sepet ve sipariş | `07` UC-31…UC-34, UC-36 (5) |
| T512 | F5 | İptal ve gecikme feshi ekranı, geri ödeme IBAN girişi (E-17, E-16 IBAN formu) | İptal, cayma ve iade | `07` UC-35, UC-38…UC-41 (5) |
| T513 | F5 | Cayma beyanı ve ayıp talebi ekranları (E-18, E-19) | İptal, cayma ve iade | `07` UC-42…UC-46 (5) |
| T514 | F5 | Kayıt ve doğrulama bağlantısının inişi (E-20, E-21) | Üyelik | `07` UC-47…UC-49, UC-52, UC-53 (5) |
| T515 | F5 | Müşteri girişi ve Google ile giriş (E-22) | Üyelik | `07` UC-50, UC-51, UC-54, UC-55 (4) |
| T516 | F5 | Şifre sıfırlama ve yeniden doğrulama (E-23, E-24) | Üyelik | `07` UC-56…UC-62 (7) |
| T517 | F5 | Hesap — profil ve güvenlik (E-25) | Üyelik | `07` UC-63…UC-69 (7) |
| T518 | F5 | Adres defteri ve sipariş geçmişi (E-26, E-27) | Üyelik | `07` UC-70…UC-74 (5) |
| T519 | F5 | E-posta değişikliğini geri alma ve müşteri çıkışı (E-28) | Üyelik | `07` UC-75…UC-77 (3) |
| T520 | F5 | Panel girişi, şifre sıfırlama, davet kabulü ve yönetici çıkışı (E-29, E-30) | Yönetim ve işlem izi | `07` UC-84, UC-85, UC-109…UC-113 (7) |
| T521 | F5 | Panel ana sayfası ve yönetici hesabının doğrulama ekranları (E-31, E-54) | Yönetim ve işlem izi | `07` UC-86, UC-108, UC-185…UC-187 (5) |
| T522 | F5 | Yönetici hesapları ve işlem izi (E-49, E-52) | Yönetim ve işlem izi | `07` UC-103, UC-106, UC-175…UC-177 (5) |
| T523 | F5 | Yöneticinin kendi hesabı (E-50) | Yönetim ve işlem izi | `07` UC-104, UC-178…UC-181 (5) |
| T524 | F5 | Satış özeti ve dışa aktarma (E-51, E-53) | Yönetim ve işlem izi | `07` UC-105, UC-107, UC-182…UC-184 (5) |
| T525 | F5 | Ürün listesi ve ürün formu, görsel yükleme (E-32, E-33) | Katalog | `07` UC-87, UC-88, UC-114…UC-117 (6) |
| T526 | F5 | Kategori ağacı ve kuponlar (E-34, E-35) | Katalog | `07` UC-89, UC-90, UC-118…UC-122 (7) |
| T527 | F5 | Sipariş listesi, sipariş ayrıntısı ve durum geçişleri (E-36, E-37) | Sepet ve sipariş | `07` UC-91, UC-92, UC-123…UC-128 (8) |
| T528 | F5 | Sipariş ayrıntısı: firma iptali, bildirim kaydı, kalem çıkarma, iade malı ve ayıp talebi (E-37) | İptal, cayma ve iade | `07` UC-129, UC-130, UC-138, UC-140…UC-142, UC-144, UC-145 (8) |
| T529 | F5 | Sipariş ayrıntısı: geri ödeme işlemleri (E-37) | Ödeme | `07` UC-131…UC-137 (7) |
| T530 | F5 | Sipariş ayrıntısı: adres, kargo, teslim tarihi, durum geçişi ve e-posta düzeltmeleri (E-37) | Sepet ve sipariş | `07` UC-146…UC-151 (6) |
| T531 | F5 | Sipariş ayrıntısı: hizmet kalemi, indirme hakkı, iç not ve e-posta yeniden gönderim (E-37) | Sepet ve sipariş | `07` UC-139, UC-143, UC-152…UC-154 (5) |
| T532 | F5 | Talepler ve iletişim talebi ayrıntısı (E-38, E-39) | Kurumsal içerik | `07` UC-93, UC-94, UC-155, UC-156 (4) |
| T533 | F5 | Üye kaydı görünümü (E-40) | Üyelik | `07` UC-95, UC-157 (2) |
| T534 | F5 | Kurumsal içerik ve marka yönetimi (E-41, E-43) | Kurumsal içerik | `07` UC-96, UC-98, UC-158…UC-161, UC-163 (7) |
| T535 | F5 | Ana sayfa ve menü, firma kimliği ve satış (E-42, E-44) | Ayarlar | `07` UC-97, UC-99, UC-162, UC-164…UC-166 (6) |
| T536 | F5 | Ödeme yöntemleri, kargo, süreler ve varsayılanlar (E-45, E-46) | Ayarlar | `07` UC-100, UC-101, UC-167…UC-172 (8) |
| T537 | F5 | Vitrin çerçevesinin modeli: marka, menü, duyuru şeridi, hesap alanı, sepet sayısı | Ortak altyapı | `07 §5.5` (1) |
| T538 | F5 | Uç ve model eşitlik denetimleri: uç envanteri, form girdi ve sorgu modelleri | Ortak altyapı | `07 §5.2`, `§5.3` (2) |
| T539 | F5 | Betiksiz yol: JavaScript kapalıyken temel akışlar ve betikli iyileştirme | Ortak altyapı | `07 §8.2` (1) |
| T540 | F5 | Tasarım tabanı, vitrin ve panel çerçevesi, yönetici şeridi ve Taslak bandı (OB-01, OB-02, OB-09) | Ortak altyapı | `04 §2.1`, §2.2, §2.9, §3.1, §3.2, §8.3 (3 OB + 3 alt bölüm) |
| T541 | F5 | Mesaj, onay, boş ve hata hâlleri ve satış kapalı hâli (OB-03, OB-04, OB-07, OB-08) | Ortak altyapı | `04 §2.3`, §2.4, §2.7, §2.8 (4 OB) |
| T542 | F5 | Rozet ve işaretler, sayaç ve süre göstergeleri, eşzamanlı düzenleme uyarısı (OB-05, OB-06, OB-10) | Ortak altyapı | `04 §2.5`, §2.6, §2.10 (3 OB) |
| T543 | F5 | Form alanı ve düzeni, adres formu, IBAN alanı, sebep seçimi ve ortak alan kuralları (OB-12…OB-15) | Ortak altyapı | `04 §2.12.1`–§2.12.4, §7.3 (4 OB + 1) |
| T544 | F5 | Tarih alanı, kupon alanı, onay kutuları, görsel ve dosya yükleme arayüzü (OB-16…OB-19) | Ortak altyapı | `04 §2.12.5`–§2.12.8 (4 OB) |
| T545 | F5 | Liste ve tablo, ürün kartı (OB-20, OB-11) | Ortak altyapı | `04 §2.11`, §2.13 (2 OB) |
| T546 | F5 | Gezinme ve geçiş eşitlik denetimleri: iskeletler, geçişler, dışarıdan girişler, dönüşler | Ortak altyapı | `04 §3.3`–§3.7 (5) |
| T547 | F5 | Durum × rol matrisi ve form envanteri eşitlik denetimleri | Ortak altyapı | `04 §6.1`–§6.3, §7.1, §7.2 (5) |
| T548 | F5 | Parçalı yükleme betiği: başlatma–parça–durum akışı, 409/404/400/413 yanıtları, ağ hatasında üç kez sürdürme | Ortak altyapı | `09 §16.2`; `07 §4.31.2`; K-1354 |
| T549 | F5 | Vitrin: arşivlenmiş ürün sayfası (E-05) — "bu ürün artık satılmıyor" bilgisi, fiyatsız gösterim (KP-7, `02 §3.7`, §5.1) | Katalog | `04` E-05; `07` UC-04 (K-1437) |
| T10 | F0 | Veritabanı kullanıcıları: kurucu betik, beş kullanıcı, `pg_hba`, rol ayarları, `pg_trgm`, yetki matrisi testi | Ortak altyapı | `09 §10.8`, §17.1; `05 §7.1.3` (K-1098, K-1124, K-1343; K-1441) |
| T232 | F2 | Veri koruma halkası: tek kurulum, amaç dizgeleri listesi, istisnasız çözme, çapraz amaç testi | Ortak altyapı | `09 §15.5`; `05 §7.6.3` (K-1082, K-1217; K-1441) |
| T233 | F2 | Veritabanı oturum süreleri: `DatabaseTimeouts`, Web ve Maintenance bağlantı profilleri, aşımın beklenmeyen hâle düşmesi | Ortak altyapı | `09 §10.7`, §3.5 (K-1441) |
| T389 | F3 | Hesap ve durum yönetim komutları: oturumları sonlandırma, şifreyi geçersiz kılma, durum | Yönetim ve işlem izi | `05 §8.5` (8.5.3, 8.5.4, 8.5.6); `09 §13.2` (MK-72; K-1441) |
| T390 | F3 | Uçtan uca dal akış testleri (servis düzeyi): karışık sipariş, kargoda cayma, IBAN'sız iade, e-posta düzeltme | Sepet ve sipariş | `03 §2.10.2`–§2.10.5 (K-1410, K-1419; K-1441) |
| T601 | F6 | `DEPLOY_RUNBOOK` provası, dış sistemde kayıtlı adresler ve yansıyan tablolar | Ortak | `06 §8.5`, `08 §4.2` |
| T602 | F6 | Güncelleme komutu: referans kurulumdan başlayıp sırayla, geride kalmış kurulumun ara sürümlerden geçişi, geri alma ve yedekten dönüş koşulu | Ortak | `05 §8.6`, §8.7 (MK-73, MK-74) |

**Kapsama komutları (Git Bash; depo kökünden).** Sayımlar aşağıdaki sayılarla aynı çıkmalı, her iki betik de çıkış kodu 0 vermelidir (eksik, fazla, yinelenen 0).

```sh
python -I scripts/doc-scans/11_matris_envanter.py . /tmp/envanter.txt    # kaynak envanteri: 06 117, 07 208, 08 30 (ve öteki aileler)
python -I scripts/doc-scans/11_matris_kapsama.py . /tmp/kapsama.txt      # matris satırları ↔ beklenen küme (dolu aileler: 06, 07, 08, 04, 03, 02 §1–§13, 05, 09, 10, devir — 1.501/1.501)
python -I scripts/doc-scans/11_geri_yon.py . --denetle                       # 7.2 ↔ 7.1 iki yönlü eşitlik (kapsama betiği aynı denetimi ve K-1409 boyut sınırlarını da koşar — 209 aday, 3.239 bağ)
```

**Okuma derinliği (O1).** `06`: §3.1 entity tablosu bütün, §2/§5–§9 alt bölüm başlıkları ve ilk paragrafları, §8 bütün, §7.2'nin 130 bağı betikle; §4 entity tanımları okunmadı — kaynak birim §3.1'in satırıdır. `08`: §1 bütün, §2–§5 başlıkları ve her başlığın ilk iki paragrafı (satır başına ilk 300–380 karakter; kesilerek okundu).

**Okuma derinliği (O3).** `04`: §4 ekran envanteri (53 satır) ve §4.3, §4.4 bütün; §2'nin bileşen özet tablosu (20 satır, her hücre) ve alt bölüm başlıkları; §3, §6, §7, §8 alt bölüm başlıkları ve ilk paragrafları. Okunmadı: §5 ve §9 ekran tanımları, §2 bileşen tanımlarının gövdesi, §6.2–§6.3 matris satırları, §7.1–§7.3 alan tabloları. `03`: bütün `###` başlıkları (55); §2.10, §4, §5, §6, §7, §8.2, §8.5, §9.5, §10 bölümlerinin girişi ve ilk tablo satırları (satır başına ilk 250–420 karakter); §1, §2.1–§2.9, §3, §8.1, §8.3, §8.4, §8.6–§8.9, §9.1–§9.4 yalnız başlık ve ilk paragraf. Eşleme ekran adı ve `07`'nin F5 gruplarından türer; gövdesi okunmayan alt bölümlerde (özellikle `03 §3.5`, §6.2) task listesi sonraki oturumda örneklemle doğrulanır.

**Okuma derinliği (O4a).** `02 §3`'ün 257 kuralı ve 34 alt bölümü bütün okundu — her kural satırı tamamıyla (§3.1–§3.34, satır 497–1090); §2 (§2.1 envanter tablosu, §2.2 ve §2.3 bütün) ve §1.1, §1.3 bütün okundu. **Okunmadı:** §1.2 terim sözlüğünün tablo satırları (birim değildir; alt bölüm satırı karşılar), her bölümün sonundaki `Kaynak:` satırları (karar atıfları; birim değildir). Okunan kuralların hiçbirinde kesme yapılmadı.

**Okuma derinliği (O4b 1/2).** `02 §4`–§8 (satır 1094–1802): her kural satırı, Z ve L satırı ve `###` başlığı tamamıyla okundu; satırların uzun olanları ilk 430–700 karakter ve kalan parçalar ayrı ikinci okumayla (kesilmiş satır kalmadı). Okunmadı: her bölümün sonundaki `Kaynak:` satırlarının K-no sütunu (karar atıfları; birim değildir) ve tablo satırlarındaki K-no sütunu. `§6`'nın `6.2.n`…`6.9.n` satırları (birim değildir) alt bölüm satırının task birleşimi için okundu. §9–§13 bu oturumda okunmadı.

**Okuma derinliği (O4b 2/2).** `02 §9`–§13 (satır 1803–2241): her kural satırı, B ve F satırı, P ve H satırı ve `###` başlığı tamamıyla okundu (uzun satırlar kesilmeden, `§10.4` ve `§12.2.7` dahil); `§12.5` tablosunun 32 satırı bütün okundu; `§13` bütün. Okunmadı: bölüm sonlarındaki `Kaynak:` satırlarının K-no listeleri (karar atfı; birim değildir) ve dosya sonundaki sürüm geçmişi. Eşleme `02` metninden elle yapıldı; betikle üretilen yalnız satır metni, başlık, sayım ve `§11` P satırlarının "Nerede kullanılır" atıflarından çıkan task adayıdır. Örneklem: `§5` yazım oturumu her F3 task'ının kural kümesini kaynak satırlarıyla yeniden okur, ayrışma %10'u aşarsa genişletir (K-1424 (4)'ün kalıbı).

**Okuma derinliği (O5 1/2).** `05 §12` (satır 1440–1529): 78 MK satırının her hücresi (karar, gerekçe, geri alınabilirlik) tamamıyla, kesme yapılmadan; `###` alt bölümlerinin 58 başlığı; `§11.1`, `§11.2` ve `§11.3`'ün girişi. Okunmadı: `§3`–§11 alt bölüm gövdeleri, `§0`–§2 gövdeleri, `§12`'nin `Kaynak:` satırı ve dosya sonundaki sürüm geçmişi (alt bölüm birimi başlık satırıdır; task birleşimi MK satırlarının `(§n.m)` atıflarından kuruldu). Gövde okuması `§5` yazım oturumuna kalır (K-1433).

**Okuma derinliği (O5 2/2).** `10 §2` (satır 135–233): 77 KP satırının her hücresi (özellik, neden, kaynak) tamamıyla, kesme yapılmadan; `§3` (satır 237–324): 64 KD satırının özellik ve kaynak hücresi, uzun gerekçe hücrelerinin ilk 470 ve son 140 karakteri (620 karakteri aşan satırlarda) ve girişin biçim paragrafları; `§4.1` ve `§4.2` başlıkları, girişleri ve ÖK/SK satırlarının ilk 330 karakteri. Okunmadı: KD gerekçe hücrelerinin ortası, `§4.1`/`§4.2` satırlarının sonları ve `Kaynak:` satırları, `§1`, `§5`. `09`: 25 bölümün başlığı ve `§1`, `§2`, `§17`, `§20`, `§22`, `§24`, `§25`'in girişi ve ilk maddeleri (ilk 340 karakter); öteki bölümler başlık düzeyinde (alt bölüm başlık listesi) ve `09` başlık notunun okuma tablosuna göre eşlendi — gövde okunmadı (465 KB). Devir: `11` park bloğu (P8.1–P8.5) tam, `DEFERRED_BACKLOG` D-01 satırı tam, `04` 1948 (GA-7) karar kaydı §22.1'in ★ paragrafı üzerinden (K-1403). Gövde okuması `§5` yazım oturumuna kalır (K-1436).

**Geri yön ve gerekçesiz ekleme taraması (O6 1/2).** `7.2` (task → kaynak öğe) `scripts/doc-scans/11_geri_yon.py --yaz` ile 7.1'in "Task" hücrelerinden türetildi: 209 aday task, 3.239 (kaynak, task) bağı; `Durum` sütunu kaynak sayısını ve türünü (özgün · alt bölüm · devir) yazar. Kapsama betiği 7.1 ile 7.2'nin iki yönlü eşitliğini (yalnız 7.1'de: 0, yalnız 7.2'de: 0), her aday task'ın ≥1 kaynağı olduğunu ve K-1409 sınırlarını denetler. **Gerekçesiz ekleme taraması:** kaynağı yalnız alt bölüm satırı ya da yalnız alt bölüm + devir olan 18 task (T03, T102, T119, T120, T204, T205, T211, T213, T214, T217, T230, T231, T402, T408, T410, T538, T546, T548) aday tablosundaki kısa adlarına ve kayıt satırlarına karşı okundu; 18'i de ilgili matris kararında adıyla bağlı (örtük işi anlatan K'si var) — **gerekçesiz ekleme: 0**, `§8`e aday yok. **Boyut denetimi:** F1 entity ≤8, F3 ve F4 uç ≤8, F5 ekran ≤2, OB ≤4 (K-1409) — tek aşım T503 (üç ekran: E-03, E-04, E-05); E-05 (arşivlenmiş ürün sayfası) `T549`'a ayrıldı (K-1437), aşım 0. Faz başına task: F0 9 · F1 20 · F2 31 · F3 88 · F4 10 · F5 49 · F6 2. **Okuma derinliği:** bu oturum satır kümelerini kesmeden okumadı; 7.1'in "Task" hücreleri ve aday tablosu betikle işlendi, 18 zayıf kaynaklı task'ın K satırları ilgili cümleye kadar okundu (tam K gövdesi değil).

**Okuma derinliği örneklemi ve boşluk taraması (O6 2/2).** Matris oturumlarında kesilerek okunan beş küme, ~%10 (en az 5) satırla tam metinden sınandı; okumayı küme başına Sonnet `low` alt ajanlar yaptı, her bulgu Opus `high` doğrulayıcılarla kademeli sınandı (GAP adayları tek tek, satır eksikleri 5–8'li, belirsizler 3–4'lü). **Ayrışma oranı:** `08 §2`–§5 alt bölümleri 7 örnek, 0 (%0); `07 §4`/§9 uç tanımları 20 uç, 1 (UC-117 → T534; %5); `04 §5`/§9 ekran ve bileşen tanımları 9 örnek, 0 (%0); `10 §3` KD uzun satırları 5 örnek, 0 (%0); **`03` gövdeleri 7 örnek, 6 (%86), `05 §3`–§11 gövdeleri 8 örnek, 2 (%25) ve `09` gövdeleri 5 örnek, 4 (%80) — üçü %10'u aştı ve kümenin tamamına genişledi:** `03`'ün 55 alt bölümü, `05`'in 58 alt bölümü ve `09`'un 25 bölümü gövdesiyle okundu. Örneklemin ve genişlemenin 45 satır-eksik iddiasından 4'ü tümüyle, 11'i kısmen reddedildi, 30'u doğrulandı; 25 belirsiz iddianın 14'ü satıra bağ olarak girdi; **59 matris satırına 128 task bağı** (yeni beş task'ın bağları dahil) eklendi (`03` 29 satır, `05` 14, `09` 15, `07` 1; bunlar eşlemenin eksik bağlarıdır, yeni iş değildir). 7.2 betikle yeniden türetildi (3.367 bağ, 7.1 ile eşit). Satır eklemesi gerekmeyen belirsizler (sahibi satırda, yalnız atıf ya da `DEPLOY_RUNBOOK` işi) K-1440'ta sayılır. **Okunmadı:** `06 §4` entity tanımları (kaynak birim §3.1 satırıdır; O1'in kararı, örnekleme girmedi), `05 §0`–§2 gövdeleri (satır birimi yok), `02`'nin O4a/O4b'de tam okunan kuralları. **Boşluk (GAP) taraması:** gövdelerin tam okunması **yedi** sahipsiz iş çıkardı (`GAP-15`…`GAP-21`; beşi yeni aday task `T10`, `T232`, `T233`, `T389`, `T390`, üçü var olan T202, T222 ve T303'ün kapsam genişlemesi); ayrıntı §8'de, karar K-1441'dedir. Aday task 209 → 214; sayaçlar F0 T11 · F2 T234 · F3 T391.

#### 7.1.1 Veri modeli → task

| Kaynak öğe | Task | Implement edildi | Kanıt |
|---|---|---|---|
| 06:§2.1 Şablonun altı dersi ve bu projedeki yeri | T101 | | |
| 06:§2.2 Silme kategorileri ve bağların silme davranışı | T101 | | |
| 06:§2.3 Anahtarlar ve dışa görünen kimlikler | T101 | | |
| 06:§2.4 Adlandırma ve şema | T101 | | |
| 06:§2.5 Türler ve sayısal ölçek | T101 | | |
| 06:§2.6 Eşzamanlılık — sürüm değeri ve koşullu güncelleme | T101 | | |
| 06:§2.7 Denormalizasyon envanteri | T120 | | |
| 06:§2.8 Metin karşılaştırma, sıralama, oluşturulma anı ve boş değer | T101 | | |
| 06:§2.9 Enum'ların biçimi | T101 | | |
| 06:§2.10 Uzun metnin saklanışı | T101 | | |
| 06:§4.1 Company | T103 | | |
| 06:§4.2 CompanySettings | T103 | | |
| 06:§4.2 CompanyDeliveryProvince | T103 | | |
| 06:§4.3 Province | T103 | | |
| 06:§4.4 Country | T103 | | |
| 06:§4.5 Product | T104 | | |
| 06:§4.6 Variant | T104 | | |
| 06:§4.7 Option | T104 | | |
| 06:§4.8 OptionValue | T104 | | |
| 06:§4.9 Category | T104 | | |
| 06:§4.10 ProductCategory | T104 | | |
| 06:§4.11 PriceHistory | T105 | | |
| 06:§4.12 Discount | T105 | | |
| 06:§4.13 Coupon | T105 | | |
| 06:§4.14 StockReservation | T110 | | |
| 06:§4.15 Cart | T109 | | |
| 06:§4.16 CartItem | T109 | | |
| 06:§4.17 Order | T109 | | |
| 06:§4.18 OrderItem | T109 | | |
| 06:§4.19 TransitionRecord | T109 | | |
| 06:§4.20 InternalNote | T109 | | |
| 06:§4.21 CheckboxAcceptance | T109 | | |
| 06:§4.21 CheckboxAcceptanceOrderItem | T109 | | |
| 06:§4.22 Cancellation | T111 | | |
| 06:§4.23 ItemRemoval | T111 | | |
| 06:§4.24 LateDeliveryTermination | T111 | | |
| 06:§4.25 Withdrawal | T111 | | |
| 06:§4.26 Return | T111 | | |
| 06:§4.27 ReturnRejection | T113 | | |
| 06:§4.28 NotReturnedClosure | T111 | | |
| 06:§4.29 RefundIbanRequest | T113 | | |
| 06:§4.30 DefectClaim | T111 | | |
| 06:§4.31 RefundIban | T111 | | |
| 06:§4.32 PaymentRecord | T112 | | |
| 06:§4.33 Refund | T112 | | |
| 06:§4.34 RefundRecord | T112 | | |
| 06:§4.35 InboundNotificationRecord | T112 | | |
| 06:§4.36 Customer | T107 | | |
| 06:§4.37 Admin | T106 | | |
| 06:§4.38 AdminInvitation | T106 | | |
| 06:§4.39 Session | T108 | | |
| 06:§4.40 PendingEmailChange | T107 | | |
| 06:§4.41 RecognizedBrowserMark | T108 | | |
| 06:§4.42 LinkRecord | T108 | | |
| 06:§4.43 AttemptRecord | T108 | | |
| 06:§4.44 LoginRecord | T108 | | |
| 06:§4.45 AddressBook | T107 | | |
| 06:§4.46 AboutPage | T114 | | |
| 06:§4.47 Offering | T114 | | |
| 06:§4.48 PortfolioItem | T114 | | |
| 06:§4.49 FaqItem | T114 | | |
| 06:§4.50 Branch | T115 | | |
| 06:§4.51 CustomPage | T114 | | |
| 06:§4.52 Announcement | T114 | | |
| 06:§4.53 OfferingProduct | T115 | | |
| 06:§4.53 PortfolioItemProduct | T115 | | |
| 06:§4.54 Branding | T114 | | |
| 06:§4.55 SocialLink | T114 | | |
| 06:§4.56 LegalTextVersion | T103 | | |
| 06:§4.57 ContactRequest | T115 | | |
| 06:§4.58 DispatchRecord | T117 | | |
| 06:§4.59 Job | T118 | | |
| 06:§4.60 ChannelStateRecord | T117 | | |
| 06:§4.61 OrderEmailHistory | T110 | | |
| 06:§4.62 OrderAddressHistory | T110 | | |
| 06:§4.63 AuditLog | T106 | | |
| 06:§4.64 DisposalRecord | T118 | | |
| 06:§4.65 HeartbeatRecord | T118 | | |
| 06:§4.66 OutageRecord | T118 | | |
| 06:§4.67 BackupStatusRecord | T118 | | |
| 06:§4.68 DataProtectionKey | T118 | | |
| 06:§4.69 Image | T104 | | |
| 06:§4.70 DigitalFile | T105 | | |
| 06:§4.71 ProductImage | T104 | | |
| 06:§4.71 AboutPageImage | T116 | | |
| 06:§4.71 OfferingImage | T116 | | |
| 06:§4.71 PortfolioItemImage | T116 | | |
| 06:§4.71 FaqItemImage | T116 | | |
| 06:§4.71 CustomPageImage | T116 | | |
| 06:§5.1 Biçim, kaynak ve okunuş | T101 | | |
| 06:§5.2 Durum setleri ve geçiş kimlikleri | T102 | | |
| 06:§5.3 Sözlüğün listeleri | T102 | | |
| 06:§5.4 Teknik kayıtların listeleri | T102 | | |
| 06:§5.5 Kimlikli ürün listeleri | T102 | | |
| 06:§5.6 Kayıt düzeyindeki kapalı değerler | T102 | | |
| 06:§6.1 Okunuş | T120 | | |
| 06:§6.2 Firma, ayarlar ve katalog | T120 | | |
| 06:§6.3 Sipariş, kalem kayıtları ve ödeme | T120 | | |
| 06:§6.4 Hesaplar ve oturum | T120 | | |
| 06:§6.5 İçerik, yasal metinler ve dosyalar | T120 | | |
| 06:§6.6 Bağsız sistem kayıtları | T120 | | |
| 06:§7.1 Tekillikler | T120 | | |
| 06:§7.2 Yabancı anahtarlar ve silme davranışları | T120 | | |
| 06:§7.3 Kontrol kısıtları | T120 | | |
| 06:§7.4 Koşullu güncellemelerin dayandığı kısıtlar | T120 | | |
| 06:§7.5 Sorgu dizinleri | T120 | | |
| 06:§7.6 Hacim kabulleri | T120 | | |
| 06:§8.1 Seed | T119 | | |
| 06:§8.2 Fail-fast zorunlu | T205 | | |
| 06:§8.3 Kuruluma özgü | T303 | | |
| 06:§8.4 Veritabanında olmayanlar | T119 | | |
| 06:§8.5 `DEPLOY_RUNBOOK`'a yansıma | T601 | | |
| 06:§9.1 Sürelerin başlangıç değerleri | T201 | | |
| 06:§9.2 Siparişin imha parçası — alan listesi | T301 | | |
| 06:§9.3 Öteki kayıtların süreyle imhası | T302 | | |
| 06:§9.4 Anlık silmeler ve gecelik temizlik | T201 | | |
| 06:§9.5 Arşiv tablosu yoktur; yedeklerdeki kopyalar | T201 | | |

#### 7.1.2 API → task

> **Eşleme kuralı (K-1415, K-1416).** Sayfa ve form ucu (181 uç) kendi ekranının F5 task'ına girer: en çok 2 ekran ve 8 uç, tek modül (K-1409); sipariş ayrıntısı (E-37) uç sınırı ve modül ölçütü yüzünden beş task'a bölünür (T527–T531). Sayfasız uçlar F3'tedir (T311–T314), canlılık ucu F0'dadır (T01; K-1407 F0 çıkışı). `§2`, `§5` ve `§6`'nın ortak kalıpları F2'dedir (T206–T217); `§2.10` T206'ya, `§5.2`–`§5.3` eşitlik denetimi T538'e girer. İş mantığı bu satırlarda değil `02` satırlarındadır (O4a, O4b).

**Okuma derinliği (O2).** `07 §3.2`: 187 uç satırı bütün (her hücre). `§3.3`, `§3.4` ve `§4`'ün girişi bütün. `§4.33`, `§4.34` bütün. `§2`, `§5`, `§6`, `§8`: alt bölüm başlıkları, ilk paragraflar ve kalın maddelerin başlıkları (kesilerek okundu); `§2.10` bütün. **Okunmadı:** `§4` ve `§9`'un uç tanımları (`§4.33` ve `§4.34` dışında), `§5.2`–`§5.5` ve `§6.2`–`§6.3` tablolarının satırları — kaynak birim `§3.2`'nin satırı ve alt bölümün başlığıdır; sonraki oturum örneklemle doğrular.

| Kaynak öğe | Task | Implement edildi | Kanıt |
|---|---|---|---|
| 07:§2.1 Adres yapısı ve önekler | T206 | | |
| 07:§2.2 Sayfa ve form ucunun biçimi | T206 | | |
| 07:§2.3 Kimlik doğrulama ve erişim reddi | T207 | | |
| 07:§2.4 Sürümün ve gösterilen değerin formda taşınması | T208 | | |
| 07:§2.5 Tek kullanımlık değerler, tekrar gönderim ve sahteciliğe karşı koruma | T209 | | |
| 07:§2.6 Listeler | T210 | | |
| 07:§2.7 Hata gösterimi ve HTTP durumları | T212 | | |
| 07:§2.8 Hız sınırı ve gövde tavanı | T215 | | |
| 07:§2.9 Tarih, sayı ve para biçimi | T216 | | |
| 07:§2.10 Sürümleme | T206 | | |
| 07 UC-01 GET `/` — Ana sayfa | T502 | | |
| 07 UC-02 GET `/kategori/{Slug}` — Kategori sayfası | T502 | | |
| 07 UC-03 GET `/arama` — Arama sonuçları | T503 | | |
| 07 UC-04 GET `/urun/{Slug}` — Ürün adresi | T503 | | |
| 07 UC-05 POST `/urun/{Slug}?handler=AddCartItem` — Sepete ekleme | T503 | | |
| 07 UC-06 — her isteğin yanıtı (404; §2.7.1) — Bulunamayan adres | T504 | | |
| 07 UC-07 GET `/hizmetler` — Hizmetlerimiz listesi | T505 | | |
| 07 UC-08 GET `/referanslar` — Referanslarımız listesi | T505 | | |
| 07 UC-09 GET `/hakkimizda` — Hakkımızda | T505 | | |
| 07 UC-10 GET `/hizmet/{Slug}` — Hizmet tanıtımı sayfası | T505 | | |
| 07 UC-11 GET `/referans/{Slug}` — Referans iş sayfası | T505 | | |
| 07 UC-12 GET `/sayfa/{Slug}` — Genel sayfa | T505 | | |
| 07 UC-13 GET `/sss` — Sık sorulan sorular | T506 | | |
| 07 UC-14 GET `/iletisim` — İletişim sayfası | T506 | | |
| 07 UC-15 POST `/iletisim?handler=SendContactRequest` — İletişim formunun gönderimi | T506 | | |
| 07 UC-16 GET `/aydinlatma-metni` — Aydınlatma metni | T507 | | |
| 07 UC-17 GET `/cerez-politikasi` — Çerez politikası | T507 | | |
| 07 UC-18 GET `/islem-rehberi` — İşlem rehberi | T507 | | |
| 07 UC-19 GET `/sepet` — Sepet | T508 | | |
| 07 UC-20 POST `/sepet?handler=UpdateCartItem` — Sepet kaleminin adedini değiştirme | T508 | | |
| 07 UC-21 POST `/sepet?handler=RemoveCartItem` — Sepet kalemini çıkarma | T508 | | |
| 07 UC-22 GET `/odeme` — Ödeme adımı | T509 | | |
| 07 UC-23 POST `/odeme?handler=ApplyCoupon` — Kuponu uygulama ya da değiştirme | T509 | | |
| 07 UC-24 POST `/odeme?handler=RemoveCoupon` — Kuponu kaldırma | T509 | | |
| 07 UC-25 POST `/odeme?handler=Recalculate` — Ödeme adımının ara gönderimi | T509 | | |
| 07 UC-26 POST `/odeme?handler=Leave` — Ödeme adımından girişe ya da sepete | T509 | | |
| 07 UC-27 POST `/odeme?handler=PlaceOrder` — Siparişi onaylama | T509 | | |
| 07 UC-28 GET `{kök}/teyit` — Sipariş teyit ve bekleme ekranı | T510 | | |
| 07 UC-29 POST `{kök}/teyit?handler=StartPayment` — Ödeme sağlayıcısına geçiş | T510 | | |
| 07 UC-30 GET `/odeme/donus/{erişim anahtarı}` — Ödeme sağlayıcısından dönüş | T510 | | |
| 07 UC-31 GET `/siparis-takibi` — Sipariş takibi girişi | T511 | | |
| 07 UC-32 POST `/siparis-takibi?handler=FindOrder` — Sipariş takibi sorgusu | T511 | | |
| 07 UC-33 GET `/siparis/{sipariş numarası}` — Sipariş sayfası | T511 | | |
| 07 UC-34 GET `/siparis/erisim/{erişim anahtarı}` — Erişim anahtarlı sipariş adresi | T511 | | |
| 07 UC-35 POST `{kök}?handler=SaveRefundIban` — Geri ödeme IBAN'ının girişi ve düzeltmesi | T512 | | |
| 07 UC-36 POST `{kök}?handler=DownloadDigitalFile` — İndirme bağlantısının üretimi | T511 | | |
| 07 UC-37 GET `/indir/{imzalı değer}` — İndirme ucu | T312 | | |
| 07 UC-38 GET `{kök}/iptal` ve 1 adres daha — İptal ve gecikme feshi ekranı | T512 | | |
| 07 UC-39 POST `{kök}/iptal?handler=CancelOrder` — Siparişin iptali | T512 | | |
| 07 UC-40 POST `{kök}/iptal?handler=CancelOrderItems` — Kalem iptali | T512 | | |
| 07 UC-41 POST `{kök}/gecikme-feshi?handler=SubmitLateDeliveryTermination` — Gecikme feshi | T512 | | |
| 07 UC-42 GET `{kök}/cayma` — Cayma beyanı ekranı | T513 | | |
| 07 UC-43 POST `{kök}/cayma?handler=SubmitWithdrawal` — Cayma beyanı | T513 | | |
| 07 UC-44 GET `{kök}/ayip-talebi` — Ayıp talebi ekranı | T513 | | |
| 07 UC-45 POST `{kök}/ayip-talebi?handler=SubmitDefectClaim` — Ayıp talebinin gönderimi | T513 | | |
| 07 UC-46 POST `{kök}/ayip-talebi?handler=ReopenDefectClaim` — Ayıp talebinin yeniden açılması | T513 | | |
| 07 UC-47 GET `/kayit` — Kayıt ekranı | T514 | | |
| 07 UC-48 POST `/kayit?handler=Register` — Kayıt | T514 | | |
| 07 UC-49 POST `/kayit?handler=ResendVerificationLink` ve 1 adres daha — Kayıt doğrulama bağlantısını yeniden isteme | T514 | | |
| 07 UC-50 POST `/kayit?handler=StartSocialLogin` ve 1 adres daha — Google ile giriş | T515 | | |
| 07 UC-51 GET `/giris/google` — Google dönüşü | T515 | | |
| 07 UC-52 GET `/dogrulama/{anahtar}` — Doğrulama bağlantısının inişi | T514 | | |
| 07 UC-53 POST `/dogrulama/{anahtar}?handler=VerifyEmail` — Doğrulama | T514 | | |
| 07 UC-54 GET `/giris` — Müşteri girişi ekranı | T515 | | |
| 07 UC-55 POST `/giris?handler=SignIn` — Müşteri girişi | T515 | | |
| 07 UC-56 GET `/sifre-sifirlama` — Şifre sıfırlama | T516 | | |
| 07 UC-57 POST `/sifre-sifirlama?handler=RequestPasswordReset` — Şifre sıfırlama isteği | T516 | | |
| 07 UC-58 GET `/sifre-sifirlama/{anahtar}` — Sıfırlama bağlantısının inişi | T516 | | |
| 07 UC-59 POST `/sifre-sifirlama/{anahtar}?handler=ResetPassword` — Yeni şifrenin kurulması | T516 | | |
| 07 UC-60 GET `/hesap/yeniden-dogrulama` — Yeniden doğrulama ekranı | T516 | | |
| 07 UC-61 POST `/hesap/yeniden-dogrulama?handler=Reauthenticate` — Şifreyle yeniden doğrulama | T516 | | |
| 07 UC-62 POST `/hesap/yeniden-dogrulama?handler=StartSocialReauthentication` — Google ile yeniden doğrulama | T516 | | |
| 07 UC-63 GET `/hesap` — Hesap | T517 | | |
| 07 UC-64 POST `/hesap?handler=ChangeName` — Adı değiştirme | T517 | | |
| 07 UC-65 POST `/hesap?handler=ChangePassword` — Şifreyi değiştirme | T517 | | |
| 07 UC-66 POST `/hesap?handler=ChangeEmail` — E-postayı değiştirme | T517 | | |
| 07 UC-67 POST `/hesap?handler=ResendEmailChangeLink` — Bekleyen e-posta değişikliğinin bağlantısını yeniden isteme | T517 | | |
| 07 UC-68 POST `/hesap?handler=DeleteAccount` — Hesabı silme | T517 | | |
| 07 UC-69 GET `/hesap/silindi` — Hesap silmenin sonuç hâli | T517 | | |
| 07 UC-70 GET `/hesap/adresler` — Adres defteri | T518 | | |
| 07 UC-71 POST `/hesap/adresler?handler=CreateAddressBook` — Adres ekleme | T518 | | |
| 07 UC-72 POST `/hesap/adresler?handler=UpdateAddressBook` — Adres düzenleme | T518 | | |
| 07 UC-73 POST `/hesap/adresler?handler=DeleteAddressBook` — Adres silme | T518 | | |
| 07 UC-74 GET `/hesap/siparisler` — Sipariş geçmişi | T518 | | |
| 07 UC-75 GET `/geri-al/{anahtar}` — Geri alma bağlantısının inişi | T519 | | |
| 07 UC-76 POST `/geri-al/{anahtar}?handler=RevertPendingEmailChange` — E-posta değişikliğini geri alma | T519 | | |
| 07 UC-77 POST `/cikis?handler=SignOut` — Müşteri çıkışı | T519 | | |
| 07 UC-78 — sayfa isteğinin yanıtı (500; §2.7.1) — Beklenmeyen hata sayfası | T504 | | |
| 07 UC-79 GET `/sitemap.xml` ve 1 adres daha — Site haritası ve robots.txt | T313 | | |
| 07 UC-80 POST `/sistem/odeme-bildirimi` — Ödeme sağlayıcısının bildirim ucu | T311 | | |
| 07 UC-81 GET `/sistem/canlilik` — Canlılık ucu | T01 | | |
| 07 UC-82 GET `/sistem/durum` — Durum ucu | T314 | | |
| 07 UC-83 POST `/panel/yukleme` (başlatma) ve 2 adres daha — Parçalı dosya yükleme ucu | T312 | | |
| 07 UC-84 GET `/panel/giris` ve 2 adres daha — Panel girişi ve şifre sıfırlama | T520 | | |
| 07 UC-85 GET `/panel/davet/{anahtar}` — Yönetici daveti kabul ekranı | T520 | | |
| 07 UC-86 GET `/panel` — Panel ana sayfası | T521 | | |
| 07 UC-87 GET `/panel/urunler` — Ürün listesi | T525 | | |
| 07 UC-88 GET `/panel/urunler/yeni` ve 1 adres daha — Ürün formu | T525 | | |
| 07 UC-89 GET `/panel/kategoriler` — Kategori ağacı | T526 | | |
| 07 UC-90 GET `/panel/kuponlar` ve 2 adres daha — Kuponlar | T526 | | |
| 07 UC-91 GET `/panel/siparisler` — Sipariş listesi | T527 | | |
| 07 UC-92 GET `/panel/siparisler/{iç anahtar}` — Sipariş ayrıntısı | T527 | | |
| 07 UC-93 GET `/panel/talepler` — Talepler | T532 | | |
| 07 UC-94 GET `/panel/talepler/{iç anahtar}` — İletişim talebi ayrıntısı | T532 | | |
| 07 UC-95 GET `/panel/uyeler` — Üye kaydı görünümü | T533 | | |
| 07 UC-96 GET `/panel/kurumsal-icerik` ve 2 adres daha — Kurumsal içerik | T534 | | |
| 07 UC-97 GET `/panel/ana-sayfa-ve-menu` — Ana sayfa ve menü | T535 | | |
| 07 UC-98 GET `/panel/marka` — Marka | T534 | | |
| 07 UC-99 GET `/panel/firma-kimligi-ve-satis` — Firma kimliği ve satış | T535 | | |
| 07 UC-100 GET `/panel/odeme-yontemleri` — Ödeme yöntemleri | T536 | | |
| 07 UC-101 GET `/panel/kargo-sureler-ve-varsayilanlar` — Kargo, süreler ve varsayılanlar | T536 | | |
| 07 UC-102 GET `/panel/yasal-metinler` — Yasal metinler | T507 | | |
| 07 UC-103 GET `/panel/yonetici-hesaplari` — Yönetici hesapları | T522 | | |
| 07 UC-104 GET `/panel/hesabim` — Yöneticinin kendi hesabı | T523 | | |
| 07 UC-105 GET `/panel/satis-ozeti` — Satış özeti | T524 | | |
| 07 UC-106 GET `/panel/islem-izi` — İşlem izi | T522 | | |
| 07 UC-107 GET `/panel/disa-aktarma` — Dışa aktarma | T524 | | |
| 07 UC-108 GET `/panel/yeniden-dogrulama` ve 2 adres daha — Yönetici hesabının doğrulama ekranları | T521 | | |
| 07 UC-109 POST `/panel/cikis?handler=SignOut` — Yönetici çıkışı | T520 | | |
| 07 UC-110 POST `/panel/giris?handler=SignIn` — Panel girişi | T520 | | |
| 07 UC-111 POST `/panel/sifre-sifirlama?handler=RequestPasswordReset` — Panel şifre sıfırlama isteği | T520 | | |
| 07 UC-112 POST `/panel/sifre-sifirlama/{anahtar}?handler=ResetPassword` — Panelde yeni şifrenin kurulması | T520 | | |
| 07 UC-113 POST `/panel/davet/{anahtar}?handler=AcceptAdminInvitation` — Davetle yönetici hesabını açma | T520 | | |
| 07 UC-114 POST `/panel/urunler/yeni?handler=SaveProduct` ve 1 adres daha — Ürünü kaydetme | T525 | | |
| 07 UC-115 POST `/panel/urunler/{iç anahtar}?handler=ChangeProductPublishStatus` ve 1 adres daha — Ürünün ya da varyantın yayın geçişi | T525 | | |
| 07 UC-116 POST `/panel/urunler/{iç anahtar}?handler=DeleteProduct` ve 1 adres daha — Ürünü ya da varyantı kalıcı silme | T525 | | |
| 07 UC-117 POST `…?handler=UploadImage` — Görsel yükleme | T525, T534 | | |
| 07 UC-118 POST `/panel/kategoriler?handler=CreateCategory` — Kategori ekleme | T526 | | |
| 07 UC-119 POST `/panel/kategoriler?handler=RenameCategory` — Kategoriyi yeniden adlandırma | T526 | | |
| 07 UC-120 POST `/panel/kategoriler?handler=MoveCategory` — Kategoriyi taşıma | T526 | | |
| 07 UC-121 POST `/panel/kategoriler?handler=DeleteCategory` — Kategoriyi silme | T526 | | |
| 07 UC-122 POST `/panel/kuponlar/yeni?handler=SaveCoupon` ve 1 adres daha — Kuponu kaydetme | T526 | | |
| 07 UC-123 POST `/panel/siparisler/{iç anahtar}?handler=MarkPaid` — Havale siparişini "Ödendi" işaretleme | T527 | | |
| 07 UC-124 POST `/panel/siparisler/{iç anahtar}?handler=MarkShipped` — Kargoya verme | T527 | | |
| 07 UC-125 POST `/panel/siparisler/{iç anahtar}?handler=MarkReshipped` — Kargoya yeniden verme | T527 | | |
| 07 UC-126 POST `/panel/siparisler/{iç anahtar}?handler=MarkDelivered` — Teslim işareti ve tarihi | T527 | | |
| 07 UC-127 POST `/panel/siparisler/{iç anahtar}?handler=MarkDeliveryFailed` — "Teslim edilemedi" işareti | T527 | | |
| 07 UC-128 POST `/panel/siparisler/{iç anahtar}?handler=CloseDeliveryFailed` — Teslim edilemedi siparişini kapatma | T527 | | |
| 07 UC-129 POST `/panel/siparisler/{iç anahtar}?handler=CancelOrderByCompany` — Firma iptali | T528 | | |
| 07 UC-130 POST `/panel/siparisler/{iç anahtar}?handler=RecordWithdrawal` ve 1 adres daha — Başka kanaldan gelen cayma ya da gecikme feshi bildiriminin kaydı | T528 | | |
| 07 UC-131 POST `/panel/siparisler/{iç anahtar}?handler=ProcessRefund` — Geri ödemeyi işleme | T529 | | |
| 07 UC-132 POST `/panel/siparisler/{iç anahtar}?handler=MarkBankTransferRefundFailed` — "Havale gerçekleşmedi" | T529 | | |
| 07 UC-133 POST `/panel/siparisler/{iç anahtar}?handler=ProcessAmountBasedRefund` — Tutar bazlı kısmi geri ödeme | T529 | | |
| 07 UC-134 POST `/panel/siparisler/{iç anahtar}?handler=RequestRefundIban` — IBAN isteme | T529 | | |
| 07 UC-135 POST `/panel/siparisler/{iç anahtar}?handler=RetryCardRefund` — Kart iadesini yeniden deneme | T529 | | |
| 07 UC-136 POST `/panel/siparisler/{iç anahtar}?handler=SwitchRefundToBankTransfer` — Havale yolunu açma | T529 | | |
| 07 UC-137 POST `/panel/siparisler/{iç anahtar}?handler=MarkCardRefundFailed` — Sonucu beklenen kart iadesini "gerçekleşmedi" işaretleme | T529 | | |
| 07 UC-138 POST `/panel/siparisler/{iç anahtar}?handler=RemoveOrderItem` — Kalem çıkarma ya da adet azaltma | T528 | | |
| 07 UC-139 POST `/panel/siparisler/{iç anahtar}?handler=CompleteServiceItem` — Hizmet kalemini tamamlama | T531 | | |
| 07 UC-140 POST `/panel/siparisler/{iç anahtar}?handler=ReceiveReturn` ve 1 adres daha — İade malını teslim alma ya da reddetme | T528 | | |
| 07 UC-141 POST `/panel/siparisler/{iç anahtar}?handler=RestockReturn` — İade malını stoğa ekleme | T528 | | |
| 07 UC-142 POST `/panel/siparisler/{iç anahtar}?handler=CloseNotReturned` — "Mal dönmedi" kapanışı | T528 | | |
| 07 UC-143 POST `/panel/siparisler/{iç anahtar}?handler=ResetDownloadCount` — İndirme hakkını yenileme | T531 | | |
| 07 UC-144 POST `/panel/siparisler/{iç anahtar}?handler=ResolveDefectClaim` — Ayıp talebini çözüldü işaretleme | T528 | | |
| 07 UC-145 POST `/panel/siparisler/{iç anahtar}?handler=ReopenDefectClaim` — Ayıp talebini yeniden açma | T528 | | |
| 07 UC-146 POST `/panel/siparisler/{iç anahtar}?handler=CorrectAddress` — Adres düzeltmesi | T530 | | |
| 07 UC-147 POST `/panel/siparisler/{iç anahtar}?handler=CorrectCarrier` — Kargo şirketi ve takip numarası düzeltmesi | T530 | | |
| 07 UC-148 POST `/panel/siparisler/{iç anahtar}?handler=CorrectDeliveryDate` — Teslim tarihi düzeltmesi | T530 | | |
| 07 UC-149 POST `/panel/siparisler/{iç anahtar}?handler=CorrectArrivalDate` — Ulaşma tarihi düzeltmesi | T530 | | |
| 07 UC-150 POST `/panel/siparisler/{iç anahtar}?handler=CorrectTransition` — Durum geçişi düzeltmesi | T530 | | |
| 07 UC-151 POST `/panel/siparisler/{iç anahtar}?handler=CorrectOrderEmail` — Misafir siparişinin e-postasını düzeltme | T530 | | |
| 07 UC-152 POST `/panel/siparisler/{iç anahtar}?handler=SaveInternalNote` — İç notu kaydetme | T531 | | |
| 07 UC-153 POST `/panel/siparisler/{iç anahtar}?handler=ResendNotification` — Ulaşmayan e-postayı yeniden gönderme | T531 | | |
| 07 UC-154 POST `/panel/siparisler/{iç anahtar}?handler=ResendAllNotifications` — Ulaşmayan bütün e-postaları yeniden gönderme | T531 | | |
| 07 UC-155 POST `/panel/talepler/{iç anahtar}?handler=CloseContactRequest` — İletişim talebini kapatma | T532 | | |
| 07 UC-156 POST `/panel/talepler/{iç anahtar}?handler=ReopenContactRequest` — İletişim talebini yeniden açma | T532 | | |
| 07 UC-157 POST `/panel/uyeler?handler=DeleteCustomer` — Üye hesabını silme | T533 | | |
| 07 UC-158 POST `/panel/kurumsal-icerik/{tip}/yeni?handler=SaveContent` ve 3 adres daha — İçerik kaydını kaydetme | T534 | | |
| 07 UC-159 POST `/panel/kurumsal-icerik/{tip}/{iç anahtar}?handler=ChangePublishStatus` — İçerik kaydının yayın geçişi | T534 | | |
| 07 UC-160 POST `/panel/kurumsal-icerik/{tip}/{iç anahtar}?handler=DeleteContent` — İçerik kaydını kalıcı silme | T534 | | |
| 07 UC-161 POST `/panel/kurumsal-icerik?handler=ReorderContent` — İçerik kaydının elle sırası | T534 | | |
| 07 UC-162 POST `/panel/ana-sayfa-ve-menu?handler=SaveHomepage` — Ana sayfa ve menü ayarlarını kaydetme | T535 | | |
| 07 UC-163 POST `/panel/marka?handler=SaveBranding` — Marka ayarlarını kaydetme | T534 | | |
| 07 UC-164 POST `/panel/firma-kimligi-ve-satis?handler=SaveCompany` — Firma kimliğini kaydetme | T535 | | |
| 07 UC-165 POST `/panel/firma-kimligi-ve-satis?handler=SuspendSales` — Satışı geçici olarak kapatma | T535 | | |
| 07 UC-166 POST `/panel/firma-kimligi-ve-satis?handler=ResumeSales` — Satışı yeniden açma | T535 | | |
| 07 UC-167 POST `/panel/odeme-yontemleri?handler=SavePaymentMethods` — Ödeme yöntemlerini kaydetme | T536 | | |
| 07 UC-168 POST `/panel/kargo-sureler-ve-varsayilanlar?handler=SaveShippingFee` — Kargo ücreti ve eşikleri kaydetme | T536 | | |
| 07 UC-169 POST `/panel/kargo-sureler-ve-varsayilanlar?handler=SaveDeliveryProvinces` — Teslimat illerini kaydetme | T536 | | |
| 07 UC-170 POST `/panel/kargo-sureler-ve-varsayilanlar?handler=SavePeriods` — Süreleri kaydetme | T536 | | |
| 07 UC-171 POST `/panel/kargo-sureler-ve-varsayilanlar?handler=SaveProductDefaults` — Ürün varsayılanlarını kaydetme | T536 | | |
| 07 UC-172 POST `/panel/kargo-sureler-ve-varsayilanlar?handler=SaveReturnAddress` — İade adresini kaydetme | T536 | | |
| 07 UC-173 POST `/panel/yasal-metinler?handler=SaveLegalTextDraft` — Yasal metnin taslağını kaydetme | T507 | | |
| 07 UC-174 POST `/panel/yasal-metinler?handler=PublishLegalText` — Yasal metni yayına alma | T507 | | |
| 07 UC-175 POST `/panel/yonetici-hesaplari?handler=SendAdminInvitation` — Yönetici daveti gönderme | T522 | | |
| 07 UC-176 POST `/panel/yonetici-hesaplari?handler=RevokeAdminInvitation` — Daveti geri çekme | T522 | | |
| 07 UC-177 POST `/panel/yonetici-hesaplari?handler=RemoveAdmin` — Yöneticiyi kaldırma | T522 | | |
| 07 UC-178 POST `/panel/hesabim?handler=ChangeName` — Yöneticinin adını değiştirme | T523 | | |
| 07 UC-179 POST `/panel/hesabim?handler=ChangeEmail` — Yöneticinin e-postasını değiştirme | T523 | | |
| 07 UC-180 POST `/panel/hesabim?handler=ResendEmailChangeLink` — Yönetici e-posta değişikliğinin bağlantısını yeniden isteme | T523 | | |
| 07 UC-181 POST `/panel/hesabim?handler=ChangePassword` — Yöneticinin şifresini değiştirme | T523 | | |
| 07 UC-182 POST `/panel/disa-aktarma?handler=ExportOrders` — Siparişlerin dışa aktarılması | T524 | | |
| 07 UC-183 POST `/panel/disa-aktarma?handler=ExportCustomers` — Üye listesinin dışa aktarılması | T524 | | |
| 07 UC-184 POST `/panel/disa-aktarma?handler=ExportContactRequests` — İletişim taleplerinin dışa aktarılması | T524 | | |
| 07 UC-185 POST `/panel/yeniden-dogrulama?handler=Reauthenticate` — Yöneticinin şifreyle yeniden doğrulaması | T521 | | |
| 07 UC-186 POST `/panel/dogrulama/{anahtar}?handler=VerifyEmail` — Yöneticinin yeni adresinin doğrulanması | T521 | | |
| 07 UC-187 POST `/panel/geri-al/{anahtar}?handler=RevertPendingEmailChange` — Yöneticinin e-posta değişikliğini geri alması | T521 | | |
| 07:§5.1 Okunuş ve bağlama | T211 | | |
| 07:§5.2 Form girdi modelleri | T538 | | |
| 07:§5.3 Sorgu modelleri | T538 | | |
| 07:§5.4 Makine uçlarının gövdeleri | T312, T538 | | |
| 07:§5.5 Çerçevenin modeli | T537 | | |
| 07:§6.1 Okunuş — kodun biçimi | T212 | | |
| 07:§6.2 Hata kodları | T213 | | |
| 07:§6.3 Kısa süreli bildirim kodları | T214 | | |
| 07:§6.4 Özel hâller | T212 | | |
| 07:§8.1 Yapılabilir işlemler sunucuda | T217 | | |
| 07:§8.2 JavaScript kapalıyken temel akışlar | T539 | | |

#### 7.1.3 Entegrasyon → task

| Kaynak öğe | Task | Implement edildi | Kanıt |
|---|---|---|---|
| 08:§2.1 Ödeme sağlayıcısı — PayTR | T401 | | |
| 08:§2.1.1 Sağlayıcının seçimi | T401 | | |
| 08:§2.1.2 Anahtarlar ve değişim | T401 | | |
| 08:§2.1.3 3D Secure | T406 | | |
| 08:§2.1.4 Ödemenin başlatılması, dönüş ve çerçeve | T401, T510 | | |
| 08:§2.1.5 Bildirimin kabulü ve alan eşlemesi | T402, T311 | | |
| 08:§2.1.6 Son sorgu | T403 | | |
| 08:§2.1.7 Tek tahsilat | T403 | | |
| 08:§2.1.8 Bağlantı hatası ve ayrım | T404 | | |
| 08:§2.1.9 Kart iadesi | T405 | | |
| 08:§2.1.10 Test ve canlı kanıt | T406 | | |
| 08:§2.2 E-posta gönderim altyapısı — Amazon SES | T407 | | |
| 08:§2.2.1 Servisin seçimi | T407 | | |
| 08:§2.2.2 Gönderim ve sonuç sınıfları | T407 | | |
| 08:§2.2.3 Geri dönen adreslerin öğrenilmesi | T408 | | |
| 08:§2.2.4 Alıcı başına gönderim sınırı ve kaçış | T304 | | |
| 08:§2.2.5 Ortak kalıp | T305 | | |
| 08:§2.2.6 Müşteri bildirimleri B-1…B-16 | T306, T307 | | |
| 08:§2.2.7 Firma bildirimleri F-1…F-6 | T308 | | |
| 08:§2.2.8 Hesap ve yönetim e-postaları | T309 | | |
| 08:§2.3 Google ile giriş, kayıt ve yeniden doğrulama | T409 | | |
| 08:§2.4 Çağrısı olmayan dış bağlantılar — kargo şirketi listesi | T310 | | |
| 08:§3.1 Bağlantı katmanının ortak yüzü | T202 | | |
| 08:§3.2 İmzalı makine uçlarının ortak ilkesi | T203 | | |
| 08:§3.3 Bir dış servis çökerse | T204 | | |
| 08:§4.1 Dış olgu varsayımları | T410 | | |
| 08:§4.2 Dış sistemde kayıtlı adresler | T601 | | |
| 08:§4.3 Ürünle gelen dış kaynaklı varlık — yerli üretim logosu | T501 | | |
| 08:§5.1 `07`'ye geri yansıyan uçlar | — | | |
| 08:§5.2 Üst dokümanlara geri besleme | T410 | | |

#### 7.1.4 Arayüz → task

Kaynak: `04` (102: ekran 53 `04:E-nn` · ortak bileşen 20 `04:OB-nn` · alt bölüm 29 `04:§n.m`) ve `03` (55 alt bölüm, `03:§n.m`). Ekran birimleri O2'nin F5 ekran gruplu task'larına (T502–T539) bağlandı; ortak bileşenler, gezinme ve form eşitliği için F5'te sekiz, durum makinesi, süreler ve bildirim eşlemesi için F3'te on beş aday açıldı (K-1418).

| Kaynak öğe | Task | Implement edildi | Kanıt |
|---|---|---|---|
| 04:E-01 Ana sayfa | T502 | | |
| 04:E-02 Kategori sayfası | T502 | | |
| 04:E-03 Arama sonuçları | T503 | | |
| 04:E-04 Ürün sayfası | T503 | | |
| 04:E-05 Arşivlenmiş ürün sayfası | T549 | | |
| 04:E-06 "Sayfa bulunamadı" sayfası | T504 | | |
| 04:E-07 İçerik liste sayfası | T505 | | |
| 04:E-08 İçerik sayfası | T505 | | |
| 04:E-09 Sık sorulan sorular sayfası | T506 | | |
| 04:E-10 İletişim sayfası ve formu | T506 | | |
| 04:E-11 Yasal metin sayfası | T507 | | |
| 04:E-12 Sepet | T508 | | |
| 04:E-13 Ödeme adımı | T509 | | |
| 04:E-14 Sipariş teyit ve bekleme ekranı | T510 | | |
| 04:E-15 Sipariş takibi girişi | T511 | | |
| 04:E-16 Sipariş sayfası | T511, T512 | | |
| 04:E-17 İptal ve gecikme feshi ekranı | T512 | | |
| 04:E-18 Cayma beyanı ekranı | T513 | | |
| 04:E-19 Ayıp talebi ekranı | T513 | | |
| 04:E-20 Kayıt ekranı | T514 | | |
| 04:E-21 Doğrulama bağlantısının iniş ekranı | T514 | | |
| 04:E-22 Müşteri girişi | T515 | | |
| 04:E-23 Şifre sıfırlama ekranları | T516 | | |
| 04:E-24 Yeniden doğrulama ekranı | T516 | | |
| 04:E-25 Hesap — profil ve güvenlik | T517 | | |
| 04:E-26 Adres defteri | T518 | | |
| 04:E-27 Sipariş geçmişi | T518 | | |
| 04:E-28 E-posta değişikliğini geri alma ekranı | T519 | | |
| 04:E-29 Panel girişi ve şifre sıfırlama | T520 | | |
| 04:E-30 Yönetici daveti kabul ekranı | T520 | | |
| 04:E-31 Panel ana sayfası | T521 | | |
| 04:E-32 Ürün listesi | T525 | | |
| 04:E-33 Ürün formu | T525 | | |
| 04:E-34 Kategori ağacı | T526 | | |
| 04:E-35 Kuponlar | T526 | | |
| 04:E-36 Sipariş listesi | T527 | | |
| 04:E-37 Sipariş ayrıntısı | T527, T528, T529, T530, T531 | | |
| 04:E-38 Talepler | T532 | | |
| 04:E-39 İletişim talebi ayrıntısı | T532 | | |
| 04:E-40 Üye kaydı görünümü | T533 | | |
| 04:E-41 Kurumsal içerik | T534 | | |
| 04:E-42 Ana sayfa ve menü | T535 | | |
| 04:E-43 Marka | T534 | | |
| 04:E-44 Firma kimliği ve satış | T535 | | |
| 04:E-45 Ödeme yöntemleri | T536 | | |
| 04:E-46 Kargo, süreler ve varsayılanlar | T536 | | |
| 04:E-47 Yasal metinler | T507 | | |
| 04:E-49 Yönetici hesapları | T522 | | |
| 04:E-50 Yöneticinin kendi hesabı | T523 | | |
| 04:E-51 Satış özeti | T524 | | |
| 04:E-52 İşlem izi | T522 | | |
| 04:E-53 Dışa aktarma | T524 | | |
| 04:E-54 Yönetici hesabının doğrulama ekranları | T521 | | |
| 04:OB-01 Tasarım tabanı | T540 | | |
| 04:OB-02 Çerçeve | T540, T537 | | |
| 04:OB-03 Mesaj | T541 | | |
| 04:OB-04 Onay | T541 | | |
| 04:OB-05 Rozet ve işaretler | T542 | | |
| 04:OB-06 Sayaç ve süre göstergeleri | T542 | | |
| 04:OB-07 Boş, yükleniyor ve hata hâlleri | T541 | | |
| 04:OB-08 Satış kapalı hâli | T541 | | |
| 04:OB-09 "Taslak" bandı ve yönetici şeridi | T540 | | |
| 04:OB-10 Eşzamanlı düzenleme uyarısı | T542 | | |
| 04:OB-11 Ürün kartı | T545 | | |
| 04:OB-12 Form alanı ve form düzeni | T543 | | |
| 04:OB-13 Adres formu | T543 | | |
| 04:OB-14 IBAN alanı | T543 | | |
| 04:OB-15 Sebep seçimi | T543 | | |
| 04:OB-16 Tarih alanı | T544 | | |
| 04:OB-17 Kupon alanı | T544 | | |
| 04:OB-18 Onay kutuları | T544 | | |
| 04:OB-19 Görsel ve dosya yükleme | T544 | | |
| 04:OB-20 Liste ve tablo | T545 | | |
| 04:§2.1 Tasarım tabanı | T540 | | |
| 04:§2.2 Çerçeve | T540, T537 | | |
| 04:§2.3 Mesaj | T541 | | |
| 04:§2.4 Onay | T541 | | |
| 04:§2.5 Rozet ve işaretler | T542 | | |
| 04:§2.6 Sayaç ve süre göstergeleri | T542 | | |
| 04:§2.7 Boş, yükleniyor ve hata hâlleri | T541 | | |
| 04:§2.8 Satış kapalı hâli | T541 | | |
| 04:§2.9 "Taslak" bandı ve yönetici şeridi | T540 | | |
| 04:§2.10 Eşzamanlı düzenleme uyarısı | T542 | | |
| 04:§2.11 Ürün kartı | T545 | | |
| 04:§2.12 Form bileşenleri | T543, T544 | | |
| 04:§2.13 Liste ve tablo | T545 | | |
| 04:§3.1 Vitrinin gezinme iskeleti | T540 | | |
| 04:§3.2 Panelin gezinme iskeleti | T540 | | |
| 04:§3.3 Müşteri tarafının geçişleri | T546 | | |
| 04:§3.4 Panelin geçişleri | T546 | | |
| 04:§3.5 Sitenin dışından girişler | T546 | | |
| 04:§3.6 Girişten ve çıkıştan dönüş | T546 | | |
| 04:§3.7 Panel ile vitrin arasındaki geçişler | T546 | | |
| 04:§6.1 Kapsam ve okunuş — durum taşıyan ekranlar | T547 | | |
| 04:§6.2 Müşteri tarafı | T547 | | |
| 04:§6.3 Panel | T547 | | |
| 04:§7.1 Müşteri tarafının formları | T547 | | |
| 04:§7.2 Panelin formları | T547 | | |
| 04:§7.3 Ortak alan kuralları | T543 | | |
| 04:§8.1 Tek dil ve dil seçiminin yokluğu | T216 | | |
| 04:§8.2 Türkçenin biçimleri — tarih, sayı, para | T216 | | |
| 04:§8.3 Metin uzunluğunun yerleşime etkisi | T540 | | |
| 03:§1.1 Envanter | T315 | | |
| 03:§1.2 Sevkiyat ekseni — Sipariş durumu | T316 | | |
| 03:§1.3 Ödeme ekseni — Ödeme durumu | T317 | | |
| 03:§1.4 İki eksenin bağı | T318 | | |
| 03:§1.5 Tipe göre hatlar | T318 | | |
| 03:§1.6 Geri alınamaz geçişler, onay pencereleri ve düzeltme | T319 | | |
| 03:§1.7 Kalem kayıtları ve panel işaretleri | T320, T370, T385, T387 | | |
| 03:§1.8 Yayın durumları | T321, T322 | | |
| 03:§1.9 Talep durumları | T320, T322 | | |
| 03:§1.10 Durum makinesi olmayan varlıklar | T315 | | |
| 03:§1.11 Durum × rol × işlem | T323, T217 | | |
| 03:§2.1 Ziyaretçi: vitrinde gezinme ve ürünü bulma | T502, T503 | | |
| 03:§2.2 Ziyaretçi: kurumsal içerik ve iletişim formu | T505, T506, T537 | | |
| 03:§2.3 Müşteri: sepet | T508 | | |
| 03:§2.4 Müşteri: ödeme adımı ve sipariş onayı | T509, T510 | | |
| 03:§2.5 Müşteri: ödeme ve teslim | T311, T316, T317, T401, T403, T510 | | |
| 03:§2.6 Müşteri: sipariş takibi (akış 2) | T511, T518 | | |
| 03:§2.7 Müşteri: iptal ve gecikme feshi | T405, T512 | | |
| 03:§2.8 Müşteri: cayma ve iade | T405, T513, T527, T530 | | |
| 03:§2.9 Müşteri: ayıp talebi | T513 | | |
| 03:§2.10 Uçtan uca anlatılar | T329, T390 | | |
| 03:§3.1 Bütün akışlara uygulanan ilkeler | T212, T541 | | |
| 03:§3.2 Satın alma ve sipariş takibi | T403, T404, T405, T508, T509, T510, T511 | | |
| 03:§3.3 İptal, cayma ve iade (akış 3) | T405, T512, T513 | | |
| 03:§3.4 Üyelik (akış 4) | T514, T515, T516, T517, T519, T533 | | |
| 03:§3.5 Firma tarafı | T520, T522, T525, T526, T527, T528, T529, T530, T531, T534, T535, T536 | | |
| 03:§4.1 Süre envanterinden zaman aşımı akışları | T201, T301, T302, T324, T325, T326, T368, T527 | | |
| 03:§4.2 Kendiliğinden işleyen anlar | T327 | | |
| 03:§5.1 Üç yol ve itirazın yeri | T511, T512, T513, T506 | | |
| 03:§5.2 Ürünün dışında çözülen anlaşmazlıklar | T506, T512, T528, T529, T530 | | |
| 03:§5.3 İade reddi ve değer kaybı | T320, T512, T528, T529 | | |
| 03:§6.1 Deneme limitleri | T215, T376, T377, T509, T515, T516 | | |
| 03:§6.2 Teyit ve kandırılma akışları | T519, T522, T527, T530, T536 | | |
| 03:§6.3 Firmanın inceleme akışları | T521, T522, T524, T527, T536 | | |
| 03:§7.1 Bildirimler ve tetikleyicileri | T306, T307, T308, T309, T328, T385 | | |
| 03:§7.2 Bildirim üretmeyen olaylar ve olmayan bildirimler | T328 | | |
| 03:§7.3 Geçiş ve olay bazlı eşleme | T328 | | |
| 03:§8.1 Katalog yönetimi (akış 5) | T312, T340, T503, T525, T526, T540 | | |
| 03:§8.2 Sipariş yürütümü: olağan hat (akış 6) | T316, T318, T527, T531 | | |
| 03:§8.3 Firma iptali, iade teslim alma ve geri ödeme | T405, T528, T529 | | |
| 03:§8.4 Sipariş müdahaleleri | T528, T530, T531 | | |
| 03:§8.5 Manuel adım bütçesi | T318 | | |
| 03:§8.6 Kurumsal içerik, marka, duyuru ve iletişim talepleri (akış 7'nin panel yüzü) | T532, T534, T535 | | |
| 03:§8.7 Mağaza ayarları, satış kapısı ve kurulum kontrol listesi (akış 8) | T507, T521, T535, T536 | | |
| 03:§8.8 Yönetici hesapları ve davet | T520, T522 | | |
| 03:§8.9 Bekleyen işler, satış özeti, işlem izi ve dışa aktarma | T384, T385, T521, T522, T524, T531 | | |
| 03:§9.1 Kayıt, e-posta doğrulama ve Google ile ilk giriş | T409, T514, T515 | | |
| 03:§9.2 Giriş, oturum, şifre sıfırlama ve yeniden doğrulama | T515, T516, T519, T520 | | |
| 03:§9.3 Profil ve adres defteri | T517, T518, T519, T523 | | |
| 03:§9.4 Hesabın silinmesi ve kişisel veri başvurusu | T517, T533 | | |
| 03:§9.5 Tercih yönetimi | — | | |
| 03:§10.1 Dış servis kesintileri | T204, T382, T385, T403, T404, T405, T407, T409, T509, T510, T521 | | |
| 03:§10.2 Bakım ve güncelleme | T331, T360, T534, T535 | | |
| 03:§10.3 Sitenin kesintisi | T327 | | |
| 03:§10.4 Panele erişimin kaybı | T303, T520 | | |

#### 7.1.5 İş kuralı → task

> Doldurulacak: O4a (`02 §1`–§3; yazıldı), O4b 1/2 (`02 §4`–§8; yazıldı), O4b 2/2 (`02 §9`–§13; yazıldı), O5 1/2 (`05`; yazıldı), O5 2/2 (`09`, `10`, devir; yazıldı). Beş kalın etiketli grup, her birinin kendi tablosu: **`02`** (O4a: §1–§3; O4b: §4–§13) · **`05`** · **`09`** · **`10`** · **Devir satırları** (O5).

Kaynak: `02 §1`–§3 (297: iş kuralı 257 `02:§3.n.m` · alt bölüm 40 `02:§n.m` — §1'in 3'ü, §2'nin 3'ü, §3'ün 34'ü). Kural satırı kuralın kalın başlık cümlesidir; kuralın iş mantığı F3'ün modül task'larındadır (T330–T366, K-1421), ekranda görünen yüzü F5'in ekran task'larındadır — bir kural birden çok task alabilir. `02 §3.26` ve `§1.1`–§1.2 task gerektirmez (gerekçesi yukarıda).

Kaynak (O4b 1/2): `02 §4`–§8 (193: iş kuralı 101 `02:§n.m.k` · kimlikli satır 56 — Z-1…Z-47 ve L-1…L-9 · alt bölüm 36 `02:§n.m`; §4 55 · §5 31 · §6 15 · §7 50 · §8 42). `§6`'nın `6.2.n`…`6.9.n` senaryo satırları birim değildir: alt bölüm satırı o bölümün task birleşimini taşır. `§5.4` ve `§5.5`'in geçiş tabloları (S1–S11, Ö1–Ö5) T315–T319'a bağlıdır (K-1419 (5)); yeni durum makinesi task'ı açılmadı. `§4.2`'nin Z satırları süre akışlarına T324–T326'dan (O3), saklama satırları T201, T301, T302'den gelir. İptal, cayma, iade ve ayıp (`§7`) için F3'ün modül task'ları T367–T375'tir (K-1425); sınır deneme limitleri ve zaman sayımı T376–T378'dir. Kuralın ekranda görünen yüzü F5 task'larındadır (T511–T513, T528–T531, T541, T543). **Task gerektirmeyen iki birim:** `02:Z-25` (KVKK başvurusuna cevap, 30 gün) ve `02:Z-37` (veri ihlalinde Kurul'a bildirim, 72 saat) — satır "sistemde sayaç tutulmaz; yükümlülük firmanındır" der, ürün bir süre işletmez; "Task" hücresi `—` (K-1426). GAP değildir. `02:§3.26`'nın işaret ettiği kurallar `§7.2`–§7.5 satırlarında eşlendi.

**`02`** — `§1`–§8 (O4a: `§1`–§3; O4b 1/2: `§4`–§8)

| Kaynak öğe | Task | Implement edildi | Kanıt |
|---|---|---|---|
| 02:§1.1 Sözlük kuralı | — | | |
| 02:§1.2 Terim sözlüğü | — | | |
| 02:§1.3 Aktörler | T106, T107, T207 | | |
| 02:§2.1 Akış envanteri | T329, T511, T512, T513, T514, T525, T527, T534, T536 | | |
| 02:§2.2 Uçtan uca ana akış | T329, T509, T510 | | |
| 02:§2.3 Kurumsal içerik akışı | T358, T360, T361, T534 | | |
| 02:§3.1 Firma, kurulum ve satış kapısı | T330, T331 | | |
| 02:§3.1.1 Uygulama kurulum başına tek bir firmaya hizmet eder | T330, T103 | | |
| 02:§3.1.2 Firmanın kimliği panelden yönetilir; altyapısı kurulumdan gelir | T330, T535 | | |
| 02:§3.1.3 Firma tipi seçilir ve zorunlu kimlik alanları tipe göre değişir | T330, T535 | | |
| 02:§3.1.4 Yasal kimlik bilgileri sitede sürekli erişilebilir durur | T330, T506, T537 | | |
| 02:§3.1.5 Satış yalnız dört koşul birlikte sağlandığında açıktır | T331, T503, T509, T535 | | |
| 02:§3.1.6 Firma satışı panelden geçici olarak kapatabilir | T331, T508, T535 | | |
| 02:§3.1.7 Firmanın iade adresi zorunlu bir firma ayarıdır | T330, T513, T535 | | |
| 02:§3.2 Satış modeli ve ürün tipleri | T332 | | |
| 02:§3.2.1 Satış yalnız doğrudandır | T332, T503 | | |
| 02:§3.2.2 Katalog üç tipte ürün taşır: fiziksel ürün, dijital ürün ve hizmet | T332, T525 | | |
| 02:§3.2.3 Tip ürün düzleminde yaşar ve üç değerli kapalı bir alandır | T332 | | |
| 02:§3.3 Ürün, varyant ve seçenek | T332 | | |
| 02:§3.3.1 Satılan birim varyanttır; ürün vitrin ve katalog birimidir | T332, T503 | | |
| 02:§3.3.2 Bir ürün en fazla iki seçenek boyutu taşır | T332, T525 | | |
| 02:§3.3.3 İki boyutun birleşim matrisinin tam olması gerekmez | T332, T503 | | |
| 02:§3.3.4 Her varyant zorunlu ve tekil bir stok kodu (`SKU`) taşır; | T332, T525 | | |
| 02:§3.3.5 Ürün başına asgari adet yoktur | T332 | | |
| 02:§3.3.6 Ürün sayfasında yorum, puan ve öneri bloğu yoktur | T332, T503 | | |
| 02:§3.4 Kategori | T333 | | |
| 02:§3.4.1 Kategori ağacı en fazla üç seviyedir | T333, T526 | | |
| 02:§3.4.2 Ürün ağacın herhangi bir düğümüne asılabilir; | T333, T502 | | |
| 02:§3.4.3 Kategori sayfası alt dallarındaki ürünleri de gösterir | T333, T502 | | |
| 02:§3.4.4 Bir ürün birden fazla kategoriye asılabilir; asıldığı kategorilerden biri ana kategoridir | T333, T525 | | |
| 02:§3.4.5 İçinde ürün ya da alt kategori bulunan kategori silinemez | T333, T526 | | |
| 02:§3.4.6 Kategori taşımak serbesttir, iki sınırla | T333, T526 | | |
| 02:§3.5 Arama, süzgeç ve liste düzeni | T333 | | |
| 02:§3.5.1 Site içi arama yalnız ürün adında çalışır | T333, T503 | | |
| 02:§3.5.2 Ürün listeleri iki sabit eksende süzülür: fiyat aralığı ve stok durumu | T333, T502, T503 | | |
| 02:§3.5.3 Ziyaretçiye sıralama seçeneği sunulmaz | T333, T502, T503 | | |
| 02:§3.6 Stok ve tükenme | T334 | | |
| 02:§3.6.1 Stok, varyantın sayısal adedidir | T334, T525 | | |
| 02:§3.6.2 Stok takibi tipe göre değişir | T334, T525 | | |
| 02:§3.6.3 Tükenmiş varyant seçenek listesinde "Tükendi" işaretiyle görünür ve seçilemez; | T334, T503 | | |
| 02:§3.6.4 Bütün varyantları tükenmiş ürün vitrinde kalır, | T334, T503 | | |
| 02:§3.6.5 Stok adedi ziyaretçiye hiçbir yerde gösterilmez | T334, T503 | | |
| 02:§3.6.6 İptal edilen adet stoğa kendiliğinden döner; iade edilen fiziksel adedi firma teslim alıp kontrol ettikten sonra panelden ekler | T334, T528 | | |
| 02:§3.6.7 Stok ve hizmet kontenjanı, ödemesi beklenen siparişlerin ayırdığı adedin altına indirilemez | T334, T525 | | |
| 02:§3.7 Ürünün yayın durumu, arşiv ve silme | T335 | | |
| 02:§3.7.1 Ürün ve varyant yayın durumunu ayrı ayrı taşır: Taslak, Yayında, Arşiv | T335, T321, T525 | | |
| 02:§3.7.2 Arşiv silme değildir | T335 | | |
| 02:§3.7.3 Bir ürün yayına ancak adı, ana kategorisi ve tipi girilmişse ve en az bir Yayında varyantı varsa alınır | T335, T525 | | |
| 02:§3.7.4 Dijital ürünün yayın kapısı bir koşul daha taşır | T335, T340, T525 | | |
| 02:§3.7.5 Taslak ürünün adresi ziyaretçiye "sayfa bulunamadı" döner; giriş yapmış firma yöneticisine ürün sayfasını "Taslak" bandıyla gösterir | T335, T503 | | |
| 02:§3.7.6 Arşivlenmiş ürünün adresi çalışmaya devam eder ve "Bu ürün artık satılmıyor" sayfası döner | T335, T503 | | |
| 02:§3.7.7 Yönetici ürünü kalıcı olarak silebilir | T335, T525 | | |
| 02:§3.7.8 Yayındaki bir ürünün son Yayında varyantı arşivlenemez, taslağa çekilemez ve kalıcı olarak silinemez | T335, T525 | | |
| 02:§3.8 Fiyat ve KDV | T336 | | |
| 02:§3.8.1 Fiyat varyantta yaşar ve KDV dahil girilir | T336, T525 | | |
| 02:§3.8.2 KDV oranı ürün düzleminde tutulur ve varyantlar devralır; | T336, T525 | | |
| 02:§3.8.3 KDV kalem bazında hesaplanır ve kuruşa orada yuvarlanır | T336, T355 | | |
| 02:§3.8.4 Fiyatın hassasiyeti kuruştur — iki ondalık; | T336, T525 | | |
| 02:§3.8.5 Ürün sayfası fiyat etiketinin zorunlu bilgilerini taşır | T336, T501, T503, T525 | | |
| 02:§3.9 İndirim ve referans fiyat | T337 | | |
| 02:§3.9.1 İndirim ürün düzleminde yüzde olarak tanımlanır ve bütün varyantlara iner; | T337, T525 | | |
| 02:§3.9.2 İndirim tarihlidir | T337, T502, T503 | | |
| 02:§3.9.3 İndirim beyanı referans fiyata dayanır ve referansı sistem hesaplar; | T337 | | |
| 02:§3.9.4 10 günlük geçmişi olmayan malda referans, | T337 | | |
| 02:§3.9.5 İndirimli fiyat referansın altına inmiyorsa sistem indirimi kaydetmez | T337, T525 | | |
| 02:§3.9.6 Beyan edilen referans fiyat sipariş kalemine donar | T337, T355 | | |
| 02:§3.10 Kupon | T338 | | |
| 02:§3.10.1 Kupon, ödeme adımında girilen ve sepet toplamına inen bir koddur | T338, T509 | | |
| 02:§3.10.2 Kupon yüzde ya da sabit tutar olarak tanımlanır | T338, T526 | | |
| 02:§3.10.3 Kuponun payı kalem kalem belirlenir | T338 | | |
| 02:§3.10.4 Kupon siparişi sıfır tutara indiremez: sepet toplamına eşit ya da onu aşan kupon reddedilir | T338, T526 | | |
| 02:§3.10.5 Kuponun sınırı tarih aralığı ve toplam kullanım adedidir | T338, T526 | | |
| 02:§3.10.6 Kuponun kullanım hakkı stok gibi ayrılır | T338, T348, T526 | | |
| 02:§3.10.7 Kupon, ücretsiz kargo eşiğinin ve asgari sipariş tutarının tabanını düşürmez; | T338, T349 | | |
| 02:§3.11 Ürün görseli ve metni | T339 | | |
| 02:§3.11.1 Görsel ürün düzleminde yaşar; | T339, T525 | | |
| 02:§3.11.2 Ayrı bir "ana görsel" işareti vardır ve galeri sırasından bağımsızdır; | T339, T525 | | |
| 02:§3.11.3 Görsele üç kısıt uygulanır | T339, T525 | | |
| 02:§3.11.4 Görselin alternatif metni isteğe bağlıdır ve geri düşüşlüdür | T339 | | |
| 02:§3.11.5 Ürün adına karakter tavanı konur; açıklama serbesttir | T339, T525 | | |
| 02:§3.11.6 Ürün açıklaması, kurumsal içeriğin uzun metinleri ve firmanın düzenlediği iki yasal metin tek bir kapalı metin biçimi setini kullanır | T339, T534 | | |
| 02:§3.11.7 Garanti bilgisinin ayrı alanı yoktur | T339 | | |
| 02:§3.12 Dijital ürün | T340 | | |
| 02:§3.12.1 Dijital ürünü sistem teslim eder | T340, T312, T511 | | |
| 02:§3.12.2 Dosya ürün düzleminde yaşar; varyant isterse kendi dosyasını taşır, taşımıyorsa ürününkini devralır | T340, T525 | | |
| 02:§3.12.3 Tek dosyanın boyut tavanı vardır; | T340, T312 | | |
| 02:§3.12.4 Dijital üründe adet her zaman 1'dir | T340, T508 | | |
| 02:§3.12.5 İndirmenin adet sınırı vardır, süre sınırı yoktur | T340, T312, T511 | | |
| 02:§3.12.6 Hakkı dolan müşteri firmaya başvurur; firma paneldeki sipariş sayfasından o kalemin indirme hakkını yeniler ve sayaç sıfırlanır | T340, T531 | | |
| 02:§3.12.7 Sipariş sayfası, kalemin bağlı olduğu varyantın o anki dosyasını verir | T340, T312 | | |
| 02:§3.12.8 Dosya güncellemesi indirme hakkını değiştirmez; | T340 | | |
| 02:§3.12.9 Daha önce alınmış dijital varyant yeniden satın alınabilir | T340, T508 | | |
| 02:§3.13 Üyelik, e-posta doğrulama ve oturum | T341, T342, T343 | | |
| 02:§3.13.1 Sipariş vermek için üyelik zorunlu değildir | T341, T509 | | |
| 02:§3.13.2 Hesap açıldığında, o e-postaya ait geçmiş misafir siparişleri hesaba düşer | T341 | | |
| 02:§3.13.3 Hesabı olan kişi giriş yapmadan misafir olarak sipariş verirse sipariş kabul edilir ve hesaba anında düşer; | T341, T509 | | |
| 02:§3.13.4 E-posta doğrulaması sert bir kapıdır | T341, T514 | | |
| 02:§3.13.5 Doğrulanmamış hesap geçicidir | T341, T514 | | |
| 02:§3.13.6 Sosyal giriş yalnız Google ile yapılır; | T343, T409, T515 | | |
| 02:§3.13.7 Aynı e-posta iki giriş yolundan gelirse tek hesaba bağlanır | T343, T409 | | |
| 02:§3.13.8 Google ile açılmış hesap şifresiz başlar ve sonradan şifre konabilir; | T343, T516 | | |
| 02:§3.13.9 Şifre sıfırlama bağlantısının bir ömrü vardır ve bağlantı tek kullanımlıktır | T342, T516 | | |
| 02:§3.13.10 Şifre sıfırlandığında hesabın bütün cihazlardaki oturumları kapanır; | T342, T516, T517 | | |
| 02:§3.13.11 Ekranlar hesabın varlığını iki farklı biçimde ele verir | T342, T514, T516 | | |
| 02:§3.13.12 Oturum ömrünü kullanıcı seçer | T342, T515 | | |
| 02:§3.13.13 Üç işlem, oturum açık olsa bile yeniden doğrulama ister | T342, T517 | | |
| 02:§3.13.14 Hesabın e-posta adresi, yeni adres doğrulanmadan değişmez | T343, T517, T519 | | |
| 02:§3.13.15 Şifre politikası uzunluk tabanlıdır | T342, T517 | | |
| 02:§3.13.16 Bu grubun kimlik doğrulama kuralları firma yöneticisi hesaplarına da aynen ve aynı değerlerle uygulanır | T342, T520, T523 | | |
| 02:§3.13.17 Giriş, şifre sıfırlama, hesap kaydı ve doğrulama bağlantısının yeniden istenmesi limitlidir | T342, T215 | | |
| 02:§3.13.18 Kayıtta ve hesapta pazarlama onayı yoktur | T341, T514 | | |
| 02:§3.13.19 Hesap kaydı ve Google ile ilk giriş veri toplayan girişlerin kapısına bağlıdır | T341, T331 | | |
| 02:§3.13.20 Her giriş denemesi giriş kaydına yazılır | T341, T108 | | |
| 02:§3.13.21 Hesap bir ad taşır | T341, T517 | | |
| 02:§3.14 Adres | T344 | | |
| 02:§3.14.1 Üye müşterinin adres defteri vardır | T344, T509, T518 | | |
| 02:§3.14.2 Fatura adresi teslimat adresinden ayrı seçilebilir | T344, T509 | | |
| 02:§3.14.3 Adreste il serbest metin değildir; | T344, T103 | | |
| 02:§3.14.4 Siparişin istediği adresler kalemlerin tipine bağlıdır | T344, T509 | | |
| 02:§3.14.5 Sipariş, adreslerini kendi içine dondurur; | T344, T355 | | |
| 02:§3.14.6 Adresin alanları | T344, T509, T518 | | |
| 02:§3.15 Hesabın silinmesi ve kişisel veri başvurusu | T345 | | |
| 02:§3.15.1 Hesap silindiğinde hesap kapanır, sipariş kaydı yasal saklama süresi boyunca durur | T345, T309, T517 | | |
| 02:§3.15.2 Yürüyen sipariş hesap silmeyi engellemez | T345 | | |
| 02:§3.15.3 Silinmiş bir hesabın siparişleri hiçbir hesaba bağlanmaz; | T345 | | |
| 02:§3.15.4 Ürünün sunduğu başvuru kanalı iletişim formudur | T345, T365 | | |
| 02:§3.15.5 Uygulama içi veri indirme yoktur | T345, T533 | | |
| 02:§3.15.6 E-posta değişikliğini geri alma bağlantısı açıkken hesap silinmez | T345, T517, T533 | | |
| 02:§3.16 Sepet | T346, T347 | | |
| 02:§3.16.1 Üyenin sepeti hesabında durur ve her cihazda aynıdır; misafirin sepeti bulunduğu tarayıcıya bağlıdır | T346, T508 | | |
| 02:§3.16.2 Sepet kendiliğinden boşalmaz | T346 | | |
| 02:§3.16.3 Giriş anında iki sepet birleşir; aynı varyant toplanmaz | T346, T508 | | |
| 02:§3.16.4 Birleştirme, müşterinin o ziyarette görmediği bir ürünü ya da adedi sepete eklediyse müşteriye bir kez söylenir | T346, T508 | | |
| 02:§3.16.5 Sepet kalemi canlıdır | T346, T508 | | |
| 02:§3.16.6 Vitrinde görünen ama satın alınamayan kalem sepette "Tükendi" işaretiyle kalır ve siparişe girmez | T346, T508 | | |
| 02:§3.16.7 Vitrinden kalkan kalem sepetten çıkar ve müşteriye bir kez söylenir | T346, T508 | | |
| 02:§3.16.8 Stoktan fazla adet sepete eklenmez ve stok adedi söylenmez | T347, T503, T508 | | |
| 02:§3.16.9 Firma bir üründen bir siparişte alınabilecek adedi sınırlayabilir | T347, T503, T525 | | |
| 02:§3.16.10 Sonradan aşılan sınırda kalemler sepette mesajıyla kalır | T347, T508 | | |
| 02:§3.16.11 Sepetteki kalem sayısına tavan yoktur | T347 | | |
| 02:§3.16.12 Sepet, ödeme başarılı olduğunda boşalır | T347, T348 | | |
| 02:§3.16.13 İstek listesi ve sepet hatırlatması yoktur | T347 | | |
| 02:§3.17 Sipariş onayı ve stok ayırma | T348 | | |
| 02:§3.17.1 Sipariş, müşterinin ödeme adımında siparişi onayladığı anda oluşur | T348, T329, T509, T510 | | |
| 02:§3.17.2 Onaylanan özet ile oluşan sipariş birebir aynıdır | T348, T509 | | |
| 02:§3.17.3 Sipariş onaylandığında kalemlerin adedi stoktan ayrılır ve müşteriye bir ödeme süresi tanınır | T348 | | |
| 02:§3.17.4 Ödeme süresi yönteme göre ayrıdır | T348, T324, T536 | | |
| 02:§3.17.5 Ödeme süresi dolan ya da kart ödemesi başarısız olan sipariş kendiliğinden iptal olur | T348, T324 | | |
| 02:§3.17.6 Başarısız ödemenin tekrarı yeni bir siparişle olur | T348, T510 | | |
| 02:§3.17.7 Müşteri aynı sepetten yeni bir siparişi onayladığında, o sepete bağlı ödenmemiş önceki siparişi kendiliğinden iptal edilir | T348 | | |
| 02:§3.17.8 Ödemesi hiç başarılı olmamış ve kendiliğinden iptal edilmiş sipariş — ödeme süresi dolmuş, kart ödemesi başarısız olmuş ya da aynı sepetten yeni… | T348, T215, T511 | | |
| 02:§3.18 Asgari sipariş tutarı | T349 | | |
| 02:§3.18.1 Firma isteğe bağlı tek bir asgari sipariş tutarı girebilir; | T349, T508, T509, T536 | | |
| 02:§3.18.2 Karşılaştırma tabanı ücretsiz kargo eşiğininkiyle aynıdır | T349 | | |
| 02:§3.18.3 Asgari tutar yalnız en az bir fiziksel kalem içeren siparişe uygulanır | T349 | | |
| 02:§3.19 Kargo ücreti ve ücretsiz kargo eşiği | T349 | | |
| 02:§3.19.1 Kargo ücreti sipariş başına sabittir | T349, T536 | | |
| 02:§3.19.2 Kargo ücreti KDV dahil tek tutar olarak girilir; KDV'si siparişteki fiziksel kalemlerin oranını izler | T349, T536 | | |
| 02:§3.19.3 Firma isteğe bağlı bir ücretsiz kargo eşiği girebilir | T349, T536 | | |
| 02:§3.19.4 Eşiğin tabanı kupondan önceki tutardır | T349 | | |
| 02:§3.19.5 Eşik tanımlıysa ve siparişte fiziksel kalem varsa, sepette ve ödeme özetinde eşiğe kalan tutar yazar | T349, T508, T509 | | |
| 02:§3.19.6 Kargo ücreti ve ücretsiz kargo kararı sipariş düzleminde donar; | T349, T355 | | |
| 02:§3.20 Teslim: bölge, yol, kargoya verme ve takip | T350, T351 | | |
| 02:§3.20.1 Firma teslimat yaptığı illeri seçer; varsayılan seçim tüm Türkiye'dir | T350, T509, T536 | | |
| 02:§3.20.2 Tek teslimat yolu kargodur | T350 | | |
| 02:§3.20.3 Sipariş tek parça gider ve tek takip numarası taşır; | T350 | | |
| 02:§3.20.4 Firmanın teslimat sözü kargoya verme süresidir | T350, T503, T536 | | |
| 02:§3.20.5 Kargoya verme süresinin firma varsayılanı vardır; ürün kendi süresini taşıyabilir | T350, T525, T536 | | |
| 02:§3.20.6 Kargoya verme süresi ödemenin onaylandığı anda başlar | T350, T325 | | |
| 02:§3.20.7 Sipariş "kargoya verildi" olarak işaretlenirken ya kargo şirketi + takip numarası girilir ya da "kendi aracımızla teslim" seçeneği işaretlenir; | T350, T310, T527 | | |
| 02:§3.20.8 Kargo şirketi ürünle gelen bir listeden seçilir; | T350, T310 | | |
| 02:§3.20.9 Kalemin teslimi tipine göre işler | T351, T531 | | |
| 02:§3.20.10 Karışık siparişte sevkiyat tek eksende kalır ve iptal edilmemiş fiziksel kalem kaldığı sürece fiziksel hattı izler; | T351, T316 | | |
| 02:§3.20.11 Fiziksel teslimi firma işaretler ve teslim tarihini kendisi girer | T351, T527 | | |
| 02:§3.20.12 Fiziksel kalemi olmayan sipariş kargo adımlarından geçmez | T351, T316 | | |
| 02:§3.21 Ödeme | T352, T353 | | |
| 02:§3.21.1 Satıcı firmadır; platform ticari zincirde yer almaz | T352, T401 | | |
| 02:§3.21.2 İki ödeme yöntemi vardır: kart ve havale/EFT | T352, T536 | | |
| 02:§3.21.3 Her kart ödemesi istisnasız 3D Secure doğrulamasından geçer; | T352, T406 | | |
| 02:§3.21.4 Kart yönteminin panelde açık/kapalı anahtarı yoktur | T352, T401 | | |
| 02:§3.21.5 Havale/EFT panelden açılıp kapatılır; açıkken IBAN zorunludur | T353, T536 | | |
| 02:§3.21.6 Havale ile siparişi onaylayan müşteri ekranda firmanın siparişe donan IBAN'ını (§3.21.5) ve sipariş numarasını görür; | T353, T307 | | |
| 02:§3.21.7 Firma havalede gelen tutarı sisteme girmez | T353 | | |
| 02:§3.21.8 Süresi dolup iptal edilmiş bir sipariş sonradan "ödendi" işaretlenemez | T353, T317 | | |
| 02:§3.21.9 Ürün taksiti bilmez | T352 | | |
| 02:§3.21.10 Sipariş hazırlığa ancak ödeme onaylandığında geçer | T352, T316 | | |
| 02:§3.21.11 Ters ibraz (harcama itirazı) ürünün dışındadır | T352, T529 | | |
| 02:§3.22 Sipariş numarası ve takip erişimi | T354 | | |
| 02:§3.22.1 Sipariş numarası tahmin edilemez bir koddur | T354 | | |
| 02:§3.22.2 Numara sipariş oluştuğu anda verilir ve sipariş iptal edilse bile tekrar kullanılmaz | T354 | | |
| 02:§3.22.3 Sipariş sayfasına üç yoldan girilir | T354, T511, T215 | | |
| 02:§3.22.4 Sipariş sayfası her bilgiyi taşır | T354, T511 | | |
| 02:§3.22.5 Sipariş e-postasındaki bağlantı, misafir alıcının ve hesabı silinmiş müşterinin siparişinde, sipariş numarasından ayrı, siparişe özgü ve tahmin… | T354, T511 | | |
| 02:§3.23 Sipariş anında donan değerler | T355 | | |
| 02:§3.23.1 Sipariş kalemi, ödenen tutarı üreten her değeri ve ürünün o anki tanımını dondurur | T355 | | |
| 02:§3.23.2 Donan her değer sipariş kaleminde düz olarak yaşar | T355 | | |
| 02:§3.23.3 Sipariş, teslimat ve fatura adreslerini, iletişim e-postasını, firmanın o günkü yasal kimliğini (§3.1.3'ün tipe göre kimlik alanları, adres,… | T355, T530 | | |
| 02:§3.23.4 Donma anı siparişin oluştuğu andır | T355 | | |
| 02:§3.23.5 Marka ayarları siparişe donmaz | T355 | | |
| 02:§3.24 Onay adımı ve yasal metinler | T356 | | |
| 02:§3.24.1 Sipariş iki ayrı onay kutusuyla onaylanır | T356, T509, T544 | | |
| 02:§3.24.2 Sepette dijital kalem varsa üçüncü bir kutu çıkar | T356, T509, T544 | | |
| 02:§3.24.3 Ön Bilgilendirme Formu onay özetiyle birebir aynıdır | T356, T366 | | |
| 02:§3.24.4 Yaş için ayrı beyan istenmez | T356 | | |
| 02:§3.24.5 Misafir alıcının e-postası doğrulama koduyla sınanmaz | T356, T509 | | |
| 02:§3.24.6 Onaylanan metinlerin sürümü ve firmanın kimliği siparişe donar | T356, T355 | | |
| 02:§3.24.7 Ödeme adımında müşteri notu alanı yoktur | T356 | | |
| 02:§3.24.8 Sitede "İşlem rehberi" başlıklı sabit bir sayfa vardır | T356, T507 | | |
| 02:§3.25 Fatura | T357 | | |
| 02:§3.25.1 Ürün fatura kesmez; e-Arşiv ve e-Fatura entegrasyonu yoktur | T357, T524 | | |
| 02:§3.25.2 Fatura sistemde saklanmaz ve gösterilmez | T357 | | |
| 02:§3.25.3 Dışa aktarmada siparişin donmuş firma kimliği kullanılır, | T357, T524 | | |
| 02:§3.26 İptal, cayma, iade ve ayıp talebi | — | | |
| 02:§3.27 Kurumsal içerik | T358, T359, T360 | | |
| 02:§3.27.1 Kurumsal içerik hazır tiplerle ve düzeni sabit bir genel sayfayla girilir; | T358 | | |
| 02:§3.27.2 Beş hazır tip vardır: Hakkımızda, hizmet tanıtımı, referans iş, sık sorulan soru ve şube; | T358 | | |
| 02:§3.27.3 Hakkımızda tek kayıttır | T358, T534 | | |
| 02:§3.27.4 Hizmet tanıtımı fiyatsızdır ve satın alınmaz | T358, T505 | | |
| 02:§3.27.5 Referans iş, firmanın yaptığı bir işi anlatır | T358, T505 | | |
| 02:§3.27.6 Sık sorulan soru, soru ve cevaptan oluşur; | T358, T506 | | |
| 02:§3.27.7 Şubeler İletişim sayfasında listelenir; | T358, T506 | | |
| 02:§3.27.8 İletişim sayfası ayrı bir içerik tipi değildir | T358, T506 | | |
| 02:§3.27.9 Genel sayfa başlık, metin ve görsellerden oluşur, düzeni sabittir ve adet sınırı yoktur | T358, T534 | | |
| 02:§3.27.10 Hizmet tanıtımı ve referans iş katalogdan ürün bağlayabilir | T359, T505 | | |
| 02:§3.27.11 Hizmet tanıtımı, referans iş, SSS, şube ve genel sayfa listelerini firma panelde elle sıralar; | T359, T534 | | |
| 02:§3.27.12 Firma isteğe bağlı sosyal medya hesap bağlantıları ve bir WhatsApp numarası girer | T358, T534 | | |
| 02:§3.27.13 Hiçbir kurumsal içerik tipi zorunlu değildir; | T359, T534 | | |
| 02:§3.27.14 Hakkımızda boşken ya da yayında değilken ana sayfanın kurumsal bloğu marka adını ve logoyu gösterir; | T359, T502 | | |
| 02:§3.27.15 Kurumsal içeriğin görselleri ve kısa alanları ürünün kurallarına tabidir | T359, T534 | | |
| 02:§3.27.16 Uzun metinler §3.11.6'nın metin biçimi setini kullanır | T359, T339 | | |
| 02:§3.27.17 Kurumsal içerikte gömülü video oynatıcı yoktur | T359 | | |
| 02:§3.27.18 Kurumsal içeriğe dosya eki (PDF vb.) eklenmez | T359 | | |
| 02:§3.27.19 Müşteri görüşleri ayrı bir içerik tipi değildir | T359 | | |
| 02:§3.27.20 Blog yoktur | T359 | | |
| 02:§3.27.21 Duyuru şeridi vardır: tek, kısa, tarihli | T360, T534 | | |
| 02:§3.27.22 Site boş kurulur | T360 | | |
| 02:§3.27.23 Her içerik kaydı kendi yayın durumunu taşır: Taslak ya da Yayında; | T360, T322 | | |
| 02:§3.27.24 Taslak kayıt yalnız adla ya da başlıkla — SSS'de soruyla — kaydedilebilir; yayına almak §3.27.13'ün zorunlu alanlarını ister | T360, T534 | | |
| 02:§3.27.25 Taslak içeriğin sayfası ziyaretçiye "sayfa bulunamadı" döner; giriş yapmış yöneticiye sayfayı "Taslak" bandıyla gösterir | T360, T505 | | |
| 02:§3.27.26 İçerikte sürüm geçmişi yoktur | T360 | | |
| 02:§3.27.27 Kurumsal içerik kalıcı olarak silinebilir; silme geri alınamaz işlem uyarısıyla yapılır | T360, T534 | | |
| 02:§3.27.28 Kurumsal içerikte ileri tarihli yayın yoktur; | T360 | | |
| 02:§3.28 Ana sayfa ve menü | T361 | | |
| 02:§3.28.1 İki hazır ana sayfa düzeni vardır ve firma panelden birini seçer: tanıtım öncelikli ya da mağaza öncelikli | T361, T535, T502 | | |
| 02:§3.28.2 Ana sayfanın kurumsal bloğu her zaman Hakkımızda'dır | T361, T502 | | |
| 02:§3.28.3 Hizmet tanıtımı ve referans iş bir "ana sayfada göster" işareti taşır; | T361, T534, T502 | | |
| 02:§3.28.4 Menü düzenleyici yoktur | T361, T535, T537 | | |
| 02:§3.28.5 Genel sayfalar altbilgide listelenir; | T361, T537 | | |
| 02:§3.28.6 Yayında ürünü olmayan kategori menüde görünmez | T361, T333 | | |
| 02:§3.29 Marka kimliği | T362 | | |
| 02:§3.29.1 Firma görünümde dört şeyi ayarlar: logo, marka adı, site simgesi ve tek bir marka rengi | T362, T534 | | |
| 02:§3.29.2 Logo zorunlu değildir | T362, T537 | | |
| 02:§3.29.3 Marka adı unvandan ayrı ve zorunlu bir alandır; | T362, T534 | | |
| 02:§3.29.4 Site simgesi isteğe bağlıdır; | T362 | | |
| 02:§3.29.5 Marka rengi okunurluğu bozamaz | T362 | | |
| 02:§3.29.6 Sitenin altbilgisinde "Shopfolio ile kuruldu" platform imzası varsayılan olarak açıktır; firma panelden kapatabilir | T362, T534 | | |
| 02:§3.30 Sayfa adresleri, arama motorları ve paylaşım | T363 | | |
| 02:§3.30.1 Ürünün adresi kategoriye bağlı değildir; | T363 | | |
| 02:§3.30.2 Her sayfanın adresi kaydın adından üretilir ve ilk yayında sabitlenir | T363 | | |
| 02:§3.30.3 Yönlendirme sistemi yoktur | T363 | | |
| 02:§3.30.4 Var olmayan, silinmiş ve yeni bir kayda verilmemiş ya da ziyaretçiye kapalı (taslak) her adres "sayfa bulunamadı" sayfasına düşer | T363, T504 | | |
| 02:§3.30.5 Arama motoru bilgileri otomatik üretilir; | T363, T313 | | |
| 02:§3.30.6 Arama motorlarına kapalı sayfalar | T363, T313 | | |
| 02:§3.30.7 Ürün ve kurumsal içerik sayfalarında tek bir "Paylaş" düğmesi vardır | T363, T503, T505 | | |
| 02:§3.31 Panelde eşzamanlı düzenleme | T364 | | |
| 02:§3.31.1 Sessiz üzerine yazma yoktur | T364, T208 | | |
| 02:§3.31.2 Siparişe yönelik her işlem siparişin güncel hâline karşı uygulanır | T364, T527 | | |
| 02:§3.32 İletişim talebi | T365 | | |
| 02:§3.32.1 İletişim formu girişsiz açıktır | T365, T506 | | |
| 02:§3.32.2 Formun alanları | T365, T506 | | |
| 02:§3.32.3 Konu tipi kapalı beş değerden seçilir | T365, T506 | | |
| 02:§3.32.4 Talebin iki durumu vardır: Açık ve Kapatıldı | T365, T322, T532 | | |
| 02:§3.32.5 Sistem talebi kaydeder ve iletir, yazışmayı yürütmez | T365, T308, T532 | | |
| 02:§3.32.6 Müşteri talebini takip etmez | T365 | | |
| 02:§3.32.7 Form spam'e karşı iki mekanizmayla korunur | T365, T215 | | |
| 02:§3.32.8 Aydınlatma metni tamamlanmış ve yayına alınmış değilse ya da çerez politikası yayına alınmamışsa iletişim formu kapalıdır | T365, T331 | | |
| 02:§3.32.9 Ayrı bir şikâyet kanalı ve canlı destek yoktur | T365 | | |
| 02:§3.32.10 İndirme hakkını yenileme başvurusu da bu formdan, "Sipariş hakkında" tipiyle gelir | T365, T531 | | |
| 02:§3.33 Yasal metinler | T366 | | |
| 02:§3.33.1 Dört yasal metin vardır | T366, T507 | | |
| 02:§3.33.2 Firmanın düzenlediği iki metin zorunludur ve boşaltılamaz | T366, T507 | | |
| 02:§3.33.3 Sürüm kendiliğinden artar | T366 | | |
| 02:§3.33.4 Metin değiştiğinde yeniden onay alınmaz | T366 | | |
| 02:§3.33.5 Aydınlatma metninin bağlantısı yeni bir kişisel veri alanı açan her ekranda görünür | T366, T540 | | |
| 02:§3.33.6 Ön Bilgilendirme Formu ve Mesafeli Satış Sözleşmesi müşteriye e-postayla da gönderilir | T366, T306 | | |
| 02:§3.33.7 Uyuşmazlık bilgisi iki metnin zorunlu parçasıdır | T366 | | |
| 02:§3.33.8 Ayarlarla çelişen serbest metin firmanın sorumluluğundadır | T366 | | |
| 02:§3.33.9 Dört metin tamamlanmadan satış açılmaz; | T366, T331 | | |
| 02:§3.34 Site geneli: cihaz, tarayıcı, erişilebilirlik ve süreklilik | T540, T327, T118 | | |
| 02:§3.34.1 Tek site, duyarlı tasarım | T540 | | |
| 02:§3.34.2 Vitrin mobil önceliklidir, panel masaüstü önceliklidir; panelde hiçbir işlev mobilde kapatılmaz | T540 | | |
| 02:§3.34.3 Tarayıcı tabanı son iki büyük sürümdür | T540 | | |
| 02:§3.34.4 En dar desteklenen ekran genişliği bir sayıdır; | T540 | | |
| 02:§3.34.5 Erişilebilirliğin tabanı Aile ve Sosyal Hizmetler Bakanlığı'nın "Web Siteleri ve Mobil Uygulamaların Erişilebilirliği Kontrol Listesi — A Seviyesi"… | T540, T543 | | |
| 02:§3.34.6 Sitede ziyaretçi ölçümü yoktur | T540 | | |
| 02:§3.34.7 Ürün çalışma süresi taahhüdü vermez | T327 | | |
| 02:§3.34.8 Sipariş tarafında veri kaybı toleransı sıfırdır | T118, T327 | | |
| 02:§4.1 Sayım kuralları | T324, T325, T337, T338, T350, T353, T360, T378 | | |
| 02:§4.1.1 Tek saat dilimi Türkiye saatidir | T378, T101, T216 | | |
| 02:§4.1.2 İş günü pazartesi–cumadır, resmî tatiller hariç | T378, T350, T353 | | |
| 02:§4.1.3 İki süre iş günüyle, gerisi takvim günüyle sayılır | T378, T324, T325 | | |
| 02:§4.1.4 Gün olarak belirlenen süre, başlangıç gününün ertesi günü başlar ve son günün 23:59'unda dolar | T378, T337, T338, T360 | | |
| 02:§4.1.5 Üç süre çitlidir: | T378, T350, T353, T536 | | |
| 02:§4.2 Süreler | T324, T325, T326, T301, T302, T201 | | |
| 02:Z-1 E-posta doğrulama bağlantısının ömrü | T324, T341 | | |
| 02:Z-2 Şifre sıfırlama bağlantısının ömrü | T324, T342, T343 | | |
| 02:Z-3 Yönetici daveti bağlantısının ömrü | T324, T106, T520 | | |
| 02:Z-4 Kısa oturum — "beni hatırla" işaretsiz | T324, T342 | | |
| 02:Z-5 Uzun oturum — "beni hatırla" işaretli | T324, T342 | | |
| 02:Z-6 Sepetin ömrü | T324, T346 | | |
| 02:Z-7 Kart ödeme süresi — stok ayırma bu süre boyunca sürer | T324, T348, T352, T311 | | |
| 02:Z-8 Havale ödeme süresi — stok ayırma bu süre boyunca sürer | T324, T348, T353, T327 | | |
| 02:Z-9 Havale ödeme hatırlatması | T324, T353, T306 | | |
| 02:Z-10 Kargoya verme süresi — firmanın vitrindeki sözü | T325, T350 | | |
| 02:Z-11 Yasal teslim üst sınırı — kişiye özel üretilen mallar dışında | T325, T350, T368 | | |
| 02:Z-12 Teslim tarihi | T325, T351 | | |
| 02:Z-39 İfa süresi — hizmet kalemi; firmanın hizmetteki sözü | T325, T332, T355, T367, T368 | | |
| 02:Z-13 Cayma penceresi — fiziksel kalem | T325, T371, T351 | | |
| 02:Z-14 Cayma penceresi — hizmet kalemi | T325, T371 | | |
| 02:Z-15 Cayma hakkı — dijital kalem | T325, T371 | | |
| 02:Z-16 Geri ödeme — cayma | T325, T370, T373 | | |
| 02:Z-42 Müşterinin iade malını gönderme süresi | T325, T373 | | |
| 02:Z-17 Geri ödeme — iptal edilen ödenmiş sipariş ya da kalem | T325, T370 | | |
| 02:Z-47 İfanın imkânsızlaştığının bildirimi | T325, T368, T370, T306 | | |
| 02:Z-18 Ayıp talebinin iki yıllık zamanaşımı | T325, T374 | | |
| 02:Z-19 İndirme hakkı | T325, T340 | | |
| 02:Z-20 İndirimin tarih aralığı | T326, T337 | | |
| 02:Z-21 Referans fiyatın geriye bakış penceresi | T326, T337 | | |
| 02:Z-22 Kuponun tarih aralığı | T326, T338 | | |
| 02:Z-23 Duyurunun tarih aralığı | T326, T360 | | |
| 02:Z-24 İleri tarihli yayın | T326, T335, T360 | | |
| 02:Z-25 KVKK başvurusuna cevap | — | | |
| 02:Z-26 Gönderilemeyen e-postanın yeniden denenmesi | T326, T305 | | |
| 02:Z-27 Deneme limitlerinin penceresi | T326, T377 | | |
| 02:Z-28 Sipariş, ödeme ve fatura verisi; sözleşme ve ön bilgilendirme kayıtları; siparişe donmuş yasal… | T301 | | |
| 02:Z-41 Ödemesi hiç alınmamış (Başarısız) siparişin kişisel verisi | T301 | | |
| 02:Z-29 Ayıp talebi kaydı | T302, T374 | | |
| 02:Z-30 İşlem izi | T302, T106 | | |
| 02:Z-31 Hesap verisi — giriş bilgileri, şifre, açık oturumlar, adres defteri | T345 | | |
| 02:Z-32 Sepet | T345 | | |
| 02:Z-33 İletişim talebi — KVKK talebi dahil | T302, T365 | | |
| 02:Z-34 Bildirim gönderim kaydı — hesap ve yönetim e-postaları ile firmaya giden bildirimler; yeniden… | T302, T305 | | |
| 02:Z-43 Bildirim gönderim kaydı — siparişe bağlı müşteri bildirimleri (B-1…B-16); yeniden denemeler ve… | T302, T301, T305 | | |
| 02:Z-35 Havale hattında müşterinin geri ödeme için girdiği IBAN | T302, T370 | | |
| 02:Z-36 Periyodik imha | T201 | | |
| 02:Z-40 Giriş kaydı ve sistem kayıtları | T302, T341 | | |
| 02:Z-44 İmha kaydı — her periyodik imha çalışmasının ve kişisel veriyi hemen silen her işlemin (hesabın… | T201 | | |
| 02:Z-45 E-posta değişikliğini geri alma bağlantısının ömrü | T302, T343 | | |
| 02:Z-46 Girişte tanınan tarayıcı işareti | T302, T377 | | |
| 02:Z-37 Veri ihlalinde Kurul'a bildirim | — | | |
| 02:Z-38 Malı dönmeyen caymada havale hattı IBAN'ının kendiliğinden silinmesi | T327, T370, T373 | | |
| 02:§4.3 Kendiliğinden işleyen anlar | T327, T348, T370 | | |
| 02:§5.1 Ürün ve varyantın yayın durumu | T321, T335 | | |
| 02:§5.2 Kurumsal içeriğin yayın durumu | T322, T360, T335 | | |
| 02:§5.3 Sipariş: iki eksen ve oluşma anı | T315, T316, T317, T318, T345, T348, T355, T376 | | |
| 02:§5.3.1 Sipariş iki bağımsız eksende durum taşır: | T315, T318 | | |
| 02:§5.3.2 Sipariş, müşteri onayladığı anda Alındı + Bekliyor olarak doğar | T348, T355, T316, T317 | | |
| 02:§5.3.3 Onay anında özet değişmişse sipariş hiç doğmaz | T348 | | |
| 02:§5.3.4 Her ödeme denemesi ayrı bir sipariştir | T348, T376 | | |
| 02:§5.3.5 Sipariş hesabın kaderinden bağımsızdır: | T345 | | |
| 02:§5.4 Sevkiyat ekseni — Sipariş durumu | T315, T316, T319, T320 | | |
| 02:§5.5 Ödeme ekseni — Ödeme durumu | T315, T317, T319, T370 | | |
| 02:§5.6 İki eksenin bağı ve kendiliğinden geçişler | T311, T316, T317, T318, T324, T348, T372 | | |
| 02:§5.6.1 İki eksen tek bir kapı koşuluyla bağlıdır: | T318, T316, T317 | | |
| 02:§5.6.2 Yalnız dijital siparişte ödemenin onayı sevkiyatı doğrudan Teslim edildi'ye alır | T318, T316 | | |
| 02:§5.6.3 Ödeme süresi dolduğunda ya da kart ödemesi başarısız olduğunda — hangisi önce gelirse — sipariş kendiliğinden İptal edildi, ödemesi Başarısız olur; | T348, T318, T317, T324, T311 | | |
| 02:§5.6.4 Müşteri aynı sepetten yeni bir siparişi onayladığında o sepete bağlı ödenmemiş önceki sipariş kendiliğinden iptal edilir | T348 | | |
| 02:§5.6.5 Ödenmeden iptal edilen siparişin ödemesi Başarısız olur | T317, T318 | | |
| 02:§5.6.6 Cayma beyanı kendi başına hiçbir ekseni değiştirmez: | T318, T372 | | |
| 02:§5.7 Tipe göre sevkiyat hattı | T318, T350 | | |
| 02:§5.8 Sipariş kalemi düzeyindeki kayıtlar | T320, T351, T367, T368, T372, T373, T374 | | |
| 02:§5.9 Geri alınamaz geçişler | T319, T367, T368, T372, T345 | | |
| 02:§5.10 Ayıp talebinin durumu | T320, T374 | | |
| 02:§5.11 İndirim ve kuponun zamana ve kullanıma bağlı geçerliliği | T337, T338, T355, T367 | | |
| 02:§5.11.1 İndirim tarih aralığında kendiliğinden işler: | T337 | | |
| 02:§5.11.2 Kupon, tarih aralığının içinde ve kullanım hakkı kaldıkça geçerlidir | T338 | | |
| 02:§5.11.3 Kuponun kullanım hakkının ömrü: | T338, T367 | | |
| 02:§5.11.4 Kuponun kalemlere dağıtılan payı sipariş anında donar | T338, T355 | | |
| 02:§5.12 Hesap ve oturum | T341, T342, T343, T376 | | |
| 02:§5.12.1 Hesabın durum makinesi yoktur | T341 | | |
| 02:§5.12.2 Oturum "beni hatırla" seçimine göre kısa ya da uzun ömürlü açılır | T342, T343 | | |
| 02:§5.12.3 Deneme limiti hesabın durumunu değiştirmez | T376 | | |
| 02:§5.13 İletişim talebinin durumu | T322, T365 | | |
| 02:§6.1 Bütün akışlara uygulanan ilkeler | T305, T306, T307, T311, T324, T331, T346, T348, T352, T354, T356, T375, T376, T377 | | |
| 02:§6.1.1 Hiçbir sipariş ya da talep akışı bir e-postanın ulaşmasına bağlı değildir | T305, T306, T307, T354, T375, T511, T531 | | |
| 02:§6.1.2 Sistem, müşterinin ödediği bir siparişi kendi bilgisizliği yüzünden iptal etmez | T348, T352, T311, T324 | | |
| 02:§6.1.3 Onaylanan özet ile oluşan sipariş birebir aynıdır | T348, T356 | | |
| 02:§6.1.4 Sepet hatalarda korunur | T346, T352, T331 | | |
| 02:§6.1.5 Limit aşımı nötr konuşur | T376, T377, T541 | | |
| 02:§6.1.6 Hata ve uyarının anlamı yalnız renkle taşınmaz; | T540, T541 | | |
| 02:§6.2 Satın alma (akış 1) | T346, T347, T348, T349, T350, T352, T353, T334, T338, T340, T331, T376, T356, T354 | | |
| 02:§6.3 Sipariş takibi (akış 2) | T354, T375, T376, T306, T307, T348 | | |
| 02:§6.4 İptal, cayma ve iade (akış 3) | T367, T368, T369, T370, T371, T372, T373, T374, T351, T350, T316 | | |
| 02:§6.5 Üyelik (akış 4) | T341, T342, T343, T345, T376, T309, T366 | | |
| 02:§6.6 Katalog yönetimi (akış 5) | T333, T334, T335, T336, T337, T338, T340, T350, T364 | | |
| 02:§6.7 Sipariş yürütümü (akış 6) | T305, T306, T316, T317, T319, T340, T351, T353, T364, T368, T375 | | |
| 02:§6.8 Kurumsal içerik (akış 7) | T358, T359, T360, T361, T364, T365 | | |
| 02:§6.9 Mağaza ayarları (akış 8) | T330, T331, T350, T352, T353, T362, T366, T106, T520 | | |
| 02:§7.1 Üç yol ve ortak ilkeler | T306, T307, T365, T366, T367, T369, T371, T372, T374 | | |
| 02:§7.1.1 Müşterinin satın almadan sonra başvurabileceği üç ayrı yol vardır | T367, T371, T374 | | |
| 02:§7.1.2 Tek akış tüketici akışıdır | T371 | | |
| 02:§7.1.3 İptal ve iade kalem düzeyinde ve adetle işler | T369, T367 | | |
| 02:§7.1.4 Üç yol da sipariş sayfasından kullanılır; | T367, T372, T374, T306, T307, T511 | | |
| 02:§7.1.5 İtirazın sistemdeki yeri | T365, T366 | | |
| 02:§7.1.6 Kalemin işlem birimi adettir; kalem parça parça kapanabilir (K-787, K-790) | T369, T367, T372 | | |
| 02:§7.1.7 İşleme giren adetlerin tutarı kalemin donmuş değerlerinden ayrılır (K-788) | T369 | | |
| 02:§7.2 İptal | T306, T307, T316, T325, T334, T338, T340, T349, T350, T351, T353, T367, T368, T369, T370 | | |
| 02:§7.2.1 Müşteri fiziksel kalemi, sipariş "Kargoya verildi"ye geçene kadar kendi iptal eder | T367, T316 | | |
| 02:§7.2.2 İptal kalem ve adet düzeyindedir | T367, T316 | | |
| 02:§7.2.3 Teslimat gecikmesinde fesih iki yoldan işler | T368, T350, T325 | | |
| 02:§7.2.4 Firma da iptal edebilir | T368, T306, T307 | | |
| 02:§7.2.5 Dijital kalem ödeme onayından sonra iptal edilemez: | T367, T340, T351 | | |
| 02:§7.2.6 İptalde sayaçlar kendiliğinden döner | T367, T334, T338 | | |
| 02:§7.2.7 Kısmi iptalde kupon yeniden hesaplanmaz | T367, T369 | | |
| 02:§7.2.8 İptal edilen ödenmiş kalemin parası, ödemenin geldiği yoldan geri ödenir | T370, T369, T353 | | |
| 02:§7.2.9 İptalde kargo ücreti sebebe ve kapsama göre ayrılır | T369, T349 | | |
| 02:§7.3 Cayma | T306, T307, T318, T325, T330, T332, T351, T356, T367, T371, T372 | | |
| 02:§7.3.1 Cayma penceresi on dört gündür ve sabittir; | T371, T351, T325 | | |
| 02:§7.3.2 Dijital üründe cayma hakkı yoktur; koşulu ayrı bir onaydır | T371, T356 | | |
| 02:§7.3.3 Hizmette cayma hakkı, müşterinin onayıyla başlayan ifa tamamlanınca düşer | T371, T356, T351 | | |
| 02:§7.3.4 Fiziksel üründe cayma istisnası ürün düzleminde isteğe bağlı bir işarettir | T371, T332, T356 | | |
| 02:§7.3.5 Cayma sipariş sayfasından bildirilir | T372, T330, T306, T307 | | |
| 02:§7.3.6 Cayma beyanı siparişi kapatmaz | T372, T318 | | |
| 02:§7.3.7 Kargoya verilmemiş fiziksel kalemde tüketicinin yolu iptaldir | T372, T367 | | |
| 02:§7.4 İade ve geri ödeme | T307, T325, T330, T334, T338, T349, T356, T366, T369, T370, T371, T373, T374 | | |
| 02:§7.4.1 Geri ödemenin on dört günü, cayma beyanı kalemin teslim tarihinden önce yapıldıysa ya da kalem hizmetse beyandan, teslimden sonra yapıldıysa iade… | T370, T373, T325 | | |
| 02:§7.4.2 Cayma hâlinde iade kargo bedeli firmaya aittir; | T373, T356 | | |
| 02:§7.4.3 Ayıplı malda iade kargo bedeli her hâlükârda firmaya aittir | T373, T374 | | |
| 02:§7.4.4 Müşteri malı firmanın iade adresine karşı ödemeli gönderir | T373, T330, T356, T366 | | |
| 02:§7.4.5 Geri ödeme, ödemenin geldiği yoldan gider | T370, T543 | | |
| 02:§7.4.6 Kısmi iadede kargo ücreti ve ücretsiz kargo kararı yeniden hesaplanmaz | T369, T349, T373 | | |
| 02:§7.4.7 İade edilen fiziksel kalemin adedi stoğa kendiliğinden eklenmez | T373, T334 | | |
| 02:§7.4.8 Caymada hizmet kontenjanı cayılan adet kadar havuza döner; kuponun kullanım hakkı yalnız siparişin tamamı — bütün kalemlerinin bütün adetleri — iptal… | T373, T334, T338 | | |
| 02:§7.4.9 Koşullu istisna kaleminde koruyucu ambalajı açılmış dönen mal reddedilebilir | T373, T371, T307 | | |
| 02:§7.4.10 Kullanılmış ya da hasar görmüş dönen malda geri ödeme tam tutarla yapılır; değer kaybı talebi ürünün dışındadır | T373 | | |
| 02:§7.5 Ayıp talebi | T320, T325, T340, T370, T374 | | |
| 02:§7.5.1 Ayıp talebi üçüncü yoldur | T374, T325 | | |
| 02:§7.5.2 Kanal sipariş sayfasındaki "sorun bildir"dir | T374, T513 | | |
| 02:§7.5.3 Sistem talebi kaydeder ve iletir, çözümü yürütmez | T374, T320, T370 | | |
| 02:§7.5.4 Ayıplı dijital üründe doğal çözüm dosya güncellemesidir | T374, T340 | | |
| 02:§7.6 Geri alma sınırları | T316, T317, T319, T345, T353, T360, T372, T375 | | |
| 02:§7.6.1 Panel "geri al" düğmesi sunmaz | T319 | | |
| 02:§7.6.2 Süresi dolup iptal edilmiş siparişe gelen ödeme işlenemez; | T353, T317 | | |
| 02:§7.6.3 Müşteri akışında iptal edilmiş ya da teslim edilmiş sipariş yeniden açılmaz; | T316, T319 | | |
| 02:§7.6.4 Kurumsal içerikte eski hâle dönülmez ve silme geri alınmaz: | T360 | | |
| 02:§7.6.5 Silinmiş bir hesabın siparişleri hiçbir hesaba geri bağlanmaz | T345 | | |
| 02:§7.6.6 Yöneticinin sipariş düzeltmeleri ileri yönlü kayıtlardır ve tutarı değiştirmez | T375 | | |
| 02:§7.6.7 Cayma beyanı geri alınmaz ve düzeltilmez | T372, T375 | | |
| 02:§8.1 Kimlik doğrulama tabanı | T341, T342, T343, T375, T376 | | |
| 02:§8.1.1 Doğrulanmamış hesapla giriş ve sipariş yoktur | T341, T342 | | |
| 02:§8.1.2 Bir siparişin ya da bir giriş yolunun hesaba bağlanmasının tek dayanağı doğrulanmış e-postadır | T341, T343, T375 | | |
| 02:§8.1.3 Bağlantılar süreli ve tek kullanımlıktır | T341, T342, T343, T520 | | |
| 02:§8.1.4 Ekranlar hesabın varlığını kayıtta, ödeme adımının hatırlatmasında ve hesabın e-posta değişikliğinde ele verir | T341, T342, T376 | | |
| 02:§8.1.5 Üç işlem oturum açıkken de yeniden doğrulama ister: | T342, T343 | | |
| 02:§8.1.6 Şifre politikası uzunluk tabanlıdır | T342 | | |
| 02:§8.1.7 Yönetici tarafı aynı rejimdedir | T342, T106, T520 | | |
| 02:§8.2 Deneme limitleri | T338, T341, T342, T343, T348, T354, T365, T376, T377 | | |
| 02:L-1 Başarısız giriş denemesi — şifreyle; yeniden doğrulamadaki başarısız şifre denemesi de (§3.13.13;… | T376, T342 | | |
| 02:L-2 Şifre sıfırlama talebi | T376, T342 | | |
| 02:L-3 Hesap kaydı | T376, T341 | | |
| 02:L-4 İletişim formu gönderimi | T376, T365 | | |
| 02:L-5 Başarısız misafir sipariş takibi sorgusu | T376, T354 | | |
| 02:L-6 Geçersiz kupon kodu denemesi | T376, T338 | | |
| 02:L-7 Kendiliğinden iptal edilen ödenmemiş sipariş | T376, T348 | | |
| 02:L-8 Aynı anda açık ödenmemiş sipariş | T376, T348 | | |
| 02:L-9 Doğrulama bağlantısının yeniden istenmesi | T376, T341, T343 | | |
| 02:§8.2.1 Limit aşıldığında yalnız o işlem geçici olarak engellenir | T376 | | |
| 02:§8.2.2 İki eksen ayrı sayılır | T377 | | |
| 02:§8.2.3 L-7 tekrarı ölçer, tek denemeyi değil | T377, T348 | | |
| 02:§8.2.4 Yönetici de aynı limitlere aynı değerlerle tabidir; | T377, T342 | | |
| 02:§8.2.5 Sepet ve stok tarafında ürün düzeyinde limit yoktur | T377 | | |
| 02:§8.2.6 Girişin e-posta ekseni tanınan tarayıcıyı engellemez | T377, T342 | | |
| 02:§8.3 Form ve site güvenliği | T307, T352, T354, T365, T368, T372, T375, T377 | | |
| 02:§8.3.1 İletişim formu üçüncü taraf captcha kullanmaz | T365, T215 | | |
| 02:§8.3.2 Siteye kod eklenmez | T209, T540 | | |
| 02:§8.3.3 Sitede üçüncü taraf gömme içerik yoktur: | T209, T540, T352 | | |
| 02:§8.3.4 Ürün kart verisini görmez, saklamaz, loglamaz | T352, T202 | | |
| 02:§8.3.5 Sipariş erişimi tahmin edilemez anahtarlara dayanır | T354, T377 | | |
| 02:§8.3.6 Misafir siparişinin e-postası yalnız firmanın teyidiyle değişir | T375, T354, T307 | | |
| 02:§8.3.7 Siparişin adresi yalnız firmanın teyidiyle düzeltilir | T375, T307 | | |
| 02:§8.3.8 Başka kanaldan gelen cayma bildirimi yalnız firmanın teyidiyle işlenir | T372, T368, T307 | | |
| 02:§8.4 Veri asgariliği | T301, T302, T341, T356, T365, T370, T373 | | |
| 02:§8.4.1 Ürün yalnız işlediği kişisel veriyi toplar | T356, T365, T341, T540 | | |
| 02:§8.4.2 IBAN yalnız gerektiğinde istenir ve iş bitince silinir | T370, T373, T201 | | |
| 02:§8.4.3 Serbest metin alanları kayda geçen bir risk taşır | T301, T356, T531 | | |
| 02:§8.4.4 Çerezler yalnız zorunludur ve — ödeme sağlayıcısının çerçevesi dışında — birinci taraftır; onay bandı yoktur | T209, T540 | | |
| 02:§8.4.5 Saklama süreleri §4'te, imhanın biçimi ve kişisel veri yükümlülükleri §12'dedir | T201, T301, T302 | | |
| 02:§8.5 Kayıtlar ve veri ihlali | T302, T341 | | |
| 02:§8.5.1 Ürün ayrı bir ihlal kaydı tutmaz | T341, T302, T106, T201 | | |
| 02:§8.5.2 İşlem izi on yıllık saklama süresi boyunca değiştirilemez ve silinemez; | T106, T302, T201 | | |
| 02:§9.1 Kanal ve ilkeler | T201, T304, T305, T328, T330, T362, T382, T385, T407, T511, T531, T601 | | |
| 02:§9.1.1 Tek kanal e-postadır | T305, T328 | | |
| 02:§9.1.2 Hiçbir sipariş ya da talep akışı bir e-postanın ulaşmasına bağlı değildir | T305, T328, T511 | | |
| 02:§9.1.3 İşlem bildirimleri ticari elektronik ileti değildir ve kapatılamaz | T305, T328 | | |
| 02:§9.1.4 Gönderen ve yanıt | T305, T330, T362 | | |
| 02:§9.1.5 Çalışan bir e-posta gönderim yapılandırması her kurulumun dış ön koşuludur; | T407, T601 | | |
| 02:§9.1.6 Gönderilemeyen e-posta üç kez, artan aralıkla yeniden denenir | T385, T304, T407, T382, T531, T201 | | |
| 02:§9.2 Müşteriye giden bildirimler | T305, T306, T307, T311, T316, T340, T348, T350, T353, T367, T368, T370, T372, T373, T374, T375 | | |
| 02:B-1 Sipariş alındı — firma misafir siparişinin e-postasını düzelttiğinde yeni adrese yeniden gider (§10.… | T306, T305, T348, T375 | | |
| 02:B-2 Havale/EFT seçildi | T306, T353 | | |
| 02:B-3 Havale ödeme hatırlatması — havale "ödendi" işaretinin düzeltilmesiyle yeniden başlayan sürede bir k… | T306, T353, T375 | | |
| 02:B-4 Ödeme onaylandı | T306, T311, T340 | | |
| 02:B-5 Kargoya verildi | T306, T350 | | |
| 02:B-6 Teslim edilemedi | T306, T316 | | |
| 02:B-7 İptal edildi — sipariş ya da tek bir kalem | T306, T367, T368 | | |
| 02:B-8 Geri ödeme işlendi — sistemin iptal edilmiş siparişe gelen kart ödemesini kendiliğinden geri ödemesi… | T306, T370 | | |
| 02:B-9 Cayma beyanı, gecikme feshi ya da ayıp talebi alındı — firmanın başka kanaldan gelen cayma ya da gec… | T307, T372, T368, T374 | | |
| 02:B-10 Teslimat ya da fatura adresi düzeltildi | T307, T375 | | |
| 02:B-11 Kalem çıkarıldı ya da adedi azaltıldı | T307, T375 | | |
| 02:B-12 Kargo şirketi ya da takip numarası düzeltildi | T307, T375 | | |
| 02:B-13 Yanlış durum geçişi düzeltildi | T307, T375, T353 | | |
| 02:B-14 Havale hattında IBAN silindikten sonra iade edilen mal firmaya ulaştı — firma teslim alma adımını iş… | T307, T370, T373, T372 | | |
| 02:B-15 Firma misafir siparişinin e-postasını düzeltti (§10.4.11) — eski adrese gider | T307, T375 | | |
| 02:B-16 İade reddedildi — firma koşullu istisna kaleminde koruyucu ambalajı açılmış dönen malı reddetti (§7.… | T307, T373 | | |
| 02:§9.3 Firmaya giden bildirimler | T308, T330, T348, T353, T365, T370, T372, T374, T379, T382, T385 | | |
| 02:F-1 Yeni sipariş | T308, T348 | | |
| 02:F-2 Yeni iletişim talebi | T308, T365 | | |
| 02:F-3 Yeni cayma beyanı, gecikme feshi ya da ayıp talebi — müşterinin çözülmüş ayıp talebini yeniden açmas… | T308, T372, T374 | | |
| 02:F-4 Sistemin kendiliğinden yaptığı karta geri ödeme — iptal edilmiş siparişe gelen ödemenin geri ödenmes… | T308, T370 | | |
| 02:F-5 Havale/EFT'nin IBAN'ı panelde girildi ya da değiştirildi — bütün yöneticilerin kendi adreslerine gid… | T308, T353 | | |
| 02:F-6 Yönetici daveti gönderildi ya da bir yönetici kaldırıldı — bütün yöneticilerin kendi adreslerine gid… | T308, T379 | | |
| 02:§9.3.1 Firmaya giden bildirimler firma kimliğindeki iletişim e-postasına gider | T308, T330, T382 | | |
| 02:§9.3.2 Firmanın kendi yaptığı işlem kendisine bildirilmez; | T308, T382 | | |
| 02:§9.3.3 Havale IBAN'ının değişikliği bütün yöneticilere bildirilir (F-5) | T308, T353, T385 | | |
| 02:§9.3.4 Yönetici daveti gönderildiğinde ya da bir yönetici kaldırıldığında bütün yöneticilere bildirim gider… | T308, T379, T385 | | |
| 02:§9.4 Hesap ve yönetim e-postaları | T309, T341, T342, T343, T345, T379 | | |
| 02:§9.5 Olmayan bildirimler | T328, T308, T365, T305 | | |
| 02:§10.1 Panelin kapsamı | T308, T323, T330, T364, T375, T379, T381, T522, T540, T547, T601 | | |
| 02:§10.1.1 Panel tek firmayı yönetir; firma seçimi yoktur | T330 | | |
| 02:§10.1.2 Yöneticinin yapabildikleri ve yapamadıkları | T323, T547, T379, T375, T522 | | |
| 02:§10.1.3 Parametreler üç katmandadır | T381, T601 | | |
| 02:§10.1.4 Panelde hiçbir işlev mobilde kapatılmaz; | T364, T540 | | |
| 02:§10.1.5 Firmaya giden bildirimler | T308 | | |
| 02:§10.2 Yönetici hesapları | T303, T308, T309, T324, T341, T342, T343, T379, T380, T520, T601 | | |
| 02:§10.2.1 İlk yönetici kurulumda doğar, sonrakiler davetle açılır | T379, T303, T309, T520 | | |
| 02:§10.2.2 Bir yönetici başka bir yöneticiyi kaldırabilir; son yönetici kaldırılamaz ve hiç kimse kendi hesabın… | T379, T308, T380 | | |
| 02:§10.2.3 Bütün yöneticiler aynı yetkiye sahiptir | T379 | | |
| 02:§10.2.4 Kimlik doğrulama müşteri tarafıyla aynıdır | T342, T343, T379, T380 | | |
| 02:§10.2.5 Yönetici hesabının sepeti ve siparişi olmaz | T379, T341, T342, T343 | | |
| 02:§10.2.6 Kullanılmamış davet geri çekilebilir; bir adres için aynı anda tek geçerli davet vardır | T379, T308, T380, T324 | | |
| 02:§10.2.7 Panele erişimin kaybında kurtarma ürünün dışındadır ve kurulum tarafının işidir | T379, T303, T601 | | |
| 02:§10.3 İşlem izi | T106, T201, T341, T345, T375, T379, T380, T381, T384, T522 | | |
| 02:§10.3.1 İz sekiz alanda tutulur | T380, T379, T381, T384, T375, T345 | | |
| 02:§10.3.2 İz saklama süresi boyunca (§10.3.5) değiştirilemez ve silinemez | T380, T106 | | |
| 02:§10.3.3 Sistem olayları ize yazılmaz | T380, T201, T341 | | |
| 02:§10.3.4 İz panelde düz bir liste olarak görünür; | T380, T522 | | |
| 02:§10.3.5 İz on yıl saklanır; | T380, T201 | | |
| 02:§10.4 Sipariş müdahaleleri | T301, T307, T316, T317, T319, T334, T341, T350, T351, T353, T354, T355, T367, T368, T370, T371, T372, T373, T375, T380, T382, T383, T386 | | |
| 02:§10.4.1 Dokuz müdahale vardır | T375, T386, T380 | | |
| 02:§10.4.2 Adres yalnız "Kargoya verildi"ye kadar düzeltilir | T375, T355, T307 | | |
| 02:§10.4.3 Kalem adedi azaltılabilir ya da kalem çıkarılabilir; kalem eklenemez ve fiyat değiştirilemez | T386, T367, T370, T319, T334 | | |
| 02:§10.4.4 Kargo şirketi ve takip numarası düzeltilebilir; | T386, T350, T307 | | |
| 02:§10.4.5 Teslim tarihi düzeltilebilir; | T386, T351, T371, T373 | | |
| 02:§10.4.6 Yanlış yapılmış durum geçişi yeni bir geçişle düzeltilir | T319, T316, T317, T353, T334, T307 | | |
| 02:§10.4.7 İç not müşteriye hiçbir yerde görünmez | T386, T301 | | |
| 02:§10.4.8 Firma iptalinin sebep listesi §7.2.4'te, | T368, T319 | | |
| 02:§10.4.9 Malı dönmemiş cayma kalemi "mal dönmedi" gerekçesiyle kapatılabilir | T373, T370, T382, T383 | | |
| 02:§10.4.10 Başka bir kanaldan gelen cayma bildirimi panelden kaydedilir | T372, T368, T370, T307 | | |
| 02:§10.4.11 Misafir siparişinin iletişim e-postası müşterinin talebi üzerine düzeltilir | T375, T354, T341, T307 | | |
| 02:§10.5 Manuel adımlar ve bütçe | T318, T357 | | |
| 02:§10.5.1 Sipariş başına sistem içi zorunlu elle adımlar | T318 | | |
| 02:§10.5.2 Koşullu adımlar — her biri bir adım | T318 | | |
| 02:§10.5.3 Fatura sipariş başına bir sistem dışı adımdır | T357 | | |
| 02:§10.5.4 Bütçe hat başınadır: tek tipli bir siparişin hattında sistem içi zorunlu elle adım üçü geçmez | T318 | | |
| 02:§10.5.5 Hacim varsayımı | — | | |
| 02:§10.6 Bekleyen işler ve satış özeti | T348, T350, T353, T370, T373, T382, T383, T385 | | |
| 02:§10.6.1 Panel ana sayfası bekleyen işleri sayar | T382, T385, T370, T373, T353 | | |
| 02:§10.6.2 Panelde bir satış özeti vardır ve analitik değildir | T383 | | |
| 02:§10.6.3 Ölçülerin tanımı | T383, T348, T350 | | |
| 02:§10.7 Toplu veri işlemleri | T302, T341, T357, T384, T525 | | |
| 02:§10.7.1 Siparişler dışa aktarılır | T384, T357, T302 | | |
| 02:§10.7.2 Ürün içe aktarma ve toplu güncelleme yoktur | T525, T384 | | |
| 02:§10.7.3 Katalog ve içerik için ürün içinde dışa aktarma yoktur; | T384 | | |
| 02:§10.7.4 Sipariş dışındaki kişilere ulaşma listesi de dışa aktarılır | T384, T341 | | |
| 02:§10.8 İlk kurulum kontrol listesi | T331, T360, T366, T382 | | |
| 02:§10.8.1 Site boş kurulur; | T360 | | |
| 02:§10.8.2 Panel ana sayfasında bir kurulum kontrol listesi durur | T382, T331, T366 | | |
| 02:§11.1 Firma ayarları | T312, T324, T325, T327, T332, T335, T336, T340, T347, T348, T349, T350, T353, T355, T381, T503, T508, T509, T511, T525, T536 | | |
| 02:P-1 Kargo ücreti — sipariş başına, KDV dahil | T381, T349, T536 | | |
| 02:P-2 Kaldırıldı — kargo ücretinin KDV'si fiziksel kalemlerin oranını izler, firma ayarı değildir (§3.19.2… | — | | |
| 02:P-3 Ücretsiz kargo eşiği | T381, T349, T508, T509, T536 | | |
| 02:P-4 Asgari sipariş tutarı | T381, T349 | | |
| 02:P-5 Kargoya verme süresi — firma varsayılanı | T381, T325, T350, T503, T525, T536 | | |
| 02:P-6 Kargoya verme süresi — ürün düzeyi | T381, T350, T525, T536 | | |
| 02:P-7 Havale ödeme süresi | T381, T324, T327, T348, T353, T536 | | |
| 02:P-8 İndirme hakkı — sipariş kalemi başına | T381, T312, T340, T355, T511 | | |
| 02:P-9 Bir siparişte en fazla adet — ürün başına | T381, T347, T503, T525 | | |
| 02:P-10 KDV oranı varsayılanı | T381, T336, T525 | | |
| 02:P-11 Kaldırıldı — duyuru metni firma ayarı değil, kurumsal içerik kaydıdır (§3.27.21); kimlik yeniden kul… | — | | |
| 02:P-44 İfa süresi — hizmet ürünü | T381, T325, T332, T335, T340, T525 | | |
| 02:§11.2 Ürün sabitleri | T106, T108, T201, T215, T302, T305, T311, T312, T324, T326, T327, T332, T333, T338, T339, T340, T341, T342, T343, T348, T352, T353, T354, T358, T359, T360, T362, T365, T370, T373, T376, T377, T381, T385, T409, T506, T511, T514, T515, T516, T517, T519, T520, T525, T526, T534, T536, T540, T543 | | |
| 02:P-12 E-posta doğrulama bağlantısının ömrü | T381, T324, T341, T514 | | |
| 02:P-13 Şifre sıfırlama bağlantısının ömrü — Google girişinin teyit bağlantısı da bu ömrü taşır | T381, T324, T342, T343, T409, T516 | | |
| 02:P-14 Yönetici daveti bağlantısının ömrü | T381, T106, T324, T520 | | |
| 02:P-47 E-posta değişikliğini geri alma bağlantısının ömrü | T381, T302, T343, T517, T519 | | |
| 02:P-15 Kısa oturum — "beni hatırla" işaretsiz | T381, T324, T342, T515 | | |
| 02:P-16 Uzun oturum — "beni hatırla" işaretli | T381, T324, T342, T515 | | |
| 02:P-48 Girişte tanınan tarayıcı işaretinin ömrü | T381, T302, T342, T377 | | |
| 02:P-17 Asgari şifre uzunluğu | T381, T342, T517 | | |
| 02:P-18 Kart ödeme süresi | T381, T311, T324, T348, T352, T536 | | |
| 02:P-19 Sipariş numarasının rastgele blok uzunluğu | T381, T354 | | |
| 02:P-20 Kategori ağacının derinliği | T381, T333, T526 | | |
| 02:P-21 Ürün başına seçenek boyutu | T381, T332, T525 | | |
| 02:P-22 Kabul edilen görsel biçimleri | T381, T339, T359, T362, T525, T534 | | |
| 02:P-23 Tek görsel dosyasının boyut tavanı | T381, T339, T359, T362, T525, T534 | | |
| 02:P-24 Ürün başına görsel adedi — ürünün ve varyantlarının görselleri birlikte | T381, T339, T525 | | |
| 02:P-25 Kurumsal içerik kaydı başına görsel adedi | T381, T359, T534 | | |
| 02:P-26 Dijital ürün dosyasının boyut tavanı | T381, T312, T340 | | |
| 02:P-27 Ürün adının ve kurumsal içerikte ad ya da başlığın karakter tavanı | T381, T339, T359, T525, T534 | | |
| 02:P-28 SSS sorusunun, kısa açıklamanın ve şubenin çalışma saatlerinin karakter tavanı | T381, T358, T359, T506, T534 | | |
| 02:P-29 Hakkımızda kısa tanıtımının karakter tavanı | T381, T358, T359, T534 | | |
| 02:P-30 Duyuru metninin karakter tavanı | T381, T360, T534 | | |
| 02:P-31 L-1 Başarısız giriş denemesi | T381, T215, T342, T376 | | |
| 02:P-32 L-2 Şifre sıfırlama talebi | T381, T215, T342, T376 | | |
| 02:P-33 L-3 Hesap kaydı | T381, T215, T341, T342, T376 | | |
| 02:P-34 L-4 İletişim formu gönderimi | T381, T215, T365, T376 | | |
| 02:P-35 L-5 Başarısız misafir sipariş takibi sorgusu | T381, T215, T354, T376, T511 | | |
| 02:P-36 L-6 Geçersiz kupon kodu denemesi | T381, T338, T376, T526 | | |
| 02:P-37 L-7 Kendiliğinden iptal edilen ödenmemiş sipariş | T381, T215, T348, T376, T511 | | |
| 02:P-46 L-8 Aynı anda açık ödenmemiş sipariş | T381, T215, T348, T353, T376, T511 | | |
| 02:P-49 L-9 Doğrulama bağlantısının yeniden istenmesi | T381, T341, T343, T376, T514 | | |
| 02:P-38 Gönderilemeyen e-postanın yeniden deneme aralıkları | T381, T385, T305, T326 | | |
| 02:P-39 Periyodik imha aralığı | T381, T201 | | |
| 02:P-40 İletişim talebinin saklama süresi | T381, T302, T365 | | |
| 02:P-41 Bildirim gönderim kaydının saklama süresi — hesap ve yönetim e-postaları ile firmaya giden bildiriml… | T381, T302, T305 | | |
| 02:P-43 Malı dönmeyen caymada havale hattı IBAN'ının saklama süresi | T381, T327, T370, T373, T543 | | |
| 02:P-45 Giriş kaydının ve sistem kayıtlarının saklama süresi | T381, T106, T108, T201, T302, T341 | | |
| 02:P-42 En dar desteklenen ekran genişliği | T381, T540 | | |
| 02:§11.3 Hacim kabulleri | T383 | | |
| 02:H-1 Katalog | — | | |
| 02:H-2 Sipariş hacmi | T383 | | |
| 02:H-3 Sepet | — | | |
| 02:H-4 Eşzamanlı ziyaretçi | — | | |
| 02:§12.1 Ticari çerçeve ve tüketici rejimi | T216, T305, T306, T328, T330, T336, T337, T348, T350, T352, T355, T356, T357, T366, T368, T370, T371, T372, T373, T374, T384, T401, T501, T503, T535 | | |
| 02:§12.1.1 Tek pazar Türkiye'dir; | T216, T330 | | |
| 02:§12.1.2 Ürün tüketiciye satış için kurulmuştur | T356, T366 | | |
| 02:§12.1.3 Satıcı firmadır | T352, T401 | | |
| 02:§12.1.4 ETBİS kaydı ve yasal kimlik | T330, T535 | | |
| 02:§12.1.5 Fiyat tüketiciye KDV dahil gösterilir | T336, T501, T503 | | |
| 02:§12.1.6 İndirim beyanı referans fiyata dayanır | T337, T355 | | |
| 02:§12.1.7 Mesafeli satış | T356, T366, T348, T355, T305, T306 | | |
| 02:§12.1.8 Teslim | T368, T350, T370 | | |
| 02:§12.1.9 Cayma, iade ve ayıp | T371, T372, T373, T374, T370 | | |
| 02:§12.1.10 Fatura ürünün dışındadır | T357, T384 | | |
| 02:§12.1.11 Ticari elektronik ileti gönderilmez | T328, T305 | | |
| 02:§12.1.12 Ürün ne satıldığını denetlemez | — | | |
| 02:§12.2 Kişisel verilerin korunması | T201, T209, T301, T302, T331, T341, T342, T345, T346, T356, T365, T366, T370, T375, T377, T379, T380, T384, T401, T409, T507, T601 | | |
| 02:§12.2.1 Veri sorumlusu firmadır | T366, T601 | | |
| 02:§12.2.2 Aydınlatma yükümlülüğü | T366, T507, T356 | | |
| 02:§12.2.3 Hukuki sebepler KVKK m.5/2'nin kapalı sayımıyla yazılır; açık rıza alınmaz | T366, T301, T302 | | |
| 02:§12.2.4 Ürün hiçbir noktada açık rıza toplamaz | T356, T366 | | |
| 02:§12.2.5 Çerezler | T366, T331, T346, T209, T342, T377 | | |
| 02:§12.2.6 İletişim formunun "KVKK talebi" tipi ürünün sunduğu başvuru kanalıdır | T365, T345 | | |
| 02:§12.2.7 Saklama ve imha | T201, T301, T302, T345, T370, T375 | | |
| 02:§12.2.8 Kişisel verinin yurt dışına aktarımı | T366, T209, T401, T409, T601 | | |
| 02:§12.2.9 Veri ihlali | T380, T384, T341 | | |
| 02:§12.2.10 Yönetim tarafında veri erişimi ayrımı yoktur | T379 | | |
| 02:§12.2.11 Kurumsal içerikte başkasına ait kişisel veriyi yayımlamanın sorumluluğu firmadadır | — | | |
| 02:§12.2.12 Veri asgariliği | T356, T365, T341 | | |
| 02:§12.3 Veri sahipliği ve süreklilik | T327, T384, T601 | | |
| 02:§12.3.1 Veri firmanındır | T601 | | |
| 02:§12.3.2 Çıkış hakkı barındırma düzleminde karşılanır | T384, T601 | | |
| 02:§12.3.3 Ürün çalışma süresi taahhüdü vermez ve sipariş tarafında veri kaybına tolerans sıfırdır — tek istisn… | T327, T601 | | |
| 02:§12.4 Erişilebilirlik | T540, T543, T544 | | |
| 02:§12.4.1 Erişilebilirlik yasal bir yükümlülüktür; taban Kontrol Listesi A Seviyesi ile WCAG 2.2'dir, hedef WC… | T540, T543, T544 | | |
| 02:§12.5 Firmanın yükümlülükleri | T330, T357, T366, T370, T372, T373, T375, T384, T601 | | |

**`05`** — Teknik Mimari (O5 1/2: MK-01…MK-78 kararı 78 · `###` alt bölümü 58)

| Kaynak öğe | Task | Implement edildi | Kanıt |
|---|---|---|---|
| 05:MK-01 C# / .NET; tek uygulama, sayfalar sunucuda üretilir | T02 | | |
| 05:MK-02 ASP.NET Core; .NET 10, yalnız uzun süreli destekli (LTS) sürümler | T02, T06 | | |
| 05:MK-03 Entity Framework Core; şema değişiklikleri onun göçleriyle | T02, T101 | | |
| 05:MK-04 Razor Pages; bütün sayfalar sunucuda tam HTML, gönder–yönlendir–göster; ayrı JSON API katmanı yok | T02, T206 | | |
| 05:MK-05 Düz JavaScript modülleri ve düz CSS; ön yüz çatısı ve npm hattı yok, JavaScript yalnız iyileştirir | T02, T540, T539 | | |
| 05:MK-06 PostgreSQL 18; tek örnek, aynı sunucuda kendi konteynerinde | T05, T06, T101 | | |
| 05:MK-07 Firmanın adına kiralanan sanal sunucu; Linux konteynerleri | T05, T601 | | |
| 05:MK-08 Caddy 2 | T05 | | |
| 05:MK-09 Veritabanının yedeği pgBackRest'le, veritabanı konteynerinde; dosyaların ve sistem kayıtlarının yedeği bir… | T06, T07 | | |
| 05:MK-10 Tek Compose tanımı; her imaj etiket ve özetle sabit | T05, T06 | | |
| 05:MK-11 GitHub Container Registry | T06 | | |
| 05:MK-12 GitHub Actions; iskelet değişmez, `publish.yml` kalır, kurulumlara otomatik dağıtım yok | T04 | | |
| 05:MK-13 Tek dağıtılabilir uygulama | T218 | | |
| 05:MK-14 Sunucuda dört konteyner | T05 | | |
| 05:MK-15 Dokuz modül; modüller arası erişim yalnız açık arayüzden, eşzamanlı ve aynı işlemde; uygulama içi olay yayı… | T218 | | |
| 05:MK-16 Vitrin ve panel aynı alan adında; panel ayrı bir yol alanı | T206, T207 | | |
| 05:MK-17 Uygulamanın çağırdığı üç dış servis bir bağlantı katmanının arkasında; ürün kuralı çekirdekte; dış serviste… | T202, T203 | | |
| 05:MK-18 Adres addan sunucuda üretilir; arama motoru verileri JSON-LD ve Open Graph ile ilk yanıtta; kapalı sayfalar… | T219, T206 | | |
| 05:MK-19 Zaman UTC'de tutulur, Türkiye saatiyle sayılır; para ondalıklı sayıyla; yuvarlama tek işlevden | T227, T101 | | |
| 05:MK-20 Kayıt varyantı: iyimser eşzamanlılık, sürüm değeri, kilit yok | T228, T208 | | |
| 05:MK-21 Sipariş varyantı: sürüm ve kısa satır kilidi; kilit sırası bütün işlemlerde aynı | T228, T348 | | |
| 05:MK-22 Tek gecelik imha işi; süre veriden hesaplanır; imha kaydı ayrı tabloda | T201 | | |
| 05:MK-23 İmha fiziksel silmedir; siparişte ve ona bağlı kayıtlarda kişisel alanlar boşaltılır | T201, T301, T302 | | |
| 05:MK-24 İz tablosunda uygulama kullanıcısı yalnız ekler ve okur; süresi dolan satırı veritabanı fonksiyonu siler | T106, T380 | | |
| 05:MK-25 Sürekli günlük aktarımı | T07, T601 | | |
| 05:MK-26 Yedeğin kapsamı, yeri, iki anahtarla şifrelenmesi, her kopyada otuz günlük saklaması | T07 | | |
| 05:MK-27 Kurtarma hedefi aynı gün | T601, T07 | | |
| 05:MK-28 Dosyalar sunucunun diskinde, dosya deposu arayüzüyle; dijital dosya genel adresle sunulmaz | T223 | | |
| 05:MK-29 Yüklenen görsel yeniden kodlanır | T223, T339 | | |
| 05:MK-30 Dijital ürün dosyası parçalı yüklenir; her yükleme yeni bir dosya kaydıdır | T312, T340 | | |
| 05:MK-31 İndirme hakkı düğmeye basılıp gönderim başlayınca bir kez düşer; bir saatlik bağlantı, ömrü isteğin kabulün… | T312, T340, T531 | | |
| 05:MK-32 Önerilen stok kodu ürünün adresinden ve varyantın sırasından; kaydedilmemiş üründe o anki addan, ad boşsa ö… | T332 | | |
| 05:MK-33 Site içi arama veritabanında, sadeleştirilmiş ad üzerinde içerir eşleşmesiyle | T333 | | |
| 05:MK-34 Önbellek uygulamanın belleğinde; ayrı önbellek sunucusu yok | T229 | | |
| 05:MK-35 Durum makinesi tek geçiş biriminde; beyaz liste | T315, T319 | | |
| 05:MK-36 Onay isteği müşterinin gördüğünü taşır; onay tek işlemde, koşullu ayırmayla | T348 | | |
| 05:MK-37 İç yan etkiler geçişle aynı işlemde, dış yan etkiler veritabanındaki iş kuyruğunda, e-posta gönderim kaydın… | T220, T305 | | |
| 05:MK-38 Her e-posta gönderim kaydında bir satır olarak doğar; gövde gönderim anında üretilir | T305, T220, T117 | | |
| 05:MK-39 Tek işlenme: onay anahtarı, önce kaydedilen sağlayıcı bildirimi | T311, T348, T403 | | |
| 05:MK-40 Karta para gönderen istekten önce geri ödeme kaydı; sonucu bilinmeyen istek yeniden gönderilmez, sonuç bekl… | T370, T404, T405 | | |
| 05:MK-41 Uygulamanın içinde tek zamanlayıcı | T221 | | |
| 05:MK-42 İşlerin envanteri | T220, T221 | | |
| 05:MK-43 Hatırlatma 08:00, imha 03:00, son sorgu beş dakikada bir | T221 | | |
| 05:MK-44 Kesinti nabızla ölçülür; dönüşte havale iptali ertelenir, kaçırılmış hatırlatması düşer | T221, T327 | | |
| 05:MK-45 Oturum sunucuda tutulur; iki hesap türü iki kimlik şeması | T207, T342 | | |
| 05:MK-46 Şifre çatının özetleyicisiyle; Google ile giriş OpenID Connect'in yetki kodu akışı | T342, T343, T409 | | |
| 05:MK-47 Yetki sunucuda, varsayılan kapalı, kayıt düzeyinde süzülerek | T207, T323 | | |
| 05:MK-48 Deneme limitleri veritabanında, deneme kaydıyla; IPv6'da /64 | T215, T108 | | |
| 05:MK-49 Genel istek hızı sınırı uygulamada | T215, T203 | | |
| 05:MK-50 Anahtarlar kriptografik üreteçle | T222, T108 | | |
| 05:MK-51 E-postayla gelen bağlantının açılması işlem yapmaz; işlem ekrandaki düğmeyle | T222, T342 | | |
| 05:MK-52 Her POST sahteciliğe karşı korumadan geçer | T209 | | |
| 05:MK-53 Güvenlik başlıkları: içerik güvenlik politikası | T209 | | |
| 05:MK-54 Görselin türü içerikten; piksel tavanı; dijital dosyada tür sınırı yok, ek olarak sunulur | T223, T339 | | |
| 05:MK-55 Bütün çerezler birinci taraf, zorunlu, Secure, HttpOnly, SameSite=Lax, `__Host-` önekli | T224 | | |
| 05:MK-56 Ödeme adımında yazılanlar oturumluk, şifreli bir çerezde; sunucuda kayıt yok | T224, T356 | | |
| 05:MK-57 Çerez politikası yayına alınana kadar vitrinin yanıtlarından çerez başlıklarını çıkaran ara katman | T224 | | |
| 05:MK-58 Yalnız HTTPS ve bir yıllık HSTS; kart verisi ürüne gelmez; alan düzeyinde şifreleme yok | T209, T05 | | |
| 05:MK-59 Sırlar bileşen başına dosyalarda, yalnız kullanan konteynere; paylaşılan sır yok | T08 | | |
| 05:MK-60 Döndürme yılda bir ve olay üzerine; iki yedeğin şifreleme anahtarı döndürülmez; sırların ayrı yedeği yok | T08 | | |
| 05:MK-61 Dış çağrı süreli | T202 | | |
| 05:MK-62 Kanal uyarısı art arda beş kanal hatası ve on beş dakikada; firma bildirimleri uyarısı gönderim kaydından,… | T314, T382 | | |
| 05:MK-63 Beklenmeyen hata tek yakalama katmanında üç hâle ayrılır; sonucu belirsiz işlemler kodda adıyla işaretli | T212 | | |
| 05:MK-64 Sistem kayıtları üç kaynak | T225 | | |
| 05:MK-65 Sistem kayıtları P-45 kadar saklanır; kayıt dosyalarını imha işi siler; sunucunun kayıtları kendi döngüleriyle | T225, T201 | | |
| 05:MK-66 Kayıtları kurulumu yapan okuma komutuyla okur; sonuç şifreli iletilir ve sunucudan silinir | T387 | | |
| 05:MK-67 Canlılık ve durum uçları | T314, T01 | | |
| 05:MK-68 Dört ortam; ayrı hazırlık sunucusu yok, referans kurulum öncüdür | T05, T09 | | |
| 05:MK-69 Bir sürüm tek eser | T06 | | |
| 05:MK-70 CI adımlarının işi | T04 | | |
| 05:MK-71 Üç ayar katmanı ve tasarım değerleri; açılışta ayar denetimi | T226 | | |
| 05:MK-72 Kurulum tarafının işlemleri uygulama imajındaki yönetim komutlarıyla | T387, T388, T303 | | |
| 05:MK-73 Güncellemeyi kurulumu yapan tek komutla, önce referans kurulumda, sonra teker teker başlatır; kurulumlar ke… | T602 | | |
| 05:MK-74 Göçler genişlet–daralt | T602, T04, T101 | | |
| 05:MK-75 İşletim sistemi yamaları kendiliğinden, gerekirse 04:30'da yeniden başlatma; aylık bakım sürümü ve acil yama | T601 | | |
| 05:MK-76 Sunucuya yalnız SSH ve anahtarla, kurulum başına anahtar; açık kapılar 80, 443 ve SSH | T05, T601 | | |
| 05:MK-77 H-3: isteklerin %95'i bir saniyede; H-4: dakikada 100 farklı ziyaretçi | — | | |
| 05:MK-78 Büyüme yolunun sırası ve tetikleyicileri | — | | |
| 05:§3.1 Uygulamanın biçimi | T02, T218 | | |
| 05:§3.2 Sunucudaki bileşenler ve ağ | T05, T06 | | |
| 05:§3.3 Modüller | T218 | | |
| 05:§3.4 Modüller arası iletişim kuralı | T218 | | |
| 05:§3.5 Vitrin ve panel | T206, T207 | | |
| 05:§3.6 Dış servislerin sınırı — bağlantı katmanı | T202, T203, T401, T404 | | |
| 05:§3.7 Sayfa adresleri ve arama motoru verileri | T219, T313, T206 | | |
| 05:§3.8 Ön yüz ve tasarım tabanının uygulanması | T540, T539, T02 | | |
| 05:§4.1 Durum makinesinin yeri — geçiş birimi | T315, T319 | | |
| 05:§4.2 Sipariş onayı | T348 | | |
| 05:§4.3 Yan etkiler ve iş kuyruğu — olay kaybı | T220 | | |
| 05:§4.4 E-postanın gönderimi ve gönderim kaydı | T117, T220, T305, T375, T385 | | |
| 05:§4.5 Tek işlenme — sipariş onayı, sağlayıcı bildirimi ve son sorgu | T311, T348, T403 | | |
| 05:§4.6 Karta para gönderen isteğin tek işlenmesi | T370, T404, T405 | | |
| 05:§4.7 İşlem izi | T380, T106 | | |
| 05:§5.1 Çalıştırıcılar — zamanlayıcı ve iş kuyruğu | T220, T221 | | |
| 05:§5.2 İşlerin envanteri | T201, T220, T221, T302, T385, T408 | | |
| 05:§5.3 İşlerin saatleri ve gecenin sırası | T221 | | |
| 05:§5.4 Kesintinin tespiti ve dönüşteki sıra | T221, T327 | | |
| 05:§6.1 Kimlik doğrulama ve oturum | T207, T342, T343, T345, T379, T409 | | |
| 05:§6.2 Yetkilendirme | T207, T323 | | |
| 05:§6.3 Deneme limitleri ve istek hızı | T05, T215, T601 | | |
| 05:§6.4 Anahtarlar ve tek kullanımlık bağlantılar | T222 | | |
| 05:§6.5 Form ve site güvenliği | T207, T209, T339, T401 | | |
| 05:§6.6 Yüklenen dosya | T223 | | |
| 05:§6.7 Çerezler ve tarayıcıda tutulan veri | T224 | | |
| 05:§6.8 Hassas verinin taşınması ve saklanması | T209, T05, T07 | | |
| 05:§6.9 Kurulum tarafının işlemleri ve bakım erişimi | T303, T387, T389, T601 | | |
| 05:§6 6.x Secret yönetimi (bu alt bölüm zorunludur) | T08, T222 | | |
| 05:§7.1 Veritabanı ve işlem bütünlüğü | T10, T101, T228 | | |
| 05:§7.2 Zamanın ve paranın temsili | T227 | | |
| 05:§7.3 Eşzamanlılık kontrolü | T228, T208 | | |
| 05:§7.4 Saklama ve imha | T201, T301, T302 | | |
| 05:§7.5 İşlem izinin korunması | T106, T380, T201 | | |
| 05:§7.6 Yedekleme ve kurtarma | T07, T232, T601 | | |
| 05:§7.7 Dosya depolama ve dijital teslim | T223, T302, T312, T340 | | |
| 05:§7.8 Önerilen stok kodu | T332 | | |
| 05:§7.9 Site içi arama | T333 | | |
| 05:§7.10 Önbellek | T229 | | |
| 05:§8.1 Ortamlar | T05, T09 | | |
| 05:§8.2 Paketleme ve kayıt defteri | T06 | | |
| 05:§8.3 Dal stratejisi ve CI/CD hattı | T04, T03 | | |
| 05:§8.4 Konfigürasyon yönetimi | T226 | | |
| 05:§8.5 Yönetim komutları ve ilk kayıtlar | T303, T387, T388, T389 | | |
| 05:§8.6 Güncellemenin kurulumlara ulaşması | T602 | | |
| 05:§8.7 Göçler ve geri alma | T602, T04, T388 | | |
| 05:§8.8 Güvenlik güncellemeleri | T601 | | |
| 05:§8.9 Sunucuya bakım erişimi | T601, T05 | | |
| 05:§9.1 Sistem kayıtları — kaynaklar, içerik ve maskeleme | T225, T230, T231 | | |
| 05:§9.2 Kayıtların saklanması ve korunması | T07, T201, T225 | | |
| 05:§9.3 Kayıtların okuma yolu | T387 | | |
| 05:§9.4 İzleme ve uyarı | T01, T07, T314 | | |
| 05:§10.1 Dış bağımlılıklar — "çökerse ne olur" | T204, T202 | | |
| 05:§10.2 E-posta kanalının ve firma bildirimlerinin uyarısı | T314, T382, T385 | | |
| 05:§10.3 Beklenmeyen hata | T212 | | |
| 05:§11.1 Tasarım değerleri | T226 | | |
| 05:§11.2 Bilinçli tekil bırakılanlar | — | | |
| 05:§11.3 Büyüme yolu | — | | |

**`09`** — Kodlama Kılavuzu (O5 2/2: `## n.` bölümü 25)

| Kaynak öğe | Task | Implement edildi | Kanıt |
|---|---|---|---|
| 09:§1 Temel ilkeler | — | | |
| 09:§2 Source of truth hiyerarşisi | — | | |
| 09:§3 Genel kod yazım kuralları | T02, T03, T04, T211, T226, T381 | | |
| 09:§4 Proje yapısı ve klasör organizasyonu | T02, T05, T06, T202 | | |
| 09:§5 Adlandırma konvansiyonları | T101, T120, T206, T213, T224 | | |
| 09:§6 Mimari sınırlar | T03, T218, T223 | | |
| 09:§7 Zaman ve biçim standartları | T211, T216, T227, T378 | | |
| 09:§8 Hata yönetimi | T205, T212, T213, T214 | | |
| 09:§9 Domain kuralları | T101, T106, T201, T218, T219, T228, T301, T315, T333, T348, T355, T356, T363, T380 | | |
| 09:§10 Veri erişimi | T01, T04, T10, T101, T118, T119, T208, T210, T228, T229, T233, T333, T384, T386 | | |
| 09:§11 Entegrasyon kuralları | T202, T203, T305, T366, T401, T407, T409, T501 | | |
| 09:§12 Sözleşme ve olay sürümleme | T101, T102, T203, T220, T311, T538 | | |
| 09:§13 Arka plan iş kuralları | T220, T221, T303, T387, T388, T389 | | |
| 09:§14 Hassas hesaplama kuralları | T227, T336, T337, T338, T349, T369, T370, T401, T402, T403, T405 | | |
| 09:§15 Güvenlik kuralları | T207, T209, T215, T219, T222, T223, T224, T232, T312, T339, T342, T354, T376 | | |
| 09:§16 İstemci tarafı kurallar | T525, T539, T540, T543, T544, T548 | | |
| 09:§17 Yardımcı servis / sidecar standartları (varsa) | T06, T07, T08, T10 | | |
| 09:§18 Loglama ve gözlemlenebilirlik | T225, T230, T231, T314 | | |
| 09:§19 Test kuralları | T03, T04, T120, T202, T329, T538, T546, T547 | | |
| 09:§20 Performans kuralları | T03, T210, T220, T223, T229 | | |
| 09:§21 Git ve CI/CD kuralları | T04, T06, T09 | | |
| 09:§22 Refactor kuralları | — | | |
| 09:§23 Yasaklar | T03, T04 | | |
| 09:§24 AI çalışma kuralları | — | | |
| 09:§25 Review checklist | — | | |

**`10`** — MVP Kapsamı (O5 2/2: KP satırı 77 · KD satırı 64 · alt bölüm 2)

| Kaynak öğe | Task | Implement edildi | Kanıt |
|---|---|---|---|
| 10:KP-1 Ziyaretçi kategori ağacında gezer; alt dal ürünleri listelenir, boş kategori menüde görünmez | T333, T361, T502 | | |
| 10:KP-2 Ürün adında arama; büyük-küçük harfe ve Türkçe karaktere duyarsız | T333, T503 | | |
| 10:KP-3 Fiyat aralığı ve stok durumu süzgeci; sabit düzen, en yeni önce | T210, T333, T503 | | |
| 10:KP-4 Varyant seçimi (en çok iki boyut), ürün başına tek kart, seçilemeyen kombinasyon | T332, T334, T503 | | |
| 10:KP-5 Ürün sayfası: görsel, KDV dahil fiyat, tarih, üretim yeri, yerli logo, birim fiyat, referans fiyat | T332, T336, T337, T339, T501, T503 | | |
| 10:KP-6 Üç ürün tipi (fiziksel, dijital, hizmet) tek sepette; hizmetin ifa süresi | T332, T351, T356, T503 | | |
| 10:KP-7 Arşivlenmiş, taslak ve silinmiş ürün adresi sayfaları | T321, T335, T363, T503, T504 | | |
| 10:KP-8 Hesapsız sipariş; misafir e-postası onay özetinde, firma düzeltmesi | T341, T348, T356, T386, T509 | | |
| 10:KP-9 Sepete ekleme: güncel fiyat, adet sınırları, dijital ürün uyarısı | T340, T346, T347, T508 | | |
| 10:KP-10 Üye ve misafir sepeti, girişte birleşme, kendiliğinden boşalmama | T346, T508 | | |
| 10:KP-11 Kupon kodu girişi; sıfıra indirememe | T338, T348, T508 | | |
| 10:KP-12 Teslimat ve fatura adresi, adres defteri, alan kümesi | T344, T509, T543 | | |
| 10:KP-13 Kargo ücreti, ücretsiz eşik, asgari tutar, kapalı il | T344, T349, T350, T509 | | |
| 10:KP-14 Onay özeti, Ön Bilgilendirme Formu, onay kutuları, sürüm değişince sipariş oluşmaz | T348, T356, T366, T509 | | |
| 10:KP-15 Kart (3D Secure) ve havale/EFT ile ödeme; yarım kalan ödeme, geç gelen ödemenin geri ödenmesi | T311, T348, T352, T353, T370, T401, T509, T510 | | |
| 10:KP-16 Siparişin stoğunun, kontenjanın ve kupon hakkının ayrılması | T334, T338, T348 | | |
| 10:KP-17 Sipariş numarası ve sipariş anında donan değerler | T348, T354, T355, T375 | | |
| 10:KP-18 Sipariş takibi: üç giriş yolu, durum, geri ödeme, takip, indirme, IBAN | T317, T354, T511 | | |
| 10:KP-19 Kargoya verilme süresi vitrinde, kargo şirketi ve takip bağlantısı | T310, T350, T511 | | |
| 10:KP-20 Dijital ürünün indirilmesi: hak, güncel dosya | T312, T340, T511 | | |
| 10:KP-21 Müşteri iptali ve gecikme nedeniyle fesih | T367, T368, T369, T370, T512 | | |
| 10:KP-22 Cayma: pencere, beyan, iade gönderimi, geri ödeme süresi, istisnalar | T369, T370, T371, T372, T373, T513 | | |
| 10:KP-23 Ayıp talebi: kalem, süre, çözüm, yeniden açma | T320, T374, T513 | | |
| 10:KP-24 Tek kalemde ve adet düzeyinde iptal ya da iade; kupon payı | T367, T369, T373 | | |
| 10:KP-25 Ad, e-posta ve şifreyle kayıt; doğrulanmadan giriş yok | T331, T341, T366, T514 | | |
| 10:KP-26 Google ile giriş ve hesap eşleme | T343, T409, T515 | | |
| 10:KP-27 Uzun oturum, şifre, ad ve e-posta değişikliği, geri alma, yeniden doğrulama | T342, T343, T344, T515, T516, T517, T519 | | |
| 10:KP-28 Misafir siparişlerinin hesaba düşmesi | T341, T375 | | |
| 10:KP-29 Hesabın silinmesi (müşteri ve firma); ticari kayıt kalır | T345, T517, T533 | | |
| 10:KP-30 Kişisel veri başvurusu: iletişim formunun "KVKK talebi" tipi | T345, T365, T506 | | |
| 10:KP-31 Ana sayfada tanıtım ve vitrin; düzen seçimi | T361, T502, T535 | | |
| 10:KP-32 Hakkımızda, hizmet, referans iş, SSS, genel sayfa; ilgili ürünler | T358, T359, T505 | | |
| 10:KP-33 İletişim sayfası: bilgiler, şubeler, harita bağlantısı, form | T358, T506 | | |
| 10:KP-34 İletişim formundan talep gönderme | T308, T365, T506 | | |
| 10:KP-35 Yasal kimlik ve iletişim bilgileri, ETBİS bandı, altbilgi bağlantıları | T330, T356, T366, T537 | | |
| 10:KP-36 Sosyal bağlantılar, WhatsApp, duyuru şeridi, paylaş düğmesi | T358, T360, T363, T537 | | |
| 10:KP-37 Arama motoru verileri, kapalı sayfalar, sabit adresler | T219, T313, T363 | | |
| 10:KP-38 Satış kapalıyken vitrin yayında, sepet ve ödeme kapalı | T331, T508, T541 | | |
| 10:KP-39 Ürün tanımı ve yayına alma: varyant, stok, fiyat, KDV, ölçü, ifa süresi | T332, T334, T335, T336, T525 | | |
| 10:KP-40 Ürün ve varyant durumları, önizleme, kalıcı silme | T321, T335, T525 | | |
| 10:KP-41 Kategori ağacı kurma, birden çok kategori, silme sınırı | T333, T526 | | |
| 10:KP-42 Ürün görselleri, alternatif metin, kapalı metin biçimi seti | T223, T312, T339, T525 | | |
| 10:KP-43 İndirim, referans fiyat ve kupon tanımı | T337, T338, T525, T526 | | |
| 10:KP-44 Dijital ürün dosyası ve indirme hakkının yenilenmesi | T312, T340, T525, T531 | | |
| 10:KP-45 Cayma hakkı istisnası işareti ve kapalı sebep listesi | T371, T525 | | |
| 10:KP-46 Sipariş listesi, havale ödendi, kargoya verme, teslim, teslim edilemedi, hizmet tamamlandı | T316, T318, T319, T350, T353, T527 | | |
| 10:KP-47 Firma iptali, kart ve havale geri ödemesi, iade teslim alma, cayma bildirimi kaydı | T368, T370, T372, T373, T528, T529 | | |
| 10:KP-48 Siparişe müdahale: adres, kalem, tarih, durum düzeltmesi, iç not, e-posta düzeltmesi | T375, T386, T530, T531 | | |
| 10:KP-49 Ayıp talebi ve iletişim talebinin çözülmesi, kapatılması, yeniden açılması | T365, T374, T532 | | |
| 10:KP-50 Panel ana sayfasında altı sayaç ve kanal uyarıları | T314, T382, T521 | | |
| 10:KP-51 Manuel adım bütçesi: en çok üç sistem içi adım | T318 | | |
| 10:KP-52 Siparişlerin, üyelerin ve iletişim taleplerinin dışa aktarılması | T357, T384, T524 | | |
| 10:KP-53 Satış özeti ve mağaza ölçüleri | T383, T524 | | |
| 10:KP-54 Kurumsal içerik girişi: taslak, önizleme, yayın, sıralama, ürün bağı, silme | T358, T359, T360, T534 | | |
| 10:KP-55 Ana sayfa düzeni, menü adları, sosyal bağlantılar, WhatsApp | T361, T535 | | |
| 10:KP-56 Logo, marka adı, site simgesi, marka rengi, platform imzası | T362, T534 | | |
| 10:KP-57 Duyuru şeridi: taslak, önizleme, tarih aralığı | T360, T534 | | |
| 10:KP-58 Firma kimliği: tip, zorunlu alanlar, KEP, ETBİS | T330, T535 | | |
| 10:KP-59 Havale/EFT ve IBAN, kargo ücreti, eşik, asgari tutar, iller, süreler, iade adresi | T331, T349, T350, T353, T536 | | |
| 10:KP-60 Satışın geçici kapatılması; açık siparişler etkilenmez | T331, T535 | | |
| 10:KP-61 Aydınlatma metni, çerez politikası, üretilen iki metin, sürümlü dört metin | T366, T507 | | |
| 10:KP-62 Yönetici daveti, kaldırma, ayrı hesap türü, e-posta değişikliği | T379, T520, T522, T523 | | |
| 10:KP-63 İşlem izi: kapsam, süzgeç, on yıl, değiştirilemez | T380, T522 | | |
| 10:KP-64 İşletme parametreleri: on firma ayarı, ürün sabitleri | T226, T381, T535, T536 | | |
| 10:KP-65 Kurulum kontrol listesi, boş kurulum | T382, T521 | | |
| 10:KP-66 Müşteri bildirimleri: on altı olay, e-posta içeriği | T305, T306, T307, T309, T328, T407 | | |
| 10:KP-67 Firmaya giden bildirimler ve yöneticilere giden bildirimler | T308, T309, T328 | | |
| 10:KP-68 Yanıtlanabilir e-posta, üç yeniden deneme, "ulaşmadı" işareti, yeniden gönderim | T220, T305, T385 | | |
| 10:KP-69 Duyarlı site; panelin her işi telefondan | T540, T541 | | |
| 10:KP-70 Desteklenen tarayıcı tabanı | T539, T540 | | |
| 10:KP-71 Erişilebilirlik: A Seviyesi, WCAG 2.2, yedi ayrıca sınanan kural | T540, T543, T544 | | |
| 10:KP-72 Deneme limitleri, ödenmemiş sipariş tavanı, giriş kaydı | T215, T376, T377 | | |
| 10:KP-73 Çerez onayı gerektirmeyen site: yalnız zorunlu birinci taraf çerezler | T224, T366 | | |
| 10:KP-74 Aydınlatma bağlantısı, günlük imha, imha kaydı | T201, T225, T301, T302 | | |
| 10:KP-75 Kayıtların saklama süresince kaybolmaması; yedek penceresi | T07, T118, T327 | | |
| 10:KP-76 Eşzamanlı düzenleme uyarısı; siparişte önce tamamlanan geçerli | T208, T228, T364, T542 | | |
| 10:KP-77 Salt okunur üye kaydı görünümü ve firma tarafından hesap silme | T345, T380, T533 | | |
| 10:KD-1 Fatura kesilmez ve saklanmaz; e-Arşiv, e-Fatura ve fatura yükleme yok | T357, T384 | | |
| 10:KD-2 Kargo şirketi entegrasyonu yok | T310, T350 | | |
| 10:KD-3 Havale banka hareketiyle eşleştirilmez | T353 | | |
| 10:KD-4 Kapıda ödeme yok | T352 | | |
| 10:KD-5 Ödeme adımında müşteri notu ya da teslimat talimatı yok | T509 | | |
| 10:KD-6 Ürün yorumu, puanı ve yıldızı yok | T503 | | |
| 10:KD-7 İstek listesi yok | T347 | | |
| 10:KD-8 Öneri blokları yok; ilgili ürünler firmanın elle kurduğu bağdır | T359, T503 | | |
| 10:KD-9 "Stokta haber ver" yok | T334, T503 | | |
| 10:KD-10 Sepet hatırlatma e-postası yok | T328, T346 | | |
| 10:KD-11 Facebook ile giriş yok | T343, T409 | | |
| 10:KD-12 Toplu içe aktarma ve toplu fiyat ya da stok güncelleme yok | T525 | | |
| 10:KD-13 Yönetici müşteri adına elle sipariş oluşturamaz | T375, T527 | | |
| 10:KD-14 Blog yok | T358 | | |
| 10:KD-15 Canlı destek yok | T224, T506 | | |
| 10:KD-16 SMS, anlık bildirim ve bildirim merkezi yok | T305, T328 | | |
| 10:KD-17 Pazarlama iletisi, İYS bağı ve pazarlama onayı yok | T305, T328 | | |
| 10:KD-18 Rol, yetki matrisi ve iki adımlı doğrulama yok | T207, T379 | | |
| 10:KD-19 Ziyaretçi analitiği yok | T224 | | |
| 10:KD-20 Çok dilli değil | T206, T216 | | |
| 10:KD-21 Çok kiracılı kullanım ve operatör paneli yok | T218, T330 | | |
| 10:KD-22 Mobil uygulama ve PWA yok | T539, T540 | | |
| 10:KD-23 Taksit bilgisi gösterilmez | T352, T510 | | |
| 10:KD-24 Parçalı gönderim yok | T316, T350 | | |
| 10:KD-25 Mağazadan teslim ve adlandırılmış teslimat seçenekleri yok | T350 | | |
| 10:KD-26 Kargo ücreti ağırlığa, ölçüye ya da ile göre hesaplanmaz | T349 | | |
| 10:KD-27 Teslimat bölgesi ilçe düzeyinde kısıtlanamaz | T344 | | |
| 10:KD-28 Alkol, tütün, ilaç ve silah gibi ürünlerin ek satış akışı yok | T332, T335 | | |
| 10:KD-29 Yaş için beyan kutusu ve doğum tarihi alanı yok | T356 | | |
| 10:KD-30 Arama açıklamada, kategori adında ve seçenek değerlerinde yapılmaz | T333 | | |
| 10:KD-31 Seçenek değerine göre süzgeç yok | T333 | | |
| 10:KD-32 Ziyaretçiye sıralama seçeneği ve elle sıralama yok | T210, T333 | | |
| 10:KD-33 Stok adedi ve eşik uyarısı gösterilmez | T334, T503 | | |
| 10:KD-34 Ürün sayfasında ters yönlü bağ yok | T359, T503 | | |
| 10:KD-35 Asgari adet tanımlanamaz | T332, T347 | | |
| 10:KD-36 "Verilerimi indir" düğmesi yok | T345, T517 | | |
| 10:KD-37 Üç ve üzeri seçenek boyutu ve dördüncü kategori seviyesi yok | T332, T333 | | |
| 10:KD-38 İçerik ileri tarihe zamanlanarak yayımlanamaz | T322, T335, T360 | | |
| 10:KD-39 Sayfa kurucu ve menü düzenleyici yok | T358, T361 | | |
| 10:KD-40 Kayan afiş ve kampanya görseli yok | T361, T502 | | |
| 10:KD-41 Gömülü harita, video ve sosyal medya akışı yok | T224, T358 | | |
| 10:KD-42 İçeriğe dosya eki ve müşteri dosya yüklemesi yok | T358, T365, T374 | | |
| 10:KD-43 Müşteri görüşleri ayrı tip ya da alıntı bloğu değil | T358 | | |
| 10:KD-44 Referans işte süzgeç, SSS'de başlık bölme ve şube sayfası yok | T358, T359 | | |
| 10:KD-45 Kurumsal içerikte sürüm geçmişi yok | T358, T360 | | |
| 10:KD-46 Yazı tipi, renk paleti, tema ve koyu tema seçimi yok | T362, T540 | | |
| 10:KD-47 Firma siteye kod ekleyemez | T209, T362 | | |
| 10:KD-48 Elle doldurulan SEO alanları ve elle adres düzenleme yok | T219, T363 | | |
| 10:KD-49 Adres yönlendirme ve eski siteden adres taşıma yok | T363 | | |
| 10:KD-50 İletişim talebi takibi ve panelden yanıt yok | T365, T532 | | |
| 10:KD-51 Ayrı şikâyet kanalı yok | T365, T532 | | |
| 10:KD-52 İşlem bildirimleri kapatılamaz; bildirim tercihi ekranı yok | T309, T328 | | |
| 10:KD-53 Ayarlardan üretilen bilgi sayfası ve çelişki denetimi yok | T366, T507 | | |
| 10:KD-54 Satış özeti, işlem izi, katalog ve içerik dışa aktarılamaz | T384, T524 | | |
| 10:KD-55 Üçüncü taraf çerez, izleme pikseli ve çerez bandı yok | T224, T366 | | |
| 10:KD-56 Açık rıza yönetimi yok | T201, T366 | | |
| 10:KD-57 İletişim formunda üçüncü taraf captcha yok | T209, T365 | | |
| 10:KD-58 Veri ihlali modülü ve toplu ihlal bildirimi yok | T225, T380 | | |
| 10:KD-59 Sepete ekleme ve stok yoklamada ürün düzeyinde limit yok | T215, T347 | | |
| 10:KD-60 Planlı bakım penceresi ve bakım modu yok | T314, T331 | | |
| 10:KD-61 Garanti süresi alanı ve garanti belgesi yok | T332, T374 | | |
| 10:KD-62 Ters ibraz ürüne girmez | T317, T352 | | |
| 10:KD-63 Ayrı galeri içerik tipi yok | T339, T358, T359 | | |
| 10:KD-64 İkinci ödeme sağlayıcısı desteklenmez | T202, T352, T401 | | |
| 10:§4.1 Kurulum kontrol listesi (ÖK-1…ÖK-12) | T09, T303, T330, T331, T382, T401, T407, T409, T601 | | |
| 10:§4.2 Kısıtlar ve kabuller (SK-1…SK-10) | T226, T318, T330, T332, T333, T381 | | |

**Devir satırları** — Aşama 8'den ve Aşama 1'den gelen devirler (O5 2/2: karar 9 · park maddesi 5 · gövde anması 4 · D-01 1 = 19; `11_devir_esle.py` A–F)

| Kaynak öğe | Task | Implement edildi | Kanıt |
|---|---|---|---|
| devir:K-423 Yol haritası adaylarının sıralama ölçütü | — | | |
| devir:K-424 Yol haritası adayları arası bağımlılık zincirleri | — | | |
| devir:K-1302 Okunacak 09 bölümleri ve okuma düzeni | — | | |
| devir:K-1354 Yükleme betiği, kayıt biçimleyicisi ve maskeleme testleri | T230, T231, T548 | | |
| devir:K-1355 Test projelerinde sahtelerin paylaşımı ve xUnit v3 sınaması | T03 | | |
| devir:K-1356 Eşitlik denetimlerinin Lint adımına ve yama sürümü denetiminin publish.yml'e girmesi | T02, T04 | | |
| devir:K-1357 Okunacak 09 bölümleri (çekirdek yedi bölüm) | — | | |
| devir:K-1358 Yama sürümü denetimi ve ilk yayında geri alma sınamasının atlanması | T04, T09 | | |
| devir:K-1391 Yükleme betiği, kayıt biçimleyicisi ve maskeleme testleri (açılış) | T230, T231, T548 | | |
| devir:P8.1 Park maddesi 1 — okunacak 09 bölümleri | — | | |
| devir:P8.2 Park maddesi 2 — eşitlik denetimlerinin Lint adımına girmesi | T04 | | |
| devir:P8.3 Park maddesi 3 — yama sürümü denetiminin publish.yml'e girmesi | T04, T09 | | |
| devir:P8.4 Park maddesi 4 — sahtelerin paylaşımı, xUnit v3 sınaması | T03 | | |
| devir:P8.5 Park maddesi 5 — yükleme betiği, kayıt biçimleyicisi, maskeleme testleri | T230, T231, T548 | | |
| devir:04-1948 Gövde anması: yasal metinlerin içeriği (GA-7) ★ | T356, T366, T373 | | |
| devir:09-117 Gövde anması: okunacak 09 bölümleri | — | | |
| devir:09-2249 Gövde anması: okunacak 09 bölümleri (§24 madde 1) | — | | |
| devir:12-177 Gövde anması: kabul kriterleri ve kontrol listesi 11'de tanımlı olmadan doğrulama başlamaz | — | | |
| devir:D-01 SETUP §2 ve §4 kurulumda tamamlanmadı (DEFERRED_BACKLOG) | T02, T03, T04, T05, T06, T07, T09 | | |

### 7.2 Geri izlenebilirlik (task → kaynak öğe)

Her task → hangi kaynak öğeden besleniyor?
**Kaynağı olmayan task = gerekçesiz iş / sessiz kapsam genişlemesi.**

| Task | Beslendiği kaynak öğe(ler) | Durum |
|---|---|---|
| T01 | `07` UC-81; `05` MK-67, §9.4; `09` §10 | ✓ 4 kaynak (özgün 2, alt bölüm 2, devir 0) |
| T02 | `05` MK-01, MK-02, MK-03, MK-04, MK-05, §3.1, §3.8; `09` §3, §4; `devir` K-1356, D-01 | ✓ 11 kaynak (özgün 5, alt bölüm 4, devir 2) |
| T03 | `05` §8.3; `09` §3, §6, §19, §20, §23; `devir` K-1355, P8.4, D-01 | ✓ gerekçeli (alt bölüm + devir) — K-1432, K-1434 |
| T04 | `05` MK-12, MK-70, MK-74, §8.3, §8.7; `09` §3, §10, §19, §21, §23; `devir` K-1356, K-1358, P8.2, P8.3, D-01 | ✓ 15 kaynak (özgün 3, alt bölüm 7, devir 5) |
| T05 | `05` MK-06, MK-07, MK-08, MK-10, MK-14, MK-58, MK-68, MK-76, §3.2, §6.3, §6.8, §8.1, §8.9; `09` §4; `devir` D-01 | ✓ 15 kaynak (özgün 8, alt bölüm 6, devir 1) |
| T06 | `05` MK-02, MK-06, MK-09, MK-10, MK-11, MK-69, §3.2, §8.2; `09` §4, §17, §21; `devir` D-01 | ✓ 12 kaynak (özgün 6, alt bölüm 5, devir 1) |
| T07 | `05` MK-09, MK-25, MK-26, MK-27, §6.8, §7.6, §9.2, §9.4; `09` §17; `10` KP-75; `devir` D-01 | ✓ 11 kaynak (özgün 5, alt bölüm 5, devir 1) |
| T08 | `05` MK-59, MK-60, §6; `09` §17 | ✓ 4 kaynak (özgün 2, alt bölüm 2, devir 0) |
| T09 | `05` MK-68, §8.1; `09` §21; `10` §4.1; `devir` K-1358, P8.3, D-01 | ✓ 7 kaynak (özgün 1, alt bölüm 3, devir 3) |
| T10 | `05` §7.1; `09` §10, §17 | ✓ gerekçeli (alt bölüm) — K-1441 |
| T101 | `06` §2.1, §2.2, §2.3, §2.4, §2.5, §2.6, §2.8, §2.9, §2.10, §5.1; `02` §4.1.1; `05` MK-03, MK-06, MK-19, MK-74, §7.1; `09` §5, §9, §10, §12 | ✓ 20 kaynak (özgün 5, alt bölüm 15, devir 0) |
| T102 | `06` §5.2, §5.3, §5.4, §5.5, §5.6; `09` §12 | ✓ gerekçeli (alt bölüm) — K-1413, K-1414 |
| T103 | `06` §4.1 Company, §4.2 CompanySettings, §4.2 CompanyDeliveryProvince, §4.3 Province, §4.4 Country, §4.56 LegalTextVersion; `02` §3.1.1, §3.14.3 | ✓ 8 kaynak (özgün 8, alt bölüm 0, devir 0) |
| T104 | `06` §4.5 Product, §4.6 Variant, §4.7 Option, §4.8 OptionValue, §4.9 Category, §4.10 ProductCategory, §4.69 Image, §4.71 ProductImage | ✓ 8 kaynak (özgün 8, alt bölüm 0, devir 0) |
| T105 | `06` §4.11 PriceHistory, §4.12 Discount, §4.13 Coupon, §4.70 DigitalFile | ✓ 4 kaynak (özgün 4, alt bölüm 0, devir 0) |
| T106 | `06` §4.37 Admin, §4.38 AdminInvitation, §4.63 AuditLog; `02` §1.3, Z-3, Z-30, §6.9, §8.1.7, §8.5.1, §8.5.2, §10.3, §10.3.2, §11.2, P-14, P-45; `05` MK-24, §4.7, §7.5; `09` §9 | ✓ 19 kaynak (özgün 12, alt bölüm 7, devir 0) |
| T107 | `06` §4.36 Customer, §4.40 PendingEmailChange, §4.45 AddressBook; `02` §1.3 | ✓ 4 kaynak (özgün 3, alt bölüm 1, devir 0) |
| T108 | `06` §4.39 Session, §4.41 RecognizedBrowserMark, §4.42 LinkRecord, §4.43 AttemptRecord, §4.44 LoginRecord; `02` §3.13.20, §11.2, P-45; `05` MK-48, MK-50 | ✓ 10 kaynak (özgün 9, alt bölüm 1, devir 0) |
| T109 | `06` §4.15 Cart, §4.16 CartItem, §4.17 Order, §4.18 OrderItem, §4.19 TransitionRecord, §4.20 InternalNote, §4.21 CheckboxAcceptance, §4.21 CheckboxAcceptanceOrderItem | ✓ 8 kaynak (özgün 8, alt bölüm 0, devir 0) |
| T110 | `06` §4.14 StockReservation, §4.61 OrderEmailHistory, §4.62 OrderAddressHistory | ✓ 3 kaynak (özgün 3, alt bölüm 0, devir 0) |
| T111 | `06` §4.22 Cancellation, §4.23 ItemRemoval, §4.24 LateDeliveryTermination, §4.25 Withdrawal, §4.26 Return, §4.28 NotReturnedClosure, §4.30 DefectClaim, §4.31 RefundIban | ✓ 8 kaynak (özgün 8, alt bölüm 0, devir 0) |
| T112 | `06` §4.32 PaymentRecord, §4.33 Refund, §4.34 RefundRecord, §4.35 InboundNotificationRecord | ✓ 4 kaynak (özgün 4, alt bölüm 0, devir 0) |
| T113 | `06` §4.27 ReturnRejection, §4.29 RefundIbanRequest | ✓ 2 kaynak (özgün 2, alt bölüm 0, devir 0) |
| T114 | `06` §4.46 AboutPage, §4.47 Offering, §4.48 PortfolioItem, §4.49 FaqItem, §4.51 CustomPage, §4.52 Announcement, §4.54 Branding, §4.55 SocialLink | ✓ 8 kaynak (özgün 8, alt bölüm 0, devir 0) |
| T115 | `06` §4.50 Branch, §4.53 OfferingProduct, §4.53 PortfolioItemProduct, §4.57 ContactRequest | ✓ 4 kaynak (özgün 4, alt bölüm 0, devir 0) |
| T116 | `06` §4.71 AboutPageImage, §4.71 OfferingImage, §4.71 PortfolioItemImage, §4.71 FaqItemImage, §4.71 CustomPageImage | ✓ 5 kaynak (özgün 5, alt bölüm 0, devir 0) |
| T117 | `06` §4.58 DispatchRecord, §4.60 ChannelStateRecord; `05` MK-38, §4.4 | ✓ 4 kaynak (özgün 3, alt bölüm 1, devir 0) |
| T118 | `06` §4.59 Job, §4.64 DisposalRecord, §4.65 HeartbeatRecord, §4.66 OutageRecord, §4.67 BackupStatusRecord, §4.68 DataProtectionKey; `02` §3.34, §3.34.8; `09` §10; `10` KP-75 | ✓ 10 kaynak (özgün 8, alt bölüm 2, devir 0) |
| T119 | `06` §8.1, §8.4; `09` §10 | ✓ gerekçeli (alt bölüm) — K-1413, K-1414 |
| T120 | `06` §2.7, §6.1, §6.2, §6.3, §6.4, §6.5, §6.6, §7.1, §7.2, §7.3, §7.4, §7.5, §7.6; `09` §5, §19 | ✓ gerekçeli (alt bölüm) — K-1414, K-1416 |
| T201 | `06` §9.1, §9.4, §9.5; `03` §4.1; `02` §4.2, Z-36, Z-44, §8.4.2, §8.4.5, §8.5.1, §8.5.2, §9.1, §9.1.6, §10.3, §10.3.3, §10.3.5, §11.2, P-39, P-45, §12.2, §12.2.7; `05` MK-22, MK-23, MK-65, §5.2, §7.4, §7.5, §9.2; `09` §9; `10` KP-74, KD-56 | ✓ 31 kaynak (özgün 17, alt bölüm 14, devir 0) |
| T202 | `08` §3.1; `02` §8.3.4; `05` MK-17, MK-61, §3.6, §10.1; `09` §4, §11, §19; `10` KD-64 | ✓ 10 kaynak (özgün 4, alt bölüm 6, devir 0) |
| T203 | `08` §3.2; `05` MK-17, MK-49, §3.6; `09` §11, §12 | ✓ 6 kaynak (özgün 2, alt bölüm 4, devir 0) |
| T204 | `08` §3.3; `03` §10.1; `05` §10.1 | ✓ gerekçeli (alt bölüm) — K-1414, K-1419 |
| T205 | `06` §8.2; `09` §8 | ✓ gerekçeli (alt bölüm) — K-1414 |
| T206 | `07` §2.1, §2.2, §2.10; `05` MK-04, MK-16, MK-18, §3.5, §3.7; `09` §5; `10` KD-20 | ✓ 10 kaynak (özgün 4, alt bölüm 6, devir 0) |
| T207 | `07` §2.3; `02` §1.3; `05` MK-16, MK-45, MK-47, §3.5, §6.1, §6.2, §6.5; `09` §15; `10` KD-18 | ✓ 11 kaynak (özgün 4, alt bölüm 7, devir 0) |
| T208 | `07` §2.4; `02` §3.31.1; `05` MK-20, §7.3; `09` §10; `10` KP-76 | ✓ 6 kaynak (özgün 3, alt bölüm 3, devir 0) |
| T209 | `07` §2.5; `02` §8.3.2, §8.3.3, §8.4.4, §12.2, §12.2.5, §12.2.8; `05` MK-52, MK-53, MK-58, §6.5, §6.8; `09` §15; `10` KD-47, KD-57 | ✓ 15 kaynak (özgün 10, alt bölüm 5, devir 0) |
| T210 | `07` §2.6; `09` §10, §20; `10` KP-3, KD-32 | ✓ 5 kaynak (özgün 2, alt bölüm 3, devir 0) |
| T211 | `07` §5.1; `09` §3, §7 | ✓ gerekçeli (alt bölüm) — K-1416 |
| T212 | `07` §2.7, §6.1, §6.4; `03` §3.1; `05` MK-63, §10.3; `09` §8 | ✓ 7 kaynak (özgün 1, alt bölüm 6, devir 0) |
| T213 | `07` §6.2; `09` §5, §8 | ✓ gerekçeli (alt bölüm) — K-1416 |
| T214 | `07` §6.3; `09` §8 | ✓ gerekçeli (alt bölüm) — K-1416 |
| T215 | `07` §2.8; `03` §6.1; `02` §3.13.17, §3.17.8, §3.22.3, §3.32.7, §8.3.1, §11.2, P-31, P-32, P-33, P-34, P-35, P-37, P-46; `05` MK-48, MK-49, §6.3; `09` §15; `10` KP-72, KD-59 | ✓ 21 kaynak (özgün 16, alt bölüm 5, devir 0) |
| T216 | `07` §2.9; `04` §8.1, §8.2; `02` §4.1.1, §12.1, §12.1.1; `09` §7; `10` KD-20 | ✓ 8 kaynak (özgün 3, alt bölüm 5, devir 0) |
| T217 | `07` §8.1; `03` §1.11 | ✓ gerekçeli (alt bölüm) — K-1416, K-1419 |
| T218 | `05` MK-13, MK-15, §3.1, §3.3, §3.4; `09` §6, §9; `10` KD-21 | ✓ 8 kaynak (özgün 3, alt bölüm 5, devir 0) |
| T219 | `05` MK-18, §3.7; `09` §9, §15; `10` KP-37, KD-48 | ✓ 6 kaynak (özgün 3, alt bölüm 3, devir 0) |
| T220 | `05` MK-37, MK-38, MK-42, §4.3, §4.4, §5.1, §5.2; `09` §12, §13, §20; `10` KP-68 | ✓ 11 kaynak (özgün 4, alt bölüm 7, devir 0) |
| T221 | `05` MK-41, MK-42, MK-43, MK-44, §5.1, §5.2, §5.3, §5.4; `09` §13 | ✓ 9 kaynak (özgün 4, alt bölüm 5, devir 0) |
| T222 | `05` MK-50, MK-51, §6.4, §6; `09` §15 | ✓ 5 kaynak (özgün 2, alt bölüm 3, devir 0) |
| T223 | `05` MK-28, MK-29, MK-54, §6.6, §7.7; `09` §6, §15, §20; `10` KP-42 | ✓ 9 kaynak (özgün 4, alt bölüm 5, devir 0) |
| T224 | `05` MK-55, MK-56, MK-57, §6.7; `09` §5, §15; `10` KP-73, KD-15, KD-19, KD-41, KD-55 | ✓ 11 kaynak (özgün 8, alt bölüm 3, devir 0) |
| T225 | `05` MK-64, MK-65, §9.1, §9.2; `09` §18; `10` KP-74, KD-58 | ✓ 7 kaynak (özgün 4, alt bölüm 3, devir 0) |
| T226 | `05` MK-71, §8.4, §11.1; `09` §3; `10` KP-64, §4.2 | ✓ 6 kaynak (özgün 2, alt bölüm 4, devir 0) |
| T227 | `05` MK-19, §7.2; `09` §7, §14 | ✓ 4 kaynak (özgün 1, alt bölüm 3, devir 0) |
| T228 | `05` MK-20, MK-21, §7.1, §7.3; `09` §9, §10; `10` KP-76 | ✓ 7 kaynak (özgün 3, alt bölüm 4, devir 0) |
| T229 | `05` MK-34, §7.10; `09` §10, §20 | ✓ 4 kaynak (özgün 1, alt bölüm 3, devir 0) |
| T230 | `05` §9.1; `09` §18; `devir` K-1354, K-1391, P8.5 | ✓ gerekçeli (alt bölüm + devir) — K-1435 |
| T231 | `05` §9.1; `09` §18; `devir` K-1354, K-1391, P8.5 | ✓ gerekçeli (alt bölüm + devir) — K-1435 |
| T232 | `05` §7.6; `09` §15 | ✓ gerekçeli (alt bölüm) — K-1441 |
| T233 | `09` §10 | ✓ gerekçeli (alt bölüm) — K-1441 |
| T301 | `06` §9.2; `03` §4.1; `02` §4.2, Z-28, Z-41, Z-43, §8.4, §8.4.3, §8.4.5, §10.4, §10.4.7, §12.2, §12.2.3, §12.2.7; `05` MK-23, §7.4; `09` §9; `10` KP-74 | ✓ 18 kaynak (özgün 10, alt bölüm 8, devir 0) |
| T302 | `06` §9.3; `03` §4.1; `02` §4.2, Z-29, Z-30, Z-33, Z-34, Z-43, Z-35, Z-40, Z-45, Z-46, §8.4, §8.4.5, §8.5, §8.5.1, §8.5.2, §10.7, §10.7.1, §11.2, P-47, P-48, P-40, P-41, P-45, §12.2, §12.2.3, §12.2.7; `05` MK-23, §5.2, §7.4, §7.7; `10` KP-74 | ✓ 33 kaynak (özgün 22, alt bölüm 11, devir 0) |
| T303 | `06` §8.3; `03` §10.4; `02` §10.2, §10.2.1, §10.2.7; `05` MK-72, §6.9, §8.5; `09` §13; `10` §4.1 | ✓ 10 kaynak (özgün 3, alt bölüm 7, devir 0) |
| T304 | `08` §2.2.4; `02` §9.1, §9.1.6 | ✓ 3 kaynak (özgün 1, alt bölüm 2, devir 0) |
| T305 | `08` §2.2.5; `02` Z-26, Z-34, Z-43, §6.1, §6.1.1, §6.7, §9.1, §9.1.1, §9.1.2, §9.1.3, §9.1.4, §9.2, B-1, §9.5, §11.2, P-38, P-41, §12.1, §12.1.7, §12.1.11; `05` MK-37, MK-38, §4.4; `09` §11; `10` KP-66, KP-68, KD-16, KD-17 | ✓ 29 kaynak (özgün 19, alt bölüm 10, devir 0) |
| T306 | `08` §2.2.6; `03` §7.1; `02` §3.33.6, Z-9, Z-47, §6.1, §6.1.1, §6.3, §6.7, §7.1, §7.1.4, §7.2, §7.2.4, §7.3, §7.3.5, §9.2, B-1, B-2, B-3, B-4, B-5, B-6, B-7, B-8, §12.1, §12.1.7; `10` KP-66 | ✓ 27 kaynak (özgün 17, alt bölüm 10, devir 0) |
| T307 | `08` §2.2.6; `03` §7.1; `02` §3.21.6, §6.1, §6.1.1, §6.3, §7.1, §7.1.4, §7.2, §7.2.4, §7.3, §7.3.5, §7.4, §7.4.9, §8.3, §8.3.6, §8.3.7, §8.3.8, §9.2, B-9, B-10, B-11, B-12, B-13, B-14, B-15, B-16, §10.4, §10.4.2, §10.4.4, §10.4.6, §10.4.10, §10.4.11; `10` KP-66 | ✓ 34 kaynak (özgün 23, alt bölüm 11, devir 0) |
| T308 | `08` §2.2.7; `03` §7.1; `02` §3.32.5, §9.3, F-1, F-2, F-3, F-4, F-5, F-6, §9.3.1, §9.3.2, §9.3.3, §9.3.4, §9.5, §10.1, §10.1.5, §10.2, §10.2.2, §10.2.6; `10` KP-34, KP-67 | ✓ 22 kaynak (özgün 16, alt bölüm 6, devir 0) |
| T309 | `08` §2.2.8; `03` §7.1; `02` §3.15.1, §6.5, §9.4, §10.2, §10.2.1; `10` KP-66, KP-67, KD-52 | ✓ 10 kaynak (özgün 5, alt bölüm 5, devir 0) |
| T310 | `08` §2.4; `02` §3.20.7, §3.20.8; `10` KP-19, KD-2 | ✓ 5 kaynak (özgün 5, alt bölüm 0, devir 0) |
| T311 | `07` UC-80; `08` §2.1.5; `03` §2.5; `02` Z-7, §5.6, §5.6.3, §6.1, §6.1.2, §9.2, B-4, §11.2, P-18; `05` MK-39, §4.5; `09` §12; `10` KP-15 | ✓ 16 kaynak (özgün 8, alt bölüm 8, devir 0) |
| T312 | `07` UC-37, UC-83, §5.4; `03` §8.1; `02` §3.12.1, §3.12.3, §3.12.5, §3.12.7, §11.1, P-8, §11.2, P-26; `05` MK-30, MK-31, §7.7; `09` §15; `10` KP-20, KP-42, KP-44 | ✓ 19 kaynak (özgün 13, alt bölüm 6, devir 0) |
| T313 | `07` UC-79; `02` §3.30.5, §3.30.6; `05` §3.7; `10` KP-37 | ✓ 5 kaynak (özgün 4, alt bölüm 1, devir 0) |
| T314 | `07` UC-82; `05` MK-62, MK-67, §9.4, §10.2; `09` §18; `10` KP-50, KD-60 | ✓ 8 kaynak (özgün 5, alt bölüm 3, devir 0) |
| T315 | `03` §1.1, §1.10; `02` §5.3, §5.3.1, §5.4, §5.5; `05` MK-35, §4.1; `09` §9 | ✓ 9 kaynak (özgün 2, alt bölüm 7, devir 0) |
| T316 | `03` §1.2, §2.5, §8.2; `02` §3.20.10, §3.20.12, §3.21.10, §5.3, §5.3.2, §5.4, §5.6, §5.6.1, §5.6.2, §6.4, §6.7, §7.2, §7.2.1, §7.2.2, §7.6, §7.6.3, §9.2, B-6, §10.4, §10.4.6; `10` KP-46, KD-24 | ✓ 25 kaynak (özgün 13, alt bölüm 12, devir 0) |
| T317 | `03` §1.3, §2.5; `02` §3.21.8, §5.3, §5.3.2, §5.5, §5.6, §5.6.1, §5.6.3, §5.6.5, §6.7, §7.6, §7.6.2, §10.4, §10.4.6; `10` KP-18, KD-62 | ✓ 17 kaynak (özgün 9, alt bölüm 8, devir 0) |
| T318 | `03` §1.4, §1.5, §8.2, §8.5; `02` §5.3, §5.3.1, §5.6, §5.6.1, §5.6.2, §5.6.3, §5.6.5, §5.6.6, §5.7, §7.3, §7.3.6, §10.5, §10.5.1, §10.5.2, §10.5.4; `10` KP-46, KP-51, §4.2 | ✓ 22 kaynak (özgün 12, alt bölüm 10, devir 0) |
| T319 | `03` §1.6; `02` §5.4, §5.5, §5.9, §6.7, §7.6, §7.6.1, §7.6.3, §10.4, §10.4.3, §10.4.6, §10.4.8; `05` MK-35, §4.1; `10` KP-46 | ✓ 15 kaynak (özgün 7, alt bölüm 8, devir 0) |
| T320 | `03` §1.7, §1.9, §5.3; `02` §5.4, §5.8, §5.10, §7.5, §7.5.3; `10` KP-23 | ✓ 9 kaynak (özgün 2, alt bölüm 7, devir 0) |
| T321 | `03` §1.8; `02` §3.7.1, §5.1; `10` KP-7, KP-40 | ✓ 5 kaynak (özgün 3, alt bölüm 2, devir 0) |
| T322 | `03` §1.8, §1.9; `02` §3.27.23, §3.32.4, §5.2, §5.13; `10` KD-38 | ✓ 7 kaynak (özgün 3, alt bölüm 4, devir 0) |
| T323 | `03` §1.11; `02` §10.1, §10.1.2; `05` MK-47, §6.2 | ✓ 5 kaynak (özgün 2, alt bölüm 3, devir 0) |
| T324 | `03` §4.1; `02` §3.17.4, §3.17.5, §4.1, §4.1.3, §4.2, Z-1, Z-2, Z-3, Z-4, Z-5, Z-6, Z-7, Z-8, Z-9, §5.6, §5.6.3, §6.1, §6.1.2, §10.2, §10.2.6, §11.1, P-7, §11.2, P-12, P-13, P-14, P-15, P-16, P-18 | ✓ 30 kaynak (özgün 22, alt bölüm 8, devir 0) |
| T325 | `03` §4.1; `02` §3.20.6, §4.1, §4.1.3, §4.2, Z-10, Z-11, Z-12, Z-39, Z-13, Z-14, Z-15, Z-16, Z-42, Z-17, Z-47, Z-18, Z-19, §7.2, §7.2.3, §7.3, §7.3.1, §7.4, §7.4.1, §7.5, §7.5.1, §11.1, P-5, P-44 | ✓ 29 kaynak (özgün 21, alt bölüm 8, devir 0) |
| T326 | `03` §4.1; `02` §4.2, Z-20, Z-21, Z-22, Z-23, Z-24, Z-26, Z-27, §11.2, P-38 | ✓ 11 kaynak (özgün 8, alt bölüm 3, devir 0) |
| T327 | `03` §4.2, §10.3; `02` §3.34, §3.34.7, §3.34.8, Z-8, Z-38, §4.3, §11.1, P-7, §11.2, P-43, §12.3, §12.3.3; `05` MK-44, §5.4; `10` KP-75 | ✓ 17 kaynak (özgün 9, alt bölüm 8, devir 0) |
| T328 | `03` §7.1, §7.2, §7.3; `02` §9.1, §9.1.1, §9.1.2, §9.1.3, §9.5, §12.1, §12.1.11; `10` KP-66, KP-67, KD-10, KD-16, KD-17, KD-52 | ✓ 16 kaynak (özgün 10, alt bölüm 6, devir 0) |
| T329 | `03` §2.10; `02` §2.1, §2.2, §3.17.1; `09` §19 | ✓ 5 kaynak (özgün 1, alt bölüm 4, devir 0) |
| T330 | `02` §3.1, §3.1.1, §3.1.2, §3.1.3, §3.1.4, §3.1.7, §6.9, §7.3, §7.3.5, §7.4, §7.4.4, §9.1, §9.1.4, §9.3, §9.3.1, §10.1, §10.1.1, §12.1, §12.1.1, §12.1.4, §12.5; `10` KP-35, KP-58, KD-21, §4.1, §4.2 | ✓ 26 kaynak (özgün 15, alt bölüm 11, devir 0) |
| T331 | `03` §10.2; `02` §3.1, §3.1.5, §3.1.6, §3.13.19, §3.32.8, §3.33.9, §6.1, §6.1.4, §6.2, §6.9, §10.8, §10.8.2, §12.2, §12.2.5; `10` KP-25, KP-38, KP-59, KP-60, KD-60, §4.1 | ✓ 21 kaynak (özgün 13, alt bölüm 8, devir 0) |
| T332 | `02` §3.2, §3.2.1, §3.2.2, §3.2.3, §3.3, §3.3.1, §3.3.2, §3.3.3, §3.3.4, §3.3.5, §3.3.6, Z-39, §7.3, §7.3.4, §11.1, P-44, §11.2, P-21; `05` MK-32, §7.8; `10` KP-4, KP-5, KP-6, KP-39, KD-28, KD-35, KD-37, KD-61, §4.2 | ✓ 29 kaynak (özgün 22, alt bölüm 7, devir 0) |
| T333 | `02` §3.4, §3.4.1, §3.4.2, §3.4.3, §3.4.4, §3.4.5, §3.4.6, §3.5, §3.5.1, §3.5.2, §3.5.3, §3.28.6, §6.6, §11.2, P-20; `05` MK-33, §7.9; `09` §9, §10; `10` KP-1, KP-2, KP-3, KP-41, KD-30, KD-31, KD-32, KD-37, §4.2 | ✓ 28 kaynak (özgün 20, alt bölüm 8, devir 0) |
| T334 | `02` §3.6, §3.6.1, §3.6.2, §3.6.3, §3.6.4, §3.6.5, §3.6.6, §3.6.7, §6.2, §6.6, §7.2, §7.2.6, §7.4, §7.4.7, §7.4.8, §10.4, §10.4.3, §10.4.6; `10` KP-4, KP-16, KP-39, KD-9, KD-33 | ✓ 23 kaynak (özgün 17, alt bölüm 6, devir 0) |
| T335 | `02` §3.7, §3.7.1, §3.7.2, §3.7.3, §3.7.4, §3.7.5, §3.7.6, §3.7.7, §3.7.8, Z-24, §5.1, §5.2, §6.6, §11.1, P-44; `10` KP-7, KP-39, KP-40, KD-28, KD-38 | ✓ 20 kaynak (özgün 15, alt bölüm 5, devir 0) |
| T336 | `02` §3.8, §3.8.1, §3.8.2, §3.8.3, §3.8.4, §3.8.5, §6.6, §11.1, P-10, §12.1, §12.1.5; `09` §14; `10` KP-5, KP-39 | ✓ 14 kaynak (özgün 9, alt bölüm 5, devir 0) |
| T337 | `02` §3.9, §3.9.1, §3.9.2, §3.9.3, §3.9.4, §3.9.5, §3.9.6, §4.1, §4.1.4, Z-20, Z-21, §5.11, §5.11.1, §6.6, §12.1, §12.1.6; `09` §14; `10` KP-5, KP-43 | ✓ 19 kaynak (özgün 13, alt bölüm 6, devir 0) |
| T338 | `02` §3.10, §3.10.1, §3.10.2, §3.10.3, §3.10.4, §3.10.5, §3.10.6, §3.10.7, §4.1, §4.1.4, Z-22, §5.11, §5.11.2, §5.11.3, §5.11.4, §6.2, §6.6, §7.2, §7.2.6, §7.4, §7.4.8, §8.2, L-6, §11.2, P-36; `09` §14; `10` KP-11, KP-16, KP-43 | ✓ 29 kaynak (özgün 19, alt bölüm 10, devir 0) |
| T339 | `02` §3.11, §3.11.1, §3.11.2, §3.11.3, §3.11.4, §3.11.5, §3.11.6, §3.11.7, §3.27.16, §11.2, P-22, P-23, P-24, P-27; `05` MK-29, MK-54, §6.5; `09` §15; `10` KP-5, KP-42, KD-63 | ✓ 21 kaynak (özgün 17, alt bölüm 4, devir 0) |
| T340 | `03` §8.1; `02` §3.7.4, §3.12, §3.12.1, §3.12.2, §3.12.3, §3.12.4, §3.12.5, §3.12.6, §3.12.7, §3.12.8, §3.12.9, Z-19, §6.2, §6.6, §6.7, §7.2, §7.2.5, §7.5, §7.5.4, §9.2, B-4, §11.1, P-8, P-44, §11.2, P-26; `05` MK-30, MK-31, §7.7; `10` KP-9, KP-20, KP-44 | ✓ 33 kaynak (özgün 22, alt bölüm 11, devir 0) |
| T341 | `02` §3.13, §3.13.1, §3.13.2, §3.13.3, §3.13.4, §3.13.5, §3.13.18, §3.13.19, §3.13.20, §3.13.21, Z-1, Z-40, §5.12, §5.12.1, §6.5, §8.1, §8.1.1, §8.1.2, §8.1.3, §8.1.4, §8.2, L-3, L-9, §8.4, §8.4.1, §8.5, §8.5.1, §9.4, §10.2, §10.2.5, §10.3, §10.3.3, §10.4, §10.4.11, §10.7, §10.7.4, §11.2, P-12, P-33, P-49, P-45, §12.2, §12.2.9, §12.2.12; `10` KP-8, KP-25, KP-28 | ✓ 47 kaynak (özgün 33, alt bölüm 14, devir 0) |
| T342 | `02` §3.13, §3.13.9, §3.13.10, §3.13.11, §3.13.12, §3.13.13, §3.13.15, §3.13.16, §3.13.17, Z-2, Z-4, Z-5, §5.12, §5.12.2, §6.5, §8.1, §8.1.1, §8.1.3, §8.1.4, §8.1.5, §8.1.6, §8.1.7, §8.2, L-1, L-2, §8.2.4, §8.2.6, §9.4, §10.2, §10.2.4, §10.2.5, §11.2, P-13, P-15, P-16, P-48, P-17, P-31, P-32, P-33, §12.2, §12.2.5; `05` MK-45, MK-46, MK-51, §6.1; `09` §15; `10` KP-27 | ✓ 48 kaynak (özgün 37, alt bölüm 11, devir 0) |
| T343 | `02` §3.13, §3.13.6, §3.13.7, §3.13.8, §3.13.14, Z-2, Z-45, §5.12, §5.12.2, §6.5, §8.1, §8.1.2, §8.1.3, §8.1.5, §8.2, L-9, §9.4, §10.2, §10.2.4, §10.2.5, §11.2, P-13, P-47, P-49; `05` MK-46, §6.1; `10` KP-26, KP-27, KD-11 | ✓ 29 kaynak (özgün 20, alt bölüm 9, devir 0) |
| T344 | `02` §3.14, §3.14.1, §3.14.2, §3.14.3, §3.14.4, §3.14.5, §3.14.6; `10` KP-12, KP-13, KP-27, KD-27 | ✓ 11 kaynak (özgün 10, alt bölüm 1, devir 0) |
| T345 | `02` §3.15, §3.15.1, §3.15.2, §3.15.3, §3.15.4, §3.15.5, §3.15.6, Z-31, Z-32, §5.3, §5.3.5, §5.9, §6.5, §7.6, §7.6.5, §9.4, §10.3, §10.3.1, §12.2, §12.2.6, §12.2.7; `05` §6.1; `10` KP-29, KP-30, KP-77, KD-36 | ✓ 26 kaynak (özgün 17, alt bölüm 9, devir 0) |
| T346 | `02` §3.16, §3.16.1, §3.16.2, §3.16.3, §3.16.4, §3.16.5, §3.16.6, §3.16.7, Z-6, §6.1, §6.1.4, §6.2, §12.2, §12.2.5; `10` KP-9, KP-10, KD-10 | ✓ 17 kaynak (özgün 13, alt bölüm 4, devir 0) |
| T347 | `02` §3.16, §3.16.8, §3.16.9, §3.16.10, §3.16.11, §3.16.12, §3.16.13, §6.2, §11.1, P-9; `10` KP-9, KD-7, KD-35, KD-59 | ✓ 14 kaynak (özgün 11, alt bölüm 3, devir 0) |
| T348 | `02` §3.10.6, §3.16.12, §3.17, §3.17.1, §3.17.2, §3.17.3, §3.17.4, §3.17.5, §3.17.6, §3.17.7, §3.17.8, Z-7, Z-8, §4.3, §5.3, §5.3.2, §5.3.3, §5.3.4, §5.6, §5.6.3, §5.6.4, §6.1, §6.1.2, §6.1.3, §6.2, §6.3, §8.2, L-7, L-8, §8.2.3, §9.2, B-1, §9.3, F-1, §10.6, §10.6.3, §11.1, P-7, §11.2, P-18, P-37, P-46, §12.1, §12.1.7; `05` MK-21, MK-36, MK-39, §4.2, §4.5; `09` §9; `10` KP-8, KP-11, KP-14, KP-15, KP-16, KP-17 | ✓ 56 kaynak (özgün 39, alt bölüm 17, devir 0) |
| T349 | `02` §3.10.7, §3.18, §3.18.1, §3.18.2, §3.18.3, §3.19, §3.19.1, §3.19.2, §3.19.3, §3.19.4, §3.19.5, §3.19.6, §6.2, §7.2, §7.2.9, §7.4, §7.4.6, §11.1, P-1, P-3, P-4; `09` §14; `10` KP-13, KP-59, KD-26 | ✓ 25 kaynak (özgün 18, alt bölüm 7, devir 0) |
| T350 | `02` §3.20, §3.20.1, §3.20.2, §3.20.3, §3.20.4, §3.20.5, §3.20.6, §3.20.7, §3.20.8, §4.1, §4.1.2, §4.1.5, Z-10, Z-11, §5.7, §6.2, §6.4, §6.6, §6.9, §7.2, §7.2.3, §9.2, B-5, §10.4, §10.4.4, §10.6, §10.6.3, §11.1, P-5, P-6, §12.1, §12.1.8; `10` KP-13, KP-19, KP-46, KP-59, KD-2, KD-24, KD-25 | ✓ 39 kaynak (özgün 26, alt bölüm 13, devir 0) |
| T351 | `02` §3.20, §3.20.9, §3.20.10, §3.20.11, §3.20.12, Z-12, Z-13, §5.8, §6.4, §6.7, §7.2, §7.2.5, §7.3, §7.3.1, §7.3.3, §10.4, §10.4.5; `10` KP-6 | ✓ 18 kaynak (özgün 11, alt bölüm 7, devir 0) |
| T352 | `02` §3.21, §3.21.1, §3.21.2, §3.21.3, §3.21.4, §3.21.9, §3.21.10, §3.21.11, Z-7, §6.1, §6.1.2, §6.1.4, §6.2, §6.9, §8.3, §8.3.3, §8.3.4, §11.2, P-18, §12.1, §12.1.3; `10` KP-15, KD-4, KD-23, KD-62, KD-64 | ✓ 26 kaynak (özgün 19, alt bölüm 7, devir 0) |
| T353 | `02` §3.21, §3.21.5, §3.21.6, §3.21.7, §3.21.8, §4.1, §4.1.2, §4.1.5, Z-8, Z-9, §6.2, §6.7, §6.9, §7.2, §7.2.8, §7.6, §7.6.2, §9.2, B-2, B-3, B-13, §9.3, F-5, §9.3.3, §10.4, §10.4.6, §10.6, §10.6.1, §11.1, P-7, §11.2, P-46; `10` KP-15, KP-46, KP-59, KD-3 | ✓ 36 kaynak (özgün 23, alt bölüm 13, devir 0) |
| T354 | `02` §3.22, §3.22.1, §3.22.2, §3.22.3, §3.22.4, §3.22.5, §6.1, §6.1.1, §6.2, §6.3, §8.2, L-5, §8.3, §8.3.5, §8.3.6, §10.4, §10.4.11, §11.2, P-19, P-35; `09` §15; `10` KP-17, KP-18 | ✓ 23 kaynak (özgün 14, alt bölüm 9, devir 0) |
| T355 | `02` §3.8.3, §3.9.6, §3.14.5, §3.19.6, §3.23, §3.23.1, §3.23.2, §3.23.3, §3.23.4, §3.23.5, §3.24.6, Z-39, §5.3, §5.3.2, §5.11, §5.11.4, §10.4, §10.4.2, §11.1, P-8, §12.1, §12.1.6, §12.1.7; `09` §9; `10` KP-17 | ✓ 25 kaynak (özgün 18, alt bölüm 7, devir 0) |
| T356 | `02` §3.24, §3.24.1, §3.24.2, §3.24.3, §3.24.4, §3.24.5, §3.24.6, §3.24.7, §3.24.8, §6.1, §6.1.3, §6.2, §7.3, §7.3.2, §7.3.3, §7.3.4, §7.4, §7.4.2, §7.4.4, §8.4, §8.4.1, §8.4.3, §12.1, §12.1.2, §12.1.7, §12.2, §12.2.2, §12.2.4, §12.2.12; `05` MK-56; `09` §9; `10` KP-6, KP-8, KP-14, KP-35, KD-29; `devir` 04-1948 | ✓ 37 kaynak (özgün 27, alt bölüm 9, devir 1) |
| T357 | `02` §3.25, §3.25.1, §3.25.2, §3.25.3, §10.5, §10.5.3, §10.7, §10.7.1, §12.1, §12.1.10, §12.5; `10` KP-52, KD-1 | ✓ 13 kaynak (özgün 8, alt bölüm 5, devir 0) |
| T358 | `02` §2.3, §3.27, §3.27.1, §3.27.2, §3.27.3, §3.27.4, §3.27.5, §3.27.6, §3.27.7, §3.27.8, §3.27.9, §3.27.12, §6.8, §11.2, P-28, P-29; `10` KP-32, KP-33, KP-36, KP-54, KD-14, KD-39, KD-41, KD-42, KD-43, KD-44, KD-45, KD-63 | ✓ 28 kaynak (özgün 24, alt bölüm 4, devir 0) |
| T359 | `02` §3.27, §3.27.10, §3.27.11, §3.27.13, §3.27.14, §3.27.15, §3.27.16, §3.27.17, §3.27.18, §3.27.19, §3.27.20, §6.8, §11.2, P-22, P-23, P-25, P-27, P-28, P-29; `10` KP-32, KP-54, KD-8, KD-34, KD-44, KD-63 | ✓ 25 kaynak (özgün 22, alt bölüm 3, devir 0) |
| T360 | `03` §10.2; `02` §2.3, §3.27, §3.27.21, §3.27.22, §3.27.23, §3.27.24, §3.27.25, §3.27.26, §3.27.27, §3.27.28, §4.1, §4.1.4, Z-23, Z-24, §5.2, §6.8, §7.6, §7.6.4, §10.8, §10.8.1, §11.2, P-30; `10` KP-36, KP-54, KP-57, KD-38, KD-45 | ✓ 28 kaynak (özgün 19, alt bölüm 9, devir 0) |
| T361 | `02` §2.3, §3.28, §3.28.1, §3.28.2, §3.28.3, §3.28.4, §3.28.5, §3.28.6, §6.8; `10` KP-1, KP-31, KP-55, KD-39, KD-40 | ✓ 14 kaynak (özgün 11, alt bölüm 3, devir 0) |
| T362 | `02` §3.29, §3.29.1, §3.29.2, §3.29.3, §3.29.4, §3.29.5, §3.29.6, §6.9, §9.1, §9.1.4, §11.2, P-22, P-23; `10` KP-56, KD-46, KD-47 | ✓ 16 kaynak (özgün 12, alt bölüm 4, devir 0) |
| T363 | `02` §3.30, §3.30.1, §3.30.2, §3.30.3, §3.30.4, §3.30.5, §3.30.6, §3.30.7; `09` §9; `10` KP-7, KP-36, KP-37, KD-48, KD-49 | ✓ 14 kaynak (özgün 12, alt bölüm 2, devir 0) |
| T364 | `02` §3.31, §3.31.1, §3.31.2, §6.6, §6.7, §6.8, §10.1, §10.1.4; `10` KP-76 | ✓ 9 kaynak (özgün 4, alt bölüm 5, devir 0) |
| T365 | `02` §3.15.4, §3.32, §3.32.1, §3.32.2, §3.32.3, §3.32.4, §3.32.5, §3.32.6, §3.32.7, §3.32.8, §3.32.9, §3.32.10, Z-33, §5.13, §6.8, §7.1, §7.1.5, §8.2, L-4, §8.3, §8.3.1, §8.4, §8.4.1, §9.3, F-2, §9.5, §11.2, P-34, P-40, §12.2, §12.2.6, §12.2.12; `10` KP-30, KP-34, KP-49, KD-42, KD-50, KD-51, KD-57 | ✓ 39 kaynak (özgün 28, alt bölüm 11, devir 0) |
| T366 | `02` §3.24.3, §3.33, §3.33.1, §3.33.2, §3.33.3, §3.33.4, §3.33.5, §3.33.6, §3.33.7, §3.33.8, §3.33.9, §6.5, §6.9, §7.1, §7.1.5, §7.4, §7.4.4, §10.8, §10.8.2, §12.1, §12.1.2, §12.1.7, §12.2, §12.2.1, §12.2.2, §12.2.3, §12.2.4, §12.2.5, §12.2.8, §12.5; `09` §11; `10` KP-14, KP-25, KP-35, KP-61, KP-73, KD-53, KD-55, KD-56; `devir` 04-1948 | ✓ 40 kaynak (özgün 29, alt bölüm 10, devir 1) |
| T367 | `02` Z-39, §5.8, §5.9, §5.11, §5.11.3, §6.4, §7.1, §7.1.1, §7.1.3, §7.1.4, §7.1.6, §7.2, §7.2.1, §7.2.2, §7.2.5, §7.2.6, §7.2.7, §7.3, §7.3.7, §9.2, B-7, §10.4, §10.4.3; `10` KP-21, KP-24 | ✓ 25 kaynak (özgün 16, alt bölüm 9, devir 0) |
| T368 | `03` §4.1; `02` Z-11, Z-39, Z-47, §5.8, §5.9, §6.4, §6.7, §7.2, §7.2.3, §7.2.4, §8.3, §8.3.8, §9.2, B-7, B-9, §10.4, §10.4.8, §10.4.10, §12.1, §12.1.8; `10` KP-21, KP-47 | ✓ 23 kaynak (özgün 13, alt bölüm 10, devir 0) |
| T369 | `02` §6.4, §7.1, §7.1.3, §7.1.6, §7.1.7, §7.2, §7.2.7, §7.2.8, §7.2.9, §7.4, §7.4.6; `09` §14; `10` KP-21, KP-22, KP-24 | ✓ 15 kaynak (özgün 10, alt bölüm 5, devir 0) |
| T370 | `03` §1.7; `02` Z-16, Z-17, Z-47, Z-35, Z-38, §4.3, §5.5, §6.4, §7.2, §7.2.8, §7.4, §7.4.1, §7.4.5, §7.5, §7.5.3, §8.4, §8.4.2, §9.2, B-8, B-14, §9.3, F-4, §10.4, §10.4.3, §10.4.9, §10.4.10, §10.6, §10.6.1, §11.2, P-43, §12.1, §12.1.8, §12.1.9, §12.2, §12.2.7, §12.5; `05` MK-40, §4.6; `09` §14; `10` KP-15, KP-21, KP-22, KP-47 | ✓ 44 kaynak (özgün 26, alt bölüm 18, devir 0) |
| T371 | `02` Z-13, Z-14, Z-15, §6.4, §7.1, §7.1.1, §7.1.2, §7.3, §7.3.1, §7.3.2, §7.3.3, §7.3.4, §7.4, §7.4.9, §10.4, §10.4.5, §12.1, §12.1.9; `10` KP-22, KP-45 | ✓ 20 kaynak (özgün 14, alt bölüm 6, devir 0) |
| T372 | `02` §5.6, §5.6.6, §5.8, §5.9, §6.4, §7.1, §7.1.4, §7.1.6, §7.3, §7.3.5, §7.3.6, §7.3.7, §7.6, §7.6.7, §8.3, §8.3.8, §9.2, B-9, B-14, §9.3, F-3, §10.4, §10.4.10, §12.1, §12.1.9, §12.5; `10` KP-22, KP-47 | ✓ 28 kaynak (özgün 15, alt bölüm 13, devir 0) |
| T373 | `02` Z-16, Z-42, Z-38, §5.8, §6.4, §7.4, §7.4.1, §7.4.2, §7.4.3, §7.4.4, §7.4.6, §7.4.7, §7.4.8, §7.4.9, §7.4.10, §8.4, §8.4.2, §9.2, B-14, B-16, §10.4, §10.4.5, §10.4.9, §10.6, §10.6.1, §11.2, P-43, §12.1, §12.1.9, §12.5; `10` KP-22, KP-24, KP-47; `devir` 04-1948 | ✓ 34 kaynak (özgün 23, alt bölüm 10, devir 1) |
| T374 | `02` Z-18, Z-29, §5.8, §5.10, §6.4, §7.1, §7.1.1, §7.1.4, §7.4, §7.4.3, §7.5, §7.5.1, §7.5.2, §7.5.3, §7.5.4, §9.2, B-9, §9.3, F-3, §12.1, §12.1.9; `10` KP-23, KP-49, KD-42, KD-61 | ✓ 25 kaynak (özgün 16, alt bölüm 9, devir 0) |
| T375 | `02` §6.1, §6.1.1, §6.3, §6.7, §7.6, §7.6.6, §7.6.7, §8.1, §8.1.2, §8.3, §8.3.6, §8.3.7, §9.2, B-1, B-3, B-10, B-11, B-12, B-13, B-15, §10.1, §10.1.2, §10.3, §10.3.1, §10.4, §10.4.1, §10.4.2, §10.4.11, §12.2, §12.2.7, §12.5; `05` §4.4; `10` KP-17, KP-28, KP-48, KD-13 | ✓ 36 kaynak (özgün 23, alt bölüm 13, devir 0) |
| T376 | `03` §6.1; `02` §5.3, §5.3.4, §5.12, §5.12.3, §6.1, §6.1.5, §6.2, §6.3, §6.5, §8.1, §8.1.4, §8.2, L-1, L-2, L-3, L-4, L-5, L-6, L-7, L-8, L-9, §8.2.1, §11.2, P-31, P-32, P-33, P-34, P-35, P-36, P-37, P-46, P-49; `09` §15; `10` KP-72 | ✓ 35 kaynak (özgün 24, alt bölüm 11, devir 0) |
| T377 | `03` §6.1; `02` Z-27, Z-46, §6.1, §6.1.5, §8.2, §8.2.2, §8.2.3, §8.2.4, §8.2.5, §8.2.6, §8.3, §8.3.5, §11.2, P-48, §12.2, §12.2.5; `10` KP-72 | ✓ 18 kaynak (özgün 12, alt bölüm 6, devir 0) |
| T378 | `02` §4.1, §4.1.1, §4.1.2, §4.1.3, §4.1.4, §4.1.5; `09` §7 | ✓ 7 kaynak (özgün 5, alt bölüm 2, devir 0) |
| T379 | `02` §9.3, F-6, §9.3.4, §9.4, §10.1, §10.1.2, §10.2, §10.2.1, §10.2.2, §10.2.3, §10.2.4, §10.2.5, §10.2.6, §10.2.7, §10.3, §10.3.1, §12.2, §12.2.10; `05` §6.1; `10` KP-62, KD-18 | ✓ 21 kaynak (özgün 14, alt bölüm 7, devir 0) |
| T380 | `02` §10.2, §10.2.2, §10.2.4, §10.2.6, §10.3, §10.3.1, §10.3.2, §10.3.3, §10.3.4, §10.3.5, §10.4, §10.4.1, §12.2, §12.2.9; `05` MK-24, §4.7, §7.5; `09` §9; `10` KP-63, KP-77, KD-58 | ✓ 21 kaynak (özgün 14, alt bölüm 7, devir 0) |
| T381 | `02` §10.1, §10.1.3, §10.3, §10.3.1, §11.1, P-1, P-3, P-4, P-5, P-6, P-7, P-8, P-9, P-10, P-44, §11.2, P-12, P-13, P-14, P-47, P-15, P-16, P-48, P-17, P-18, P-19, P-20, P-21, P-22, P-23, P-24, P-25, P-26, P-27, P-28, P-29, P-30, P-31, P-32, P-33, P-34, P-35, P-36, P-37, P-46, P-49, P-38, P-39, P-40, P-41, P-43, P-45, P-42; `09` §3; `10` KP-64, §4.2 | ✓ 56 kaynak (özgün 50, alt bölüm 6, devir 0) |
| T382 | `03` §10.1; `02` §9.1, §9.1.6, §9.3, §9.3.1, §9.3.2, §10.4, §10.4.9, §10.6, §10.6.1, §10.8, §10.8.2; `05` MK-62, §10.2; `10` KP-50, KP-65, §4.1 | ✓ 17 kaynak (özgün 9, alt bölüm 8, devir 0) |
| T383 | `02` §10.4, §10.4.9, §10.6, §10.6.2, §10.6.3, §11.3, H-2; `10` KP-53 | ✓ 8 kaynak (özgün 5, alt bölüm 3, devir 0) |
| T384 | `03` §8.9; `02` §10.3, §10.3.1, §10.7, §10.7.1, §10.7.2, §10.7.3, §10.7.4, §12.1, §12.1.10, §12.2, §12.2.9, §12.3, §12.3.2, §12.5; `09` §10; `10` KP-52, KD-1, KD-54 | ✓ 19 kaynak (özgün 11, alt bölüm 8, devir 0) |
| T385 | `03` §1.7, §7.1, §8.9, §10.1; `02` §9.1, §9.1.6, §9.3, §9.3.3, §9.3.4, §10.6, §10.6.1, §11.2, P-38; `05` §4.4, §5.2, §10.2; `10` KP-68 | ✓ 17 kaynak (özgün 6, alt bölüm 11, devir 0) |
| T386 | `02` §10.4, §10.4.1, §10.4.3, §10.4.4, §10.4.5, §10.4.7; `09` §10; `10` KP-8, KP-48 | ✓ 9 kaynak (özgün 7, alt bölüm 2, devir 0) |
| T387 | `03` §1.7; `05` MK-66, MK-72, §6.9, §8.5, §9.3; `09` §13 | ✓ 7 kaynak (özgün 2, alt bölüm 5, devir 0) |
| T388 | `05` MK-72, §8.5, §8.7; `09` §13 | ✓ 4 kaynak (özgün 1, alt bölüm 3, devir 0) |
| T389 | `05` §6.9, §8.5; `09` §13 | ✓ gerekçeli (alt bölüm) — K-1441 |
| T390 | `03` §2.10 | ✓ gerekçeli (alt bölüm) — K-1441 |
| T401 | `08` §2.1, §2.1.1, §2.1.2, §2.1.4; `03` §2.5; `02` §3.21.1, §3.21.4, §12.1, §12.1.3, §12.2, §12.2.8; `05` §3.6, §6.5; `09` §11, §14; `10` KP-15, KD-64, §4.1 | ✓ 18 kaynak (özgün 7, alt bölüm 11, devir 0) |
| T402 | `08` §2.1.5; `09` §14 | ✓ gerekçeli (alt bölüm) — K-1415 |
| T403 | `08` §2.1.6, §2.1.7; `03` §2.5, §3.2, §10.1; `05` MK-39, §4.5; `09` §14 | ✓ 8 kaynak (özgün 1, alt bölüm 7, devir 0) |
| T404 | `08` §2.1.8; `03` §3.2, §10.1; `05` MK-40, §3.6, §4.6 | ✓ 6 kaynak (özgün 1, alt bölüm 5, devir 0) |
| T405 | `08` §2.1.9; `03` §2.7, §2.8, §3.2, §3.3, §8.3, §10.1; `05` MK-40, §4.6; `09` §14 | ✓ 10 kaynak (özgün 1, alt bölüm 9, devir 0) |
| T406 | `08` §2.1.3, §2.1.10; `02` §3.21.3 | ✓ 3 kaynak (özgün 1, alt bölüm 2, devir 0) |
| T407 | `08` §2.2, §2.2.1, §2.2.2; `03` §10.1; `02` §9.1, §9.1.5, §9.1.6; `09` §11; `10` KP-66, §4.1 | ✓ 10 kaynak (özgün 4, alt bölüm 6, devir 0) |
| T408 | `08` §2.2.3; `05` §5.2 | ✓ gerekçeli (alt bölüm) — K-1414 |
| T409 | `08` §2.3; `03` §9.1, §10.1; `02` §3.13.6, §3.13.7, §11.2, P-13, §12.2, §12.2.8; `05` MK-46, §6.1; `09` §11; `10` KP-26, KD-11, §4.1 | ✓ 15 kaynak (özgün 8, alt bölüm 7, devir 0) |
| T410 | `08` §4.1, §5.2 | ✓ gerekçeli (alt bölüm) — K-1413, K-1414 |
| T501 | `08` §4.3; `02` §3.8.5, §12.1, §12.1.5; `09` §11; `10` KP-5 | ✓ 6 kaynak (özgün 3, alt bölüm 3, devir 0) |
| T502 | `07` UC-01, UC-02; `04` E-01, E-02; `03` §2.1; `02` §3.4.2, §3.4.3, §3.5.2, §3.5.3, §3.9.2, §3.27.14, §3.28.1, §3.28.2, §3.28.3; `10` KP-1, KP-31, KD-40 | ✓ 17 kaynak (özgün 16, alt bölüm 1, devir 0) |
| T503 | `07` UC-03, UC-04, UC-05; `04` E-03, E-04; `03` §2.1, §8.1; `02` §3.1.5, §3.2.1, §3.3.1, §3.3.3, §3.3.6, §3.5.1, §3.5.2, §3.5.3, §3.6.3, §3.6.4, §3.6.5, §3.7.5, §3.7.6, §3.8.5, §3.9.2, §3.16.8, §3.16.9, §3.20.4, §3.30.7, §11.1, P-5, P-9, §12.1, §12.1.5; `10` KP-2, KP-3, KP-4, KP-5, KP-6, KP-7, KD-6, KD-8, KD-9, KD-33, KD-34 | ✓ 42 kaynak (özgün 38, alt bölüm 4, devir 0) |
| T504 | `07` UC-06, UC-78; `04` E-06; `02` §3.30.4; `10` KP-7 | ✓ 5 kaynak (özgün 5, alt bölüm 0, devir 0) |
| T505 | `07` UC-07, UC-08, UC-09, UC-10, UC-11, UC-12; `04` E-07, E-08; `03` §2.2; `02` §3.27.4, §3.27.5, §3.27.10, §3.27.25, §3.30.7; `10` KP-32 | ✓ 15 kaynak (özgün 14, alt bölüm 1, devir 0) |
| T506 | `07` UC-13, UC-14, UC-15; `04` E-09, E-10; `03` §2.2, §5.1, §5.2; `02` §3.1.4, §3.27.6, §3.27.7, §3.27.8, §3.32.1, §3.32.2, §3.32.3, §11.2, P-28; `10` KP-30, KP-33, KP-34, KD-15 | ✓ 21 kaynak (özgün 17, alt bölüm 4, devir 0) |
| T507 | `07` UC-16, UC-17, UC-18, UC-102, UC-173, UC-174; `04` E-11, E-47; `03` §8.7; `02` §3.24.8, §3.33.1, §3.33.2, §12.2, §12.2.2; `10` KP-61, KD-53 | ✓ 16 kaynak (özgün 14, alt bölüm 2, devir 0) |
| T508 | `07` UC-19, UC-20, UC-21; `04` E-12; `03` §2.3, §3.2; `02` §3.1.6, §3.12.4, §3.12.9, §3.16.1, §3.16.3, §3.16.4, §3.16.5, §3.16.6, §3.16.7, §3.16.8, §3.16.10, §3.18.1, §3.19.5, §11.1, P-3; `10` KP-9, KP-10, KP-11, KP-38 | ✓ 25 kaynak (özgün 22, alt bölüm 3, devir 0) |
| T509 | `07` UC-22, UC-23, UC-24, UC-25, UC-26, UC-27; `04` E-13; `03` §2.4, §3.2, §6.1, §10.1; `02` §2.2, §3.1.5, §3.10.1, §3.13.1, §3.13.3, §3.14.1, §3.14.2, §3.14.4, §3.14.6, §3.17.1, §3.17.2, §3.18.1, §3.19.5, §3.20.1, §3.24.1, §3.24.2, §3.24.5, §11.1, P-3; `10` KP-8, KP-12, KP-13, KP-14, KP-15, KD-5 | ✓ 36 kaynak (özgün 30, alt bölüm 6, devir 0) |
| T510 | `07` UC-28, UC-29, UC-30; `08` §2.1.4; `04` E-14; `03` §2.4, §2.5, §3.2, §10.1; `02` §2.2, §3.17.1, §3.17.6; `10` KP-15, KD-23 | ✓ 14 kaynak (özgün 8, alt bölüm 6, devir 0) |
| T511 | `07` UC-31, UC-32, UC-33, UC-34, UC-36; `04` E-15, E-16; `03` §2.6, §3.2, §5.1; `02` §2.1, §3.12.1, §3.12.5, §3.17.8, §3.22.3, §3.22.4, §3.22.5, §6.1.1, §7.1.4, §9.1, §9.1.2, §11.1, P-8, §11.2, P-35, P-37, P-46; `10` KP-18, KP-19, KP-20 | ✓ 30 kaynak (özgün 23, alt bölüm 7, devir 0) |
| T512 | `07` UC-35, UC-38, UC-39, UC-40, UC-41; `04` E-16, E-17; `03` §2.7, §3.3, §5.1, §5.2, §5.3; `02` §2.1; `10` KP-21 | ✓ 14 kaynak (özgün 8, alt bölüm 6, devir 0) |
| T513 | `07` UC-42, UC-43, UC-44, UC-45, UC-46; `04` E-18, E-19; `03` §2.8, §2.9, §3.3, §5.1; `02` §2.1, §3.1.7, §7.5.2; `10` KP-22, KP-23 | ✓ 16 kaynak (özgün 11, alt bölüm 5, devir 0) |
| T514 | `07` UC-47, UC-48, UC-49, UC-52, UC-53; `04` E-20, E-21; `03` §3.4, §9.1; `02` §2.1, §3.13.4, §3.13.5, §3.13.11, §3.13.18, §11.2, P-12, P-49; `10` KP-25 | ✓ 18 kaynak (özgün 14, alt bölüm 4, devir 0) |
| T515 | `07` UC-50, UC-51, UC-54, UC-55; `04` E-22; `03` §3.4, §6.1, §9.1, §9.2; `02` §3.13.6, §3.13.12, §11.2, P-15, P-16; `10` KP-26, KP-27 | ✓ 16 kaynak (özgün 11, alt bölüm 5, devir 0) |
| T516 | `07` UC-56, UC-57, UC-58, UC-59, UC-60, UC-61, UC-62; `04` E-23, E-24; `03` §3.4, §6.1, §9.2; `02` §3.13.8, §3.13.9, §3.13.10, §3.13.11, §11.2, P-13; `10` KP-27 | ✓ 19 kaynak (özgün 15, alt bölüm 4, devir 0) |
| T517 | `07` UC-63, UC-64, UC-65, UC-66, UC-67, UC-68, UC-69; `04` E-25; `03` §3.4, §9.3, §9.4; `02` §3.13.10, §3.13.13, §3.13.14, §3.13.15, §3.13.21, §3.15.1, §3.15.6, §11.2, P-47, P-17; `10` KP-27, KP-29, KD-36 | ✓ 24 kaynak (özgün 20, alt bölüm 4, devir 0) |
| T518 | `07` UC-70, UC-71, UC-72, UC-73, UC-74; `04` E-26, E-27; `03` §2.6, §9.3; `02` §3.14.1, §3.14.6 | ✓ 11 kaynak (özgün 9, alt bölüm 2, devir 0) |
| T519 | `07` UC-75, UC-76, UC-77; `04` E-28; `03` §3.4, §6.2, §9.2, §9.3; `02` §3.13.14, §11.2, P-47; `10` KP-27 | ✓ 12 kaynak (özgün 7, alt bölüm 5, devir 0) |
| T520 | `07` UC-84, UC-85, UC-109, UC-110, UC-111, UC-112, UC-113; `04` E-29, E-30; `03` §3.5, §8.8, §9.2, §10.4; `02` §3.13.16, Z-3, §6.9, §8.1.3, §8.1.7, §10.2, §10.2.1, §11.2, P-14; `10` KP-62 | ✓ 23 kaynak (özgün 16, alt bölüm 7, devir 0) |
| T521 | `07` UC-86, UC-108, UC-185, UC-186, UC-187; `04` E-31, E-54; `03` §6.3, §8.7, §8.9, §10.1; `10` KP-50, KP-65 | ✓ 13 kaynak (özgün 9, alt bölüm 4, devir 0) |
| T522 | `07` UC-103, UC-106, UC-175, UC-176, UC-177; `04` E-49, E-52; `03` §3.5, §6.2, §6.3, §8.8, §8.9; `02` §10.1, §10.1.2, §10.3, §10.3.4; `10` KP-62, KP-63 | ✓ 18 kaynak (özgün 11, alt bölüm 7, devir 0) |
| T523 | `07` UC-104, UC-178, UC-179, UC-180, UC-181; `04` E-50; `03` §9.3; `02` §3.13.16; `10` KP-62 | ✓ 9 kaynak (özgün 8, alt bölüm 1, devir 0) |
| T524 | `07` UC-105, UC-107, UC-182, UC-183, UC-184; `04` E-51, E-53; `03` §6.3, §8.9; `02` §3.25.1, §3.25.3; `10` KP-52, KP-53, KD-54 | ✓ 14 kaynak (özgün 12, alt bölüm 2, devir 0) |
| T525 | `07` UC-87, UC-88, UC-114, UC-115, UC-116, UC-117; `04` E-32, E-33; `03` §3.5, §8.1; `02` §2.1, §3.2.2, §3.3.2, §3.3.4, §3.4.4, §3.6.1, §3.6.2, §3.6.7, §3.7.1, §3.7.3, §3.7.4, §3.7.7, §3.7.8, §3.8.1, §3.8.2, §3.8.4, §3.8.5, §3.9.1, §3.9.5, §3.11.1, §3.11.2, §3.11.3, §3.11.5, §3.12.2, §3.16.9, §3.20.5, §10.7, §10.7.2, §11.1, P-5, P-6, P-9, P-10, P-44, §11.2, P-21, P-22, P-23, P-24, P-27; `09` §16; `10` KP-39, KP-40, KP-42, KP-43, KP-44, KP-45, KD-12 | ✓ 58 kaynak (özgün 51, alt bölüm 7, devir 0) |
| T526 | `07` UC-89, UC-90, UC-118, UC-119, UC-120, UC-121, UC-122; `04` E-34, E-35; `03` §3.5, §8.1; `02` §3.4.1, §3.4.5, §3.4.6, §3.10.2, §3.10.4, §3.10.5, §3.10.6, §11.2, P-20, P-36; `10` KP-41, KP-43 | ✓ 23 kaynak (özgün 20, alt bölüm 3, devir 0) |
| T527 | `07` UC-91, UC-92, UC-123, UC-124, UC-125, UC-126, UC-127, UC-128; `04` E-36, E-37; `03` §2.8, §3.5, §4.1, §6.2, §6.3, §8.2; `02` §2.1, §3.20.7, §3.20.11, §3.31.2; `10` KP-46, KD-13 | ✓ 22 kaynak (özgün 15, alt bölüm 7, devir 0) |
| T528 | `07` UC-129, UC-130, UC-138, UC-140, UC-141, UC-142, UC-144, UC-145; `04` E-37; `03` §3.5, §5.2, §5.3, §8.3, §8.4; `02` §3.6.6; `10` KP-47 | ✓ 16 kaynak (özgün 11, alt bölüm 5, devir 0) |
| T529 | `07` UC-131, UC-132, UC-133, UC-134, UC-135, UC-136, UC-137; `04` E-37; `03` §3.5, §5.2, §5.3, §8.3; `02` §3.21.11; `10` KP-47 | ✓ 14 kaynak (özgün 10, alt bölüm 4, devir 0) |
| T530 | `07` UC-146, UC-147, UC-148, UC-149, UC-150, UC-151; `04` E-37; `03` §2.8, §3.5, §5.2, §6.2, §8.4; `02` §3.23.3; `10` KP-48 | ✓ 14 kaynak (özgün 9, alt bölüm 5, devir 0) |
| T531 | `07` UC-139, UC-143, UC-152, UC-153, UC-154; `04` E-37; `03` §3.5, §8.2, §8.4, §8.9; `02` §3.12.6, §3.20.9, §3.32.10, §6.1.1, §8.4.3, §9.1, §9.1.6; `05` MK-31; `10` KP-44, KP-48 | ✓ 20 kaynak (özgün 15, alt bölüm 5, devir 0) |
| T532 | `07` UC-93, UC-94, UC-155, UC-156; `04` E-38, E-39; `03` §8.6; `02` §3.32.4, §3.32.5; `10` KP-49, KD-50, KD-51 | ✓ 12 kaynak (özgün 11, alt bölüm 1, devir 0) |
| T533 | `07` UC-95, UC-157; `04` E-40; `03` §3.4, §9.4; `02` §3.15.5, §3.15.6; `10` KP-29, KP-77 | ✓ 9 kaynak (özgün 7, alt bölüm 2, devir 0) |
| T534 | `07` UC-96, UC-98, UC-117, UC-158, UC-159, UC-160, UC-161, UC-163; `04` E-41, E-43; `03` §3.5, §8.6, §10.2; `02` §2.1, §2.3, §3.11.6, §3.27.3, §3.27.9, §3.27.11, §3.27.12, §3.27.13, §3.27.15, §3.27.21, §3.27.24, §3.27.27, §3.28.3, §3.29.1, §3.29.3, §3.29.6, §11.2, P-22, P-23, P-25, P-27, P-28, P-29, P-30; `10` KP-54, KP-56, KP-57 | ✓ 40 kaynak (özgün 34, alt bölüm 6, devir 0) |
| T535 | `07` UC-97, UC-99, UC-162, UC-164, UC-165, UC-166; `04` E-42, E-44; `03` §3.5, §8.6, §8.7, §10.2; `02` §3.1.2, §3.1.3, §3.1.5, §3.1.6, §3.1.7, §3.28.1, §3.28.4, §12.1, §12.1.4; `10` KP-31, KP-55, KP-58, KP-60, KP-64 | ✓ 26 kaynak (özgün 21, alt bölüm 5, devir 0) |
| T536 | `07` UC-100, UC-101, UC-167, UC-168, UC-169, UC-170, UC-171, UC-172; `04` E-45, E-46; `03` §3.5, §6.2, §6.3, §8.7; `02` §2.1, §3.17.4, §3.18.1, §3.19.1, §3.19.2, §3.19.3, §3.20.1, §3.20.4, §3.20.5, §3.21.2, §3.21.5, §4.1.5, §11.1, P-1, P-3, P-5, P-6, P-7, §11.2, P-18; `10` KP-59, KP-64 | ✓ 36 kaynak (özgün 29, alt bölüm 7, devir 0) |
| T537 | `07` §5.5; `04` OB-02, §2.2; `03` §2.2; `02` §3.1.4, §3.28.4, §3.28.5, §3.29.2; `10` KP-35, KP-36 | ✓ 10 kaynak (özgün 7, alt bölüm 3, devir 0) |
| T538 | `07` §5.2, §5.3, §5.4; `09` §12, §19 | ✓ gerekçeli (alt bölüm) — K-1416, K-1418 |
| T539 | `07` §8.2; `05` MK-05, §3.8; `09` §16; `10` KP-70, KD-22 | ✓ 6 kaynak (özgün 3, alt bölüm 3, devir 0) |
| T540 | `04` OB-01, OB-02, OB-09, §2.1, §2.2, §2.9, §3.1, §3.2, §8.3; `03` §8.1; `02` §3.33.5, §3.34, §3.34.1, §3.34.2, §3.34.3, §3.34.4, §3.34.5, §3.34.6, §6.1.6, §8.3.2, §8.3.3, §8.4.1, §8.4.4, §10.1, §10.1.4, §11.2, P-42, §12.4, §12.4.1; `05` MK-05, §3.8; `09` §16; `10` KP-69, KP-70, KP-71, KD-22, KD-46 | ✓ 37 kaynak (özgün 24, alt bölüm 13, devir 0) |
| T541 | `04` OB-03, OB-04, OB-07, OB-08, §2.3, §2.4, §2.7, §2.8; `03` §3.1; `02` §6.1.5, §6.1.6; `10` KP-38, KP-69 | ✓ 13 kaynak (özgün 8, alt bölüm 5, devir 0) |
| T542 | `04` OB-05, OB-06, OB-10, §2.5, §2.6, §2.10; `10` KP-76 | ✓ 7 kaynak (özgün 4, alt bölüm 3, devir 0) |
| T543 | `04` OB-12, OB-13, OB-14, OB-15, §2.12, §7.3; `02` §3.34.5, §7.4.5, §11.2, P-43, §12.4, §12.4.1; `09` §16; `10` KP-12, KP-71 | ✓ 15 kaynak (özgün 10, alt bölüm 5, devir 0) |
| T544 | `04` OB-16, OB-17, OB-18, OB-19, §2.12; `02` §3.24.1, §3.24.2, §12.4, §12.4.1; `09` §16; `10` KP-71 | ✓ 11 kaynak (özgün 8, alt bölüm 3, devir 0) |
| T545 | `04` OB-11, OB-20, §2.11, §2.13 | ✓ 4 kaynak (özgün 2, alt bölüm 2, devir 0) |
| T546 | `04` §3.3, §3.4, §3.5, §3.6, §3.7; `09` §19 | ✓ gerekçeli (alt bölüm) — K-1418 |
| T547 | `04` §6.1, §6.2, §6.3, §7.1, §7.2; `02` §10.1, §10.1.2; `09` §19 | ✓ 8 kaynak (özgün 1, alt bölüm 7, devir 0) |
| T548 | `09` §16; `devir` K-1354, K-1391, P8.5 | ✓ gerekçeli (alt bölüm + devir) — K-1435 |
| T549 | `04` E-05 | ✓ 1 kaynak (özgün 1, alt bölüm 0, devir 0) |
| T601 | `06` §8.5; `08` §4.2; `02` §9.1, §9.1.5, §10.1, §10.1.3, §10.2, §10.2.7, §12.2, §12.2.1, §12.2.8, §12.3, §12.3.1, §12.3.2, §12.3.3, §12.5; `05` MK-07, MK-25, MK-27, MK-75, MK-76, §6.3, §6.9, §7.6, §8.8, §8.9; `10` §4.1 | ✓ 27 kaynak (özgün 13, alt bölüm 14, devir 0) |
| T602 | `05` MK-73, MK-74, §8.6, §8.7 | ✓ 4 kaynak (özgün 2, alt bölüm 2, devir 0) |

> **Neden geri yön de gerekli:** İleri yön yalnız **eksik** task'ı yakalar (S3). Planın kendisine sızmış, hiçbir gereksinime dayanmayan işi ise yalnız geri yön yakalar — ve o iş bir kez plana girdiğinde faz boyunca "planda var" gerekçesiyle sorgulanmadan yapılır.

> **Gate check kullanımı:** "Implement edildi" kolonu her faz sonunda kontrol edilir; eşlenip implement edilmeyen öğe **S3 Eksik** bulgusudur. Kaynağı olmayan task gate'te **kapsam bulgusu** olarak raporlanır.

## 8. Boşluk raporu

> **Kaynak:** matrisin O6 2/2 oturumu — `03`, `05`, `09` gövdelerinin tam okunması ve beş kümenin örneklemi (§7.1, "Okuma derinliği örneklemi ve boşluk taraması"). Karar: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §3 `GAP-15`…`GAP-21`, K-1441. Her boşluk Opus doğrulayıcıyla tek tek sınandı; üst dokümana birim yazan karar çıkmadı (boşluk planın kendi eksiğidir, `02`–`10` değişmez).

| # | Boşluk | Durum | Kapatan task |
|---|---|---|---|
| GAP-15 | Yönetim komutları `end-sessions`, `invalidate-password`, `status` ile komut çatısı (`CommandHost`, dağıtıcı, terminalden okuma, komut kaydı) hiçbir task'ta yoktu (`05 §8.5`, `09 §13.2`) | Kapandı — çatı T303'e, üç komut yeni T389'a (K-1441) | T303, T389 |
| GAP-16 | Geliştirmede `IEmailGateway` gerçeklemesi `SmtpCaptureEmailGateway` (Mailpit'in SMTP kapısı) arayüzün doğduğu fazda sahipsizdi (`09 §4.3`, §19.1) | Kapandı — T202'nin kapsamına (K-1441) | T202 |
| GAP-17 | Veri koruma halkası: tek kurulum, `DataProtectionPurposes` (6 amaç), istisnasız çözme, çapraz amaç testi, kaynak taramaları (`09 §15.5`, `05 §7.6.3`) | Kapandı — yeni T232 (K-1441) | T232 |
| GAP-18 | Açılışta, iş kuyruğundan önce bekleyen geri alma bildiriminin bağlantı özetinin o anki sırla yeniden yazımı (`05 §6.x`, K-1137) | Kapandı — T222'nin kapsamına (K-1441) | T222 |
| GAP-19 | Veritabanı oturum süreleri: `DatabaseTimeouts`, Web ve Maintenance bağlantı profilleri, aşımın beklenmeyen hâle düşmesi (`09 §10.7`) | Kapandı — yeni T233 (K-1441) | T233 |
| GAP-20 | Veritabanı kullanıcıları: kurucu betik, beş kullanıcı, `pg_hba`, rol ayarları, `pg_trgm`, yetki matrisi testi (`09 §10.8`, `05 §7.1.3`) | Kapandı — yeni T10 (K-1441) | T10 |
| GAP-21 | `03 §2.10.2`–§2.10.5 anlatılarının (karışık sipariş, kargoda cayma, IBAN'sız iade, e-posta düzeltme) servis düzeyi uçtan uca testi — T329 yalnız ana akışı sınar | Kapandı — yeni T390 (K-1441) | T390 |

**Boşluk olmayanlar (doğrulayıcı reddetti, K-1440).** Dependabot ve derleme adımında açık taraması (T04'ün `05 §8.3` kaynağında); girdi doğrulama kalıbı (T211, T212, T213; yalnız T211'in kısa adı "doğrulama" demiyor); işletim sistemi güncellemesi, sıralama sürümü denetimi, kurtarma sonrası eksik dosya listesi ve kapasite okuma komutu (`DEPLOY_RUNBOOK` işi, provası T601); demo sitesi (T09, T602). Bunlar §7.1 satırlarında ya da K-1440'ta gerekçelidir.

**Açık boşluk: yok.** Matris tamamdır (ileri yön 1.501/1.501, 7.2 eşit, 214 aday task); task listesine (`§5`) geçiş kapısı açıktır.

## 9. Post-MVP parkı

> **Ne yazılır:** Plan yazılırken ortaya çıkan ama MVP'ye girmeyen işler. Ertelenen işlerden farklıdır — bunlar hiç başlanmamış, bilinçli olarak kapsam dışı bırakılmış kalemlerdir.

---

*Shopfolio — Implementation Plan v0.3*
