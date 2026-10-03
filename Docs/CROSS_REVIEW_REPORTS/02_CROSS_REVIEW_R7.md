# Cross-Review — 02 Product Requirements (Tur 7)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/02_PRODUCT_REQUIREMENTS.md` v0.21 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki turlar ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde; satır sonlarındaki boşluklar atıldı). İstem önceki turlarınkiyle aynıdır: yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi.

> ```text
> BULGU-1
> Kriter: edge case
> Seviye: Yüksek
> Yer: §10.4.10 Başka bir kanaldan gelen cayma bildiriminin kaydı
> Alıntı: "Havale hattında firma IBAN'ı girer; IBAN müşterinin bildiriminden gelir."
> Sorun: Geçerli bir e-posta, mektup veya örnek form iletilmiş cayma beyanı IBAN içermek zorunda değildir. Bu durumda IBAN girilmesini zorunlu tutmak, cayma kaydını engelleyebilir veya yöneticiyi müşteriden gelmemiş bir veriyi girmeye zorlayabilir. IBAN beklerken geri ödeme süresinin nasıl işleyeceği de bu senaryo için tanımlı değildir.
> Öneri: Harici kanaldan cayma kaydında IBAN’ı isteğe bağlı yapın. Beyanı derhal kaydedin; IBAN yoksa müşterinin sipariş sayfasındaki güvenli alandan IBAN istenecek ayrı bir akış ve bildirim tanımlayın. Geri ödeme süresinin bu bekleyişte durmadığını açıkça belirtin.
>
> BULGU-2
> Kriter: teknik doğruluk
> Seviye: Orta
> Yer: §3.14.6 Adresin alanları
> Alıntı: "T.C. kimlik numarası istenmez: ... nihai tüketiciye kesilen faturada T.C. kimlik numarası zorunlu değildir"
> Sorun: Bu ifade koşulsuz yazılmıştır. E-Arşiv/fatura düzeninde belge tutarı ve işlem koşullarına göre alıcının T.C. kimlik numarası veya başka kimlik bilgisinin gerekebileceği durumlar vardır. Ürün bu bilgiyi hiç toplamıyor; muhasebe aracında “tamamlanacağı” söyleniyor ancak verinin müşteriden hangi yolla alınacağı belirtilmiyor.
> Öneri: Mutlak iddiayı kaldırın. Güncel mali mevzuatın gerektirdiği durumlarda kimlik bilgisinin ürün dışında güvenli biçimde müşteriden alınacağı ve faturanın bu kanaldan tamamlanacağı iş kuralını ekleyin; eşik değerleri dokümana sabitlemeyin.
>
> BULGU-3
> Kriter: güvenlik
> Seviye: Orta
> Yer: §12.2.7 Saklama ve imha
> Alıntı: "tutar ve tarih gibi kişisel olmayan alanlar firmanın ticari kaydı olarak kalır."
> Sorun: Tutar, tarih, sipariş numarası, ürün kalemleri veya diğer kalan alanlar birlikte tutulduğunda kişiyi yeniden belirlenebilir kılabilir. Doküman, süre sonunda hangi alanların silineceğini, hangi alanların geri döndürülemez biçimde anonimleştirileceğini ve kalan ticari kaydın sipariş sahibine bağlanamayacağını tanımlamıyor.
> Öneri: Saklama süresi sonunda kalan kayıt için açık bir anonimleştirme/minimizasyon kuralı ekleyin: silinecek tanımlayıcılar, korunacak muhasebe alanları, sipariş numarası ve kalem ayrıntılarının durumu ile yeniden kimliklendirmeyi engelleyen koşullar belirtilsin.
>
> BULGU-4
> Kriter: tutarlılık
> Seviye: Orta
> Yer: §13.4 Açık kararlar
> Alıntı: "A-18 K-498 ile kapandı... Bu yüzden tablo boştur."
> Sorun: Dokümanın sürüm geçmişi K-498 kararının “öneriyle kaydedildiğini” ve proje sahibinin gözden geçirmesi için ⚠ işareti taşıdığını söylüyor. Buna rağmen §13, ilgili kararı kapanmış kabul edip açık karar tablosunu boş gösteriyor. IBAN’ın yeniden istenme kanalı ve süre etkisi gibi temel bir kuralın onay durumu belirsiz kalıyor.
> Öneri: K-498 proje sahibi tarafından onaylandıysa ⚠ ve “gözden geçirme” ifadelerini kaldırıp kabul kaydını ekleyin; onaylanmadıysa A-18’i §13 tablosunda açık karar olarak yeniden açın.
>
> SONUÇ: 4 BULGU
> ```

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ✅ KABUL | Doğru. §10.4.10 (K-510) firmanın havale hattında IBAN'ı *"müşterinin bildiriminden"* gireceğini yazıyordu. Ama yasal bir cayma bildiriminin IBAN taşıma zorunluluğu yok: Mesafeli Sözleşmeler Yönetmeliği m.11/1 yalnız bildirimin süresinde ve yazılı ya da kalıcı veri saklayıcısıyla yapılmasını arıyor. IBAN'sız gelen e-postada kayıt tanımsızdı. Firma ya kaydı bekletecekti ya da müşteriden gelmemiş bir IBAN girecekti. İkincisi K-498'in tek giriş yolu kuralına aykırı. Önerilen çözüm projenin mevcut yollarıyla örtüşüyor. K-498 ve K-575'in istek yolu (sipariş sayfasında IBAN alanı, B-14, "IBAN bekleniyor" listesi, süre durmaz) aynen kullanılabiliyor. Yeni ekran, bildirim ya da elle adım gerekmiyor. | K-581 `(öneriyle kaydedildi — ⚠)`: bildirim IBAN taşımıyorsa ya da taşıdığı IBAN denetimden geçmiyorsa kayıt hemen ve IBAN'sız yapılır. Kayıtla birlikte sipariş sayfasında o kalem için IBAN alanı açılır, müşteriye B-14 gider ve süre durmaz. Firma IBAN'ı başka bir yoldan isteyip girmez. Aktarılan IBAN da sipariş sayfasındakiyle aynı denetimden geçer. Güncellenen yerler: §5.8 (IBAN isteği), §6.4.25, §7.4.5, §8.4.2, §9.2 B-14, §10.4.10, §10.6.1. |
| BULGU-2 | ❌ RET | Doküman doğru. Gelir İdaresi'nin e-Arşiv duyurusu, vergi mükellefi olmayan nihai tüketiciye düzenlenen faturada T.C. kimlik numarası zorunluluğu olmadığını söylüyor. Numarasını paylaşmak istemeyen tüketicinin e-Arşiv faturasında alıcı alanına *"11111111111"* girilebiliyor ve satıcının bu bilgiyi doğrulama sorumluluğu yok. Bulgu, numaranın *"belge tutarı ve işlem koşullarına göre"* gerekebileceğini söylüyor ama bir hüküm göstermiyor. 509 sıra no.lu Tebliğ'deki tutar sınırları e-Arşiv'e geçme zorunluluğuyla ilgili, alıcının kimlik numarasıyla değil. Bu, K-561'in 2026-10-03'te kontrol edilmiş kararı. Ama bulgunun kaynağı metindeki bir eksiklikti. *"Bu alanı firma muhasebe aracında tamamlar"* cümlesi, firmanın gerçek numarayı bir yerden alması gerekiyormuş gibi okunuyordu. Konu önceki turlarda gelmedi. | Karar kaydı gerekmedi. §3.14.6'ya kuralın kapsamı (*"vergi mükellefi olmayan"*) ve 11111111111 uygulaması yazıldı. Alanın böyle tamamlandığı ve ürünün numarayı toplamadığı da eklendi. |
| BULGU-3 | ⚠️ KISMİ | Sorunun çekirdeği doğru. K-357 sipariş kaydında anonimleştirmeyi seçti ve imha edilecek alanları saydı, ama kalan kaydın alanlarını ve hesapla bağını yazmadı. Hesabı hâlâ açık olan üyenin on yıllık siparişi, kişisel verileri silinse de o hesabın sipariş geçmişinde duruyordu. Böylece kayıt kişiyle ilişkilendirilebilir kalıyordu. KVKK m.3/1-b anonim hâle getirmeyi *"başka verilerle eşleştirilerek dahi"* ilişkilendirilemeyecek hâl olarak tanımlıyor. K-117 bağı yalnız hesap silindiğinde kesiyordu. Önerinin ikinci yarısı reddedildi: yeniden kimliklendirmeyi engelleyen ayrıntılı koşullar ve kalem ayrıntılarının durumu. Bağ kesildikten sonra kalan kayıtta kişiye götüren bir alan yok; ayrıntı Aşama 1'in kapsamı dışında. Sipariş numarası da korunur, çünkü muhasebedeki faturayla eşleşme buna dayanır. | K-582 `(öneriyle kaydedildi — ⚠)`: süresi dolan siparişin (Z-28, Z-41) hesapla bağı da kesilir; kayıt sipariş geçmişinden düşer, sipariş sayfasına girilemez. Kalan ticari kaydın alanları sayıldı. Muhasebe aracındaki faturanın sipariş numarasını taşıyabileceği kalan risk olarak yazıldı. Güncellenen yerler: §4.2 Z-28 ve Z-41, §12.2.7. |
| BULGU-4 | ❌ RET | Çelişki yok. `(öneriyle kaydedildi)` işaretli karar geçerli ve kapalı bir kayıttır. Proje sahibi 2026-09-17'de bu yetkiyi verdi ve itiraz gelirse kararın değiştirileceğini söyledi (`.claude/INSTRUCTIONS.md` — öneriyle kayıt yetkisi). ⚠ yalnız proje sahibinin gözden geçirme listesini işaretliyor; kararı askıya almıyor. K-498'in kapattığı A-18 bu yüzden §13'ün açık karar tablosuna girmez. Bulgunun iki önerisi de reddedildi. ⚠'yi kaldırmak gözden geçirmeyi siler. A-18'i yeniden açmak, kullanılan kararı askıya alır. Ama bulgunun kaynağı metindeki bir boşluktu: `02` bu kuralı hiç yazmıyordu ve dokümanı tek başına okuyan biri ⚠'yi "onay bekliyor" diye anlayabilirdi. Konu önceki turlarda gelmedi. | Karar kaydı gerekmedi. §13.4'e kural yazıldı: öneriyle kaydedilen karar ⚠ taşısa da geçerli bir kayıttır, ⚠ gözden geçirme listesini gösterir. İtiraz gelirse karar yeni bir satırla değişir ve açık karar olarak tabloya dönmez. |

**Dağılım:** 1 KABUL · 1 KISMİ · 2 RET.

Önceki altı turun konuları bu turda dönmedi. İki RET de doğru bir kurala yöneldi, ama ikisi de metindeki bir boşluktan beslenmiş olabilir. Bu yüzden kural korunup metin netleştirildi (R6 BULGU-1 ile aynı yaklaşım).

## 3. Ek bulgular

- **Etki yansıtma için not:** iki karar başka dokümanlara dokunuyor:
  - `10` §2 KP-47 başka kanaldan gelen caymanın panelden kaydını sayıyor. K-581 yeni bir elle adım açmadığı için satır değişmez; KP-66'nın bildirim sayısı da değişmez (B-14'e yalnız yeni bir tetik eklendi). Etki yansıtmada teyit edilecek.
  - K-582 `05`, `06` ve `12`'yi ilgilendiriyor (anonimleştirme işinin kapsamı). Bu dokümanlar henüz yazılmadı. `01` ve `10`'da kalan ticari kayda dair bir cümle yok.

  Bariz bir çelişki doğmadığı için `01` ve `10`'a bu turda dokunulmadı.
- **Kendi kontrolüm:** BULGU-1 düzeltmesinde §10.4.10'un elenen yollarına bir madde eklendi: IBAN'sız bildirimi IBAN gelene kadar kaydetmemek beyanın tarihini kaydırır. Bu madde K-510'un *"süresi içinde yapılmış yasal bir bildirimi yok sayar"* gerekçesinin devamıdır.

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`02` v0.22). İki karar satırı açıldı:

- K-581 `(öneriyle kaydedildi — ⚠)`
- K-582 `(öneriyle kaydedildi — ⚠)`

K-581 paraya ve kişisel veriye, K-582 kişisel veriye dokunduğu için ikisi de proje sahibinin gözden geçirme listesine girer. İkisinde de seçenekler gerçekten ayrışmıyor. K-581 mevcut K-498/K-575 yolunu yeni bir tetikle kullanıyor. K-582 K-357'nin seçtiği anonimleştirmeyi tamamlıyor. Karar kaydında K-510, K-498 ve K-357'nin etki sütunlarına yeni satırlara atıf eklendi.

- [x] BULGU-1 · [x] BULGU-2 · [x] BULGU-3 · [x] BULGU-4

**Hukuki kontrol:**

- **BULGU-1:** Mesafeli Sözleşmeler Yönetmeliği m.11/1 ve m.12'nin güncel metni K-510 ve K-498'de resmî metne karşı doğrulanmıştı (mevzuat.gov.tr konsolide metni). Bildirimin içeriği için IBAN şartı yok; geri ödeme süresi için durma hâli yok.
- **BULGU-2:** Gelir İdaresi Başkanlığı'nın e-Arşiv fatura düzenleyen mükelleflere duyurusu (2018; alomaliye.com'daki metninden, 2026-10-03'te okundu): *"vergi mükellefi olmayan nihai tüketici mahiyetindeki müşteriler tarafından T.C. Kimlik Numarası bilgilerinin paylaşılmak istenmediği hallerde, e-Arşiv Faturalarında alıcı hesap numarası alanına '11111111111' girilebilecektir."* 509 sıra no.lu VUK Genel Tebliği'nin güncel tutar sınırları e-Arşiv'e geçme zorunluluğuyla ilgili; alıcının kimlik numarası için bir eşik bulunamadı. Duyurunun resmî GİB sayfası bu oturumda açılmadı, ikincil kaynaktan okundu.
- **BULGU-3:** KVKK m.3/1-b (anonim hâle getirme tanımı). Ürün kuralı yalnız tanıma uyumlu hâle getirildi; yeni bir hukuki iddia yok.
- **BULGU-4:** Hukuki iddia taşımıyor; proje kuralına (`.claude/INSTRUCTIONS.md`) karşı değerlendirildi.
