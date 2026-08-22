# Hafıza Katmanı — Ne Buraya Girer, Ne Girmez

**Katman:** L4 | **Son güncelleme:** 2026-08-22

---

## Ayrım

| Tür | Nerede yaşar | Örnek |
|---|---|---|
| **Süreç kuralı** | **L1–L5** (metodoloji / talimat / guardrail / skill / hook) | "Doğrulama ayrı chat'te yapılır", "CI'yi ajan izler", "kırık CI üstüne task başlamaz" |
| **Proje hafızası** | `MEMORY.md` + `project_*.md` | Güncel durum snapshot'ı, teknoloji kararları, mimari kararlar |
| **Kişisel çalışma tercihi** | `feedback_*.md` | Yanıt dili, öneri savunma tarzı, onay isteme sınırı, açıklama derinliği |
| **Dış referans** | `reference_*.md` | Panolar, biletler, hesap/kota bilgileri, dış doküman bağlantıları |
| **Kullanıcı profili** | `user_*.md` | Teknik geçmiş, hangi kodu review edeceği, hangi analojilerin işe yaradığı |

---

## Terfi kuralı (kritik)

> **Bir süreç kuralı yalnızca hafızada yaşıyorsa kırılgandır.**

Bir hafıza notu **ikinci kez** bir süreç ihlalini önlemek için kullanılıyorsa, o not artık bir **kuraldır**:

1. İlgili L1–L5 dosyasına **terfi eder** (metodoloji / INSTRUCTIONS / GUARDRAILS / skill / hook)
2. Hafızada yalnız bir işaretçi kalır: *"→ terfi etti: `.claude/INSTRUCTIONS.md` §3.2"*
3. Terfi, faz gate check'inin Adım 7'sinde (öğrenim terfisi) veya `/handoff` Adım 6'da tetiklenir

**Neden:** Bir referans projede yürürlükteki kural katmanı beş yere dağılmıştı ve önemli bir kısmı yalnız hafızada yaşıyordu. Hafıza dosyaları oturuma her zaman yüklenmez, sürüm kontrolünde diff'lenmez ve yeni bir chat'te sessizce kaybolabilir. Kural, `exit 1` verebilen veya skill adımı olarak okunan bir yerde durmalıdır.

---

## Dosya formatı

```markdown
---
name: <kısa-kebab-case-slug>
description: <tek satır özet>
type: project | feedback | reference | user
---

<Kural veya bilgi — tek fikir.>

**Why:** <neden — hangi somut olaydan doğdu>

**How to apply:** <nasıl uygulanır — maddeler>
```

- Her dosya **tek bir fikir** taşır.
- Göreli tarihler mutlak tarihe çevrilir ("geçen hafta" değil, "2026-07-14").
- Kaynak dosyaya referans verilirken yol yazılır; o dosya taşınırsa referans güncellenir.

---

## MEMORY.md nedir

`MEMORY.md` **indeks + güncel durum snapshot**'ıdır:

- Her hafıza dosyası için **tek satır** işaretçi
- "Güncel Durum" bloğu: son tamamlanan iş, sıradaki iş, açık PR'lar

**MEMORY.md'ye içerik gömülmez.** Her hafıza bir dosyadır; `MEMORY.md` yalnız onları listeler.

---

## Şişme kuralı (00 §G.2)

`MEMORY.md` prepend-only changelog'a **dönüştürülmez**.

- Güncel snapshot **kısa** kalır (bir ekran boyu hedefi)
- Tarihsel task detayı `MEMORY_ARCHIVE.md`'ye taşınır
- Tek bir task'ın detayı gerektiğinde arşiv `grep`'lenir

**Neden:** Bir referans projede hafıza dosyası okuma sınırını aşacak kadar büyüdü ve **hiç açılamaz** hâle geldi; "başka bir oturumda nerede kaldığımızı bulamıyorum" şikâyetiyle fark edildi.

---

## Otoritesizlik kuralı (00 §G.3)

Hafıza **bilgilendiricidir, otoriter değildir**.

"Sırada ne var / nerede kaldık" sorularında **dönemin tracker'ı** okunur (ilgili satır `grep`'lenerek) — doküman üretiminde `Docs/PRODUCT_DISCOVERY_STATUS.md`, implementation'da `Docs/IMPLEMENTATION_STATUS.md` (00 §G.1). İkisi çelişirse **tracker kazanır** ve hafıza düzeltilir.
