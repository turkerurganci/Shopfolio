# TXX — <Task Adı>

**Faz:** FX | **Durum:** ⏳ Devam ediyor / ✓ Tamamlandı / ✗ FAIL | **Tarih:** YYYY-AA-GG

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- …

## Etkilenen Modüller / Dosyalar

- …

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | | ✓ / ✗ / ~ / ? | komut + çıktı + commit |

> `? Doğrulanamadı` **FAIL değildir** — kanıt eksikliğidir. FAIL'den ayrı tutulur.

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | | | |
| Integration | | | |
| Contract | | | |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS / ✗ FAIL / ⛔ BLOCKED |
| Bulgu sayısı | |
| Düzeltme gerekli mi | |
| Bloke-etmeyen not (K) sayısı | |

## Altyapı Değişiklikleri

- **Şema/migration:** Var / Yok — varsa açıklama
- **Config / ortam değişkeni:** Var / Yok
- **Paketleme / dağıtım:** Var / Yok
- **Yeni bağımlılık:** Var / Yok — varsa güvenilirlik değerlendirmesi

## Commit & PR

- **Branch:** `task/TXX-…`
- **Commit:** `<hash>` — mesaj
- **PR:** #NN
- **CI:** ✓ / ✗ — run ID
- **Post-merge run'lar:** *(validator doldurur — merge sonrası; bu satır ayrı bir `chore:` PR'ında veya sonraki task dalında güncellenir, doğrudan ana dala push edilmez)*

## Dış Varsayımlar

> Ön-uçuş kontrolü (00 §F.3). Her varsayım için **bir satır kanıt**.
> "Dış varsayım: yok" geçerli bir sonuçtur — ama **açıkça** yazılır. Boş bırakılmaz.

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|

## Known Limitations / Follow-up

> Ertelenen her kalem `Docs/DEFERRED_BACKLOG.md`'ye **yazılır**. Yalnız burada kalan iş buharlaşır.

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|

## Notlar

- **Working tree (Adım -1):** temiz / N dosya → karar
- **Ana dal CI startup (Adım 0):** run ID'leri + conclusion
- **Repo memory (bitiş kapısı 8):** güncellendi
- **Diğer:**
