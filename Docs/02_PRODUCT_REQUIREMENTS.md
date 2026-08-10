# <PROJE> — Product Requirements

**Versiyon: v0.1** | **Bağımlılıklar:** `01_PROJECT_VISION.md`, `PRODUCT_DISCOVERY_STATUS.md` | **Son güncelleme:** YYYY-AA-GG

> **Aşama:** 1 — Product Discovery · **Rol:** Product Manager
> **Traceability zorunlu:** Hayır (kaynak doküman — sonraki aşamalar buraya izlenir)
> **Bu doküman tüm iş kurallarının tek kaynağıdır.** Kod bu dokümanla çelişemez.

---

## 0. Nasıl kullanılır

- Her iş kuralı **numaralandırılır** ve bölüm referansıyla anılır (`§4.7`). Sonraki dokümanlar ve task'lar bu numaraya atıf yapar.
- Sayısal parametreler (süre, oran, limit, eşik) **tablo hâlinde** toplanır — dağınık yazılırsa tutarsızlık kaçınılmaz olur.
- **Admin esnekliği prensibi:** Rakamsal parametreleri yönetici tarafından değiştirilebilir yapmak, "doğru rakam ne?" tartışmasını ürün aşamasından çıkarır. Hangi parametrenin runtime ayarlanabilir olduğu burada belirtilir.
- "Muhtemelen", "belki", "sonra karar veririz" yasak. Detay ileriye bırakılabilir; **varlık kararı** burada alınır.

---

## 1. Terimler ve aktörler

> **Ne yazılır:** Projede kullanılan her terimin tek tanımı. Aynı kavram iki farklı isimle anılmamalı.

## 2. Temel akış

> **Ne yazılır:** Ürünün ana iş akışı, uçtan uca. Adım adım, dallanmasız. Detaylı akışlar `03_USER_FLOWS.md`'de.

## 3. İş kuralları

> **Ne yazılır:** Her kural ayrı madde. Kuralın **koşulu**, **sonucu** ve **istisnası**. Her kural bir sonraki dokümanda karşılığını bulacak şekilde somut olmalı.

### 3.x <Kural grubu>

## 4. Zaman kuralları

> **Ne yazılır:** Tüm süre/timeout kuralları **tek bölümde**. Aktör akışlarına gömülmez — dağıtılırsa tutarsızlık doğar.

| # | Kural | Süre | Süre dolunca ne olur | Ayarlanabilir mi |
|---|---|---|---|---|

## 5. Durum tanımları

> **Ne yazılır:** Ana varlıkların (işlem, hesap, talep vb.) alabileceği durumlar ve geçiş koşulları. Durum makinesi mantığı burada başlar, `03`'te akışa, `06`'da şemaya döner. **Bu isimler tüm dokümanlarda birebir aynı kalır.**

## 6. Hata ve istisna senaryoları

> **Ne yazılır:** Her ana akış için "ne ters gidebilir" ve sistemin cevabı. Happy path kadar detaylı.

## 7. İptal, itiraz ve geri alma

> **Ne yazılır:** Kullanıcının bir işlemi geri alma yolları, koşulları, sınırları.

## 8. Kötüye kullanım ve güvenlik kuralları

> **Ne yazılır:** Kötüye kullanım senaryoları ve karşı önlemler. Teknik güvenlik değil, **ürün düzeyinde** kurallar (limit, cooldown, doğrulama zorunluluğu).

## 9. Bildirimler

> **Ne yazılır:** Hangi olayda kime bildirim gider. Kanal detayı `08`'de; burada **tetikleyici ve alıcı**.

## 10. Yönetim (admin) kuralları

> **Ne yazılır:** Yöneticinin yapabildikleri ve yapamadıkları. **"Sonra ekleriz" yaklaşımı riskli** — yönetim akışları kullanıcı akışları kadar karmaşıktır, aynı derinlikte ele alınır.

## 11. Sayısal parametreler (özet)

> **Ne yazılır:** Dokümanın her yerine dağılmış sayıların tek tablosu. Diğer dokümanlar bu tabloyu referans alır.

| # | Parametre | Değer | Nerede kullanılır | Runtime ayarlanabilir |
|---|---|---|---|---|

## 12. Uyumluluk ve yasal

> **Ne yazılır:** Yasal yükümlülükler, veri saklama/silme hakları, erişim kısıtları.

## 13. Açık kararlar

> **Ne yazılır:** Bilinçli olarak ileriye bırakılan **detaylar** ve ne zaman karara bağlanacakları. Varlık kararı burada asla açık kalmaz.

| # | Konu | Ne belirsiz | Ne zaman karara bağlanır |
|---|---|---|---|

---

*<PROJE> — Product Requirements v0.1*
