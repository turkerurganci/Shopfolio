# Cross-Review — 12 Doğrulama Protokolü (Tur 5)

**Tarih:** 2026-11-02 · **Hedef:** `Docs/12_VALIDATION_PROTOCOL.md` v0.28 (girdi commit'i `1991439`, 837 623 bayt, 2 639 satır) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Ciddiyet ölçüsü:** 1. turdan itibaren (K-1399 (1); mekanizma okuma paragrafıyla) · **Sonuç:** TEMİZ DEĞİL — 7 ham bulgu = 6 ayrı bulgu (altı çağrıdan; tek parçadaki `mock` bulgusu ile P1 BULGU-1 aynı iddia): 2 doğru (Yüksek) · 1 kısmen (kaynak devri, K-1568 ⚠) · 1 çürük · 2 doğrulayıcısız tekrar eden iddia; 2'si uygulandı (6 şartname çifti).

## 1. Koşum

Betikler, istem, parça notu ve **parça sınırları değişmedi** (`12_CROSS_REVIEW.md` §1; kayıt K-1561): `C:\Users\teknodeveloper\.claude\helper-state\shopfolio\a10-kd-cross\betik\` — `build_inputs.py` (`python -I build_inputs.py <doküman> template12.md prompt.txt partnote.txt <girdi klasörü>`; kapsanmayan satır: yok), `prompt.txt`, `partnote.txt`, `template12.md`. Koşum: `bash scripts/doc-scans/cross_review_ikinci_model.sh <girdi> <çıktı> c1 p1 p2 p3 p4 p5` (K-1484). İlk deneme Codex kullanım sınırına takıldı (altı çağrı çıkış 1, çıktı 0 bayt; Cursor sağlık çağrısı da başarısız, `model=none`); o çıktı kullanılmadı. Sınır açılınca girdi yeniden üretilip eskisiyle bayt bayt karşılaştırıldı (aynı), komut yeni çıktı dizininde bir kez koşuldu: altı çağrı çıkış 0, hepsi çıktı verdi, yedek modele geçilmedi (`status.txt`: `model=codex gpt-5.6-terra high`). Girdi `12` v0.28: 4. turun uyguladığı SK kimliği yasağı ve üst/alt bilgi dışında önceki turdan farkı yoktur.

| Çağrı | Girdi | Token | Süre | Sonuç |
|---|---|---|---|---|
| tek parça (c1) | 855 106 bayt | 374 155 | 183 sn | 2 bulgu |
| P1 (§1–§4.2, §5–§11) | 135 384 bayt | 79 977 | 100 sn | 1 bulgu |
| P2 (§4.3–§4.12) | 280 351 bayt | 108 650 | 161 sn | TEMİZ |
| P3 (§4.13–§4.20) | 336 641 bayt | 156 148 | 119 sn | 1 bulgu |
| P4 (§12 girişi, §12.1–§12.7) | 201 856 bayt | 94 263 | 141 sn | 1 bulgu |
| P5 (§12.8–§12.13) | 216 001 bayt | 117 807 | 69 sn | 2 bulgu |

**İki koşumun ölçümü.** Tek parça çağrı iki bulgu getirdi (kanıt ortamı `mock` / final kapısı; VAL-A844); parçalı koşum beş ham bulgu getirdi (P1 1, P3 1, P4 1, P5 2; P2 temiz). `mock` bulgusu iki koşumda da çıktı (aynı iddia, iki açıdan); öteki beşi yalnız bir koşumda çıktı. Yasal dayanak taşıyan iddia (F5) doğrulayıcıda resmî metne karşı okundu (okuma tarihi 2026-11-02): Elektronik Ticaret Aracı Hizmet Sağlayıcı ve Elektronik Ticaret Hizmet Sağlayıcılar Hakkında Yönetmelik m.4/1-n ve m.5/1-c (RG 29/12/2022-32058; bulgunun bağladığı 2015 yönetmeliği m.34 ile yürürlükten kalkmıştır), 5174 sayılı Kanun m.9/1, 5362 sayılı Kanun m.6 ve m.68, 6563 sayılı Kanun m.3/2.

## 2. Ham bulgular

İkinci modelin çıktıları, satır sonu boşlukları dışında düzenlenmeden (alıntı blokları içinde).

**(a) Tek parça çağrı:**
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §4.2 madde 8; §6.3; §11.1 madde 4
> Alıntı: “Dış servisin sonucuna dayanan satır — ödeme sağlayıcısı, e-posta altyapısı, Google — `mock` yazar…” / “Düşük kanıt ortamında alınmış PASS'ler üst ortama geçildiğinde **geçersiz sayılır** ve madde `BEKLEMEDE`'ye döner.”
> Sorun: Dış servise bağlı B/C/D satırları zorunlu olarak `mock` ortamına bağlanmış; ortam yükselince de PASS’leri BEKLEMEDE’ye dönüyor. Final çıkış ise tüm KRİTİK satırların PASS olmasını istiyor. Örneğin kart ödeme akışındaki KRİTİK `VAL-B008`, `VAL-C002` ve `VAL-D003` için bu kapı mekanik olarak kapanamaz. Bu, dokümanın kendi final güvencesini sağlayamaması nedeniyle (4) türü çelişkidir.
> Öneri: Mock satırlarının PASS’ini uygulama davranışı kanıtı olarak koruyup yalnız entegrasyon sözleşmesi satırlarını yeniden doğrulamaya bağlayın; veya aynı davranışlar için `sandbox`/`gerçek` ortamlı final yeniden-doğrulama satırları ekleyip final kapısının bu satırları aramasını açıkça tanımlayın.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §7.2; §12.8 `VAL-A844`
> Alıntı: “| **KRİTİK** | **En az 2 farklı türde** kanıt (çok katmanlı — P3) |”
> Sorun: `VAL-A844` KRİTİK olduğu hâlde kanıt türü yalnız `Tarayıcı gözlemi`dir. Bu, KRİTİK satırlar için konmuş iki bağımsız kanıt türü kuralını ihlal eder; yasal onay kutularının başlangıç durumunun tek yüzey gözlemiyle yanlış PASS almasına izin verir. Bu, (4) türü iç çelişkidir.
> Öneri: `VAL-A844`e ikinci, bağımsız bir kanıt türü ekleyin; örneğin yanıt/HTML doğrulaması veya onay kaydının işaretli değer yazmadığını gösteren veri kaydı. Aynı ihlali taşıyan diğer KRİTİK satırları betikle tarayın.
>
> SONUÇ: 2 BULGU

**(b) Parça 1:**
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §4.2 madde 8
> Alıntı: "Dış servisin sonucuna dayanan satır — ödeme sağlayıcısı, e-posta altyapısı, Google — `mock` yazar ve final çıkışta çekirdek entegrasyon kanıtı sayılmaz (§6.3); öteki bütün satırlar `yok`. `sandbox` ve `gerçek` değerleri seviye E ve canlı kanıt satırlarının işidir."
> Sorun: Aynı cümle, dış servis sonucuna dayanan satırı zorunlu olarak `mock` ortamına koyarken seviye E ve canlı kanıt satırları için `sandbox`/`gerçek` ortamını da öngörüyor. Bu, §6.2’de zorunlu F4 sandbox kanıtı ve §6.3’te gereken mock-olmayan çekirdek entegrasyon doğrulamasını mekanik olarak uygulanamaz hâle getirir. Ödeme, e-posta ve Google entegrasyonlarının gerçek/sandbox doğrulaması ya eksik kalır ya da satırın tanımlı ortamına aykırı yürütülür; bu, para ve sipariş akışının yanlış doğrulanmasına yol açabilecek bir iç çelişkidir.
> Öneri: Maddeyi, satırın hedef/son geçerli kanıt ortamını yazacak şekilde düzeltin: geliştirme doğrulaması `mock`, F4 kanıtı `sandbox`, F6/kabul kanıtı gereken satırlar `gerçek` olabilir. Ortam yükseltmesinde yalnız durumun değil Kanıt ortamı hücresinin de hangi yetkiyle güncelleneceğini §7.5 ve §11 ile açıkça bağlayın.
>
> SONUÇ: 1 BULGU

**(c) Parça 2:**
> SONUÇ: TEMİZ

**(d) Parça 3:**
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §4.16, VAL-E005 ve VAL-E067
> Alıntı: "`frame-src 'self'` ve 3D Secure sayfası engellenmez" / "ödeme sayfasının politikasında çerçeve izni `frame-src 'self' https://www.paytr.com`'dur"
> Sorun: Aynı ödeme sayfası için iki çelişen CSP kuralı vardır. Yalnız `frame-src 'self'`, PayTR’nin farklı origin’deki ödeme çerçevesini yüklemeyi engeller; VAL-E067 ise PayTR origin’ini açıkça zorunlu tutar. Bu, ödeme formunun/3DS akışının açılmamasına ve müşterinin ödeme adımında takılmasına yol açabilir (doküman içi çelişki ve çıkışsız akış).
> Öneri: VAL-E005’i VAL-E067 ile hizalayın: ödeme sayfası için `frame-src 'self' https://www.paytr.com` beklenmelidir. 3DS’nin gerçek banka sayfasına geçişi ayrıca VAL-E005/E006’daki canlı gözlemle doğrulansın.
>
> SONUÇ: 1 BULGU

**(e) Parça 4:**
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §12.6, `VAL-A826` (`VAL-C110` ve `VAL-A827` ile çelişiyor)
> Alıntı: "2 dakika 1 saniyede süresi dolan havale siparişi iptal edilmez; başka iş işlenir; nabız yazılabildiği ilk anda havale iptali yapılır"
> Sorun: Kesinti eşiğini aşan bu durumda ilk yeni nabız, kesintiyi saptar. `VAL-C110`, kesinti aralığında dolan havale siparişinin son anının dönüşten sonraki ilk iş gününün sonuna taşınmasını; `VAL-A827` de yalnız kesintinin ilk iki dakikasındaki son anın ertelenmemesini söyler. Buna karşılık `VAL-A826`, iki dakikayı aşan örnekte siparişi ilk nabızda iptal ettirir. Bu, aynı olaya iki farklı sonuç bağlayan bir iç çelişkidir; müşterinin siparişi ve ayrılmış stok yanlışlıkla iptal edilebilir.
> Öneri: `VAL-A826` beklenen sonucunu, 2 dakika 1 saniye örneğinde ilk nabızla kesinti kaydının açılması ve siparişin son anının `VAL-C110` uyarınca dönüşten sonraki ilk iş gününün sonuna ertelenmesi olarak değiştirin. İlk iki dakikaya düşen son anın ertelenmemesi istisnasını ayrı tutun.
>
> SONUÇ: 1 BULGU

**(f) Parça 5:**
> BULGU-1
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: §12.8, VAL-A854 ve VAL-A856
> Alıntı: "Firma tipi sonradan değiştirilince blok yeni tipin setini gösterir"
> Sorun: “Zorunlu set” doğrulaması meslek odası bilgisini kapsamıyor. Oysa elektronik ticaret hizmet sağlayıcısı, mensubu olduğu meslek odası ile meslek kurallarına erişim bilgisini sunmakla yükümlüdür. Bu eksiklik, yasal kimlik bloğunun PASS almasına rağmen mevzuata aykırı eksik kalmasına yol açar (1). [Yönetmelik m.5](https://www.mevzuat.gov.tr/Metin.Aspx?MevzuatIliski=0&MevzuatKod=7.5.21093)
> Öneri: VAL-A854’ün zorunlu sete “mensubu olunan meslek odası”nı ekleyin; VAL-A856’da hem oda bilgisinin hem de varsa meslek kurallarına erişimin tüm firma tiplerinde görünmesini sınayın.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §12.9, VAL-A900
> Alıntı: "İkinci talep açılamaz; talep yeniden açılır ve ayıplı adet, açılırken seçilen adet olarak kalır"
> Sorun: Başka bir adedin sonradan ayıplı çıkması için yeni talep yasaklanıyor; ancak yeniden açılan talebin ayıplı adedi de ilk seçilen adet olarak sabitleniyor. Böylece sonradan ayıplı olduğu ortaya çıkan ayrı adet kayda, iade hattına veya seçilen çözüme konu olamaz. Bu, tüketicinin ayıplı mal için seçimlik hakkını fiilen kullanamamasına yol açar (1, 2). [6502 sayılı Kanun m.11](https://tuketici.ticaret.gov.tr/data/5e81982d13b876a1b04c7a42/2023-6502%20Say%C4%B1l%C4%B1%20T%C3%BCketicinin%20Korunmas%C4%B1%20Hakkında%20Kanun.pdf)
> Öneri: Yeniden açmada sonradan ayıplı olduğu bildirilen adedin talebin kapsamına eklenmesini ve her adet/çözümün izlenmesini sağlayın; alternatif olarak aynı kalem için yeni talebe izin verin.
>
> SONUÇ: 2 BULGU

## 3. Bağımsız değerlendirme

Yedi ham bulgu altı ayrı bulgudur (c1 BULGU-1 ve P1 BULGU-1 birleşti: F1). Dört bulgu için dört Opus/high doğrulayıcı (D1 F2; D2 F4; D3 F5; D4 F1; **adım sınırı 4/20**). **F3** (CSP `frame-src`) ve **F6** (VAL-A900) doğrulayıcıya gitmeden kaydedildi (hüküm dayanakları tabloda). Hüküm dosyaları: `C:\\Users\\teknodeveloper\\.claude\\helper-state\\shopfolio\\a10-kd-cross\\r5\\hukum_D1.md`…`hukum_D4.md` (`12_audit_hukum_denetle.py`: hüküm doğru 2 · kısmen 1 · çürük 1, çift 6, uygulanabilir çiftlerde sorun 0).

| # | Parça · Yer | Konu | Hüküm | Seviye (model → doğrulayıcı) | Gerekçe ve şartname |
|---|---|---|---|---|---|
| F1 | tek parça + P1 · §4.2 madde 8; §6.3; §7.4; §11; §11.1 madde 4 | `mock` yazan dış servis satırları final kapısında hiç PASS alamıyor: "tüm KRİTİK PASS" ↔ "`mock` ile PASS sayılmaz" ve "düşük ortam PASS'i geçersiz" | **doğru** | Yüksek → Yüksek | **Yeni kanıt** (4. tur F3 yalnız §4.2 madde 8 ↔ F4 `sandbox` çelişkisini sınadı ve çürüttü; kapının kilitlenmesini görmedi): 12:1734 "Tüm KRİTİK maddeler PASS — hiçbiri BEKLEMEDE olamaz" ↔ 12:1739 "Seviye E satırları … `mock` ile PASS olarak sayılmaz — BEKLEMEDE kalır" §4.16 (3)'ün `mock` yazan 20 KRİTİK E satırını finalde PASS alamaz kılar; 12:1914 ve 12:1926 (§11.1 madde 4) "düşük ortamla alınmış PASS'ler geçersiz" diyerek 12:374 ve K-1521 (6) gereği `mock`ta sabit kalan 22 KRİTİK B/C/D satırını (VAL-B008, VAL-C002, VAL-D003 dahil) F4'ten sonra kalıcı BEKLEMEDE'ye düşürür. Kaynak daha dardır: K-1517 (5) yalnız "çekirdek E satırları"nı, K-1531 (5) `mock` satırları "ürünün bağlantı katmanı sınıflaması" diye yazar. **Uygulandı (5 çift):** §6.3 koşulu Kanıt ortamı `sandbox`/`gerçek` olan E satırlarını bağlar, `mock` satırın PASS'i ürünün sahte servise tepkisinin kanıtıdır ve geçerli kalır; §7.4 "ortam değerinin sonuçları"; §11 yükselme geçersizleşmesi yalnız hücresi yükselinen ortamı yazan satırlara; §11.1 madde 4 aynı şekilde; "ortam değişince" özeti. Önceki turlardan devreden "§11.1 Kanıt ortamı yükselmesi" notu bu çiftlerle kapandı; `skills/gate-check` Durum anlatımı hizalandı. |
| F2 | tek parça · §7.2; §12.8 VAL-A844 | KRİTİK satır tek kanıt türü taşıyor | **çürük** | Yüksek → Düşük | Satırın kanıt türü hücresi "Tarayıcı gözlemi + Uç yanıtı"dır (12:2287); "Uç yanıtı" §7.1'de ayrı kanıt türüdür ve beklenen sonucun "sunucu önceden işaretli bir değer yazmaz" kısmını gösterir; §7.2 koşulu sağlanır. Betikle tarama: 637 KRİTİK VAL satırının 637'si en az iki tür taşır. İkinci model hücreyi yanlış okumuştur. |
| F3 | P3 · §4.16 VAL-E005, VAL-E067 | `frame-src 'self'` ↔ `frame-src 'self' https://www.paytr.com` | **tekrar eden iddia (doğrulayıcısız)** | Yüksek → Düşük | **Önceki hüküm:** 1. tur F6 (K-1561; VAL-E067 Beklenen düzeltildi) ve 2. tur F4 (çürük): VAL-E005 `frame-src 'self'`'i "yalnız self" diye istemez, DV-5 geçiş sayfasının o izinle engellenmemesini ister (VAL-E005 DV-5'in kısaltmasıdır); gerçek çelişki E067'deydi ve kapandı (12:1204). Aynı yer, aynı iddia, yeni kanıt yok. Turun sonucunu belirlemez. |
| F4 | P4 · §12.6 VAL-A826 ↔ VAL-C110, VAL-A827 | 2 dakika 1 saniyede süresi dolan havale siparişi ilk nabızda iptal ediliyor; C110 son anı dönüşten sonraki ilk iş gününe taşır | **doğru** | Yüksek → Yüksek | 12:2199 "nabız yazılabildiği ilk anda havale iptali yapılır" ↔ 12:2198 VAL-C110 "son anı dönüşten sonraki ilk iş gününün 23:59'una taşınır"; `05 §5.4.3` ("ödeme süresi kesinti aralığında dolmuş her havale siparişinin son anı … taşınır"), `02` 6.7.17 ("Sipariş dönüşte hemen iptal edilmez"), `02 §4.3` Z-8; `05 §5.4.6` yalnız ilk iki dakikadaki son anı ertelemez (A827 ile uyumlu). Müşterinin siparişi ve ayrılmış stok yanlış iptal olur (2). Önceki turlar (audit-2c, deep #80) yalnız eşiğin iki yanını ve ölçüm anını ele aldı; ikinci beklenen cümle onlardan aynen kalmıştı. **Uygulandı (1 çift):** nabız yazılabildiği ilk anda kesinti kaydı açılır ve 2 dakika 1 saniyedeki sipariş dönüşte iptal edilmez, son anı ilk iş gününün 23:59'una taşınır; 1 dakika 59 saniyedeki son an kesintinin ilk iki dakikasında kaldığı için ertelenmez (VAL-C110, VAL-A827). |
| F5 | P5 · §12.8 VAL-A854, VAL-A856 | Zorunlu kimlik setinde meslek odası yok | **kısmen** | Yüksek → Yüksek | `12` kaynağı birebir izler: `02 §3.1.3` meslek odasını isteğe bağlı yapar (K-547; Kanun m.3/2 "varsa"); VAL-A126 ve A286 bunu sınar. `12`'de değişiklik yapılmaz (K-652). **Yeni kanıt:** yürürlükteki yönetmelik m.5/1-c meslek odasını "eksiksiz olarak" ister ve "varsa" demez; 5174 m.9/1 (tacirler odaya kaydolmak zorunda) ve 5362 m.6, m.68 (esnaf sicili oda kaydının şartı) gereği üç firma tipinin her birinin bir odası vardır, "varsa" bu tiplerde boşa düşer; K-627'nin KEP için reddettiği durumun aynısıdır. Bulgunun bağladığı 2015 yönetmeliği yürürlükten kalkmıştır. **KARAR (K-1568 ⚠, öneriyle):** meslek odası ve davranış kurallarına bağlantı ya da kısa metin üç tipte de zorunlu; kaynak değişikliği ve ardından 7 `12` satırı etki yansıtmada. |
| F6 | P5 · §12.9 VAL-A900 | Yeniden açılan talepte ayıplı adet artırılamıyor; yeni talep de açılamıyor | **tekrar eden iddia (doğrulayıcısız)** | Yüksek → Orta | 3. tur F5 ve 4. tur F7: çelişki kaynaktadır (K-793 ↔ K-808), `12` kaynağı birebir izler (K-652); karar K-1566 ⚠ (yeniden açmada ayıplı adet artırılabilir) kaynağa ve etki yansıtmaya devredildi. Aynı yer, aynı iddia, yeni kanıt yok. Turun sonucunu belirlemez. |

**Hüküm dağılımı:** 2 doğru (F1, F4) · 1 kısmen (F5) · 1 çürük (F2) · 2 doğrulayıcısız tekrar eden iddia (F3, F6); seviyesi yükselen yok.

**Sonuç sınıfı ve sıradaki adım.** Kabul edilen iki bulgu Yüksektir: F1 dokümanın kendi koyduğu final güvencesini sağlamayan mekanizma (4), F4 iki satırın çelişkisi (4; sipariş ve stok akışı, 2). Kısmen kabul F5 kaynak devridir. 5. turdan itibaren çıkış kuralı (skill Faz 4 madde 5) kabullerin hepsinin Düşük metin farkı olmasını ister; bu sağlanmadı: **döngü sürer, 6. tur gerekir** (aynı betikler ve parça sınırları, `12` v0.29). Hedefli mercek (Faz 4 madde 6) gerekmez: art arda iki turda aynı sınıfın Orta kabulü şartı sağlanmadı (4. turda kabul yalnız Düşük). F1 ile ilgili aynı kalıp (kanıt ortamı değerine bağlı geçersizleşme/PASS sayımı) tarandı: `12 §6.1`, `§6.2`, `§7.5`, `§8.2`, `skills/validate` metinlerinde `mock` PASS'ini geçersizleştiren başka cümle yok; `skills/gate-check` Durum anlatımı hizalandı.

## 4. Ek bulgular ve devirler

- **Etki yansıtmaya (F5, K-1568 ⚠):** meslek odası üç tipte zorunlu kimlik alanı; kaynak: `02 §3.1.3` (meslek odası cümlesi) ve `§3.1.5` (1. koşul), `10` KP-35/KP-58, `04 §7.2.19.6` ve `§9.16`, `06` `ChamberName` (hayır → zorunlu), `11` (2442. satır, madde 5); sonra `12` VAL-A854 (zorunlu sete eklenir), A855, A285 deseniyle yeni satır, A286 ("isteğe bağlı" listesinden çıkar), A126 ("doluysa" koşulu kalkar), A125, A856 (davranış kuralları da zorunlu olursa).
- **Etki yansıtmaya (F1):** K-1517 (5) kayıt notu — "çekirdek E satırları" Kanıt ortamı `sandbox`/`gerçek` olan E satırlarıdır; K-1531 (5)'in `mock` bağlantı katmanı satırları ve K-1521 (6)'nın `mock` B/C/D satırları bu koşulun kapsamı dışındadır (karar değişmez). "`12 §11.1` Kanıt ortamı yükselmesi" devri F1 çiftleriyle kapandı.
- **Etki yansıtmaya (önceki turlardan sürenler):** `12` P7'nin `12_kapsam_hizasi.py` için verdiği "kriter satırı taşıyan KD, SK ya da YH olmadığını gösterir" sözü ↔ betik (4. tur F4); `02 §7.3.5` "süreye uygunluk gönderimle ölçülür" notu (K-1565); `10` KP-31 cümlesi ↔ `02 §3.28.1`; K-1545 (1) yük satırları `mock` ↔ K-1518 tanımı kayıt notu; `skills/validate` PASS tanımı ↔ `12 §6.1` istisnası; K-1563 kaynak düzeltmesi ve VAL-A081/A236/A532/B142; K-1566 kaynak değişikliği (`02 §5.10`, `02 §7.5.3`, `04 §5.19.4`, `04 §9.9.30`, `07` UC-46) ve ardından VAL-A900, VAL-A905, VAL-A687, `06 §4.30`.
- **Hedefli mercek:** bu tur için gerekmez (§3 son paragraf).
- **`00`, `CLAUDE.md`, `SETUP.md`:** değişiklik önerilmedi.

## 5. Kullanıcı onay checklist'i

Toplu onay modu açık (K-1504); karar **(toplu onayla kaydedildi)**; ⚠ taşıyan yeni karar: K-1568 (meslek odası üç tipte zorunlu).

- [x] F1 — `mock` satırların final kapısı (doğru) — 5 çift uygulandı; `skills/gate-check` hizalandı
- [ ] F2 — çürük, uygulama yok
- [ ] F3, F6 — tekrar eden iddia / K-1566 devri, uygulama yok
- [x] F4 — VAL-A826 beklenen sonuç (doğru) — 1 çift uygulandı
- [ ] F5 — kısmen; `12`'de değişiklik yok, kaynak devri (K-1568 ⚠)
- [x] Sayımlar: satır eklenmedi — matris 1.502 satır, ORTA 733, KRİTİK 637, DÜŞÜK 132, `[güvenlik]` 63; `12_matris_denetle.py --kapsam 1-77`, `12_kapsam_hizasi.py`, `12_atif_denetle.py` hata 0

**Sonuç: TEMİZ DEĞİL — 6. tur** (`12_CROSS_REVIEW_R6.md`, aynı betikler ve parça sınırları, `12` v0.29; Yüksek kabul olduğu için çıkış kuralı işlemedi).
