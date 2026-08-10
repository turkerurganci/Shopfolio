# <PROJE> — Coding Guidelines

**Versiyon: v0.1** | **Bağımlılıklar:** `02`, `04`, `05`, `06`, `07`, `08`, `10_MVP_SCOPE.md` | **Son güncelleme:** YYYY-AA-GG

> **Aşama:** 8 — Kodlama Kılavuzu · **Rol:** Tech Lead
> **Amaç:** Ajanın tutarlı kod üretmesi. Bu doküman **her kodlama görevinde** ajana verilir.
> **Uyarı:** Bu doküman diğerlerinden zordur — hem çok sayıda dokümanla tutarlılık hem implementasyon düzeyi detay ister; kalite döngüsü genelde daha uzun sürer. Burada eklenen her yeni alan/parametre `06`'da tanımlı mı diye **kontrol edilmelidir**.

---

## 1. Temel ilkeler

- Somut örnekle tanımla. "Hata yönetimi yapılmalı" değil, "hata yönetimi şöyle yapılır: (somut örnek)".
- Tek kaynak, tek doğru. Bir kural iki yerde farklı anlatılmaz.

## 2. Source of truth hiyerarşisi

> **Ne yazılır:** Çelişki durumunda hangi doküman kazanır. Örn. iş kuralı → `02`; şema → `06`; sözleşme → `07`.

## 3. Genel kod yazım kuralları

## 4. Proje yapısı ve klasör organizasyonu

## 5. Adlandırma konvansiyonları

## 6. Mimari sınırlar

> **Ne yazılır:** Katman sınırları, modüller arası iletişim kuralı, iş kuralının hangi katmanda yaşayacağı.

## 7. Zaman ve biçim standartları

> **Ne yazılır:** Zaman damgası kaynağı (yerel saat kullanımı yasağı), tarih/sayı biçimleri, zaman dilimi politikası.

## 8. Hata yönetimi

## 9. Domain kuralları

> **Ne yazılır:** Durum makinesi kullanımı, geçiş koruma, yan etki yerleşimi, olay yayını, denetim kaydı.

## 10. Veri erişimi

> **Ne yazılır:** Sorgu kuralları, sayfalama zorunluluğu, izleme kapalı sorgular, işlem sınırları.

## 11. Entegrasyon kuralları

## 12. Sözleşme ve olay sürümleme

## 13. Arka plan iş kuralları

## 14. Hassas hesaplama kuralları

> **Ne yazılır:** Para/oran/ölçü hesaplamalarında tip seçimi, yuvarlama politikası (**ara adımda yuvarlama yok**), test zorunluluğu.

## 15. Güvenlik kuralları

## 16. İstemci tarafı kurallar

## 17. Yardımcı servis / sidecar standartları (varsa)

## 18. Loglama ve gözlemlenebilirlik

## 19. Test kuralları

> **Ne yazılır:** Test katmanları, hangi davranış hangi katmanda test edilir, test verisi stratejisi, izolasyon.
> **Ek kural:** `skip` / `only` / boş assertion içeren test **yazılmaz**; geçen ama hiçbir şey doğrulamayan test, boşluğun gizlenmiş hâlidir (gate check bunu tarar).

## 20. Performans kuralları

## 21. Git ve CI/CD kuralları

### 21.1 Dal stratejisi

> **Kural:** Bu bölüm `.claude/INSTRUCTIONS.md §3.2` ile **birebir aynı** olmalıdır. İki farklı yerde iki farklı dal stratejisi, bu dokümanların en sık çelişkisidir.

| Dal | Amaç |
|---|---|
| `main` | Çalışır durumda kod. Doğrudan push **yasak**. |
| `task/TXX-*` | Tek bir task. `main`'den ayrılır, `main`'e squash merge olur. |
| `chore/*`, `docs/*`, `infra/*` | Task'a ait olmayan işler — ayrı PR. |

### 21.2 Commit mesaj formatı

```
{tip}: {kısa açıklama}      # task dışı
TXX: {kısa açıklama}        # task commit'i
```

Tipler: `feat` · `fix` · `refactor` · `test` · `docs` · `chore` · `migration`

### 21.3 PR kuralları

- PR tek bir amaca hizmet eder. Karışık değişiklik yasak.
- **Bundled-PR yasağı** — başka bir task'ın PR'ına commit gömülmez.
- CI geçmeden merge yasak. Bağımsız validator PASS olmadan merge yasak.
- Merge'ü validator chat yapar.

### 21.4 CI pipeline sıralaması

```
0. Guard (direct push)
1. Lint
2. Build
3. Unit test
4. Integration test
5. Contract test
6. Migration dry-run
7. Paketleme doğrulaması
8. E2E (advisory)
→ CI Gate (toplu kapı)
```

Komutlar: `.github/ci-commands.env` · Kurulum: `Docs/CI_CD_SETUP.md`

### 21.5 Savunma katmanları

> Tek bir savunma katmanı yetmez. Katmanların tam listesi ve gerekçeleri: `00_PROJECT_METHODOLOGY.md` §F.
> Bu bölüm o listeye referans verir; **kuralı burada tekrar tanımlama** — iki yerde iki farklı ifade çelişki üretir.

## 22. Refactor kuralları

- Refactor davranış değiştirmeden yapılır. Davranış değişecekse **ayrı commit/PR**.
- Refactor PR'ı feature PR'ıyla karıştırılmaz.
- Kritik akışa dokunan kod, test veya koruma olmadan taşınmaz.

## 23. Yasaklar

> **Ne yazılır:** Koşulsuz yasaklar — iş kuralı uydurma, sessiz kapsam genişletme, katman ihlali, secret gömme, hassas veri loglama, exception yutma, sınırsız liste dönme, testsiz kritik davranış değişikliği, CI geçmeden merge.
> Süreç yasakları için ayrıca: `.claude/GUARDRAILS.md` §8.

## 24. AI çalışma kuralları

> **Ne yazılır:** Kod yazmadan önce hangi source-of-truth kontrol edilir · küçük diff · mevcut mimariyi bozmama · gereksiz bağımlılık eklememe · kritik akışlara özel kontroller (durum geçişi eklerken guard + yan etki + olay + denetim kaydının tamlığı gibi).

## 25. Review checklist

> **Ne yazılır:** Her kod tesliminde sorulacak sorular — doküman uyumu, mimari, durum/olay, güvenlik, test, performans.

---

*<PROJE> — Coding Guidelines v0.1*
