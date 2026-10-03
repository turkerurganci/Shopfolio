# Cross-Review — 02 Product Requirements (Tur 5)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/02_PRODUCT_REQUIREMENTS.md` v0.19 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki turlar ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde; satır sonlarındaki boşluklar atıldı). İstem önceki turlarınkiyle aynıdır: yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi.

> ```text
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §7.4 İade ve geri ödeme — 7.4.1
> Alıntı: "geri ödemenin on dört günü ... iade edilen malın firmaya ulaştığı tarihten işler."
> Sorun: Teslimden sonraki caymada malın geri gelmesi geri ödemeyi bekletmeye imkân verebilir; ancak on dört günlük yasal sürenin başlangıcını malın ulaşmasına taşımak, geri ödemeyi fazladan 14 gün geciktirebilir. Z-16 aynı hatalı başlangıcı tekrarlar.
> Öneri: Geri ödeme süresini cayma bildiriminin firmaya ulaştığı andan başlatın; mal veya geçerli gönderim kanıtı gelene kadarki bekletme imkânını ayrı bir kural olarak tanımlayın.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §3.1 Firma, kurulum ve satış kapısı — 3.1.5
> Alıntı: "dört yasal metin tamamlanmıştır — firmanın düzenlediği aydınlatma metni ve çerez politikası doludur"
> Sorun: Satış kapısı, aydınlatma metninin yayında olmasını şart koşmaz. Buna karşılık hesap kaydı ve iletişim formu için metnin hem "tamamlanmış ve yayına alınmış" olması istenir (§3.13.19, §3.32.8). Ayrıca §3.33.2’de zorunlu olduğu yazılan alıcı gruplarının adıyla doldurulması da satış kapısının tamamlanmışlık tanımına dahil değildir. Böylece ödeme adımında kişisel veri toplanırken aydınlatma metni taslakta veya eksik kalabilir.
> Öneri: Tek bir “aydınlatma metni tamamlanmış ve yayında” tanımı oluşturun; firma kimliği, barındırma konumu, alıcı grupları ve hukuki sebepler dahil tüm zorunlu alanları bu kapının koşulu yapın. Aynı tanımı satış, kayıt, Google ile ilk giriş ve iletişim formunda kullanın.
>
> BULGU-3
> Kriter: Edge case
> Seviye: Yüksek
> Yer: §10.4 Sipariş müdahaleleri — 10.4.3
> Alıntı: "teslim işaretini almış kalem ... çıkarılamaz"
> Sorun: Kargoya verilmiş ama henüz teslim işareti almamış fiziksel kalemin yönetici tarafından çıkarılması yasaklanmamıştır. Son fiziksel kalem bu aşamada çıkarılırsa §5.4’te `Kargoya verildi` durumundan fiziksel kalemsiz hatta geçecek bir geçiş yoktur; S10 yalnız `Hazırlanıyor → Teslim edildi` geçişini tanımlar. Sipariş sevkiyat durumu, gerçek dünyadaki paket ve geri ödeme işlemi belirsiz kalır.
> Öneri: Fiziksel kalem çıkarılmasını `Kargoya verildi` öncesiyle sınırlayın. Gönderilmiş kalem için cayma, teslim edilemedi veya ayrı bir iade/geri çağırma akışını zorunlu kılın; aksi tercih edilirse `Kargoya verildi` durumundan geçişi, paket sonucunu ve geri ödeme kuralını açıkça ekleyin.
>
> BULGU-4
> Kriter: Belirsizlik
> Seviye: Orta
> Yer: §7.3 Cayma — 7.3.3
> Alıntı: "müşterinin onayıyla başlayan ifa tamamlanınca düşer."
> Sorun: Hizmet ifasının ne zaman “başladığı” tanımlı ve kayıtlı değildir; yalnız “tamamlandı” işareti vardır. Hizmet kısmen ifa edilmişken müşterinin cayması halinde cayma hakkının durumu, geri ödeme tutarı ve varsa kısmi ifanın bedeli belirlenmemiştir. Metindeki “ifa başlaması” ile “ifa tamamlanması” farklı hukuki ve ürün sonuçları doğurabilir.
> Öneri: Hizmet ifasının başlangıç olayını ve zaman damgasını tanımlayın. Kısmi ifa sonrası cayma için uygulanacak yaklaşımı—tam geri ödeme, orantılı bedel veya açıkça tanımlanmış müşteri lehine ek hak—ve buna bağlı ödeme akışını yazın.
>
> SONUÇ: 4 BULGU
> ```

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ❌ RET | Bulgu, Mesafeli Sözleşmeler Yönetmeliği m.12'nin 2022 değişikliğinden önceki metnine dayanıyor. 23/8/2022 tarihli değişiklikten (RG 31932) sonraki **m.12/1** şöyle: *"Satıcı, cayma hakkına konu malın, iade için ön bilgilendirmede belirtilen taşıyıcıya teslim edildiği tarihten itibaren on dört gün içinde [...] tahsil edilen tüm ödemeleri iade etmekle yükümlüdür. Ancak tüketicinin malı, iade için öngörülenin haricinde bir taşıyıcı ile iade etmesi durumunda söz konusu yükümlülük malın satıcıya ulaştığı tarihten itibaren başlar."* Geçici Madde 1'e göre bu hüküm 1/1/2026'dan beri uygulanıyor. "Bildirimin ulaştığı tarih" yalnız teslimden önceki caymada (m.12/2) ve hizmette (m.12/3) başlangıçtır; `02` bu ikisini de beyandan başlatıyor. Firma iade taşıyıcısı belirlemediği için (K-293, K-493) teslimden sonraki her iade "öngörülenin haricinde bir taşıyıcı" ile yapılıyor ve süre malın ulaşmasıyla başlıyor. Bu, K-491'in resmî metne karşı doğrulanmış kararı. §7.4.1 dayanağı, değişikliğin tarihini ve kalan riski zaten yazıyor; Z-16 da aynı kuralı taşıyor. Metinde giderilmesi gereken bir belirsizlik bulunmadı. Bu konu önceki dört turda gelmedi. | Yok. |
| BULGU-2 | ⚠️ KISMİ | Birinci yarısı doğru. K-517 veri toplayan girişlerin kapısını *"tamamlanmış ve yayına alınmış"* yaptı, ama §3.1.5'in satış koşulu *"aydınlatma metni ve çerez politikası doludur"* olarak kaldı. Metin ürünle bir taslakla geldiği için "dolu" koşulu her zaman sağlanıyor. Sipariş onay adımı da (misafir siparişi dahil) kişisel veri topluyor ve aydınlatma bağlantısını gösteriyor (§3.33.5). Bu yüzden satış kapısı iletişim formunun kapısından gevşek kalıyordu. İkinci yarısı reddedildi: alıcı gruplarının adlarının kapı koşulu yapılması. KVKK m.10/1-c ve Aydınlatma Tebliği alıcıyı kategori olarak ister. Tebliğ m.3'ün tanımı şöyle: *"Veri sorumlusu tarafından kişisel verilerin aktarıldığı gerçek veya tüzel kişi kategorisi"*. Tebliğ m.5'e göre aktarılacak alıcı grupları belirtilir. Taslağın alıcı grupları bölümü bu kategorileri zaten sayıyor (§3.33.2, K-553). Grupların adıyla doldurulması §12.5'te firmanın sorumluluğu olarak yazılı. | K-576 `(öneriyle kaydedildi — ⚠)`: satış kapısının dördüncü koşulu aydınlatma için K-517 ile aynı tanımı kullanır (tamamlanmış ve yayına alınmış). Çerez politikası da yayına alınmış olmalıdır. Alıcı gruplarının adları kapıya girmez. §3.1.5, §6.9.10, §10.8.2. |
| BULGU-3 | ✅ KABUL | Doğru. §5.4'ün notu *"adres ve kalem düzeltmesinin sınırı Kargoya verildi"* diyordu. §10.4.3 ise yalnız teslim işaretini almış kalemin çıkarılmasını yasaklıyordu. Bu hâliyle kargodaki fiziksel kalemin çıkarılması açık görünüyordu. Son fiziksel kalem bu aşamada çıkarılırsa "Kargoya verildi"den fiziksel kalemsiz hatta bir geçiş yok (S10 yalnız Hazırlanıyor'dan çıkıyor) ve paket gerçek dünyada yolda. K-374'ün adres için yazdığı gerekçe burada da geçerli. Önerinin ilk seçeneği uygulandı. "Kargoya verildi'den yeni bir geçiş" seçeneği elendi, çünkü yoldaki paketin sonucunu kayıttan bağımsız bırakır (K-225). | K-577 `(öneriyle kaydedildi)`: fiziksel kalem sipariş "Kargoya verildi"ye geçtikten sonra çıkarılamaz. Gönderilmiş malda müşterinin yolu cayma, geri dönen gönderide firmanın yolu Teslim edilemedi hattıdır. Para iadesi gerekiyorsa tutar bazlı kısmi geri ödeme kullanılır. Dijital ve hizmet kalemi teslim işaretine kadar çıkarılabilir. §10.4.3, §5.4. |
| BULGU-4 | ⚠️ KISMİ | Sorun gerçek ama sınırlı. §7.3.3'ün başlığı "müşterinin onayıyla başlayan ifa" ile "tamamlanınca" ifadelerini yan yana kullanıyor. Ürünün ifanın başladığı anı izlemediği yazılı değildi. Kısmen ifa edilmiş hizmetten caymanın parasal sonucu da yazılı değildi. Önerinin çözümü uygulanmadı. K-205 hakkın düştüğü anı bilerek tamamlanma işaretine bağladı. Bu, Yönetmelik m.15/1-h'nin *"tüketicinin onayı ile ifasına başlanan hizmetler"* istisnasından daha geç bir an ve tüketici lehine bir seçim; hukuka aykırılık doğurmuyor. Yönetmelik'te kısmen ifa edilmiş hizmet için orantılı bir bedel hükmü yok, bu yüzden "orantılı bedel" seçeneği de uygulanmadı. Başlangıç işareti ise K-205'i tersine çevirir ve hizmete ikinci bir elle adım ekler. | K-578 `(öneriyle kaydedildi — ⚠)`: ürün ifanın başladığı anı izlemez ve ifanın başlaması hakkı düşürmez. Kısmen ifa edilmiş hizmetten caymada kalemin ödenmiş bedelinin tamamı geri ödenir. Kısmi ifanın maliyeti firmadadır ve bu, kalan risk olarak yazıldı. §7.3.3, yeni §6.4.28, §12.1.9. |

**Dağılım:** 1 KABUL · 2 KISMİ · 1 RET.

Önceki dört turun konuları bu turda dönmedi. BULGU-1 bilinçli ve doğrulanmış bir karara yöneldi, ama "katılmıyorum" itirazı değil, olgusal bir hukuki iddia olarak geldi. Bu yüzden resmî metne karşı yeniden kontrol edildi ve iddia doğrulanmadı.

## 3. Ek bulgular

- **Etki yansıtma için not:** Üç kararın başka dokümanlarda karşılığı var:
  - `10` §2 KP-48 yöneticinin *"teslim işaretini almamış kalemi"* çıkardığını söylüyor. K-577'den sonra fiziksel kalem için sınır "Kargoya verildi"dir.
  - `10` KP-38 ve §4.1 satış kapısının yasal metin koşulunu anıyor. K-576'nın "yayına alınmış" tanımına hizalanmalı.
  - `03` ve `04` hizmet kaleminin cayma ekranında kısmi ifada tam geri ödemeyi (K-578) devralır.

  Bunlar bir çelişki doğurmuyor, yalnız ifade farkı; etki yansıtma adımında toplu hizalanacak.

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`02` v0.20). Üç karar satırı açıldı:

- K-576 `(öneriyle kaydedildi — ⚠)`
- K-577 `(öneriyle kaydedildi)`
- K-578 `(öneriyle kaydedildi — ⚠)`

K-576 kişisel veriye ve yasal yükümlülüğe, K-578 paraya dokunduğu için ikisi proje sahibinin gözden geçirme listesine girer. K-578'in seçenekleri gerçekten ayrışıyor. Proje sahibi firmayı korumayı öncelerse yasal asgari olan "ifaya başlandı işareti" seçeneği karar kaydında gerekçesiyle duruyor. Seçilmeme sebebi: K-205'i tersine çevirir, elle adım ekler ve ispat yükü doğurur. Karar kaydında K-465, K-517, K-374, K-537 ve K-205'in etki sütunlarına yeni satırlara atıf eklendi.

- [x] BULGU-1 · [x] BULGU-2 · [x] BULGU-3 · [x] BULGU-4

**Hukuki kontrol:**

- **BULGU-1:** Mesafeli Sözleşmeler Yönetmeliği'nin mevzuat.gov.tr'deki konsolide metnine (MevzuatNo 20237, 2026-10-03'te indirilen kopya) karşı kontrol edildi: m.12/1–3, m.13/1 ve Geçici Madde 1. Sonrasında yayımlanan bir değişiklik aramada çıkmadı; son değişiklik RG 24/5/2025-32909'dur ve m.12/1'e dokunmaz.
- **BULGU-2:** KVKK m.10/1-c ve Aydınlatma Yükümlülüğünün Yerine Getirilmesinde Uyulacak Usul ve Esaslar Hakkında Tebliğ'in m.3 ve m.5'ine karşı kontrol edildi.
- **BULGU-4:** Yönetmelik m.15/1-h'ye karşı kontrol edildi; Yönetmelik'te kısmi ifa için orantılı bedel hükmü bulunmadığı aynı konsolide metinde doğrulandı.
