# Cross-Review — 01 Project Vision (Tur 17)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/01_PROJECT_VISION.md` v0.25 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra` (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu, yeni ve izole bir çalışma alanında; model önceki turları görmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde).

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §2; §4.1 D-1
> Alıntı: "aynı bilgi ikinci bir yere girilmez." / "Firmanın SSS ya da sayfa metninde elle tekrarladığı bir ayar bilgisi ayar değişince çelişebilir"
> Sorun: §2, bilginin ikinci bir yere hiç girilmediğini kesin olarak söylerken D-1, ayar bilgisinin SSS veya sayfa metninde elle tekrar girilebildiğini ve çelişebildiğini kabul ediyor. Bu, çözümün tek giriş iddiasının kapsamını belirsizleştirir.
> Öneri: §2’deki iddiayı yapılandırılmış ürün, firma kimliği ve ayar kayıtlarıyla sınırlandır; serbest metinlerdeki tekrarların senkronize edilmediğini açıkça belirt.
> 
> BULGU-2
> Kriter: Edge case
> Seviye: Orta
> Yer: §6, M-3
> Alıntı: "Fiziksel siparişte ödemenin onaylandığı andan kargoya verilene geçen sürenin, vitrinde verilen kargoya verme sözüne uyan siparişlerin oranı."
> Sorun: Sipariş verildikten sonra vitrindeki kargoya verme sözü değiştirilirse M-3’ün siparişi eski sözle mi, yeni sözle mi değerlendireceği tanımlı değildir. Bu, aynı siparişin ölçüm sonucunu sonradan değiştirebilir.
> Öneri: M-3’ün, sipariş anında müşteriye gösterilen kargoya verme sözünün siparişe sabitlenen sürümüne göre hesaplandığını yaz.
> 
> BULGU-3
> Kriter: Güvenlik
> Seviye: Yüksek
> Yer: §3.2, Misafir alıcı satırı
> Alıntı: "Hesap açarsa doğrulanmış e-postasına ait geçmiş misafir siparişleri hesabına düşer" / "adres el değiştirmişse eski siparişlerin yeni sahibine görünmesi bilinçle kabul edilmiş bir risktir"
> Sorun: E-posta adresinin sonradan başka bir kişiye geçmesi, geçmiş sipariş, adres ve iletişim verilerinin siparişi vermemiş kişiye açıklanmasına yol açar. Kabul edilmiş risk olarak yazılması, kişisel veriye yetkisiz erişim sonucunu ortadan kaldırmaz.
> Öneri: Geçmiş misafir siparişlerini yalnız e-posta eşleşmesiyle otomatik bağlama; siparişe özgü ek doğrulama, sınırlı süreli bağlama veya geçmiş siparişlerin varsayılan olarak görünmemesi gibi bir yetkilendirme kuralı tanımla.
> 
> SONUÇ: 3 BULGU
> ```

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ✅ KABUL | §2 tek giriş iddiasını mutlak kuruyordu; D-1 aynı dokümanda serbest metindeki tekrarın çelişebildiğini kabul ediyor (K-364). İddia, ürünün kayıtlı bilgisiyle sınırlanınca ikisi uyuşuyor. | §2: "… ürün, firma kimliği ve ayarlar için ikinci bir kayıt yeri yoktur. Firmanın serbest metinde — ör. SSS'de — elle tekrarladığı bilgi ise ayarla eşitlenmez (D-1)." |
| BULGU-2 | ✅ KABUL | M-3 satırı "vitrinde verilen söz" diyordu; söz sonradan değişirse hangisinin geçerli olduğu yazılı değildi. Kayıt cevabı veriyor: siparişin sözü sipariş onaylandığında donar (K-140) ve Ürün Gereksinimleri §10.6.3 ölçüyü "siparişin donmuş kargoya verme sözü" ile tanımlıyor. | M-3: "… sipariş anında vitrinde gösterilen ve siparişe donan kargoya verme sözüne uyan siparişlerin oranı; söz sonradan değişse de sipariş kendi sözüyle ölçülür." |
| BULGU-3 | ❌ RET | Dördüncü tekrar (tur 8, 10, 12, 17). Talimattaki "bilinçli karara yalnız katılmadığın için bulgu yazma" cümlesine rağmen geldi; ikinci model bunu kişisel veri sonucu olarak çerçeveliyor. Karar proje sahibinindir ve bu oturumda sorulup verildi (K-489): e-posta doğrulaması sektördeki olağan yöntemdir, adres devri nadirdir ve önerilen ek doğrulama K-98'in gerekçesini bozar. Dokümanda çelişki ya da olgusal hata yok; risk açıkça yazılı. | — |

**Dağılım:** 2 KABUL · 0 KISMİ · 1 RET.

## 3. Ek bulgular

Yok.

## 4. Kullanıcı onay checklist'i

Oturum açılışındaki talimat gereği uygulandı (`01` v0.26). Yeni karar satırı açılmadı.

- [x] BULGU-1 · [x] BULGU-2 · [x] BULGU-3
