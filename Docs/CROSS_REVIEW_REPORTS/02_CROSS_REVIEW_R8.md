# Cross-Review — 02 Product Requirements (Tur 8)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/02_PRODUCT_REQUIREMENTS.md` v0.22 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki turlar ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde; satır sonlarındaki boşluklar atıldı). İstem önceki turlarınkiyle aynıdır: yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi.

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §6.1.1 — Bütün akışlara uygulanan ilkeler
> Alıntı: "Hiçbir akış bir e-postanın ulaşmasına bağlı değildir."
> Sorun: Bu mutlak ifade, üyelik akışındaki e-posta doğrulaması, şifre sıfırlama ve yönetici davetiyle çelişir. Bu işlemler, kullanıcı e-postadaki bağlantıya erişemeden tamamlanamaz.
> Öneri: Kuralı sipariş, ödeme, teslim ve iade akışlarıyla sınırlayın; hesap doğrulama, şifre sıfırlama ve davetin e-posta bağlantısına bağımlı istisnalar olduğunu açıkça yazın.
>
> BULGU-2
> Kriter: Güvenlik
> Seviye: Orta
> Yer: §8.1.2 — Kimlik doğrulama tabanı
> Alıntı: "Hesaba ve siparişe bağlanmanın tek dayanağı doğrulanmış e-postadır."
> Sorun: Misafir sipariş takibi doğrulanmış e-postaya değil, sipariş numarası + siparişe yazılmış e-postaya dayanır; ayrıca e-postadaki erişim bağlantısı ayrı bir anahtar kullanır (§3.22.3, §3.22.5). İfade, sipariş erişimi ile hesabın geçmiş siparişleri sahiplenmesini aynı güvenlik mekanizması gibi gösterir.
> Öneri: Hesaba/geçmiş siparişlere bağlamayı “doğrulanmış e-posta” ile; misafir sipariş erişimini ise “sipariş numarası + donmuş e-posta” ve e-posta bağlantısını “erişim anahtarı” ile ayrı ayrı tanımlayın.
>
> BULGU-3
> Kriter: Güvenlik
> Seviye: Yüksek
> Yer: §12.2.7 — Saklama ve imha
> Alıntı: "kişiye götüren bir alan kalmaz ve kayıt anonim hâle gelir"
> Sorun: Saklanan sipariş numarası, tarih, ürün, tutar ve firma kayıtları kişiyi dolaylı olarak yeniden belirlenebilir kılabilir. Doküman ayrıca firmanın muhasebe aracındaki faturanın sipariş numarasını taşıyabileceğini söylüyor; aynı veri sorumlusu bu iki kaydı eşleyebileceğinden kalan kayıt otomatik olarak anonim sayılamaz.
> Öneri: Kalan kaydı anonim değil, yeniden eşleme imkânı kaldırılana kadar takma adlı/kişisel veri olarak tanımlayın. Anonimleştirme hedefleniyorsa sipariş numarası ve dış sistemlerle eşleme imkânı dahil yeniden tanımlamaya yarayan tüm bağların nasıl geri döndürülemez biçimde kaldırılacağını belirleyin.
>
> SONUÇ: 3 BULGU
> ```

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ✅ KABUL | Doğru. §6.1.1 ve §9.1.2'nin başlığı *"Hiçbir akış"* diyordu, ama kuralın kendi gövdesi ve K-234 yalnız *"sipariş, ödeme ve durum geçişleri"*ni kapsıyor. Hesap ve yönetim bağlantıları ise adresin sahibini kanıtlamak için bilerek e-postaya bağlı: e-posta doğrulaması (K-101), şifre sıfırlama (K-105), yeni adresin doğrulanması (K-110) ve yönetici daveti (K-308). §9.4 bu e-postaları bildirim matrisinin dışında sayıp ulaşmadıklarında ne olacağını zaten yazıyordu (*"kullanıcı bağlantıyı ekrandan yeniden ister"*), ama §6.1.1 onlara istisna demiyordu. Dokümanın iki yeri birbirini yalanlıyordu. Önerinin kapsamı da doğru; teslim ve iade sipariş akışının parçası, iletişim talebi de §6.1.1'in "e-posta ulaşmadı" işaretini zaten taşıyor. | Karar kaydı gerekmedi: K-234'ün kapsamı değişmedi, başlık gövdeye hizalandı. §6.1.1 ve §9.1.2'nin başlığı *"Hiçbir sipariş ya da talep akışı"* oldu. §6.1.1'e hesap ve yönetim bağlantılarının istisnası ve ulaşmadıklarında izlenen yol yazıldı (§9.4'e atıfla). §3.22.4'teki tekrar *"hiçbir sipariş akışı"* diye düzeltildi. §2.2'nin *"hiçbir adım"* cümlesi zaten sipariş akışının içinde olduğu için değişmedi. |
| BULGU-2 | ❌ RET | Önerilen ayrım dokümanda zaten var. Sipariş sayfasına erişim §8.3.5'te ayrı bir kural: tahmin edilemez sipariş numarası, misafirde numara + e-posta ve bağlantıdaki ayrı erişim anahtarı (K-185, K-187, K-562). §3.22.3 ve §3.22.5 de aynısını yazıyor. §8.1.2'nin gövdesi yalnız bağlamayı anlatıyor: geçmiş misafir siparişlerinin hesaba düşmesi ve Google girişinin mevcut hesaba bağlanması (K-98, K-104). İkisi de doğrulanmış e-postaya dayanıyor, yani kural doğru. Ama bulgunun kaynağı metindeki bir belirsizlikti. *"Hesaba ve siparişe bağlanma"* başlığı "siparişe erişme" diye okunabiliyordu ve §8.1.2 §8.3.5'e atıf vermiyordu. Konu önceki turlarda gelmedi. | Karar kaydı gerekmedi. §8.1.2'nin başlığı *"Bir siparişin ya da bir giriş yolunun hesaba bağlanmasının tek dayanağı doğrulanmış e-postadır"* oldu. Sipariş sayfasına erişimin §8.3.5'in anahtarlarına dayandığı da eklendi. |
| BULGU-3 | ✅ KABUL | Doğru, ve hata 7. turun düzeltmesinden geliyordu. K-582 kalan ticari kaydı *"anonim hâle gelir"* diye koşulsuz yazdı. Aynı satır muhasebe aracındaki faturanın sipariş numarasını taşıyabileceğini de kalan risk olarak kaydetti. Bu ikisi birlikte doğru olamaz. Kişisel Verilerin Silinmesi, Yok Edilmesi veya Anonim Hale Getirilmesi Hakkında Yönetmelik m.10/2 anonimliği, verinin *"veri sorumlusu ... tarafından ... başka verilerle eşleştirilmesi"* yoluyla dahi kişiyle ilişkilendirilemez olmasına bağlıyor. Eşleştirmeyi yapabilecek olan firmanın kendisi; fatura sipariş numarasını, tarihi, tutarı ve kalemleri taşıyor. Müşterinin firmaya yazdığı e-posta yanıtları da firmanın kutusunda (§9.1.4). Bulgu iki yol öneriyordu. İlki uygulandı: kayıt, eşleşme imkânı sürdükçe kişisel veri sayılıyor. İkinci yol, kaydı geri döndürülemez biçimde anonimleştirmek için bütün bağları ürün içinden kaldırmaktı. Bu yol seçilmedi, çünkü ürün kendi dışındaki faturaya dokunamıyor. Sipariş numarasını silmek de tarih ve tutar üzerinden eşleşmeyi kaldırmıyor; K-583 K-582'nin bu eleme gerekçesini de düzeltiyor. Konu 7. turdaki BULGU-3'ün devamı; bu kez dönmesinin sebebi metindeki yanlış iddiaydı, belirsizlik değil. | K-583 `(öneriyle kaydedildi — ⚠)`: kalan kayıt ürünün içinde kişiye götüren alan taşımaz. Ama firmanın elinde aynı siparişi anan bir fatura ya da yazışma durdukça anonim sayılmaz ve kişisel veridir. Firma o kayıtları kendi saklama süresinin sonunda imha ettiğinde kayıt anonim hâle gelir. Bu imha firmanın yükümlülüğü olarak §12.5'e yazıldı. Kalan kaydın alanları, sipariş numarası dahil, değişmedi. Güncellenen yerler: §4.2 Z-28, §12.2.7, §12.5 (saklama ve imha politikası satırı). Karar kaydında K-582 ve K-357'nin etki sütunlarına K-583 eklendi. |

**Dağılım:** 2 KABUL · 1 RET.

İki KABUL'ün ikisi de dokümanın kendi içindeki bir çelişkiden geliyor. BULGU-1'de başlık gövdesinden geniş yazılmıştı. BULGU-3'te 7. turun düzeltmesi kendi kalan riskiyle çelişiyordu. RET verilen BULGU-2 de doğru bir kurala yöneldi ama metindeki belirsiz bir başlıktan beslendi. Bu yüzden kural korunup başlık netleştirildi (R6 BULGU-1 ve R7 BULGU-2, BULGU-4 ile aynı yaklaşım).

## 3. Ek bulgular

- **Etki yansıtma için not:** K-583 `12` (saklama ve imha politikası) ile `05` ve `06`'yı ilgilendiriyor, ama bu dokümanlar henüz yazılmadı. `01` ve `10`'da kalan ticari kayda ya da anonimliğe dair bir cümle yok (`grep` ile kontrol edildi). BULGU-1'in ilkesi `01` ve `10`'da geçmiyor; `10` §2 KP-18 yalnız sipariş sayfasını anlatıyor. Bariz bir çelişki doğmadığı için `01` ve `10`'a bu turda dokunulmadı.
- **Kendi kontrolüm:** K-234'ün karar satırı da *"Hiçbir akış"* diye başlıyor. Kararın gövdesi ve konusu (`B6-15`, bildirim gönderilemediğinde) kapsamı zaten sipariş, ödeme ve durum geçişleriyle sınırladığı için satır değiştirilmedi. `02` artık bu kapsamı açıkça yazıyor.

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`02` v0.23). Bir karar satırı açıldı:

- K-583 `(öneriyle kaydedildi — ⚠)`

K-583 kişisel veriye ve önceki bir kararın (K-582) cümlesine dokunduğu için proje sahibinin gözden geçirme listesine girer. Seçenekler gerçekten ayrışmıyor. Kayıt, firmanın elindeki fatura durdukça hukuken anonim değil. Bunu ürün içinden kaldırmanın tek yolu tarih ve tutarları da bozmak olurdu, bu da K-357'nin korumak istediği ticari kaydı yok ederdi. K-582'nin alanları ve hesapla bağın kesilmesi geçerli kalıyor.

- [x] BULGU-1 · [x] BULGU-2 · [x] BULGU-3

**Hukuki kontrol:**

- **BULGU-1:** Hukuki iddia taşımıyor; dokümanın kendi kurallarına (§9.4, K-234) karşı değerlendirildi.
- **BULGU-2:** Hukuki iddia taşımıyor; §8.3.5 ve §3.22'ye karşı değerlendirildi.
- **BULGU-3:** Kişisel Verilerin Silinmesi, Yok Edilmesi veya Anonim Hale Getirilmesi Hakkında Yönetmelik m.10, KVKK'nın resmî sayfasından (kvkk.gov.tr, 2026-10-03'te okundu): m.10/1 *"kişisel verilerin başka verilerle eşleştirilse dahi hiçbir surette kimliği belirli veya belirlenebilir bir gerçek kişiyle ilişkilendirilemeyecek hale getirilmesidir"*; m.10/2 *"veri sorumlusu, alıcı veya alıcı grupları tarafından geri döndürme ve verilerin başka verilerle eşleştirilmesi gibi ... uygun tekniklerin kullanılması yoluyla dahi ... ilişkilendirilemez hale getirilmesi gerekir"*. KVKK m.3/1-b'nin tanımı aynı. Faturanın kendi saklama süresine dair yeni bir iddia yazılmadı. Doküman yalnız firmanın o kaydı *"kendi saklama süresinin sonunda"* imha edeceğini söylüyor.
