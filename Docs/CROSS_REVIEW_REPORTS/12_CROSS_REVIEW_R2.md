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


## Hedefli mercek (Faz 4 madde 6)

**Tarih:** 2026-11-01 · **Hedef:** `Docs/12_VALIDATION_PROTOCOL.md` v0.25 → v0.26 (girdi commit'i `fd830da`) · **Sınıf:** kabul edilmiş risk yolunun ve `Durum` hücresi yaşam döngüsünün güvencesi — her geçiş için dört taraf: (a) kim yazar, (b) hangi kapı okur, (c) betik ve okuma sorgusu değeri tanır mı, (d) kanıt ve backlog bağı nerede (1. tur F1 ve 2. tur F2 bu sınıfın iki örneğiydi; yukarıdaki §3 son paragraf ve §4). Sonuç: **ikinci modele (Codex) çıkılmadı**; mercek ve doğrulayıcılar alt ajandır. **Sonuç: TEMİZ DEĞİL** — 13 tekil bulgu: 3 doğru · 9 kısmen · 1 çürük; 12'si uygulandı.

### Koşum

`paketle.py` ile `12`'nin ilgili satırları (satır numaralı `12:<n>|`; §4 / §4.1 / §4.2, okuma tablosu, §6.1–§6.3, §7.5, §8.1–§8.5, §10, §11, `Durum` sayımı), `00 §D.3`, §D.7, §D.8, §E, §F, §H, iki betiğin tamamı (`12_matris_denetle.py`, `12_kapsam_hizasi.py`) ve iki skill (`validate`, `gate-check`) paketlendi. **İki dalga, on `[ozet]` Sonnet / low mercek** (beşli dalgalar, ön planda; talimat 3,5 KB'ın altında: yalnız bu sınıf, örnekler, önceki turlarda RET edilen iddiaların listesi, bulgu biçimi, dolgu yok):

- **1. dalga (S1–S5):** her dilim bir konu dilimiydi (değer kümesi ve betikler; kapı satırları ve gate-check; §7–§8; §9–§11 ve `00 §F`; validate skill). Dört taraf ayrı dilimlere düştüğü için merceklerin üçü "öbür taraf pakette yok" diye döndü: 1 ham bulgu (S2: gate-check Adım 8 ↔ `§6.2`), öteki dört dilim 0. Bu, dilimleme hatasıydı (sınıf çapraz bir eşleştirme ister).
- **2. dalga (T1–T5):** her pakete yaşam döngüsü çekirdeği (`§4.1` değer kümesi, `§6.1`–`§6.3`, `§7.5`, `§8.2`–`§8.4`, `§10`, `§11`) ve bir karşı taraf kondu (T1 iki betik; T2 validate skill; T3 gate-check + `00 §E`; T4 `00 §D`, §H + `§8.5`–`§10`; T5 `§6.2`–`§6.3` + `§7`). 17 ham bulgu.

| Dilim | Ham bulgu (Orta · Düşük) |
|---|---|
| S1, S3, S4, S5 | 0 |
| S2 | 1 (1 · 0) |
| T1 | 3 (3 · 0) |
| T2 | 3 (3 · 0) |
| T3 | 3 (3 · 0) |
| T4 | 4 (3 · 1) |
| T5 | 4 (2 · 2) |
| **Toplam** | **18 (15 · 3)** |

**Ham → tekil:** 18 ham bulgu **13 tekil** bulguya birleşti (M01…M13; birleşme: M01 = §8.2 "backlog'a yazıldı" ↔ §6.1, dört mercekte; M06 = `KRİTİK` + `KABUL EDİLMİŞ RİSK` kısıtı, iki mercekte; M08 = `KABUL EDİLMİŞ RİSK`'ten çıkış, iki mercekte). Mercek seviyeleri: Yüksek 0, Orta 15 ham, Düşük 3 ham; hiçbiri yükselmedi.

### Doğrulayıcılar

4 `[dogrulama]` Opus / high doğrulayıcı (adım sınırı 4/20; bekçi olayı yok), kademeli, aynı bölümden 2–4'lü: D1 (M01 · M07 · M08: `12 §8.2`/`§8.4`), D2 (M02 · M03: `skills/validate`), D3 (M04 · M05 · M10 · M11: `skills/gate-check`), D4 (M06 · M09 · M12 · M13: `§4.1`/`§6.1`/`§6.3`/`§7.5`). Doğrulayıcı talimatı: `a10-kd-cross/m/talimat_sablon_m.md` (R2 talimatının hedefli merceğe uyarlanmışı; skill ve betik düzeltmeleri için `DOSYA` / `D_ESKI` / `D_YENI` alanları). Hükümler `12_audit_hukum_denetle.py` ile sınandı (hüküm doğru 3 · kısmen 9 · çürük 1, çift 11, sorun 0); skill çiftlerinin `D_ESKI` parçaları ayrıca `grep -c -F` ile tam bir kez geçtiği için uygulandı. Sığmayan yok (`dogrulanmadi.md`: 0). Yükselen yok.

| # | Konu (taraf) | Hüküm | Seviye (mercek → doğrulayıcı) | Gerekçe ve şartname |
|---|---|---|---|---|
| M01 | `§8.2` Karar alanı "`DEFERRED_BACKLOG`'e yazıldı" ve "Kapanmayan ORTA ve DÜŞÜK bulgu backlog'a taşınır" ↔ `§6.1` "onaysız backlog'a yazılarak geçilmez" (yazan ↔ okuyan kapı) | **doğru** | Orta → Orta | İki yer çelişiyordu (4); `§8.2` kapanmayan satıra bağlı bulguyu onaysız backlog'a taşıyordu. **Uygulandı (2 çift):** Karar seçenekleri "Bu PR'da düzeltildi / `TXXa` açıldı / kabul edilmiş risk (`§8.4`; onay ve backlog kaydı)"; ORTA/DÜŞÜK backlog'a yalnız onayla kabul edilmiş risk olarak taşınır, satıra bağlı olmayan gözlem K-notudur. Yan: `§6.3` "DÜŞÜK … kayıtlıdır" (M13). |
| M02 | `skills/validate` adım 16 matris `Durum` hücrelerini yazmıyor ↔ `§7.5`, `§6.1` (yazan) | **kısmen** | Orta → Orta | Ana iddia doğru: `12` hücreyi adım 16 commit'ine bağlar, skill hiç anmaz. "FAIL'de skill commit yazmaz" kısmı çürük (adım 14 ve 16 verdict'e bağlı değil) ama skill açıkça yazmıyordu. **Uygulandı (skill, 1 çift):** adım 16 `Durum` hücrelerini PASS'te de FAIL'de de yazar (eşleme `§6.1`: `✓` PASS · `✗` FAIL · `?` BEKLEMEDE · `~` §6.1 eşlemesi · onaylı risk KABUL EDİLMİŞ RİSK). |
| M03 | `skills/validate` verdict'i kabul edilmiş risk istisnasını, backlog okumasını ve KRİTİK kısıtını bilmiyor ↔ `§6.1` (okuyan kapı, kanıt bağı) | **kısmen** | Orta → Orta | `12 §6.1` "Verdict PASS" satırı skill verdict'ini istisnayla tanımlar; skill'in PASS/FAIL tanımı bilmiyordu (K-1561 (6), K-1562 (8)'de devredilmiş açık not). Adım 12 "Üçüncü seçenek yoktur" yalnız satıra bağlı olmayan K-notları içindir (çürük kısım). **Uygulandı (skill 3 çift + 12 1 çift):** adım 5'e "Kabul edilmiş risk" maddesi (`grep -n '<VAL-kimliği>' DEFERRED_BACKLOG.md`; KRİTİK değilse ve kayıt varsa satır KER, verdict FAIL olmaz; yoksa FAIL; onay doğrulayıcının kararı değildir), adım 11 PASS/FAIL tanımları; `12:1701`'deki bayat "kabul edilebilir `~`" atfı kaldırıldı. |
| M04 | `skills/gate-check` Adım 8 `§6.2` "Açık risk envanteri güncel" kontrolünü yapmıyor (okuyan kapı) | **doğru** | Orta → Düşük | Kapı `12`'de var, onu işleten skill'de yok (4); düşürme: her risk oluşurken zaten proje sahibi onayı alınır (`§8.4`), Adım 8 açık kalemleri kod düzeyinde doğrular — eksik olan faz düzeyinde envanter görüşü ve hücre ↔ backlog bağının faz kapısında okunması. **Uygulandı (skill, 2 çift):** Adım 8'e "Açık risk envanteri" maddesi ve çıktı biçimine "Açık Risk Envanteri" tablosu. |
| M05 | `skills/gate-check` faz satırlarını (`§6.2` tablosu) okumuyor, `Durum` hücrelerini yazmıyor; PASS ölçütü satıra bakmıyor (okuyan kapı, yazan) | **doğru** | Orta → Orta | `§6.2` hangi satırların okunduğunu söyler ama o satırların hangi `Durum`la kapıdan geçtiğini söylemez; skill PASS tanımı kapalı bir liste. **Uygulandı (12 2 çift + skill 5 çift):** `§6.2`: okunan satır PASS ya da kaydı bulunan KER değilse gate FAIL, hücreleri gate-check raporla aynı commit'te günceller (`§7.5`); skill: Adım 4'e "Faz satırları" maddesi, PASS ve FAIL ölçütleri, chore PR içeriği, çıktı biçimine "Faz Satırları" tablosu. |
| M06 | `12_matris_denetle.py` KRİTİK + KER birlikteliğini sınamıyor; `§6.1` "KRİTİK açık bulgu yok" satırı KER'i anmıyor (betik ↔ değer kümesi) | **çürük** | Orta → Düşük | Birliktelik kapılarda zaten kapalı: `§6.1` ilk satır yalnız "KRİTİK olmayan" KER'i kabul eder (12:1700), final çıkış "Tüm KRİTİK maddeler PASS" der (12:1734); `§6.1` KRİTİK satırı aynı tablonun ek kriteridir. Betik bu kontrolü yaptığını iddia etmez (12:376 saydıklarını sayar; 12:1860 "başka değeri reddeder" yalnız değer kümesi için doğrudur). Betiğe kontrol eklemek savunmada derinliktir. |
| M07 | FAIL → KABUL EDİLMİŞ RİSK geçişinin yazarı ve turu (yazan) | **kısmen** | Orta → Orta | Yazar çelişkisi iddiası çürük: karar proje sahibinindir (`§10`), hücreyi doğrulayıcı backlog kaydını okuyarak yazar (12:1700, 12:1799). Geçerli kalan: onay FAIL verdict'inden sonra gelir; kaydı kimin açtığı ve PASS'i verecek turun iterasyon sayacına girip girmediği yazılı değil, eskalasyonda (`§8.5` seçeneği kabul edilmiş risk) PASS'e giden yol yok (3). **Uygulandı (12 1 çift + skill 1 çift + L3 1 cümle):** geçiş sırası `§8.4`'te; kabul turu iterasyon sayılmaz, tur numarası artmaz; `skills/validate` iterasyon paragrafı ve `INSTRUCTIONS.md` §3.3 eskalasyon maddesine istisna cümlesi. Karar (K-1564 (6)). |
| M08 | KABUL EDİLMİŞ RİSK'ten çıkış (çözüldü / onay geri çekildi) ve gate regresyonunun KER satırını yeniden FAIL yapması (yazan, kanıt/backlog bağı) | **kısmen** | Orta → Orta | Geçerli: `✅ Çözüldü` olan kayıt silinmediği için `grep` onu bulmaya devam eder ve hücre KER kalır; regresyonda kayıtta kabul edilen sapmanın yeniden görülmesi yeni FAIL gibi okunuyordu; onay geri çekilince FAIL'e dönüş yazılı değildi (4). Çürük kısım: test raporlu A–E satırlarında çözülen riskin PASS'e dönüşü regresyon + `§11.1` madde 1 ile zaten kapsanıyor. **Uygulandı (12 2 çift + skill 1 çift):** `§8.4` "Çıkış": açık (`✅ Çözüldü` işaretsiz) kaydı olmayan KER hücresi FAIL; kayıttaki sapma hücreyi değiştirmez ve yeni FAIL değildir; beklenen sonuç karşılanırsa PASS ve kayıt `✅ Çözüldü`; sapma aşılır ya da onay geri çekilirse FAIL ve `§8.2` (`TXXa`); `§11.1` madde 1'e aynı ek; gate-check hijyen maddesi. |
| M09 | `Durum` hücresini gate'te ve final çıkışta kimin yazdığı; mock PASS'in BEKLEMEDE'ye dönüşü (yazan) | **kısmen** | Orta → Orta | Geçerli: `§7.5` yazarı yalnız task doğrulayıcısı (`skills/validate` adım 16) olarak adlandırır; gate regresyonu ve `§11.1` madde 4 dönüşü task dışında olur. Çürük kısım: `§6.3` "BEKLEMEDE kalır" final anından bakar, `§4.1` BEKLEMEDE tanımı dönüşü kapsar, çelişki yok. **Uygulandı (12 1 çift + skill 2 çift):** `§7.5`: task dışında yeniden doğrulanan satırların hücresini gate-check gate raporuyla aynı PR'da günceller; `validate` adım 16 ve `gate-check` PASS ritüeli. |
| M10 | `skills/gate-check` Adım 1 yalnız test koşar; `§6.2` Test raporu dışı kanıtlı A–E satırlarının kanıtının yeniden üretilmesini ister (okuyan kapı, kanıt) | **kısmen** | Orta → Orta | Çekirdek doğru (4): skill'de test dışı kanıtlı satırların yeniden doğrulanması yoktu; "satır düzeyinde S2 yok" kısmı abartılı (madde 7 kırılan testi zaten S2 sayar). **Uygulandı (skill 1 çift):** madde 7'ye kayıtlı `Durum` yeterli değildir; kanıtı Test raporu olmayan A–E satırlarından yeni kodun etkileyebilecekleri yeniden doğrulanır, daha önce PASS almış satırın FAIL'i S2 Kırılmadır (`§11.1` madde 2). |
| M11 | `skills/gate-check` madde 16 "bloke-etmeyen bulgu backlog'a forward" ↔ `12 §6.1`/`§8.4` (okuyan kapı) | **kısmen** | Orta → Orta | Forward kalıbı `00 §E.3`/§H kaynaklıdır ve satıra bağlı olmayan gözlem içindir; ama skill bloke edeni yalnız "kritik bulgu" ile ayırır, gate'te FAIL alan ORTA/DÜŞÜK satır onaysız backlog'a geçebilirdi (4). **Uygulandı (skill 1 çift):** bloke-etmeyen bulgu yalnız `12` satırına bağlı olmayan gözlemdir; FAIL satır bloke eder ve backlog'a yalnız onayla KER olarak yazılır (KRİTİK olamaz). |
| M12 | KER kaydının tipi `k-note` ve onay kaynağı alanı yok (kanıt/backlog bağı) | **kısmen** | Orta → Düşük | Tip kısmı çürük (`00 §H` tip listesinde risk tipi yok; KER ↔ K-notu ayrımını tip değil açıklamanın başındaki "KABUL EDİLMİŞ RİSK — `VAL-…`" öneki taşır). Geçerli (mekanizma): kayıt biçimi yalnız kabul tarihini yazdırır, doğrulayıcı "o risk için onay" şartını kayıttan sınayamaz. **Uygulandı (1 çift):** kayıt onayın kaynağını da yazar (karar satırının kimliği). |
| M13 | `§6.3` "DÜŞÜK maddeler bloklamaz — Ama kayıtlıdır" hangi kayıt, hangi `Durum`; KER'in finalde yeniden değerlendirilmesi (okuyan kapı) | **kısmen** | Orta → Düşük | Yeniden değerlendirme kısmı çürük (final çıkış raporu F6 gate raporudur; `§6.2` envanter ve hijyen taraması F6'da da koşar). Geçerli (4): "kayıtlıdır" FAIL kalan DÜŞÜK satırı onaysız geçirecek gibi okunuyordu. **Uygulandı (1 çift):** `BEKLEMEDE` kalan DÜŞÜK satır bloklamaz ve final çıkış raporunda kimliğiyle listelenir; `FAIL` alan DÜŞÜK satır düzelir ya da KER olur. Karar (öneriyle). |

**Hüküm dağılımı:** 3 doğru (M01, M04, M05) · 9 kısmen · 1 çürük (M06); seviyesi yükselen yok. Seviye: M04, M06, M12, M13 Orta → Düşük; öteki dokuzu Orta korundu. Tekrar eden RET yok (M06 yeni iddiaydı).

### Uygulanan

- **`12` (v0.26):** 11 ESKİ → YENİ çifti (`§6.1` bayat atıf 1, `§6.2` 1, `§7.5` 2, `§6.3` 1, `§8.2` 2, `§8.4` 3, `§11.1` 1); `12_audit_hukum_denetle.py` sorun 0. Matris satırı eklenmedi, değiştirilmedi.
- **L4 skill hizası (aynı PR):** `skills/validate` 5 değişiklik (adım 5 kabul edilmiş risk maddesi, adım 11 PASS/FAIL, adım 16 `Durum` hücreleri, iterasyon paragrafı); `skills/gate-check` 10 değişiklik (Adım 1 madde 7, Adım 4 faz satırları, madde 16, Adım 6 PASS/FAIL, Adım 8 envanter ve çözülen KER, PASS ritüeli, çıktı biçimi: "Faz Satırları", "Açık Risk Envanteri"). Önceki turlardan devreden "`skills/validate` PASS tanımı ↔ `12 §6.1` istisnası" notu kapandı (etki yansıtmaya artık devredilmez).
- **L3:** `.claude/INSTRUCTIONS.md` §3.3 eskalasyon maddesine kabul turu istisnası (1 cümle).
- **Betik:** `12_matris_denetle.py` ve `12_kapsam_hizasi.py` değer kümeleri değişmedi (M06 çürük; `Durum` değer kümesi dört değer, `§4.1` ile hizalı).
- **Sayımlar:** matris 1.502 satır, KRİTİK 637 · ORTA 733 · DÜŞÜK 132; `[güvenlik]` 63 (değişmedi); `12_kapsam_hizasi.py`, `12_matris_denetle.py --kapsam 1-77`, `12_atif_denetle.py` hata 0.

### Etki yansıtmaya devirler ve sonraki adım

- **`00`, `CLAUDE.md`, `SETUP.md`:** değişiklik önerilmedi (`00 §H` tip listesine risk tipi eklemek M12'de gerekmedi).
- **Etki yansıtmaya (kalanlar):** `10` KP-31 cümlesi ↔ `02 §3.28.1`; K-1545 (1) yük satırları `mock` kayıt notu; `12 §11.1` "Kanıt ortamı yükselmesi" satırı; K-1563 kaynak değişikliği ve VAL-A081/A236/A532/B142. Hedefli mercekten yeni üst doküman devri yok.
- **Sonuç sınıfı:** kabul edilen on iki bulgunun hiçbiri Yüksek değil; dokuzu Orta (kapı ↔ skill ↔ `§8.x` çelişkileri), üçü Düşük. Skill Faz 4 madde 6 gereği hedefli mercek bu adımla yapıldı: **sıradaki 3. tur** (`12_CROSS_REVIEW_R3.md`, aynı betikler ve parça sınırları, `12` v0.26).
