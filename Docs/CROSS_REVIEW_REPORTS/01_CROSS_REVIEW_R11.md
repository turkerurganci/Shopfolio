# Cross-Review — 01 Project Vision (Tur 11)

**Tarih:** 2026-10-02 · **Hedef:** `Docs/01_PROJECT_VISION.md` v0.19 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra` (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu, yeni ve izole bir çalışma alanında; model önceki turları görmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde).

> ```text
> BULGU-1
> Kriter: Belirsizlik
> Seviye: Orta
> Yer: §6, M-4
> Alıntı: "İlk gerçek kurulumun ilk üç ayı; iade takvimi — cayma penceresi (K-203) ve geri ödeme çatısı (K-209) — dolduktan sonra okunur; okunma anının kuralı `02 §10.6.3`'tedir (K-488)"
> Sorun: Dönem içindeki bir siparişin iade veya geri ödemesi ölçüm döneminden sonra sonuçlanırsa M-4’e girip girmeyeceği dokümanın kendi içinde belirlenmemiştir. Okunma kuralı başka bir dokümana bırakıldığı için oran tek başına hesaplanabilir değildir.
> Öneri: M-4 satırına, dönem içindeki siparişlerin hangi tarihe kadar sonuçlanan iptal ve iadelerinin orana dahil edileceğini açıkça yazın.
> 
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §8, V-4
> Alıntı: "Firma kendi pazarlamasını yapar: siteye siparişi firma getirir"
> Sorun: Bu varsayımın doğrulaması yalnız "günlük ortalama birin altında" sipariş sayısına bağlanmıştır. Düşük sipariş sayısı, firmanın pazarlama yapmadığını tek başına göstermez; ürün, fiyat, sezon, stok veya hedef müşteri gibi başka nedenlerden de doğabilir.
> Öneri: Doğrulamaya firmanın fiilen yürüttüğü pazarlama faaliyetini ölçen bir koşul ekleyin; sipariş hacmini bu koşuldan ayrı sonuç göstergesi olarak kullanın.
> 
> SONUÇ: 2 BULGU
> ```

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ✅ KABUL | Tur 7'de kural ayrıntısı `02`'ye bırakılmıştı, çünkü `01`'de yazılan sürümler her turda yeni bir kenar durumu doğuruyordu. Kural K-488'de artık oturdu (teslim edilmeyen sipariş dahil), bu yüzden kısa hâli satıra güvenle geri yazılabilir: oranın neyi saydığı `01`'in kendi başına okunduğunda da belli olmalı. | M-4 "Ne zaman ölçülür": dönemdeki siparişlerin, pencereler kapanana kadar gerçekleşen iptal ve iadeleri sayılır; oran bu pencereler kapandığında okunur; teslim edilmemiş sipariş beklenmez (K-488; ayrıntı `02 §10.6.3`). |
| BULGU-2 | ⚠️ KISMİ | Düşük sipariş hacmi pazarlamanın yapılmadığını tek başına göstermez; bu doğru. Ama pazarlama faaliyetini ölçen bir koşul ürünün içine konamaz: ürün ziyaretçi ve kanal ölçmez (K-397, K-400). Sebep sorusu, zaten kurulu olan ilk müşteri görüşmesine (V-7) bağlandı; yanlışlama kuralı (K-487) değişmedi. | V-4: "Düşük hacmin sebebi — pazarlamanın yapılmaması mı, ürün, fiyat ya da sezon mu — ürünün içinde ölçülemez (K-397); V-7'nin görüşmesinde firma sahibine sorulur …" |

**Dağılım:** 1 KABUL · 1 KISMİ · 0 RET.

## 3. Ek bulgular

Yok.

## 4. Kullanıcı onay checklist'i

Oturum açılışındaki talimat gereği uygulandı (`01` v0.20). Yeni karar satırı açılmadı.

- [x] BULGU-1 · [x] BULGU-2
