# Instructions

Read and follow these files at the start of every session:

- `Docs/00_PROJECT_METHODOLOGY.md` — metodoloji: aşamalar, kapılar, konvansiyonlar (L1)
- `.claude/INSTRUCTIONS.md` — çalışma talimatları (L3)
- `.claude/GUARDRAILS.md` — sınırlar ve yasaklar (L3)
- `.claude/CONTEXT.md` — proje bağlamı ve klasör haritası (L3)
- `.claude/memory/MEMORY.md` — proje hafızası (L4)

## Oturum başlangıcı (istisnasız)

1. `git status --short` — working tree dirty ise **dur**, karar al (INSTRUCTIONS §3.0)
2. Bir task veya doğrulama başlatılacaksa ilgili skill'in başlangıç kapılarını işlet (`.claude/skills/`)
3. Durum sorusu geldiyse `Docs/IMPLEMENTATION_STATUS.md`'yi oku — hafıza snapshot'ı otoriter değildir

## Kural katmanları

Her kuralın tek bir evi vardır: **L1** metodoloji · **L2** `Docs/01`–`12` · **L3** `.claude/*.md` · **L4** `.claude/skills/` ve `.claude/memory/` · **L5** `scripts/git-hooks/`, `.github/workflows/`.

Bir süreç kuralı yalnızca hafızada yaşıyorsa **kırılgandır** — L1–L5'e terfi eder (00 §K).

## Dil

Dokümanlar, tartışmalar ve raporlar **Türkçe**. Kod, kod yorumları ve sembol adları **İngilizce**. Bu bir parametre değil, sabittir.

## Kurulum

Yeni bir projede ilk iş: [`SETUP.md`](SETUP.md).
