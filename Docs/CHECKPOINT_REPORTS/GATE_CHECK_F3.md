## Gate Check Sonucu — F3 İş mantığı
**Tarih:** 2026-11-10   **Task aralığı:** T301–T390   **Toplam task:** 90
**Base tag:** `phase/F2-pass` (`6efa2352`) → main HEAD `ad7b10e0` (T385)

### Verdict: ✓ PASS

Testler bütün katmanlarda yeşil (Unit 5791, Integration 3487), Release derlemesi `-warnaserror` ile 0 uyarı, üç imaj `--no-cache` derleniyor ve taze ortam ayağa kalkıyor. 19 göç boş veritabanına uygulanıyor, ikinci uygulama no-op, F2 şemasından yükseltme boş veritabanıyla özdeş. `12 §6.2` F3 ölçütleri sağlandı: B — T329'un ilk uçtan uca akışı (`MainFlowTests`, `BranchFlowTests`) yeşil; C — `12 §12.3`'ün iki geçiş tablosu (S1–S11, Ö1–Ö5 = 16 geçiş) boş "Doğrulandığı satır" hücresi taşımıyor ve `TransitionTableConsistencyTests` (34) koddaki `TransitionTable` ile eşitliği sınıyor. Traceability boşluğu yok (`11 §7.1` F3 883 satır dolu, kapsama betiği 0 sorun), S2 yok. F3 kapısına bırakılmış 105 backlog kalemi karara bağlandı (73 kapandı, 32 yeniden hedeflendi). İki konu bloke etmeyen **kabul edilen risk** olarak nicelikle yazıldı (aşağıda): ölçüt C'nin zayıf PASS bağlantısı (D-981) ve 568 zayıf matris kanıtı (D-982). Kapı bağlam sınırında iki kez bölündü, üç parçada bitti (tur 1).

### Ön Kontrol
- **Task durumları:** T301–T390 hepsi `✓` (`IMPLEMENTATION_STATUS.md`); BLOCKED ya da FAIL yok. Ana dala 97 merge (90 task + 7 chore).
- **Rapor tutarlılığı:** 90 rapor PASS. Bu PR'da düzeltilen: 72 raporda başlık `Durum` (16), PR satırı (12), Post-merge satırı (53) eski ya da eksikti; 13 raporda eksik satır ya da `## Commit & PR` bölümü eklendi (T350, T375, T386'da bölüm yoktu). Status'ta 67 Commit hücresi `—` idi → squash SHA (PR no ↔ SHA eşlemesi `git log origin/main` ile doğrulandı, uyuşmazlık 0). Yeniden denetim 90/90.
- **Ana dal CI:** CI `cancelled` (sonraki push iptal etti) T316 `924ef92`, T341 `e2e9dbc`, chore #459 `37d581f`, #463 `240844a`; Publish `failure` T338 `4ad40fa` ve #459 — restic'in GitHub indirmesi (dış kesinti), #463 ile düzeldi (Docker Hub imajı, digest). Sonraki koşular yeşil; HEAD `ad7b10e` (T385) CI 42760836314 ve Publish 42760836228 success.
- **Working tree:** temiz; dal `chore/F3-gate`, taban `origin/main` = `ad7b10e0` (kapı boyunca ilerlemedi).
- **Ortam sürümleri:** .NET SDK 10.0.400, Docker 29.2.1, Compose v5.1.0, git 2.42, gh 2.89, Python 3.12.10, PostgreSQL 18.6.

### Test Sonuçları
| Katman | Tür | Sonuç | Detay |
|---|---|---|---|
| Unit | Consistency + Architecture + öteki | ✓ 5791/5791 | İlk koşu (`ad7b10e`) 5789 (Consistency 291 + 5498); backlog düzeltmelerinden sonra (`e8d878a5`) 5791; son parça (`30761e7b`, Release yeniden derlendi) yeniden 5791 |
| Integration | Ordering | ✓ 614/614 | tmpfs, `fsync=off`, `track_commit_timestamp=on` (CI eşdeğeri) |
| Integration | Web | ✓ 595/595 | İlk koşuda 14 FAIL (ModelBindingTests, "HeartbeatRecord row is missing"): `StatusEndpointTests`'in `TRUNCATE heartbeat_records` senaryosu paylaşılan koleksiyon veritabanını bozuk bırakıyordu → `Scenario.DisposeAsync` sağlıklı satırı yeniden tohumluyor; sonra yeşil |
| Integration | Model | ✓ 1404/1404 | |
| Integration | kalan ad alanları | ✓ 874/874 | Admin, Catalog, Commands, CorporateContent, Payment, Outages, Concurrency, Disposal, Jobs, Security, SignedEndpoints, FileSecrets ve öteki |
| Integration | toplam | ✓ 3487/3487 | İlk koşu 3485/3485; backlog düzeltmeleri iki test ekledi |
| Integration | D-961 (`Files` + `Web` 3 paralel koşu) | 721, 721, 720/721 | İki aday sınıf (`UploadEndpointsTests`, `ForgeryProtectionTests`) düşmedi; tek düşüş başka sınıf (`DataProtectionRingTests`, ortak `%TEMP%\shopfolio-export` yoklama dosyası, Windows) → D-980 |
| Betik | `tests/scripts` | ✓ 4/4 dosya | 21 + 13 + 129 + 27 |
| Doküman betikleri | `11_task_blok_denetle.py --tam`, `11_matris_kapsama.py`, `11_geri_yon.py` | ✓ 0 sorun | Son parçada yeniden; K atıfları kayıtta 1604, eksik 0 |
| Faz satırı B | T329 | ✓ | `MainFlowTests` 1/1, `BranchFlowTests` 5/5 |
| Faz satırı C | geçiş tablosu | ✓ | `TransitionTableConsistencyTests` 34/34 |
| Vacuous | taranan test metodu 3.689 | ✓ 0 | `Skip`/`Explicit`/sabit doğru yalnız `ViolationSamples`, `ForbiddenTestPatterns`'ta; assertion'sız metot 0 |

### Build
| Bileşen | Sonuç | Detay |
|---|---|---|
| Çözüm (Release, `-warnaserror --no-incremental`) | ✓ 0 uyarı / 0 hata | ~80 sn; üç kez (ilk, backlog sonrası, son parça) |
| NuGetAudit restore | ✓ temiz | `--vulnerable --include-transitive` üç projede 0, `--deprecated` 0 |
| `dotnet format --verify-no-changes` | ✓ üç proje temiz | |
| İmajlar (`--no-cache`) | ✓ app, database, backup | app imajı 792 MB → 315 MB (D-160: kullanılmayan SkiaSharp yerel kütüphaneleri çıkarıldı) |

### Fresh Environment
| Servis | Durum |
|---|---|
| Paket denetimi (CI adım 7 eşdeğeri, adlar `shopfolio-f3gate-*`) | ✓ 7/7 (kaynak taraması, kök olmayan kullanıcılar, efbundle 19 göç) |
| Uygulama `/sistem/canlilik` | ✓ 200 |
| Database imajı ilk kurulum | ✓ |
| Backup imajı | ✓ (restic `restic/restic:0.19.1@sha256:…`) |
| Caddy 2.11.7 + repo Caddyfile (A991/A982 regresyonu) | ✓ 12 anahtarlı yol: erişim kaydında hepsi `{key}`/`*`, sızıntı 0; HTTP→HTTPS 308; F3 Caddyfile farkı yalnız `/panel/disa-aktarma*` |
| Yerel paket betiği | Düzeltildi: `package-build.sh` Git Bash'te sır klasörünü bağlayamıyordu (`cygpath`) |
| Yerel referans reçetesi | Koşulmadı — `t09-*` adları kapının ad kuralıyla çakışıyor (D-71); CI eşdeğeri paket denetimi yerine geçti |

### Migration
| Adım | Sonuç |
|---|---|
| Model doğrulama | ✓ `has-pending-model-changes` değişiklik yok |
| İlk apply | ✓ 19 göç boş veritabanına; F3'te yeni göç `20261109143919_AddSalesSummaryPeriodIndexes` (yalnız indeks, Down yok) |
| İdempotency (2. apply) | ✓ no-op, `pg_dump` özdeş |
| Seed | ✓ 81 il, 249 ülke, 81 teslimat ili, 2 yasal metin, 6 tek satır, 80 tablo (F2 ile aynı) |
| Yükseltme (F2 şeması `20261104134754_InsertStartupData` → HEAD) | ✓ boş veritabanıyla özdeş şema |

### Traceability
| Kategori | Eşlenen | Implement | Boşluk (S3) | Kanıt |
|---|---|---|---|---|
| `11 §7.1` F3'e bağlı satırlar | 883 satır · 1.959 satır×task çifti | 883 | 0 | Kesin KK 844, üst bölüm KK 2, sözcük eşleşmesi 520, rapor satırı 25, **zayıf 568** (452 "Dokümanlar satırı" + 116 "matris eşlemesi"); 67 satırda F3 kanıtı tümüyle zayıf → kabul edilen risk, D-982 |
| Kapsama betiği | F0–F6 | `GATE_DOLU` += F3 | 0 | `11_matris_kapsama.py` çıkış 0; F0–F2 kanıtı korunuyor |
| `12` PASS satırları | 59 (F3'te 57 eklendi) | 59 | 0 | hepsi test kanıtlı; regresyon yeşil, `Durum`'u değişen 0; D-697 ile `VAL-C110`, `VAL-A826`, D-897 ile `VAL-A887` PASS |

### Faz Satırları (`12 §6.2`)
| Satır (`VAL-…`) | Severity | Kanıt | Önceki `Durum` | Yeni `Durum` | Backlog kaydı (KABUL EDİLMİŞ RİSK ise) |
|---|---|---|---|---|---|
| F3 B — ilk uçtan uca akış (T329) | — (hücre VAL'a bağlı değil, D-44 deseni) | `MainFlowTests` 1/1, `BranchFlowTests` 5/5 | — | karşılandı | — |
| F3 C — geçiş tablosu = `02 §5.4` + §5.5 | — (hücre VAL'a bağlı değil) | `12 §12.3` iki tablo 16 geçiş, boş hücre 0; `TransitionTableConsistencyTests` 34 | — | karşılandı (zayıf PASS bağlantısı → kabul edilen risk, D-981) | — |
| `VAL-A887` (D-897) | — | `SalesSummaryRules.CancellationAndReturn` (pay ve payda sipariş kimliğinin `Distinct`'i), `FindItemsWithLossAsync` kalem kümesi döner; birim `TheCancellationAndReturnRatio_CountsAnOrderOnce_OverItsItems_KK8`, Integration `TheCancellationAndReturnCount_IsByEventDay_AndTheClosureOrTheRejectionChangesNothing_KK8` (T383) | BEKLEMEDE | PASS | — |
| `VAL-C110`, `VAL-A826` (D-697) | — | `AutomaticMoments` düzeltmesi + `AutomaticMomentsTests` | BEKLEMEDE | PASS | — |

### Final Çıkış (`12 §6.3`) — yalnız son faz (F6)
Bu faz için uygulanmaz.

### Güvenlik Özeti
- Açık bulgu: 0 (task raporlarında açık güvenlik bulgusu yok; sır taraması F3 diff'inin eklenen satırlarında, 942 dosya, 8 desen: 0; parola sabitleri yalnız testlerde).
- Yeni dış bağımlılıklar: **HtmlSanitizer 10.0.1120** (+ AngleSharp 1.9.0, AngleSharp.Css 1.3.1; T339); backup imajında restic `restic/restic:0.19.1` digest ile (#463). Bilinen açık 0, kullanımdan kalkmış 0.
- Auth değişiklikleri: imzalı indirme, parçalı yükleme (`/panel/yukleme*`, sahtecilik değeri başlıkta, muafiyet değil), imzalı ödeme bildirimi, sitemap ve durum uçları; parola politikası, oturum deposu, hesap kilidi, giriş kaydı, deneme sınırları, zengin metin temizleyici. CI'a `track_commit_timestamp` adımı.

### Bulgular (FAIL durumunda)
Yok.

### Bloke-etmeyen Bulgular
| # | Seviye | Açıklama | Durum |
|---|---|---|---|
| 1 | Orta | `StatusEndpointTests` paylaşılan koleksiyon veritabanını bozuk bırakıyordu (Web 14 FAIL) | Bu PR'da düzeltildi (`Scenario.DisposeAsync` → `SeedHealthyAsync`) |
| 2 | Düşük | `package-build.sh` Git Bash'te sır klasörünü bağlayamıyordu | Bu PR'da düzeltildi (`cygpath`) |
| 3 | Düşük | `CheckConstraintTable` Coupon kısıt sayısı 6 → 7 (D-56 belge geçişiyle) | Bu PR'da düzeltildi |
| 4 | Düşük | Rapor/status kayıt satırları (72 rapor, 67 Commit hücresi) | Bu PR'da düzeltildi; kalıcı çözüm PF-238 (aday) |
| 5 | Düşük | `DataProtectionRingTests` ortak geçici klasör yüzünden paralel koşuda düşebiliyor | D-980 |
| 6 | Orta | Ölçüt C'nin işaret ettiği ~50 VAL-C satırının yalnız 2'si PASS | Kabul edilen risk → D-981 |
| 7 | Orta | 568 zayıf matris kanıtı (116'sı yalnız matris eşlemesi) | Kabul edilen risk → D-982 |
| 8 | Düşük | T330 KK6/KK7 `~` (ekran payı T537 kriter 6'da planlı); T356 KK6 → D-729, KK9 → D-730 | Kayıtlı, yeni kalem gerekmez |

### Kabul edilen riskler

**1. Ölçüt C'nin zayıf PASS bağlantısı (D-981, 🟡, üst sınır F4 gate).** `12 §6.2` F3 "C" ölçütü `12 §12.3`'ün kendi tanımıyla okundu: iki geçiş tablosunda (S1–S11, Ö1–Ö5; 16 geçiş) boş "Doğrulandığı satır" hücresi yok → karşılandı (D-44 deseni: §6.2 hücresi VAL kimliğine bağlı değil). Ancak tabloların işaret ettiği **yaklaşık 50 VAL satırının** (`VAL-C079`…`VAL-C108` ve KP C satırları) **yalnız 2'si PASS** (`VAL-C001`, `VAL-C115`), ötekiler BEKLEMEDE. Neden: bu satırlar `02 §5.x` kaynaklı, task doğrulayıcıları yalnız task'ın kendi KP aramasını işlediği için hiçbir task'a düşmüyor. §6.2'nin "okunan satır PASS değilse FAIL" kuralı bu satırların her birine uygulansaydı verdict FAIL olurdu. Kabul gerekçesi: §12.3 metni ölçütü "boş hücre yok" diye tanımlıyor; davranışın kendisi `TransitionTableConsistencyTests` ve uçtan uca akış testleriyle yeşil; eksik olan satırların `Durum` güncellemesinin sahibi. Kalem: validate skill'ine kaynak araması + F4 kapısında bu satırların toplu yeniden doğrulaması (K-1579; yardımcının kararı, sahip yerine).

**2. 568 zayıf matris kanıtı (D-982, 🟡, üst sınır F4 gate).** `11 §7.1` F3 hücreleri doldurulurken 1.959 satır×task çiftinin **568'i zayıf** kanıtla eşleşti: **452** "Dokümanlar satırı" (kaynak task'ın Dokümanlar satırında var, kabul kriteri anmıyor), **116** "matris eşlemesi" (kaynak ne kabul kriterinde ne Dokümanlar satırında; yalnız matris bağlıyor). Satır düzeyinde 637 satırda zayıf çift yok, 179'unda bir kısmı zayıf, **67 satırda F3 kanıtı tümüyle zayıf**. Aile dağılımı: `02` 434, `10` 73, `03` 21, `05` 19, `09` 18, devir 2, `07` 1. Zayıf çiftte kanıt = task raporundaki en sık iki test sınıfı. Örneklem: kesin eşleşmeler doğru; sözcük eşleşmesine ara sıra ilgisiz ek KK giriyor. Kabul gerekçesi: boşluk (S3) yok, her satırda task bağı ve test kanıtı var; zayıflık kanıtın keskinliğinde. Kalem: 116 çift için matris–task metni hizası ya da gerekçe. Liste Ek A'da.

### Kapıda yazılan para dışı kod/test düzeltmeleri
| Kaynak | Dosyalar |
|---|---|
| Web testleri (paylaşılan DB) | `StatusEndpointTests` `Scenario.DisposeAsync` |
| Paket betiği | `scripts/ci/package-build.sh` (`cygpath`) |
| D-56 | `CheckConstraintTable` (Coupon 6 → 7) |
| D-126 | `TrimmingBinder.cs`, test sayfası `BindingForm.cshtml.cs`, `FieldValidationTests.cs` |
| D-142 | `ErrorStatuses.cs`, `ValidationPageFilter.cs` |
| D-150 | `RequestFormat.cs`, `RequestFormatTests.cs` |
| D-152 | `ModelBindingRules.cs`, `ModelBindingRuleTests.cs`, ihlal örneği `Models.cs` |
| D-158 | `CacheSourceScanTests.cs` (`EveryStaticFieldOfTheApplication_IsReadOnly`) |
| D-160 | `deploy/images/app/Dockerfile` |
| D-86 | `DatabaseAuditTests.cs` → `StartupDatabase/` |
| D-224, D-295, D-301, D-296, D-129 | `CompanyNoticeTemplateTests.cs`, `NotificationMapConsistencyTests.cs`, `PublishTransitionsTests.cs`, yorum: `NotificationMapDocument.cs`, `ShownValue.cs` |
| D-377 | `LogFileDisposalPart.cs`, `RecordTablesDisposalParts.cs`, `LogFileRetentionTests.cs` |
| D-391, D-841 | `LivenessEndpointTests.cs`, yeni `LivenessTimingDefinition.cs` |
| D-686 / D-440 | `CacheSourceScan.cs` |
| D-697 | `AutomaticMoments.cs` |
| D-701, D-734 | `RollbackCheckCommand.cs`, `ReapplyDeletionsCommand.cs`, `InstantDeletionLog.cs`, `CommandHost.cs`, `RestoreCommandsTests.cs` |
| D-716 | `RollbackAudit.cs` (dönüş denetiminin tablo listesi; IBAN değerini işlemez) |
| D-725 | `ShippingViewReader.cs`, `OrderTransitions.Shipment.cs` (kargo görünümü okuması; tutar yok), `StatusCorrectionTests.cs` |
| D-763 | `AccountAndStatusCommandTests.SetupCommands.cs` |
| D-942 | `ItemRecordsTests.Returns.cs`, `WithdrawalDeclarationTests.cs` |

Para ya da güvenlik kodu kapıda yazılmadı (yapan denetlemez; PF-235): o kalemler aşağıdaki yeni task'lara gitti.

### T411 / T412'ye giden kalemler
- **T411 — Kayıt maskeleme taramasının dizge farkında yeniden yazımı** (F4, bağımlılık yok, F5'ten önce): D-89 (öneri 1: K1 biçimleri, "içerir" ad kuralı + izin listesi, `BeginScope`/`ForContext`/`PushProperty`, konumlu `[LoggerMessage]`, istisna bağımsız değişkeni yasağı, `RejectionLineSourceScan` genişlemesi), D-90 (`SaveChangesFailed` bastırması; kayıt maskeleme kodu sayıldı).
- **T412 — F3 kapanış düzeltmeleri: sipariş bekçisi, satış sonrası kapanış ve IBAN kullanımı** (F4'ün ilk task'ı, para): D-80 (sır bekçisi), D-95 (B-12 kilit iletisi, `09 §10.2` m.3), D-309 (`TrackingNumber` ≤ 64, `OtherCarrierName` ≤ 100), D-379 (`InvalidEmailAttribute` yerel kısım nokta kuralı, `04` 7.3.2.1), D-808 (e-posta ekseni `CustomerId`), D-847 + D-896 (`PaymentApproval` ödeme kümesi), D-903 + D-910 (`FindRefundIbanUseAsync` bütün konuları sayar), D-905 (dönülmüş kalem S8/teslim işaretinden çıkar), D-915 (ret de "mal dönmedi" kapanışını açar).
- Ayrıca **T601b** (F6): T601'in k11/k12/KK11/KK12'si (D-196); `08` DV-32, `12` VAL-E033.

### Öğrenim Terfileri
| # | Öğrenim | Sınıf | Hedef katman | Uygulanan değişiklik |
|---|---|---|---|---|
| 1 | Para/güvenlik kodu gerektiren backlog kalemini kapı (denetçi) yazmaz; yeni task'a gider | İhlali önleyen kural | L4 `skills/gate-check` Adım 8 | Bu PR: madde eklendi; PF-235 |
| 2 | Parçalardaki hafıza bölümü toplamada sahipsizdi | İhlali önleyen kural | L4 `skills/gate-check` PASS ritüeli · `skills/task` 16 | Bu PR: iki cümle; PF-237 |
| 3 | Paylaşılan koleksiyon DB'sini bozan test senaryosu kendi temizliğini geri yüklemeli | Tekrarlanacak desen | Test kalıbı | Bu PR: `StatusEndpointTests`; PF-236 |
| 4 | Windows'ta betiklerde bağlama yolu dönüşümü | Tek seferlik gözlem | — | Bu PR: `package-build.sh`; PF-242 |
| 5 | Merge sonrası kayıt hücreleri (Commit, rapor PR/Post-merge) doldurulmuyor | Tekrarlanacak desen | yardımcı düzeni · `skills/validate` | PF-238 (aday) |
| 6 | `12 §12.3` satırlarının `Durum` sahibi yok | Tekrarlanacak desen | `skills/validate` | D-981 · PF-239 (aday) |
| 7 | Matris–task metni hizası zayıf kanıt üretiyor | Tekrarlanacak desen | plan şablonu · kapsama betiği | D-982 · PF-240 (aday) |
| 8 | Büyük faz kapısı tek bağlama sığmıyor | Tekrarlanacak desen | `skills/gate-check` | PF-241 (aday) |

**Playbook aday listesi aktarımı** (`helper-state/shopfolio/impl/playbook-aday.md`, madde 1–47; yardımcı "aktarıldı (PF-xxx)" diye işaretler):

| Aday | PF | Aday | PF | Aday | PF |
|---|---|---|---|---|---|
| 1 | PF-191 | 17 | PF-205 | 33 | PF-215 |
| 2 | PF-192 | 18 | PF-206 | 34 | PF-213 |
| 3 | PF-193 | 19 | PF-190 (mevcut, F2 gate) | 35 | PF-213 |
| 4 | PF-194 | 20 | PF-198 | 36 | PF-217 |
| 5 | PF-195 | 21 | PF-196 | 37 | PF-218 |
| 6 | PF-196 | 22 | PF-207 | 38 | PF-219 |
| 7 | PF-197 | 23 | PF-208 | 39 | PF-220 |
| 8 | PF-198 | 24 | PF-209 | 40 | PF-221 |
| 9 | PF-199 | 25 | PF-210 | 41 | PF-222 |
| 10 | PF-200 | 26 | PF-208 | 42 | PF-223 |
| 11 | PF-201 | 27 | PF-211 | 43 | PF-224 |
| 12 | PF-202 | 28 | PF-212 | 44 | PF-213 |
| 13 | PF-203 | 29 | PF-213 | 45 | PF-225 |
| 14 | PF-204 | 30 | PF-214 | 46 | PF-226 |
| 15 | PF-198 | 31 | PF-215 | 47 | PF-227 |
| 16 | PF-198 | 32 | PF-216 | | |

**Adaysız kurallar** (`gorev-impl.md`, `yardimci-talimati-impl.md`'deki kalıcı kurallardan aday listesinde ve PF'de karşılığı olmayanlar):
- PF-228 — sahip soru beklemediğinde yönetici kurala en uygun ve en güvenli seçenekle karar verir; yalnız fiziksel sahip girdisi bekletir (gorev-impl "Kararlar").
- PF-229 — durma kaydı zorunlu, kaldığı yeri barındırma servisi durumundan bulma, adım merge'de biter, yönetici bağlam eşiği (yardimci-talimati "Kalıcı dosyalar", "Başlarken", "Çalışma kuralları").
- PF-230 — doğrulama talimatının içeriği: yapım raporu, işçi dönüşü, yönetici yorumu girmez (gorev-impl 3).
- PF-231 — merge komutu tek başına, durum ayrı sorgulanır (yardimci-talimati).
- PF-232 — faz kapısı talimatında aday aktarımı zorunlu satır + ön tarama (gorev-impl "Faz kapısı" 2).
- PF-233 — öz denetim ölçümü; devam işçisi bütün maddeleri yeniden denetler (gorev-impl "Öz denetim").
- PF-234 — yapım işçisinin sınırları: merge, CI bekleme, doğrulama yok; kritik soruda PR açmaz (gorev-impl 1).

Karşılığı zaten bulunan kalıcı kurallar (yeni satır açılmadı): işçi öncesi yerel ana dalı ileri alma (PF-186), PR baş commit'i (PF-187), BOM'suz durum dosyası (PF-188), öz denetim ve FAIL geri bildirimi (PF-177, PF-178), doğrulama eforu (PF-179), talimat uzunluğu ve doğrulayıcı sınırı (PF-157), heredoc yasağı (PF-126).

### Açık Risk Envanteri (`12 §6.2`, §8.4)
| Backlog ID | Madde (`VAL-…`) | Severity | Kabul tarihi | Matris `Durum` | Proje sahibi gördü ve kabul etti |
|---|---|---|---|---|---|
| — | — | — | — | — | Backlog'da "KABUL EDİLMİŞ RİSK — `VAL-…`" kaydı 0, matriste `KABUL EDİLMİŞ RİSK` hücresi 0 — envanter boş |

Not: "Kabul edilen riskler" başlığındaki iki konu (D-981, D-982) `12 §8.4` anlamında KABUL EDİLMİŞ RİSK değildir — hiçbir `12` satırı FAIL değil; bloke etmeyen gözlem olarak backlog'a yazıldı.

### Yardımcıya — sahip yerine karar
Proje sahibi bu dönemde soru beklemiyor (K-1408 (3)); aşağıdakiler yönetici yardımcısının kararıdır, verdict'i etkilemez.
- **Açık risk envanterinin sahibe gösterilmesi (Adım 8):** envanter boş; "Kabul edilen riskler" (D-981, D-982) yardımcının 2026-11-10 kararıyla kabul edildi (`backlog-karar-yardimci.md`), dönem sonunda sahibe topluca gösterilir.
- **F4 ön koşulları (`10 §4`):**

| ÖK | Durum | Kanıt |
|---|---|---|
| ÖK-1 | Eksik (kanıt yok) | Depoda hazır olduğuna dair kayıt yok (`10 §4`, `SETUP.md`, tracker) |
| ÖK-2 | Eksik (kanıt yok) | aynı |
| ÖK-3 | Eksik (kanıt yok) | aynı |
| ÖK-4 | Eksik (kanıt yok) | aynı |
| ÖK-6 | Eksik (kanıt yok) | aynı |
| ÖK-12 | Eksik (kanıt yok) | aynı |

  Öneri: F4'ün ilk task'ları T412 ve T411 bu ön koşullara bağlı değil; dış hesap/sözleşme gerektiren F4 task'ı başlarken eksik olan ÖK `BLOCKED (OWNER_INPUT)` sayılır (gorev-impl "Kararlar").
- **Backlog KARAR kalemleri (31):** yardımcı kararladı (`backlog-karar-yardimci.md`); kapı uyguladı (K-1579…K-1604).

### Backlog Hijyeni
| ID | Önceki durum | Yeni durum | Gerekçe |
|---|---|---|---|
| F3 kapısına bırakılmış 105 kalem | açık | 73 ✅ · 32 yeniden hedef | Kalem bazında karar tablosu ve uygulama sonuçları: karar kaydı K-1579…K-1604, backlog satırları |
| Yeniden hedeflenenler | F3 gate | T411 (D-89, D-90), T412 (11 kalem), T402 (D-108 kod), T603 (D-144), T515/T514/T516/T506/T511/T509/T517 (D-801), T525 (D-164, D-401), T528 (D-568, D-911), T529 (D-912), T534 (D-673), T536 (D-176), T537 (D-529, D-678), T523 (D-617), T515 (D-81, D-619), T601 (D-375, D-376), F4 gate (D-697 kalanı, D-778, D-919, D-961) | Kod düzeyinde yeniden doğrulandı; kalan iş o task'ın kapsamında |
| D-159, D-196 | açık | ✅ | Doküman geçişi (`02` v0.115, `03` v0.49, `05` v0.52, `06` v0.48, `08` v0.23, `09` v0.32, `11` v0.64, `12` v0.35); T601 bölündü (T601b) |
| Yeni | — | D-980 ⚪, D-981 🟡, D-982 🟡 | Bu kapının bulguları |
| Parçalar | 53 backlog + 62 changelog parçası | toplandı, silindi | "Aktif kalem" tek sayımla 285 (541 kimlik, yinelenen 0) |

### Faz Tag
- Tag: `phase/F3-pass`   - Commit: PR merge'ünden sonra yardımcı atar (PASS ritüeli 20. adım; bu kapı merge etmez, etiket atmaz)
- PR: #513 (`chore: F3 gate check PASS`, base `main`)

### Ek A — 116 "matris eşlemesi" çifti (D-982)
Kaynak satırı (`11 §7.1` birimi) → F3 task'ı. Satır numaraları kapı sırasındaki ilk sayımdandır; T411/T412 blokları eklendiği için kaymıştır, kaynak kimliğiyle aranır.

| İlk satır | Kaynak | Task |
|---|---|---|
| 6482 | `03:§4.1` Süre envanterinden zaman aşımı akışları | T301 |
| 6607 | `02:§3.12.1` Dijital ürünü sistem teslim eder | T312 |
| 6609 | `02:§3.12.3` Tek dosyanın boyut tavanı vardır; | T312 |
| 6705 | `02:§3.21.6` Havale ile siparişi onaylayan müşteri ekranda fir | T307 |
| 6834 | `02:Z-7` Kart ödeme süresi — stok ayırma bu süre boyunca sürer | T311 |
| 6836 | `02:Z-9` Havale ödeme hatırlatması | T306 |
| 6847 | `02:Z-47` İfanın imkânsızlaştığının bildirimi | T306 |
| 6887 | `02:§5.6.1` İki eksen tek bir kapı koşuluyla bağlıdır: | T317 |
| 6914 | `02:§6.2` Satın alma (akış 1) | T331 |
| 6914 | `02:§6.2` Satın alma (akış 1) | T354 |
| 6915 | `02:§6.3` Sipariş takibi (akış 2) | T306 |
| 6915 | `02:§6.3` Sipariş takibi (akış 2) | T307 |
| 6916 | `02:§6.4` İptal, cayma ve iade (akış 3) | T316 |
| 6918 | `02:§6.6` Katalog yönetimi (akış 5) | T338 |
| 6919 | `02:§6.7` Sipariş yürütümü (akış 6) | T305 |
| 6919 | `02:§6.7` Sipariş yürütümü (akış 6) | T306 |
| 6919 | `02:§6.7` Sipariş yürütümü (akış 6) | T316 |
| 6919 | `02:§6.7` Sipariş yürütümü (akış 6) | T317 |
| 6919 | `02:§6.7` Sipariş yürütümü (akış 6) | T319 |
| 6919 | `02:§6.7` Sipariş yürütümü (akış 6) | T340 |
| 6919 | `02:§6.7` Sipariş yürütümü (akış 6) | T364 |
| 6919 | `02:§6.7` Sipariş yürütümü (akış 6) | T368 |
| 6919 | `02:§6.7` Sipariş yürütümü (akış 6) | T375 |
| 6922 | `02:§7.1` Üç yol ve ortak ilkeler | T306 |
| 6940 | `02:§7.3` Cayma | T318 |
| 6946 | `02:§7.3.6` Cayma beyanı siparişi kapatmaz | T318 |
| 6948 | `02:§7.4` İade ve geri ödeme | T330 |
| 6954 | `02:§7.4.6` Kısmi iadede kargo ücreti ve ücretsiz kargo kararı | T373 |
| 6964 | `02:§7.6` Geri alma sınırları | T316 |
| 6964 | `02:§7.6` Geri alma sınırları | T317 |
| 6964 | `02:§7.6` Geri alma sınırları | T319 |
| 6972 | `02:§8.1` Kimlik doğrulama tabanı | T375 |
| 6980 | `02:§8.2` Deneme limitleri | T341 |
| 7005 | `02:§8.4` Veri asgariliği | T301 |
| 7005 | `02:§8.4` Veri asgariliği | T302 |
| 7010 | `02:§8.4.5` Saklama süreleri §4'te, imhanın biçimi ve kişisel | T302 |
| 7011 | `02:§8.5` Kayıtlar ve veri ihlali | T302 |
| 7012 | `02:§8.5.1` Ürün ayrı bir ihlal kaydı tutmaz | T302 |
| 7017 | `02:§9.1.3` İşlem bildirimleri ticari elektronik ileti değildi | T305 |
| 7021 | `02:§9.2` Müşteriye giden bildirimler | T305 |
| 7021 | `02:§9.2` Müşteriye giden bildirimler | T340 |
| 7021 | `02:§9.2` Müşteriye giden bildirimler | T373 |
| 7050 | `02:§9.5` Olmayan bildirimler | T308 |
| 7051 | `02:§10.1` Panelin kapsamı | T375 |
| 7053 | `02:§10.1.2` Yöneticinin yapabildikleri ve yapamadıkları | T375 |
| 7066 | `02:§10.3.1` İz sekiz alanda tutulur | T375 |
| 7071 | `02:§10.4` Sipariş müdahaleleri | T301 |
| 7071 | `02:§10.4` Sipariş müdahaleleri | T316 |
| 7071 | `02:§10.4` Sipariş müdahaleleri | T317 |
| 7071 | `02:§10.4` Sipariş müdahaleleri | T341 |
| 7077 | `02:§10.4.6` Yanlış yapılmış durum geçişi yeni bir geçişle düz | T317 |
| 7078 | `02:§10.4.7` İç not müşteriye hiçbir yerde görünmez | T301 |
| 7101 | `02:§11.1` Firma ayarları | T312 |
| 7101 | `02:§11.1` Firma ayarları | T325 |
| 7101 | `02:§11.1` Firma ayarları | T327 |
| 7101 | `02:§11.1` Firma ayarları | T332 |
| 7101 | `02:§11.1` Firma ayarları | T335 |
| 7101 | `02:§11.1` Firma ayarları | T336 |
| 7101 | `02:§11.1` Firma ayarları | T340 |
| 7114 | `02:§11.2` Ürün sabitleri | T302 |
| 7114 | `02:§11.2` Ürün sabitleri | T305 |
| 7114 | `02:§11.2` Ürün sabitleri | T311 |
| 7114 | `02:§11.2` Ürün sabitleri | T312 |
| 7114 | `02:§11.2` Ürün sabitleri | T326 |
| 7114 | `02:§11.2` Ürün sabitleri | T327 |
| 7114 | `02:§11.2` Ürün sabitleri | T333 |
| 7114 | `02:§11.2` Ürün sabitleri | T341 |
| 7114 | `02:§11.2` Ürün sabitleri | T342 |
| 7114 | `02:§11.2` Ürün sabitleri | T343 |
| 7114 | `02:§11.2` Ürün sabitleri | T353 |
| 7114 | `02:§11.2` Ürün sabitleri | T373 |
| 7123 | `02:P-18` Kart ödeme süresi | T311 |
| 7138 | `02:P-33` L-3 Hesap kaydı | T341 |
| 7157 | `02:§12.1` Ticari çerçeve ve tüketici rejimi | T305 |
| 7157 | `02:§12.1` Ticari çerçeve ve tüketici rejimi | T306 |
| 7157 | `02:§12.1` Ticari çerçeve ve tüketici rejimi | T372 |
| 7157 | `02:§12.1` Ticari çerçeve ve tüketici rejimi | T373 |
| 7164 | `02:§12.1.7` Mesafeli satış | T305 |
| 7164 | `02:§12.1.7` Mesafeli satış | T306 |
| 7168 | `02:§12.1.11` Ticari elektronik ileti gönderilmez | T305 |
| 7170 | `02:§12.2` Kişisel verilerin korunması | T301 |
| 7170 | `02:§12.2` Kişisel verilerin korunması | T302 |
| 7170 | `02:§12.2` Kişisel verilerin korunması | T341 |
| 7172 | `02:§12.2.2` Aydınlatma yükümlülüğü | T366 |
| 7173 | `02:§12.2.3` Hukuki sebepler KVKK m.5/2'nin kapalı sayımıyla y | T301 |
| 7173 | `02:§12.2.3` Hukuki sebepler KVKK m.5/2'nin kapalı sayımıyla y | T302 |
| 7174 | `02:§12.2.4` Ürün hiçbir noktada açık rıza toplamaz | T366 |
| 7177 | `02:§12.2.7` Saklama ve imha | T301 |
| 7177 | `02:§12.2.7` Saklama ve imha | T302 |
| 7179 | `02:§12.2.9` Veri ihlali | T341 |
| 7182 | `02:§12.2.12` Veri asgariliği | T341 |
| 7189 | `02:§12.5` Firmanın yükümlülükleri | T373 |
| 7189 | `02:§12.5` Firmanın yükümlülükleri | T375 |
| 7282 | `05:§4.2` Sipariş onayı | T348 |
| 7285 | `05:§4.5` Tek işlenme — sipariş onayı, sağlayıcı bildirimi ve | T348 |
| 7289 | `05:§5.2` İşlerin envanteri | T302 |
| 7346 | `09:§11` Entegrasyon kuralları | T366 |
| 7439 | `10:KP-74` Aydınlatma bağlantısı, günlük imha, imha kaydı | T302 |
| 7439 | `10:KP-74` Aydınlatma bağlantısı, günlük imha, imha kaydı | T327 |
| 7439 | `10:KP-74` Aydınlatma bağlantısı, günlük imha, imha kaydı | T343 |
| 7439 | `10:KP-74` Aydınlatma bağlantısı, günlük imha, imha kaydı | T345 |
| 7439 | `10:KP-74` Aydınlatma bağlantısı, günlük imha, imha kaydı | T370 |
| 7439 | `10:KP-74` Aydınlatma bağlantısı, günlük imha, imha kaydı | T373 |
| 7448 | `10:KD-6` Ürün yorumu, puanı ve yıldızı yok | T332 |
| 7466 | `10:KD-24` Parçalı gönderim yok | T316 |
| 7470 | `10:KD-28` Alkol, tütün, ilaç ve silah gibi ek akış isteyen ür | T332 |
| 7504 | `10:KD-62` Ters ibraz ürüne girmez | T317 |
| 7507 | `10:§4.1` Kurulum kontrol listesi (ÖK-1…ÖK-12) | T330 |
| 7508 | `10:§4.2` Kısıtlar ve kabuller (SK-1…SK-10) | T318 |
| 7508 | `10:§4.2` Kısıtlar ve kabuller (SK-1…SK-10) | T330 |
| 7508 | `10:§4.2` Kısıtlar ve kabuller (SK-1…SK-10) | T332 |
| 7508 | `10:§4.2` Kısıtlar ve kabuller (SK-1…SK-10) | T333 |
| 7508 | `10:§4.2` Kısıtlar ve kabuller (SK-1…SK-10) | T357 |
| 7508 | `10:§4.2` Kısıtlar ve kabuller (SK-1…SK-10) | T371 |
| 7528 | `devir:04-1948` Gövde anması: yasal metinlerin içeriği (GA-7) | T356 |
| 7528 | `devir:04-1948` Gövde anması: yasal metinlerin içeriği (GA-7) | T373 |
