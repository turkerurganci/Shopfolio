# Checkpoint ve Gate Raporları

Dört tür rapor burada yaşar:

| Tür | Dosya adı | Üreten |
|---|---|---|
| **Aşama checkpoint'i** (doküman dönemi) | `CP<NN>_<konu>.md` | `/checkpoint` |
| **Aşama kapanışı çakışma taraması** (doküman dönemi; checkpoint'in girdisi) | `PHASE<N>_CONFLICT_SCAN.md` | Kapanış sırasının 3. adımı — K-429, K-437 |
| **Aşama kapanışı öğrenim terfisi** (doküman dönemi; `00 §K`) | `PHASE<N>_LEARNING_PROMOTION.md` | Kapanış sırasının 5. adımı — K-437, K-438 (`checklists/document-stage.md` §7) |
| **Faz gate check'i** (implementation dönemi) | `GATE_CHECK_FX.md` | `/gate-check FX` |

## Gate raporu neleri **mutlaka** içerir

Bir gate raporu aşağıdaki bölümlerden biri eksikse **tamamlanmamıştır**:

- Ön kontrol (task durumları, rapor tutarlılığı, working tree, ana dal CI, **ortam sürümleri**)
- Test sonuçları (mevcut faz + **önceki tüm fazlar**)
- Build
- Fresh environment smoke
- Migration rehearsal (**idempotency dahil**)
- Traceability ve boşluk taraması (+ vacuous test taraması)
- Güvenlik özeti (**yeni dış bağımlılıklar** dahil)
- Bulgular / bloke-etmeyen bulgular
- **Öğrenim terfileri** (00 §K) — boşsa gerekçesi yazılır
- **Backlog hijyeni**
- Faz tag'i

## Retro güncellemesi

Bir gate raporu **sonradan** genişletilebilir: verdict korunur, kanıt tabanı genişletilir. Bunun için rapora `## Retro Güncelleme — YYYY-AA-GG` bölümü eklenir.

> **Ne zaman gerekir:** Bir gate PASS verdikten sonra, o gate anında görülmemiş bir borç ortaya çıkarsa. Raporu **silmek veya sessizce düzeltmek yerine** retro bölümü eklenir — kararın hangi kanıtla verildiği ve borcun nasıl kapandığı ikisi de kayıtta kalır.
>
> **Vaka:** Bir referans projede bir faz gate'i, ana dalın CI'ı kırmızıyken PASS aldı; bu ancak bir sonraki fazda, başka bir validator tarafından fark edildi. Rapor geri alınmadı — retro bölümüyle borç, kök neden ve kapatan işler belgelendi.
