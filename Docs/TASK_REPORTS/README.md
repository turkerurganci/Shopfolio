# Task Raporları

Her task için bir rapor: `TXX_REPORT.md` (borç paketleri için `WPn_REPORT.md`).

## Şablonlar

| Şablon | Ne zaman |
|---|---|
| [`_TEMPLATE.md`](_TEMPLATE.md) | Normal task |
| [`_TEMPLATE_BLOCKED.md`](_TEMPLATE_BLOCKED.md) | Task ilerleyemiyorsa |

## Yaşam döngüsü

1. **Yapım chat'i** taslağı oluşturur (kabul kriterleri self-check + kanıtlar + dış varsayımlar + startup check sonuçları).
2. Rapor + status **merge'den önce** commit+push edilir.
3. **Doğrulama chat'i** raporu **Faz 3'e kadar açmaz** — kendi bağımsız verdict'ini önce oluşturur.
4. Validator raporu finalize eder: doğrulama bölümü, kanıtlarla güncellenmiş kabul kriterleri tablosu, bulgular, K-notları.
5. **Kural:** Rapor finalize edilmeden `IMPLEMENTATION_STATUS.md` güncellenmiş sayılmaz.

## Sık yapılan hatalar

- **PR alanını boş bırakmak** → otomatik BLOCKED tetikler (`task` skill'i bitiş kapısı).
- **"Dış varsayım" bölümünü boş bırakmak** → audit trail'in eksikliği, varsayımın yokluğu anlamına gelmez. "Yok" da yazılır.
- **Follow-up'ı yalnız burada bırakmak** → ertelenen iş `DEFERRED_BACKLOG.md`'ye yazılmadıysa ertelenmiş sayılmaz.
- **Post-merge run ID'lerini doğrudan ana dala push etmek** → ayrı `chore:` PR'ı veya sonraki task dalı kullanılır.
