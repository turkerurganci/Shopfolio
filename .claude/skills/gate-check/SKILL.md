---
name: gate-check
description: "Bir fazın tüm task'ları bittikten sonra faz kapanışını doğrular — test, build, ortam, migration, öğrenim terfisi, backlog hijyeni. Kullan: 'FX gate check', 'faz kontrolü', /gate-check FX. Ayrı chat'te."
user-invocable: true
---

# Gate Check — Faz Sonu Doğrulama

> **Ne zaman:** Bir fazın tüm task'ları tamamlandıktan sonra, sonraki faza geçiş onayı için.
> **Tetikleme:** "FX gate check", "faz kontrolü" veya `/gate-check FX`.
> **Parametre:** `hedef` — faz numarası (örn. `F0`, `F1`)
> **Kural:** **Ayrı chat**'te çalıştırılır. Gate check PASS vermeden sonraki faza geçilmez.

---

## Ön kontrol

1. **Faz tamamlanmış mı?** `Docs/IMPLEMENTATION_STATUS.md`'de fazın tüm task'ları `✓ Tamamlandı` mı? `⛔ BLOCKED` veya `✗ FAIL` varsa gate başlatılmaz.
2. **Rapor tutarlılığı:** Her task için rapor mevcut ve finalize mi? Status tablosu ile rapor verdict'leri eşleşiyor mu? PR numaraları birebir mi?
3. **Working tree temiz mi?** Dirty ise karar al (task/validate Adım -1 ile aynı kural).
4. **Ana dal CI yeşil mi?** Son 3 run + HEAD commit'in tüm workflow'ları.
5. **Ortam bilgisi kaydedilir** — çalıştırma ortamının sürümleri (runtime, container motoru, paket yöneticisi). Gate raporu tekrar edilebilir olmalı.

---

## Adım 1 — Regresyon testi

6. **Mevcut fazın testleri** — tüm test katmanları. Sonuçları kaydet (sayı, süre, başarısız varsa detay).

7. **Önceki fazların testleri** (ilk fazdan sonra) — tekrar çalıştırılır. Kırılan test = **S2 Kırılma** → gate FAIL. Kayıtlı `Durum` yeterli değildir: `12`'nin kanıtı Test raporu olmayan A–E satırlarından yeni kodun etkileyebilecekleri kanıt yeniden üretilerek doğrulanır (`12 §6.2` "Regresyon kontrolü", §11 "Faz geçişi"; şüphede yeniden doğrulanır, §8.3). Daha önce PASS almış satırın FAIL'i **S2 Kırılma**dır, satır severity'sini ayrıca alır (`12 §11.1` madde 2) → gate FAIL.

> **Test altyapısı artefaktı ≠ regresyon.** Paralel koşumda kaynak açlığından düşen testler *rasyonelize edilmez*, **temiz yeniden koşumla** ayrıştırılır: (a) seri/izole koşumda geçiyor mu, (b) aynı commit'in CI run'ı yeşil mi. Her iki kanıt da yoksa bu bir regresyondur.

---

## Adım 2 — Build ve ortam

8. **Temiz build** — tüm bileşenler.

9. **Fresh environment smoke test** — tüm servisleri sıfırdan ayağa kaldır, health check'leri doğrula, sonra temizle. Servislerin **hepsi** ayağa kalkmalı.

   **⚠ Hangi konfigürasyonla?** Bu adım **test/E2E konfigürasyonuyla** tatmin edilebilir — ve genellikle öyle olur, çünkü o konfigürasyon zaten çalışır durumdadır. Rapora **hangi konfigürasyonun kullanıldığı açıkça yazılır**.

   - Ara fazlarda test konfigürasyonu **kabul edilebilir**.
   - **Son fazda (kapanış) gerçek dağıtım konfigürasyonu zorunludur** — bkz. `00 §L.1` boot provası. Bir referans projede son gate, E2E harness'ının kendi konfigürasyonuyla ✓ PASS aldı; gerçek dağıtım konfigürasyonu ilk kez kapanıştan sonra denendi ve **hiç açılmadı**.

---

## Adım 3 — Migration rehearsal (veri katmanı fazından itibaren)

10. Temiz veritabanı üzerinde:
    - Model doğrulama (bekleyen model değişikliği uyarısı var mı?)
    - İlk uygulama — migration zinciri hatasız mı?
    - **İdempotency** — ikinci kez çalıştır, no-op olmalı
    - Seed verisi yükleniyor mu, sayılar bekleneni veriyor mu?

---

## Adım 4 — Traceability ve boşluk taraması

11. **Traceability matrix kontrolü** — `Docs/11_IMPLEMENTATION_PLAN.md` §7'deki matriste bu fazda implement edilmesi gereken her kaynak öğe implement edilmiş mi? Eşlenip implement edilmeyen öğe = **S3 Eksik**.

12. **Vacuous test taraması** — `skip` / `fixme` / `only` / `expect(true)` / boş assertion araması. Geçen ama hiçbir şey doğrulamayan test, boşluğun gizlenmiş hâlidir.

13. **Doküman uyumu** — enum değerleri, API sözleşmeleri, entity alanları referans dokümanlarla tutarlı mı?

   **Faz satırları (`12 §6.2`, `11 §6.2`):** fazın `11 §6.2` ek kontrolleri ve `12 §6.2` tablosunda bu faz için sayılan `12` satırları okunur, kanıtları üretilir ve gate raporunun "Faz Satırları" tablosuna satır kimliğiyle yazılır. Satır `PASS` ya da kaydı bulunan `KABUL EDİLMİŞ RİSK` (`grep -n 'VAL-…' Docs/DEFERRED_BACKLOG.md`; `12 §8.4`) değilse — `BEKLEMEDE` dahil — verdict FAIL'dir. Satırların `Durum` hücresi gate raporuyla aynı commit'te güncellenir (`12 §7.5`).

   **Son fazın gate'i (F6) — final çıkış (`12 §6.3`):** yukarıdaki satırlara ek olarak `12 §6.3` tablosunun her kriteri satır kimlikleriyle işaretlenir ve gate raporunun "Final Çıkış" tablosuna yazılır: tüm KRİTİK satırlar `PASS` (kimliklerle sayım) · ORTA satırlar `PASS` ya da kaydı bulunan `KABUL EDİLMİŞ RİSK` · `BEKLEMEDE` kalan DÜŞÜK satırlar kimliğiyle listelenir (istisna yalnız F6 gate'inde okunmayan satır içindir; "E `gerçek`" satırları dahil gate'te okunan satır istisna dışıdır) · çekirdek entegrasyon: Kanıt ortamı `sandbox`/`gerçek` olan E satırları `mock` ile `PASS` sayılmaz; K-23 DV satırları (DV-6, DV-7, DV-10; DV-5 koşullu) yalnız `gerçek` · seviye F satırları ve `[yük]` iki satırı · kabul kanıtı (`VP12-01` senaryoları). Final çıkış yedi gate'in `PASS`'ini varsayar (`12 §5`); biri eksikse bu tablo doldurulmaz, verdict FAIL'dir.

---

## Adım 5 — Güvenlik özeti

14. Bu fazdaki tüm task'ların mini güvenlik kontrollerini derle:
    - Açık kalan güvenlik bulgusu var mı?
    - **Yeni dış bağımlılıklar** — faz aralığındaki manifest diff'i (`git diff <önceki-tag>..HEAD -- <manifest dosyaları>`)
    - Auth/authorization değişiklikleri özeti
    - Kod içine gömülmüş secret taraması

---

## Adım 6 — Verdict

15. **Verdict:**
    - **PASS** — testler geçiyor, build temiz, ortam ayağa kalkıyor, migration temiz, traceability boşluğu yok, S2 yok, fazın `12` satırları ve `11 §6.2` ek kontrolleri karşılanmış (Adım 4), açık risk envanteri proje sahibince görülmüş ve kabul edilmiş (Adım 8; `12 §6.2`)
    - **FAIL** — en az bir kritik bulgu (kırık test, build hatası, traceability boşluğu, S2, `PASS` ya da kaydı bulunan `KABUL EDİLMİŞ RİSK` olmayan faz satırı, kaydı olmayan `KABUL EDİLMİŞ RİSK` hücresi)

16. **Bloke-etmeyen bulgular** ayrı tabloda listelenir. Her biri ya bu gate'in chore PR'ında kapatılır ya `Docs/DEFERRED_BACKLOG.md`'ye forward edilir. **Kaydı olmayan bulgu yoktur.** Bloke-etmeyen bulgu yalnız `12` satırına bağlı olmayan gözlemdir: FAIL alan satır severity'sine bakılmadan bloke eder ve backlog'a yalnız proje sahibinin o risk için onayıyla `KABUL EDİLMİŞ RİSK` olarak yazılır (KRİTİK olamaz; `12 §8.4`); düzeltme bu fazda yapılır (`12 §8.2`).

---

## Adım 7 — Öğrenim terfisi (00 §K)

17. **Bir faz, öğrenimi yazılmadan kapanmaz.** Bu adım atlanamaz.

    Her öğrenim için:
    1. Ne beklenmedik şekilde çalıştı / çalışmadı?
    2. Sınıfla:
       - **Tek seferlik gözlem** → yalnız gate raporunda kalır
       - **Tekrarlanacak desen** → `Docs/00_PROJECT_METHODOLOGY.md` §N'e yazılır
       - **İhlali önleyen kural** → **terfi eder**: hedef katman (L1–L5) + hedef dosya belirtilir ve **aynı chore PR'ında uygulanır**
    3. Gate raporuna "Öğrenim terfileri" tablosu:

    | # | Öğrenim | Sınıf | Hedef katman | Uygulanan değişiklik |
    |---|---|---|---|---|

    **Terfi kaydı boşsa gerekçesi yazılır** ("bu fazda kurala dönüşecek öğrenim çıkmadı"). Boş bırakılmış bir terfi tablosu, kapanmamış bir gate demektir.

    **Playbook'a geri akış (K-649):** sonucu playbook'tan gelen bir dosyaya dokunan — ya da dokunması gereken — her öğrenim `Docs/PLAYBOOK_FEEDBACK.md`'ye satır olarak da girer. Liste proje tamamlandıktan sonra proje sahibince gönderilir.

---

## Adım 8 — Backlog hijyeni

18. `Docs/DEFERRED_BACKLOG.md`'deki açık kalemleri **kod düzeyinde** yeniden doğrula:
    - Araya giren işlerle çözülmüş olanlar → `✅ Çözüldü → <nerede>`; kayıt kabul edilmiş riskse ("KABUL EDİLMİŞ RİSK — `VAL-…`") satır bu gate'te yeniden doğrulanır ve `Durum` hücresi `PASS` olur (`12 §8.4`)
    - Stale olanlar (açıklama artık gerçeği yansıtmıyor) → düzelt/daralt
    - Hâlâ açık olanlar → öncelik ve "neyi bloklar" alanını güncelle
    - **Açık risk envanteri (`12 §6.2`, §8.4, §10):** açıklaması "KABUL EDİLMİŞ RİSK — `VAL-…`" diye başlayan her kalem listelenir; matriste `Durum` hücresi `KABUL EDİLMİŞ RİSK` olan her satırın kaydı `grep -n 'VAL-…' Docs/DEFERRED_BACKLOG.md` ile bulunur (kaydı olmayan hücre `FAIL`'dir → verdict FAIL), kaydın kabul tarihi ve proje sahibinin o risk için onayı okunur; envanter proje sahibine gösterilir ve kabulü gate raporuna yazılır. Onay gate-check'in kararı değildir.

---

## PASS ritüeli

19. Tek bir `chore:` PR'ında:
    - Gate raporu → `Docs/CHECKPOINT_REPORTS/GATE_CHECK_FX.md`
    - `Docs/IMPLEMENTATION_STATUS.md` faz satırı `✓ PASS`
    - Repo memory: faz özet satırı (tarih, PR no, squash hash, tag, ana metrikler) + "sıradaki" satırı + tarih başlığı
    - Backlog forward'ları ve hijyen düzeltmeleri
    - Gate'te okunan ya da yeniden doğrulanan `12` satırlarının `Durum` hücreleri (`12 §7.5`, `§11.1` madde 1 ve 4, Kanıt ortamı hücresi üst ortamı yazan satırda düşük ortam `PASS`'inin `BEKLEMEDE`'ye dönüşü dahil; `12`'nin sürümünü artırmaz)
    - Öğrenim terfileri

20. PR merge + CI yeşil + **tüm workflow'lar** teyit edildikten sonra ana dal üzerinde tag:
    ```bash
    git tag phase/FX-pass && git push origin phase/FX-pass
    ```

## FAIL durumunda

- Bulguları S1/S2/S3 sınıflamasıyla listele, etkilenen task'ları belirt, düzeltme planı öner.
- Düzeltmeler **bu fazda** yapılır, sonraki faza ertelenmez.
- Düzeltmeler tamamlandıktan sonra gate check tekrar çalıştırılır.

---

## Çıktı formatı

```
## Gate Check Sonucu — FX [Faz Adı]
**Tarih:** YYYY-AA-GG   **Task aralığı:** TXX–TYY   **Toplam task:** N
**Base tag:** phase/F(X-1)-pass → main HEAD <hash>

### Verdict: ✓ PASS / ✗ FAIL

### Ön Kontrol
[task durumları, rapor tutarlılığı, working tree, ana dal CI, ortam sürümleri]

### Test Sonuçları
| Katman | Tür | Sonuç | Detay |
|---|---|---|---|

### Build
| Bileşen | Sonuç | Detay |
|---|---|---|

### Fresh Environment
| Servis | Durum |
|---|---|

### Migration
| Adım | Sonuç |
|---|---|
| Model doğrulama | |
| İlk apply | |
| İdempotency (2. apply) | |
| Seed | |

### Traceability
| Kategori | Eşlenen | Implement | Boşluk (S3) | Kanıt |
|---|---|---|---|---|

### Faz Satırları (`12 §6.2`)
| Satır (`VAL-…`) | Severity | Kanıt | Önceki `Durum` | Yeni `Durum` | Backlog kaydı (KABUL EDİLMİŞ RİSK ise) |
|---|---|---|---|---|---|

### Final Çıkış (`12 §6.3`) — yalnız son faz (F6)
| Kriter (`12 §6.3`) | Satır kimlikleri / sayım | Sonuç | Kanıt |
|---|---|---|---|

### Güvenlik Özeti
- Açık bulgu: N
- Yeni dış bağımlılıklar: [liste veya yok]
- Auth değişiklikleri: [özet]

### Bulgular (FAIL durumunda)
| # | Seviye | Açıklama | Etkilenen task | Düzeltme önerisi |
|---|---|---|---|---|

### Bloke-etmeyen Bulgular
| # | Seviye | Açıklama | Durum |
|---|---|---|---|

### Öğrenim Terfileri
| # | Öğrenim | Sınıf | Hedef katman | Uygulanan değişiklik |
|---|---|---|---|---|

### Açık Risk Envanteri (`12 §6.2`, §8.4)
| Backlog ID | Madde (`VAL-…`) | Severity | Kabul tarihi | Matris `Durum` | Proje sahibi gördü ve kabul etti |
|---|---|---|---|---|---|

### Backlog Hijyeni
| ID | Önceki durum | Yeni durum | Gerekçe |
|---|---|---|---|

### Faz Tag
- Tag: `phase/FX-pass`   - Commit: <hash>
```
