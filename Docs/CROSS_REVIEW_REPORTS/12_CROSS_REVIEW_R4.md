# Cross-Review — 12 Doğrulama Protokolü (Tur 4)

**Tarih:** 2026-11-01 · **Hedef:** `Docs/12_VALIDATION_PROTOCOL.md` v0.27 (girdi commit'i `7c5ee2b`, 837 194 bayt, 2 639 satır) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Ciddiyet ölçüsü:** 1. turdan itibaren (K-1399 (1); mekanizma okuma paragrafıyla) · **Sonuç:** TEMİZ DEĞİL — 7 ham bulgu = 7 ayrı bulgu (altı çağrıdan): 1 doğru (Düşük) · 4 çürük · 2 doğrulayıcısız tekrar eden RET; 1'i uygulandı (4 şartname çifti ve betik düzeltmesi).

## 1. Koşum

Betikler, istem, parça notu ve **parça sınırları değişmedi** (`12_CROSS_REVIEW.md` §1; kayıt K-1561): `C:\Users\teknodeveloper\.claude\helper-state\shopfolio\a10-kd-cross\betik\` — `build_inputs.py` (`python -I build_inputs.py <doküman> template12.md prompt.txt partnote.txt <girdi klasörü>`; kapsanmayan satır: yok), `prompt.txt`, `partnote.txt`, `template12.md`. Koşum: `bash scripts/doc-scans/cross_review_ikinci_model.sh <girdi> <çıktı> c1 p1 p2 p3 p4 p5` (K-1484). Altı çağrı paralel koştu, hepsi çıktı verdi; kullanım sınırı dolmadı, yedek modele geçilmedi (`status.txt`: `model=codex gpt-5.6-terra high`). Girdi `12` v0.27: 3. turun uyguladığı VAL-A231 ve VAL-A370 değişikliği ve üst/alt bilgi dışında önceki turdan farkı yoktur.

| Çağrı | Girdi | Token | Süre | Sonuç |
|---|---|---|---|---|
| tek parça (c1) | 854 677 bayt | 407 980 | 260 sn | 2 bulgu |
| P1 (§1–§4.2, §5–§11) | 134 955 bayt | 67 183 | 73 sn | 2 bulgu |
| P2 (§4.3–§4.12) | 280 102 bayt | 135 785 | 253 sn | 1 bulgu |
| P3 (§4.13–§4.20) | 336 392 bayt | 125 729 | 109 sn | TEMİZ |
| P4 (§12 girişi, §12.1–§12.7) | 201 607 bayt | 97 292 | 117 sn | 1 bulgu |
| P5 (§12.8–§12.13) | 215 752 bayt | 111 285 | 127 sn | 1 bulgu |

**İki koşumun ölçümü.** Tek parça çağrı iki bulgu getirdi (VAL-A073, VAL-A075; ikisi de yalnız tek parçada çıktı: parçalı koşum bunları üretmedi); parçalı koşum beş ham bulgu getirdi (P1 2, P2 1, P4 1, P5 1). Tek parça ile parçalı koşum bu tur birbirinin bulgusunu üretmedi. Yasal dayanak taşıyan iddialar doğrulayıcılarda resmî metne karşı okundu (okuma tarihi 2026-11-01): Mesafeli Sözleşmeler Yönetmeliği m.5/1-g ve m.12/5 (24/5/2025-32909 sayılı Resmî Gazete), m.12/1 (23/8/2022-31932 sayılı Resmî Gazete), 6563 sayılı Kanun m.11 (7416 sayılı Kanun m.5; TBMM kanun metni — mevzuat.gov.tr PDF'inden metin çıkarılamadı).

## 2. Ham bulgular

İkinci modelin çıktıları, satır sonu boşlukları dışında düzenlenmeden (alıntı blokları içinde).

**(a) Tek parça çağrı:**
> BULGU-1
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: §4.6, VAL-A073
> Alıntı: "İade için taşıyıcı belirlenmez; taşıyıcıyı müşteri seçer."
> Sorun: 1 Ocak 2026’dan beri ön bilgilendirmede satıcının iade için öngördüğü taşıyıcının belirtilmesi zorunludur. Satır, bunun hiç belirlenmemesini PASS davranışı yapıyor; bu yürürlükteki mevzuata aykırıdır. [Resmî Gazete değişikliği](https://resmigazete.gov.tr/eskiler/2025/05/20250524-2.htm)
> Öneri: Ön Bilgilendirme Formu ve sözleşme kopyasında öngörülen iade taşıyıcısını zorunlu kılın; VAL-A073’ü bu bilginin görünmesini ve belirtilen taşıyıcıyla iadede tüketiciye masraf yüklenmemesini sınayacak şekilde değiştirin.
>
> BULGU-2
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: §4.6, VAL-A075
> Alıntı: "malın firmaya ulaştığı tarihten"
> Sorun: Satır, caymada iade süresini koşulsuz olarak malın firmaya ulaşmasına bağlıyor. Oysa tüketicinin ön bilgilendirmede belirtilen taşıyıcıya ürünü teslim ettiği tarihte iade süresi başlar; yalnız farklı bir taşıyıcı kullanılmışsa ürünün satıcıya ulaşması esas alınır. Yanlış başlangıç, bedel iadesini geciktirerek tüketicinin para ve hak kaybına yol açar. [Ticaret Bakanlığı güncel rehberi](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/mesafeli-sozlesmeler-hakkinda-bilgilendirme)
> Öneri: VAL-A075’i iki ayrı senaryoya ayırın: öngörülen taşıyıcıda teslim tarihi, başka taşıyıcıda firmaya ulaşma tarihi başlangıç olsun; her iki durumda 14 günlük iade süresini doğrulayın.
>
> SONUÇ: 2 BULGU

**(b) Parça 1:**
> BULGU-1
> Kriter: tutarlılık
> Seviye: Yüksek
> Yer: §4.2 madde 8; §6.2 F4; §7.4
> Alıntı: "Dış servisin sonucuna dayanan satır — ödeme sağlayıcısı, e-posta altyapısı, Google — `mock` yazar"
> Sorun: Bu kural, dış servis sonucuna bağlı tüm satırlar için `mock` zorunluluğu koyuyor; oysa F4 kapısı aynı entegrasyonlar için `sandbox` kanıtı istiyor ve §7.4 seviye E satırlarında `sandbox`/`gerçek` ortamlarını tanımlıyor. Aynı satırın hangi ortam değerini taşıyacağı belirlenemediğinden, entegrasyon kanıtı ve final kapısı mekanik olarak yanlış değerlendirilebilir. Bu, doküman içi çelişkidir.
> Öneri: Maddeyi, ortamın gerçek koşuma göre `mock` / `sandbox` / `gerçek` yazılacağını; yalnız mock ortamda üretilen PASS’in finalde çekirdek entegrasyon kanıtı sayılmayacağını söyleyecek biçimde düzeltin.
>
> BULGU-2
> Kriter: tutarlılık
> Seviye: Orta
> Yer: §4 matris sütun açıklaması; §4.2 madde 4 ve 10; §1.3; §2 P7
> Alıntı: "Kuralın tanımlandığı kaynak birimi: `10` KP (ÖK, SK) kimliği"
> Sorun: SK-1…SK-10, §1.3’e göre “kriter değildir”; P7 de kriter satırı taşıyan SK kimliği bulunmamasını zorunlu kılar. Buna rağmen matrisin kaynak tanımı SK kimliğini kabul ediyor; denetim kuralı ise yalnız KD ve YH yasağını denetliyor. Böylece SK kaynaklı kapsam dışı bir kriter matrise girebilir ve belirtilen mekanik denetimden geçebilir. Bu, kapsam hizası güvencesiyle çelişir.
> Öneri: Kaynak açıklamasından SK’yi çıkarın; ÖK kullanımını yalnız dış ön koşul olarak sınırlayın. Matris denetimine SK kimliği yasağını ekleyin.
>
> SONUÇ: 2 BULGU

**(c) Parça 2:**
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §4.6 — VAL-A080, VAL-A081
> Alıntı: “Firma iki bildirimi de kaydeder; kayıt bildirimin firmaya ulaştığı tarihi taşır; pencere dışındaki tarihte panel uyarır ve kaydı ayrıca onaylatır; gönderim tarihi ayrıca tutulmaz”
> Sorun: VAL-A080, süresinde gönderilip geç ulaşan cayma bildirimini kabul ederken kayda yalnız ulaşma tarihinin yazılmasını öngörüyor. VAL-A081 ise kabul/ret kararını yalnız girilen tarihe bağlıyor ve gönderim tarihini taşıyan alanı açıkça yasaklıyor. Bu, süresinde yöneltilmiş bir cayma bildiriminin geç ulaştığı durumda hakkın reddedilmesine yol açar; hem iki satır çelişir hem tüketicinin cayma hakkı bakımından hak kaybı doğurur.
> Öneri: Gönderim/yöneltme tarihi ile firmaya ulaşma tarihini ayrı, kanıt referanslı alanlar olarak kaydedin. Süre uygunluğunu gönderim/yöneltme tarihine göre; operasyonel işlem sürelerini ise mevzuattaki ilgili başlangıç anına göre değerlendirin.
>
> SONUÇ: 1 BULGU

**(d) Parça 3:**
> SONUÇ: TEMİZ

**(e) Parça 4:**
> BULGU-1
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: §12.7, `VAL-D091`
> Alıntı: "Ödemesi hiç alınmamış siparişin kişisel verisi — ön bilgilendirme ve onay kayıtları dahil — Başarısız olduğu andan üç yılda silinir"
> Sorun: Bu kural, ödeme alınmamış olsa bile verilmiş elektronik siparişe ait ön bilgilendirme ve onay kayıtlarını üç yılda silmektedir. 6563 sayılı Kanun m.11, hizmet sağlayıcının kanun kapsamındaki iş ve işlemlerine ilişkin bilgi, belge, defter ve elektronik kayıtları işlem tarihinden itibaren on yıl saklamasını zorunlu kılar. Bu, yasal saklama yükümlülüğüne aykırılık ve tüketici uyuşmazlığında ispat/hak kaybı riski doğurur. [Resmî kanun değişikliği metni](https://cdn.tbmm.gov.tr/KKBSPublicFile/D27/Y5/T2/KanunMetni/ee8386bd-1e17-4967-a960-3deb60425652.html)
> Öneri: `VAL-D091` ve dayandığı saklama kuralı, siparişe ilişkin zorunlu elektronik kayıtları — ön bilgilendirme ve onay dâhil — işlem tarihinden itibaren en az on yıl saklayacak şekilde düzeltilmeli; erişim kısıtlama/ayrı arşivleme uygulanacaksa silme yerine bu yöntem doğrulanmalıdır.
>
> SONUÇ: 1 BULGU

**(f) Parça 5:**
> BULGU-1
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: §12.9, VAL-A900
> Alıntı: "İkinci talep açılamaz; talep yeniden açılır ve ayıplı adet, açılırken seçilen adet olarak kalır"
> Sorun: Yeni ortaya çıkan ayıplı adet için ayrı talep açılamaması ve yeniden açmada adedin ilk seçilen miktara kilitlenmesi, tüketicinin başka bir ayıplı adet için seçimlik hakkını kullanacağı bir yol bırakmaz. Bu, hak kaybı doğurur ve 6502 sayılı Kanun m.11’deki ayıplı malda tüketicinin seçtiği talebi yerine getirme yükümlülüğüyle bağdaşmaz. [Resmî metin](https://tuketici.ticaret.gov.tr/data/5e81982d13b876a1b04c7a42/Tuketicinin_Korunmasi_Hakkinda_Kanun_6502_Ocak_2021.pdf)
> Öneri: Kalem başına tek ana talep kaydı korunacaksa dahi, sonradan ortaya çıkan her ayıplı adet için talep altında yeni bir adet/olay ve seçimlik hak kaydı açılabilsin; ilk talebin adet ve geçmişi değişmeden kalsın.
>
> SONUÇ: 1 BULGU

## 3. Bağımsız değerlendirme

Yedi ham bulgu yedi ayrı bulgudur (birleşme yok). Beş bulgu için beş Opus/high doğrulayıcı (D1 F1; D2 F2; D3 F3; D4 F4; D5 F5; **adım sınırı 5/20**). **F6** (VAL-A080/A081 gönderim tarihi) ve **F7** (VAL-A900) doğrulayıcıya gitmeden kaydedildi (hüküm dayanakları tabloda). Hüküm dosyaları: `C:\Users\teknodeveloper\.claude\helper-state\shopfolio\a10-kd-cross\r4\hukum_D1.md`…`hukum_D5.md` (`12_audit_hukum_denetle.py`: hüküm doğru 1 · çürük 4, çift 4, uygulanabilir çiftlerde sorun 0; tek uyarı çürük hükmün boş `ESKI:` alanıdır).

| # | Parça · Yer | Konu | Hüküm | Seviye (model → doğrulayıcı) | Gerekçe ve şartname |
|---|---|---|---|---|---|
| F1 | tek parça · §4.6 VAL-A073 | İade için taşıyıcı belirlenmez; 1 Ocak 2026'dan beri ön bilgilendirmede satıcının öngördüğü taşıyıcı zorunludur | **çürük** | Yüksek → Düşük | **Önceki hüküm** (`03` cross-review R4 BULGU-2: aynı iddia RG 24/5/2025-32909 metnine karşı reddedildi; K-492, K-493). Resmî metin yeniden okundu: m.5/1-g yalnız "satıcının iade için öngördüğü taşıyıcıya ilişkin bilgiler"in verilmesini ister; **m.12/5** "satıcının ön bilgilendirmede iade için herhangi bir taşıyıcıyı belirtmediği durumda" tüketiciden iade masrafı istenemeyeceğini düzenleyerek taşıyıcı belirtmemeyi açıkça öngörür. 12:531 VAL-A073 ile `02 §7.4.4` hizalıdır; formun taşıyıcı bilgisi VAL-A846'da (12:2289) sınanır. İkinci model yeni kanıt getirmedi. |
| F2 | tek parça · §4.6 VAL-A075 | İade süresi koşulsuz malın firmaya ulaşmasına bağlanıyor | **çürük** | Yüksek → Düşük | Yönetmelik **m.12/1** (RG 23/8/2022-31932): tüketici malı iade için öngörülenin haricinde bir taşıyıcıyla iade ederse süre malın satıcıya ulaştığı tarihten başlar. Firma iade taşıyıcısı belirlemediği için (`02 §7.4.1`, `§7.4.4`; K-491, K-493) teslimden sonraki her iade bu dala girer; VAL-A075 (12:533) kaynakla uyumludur. Dalın dayandığı olgu F1'de sınanır. |
| F3 | P1 · §4.2 madde 8; §6.2 F4; §7.4 | Dış servis satırları için `mock` zorunluluğu ↔ F4 kapısının `sandbox` kanıtı | **çürük** | Yüksek → Düşük | **Önceki hüküm / zaten karşılandı:** madde 8 kendi son cümlesiyle `sandbox` ve `gerçek` değerlerini seviye E ve canlı kanıt satırlarına bırakır (12:374), madde 5 A–D satırlarının servisin sözleşmesini değil ürünün tepkisini sınadığını söyler (12:371); F4 kapısı E satırlarını okur ve bunlar §4.16 (1)'e göre `sandbox` taşır (12:1130, 12:1726). 1. tur D3 #4 (K-1561) ve K-1521 (6) ile aynıdır; çelişki yok. |
| F4 | P1 · §4 sütun tanımı; §4.2 madde 4 ve 10; §1.3; §2 P7 | Kaynak hücresi tanımı `10` KP (ÖK, SK) kimliğini kabul ediyor; SK kriter değildir | **doğru** | Orta → Düşük | 12:338 sütun tanımı SK'yi kaynak kimliği sayar; §1.3 (12:239) SK-1…SK-10'u "sınırdır, kriter değildir" diye yazar, P7 (12:281) kriter satırı taşıyan SK olmamasını şart koşar, §4.2 madde 4 (12:370) yalnız KP'yi ve koşullu ÖK'yi yazdırır — dokümanın iki yeri çelişir (4). Mekanik güvence de eksikti: madde 10 (12:376) ve `12_matris_denetle.py` Kaynak denetimi yalnız `(KD\|YH)-\d+` reddediyordu. Bugün Kaynak hücresinde SK taşıyan satır 0 olduğu için etki gizildir, seviye Düşük. **Uygulandı (4 çift + betik):** (1) sütun tanımı: `10` KP kimliği; ÖK yalnız ön koşulu dış ön koşula bağlarken yazılır, KD, SK ve YH kimliği yazılmaz (§4.2 madde 4); (2) §4.2 madde 4: "KD, SK ve YH kimliği hiçbir satırda yazılmaz (kapsam hizası; SK sınırdır, kriter değildir — §1.3)"; (3) madde 10: "KD, SK ve YH yasağını"; (4) P7: "KD, SK ya da YH kimliği taşımadığını sınar". `scripts/doc-scans/12_matris_denetle.py`: Kaynak denetimi `(KD\|SK\|YH)-\d+` reddeder, mesaj ve yardım metni hizalandı. |
| F5 | P4 · §12.7 VAL-D091 | Ödemesi alınmamış siparişin ön bilgilendirme ve onay kayıtları 3 yılda siliniyor; 6563 m.11: on yıl | **çürük** | Yüksek → Düşük | **Önceki hüküm / zaten karşılandı:** VAL-D091 (12:2247) kaynağın kendisidir (`02` Z-41; `02 §12.2.7`: "Kişisel veri: 3 yıl · kişisel verisiz kayıt: en az 10 yıl"); bu karma süre proje sahibinin 6563 m.11/3 için bilinçli kararıdır (K-951 (2): "hepsi on yıl, kişisel bilgiyle" seçeneği elendi, KVKK m.4/2-ç, m.4/2-d). İddianın "kayıtları üç yılda siler" okuması eksiktir: kalan kişisel verisiz kayıt on yıl durur ve onaylanan yasal metin sürümlerini, onay kutularının kaydını ve Ön Bilgilendirme Formu ile Mesafeli Satış Sözleşmesi'nin kişisel verisiz kopyasını taşır. Resmî metin: 7416 sayılı Kanun m.5 ile 6563 m.11'e eklenen cümle (TBMM). |
| F6 | P2 · §4.6 VAL-A080, VAL-A081 | Gönderim tarihi tutulmuyor; cayma süresi ulaşmaya bağlanıyor | **çürük (doğrulayıcısız)** | Yüksek → Düşük | **Tekrar eden RET (beşinci kez):** aynı yer ve aynı iddia audit 95, deep-2a 23, 2. tur F3 ve 3. tur F1'de gerekçeyle reddedildi (K-709 bilinçli karar: "kayıt iki tarih taşısın" elendi; 12:539 uyarı ve ayrı onayla süresinde gönderilmiş, geç ulaşmış hâli kapatır; Yönetmelik m.11/1 "yöneltilmesi yeterlidir" ve m.11/4 ispat tüketicide). 3. turda tek geçerli kısım (VAL-A231 Beklenen) uygulandı; yeni kanıt yok. Turun sonucunu belirlemez (skill Faz 4 madde 5). |
| F7 | P5 · §12.9 VAL-A900 | Yeniden açılan talepte ayıplı adet artırılamıyor; yeni talep de açılamıyor | **kısmen (3. tur F5; doğrulayıcısız)** | Yüksek → Orta | **Tekrar eden iddia:** 3. tur F5'te `12`'nin kaynağı birebir izlediği, çelişkinin kaynakta (K-793 ↔ K-808) olduğu ve `12`'de değişiklik yapılmayacağı hükmedildi (K-652); karar K-1566 ⚠ (yeniden açmada ayıplı adet artırılabilir) kaynağa ve etki yansıtmaya (A10-KD-etki) devredildi. Aynı yer, aynı iddia, yeni kanıt yok; `12` satırı kaynak değişene kadar kaynağı izler. Turun sonucunu belirlemez. |

**Hüküm dağılımı:** 1 doğru (F4) · 4 çürük (F1, F2, F3, F5) · 2 doğrulayıcısız tekrar eden RET (F6, F7); seviyesi yükselen yok. Seviye: F1–F3, F5, F6 Yüksek iddia edildi, Düşük; F4 Orta iddia edildi, Düşük; F7 Orta.

**Sonuç sınıfı ve sıradaki adım.** Kabul edilen tek bulgu Düşük ve iki yerin metin farkıdır (4); Orta kabul yoktur. **Hedefli mercek (skill Faz 4 madde 6) gerekmez:** art arda iki turda aynı sınıfın Orta kabulü şartı sağlanmadı. **5. tur gerekir** (aynı betikler ve parça sınırları, `12` v0.28). 5. turdan itibaren çıkış kuralı (skill Faz 4 madde 5) işler: kabuller Düşük metin farkıysa döngü o turda kapanır; kalan sınıf checkpoint'e devredilir. Tekrar eden RET sayısı: F6 (gönderim tarihi, beşinci kez), F7 (VAL-A900, 2. kez), VAL-A913 ve VAL-A907 bu turda gelmedi.

## 4. Ek bulgular ve devirler

- **Etki yansıtmaya (F4):** `12` P7'nin `12_kapsam_hizasi.py` için verdiği "kriter satırı taşıyan KD, SK ya da YH olmadığını gösterir" sözü betikte karşılıksızdır: `12_kapsam_hizasi.py` yalnız anma sayar (YH için "0 anma" sabit metindir); satır yasağını `12_matris_denetle.py` sınar. Cümle ya da betik etki yansıtmada hizalanır. Bugünkü satırlar etkilenmez (Kaynak'ında SK taşıyan VAL satırı 0; ÖK taşıyan 14 satır madde 4'e uygun).
- **Etki yansıtmaya (önceki turlardan sürenler):** `02 §7.3.5` "süreye uygunluk gönderimle ölçülür" notu (K-1565); `10` KP-31 cümlesi ↔ `02 §3.28.1`; K-1545 (1) yük satırları `mock` ↔ K-1518 tanımı kayıt notu; `12 §11.1` "Kanıt ortamı yükselmesi" satırı; `skills/validate` PASS tanımı ↔ `12 §6.1` istisnası; K-1563 kaynak düzeltmesi ve VAL-A081/A236/A532/B142; K-1566 kaynak değişikliği (`02 §5.10`, `02 §7.5.3`, `04 §5.19.4`, `04 §9.9.30`, `07` UC-46) ve ardından VAL-A900, VAL-A905, VAL-A687, `06 §4.30`.
- **Hedefli mercek:** bu tur için gerekmez (§3 son paragraf).
- **`00`, `CLAUDE.md`, `SETUP.md`:** değişiklik önerilmedi.

## 5. Kullanıcı onay checklist'i

Toplu onay modu açık (K-1504); karar **(toplu onayla kaydedildi)**; ⚠ taşıyan yeni karar yok.

- [ ] F1, F2, F3, F5 — çürük, uygulama yok
- [x] F4 — SK kimliği Kaynak hücresinde yazılmaz (doğru) — 4 çift ve `12_matris_denetle.py` uygulandı
- [ ] F6, F7 — tekrar eden RET / K-1566 devri, uygulama yok
- [x] Sayımlar: satır eklenmedi — matris 1.502 satır, ORTA 733, KRİTİK 637, DÜŞÜK 132, `[güvenlik]` 63; `12_matris_denetle.py --kapsam 1-77`, `12_kapsam_hizasi.py`, `12_atif_denetle.py` hata 0

**Sonuç: TEMİZ DEĞİL — 5. tur** (`12_CROSS_REVIEW_R5.md`, aynı betikler ve parça sınırları, `12` v0.28; çıkış kuralı 5. turdan itibaren).
