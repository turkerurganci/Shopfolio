# Cross-Review — 02 Product Requirements (Tur 21)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/02_PRODUCT_REQUIREMENTS.md` v0.36 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki turlar ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde; satır sonlarındaki boşluklar atıldı). İstem önceki turlarınkiyle aynıdır: yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi.

> ```text
> BULGU-1
> Kriter: Güvenlik
> Seviye: Yüksek
> Yer: §10.4.10 Başka bir kanaldan gelen cayma bildiriminin kaydı
> Alıntı: "Firma kalemi seçer ve bildirimin firmaya ulaştığı tarihi girer... Havale hattında bildirim IBAN taşıyorsa firma onu aktarır"
> Sorun: E-posta, mektup veya form üzerinden gelen bildirimin sipariş sahibinden geldiğini doğrulayan bir kural yoktur. Bildirim doğrudan cayma kaydı, geri ödeme süresi ve havale hattında IBAN’a para gönderme süreci başlatabilir. Adres ve misafir e-posta düzeltmelerinde teyit zorunluyken, para ve sipariş hakkını etkileyen bu işlemde yoktur.
> Öneri: Bildirimi alındığı tarih ile hemen kaydedin; ancak cayma/geri ödeme işlemini “kimlik teyidi bekliyor” durumunda tutun. Teyidi siparişin donmuş e-postası, teslimat telefonu veya sipariş sayfasındaki erişim anahtarıyla tanımlayın; özgün bildirim tarihi yasal süre hesabında korunmalıdır.
>
> SONUÇ: 1 BULGU```

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ⚠️ KISMİ | **Boşluk gerçekti; önerilen çözümün yarısı alındı.** §10.4.10 başka kanaldan gelen cayma bildiriminin kaydını tanımlıyordu (K-510, K-581), ama bildirimin siparişin sahibinden geldiğini kimin ve nasıl doğrulayacağını yazmıyordu. Karar kaydında da bu konuyu ele alan bir satır yok. Adres düzeltmesinde (§10.4.2; K-601) ve misafir e-postasının düzeltmesinde (§10.4.11; K-585) teyit zorunluydu; paraya dokunan bu kayıtta değildi. Somut saldırı şudur: sipariş numarasını ve teslimat adresini bilen biri, havale hattındaki ve kargodaki bir sipariş için kendi IBAN'ını taşıyan bir cayma mektubu gönderir. Beyan teslimden önce yapıldığı için geri ödeme bildirimden on dört gün içinde yapılır (§7.4.1; Yönetmelik m.12/2), para saldırganın IBAN'ına gider ve gerçek sahip istemediği bir caymayla karşılaşır. Aynı açık, kargodan önce gelen bildirimi işleyen "müşteriyle anlaşıldı (müşteri talebi)" iptalinde (§6.4.31; K-604) ve telefonla istenen her müşteri talebi iptalinde (§7.2.4) de vardı. **Önerinin "kimlik teyidi bekliyor" durumu alınmadı:** yeni bir kalem durumu ve elle adım açar. Kayıt zaten geçmişe dönük tarihle yapılır (K-510), yani firma önce teyit edip sonra bildirimin ulaştığı tarihle kaydettiğinde yasal tarih korunur. Codex'in teyit kanalları (siparişin e-postası, teslimat telefonu) alındı; bunlar K-601'in kalıbıyla aynıdır. | **K-607 (öneriyle kaydedildi — ⚠).** §10.4.10'a teyit kuralı yazıldı: bildirim siparişin iletişim e-postasından gelmediyse firma siparişin e-postasına yazar ya da teslimat telefonunu arar; teyit kaydın tarihini değiştirmez. Havale hattında bildirimin içindeki IBAN artık yalnız bildirim siparişin iletişim e-postasından geldiyse aktarılır. Başka yoldan gelen bildirimde kayıt IBAN'sız yapılır ve IBAN sipariş sayfasından istenir (K-581'in yolu). Gerekçe: teyit caymanın iradesini doğrular, mektuptaki IBAN'ı değil. Aynı teyit her "müşteriyle anlaşıldı (müşteri talebi)" iptaline uygulandı (§7.2.4). Yeni §8.3.8 ve yeni 6.4.33 satırı eklendi; §5.8, §6.4.25, §6.4.31, §7.3.5, §8.4.2 ve §12.5 hizalandı. |

**Dağılım:** 0 KABUL · 1 KISMİ · 0 RET.

- **BULGU-1:** Daha önce gelmemişti. Başka kanaldan gelen bildirim 7. turda (K-581, IBAN'sız bildirim), 19. turda (K-604, kargodan önce gelen bildirim) ve 20. turda (K-606, eksik bilgilendirme) ele alınmıştı; hiçbiri bildirimin kimden geldiğini sormamıştı.
- **Tek bulgu, kısmi kabul:** Sorun gerçekti ve karar kaydında karşılığı yoktu. Önerinin yeni bir durum açan kolu yerine ürünün var olan teyit kalıbı (K-601) ve IBAN'ın tek giriş yolu (K-498) kullanıldı.

## 3. Ek bulgular

- **IBAN aktarmasının daralması:** Codex yalnız teyidi istedi. Değerlendirme sırasında görülen ikinci açık şuydu: teyit edilmiş bir bildirimde bile mektuptaki IBAN'ın sahibine ait olduğu doğrulanmaz. Sahibi telefonda "evet, caydım" derken mektuptaki IBAN'ı görmez. Bu yüzden K-607, K-510'un IBAN aktarmasını siparişin iletişim e-postasından gelen bildirimle sınırladı. Mektupla cayan müşteri IBAN'ı sipariş sayfasından girer. Bu ek bir adım ama yeni bir ekran ya da bildirim açmaz (B-14 K-581'den beri var).
- **Üye siparişinde "siparişin iletişim e-postası":** Üyenin siparişinin e-postası hesabın doğrulanmış e-postasıdır (§10.4.11). Üye hesabının e-postasını sipariş verdikten sonra değiştirirse teyidin hangi adrese yapılacağı K-601'de de K-607'de de açıkça yazılı değil. Ürün siparişin donmuş e-postasını gösterir (§3.23.3). `04` teyit hatırlatmasını tasarlarken panelde hangi adresin gösterileceğini netleştirmeli.
- **Önceki turlardan açık kalanlar:** 20. turun ek bulguları bu turda gelmedi: aynı gün caymada firmanın erken ödeme yolu ve K-606'nın hizmet ve dijital kalemi. 19. ve 18. turun açık kalan notları da gelmedi.
- **Etki yansıtma için not:** `10` KP-47'nin (K-510) satırı teyidi ve IBAN aktarmasının daralmasını taşımalı. `04` kayıt ekranında ve "müşteriyle anlaşıldı (müşteri talebi)" iptalinde teyit hatırlatmasını tasarlamalı. `12` Ön Bilgilendirme Formu taslağı, e-posta dışı kanallarla cayanlardan firmanın siparişin kanallarından teyit isteyebileceğini ve havale hattında IBAN'ın sipariş sayfasından girileceğini söylemeli. `01`'de bu turun değiştirdiği cümlelerin karşılığı yok.

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`02` v0.37). Bir yeni karar satırı açıldı ve ⚠ ile işaretli. Seçenekleri gerçekten ayrışan ve firmaya ya da müşteriye ciddi para veya hukuki risk yükleyen bir konu çıkmadı: K-607 firmanın para riskini azaltır, tüketiciye yeni bir şekil şartı yüklemez ve var olan bir kalıbı (K-601) uygular.

- [x] BULGU-1 (kısmi — K-607 ⚠)
- ⚠ **K-607:** Başka kanaldan gelen cayma bildirimini firma, bildirim siparişin iletişim e-postasından gelmediyse siparişin e-postasına ya da teslimat telefonuna dönerek teyit eder. Havale hattında mektuptaki ya da başka adresten gelen IBAN aktarılmaz; IBAN sipariş sayfasından istenir. Gözden geçirilecek nokta şu: bu, K-510'un "firma IBAN'ı bildirimden aktarır" kuralını daraltır ve mektupla cayan müşteriye IBAN'ı sipariş sayfasından girme adımı ekler. Elenen seçenek, bildirimi hemen kaydedip "kimlik teyidi bekliyor" durumunda tutmaktı; bu, yeni bir durum ve elle adım açardı.

**Hukuki kontrol** (2026-10-03, Mesafeli Sözleşmeler Yönetmeliği'nin mevzuat.gov.tr konsolide metni):
- **BULGU-1:** m.11/1: *"Cayma hakkının kullanıldığına dair bildirimin cayma hakkı süresi dolmadan, yazılı olarak veya kalıcı veri saklayıcısı ile satıcı, sağlayıcı veya aracı hizmet sağlayıcıya yöneltilmesi yeterlidir."* Hüküm bildirimin şeklini ve süresini düzenler; satıcının bildirimin tüketiciden geldiğini doğrulamasını yasaklamaz. K-607 tüketiciye yeni bir şekil şartı yüklemez: teyit için firma siparişin kanallarına kendisi döner ve kaydın tarihi bildirimin firmaya ulaştığı tarih kalır, bu yüzden m.12'nin on dört günü değişmez. m.12/2: *"Malın tesliminden önce cayma hakkının kullanılması durumunda ... cayma hakkının kullanıldığına ilişkin bildirimin kendisine ulaştığı tarihten itibaren on dört gün içinde"* geri ödeme yapılır. Saldırının para kolu bu hükümden doğar: teslimden önceki sahte bildirimde para mal dönmeden gider.
