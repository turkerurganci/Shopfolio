# <PROJE> — Bypass ve Süreç İhlali Kaydı

> Disiplin kapılarının bypass edildiği her durum burada kayıtlıdır. **Hook'lar bu dosyaya otomatik satır ekler.**
>
> **Kural:** Bypass kullanıldıktan sonraki **ilk normal commit'te** bu dosyadaki değişiklik commit'lenir.

---

## `[kind]` önekleri

| Önek | Ne zaman |
|---|---|
| `[direct-push]` | Korunan dala direct push bypass'ı (`PB_ALLOW_DIRECT_PUSH=1`, pre-push Layer 1) |
| `[ci-failure]` | Dalın son CI run'ı başarısızken push bypass'ı (`PB_ALLOW_DIRECT_PUSH=1`, pre-push Layer 2) |
| `[bundled-pr]` | Dal izolasyon bypass'ı (`PB_ALLOW_BUNDLED=1`, commit-msg veya pre-push Layer 3). Ayrıca: bir task'ın PR'ı açılmadan başka bir PR'a gömülmesi (bitiş kapısı ihlali) de bu etiketi alır. |
| `[secret]` | Sır guard'ı bypass'ı (`PB_ALLOW_SECRET=1`, pre-commit). **Diğerlerinden farklıdır:** yanlış pozitifse desen daraltılır; gerçek sır commit'lendiyse olay **geri alınamaz** — değer döndürülür, geçmiş temizliği yeterli değildir. |
| `[skip-guard]` | CI guard job bypass'ı (commit mesajında `[skip-guard]`) — hook değil, CI tarafı |

---

## Bypass'ın maliyeti (okumadan bypass'a başvurma)

- Bypass **sessiz** olmaz: hook satır yazar, o satır working tree'yi kirletir, kirli tree bir sonraki oturumun başlangıç kapısını tetikler.
- Post-merge kozmetik düzeltmeyi direct-push ile yapmak **döngü** yaratır: bypass push → hook log satırı → dirty tree → yeni bypass commit'i gerekir. Bu durumda doğru çözüm `chore:` PR'ıdır.
- CI guard job ayrı bir katmandır: hook bypass'ı push'u geçirse bile commit mesajında PR referansı yoksa CI kırmızı yanar. **İki mekanizma birlikte düşünülmelidir.**

---

## Log

| Tarih | Kullanıcı | Dal | Commit | Sebep |
|---|---|---|---|---|
| 2026-08-01 20:03 UTC | Türker urgancı | main | `0c6213f` | **[direct-push]** ilk yayin — remote bootstrap; main'in uzakta karsiligi yok, PR akisi henuz mumkun degil  |
