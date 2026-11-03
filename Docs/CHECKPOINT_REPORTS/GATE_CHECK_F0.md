## Gate Check Sonucu — F0 Proje iskeleti
**Tarih:** 2026-11-03   **Task aralığı:** T01–T10   **Toplam task:** 10
**Base tag:** yok (ilk faz; aralık `8094071..93ba91f`) → main HEAD `93ba91f`

### Verdict: ✓ PASS

Testler bütün katmanlarda yeşil, build temiz, üç imaj derleniyor, taze ortam yerel referans kurulumda sıfırdan ayağa kalkıyor ve canlılık ucu 200 veriyor, Lint eşitlik testi kırmızıdan yeşile gösterildi, F0'a bağlı 66 matris satırı implement edilmiş ve kanıtlı (S3 yok), S2 yok. `12 §6.2`'nin F0 satırı bir `12` satır kimliğine bağlı değil; gate'te okunacak `VAL-…` satırı yok (D-44). Açık risk envanteri boş.

### Ön Kontrol
- **Task durumları:** T01–T10 hepsi `✓` (`IMPLEMENTATION_STATUS.md`); BLOCKED ya da FAIL yok.
- **Rapor tutarlılığı:** on rapor var ve PASS. Bu PR'da düzeltilenler: T07 raporunun Durum satırı "✗ tur 3 FAIL" kalmıştı (tur 4 PASS); T02 ve T09 raporunda PR numarası yoktu (#335, #354); on raporun "Post-merge run'lar" satırı ve status'un Commit sütunu boştu — squash hash'leri ve ana dal run'ları yazıldı. Status ile rapor verdict'leri ve PR numaraları artık birebir.
- **Working tree:** temiz.
- **Ana dal CI:** HEAD `93ba91f` — CI 41696545511 (9 iş + CI Gate success; E2E ve Contract `NOT_APPLICABLE`) ve Publish 41696545899 success; son üç run yeşil. Her task'ın merge'ündeki iki workflow da success:

| Task | Squash | PR | CI | Publish |
|---|---|---|---|---|
| T02 | `f83876a` | #335 | 41489370654 | 41489370463 |
| T03 | `e4de649` | #337 | 41542556291 | 41542556242 |
| T04 | `20fdd0c` | #339 | 41552961812 | 41552961821 |
| T01 | `6c07304` | #338 | 41588356448 | 41588356390 |
| T06 | `aaf2f2a` | #347 | 41619412629 | 41619412883 |
| T05 | `5509c5d` | #348 | 41630669803 | 41630669794 |
| T08 | `ca42ca9` | #349 | 41649150934 | 41649150970 |
| T10 | `af4103c` | #350 | 41664366553 | 41664366613 |
| T07 | `6a8dc38` | #351 | 41673898932 | 41673898671 |
| T09 | `93ba91f` | #354 | 41696545511 | 41696545899 |

- **Ortam:** Windows 11 + Docker Desktop (bellek ~4 GB) · .NET SDK 10.0.400 · Docker 29.2.1 · Compose v5.1.0 · git 2.42 · gh 2.89 · Python 3.12.10.

### Test Sonuçları
| Katman | Tür | Sonuç | Detay |
|---|---|---|---|
| Lint | `restore --locked-mode` + `dotnet format --verify-no-changes` + Consistency | ✓ | Consistency 34/34 |
| Unit | xUnit v3 | ✓ | 68/68 (gate PR'ının son hâlinde Consistency dahil 102/102) |
| Integration | PostgreSQL `postgres:18.6` | ✓ | 62/62, 28 sn |
| Betik testleri | `tests/scripts/run-script-tests.sh` (Debian konteyneri) | ✓ | 4 dosya: 11 + 13 + 129 + 27 assertion |
| Göç modeli | `dotnet ef migrations has-pending-model-changes` | ✓ | değişiklik yok (F0'da göç yok) |
| E2E · Contract | — | N/A | `NOT_APPLICABLE` (E2E F6 T601, D-41) |
| Önceki fazlar | — | N/A | ilk faz |

**Lint kırmızı→yeşil (`11 §6.2` F0, `09 §21.4` madde 1):** `Directory.Packages.props`'a Kütüphaneler tablosunda olmayan bir paket eklendi → `LibraryTableConsistencyTests` FAIL (çıkış 2); geri alınınca 34/34.

### Build
| Bileşen | Sonuç | Detay |
|---|---|---|
| Çözüm (Release, uyarılar hata) | ✓ | 0 uyarı, 0 hata |
| NuGetAudit (`restore --force`) | ✓ | bilinen açık yok |
| Uygulama imajı (`--no-cache`) | ✓ | 43 sn; kullanıcı 1654 |
| Veritabanı imajı (`--no-cache`) | ✓ | 8,5 sn; kullanıcı 999 |
| Yedek aracı imajı (`--no-cache`) | ✓ | 5,8 sn; kullanıcı 10001 |

### Fresh Environment
Yapılandırma: **yerel referans kurulum** (`scripts/local-reference/`, `DEPLOY_RUNBOOK §K`; Docker içinde ayrıcalıklı Linux konteyneri, MinIO, Caddy iç sertifikası, sahte sırlar). Reçete sıfırdan koşuldu (2026-11-03 18:55–18:59Z): `host-up`, `storage-setup` (All checks passed), `install-installation`. Gerçek sunucuya ilk iniş T603'tedir (`11 §6.2` F0; D-40).

| Servis | Durum |
|---|---|
| `proxy` (Caddy) | up |
| `app` | up — konteyner içi canlılık 200; `https://shopfolio.internal/sistem/canlilik` → 200 |
| `database` | up |
| `backup` | up |
| `migrate` (tek seferlik) | Exited (0) |
| Denetimler | secrets, bootstrap, image-ids ✓ · isolation 7 ok · egress 6 ok · redirect-hsts 3 ok |

Temizlik: `down.sh all`, gate'in imajları ve geçici PostgreSQL silindi; `t09-*` kalıntısı yok; `skinora-*` konteynerlerine dokunulmadı.

### Migration
F0'ın göç adımı yok (veri katmanı F1'de; skill Adım 3 "veri katmanı fazından itibaren").

| Adım | Sonuç |
|---|---|
| Model doğrulama | ✓ `has-pending-model-changes` değişiklik yok |
| İlk apply | N/A — göç demeti sıfır göç taşır; taze ortamda `migrate` çıkış 0 |
| İdempotency (2. apply) | N/A |
| Seed | N/A |

### Traceability
`11 §7.1`'de F0 task'larına bağlı 66 satır (21'i yalnız F0, 45'i sonraki fazlarla ortak). Her satırın F0 payı kabul kriteri, test ya da kayıtla eşlendi; "Implement edildi" ve "Kanıt" hücreleri bu PR'da dolduruldu (`11 §6.1` madde 3). Kapsama betiği çıkış 0.

| Kategori | Eşlenen | Implement | Boşluk (S3) | Kanıt |
|---|---|---|---|---|
| `05` Teknik Mimari (MK ve alt bölüm) | 45 | 45 | 0 | `11 §7.1.n` hücreleri |
| `09` Kodlama Kuralları | 9 | 9 | 0 | aynı |
| `devir` (K-1355, K-1356, K-1358, P8.2–P8.4, D-01) | 7 | 7 | 0 | aynı |
| `10` MVP Kapsamı (KP-75, §4.1) | 2 | 2 | 0 | aynı |
| `02`, `06`, `07` (§3.34.8, §2.8, UC-81) | 3 | 3 | 0 | aynı |
| **Toplam** | **66** | **66** | **0** | |

`05:MK-60` (döndürme) T08 KK5 ile implement edilmiştir (`SecretRunbookConsistencyTests.Runbook_RotatesEverySecretExceptTheTwoBackupCipherKeys`); döndürme adımlarının ayrıntısı (D-35 (d)) runbook ayrıntısıdır ve gerçek sunucu ister → T603.

**Vacuous test taraması:** `skip`, `only`, sabit doğru, boş assertion yok; eşleşmeler yalnız derlemeye girmeyen `ViolationSamples/` örnekleri.

**Doküman uyumu:** kod ile `05`, `09` arasındaki açık uçlar backlog'da kayıtlı (D-10, D-20, D-22, D-25, D-26, D-32, D-35 (b) `05` payı); bu gate'te `09`'un dört drift kalemi kapandı (D-07, D-11, D-18, D-23).

### Faz Satırları (`12 §6.2`)
| Satır (`VAL-…`) | Severity | Kanıt | Önceki `Durum` | Yeni `Durum` | Backlog kaydı (KABUL EDİLMİŞ RİSK ise) |
|---|---|---|---|---|---|
| — (F0 hücresi "F — boot provasının 1. turu, canlılık ucu"; seviye F'de buna karşılık gelen satır yok) | — | `11 §6.2` F0: yerel referans kurulumda canlılık 200 (T09 ve bu gate'in taze ortamı), üç imaj, Lint kırmızı→yeşil | — | — | — (doküman boşluğu D-44) |

`12`'nin 1.503 satırının hepsi `BEKLEMEDE`; F0 task'larına bağlı tek satır `VAL-F016`'dır (kaynağı `11 T07 (7)`), ortamı `gerçek`tir ve koşumu T601'dedir — F0 gate'inde okunmaz, `Durum` hücresi değişmedi.

### Final Çıkış (`12 §6.3`) — yalnız son faz (F6)
Uygulanamaz (F0).

### Güvenlik Özeti
- **Açık bulgu:** 0.
- **Yeni dış bağımlılıklar (`8094071..93ba91f`):** NuGet 8 — `EFCore.NamingConventions` 10.0.1, `Microsoft.AspNetCore.Mvc.Testing` 10.0.11, `Microsoft.EntityFrameworkCore` 10.0.11, `Microsoft.EntityFrameworkCore.Design` 10.0.11, `Npgsql` 10.0.3, `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.3, `TngTech.ArchUnitNET` 0.13.3, `xunit.v3` 3.2.2 · yerel araç `dotnet-ef` 10.0.11 · taban imajlar etiket+özetle sabit: `dotnet/sdk:10.0.400-noble`, `dotnet/aspnet:10.0.14-noble`, `postgres:18.6-trixie`, `alpine:3.24.3` · restic 0.19.1 (SHA-256 sabit) · pgBackRest (Debian paketi; sürüm sabit değil — bilinçli istisna, `09 §17` madde 6, D-28). NuGetAudit temiz.
- **Auth değişiklikleri:** kullanıcı kimlik doğrulaması yok (F2/F3). Altyapı: sırlar bileşen başına dosyada, ortam değişkeninde değil (T08); veritabanının dört kullanıcısı tehlikeli öznitelik taşımaz, `PUBLIC` yetkisi alındı, uygulama kullanıcısı şema değiştiremez (T10); ters vekil yalnız 80/443, HSTS bir yıl (T05).
- **Gömülü sır taraması:** temiz (eşleşmeler yalnız test ve uydurma değerler).
- **Açık Dependabot PR'ları:** #341–#346, #355; #344 ve #345 Lint'te `NU1004` ile kırmızı → D-43.

### Bulgular (FAIL durumunda)
Yok.

### Bloke-etmeyen Bulgular
| # | Seviye | Açıklama | Durum |
|---|---|---|---|
| 1 | DÜŞÜK | `12 §6.2` F0 satırı bir `VAL-…` kimliğine bağlı değil | D-44 |
| 2 | DÜŞÜK | Açık Dependabot PR'ları kilit dosyası yüzünden kırmızı ya da bekliyor | D-43 |
| 3 | DÜŞÜK | Rapor tutarlılığı: T07 Durum satırı, T02/T09 PR no, on raporun post-merge satırı, status Commit sütunu | Bu PR'da kapandı |
| 4 | DÜŞÜK | Matris kapsama betiği gate dolumunu reddediyordu | Bu PR'da kapandı (öğrenim 1) |
| 5 | DÜŞÜK | `DEPLOY_RUNBOOK §B` satır 1 döndürme adımlarını "T09 provasında" der (T09 yerel oldu) | Bu PR'da T603'e çevrildi; D-35 (d) |
| 6 | DÜŞÜK | `DEFERRED_BACKLOG.md` tablosunun ortasında boş satır (D-33…D-42 tablodan kopuyordu) | Bu PR'da kapandı |

### Öğrenim Terfileri
| # | Öğrenim | Sınıf | Hedef katman | Uygulanan değişiklik |
|---|---|---|---|---|
| 1 | Kapsama betiği "Implement edildi" ve "Kanıt" hücrelerini boş şart koşuyordu; `11 §6.1` madde 3'ün gate dolumu betiği kırıyordu | İhlali önleyen kural | L5 betik · L2 `11 §7` · L4 `skills/gate-check` | `11_matris_kapsama.py` `GATE_DOLU` (gate'i geçmiş faza bağlı satır dolu, öteki boş; kırmızı→yeşil gösterildi: boşaltılan F0 hücresi ve doldurulan F2 hücresi çıkış 1); `11 §7` satır sütunları kuralı; gate-check 19. adım; PF-173 |
| 2 | Yardımcı düzeninde merge sonrası run ID'leri ve squash hash'i on task'ın hiçbirinde rapora ve status'a yazılmadı | İhlali önleyen kural | L4 `skills/task` | 15. adıma "önceki task'ların boş merge kaydı bu dalda doldurulur"; PF-174 |
| 3 | Dependabot yalnız doğrudan kullanan projenin kilidini günceller; test projeleri `NU1004` ile düşer | Tekrarlanacak desen | L2 `09 §3.2` | gözlem ve yol `09 §3.2`'ye yazıldı (D-07 kapandı); açık PR'lar D-43 |
| 4 | Task boyutu (`11 §6.1` madde 4, K-1409 (3)): on task'ın 7'si devir dosyasıyla (bağlam bölünmesi) bitti; doğrulama turu toplamı 27 (T01, T07, T08 dörder); merge araları işlenen belirteç (M): T02 41,9 · T03 46,9 · T04 84,6 · T01 38,4 · T06 62,0 · T05 65,4 · T08 100,9 · T10 124,6 · T07 49,4 · T09 93,9 (+ #336 65,0, #352 8,4, #353 20,0) | Tek seferlik gözlem (ölçüm) | — | Eşik değişmedi; ayar F1 gate'inin ikinci ölçümüyle birlikte (yardımcıya — sahip yerine karar) |
| 5 | Gerçek sunucu ön koşulu yokken ilk iniş provası yerel referans kuruluma çevrildi (PR #353, D-40) | Tek seferlik gözlem | — | Plan ve backlog'da kayıtlı |

### Açık Risk Envanteri (`12 §6.2`, §8.4)
| Backlog ID | Madde (`VAL-…`) | Severity | Kabul tarihi | Matris `Durum` | Proje sahibi gördü ve kabul etti |
|---|---|---|---|---|---|
| — | — | — | — | — | Envanter boş: "KABUL EDİLMİŞ RİSK" kaydı 0, matriste `KABUL EDİLMİŞ RİSK` hücresi 0 |

### Backlog Hijyeni
| ID | Önceki durum | Yeni durum | Gerekçe |
|---|---|---|---|
| D-07 | ⚪ açık | ✅ | İlk Dependabot PR'larında gözlendi; `09 §3.2`'ye yazıldı; açık PR'lar D-43 |
| D-11 | ⚪ açık | ✅ | `09`'un üç "bilgim tarihli" işareti kalktı |
| D-18 | ⚪ açık | ✅ | `.config/` `09 §4.3`'te |
| D-23 | ⚪ açık | ✅ | (a) `CONTEXT.md` §2, (b) `09 §21.4` m.2, (c) `CI_CD_SETUP.md` tarihi |
| D-19 | hedef T09 | hedef T603 | İlk sürüm etiketi T603'te (D-40) |
| D-21 | hedef "F0 gate" | sır bekçisine dokunan sonraki `hooks:` PR'ı, en geç F1 gate | Tasarım kararı (istisnayı daraltma mı, takma ad kuralı mı) yardımcıya |
| D-24 | hedef "T06 sonrası ilk yayın" | T603 | İlk etiket T603'te |
| D-25 | hedef "F0 gate" | en geç T212 | Yakalama katmanı T212 |
| D-27 | hedef "F0 gate" | T230 | Uygulama kayıtları task'ı; varsayılan: kütüphane eklenmez |
| D-32 | hedef T07, T08 (bitti) | (a) T226; yollar T223, T230 | Kalan uçların sahibi |
| D-33 | kurulum denemesi T09 | T603 | Gerçek sunucu |
| D-35 | (b) "F0 gate", (d) T09 | (b) `09` payı ✅, `05` payı en geç T603; (d) T603 | Runbook satır 1 güncellendi |
| D-36 | (a) "F0 gate" | (a) en geç T603 | Repo dosyalarında biçim yok; risk düşük |
| D-37 | (b) T10 CI | (b) ✅ (hedef hücresi hizalandı) | — |
| D-38 | (a) T07/T09, (b) T09 | (a) en geç T603, (b) T603 | Gerçek sunucuda ilk kurulum |
| D-39 | (b),(c) T09; (e),(f) "F0 gate" | (b),(c) D-40 (T603); (e) en geç T603; (f) T221 | Yerelde sınandı; gerçek gözlem T603 |
| D-43 | — | 🟡 yeni | Açık Dependabot PR'ları |
| D-44 | — | ⚪ yeni | `12 §6.2` F0 satırı kimliksiz |

Hâlâ açık ve hedefi geçerli: D-10, D-13, D-20, D-22, D-26, D-40, D-41, D-42. Aktif kalem 24 → 22.

**Yardımcıya — sahip yerine karar:** (1) task boyutu eşiği (öneri: değişmez, F1 ölçümüyle); (2) D-21'in tasarım kararı; (3) açık Dependabot PR'ları (D-43) F1'den önce birleştirilir ya da kapatılır; (4) açık risk envanteri boş — kabul edilecek kalem yok.

### Faz Tag
- Tag: `phase/F0-pass`   - Commit: gate PR'ının squash commit'i (merge ve etiket yardımcıda, PASS ritüeli 20. adım)
