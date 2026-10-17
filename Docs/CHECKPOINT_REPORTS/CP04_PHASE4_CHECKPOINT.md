# Aşama 4 — Checkpoint (CP04)

**Tarih:** 2026-10-17 | **Aşama:** 4 — Teknik mimari | **Kapanış adımı:** K-437'nin 4. adımı (`/checkpoint`; `checklists/document-stage.md` §6 son madde, §7 madde 4)
**Girdi sürümleri:** karar kaydı v0.113 · Teknik Mimari (`05`) v0.15 · Ürün Gereksinimleri (`02`) v0.75 · Kullanıcı Akışları (`03`) v0.27 · Arayüz Tanımları (`04`) v0.27 · MVP Kapsamı (`10`) v0.50 · Proje Vizyonu (`01`) v0.33 · `06`–`09`, `11`, `12` ve `DEPLOY_RUNBOOK`'un park blokları · `DEFERRED_BACKLOG.md` — `origin/main` @ `0b10dbb`
**Çıktı sürümleri:** karar kaydı v0.114 · Teknik Mimari v0.16 · Ürün Gereksinimleri v0.76 · Kullanıcı Akışları v0.28 · Arayüz Tanımları v0.28 · MVP Kapsamı v0.51 · Proje Vizyonu değişmedi · `06`, `07`, `08`, `09`, `12` ve `DEPLOY_RUNBOOK`'un park satırları · `DEFERRED_BACKLOG.md` D-01 · `PLAYBOOK_FEEDBACK.md` PF-74
**Girdi raporu:** [Aşama 4 — Çakışma Taraması](PHASE4_CONFLICT_SCAN.md) (K-437'nin 3. adımı) — açık kalem durumu, ⚠ listesinin hazır metni (§4.1), devir dizini (§6) ve checkpoint'e notlar (§8) oradan alındı. **K-976'nın devri** — Teknik Mimari'nin cross-review'ının bıraktığı "iki yerin metin farkı ve güvence cümlesi ile mekanizmanın farkı" sınıfı — karar kaydı §12.1'in checkpoint maddesinden.

> **Ne işe yarar:** Aşama kapanmadan önce Teknik Mimari'nin kendi içinde, Aşama 4'ün geri beslediği dokümanlarla ve park satırlarıyla karar kaydına ve metodolojiye karşı tutarlı olduğunu doğrular; bulunan her uyumsuzluğu ya kapatır ya da adıyla proje sahibine bırakır. Sonuç öğrenim terfisinin (5. adım) ve arşiv işaretinin (6. adım, K-436 — kapsamı K-647) girdisidir.

---

## Checkpoint Sonucu — 2026-10-17

**Aşama:** 4 — Teknik mimari (kapanış sırası K-437: yazım turu ✓ · kalite döngüsü ✓ · çakışma taraması ✓ · **checkpoint** · öğrenim terfisi · arşiv işareti)
**Genel durum:** ⚠ Dikkat gerektiren noktalar — seksen iki bulgu (on beş orta, altmış yedi düşük); hepsi bu checkpoint'in PR'ında kapandı — elli ikisi var olan kararların ya da dokümanın kendi kuralının yansımasıyla, otuzu dokuz yeni kararla (K-980…K-988; üçü ⚠ — K-982, K-983, K-984). Çok kritik konu yok. Kapanıştan sonra durum **✓ Yolunda**; açık kalan kapı kırk dört ⚠ kararın proje sahibine gösterilmesidir (arşiv işaretinden önce, §5).

### Kontrol Özeti

| # | Kontrol | Sonuç | Detay |
|---|---|---|---|
| 1 | Yol haritası | ✓ | Aşama 4'teyiz (`00 §C.1`: rol Senior Software Architect, girdi `01`–`04`, `10`, çıktı `05`; izlenebilirlik matrisi zorunlu değil). Açılış (PR #98), workshop üç oturum (#99–#101), tek yazım turu iki oturum (#102, #103), kalite döngüsü — audit ve deep review (#104), çok kritik soru (#105, K-951), cross-review dokuz tur, iki güvence merceği, 9. turda K-976'nın çıkış kuralıyla TEMİZ ve etki yansıtma (#106–#114) — ve çakışma taraması (#115) bu sırayla kapandı. Atlanan ya da sırası değişen adım yok (K-437, K-432, K-430). |
| 2 | Doküman durumu | ✓ | Başlık ve dosya sonu sürümleri kayıt §1 ile aynıydı (`01` v0.33 / 2026-10-04; `02` v0.75, `03` v0.27, `04` v0.27, `05` v0.15, `10` v0.50 / 2026-10-17). Şablon dokümanlarda (`06`–`09`, `11`, `12`) başlık v0.1 / `YYYY-AA-GG`, dosya sonu aynı; park bloğu eklemeleri başlığa dokunmaz. Bu PR'da beş doküman ve kayıt birlikte arttı; `05`'in Checkpoint sütunu ✓ oldu. |
| 3 | Tutarsızlık | ⚠ → ✓ | **K-976'nın devri** — iki yerin metin farkı: kırk beş bulgu, beşi orta (§3.1); güvence cümlesi ile mekanizmanın farkı: yirmi üç bulgu, yedisi orta (§3.2). **Yansıma kaçakları** — Aşama 4'ün geri beslediği kuralın öteki yüzü: on bulgu, ikisi orta (§3.3). Sayılar, kimlikler ve atıflar betikle tutarlı (Notlar). |
| 4 | Açık kararlar | ✓ | Tracker §4'te açık satır yok, A-19 açılmadı; Teknik Mimari'de "açık" işaretli madde yok; Ürün Gereksinimleri §13 boş (çakışma taraması §1, §3). §12.1'in açık süreç maddeleri kapısındadır: K-976'nın devri bu checkpoint'te kapandı; ⚠ listesi — §5; öğrenim terfisi adayları, Ö-23 ve aday 13 — 5. adım. Çakışma taramasının kapı verdiği çok kritik soru adayı (desteklenen ödeme sağlayıcısının sayısı) `08`'in park satırında, Entegrasyon spesifikasyonları aşamasının açılışında. |
| 5 | Aşama çıktıları | ✓ | `05` yazıldı ve şablonun bütün bölümleri doludur (§0 dört ilke ve on üç kısıt; §1 Mermaid diyagramı ve otuz üç satır; §2 MK-01…MK-12; §3–§11; §12 MK-01…MK-78, şablonun beş sütunuyla); **§6.x** zorunlu alt bölümü üç soruyu ve ek kuralları cevaplar; **§8** dal stratejisini kopyalamaz, `INSTRUCTIONS §3.2`'ye işaret eder (8.3.1; K-859 — PF-72). Kalite döngüsünden geçti, etkileri üst dokümanlara geri beslendi (K-652; `02` v0.67…v0.76, `03` v0.23…v0.28, `04` v0.21…v0.28, `10` v0.45…v0.51). Checklist'in Teknik mimari hatırlatması karşılandı: olay kaybı (§4.3, §4.4) ve denetim izi (§4.7) baştan kuruldu; maliyet kısıtı açılışta soruldu (K-861; §0.2). |
| 6 | Geriye dönük etki | ⚠ → ✓ | Proje Vizyonu'na etki yok. Önceki dokümanların eski kalan yüzleri §3.3'te kapandı — Ürün Gereksinimleri altı, Kullanıcı Akışları iki, Arayüz Tanımları yedi, MVP Kapsamı iki yer. **Veri modeli aşamasının girdileri hazır:** `00 §C.1`'in girdileri (`02`, `03`, `05`, `10`) ✓; `06`'nın dört park bloğu (Aşama 1–4) ve dizin satırları; çakışma taramasının §6 devir dizini (111 karar, 204 atıf) — bu checkpoint'in kararları (K-980, K-982, K-984, K-985, K-988) `06`'nın park satırlarına etiketiyle girdi. **Bir şablon uyumsuzluğu:** `06`'nın başlığındaki "Bağımlılıklar" alanı henüz yazılmamış `09`'u sayar, `00 §C.1` saymaz (C-82; PF-74) — Veri modeli aşamasının açılışının şablon taramasına (checklist §1) bırakıldı. |
| 7 | Yeni alan/kural | ⚠ → ✓ | Kuralın evinde eksik kalan mekanizmalar dokuz kararla yazıldı (K-980…K-988; §3.2, §3.3); yeni tasarım değerleri §11.1'e girdi (11.1.31, 11.1.32); D-01'de doğrulanacak üç "bilgim tarihli" olgunun evi yoktu — `05 §2.3.8` ve D-01'e girdi (C-81). Kaynak satırları (betik 2): bir eksik atıf (C-79). |

### Aksiyon Gerektiren Maddeler

- [x] Seksen iki bulgu düzeltmeyle kapandı — `05` v0.16, `02` v0.76, `03` v0.28, `04` v0.28, `10` v0.51, park satırları, D-01, karar kaydı v0.114 (§3)
- [x] Dokuz karar — K-980, K-981, K-985, K-986, K-987 (teknik mekanizma), K-988 (yerleşim), K-982 ⚠, K-983 ⚠, K-984 ⚠ (öneriyle kaydedildi); ⚠ listesi 41 → **44**
- [x] K-976'nın devri kapandı (karar kaydı §12.1)
- [x] Checkpoint'in dokunduğu Aşama 4 kararlarının etki sütununa `(taslak güncellendi — vX.Y; checkpoint)` parçası ya da yeni kararın geri işareti eklendi (§4); Aşama 1–3 kayıtlarında yalnız geri işaret (K-557, K-708, K-734)
- [x] **Kırk dört ⚠ kararın proje sahibine gösterilmesi** — kırk biri çakışma taramasının §4.1'inde hazır metniyle, üçü bu raporun §5'inde. Kapı: arşiv işaretinden (6. adım) önce, tek mesajda (`INSTRUCTIONS.md` §2, §9; tracker §12.1) — **kapandı (2026-10-17, kapısında; K-990):** liste tek mesajda gösterildi, itiraz yok, retro bölümü gerekmedi; sonuç §5.1'in sonunda
- [x] Sırada öğrenim terfisi (K-437'nin 5. adımı) — tracker §12.1'in on iki adayı (11 ve 12 bu checkpoint'te eklendi, §6), aday 13 ve Ö-23 — **kapandı (2026-10-17, PR #117, K-989)**
- [ ] Veri modeli aşamasının açılışı: `06` şablonunun bağımlılıklar alanı (C-82, PF-74) — kapısı Veri modeli aşamasının açılışı; devir karar kaydının §13'ünde (2026-10-17)

### Notlar

**Yöntem.** Mekanik kontroller betikle koştu: atıf çözümü, Kaynak satırları (K-652'nin betik 2'si), ⚠ listesinin kayıtla karşılaştırılması, K tekilliği, sürümler, MK kimlikleri; her betik başlamadan örneklendi — atıf betiği bilerek bozuk dört atıfla denendi ve dördünü de yakaladı; Kaynak betiğinin üç ilk sürümü (Kaynak satırını yalnız `###` içinde, yalnız satır başında, `(§4.1–§4.4)` aralığı olmadan arıyordu) örneklemde düzeltildi. Anlamsal tarama sekiz salt okuma merceğiyle, ayrı alt ajanlarda, **ön planda** ve beşli dalga + üç olarak koştu; her mercek bulgusunu bulduğu anda dosyaya yazdı: **L1** — başlık notunun konvansiyonları ve §0–§3'ün özet yüzleri ↔ dokümanın tamamı (~190 özet yer; 15 bulgu) · **L2a** — §12'nin 78 MK satırı ↔ evleri, §4–§6'nın kapalı listeleri ve ~95 iç atıf (10) · **L2b** — §7–§11'in kapalı listeleri, 44 kapalı liste ve 200'ü aşkın iç atıf (20) · **L3** — güvence ↔ mekanizma, güvence merceğinin koşmadığı §2, §3, §7.1–§7.3, §7.7–§7.10, §8.1, §8.3, §8.4, §8.9 (247 satır, 165 güvence: 148 tutuyor, 16 kısmen, 1 tutmuyor; 14) · **L4a**, **L4b** — geri besleme tablosunun yirmi satırının kuralları, nesne ve işlem terimleriyle, Türkçe ek varyantlarıyla (3, 7) · **L5** — güvence ↔ mekanizma, §9.1, §9.3, §10, §11.1, §11.3 (143 satır, 102 güvence: 81 tutuyor, 21 kısmen) ve K-966…K-979'un `05` içindeki yan yüzleri (47 değişiklik; 9) · **L6** — K-977…K-979'un yüzleri, PR #115'in değiştirdiği 40 park ve D-01 satırı, çakışma taramasının §6 sayımları (14). 92 ham bulgu, örtüşenler birleşince **82**. Düzeltmeler beş alt ajanın bölge bölge (`05` §0–§3 ve §12 · §4–§7 · §8–§11 · üst dokümanlar · park satırları ve D-01) yazdığı şartnameyle uygulandı: her ajan bölgesindeki bulguyu güncel metne karşı yeniden okudu (karşı-doğrulama — 82 bulgunun hepsi doğrulandı, çürüyen yok) ve birebir değiştirmeleri yazdı; yönetici 268 değiştirmeyi her birinde tam bir eşleşme isteyen bir betikle uyguladı, bölgeler arası dört yüzü (8.3.9 ↔ 2.3.5 ↔ MK-70; 6.x.8 ↔ 8.2.5; MK-17; 4.1.5'in atıf dizisi) ayrıca hizaladı. Düzeltmeler var olan kararların yansıması ya da bu checkpoint'in kararlarıdır — üst dokümanların ✓ durumu ve kalite döngüleri yeniden açılmadı (K-652). Ortam: Git Bash ve node, Python yok.

**Betik 1 — kuralların yüzleri.** Mercek L4a: ÖK-2 · K-888 · K-939 ve yansıması · K-916 · K-916/K-875 · K-951 · K-949 · K-934 — `01`, `02`, `03`, `04`, `10` ve `05`'te; sözlük, Z- ve P- satırları, bildirim içerikleri, `02 §12`, KP ve ÖK satırları, `03 §4.1`, `03 §10` ve `04 §1.1`'in matris özetleri ayrıca okundu; kuralın evinin Kaynak hücresinden geri izlendi. Temiz dönen kurallar: ÖK-2, K-916, K-934, K-916/K-875, K-949, K-939'un "on dakika" yansıması. Mercek L4b: K-904 (234 eşleşme) · K-940 (114) · çerezler K-907, K-942, K-950 (146) · K-968 (34) · K-977 (36) · K-885 (20) · K-959 (17). Temiz dönenler: K-959 (dört yetenek üç yüzde aynı), K-907/K-942/K-950 (`02 §12.2.5`'in yedi zorunlu çerezi `05 §6.7` ile birebir), K-885, K-940'ın ve K-904'ün çakışma taramasındaki yansımaları. Düzeltmenin geri beslemesinde (alt ajan S4) on dokuz terim grubu yeniden tarandı — ör. K-983'te "kaybolama-/sıfır kayıp" `02` 6, `03` 3, `04` 4, `10` 15 eşleşme; listeyi saymayan özetler (`02 §12.3.3`, `10 §1`, `04 §1.1`'in KP-75 özeti) değişmedi; K-982'de e-posta tekilliğini söyleyen öteki yüzler karşılaştırmanın biçimini söylemez — tek ev `02 §10.2.5`.

**Betik 2 — Kaynak satırları.** `05`'te gövdede anılan her K numarası bulunduğu alt bölümün Kaynak satırıyla — tablo satırında Kaynak sütunu varsa satırın hücresiyle — karşılaştırıldı (aralıklar açıldı; MK-, ÖK-, SK- kuyrukları süzüldü). Kapsam: `05`'in bütün kararları; `02`, `03`, `04`, `10`'da Aşama 4'ün değiştirdiği satırlar (`git diff 3e3a8e6`). Bulunan tek eksik: `04 §4`'ün Kaynak satırı E-21 ve E-54 satırlarının andığı K-904'ü taşımıyordu (C-79). Checkpoint düzeltmelerinin gövdeye getirdiği her K aynı PR'da Kaynak satırına girdi. **Son koşum boş.** Kapsam dışı: §12'nin satırları (gerekçe hücresi kaynaktır — C-6 ayrıca okundu); bölüm girişleri (`05 §8`'in girişindeki K-859 — §8.3'ün Kaynak'ındadır); `04 §1`'in matris özetleri (Kaynak satırı yoktur, K özetin kendisindedir); `03`'ün Kaynak sütunu olmayan tabloları (§1.6.2, §4.1, §7).

**Betikle doğrulanıp tutarlı çıkanlar:**

- **⚠ listesi:** konu hücresi `(öneriyle kaydedildi — ⚠` taşıyan K-855…K-988 satırları **44**; §12.1'in listesindeki numaralı K'lar **44** — iki yön boş. Aşama 4'ün 134 satırının işaret dağılımı: ⚠ 44 · `(öneriyle kaydedildi)` 84 · işaretsiz 6 (K-855, K-861, K-863, K-866, K-888, K-951 — proje sahibinin kararları).
- **K tekilliği:** komut boş; en büyük numara K-988.
- **Atıflar:** `05`'in 2.871 atfı — önsüz `§`, çıplak madde numarası (`03` ve `04` dizileri dahil), `0X §` atıfları, MK, K, Z-/P-/L-/H-/KP-/KD-/ÖK-/SK-/YH-/E- kimlikleri — çözülüyor; çözülmeyen 28'i geri besleme tablosunda parantezle bölünen `03`/`04` madde dizileridir (yanlış pozitif). Düzeltmenin yazdığı bir atıf dizisi (`02 §3.1.5`'in ardından gelen "§7.10.1") konvansiyon 6 gereği `02`'ye okunurdu — noktalı virgülle ayrıldı. Öteki dokümanlara eklenen 178 `05 §…` atfı çözülüyor. 264 + 9 farklı K — hepsi kayıtta.
- **MK:** 78 kimlik, 78 §12 satırı, her biri tek evde.
- **Sayılar:** çakışma taramasının §6.1 sayımları (204 atıf, 111 karar, 197 çift, hedef başına sayılar, altı dizin satırı) etki sütunlarıyla birebir (L6).
- **Sürümler:** tracker §1, başlıklar ve dosya sonu notları birebir; bu PR'da birlikte arttı.

**İşlenmeyenler — düzeltme gerektirmedi:** (1) 3.1.2 "aynı anda iki örnek yoktur" ile 5.1.3'ün "güncellemede eski ve yeni örnek bir an üst üste binse bile" cümlesi — ikincisi savunmada derinliktir. (2) Sırasız madde kimlikleri (8.5.9–8.5.11, 9.4.16–9.4.17, 11.1.20–11.1.30, 5.2.15, 2.3.8) konvansiyon 2–3'ün "yeniden numaralanmaz" ilkesinin sonucudur. (3) `02` 6.4.24, `03` 3.3.24 ve 8.3.3.4'ün başlık hücresi "başarısız olur — sağlayıcı reddeder ya da ulaşılamaz" der; gövde iki dalı (K-940) kalın başlıkla ayırır ve yolları doğru verir. (4) `05` 4.6.2 sonucun bilinmemesine "isteği gönderdikten sonra çöken süreç" sebebini ekler; ürün tanımı "bilinmezken" diye geneldir ve panelin görünümü aynıdır (K-894). (5) `02 §9.4`'ün "kaydı kendisi yapmadıysa bağlantıya tıklamayın" uyarısı K-904'ten sonra da koruyucudur. (6) `04` 5.21.8'in "kullanılmış" bağlantısının yeniden açılışı — doğrulama gerçekleşip yanıt kaybolduysa "değişiklik geçerli değil" der; düzeltmesi yeni bir ekran kuralıdır ve geri alma ekranında aynı kalıp vardır (5.28.4) — bulgu değil, Veri modeli ve API tasarımı aşamalarında bağlantının hâlleri yazılırken okunur. (7) K-977'nin listesi örnektir; `05`'in kurduğu öteki kayıtlar (oturum, tanınan tarayıcı, yedeğin durum kaydı) K-988'in tanımıyla kapsanır. (8) Merceklerin "İşlenmeyenler" bölümlerindeki öteki gözlemler çalışma dosyalarındadır; hiçbiri ürün kuralını ya da güvenceyi değiştirmez.

---

## 3. Bulgular ve kapanışları

Seviye: **orta** — iki yer çelişiyor; kapalı liste, sayı ya da hedef yanlış; güvence mekanizmasıyla tutmuyor; ya da source-of-truth'ta bir kural eksik; **düşük** — ifade, kapsam ya da atıf farkı, anlamı değiştirmez. Kontrol sütunu skill'in yedi kontrolüne gider. Yer sütununda önsüz numara Teknik Mimari'nin maddesidir. Mercek kimlikleri çalışma dosyalarınındır.

### 3.1 Teknik Mimari'nin iç tutarlılığı — iki yerin metin farkı (K-976'nın devri)

| # | Kontrol | Yer | Bulgu | Kapanış | Seviye |
|---|---|---|---|---|---|
| C-1 | 3 | 1.2.20, 3.4.3, 4.3.1, 5.1.4, §5.2, 10.1.1 ↔ 4.3.4, 4.5.2, 4.6.2 | İş kuyruğunun işlerinin listeleri gelen sağlayıcı bildiriminin işlenmesini ve sonucu bilinmeyen kart iadesinin sorgusunu saymıyordu; §5.2'nin envanterinde ikisi de yoktu (L1-1, L2a-4, L2b-3) | Altı yüz sayar; §5.2'ye **5.2.15**, 5.2.11'e sorgu, 5.2.1'e büyük veri dönüşümü (§8.7.5) | **orta** |
| C-2 | 3 | MK-04, §12 MK-04 ve MK-17 ↔ 3.6.1; `07` K-868 satırı | "Dış servisten istek alan uç imzalıdır" — 3.6.1 ilkeyi "ürüne veri getiren istek"e daraltıp canlılık ve durum uçlarını dışarıda bırakmıştı; durum ucu dış kontrolden istek alır ve imzasızdır (L1-13, L3-13, L6-7) | Dört yüz 3.6.1'in diliyle | **orta** |
| C-3 | 7 | MK-04, 1.2.7, 3.7.4; `07` | robots.txt makine uçlarının kapalı listesinde yoktu ve üreteni yazılı değildi (L1-14) | Site haritasıyla aynı yoldan bir makine ucu; listeler ve `07` | düşük |
| C-4 | 3 | 6.x.19, 6.x.8, 7.1.3, 3.2.6 ↔ 8.5.6, 9.4.10 | Durum komutu dağıtım yarıçapı tablosunun hiçbir satırında yoktu; salt okuma kullanıcısıyla çalışırsa 9.4.10'un listesini okuyamaz; salt okuma kullanıcısının yerleri dönüş denetimini saymıyordu (L2a-5, L2b-1, L2b-4) | 6.x.19'a durum komutu, uygulama kullanıcısıyla (K-986 (8)); üç özet hizalandı | **orta** |
| C-5 | 3 | §12'nin girişi ↔ gerekçe hücreleri | "Kararların hepsi … 2026-10-16'da alındı" — on sekiz kalite döngüsü kararı 2026-10-17 tarihlidir (L2a-1) | "Tarih" kararın ilk alındığı gündür; daralmaların K'ları gerekçededir | düşük |
| C-6 | 7 | §12'nin gerekçe hücreleri ↔ MK evlerinin Kaynak satırları | K-959, K-963, K-969, K-970, K-971, K-974, K-979 ve ikinci halkada K-951, K-965, K-967, K-973, K-975 maddesini değiştirdikleri MK'nin satırında yoktu (konvansiyon 2, 7) (L2a-2) | Otuz dört gerekçe hücresine K parantezi; bu PR'ın kararları dahil | düşük |
| C-7 | 3 | 1.2.18, 3.3.11 ↔ 6.3.2, 6.7.6, 7.2.6, 3.7.1 | Ortak altyapı özeti iki kalemi düşürüyordu; tanınan tarayıcı kaydının sahibi yoktu (L1-2, L1-3) | Yuvarlama işlevi, iş günü takvimi, adresin üretilmesi ve tanınan tarayıcı kayıtları | düşük |
| C-8 | 3 | 1.2.22, 3.2.2 ↔ 7.7.1, 7.4.5 | "Ürünün bütün kalıcı durumu" — dosyalar ve uygulama kaydı veritabanının dışındadır (L1-4) | "Dosyalar ve kayıt dosyaları dışındaki" | düşük |
| C-9 | 3 | 0.2 | Ücretli yönetilen servis yasağı aynı hücredeki ücretli nesne depolama ve e-postayla çelişik okunuyordu; K-862'nin parantezi yoktu (L1-5) | Yönetilen veritabanı, kuyruk, önbellek, kayıt ve izleme servisi | düşük |
| C-10 | 3 | 3.6.1, §12 MK-17 ↔ §10.1 | "Üç dış servis vardır" — §10.1 dokuz dış bağımlılık sayar (L1-6) | "Uygulamanın çağırdığı üç dış servis"; altyapınınkiler §10.1'de | düşük |
| C-11 | 3 | 3.6.2, 4.3.2, 10.1.3 → §4.5.3; 4.1.4 → §0.13 | Atfın hedefi atfedileni söylemiyordu: yanıtsız son sorguda iptalin beklemesi 5.2.2 ve 5.3.3'tedir; "durum geçişleri" satırı `02 §3.34.8`'dedir (L1-7, L2a-8, L2b-8) | Atıflar hedefe | düşük |
| C-12 | 3 | Konvansiyon 5, 7.6.6, §11.1 | "Gecelik yedeğin saati tek istisnadır" — değeri başka evde duran tasarım değerleri de var; 7.6.6'nın "birkaç gün"ü değersizdi (L1-8) | İstisnalar sayıldı; yedi gün (K-985 (4)), **11.1.31** | düşük |
| C-13 | 3 | 2.2.3, 2.2.4, 2.2.6 ↔ 6.6, 7.4.5, 7.6.6, 7.6.8, 9.2.2 | Ölçüt hücreleri kalite döngüsünün araçlara yüklediği yetenekleri taşımıyordu (L1-9) | Üç hücre | düşük |
| C-14 | 3 | 1.2.24, 3.2.6, 6.x.4, 8.2.4, 8.7.2 | "Göç hizmeti yalnız güncellemede" — ilk kurulumda şemayı da göç demeti kurar; yazılı değildi (L1-10) | "İlk kurulumda ve güncellemede"; ilk kurulumun cümlesi | düşük |
| C-15 | 3 | §0'ın girişi ↔ 0.4, 0.5, 0.13 | "İlk dört satır çok kritik soruların cevaplarıdır" — dört soru üç satırdadır; 0.4 ve 0.5 aşamanın kararlarıdır (L1-11) | Satırların sınıfı | düşük |
| C-16 | 3 | 1.2.29 ↔ 6.1.4, 6.1.5 | "Google yalnız adı ve e-postayı verir" — yeniden doğrulama girişin anına dayanır (L1-12) | Verilen ile saklanan ayrıldı | düşük |
| C-17 | 3 | 3.6.5 ↔ 11.3.10 | Yol haritasının dış servis listesi YH-1 ve YH-11'i düşürüyordu (L1-15) | Beş aday | düşük |
| C-18 | 3 | 5.2.13 ↔ `03 §4.1`, `02 §4.2` | İş gerektirmeyen sürelerin listesi Z-39, Z-42, Z-16, Z-17, Z-21, Z-27'yi saymıyordu (L2a-3) | Eklendi | düşük |
| C-19 | 3 | 6.5.1 ↔ MK-52 | "İstisna yalnız imzalı makine uçlarıdır" — aynı madde ödeme dönüşünü ikinci istisna sayar (L2a-6) | "Yalnız" kalktı | düşük |
| C-20 | 3 | 6.3.1 ↔ 6.3.4, MK-48 | "Satır sayımdan önce kesinleşir" bütün limitlere genelleniyordu; mekanizma yalnız başarısızlıkta sayılanlarda yazılıydı (L2a-7) | Her istekte sayılan limitlerde de satır kendi işleminde kesinleşir (K-972'nin ilkesi) | düşük |
| C-21 | 7 | 4.3.4, 5.2.9 ↔ 8.5.9 | Komutla kapatılan iş satırının akıbeti yazılı değildi (L2a-9) | Tamamlanmış sayılır, gece silinir (K-985 (2)) | düşük |
| C-22 | 3 | 5.3.2, 5.3.4, 5.3.5, 7.6.1 ↔ 8.8.1 | Gecenin sırası dosya yedeğini 03:00'ten önceye koyuyordu, kısıt yalnız tam yedeği bağlıyordu (L2a-10) | Dosya yedeği de 03:00'ten önce biter (K-985 (3)) | düşük |
| C-23 | 3 | 8.6.3 ↔ 8.7.4, 10.1.14, 11.1.4, 11.3.2, 8.2.5 | Güncelleme turunun adımları başka bölümlerin tura yüklediği dört işi taşımıyordu — dönüş noktası, saat eşitlemesi, kapasite okuması, kota (L2b-2) | Adımlara girdi | **orta** |
| C-24 | 3 | 7.4.5, 8.7.4, 8.5.11 ↔ 5.2.5, 5.2.7 | Süreye bağlı silmelerin yeniden uygulanması imha işine bağlanıyordu; IBAN'ın ve bekleyen kaydın silinmesi zamanlayıcının işidir (L2b-5) | İki yürütücü | düşük |
| C-25 | 3 | 8.7.4 ↔ §8.5 | "Yönetim komutuyla düzeltilir" — listede veri düzelten komut yok (L2b-6) | Düzeltmeyi taşıyan sürümle gelen komut | düşük |
| C-26 | 3 | 8.6.7 ↔ 8.8.5 | Ana sürüm geçişinin özeti iki ek adımı (yedek deposunun yükseltilmesi ve ilk tam yedek, iki kat disk) taşımıyordu (L2b-9) | Eklendi | düşük |
| C-27 | 3 | 11.3.1, §12 MK-78 ↔ 11.3.6, 11.3.7 | Büyüme yolu katı bir sıra olarak yazılıydı; son iki adım sıraya oturmuyordu (L2b-10) | 11.3.6 ve 11.3.7 kendi tetikleyicileriyle | düşük |
| C-28 | 3 | 11.1.7, 8.4 ↔ 11.1.11, 5.3.5 | Kurulumda değişenler "yalnız §8.4.5'in saydıklarıdır" — gecelik yedeğin saati ve dış kontrolün aralığı kurulumda kurulur (L2b-11) | Eklendi | düşük |
| C-29 | 3 | §11.1, 5.2.9 ↔ 6.7.8 | Sunucudaki misafir sepetinin silinme eşiği (400 gün) tasarım değerleri tablosunda yoktu (L2b-12) | **11.1.32**; 5.2.9 sayıyla | düşük |
| C-30 | 3 | §8'in girişi ↔ §8.4, §8.5, §8.8, §8.9 | Bölümün kapsamı kapalı ve eksik sayılıyordu (L2b-13) | Hizalandı | düşük |
| C-31 | 3 | 8.9.5, 6.9.4; `DEPLOY_RUNBOOK` inceleme araçları satırı ↔ 8.7.4, 8.8.5 | "Veri yazan işler yalnız göç, komutlar ve kurtarmadır" — eski bir ana dönüş ve ana sürüm geçişi yoktu (L2b-14, L6-8) | Üç yüz | düşük |
| C-32 | 3 | 8.1.6 ↔ 8.1.2, 2.2.8 | "Ortamlar arasında yalnız ayarlar ve sırlar değişir" — geliştirme varyantı bir bileşen ekler (L2b-15) | Referans ve firma kurulumlarına daraldı | düşük |
| C-33 | 3 | 10.2.1, §12 MK-62 ↔ 10.2.3 | "İki uyarı da gönderim kaydından okunur" — kanal uyarısı kanal hâli kaydından okunur (L2b-17) | Ayrıldı | düşük |
| C-34 | 3 | 3.2.4, 7.6.1, 8.2.4, 9.2.3 ↔ 9.1.4, 9.4.10 | Yedek aracı kendi kayıtlarını "salt okuma bağlar" — kendi kaydını ve durum dosyasını yazar (L2b-20) | Yazdığı bölümler | düşük |
| C-35 | 3 | 8.1.4 ↔ 8.6.2, 8.8.3 | "En az bir gün çalıştıktan sonra" — acil güvenlik yaması beklemeden yayılır (L3-10) | İstisna eklendi | düşük |
| C-36 | 3 | 8.1.7 ↔ 8.7.2 | Göç tek işlem gibi okunuyordu — K-965'ten sonra her göç kendi işlemindedir, öncekiler kalır (L3-12) | Hizalandı | düşük |
| C-37 | 3 | 6.1.1 ↔ 5.2.9, 7.4.4 | Oturum kaydı "hesapla yaşar ve hesapla silinir" — süresi dolan oturum gece silinir (K-979) (L6-4) | Açık oturum Z-31; süresi dolan gece | **orta** |
| C-38 | 3 | 4.6.2, 10.1.3; `08` ↔ aynı maddeler | "Geç sonuç yalnız bildirimle ulaşır" — işaret anında yanıtı beklenen sorgu da getirir (K-970 (4)) (L6-1) | Yan cümle | düşük |
| C-39 | 3 | 10.3.5; `07`, `08` ↔ 4.6.2 | Kaydedilemeyen bildirimin sonuçları K-978'in kolunu saymıyordu (L6-9) | Üçüncü kol; K-978'in etki sütununa `07` | düşük |
| C-40 | 3 | `08` ↔ 4.3.4 | "Varsayılan beş deneme kuralına düşmez" — kod hatasında düşer (L6-10) | Hizalandı | düşük |
| C-41 | 3 | 5.4.4, 10.2.5 ↔ 9.4.14, 7.6.2 | "Kurulumu yapana gider" koşulsuzdu — K-974 (3) açık koşulun yeni uyarıyı örttüğünü yazdı (L5-8) | 9.4.14'ün sınırı | düşük |
| C-42 | 3 | 10.1.12 ↔ 9.4.1, 9.4.14 | Dış kontrolün kalan riski yalnız kesintiyi sayıyordu — bütün durum koşulları susar, aktarımın durması dahil (L5-5) | Hizalandı | düşük |
| C-43 | 3 | 11.3.4, 11.3.5 ↔ 9.4.10 | Büyüme adımları durum ucunun sunucuya bağlı kaynaklarını saymıyordu (L5-6) | Eklendi | düşük |
| C-44 | 3 | 6.4.7, 7.7.4 ↔ `04` 3.5.2, `02 §10.4.11` | "İndirme bağlantısı" iki ayrı şeyin adıydı (L4b-7) | `05`'in cümlesi ayrıldı | düşük |
| C-45 | 3 | 4.5.5 ↔ `02 §12.2.7` | Siparişe bağlanmayan bildirim `02 §12.2.7`'ye dayandırılıyordu; o madde "dört kayıt" sayar (L4a-3) | Atıf: bu dokümanın tasarım değeri, dört kaydın dışında | düşük |

### 3.2 Güvence cümlesi ile mekanizmanın farkı (K-976'nın devri)

Güvence merceğinin iki koşumunun (`AUDIT_REPORTS/05_GUARANTEE_LENS.md`) koşmadığı bölgeler ve kalite döngüsünün son düzeltmelerinin yan yüzleri.

| # | Kontrol | Yer | Bulgu | Kapanış | Seviye |
|---|---|---|---|---|---|
| C-46 | 7 | 7.3.1, 7.3.3; `06`, `07`, `12` ↔ `02 §3.6.7`, §3.17.3 | Ödemede stoğun kesin düşmesi ürünün sürümünü artırmazsa önceden açılmış form satılmış adedi geri yazar; `02 §3.6.7`'nin mekanizması yoktu (L3-1) | **K-980 (1)** — stok alanı kendi koşullu güncellemesiyle | **orta** |
| C-47 | 3 | MK-11, 8.2.5, 6.x.8; D-01 | "Repo açıksa imajlar da açıktır" — paketin görünürlüğü repodan geçmez, açık paket yeniden özel yapılamaz (GitHub belgesi) (L3-4) | **K-981 (1)** | **orta** |
| C-48 | 3 | 7.1.3, 6.x.6, 6.x.8, 3.2.5 | "Hiçbir bileşen kurucu kullanıcıyla çalışmaz" — pgBackRest yerel soketle o yetkiyle bağlanır (L2b-7, L3-3) | **K-987 (3)** — ağ bağlantısına daraldı | **orta** |
| C-49 | 3 | 9.1.6, 8.4.5 ↔ 7.4.5, 8.5.7 | Kayıt düzeyi yükseltilirse anlık silme satırı yazılmaz ve 7.4.5'in güvencesi (K-955) düşer (L2b-19) | **K-986 (5)** | **orta** |
| C-50 | 3 | 9.1.5, 6.4.8; `DEPLOY_RUNBOOK` | Erişim kaydının kapalı alan listesi ve maskelemesi yalnız uygulamanın koduna bağlıydı; erişim kaydını Caddy yazar ve varsayılanı başlıkları yazar (L5-1) | **K-986 (1)** | **orta** |
| C-51 | 3 | 2.3.5, 8.3.9, 8.7.3, MK-70 | Geri alma yolu canlılık ucuyla sınanıyordu — genişlet–daralt kuralını çiğneyen göç geçer (L3-5) | **K-981 (2)** | düşük |
| C-52 | 3 | 7.10.1, 4.1.5, 8.4.2; `09` | Önbelleğin bayatlamasına karşı mekanizma yoktu; onayın kapıyı önbellekten okuyup okumadığı yazılı değildi (L3-6) | **K-980 (5)** | düşük |
| C-53 | 3 | 7.7.1 | "Kırık bağlantı doğmaz" — süpürme kaydı henüz kesinleşmemiş dosyayı silebilir (L3-7) | **K-980 (3)** | düşük |
| C-54 | 3 | 7.7.4; `06`, `12` | Son hakta iki eşzamanlı basış iki indirme verirdi (L3-8) | **K-980 (2)** | düşük |
| C-55 | 3 | 7.7.1, 10.1.8 ↔ 9.4.10 | Yükleme ancak disk dolunca reddediliyordu; veritabanı aynı disktedir (L3-9) | **K-980 (4)** | düşük |
| C-56 | 3 | 3.2.5; `DEPLOY_RUNBOOK` | Çıkış kuralı ana makinenin güvenlik duvarına yazılırsa konteyner trafiğine uygulanmaz (Docker belgesi) (L3-2) | **K-987 (1)** | düşük |
| C-57 | 3 | 8.9.4; `DEPLOY_RUNBOOK` | Firmanın anahtarı kaldırmasının yolu yazılı değildi (L3-14) | **K-987 (2)** | düşük |
| C-58 | 7 | 3.5.2, 6.9.2; `02 §10.2.5`; `06` | E-posta tekilliğinin harf karşılaştırması hiçbir dokümanda yoktu (L3-11) | **K-982 ⚠** | düşük |
| C-59 | 3 | 7.4.2 | Dosya silen imha parçasında satır ile silmenin sırası yazılı değildi (L2b-18) | **K-985 (1)** | düşük |
| C-60 | 7 | 9.4.10, 11.2.9, 11.3.5 | Beklenmeyen hata sayısının nerede tutulduğu yazılı değildi (L2b-16) | **K-986 (6)** | düşük |
| C-61 | 3 | 9.3.1, 9.3.4, 8.5 | Komutlar uygulamanın kayıt dosyasına ikinci süreç olarak yazabilirdi; okuma komutunun satırının sırası yazılı değildi (L5-2) | **K-986 (3)** | düşük |
| C-62 | 3 | 9.3.1, 9.3.3, 6.8.4; `DEPLOY_RUNBOOK` | "Sunucuda dosya bırakılmaz" yalnız elle silmeye dayanıyordu; tek yazılabilir bölüm yedeğe giren kayıt dizinidir (L5-3) | **K-986 (4)** | düşük |
| C-63 | 3 | 10.1.16; `DEPLOY_RUNBOOK` | "Kesinti ölçülür" yalnız nabız adı çözümleyiciyle çözüyorsa tutar; park satırı seçimi açık bırakıyordu (L5-4) | **K-986 (7)** | düşük |
| C-64 | 3 | 9.2.2; `DEPLOY_RUNBOOK` | "Günlük dosyalar … yalnız imha işi siler" boyla dönen Caddy'ye uymuyordu (L5-7) | **K-986 (2)** | düşük |
| C-65 | 3 | 5.2.9 | Gündüz silinen oturum kayıtları ne imha kaydına yazılanlarda ne istisnada sınıflanmıştı; ikinci istisnanın ifadesi belirsizdi (L5-9, L6-5) | **K-984 (3) ⚠** | düşük |
| C-66 | 7 | 5.2.7, 7.4.2; `02` Z-44, §12.2.7; `03` 4.2.9; `10` KP-74; `06` | Bekleyen e-posta değişikliğinin ömrüyle silinmesi — kişisel veri — iki kapalı listenin de dışındaydı (L6-2) | **K-984 (1) ⚠** | **orta** |
| C-67 | 3 | 5.2.9, 7.4.4; `06` ↔ 6.7.8, `02 §12.2.5` | K-979 misafir sepetini kişisel veri taşımayan sayıyordu; K-950 aynı sepetin çerezine KVKK m.5/2-c dayanağı verdi (L6-3) | **K-984 (2) ⚠** | **orta** |
| C-68 | 3 | 5.2.9 ↔ `02 §4.2` Z-44 | Gecelik temizliğin satırları Z-44'ün "periyodik imha çalışması" terimiyle bağlı değildi (L6-6) | `05`'ten bağlandı; `02` değişmez | düşük |

### 3.3 Yansıma kaçakları — geri beslenen kuralların öteki yüzleri (K-652, betik 1)

| # | Kontrol | Yer | Bulgu | Kapanış | Seviye |
|---|---|---|---|---|---|
| C-69 | 6 | `02 §3.34.8` ↔ `05` 7.6.2, 7.6.9, 10.1.14 | "Aktarım durmuşken" kapalı tanımı hata vermeden geride kalan aktarımı (K-966) saymıyordu (L4a-1) | Tanım `05 §7.6.9`'a bağlandı | düşük |
| C-70 | 6 | `02 §3.34.8`, `03` 10.2.4, `10` KP-75, `05` 8.7.4; `DEPLOY_RUNBOOK` | Sıfır kayıp listesi "yasal kayıt" ölçütüyle kurulur; K-951'den sonra yasal kayıt olan gönderim kaydı listede yoktu ve dönüş denetimi listeye bağlıydı (L4a-2) | **K-983 ⚠** | **orta** |
| C-71 | 6 | `04` 2.7.3.3, 5.21.12, 9.25.11 | K-904'ün doğrulama düğmesi yükleniyor ve hata satırına, sonucu belirsiz işlemin kalıbına ve yöneticinin ekranına yansımamıştı (L4b-1) | Hizalandı | düşük |
| C-72 | 3 | `04 §1.1.2` E-28 | Aday listesinin E-28 satırı K-839 öncesi hâldeydi (L4b-2) | Hizalandı | düşük |
| C-73 | 6 | `04 §1.1` (`03 §2.8.4.3`, §8.3.3.5) | İki matris özeti K-940'ın koşulunu taşımıyordu; biri yolun koşulsuz açıldığını okutuyordu (L4b-3) | Hizalandı | düşük |
| C-74 | 6 | `02 §3.21.11` ↔ `03` 5.2.1.3, `02 §12.5` | Ters ibraz kuralının evi geri ödeme adımlarını üç sayıyordu — iki yüzü dördüncüyü ("gerçekleşmedi" işareti) sayar (L4b-4) | Dört adım; Kaynak'a K-940 | düşük |
| C-75 | 6 | `04` 5.24.7 ↔ `05` 6.1.5 | Google ile yeniden doğrulamanın tutmadığı hâller doğrulama anı hiç gelmeyen dönüşü saymıyordu (L4b-5) | Hizalandı | düşük |
| C-76 | 3 | `02 §1.1` ↔ K-977 | Ürüne görünen kavramın sayımı kararın "ekran öğesi"ni düşürmüştü (L4b-6) | **K-988 (2)** | düşük |
| C-77 | 3 | `02 §1.1`; `06`, `07`, `09` K-17 satırları ↔ Z-43, Z-44, §3.31.2 | Teknik kavram tanımı ("ürünün kuralında geçmeyen") kendi listesindeki üç kaydı dışarıda bırakıyordu (L6-13) | **K-988 (1)** | düşük |
| C-78 | 3 | `05` 4.6.3 ↔ `04` 9.9.8 | Geri ödeme kaydının dört hâlinden ikisi `04` ile `05`'te farklı adlıydı (L6-12) | **K-988 (3)** — eşleme | **orta** |

### 3.4 Kaynak satırları, geri işaret ve D-01

| # | Kontrol | Yer | Bulgu | Kapanış | Seviye |
|---|---|---|---|---|---|
| C-79 | 7 | `04 §4`'ün Kaynak satırı | E-21 ve E-54 satırlarının andığı K-904 Kaynak'ta yoktu (betik 2) | Eklendi | düşük |
| C-80 | — | Karar kaydı K-708 | K-979 K-708'i ilişki sözcüğü olmadan anıyordu; geri işaret yoktu (L6-14) | K-708'e K-984'ün geri işareti | düşük |
| C-81 | 7 | `DEFERRED_BACKLOG.md` D-01, `05 §2.3` ↔ 7.6.10, 8.2.2, 8.5.11 | "D-01'de doğrulanır" diyen üç olgunun D-01'de evi yoktu; çakışma taramasının §6.4'ü "`05 §2.3`'ün tablosundadır" diyordu (L6-11) | **2.3.8**; D-01'in açıklamasına (c) ve kanıtına K-961, K-962, K-981, K-987 | **orta** |

### 3.5 Aşama geçişi

| # | Kontrol | Yer | Bulgu | Kapanış | Seviye |
|---|---|---|---|---|---|
| C-82 | 1, 5 | `06`'nın başlık satırı ↔ `00 §C.1` | Veri Modeli şablonunun "Bağımlılıklar" alanı `09`'u sayar; `00 §C.1`'in girdisi `02`, `03`, `05`, `10`'dur ve `09` sonra yazılır — öteki şablonların alanları `00 §C.1` ile tutar | Bu repoda değişmedi — Veri modeli aşamasının açılışının şablon taramasına (checklist §1); PF-74 | düşük |

**Sayım:** seksen iki bulgu (C-1…C-82) — **orta 15** (C-1, C-2, C-4, C-23, C-37, C-46, C-47, C-48, C-49, C-50, C-66, C-67, C-70, C-78, C-81), **düşük 67**. İki yerin metin farkı 45 (§3.1) · güvence ↔ mekanizma 23 (§3.2) · yansıma kaçağı 10 (§3.3) · Kaynak, geri işaret ve D-01 3 (§3.4) · aşama geçişi 1 (§3.5). Hepsi kapandı.

**Park satırlarındaki yüzler:** `07` (C-2, C-3, C-39; K-980 (1)), `08` (C-38, C-39, C-40), `06` (C-46, C-54, C-66, C-67, C-77; K-982, K-985 (2)), `09` (C-52, C-77; K-981 (2), K-986), `12` (C-54; K-980 (1)), `DEPLOY_RUNBOOK` (C-31, C-50, C-56, C-57, C-62, C-63, C-64; K-981 (1), K-983, K-985, K-986 (8)). Park satırı ↔ etki sütunu iki yönde eşleşir: yeni kararların etiketleri karar kaydındaki etki sütunlarıyla birlikte yazıldı; dizin satırlarına "K-980…K-988 tarama raporunda yoktur" cümlesi girdi.

**Yansıma kaçaklarının yerleri:** on yedi yer — Ürün Gereksinimleri altı (§1.1, §3.21.11, §3.34.8, §4.2 Z-44, §10.2.5, §12.2.7), Kullanıcı Akışları iki (4.2.9, 10.2.4), MVP Kapsamı iki (KP-74, KP-75), Arayüz Tanımları yedi (2.7.3.3, 5.21.12, 5.24.7, 9.25.11, §1.1.2, §1.1'in iki özeti). **En önemli üçü:** (1) sıfır kayıp listesi kendi ölçütünün yeni bir kaydını saymıyordu ve Teknik Mimari'nin dönüş denetimi bu listeye bağlıydı (C-70); (2) iş kuyruğunun işleri altı özet yüzde ve envanterde eksikti — kart iadesinin sorgusu ve gelen bildirimin işlenmesi (C-1); (3) imzalı uç ilkesinin daralması MK-04'e, §12'ye ve API Tasarımı'nın park satırına taşınmamıştı (C-2).

---

## 4. Etki işaretleri

Karar kaydında şu Aşama 4 kararlarının etki sütununa `(taslak güncellendi — vX.Y; checkpoint)` parçası eklendi: K-857, K-862, K-868, K-891, K-904, K-906, K-913, K-920, K-922, K-927, K-929, K-932, K-933, K-937, K-939, K-940, K-941, K-954, K-955, K-962, K-963, K-965, K-966, K-968, K-970, K-972, K-973, K-974, K-978, K-979 (otuz). Yeni kararların geri işareti: K-873, K-883, K-884, K-887, K-948 (K-980) · K-920, K-965 (K-981) · K-899, K-972 (K-982) · K-951, K-956, K-557 (K-983) · K-708, K-734, K-950, K-979 (K-984) · K-897, K-946, K-964, K-975 (K-985) · K-898, K-910, K-915, K-917, K-918, K-923, K-955 (K-986) · K-910, K-927, K-937 (K-987) · K-977 (K-988). Aşama 1–3'ün salt okunur kayıtlarına yalnız geri işaret girdi (K-557, K-708, K-734). Yeni satırlar: **K-980…K-988**.

---

## 5. Proje sahibinin gözden geçirme listesi

### 5.1 ⚠ işaretli kırk dört karar

Kaynak: [çakışma taraması §4.1](PHASE4_CONFLICT_SCAN.md) — kırk bir karar, hazır metniyle ve konulara göre; bu checkpoint üç karar ekledi. Kayıtla birebir (44 / 44; betikle sayıldı, §Notlar). Bu kararlar proje sahibinin Teknik mimari aşamasının açılışındaki talimatıyla (K-855) sorulmadan kaydedildi ve `(öneriyle kaydedildi — ⚠)` işaretini taşır. **Kapı:** liste arşiv işaretinden (K-437'nin 6. adımı) önce proje sahibine tek mesajda, karar başına bir sade cümleyle ve gerekçesiz gösterilir (`INSTRUCTIONS.md` §2, "Ortak kural"); yöneticiden proje sahibine gider (§9). İtiraz gelmeyen satırın işareti `(öneriyle kaydedildi — ⚠ — gözden geçirildi YYYY-AA-GG, itiraz yok)` olur; itiraz gelen karar yeni bir karar satırıyla değişir.

**Hazır metnin eki — bu checkpoint'in üç kararı** (çakışma taramasının §4.1'indeki listenin konularına eklenir):

*Hesap, giriş ve çerezler*
- K-982 — E-posta adresi hesap açarken, girişte ve misafir siparişlerinin hesaba bağlanmasında büyük-küçük harf farkı gözetilmeden karşılaştırılır; ekranda ve e-postada yazıldığı gibi görünür.

*Sunucu, yedek ve veri kaybı*
- K-983 — Siparişlerle ilgili gönderilen e-postaların kaydı da kaybolmaması gereken kayıtlar arasına girer; güncellemede veritabanı eski bir ana döndürülmeden önce bu kayıt da denetlenir.

*Silme ve kayıtlar*
- K-984 — Doğrulanmadan süresi dolan yeni e-posta adresinin silinmesi ve sunucudaki misafir sepetinin silinmesi de imha kaydına yazılır; çıkışta ya da şifre değişikliğinde kapanan oturumların silinmesi yazılmaz.

**Sayım:** çakışma taramasının listesi 41 · checkpoint 3 — **toplam 44**. Betikle sayıldı; kırk dördünün de konu hücresi ⚠ işaretini taşıyor.

**Gözden geçirme sonucu — 2026-10-17 (K-437'nin 6. adımı, arşiv işaretinden önce; K-990).** Liste yönetici aracılığıyla tek mesajda, karar başına bir sade cümleyle gösterildi; öğrenim terfisinin üç önerisi aynı mesajda ayrı numaralı sorulardı (K-991). **İtiraz yok:** hiçbir karar değişmedi, yeni karar satırı açılmadı ve bu rapora retro bölümü gerekmedi. Karar kaydında kırk dört satırın konu hücresindeki işaret silinmedi, `(öneriyle kaydedildi — ⚠ — gözden geçirildi 2026-10-17, itiraz yok)` biçimine dönüştürüldü (Aşama 3'ün kalıbı, CP03 §5.1). Öğrenim terfisi (K-989) yeni ⚠ karar doğurmadı. **Mekanik doğrulama:** bu listenin 44 tekil K numarası ile dönüştürülen 44 satır birebir — eksik 0, fazla 0; konu hücresinde eski işareti taşıyan Aşama 4 karar satırı kalmadı. Kayıt: karar kaydı §12.1 (v0.116).

---

## 6. Öğrenim terfisine notlar — 5. adımın girdisi

Adayların metinleri tracker §12.1'dedir; bu checkpoint iki aday ekledi ve bir adaya kanıt verdi.

| # | Aday | Kaynak | Playbook |
|---|---|---|---|
| 11 | Kalite döngüsünün düzeltmesi MK'nin özet satırının gerekçe hücresine yansımıyor — Kaynak betiği alt bölümün Kaynak satırına bakar, özet tablonun gerekçe hücresine bakmaz | C-6 | — |
| 12 | Bir kayda yeni bir sınıf veren karar (yasal kayıt, kişisel veri) o sınıfı ölçüt alan öteki kapalı listeleri güncellemiyor | C-70, C-45, C-66 | — |

**Kanıt — mevcut adaylara:** aday 7'nin ("güvence cümleleri yazımda mekanizmaya karşı okunur") sınıfı güvence merceğinin taramadığı bölgelerde yirmi üç bulguyla yeniden çıktı — 267 güvence cümlesinin 37'si kısmen tuttu, biri tutmadı; merceğin dokümanın tamamında bir kez koşması gerektiği gözlemi güçlendi. Aday 8'in ("park satırı kararın bütün kollarını taşımıyor") sınıfı yedi park yüzünde (§3'ün park satırları notu) yeniden görüldü — bu kez kararın değil, `05`'teki düzeltmenin park satırına taşınmaması (C-2, C-31).

**Tek seferlik gözlem (raporda kalır):** düzeltmeleri bölgelere bölen ve her birini birebir değiştirme şartnamesi olarak yazan alt ajanlar paralel koştu, yönetici değiştirmeleri tek betikle uyguladı — 268 değiştirmenin hiçbiri çakışmadı; bölgeler arası yüzlerin dördü yönetici tarafından ayrıca hizalandı (Notlar). `06` şablonunun bağımlılık alanı PF-74 olarak playbook listesine girdi.

---

*Kaynak: K-437 (kapanış sırası, 4. adım) · K-429 (çakışma taraması — girdi) · K-976 (cross-review'ın çıkış kuralı ve checkpoint'e devri) · K-652 (geri beslemenin işletimi ve iki betik — ✓ ve döngüler yeniden açılmaz) · K-29 (etki işaretleri) · K-647 (kaydın evi; Aşama 1–3 kayıtları salt okunur) · K-855 (⚠ öneriyle kayıt ve liste kapısı) · K-859 (§8'in dal stratejisi) · K-980…K-988 (bu checkpoint'in kararları) · `.claude/skills/checkpoint/SKILL.md` (yedi kontrol, çıktı biçimi ve koşum) · `.claude/skills/audit/SKILL.md` ("Koşum biçimi") · `Docs/CHECKPOINT_REPORTS/CP03_PHASE3_CHECKPOINT.md` (biçim örneği) · `Docs/AUDIT_REPORTS/05_GUARANTEE_LENS.md` (güvence merceğinin kalıbı ve kapsamı) · `Docs/CHECKPOINT_REPORTS/README.md` (rapor türü ve retro güncelleme).*
