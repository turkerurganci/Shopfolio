---
name: cross-review
description: "Bir dokümanı bağımsız ikinci bir AI'ya okutup bulguları yansıtır; audit ve deep review'dan sonra çalışır. Kullan: 'cross-review', 'ikinci görüş', /cross-review Docs/XX_....md."
user-invocable: true
---

# Cross Review — Bağımsız İkinci AI Review Döngüsü

> **Ne zaman:** Bir dokümanın audit'i ve deep review'ı tamamlandıktan sonra.
> **Tetikleme:** "cross-review", "ikinci görüş" veya `/cross-review Docs/XX_....md`.
>
> **Temel fark:** Audit ve deep review **aynı ajanın iç denetimi**dir. Cross review, dokümanı **farklı bir modele** okutarak birinci ajanın kaçırdığını yakalar. Farklı model hem avantajdır (taze göz) hem dezavantaj (proje bağlamını bilmez).

## Parametreler

| Parametre | Zorunlu | Açıklama |
|---|---|---|
| `hedef` | Evet | Review edilecek doküman yolu |
| `round` | Hayır | Kaçıncı tur (varsayılan 1) |

---

## Faz 1 — İkinci modele gönder

1. Dokümanı ikinci modele gönder (SETUP'ta tanımlanan yöntemle: script, web arayüzü veya API).
2. **Yedi kriter** istenir: tutarlılık · eksiklik · belirsizlik · teknik doğruluk · edge case · güvenlik · kullanıcı deneyimi.
3. Çıktı yapılandırılmış olmalı: `BULGU-N: …` veya `SONUÇ: TEMİZ`.
4. Ham çıktı `Docs/CROSS_REVIEW_REPORTS/XX_CROSS_REVIEW[_RN].md` dosyasına yazılır.
5. **Çıktıyı tam oku** — her bulguyu not al.

---

## Faz 2 — Bağımsız değerlendirme

> **KRİTİK KURAL:** Her bulguya otomatik "katılıyorum" deme. **Rubber stamp yasaktır.**

Her bulgu için:

1. **Dokümanı bizzat kontrol et** — işaret edilen bölümü oku.
2. **Proje bağlamını değerlendir** — ilgili diğer dokümanları kontrol et. İkinci modelin bunlara erişimi olmadığı için bağlam kaçırmış olabilir.
3. **Karar ver:**
   - ✅ **KABUL** — haklı, düzeltme gerekli; düzeltme önerisi sun
   - ❌ **RET** — yanlış veya bağlamı kaçırıyor; **somut gerekçe zorunlu** (doküman bölümü, proje kararı — "ben böyle düşünüyorum" yetersiz)
   - ⚠️ **KISMİ** — sorun gerçek ama önerilen çözüm uygun değil; alternatif sun
4. **Kaçırdıklarını da raporla** — ikinci modelin listesiyle sınırlı kalma; okurken fark ettiklerini "Ek Bulgular" olarak ekle.

**Objektivite kuralları:**
- **%100 KABUL şüphelidir.** Her zaman kendi analizini yap.
- **%100 RET de şüphelidir.** Savunmacılık, rubber stamp'in aynadaki hâlidir.
- RET gerekçesi somut referans olmalıdır.

---

## Faz 3 — Sunum

1. Rapor dosyasındaki "Bağımsız Değerlendirme" tablosunu doldur.
2. Kullanıcıya özet sun: kaç bulgu geldi, kaçı kabul/ret/kısmi, her biri için kısa açıklama ve karar.
3. Ek bulguları da sun.
4. **Hangi düzeltmelerin uygulanacağına kullanıcı karar verir.**

---

## Faz 4 — Düzeltme ve tekrar

1. Onaylanan düzeltmeleri uygula, doküman versiyonunu yükselt.
2. `--round N+1` ile tekrar gönder.
3. Faz 1'den itibaren tekrarla.
4. **Çıkış koşulu:** İkinci model `SONUÇ: TEMİZ` döndüğünde döngü biter.

---

## Faz 5 — Etki yansıtma (ATLANAMAZ)

> İkinci model her dokümanı **izole** okur; cross-document uyumsuzlukları yakalamaz. Bu adım o boşluğu kapatır.

TEMİZ sonrası:

1. **Downstream tarama** — bu dokümanı bağımlılık olarak listeleyen dokümanlar: yapılan düzeltmeler oralarda karşılığını buldu mu?
2. **Upstream tarama** — bu dokümanın bağımlılıkları: cross-review sırasında alınan yeni kararlar kaynak dokümanları etkiliyor mu?
3. **Yeni alan/kural taraması** — cross-review sırasında eklenen her yeni alan, enum değeri, parametre, iş kuralı ilgili dokümanda tanımlı mı?
4. Uyumsuzlukları hedefli düzeltmelerle kapat; her düzeltme kaydedilir.

> **Vaka:** Bir kodlama kılavuzunun cross-review'ında eklenen üç yeni alan, veri modeli dokümanında tanımlı değildi. Ne audit ne cross-review yakaladı — **etki yansıtmanın ardından çalıştırılan checkpoint** yakaladı. Kalite döngüsünün tamamı gerekli.

---

## Rapor dosya yapısı

```
Docs/CROSS_REVIEW_REPORTS/
├── XX_CROSS_REVIEW.md      # Tur 1
├── XX_CROSS_REVIEW_R2.md   # Tur 2
└── …
```

Her rapor: ham bulgular · bağımsız değerlendirme tablosu (KABUL/RET/KISMİ + gerekçe) · ek bulgular · kullanıcı onay checklist'i · (son turda) etki yansıtma sonucu.
