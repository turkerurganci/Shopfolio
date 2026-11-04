## Gate Check Sonucu — F1 Veri katmanı
**Tarih:** 2026-11-04   **Task aralığı:** T101–T120   **Toplam task:** 20
**Base tag:** `phase/F0-pass` (`5e23745`) → main HEAD `c59321d`

### Verdict: ✓ PASS

Testler bütün katmanlarda yeşil, build temiz, üç imaj `--no-cache` derleniyor, yerel referans kurulum sıfırdan ayağa kalkıyor ve 18 göçü uyguluyor. Göç ileri, idempotent (ikinci uygulama no-op, şema ve veri dökümü özdeş) ve geri alınabilir (F0 ve T118 imajları F1 şemasında açılıyor). Eşitlik denetimleri yeşil, seed sayıları beklenen. F1'e bağlı 144 matris satırı implement edilmiş ve kanıtlı (S3 yok), S2 yok. Para sütunları faz düzeyinde tutarlı: 31 tutar `numeric(12,2)`, 6 oran `numeric(5,2)`, kayan nokta yok, hepsinde sınır kısıtı. `12 §6.2`'nin F1 hücresi bir `VAL-…` satırına bağlı değil (D-44 genişletildi). F1 task'larında `PASS` alan tek satır `VAL-A838` yeniden doğrulandı ve PASS kaldı. Açık risk envanteri boş.

### Ön Kontrol
- **Task durumları:** T101–T120 hepsi `✓` (`IMPLEMENTATION_STATUS.md`); BLOCKED ya da FAIL yok.
- **Rapor tutarlılığı:** yirmi rapor var ve PASS; status ile verdict'ler ve PR numaraları birebir. Bu PR'da düzeltilenler: status'un yirmi Commit hücresi dalın head hash'ini taşıyordu, squash hash'leri yazıldı. "Post-merge run'lar" satırı T101, T103, T106, T107, T118, T119 ve T120'de boştu, T102, T104 ve T105'te hiç yoktu; dolduruldu. T103 raporunda PR numarası yoktu ("dönüşte yazılı" → #360). T115 raporunda CI ve Publish kimlikleri ters yazılmıştı. T114 raporu Publish'i failure diye bırakmıştı (ikinci denemede success).
- **Working tree:** temiz (`origin/main` = `c59321d`).
- **Ana dal CI:** HEAD `c59321d` — CI 41858025823 (9 iş + CI Gate success) ve Publish 41858025821 success; son üç run yeşil. Her merge'ün iki workflow'u da success:

| Task | Squash | PR | CI | Publish |
|---|---|---|---|---|
| T101 | `f2470d8` | #358 | 41710384453 | 41710384654 |
| T102 | `597e14a` | #359 | 41715894986 | 41715895065 |
| T103 | `6b5cc83` | #360 | 41728889974 | 41728889871 |
| T104 | `0c270de` | #361 | 41746250013 | 41746249953 |
| T105 | `7b75bb5` | #362 | 41751877585 | 41751877590 |
| T106 | `93c164e` | #363 | 41767748868 | 41767748965 (2. deneme) |
| T107 | `d374bc1` | #364 | 41770864703 | 41770864743 |
| T108 | `6c390c2` | #365 | 41777013441 | 41777013421 |
| T109 | `521bb61` | #366 | 41782286493 | 41782286498 |
| T110 | `f5b786c` | #367 | 41786270867 | 41786270854 |
| (#368 bekçi) | `78075dd` | #368 | 41794927676 | 41794927593 |
| T111 | `f90d538` | #369 | 41798998317 | 41798998313 |
| T112 | `3add87b` | #370 | 41805273814 | 41805273609 |
| T113 | `0476eed` | #371 | 41809802367 | 41809802191 |
| T114 | `7fa45cf` | #372 | 41815985996 | 41815986016 (2. deneme) |
| T115 | `d2c7c36` | #373 | 41820820591 | 41820820561 |
| T116 | `7afeaff` | #374 | 41825046994 | 41825046782 |
| T117 | `b7bd9b2` | #375 | 41830794252 | 41830794202 |
| T118 | `f088c3f` | #376 | 41839444366 | 41839444317 |
| T119 | `07e6121` | #377 | 41848143907 | 41848143892 |
| T120 | `c59321d` | #378 | 41858025823 | 41858025821 |

- **Ortam:** Windows 11 + Docker Desktop (bellek ~4 GB) · .NET SDK 10.0.400 · Docker 29.2.1 · Compose v5.1.0 · git 2.42 · gh 2.89 · Python 3.12.10 · PostgreSQL 18.6.

### Test Sonuçları
| Katman | Tür | Sonuç | Detay |
|---|---|---|---|
| Lint | `restore --locked-mode` + `dotnet format --verify-no-changes` + Consistency | ✓ | Consistency 101/101 |
| Unit | xUnit v3 | ✓ | 241/241 |
| Integration | PostgreSQL `postgres:18.6` | ✓ | 952/952, tek koşu 84 sn; Npgsql zaman aşımı yok (D-68) |
| Betik testleri | `tests/scripts/run-script-tests.sh` (Debian konteyneri) | ✓ | 4 dosya: 21 + 13 + 129 + 27 assertion |
| Göç modeli | `dotnet ef migrations has-pending-model-changes` | ✓ | değişiklik yok |
| E2E · Contract | — | N/A | `NOT_APPLICABLE` (E2E F6 T601, D-41) |
| Önceki faz (F0) | aynı koşu | ✓ | F0'ın testleri yukarıdaki sayılarda (Unit, Integration, betik testleri, Consistency); kırılan yok |

**Regresyon (`12 §6.2`, §11):** F0'da `PASS` alan `12` satırı yoktu. Önceki fazın kanıtı yeniden üretildi: canlılık ucu taze ortamda 200, üç imaj derleniyor, betik testleri yeşil.

### Build
| Bileşen | Sonuç | Detay |
|---|---|---|
| Çözüm (Release, `-warnaserror --no-incremental`) | ✓ | 0 uyarı, 0 hata (19 sn) |
| NuGetAudit (restore) | ✓ | bilinen açık yok |
| `dotnet format --verify-no-changes` | ✓ | temiz |
| Uygulama imajı (`--no-cache`) | ✓ | 79 sn |
| Veritabanı imajı (`--no-cache`) | ✓ | 8 sn |
| Yedek aracı imajı (`--no-cache`) | ✓ | 5 sn |

### Fresh Environment
Yapılandırma: **yerel referans kurulum** (`scripts/local-reference/`, `DEPLOY_RUNBOOK §K`; `IMAGE_TAG=f1gate`). Reçete sıfırdan koşuldu: `host-up`, `storage-setup` (All checks passed), `install-installation`. Gerçek sunucuya ilk iniş T603'tedir (D-40). Reçetenin kaynak adları sabit `t09-*` olduğu için kurulum o adlarla açıldı (D-71).

| Servis | Durum |
|---|---|
| `proxy` (Caddy) | up |
| `app` | up — konteyner içi canlılık 200; `https://shopfolio.internal` canlılık 200 |
| `database` | up |
| `backup` | up |
| `migrate` (tek seferlik) | Exited (0), 18 göç; ikinci `migrate` "No migrations were applied", çıkış 0 |
| Denetimler | secrets, bootstrap ✓ · image-ids 3 ok · isolation 7 ok · egress 6 ok · redirect-hsts 3 ok · client-address 3 ok |

Kurulumda seed: 81 il, 249 ülke, 81 teslimat ili, 2 yasal metin taslağı, 6 tek satırlık kayıt (her biri 1 satır); 80 tablo (79 + göç geçmişi). Göç referans kuruluma uygulandı (`11 §6.2` F1).

Temizlik: `down.sh all`; `shopfolio-*:f1gate` imajları, geçici `shopfolio-f1gate-pg` konteyneri ve `debian:trixie-slim` imajı silindi; `t09-*` kalıntısı yok; `skinora-*` ve başka `shopfolio-*` konteynerlerine dokunulmadı.

### Migration
Ayrı prova: boş PostgreSQL 18.6 (`shopfolio-f1gate-pg`), veritabanı kurulum SQL'iyle.

| Adım | Sonuç |
|---|---|
| Model doğrulama | ✓ `has-pending-model-changes` değişiklik yok |
| İlk apply | ✓ demet 18 göçü 4 sn'de uyguladı; `Down` yok (0/18); yıkıcı işlem yok (`AlterColumn` yalnız varsayılan ekler, D-52) |
| İdempotency (2. apply) | ✓ 0 göç; şema ve veri dökümü (`pg_dump`) özdeş (`\restrict` satırı hariç) |
| Seed | ✓ 81 il (kod plaka), 249 ülke, 81 teslimat ili, 2 taslak (yayın anı ve numara boş), 6 tek satır |
| Geri alınabilirlik | ✓ CI'ın geri alma sınaması sürüm etiketi olmadığından atlanıyor (D-19, T603). Elle: F0 gate imajı (`5e23745`) ve T118 imajı (`f088c3f`) F1 şemasında canlılık 200; negatif kontrol (yanlış port) 503 |

**Para (faz düzeyi):** 31 para sütunu `numeric(12,2)`, 6 oran `numeric(5,2)`, `variants.net_quantity` `numeric(12,3)`; `real`/`double precision` yok. 31 tutar sütununun hepsinde sınır kısıtı var. `ModelRulesConvention` türü yazılmamış `decimal`'ı reddeder.

### Traceability
`11 §7.1`'de F1 task'larına bağlı 144 satır var: 105'i yalnız F1, 39'u başka fazlarla ortak. Her satırın F1 payı kabul kriteri, test ya da kayıtla eşlendi. "Implement edildi" ve "Kanıt" hücreleri bu PR'da dolduruldu (`11 §6.1` madde 3); F0 payı dolu altı satırda F1 payı eklendi. `GATE_DOLU = {"F0", "F1"}`; kapsama betiği çıkış 0, sorun 0.

| Kategori | Eşlenen | Implement | Boşluk (S3) | Kanıt |
|---|---|---|---|---|
| `06` Veri Modeli — entity (§4) | 79 | 79 | 0 | `11 §7.1.1` hücreleri; task'ların tablo testleri + T120 |
| `06` — alt bölüm (§2, §5, §6, §7, §8) | 30 | 30 | 0 | aynı |
| `02` Ürün Gereksinimleri | 18 | 18 (F1 payı) | 0 | `11 §7.1.5` |
| `05` Teknik Mimari | 11 | 11 (F1 payı) | 0 | aynı |
| `09` Kodlama Kuralları | 5 | 5 (F1 payı) | 0 | aynı |
| `10` MVP Kapsamı (KP-75) | 1 | 1 (F1 payı) | 0 | aynı |
| **Toplam** | **144** | **144** | **0** | |

**`06 §7.4` ve §7.6 (yardımcının kararı, sahip yerine):** T120'nin kabul kriterleri yalnız §7.1, §7.2, §7.3, §7.5 ve §2.7'yi adlandırır. Bu yüzden §7.4 ve §7.6 T120'nin eksiği sayılmadı. §7.4 tablosunun dayandığı kısıt ve dizinler modelde (`AppDbContextModelSnapshot`, göç SQL'i) tek tek arandı ve hepsi bulundu. Bulunanlar: `Variant` stok ≥ ayrılmış ve ayrılmış ≥ 0; `StockReservation`'ın iki filtreli tekil dizini; kupon haklarının ≥ 0 kısıtı; `OrderItem` indirme sayısı ≤ hak; `LinkRecord` kullanım/yerine geçme en çok biri ve `TokenHash` tekil; `Session` iznin iki alanı birlikte; `Customer` ve `Admin` `NormalizedEmail` tekil; `AdminInvitation` üç anın en çok biri ve adresin filtreli tekil dizini; `InternalNote.OrderId` tekil; beş `Slug` tekil dizini; hakkımızda ve duyurunun `((true))` dizini; `CartItem` filtreli tekil; `Cart.CustomerId` tekil; `RefundRecord`'un iki, `Refund`'ın iki, `InboundNotificationRecord`'un iki filtreli tekil dizini; `PaymentRecord.ProviderTransactionId`; `Order.ConfirmationKey`; `Job`'un bekleyen satır dizini; `ChannelStateRecord`'un iki kısıtı; `DefectClaim.OrderItemId` tekil; yasal metin sürümünün iki filtreli dizini. Tablonun "veritabanı kısıtı değildir" dediği üç koşul uygulamanındır (F3). Bulgu yok. §7.6 hacim kabulüdür, model öğesi değildir. Dizin setinin yalnız tekillik, bağ ve §7.5 dizinlerinden oluşması T120'nin iki yönlü dizin eşitliğiyle (`IndexConsistencyTests`) karşılanır.

**Vacuous test taraması:** `skip`, `only`, sabit doğru ve boş assertion yok. Sezgisel taramanın üç eşleşmesi `InlineData` parantezinden doğan yanılgı.

**Doküman uyumu:** belge–model eşitliği T120'nin dört ailesiyle mekanik (ilişki 130/130, tekil dizin, kontrol kısıtı, sorgu dizini) ve saklanan türetilmiş değerle sınanıyor. Kalan doküman uçları backlog'da: D-51, D-55, D-56, D-58, D-63 (`06`/`09` doküman geçişi), D-45 (`09` "bilgim tarihli" ibareleri), D-57, D-65, D-67 (`jsonb` alanlarının tipi), D-70 (saklanan değer denetiminin ad kalıbı).

### Faz Satırları (`12 §6.2`)
| Satır (`VAL-…`) | Severity | Kanıt | Önceki `Durum` | Yeni `Durum` | Backlog kaydı (KABUL EDİLMİŞ RİSK ise) |
|---|---|---|---|---|---|
| — (F1 hücresi "D — göçün ileri, idempotent ve geri alınabilir olması; model eşitlik denetimleri"; seviye D'de karşılık gelen satır yok) | — | `11 §6.2` F1: göç ileri ve idempotent (Migration tablosu), geri alınabilir (iki eski imaj F1 şemasında 200), eşitlik denetimleri 101/101, göç yerel referans kuruluma uygulandı | — | — | — (doküman boşluğu D-44, F1 için genişletildi) |
| `VAL-A838` (T106 KK4; F1'de `PASS` alan tek satır, gate'te yeniden doğrulandı) | KRİTİK | `prosecdef` t, `search_path` sabit, sahip migrator, EXECUTE yalnız migrator ve app; salt okuma kullanıcısının çağrısı "permission denied", uygulama kullanıcısının çağrısı 0 satır | PASS | PASS (değişmedi) | — |

`12`'nin 1.503 satırından yalnız `VAL-A838` `PASS`, öteki satırlar `BEKLEMEDE`. F1 task'larının kaynak birimlerinden gelen öteki satırlar davranış satırıdır, koşumları sonraki fazlarda. F1 gate'inde okunmadılar, `Durum` hücreleri değişmedi.

### Final Çıkış (`12 §6.3`) — yalnız son faz (F6)
Uygulanamaz (F1).

### Güvenlik Özeti
- **Açık bulgu:** 0.
- **Yeni dış bağımlılıklar (`phase/F0-pass..c59321d`):** NuGet `Microsoft.AspNetCore.DataProtection.EntityFrameworkCore` 10.0.14 (T118, MIT), `Microsoft.EntityFrameworkCore.Relational` 10.0.14 (T102; EF Core ile birlikte sabitlendi). Yükseltmeler (#357, D-43): EF Core 10.0.11 → 10.0.14, `Microsoft.AspNetCore.Mvc.Testing` 10.0.14, `TngTech.ArchUnitNET` 0.13.5, `dotnet-ef` 10.0.14, `actions/checkout` v7, `actions/setup-dotnet` v5. NuGetAudit temiz.
- **Auth değişiklikleri:** kullanıcı kimlik doğrulaması kodu yok (F2/F3). Veri katmanı: oturum, bağlantı ve tanınan tarayıcı belirteçleri yalnız özetle, tekil dizinle (T108). İz tablosunda uygulama kullanıcısı yalnız ekler ve okur; imha `SECURITY DEFINER` fonksiyonuyla, çağrı yalnız uygulama kullanıcısında (T106, `VAL-A838`). `data_protection_keys` salt okuma kullanıcısından alındı; durum kullanıcısına yalnız `backup_status_records`'un iki sütunu (T118).
- **Gömülü sır taraması:** temiz.
- **Açık PR:** yok (yedi Dependabot PR'ı #357'yle alındı ve kapandı).

### Bulgular (FAIL durumunda)
Yok.

### Bloke-etmeyen Bulgular
| # | Seviye | Açıklama | Durum |
|---|---|---|---|
| 1 | DÜŞÜK | `12 §6.2` F1 hücresi bir `VAL-…` kimliğine bağlı değil | D-44 (F1 için genişletildi, en geç F2 gate) |
| 2 | DÜŞÜK | Rapor tutarlılığı: yirmi Commit hücresi head hash, on raporda post-merge boş ya da yok, T103 PR no, T115 ters kimlik, T114 Publish yeniden denemesi | Bu PR'da kapandı |
| 3 | DÜŞÜK | "Sonraki PR" hedefli beş backlog kalemi hedefini kaçırdı (D-20, D-21, D-22, D-45, D-60) | Bu PR'da yeniden hedeflendi; kayıt kuralı 5 (öğrenim 2) |
| 4 | DÜŞÜK | Yerel referans kurulum reçetesinin kaynak adları sabit `t09-*`; önek parametresi yok | D-71 |

### Öğrenim Terfileri
| # | Öğrenim | Sınıf | Hedef katman | Uygulanan değişiklik |
|---|---|---|---|---|
| 1 | F0 gate'inin kuralı (`skills/task` 15. adım) yalnız boş Commit hücresini kapsıyordu. Doğrulama status'u merge'den önce yazdığı için hücreye dalın head hash'i girdi ve kural hiç tetiklenmedi: F1'in yirmi hücresi head hash'iydi, on raporda post-merge satırı boş ya da yoktu | İhlali önleyen kural | L4 `skills/task` | 15. adım squash hash'i olmayan hücreyi ve eksik satırı da kapsar; PF-175 |
| 2 | "X'e dokunan sonraki PR" hedefi task PR'larında görülmüyor: `09`'a dört PR, bekçiye iki PR dokundu, ilgili kalemler açık kaldı | İhlali önleyen kural | L2 `DEFERRED_BACKLOG.md` kayıt kuralları | Kural 5: böyle hedef bir üst sınır da yazar, gate üst sınırı geçeni yeniden hedefler; beş kalem yeniden hedeflendi, beş doküman geçişi kalemine üst sınır eklendi; PF-176 |
| 3 | Task boyutu (`11 §6.1` madde 4, K-1409 (3)), ikinci ölçüm: yirmi task'ın 13'ü devir dosyasıyla bitti (20 dosya). Doğrulama turu toplamı 27 (T104 ve T106 üçer). Merge araları işlenen belirteç (M): T101 24,8 · T102 34,9 · T103 84,4 · T104 103,0 · T105 36,4 · T106 74,8 · T107 29,5 · T108 55,6 · T109 46,9 · T110 26,6 · T111 28,3 · T112 34,0 · T113 27,2 · T114 26,4 · T115 17,2 · T116 15,7 · T117 21,3 · T118 47,3 · T119 42,8 · T120 39,4 (+ #357 20,1, #368 27,6). Ortalama ~41 M; F0'da ~71 M | Tek seferlik gözlem (ölçüm) | — | Eşik değişmedi (en çok 10 kriter, blok 2,5 KB): F1 task'ları F0'dan ucuz, tur ortalaması 1,35. Devir dosyası bağlam sınırının olağan sonucu. Yardımcıya — sahip yerine karar |
| 4 | Yerel referans kurulum reçetesi sabit `t09-*` adlı; gate'in kendi ad kuralı uygulanamadı | Tek seferlik gözlem | — | D-71 |
| 5 | `12 §6.2` faz satırları F0'dan sonra F1'de de kimliksiz; F2 hücresi ("D — kayıt maskeleme") aynı risk taşıyor | Tek seferlik gözlem | — | D-44 kapsamında, en geç F2 gate |

### Açık Risk Envanteri (`12 §6.2`, §8.4)
| Backlog ID | Madde (`VAL-…`) | Severity | Kabul tarihi | Matris `Durum` | Proje sahibi gördü ve kabul etti |
|---|---|---|---|---|---|
| — | — | — | — | — | Envanter boş: "KABUL EDİLMİŞ RİSK" kaydı 0, matriste `KABUL EDİLMİŞ RİSK` hücresi 0 |

### Backlog Hijyeni
| ID | Önceki durum | Yeni durum | Gerekçe |
|---|---|---|---|
| D-43 | 🟡 açık | ✅ | #357 (`66af43e`) yedi güncellemeyi aldı; Dependabot PR'ları kapalı, açık PR yok |
| D-20 | T602 ya da "`05`/`09`'a dokunan sonraki PR" | T602 | `09`'a dört PR dokundu, çelişki duruyor |
| D-21 | "en geç F1 gate" | yardımcının kararı, en geç F2 gate | Bekçiye #363 ve #368 dokundu; karar verilmedi; takma ad altı kaynak dosyada |
| D-22 | "`09`'a dokunan sonraki PR" ya da T233 | T233 | Sonraki PR kolu kaçtı |
| D-44 | F0 hücresi, en geç F1 gate | F0 + F1 hücresi, `12`'nin sonraki sürümü, en geç F2 gate | F1 hücresi de kimliksiz |
| D-45 | "`09`'a dokunan sonraki PR" | `09` doküman geçişi, en geç F2 gate | T108, T118, #357 `09`'a dokundu, ibareler kalmış |
| D-51, D-55, D-56, D-58, D-63 | sonraki doküman geçişi | aynı; en geç F2 gate | Kayıt kuralı 5 |
| D-60 | `scripts/git-hooks/`'a dokunan sonraki PR | aynı, en geç F2 gate | #368 dokundu, README güncellenmedi |
| D-68 | hedefsiz | test veritabanı fikstürlerine dokunan sonraki task, en geç F2 gate | Gate'in koşusunda zaman aşımı yok; hedef eksikti |
| D-70 | "F1 gate hijyeni" ya da ilk task | `StoredDerivedValueConsistencyTests`'e ya da `06 §2.7`'ye dokunan ilk task, en geç F2 gate | Kod düzeyinde yeniden okundu, durum aynı; test davranışı değişikliği task işidir |
| D-71 | — | ⚪ yeni | Reçetenin sabit `t09-*` adları |

Hâlâ açık ve hedefi geçerli (kod düzeyinde ya da hedef task'ı başlamadığı için): D-10, D-13, D-19, D-24, D-25, D-26, D-27, D-32, D-33, D-35, D-36, D-38, D-39, D-40, D-41, D-42, D-47, D-53, D-57, D-59, D-64, D-65, D-66, D-67, D-69. D-38 (a)'ya T118 veritabanı betiğinde dokundu, tarama değişmedi; üst sınırı T603, geçerli. Aktif kalem 39 → 39 (D-43 kapandı, D-71 açıldı).

**Yardımcıya — sahip yerine karar:** (1) task boyutu eşiği (öneri: değişmez); (2) D-21'in tasarım kararı (bekçi istisnası mı, takma ad kuralı mı); (3) D-70'in önerisi (yasak ad listesi); (4) açık risk envanteri boş, kabul edilecek kalem yok.

### Faz Tag
- Tag: `phase/F1-pass`   - Commit: gate PR'ının squash commit'i (merge ve etiket yardımcıda, PASS ritüeli 20. adım)
