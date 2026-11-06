# Shopfolio — Status Changelog

> [`IMPLEMENTATION_STATUS.md`](IMPLEMENTATION_STATUS.md)'nin **tarihsel arşivi**.
>
> **Neden ayrı dosya:** Güncel snapshot kısa kalmalı, tarihsel detay büyüyebilir (00 §G.2).
> Status dosyasının "son güncelleme" bloğu bir sonraki güncellemeye yer açarken **eski giriş buraya taşınır** — status dosyasında birikmez.
>
> **Sıra:** En yeni giriş en üstte.

---

## YYYY-AA-GG — <özet başlık>

`<Taşınan status girişi buraya olduğu gibi yazılır.>`

---

## 2026-11-06 — T212 doğrulama tur 1 PASS

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
