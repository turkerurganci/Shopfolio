# Oturum Hook'ları (opsiyonel)

**Katman:** L4 — **L5 değil.**

---

## Bu klasör ne için

Ajan harness'ının desteklediği **oturum hook'ları** buraya konur: bir araç çağrısından önce/sonra çalışan, bağlama bilgi enjekte eden veya hatırlatma yapan script'ler.

**Bunlar L5 sayılmaz.** L5'in tanımı `exit 1` veren katmandır (`00 §A.4`, `§F.2`). Oturum hook'ları:

- **bloklamaz** — hatırlatır
- **platforma bağlıdır** — kabuk, işletim sistemi ve harness sürümüne göre değişir
- **kuralın yerine geçmez** — kural her zaman metodolojide, talimatlarda ve skill'de yazılıdır

Bir kural yalnız buradaki bir script'e dayanıyorsa, o kural **yazılı değildir**.

---

## Kayıt

Hook'lar `.claude/settings.json` içinde kaydedilir — **repo'da izlenen** dosya. Kişisel ayarlar `.claude/settings.local.json`'dadır ve `.gitignore`'dadır.

| Dosya | İzlenir mi | İçerik |
|---|---|---|
| `.claude/settings.json` | ✓ Evet | Ekip geneli hook kayıtları + paylaşılan izin listesi |
| `.claude/settings.local.json` | ✗ Hayır | Kişisel/makineye özgü ayarlar |
| `.claude/hooks/*.log` | ✗ Hayır | Hook çalışma zamanı çıktıları |

---

## Faydalı desenler

| Amaç | Nasıl |
|---|---|
| **Post-merge workflow hatırlatıcısı** | Merge komutu algılandığında, o commit için tetiklenen workflow ID'lerini bağlama enjekte et. `validate` skill'i Adım 18'in (post-merge CI watch) unutulmasına karşı yapısal destek. |
| **Salt-okunur komut ön onayı** | Sık kullanılan zararsız komutları otomatik onayla; izin sorularını azalt. |
| **Oturum başı durum enjeksiyonu** | Açık PR'lar veya son CI durumu gibi bağlamı oturum başında hazır et. |

---

## Kural

Bir oturum hook'u eklenirken **kural nerede yazılı** sorusu cevaplanır. Hook yalnız hatırlatıcıdır; kuralın kendisi `Docs/00_PROJECT_METHODOLOGY.md`, `.claude/INSTRUCTIONS.md` veya ilgili skill'de bulunmalıdır. Hook silinince kural ayakta kalmalıdır.
