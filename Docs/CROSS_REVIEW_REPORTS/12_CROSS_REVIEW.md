# Cross-Review — 12 Doğrulama Protokolü (Tur 1)

**Tarih:** 2026-11-01 · **Hedef:** `Docs/12_VALIDATION_PROTOCOL.md` v0.23 (girdi commit'i `f148238`, 830 377 bayt, 2 638 satır) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Ciddiyet ölçüsü:** 1. turdan itibaren uygulanır, mekanizma okuma paragrafıyla (K-1399 (1); kuralı `02`–`11`'den devralan ve kendi mekanizma kararlarını koyan Doğrulama Protokolü ağır sayma ölçüsü listesindedir; ölçü 1. turdan açık) · **Sonuç:** TEMİZ DEĞİL — 8 ayrı bulgu: 2 doğru · 4 kısmen · 2 çürük; 6'sı uygulandı (7 şartname çifti + 2 hücre + 1 yeni satır).

## 1. Koşum

**Betikler** (sonraki turlar aynılarını kullanır; depoda değil, adım ajanının çalışma dizininde): `C:\Users\teknodeveloper\.claude\helper-state\shopfolio\a10-kd-cross\betik\` — `build_inputs.py` (girdileri kurar: `python -I build_inputs.py <doküman> template12.md prompt.txt partnote.txt <girdi klasörü>`; parça sınırları betiğin `BOUNDS` listesinde, başlık önekiyle; kapsanmayan satırı basar), `prompt.txt` (istem), `partnote.txt` (parça notu), `template12.md` (`project-playbook` deposunun `Docs/12_VALIDATION_PROTOCOL.md` şablonu, 12 553 bayt), `talimat_uret.py` + `talimat_sablon.md` (doğrulayıcı talimatları). Koşum: `bash scripts/doc-scans/cross_review_ikinci_model.sh <girdi> <çıktı> c1 p1 p2 p3 p4 p5` (K-1484).

**İstem.** Uygulama Planı turunun istemi (`11_CROSS_REVIEW.md` §1) bu dokümana uyarlandı: bağlam paragrafı (Doğrulama Protokolü; kuralı `02`–`11`'den devralır; seviyeler A–F, matris satırı = ön koşul, adımlar, beklenen sonuç, severity, kanıt, kaynak; giriş ve çıkış kriterleri, kanıt standardı, FAIL akışı, ajan modeli, proje ekleri; o dokümanlar verilmedi), yedi kriter, bulgu biçimi (`BULGU-N` — Kriter · Seviye · Yer · Alıntı · Sorun · Öneri), **bilinçli karar cümlesi (değiştirilmeden)**, ciddiyet ölçüsü paragrafı (dört sınıf: mevzuata aykırılık · para ya da hak kaybı doğuran kural boşluğu · çıkışsız akış · iki yerin çelişmesi) ve **mekanizma okuma paragrafı** (bu dokümanın mekanizmaları: satır → kaynak kuralı; severity → minimum kanıt; seviye → faz kapısı; FAIL → yeniden doğrulama; durum hücresinin yaşam döngüsü — kendi koyduğu güvenceyi sağlamaması (4); para, sipariş ya da kişisel veri akışının yanlış doğrulanması (1) ya da (2); ek koruma, izleme ve daha güçlü araç önerisi savunmada derinliktir). Parçalı çağrıların istemine aynı tek paragraflık parça notu eklenir (`partnote.txt`): verilmeyen bölüme yapılan atıf ve orada yaşayan içeriğin görünmemesi bulgu değildir.

**Komut** (Git Bash; çalışma klasörü boş): `codex exec -s read-only -C <boş klasör> --skip-git-repo-check --ephemeral --ignore-user-config --color never -m gpt-5.6-terra -c 'model_reasoning_effort="high"' -o <çıktı> - < <girdi>`. Altı çağrı paralel koştu; kullanım sınırı dolmadı, yedek modele geçilmedi (`status.txt`: `model=codex gpt-5.6-terra high`); çıktısız çağrı olmadı.

**İki koşum — doküman 830 KB, 400 KB eşiğinin üstünde (skill, Faz 1 madde 9).** Parça sınırları **başlıkla** tanımlıdır ve sonraki turlarda değişmez (kayıt: K-1561). **Başlık notu** dokümanın ilk satırından `## 1. Amaç ve kapsam` başlığına kadardır (sürüm, Aşama 1–8 park blokları, yazım durumu, yazım konvansiyonları; satır 1–171); her parça başlık notunu ve şablonu taşır. Parçalar çerçeve, matris ve ek sınırlarından geçer; **P1 iki ayrık aralıktır** (çerçeve: §1–§3, §4 girişi, §4.1, §4.2 ve §5–§11).

| Parça | Başlar → biter (dahil değil) | Bölümler | Satırlar | Girdi | Token | Süre | Sonuç |
|---|---|---|---|---|---|---|---|
| 1 | `## 1. ` → `### 4.3 ` ve `## 5. ` → `## 12. ` | §1 · §2 · §3 · §4 (giriş, 4.1, 4.2) · §5 · §6 · §7 · §8 · §9 · §10 · §11 | 172–377, 1657–1928 | 129 935 bayt | 83 955 | 92 sn | 4 bulgu |
| 2 | `### 4.3 ` → `### 4.13 ` | §4.3 – §4.12 (KP kümelerinin matrisi) | 378–987 | 278 356 bayt | 152 110 | 204 sn | 1 bulgu |
| 3 | `### 4.13 ` → `## 5. ` | §4.13 – §4.20 (teknik kesitlerin matrisi) | 988–1656 | 334 420 bayt | 130 857 | 156 sn | 1 bulgu |
| 4 | `## 12. ` → `### 12.8 ` | §12 girişi · §12.1 – §12.7 | 1929–2253 | 200 618 bayt | 107 608 | 207 sn | 1 bulgu |
| 5 | `### 12.8 ` → dosya sonu | §12.8 – §12.13 | 2254–2639 | 214 706 bayt | 97 214 | 139 sn | 1 bulgu |

Kapsanmayan satır yoktur (betiğin denetimi: `uncovered lines: none`).

**(a) Tek parça çağrı.** Girdi 847 860 bayt (775 347 karakter; aracın 1 048 576 karakterlik sınırının altında), 402 784 token, 209 sn: `SONUÇ: TEMİZ`. Tek parça girdi sınırına takılmadığı için birleşim çağrısı gerekmedi.

**Kapsam sınırı.** Parça öteki bölümleri görmez; tek parça çağrı parçalar arası okumayı yapar. Değerlendirme her iddiayı dokümanın tamamına ve ilgili yerde `02`–`11`'e karşı okudu (§3). **Web araması:** çağrı günlüklerindeki web araması satırı sayısı: tek parça 4, P1 3, P2 5, P3 3, P4 4, P5 3 (mevzuat.gov.tr, ticaret.gov.tr, resmigazete.gov.tr sitelerine kısıtlı aramalar: Mesafeli Sözleşmeler Yönetmeliği m.5, m.10, m.12, m.15; 6502 m.11; 6563; ETBİS ve kayıt saklama süresi). Yasal dayanak taşıyan iki bulgu (F5: sipariş teyidi; F7: ayıpta bedelin iadesi) resmî metne karşı doğrulayıcılarda okundu (okuma tarihi 2026-11-01): F5 için Elektronik Ticaret Aracı Hizmet Sağlayıcı ve Elektronik Ticaret Hizmet Sağlayıcılar Hakkında Yönetmelik m.9/1 (RG 29.12.2022/32058, resmigazete.gov.tr); 6563'ün resmî PDF'i okunamadı. F7 için 6502 sayılı Kanun m.11/5 (mevzuat.gov.tr).

**İki koşumun ölçümü (skill, Faz 1 madde 9).** Parçalı koşum sekiz bulgunun hepsini getirdi (P1 4, P2 1, P3 1, P4 1, P5 1); tek parça çağrı hiçbirini görmedi (`SONUÇ: TEMİZ`). Kabul/kısmi edilen altı bulgunun hepsi parçalı koşumdandır. **İki koşum sonraki turlarda da sürer.**

## 2. Ham bulgular

İkinci modelin çıktıları, satır sonu boşlukları dışında düzenlenmeden (alıntı blokları içinde).

**(a) Tek parça çağrı:**
> SONUÇ: TEMİZ

**(b) Parça 1:**
> BULGU-1
> Kriter: teknik doğruluk
> Seviye: Yüksek
> Yer: §6.1, §8.4
> Alıntı: "Task'ın kabul kriterlerine ve kontrol listesine bağlı bütün satırlar PASS."
> Sorun: Kabul edilmiş risk satırının durumu §8.4 uyarınca `KABUL EDİLMİŞ RİSK` olur, PASS olmaz. Buna rağmen §6.1 bütün ilgili satırların PASS olmasını ve validator verdict’inin tüm kriterlerde `✓` olmasını şart koşar. Böylece protokolün izin verdiği ORTA/DÜŞÜK kabul edilmiş risk, task çıkışını mekanik olarak imkânsız kılar; faz çıkışı da tüm task’ların §6.1’i sağlamasını istediğinden akış çıkışsız kalır. (3) ve (4)
> Öneri: §6.1’i, proje sahibi onaylı ve kaydı bağlı KRİTİK olmayan `KABUL EDİLMİŞ RİSK` satırlarını açık istisna olarak kabul edecek; validator verdict’inin bu durumda nasıl verileceğini de belirleyecek şekilde güncelleyin.
>
> BULGU-2
> Kriter: tutarlılık
> Seviye: Orta
> Yer: §4.2 madde 8, §7.4
> Alıntı: "Sahte bir sağlayıcıyla koşan ama sonucu sağlayıcının davranışına dayanmayan satır (ör. yük koşusunun ölçtüğü iki istek — sağlayıcıya yönlendirme işlemden sonradır, K-1515) `mock` yazar."
> Sorun: §4.2 madde 8, dış servisin sonucuna dayanmayan “öteki bütün” satırların `yok` yazacağını söyler. §7.4 ise aynı nitelikteki yük satırını `mock` yapar. Kanıt ortamı final kapısı ve yeniden doğrulama kapsamını belirlediğinden, bu çelişki satırın hangi kapı kuralına tabi olduğunun mekanik olarak belirlenmesini engeller. (4)
> Öneri: Kanıt ortamının dış servisin sonucuna mı, yoksa test kurulumunda sahte sağlayıcı bulunmasına mı göre seçileceğini tek tanımda belirleyin; §4.2 ve §7.4’ü buna göre aynı değeri zorunlu kılacak biçimde hizalayın.
>
> BULGU-3
> Kriter: tutarlılık
> Seviye: Yüksek
> Yer: §6.3, §7.4
> Alıntı: "`gerçek` olmayan bir değer final çıkışta çekirdek entegrasyon satırını PASS saymaz (§6.3)"
> Sorun: §6.3 çekirdek entegrasyonlar için yalnız `mock` PASS’ini dışlar; dolayısıyla `sandbox` “mock olmayan” ortam olarak yeterlidir. §7.4 ise `sandbox` dahil gerçek olmayan her ortamı finalde geçersiz sayar. F4’te sandbox kanıtı açıkça istenmesine rağmen finalde aynı E satırlarının geçerli olup olmadığı çelişkilidir; final kapısı yanlış geçebilir veya gereksiz biçimde bloke olabilir. (4)
> Öneri: Final için genel kuralı açıkça seçin: ya tüm çekirdek E satırlarında `gerçek` zorunlu olsun ya da `sandbox` yeterli olanları ve gerçek kanıt gerektiren istisnaları tek listede tanımlayın; §6.3 ve §7.4’ü aynı kurala bağlayın.
>
> BULGU-4
> Kriter: eksiklik
> Seviye: Yüksek
> Yer: Aşama 7 park girdileri, K-1257…K-1291, madde 8
> Alıntı: "alıcı başına 24 saatte 20 müşteri bildirimini aşan satır servise çağrı yapılmadan \"ulaşmadı\" olur ve sipariş etkilenmez."
> Sorun: Kural, sipariş alındısının gecikmeksizin elektronik iletişim aracıyla teyidi gibi zorunlu işlem bildirimleri için istisna koymadan bildirimi sessizce düşürüyor ve siparişin sürmesine izin veriyor. Bu, 6563 kapsamındaki sipariş teyidi yükümlülüğünün ihlaline ve tüketicinin siparişine ilişkin kanıt/bilgi hakkının kaybına yol açabilir. (1)
> Öneri: Oran sınırından sipariş alındısı, cayma beyanı alındısı ve diğer zorunlu işlem bildirimlerini açıkça muaf tutun; bunlar gönderilemezse siparişi hukuka uygun teyit kanalında bekleten veya güvenilir kuyruğa alan bir sonuç tanımlayın.
>
> SONUÇ: 4 BULGU

**Parça 2:**
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §4.3 VAL-B002 / VAL-B003; §4.8 VAL-A112
> Alıntı: “Firmanın seçtiği düzen yalnız hangisinin önce geldiğini değiştirir.”
> Sorun: VAL-B002 tanıtım öncelikli düzende vitrini en çok dört, VAL-B003 mağaza öncelikli düzende en çok sekiz ürünle zorunlu kılar. Düzen seçimi bu nedenle yalnız sırayı değil vitrin kapasitesini de değiştirir. Aynı uygulama bir satırı PASS yaparken diğerini FAIL yapabilir; bu, doküman içi çelişkidir.
> Öneri: VAL-A112’yi, iki düzende de iki bloğun bulunduğunu; düzenin blok sırasını ve vitrin kapasitesini (tanıtım öncelikli: 4, mağaza öncelikli: 8) belirlediğini söyleyecek şekilde düzeltin.
>
> SONUÇ: 1 BULGU

**Parça 3:**
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §4.16 VAL-E005, VAL-E067; §4.20 VAL-A980
> Alıntı: "`frame-src 'self'` ve 3D Secure sayfası engellenmez" / "Çerçeve izni yalnız ödeme sayfasındadır ve yalnız PayTR adresine verilir" / "`default-src 'self'`"
> Sorun: CSP’de `frame-src` yoksa `default-src 'self'` ona uygulanır; ayrıca VAL-E005 doğrudan yalnız `self` ister. Bu, dış alan adındaki PayTR ödeme çerçevesini engeller. Aynı doküman ise ödeme formunun PayTR’den çerçevede açılmasını zorunlu tutuyor. Sonuçta kartla ödeme akışı açılmaz; bu dokümanın kendi güvence tanımında çelişki (4) ve kullanıcı için çıkışsız ödeme akışı (3) doğurur.
> Öneri: VAL-E005 ve VAL-A980’i ödeme sayfasına özgü CSP ile hizalayın: yalnız yetkili PayTR alan ad(lar)ını içeren açık bir `frame-src` tanımlayın; diğer sayfalarda çerçeve iznini kapalı tutun. 3D yönlendirme zincirinin gereken alan adlarını da aynı satırda sınanabilir biçimde belirtin.
>
> SONUÇ: 1 BULGU

**Parça 4:**
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: Aşama 3 park girdisi K-808; §12.6 `VAL-A985`
> Alıntı: "havale hattında önce IBAN isteği; geri ödemenin süre sayacı yoktur"
> Sorun: Ayıplı malda tüketicinin sözleşmeden dönme veya bedel indirimi seçimi sonrası bedelin derhâl iadesi gerekir (6502 s. Kanun m.11/5). Buna karşın kart anahtarları kaldırılınca `VAL-A985` kart iadesini hiç göndermeyip sonucu “gerçekleşmedi” yapıyor; yalnız tüketicinin seçimine bağlı havale yolunu bırakıyor. Süre sayacının da olmaması, tüketicinin iadesinin belirsiz süreyle engellenmesine yol açar. Bu mevzuata aykırıdır.
> Öneri: Kart anahtarları, ayıp kaynaklı iade yükümlülükleri güvenle tamamlanana kadar kaldırılamamalı; kaldırma zorunluysa kart iadesi için güvenli tarihsel yetkilendirme ya da tüketiciyi bekletmeden işletilen alternatif ödeme süreci tanımlanmalıdır. Ayıp iadesine yasal “derhâl” yükümlülüğünü karşılayan ölçülebilir süre ve eskalasyon satırı eklenmelidir.
>
> SONUÇ: 1 BULGU

**Parça 5:**
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §12.10, VAL-A913
> Alıntı: "bağlantı üretilince kalan hak bir azalır ve bağlantıyla gelen gönderim ikinci hak düşürmez."
> Sorun: Bu, aynı dokümanın K-884 kuralıyla çelişir: hak, düğmeye basılmasıyla değil dosya gönderimi başladığında düşmelidir. Bağlantının üretilip istemcinin dosya isteğini hiç başlatmaması veya isteğin başlamadan kesilmesi hâlinde hak yine düşer. Bu, müşterinin ücretli dijital indirme hakkını dosyayı alamadan kaybetmesine yol açar (para/hak kaybı).
> Öneri: Hakkın, imzalı bağlantı üretilirken değil dosya isteği sunucu tarafından kabul edilip aktarım başlatıldığında atomik olarak düşmesini yazın. “Bağlantı üretildi ancak dosya isteği başlamadı” ve “istek aktarım başlamadan kesildi” senaryolarında kalan hakkın değişmediğini ayrıca sınayın.
>
> SONUÇ: 1 BULGU

## 3. Bağımsız değerlendirme

Sekiz ayrı bulgu (birleştirilen yok); yedi Opus/high doğrulayıcı (F1, F4, F5, F6, F7, F8 Yüksek, tek tek; F2 ve F3 Orta, ikisi bir doğrulayıcıda — ikisi de `§4` matrisidir; adım sınırı 7/20). Doğrulayıcı hükümleri: `C:\Users\teknodeveloper\.claude\helper-state\shopfolio\a10-kd-cross\r1\hukum_D1.md`…`hukum_D7.md` (`12_audit_hukum_denetle.py`: hüküm doğru 2 · kısmen 4 · çürük 2, çift 7, sorun 0).

| # | Parça · Yer | Konu | Hüküm | Seviye (model → doğrulayıcı) | Gerekçe ve şartname |
|---|---|---|---|---|---|
| F1 | P1 · §6.1 ↔ §8.4 | Task çıkışı "bütün satırlar PASS" der; `KABUL EDİLMİŞ RİSK` satırı olan task hiçbir kapıdan çıkamaz | **doğru** | Yüksek → Orta | §6.1'in ilk iki satırı (12:1699–1700) istisnasızdır; aynı tablonun dördüncü satırı, §4.1, §6.3 (ORTA satırlar PASS ya da kabul edilmiş risk) ve §8.5 kabul edilmiş riskle geçişi öngörür; §6.2 ve §6.3 §6.1'i varsaydığı için kapıdan çıkış yoktur ve doğrulayıcı kabul edilmiş risk kararı alamadığından (§10) verdict FAIL'de kilitlenir (4)+(3). Önceki hüküm (deep-2 2b #1) yalnız 12:1702'ye istisnayı yazmıştı; madde yeni kanıttır. Düşürme: yalnız proje sahibinin açık onayından sonra doğan nadir yol, para kaybı yok. **Uygulandı:** "İlgili maddeler PASS" satırı `PASS` ya da KRİTİK olmayan, proje sahibi onaylı, backlog kimlikli `KABUL EDİLMİŞ RİSK` satırı olarak yazıldı (onay doğrulayıcının kararı değildir); "Verdict PASS" satırı aynı istisnayla hizalandı. |
| F2 | P1 · §4.2 madde 8 ↔ §7.4 | `yok` tanımı iki yerde çelişir: yük satırı `mock` mı `yok` mu | **kısmen** | Orta → Orta | §4.2 madde 8 ve §7.4 `yok`u "sonucu dış servisin davranışına dayanmayan" diye tanımlar; 12:1791 ve VAL-F013/F014 aynı nitelikteki yük satırına `mock` verir. `mock` kalırsa §11.1 madde 4 F6'da iki yük satırını BEKLEMEDE'ye düşürür ve yük koşusu referans kurulumda koşulamadığı için satır hiç kapanmaz. K-1518 (2) ve K-1521 (6) `yok`u dış servise dayanmayan satır diye tanımlar; K-1515 (8) değeri onlara bırakır; K-1545 (1) ise `mock` der (kaynak kendi içinde çelişir). **Uygulandı (2 çift + 2 hücre):** §7.4 örneği ve tanım cümlesi `yok` (sahte sağlayıcı ön koşulda yazılır); VAL-F013 ve VAL-F014 Kanıt ortamı `mock` → `yok`. `UST:` K-1545 (1) için kayıt satırı düzeltmesi — etki yansıtmaya. |
| F3 | P2 · §4.8 VAL-A112 ↔ §4.3 VAL-B002/B003 | "Düzen yalnız sırayı değiştirir" der; iki düzenin vitrin kapasitesi farklıdır (dört / sekiz) | **kısmen** | Orta → Orta | VAL-A112 Beklenen'i "İki düzende de aynı iki blok vardır; yalnız sırası değişir" der; VAL-A025 "değişen yalnız ağırlık ve sıradır", VAL-B002/B003 vitrinin en çok dört / sekiz kaydını sınar, `02 §3.28.1` ve `04 §5.1.3`, §5.1.4 aynı. Modelin "iki blok" önerisi eksik (04 dört blok sayar). **Uygulandı (1 çift):** Beklenen, iki düzende de kurumsal blok ve ürün vitrinini, düzenin blokların sırasını ve ağırlığını (kurumsal bloğun geniş / dar hâli, vitrinin tek / iki satırı) değiştirdiğini, vitrin kapasitesinin VAL-B002/B003'te sınandığını yazar. `UST:` `10` KP-31 cümlesi (satır 180) `02 §3.28.1`'den dardır — etki yansıtmaya. |
| F4 | P1 · §6.3 ↔ §7.4 | "`gerçek` olmayan değer final çıkışta çekirdek entegrasyon satırını PASS saymaz" ↔ §6.3 yalnız `mock`u dışlar | **doğru** | Yüksek → Yüksek | §6.3 ve K-1517 (5) yalnız `mock`u dışlar, `gerçek`i yalnız K-23 DV satırlarına şart koşar; §7.4 `sandbox` dahil her gerçek-olmayan değeri finalde geçersiz sayar ve matrisin `sandbox` KRİTİK satırlarını (VAL-E003, E012, E043, E067) finalde hiç PASS alamaz kılar — oysa KRİTİK satır finalde BEKLEMEDE kalamaz (§6.3). Kaynak kazanır (K-652): düzeltme §7.4'te. **Uygulandı (1 çift):** §7.4: `mock` değeri final çıkışta çekirdek entegrasyon satırını PASS saymaz; K-23 canlı kanıt zincirine ve `08 §2.1.10` kabul kanıtına düşen DV satırlarında yalnız `gerçek` sayılır, öteki çekirdek E satırlarında `sandbox` yeterlidir. `YAN:` §11.1 "Kanıt ortamı yükselmesi" satırı (F6'da tüm E satırları) aynı kalıpla yanlış okunabilir — etki yansıtmada ele alınır. |
| F5 | P1 · §1 Aşama 7 park girdisi (madde 8) | Alıcı başına 24 saatte 20 müşteri bildirimi sınırı sipariş alındısını da düşürür; 6563 / ETA Yönetmeliği ihlali | **kısmen** | Yüksek → Orta | Ana iddia (B-1'i sınırdan muaf tutmak) **bilinçli ⚠ karardır** ve yasal bedeli kaynakta yazılıdır: ilk B-1 muaf tutulmaz, m.9/1'in ikinci kolunu firmanın "e-posta ulaşmadı" işaretli B-1'i gecikmeksizin ve sınırdan muaf olarak yeniden göndermesi karşılar (`08 §2.2.4`; K-1278, K-1271); ETA Yönetmeliği m.9/1 (RG 29.12.2022/32058): siparişin alındığı işlem ortamında ve ayrıca e-posta / SMS / telefondan biriyle gecikmeksizin bildirilir — K-1278'in okumasıyla aynı. Geçerli kalan kısım (4): bu telafinin iki dayanağı `12`'de doğrulanmıyor — sınırdan muafiyet için satır yok ve VAL-E086 aşımda siparişin satırına düşen "e-posta ulaşmadı" işaretini beklemiyor. **Uygulandı (1 çift + 1 yeni satır):** VAL-E086 Beklenen'e işaret eklendi; **VAL-E095** (E, ORTA, `mock`): panelden yeniden gönderim ve düzeltme sonrası B-1 sınırdan muaftır ama sayılır; §1 park girdisi madde 8 hizalandı. |
| F6 | P3 · §4.16 VAL-E005, VAL-E067; §4.20 VAL-A980 | CSP'de `frame-src 'self'` PayTR çerçevesini engeller | **kısmen** | Yüksek → Orta | Ana iddia yanlış: VAL-E005 `frame-src 'self'`'i "yalnız self" diye istemez, geçiş sayfasının o izinle engellenmemesini ister; kaynakta ödeme sayfası politikası `frame-src 'self' https://www.paytr.com`, öteki sayfalar `frame-src 'none'` (`08 §2.1.4`; `09 §15.2`; K-1274) ve VAL-A980'in sayfaları ödeme sayfasını içermez. Gerçek çelişki VAL-E067'nin Beklenen hücresindedir: Kural'ı ve VAL-E005 `'self'`'i isterken Beklenen çerçeve iznini "yalnız PayTR adresine" sınırlar (4). **Uygulandı (1 çift):** VAL-E067 Beklenen: ödeme sayfasında `frame-src 'self' https://www.paytr.com`, öteki sayfalarda `frame-src 'none'`. |
| F7 | P4 · §12.6 VAL-A985; K-808 park girdisi | Kart anahtarları kaldırılınca ayıp iadesi hiç gönderilmez; süre sayacı yok — 6502 m.11/5 | **çürük** | Yüksek → Düşük | İki nokta da kaynakta bilinçli kararla kapatılmış, `12` kaynağı birebir taşır: kaldırma sonrası ayıp iadesinin havale yoluna kalması K-1287'nin kaydıdır ("kaldırmayı iade yükümlülüğü bitene kadar mutlak engellemek" elendi: ayıp hakkı yıllarca sürer, PayTR bir yıldan eski işlemi iade etmez) ve süre sayacının olmaması K-716'nın ("derhal" bir gün sayısı değildir; yükümlülük `02 §12.5`'te firmaya adlandırılır). 6502 m.11/5 (mevzuat.gov.tr) iadenin aracını ya da gün sayısını belirlemez; ürün "geri ödeme gerçekleşmedi" işareti, bekleyen işler sayacı, panel uyarısı ve müşteriye IBAN alanı + B-14 ile yükümlülüğü açar — akış çıkışsız değil. İkinci model K-716 ve K-1287'yi görmedi; bilinçli karara katılmamak bulgu değildir. |
| F8 | P5 · §12.10 VAL-A913 | Hak bağlantı üretilince düşer; model "dosya gönderimi başlayınca" düşmesini ister (K-884) | **çürük** | Yüksek → Düşük | Satır kaynağın ilk cümlesini birebir taşır; `05 §7.7.4` ve K-1225 (1): hak bağlantının üretildiği anda düşer, bastığı an ile gönderimin başlaması tek adımdır ve tek hak yer; K-884'ün kuralı değişmez. İleri sürülen hak kaybı yoktur: üretilen bağlantı bir saat boyunca ek hak düşürmeden yeniden istenebilir (VAL-A914), ömrün dolmasıyla hakkın yanması K-884'te bilinçli seçilmiş kuraldır; önerilen model K-884 / K-1225'te elenen yönde kaynağı değiştirir, `12` tüketicidir (K-652). |

**Hüküm dağılımı:** 2 doğru (F1, F4) · 4 kısmen (F2, F3, F5, F6) · 2 çürük (F7, F8); seviyesi yükselen yok. Seviye: F1, F5, F6 Yüksek → Orta; F7, F8 Yüksek → Düşük (çürük); F4 Yüksek korundu.

**Sonuç sınıfı.** Kabul edilen altı bulgunun beşi Orta, biri (F4) Yüksek; hiçbiri düşük şiddetli metin farkı değildir. F2 ve F4 aynı sınıftandır (kanıt ortamı değerinin tanımı ile kapı kuralının farkı); F1 de bir kapı kuralı ile kabul edilmiş risk yolunun farkıdır. Skill Faz 4 madde 6 (hedefli mercek) iki tur gerektirir; ilk turdur, ama sınıf 2. turdan önce izlenir.

## 4. Ek bulgular

Doğrulayıcıların okurken bulduğu ve ikinci modelin listesinde olmayan, uygulanmayıp devredilen noktalar:

- **UST (K-1545 (1)):** kanıt ortamı kararı yük satırları için `mock` der, K-1518 (2) ve K-1521 (6) `yok` der; `12` tanımı K-1518'e hizalandı (F2). Karar kaydı geriye dönük düzenlenmez; K-1561 satırı iki yük satırının `yok`a çevrildiğini K-1545 (1)'e atıfla yazar.
- **UST (`10` KP-31, satır 180):** "firmanın seçtiği düzen yalnız hangisinin önce geldiğini değiştirir" cümlesi `02 §3.28.1` ("değişen yalnız ağırlık ve sıradır") ve `04 §5.1.3`, §5.1.4, §5.1.6 ile hizalanmalı (F3). `12` bunu K-24 gereği devralır; `10` düzeltilince VAL-A112'nin Kural hücresi aynı cümleyi taşır — **etki yansıtmaya devredildi** (A10-KD-etki).
- **YAN (F1):** `.claude/skills/validate/SKILL.md` Faz 2 madde 11 PASS tanımı (`12 §6.1`'in kabul edilmiş risk satırı istisnası) — L4; etki yansıtmada `validate` skill'i düzeltmesiyle birlikte ele alınır.
- **YAN (F4):** `12 §11.1` "Kanıt ortamı yükselmesi" satırı F6'da O entegrasyonun **tüm** E satırlarının yükseldiğini söyler; `sandbox` satırları (test_mode = 1 gözlemleri) `gerçek`te koşulamaz — yükselme yalnız Kanıt ortamı hücresi üst ortamı yazan satırlarda olacak biçimde etki yansıtmada hizalanır.

## 5. Kullanıcı onay checklist'i

Toplu onay modu açık (K-1504); kararlar **(toplu onayla kaydedildi)**, ⚠ taşıyan karar yok.

- [x] F1 — `12 §6.1` kabul edilmiş risk istisnası (KRİTİK olmayan, proje sahibi onaylı, backlog kimlikli) — uygulandı
- [x] F2 — kanıt ortamı `yok` tanımı yük satırlarında; VAL-F013, VAL-F014 `yok` — uygulandı
- [x] F3 — VAL-A112 Beklenen — uygulandı
- [x] F4 — `12 §7.4` final çıkışta `mock` dışlanır, K-23 DV satırlarında `gerçek` — uygulandı
- [x] F5 — VAL-E086 işareti ve VAL-E095 (yeniden gönderim sınırdan muaf, sayılır) — uygulandı
- [x] F6 — VAL-E067 Beklenen — uygulandı
- [ ] F7, F8 — çürük, uygulama yok
- [x] Sayımlar: matris 1.501 → 1.502 satır (E 94 → 95), ORTA 732 → 733, kanıt ortamı `yok` 1.242 · `mock` 198, `12 §1.2` tablosu (§4.14…§4.20 567 → 568) ve `12` başlık notu; `12_matris_denetle.py --kapsam 1-77`, `12_kapsam_hizasi.py`, `12_atif_denetle.py` hata 0

**Sonuç: TEMİZ DEĞİL — sıradaki 2. tur** (`12_CROSS_REVIEW_R2.md`, aynı betikler ve parça sınırları, `12` v0.24).
