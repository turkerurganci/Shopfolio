# Cross-Review — 01 Project Vision (Tur 2)

**Tarih:** 2026-10-02 · **Hedef:** `Docs/01_PROJECT_VISION.md` v0.10 · **İkinci model:** `cursor-agent` (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu, yeni ve izole bir çalışma alanında; model önceki turu görmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden. Model son bulguyu ve sonuç satırını cevabın başında bir kez daha tekrar etti; tekrar olduğu gibi bırakıldı.

> ```text
> Denetimi tamamladım. Bulgular aşağıda.
> 
> BULGU-1
> Kriter: Belirsizlik
> Seviye: Yüksek
> Yer: §7 S-10
> Alıntı: "| # | Shopfolio ... değildir |" / "S-10 | **Tek pazara ve tek para birimine bağlıdır.** Türkiye'ye satar ve TRY ile çalışır; yurt dışına satış ve çok para birimi yoktur."
> Sorun: Sütun başlığı her satırı "Shopfolio … değildir" kalıbına sokar; S-1…S-9 bu kalıpla yazılmıştır. S-10 ise olumlayıcıdır. Başlığa göre okunursa anlam tersine döner: "tek pazara bağlı değildir" — oysa metin tek pazar/TRY bağlılığını kalıcı sınır yapar.
> Öneri: S-10 başlığını diğerleriyle hizala; örneğin "**Çok pazarlı / çok para birimli ticaret sistemi değildir.** Türkiye'ye satar ve TRY ile çalışır…"
> 
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §6 Ü-1; §2
> Alıntı: "dış ön koşullar: alan adı (K-20), barındırma ve konumu (K-358), e-posta gönderim yapılandırması (K-378), firmanın ödeme sağlayıcı sözleşmesi (K-12), ETBİS kaydı (K-14), Google uygulaması (K-103) ve ilk yönetici hesabı (K-308)"
> Sorun: Ü-1, ÖK-1…ÖK-11'in tamamlanmasını ve bu listede Google uygulamasını ilk sipariş için dış ön koşul sayar. §2 ise Google girişini isteğe bağlı bırakır: "kurulumda Google uygulaması tanımlıysa **Google** hesabıyla da giriş yapar". Misafir/kartlı sipariş Google olmadan mümkünken kurulabilirlik kabulü Google'ı zorunlu gibi yazar.
> Öneri: Google'ı Ü-1 dış ön koşullarından çıkar veya "kullanılacaksa yapılandırılır; ilk sipariş için zorunlu değildir" diye açıkça ayır.
> 
> BULGU-3
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §3.2
> Alıntı: "6502 sayılı Kanun'un tüketici korumaları istisnasız her siparişte uygulanır (K-07). Ürün alıcının sıfatını sorgulamaz ve kurumsal fatura alanı açmaz: ticari amaçla alan biri de olsa her sipariş tüketici siparişi olarak işler (K-07, K-112)."
> Sorun: 6502 tüketici işlemlerine uygulanır; alıcı tacir/ticari alıcıysa kanun kendiliğinden her siparişe "istisnasız uygulanır" denemez. Ürünün her siparişe tüketici koruması tanıması ürün politikasıdır; bununla kanunun kapsamı aynı şey değildir.
> Öneri: "Kanun uygulanır" yerine "ürün, alıcı sıfatına bakmadan her siparişe tüketici koruması rejimini uygular; hukuki nitelendirme ürün politikasıdır" yaz.
> 
> BULGU-4
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §3.1; §7 S-10
> Alıntı: "Pazar | Türkiye; tek dil Türkçe, tek para birimi TRY (K-16)" / "Çoklu dil bu sınırın parçası değildir: yol haritası adayıdır"
> Sorun: §3.1 tek dili TRY ve Türkiye ile aynı hücrede, aynı K-16 altında sabit pazar tanımı gibi sunar. S-10 ise çoklu dilin bu kalıcı sınırın parçası olmadığını, yol haritası adayı olduğunu söyler. Dilin kalıcılık düzeyi iki bölümde çelişir.
> Öneri: §3.1'de dili ayır: "Pazar/para: Türkiye, TRY (kalıcı — S-10); dil: MVP'de Türkçe, çoklu dil yol haritası adayı (S-10)."
> 
> BULGU-5
> Kriter: Eksiklik
> Seviye: Orta
> Yer: §4.2; §4.3
> Alıntı: "**Değer: kesintisiz yolculuk.** Ziyaretçi firmayı tanıdığı yerden ürüne, üründen satın almaya kanal değiştirmeden geçer." / "**Değer: hesap sürekliliği.** Üye müşterinin hesabı siparişleri arasında sürer…"
> Sorun: Şablon §4 "Her aktör için 'bu ürün olmasaydı ne olurdu'" ister. §4.1 firma için karşı-olgu sütunu vardır; §4.2 ve §4.3 yalnız olumlu değeri yazar, "olmasaydı ne olurdu" karşı-olgusunu yazmaz.
> Öneri: Ziyaretçi ve üye müşteri için firma tablosundaki gibi kısa karşı-olgu cümlesi ekle (ör. kanal kopması; her seferinde baştan sipariş / dağınık takip).
> 
> SONUÇ: 5 BULGU
> ```

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ✅ KABUL | §7 tablosunun sütunu "Shopfolio ... değildir" diyor; S-10 tek olumlayıcı başlıktı ve sütunla okununca anlamı tersine dönüyordu. İçerik (K-16, K-439) değişmedi. | S-10: "**Çok pazarlı ve çok para birimli bir satış sistemi değildir.**" |
| BULGU-2 | ⚠️ KISMİ | Google uygulamasını Ü-1'den çıkarmak yanlış olurdu: K-478 onu `10 §4.1`'de **koşullu** ön koşul yaptı ve referans kurulumda tamamlanmasını istedi; Ü-1 de ÖK-1…ÖK-11'in tamamını sınar (K-477). Gerçek olan, koşulluluğun Ü-1'de görünmemesi. | Ü-1: "Google uygulaması (K-103 — koşullu ön koşuldur: tanımlı değilse yalnız Google ile giriş görünmez ve sipariş engellenmez; referans kurulumda tamamlanır, K-478)" |
| BULGU-3 | ✅ KABUL | Hukuken doğru: 6502 tüketici işlemlerine uygulanır; ticari alıcıda kanun kendiliğinden uygulanmaz. K-07'nin özü bir **ürün politikasıdır** — ürün herkese tüketici rejimini uygular. Önceki deep review'da benzer bir bulgu (YS-2) cümlenin K-07'nin birebiri olduğu gerekçesiyle çürütülmüştü; ama tur 1'de eklenen "ticari amaçla alan biri de olsa" cümlesi ifadeyi artık hukuki bir iddiaya çeviriyordu. Karar değişmedi, yalnız özne düzeltildi. | §3.2: "ürün 6502 sayılı Kanun'un tüketici korumalarını istisnasız her siparişe uygular … Bu bir ürün politikasıdır — kanunun kapsamını genişletmez, alıcıya ondan daha az koruma tanımaz." |
| BULGU-4 | ✅ KABUL | §3.1 dili pazar ve para birimiyle aynı hücrede tutuyordu; S-10 ise çoklu dilin kalıcı sınırın parçası olmadığını söylüyor (K-439). | §3.1 Pazar satırı: "Türkiye ve tek para birimi TRY — kalıcı sınırdır (§7 S-10); dil Türkçedir, çoklu dil yol haritası adayıdır (K-16, K-439)" |
| BULGU-5 | ✅ KABUL | Şablon §4 her aktör için "bu ürün olmasaydı ne olurdu"yu istiyor; §4.1 bunu tabloyla yazıyor, §4.2 ve §4.3 yazmıyordu. Eklenen cümleler yeni içerik değildir: ziyaretçininki D-2'nin karşı-olgusu, üye müşterininki K-19'un "daha az adım, tek yerden takip" cümlesinin tersidir. | §4.2 ve §4.3'e birer "Bu ürün olmasaydı" cümlesi |

**Dağılım:** 4 KABUL · 1 KISMİ · 0 RET.

## 3. Ek bulgular

Yok.

## 4. Kullanıcı onay checklist'i

Oturum açılışındaki talimat gereği uygulandı (`01` v0.11). Yeni karar satırı açılmadı.

- [x] BULGU-1 · [x] BULGU-2 · [x] BULGU-3 · [x] BULGU-4 · [x] BULGU-5
