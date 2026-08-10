# <SERVİS ADI> — Entegrasyon Runbook

**Son güncelleme:** YYYY-AA-GG | **İlgili task:** TXX | **Spec:** [`../08_INTEGRATION_SPEC.md`](../08_INTEGRATION_SPEC.md) §x

> **Kural:** Bir dış servise **ilk kez bağlanan task**, aynı PR'da bu runbook'u da yazar.
> "Sonra yazarız" denen kurulum bilgisi, ikinci kez lazım olduğunda kaybolmuş olur.

---

## 1. Hesap açma

> Adım adım. Ekran adları ve menü yolları dahil — altı ay sonra kimse hatırlamaz.

## 2. Kimlik bilgisi alma

| Bilgi | Nereden alınır | Nerede saklanır | Rotasyon |
|---|---|---|---|

## 3. Plan / kota

| Plan | Limit | Ücret | MVP'ye yeter mi |
|---|---|---|---|

> **Dış varsayım kanıtı:** `<bu bilginin nereden ve ne zaman doğrulandığı — bağlantı veya komut çıktısı>`

## 4. Konfigürasyon

| Ayar | Değer | Nerede tanımlı |
|---|---|---|

## 5. Geri çağrım / webhook kurulumu

- **URL:**
- **İmza doğrulama:** (zorunlu — atlanamaz)
- **Yerelde test:** (tünel aracı vb.)
- **Tekrar gönderim davranışı:**

## 6. Test etme

```
<doğrulama komutu veya adımları>
```

**Beklenen çıktı:**

## 7. Sorun giderme

| Belirti | Olası sebep | Çözüm |
|---|---|---|

## 8. Kesinti davranışı

> Bu servis erişilemez olduğunda sistem ne yapar (00 §M — `05` ve `08`'deki kararla tutarlı olmalı).

## 9. Prod farkları

> Test/sandbox ile prod arasındaki farklar: farklı uç adresleri, farklı limitler, onay süreci gerektiren adımlar.
