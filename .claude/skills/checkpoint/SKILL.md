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

**Koşum:** mekanik kontroller betikle yapılır; anlamsal tarama alt ajan mercekleriyle koşuyorsa `audit` skill'inin "Koşum biçimi" 4. ve 5. maddeleri geçerlidir — betiğin örneklenerek doğrulanması; dalga büyüklüğü, bulgunun bulunduğu anda dosyaya yazılması ve merceklerin ön planda başlatılması. Bulgu doğrulayıcıları kademeyle koşar (Opus / `high`; yüksek bulgu başına bir, orta aynı bölümden 2–4'lü, düşük aynı bölümden 5–8'li; `INSTRUCTIONS.md` §9 (1), K-1224, K-1300); mercek ve doğrulayıcı önce kendi dilim dosyasını okur, karar kaydı hiçbir ajana bütünüyle okutulmaz ve doğrulayıcının düzeltmesi birebir şartname olarak yazılıp betikle uygulanır (`audit` skill'i, "Koşum biçimi" 1. ve 2. madde). **Üst doküman hizası checkpoint PR'ında yapılır:** bulgunun düzelttiği kural bir üst dokümanın (`07`, `02`, `09` gibi) lafzını da etkiliyorsa not o dokümanda aynı PR'da yazılır, sonraki adıma bırakılmaz (Aşama 10 CP10 §6.5; Ö-197). **Mercek talimatı farkın açıklanmış olabileceğini söyler:** "ortam ya da severity farkı bölüm girişinde açıklanmış olabilir; önce bölüm girişini oku, açıklanmışsa bulgu açma" (Ö-204; çürük bulgu oranını düşürür). **Kaynak karar zaten hükmetmişse doğrulayıcı dayanağı yeniden açmaz:** var olan bir kararın yansıması, "dayanak doğrulanamadı" diye soruya çevrilmez; yansıma olarak uygulanır. **Neden:** Aşama 8'in checkpoint'inde bir kararın (K-546) yansıması "yasal dayanak doğrulanamadı" diye soru yapıldı, yönetici yansıma olarak uyguladı (S3-10; Aşama 8'in öğrenim terfisi, Ö-153).

**Mercek dilimi küçük tutulur (Aşama 9, CP09 §5.2, §6):** Sonnet / `low` mercek, kaynak karşılaştırması gereken dilimde kapsamı bitirmez. Task bloğu merceği için paket en çok ~60 KB ve 8–12 task'tır (KP-checkpoint-3'te en çok 8 task / ≤ 60 KB'lık 41 paket açık task bırakmadan bitti); matris merceği için dilim satır başına değil task başına kümelenir ya da matris için atıf ↔ `Dokümanlar` kapsaması betikle sınanır (tablo satırı başına kaynak cümle açmak task bloğu başına açmaktan çok pahalıdır); kapsam notu merceği yalnız açık kalan kaynak bölümlerini ve en çok ~12 task'ı alır. Her mercek bitirirken **hangi dilimleri / task'ları bitirdiğini** bir satırda yazar; yazılmayan dilim bitmemiş sayılır ve yeniden paketlenir. Kapsam açığı "doğrulanmayanlar" listesine bırakılmaz, hiçbir adım açığı sonraki aşamaya ya da başka bir kapıya ("`validate`'te kapanır") devretmez; checkpoint kapsamı tam bitmeden kapanmaz. **Neden:** Aşama 9'da 80 KB'lık dilimler (4, 6, 9), 140 satırlık matris bölümleri (B3a, B3c) ve 598 satırlık kapsam notu paketi (X05) merceğin bağlam sınırında kapsamı bırakmasına yol açtı ve KP-checkpoint-2 ile -3 olarak iki adım daha gerekti; ≤ 60 KB paketleri bitirdi (CP09 §5.1, §5.2, §6; Aşama 9'un öğrenim terfisi, Ö-164, Ö-165).

**Kalite döngüsünün devri:** cross-review çıkış kuralıyla kapandıysa (`cross-review` skill'i, Faz 4 madde 5) devredilen sınıf — karar kaydının açık süreç maddesinde adıyla ve örnekleriyle yazılıdır — bir iç tutarlılık merceğine verilir ve dokümanın tamamında aranır; sonuç checkpoint raporuna ve maddenin kapanışına yazılır. **Neden:** Arayüz Tanımları'nın devredilen "özet cümle ile ayrıntının ayrışması" sınıfı checkpoint'te kırk üç yerde bulundu (CP03 §3.1, §3.2; K-849).

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
