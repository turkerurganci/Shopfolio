# Aşama 2 — Checkpoint (CP02)

**Tarih:** 2026-10-04 | **Aşama:** 2 — Kullanıcı Akışları | **Kapanış adımı:** K-437'nin 4. adımı (`/checkpoint`; `checklists/document-stage.md` §6 son madde, §7 madde 4)
**Girdi sürümleri:** karar kaydı v0.68 · Proje Vizyonu (`01`) v0.32 · Ürün Gereksinimleri (`02`) v0.54 · Kullanıcı Akışları (`03`) v0.10 · MVP Kapsamı (`10`) v0.36 — `origin/main` @ `16fc3c8`
**Çıktı sürümleri:** karar kaydı v0.69 · Ürün Gereksinimleri v0.55 · Kullanıcı Akışları v0.11 · MVP Kapsamı v0.37 · Proje Vizyonu değişmedi · `04`, `05`, `06`, `07`, `09`'un park satırları · `PLAYBOOK_FEEDBACK.md` 48 satır
**Girdi raporu:** [Aşama 2 — Çakışma Taraması](PHASE2_CONFLICT_SCAN.md) (K-437'nin 3. adımı, K-429) — açık kalem durumu, ⚠ kayıtların listesi (§4.1), avukat teyidi önerisinin kapısı (§4.2) ve devir dizini (§6) oradan alındı.

> **Ne işe yarar:** Aşama kapanmadan önce Kullanıcı Akışları'nın ve Aşama 2'nin geri beslediği iki dokümanın birbirine, karar kaydına ve metodolojiye karşı tutarlı olduğunu doğrular; bulunan her uyumsuzluğu ya kapatır ya da adıyla proje sahibine bırakır. Sonuç öğrenim terfisinin (5. adım) ve arşiv işaretinin (6. adım, K-436 — kapsamı K-647) girdisidir.

---

## Checkpoint Sonucu — 2026-10-04

**Aşama:** 2 — Kullanıcı Akışları (kapanış sırası K-437: yazım turu ✓ · kalite döngüsü ✓ · çakışma taraması ✓ · **checkpoint** · öğrenim terfisi · arşiv işareti)
**Genel durum:** ⚠ Dikkat gerektiren noktalar — otuz sekiz bulgu (on orta, yirmi sekiz düşük); hepsi bu checkpoint'in PR'ında var olan kararların yansımasıyla kapandı. Yeni karar yok, çok kritik konu yok. Kapanıştan sonra durum **✓ Yolunda**; açık kalan tek kapı ⚠ listesinin ve avukat teyidi önerisinin proje sahibine gösterilmesidir (arşiv işaretinden önce, §5).

### Kontrol Özeti

| # | Kontrol | Sonuç | Detay |
|---|---|---|---|
| 1 | Yol haritası | ✓ | Aşama 2'deyiz (`00 §C.1`: rol Product Owner / Business Analyst, girdi `01` + `02`, çıktı `03`). Açılış (PR #56–#58), konu planı (#59), workshop (#60), tek yazım turu beş oturumda (#61–#65), kalite döngüsü — audit ve deep review (#66), cross-review üç tur, 3. tur TEMİZ ve etki yansıtma (#67–#69) — ve çakışma taraması (#70) bu sırayla kapandı. Atlanan ya da sırası değişen adım yok (K-437, K-432). |
| 2 | Doküman durumu | ⚠ → ✓ | Başlık ve dosya sonu sürümleri kayıt §1 ile aynıydı (`01` v0.32 / 2026-10-03, `02` v0.54, `03` v0.10, `10` v0.36 / 2026-10-04). İki kusur: kayıt §1'in "`03` sürüm geçmişi" notu v0.9'da kalmış, "Sırada cross-review 3. tur" diyordu (U-36); `02` ve `10`'un dosya sonu notu etki yansıtması yapılmış olduğu hâlde *"`03`'ün etki yansıtmasında yeniden taranır"* diyordu (U-37). Şablon dokümanlarda (`04`–`09`, `11`, `12`) başlık v0.1 / `YYYY-AA-GG` ve dosya sonu aynı; park bloğu eklemeleri başlığa dokunmaz — Aşama 1'in düzeni. `DEPLOY_RUNBOOK`'un sürüm alanı yok, durumu "⬚ Doldurulmayı bekliyor". |
| 3 | Tutarsızlık | ⚠ → ✓ | Sayılar, kimlikler ve durum adları üç dokümanda birebir (Notlar). Aynı kural iki yerde farklı kapsamla yazılmıştı — en genişi başka kanaldan gecikme feshi (K-706): dokuz yerde eski hâlindeydi; hepsi kararın kendisine hizalandı (§3.1–§3.3). |
| 4 | Açık kararlar | ✓ | Tracker §4'te açık satır yok, A-19 açılmadı; `02 §13` boş; `03` ve `10`'da açık kalem yok (çakışma taraması §1, §3). §8.1'in açık iki süreç maddesi kapısındadır: ⚠ listesi ve avukat teyidi önerisi — ikisi bu raporun §5'inde adıyla; öğrenim terfisi adayları — 5. adım. |
| 5 | Aşama çıktıları | ✓ | `03` yazıldı (§0–§11, 62 konunun 62'si), kalite döngüsünden geçti (cross-review üç turda TEMİZ, K-718'in ölçüsüyle) ve etkileri `02`'ye (v0.48…v0.54) ve `10`'a (v0.30…v0.36) geri beslendi (K-652). Checklist'in aşamaya özgü üç hatırlatması karşılandı: durum makinesi §1'de akışlardan önce, bildirim haritası §7'de ayrı tek tablo (K-689), hata akışları §3'te `02 §6`'nın 141 senaryosunu birer satırla taşır. Kayıt §1'de `03`'ün Checkpoint sütunu ✓; Durum sütunu ⏳ kalır — ✓ arşiv işaretindedir (6. adım). |
| 6 | Geriye dönük etki | ⚠ → ✓ | Proje Vizyonu'na (`01`) etki yok — Ü-1 ↔ ÖK-1…ÖK-11 birebir kalıyor (K-699 ÖK-7'ye yalnız kurtarma cümlesi ekledi), M-4'ün okunuşu iade reddi ve başka kanaldan feshe dayanıklı, S-1…S-10 bozulmadı. Önceki aşamanın park satırlarından ikisi sonraki Aşama 1 kararlarını izlemiyordu ve çakışma taramasından kaçmıştı (U-10, U-11); K-647 `02` ve `10`'un "Karar referansları" notuna yansımamıştı (U-35). |
| 7 | Yeni alan/kural | ⚠ → ✓ | Aşama 2'de `02`'ye eklenen kimlikler kendi evinde tanımlı: L-9 (§8.2), P-49 (§11), B-16 (§9.2), F-6 (§9.3), "İade reddi" (§1.2, 104. terim), 6.4.35–6.4.36, 6.5.14–6.5.17, 6.6.14–6.6.16, 6.7.17, 6.9.14–6.9.16. Eksik tanımlar: iade reddinin geri alınamaz onayı §5.9'da yoktu (U-4); iade reddi ve eski iade adresi yükümlülükleri §12.5'te yoktu (U-5); IBAN isteğinin ve gecikme feshinin sözlük ve kalem kaydı tanımı eski hâlindeydi (U-1, U-3); bölüm sonu Kaynak satırlarında Aşama 2'nin kırk bir atfı yoktu (U-26). |

### Aksiyon Gerektiren Maddeler

- [x] Otuz sekiz bulgu düzeltmeyle kapandı — `02` v0.55, `10` v0.37, `03` v0.11, `04`, `05`, `06`, `07`, `09` park satırları, karar kaydı v0.69 (aşağıdaki §3)
- [x] Yeni karar yok — düzeltmelerin hepsi var olan kararların yansımasıdır; ⚠ listesi değişmedi (39)
- [x] Checkpoint'in dokunduğu kırk dört Aşama 2 kararının etki sütununa `(… — checkpoint)` parçası eklendi (§4); K-17'ye K-531 geri işareti (Aşama 1 kaydının tek istisnası)
- [ ] **Otuz dokuz ⚠ kararın ve avukat teyidi önerisinin proje sahibine gösterilmesi** — liste §5.1'de karar başına bir sade cümleyle, öneri §5.2'de; kapı arşiv işaretinden (6. adım) önce, tek mesajda (`INSTRUCTIONS.md` §2; tracker §8.1). İtiraz gelen karar yeni bir satırla değişir ve bu rapora `## Retro Güncelleme` bölümü eklenir
- [ ] Sırada öğrenim terfisi (K-437'nin 5. adımı) — on iki aday tracker §8.1'de; 11 ve 12 bu checkpoint'te eklendi, 9'a kanıt (§6)

### Notlar

**Yöntem.** Mekanik kontroller betikle koştu (kimlik aralıkları, sayılar, durum adları, K atıfları, `03` ve `10`'un `02`'ye bölüm atıfları, K-701'in devralma kuralı, bölüm sonu Kaynak satırları, sürüm ve tarihler). Anlamsal tarama dört salt okuma merceğiyle, ön planda koştu: (1) `02` ↔ `03`, K-653…K-688; (2) `02` ↔ `03`, K-689…K-717; (3) `02` ↔ `10` ve `10`'un iç tutarlılığı; (4) Proje Vizyonu'na geriye dönük etki ve park blokları. Her bulgu düzeltmeden önce güncel metne karşı yeniden okundu; dört aday düzeltme gerektirmedi (aşağıda "İşlenmeyenler"). Düzeltmeler var olan kararların yansımasıdır — `02` ve `10`'un ✓ durumu ve kalite döngüleri yeniden açılmadı (K-652'nin kalıbı).

**Betikle doğrulanıp tutarlı çıkanlar:**

- **`02`'nin kimlikleri:** Z-1…Z-47 · L-1…L-9 · P-1…P-49 (P-2 ve P-11 kaldırılmış satırlar; kırk yedi parametre: on firma ayarı, otuz yedi ürün sabiti) · B-1…B-16 · F-1…F-6 · H-1…H-4 — boşluk yok; `01`, `03` ve `10` en büyük kimliği aşan ya da kaldırılmış kimlik kullanmıyor. `03 §0.3.3`'ün aralıkları kaynağa eşit.
- **`10`'un kimlikleri:** KP-1…KP-77 · KD-1…KD-63 · ÖK-1…ÖK-12 · SK-1…SK-10 · YH-1…YH-22; `01`, `02` ve `03`'teki her atıf var olan bir satıra gider.
- **Sayılar:** KP-64 "kırk yedi parametre" = 49 − 2 · KP-66 "on altı olay" = B-1…B-16 ("dört düzeltme" B-10…B-13) · KP-67 F-1…F-6'nın her tetikleyicisi · KP-72 dokuz limit = `02 §8.2` · KP-48, `02 §10.4.1` ve `03 §8.4` dokuz müdahale, dördüncü ve sekizincinin yeni adlarıyla · KP-51 ve SK-3 = `02 §10.5` (kart fiziksel 2, havale fiziksel 3, hizmet 1, dijital 0; karışık 4) · KP-50 altı sayaç · sözlük 104 terim · `02 §6` 141 senaryo (6.2: 22 · 6.3: 6 · 6.4: 36 · 6.5: 17 · 6.6: 16 · 6.7: 17 · 6.8: 11 · 6.9: 16) · Proje Vizyonu §3.2, `02 §1.3` ve `03 §0.1.1` dört aktör.
- **Durum adları:** sevkiyat, ödeme, yayın, ayıp ve iletişim talebi durumları `02 §5` ve §1.2 ile birebir; büyük harfli ya da eş anlamlı biçim ("Teslim Edildi", "Kargoda" durum olarak, "Kısmi geri ödendi", "Kapandı") yok.
- **K atıfları:** `01`'de 162, `02`'de 658, `03`'te 95, `10`'da 623 farklı K numarası — hepsi kayıtta; K tekilliği komutu boş.
- **Bölüm atıfları:** `03`'ün `02`'ye 1842, `10`'un `02`'ye 252 bölüm atfının hepsi var olan bir bölüme gidiyor; K-701'in devralma kuralına göre okunduğunda yanlış dokümana giden beş hücre düzeltildi (U-33).
- **Geri besleme:** `02`'nin v0.48…v0.54 notları ve `10`'un v0.30…v0.36 notları ile `03 §11`'in 45 satırı sürüm ve bölüm listesinde birbirini tutuyor (çakışma taraması §5.2'nin 4. ve 5. sınıfı yeniden koştu).
- **Park blokları:** Aşama 2'nin beş park satırı (K-666, K-667, K-687 → `05`; K-705 → `08`; K-699 → `DEPLOY_RUNBOOK`) kaynak satırıyla aynı içerikte ve karar almıyor; dizin satırlarının sayıları çakışma taraması §6.1 ile birebir.

**İşlenmeyenler — düzeltme gerektirmedi:** (1) `02 §10.4.5` ve §10.4.10'un kalın başlıkları yalnız teslim tarihini ve caymayı anar; iade malının ulaşma tarihi ve gecikme feshi aynı bölümde kalın bir alt başlıkla yazılıdır ve §10.4.1'in müdahale adları ikisini de sayar. (2) `02 §4.3`'ün "Sipariş onayı" anı ve §5.3.2 donan değerleri özetler, tam liste değildir ve §3.23'e işaret eder — havale IBAN'ı §3.23.3'tedir (K-663). (3) `10`'un v0.30 ve v0.36 notlarının eksiği geçmiş notlar yeniden yazılmadan v0.37 notunda kayda geçti. (4) `02`'nin başlık notlarında v0.46'nın v0.47'den sonra durması Aşama 1'in tarihçesidir.

---

## 3. Bulgular ve kapanışları

Seviye: **orta** — bir doküman bir kararı eksik uyguluyor, iki yer çelişiyor ya da source-of-truth'ta bir kural eksik; **düşük** — ifade, kapsam ya da atıf farkı, anlamı değiştirmez. Kontrol sütunu skill'in yedi kontrolüne gider.

### 3.1 Ürün Gereksinimleri ↔ Kullanıcı Akışları — kuralın evi

| # | Kontrol | Yer | Bulgu | Kapanış | Seviye |
|---|---|---|---|---|---|
| U-1 | 3, 7 | `02 §1.2` Gecikme feshi, §5.8 gecikme feshi kaydı, §9.2 B-9 ve B-14 | K-706 feshin başka kanaldan bildirilip firmanın panelden kaydetmesini `02 §10.4.10` ve §7.2.3'e yazdı; tanım ve kalem kaydı feshi yalnız sipariş sayfasından doğuyormuş gibi anlatıyordu, B-9'un ve B-14'ün olay hücresi yalnız caymayı sayıyordu — `03 §1.7.1.4`, §7.1.22 ve §7.1.32 `02`'nin bu yerlerine dayanıyordu | Dört yer feshin firmanın panel kaydıyla da doğduğunu ve damganın bildirimin ulaştığı tarih olduğunu yazar; B-9 ve B-14 fesih kaydını sayar, Kaynak'larına K-706 | **orta** |
| U-2 | 3 | `02 §7.4.5` (iki yer), §10.1.2 "Siparişler" satırı (üç yer), §10.5.2, §10.6.1, §12.5 teyit satırı · `03 §7.2.16` | Aynı kararın (K-706) eski adla kalan yansımaları: sekizinci müdahale "başka kanaldan gelen cayma bildiriminin kaydı", IBAN aktarma istisnası yalnız caymada, "cayma bildirimini ileri tarihli kaydedemez" | Her yerde "cayma ya da gecikme feshi bildirimi"; Kaynak'lara K-706 | düşük |
| U-3 | 3, 7 | `02 §1.2` IBAN isteği, §5.8 IBAN isteği kaydı, §4.2 Z-35 | Sözlük isteğin üç tetikleyicisini sayıyordu; başarısız kart iadesinde havale yolu (K-569), firma iptali ve çıkarma (K-690) ve yöneticinin açtığı istek (K-686) yoktu; §5.8 K-686'yı saymıyordu; Z-35'in başlangıcı yalnız "Beyan anı" — firmanın açtığı istekte beyan yoktur, `03 §4.1.40` "beyan ya da IBAN girişi" diyordu | Sözlük ve §5.8 bütün tetikleyicileri sayar; Z-35: "Beyan anı — IBAN isteğinde müşterinin IBAN'ı girdiği an", Kaynak'a K-686 | **orta** |
| U-4 | 7 | `02 §5.9`, §7.6.1 | K-656 iade reddini "onaydan önce sonucunu tek cümleyle söyler ve geri alınmaz" diye kurdu; `03 §1.6.1.6` reddi geri alınamaz onay listesinde sayar ve `02 §5.9`'a dayanır, ama §5.9 reddi saymıyordu — `04` onay pencerelerini bu listeden türetir | §5.9'a "koşullu istisna kaleminde iade reddi (§7.4.9) de aynı onayı ister — ret geri alınmaz (K-656)"; §7.6.1 öteki onaylı işlemleri §5.9'a bağlar | **orta** |
| U-5 | 7 | `02 §12.5` | §12 girişi firmanın yükümlülüklerini §12.5'te tek tabloda toplar; K-656, K-657 ve K-665'in üç yükümlülüğü — reddin ispatı, reddedilen malın firmanın bedeliyle geri gönderilmesi, eski iade adresine gelen malın teslim alınması — tabloda yoktu | İki yeni satır: "İade reddinin ispatı ve reddedilen malın geri gönderilmesi" (K-656, K-657) · "Eski iade adresine gönderilen malın teslim alınması" (K-665) | **orta** |
| U-6 | 3 | `02 §5.12.2` | Oturumların kapandığı hâlleri sayan liste şifrenin hesaptan değiştirilmesini saymıyordu (K-695); `03 §1.10.6` bu kuralın kaynağı olarak §5.12.2'yi gösterir | "…şifre hesaptan değiştirildiğinde değişikliği yapan dışındaki bütün oturumlar kapanır (§3.13.10; K-695)" | **orta** |
| U-7 | 3 | `02 §6` 6.7.5 | Havale "ödendi" işaretinin düzeltilme koşulu K-703'ün koşulunu — hiçbir kalemde iptal, çıkarma, fesih ya da cayma kaydı yokken — ve "hiçbir geri ödeme yapılmamışken"i taşımıyordu; satır §10.4.6'nın yasakladığı düzeltmeye izin veriyor gibi okunuyordu | Koşul §10.4.6'nın sözcükleriyle tamamlandı; Kaynak'a K-703 | **orta** |
| U-8 | 3 | `02 §10.6.1` ↔ §12.5 · `03 §8.3.3.2` | "IBAN bekleniyor" listesindeki her kalem — ayıp çözümü ve tutar bazlı geri ödeme dahil — "geri ödemenin kalan süresiyle" görünüyordu; §12.5 ayıpta "ürün süre sayacı tutmaz" der (K-716). `03 §8.3.3.2` tutar bazlı geri ödemede de kalan süre gösteriyordu, 8.3.3.6 ile çelişiyordu | §10.6.1: "süresi olan hatta geri ödemenin kalan süresiyle … ayıp talebinin çözümünde ve tutar bazlı geri ödemede süre sayacı yoktur (§12.5; K-716)"; 8.3.3.2: "Süresi olan hatta panel kalan süreyi gösterir" | **orta** |
| U-12 | 3 | `02 §10.2.2` | K-662'nin "davetin geçersizleşmesi kaldırmanın iz satırında durur" cümlesi yoktu | "…davetlerin geçersizleşmesi kaldırmanın iz satırında durur (§10.3.1; K-662)" | düşük |
| U-13 | 3 | `02 §4.2` Z-1 | Sonuç sütunu yeni e-posta adresinin doğrulamasında bekleyen değişikliğin düşmesini yazmıyordu (K-659; kural §3.13.5'te) | Sütuna eklendi | düşük |
| U-14 | 3 | `02 §9.2` B-13 | "havale bilgisi" — siparişe donan IBAN (K-663) Kaynak'ta yoktu | "siparişe donan havale bilgisi (§3.21.5)", Kaynak'a K-663 | düşük |
| U-15 | 3 | `02 §1.2` Cayma | Hizmet kaleminde düğmenin ödeme onayından sonra açıldığı (K-673) tanımda yoktu | Tanıma eklendi | düşük |
| U-16 | 3 | `02 §1.2` "Geri ödeme gerçekleşmedi" işareti | Kalkış yalnız yanlış iptalin düzeltilmesine bağlıydı; K-675 ve §7.2.8 üç koşul sayar | Üç koşul, Kaynak'a K-675 | düşük |
| U-17 | 3 | `02 §2.2`, 3. adım | "Üye adres defterinden seçer" — K-680'in yeni adresi yoktu | "…seçer ya da yeni bir adres yazar — isterse deftere kaydeder (§3.14.1; K-680)" | düşük |
| U-18 | 3 | `02 §6` 6.7.9 | Müşteriye giden B-8 (K-684) yoktu; `03 §3.5.2.9` bu satıra dayanır | "firmaya F-4, müşteriye B-8 gider", Kaynak'a K-684 | düşük |
| U-19 | 3 | `02 §4.2` Z-9 | "Kesintide ertelenen iptalde yeni hatırlatma gitmez" (K-685) yazılı değildi; Z-8'in "yeni son gün" ifadesi yeni süre gibi okunabiliyordu | Sütuna eklendi | düşük |
| U-20 | 3 | `02 §1.2` Sipariş kalemi, §11 P-8 | Donan değerlerde indirme hakkı (K-713) yoktu; P-8'in kullanıldığı yer §3.23.1'i saymıyordu | İkisine eklendi | düşük |
| U-21 | 3 | `02 §4.2` Z-45 | Sürenin dolmasıyla hesap silmenin de açıldığı (K-698, §3.15.6) yazılı değildi | Sütuna eklendi, Kaynak'a K-698 | düşük |
| U-22 | 3 | `02 §3.21.11`, 6.2.22, §12.5 ters ibraz satırı | Uyarı eski biçimdeydi: yalnız ters ibrazı ve yalnız havale yolunu açmadan önce; K-705 iadenin kendisine bakılmasını ve yeniden denemeyi ekledi | Üç yer K-705'e hizalandı | düşük |
| U-23 | 3 | `02 §9.2` B-7 | "Gitmez" listesi kalem iptaliyle kapanışı saymıyordu (K-717) | "…kalem iptali ya da kalem çıkarmasıyla … kalem iptalinin B-7'si iptal anında gitmiştir", Kaynak'a K-717 | düşük |
| U-24 | 3 | `02 §3.17.1` | K-704'ün "başarısız ödemede sayfa ödemenin gerçekleşmediğini söyler" parçası yoktu; `03 §2.5.1.3` yazıyordu | Eklendi (§6.2.1) | düşük |
| U-25 | 2 | `02 §6` 6.3.5 · karar kaydı K-710 | Satırın gövdesi K-710'u anıyor, Kaynak sütunu anmıyordu; K-710'un etki sütunu 6.3.5'i saymıyordu | Kaynak'a K-710; etki sütununa 6.3.5 | düşük |
| U-26 | 7 | `02` bölüm sonu Kaynak satırları (§1, §3–§12; on üç satır) | Gövdede anılan Aşama 2 kararlarının kırk biri Kaynak satırında yoktu — v0.54'ün on beş kararından hiçbiri (CP01 B-20'nin sınıfı) | Kaynak satırları betikle tamamlandı; checkpoint düzeltmelerinin getirdiği atıflarla on dört satıra altmış üç atıf, "(Aşama 2 checkpoint'i — Kaynak tamamlandı)" | düşük |

### 3.2 Ürün Gereksinimleri ↔ MVP Kapsamı

| # | Kontrol | Yer | Bulgu | Kapanış | Seviye |
|---|---|---|---|---|---|
| U-9 | 3 | `10` KP-22 | Pencere kapandıktan sonraki kaydı yalnız eksik bilgilendirmeye bağlıyordu — K-709'un düzelttiği dar okuma (firma süresinde postalanmış caymayı geç sayabilirdi); v0.36 notu K-709'u "kapsam satırı değiştirmez" kümesine koymuştu | "…süresinde gönderilmiş ama geç ulaşmış bildirimde ve eksik bilgilendirmede …; süreye uygunluk bildirimin süresinde gönderilmesiyle ölçülür"; Kaynak'a K-709, `02 §10.4.10` | **orta** |
| U-27 | 3, 6 | `10` KP-47 | K-668'in (⚠) `10`'da izi yoktu — v0.30 notu onu ve K-659'u hiç anmıyordu; KP-47 iade yürütümünü ve yalnız koşullu istisnadaki reddi anlatıyor, kullanılmış ya da hasarlı dönen malda sessizdi (CP01'in K-236 ve K-184 emsali) | "Geri ödeme kalemin ödenmiş bedelinin tamamıdır: kullanılmış, hasar görmüş ya da eksik dönen malı — koşullu istisna kaleminde ambalajı açılmış mal dışında — reddedemez ve geri ödemeden kesinti yapamaz; değer kaybı talebi ürünün dışındadır…"; Kaynak'a K-668, `02 §7.4.10`. Aşama 1'in kalıbı: kural sınırı KP satırında cümle, olmayan yetenek KD satırında — bu bir geri ödeme kuralıdır | düşük |
| U-28 | 3 | `10` KP-66 | Firmanın başka kanaldan kaydettiği gecikme feshi yoktu (K-706); Kaynak'ta K-690 ve K-706 yoktu | "…caymayı ya da gecikme feshini panelden kaydetmesi…"; Kaynak'a K-690, K-706, `02 §10.4.10` | düşük |
| U-29 | 3 | `10` KP-17 | Donan değerlerin tam listesinde havale IBAN'ı (K-663, `02 §3.23.3`) yoktu — yalnız KP-59 ve KP-60'ta geçiyordu (CP01'de KP-17'ye aynı gerekçeyle iki değer eklenmişti) | Eklendi; Kaynak'a K-663 | düşük |
| U-30 | 3 | `10` KP-48, gerekçe | "iç not, teslim tarihi düzeltmesi ve 'mal dönmedi' kapatması bildirilmez" — v0.36 gövdeyi güncellemiş, gerekçeyi güncellememişti (K-712) | "…teslim tarihinin ve iade malının ulaşma tarihinin düzeltilmesi…" | düşük |
| U-31 | 3 | `10` KP-27 · KP-46 | KP-27 şifre değişikliğinde tanınan tarayıcı işaretlerinin düştüğünü (K-695, `02 §3.13.10`) yazmıyordu; KP-46'nın Kaynak'ında kararın evi `02 §10.1.2` yoktu, K-714'ün etki sütunu `03`'ün yeni satırını 8.2.10 diye anıyordu (doğrusu 8.2.11) | KP-27'ye eklendi, Kaynak'a §3.13.10, §8.2.6; KP-46'ya §10.1.2; K-714'ün etki sütunu düzeltildi | düşük |

### 3.3 Kullanıcı Akışları'nın kendi içi

| # | Kontrol | Yer | Bulgu | Kapanış | Seviye |
|---|---|---|---|---|---|
| U-33 | 3 | `03` 7.2.15 · §11'in 11.2, 11.23, 11.26, 11.36 satırları | K-701'in devralma kuralına göre okunduğunda beş hücrede `02`'yi göstermesi gereken önsüz `§`'ler zincir bittikten sonra geliyordu — parantez kapanışı, ` · ` ya da önek hiç yok (11.36: "Z-44, §12.2.7") — ve `03`'ün olmayan bölümlerine (§10.6.1, §5.8, §7.4.7, §6.7.14, §12.2.7…) gidiyordu | Zincir önek ya da parantezsiz yazımla onarıldı; betik yeniden koştu — yanlış dokümana giden atıf kalmadı | düşük |
| U-34 | 3 | `03 §0.3.2` ↔ `10 §2` | "Kullanıcı" `10 §2`'de *"aktör değildir; satırın kapsadığı aktörlerin ortak adıdır"* (K-638), `03`'te adım tablolarının dar tanımlı bir aktör adıdır (K-700) — aynı sözcük iki tanım | `03 §0.3.2`: "…MVP Kapsamı'nın satırlarındaki 'Kullanıcı' ortak adından ayrıdır: `10 §2`, K-638" | düşük |
| U-38 | 3 | `03` 6.1.2.2 | Aktör hücresi "Engellenen kişi bekler" — §0.3.2'nin kapalı listesinde "kişi" yok ve olay cümlesi değil (K-702) | "Zaman geçer ya da — L-8'de — açık siparişlerden biri kapanır" | düşük |

### 3.4 Park blokları ve geriye dönük etki

| # | Kontrol | Yer | Bulgu | Kapanış | Seviye |
|---|---|---|---|---|---|
| U-10 | 6 | `04` "Aşama 1'den park edilen girdiler", K-14 satırı | *"ETBİS doğrulama bandı sitede sürekli erişilebilir olur"* — K-574 bandı isteğe bağlı bir alana bağladı (*"boşsa band görünmez"*) ve "alan boşken band genel bir ifadeyle görünsün" seçeneğini eledi; park satırı tasarımcıyı elenen yer tutucu banda yöneltebilirdi. Çakışma taraması K-14'ü yalnız K-259 ve K-480'e karşı denetlemişti | Etiket "K-14 · K-574"; "…ve — firma ETBİS doğrulama bilgisini girdiyse — doğrulama bandı … alan boşsa band görünmez (K-574)". K-14'ün etki sütununda K-574'e geri işaret zaten vardı | **orta** |
| U-11 | 6 | `06` K-17 satırı (`07`, `09` aynı köken) | *"Bu doküman sözlüğü birebir devralır, isim icat etmez"* — K-531 kapalı listelerin değerlerinin kod adlarını `06`'ya bıraktı; park satırı `06`'ya düşen işi yasak gibi gösteriyordu. K-17'nin etki sütununda K-531'e geri işaret yoktu (çakışma taramasının K-06 ↔ K-97 sınıfı) | `06`: "**Tek istisna:** kapalı listelerin … değerlerinin kod adlarını bu doküman verir (K-531, `02 §1.2`)"; `07` ve `09`: kapalı liste değerleri `06`'nın adlarından; K-17'nin etki sütununa K-531 geri işareti | **orta** |
| U-32 | 6 | `05` K-666 · K-667 park satırı | Dönüşte çalışan işlerin listesi hatırlatmayı koşulsuz sayıyordu; K-666'ya göre hatırlatma yalnız havale süresi hâlâ işliyorsa gider, K-685'e göre ertelenen iptalde yeni hatırlatma gitmez; kart hattında iptal son sorguyladır | Liste iki koşulu taşır (K-685, `02 §6.1.2`) | düşük |
| U-35 | 6 | `02` ve `10` başlığının "Karar referansları" notu | *"Kayıt aşama kapanışında arşiv işareti alır"* (K-436) — K-647 kaydı dönem boyu tek dosya yaptı, arşiv işareti aşamanın kayıtlarını salt okunur yapar | "…kayıt doküman döneminin tek dosyasıdır: aşama kapanışında o aşamanın kayıtları arşiv işareti (salt okunur) alır …, dosyanın tamamı dönemin sonunda arşivlenir (K-436, K-647)" | düşük |

### 3.5 Doküman durumu

| # | Kontrol | Yer | Bulgu | Kapanış | Seviye |
|---|---|---|---|---|---|
| U-36 | 2 | Karar kaydı §1, "`03` sürüm geçmişi" notu | v0.10 girdisi yoktu; not "Sırada cross-review 3. tur" diye bitiyordu — PR #69 tabloyu ve sürüm başlığını güncellemiş, notu güncellememişti | v0.10 ve v0.11 girdileri | düşük |
| U-37 | 2 | `02` ve `10`'un dosya sonu notu | *"v0.48…v0.54 Aşama 2'nin geri beslemesidir … — `03`'ün etki yansıtmasında yeniden taranır"* — tarama v0.67'de yapılmıştı | "…ve `03`'ün etki yansıtmasında tarandı · v0.55 (v0.37) Aşama 2 checkpoint'i" | düşük |

**Sayım:** otuz sekiz bulgu (U-1…U-38) — **orta 10** (U-1, U-3…U-11), **düşük 28**. Hepsi kapandı.

---

## 4. Etki işaretleri

Checkpoint'in düzeltmeleri var olan kararların yansımasıdır; karar kaydında şu kırk dört Aşama 2 kararının etki sütununa `(taslak güncellendi — vX.Y; checkpoint)` ya da `(Kaynak — v0.55; checkpoint)` parçası eklendi: K-647, K-656, K-657, K-659, K-660, K-661, K-662, K-663, K-665, K-666, K-668, K-671, K-672, K-673, K-674, K-675, K-680, K-684, K-685, K-686, K-690, K-692, K-695, K-696, K-697, K-698, K-700, K-701, K-702, K-703, K-704, K-705, K-706, K-707, K-708, K-709, K-710, K-711, K-712, K-713, K-714, K-715, K-716, K-717. Aşama 1'in salt okunur kayıtlarından yalnız K-17'nin etki sütununa K-531 geri işareti düştü — kaydın tek istisnası (tracker başlığı); K-574 ve K-14'e dokunulmadı (K-14'te geri işaret zaten vardı).

---

## 5. Proje sahibinin gözden geçirme listesi

### 5.1 ⚠ işaretli otuz dokuz karar

Kaynak: [çakışma taraması §4.1](PHASE2_CONFLICT_SCAN.md) — kayıtla bire bir (39 / 39). Bu kararlar proje sahibinin Aşama 2 açılışındaki talimatıyla (K-648, *"kolay soruları sorma, çok kritik konuları sor"*) sorulmadan kaydedildi ve `(öneriyle kaydedildi — ⚠)` işaretini taşır — kişisel veri, para ya da yasal hak, varlık ya da önceki bir karardan ayrılma. Oturum raporlarında yöneticiye gösterildi, **proje sahibine gösterilmedi.** **Kapı:** liste arşiv işaretinden (K-437'nin 6. adımı) önce proje sahibine tek listede, karar başına bir sade cümleyle gösterilir (`INSTRUCTIONS.md` §2); itiraz gelen karar yeni bir karar satırıyla değişir, itiraz gelmeyen satırın işareti `(öneriyle kaydedildi — ⚠ — gözden geçirildi YYYY-AA-GG, itiraz yok)` olur. Bu checkpoint listeye karar eklemedi ve listeden karar çıkarmadı.

| Karar | Adım | Konu | Tek cümle |
|---|---|---|---|
| K-656 | Workshop | Koşullu cayma istisnasında iade malının reddi | Hijyen ürünü, kitap gibi koşullu istisna kaleminde ambalajı açılmış dönen malı firma teslim almada reddedebilir; geri ödeme yapılmaz, ret geri alınmaz. |
| K-657 | Workshop | Reddedilen iade malının dönüşü ve bildirimi | Reddedilen mal firmanın bedeliyle müşterinin teslimat adresine geri gönderilir ve müşteriye "İade reddedildi" e-postası (B-16) gider. |
| K-660 | Workshop | Yönetici davetinin geri çekilmesi | Kullanılmamış yönetici daveti geri çekilebilir; davetliye haber gitmez, gönderme ve geri çekme işlem izine yazılır. |
| K-662 | Workshop | Daveti gönderen yöneticinin kaldırılması | Kaldırılan yöneticinin gönderdiği kullanılmamış davetler kaldırmayla birlikte geçersizleşir. |
| K-663 | Workshop | Ödemesi beklenen havale siparişinde firmanın IBAN'ı | Havale IBAN'ı siparişe donar; panelde IBAN değişirse yalnız yeni siparişlere işler. |
| K-664 | Workshop | Satış kapısının bir koşulunun düşmesi | Havalenin kapatılması dahil satış kapısının bir koşulunun düşmesi açık siparişleri etkilemez. |
| K-665 | Workshop | İade adresinin değişmesi | Cayma ve ayıp beyanı o günkü iade adresini kayda yazar; adres sonradan değişirse eski adrese gelen malı firma teslim alır. |
| K-666 | Workshop | Sitenin kesintisinde süreler | Site kesintisinde süreler dondurulmaz, kaçan işler dönüşte sırayla çalışır; yasal süreler hiç dondurulmaz. |
| K-667 | Workshop | Havale süresinin kesintide dolması | Havale ödeme süresi kesintide dolarsa kendiliğinden iptal, dönüşten sonraki ilk iş gününün sonuna ertelenir. |
| K-668 | Workshop | Kullanılmış ya da hasarlı dönen mal — değer kaybı | Kullanılmış ya da hasarlı dönen malda geri ödeme tam tutardır; değer kaybı talebi ürünün dışındadır. |
| K-673 | Yazım turu | Hizmet kaleminde cayma düğmesinin açıldığı an | Hizmet kaleminde cayma düğmesi ödeme onayından sonra açılır; ödeme beklenirken yol siparişin iptalidir. |
| K-678 | Yazım turu | İletişim formunu gönderene alındı e-postası | İletişim formunu gönderene alındı e-postası gönderilmez. |
| K-680 | Yazım turu | Ödeme adımında yeni adres ve adres defteri | Üye ödeme adımında yeni adres yazabilir; adres siparişe donar, üye isterse deftere de kaydedilir. |
| K-681 | Yazım turu | Ayıp talebinin müşterice yeniden açılmasında bildirim | Müşteri çözülmüş ayıp talebini yeniden açınca kendisine kayıt kopyası e-postası (B-9) gider. |
| K-683 | Yazım turu | Hata ve sistem olaylarında bildirim yokluğu | Başarısız kart iadesi, yanıtsız son sorgu, indirme hakkının yenilenmesi, dosya güncellemesi ve doğrulanmamış kaydın silinmesi bildirim üretmez. |
| K-684 | Yazım turu | Geç gelen kart ödemesinin iadesinde müşteriye bildirim | İptal edilmiş siparişe geç gelen kart ödemesi kendiliğinden geri ödenince müşteriye geri ödeme e-postası (B-8) gider. |
| K-686 | Yazım turu | IBAN'ı olmayan havale geri ödemesinde IBAN isteği | Havale hattında IBAN'ı olmayan bir geri ödemede (ayıp çözümü, tutar bazlı geri ödeme vb.) firma panelden IBAN ister; IBAN'ı kendisi girmez. |
| K-687 | Yazım turu | Giriş kaydının okunduğu yer | Giriş kaydı panelde görünmez; incelemede barındırma tarafında okunur. |
| K-688 | Yazım turu | Cayma beyanının geri alınması | Cayma beyanı geri alınmaz ve düzeltilmez. |
| K-690 | Yazım turu | Firma iptalinde ve kalem çıkarmasında geri ödemenin IBAN'ı | Havale hattında ödenmiş kalemi firma iptal edince ya da çıkarınca müşteriden IBAN isteği kendiliğinden düşer ve e-posta (B-14) gider. |
| K-692 | Yazım turu | Yönetici daveti ve kaldırmanın bildirimi | Yönetici daveti gönderildiğinde ya da bir yönetici kaldırıldığında bütün yöneticilere e-posta (F-6) gider. |
| K-693 | Yazım turu | Hesabın silinmesinde bildirim | Hesap silindiğinde hesabın adresine bir kez "hesabınız silindi" e-postası gider. |
| K-694 | Yazım turu | Hesap olaylarında bildirim yokluğu | Doğrulama, giriş, şifre değişikliği, ad ve adres değişikliği gibi hesap olayları bildirim üretmez. |
| K-696 | Yazım turu | Google ile ilk girişte doğrulanmamış bekleyen kayıt | Google ile ilk girişte aynı adresin doğrulanmamış bekleyen kaydı devralınmaz — silinir, hesap Google ile şifresiz doğar. |
| K-698 | Yazım turu | Geri alma bağlantısı açıkken hesap silme | E-posta değişikliğini geri alma bağlantısı açıkken (yedi gün) hesap silinemez. |
| K-699 | Yazım turu | Panele erişimin kaybı — kurtarma | Hiçbir yönetici panele giremezse kurtarma ürünün dışındadır; kurulumu yapan, ilk yöneticiyi açtığı yolla yeni yönetici açar. |
| K-703 | Audit ve deep review | Durum düzeltmesi ve yanlış aralıkta doğmuş kalem kayıtları | Siparişte iptal, çıkarma, fesih ya da cayma kaydı varsa havale "ödendi" işareti düzeltilemez; yanlış aralıkta doğan kayıtlar düzeltmeden sonra kalır. |
| K-704 | Audit ve deep review | Siparişin alındığının sitede gösterilmesi | Site, sipariş oluşunca iki ödeme yolunda da siparişin alındığını ve numarasını gösterir (Hizmet Sağlayıcılar Yönetmeliği m.9/1). |
| K-705 | Audit ve deep review | Sonucu belirsiz kart işlemleri | Başarısız kart iadesinde panel, yeniden denemeden önce iadenin sağlayıcıda gerçekleşip gerçekleşmediğine de bakmayı hatırlatır; iki teknik iş Entegrasyon Spesifikasyonu'na park edildi. |
| K-706 | Audit ve deep review | Başka kanaldan gelen gecikme feshi | Müşteri gecikme feshini e-postayla ya da başka bir kanaldan bildirirse firma onu panelden kaydeder. |
| K-707 | Audit ve deep review | "Sipariş hakkında" talebinin saklama süresi | "Sipariş hakkında" tipindeki iletişim talebi üç yıl saklanır; öteki tipler iki yılda kalır. |
| K-708 | Audit ve deep review | İmha kaydının kapsamı | Kişisel veriyi hemen silen işlemler (hesap silme, bekleyen kaydın silinmesi, IBAN silme) de imha kaydına yazılır. |
| K-709 | Audit ve deep review | Başka kanaldan caymada süreye uygunluk | Başka kanaldan gelen caymada süreye uygunluk bildirimin süresinde gönderilmesiyle ölçülür; kaydın tarihi ulaşma tarihi kalır. |
| K-710 | Audit ve deep review | Müşterinin IBAN girişi ve düzeltmesi | Müşterinin IBAN girişi ve düzeltmesi bildirim üretmez. |
| K-711 | Audit ve deep review | Yeniden doğrulamada başarısız şifre | Yeniden doğrulama ekranındaki başarısız şifre denemesi giriş limitine (L-1) sayılır. |
| K-712 | Audit ve deep review | İade malının ulaşma tarihinin düzeltilmesi | İade malının ulaşma tarihi teslim tarihi gibi düzeltilebilir; geri ödeme süresi düzeltilmiş tarihten işler. |
| K-713 | Audit ve deep review | İndirme hakkının siparişe donması | Dijital kalemin indirme hakkı sipariş anında donar; ayarın değişmesi yalnız yeni siparişlere işler. |
| K-714 | Audit ve deep review | Panelde siparişi bulma | Panelin sipariş listesinde numarayla ve e-postayla arama ile durum süzgeci vardır. |
| K-716 | Audit ve deep review | Ayıp çözümünde geri ödemenin zamanı | Ayıp talebi sözleşmeden dönme ya da bedel indirimiyle çözülürse para derhâl geri ödenir; ürün süre sayacı tutmaz. |

**Sayım:** workshop 10 · yazım turu 16 · audit ve deep review 13 — **toplam 39**. Betikle sayıldı; otuz dokuzunun da konu hücresi ⚠ işaretini taşıyor ve çakışma taramasının §4.1 listesiyle birebir.

### 5.2 Avukat teyidi önerisi — aynı mesajda

Kaynak: Aşama 1'in devir notu (tracker §7) ve çakışma taraması §4.2; kapı tracker §8.1. İki kararın dayandığı okuma resmî metnin açık hükmü değil, lafzından çıkan bir sonuçtur; **öneri:** ikisi bir avukata teyit ettirilir.

| Karar | Konu | Kalan risk (tek cümle) |
|---|---|---|
| K-491 | Teslimden sonraki caymada geri ödeme süresinin başlangıcı | Firma iade taşıyıcısı belirlemediği için on dört gün malın firmaya ulaştığı tarihten işler; tüketici lehine bir yorum başlangıcı malın kargoya verildiği güne çekebilir. |
| K-627 | KEP adresinin üç firma tipinde zorunlu olması | Yükümlülük esnaf için dar okunursa ürün KEP'i olmayan esnafın satışını gereksiz yere tutar; geniş okunursa bugünkü kural doğrudur. |

**Kapı:** öneri ⚠ listesiyle aynı mesajda proje sahibine sunulur — kabul ya da ret. Kabul edilirse aynı PR'da `Docs/DEFERRED_BACKLOG.md`'ye kalem olarak girer (hedef MVP Kapsamı ÖK-12'nin referans kurulumundan önce, bloklar: MVP kabulü); teyit itiraz getirirse karar yeni bir satırla değişir. Ret gelirse madde gerekçesiyle kapanır.

---

## 6. Öğrenim terfisine notlar — 5. adımın girdisi

Adayların metinleri tracker §8.1'dedir; bu checkpoint iki aday ekledi ve birine kanıt verdi.

| # | Aday | Kaynak | Playbook |
|---|---|---|---|
| 9 (kanıt eki) | Park satırı kaynak kararı değişince güncellenmedi — çakışma taramasının park taraması yalnız ilişki sözcüğü taşıyan sonraki kararları aradı ve K-14 → K-574 ("netleştirir"), K-17 → K-531 ("istisna") çiftlerini kaçırdı | U-10, U-11 | PF-45 |
| 11 | Bir kuralı genişleten geri besleme kararı kuralın bütün yüzlerine yansımadı — K-706 dokuz yerde, K-686, K-656, K-716 daha az yerde | U-1…U-5, U-8 | PF-47 |
| 12 | Geri beslemede bölüm sonu Kaynak satırları güncellenmedi — kırk bir eksik atıf; CP01 B-20'nin tekrarı | U-26 | PF-48 |

**Tek seferlik gözlem (raporda kalır):** kayıt §1'in "`03` sürüm geçmişi" notunun v0.10'u taşımaması (U-36) — PR #69 sürüm başlığını ve tabloyu güncelledi, notu atladı; K-38'in kuralı vardı.

---

*Kaynak: K-437 (kapanış sırası, 4. adım) · K-429 (çakışma taraması — girdi) · K-652 (geri beslemenin işletimi — ✓ ve döngüler yeniden açılmaz) · K-29 (etki işaretleri) · K-647 (kaydın evi; Aşama 1 kayıtlarının tek istisnası geri işaret) · K-648 (⚠ öneriyle kayıt ve liste kapısı) · K-701 (atıf dizisinde devralma) · `.claude/skills/checkpoint/SKILL.md` (yedi kontrol ve çıktı biçimi) · `Docs/CHECKPOINT_REPORTS/CP01_PHASE1_CHECKPOINT.md` (kapsam ve derinlik örneği) · `Docs/CHECKPOINT_REPORTS/README.md` (rapor türü ve retro güncelleme).*
