---
name: deep-review
description: "Bir dokümanın kalitesini ve tutarlılığını 8 katmanda analiz eder. Audit kapsama bakar, bu kaliteye bakar. Kullan: 'deep review yap', 'zayıf noktaları bul', /deep-review XX."
user-invocable: true
---

# Deep Review — Doküman Kalite ve Tutarlılık Analizi

> **Ne zaman:** Bir dokümanın içeriğinin yeterliliğini, tutarlılığını ve sağlamlığını doğrulamak için.
> **Tetikleme:** "deep review yap", "zayıf noktaları bul" veya `/deep-review XX`.
>
> **Audit'ten farkı:** Audit *kapsam*a bakar (her kaynak öğe karşılık buldu mu). Deep review *kalite*ye bakar (bulduğu karşılık yeterince sağlam mı). İkisi birbirinin yerine geçmez.

## Parametreler

| Parametre | Zorunlu | Açıklama |
|---|---|---|
| `hedef` | Evet | Kontrol edilecek doküman numarası |
| `bağlam` | Hayır | Karşılaştırılacak dokümanlar; verilmezse header'daki bağımlılıklar + MVP kapsamı |
| `odak` | Hayır | Belirli konu; verilirse **tüm katmanlar** yalnız o konuya odaklanır |

## Ön hazırlık

1. Hedef dokümanı **baştan sona** oku.
2. Bağlam dokümanlarını oku.
3. `Docs/00_PROJECT_METHODOLOGY.md` §C.1'den hedefin aşamasını, amacını ve çıktı beklentisini al — doküman bu beklentileri karşılıyor mu?

---

## Analiz katmanları

### Katman 1 — Kapsam (Coverage)
**Soru:** Bağlam dokümanlarındaki her gereksinim/akış/karar hedefte karşılık buluyor mu?

- Karşılığı olmayan madde → `BOŞLUK (GAP)`
- Hedefte olup kaynağı olmayan madde → `KAYNAKSIZ EKLEME`

### Katman 2 — Tutarlılık (Consistency)
**Soru:** Aynı kavram tüm dokümanlarda aynı mı?

- Durum/state isimleri, terimler, roller, modül adları
- Sayısal değerler (süreler, oranlar, limitler, eşikler)
- Akış sıralaması
- **Hedefin kendi içindeki** tutarlılık (özet tablo ↔ detay bölüm)

### Katman 3 — Teknik derinlik (Depth)
**Soru:** Her karar yeterince detaylı mı, yoksa uygulamacıyı belirsizlikle mi bırakıyor?

- Her karar/mekanizma için: *"Bunu okuyan bir geliştirici uygulamaya başlayabilir mi, yoksa 'ama nasıl?' mı diyecek?"*
- Belirsiz ifadeleri tespit et: "gereksinime göre belirlenecek", "ileride düşünülecek", "detaylar sonra"
- Her belirsizlik için: bu dokümanda mı çözülmeli, yoksa sonraki dokümanın sorumluluğunda mı?

### Katman 4 — Güvenlik (Security)
**Soru:** Güven-kritik akışlarda açık var mı?

- Her dış iletişim kanalını tara (servisler arası, kullanıcı↔platform, platform↔üçüncü parti)
- Her veri akışında *"bu manipüle edilebilir mi?"*
- Kimlik doğrulama, yetkilendirme, input validation, şifreleme katmanları
- Webhook/callback'lerde imza doğrulama
- Hassas veri (key, secret, token) saklama stratejisi **ve dağıtım yarıçapı** (hangi bileşen hangi sırrı alır)
- Rate limiting kritik yüzeylerde tanımlı mı
- **OWASP Top 10 perspektifinden değerlendir** — adlandırılmış, teknoloji-bağımsız bir çerçeve, katmanı denetlenebilir kılar: *"neye göre baktın?"* sorusunun cevabıdır. Çerçevesiz güvenlik katmanı serbest-form bir okumaya iner.

### Katman 5 — Hata modu (Failure Mode)
**Soru:** Her bileşen çökerse ne olur?

- Her bileşen için *"bu 10 dakika erişilemez olursa ne olur?"*
- Tek nokta hatası (SPOF) tespiti
- Recovery / fallback mekanizması tanımlı mı
- Veri kaybı riski taşıyan senaryolar

### Katman 6 — Veri akışı izleme (Data Flow)
**Soru:** Kritik veri parçaları dokümanlar boyunca tutarlı yönetiliyor mu?

- Her kritik veri için yolculuğu izle: nerede oluşur → doğrulanır → kullanılır → güncellenir → silinir/arşivlenir
- Kopukluk: bir dokümanda "X saklanır" denmiş ama hedefte teknik karşılığı yok
- **Snapshot mı referans mı** kararları net mi
- Veri dönüşümleri (hesaplama formülleri) tüm dokümanlarda aynı mı

### Katman 7 — Ölçeklenebilirlik (Scalability)
**Soru:** Yük artarsa mimari dayanır mı? MVP kararları ileriyi engelliyor mu?

- Yatay ölçekleme yolu açık mı
- Veri büyümesi stratejisi (partitioning, arşivleme)
- Stateful bileşenlerin ölçekleme planı
- "Büyüdüğünde X'e geçilir" ifadeleri gerçekçi mi

### Katman 8 — Bağımlılık riski (Dependency Risk)
**Soru:** Dış bağımlılıkların değişme/kapanma/kısıtlanma riskleri değerlendirilmiş mi?

- Tüm dış bağımlılıklar listelenmiş mi
- Her biri için rate limit ve kısıtlar belirtilmiş mi
- Soyutlama katmanı var mı (bağımlılık değişirse etki alanı sınırlı mı)
- Kritik bağımlılıklar için alternatif/fallback

---

## Çıktı formatı

```
## Deep Review Raporu — [Hedef Doküman]
**Tarih:** … **Hedef:** … **Bağlam:** … **Odak:** [veya "Tam analiz"]

### Özet Skor Tablosu
| # | Katman | Skor | Kritik Bulgu |
|---|---|---|---|
| 1 | Kapsam | ✓/⚠/✗ | |
| … | … | | |

**Genel:** ✓ Sağlam / ⚠ İyileştirme gerekli / ✗ Kritik eksikler var

### Katman Detayları
#### Katman N — [Ad]
| # | Seviye | Bulgu | Kaynak | Öneri |
|---|---|---|---|---|

### Aksiyon Planı
**Critical:** … **High:** … **Medium:** … → [hangi dokümanda] **Low:** …

### Cross-reference Haritası
| Hedef Bölüm | 02 | 03 | 04 | 10 | Durum |
|---|---|---|---|---|---|
```

**Skor:** ✓ sorun yok/minor · ⚠ güçlendirilmeli ama kullanılabilir · ✗ düzeltilmeden sonraki aşamaya geçilmez

---

## Kurallar

1. **Önce oku, sonra yargıla.** Bir bölümdeki "eksiklik" başka bölümde karşılanmış olabilir.
2. **Seviye ayrımını doğru yap.** Güvenlik/veri kaybı → Critical; uygulama belirsizliği → High; sonraki dokümanda çözülebilir → Medium; farkındalık yeterli → Low.
3. **Sonraki dokümanların kapsamını bil.** Henüz yazılmamış dokümanların doğal kapsamına giren eksiklik Medium'dur.
4. **Pozitif bulguları da belirt** — özet tabloda güçlü yönleri not et.
5. **Odak verilmişse** 8 katmanı yalnız o konuya odaklayarak çalıştır.
6. **Çıktı uzunluğunu bulguya göre ayarla.** Gereksiz dolgu yazma.
