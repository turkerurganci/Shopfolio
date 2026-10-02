# Cross-Review — 01 Project Vision (Tur 13)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/01_PROJECT_VISION.md` v0.21 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra` (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu, yeni ve izole bir çalışma alanında; model önceki turları görmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde).

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §3.2 — Aktör olmayanlar
> Alıntı: "Ürün alıcıdan ticari ya da kurumsal alıcı bilgisi istemez ve kurumsal fatura alanı açmaz; ticari amaçla alan biri de aynı akıştan geçer ve aynı sözleşmeyi kurar."
> Sorun: Ticari amaçla alışverişe izin verilirken ticari fatura bilgileri alınmıyor. Bu, §7 S-4’teki “sistem faturanın gerektirdiği veriyi tam taşır ve dışa aktarır” iddiasını ticari alıcılar için karşılanamaz hâle getirir.
> Öneri: Ticari alıcıları satın alma akışından açıkça dışla veya ticari fatura için gerekli bilgilerin alınacağı ve dışa aktarılacağı akışı tanımla.
> 
> BULGU-2
> Kriter: Edge case
> Seviye: Orta
> Yer: §8, V-3 — Nasıl doğrulanacak
> Alıntı: "İlk üç ayda ödemesi tamamlanmış siparişlerin %5'inden azı havale ile ödenmişse yanlışlanır."
> Sorun: İlk üç ayda hiç ödemesi tamamlanmış sipariş yoksa oranının paydası sıfırdır; V-3 doğrulanmış ya da yanlışlanmış sayılamaz. Aynı sıfır-payda durumu §6’daki M-1, M-3 ve M-4 oranları için de tanımlanmamıştır.
> Öneri: Her oran için sıfır payda sonucunu açıkça tanımla; örneğin “ölçü yok / değerlendirilemez” olarak göster ve V-3’te bu durumda görüşme veya sonraki ölçüm dönemine erteleme kuralı koy.
> 
> BULGU-3
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §8, V-4 — Nasıl doğrulanacak
> Alıntı: "İlk üç ayın sipariş sayısı: günlük ortalama birin altında kalırsa yanlışlanır. Düşük hacmin sebebi — pazarlamanın yapılmaması mı, ürün, fiyat ya da sezon mu — ürünün içinde ölçülemez."
> Sorun: İlk cümle düşük siparişi doğrudan “firma kendi pazarlamasını yapar” varsayımını yanlışlayan koşul yapar; ikinci cümle ise düşük siparişin nedeninin pazarlama olup olmadığının ölçülemediğini kabul eder. Düşük hacim tek başına pazarlamanın yapılmadığını göstermez.
> Öneri: Düşük hacmi yanlışlama yerine görüşme tetikleyicisi yap; varsayımın yanlışlanmasını, firma sahibinin pazarlama faaliyeti yapmadığını doğrulayan görüşme bulgusuna bağla.
> 
> BULGU-4
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §8, V-2 — Yanlışsa ne olur
> Alıntı: "Elle adımlar darboğaz olur: varsayılan tepe günde bile en ağır hatta 150 panel işlemi, tek kişinin yaklaşık bir saatlik işidir."
> Sorun: Bu hesap 50 siparişin her birinde en fazla üç adımlı tek bir hat bulunduğunu varsayar. §6 Ü-3 ise karışık siparişte hat adımlarının toplandığını ve toplamın tavana bağlı olmadığını söyler; örneğin havale ile fiziksel ürün ve hizmet içeren bir sipariş dört adımdır. Sipariş başına hat sayısı sınırlandırılmadığı için 150 toplam işlem üst sınırı değildir.
> Öneri: V-2’ye sipariş başına hat bileşimi için açık bir varsayım ekle veya 150 işlemi “50 tek hatlı havale + fiziksel sipariş” örneği olarak yeniden ifade et.
> 
> BULGU-5
> Kriter: Eksiklik
> Seviye: Orta
> Yer: §8, V-5 — Nasıl doğrulanacak
> Alıntı: "Abonelik sözleşmesi (K-18): verinin firmada kaldığını, aboneliğin bitmesinin kurulumu durdurmadığını ve güncelleme almayan kurulumun uyumunun firmada olduğunu yazıyorsa doğrulanır."
> Sorun: Sözleşmede hüküm bulunması, “firma verisiyle ve çalışan kurulumuyla kalır” varsayımının fiilen gerçekleştiğini doğrulamaz. Kurulumun çalışmaya devam etmesi ve firmanın verisine erişebilmesi için doğrulama kuralı yoktur.
> Öneri: Sözleşme incelemesine ek olarak abonelik sonrası durumu temsil eden bir kabul senaryosu tanımla: firma veriye erişebilmeli, dışa aktarabilmeli ve kurulum güncelleme/destek olmadan çalışmaya devam etmelidir.
> 
> SONUÇ: 5 BULGU
> ```

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ⚠️ KISMİ | S-4 "faturanın gerektirdiği veriyi tam taşır" diyordu; ticari amaçla alan birinin faturası vergi bilgisi ister ve ürün bunu toplamaz (K-112 kurumsal fatura alanını bilinçli olarak açmadı). Ticari alıcıyı dışlamak ya da kurumsal fatura akışı eklemek varlık kararıdır ve K-07, K-112'ye aykırıdır. Doğru düzeltme iddiayı daraltmak. | S-4: "… sistem tüketiciye kesilecek faturanın gerektirdiği veriyi tam taşır … Kurumsal fatura alanı yoktur: ticari amaçla alan birinin vergi bilgisi ürünün dışında alınır (K-112)." |
| BULGU-2 | ✅ KABUL | Gerçek bir kenar durumu: hiç sipariş gelmezse V-3'ün ve M-1, M-3, M-4'ün paydası sıfırdır ve kurallar sonuç üretmiyordu. Hiçbir karar bunu kapsamıyordu. | §8 karar kuralı maddesine sıfır payda cümlesi; K-490 öneriyle kaydedildi. |
| BULGU-3 | ✅ KABUL | Tur 11'de V-4'e eklenen cümle (sebep ürün içinde ölçülemez) ile yanlışlama kuralı (düşük hacim yanlışlar) gerçekten çelişiyordu. Önerilen yapı mantıken doğru: düşük hacim görüşmeyi tetikler, yanlışlama görüşmenin bulgusuna bağlanır. K-487'nin V-4 kuralı buna göre düzeltildi. | V-4: "… birin altında kalırsa bu bir tetikleyicidir, yanlışlama değil … firma sahibi siteye pazarlama yapmadığını söylerse varsayım yanlışlanır …" |
| BULGU-4 | ✅ KABUL | 150 işlem K-403'ün hesabıdır ve her siparişin en ağır tek hattan geldiğini varsayar; K-454'ten sonra karışık sipariş dört adım edebilir. Sayı bir üst sınır gibi okunuyordu. | V-2: "… 50 siparişin hepsi en ağır hattan — havale + fiziksel — gelse 150 panel işlemi eder … karışık siparişler bu sayıyı artırabilir (K-454)" |
| BULGU-5 | ⚠️ KISMİ | Sözleşmenin yazması varsayımın fiilen tuttuğunu göstermez; bu doğru. Ama önerilen "kabul senaryosu" MVP kabulüne yeni bir kriter ekler (K-415'in listesi). Fiilî sınavın doğal anı aboneliği biten ilk kurulumdur; o an doğrulama satırına eklendi, kabul kapısı değişmedi. | V-5: "… sözleşme düzeyinde doğrulanır … Fiilî sınavı aboneliği biten ilk kurulumdur: o kurulum çalışmayı sürdürmez ya da firma veritabanına ve yedeklerine erişemezse yanlışlanır (K-410, K-427)" |

**Dağılım:** 3 KABUL · 2 KISMİ · 0 RET. Talimata eklenen cümleden sonra ilk tur: önceki kararların tekrarı gelmedi; beş bulgunun beşi de yeni.

## 3. Ek bulgular

Yok.

## 4. Kullanıcı onay checklist'i

Oturum açılışındaki talimat gereği uygulandı (`01` v0.22). Bir karar satırı açıldı: K-490 (öneriyle kaydedildi) — sıfır payda kuralı. K-487'nin V-4 kuralı aynı oturumda düzeltildi ve satırına not düşüldü.

- [x] BULGU-1 · [x] BULGU-2 · [x] BULGU-3 · [x] BULGU-4 · [x] BULGU-5
