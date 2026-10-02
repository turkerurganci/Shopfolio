# Cross-Review — 01 Project Vision (Tur 3)

**Tarih:** 2026-10-02 · **Hedef:** `Docs/01_PROJECT_VISION.md` v0.11 · **İkinci model:** `cursor-agent` (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu, yeni ve izole bir çalışma alanında (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde). Model ikinci bulguyu ve sonuç satırını cevabın başında bir kez daha tekrar etti.

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §6 — M-3
> Alıntı: "Sipariş işleme süresi — ödemenin onaylandığı andan kargoya verilene geçen süre" / "Vitrinde verilen kargoya verme sözüne uyum oranı olarak okunur" / "Payda, kargoya verme süresi dönem içinde dolan fiziksel siparişlerdir"
> Sorun: Kriter bir süre (gün/saat) olarak tanımlanıyor; ölçüm yöntemi ise uyum oranı (yüzde). Aynı satırda birim çelişiyor: tabloya yazılacak sayı süre mi, oran mı belirsiz. Payda yalnız fiziksel siparişi kapsarken kriter adı ve tanımı tüm siparişlere yayılıyor gibi duruyor; dijital ve hizmet hatlarında "kargoya verilene geçen süre" tanımsız kalıyor.
> Öneri: M-3'ü ya "kargoya verme sözüne uyum oranı" diye yeniden adlandırıp tanımı oran olarak düzelt; ya da ortalama/medyan işleme süresi olarak bırakıp ölçümü süre birimiyle yaz. Her iki durumda da kapsamı "fiziksel siparişler" diye açıkça sınırla.
> 
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §8 — V-1
> Alıntı: "KOBİ ölçeği: fiziksel üründe katalog birkaç yüz ürüne kadardır" / "yayındaki fiziksel ürün sayısı 1.000'i aşarsa ya da katalog bir ürünü veya kategori dalını iki seçenek boyutu ya da üç kategori seviyesi yüzünden bölmeyi gerektirirse yanlışlanır"
> Sorun: Varsayım yalnızca katalog büyüklüğüdür (ürün adedi). Doğrulama kuralının ikinci kolu ise seçenek boyutu ve kategori derinliği sınırına bağlanıyor; bu, §3.1'deki yapı kararlarıyla ilgili ayrı bir iddia. Yapı sınırı aşılınca "birkaç yüz ürün" varsayımı yanlışlanmış sayılıyor — ölçek ile katalog modeli aynı satırda karışmış.
> Öneri: V-1 doğrulamasını yalnız ürün adedine indir (ör. 1.000 eşiği). Seçenek boyutu / kategori seviyesi için ayrı varsayım satırı aç veya mevcut bir V satırına taşı.
> 
> SONUÇ: 2 BULGU
> ```

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ✅ KABUL | M-3'ün adı K-416'dan "sipariş işleme süresi" olarak geliyor; ölçünün kendisi ise `02 §10.6.3`'te "kargoya verme sözüne uyum oranı" ve paydası K-484 ile fiziksel siparişlere bağlı. Satır bir süre adlandırıp bir oran ölçüyordu ve dijital ile hizmet siparişinin durumu yazılı değildi. Kararın adı korunarak ölçüyle birleştirildi. | M-3: "**Sipariş işleme süresi — kargoya verme sözüne uyum oranı.** … ölçünün değeri bir süre değil, orandır. Dijital ve hizmet siparişinde kargoya verme olmadığı için bu ölçüye girmez." |
| BULGU-2 | ⚠️ KISMİ | Sorun gerçek: v0.9'da K-487 ile eklenen ikinci kol, varsayım metninde olmayan bir şeyi yanlışlıyordu. Ama önerilen çözüm — kolu ayrı bir satıra taşımak — K-475'i bozar: K-475 `10 §4.2` SK-6'nın tetikleyicisini açıkça "V-1'in ölçümü"ne bağladı. Doğru düzeltme varsayımı, ölçtüğü şeyi adıyla taşıyacak biçimde yazmak; yeni satır açılmaz. | V-1: "**KOBİ kataloğu:** … birkaç yüz ürüne kadardır ve ürünün katalog yapısına — ürün başına en fazla iki seçenek boyutu, en fazla üç seviyeli kategori ağacı — sığar (K-40, K-42)." |

**Dağılım:** 1 KABUL · 1 KISMİ · 0 RET.

## 3. Ek bulgular

Yok.

## 4. Kullanıcı onay checklist'i

Oturum açılışındaki talimat gereği uygulandı (`01` v0.12). Yeni karar satırı açılmadı.

- [x] BULGU-1 · [x] BULGU-2
