---
name: audit
description: "Bir dokümanı envanter bazlı sistematik denetimden geçirir; önce kontrol edilecek her şeyi listeler, sonra tek tek denetler, atlama olmaz. Kullan: 'audit yap', 'sistematik kontrol', /audit XX."
user-invocable: true
---

# Audit — Sistematik Doküman Denetimi

> **Ne zaman:** Bir dokümanın eksik, tutarsız veya yetersiz noktalarını **eksiksiz** tespit etmek için.
> **Tetikleme:** "audit yap", "sistematik kontrol", "eksiksiz review" veya `/audit XX`.
>
> **Temel fark:** Klasik review *"dokümanı oku, ne bulursan raporla"* der — her seferinde farklı şeye dikkat eder, farklı şeyi atlar. Audit *"önce kontrol edilecek her şeyi listele, sonra her birini tek tek denetle"* der. **Envanter sabittir, atlama olmaz.**

## Parametreler

| Parametre | Zorunlu | Açıklama |
|---|---|---|
| `hedef` | Evet | Denetlenecek doküman numarası (örn. `05`) |
| `bağlam` | Hayır | Kaynak dokümanlar. Verilmezse hedefin `Bağımlılıklar` header alanı + `10_MVP_SCOPE.md` |
| `odak` | Hayır | Belirli bir konuya odaklanma; verilirse envantere yalnız o konuyla ilgili öğeler girer |

---

## Faz 0 — Ön hazırlık

1. **Hedef dokümanı oku** — baştan sona, tam olarak.
2. **Bağlam dokümanlarını belirle** — `bağlam` parametresi veya hedefin header'ındaki bağımlılıklar + MVP kapsamı.
3. **Bağlam dokümanlarını oku** — baştan sona, tam olarak.
4. **Metodoloji kontrolü** — `Docs/00_PROJECT_METHODOLOGY.md` §C.1'den hedefin aşamasını bul; o aşamanın amacı ve çıktı beklentisi referanstır.

**Kural:** Tüm dokümanlar tam okunmadan Faz 1'e geçilmez.

---

## Faz 1 — Envanter çıkarma

**Amaç:** Analiz başlamadan önce kontrol edilecek **her** öğeyi çıkar ve numaralandır. Envanter = neyin kontrol edileceğinin sözleşmesi.

### 1.1 Kaynak dokümanlardan

| Yapı | Kural |
|---|---|
| Tablo satırları | Her satır bir öğe |
| Madde işaretli listeler | Her madde bir öğe |
| Numaralı adımlar | Her adım bir öğe |
| Alt başlık altındaki kurallar | Her kural bir öğe |
| Edge case tabloları | Her senaryo satırı bir öğe |

**Numaralandırma:** `[DokümanNo]-§[BölümNo]-[SıraNo]` → örn. `02-§4.4-02`

### 1.2 Hedef dokümanın iç envanteri

İç tutarlılık ve kalite kontrolü için hedeften de öğe çıkarılır: her bileşen/servis/modül · her teknoloji seçimi veya mimari karar · her sayısal değer (timeout, oran, limit, eşik) · her dış bağımlılık · her iletişim kanalı · her veri varlığı veya akışı.

Format: `HH-§[BölümNo]-[SıraNo]` (HH = hedef doküman numarası)

### 1.3 Envanter çıktısı

Her kaynak için ayrı tablo + **toplam öğe sayısı**.

**Kural:** Envanter çıktıya yazılmadan ve toplam sayı belirtilmeden Faz 2'ye geçilmez.

---

## Faz 2 — Eşleştirme

Her envanter öğesi için hedefte karşılığını bul ve işaretle:

- **✓** — açık karşılığı var
- **⚠** — var ama eksik, belirsiz veya kısmi
- **✗** — karşılığı yok (GAP)

**Kural:** Hiçbir öğe durumu belirsiz bırakılamaz. Faz 2 sonunda her kaynak için `Toplam: N öğe (X ✓, Y ⚠, Z ✗)` yazılır ve **Faz 1'deki toplamla eşleşmelidir**.

---

## Faz 3 — Analiz

### 3.1 ✗ (GAP) öğeler
- **Ne eksik:** kaynakta ne tanımlı, hedefte ne yok
- **Seviye:** bu dokümanın kapsamında mı (High), sonraki dokümanın doğal sorumluluğunda mı (Medium)
- **Öneri:** nereye, ne eklenmeli

### 3.2 ⚠ (Kısmi) öğeler
- **Ne var, ne eksik** + seviye + öneri

### 3.3 ✓ öğeler — kalite denetimi

Her ✓ öğesi aşağıdaki sorularla denetlenir. **Her soru her öğe için geçerli değildir** — yalnız ilgili olanları uygula:

| Soru | Ne zaman |
|---|---|
| **Tutarlılık:** Kaynakla birebir aynı mı? Terimler, sayısal değerler, sıralama eşleşiyor mu? | Her zaman |
| **Yeterlilik:** Bu bilgiyle geliştirici uygulamaya başlayabilir mi, yoksa "ama nasıl?" mı diyecek? | Mekanizma/akış/teknik karar içerenler |
| **Güvenlik:** Dış etkileşim, veri saklama veya yetkilendirme içeriyorsa ele alınmış mı? | Dış iletişim, hassas veri, auth |
| **Dayanıklılık:** Bu bileşen/bağımlılık çökerse ne olur? Hata senaryosu tanımlı mı? | Bileşen, servis, dış bağımlılık |
| **Veri bütünlüğü:** Bu veri nerede oluşuyor, doğrulanıyor, kullanılıyor? Yaşam döngüsü izlenebilir mi? | Veri akışı/dönüşümü |

**Sorun yoksa bulgu üretme.** "Sorun yok" geçerli ve istenen bir sonuçtur.

Her bulgu için: envanter ID'si + seviye (Critical / High / Medium / Low) + somut öneri.

---

## Çıktı formatı

```
## Audit Raporu — [Hedef Doküman]
**Tarih:** … **Hedef:** … **Bağlam:** … **Odak:** [veya "Tam denetim"]

### Envanter Özeti
| Kaynak | Toplam | ✓ | ⚠ | ✗ |
|---|---|---|---|---|
| … | | | | |
| **Toplam** | | | | |

### Envanter ve Eşleştirme Detayı
[her kaynak için Faz 1 + Faz 2 birleşik tablosu]

### Bulgular
| # | Envanter ID | Tür | Seviye | Bulgu | Öneri |
|---|---|---|---|---|---|

### Aksiyon Planı
**Critical:** …  **High:** …  **Medium:** … → [hangi dokümanda]  **Low:** …
```

**Seviye tanımları:**
- **Critical** — güvenlik açığı, veri kaybı riski, temel işlevsellik eksikliği; düzeltilmeden ilerlenmez
- **High** — bu dokümanda düzeltilmeli
- **Medium** — bu veya sonraki dokümanda ele alınabilir
- **Low** — farkındalık yeterli

---

## Kurallar

1. **Envanter her şeyden önce gelir.** Faz sırası kesindir.
2. **Her öğenin durumu işaretlenir.** Belirsiz bırakılamaz.
3. **Sayılar tutmalı.** Faz 1 toplamı = Faz 2 eşleştirme sayısı. Eşit değilse öğe atlanmıştır.
4. **"Sorun yok" geçerli bir sonuçtur.** Bulgu üretme zorunluluğu yoktur.
5. **Seviye ayrımını doğru yap.** Her şey Critical değildir.
6. **Sonraki dokümanların kapsamını bil.** Eksiklik sonraki dokümanın doğal kapsamındaysa Medium + hedef doküman belirtilir.
7. **Pozitif bulguları envanter özeti gösterir** — ayrı "iyi yapılmış" listesi gerekmez.
8. **Hedefin kendi iç tutarlılığı da envanterin parçasıdır.**
