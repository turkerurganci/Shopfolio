---
name: checkpoint
description: "Aşama içi veya aşamalar arası geçişte ilerlemeyi doğrular ve tutarsızlıkları tarar. Doküman aşamaları içindir; kod fazları için gate-check. Kullan: 'checkpoint yap', 'cross-check', /checkpoint."
user-invocable: true
---

# Checkpoint — Aşama Doğrulama

> **Ne zaman:** Bir aşama içinde veya aşamalar arası geçişte; ilerlemeyi doğrulamak ve eksikleri yakalamak için.
> **Tetikleme:** "checkpoint yap", "cross-check" veya `/checkpoint`.
>
> **Gate check'ten farkı:** Gate check **kod fazlarını** kapatır (test, build, ortam, migration). Checkpoint **doküman aşamalarını** ve genel tutarlılığı tarar.

---

## Kontrol adımları

1. **Yol haritası kontrolü** — `Docs/00_PROJECT_METHODOLOGY.md`'yi oku. Şu an hangi aşamadayız? Sıralama doğru mu? Atlanan aşama var mı?

2. **Doküman durumu** — `Docs/PRODUCT_DISCOVERY_STATUS.md`'deki doküman durumu tablosu: tamamlanmış olması gereken ama eksik kalan doküman var mı? Header/footer versiyonları ve "son güncelleme" tarihleri tutarlı mı?

3. **Tutarsızlık taraması** — tamamlanmış tüm dokümanlarda:
   - İki farklı dokümanda aynı konu farklı mı anlatılıyor?
   - Bir dokümanda karar alınmış ama diğerinde yansıması eksik mi?
   - Sayısal değerler (süre, oran, limit, eşik) tüm dokümanlarda tutarlı mı?
   - Enum/durum isimleri birebir eşleşiyor mu?

4. **Açık kararlar** — "detaylandırılacak konular" listesinde, mevcut aşamada netleşmesi gereken ama hâlâ açık karar var mı?

5. **Aşama çıktı kontrolü** — metodolojideki beklenen çıktılar üretildi mi?

6. **Geriye dönük etki** — mevcut aşamada alınan kararlar önceki dokümanları etkiliyor mu? Etkiliyorsa hangi dokümanın güncelleneceğini belirt.

7. **Yeni alan/kural taraması** — bu aşamada eklenen her yeni alan, parametre, enum değeri veya iş kuralı **kendi source-of-truth dokümanında** tanımlı mı? (Cross-review sırasında eklenen öğeler en sık buradan kaçar.)

---

## Çıktı formatı

```
## Checkpoint Sonucu — [Tarih]
**Aşama:** [Mevcut aşama]
**Genel durum:** ✓ Yolunda / ⚠ Dikkat gerektiren noktalar / ✗ Sorun tespit edildi

### Kontrol Özeti
| # | Kontrol | Sonuç | Detay |
|---|---|---|---|
| 1 | Yol haritası | ✓/⚠/✗ | |
| 2 | Doküman durumu | ✓/⚠/✗ | |
| 3 | Tutarsızlık | ✓/⚠/✗ | |
| 4 | Açık kararlar | ✓/⚠/✗ | |
| 5 | Aşama çıktıları | ✓/⚠/✗ | |
| 6 | Geriye dönük etki | ✓/⚠/✗ | |
| 7 | Yeni alan/kural | ✓/⚠/✗ | |

### Aksiyon Gerektiren Maddeler
- [ ] …

### Notlar
…
```

Bu çıktı `Docs/CHECKPOINT_REPORTS/` altına yazılır ve özeti `Docs/PRODUCT_DISCOVERY_STATUS.md`'nin "Checkpoint Log" bölümüne eklenir.
