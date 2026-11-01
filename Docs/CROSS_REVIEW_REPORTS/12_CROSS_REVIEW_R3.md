# Cross-Review — 12 Doğrulama Protokolü (Tur 3)

**Tarih:** 2026-11-01 · **Hedef:** `Docs/12_VALIDATION_PROTOCOL.md` v0.26 (girdi commit'i `d21f850`, 836 470 bayt, 2 639 satır) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Ciddiyet ölçüsü:** 1. turdan itibaren (K-1399 (1); mekanizma okuma paragrafıyla) · **Sonuç:** TEMİZ DEĞİL — 7 ayrı bulgu (7 ham, altı çağrıdan): 3 kısmen · 3 çürük · 1 doğrulayıcısız tekrar eden RET; 3'ü uygulandı (2 şartname çifti ve 1 hücre; biri kaynak devri).

## 1. Koşum

Betikler, istem, parça notu ve **parça sınırları değişmedi** (`12_CROSS_REVIEW.md` §1; kayıt K-1561): `C:\Users\teknodeveloper\.claude\helper-state\shopfolio\a10-kd-cross\betik\` — `build_inputs.py` (`python -I build_inputs.py <doküman> template12.md prompt.txt partnote.txt <girdi klasörü>`; kapsanmayan satır: yok), `prompt.txt`, `partnote.txt`, `template12.md`. Koşum: `bash scripts/doc-scans/cross_review_ikinci_model.sh <girdi> <çıktı> c1 p1 p2 p3 p4 p5` (K-1484). Altı çağrı paralel koştu, hepsi çıktı verdi; kullanım sınırı dolmadı, yedek modele geçilmedi (`status.txt`: `model=codex gpt-5.6-terra high`). Girdi `12` v0.26: önceki turlardan farkı hedefli mercekte değişen §6.2, §6.3, §7.5, §8.2, §8.4, §11.1 ve üst/alt bilgi.

| Çağrı | Girdi | Token | Süre | Sonuç |
|---|---|---|---|---|
| tek parça (c1) | 853 953 bayt | 418 811 | 215 sn | 1 bulgu |
| P1 (§1–§4.2, §5–§11) | 134 734 bayt | 48 509 | 100 sn | TEMİZ |
| P2 (§4.3–§4.12) | 279 378 bayt | 139 539 | 192 sn | 4 bulgu |
| P3 (§4.13–§4.20) | 336 171 bayt | 147 171 | 113 sn | TEMİZ |
| P4 (§12 girişi, §12.1–§12.7) | 201 386 bayt | 96 419 | 164 sn | TEMİZ |
| P5 (§12.8–§12.13) | 215 531 bayt | 108 709 | 172 sn | 2 bulgu |

**İki koşumun ölçümü.** Tek parça çağrı bir bulgu getirdi (VAL-A907 süre sayacı; 1. ve 2. turda iki kez reddedilmiş iddia); parçalı koşum altı ham bulgu getirdi (P2 4, P5 2). Tek parça ile parçalı koşum bu tur birbirinin bulgusunu üretmedi: tek parçanın tek bulgusu hiçbir parçada çıkmadı. Yasal dayanak taşıyan iki iddia (VAL-A081/A231 cayma bildiriminin zamanı; VAL-A900 ayıplı adedin talebi) doğrulayıcılarda resmî metne karşı okundu (okuma tarihi 2026-11-01): Mesafeli Sözleşmeler Yönetmeliği m.11/1 ve m.11/4, 6502 sayılı Kanun m.11/1-a (mevzuat.gov.tr, resmigazete.gov.tr).

## 2. Ham bulgular

İkinci modelin çıktıları, satır sonu boşlukları dışında düzenlenmeden (alıntı blokları içinde).

**(a) Tek parça çağrı:**
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §12.9, `VAL-A907`
> Alıntı: "Ayıpta sözleşmeden dönmenin geri ödemesinin süre sayacı yoktur"
> Sorun: Bu, ayıplı mal veya hizmette sözleşmeden dönme seçildiğinde bedelin “derhal” iade edilmesini zorunlu kılan 6502 sayılı Kanun m.11/5 ve m.15/3 ile çelişir. Süresiz akış, geri ödemeyi denetlenemez biçimde geciktirerek tüketicinin para kaybına ve firmanın mevzuat ihlaline yol açar. [Resmî Kanun metni](https://tuketici.ticaret.gov.tr/data/5e81982d13b876a1b04c7a42/2023-6502%20Say%C4%B1l%C4%B1%20T%C3%BCketicinin%20Korunmas%C4%B1%20Hakk%C4%B1nda%20Kanun.pdf)
> Öneri: `VAL-A907` ve bağlı kaynak ifadesini, ayıpta dönme bedelinin derhal iade edilmesini zorunlu ve sınanabilir bir zaman kuralına bağlayacak şekilde düzeltin; havale için IBAN beklenmesi ayrıca, izlenebilir bir bekleme durumu olarak tanımlansın.
>
> SONUÇ: 1 BULGU

**(b) Parça 1:**
> SONUÇ: TEMİZ

**(c) Parça 2:**
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §4.6 VAL-A080–VAL-A081; §4.10 VAL-A231
> Alıntı: “kaydın tarihi cayma süresi hesabında esas alınır”
> Sorun: Cayma bildiriminin süresinde olup olmadığı, firmaya ulaşma anına göre değil süre dolmadan yazılı veya kalıcı veri saklayıcısıyla yöneltilmiş olmasına göre değerlendirilir. Doküman, gönderim zamanını tutacak alanı reddedip ulaşma tarihini esas alıyor; bu, zamanında gönderilmiş bildirimin reddine ve tüketicinin cayma hakkını kaybetmesine yol açar. Ayrıca VAL-A080’in “süresinde gönderilmiş” geç ulaşan bildirimin kaydedileceği kuralıyla çelişir. [Ticaret Bakanlığı açıklaması](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/mesafeli-sozlesmeler-hakkinda-bilgilendirme)
> Öneri: Bildirimin gönderim/yöneltim zamanı ile ulaşma zamanını ayrı kaydedin; cayma süresi değerlendirmesini kanıtlanmış yöneltim zamanına bağlayın. VAL-A080, VAL-A081 ve VAL-A231’i bu kuralla hizalayın.
>
> BULGU-2
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §4.8 VAL-A126
> Alıntı: “Kanıt türü | Tarayıcı gözlemi | yok | KRİTİK”
> Sorun: Dokümanın kendi minimum kanıt kuralına göre KRİTİK satır en az iki farklı kanıt türü taşımalıdır. Bu satır tek kanıt türüyle PASS alabilir; böylece zorunlu olabilecek yasal kimlik bilgilerinin tüm ilgili yüzeylerde doğru yayımlandığı yeterince doğrulanmadan final kapısı geçilebilir. Bu, dokümanın kendi güvencesinin ihlalidir (4).
> Öneri: Tarayıcı gözlemine ek olarak uç yanıtı veya yapılandırma/veri kaydı kanıtını zorunlu kılın.
>
> BULGU-3
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §4.10 VAL-A226
> Alıntı: “Kanıt türü | Tarayıcı gözlemi | yok | KRİTİK”
> Sorun: Satır, değer kaybı gerekçesiyle geri ödemeden kesinti yapılamamasını yalnız ekran gözlemiyle doğrulatıyor. Arayüzde alan olmaması, doğrudan uç veya arka uç hesaplamasının kesinti yapmadığını kanıtlamaz; müşteriye eksik geri ödeme yapılması doğrulanmadan kalabilir. Ayrıca KRİTİK satır için iki farklı kanıt türü şartı ihlal edilmektedir (2), (4).
> Öneri: Gerçekleştirilmiş geri ödeme için uç yanıtı ve ödeme/veri kaydı kanıtını ekleyin; tutarın kesintisiz tam iade olduğunu bu kanıtlarla sınayın.
>
> BULGU-4
> Kriter: Güvenlik
> Seviye: Yüksek
> Yer: §4.12 VAL-A370
> Alıntı: “Kanıt türü | Tarayıcı gözlemi | yok | KRİTİK”
> Sorun: “Ürün hiçbir noktada açık rıza toplamaz” kuralı yalnız görünür ekranların incelenmesiyle PASS alabilir. Görünmeyen istemci isteği, uç veya veri kaydında açık rıza toplanması bu kanıtla dışlanamaz. KRİTİK satırın iki farklı kanıt türü taşıması kuralı da ihlal edilmektedir (4).
> Öneri: Tarayıcı gözlemine ek olarak ilgili istek/yanıtların ve saklanan veri kayıtlarının denetimini zorunlu kanıt yapın.
>
> SONUÇ: 4 BULGU

**(d) Parça 3:**
> SONUÇ: TEMİZ

**(e) Parça 4:**
> SONUÇ: TEMİZ

**(f) Parça 5:**
> BULGU-1
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: §12.9 / VAL-A900
> Alıntı: "İkinci talep açılamaz; talep yeniden açılır ve ayıplı adet, açılırken seçilen adet olarak kalır"
> Sorun: Sonradan ayıbı ortaya çıkan başka adet için yeni talep yasaklanırken, yeniden açılan talebin adedi de artırılamıyor. Böylece bu adet hiçbir ayıp talebine konu olamaz; tüketicinin seçimlik hakkı fiilen çıkışsız kalır. Bu, satıcının ayıplı maldaki tüketici talebini yerine getirme yükümlülüğüyle çelişir. [6502 sayılı Kanun, m.11](https://tuketici.ticaret.gov.tr/data/5e81982d13b876a1b04c7a42/Tuketicinin_Korunmasi_Hakkinda_Kanun_6502_Ocak_2021.pdf)
> Öneri: Yeniden açılan talebin ayıplı adet kapsamını, önceden işlenen adetleri koruyarak artırın veya aynı kalemde ek adet için ayrı talep açılmasına izin verin; her adedin talep ve çözüm izini ayrı doğrulayın.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: Aşama 4 park girdisi K-884 / §12.10 VAL-A913
> Alıntı: "Düğmeye basış ile dosyanın gönderiminin başlaması tek adımdır ve tek hak yer: bağlantı üretilince kalan hak bir azalır"
> Sorun: Park girdisi, hakkın “dosyanın gönderimi başladığında” düşeceğini söylerken satır, yalnız bağlantı üretilince düşürüyor. Bağlantı oluşturulup aktarım başlamazsa müşterinin indirme hakkı dosyayı almadan kaybolabilir. Bu hem doküman içi çelişkidir hem de ücretli dijital üründe hak/para kaybına yol açar.
> Öneri: Hakkı bağlantı üretiminde değil, dosya gönderimini başlatan isteğin kabulüyle atomik olarak düşürün; bağlantı üretimi sonrası aktarım başlamazsa hakkın aynı kaldığını ayrıca sınayın.
>
> SONUÇ: 2 BULGU

## 3. Bağımsız değerlendirme

Yedi ham bulgu yedi ayrı bulgudur (birleşme yok). Altı bulgu için altı Opus/high doğrulayıcı (D1 F1; D2 F2; D3 F3; D4 F4; D5 F5; D6 F6; **adım sınırı 6/20**). **F7** (VAL-A907) üçüncü kez gelen aynı yer ve aynı iddiadır; skill Faz 4 madde 5 gereği "tekrar eden RET" olarak kaydedilir ve doğrulayıcıya gitmez (sınır tasarrufu; hüküm 1. tur F7 ve 2. tur F5'tir). Hüküm dosyaları: `C:\Users\teknodeveloper\.claude\helper-state\shopfolio\a10-kd-cross\r3\hukum_D1.md`…`hukum_D6.md` (`12_audit_hukum_denetle.py`: hüküm kısmen 3 · çürük 3, çift 2, sorun 0).

| # | Parça · Yer | Konu | Hüküm | Seviye (model → doğrulayıcı) | Gerekçe ve şartname |
|---|---|---|---|---|---|
| F1 | P2 · §4.6 VAL-A080, VAL-A081; §4.10 VAL-A231 | Cayma süresi gönderime değil ulaşmaya bağlanıyor; gönderim tarihi tutulmuyor | **kısmen** | Yüksek → Orta | Ana iddia (gönderim tarihi alanı, geçerliliğin gönderime bağlanması) **çürük** ve dördüncü kez geliyor (audit 95, deep-2a 23, 2. tur F3): gönderim tarihini tutmamak K-709'un bilinçli kararıdır ("Kayıt iki tarih taşısın" elendi); 12:539 uyarı ve ayrı onayla süresinde gönderilmiş, geç ulaşmış hâli kapatır; Yönetmelik m.11/1 ("yöneltilmesi yeterlidir") ve m.11/4 (ispat tüketicide) bu kararla uyumludur. **Yeni ve doğru kısım:** 12:769 VAL-A231 Beklenen "kaydın tarihi cayma süresi hesabında esas alınır" `02 §10.4.10` ("geri ödeme süresi ... bu tarihle işler; süreye uygunluk ise bildirimin süresi içinde gönderilmiş olmasıyla ölçülür") ve `12`'nin kendi VAL-A080/A081 satırlarıyla çelişir (4). **Uygulandı (1 çift):** VAL-A231 Beklenen: kaydın tarihi geri ödeme süresinin ve teslimden önce/sonra ayrımının hesabında esas alınır; süreye uygunluk gönderimle ölçülür, ürün gönderim tarihini tutmaz. Üst doküman notu aşağıda (§4). |
| F2 | P2 · §4.8 VAL-A126 | KRİTİK satır tek kanıt türüyle PASS alabilir | **çürük** | Yüksek → Düşük | Alıntı yanlış: 12:627 satırı "Tarayıcı gözlemi + Uç yanıtı" iki tür taşır ve §7.2 (12:373) karşılanır; ikinci modelin kendi girdisinde de (`p2.txt:449`) böyledir. |
| F3 | P2 · §4.10 VAL-A226 | KRİTİK satır yalnız ekran gözlemiyle kesintisiz iadeyi doğrular | **çürük** | Yüksek → Düşük | Alıntı yanlış: 12:762 "Tarayıcı gözlemi + Uç yanıtı" iki tür taşır (v0.5'ten beri); kesintisiz tam iade arka uç kanıtıyla VAL-A224 ve VAL-A225'te (12:760–761, "Uç yanıtı + Veri kaydı") sınanır; `02 §7.4.10` ile uyumlu. |
| F4 | P2 · §4.12 VAL-A370 | "Açık rıza toplamaz" yalnız ekran gözlemiyle PASS alır | **kısmen** | Yüksek → Orta | "Tek kanıt türü" iddiası çürük (12:971 satırı iki tür taşır, 12:373 karşılanır). **Öz geçerli:** Beklenen yalnız ekranı söylüyor; kaynak rıza kaydının yokluğunu ve onay kutularının KVKK rızası olmadığını da yazıyor (`02 §12.2.4`, `02 §8.4.1`) — satır-kaynak açığı (4). **Uygulandı (1 çift + 1 hücre):** Beklenen ekran dışında rıza yönetimi ekranı ve geri alma yolunun yokluğunu, isteklerin ve yanıtların rıza alanı taşımamasını, veri kayıtlarında rıza kaydının yokluğunu ve onay kutusu kaydının sözleşmeye ilişkin onay olarak tutulmasını sayar; Kanıt türü `Tarayıcı gözlemi + Uç yanıtı + Veri kaydı`. Resmî KVKK metni indirildi ama metin çıkarılamadı; hüküm kaynağın kendi kuralına dayanır, yasal atıf yoktur. |
| F5 | P5 · §12.9 VAL-A900; K-793 / K-808 | Yeniden açılan talepte ayıplı adet artırılamıyor; yeni talep de açılamıyor — sonradan ayıbı çıkan adet çıkışsız | **kısmen** | Yüksek → Orta | `12` kaynağı birebir yansıtır (K-793 "adet talep açılırken seçilir"; `04 §5.19.4` yeniden açma ekranında adet alanı yok; `04 §9.9.30`). Gerçek çelişki **kaynakta**, iki karar arasında: K-793 bildirim yolu ile K-808 para yolu (sözleşmeden dönme yalnız "talebin ayıplı adetleri" için işler; teslim almanın üst sınırı "talebin ayıplı adedinden henüz teslim alınmamış olanıdır") birbirini kapatıyor — 1 adetle açılıp o adetten dönülmüş kalemde sonradan ayıbı çıkan ikinci adet için tüketici sözleşmeden dönmeyi seçerse (6502 m.11/1-a: satıcı tercih edilen talebi yerine getirmekle yükümlü) panelde teslim alma ve `02 §7.1.7` tutarlı iade yolu yoktur (4), para yolunda (2). Orta: tutar bazlı bedel indirimi yine ödenebilir, hak tamamen kaybolmaz. **`12`'de değişiklik yok** (K-652); `UST` ve `KARAR` ⚠ aşağıda (§4, K-1566). |
| F6 | P5 · §12.10 VAL-A913; K-884 park girdisi | Hak bağlantı üretilince düşer, K-884 dosya gönderimi başlayınca der | **çürük** | Yüksek → Düşük | **Önceki hüküm** (1. tur F8, K-1561): satır `05 §7.7.4` ilk cümlesini birebir taşır ("bastığı an ile gönderimin başlaması tek adımdır ve tek hak yer"); K-1225 (1): hak bağlantının üretildiği anda düşer, K-884'ün kuralı değişmez; park girdisi (12:41) K-884'ün özetidir, çelişki değildir. Hak kaybı yoktur: üretilen bağlantı bir saat boyunca ek hak düşürmeden yeniden istenebilir (VAL-A914); önerilen model K-884 / K-1225'te elenen yönde kaynağı değiştirir. **Tekrar eden RET:** bu iddia 2. kez gerekçeyle reddedildi (1. tur F8; bu tur). |
| F7 | tek parça · §12.9 VAL-A907 | Ayıpta sözleşmeden dönmenin geri ödemesinin süre sayacı yoktur — 6502 m.11/5, m.15/3 | **çürük (doğrulayıcısız)** | Yüksek → Düşük | **Tekrar eden RET (3. kez):** 1. tur F7 ve 2. tur F5'te gerekçeyle reddedildi (K-716 bilinçli karar; "derhâl" bir gün sayısı değildir; yükümlülük `02 §12.5`'te firmaya yazılı; iade "geri ödeme gerçekleşmedi" işareti, bekleyen işler sayacı ve panel uyarısıyla izlenir). Aynı yer, aynı iddia, yeni kanıt yok; öneri (IBAN beklenmesinin izlenebilir bekleme durumu olması) VAL-A906 ve işaret mekanizmasında zaten vardır. Turun sonucunu belirlemez (skill Faz 4 madde 5). |

**Hüküm dağılımı:** 3 kısmen (F1, F4, F5) · 3 çürük (F2, F3, F6) · 1 doğrulayıcısız tekrar eden RET (F7); seviyesi yükselen yok. Seviye: hepsi Yüksek iddia edildi; F1, F4, F5 Orta, F2, F3, F6, F7 Düşük.

**Sonuç sınıfı ve sıradaki adım.** Üç kabulün üçü de Orta ve hepsi (4) sınıfı: F1 ve F4 `12` satırının Beklenen hücresinin kaynak kuralın tamamını taşımaması (satır ↔ kaynak), F5 iki kaynak kararın birbirini kapatması. **Bu tur sınıf tek turluk:** hedefli mercek koşulu (skill Faz 4 madde 6) art arda **iki** turda aynı sınıfın Orta kabulüdür; 2. turun Orta kabulü kabul edilmiş risk sınıfındaydı ve hedefli merceği alındı, bu turun Orta kabulleri farklı sınıftadır (VAL satırı Beklenen hücresinin kaynakla farkı). **4. tur gerekir**; 4. turun da Orta kabulü satır ↔ kaynak sınıfından çıkarsa 5. tur öncesi hedefli mercek koşulur. Etki yansıtma TEMİZ'den sonradır. 5. tur çıkış kuralı (skill Faz 4 madde 5) 5. turdan itibaren işler. Tekrar eden RET sayısı: F6 (2. kez), F7 (3. kez), F1'in gönderim tarihi kısmı (4. kez).

## 4. Ek bulgular ve devirler

- **UST (F1, VAL-A231):** `02_PRODUCT_REQUIREMENTS.md` §7.3.5 ("beyanın tarihi bildirimin firmaya ulaştığı tarih olur") aynı paragraftaki "pencerenin içinde yapılıp yapılmadığı bu damgadan okunur" ile yan yana başka kanaldan gelen bildirimde süreye uygunluğun ulaşma tarihinden okunduğu gibi anlaşılabilir; cümleye "süreye uygunluk §10.4.10'a göre bildirimin süresinde gönderilmesiyle ölçülür (K-709)" eklenmeli. **Etki yansıtmaya devredildi** (K-1565); `12` satırı bu cümleden bağımsız olarak `02 §10.4.10`'a hizalıdır.
- **UST ve KARAR (F5, VAL-A900 — K-1566 ⚠):** `02_PRODUCT_REQUIREMENTS.md` §5.10 son madde ve §7.5.3 (K-793, K-808): yeniden açılan talepte sonradan ayıbı çıkan başka adedin talebin ayıplı adedine nasıl gireceği ve sözleşmeden dönmenin iade hattının bu adet için nasıl açılacağı yazılmalı; `04_UI_SPECS.md` 5.19.4 ve 9.9.30, `07` UC-46 hizalanmalı. Öneri (toplu onayla kaydedildi, K-1566): müşteri yeniden açmada ayıplı adedi **artırabilir** (azaltamaz): 5.19.4'e adet alanı gelir, alt sınır mevcut ayıplı adet, üst sınır kalemin talebe açık adedi (K-830); artış durum geçmişine girer, B-9/F-3 yeni adedi taşır (K-790); K-676'nın tek kayıt kuralı ve K-297'nin tek geçmişi korunur. Elenen: "aynı kalemde ek adet için ikinci talep" (K-676/K-793'ün elediği bölünmüş geçmiş), "yönetici panelde adedi artırır". **Etki yansıtmaya devredildi:** kaynak (`02`, `04`, `07`) düzeltilince `12` VAL-A900, VAL-A905 (12:2389, teslim almanın üst sınırı talebin ayıplı adedi), VAL-A687 (12:1561, "ayıpta talebe açık adet") ve `06 §4.30` DefectClaim ayıplı adet alanı (değişebilirlik) izler.
- **Etki yansıtmaya (önceki turlardan sürenler):** `10` KP-31 cümlesi ↔ `02 §3.28.1`; K-1545 (1) yük satırları `mock` ↔ K-1518 tanımı kayıt notu; `12 §11.1` "Kanıt ortamı yükselmesi" satırı; K-1563 kaynak değişikliği (`02 §10.4.10`, `02 §6.4.32`, `03 §8.4.8`) ve VAL-A081/A236/A532/B142 (F1'in UST'si ile aynı kaynak cümleleri birlikte ele alınır).
- **Hedefli mercek:** bu tur için gerekmez (§3 son paragraf); 4. turun sonucuna bağlı.
- **`00`, `CLAUDE.md`, `SETUP.md`:** değişiklik önerilmedi.

## 5. Kullanıcı onay checklist'i

Toplu onay modu açık (K-1504); karar **(toplu onayla kaydedildi)**; ⚠ taşıyan karar: K-1566.

- [x] F1 — VAL-A231 Beklenen `02 §10.4.10`'a hizalandı (kısmen) — uygulandı; `02 §7.3.5` notu etki yansıtmada
- [ ] F2, F3 — çürük, uygulama yok
- [x] F4 — VAL-A370 Beklenen ve Kanıt türü (kısmen) — uygulandı
- [ ] F5 — `12`'de uygulama yok; kaynak devri K-1566 ⚠ (etki yansıtmada)
- [ ] F6, F7 — çürük, uygulama yok (F6 2. kez, F7 3. kez tekrar eden RET)
- [x] Sayımlar: satır eklenmedi — matris 1.502 satır, ORTA 733, KRİTİK 637, DÜŞÜK 132, `[güvenlik]` 63; `12_matris_denetle.py --kapsam 1-77`, `12_kapsam_hizasi.py`, `12_atif_denetle.py` hata 0

**Sonuç: TEMİZ DEĞİL — 4. tur** (`12_CROSS_REVIEW_R4.md`, aynı betikler ve parça sınırları, `12` v0.27).
