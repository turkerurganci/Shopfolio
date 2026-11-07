## Gate Check Sonucu — F2 Çekirdek servisler
**Tarih:** 2026-11-07   **Task aralığı:** T201–T233   **Toplam task:** 33
**Base tag:** `phase/F1-pass` (`8700785`) → main HEAD `f4da03c`

### Verdict: ✓ PASS

Testler bütün katmanlarda yeşil, build temiz, üç imaj `--no-cache` derleniyor, yerel referans kurulum sıfırdan ayağa kalkıyor. F2'de göç yok; F1'in 18 göçü boş veritabanına uygulanıyor, ikinci uygulama no-op, şema ve veri dökümü özdeş, seed sayıları F1 ile aynı. `11 §6.2` F2 ek kontrolü sağlandı: mimari testler kasıtlı ihlalle kırmızıya dönüyor (3 test), kayıt maskeleme testleri yeşil. F2'ye bağlı 189 matris satırı implement edilmiş ve kanıtlı (S3 yok), S2 yok. `12 §6.2`'nin F2 hücresi bir `VAL-…` satırına bağlı değil (D-44 genişletildi, D-159). F2'de `PASS` alan tek satır `VAL-A991` (KRİTİK) gate'te yeniden doğrulandı ve PASS kaldı. Açık risk envanteri boş.

### Ön Kontrol
- **Task durumları:** T201–T233 hepsi `✓` (`IMPLEMENTATION_STATUS.md`); BLOCKED ya da FAIL yok.
- **Rapor tutarlılığı:** otuz üç rapor var ve PASS; status ile verdict'ler ve PR numaraları birebir. Bu PR'da düzeltilenler: beş raporun başlık `Durum`'u eski (`⏳`) kalmıştı (T203, T215, T217, T221, T225), üçünde "merge bekliyor" yazıyordu (T219, T229, T230) — hepsi `✓ Tamamlandı`. T205 ve T210 raporlarında CI ve Publish kimlikleri ters yazılmıştı; T216'da kimlikler yoktu, T229'da satır boştu; T230 raporunda PR numarası yoktu (#386). Status'ta T229 Commit hücresi boştu (`f4da03c`), dokuz hücre (T205, T225, T226, T227, T230–T233) backtick'sizdi.
- **Working tree:** temiz (`origin/main` = `f4da03c`).
- **Ana dal CI:** HEAD `f4da03c` — CI 42306192016 ve Publish 42306192052 success; son üç run yeşil. Otuz beş merge'ün (33 task + #381 + #390) iki workflow'u da success:

| Task | Squash | PR | CI | Publish |
|---|---|---|---|---|
{{RUNS}}

- **Ortam:** Windows 11 + Docker Desktop (bellek ~4 GB, 12 çekirdek; makinede başka bir projenin 14 konteyneri çalışıyordu) · .NET SDK 10.0.400 · Docker 29.2.1 · Compose v5.1.0 · git 2.42 · gh 2.89 · Python 3.12.10 · PostgreSQL 18.6.

### Test Sonuçları
| Katman | Tür | Sonuç | Detay |
|---|---|---|---|
| Lint | `restore --locked-mode` + `dotnet format --verify-no-changes` + Consistency | ✓ | üç proje temiz; Consistency 207/207 |
| Unit | xUnit v3 | ✓ | 2619/2619 (Consistency hariç); toplam 2826 |
| Integration | PostgreSQL `postgres:18.6` | ✓ | 1730/1730, parçalı (Web 542 altı parça, Security + SignedEndpoints + Outages + FileSecrets 75, Concurrency + Disposal 44, Jobs 77, Files + Logging 63, Database 36, Model 893 üç parça `--max-threads 3`) |
| Betik testleri | `tests/scripts/run-script-tests.sh` (Debian konteyneri) | ✓ | 4 dosya: 21 + 13 + 129 + 27 assertion |
| Göç modeli | `dotnet ef migrations has-pending-model-changes` | ✓ | {{PENDING}} |
| E2E · Contract | — | N/A | `NOT_APPLICABLE` (E2E F6 T601, D-41) |
| Önceki fazlar (F0, F1) | aynı koşu | ✓ | F0 ve F1 testleri yukarıdaki sayılarda; kırılan yok |

**Altyapı artefaktları (`skills/gate-check` Adım 1 kutusu; ikisi de izole koşuda geçti, aynı commit'in CI'ı yeşil):** (1) ilk Web koşusunda `SearchEngineDataTests` "address in use" (127.0.0.1:5000): Kestrel açan dört test sınıfı varsayılan portta dinliyordu — D-128'in nedeni, bu PR'da `UseKestrel(0)` ile düzeltildi. (2) Database ve Model koşularında fikstürün `CREATE/DROP DATABASE` komutu yük altında Npgsql'in 30 sn varsayılanını aştı — D-68, bu PR'da yönetici komutuna 120 sn; Database 36/36 yeniden koşuldu, kalıntı veritabanı yok. Integration'ın göç paketi artık önbellekte (D-157): aynı derleme için ilk koşu ~110 sn, sonrakiler ~25 sn. Gate'in Database koşuları `tmpfs` üzerinde `fsync=off` bir PostgreSQL ile yapıldı (yalnız hız; CI varsayılan yapılandırmada).

**Regresyon (`12 §6.2`, §11):** F0 ve F1'de `PASS` alan `12` satırı `VAL-A838` (T106) — kanıtı `SECURITY DEFINER` imha fonksiyonunun testleriyle Integration koşusunda (`Model` ad alanı) yeniden üretildi, yeşil. Önceki fazların kanıtı: canlılık ucu taze ortamda 200, üç imaj derleniyor, göç zinciri temiz, betik testleri yeşil.

### Build
| Bileşen | Sonuç | Detay |
|---|---|---|
| Çözüm (Release, `-warnaserror --no-incremental`) | ✓ | 0 uyarı, 0 hata (85 sn) |
| NuGetAudit (restore) | ✓ | bilinen açık yok |
| `dotnet format --verify-no-changes` | ✓ | üç proje temiz |
| Uygulama imajı (`--no-cache`) | ✓ | katmanlar sıfırdan: restore 178 sn, publish 192 sn, göç paketi 567 sn (makine yüklü). Komut süre sınırı yüzünden yapı Docker içinde ayrı bir istemci konteyneriyle koşuldu. İmaj 781 MB: SkiaSharp'ın Windows ve macOS yerel kütüphaneleri ve PDB'leri (`/app/runtimes`, 467 MB) imaja giriyor → D-160 |
| Veritabanı imajı (`--no-cache`) | ✓ | 73 sn |
| Yedek aracı imajı (`--no-cache`) | ✓ | 38 sn |

### Fresh Environment
{{FRESH}}

### Migration
Ayrı prova: boş PostgreSQL 18.6 (`f2gate-pg`), veritabanı imajının kurulum SQL'iyle (`10-roles` … `40-schema`), demet göç paketi (`efbundle`) şema sahibi kullanıcısıyla.

| Adım | Sonuç |
|---|---|
| Model doğrulama | ✓ `has-pending-model-changes` {{PENDING}} |
| İlk apply | ✓ 18 göç 21 sn'de uygulandı; F2'de yeni göç yok (`git diff phase/F1-pass..HEAD -- src/Shopfolio.Web/Infrastructure/Migrations` boş) |
| İdempotency (2. apply) | ✓ "No migrations were applied"; şema ve veri dökümü (`pg_dump`) özdeş (`\restrict` satırı hariç) |
| Seed | ✓ 81 il, 249 ülke, 81 teslimat ili, 2 yasal metin taslağı, 6 tek satırlık kayıt (her biri 1 satır); 80 tablo (79 + göç geçmişi) — F1 ile aynı |
| Geri alınabilirlik | Uygulanamaz: F2 şemayı değiştirmedi; F1 imajları aynı şemada açılır (F1 gate kanıtı) |

### Traceability
`11 §7.1`'de F2 task'larına bağlı 189 satır var: 64'ü yalnız F2, 125'i başka fazlarla ortak. Her satırın F2 payı kabul kriteri, test ya da kayıtla eşlendi; "Implement edildi" ve "Kanıt" hücreleri bu PR'da dolduruldu (`11 §6.1` madde 3). Zayıf altı satır (T201 KK1, KK4, KK5, KK7; T220, T203) task raporlarından elle tamamlandı. `GATE_DOLU = {"F0", "F1", "F2"}`; kapsama betiği çıkış 0, sorun 0; 105 test sınıfı adı matriste geçiyor; F0 ve F1 hücreleri bozulmadı.

| Kategori | Eşlenen | Implement | Boşluk (S3) | Kanıt |
|---|---|---|---|---|
| `05` Teknik Mimari | 75 | 75 (F2 payı) | 0 | `11 §7.1` hücreleri; task testleri |
| `02` Ürün Gereksinimleri | 42 | 42 (F2 payı) | 0 | aynı |
| `10` MVP Kapsamı | 23 | 23 (F2 payı) | 0 | aynı |
| `07` Kullanıcı Akışları | 16 | 16 (F2 payı) | 0 | aynı |
| `09` Kodlama Kuralları | 16 | 16 (F2 payı) | 0 | aynı |
| `03`, `06`, `08`, `04`, devir kararları | 5 + 4 + 3 + 2 + 3 | 17 | 0 | aynı |
| **Toplam** | **189** | **189** | **0** | |

**Vacuous test taraması:** `Skip`, `Explicit`, `Assert.Skip`, sabit doğru ve boş assertion yalnız mimari testin kasıtlı ihlal örneğinde (`ViolationSamples/ForbiddenPatterns/ForbiddenTests.cs`); uygulama ve test kodunda yok. Koşuların hepsinde atlanan test 0.

**Doküman uyumu:** belge–kod eşitliği F2 task'larının Consistency testleriyle (207) mekanik. Kalan doküman uçları tek geçişe bağlandı (D-159: D-21 öneri, D-44, D-45, D-51, D-55, D-56, D-58, D-63, D-86, D-90 K3, D-120, D-138, D-158 (a)).

**`11 §6.2` F2 ek kontrolü:** `Modules/Ordering`'e geçici public sınıf ve Catalog iç tipine erişim eklendi → mimari testlerden üçü kırmızı (`ModuleReferences_InApplicationAssembly_OnlyGoThroughContracts`, `ModuleTypes_OutsideContracts_AreNotPublic`, `CachePattern_…`); ihlal kaldırılınca Architecture 763/763. Kayıt maskeleme testleri (`PathMaskerTests`, `ExceptionMaskerTests`, Logging integration 63) yeşil.

### Faz Satırları (`12 §6.2`)
| Satır (`VAL-…`) | Severity | Kanıt | Önceki `Durum` | Yeni `Durum` | Backlog kaydı (KABUL EDİLMİŞ RİSK ise) |
|---|---|---|---|---|---|
| — (F2 hücresi "D — kayıt maskeleme"; seviye D'de maskeleme satırı yok, `VAL-D004` kart verisidir) | — | `11 §6.2` F2: maskeleme testleri yeşil (yukarıda); `VAL-A991` aşağıda | — | — | — (doküman boşluğu D-44, F2 için genişletildi; D-159) |
| `VAL-A991` (T225; F2'de `PASS` alan tek satır, gate'te yeniden doğrulandı) | KRİTİK | {{A991}} | PASS | PASS (değişmedi) | — |

`12`'nin 1.503 satırından `PASS` olanlar `VAL-A838` (F1) ve `VAL-A991` (F2); F2 gate'inde `Durum`'u değişen satır yok. Maskelemeyi anan öteki satır `VAL-A960` (KRİTİK, `BEKLEMEDE`) beklenmeyen hatanın uçtan uca kaydıdır, ön koşulu oturumu açık müşteri ve form (F3); F2 hücresi ona bağlı değildir.

### Final Çıkış (`12 §6.3`) — yalnız son faz (F6)
Uygulanamaz (F2).

### Güvenlik Özeti
- **Açık bulgu:** 0 (bloke-etmeyen güvenlik notları backlog'da: D-89, D-90, D-92, D-93, D-108, D-144, D-147).
- **Yeni dış bağımlılıklar (`phase/F1-pass..f4da03c`):** NuGet `Serilog` 4.3.1, `Serilog.AspNetCore` 10.0.0, `Serilog.Sinks.File` 7.0.0 (T230; Apache-2.0), `SkiaSharp` 4.154.0 ve `SkiaSharp.NativeAssets.Linux.NoDependencies` 4.154.0 (T223; MIT). Veritabanı imajına kayıt ayarı dosyası (T225). NuGetAudit temiz.
- **Auth değişiklikleri:** kimlik şemaları ve erişim reddi (T207: panel ve hesap klasör kuralı, açık sayfa listesi `07 §2.3.1` ile eşit), tek kullanımlık değerler, sahteciliğe karşı koruma ve güvenlik başlıkları (T209), hız sınırı ve deneme limitleri (T215), imzalı makine ucu ve imza denetimi (T203), veri koruma halkası (T232), anahtarlar ve tek kullanımlık bağlantılar (T222), çerez politikası (T224). Kullanıcı girişi akışı yok (F3).
- **Gömülü sır taraması:** temiz (F2 diff'inde özel anahtar, bulut anahtarı ya da değerli parola yok; eşleşmeler sır adlarıdır).
- **Açık PR:** yok.

### Bulgular (FAIL durumunda)
Yok.

### Bloke-etmeyen Bulgular
| # | Seviye | Açıklama | Durum |
|---|---|---|---|
| 1 | DÜŞÜK | `12 §6.2` F2 hücresi bir `VAL-…` kimliğine bağlı değil (F0, F1 gibi) | D-44 genişletildi → D-159 |
| 2 | DÜŞÜK | Rapor tutarlılığı: sekiz başlık `Durum`'u, iki ters ve iki eksik post-merge satırı, T230 PR no, on status Commit hücresi | Bu PR'da kapandı |
| 3 | DÜŞÜK | Kestrel açan dört test sınıfı varsayılan 5000 portunda dinliyordu (paralel koşuda rastgele kırmızı) | Bu PR'da kapandı (D-128) |
| 4 | DÜŞÜK | Test fikstürünün yönetici komutu yük altında 30 sn'yi aşıyordu | Bu PR'da kapandı (D-68) |
| 5 | DÜŞÜK | Integration her koşuda göç paketini yeniden kuruyordu | Bu PR'da kapandı (D-157) |
| 6 | ORTA | Uygulama imajı SkiaSharp'ın kullanılmayan Windows ve macOS yerel kütüphanelerini PDB'leriyle taşıyor (467 MB; imaj 277 → 781 MB) | D-160 |
| 7 | DÜŞÜK | F1 gate'in "en geç F2 gate" üst sınırı koyduğu on üç doküman kalemi kapanmadı (`06`, `09` 2026-11-02'den beri değişmedi) | D-159 (tek doküman geçişi, en geç F3 gate) |
{{EXTRA_FINDINGS}}

### Öğrenim Terfileri
| # | Öğrenim | Sınıf | Hedef katman | Uygulanan değişiklik |
|---|---|---|---|---|
| 1 | Yerel git bekçisi ortam değişkeniyle (korunan dal listesi), `-c core.hooksPath` ya da `--no-verify` ile atlanabiliyordu | İhlali önleyen kural | L3 kullanıcı düzeyi kabuk bekçisi (repo dışı) | Kabuk bekçisi bu yolları engelliyor (2026-11-06); PF-185 (PF-181'in kararı) |
| 2 | Yapım işçisi açılmadan önce yerel ana dal uzaktakine ileri alınmalı (eski ana dal bekçiyi yanıltıyor) | Tekrarlanacak desen | Yardımcı düzeni (repo dışı) | PF-186 |
| 3 | PR'ın baş commit'i dalın ucunu göstermiyorsa PR kapatılıp yeniden açılmalı | Tekrarlanacak desen | Yardımcı düzeni (repo dışı) | PF-187 |
| 4 | Ajanların okuduğu durum JSON dosyaları BOM'suz yazılmalı | Tekrarlanacak desen | Yardımcı düzeni (repo dışı) | PF-188 |
| 5 | Uzun test ve biçim komutları araç sınırını aşınca arka plana düşüyor; koşular baştan parçalanmalı, test altyapısı pahalı kurulumu tekrarlamamalı | İhlali önleyen kural | L5 test altyapısı | Göç paketi önbelleği (D-157); PF-189 |
| 6 | Gerçek Kestrel açan test sabit portta dinlerse paralel sınıflar çarpışır | İhlali önleyen kural | L5 test kodu | Dört sınıf `UseKestrel(0)` (D-128); kural cümlesi `09` test bölümüne D-159 geçişinde; PF-190 |
| 7 | F1 gate'in doküman kalemlerine koyduğu "en geç F2 gate" üst sınırı tek başına işe yaramadı: hiçbir F2 task'ı `06`/`09` doküman geçişini üstlenmedi | Tekrarlanacak desen | L2 `DEFERRED_BACKLOG.md` | Kalemler tek `task` kalemine (D-159) bağlandı; gate yeniden hedeflemek yerine ayrı bir `docs:` PR'ı açar |
| 8 | Task boyutu (`11 §6.1` madde 4), üçüncü ölçüm: 33 task, doğrulama turu toplamı 49 (T217, T218, T221, T225 üçer; T203, T211, T215, T223, T224, T230, T232, T233 ikişer) | Tek seferlik gözlem (ölçüm) | — | Eşik değişmedi; yardımcıya — sahip yerine karar |

### Açık Risk Envanteri (`12 §6.2`, §8.4)
| Backlog ID | Madde (`VAL-…`) | Severity | Kabul tarihi | Matris `Durum` | Proje sahibi gördü ve kabul etti |
|---|---|---|---|---|---|
| — | — | — | — | — | Envanter boş: "KABUL EDİLMİŞ RİSK" kaydı 0, matriste `KABUL EDİLMİŞ RİSK` hücresi 0 |

### Backlog Hijyeni
| ID | Önceki durum | Yeni durum | Gerekçe |
|---|---|---|---|
| D-32 | kalan pay T230 | ✅ | Uygulama kaydı klasörü Compose ve imajla eşit (`LoggingConsistencyTests`) |
| D-60 | en geç F2 gate | ✅ | README "Bilinen sınır"ına dört biçim |
| D-68 | en geç F2 gate | ✅ | Yönetici komutuna 120 sn; Database 36/36 |
| D-85 | en geç F2 gate | ✅ | Altı özet değeri yazmıyor |
| D-123 | (b) en geç F2 gate | ✅ | Desen `data-val(?![a-z0-9_])`, ihlal örneğine sessiz satırlar |
| D-128 | F2 gate | ✅ | Neden bulundu ve düzeltildi (sabit port) |
| D-157 | F2 gate | ✅ | Göç paketi önbelleği |
| D-21, D-44, D-45, D-51, D-55, D-56, D-58, D-63, D-86, D-120 | en geç F2 gate | D-159, en geç F3 gate | Doküman geçişi F2'de yapılmadı; tek kalemde toplandı |
| D-27 | T230 | T601, en geç F6 gate | T230 satırı belgeye yazmadı; {{D27}} |
| D-70 | en geç F2 gate | en geç T538 | Kod düzeyinde durum aynı |
| D-71, D-91 (K2), D-92, D-93 | en geç F2 gate | T601 | Reçete ve Caddyfile'a F2'de dokunulmadı |
| D-80, D-89, D-90, D-96, D-108, D-126, D-144, D-147, D-150, D-151, D-152, D-158 (b) | en geç F2 gate | en geç F3 gate (adlı task'larla) | Bugün `src/`'de örneği yok ya da sahibi F3 task'ı; D-89 Roslyn kararı, D-90 K3 kararı, D-108 tavan kararı yardımcıya |
| D-81 | en geç F2 gate | T343, T352, T389, T515; en geç F3 gate | Tüketici F3'te |
| D-138, D-141, D-146 | en geç F2 gate | T537, T541, T503; en geç F5 gate | Sahip task'ları F5'te |
| D-159 | — | ⚪ yeni | `06`/`09`/`12` doküman geçişi |
| D-160 | — | ⚪ yeni | İmajdaki kullanılmayan yerel kütüphaneler |

Hâlâ açık ve hedefi geçerli öteki kalemler F2 task'larına bağlı değil (hedefleri F3–F6 task'ları ya da T603). D-99'un T201 payı T201'de kapandı, kalanları F3 task'larında. Aktif kalem 98 → 93 (yedi kapandı, iki açıldı).

**Yardımcıya — sahip yerine karar:** (1) D-89: Roslyn tabanlı çözümleyiciye geçilmez, metin taraması dizge farkında yeniden yazılır; (2) D-90 K3: `SaveChangesFailed` süzgece üçüncü olay olarak girer; (3) D-108: seyreltme belleğine tavan F2'de konmaz, T376'da karar kaydıyla; (4) D-51/D-55: (a) seçeneği (çatının adı kabul) önerilir; (5) D-120: kod kalır, iki belge cümlesi koda hizalanır; (6) D-138: boşluk T537'de; kapanmazsa istisna `05 §10.3.6`'ya yazılır; (7) task boyutu eşiği değişmez; (8) açık risk envanteri boş, kabul edilecek kalem yok.

### Faz Tag
- Tag: `phase/F2-pass`   - Commit: gate PR'ının squash commit'i (merge ve etiket yardımcıda, PASS ritüeli 20. adım)
