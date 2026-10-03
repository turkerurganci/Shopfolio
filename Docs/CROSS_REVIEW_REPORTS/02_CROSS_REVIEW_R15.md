# Cross-Review — 02 Product Requirements (Tur 15)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/02_PRODUCT_REQUIREMENTS.md` v0.30 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki turlar ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde; satır sonlarındaki boşluklar atıldı). İstem önceki turlarınkiyle aynıdır: yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi.

> ```text
> BULGU-1
> Kriter: Eksiklik
> Seviye: Orta
> Yer: §3.7.3 Ürünün yayın durumu, arşiv ve silme
> Alıntı: "Yayındaki her varyantın fiyatı vardır."
> Sorun: Fiyatın 0 TL’den büyük olması tanımlanmamıştır. 0 TL fiyatlı varyant yayına çıkabilir; bu da “her sipariş bir ödemeden geçer” kuralıyla çelişen sıfır tutarlı sipariş üretebilir.
> Öneri: Yayındaki varyantın fiyatının KDV dahil 0,00 TL’den büyük olması gerektiğini; sipariş toplamı 0 TL ise siparişin oluşmayacağını açıkça yazın.
>
> BULGU-2
> Kriter: Güvenlik
> Seviye: Yüksek
> Yer: §10.4.2 Adres yalnız “Kargoya verildi”ye kadar düzeltilir
> Alıntı: "Adres yalnız 'Kargoya verildi'ye kadar düzeltilir."
> Sorun: Adres düzeltmesini talep eden kişinin yetkisinin nasıl doğrulanacağı tanımlı değildir. Bir saldırgan firma personelini yanıltarak teslimat adresini değiştirebilir; müşteriye değişiklikten sonra bildirim göndermek, paketin yanlış adrese gönderilmesini önlemez.
> Öneri: Adres değişikliği için sipariş sayfasındaki yetkili erişimden onay veya mevcut iletişim kanalına gönderilen ön-onay şartı koyun; firma tarafından alınan taleplerde kullanılacak kimlik teyit kuralını da e-posta düzeltmesindeki kadar somutlaştırın.
>
> BULGU-3
> Kriter: Teknik doğruluk
> Seviye: Orta
> Yer: §12.2.8 Kişisel verinin yurt dışına aktarımı
> Alıntı: "Google ile girişte ürün yalnız dönen ad ve e-postayı alır — bu ilişki bir aktarım değildir."
> Sorun: Google ile giriş, kullanıcının tarayıcısı ile Google arasında kimlik doğrulama trafiği ve sağlayıcıya yönlendirme içerir. Bu akışın peşinen “aktarım değildir” sayılması, sağlayıcının konumu, tarafların rolleri ve somut veri akışı değerlendirilmeden yurt dışı aktarımı yükümlülüklerinin atlanmasına yol açabilir.
> Öneri: Google girişini aydınlatma ve aktarım envanterine ekleyin; sağlayıcının rolü, işlenen veri kategorileri, veri konumu ve uygulanacak yurt dışı aktarım mekanizmasını kurulum bazında belirleyin.
>
> SONUÇ: 3 BULGU```

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ✅ KABUL | Doğru. §3.10.4 *"her sipariş bir ödemeden geçer ve 0 TL'lik sipariş doğmaz"* diyordu, ama bu güvenceyi yalnız kupon tarafında kuruyordu (K-565). K-11 fiyatı zorunlu kıldı, sıfırdan büyük olmasını istemedi. K-65 de indirim yüzdesine üst sınır koymadı. 0,00 TL'lik bir varyant ya da %100 indirim, yalnız dijital ya da hizmetten oluşan veya ücretsiz kargolu bir siparişi 0 TL'ye indiriyordu. O siparişte ne 3D Secure'un uygulanacağı bir ödeme ne de ödeme ekseninin bir dalı vardı. 2. turun BULGU-1'i %100 indirimi de anmıştı, ama K-565 yalnız kuponu kapattı. Konu bu yüzden geri döndü. Codex'in ikinci önerisi (*"toplam 0 TL ise sipariş oluşmaz"*) ayrı bir kural olarak alınmadı: fiyat ve indirim sıfıra inemediği için toplam zaten sıfırın üstündedir. | K-600 `(öneriyle kaydedildi)`. En küçük fiyat 0,01 TL'dir. %100 indirim girilemez. Kuruşa yuvarlanmış indirimli fiyat bir varyantta 0,00 TL olacaksa indirim kaydedilmez ve yönetici uyarılır (K-68'in kalıbı). İndirim yürürken varyanta böyle bir fiyat da girilemez. Güncellenen yerler: §1.2 (İndirim), §3.7.3, §3.8.4, §3.9.1, §3.10.4, yeni satırlar 6.6.12 ve 6.6.13. |
| BULGU-2 | ⚠️ KISMİ | Sorun gerçek. K-374 adresin hangi duruma kadar düzeltileceğini, K-375 de düzeltmenin bildirileceğini yazdı. Düzeltmenin kimin talebiyle ve nasıl bir teyitle yapılacağı ise yazılı değildi. Misafir siparişinin e-posta düzeltmesinde aynı soru cevaplanmıştı (§10.4.11, K-585), adreste cevaplanmamıştı. Firmayı arayıp "adresimi değiştirin" diyen biri malı kendi adresine çevirtebiliyordu. B-10 da yalnız düzeltilen adresi taşıyordu, gerçek sahibine ne yapacağını söylemiyordu. Codex'in önerisinin bir parçası alınmadı: **düzeltmeyi sipariş sayfasından verilecek ön onaya bağlamak.** Bu, sevkiyat hattına onay bekleyen bir ara adım ekler. Onay gelene kadar paket kargoya verilemez, ama kargoya verme süresi işlemeye devam eder ve gecikme feshinin yolu açılır (§7.2.3, K-499). Adreste, e-posta düzeltmesinden farklı olarak, siparişin e-postası ve teslimat telefonu yerinde durur. Bu yüzden daha basit ve güçlü bir teyit mümkündür: talebin geldiği kanala değil, siparişin kendi kanalına dönmek. | K-601 `(öneriyle kaydedildi — ⚠)`. Adres, teslimat telefonu dahil, müşterinin talebiyle düzeltilir. Talep siparişin iletişim e-postasından gelmediyse firma siparişin e-postasına yazarak ya da teslimat telefonunu arayarak teyit eder. Talebin geldiği numara ya da adres teyit sayılmaz, sipariş numarası da tek başına teyit değildir. Ürün teyidi denetlemez; ispatı ve kandırılmanın sonucu firmadadır. B-10 artık değişikliği istemeyen müşteriye firmaya ulaşmasını söyler. Kalan risk ve elenen yol yazıldı. Güncellenen yerler: yeni satır 6.3.6, §6.7.12, yeni madde 8.3.7, §9.2 B-10, §10.4.2, §12.5 (yeni satır). |
| BULGU-3 | ❌ RET | Önerinin dayanağı yanlış. Kişisel Verileri Koruma Kurumu'nun Yurt Dışına Kişisel Veri Aktarımı Rehberi'ne (Ocak 2025) göre aktarım, veri aktaranın işlediği verinin yurt dışındaki alıcıyla paylaşılması ya da ona erişilebilir kılınmasıdır. Rehber açıkça şunu söyler: *"Üçüncü ülkede yerleşik veri sorumlusunun Türkiye'de yerleşik veri sahibinden doğrudan kişisel veri elde etmesi aktarım teşkil etmeyecek."* Google ile girişte ürün Google'a kendi kayıtlarından kişisel veri göndermez. Kimlik doğrulaması kullanıcının kendi tıklamasıyla Google'ın ekranında olur, Google veriyi ilgili kişiden doğrudan, ayrı veri sorumlusu olarak elde eder (K-359). Codex'in istediği "aydınlatmaya ekleme" zaten vardır: Google'ın aydınlatmada adıyla anılması firmanın yükümlülüğüdür (§12.2.8, §12.5, K-359). Ödeme sağlayıcısında durum farklıdır, çünkü ürün ödemeyi başlatırken alıcının verisini sağlayıcıya gönderir (K-513). **Konunun gelmesinin sebebi metindeki boşluktu:** §12.2.8 sonucu (*"bu ilişki bir aktarım değildir"*) gerekçesiz yazıyordu. Hemen önceki cümle de gömülü içeriği aktarım kaynağı sayıyordu. Konu daha önceki turlarda gelmemişti. | Karar değişmedi, yeni karar satırı açılmadı. §12.2.8'e gerekçe yazıldı: ürün Google'a kendi kayıtlarından veri göndermez. Giriş isteği yalnız firmanın uygulama kimliğini taşır; yeniden doğrulamada da ürün hesabın e-postasını göndermez, dönen hesabı kendisi karşılaştırır. Rehberin ölçütü yazıldı. Gömülü içerikten farkı da yazıldı: gömülü içerik her ziyaretçinin tarayıcısını kendiliğinden bağlardı, Google girişi ise yalnız düğmeye basanı kapsar ve isteğe bağlıdır. "Ürün Google'a kendi kayıtlarından veri göndermez" cümlesi K-359'un sonucunun koşuludur; yeni bir karar değildir. |

**Dağılım:** 1 KABUL · 1 KISMİ · 1 RET.

- **BULGU-1:** Yarısı ikinci kez geliyor. 2. tur %100 indirimi de anmıştı, K-565 yalnız kuponu kapatmıştı. Sıfır fiyat ilk kez geldi.
- **BULGU-2:** Daha önce gelmemişti. K-585 e-posta düzeltmesinin teyidini yazmıştı, adres düzeltmesi aynı soruyu cevapsız taşıyordu.
- **BULGU-3:** Daha önce gelmemişti. Doküman doğruydu ama gerekçesizdi. Gerekçe yazıldı, karar değişmedi.

## 3. Ek bulgular

- **Fesihten sonra kargonun malı yine de teslim etmesi (11. turdan açık, işlenmedi):** Bu turda da gelmedi. Etki yansıtmada ya da sonraki turda bakılmalı.
- **Şifre değişikliği (sıfırlama değil, 14. turdan açık):** §3.13.10 yalnız sıfırlamada oturumları kapatıyor. Bu turda gelmedi. Etki yansıtmada bakılabilir.
- **İndirim yürürken fiyat değişikliği:** K-600'ün "indirim yürürken böyle bir fiyat girilemez" cümlesi, fiyat değişikliğinin indirimli fiyatı referansın altına indirme kuralına (§3.9.5) dokunmuyor. İki kural farklı şeyi koruyor: biri sıfır tutarı, öteki indirim beyanının doğruluğunu. Çakışma yok.
- **Etki yansıtma için not:** `04` B-10'un yeni cümlesini, adres düzeltme ekranındaki teyit hatırlatmasını ve fiyat ve indirim alanlarının uyarısını yazmalı. `06` fiyatın alt sınırını (0,01 TL) taşımalı. `01` ve `10`'a bu iki kararın yansıyıp yansımayacağına etki yansıtmada bakılmalı. Bu turda `01` ve `10`'a dokunulmadı.

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`02` v0.31). İki karar satırı açıldı:

- K-600 `(öneriyle kaydedildi)`: K-565'in amacını fiyat ve indirime uygular; yeni bir seçim değildir.
- K-601 `(öneriyle kaydedildi — ⚠)`: kişisel veriye (adres) ve firmaya düşen para riskine dokunuyor; teyit yükünü firmaya yazıyor.

- [x] BULGU-1 · [x] BULGU-2 · [x] BULGU-3 (ret; §12.2.8'e gerekçe yazıldı)

**Hukuki kontrol** (2026-10-03):

- **BULGU-1:** Hukuki iddia içermiyor. Sıfır tutarlı satış yasak değildir. Kural ürünün kendi ödeme ekseninin tutarlılığından gelir (K-73, K-565).
- **BULGU-2:** Hukuki iddia içermiyor. Teyit yükümlülüğünün firmada olması K-585'in kalıbıdır (§12.5).
- **BULGU-3:** KVKK m.9 (7499 sayılı Kanun ile değişik) ve Kişisel Verileri Koruma Kurumu'nun "Kişisel Verilerin Yurt Dışına Aktarılması Rehberi" (2 Ocak 2025). Rehbere göre aktarım için veri aktaranın KVKK'ya tabi olması, işlediği verinin doğrudan paylaşılması ya da erişilebilir kılınması ve alıcının yurt dışında olması gerekir. Yurt dışındaki veri sorumlusunun Türkiye'deki ilgili kişiden doğrudan veri elde etmesi aktarım değildir. Kaynak: Erdem & Erdem, "Yurt Dışına Kişisel Veri Aktarımı Rehberi Neleri Düzenliyor?" (rehberin alıntısı).
