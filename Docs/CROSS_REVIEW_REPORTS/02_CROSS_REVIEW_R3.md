# Cross-Review — 02 Product Requirements (Tur 3)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/02_PRODUCT_REQUIREMENTS.md` v0.17 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki turlar ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde). İstem 1. ve 2. turunkiyle aynıdır: yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi. Bu turdan önce dal `main`'e taşındı ve dalın karar numaraları K-560…K-567'den K-561…K-568'e kaydırıldı (`main`'deki K-560 A-13'ündür); 1. ve 2. tur raporlarındaki atıflar yeni numaraları gösterir.

> ```text
> BULGU-1
> Kriter: teknik doğruluk
> Seviye: Yüksek
> Yer: §7.2.8 İptal edilen ödenmiş kalemin parası
> Alıntı: "Firma panelden yeniden dener ya da havaleyle öder"
> Sorun: Kartla yapılan ödemenin iadesinde firma, müşterinin açık tercihi/onayı olmadan havale yoluna geçebiliyor. Bu, iadenin kural olarak kullanılan ödeme aracıyla yapılması ilkesini ihlal edebilir.
> Öneri: Havale alternatifini müşterinin sipariş sayfasında vereceği açık onaya bağlayın; onay yoksa kart iadesini yeniden deneme veya sağlayıcı üzerinden çözüm yolunu koruyun.
>
> BULGU-2
> Kriter: edge case
> Seviye: Orta
> Yer: §7.4.7 İade edilen fiziksel kalemin stoğa eklenmesi
> Alıntı: "kalemin teslim tarihinden önce ... tarihi ... reddeder"
> Sorun: Müşteri malı teslim almadan caydığında gönderi firmaya dönebilir; bu durumda kalemin teslim tarihi hiç oluşmamış olabilir. Buna rağmen iade teslim alma tarihi girilemediği için malın dönüşü, stok işlemi ve ilgili iade akışı kilitlenir.
> Öneri: Bu tarih kısıtını yalnız teslimden sonraki iadeler için uygulayın; teslimden önce cayılıp firmaya dönen gönderiler için teslim tarihi gerektirmeyen ayrı bir teslim alma kaydı tanımlayın.
>
> BULGU-3
> Kriter: kullanıcı deneyimi
> Seviye: Yüksek
> Yer: §6.4.12 İptal, cayma ve iade
> Alıntı: "Cayma düğmesi kapanır"
> Sorun: Firma teslimi geç işaretleyip geçmiş tarih girdiğinde müşteri, fiilî cayma süresinde başvurmuş olsa bile ürün içindeki ana cayma yolunu kullanamaz. Dış kanaldan bildirim hakkının bulunması, ürünün kendi kanalının satıcının geç kaydıyla kapanmasını telafi etmez.
> Öneri: Teslim kaydı geç girildiğinde cayma yolunu müşterinin aleyhine kapatmayın; en azından kaydın girildiği andan itibaren ek bir 14 günlük güvenli pencere veya “teslim tarihi uyuşmazlığı” başvuru yolu tanımlayın.
>
> BULGU-4
> Kriter: tutarlılık
> Seviye: Yüksek
> Yer: §5.6.1 İki eksenin bağı ve §10.4.6 Yanlış yapılmış durum geçişi
> Alıntı: "Ödendi'den Bekliyor'a düzeltilemez, önce sevkiyat düzeltilir"
> Sorun: Metin, sevkiyat düzeltildikten sonra Ödendi → Bekliyor düzeltmesinin mümkün olduğunu ima ediyor; ancak §5.5 ödeme beyaz listesi bu geçişi tanımlamıyor. Böyle bir düzeltmede stok, hizmet kontenjanı, kupon hakkı ve kargoya verme süresinin nasıl geri alınacağı da belirsiz.
> Öneri: Ödeme yanlış işaretleme düzeltmesini §5.5 ve §10.4.6’da açıkça tanımlayın: izin koşulları, hedef durum, stok/kupon yan etkileri ve düzeltmenin yapılamayacağı noktalar belirlenmeli.
>
> BULGU-5
> Kriter: belirsizlik
> Seviye: Yüksek
> Yer: §7.2.3 Teslimat gecikmesinde fesih
> Alıntı: "Tahsil edilen tutar kargo ücreti dahil ... geri ödenir"
> Sorun: Gecikme feshi yalnız teslim edilmemiş fiziksel kalemlere uygulanırken, karışık siparişte iade edilecek tutarın kapsamı belirsizdir. İfade, teslim edilmiş dijital veya hizmet kalemlerinin de tüm bedelinin iade edilmesi şeklinde yorumlanabilir; kupon payı ve kargo ücretinin hangi kaleme ait olduğu da belirtilmemiştir.
> Öneri: Fesihte yalnız etkilenmiş fiziksel kalemlerin siparişte donmuş net bedellerinin ve uygulanıyorsa kargo ücretinin iade edildiğini; teslim edilmiş dijital/hizmet kalemlerinin kapsam dışında kaldığını açıkça yazın.
>
> BULGU-6
> Kriter: tutarlılık
> Seviye: Orta
> Yer: §12.2.3 Hukuki sebepler
> Alıntı: "iletişim talebi → m.5/2-f"
> Sorun: İletişim talebi türleri arasında KVKK talebi de vardır; aynı bölüm KVKK başvurusunun kaydını m.5/2-ç’ye dayandırır. Böylece KVKK talebinin hukuki sebebi iki farklı şekilde tanımlanmış olur.
> Öneri: Genel iletişim taleplerini ve KVKK başvurularını açıkça ayırın; KVKK talebinin alınması, kaydı ve yanıtlanmasını m.5/2-ç kapsamında; diğer iletişim taleplerini seçilen ayrı hukuki sebep kapsamında gösterin.
>
> SONUÇ: 6 BULGU
> ```

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ⚠️ KISMİ | İddianın çekirdeği abartılı: §7.2.8'in havale yolu zaten müşterinin IBAN'ı sipariş sayfasından kendisinin girmesine bağlıydı (K-498, K-521) ve firma IBAN'ı panelden giremez (§8.4.2) — firma müşterinin iradesi olmadan havaleye geçemiyordu. Ama metin (*"Firma panelden yeniden dener ya da havaleyle öder"*, B-14'te *"firma havaleyle ödemeyi seçti"*) havaleyi firmanın tek taraflı seçimi gibi okutuyordu ve IBAN istendikten sonra kart yolunun açık kalıp kalmadığı yazılı değildi. Resmî metin kontrol edildi: Mesafeli Sözleşmeler Yönetmeliği m.12/4 (mevzuat.gov.tr konsolide metni; 24/5/2025 değişikliği yalnız "13 üncü maddenin üçüncü fıkrası hükmü saklı kalmak üzere" ibaresini kaldırdı) geri ödemenin *"tüketicinin satın alırken kullandığı ödeme aracına uygun bir şekilde"* yapılmasını ister. Önerinin ayrı bir onay kutusu kısmı uygulanmadı — IBAN'ı girmek tercihin kendisidir. | K-569 `(öneriyle kaydedildi — ⚠)`: havale yolu müşterinin seçimidir; sipariş sayfası kart iadesinin gerçekleşmediğini söyler; IBAN girilene kadar kart iadesi yeniden denenebilir, kart iadesi gerçekleşirse IBAN alanı kapanır, IBAN girildikten sonra kart yeniden denenmez (çift ödeme kapanır). §7.2.8, §5.5, §6.4.24, §7.4.5, §8.4.2, §9.2 B-14, §10.1. |
| BULGU-2 | ⚠️ KISMİ | Senaryo doğru: kargodayken cayılan ve alıcıya teslim edilemeyip firmaya dönen gönderide (§6.4.22) kalemin teslim tarihi yoktur. Ama sonuç abartılı: §6.4.22 bu gönderinin teslim alma adımıyla işleneceğini açıkça yazıyor ve teslimden önceki caymada geri ödeme süresi zaten beyandan işler (§7.4.1) — akış kilitlenmiyor. Sorun §7.4.7'nin *"kalemin teslim tarihinden önce ... olamaz"* ifadesinin teslim tarihi olmayan kalemde nasıl okunacağının yazılmamasıydı. Önerilen ayrı kayıt tipi gereksiz; mevcut adım yeter. | §7.4.7: alt sınır yalnız teslim tarihi girilmiş kalemde uygulanır; teslimden önce cayılıp dönen gönderide adım aynı biçimde işler. Karar kaydı gerekmedi (§6.4.22 ve K-491'in sonucu). |
| BULGU-3 | ❌ RET | Önerilen çözüm — işaret anından ek on dört günlük pencere — K-340'ın bilinçle elediği seçenektir (*"Geç işaretlemede pencere işaret tarihinden başlatılır" elendi: K-203 başlangıcı teslim tarihine bağladı ve firma kendi gecikmesinden yararlanarak pencereyi ileri kaydıramaz; tersi de doğrudur — gecikmenin riskini firma taşır*). Yasal pencere fiilî teslimden işler (Yönetmelik m.9); doğru girilmiş tarihte süre gerçekten dolmuştur. Bulgudaki *"fiilî cayma süresinde başvurmuş olsa bile"* müşteri korunuyor: daha önce yapılmış beyan geçerli kalır (§7.3.1) ve başka kanaldan gelen bildirim kendi ulaşma tarihiyle kaydedilir (§7.3.5, §10.4.10; K-510); uydurulmuş tarihin ispat yükü firmadadır (K-07, K-288). Bulgunun dönmesinin sebebi seçimin gerekçesinin `02`'de yazılı olmamasıydı. | Öneri uygulanmadı. Belirsizlik giderildi: §7.3.1'e pencerenin işaret anından yeniden başlatılmamasının bilinçli bir seçim olduğu ve gerekçesi bir cümleyle yazıldı. |
| BULGU-4 | ✅ KABUL | Doğru. §5.6.1 ve §10.4.6 *"sipariş Hazırlanıyor'dayken ödeme Ödendi'den Bekliyor'a düzeltilemez, önce sevkiyat düzeltilir"* diyerek (K-504) ödeme düzeltmesinin var olduğunu ima ediyordu; ama §5.5'in geçiş listesi, §10.4.6'nın *"ödeme hiçbir düzeltmede kendiliğinden değişmez"* cümlesi ve §10.4'ün sekiz müdahalesi böyle bir geçişi tanımlamıyordu. Gerçek hata da yaygındır: K-466'nın iki sebebi (yanlış siparişte işlem · işlem gerçekleşmeden işaretleme) havale işaretinde olur; düzeltilemezse parası gelmemiş sipariş ödenmiş görünür. Önerinin iki ekseni ayrı adımlarla düzeltme okuması uygulanmadı — arada §5.6.1'in yasak birleşimi (Hazırlanıyor + Bekliyor) doğardı. | K-570 `(öneriyle kaydedildi — ⚠)`: düzeltilebilen tek ödeme geçişi para gelmeden konmuş havale "ödendi" işaretidir; iki eksen birlikte geri alınır (Ödendi → Bekliyor, Hazırlanıyor → Alındı); koşul kargoya verilmemiş, geri ödemesiz ve teslim işaretsiz sipariş (dijital kalemli siparişte yapılamaz); ayırmalar tutulur, ödeme süresi düzeltmeden yeniden başlar, B-13 havale bilgisini ve yeni son günü taşır; kartta ödeme düzeltmesi yoktur. §10.4.6, §5.5, §5.6.1, §4.2 Z-8, §6.7.5, §9.2 B-13. |
| BULGU-5 | ✅ KABUL | Doğru. §7.2.3 feshin kapsamını kalem düzeyinde yazıyor (*"teslim edilmemiş fiziksel kalemlerin tamamına uygulanır"*) ama tutarı *"Tahsil edilen tutar kargo ücreti dahil"* diye anıyordu; karışık siparişte teslim edilmiş dijital ya da hizmet kaleminin bedelinin de geri ödeneceği okunabiliyordu ve aynı ifade sözlükte (§1.2), Z-11'de ve §12.5'te tekrarlanıyordu. Resmî metin kontrol edildi: Yönetmelik m.16/3 *"varsa teslimat masrafları da dâhil olmak üzere tahsil edilen tüm ödemeleri"* der; ürün iptal ve caymayı kalem düzeyinde kurduğu için (K-197, K-178) fesih gecikmenin dokunduğu kalemlerle sınırlandı ve tüketici lehine okuma kalan risk olarak yazıldı. | K-571 `(öneriyle kaydedildi — ⚠)`: geri ödenen tutar feshedilen kalemlerin ödenmiş bedelleri (kupon payı §7.2.7'nin kalıbıyla) ve kargo ücretidir; teslim edilmiş dijital ve hizmet kalemleri ile tamamlanmamış hizmet kalemi feshin dışındadır; kalan risk yazıldı. §7.2.3, §1.2, §4.2 Z-11, §12.5. |
| BULGU-6 | ✅ KABUL | Doğru ve küçük. KVKK talebi iletişim formunun bir konu tipidir (K-118, K-302); §12.2.3 *"KVKK başvurusunun kaydı → m.5/2-ç"* ve *"iletişim talebi → m.5/2-f"* diyerek aynı başvuruyu iki sebebe bağlıyordu. Resmî metin kontrol edildi: KVKK m.13/1–2 başvurunun veri sorumlusuna iletilmesini ve en geç otuz günde sonuçlandırılmasını yükümlülük olarak yazar; m.5/2-ç doğru sebeptir. | K-572 `(öneriyle kaydedildi — ⚠)`: KVKK talebinin alınması, kaydı ve cevaplanması m.5/2-ç; m.5/2-f KVKK talebi dışındaki iletişim talepleridir. §12.2.3. Aydınlatma taslağı sebepleri §12.2.3'ten aldığı için (§3.33.2) ayrı değişiklik gerekmedi. |

**Dağılım:** 3 KABUL · 2 KISMİ · 1 RET.

Tur 1 ve 2'nin konuları bu turda dönmedi. BULGU-3, K-340'ın bilinçli seçimine yöneldi; gerekçe artık `02`'de yazılı.

## 3. Ek bulgular

Yok. Düzeltmeler sırasında görülen iki yan etki aynı kararlarla kapandı: BULGU-1'de §7.4.5 ve §8.4.2'nin "firmanın havaleyle ödemeyi seçtiği" ifadeleri ve §10.1'in panel kapsamı satırı (K-569); BULGU-4'te havale ödeme süresinin başlangıcı (Z-8) ve düzeltme bildiriminin içeriği (B-13) (K-570). `10` KP özetindeki *"firma yeniden dener ya da havaleyle öder"* cümlesi bir çelişki değil özettir; etki yansıtma adımında hizalanacak.

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`02` v0.18). Dört karar satırı açıldı: K-569, K-570, K-571 ve K-572 `(öneriyle kaydedildi — ⚠)` — para, yasal hak ya da kişisel veriye dokundukları için proje sahibinin gözden geçirme listesine girer. Karar kaydında K-499, K-504, K-516 ve K-521'in etki sütunlarına yeni satırlara atıf eklendi.

- [x] BULGU-1 · [x] BULGU-2 · [x] BULGU-3 · [x] BULGU-4 · [x] BULGU-5 · [x] BULGU-6

**Hukuki kontrol:** BULGU-1 Mesafeli Sözleşmeler Yönetmeliği m.12/4'e, BULGU-5 m.16/3'e, BULGU-6 KVKK m.5/2 ve m.13'e karşı mevzuat.gov.tr metinleriyle (2026-10-03) doğrulandı; m.12/4'teki 24/5/2025 değişikliği (RG 32909) yalnız m.13/3 atfını kaldırdı, ödeme aracı kuralı yürürlüktedir.
