# Cross-Review — 01 Project Vision (Tur 1)

**Tarih:** 2026-10-02 · **Hedef:** `Docs/01_PROJECT_VISION.md` v0.9 · **İkinci model:** `cursor-agent` (CLI, `--mode ask`, salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`git show e74883f:Docs/01_PROJECT_VISION.md`), izole bir çalışma alanında — karar kaydı verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde — pre-commit sır guard'ı düz metindeki bir sözcüğü sır ataması sanıyordu):

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §5.3 (satır 180) ↔ sürüm notu v0.8 (satır 21)
> Alıntı: "kurulumdan yalnız sır ve altyapı gelir — alan adı, e-posta gönderim kimliği, ödeme sağlayıcı anahtarları, Google uygulamasının kimlik bilgileri ve firmanın kendi barındırması" / "§5.3'ün kurulumdan gelenler listesi dörde çıktı"
> Sorun: v0.8 notu listeyi dört kaleme indirdiğini söylüyor; gövde beş kalem sayıyor (alan adı, e-posta, ödeme anahtarları, Google, barındırma).
> Öneri: Ya gövdeyi dört kaleme indir (muhtemelen alan adını bu “sır ve altyapı” listesinden çıkarıp yalnız Ü-1 ön koşullarında bırak) ya da sürüm notundaki sayıyı beşe düzelt; ikisini aynı sayıda sabitle.
> 
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §3.2 aktör tablosu — Misafir alıcı (satır 96)
> Alıntı: "Neden kullanır: … siparişini sipariş numarası ve e-postasıyla takip etmek" / "Neden geri döner: Geri dönmesi beklenmez: … bir kez alır, gider (K-100)"
> Sorun: Aynı satırda sipariş takibi için siteye yeniden gelmek kullanım gerekçesi iken “geri dönmesi beklenmez” deniyor; takip ziyareti ile “bir kez alır, gider” çelişiyor.
> Öneri: “Neden geri döner” hücresini netleştir: tekrar satın alma/süreklilik beklenmez; sipariş durumu görmek için numarası+e-posta ile dönüş vardır (veya takibi “neden kullanır” içinde aynı oturumla sınırla).
> 
> BULGU-3
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: §6 Başarı kriterleri — M-1…M-4 (satır 209–212); şablon §6 “Hedef”
> Alıntı: "Ödeme tamamlama oranı … | … | İlk gerçek kurulumun ilk üç ayı | Hayır" (M-2, M-3, M-4’te de hedef yok)
> Sorun: Şablon “Başarılı sayılması için ne olmalı?”ya sayısal Hedef ister. Mağaza düzeyi ölçüler toplanıyor ama hiçbirinde eşik/hedef yok; post-MVP’yi neyin besleyeceği belirsiz.
> Öneri: M-1…M-4 için sayısal hedef veya en azından “iyi/kötü” eşikleri ekle (ör. M-1 ≥ x%, M-3 sözün ≥ y%’i); kabul kapısı olmasalar da ölçümün karar kuralını yaz.
> 
> BULGU-4
> Kriter: Güvenlik
> Seviye: Yüksek
> Yer: §2 (satır 64); §3.2 Misafir alıcı (satır 96)
> Alıntı: "misafir alıcı siparişini sipariş numarası ve e-postasıyla izler"
> Sorun: Sipariş erişimi yalnız numara + e-posta ile tanımlanmış; numaranın tahmin edilemez / yüksek entropili olduğu veya ek sır (token/link) gerektiği yazılmıyor. Tahmin edilebilir numarada başkasının sipariş/KVKK kapsamındaki verisine erişim riski ürün iddiası olarak açıkta.
> Öneri: Vizyonda misafir takibinin tahmin edilemez sipariş kimliği veya tek kullanımlık/ gizli erişim bağı ile yapılacağını yaz; e-posta + sıradan sıra numarasının yeterli olmadığını belirt.
> 
> BULGU-5
> Kriter: Edge case
> Seviye: Orta
> Yer: §6 Ü-3 (satır 206)
> Alıntı: "karışık siparişte hatların adımları toplanır" / "havale ile ödenen hizmet ve dijital siparişe \"ödendi\" işareti birer adım ekler"
> Sorun: “Ödendi” sipariş düzeyinde bir kez yapılan iş; karışık (ör. fiziksel + dijital) havale sepetinde bu adımın hat başına mı yoksa sipariş başına bir mi sayılacağı tanımlı değil; toplamın Ü-3 okuması belirsiz kalır.
> Öneri: Karışık + havale için kuralı yaz: “ödendi” sipariş başına 1 sayılır (hatlara yayılmaz) veya hangi hatlara ekleneceğini tek cümleyle sabitle.
> 
> BULGU-6
> Kriter: Edge case
> Seviye: Orta
> Yer: §3.2 (satır 103); §7 S-6 (satır 238)
> Alıntı: "6502 sayılı Kanun'un tüketici korumaları istisnasız her siparişte uygulanır" / "Alıcı tüketicidir"
> Sorun: Ticari/mesleki amaçla satın alan kişi (TTK bağlamında tacir vb.) engellenmiyor veya ayrılmıyor; yine de her siparişte istisnasız tüketici rejimi uygulanıyor deniyor — hukuken tüketici olmayan alıcıda sonuç belirsiz.
> Öneri: Ya alıcının tüketici olduğu varsayımını/onayını (veya ticari alımı reddini) ürün davranışına bağla ya da “istisnasız”i “ürün tüketici siparişi olarak işler; ticari alıcı ayrımı yapılmaz, risk firmada” diye sınırla.
> 
> SONUÇ: 6 BULGU
> ```

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ⚠️ KISMİ | Sorun gerçek ama yeri yanlış adlandırılmış: v0.8 notu kendi anını doğru anlatıyor (K-478 kurulum **ayarlarını** dörde çıkardı); sayıyı bozan, v0.9'da deep review bulgusu DR-5 ile listeye eklenen barındırmadır. Barındırma bir kurulum ayarı değil, kurulumun çalıştığı altyapıdır (K-358). Tarihçe notu değiştirilmez. | §5.3: "dört kurulum ayarı — … — ve kurulumun çalıştığı, firmanın kendi barındırması (K-358)" |
| BULGU-2 | ✅ KABUL | Aynı satırda "takip etmek için kullanır" ile "geri dönmesi beklenmez" yan yana duruyor. K-100'ün "bir kez alır, gider" cümlesi süreklilik (yeniden satın alma) hakkındadır, sipariş takibini dışlamaz. | §3.2: "Yeniden satın almak için geri dönmesi beklenmez … Siteye yalnız o siparişin durumunu görmek için döner." |
| BULGU-3 | ⚠️ KISMİ | Sayısal hedef konmaz: mağaza düzeyi ölçüler kabul kapısı değildir (K-418) ve şablonun "Hedef" sütunu K-433 ile bilinçli olarak "Kabul kapısı mı" sütununa çevrildi. Gerçek olan, hedef yokluğunun dokümanda söylenmemesi ve "düşük/yüksek" eşiğinin sahipsiz görünmesi. Eşik zaten tracker A-13 olarak `10`'un döngüsüne açık. | §6: "Mağaza düzeyi ölçüler hedef değer taşımaz … eşik `10 §5`'in işidir (K-423)." |
| BULGU-4 | ⚠️ KISMİ | Ürün düzeyinde açık yok: sipariş numarası tahmin edilemez bir koddur (K-185, `02 §3.22.1`) ve başarısız takip sorgusu IP başına sınırlıdır (K-330, `02 §8` L-5). Bu bilgi `01`'de olmadığı için iddia tek başına okununca eksik görünüyor; mekanizma `02`'nin işidir, vizyona tek nitelik yeter. "Tek kullanımlık bağ" önerisi yeni bir karar olurdu ve gerekmez. | §2: "tahmin edilemez sipariş numarası ve e-postasıyla izler (K-97, K-185)" |
| BULGU-5 | ✅ KABUL | v0.9'da Ü-3'e eklenen "birer adım ekler" cümlesi karışık siparişte "ödendi"nin her hatta mı sayıldığını açık bırakıyordu. K-454 örneği (havale ile ödenen fiziksel + hizmet = 4 adım) adımın sipariş başına bir kez sayıldığını gösteriyor. | Ü-3: "'Ödendi' sipariş düzeyinde bir kez yapılır: karışık siparişte hatlara dağıtılmaz, bir kez sayılır (K-454)." |
| BULGU-6 | ⚠️ KISMİ | Sonuç belirsiz değil: K-07 her siparişe tüketici rejimini uygular — ticari amaçla alan biri de daha koruyucu rejimi alır; K-112 kurumsal fatura alanı açmaz. Önerilen onay kutusu ya da reddetme mekanizması yeni bir ürün kararı olurdu ve K-07'nin "dallanmadan" gerekçesine aykırıdır. Eksik olan, ürünün sıfatı sorgulamadığının yazılmaması. | §3.2: "Ürün alıcının sıfatını sorgulamaz ve kurumsal fatura alanı açmaz: … her sipariş tüketici siparişi olarak işler (K-07, K-112)." |

**Dağılım:** 2 KABUL · 4 KISMİ · 0 RET. Ret çıkmadı çünkü altı bulgunun altısında da metinde gerçek bir okuma sorunu vardı; dördünde önerilen çözüm yeni bir karar ya da yanlış yer gerektirdiği için alternatif uygulandı.

## 3. Ek bulgular

Yok. Bulgular okunurken `01`'in değişen yerleri yeniden tarandı; yeni bir tutarsızlık görülmedi.

## 4. Kullanıcı onay checklist'i

Proje sahibi oturumun açılışında "doğrulanan bulguları `01`'e uygula; cross-review TEMİZ dönene kadar okut" talimatını verdi. Altı düzeltme de bu yetkiyle `01` v0.10'da uygulandı. Hiçbiri yeni bir karar satırı açmadı.

- [x] BULGU-1 · [x] BULGU-2 · [x] BULGU-3 · [x] BULGU-4 · [x] BULGU-5 · [x] BULGU-6
