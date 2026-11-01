# Deep Review Raporu — 12 Validation Protocol

**Tarih:** 2026-11-01 · **Hedef:** `Docs/12_VALIDATION_PROTOCOL.md` v0.20 (bulgular v0.20'de üretildi, v0.21'de işlendi; taban `main` = c120777) · **Bağlam:** `PRODUCT_DISCOVERY_STATUS.md` K-1512…K-1555; üst dokümanlar `02`–`11`; `AUDIT_REPORTS/12_AUDIT.md` · **Odak:** audit'ten sonra koşan deep review (skill Kurallar 7: kapsam ve tutarlılık katmanları yeniden tam koşmaz; audit bunları 38 mercek ve 105 madde ile geniş koştu) — audit kararlarının doğurduğu yeni tutarsızlıklar ve devreden YAN notları, ardından teknik derinlik, güvenlik, hata modu, veri akışı, ölçeklenebilirlik ve bağımlılık katmanları.

### Koşum biçimi (K-431, K-1232; `deep-review` ve `audit` skill'leri, "Koşum biçimi")

Adım iki parçaya bölündü (`INSTRUCTIONS §9` (8)). **Parça A:** paketleyici (`scripts/doc-scans/12_dr_paketle.py`) mercek paketlerini üretti, mercekler okudu, `12_dr_birlestir.py` bulguları birleştirip doğrulayıcı görev paketlerini kurdu, doğrulayıcılar hükme bağladı; her bulgunun ESKİ → YENİ ya da satır kimlikli şartnamesi hüküm dosyasında tutuldu. **Parça B:** şartnameler `scripts/doc-scans/12_dr_uygula.py` ile (ESKİ → YENİ çiftleri, hücre düzeltmeleri, yeni satırlar; önce hepsi sınanır, sorun varsa hiçbir şey yazılmaz) tek seferde uygulandı, üst doküman geri beslemesi yapıldı, kapılar koşuldu, kayıt yazıldı. Çalışma dizini: `C:/Users/teknodeveloper/.claude/helper-state/shopfolio/a10-kd-deep/` (paketler, ham bulgular, hükümler, `uygulanan.md`, `dogrulanmadi.md`).

- **Paketleyici.** Her mercek için satır numaralı dar paket (≤ 85 KB): mercek kapsamındaki `12` satırları, Kaynak hücresindeki birimlerin kaynak metni, ilgili K satırları. İki yönlü sına: bilinen bağ çözülür (VAL-A238 Kaynak → üç birim), bozulmuş girdi (olmayan kaynak) çözülmez.
- **Yirmi dört mercek paketi**, hepsi `[ozet]` Sonnet / low, beşli dalga (5 dalga).
- **Ham bulgu: 169** (AY 19 · TD 115 · GV 19 · HM 8 · VA 5 · OB 3); yinelenenler ve bir yönüyle audit'te hükme bağlanmış olanlar elenince **164 bulgu** (Yüksek 9 · Orta 102 · Düşük 53). Audit ile örtüşme adayı 30 bulgu elenmedi; doğrulayıcı kural (b) ile çürük verirdi — hiçbiri çürük çıkmadı.
- **Kademeli doğrulama — 20 doğrulayıcı, hepsi `[dogrulama]` Opus / high** (adım başına sınır; sayaç 20/20, bekçi olayı yok): D1…D9 Yüksek tek (9 bulgu) · D10, D11, D13…D20 Orta 2–4'lü (10 grup, 27 bulgu) · D12 Düşük (1 bulgu, #113: Kaynak hücresi atfı).
- **Hüküm:** doğrulanan 37 bulgudan **21 doğru · 16 kısmen · 0 çürük**. Seviyesi **yükselen bulgu yok** (`yeniden.md`: yok); dört bulgu düştü: #6 ve #7 Yüksek → Orta, #88 ve #95 Orta → Düşük (düşürülenler uygulandı).
- **Uygulama:** 46 ESKİ → YENİ çifti (ikisi — #85'in VAL-D092 çiftleri — #86'nın aynı hücreye yazan SATIR şartnamesiyle birleştirildi), 39 hücre düzeltmesi, 14 yeni satır. Matris 1.476 → 1.490 satır.
- **Doğrulanmayan: 127 bulgu** (Orta 75 · Düşük 52; Yüksek 0). Dokümana uygulanmaz; `A10-KD-deep-2a…` adımlarında doğrulanır (doğrulamasız uygulama, örnekleme ya da atlama yoktur).

### Mercek tablosu

| Mercek | Katman | Paket (bayt) | Ham bulgu |
|---|---|---|---|
| AY_01…AY_05 (audit kararlarının dokunduğu satırlar K-1549…K-1555 ve 79 YAN notu) | AY (1–2, dar) | 80.137 / 84.110 / 80.263 / 83.689 / 10.856 | 8 + 5 + 4 + 1 + 1 = 19 |
| TD_01…TD_10 (teknik derinlik; §4 ve §12 bölüm dilimleri) | 3 | 76.937 / 76.911 / 76.895 / 77.356 / 77.181 / 64.641 / 76.763 / 77.326 / 77.179 / 20.292 | 13 / 3 / 13 / 13 / 8 / 7 / 12 / 16 / 19 / 4 |
| TD_K (§5…§11 kapıları; `11` §6 faz kapıları; `skills/validate`) | 3 | 47.170 | 7 |
| GV_01…GV_03 (OWASP Top 10: 238 + 179 + 16 satır) | 4 | 84.960 / 84.711 / 12.895 | 4 / 10 / 5 |
| HM_01, HM_02 (dış servis kesintisi, yeniden deneme, idempotency, yedekten dönüş) | 5 | 81.993 / 67.197 | 6 / 2 |
| VA_01, VA_02 (para, durum ve kişisel veri akışı) | 6 | 84.943 / 19.297 | 3 / 2 |
| OB_01 (yük senaryosu, seviye E sözleşmeleri, sağlayıcı değişimi) | 7–8 | 64.569 | 3 |

AY mercekleri 12'nin audit'te değişen 248 satırını (224 VAL satırı, 24 düzyazı) ve 79 YAN notunu taradı; OWASP eşlemesi 10 maddeyi (A01 53 · A02 151 · A03 226 · A04 53 · A05 99 · A06 56 · A07 296 · A08 35 · A09 28 · A10 24 satır) kapsadı.

### Özet Skor Tablosu

| Katman | Ham (birleşik) | Hükme bağlanan | Doğru | Kısmen | Çürük | Doğrulanmadı |
|---|---|---|---|---|---|---|
| 1-2 Kapsam + tutarlılık (audit koştu; AY: audit kararlarının etrafı) | 18 | 4 | 3 | 1 | 0 | 14 |
| 3 Teknik derinlik | 113 | 23 | 12 | 11 | 0 | 90 |
| 4 Güvenlik (OWASP) | 18 | 5 | 2 | 3 | 0 | 13 |
| 5 Hata modu | 8 | 2 | 1 | 1 | 0 | 6 |
| 6 Veri akışı | 5 | 3 | 3 | 0 | 0 | 2 |
| 7-8 Ölçeklenebilirlik + bağımlılık | 2 | 0 | 0 | 0 | 0 | 2 |
| **Toplam** | **164** | **37** | **21** | **16** | **0** | **127** |

Katmanlar 1–2 "audit koştu" notuyla yalnız AY merceği (audit kararlarının doğurduğu tutarsızlıklar ve devreden YAN notları) olarak koştu; tam kapsam ve tutarlılık taraması yeniden yapılmadı. Hükme bağlanan 37, kademe kuralına göre seçildi: 9 Yüksek bulgunun hepsi, sonra puana göre Orta (27) ve bir Düşük. Yüksek bulguların yedisi güvenlik ve veri/yedek boşluğudur (#1, #3, #4, #5, #7, #8, #9 — F6 bağımlılık taraması, `Referrer-Policy`, güvenlik başlıkları, oturum sabitleme, sunucu dışı yedekten dönüş, şifre ve oturum anahtarı saklama, sağlayıcı anahtarlarının kaldırılması); kalan ikisi ölçülebilirlik (#2, #6).

### Doğrulayıcı tablosu

| Doğrulayıcı | Model | Kademe | Bulgu | Hüküm |
|---|---|---|---|---|
| D1…D9 | Opus / high | Yüksek tek | #1, #2, #3, #4, #5, #6, #7, #8, #9 | 4 doğru (#2, #5, #8, #9) · 5 kısmen (#1, #3, #4, #6, #7) |
| D10 | Opus / high | Orta 3'lü | #12, #13, #14 | 2 doğru · 1 kısmen (#12) |
| D11 | Opus / high | Orta 3'lü | #18, #19, #20 | 2 doğru · 1 kısmen (#18) |
| D12 | Opus / high | Düşük | #113 | 1 doğru |
| D13 | Opus / high | Orta 3'lü | #69, #70, #71 | 1 doğru · 2 kısmen |
| D14 | Opus / high | Orta 2'li | #72, #73 | 2 doğru |
| D15 | Opus / high | Orta 2'li | #74, #75 | 2 doğru |
| D16 | Opus / high | Orta 3'lü | #78, #79, #80 | 3 kısmen |
| D17 | Opus / high | Orta 2'li | #81, #82 | 1 doğru · 1 kısmen (#81) |
| D18 | Opus / high | Orta 3'lü | #83, #84, #85 | 2 doğru · 1 kısmen (#83) |
| D19 | Opus / high | Orta 3'lü | #86, #87, #88 | 3 doğru |
| D20 | Opus / high | Orta 3'lü | #93, #94, #95 | 1 doğru · 2 kısmen |
| **Toplam** | 20 doğrulayıcı | | 37 bulgu | **21 doğru · 16 kısmen · 0 çürük** |

Doğrulayıcı sayacı 20/20; hiçbir çağrı reddedilmedi, yeniden doğrulama yok.

### Bulgu → hüküm → uygulama

Katman AY = audit kararlarının etrafı. "Uygulama" sütunu `uygulanan.md` satır sayısıdır (ESKİ → YENİ çifti, hücre ya da yeni satır).

| Bulgu | Katman | Seviye (ham → hüküm) | Yer | Hüküm | Bulgu (özet) | Uygulama |
|---|---|---|---|---|---|---|
| #1 | 4 | Yüksek | VAL-F001,VAL-F014 | kısmen | F6 gate'inde yapılacağı söylenen "güvenlik ve bağımlılık taraması" için hiçbir matri… | 2 düzeltme |
| #2 | 3 | Yüksek | VAL-D004 | doğru | "Kart numarasi ve guvenlik kodu hicbir istekte/tabloda/kayit satirinda yer almaz" satirinin on k… | 2 düzeltme |
| #3 | 4 | Yüksek | VAL-A054,VAL-A604,VAL-A773 | kısmen | Erisim anahtari tasiyan sayfalarin `Referrer-Policy: no-referrer` verdigini ve baska siteye gide… | 1 düzeltme |
| #4 | 4 | Yüksek | VAL-A685,VAL-A773,VAL-E070 | kısmen | 05 MK-53 guvenlik basliklarinin (HSTS, tur tahmini yasagi, Referrer sinirli, tarayici izinleri, … | 2 düzeltme |
| #5 | 4 | Yüksek | VAL-A562,VAL-A929,VAL-A931 | doğru | Oturum sabitlemesine karsi kural (her giris yeni anahtar uretir, tarayicinin getirdigi deger otu… | 1 düzeltme |
| #6 | 3 | Yüksek → Orta | VAL-A590,VAL-A602 | kısmen | "Yanıt süresi dağılımı ayırt edilemez" ve "süre sınıfı aynıdır" ölçülebilir değ… | 2 düzeltme |
| #7 | 5 | Yüksek → Orta | VAL-A808,VAL-A813,VAL-A939,VAL-F001 | kısmen | Yedekten donus ve sunucu diskinin kalici kaybi (kayip penceresi) icin hicbir satir yok; 12 hem F… | 4 düzeltme |
| #8 | 4 | Yüksek | VAL-A927,VAL-D116 | doğru | Sifrenin tuzlu PBKDF2 ozet olarak saklandigi ve oturum anahtarinin yalniz ozetinin kayitta durdu… | 2 düzeltme |
| #9 | 5 | Yüksek | VAL-A213,VAL-A214,VAL-A215,VAL-A829,VAL-C093 | doğru | Saglayicinin anahtarlari kaldirilirken geri odemesi dogabilecek kart siparisi kalmama sarti ve a… | 2 düzeltme |
| #12 | AY | Orta | VAL-A978,VAL-A979,VAL-B007 | kısmen | Giriste birlesen sepette stoktan fazla adet kalirsa kalemin sepette mi kaldigi yoksa eklenmedigi… | KARAR (K-1557) + VAL-A989 |
| #13 | 3 | Orta | VAL-D002 | doğru | "Zaman ileri alinir" ne kadar ilerletilecegini soylemiyor; "sepeti bosaltan zamanli is yoktur" o… | 1 düzeltme |
| #14 | 3 | Orta | VAL-A051,VAL-A052 | doğru | Kural uc ayri surum degisikligini (ozet, Ön Bilgilendirme Formu, Mesafeli Satis Sozlesmesi) kap… | 3 düzeltme |
| #18 | 3 | Orta | VAL-A067,VAL-A069,VAL-A074,VAL-A075 | kısmen | "On dort gun icinde geri odenir" kurallari icin on kosul zaman ilerletme/vade kontrolunu tarif e… | 4 düzeltme |
| #19 | AY | Orta | VAL-A068,VAL-A069,VAL-A077,VAL-A078,VAL-A079,VAL-A217,VAL-A230,VAL-A238,VAL-A251 | doğru | IBAN'in geri odemeye baglanmasi (iptal, cayma, sonradan yeniden alma) ORTA, ayni IBAN hatti bask… | 3 düzeltme |
| #20 | AY | Orta | VAL-A069,VAL-A070,VAL-A080 | doğru | A070 K-1555'te "para/yasal sure" gerekcesiyle KRITIK'e yukseltildi, ama Beklenen yalniz "sipari… | 1 düzeltme |
| #69 | 3 | Orta | VAL-C079,VAL-C087,VAL-C090 | kısmen | "iki degisim tek islemdedir", "ayni anda" gibi atomiklik beklentisi icin gozlem yontemi yok (yal… | 2 düzeltme |
| #70 | 3 | Orta | VAL-C082,VAL-C106 | doğru | Sure/zaman bagimli beklenen sonuclar icin on kosulda zaman ilerletme (test saati) yok; C082'de "… | 2 düzeltme |
| #71 | 3 | Orta | VAL-C088 | kısmen | "sorgu yinelenir" icin aralik/sayi/bekleme siniri yok; ayrica sonuc gelince "Ö1 ya da S3 + Ö2"… | 1 düzeltme |
| #72 | 3 | Orta | VAL-C095,VAL-C097,VAL-C101 | doğru | "Geri yonlu her istek", "beyaz liste disi her gecis" ve "her durumdan cikis" sayilmis bir kume o… | 3 düzeltme |
| #73 | 3 | Orta | VAL-C086,VAL-C103,VAL-C105 | doğru | Kural "cayma penceresi ve ayiplik suresi yeni teslim tarihine kadar islemez" der, beklenen sonuc… | 1 düzeltme |
| #74 | 6 | Orta | VAL-A798 | doğru | Beklenen sonuc "indirimli varyantta indirimli fiyattan hesaplanir" der, ama on kosuldaki alti va… | 2 düzeltme |
| #75 | 3 | Orta | VAL-A807 | doğru | Tek satirda uc ayri senaryo (yonetici azaltmasi, ayipta sozlesmeden donme, tutar bazli 20,00 TL … | 1 düzeltme |
| #78 | 3 | Orta | VAL-A816 | kısmen | Ön koşul iki farklı müşteriyi yazar ama aynı varyanta/kilide yarıştıklarını söylemez… | 1 düzeltme |
| #79 | 3 | Orta | VAL-A820,VAL-A831,VAL-A850,VAL-C109,VAL-C110 | kısmen | Kural/Beklenen sonuçta yer alan kollar ön koşulda kurulmamış, dolayısıyla uygulayıcı on… | 5 düzeltme |
| #80 | 3 | Orta | VAL-A826 | kısmen | Nabız yaşının hangi anda ölçüldüğü (iptal işinin çalıştığı an) yazılmamış; z… | 1 düzeltme |
| #81 | 3 | Orta | VAL-A829 | kısmen | Tek satırda üç ayrı senaryo (gönderilmiş iade, kaydı yazılmadan düşen deneme, e-posta)… | 5 düzeltme |
| #82 | 3 | Orta | VAL-A834 | doğru | "Eşiğe yakın disk" nasıl hazırlanır belli değil, eşik değeri ve yükleme boyları sayı… | 6 düzeltme |
| #83 | 3 | Orta | VAL-A838 | kısmen | Güvenlik satırı: "arama yolu değiştirilmiş oturumda sabit yolla çalışır" nasıl gözle… | 3 düzeltme |
| #84 | 6 | Orta | VAL-D089,VAL-D090,VAL-D091 | doğru | D090 gonderim kaydinda "uc yil dolunca" kisisel degerlerin silinmesini ister ama uc yilin baslan… | 3 düzeltme |
| #85 | 3 | Orta | VAL-D090,VAL-D091,VAL-D092,VAL-D120 | doğru | Bölüm kuralı (12:2201) kenar için "bir gün önce ve dolduğu gün" ister; bu satırlar yaln… | 5 düzeltme |
| #86 | 6 | Orta | VAL-D092,VAL-D094 | doğru | Iki saklama satiri da "yeniden acilip kapatilan talepte son kapatilis / son degisiklik" baslangi… | 4 düzeltme |
| #87 | 3 | Orta | VAL-D093 | doğru | "Öteki üç türün süresi K-951 ile değişmemiştir" için ön koşulda kayıt, test saati y… | 3 düzeltme |
| #88 | 3 | Orta → Düşük | VAL-D095 | doğru | Kuralda "`DisposalRecordType` yirmi dört değer taşır" var ama beklenen sonuç bunu sınamaz;… | 1 düzeltme |
| #93 | 3 | Orta | VAL-A887 | kısmen | Ön koşul yalnız bir adedi iptal edilmiş sipariş kuruyor; beklenen sonuç "kalan adetlerin s… | 2 düzeltme |
| #94 | 3 | Orta | VAL-D103,VAL-D107,VAL-D112,VAL-D113 | doğru | Dört satır "aynı işlemde yazılır / geçersizleşir / silinir" atomikliğini kural yapıyor… | 8 düzeltme |
| #95 | 3 | Orta → Düşük | VAL-A902,VAL-D104 | kısmen | A902 kuralı "stoğa ekleme teslim alınan adet kadardır" der ama beklenen yalnız reddedilen a… | 3 düzeltme |
| #113 | AY | Düşük | VAL-A072,VAL-A080,VAL-C012,VAL-C015 | doğru | Kaynak hucresi "10 KP-22 · 02 §7.4" (Iade ve geri odeme) gosteriyor; bu satirlarin kurallari c… | 4 düzeltme |

### Yeni VAL satırları ve kimlik çakışması

14 yeni satır eklendi: VAL-A980…A989 (10), VAL-D121, D122 (2), VAL-F015, F016 (2).

| Kimlik | Bulgu | Konu | Yer |
|---|---|---|---|
| VAL-A980 | #4 | Her yanıtta güvenlik başlıkları: CSP (nonce, `unsafe-inline` yok), `nosniff`, izinler | §4.20, VAL-A773'ün ardı |
| VAL-A981 | #3 | `Referrer-Policy` (anahtarlı sayfalar `no-referrer`, öteki `strict-origin-when-cross-origin`) | VAL-A980'in ardı |
| VAL-A982 | #4 | HSTS ters vekilde (bir yıl, alt alan adı ve ön yükleme yok); uygulama HSTS yazmaz | VAL-A981'in ardı |
| VAL-A983 | #5 | Oturum sabitleme: her giriş yeni anahtar, getirilen değer oturuma dönüşmez | §12.10, VAL-A931'in ardı |
| VAL-A984, A985 | #9 | Durum komutu anahtar kaldırma şartlarını sayar; kaldırıldıktan sonra kart geri ödemesi istek yapmadan reddedilir | §12.6, VAL-A831 ve VAL-A829'un ardı |
| VAL-A986 | #14 | Onay adımı açıkken aydınlatma metni ya da çerez politikası sürümü artarsa sipariş durmaz | §4.4 KP-14, VAL-A052'nin ardı |
| VAL-A987 | #81 | Geri ödeme kaydı yazılmadan düşen kart iadesi denemesi istek göndermemiştir ve yeniden denenir | §12.6, VAL-A985'in ardı |
| VAL-A988 | #82 | Yükleme, disk eşiğini aşacaksa ya da eşiğe tam ulaşacaksa kabulde reddedilir | §12.6, VAL-A834'ün ardı |
| VAL-A989 | #12 (KARAR K-1557) | Girişte birleşen sepette stoğu aşan kalem mesajla sepette kalır | §4.4 KP-10, VAL-A978'in ardı |
| VAL-D121, D122 | #8 | Şifre geri çevrilebilir saklanmaz (PBKDF2 özeti); oturum anahtarı veritabanında açık durmaz | §12.10, VAL-D116'nın ardı |
| VAL-F015 | #1 | F6 kapısı: bağımlılık taraması bulgusuz ve gömülü sır yok | §12.1, VAL-F012'nin ardı |
| VAL-F016 | #7 | Kurtarma provası sunucunun dışındaki yedekten döner | §12.11, VAL-A975'in ardı |

**Çakışma.** D3, D4, D5, D9 ve D17 hepsi VAL-A980'i önermişti; D3 (Referrer) ile D4 (Referrer + CSP + HSTS) aynı `Referrer-Policy` boşluğuna iki satır yazıyordu. D3'ün satırı tutuldu (UC-34, UC-52, UC-58, UC-75, UC-85, UC-108 ve hata yanıtı ile statik dosyayı da sınar; 09 §15.2 madde 3 ile aynı değerler), D4'ün Referrer satırı bırakıldı; D4'ün CSP ve HSTS satırları kaldı. Kimlikler o seviyenin son numarasından (A979, D120, F014) sırayla verildi. `12 §1.2` sayımı (toplam, seviye, severity, tür etiketi, kanıt ortamı, durum; KP-10 satır sayısı) ve `12:180` aynı PR'da hizalandı. Park tablosunun hücreleri bu adımda değişmedi (yeni satırların hiçbiri park maddesi karşılığı değil; `P4.n` hücre düzeltmesi yok).

**Matris sayımı (betik çıktısı).** 1.476 → 1.490 satır; Seviye A 979 → 989 · D 120 → 122 · F 14 → 16 (B 161, C 114, E 88 aynı); Severity KRİTİK 614 → 628 · ORTA 730 → 730 · DÜŞÜK 132 (üç satır ORTA → KRİTİK: #19 VAL-A068, A077, A078; on bir yeni KRİTİK, üç yeni ORTA: A986, A988, A989); `[güvenlik]` 39 → 46; kanıt ortamı `yok` 1.226 → 1.237 · `mock` 189 → 191 · `gerçek` 38 → 39.

### Devreden YAN notlarının hükmü (audit'ten)

| YAN | Sonuç |
|---|---|
| (1) VAL-A080, C012, C015, A072 Kaynak hücresi `02 §7.4` yerine `02 §7.3.x` | #113 (D12) doğru; dört Kaynak hücresi düzeltildi |
| (2) VAL-A068/A078 (ORTA) ↔ eşdeğeri VAL-A238/A069 (KRİTİK) | #19 (D11) doğru; severity hizalandı |
| (3) Birleşmede stok kolunun sonucu kaynakta belirsiz | #12 (D10) kısmen; KARAR K-1557 (⚠) + `02 §3.16.8` geri besleme + VAL-A989 |
| (4) VAL-A147, A177, A178, A730 (oturum/yetki davranışı ORTA) | Yeniden bakılmadı; AY merceğinde çelişki bulunmadı |
| (5) 2a/2b/2c "ayrı madde" notları | AY mercekleri `uygulanan.md` ile karşılaştırdı; karşılanmamışlar bulgu oldu |

### ⚠ ayrımı

- **KARAR (⚠, öneriyle, toplu onayla kaydedildi) — K-1557:** birleşmede stoğu aşan kalem kaybolmaz; "Bu adette stok yok" mesajıyla sepette kalır ve adet düşürülene kadar siparişe girmez (`02 §3.16.8`; K-124, K-150 (2), K-152 aynı yönü gösterir). Ürün davranışı değil, mevcut kuralın birleşme kolunun açık yazılmasıdır; seçenekler yasal ve ticari olarak yakın olduğundan sorulmadı.
- **KARAR (⚠ değil):** #6 — VAL-A590 ölçüm örneklem sayısı kaynakta yok; her durum için VAL-F013'teki "en az 200 tekrar" düzenine (K-1515) atıfla aynı tekrar sayısı belirlendi.
- "Yine de sorulur" ölçütünü karşılayan madde yok; hiçbir madde uygulanmadan bırakılmadı.

### Üst doküman geri beslemesi (K-652, aynı PR)

| Doküman | Yer | Değişiklik | Sürüm |
|---|---|---|---|
| `02` | §3.16.3, §3.16.8 | Birleşmede stoğu aşan kalem mesajla sepette kalır (K-1557) | v0.109 → v0.110 |
| `02` | §10.6.2 | Satış özetinin iptal ve iade sayısı sipariş sayısıdır; kısmi iptal ya da iade siparişi bir kezden fazla saymaz | v0.110 |
| `05` | §7.7.1 | Yükleme "aşacaksa ya da eşiğe tam ulaşacaksa" reddedilir (§10.1.8 ile aynı) | v0.47 → v0.48 |
| `09` | §15.2 madde 3 | `no-referrer` kümesi erişim anahtarlı sipariş sayfasını (UC-34) da kapsar | v0.28 → v0.29 |
| `11` | §6.2 F6 | Güvenlik taramasının kapsamı atıfla yazıldı (`05 §8.8.4`, K-1512 (3)) | v0.61 → v0.62 |

Hiçbiri yeni kural değildir; kaynak kazandı. `00`, `CLAUDE.md` ve `SETUP.md` değişikliği önerilmedi. Kaynak satırlarına K-1556 (ve `02 §3.16.8` için K-1557) eklendi.

### Kapılar

- `12_kapsam_hizasi.py`: hata 0 (KP 77, KD 64, ÖK 12, SK 10; boşluk 0).
- `12_matris_denetle.py --kapsam 1-77`: 1.490 satır, kimlik tekil, 77 KP kapsanan, eksik 0; hata 0.
- `12_atif_denetle.py .`: 2.772 belge numaralı atıf, hedefi bulunamayan 0; hata 0.
- `12_audit_hukum_denetle.py a10-kd-deep`: uygulamadan önce sorun 0, bulgu sayıları paketlerle eşit (uygulamadan sonra ESKİ metinler bulunmaz; beklenen).

### Doğrulanmayanlar (A10-KD-deep-2a…)

127 bulgu (Orta 75 · Düşük 52; Yüksek 0) 20'lik doğrulayıcı sınırına sığmadı: katman 3 → 90 · katman 4 → 13 · katman 5 → 6 · katman 6 → 2 · katman 7 → 2 · AY → 14. Liste ve planı `a10-kd-deep/dogrulanmadi.md` ve `plan.md` dosyalarındadır (53 grup); dokümana uygulanmadı. Sıradaki adımlar `A10-KD-deep-2a…` (adım başına en çok 20 doğrulayıcı), sonra cross-review.

### Sınırlar

- Mercekler Sonnet / low okudu; bir merceğin okuma kapsamı paketin ≤ 85 KB sınırıyla çizildi. TD_01…TD_09 bölüm dilimleridir; dilim kenarında kalan ilişkiler (iki dilime yayılan satır grupları) AY merceği dışında çapraz okunmadı.
- Hükme bağlanan 37 bulgu, 164'ün yaklaşık beşte biridir; kalanı doğrulanmadığı için uygulanmadı. Doğrulanmayan bulguların seviyeleri merceğin kendi ölçüsüdür, doğrulayıcı düşürebilir.
- Yeni güvenlik satırları (VAL-A980…A983, D121, D122, F015) `[güvenlik]` KRİTİK'tir ve kanıt ortamı `yok`/`mock`; gerçek kanıtın nasıl alınacağı Implementation döneminde ilgili task'ın işidir.
- Hüküm dosyalarında "satır kimlikli şartname" sayısı (Parça A devir notunda 77: 67 SATIR + 10 YENI_SATIR) Parça B'de betikle yeniden sayıldı: 39 hücre düzeltmesi ve 14 yeni satır (13 hükümde yazılı + #12'nin YAN'ında bekleyen VAL-A989; üst doküman kapandıktan sonra eklendi). ESKİ → YENİ çifti sayısı 46'dır.
- Karar kaydı K-1556 koşumu, K-1557 ⚠ kararı taşır. Toplu onay modu açıktır (K-1504).

### Doğrulanmamış bulgular — A10-KD-deep-2a

**Tarih:** 2026-11-01 · **Girdi:** `a10-kd-deep/dogrulanmadi.md`'deki 127 bulgu (Orta 75 · Düşük 52); `12_dr_grupla2.py` ile 34 grup, iki adım (2a: 20 doğrulayıcı, 79 madde; 2b: 14 doğrulayıcı, 48 madde). Parça A hükümleri `a10-kd-deep-2/2a/hukum_1…20.md`.

#### Doğrulayıcılar (20; hepsi `[dogrulama]` Opus / high, ön planda; adım sınırı 20/20)

| Doğrulayıcı | Kademe | Bulgu | Hüküm |
|---|---|---|---|
| D1 | Orta 3'lü | #106, #103, #104 | 1 doğru (#106) · 2 kısmen |
| D2 | Orta 3'lü | #96, #97, #98 | 1 doğru (#98) · 1 kısmen (#96) · 1 çürük (#97) |
| D3 | Orta 2'li | #76, #77 | 2 doğru |
| D4 | Orta 3'lü | #15, #16, #17 | 1 doğru (#16) · 1 kısmen (#15) · 1 çürük (#17) |
| D5 | Orta 3'lü | #21, #22, #23 | 3 kısmen |
| D6 | Orta 4'lü | #89, #90, #91, #92 | 1 doğru (#89) · 3 kısmen |
| D7 | Düşük 6'lı | #148, #149, #150, #147, #156, #157 | 4 doğru · 2 kısmen (#150, #157) |
| D8 | Düşük 6'lı | #141, #142, #143, #144, #145, #146 | 3 doğru (#143, #145, #146) · 2 kısmen (#142, #144) · 1 çürük (#141) |
| D9 | Orta 3'lü | #31, #32, #33 | 3 kısmen |
| D10 | Orta 3'lü | #34, #35, #36 | 1 doğru (#34) · 2 kısmen |
| D11 | Orta 3'lü | #37, #38, #39 | 3 doğru |
| D12 | Düşük 7'li | #122…#128 | 6 doğru · 1 kısmen (#125) |
| D13 | Orta 4'lü | #41, #42, #43, #44 | 2 doğru (#42, #43) · 2 kısmen (#41, #44) |
| D14 | Orta 4'lü | #45, #46, #47, #49 | 3 doğru · 1 kısmen (#49) |
| D15 | Orta 3'lü | #50, #51, #105 | 1 doğru (#51) · 2 kısmen |
| D16 | Orta 4'lü | #54, #56, #57, #58 | 3 doğru · 1 kısmen (#57) |
| D17 | Orta 4'lü | #59, #62, #63, #64 | 1 doğru (#62) · 3 kısmen |
| D18 | Orta 4'lü | #65, #66, #67, #68 | 3 doğru · 1 kısmen (#66) |
| D19 | Düşük 6'lı | #114, #135…#139 | 6 doğru |
| D20 | Düşük 4'lü (çerçeve havuzu) | #112, #163, #140, #164 | 2 doğru (#112, #140) · 2 kısmen (#163, #164) |

Toplam 79 bulgu: **44 doğru · 32 kısmen · 3 çürük**. Çürük üçü "zaten karşılandı" (önceki adımların uygulaması). Seviyesi yükselen yok; dört bulgu düştü (#44, #90, #91, #98 Orta → Düşük). Hükümde seviye sütunu: 42 Orta · 37 Düşük.

#### Uygulama

`12_dr_uygula2.py` (önce kuru koşu, sorun 0): 83 ESKİ → YENİ çifti (hükümlerde 84; #59'un `[güvenlik]` sayım çifti betikle yeniden sayıldı, aynı hücreye yazan ikinci çift #58'in hücre düzeltmesiyle karşılandı), 49 hücre düzeltmesi, 8 yeni satır. Hükümlerde sayılan 61 satır kimlikli şartnamenin 51'i SATIR, 10'u YENI_SATIR idi; ikişi boş (`SATIR:` / `YENI_SATIR:` değersiz) olduğundan uygulanan 49 + 8'dir. Uygulanan liste `a10-kd-deep-2/2a/uygulanan.md`.

| Yeni satır | Konum (sonrasına) | Bulgu |
|---|---|---|
| VAL-C115 | VAL-C088 | #106 |
| VAL-E089 | VAL-E061 | #43 |
| VAL-E090 | VAL-E056 | #46 |
| VAL-E091 | VAL-E083 | #51 |
| VAL-E092 | VAL-E086 | #51 |
| VAL-E093 | VAL-E056 | #105 |
| VAL-A990 | VAL-A829 | #105 |
| VAL-D123 | VAL-A703 | #137 |

`[güvenlik]` etiketi (#58, #59, #63 ve devredenler) 16 satıra eklendi: 46 → 62 (`12 §1.2`'de betikle yeniden sayıldı). Matris 1.490 → 1.498 satır (A 990 · B 161 · C 115 · D 123 · E 93 · F 16); KRİTİK 634 · ORTA 732 · DÜŞÜK 132; kanıt ortamı `yok` 1.238 · `mock` 198 · `gerçek` 39 · `sandbox` 23; `12 §1.2` bölüm satır sayıları (§4.14…§4.20: 565; §12: 356; toplam 1.498) hizalandı.

#### KARAR (beş madde; ⚠ yok; toplu onayla kaydedildi, K-1558)

1. **#103** — Yük koşusunda ayrı istemci adresi yük makinesinde üretilir; ters vekilin güven ayarı üretimdeki gibi kalır. Ziyaretçi başına istek sayısı, karışım ve H-3 dağılışı sabitlenmez, T601 betiği seçer ve gerçekleşen değer rapora yazılır.
2. **#15** — Sipariş numarası ön koşulunda adet: en az 20 art arda verilmiş sipariş (test düzeni kararı).
3. **#125** — Kısa alanı olmayan sayfada açıklama metnin düz hâlinin başından 200 karakterle (P-28) alınır, sözcük ortasında kesilmez (`02 §3.30.5` (2) ve VAL-A375).
4. **#41** — Ödeme çerçevesinde çerçeve içi kaydırma kabul edilir (`08 §2.1.4`, DV-5; `04` 5.14.6).
5. **#164** — FAIL eskalasyonunu yönetici yardımcı düzeninde yardımcı, düzen kapalıyken ana oturum açar (`INSTRUCTIONS.md` §3.3).

#### Üst doküman geri beslemesi (K-652, aynı PR)

| Doküman | Yer | Değişiklik | Sürüm |
|---|---|---|---|
| `02` | §3.30.5 (2) | Açıklama sınırı 200 karakter, sözcük ortasında kesilmez (#125) | v0.110 → v0.111 |
| `04` | 2.7.3; 5.14.6 | Sepetin durduğu yalnız müşteri tarafında (#150); çerçeve yüksekliği kuralı (#41) | v0.51 → v0.52 |
| `05` | §6.5.6 | Panelin liste dönüşünde düşüş yeri listenin kökü (#63) | v0.48 → v0.49 |
| `08` | §2.1.4 madde 2; §4.1 DV-5 | Çerçeve içi kaydırma kabulü (#41) | v0.20 → v0.21 |
| `.claude/INSTRUCTIONS.md` (L3) | §3.3 | "Tamamlanmış" CI run'ı (#163); eskalasyon kuralı (#164) | — |
| `.claude/skills/validate/SKILL.md` (L4) | Adım 0, 8a, FAIL durumunda | `--status completed`; tam dal adı; tur sayacı ve eskalasyon (#163, #164) | — |

`.claude/INSTRUCTIONS.md` ve `skills/validate` GUARDRAILS §2'deki onay kapısında değildir; uygulandı. `00`, `CLAUDE.md` ve `SETUP.md` değişikliği önerilmedi.

**Üst dokümanda açık kalan boşluklar (2a'da uygulanmadı; 2b'de beşi de karara bağlanıp uygulandı, K-1559 ve K-1560 — bkz. "Doğrulanmamış bulgular — A10-KD-deep-2b"):** `02 §3.31.2` (#39: ret sebebinin kime ve nerede söylendiği), `08 §2.1.6` (#46: tutarı tutmayan son sorgu yanıtından sonra siparişin ve sorgu satırının akıbeti), `04 §5.22.11` (#150: Google'a ulaşılamadığında ekran ve mesaj), `02 §3.30.5` (1) (#142: hesap ve panel sekme başlığı), `02 §4.1.4` (#144: bitiş gününün son saniyesi). 12'deki satırlar bugünkü kaynağa göre yazıldı; boşluklar kapanınca ilgili satırlar sayıyla sıkılaştırılır.

#### Kapılar

- `12_kapsam_hizasi.py`: hata 0.
- `12_matris_denetle.py --kapsam 1-77`: 1.498 satır, kimlik tekil, 77 KP kapsanan, eksik 0; hata 0.
- `12_atif_denetle.py .`: hedefi bulunamayan atıf 0; hata 0.
- `12_audit_hukum_denetle.py a10-kd-deep-2/2a`: Parça A'da sorun 0.

#### İlerleme

**79/127** bulgu hükme bağlandı; kalan 48 bulgu (2b: 14 doğrulayıcı; `a10-kd-deep-2/plan.md`). Sıradaki adım A10-KD-deep-2b, sonra cross-review. Yükselen yok (`yeniden.md` boş).


### Doğrulanmamış bulgular — A10-KD-deep-2b

**Tarih:** 2026-11-01 · **Girdi:** 2a'dan kalan 48 bulgu (Orta 25 · Düşük 23; `a10-kd-deep-2/plan.md`) ve 2a'nın açık bıraktığı beş üst doküman boşluğu. Parça A hükümleri `a10-kd-deep-2/2b/hukum_1…16.md`. Bu, deep review'un son alt adımıdır: yükselen bulgu olmadığı için 2r açılmadı.

#### Doğrulayıcılar (16; hepsi `[dogrulama]` Opus / high, ön planda; adım sınırı 16/20)

| Doğrulayıcı | Kademe | Bulgu | Hüküm |
|---|---|---|---|
| D1 | Orta 3'lü | #10, #109, #111 | 2 doğru · 1 kısmen (#10) |
| D2 | Orta 3'lü | #11, #26, #27 | 2 doğru (#11, #27) · 1 kısmen (#26) |
| D3 | Orta 2'li | #24, #25 | 1 kısmen (#25) · 1 çürük (#24) |
| D4 | Düşük 7'li | #115…#121 | 6 doğru · 1 kısmen (#115) |
| D5 | Orta 3'lü | #28, #29, #30 | 1 doğru (#29) · 2 kısmen (#28, #30) |
| D6 | Orta 2'li | #40, #53 | 1 doğru (#53) · 1 kısmen (#40) |
| D7 | Düşük 6'lı | #129…#134 | 5 doğru · 1 kısmen (#130) |
| D8 | Orta 2'li | #48, #52 | 1 doğru (#52) · 1 çürük (#48) |
| D9 | Orta 3'lü | #55, #60, #61 | 2 doğru · 1 kısmen (#61) |
| D10 | Orta 3'lü | #110, #107, #108 | 3 kısmen |
| D11 | Orta 2'li | #99, #100 | 1 doğru (#99) · 1 kısmen (#100) |
| D12 | Düşük 5'li | #151…#155 | 2 doğru (#152, #154) · 2 kısmen (#151, #153) · 1 çürük (#155) |
| D13 | Orta 2'li | #101, #102 | 2 doğru |
| D14 | Düşük 5'li | #158…#162 | 4 doğru · 1 çürük (#159) |
| D15 | Boşluk (`02`) | #39, #142, #144 | 3 kısmen (karar önerisi) |
| D16 | Boşluk (`08`, `04`) | #46, #150 | 2 kısmen (karar önerisi) |

D1…D14: 48 bulgu, **29 doğru · 15 kısmen · 4 çürük**. Çürük dördü "zaten karşılandı" (#24, #48, #155, #159). Seviyesi yükselen yok; üç bulgu düştü (#10, #24, #40 Orta → Düşük). Hükümde seviye sütunu: 22 Orta · 26 Düşük. D15 ve D16 yeni bulgu değil, 2a'nın beş boşluğunun karar önerisidir; hiçbiri "yine de sorulur" ölçütünü karşılamadı.

#### Uygulama

`12_dr_uygula2.py` (önce kuru koşu, sorun 0): 48 ESKİ → YENİ çifti, 26 hücre düzeltmesi, 3 yeni satır (hükümlerdeki 29 satır kimlikli şartnamenin tamamı). Uygulanan liste `a10-kd-deep-2/2b/uygulanan.md`; 44 bulgu uygulandı, 4 çürük uygulanmadı. Ayrıca VAL-A190 seviyesi Orta → Kritik yapıldı (KARAR) ve VAL-D039'un listesine ürün formundaki P-6, P-9, P-44 eklendi.

| Yeni satır | Konum (sonrasına) | Bulgu |
|---|---|---|
| VAL-C116 | VAL-C029 | #25 |
| VAL-E094 | VAL-E089 | #131 |
| VAL-A991 | VAL-A981 | #101 |

Matris 1.498 → 1.501 satır (A 991 · B 161 · C 116 · D 123 · E 94 · F 16); KRİTİK 637 · ORTA 732 · DÜŞÜK 132; `[güvenlik]` 62 → 63; kanıt ortamı `yok` 1.240 · `mock` 199 · `gerçek` 39 · `sandbox` 23; `12 §1.2` sayımı ve bölüm satır sayıları (§4.7: 38; KP kümesi 578; §4.14…§4.20: 567; §12: 356) aynı PR'da hizalandı.

#### KARAR (toplu onayla kaydedildi; K-1559, K-1560)

1. **#39** — Sistemin kendiliğinden geçişinin (ödeme süresinin dolması, sağlayıcı bildirimi, son sorgu) reddinde sebep, siparişin iç kimliği ve sebebin koduyla uygulama kaydına yazılır; bildirim üretilmez. Müşterinin işleminin reddini sipariş sayfası, yöneticininkini panel söyler. ⚠ değil.
2. **#142** — Arama motoruna kapalı ekranların (sepet, ödeme adımı, sipariş takibi, hesap, panel) sekme başlığı ekranın `04 §4` envanterindeki adı ve marka adıdır. ⚠ değil.
3. **#144** — Tarih aralığının bitiş günü bütünüyle dahildir; aralık bitiş tarihinden sonraki günün 00.00'ında biter ve indirimin bitişi fiyat geçmişine o anla yazılır (`09 §7.1` (3) ile hizalı). ⚠ değil.
4. **#46** — Başarısı uygulanmayan (tutarı tutmayan) `success` yanıtı "yanıt alınamadı" gibi işlenir: sipariş iptal edilmez, "ödeme sonucu alınamadı" işaretiyle Bekliyor'da kalır, sorgu yinelenir, sonra "başarısız" ve uyarı; sipariş panelden elle kapatılır. **⚠** (K-1560).
5. **#150** — Google ile giriş tamamlanmazsa giriş ekranı formun başında kalıcı bir mesajla bunu söyler; tek kod üç sebebi kapsadığı için metin sebebi ayırmaz, yalnız sonucu ve iki yolu (yeniden denemek, şifreyle girmek) söyler. ⚠ değil.
6. **#26** — VAL-A190 (açıklama alanında saldırı kümesi) seviyesi Orta → Kritik; aynı gözle taranması gereken 13 `[güvenlik]` Orta satır cross-review'a bırakıldı.
7. **#30** — Ürün formundaki firma ayarları (P-6, P-9, P-44) eski ve yeni değeriyle ize yazılır (`02 §10.3.1` (6)).
8. **#100** — Parça boyutu (bayt) sınırı `12`'de taşınmaz; `05 §6.7.9`'a istenirse eklenir.
9. **#53** (kaydın yeri) — Kayıt yeri testin beklenen değeri ve ilk koşunun doğrulama raporudur.

#### Üst doküman geri beslemesi (K-652, aynı PR)

| Doküman | Yer | Değişiklik | Sürüm |
|---|---|---|---|
| `02` | §3.31.2; §3.30.5 (1); §4.1.4; §3.13.14; §6.1.2; §10.3.1 (6) | Ret sebebinin yeri (#39); ekran adı sekme başlığı (#142); bitiş ertesi günün 00.00'ı (#144); şifresiz hesapta sıfırlama bağlantısı gitmez (#25); tutarı tutmayan `success` (#46); ürün formu ayarları ize yazılır (#30) | v0.111 → v0.112 |
| `03` | §10.1.3.1 | Google ile giriş tamamlanmazsa giriş ekranı söyler (#150) | v0.46 → v0.47 |
| `04` | 4.4.6; 5.22.11; 6.2.22.6 | Sekme başlığı (#142); Google ile giriş mesajı (#150) | v0.52 → v0.53 |
| `05` | §9.1.3; §5.2.2; §10.3.6 | Sistem geçişinin reddi kayda yazılır (#39); "ödeme yoksa" (#46); en yalın hata yanıtı 500 (#102) | v0.49 → v0.50 |
| `06` | §2.5.2; §1 `02 §4.1.4` satırı; iki EndDate satırı | Bitiş ertesi günün 00.00'ı (#144) | v0.45 → v0.46 |
| `07` | §2.9.2; §2.8.10; §9.26.2; görsel yükleme satırı | Bitiş (#144); 413 sınırı gövdeyle: P-23 + 1 MB (#52) | v0.39 → v0.40 |
| `08` | §2.1.6 | Başarısı uygulanmayan `success` yanıtı yanıt alınamamış sayılır (#46) | v0.21 → v0.22 |

Üst dokümanda yazılı öneriler uygulanmadı: `07 §4.5.2` ve `09 §18.2` (seyreltmenin penceresi, #130), `05 §6.7.9` (bayt sınırı, #100 kararı gereği). L4 (doğrudan değiştirilmedi, öneri): `.claude/skills/gate-check/SKILL.md` — son fazın gate'inde `12 §6.3` kriterlerini satır kimlikleriyle işaretleyen adım ve "bu MVP'nin son uygulama fazıdır" notu; `.claude/skills/validate/SKILL.md` — Faz 1'e `12` satırlarını okuma ve `Durum` hücrelerini güncelleme adımı, çıktı formatına `12 §8.2` alanları (#107, #108, #110). `00`, `CLAUDE.md` ve `SETUP.md` değişikliği önerilmedi.

#### Kapılar

- `12_kapsam_hizasi.py`: hata 0.
- `12_matris_denetle.py --kapsam 1-77`: 1.501 satır, kimlik tekil, 77 KP kapsanan, eksik 0; hata 0.
- `12_atif_denetle.py .`: hedefi bulunamayan atıf 0; hata 0.
- `12_audit_hukum_denetle.py a10-kd-deep-2/2b`: Parça A'da sorun 0.

#### İlerleme

**127/127** bulgu hükme bağlandı; deep review tamamlandı. Yükselen yok (`yeniden.md` boş); 2r açılmadı. Sıradaki adım A10-KD-cross-1.
