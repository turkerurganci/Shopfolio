# Aşama 5 — Checkpoint (CP05)

**Tarih:** 2026-10-18 | **Aşama:** 5 — Veri modeli | **Kapanış adımı:** K-437'nin 4. adımı (`/checkpoint`; `checklists/document-stage.md` §6 son madde, §7 madde 4)
**Girdi sürümleri:** karar kaydı v0.141 · Veri Modeli (`06`) v0.25 · Ürün Gereksinimleri (`02`) v0.88 · Kullanıcı Akışları (`03`) v0.37 · Arayüz Tanımları (`04`) v0.35 · Teknik Mimari (`05`) v0.24 · MVP Kapsamı (`10`) v0.54 · Proje Vizyonu (`01`) v0.33 · `07`, `08`, `09`, `11`, `12` ve `DEPLOY_RUNBOOK`'un park blokları — `origin/main` @ `a8fc09b`
**Çıktı sürümleri:** karar kaydı v0.142 · Veri Modeli v0.26 · Ürün Gereksinimleri v0.89 · Kullanıcı Akışları v0.38 · Arayüz Tanımları v0.36 · Teknik Mimari v0.25 · MVP Kapsamı v0.55 · Proje Vizyonu değişmedi · `07`, `09`, `12` ve `DEPLOY_RUNBOOK`'un park satırları · çakışma taramasının raporuna retro güncelleme
**Girdi raporu:** [Aşama 5 — Çakışma Taraması](PHASE5_CONFLICT_SCAN.md) (K-437'nin 3. adımı) — açık kalem durumu, ⚠ listesinin hazır metni (§4.1), devir dizini (§6) ve checkpoint'e notlar (§8) oradan alındı. **K-1129'un devri** — Veri Modeli'nin cross-review'ının bıraktığı "iki yerin metin farkı: özet hücresi ya da genel tanım cümlesi ile ayrıntının kuralı" sınıfı — karar kaydı §14.1'in checkpoint maddesinden.

> **Ne işe yarar:** Aşama kapanmadan önce Veri Modeli'nin kendi içinde, Aşama 5'in geri beslediği dokümanlarla ve park satırlarıyla karar kaydına ve metodolojiye karşı tutarlı olduğunu doğrular; bulunan her uyumsuzluğu ya kapatır ya da adıyla proje sahibine bırakır. Sonuç öğrenim terfisinin (5. adım) ve arşiv işaretinin (6. adım, K-436 — kapsamı K-647) girdisidir.

---

## Checkpoint Sonucu — 2026-10-18

**Aşama:** 5 — Veri modeli (kapanış sırası K-437: yazım turu ✓ · kalite döngüsü ✓ · çakışma taraması ✓ · **checkpoint** · öğrenim terfisi · arşiv işareti)
**Genel durum:** ⚠ Dikkat gerektiren noktalar — 180 ham bulgunun 159'u kabul ya da kısmen kabul edildi (111 bulgu satırı: otuz orta, seksen bir düşük), yan yüz doğrulayıcısı on beş satır daha getirdi (iki orta); hepsi bu checkpoint'in PR'ında kapandı — çoğu var olan kararların yansımasıyla, dördü yeni kararla (K-1137…K-1140; üçü ⚠ — K-1137, K-1138, K-1140). Çok kritik konu yok. Kapanıştan sonra durum **✓ Yolunda**; açık kalan kapı üç yeni ⚠ kararın proje sahibine gösterilmesidir (arşiv işaretinden önce, §5).

### Kontrol Özeti

| # | Kontrol | Sonuç | Detay |
|---|---|---|---|
| 1 | Yol haritası | ✓ | Aşama 5'teyiz (`00 §C.1`: rol Data / Domain Modeler, girdi `02`, `03`, `05`, `10`, çıktı `06`; izlenebilirlik matrisi zorunlu). Açılış (PR #119), matrisin altı oturumu (#120–#125), workshop iki oturum (#126, #127), tek yazım turu yedi oturum (#128–#135), kalite döngüsü — audit ve güvence merceği (#136), deep review (#137), cross-review beş tur, 5. turda K-1129'un çıkış kuralıyla TEMİZ (#138–#142), etki yansıtma (#143) — ve çakışma taraması (#144) bu sırayla kapandı. Atlanan ya da sırası değişen adım yok (K-437, K-432, K-430). |
| 2 | Doküman durumu | ✓ | Başlık ve dosya sonu sürümleri kayıt §1 ile aynıydı (`02` v0.88, `03` v0.37, `04` v0.35, `05` v0.24, `06` v0.25 / 2026-10-18; `10` v0.54 / 2026-10-17; `01` v0.33). Şablon dokümanlarda (`07`–`09`, `11`, `12`) başlık v0.1; park bloğu eklemeleri başlığa dokunmaz. Bu PR'da altı doküman ve kayıt birlikte arttı; `06`'nın Checkpoint sütunu ✓ oldu (✓ Tamamlandı işareti arşiv adımında). |
| 3 | Tutarsızlık | ⚠ → ✓ | **K-1129'un devri** — matrisin özet hücreleri ve K anahtarlı satırları: elli yedi bulgu satırı, yirmisi orta (§3.1); genel kural cümleleri, özet tablolar ve başlık notu: on sekiz, biri orta (§3.2); güvence ↔ mekanizma: üç, ikisi orta (§3.3); geri beslemenin yan yüzleri: on dokuz, yedisi orta (§3.4); park satırları ve karar kaydının özet yüzü: on dört (§3.5); yan yüzler: on beş, ikisi orta (§3.6). Sayılar, kimlikler ve atıflar betikle tutarlı (Notlar). |
| 4 | Açık kararlar | ✓ | Tracker §4'te açık satır yok, A-19 açılmadı; Veri Modeli'nde "açık" işaretli madde yok (konvansiyon 11); Ürün Gereksinimleri §13 boş (çakışma taraması §1, §3). §14.1'in açık süreç maddeleri kapısındadır: K-1129'un devri bu checkpoint'te kapandı; ⚠ listesi — §5; öğrenim terfisi adayları 1–26 ve Ö-23 — 5. adım. Desteklenen ödeme sağlayıcısının sayısı Entegrasyon spesifikasyonları aşamasının açılışındadır (`08`'in K-932 satırı). |
| 5 | Aşama çıktıları | ✓ | `06` yazıldı ve şablonun bütün bölümleri doludur (§1.1–§1.3 matris, §2 modelleme kuralları, §3 envanter — 79 entity, §4 entity tanımları, §5 enum'lar, §6 ilişki diyagramı, §7 indeks ve kısıt özeti, §8 seed — üç sınıfıyla, §9 saklama). §6 ve §7 §4'ten betikle tutarlı (`verify67.py`: 130 bağ — §4, §7.2, §6'da 130 / 130 / 130; eksik, fazla, zorunluluk farkı yok; §7.1 ve §7.5'in dizin kimlikleri §4'te); §4'te kullanılan her enum §5'te tanımlı; 79 entity §7'de ve §9'da. Kalite döngüsünden geçti, etkileri üst dokümanlara geri beslendi (K-652). Checklist'in Veri modeli hatırlatması karşılandı: enum'lar kaynaktan birebir (§5.1), silme ve saklama erken (§2.2, §9), eşzamanlılık baştan (§2.6), denormalizasyon "nerede güncellenir" ile (§2.7). |
| 6 | Geriye dönük etki | ⚠ → ✓ | Proje Vizyonu'na etki yok. Önceki dokümanların eski kalan yüzleri §3.4 ve §3.6'da kapandı — Ürün Gereksinimleri, Kullanıcı Akışları, Arayüz Tanımları, Teknik Mimari ve MVP Kapsamı. **API tasarımı aşamasının girdileri hazır:** `00 §C.1`'in girdileri `02`–`06`, `10` — `06` dışında hepsi ✓, `06` ✓ işaretini arşiv adımında alır; `07`'nin beş park bloğu (Aşama 1–5) ve Aşama 5 bloğunun dizin satırı; çakışma taramasının §6 devir dizini (70 karar, 93 atıf) — bu checkpoint'in kararları `07`'ye iş bırakmaz. **Şablon:** API Tasarımı şablonunun "Bağımlılıklar" alanı (`02`, `03`, `04`, `05`, `06`, `10`) ve izlenebilirlik satırı ("ekran aksiyonu / akış adımı → endpoint") `00 §C.1` ile uyumludur — PF-74'ün kalıbı burada yok; açılışın şablon taraması (checklist §1) yine koşar. |
| 7 | Yeni alan/kural | ⚠ → ✓ | Kuralın evinde eksik kalan alan: aydınlatma metninin barındırma konumu bölümü (K-818) §4.56'da yoktu — `HostingLocation` yazıldı (C-10). Mekanizması yazılı olmayan güvence: K-1133 (2)'nin yeniden yazımı — K-1137 (C-76). Ürün kuralının evinde olmayan kalan risk: K-1084 (4) — K-1139 (C-82). Kaynak satırları (betik 2): `06`'nın bütün alt bölümlerinde ve üst dokümanların değişen satırlarında son koşum boş (Notlar). |

### Aksiyon Gerektiren Maddeler

- [x] 159 kabul edilen bulgu ve on beş yan yüz satırı düzeltmeyle kapandı — `06` v0.26, `02` v0.89, `03` v0.38, `04` v0.36, `05` v0.25, `10` v0.55, park satırları, karar kaydı v0.142 (§3)
- [x] Dört karar — K-1139 (öneriyle kaydedildi), K-1137 ⚠, K-1138 ⚠, K-1140 ⚠ (öneriyle kaydedildi); ⚠ listesi 34 → **37**
- [x] K-1129'un devri kapandı (karar kaydı §14.1)
- [x] Checkpoint'in dokunduğu Aşama 5 kararlarının etki sütununa `(taslak güncellendi — …; checkpoint — CP05 …)` parçası ya da yeni kararın geri işareti eklendi (§4); Aşama 1–4 kayıtlarında yalnız geri işaret
- [x] **Üç ⚠ kararın proje sahibine gösterilmesi** — K-1137, K-1138, K-1140; hazır metin §5.2. Kapı: arşiv işaretinden (6. adım) önce, tek mesajda (`INSTRUCTIONS.md` §2, §9 işletim kuralı 3). Otuz dört kararlık liste 2026-10-18'de gösterildi, itiraz gelmedi; işaret dönüşümü arşiv adımında. — **kapandı (2026-10-18, kapısında; K-1142):** üç karar ve iki düzeltilmiş cümle tek mesajda gösterildi, itiraz yok; otuz yedi işaret dönüştü, retro bölümü gerekmedi; sonuç §5.2'nin sonunda
- [x] Sırada öğrenim terfisi (K-437'nin 5. adımı) — tracker §14.1'in yirmi altı adayı (24–26 bu checkpoint'te eklendi, §6) ve Ö-23 — **kapandı (2026-10-18, PR #146, K-1141)**

### Notlar

**Yöntem.** Mekanik kontroller betikle koştu: §6/§7 ↔ §4 (`verify67.py`, §4'ün ayrıştırması güncel metinden yeniden üretilerek), K atıfları, enum'lar, entity listeleri (`checks.py`), Kaynak satırları (K-652'nin betik 2'si — `kaynak2.py`), güvence envanteri (`gscan.py`), matris kurulduktan sonra değişen kaynak birimleri, geri besleme tablosu, ⚠ listesi ve K tekilliği. Her betik başlamadan iki yönlü sınandı: Kaynak betiği bilerek eklenen üç sahte K'yı (gövde paragrafı, Kaynak sütunu olan tablonun başka hücresi, §5.6 satırı) yakaladı; ilk sürümü tablonun Kaynak sütunundaki K'ları eksik sayıyordu (109 yanlış eksik) ve satırın Kaynak hücresini okuyacak biçimde düzeltildi; güvence envanteri v0.16 ile bugünkü metnin farkını aldı ve K-1135 ile K-1133'ün bilinen cümlelerini buldu. Anlamsal tarama on beş salt okuma merceğiyle, ayrı alt ajanlarda, ön planda, üç beşli dalgada koştu; her mercek bulgusunu bulduğu anda dosyaya yazdı: **MA** — matrisin §1.1.3–§1.1.4'ü (270 satır; 9 bulgu) · **MB** — §1.1.5 (320; 20) · **MC** — §1.1.6–§1.1.7 (314; 19) · **MD** — §1.1.8 (391; 12) · **ME** — §1.1.9–§1.1.10 (405; 16) · **MF** — §1.1.11'in K anahtarlı satırları, park maddeleri ve gövde cümleleri (213; 14) · **MG** — §1, §1.1.2, §1.1.12, §1.2, §1.3 (376; 10) · **I1** — başlık notu, §2, §3 (474; 12) · **I2** — §5–§8 (1.224; 4) · **I3** — §9 ve üst doküman kapalı listeleri (139 + listeler; 8) · **G** — güvence ↔ mekanizma, merceğin v0.16'dan sonra koşmadığı 347 birim (283 güvence: 273 tutuyor, 9 kısmen, 1 tutmuyor; 4) · **F1**, **F2** — geri beslemenin yan yüzleri, 57 satırın kuralları, yaklaşık 4.900 eşleşme (15, 13) · **P1** — #144'ün değiştirdiği 47 park satırı (13) · **P2** — #144'ün 89 karar satırı, 65 geri işaret, K-1075…K-1136'nın 143 sürüm damgası, §14.1 (11). 180 ham bulgu beş şüpheciye (V1–V5, Opus) gruplanarak verildi; her şüpheci bulguyu güncel metne karşı yeniden okudu, çürütmeye çalıştı ve kabul edilen her bulgu için birebir şartname yazdı (eski metin dosyada tam bir kez — betikle). Şüphecilerin bıraktığı 47 yan yüz ayrı bir doğrulayıcıya (Opus) verildi; ilk deneme arka planda başlatılmıştı ve aynı kapsam ön planda yeniden koşuldu — iki doğrulayıcı yedi maddede ayrıştı, yönetici ön plandakinin kümesini seçti (§3.6). Şartnameler beş Sonnet ajanınca dosya başına uygulandı (`06` · `02`–`05` · park satırları ve çakışma raporu · karar kaydı · yan yüzler); her ajan eski metnin tek eşleşmesini denetledi. **Model dağılımı:** mercekler, şüpheciler ve yan yüz doğrulayıcısı Opus; uygulama ve §3'ün tablo taslağı Sonnet; sayım ve karşılaştırma betikleri yöneticide. Ortam: Git Bash ve Python 3.12 (`python -I -X utf8`).

**Son şüpheci.** Uygulamadan sonra bir Opus doğrulayıcı diff'i güncel metne karşı okudu (yaklaşık 250 değişen satır gözle, 519 tablo satırı ve 81 karar satırı betikle): **10 bulgu — 1 orta, 9 düşük**; hepsi bu PR'da uygulandı, yeni karar istemedi. Orta bulgu: otuz dört kararlık liste çakışma raporunun eski cümleleriyle gösterilmişti — K-1092 ve K-1135'in düzeltilmiş cümleleri §5.2'nin mesajında yeniden gösterilir. Düşüklerin altısı sürüm notlarının bölüm listesiydi (öğrenim adayı 25'in bu PR'daki kanıtı), ikisi Kaynak hücreleri, biri biçim, biri K-1133'ün geri işaretinin ilişki sözü ("değiştirilir"). Temiz dönenler: K-1137…K-1140'ın bütün kolları hedef dokümanlarda aynı sözle; yedi orta düzeltme kümesi şartnameyle aynı; raporun sayıları.

**§1.2'nin sayıları.** Matrisin "Entity" sütunundaki düzeltmeler §1.2'nin (geri izlenebilirlik) aday başına satır sayılarını değiştirdi; `06 §1.1.1`'in `geri()` betiği önce değişiklikten önceki metinde koşup bugünkü değerleri ürettiği doğrulandı (2.915 çift), sonra güncel metinde yeniden koştu: **2.990 çift**, yirmi beş aday değişti; durum dağılımı (aday taşıyan 1.282 / veri doğurmaz 289) yalnız `02 §6.6.10` satırının "eşlendi"ye dönmesiyle değişti. §1.2'nin altına tarihli paragraf girdi.

**Betik 2 — Kaynak satırları.** `06`'da bütün alt bölümler; `02`, `03`, `04`, `05`'te bu PR'ın ve #136–#144'ün değiştirdiği satırlar. Son koşum boş. Kapsam dışı: bölüm girişleri (`06` §3, §5, §8'in giriş cümleleri — K'ları alt bölümlerin Kaynak satırındadır), `05 §12`'nin gerekçe hücreleri (kaynağın kendisi), `06 §1.1`'in matris satırları (kaynak hücrededir), Aşama 1–4'ün değişmemiş satırları.

**Betikle doğrulanıp tutarlı çıkanlar:**

- **⚠ listesi:** konu hücresi `(öneriyle kaydedildi — ⚠` taşıyan K-992…K-1140 satırları **37**; §14.1'in listesi 34 + checkpoint'in üçü — iki yön boş.
- **K tekilliği:** komut boş; en büyük numara K-1140; karar satırlarının hepsi altı sütun.
- **Entity ve şema:** 79 entity §3.1'de, §7'de, §9'da; 130 bağ; §4'te kullanılan enum'ların hepsi §5'te.
- **Sürümler:** tracker §1, başlıklar ve dosya sonu notları birebir; bu PR'da birlikte arttı.
- **Değişen dosyalar:** CR, kontrol karakteri ve `$[0-9]` artığı sıfır.

**İşlenmeyenler — düzeltme gerektirmedi:** (1) `03` 1.6.2.2'nin parantezi ve matrisin `03 §1.6.2.2` satırı yalnız "sonucu bekleniyor"u anar — genel koşul `02 §10.4.6`'dadır, "istek gönderiliyor" teknik bir hâldir. (2) `02 §5.6.5`'in "iptal kaydı" sözü ürün dilidir; verinin yeri K-1024 (2)'dedir. (3) Gecikme feshinin kapanış kenarı — bütün adetleri ayıpta sözleşmeden dönmeyle biten kalem açık adet taşır ve fesih kapanışı işlemez; çıkışı S6'dır (firma teslim işaretini koyar), K-1104 kapanış kolunu eledi; kenarın düğme yüzü K-1140'la kapandı. (4) `DEPLOY_RUNBOOK`'un K-1081 · K-1133 · K-1137 satırının öznesi ve `06` matrisinin `03 §1.4.4.4` satırının "iptal kayıtlarından" sözü — paralel yan yüz doğrulayıcısının iki ek önerisi; ön plandaki doğrulayıcı üslup ve bağlamda doğru saydı. (5) Merceklerin "İşlenmeyenler" bölümlerindeki öteki gözlemler çalışma dosyalarındadır; hiçbiri ürün kuralını ya da güvenceyi değiştirmez.

---

## 3. Bulgular ve kapanışları

Seviye: **orta** — iki yer çelişiyor; kapalı liste, sayı ya da hedef yanlış; güvence mekanizmasıyla tutmuyor; ya da source-of-truth'ta bir kural eksik; **düşük** — ifade, kapsam ya da atıf farkı, anlamı değiştirmez. Kontrol sütunu skill'in yedi kontrolüne gider. Yer sütununda önsüz numara Veri Modeli'nin maddesidir; matris satırı "matris `02 §3.17.8`" diye yazılır; öteki dokümanlar numarasıyla gelir. Mercek kimlikleri çalışma dosyalarınındır.

### 3.1 Matrisin özet hücreleri ↔ kaynak ve entity ayrıntısı (devredilen sınıf — K-1129)

| # | Kontrol | Yer | Bulgu | Kapanış | Seviye |
|---|---|---|---|---|---|
| C-1 | 3 | §3.2 aday 25, KP-21; matris `02 §1.2` Cancellation, `02 §3.17.8`, §5.3.4, §5.4 m4, §5.5 m5, §5.6.5, §6.2.15, §7.2.4, §9.2 B-7, §12.5 m15; matris `03 §1.3`, §1.11.1, §2.4, §4.2.1, §4.2.3, §6.3, §10.1 ↔ §4.19, §4.22, §4.32 | Ödeme onayından önce iptal edilen siparişin sebebi ve kendiliğinden iptalin türü iptal kaydına (Cancellation) yazılıyor sayılıyordu. §4.22 bu kaydı yasaklar; ikisi siparişi kapatan geçiş kaydındadır ve ödeme kaydı yazılmaz; §3.2 bu satırı saymıyordu (MA-6, MB-9, MC-1, MC-4, MD-12, ME-1) | §3.2'ye aday 25 satırı; hücreler geçiş kaydına çevrildi — ortak söz "kalem kaydı yazmaz, sebebi siparişi kapatan geçişin kaydındadır" (K-1024 (2), K-1050) | **orta** |
| C-2 | 3 | matris `02 §1.2` GuestBuyer, `02 §3.17.8` ↔ §4.17 `IsGuestOrder` | "Hesap bağı boş sipariş" yazılıydı; hesaba düşen misafir siparişinde bağ doludur ve sipariş girişsiz kalır. E-posta düzeltmesi ve L-7/L-8 bu işaretten okunur (MA-2, MB-9) | Alan `IsGuestOrder`, hesap bağından ayrı; e-posta ekseni yalnız girişli siparişi sayar (K-579) | **orta** |
| C-3 | 3 | matris `02 §2.2` m6, `02 §3.20.6`, §3.20.7, §3.21.6, `02 §4.2` Z-10, §4.3 m2 ↔ §4.17 | Ödeme onayı ve kargoya verilme anları Order'ın alanı gibi yazılmıştı; §4.17 bunları geçiş kaydından okur, saklamaz (MA-3, MB-10, MC-13) | Entity'ye geçiş kaydı; "geçiş kaydından okunur, saklanmaz" (K-1046) | düşük |
| C-4 | 3 | matris `02 §2.2` m6, `02 §6.1.4` | "Sepet boşalır" kaynaktan geniştir: yalnız siparişe giren kalemler ödeme başarısında sepetten çıkar (MA-3, MC-6) | "Siparişe giren kalemler sepetten çıkar" (`02 §3.16.12`; K-1091 (2)) | düşük |
| C-5 | 3 | matris `02 §3.17.3`, `02 §4.3` m2, matris `03 §1.4.4`, §1.4.4.2, §1.4.4.7, §1.4.4.8, §1.6.2.2, §1.10.3, §4.2.2; §1.1.11 K-566 ↔ §4.14 | Hücreler ayırmanın "hâli" olduğunu ve düzeltmede ayırma satırının yeniden yazıldığını söylüyordu; §4.14'te hâl alanı yoktur — satırın varlığı ayırmadır, düzeltmede satır yazılmaz (MB-8, ME-6, MF-8, MC-13) | Hücreler "satırın silinmesi" diliyle; düzeltmede "ayırma satırı yazılmaz" (K-1035 (1)) | **orta** |
| C-6 | 3 | matris `02 §5.6.1`, matris `03 §1.4.1` ↔ §4.17, §7.3 | İki eksenin yasak çiftleri "uygulama düzeyi" yazılmıştı; K-1046'dan beri kontrol kısıtıdır (MC-2, ME-4) | "Yasak çiftler kontrol kısıtı; izinli çiftin geçişi geçiş biriminin koşulu" (K-1046) | **orta** |
| C-7 | 3 | matris `02 §3.20.9`, `02 §5.8` m1 ↔ §4.18 | Teslim işareti `DeliveryDate` diye yazılmıştı; işaret `DeliveryMarkedAt`'in doluluğudur, `DeliveryDate` teslim tarihidir (MB-11, MC-15) | İki alan ayrıldı (K-608) | **orta** |
| C-8 | 3 | matris `02 §3.17.7`, `02 §5.6.4`, `02 §6.2.2` ↔ §4.17 | `CartId` "sepet bağı" diye yazılmıştı; değerdir, bağ değil (§2.2.6) (MB-12, MC-14) | "Değer, bağ değil" (K-1086) | düşük |
| C-9 | 3 | matris `02 §3.16.7`, matris `03 §1.8.3` ↔ §4.16 | "Kalemin silinmesi" yazılıydı; vitrinden kalkan kalem önce `RemovedProductName` ile işaretlenir, gösterildiği istekte silinir (MB-7, ME-11) | Hücreler işaret–silme sırasıyla (K-1044 (3)) | **orta** |
| C-10 | 7 | matris `02 §3.33.2` ↔ §4.56 | Aydınlatmanın barındırma konumu bölümü ayrı ve zorunlu alandır (K-818; `04` 7.2.22.2, 9.19); §4.56'da yoktu ve "bölüm dolu"nun neyden okunduğu yazılı değildi (MB-18) | §4.56'ya `HostingLocation` (sürüm satırında, yayına almayla donar); Kısıtlar, §7.3, §3.1; metnin tamamlanmış sayılma koşulu (K-818, K-360, K-1027 (1)) | **orta** |
| C-11 | 3 | matris `02 §3.33.6`, `02 §6.1.1` | Yeniden gönderimin belgesi yalnız sürüm kaydı gibi yazılmıştı; belge siparişin donmuş kopyasıdır (MB-19) | Donmuş kopya diliyle (K-1027 (4), (5)) | düşük |
| C-12 | 3 | matris `02 §3.34.8` | Kaybolamaz kayıtların Entity listesi kalem ve kalem kayıtlarını düşürüyordu (MB-20) | Listeye eklendi (K-557, K-1010) | düşük |
| C-13 | 3 | matris `02 §3.9.2`, `02 §4.2` Z-20 ↔ §4.12 | Başlangıç ve bitiş "an"ları sayılıyordu; §4.12 tarih saklar (MB-1) | "Tarih" (K-1023 (4)) | düşük |
| C-14 | 3 | matris `02 §3.8.2`, `02 §3.8.3` ↔ §4.18 | Tutar ve matrah donmuş alan gibi sayılıyordu; §4.18 ikisini donmuş değerlerden türetir (MB-2) | "Türetilir" (K-1015 (4)) | düşük |
| C-15 | 3 | matris `02 §1.2` ReturnPendingMark ↔ §2.7.3 | İşaretin girdileri yalnız teslim alınan adet sayılıyordu; ret ve açık "mal dönmedi" kapanışı da işareti kaldırır (MA-1) | Dört kayıt (K-1015, K-1024 (6)) | **orta** |
| C-16 | 3 | matris KP-18 ↔ §4.19 | "İptalin türü"nü taşıyan kayıt Entity'de yoktu; değer `TransitionReason`'dır (MA-7) | `TransitionReason` (K-1050) | düşük |
| C-17 | 3 | matris `02 §1.2` AccountType ↔ §4.39, §4.44 | Enum'un sütunu Session sayılıyordu; tek sütun giriş kaydındadır (MA-8) | Giriş kaydı (K-1062 (3), K-1025 (7)) | düşük |
| C-18 | 3 | matris `02 §1.2` LegalTextVersion, `02 §3.33.3` | Sürümün artış tetikleri yalnız üretilen metinleri sayıyordu; düzenlenen metinde yayına alma da artırır (MA-9) | Tetik eklendi (K-1027 (1), K-1071) | düşük |
| C-19 | 3 | matris `02 §3.1.3` ↔ §4.1 | Ad-soyad `LegalName`'den ayrı alan gibi sayılıyordu (MB-5) | Hücre düzeltildi (K-1039 (1)) | düşük |
| C-20 | 3 | matris `02 §3.1.7`, `02 §6.9.16`, `02 §7.4.4`, `02 §12.5` m29 ↔ §4.30 | İade adresi yalnız cayma beyanına donuyor yazılıydı; ayıp talebine de donar (MB-6, MC-17, MD-4) | "Cayma beyanına ve ayıp talebine" (K-665, K-1024 (5)) | düşük |
| C-21 | 3 | matris `02 §3.24.1` ↔ §4.21, §5.6.2 | ÖBF ve MSS kutuları onay kutusu kaydına yazılıyor sayılıyordu; kayıt yalnız dijital ve hizmet kutusunu tanır (MB-15) | Hücre düzeltildi (K-1027 (7)) | düşük |
| C-22 | 3 | matris `02 §3.24.3` | Belgenin kopya mı sürümden mi geldiği sorusu açık duruyordu; K-1027 (4) kapattı (MB-16) | Açık soru kalktı (K-1027 (4)) | düşük |
| C-23 | 3 | matris `02 §3.13.16` ↔ §4.37 | Yönetici hesabı için "doğrulama" alanı sayılıyordu (MB-17) | Alan çıkarıldı (K-1062 (7)) | düşük |
| C-24 | 3 | matris `02 §3.20.4` ↔ §4 girişi, §4.2 | "Çit kodda" çitin yalnız kodda olduğunu okutuyordu; çit kontrol kısıtıdır (MB-13) | Kontrol kısıtı diliyle (K-1022 (4)) | düşük |
| C-25 | 3 | matris `02 §6.2.16`, `02 §4.2` Z-28, Z-29 ↔ §9.1 | Z-28'in başlangıcı ödemesi alınmış sayılmaya göre kol seçer; Z-29'un "girdi yoksa açılış anı" kolu yoktu (MC-8, MC-9) | İki hücre (K-1030 (3), K-1054) | düşük |
| C-26 | 3 | matris `02 §4.2` Z-41, §1.1.11 K-519 ↔ §9.1.1 | "Başarısız olduğu an" Order'ın alanı gibi yazılmıştı; başlangıç Ö2 geçiş kaydının anıdır (MC-3, MF-12) | Geçiş ve ödeme kaydı; "saklanmaz" (K-1074, K-1030 (3)) | **orta** |
| C-27 | 3 | matris `02 §4.2` Z-21 | Referans fiyat penceresi K-1023 (2)'den önceki hâldeydi (MC-10) | Hizalandı (K-1023 (2)) | düşük |
| C-28 | 3 | matris `02 §4.2` Z-35, §1.1.11 K-497, K-656 | IBAN'ın erken silinme hâlleri düzeltmeyi ve aynı IBAN'ı kullanan başka bekleyen geri ödemeyi saymıyordu (MC-5, MF-7) | K-1131, K-1127, K-1097 | düşük |
| C-29 | 3 | matris `02 §6.4.32`, matris `03 §1.11.34`, §5.2 ↔ §4.25 | Pencere dışı kaydın onayı kaydın alanı gibi yazılmıştı; onay yalnız işlem izindedir (MC-7, ME-13) | AuditLog eklendi (K-606, K-1024 (5)) | düşük |
| C-30 | 3 | matris `02 §6.4.4`, `02 §7.3.4`, `02 §7.3` | İstisna işareti ile sebep ayrı değerler gibi sayılıyordu; işaret alanın doluluğudur (MC-19, MD-11) | Tek alan diliyle (K-1040 (1)) | düşük |
| C-31 | 3 | matris `02 §4.2` Z-3, Z-46, `02 §11.2` P-48, `02 §8.2.6`, matris `03 §1.10.7`, §4.1.3 ↔ §4.38, §4.41 | Davetin beş geçersizleşmesi alanlara inmemişti; "son başarılı giriş anı" ve "davetin hâli" alan gibi yazılıydı (MC-11, MC-12, MD-8, ME-12) | Alanlarla (K-1085, K-1062 (4)) | düşük |
| C-32 | 3 | matris `02 §6.5.9`, `02 §11.2` P-14 ↔ §4.38, §4.40, §4.42 | Davetin ve bekleyen değişikliğin ömrü kendi tablosuna yazılmıştı; ömür bağlantı kaydının `ExpiresAt`'indedir (MC-18, MD-7) | Bağlantı kaydına (K-1062 (2), K-1025 (5), (6)) | düşük |
| C-33 | 3 | matris `02 §7.4.5` ↔ §4.29, §5.6.4, §4.31 | IBAN isteğinin sebebi dört öğelik liste gibi sayılıyordu; sebep altı olaydır ve silinmenin sebebi saklanmaz (MD-1) | `RefundIbanRequestReason`, altı olay (K-1024 (7), K-1036) | **orta** |
| C-34 | 3 | matris `02 §7.2.5` ↔ §4.18 | İfa süresinin bittiği an canlı ürünün alanına işaret ediyordu; kaleme donan süreden türer (MD-2) | "Kaleme donan ifa süresi" (K-598) | düşük |
| C-35 | 3 | matris `02 §9.1.6` ↔ §4.58 | İletişim e-postasının değiştiği an saklanıyor sayılıyordu (MD-3) | Hücre düzeltildi (K-760, K-1066) | düşük |
| C-36 | 3 | matris `02 §12.5` m12, `02 §12.2.7` ↔ §4.61, §4.62 | On yıllık saklama özeti dört kayıt sayıyordu, ayıp talebi yoktu; iki geçmiş boşaltılmaz, imhada satır olarak silinir (MD-5, MD-6, F1-13) | Beşinci kayıt ve silme biçimi (K-1092, K-1030 (1)) | **orta** |
| C-37 | 3 | §1.1.8 girişi, matris `02 §10.1` m3 | "Firma ayarı firma ayarlarına" sözü P-6, P-9, P-44 için tutmuyordu (MD-9) | Üç ayar ürüne ayrıldı (K-447) | düşük |
| C-38 | 3 | matris `03 §1.4.3` m2, S3, S9, `03 §1.11.40`; §1.1.11 K-592 ↔ §4.19 | Kapanış ile firma iptalinin ayrımı iptal kaydının varlığına bağlanmıştı; ayrım `TransitionReason` = `Closure`'dır (ME-2, MF-13) | `Closure` ile (K-1050) | **orta** |
| C-39 | 3 | matris `03 §7.3`, S1 ↔ §4.58 | Gönderim kaydı geçişe bağlı yazılmıştı; bağ siparişedir (ME-3) | Hücreler (K-1066, K-1028 (2)) | **orta** |
| C-40 | 3 | §1.1.11 K-889 ↔ §4.19 | Geçişin kimliği "S, Ö, kalem kaydı" yazılmıştı; kalem kayıtları geçiş kaydına satır yazmaz (MF-3) | "S, Ö, düzeltme" (K-1050 (5), K-1053 (1)) | **orta** |
| C-41 | 3 | matris `03 §1.4.4.5` | Kupon "kapanışta" döner yazılıydı; K-1106'dan beri bütün adetler iptal ya da iade edilmişse döner (ME-5) | `03`'ün cümlesi (K-1106) | **orta** |
| C-42 | 3 | matris `03 §4.2.9` ↔ §4.64 | İmha kaydı özeti silecek kaydı olmayan parçanın satır yazmadığını söylemiyordu (ME-7) | Eklendi (K-1068 (2)) | düşük |
| C-43 | 3 | matris `03 §1.11.31`, `03 §1.7.1.9`, `02 §1.2` RefundIbanRequest ↔ §4.29 | İptal edilmiş siparişe geç gelen ödemenin geri ödemesi hücrelerde yoktu (MA-4, ME-8, ME-14) | Hücreler (K-1075) | düşük |
| C-44 | 3 | matris `03 §4.1.44`, `03 §8.8`; §1.1.11 K-945, K-979 ↔ §4.40 | Gecelik temizliğin hâlleri eksikti: davetin bağlantı kaydı davetle gider, geri almanın eski adresi ömür dolunca boşalır (MF-5, MF-10, ME-9, ME-16) | Hücreler; K-945'e geri işaret (K-1030 (8), K-1064 (3), K-1065) | **orta** |
| C-45 | 2 | §1.1.11 "Satırın yazımı" | 156 "eşlendi" + 17 "veri doğurmaz" = 173; paragraf 155 diyordu (MF-1) | 156 (K-1009) | **orta** |
| C-46 | 3 | §1.1.11 K-585 ↔ matris `02 §10.4.11` | Donmanın "tek istisnası" yazılıydı; K-1007'den beri iki istisna vardır (MF-2) | "İki istisnadan biri" (K-1007) | **orta** |
| C-47 | 3 | §1.1.11 K-883, park A4-4 ↔ §4.70 | Dosya kaydı yalnız varyanta bağlı yazılmıştı (MF-4) | "Ürün ya da varyant" (K-1029 (3), K-1069) | düşük |
| C-48 | 3 | §1.1.11 `05 §7.4.3 g1` | Belge kişisel verisiz kopya olarak donar, ayıp talebi siparişin imha parçasında silinmez — özet eskiydi (MF-6) | Hizalandı (K-1092, K-1027 (4)–(6)) | **orta** |
| C-49 | 3 | §1.1.11 K-582 | Kalan ticari alanların listesi K-1030 (2) ile genişlemişti (MF-9) | Genişletildi (K-1030 (2)) | düşük |
| C-50 | 3 | §1.1.11 K-940, "Tarama sonuçları" (4) | İşaretlerin yeri sorusu K-1015 (5) ile kapanmıştı (MF-11) | Hâlden okunur (K-1015 (5)) | düşük |
| C-51 | 7 | §1.1.11 K-531 | Park A1-2 satırı gösterilmemişti (MF-14) | Eklendi | düşük |
| C-52 | 3 | §1.2, §3.2 aday 4; matris `02 §3.29.6`, `02 §10.1` m6 | WhatsApp numarası §1.2'de iki adaya yazılıydı; §3.2 aday 4'ün dönüşümünü yazmıyordu (MG-1, MA-5, I1-4) | *firma ayarları* satırından kopya düştü; §3.2'ye aday 4 satırı (K-1026 (6), K-1034) | **orta** |
| C-53 | 2 | §1.2, "Yazım 2b2" paragrafı | §1.2'nin toplamı audit'te kalmıştı (2.916 → 2.915); bir paragrafın sonu kaymıştı (MG-2, MG-3) | Sayılar betikle yeniden üretildi; paragraf düzeltildi | düşük |
| C-54 | 2 | §1 alt bölüm haritası, §1.3 tarama paragrafları | Haritanın §1.3 satırı, "son dört satır", "dokuz Kaynak satırı" ve Oturum hücresinin terim sayısı tutmuyordu (MG-4, MG-5, MG-9, MG-10) | Hizalandı | düşük |
| C-55 | 6 | §1.3 "Değişen bölümler" | Çakışma taramasının beş satırı Teknik Mimari v0.24'ün Kaynak satırı atıflarını saymıyordu (MG-6) | Eklendi | düşük |
| C-56 | 3 | §1.1.2 "veri doğurmaz" değer kümesi ↔ §2.7.1 | Saklanan türetilmiş değerler üç sayılıyordu; dördüncüsü `Category.Level`'dır (MG-8) | Dört (K-1040 (4)) | düşük |
| C-57 | 3 | matris `02 §6.6.10` ↔ §2.6 | Eşzamanlı düzenleme "veri doğurmaz" yazılıydı; veri karşılığı sürüm alanlarıdır (MC-16) | `02 §3.31.1` satırıyla aynı (K-873, K-874, K-1014) | düşük |

### 3.2 Genel kural cümleleri, özet tablolar ve başlık notu ↔ entity ayrıntısı (devredilen sınıf)

| # | Kontrol | Yer | Bulgu | Kapanış | Seviye |
|---|---|---|---|---|---|
| C-58 | 3 | §4.17 Sürüm tablosu ↔ §2.6.4, §4.18 | Tablo K-1119'un iki yazımını taşımıyordu; §4.18 "artırmaz"ı bu tabloya dayandırır (I1-1) | İki yazım satırı (K-1119, K-1047) | **orta** |
| C-59 | 3 | §3.1 Customer, OrderItem, OfferingProduct, PortfolioItemProduct ↔ §4.36, §4.18, §4.53, §9.2 | Silme kategorisi hücreleri ayrıntıdan dardı (I1-2, I1-3, I1-12) | Üç hücre (K-696, K-1080, K-1069, K-1026 (5), K-1010 (3)) | düşük |
| C-60 | 3 | §2.7.1 ilk satır | "Nerede güncellenir" kupon hakkının dönüş yolunu saymıyordu (I1-5) | Dönüşün tetiği §4.14'e (K-791, K-1106) | düşük |
| C-61 | 3 | §2.7.2 kapanış cümlesi | "Saklanmaz, okunur" maddelerinin atıf listesi beşini atlıyor, maddesi olmayan ikisini sayıyordu (I1-6) | Betikle yeniden kuruldu (K-1015 (4)) | düşük |
| C-62 | 3 | başlık notu, Aşama 4 park satırı K-883 · K-884 · K-948 · K-980 ↔ §4.18 | Kalemin kısa ömürlü indirme bağlantısını "taşır" diyordu; bağlantı saklanmaz (I1-7) | Düzeltildi (K-1029 (5)) | düşük |
| C-63 | 3 | başlık notu, Aşama 4 park satırı K-899 … K-982 ↔ §4.38 | Daveti gönderen yönetici bağlantı kaydına yazılıyordu; davetin satırındadır (I1-8) | Düzeltildi (K-1025 (6), K-1062 (1)) | düşük |
| C-64 | 3 | başlık notu, konvansiyon 8 | "Tablonun satırı Kaynak sütunu taşır" — on sekiz özet tablosunda Kaynak sütunu yok (I1-9) | Söz daraltıldı (K-1032) | düşük |
| C-65 | 3 | §5.6 girişi | "İki değerli seçim enum değildir" — iki değerli enum'lar var; kastedilen evet/hayır seçimidir (I2-1) | "Evet/hayır seçimi" (K-1036 (7)) | düşük |
| C-66 | 3 | §7.4 son satır ↔ §4.30 | Kalem başına tek talep dizini açılışı korur; satır "veritabanı kısıtı değildir" diye işaretli değildi (I2-2) | İşaret eklendi (K-676) | düşük |
| C-67 | 3 | §5.6.4 Kaynak hücreleri | K-581 ve K-690 yer değiştirmişti (I2-3) | Yerine kondu (K-581, K-690, K-686) | düşük |
| C-68 | 3 | §7.6.1 | "En uzun yaşayan tablolar" "Sipariş hakkında" iletişim talebini saymıyordu (I2-4) | Eklendi (K-1030 (6)) | düşük |
| C-69 | 3 | §9.1.1 görsel satırı ↔ `05 §7.7.2` | Görselin bir günlük eşiği `05 §7.7.2`'de yoktu (I3-2) | Eşik `05 §7.7.2`'ye yazıldı (C-89); `06` değişmedi | düşük |
| C-70 | 3 | §9.1.1 tablosu | Tamamlanmış dijital dosyanın satırı yoktu (I3-3) | Eklendi (K-1118, K-1069) | düşük |
| C-71 | 3 | §9.1.4 | Süresiz kayıtların gruplarında `Admin` ve `Job` yoktu (I3-4) | Eklendi (K-985, K-1064 (1)) | düşük |
| C-72 | 3 | §9.5.1 ↔ `05 §4.5.5`, `02 §12.2.7` | Gelen bildirimin on yılı tasarım değeridir, yasal beş kaydın dışındadır (I3-5) | Ayrıldı (K-951, K-954, K-1092) | düşük |
| C-73 | 3 | §9.5.3 ↔ §9.4.1 | "Listenin kayıtlarına süreleri dolmadan dokunmaz" — bağ boşaltmaları dokunur (I3-6) | Bağ boşaltması ayrıldı (K-354, K-1096) | düşük |
| C-74 | 3 | §5.5.3, §3.1, §9.3.2; `05 §7.7.3` | Dijital dosyanın silme ölçütü eski "indiremeyeceği" diliyle kalmıştı — hak yenileme yolunu kırardı (I3-7) | §4.70 ve K-1029 (4) diliyle (K-1069) | düşük |
| C-75 | 3 | §9.4.2 giriş cümlesi | İmha kaydı yazmayan silmelerin gerekçesi eksikti (I3-8) | Gerekçe ayrıldı (K-984) | düşük |

### 3.3 Güvence cümlesi ile mekanizmanın farkı

Güvence merceği v0.16'da koştu (`AUDIT_REPORTS/06_GUARANTEE_LENS.md`); bu bölüm sonraki düzeltmelerin yazdığı 347 birimi okur.

| # | Kontrol | Yer | Bulgu | Kapanış | Seviye |
|---|---|---|---|---|---|
| C-76 | 7 | §4.42 `ExpiresAt`, §9.5.2; `05 §6.x.20`, §6.x.15, §8.5.10; `DEPLOY_RUNBOOK` K-1081 · K-1133 satırı | K-1133 (2) kuyruktaki geri alma e-postasının özetinin döndürmede yeni sırla yeniden yazılacağını söylüyordu; yazacak bileşen yoktu — sırrı yalnız uygulama alır ve döndürmeyi anlamaz; eski bir ana dönüş eski sırrın özetini geri getirir ve K-1113'ün güvencesi kırılır (G-1) | **K-1137 ⚠** — uygulama her açılışta özeti eş güçlü yeniden yazar | **orta** |
| C-77 | 3 | §4.22, §4.33 `VoidedAt`, §4.34, §7.4 ↔ `02 §10.4.6` | Yanlış iptalin düzeltmesi yalnız "sonucu bekleniyor"u kapatıyordu; yolda olan istekte düzeltme geçer, başarılı sonuç geçersizleşmiş satıra düşer (G-4) | Koşul "istek gönderiliyor"u da kapatır; §7.4'e satır; `02` değişmez (K-940, K-1078) | **orta** |
| C-78 | 3 | §4.18 `DigitalFileId`, §4.70, §2.6.4 | Kaleme "son dosya" yazımının ölçütü düzlemler karışınca bağı indirilen dosyadan ayırıyordu (G-3) | "Kalemin indireceği dosya" (K-1119, K-1029 (4)) | düşük |

### 3.4 Geri beslemenin yan yüzleri (K-652, betik 1)

Kupon ve açık adet kümesi (F2-1, G-2, F2-2, F1-5, P1-6) tek hükümle kapandı: kupon hakkı işleme açık adet kalmadığında ve fiziksel caymada teslim alması, reddi ya da "mal dönmedi" kapanışı beklenen adet kalmadığında döner — K-1104 (2) ve K-1106 (2)'nin yansımasıdır, yeni karar değildir.

| # | Kontrol | Yer | Bulgu | Kapanış | Seviye |
|---|---|---|---|---|---|
| C-79 | 3 | §4.14 son paragraf, başlık notu K-791; `03` 4.2.4 | Kupon hakkı "son açık adetleri kapatan kayıtta" döner denmişti; ayıpta dönme açık adedi düşürmez, fiziksel caymada açık adet beyanda düşer (F2-1, G-2, F2-2) | Ölçüt yukarıdaki cümle; dijital ve hizmette ayıpta dönmenin geri ödemesi; `03` 4.2.4'e fesih kolu (K-1104, K-1106) | **orta** |
| C-80 | 3 | `02 §1.2`, §5.8, §7.1.6, §10.4.10; `03` 1.7.1.4, §1.7.4, 2.7.7, 8.4.8; `04` 5.17.4, 9.9.20; §4.24; `12` ve `07` park satırları ↔ §4.18 | Gecikme feshi "açık adetlerin tamamına" uygulanıyordu; ayıpta dönülmüş adet işleme açık adetten düşer ve ikinci kez geri ödenmez (F1-5, P1-6) | "İşleme açık adet"; fesih hakkı daralmaz (K-1077) | **orta** |
| C-81 | 3 | `05 §4.4.3`; §4.58; `02 §10.4.11`; `03` 8.4.9 | Alıcı satır doğarken yazılır, gövde gönderimde siparişin güncel anahtarıyla üretilir; e-postası düzeltilen siparişin gönderilmemiş bildirimi yeni anahtarlı bağlantıyı eski adrese götürür (F2-8) | **K-1138 ⚠** — düzeltme aynı işlemde bekleyen müşteri bildirimlerinin alıcısını yeni adrese yazar (B-15 hariç) | **orta** |
| C-82 | 7 | `02 §3.13.2`; `05 §6.1.2` | K-1084 (4)'ün kalan riski yalnız `06 §4.36`'daydı (F1-8) | **K-1139** — `02 §3.13.2`'ye kalan risk; `05 §6.1.2` hizalandı | düşük |
| C-83 | 3 | `03` 1.7.3.4; `04` matrisin `03 §1.7.3.4` satırı; `06` matrisi | "IBAN bekleniyor" işaretinin evi yalnız kalem sayılıyordu; kalemsiz istekte siparişe düşer (F2-3, F1-2, F1-1) | Hücreler (K-1108 (3), K-1075) | düşük |
| C-84 | 3 | `02 §5.8` "IBAN isteği" | Tetikleyiciler eksikti — kart iadesinde havale yolu, "havale gerçekleşmedi", kalemsiz istek (F1-3) | Madde ve Kaynak (K-569, K-575, K-1075, K-1108) | **orta** |
| C-85 | 3 | `03` 2.8.4.1, 8.3.3.3 | Kargo ücreti "son caymanın geri ödemesine" eklenir denmişti; teslim alma başına birden çok geri ödeme olur (F1-4) | İki satır (K-1076) | **orta** |
| C-86 | 3 | `03` 9.3.5, 9.3.6 | K-1080'in silme ve imha kaydı kolları satırlara inmemişti (F1-7) | İki satır (K-1080) | düşük |
| C-87 | 7 | `05 §4.4.3` | Yeniden gönderimin satır kilidi yalnız `06 §4.58`'deydi (F1-9) | Kilit cümlesi (K-1084 (5), (6)) | düşük |
| C-88 | 3 | `03` 1.7.3.1 | Davetin geçersizleşme anları beş değil iki sayılıyordu (F1-10) | Hücre (K-1085) | düşük |
| C-89 | 3 | `05 §7.7.2`, §7.4.4, §5.2.8; K-948 etki sütunu ↔ §4.69 | Görselin silinme listesi varyantı ve marka kimliğini saymıyor, yaş koşulunu taşımıyordu (F1-11, F2-12, I3-1) | Bağ listesi ve yaş koşulu; K-948'e geri işaret (K-1029 (2), K-1098 (1), K-1100 (1)) | **orta** |
| C-90 | 3 | `04` matrisin `03 §1.11.31` satırı, 6.2.16.18, 3.5.3 | Geç gelen ödemenin geri ödemesi `04`'te yoktu (F1-14, F1-15) | Matris ve iki satır (K-1075, K-1130) | düşük |
| C-91 | 3 | `03` 8.3.3.8, 1.11.33 | "Havale gerçekleşmedi" adımı K-1131 (2)'yi taşımıyordu (F2-4) | Adım ve yetenek satırı (K-1131, K-1108 (2)) | düşük |
| C-92 | 3 | `02 §8.4.2`, §12.2.7, §7.4.4; §4.33 | IBAN'ın silinmesi koşulsuz yazılıydı; onu kullanan son geri ödemeye kadar tutulur (F2-5) | `02` ve `06` (K-1110, K-1127) | **orta** |
| C-93 | 3 | `05 §10.1.1`; başlık notu ↔ §4.59 | "Kart iadesi hiç yeniden denenmez" — K-1111 gönderilmiş isteğe daralttı (F2-6) | İki yer (K-1111) | düşük |
| C-94 | 3 | `05 §12` MK-72 | K-1135 maddeyi daralttı, gerekçe ve K listesi eskiydi (F2-7) | K-1135 eklendi | düşük |
| C-95 | 3 | `05 §8.7.1` | Turun adımları K-1121'in beklemesini taşımıyordu (F2-9) | Bekleme cümlesi (K-1121 (1)) | düşük |
| C-96 | 3 | `03` 8.4.1 | "Müşteriye görünmez" geçmişin listesine daraldı; ilk değer belgelerde görünür (F2-11) | Söz değerlere bağlandı (K-1136) | düşük |
| C-97 | 3 | `05 §12` MK-36 | "Müşteri kendi ayırdığı stoğa takılmaz" istisnasızdı (F2-13) | K-1086 eklendi | düşük |

### 3.5 Park satırları, geri işaretler ve karar kaydının özet yüzü

| # | Kontrol | Yer | Bulgu | Kapanış | Seviye |
|---|---|---|---|---|---|
| C-98 | 3 | başlık notu, Aşama 3 park satırı K-787 · K-790 · K-793 · K-794 | "Düzeltmeyle geri alınmış firma iptali sayılmaz" kolu düşmüştü (P1-1) | Kol eklendi (K-1056) | düşük |
| C-99 | 3 | başlık notu, Aşama 4 park satırı K-873 · K-874 · K-980 | Sürümü artırmayan yazımların iki kolu düşmüştü (P1-2) | Üç kol; etikete K-1119 (K-1029 (4)) | düşük |
| C-100 | 3 | başlık notu, Aşama 4 park satırı K-875 … K-984 | Belgenin biçimi "bu aşamanın kararı" denmişti; K-1027 kopya dedi (P1-3) | Hizalandı (K-1027) | düşük |
| C-101 | 3 | başlık notu, Aşama 4 park satırı K-883 · K-884 · K-948 · K-980 | Dosyanın silme ölçütü eski dildeydi; görselde yaş koşulu yalnız yüklemeye bağlıydı (P1-4) | §4.70'in dili; yaş koşulu (K-1029, K-1118, K-1069) | düşük |
| C-102 | 2 | başlık notu v0.25; tracker doküman durumu tablosu | "On bir hizalama" — dokuz madde, on iki hizalama (P1-5) | İki yerde düzeltildi | düşük |
| C-103 | 3 | K-1036 etki sütunu, K-17 geri işareti ↔ `09` Aşama 1 park satırı | K-1036 `09` için "park gerekmez" diyordu; `09`'un K-17 satırı onu taşır (P1-7, P2-1) | Etki sütunu ve K-17 geri işareti düzeltildi | düşük |
| C-104 | 3 | `09` iki park satırı; `12` K-951 · K-1092 satırı | Satırlar arası işaret eski etiketi gösteriyordu; "eklenti kurma yetkisi almaz" kararın söylemediği bir kuraldı; `12` gelen bildirimin kolunu düşürüyordu (P1-9, P1-10, P1-11) | Üç satır (K-1124, K-1092) | düşük |
| C-105 | 3 | `DEPLOY_RUNBOOK` Aşama 4 satırı K-924 … K-1096; K-1135 etki sütunu | "Anlık silmeler yeniden uygulanır" K-1135'ten geniş; etiket ve etki sütunu runbook'u taşımıyordu (P1-12, F2-7) | Daraltıldı; etikete ve etki sütununa K-1135 | düşük |
| C-106 | 3 | `DEPLOY_RUNBOOK` Aşama 5 satırı K-1081 · K-1133 | K-1133'ün (4) kalan riski yoktu (P1-13) | Eklendi; mekanizma cümlesi K-1137 ile aynı düzenlemede (C-76) | düşük |
| C-107 | 3 | K-898, K-970 geri işaretleri | K-898'e K-1109 yanlış satırdan işaret edilmişti; K-970'in işareti kolu okutmuyordu (P2-3, P2-4) | K-898'den işaret kalktı (satır #144 öncesi hâline döndü), K-1109 iki satırı ayırdı; K-970 ve K-1078 kolla | düşük |
| C-108 | 3 | K-883 geri işareti | İşaret silme ölçütünü taşımıyordu (P2-5) | Ölçüt eklendi | düşük |
| C-109 | 3 | K-1086, K-1108 etki sütunları | #144'ün değiştirdiği `05 §4.2.2` ve `06 §4.29` etki sütunlarında yoktu (P2-6, P2-7) | Eklendi | düşük |
| C-110 | 2 | `05` v0.24 ve v0.21 notları, `02` v0.84 notu, dipnotlar | Sürüm notları değişen bölümleri saymıyordu: `05 §8.7.4`, §8.7 Kaynak, §5.2.8; `02 §12.2.3` (P2-8, P2-9) | Notlara ve dipnotlara eklendi | düşük |
| C-111 | 3 | §14.1 ⚠ listesi; `PHASE5_CONFLICT_SCAN` §4.1 | K-1135 özeti silmenin iki anını sayıyordu (üçüncüsü geri alma); K-1092 özeti saklamayı kararın sözünden kısa yazıyordu (P2-10, P2-11) | §14.1'de düzeltildi; çakışma raporunun §4.1'inin iki satırına işaret ve "Retro Güncelleme — 2026-10-18"; §5.1 | düşük |

### 3.6 Yan yüzler — şüphecilerin bıraktığı eşler ve düzeltmelerin kendi yüzleri

Beş şüpheci 47 yan yüzü şartnameye koymadan bıraktı. Ayrı bir doğrulayıcı (Opus) her birini güncel metne karşı okudu ve düzeltmelerin kendi yüzlerini üç kümede (işleme açık adet, görselin silinme ölçütü, `HostingLocation`) betikle aradı: on üç yan yüz ve üç kendi yüz şartnameye girdi (66 parça), on dört "değişmez", on altısı zaten yazılmıştı. İlk deneme arka planda başlatılmıştı; aynı kapsam ön planda yeniden koşuldu ve iki doğrulayıcı yedi maddede ayrıştı — yönetici ön plandakinin kümesini seçti (öteki kümenin iki ek önerisi Notlar'ın "İşlenmeyenler" (4)'ünde).

| # | Kontrol | Yer | Bulgu | Kapanış | Seviye |
|---|---|---|---|---|---|
| C-112 | 3 | matris `02 §8.2` L-7, `02 §8.2.3` ↔ §4.19 | L-7'nin sayımı ödeme kaydına bağlıydı; ödeme sayfasının açılmadığı geçiş kaydındadır (`PaymentPageNotOpened`) — C-1'in eşi (Y-V1-4) | Geçiş kaydı (K-1050, K-579) | **orta** |
| C-113 | 3 | `02 §7.2.3`; `03` 1.11.8, 1.11.41, 8.4.8; §4.24 | Bütün adetleri ayıpta sözleşmeden dönmeyle biten fiziksel kalemde fesih düğmesi açılıyor, fesih hiçbir adede düşmüyordu (`Quantity` > 0) (Y-KY1) | **K-1140 ⚠** — düğme yalnız işleme açık adedi kalan kalemde | **orta** |
| C-114 | 3 | matris `02 §8.1.7` ↔ §4.39 | Oturumda `AccountType` sütunu varmış gibi okunuyordu (Y-V1-3) | Hesap türü bağdan okunur (K-1062 (3)) | düşük |
| C-115 | 3 | matris `02 §4.1.5`, P-20 ve üç eşi | "Çitler kodda" — çit kontrol kısıtıdır; P-20'nin "derinlik kodda"sı da öyle (Y-V1-5) | Kontrol kısıtı diliyle (K-1022 (4), K-1040 (4)) | düşük |
| C-116 | 3 | matris `02 §9.2` B-1 | Tam metnin kaynağı yalnız sürüm bağı gösteriliyordu (Y-V1-6) | Donmuş kopya (K-1027 (4), (5)) | düşük |
| C-117 | 3 | matris `02 §10.1.4` | Veri karşılığı K-800'ün park satırına işaret ediyordu (Y-V1-7) | Sürüm alanları (K-873, K-874, K-1014) | düşük |
| C-118 | 3 | matris KP-16 | Ayırmanın "hâli" — C-5'in eşi (Y-V1-8) | Satırın varlığı (K-1035 (1)) | düşük |
| C-119 | 3 | matris KP-45 | İstisna işareti ile alan ayrı değerler gibi — C-30'un eşi (Y-V2-2) | Tek alan (K-1040 (1)) | düşük |
| C-120 | 3 | matris `02 §7.2.1` ve bir eşi | Ödeme beklenirken iptal kalem kaydı yazmaz — C-1'in eşi (Y-V2-4) | Geçiş kaydı (K-1024 (2), K-1050) | düşük |
| C-121 | 3 | matris `03 §4.2.2` ve yedi eşi | "Ödeme onayı anı" Order'da saklanmaz — C-3'ün eşi (Y-V2-8) | Geçiş kaydından okunur (K-1046, K-1074) | düşük |
| C-122 | 3 | matris `02 §3.13.14` ve iki eşi | Bekleyen kaydın silinmesi imha kaydına yazılır — C-86'nın matris yüzü (Y-V4-F1-7) | İmha kaydı (K-1080) | düşük |
| C-123 | 3 | `02 §4.3` (İptal, İade), §7.2.6, §7.4.8; `03` 1.4.4.4, 1.4.4.6, 2.7.2, 2.8.4.1, 4.2.4, 4.2.5, 8.3.1.1, 8.3.3.3; `04` ve `06` matris satırları | Kupon hakkı "yalnız iptal" / "yalnız iade" — karışık siparişte ikisi de hakkı döndürmezdi; kuralın evi "iptal ya da iade" der (Y-V4-1; V4'ün ret gerekçesi tutmadı) | "İptal ya da iade" (K-791, K-1106) | düşük |
| C-124 | 3 | matris `02 §5.8` m4; `02 §7.2.3`; `04` 6.2.17.4; `10` KP-21 | Fesih kapsamının kendi yüzleri — C-80'in eşleri (Y-F1-5) | "İşleme açık adet" (K-1077) | düşük |
| C-125 | 3 | §4.56 girişi | Yayına alma yalnız metni yazar gibi anlatılıyordu — C-10'un kendi yüzü (Y-MB18) | `HostingLocation` da donar (K-818, K-1027) | düşük |
| C-126 | 3 | `05 §12` MK-29 | Bağsız görselin silinmesi yaş koşulsuzdu — C-89'un kendi yüzü (Y-F2-12) | Bir günden eski (K-1069 (3), K-1098 (1)) | düşük |

§1.2'nin sayıları yan yüzlerden sonra yeniden üretildi (2.990 çift; Notlar).

### 3.7 Elenenler

| Bulgu | Gerekçe |
|---|---|
| MB-3 | Kuponun türü §4'te dolu alandan okunur; Alan sütunu aday alanın tarifidir (audit TE-10'un sınıfı) |
| MB-4 | Product bağın sahibi tarafıdır; yayın kapısının özetidir (TE-10'un sınıfı) |
| MB-14 | Hücre kaynağın sözünü aday düzeyinde yineler (TE-10'un sınıfı) |
| MD-10 | Köprüyü §3.2'nin son satırı kurar; RefundIbanRequest bir kalem kaydıdır (TE-10'un sınıfı) |
| ME-10 | Hücreler sözlüğün işaret adını anar, alan iddia etmez |
| ME-15 | Birleşim tablosunun satır özetidir; okunuşu hemen altındaki satırdadır |
| MG-7 | Tarihli tarama paragraflarıdır; sonradan düzeltilen hükümler çakışma taramasının satırlarındadır (öğrenim adayı 23) |
| I1-10 | "Bilgim tarihli" işareti özetin gösterdiği §4.68'dedir; özete kopyalamak ayrıntı kopyalamaktır |
| I1-11 | Zamanlanmış işin anı birincil veridir; öneri K-1109'un elediği genellemedir |
| F1-6 | "Ödenmiş ve henüz geri ödenmemiş tutar" tanımlı terimdir; K-1078 kapsamı evinde genişletti |
| F1-12 | MK-31'in cümlesi adını verdiği riski doğru sınırlar |
| F2-10 | K-1124 `ALTER EXTENSION` ayrıntısını bilerek eledi |
| P1-8 | K-1111'in etki hücresi `09`'u açıkça anar; fark biçimdir |
| P2-2 | K-965'in talimatı `09`'da başka satırda yaşar; "genişletir" ilişkisi yanlış okutmaz |

**Birleşenler.** MA-5 ve I1-4 → MG-1 (C-52); F1-1 → F2-3 (C-83); F1-13 → MD-5 (C-36); I3-1 → F2-12 (C-89). **Devredilenler.** G-2 → kupon hükmü (C-79); P1-6 → açık adet hükmü (C-80). **Kısmen elenen parçalar:** MC-7 (Z-13), ME-1 (`03 §2.5` — ödemesi alınmış siparişin S9 iptalini de kapsar), ME-2 (S2, S4, S10), ME-7 (`02` Z-44 ve `03` 4.2.9'a geri besleme — kural `06 §4.64`'ün mekanizmasıdır), MF-6 (aday olmayan üç entity), MG-1 (§1.1.2'nin aday sorumlulukları), F1-4 (hata ve özet satırları), F2-9 (§8.6.3, MK-73), F2-12 (K-948'in kendi bağlarını taşıyan iki satır), P2-5 (ilişki "netleştirir" kalır). F2-2'nin "yalnız iptal / yalnız iade" parçasını V4 reddetmişti; yan yüz doğrulayıcısı gerekçeyi sınadı ve düzeltti (C-123).

**Sayım:** 126 bulgu satırı (C-1…C-126) — **orta 32**, **düşük 94**. Bölüm başına: matrisin özet hücreleri 57 (§3.1; orta 20) · genel kural cümleleri ve özet tablolar 18 (§3.2; orta 1) · güvence ↔ mekanizma 3 (§3.3; orta 2) · geri beslemenin yan yüzleri 19 (§3.4; orta 7) · park satırları ve karar kaydının özet yüzü 14 (§3.5) · yan yüzler 15 (§3.6; orta 2). Ham bulgu 180 (V1 48, V2 52, V3 28, V4 28, V5 24): kabul 145 + kısmi 14 = 159, ret 14, birleşen 5, devredilen 2 (ikisi de hükme bağlandı). Uygulanmayan şartnameler: V3 Ş-I3-2 (aynı eşik `05 §7.7.2`'ye yazıldı), V4 F1-2, F1-14a/b ve F2-3d'nin bir parçası (başka şartnameyle karşılandı). Dört karar: K-1137 ⚠, K-1138 ⚠, K-1139, K-1140 ⚠.

**Park satırlarındaki yüzler:** `07` (C-80), `09` (C-103, C-104), `12` (C-80, C-104), `DEPLOY_RUNBOOK` (C-76, C-105, C-106), `06`'nın Aşama 3–4 blokları (C-62, C-63, C-98…C-101). Park etiketi ↔ etki sütunu: K-1135 runbook etiketine ve etki sütununa, K-1137 runbook etiketine ve etki sütununa birlikte girdi; K-1138, K-1139 ve K-1140 sonraki dokümana iş bırakmaz.

**En önemli üçü:** (1) matris §4'ten önce kurulmuş ve sonra ona karşı okunmamıştı — ödeme onayından önceki iptalin, ödeme ve kargo anlarının, ayırmanın ve iki eksenin yerleşimi elli yedi satırda eskiydi; matrisi okuyan geliştirici iptal kaydını ödenmemiş siparişte yazardı (C-1, C-3, C-5, C-6); (2) K-1133'ün güvencesinin uygulayıcısı yoktu ve eski bir ana dönüşle birlikte geri alma yolu kapanabiliyordu (C-76 — K-1137); (3) e-posta düzeltmesinden önce kuyruğa girmiş bildirim yeni erişim bağlantısını eski adrese götürebiliyordu (C-81 — K-1138).

---

## 4. Etki işaretleri

Karar kaydında yetmiş üç Aşama 5 kararının etki sütununa `(taslak güncellendi — <doküman sürümleri>; checkpoint — CP05 <bulgu kimlikleri>)` parçası eklendi (betikle, hüküm dosyalarının "Dayanak K" sütunundan; K-994…K-1136). Yeni kararların geri işareti: K-1133, K-1113 (K-1137) · K-1115, K-585 (K-1138) · K-1084 (K-1139) · K-1077 (K-1140). Hizalamaların geri işaretleri: K-945 (K-1030 (8), K-1064 (3), K-1065) · K-948 (K-1029 (2)) · K-17 (K-1036; `07` ve `09`) · K-883 (K-1029 (4)) · K-970 ve K-1078 (kol) · K-1109'un K-898 parçası K-895'e taşındı. Etki sütunları: K-1036, K-1086, K-1108, K-1135. Aşama 1–4'ün salt okunur kayıtlarına yalnız geri işaret girdi; K-898'deki yanlış işaret kaldırıldı (satır #144 öncesi hâline döndü). Yeni satırlar: **K-1137…K-1140**.

---

## 5. Proje sahibinin gözden geçirme listesi

### 5.1 Otuz dört kararlık liste — gösterildi

Kaynak: [çakışma taraması §4.1](PHASE5_CONFLICT_SCAN.md). Liste 2026-10-18'de yönetici aracılığıyla proje sahibine gösterildi; **itiraz gelmedi**. İşaret dönüşümü — `(öneriyle kaydedildi — ⚠ — gözden geçirildi 2026-10-18, itiraz yok)` — arşiv işaretinin adımında yapılır.

**Hazır metnin düzeltmesi (P2-10, P2-11; C-111).** İki kararın tek cümlelik özeti kararın sözünden kısaydı; kararların kendisi değişmedi. Gösterim §4.1'in eski cümleleriyle yapıldığı için iki düzeltilmiş cümle §5.2'nin mesajında yeniden gösterilir. Düzeltilmiş cümleler — çakışma taramasının "Retro Güncelleme — 2026-10-18" bölümünde ve karar kaydı §14.1'de aynı:
- K-1092 — Ayıp talebi son değiştiği andan on yıl saklanır ve siparişin kişisel bilgilerinden önce silinmez; ödemesi alınmış siparişe gelen ödeme bildirimi de geldiği andan on yıl saklanır ve siparişin kişisel bilgilerinden önce silinmez.
- K-1135 — Doğrulanmamış bir kaydın, yeni e-posta adresinin doğrulanması, "bu değişikliği ben yapmadım" bağlantısıyla e-posta değişikliğinin geri alınması ya da Google ile ilk giriş sırasında silinmesi, veritabanı yedekten eski bir ana dönerse o işlemle birlikte geri gelir ve ayrıca yeniden uygulanmaz.

### 5.2 Checkpoint'in üç ⚠ kararı — arşivden önce ayrıca gösterilir

Bu kararlar listenin gösteriminden sonra kaydedildi (`INSTRUCTIONS.md` §9 işletim kuralı 3). **Kapı:** arşiv işaretinden önce, tek mesajda, karar başına bir sade cümleyle ve gerekçesiz.

*Hesap ve güvenlik*
- K-1137 — Hesap e-postalarındaki bağlantıları üreten gizli anahtar değiştirildiğinde ya da veritabanı yedekten eski bir ana döndüğünde, kuyrukta bekleyen "bu değişikliği ben yapmadım" e-postası uygulama her açıldığında yeni anahtarla yeniden hazırlanır; bağlantı çalışmaya devam eder.

*Sipariş ve e-posta*
- K-1138 — Firma bir misafir siparişinin e-posta adresini düzeltince, henüz gönderilmemiş ya da yeniden denenmeyi bekleyen sipariş e-postaları yeni adrese gider; eski adrese yeni sipariş bağlantısı gitmez.

*Fesih*
- K-1140 — Bir kalemin bütün adetleri ayıp yüzünden sözleşmeden dönülerek kapandıysa o kalem için "gecikme nedeniyle fesih" düğmesi açılmaz; fesih yalnız işlem görmemiş adedi kalan kalemde açılır.

*Yeniden gösterilecek iki düzeltilmiş cümle (§5.1)*
- K-1092 — Ayıp talebi son değiştiği andan on yıl saklanır ve siparişin kişisel bilgilerinden önce silinmez; ödemesi alınmış siparişe gelen ödeme bildirimi de geldiği andan on yıl saklanır ve siparişin kişisel bilgilerinden önce silinmez.
- K-1135 — Doğrulanmamış bir kaydın, yeni e-posta adresinin doğrulanması, "bu değişikliği ben yapmadım" bağlantısıyla e-posta değişikliğinin geri alınması ya da Google ile ilk giriş sırasında silinmesi, veritabanı yedekten eski bir ana dönerse o işlemle birlikte geri gelir ve ayrıca yeniden uygulanmaz.

**Sayım:** gösterilen 34 · checkpoint 3 — **toplam 37**. Betikle sayıldı; otuz yedisinin konu hücresi ⚠ işaretini taşıyor. K-1139 ⚠ grubuna girmez. İki düzeltilmiş cümle (K-1092, K-1135) aynı mesajda yeniden gösterilir.

**Gözden geçirme sonucu — 2026-10-18 (K-437'nin 6. adımı, arşiv işaretinden önce; K-1142).** Üç karar ve iki düzeltilmiş cümle yönetici aracılığıyla tek mesajda, karar başına bir sade cümleyle gösterildi (Soru 6); öğrenim terfisinin iki önerisi aynı turda ayrı numaralı sorulardı (Soru 7, Soru 8; K-1143). **İtiraz yok:** hiçbir karar değişmedi, yeni karar satırı açılmadı ve bu rapora retro bölümü gerekmedi. §5.1'in otuz dört kararı aynı gün daha önce gösterilmişti (Soru 5), itiraz yok. Karar kaydında otuz yedi satırın konu hücresindeki işaret silinmedi, `(öneriyle kaydedildi — ⚠ — gözden geçirildi 2026-10-18, itiraz yok)` biçimine dönüştürüldü (Aşama 4'ün kalıbı, CP04 §5.1); §3'ün GAP-13 satırının karar hücresindeki K-1008 işareti de (Aşama 3'ün kalıbı). Öğrenim terfisi (K-1141) yeni ⚠ karar doğurmadı. **Mekanik doğrulama:** §5.1 ile bu listenin 37 tekil K numarası ile dönüştürülen 37 satır birebir — eksik 0, fazla 0; konu hücresinde eski işareti taşıyan Aşama 5 karar satırı kalmadı. Kayıt: karar kaydı §14.1 (v0.144).

---

## 6. Öğrenim terfisine notlar — 5. adımın girdisi

Adayların metinleri tracker §14.1'dedir; bu checkpoint üç aday ekledi.

| # | Aday | Kaynak | Playbook |
|---|---|---|---|
| 24 | Matris çıktıdan önce kurulur ama çıktı yazıldıktan sonra ona karşı okunmaz — matrisli dokümanda yazım bitince "Entity" ve "Alan" hücreleri çıktının ayrıntısına karşı bir mercekle okunur | §3.1 (57 satır), §3.6 | `06` şablonu §1 ve `checklists/document-stage.md` §4 — playbook'a da gider |
| 25 | Sürüm notunun bölüm listesi `git diff`'le karşılaştırılmıyor | C-110 | — |
| 26 | Şüphecilerin "yan yüz" listesi sahipsiz kalabilir; iki bağımsız doğrulayıcı yedi maddede ayrıştı | §3.6 | — |

**Kanıt — mevcut adaylara ve kurallara:** aday 18'in ("matrisli dokümanın cross-review'ında kabullerin çoğu matrisin özet hücrelerinden geldi") sınıfı checkpoint'te elli yedi satırla doğrulandı — aday 24 onun kökünü adlandırır. Aday 20 ve 21'in park satırı sınıfı #144'ün kendi düzeltmelerinde on üç yüzle yeniden görüldi (§3.5). Aday 22'nin "park gerekmez" öncülü K-1036'da yeniden çıktı (C-103). `audit` skill'inin "Koşum biçimi" 5. maddesi (alt ajanları ön planda başlat) bu checkpoint'te bir kez çiğnendi: iki alt ajan arka planda başlatıldı, bitiş bildirimleri yöneticiye gitti ve yan yüz doğrulayıcısı ön planda yeniden koşuldu — kural yazılıdır, aday değildir.

**Tek seferlik gözlem (raporda kalır):** şüpheciler hükümle birlikte birebir şartname yazdı ve Sonnet ajanları dosya başına uyguladı — 161 + 66 şartname parçasının hiçbiri çakışmadı; aynı dosyaya sırayla yazan şartnamelerin sırası hüküm dosyasında yazılıydı.

---

*Kaynak: K-437 (kapanış sırası, 4. adım) · K-429 (çakışma taraması — girdi) · K-1129 (cross-review'ın çıkış kuralı ve checkpoint'e devri) · K-652 (geri beslemenin işletimi ve iki betik) · K-29 (etki işaretleri) · K-647 (kaydın evi; Aşama 1–4 kayıtları salt okunur) · K-992 (⚠ öneriyle kayıt ve liste kapısı) · K-1137…K-1140 (bu checkpoint'in kararları) · `.claude/skills/checkpoint/SKILL.md` · `.claude/skills/audit/SKILL.md` ("Koşum biçimi") · `Docs/CHECKPOINT_REPORTS/CP04_PHASE4_CHECKPOINT.md` (biçim örneği) · `Docs/AUDIT_REPORTS/06_GUARANTEE_LENS.md` (güvence merceğinin kalıbı).*
