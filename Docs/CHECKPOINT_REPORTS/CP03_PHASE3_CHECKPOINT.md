# Aşama 3 — Checkpoint (CP03)

**Tarih:** 2026-10-05 | **Aşama:** 3 — UI/UX tasarım | **Kapanış adımı:** K-437'nin 4. adımı (`/checkpoint`; `checklists/document-stage.md` §6 son madde, §7 madde 4)
**Girdi sürümleri:** karar kaydı v0.91 · Arayüz Tanımları (`04`) v0.18 · Ürün Gereksinimleri (`02`) v0.65 · Kullanıcı Akışları (`03`) v0.21 · MVP Kapsamı (`10`) v0.43 · Proje Vizyonu (`01`) v0.33 · `05`–`09`, `11`, `12` ve `DEPLOY_RUNBOOK`'un park blokları — `origin/main` @ `58cf0b9`
**Çıktı sürümleri:** karar kaydı v0.92 · Arayüz Tanımları v0.19 · Ürün Gereksinimleri v0.66 · Kullanıcı Akışları v0.22 · MVP Kapsamı v0.44 · Proje Vizyonu değişmedi · park blokları değişmedi
**Girdi raporu:** [Aşama 3 — Çakışma Taraması](PHASE3_CONFLICT_SCAN.md) (K-437'nin 3. adımı, K-429) — açık kalem durumu, ⚠ kayıtların listesi ve hazır metni (§4.1), devir dizini (§6) ve checkpoint'e notlar (§8) oradan alındı. **K-849'un devri** — Arayüz Tanımları'nın cross-review'ının bıraktığı "özet cümle ile ayrıntının ayrışması" sınıfı — karar kaydı §10.1'in checkpoint maddesinden.

> **Ne işe yarar:** Aşama kapanmadan önce Arayüz Tanımları'nın kendi içinde ve Aşama 3'ün geri beslediği dokümanlarla karar kaydına ve metodolojiye karşı tutarlı olduğunu doğrular; bulunan her uyumsuzluğu ya kapatır ya da adıyla proje sahibine bırakır. Sonuç öğrenim terfisinin (5. adım) ve arşiv işaretinin (6. adım, K-436 — kapsamı K-647) girdisidir.

---

## Checkpoint Sonucu — 2026-10-05

**Aşama:** 3 — UI/UX tasarım (kapanış sırası K-437: yazım turu ✓ · kalite döngüsü ✓ · çakışma taraması ✓ · **checkpoint** · öğrenim terfisi · arşiv işareti)
**Genel durum:** ⚠ Dikkat gerektiren noktalar — yetmiş altı bulgu (on iki orta, altmış dört düşük); hepsi bu checkpoint'in PR'ında kapandı — yetmiş beşi var olan kararların ya da dokümanın kendi kuralının yansımasıyla, biri yeni bir kararla (K-851, öneriyle kaydedildi — ⚠ değil). Çok kritik konu yok. Kapanıştan sonra durum **✓ Yolunda**; açık kalan tek kapı kırk ⚠ kararın proje sahibine gösterilmesidir (arşiv işaretinden önce, §5).

### Kontrol Özeti

| # | Kontrol | Sonuç | Detay |
|---|---|---|---|
| 1 | Yol haritası | ✓ | Aşama 3'teyiz (`00 §C.1`: rol Senior Product Designer / UX Architect, girdi `02` + `03` + `10`, çıktı `04`; izlenebilirlik matrisi zorunlu). Açılış (PR #75), izlenebilirlik matrisi üç oturumda (#76–#78), workshop (#79), tek yazım turu beş oturumda ve kısmi adet kararı (#80–#86), kalite döngüsü — audit ve deep review (#87), cross-review beş tur, 5. turda K-849'un çıkış kuralıyla TEMİZ ve etki yansıtma (#88–#92) — ve çakışma taraması (#93) bu sırayla kapandı. Atlanan ya da sırası değişen adım yok (K-437, K-432, K-430). |
| 2 | Doküman durumu | ✓ | Başlık ve dosya sonu sürümleri kayıt §1 ile aynıydı (`01` v0.33 / 2026-10-04; `02` v0.65, `03` v0.21, `04` v0.18, `10` v0.43 / 2026-10-05); kayıt §1'in `01`, `02`, `03`, `10` sürüm geçmişi notları son sürümü taşıyordu (CP02'nin U-36 sınıfı yok). Şablon dokümanlarda (`05`–`09`, `11`, `12`) başlık v0.1 / `YYYY-AA-GG`, dosya sonu aynı; park bloğu eklemeleri başlığa dokunmaz. `DEPLOY_RUNBOOK`'un sürüm alanı yok. Bu PR'da dört doküman ve kayıt birlikte arttı; `04`'ün Checkpoint sütunu ✓ oldu, Durum sütunu ⏳ kalır — ✓ arşiv işaretindedir. |
| 3 | Tutarsızlık | ⚠ → ✓ | **K-849'un devri** — Arayüz Tanımları'nın kendi içinde özet cümle ile ayrıntının ayrışması: kırk üç bulgu, on orta (§3.1, §3.2). **Yansıma kaçakları** — Aşama 3'ün geri beslediği bir kural bir yüzde değişip ötekinde eski kalmış: yirmi dokuz bulgu, altmış dokuz yer, iki orta (§3.3). Sayılar, kimlikler, durum ve aktör adları betikle tutarlı (Notlar). |
| 4 | Açık kararlar | ✓ | Tracker §4'te açık satır yok, A-19 açılmadı; Ürün Gereksinimleri §13 boş; `04`, `03`, `10` ve `01`'de açık kalem yok (çakışma taraması §1, §3). §10.1'in açık süreç maddeleri kapısındadır: K-849'un devri bu checkpoint'te kapandı; ⚠ listesi — §5; öğrenim terfisi adayları ve Ö-23 — 5. adım. |
| 5 | Aşama çıktıları | ✓ | `04` yazıldı (§1 matris — 1.193 kaynak satırı, on bir GAP karara bağlı; §2 yirmi ortak bileşen; §3 124 geçiş; §4 53 ekran; §5 ve §9 elli üç ekran tanımı; §6 285 satır; §7 42 form, 165 alan satırı; §8), kalite döngüsünden geçti ve etkileri üst dokümanlara geri beslendi (K-652; `02` v0.56…v0.66, `03` v0.14…v0.22, `10` v0.38…v0.44, `01` v0.33). Checklist'in aşamaya özgü hatırlatmaları karşılandı: ortak bileşen kütüphanesi (§2) ve navigasyon haritası (§3) ekran tanımlarından önce; {ekran × rol × durum} matrisi (§6) eksik varyant yakaladı (K-826…K-829) ve bu checkpoint'te iki satır daha aldı; yönetim ekranları elli üçün yirmi beşi. |
| 6 | Geriye dönük etki | ⚠ → ✓ | Proje Vizyonu'na etki yok — M-4'ün okunuşu kısmi adetle uyumlu (K-790), Ü-1…Ü-4 ve S-1…S-10 bozulmadı; mercek 3'ün `01` eşleşmelerinin hepsi uyumlu. Önceki dokümanların eski kalan yüzleri §3.3'te kapandı — Ürün Gereksinimleri on beş, Kullanıcı Akışları on, MVP Kapsamı dört yer. Park blokları: çakışma taramasının değiştirdiği satırlar (`05`, `12` K-834 · K-850; `06` K-17 · K-531; `08` K-12; `12` K-22, kısmi adet etiketi; `DEPLOY_RUNBOOK` K-736) ve dizin satırları kaynak kararlarla aynı içerikte; bu checkpoint park satırı gerektirmedi (K-851 sonraki dokümana iş bırakmaz). |
| 7 | Yeni alan/kural | ⚠ → ✓ | Arayüz Tanımları'nın kendi sabiti eksikti: içerik liste sayfasının sayfa boyu hiçbir kararda yoktu — K-851 (C-3). Kuralın evinde eksik kalan Aşama 3 kuralları: veri toplayan girişlerin kapısının panel yüzü (C-54), yayından çekmenin yokluğu (`02 §3.1.6`, §10.1.2 — C-65), kategorilerin elle sıralanamaması (`02 §10.1.2` — C-69). Kaynak satırları (betik 2): yirmi iki eksik atıf (§3.4). |

### Aksiyon Gerektiren Maddeler

- [x] Yetmiş altı bulgu düzeltmeyle kapandı — `04` v0.19, `02` v0.66, `03` v0.22, `10` v0.44, karar kaydı v0.92 (aşağıdaki §3)
- [x] Bir karar — K-851 (öneriyle kaydedildi — ⚠ değil): içerik liste sayfası (E-07) sayfa başına 24 kayıt; ⚠ listesi değişmedi (40)
- [x] K-849'un devri kapandı (karar kaydı §10.1); matrisin `03 §3.2.1.8`–§3.2.1.9 → E-12 eşlemesi karara bağlandı — çıktı (C-76)
- [x] Checkpoint'in dokunduğu otuz sekiz Aşama 3 kararının etki sütununa `(taslak güncellendi — vX.Y; checkpoint)` parçası eklendi (§4); Aşama 1–2 kayıtlarına dokunulmadı
- [x] **Kırk ⚠ kararın proje sahibine gösterilmesi** — liste §5.1'de karar başına bir sade cümleyle; K-848 ve K-849 süreç kararı olarak aynı mesajın sonunda. Kapı: arşiv işaretinden (6. adım) önce, tek mesajda (`INSTRUCTIONS.md` §2, §9; tracker §10.1). İtiraz gelen karar yeni bir satırla değişir ve bu rapora `## Retro Güncelleme` bölümü eklenir — **kapandı (2026-10-05, kapısında; K-854):** liste kapıdan önce gösterildi, itiraz yok, retro bölümü gerekmedi; sonuç §5.1'in sonunda
- [x] Sırada öğrenim terfisi (K-437'nin 5. adımı) — on iki aday tracker §10.1'de; 11 ve 12 bu checkpoint'te eklendi (§6); Ö-23'ün kapısı aynı adım — **kapandı (2026-10-05, PR #95, K-852)**

### Notlar

**Yöntem.** Mekanik kontroller betikle koştu: K-652'nin iki betiği (aşağıda), K atıflarının kayıtta varlığı, ⚠ listesinin kayıtla karşılaştırılması, sayımlar (§3 geçiş, §6 satır, §7 alan, §1.2 sayıları), sürüm ve tarihler. Anlamsal tarama dört salt okuma merceğiyle, ön planda koştu; her mercek bulgusunu bulduğu anda dosyaya yazdı: **(1)** Arayüz Tanımları'nın iç tutarlılığı, müşteri tarafı — §5'in yirmi sekiz ekranı ↔ başlık notu, §2, §3, §4, §6.1–§6.2, §7.1, §7.3, §8 (589 ekran kimliği ve 347 madde atfı okundu; §3'ün 139 {geçiş × ekran} çifti iki yönlü); **(2)** aynısı panel tarafı — §9'un yirmi beş ekranı ↔ §2, §3, §4, §6.3, §7.2 (483 satır, 223 madde atfı); **(3)** geri beslenen kuralların yüzleri, müşteri tarafı (on kural); **(4)** aynısı firma tarafı (altı kural grubu). Her bulgu düzeltmeden önce güncel metne karşı yeniden okundu; on yedi adayın on biri doğrulandı, altısı düzeltme gerektirmedi ("İşlenmeyenler"). Düzeltmeler var olan kararların yansımasıdır — üst dokümanların ✓ durumu ve kalite döngüleri yeniden açılmadı (K-652). Ortam: `LC_ALL=C.UTF-8`, Python yok; düzeltmeler birebir metin değiştiren bir betikle — her değiştirme tam bir eşleşme istedi — uygulandı.

**Betik 1 — kuralların yüzleri.** Mercek 3: kısmi adet (415 eşleşme), ayıp talebi (304), ödeme adımı (186), kargodaki cayma (58), geri alma düğmesi (89), çerez ve veri toplayan girişlerin kapısı (104), sipariş sayfasının geri ödemeleri (45), dijital ürün uyarıları (17), müşterinin son adımı · Google bağı · GAP-3 · başka hesaba ait adres (148), vitrin (140). Mercek 4: dışa aktarmanın yeri (128), yönetici adı (~260), ürün listesi araması (97), yeniden gönderim kapsamı (172), K-805 · K-806 · K-808 · K-813 · K-818 · K-821 · K-835 · teslimat illeri (~205), firmanın adetli işlemleri ve panelin uyarıları (~395). Her eşleşme okundu ve "uyumlu · ilgisiz · kayıt · eski hâlde" diye sınıflandı; desenler başlamadan örneklendi (ör. "Kargoda" sıfat, "tek tıkla", "kart verisi" ayrıldı). Temiz dönen kurallar: dışa aktarmanın yeri (K-836), yönetici adı (K-736), ürün listesi araması (K-737), geri alma düğmesi (K-839), kargodaki cayma (K-838), "Bu ürünü daha önce aldınız" ve "Bu ürün zaten sepetinde" (K-236 — her yüzde tam metin), K-806, K-813, K-835.

**Betik 2 — Kaynak satırları.** Beş dokümanda gövdede anılan her K numarası bulunduğu bölümün sonundaki Kaynak satırıyla — tablo satırında Kaynak sütunu varsa satırın hücresiyle — karşılaştırıldı (aralıklar `K-a…K-b` ve `K-a–K-b` açıldı; `ÖK-`, `SK-`, bulgu kimliklerinin `K-n` kuyrukları süzüldü). Kapsam: `01`, `02`, `03`, `10`'da Aşama 2 ve 3 kararları; `04`'te bütün kararlar (Aşama 3'ün kendi dokümanı). Bulunan yirmi iki eksik atıf §3.4'te; checkpoint düzeltmelerinin gövdeye getirdiği atıflar (`02`'nin altı Kaynak satırına dokuz, `04`'ün §2.13, §5.7, §9.16, §9.19 satırlarına yedi) aynı PR'da eklendi. **Son koşum boş.** Kapsam dışı: `04`'ün bölüm girişlerindeki yazım oturumu notları (§3, §5, §7, §8, §9 girişleri — kaynak değil oturum kaydı); `01`, `02` ve `10`'da Aşama 1 kararlarının satır içinde anılması (`02 §5`'in durum tabloları, `10 §3` ve §4.2'nin satırları — karar satırın kendi kaynağıdır; CP01'in yapısı); `03 §7`'nin bildirim haritası (Kaynak sütunu yoktur, K hücrede durur).

**Betikle doğrulanıp tutarlı çıkanlar:**

- **⚠ listesi:** konu hücresi `(öneriyle kaydedildi — ⚠` taşıyan K-723…K-851 satırları **40**; §10.1'in listesindeki numaralı K'lar **40** — `comm` iki yönde boş. Aşama 3'ün 129 satırının işaret dağılımı: ⚠ 40 · `(öneriyle kaydedildi)` 87 (K-851 dahil) · işaretsiz 2 (K-723, K-787 — proje sahibinin kararları).
- **K tekilliği:** komut boş; en büyük numara K-851.
- **K atıfları:** `01`'de 165, `02`'de 694, `03`'te 123, `10`'da 647, `04`'te 299 farklı K numarası — hepsi kayıtta (`04`'teki iki görünür istisna `KAPSAM-K-2` ve `KAPSAM-K-6` audit bulgu kimlikleridir).
- **Sayılar:** `04 §3` 124 geçiş (123 + 3.4.23) · §6 285 satır (283 + 6.2.13.14 + 6.3.16.6) · §7 42 form, 165 alan satırı · matris 1.193 satır, durum dağılımı değişmedi · §1.2'de E-12 46 → 44, E-17 24 → 25, E-18 30 → 31, E-25 25 → 26 · `03` 793 satır · `02` 438 kalın kural · `10` KP-1…KP-77.
- **Kimlikler:** bu PR yeni B-, F-, Z-, L-, P-, H-, KP- ya da E- kimliği açmadı; çakışma taramasının §5.4 sınıfları değişen satırlarda yeniden koştu — temiz.
- **Durum ve aktör adları:** değişen satırlarda yanlış biçim yok; panelin giriş adımında rol "Yönetici", davet, sıfırlama ve bağlantı adımlarında "Kullanıcı" (C-37; konvansiyon 7).

**İşlenmeyenler — düzeltme gerektirmedi:** (1) Ana sayfada blok başlığının bağlantısı ile "Tümünü gör" (5.1.5, 5.1.8, 3.3.4) — K-753'ün kendi metnidir, iki okuma da kuralla uyumlu. (2) 2.7.1.1'in "görünmez" bölge listesi örnek listesidir; 9.9.40 ve 9.20.4 kurala atıfla aynı biçimi kullanır. (3) E-47'nin birincil düğmesi "düzenlenen metnin 'Yayına al'ı" — K-818'in kurgusu, konvansiyon 4'le çelişmez. (4) `02 §4.2` Z-42'nin kapsamı — süre teslimden sonraki caymanın sistem davranışını tanımlar; kargodaki kalemden caymada yükümlülük §7.3.5'te (K-838) yazılı, ekran onu koşullu söyler. (5) `04 §1.1` KP-50 ve KP-68 özetleri eksik ama çelişmez — özet kaynak satırın tamamı değildir. (6) §1.1'in 1b yöntem notundaki "işaretin biçimi workshop konusudur" tarihçedir. Ayrıca 5.13.9 (h)'nin "ödenecek toplam"ı ile 2.12.7.2'nin "kutular ile düğme arasına başka içerik girmez" cümlesi K-756'nın metnidir — bilinçli karar, bulgu yazılmadı.

---

## 3. Bulgular ve kapanışları

Seviye: **orta** — iki yer çelişiyor, kapalı liste, sayı ya da hedef yanlış, ya da source-of-truth'ta bir kural eksik; **düşük** — ifade, kapsam ya da atıf farkı, anlamı değiştirmez. Kontrol sütunu skill'in yedi kontrolüne gider. Yer sütununda önsüz numara Arayüz Tanımları'nın maddesidir.

### 3.1 Arayüz Tanımları'nın iç tutarlılığı — müşteri tarafı (K-849'un devri)

| # | Kontrol | Yer | Bulgu | Kapanış | Seviye |
|---|---|---|---|---|---|
| C-1 | 3 | 5.15.1, 5.16.1 ↔ 3.3.39, 5.14.1 | Kalite döngüsünde eklenen 3.3.39 (yeniden açılan kart teyit ekranı → üyede E-16, misafirde E-15; K-840) hedef ekranların giriş listesinde yoktu | İki giriş listesi 3.3.39'u sayar | **orta** |
| C-2 | 3 | 2.6.4 ↔ 5.14.4, 2.6 | Son ödeme günü yalnız sipariş sayfasında sayılıyordu; teyit ekranı da gösterir (K-776) | "teyit ekranında ve sipariş sayfasında" | düşük |
| C-3 | 7 | 5.7.3 ↔ 2.13.3 | İçerik liste sayfası sayfa boyu için §2.13.3'e işaret ediyordu; orada içerik listesi için sayı yoktu, hiçbir karar vermiyordu | **K-851:** sayfa başına 24 kayıt; 2.13.3 ve 5.7.3 | **orta** |
| C-4 | 3 | 2.13.3 ↔ 5.27.3 | Sipariş geçmişinin 25'i (K-784) bileşenin sayfalama kuralında yoktu | 2.13.3 sayar; Kaynak'a K-784 | düşük |
| C-5 | 3 | 5.26.1, 5.27.1 ↔ 3.6.1, 3.5.14 | Oturumsuz gelişte girişten sonraki dönüş E-25'in girişinde vardı, E-26 ve E-27'ninkinde yoktu | İkisine eklendi | düşük |
| C-6 | 3 | §4.1 E-28 ↔ 5.28.2, 5.28.5, 3.3.33 | Amaç "şifrenin yeniden belirlenmesine götürür" diyordu; ekran düğme ya da geçiş taşımaz, şifre e-postadaki bağlantıyla kurulur | "yeni şifrenin … sıfırlama bağlantısıyla kurulacağını bildirir" | düşük |
| C-7 | 3 | 5.19.1, 5.19.2 ↔ 5.19.4, 3.3.25 | Birincil düğme ve çıkış yalnız yeni talebin gönderimiydi; yeniden açma hâli yoktu | "hâlin düğmesi — gönderim ya da yeniden açma"; çıkışa yeniden açma | düşük |
| C-8 | 3 | 5.14.9 ↔ 5.14.10, 6.2.14.5 | "Üye ile misafir alıcı arasındaki tek fark" — K-840'ın yeniden açılan ekranı iki aktörü farklı ekrana götürür | Fark "siparişe ulaşma yolu" olarak iki hâliyle | **orta** |
| C-9 | 3 | 5.12.6, 2.6.5 ↔ 5.12.10, 6.1.4 | Asgari tutar eksiğinin yeri üç yerde farklıydı (düğmenin üstünde · düğmenin yerinde · dökümün içinde) | Dökümün altında, geçiş düğmesinin yerinde | düşük |
| C-10 | 3 | 6.1.5.1 ↔ 6.2.5.1, 5.5.7 | Yönetici satırı olan ekranların kapalı listesi E-05'i saymıyordu | E-05'in satırı ekranın yöneticiye aynı olduğunu söyler | düşük |
| C-11 | 3 | 2.3.1.3 ↔ 5.12.15, 6.2.12.6 | Bir kez söylenen mesajın kullanım listesinde ödeme yöntemi kalmadığı için sepete dönüş yoktu | Eklendi | düşük |
| C-12 | 3 | 2.7.1.2 ↔ 5.26.9, 5.7.8, 5.9.8 | Boş hâlin vitrin dalı yalnız dönüş yolunu sayıyordu; adres defteri ilk kaydı açan düğmeyi taşır; içerik liste ve SSS'nin boş hâli listede yoktu | İki dal ve iki ekran eklendi | düşük |
| C-13 | 3 | 2.12.2.2, 2.1.4 ↔ 5.13.7, 7.1.5.5 | Fatura işareti koşulsuz yazılıydı; fiziksel kalemsiz siparişte işaret yoktur | Koşul eklendi | düşük |
| C-14 | 3 | 2.12.7.5 ↔ 5.16.9; `02 §3.24.6` | Kutu kaydını bütün kutulara genişletiyordu; kaynak dijital ve hizmet kutularıyla sınırlar | "dijital ve hizmet kalemi için işaretlenen kutular" | düşük |
| C-15 | 3 | 2.1.2 (geniş sınıf) ↔ 5.10.15, 5.12.18, 5.16.21, 5.25.18 | İki sütunlu ekranlar yalnız ürün sayfası ve ödeme adımıydı | Sepet, İletişim, sipariş sayfası ve hesap ekranları eklendi | düşük |
| C-16 | 3 | 3.5.9 ↔ 5.22.10 | Google dönüşünün hedefleri kapı kapalıyken yeni hesap açacak dönüşü (E-22, mesajla) saymıyordu | Üçüncü dal eklendi (K-850) | düşük |
| C-17 | 3 | 2.7.3.3 ↔ 5.28.9 | Kalıbın kapsamı e-posta değişikliğinin geri alınmasını saymıyor, istisnaları "iki ekran" diye kapatıyordu | Kapsama girdi; "üç ekran" | düşük |
| C-18 | 3 | 2.2.1 ↔ 2.2.7, 5.13.3 | "Vitrinin her sayfası beş bölge" — ödeme adımı sadeleşmiş çerçevedir | "ödeme adımı dışında" | düşük |
| C-19 | 3 | 2.2.2 ↔ 2.9.3, 6.1.5.1 | Duyuru şeridi "yalnız Yayında" — taslak duyuru yöneticiye etiketle görünür | Ziyaretçi / yönetici ayrımı | düşük |
| C-20 | 3 | 5.16.5, 5.16.12, 6.2.16.9, 7.1.7.1 ↔ 2.12.3.3; `03 §1.11.12` | Beyanda (iptal, fesih, cayma) verilen IBAN'ın sipariş sayfasında nerede görünüp düzeltildiği yazılı değildi; alan yalnız istekle açılan alan olarak anlatılıyordu | Dört yer: beyandaki IBAN da kalemin alanında görünür ve işlenene kadar düzeltilir | **orta** |
| C-21 | 3 | 5.15.1 ↔ 5.15.3 | Hatırlatmanın "Giriş yap" bağlantısı çıkış listesinde yoktu | Eklendi (3.1.4, §3.6.3) | düşük |
| C-22 | 3 | 3.3.12, 5.1.1, 5.2.1 ↔ 2.7.1.2 | Boş hâl satırlarının E-01/E-02'ye dönüşü §3'te ve hedef ekranların girişinde yoktu | 3.3.12'nin "Nereden"ine altı ekran; iki giriş listesi | düşük |
| C-23 | 3 | 2.5.6 ↔ 5.5.3 | E-05 bileşeni kullanır ama "Bu ürün artık satılmıyor" bilgisi vitrin işaretlerinde yoktu | Metinli durum bilgisi olarak, rozet olmadığı yazılarak eklendi | düşük |
| C-24 | 3 | 5.4.16 ↔ 5.4.15, 6.2.4.9 | "Aktör farkı yalnız …" yönetici önizlemesini saymıyordu | "Misafir alıcı ile üye arasındaki fark"; önizleme 5.4.15'te | düşük |

### 3.2 Arayüz Tanımları'nın iç tutarlılığı — panel tarafı (K-849'un devri)

| # | Kontrol | Yer | Bulgu | Kapanış | Seviye |
|---|---|---|---|---|---|
| C-25 | 3 | 2.4.2.1, 2.4.2.2 ↔ 9.12.7 | Sekiz işlem "siparişe dokunan" diye sınıflanmıştı; üye hesabının silinmesi siparişe dokunmaz | "`03 §1.6.1`'in sekiz işlemi — yedisi siparişe dokunur"; 2.4.2.2 "`03 §1.6.1`'in dışında" | düşük |
| C-26 | 3 | 2.13.3 ↔ 9.13.3 | Bütün panel listeleri 25'lik sayfa; K-843'ün istisnası (kurumsal içerik grupları sayfalanmaz) kuralda yoktu | İstisna yazıldı; Kaynak'a K-843 | **orta** |
| C-27 | 3 | 2.5.1.1–2.5.1.3 ↔ 9.12.6, 9.13.4 | Rozetlerin "Nerede görünür" listeleri E-40'ın bağlı siparişlerini ve E-41'in ilgili ürünlerini saymıyordu | Eklendi | **orta** |
| C-28 | 3 | OB-06 tablosu, 2.6 ↔ 9.12.7 | E-40'ın geri alma bağlantısının bitiş tarihi bileşenin kullanım kümelerinde yoktu | Eklendi | düşük |
| C-29 | 3 | §3.4 ↔ 9.25.1, 9.21.1, 6.3.25.3 | E-54'ün iniş adımlarından E-50/E-29'a dönüş ekranlarda ve matriste vardı, §3.4'te satırı yoktu; 9.21.1 bu girişi hedefi E-54 olan 3.5.6'ya bağlıyordu | **3.4.23** açıldı (3.3.35'in eşi); 9.21.1 ve 9.25.1 ona işaret eder | **orta** |
| C-30 | 3 | 6.3.16.3, 6.3.16.1 ↔ 9.16.3, 9.16.8, 2.8.2 | Yalnız koşul eksiğiyle kapalı satışta matris "Satışı yeniden aç" düğmesini yazıyordu; ekranda düğme "Satışı geçici olarak kapat"tır | 6.3.16.3 geçici kapatma hâline daraldı; **6.3.16.6** (yalnız koşul eksiği) açıldı; 6.3.16.1'e anahtar | **orta** |
| C-31 | 3 | §4.2 E-36 ↔ 9.8.4, 2.13.2 | Amaç süzgeçleri "durumuna göre" diye eksik sayıyordu — cross-review'ın 2.13.2'de düzelttiği eksiğin dönüşü | İki eksen, bekleyen iş ve işaret | düşük |
| C-32 | 3 | 9.22 bilgi hiyerarşisi ↔ 9.22.3–9.22.6 | "Dönem seçimi ve dört bölge" — ayrıntı dönem seçimi dahil dört bölge sayar | "dört bölge: dönem seçimi ve üç bölge" | düşük |
| C-33 | 3 | 2.12.4.1, 2.12.4.3, 9.9.36 ↔ 9.9.31, 7.2.14; `03 §8.4.5` | Durum düzeltmesi onaylı işlem gibi anlatılıyordu ("onayı açılmaz"); düzeltmeler onay istemez | Firma iptalinde onay, durum düzeltmesinde form | **orta** |
| C-34 | 3 | 2.7.3.3 ↔ 9.12.14, 9.13.16, 9.16.13, 9.17.14, 9.18.15, 9.20.16 | Yöneticinin bütün onaylı işlemleri "siparişin güncel hâline" götürülüyordu; siparişe dokunmayanlar kaydı ya da ayarı yeniler | "işlemin dokunduğu kaydın güncel hâline" (C-17 ile aynı madde) | düşük |
| C-35 | 3 | 2.8.3 ↔ 9.3.4, 9.16.3, 9.19.3 | Kapının panel yüzü tek yer sayılıyordu; ayrıntı üç yerde gösterir | Üç yer sayılır | düşük |
| C-36 | 3 | 2.5.3.1 ↔ 9.9.7 | Ayıp talebinin satırındaki işaret E-37'de de görünür; tablo yalnız E-38'i sayıyordu | Eklendi | düşük |
| C-37 | 3 | 6.3.1.1–6.3.1.3, 6.3 girişi, §4.2 girişi, 6.1.3 ↔ 9.1.1, konvansiyon 7 | Panel girişinin aktörü ekran tanımında Yönetici, matriste Kullanıcı'ydı | Girişte Yönetici; davet, sıfırlama ve bağlantı adımlarında Kullanıcı | düşük |
| C-38 | 3 | 2.6.3 ↔ 9.9.5, 6.3.9.21, 9.8.7 | Panel sürelerinin kapalı listesi hizmetin ifa süresini (Z-39) ve havalenin son ödeme gününü (Z-8) saymıyordu | Eklendi | **orta** |
| C-39 | 3 | 2.10.1, 2.10 ↔ 9.19.8 | Bölüm başına eşzamanlı düzenleme uyarısı yalnız E-46'da sayılıyordu; E-47'nin iki metni de bölümdür | E-47 eklendi | düşük |
| C-40 | 3 | 2.10.1 ↔ 9.7.7; K-800 | Kayıt varyantı kuponu saymıyordu; K-800 kupon formunu sayar | "kupon dahil katalog kayıtları"; Kaynak'a K-800 | düşük |
| C-41 | 3 | 6.3.19.1 ↔ 9.19.7 | İlk kurulumda yayına alma iki metin için "yok" yazılıydı; tamamlanma kapısı yalnız aydınlatma metnindedir | Aydınlatmada yok, çerez politikasında var | düşük |
| C-42 | 3 | §3.2 kapanış notu ↔ 2.2.8, §4.2 E-50 | Menüde yeri olmayan ekranlar E-50'yi saymıyordu — üst satırdadır | Eklendi (3.2.21) | düşük |
| C-43 | 3 | 2.4.3 ↔ 9.9.19, 9.9.20; K-798 | Onaydan önceki uyarılar üç sayılıyordu; kanuni faiz uyarısı ve teyit hatırlatması da gösterilir | İkisi eklendi | düşük |

### 3.3 Yansıma kaçakları — geri beslenen kuralların öteki yüzleri (K-652, betik 1)

| # | Kontrol | Yer | Bulgu | Kapanış | Seviye |
|---|---|---|---|---|---|
| C-44 | 3 | `04 §1.1` (`03 §8.4.8`) | "kalem ve tarih" iki kayda birden okunuyordu; caymanın adedi yoktu (K-787, K-796) | Caymada kalem, adet, tarih; fesihte seçim yok | düşük |
| C-45 | 3 | `04 §1.1` (`03 §2.8.1.2`, §2.9.2) | "Kalem seçimi" — adet yok (K-787, K-793) | Kalem ve (ayıplı) adet | düşük |
| C-46 | 3 | `02 §9.2` B-16 · `03` 7.1.36 · `04 §1.1` (`03 §7.1.36`) | Ret teslim alınan adetle yapılır (K-794); B-16 reddedilen adedi saymıyordu | Üç yüzde adet; Kaynak'a K-794 | düşük |
| C-47 | 3 | `04 §1.1` KP-21…KP-24 | Dört özet kısmi adet kararından önceki hâldeydi | Adet, ardışık işlem, fesihte seçim yok | düşük |
| C-48 | 6 | `02 §1.2` Ayıp talebi | Kanal teslim işaretinden açılıyor gibiydi (çakışma taramasının A-3'ü §7.1.1 ve §7.5.1'i düzeltmişti); ayıplı adet yoktu | Ayıplı adet ve kanalın açıldığı an; K-793, K-830 | düşük |
| C-49 | 3 | `04 §1.1` (`03 §2.4.6`) | Onay özetinde cayma özeti yoktu (K-831; R-6'nın kardeşi) | Eklendi | düşük |
| C-50 | 3 | `04` 6.2.13 ↔ 5.13.9 (a) | K-837'nin hâli (sepete bağlı ödenmemiş önceki sipariş) matriste yoktu | **6.2.13.14** açıldı | düşük |
| C-51 | 6 | `02 §2.2` adım 4, §12.1.7 | Cayma özeti ve okunabilirlik alt sınırı (K-831, K-832) ana akışta ve yasal uyumluluk özetinde yoktu | İkisine eklendi; Kaynak'a K-831, K-832 | düşük |
| C-52 | 6 | `03` 2.10.1.4 | C-51'in Kullanıcı Akışları'ndaki aynası | Eklendi; Kaynak'a K-831 | düşük |
| C-53 | 3 | `03` 8.7.1.2 | Veri toplayan girişlerin kapısı yalnız aydınlatma metnine bağlıydı (K-850 öncesi) | İki metin; Kaynak'a K-850 | **orta** |
| C-54 | 3, 7 | `04` 9.16.3, 9.19.3 | Yöneticiye gösterilen satırlar kapıyı yalnız aydınlatmanın yayınına bağlıyordu — aydınlatmayı önce yayına alan yönetici girişlerin açıldığını sanırdı | İki cümle K-850'ye; çerez politikası bölümü kapıyı ve çerez yazılmadığını söyler | **orta** |
| C-55 | 3 | `04 §1.1` (`03 §3.4.12`, §3.5.3.9, `02 §3.32.8`) | Üç özet K-517 hâlindeydi | K-850'ye hizalandı | düşük |
| C-56 | 3 | `04 §1.1` (`03 §2.6.4`) · `10 §2` KP-18 | Sipariş sayfasının içeriğinde gerçekleşmiş geri ödemeler yoktu (K-826) | İkisine eklendi; KP-18'in Kaynak'ına K-826 | düşük |
| C-57 | 3 | `04 §1.1.6` (`02 §5.9`), §1.2 | K-732'nin `02 §5.9`'a eklediği müşteri yüzünün ekranları (E-17, E-18, E-25) eşlemede yoktu | Eşlendi; §1.2'de üç sayı | düşük |
| C-58 | 6 | `10 §2` KP-5 | Üretim yeri yalnız fiziksel üründe sayılıyordu; dijital üründe doluysa görünür (`02 §3.8.5`; FORM-7) | Hizalandı | düşük |
| C-59 | 6 | `02 §4.3` · `03` 4.2.4, 4.2.5 | Sayaçların dönüşü kalem birimiyle yazılıydı (K-791 adet) | Adet kadar; Kaynak'a K-791 | düşük |
| C-60 | 6 | `02 §6.1.1`, §9.1.6 | Yeniden gönderim "işaretli satırda" — siparişin ayrıntısındadır (K-761; S-5 `03`'ü düzeltmişti) | "işaretli siparişin ayrıntısından"; Kaynak'a K-761 | düşük |
| C-61 | 6 | `02 §8.2` L-9 · `03` 6.1.1.9 | "Davetin yeniden gönderilmesi" — işlem yok, yol yeni davettir (K-759) | "Aynı adrese yeni davet … limite girmez"; Kaynak'a K-759 | düşük |
| C-62 | 6 | `03` 4.1.29 | Yeniden gönderim davet satırını da kapsar gibiydi | Siparişte ayrıntıdan; davette yol yeni davet | düşük |
| C-63 | 3 | `03` 7.2.29, 7.3.20 · `04 §1.1` iki özet | "Ulaşmayan her e-posta işaret düşürür" genellemesi (S-1/S-2'nin sınıfı) | Müşteri bildirimi · davet · firma bildirimi · hesap e-postası ayrımı | düşük |
| C-64 | 3 | `04 §1.1` beş özet (`03 §1.7.3.1`, §1.11.36, §7.1.37, §8.9.2, §10.1.2.6) | Çakışma taramasının S-3…S-5'te düzelttiği satırların özetleri eski metindeydi | Güncel metne | düşük |
| C-65 | 6, 7 | `02 §3.1.6`, §10.1.2 | Kapı kuralı var olmayan "yasal metnin yayından çekilmesi"ni sayıyordu (K-818) | Yayından çekilmez — değişiklik yeni sürüm; §10.1.2'nin yapamadıklarına | düşük |
| C-66 | 3 | `04 §1.1` (`03 §1.11.28`, §8.3.2.1) | Teslim alma ayıpta sözleşmeden dönmeyi (K-808) ve adedi (K-794) saymıyordu | Eklendi | düşük |
| C-67 | 6 | `02 §1.2` "Mal dönmedi" kapanışı, İade, İade reddi | Üç terim K-794 öncesi kalem düzeyinde tanımdaydı | Adetle; K-794 | düşük |
| C-68 | 3 | `04 §1.1` on özet (`03 §1.7.1.2`, §1.7.1.6, §1.7.1.8, §1.7.3.5, §1.11.21, §1.11.30, §8.3.1.1, §8.3.2.2, §8.3.2.3, §8.4.7) | Firma tarafının özetleri K-794 öncesi metindeydi; ikisi kaynağın tersini söylüyordu ("kalemi kapatır", "kalem düzeyinde") | Adetle (K-794; 8.3.2.2'de K-791) | düşük |
| C-69 | 6, 7 | `02 §10.1.2` Katalog | Yöneticinin yapamadıkları ürünlerin elle sıralanamamasını sayıyor, kategorilerinkini (K-805) saymıyordu | Eklendi; K-805 | düşük |
| C-70 | 6 | `10 §2` KP-47 | Teslim alma yalnız caymaya bağlıydı; ayıpta sözleşmeden dönme de bu adımla işler (K-808, `02 §7.5.3`) | Eklendi; Kaynak'a K-808, `02 §7.5.3` | düşük |
| C-71 | 6 | `10 §2` KP-53 | Ciro tanımı geri ödemelerin düşülmediğini söylemiyordu (K-821) | Eklendi; Kaynak'a K-821 | düşük |
| C-72 | 3 | `04` 6.3.19.3 · `03` 8.7.4.1 | Üretilen metinlerin sürüm tetiği yalnız kimlikti; kargo ücreti satırı sürüm artışını yazmıyordu (YASAL-4; `02 §3.33.3`) | Beş besleyen ayar; 8.7.4.1'e sürüm artışı | düşük |

### 3.4 Kaynak satırları (K-652, betik 2)

| # | Kontrol | Yer | Bulgu | Kapanış | Seviye |
|---|---|---|---|---|---|
| C-73 | 7 | `03` 9.1.1 | Gövde K-850'yi anıyor, satırın Kaynak sütunu anmıyordu — Aşama 3'ün tek eksik atfı | Eklendi | düşük |
| C-74 | 7 | `04` Kaynak satırları — §5.16, §5.17, §5.20, §5.22, §5.25, 6.2.16.1, §9.3, §9.9, §9.16, §9.19, §9.20, §9.22 | Gövdede anılan Aşama 1–2 kararlarının on dokuz atfı Kaynak'ta yoktu (K-404, K-486, K-505 ×2, K-518, K-588, K-593, K-628, K-659, K-664, K-673, K-683, K-693, K-695 ×2, K-696, K-710, K-716 ×2) — cross-review 5. turunun betiği yalnız Aşama 3 kararlarına bakmıştı | On iki satır tamamlandı | düşük |
| C-75 | 7 | `02 §4.2` Z-35 · `03` 8.3.2.1 | Aşama 2 satırlarının Kaynak sütununda K-656 ve K-716 yoktu (CP02'nin kaçırdığı) | Eklendi | düşük |

### 3.5 İzlenebilirlik matrisi — `03 §3.2.1.8`–§3.2.1.9 → E-12 (K-849'un devri)

| # | Kontrol | Yer | Bulgu | Kapanış | Seviye |
|---|---|---|---|---|---|
| C-76 | 3 | `04 §1.1` (`03 §3.2.1.8`, §3.2.1.9), §1.2, 5.12'nin Kaynak'ı | İki satır E-04 ve E-12'ye eşlenmişti; uyarılar yalnız E-04'te yazılıydı. **Kaynakta:** `03` 2.3.1 ve `02 §3.12.4`, §3.12.9 uyarıyı sepete eklerken verir; eklemek ürün sayfasındadır (5.4.10), sepette dijital kalemin adet alanı yoktur (5.12.4) — iki uyarı E-12'de görünmez | **E-12 eşlemeden çıktı** (en küçük düzeltme: kaynakta görünmüyor); §1.2'de E-12 46 → 44 (`03` §3 12 → 10); 5.12'nin Kaynak aralığı §3.2.1.1–§3.2.1.7, §3.2.1.10, §3.2.1.11 oldu | düşük |

**Sayım:** yetmiş altı bulgu (C-1…C-76) — **orta 12** (C-1, C-3, C-8, C-20, C-26, C-27, C-29, C-30, C-33, C-38, C-53, C-54), **düşük 64**. İç tutarlılık 43 (§3.1: 24, §3.2: 19) · yansıma kaçağı 29 (§3.3) · Kaynak 3 sınıf, 22 atıf (§3.4) · matris eşlemesi 1 (§3.5). Hepsi kapandı.

**Yansıma kaçaklarının yerleri:** altmış dokuz yer — Ürün Gereksinimleri on beş (§1.2 dört terim, §2.2, §3.1.6, §4.3 iki madde, §6.1.1, §8.2 L-9, §9.1.6, §9.2 B-16, §10.1.2 iki satır, §12.1.7) · Kullanıcı Akışları on (2.10.1.4, 4.1.29, 4.2.4, 4.2.5, 6.1.1.9, 7.1.36, 7.2.29, 7.3.20, 8.7.1.2, 8.7.4.1) · MVP Kapsamı dört (KP-5, KP-18, KP-47, KP-53) · Arayüz Tanımları kırk (§1.1'in otuz üç özeti, §1.2'nin üç satırı, 9.16.3, 9.19.3, 6.2.13.14, 6.3.19.3). **En önemli üçü:** (1) K-850'nin kapısı yöneticiye gösterilen iki satırda ve Kullanıcı Akışları 8.7.1.2'de eski hâlindeydi — aydınlatmayı önce yayına alan yönetici girişlerin açıldığını okurdu (C-53, C-54); (2) K-794'ün firma tarafı — sözlüğün üç terimi, B-16 ve on iki matris özeti adetsiz, ikisi kaynağın tersini söylüyordu (C-46, C-66…C-68); (3) yeniden gönderim — kuralın evi `02 §6.1.1` ve §9.1.6 hâlâ "işaretli satırda" diyordu, L-9 ortadan kalkmış bir işlemi adlandırıyordu (C-60…C-64).

---

## 4. Etki işaretleri

Karar kaydında şu otuz sekiz Aşama 3 kararının etki sütununa `(taslak güncellendi — vX.Y; checkpoint)` parçası eklendi: K-732, K-745, K-746, K-752, K-756, K-759, K-760, K-761, K-765, K-767, K-768, K-776, K-777, K-780, K-784, K-787, K-791, K-793, K-794, K-796, K-798, K-800, K-802, K-805, K-808, K-818, K-819, K-821, K-826, K-830, K-831, K-832, K-834, K-837, K-839, K-840, K-843, K-850. Yeni satır: **K-851** (öneriyle kaydedildi). Aşama 1 ve 2'nin salt okunur kayıtlarına dokunulmadı — C-74 ve C-75'in Aşama 1–2 kararları yalnız dokümanların Kaynak satırına girdi; kararların kuralı değişmedi, geri işaret gerekmedi. FORM-7 ve YASAL-4'ün hizalamaları (C-58, C-72) karar satırı değil audit bulgusudur; izleri `04 §1.3`'ün checkpoint tablosundadır.

---

## 5. Proje sahibinin gözden geçirme listesi

### 5.1 ⚠ işaretli kırk karar

Kaynak: [çakışma taraması §4.1](PHASE3_CONFLICT_SCAN.md) — kayıtla bire bir (40 / 40; betikle yeniden sayıldı, §Notlar). Bu kararlar proje sahibinin Aşama 3 açılışındaki talimatıyla (K-723, *"kolay soruları sorma, çok kritik konuları sor"*) sorulmadan kaydedildi ve `(öneriyle kaydedildi — ⚠)` işaretini taşır — kişisel veri, para ya da yasal hak, varlık ya da önceki bir karardan ayrılma. Oturum raporlarında yöneticiye gösterildi, **proje sahibine gösterilmedi.** **Kapı:** liste arşiv işaretinden (K-437'nin 6. adımı) önce proje sahibine tek mesajda, karar başına bir sade cümleyle ve gerekçesiz gösterilir (`INSTRUCTIONS.md` §2, "Ortak kural"); yöneticiden proje sahibine gider (§9). İtiraz gelmeyen satırın işareti `(öneriyle kaydedildi — ⚠ — gözden geçirildi YYYY-AA-GG, itiraz yok)` olur; itiraz gelen karar yeni bir karar satırıyla değişir. **Bu checkpoint listeye karar eklemedi ve listeden karar çıkarmadı** — K-851 bir düzen sabitidir (ekran kurgusu), ⚠ grubuna girmez.

**Hazır metin — proje sahibine gösterilecek liste** (konulara göre; dokümanlar adıyla):

*Ödeme adımı ve siparişin onayı*
- K-756 — Ödeme adımı tek ekrandır; sipariş özeti, iki yasal metnin tamamı, onay kutuları ve "ödeme yükümlülüğü doğar" yazan düğme aynı bölümde art arda durur.
- K-831 — Onay kutularının hemen üstünde, Ön Bilgilendirme Formu'ndan ayrı kısa bir cayma özeti durur.
- K-832 — İki yasal metin ekranda ve e-postada en az on iki punto karşılığı boyutla, küçültülmeden gösterilir.
- K-833 — Onay anında fiyat ya da kargo gibi bir fark çıkarsa müşteri iki yasal metnin kutularını yeniden işaretler.
- K-771 — Dijital ürün ve hizmet için onay kutularında Ürün Gereksinimleri'ndeki taslak metin son metin olur; kutu hangi ürünleri kapsadığını adıyla sayar.
- K-837 — Aynı sepetten ödenmemiş eski bir sipariş varken ödeme adımı onun iptal edileceğini önceden söyler.

*Müşterinin iptal, cayma ve iade işlemleri*
- K-732 — Müşteri iptalde, gecikme nedeniyle fesihte, caymada ve hesap silmede son adımda sonucu tek cümleyle görür ve onaylayarak tamamlar.
- K-785 — Bu son adımda düğme işlemin adını taşır ("Siparişi iptal et", "Cayma beyanını gönder", "Hesabımı sil" gibi).
- K-838 — Kargodaki üründen cayan müşteriye ekran önce kargonun ürünü firmaya geri götüreceğini söyler.
- K-826 — Müşterinin sipariş sayfası yapılan her geri ödemeyi tarih, tutar ve yolla (karta ya da IBAN'a) gösterir; henüz yapılmamış geri ödeme ayrıca gösterilmez.

*Bir üründen birkaç adet alındığında*
- K-788 — Bir ürünün bazı adetleri iptal edilince ya da onlardan cayılınca o adetlerin parası geri ödenir; kuponun indirimi adetlere bölünür, artan kuruş son işlemde ödenir.
- K-789 — Siparişten bir adet bile gönderilmiş ya da müşteride kalmışsa kargo ücreti geri ödenmez; hepsi kargodan önce iptal edilirse ya da hepsi iade edilirse ödenir.
- K-794 — Firma da bir ürünün bazı adetlerini iptal edebilir ve iadeyi gelen adet kadar teslim alır; firmaya yeni zorunlu adım eklenmez.
- K-796 — Gecikme nedeniyle fesihte ürün ve adet seçilmez; fesih teslim edilmemiş bütün ürünlere uygulanır.
- K-830 — Ayıp talebinde müşteri ayıplı adedi seçer; ürün kargoya verilmişse teslim işareti beklenmeden talep açılır.
- K-808 — Ayıp talebinde sözleşmeden dönülürse firma ürünü iade teslim alma adımıyla alır ve parayı ardından öder.

*Vitrin ve yasal bilgiler*
- K-747 — Firmanın yasal kimlik ve iletişim bilgilerinin tamamı ve ETBİS doğrulama bandı vitrinin her sayfasının altında, ayrıca İletişim sayfasında durur.
- K-754 — Ana sayfanın ürün vitrini yayındaki en yeni ürünleri gösterir; firma ana sayfa için ürün seçmez.
- K-774 — Vitrindeki ürün kartında sepete ekleme yoktur; renk, beden gibi seçenek ürün sayfasında seçilir.
- K-835 — Sepetteki ücretsiz kargo eşiği satırı süreli kampanya sayılmaz, tarih göstermez.
- K-770 — Firma aydınlatma metnini ya da çerez politikasını ilk kez yayına alana kadar o metnin bağlantısı görünmez; ürünün taslak metni ziyaretçiye gösterilmez.
- K-834 — Çerez politikası ilk kez yayına alınana kadar site ziyaretçiye çerez yazmaz.
- K-850 — Aydınlatma metni ve çerez politikası ikisi de yayına alınmadan üyelik, Google ile ilk giriş ve iletişim formu açılmaz.

*Müşteri hesabı ve giriş*
- K-733 — Üye hesabına bağlanmış Google girişini kendisi kaldıramaz.
- K-786 — E-posta adresini başka bir müşterinin kayıtlı adresine çevirmek isteyen üye, adresin başka bir hesapta kayıtlı olduğunu görür.
- K-839 — E-posta değişikliğini geri alma bağlantısı açılınca kendiliğinden geri almaz; geri alma ekrandaki tek düğmeyle olur.

*E-postalar*
- K-759 — Müşteriye ulaşmayan her sipariş e-postası (on altı bildirimin hepsi) panelden yeniden gönderilebilir.
- K-760 — Firmaya giden bildirim ulaşmadığında siparişe işaret konmaz; panelin ana sayfasında "firma bildirimleri ulaşmıyor" uyarısı çıkar.

*Yönetim paneli*
- K-736 — Yönetici hesabı bir ad taşır; davetli hesabını açarken yazar, sonra kendi hesabından değiştirir.
- K-737 — Paneldeki ürün listesinde ürün adıyla arama ve yayın durumuna göre süzme vardır.
- K-798 — Firma "stokta bulunamadı" diye iptal ederken, gecikmede, süresi geçmiş ya da istisnalı üründe cayma kaydederken ve karta iade gerçekleşmediğinde panel yasal sonucu söyleyen sabit uyarılar gösterir; süresi geçmiş kayıt ayrıca bir kutuyla onaylanır.
- K-805 — Kategoriler her yerde ada göre alfabetik dizilir; firma sırayı elle değiştiremez.
- K-806 — Kupon kodu tektir ve büyük-küçük harf fark etmeden çalışır; kupon silinemez, bitiş tarihi öne çekilerek durdurulur.
- K-811 — Panelde üye arandığında e-postası doğrulanmamış kayıt da bulunur; geri alma bağlantısı açıkken hesap silinemez ve ekran bunu tarihiyle söyler.
- K-814 — Ayar değişikliklerinin onayı sonucu sayıyla söyler; iade adresi değişirken eski adrese gelen ürünü teslim almanın firmanın yükümlülüğü olduğu yazar.
- K-815 — Firma kimliği ekranı satışın neden kapalı olduğunu tek tek gösterir; ETBİS alanının yanında kaydın firmanın yükümlülüğü olduğu yazar.
- K-818 — Aydınlatma metni ve çerez politikası taslakta düzenlenip "Yayına al" ile yayınlanır, yayından çekilmez; metin yalnız kalın, liste, ara başlık gibi sınırlı biçimlerle yazılır.
- K-821 — Satış özetindeki ciro onaylanan siparişlerin toplamıdır; sonradan yapılan geri ödemeler cirodan düşülmez, iptal ve iade ayrıca sayılır.
- K-823 — Dışa aktarma ekranının başında dosyadaki kişisel verinin sorumluluğunun firmada olduğu yazar; üye ve talep listesi her zaman tamamıyla iner.
- K-836 — Üye ve talep listesi yalnız panelin dışa aktarma ekranından indirilir.

*Süreç kararları — ⚠ değil, bilgi için (Arayüz Tanımları'nın ikinci model incelemesi)*
- K-848 — Arayüz Tanımları büyük olduğu için ikinci model incelemesinde her tur doküman hem tek seferde hem dört parçaya bölünerek okutuldu; tur ancak hepsi temiz dönerse temiz sayıldı.
- K-849 — İnceleme beşinci turda, kabul edilen bulgu kalmayınca kapatıldı; aynı yerde tekrar tekrar reddedilen bir itiraz sonucu belirlemedi, kalan küçük metin farklarına sıradaki genel kontrol (checkpoint) baktı — bu kontrol kırk üç küçük metin farkını düzeltti, hiçbiri bir ürün kuralını değiştirmedi.

**Sayım:** matrisin 2. oturumu 4 · workshop 5 · yazım turu 2a ve 2b 5 · kısmi adet kararı 4 · yazım turu 3.–5. oturum 11 · audit ve deep review 10 · çakışma taraması 1 · checkpoint 0 — **toplam 40**. Betikle sayıldı; kırkının da konu hücresi ⚠ işaretini taşıyor ve çakışma taramasının §4.1 listesiyle birebir.

**Gözden geçirme sonucu — 2026-10-05 (K-437'nin 6. adımı, arşiv işaretinden önce; K-854).** Liste proje sahibinin isteğiyle kapanıştan önce, yönetici aracılığıyla bu tabloyla, karar başına bir cümleyle gösterildi; K-848 ve K-849 aynı mesajın sonunda bilgi olarak. **İtiraz yok:** hiçbir karar değişmedi, yeni karar satırı açılmadı ve bu rapora retro bölümü gerekmedi. Karar kaydında kırk satırın konu hücresindeki işaret silinmedi, `(öneriyle kaydedildi — ⚠ — gözden geçirildi 2026-10-05, itiraz yok)` biçimine dönüştürüldü (Aşama 2'nin kalıbı, CP02 §5.1). Öğrenim terfisi (K-852) ve `00` v1.0.5 (K-853) yeni ⚠ karar doğurmadı. **Mekanik doğrulama:** bu tablonun 40 tekil K numarası ile dönüştürülen 40 satır birebir — eksik 0, fazla 0; konu hücresinde eski işareti taşıyan Aşama 3 karar satırı kalmadı. Kayıt: karar kaydı §10.1 (v0.95).

---

## 6. Öğrenim terfisine notlar — 5. adımın girdisi

Adayların metinleri tracker §10.1'dedir; bu checkpoint iki aday ekledi.

| # | Aday | Kaynak | Playbook |
|---|---|---|---|
| 11 | Özet cümle ile ayrıntının ayrışması düzeltme turlarında büyüyor — bir maddeyi değiştiren düzeltme o maddeyi anan özet yerleri (madde numarası, ekran kimliği) betikle bulup okumuyor; kırk üç yerin çoğu kalite döngüsünün bir kararının yalnız ekranın maddesine yazılmasından doğdu (K-840 → 3.3.39'un hedefleri; K-843 → 2.13.3) | §3.1, §3.2 | — |
| 12 | İzlenebilirlik matrisinin özet hücresi kaynak satırı değişince eski kalıyor — geri besleme ve çakışma taraması üst dokümanın satırını güncelledi, `04 §1.1`'in özetini güncellemedi (otuz üç özet; çakışma taramasının S-9'u dördünü görmüştü) | C-44…C-47, C-49, C-55, C-56, C-63, C-64, C-66, C-68 | — |

**Kanıt — mevcut adaylara:** aday 8'in ("geri besleme betiği 1 kuralın yan yüzlerini kaçırıyor") sınıfı bu checkpoint'te yirmi dokuz bulguyla yeniden çıktı — çakışma taramasının yirmi üç hizalamasından sonra; yan yüzlerin çoğu sözlük terimleri, zaman satırları (`02 §4.3`), bildirim içerik listeleri (B-16) ve matris özetleriydi.

**Tek seferlik gözlem (raporda kalır):** cross-review'ın 5. turunun Kaynak betiği yalnız Aşama 3 kararlarına baktı ve `04`'ün kendi Kaynak satırlarındaki Aşama 1–2 atıflarını (C-74) görmedi; CP02'nin betiği de iki Aşama 2 satırını (C-75) kaçırmıştı. Betik 2'nin kapsamı — Aşama 3'ün dokümanında bütün kararlar — bu raporun Notlar'ında yazılıdır.

---

*Kaynak: K-437 (kapanış sırası, 4. adım) · K-429 (çakışma taraması — girdi) · K-849 (cross-review'ın çıkış kuralı ve checkpoint'e devri) · K-652 (geri beslemenin işletimi ve iki betik — ✓ ve döngüler yeniden açılmaz) · K-29 (etki işaretleri) · K-647 (kaydın evi; Aşama 1–2 kayıtları salt okunur) · K-723 (⚠ öneriyle kayıt ve liste kapısı) · K-726 (`04 §1.3`'ün geri besleme tablosu) · K-851 (içerik liste sayfasının sayfa boyu) · `.claude/skills/checkpoint/SKILL.md` (yedi kontrol, çıktı biçimi ve koşum) · `Docs/CHECKPOINT_REPORTS/CP02_PHASE2_CHECKPOINT.md` (biçim örneği) · `Docs/CHECKPOINT_REPORTS/README.md` (rapor türü ve retro güncelleme).*
