---
name: handoff
description: "Oturumu temiz kapatır ve sonraki chat'in sorunsuz başlamasını sağlar. Kullan: 'yeni chate geçiyorum', 'oturumu kapat', /handoff."
user-invocable: true
---

# Handoff — Chat Geçişi

> **Ne zaman:** Yeni bir chat'e geçilmeden önce; mevcut oturumun temiz kapanması ve sonrakinin sorunsuz başlaması için.
> **Tetikleme:** "yeni chate geçiyorum", "oturumu kapat", "handoff" veya `/handoff`.

---

## Kontrol adımları

1. **Working tree kontrolü** — `git status --short`. Dirty ise oturum kapatılmadan karar alınır: commit + PR / stash / discard. **Kirli working tree ile oturum kapatılmaz** — bir sonraki oturum bunu kendi işine bundle etme riskiyle başlar.

2. **Bekleyen iş kontrolü** — başlanmış ama tamamlanmamış iş var mı?
   - Açık PR var mı, CI'sı izlendi mi, sonucu raporlandı mı?
   - Yarım kalan iş varsa tamamla veya proje sahibine açıkça bildir.

3. **Repo hafızası** — `.claude/memory/MEMORY.md`:
   - "Güncel Durum" bloğu güncel mi?
   - Bu oturumda alınan kararlar veya öğrenimler yazıldı mı?
   - Yeni bir çalışma tercihi (feedback) kaydedilmeli mi?
   - Snapshot şişmiş mi? Şişmişse tarihsel detay arşiv dosyasına taşınır (00 §G.2).

4. **Durum dosyaları** — `Docs/IMPLEMENTATION_STATUS.md` (implementation dönemi) veya `Docs/PRODUCT_DISCOVERY_STATUS.md` (doküman dönemi) güncel mi? Header/footer versiyonları ve tarih tutarlı mı?

5. **Tutarsızlık taraması** — bu oturumda değiştirilen dosyalarda cross-reference kontrolü: doküman referansları, bölüm numaraları, versiyon numaraları.

6. **Kural terfisi kontrolü** — bu oturumda bir süreç ihlali yaşandı veya önlendi mi? Yaşandıysa bu bir **kural adayıdır**: hafızaya not olarak değil, ilgili L1–L5 dosyasına terfi olarak yazılmalı (00 §K).

7. **CONTEXT.md** — bu oturumda yeni bir **klasör** açıldı mı? (Yeni dosya değil — klasör.) Açıldıysa `.claude/CONTEXT.md` klasör haritası güncellenir.

8. **Sıradaki adım** — proje sahibine sonraki oturumda ne yapılacağını bildir.

---

## Çıktı formatı

```
## Handoff Sonucu — [Tarih]

### Oturum Özeti
- **Yapılan iş:** …
- **Değiştirilen dosyalar:** …
- **Açılan PR'lar ve CI sonuçları:** …

### Kontrol Özeti
| # | Kontrol | Sonuç |
|---|---|---|
| 1 | Working tree | ✓ Temiz / ⚠ [karar] |
| 2 | Bekleyen iş | ✓ Yok / ⚠ [detay] |
| 3 | Repo hafızası | ✓ Güncel / ⚠ Güncellendi |
| 4 | Durum dosyası | ✓ Güncel / ⚠ Güncellendi |
| 5 | Tutarsızlık | ✓ Yok / ⚠ [düzeltilen] |
| 6 | Kural terfisi | ✓ Yok / ⚠ [terfi edilen] |
| 7 | CONTEXT.md | ✓ Güncel / ⚠ Güncellendi |

### Sıradaki Adım
[Sonraki oturumda yapılacak iş]
```
