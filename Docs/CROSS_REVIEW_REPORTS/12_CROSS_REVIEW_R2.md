# Cross-Review — 12 Doğrulama Protokolü (Tur 2)

**Tarih:** 2026-11-01 · **Hedef:** `Docs/12_VALIDATION_PROTOCOL.md` v0.24 (girdi commit'i `9e90110`, 833 157 bayt, 2 639 satır) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Ciddiyet ölçüsü:** 1. turdan itibaren (K-1399 (1); mekanizma okuma paragrafıyla) · **Sonuç:** TEMİZ DEĞİL — 5 ayrı bulgu (7 ham, altı çağrıdan): 2 doğru · 1 kısmen · 2 çürük; 2'si uygulandı (3 şartname çifti).

## 1. Koşum

Betikler, istem, parça notu ve **parça sınırları değişmedi** (`12_CROSS_REVIEW.md` §1; kayıt K-1561): `C:\Users\teknodeveloper\.claude\helper-state\shopfolio\a10-kd-cross\betik\` — `build_inputs.py` (`python -I build_inputs.py <doküman> template12.md prompt.txt partnote.txt <girdi klasörü>`; kapsanmayan satır: yok), `prompt.txt`, `partnote.txt`, `template12.md`. Koşum: `bash scripts/doc-scans/cross_review_ikinci_model.sh <girdi> <çıktı> c1 p1 p2 p3 p4 p5` (K-1484). Altı çağrı paralel koştu, hepsi çıktı verdi; kullanım sınırı dolmadı, yedek modele geçilmedi (`status.txt`: `model=codex gpt-5.6-terra high`).

| Çağrı | Girdi | Token | Süre | Sonuç |
|---|---|---|---|---|
| tek parça (c1) | 850 205 bayt | 295 433 | 106 sn | 1 bulgu |
| P1 (§1–§4.2, §5–§11) | 130 986 bayt | 79 381 | 185 sn | 1 bulgu |
| P2 (§4.3–§4.12) | 278 931 bayt | 157 576 | 92 sn | 1 bulgu |
| P3 (§4.13–§4.20) | 335 724 bayt | 135 348 | 70 sn | 1 bulgu |
| P4 (§12 girişi, §12.1–§12.7) | 200 939 bayt | 146 579 | 113 sn | 1 bulgu |
| P5 (§12.8–§12.13) | 215 084 bayt | 102 652 | 118 sn | 2 bulgu |

**İki koşumun ölçümü.** Bu tur tek parça çağrı bir bulgu getirdi (alt bilgi sürümü; 1. turda `TEMİZ` dönmüştü); parçalı koşum altı ham bulgu getirdi (alt bilgi sürümü P5'te de çıktı). Yasal dayanak taşıyan üç iddia (VAL-A081; VAL-A907 ve K-808 park girdisi) doğrulayıcılarda resmî metne karşı okundu (okuma tarihi 2026-11-01): Mesafeli Sözleşmeler Yönetmeliği m.10/1, m.11/1 (Resmî Gazete 27.11.2014/29188, resmigazete.gov.tr) ve 6502 sayılı Kanun m.11/5, m.15/3 (mevzuat.gov.tr).

## 2. Ham bulgular

İkinci modelin çıktıları, satır sonu boşlukları dışında düzenlenmeden (alıntı blokları içinde).

**(a) Tek parça çağrı:**
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: Başlık / belge sonu
> Alıntı: `"**Versiyon: v0.24**"` / `"*Shopfolio — Validation Protocol v0.23*"`
> Sorun: Dokümanın sürümü iki farklı değerde. Bu, task ve faz kapanışlarında hangi protokol revizyonunun doğrulandığını belirsizleştirir; dokümanın kendi iç çelişkisidir (4).
> Öneri: Alt bilgiyi v0.24 olarak düzeltin veya sürümü tek bir kaynaktan üretin.
>
> SONUÇ: 1 BULGU

**(b) Parça 1:**
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §4, §6.1, §8.4; Okuma tablosu
> Alıntı: “Matrisin `Durum` hücresi `KABUL EDİLMİŞ RİSK` olur ve backlog kimliğini anar.”
> Sorun: `Durum` değer kümesi yalnız `KABUL EDİLMİŞ RİSK` değerini tanır; buna karşılık §6.1 onayı bu hücredeki backlog kimliğinden okumayı zorunlu kılar. Ayrıca okuma tablosundaki sorgu yalnız tam olarak `KABUL EDİLMİŞ RİSK` ile biten hücreleri bulur. Kimlik hücreye eklenirse sorgu ve değer kümesi bozulur; eklenmezse gerekli risk kaydı mekanik olarak izlenemez. Bu, kabul edilmiş risklerin kapılarda doğrulanması güvencesini bozan iç çelişkidir.
> Öneri: Matriste ayrı, makinece okunabilir bir `Risk kaydı` sütunu tanımlayın; `KABUL EDİLMİŞ RİSK` durumunda zorunlu kılın ve betik/okuma komutlarını bu sütunu denetleyecek şekilde güncelleyin.
>
> SONUÇ: 1 BULGU

**(c) Parça 2:**
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §4.6, VAL-A081
> Alıntı: "kayıt isteği ve kaydı gönderim tarihini ya da bilgilendirmenin eksikliğini taşıyan bir alan içermez ... kabul ya da ret yalnız girilen tarihin pencereye ve bir yıl sınırına göre yerine bağlıdır"
> Sorun: Cayma bildiriminin süresi, firmaya ulaşma tarihine değil, süre dolmadan satıcıya yöneltilmesine göre değerlendirilmelidir. Bu satır gönderim tarihini kaydetmeden kabul/ret kararını yalnız kayıt tarihine bağlayarak süresinde gönderilip geç ulaşan caymaları reddedebilir; tüketici hakkı kaybı ve mevzuata aykırılık doğurur. [Mesafeli Sözleşmeler Yönetmeliği bilgilendirmesi](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/mesafeli-sozlesmeler-hakkinda-bilgilendirme)
> Öneri: Panel kaydına bildirimin gönderim tarihi ve bunu destekleyen kanıt bilgisini ekleyin; caymanın geçerliliğini bu tarihe göre, geri ödeme süresini ise bildirimin firmaya ulaştığı tarihe göre değerlendirin.
>
> SONUÇ: 1 BULGU

**(d) Parça 3:**
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §4.16 VAL-E005, VAL-E067; §4.20 VAL-A980
> Alıntı: "`frame-src 'self'` ve 3D Secure sayfası engellenmez" / "Ödeme sayfasının politikasında çerçeve izni `frame-src 'self' https://www.paytr.com`'dur" / "Her yanıtta içerik güvenlik politikası (`default-src 'self'` …) vardır"
> Sorun: Aynı ödeme sayfası için çelişen CSP kuralları konmuş. `frame-src 'self'` PayTR çerçevesini engeller. Ayrıca VAL-A980’in her yanıt için zorunlu tuttuğu `default-src 'self'`, `frame-src` tanımlanmadığında çerçevelere de uygulanarak PayTR’yi yine engeller. Bu durumda kartla ödeme ve 3D Secure akışı çalışamaz; müşterinin satın alma akışı kesilir ve firmaya doğrudan satış kaybı doğar.
> Öneri: VAL-E005’i `frame-src 'self' https://www.paytr.com` olarak düzeltin. VAL-A980’de ödeme sayfasını açık istisna yapın veya bu sayfada `frame-src 'self' https://www.paytr.com` zorunlu kılın; diğer sayfalarda `frame-src 'none'` kullanın.
>
> SONUÇ: 1 BULGU

**(e) Parça 4:**
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: Aşama 3 park girdileri · P3.6 / K-808
> Alıntı: "geri ödemenin süre sayacı yoktur"
> Sorun: Ayıplı malda sözleşmeden dönme/bedel indirimi seçildiğinde bedelin tüketiciye derhal iadesi gerekir. Süre takibi olmaması, iadenin belirsiz süre bekletilmesine ve tüketicinin parasal hakkının ihlaline yol açar; mevzuata aykırıdır. [Ticaret Bakanlığı rehberi](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/ayipli-mal-ve-hizmetler-hakkinda-bilgilendirme)
> Öneri: Bu hükmü, ayıpta dönme veya bedel indirimi kabul edildiğinde bedelin derhal iadesini zorunlu kılacak şekilde değiştirin; fiziksel kalemde tüketicinin malı geri vermeye hazır olduğunu bildirmesiyle başlayan iade akışını ve gecikmesiz iade kanıtını matris satırında doğrulayın.
>
> SONUÇ: 1 BULGU

**(f) Parça 5:**
> BULGU-1
> Kriter: teknik doğruluk
> Seviye: Yüksek
> Yer: §12.9, VAL-A907
> Alıntı: "Ayıpta sözleşmeden dönmenin geri ödemesinin süre sayacı yoktur"
> Sorun: Ayıplı mal ve ayıplı hizmette sözleşmeden dönme seçildiğinde bedelin derhâl iadesi zorunludur. “Süre sayacı yoktur” ve “son tarih hesaplanmaz” sonucu, iadenin süresiz beklemesine izin verir; bu mevzuata aykırıdır ve müşterinin para kaybına yol açar. [TKHK m.11/5 ve m.15/3](https://tuketici.ticaret.gov.tr/data/5e81982d13b876a1b04c7a42/Tuketicinin_Korunmasi_Hakkinda_Kanun_6502_Ocak_2021.pdf)
> Öneri: `02 §12.5` ile birlikte kuralı, ayıpta dönme/bedel indirimi seçildiğinde iadenin derhâl yapılmasını zorunlu ve sınanabilir kılacak şekilde düzeltin; VAL-A907’de “süre sayacı yoktur” yerine bu yükümlülüğün kanıtını arayın.
>
> BULGU-2
> Kriter: tutarlılık
> Seviye: Orta
> Yer: Başlık notu / belge sonu
> Alıntı: "**Versiyon: v0.24**" / "*Shopfolio — Validation Protocol v0.23*"
> Sorun: Aynı doküman iki farklı sürüm kimliği taşıyor. Faz ve task kapanışlarında hangi doğrulama kurallarının yürürlükte olduğunun mekanik olarak belirlenmesini engelleyen doğrudan iç çelişkidir.
> Öneri: Altbilgiyi v0.24 olarak güncelleyin veya sürüm bilgisini yalnız başlık notunda tutun.
>
> SONUÇ: 2 BULGU

## 3. Bağımsız değerlendirme

Yedi ham bulgu beş ayrı bulguya birleşti (F1: tek parça ve P5 aynı sürüm çelişkisi; F5: P4 ve P5 aynı "süre sayacı" iddiası); beş Opus/high doğrulayıcı (D1 F1; D2 F2; D3 F3; D4 F4; D5 F5; adım sınırı 5/20). Hüküm dosyaları: `C:\Users\teknodeveloper\.claude\helper-state\shopfolio\a10-kd-cross\r2\hukum_D1.md`…`hukum_D5.md` (`12_audit_hukum_denetle.py`: hüküm doğru 3 (D1'in iki kopyası dahil) · kısmen 1 · çürük 3, çift 3, sorun 0). F1 ve F5 doğrulayıcıya iki ham madde olarak gitti; ayrı bulgu sayımı beşdir.

| # | Parça · Yer | Konu | Hüküm | Seviye (model → doğrulayıcı) | Gerekçe ve şartname |
|---|---|---|---|---|---|
| F1 | tek parça, P5 · başlık notu ↔ belge sonu | Üst bilgi `v0.24`, alt bilgi `v0.23` | **doğru** | Orta → Düşük | 12:3 `Versiyon: v0.24` ↔ 12:2639 alt bilgi `v0.23`; önceki sürümlerde alt bilgi başlıkla birlikte güncelleniyordu (v0.22 → v0.23), v0.24'te (PR #322) atlanmış. İki yerin çelişmesi (4) ama hiçbir mekanizmayı etkilemez, otoriter sürüm başlıktadır. **Uygulandı (1 çift):** alt bilgi `v0.24` (bu turda `v0.25`). |
| F2 | P1 · §4, §6.1, §8.4; okuma tablosu | `KABUL EDİLMİŞ RİSK` durumunda backlog kimliği `Durum` hücresine yazılır; hücrenin değer kümesi ve okuma sorgusu yalnız değeri tanır | **doğru** | Orta → Orta | 12:1860 (§8.4) "`Durum` hücresi `KABUL EDİLMİŞ RİSK` olur ve backlog kimliğini anar" ve 12:1700 (§6.1) "doğrulayıcı onayı `Durum` hücresindeki backlog kimliğinden okur" ↔ 12:344 değer kümesi tek değer; 12:142 okuma sorgusu hücrenin tam değerle bitmesini ister; `12_matris_denetle.py:107` başka değeri "geçersiz" sayar. Kimlik yazılırsa betik ve sorgu kırılır, yazılmazsa onay okuması dayanaksız kalır — durum hücresinin yaşam döngüsü güvencesinin iç çelişkisi (4). 12:1700'deki ifade K-1561 F1'in uygulamasıyla eklenmişti; çelişki yenidir. Modelin `Risk kaydı` sütunu önerisi **reddedildi**: her bölümün başlığı, yüzlerce satır ve iki betik değişirdi; bağ zaten zorunlu olan ters yöndedir — backlog açıklaması "KABUL EDİLMİŞ RİSK — `VAL-…`" diye madde kimliğiyle başlar (12:1860). **Uygulandı (2 çift):** §8.4: hücre yalnız `KABUL EDİLMİŞ RİSK` değerini taşır, backlog kimliği hücreye yazılmaz; bağ `grep -n 'VAL-C012' DEFERRED_BACKLOG.md` ile okunur; kaydı olmayan hücre `FAIL` sayılır. §6.1 "İlgili maddeler PASS" satırı aynı bağla hizalandı (doğrulayıcı onayı satırın kimliğiyle bulunan backlog kaydından okur). |
| F3 | P2 · §4.6 VAL-A081 | Cayma bildirimi süresinde gönderilip geç ulaşırsa reddedilir; gönderim tarihi tutulmuyor — Mesafeli Sözleşmeler Yönetmeliği | **kısmen** | Yüksek → Düşük | Ana iddia çürük: 12:539 pencere bitiminden sonraki bir yıl içine tarihli kaydı uyarı ve ayrı onayla KABUL eder, uyarı "süresinde gönderilmiş, geç ulaşmış" hâlini de sayar; gönderim tarihini tutmama ve kaydı ulaşma tarihiyle yapma bilinçli karardır (`02 §10.4.10`: "ürün denetlemez ve gönderim tarihini ayrıca tutmaz"; K-709: "Kayıt iki tarih taşısın" elendi); geri ödeme süresi zaten ulaşma tarihinden işler (VAL-A080). Bu kısım audit'te hükme bağlıydı (audit #95; deep-2a #23). **Yeni dar kanıt (kaynak boşluğu):** üründe tek ret yolu "pencere bitiminden bir yıldan sonraya tarihli kayıt" ve bu tarih **ulaşma** tarihidir. Yönetmelik m.10/1'e göre uzamış süre pencere bitiminden bir yıl sonra sona erer, m.11/1'e göre ise bildirimin süre dolmadan yöneltilmesi yeterlidir; bir yıllık sürenin son günlerinde postalanıp süre bittikten sonra ulaşan bildirim panelde reddedilir ve onay yolu yoktur (çıkışsız akış + mevzuata aykırılık). `12` kaynağı birebir izliyor; hata kaynaktadır (K-606'nın bir yıl reddi ↔ K-709'un "süreye uygunluk gönderimle ölçülür" ilkesi). **`12`'de değişiklik yok** (K-652); `UST:` ve `KARAR` aşağıda (§4). |
| F4 | P3 · §4.16 VAL-E005, VAL-E067; §4.20 VAL-A980 | `frame-src 'self'` PayTR çerçevesini engeller; VAL-A980'in `default-src 'self'` zorunluluğu da engeller | **çürük** | Yüksek → Düşük | **Önceki hüküm** (1. tur F6, K-1561): VAL-E005 `frame-src 'self'`'i "yalnız self" diye istemez, DV-5 geçiş sayfasının o izinle engellenmemesini ister (VAL-E005 DV-5'in kısaltmasıdır, 08:849); gerçek çelişki VAL-E067'nin Beklenen hücresindeydi ve düzeltildi (12:1204: ödeme sayfasında `frame-src 'self' https://www.paytr.com`, öteki sayfalarda `'none'`). VAL-A980'in ön koşulunda ödeme sayfası yok; `frame-src` her sayfada açık yazıldığı için `default-src` çerçeveye uygulanmaz (`09 §15.2`, 09:1615). Yeni kanıt yok. |
| F5 | P4, P5 · K-808 park girdisi; VAL-A907 | Ayıpta sözleşmeden dönmede "geri ödemenin süre sayacı yoktur" — iade süresiz beklemeye izin verir; TKHK m.11/5, m.15/3 | **çürük** | Yüksek → Düşük | **Önceki hüküm** (1. tur F7, K-1561; `r1/hukum_D6.md` #7): "süre sayacının olmaması" K-716'nın bilinçli kararıdır ("derhal" bir gün sayısı değildir; yükümlülük `02 §12.5`'te firmaya adlandırılır). Resmî metin (mevzuat.gov.tr, 6502 m.11/5 ve m.15/3): iade "derhâl" yapılır, gün sayısı verilmez; ürün geri ödemeyi engellemez, yükümlülüğü "geri ödeme gerçekleşmedi" işareti, bekleyen işler sayacı ve panel uyarısı ile açar. Yeni kanıt yok. **Tekrar eden RET:** bu iddia 2. kez gerekçeyle reddedildi (1. tur F7; bu tur; skill Faz 4 madde 5). |

**Hüküm dağılımı:** 2 doğru (F1, F2) · 1 kısmen (F3) · 2 çürük (F4, F5); seviyesi yükselen yok. Seviye: F3, F4, F5 Yüksek → Düşük; F1 Orta → Düşük; F2 Orta korundu.

**Sonuç sınıfı ve sıradaki adım.** Kabul edilen ikisinden biri (F1) Düşük metin farkı, biri (F2) Orta; F2 durum hücresinin yaşam döngüsü güvencesinin iç çelişkisidir (4). **Art arda iki turda kabul edilen Orta bulgular aynı sınıftadır:** 1. tur F1 (task çıkışı kuralı ↔ kabul edilmiş risk yolu) ve bu tur F2 (kabul edilmiş riskin `Durum` hücresi ↔ değer kümesi, okuma sorgusu ve betik) — kabul edilmiş risk yolunun güvencesi. Skill Faz 4 madde 6 gereği **3. tur koşmadan önce sınıf `12`'nin tamamında hedefli mercekle taranır** (`audit` skill'i, güvence merceği; kapsam: `KABUL EDİLMİŞ RİSK` geçen her satır, §4.1 değer kümesi, §6.1–§6.3 kapı satırları, §8.4–§8.5, okuma tablosu sorguları, `12_matris_denetle.py` ve `12_kapsam_hizasi.py` değer kümeleri, `skills/validate` ve `skills/gate-check` Durum anlatımı). 2. tur tekrar eden RET sayısı: F5 (1. tur F7 ile aynı iddia).

## 4. Ek bulgular ve devirler

- **UST ve KARAR (F3, VAL-A081 — K-1563 ⚠):** `02 §10.4.10` ("Pencerenin bitiminden bir yıldan sonraya tarihli kaydı panel sebebini söyleyerek reddeder"), aynı kuralı taşıyan `02 §6.4.32` ve `03 §8.4.8`: bir yıl sınırı ulaşma tarihine uygulanınca bir yıl içinde gönderilip sonra ulaşan bildirim çıkışsız reddedilir. Öneri (toplu onayla kaydedildi, K-1563): bir yıldan sonraya tarihli kayıt reddedilmez; pencere dışı kayıttaki gibi panel uyarır ve ayrıca onaylatır; uyarı "bildirim bir yıllık süre dolmadan gönderilmiş olmalı; gönderimin ispatı tüketicidedir (m.11/4)" der; gönderim tarihi yine tutulmaz (K-709 korunur). **Etki yansıtmaya devredildi:** kaynak (`02`, `03`) düzeltilince `12` VAL-A081 (kural), VAL-A236 (12:774, bir yıldan sonrası ret), VAL-A532 (12:1399, `DeclarationOutsideWindow` "pencerenin bitiminden bir yıl sonrası") ve VAL-B142 (12:2616, U3a/U3c uyarısı) izler.
- **Etki yansıtmaya (1. turdan sürenler):** `10` KP-31 cümlesi ↔ `02 §3.28.1`; K-1545 (1) yük satırları `mock` ↔ K-1518 tanımı kayıt notu; `12 §11.1` "Kanıt ortamı yükselmesi" satırı; `skills/validate` PASS tanımı ↔ `12 §6.1` istisnası. Bu turdan eklenen: `skills/validate` ve `skills/gate-check` `Durum` anlatımında backlog kimliğinin hücreye yazıldığı bir ifade varsa F2 bağıyla hizalanır (`.claude/skills` altında "backlog kimliği" kalıbı bulunmadı — doğrulayıcı D2 tarama sonucu; etki yansıtmada yeniden taranır).
- **Hedefli mercek (3. tur öncesi):** kabul edilmiş risk sınıfı, §3 son paragraf.
- **`00`, `CLAUDE.md`, `SETUP.md`:** değişiklik önerilmedi.

## 5. Kullanıcı onay checklist'i

Toplu onay modu açık (K-1504); karar **(toplu onayla kaydedildi)**; ⚠ taşıyan karar: K-1563.

- [x] F1 — alt bilgi sürümü (v0.24 → v0.25 bu turda) — uygulandı
- [x] F2 — `Durum` hücresi yalnız `KABUL EDİLMİŞ RİSK`; risk kaydı madde kimliğiyle bulunur (§8.4, §6.1) — uygulandı
- [ ] F3 — `12`'de uygulama yok; kaynak devri K-1563 ⚠ (etki yansıtmada)
- [ ] F4, F5 — çürük, uygulama yok (F5 tekrar eden RET)
- [x] Sayımlar: satır eklenmedi — matris 1.502 satır, ORTA 733, KRİTİK 637, DÜŞÜK 132, `[güvenlik]` 63; `12_matris_denetle.py --kapsam 1-77`, `12_kapsam_hizasi.py`, `12_atif_denetle.py` hata 0

**Sonuç: TEMİZ DEĞİL — 3. tur öncesi kabul edilmiş risk sınıfının hedefli merceği** (`12_CROSS_REVIEW_R3.md`, aynı betikler ve parça sınırları, `12` v0.25).
