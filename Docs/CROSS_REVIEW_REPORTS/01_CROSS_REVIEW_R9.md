# Cross-Review — 01 Project Vision (Tur 9)

**Tarih:** 2026-10-02 · **Hedef:** `Docs/01_PROJECT_VISION.md` v0.17 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra` (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu, yeni ve izole bir çalışma alanında; model önceki turları görmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde).

> ```text
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §3.2, “Aktör olmayanlar” / ticari-kurumsal alıcı maddesi
> Alıntı: "Alıcı yalnız tüketicidir" ve "ticari amaçla alan biri de olsa sipariş tüketici siparişi olarak işler."
> Sorun: Bu iki ifade, ticari amaçla alış yapan bir kişiyi ürün politikasıyla tüketici saymaktadır. Oysa tüketici sıfatı 6502 kapsamındaki işlemin ticari veya mesleki olmayan amaçla yapılmasına bağlıdır; ürün bunu tek taraflı olarak değiştiremez. Metnin hemen ardından gelen “kanunun kapsamını genişletmez” ifadesiyle de çelişir.
> Öneri: “Ürün, alıcıdan ticari/kurumsal alıcı bilgisi istemez ve tüketici akışını sunar; ancak alıcının tüketici sıfatı ve kanuni korumaların uygulanması ilgili mevzuata göre belirlenir” şeklinde düzeltin.
> 
> SONUÇ: 1 BULGU
> ```

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ✅ KABUL | Aynı cümle dördüncü kez geldi (tur 2, 4, 8, 9). Önceki turlarda ifade daraltıldı ama hep ürünün bir sıfat "verdiği" okunabiliyordu. Bu turun önerdiği ayrım hukuken doğru ve hiçbir kararı değiştirmiyor: ürünün yaptığı şey tek bir tüketici akışı sunmak ve ticari alıcı bilgisi istememektir (K-07, K-112); alıcının tüketici sayılıp sayılmadığını mevzuat belirler. K-07'nin özü — dallanmayan tek rejim — korunuyor. | §3.2: "Ürünün alıcısı tüketicidir … ürün tek bir akış sunar ve bu akış … korumaları her siparişte işletir. Ürün alıcıdan ticari ya da kurumsal alıcı bilgisi istemez … Alıcının hukuken tüketici sayılıp sayılmadığı ve kanuni korumaların ona uygulanıp uygulanmadığı ise ürünün değil, mevzuatın belirlediği bir şeydir." |

**Dağılım:** 1 KABUL · 0 KISMİ · 0 RET.

## 3. Ek bulgular

Yok.

## 4. Kullanıcı onay checklist'i

Oturum açılışındaki talimat gereği uygulandı (`01` v0.18). Yeni karar satırı açılmadı.

- [x] BULGU-1
