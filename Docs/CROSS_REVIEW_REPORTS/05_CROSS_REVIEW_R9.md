# Cross-Review — 05 Technical Architecture (Tur 9)

**Tarih:** 2026-10-17 · **Hedef:** `Docs/05_TECHNICAL_ARCHITECTURE.md` v0.14 — güvence merceğinin 2. koşumunun düzeltmelerinden sonra, bu turun düzeltmelerinden önce (dal `docs/asama4-05-cross-review-r9`, taban `09942d8`; 356 655 bayt, 1 494 satır) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Ciddiyet ölçüsü:** 1. turdan itibaren uygulanır (K-952) · **Koşum:** iki biçimde, altı çağrı — tek parça çağrı ve beş parçalı kontrol koşumu (K-953); turun sonucu ikisinin birleşimidir · **Şablon:** `58cf0b9`'daki v0.1 — Aşama 4 açılmadan önceki hâl, park blokları çıkarılmış, 87 satır · **Girdi:** yalnız doküman ve boş şablonu, stdin'den tek metin, boş ve izole bir çalışma klasöründe; karar kaydı, önceki turların raporları, güvence merceğinin raporu, Ürün Gereksinimleri, Kullanıcı Akışları, Arayüz Tanımları, MVP Kapsamı ve audit/deep review raporları verilmedi (K-431) · **Çıkış kuralı:** skill Faz 4 madde 5 — bu turda uygulandı (§6)

## 1. Koşum

**İstem ve şablon** 1. turunkidir, değiştirilmeden (`05_CROSS_REVIEW.md` §1'in alıntı blokları — istem ve parça notu; istem dosyasının SHA-256'sı `a146e8e2581d375a…`, şablonunki `fdf1b15e21d82277…` — 7. ve 8. turunkilerle aynı). Girdiler aynı betikle yeniden kuruldu: `istem + "=== DOKÜMAN ===" + güncel doküman + "=== ŞABLON ===" + şablon`; parçalı çağrılarda istemden sonra parça notu, parçada başlık notu. **Komut** 1. turunkidir: `codex exec -s read-only -C <boş klasör> --skip-git-repo-check --ephemeral --ignore-user-config --color never -m gpt-5.6-terra -c 'model_reasoning_effort="high"' -o <çıktı> - < <girdi>`. Altı çağrı paralel koştu; altısı da hatasız döndü (çıkış kodu 0). Yedek yol (`cursor-agent`) gerekmedi.

**Parça sınırları** 1. turunkidir — bölüm başlıkları (K-953); satır numaraları girdinin (v0.14, bu turun düzeltmelerinden önce) satırlarıdır.

| Parça | Bölümler | Satırlar | Girdi | Token | Süre | Sonuç |
|---|---|---|---|---|---|---|
| — | Tek parça — dokümanın tamamı | 1–1494 | 356 655 bayt (doküman) | 147 889 | 203 sn | 2 bulgu |
| 1 | Başlık notu · §0 · §1 · §2 · §3 | 1–448 | 114 377 bayt | 39 706 | 38 sn | `SONUÇ: TEMİZ` |
| 2 | §4 durum ve olaylar · §5 arka plan işleri | 449–624 | 92 885 bayt | 38 551 | 64 sn | `SONUÇ: TEMİZ` |
| 3 | §6 güvenlik (§6.x dahil) | 625–830 | 109 605 bayt | 76 564 | 88 sn | 2 bulgu |
| 4 | §7 veri katmanı | 831–965 | 99 734 bayt | 43 699 | 38 sn | 1 bulgu |
| 5 | §8 ortamlar · §9 gözlemlenebilirlik · §10 hata toleransı · §11 ölçeklenebilirlik · §12 özet | 966–1494 | 173 723 bayt | 74 596 | 147 sn | 1 bulgu |

**Web araması:** üç çağrı web araması yaptı — tek parça ve parça 4 (6563 sayılı Kanun m.11/3, mevzuat.gov.tr) ve parça 3 (KVKK'nın çerez rehberi). Tek parçanın ve parça 4'ün bulgusu kanun metnine bağlantı verir. Parça 1, 2 ve 5 aramadı.

## 2. Ham bulgular

İkinci modelin çıktıları, satır sonu boşlukları dışında düzenlenmeden (alıntı blokları içinde).

**Tek parça çağrı:**

> ```text
> BULGU-1
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: §7.4.3 (4), §4.4.4
> Alıntı: “Ödemesi hiç alınmamış siparişin kendisi üç yılda (2)'nin biçimiyle kişisel verisiz kalır ve on yıl dolmadan silinmez.”
> Sorun: 6563 sayılı Kanun m.11/3, elektronik ticaret kapsamındaki iş ve işlemlere ilişkin bilgi, belge, defter ve elektronik kayıtlar için ödeme alınıp alınmadığına göre ayrım yapmadan on yıllık saklama öngörür. Üç yılda alıcı ve olay verilerinin boşaltılması, ödenmemiş siparişin tam işlem kaydını yok eder. Bu mevzuata aykırılık ve firma açısından denetim/hak kaybı riskidir.
> Öneri: Ödemesiz siparişe ait kanunen gerekli işlem kaydı ve onu doğrulayan kişisel alanları da on yıl saklayın; erişimi amaçla sınırlayın. Üç yıllık anonimleştirme ancak bu kayıtların Kanun kapsamı dışında olduğuna dair somut hukuki değerlendirme varsa uygulanmalıdır. [Kanun değişiklik metni](https://www.turmob.org.tr/arsiv/mbs/resmigazete/31889-4.pdf)
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: Başlık / son sürüm notu
> Alıntı: “**Versiyon: v0.14**” / “*Shopfolio — Technical Architecture v0.13*”
> Sorun: Dokümanın aynı sürümü iki farklı değerle belirtilmiş. Bu, hangi karar ve kalite döngüsü çıktısının uygulanacağı konusunda çelişkidir (sınıf 4).
> Öneri: Son sürüm notunu v0.14 yapın ve v0.14 değişikliklerini footer’daki sürüm özetine ekleyin; alternatif olarak footer’daki sürüm özetini kaldırıp başlıktaki sürümü tek kaynak yapın.
>
> SONUÇ: 2 BULGU
> ```

**Parça 1 — başlık notu, §0–§3:**

> ```text
> SONUÇ: TEMİZ
> ```

**Parça 2 — başlık notu, §4–§5:**

> ```text
> SONUÇ: TEMİZ
> ```

**Parça 3 — başlık notu, §6:**

> ```text
> BULGU-1
> Kriter: Güvenlik
> Seviye: Yüksek
> Yer: §6.1.4
> Alıntı: "Yalnız Google'ın doğrulanmış verdiği e-posta kabul edilir ve ürün Google'dan yalnız adı ve e-postayı alıp saklar — Google hesabının kalıcı kimliği saklanmaz"
> Sorun: Google e-postası kalıcı ve benzersiz kimlik değildir; değişebilir veya başka hesaba geçebilir. Sistemin e-postayla hesap eşlemesi, yeni e-posta sahibinin önceki kullanıcının hesabına ve kişisel verilerine Google ile erişmesine yol açar. Bu, kişisel verinin yetkisiz kişiye açılmasıdır (1).
> Öneri: Google girişini doğrulanmış `iss` + kalıcı `sub` çiftiyle hesabın kimlik kaydına bağlayın; e-postayı yalnız iletişim/görüntüleme verisi olarak kullanın. Mevcut hesapla Google kimliği ilişkilendirme işlemi ayrıca doğrulanmış bir akışla yapılmalıdır.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §6.x.8, §6.x.16
> Alıntı: "veri yazan yönetim komutları uygulamanın koduyla ve uygulama kullanıcısıyla, ama uygulamanın dış servis sırları olmadan (6.x.19)"
> Sorun: Doküman §6.x.19'a dayanıyor ancak bu madde yoktur; tablo da yönetim komutuna veritabanı parolasını dağıtmıyor. Komutlar uygulama kullanıcısıyla yazım yapacaksa bu parolaya ihtiyaç duyar. Bu haliyle kurtarma ve oturumları sonlandırma komutları çalışamaz ya da uygulamanın dış servis sırlarını da içeren sır paketiyle çalıştırılmak zorunda kalır. İlk durumda yöneticisiz kalınan kurtarma akışı çıkışsızdır; ikinci durum dağıtım yarıçapı kuralıyla çelişir (3), (4).
> Öneri: §6.x.19'u ekleyip yönetim komutuna yalnız gerekli veritabanı kimliğinin nasıl verileceğini açıkça tanımlayın; bunu ayrı ve en az yetkili bir komut kimliğiyle yapın veya uygulama parolasının bu tek seferlik bileşenle paylaşılmasını açık istisna olarak belirtip §6.x.8 ve sır envanterini buna göre düzeltin.
>
> SONUÇ: 2 BULGU
> ```

**Parça 4 — başlık notu, §7:**

> ```text
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §7.4.3 (4)
> Alıntı: "ödemesi hiç alınmamış siparişin (Z-41) satırında üç yıl dolunca alıcı ve olayın değerleri boşaltılır"
> Sorun: 6563 sayılı Kanun m.11/3, Kanun kapsamındaki iş ve işlemlere ilişkin bilgi, belge, defter ve elektronik kayıtların işlem tarihinden itibaren on yıl saklanmasını zorunlu kılar. Ödeme alınmamış sipariş de e-ticaret sürecindeki iş/işlem kaydıdır; alıcı ve olay değerlerinin üçüncü yılda silinmesi denetimde kaydın eksik sunulmasına yol açar. Bu yürürlükteki mevzuata aykırılıktır. [Kanun m.11/3](https://www.lexpera.com.tr/mevzuat/kanunlar/elektronik-ticaretin-duzenlenmesi-hakkinda-kanun-6563)
> Öneri: Ödeme alınmamış siparişlerin Kanun kapsamındaki işlem ve dayanak kayıtlarını da on yıl boyunca saklayacak biçimde saklama tasarımını düzeltin; KVKK minimizasyonu gerekiyorsa silme yerine erişim kısıtlama, ayrıştırma veya hukuki saklama dayanağıyla sınırlı işleme yaklaşımını ilgili üst dokümanlarla hizalayın.
>
> SONUÇ: 1 BULGU
> ```

**Parça 5 — başlık notu, §8–§12:**

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: Başlık notu / doküman sonu sürüm dipnotu
> Alıntı: "**Versiyon: v0.14**" / "*Shopfolio — Technical Architecture v0.13*"
> Sorun: Dokümanın sürümü iki yerde farklıdır. Son dipnot v0.14’te eklenen kalite döngüsü değişikliklerini de içermediğinden, hangi sürümün geçerli olduğu belirsizleşir; bu bir doküman içi çelişkidir (4).
> Öneri: Son dipnotu v0.14 yapın ve v0.14 değişikliklerini (K-970…K-975) dipnota ekleyin.
>
> SONUÇ: 1 BULGU
> ```

## 3. Bağımsız değerlendirme

Her bulgu dokümanın tamamına (v0.14), ilgili yerde Ürün Gereksinimleri'ne (`02` v0.74), karar kaydına, önceki turların raporlarına ve güvence merceğinin raporuna karşı okundu — ikinci model bunları görmedi, parçalı çağrılar dokümanın öteki bölümlerini de görmedi. Altı ham bulgu dört ayrı bulgudur: tek parça ile parça 5 aynı sürüm farkını, tek parça ile parça 4 aynı saklama iddiasını getirdi. **Şiddet** 5. turun raporunun §3'ündeki ölçüyle yazılır — yüksek: mevzuata aykırılık ya da olağan işleyişte para, hak veya veri kaybı; orta: bir güvencenin mekanizmasının eksikliği ya da nadir olmayan bir hâlde tutmaması; düşük: iki yerin metin farkı, aşırı kesin cümle, yalnız nadir arızada açılan pencere.

| # | Karar | Şiddet · sınıf | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|---|
| Tek · BULGU-2 = P5 · BULGU-1 — yeni | ✅ KABUL | **Düşük** — iki yerin metin farkı | **Fark gerçek.** Başlık "Versiyon: v0.14" diyor, dokümanın sonundaki sürüm dipnotu "v0.13" ile başlıyor ve v0.14'ün değişikliklerini saymıyordu: güvence merceğinin 2. koşumu başlığı ve başlık notunu güncelledi, dipnotu güncellemedi. İkinci modelin "hangi kararların uygulanacağı belirsiz" iddiası abartılıdır — kararların evi gövde ve karar kaydıdır, dipnot sürüm geçmişidir —; sınıf metin farkıdır. | Dipnot v0.14 oldu ve bu sürümün değişikliklerini sayar (güvence merceğinin 2. koşumu, K-970…K-975, 9. tur, K-976). |
| P3 · BULGU-2 — yeni | ⚠️ KISMİ | **Düşük** — iki yerin metin farkı: atıf, hedefi tabloda olmayan bir satıra gidiyordu | **Fark gerçek.** 2. koşum (K-971) §6.x.8'e "veri yazan yönetim komutları uygulamanın koduyla ve uygulama kullanıcısıyla, ama uygulamanın dış servis sırları olmadan (6.x.19)" ve §6.x.16'ya "6.x.19'a göre" yazdı; dağıtım yarıçapı tablosuna 6.x.19 satırı eklenmemişti. **"Kurtarma akışı çıkışsız kalır" iddiası reddedildi:** §6.x.8'in cümlesi komutların hangi kullanıcıyla bağlandığını ve hangi sırrı almadığını yazıyordu — eksik olan tablonun satırıdır; `DEPLOY_RUNBOOK`'un park satırı da aynı şeyi söylüyordu (K-971). **"Ayrı ve en az yetkili bir komut kimliği" önerisi uygulanmadı:** komutlar modüllerin arayüzünü çağırır (§8.5.1) ve uygulamanın yazdığı tablolara yazar — ayrı kullanıcı aynı yetkiyi ikinci kez tanımlardı; şablonun "paylaşılan sır bileşen başına varyant olur" kuralı bozulmaz, çünkü komutlar uygulama imajının tek seferlik çalışmasıdır ve sır imajla birliktedir (MK-59). | Tabloya 6.x.19 satırı eklendi: veri yazan yönetim komutları — uygulama kullanıcısının parolası; uygulamanın dış servis sırları, şema sahibinin ve salt okuma kullanıcısının parolaları ve yedek sırları alınmaz. |
| Tek · BULGU-1 = P4 · BULGU-1 — **tekrar eden RET** | ❌ RET (tekrar eden) | — | Ödemesi hiç alınmamış siparişte kişisel verinin üç yılda boşaltılması proje sahibinin kararıdır (K-951; §7.4.3 (4), §4.4.4 — "iki kolun ayrımı bilinçli karardır"): on yıllık saklama işlemin kaydını tutar, tamamlanmamış işlemin kişisel verisi gerektiğinden uzun tutulmaz (KVKK m.4/2-ç, m.4/2-d). Aynı yer ve iddiayla 2. turda (KISMİ — iddianın mevzuat kolu reddedildi), 4. ve 7. turda reddedildi. Turun sonucunu belirlemez. | — |
| P3 · BULGU-1 — **tekrar eden RET** | ❌ RET (tekrar eden) | — | Google hesabının kalıcı kimliğinin saklanmaması bilinçli karardır (K-900; §6.1.4); bir siparişin ya da giriş yolunun hesaba bağlanmasının tek dayanağı doğrulanmış e-postadır (`02 §8.1.2`) ve Google hesabının e-postası değişirse Google ile giriş tutmaz (§6.1.5). Aynı yer ve iddiayla 4., 5., 7. ve 8. turda reddedildi. Turun sonucunu belirlemez. | — |

**Dağılım:** dört ayrı bulgu — 1 KABUL · 1 KISMİ · 2 tekrar eden RET. **Şiddet:** iki kabulün ikisi de düşük, ikisi de iki yerin metin farkı. **%100 kabul değil:** P3 · BULGU-2'nin şiddet iddiası ve önerisi somut yerle (§6.x.8, §8.5.1, MK-59) reddedildi. **Yeni karar:** turun bulguları karar doğurmadı; çıkış kuralının uygulanışı bir süreç kararıyla kaydedildi (K-976, ⚠ değil).

**Hukuki kontrol** (2026-10-17): iki çağrı (tek parça, parça 4) 6563 sayılı Kanun m.11/3'e dayanır — dayanak K-951'in kendisinin dayanağıdır ve proje sahibinin kararıyla tartılmıştır (karar kaydı §12.1'in çok kritik sorusu, 2026-10-16); bu tur yeni bir hukuki içerik getirmedi. Tekrar eden RET kuralı (skill Faz 4 madde 5) gereği yeniden tartılmadı.

## 4. Ek bulgular

- **Aynı kalıbın taraması — iç atıfların hedefi:** dokümanın kendi alt bölümlerine giden bütün atıflar (§n.m, §n.m.k, n.m.k ve 6.x.n biçimleri; başka dokümanın dizisindeki atıflar konvansiyon 6'nın devralma kuralıyla ayıklanarak) tanımlı maddelerle betikle karşılaştırıldı. Hedefi olmayan tek atıf ailesi 6.x.19'du (§6.x.8 ve §6.x.16) — satır eklendikten sonra eşleşmeyen atıf kalmadı.
- **Aynı kalıbın taraması — sürüm damgaları:** başlık, başlık notunun son satırları ve dipnot v0.14'ü taşır; başlık notundaki eski sürüm numaraları o sürümün kaydıdır.
- **Kendi okumam — merceğin bölgeleri:** 8. turun beş kısmi kabulünün dördü merceğin 1. koşumunun taramadığı §4, §5 ve §6'daydı; 2. koşumdan sonra parça 2 (§4–§5) TEMİZ döndü, parça 3'ün (§6) iki bulgusundan biri tekrar eden RET, öteki merceğin kendi düzeltmesinin metin farkıdır. Güvence cümlesi ile mekanizmanın farkı sınıfından bu turda bulgu yok.
- **Kendi okumam — K-974'ün kalan riskleri:** ikinci model 10.1.3'ün yeni kalan riskini (iptal edilmiş siparişe gelen geç ödemenin kaybolan bildirimi) ve 9.4.14'ün açık koşul notunu bulgu yapmadı.
- **Mekanik tarama:** karar kaydında çift numara yok; K-855'ten bu yana `(öneriyle kaydedildi — ⚠)` taşıyan satırlar betikle sayıldı — 39 — ve §12.1'in listesiyle aynı. Değişen dosyalarda CR yok; geri başvurulu değiştirme kullanılmadı (değişiklikler birebir metin değiştiren betikle yapıldı).
- **Önceki turlardan açık kalanlar:** yok — 8. turun önerisi (9. turdan önce güvence merceği §4, §5, §6'da) bu adımda uygulandı (`AUDIT_REPORTS/05_GUARANTEE_LENS.md`, 2. koşum).

## 5. Kullanıcı onay checklist'i

K-855 kararıyla (Aşama 4 boyunca ⚠ öneriyle kayıt, CI yeşilse `docs:` PR merge yetkisi ve yönetici + alt ajan düzeni) alt ajan değerlendirdi ve uyguladı (`05` v0.14). Bu turun kararı: **K-976** (⚠ dışı — Teknik Mimari'nin cross-review'ı çıkış kuralıyla kapanır). Aynı adımın güvence merceğinin kararları K-970…K-975'tir; biri ⚠ (K-974 — üç kalan risk). ⚠ listesi 39. Geri besleme: yalnız etki yansıtmanın Kaynak taraması — Arayüz Tanımları v0.26 (üç Kaynak satırı; kural değişmedi).

- [x] Tek · BULGU-2 = P5 · BULGU-1 (KABUL, düşük — sürüm dipnotu)
- [x] P3 · BULGU-2 (KISMİ, düşük — 6.x.19 satırı)
- [x] Tek · BULGU-1 = P4 · BULGU-1 (tekrar eden RET — K-951)
- [x] P3 · BULGU-1 (tekrar eden RET — K-900)
- [x] Etki yansıtma (Faz 5) — §7

## 6. Sonuç

**Sonuç: TEMİZ — çıkış kuralıyla, 9 tur** (skill Faz 4 madde 5; K-976). Ciddiyet ölçüsü 1. turdan uygulanır (K-952); TEMİZ o ölçüdedir.

**Çıkış kuralının koşulları harfiyen sağlanır.** (1) Tur beşinciden sonradır. (2) Kabul edilen iki bulgunun ikisi de düşük şiddette iki yerin metin farkıdır — mevzuata aykırılık, para ya da hak kaybı, çıkışsız akış değildir; orta ya da yüksek kabul yoktur. (3) Kalan iki iddia tekrar eden RET'tir ve turun sonucunu belirlemez. Bulgular uygulandı ve kalıp dokümanın tamamında tarandı (§4). **Sınıfın kalan riski** — iki yerin metin farkı ve güvence cümlesi ile mekanizmanın farkı — Aşama 4'ün checkpoint'ine devredildi: karar kaydı §12.1'in açık süreç maddesi sınıfın adını ve örneklerin yerini (3.–9. turların raporları, güvence merceğinin iki koşumu) taşır; checkpoint'in iç tutarlılık merceği sınıfı dokümanın tamamında, geri beslenen dokümanlarda ve park satırlarında arar.

**Yakınsama ölçüsü:** ayrı bulgu 7 → 5 → 6 → 6 → 5 → 5 → 4 → 6 → 4; kabul edilen ya da kısmen kabul edilen 7 → 3 → 3 → 3 → 4 → 3 → 1 → 5 → 2; turun yeni kararı 2 → 0 → 1 → 2 → 2 → 1 → 1 → 3 → 0 (çıkış kuralının K-976'sı hariç); tekrar eden RET 0 → 0 → 0 → 2 → 1 → 0 → 2 → 1 → 2; TEMİZ çağrı 0 → 1 → 1 → 1 → 2 → 2 → 2 → 2 → 2. Güvence merceğinin 2. koşumu dokümanı 341 516 bayttan 356 655 bayta büyüttü; §6.x'in tablosuna bir satır ekledi, MK açılmadı.

**Gözlem.** 3.–8. turların orta ve kısmi kabullerinin çoğu güvence cümlesi ile mekanizmanın farkıydı ve ikinci model onları her turda başka bir bölgede buluyordu. Güvence merceği iki koşumda bu sınıfı dokümanın iki büyük bölgesinde bir kerede taradı; ikinci model ilk koşumdan sonra o bölgelerde, ikinci koşumdan sonra dokümanın hiçbir yerinde bu sınıftan bulgu getirmedi. Kalan iki kabul merceğin kendi düzeltmesinin metin farkıdır — düzeltmenin andığı hedefler de betikle denetlenmelidir (karar kaydı §12.1, öğrenim adayı 7'nin kanıtı).

## 7. Etki yansıtma (Faz 5)

Etki yansıtma `cross-review` skill'inin Faz 5'iyle koştu. Kapsam: kalite döngüsünün bütün turlarında `05`'te yapılan değişiklikler ve K-855…K-976'nın sonraki ve önceki dokümanlara bıraktığı işler.

1. **Downstream tarama — park satırları, iki yönde.** Veri Modeli (`06`), API Tasarımı (`07`), Entegrasyon Spesifikasyonu (`08`), Kodlama Kılavuzu (`09`), Implementation Planı (`11`), Doğrulama Protokolü (`12`) ve `DEPLOY_RUNBOOK`'un "Aşama 4'ten park edilen girdiler" bloklarındaki etiketler, K-855…K-976'nın etki sütunlarında "park satırı" diyen hedeflerle betikle karşılaştırıldı (`checklists/document-stage.md` §3). K-970…K-975'in on üç park güncellemesi iki yönde eşleşir. Eşleşmeyen üç satır betiğin yanlış pozitifidir, gerçek bir kaçak değildir: K-969 ve K-933'ün etki sütunu "park gerekmez" der ve gerekçesinde "park satırı" sözcüğünü taşır; K-900'ün `08` hedefi K-942'nin kalemi içinde anılır. Bu turda yapılan düzeltmeler (6.x.19, dipnot) sonraki bir dokümana iş bırakmaz — 6.x.19'un içeriği `DEPLOY_RUNBOOK`'un sırlar park satırında K-971 ile zaten vardı.
2. **Upstream tarama — Proje Vizyonu (`01`), Ürün Gereksinimleri (`02`), Kullanıcı Akışları (`03`), Arayüz Tanımları (`04`), MVP Kapsamı (`10`).** Bu adımın kararları ürün kuralını değiştirmez; geri besleme gerekmedi. İki betik koştu: **(1) terim taraması** — kararların nesnesi ve işlemi ("bağlantının açılması", "oturumlarını sonlandır", "son yönetici", "gönderdiği kullanılmamış davet", "yeniden gönderim", "kaçırdıkları sırayla", "beni hatırla", "geç gelen", "kendiliğinden iade", "imha kaydı", "işlem kimliği", "aynı bildirim", "durum değiştirmez", "imzalı makine", "salt okuma" ve ötekiler) beş dokümanda arandı ve her eşleşme okundu: hepsi ürün düzeyinde doğrudur ve `05`'in yeni mekanizmasıyla çelişmez — `02 §6.2.16`'nın kendiliğinden iadesi ürün kuralıdır, `05`'in yeni kalan riski (10.1.3) yalnız bir arızada açılır ve ürün kuralını değiştirmez (güvence merceğinin 1. koşumunun E satırıyla aynı ilke); `02 §3.34.7` ve §4.3'ün "kaçırdıkları sırayla"sı ürün düzeyinde tutar. **(2) Kaynak satırları** — gövdede anılan her Aşama 4 kararı bölümünün Kaynak satırıyla karşılaştırıldı: `05`'te değişiklikten önce ve sonra aynı sonuç (yalnız §12'nin gerekçe hücreleri ve dipnot — §12'nin Kaynak satırı "K numaraları gerekçe hücresindedir" der); Arayüz Tanımları'nda üç eksik atıf bulundu — §3.4 (K-940), §3.5 ve §6.3 (K-904) — ve tamamlandı (`04` v0.26; kural ve metin değişmedi, ✓ korunur); öteki dört dokümanda bulunanlar dipnotun yanlış pozitifidir.
3. **Yeni alan ve kural taraması.** Bu adımın yeni alanları — ödeme kaydında sağlayıcının ödeme işlem kimliği, bildirimin tekil kimliği ve olay türü, yönetici davetinin göndereni, yeniden doğrulama izninin tek kullanımlık değeri, deneme satırının ön yazımı, iz satırında yöneticinin adı ve e-postası — Veri Modeli'nin park satırlarındadır (K-970, K-971, K-972, K-975); bildirimin kimliği ve olay türleri Entegrasyon Spesifikasyonu'nun park satırındadır (K-970); yazan komutların sırrı `DEPLOY_RUNBOOK`'un sırlar satırındadır (K-971). Yeni enum değeri ve yeni ürün parametresi yok.
4. **Kaynak satırları taraması** — madde 2'nin (2)'si.
5. **Raporlarda taşınan açık notlar:** 1.–8. turların raporlarındaki "önceki turlardan açık kalanlar" listeleri kapanmıştı; 8. turun tek önerisi bu adımda uygulandı. Güvence merceğinin iki koşumunun kalan riskleri (raporun iki §5'i) `05`'in gövdesinde adıyla yazılıdır.
6. **Uyumsuzluklar:** yalnız madde 2'nin Arayüz Tanımları Kaynak satırları — kapatıldı.

**Teknik Mimari ✓** (karar kaydı §1: Durum ✓, Cross-review ✓ (9 tur), Checkpoint ⬚). **Sıradaki iş:** Aşama 4'ün kapanışı — çakışma taraması (K-437'nin 3. adımı; `checklists/document-stage.md` §7).
